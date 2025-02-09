using Epsilon.Themes;
using Stylet;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using TagTool.Cache;
using TagTool.Cache.HaloOnline;
using TagTool.Tags;

namespace Epsilon.Shell.TreeModels
{

	public class FolderNode : TreeNode
	{
        public FolderNode(TreeModel treeModel, string name) {
			TreeModel = treeModel;
			Text = name; 
        }
		public override bool IsLeafNode { get; set; } = false;
	}

	public class GroupNode : TreeNode
	{
        public GroupNode(TreeModel treeModel, object tag, string text, string altText, string tagGroupName, string tagGroupAbbreviation) {
            TreeModel = treeModel; Text = text; AltText = altText; TagGroupName = tagGroupName; TagGroupAbbreviation = tagGroupAbbreviation;
			if (tag is IEnumerator<TreeNode> nodes) {
				while (nodes.MoveNext()) { Children.Add(nodes.Current); }
			}
		}

        public GroupNode VisibleGroup() {
            GroupNode group = new GroupNode(TreeModel, Tag, Text, AltText, TagGroupName, TagGroupAbbreviation);
            foreach (TreeNode child in Children) {
                if (child.IsVisible) { group.Children.Add(child); }
            }
            return group;
        }

		public string TagGroupName { get; set; }
        public string TagGroupAbbreviation { get; set; }

		public override bool IsLeafNode { get; set; } = false;
	}

	public class TreeNode : PropertyChangedBase
    {

        public static TreeNode LastSelected { get; set; }

        public TreeModel TreeModel { get; set; }

		protected TreeNode() { }
		public TreeNode(TreeModel treeModel, CachedTag tag, string name) {
			TreeModel = treeModel;
			_children = new ObservableCollection<TreeNode>();
			Tag = tag;
			Text = name;
			UpdateAppearance();
		}

		private object _tag;
        public object Tag
        {
            get => _tag;
            set => SetAndNotify(ref _tag, value);
        }

        public bool IsTag => Tag is CachedTag;

		public virtual bool IsLeafNode { get; set; } = true;

		private bool _isSelected = false;
        public bool IsSelected
        {
            get => _isSelected;
            set { 
                if (!_isSelected && value) { LastSelected = this; }
				SetAndNotify(ref _isSelected, value); 
            }
        }

        private bool _isExpanded = false;
        public bool IsExpanded
        {
            get => _isExpanded;
            set { SetAndNotify(ref _isExpanded, value); if (!value) { DeselectChildren(); } }
        }

        public void SelectChildren() {
            if (TreeModel == null) { return; }
			foreach (TreeNode child in Children) {
                if (TreeModel.SelectedNodes.Contains(child)) { continue; }
				TreeModel.SelectedNodes.Add(child);
			}
		}
        public void DeselectChildren() {
            if (TreeModel == null) { return; }
            foreach (TreeNode child in Children) {
                TreeModel.SelectedNodes.Remove(child);
            }
        }

		private bool _isVisible = true;
		public bool IsVisible {
			get => _isVisible;
			set { SetAndNotify(ref _isVisible, value); if (!value && _isSelected) { IsSelected = false; } }
		}

		private TreeNode _parent;
		public TreeNode Parent {
			get => _parent;
			set => SetAndNotify(ref _parent, value);
		}

        private IList<TreeNode> _children = new ObservableCollection<TreeNode>();
		public IList<TreeNode> Children
        {
            get => _children;
            protected set => SetAndNotify(ref _children, value);
        }

        private string _text;
        public string Text
        {
            get => _text;
            set => SetAndNotify(ref _text, value);
        }
        
        private string _altText;
        public string AltText
        {
            get => _altText;
            set => SetAndNotify(ref _altText, value);
        }

        private ColorHint _textColor;
        public ColorHint TextColor
        {
            get => _textColor;
            set => SetAndNotify(ref _textColor, value);
        }

		public void UpdateAppearance() { TextColor = DetermineTextColor(); }

		private ColorHint DetermineTextColor() {
			return  (Tag is CachedTagHaloOnline hoTag && hoTag.IsEmpty())
                ?  ColorHint.Muted
                :  ColorHint.Default;
		}


	}

}
