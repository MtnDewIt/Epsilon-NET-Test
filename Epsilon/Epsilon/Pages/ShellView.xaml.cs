using System.ComponentModel.Composition;
using WpfApp20;

namespace Epsilon.Pages
{
    /// <summary>
    /// Interaction logic for ShellView.xaml
    /// </summary>
    [Export]
    
    public partial class ShellView : ChromeWindow
    {
        public ShellView()
        {
            InitializeComponent();

        }
    }
}
