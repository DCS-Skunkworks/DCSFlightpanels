using System.Diagnostics;
using System.Windows;
using System.Windows.Navigation;

namespace DCSFlightpanels
{
    /// <summary>
    /// Interaction logic for AboutFPWindow.xaml
    /// </summary>
    public partial class AboutFpWindow : Window
    {
        public AboutFpWindow()
        {
            InitializeComponent();
        }

        private void WindowLoaded(object sender, RoutedEventArgs e)
        {

        }

        private void HyperlinkRequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }
    }
}
