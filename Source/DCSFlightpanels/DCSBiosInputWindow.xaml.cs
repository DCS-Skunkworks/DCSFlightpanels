﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using ClassLibraryCommon;
using DCS_BIOS;

namespace DCSFlightpanels
{
    /// <summary>
    /// Interaction logic for DCSBiosInputWindow.xaml
    /// </summary>
    public partial class DCSBiosInputWindow : Window
    {
        private DCSBIOSInput _dcsBiosInput;
        private string _description;
        private bool _formLoaded;
        private DCSBIOSControl _dcsbiosControl;
        private DCSAirframe _dcsAirframe;
        private IEnumerable<DCSBIOSControl> _dcsbiosControls;
        private Popup _popupSearch;
        private DataGrid _dataGridValues;

        public DCSBiosInputWindow()
        {
            InitializeComponent();
            DCSBIOSControlLocator.LoadControls();
            _dcsbiosControls = DCSBIOSControlLocator.GetInputControls();
            foreach (var dcsbiosControl in _dcsbiosControls)
            {
                Debug.Print(dcsbiosControl.identifier);
            }
        }

        public DCSBiosInputWindow(DCSAirframe dcsAirframe, string description)
        {
            InitializeComponent();
            _dcsAirframe = dcsAirframe;
            _description = description;
            _dcsbiosControls = DCSBIOSControlLocator.GetInputControls();
        }

        public DCSBiosInputWindow(DCSAirframe dcsAirframe, string description, DCSBIOSInput dcsBiosInput)
        {
            InitializeComponent();
            _dcsAirframe = dcsAirframe;
            _description = description;
            _dcsBiosInput = dcsBiosInput;
            _dcsbiosControl = DCSBIOSControlLocator.GetControl(_dcsBiosInput.ControlId);
            _dcsbiosControls = DCSBIOSControlLocator.GetInputControls();
            PopulateComboBoxInterfaceType(_dcsBiosInput);
        }

        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                _popupSearch = (Popup)FindResource("PopUpSearchResults");
                _popupSearch.Height = 400;
                _dataGridValues = ((DataGrid)LogicalTreeHelper.FindLogicalNode(_popupSearch, "DataGridValues"));
                LabelDescription.Content = _description;
                ShowValues1();
                ShowValues2();
                _formLoaded = true;
                SetFormState();
                TextBoxSearchWord.Focus();
                ComboBoxInterfaceType.SelectionChanged += ComboBoxInterfaceType_OnSelectionChanged;
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(1001, ex);
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
                        TextBoxInputValueSetState.Visibility = Visibility.Collapsed;

