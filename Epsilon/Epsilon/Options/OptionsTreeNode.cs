using Epsilon.Options;
using Epsilon.Shell.TreeModels;
using Stylet;
using System.Collections.Generic;

namespace Epsilon.Options
{
    class OptionsTreeNode : TreeNode
    {

        public IOptionsPage Page { get; set; }

        public OptionsViewModel OptionsViewModel { get; set; }

		public OptionsTreeNode(string displayName, IOptionsPage page, IEnumerable<OptionsTreeNode> children) {
            Text = displayName; Page = page;
            if(children != null) { Children = new BindableCollection<TreeNode>(children); }
        }

    }
}
