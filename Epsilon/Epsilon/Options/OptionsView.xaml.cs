using System.ComponentModel.Composition;
using System.Windows;

namespace Epsilon.Options
{
    /// <summary>
    /// Interaction logic for OptionsView.xaml
    /// </summary>
    [Export]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public partial class OptionsView : Window
    {
        public OptionsView()
        {
            InitializeComponent();
        }
    }
}
