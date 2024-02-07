using DCS_BIOS.Json;

namespace DCSFlightpanels.Windows
{
    using ClassLibraryCommon;
    using DCS_BIOS.ControlLocator;
    using DCS_BIOS.EventArgs;
    using DCS_BIOS.Interfaces;
    using DCS_BIOS.misc;
    using DCS_BIOS.Serialized;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Threading;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Controls.Primitives;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Navigation;

    /// <summary>
    /// Interaction logic for JaceSandbox.xaml
    /// </summary>
    public partial class JaceSandboxWindow : IDcsBiosDataListener, IDisposable
    {
        private readonly AutoResetEvent _autoResetEvent = new(false);
        private readonly string _typeToSearch = "Type to search control";
        private readonly IEnumerable<DCSBIOSControl> _dcsbiosControls;
        private readonly JaceExtended _jaceExtended = new();
        private DCSBIOSOutput _dcsbiosOutput1;
        private DCSBIOSOutput _dcsbiosOutput2;
        private DCSBIOSOutput _dcsbiosOutput3;
        private DCSBIOSOutput _dcsbiosOutput4;
        private DCSBIOSOutput _dcsbiosOutput5;
        private volatile uint _value1 = uint.MaxValue;
        private volatile uint _value2 = uint.MaxValue;
        private volatile uint _value3 = uint.MaxValue;
        private volatile uint _value4 = uint.MaxValue;
        private volatile uint _value5 = uint.MaxValue;
        private bool _dataChanged;
        private bool _formLoaded;
        private bool _isLooping;
        private bool _exitThread;
        private Popup _popupSearch;
        private DataGrid _dataGridValues;
        private DCSBIOSControl _dcsbiosControl1;
        private DCSBIOSControl _dcsbiosControl2;
        private DCSBIOSControl _dcsbiosControl3;
        private DCSBIOSControl _dcsbiosControl4;
        private DCSBIOSControl _dcsbiosControl5;

        public JaceSandboxWindow()
        {
            BIOSEventHandler.AttachDataListener(this);
            _dcsbiosControls = DCSBIOSControlLocator.GetOutputControls(DCSBiosOutputType.IntegerType);
            var thread = new Thread(ThreadLoop);
            thread.Start();
            InitializeComponent();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // dispose managed resources
                _autoResetEvent?.Dispose();
                BIOSEventHandler.DetachDataListener(this);
            }
            // free native resources
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void ThreadLoop()
        {
            try
            {
                while (!_exitThread)
                {
                    _autoResetEvent.WaitOne();
                    string formula = null;

                    var variables = new Dictionary<string, double>();

                    if (_dcsbiosOutput1 != null)
                    {
                        variables.Add(_dcsbiosOutput1.ControlId, 0);
                    }
                    if (_dcsbiosOutput2 != null)
                    {
                        variables.Add(_dcsbiosOutput2.ControlId, 0);
                    }
                    if (_dcsbiosOutput3 != null)
                    {
                        variables.Add(_dcsbiosOutput3.ControlId, 0);
                    }
                    if (_dcsbiosOutput4 != null)
                    {
                        variables.Add(_dcsbiosOutput4.ControlId, 0);
                    }
                    if (_dcsbiosOutput5 != null)
                    {
                        variables.Add(_dcsbiosOutput5.ControlId, 0);
                    }
                    while (_isLooping)
                    {
                        if (_dataChanged)
                        {
                            try
                            {
                                Dispatcher?.BeginInvoke(
                                    (Action)delegate
                                    {
                                        LabelErrors.Content = string.Empty;
                                    });
                                Dispatcher?.Invoke(() =>
                                {
                                    formula = TextBoxFormula.Text;
                                });
                                if (_dcsbiosOutput1 != null)
                                {
                                    variables[_dcsbiosOutput1.ControlId] = GetVariableValues(_dcsbiosOutput1.ControlId);
                                }
                                if (_dcsbiosOutput2 != null)
                                {
                                    variables[_dcsbiosOutput2.ControlId] = GetVariableValues(_dcsbiosOutput2.ControlId);
                                }
                                if (_dcsbiosOutput3 != null)
                                {
                                    variables[_dcsbiosOutput3.ControlId] = GetVariableValues(_dcsbiosOutput3.ControlId);
                                }
                                if (_dcsbiosOutput4 != null)
                                {
                                    variables[_dcsbiosOutput4.ControlId] = GetVariableValues(_dcsbiosOutput4.ControlId);
                                }
                                if (_dcsbiosOutput5 != null)
                                {
                                    variables[_dcsbiosOutput5.ControlId] = GetVariableValues(_dcsbiosOutput5.ControlId);
                                }
                                var result = _jaceExtended.CalculationEngine.Calculate(formula, variables);

                                Dispatcher?.BeginInvoke(
                                    (Action)delegate
                                    {
                                        LabelResult.Content = "Result : " + result;
                                    });
                            }
                            catch (Exception ex)
                            {
                                Dispatcher?.BeginInvoke(
                                    (Action)delegate
                                    {
                                        LabelErrors.Content = ex.Message;
                                    });
                            }
                        }
                        Thread.Sleep(10);
                    }
                }
            }
            catch (Exception ex)
            {
                LabelErrors.Content = "Failed to start thread " + ex.Message;
            }
        }

