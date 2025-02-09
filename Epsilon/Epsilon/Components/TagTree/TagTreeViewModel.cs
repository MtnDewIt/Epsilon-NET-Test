using Epsilon.Components.TagTree.Commands;
using Epsilon.TagEditing;
using Epsilon.Commands;
using Epsilon.Menus;
using Epsilon;
using Epsilon.Shell;
using Epsilon.Shell.Commands;
using Epsilon.Shell.TreeModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Windows;
using System.Windows.Input;
using TagTool.Cache;
using TagTool.Tags;
using TagTool.Tags.Definitions;

namespace Epsilon.Components.TagTree
{

    public enum TagTreeViewMode
    {
        Folders,
        Groups
    }

    public enum TagTreeGroupDisplayMode
    {
        TagGroup,
        TagGroupName
    }

    public class TagTreeViewModel : TreeModel, IDisposable,
        ICommandHandler<ToggleFoldersViewCommand>,
        ICommandHandler<ToggleGroupsViewCommand>,
        ICommandHandler<CopyCommand>,
        ICommandHandler<CopyTagNameCommand>,
        ICommandHandler<CopyTagIndexCommand>,
        ICommandHandler<ToggleGroupNameViewCommand>,
        ICommandHandler<ToggleGroupTagNameViewCommand>,
        ICommandHandler<ExtractBitmapCommand>,
        ICommandHandler<ExportModeJMSCommand>,
        ICommandHandler<ExportCollJMSCommand>,
        ICommandHandler<ExportPhmoJMSCommand>,
        ICommandHandler<ExtractSoundCommand>
    {

        private ICacheFile _cacheFile;
        private string _filterText = string.Empty;
        private bool _showAltText;
		private TagTreeViewMode _viewMode = TagTreeViewMode.Groups;
        private TagTreeGroupDisplayMode _groupDisplayMode = TagTreeGroupDisplayMode.TagGroupName;
        private TagExtract _extraction;
        private ICacheEditingService _cacheEditingService;

        public MenuItemDefinition ContextMenu { get; set; } = MenuDefinitions.ContextMenu;

        public TagTreeViewModel(ICacheEditingService cacheEditingService, ICacheFile cacheFile, TagExtract extraction = null)
        {

			// The TagTree supports multi-selection
			MultiSelectionEnabled = true;

            _viewMode = cacheEditingService.Settings.GetEnum<TagTreeViewMode>(Settings.TagTreeViewModeSetting);
            _groupDisplayMode = cacheEditingService.Settings.GetEnum<TagTreeGroupDisplayMode>(Settings.TagTreeGroupDisplaySetting);

            _cacheEditingService = cacheEditingService;
            _cacheFile = cacheFile;
            _extraction = extraction;
            _cacheFile.TagSerialized += CacheFile_TagSaved;

            FilterText = string.Empty;

            Refresh();
            //FilterRefresh();

        }

        private void CacheFile_TagSaved(object sender, CachedTag e) {
            if(FindTag(e) is TreeNode node) {
                node.Tag = e; node.UpdateAppearance();
            }
        }

        public TagTreeViewMode ViewMode
        {
            get => _viewMode;
            set { if (SetAndNotify(ref _viewMode, value)) { Refresh(true); } }
        }

        public TagTreeGroupDisplayMode GroupDisplayMode
        {
            get => _groupDisplayMode;
            set { if (SetAndNotify(ref _groupDisplayMode, value)) { Refresh(true); } }
        }

        public string FilterText
        {
            get => _filterText;
            set { 
                if (value != null) { value = value.Trim(); } else { value = string.Empty; }
                _filterTextBlank = value == string.Empty;
				_filterTextIndex = value.StartsWith("0x");
                if (_filterTextIndex) { 
                    if (!int.TryParse(value.Remove(0, 2), NumberStyles.HexNumber, null, out _filterTextIndexValue)) {
                        _filterTextIndex = false;
                        _filterTextIndexValue = -1;
                    }
                }
				if (SetAndNotify(ref _filterText, value)) { FilterRefresh(); }
            }
        }
        private bool _filterTextBlank = false;
        private bool _filterTextIndex = false;
        private int _filterTextIndexValue = -1;

		public void Refresh(bool retainState = false)
        {

			List<string> expandedNodes = new List<string>();
            if (retainState) { expandedNodes = GetExpandedNodeNames(); }
            List<TreeNode> currentlySelectedNodes = SelectedNodes.ToList();

			switch (_viewMode) {
                
                case TagTreeViewMode.Folders:
					FolderView_BuildTree(_cacheFile.Cache, FilterTag, true);
					break;

				case TagTreeViewMode.Groups:
					_showAltText = _cacheEditingService.Settings.GetBool(Settings.ShowTagGroupAltNamesSetting);
					GroupView_BuildTree(_cacheFile.Cache, true);
					break;

                default:
                    break;

			}

            if (retainState) {
                foreach (TreeNode node in Nodes) {
                    if (expandedNodes.Contains(node.Text)) { node.IsExpanded = true; }
                    if (currentlySelectedNodes.Any(n => n.Text == node.Text)) {
                        SelectedNodes.Add(node);
                    }
                }
            }

            if (Nodes.Count == 1) { Nodes.First().IsExpanded = true; }

        }

		public void FilterRefresh() 
        {
			switch (_viewMode) {
				
                case TagTreeViewMode.Folders:
					FolderView_BuildTree(_cacheFile.Cache, FilterTag, false);
					break;

				case TagTreeViewMode.Groups:
                    GroupView_BuildTree(_cacheFile.Cache, false);
                    break;

                default:
					break;

			}
		}

        private void FilterGroupNode(GroupNode groupNode) {
			if (_filterTextBlank || groupNode.TagGroupName.IndexOf(FilterText, StringComparison.OrdinalIgnoreCase) >= 0 || groupNode.TagGroupAbbreviation.IndexOf(FilterText, StringComparison.OrdinalIgnoreCase) >= 0) {
			    // || groupNode.Text.ToString().IndexOf(FilterText, StringComparison.OrdinalIgnoreCase) >= 0
                groupNode.IsVisible = true;
				foreach (TreeNode node in groupNode.Children) { node.IsVisible = true; }
			}
            else {
                foreach (TreeNode node in groupNode.Children) { node.IsVisible = FilterTreeNode(node); }
                groupNode.IsVisible = groupNode.Children.Any(n => n.IsVisible);
			}

		}

        private bool FilterTreeNode(TreeNode treeNode) {
            if (treeNode == null) { return false; }
			if (_filterTextBlank) { return true; }
			return treeNode.Text.ToString().IndexOf(FilterText, StringComparison.OrdinalIgnoreCase) >= 0;
		}

		private bool FilterTag(CachedTag tag)
        {

            if (tag == null) { return false; }
			if (_filterTextBlank) { return true; }
			if (tag.Group == null) { return false; }

			// check for filter match with tag group name
            if (tag.Group.Tag.ToString().IndexOf(FilterText, StringComparison.OrdinalIgnoreCase) >= 0) { return true; }

            // check for filter match with tag name
            if (tag.ToString().IndexOf(FilterText, StringComparison.OrdinalIgnoreCase) >= 0) { return true; }

			// check for filter match with tag index
			if (_filterTextIndex) { return tag.Index == _filterTextIndexValue || ( tag.ID != 0 && tag.ID == _filterTextIndexValue ); }
            
            return false;

        }

        public TreeNode FindTag(CachedTag tag) { return FindNodesWithTag(tag, Nodes).FirstOrDefault(); }

        public void EnsureVisible(TreeNode treeNode)
        {
			TreeNode node = treeNode;
            while(node != null) {
                node.IsExpanded = true;
                node = node.Parent;
            }
        }

        List<string> GetExpandedNodeNames() {
            try { return GroupNodes.Where(n => n.IsExpanded).Select(n => n.Text).ToList(); }
            catch { return new List<string>(); }
		}

		/// <summary>
		/// Handles the MouseDown event for the TreeView.
		/// </summary>
		/// <param name="sender"> This should be the <see cref="TreeNode"/> that was clicked. </param>
		/// <param name="e"> The <see cref="MouseButtonEventArgs"/> for the event. </param>
		public void OnMouseDown(object sender, MouseButtonEventArgs e) {
			// if a TreeNode was clicked, call the base OnNodeSelected method with appropriate TreeNodeEventArgs
			if (sender is TreeNode node) {
                OnNodeSelected(new TreeNodeEventArgs(e.RoutedEvent, sender, node, Key.None, Keyboard.Modifiers));
            }
		}

		/// <summary>
		/// Handles the KeyDown event for the TreeView.
		/// </summary>
		public void OnKeyDown(object sender, KeyEventArgs e) {
			// if a relevant keyboard event was triggered, call the base OnNodeSelected method with appropriate TreeNodeEventArgs
            if (sender is TreeNode node) {
				OnNodeAttemptActivate(new TreeNodeEventArgs(e.RoutedEvent, sender, node, e.Key, Keyboard.Modifiers));
			}
		}

		public object GetFirstVisible() { return GroupNodes.FirstOrDefault(n => n.IsVisible); }
		public void FocusFirstVisibleGroup() {
            TreeNode firstVisible = GroupNodes.FirstOrDefault(n => n.IsVisible);
            if (firstVisible != null) { SelectedNodes.Add(firstVisible); }
		}

		#region Group View

		public void GroupView_BuildTree(GameCache cache, bool readCache = false) {

			if (readCache) {
				CacheNodes.Clear();
				CacheNodes = cache.TagCache
					.NonNull()
					.OrderByDescending(tag => tag.Name)
					.Select(tag => new TreeNode(this, tag, tag.Name ?? $"0x{tag.Index:X8}"))
					.ToList();

				NodesByTag = CacheNodes.Where(node => node.IsTag).ToDictionary(node => node.Tag as CachedTag);
			}

            GroupNodes.Clear();

            List<TreeNode> groupView = CacheNodes.Where(node => node.IsTag).ToList();
			List<IGrouping<TagGroup, TreeNode>> x = groupView.GroupBy(node => ( node.Tag as CachedTag ).Group, node => NodesByTag[node.Tag as CachedTag]).ToList(); // Group tags by their TagGroup
            List<GroupNode> groups = x.Select(group => GroupView_CreateGroupNode(group)).OrderBy(node => node.Text).ToList(); // Create a TagTreeGroupNode for each group, and add the tags to it
			// groupView = groups.OrderBy(node => node.Text);  // Order the groups by name

			foreach (GroupNode group in GroupNodes) { FilterGroupNode(group); }

			SetNodesCollection(
				GroupNodes
				.Where(group => group.IsVisible)
				.Select(group => group.VisibleGroup())
				.Cast<TreeNode>()
				.ToList()
			);

			// To go a bit more in depth on the LINQ query:
			// 1. We filter out any null tags
			// 2. We filter out any tags that don't match the name / group name text filter
			// 3. We group the tags by their TagGroup
			// --> This is done by calling the CreateTagNode function for each tag in the group
			// --> This creates a TreeNode for each tag, which is then grouped by the TagGroup
			// --> This effectively creates a mapping or dictionary of TagGroups to TreeNodes, where each TagGroup corresponds to one or more TreeNodes
			// 4. We create a TagTreeGroupNode for each group, and add the tags to it
			// --> This is done by calling the CreateGroupNode function for each group
			// --> This creates a TagTreeGroupNode for each group, and adds the tags to it
		}

		private GroupNode GroupView_CreateGroupNode(IGrouping<TagGroup, TreeNode> group) {

            string text = _groupDisplayMode == TagTreeGroupDisplayMode.TagGroup
                ? group.Key.ToString()
                : group.Key.Tag.ToString();

            string altText = _showAltText
                ? ( _groupDisplayMode == TagTreeGroupDisplayMode.TagGroupName ? group.Key.Tag.ToString() : group.Key.ToString() )
                : null;

			GroupNode groupNode = new GroupNode(this, group.OrderBy(node => node.Text).GetEnumerator(), text, altText, group.Key.ToString(), group.Key.Tag.ToString());

            GroupNodes.Add(groupNode);

			foreach (TreeNode node in group) { node.Parent = groupNode; }

			return groupNode;

		}

		#endregion

		#region Folder View

		//TODO refactor the folder view construction to make it more straightforward and properly assign parent references

		public Dictionary<string, FolderNode> Folders { get; set; }
			= new Dictionary<string, FolderNode>();

		public void FolderView_BuildTree(GameCache cache, Func<CachedTag, bool> filter, bool readCache = false) {

            IEnumerable<CachedTag> tags;
                
            if (readCache) {
				CacheNodes.Clear();
				CacheNodes = cache.TagCache
                    .NonNull()
                    .OrderByDescending(tag => tag.Name)
                    .Select(tag => new TreeNode(this,tag, tag.Name ?? $"0x{tag.Index:X8}"))
                    .ToList();

				NodesByTag = CacheNodes.ToDictionary(node => node.Tag as CachedTag);
			}

			Folders.Clear(); // Dictionary<string, FolderNode> folderLookup = new Dictionary<string, FolderNode>();

            tags = CacheNodes.Where(node => node.IsTag).Select(node => node.Tag as CachedTag).Where(filter);

            List<TreeNode> nodes = new List<TreeNode>();
			
            foreach (KeyValuePair<CachedTag, TreeNode> pair in NodesByTag) { 
                FolderView_AddTags(nodes, pair.Key, pair.Value /**, folderLookup**/); 
            }
			FolderView_SortNodes(nodes);

            SetNodesCollection(nodes);

		}

		private void FolderView_AddTags(IList<TreeNode> roots, CachedTag tag, TreeNode node /**, Dictionary<string, FolderNode> Folders**/) {
			IList<TreeNode> currentRoots = roots;

			if (tag.Name == null) { currentRoots.Add(node); return; }

			string[] segments = tag.Name.Split('\\');

			for (int i = 0; i < segments.Length; i++) {
				if (i < segments.Length - 1) {

					string folderKey = string.Join("\\", segments.Take(i + 1));

					if (!Folders.TryGetValue(folderKey, out FolderNode folderNode)) {
						folderNode = new FolderNode(this, segments[i]);
						currentRoots.Add(folderNode);
						Folders[folderKey] = folderNode;
					}

					currentRoots = folderNode.Children;
				}
				else { currentRoots.Add(node); }
			}

		}

		private void FolderView_SortNodes(IList<TreeNode> nodes) {
			List<FolderNode> folderNodes = nodes.OfType<FolderNode>().OrderBy(n => n.Text).ToList();
			List<TreeNode> tagNodes = nodes.OfType<TreeNode>().OrderBy(n => n.Text).ToList();

			nodes.Clear();
			foreach (FolderNode folderNode in folderNodes) {
				nodes.Add(folderNode);
			}
			foreach (TreeNode tagNode in tagNodes) {
				nodes.Add(tagNode);
			}
			foreach (FolderNode folderNode in folderNodes) {
				FolderView_SortNodes(folderNode.Children);
			}
		}

		//private string FormatNameFolderView(CachedTag tag)
		//{
		//    return tag.Name == null 
		//        ? $"0x{tag.Index:X8}.{tag.Group}" 
		//        : $"{Path.GetFileName(tag.Name)}.{tag.Group}";
		//}

		#endregion

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
            if (SelectedNode is TreeNode node)
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
            if (SelectedNode is TreeNode node && node.Tag is CachedTag tag)
                    ClipboardEx.SetTextSafe($"{tag}");
        }

