using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ClassLibraryCommon;
using DCS_BIOS;
using NonVisuals;
using DCSFlightpanels.Properties;
using DCSFlightpanels.Bills;
using DCSFlightpanels.CustomControls;
using DCSFlightpanels.Windows;
using NonVisuals.DCSBIOSBindings;
using NonVisuals.Interfaces;
using NonVisuals.Saitek;

namespace DCSFlightpanels.PanelUserControls
{
    /// <summary>
    /// Interaction logic for MultiPanelUserControl.xaml
    /// </summary>

    public partial class MultiPanelUserControl : UserControlBase, IGamingPanelListener, IProfileHandlerListener, IGamingPanelUserControl
    {
        private readonly MultiPanelPZ70 _multiPanelPZ70;
        
        private bool _textBoxBillsSet;

        public MultiPanelUserControl(HIDSkeleton hidSkeleton, TabItem parentTabItem, IGlobalHandler globalHandler)
        {
            InitializeComponent();
            ParentTabItem = parentTabItem;

            hidSkeleton.HIDReadDevice.Removed += DeviceRemovedHandler;

            _multiPanelPZ70 = new MultiPanelPZ70(hidSkeleton);
            _multiPanelPZ70.Attach((IGamingPanelListener)this);
            globalHandler.Attach(_multiPanelPZ70);
            GlobalHandler = globalHandler;

            HideAllImages();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _multiPanelPZ70.Dispose();
            }
        }
        
        private void MultiPanelUserControl_OnLoaded(object sender, RoutedEventArgs e)
        {
            ComboBoxLcdKnobSensitivity.SelectedValue = Settings.Default.PZ70LcdKnobSensitivity;
            SetTextBoxBills();
            SetContextMenuClickHandlers();
            UserControlLoaded = true;
            ShowGraphicConfiguration();
        }

        public void BipPanelRegisterEvent(object sender, BipPanelRegisteredEventArgs e)
        {
            var now = DateTime.Now.Ticks;
            RemoveContextMenuClickHandlers();
            SetContextMenuClickHandlers();
        }

        public override GamingPanel GetGamingPanel()
        {
            return _multiPanelPZ70;
        }

        public void UpdatesHasBeenMissed(object sender, DCSBIOSUpdatesMissedEventArgs e) { }

        public string GetName()
        {
            return GetType().Name;
        }

        public void SelectedAirframe(object sender, AirframeEventArgs e)
        {
            try
            {
                SetApplicationMode();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox( ex);
            }
        }

        private void SetApplicationMode()
        {
            if (Common.IsOperationModeFlagSet(OperationFlag.DCSBIOSOutputEnabled))
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

        public void UISwitchesChanged(object sender, SwitchesChangedEventArgs e)
        {
            try
            {
                if (e.GamingPanelEnum == GamingPanelEnum.PZ70MultiPanel && e.UniqueId.Equals(_multiPanelPZ70.InstanceId))
                {
                    NotifyKnobChanges(e.Switches);
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox( ex);
            }
        }

        public void PanelSettingsReadFromFile(object sender, SettingsReadFromFileEventArgs e)
        {
            try
            {
                ShowGraphicConfiguration();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox( ex);
            }
        }

        public void SettingsCleared(object sender, PanelEventArgs e)
        {
            try
            {
                ClearAll(false);
                ShowGraphicConfiguration();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox( ex);
            }
        }

        private void ClearAll(bool clearAlsoProfile = true)
        {
            foreach (var textBox in Common.FindVisualChildren<PZ70TextBox>(this))
            {
                if (textBox.Equals(TextBoxLogPZ70))
                {
                    continue;
                }
                textBox.Bill.Clear();
            }
            if (clearAlsoProfile)
            {
                _multiPanelPZ70.ClearSettings();
            }
        }

        private void SetTextBoxBills()
        {
            if (_textBoxBillsSet || !Common.FindVisualChildren<PZ70TextBox>(this).Any())
            {
                return;
            }
            foreach (var textBox in Common.FindVisualChildren<PZ70TextBox>(this))
            {
                if (!textBox.Equals(TextBoxLogPZ70))
                {
                    textBox.Bill = new BillPZ70(textBox, GetPZ70Knob(textBox));
                }
            }
            _textBoxBillsSet = true;
        }

        public void LedLightChanged(object sender, LedLightChangeEventArgs e) { }

        public void PanelSettingsChanged(object sender, PanelEventArgs e) { }

        public void PanelDataAvailable(object sender, PanelDataToDCSBIOSEventEventArgs e) { }

        public void SettingsApplied(object sender, PanelEventArgs e)
        {
            try
            {
                if (e.UniqueId.Equals(_multiPanelPZ70.InstanceId) && e.GamingPanelEnum == GamingPanelEnum.PZ70MultiPanel)
                {
                    Dispatcher?.BeginInvoke((Action)(ShowGraphicConfiguration));
                    Dispatcher?.BeginInvoke((Action)(() => TextBoxLogPZ70.Text = ""));
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox( ex);
            }
        }

        public void DeviceAttached(object sender, PanelEventArgs e) { }

        public void DeviceDetached(object sender, PanelEventArgs e) { }

        private void ButtonGetId_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_multiPanelPZ70 != null)
                {
                    TextBoxLogPZ70.Text = "";
                    TextBoxLogPZ70.Text = _multiPanelPZ70.InstanceId;
                    Clipboard.SetText(_multiPanelPZ70.InstanceId);
                    MessageBox.Show("Instance id has been copied to the ClipBoard.");
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox( ex);
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
                Common.ShowErrorMessageBox( ex);
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
                Common.ShowErrorMessageBox( ex);
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
                            dcsBiosOutputFormulaWindow = new DCSBiosOutputFormulaWindow(GlobalHandler.GetAirframe(), description, dcsbiosBindingLCDPZ70.DCSBIOSOutputFormulaObject);
                            break;
                        }
                        dcsBiosOutputFormulaWindow = new DCSBiosOutputFormulaWindow(GlobalHandler.GetAirframe(), description, dcsbiosBindingLCDPZ70.DCSBIOSOutputObject);
                        break;
                    }
                }
                if (dcsBiosOutputFormulaWindow == null)
                {
                    dcsBiosOutputFormulaWindow = new DCSBiosOutputFormulaWindow(GlobalHandler.GetAirframe(), description);
                }

