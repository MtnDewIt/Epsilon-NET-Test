using System.ComponentModel.Composition;
using System.Windows.Controls;

namespace BlamScriptEditorPlugin
{
    /// <summary>
    /// Interaction logic for ScriptTagEditorView.xaml
    /// </summary>
    [Export]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public partial class ScriptTagEditorView : UserControl
    {
        public ScriptTagEditorView()
        {
            InitializeComponent();
        }
    }
}
