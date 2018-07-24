using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;
using ClassLibraryCommon;
using DCS_BIOS;

namespace DCSFlightpanels
{
    /// <summary>
    /// Interaction logic for JaceSandbox.xaml
    /// </summary>
    public partial class JaceSandboxWindow : Window, IDcsBiosDataListener, IDisposable
    {

        private DCSBIOSOutput _dcsbiosOutput1 = null;
        private DCSBIOSOutput _dcsbiosOutput2 = null;
        private DCSBIOSOutput _dcsbiosOutput3 = null;
        private DCSBIOSOutput _dcsbiosOutput4 = null;
        private DCSBIOSOutput _dcsbiosOutput5 = null;
        private volatile uint _value1 = 0;
        private volatile uint _value2 = 0;
        private volatile uint _value3 = 0;
        private volatile uint _value4 = 0;
        private volatile uint _value5 = 0;
        private bool _dataChanged;
        private bool _formLoaded;
        private bool _isLooping;
        private bool _exitThread;
        private readonly AutoResetEvent _autoResetEvent = new AutoResetEvent(false);
        private readonly string _typeToSearch = "Type to search control";
        private readonly DCSBIOS _dcsbios;
        private Popup _popupSearch;
        private DataGrid _dataGridValues;
        private readonly IEnumerable<DCSBIOSControl> _dcsbiosControls;
        private DCSBIOSControl _dcsbiosControl1;
        private DCSBIOSControl _dcsbiosControl2;
        private DCSBIOSControl _dcsbiosControl3;
        private DCSBIOSControl _dcsbiosControl4;
        private DCSBIOSControl _dcsbiosControl5;
        private readonly JaceExtended _jaceExtended = new JaceExtended();
        private Dictionary<string, double> _variables = new Dictionary<string, double>();

        public JaceSandboxWindow(DCSBIOS dcsbios)
        {
            _dcsbios = dcsbios;
            _dcsbios.AttachDataReceivedListener(this);
            DCSBIOSControlLocator.LoadControls();
            _dcsbiosControls = DCSBIOSControlLocator.GetIntegerOutputControls();
            var thread = new Thread(ThreadLoop);
            thread.Start();
            InitializeComponent();
        }

