using System;
using System.Media;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using ClassLibraryCommon;
using ControlReference.Events;
using DCS_BIOS;
using DCS_BIOS.EventArgs;
using DCS_BIOS.Interfaces;
using DCS_BIOS.Json;

namespace ControlReference.UserControls
{
    /// <summary>
    /// Interaction logic for DCSBIOSControlUserControl.xaml
    /// </summary>
    public partial class DCSBIOSControlUserControl : UserControl, IDisposable, IDcsBiosDataListener, IDCSBIOSStringListener
    {
        private readonly DCSBIOSControl _dcsbiosControl;
        private ToolTip _copyToolTip = null;
        private readonly DCSBIOSOutput _dcsbiosOutput = null;

        public DCSBIOSControlUserControl(DCSBIOSControl dcsbiosControl)
        {
            InitializeComponent();
            _dcsbiosControl = dcsbiosControl;
            _dcsbiosOutput = DCSBIOSControlLocator.GetDCSBIOSOutput(_dcsbiosControl.Identifier);
            if (_dcsbiosOutput.DCSBiosOutputType == DCSBiosOutputType.StringType)
            {
                DCSBIOSStringManager.AddListeningAddress(_dcsbiosOutput);
            }
            
            BIOSEventHandler.AttachDataListener(this);
            BIOSEventHandler.AttachStringListener(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                BIOSEventHandler.DetachDataListener(this);
                BIOSEventHandler.DetachStringListener(this);
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        
        private void DCSBIOSControlUserControl_OnLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                SetFormState();
                ShowControl();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void SetFormState()
        {
            try
            {
                ButtonSetVariableStep.IsEnabled = !string.IsNullOrEmpty(TextBoxVariableStepValue.Text);
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void ShowControl()
        {
            try
            {
                LabelControlId.Content = _dcsbiosControl.Identifier;
                LabelCategory.Content = _dcsbiosControl.Category;
                LabelDescription.Content = _dcsbiosControl.Description;

                StackPanelFixedStep.Visibility = Visibility.Collapsed;
                StackPanelVariableStep.Visibility = Visibility.Collapsed;
                StackPanelSetState.Visibility = Visibility.Collapsed;
                StackPanelAction.Visibility = Visibility.Collapsed;

                foreach (var dcsbiosControlInput in _dcsbiosControl.Inputs)
                {
                    switch (dcsbiosControlInput.ControlInterface)
                    {
                        case "fixed_step":
                            {
                                StackPanelFixedStep.Visibility = Visibility.Visible;
                                break;
                            }
                        case "variable_step":
                            {
                                StackPanelVariableStep.Visibility = Visibility.Visible;
                                break;
                            }
                        case "set_state":
                            {
                                StackPanelSetState.Visibility = Visibility.Visible;
                                SliderSetState.Minimum = 0;
                                SliderSetState.Maximum = Convert.ToDouble(dcsbiosControlInput.MaxValue);
                                ButtonSetState.Content = 0;
                                SliderSetState.Value = 0;
                                break;
                            }
                        case "action":
                            {
                                StackPanelAction.Visibility = Visibility.Visible;
                                break;
                            }
                        default:
                            {
                                throw new Exception(
                                    $"Failed to identify Input Interface {dcsbiosControlInput.ControlInterface}.");
                            }
                    }
                }

                if (_dcsbiosControl.Outputs[0].OutputDataType == DCSBiosOutputType.IntegerType)
                {
                    LabelOutputType.Content = "integer";
                    LabelOutputMaxValue.Content = _dcsbiosControl.Outputs[0].MaxValue;
                }
                if (_dcsbiosControl.Outputs[0].OutputDataType == DCSBiosOutputType.StringType)
                {
                    LabelOutputMaxValueDesc.Content = "Max Length: ";
                    LabelOutputType.Content = "string";
                    LabelOutputMaxValue.Content = _dcsbiosControl.Outputs[0].MaxLength;
                }

            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void LabelControlId_OnMouseEnter(object sender, MouseEventArgs e)
        {
            try
            {
                Mouse.OverrideCursor = Cursors.Hand;
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void LabelControlId_OnMouseLeave(object sender, MouseEventArgs e)
        {
            try
            {
                Mouse.OverrideCursor = Cursors.Arrow;
                if (_copyToolTip != null)
                {
                    _copyToolTip.IsOpen = false;
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void LabelControlId_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Clipboard.SetText(_dcsbiosControl.Identifier);
                SystemSounds.Exclamation.Play();
                _copyToolTip = new ToolTip
                {
                    Content = "Value copied.",
                    PlacementTarget = LabelControlId,
                    Placement = PlacementMode.Bottom,
                    IsOpen = true,
                };
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        public void DcsBiosDataReceived(object sender, DCSBIOSDataEventArgs e)
        {
            try
            {
                if (e.Address == _dcsbiosOutput.Address)
                {
                    var value = _dcsbiosOutput.GetUIntValue(e.Data);
                    Dispatcher?.BeginInvoke((Action)(() => LabelCurrentValue.Content = value));

                    if (_dcsbiosOutput.MaxValue == 0)
                    {
                        return;
                    }
                    var percentage = (value * 100) / _dcsbiosOutput.MaxValue;
                    Dispatcher?.BeginInvoke((Action)(() => LabelPercentage.Content = $"({percentage}%)"));
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        public void DCSBIOSStringReceived(object sender, DCSBIOSStringDataEventArgs e)
        {
            try
            {
                if (e.Address == _dcsbiosOutput.Address)
                {
                    Dispatcher?.BeginInvoke((Action)(() => LabelCurrentValue.Content = $"->{e.StringData}<-"));
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }


        private void SliderSetState_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            try
            {
                ButtonSetState.Content = e.NewValue;
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void ButtonFixedStepDec_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                DCSBIOS.Send($"{_dcsbiosControl.Identifier} DEC\n");
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void ButtonFixedStepInc_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                DCSBIOS.Send($"{_dcsbiosControl.Identifier} INC\n");
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void ButtonSetState_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                DCSBIOS.Send($"{_dcsbiosControl.Identifier} {Convert.ToUInt32(SliderSetState.Value)}\n");
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void ButtonSetVariableStep_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                DCSBIOS.Send($"{_dcsbiosControl.Identifier} {TextBoxVariableStepValue.Text}\n");
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void TextBoxVariableStepValue_OnKeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key is not (>= Key.D0 and <= Key.D9 or >= Key.NumPad0 and <= Key.NumPad9) && e.Key != Key.OemMinus && e.Key != Key.OemPlus
                    && e.Key != Key.Add && e.Key != Key.Subtract)
                {
                    e.Handled = true;
                    return;
                }
                SetFormState();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void ButtonToggle_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                DCSBIOS.Send($"{_dcsbiosControl.Identifier} TOGGLE\n");
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void LabelCategory_OnMouseEnter(object sender, MouseEventArgs e)
        {
            try
            {
                Mouse.OverrideCursor = Cursors.Hand;
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void LabelCategory_OnMouseLeave(object sender, MouseEventArgs e)
        {
            try
            {
                Mouse.OverrideCursor = Cursors.Arrow;
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void LabelCategory_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                REFEventHandler.ChangeCategory(this, LabelCategory.Content.ToString());
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }
    }
}
