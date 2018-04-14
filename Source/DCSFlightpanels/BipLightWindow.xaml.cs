using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using ClassLibraryCommon;
using DCS_BIOS;
using NonVisuals;

namespace DCSFlightpanels
{
    /// <summary>
    /// Interaction logic for BipLightWindow.xaml
    /// </summary>
    public partial class BipLightWindow : Window
    {
        private string _description;
        private bool _formLoaded;
        private BIPLight _bipLight;

        public BipLightWindow()
        {
            InitializeComponent();
        }

        public BipLightWindow(BIPLight bipLight, string description)
        {
            InitializeComponent();
            _description = description;
            _bipLight = bipLight;
            PopulateComboBoxes();
        }

        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                LabelDescription.Content = _description;
                ShowValues1();
                ShowValues2();
                _formLoaded = true;
                SetFormState();
                ComboBoxPosition.SelectionChanged += ComboBox_OnSelectionChanged;
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(1001, ex);
            }
        }


        
        private void SetFormState()
        {
            if (!_formLoaded)
            {
                return;
            }
            ButtonOk.IsEnabled = _bipLight != null;
            if (_bipLight == null)
            {
                LabelDescription.Visibility = Visibility.Collapsed;
            }
            else if (_bipLight._dcsBiosInput != null && _dcsBiosInput.SelectedDCSBIOSInput == null)
            {
                LabelInterfaceType.Visibility = Visibility.Visible;
                ComboBoxPosition.Visibility = Visibility.Visible;
                SetVisibility((DCSBIOSInputType)Enum.Parse(typeof(DCSBIOSInputType), ComboBoxPosition.SelectedValue.ToString()));
            }
            else if (_dcsBiosInput != null && _dcsBiosInput.SelectedDCSBIOSInput != null)
            {
                LabelInterfaceType.Visibility = Visibility.Visible;
                ComboBoxPosition.Visibility = Visibility.Visible;
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
            _bipLight = null;
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

        public BIPLight BIPLight
        {
            get { return _bipLight; }
            set { _bipLight = value; }
        }

        private void PopulateComboBoxes()
        {
            try
            {
                ComboBoxPosition.SelectionChanged -= ComboBox_OnSelectionChanged;
                ComboBoxPosition.Items.Clear();
                foreach (BIPLedPositionEnum position in Enum.GetValues(typeof(BIPLedPositionEnum)))
                {
                    var comboBoxItem = new ComboBoxItem();
                    comboBoxItem.Content = position;
                    ComboBoxPosition.Items.Add(comboBoxItem);
                }

                ComboBoxDelay.SelectionChanged -= ComboBox_OnSelectionChanged;
                ComboBoxDelay.Items.Clear();
                foreach (BIPLightDelays delay in Enum.GetValues(typeof(BIPLightDelays)))
                {
                    var comboBoxItem = new ComboBoxItem();
                    comboBoxItem.Content = delay;
                    ComboBoxDelay.Items.Add(comboBoxItem);
                }

                ComboBoxColor.SelectionChanged -= ComboBox_OnSelectionChanged;
                ComboBoxColor.Items.Clear();
                foreach (PanelLEDColor color in Enum.GetValues(typeof(PanelLEDColor)))
                {
                    var comboBoxItem = new ComboBoxItem();
                    comboBoxItem.Content = color;
                    ComboBoxColor.Items.Add(comboBoxItem);
                }

                if (_bipLight != null)
                {
                    ComboBoxPosition.SelectedValue = _bipLight.BIPLedPosition;
                    ComboBoxDelay.SelectedValue = _bipLight.DelayBefore;
                    ComboBoxColor.SelectedValue = _bipLight.LEDColor;
                }
            }
            finally
            {
                ComboBoxPosition.SelectionChanged += ComboBox_OnSelectionChanged;
                ComboBoxDelay.SelectionChanged += ComboBox_OnSelectionChanged;
                ComboBoxColor.SelectionChanged += ComboBox_OnSelectionChanged;
            }
        }
        
        private void ComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (_dcsBiosInput != null)
                {
                    var inputType = (DCSBIOSInputType)Enum.Parse(typeof(DCSBIOSInputType), ComboBoxPosition.SelectedValue.ToString());
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
    }
}