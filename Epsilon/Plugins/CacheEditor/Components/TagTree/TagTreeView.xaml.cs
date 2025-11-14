using EpsilonLib.Shell.TreeModels;
using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CacheEditor;
using TagTool.Cache;

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
            Loaded += TagTreeView_Loaded;

            EventManager.RegisterClassHandler(typeof(Window), Window.PreviewKeyUpEvent, new KeyEventHandler(TagTreeWindowKeyUp));
        }

        private void TagTreeView_Loaded(object sender, RoutedEventArgs e)
        {
            SearchBox.Focus();
            Keyboard.Focus(SearchBox);
        }

        private void TreeView_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var item = (e.OriginalSource as DependencyObject).FindAncestors<TreeViewItem>().FirstOrDefault();
            if (item != null)
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

        private void TreeViewItem_Selected(object sender, RoutedEventArgs e)
        {
            TreeViewItem tvi = e.OriginalSource as TreeViewItem;

            if (tvi == null || e.Handled) return;

            tvi.IsExpanded = !tvi.IsExpanded;
            //tvi.IsSelected = false;
            e.Handled = true;
        }

        private void TagTreeWindowKeyUp(object sender, KeyEventArgs e)
        {
            // ctrl-T to focus Tag Tree Search

            if ((e.Key == Key.T && e.KeyboardDevice.IsKeyDown(Key.LeftCtrl)) || (e.Key == Key.LeftCtrl && e.KeyboardDevice.IsKeyDown(Key.T)))
            {
                TagTreeViewModel tagTreeViewModel = (TagTreeViewModel)DataContext;
                if (tagTreeViewModel != null && IsVisible)
                {
                    SearchBox.Focus();
                    Keyboard.Focus(SearchBox);
                    SearchBox.Select(0, SearchBox.Text.Length);
                    e.Handled = true;
                }
            }
        }

        private void TreeView_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter)
                return;

            // If focus is in search box, ignore
            if (SearchBox.IsFocused)
                return;

            var vm = DataContext as TagTreeViewModel;
            if (vm == null)
                return;

            ITreeNode node = null;

            // Prefer the element that currently has keyboard focus
            var focused = Keyboard.FocusedElement as DependencyObject;
            if (focused != null)
            {
                // If the focused element itself has a DataContext that is the node
                if (focused is FrameworkElement fe && fe.DataContext is ITreeNode feNode)
                    node = feNode;

                // Otherwise walk up to find the enclosing TreeViewItem and use its DataContext
                if (node == null)
                {
                    var tvi = focused.FindAncestors<TreeViewItem>().FirstOrDefault();
                    if (tvi != null)
                        node = tvi.DataContext as ITreeNode;
                }
            }

            // Fallback to the view model's SelectedNode
            if (node == null)
                node = vm.SelectedNode;

            if (node?.Tag is CachedTag cached)
            {
                var args = new TreeNodeEventArgs(node);
                vm.SimulateDoubleClick(args);
                e.Handled = true;
            }
        }
    }
}
