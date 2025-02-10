using Epsilon.Shell.TreeModels;
using Stylet;
using System.Linq;

namespace Epsilon.Options
{
	public class OptionsViewModel : Conductor<IOptionsPage>.Collection.OneActive
    {
        public OptionsPageTreeViewModel CategoryTree { get; }

        private IOptionsService _optionsService;

        public OptionsViewModel(IOptionsService optionsService)
        {
            _optionsService = optionsService;
            DisplayName = "Options";
            CategoryTree = new OptionsPageTreeViewModel(optionsService.OptionPages);
            CategoryTree.NodeSelected += CategoryTree_NodeSelected;

            foreach (var node in CategoryTree.Nodes.Where(x => x.Children != null)) {
                node.IsExpanded = true;
            }

            foreach (TreeNode n in CategoryTree.Nodes) {
                SetOptionsViewModelRecursive(n);
			}

        }

        private void SetOptionsViewModelRecursive(TreeNode node) {
			if (node is OptionsTreeNode n) {
				n.OptionsViewModel = this;
			}
			foreach (TreeNode child in node.Children) {
				SetOptionsViewModelRecursive(child);
			}
		}

		public void CategoryTree_NodeSelected(object sender, TreeNodeEventArgs e)
        {
            if(e.Node is OptionsTreeNode node)
            {
                ActiveItem = node.Page;
                if(node.Children.Count > 0)
                    node.IsExpanded = true;
            }
        }

        public void Apply() { 
            ApplyChanges(); 
        }

        public void Save() {
            ApplyChanges();
			RequestClose(true);
		}

        private void ApplyChanges() {
			foreach (var page in _optionsService.OptionPages) {
				if (page.IsDirty)
					page.Apply();

				page.IsDirty = false;
			}
		}

        public void Cancel()
        {
            GeneralOptionsViewModel general = (GeneralOptionsViewModel)_optionsService.OptionPages.First();
            general.RevertAppearance();

            RequestClose(false);
        }
    }
}