        private double GetVariableValues(string controlId)
        {
            if (Equals(controlId, _dcsbiosOutput1.ControlId))
            {
                return _value1;
            }
            if (Equals(controlId, _dcsbiosOutput2.ControlId))
            {
                return _value2;
            }
            if (Equals(controlId, _dcsbiosOutput3.ControlId))
            {
                return _value3;
            }
            if (Equals(controlId, _dcsbiosOutput4.ControlId))
            {
                return _value4;
            }
            if (Equals(controlId, _dcsbiosOutput5.ControlId))
            {
                return _value5;
            }

            throw new Exception("Failed to pair DCSBIOSOutput " + controlId);
        }

        private void Control_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (_dataGridValues.SelectedItems.Count == 1)
                {
                    var dcsbiosControl = (DCSBIOSControl)_dataGridValues.SelectedItem;
                    var dcsBiosOutput = DCSBIOSControlLocator.GetUIntDCSBIOSOutput(dcsbiosControl.Identifier);
                    var textBox = (TextBox)_dataGridValues.Tag;
                    if (Equals(textBox, TextBoxSearch1))
                    {
                        _dcsbiosOutput1 = dcsBiosOutput;
                        _dcsbiosControl1 = dcsbiosControl;
                        TextBoxId1.Text = _dcsbiosControl1.Identifier;
                        TextBoxSearch1.Text = _typeToSearch;
                    }
                    if (Equals(textBox, TextBoxSearch2))
                    {
                        _dcsbiosOutput2 = dcsBiosOutput;
                        _dcsbiosControl2 = dcsbiosControl;
                        TextBoxId2.Text = _dcsbiosControl2.Identifier;
                        TextBoxSearch2.Text = _typeToSearch;
                    }
                    if (Equals(textBox, TextBoxSearch3))
                    {
                        _dcsbiosOutput3 = dcsBiosOutput;
                        _dcsbiosControl3 = dcsbiosControl;
                        TextBoxId3.Text = _dcsbiosControl3.Identifier;
                        TextBoxSearch3.Text = _typeToSearch;
                    }
                    if (Equals(textBox, TextBoxSearch4))
                    {
                        _dcsbiosOutput4 = dcsBiosOutput;
                        _dcsbiosControl4 = dcsbiosControl;
                        TextBoxId4.Text = _dcsbiosControl4.Identifier;
                        TextBoxSearch4.Text = _typeToSearch;
                    }
                    if (Equals(textBox, TextBoxSearch5))
                    {
                        _dcsbiosOutput5 = dcsBiosOutput;
                        _dcsbiosControl5 = dcsbiosControl;
                        TextBoxId5.Text = _dcsbiosControl5.Identifier;
                        TextBoxSearch5.Text = _typeToSearch;
                    }
                    SetFormState();
                }
                _popupSearch.IsOpen = false;
                SetFormState();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        public void DcsBiosDataReceived(object sender, DCSBIOSDataEventArgs e)
        {
            if (_dcsbiosOutput1?.UIntValueHasChanged(e.Address, e.Data) == true)
            {
                _value1 = _dcsbiosOutput1.GetUIntValue(e.Data);
                _dataChanged = true;
                Dispatcher?.BeginInvoke(
                    (Action)delegate
                    {
                        LabelSourceRawValue1.Content = "Value : " + _value1;
                    });
            }
            if (_dcsbiosOutput2?.UIntValueHasChanged(e.Address, e.Data) == true)
            {
                _value2 = _dcsbiosOutput2.GetUIntValue(e.Data);
                _dataChanged = true;
                Dispatcher?.BeginInvoke(
                    (Action)delegate
                    {
                        LabelSourceRawValue2.Content = "Value : " + _value2;
                    });
            }
            if (_dcsbiosOutput3?.UIntValueHasChanged(e.Address, e.Data) == true)
            {
                _value3 = _dcsbiosOutput3.GetUIntValue(e.Data);
                _dataChanged = true;
                Dispatcher?.BeginInvoke(
                    (Action)delegate
                    {
                        LabelSourceRawValue3.Content = "Value : " + _value3;
                    });
            }
            if (_dcsbiosOutput4?.UIntValueHasChanged(e.Address, e.Data) == true)
            {
                _value4 = _dcsbiosOutput4.GetUIntValue(e.Data);
                _dataChanged = true;
                Dispatcher?.BeginInvoke(
                    (Action)delegate
                    {
                        LabelSourceRawValue4.Content = "Value : " + _value4;
                    });
            }
            if (_dcsbiosOutput5?.UIntValueHasChanged(e.Address, e.Data) == true)
            {
                _value5 = _dcsbiosOutput5.GetUIntValue(e.Data);
                _dataChanged = true;
                Dispatcher?.BeginInvoke(
                    (Action)delegate
                    {
                        LabelSourceRawValue5.Content = "Value : " + _value5;
                    });
            }
        }

