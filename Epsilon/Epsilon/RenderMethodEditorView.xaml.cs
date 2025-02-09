using System.Windows.Controls;
using System.ComponentModel.Composition;

namespace Epsilon
{
    /// <summary>
    /// Interaction logic for RenderMethodEditorView.xaml
    /// </summary>
    [Export]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public partial class RenderMethodEditorView : UserControl
    {
        public RenderMethodEditorView() { InitializeComponent(); }
    }
}
