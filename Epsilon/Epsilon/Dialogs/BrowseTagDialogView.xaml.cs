using System.ComponentModel.Composition;
using Epsilon.Controls;
using Stylet;

namespace Epsilon
{
    /// <summary>
    /// Interaction logic for BrowseTagDialog.xaml
    /// </summary> 
    [Export]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public partial class BrowseTagDialogView : ChromeWindow
    {
        public BrowseTagDialogView()
        {
            InitializeComponent();
        }
    }
}
