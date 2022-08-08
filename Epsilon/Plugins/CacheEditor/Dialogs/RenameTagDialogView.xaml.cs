using System.ComponentModel.Composition;
using System.Windows;

namespace CacheEditor.Views
{
    /// <summary>
    /// Interaction logic for RenameTagDialogView.xaml
    /// </summary>
    [Export]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public partial class RenameTagDialogView : Window
    {
        public RenameTagDialogView()
        {
            InitializeComponent();
        }
    }
}
