using System;
using System.IO;
using System.Windows;
using System.Windows.Documents;
using ClassLibraryCommon;

namespace DCSFlightpanels
{
    /// <summary>
    /// Interaction logic for InformationWindow.xaml
    /// </summary>
    public partial class InformationTextBlockWindow : Window
    {
        private readonly string _message;

        public InformationTextBlockWindow(string message)
        {
            InitializeComponent();
            _message = message;
        }

        private void InformationWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(_message))
                {
                    return;
                }
                TextBlockInformation.Text = _message;
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(84348, ex);
            }
        }

        private void ButtonOkClick(object sender, RoutedEventArgs e)
        {
            try
            {
                Close();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(84349, ex);
            }
        }
    }
}
