﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ClassLibraryCommon;
using DCS_BIOS;
using NonVisuals;
using DCSFlightpanels.Properties;

namespace DCSFlightpanels
{
    /// <summary>
    /// Interaction logic for MultiPanelUserControl.xaml
    /// </summary>

    public partial class MultiPanelUserControl : ISaitekPanelListener, IProfileHandlerListener, ISaitekUserControl
    {
        private readonly MultiPanelPZ70 _multiPanelPZ70;
        private readonly TabItem _parentTabItem;
        private string _parentTabItemHeader;
        private readonly IGlobalHandler _globalHandler;
        private bool _userControlLoaded;
        private bool _textBoxTagsSet;

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
        
        private void MultiPanelUserControl_OnLoaded(object sender, RoutedEventArgs e)
        {
            ComboBoxLcdKnobSensitivity.SelectedValue = Settings.Default.PZ70LcdKnobSensitivity;
            SetTextBoxTagObjects();
            SetContextMenuClickHandlers();
            _userControlLoaded = true;
            ShowGraphicConfiguration();
        }

        public void BipPanelRegisterEvent(object sender, BipPanelRegisteredEventArgs e)
        {
            var now = DateTime.Now.Ticks;
            Debug.WriteLine("Start BipPanelRegisterEvent");
            RemoveContextMenuClickHandlers();
            SetContextMenuClickHandlers();
            Debug.WriteLine("End BipPanelRegisterEvent" + new TimeSpan(DateTime.Now.Ticks - now).Milliseconds);
        }

        public SaitekPanel GetSaitekPanel()
        {
            return _multiPanelPZ70;
        }

        public void UpdatesHasBeenMissed(object sender, DCSBIOSUpdatesMissedEventArgs e)
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

