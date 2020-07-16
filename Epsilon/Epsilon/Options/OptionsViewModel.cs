using EpsilonLib.Options;
using EpsilonLib.Shell.TreeModels;
using Stylet;

namespace Epsilon.Options
{
    class OptionsViewModel : Conductor<IOptionsPage>.Collection.OneActive
    {
        public OptionsPageTreeViewModel CategoryTree { get; }

        private IOptionsService _optionsService;

        public OptionsViewModel(IOptionsService optionsService)
        {
            _optionsService = optionsService;
            DisplayName = "Options";
            CategoryTree = new OptionsPageTreeViewModel(optionsService.OptionPages);
            CategoryTree.NodeSelected += CategoryTree_NodeSelected;
        }

        private void CategoryTree_NodeSelected(object sender, TreeNodeEventArgs e)
        {
            if(e.Node is OptionsTreeNode node)
            {
                ActiveItem = node.Page;
                if(node.Children.Count > 0)
                    node.IsExpanded = true;
            }
        }

        public void Apply()
        {
            foreach(var page in _optionsService.OptionPages)
            {
                if(page.IsDirty)
                    page.Apply();

                page.IsDirty = false;
            }

            RequestClose(true);
        }

        public void Cancel()
        {
            RequestClose(false);
        }
    }
}
