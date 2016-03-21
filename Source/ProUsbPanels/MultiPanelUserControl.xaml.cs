using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using DCS_BIOS;
using NonVisuals;
using ProUsbPanels.Properties;

namespace ProUsbPanels
{
    /// <summary>
    /// Interaction logic for MultiPanelUserControl.xaml
    /// </summary>
    public partial class MultiPanelUserControl : ISaitekPanelListener, IProfileHandlerListener, ISaitekUserControl
    {
        private readonly MultiPanelPZ70 _multiPanelPZ70;
        private TabItem _parentTabItem;
        private string _parentTabItemHeader;
        private IGlobalHandler _globalHandler;
        private bool _userControlLoaded;

        public MultiPanelUserControl(HIDSkeleton hidSkeleton, TabItem parentTabItem, IGlobalHandler globalHandler)
        {
            InitializeComponent();
            _parentTabItem = parentTabItem;
            _parentTabItemHeader = _parentTabItem.Header.ToString();
            _multiPanelPZ70 = new MultiPanelPZ70(hidSkeleton);
            _multiPanelPZ70.Attach((ISaitekPanelListener)this);
            globalHandler.Attach(_multiPanelPZ70);
            _globalHandler = globalHandler;

            HideAllImages();
        }

        public SaitekPanel GetSaitekPanel()
        {
            return _multiPanelPZ70;
        }

        public void UpdatesHasBeenMissed(string uniqueId, SaitekPanelsEnum saitekPanelsEnum, int count)
        {
            try
            {
                //todo
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(471073, ex);
            }
        }

        public string GetName()
        {
            return GetType().Name;
        }

        public void SelectedAirframe(DCSAirframe dcsAirframe)
        {
            try
            {
                SetApplicationMode(dcsAirframe);
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(471573, ex);
            }
        }

        private void SetApplicationMode(DCSAirframe dcsAirframe)
        {
            ButtonKnobAltLcdUpper.IsEnabled = dcsAirframe != DCSAirframe.NONE;
            ButtonKnobAltLcdLower.IsEnabled = dcsAirframe != DCSAirframe.NONE;
            ButtonKnobVsLcdUpper.IsEnabled = dcsAirframe != DCSAirframe.NONE;
            ButtonKnobVsLcdLower.IsEnabled = dcsAirframe != DCSAirframe.NONE;
        }

        public void SwitchesChanged(string uniqueId, SaitekPanelsEnum saitekPanelsEnum, HashSet<object> hashSet)
        {
            try
            {
                if (saitekPanelsEnum == SaitekPanelsEnum.PZ70MultiPanel && uniqueId.Equals(_multiPanelPZ70.InstanceId))
                {
                    NotifyKnobChanges(hashSet);
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(1018, ex);
            }
        }

        public void PanelSettingsReadFromFile(List<string> settings)
        {
            try
            {
                ShowGraphicConfiguration();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(1019, ex);
            }
        }

        public void SettingsCleared(string uniqueId, SaitekPanelsEnum saitekPanelsEnum)
        {
            try
            {
                ClearAll(false);
                ShowGraphicConfiguration();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(1020, ex);
            }
        }

        private void ClearAll(bool clearAlsoProfile = true)
        {
            foreach (var textBox in Common.FindVisualChildren<TextBox>(this))
            {
                textBox.Text = "";
                textBox.Tag = null;
            }
            if (clearAlsoProfile)
            {
                _multiPanelPZ70.ClearSettings();
            }
        }

        public void LedLightChanged(string uniqueId, SaitekPanelLEDPosition saitekPanelLEDPosition, PanelLEDColor panelLEDColor)
        {
            try
            {
                //todo
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(1021, ex);
            }
        }

        public void PanelSettingsChanged(string uniqueId, SaitekPanelsEnum saitekPanelsEnum)
        {
            try
            {
                //todo nada?
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(1022, ex);
            }
        }

        public void PanelDataAvailable(string stringData)
        {
            try
            {
                //todo
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(1023, ex);
            }
        }

        public void SettingsApplied(string uniqueId, SaitekPanelsEnum saitekPanelsEnum)
        {
            try
            {
                if (uniqueId.Equals(_multiPanelPZ70.InstanceId) && saitekPanelsEnum == SaitekPanelsEnum.PZ70MultiPanel)
                {
                    Dispatcher.BeginInvoke((Action)(ShowGraphicConfiguration));
                    Dispatcher.BeginInvoke((Action)(() => TextBoxLogPZ70.Text = ""));
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(992032, ex);
            }
        }

        public void DeviceAttached(string uniqueId, SaitekPanelsEnum saitekPanelsEnum)
        {
            try
            {
                //todo
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(1025, ex);
            }
        }

        public void DeviceDetached(string uniqueId, SaitekPanelsEnum saitekPanelsEnum)
        {
            try
            {
                //todo
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(1026, ex);
            }
        }

        private void ButtonGetId_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_multiPanelPZ70 != null)
                {
                    Clipboard.SetText(_multiPanelPZ70.InstanceId);
                    MessageBox.Show("Instance id has been copied to the ClipBoard.");
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(2000, ex);
            }
        }

