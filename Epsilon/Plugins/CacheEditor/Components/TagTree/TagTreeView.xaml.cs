using EpsilonLib.Shell.TreeModels;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CacheEditor.Components.TagTree
{
    /// <summary>
    /// Interaction logic for TagTreeView.xaml
    /// </summary>
    [Export]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public partial class TagTreeView : UserControl
    {
        public TagTreeView()
        {
            InitializeComponent();
        }

        private void TreeView_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var item = (e.OriginalSource as DependencyObject).FindAncestors<TreeViewItem>().FirstOrDefault();
            if(item != null)
            {
                item.Focus();
            }
            else
            {
                ((TreeView)sender).Focus();
            }
           
        }

        private void TreeViewItem_RequestBringIntoView(object sender, RequestBringIntoViewEventArgs e)
        {
            e.Handled = true;
        }
    }
}
