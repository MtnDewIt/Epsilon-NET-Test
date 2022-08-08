using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Windows.Navigation;
using WpfApp20;

namespace Epsilon.Pages
{
    /// <summary>
    /// Interaction logic for AboutView.xaml
    /// </summary>
    [Export]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public partial class AboutView : ChromeWindow
    {
        public AboutView()
        {
            InitializeComponent();
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(e.Uri.AbsoluteUri);
        }
    }
}
