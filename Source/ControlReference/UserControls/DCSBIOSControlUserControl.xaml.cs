using System;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ClassLibraryCommon;
using DCS_BIOS.Json;

namespace ControlReference.UserControls
{
    /// <summary>
    /// Interaction logic for DCSBIOSControlUserControl.xaml
    /// </summary>
    public partial class DCSBIOSControlUserControl : UserControl
    {
        private readonly DCSBIOSControl _dcsbiosControl;

        public DCSBIOSControlUserControl(DCSBIOSControl dcsbiosControl)
        {
            InitializeComponent();
            _dcsbiosControl = dcsbiosControl;
        }

        private void DCSBIOSControlUserControl_OnLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                ShowControl();
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

                    LabelOutputType.Content = _dcsbiosControl.Outputs[0].OutputDataType;
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
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }
    }
}