        ~JaceSandboxWindow()
        {
            // Finalizer calls Dispose(false)  
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // dispose managed resources
                _autoResetEvent?.Close();
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
                                Dispatcher.BeginInvoke(
                                    (Action)delegate
                                    {
                                        LabelErrors.Content = "";
                                    });
                                Dispatcher.Invoke(() =>
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

                                Dispatcher.BeginInvoke(
                                    (Action)delegate
                                    {
                                        LabelResult.Content = "Result : " + result;
                                    });
                            }
                            catch (Exception e)
                            {
                                Dispatcher.BeginInvoke(
                                    (Action)delegate
                                    {
                                        LabelErrors.Content = e.Message;
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
                    var dcsBiosOutput = DCSBIOSControlLocator.GetDCSBIOSOutput(dcsbiosControl.identifier);
                    var textBox = (TextBox)_dataGridValues.Tag;
                    if (Equals(textBox, TextBoxSearch1))
                    {
                        _dcsbiosOutput1 = dcsBiosOutput;
                        _dcsbiosControl1 = dcsbiosControl;
                        TextBoxId1.Text = _dcsbiosControl1.identifier;
                        TextBoxSearch1.Text = _typeToSearch;
                    }
                    if (Equals(textBox, TextBoxSearch2))
                    {
                        _dcsbiosOutput2 = dcsBiosOutput;
                        _dcsbiosControl2 = dcsbiosControl;
                        TextBoxId2.Text = _dcsbiosControl2.identifier;
                        TextBoxSearch2.Text = _typeToSearch;
                    }
                    if (Equals(textBox, TextBoxSearch3))
                    {
                        _dcsbiosOutput3 = dcsBiosOutput;
                        _dcsbiosControl3 = dcsbiosControl;
                        TextBoxId3.Text = _dcsbiosControl3.identifier;
                        TextBoxSearch3.Text = _typeToSearch;
                    }
                    if (Equals(textBox, TextBoxSearch4))
                    {
                        _dcsbiosOutput4 = dcsBiosOutput;
                        _dcsbiosControl4 = dcsbiosControl;
                        TextBoxId4.Text = _dcsbiosControl4.identifier;
                        TextBoxSearch4.Text = _typeToSearch;
                    }
                    if (Equals(textBox, TextBoxSearch5))
                    {
                        _dcsbiosOutput5 = dcsBiosOutput;
                        _dcsbiosControl5 = dcsbiosControl;
                        TextBoxId5.Text = _dcsbiosControl5.identifier;
                        TextBoxSearch5.Text = _typeToSearch;
                    }
                    SetFormState();
                }
                _popupSearch.IsOpen = false;
                SetFormState();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(1006, ex);
            }
        }

        public void DcsBiosDataReceived(object sender, DCSBIOSDataEventArgs e)
        {
            if (_dcsbiosOutput1?.Address == e.Address)
            {
                if (!Equals(_value1, e.Data))
                {
                    _value1 = e.Data;
                    _dataChanged = true;
                    Dispatcher.BeginInvoke(
                        (Action)delegate
                        {
                            LabelSourceRawValue1.Content = "Value : " + _value1;
                        });
                }
            }
            if (_dcsbiosOutput2?.Address == e.Address)
            {
                if (!Equals(_value2, e.Data))
                {
                    _value2 = e.Data;
                    _dataChanged = true;
                    Dispatcher.BeginInvoke(
                        (Action)delegate
                        {
                            LabelSourceRawValue2.Content = "Value : " + _value2;
                        });
                }
            }
            if (_dcsbiosOutput3?.Address == e.Address)
            {
                if (!Equals(_value3, e.Data))
                {
                    _value3 = e.Data;
                    _dataChanged = true;
                    Dispatcher.BeginInvoke(
                        (Action)delegate
                        {
                            LabelSourceRawValue3.Content = "Value : " + _value3;
                        });
                }
            }
            if (_dcsbiosOutput4?.Address == e.Address)
            {
                if (!Equals(_value4, e.Data))
                {
                    _value4 = e.Data;
                    _dataChanged = true;
                    Dispatcher.BeginInvoke(
                        (Action)delegate
                        {
                            LabelSourceRawValue4.Content = "Value : " + _value4;
                        });
                }
            }
            if (_dcsbiosOutput5?.Address == e.Address)
            {
                if (!Equals(_value5, e.Data))
                {
                    _value5 = e.Data;
                    _dataChanged = true;
                    Dispatcher.BeginInvoke(
                        (Action)delegate
                        {
                            LabelSourceRawValue5.Content = "Value : " + _value5;
                        });
                }
            }
        }

        private void ButtonTestFormula_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                string formula = TextBoxFormula.Text;
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

                LabelErrors.Content = "";
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
            ButtonTestFormula.IsEnabled = !String.IsNullOrEmpty(TextBoxFormula.Text) && !_isLooping;
            ButtonStartTest.IsEnabled = !String.IsNullOrEmpty(TextBoxFormula.Text) && !_isLooping;
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
            try
            {
                _popupSearch = (Popup)FindResource("PopUpSearchResults");
                _popupSearch.Height = 400;
                _dataGridValues = ((DataGrid)LogicalTreeHelper.FindLogicalNode(_popupSearch, "DataGridValues"));

                _formLoaded = true;
                SetFormState();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(1001, ex);
            }
        }

        private void AdjustShownPopupData(TextBox textBox)
        {
            _popupSearch.PlacementTarget = textBox;
            _popupSearch.Placement = PlacementMode.Bottom;
            _dataGridValues.Tag = textBox;
            if (!_popupSearch.IsOpen)
            {
                _popupSearch.IsOpen = true;
            }
            if (_dataGridValues != null)
            {
                if (string.IsNullOrEmpty(textBox.Text))
                {
                    _dataGridValues.DataContext = _dcsbiosControls;
                    _dataGridValues.ItemsSource = _dcsbiosControls;
                    _dataGridValues.Items.Refresh();
                    return;
                }
                var subList = _dcsbiosControls.Where(controlObject => (!string.IsNullOrWhiteSpace(controlObject.identifier) && controlObject.identifier.ToUpper().Contains(textBox.Text.ToUpper()))
                                                                      || (!string.IsNullOrWhiteSpace(controlObject.description) && controlObject.description.ToUpper().Contains(textBox.Text.ToUpper())));
                _dataGridValues.DataContext = subList;
                _dataGridValues.ItemsSource = subList;
                _dataGridValues.Items.Refresh();
            }
        }

        private void TextBoxSearch_OnKeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                AdjustShownPopupData((TextBox)sender);
                SetFormState();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(1005, ex);
            }
        }

        private void ButtonClear1_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                TextBoxId1.Text = "";
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
                TextBoxId2.Text = "";
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
                TextBoxId3.Text = "";
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
                TextBoxId4.Text = "";
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
                TextBoxId5.Text = "";
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
            _dcsbios.DetachDataReceivedListener(this);
            _exitThread = true;
            _isLooping = false;
            _autoResetEvent.Set();
        }

        private void TextBoxSearch_OnGotFocus(object sender, RoutedEventArgs e)
        {
            var textbox = (TextBox)sender;
            textbox.Text = "";
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