                        LabelMaxValue.Visibility = Visibility.Collapsed;
                        TextBoxMaxValue.Visibility = Visibility.Collapsed;
                        break;
                    }
                case DCSBIOSInputType.ACTION:
                    {
                        //TOGGLE
                        ComboBoxInputValueFixedStep.Visibility = Visibility.Collapsed;
                        ComboBoxInputValueAction.Visibility = Visibility.Visible;
                        TextBoxInputValueSetState.Visibility = Visibility.Collapsed;

                        LabelMaxValue.Visibility = Visibility.Collapsed;
                        TextBoxMaxValue.Visibility = Visibility.Collapsed;
                        break;
                    }
                case DCSBIOSInputType.SET_STATE:
                    {
                        //INTEGER
                        ComboBoxInputValueFixedStep.Visibility = Visibility.Collapsed;
                        ComboBoxInputValueAction.Visibility = Visibility.Collapsed;
                        TextBoxInputValueSetState.Visibility = Visibility.Visible;

                        LabelMaxValue.Visibility = Visibility.Visible;
                        TextBoxMaxValue.Visibility = Visibility.Visible;
                        break;
                    }
                case DCSBIOSInputType.VARIABLE_STEP:
                    {
                        //INTEGER
                        ComboBoxInputValueFixedStep.Visibility = Visibility.Collapsed;
                        ComboBoxInputValueAction.Visibility = Visibility.Collapsed;
                        TextBoxInputValueSetState.Visibility = Visibility.Visible;

                        LabelMaxValue.Visibility = Visibility.Visible;
                        TextBoxMaxValue.Visibility = Visibility.Visible;
                        break;
                    }
            }
        }

        private void ShowValues1()
        {
            if (_dcsBiosInput == null)
            {
                return;
            }
            ComboBoxInterfaceType.SelectedValue = _dcsBiosInput.SelectedDCSBIOSInput.Interface;
            ComboBoxDelay.SelectedValue = _dcsBiosInput.SelectedDCSBIOSInput.Delay;
            SetVisibility(_dcsBiosInput.SelectedDCSBIOSInput.Interface);
            switch (_dcsBiosInput.SelectedDCSBIOSInput.Interface)
            {
                case DCSBIOSInputType.FIXED_STEP:
                    {
                        //INC / DEC
                        ComboBoxInputValueFixedStep.SelectedValue = _dcsBiosInput.SelectedDCSBIOSInput.SpecifiedFixedStepArgument;
                        break;
                    }
                case DCSBIOSInputType.ACTION:
                    {
                        //TOGGLE
                        ComboBoxInputValueAction.SelectedValue = _dcsBiosInput.SelectedDCSBIOSInput.SpecifiedActionArgument;
                        break;
                    }
                case DCSBIOSInputType.SET_STATE:
                    {
                        //INTEGER
                        TextBoxInputValueSetState.Text = _dcsBiosInput.SelectedDCSBIOSInput.SpecifiedSetStateArgument.ToString();
                        break;
                    }
                case DCSBIOSInputType.VARIABLE_STEP:
                    {
                        //INTEGER
                        TextBoxInputValueSetState.Text = _dcsBiosInput.SelectedDCSBIOSInput.SpecifiedVariableStepArgument.ToString();
                        break;
                    }
            }
        }

        private void ShowValues2()
        {
            if (_dcsbiosControl != null)
            {
                TextBoxControlId.Text = _dcsbiosControl.identifier;
                TextBoxControlDescription.Text = _dcsbiosControl.description;
            }
            if (_dcsBiosInput != null && _dcsBiosInput.SelectedDCSBIOSInput != null)
            {
                TextBoxInputTypeDescription.Text = _dcsBiosInput.SelectedDCSBIOSInput.Description;
                if (_dcsBiosInput.SelectedDCSBIOSInput.Interface == DCSBIOSInputType.SET_STATE || _dcsBiosInput.SelectedDCSBIOSInput.Interface == DCSBIOSInputType.VARIABLE_STEP)
                {
                    TextBoxMaxValue.Text = _dcsBiosInput.GetMaxValueForInterface(_dcsBiosInput.SelectedDCSBIOSInput.Interface).ToString();
                }
                else
                {
                    TextBoxMaxValue.Text = "";
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
                TextBoxInputValueSetState.Visibility = Visibility.Collapsed;
                LabelMaxValue.Visibility = Visibility.Collapsed;
                TextBoxMaxValue.Visibility = Visibility.Collapsed;
            }
            else if (_dcsBiosInput != null && _dcsBiosInput.SelectedDCSBIOSInput == null)
            {
                LabelInterfaceType.Visibility = Visibility.Visible;
                ComboBoxInterfaceType.Visibility = Visibility.Visible;
                SetVisibility((DCSBIOSInputType)Enum.Parse(typeof(DCSBIOSInputType), ComboBoxInterfaceType.SelectedValue.ToString()));
            }
            else if (_dcsBiosInput != null && _dcsBiosInput.SelectedDCSBIOSInput != null)
            {
                LabelInterfaceType.Visibility = Visibility.Visible;
                ComboBoxInterfaceType.Visibility = Visibility.Visible;
                LabelInputValue.Visibility = Visibility.Visible;
                SetVisibility(_dcsBiosInput.SelectedDCSBIOSInput.Interface);
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
                _dcsBiosInput.SetSelectedInputBasedOnInterfaceType(GetChosenInterfaceType());
                _dcsBiosInput.SelectedDCSBIOSInput.Delay = int.Parse(ComboBoxDelay.SelectedValue.ToString());
                _dcsBiosInput.Delay = int.Parse(ComboBoxDelay.SelectedValue.ToString());
                switch (_dcsBiosInput.SelectedDCSBIOSInput.Interface)
                {
                    case DCSBIOSInputType.ACTION:
                        {
                            _dcsBiosInput.SelectedDCSBIOSInput.SpecifiedActionArgument = ComboBoxInputValueAction.SelectedValue.ToString();
                            break;
                        }
                    case DCSBIOSInputType.SET_STATE:
                        {
                            uint tmp;
                            try
                            {
                                tmp = uint.Parse(TextBoxInputValueSetState.Text);
                            }
                            catch (Exception)
                            {
                                var dcsbiosInputString = "";
                                if (_dcsBiosInput != null)
                                {
                                    dcsbiosInputString = _dcsBiosInput.ControlId + " / " + _dcsBiosInput.SelectedDCSBIOSInput.Interface;
                                }
                                throw new Exception("Please enter a valid value (positive whole number). Value found : [" + TextBoxInputValueSetState.Text + "]" + Environment.NewLine + " DCS-BIOS Input is " + dcsbiosInputString);
                            }
                            if (tmp > _dcsBiosInput.SelectedDCSBIOSInput.MaxValue)
                            {
                                throw new Exception("Input value must be between 0 - " + _dcsBiosInput.SelectedDCSBIOSInput.MaxValue);
                            }
                            _dcsBiosInput.SelectedDCSBIOSInput.SpecifiedSetStateArgument = tmp;

                            break;
                        }
                    case DCSBIOSInputType.VARIABLE_STEP:
                        {
                            int tmp;
                            try
                            {
                                tmp = int.Parse(TextBoxInputValueSetState.Text);
                            }
                            catch (Exception)
                            {
                                var dcsbiosInputString = "";
                                if (_dcsBiosInput != null)
                                {
                                    dcsbiosInputString = _dcsBiosInput.ControlId + " / " + _dcsBiosInput.SelectedDCSBIOSInput.Interface;
                                }
                                throw new Exception("Please enter a valid value (whole number). Value found : [" + TextBoxInputValueSetState.Text + "]" + Environment.NewLine + " DCS-BIOS Input is " + dcsbiosInputString);
                            }
                            if (tmp > _dcsBiosInput.SelectedDCSBIOSInput.MaxValue)
                            {
                                throw new Exception("Input value must be between 0 - " + _dcsBiosInput.SelectedDCSBIOSInput.MaxValue);
                            }
                            _dcsBiosInput.SelectedDCSBIOSInput.SpecifiedVariableStepArgument = tmp;
                            break;
                        }
                    case DCSBIOSInputType.FIXED_STEP:
                        {
                            _dcsBiosInput.SelectedDCSBIOSInput.SpecifiedFixedStepArgument = (DCSBIOSFixedStepInput)Enum.Parse(typeof(DCSBIOSFixedStepInput), ComboBoxInputValueFixedStep.SelectedValue.ToString());
                            break;
                        }
                }
            }
            catch (Exception e)
            {
                throw new Exception("1003351 Error in CopyValues() : " + e.Message);
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
                Common.ShowErrorMessageBox(1002, ex);
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
                Common.ShowErrorMessageBox(1003, ex);
            }
        }

        public DCSBIOSInput DCSBiosInput
        {
            get { return _dcsBiosInput; }
            set { _dcsBiosInput = value; }
        }

        private void AdjustShownPopupData()
        {
            try
            {
                _popupSearch.PlacementTarget = TextBoxSearchWord;
                _popupSearch.Placement = PlacementMode.Bottom;
                if (!_popupSearch.IsOpen)
                {
                    _popupSearch.IsOpen = true;
                }
                if (_dataGridValues != null)
                {
                    if (string.IsNullOrEmpty(TextBoxSearchWord.Text))
                    {
                        _dataGridValues.DataContext = _dcsbiosControls;
                        _dataGridValues.ItemsSource = _dcsbiosControls;
                        _dataGridValues.Items.Refresh();
                        return;
                    }
                    var subList = _dcsbiosControls.Where(controlObject => (!string.IsNullOrWhiteSpace(controlObject.identifier) && controlObject.identifier.ToUpper().Contains(TextBoxSearchWord.Text.ToUpper()))
                                                                          || (!string.IsNullOrWhiteSpace(controlObject.description) && controlObject.description.ToUpper().Contains(TextBoxSearchWord.Text.ToUpper())));
                    _dataGridValues.DataContext = subList;
                    _dataGridValues.ItemsSource = subList;
                    _dataGridValues.Items.Refresh();
                }
            }
            catch (Exception ex)
            {
                Common.LogError(1003, ex, "AdjustShownPopupData()");
            }
        }

        private void TextBoxSearchWord_OnKeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                AdjustShownPopupData();
                SetFormState();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(1005, ex);
            }
        }

        private void DataGridValues_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (_dataGridValues.SelectedItems.Count == 1)
                {
                    _dcsbiosControl = (DCSBIOSControl)_dataGridValues.SelectedItem;
                    _dcsBiosInput = new DCSBIOSInput();
                    _dcsBiosInput.Consume(_dcsbiosControl);
                    _dcsBiosInput.SetSelectedInputBasedOnInterfaceType(GetChosenInterfaceType());
                    PopulateComboBoxInterfaceType(_dcsBiosInput);
                    //TextBoxInputTypeDescription.Text = _dcsBiosInput.GetDescriptionForInterface(GetChosenInterfaceType());
                    //TextBoxMaxValue.Text = _dcsBiosInput.GetMaxValueForInterface(GetChosenInterfaceType()).ToString();
                    ShowValues2();
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

        private void DataGridValues_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (_dataGridValues.SelectedItems.Count == 1)
                {
                    _dcsbiosControl = (DCSBIOSControl)_dataGridValues.SelectedItem;
                    _dcsBiosInput = new DCSBIOSInput();
                    _dcsBiosInput.Consume(_dcsbiosControl);
                    _dcsBiosInput.SetSelectedInputBasedOnInterfaceType(GetChosenInterfaceType());
                    PopulateComboBoxInterfaceType(_dcsBiosInput);
                    //TextBoxInputTypeDescription.Text = _dcsBiosInput.GetDescriptionForInterface(GetChosenInterfaceType());
                    //TextBoxMaxValue.Text = _dcsBiosInput.GetMaxValueForInterface(GetChosenInterfaceType()).ToString();
                    ShowValues2();
                }
                SetFormState();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(1007, ex);
            }
        }


        private void DataGridValues_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (_dataGridValues.SelectedItems.Count == 1)
                {
                    _dcsbiosControl = (DCSBIOSControl)_dataGridValues.SelectedItem;
                    _dcsBiosInput = new DCSBIOSInput();
                    _dcsBiosInput.Consume(_dcsbiosControl);
                    _dcsBiosInput.SetSelectedInputBasedOnInterfaceType(GetChosenInterfaceType());
                    PopulateComboBoxInterfaceType(_dcsBiosInput);
                    //TextBoxInputTypeDescription.Text = _dcsBiosInput.GetDescriptionForInterface(GetChosenInterfaceType());
                    //TextBoxMaxValue.Text = _dcsBiosInput.GetMaxValueForInterface(GetChosenInterfaceType()).ToString();
                    ShowValues2();
                }
                _popupSearch.IsOpen = false;
                SetFormState();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(1006, ex);
            }
        }

        private void PopulateComboBoxInterfaceType(DCSBIOSInput dcsbiosInput)
        {
            try
            {
                ComboBoxInterfaceType.SelectionChanged -= ComboBoxInterfaceType_OnSelectionChanged;
                ComboBoxInterfaceType.Items.Clear();
                foreach (var dcsbiosInputObject in dcsbiosInput.DCSBIOSInputObjects)
                {
                    var comboBoxItem = new ComboBoxItem();
                    comboBoxItem.Content = dcsbiosInputObject.Interface.ToString();
                    ComboBoxInterfaceType.Items.Add(comboBoxItem);
                }
                if (dcsbiosInput.SelectedDCSBIOSInput != null)
                {
                    ComboBoxInterfaceType.SelectedValue = dcsbiosInput.SelectedDCSBIOSInput.Interface.ToString();
                }
                else
                {
                    ComboBoxInterfaceType.SelectedValue = dcsbiosInput.DCSBIOSInputObjects[0].Interface.ToString();
                }
            }
            finally
            {
                ComboBoxInterfaceType.SelectionChanged += ComboBoxInterfaceType_OnSelectionChanged;
            }
        }

        private DCSBIOSInputType GetChosenInterfaceType()
        {
            if (ComboBoxInterfaceType.SelectedValue == null)
            {
                return DCSBIOSInputType.SET_STATE;
            }
            var result = (DCSBIOSInputType)Enum.Parse(typeof(DCSBIOSInputType), ComboBoxInterfaceType.SelectedValue.ToString());
            return result;
        }

        private void ComboBoxInterfaceType_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (_dcsBiosInput != null)
                {
                    var inputType = (DCSBIOSInputType)Enum.Parse(typeof(DCSBIOSInputType), ComboBoxInterfaceType.SelectedValue.ToString());
                    _dcsBiosInput.SetSelectedInputBasedOnInterfaceType(inputType);
                    TextBoxInputTypeDescription.Text = _dcsBiosInput.SelectedDCSBIOSInput.Description;
                    if (_dcsBiosInput.SelectedDCSBIOSInput.Interface == DCSBIOSInputType.SET_STATE || _dcsBiosInput.SelectedDCSBIOSInput.Interface == DCSBIOSInputType.VARIABLE_STEP)
                    {
                        TextBoxMaxValue.Text = _dcsBiosInput.SelectedDCSBIOSInput.MaxValue.ToString();
                    }
                    SetFormState();
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(100906, ex);
            }
        }

        private void DCSBiosInputWindow_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (!ButtonOk.IsEnabled)
            {
                DialogResult = false;
                Close();
            }
        }
    }
}