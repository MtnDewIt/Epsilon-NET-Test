using System.ComponentModel.Composition;
using System.Windows.Controls;

namespace Epsilon.Options
{
    /// <summary>
    /// Interaction logic for EpsilonOptionsPageView.xaml
    /// </summary>

    [Export]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public partial class GeneralOptionsPageView : UserControl
    {
        public GeneralOptionsPageView()
        {
            InitializeComponent();
        }
    }
}
