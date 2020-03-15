using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;
using ClassLibraryCommon;
using DCS_BIOS;

namespace DCSFlightpanels.Windows
{
    /// <summary>
    /// Interaction logic for JaceSandbox.xaml
    /// </summary>
    public partial class StreamDeckJaceDecodeFormulaWindow : Window
    {

        private volatile uint _value1 = 0;
        private bool _dataChanged;
        private bool _formLoaded;
        private DataGrid _dataGridValues;
        private readonly JaceExtended _jaceExtended = new JaceExtended();
        private Dictionary<string, double> _variables = new Dictionary<string, double>();
        private bool _formulaHadErrors = false;
        private bool _changesMade = false;

        public StreamDeckJaceDecodeFormulaWindow(DCSBIOS dcsbios)
        {
            InitializeComponent();
            _variables.Add("DCSBIOS", 0);
        }

        private void ButtonTestFormula_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var formula = TextBoxFormula.Text;
                _variables["DCSBIOS"] = double.Parse(TextBoxDCSBIOSValue.Text);
                
                
                var result = _jaceExtended.CalculationEngine.Calculate(formula, _variables);

                LabelErrors.Content = "";
                LabelResult.Content = "Result : " + result;
                SetFormState();
                _formulaHadErrors = false;
            }
            catch (Exception ex)
            {
                LabelErrors.Content = ex.Message;
                _formulaHadErrors = true;
            }
        }

        private void TextBoxFormula_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            _changesMade = true;
            SetFormState();
        }

        private void SetFormState()
        {
            if (!_formLoaded)
            {
                return;
            }

            ButtonTestFormula.IsEnabled = !string.IsNullOrEmpty(TextBoxDCSBIOSValue.Text) && !string.IsNullOrEmpty(TextBoxFormula.Text);
            ButtonSave.IsEnabled = !string.IsNullOrEmpty(TextBoxDCSBIOSValue.Text) && !string.IsNullOrEmpty(TextBoxFormula.Text) && !_formulaHadErrors;
        }


        private void ButtonFormulaHelp_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var helpWindow = new JaceHelpWindow();
                helpWindow.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void StreamDeckJaceDecodeFormulaWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                _formLoaded = true;
                SetFormState();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(1001, ex);
            }
        }

        private void StreamDeckJaceDecodeFormulaWindow_OnClosing(object sender, CancelEventArgs e)
        {
            if (_changesMade)
            {
                if (MessageBox.Show("Discard changes?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
                {
                    e.Cancel = true;
                }
            }
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

        private void StreamDeckJaceDecodeFormulaWindow_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                e.Handled = true;
                Close();
            }
        }
    }
}