        public void SelectedAirframe(object sender, AirframeEventArgs e)
        {
            try
            {
                SetApplicationMode();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(471573, ex);
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

        public void SwitchesChanged(object sender, SwitchesChangedEventArgs e)
        {
            try
            {
                if (e.SaitekPanelEnum == SaitekPanelsEnum.PZ70MultiPanel && e.UniqueId.Equals(_multiPanelPZ70.InstanceId))
                {
                    NotifyKnobChanges(e.Switches);
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(1018, ex);
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
                Common.ShowErrorMessageBox(1019, ex);
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
                Common.ShowErrorMessageBox(1020, ex);
            }
        }

        private void ClearAll(bool clearAlsoProfile = true)
        {
            foreach (var textBox in Common.FindVisualChildren<TextBox>(this))
            {
                var tagHolderClass = (TagDataClassPZ70)textBox.Tag;
                textBox.Text = "";
                tagHolderClass.ClearAll();
            }
            if (clearAlsoProfile)
            {
                _multiPanelPZ70.ClearSettings();
            }
        }

        private void SetTextBoxTagObjects()
        {
            if (_textBoxTagsSet || !Common.FindVisualChildren<TextBox>(this).Any())
            {
                return;
            }
            foreach (var textBox in Common.FindVisualChildren<TextBox>(this))
            {
                //Debug.WriteLine("Adding TextBoxTagHolderClass for TextBox " + textBox.Name);
                textBox.Tag = new TagDataClassPZ70();
            }
            _textBoxTagsSet = true;
        }

        public void LedLightChanged(object sender, LedLightChangeEventArgs e)
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

        public void PanelSettingsChanged(object sender, PanelEventArgs e)
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

        public void PanelDataAvailable(object sender, PanelDataToDCSBIOSEventEventArgs e)
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

        public void SettingsApplied(object sender, PanelEventArgs e)
        {
            try
            {
                if (e.UniqueId.Equals(_multiPanelPZ70.InstanceId) && e.SaitekPanelEnum == SaitekPanelsEnum.PZ70MultiPanel)
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

        public void DeviceAttached(object sender, PanelEventArgs e)
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

        public void DeviceDetached(object sender, PanelEventArgs e)
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
                                Dispatcher.BeginInvoke(
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
                                Dispatcher.BeginInvoke(
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
                                Dispatcher.BeginInvoke(
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
                                Dispatcher.BeginInvoke(
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
                                Dispatcher.BeginInvoke(
                                    (Action)delegate
                                    {
                                        ImageLcdButtonAp.Visibility = _multiPanelPZ70.LCDButtonByteListHandler.IsOn(_multiPanelPZ70.PZ70_DialPosition, multiKnob.MultiPanelPZ70Knob) ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case MultiPanelPZ70Knobs.HDG_BUTTON:
                            {
                                Dispatcher.BeginInvoke(
                                    (Action)delegate
                                    {
                                        ImageLcdButtonHdg.Visibility = _multiPanelPZ70.LCDButtonByteListHandler.IsOn(_multiPanelPZ70.PZ70_DialPosition, multiKnob.MultiPanelPZ70Knob) ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case MultiPanelPZ70Knobs.NAV_BUTTON:
                            {
                                Dispatcher.BeginInvoke(
                                    (Action)delegate
                                    {
                                        ImageLcdButtonNav.Visibility = _multiPanelPZ70.LCDButtonByteListHandler.IsOn(_multiPanelPZ70.PZ70_DialPosition, multiKnob.MultiPanelPZ70Knob) ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case MultiPanelPZ70Knobs.IAS_BUTTON:
                            {
                                Dispatcher.BeginInvoke(
                                    (Action)delegate
                                    {
                                        ImageLcdButtonIas.Visibility = _multiPanelPZ70.LCDButtonByteListHandler.IsOn(_multiPanelPZ70.PZ70_DialPosition, multiKnob.MultiPanelPZ70Knob) ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case MultiPanelPZ70Knobs.ALT_BUTTON:
                            {
                                Dispatcher.BeginInvoke(
                                    (Action)delegate
                                    {
                                        ImageLcdButtonAlt.Visibility = _multiPanelPZ70.LCDButtonByteListHandler.IsOn(_multiPanelPZ70.PZ70_DialPosition, multiKnob.MultiPanelPZ70Knob) ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case MultiPanelPZ70Knobs.VS_BUTTON:
                            {
                                Dispatcher.BeginInvoke(
                                    (Action)delegate
                                    {
                                        ImageLcdButtonVs.Visibility = _multiPanelPZ70.LCDButtonByteListHandler.IsOn(_multiPanelPZ70.PZ70_DialPosition, multiKnob.MultiPanelPZ70Knob) ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case MultiPanelPZ70Knobs.APR_BUTTON:
                            {
                                Dispatcher.BeginInvoke(
                                    (Action)delegate
                                    {
                                        ImageLcdButtonApr.Visibility = _multiPanelPZ70.LCDButtonByteListHandler.IsOn(_multiPanelPZ70.PZ70_DialPosition, multiKnob.MultiPanelPZ70Knob) ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case MultiPanelPZ70Knobs.REV_BUTTON:
                            {
                                Dispatcher.BeginInvoke(
                                    (Action)delegate
                                    {
                                        ImageLcdButtonRev.Visibility = _multiPanelPZ70.LCDButtonByteListHandler.IsOn(_multiPanelPZ70.PZ70_DialPosition, multiKnob.MultiPanelPZ70Knob) ? Visibility.Visible : Visibility.Collapsed;
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
                Common.ShowErrorMessageBox(9965005, ex);
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
                    if (dcsbiosBindingLCDPZ70.DialPosition == _multiPanelPZ70.PZ70_DialPosition && dcsbiosBindingLCDPZ70.PZ70LCDPosition == pz70LCDPosition)
                    {
                        if (dcsbiosBindingLCDPZ70.UseFormula)
                        {
                            dcsBiosOutputFormulaWindow = new DCSBiosOutputFormulaWindow(_globalHandler.GetAirframe(), description, dcsbiosBindingLCDPZ70.DCSBIOSOutputFormulaObject);
                            break;
                        }
                        dcsBiosOutputFormulaWindow = new DCSBiosOutputFormulaWindow(_globalHandler.GetAirframe(), description, dcsbiosBindingLCDPZ70.DCSBIOSOutputObject);
                        break;
                    }
                }
                if (dcsBiosOutputFormulaWindow == null)
                {
                    dcsBiosOutputFormulaWindow = new DCSBiosOutputFormulaWindow(_globalHandler.GetAirframe(), description);
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
                Common.ShowErrorMessageBox(49942044, ex);
            }
        }

        private void TextBoxLostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                var textBox = (TextBox)sender;
                if (((TagDataClassPZ70)textBox.Tag).ContainsBIPLink())
                {
                    ((TextBox)sender).Background = Brushes.Bisque;
                }
                else
                {
                    ((TextBox)sender).Background = Brushes.White;
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(3005, ex);
            }
        }

        private void ShowGraphicConfiguration()
        {
            try
            {
                if (!_userControlLoaded || !_textBoxTagsSet)
                {
                    return;
                }

                SetApplicationMode();
                ImageLcdButtonAp.Visibility = _multiPanelPZ70.LCDButtonByteListHandler.IsOn(_multiPanelPZ70.PZ70_DialPosition, MultiPanelPZ70Knobs.AP_BUTTON) ? Visibility.Visible : Visibility.Collapsed;
                ImageLcdButtonHdg.Visibility = _multiPanelPZ70.LCDButtonByteListHandler.IsOn(_multiPanelPZ70.PZ70_DialPosition, MultiPanelPZ70Knobs.HDG_BUTTON) ? Visibility.Visible : Visibility.Collapsed;
                ImageLcdButtonNav.Visibility = _multiPanelPZ70.LCDButtonByteListHandler.IsOn(_multiPanelPZ70.PZ70_DialPosition, MultiPanelPZ70Knobs.NAV_BUTTON) ? Visibility.Visible : Visibility.Collapsed;
                ImageLcdButtonIas.Visibility = _multiPanelPZ70.LCDButtonByteListHandler.IsOn(_multiPanelPZ70.PZ70_DialPosition, MultiPanelPZ70Knobs.IAS_BUTTON) ? Visibility.Visible : Visibility.Collapsed;
                ImageLcdButtonAlt.Visibility = _multiPanelPZ70.LCDButtonByteListHandler.IsOn(_multiPanelPZ70.PZ70_DialPosition, MultiPanelPZ70Knobs.ALT_BUTTON) ? Visibility.Visible : Visibility.Collapsed;
                ImageLcdButtonVs.Visibility = _multiPanelPZ70.LCDButtonByteListHandler.IsOn(_multiPanelPZ70.PZ70_DialPosition, MultiPanelPZ70Knobs.VS_BUTTON) ? Visibility.Visible : Visibility.Collapsed;
                ImageLcdButtonApr.Visibility = _multiPanelPZ70.LCDButtonByteListHandler.IsOn(_multiPanelPZ70.PZ70_DialPosition, MultiPanelPZ70Knobs.APR_BUTTON) ? Visibility.Visible : Visibility.Collapsed;
                ImageLcdButtonRev.Visibility = _multiPanelPZ70.LCDButtonByteListHandler.IsOn(_multiPanelPZ70.PZ70_DialPosition, MultiPanelPZ70Knobs.REV_BUTTON) ? Visibility.Visible : Visibility.Collapsed;

                foreach (var keyBinding in _multiPanelPZ70.KeyBindingsHashSet)
                {
                    if (keyBinding.DialPosition == _multiPanelPZ70.PZ70_DialPosition && keyBinding.MultiPanelPZ70Knob == MultiPanelPZ70Knobs.LCD_WHEEL_DEC)
                    {
                        if (keyBinding.WhenTurnedOn)
                        {
                            if (keyBinding.OSKeyPress != null)
                            {
                                ((TagDataClassPZ70)TextBoxLcdKnobDecrease.Tag).KeyPress = keyBinding.OSKeyPress;
                                TextBoxLcdKnobDecrease.Text = ((TagDataClassPZ70)TextBoxLcdKnobDecrease.Tag).GetTextBoxKeyPressInfo();
                            }
                        }
                    }
                    if (keyBinding.DialPosition == _multiPanelPZ70.PZ70_DialPosition && keyBinding.MultiPanelPZ70Knob == MultiPanelPZ70Knobs.LCD_WHEEL_INC)
                    {
                        if (keyBinding.WhenTurnedOn)
                        {
                            if (keyBinding.OSKeyPress != null)
                            {
                                ((TagDataClassPZ70)TextBoxLcdKnobIncrease.Tag).KeyPress = keyBinding.OSKeyPress;
                                TextBoxLcdKnobIncrease.Text = ((TagDataClassPZ70)TextBoxLcdKnobIncrease.Tag).GetTextBoxKeyPressInfo();
                            }
                        }
                    }
                    if (keyBinding.DialPosition == _multiPanelPZ70.PZ70_DialPosition && keyBinding.MultiPanelPZ70Knob == MultiPanelPZ70Knobs.AUTO_THROTTLE)
                    {
                        if (keyBinding.WhenTurnedOn)
                        {
                            if (keyBinding.OSKeyPress != null)
                            {
                                ((TagDataClassPZ70)TextBoxAutoThrottleOn.Tag).KeyPress = keyBinding.OSKeyPress;
                                TextBoxAutoThrottleOn.Text = ((TagDataClassPZ70)TextBoxAutoThrottleOn.Tag).GetTextBoxKeyPressInfo();
                            }
                        }
                        else
                        {
                            if (keyBinding.OSKeyPress != null)
                            {
                                ((TagDataClassPZ70)TextBoxAutoThrottleOff.Tag).KeyPress = keyBinding.OSKeyPress;
                                TextBoxAutoThrottleOff.Text = ((TagDataClassPZ70)TextBoxAutoThrottleOff.Tag).GetTextBoxKeyPressInfo();
                            }
                        }
                    }
                    if (keyBinding.DialPosition == _multiPanelPZ70.PZ70_DialPosition && keyBinding.MultiPanelPZ70Knob == MultiPanelPZ70Knobs.FLAPS_LEVER_UP)
                    {
                        if (keyBinding.WhenTurnedOn)
                        {
                            if (keyBinding.OSKeyPress != null)
                            {
                                ((TagDataClassPZ70)TextBoxFlapsUp.Tag).KeyPress = keyBinding.OSKeyPress;
                                TextBoxFlapsUp.Text = ((TagDataClassPZ70)TextBoxFlapsUp.Tag).GetTextBoxKeyPressInfo();
                            }
                        }
                    }
                    if (keyBinding.DialPosition == _multiPanelPZ70.PZ70_DialPosition && keyBinding.MultiPanelPZ70Knob == MultiPanelPZ70Knobs.FLAPS_LEVER_DOWN)
                    {
                        if (keyBinding.WhenTurnedOn)
                        {
                            if (keyBinding.OSKeyPress != null)
                            {
                                ((TagDataClassPZ70)TextBoxFlapsDown.Tag).KeyPress = keyBinding.OSKeyPress;
                                TextBoxFlapsDown.Text = ((TagDataClassPZ70)TextBoxFlapsDown.Tag).GetTextBoxKeyPressInfo();
                            }
                        }
                    }

                    if (keyBinding.DialPosition == _multiPanelPZ70.PZ70_DialPosition && keyBinding.MultiPanelPZ70Knob == MultiPanelPZ70Knobs.PITCH_TRIM_WHEEL_UP)
                    {
                        if (keyBinding.WhenTurnedOn)
                        {
                            if (keyBinding.OSKeyPress != null)
                            {
                                ((TagDataClassPZ70)TextBoxPitchTrimUp.Tag).KeyPress = keyBinding.OSKeyPress;
                                TextBoxPitchTrimUp.Text = ((TagDataClassPZ70)TextBoxPitchTrimUp.Tag).GetTextBoxKeyPressInfo();
                            }
                        }
                    }
                    if (keyBinding.DialPosition == _multiPanelPZ70.PZ70_DialPosition && keyBinding.MultiPanelPZ70Knob == MultiPanelPZ70Knobs.PITCH_TRIM_WHEEL_DOWN)
                    {
                        if (keyBinding.WhenTurnedOn)
                        {
                            if (keyBinding.OSKeyPress != null)
                            {
                                ((TagDataClassPZ70)TextBoxPitchTrimDown.Tag).KeyPress = keyBinding.OSKeyPress;
                                TextBoxPitchTrimDown.Text = ((TagDataClassPZ70)TextBoxPitchTrimDown.Tag).GetTextBoxKeyPressInfo();
                            }
                        }
                    }
                    if (keyBinding.DialPosition == _multiPanelPZ70.PZ70_DialPosition && keyBinding.MultiPanelPZ70Knob == MultiPanelPZ70Knobs.AP_BUTTON)
                    {
                        if (keyBinding.WhenTurnedOn)
                        {
                            if (keyBinding.OSKeyPress != null)
                            {
                                ((TagDataClassPZ70)TextBoxApButtonOn.Tag).KeyPress = keyBinding.OSKeyPress;
                                TextBoxApButtonOn.Text = ((TagDataClassPZ70)TextBoxApButtonOn.Tag).GetTextBoxKeyPressInfo();
                            }
                        }
                        else
                        {
                            if (keyBinding.OSKeyPress != null)
                            {
                                ((TagDataClassPZ70)TextBoxApButtonOff.Tag).KeyPress = keyBinding.OSKeyPress;
                                TextBoxApButtonOff.Text = ((TagDataClassPZ70)TextBoxApButtonOff.Tag).GetTextBoxKeyPressInfo();
                            }
                        }
                    }
                    if (keyBinding.DialPosition == _multiPanelPZ70.PZ70_DialPosition && keyBinding.MultiPanelPZ70Knob == MultiPanelPZ70Knobs.HDG_BUTTON)
                    {
                        if (keyBinding.WhenTurnedOn)
                        {
                            if (keyBinding.OSKeyPress != null)
                            {
                                ((TagDataClassPZ70)TextBoxHdgButtonOn.Tag).KeyPress = keyBinding.OSKeyPress;
                                TextBoxHdgButtonOn.Text = ((TagDataClassPZ70)TextBoxHdgButtonOn.Tag).GetTextBoxKeyPressInfo();
                            }
                        }
                        else
                        {
                            if (keyBinding.OSKeyPress != null)
                            {
                                ((TagDataClassPZ70)TextBoxHdgButtonOff.Tag).KeyPress = keyBinding.OSKeyPress;
                                TextBoxHdgButtonOff.Text = ((TagDataClassPZ70)TextBoxHdgButtonOff.Tag).GetTextBoxKeyPressInfo();
                            }
                        }
                    }
                    if (keyBinding.DialPosition == _multiPanelPZ70.PZ70_DialPosition && keyBinding.MultiPanelPZ70Knob == MultiPanelPZ70Knobs.NAV_BUTTON)
                    {
                        if (keyBinding.WhenTurnedOn)
                        {
                            if (keyBinding.OSKeyPress != null)
                            {
                                ((TagDataClassPZ70)TextBoxNavButtonOn.Tag).KeyPress = keyBinding.OSKeyPress;
                                TextBoxNavButtonOn.Text = ((TagDataClassPZ70)TextBoxNavButtonOn.Tag).GetTextBoxKeyPressInfo();
                            }
                        }
                        else
                        {
                            if (keyBinding.OSKeyPress != null)
                            {
                                ((TagDataClassPZ70)TextBoxNavButtonOff.Tag).KeyPress = keyBinding.OSKeyPress;
                                TextBoxNavButtonOff.Text = ((TagDataClassPZ70)TextBoxNavButtonOff.Tag).GetTextBoxKeyPressInfo();
                            }
                        }
                    }
                    if (keyBinding.DialPosition == _multiPanelPZ70.PZ70_DialPosition && keyBinding.MultiPanelPZ70Knob == MultiPanelPZ70Knobs.IAS_BUTTON)
                    {
                        if (keyBinding.WhenTurnedOn)
                        {
                            if (keyBinding.OSKeyPress != null)
                            {
                                ((TagDataClassPZ70)TextBoxIasButtonOn.Tag).KeyPress = keyBinding.OSKeyPress;
                                TextBoxIasButtonOn.Text = ((TagDataClassPZ70)TextBoxIasButtonOn.Tag).GetTextBoxKeyPressInfo();
                            }
                        }
                        else
                        {
                            if (keyBinding.OSKeyPress != null)
                            {
                                ((TagDataClassPZ70)TextBoxIasButtonOff.Tag).KeyPress = keyBinding.OSKeyPress;
                                TextBoxIasButtonOff.Text = ((TagDataClassPZ70)TextBoxIasButtonOff.Tag).GetTextBoxKeyPressInfo();
                            }
                        }
                    }
                    if (keyBinding.DialPosition == _multiPanelPZ70.PZ70_DialPosition && keyBinding.MultiPanelPZ70Knob == MultiPanelPZ70Knobs.ALT_BUTTON)
                    {
                        if (keyBinding.WhenTurnedOn)
                        {
                            if (keyBinding.OSKeyPress != null)
                            {
                                ((TagDataClassPZ70)TextBoxAltButtonOn.Tag).KeyPress = keyBinding.OSKeyPress;
                                TextBoxAltButtonOn.Text = ((TagDataClassPZ70)TextBoxAltButtonOn.Tag).GetTextBoxKeyPressInfo();
                            }
                        }
                        else
                        {
                            if (keyBinding.OSKeyPress != null)
                            {
                                ((TagDataClassPZ70)TextBoxAltButtonOff.Tag).KeyPress = keyBinding.OSKeyPress;
                                TextBoxAltButtonOff.Text = ((TagDataClassPZ70)TextBoxAltButtonOff.Tag).GetTextBoxKeyPressInfo();
                            }
                        }
                    }
                    if (keyBinding.DialPosition == _multiPanelPZ70.PZ70_DialPosition && keyBinding.MultiPanelPZ70Knob == MultiPanelPZ70Knobs.VS_BUTTON)
                    {
                        if (keyBinding.WhenTurnedOn)
                        {
                            if (keyBinding.OSKeyPress != null)
                            {
                                ((TagDataClassPZ70)TextBoxVsButtonOn.Tag).KeyPress = keyBinding.OSKeyPress;
                                TextBoxVsButtonOn.Text = ((TagDataClassPZ70)TextBoxVsButtonOn.Tag).GetTextBoxKeyPressInfo();
                            }
                        }
                        else
                        {
                            if (keyBinding.OSKeyPress != null)
                            {
                                ((TagDataClassPZ70)TextBoxVsButtonOff.Tag).KeyPress = keyBinding.OSKeyPress;
                                TextBoxVsButtonOff.Text = ((TagDataClassPZ70)TextBoxVsButtonOff.Tag).GetTextBoxKeyPressInfo();
                            }
                        }
                    }
                    if (keyBinding.DialPosition == _multiPanelPZ70.PZ70_DialPosition && keyBinding.MultiPanelPZ70Knob == MultiPanelPZ70Knobs.APR_BUTTON)
                    {
                        if (keyBinding.WhenTurnedOn)
                        {
                            if (keyBinding.OSKeyPress != null)
                            {
                                ((TagDataClassPZ70)TextBoxAprButtonOn.Tag).KeyPress = keyBinding.OSKeyPress;
                                TextBoxAprButtonOn.Text = ((TagDataClassPZ70)TextBoxAprButtonOn.Tag).GetTextBoxKeyPressInfo();
                            }
                        }
                        else
                        {
                            if (keyBinding.OSKeyPress != null)
                            {
                                ((TagDataClassPZ70)TextBoxAprButtonOff.Tag).KeyPress = keyBinding.OSKeyPress;
                                TextBoxAprButtonOff.Text = ((TagDataClassPZ70)TextBoxAprButtonOff.Tag).GetTextBoxKeyPressInfo();
                            }
                        }
                    }
                    if (keyBinding.DialPosition == _multiPanelPZ70.PZ70_DialPosition && keyBinding.MultiPanelPZ70Knob == MultiPanelPZ70Knobs.REV_BUTTON)
                    {
                        if (keyBinding.WhenTurnedOn)
                        {
                            if (keyBinding.OSKeyPress != null)
                            {
                                ((TagDataClassPZ70)TextBoxRevButtonOn.Tag).KeyPress = keyBinding.OSKeyPress;
                                TextBoxRevButtonOn.Text = ((TagDataClassPZ70)TextBoxRevButtonOn.Tag).GetTextBoxKeyPressInfo();
                            }
                        }
                        else
                        {
                            if (keyBinding.OSKeyPress != null)
                            {
                                ((TagDataClassPZ70)TextBoxRevButtonOff.Tag).KeyPress = keyBinding.OSKeyPress;
                                TextBoxRevButtonOff.Text = ((TagDataClassPZ70)TextBoxRevButtonOff.Tag).GetTextBoxKeyPressInfo();
                            }
                        }
                    }
                }

                foreach (var dcsBiosBinding in _multiPanelPZ70.DCSBiosBindings)
                {
                    if (dcsBiosBinding.DialPosition == _multiPanelPZ70.PZ70_DialPosition && dcsBiosBinding.MultiPanelPZ70Knob == MultiPanelPZ70Knobs.LCD_WHEEL_DEC && dcsBiosBinding.WhenTurnedOn && dcsBiosBinding.DCSBIOSInputs.Count > 0)
                    {
                        ((TagDataClassPZ70)TextBoxLcdKnobDecrease.Tag).DCSBIOSInputs = dcsBiosBinding.DCSBIOSInputs;
                        TextBoxLcdKnobDecrease.Text = dcsBiosBinding.Description;
                        TextBoxLcdKnobDecrease.ToolTip = "DCS-BIOS";
                    }
                    if (dcsBiosBinding.DialPosition == _multiPanelPZ70.PZ70_DialPosition && dcsBiosBinding.MultiPanelPZ70Knob == MultiPanelPZ70Knobs.LCD_WHEEL_INC && dcsBiosBinding.WhenTurnedOn && dcsBiosBinding.DCSBIOSInputs.Count > 0)
                    {
                        ((TagDataClassPZ70)TextBoxLcdKnobIncrease.Tag).DCSBIOSInputs = dcsBiosBinding.DCSBIOSInputs;
                        TextBoxLcdKnobIncrease.Text = dcsBiosBinding.Description;
                        TextBoxLcdKnobIncrease.ToolTip = "DCS-BIOS";
                    }
                    if (dcsBiosBinding.DialPosition == _multiPanelPZ70.PZ70_DialPosition && dcsBiosBinding.MultiPanelPZ70Knob == MultiPanelPZ70Knobs.FLAPS_LEVER_UP && dcsBiosBinding.WhenTurnedOn && dcsBiosBinding.DCSBIOSInputs.Count > 0)
                    {
                        ((TagDataClassPZ70)TextBoxFlapsUp.Tag).DCSBIOSInputs = dcsBiosBinding.DCSBIOSInputs;
                        TextBoxFlapsUp.Text = dcsBiosBinding.Description;
                        TextBoxFlapsUp.ToolTip = "DCS-BIOS";
                    }
                    if (dcsBiosBinding.DialPosition == _multiPanelPZ70.PZ70_DialPosition && dcsBiosBinding.MultiPanelPZ70Knob == MultiPanelPZ70Knobs.FLAPS_LEVER_DOWN && dcsBiosBinding.WhenTurnedOn && dcsBiosBinding.DCSBIOSInputs.Count > 0)
                    {
                        ((TagDataClassPZ70)TextBoxFlapsDown.Tag).DCSBIOSInputs = dcsBiosBinding.DCSBIOSInputs;
                        TextBoxFlapsDown.Text = dcsBiosBinding.Description;
                        TextBoxFlapsDown.ToolTip = "DCS-BIOS";
                    }
                    if (dcsBiosBinding.DialPosition == _multiPanelPZ70.PZ70_DialPosition && dcsBiosBinding.MultiPanelPZ70Knob == MultiPanelPZ70Knobs.PITCH_TRIM_WHEEL_UP && dcsBiosBinding.WhenTurnedOn && dcsBiosBinding.DCSBIOSInputs.Count > 0)
                    {
                        ((TagDataClassPZ70)TextBoxPitchTrimUp.Tag).DCSBIOSInputs = dcsBiosBinding.DCSBIOSInputs;
                        TextBoxPitchTrimUp.Text = dcsBiosBinding.Description;
                        TextBoxPitchTrimUp.ToolTip = "DCS-BIOS";
                    }
                    if (dcsBiosBinding.DialPosition == _multiPanelPZ70.PZ70_DialPosition && dcsBiosBinding.MultiPanelPZ70Knob == MultiPanelPZ70Knobs.PITCH_TRIM_WHEEL_DOWN && dcsBiosBinding.WhenTurnedOn && dcsBiosBinding.DCSBIOSInputs.Count > 0)
                    {
                        ((TagDataClassPZ70)TextBoxPitchTrimDown.Tag).DCSBIOSInputs = dcsBiosBinding.DCSBIOSInputs;
                        TextBoxPitchTrimDown.Text = dcsBiosBinding.Description;
                        TextBoxPitchTrimDown.ToolTip = "DCS-BIOS";
                    }

                    if (dcsBiosBinding.DialPosition == _multiPanelPZ70.PZ70_DialPosition && dcsBiosBinding.MultiPanelPZ70Knob == MultiPanelPZ70Knobs.AUTO_THROTTLE)
                    {
                        if (dcsBiosBinding.WhenTurnedOn)
                        {
                            if (dcsBiosBinding.DCSBIOSInputs.Count > 0)
                            {
                                ((TagDataClassPZ70)TextBoxAutoThrottleOn.Tag).DCSBIOSInputs = dcsBiosBinding.DCSBIOSInputs;
                                TextBoxAutoThrottleOn.Text = dcsBiosBinding.Description;
                                TextBoxAutoThrottleOn.ToolTip = "DCS-BIOS";
                            }
                        }
                        else
                        {
                            if (dcsBiosBinding.DCSBIOSInputs.Count > 0)
                            {
                                ((TagDataClassPZ70)TextBoxAutoThrottleOff.Tag).DCSBIOSInputs = dcsBiosBinding.DCSBIOSInputs;
                                TextBoxAutoThrottleOff.Text = dcsBiosBinding.Description;
                                TextBoxAutoThrottleOff.ToolTip = "DCS-BIOS";
                            }
                        }
                    }
                    if (dcsBiosBinding.DialPosition == _multiPanelPZ70.PZ70_DialPosition && dcsBiosBinding.MultiPanelPZ70Knob == MultiPanelPZ70Knobs.AP_BUTTON)
                    {
                        if (dcsBiosBinding.WhenTurnedOn)
                        {
                            if (dcsBiosBinding.DCSBIOSInputs.Count > 0)
                            {
                                ((TagDataClassPZ70)TextBoxApButtonOn.Tag).DCSBIOSInputs = dcsBiosBinding.DCSBIOSInputs;
                                TextBoxApButtonOn.Text = dcsBiosBinding.Description;
                                TextBoxApButtonOn.ToolTip = "DCS-BIOS";
                            }
                        }
                        else
                        {
                            if (dcsBiosBinding.DCSBIOSInputs.Count > 0)
                            {
                                ((TagDataClassPZ70)TextBoxApButtonOff.Tag).DCSBIOSInputs = dcsBiosBinding.DCSBIOSInputs;
                                TextBoxApButtonOff.Text = dcsBiosBinding.Description;
                                TextBoxApButtonOff.ToolTip = "DCS-BIOS";
                            }
                        }
                    }
                    if (dcsBiosBinding.DialPosition == _multiPanelPZ70.PZ70_DialPosition && dcsBiosBinding.MultiPanelPZ70Knob == MultiPanelPZ70Knobs.HDG_BUTTON)
                    {
                        if (dcsBiosBinding.WhenTurnedOn)
                        {
                            if (dcsBiosBinding.DCSBIOSInputs.Count > 0)
                            {
                                ((TagDataClassPZ70)TextBoxHdgButtonOn.Tag).DCSBIOSInputs = dcsBiosBinding.DCSBIOSInputs;
                                TextBoxHdgButtonOn.Text = dcsBiosBinding.Description;
                                TextBoxHdgButtonOn.ToolTip = "DCS-BIOS";
                            }
                        }
                        else
                        {
                            if (dcsBiosBinding.DCSBIOSInputs.Count > 0)
                            {
                                ((TagDataClassPZ70)TextBoxHdgButtonOff.Tag).DCSBIOSInputs = dcsBiosBinding.DCSBIOSInputs;
                                TextBoxHdgButtonOff.Text = dcsBiosBinding.Description;
                                TextBoxHdgButtonOff.ToolTip = "DCS-BIOS";
                            }
                        }
                    }
                    if (dcsBiosBinding.DialPosition == _multiPanelPZ70.PZ70_DialPosition && dcsBiosBinding.MultiPanelPZ70Knob == MultiPanelPZ70Knobs.NAV_BUTTON)
                    {
                        if (dcsBiosBinding.WhenTurnedOn)
                        {
                            if (dcsBiosBinding.DCSBIOSInputs.Count > 0)
                            {
                                ((TagDataClassPZ70)TextBoxNavButtonOn.Tag).DCSBIOSInputs = dcsBiosBinding.DCSBIOSInputs;
                                TextBoxNavButtonOn.Text = dcsBiosBinding.Description;
                                TextBoxNavButtonOn.ToolTip = "DCS-BIOS";
                            }
                        }
                        else
                        {
                            if (dcsBiosBinding.DCSBIOSInputs.Count > 0)
                            {
                                ((TagDataClassPZ70)TextBoxNavButtonOff.Tag).DCSBIOSInputs = dcsBiosBinding.DCSBIOSInputs;
                                TextBoxNavButtonOff.Text = dcsBiosBinding.Description;
                                TextBoxNavButtonOff.ToolTip = "DCS-BIOS";
                            }
                        }
                    }
                    if (dcsBiosBinding.DialPosition == _multiPanelPZ70.PZ70_DialPosition && dcsBiosBinding.MultiPanelPZ70Knob == MultiPanelPZ70Knobs.IAS_BUTTON)
                    {
                        if (dcsBiosBinding.WhenTurnedOn)
                        {
                            if (dcsBiosBinding.DCSBIOSInputs.Count > 0)
                            {
                                ((TagDataClassPZ70)TextBoxIasButtonOn.Tag).DCSBIOSInputs = dcsBiosBinding.DCSBIOSInputs;
                                TextBoxIasButtonOn.Text = dcsBiosBinding.Description;
                                TextBoxIasButtonOn.ToolTip = "DCS-BIOS";
                            }
                        }
                        else
                        {
                            if (dcsBiosBinding.DCSBIOSInputs.Count > 0)
                            {
                                ((TagDataClassPZ70)TextBoxIasButtonOff.Tag).DCSBIOSInputs = dcsBiosBinding.DCSBIOSInputs;
                                TextBoxIasButtonOff.Text = dcsBiosBinding.Description;
                                TextBoxIasButtonOff.ToolTip = "DCS-BIOS";
                            }
                        }
                    }
                    if (dcsBiosBinding.DialPosition == _multiPanelPZ70.PZ70_DialPosition && dcsBiosBinding.MultiPanelPZ70Knob == MultiPanelPZ70Knobs.ALT_BUTTON)
                    {
                        if (dcsBiosBinding.WhenTurnedOn)
                        {
                            if (dcsBiosBinding.DCSBIOSInputs.Count > 0)
                            {
                                ((TagDataClassPZ70)TextBoxAltButtonOn.Tag).DCSBIOSInputs = dcsBiosBinding.DCSBIOSInputs;
                                TextBoxAltButtonOn.Text = dcsBiosBinding.Description;
                                TextBoxAltButtonOn.ToolTip = "DCS-BIOS";
                            }
                        }
                        else
                        {
                            if (dcsBiosBinding.DCSBIOSInputs.Count > 0)
                            {
                                ((TagDataClassPZ70)TextBoxAltButtonOff.Tag).DCSBIOSInputs = dcsBiosBinding.DCSBIOSInputs;
                                TextBoxAltButtonOff.Text = dcsBiosBinding.Description;
                                TextBoxAltButtonOff.ToolTip = "DCS-BIOS";
                            }
                        }
                    }
                    if (dcsBiosBinding.DialPosition == _multiPanelPZ70.PZ70_DialPosition && dcsBiosBinding.MultiPanelPZ70Knob == MultiPanelPZ70Knobs.VS_BUTTON)
                    {
                        if (dcsBiosBinding.WhenTurnedOn)
                        {
                            if (dcsBiosBinding.DCSBIOSInputs.Count > 0)
                            {
                                ((TagDataClassPZ70)TextBoxVsButtonOn.Tag).DCSBIOSInputs = dcsBiosBinding.DCSBIOSInputs;
                                TextBoxVsButtonOn.Text = dcsBiosBinding.Description;
                                TextBoxVsButtonOn.ToolTip = "DCS-BIOS";
                            }
                        }
                        else
                        {
                            if (dcsBiosBinding.DCSBIOSInputs.Count > 0)
                            {
                                ((TagDataClassPZ70)TextBoxVsButtonOff.Tag).DCSBIOSInputs = dcsBiosBinding.DCSBIOSInputs;
                                TextBoxVsButtonOff.Text = dcsBiosBinding.Description;
                                TextBoxVsButtonOff.ToolTip = "DCS-BIOS";
                            }
                        }
                    }
                    if (dcsBiosBinding.DialPosition == _multiPanelPZ70.PZ70_DialPosition && dcsBiosBinding.MultiPanelPZ70Knob == MultiPanelPZ70Knobs.APR_BUTTON)
                    {
                        if (dcsBiosBinding.WhenTurnedOn)
                        {
                            if (dcsBiosBinding.DCSBIOSInputs.Count > 0)
                            {
                                ((TagDataClassPZ70)TextBoxAprButtonOn.Tag).DCSBIOSInputs = dcsBiosBinding.DCSBIOSInputs;
                                TextBoxAprButtonOn.Text = dcsBiosBinding.Description;
                                TextBoxAprButtonOn.ToolTip = "DCS-BIOS";
                            }
                        }
                        else
                        {
                            if (dcsBiosBinding.DCSBIOSInputs.Count > 0)
                            {
                                ((TagDataClassPZ70)TextBoxAprButtonOff.Tag).DCSBIOSInputs = dcsBiosBinding.DCSBIOSInputs;
                                TextBoxAprButtonOff.Text = dcsBiosBinding.Description;
                                TextBoxAprButtonOff.ToolTip = "DCS-BIOS";
                            }
                        }
                    }
                    if (dcsBiosBinding.DialPosition == _multiPanelPZ70.PZ70_DialPosition && dcsBiosBinding.MultiPanelPZ70Knob == MultiPanelPZ70Knobs.REV_BUTTON)
                    {
                        if (dcsBiosBinding.WhenTurnedOn)
                        {
                            if (dcsBiosBinding.DCSBIOSInputs.Count > 0)
                            {
                                ((TagDataClassPZ70)TextBoxRevButtonOn.Tag).DCSBIOSInputs = dcsBiosBinding.DCSBIOSInputs;
                                TextBoxRevButtonOn.Text = dcsBiosBinding.Description;
                                TextBoxRevButtonOn.ToolTip = "DCS-BIOS";
                            }
                        }
                        else
                        {
                            if (dcsBiosBinding.DCSBIOSInputs.Count > 0)
                            {
                                ((TagDataClassPZ70)TextBoxRevButtonOff.Tag).DCSBIOSInputs = dcsBiosBinding.DCSBIOSInputs;
                                TextBoxRevButtonOff.Text = dcsBiosBinding.Description;
                                TextBoxRevButtonOff.ToolTip = "DCS-BIOS";
                            }
                        }
                    }
                }


                foreach (var bipLink in _multiPanelPZ70.BIPLinkHashSet)
                {
                    if (bipLink.DialPosition == _multiPanelPZ70.PZ70_DialPosition && bipLink.MultiPanelPZ70Knob == MultiPanelPZ70Knobs.LCD_WHEEL_DEC && bipLink.WhenTurnedOn && bipLink.BIPLights.Count > 0)
                    {
                        ((TagDataClassPZ70) TextBoxLcdKnobDecrease.Tag).BIPLink = bipLink;
                        TextBoxLcdKnobDecrease.Background = Brushes.Bisque;
                    }
                    if (bipLink.DialPosition == _multiPanelPZ70.PZ70_DialPosition && bipLink.MultiPanelPZ70Knob == MultiPanelPZ70Knobs.LCD_WHEEL_INC && bipLink.WhenTurnedOn && bipLink.BIPLights.Count > 0)
                    {
                        ((TagDataClassPZ70)TextBoxLcdKnobIncrease.Tag).BIPLink = bipLink;
                        TextBoxLcdKnobIncrease.Background = Brushes.Bisque;
                    }
                    if (bipLink.DialPosition == _multiPanelPZ70.PZ70_DialPosition && bipLink.MultiPanelPZ70Knob == MultiPanelPZ70Knobs.FLAPS_LEVER_UP && bipLink.WhenTurnedOn && bipLink.BIPLights.Count > 0)
                    {
                        ((TagDataClassPZ70)TextBoxFlapsUp.Tag).BIPLink = bipLink;
                        TextBoxFlapsUp.Background = Brushes.Bisque;
                    }
                    if (bipLink.DialPosition == _multiPanelPZ70.PZ70_DialPosition && bipLink.MultiPanelPZ70Knob == MultiPanelPZ70Knobs.FLAPS_LEVER_DOWN && bipLink.WhenTurnedOn && bipLink.BIPLights.Count > 0)
                    {
                        ((TagDataClassPZ70)TextBoxFlapsDown.Tag).BIPLink = bipLink;
                        TextBoxFlapsDown.Background = Brushes.Bisque;
                    }
                    if (bipLink.DialPosition == _multiPanelPZ70.PZ70_DialPosition && bipLink.MultiPanelPZ70Knob == MultiPanelPZ70Knobs.PITCH_TRIM_WHEEL_UP && bipLink.WhenTurnedOn && bipLink.BIPLights.Count > 0)
                    {
                        ((TagDataClassPZ70)TextBoxPitchTrimUp.Tag).BIPLink = bipLink;
                        TextBoxPitchTrimUp.Background = Brushes.Bisque;
                    }
                    if (bipLink.DialPosition == _multiPanelPZ70.PZ70_DialPosition && bipLink.MultiPanelPZ70Knob == MultiPanelPZ70Knobs.PITCH_TRIM_WHEEL_DOWN && bipLink.WhenTurnedOn && bipLink.BIPLights.Count > 0)
                    {
                        ((TagDataClassPZ70)TextBoxPitchTrimDown.Tag).BIPLink = bipLink;
                        TextBoxPitchTrimDown.Background = Brushes.Bisque;
                    }

                    if (bipLink.DialPosition == _multiPanelPZ70.PZ70_DialPosition && bipLink.MultiPanelPZ70Knob == MultiPanelPZ70Knobs.AUTO_THROTTLE)
                    {
                        if (bipLink.WhenTurnedOn)
                        {
                            if (bipLink.BIPLights.Count > 0)
                            {
                                ((TagDataClassPZ70)TextBoxAutoThrottleOn.Tag).BIPLink = bipLink;
                                TextBoxAutoThrottleOn.Background = Brushes.Bisque;
                            }
                        }
                        else
                        {
                            if (bipLink.BIPLights.Count > 0)
                            {
                                ((TagDataClassPZ70)TextBoxAutoThrottleOff.Tag).BIPLink = bipLink;
                                TextBoxAutoThrottleOff.Background = Brushes.Bisque;
                            }
                        }
                    }
                    if (bipLink.DialPosition == _multiPanelPZ70.PZ70_DialPosition && bipLink.MultiPanelPZ70Knob == MultiPanelPZ70Knobs.AP_BUTTON)
                    {
                        if (bipLink.WhenTurnedOn)
                        {
                            if (bipLink.BIPLights.Count > 0)
                            {
                                ((TagDataClassPZ70)TextBoxApButtonOn.Tag).BIPLink = bipLink;
                                TextBoxApButtonOn.Background = Brushes.Bisque;
                            }
                        }
                        else
                        {
                            if (bipLink.BIPLights.Count > 0)
                            {
                                ((TagDataClassPZ70)TextBoxApButtonOff.Tag).BIPLink = bipLink;
                                TextBoxApButtonOff.Background = Brushes.Bisque;
                            }
                        }
                    }
                    if (bipLink.DialPosition == _multiPanelPZ70.PZ70_DialPosition && bipLink.MultiPanelPZ70Knob == MultiPanelPZ70Knobs.HDG_BUTTON)
                    {
                        if (bipLink.WhenTurnedOn)
                        {
                            if (bipLink.BIPLights.Count > 0)
                            {
                                ((TagDataClassPZ70)TextBoxHdgButtonOn.Tag).BIPLink = bipLink;
                                TextBoxHdgButtonOn.Background = Brushes.Bisque;
                            }
                        }
                        else
                        {
                            if (bipLink.BIPLights.Count > 0)
                            {
                                ((TagDataClassPZ70)TextBoxHdgButtonOff.Tag).BIPLink = bipLink;
                                TextBoxHdgButtonOff.Background = Brushes.Bisque;
                            }
                        }
                    }
                    if (bipLink.DialPosition == _multiPanelPZ70.PZ70_DialPosition && bipLink.MultiPanelPZ70Knob == MultiPanelPZ70Knobs.NAV_BUTTON)
                    {
                        if (bipLink.WhenTurnedOn)
                        {
                            if (bipLink.BIPLights.Count > 0)
                            {
                                ((TagDataClassPZ70)TextBoxNavButtonOn.Tag).BIPLink = bipLink;
                                TextBoxNavButtonOn.Background = Brushes.Bisque;
                            }
                        }
                        else
                        {
                            if (bipLink.BIPLights.Count > 0)
                            {
                                ((TagDataClassPZ70)TextBoxNavButtonOff.Tag).BIPLink = bipLink;
                                TextBoxNavButtonOff.Background = Brushes.Bisque;
                            }
                        }
                    }
                    if (bipLink.DialPosition == _multiPanelPZ70.PZ70_DialPosition && bipLink.MultiPanelPZ70Knob == MultiPanelPZ70Knobs.IAS_BUTTON)
                    {
                        if (bipLink.WhenTurnedOn)
                        {
                            if (bipLink.BIPLights.Count > 0)
                            {
                                ((TagDataClassPZ70)TextBoxIasButtonOn.Tag).BIPLink = bipLink;
                                TextBoxIasButtonOn.Background = Brushes.Bisque;
                            }
                        }
                        else
                        {
                            if (bipLink.BIPLights.Count > 0)
                            {
                                ((TagDataClassPZ70)TextBoxIasButtonOff.Tag).BIPLink = bipLink;
                                TextBoxIasButtonOff.Background = Brushes.Bisque;
                            }
                        }
                    }
                    if (bipLink.DialPosition == _multiPanelPZ70.PZ70_DialPosition && bipLink.MultiPanelPZ70Knob == MultiPanelPZ70Knobs.ALT_BUTTON)
                    {
                        if (bipLink.WhenTurnedOn)
                        {
                            if (bipLink.BIPLights.Count > 0)
                            {
                                ((TagDataClassPZ70)TextBoxAltButtonOn.Tag).BIPLink = bipLink;
                                TextBoxAltButtonOn.Background = Brushes.Bisque;
                            }
                        }
                        else
                        {
                            if (bipLink.BIPLights.Count > 0)
                            {
                                ((TagDataClassPZ70)TextBoxAltButtonOff.Tag).BIPLink = bipLink;
                                TextBoxAltButtonOff.Background = Brushes.Bisque;
                            }
                        }
                    }
                    if (bipLink.DialPosition == _multiPanelPZ70.PZ70_DialPosition && bipLink.MultiPanelPZ70Knob == MultiPanelPZ70Knobs.VS_BUTTON)
                    {
                        if (bipLink.WhenTurnedOn)
                        {
                            if (bipLink.BIPLights.Count > 0)
                            {
                                ((TagDataClassPZ70)TextBoxVsButtonOn.Tag).BIPLink = bipLink;
                                TextBoxVsButtonOn.Background = Brushes.Bisque;
                            }
                        }
                        else
                        {
                            if (bipLink.BIPLights.Count > 0)
                            {
                                ((TagDataClassPZ70)TextBoxVsButtonOff.Tag).BIPLink = bipLink;
                                TextBoxVsButtonOff.Background = Brushes.Bisque;
                            }
                        }
                    }
                    if (bipLink.DialPosition == _multiPanelPZ70.PZ70_DialPosition && bipLink.MultiPanelPZ70Knob == MultiPanelPZ70Knobs.APR_BUTTON)
                    {
                        if (bipLink.WhenTurnedOn)
                        {
                            if (bipLink.BIPLights.Count > 0)
                            {
                                ((TagDataClassPZ70)TextBoxAprButtonOn.Tag).BIPLink = bipLink;
                                TextBoxAprButtonOn.Background = Brushes.Bisque;
                            }
                        }
                        else
                        {
                            if (bipLink.BIPLights.Count > 0)
                            {
                                ((TagDataClassPZ70)TextBoxAprButtonOff.Tag).BIPLink = bipLink;
                                TextBoxAprButtonOff.Background = Brushes.Bisque;
                            }
                        }
                    }
                    if (bipLink.DialPosition == _multiPanelPZ70.PZ70_DialPosition && bipLink.MultiPanelPZ70Knob == MultiPanelPZ70Knobs.REV_BUTTON)
                    {
                        if (bipLink.WhenTurnedOn)
                        {
                            if (bipLink.BIPLights.Count > 0)
                            {
                                ((TagDataClassPZ70)TextBoxRevButtonOn.Tag).BIPLink = bipLink;
                                TextBoxRevButtonOn.Background = Brushes.Bisque;
                            }
                        }
                        else
                        {
                            if (bipLink.BIPLights.Count > 0)
                            {
                                ((TagDataClassPZ70)TextBoxRevButtonOff.Tag).BIPLink = bipLink;
                                TextBoxRevButtonOff.Background = Brushes.Bisque;
                            }
                        }
                    }
                }

                ImageLcdUpperRow.Visibility = Visibility.Collapsed;
                ImageLcdLowerRow.Visibility = Visibility.Collapsed;
                //Dial position IAS HDG CRS -> Only upper LCD row can be used -> Hide Lower Button
                if (Common.NoDCSBIOSEnabled() || _multiPanelPZ70.PZ70_DialPosition == PZ70DialPosition.IAS || _multiPanelPZ70.PZ70_DialPosition == PZ70DialPosition.HDG || _multiPanelPZ70.PZ70_DialPosition == PZ70DialPosition.CRS)
                {
                    ButtonLcdLower.Visibility = Visibility.Hidden;
                }
                else if (Common.IsOperationModeFlagSet(OperationFlag.DCSBIOSOutputEnabled))  
                {
                    ButtonLcdLower.Visibility = Visibility.Visible;
                }
                foreach (var dcsBiosBindingLCD in _multiPanelPZ70.LCDBindings)
                {
                    if (dcsBiosBindingLCD.DialPosition == _multiPanelPZ70.PZ70_DialPosition && dcsBiosBindingLCD.PZ70LCDPosition == PZ70LCDPosition.UpperLCD && dcsBiosBindingLCD.HasBinding)
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
                    if (dcsBiosBindingLCD.DialPosition == _multiPanelPZ70.PZ70_DialPosition && dcsBiosBindingLCD.PZ70LCDPosition == PZ70LCDPosition.LowerLCD && dcsBiosBindingLCD.HasBinding)
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
                Common.ShowErrorMessageBox(993013, ex);
            }
        }


        private void TextBoxShortcutKeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                var textBox = ((TextBox)sender);
                //Check if this textbox contains sequence or DCS-BIOS information. If so then exit
                if (((TagDataClassPZ70)textBox.Tag).ContainsKeySequence() || ((TagDataClassPZ70)textBox.Tag).ContainsDCSBIOS())
                {
                    return;
                }
                var keyPressed = (VirtualKeyCode)KeyInterop.VirtualKeyFromKey(e.Key);
                e.Handled = true;

                var hashSetOfKeysPressed = new HashSet<string>();
                hashSetOfKeysPressed.Add(Enum.GetName(typeof(VirtualKeyCode), keyPressed));

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
                textBox.Text = result;
                ((TagDataClassPZ70)textBox.Tag).KeyPress = new OSKeyPress(result);
                UpdateKeyBindingProfileSequencedKeyStrokesPZ70(textBox);
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(3008, ex);
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
                    if (((TagDataClassPZ70)textBox.Tag).ContainsDCSBIOS())
                    {
                        if (MessageBox.Show("Do you want to delete the DCS-BIOS configuration?", "Delete DCS-BIOS control?", MessageBoxButton.OKCancel, MessageBoxImage.Question) != MessageBoxResult.OK)
                        {
                            return;
                        }
                        textBox.Text = "";
                        _multiPanelPZ70.RemoveMultiPanelKnobFromList(ControlListPZ70.DCSBIOS,GetPZ70Knob(textBox).MultiPanelPZ70Knob, GetPZ70Knob(textBox).On);
                        ((TagDataClassPZ70)textBox.Tag).DCSBIOSInputs.Clear();
                    }
                    else if (((TagDataClassPZ70)textBox.Tag).ContainsKeySequence())
                    {
                        //Check if this textbox contains sequence information. If so then prompt the user for deletion
                        if (MessageBox.Show("Do you want to delete the key sequence?", "Delete key sequence?", MessageBoxButton.OKCancel, MessageBoxImage.Question) != MessageBoxResult.OK)
                        {
                            return;
                        }
                        ((TagDataClassPZ70)textBox.Tag).KeyPress.KeySequence.Clear();
                        textBox.Text = "";
                        UpdateKeyBindingProfileSimpleKeyStrokes(textBox);
                    }
                    else if (((TagDataClassPZ70)textBox.Tag).ContainsSingleKey())
                    {
                        ((TagDataClassPZ70)textBox.Tag).KeyPress.KeySequence.Clear();
                        textBox.Text = "";
                        UpdateKeyBindingProfileSimpleKeyStrokes(textBox);
                    }
                    if (((TagDataClassPZ70)textBox.Tag).ContainsBIPLink())
                    {
                        //Check if this textbox contains sequence information. If so then prompt the user for deletion
                        if (MessageBox.Show("Do you want to delete BIP Links?", "Delete BIP Link?", MessageBoxButton.OKCancel, MessageBoxImage.Question) != MessageBoxResult.OK)
                        {
                            return;
                        }
                        ((TagDataClassPZ70)textBox.Tag).BIPLink.BIPLights.Clear();
                        textBox.Background = Brushes.White;
                        UpdateBIPLinkBindings(textBox);
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
                if (((TagDataClassPZ70)textBox.Tag).ContainsKeySequence() || ((TagDataClassPZ70)textBox.Tag).ContainsDCSBIOS())
                {
                    return;
                }
                var hashSetOfKeysPressed = new HashSet<string>();

                /*if (((TextBoxTagHolderClass)textBox.Tag) == null)
                {
                    ((TextBoxTagHolderClass)textBox.Tag) = xxKeyPressLength.FiftyMilliSec;
                }*/

                var keyCode = KeyInterop.VirtualKeyFromKey(e.Key);
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
                textBox.Text = result;
                ((TagDataClassPZ70)textBox.Tag).KeyPress = new OSKeyPress(result);
                UpdateKeyBindingProfileSimpleKeyStrokes(textBox);
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(3006, ex);
            }
        }

        private void TextBoxTextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                //MAKE SURE THE Tag iss SET BEFORE SETTING TEXT! OTHERWISE THIS DOESN'T FIRE
                var textBox = (TextBox)sender;
                if (((TagDataClassPZ70)textBox.Tag).ContainsKeySequence())
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

                if (!(bool)e.NewValue)
                {
                    //Do not show if not visible
                    return;
                }



                if (!((TagDataClassPZ70)textBox.Tag).ContainsSingleKey())
                {
                    return;
                }
                var keyPressLength = ((TagDataClassPZ70)textBox.Tag).KeyPress.GetLengthOfKeyPress();

                foreach (MenuItem item in contextMenu.Items)
                {
                    item.IsChecked = false;
                }

                foreach (MenuItem item in contextMenu.Items)
                {
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
                if (contextMenuItem.Name == "contextMenuItemFiftyMilliSec")
                {
                    ((TagDataClassPZ70)textBox.Tag).KeyPress.SetLengthOfKeyPress(KeyPressLength.FiftyMilliSec);
                }
                else if (contextMenuItem.Name == "contextMenuItemHalfSecond")
                {
                    ((TagDataClassPZ70)textBox.Tag).KeyPress.SetLengthOfKeyPress(KeyPressLength.HalfSecond);
                }
                else if (contextMenuItem.Name == "contextMenuItemSecond")
                {
                    ((TagDataClassPZ70)textBox.Tag).KeyPress.SetLengthOfKeyPress(KeyPressLength.Second);
                }
                else if (contextMenuItem.Name == "contextMenuItemSecondAndHalf")
                {
                    ((TagDataClassPZ70)textBox.Tag).KeyPress.SetLengthOfKeyPress(KeyPressLength.SecondAndHalf);
                }
                else if (contextMenuItem.Name == "contextMenuItemTwoSeconds")
                {
                    ((TagDataClassPZ70)textBox.Tag).KeyPress.SetLengthOfKeyPress(KeyPressLength.TwoSeconds);
                }
                else if (contextMenuItem.Name == "contextMenuItemThreeSeconds")
                {
                    ((TagDataClassPZ70)textBox.Tag).KeyPress.SetLengthOfKeyPress(KeyPressLength.ThreeSeconds);
                }
                else if (contextMenuItem.Name == "contextMenuItemFourSeconds")
                {
                    ((TagDataClassPZ70)textBox.Tag).KeyPress.SetLengthOfKeyPress(KeyPressLength.FourSeconds);
                }
                else if (contextMenuItem.Name == "contextMenuItemFiveSecs")
                {
                    ((TagDataClassPZ70)textBox.Tag).KeyPress.SetLengthOfKeyPress(KeyPressLength.FiveSecs);
                }
                else if (contextMenuItem.Name == "contextMenuItemTenSecs")
                {
                    ((TagDataClassPZ70)textBox.Tag).KeyPress.SetLengthOfKeyPress(KeyPressLength.TenSecs);
                }
                else if (contextMenuItem.Name == "contextMenuItemFifteenSecs")
                {
                    ((TagDataClassPZ70)textBox.Tag).KeyPress.SetLengthOfKeyPress(KeyPressLength.FifteenSecs);
                }
                else if (contextMenuItem.Name == "contextMenuItemTwentySecs")
                {
                    ((TagDataClassPZ70)textBox.Tag).KeyPress.SetLengthOfKeyPress(KeyPressLength.TwentySecs);
                }
                else if (contextMenuItem.Name == "contextMenuItemThirtySecs")
                {
                    ((TagDataClassPZ70)textBox.Tag).KeyPress.SetLengthOfKeyPress(KeyPressLength.ThirtySecs);
                }
                else if (contextMenuItem.Name == "contextMenuItemFortySecs")
                {
                    ((TagDataClassPZ70)textBox.Tag).KeyPress.SetLengthOfKeyPress(KeyPressLength.FortySecs);
                }
                else if (contextMenuItem.Name == "contextMenuItemSixtySecs")
                {
                    ((TagDataClassPZ70)textBox.Tag).KeyPress.SetLengthOfKeyPress(KeyPressLength.SixtySecs);
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
                SequenceWindow sequenceWindow;
                if (((TagDataClassPZ70)textBox.Tag).ContainsKeySequence())
                {
                    sequenceWindow = new SequenceWindow(textBox.Text, ((TagDataClassPZ70)textBox.Tag).GetKeySequence());
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
                    if (sequenceList.Count > 1)
                    {
                        var osKeyPress = new OSKeyPress("Key press sequence", sequenceList);
                        ((TagDataClassPZ70)textBox.Tag).KeyPress = osKeyPress;
                        ((TagDataClassPZ70)textBox.Tag).KeyPress.Information = sequenceWindow.GetInformation;
                        if (!string.IsNullOrEmpty(sequenceWindow.GetInformation))
                        {
                            textBox.Text = sequenceWindow.GetInformation;
                        }
                        //textBox.Text = string.IsNullOrEmpty(sequenceWindow.GetInformation) ? "Key press sequence" : sequenceWindow.GetInformation;
                        /*if (!string.IsNullOrEmpty(sequenceWindow.GetInformation))
                        {
                            var toolTip = new ToolTip { Content = sequenceWindow.GetInformation };
                            textBox.ToolTipa = toolTip;
                        }*/
                        UpdateKeyBindingProfileSequencedKeyStrokesPZ70(textBox);
                    }
                    else
                    {
                        //If only one press was created treat it as a simple keypress
                        ((TagDataClassPZ70)textBox.Tag).ClearAll();
                        var osKeyPress = new OSKeyPress(sequenceList[0].VirtualKeyCodesAsString, sequenceList[0].LengthOfKeyPress);
                        ((TagDataClassPZ70)textBox.Tag).KeyPress = osKeyPress;
                        ((TagDataClassPZ70)textBox.Tag).KeyPress.Information = sequenceWindow.GetInformation;
                        textBox.Text = sequenceList[0].VirtualKeyCodesAsString;
                        /*textBox.Text = sequenceList.Values[0].VirtualKeyCodesAsString;
                        if (!string.IsNullOrEmpty(sequenceWindow.GetInformation))
                        {
                            var toolTip = new ToolTip { Content = sequenceWindow.GetInformation };
                            textBox.ToolTipa = toolTip;
                        }*/
                        UpdateKeyBindingProfileSimpleKeyStrokes(textBox);
                    }
                }
                TextBoxLogPZ70.Focus();
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
                    item.IsEnabled = !Common.KeyEmulationOnly();
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
                if (((TagDataClassPZ70)textBox.Tag).ContainsDCSBIOS())
                {
                    dcsBIOSControlsConfigsWindow = new DCSBIOSControlsConfigsWindow(_globalHandler.GetAirframe(), textBox.Name.Replace("TextBox", ""), ((TagDataClassPZ70)textBox.Tag).DCSBIOSInputs, textBox.Text);
                }
                else
                {
                    dcsBIOSControlsConfigsWindow = new DCSBIOSControlsConfigsWindow(_globalHandler.GetAirframe(), textBox.Name.Replace("TextBox", ""), null);
                }
                dcsBIOSControlsConfigsWindow.ShowDialog();
                if (dcsBIOSControlsConfigsWindow.DialogResult.HasValue && dcsBIOSControlsConfigsWindow.DialogResult == true)
                {
                    var dcsBiosInputs = dcsBIOSControlsConfigsWindow.DCSBIOSInputs;
                    var text = string.IsNullOrWhiteSpace(dcsBIOSControlsConfigsWindow.Description) ? "DCS-BIOS" : dcsBIOSControlsConfigsWindow.Description;
                    //1 appropriate text to textbox
                    //2 update bindings
                    textBox.Text = text;
                    ((TagDataClassPZ70)textBox.Tag).DCSBIOSInputs = dcsBiosInputs;
                    UpdateDCSBIOSBinding(textBox);
                }
                TextBoxLogPZ70.Focus();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(442044, ex);
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
                if (((TagDataClassPZ70)textBox.Tag).ContainsBIPLink())
                {
                    var bipLink = ((TagDataClassPZ70)textBox.Tag).BIPLink;
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
                    ((TagDataClassPZ70)textBox.Tag).BIPLink = (BIPLinkPZ70) bipLinkWindow.BIPLink;
                    UpdateBIPLinkBindings(textBox);
                }
                TextBoxLogPZ70.Focus();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(442044, ex);
            }
        }


        private void UpdateBIPLinkBindings(TextBox textBox)
        {
            try
            {
                if (textBox.Equals(TextBoxLcdKnobDecrease))
                {
                    _multiPanelPZ70.AddOrUpdateBIPLinkKnobBinding(MultiPanelPZ70Knobs.LCD_WHEEL_DEC, ((TagDataClassPZ70)textBox.Tag).BIPLink);
                }
                if (textBox.Equals(TextBoxLcdKnobIncrease))
                {
                    _multiPanelPZ70.AddOrUpdateBIPLinkKnobBinding(MultiPanelPZ70Knobs.LCD_WHEEL_INC, ((TagDataClassPZ70)textBox.Tag).BIPLink);
                }
                if (textBox.Equals(TextBoxAutoThrottleOff))
                {
                    _multiPanelPZ70.AddOrUpdateBIPLinkKnobBinding(MultiPanelPZ70Knobs.AUTO_THROTTLE, ((TagDataClassPZ70)textBox.Tag).BIPLink, false);
                }
                if (textBox.Equals(TextBoxAutoThrottleOn))
                {
                    _multiPanelPZ70.AddOrUpdateBIPLinkKnobBinding(MultiPanelPZ70Knobs.AUTO_THROTTLE, ((TagDataClassPZ70)textBox.Tag).BIPLink);
                }
                if (textBox.Equals(TextBoxFlapsUp))
                {
                    _multiPanelPZ70.AddOrUpdateBIPLinkKnobBinding(MultiPanelPZ70Knobs.FLAPS_LEVER_UP, ((TagDataClassPZ70)textBox.Tag).BIPLink);
                }
                if (textBox.Equals(TextBoxFlapsDown))
                {
                    _multiPanelPZ70.AddOrUpdateBIPLinkKnobBinding(MultiPanelPZ70Knobs.FLAPS_LEVER_DOWN, ((TagDataClassPZ70)textBox.Tag).BIPLink);
                }
                if (textBox.Equals(TextBoxPitchTrimUp))
                {
                    _multiPanelPZ70.AddOrUpdateBIPLinkKnobBinding(MultiPanelPZ70Knobs.PITCH_TRIM_WHEEL_UP, ((TagDataClassPZ70)textBox.Tag).BIPLink);
                }
                if (textBox.Equals(TextBoxPitchTrimDown))
                {
                    _multiPanelPZ70.AddOrUpdateBIPLinkKnobBinding(MultiPanelPZ70Knobs.PITCH_TRIM_WHEEL_DOWN, ((TagDataClassPZ70)textBox.Tag).BIPLink);
                }
                if (textBox.Equals(TextBoxApButtonOn))
                {
                    _multiPanelPZ70.AddOrUpdateBIPLinkKnobBinding(MultiPanelPZ70Knobs.AP_BUTTON, ((TagDataClassPZ70)textBox.Tag).BIPLink);
                }
                if (textBox.Equals(TextBoxApButtonOff))
                {
                    _multiPanelPZ70.AddOrUpdateBIPLinkKnobBinding(MultiPanelPZ70Knobs.AP_BUTTON, ((TagDataClassPZ70)textBox.Tag).BIPLink, false);
                }
                if (textBox.Equals(TextBoxHdgButtonOn))
                {
                    _multiPanelPZ70.AddOrUpdateBIPLinkKnobBinding(MultiPanelPZ70Knobs.HDG_BUTTON, ((TagDataClassPZ70)textBox.Tag).BIPLink);
                }
                if (textBox.Equals(TextBoxHdgButtonOff))
                {
                    _multiPanelPZ70.AddOrUpdateBIPLinkKnobBinding(MultiPanelPZ70Knobs.HDG_BUTTON, ((TagDataClassPZ70)textBox.Tag).BIPLink, false);
                }
                if (textBox.Equals(TextBoxNavButtonOn))
                {
                    _multiPanelPZ70.AddOrUpdateBIPLinkKnobBinding(MultiPanelPZ70Knobs.NAV_BUTTON, ((TagDataClassPZ70)textBox.Tag).BIPLink);
                }
                if (textBox.Equals(TextBoxNavButtonOff))
                {
                    _multiPanelPZ70.AddOrUpdateBIPLinkKnobBinding(MultiPanelPZ70Knobs.NAV_BUTTON, ((TagDataClassPZ70)textBox.Tag).BIPLink, false);
                }
                if (textBox.Equals(TextBoxIasButtonOn))
                {
                    _multiPanelPZ70.AddOrUpdateBIPLinkKnobBinding(MultiPanelPZ70Knobs.IAS_BUTTON, ((TagDataClassPZ70)textBox.Tag).BIPLink);
                }
                if (textBox.Equals(TextBoxIasButtonOff))
                {
                    _multiPanelPZ70.AddOrUpdateBIPLinkKnobBinding(MultiPanelPZ70Knobs.IAS_BUTTON, ((TagDataClassPZ70)textBox.Tag).BIPLink, false);
                }
                if (textBox.Equals(TextBoxAltButtonOn))
                {
                    _multiPanelPZ70.AddOrUpdateBIPLinkKnobBinding(MultiPanelPZ70Knobs.ALT_BUTTON, ((TagDataClassPZ70)textBox.Tag).BIPLink);
                }
                if (textBox.Equals(TextBoxAltButtonOff))
                {
                    _multiPanelPZ70.AddOrUpdateBIPLinkKnobBinding(MultiPanelPZ70Knobs.ALT_BUTTON, ((TagDataClassPZ70)textBox.Tag).BIPLink, false);
                }
                if (textBox.Equals(TextBoxVsButtonOn))
                {
                    _multiPanelPZ70.AddOrUpdateBIPLinkKnobBinding(MultiPanelPZ70Knobs.VS_BUTTON, ((TagDataClassPZ70)textBox.Tag).BIPLink);
                }
                if (textBox.Equals(TextBoxVsButtonOff))
                {
                    _multiPanelPZ70.AddOrUpdateBIPLinkKnobBinding(MultiPanelPZ70Knobs.VS_BUTTON, ((TagDataClassPZ70)textBox.Tag).BIPLink, false);
                }
                if (textBox.Equals(TextBoxAprButtonOn))
                {
                    _multiPanelPZ70.AddOrUpdateBIPLinkKnobBinding(MultiPanelPZ70Knobs.APR_BUTTON, ((TagDataClassPZ70)textBox.Tag).BIPLink);
                }
                if (textBox.Equals(TextBoxAprButtonOff))
                {
                    _multiPanelPZ70.AddOrUpdateBIPLinkKnobBinding(MultiPanelPZ70Knobs.APR_BUTTON, ((TagDataClassPZ70)textBox.Tag).BIPLink, false);
                }
                if (textBox.Equals(TextBoxRevButtonOn))
                {
                    _multiPanelPZ70.AddOrUpdateBIPLinkKnobBinding(MultiPanelPZ70Knobs.REV_BUTTON, ((TagDataClassPZ70)textBox.Tag).BIPLink);
                }
                if (textBox.Equals(TextBoxRevButtonOff))
                {
                    _multiPanelPZ70.AddOrUpdateBIPLinkKnobBinding(MultiPanelPZ70Knobs.REV_BUTTON, ((TagDataClassPZ70)textBox.Tag).BIPLink, false);
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(3011, ex);
            }
        }

        private void UpdateKeyBindingProfileSequencedKeyStrokesPZ70(TextBox textBox)
        {
            try
            {
                if (textBox.Equals(TextBoxLcdKnobDecrease))
                {
                    _multiPanelPZ70.AddOrUpdateSequencedKeyBinding(textBox.Text, MultiPanelPZ70Knobs.LCD_WHEEL_DEC, ((TagDataClassPZ70)textBox.Tag).GetKeySequence());
                }
                if (textBox.Equals(TextBoxLcdKnobIncrease))
                {
                    _multiPanelPZ70.AddOrUpdateSequencedKeyBinding(textBox.Text, MultiPanelPZ70Knobs.LCD_WHEEL_INC, ((TagDataClassPZ70)textBox.Tag).GetKeySequence());
                }
                if (textBox.Equals(TextBoxAutoThrottleOff))
                {
                    _multiPanelPZ70.AddOrUpdateSequencedKeyBinding(textBox.Text, MultiPanelPZ70Knobs.AUTO_THROTTLE, ((TagDataClassPZ70)textBox.Tag).GetKeySequence(), false);
                }
                if (textBox.Equals(TextBoxAutoThrottleOn))
                {
                    _multiPanelPZ70.AddOrUpdateSequencedKeyBinding(textBox.Text, MultiPanelPZ70Knobs.AUTO_THROTTLE, ((TagDataClassPZ70)textBox.Tag).GetKeySequence());
                }
                if (textBox.Equals(TextBoxFlapsUp))
                {
                    _multiPanelPZ70.AddOrUpdateSequencedKeyBinding(textBox.Text, MultiPanelPZ70Knobs.FLAPS_LEVER_UP, ((TagDataClassPZ70)textBox.Tag).GetKeySequence());
                }
                if (textBox.Equals(TextBoxFlapsDown))
                {
                    _multiPanelPZ70.AddOrUpdateSequencedKeyBinding(textBox.Text, MultiPanelPZ70Knobs.FLAPS_LEVER_DOWN, ((TagDataClassPZ70)textBox.Tag).GetKeySequence());
                }
                if (textBox.Equals(TextBoxPitchTrimUp))
                {
                    _multiPanelPZ70.AddOrUpdateSequencedKeyBinding(textBox.Text, MultiPanelPZ70Knobs.PITCH_TRIM_WHEEL_UP, ((TagDataClassPZ70)textBox.Tag).GetKeySequence());
                }
                if (textBox.Equals(TextBoxPitchTrimDown))
                {
                    _multiPanelPZ70.AddOrUpdateSequencedKeyBinding(textBox.Text, MultiPanelPZ70Knobs.PITCH_TRIM_WHEEL_DOWN, ((TagDataClassPZ70)textBox.Tag).GetKeySequence());
                }
                if (textBox.Equals(TextBoxApButtonOn))
                {
                    _multiPanelPZ70.AddOrUpdateSequencedKeyBinding(textBox.Text, MultiPanelPZ70Knobs.AP_BUTTON, ((TagDataClassPZ70)textBox.Tag).GetKeySequence());
                }
                if (textBox.Equals(TextBoxApButtonOff))
                {
                    _multiPanelPZ70.AddOrUpdateSequencedKeyBinding(textBox.Text, MultiPanelPZ70Knobs.AP_BUTTON, ((TagDataClassPZ70)textBox.Tag).GetKeySequence(), false);
                }
                if (textBox.Equals(TextBoxHdgButtonOn))
                {
                    _multiPanelPZ70.AddOrUpdateSequencedKeyBinding(textBox.Text, MultiPanelPZ70Knobs.HDG_BUTTON, ((TagDataClassPZ70)textBox.Tag).GetKeySequence());
                }
                if (textBox.Equals(TextBoxHdgButtonOff))
                {
                    _multiPanelPZ70.AddOrUpdateSequencedKeyBinding(textBox.Text, MultiPanelPZ70Knobs.HDG_BUTTON, ((TagDataClassPZ70)textBox.Tag).GetKeySequence(), false);
                }
                if (textBox.Equals(TextBoxNavButtonOn))
                {
                    _multiPanelPZ70.AddOrUpdateSequencedKeyBinding(textBox.Text, MultiPanelPZ70Knobs.NAV_BUTTON, ((TagDataClassPZ70)textBox.Tag).GetKeySequence());
                }
                if (textBox.Equals(TextBoxNavButtonOff))
                {
                    _multiPanelPZ70.AddOrUpdateSequencedKeyBinding(textBox.Text, MultiPanelPZ70Knobs.NAV_BUTTON, ((TagDataClassPZ70)textBox.Tag).GetKeySequence(), false);
                }
                if (textBox.Equals(TextBoxIasButtonOn))
                {
                    _multiPanelPZ70.AddOrUpdateSequencedKeyBinding(textBox.Text, MultiPanelPZ70Knobs.IAS_BUTTON, ((TagDataClassPZ70)textBox.Tag).GetKeySequence());
                }
                if (textBox.Equals(TextBoxIasButtonOff))
                {
                    _multiPanelPZ70.AddOrUpdateSequencedKeyBinding(textBox.Text, MultiPanelPZ70Knobs.IAS_BUTTON, ((TagDataClassPZ70)textBox.Tag).GetKeySequence(), false);
                }
                if (textBox.Equals(TextBoxAltButtonOn))
                {
                    _multiPanelPZ70.AddOrUpdateSequencedKeyBinding(textBox.Text, MultiPanelPZ70Knobs.ALT_BUTTON, ((TagDataClassPZ70)textBox.Tag).GetKeySequence());
                }
                if (textBox.Equals(TextBoxAltButtonOff))
                {
                    _multiPanelPZ70.AddOrUpdateSequencedKeyBinding(textBox.Text, MultiPanelPZ70Knobs.ALT_BUTTON, ((TagDataClassPZ70)textBox.Tag).GetKeySequence(), false);
                }
                if (textBox.Equals(TextBoxVsButtonOn))
                {
                    _multiPanelPZ70.AddOrUpdateSequencedKeyBinding(textBox.Text, MultiPanelPZ70Knobs.VS_BUTTON, ((TagDataClassPZ70)textBox.Tag).GetKeySequence());
                }
                if (textBox.Equals(TextBoxVsButtonOff))
                {
                    _multiPanelPZ70.AddOrUpdateSequencedKeyBinding(textBox.Text, MultiPanelPZ70Knobs.VS_BUTTON, ((TagDataClassPZ70)textBox.Tag).GetKeySequence(), false);
                }
                if (textBox.Equals(TextBoxAprButtonOn))
                {
                    _multiPanelPZ70.AddOrUpdateSequencedKeyBinding(textBox.Text, MultiPanelPZ70Knobs.APR_BUTTON, ((TagDataClassPZ70)textBox.Tag).GetKeySequence());
                }
                if (textBox.Equals(TextBoxAprButtonOff))
                {
                    _multiPanelPZ70.AddOrUpdateSequencedKeyBinding(textBox.Text, MultiPanelPZ70Knobs.APR_BUTTON, ((TagDataClassPZ70)textBox.Tag).GetKeySequence(), false);
                }
                if (textBox.Equals(TextBoxRevButtonOn))
                {
                    _multiPanelPZ70.AddOrUpdateSequencedKeyBinding(textBox.Text, MultiPanelPZ70Knobs.REV_BUTTON, ((TagDataClassPZ70)textBox.Tag).GetKeySequence());
                }
                if (textBox.Equals(TextBoxRevButtonOff))
                {
                    _multiPanelPZ70.AddOrUpdateSequencedKeyBinding(textBox.Text, MultiPanelPZ70Knobs.REV_BUTTON, ((TagDataClassPZ70)textBox.Tag).GetKeySequence(), false);
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
                if (!((TagDataClassPZ70)textBox.Tag).ContainsOSKeyPress() || ((TagDataClassPZ70)textBox.Tag).KeyPress.KeySequence.Count == 0)
                {
                    keyPressLength = KeyPressLength.FiftyMilliSec;
                }
                else
                {
                    keyPressLength = ((TagDataClassPZ70)textBox.Tag).KeyPress.GetLengthOfKeyPress();
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
                    return new MultiPanelPZ70KnobOnOff(_multiPanelPZ70.PZ70_DialPosition, MultiPanelPZ70Knobs.LCD_WHEEL_DEC, true);
                }
                if (textBox.Equals(TextBoxLcdKnobIncrease))
                {
                    return new MultiPanelPZ70KnobOnOff(_multiPanelPZ70.PZ70_DialPosition, MultiPanelPZ70Knobs.LCD_WHEEL_INC, true);
                }
                if (textBox.Equals(TextBoxAutoThrottleOff))
                {
                    return new MultiPanelPZ70KnobOnOff(_multiPanelPZ70.PZ70_DialPosition, MultiPanelPZ70Knobs.AUTO_THROTTLE, false);
                }
                if (textBox.Equals(TextBoxAutoThrottleOn))
                {
                    return new MultiPanelPZ70KnobOnOff(_multiPanelPZ70.PZ70_DialPosition, MultiPanelPZ70Knobs.AUTO_THROTTLE, true);
                }
                if (textBox.Equals(TextBoxFlapsUp))
                {
                    return new MultiPanelPZ70KnobOnOff(_multiPanelPZ70.PZ70_DialPosition, MultiPanelPZ70Knobs.FLAPS_LEVER_UP, true);
                }
                if (textBox.Equals(TextBoxFlapsDown))
                {
                    return new MultiPanelPZ70KnobOnOff(_multiPanelPZ70.PZ70_DialPosition, MultiPanelPZ70Knobs.FLAPS_LEVER_DOWN, false);
                }
                if (textBox.Equals(TextBoxPitchTrimUp))
                {
                    return new MultiPanelPZ70KnobOnOff(_multiPanelPZ70.PZ70_DialPosition, MultiPanelPZ70Knobs.PITCH_TRIM_WHEEL_UP, true);
                }
                if (textBox.Equals(TextBoxPitchTrimDown))
                {
                    return new MultiPanelPZ70KnobOnOff(_multiPanelPZ70.PZ70_DialPosition, MultiPanelPZ70Knobs.PITCH_TRIM_WHEEL_DOWN, false);
                }
                if (textBox.Equals(TextBoxApButtonOn))
                {
                    return new MultiPanelPZ70KnobOnOff(_multiPanelPZ70.PZ70_DialPosition, MultiPanelPZ70Knobs.AP_BUTTON, true);
                }
                if (textBox.Equals(TextBoxApButtonOff))
                {
                    return new MultiPanelPZ70KnobOnOff(_multiPanelPZ70.PZ70_DialPosition, MultiPanelPZ70Knobs.AP_BUTTON, false);
                }
                if (textBox.Equals(TextBoxHdgButtonOn))
                {
                    return new MultiPanelPZ70KnobOnOff(_multiPanelPZ70.PZ70_DialPosition, MultiPanelPZ70Knobs.HDG_BUTTON, true);
                }
                if (textBox.Equals(TextBoxHdgButtonOff))
                {
                    return new MultiPanelPZ70KnobOnOff(_multiPanelPZ70.PZ70_DialPosition, MultiPanelPZ70Knobs.HDG_BUTTON, false);
                }
                if (textBox.Equals(TextBoxNavButtonOn))
                {
                    return new MultiPanelPZ70KnobOnOff(_multiPanelPZ70.PZ70_DialPosition, MultiPanelPZ70Knobs.NAV_BUTTON, true);
                }
                if (textBox.Equals(TextBoxNavButtonOff))
                {
                    return new MultiPanelPZ70KnobOnOff(_multiPanelPZ70.PZ70_DialPosition, MultiPanelPZ70Knobs.NAV_BUTTON, false);
                }
                if (textBox.Equals(TextBoxIasButtonOn))
                {
                    return new MultiPanelPZ70KnobOnOff(_multiPanelPZ70.PZ70_DialPosition, MultiPanelPZ70Knobs.IAS_BUTTON, true);
                }
                if (textBox.Equals(TextBoxIasButtonOff))
                {
                    return new MultiPanelPZ70KnobOnOff(_multiPanelPZ70.PZ70_DialPosition, MultiPanelPZ70Knobs.IAS_BUTTON, false);
                }
                if (textBox.Equals(TextBoxAltButtonOn))
                {
                    return new MultiPanelPZ70KnobOnOff(_multiPanelPZ70.PZ70_DialPosition, MultiPanelPZ70Knobs.ALT_BUTTON, true);
                }
                if (textBox.Equals(TextBoxAltButtonOff))
                {
                    return new MultiPanelPZ70KnobOnOff(_multiPanelPZ70.PZ70_DialPosition, MultiPanelPZ70Knobs.ALT_BUTTON, false);
                }
                if (textBox.Equals(TextBoxVsButtonOn))
                {
                    return new MultiPanelPZ70KnobOnOff(_multiPanelPZ70.PZ70_DialPosition, MultiPanelPZ70Knobs.VS_BUTTON, true);
                }
                if (textBox.Equals(TextBoxVsButtonOff))
                {
                    return new MultiPanelPZ70KnobOnOff(_multiPanelPZ70.PZ70_DialPosition, MultiPanelPZ70Knobs.VS_BUTTON, false);
                }
                if (textBox.Equals(TextBoxAprButtonOn))
                {
                    return new MultiPanelPZ70KnobOnOff(_multiPanelPZ70.PZ70_DialPosition, MultiPanelPZ70Knobs.APR_BUTTON, true);
                }
                if (textBox.Equals(TextBoxAprButtonOff))
                {
                    return new MultiPanelPZ70KnobOnOff(_multiPanelPZ70.PZ70_DialPosition, MultiPanelPZ70Knobs.APR_BUTTON, false);
                }
                if (textBox.Equals(TextBoxRevButtonOn))
                {
                    return new MultiPanelPZ70KnobOnOff(_multiPanelPZ70.PZ70_DialPosition, MultiPanelPZ70Knobs.REV_BUTTON, true);
                }
                if (textBox.Equals(TextBoxRevButtonOff))
                {
                    return new MultiPanelPZ70KnobOnOff(_multiPanelPZ70.PZ70_DialPosition, MultiPanelPZ70Knobs.REV_BUTTON, false);
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
                if (((TagDataClassPZ70)textBox.Tag).ContainsDCSBIOS())
                {
                    dcsBiosInputs = ((TagDataClassPZ70)textBox.Tag).DCSBIOSInputs;
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
                if (textBox.Equals(TextBoxApButtonOff))
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
                Common.ShowErrorMessageBox(34501287, ex);
            }
        }

        private void RemoveContextMenuClickHandlers()
        {
            foreach (var textBox in Common.FindVisualChildren<TextBox>(this))
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
            foreach (var textBox in Common.FindVisualChildren<TextBox>(this))
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
                var textBox = (TextBox)sender;
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

                if (((TagDataClassPZ70)textBox.Tag).ContainsDCSBIOS())
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
                else if (((TagDataClassPZ70)textBox.Tag).ContainsKeySequence())
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
                else if (((TagDataClassPZ70)textBox.Tag).IsEmpty())
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
                        else
                        {
                            item.Visibility = Visibility.Collapsed;
                        }
                    }
                }
                else if (((TagDataClassPZ70)textBox.Tag).ContainsSingleKey())
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
                else if (((TagDataClassPZ70)textBox.Tag).ContainsBIPLink())
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

                    if (_multiPanelPZ70.ForwardPanelEvent)
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
                             TextBoxLogPZ70.Text = TextBoxLogPZ70.Text.Insert(0, "No action taken, panel events Disabled.\n")));
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
                Common.ShowErrorMessageBox(7365005, ex);
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
                Common.ShowErrorMessageBox(7265005, ex);
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




        private void TextBox_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                ((TextBox)sender).Background = Brushes.Yellow;
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(3004, ex);
            }
        }

    }
}