        private void SetGraphicsState(HashSet<object> knobs)
        {
            try
            {
                foreach (var multiKnobO in knobs)
                {
                    var multiKnob = (MultiPanelKnob)multiKnobO;
                    switch (multiKnob.MultiPanelPZ70Knob)
                    {
                        case MultiPanelPZ70Knobs.KNOB_ALT:
                            {
                                var key = multiKnob;
                                Dispatcher.BeginInvoke(
                                    (Action)delegate
                                    {
                                        ImageLeftKnobAlt.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case MultiPanelPZ70Knobs.KNOB_VS:
                            {
                                var key = multiKnob;
                                Dispatcher.BeginInvoke(
                                    (Action)delegate
                                    {
                                        ImageLeftKnobVs.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case MultiPanelPZ70Knobs.KNOB_IAS:
                            {
                                var key = multiKnob;
                                Dispatcher.BeginInvoke(
                                    (Action)delegate
                                    {
                                        ImageLeftKnobIas.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case MultiPanelPZ70Knobs.KNOB_HDG:
                            {
                                var key = multiKnob;
                                Dispatcher.BeginInvoke(
                                    (Action)delegate
                                    {
                                        ImageLeftKnobHdg.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case MultiPanelPZ70Knobs.KNOB_CRS:
                            {
                                var key = multiKnob;
                                Dispatcher.BeginInvoke(
                                    (Action)delegate
                                    {
                                        ImageLeftKnobCrs.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case MultiPanelPZ70Knobs.AP_BUTTON:
                            {
                                var key = multiKnob;
                                Dispatcher.BeginInvoke(
                                    (Action)delegate
                                    {
                                        ImageLcdButtonAp.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case MultiPanelPZ70Knobs.HDG_BUTTON:
                            {
                                var key = multiKnob;
                                Dispatcher.BeginInvoke(
                                    (Action)delegate
                                    {
                                        ImageLcdButtonHdg.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case MultiPanelPZ70Knobs.NAV_BUTTON:
                            {
                                var key = multiKnob;
                                Dispatcher.BeginInvoke(
                                    (Action)delegate
                                    {
                                        ImageLcdButtonNav.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case MultiPanelPZ70Knobs.IAS_BUTTON:
                            {
                                var key = multiKnob;
                                Dispatcher.BeginInvoke(
                                    (Action)delegate
                                    {
                                        ImageLcdButtonIas.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case MultiPanelPZ70Knobs.ALT_BUTTON:
                            {
                                var key = multiKnob;
                                Dispatcher.BeginInvoke(
                                    (Action)delegate
                                    {
                                        ImageLcdButtonAlt.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case MultiPanelPZ70Knobs.VS_BUTTON:
                            {
                                var key = multiKnob;
                                Dispatcher.BeginInvoke(
                                    (Action)delegate
                                    {
                                        ImageLcdButtonVs.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case MultiPanelPZ70Knobs.APR_BUTTON:
                            {
                                var key = multiKnob;
                                Dispatcher.BeginInvoke(
                                    (Action)delegate
                                    {
                                        ImageLcdButtonApr.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case MultiPanelPZ70Knobs.REV_BUTTON:
                            {
                                var key = multiKnob;
                                Dispatcher.BeginInvoke(
                                    (Action)delegate
                                    {
                                        ImageLcdButtonRev.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case MultiPanelPZ70Knobs.LCD_WHEEL_DEC:
                            {
                                var key = multiKnob;
                                Dispatcher.BeginInvoke(
                                    (Action)delegate
                                    {
                                        ImageLcdKnobDec.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case MultiPanelPZ70Knobs.LCD_WHEEL_INC:
                            {
                                var key = multiKnob;
                                Dispatcher.BeginInvoke(
                                    (Action)delegate
                                    {
                                        ImageLcdKnobInc.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case MultiPanelPZ70Knobs.AUTO_THROTTLE:
                            {
                                var key = multiKnob;
                                Dispatcher.BeginInvoke(
                                    (Action)delegate
                                    {
                                        ImageLcdAutoThrottleArm.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                        ImageLcdAutoThrottleOff.Visibility = !key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case MultiPanelPZ70Knobs.FLAPS_LEVER_UP:
                            {
                                var key = multiKnob;
                                Dispatcher.BeginInvoke(
                                    (Action)delegate
                                    {
                                        ImageFlapsUp.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case MultiPanelPZ70Knobs.FLAPS_LEVER_DOWN:
                            {
                                var key = multiKnob;
                                Dispatcher.BeginInvoke(
                                    (Action)delegate
                                    {
                                        ImageFlapsDown.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case MultiPanelPZ70Knobs.PITCH_TRIM_WHEEL_UP:
                            {
                                var key = multiKnob;
                                Dispatcher.BeginInvoke(
                                    (Action)delegate
                                    {
                                        ImagePitchUp.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case MultiPanelPZ70Knobs.PITCH_TRIM_WHEEL_DOWN:
                            {
                                var key = multiKnob;
                                Dispatcher.BeginInvoke(
                                    (Action)delegate
                                    {
                                        ImagePitchDown.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                    }
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(2019, ex);
            }
        }

        private void HideAllImages()
        {
            ImageLeftKnobAlt.Visibility = Visibility.Collapsed;
            ImageLeftKnobVs.Visibility = Visibility.Collapsed;
            ImageLeftKnobIas.Visibility = Visibility.Collapsed;
            ImageLeftKnobHdg.Visibility = Visibility.Collapsed;
            ImageLeftKnobCrs.Visibility = Visibility.Collapsed;
            ImageLcdButtonAp.Visibility = Visibility.Collapsed;
            ImageLcdButtonHdg.Visibility = Visibility.Collapsed;
            ImageLcdButtonNav.Visibility = Visibility.Collapsed;
            ImageLcdButtonIas.Visibility = Visibility.Collapsed;
            ImageLcdButtonAlt.Visibility = Visibility.Collapsed;
            ImageLcdButtonVs.Visibility = Visibility.Collapsed;
            ImageLcdButtonApr.Visibility = Visibility.Collapsed;
            ImageLcdButtonRev.Visibility = Visibility.Collapsed;
            ImageLcdKnobDec.Visibility = Visibility.Collapsed;
            ImageLcdKnobInc.Visibility = Visibility.Collapsed;
            ImageLcdAutoThrottleOff.Visibility = Visibility.Collapsed;
            ImageLcdAutoThrottleArm.Visibility = Visibility.Collapsed;
            ImageFlapsUp.Visibility = Visibility.Collapsed;
            ImageFlapsDown.Visibility = Visibility.Collapsed;
            ImagePitchUp.Visibility = Visibility.Collapsed;
            ImagePitchDown.Visibility = Visibility.Collapsed;

            ImageAltUpperRow.Visibility = Visibility.Collapsed;
            ImageAltLowerRow.Visibility = Visibility.Collapsed;
            ImageVsUpperRow.Visibility = Visibility.Collapsed;
            ImageVsLowerRow.Visibility = Visibility.Collapsed;
        }

        private void ButtonKnobLcd_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var button = (Button)sender;

                switch (button.Name)
                {
                    case "ButtonKnobAltLcdUpper":
                        {
                            ButtonLcdConfig(button, "Data to display on upper LCD Row when Knob = ALT");
                            break;
                        }
                    case "ButtonKnobAltLcdLower":
                        {
                            ButtonLcdConfig(button, "Data to display on lower LCD Row when Knob = ALT");
                            break;
                        }
                    case "ButtonKnobVsLcdUpper":
                        {
                            ButtonLcdConfig(button, "Data to display on upper LCD Row when Knob = VS");
                            break;
                        }
                    case "ButtonKnobVsLcdLower":
                        {
                            ButtonLcdConfig(button, "Data to display on lower LCD Row when Knob = VS");
                            break;
                        }
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(9965005, ex);
            }
        }

        private void ButtonLcdConfig(Button button, string description)
        {
            try
            {
                if (button == null)
                {
                    throw new Exception("Failed to locate which button was clicked.");
                }
                DCSBiosOutputFormulaWindow dcsBiosOutputFormulaWindow;
                if (button.Tag is DCSBIOSOutput)
                {
                    dcsBiosOutputFormulaWindow = new DCSBiosOutputFormulaWindow(_globalHandler.GetAirframe(), description, (DCSBIOSOutput)button.Tag);
                }
                else if (button.Tag is DCSBIOSOutputFormula)
                {
                    dcsBiosOutputFormulaWindow = new DCSBiosOutputFormulaWindow(_globalHandler.GetAirframe(), description, (DCSBIOSOutputFormula)button.Tag);
                }
                else
                {
                    dcsBiosOutputFormulaWindow = new DCSBiosOutputFormulaWindow(_globalHandler.GetAirframe(), description);
                }
                dcsBiosOutputFormulaWindow.ShowDialog();
                if (dcsBiosOutputFormulaWindow.DialogResult.HasValue && dcsBiosOutputFormulaWindow.DialogResult.Value)
                {
                    if (dcsBiosOutputFormulaWindow.UseFormula())
                    {
                        var dcsBiosOutputFormula = dcsBiosOutputFormulaWindow.DCSBIOSOutputFormula;
                        //1 appropriate text to textbox
                        //2 update bindings
                        //button.Text = dcsBiosOutputFormula.ToString();
                        button.Tag = dcsBiosOutputFormula;
                        button.ToolTip = dcsBiosOutputFormula.ToString();
                        UpdateDCSBIOSBindingLCD(button);
                    }
                    else if (dcsBiosOutputFormulaWindow.UseSingleDCSBiosControl())
                    {
                        var dcsBiosOutput = dcsBiosOutputFormulaWindow.DCSBiosOutput;
                        //1 appropriate text to textbox
                        //2 update bindings
                        //button.Text = dcsBiosInput.ToString();
                        button.Tag = dcsBiosOutput;
                        button.ToolTip = dcsBiosOutput.ToString();
                        UpdateDCSBIOSBindingLCD(button);
                    }
                    else
                    {
                        //Delete config
                        //1 appropriate text to textbox
                        //2 update bindings
                        //button.Text = dcsBiosInput.ToString();
                        button.Tag = null;
                        button.ToolTip = "";
                        UpdateDCSBIOSBindingLCD(button);
                    }
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(49942044, ex);
            }
        }

        private void TextBoxLostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                ((TextBox)sender).Background = Brushes.White;
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(993005, ex);
            }
        }

        private void ShowGraphicConfiguration()
        {
            try
            {
                foreach (var keyBinding in _multiPanelPZ70.KeyBindingsHashSet)
                {
                    if (keyBinding.MultiPanelPZ70Knob == MultiPanelPZ70Knobs.LCD_WHEEL_DEC)
                    {
                        if (keyBinding.WhenTurnedOn)
                        {
                            if (keyBinding.OSKeyPress != null && !keyBinding.OSKeyPress.IsMultiSequenced())
                            {
                                TextBoxLcdKnobDecrease.Tag = keyBinding.OSKeyPress.LengthOfKeyPress();
                                TextBoxLcdKnobDecrease.Text = keyBinding.OSKeyPress.GetSimpleVirtualKeyCodesAsString();
                            }
                            else if (keyBinding.OSKeyPress != null && keyBinding.OSKeyPress.IsMultiSequenced())
                            {
                                TextBoxLcdKnobDecrease.Tag = keyBinding.OSKeyPress.GetSequence;
                                TextBoxLcdKnobDecrease.Text = keyBinding.OSKeyPress.Information;
                            }
                        }
                    }
                    if (keyBinding.MultiPanelPZ70Knob == MultiPanelPZ70Knobs.LCD_WHEEL_INC)
                    {
                        if (keyBinding.WhenTurnedOn)
                        {
                            if (keyBinding.OSKeyPress != null && !keyBinding.OSKeyPress.IsMultiSequenced())
                            {
                                TextBoxLcdKnobIncrease.Tag = keyBinding.OSKeyPress.LengthOfKeyPress();
                                TextBoxLcdKnobIncrease.Text = keyBinding.OSKeyPress.GetSimpleVirtualKeyCodesAsString();
                            }
                            else if (keyBinding.OSKeyPress != null && keyBinding.OSKeyPress.IsMultiSequenced())
                            {
                                TextBoxLcdKnobIncrease.Tag = keyBinding.OSKeyPress.GetSequence;
                                TextBoxLcdKnobIncrease.Text = keyBinding.OSKeyPress.Information;
                            }
                        }
                    }
                    if (keyBinding.MultiPanelPZ70Knob == MultiPanelPZ70Knobs.AUTO_THROTTLE)
                    {
                        if (keyBinding.WhenTurnedOn)
                        {
                            if (keyBinding.OSKeyPress != null && !keyBinding.OSKeyPress.IsMultiSequenced())
                            {
                                TextBoxAutoThrottleOn.Tag = keyBinding.OSKeyPress.LengthOfKeyPress();
                                TextBoxAutoThrottleOn.Text = keyBinding.OSKeyPress.GetSimpleVirtualKeyCodesAsString();
                            }
                            else if (keyBinding.OSKeyPress != null && keyBinding.OSKeyPress.IsMultiSequenced())
                            {
                                TextBoxAutoThrottleOn.Tag = keyBinding.OSKeyPress.GetSequence;
                                TextBoxAutoThrottleOn.Text = keyBinding.OSKeyPress.Information;
                            }
                        }
                        else
                        {
                            if (keyBinding.OSKeyPress != null && !keyBinding.OSKeyPress.IsMultiSequenced())
                            {
                                TextBoxAutoThrottleOff.Tag = keyBinding.OSKeyPress.LengthOfKeyPress();
                                TextBoxAutoThrottleOff.Text = keyBinding.OSKeyPress.GetSimpleVirtualKeyCodesAsString();
                            }
                            else if (keyBinding.OSKeyPress != null && keyBinding.OSKeyPress.IsMultiSequenced())
                            {
                                TextBoxAutoThrottleOff.Tag = keyBinding.OSKeyPress.GetSequence;
                                TextBoxAutoThrottleOff.Text = keyBinding.OSKeyPress.Information;
                            }
                        }
                    }
                    if (keyBinding.MultiPanelPZ70Knob == MultiPanelPZ70Knobs.FLAPS_LEVER_UP)
                    {
                        if (keyBinding.WhenTurnedOn)
                        {
                            if (keyBinding.OSKeyPress != null && !keyBinding.OSKeyPress.IsMultiSequenced())
                            {
                                TextBoxFlapsUp.Tag = keyBinding.OSKeyPress.LengthOfKeyPress();
                                TextBoxFlapsUp.Text = keyBinding.OSKeyPress.GetSimpleVirtualKeyCodesAsString();
                            }
                            else if (keyBinding.OSKeyPress != null && keyBinding.OSKeyPress.IsMultiSequenced())
                            {
                                TextBoxFlapsUp.Text = keyBinding.OSKeyPress.Information;
                                TextBoxFlapsUp.Tag = keyBinding.OSKeyPress.GetSequence;
                            }
                        }
                    }
                    if (keyBinding.MultiPanelPZ70Knob == MultiPanelPZ70Knobs.FLAPS_LEVER_DOWN)
                    {
                        if (keyBinding.WhenTurnedOn)
                        {
                            if (keyBinding.OSKeyPress != null && !keyBinding.OSKeyPress.IsMultiSequenced())
                            {
                                TextBoxFlapsDown.Tag = keyBinding.OSKeyPress.LengthOfKeyPress();
                                TextBoxFlapsDown.Text = keyBinding.OSKeyPress.GetSimpleVirtualKeyCodesAsString();
                            }
                            else if (keyBinding.OSKeyPress != null && keyBinding.OSKeyPress.IsMultiSequenced())
                            {
                                TextBoxFlapsDown.Tag = keyBinding.OSKeyPress.GetSequence;
                                TextBoxFlapsDown.Text = keyBinding.OSKeyPress.Information;
                            }
                        }
                    }

                    if (keyBinding.MultiPanelPZ70Knob == MultiPanelPZ70Knobs.PITCH_TRIM_WHEEL_UP)
                    {
                        if (keyBinding.WhenTurnedOn)
                        {
                            if (keyBinding.OSKeyPress != null && !keyBinding.OSKeyPress.IsMultiSequenced())
                            {
                                TextBoxPitchTrimUp.Tag = keyBinding.OSKeyPress.LengthOfKeyPress();
                                TextBoxPitchTrimUp.Text = keyBinding.OSKeyPress.GetSimpleVirtualKeyCodesAsString();
                            }
                            else if (keyBinding.OSKeyPress != null && keyBinding.OSKeyPress.IsMultiSequenced())
                            {
                                TextBoxPitchTrimUp.Tag = keyBinding.OSKeyPress.GetSequence;
                                TextBoxPitchTrimUp.Text = keyBinding.OSKeyPress.Information;
                            }
                        }
                    }
                    if (keyBinding.MultiPanelPZ70Knob == MultiPanelPZ70Knobs.PITCH_TRIM_WHEEL_DOWN)
                    {
                        if (keyBinding.WhenTurnedOn)
                        {
                            if (keyBinding.OSKeyPress != null && !keyBinding.OSKeyPress.IsMultiSequenced())
                            {
                                TextBoxPitchTrimDown.Tag = keyBinding.OSKeyPress.LengthOfKeyPress();
                                TextBoxPitchTrimDown.Text = keyBinding.OSKeyPress.GetSimpleVirtualKeyCodesAsString();
                            }
                            else if (keyBinding.OSKeyPress != null && keyBinding.OSKeyPress.IsMultiSequenced())
                            {
                                TextBoxPitchTrimDown.Tag = keyBinding.OSKeyPress.GetSequence;
                                TextBoxPitchTrimDown.Text = keyBinding.OSKeyPress.Information;
                            }
                        }
                    }
                    if (keyBinding.MultiPanelPZ70Knob == MultiPanelPZ70Knobs.AP_BUTTON)
                    {
                        if (keyBinding.WhenTurnedOn)
                        {
                            if (keyBinding.OSKeyPress != null && !keyBinding.OSKeyPress.IsMultiSequenced())
                            {
                                TextBoxApButtonOn.Tag = keyBinding.OSKeyPress.LengthOfKeyPress();
                                TextBoxApButtonOn.Text = keyBinding.OSKeyPress.GetSimpleVirtualKeyCodesAsString();
                            }
                            else if (keyBinding.OSKeyPress != null && keyBinding.OSKeyPress.IsMultiSequenced())
                            {
                                TextBoxApButtonOn.Tag = keyBinding.OSKeyPress.GetSequence;
                                TextBoxApButtonOn.Text = keyBinding.OSKeyPress.Information;
                            }
                        }
                        else
                        {
                            if (keyBinding.OSKeyPress != null && !keyBinding.OSKeyPress.IsMultiSequenced())
                            {
                                TextBoxApButtonOff.Tag = keyBinding.OSKeyPress.LengthOfKeyPress();
                                TextBoxApButtonOff.Text = keyBinding.OSKeyPress.GetSimpleVirtualKeyCodesAsString();
                            }
                            else if (keyBinding.OSKeyPress != null && keyBinding.OSKeyPress.IsMultiSequenced())
                            {
                                TextBoxApButtonOff.Tag = keyBinding.OSKeyPress.GetSequence;
                                TextBoxApButtonOff.Text = keyBinding.OSKeyPress.Information;
                            }
                        }
                    }
                    if (keyBinding.MultiPanelPZ70Knob == MultiPanelPZ70Knobs.HDG_BUTTON)
                    {
                        if (keyBinding.WhenTurnedOn)
                        {
                            if (keyBinding.OSKeyPress != null && !keyBinding.OSKeyPress.IsMultiSequenced())
                            {
                                TextBoxHdgButtonOn.Tag = keyBinding.OSKeyPress.LengthOfKeyPress();
                                TextBoxHdgButtonOn.Text = keyBinding.OSKeyPress.GetSimpleVirtualKeyCodesAsString();
                            }
                            else if (keyBinding.OSKeyPress != null && keyBinding.OSKeyPress.IsMultiSequenced())
                            {
                                TextBoxHdgButtonOn.Tag = keyBinding.OSKeyPress.GetSequence;
                                TextBoxHdgButtonOn.Text = keyBinding.OSKeyPress.Information;
                            }
                        }
                        else
                        {
                            if (keyBinding.OSKeyPress != null && !keyBinding.OSKeyPress.IsMultiSequenced())
                            {
                                TextBoxHdgButtonOff.Tag = keyBinding.OSKeyPress.LengthOfKeyPress();
                                TextBoxHdgButtonOff.Text = keyBinding.OSKeyPress.GetSimpleVirtualKeyCodesAsString();
                            }
                            else if (keyBinding.OSKeyPress != null && keyBinding.OSKeyPress.IsMultiSequenced())
                            {
                                TextBoxHdgButtonOff.Tag = keyBinding.OSKeyPress.GetSequence;
                                TextBoxHdgButtonOff.Text = keyBinding.OSKeyPress.Information;
                            }
                        }
                    }
                    if (keyBinding.MultiPanelPZ70Knob == MultiPanelPZ70Knobs.NAV_BUTTON)
                    {
                        if (keyBinding.WhenTurnedOn)
                        {
                            if (keyBinding.OSKeyPress != null && !keyBinding.OSKeyPress.IsMultiSequenced())
                            {
                                TextBoxNavButtonOn.Tag = keyBinding.OSKeyPress.LengthOfKeyPress();
                                TextBoxNavButtonOn.Text = keyBinding.OSKeyPress.GetSimpleVirtualKeyCodesAsString();
                            }
                            else if (keyBinding.OSKeyPress != null && keyBinding.OSKeyPress.IsMultiSequenced())
                            {
                                TextBoxNavButtonOn.Tag = keyBinding.OSKeyPress.GetSequence;
                                TextBoxNavButtonOn.Text = keyBinding.OSKeyPress.Information;
                            }
                        }
                        else
                        {
                            if (keyBinding.OSKeyPress != null && !keyBinding.OSKeyPress.IsMultiSequenced())
                            {
                                TextBoxNavButtonOff.Tag = keyBinding.OSKeyPress.LengthOfKeyPress();
                                TextBoxNavButtonOff.Text = keyBinding.OSKeyPress.GetSimpleVirtualKeyCodesAsString();
                            }
                            else if (keyBinding.OSKeyPress != null && keyBinding.OSKeyPress.IsMultiSequenced())
                            {
                                TextBoxNavButtonOff.Tag = keyBinding.OSKeyPress.GetSequence;
                                TextBoxNavButtonOff.Text = keyBinding.OSKeyPress.Information;
                            }
                        }
                    }
                    if (keyBinding.MultiPanelPZ70Knob == MultiPanelPZ70Knobs.IAS_BUTTON)
                    {
                        if (keyBinding.WhenTurnedOn)
                        {
                            if (keyBinding.OSKeyPress != null && !keyBinding.OSKeyPress.IsMultiSequenced())
                            {
                                TextBoxIasButtonOn.Tag = keyBinding.OSKeyPress.LengthOfKeyPress();
                                TextBoxIasButtonOn.Text = keyBinding.OSKeyPress.GetSimpleVirtualKeyCodesAsString();
                            }
                            else if (keyBinding.OSKeyPress != null && keyBinding.OSKeyPress.IsMultiSequenced())
                            {
                                TextBoxIasButtonOn.Tag = keyBinding.OSKeyPress.GetSequence;
                                TextBoxIasButtonOn.Text = keyBinding.OSKeyPress.Information;
                            }
                        }
                        else
                        {
                            if (keyBinding.OSKeyPress != null && !keyBinding.OSKeyPress.IsMultiSequenced())
                            {
                                TextBoxIasButtonOff.Tag = keyBinding.OSKeyPress.LengthOfKeyPress();
                                TextBoxIasButtonOff.Text = keyBinding.OSKeyPress.GetSimpleVirtualKeyCodesAsString();
                            }
                            else if (keyBinding.OSKeyPress != null && keyBinding.OSKeyPress.IsMultiSequenced())
                            {
                                TextBoxIasButtonOff.Tag = keyBinding.OSKeyPress.GetSequence;
                                TextBoxIasButtonOff.Text = keyBinding.OSKeyPress.Information;
                            }
                        }
                    }
                    if (keyBinding.MultiPanelPZ70Knob == MultiPanelPZ70Knobs.ALT_BUTTON)
                    {
                        if (keyBinding.WhenTurnedOn)
                        {
                            if (keyBinding.OSKeyPress != null && !keyBinding.OSKeyPress.IsMultiSequenced())
                            {
                                TextBoxAltButtonOn.Tag = keyBinding.OSKeyPress.LengthOfKeyPress();
                                TextBoxAltButtonOn.Text = keyBinding.OSKeyPress.GetSimpleVirtualKeyCodesAsString();
                            }
                            else if (keyBinding.OSKeyPress != null && keyBinding.OSKeyPress.IsMultiSequenced())
                            {
                                TextBoxAltButtonOn.Tag = keyBinding.OSKeyPress.GetSequence;
                                TextBoxAltButtonOn.Text = keyBinding.OSKeyPress.Information;
                            }
                        }
                        else
                        {
                            if (keyBinding.OSKeyPress != null && !keyBinding.OSKeyPress.IsMultiSequenced())
                            {
                                TextBoxAltButtonOff.Tag = keyBinding.OSKeyPress.LengthOfKeyPress();
                                TextBoxAltButtonOff.Text = keyBinding.OSKeyPress.GetSimpleVirtualKeyCodesAsString();
                            }
                            else if (keyBinding.OSKeyPress != null && keyBinding.OSKeyPress.IsMultiSequenced())
                            {
                                TextBoxAltButtonOff.Tag = keyBinding.OSKeyPress.GetSequence;
                                TextBoxAltButtonOff.Text = keyBinding.OSKeyPress.Information;
                            }
                        }
                    }
                    if (keyBinding.MultiPanelPZ70Knob == MultiPanelPZ70Knobs.VS_BUTTON)
                    {
                        if (keyBinding.WhenTurnedOn)
                        {
                            if (keyBinding.OSKeyPress != null && !keyBinding.OSKeyPress.IsMultiSequenced())
                            {
                                TextBoxVsButtonOn.Tag = keyBinding.OSKeyPress.LengthOfKeyPress();
                                TextBoxVsButtonOn.Text = keyBinding.OSKeyPress.GetSimpleVirtualKeyCodesAsString();
                            }
                            else if (keyBinding.OSKeyPress != null && keyBinding.OSKeyPress.IsMultiSequenced())
                            {
                                TextBoxVsButtonOn.Tag = keyBinding.OSKeyPress.GetSequence;
                                TextBoxVsButtonOn.Text = keyBinding.OSKeyPress.Information;
                            }
                        }
                        else
                        {
                            if (keyBinding.OSKeyPress != null && !keyBinding.OSKeyPress.IsMultiSequenced())
                            {
                                TextBoxVsButtonOff.Tag = keyBinding.OSKeyPress.LengthOfKeyPress();
                                TextBoxVsButtonOff.Text = keyBinding.OSKeyPress.GetSimpleVirtualKeyCodesAsString();
                            }
                            else if (keyBinding.OSKeyPress != null && keyBinding.OSKeyPress.IsMultiSequenced())
                            {
                                TextBoxVsButtonOff.Tag = keyBinding.OSKeyPress.GetSequence;
                                TextBoxVsButtonOff.Text = keyBinding.OSKeyPress.Information;
                            }
                        }
                    }
                    if (keyBinding.MultiPanelPZ70Knob == MultiPanelPZ70Knobs.APR_BUTTON)
                    {
                        if (keyBinding.WhenTurnedOn)
                        {
                            if (keyBinding.OSKeyPress != null && !keyBinding.OSKeyPress.IsMultiSequenced())
                            {
                                TextBoxAprButtonOn.Tag = keyBinding.OSKeyPress.LengthOfKeyPress();
                                TextBoxAprButtonOn.Text = keyBinding.OSKeyPress.GetSimpleVirtualKeyCodesAsString();
                            }
                            else if (keyBinding.OSKeyPress != null && keyBinding.OSKeyPress.IsMultiSequenced())
                            {
                                TextBoxAprButtonOn.Tag = keyBinding.OSKeyPress.GetSequence;
                                TextBoxAprButtonOn.Text = keyBinding.OSKeyPress.Information;
                            }
                        }
                        else
                        {
                            if (keyBinding.OSKeyPress != null && !keyBinding.OSKeyPress.IsMultiSequenced())
                            {
                                TextBoxAprButtonOff.Tag = keyBinding.OSKeyPress.LengthOfKeyPress();
                                TextBoxAprButtonOff.Text = keyBinding.OSKeyPress.GetSimpleVirtualKeyCodesAsString();
                            }
                            else if (keyBinding.OSKeyPress != null && keyBinding.OSKeyPress.IsMultiSequenced())
                            {
                                TextBoxAprButtonOff.Tag = keyBinding.OSKeyPress.GetSequence;
                                TextBoxAprButtonOff.Text = keyBinding.OSKeyPress.Information;
                            }
                        }
                    }
                    if (keyBinding.MultiPanelPZ70Knob == MultiPanelPZ70Knobs.REV_BUTTON)
                    {
                        if (keyBinding.WhenTurnedOn)
                        {
                            if (keyBinding.OSKeyPress != null && !keyBinding.OSKeyPress.IsMultiSequenced())
                            {
                                TextBoxRevButtonOn.Tag = keyBinding.OSKeyPress.LengthOfKeyPress();
                                TextBoxRevButtonOn.Text = keyBinding.OSKeyPress.GetSimpleVirtualKeyCodesAsString();
                            }
                            else if (keyBinding.OSKeyPress != null && keyBinding.OSKeyPress.IsMultiSequenced())
                            {
                                TextBoxRevButtonOn.Tag = keyBinding.OSKeyPress.GetSequence;
                                TextBoxRevButtonOn.Text = keyBinding.OSKeyPress.Information;
                            }
                        }
                        else
                        {
                            if (keyBinding.OSKeyPress != null && !keyBinding.OSKeyPress.IsMultiSequenced())
                            {
                                TextBoxRevButtonOff.Tag = keyBinding.OSKeyPress.LengthOfKeyPress();
                                TextBoxRevButtonOff.Text = keyBinding.OSKeyPress.GetSimpleVirtualKeyCodesAsString();
                            }
                            else if (keyBinding.OSKeyPress != null && keyBinding.OSKeyPress.IsMultiSequenced())
                            {
                                TextBoxRevButtonOff.Tag = keyBinding.OSKeyPress.GetSequence;
                                TextBoxRevButtonOff.Text = keyBinding.OSKeyPress.Information;
                            }
                        }
                    }
                }





                foreach (var dcsBiosBinding in _multiPanelPZ70.DCSBiosBindings)
                {
                    if (dcsBiosBinding.MultiPanelPZ70Knob == MultiPanelPZ70Knobs.LCD_WHEEL_DEC && dcsBiosBinding.WhenTurnedOn && dcsBiosBinding.DCSBIOSInputs.Count > 0)
                    {
                        TextBoxLcdKnobDecrease.Tag = dcsBiosBinding.DCSBIOSInputs;
                        TextBoxLcdKnobDecrease.Text = dcsBiosBinding.Description;
                        TextBoxLcdKnobDecrease.ToolTip = "DCS-BIOS";
                    }
                    if (dcsBiosBinding.MultiPanelPZ70Knob == MultiPanelPZ70Knobs.LCD_WHEEL_INC && dcsBiosBinding.WhenTurnedOn && dcsBiosBinding.DCSBIOSInputs.Count > 0)
                    {
                        TextBoxLcdKnobIncrease.Tag = dcsBiosBinding.DCSBIOSInputs;
                        TextBoxLcdKnobIncrease.Text = dcsBiosBinding.Description;
                        TextBoxLcdKnobIncrease.ToolTip = "DCS-BIOS";
                    }
                    if (dcsBiosBinding.MultiPanelPZ70Knob == MultiPanelPZ70Knobs.FLAPS_LEVER_UP && dcsBiosBinding.WhenTurnedOn && dcsBiosBinding.DCSBIOSInputs.Count > 0)
                    {
                        TextBoxFlapsUp.Tag = dcsBiosBinding.DCSBIOSInputs;
                        TextBoxFlapsUp.Text = dcsBiosBinding.Description;
                        TextBoxFlapsUp.ToolTip = "DCS-BIOS";
                    }
                    if (dcsBiosBinding.MultiPanelPZ70Knob == MultiPanelPZ70Knobs.FLAPS_LEVER_DOWN && dcsBiosBinding.WhenTurnedOn && dcsBiosBinding.DCSBIOSInputs.Count > 0)
                    {
                        TextBoxFlapsDown.Tag = dcsBiosBinding.DCSBIOSInputs;
                        TextBoxFlapsDown.Text = dcsBiosBinding.Description;
                        TextBoxFlapsDown.ToolTip = "DCS-BIOS";
                    }
                    if (dcsBiosBinding.MultiPanelPZ70Knob == MultiPanelPZ70Knobs.PITCH_TRIM_WHEEL_UP && dcsBiosBinding.WhenTurnedOn && dcsBiosBinding.DCSBIOSInputs.Count > 0)
                    {
                        TextBoxPitchTrimUp.Tag = dcsBiosBinding.DCSBIOSInputs;
                        TextBoxPitchTrimUp.Text = dcsBiosBinding.Description;
                        TextBoxPitchTrimUp.ToolTip = "DCS-BIOS";
                    }
                    if (dcsBiosBinding.MultiPanelPZ70Knob == MultiPanelPZ70Knobs.PITCH_TRIM_WHEEL_DOWN && dcsBiosBinding.WhenTurnedOn && dcsBiosBinding.DCSBIOSInputs.Count > 0)
                    {
                        TextBoxPitchTrimDown.Tag = dcsBiosBinding.DCSBIOSInputs;
                        TextBoxPitchTrimDown.Text = dcsBiosBinding.Description;
                        TextBoxPitchTrimDown.ToolTip = "DCS-BIOS";
                    }

                    if (dcsBiosBinding.MultiPanelPZ70Knob == MultiPanelPZ70Knobs.AUTO_THROTTLE)
                    {
                        if (dcsBiosBinding.WhenTurnedOn)
                        {
                            if (dcsBiosBinding.DCSBIOSInputs.Count > 0)
                            {
                                TextBoxAutoThrottleOn.Tag = dcsBiosBinding.DCSBIOSInputs;
                                TextBoxAutoThrottleOn.Text = dcsBiosBinding.Description;
                                TextBoxAutoThrottleOn.ToolTip = "DCS-BIOS";
                            }
                        }
                        else
                        {
                            if (dcsBiosBinding.DCSBIOSInputs.Count > 0)
                            {
                                TextBoxAutoThrottleOff.Tag = dcsBiosBinding.DCSBIOSInputs;
                                TextBoxAutoThrottleOff.Text = dcsBiosBinding.Description;
                                TextBoxAutoThrottleOff.ToolTip = "DCS-BIOS";
                            }
                        }
                    }
                    if (dcsBiosBinding.MultiPanelPZ70Knob == MultiPanelPZ70Knobs.AP_BUTTON)
                    {
                        if (dcsBiosBinding.WhenTurnedOn)
                        {
                            if (dcsBiosBinding.DCSBIOSInputs.Count > 0)
                            {
                                TextBoxApButtonOn.Tag = dcsBiosBinding.DCSBIOSInputs;
                                TextBoxApButtonOn.Text = dcsBiosBinding.Description;
                                TextBoxApButtonOn.ToolTip = "DCS-BIOS";
                            }
                        }
                        else
                        {
                            if (dcsBiosBinding.DCSBIOSInputs.Count > 0)
                            {
                                TextBoxApButtonOff.Tag = dcsBiosBinding.DCSBIOSInputs;
                                TextBoxApButtonOff.Text = dcsBiosBinding.Description;
                                TextBoxApButtonOff.ToolTip = "DCS-BIOS";
                            }
                        }
                    }
                    if (dcsBiosBinding.MultiPanelPZ70Knob == MultiPanelPZ70Knobs.HDG_BUTTON)
                    {
                        if (dcsBiosBinding.WhenTurnedOn)
                        {
                            if (dcsBiosBinding.DCSBIOSInputs.Count > 0)
                            {
                                TextBoxHdgButtonOn.Tag = dcsBiosBinding.DCSBIOSInputs;
                                TextBoxHdgButtonOn.Text = dcsBiosBinding.Description;
                                TextBoxHdgButtonOn.ToolTip = "DCS-BIOS";
                            }
                        }
                        else
                        {
                            if (dcsBiosBinding.DCSBIOSInputs.Count > 0)
                            {
                                TextBoxHdgButtonOff.Tag = dcsBiosBinding.DCSBIOSInputs;
                                TextBoxHdgButtonOff.Text = dcsBiosBinding.Description;
                                TextBoxHdgButtonOff.ToolTip = "DCS-BIOS";
                            }
                        }
                    }
                    if (dcsBiosBinding.MultiPanelPZ70Knob == MultiPanelPZ70Knobs.NAV_BUTTON)
                    {
                        if (dcsBiosBinding.WhenTurnedOn)
                        {
                            if (dcsBiosBinding.DCSBIOSInputs.Count > 0)
                            {
                                TextBoxNavButtonOn.Tag = dcsBiosBinding.DCSBIOSInputs;
                                TextBoxNavButtonOn.Text = dcsBiosBinding.Description;
                                TextBoxNavButtonOn.ToolTip = "DCS-BIOS";
                            }
                        }
                        else
                        {
                            if (dcsBiosBinding.DCSBIOSInputs.Count > 0)
                            {
                                TextBoxNavButtonOff.Tag = dcsBiosBinding.DCSBIOSInputs;
                                TextBoxNavButtonOff.Text = dcsBiosBinding.Description;
                                TextBoxNavButtonOff.ToolTip = "DCS-BIOS";
                            }
                        }
                    }
                    if (dcsBiosBinding.MultiPanelPZ70Knob == MultiPanelPZ70Knobs.IAS_BUTTON)
                    {
                        if (dcsBiosBinding.WhenTurnedOn)
                        {
                            if (dcsBiosBinding.DCSBIOSInputs.Count > 0)
                            {
                                TextBoxIasButtonOn.Tag = dcsBiosBinding.DCSBIOSInputs;
                                TextBoxIasButtonOn.Text = dcsBiosBinding.Description;
                                TextBoxIasButtonOn.ToolTip = "DCS-BIOS";
                            }
                        }
                        else
                        {
                            if (dcsBiosBinding.DCSBIOSInputs.Count > 0)
                            {
                                TextBoxIasButtonOff.Tag = dcsBiosBinding.DCSBIOSInputs;
                                TextBoxIasButtonOff.Text = dcsBiosBinding.Description;
                                TextBoxIasButtonOff.ToolTip = "DCS-BIOS";
                            }
                        }
                    }
                    if (dcsBiosBinding.MultiPanelPZ70Knob == MultiPanelPZ70Knobs.ALT_BUTTON)
                    {
                        if (dcsBiosBinding.WhenTurnedOn)
                        {
                            if (dcsBiosBinding.DCSBIOSInputs.Count > 0)
                            {
                                TextBoxAltButtonOn.Tag = dcsBiosBinding.DCSBIOSInputs;
                                TextBoxAltButtonOn.Text = dcsBiosBinding.Description;
                                TextBoxAltButtonOn.ToolTip = "DCS-BIOS";
                            }
                        }
                        else
                        {
                            if (dcsBiosBinding.DCSBIOSInputs.Count > 0)
                            {
                                TextBoxAltButtonOff.Tag = dcsBiosBinding.DCSBIOSInputs;
                                TextBoxAltButtonOff.Text = dcsBiosBinding.Description;
                                TextBoxAltButtonOff.ToolTip = "DCS-BIOS";
                            }
                        }
                    }
                    if (dcsBiosBinding.MultiPanelPZ70Knob == MultiPanelPZ70Knobs.VS_BUTTON)
                    {
                        if (dcsBiosBinding.WhenTurnedOn)
                        {
                            if (dcsBiosBinding.DCSBIOSInputs.Count > 0)
                            {
                                TextBoxVsButtonOn.Tag = dcsBiosBinding.DCSBIOSInputs;
                                TextBoxVsButtonOn.Text = dcsBiosBinding.Description;
                                TextBoxVsButtonOn.ToolTip = "DCS-BIOS";
                            }
                        }
                        else
                        {
                            if (dcsBiosBinding.DCSBIOSInputs.Count > 0)
                            {
                                TextBoxVsButtonOff.Tag = dcsBiosBinding.DCSBIOSInputs;
                                TextBoxVsButtonOff.Text = dcsBiosBinding.Description;
                                TextBoxVsButtonOff.ToolTip = "DCS-BIOS";
                            }
                        }
                    }
                    if (dcsBiosBinding.MultiPanelPZ70Knob == MultiPanelPZ70Knobs.APR_BUTTON)
                    {
                        if (dcsBiosBinding.WhenTurnedOn)
                        {
                            if (dcsBiosBinding.DCSBIOSInputs.Count > 0)
                            {
                                TextBoxAprButtonOn.Tag = dcsBiosBinding.DCSBIOSInputs;
                                TextBoxAprButtonOn.Text = dcsBiosBinding.Description;
                                TextBoxAprButtonOn.ToolTip = "DCS-BIOS";
                            }
                        }
                        else
                        {
                            if (dcsBiosBinding.DCSBIOSInputs.Count > 0)
                            {
                                TextBoxAprButtonOff.Tag = dcsBiosBinding.DCSBIOSInputs;
                                TextBoxAprButtonOff.Text = dcsBiosBinding.Description;
                                TextBoxAprButtonOff.ToolTip = "DCS-BIOS";
                            }
                        }
                    }
                    if (dcsBiosBinding.MultiPanelPZ70Knob == MultiPanelPZ70Knobs.REV_BUTTON)
                    {
                        if (dcsBiosBinding.WhenTurnedOn)
                        {
                            if (dcsBiosBinding.DCSBIOSInputs.Count > 0)
                            {
                                TextBoxRevButtonOn.Tag = dcsBiosBinding.DCSBIOSInputs;
                                TextBoxRevButtonOn.Text = dcsBiosBinding.Description;
                                TextBoxRevButtonOn.ToolTip = "DCS-BIOS";
                            }
                        }
                        else
                        {
                            if (dcsBiosBinding.DCSBIOSInputs.Count > 0)
                            {
                                TextBoxRevButtonOff.Tag = dcsBiosBinding.DCSBIOSInputs;
                                TextBoxRevButtonOff.Text = dcsBiosBinding.Description;
                                TextBoxRevButtonOff.ToolTip = "DCS-BIOS";
                            }
                        }
                    }
                }


                ImageAltUpperRow.Visibility = Visibility.Collapsed;
                ImageAltLowerRow.Visibility = Visibility.Collapsed;
                ImageVsUpperRow.Visibility = Visibility.Collapsed;
                ImageVsLowerRow.Visibility = Visibility.Collapsed;
                foreach (var dcsBiosBindingLCD in _multiPanelPZ70.LCDBindings)
                {
                    if (dcsBiosBindingLCD.MultiPanelPZ70Knob == MultiPanelPZ70Knobs.KNOB_ALT && dcsBiosBindingLCD.PZ70LCDPosition == PZ70LCDPosition.UpperALT && dcsBiosBindingLCD.HasBinding)
                    {
                        ImageAltUpperRow.Visibility = Visibility.Visible;
                        if (dcsBiosBindingLCD.UseFormula)
                        {
                            ButtonKnobAltLcdUpper.Tag = dcsBiosBindingLCD.DCSBIOSOutputFormulaObject;
                            ButtonKnobAltLcdUpper.ToolTip = dcsBiosBindingLCD.DCSBIOSOutputFormulaObject.ToString();
                        }
                        else
                        {
                            ButtonKnobAltLcdUpper.Tag = dcsBiosBindingLCD.DCSBIOSOutputObject;
                            ButtonKnobAltLcdUpper.ToolTip = dcsBiosBindingLCD.DCSBIOSOutputObject.ToString();
                        }
                    }
                    if (dcsBiosBindingLCD.MultiPanelPZ70Knob == MultiPanelPZ70Knobs.KNOB_ALT && dcsBiosBindingLCD.PZ70LCDPosition == PZ70LCDPosition.LowerALT && dcsBiosBindingLCD.HasBinding)
                    {
                        ImageAltLowerRow.Visibility = Visibility.Visible;
                        if (dcsBiosBindingLCD.UseFormula)
                        {
                            ButtonKnobAltLcdLower.Tag = dcsBiosBindingLCD.DCSBIOSOutputFormulaObject;
                            ButtonKnobAltLcdLower.ToolTip = dcsBiosBindingLCD.DCSBIOSOutputFormulaObject.ToString();
                        }
                        else
                        {
                            ButtonKnobAltLcdLower.Tag = dcsBiosBindingLCD.DCSBIOSOutputObject;
                            ButtonKnobAltLcdLower.ToolTip = dcsBiosBindingLCD.DCSBIOSOutputObject.ToString();
                        }
                    }
                    if (dcsBiosBindingLCD.MultiPanelPZ70Knob == MultiPanelPZ70Knobs.KNOB_VS && dcsBiosBindingLCD.PZ70LCDPosition == PZ70LCDPosition.UpperVS && dcsBiosBindingLCD.HasBinding)
                    {
                        ImageVsUpperRow.Visibility = Visibility.Visible;
                        if (dcsBiosBindingLCD.UseFormula)
                        {
                            ButtonKnobVsLcdUpper.Tag = dcsBiosBindingLCD.DCSBIOSOutputFormulaObject;
                            ButtonKnobVsLcdUpper.ToolTip = dcsBiosBindingLCD.DCSBIOSOutputFormulaObject.ToString();
                        }
                        else
                        {
                            ButtonKnobVsLcdUpper.Tag = dcsBiosBindingLCD.DCSBIOSOutputObject;
                            ButtonKnobVsLcdUpper.ToolTip = dcsBiosBindingLCD.DCSBIOSOutputObject.ToString();
                        }
                    }
                    if (dcsBiosBindingLCD.MultiPanelPZ70Knob == MultiPanelPZ70Knobs.KNOB_VS && dcsBiosBindingLCD.PZ70LCDPosition == PZ70LCDPosition.LowerVS && dcsBiosBindingLCD.HasBinding)
                    {
                        ImageVsLowerRow.Visibility = Visibility.Visible;
                        if (dcsBiosBindingLCD.UseFormula)
                        {
                            ButtonKnobVsLcdLower.Tag = dcsBiosBindingLCD.DCSBIOSOutputFormulaObject;
                            ButtonKnobVsLcdLower.ToolTip = dcsBiosBindingLCD.DCSBIOSOutputFormulaObject.ToString();
                        }
                        else
                        {
                            ButtonKnobVsLcdLower.Tag = dcsBiosBindingLCD.DCSBIOSOutputObject;
                            ButtonKnobVsLcdLower.ToolTip = dcsBiosBindingLCD.DCSBIOSOutputObject.ToString();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(993013, ex);
            }
        }

        private void TextBoxShortcutKeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                var textBox = ((TextBox)sender);
                //Check if this textbox contains sequence or DCS-BIOS information. If so then exit
                if (textBox.Tag != null && (textBox.Tag is SortedList<int, KeyPressInfo> || textBox.Tag is List<DCSBIOSInput>))
                {
                    return;
                }
                if (textBox.Tag == null)
                {
                    textBox.Tag = KeyPressLength.FiftyMilliSec;
                }
                var keyPressed = (VirtualKeyCode)KeyInterop.VirtualKeyFromKey(e.Key);
                e.Handled = true;

                var hashSetOfKeysPressed = new HashSet<string>();
                hashSetOfKeysPressed.Add(Enum.GetName(typeof(VirtualKeyCode), keyPressed));

                var modifiers = Common.GetPressedVirtualKeyCodesThatAreModifiers();
                foreach (var virtualKeyCode in modifiers)
                {
                    hashSetOfKeysPressed.Add(Enum.GetName(typeof(VirtualKeyCode), virtualKeyCode));
                }
                var result = "";
                foreach (var str in hashSetOfKeysPressed)
                {
                    if (!string.IsNullOrEmpty(result))
                    {
                        result = str + " + " + result;
                    }
                    else
                    {
                        result = str + " " + result;
                    }
                }
                textBox.Text = result;
                UpdateKeyBindingProfileSequencedKeyStrokesPZ70(textBox);
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(993008, ex);
            }
        }

        private void TextBoxMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                var textBox = (TextBox)sender;

                if (e.ChangedButton == MouseButton.Left)
                {

                    //Check if this textbox contains DCS-BIOS information. If so then prompt the user for deletion
                    if (textBox.Tag != null && textBox.Tag is List<DCSBIOSInput>)
                    {
                        if (MessageBox.Show("Do you want to delete the DCS-BIOS configuration?", "Delete DCS-BIOS control?", MessageBoxButton.OKCancel, MessageBoxImage.Question) != MessageBoxResult.OK)
                        {
                            return;
                        }
                        textBox.ToolTip = null;
                        textBox.Text = "";
                        _multiPanelPZ70.ClearAllBindings(GetPZ70Knob(textBox));
                        textBox.Tag = null;
                    }
                    //Check if this textbox contains sequence information. If so then prompt the user for deletion
                    else if (textBox.Tag != null && textBox.Tag is SortedList<int, KeyPressInfo>)
                    {
                        if (MessageBox.Show("Do you want to delete the key sequence?", "Delete key sequence?", MessageBoxButton.OKCancel, MessageBoxImage.Question) != MessageBoxResult.OK)
                        {
                            return;
                        }
                        textBox.ToolTip = null;
                        textBox.Text = "";
                        textBox.Tag = null;
                        UpdateKeyBindingProfileSimpleKeyStrokes(textBox);
                    }
                    else
                    {
                        textBox.ToolTip = null;
                        textBox.Text = "";
                        textBox.Tag = null;
                        UpdateKeyBindingProfileSimpleKeyStrokes(textBox);
                    }
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(3001, ex);
            }
        }

        private void TextBoxGotFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                ((TextBox)sender).Background = Brushes.Yellow;
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(993004, ex);
            }
        }

        private void TextBoxPreviewKeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                var textBox = ((TextBox)sender);

                //Check if this textbox contains sequence or DCS-BIOS information. If so then exit
                if (textBox.Tag != null && (textBox.Tag is SortedList<int, KeyPressInfo> || textBox.Tag is List<DCSBIOSInput>))
                {
                    return;
                }
                var hashSetOfKeysPressed = new HashSet<string>();

                if (textBox.Tag == null)
                {
                    textBox.Tag = KeyPressLength.FiftyMilliSec;
                }

                var keyCode = KeyInterop.VirtualKeyFromKey(e.Key);
                e.Handled = true;

                if (keyCode > 0)
                {
                    //Common.DebugP("Pressed key is " + keyCode);
                    hashSetOfKeysPressed.Add(Enum.GetName(typeof(VirtualKeyCode), keyCode));
                }
                var modifiers = Common.GetPressedVirtualKeyCodesThatAreModifiers();
                foreach (var virtualKeyCode in modifiers)
                {
                    //Common.DebugP("Pressed modifiers -->  " + virtualKeyCode);
                    hashSetOfKeysPressed.Add(Enum.GetName(typeof(VirtualKeyCode), virtualKeyCode));
                }
                var result = "";
                foreach (var str in hashSetOfKeysPressed)
                {
                    if (!string.IsNullOrEmpty(result))
                    {
                        result = str + " + " + result;
                    }
                    else
                    {
                        result = str + " " + result;
                    }
                }
                textBox.Text = result;
                UpdateKeyBindingProfileSimpleKeyStrokes(textBox);
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(993006, ex);
            }
        }

        private void TextBoxTextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                //MAKE SURE THE TAG IS SET BEFORE SETTING TEXT! OTHERWISE THIS DOESN'T FIRE
                var textBox = (TextBox)sender;
                if (textBox.Tag is SortedList<int, KeyPressInfo>)
                {
                    textBox.FontStyle = FontStyles.Oblique;
                }
                else
                {
                    textBox.FontStyle = FontStyles.Normal;
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(993007, ex);
            }
        }

        private void MouseDownFocusLogTextBox(object sender, MouseButtonEventArgs e)
        {
            try
            {
                TextBoxLogPZ70.Focus();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(992014, ex);
            }
        }

        private void TextBoxContextMenuIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            try
            {
                if (!(bool)e.NewValue)
                {
                    //Do not show if not visible
                    return;
                }

                var textBox = GetTextBoxInFocus();
                var contextMenu = (ContextMenu)sender;
                if (textBox == null)
                {
                    throw new Exception("Failed to locate which textbox is focused.");
                }

                if (textBox.Tag == null || textBox.Tag is SortedList<int, KeyPressInfo> || textBox.Tag is List<DCSBIOSInput>)
                {
                    return;
                }
                var keyPressLength = (KeyPressLength)textBox.Tag;

                foreach (MenuItem item in contextMenu.Items)
                {
                    item.IsChecked = false;
                }

                foreach (MenuItem item in contextMenu.Items)
                {
                    /*if (item.Name == "contextMenuItemZero" && keyPressLength == KeyPressLength.Zero)
                    {
                        item.IsChecked = true;
                    }*/
                    if (item.Name == "contextMenuItemFiftyMilliSec" && keyPressLength == KeyPressLength.FiftyMilliSec)
                    {
                        item.IsChecked = true;
                    }
                    else if (item.Name == "contextMenuItemHalfSecond" && keyPressLength == KeyPressLength.HalfSecond)
                    {
                        item.IsChecked = true;
                    }
                    else if (item.Name == "contextMenuItemSecond" && keyPressLength == KeyPressLength.Second)
                    {
                        item.IsChecked = true;
                    }
                    else if (item.Name == "contextMenuItemSecondAndHalf" && keyPressLength == KeyPressLength.SecondAndHalf)
                    {
                        item.IsChecked = true;
                    }
                    else if (item.Name == "contextMenuItemTwoSeconds" && keyPressLength == KeyPressLength.TwoSeconds)
                    {
                        item.IsChecked = true;
                    }
                    else if (item.Name == "contextMenuItemThreeSeconds" && keyPressLength == KeyPressLength.ThreeSeconds)
                    {
                        item.IsChecked = true;
                    }
                    else if (item.Name == "contextMenuItemFourSeconds" && keyPressLength == KeyPressLength.FourSeconds)
                    {
                        item.IsChecked = true;
                    }
                    else if (item.Name == "contextMenuItemFiveSecs" && keyPressLength == KeyPressLength.FiveSecs)
                    {
                        item.IsChecked = true;
                    }
                    else if (item.Name == "contextMenuItemFifteenSecs" && keyPressLength == KeyPressLength.FifteenSecs)
                    {
                        item.IsChecked = true;
                    }
                    else if (item.Name == "contextMenuItemTenSecs" && keyPressLength == KeyPressLength.TenSecs)
                    {
                        item.IsChecked = true;
                    }
                    else if (item.Name == "contextMenuItemTwentySecs" && keyPressLength == KeyPressLength.TwentySecs)
                    {
                        item.IsChecked = true;
                    }
                    else if (item.Name == "contextMenuItemThirtySecs" && keyPressLength == KeyPressLength.ThirtySecs)
                    {
                        item.IsChecked = true;
                    }
                    else if (item.Name == "contextMenuItemFortySecs" && keyPressLength == KeyPressLength.FortySecs)
                    {
                        item.IsChecked = true;
                    }
                    else if (item.Name == "contextMenuItemSixtySecs" && keyPressLength == KeyPressLength.SixtySecs)
                    {
                        item.IsChecked = true;
                    }
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(992061, ex);
            }
        }


        private void TextBoxContextMenuClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var textBox = GetTextBoxInFocus();
                if (textBox == null)
                {
                    throw new Exception("Failed to locate which textbox is focused.");
                }

                var contextMenuItem = (MenuItem)sender;
                /*if(contextMenuItem.Name == "contextMenuItemZero")
                {
                    textBox.Tag = KeyPressLength.Zero;
                }*/
                if (contextMenuItem.Name == "contextMenuItemFiftyMilliSec")
                {
                    textBox.Tag = KeyPressLength.FiftyMilliSec;
                }
                else if (contextMenuItem.Name == "contextMenuItemHalfSecond")
                {
                    textBox.Tag = KeyPressLength.HalfSecond;
                }
                else if (contextMenuItem.Name == "contextMenuItemSecond")
                {
                    textBox.Tag = KeyPressLength.Second;
                }
                else if (contextMenuItem.Name == "contextMenuItemSecondAndHalf")
                {
                    textBox.Tag = KeyPressLength.SecondAndHalf;
                }
                else if (contextMenuItem.Name == "contextMenuItemTwoSeconds")
                {
                    textBox.Tag = KeyPressLength.TwoSeconds;
                }
                else if (contextMenuItem.Name == "contextMenuItemThreeSeconds")
                {
                    textBox.Tag = KeyPressLength.ThreeSeconds;
                }
                else if (contextMenuItem.Name == "contextMenuItemFourSeconds")
                {
                    textBox.Tag = KeyPressLength.FourSeconds;
                }
                else if (contextMenuItem.Name == "contextMenuItemFiveSecs")
                {
                    textBox.Tag = KeyPressLength.FiveSecs;
                }
                else if (contextMenuItem.Name == "contextMenuItemTenSecs")
                {
                    textBox.Tag = KeyPressLength.TenSecs;
                }
                else if (contextMenuItem.Name == "contextMenuItemFifteenSecs")
                {
                    textBox.Tag = KeyPressLength.FifteenSecs;
                }
                else if (contextMenuItem.Name == "contextMenuItemTwentySecs")
                {
                    textBox.Tag = KeyPressLength.TwentySecs;
                }
                else if (contextMenuItem.Name == "contextMenuItemThirtySecs")
                {
                    textBox.Tag = KeyPressLength.ThirtySecs;
                }
                else if (contextMenuItem.Name == "contextMenuItemFortySecs")
                {
                    textBox.Tag = KeyPressLength.FortySecs;
                }
                else if (contextMenuItem.Name == "contextMenuItemSixtySecs")
                {
                    textBox.Tag = KeyPressLength.SixtySecs;
                }

                UpdateKeyBindingProfileSimpleKeyStrokes(textBox);
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(992082, ex);
            }
        }

        private TextBox GetTextBoxInFocus()
        {
            foreach (var textBox in Common.FindVisualChildren<TextBox>(this))
            {
                if (!textBox.Equals(TextBoxLogPZ70) && textBox.IsFocused && textBox.Background == Brushes.Yellow)
                {
                    return textBox;
                }
            }
            return null;
        }


        private void MenuContextEditTextBoxClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var textBox = GetTextBoxInFocus();
                if (textBox == null)
                {
                    throw new Exception("Failed to locate which textbox is focused.");
                }
                SequenceWindow sequenceWindow;
                if (textBox.Tag is SortedList<int, KeyPressInfo>)
                {
                    sequenceWindow = new SequenceWindow(textBox.Text, (SortedList<int, KeyPressInfo>)textBox.Tag);
                }
                else
                {
                    sequenceWindow = new SequenceWindow();
                }
                sequenceWindow.ShowDialog();
                if (sequenceWindow.DialogResult.HasValue && sequenceWindow.DialogResult.Value)
                {
                    //Clicked OK
                    //If the user added only a single key stroke combo then let's not treat this as a sequence
                    if (!sequenceWindow.IsDirty)
                    {
                        //User made no changes
                        return;
                    }
                    var sequenceList = sequenceWindow.GetSequence;
                    textBox.ToolTip = null;
                    if (sequenceList.Count > 1)
                    {
                        textBox.Tag = sequenceList;
                        textBox.Text = string.IsNullOrEmpty(sequenceWindow.GetInformation) ? "Key press sequence" : sequenceWindow.GetInformation;
                        if (!string.IsNullOrEmpty(sequenceWindow.GetInformation))
                        {
                            var toolTip = new ToolTip { Content = sequenceWindow.GetInformation };
                            textBox.ToolTip = toolTip;
                        }
                        UpdateKeyBindingProfileSequencedKeyStrokesPZ70(textBox);
                    }
                    else
                    {
                        //If only one press was created treat it as a simple keypress
                        textBox.Tag = sequenceList.Values[0].LengthOfKeyPress;
                        textBox.Text = sequenceList.Values[0].VirtualKeyCodesAsString;
                        if (!string.IsNullOrEmpty(sequenceWindow.GetInformation))
                        {
                            var toolTip = new ToolTip { Content = sequenceWindow.GetInformation };
                            textBox.ToolTip = toolTip;
                        }
                        UpdateKeyBindingProfileSimpleKeyStrokes(textBox);
                    }
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(2044, ex);
            }
        }

        private void ContextMenuItemEditDCSBIOS_OnContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            try
            {
                var contextMenu = (ContextMenu)sender;
                foreach (MenuItem item in contextMenu.Items)
                {
                    item.IsEnabled = !_multiPanelPZ70.KeyboardEmulationOnly;
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(204165, ex);
            }
        }

        private void MenuContextEditDCSBIOSControlTextBoxClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var textBox = GetTextBoxInFocus();
                if (textBox == null)
                {
                    throw new Exception("Failed to locate which textbox is focused.");
                }
                DCSBIOSControlsConfigsWindow dcsBIOSControlsConfigsWindow;
                if (textBox.Tag is List<DCSBIOSInput>)
                {
                    dcsBIOSControlsConfigsWindow = new DCSBIOSControlsConfigsWindow(_globalHandler.GetAirframe(), textBox.Name.Replace("TextBox", ""), (List<DCSBIOSInput>)textBox.Tag, textBox.Text);
                }
                else
                {
                    dcsBIOSControlsConfigsWindow = new DCSBIOSControlsConfigsWindow(_globalHandler.GetAirframe(), textBox.Name.Replace("TextBox", ""), null);
                }
                dcsBIOSControlsConfigsWindow.ShowDialog();
                if (dcsBIOSControlsConfigsWindow.DialogResult.HasValue && dcsBIOSControlsConfigsWindow.DialogResult == true && dcsBIOSControlsConfigsWindow.DCSBIOSInputs.Count > 0)
                {
                    var dcsBiosInputs = dcsBIOSControlsConfigsWindow.DCSBIOSInputs;
                    var text = string.IsNullOrWhiteSpace(dcsBIOSControlsConfigsWindow.Description) ? "DCS-BIOS" : dcsBIOSControlsConfigsWindow.Description;
                    //1 appropriate text to textbox
                    //2 update bindings
                    textBox.Text = text;
                    textBox.Tag = dcsBiosInputs;
                    textBox.ToolTip = textBox.Text;
                    UpdateDCSBIOSBinding(textBox);
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(442044, ex);
            }
        }

        private void UpdateKeyBindingProfileSequencedKeyStrokesPZ70(TextBox textBox)
        {
            try
            {
                if (textBox.Tag == null)
                {
                    textBox.Tag = new SortedList<int, KeyPressInfo>();
                }

                if (textBox.Equals(TextBoxLcdKnobDecrease))
                {
                    _multiPanelPZ70.AddOrUpdateSequencedKeyBinding(textBox.Text, MultiPanelPZ70Knobs.LCD_WHEEL_DEC, (SortedList<int, KeyPressInfo>)textBox.Tag);
                }
                if (textBox.Equals(TextBoxLcdKnobIncrease))
                {
                    _multiPanelPZ70.AddOrUpdateSequencedKeyBinding(textBox.Text, MultiPanelPZ70Knobs.LCD_WHEEL_INC, (SortedList<int, KeyPressInfo>)textBox.Tag);
                }
                if (textBox.Equals(TextBoxAutoThrottleOff))
                {
                    _multiPanelPZ70.AddOrUpdateSequencedKeyBinding(textBox.Text, MultiPanelPZ70Knobs.AUTO_THROTTLE, (SortedList<int, KeyPressInfo>)textBox.Tag, false);
                }
                if (textBox.Equals(TextBoxAutoThrottleOn))
                {
                    _multiPanelPZ70.AddOrUpdateSequencedKeyBinding(textBox.Text, MultiPanelPZ70Knobs.AUTO_THROTTLE, (SortedList<int, KeyPressInfo>)textBox.Tag);
                }
                if (textBox.Equals(TextBoxFlapsUp))
                {
                    _multiPanelPZ70.AddOrUpdateSequencedKeyBinding(textBox.Text, MultiPanelPZ70Knobs.FLAPS_LEVER_UP, (SortedList<int, KeyPressInfo>)textBox.Tag);
                }
                if (textBox.Equals(TextBoxFlapsDown))
                {
                    _multiPanelPZ70.AddOrUpdateSequencedKeyBinding(textBox.Text, MultiPanelPZ70Knobs.FLAPS_LEVER_DOWN, (SortedList<int, KeyPressInfo>)textBox.Tag);
                }
                if (textBox.Equals(TextBoxPitchTrimUp))
                {
                    _multiPanelPZ70.AddOrUpdateSequencedKeyBinding(textBox.Text, MultiPanelPZ70Knobs.PITCH_TRIM_WHEEL_UP, (SortedList<int, KeyPressInfo>)textBox.Tag);
                }
                if (textBox.Equals(TextBoxPitchTrimDown))
                {
                    _multiPanelPZ70.AddOrUpdateSequencedKeyBinding(textBox.Text, MultiPanelPZ70Knobs.PITCH_TRIM_WHEEL_DOWN, (SortedList<int, KeyPressInfo>)textBox.Tag);
                }
                if (textBox.Equals(TextBoxApButtonOn))
                {
                    _multiPanelPZ70.AddOrUpdateSequencedKeyBinding(textBox.Text, MultiPanelPZ70Knobs.AP_BUTTON, (SortedList<int, KeyPressInfo>)textBox.Tag);
                }
                if (textBox.Equals(TextBoxApButtonOff))
                {
                    _multiPanelPZ70.AddOrUpdateSequencedKeyBinding(textBox.Text, MultiPanelPZ70Knobs.AP_BUTTON, (SortedList<int, KeyPressInfo>)textBox.Tag, false);
                }
                if (textBox.Equals(TextBoxHdgButtonOn))
                {
                    _multiPanelPZ70.AddOrUpdateSequencedKeyBinding(textBox.Text, MultiPanelPZ70Knobs.HDG_BUTTON, (SortedList<int, KeyPressInfo>)textBox.Tag);
                }
                if (textBox.Equals(TextBoxHdgButtonOff))
                {
                    _multiPanelPZ70.AddOrUpdateSequencedKeyBinding(textBox.Text, MultiPanelPZ70Knobs.HDG_BUTTON, (SortedList<int, KeyPressInfo>)textBox.Tag, false);
                }
                if (textBox.Equals(TextBoxNavButtonOn))
                {
                    _multiPanelPZ70.AddOrUpdateSequencedKeyBinding(textBox.Text, MultiPanelPZ70Knobs.NAV_BUTTON, (SortedList<int, KeyPressInfo>)textBox.Tag);
                }
                if (textBox.Equals(TextBoxNavButtonOff))
                {
                    _multiPanelPZ70.AddOrUpdateSequencedKeyBinding(textBox.Text, MultiPanelPZ70Knobs.NAV_BUTTON, (SortedList<int, KeyPressInfo>)textBox.Tag, false);
                }
                if (textBox.Equals(TextBoxIasButtonOn))
                {
                    _multiPanelPZ70.AddOrUpdateSequencedKeyBinding(textBox.Text, MultiPanelPZ70Knobs.IAS_BUTTON, (SortedList<int, KeyPressInfo>)textBox.Tag);
                }
                if (textBox.Equals(TextBoxIasButtonOff))
                {
                    _multiPanelPZ70.AddOrUpdateSequencedKeyBinding(textBox.Text, MultiPanelPZ70Knobs.IAS_BUTTON, (SortedList<int, KeyPressInfo>)textBox.Tag, false);
                }
                if (textBox.Equals(TextBoxAltButtonOn))
                {
                    _multiPanelPZ70.AddOrUpdateSequencedKeyBinding(textBox.Text, MultiPanelPZ70Knobs.ALT_BUTTON, (SortedList<int, KeyPressInfo>)textBox.Tag);
                }
                if (textBox.Equals(TextBoxAltButtonOff))
                {
                    _multiPanelPZ70.AddOrUpdateSequencedKeyBinding(textBox.Text, MultiPanelPZ70Knobs.ALT_BUTTON, (SortedList<int, KeyPressInfo>)textBox.Tag, false);
                }
                if (textBox.Equals(TextBoxVsButtonOn))
                {
                    _multiPanelPZ70.AddOrUpdateSequencedKeyBinding(textBox.Text, MultiPanelPZ70Knobs.VS_BUTTON, (SortedList<int, KeyPressInfo>)textBox.Tag);
                }
                if (textBox.Equals(TextBoxVsButtonOff))
                {
                    _multiPanelPZ70.AddOrUpdateSequencedKeyBinding(textBox.Text, MultiPanelPZ70Knobs.VS_BUTTON, (SortedList<int, KeyPressInfo>)textBox.Tag, false);
                }
                if (textBox.Equals(TextBoxAprButtonOn))
                {
                    _multiPanelPZ70.AddOrUpdateSequencedKeyBinding(textBox.Text, MultiPanelPZ70Knobs.APR_BUTTON, (SortedList<int, KeyPressInfo>)textBox.Tag);
                }
                if (textBox.Equals(TextBoxAprButtonOff))
                {
                    _multiPanelPZ70.AddOrUpdateSequencedKeyBinding(textBox.Text, MultiPanelPZ70Knobs.APR_BUTTON, (SortedList<int, KeyPressInfo>)textBox.Tag, false);
                }
                if (textBox.Equals(TextBoxRevButtonOn))
                {
                    _multiPanelPZ70.AddOrUpdateSequencedKeyBinding(textBox.Text, MultiPanelPZ70Knobs.REV_BUTTON, (SortedList<int, KeyPressInfo>)textBox.Tag);
                }
                if (textBox.Equals(TextBoxRevButtonOff))
                {
                    _multiPanelPZ70.AddOrUpdateSequencedKeyBinding(textBox.Text, MultiPanelPZ70Knobs.REV_BUTTON, (SortedList<int, KeyPressInfo>)textBox.Tag, false);
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(3011, ex);
            }
        }


        private void UpdateKeyBindingProfileSimpleKeyStrokes(TextBox textBox)
        {
            try
            {
                KeyPressLength keyPressLength;
                if (textBox.Tag == null)
                {
                    keyPressLength = KeyPressLength.FiftyMilliSec;
                }
                else
                {
                    keyPressLength = ((KeyPressLength)textBox.Tag);
                }
                if (textBox.Equals(TextBoxLcdKnobDecrease))
                {
                    _multiPanelPZ70.AddOrUpdateSingleKeyBinding(MultiPanelPZ70Knobs.LCD_WHEEL_DEC, TextBoxLcdKnobDecrease.Text, keyPressLength);
                }
                if (textBox.Equals(TextBoxLcdKnobIncrease))
                {
                    _multiPanelPZ70.AddOrUpdateSingleKeyBinding(MultiPanelPZ70Knobs.LCD_WHEEL_INC, TextBoxLcdKnobIncrease.Text, keyPressLength);
                }
                if (textBox.Equals(TextBoxAutoThrottleOff))
                {
                    _multiPanelPZ70.AddOrUpdateSingleKeyBinding(MultiPanelPZ70Knobs.AUTO_THROTTLE, TextBoxAutoThrottleOff.Text, keyPressLength, false);
                }
                if (textBox.Equals(TextBoxAutoThrottleOn))
                {
                    _multiPanelPZ70.AddOrUpdateSingleKeyBinding(MultiPanelPZ70Knobs.AUTO_THROTTLE, TextBoxAutoThrottleOn.Text, keyPressLength);
                }
                if (textBox.Equals(TextBoxFlapsUp))
                {
                    _multiPanelPZ70.AddOrUpdateSingleKeyBinding(MultiPanelPZ70Knobs.FLAPS_LEVER_UP, TextBoxFlapsUp.Text, keyPressLength);
                }
                if (textBox.Equals(TextBoxFlapsDown))
                {
                    _multiPanelPZ70.AddOrUpdateSingleKeyBinding(MultiPanelPZ70Knobs.FLAPS_LEVER_DOWN, TextBoxFlapsDown.Text, keyPressLength);
                }
                if (textBox.Equals(TextBoxPitchTrimUp))
                {
                    _multiPanelPZ70.AddOrUpdateSingleKeyBinding(MultiPanelPZ70Knobs.PITCH_TRIM_WHEEL_UP, TextBoxPitchTrimUp.Text, keyPressLength);
                }
                if (textBox.Equals(TextBoxPitchTrimDown))
                {
                    _multiPanelPZ70.AddOrUpdateSingleKeyBinding(MultiPanelPZ70Knobs.PITCH_TRIM_WHEEL_DOWN, TextBoxPitchTrimDown.Text, keyPressLength);
                }
                if (textBox.Equals(TextBoxApButtonOn))
                {
                    _multiPanelPZ70.AddOrUpdateSingleKeyBinding(MultiPanelPZ70Knobs.AP_BUTTON, TextBoxApButtonOn.Text, keyPressLength);
                }
                if (textBox.Equals(TextBoxApButtonOff))
                {
                    _multiPanelPZ70.AddOrUpdateSingleKeyBinding(MultiPanelPZ70Knobs.AP_BUTTON, TextBoxApButtonOff.Text, keyPressLength, false);
                }
                if (textBox.Equals(TextBoxHdgButtonOn))
                {
                    _multiPanelPZ70.AddOrUpdateSingleKeyBinding(MultiPanelPZ70Knobs.HDG_BUTTON, TextBoxHdgButtonOn.Text, keyPressLength);
                }
                if (textBox.Equals(TextBoxHdgButtonOff))
                {
                    _multiPanelPZ70.AddOrUpdateSingleKeyBinding(MultiPanelPZ70Knobs.HDG_BUTTON, TextBoxHdgButtonOff.Text, keyPressLength, false);
                }
                if (textBox.Equals(TextBoxNavButtonOn))
                {
                    _multiPanelPZ70.AddOrUpdateSingleKeyBinding(MultiPanelPZ70Knobs.NAV_BUTTON, TextBoxNavButtonOn.Text, keyPressLength);
                }
                if (textBox.Equals(TextBoxNavButtonOff))
                {
                    _multiPanelPZ70.AddOrUpdateSingleKeyBinding(MultiPanelPZ70Knobs.NAV_BUTTON, TextBoxNavButtonOff.Text, keyPressLength, false);
                }
                if (textBox.Equals(TextBoxIasButtonOn))
                {
                    _multiPanelPZ70.AddOrUpdateSingleKeyBinding(MultiPanelPZ70Knobs.IAS_BUTTON, TextBoxIasButtonOn.Text, keyPressLength);
                }
                if (textBox.Equals(TextBoxIasButtonOff))
                {
                    _multiPanelPZ70.AddOrUpdateSingleKeyBinding(MultiPanelPZ70Knobs.IAS_BUTTON, TextBoxIasButtonOff.Text, keyPressLength, false);
                }
                if (textBox.Equals(TextBoxAltButtonOn))
                {
                    _multiPanelPZ70.AddOrUpdateSingleKeyBinding(MultiPanelPZ70Knobs.ALT_BUTTON, TextBoxAltButtonOn.Text, keyPressLength);
                }
                if (textBox.Equals(TextBoxAltButtonOff))
                {
                    _multiPanelPZ70.AddOrUpdateSingleKeyBinding(MultiPanelPZ70Knobs.ALT_BUTTON, TextBoxAltButtonOff.Text, keyPressLength, false);
                }
                if (textBox.Equals(TextBoxVsButtonOn))
                {
                    _multiPanelPZ70.AddOrUpdateSingleKeyBinding(MultiPanelPZ70Knobs.VS_BUTTON, TextBoxVsButtonOn.Text, keyPressLength);
                }
                if (textBox.Equals(TextBoxVsButtonOff))
                {
                    _multiPanelPZ70.AddOrUpdateSingleKeyBinding(MultiPanelPZ70Knobs.VS_BUTTON, TextBoxVsButtonOff.Text, keyPressLength, false);
                }
                if (textBox.Equals(TextBoxAprButtonOn))
                {
                    _multiPanelPZ70.AddOrUpdateSingleKeyBinding(MultiPanelPZ70Knobs.APR_BUTTON, TextBoxAprButtonOn.Text, keyPressLength);
                }
                if (textBox.Equals(TextBoxAprButtonOff))
                {
                    _multiPanelPZ70.AddOrUpdateSingleKeyBinding(MultiPanelPZ70Knobs.APR_BUTTON, TextBoxAprButtonOff.Text, keyPressLength, false);
                }
                if (textBox.Equals(TextBoxRevButtonOn))
                {
                    _multiPanelPZ70.AddOrUpdateSingleKeyBinding(MultiPanelPZ70Knobs.REV_BUTTON, TextBoxRevButtonOn.Text, keyPressLength);
                }
                if (textBox.Equals(TextBoxRevButtonOff))
                {
                    _multiPanelPZ70.AddOrUpdateSingleKeyBinding(MultiPanelPZ70Knobs.REV_BUTTON, TextBoxRevButtonOff.Text, keyPressLength, false);
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(3012, ex);
            }
        }

        private MultiPanelPZ70KnobOnOff GetPZ70Knob(TextBox textBox)
        {
            try
            {
                if (textBox.Equals(TextBoxLcdKnobDecrease))
                {
                    return new MultiPanelPZ70KnobOnOff(MultiPanelPZ70Knobs.LCD_WHEEL_DEC, true);
                }
                if (textBox.Equals(TextBoxLcdKnobIncrease))
                {
                    return new MultiPanelPZ70KnobOnOff(MultiPanelPZ70Knobs.LCD_WHEEL_INC, true);
                }
                if (textBox.Equals(TextBoxAutoThrottleOff))
                {
                    return new MultiPanelPZ70KnobOnOff(MultiPanelPZ70Knobs.AUTO_THROTTLE, false);
                }
                if (textBox.Equals(TextBoxAutoThrottleOn))
                {
                    return new MultiPanelPZ70KnobOnOff(MultiPanelPZ70Knobs.AUTO_THROTTLE, true);
                }
                if (textBox.Equals(TextBoxFlapsUp))
                {
                    return new MultiPanelPZ70KnobOnOff(MultiPanelPZ70Knobs.FLAPS_LEVER_UP, true);
                }
                if (textBox.Equals(TextBoxFlapsDown))
                {
                    return new MultiPanelPZ70KnobOnOff(MultiPanelPZ70Knobs.FLAPS_LEVER_DOWN, false);
                }
                if (textBox.Equals(TextBoxPitchTrimUp))
                {
                    return new MultiPanelPZ70KnobOnOff(MultiPanelPZ70Knobs.PITCH_TRIM_WHEEL_UP, true);
                }
                if (textBox.Equals(TextBoxPitchTrimDown))
                {
                    return new MultiPanelPZ70KnobOnOff(MultiPanelPZ70Knobs.PITCH_TRIM_WHEEL_DOWN, false);
                }
                if (textBox.Equals(TextBoxApButtonOn))
                {
                    return new MultiPanelPZ70KnobOnOff(MultiPanelPZ70Knobs.AP_BUTTON, true);
                }
                if (textBox.Equals(TextBoxApButtonOff))
                {
                    return new MultiPanelPZ70KnobOnOff(MultiPanelPZ70Knobs.AP_BUTTON, false);
                }
                if (textBox.Equals(TextBoxHdgButtonOn))
                {
                    return new MultiPanelPZ70KnobOnOff(MultiPanelPZ70Knobs.HDG_BUTTON, true);
                }
                if (textBox.Equals(TextBoxHdgButtonOff))
                {
                    return new MultiPanelPZ70KnobOnOff(MultiPanelPZ70Knobs.HDG_BUTTON, false);
                }
                if (textBox.Equals(TextBoxNavButtonOn))
                {
                    return new MultiPanelPZ70KnobOnOff(MultiPanelPZ70Knobs.NAV_BUTTON, true);
                }
                if (textBox.Equals(TextBoxNavButtonOff))
                {
                    return new MultiPanelPZ70KnobOnOff(MultiPanelPZ70Knobs.NAV_BUTTON, false);
                }
                if (textBox.Equals(TextBoxIasButtonOn))
                {
                    return new MultiPanelPZ70KnobOnOff(MultiPanelPZ70Knobs.IAS_BUTTON, true);
                }
                if (textBox.Equals(TextBoxIasButtonOff))
                {
                    return new MultiPanelPZ70KnobOnOff(MultiPanelPZ70Knobs.IAS_BUTTON, false);
                }
                if (textBox.Equals(TextBoxAltButtonOn))
                {
                    return new MultiPanelPZ70KnobOnOff(MultiPanelPZ70Knobs.ALT_BUTTON, true);
                }
                if (textBox.Equals(TextBoxAltButtonOff))
                {
                    return new MultiPanelPZ70KnobOnOff(MultiPanelPZ70Knobs.ALT_BUTTON, false);
                }
                if (textBox.Equals(TextBoxVsButtonOn))
                {
                    return new MultiPanelPZ70KnobOnOff(MultiPanelPZ70Knobs.VS_BUTTON, true);
                }
                if (textBox.Equals(TextBoxVsButtonOff))
                {
                    return new MultiPanelPZ70KnobOnOff(MultiPanelPZ70Knobs.VS_BUTTON, false);
                }
                if (textBox.Equals(TextBoxAprButtonOn))
                {
                    return new MultiPanelPZ70KnobOnOff(MultiPanelPZ70Knobs.APR_BUTTON, true);
                }
                if (textBox.Equals(TextBoxAprButtonOff))
                {
                    return new MultiPanelPZ70KnobOnOff(MultiPanelPZ70Knobs.APR_BUTTON, false);
                }
                if (textBox.Equals(TextBoxRevButtonOn))
                {
                    return new MultiPanelPZ70KnobOnOff(MultiPanelPZ70Knobs.REV_BUTTON, true);
                }
                if (textBox.Equals(TextBoxRevButtonOff))
                {
                    return new MultiPanelPZ70KnobOnOff(MultiPanelPZ70Knobs.REV_BUTTON, false);
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(3012, ex);
            }
            throw new Exception("Should not reach this point");
        }

        private void UpdateDCSBIOSBinding(TextBox textBox)
        {
            try
            {
                List<DCSBIOSInput> dcsBiosInputs = null;
                if (textBox.Tag is List<DCSBIOSInput>)
                {
                    dcsBiosInputs = ((List<DCSBIOSInput>)textBox.Tag);
                }
                if (textBox.Equals(TextBoxLcdKnobDecrease))
                {
                    _multiPanelPZ70.AddOrUpdateDCSBIOSBinding(MultiPanelPZ70Knobs.LCD_WHEEL_DEC, dcsBiosInputs, textBox.Text);
                }
                if (textBox.Equals(TextBoxLcdKnobIncrease))
                {
                    _multiPanelPZ70.AddOrUpdateDCSBIOSBinding(MultiPanelPZ70Knobs.LCD_WHEEL_INC, dcsBiosInputs, textBox.Text);
                }
                if (textBox.Equals(TextBoxAutoThrottleOff))
                {
                    _multiPanelPZ70.AddOrUpdateDCSBIOSBinding(MultiPanelPZ70Knobs.AUTO_THROTTLE, dcsBiosInputs, textBox.Text, false);
                }
                if (textBox.Equals(TextBoxAutoThrottleOn))
                {
                    _multiPanelPZ70.AddOrUpdateDCSBIOSBinding(MultiPanelPZ70Knobs.AUTO_THROTTLE, dcsBiosInputs, textBox.Text);
                }
                if (textBox.Equals(TextBoxFlapsUp))
                {
                    _multiPanelPZ70.AddOrUpdateDCSBIOSBinding(MultiPanelPZ70Knobs.FLAPS_LEVER_UP, dcsBiosInputs, textBox.Text);
                }
                if (textBox.Equals(TextBoxFlapsDown))
                {
                    _multiPanelPZ70.AddOrUpdateDCSBIOSBinding(MultiPanelPZ70Knobs.FLAPS_LEVER_DOWN, dcsBiosInputs, textBox.Text);
                }
                if (textBox.Equals(TextBoxPitchTrimUp))
                {
                    _multiPanelPZ70.AddOrUpdateDCSBIOSBinding(MultiPanelPZ70Knobs.PITCH_TRIM_WHEEL_UP, dcsBiosInputs, textBox.Text);
                }
                if (textBox.Equals(TextBoxPitchTrimDown))
                {
                    _multiPanelPZ70.AddOrUpdateDCSBIOSBinding(MultiPanelPZ70Knobs.PITCH_TRIM_WHEEL_DOWN, dcsBiosInputs, textBox.Text);
                }
                if (textBox.Equals(TextBoxApButtonOn))
                {
                    _multiPanelPZ70.AddOrUpdateDCSBIOSBinding(MultiPanelPZ70Knobs.AP_BUTTON, dcsBiosInputs, textBox.Text);
                }
                if (textBox.Equals(TextBoxAprButtonOff))
                {
                    _multiPanelPZ70.AddOrUpdateDCSBIOSBinding(MultiPanelPZ70Knobs.AP_BUTTON, dcsBiosInputs, textBox.Text, false);
                }
                if (textBox.Equals(TextBoxHdgButtonOn))
                {
                    _multiPanelPZ70.AddOrUpdateDCSBIOSBinding(MultiPanelPZ70Knobs.HDG_BUTTON, dcsBiosInputs, textBox.Text);
                }
                if (textBox.Equals(TextBoxHdgButtonOff))
                {
                    _multiPanelPZ70.AddOrUpdateDCSBIOSBinding(MultiPanelPZ70Knobs.HDG_BUTTON, dcsBiosInputs, textBox.Text, false);
                }
                if (textBox.Equals(TextBoxNavButtonOn))
                {
                    _multiPanelPZ70.AddOrUpdateDCSBIOSBinding(MultiPanelPZ70Knobs.NAV_BUTTON, dcsBiosInputs, textBox.Text);
                }
                if (textBox.Equals(TextBoxNavButtonOff))
                {
                    _multiPanelPZ70.AddOrUpdateDCSBIOSBinding(MultiPanelPZ70Knobs.NAV_BUTTON, dcsBiosInputs, textBox.Text, false);
                }
                if (textBox.Equals(TextBoxIasButtonOn))
                {
                    _multiPanelPZ70.AddOrUpdateDCSBIOSBinding(MultiPanelPZ70Knobs.IAS_BUTTON, dcsBiosInputs, textBox.Text);
                }
                if (textBox.Equals(TextBoxIasButtonOff))
                {
                    _multiPanelPZ70.AddOrUpdateDCSBIOSBinding(MultiPanelPZ70Knobs.IAS_BUTTON, dcsBiosInputs, textBox.Text, false);
                }
                if (textBox.Equals(TextBoxAltButtonOn))
                {
                    _multiPanelPZ70.AddOrUpdateDCSBIOSBinding(MultiPanelPZ70Knobs.ALT_BUTTON, dcsBiosInputs, textBox.Text);
                }
                if (textBox.Equals(TextBoxAltButtonOff))
                {
                    _multiPanelPZ70.AddOrUpdateDCSBIOSBinding(MultiPanelPZ70Knobs.ALT_BUTTON, dcsBiosInputs, textBox.Text, false);
                }
                if (textBox.Equals(TextBoxVsButtonOn))
                {
                    _multiPanelPZ70.AddOrUpdateDCSBIOSBinding(MultiPanelPZ70Knobs.VS_BUTTON, dcsBiosInputs, textBox.Text);
                }
                if (textBox.Equals(TextBoxVsButtonOff))
                {
                    _multiPanelPZ70.AddOrUpdateDCSBIOSBinding(MultiPanelPZ70Knobs.VS_BUTTON, dcsBiosInputs, textBox.Text, false);
                }
                if (textBox.Equals(TextBoxAprButtonOn))
                {
                    _multiPanelPZ70.AddOrUpdateDCSBIOSBinding(MultiPanelPZ70Knobs.APR_BUTTON, dcsBiosInputs, textBox.Text);
                }
                if (textBox.Equals(TextBoxAprButtonOff))
                {
                    _multiPanelPZ70.AddOrUpdateDCSBIOSBinding(MultiPanelPZ70Knobs.APR_BUTTON, dcsBiosInputs, textBox.Text, false);
                }
                if (textBox.Equals(TextBoxRevButtonOn))
                {
                    _multiPanelPZ70.AddOrUpdateDCSBIOSBinding(MultiPanelPZ70Knobs.REV_BUTTON, dcsBiosInputs, textBox.Text);
                }
                if (textBox.Equals(TextBoxRevButtonOff))
                {
                    _multiPanelPZ70.AddOrUpdateDCSBIOSBinding(MultiPanelPZ70Knobs.REV_BUTTON, dcsBiosInputs, textBox.Text, false);
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(345012, ex);
            }
        }

        private void UpdateDCSBIOSBindingLCD(Button button)
        {
            try
            {
                DCSBIOSOutput dcsBiosOutput = null;
                DCSBIOSOutputFormula dcsBiosOutputFormula = null;
                /*if (button.Tag == null)
                {
                    //Remove any binding associated with this button (position), (delete)
                    return;
                }*/

                if (button.Tag is DCSBIOSOutput)
                {
                    dcsBiosOutput = ((DCSBIOSOutput)button.Tag);

                    if (button.Equals(ButtonKnobAltLcdUpper))
                    {
                        ImageAltUpperRow.Visibility = dcsBiosOutput == null ? Visibility.Collapsed : Visibility.Visible;
                        _multiPanelPZ70.AddOrUpdateDCSBIOSLcdBinding(MultiPanelPZ70Knobs.KNOB_ALT, dcsBiosOutput, PZ70LCDPosition.UpperALT);
                    }
                    if (button.Equals(ButtonKnobAltLcdLower))
                    {
                        ImageAltLowerRow.Visibility = dcsBiosOutput == null ? Visibility.Collapsed : Visibility.Visible;
                        _multiPanelPZ70.AddOrUpdateDCSBIOSLcdBinding(MultiPanelPZ70Knobs.KNOB_ALT, dcsBiosOutput, PZ70LCDPosition.LowerALT);
                    }
                    if (button.Equals(ButtonKnobVsLcdUpper))
                    {
                        ImageVsUpperRow.Visibility = dcsBiosOutput == null ? Visibility.Collapsed : Visibility.Visible;
                        _multiPanelPZ70.AddOrUpdateDCSBIOSLcdBinding(MultiPanelPZ70Knobs.KNOB_VS, dcsBiosOutput, PZ70LCDPosition.UpperVS);
                    }
                    if (button.Equals(ButtonKnobVsLcdLower))
                    {
                        ImageVsLowerRow.Visibility = dcsBiosOutput == null ? Visibility.Collapsed : Visibility.Visible;
                        _multiPanelPZ70.AddOrUpdateDCSBIOSLcdBinding(MultiPanelPZ70Knobs.KNOB_VS, dcsBiosOutput, PZ70LCDPosition.LowerVS);
                    }
                }
                if (button.Tag is DCSBIOSOutputFormula)
                {
                    dcsBiosOutputFormula = ((DCSBIOSOutputFormula)button.Tag);

                    if (button.Equals(ButtonKnobAltLcdUpper))
                    {
                        ImageAltUpperRow.Visibility = dcsBiosOutputFormula == null ? Visibility.Collapsed : Visibility.Visible;
                        _multiPanelPZ70.AddOrUpdateDCSBIOSLcdBinding(MultiPanelPZ70Knobs.KNOB_ALT, dcsBiosOutputFormula, PZ70LCDPosition.UpperALT);
                    }
                    if (button.Equals(ButtonKnobAltLcdLower))
                    {
                        ImageAltLowerRow.Visibility = dcsBiosOutputFormula == null ? Visibility.Collapsed : Visibility.Visible;
                        _multiPanelPZ70.AddOrUpdateDCSBIOSLcdBinding(MultiPanelPZ70Knobs.KNOB_ALT, dcsBiosOutputFormula, PZ70LCDPosition.LowerALT);
                    }
                    if (button.Equals(ButtonKnobVsLcdUpper))
                    {
                        ImageVsUpperRow.Visibility = dcsBiosOutputFormula == null ? Visibility.Collapsed : Visibility.Visible;
                        _multiPanelPZ70.AddOrUpdateDCSBIOSLcdBinding(MultiPanelPZ70Knobs.KNOB_VS, dcsBiosOutputFormula, PZ70LCDPosition.UpperVS);
                    }
                    if (button.Equals(ButtonKnobVsLcdLower))
                    {
                        ImageVsLowerRow.Visibility = dcsBiosOutputFormula == null ? Visibility.Collapsed : Visibility.Visible;
                        _multiPanelPZ70.AddOrUpdateDCSBIOSLcdBinding(MultiPanelPZ70Knobs.KNOB_VS, dcsBiosOutputFormula, PZ70LCDPosition.LowerVS);
                    }
                }
                if (button.Tag == null)
                {
                    if (button.Equals(ButtonKnobAltLcdUpper))
                    {
                        ImageAltUpperRow.Visibility = Visibility.Hidden;
                        _multiPanelPZ70.AddOrUpdateDCSBIOSLcdBinding(MultiPanelPZ70Knobs.KNOB_ALT, PZ70LCDPosition.UpperALT);
                    }
                    if (button.Equals(ButtonKnobAltLcdLower))
                    {
                        ImageAltLowerRow.Visibility = Visibility.Hidden;
                        _multiPanelPZ70.AddOrUpdateDCSBIOSLcdBinding(MultiPanelPZ70Knobs.KNOB_ALT, PZ70LCDPosition.LowerALT);
                    }
                    if (button.Equals(ButtonKnobVsLcdUpper))
                    {
                        ImageVsUpperRow.Visibility = Visibility.Hidden;
                        _multiPanelPZ70.AddOrUpdateDCSBIOSLcdBinding(MultiPanelPZ70Knobs.KNOB_VS, PZ70LCDPosition.UpperVS);
                    }
                    if (button.Equals(ButtonKnobVsLcdLower))
                    {
                        ImageVsLowerRow.Visibility = Visibility.Hidden;
                        _multiPanelPZ70.AddOrUpdateDCSBIOSLcdBinding(MultiPanelPZ70Knobs.KNOB_VS, PZ70LCDPosition.LowerVS);
                    }
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(34501287, ex);
            }
        }

        private void MultiPanelUserControl_OnLoaded(object sender, RoutedEventArgs e)
        {
            ComboBoxLcdKnobSensitivity.SelectedValue = Settings.Default.PZ70LcdKnobSensitivity;
            SetContextMenuClickHandlers();
            _userControlLoaded = true;
        }

        private void SetContextMenuClickHandlers()
        {
            foreach (var textBox in Common.FindVisualChildren<TextBox>(this))
            {
                if (textBox != TextBoxLogPZ70)
                {
                    textBox.ContextMenu = (ContextMenu)Resources["TextBoxContextMenuPZ70"];
                    textBox.ContextMenuOpening += TextBoxContextMenuOpening;
                }
            }

            ButtonKnobAltLcdUpper.ContextMenu = (ContextMenu)Resources["ButtonLcdContextMenu"];
            ButtonKnobAltLcdUpper.ContextMenu.Tag = ButtonKnobAltLcdUpper;

            ButtonKnobAltLcdLower.ContextMenu = (ContextMenu)Resources["ButtonLcdContextMenu"];
            ButtonKnobAltLcdLower.ContextMenu.Tag = ButtonKnobAltLcdLower;

            ButtonKnobVsLcdUpper.ContextMenu = (ContextMenu)Resources["ButtonLcdContextMenu"];
            ButtonKnobVsLcdUpper.ContextMenu.Tag = ButtonKnobVsLcdUpper;

            ButtonKnobVsLcdLower.ContextMenu = (ContextMenu)Resources["ButtonLcdContextMenu"];
            ButtonKnobVsLcdLower.ContextMenu.Tag = ButtonKnobVsLcdLower;
        }

        private void TextBoxContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            try
            {
                //Timing values
                //Edit sequence
                //Edit DCS-BIOC Control
                var textBox = GetTextBoxInFocus();

                if (textBox == null)
                {
                    throw new Exception("Failed to locate which textbox is focused.");
                }
                var contextMenu = textBox.ContextMenu;

                // 1) If textbox.tag is List<DCSBIOSInput>, show Edit DCS-BIOS Control
                // 2) If textbox.tag is keyvaluepair, show Edit sequence
                // 3) If textbox.tag is null & text is empty, show Edit sequence & DCS-BIOS Control

                // 4) If textbox has text and tag is not keyvaluepair/DCSBIOSInput, show press times
                // 5) If textbox is not empty, no tag show key press times
                // 6) If textbox is not empty, key press tag show key press times

                //1
                if (textBox.Tag != null && textBox.Tag is List<DCSBIOSInput>)
                {
                    // 1) If textbox.tag is List<DCSBIOSInput>, show Edit DCS-BIOS Control    
                    foreach (MenuItem item in contextMenu.Items)
                    {
                        if (!item.Name.Contains("EditDCSBIOS"))
                        {
                            item.Visibility = Visibility.Collapsed;
                        }
                        else
                        {
                            item.Visibility = Visibility.Visible;
                        }
                    }
                }
                else if (textBox.Tag != null && textBox.Tag is SortedList<int, KeyPressInfo>)
                {
                    // 2) If textbox.tag is keyvaluepair, show Edit sequence
                    foreach (MenuItem item in contextMenu.Items)
                    {
                        if (!item.Name.Contains("EditSequence"))
                        {
                            item.Visibility = Visibility.Collapsed;
                        }
                        else
                        {
                            item.Visibility = Visibility.Visible;
                        }
                    }
                }
                else if (textBox.Tag == null && string.IsNullOrWhiteSpace(textBox.Text))
                {
                    // 3) If textbox.tag is null & text is empty, show Edit sequence & DCS-BIOS Control
                    foreach (MenuItem item in contextMenu.Items)
                    {
                        if (item.Name.Contains("EditSequence"))
                        {
                            item.Visibility = Visibility.Visible;
                        }
                        else if (!_multiPanelPZ70.KeyboardEmulationOnly && item.Name.Contains("EditDCSBIOS"))
                        {
                            item.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            item.Visibility = Visibility.Collapsed;
                        }
                    }
                }
                else if (!string.IsNullOrWhiteSpace(textBox.Text) && (textBox.Tag == null || (!(textBox.Tag is List<DCSBIOSInput>) && !(textBox.Tag is SortedList<int, KeyPressInfo>))))
                {
                    // 4) If textbox has text and tag is not keyvaluepair/DCSBIOSInput, show press times
                    foreach (MenuItem item in contextMenu.Items)
                    {
                        if (item.Name.Contains("EditSequence") || item.Name.Contains("EditDCSBIOS"))
                        {
                            item.Visibility = Visibility.Collapsed;
                        }
                        else
                        {
                            item.Visibility = Visibility.Visible;
                        }
                    }
                }
                else if (!string.IsNullOrWhiteSpace(textBox.Text) && (textBox.Tag == null))
                {
                    // 5) If textbox is not empty, no tag show key press times
                    foreach (MenuItem item in contextMenu.Items)
                    {
                        if (item.Name.Contains("EditDCSBIOS") || item.Name.Contains("EditSequence"))
                        {
                            item.Visibility = Visibility.Collapsed;
                        }
                        else
                        {
                            item.Visibility = Visibility.Visible;
                        }
                    }
                }

                // 6) If textbox is not empty, key press tag show key press times
                if ((string.IsNullOrEmpty(textBox.Text) && textBox.Tag != null) && textBox.Tag is KeyPressInfo)
                {
                    foreach (MenuItem item in contextMenu.Items)
                    {
                        if (item.Name.Contains("EditDCSBIOS") || item.Name.Contains("EditSequence"))
                        {
                            item.Visibility = Visibility.Collapsed;
                        }
                        else
                        {
                            item.Visibility = Visibility.Visible;
                        }
                    }
                }
                /*else
                {
                    foreach (MenuItem item in contextMenu.Items)
                    {
                        if (!item.Name.Contains("Sequence"))
                        {
                            item.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            item.Visibility = Visibility.Collapsed;
                        }
                    }
                }*/

            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(2081, ex);
            }
        }

        private void NotifyKnobChanges(HashSet<object> knobs)
        {
            try
            {
                //Set focus to this so that virtual keypresses won't affect settings
                Dispatcher.BeginInvoke((Action)(() => TextBoxLogPZ70.Focus()));
                foreach (var knob in knobs)
                {
                    var multiPanelKnob = (MultiPanelKnob)knob;

                    if (_multiPanelPZ70.ForwardKeyPresses)
                    {
                        if (!string.IsNullOrEmpty(_multiPanelPZ70.GetKeyPressForLoggingPurposes(multiPanelKnob)))
                        {
                            Dispatcher.BeginInvoke(
                                (Action)
                                (() =>
                                 TextBoxLogPZ70.Text = TextBoxLogPZ70.Text.Insert(0, _multiPanelPZ70.GetKeyPressForLoggingPurposes(multiPanelKnob) + "\n")));
                        }
                    }
                    else
                    {
                        Dispatcher.BeginInvoke(
                            (Action)
                            (() =>
                             TextBoxLogPZ70.Text = TextBoxLogPZ70.Text.Insert(0, "No action taken, virtual key press disabled.\n")));
                    }
                }
                SetGraphicsState(knobs);
            }
            catch (Exception ex)
            {
                Dispatcher.BeginInvoke(
                    (Action)
                    (() =>
                     TextBoxLogPZ70.Text = TextBoxLogPZ70.Text.Insert(0, "0x16" + ex.Message + ".\n")));
                Common.ShowErrorMessageBox(3009, ex);
            }
        }

        private void ButtonLcdMenuItemDeleteBinding_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var menuItem = (MenuItem)sender;
                var button = (Button)((ContextMenu)(menuItem.Parent)).Tag;
                DeleteLCDBinding(button);
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(7365005, ex);
            }
        }

        private void ButtonLcdContextMenu_OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            try
            {
                var contextMenu = (ContextMenu)sender;
                var button = (Button)contextMenu.Tag;
                if (button.Tag != null && button.Tag is DCSBIOSOutput)
                {
                    ((MenuItem)contextMenu.Items[0]).IsEnabled = true;
                }
                else
                {
                    ((MenuItem)contextMenu.Items[0]).IsEnabled = false;
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(7265005, ex);
            }
        }

        private void DeleteLCDBinding(Button button)
        {
            try
            {
                //Check if this button contains DCS-BIOS information. If so then prompt the user for deletion
                if (button.Tag != null && button.Tag is DCSBIOSOutput)
                {
                    if (MessageBox.Show("Do you want to delete the specified DCS-BIOS control binding?", "Delete DCS-BIOS control binding?", MessageBoxButton.OKCancel, MessageBoxImage.Question) != MessageBoxResult.OK)
                    {
                        return;
                    }
                }
                button.ToolTip = null;
                button.Tag = null;
                UpdateDCSBIOSBindingLCD(button);
                switch (button.Name)
                {
                    case "ButtonKnobAltLcdUpper":
                        {
                            ImageAltUpperRow.Visibility = Visibility.Collapsed;
                            break;
                        }
                    case "ButtonKnobAltLcdLower":
                        {
                            ImageAltLowerRow.Visibility = Visibility.Collapsed;
                            break;
                        }
                    case "ButtonKnobVsLcdUpper":
                        {
                            ImageVsUpperRow.Visibility = Visibility.Collapsed;
                            break;
                        }
                    case "ButtonKnobVsLcdLower":
                        {
                            ImageVsLowerRow.Visibility = Visibility.Collapsed;
                            break;
                        }
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(8865005, ex);
            }
        }


        private void ComboBoxLcdKnobSensitivity_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (_userControlLoaded)
                {
                    Settings.Default.PZ70LcdKnobSensitivity = int.Parse(ComboBoxLcdKnobSensitivity.SelectedValue.ToString());
                    _multiPanelPZ70.LCDKnobSensitivity = int.Parse(ComboBoxLcdKnobSensitivity.SelectedValue.ToString());
                    Settings.Default.Save();
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(4370, ex);
            }
        }
    }
}
