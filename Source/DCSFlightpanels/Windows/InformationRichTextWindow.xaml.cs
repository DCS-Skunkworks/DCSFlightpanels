using System;
using System.IO;
using System.Windows;
using System.Windows.Documents;
using ClassLibraryCommon;

namespace DCSFlightpanels.Windows
{
    /// <summary>
    /// Interaction logic for InformationWindow.xaml
    /// </summary>
    public partial class InformationRichTextWindow : Window
    {
        private readonly byte[] _bytes;

        public InformationRichTextWindow(byte[] bytes)
        {
            InitializeComponent();
            _bytes = bytes;
        }

        private void InformationWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_bytes.Length == 0)
                {
                    return;
                }
                var memoryStream = new MemoryStream(_bytes, 0, _bytes.Length);
                memoryStream.Seek(0, SeekOrigin.Begin);
                var textRange = new TextRange(RichTextBoxInformation.Document.ContentStart, RichTextBoxInformation.Document.ContentEnd);
                textRange.Load(memoryStream, System.Windows.DataFormats.Rtf);
                memoryStream.Close();
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
