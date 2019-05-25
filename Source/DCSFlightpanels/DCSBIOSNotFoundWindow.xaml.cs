using System;
using System.Windows;
using System.Windows.Documents;
using ClassLibraryCommon;

namespace DCSFlightpanels
{
    /// <summary>
    /// Interaction logic for DCSBIOSNotFoundWindow.xaml
    /// </summary>
    public partial class DCSBIOSNotFoundWindow : Window
    {
        private string _dcsbiosLocation;

        public DCSBIOSNotFoundWindow(string dcsbiosLocation)
        {
            InitializeComponent();
            _dcsbiosLocation = dcsbiosLocation;
        }

        private void ButtonClose_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                Close();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(19909017, ex);
            }
        }

        private void DCSBIOSNotFoundWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                RunSetting.Text = _dcsbiosLocation;
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(19909017, ex);
            }
        }
    }
}
