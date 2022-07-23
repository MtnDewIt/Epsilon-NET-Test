﻿using CacheEditor.Components.TagTree.Commands;
using CacheEditor.TagEditing;
using EpsilonLib.Commands;
using EpsilonLib.Menus;
using EpsilonLib.Settings;
using EpsilonLib.Shell;
using EpsilonLib.Shell.Commands;
using EpsilonLib.Shell.TreeModels;
using System;
using System.Collections.ObjectModel;
using System.Globalization;
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
        ICommandHandler<CopyTagNameCommand>,
        ICommandHandler<CopyTagIndexCommand>,
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
            _cacheFile.TagSerialized += _cacheFile_TagSaved;
            Refresh();
        }

        private void _cacheFile_TagSaved(object sender, CachedTag e)
        {

            if(FindTag(e) is TagTreeNode node && node != null)
            {
                node.Tag = e;
                node.UpdateAppearance();
            }
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

            var filterText = FilterText.Trim();

            // check for filter match with group group tag name
            string groupTagName = tag.Group.Tag.ToString();
            if (groupTagName.IndexOf(filterText, StringComparison.OrdinalIgnoreCase) >= 0)
                return true;

            // check for filter match with tag name or group name
            if (tag.ToString().IndexOf(filterText, StringComparison.OrdinalIgnoreCase) >= 0)
                return true;

            if (filterText.StartsWith("0x"))
            {
                if (int.TryParse(filterText.Remove(0, 2), NumberStyles.HexNumber, null, out int value))
                {
                    if (tag.Index == value || (tag.ID != 0 && tag.ID == value))
                        return true;
                }
            }
    
            return false;
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

        public void UpdateNodeAppearance(ITagTree node)
        {
            ((TagTreeNode)node).UpdateAppearance();
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

        void ICommandHandler<CopyTagNameCommand>.UpdateCommand(Command command)
        {
            command.IsVisible = SelectedNode != null && SelectedNode?.Tag is CachedTag;
        }

        void ICommandHandler<CopyTagNameCommand>.ExecuteCommand(Command command)
        {
            if (SelectedNode is TagTreeNode node && node.Tag is CachedTag tag)
                    ClipboardEx.SetTextSafe($"{tag}");
        }

        void ICommandHandler<CopyTagIndexCommand>.ExecuteCommand(Command command)
        {
            if (SelectedNode is TagTreeNode node && node.Tag is CachedTag tag)
                ClipboardEx.SetTextSafe($"0x{tag.Index:X08}");
        }

        void ICommandHandler<CopyTagIndexCommand>.UpdateCommand(Command command)
        {
            command.IsVisible = SelectedNode != null && SelectedNode?.Tag is CachedTag;
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