        private void ButtonTestFormula_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var formula = TextBoxFormula.Text;
                var variables = new Dictionary<string, double>();

                if (_dcsbiosOutput1 != null)
                {
                    variables.Add(_dcsbiosOutput1.ControlId, 0);
                }
                if (_dcsbiosOutput2 != null)
                {
                    variables.Add(_dcsbiosOutput2.ControlId, 0);
                }
                if (_dcsbiosOutput3 != null)
                {
                    variables.Add(_dcsbiosOutput3.ControlId, 0);
                }
                if (_dcsbiosOutput4 != null)
                {
                    variables.Add(_dcsbiosOutput4.ControlId, 0);
                }
                if (_dcsbiosOutput5 != null)
                {
                    variables.Add(_dcsbiosOutput5.ControlId, 0);
                }
                if (_dcsbiosOutput1 != null)
                {
                    variables[_dcsbiosOutput1.ControlId] = GetVariableValues(_dcsbiosOutput1.ControlId);
                }
                if (_dcsbiosOutput2 != null)
                {
                    variables[_dcsbiosOutput2.ControlId] = GetVariableValues(_dcsbiosOutput2.ControlId);
                }
                if (_dcsbiosOutput3 != null)
                {
                    variables[_dcsbiosOutput3.ControlId] = GetVariableValues(_dcsbiosOutput3.ControlId);
                }
                if (_dcsbiosOutput4 != null)
                {
                    variables[_dcsbiosOutput4.ControlId] = GetVariableValues(_dcsbiosOutput4.ControlId);
                }
                if (_dcsbiosOutput5 != null)
                {
                    variables[_dcsbiosOutput5.ControlId] = GetVariableValues(_dcsbiosOutput5.ControlId);
                }
                var result = _jaceExtended.CalculationEngine.Calculate(formula, variables);

                LabelErrors.Content = string.Empty;
                LabelResult.Content = "Result : " + result;
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
            if (!_formLoaded)
            {
                return;
            }
            ButtonTestFormula.IsEnabled = !string.IsNullOrEmpty(TextBoxFormula.Text) && !_isLooping;
            ButtonStartTest.IsEnabled = !string.IsNullOrEmpty(TextBoxFormula.Text) && !_isLooping;
            ButtonStopTest.IsEnabled = _isLooping;

            ButtonClear1.IsEnabled = !_isLooping && _dcsbiosOutput1 != null;
            ButtonClear2.IsEnabled = !_isLooping && _dcsbiosOutput2 != null;
            ButtonClear3.IsEnabled = !_isLooping && _dcsbiosOutput3 != null;
            ButtonClear4.IsEnabled = !_isLooping && _dcsbiosOutput4 != null;
            ButtonClear5.IsEnabled = !_isLooping && _dcsbiosOutput5 != null;

            TextBoxId1.IsEnabled = !_isLooping;
            TextBoxId2.IsEnabled = !_isLooping;
            TextBoxId3.IsEnabled = !_isLooping;
            TextBoxId4.IsEnabled = !_isLooping;
            TextBoxId5.IsEnabled = !_isLooping;
            TextBoxSearch1.IsEnabled = !_isLooping;
            TextBoxSearch2.IsEnabled = !_isLooping;
            TextBoxSearch3.IsEnabled = !_isLooping;
            TextBoxSearch4.IsEnabled = !_isLooping;
            TextBoxSearch5.IsEnabled = !_isLooping;
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

