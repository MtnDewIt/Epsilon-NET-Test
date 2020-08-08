using System;
using System.Media;
using System.Windows;
using WpfApp20;

namespace Epsilon
{
    /// <summary>
    /// Interaction logic for ExceptionDialog.xaml
    /// </summary>
    public partial class ExceptionDialog : Window
    {
        public Exception Exception { get; }

        public ExceptionDialog(Exception ex)
        {
            SystemSounds.Hand.Play();
            Exception = ex;
            this.DataContext = this;
            InitializeComponent();
        }

        private void btnCopy_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(Exception.ToString());
        }

        private void btnIgnore_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            App.Current.Shutdown();
        }
    }
}
