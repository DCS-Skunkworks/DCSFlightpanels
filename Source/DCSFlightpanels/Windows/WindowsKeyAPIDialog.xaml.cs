using System.Windows;

namespace DCSFlightpanels.Windows
{
    /// <summary>
    /// Interaction logic for WindowsKeyAPIDialog.xaml
    /// </summary>
    public partial class WindowsKeyAPIDialog
    {
        public bool ShowAtStartUp;

        public WindowsKeyAPIDialog()
        {
            InitializeComponent();
        }

        private void ButtonClose_OnClick(object sender, RoutedEventArgs e)
        {
            ShowAtStartUp = CheckBoxShowAgain.IsChecked == true;
            Close();
        }
    }
}