        void ICommandHandler<CopyTagIndexCommand>.ExecuteCommand(Command command)
        {
            if (SelectedNode is TreeNode node && node.Tag is CachedTag tag)
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
                _extraction.ExtractBitmap(_cacheFile.Cache, tag);
        }

        void ICommandHandler<ExtractBitmapCommand>.UpdateCommand(Command command)
        {
            command.IsVisible = SelectedNode?.Tag is CachedTag tag && tag.IsInGroup("bitm");
        }

        void ICommandHandler<ExportModeJMSCommand>.ExecuteCommand(Command command)
        {
            if (SelectedNode?.Tag is CachedTag tag)
                _extraction.ExportJMS(_cacheFile.Cache, tag, "mode");
        }

        void ICommandHandler<ExportModeJMSCommand>.UpdateCommand(Command command)
        {
            using (System.IO.Stream stream = _cacheFile.Cache.OpenCacheRead())
            {
                if (SelectedNode?.Tag is CachedTag tag && tag.IsInGroup("hlmt"))
                {
                    Model model = _cacheFile.Cache.Deserialize<Model>(stream, SelectedNode?.Tag as CachedTag);
                    command.IsVisible = model.RenderModel != null;
                }
                else
                    command.IsVisible = false;
            }
        }

        void ICommandHandler<ExportCollJMSCommand>.ExecuteCommand(Command command)
        {
            if (SelectedNode?.Tag is CachedTag tag)
                _extraction.ExportJMS(_cacheFile.Cache, tag, "coll");
        }

