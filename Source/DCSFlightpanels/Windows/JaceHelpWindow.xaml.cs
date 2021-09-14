namespace DCSFlightpanels.Windows
{
    using System;
    using System.Diagnostics;
    using System.Windows;
    using System.Windows.Navigation;

    /// <summary>
    /// Interaction logic for JaceHelpWindow.xaml
    /// </summary>
    public partial class JaceHelpWindow : Window
    {
        public JaceHelpWindow()
        {
            InitializeComponent();
        }

        private void ButtonClose_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Hyperlink_OnRequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
                e.Handled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
