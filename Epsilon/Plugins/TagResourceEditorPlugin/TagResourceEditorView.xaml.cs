using System.ComponentModel.Composition;
using System.Windows.Controls;

namespace TagResourceEditorPlugin
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    [Export]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public partial class TagResourceEditorView : UserControl
    {
        public TagResourceEditorView()
        {
            InitializeComponent();
        }
    }
}
