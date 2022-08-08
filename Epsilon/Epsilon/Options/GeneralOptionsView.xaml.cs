using System.ComponentModel.Composition;
using System.Windows.Controls;

namespace Epsilon.Options
{
    [Export]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public partial class GeneralOptionsView : UserControl
    {
        public GeneralOptionsView()
        {
            InitializeComponent();
        }
    }
}
