using System.ComponentModel.Composition;
using System.Windows.Controls;
using Epsilon.Controls;

namespace Epsilon.Options
{
    /// <summary>
    /// Interaction logic for OptionsView.xaml
    /// </summary>
    [Export]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public partial class OptionsView : UserControl
    {
        public OptionsView()
        {
            InitializeComponent();
        }
    }
}
