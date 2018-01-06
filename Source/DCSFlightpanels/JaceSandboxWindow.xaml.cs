using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using NonVisuals;

namespace DCSFlightpanels
{
    /// <summary>
    /// Interaction logic for JaceSandbox.xaml
    /// </summary>
    public partial class JaceSandboxWindow : Window
    {
        public JaceSandboxWindow()
        {
            InitializeComponent();
        }

        private void ButtonTestFormula_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                LabelErrors.Content = "";
                LabelResult.Content = "Result : " + Common.Evaluate(TextBoxFormula.Text);
                SetFormState();
            }
            catch (Exception ex)
            {
                LabelErrors.Content = ex.Message;
            }
        }

        private void TextBoxFormula_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            SetFormState();
        }

        private void SetFormState()
        {
            ButtonTestFormula.IsEnabled = !String.IsNullOrEmpty(TextBoxFormula.Text);
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
