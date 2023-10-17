using System;
using System.Windows;
using System.Windows.Documents;
using ClassLibraryCommon;

namespace DCSFlightpanels.Windows
{
    /// <summary>
    /// Interaction logic for InformationWindow.xaml
    /// </summary>
    public partial class InformationTextBlockWindow
    {
        private readonly string _message;

        public InformationTextBlockWindow()
        {
            InitializeComponent();
        }

        public InformationTextBlockWindow(string message)
        {
            InitializeComponent();
            _message = message;
        }

        public void AddInline(string text)
        {
            TextBlockInformation.Inlines.Add(text);
        }

        public void AddInline(Inline inline)
        {
            TextBlockInformation.Inlines.Add(inline);
        }

        private void InformationWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            DarkMode.SetFrameworkElementDarkMode(this);
            try
            {
                TextBlockInformation.TextWrapping = TextWrapping.Wrap;
                TextBlockInformation.Margin = new Thickness(10);
                if (string.IsNullOrWhiteSpace(_message))
                {
                    return;
                }
                TextBlockInformation.Text = _message;
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox( ex);
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
                Common.ShowErrorMessageBox( ex);
            }
        }
    }
}
