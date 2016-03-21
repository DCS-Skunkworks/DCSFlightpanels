using System.Diagnostics;
using System.Windows;
using System.Windows.Input;

namespace ProUsbPanels
{
    /// <summary>
    /// Interaction logic for DonationWindow.xaml
    /// </summary>
    public partial class DonationWindow : Window
    {
        public DonationWindow()
        {
            InitializeComponent();
        }

        private void Screenshots_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            Process.Start("https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=VN8TKY9NFQBRQ");
        }
    }
}
