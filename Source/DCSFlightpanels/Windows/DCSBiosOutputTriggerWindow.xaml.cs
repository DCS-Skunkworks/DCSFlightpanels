using DCS_BIOS.Json;

namespace DCSFlightpanels.Windows
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Controls.Primitives;
    using System.Windows.Input;
    using ClassLibraryCommon;
    using DCS_BIOS;
    using EnumEx = ClassLibraryCommon.EnumEx;

    /// <summary>
    /// Interaction logic for DCSBiosOutputTriggerWindow.xaml
    /// </summary>
    public partial class DCSBiosOutputTriggerWindow
    {
        private readonly bool _showCriteria;
        private readonly IEnumerable<DCSBIOSControl> _dcsbiosControls;
        private readonly string _description;
        private DCSBIOSOutput _dcsBiosOutput;
        private bool _formLoaded;
        private DCSBIOSControl _dcsbiosControl;
        private Popup _popupSearch;
        private DataGrid _dataGridValues;

        public DCSBiosOutputTriggerWindow(string description, bool showCriteria = true)
        {
            InitializeComponent();
            _showCriteria = showCriteria;
            _description = description;
            _dcsBiosOutput = new DCSBIOSOutput();
            DCSBIOSControlLocator.LoadControls();
            _dcsbiosControls = DCSBIOSControlLocator.GetIntegerOutputControls();
        }

        public DCSBiosOutputTriggerWindow(string description, DCSBIOSOutput dcsBiosOutput, bool showCriteria = true)
        {
            InitializeComponent();
            _showCriteria = showCriteria;
            _description = description;
            _dcsBiosOutput = dcsBiosOutput;
            DCSBIOSControlLocator.LoadControls();
            _dcsbiosControl = DCSBIOSControlLocator.GetControl(_dcsBiosOutput.ControlId);
            _dcsbiosControls = DCSBIOSControlLocator.GetIntegerOutputControls();

        }

        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            DarkMode.SetFrameworkElemenDarkMode(this);
            try
            {
                _popupSearch = (Popup)FindResource("PopUpSearchResults");
                _popupSearch.Height = 400;
                _dataGridValues = (DataGrid)LogicalTreeHelper.FindLogicalNode(_popupSearch, "DataGridValues");
                LabelDescription.Content = _description;
                ShowValues1();
                ShowValues2();
                _formLoaded = true;
                SetFormState();
                TextBoxSearchWord.Focus();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox( ex);
            }
        }

        private void ShowValues1()
        {
            ComboBoxComparisonCriteria.SelectedValue = _dcsBiosOutput.DCSBiosOutputComparison.GetEnumDescriptionField();
            if (_dcsBiosOutput.DCSBiosOutputType == DCSBiosOutputType.IntegerType)
            {
                TextBoxTriggerValue.Text = _dcsBiosOutput.SpecifiedValueInt.ToString(CultureInfo.InvariantCulture);
            }
            else
            {
                TextBoxTriggerValue.Text = _dcsBiosOutput.SpecifiedValueString;
            }
        }

        private void ShowValues2()
        {
            if (_dcsbiosControl != null)
            {
                TextBoxControlId.Text = _dcsbiosControl.Identifier;
                TextBoxControlDescription.Text = _dcsbiosControl.Description;
                TextBoxMaxValue.Text = _dcsbiosControl.Outputs[0].MaxValue.ToString();
                TextBoxOutputType.Text = _dcsbiosControl.Outputs[0].Type;
            }
        }

        private void SetFormState()
        {
            if (!_formLoaded)
            {
                return;
            }


            ComboBoxComparisonCriteria.IsEnabled = _dcsbiosControl != null && _dcsbiosControl.Outputs.Count == 1 && _dcsbiosControl.Outputs[0].OutputDataType == DCSBiosOutputType.IntegerType;

            LabelCriteria.Visibility = _showCriteria ? Visibility.Visible : Visibility.Collapsed;
            ComboBoxComparisonCriteria.Visibility = _showCriteria ? Visibility.Visible : Visibility.Collapsed;
            LabelTriggerValue.Visibility = _showCriteria ? Visibility.Visible : Visibility.Collapsed;
            TextBoxTriggerValue.Visibility = _showCriteria ? Visibility.Visible : Visibility.Collapsed;


            if (ComboBoxComparisonCriteria.IsEnabled && _dcsBiosOutput != null)
            {
                ComboBoxComparisonCriteria.SelectedValue = _dcsBiosOutput.DCSBiosOutputComparison.GetEnumDescriptionField();
            }
            if (TextBoxTriggerValue != null)
            {
                //todo
                //TextBoxTriggerValue.IsEnabled = _dcsBiosOutput != null;
            }
        }

        private void CopyValues()
        {
            try
            {
                //This is were DCSBiosOutput (subset of DCSBIOSControl) get populated from DCSBIOSControl
                if (string.IsNullOrEmpty(TextBoxTriggerValue.Text))
                {
                    throw new Exception("Value cannot be empty");
                }
                if (ComboBoxComparisonCriteria.SelectedValue == null)
                {
                    throw new Exception("Comparison criteria cannot be empty");
                }
                if (_dcsbiosControl == null)
                {
                    _dcsbiosControl = DCSBIOSControlLocator.GetControl(TextBoxControlId.Text);
                }
                _dcsBiosOutput.Consume(_dcsbiosControl);
                if (!_showCriteria)
                {
                    //Value isn't used anyways
                    _dcsBiosOutput.DCSBiosOutputComparison = DCSBiosOutputComparison.Equals;
                }
                else
                {
                    _dcsBiosOutput.DCSBiosOutputComparison = EnumEx.GetValueFromDescription<DCSBiosOutputComparison>(ComboBoxComparisonCriteria.SelectedValue.ToString());
                }
                try
                {
                    if (_showCriteria)
                    {
                        //Assume only Integer trigger values can be used. That is how it should be!!
                        try
                        {
                            var f = int.Parse(TextBoxTriggerValue.Text);
                        }
                        catch (Exception ex)
                        {
                            throw new Exception($"Error while checking Trigger value. Only integers are allowed : {ex.Message}");
                        }
                        if (_dcsBiosOutput.DCSBiosOutputType == DCSBiosOutputType.IntegerType)
                        {
                            _dcsBiosOutput.SpecifiedValueInt = (uint)Convert.ToInt32(TextBoxTriggerValue.Text);
                        }
                        else
                        {
                            throw new Exception($"Error, DCSBIOSOutput can only have a Integer type output. This has String : {_dcsBiosOutput.ControlId}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception($"Error while checking Value format : {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex, "Error in CopyValues()");
            }
        }

        private void ClearAll()
        {
            _dcsbiosControl = null;
            _dcsBiosOutput = null;
        }

        private void ButtonOkClick(object sender, RoutedEventArgs e)
        {
            try
            {
                CopyValues();
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox( ex);
            }
        }

        private void ButtonCancelClick(object sender, RoutedEventArgs e)
        {
            try
            {
                ClearAll();
                DialogResult = false;
                Close();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox( ex);
            }
        }

        public DCSBIOSOutput DCSBiosOutput
        {
            get { return _dcsBiosOutput; }
            set { _dcsBiosOutput = value; }
        }


        private void TextBoxSearchWord_OnKeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                TextBoxSearchCommon.AdjustShownPopupData(TextBoxSearchWord, _popupSearch, _dataGridValues, _dcsbiosControls);
                SetFormState();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox( ex);
            }
        }

        private void TextBoxSearchWord_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            TextBoxSearchCommon.SetBackgroundSearchBanner(TextBoxSearchWord);            
        }

        private void Control_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (_dataGridValues.SelectedItems.Count == 1)
                {
                    _dcsbiosControl = (DCSBIOSControl)_dataGridValues.SelectedItem;
                    _dcsBiosOutput = new DCSBIOSOutput();
                    _dcsBiosOutput.Consume(_dcsbiosControl);
                    ShowValues2();
                    if (_dcsBiosOutput.DCSBiosOutputType == DCSBiosOutputType.IntegerType)
                    {
                        TextBoxTriggerValue.Text = "0";
                    }
                    else if (_dcsBiosOutput.DCSBiosOutputType == DCSBiosOutputType.StringType)
                    {
                        TextBoxTriggerValue.Text = "<string>";
                    }
                }
                _popupSearch.IsOpen = false;
                SetFormState();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox( ex);
            }
        }

        private void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (_dataGridValues.SelectedItems.Count == 1)
                {
                    _dcsbiosControl = (DCSBIOSControl)_dataGridValues.SelectedItem;
                    _dcsBiosOutput = new DCSBIOSOutput();
                    _dcsBiosOutput.Consume(_dcsbiosControl);
                    ShowValues2();
                }
                SetFormState();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox( ex);
            }
        }


        private void UIElement_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (_dataGridValues.SelectedItems.Count == 1)
                {
                    _dcsbiosControl = (DCSBIOSControl)_dataGridValues.SelectedItem;
                    _dcsBiosOutput = new DCSBIOSOutput();
                    _dcsBiosOutput.Consume(_dcsbiosControl);
                    ShowValues2();
                    if (_dcsBiosOutput.DCSBiosOutputType == DCSBiosOutputType.IntegerType)
                    {
                        TextBoxTriggerValue.Text = "0";
                    }
                    else if (_dcsBiosOutput.DCSBiosOutputType == DCSBiosOutputType.StringType)
                    {
                        TextBoxTriggerValue.Text = "<string>";
                    }
                }
                _popupSearch.IsOpen = false;
                SetFormState();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox( ex);
            }
        }

        private void DCSBiosOutputTriggerWindow_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (!ButtonOk.IsEnabled && e.Key == Key.Escape)
            {
                DialogResult = false;
                e.Handled = true;
                Close();
            }
        }

        private void TextBoxSearchWord_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            TextBoxSearchCommon.HandleFirstSpace(sender, e);
        }
    }
}
