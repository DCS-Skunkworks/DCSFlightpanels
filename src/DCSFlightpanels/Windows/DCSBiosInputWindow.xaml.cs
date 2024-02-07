using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using ClassLibraryCommon;
using DCS_BIOS.ControlLocator;
using DCS_BIOS.Json;
using DCS_BIOS.Serialized;

namespace DCSFlightpanels.Windows
{
    /// <summary>
    /// Interaction logic for DCSBiosInputWindow.xaml
    /// </summary>
    public partial class DCSBiosInputWindow
    {
        private DCSBIOSInput _dcsBiosInput;
        private readonly string _description;
        private bool _formLoaded;
        private DCSBIOSControl _dcsbiosControl;
        private readonly IEnumerable<DCSBIOSControl> _dcsbiosControls;
        private Popup _popupSearch;
        private DataGrid _dataGridValues;

        /*public DCSBiosInputWindow()
        {
            InitializeComponent();
            DCSBIOSControlLocator.LoadControls();
            _dcsbiosControls = DCSBIOSControlLocator.GetInputControls();
            foreach (var dcsbiosControl in _dcsbiosControls)
            {
                Debug.Print(dcsbiosControl.identifier);
            }
        }*/

        public DCSBiosInputWindow(string description)
        {
            InitializeComponent();
            _description = description;
            _dcsbiosControls = DCSBIOSControlLocator.GetInputControls();
        }