        private void JaceSandboxWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            DarkMode.SetFrameworkElementDarkMode(this);
            try
            {
                _popupSearch = (Popup)FindResource("PopUpSearchResults");
                _popupSearch.Height = 400;
                _dataGridValues = (DataGrid)LogicalTreeHelper.FindLogicalNode(_popupSearch, "DataGridValues");

                _formLoaded = true;
                SetFormState();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void TextBoxSearch_OnKeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                TextBoxSearchCommon.AdjustShownPopupData((TextBox)sender, _popupSearch, _dataGridValues, _dcsbiosControls);
                SetFormState();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void ButtonClear1_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                TextBoxId1.Text = string.Empty;
                TextBoxSearch1.Text = _typeToSearch;
                TextBoxSearch1.Foreground = new SolidColorBrush(Colors.Gainsboro);
                _dcsbiosOutput1 = null;
                _dcsbiosControl1 = null;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void ButtonClear2_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                TextBoxId2.Text = string.Empty;
                TextBoxSearch2.Text = _typeToSearch;
                TextBoxSearch2.Foreground = new SolidColorBrush(Colors.Gainsboro);
                _dcsbiosOutput2 = null;
                _dcsbiosControl2 = null;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void ButtonClear3_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                TextBoxId3.Text = string.Empty;
                TextBoxSearch3.Text = _typeToSearch;
                TextBoxSearch3.Foreground = new SolidColorBrush(Colors.Gainsboro);
                _dcsbiosOutput3 = null;
                _dcsbiosControl3 = null;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void ButtonClear4_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                TextBoxId4.Text = string.Empty;
                TextBoxSearch4.Text = _typeToSearch;
                TextBoxSearch4.Foreground = new SolidColorBrush(Colors.Gainsboro);
                _dcsbiosOutput4 = null;
                _dcsbiosControl4 = null;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void ButtonClear5_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                TextBoxId5.Text = string.Empty;
                TextBoxSearch5.Text = _typeToSearch;
                TextBoxSearch5.Foreground = new SolidColorBrush(Colors.Gainsboro);
                _dcsbiosOutput5 = null;
                _dcsbiosControl5 = null;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void ButtonStartTest_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                _isLooping = true;
                _autoResetEvent.Set();
                SetFormState();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void ButtonStopTest_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                _isLooping = false;
                SetFormState();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void JaceSandboxWindow_OnClosing(object sender, CancelEventArgs e)
        {
            _exitThread = true;
            _isLooping = false;
            _autoResetEvent.Set();
        }

        private void TextBoxSearch_OnGotFocus(object sender, RoutedEventArgs e)
        {
            var textbox = (TextBox)sender;
            textbox.Text = string.Empty;
            textbox.Foreground = new SolidColorBrush(Colors.Black);
        }

        private void TextBoxSearch_OnLostFocus(object sender, RoutedEventArgs e)
        {
            var textbox = (TextBox)sender;
            textbox.Text = _typeToSearch;
            textbox.Foreground = new SolidColorBrush(Colors.Gainsboro);
        }

        private void ButtonClose_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Hyperlink_OnRequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = e.Uri.AbsoluteUri,
                    UseShellExecute = true
                });

                e.Handled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void JaceSandboxWindow_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                e.Handled = true;
                Close();
            }
        }

        private void LabelInsert_OnMouseEnter(object sender, MouseEventArgs e)
        {
            try
            {
                Mouse.OverrideCursor = Cursors.Hand;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void LabelInsert_OnMouseLeave(object sender, MouseEventArgs e)
        {
            try
            {
                Mouse.OverrideCursor = Cursors.Arrow;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void LabelInsert_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                TextBox textBox = null;
                switch (((Label)sender).Name)
                {
                    case "LabelInsert1":
                        {
                            textBox = TextBoxId1;
                            break;
                        }
                    case "LabelInsert2":
                        {
                            textBox = TextBoxId2;
                            break;
                        }
                    case "LabelInsert3":
                        {
                            textBox = TextBoxId3;
                            break;
                        }
                    case "LabelInsert4":
                        {
                            textBox = TextBoxId4;
                            break;
                        }
                    case "LabelInsert5":
                        {
                            textBox = TextBoxId5;
                            break;
                        }
                }

                if (textBox == null)
                {
                    return;
                }

                TextBoxFormula.Text = textBox.Text + " " + TextBoxFormula.Text;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void TextBoxSearch1_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            TextBoxSearchCommon.HandleFirstSpace(sender, e);
        }

        private void TextBoxSearch2_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            TextBoxSearchCommon.HandleFirstSpace(sender, e);
        }

        private void TextBoxSearch3_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            TextBoxSearchCommon.HandleFirstSpace(sender, e);
        }

        private void TextBoxSearch4_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            TextBoxSearchCommon.HandleFirstSpace(sender, e);
        }

        private void TextBoxSearch5_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            TextBoxSearchCommon.HandleFirstSpace(sender, e);
        }
    }
}
