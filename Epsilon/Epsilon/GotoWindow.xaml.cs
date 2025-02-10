using System;
using System.Windows;
using System.Windows.Input;

namespace Epsilon
{
	/// <summary>
	/// Interaction logic for GotoWindow.xaml
	/// </summary>
	public partial class GotoWindow : Window
    {
        public GotoWindow()
        {
            InitializeComponent();
        }

        protected override void OnActivated(EventArgs e)
        {
            txtInput.Focus();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void txtInput_KeyUp(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Return)
            {
                e.Handled = true;
                DialogResult = true;
            }
        }
    }
}