        public DCSBiosInputWindow(string description, DCSBIOSInput dcsBiosInput)
        {
            InitializeComponent();
            _description = description;
            _dcsBiosInput = dcsBiosInput;
            _dcsbiosControl = DCSBIOSControlLocator.GetControl(_dcsBiosInput.ControlId);
            _dcsbiosControls = DCSBIOSControlLocator.GetInputControls();
            PopulateComboBoxInterfaceType(_dcsBiosInput);
        }

        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            DarkMode.SetFrameworkElementDarkMode(this);
            try
            {
                LabelProfileDescription.Content = ProfileHandler.ActiveDCSAircraft.Description;
                _popupSearch = (Popup)FindResource("PopUpSearchResults");
                _popupSearch.Height = 400;
                _dataGridValues = (DataGrid)LogicalTreeHelper.FindLogicalNode(_popupSearch, "DataGridValues");
                LabelDescription.Content = _description;
                ShowRightSideValues();
                ShowValuesLeftSideValues();
                _formLoaded = true;
                SetFormState();
                TextBoxSearchWord.Focus();
                ComboBoxInterfaceType.SelectionChanged += ComboBoxInterfaceType_OnSelectionChanged;
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void SetVisibility(DCSBIOSInputType dcsbiosInputType)
        {
            LabelInputValue.Visibility = Visibility.Visible;
            switch (dcsbiosInputType)
            {
                case DCSBIOSInputType.FIXED_STEP:
                    {
                        //INC / DEC
                        ComboBoxInputValueFixedStep.Visibility = Visibility.Visible;
                        ComboBoxInputValueAction.Visibility = Visibility.Collapsed;
                        TextBoxInputValue.Visibility = Visibility.Collapsed;

                        StackPanelMaxValue.Visibility = Visibility.Collapsed;
                        break;
                    }
                case DCSBIOSInputType.ACTION:
                    {
                        //TOGGLE
                        ComboBoxInputValueFixedStep.Visibility = Visibility.Collapsed;
                        ComboBoxInputValueAction.Visibility = Visibility.Visible;
                        TextBoxInputValue.Visibility = Visibility.Collapsed;

                        StackPanelMaxValue.Visibility = Visibility.Collapsed;
                        break;
                    }
                case DCSBIOSInputType.SET_STATE:
                    {
                        //INTEGER
                        ComboBoxInputValueFixedStep.Visibility = Visibility.Collapsed;
                        ComboBoxInputValueAction.Visibility = Visibility.Collapsed;
                        TextBoxInputValue.Visibility = Visibility.Visible;

                        StackPanelMaxValue.Visibility = Visibility.Visible;
                        break;
                    }
                case DCSBIOSInputType.VARIABLE_STEP:
                    {
                        //INTEGER
                        ComboBoxInputValueFixedStep.Visibility = Visibility.Collapsed;
                        ComboBoxInputValueAction.Visibility = Visibility.Collapsed;
                        TextBoxInputValue.Visibility = Visibility.Visible;

                        StackPanelMaxValue.Visibility = Visibility.Visible;
                        break;
                    }
                case DCSBIOSInputType.SET_STRING:
                    {
                        //STRING
                        ComboBoxInputValueFixedStep.Visibility = Visibility.Collapsed;
                        ComboBoxInputValueAction.Visibility = Visibility.Collapsed;
                        TextBoxInputValue.Visibility = Visibility.Visible;

                        StackPanelMaxValue.Visibility = Visibility.Collapsed;
                        break;
                    }
            }
        }

        private void ShowRightSideValues()
        {
            if (_dcsBiosInput == null)
            {
                return;
            }
            ComboBoxInterfaceType.SelectedValue = _dcsBiosInput.SelectedDCSBIOSInterface.Interface;
            ComboBoxDelay.SelectedValue = _dcsBiosInput.SelectedDCSBIOSInterface.Delay;
            SetVisibility(_dcsBiosInput.SelectedDCSBIOSInterface.Interface);
            switch (_dcsBiosInput.SelectedDCSBIOSInterface.Interface)
            {
                case DCSBIOSInputType.FIXED_STEP:
                    {
                        //INC / DEC
                        ComboBoxInputValueFixedStep.SelectedValue = _dcsBiosInput.SelectedDCSBIOSInterface.SpecifiedFixedStepArgument;
                        break;
                    }
                case DCSBIOSInputType.ACTION:
                    {
                        //TOGGLE
                        ComboBoxInputValueAction.SelectedValue = _dcsBiosInput.SelectedDCSBIOSInterface.SpecifiedActionArgument;
                        break;
                    }
                case DCSBIOSInputType.SET_STATE:
                    {
                        //INTEGER
                        TextBoxInputValue.Text = _dcsBiosInput.SelectedDCSBIOSInterface.SpecifiedSetStateArgument.ToString();
                        break;
                    }
                case DCSBIOSInputType.VARIABLE_STEP:
                    {
                        //INTEGER
                        TextBoxInputValue.Text = _dcsBiosInput.SelectedDCSBIOSInterface.SpecifiedVariableStepArgument.ToString();
                        break;
                    }
                case DCSBIOSInputType.SET_STRING:
                    {
                        //STRING
                        TextBoxInputValue.Text = _dcsBiosInput.SelectedDCSBIOSInterface.SpecifiedSetStringArgument;
                        break;
                    }
            }
        }

        private void ShowValuesLeftSideValues()
        {
            if (_dcsbiosControl != null)
            {
                TextBoxControlId.Text = _dcsbiosControl.Identifier;
                TextBoxControlDescription.Text = _dcsbiosControl.Description;
            }

            if (_dcsBiosInput?.SelectedDCSBIOSInterface != null)
            {
                TextBoxInputTypeDescription.Text = _dcsBiosInput.SelectedDCSBIOSInterface.Description;
                if (_dcsBiosInput.SelectedDCSBIOSInterface.Interface == DCSBIOSInputType.SET_STATE || _dcsBiosInput.SelectedDCSBIOSInterface.Interface == DCSBIOSInputType.VARIABLE_STEP)
                {
                    TextBoxMaxValue.Text = _dcsBiosInput.GetMaxValueForInterface(_dcsBiosInput.SelectedDCSBIOSInterface.Interface).ToString();
                }
                else
                {
                    TextBoxMaxValue.Text = string.Empty;
                }
            }
        }

        private void SetFormState()
        {
            if (!_formLoaded)
            {
                return;
            }
            ButtonOk.IsEnabled = _dcsbiosControl != null;
            if (_dcsBiosInput == null)
            {
                LabelInterfaceType.Visibility = Visibility.Collapsed;
                ComboBoxInterfaceType.Visibility = Visibility.Collapsed;
                LabelInputValue.Visibility = Visibility.Collapsed;
                ComboBoxInputValueAction.Visibility = Visibility.Collapsed;
                ComboBoxInputValueFixedStep.Visibility = Visibility.Collapsed;
                TextBoxInputValue.Visibility = Visibility.Collapsed;
                StackPanelMaxValue.Visibility = Visibility.Collapsed;
            }
            else if (_dcsBiosInput is { SelectedDCSBIOSInterface: null })
            {
                LabelInterfaceType.Visibility = Visibility.Visible;
                ComboBoxInterfaceType.Visibility = Visibility.Visible;
                SetVisibility((DCSBIOSInputType)Enum.Parse(typeof(DCSBIOSInputType), GetComboBoxSelectedInterface()));
            }
            else if (_dcsBiosInput?.SelectedDCSBIOSInterface != null)
            {
                LabelInterfaceType.Visibility = Visibility.Visible;
                ComboBoxInterfaceType.Visibility = Visibility.Visible;
                LabelInputValue.Visibility = Visibility.Visible;
                SetVisibility(_dcsBiosInput.SelectedDCSBIOSInterface.Interface);
            }
        }

        private void CopyValues()
        {
            try
            {
                /*
                 * fixed_step = <INC/DEC>
                 * set_state = <integer>
                 * action = TOGGLE
                 * variable_step = <new_value>|-<decrease_by>|+<increase_by>
                 */
                _dcsBiosInput.SetSelectedInterface(GetChosenInterfaceType());
                _dcsBiosInput.SelectedDCSBIOSInterface.Delay = int.Parse(ComboBoxDelay.SelectedValue.ToString());
                _dcsBiosInput.Delay = int.Parse(ComboBoxDelay.SelectedValue.ToString());
                switch (_dcsBiosInput.SelectedDCSBIOSInterface.Interface)
                {
                    case DCSBIOSInputType.ACTION:
                        {
                            _dcsBiosInput.SelectedDCSBIOSInterface.SpecifiedActionArgument = ComboBoxInputValueAction.SelectedValue.ToString();
                            break;
                        }
                    case DCSBIOSInputType.SET_STATE:
                        {
                            uint tmp;
                            try
                            {
                                tmp = uint.Parse(TextBoxInputValue.Text);
                            }
                            catch (Exception)
                            {
                                var dcsbiosInputString = string.Empty;
                                if (_dcsBiosInput != null)
                                {
                                    dcsbiosInputString = _dcsBiosInput.ControlId + " / " + _dcsBiosInput.SelectedDCSBIOSInterface.Interface;
                                }
                                throw new Exception("Please enter a valid value (positive whole number). Value found : [" + TextBoxInputValue.Text + "]" + Environment.NewLine + " DCS-BIOS Input is " + dcsbiosInputString);
                            }
                            if (tmp > _dcsBiosInput.SelectedDCSBIOSInterface.MaxValue)
                            {
                                throw new Exception("Input value must be between 0 - " + _dcsBiosInput.SelectedDCSBIOSInterface.MaxValue);
                            }
                            _dcsBiosInput.SelectedDCSBIOSInterface.SpecifiedSetStateArgument = tmp;

                            break;
                        }
                    case DCSBIOSInputType.VARIABLE_STEP:
                        {
                            int tmp;
                            try
                            {
                                tmp = int.Parse(TextBoxInputValue.Text);
                            }
                            catch (Exception)
                            {
                                var dcsbiosInputString = string.Empty;
                                if (_dcsBiosInput != null)
                                {
                                    dcsbiosInputString = _dcsBiosInput.ControlId + " / " + _dcsBiosInput.SelectedDCSBIOSInterface.Interface;
                                }
                                throw new Exception("Please enter a valid value (whole number). Value found : [" + TextBoxInputValue.Text + "]" + Environment.NewLine + " DCS-BIOS Input is " + dcsbiosInputString);
                            }
                            if (tmp > _dcsBiosInput.SelectedDCSBIOSInterface.MaxValue)
                            {
                                throw new Exception("Input value must be between 0 - " + _dcsBiosInput.SelectedDCSBIOSInterface.MaxValue);
                            }
                            _dcsBiosInput.SelectedDCSBIOSInterface.SpecifiedVariableStepArgument = tmp;
                            break;
                        }
                    case DCSBIOSInputType.FIXED_STEP:
                        {
                            _dcsBiosInput.SelectedDCSBIOSInterface.SpecifiedFixedStepArgument = (DCSBIOSFixedStepInput)Enum.Parse(typeof(DCSBIOSFixedStepInput), ComboBoxInputValueFixedStep.SelectedValue.ToString());
                            break;
                        }
                    case DCSBIOSInputType.SET_STRING:
                        {
                            if (string.IsNullOrEmpty(TextBoxInputValue.Text))
                            {
                                var dcsbiosInputString = string.Empty;
                                if (_dcsBiosInput != null)
                                {
                                    dcsbiosInputString = _dcsBiosInput.ControlId + " / " + _dcsBiosInput.SelectedDCSBIOSInterface.Interface;
                                }
                                throw new Exception("Please enter a string value. DCS-BIOS Input is " + dcsbiosInputString);
                            }
                            _dcsBiosInput.SelectedDCSBIOSInterface.SpecifiedSetStringArgument = TextBoxInputValue.Text;

                            break;
                        }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"1003351 Error in CopyValues() : {ex.Message}");
            }
        }

        private void ClearAll()
        {
            _dcsbiosControl = null;
            _dcsBiosInput = null;
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
                Common.ShowErrorMessageBox(ex);
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
                Common.ShowErrorMessageBox(ex);
            }
        }

