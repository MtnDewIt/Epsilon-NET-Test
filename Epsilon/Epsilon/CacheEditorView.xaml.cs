using System;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using Xceed.Wpf.AvalonDock;

namespace Epsilon
{
    /// <summary>
    /// Interaction logic for EpsilonView.xaml
    /// </summary>
    [Export]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public partial class CacheEditorView : UserControl
    {
        public CacheEditorView()
        {
            InitializeComponent();

            EventManager.RegisterClassHandler(typeof(Window), Window.PreviewKeyDownEvent, new KeyEventHandler(OnWindowKeyDown));
        }

        private void DockingManager_ActiveContentChanged(object sender, EventArgs e)
        {
            var dockingManager = (DockingManager)sender;
            Debug.WriteLine($"Active Content {dockingManager.ActiveContent}");
        }
        private void OnWindowKeyDown(object sender, KeyEventArgs e)
        {
            // ctrl-W to close current tag

            if ((e.Key == Key.W && e.KeyboardDevice.IsKeyDown(Key.LeftCtrl)) || (e.Key == Key.LeftCtrl && e.KeyboardDevice.IsKeyDown(Key.W)))
            {
                var cacheViewModel = DataContext as CacheEditorViewModel;
                if (cacheViewModel?.IsActive ?? false)
                {
                    if (cacheViewModel.ActiveItem is TagEditorViewModel currentTagViewModel && currentTagViewModel.CloseCommand.CanExecute(null))
                    {
                        currentTagViewModel.CloseCommand.Execute(null);
                    }
                    e.Handled = true;
                }
            }

			// ctrl-I to copy cache info to clipboard
			if (( e.Key == Key.I && e.KeyboardDevice.IsKeyDown(Key.LeftCtrl) ) || ( e.Key == Key.LeftCtrl && e.KeyboardDevice.IsKeyDown(Key.I) )) {
                var cacheViewModel = DataContext as CacheEditorViewModel;
				if (cacheViewModel?.IsActive ?? false) {
                    cacheViewModel.CopyCacheInfoToClipboard();
					e.Handled = true;
				}
			}

		}
    }
}