                dcsBiosOutputFormulaWindow.ShowDialog();
                if (dcsBiosOutputFormulaWindow.DialogResult.HasValue && dcsBiosOutputFormulaWindow.DialogResult.Value)
                {
                    if (dcsBiosOutputFormulaWindow.UseFormula())
                    {
                        var dcsBiosOutputFormula = dcsBiosOutputFormulaWindow.DCSBIOSOutputFormula;
                        UpdateDCSBIOSBindingLCD(true, false, null, dcsBiosOutputFormula, button);
                    }
                    else if (dcsBiosOutputFormulaWindow.UseSingleDCSBiosControl())
                    {
                        var dcsBiosOutput = dcsBiosOutputFormulaWindow.DCSBiosOutput;
                        UpdateDCSBIOSBindingLCD(false, false, dcsBiosOutput, null, button);
                    }
                    else
                    {
                        //Delete config
                        UpdateDCSBIOSBindingLCD(false, true, null, null, button);
                    }
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox( ex);
            }
        }

        private void TextBoxLostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                var textBox = (PZ70TextBox)sender;
                if (textBox.Bill.ContainsBIPLink())
                {
                    ((PZ70TextBox)sender).Background = Brushes.Bisque;
                }
                else
                {
                    ((PZ70TextBox)sender).Background = Brushes.White;
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox( ex);
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
                    var textBox = GetTextBox(keyBinding.MultiPanelPZ70Knob, keyBinding.WhenTurnedOn);
                    if (keyBinding.DialPosition == _multiPanelPZ70.PZ70DialPosition)
                    {
                        if (keyBinding.OSKeyPress != null)
                        {
                            textBox.Bill.KeyPress = keyBinding.OSKeyPress;
                        }
                    }
                }

                foreach (var osCommand in _multiPanelPZ70.OSCommandHashSet)
                {
                    var textBox = GetTextBox(osCommand.MultiPanelPZ70Knob, osCommand.WhenTurnedOn);
                    if (osCommand.DialPosition == _multiPanelPZ70.PZ70DialPosition)
                        if (osCommand.OSCommandObject != null)
                        {
                            textBox.Bill.OSCommandObject = osCommand.OSCommandObject;
                        }
                }
            
                foreach (var dcsBiosBinding in _multiPanelPZ70.DCSBiosBindings)
                {
                    var textBox = GetTextBox(dcsBiosBinding.MultiPanelPZ70Knob, dcsBiosBinding.WhenTurnedOn);
                    if (dcsBiosBinding.DialPosition == _multiPanelPZ70.PZ70DialPosition && dcsBiosBinding.DCSBIOSInputs.Count > 0)
                    {
                        textBox.Bill.DCSBIOSBinding = dcsBiosBinding;
                    }
                }


                foreach (var bipLink in _multiPanelPZ70.BIPLinkHashSet)
                {
                    var textBox = GetTextBox(bipLink.MultiPanelPZ70Knob, bipLink.WhenTurnedOn);
                    if (bipLink.DialPosition == _multiPanelPZ70.PZ70DialPosition && bipLink.BIPLights.Count > 0)
                    {
                        textBox.Bill.BIPLink = bipLink;
                    }
                }

                ImageLcdUpperRow.Visibility = Visibility.Collapsed;
                ImageLcdLowerRow.Visibility = Visibility.Collapsed;
                //Dial position IAS HDG CRS -> Only upper LCD row can be used -> Hide Lower Button
                if (Common.NoDCSBIOSEnabled() || _multiPanelPZ70.PZ70DialPosition == PZ70DialPosition.IAS || _multiPanelPZ70.PZ70DialPosition == PZ70DialPosition.HDG || _multiPanelPZ70.PZ70DialPosition == PZ70DialPosition.CRS)
                {
                    ButtonLcdLower.Visibility = Visibility.Hidden;
                }
                else if (Common.IsOperationModeFlagSet(OperationFlag.DCSBIOSOutputEnabled))
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
                Common.ShowErrorMessageBox( ex);
            }
        }

        private void TextBoxMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                var textBox = (PZ70TextBox)sender;

                if (e.ChangedButton == MouseButton.Left)
                {

                    //Check if this textbox contains DCS-BIOS information. If so then prompt the user for deletion
                    if (textBox.Bill.ContainsDCSBIOS())
                    {
                        if (MessageBox.Show("Do you want to delete the DCS-BIOS configuration?", "Delete DCS-BIOS control?", MessageBoxButton.OKCancel, MessageBoxImage.Question) != MessageBoxResult.OK)
                        {
                            return;
                        }
                        textBox.Text = "";
                        _multiPanelPZ70.RemoveMultiPanelKnobFromList(ControlListPZ70.DCSBIOS, GetPZ70Knob(textBox).MultiPanelPZ70Knob, GetPZ70Knob(textBox).ButtonState);
                        textBox.Bill.DCSBIOSBinding = null;
                    }
                    else if (textBox.Bill.ContainsKeySequence())
                    {
                        //Check if this textbox contains sequence information. If so then prompt the user for deletion
                        if (MessageBox.Show("Do you want to delete the key sequence?", "Delete key sequence?", MessageBoxButton.OKCancel, MessageBoxImage.Question) != MessageBoxResult.OK)
                        {
                            return;
                        }
                        textBox.Bill.KeyPress.KeySequence.Clear();
                        textBox.Text = "";
                        UpdateKeyBindingProfileSimpleKeyStrokes(textBox);
                    }
                    else if (textBox.Bill.ContainsSingleKey())
                    {
                        textBox.Bill.KeyPress.KeySequence.Clear();
                        textBox.Text = "";
                        UpdateKeyBindingProfileSimpleKeyStrokes(textBox);
                    }
                    if (textBox.Bill.ContainsBIPLink())
                    {
                        //Check if this textbox contains sequence information. If so then prompt the user for deletion
                        if (MessageBox.Show("Do you want to delete BIP Links?", "Delete BIP Link?", MessageBoxButton.OKCancel, MessageBoxImage.Question) != MessageBoxResult.OK)
                        {
                            return;
                        }
                        textBox.Bill.BIPLink.BIPLights.Clear();
                        textBox.Background = Brushes.White;
                        UpdateBIPLinkBindings(textBox);
                    }
                }
                TextBoxLogPZ70.Focus();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox( ex);
            }
        }

        private void TextBoxGotFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                ((PZ70TextBox)sender).Background = Brushes.Yellow;
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox( ex);
            }
        }


        private void TextBoxPreviewKeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                var textBox = ((PZ70TextBox)sender);

                //Check if this textbox contains sequence or DCS-BIOS information. If so then exit
                if (textBox.Bill.ContainsKeySequence() || textBox.Bill.ContainsDCSBIOS())
                {
                    return;
                }
                var hashSetOfKeysPressed = new HashSet<string>();

                var keyCode = KeyInterop.VirtualKeyFromKey(e.RealKey());
                e.Handled = true;

                if (keyCode > 0)
                {
                    hashSetOfKeysPressed.Add(Enum.GetName(typeof(VirtualKeyCode), keyCode));
                }
                var modifiers = CommonVK.GetPressedVirtualKeyCodesThatAreModifiers();
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
                
                result = Common.RemoveRControl(result);

                textBox.Text = result;
                textBox.Bill.KeyPress = new KeyPress(result);
                UpdateKeyBindingProfileSimpleKeyStrokes(textBox);
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox( ex);
            }
        }

        private void TextBoxTextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                //MAKE SURE THE Tag iss SET BEFORE SETTING TEXT! OTHERWISE THIS DOESN'T FIRE
                var textBox = (PZ70TextBox)sender;
                if (textBox.Bill.ContainsKeySequence())
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
                Common.ShowErrorMessageBox( ex);
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
                Common.ShowErrorMessageBox( ex);
            }
        }

        private void TextBoxContextMenuIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            try
            {
                var contextMenu = (ContextMenu)sender;
                var textBox = GetTextBoxInFocus();
                if (textBox == null)
                {
                    foreach (MenuItem contextMenuItem in contextMenu.Items)
                    {
                        contextMenuItem.Visibility = Visibility.Collapsed;
                    }
                    return;
                    //throw new Exception("Failed to locate which textbox is focused.");
                }

                //Check new value, is menu visible?
                if (!(bool)e.NewValue)
                {
                    //Do not show if not visible
                    return;
                }
                
                if (!textBox.Bill.ContainsSingleKey())
                {
                    return;
                }
                var keyPressLength = textBox.Bill.KeyPress.GetLengthOfKeyPress();
                CheckContextMenuItems(keyPressLength, contextMenu);
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox( ex);
            }
        }


        private void TextBoxContextMenuClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var textBox = GetTextBoxInFocus();
                SetKeyPressLength(textBox, (MenuItem)sender);
                
                UpdateKeyBindingProfileSimpleKeyStrokes(textBox);
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox( ex);
            }
        }

        private PZ70TextBox GetTextBoxInFocus()
        {
            foreach (var textBox in Common.FindVisualChildren<PZ70TextBox>(this))
            {
                if (!textBox.Equals(TextBoxLogPZ70) && textBox.IsFocused && Equals(textBox.Background, Brushes.Yellow))
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
                KeySequenceWindow keySequenceWindow;
                if (textBox.Bill.ContainsKeySequence())
                {
                    keySequenceWindow = new KeySequenceWindow(textBox.Text, textBox.Bill.GetKeySequence());
                }
                else
                {
                    keySequenceWindow = new KeySequenceWindow();
                }
                keySequenceWindow.ShowDialog();
                if (keySequenceWindow.DialogResult.HasValue && keySequenceWindow.DialogResult.Value)
                {
                    //Clicked OK
                    //If the user added only a single key stroke combo then let's not treat this as a sequence
                    if (!keySequenceWindow.IsDirty)
                    {
                        //User made no changes
                        return;
                    }
                    var sequenceList = keySequenceWindow.GetSequence;
                    if (sequenceList.Count > 1)
                    {
                        var keyPress = new KeyPress("Key press sequence", sequenceList);
                        textBox.Bill.KeyPress = keyPress;
                        textBox.Bill.KeyPress.Information = keySequenceWindow.GetInformation;
                        if (!string.IsNullOrEmpty(keySequenceWindow.GetInformation))
                        {
                            textBox.Text = keySequenceWindow.GetInformation;
                        }
                        //textBox.Text = string.IsNullOrEmpty(keySequenceWindow.GetInformation) ? "Key press sequence" : keySequenceWindow.GetInformation;
                        /*if (!string.IsNullOrEmpty(keySequenceWindow.GetInformation))
                        {
                            var toolTip = new ToolTip { Content = keySequenceWindow.GetInformation };
                            textBox.ToolTipa = toolTip;
                        }*/
                        UpdateKeyBindingProfileSequencedKeyStrokesPZ70(textBox);
                    }
                    else
                    {
                        //If only one press was created treat it as a simple keypress
                        textBox.Bill.Clear();
                        var keyPress = new KeyPress(sequenceList[0].VirtualKeyCodesAsString, sequenceList[0].LengthOfKeyPress);
                        textBox.Bill.KeyPress = keyPress;
                        textBox.Bill.KeyPress.Information = keySequenceWindow.GetInformation;
                        textBox.Text = sequenceList[0].VirtualKeyCodesAsString;
                        /*textBox.Text = sequenceList.Values[0].VirtualKeyCodesAsString;
                        if (!string.IsNullOrEmpty(keySequenceWindow.GetInformation))
                        {
                            var toolTip = new ToolTip { Content = keySequenceWindow.GetInformation };
                            textBox.ToolTipa = toolTip;
                        }*/
                        UpdateKeyBindingProfileSimpleKeyStrokes(textBox);
                    }
                }
                TextBoxLogPZ70.Focus();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox( ex);
            }
        }
        private void ContextMenuItemEditDCSBIOS_OnContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            try
            {
                var contextMenu = (ContextMenu)sender;
                foreach (MenuItem item in contextMenu.Items)
                {
                    item.IsEnabled = !Common.KeyEmulationOnly();
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox( ex);
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
                DCSBIOSOutputControlsWindow dcsBIOSInputControlsWindow;
                if (textBox.Bill.ContainsDCSBIOS())
                {
                    dcsBIOSInputControlsWindow = new DCSBIOSOutputControlsWindow(GlobalHandler.GetAirframe(), textBox.Name.Replace("TextBox", ""), textBox.Bill.DCSBIOSBinding.DCSBIOSInputs, textBox.Text);
                }
                else
                {
                    dcsBIOSInputControlsWindow = new DCSBIOSOutputControlsWindow(GlobalHandler.GetAirframe(), textBox.Name.Replace("TextBox", ""), null);
                }
                dcsBIOSInputControlsWindow.ShowDialog();
                if (dcsBIOSInputControlsWindow.DialogResult.HasValue && dcsBIOSInputControlsWindow.DialogResult == true)
                {
                    var dcsBiosInputs = dcsBIOSInputControlsWindow.DCSBIOSInputs;
                    var text = string.IsNullOrWhiteSpace(dcsBIOSInputControlsWindow.Description) ? "DCS-BIOS" : dcsBIOSInputControlsWindow.Description;
                    //1 appropriate text to textbox
                    //2 update bindings
                    textBox.Text = text;
                    textBox.Bill.Consume(dcsBiosInputs);
                    UpdateDCSBIOSBinding(textBox);
                }
                TextBoxLogPZ70.Focus();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox( ex);
            }
        }

        private void MenuContextEditBipTextBoxClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var textBox = GetTextBoxInFocus();
                if (textBox == null)
                {
                    throw new Exception("Failed to locate which textbox is focused.");
                }
                BIPLinkWindow bipLinkWindow;
                if (textBox.Bill.ContainsBIPLink())
                {
                    var bipLink = textBox.Bill.BIPLink;
                    bipLinkWindow = new BIPLinkWindow(bipLink);
                }
                else
                {
                    var bipLink = new BIPLinkPZ70();
                    bipLinkWindow = new BIPLinkWindow(bipLink);
                }
                bipLinkWindow.ShowDialog();
                if (bipLinkWindow.DialogResult.HasValue && bipLinkWindow.DialogResult == true && bipLinkWindow.IsDirty && bipLinkWindow.BIPLink != null)
                {
                    textBox.Bill.BIPLink = (BIPLinkPZ70)bipLinkWindow.BIPLink;
                    UpdateBIPLinkBindings(textBox);
                }
                TextBoxLogPZ70.Focus();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox( ex);
            }
        }


        private void UpdateBIPLinkBindings(PZ70TextBox textBox)
        {
            try
            {
                var key = GetPZ70Knob(textBox);
                _multiPanelPZ70.AddOrUpdateBIPLinkKnobBinding(key.MultiPanelPZ70Knob, textBox.Bill.BIPLink, key.ButtonState);
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox( ex);
            }
        }

        private void UpdateKeyBindingProfileSequencedKeyStrokesPZ70(PZ70TextBox textBox)
        {
            try
            {
                var key = GetPZ70Knob(textBox);
                _multiPanelPZ70.AddOrUpdateSequencedKeyBinding(textBox.Text, key.MultiPanelPZ70Knob, textBox.Bill.GetKeySequence(), key.ButtonState);
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox( ex);
            }
        }


        private void UpdateKeyBindingProfileSimpleKeyStrokes(PZ70TextBox textBox)
        {
            try
            {
                KeyPressLength keyPressLength;
                if (!textBox.Bill.ContainsKeyPress() || textBox.Bill.KeyPress.KeySequence.Count == 0)
                {
                    keyPressLength = KeyPressLength.ThirtyTwoMilliSec;
                }
                else
                {
                    keyPressLength = textBox.Bill.KeyPress.GetLengthOfKeyPress();
                }
                var key = GetPZ70Knob(textBox);
                _multiPanelPZ70.AddOrUpdateSingleKeyBinding(key.MultiPanelPZ70Knob, textBox.Text, keyPressLength, key.ButtonState);
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox( ex);
            }
        }

        private void UpdateOSCommandBindingsPZ70(PZ70TextBox textBox)
        {
            try
            {
                var key = GetPZ70Knob(textBox);
                _multiPanelPZ70.AddOrUpdateOSCommandBinding(key.MultiPanelPZ70Knob, textBox.Bill.OSCommandObject, key.ButtonState);
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox( ex);
            }
        }

        private void UpdateDCSBIOSBinding(PZ70TextBox textBox)
        {
            try
            {
                var key = GetPZ70Knob(textBox);
                _multiPanelPZ70.AddOrUpdateDCSBIOSBinding(key.MultiPanelPZ70Knob, textBox.Bill.DCSBIOSBinding.DCSBIOSInputs, textBox.Text, key.ButtonState);
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox( ex);
            }
        }

        private void UpdateDCSBIOSBindingLCD(bool useFormula, bool deleteConfig, DCSBIOSOutput dcsbiosOutput, DCSBIOSOutputFormula dcsbiosOutputFormula, Button button)
        {
            try
            {
                if (deleteConfig)
                {
                    if (button.Equals(ButtonLcdUpper))
                    {
                        ImageLcdUpperRow.Visibility = Visibility.Hidden;
                        _multiPanelPZ70.AddOrUpdateDCSBIOSLcdBinding(PZ70LCDPosition.UpperLCD);
                    }
                    if (button.Equals(ButtonLcdLower))
                    {
                        ImageLcdLowerRow.Visibility = Visibility.Hidden;
                        _multiPanelPZ70.AddOrUpdateDCSBIOSLcdBinding(PZ70LCDPosition.LowerLCD);
                    }
                }

                if (!useFormula)
                {
                    if (button.Equals(ButtonLcdUpper))
                    {
                        ImageLcdUpperRow.Visibility = dcsbiosOutput == null ? Visibility.Collapsed : Visibility.Visible;
                        _multiPanelPZ70.AddOrUpdateLCDBinding(dcsbiosOutput, PZ70LCDPosition.UpperLCD);
                    }
                    if (button.Equals(ButtonLcdLower))
                    {
                        ImageLcdLowerRow.Visibility = dcsbiosOutput == null ? Visibility.Collapsed : Visibility.Visible;
                        _multiPanelPZ70.AddOrUpdateLCDBinding(dcsbiosOutput, PZ70LCDPosition.LowerLCD);
                    }
                }
                if (useFormula)
                {
                    if (button.Equals(ButtonLcdUpper))
                    {
                        ImageLcdUpperRow.Visibility = dcsbiosOutputFormula == null ? Visibility.Collapsed : Visibility.Visible;
                        _multiPanelPZ70.AddOrUpdateLCDBinding(dcsbiosOutputFormula, PZ70LCDPosition.UpperLCD);
                    }
                    if (button.Equals(ButtonLcdLower))
                    {
                        ImageLcdLowerRow.Visibility = dcsbiosOutputFormula == null ? Visibility.Collapsed : Visibility.Visible;
                        _multiPanelPZ70.AddOrUpdateLCDBinding(dcsbiosOutputFormula, PZ70LCDPosition.LowerLCD);
                    }
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox( ex);
            }
        }

        private void RemoveContextMenuClickHandlers()
        {
            foreach (var textBox in Common.FindVisualChildren<PZ70TextBox>(this))
            {
                if (!Equals(textBox, TextBoxLogPZ70))
                {
                    textBox.ContextMenu = null;
                    textBox.ContextMenuOpening -= TextBoxContextMenuOpening;
                }
            }
        }

        private void SetContextMenuClickHandlers()
        {
            foreach (var textBox in Common.FindVisualChildren<PZ70TextBox>(this))
            {
                if (!Equals(textBox, TextBoxLogPZ70))
                {
                    var contextMenu = (ContextMenu)Resources["TextBoxContextMenuPZ70"];

                    textBox.ContextMenu = contextMenu;
                    textBox.ContextMenuOpening += TextBoxContextMenuOpening;
                }
            }
            if (Common.IsOperationModeFlagSet(OperationFlag.DCSBIOSOutputEnabled))
            {
                ButtonLcdUpper.Visibility = Visibility.Hidden;
                ButtonLcdLower.Visibility = Visibility.Hidden;
            }
            else
            {
                ButtonLcdUpper.ContextMenu = (ContextMenu)Resources["ButtonLcdContextMenu"];
                ButtonLcdUpper.ContextMenu.Tag = ButtonLcdUpper;

                ButtonLcdLower.ContextMenu = (ContextMenu)Resources["ButtonLcdContextMenu"];
                ButtonLcdLower.ContextMenu.Tag = ButtonLcdLower;

            }
        }

        private void TextBoxContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            try
            {
                var textBox = (PZ70TextBox)sender;
                var contextMenu = textBox.ContextMenu;
                if (!(textBox.IsFocused && Equals(textBox.Background, Brushes.Yellow)))
                {
                    //UGLY Must use this to get around problems having different color for BIPLink and Right Clicks
                    foreach (MenuItem item in contextMenu.Items)
                    {
                        item.Visibility = Visibility.Collapsed;
                    }
                    return;
                }

                foreach (MenuItem item in contextMenu.Items)
                {
                    item.Visibility = Visibility.Collapsed;
                }

                if (textBox.Bill.ContainsDCSBIOS())
                {
                    // 1) If Contains DCSBIOS, show Edit DCS-BIOS Control & BIP
                    foreach (MenuItem item in contextMenu.Items)
                    {
                        if (!Common.KeyEmulationOnly() && item.Name.Contains("EditDCSBIOS"))
                        {
                            item.Visibility = Visibility.Visible;
                        }
                        if (BipFactory.HasBips() && item.Name.Contains("EditBIP"))
                        {
                            item.Visibility = Visibility.Visible;
                        }
                    }
                }
                else if (textBox.Bill.ContainsKeySequence())
                {
                    // 2) 
                    foreach (MenuItem item in contextMenu.Items)
                    {
                        if (item.Name.Contains("EditSequence"))
                        {
                            item.Visibility = Visibility.Visible;
                        }
                        if (BipFactory.HasBips() && item.Name.Contains("EditBIP"))
                        {
                            item.Visibility = Visibility.Visible;
                        }
                    }
                }
                else if (textBox.Bill.IsEmpty())
                {
                    // 4) 
                    foreach (MenuItem item in contextMenu.Items)
                    {
                        if (item.Name.Contains("EditSequence"))
                        {
                            item.Visibility = Visibility.Visible;
                        }
                        else if (!Common.KeyEmulationOnly() && item.Name.Contains("EditDCSBIOS"))
                        {
                            item.Visibility = Visibility.Visible;
                        }
                        else if (BipFactory.HasBips() && item.Name.Contains("EditBIP"))
                        {
                            item.Visibility = Visibility.Visible;
                        }
                        else if (item.Name.Contains("EditOSCommand"))
                        {
                            item.Visibility = Visibility.Visible;
                        }
                        else if (item.Name.Contains("AddNullKey"))
                        {
                            item.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            item.Visibility = Visibility.Collapsed;
                        }
                    }
                }
                else if (textBox.Bill.ContainsSingleKey())
                {
                    // 5) 
                    foreach (MenuItem item in contextMenu.Items)
                    {
                        if (!(item.Name.Contains("EditSequence") || item.Name.Contains("EditDCSBIOS")))
                        {
                            if (item.Name.Contains("EditBIP"))
                            {
                                if (BipFactory.HasBips())
                                {
                                    item.Visibility = Visibility.Visible;
                                }
                            }
                            else
                            {
                                item.Visibility = Visibility.Visible;
                            }
                        }
                    }
                }
                else if (textBox.Bill.ContainsBIPLink())
                {
                    // 3) 
                    foreach (MenuItem item in contextMenu.Items)
                    {
                        if (!Common.KeyEmulationOnly() && item.Name.Contains("EditDCSBIOS"))
                        {
                            item.Visibility = Visibility.Visible;
                        }
                        if (BipFactory.HasBips() && item.Name.Contains("EditBIP"))
                        {
                            item.Visibility = Visibility.Visible;
                        }
                        if (item.Name.Contains("EditSequence"))
                        {
                            item.Visibility = Visibility.Visible;
                        }
                    }
                }
                else if (textBox.Bill.ContainsOSCommand())
                {
                    foreach (MenuItem item in contextMenu.Items)
                    {
                        if (item.Name.Contains("EditOSCommand"))
                        {
                            item.Visibility = Visibility.Visible;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox( ex);
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
                Common.ShowErrorMessageBox( ex);
            }
        }

        private void ButtonLcdMenuItemDeleteBinding_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var menuItem = (MenuItem)sender;
                var button = (Button)((ContextMenu)(menuItem.Parent)).Tag;
                if (button.Name.Contains("Upper"))
                {
                    DeleteLCDBinding(PZ70LCDPosition.UpperLCD, button);
                }
                else
                {
                    DeleteLCDBinding(PZ70LCDPosition.LowerLCD, button);
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox( ex);
            }
        }

        private void ButtonLcdContextMenu_OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            try
            {
                var contextMenu = (ContextMenu)sender;
                if (Common.IsOperationModeFlagSet(OperationFlag.DCSBIOSOutputEnabled))
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
                Common.ShowErrorMessageBox( ex);
            }
        }

        private void DeleteLCDBinding(PZ70LCDPosition pz70LCDPosition, Button button)
        {
            try
            {
                //Check if this button contains DCS-BIOS information. If so then prompt the user for deletion
                if (MessageBox.Show("Do you want to delete the specified DCS-BIOS control binding?", "Delete DCS-BIOS control binding?", MessageBoxButton.OKCancel, MessageBoxImage.Question) != MessageBoxResult.OK)
                {
                    return;
                }
                UpdateDCSBIOSBindingLCD(false, true, null, null, button);
                switch (button.Name)
                {
                    case "ButtonLcdUpper":
                        {
                            ImageLcdUpperRow.Visibility = Visibility.Collapsed;
                            break;
                        }
                    case "ButtonLcdLower":
                        {
                            ImageLcdLowerRow.Visibility = Visibility.Collapsed;
                            break;
                        }
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox( ex);
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
                Common.ShowErrorMessageBox( ex);
            }
        }




        private void TextBox_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                ((PZ70TextBox)sender).Background = Brushes.Yellow;
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox( ex);
            }
        }


        private MultiPanelPZ70KnobOnOff GetPZ70Knob(PZ70TextBox textBox)
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
                    return new MultiPanelPZ70KnobOnOff(MultiPanelPZ70Knobs.FLAPS_LEVER_DOWN, true);
                }
                if (textBox.Equals(TextBoxPitchTrimUp))
                {
                    return new MultiPanelPZ70KnobOnOff(MultiPanelPZ70Knobs.PITCH_TRIM_WHEEL_UP, true);
                }
                if (textBox.Equals(TextBoxPitchTrimDown))
                {
                    return new MultiPanelPZ70KnobOnOff(MultiPanelPZ70Knobs.PITCH_TRIM_WHEEL_DOWN, true);
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
                Common.ShowErrorMessageBox( ex);
            }
            throw new Exception("Failed to find MultiPanel knob for TextBox " + textBox.Name);
        }


        private PZ70TextBox GetTextBox(MultiPanelPZ70Knobs knob, bool whenTurnedOn)
        {
            try
            {
                if (knob == MultiPanelPZ70Knobs.LCD_WHEEL_DEC && whenTurnedOn)
                {
                    return TextBoxLcdKnobDecrease;
                }
                if (knob == MultiPanelPZ70Knobs.LCD_WHEEL_INC && whenTurnedOn)
                {
                    return TextBoxLcdKnobIncrease;
                }
                if (knob == MultiPanelPZ70Knobs.AUTO_THROTTLE && !whenTurnedOn)
                {
                    return TextBoxAutoThrottleOff;
                }
                if (knob == MultiPanelPZ70Knobs.AUTO_THROTTLE && whenTurnedOn)
                {
                    return TextBoxAutoThrottleOn;
                }
                if (knob == MultiPanelPZ70Knobs.FLAPS_LEVER_UP && whenTurnedOn)
                {
                    return TextBoxFlapsUp;
                }
                if (knob == MultiPanelPZ70Knobs.FLAPS_LEVER_DOWN && whenTurnedOn)
                {
                    return TextBoxFlapsDown;
                }
                if (knob == MultiPanelPZ70Knobs.PITCH_TRIM_WHEEL_UP && whenTurnedOn)
                {
                    return TextBoxPitchTrimUp;
                }
                if (knob == MultiPanelPZ70Knobs.PITCH_TRIM_WHEEL_DOWN && whenTurnedOn)
                {
                    return TextBoxPitchTrimDown;
                }
                if (knob == MultiPanelPZ70Knobs.AP_BUTTON && whenTurnedOn)
                {
                    return TextBoxApButtonOn;
                }
                if (knob == MultiPanelPZ70Knobs.AP_BUTTON && !whenTurnedOn)
                {
                    return TextBoxApButtonOff;
                }
                if (knob == MultiPanelPZ70Knobs.HDG_BUTTON && whenTurnedOn)
                {
                    return TextBoxHdgButtonOn;
                }
                if (knob == MultiPanelPZ70Knobs.HDG_BUTTON && !whenTurnedOn)
                {
                    return TextBoxHdgButtonOff;
                }
                if (knob == MultiPanelPZ70Knobs.NAV_BUTTON && whenTurnedOn)
                {
                    return TextBoxNavButtonOn;
                }
                if (knob == MultiPanelPZ70Knobs.NAV_BUTTON && !whenTurnedOn)
                {
                    return TextBoxNavButtonOff;
                }
                if (knob == MultiPanelPZ70Knobs.IAS_BUTTON && whenTurnedOn)
                {
                    return TextBoxIasButtonOn;
                }
                if (knob == MultiPanelPZ70Knobs.IAS_BUTTON && !whenTurnedOn)
                {
                    return TextBoxIasButtonOff;
                }
                if (knob == MultiPanelPZ70Knobs.ALT_BUTTON && whenTurnedOn)
                {
                    return TextBoxAltButtonOn;
                }
                if (knob == MultiPanelPZ70Knobs.ALT_BUTTON && !whenTurnedOn)
                {
                    return TextBoxAltButtonOff;
                }
                if (knob == MultiPanelPZ70Knobs.VS_BUTTON && whenTurnedOn)
                {
                    return TextBoxVsButtonOn;
                }
                if (knob == MultiPanelPZ70Knobs.VS_BUTTON && !whenTurnedOn)
                {
                    return TextBoxVsButtonOff;
                }
                if (knob == MultiPanelPZ70Knobs.APR_BUTTON && whenTurnedOn)
                {
                    return TextBoxAprButtonOn;
                }
                if (knob == MultiPanelPZ70Knobs.APR_BUTTON && !whenTurnedOn)
                {
                    return TextBoxAprButtonOff;
                }
                if (knob == MultiPanelPZ70Knobs.REV_BUTTON && whenTurnedOn)
                {
                    return TextBoxRevButtonOn;
                }
                if (knob == MultiPanelPZ70Knobs.REV_BUTTON && !whenTurnedOn)
                {
                    return TextBoxRevButtonOff;
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox( ex);
            }
            throw new Exception("Failed to find TextBox from MultiPanel Knob : " + knob);
        }
        
        private void MenuItemAddNullKey_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var textBox = GetTextBoxInFocus();
                if (textBox == null)
                {
                    throw new Exception("Failed to locate which textbox is focused.");
                }

                textBox.Bill.Clear();
                var vkNull = Enum.GetName(typeof(VirtualKeyCode), VirtualKeyCode.VK_NULL);
                if (string.IsNullOrEmpty(vkNull))
                {
                    return;
                }
                var keyPress = new KeyPress(vkNull, KeyPressLength.ThirtyTwoMilliSec);
                textBox.Bill.KeyPress = keyPress;
                textBox.Bill.KeyPress.Information = "VK_NULL";
                textBox.Text = vkNull;
                UpdateKeyBindingProfileSimpleKeyStrokes(textBox);
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox( ex);
            }
        }

        private void MenuContextEditOSCommandTextBoxClick_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var textBox = GetTextBoxInFocus();
                if (textBox == null)
                {
                    throw new Exception("Failed to locate which textbox is focused.");
                }
                OSCommandWindow osCommandWindow;
                if (textBox.Bill.ContainsOSCommand())
                {
                    osCommandWindow = new OSCommandWindow(textBox.Bill.OSCommandObject);
                }
                else
                {
                    osCommandWindow = new OSCommandWindow();
                }
                osCommandWindow.ShowDialog();
                if (osCommandWindow.DialogResult.HasValue && osCommandWindow.DialogResult.Value)
                {
                    //Clicked OK
                    if (!osCommandWindow.IsDirty)
                    {
                        //User made no changes
                        return;
                    }
                    var osCommand = osCommandWindow.OSCommandObject;
                    textBox.Bill.OSCommandObject = osCommand;
                    textBox.Text = osCommand.Name;
                    UpdateOSCommandBindingsPZ70(textBox);
                }
                TextBoxLogPZ70.Focus();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox( ex);
            }
        }


        private void CheckContextMenuItems(KeyPressLength keyPressLength, ContextMenu contextMenu)
        {
            try
            {
                foreach (MenuItem item in contextMenu.Items)
                {
                    item.IsChecked = false;
                }

                foreach (MenuItem item in contextMenu.Items)
                {
                    if (item.Name == "contextMenuItemKeepPressed" && keyPressLength == KeyPressLength.Indefinite)
                    {
                        item.IsChecked = true;
                    }
                    else if (item.Name == "contextMenuItemThirtyTwoMilliSec" && keyPressLength == KeyPressLength.ThirtyTwoMilliSec)
                    {
                        item.IsChecked = true;
                    }
                    else if (item.Name == "contextMenuItemFiftyMilliSec" && keyPressLength == KeyPressLength.FiftyMilliSec)
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
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void SetKeyPressLength(PZ70TextBox textBox, MenuItem contextMenuItem)
        {
            try
            {
                if (contextMenuItem.Name == "contextMenuItemKeepPressed")
                {
                    var message = "Remember to set a command for the opposing action!\n\n" +
                                  "For example if you set Keep Pressed for the \"On\" position for a button you need to set a command for \"Off\" position.\n" +
                                  "This way the continuous Keep Pressed will be canceled.\n" +
                                  "If you do not want a key press to cancel the continuous key press you can add a \"VK_NULL\" key.\n" +
                                  "\"VK_NULL\'s\" sole purpose is to cancel a continuous key press.";
                    var infoDialog = new InformationTextBlockWindow(message);
                    infoDialog.Height = 250;
                    infoDialog.ShowDialog();
                    textBox.Bill.KeyPress.SetLengthOfKeyPress(KeyPressLength.Indefinite);
                }
                else if (contextMenuItem.Name == "contextMenuItemThirtyTwoMilliSec")
                {
                    textBox.Bill.KeyPress.SetLengthOfKeyPress(KeyPressLength.ThirtyTwoMilliSec);
                }
                else if (contextMenuItem.Name == "contextMenuItemFiftyMilliSec")
                {
                    textBox.Bill.KeyPress.SetLengthOfKeyPress(KeyPressLength.FiftyMilliSec);
                }
                else if (contextMenuItem.Name == "contextMenuItemHalfSecond")
                {
                    textBox.Bill.KeyPress.SetLengthOfKeyPress(KeyPressLength.HalfSecond);
                }
                else if (contextMenuItem.Name == "contextMenuItemSecond")
                {
                    textBox.Bill.KeyPress.SetLengthOfKeyPress(KeyPressLength.Second);
                }
                else if (contextMenuItem.Name == "contextMenuItemSecondAndHalf")
                {
                    textBox.Bill.KeyPress.SetLengthOfKeyPress(KeyPressLength.SecondAndHalf);
                }
                else if (contextMenuItem.Name == "contextMenuItemTwoSeconds")
                {
                    textBox.Bill.KeyPress.SetLengthOfKeyPress(KeyPressLength.TwoSeconds);
                }
                else if (contextMenuItem.Name == "contextMenuItemThreeSeconds")
                {
                    textBox.Bill.KeyPress.SetLengthOfKeyPress(KeyPressLength.ThreeSeconds);
                }
                else if (contextMenuItem.Name == "contextMenuItemFourSeconds")
                {
                    textBox.Bill.KeyPress.SetLengthOfKeyPress(KeyPressLength.FourSeconds);
                }
                else if (contextMenuItem.Name == "contextMenuItemFiveSecs")
                {
                    textBox.Bill.KeyPress.SetLengthOfKeyPress(KeyPressLength.FiveSecs);
                }
                else if (contextMenuItem.Name == "contextMenuItemTenSecs")
                {
                    textBox.Bill.KeyPress.SetLengthOfKeyPress(KeyPressLength.TenSecs);
                }
                else if (contextMenuItem.Name == "contextMenuItemFifteenSecs")
                {
                    textBox.Bill.KeyPress.SetLengthOfKeyPress(KeyPressLength.FifteenSecs);
                }
                else if (contextMenuItem.Name == "contextMenuItemTwentySecs")
                {
                    textBox.Bill.KeyPress.SetLengthOfKeyPress(KeyPressLength.TwentySecs);
                }
                else if (contextMenuItem.Name == "contextMenuItemThirtySecs")
                {
                    textBox.Bill.KeyPress.SetLengthOfKeyPress(KeyPressLength.ThirtySecs);
                }
                else if (contextMenuItem.Name == "contextMenuItemFortySecs")
                {
                    textBox.Bill.KeyPress.SetLengthOfKeyPress(KeyPressLength.FortySecs);
                }
                else if (contextMenuItem.Name == "contextMenuItemSixtySecs")
                {
                    textBox.Bill.KeyPress.SetLengthOfKeyPress(KeyPressLength.SixtySecs);
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }
    }
}