        public DCSBIOSInput DCSBiosInput
        {
            get { return _dcsBiosInput; }
            set { _dcsBiosInput = value; }
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
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void TextBoxSearchWord_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            TextBoxSearchCommon.SetBackgroundSearchBanner(TextBoxSearchWord);
        }

        private void DCSBIOSInputSelected(bool keepSearchOpen)
        {
            try
            {
                if (_dataGridValues.SelectedItems.Count == 1)
                {
                    _dcsbiosControl = (DCSBIOSControl)_dataGridValues.SelectedItem;
                    _dcsBiosInput = new DCSBIOSInput();
                    _dcsBiosInput.Consume(_dcsbiosControl);
                    PopulateComboBoxInterfaceType(_dcsBiosInput);
                    //_dcsBiosInput.SetSelectedInterface(GetChosenInterfaceType());
                    ShowValuesLeftSideValues();
                    SetFormState();
                }
                _popupSearch.IsOpen = keepSearchOpen;
                SetFormState();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void DataGridValues_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            DCSBIOSInputSelected(true);
        }

        private void DataGridValues_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DCSBIOSInputSelected(true);
        }


        private void DataGridValues_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            DCSBIOSInputSelected(false);
        }

        private void PopulateComboBoxInterfaceType(DCSBIOSInput dcsbiosInput)
        {
            try
            {
                ComboBoxInterfaceType.SelectionChanged -= ComboBoxInterfaceType_OnSelectionChanged;
                ComboBoxInterfaceType.Items.Clear();
                foreach (var dcsbiosInputInterface in dcsbiosInput.DCSBIOSInputInterfaces)
                {
                    var comboBoxItem = new ComboBoxItem
                    {
                        Content = dcsbiosInputInterface.Interface.ToString()
                    };
                    ComboBoxInterfaceType.Items.Add(comboBoxItem);
                }
                ComboBoxInterfaceType.SelectedValue = dcsbiosInput.SelectedDCSBIOSInterface != null ? 
                    dcsbiosInput.SelectedDCSBIOSInterface.Interface.ToString() 
                    : dcsbiosInput.DCSBIOSInputInterfaces[0].Interface.ToString();
            }
            finally
            {
                ComboBoxInterfaceType.SelectionChanged += ComboBoxInterfaceType_OnSelectionChanged;
            }
        }