        void ICommandHandler<ExportCollJMSCommand>.UpdateCommand(Command command)
        {
            using (System.IO.Stream stream = _cacheFile.Cache.OpenCacheRead())
            {
                if (SelectedNode?.Tag is CachedTag tag && tag.IsInGroup("hlmt"))
                {
                    Model model = _cacheFile.Cache.Deserialize<Model>(stream, SelectedNode?.Tag as CachedTag);
                    command.IsVisible = model.CollisionModel != null;
                }
                else
                    command.IsVisible = false;
            }
        }

        void ICommandHandler<ExportPhmoJMSCommand>.ExecuteCommand(Command command)
        {
            if (SelectedNode?.Tag is CachedTag tag)
                _extraction.ExportJMS(_cacheFile.Cache, tag, "phmo");
        }

        void ICommandHandler<ExportPhmoJMSCommand>.UpdateCommand(Command command)
        {
            using (System.IO.Stream stream = _cacheFile.Cache.OpenCacheRead())
            {
                if (SelectedNode?.Tag is CachedTag tag && tag.IsInGroup("hlmt"))
                {
                    Model model = _cacheFile.Cache.Deserialize<Model>(stream, SelectedNode?.Tag as CachedTag);
                    command.IsVisible = model.CollisionModel != null;
                }
                else
                    command.IsVisible = false;
            }
        }

        void ICommandHandler<ExtractSoundCommand>.ExecuteCommand(Command command)
        {
            if (SelectedNode?.Tag is CachedTag tag)
                _extraction.ExtractSound(_cacheFile.Cache, tag);
        }

        void ICommandHandler<ExtractSoundCommand>.UpdateCommand(Command command)
        {
            command.IsVisible = SelectedNode?.Tag is CachedTag tag && tag.IsInGroup("snd!");
        }


        #endregion

        public new void Dispose()
        {
            base.Dispose();
            if(_cacheFile != null)
            {
                _cacheFile.Cache = null;
                _cacheFile = null;
            }
            _cacheEditingService = null;
        }

    }

}
