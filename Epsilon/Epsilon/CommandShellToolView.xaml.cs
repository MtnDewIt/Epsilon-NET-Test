using System.ComponentModel.Composition;
using System.Windows.Controls;

namespace Epsilon
{
    /// <summary>
    /// Interaction logic for CommandShellToolView.xaml
    /// </summary>

    [Export]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public partial class CommandShellToolView : UserControl
    {
        public CommandShellToolView()
        {
            InitializeComponent();
        }
    }
}
