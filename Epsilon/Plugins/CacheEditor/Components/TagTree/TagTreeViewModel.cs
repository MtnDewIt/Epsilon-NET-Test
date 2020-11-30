using CacheEditor.Components.TagTree.Commands;
using CacheEditor.TagEditing;
using EpsilonLib.Commands;
using EpsilonLib.Menus;
using EpsilonLib.Settings;
using EpsilonLib.Shell;
using EpsilonLib.Shell.Commands;
using EpsilonLib.Shell.TreeModels;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using TagTool.Cache;


namespace CacheEditor.Components.TagTree
{
    enum TagTreeViewMode
    {
        Folders,
        Groups
    }

    enum TagTreeGroupDisplayMode
    {
        TagGroup,
        TagGroupName
    }

    class TagTreeViewModel : TreeModel, ITagTree,
        ICommandHandler<ToggleFoldersViewCommand>,
        ICommandHandler<ToggleGroupsViewCommand>,
        ICommandHandler<CopyCommand>,
        ICommandHandler<ToggleGroupNameViewCommand>,
        ICommandHandler<ToggleGroupTagNameViewCommand>,
        ICommandHandler<ExtractBitmapCommand>
    {
        private ICacheFile _cacheFile;
        private string _filterText;
        private TagTreeViewMode _viewMode = TagTreeViewMode.Groups;
        private TagTreeGroupDisplayMode _groupDisplayMode = TagTreeGroupDisplayMode.TagGroupName;
        private TagExtract _bitmapExtract;

        public MenuItemDefinition ContextMenu { get; set; } = MenuDefinitions.ContextMenu;

        public TagTreeViewModel(ICacheEditingService cacheEditingService, ICacheFile cacheFile, TagExtract bitmapExtract = null)
        {
            _viewMode = cacheEditingService.Settings.Get(
                Settings.TagTreeViewModeSetting.Key, 
                (TagTreeViewMode)Settings.TagTreeViewModeSetting.DefaultValue);

            _groupDisplayMode = cacheEditingService.Settings.Get(
                Settings.TagTreeGroupDisplaySetting.Key,
                (TagTreeGroupDisplayMode)Settings.TagTreeGroupDisplaySetting.DefaultValue);

            _cacheFile = cacheFile;
            _bitmapExtract = bitmapExtract;
            Refresh();
        }

        public TagTreeViewMode ViewMode
        {
            get => _viewMode;
            set
            {
                if (SetAndNotify(ref _viewMode, value))
                    Refresh();
            }
        }

        public TagTreeGroupDisplayMode GroupDisplayMode
        {
            get => _groupDisplayMode;
            set
            {
                if (SetAndNotify(ref _groupDisplayMode, value))
                    Refresh();
            }
        }

        public string FilterText
        {
            get => _filterText;
            set
            {
                if (SetAndNotify(ref _filterText, value))
                    Refresh();
            }
        }

        public new void Refresh()
        {
            SelectedNode = null;
            var view = CreateView(_viewMode).BuildTree(_cacheFile.Cache, FilterTag);
            Nodes = new ObservableCollection<ITreeNode>(view);
        }

        private bool FilterTag(CachedTag tag)
        {
            if (string.IsNullOrEmpty(FilterText))
                return true;

            string groupTagName = tag.Group.Tag.ToString();
            string groupName = tag.Group.ToString();
            // check for filter match with group name/group tag name
            if (groupName.Contains(FilterText) || groupTagName.Contains(FilterText))
                return true;

            return tag.ToString().Contains(FilterText);
        }

        private ITagTreeViewMode CreateView(TagTreeViewMode mode)
        {
            switch (mode)
            {
                case TagTreeViewMode.Folders:
                    return new TagTreeFolderView();
                case TagTreeViewMode.Groups:
                    return new TagTreeGroupView(_groupDisplayMode);
                default:
                    return null;
            }
        }

        public ITreeNode FindTag(CachedTag tag)
        {
            return FindNodesWithTag(tag).FirstOrDefault();
        }

        public void EnsureVisible(ITreeNode node)
        {
            var tagTreNode = node as TagTreeNode;
            while(tagTreNode != null)
            {
                tagTreNode.IsExpanded = true;
                tagTreNode = tagTreNode.Parent;
            }
        }

        #region Command Handlers

        void ICommandHandler<ToggleFoldersViewCommand>.ExecuteCommand(Command command)
        {
            ViewMode = TagTreeViewMode.Folders;
        }

        void ICommandHandler<ToggleFoldersViewCommand>.UpdateCommand(Command command)
        {
            command.IsChecked = ViewMode == TagTreeViewMode.Folders;
        }

        void ICommandHandler<ToggleGroupsViewCommand>.ExecuteCommand(Command command)
        {
            ViewMode = TagTreeViewMode.Groups;
        }

        void ICommandHandler<ToggleGroupsViewCommand>.UpdateCommand(Command command)
        {
            command.IsChecked = ViewMode == TagTreeViewMode.Groups;
        }

        void ICommandHandler<CopyCommand>.ExecuteCommand(Command command)
        {
            if (SelectedNode is TagTreeNode node)
            {
                if(node.Tag is CachedTag tag)
                    ClipboardEx.SetTextSafe($"{tag}");
                else
                    ClipboardEx.SetTextSafe(node.Text);
            }    
        }

        void ICommandHandler<CopyCommand>.UpdateCommand(Command command)
        {
            command.IsEnabled = SelectedNode != null;
        }

        void ICommandHandler<ToggleGroupNameViewCommand>.ExecuteCommand(Command command)
        {
            GroupDisplayMode = TagTreeGroupDisplayMode.TagGroupName;
        }

        void ICommandHandler<ToggleGroupNameViewCommand>.UpdateCommand(Command command)
        {
            command.IsChecked = GroupDisplayMode == TagTreeGroupDisplayMode.TagGroupName;
        }

        void ICommandHandler<ToggleGroupTagNameViewCommand>.ExecuteCommand(Command command)
        {
            GroupDisplayMode = TagTreeGroupDisplayMode.TagGroup;
        }

        void ICommandHandler<ToggleGroupTagNameViewCommand>.UpdateCommand(Command command)
        {
            command.IsChecked = GroupDisplayMode == TagTreeGroupDisplayMode.TagGroup;
        }

        void ICommandHandler<ExtractBitmapCommand>.ExecuteCommand(Command command)
        {
            if (SelectedNode?.Tag is CachedTag tag)
                _bitmapExtract.ExtractBitmap(_cacheFile.Cache, tag);
        }

        void ICommandHandler<ExtractBitmapCommand>.UpdateCommand(Command command)
        {
            command.IsVisible = SelectedNode?.Tag is CachedTag tag && tag.IsInGroup("bitm");
        }

        #endregion

    }
}
