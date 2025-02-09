using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Epsilon.Options
{
    /// <summary>
    /// Interaction logic for OptionsPageTreeView.xaml
    /// </summary>
    /// 
    [Export]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public partial class OptionsPageTreeView : UserControl
    {
        public OptionsPageTreeView()
        {
            InitializeComponent();
        }

		private void TreeViewItem_PreviewMouseDown(object sender, MouseButtonEventArgs e) {
			if (e.OriginalSource is FrameworkElement content) { 
                if (content.DataContext is OptionsTreeNode node) {
                    node.OptionsViewModel.CategoryTree_NodeSelected(this, new Epsilon.Shell.TreeModels.TreeNodeEventArgs(e.RoutedEvent, sender, node));
					e.Handled = true;
				}
            }
		}

	}
}
