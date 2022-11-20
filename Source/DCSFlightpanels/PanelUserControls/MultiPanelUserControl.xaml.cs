using NonVisuals.BindingClasses.DCSBIOSBindings;

namespace DCSFlightpanels.PanelUserControls
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using ClassLibraryCommon;

    using DCS_BIOS;
    using Bills;
    using CustomControls;
    using Interfaces;
    using Properties;
    using Windows;


    using MEF;
    using NonVisuals.EventArgs;
    using NonVisuals.Interfaces;
    using NonVisuals.Panels.Saitek.Panels;
    using NonVisuals.Panels.Saitek.Switches;
    using NonVisuals.Panels.Saitek;
    using NonVisuals.Panels;
    using NonVisuals.HID;

    /// <summary>
    /// Interaction logic for MultiPanelUserControl.xaml
    /// </summary>

    public partial class MultiPanelUserControl : UserControlBase, IGamingPanelListener, IProfileHandlerListener, IGamingPanelUserControl, IPanelUI
    {
        private readonly MultiPanelPZ70 _multiPanelPZ70;

        private bool _textBoxBillsSet;


        public MultiPanelUserControl(HIDSkeleton hidSkeleton)
        {
            InitializeComponent();
            
            _multiPanelPZ70 = new MultiPanelPZ70(hidSkeleton);
            AppEventHandler.AttachGamingPanelListener(this); 

            HideAllImages();
        }

        private bool _disposed;
        // Protected implementation of Dispose pattern.
        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _multiPanelPZ70.Dispose();
                    AppEventHandler.DetachGamingPanelListener(this);
                }

                _disposed = true;
            }

            // Call base class implementation.
            base.Dispose(disposing);
        }

        private void MultiPanelUserControl_OnLoaded(object sender, RoutedEventArgs e)
        {
            ComboBoxLcdKnobSensitivity.SelectedValue = Settings.Default.PZ70LcdKnobSensitivity;
            SetTextBoxBills();
            UserControlLoaded = true;
            ShowGraphicConfiguration();
        }
        

        public override GamingPanel GetGamingPanel()
        {
            return _multiPanelPZ70;
        }

        public override GamingPanelEnum GetPanelType()
        {
            return GamingPanelEnum.PZ70MultiPanel;
        }

        public void UpdatesHasBeenMissed(object sender, DCSBIOSUpdatesMissedEventArgs e) { }

        public string GetName()
        {
            return GetType().Name;
        }
        
        private void SetApplicationMode()
        {
            if (Common.IsEmulationModesFlagSet(EmulationMode.DCSBIOSOutputEnabled))
            {
                ButtonLcdUpper.Visibility = Visibility.Visible;
                ButtonLcdLower.Visibility = Visibility.Visible;
            }
            else
            {
                ButtonLcdUpper.Visibility = Visibility.Hidden;
                ButtonLcdLower.Visibility = Visibility.Hidden;
            }
        }

        public void SwitchesChanged(object sender, SwitchesChangedEventArgs e)
        {
            try
            {
                if (e.PanelType == GamingPanelEnum.PZ70MultiPanel && e.HidInstance.Equals(_multiPanelPZ70.HIDInstance))
                {
                    NotifyKnobChanges(e.Switches);
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        public void ProfileEvent(object sender, ProfileEventArgs e)
        {
            try
            {
                if (e.PanelBinding.PanelType == GamingPanelEnum.PZ70MultiPanel && _multiPanelPZ70.HIDInstance.Equals(e.PanelBinding.HIDInstance))
                {
                    ShowGraphicConfiguration();
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void ClearAll(bool clearAlsoProfile)
        {
            if (_textBoxBillsSet)
            {
                foreach (var textBox in Common.FindVisualChildren<PZ70TextBox>(this))
                {
                    if (textBox == TextBoxLogPZ70 || textBox.Bill == null)
                    {
                        continue;
                    }
                    textBox.Bill.ClearAll();
                }
            }

            if (clearAlsoProfile)
            {
                _multiPanelPZ70.ClearSettings(true);
            }

            ShowGraphicConfiguration();
        }

        private void SetTextBoxBills()
        {
            if (_textBoxBillsSet || !Common.FindVisualChildren<PZ70TextBox>(this).Any())
            {
                return;
            }

            foreach (var textBox in Common.FindVisualChildren<PZ70TextBox>(this))
            {
                if (textBox.Bill == null && !textBox.Equals(TextBoxLogPZ70))
                {
                    textBox.Bill = new BillPZ70(this, _multiPanelPZ70, textBox);
                }
                _textBoxBillsSet = true;
            }
            _textBoxBillsSet = true;
            
        }
        
        public void SettingsModified(object sender, PanelInfoArgs e)
        {
            if (e.HidInstance.Equals(_multiPanelPZ70.HIDInstance))
            {
                Dispatcher?.BeginInvoke((Action)(ShowGraphicConfiguration));
            }
        }
        
        public void SettingsApplied(object sender, PanelInfoArgs e)
        {
            try
            {
                if (e.HidInstance.Equals(_multiPanelPZ70.HIDInstance) && e.PanelType == GamingPanelEnum.PZ70MultiPanel)
                {
                    Dispatcher?.BeginInvoke((Action)(ShowGraphicConfiguration));
                    Dispatcher?.BeginInvoke((Action)(() => TextBoxLogPZ70.Text = string.Empty));
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }
        
        private void ButtonGetId_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_multiPanelPZ70 != null)
                {
                    TextBoxLogPZ70.Text = string.Empty;
                    TextBoxLogPZ70.Text = _multiPanelPZ70.HIDInstance;
                    Clipboard.SetText(_multiPanelPZ70.HIDInstance);
                    MessageBox.Show("Instance id has been copied to the ClipBoard.");
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
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
                                Dispatcher?.BeginInvoke(
                                    (Action)delegate
                                    {
                                        ImageLeftKnobAlt.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                        if (key.IsOn)
                                        {
                                            ClearAll(false);
                                            ShowGraphicConfiguration();
                                            LabelDialPos.Content = "ALT";
                                        }
                                    });
                                break;
                            }
                        case MultiPanelPZ70Knobs.KNOB_VS:
                            {
                                var key = multiKnob;
                                Dispatcher?.BeginInvoke(
                                    (Action)delegate
                                    {
                                        ImageLeftKnobVs.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                        if (key.IsOn)
                                        {
                                            ClearAll(false);
                                            ShowGraphicConfiguration();
                                            LabelDialPos.Content = "VS";
                                        }
                                    });
                                break;
                            }
                        case MultiPanelPZ70Knobs.KNOB_IAS:
                            {
                                var key = multiKnob;
                                Dispatcher?.BeginInvoke(
                                    (Action)delegate
                                    {
                                        ImageLeftKnobIas.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                        if (key.IsOn)
                                        {
                                            ClearAll(false);
                                            ShowGraphicConfiguration();
                                            LabelDialPos.Content = "IAS";
                                        }
                                    });
                                break;
                            }
                        case MultiPanelPZ70Knobs.KNOB_HDG:
                            {
                                var key = multiKnob;
                                Dispatcher?.BeginInvoke(
                                    (Action)delegate
                                    {
                                        ImageLeftKnobHdg.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                        if (key.IsOn)
                                        {
                                            ClearAll(false);
                                            ShowGraphicConfiguration();
                                            LabelDialPos.Content = "HDG";
                                        }
                                    });
                                break;
                            }
                        case MultiPanelPZ70Knobs.KNOB_CRS:
                            {
                                var key = multiKnob;
                                Dispatcher?.BeginInvoke(
                                    (Action)delegate
                                    {
                                        ImageLeftKnobCrs.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;

                                        if (key.IsOn)
                                        {
                                            ClearAll(false);
                                            ShowGraphicConfiguration();
                                            LabelDialPos.Content = "CRS";
                                        }
                                    });
                                break;
                            }
                        case MultiPanelPZ70Knobs.AP_BUTTON:
                            {
                                Dispatcher?.BeginInvoke(
                                    (Action)delegate
                                    {
                                        ImageLcdButtonAp.Visibility = _multiPanelPZ70.LCDButtonByteListHandler.IsOn(_multiPanelPZ70.PZ70DialPosition, multiKnob.MultiPanelPZ70Knob) ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case MultiPanelPZ70Knobs.HDG_BUTTON:
                            {
                                Dispatcher?.BeginInvoke(
                                    (Action)delegate
                                    {
                                        ImageLcdButtonHdg.Visibility = _multiPanelPZ70.LCDButtonByteListHandler.IsOn(_multiPanelPZ70.PZ70DialPosition, multiKnob.MultiPanelPZ70Knob) ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case MultiPanelPZ70Knobs.NAV_BUTTON:
                            {
                                Dispatcher?.BeginInvoke(
                                    (Action)delegate
                                    {
                                        ImageLcdButtonNav.Visibility = _multiPanelPZ70.LCDButtonByteListHandler.IsOn(_multiPanelPZ70.PZ70DialPosition, multiKnob.MultiPanelPZ70Knob) ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case MultiPanelPZ70Knobs.IAS_BUTTON:
                            {
                                Dispatcher?.BeginInvoke(
                                    (Action)delegate
                                    {
                                        ImageLcdButtonIas.Visibility = _multiPanelPZ70.LCDButtonByteListHandler.IsOn(_multiPanelPZ70.PZ70DialPosition, multiKnob.MultiPanelPZ70Knob) ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case MultiPanelPZ70Knobs.ALT_BUTTON:
                            {
                                Dispatcher?.BeginInvoke(
                                    (Action)delegate
                                    {
                                        ImageLcdButtonAlt.Visibility = _multiPanelPZ70.LCDButtonByteListHandler.IsOn(_multiPanelPZ70.PZ70DialPosition, multiKnob.MultiPanelPZ70Knob) ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case MultiPanelPZ70Knobs.VS_BUTTON:
                            {
                                Dispatcher?.BeginInvoke(
                                    (Action)delegate
                                    {
                                        ImageLcdButtonVs.Visibility = _multiPanelPZ70.LCDButtonByteListHandler.IsOn(_multiPanelPZ70.PZ70DialPosition, multiKnob.MultiPanelPZ70Knob) ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case MultiPanelPZ70Knobs.APR_BUTTON:
                            {
                                Dispatcher?.BeginInvoke(
                                    (Action)delegate
                                    {
                                        ImageLcdButtonApr.Visibility = _multiPanelPZ70.LCDButtonByteListHandler.IsOn(_multiPanelPZ70.PZ70DialPosition, multiKnob.MultiPanelPZ70Knob) ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case MultiPanelPZ70Knobs.REV_BUTTON:
                            {
                                Dispatcher?.BeginInvoke(
                                    (Action)delegate
                                    {
                                        ImageLcdButtonRev.Visibility = _multiPanelPZ70.LCDButtonByteListHandler.IsOn(_multiPanelPZ70.PZ70DialPosition, multiKnob.MultiPanelPZ70Knob) ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case MultiPanelPZ70Knobs.LCD_WHEEL_DEC:
                            {
                                var key = multiKnob;
                                Dispatcher?.BeginInvoke(
                                    (Action)delegate
                                    {
                                        ImageLcdKnobDec.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case MultiPanelPZ70Knobs.LCD_WHEEL_INC:
                            {
                                var key = multiKnob;
                                Dispatcher?.BeginInvoke(
                                    (Action)delegate
                                    {
                                        ImageLcdKnobInc.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case MultiPanelPZ70Knobs.AUTO_THROTTLE:
                            {
                                var key = multiKnob;
                                Dispatcher?.BeginInvoke(
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
                                Dispatcher?.BeginInvoke(
                                    (Action)delegate
                                    {
                                        ImageFlapsUp.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case MultiPanelPZ70Knobs.FLAPS_LEVER_DOWN:
                            {
                                var key = multiKnob;
                                Dispatcher?.BeginInvoke(
                                    (Action)delegate
                                    {
                                        ImageFlapsDown.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case MultiPanelPZ70Knobs.PITCH_TRIM_WHEEL_UP:
                            {
                                var key = multiKnob;
                                Dispatcher?.BeginInvoke(
                                    (Action)delegate
                                    {
                                        ImagePitchUp.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case MultiPanelPZ70Knobs.PITCH_TRIM_WHEEL_DOWN:
                            {
                                var key = multiKnob;
                                Dispatcher?.BeginInvoke(
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
                Common.ShowErrorMessageBox(ex);
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

            ImageLcdUpperRow.Visibility = Visibility.Collapsed;
            ImageLcdLowerRow.Visibility = Visibility.Collapsed;
        }

        private void ButtonLcd_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var button = (Button)sender;

                switch (button.Name)
                {
                    case "ButtonLcdUpper":
                        {
                            ButtonLcdConfig(PZ70LCDPosition.UpperLCD, button, "Data to display on upper LCD Row");
                            break;
                        }
                    case "ButtonLcdLower":
                        {
                            ButtonLcdConfig(PZ70LCDPosition.LowerLCD, button, "Data to display on lower LCD Row");
                            break;
                        }
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void ButtonLcdConfig(PZ70LCDPosition pz70LCDPosition, Button button, string description)
        {
            try
            {
                if (button == null)
                {
                    throw new Exception("Failed to locate which button was clicked.");
                }
                DCSBiosOutputFormulaWindow dcsBiosOutputFormulaWindow = null;
                foreach (var dcsbiosBindingLCDPZ70 in _multiPanelPZ70.LCDBindings)
                {
                    if (dcsbiosBindingLCDPZ70.DialPosition == _multiPanelPZ70.PZ70DialPosition && dcsbiosBindingLCDPZ70.PZ70LCDPosition == pz70LCDPosition)
                    {
                        if (dcsbiosBindingLCDPZ70.UseFormula)
                        {
                            dcsBiosOutputFormulaWindow = new DCSBiosOutputFormulaWindow(description, dcsbiosBindingLCDPZ70.DCSBIOSOutputFormulaObject, dcsbiosBindingLCDPZ70.LimitDecimalPlaces, dcsbiosBindingLCDPZ70.DecimalPlaces);
                            break;
                        }
                        dcsBiosOutputFormulaWindow = new DCSBiosOutputFormulaWindow(description, dcsbiosBindingLCDPZ70.DCSBIOSOutputObject, dcsbiosBindingLCDPZ70.LimitDecimalPlaces, dcsbiosBindingLCDPZ70.DecimalPlaces);
                        break;
                    }
                }

                if (dcsBiosOutputFormulaWindow == null)
                {
                    dcsBiosOutputFormulaWindow = new DCSBiosOutputFormulaWindow(description);
                }

                dcsBiosOutputFormulaWindow.ShowDialog();
                if (dcsBiosOutputFormulaWindow.DialogResult.HasValue && dcsBiosOutputFormulaWindow.DialogResult.Value)
                {
                    if (dcsBiosOutputFormulaWindow.UseFormula())
                    {
                        var dcsBiosOutputFormula = dcsBiosOutputFormulaWindow.DCSBIOSOutputFormula;
                        UpdateDCSBIOSBindingLCD(true, false, null, dcsBiosOutputFormula, button, false, 0); 
                    }
                    else if (dcsBiosOutputFormulaWindow.UseSingleDCSBiosControl())
                    {
                        var dcsBiosOutput = dcsBiosOutputFormulaWindow.DCSBiosOutput;
                        UpdateDCSBIOSBindingLCD(false, false, dcsBiosOutput, null, button, false, 0); 
                    }
                    else
                    {
                        // Delete config
                        UpdateDCSBIOSBindingLCD(false, true, null, null, button, false, 0); 
                    }
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void ShowGraphicConfiguration()
        {
            try
            {
                if (!UserControlLoaded || !_textBoxBillsSet)
                {
                    return;
                }

                SetApplicationMode();
                ImageLcdButtonAp.Visibility = _multiPanelPZ70.LCDButtonByteListHandler.IsOn(_multiPanelPZ70.PZ70DialPosition, MultiPanelPZ70Knobs.AP_BUTTON) ? Visibility.Visible : Visibility.Collapsed;
                ImageLcdButtonHdg.Visibility = _multiPanelPZ70.LCDButtonByteListHandler.IsOn(_multiPanelPZ70.PZ70DialPosition, MultiPanelPZ70Knobs.HDG_BUTTON) ? Visibility.Visible : Visibility.Collapsed;
                ImageLcdButtonNav.Visibility = _multiPanelPZ70.LCDButtonByteListHandler.IsOn(_multiPanelPZ70.PZ70DialPosition, MultiPanelPZ70Knobs.NAV_BUTTON) ? Visibility.Visible : Visibility.Collapsed;
                ImageLcdButtonIas.Visibility = _multiPanelPZ70.LCDButtonByteListHandler.IsOn(_multiPanelPZ70.PZ70DialPosition, MultiPanelPZ70Knobs.IAS_BUTTON) ? Visibility.Visible : Visibility.Collapsed;
                ImageLcdButtonAlt.Visibility = _multiPanelPZ70.LCDButtonByteListHandler.IsOn(_multiPanelPZ70.PZ70DialPosition, MultiPanelPZ70Knobs.ALT_BUTTON) ? Visibility.Visible : Visibility.Collapsed;
                ImageLcdButtonVs.Visibility = _multiPanelPZ70.LCDButtonByteListHandler.IsOn(_multiPanelPZ70.PZ70DialPosition, MultiPanelPZ70Knobs.VS_BUTTON) ? Visibility.Visible : Visibility.Collapsed;
                ImageLcdButtonApr.Visibility = _multiPanelPZ70.LCDButtonByteListHandler.IsOn(_multiPanelPZ70.PZ70DialPosition, MultiPanelPZ70Knobs.APR_BUTTON) ? Visibility.Visible : Visibility.Collapsed;
                ImageLcdButtonRev.Visibility = _multiPanelPZ70.LCDButtonByteListHandler.IsOn(_multiPanelPZ70.PZ70DialPosition, MultiPanelPZ70Knobs.REV_BUTTON) ? Visibility.Visible : Visibility.Collapsed;

                foreach (var keyBinding in _multiPanelPZ70.KeyBindingsHashSet)
                {
                    var textBox = (PZ70TextBox)GetTextBox(keyBinding.MultiPanelPZ70Knob, keyBinding.WhenTurnedOn);
                    if (keyBinding.DialPosition == _multiPanelPZ70.PZ70DialPosition)
                    {
                        if (keyBinding.OSKeyPress != null)
                        {
                            textBox.Bill.KeyPress = keyBinding.OSKeyPress;
                        }
                    }
                }

                foreach (var operatingSystemCommand in _multiPanelPZ70.OSCommandHashSet)
                {
                    var textBox = (PZ70TextBox)GetTextBox(operatingSystemCommand.MultiPanelPZ70Knob, operatingSystemCommand.WhenTurnedOn);
                    if (operatingSystemCommand.DialPosition == _multiPanelPZ70.PZ70DialPosition)
                        if (operatingSystemCommand.OSCommandObject != null)
                        {
                            textBox.Bill.OSCommandObject = operatingSystemCommand.OSCommandObject;
                        }
                }

                foreach (var dcsBiosBinding in _multiPanelPZ70.DCSBiosBindings)
                {
                    var textBox = (PZ70TextBox)GetTextBox(dcsBiosBinding.MultiPanelPZ70Knob, dcsBiosBinding.WhenTurnedOn);
                    if (dcsBiosBinding.DialPosition == _multiPanelPZ70.PZ70DialPosition && dcsBiosBinding.DCSBIOSInputs.Count > 0)
                    {
                        textBox.Bill.DCSBIOSBinding = dcsBiosBinding;
                    }
                }


                foreach (var bipLink in _multiPanelPZ70.BIPLinkHashSet)
                {
                    var textBox = (PZ70TextBox)GetTextBox(bipLink.MultiPanelPZ70Knob, bipLink.WhenTurnedOn);
                    if (bipLink.DialPosition == _multiPanelPZ70.PZ70DialPosition && bipLink.BIPLights.Count > 0)
                    {
                        textBox.Bill.BipLink = bipLink;
                    }
                }

                ImageLcdUpperRow.Visibility = Visibility.Collapsed;
                ImageLcdLowerRow.Visibility = Visibility.Collapsed;
                //Dial position IAS HDG CRS -> Only upper LCD row can be used -> Hide Lower Button
                if (Common.NoDCSBIOSEnabled() || _multiPanelPZ70.PZ70DialPosition == PZ70DialPosition.IAS || _multiPanelPZ70.PZ70DialPosition == PZ70DialPosition.HDG || _multiPanelPZ70.PZ70DialPosition == PZ70DialPosition.CRS)
                {
                    ButtonLcdLower.Visibility = Visibility.Hidden;
                }
                else if (Common.IsEmulationModesFlagSet(EmulationMode.DCSBIOSOutputEnabled))
                {
                    ButtonLcdLower.Visibility = Visibility.Visible;
                }

                foreach (var dcsBiosBindingLCD in _multiPanelPZ70.LCDBindings)
                {
                    if (dcsBiosBindingLCD.DialPosition == _multiPanelPZ70.PZ70DialPosition && dcsBiosBindingLCD.PZ70LCDPosition == PZ70LCDPosition.UpperLCD && dcsBiosBindingLCD.HasBinding)
                    {
                        ImageLcdUpperRow.Visibility = Visibility.Visible;
                        if (dcsBiosBindingLCD.UseFormula)
                        {
                            ButtonLcdUpper.Tag = dcsBiosBindingLCD.DCSBIOSOutputFormulaObject;
                            ButtonLcdUpper.ToolTip = dcsBiosBindingLCD.DCSBIOSOutputFormulaObject.ToString();
                        }
                        else
                        {
                            ButtonLcdUpper.Tag = dcsBiosBindingLCD.DCSBIOSOutputObject;
                            ButtonLcdUpper.ToolTip = dcsBiosBindingLCD.DCSBIOSOutputObject.ToString();
                        }
                    }
                    if (dcsBiosBindingLCD.DialPosition == _multiPanelPZ70.PZ70DialPosition && dcsBiosBindingLCD.PZ70LCDPosition == PZ70LCDPosition.LowerLCD && dcsBiosBindingLCD.HasBinding)
                    {
                        ImageLcdLowerRow.Visibility = Visibility.Visible;
                        if (dcsBiosBindingLCD.UseFormula)
                        {
                            ButtonLcdLower.Tag = dcsBiosBindingLCD.DCSBIOSOutputFormulaObject;
                            ButtonLcdLower.ToolTip = dcsBiosBindingLCD.DCSBIOSOutputFormulaObject.ToString();
                        }
                        else
                        {
                            ButtonLcdLower.Tag = dcsBiosBindingLCD.DCSBIOSOutputObject;
                            ButtonLcdLower.ToolTip = dcsBiosBindingLCD.DCSBIOSOutputObject.ToString();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
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
                Common.ShowErrorMessageBox(ex);
            }
        }
        
        private void UpdateDCSBIOSBindingLCD(bool useFormula,
            bool deleteConfig,
            DCSBIOSOutput dcsbiosOutput,
            DCSBIOSOutputFormula dcsbiosOutputFormula,
            Button button,
            bool limitDecimalPlaces,
            int decimalPlaces)
        {
            try
            {
                if (deleteConfig)
                {
                    if (button.Equals(ButtonLcdUpper))
                    {
                        ImageLcdUpperRow.Visibility = Visibility.Hidden;
                        _multiPanelPZ70.RemoveDCSBIOSLcdBinding(PZ70LCDPosition.UpperLCD);
                    }

                    if (button.Equals(ButtonLcdLower))
                    {
                        ImageLcdLowerRow.Visibility = Visibility.Hidden;
                        _multiPanelPZ70.RemoveDCSBIOSLcdBinding(PZ70LCDPosition.LowerLCD);
                    }
                }
                else if (!useFormula)
                {
                    if (button.Equals(ButtonLcdUpper))
                    {
                        ImageLcdUpperRow.Visibility = dcsbiosOutput == null ? Visibility.Collapsed : Visibility.Visible;
                        _multiPanelPZ70.AddOrUpdateLCDBinding(dcsbiosOutput, PZ70LCDPosition.UpperLCD, limitDecimalPlaces, decimalPlaces);
                    }

                    if (button.Equals(ButtonLcdLower))
                    {
                        ImageLcdLowerRow.Visibility = dcsbiosOutput == null ? Visibility.Collapsed : Visibility.Visible;
                        _multiPanelPZ70.AddOrUpdateLCDBinding(dcsbiosOutput, PZ70LCDPosition.LowerLCD, limitDecimalPlaces, decimalPlaces);
                    }
                }
                else // useFormula
                {
                    if (button.Equals(ButtonLcdUpper))
                    {
                        ImageLcdUpperRow.Visibility = dcsbiosOutputFormula == null ? Visibility.Collapsed : Visibility.Visible;
                        _multiPanelPZ70.AddOrUpdateLCDBinding(dcsbiosOutputFormula, PZ70LCDPosition.UpperLCD, limitDecimalPlaces, decimalPlaces);
                    }

                    if (button.Equals(ButtonLcdLower))
                    {
                        ImageLcdLowerRow.Visibility = dcsbiosOutputFormula == null ? Visibility.Collapsed : Visibility.Visible;
                        _multiPanelPZ70.AddOrUpdateLCDBinding(dcsbiosOutputFormula, PZ70LCDPosition.LowerLCD, limitDecimalPlaces, decimalPlaces);
                    }
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }
        
        private void NotifyKnobChanges(HashSet<object> knobs)
        {
            try
            {
                //Set focus to this so that virtual keypresses won't affect settings
                Dispatcher?.BeginInvoke((Action)(() => TextBoxLogPZ70.Focus()));
                foreach (var knob in knobs)
                {
                    var multiPanelKnob = (MultiPanelKnob)knob;

                    if (_multiPanelPZ70.ForwardPanelEvent)
                    {
                        if (!string.IsNullOrEmpty(_multiPanelPZ70.GetKeyPressForLoggingPurposes(multiPanelKnob)))
                        {
                            Dispatcher?.BeginInvoke(
                                (Action)
                                (() =>
                                 TextBoxLogPZ70.Text = TextBoxLogPZ70.Text.Insert(0, _multiPanelPZ70.GetKeyPressForLoggingPurposes(multiPanelKnob) + "\n")));
                        }
                    }
                    else
                    {
                        Dispatcher?.BeginInvoke(
                            (Action)
                            (() =>
                             TextBoxLogPZ70.Text = TextBoxLogPZ70.Text.Insert(0, "No action taken, panel events Disabled.\n")));
                    }
                }
                SetGraphicsState(knobs);
            }
            catch (Exception ex)
            {
                Dispatcher?.BeginInvoke(
                    (Action)
                    (() =>
                     TextBoxLogPZ70.Text = TextBoxLogPZ70.Text.Insert(0, "0x16" + ex.Message + ".\n")));
                Common.ShowErrorMessageBox(ex);
            }
        }
        
        private void ComboBoxLcdKnobSensitivity_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (UserControlLoaded)
                {
                    Settings.Default.PZ70LcdKnobSensitivity = int.Parse(ComboBoxLcdKnobSensitivity.SelectedValue.ToString());
                    _multiPanelPZ70.LCDKnobSensitivity = int.Parse(ComboBoxLcdKnobSensitivity.SelectedValue.ToString());
                    Settings.Default.Save();
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }
        
        public PanelSwitchOnOff GetSwitch(TextBox textBox)
        {
            return textBox switch
            {
                TextBox t when t.Equals(TextBoxLcdKnobDecrease) => new PZ70SwitchOnOff(MultiPanelPZ70Knobs.LCD_WHEEL_DEC, true),
                TextBox t when t.Equals(TextBoxLcdKnobIncrease) => new PZ70SwitchOnOff(MultiPanelPZ70Knobs.LCD_WHEEL_INC, true),
                TextBox t when t.Equals(TextBoxAutoThrottleOn) => new PZ70SwitchOnOff(MultiPanelPZ70Knobs.AUTO_THROTTLE, true),
                TextBox t when t.Equals(TextBoxAutoThrottleOff) => new PZ70SwitchOnOff(MultiPanelPZ70Knobs.AUTO_THROTTLE, false),
                TextBox t when t.Equals(TextBoxFlapsUp) => new PZ70SwitchOnOff(MultiPanelPZ70Knobs.FLAPS_LEVER_UP, true),
                TextBox t when t.Equals(TextBoxFlapsUpRelease) => new PZ70SwitchOnOff(MultiPanelPZ70Knobs.FLAPS_LEVER_UP, false),
                TextBox t when t.Equals(TextBoxFlapsDown) => new PZ70SwitchOnOff(MultiPanelPZ70Knobs.FLAPS_LEVER_DOWN, true),
                TextBox t when t.Equals(TextBoxFlapsDownRelease) => new PZ70SwitchOnOff(MultiPanelPZ70Knobs.FLAPS_LEVER_DOWN, false),
                TextBox t when t.Equals(TextBoxPitchTrimUp) => new PZ70SwitchOnOff(MultiPanelPZ70Knobs.PITCH_TRIM_WHEEL_UP, true),
                TextBox t when t.Equals(TextBoxPitchTrimDown) => new PZ70SwitchOnOff(MultiPanelPZ70Knobs.PITCH_TRIM_WHEEL_DOWN, true),
                TextBox t when t.Equals(TextBoxApButtonOn) => new PZ70SwitchOnOff(MultiPanelPZ70Knobs.AP_BUTTON, true),
                TextBox t when t.Equals(TextBoxApButtonOff) => new PZ70SwitchOnOff(MultiPanelPZ70Knobs.AP_BUTTON, false),
                TextBox t when t.Equals(TextBoxHdgButtonOn) => new PZ70SwitchOnOff(MultiPanelPZ70Knobs.HDG_BUTTON, true),
                TextBox t when t.Equals(TextBoxHdgButtonOff) => new PZ70SwitchOnOff(MultiPanelPZ70Knobs.HDG_BUTTON, false),
                TextBox t when t.Equals(TextBoxNavButtonOn) => new PZ70SwitchOnOff(MultiPanelPZ70Knobs.NAV_BUTTON, true),
                TextBox t when t.Equals(TextBoxNavButtonOff) => new PZ70SwitchOnOff(MultiPanelPZ70Knobs.NAV_BUTTON, false),
                TextBox t when t.Equals(TextBoxIasButtonOn) => new PZ70SwitchOnOff(MultiPanelPZ70Knobs.IAS_BUTTON, true),
                TextBox t when t.Equals(TextBoxIasButtonOff) => new PZ70SwitchOnOff(MultiPanelPZ70Knobs.IAS_BUTTON, false),
                TextBox t when t.Equals(TextBoxAltButtonOn) => new PZ70SwitchOnOff(MultiPanelPZ70Knobs.ALT_BUTTON, true),
                TextBox t when t.Equals(TextBoxAltButtonOff) => new PZ70SwitchOnOff(MultiPanelPZ70Knobs.ALT_BUTTON, false),
                TextBox t when t.Equals(TextBoxVsButtonOn) => new PZ70SwitchOnOff(MultiPanelPZ70Knobs.VS_BUTTON, true),
                TextBox t when t.Equals(TextBoxVsButtonOff) => new PZ70SwitchOnOff(MultiPanelPZ70Knobs.VS_BUTTON, false),
                TextBox t when t.Equals(TextBoxAprButtonOn) => new PZ70SwitchOnOff(MultiPanelPZ70Knobs.APR_BUTTON, true),
                TextBox t when t.Equals(TextBoxAprButtonOff) => new PZ70SwitchOnOff(MultiPanelPZ70Knobs.APR_BUTTON, false),
                TextBox t when t.Equals(TextBoxRevButtonOn) => new PZ70SwitchOnOff(MultiPanelPZ70Knobs.REV_BUTTON, true),
                TextBox t when t.Equals(TextBoxRevButtonOff) => new PZ70SwitchOnOff(MultiPanelPZ70Knobs.REV_BUTTON, false),
                _ => throw new Exception($"Failed to find MultiPanel knob for TextBox {textBox.Name}")
            };
        }


        public TextBox GetTextBox(object panelSwitch, bool isTurnedOn)
        {
            var knob = (MultiPanelPZ70Knobs)panelSwitch;
            return (knob, isTurnedOn) switch {
                (MultiPanelPZ70Knobs.LCD_WHEEL_DEC, true) => TextBoxLcdKnobDecrease,
                (MultiPanelPZ70Knobs.LCD_WHEEL_INC, true) => TextBoxLcdKnobIncrease,
                (MultiPanelPZ70Knobs.AUTO_THROTTLE, true) => TextBoxAutoThrottleOn,
                (MultiPanelPZ70Knobs.AUTO_THROTTLE, false) => TextBoxAutoThrottleOff,
                (MultiPanelPZ70Knobs.FLAPS_LEVER_UP, true) => TextBoxFlapsUp,
                (MultiPanelPZ70Knobs.FLAPS_LEVER_UP, false) => TextBoxFlapsUpRelease,
                (MultiPanelPZ70Knobs.FLAPS_LEVER_DOWN, true) => TextBoxFlapsDown,
                (MultiPanelPZ70Knobs.FLAPS_LEVER_DOWN, false) => TextBoxFlapsDownRelease,
                (MultiPanelPZ70Knobs.PITCH_TRIM_WHEEL_UP, true) => TextBoxPitchTrimUp,
                (MultiPanelPZ70Knobs.PITCH_TRIM_WHEEL_DOWN, true) => TextBoxPitchTrimDown,
                (MultiPanelPZ70Knobs.AP_BUTTON, true) => TextBoxApButtonOn,
                (MultiPanelPZ70Knobs.AP_BUTTON, false) => TextBoxApButtonOff,
                (MultiPanelPZ70Knobs.HDG_BUTTON, true) => TextBoxHdgButtonOn,
                (MultiPanelPZ70Knobs.HDG_BUTTON, false) => TextBoxHdgButtonOff,
                (MultiPanelPZ70Knobs.NAV_BUTTON, true) => TextBoxNavButtonOn,
                (MultiPanelPZ70Knobs.NAV_BUTTON, false) => TextBoxNavButtonOff,
                (MultiPanelPZ70Knobs.IAS_BUTTON, true) => TextBoxIasButtonOn,
                (MultiPanelPZ70Knobs.IAS_BUTTON, false) => TextBoxIasButtonOff,
                (MultiPanelPZ70Knobs.ALT_BUTTON, true) => TextBoxAltButtonOn,
                (MultiPanelPZ70Knobs.ALT_BUTTON, false) => TextBoxAltButtonOff,
                (MultiPanelPZ70Knobs.VS_BUTTON, true) => TextBoxVsButtonOn,
                (MultiPanelPZ70Knobs.VS_BUTTON, false) => TextBoxVsButtonOff,
                (MultiPanelPZ70Knobs.APR_BUTTON, true) => TextBoxAprButtonOn,
                (MultiPanelPZ70Knobs.APR_BUTTON, false) => TextBoxAprButtonOff,
                (MultiPanelPZ70Knobs.REV_BUTTON, true) => TextBoxRevButtonOn,
                (MultiPanelPZ70Knobs.REV_BUTTON, false) => TextBoxRevButtonOff,
                _ => throw new Exception($"Failed to find TextBox for MultiPanel Knob: {knob} & value {isTurnedOn}")
            };
        }

        private void ButtonIdentify_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                _multiPanelPZ70.Identify();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void ButtonClearSettings_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (MessageBox.Show("Clear all settings?", "Confirm", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                {
                    ClearAll(true);
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }
    }
}
