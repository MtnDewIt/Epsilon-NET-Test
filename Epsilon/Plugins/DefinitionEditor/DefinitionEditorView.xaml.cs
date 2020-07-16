using System.ComponentModel.Composition;
using System.Windows.Controls;

namespace DefinitionEditor
{
    /// <summary>
    /// Interaction logic for TagEditorView.xaml
    /// </summary>

    [Export]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public partial class DefinitionEditorView : UserControl
    {
        public DefinitionEditorView()
        {
            InitializeComponent();
        }
    }
}
