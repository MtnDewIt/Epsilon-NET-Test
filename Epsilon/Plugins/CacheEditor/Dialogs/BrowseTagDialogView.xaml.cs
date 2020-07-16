using System.ComponentModel.Composition;
using System.Windows;

namespace CacheEditor
{
    /// <summary>
    /// Interaction logic for BrowseTagDialog.xaml
    /// </summary> 
    [Export]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public partial class BrowseTagDialogView : Window
    {
        public BrowseTagDialogView()
        {
            InitializeComponent();
        }
    }
}
