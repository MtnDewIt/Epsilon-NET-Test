using EpsilonLib.Logging;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml.Linq;
using static CacheEditor.Components.CommandLog.CommandLogViewModel;

namespace CacheEditor.Components.CommandLog
{
    /// <summary>
    /// Interaction logic for DependencyExplorerView.xaml
    /// </summary>
    [Export]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public partial class CommandLogView : UserControl
    {
        public CommandLogView()
        {
            InitializeComponent();
            Logger.CommandLogChanged += OnCommandLogChanged;
            tbSettingText.Text = string.Join("\n",Logger.GetCommandLog());
            clearText.DataContext = new CommandLogControls();
        }
        private void OnCommandLogChanged(object sender, System.EventArgs e)
        {
            tbSettingText.Text = string.Join("\n", Logger.GetCommandLog());
        }
    }
}
