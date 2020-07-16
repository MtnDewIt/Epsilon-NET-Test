using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Xceed.Wpf.AvalonDock;

namespace CacheEditor
{
    /// <summary>
    /// Interaction logic for CacheEditorView.xaml
    /// </summary>
    [Export]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public partial class CacheEditorView : UserControl
    {
        public CacheEditorView()
        {
            InitializeComponent();
        }

        private void DockingManager_ActiveContentChanged(object sender, EventArgs e)
        {
            var dockingManager = (DockingManager)sender;
            Debug.WriteLine($"Active Content {dockingManager.ActiveContent}");
        }
    }
}