        private string GetComboBoxSelectedInterface()
        {
            return ComboBoxInterfaceType.SelectedValue.ToString();
        }

        private DCSBIOSInputType GetChosenInterfaceType()
        {
            var result = (DCSBIOSInputType)Enum.Parse(typeof(DCSBIOSInputType), GetComboBoxSelectedInterface());
            return result;
        }

        private void ComboBoxInterfaceType_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (ComboBoxInterfaceType.SelectedValue == null)
                {
                    return;
                }

                if (_dcsBiosInput == null)
                {
                    return;
                }

                var inputType = (DCSBIOSInputType)Enum.Parse(typeof(DCSBIOSInputType), GetComboBoxSelectedInterface());
                _dcsBiosInput.SetSelectedInterface(inputType);
                TextBoxInputTypeDescription.Text = _dcsBiosInput.SelectedDCSBIOSInterface.Description;
                if (_dcsBiosInput.SelectedDCSBIOSInterface.Interface == DCSBIOSInputType.SET_STATE || _dcsBiosInput.SelectedDCSBIOSInterface.Interface == DCSBIOSInputType.VARIABLE_STEP)
                {
                    TextBoxMaxValue.Text = _dcsBiosInput.SelectedDCSBIOSInterface.MaxValue.ToString();
                }
                SetFormState();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void DCSBiosInputWindow_OnKeyDown(object sender, KeyEventArgs e)
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