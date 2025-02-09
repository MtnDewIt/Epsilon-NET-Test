using Epsilon.Options;
using Epsilon.Shell.TreeModels;
using Stylet;
using System.Collections.Generic;
using System.Linq;

namespace Epsilon.Options
{
    public class OptionsPageTreeViewModel : TreeModel
    {

        public override bool MultiSelectionEnabled { get; protected set; } = false;

		public OptionsPageTreeViewModel(IEnumerable<IOptionsPage> pages)
        {
			IEnumerable<OptionsTreeNode> pageModels = pages.Select(page => new OptionsTreeNode(page.DisplayName, page, default));
            SetNodesCollection(new BindableCollection<TreeNode>(pageModels.GroupBy(page => page.Page.Category)
                .Select(group => new OptionsTreeNode(group.Key, group.FirstOrDefault()?.Page, group))));
        }
    }
}
