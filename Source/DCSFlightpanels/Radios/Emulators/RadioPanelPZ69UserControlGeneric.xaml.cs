﻿namespace DCSFlightpanels.Radios.Emulators
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
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
    using NonVisuals.Radios;
    using NonVisuals.Radios.Knobs;
    using NonVisuals.Panels.Saitek;
    using NonVisuals.Panels;
    using NonVisuals.HID;

    /// <summary>
    /// GUI wrapper for the RadioPanelPZ69Generic. 
    /// <see cref="RadioPanelPZ69Generic"/>
    /// </summary>
    public partial class RadioPanelPZ69UserControlGeneric : IGamingPanelListener, IProfileHandlerListener, IGamingPanelUserControl, IPanelUI
    {
        private const string UPPER_TEXT = "Upper Dial Profile : ";
        private const string LOWER_TEXT = "Lower Dial Profile : ";
        private readonly List<Key> _allowedKeys = new() { Key.D0, Key.D1, Key.D2, Key.D3, Key.D4, Key.D5, Key.D6, Key.D7, Key.D8, Key.D9, Key.OemPeriod, Key.Delete, Key.Back, Key.Left, Key.Right, Key.NumPad0, Key.NumPad1, Key.NumPad2, Key.NumPad3, Key.NumPad4, Key.NumPad5, Key.NumPad6, Key.NumPad7, Key.NumPad8, Key.NumPad9 };
        private readonly RadioPanelPZ69Generic _radioPanelPZ69;

        public RadioPanelPZ69UserControlGeneric(HIDSkeleton hidSkeleton)
        {
            InitializeComponent();
            
            HideAllImages();

            _radioPanelPZ69 = new RadioPanelPZ69Generic(hidSkeleton)
            {
                FrequencyKnobSensitivity = Settings.Default.RadioFrequencyKnobSensitivity
            };

            AppEventHandler.AttachGamingPanelListener(this);
        }

        public NumberFormatInfo NumberFormatInfoFullDisplay
        {
            get => _radioPanelPZ69.NumberFormatInfoFullDisplay;
        }

        private bool _disposed;
        // Protected implementation of Dispose pattern.
        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _radioPanelPZ69.Dispose();
                    AppEventHandler.DetachGamingPanelListener(this);
                }

                _disposed = true;
            }

            // Call base class implementation.
            base.Dispose(disposing);
        }

        public override void Init()
        {
            try
            {
                _radioPanelPZ69.InitPanel();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void RadioPanelPZ69UserControlGeneric_OnLoaded(object sender, RoutedEventArgs e)
        {
            if (!UserControlLoaded || !TextBoxBillsSet)
            {
                DarkMode.SetFrameworkElementDarkMode(this);
                ComboBoxFreqKnobSensitivity.SelectedValue = Settings.Default.RadioFrequencyKnobSensitivityEmulator;
                SetTextBoxBills();
                SetButtonBills();
                UserControlLoaded = true;
            }
            ShowGraphicConfiguration();
        }
        
        public override GamingPanel GetGamingPanel()
        {
            return _radioPanelPZ69;
        }

        public override GamingPanelEnum GetPanelType()
        {
            return GamingPanelEnum.PZ69RadioPanel;
        }

        public string GetName()
        {
            return GetType().Name;
        }

        public void UpdatesHasBeenMissed(object sender, DCSBIOSUpdatesMissedEventArgs e) { }
        
        public void SwitchesChanged(object sender, SwitchesChangedEventArgs e)
        {
            try
            {
                if (e.PanelType == GamingPanelEnum.PZ69RadioPanel && e.HidInstance.Equals(_radioPanelPZ69.HIDInstance))
                {
                    NotifySwitchChanges(e.Switches);
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        public void ProfileEvent(object sender, ProfileEventArgs e) { }
        
        public void SettingsApplied(object sender, PanelInfoArgs e)
        {
            try
            {
                if (e.HidInstance.Equals(_radioPanelPZ69.HIDInstance) && e.PanelType == GamingPanelEnum.PZ69RadioPanel)
                {
                    Dispatcher?.BeginInvoke((Action)ShowGraphicConfiguration);
                    Dispatcher?.BeginInvoke((Action)(() => TextBoxLogPZ69.Text = string.Empty));
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        public void SettingsModified(object sender, PanelInfoArgs e)
        {
            try
            {
                if (_radioPanelPZ69.HIDInstance.Equals(e.HidInstance))
                {
                    Dispatcher?.BeginInvoke((Action)ShowGraphicConfiguration);
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
                TextBoxLogPZ69.Focus();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void SetTextBoxBills()
        {
            if (TextBoxBillsSet || !Common.FindVisualChildren<PZ69GenericTextBox>(this).Any()) return;

            foreach (var textBox in Common.FindVisualChildren<PZ69GenericTextBox>(this))
            {
                if (textBox.Bill != null || textBox.Equals(TextBoxLogPZ69))
                {
                    continue;
                }
                textBox.Bill = new BillPZ69Generic(this, _radioPanelPZ69, textBox);
            }
            TextBoxBillsSet = true;
        }

        private void SetButtonBills()
        {
            if (ButtonBillsSet) return;

            ButtonUpperLeftLcd.Bill = new BillPZ69LCDButton(ButtonUpperLeftLcd);
            ButtonLowerLeftLcd.Bill = new BillPZ69LCDButton(ButtonLowerLeftLcd);
            ButtonUpperRightLcd.Bill = new BillPZ69LCDButton(ButtonUpperRightLcd);
            ButtonLowerRightLcd.Bill = new BillPZ69LCDButton(ButtonLowerRightLcd);
            ButtonBillsSet = true;
        }
        
        private void UpdateDisplayValues(TextBox textBox)
        {
            try
            {
                if (textBox.Equals(TextBoxUpperCom1ActiveNumbers))
                {
                    _radioPanelPZ69.AddOrUpdateDisplayValue(RadioPanelPZ69KnobsEmulator.UpperCOM1, textBox.Text, RadioPanelPZ69Display.UpperActive);
                }
                if (textBox.Equals(TextBoxUpperCom1StandbyNumbers))
                {
                    _radioPanelPZ69.AddOrUpdateDisplayValue(RadioPanelPZ69KnobsEmulator.UpperCOM1, textBox.Text, RadioPanelPZ69Display.UpperStandby);
                }
                if (textBox.Equals(TextBoxUpperCom2ActiveNumbers))
                {
                    _radioPanelPZ69.AddOrUpdateDisplayValue(RadioPanelPZ69KnobsEmulator.UpperCOM2, textBox.Text, RadioPanelPZ69Display.UpperActive);
                }
                if (textBox.Equals(TextBoxUpperCom2StandbyNumbers))
                {
                    _radioPanelPZ69.AddOrUpdateDisplayValue(RadioPanelPZ69KnobsEmulator.UpperCOM2, textBox.Text, RadioPanelPZ69Display.UpperStandby);
                }
                if (textBox.Equals(TextBoxUpperNav1ActiveNumbers))
                {
                    _radioPanelPZ69.AddOrUpdateDisplayValue(RadioPanelPZ69KnobsEmulator.UpperNAV1, textBox.Text, RadioPanelPZ69Display.UpperActive);
                }
                if (textBox.Equals(TextBoxUpperNav1StandbyNumbers))
                {
                    _radioPanelPZ69.AddOrUpdateDisplayValue(RadioPanelPZ69KnobsEmulator.UpperNAV1, textBox.Text, RadioPanelPZ69Display.UpperStandby);
                }
                if (textBox.Equals(TextBoxUpperNav2ActiveNumbers))
                {
                    _radioPanelPZ69.AddOrUpdateDisplayValue(RadioPanelPZ69KnobsEmulator.UpperNAV2, textBox.Text, RadioPanelPZ69Display.UpperActive);
                }
                if (textBox.Equals(TextBoxUpperNav2StandbyNumbers))
                {
                    _radioPanelPZ69.AddOrUpdateDisplayValue(RadioPanelPZ69KnobsEmulator.UpperNAV2, textBox.Text, RadioPanelPZ69Display.UpperStandby);
                }
                if (textBox.Equals(TextBoxUpperADFActiveNumbers))
                {
                    _radioPanelPZ69.AddOrUpdateDisplayValue(RadioPanelPZ69KnobsEmulator.UpperADF, textBox.Text, RadioPanelPZ69Display.UpperActive);
                }
                if (textBox.Equals(TextBoxUpperADFStandbyNumbers))
                {
                    _radioPanelPZ69.AddOrUpdateDisplayValue(RadioPanelPZ69KnobsEmulator.UpperADF, textBox.Text, RadioPanelPZ69Display.UpperStandby);
                }
                if (textBox.Equals(TextBoxUpperDMEActiveNumbers))
                {
                    _radioPanelPZ69.AddOrUpdateDisplayValue(RadioPanelPZ69KnobsEmulator.UpperDME, textBox.Text, RadioPanelPZ69Display.UpperActive);
                }
                if (textBox.Equals(TextBoxUpperDMEStandbyNumbers))
                {
                    _radioPanelPZ69.AddOrUpdateDisplayValue(RadioPanelPZ69KnobsEmulator.UpperDME, textBox.Text, RadioPanelPZ69Display.UpperStandby);
                }
                if (textBox.Equals(TextBoxUpperXPDRActiveNumbers))
                {
                    _radioPanelPZ69.AddOrUpdateDisplayValue(RadioPanelPZ69KnobsEmulator.UpperXPDR, textBox.Text, RadioPanelPZ69Display.UpperActive);
                }
                if (textBox.Equals(TextBoxUpperXPDRStandbyNumbers))
                {
                    _radioPanelPZ69.AddOrUpdateDisplayValue(RadioPanelPZ69KnobsEmulator.UpperXPDR, textBox.Text, RadioPanelPZ69Display.UpperStandby);
                }

                if (textBox.Equals(TextBoxLowerCom1ActiveNumbers))
                {
                    _radioPanelPZ69.AddOrUpdateDisplayValue(RadioPanelPZ69KnobsEmulator.LowerCOM1, textBox.Text, RadioPanelPZ69Display.LowerActive);
                }
                if (textBox.Equals(TextBoxLowerCom1StandbyNumbers))
                {
                    _radioPanelPZ69.AddOrUpdateDisplayValue(RadioPanelPZ69KnobsEmulator.LowerCOM1, textBox.Text, RadioPanelPZ69Display.LowerStandby);
                }
                if (textBox.Equals(TextBoxLowerCom2ActiveNumbers))
                {
                    _radioPanelPZ69.AddOrUpdateDisplayValue(RadioPanelPZ69KnobsEmulator.LowerCOM2, textBox.Text, RadioPanelPZ69Display.LowerActive);
                }
                if (textBox.Equals(TextBoxLowerCom2StandbyNumbers))
                {
                    _radioPanelPZ69.AddOrUpdateDisplayValue(RadioPanelPZ69KnobsEmulator.LowerCOM2, textBox.Text, RadioPanelPZ69Display.LowerStandby);
                }
                if (textBox.Equals(TextBoxLowerNav1ActiveNumbers))
                {
                    _radioPanelPZ69.AddOrUpdateDisplayValue(RadioPanelPZ69KnobsEmulator.LowerNAV1, textBox.Text, RadioPanelPZ69Display.LowerActive);
                }
                if (textBox.Equals(TextBoxLowerNav1StandbyNumbers))
                {
                    _radioPanelPZ69.AddOrUpdateDisplayValue(RadioPanelPZ69KnobsEmulator.LowerNAV1, textBox.Text, RadioPanelPZ69Display.LowerStandby);
                }
                if (textBox.Equals(TextBoxLowerNav2ActiveNumbers))
                {
                    _radioPanelPZ69.AddOrUpdateDisplayValue(RadioPanelPZ69KnobsEmulator.LowerNAV2, textBox.Text, RadioPanelPZ69Display.LowerActive);
                }
                if (textBox.Equals(TextBoxLowerNav2StandbyNumbers))
                {
                    _radioPanelPZ69.AddOrUpdateDisplayValue(RadioPanelPZ69KnobsEmulator.LowerNAV2, textBox.Text, RadioPanelPZ69Display.LowerStandby);
                }
                if (textBox.Equals(TextBoxLowerADFActiveNumbers))
                {
                    _radioPanelPZ69.AddOrUpdateDisplayValue(RadioPanelPZ69KnobsEmulator.LowerADF, textBox.Text, RadioPanelPZ69Display.LowerActive);
                }
                if (textBox.Equals(TextBoxLowerADFStandbyNumbers))
                {
                    _radioPanelPZ69.AddOrUpdateDisplayValue(RadioPanelPZ69KnobsEmulator.LowerADF, textBox.Text, RadioPanelPZ69Display.LowerStandby);
                }
                if (textBox.Equals(TextBoxLowerDMEActiveNumbers))
                {
                    _radioPanelPZ69.AddOrUpdateDisplayValue(RadioPanelPZ69KnobsEmulator.LowerDME, textBox.Text, RadioPanelPZ69Display.LowerActive);
                }
                if (textBox.Equals(TextBoxLowerDMEStandbyNumbers))
                {
                    _radioPanelPZ69.AddOrUpdateDisplayValue(RadioPanelPZ69KnobsEmulator.LowerDME, textBox.Text, RadioPanelPZ69Display.LowerStandby);
                }
                if (textBox.Equals(TextBoxLowerXPDRActiveNumbers))
                {
                    _radioPanelPZ69.AddOrUpdateDisplayValue(RadioPanelPZ69KnobsEmulator.LowerXPDR, textBox.Text, RadioPanelPZ69Display.LowerActive);
                }
                if (textBox.Equals(TextBoxLowerXPDRStandbyNumbers))
                {
                    _radioPanelPZ69.AddOrUpdateDisplayValue(RadioPanelPZ69KnobsEmulator.LowerXPDR, textBox.Text, RadioPanelPZ69Display.LowerStandby);
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void ClearAll(bool clearAlsoProfile)
        {
            foreach (var textBox in Common.FindVisualChildren<PZ69GenericTextBox>(this))
            {
                if (textBox.Equals(TextBoxLogPZ69) || textBox.Bill == null)
                {
                    continue;
                }

                textBox.Bill.ClearAll();
            }

            if (clearAlsoProfile)
            {
                _radioPanelPZ69.ClearSettings(true);
            }

            ShowGraphicConfiguration();
        }

        private void ClearAllDisplayValues()
        {
            foreach (var textBox in Common.FindVisualChildren<TextBox>(this))
            {
                if (textBox.Name.EndsWith("Numbers"))
                {
                    textBox.Text = string.Empty;
                }
            }
        }
        
        /* ------------------------------------------------------------------------------------------------------------------------------------------------------------
         * ------------------------------------------------------------------------------------------------------------------------------------------------------------
         * ------------------------------------------------------------------------------------------------------------------------------------------------------------
         * ------------------------------------------------------------------------------------------------------------------------------------------------------------
         * ------------------------------------------------------------------------------------------------------------------------------------------------------------
         * ------------------------------------------------------------------------------------------------------------------------------------------------------------
         */
        private void TextBoxPreviewKeyDownNumbers(object sender, KeyEventArgs e)
        {
            try
            {
                var textBox = (TextBox)sender;

                if (textBox.Text.Contains('.'))
                {
                    textBox.MaxLength = 6;
                }
                else
                {
                    textBox.MaxLength = 5;
                }
                if (!_allowedKeys.Contains(e.Key))
                {
                    //Only digits and period allowed
                    e.Handled = true;
                    return;
                }
                if (textBox.Text.Contains('.') && e.Key == Key.OemPeriod)
                {
                    e.Handled = true;
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void TextBoxNumbers_OnKeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                var textBox = (TextBox)sender;
                UpdateDisplayValues(textBox);
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }


        /* ------------------------------------------------------------------------------------------------------------------------------------------------------------
         * ------------------------------------------------------------------------------------------------------------------------------------------------------------
         * ------------------------------------------------------------------------------------------------------------------------------------------------------------
         * ------------------------------------------------------------------------------------------------------------------------------------------------------------
         * ------------------------------------------------------------------------------------------------------------------------------------------------------------
         * ------------------------------------------------------------------------------------------------------------------------------------------------------------
         */
         
        private void NotifySwitchChanges(HashSet<object> switches)
        {
            try
            {
                //Set focus to this so that virtual keypresses won't affect settings
                Dispatcher?.BeginInvoke((Action)(() => TextBoxLogPZ69.Focus()));
                foreach (var radioPanelKey in switches)
                {
                    var key = (RadioPanelPZ69KnobEmulator)radioPanelKey;

                    if (_radioPanelPZ69.ForwardPanelEvent)
                    {
                        if (!string.IsNullOrEmpty(_radioPanelPZ69.GetKeyPressForLoggingPurposes(key)))
                        {
                            Dispatcher?.BeginInvoke(
                                (Action)
                                (() =>
                                 TextBoxLogPZ69.Text =
                                 TextBoxLogPZ69.Text.Insert(0, _radioPanelPZ69.GetKeyPressForLoggingPurposes(key) + "\n")));
                        }
                    }
                    else
                    {
                        Dispatcher?.BeginInvoke(
                            (Action)
                            (() =>
                             TextBoxLogPZ69.Text =
                             TextBoxLogPZ69.Text = TextBoxLogPZ69.Text.Insert(0, "No action taken, panel events Disabled.\n")));
                    }
                }
                SetGraphicsState(switches);
            }
            catch (Exception ex)
            {
                Dispatcher?.BeginInvoke(
                    (Action)
                    (() =>
                     TextBoxLogPZ69.Text = TextBoxLogPZ69.Text.Insert(0, "0x16" + ex.Message + ".\n")));
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void ShowGraphicConfiguration()
        {
            try
            {
                if (!UserControlLoaded || !TextBoxBillsSet)
                {
                    return;
                }
                HideButtonImages();

                ClearAllDisplayValues();

                foreach (var displayValue in _radioPanelPZ69.DisplayValueHashSet)
                {

                    if (displayValue.RadioPanelPZ69Knob == RadioPanelPZ69KnobsEmulator.UpperCOM1)
                    {
                        if (displayValue.RadioPanelDisplay == RadioPanelPZ69Display.UpperActive)
                        {
                            TextBoxUpperCom1ActiveNumbers.Text =
                                displayValue.Value.ToString(NumberFormatInfoFullDisplay);
                        }

                        if (displayValue.RadioPanelDisplay == RadioPanelPZ69Display.UpperStandby)
                        {
                            TextBoxUpperCom1StandbyNumbers.Text =
                                displayValue.Value.ToString(NumberFormatInfoFullDisplay);
                        }
                    }
                    else if (displayValue.RadioPanelPZ69Knob == RadioPanelPZ69KnobsEmulator.UpperCOM2)
                    {
                        if (displayValue.RadioPanelDisplay == RadioPanelPZ69Display.UpperActive)
                        {
                            TextBoxUpperCom2ActiveNumbers.Text =
                                displayValue.Value.ToString(NumberFormatInfoFullDisplay);
                        }

                        if (displayValue.RadioPanelDisplay == RadioPanelPZ69Display.UpperStandby)
                        {
                            TextBoxUpperCom2StandbyNumbers.Text =
                                displayValue.Value.ToString(NumberFormatInfoFullDisplay);
                        }
                    }
                    else if (displayValue.RadioPanelPZ69Knob == RadioPanelPZ69KnobsEmulator.UpperNAV1)
                    {
                        if (displayValue.RadioPanelDisplay == RadioPanelPZ69Display.UpperActive)
                        {
                            TextBoxUpperNav1ActiveNumbers.Text =
                                displayValue.Value.ToString(NumberFormatInfoFullDisplay);
                        }

                        if (displayValue.RadioPanelDisplay == RadioPanelPZ69Display.UpperStandby)
                        {
                            TextBoxUpperNav1StandbyNumbers.Text =
                                displayValue.Value.ToString(NumberFormatInfoFullDisplay);
                        }
                    }
                    else if (displayValue.RadioPanelPZ69Knob == RadioPanelPZ69KnobsEmulator.UpperNAV2)
                    {
                        if (displayValue.RadioPanelDisplay == RadioPanelPZ69Display.UpperActive)
                        {
                            TextBoxUpperNav2ActiveNumbers.Text =
                                displayValue.Value.ToString(NumberFormatInfoFullDisplay);
                        }

                        if (displayValue.RadioPanelDisplay == RadioPanelPZ69Display.UpperStandby)
                        {
                            TextBoxUpperNav2StandbyNumbers.Text =
                                displayValue.Value.ToString(NumberFormatInfoFullDisplay);
                        }
                    }
                    else if (displayValue.RadioPanelPZ69Knob == RadioPanelPZ69KnobsEmulator.UpperADF)
                    {
                        if (displayValue.RadioPanelDisplay == RadioPanelPZ69Display.UpperActive)
                        {
                            TextBoxUpperADFActiveNumbers.Text =
                                displayValue.Value.ToString(NumberFormatInfoFullDisplay);
                        }

                        if (displayValue.RadioPanelDisplay == RadioPanelPZ69Display.UpperStandby)
                        {
                            TextBoxUpperADFStandbyNumbers.Text =
                                displayValue.Value.ToString(NumberFormatInfoFullDisplay);
                        }
                    }
                    else if (displayValue.RadioPanelPZ69Knob == RadioPanelPZ69KnobsEmulator.UpperDME)
                    {
                        if (displayValue.RadioPanelDisplay == RadioPanelPZ69Display.UpperActive)
                        {
                            TextBoxUpperDMEActiveNumbers.Text =
                                displayValue.Value.ToString(NumberFormatInfoFullDisplay);
                        }

                        if (displayValue.RadioPanelDisplay == RadioPanelPZ69Display.UpperStandby)
                        {
                            TextBoxUpperDMEStandbyNumbers.Text =
                                displayValue.Value.ToString(NumberFormatInfoFullDisplay);
                        }
                    }
                    else if (displayValue.RadioPanelPZ69Knob == RadioPanelPZ69KnobsEmulator.UpperXPDR)
                    {
                        if (displayValue.RadioPanelDisplay == RadioPanelPZ69Display.UpperActive)
                        {
                            TextBoxUpperXPDRActiveNumbers.Text =
                                displayValue.Value.ToString(NumberFormatInfoFullDisplay);
                        }

                        if (displayValue.RadioPanelDisplay == RadioPanelPZ69Display.UpperStandby)
                        {
                            TextBoxUpperXPDRStandbyNumbers.Text =
                                displayValue.Value.ToString(NumberFormatInfoFullDisplay);
                        }
                    }
                    else if (displayValue.RadioPanelPZ69Knob == RadioPanelPZ69KnobsEmulator.LowerCOM1)
                    {
                        if (displayValue.RadioPanelDisplay == RadioPanelPZ69Display.LowerActive)
                        {
                            TextBoxLowerCom1ActiveNumbers.Text =
                                displayValue.Value.ToString(NumberFormatInfoFullDisplay);
                        }

                        if (displayValue.RadioPanelDisplay == RadioPanelPZ69Display.LowerStandby)
                        {
                            TextBoxLowerCom1StandbyNumbers.Text =
                                displayValue.Value.ToString(NumberFormatInfoFullDisplay);
                        }
                    }
                    else if (displayValue.RadioPanelPZ69Knob == RadioPanelPZ69KnobsEmulator.LowerCOM2)
                    {
                        if (displayValue.RadioPanelDisplay == RadioPanelPZ69Display.LowerActive)
                        {
                            TextBoxLowerCom2ActiveNumbers.Text =
                                displayValue.Value.ToString(NumberFormatInfoFullDisplay);
                        }

                        if (displayValue.RadioPanelDisplay == RadioPanelPZ69Display.LowerStandby)
                        {
                            TextBoxLowerCom2StandbyNumbers.Text =
                                displayValue.Value.ToString(NumberFormatInfoFullDisplay);
                        }
                    }
                    else if (displayValue.RadioPanelPZ69Knob == RadioPanelPZ69KnobsEmulator.LowerNAV1)
                    {
                        if (displayValue.RadioPanelDisplay == RadioPanelPZ69Display.LowerActive)
                        {
                            TextBoxLowerNav1ActiveNumbers.Text =
                                displayValue.Value.ToString(NumberFormatInfoFullDisplay);
                        }

                        if (displayValue.RadioPanelDisplay == RadioPanelPZ69Display.LowerStandby)
                        {
                            TextBoxLowerNav1StandbyNumbers.Text =
                                displayValue.Value.ToString(NumberFormatInfoFullDisplay);
                        }
                    }
                    else if (displayValue.RadioPanelPZ69Knob == RadioPanelPZ69KnobsEmulator.LowerNAV2)
                    {
                        if (displayValue.RadioPanelDisplay == RadioPanelPZ69Display.LowerActive)
                        {
                            TextBoxLowerNav2ActiveNumbers.Text =
                                displayValue.Value.ToString(NumberFormatInfoFullDisplay);
                        }

                        if (displayValue.RadioPanelDisplay == RadioPanelPZ69Display.LowerStandby)
                        {
                            TextBoxLowerNav2StandbyNumbers.Text =
                                displayValue.Value.ToString(NumberFormatInfoFullDisplay);
                        }
                    }
                    else if (displayValue.RadioPanelPZ69Knob == RadioPanelPZ69KnobsEmulator.LowerADF)
                    {
                        if (displayValue.RadioPanelDisplay == RadioPanelPZ69Display.LowerActive)
                        {
                            TextBoxLowerADFActiveNumbers.Text =
                                displayValue.Value.ToString(NumberFormatInfoFullDisplay);
                        }

                        if (displayValue.RadioPanelDisplay == RadioPanelPZ69Display.LowerStandby)
                        {
                            TextBoxLowerADFStandbyNumbers.Text =
                                displayValue.Value.ToString(NumberFormatInfoFullDisplay);
                        }
                    }
                    else if (displayValue.RadioPanelPZ69Knob == RadioPanelPZ69KnobsEmulator.LowerDME)
                    {
                        if (displayValue.RadioPanelDisplay == RadioPanelPZ69Display.LowerActive)
                        {
                            TextBoxLowerDMEActiveNumbers.Text =
                                displayValue.Value.ToString(NumberFormatInfoFullDisplay);
                        }

                        if (displayValue.RadioPanelDisplay == RadioPanelPZ69Display.LowerStandby)
                        {
                            TextBoxLowerDMEStandbyNumbers.Text =
                                displayValue.Value.ToString(NumberFormatInfoFullDisplay);
                        }
                    }
                    else if (displayValue.RadioPanelPZ69Knob == RadioPanelPZ69KnobsEmulator.LowerXPDR)
                    {
                        if (displayValue.RadioPanelDisplay == RadioPanelPZ69Display.LowerActive)
                        {
                            TextBoxLowerXPDRActiveNumbers.Text =
                                displayValue.Value.ToString(NumberFormatInfoFullDisplay);
                        }

                        if (displayValue.RadioPanelDisplay == RadioPanelPZ69Display.LowerStandby)
                        {
                            TextBoxLowerXPDRStandbyNumbers.Text =
                                displayValue.Value.ToString(NumberFormatInfoFullDisplay);
                        }
                    }
                }

                foreach (var keyBinding in _radioPanelPZ69.KeyBindingsHashSet)
                {
                    var textBox = (PZ69GenericTextBox)GetTextBox(keyBinding.RadioPanelPZ69Key, keyBinding.WhenTurnedOn);
                    if (keyBinding.OSKeyPress != null && (keyBinding.DialPosition == _radioPanelPZ69.PZ69UpperDialPosition || keyBinding.DialPosition == _radioPanelPZ69.PZ69LowerDialPosition))
                    {
                        textBox.Bill.KeyPress = keyBinding.OSKeyPress;
                    }
                }

                foreach (var operatingSystemCommand in _radioPanelPZ69.OSCommandHashSet)
                {
                    var textBox = (PZ69GenericTextBox)GetTextBox(operatingSystemCommand.RadioPanelPZ69Key, operatingSystemCommand.WhenTurnedOn);
                    if (operatingSystemCommand.OSCommandObject != null && (operatingSystemCommand.DialPosition == _radioPanelPZ69.PZ69UpperDialPosition || operatingSystemCommand.DialPosition == _radioPanelPZ69.PZ69LowerDialPosition))
                        if (operatingSystemCommand.OSCommandObject != null)
                        {
                            textBox.Bill.OSCommandObject = operatingSystemCommand.OSCommandObject;
                        }
                }

                foreach (var bipLinkPZ69 in _radioPanelPZ69.BipLinkHashSet)
                {
                    var textBox = (PZ69GenericTextBox)GetTextBox(bipLinkPZ69.RadioPanelPZ69Knob, bipLinkPZ69.WhenTurnedOn);
                    textBox.Bill.BipLink = bipLinkPZ69;
                }

                foreach (var lcdBinding in _radioPanelPZ69.LCDBindings)
                {
                    if (!lcdBinding.HasBinding)
                    {
                        continue;
                    }

                    if (lcdBinding.DialPosition == _radioPanelPZ69.PZ69UpperDialPosition)
                    {
                        if (lcdBinding.PZ69LcdPosition == PZ69LCDPosition.UPPER_ACTIVE_LEFT)
                        {
                            ButtonUpperLeftLcd.Bill.DCSBIOSBindingLCD = lcdBinding;
                            DotTopLeftLcd.Visibility = Visibility.Visible;
                        }

                        if (lcdBinding.PZ69LcdPosition == PZ69LCDPosition.UPPER_STBY_RIGHT)
                        {
                            ButtonUpperRightLcd.Bill.DCSBIOSBindingLCD = lcdBinding;
                            DotTopRightLcd.Visibility = Visibility.Visible;
                        }

                    }
                    else if (lcdBinding.DialPosition == _radioPanelPZ69.PZ69LowerDialPosition)
                    {
                        if (lcdBinding.PZ69LcdPosition == PZ69LCDPosition.LOWER_ACTIVE_LEFT)
                        {
                            ButtonLowerLeftLcd.Bill.DCSBIOSBindingLCD = lcdBinding;
                            DotBottomLeftLcd.Visibility = Visibility.Visible;
                        }

                        if (lcdBinding.PZ69LcdPosition == PZ69LCDPosition.LOWER_STBY_RIGHT)
                        {
                            ButtonLowerRightLcd.Bill.DCSBIOSBindingLCD = lcdBinding;
                            DotBottomRightLcd.Visibility = Visibility.Visible;
                        }
                    }
                }

                foreach (var dcsbiosBinding in _radioPanelPZ69.DCSBIOSBindings)
                {
                    if (!dcsbiosBinding.HasBinding())
                    {
                        continue;
                    }

                    var textBox = (PZ69GenericTextBox)GetTextBox(dcsbiosBinding.RadioPanelPZ69Knob, dcsbiosBinding.WhenTurnedOn);

                    if (dcsbiosBinding.DialPosition == _radioPanelPZ69.PZ69UpperDialPosition || dcsbiosBinding.DialPosition == _radioPanelPZ69.PZ69LowerDialPosition)
                    {
                        textBox.Bill.DCSBIOSBinding = dcsbiosBinding;
                    }
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
                foreach (var radioKnobO in knobs)
                {
                    var radioKnob = (RadioPanelPZ69KnobEmulator)radioKnobO;
                    switch (radioKnob.RadioPanelPZ69Knob)
                    {
                        case RadioPanelPZ69KnobsEmulator.UpperCOM1:
                            {
                                var key = radioKnob;
                                Dispatcher?.BeginInvoke(
                                    (Action)delegate
                                    {
                                        TopLeftCom1.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                        if (key.IsOn)
                                        {
                                            ClearAll(false);
                                            ShowGraphicConfiguration();
                                            LabelDialPosUpper.Content = UPPER_TEXT + "COM1";
                                        }
                                    });
                                break;
                            }
                        case RadioPanelPZ69KnobsEmulator.UpperCOM2:
                            {
                                var key = radioKnob;
                                Dispatcher?.BeginInvoke(
                                    (Action)delegate
                                    {
                                        TopLeftCom2.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                        if (key.IsOn)
                                        {
                                            ClearAll(false);
                                            ShowGraphicConfiguration();
                                            LabelDialPosUpper.Content = UPPER_TEXT + "COM2";
                                        }
                                    });
                                break;
                            }
                        case RadioPanelPZ69KnobsEmulator.UpperNAV1:
                            {
                                var key = radioKnob;
                                Dispatcher?.BeginInvoke(
                                    (Action)delegate
                                    {
                                        TopLeftNav1.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                        if (key.IsOn)
                                        {
                                            ClearAll(false);
                                            ShowGraphicConfiguration();
                                            LabelDialPosUpper.Content = UPPER_TEXT + "NAV1";
                                        }
                                    });
                                break;
                            }
                        case RadioPanelPZ69KnobsEmulator.UpperNAV2:
                            {
                                var key = radioKnob;
                                Dispatcher?.BeginInvoke(
                                    (Action)delegate
                                    {
                                        TopLeftNav2.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                        if (key.IsOn)
                                        {
                                            ClearAll(false);
                                            ShowGraphicConfiguration();
                                            LabelDialPosUpper.Content = UPPER_TEXT + "NAV2";
                                        }
                                    });
                                break;
                            }
                        case RadioPanelPZ69KnobsEmulator.UpperADF:
                            {
                                var key = radioKnob;
                                Dispatcher?.BeginInvoke(
                                    (Action)delegate
                                    {
                                        TopLeftADF.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                        if (key.IsOn)
                                        {
                                            ClearAll(false);
                                            ShowGraphicConfiguration();
                                            LabelDialPosUpper.Content = UPPER_TEXT + "ADF";
                                        }
                                    });
                                break;
                            }
                        case RadioPanelPZ69KnobsEmulator.UpperDME:
                            {
                                var key = radioKnob;
                                Dispatcher?.BeginInvoke(
                                    (Action)delegate
                                    {
                                        TopLeftDME.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                        if (key.IsOn)
                                        {
                                            ClearAll(false);
                                            ShowGraphicConfiguration();
                                            LabelDialPosUpper.Content = UPPER_TEXT + "DME";
                                        }
                                    });
                                break;
                            }
                        case RadioPanelPZ69KnobsEmulator.UpperXPDR:
                            {
                                var key = radioKnob;
                                Dispatcher?.BeginInvoke(
                                    (Action)delegate
                                    {
                                        TopLeftXPDR.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                        if (key.IsOn)
                                        {
                                            ClearAll(false);
                                            ShowGraphicConfiguration();
                                            LabelDialPosUpper.Content = UPPER_TEXT + "XPDR";
                                        }
                                    });
                                break;
                            }
                        case RadioPanelPZ69KnobsEmulator.LowerCOM1:
                            {
                                var key = radioKnob;
                                Dispatcher?.BeginInvoke(
                                    (Action)delegate
                                    {
                                        LowerLeftCom1.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                        if (key.IsOn)
                                        {
                                            ClearAll(false);
                                            ShowGraphicConfiguration();
                                            LabelDialPosLower.Content = LOWER_TEXT + "COM1";
                                        }
                                    });
                                break;
                            }
                        case RadioPanelPZ69KnobsEmulator.LowerCOM2:
                            {
                                var key = radioKnob;
                                Dispatcher?.BeginInvoke(
                                    (Action)delegate
                                    {
                                        LowerLeftCom2.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                        if (key.IsOn)
                                        {
                                            ClearAll(false);
                                            ShowGraphicConfiguration();
                                            LabelDialPosLower.Content = LOWER_TEXT + "COM2";
                                        }
                                    });
                                break;
                            }
                        case RadioPanelPZ69KnobsEmulator.LowerNAV1:
                            {
                                var key = radioKnob;
                                Dispatcher?.BeginInvoke(
                                    (Action)delegate
                                    {
                                        LowerLeftNav1.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                        if (key.IsOn)
                                        {
                                            ClearAll(false);
                                            ShowGraphicConfiguration();
                                            LabelDialPosLower.Content = LOWER_TEXT + "NAV1";
                                        }
                                    });
                                break;
                            }
                        case RadioPanelPZ69KnobsEmulator.LowerNAV2:
                            {
                                var key = radioKnob;
                                Dispatcher?.BeginInvoke(
                                    (Action)delegate
                                    {
                                        LowerLeftNav2.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                        if (key.IsOn)
                                        {
                                            ClearAll(false);
                                            ShowGraphicConfiguration();
                                            LabelDialPosLower.Content = LOWER_TEXT + "NAV2";
                                        }
                                    });
                                break;
                            }
                        case RadioPanelPZ69KnobsEmulator.LowerADF:
                            {
                                var key = radioKnob;
                                Dispatcher?.BeginInvoke(
                                    (Action)delegate
                                    {
                                        LowerLeftADF.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                        if (key.IsOn)
                                        {
                                            ClearAll(false);
                                            ShowGraphicConfiguration();
                                            LabelDialPosLower.Content = LOWER_TEXT + "ADF";
                                        }
                                    });
                                break;
                            }
                        case RadioPanelPZ69KnobsEmulator.LowerDME:
                            {
                                var key = radioKnob;
                                Dispatcher?.BeginInvoke(
                                    (Action)delegate
                                    {
                                        LowerLeftDME.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                        if (key.IsOn)
                                        {
                                            ClearAll(false);
                                            ShowGraphicConfiguration();
                                            LabelDialPosLower.Content = LOWER_TEXT + "DME";
                                        }
                                    });
                                break;
                            }
                        case RadioPanelPZ69KnobsEmulator.LowerXPDR:
                            {
                                var key = radioKnob;
                                Dispatcher?.BeginInvoke(
                                    (Action)delegate
                                    {
                                        LowerLeftXPDR.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                        if (key.IsOn)
                                        {
                                            ClearAll(false);
                                            ShowGraphicConfiguration();
                                            LabelDialPosLower.Content = LOWER_TEXT + "XPDR";
                                        }
                                    });
                                break;
                            }
                        case RadioPanelPZ69KnobsEmulator.UpperSmallFreqWheelInc:
                            {
                                var key = radioKnob;
                                Dispatcher?.BeginInvoke(
                                    (Action)delegate
                                    {
                                        UpperSmallerLCDKnobInc.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case RadioPanelPZ69KnobsEmulator.UpperSmallFreqWheelDec:
                            {
                                var key = radioKnob;
                                Dispatcher?.BeginInvoke(
                                    (Action)delegate
                                    {
                                        UpperSmallerLCDKnobDec.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case RadioPanelPZ69KnobsEmulator.UpperLargeFreqWheelInc:
                            {
                                var key = radioKnob;
                                Dispatcher?.BeginInvoke(
                                    (Action)delegate
                                    {
                                        UpperLargerLCDKnobInc.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case RadioPanelPZ69KnobsEmulator.UpperLargeFreqWheelDec:
                            {
                                var key = radioKnob;
                                Dispatcher?.BeginInvoke(
                                    (Action)delegate
                                    {
                                        UpperLargerLCDKnobDec.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case RadioPanelPZ69KnobsEmulator.LowerSmallFreqWheelInc:
                            {
                                var key = radioKnob;
                                Dispatcher?.BeginInvoke(
                                    (Action)delegate
                                    {
                                        LowerSmallerLCDKnobInc.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case RadioPanelPZ69KnobsEmulator.LowerSmallFreqWheelDec:
                            {
                                var key = radioKnob;
                                Dispatcher?.BeginInvoke(
                                    (Action)delegate
                                    {
                                        LowerSmallerLCDKnobDec.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case RadioPanelPZ69KnobsEmulator.LowerLargeFreqWheelInc:
                            {
                                var key = radioKnob;
                                Dispatcher?.BeginInvoke(
                                    (Action)delegate
                                    {
                                        LowerLargerLCDKnobInc.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case RadioPanelPZ69KnobsEmulator.LowerLargeFreqWheelDec:
                            {
                                var key = radioKnob;
                                Dispatcher?.BeginInvoke(
                                    (Action)delegate
                                    {
                                        LowerLargerLCDKnobDec.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case RadioPanelPZ69KnobsEmulator.UpperFreqSwitch:
                            {
                                var key = radioKnob;
                                Dispatcher?.BeginInvoke(
                                    (Action)delegate
                                    {
                                        UpperRightSwitch.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case RadioPanelPZ69KnobsEmulator.LowerFreqSwitch:
                            {
                                var key = radioKnob;
                                Dispatcher?.BeginInvoke(
                                    (Action)delegate
                                    {
                                        LowerRightSwitch.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
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

        private void HideSwitchImages()
        {
            TopLeftCom1.Visibility = Visibility.Collapsed;
            TopLeftCom2.Visibility = Visibility.Collapsed;
            TopLeftNav1.Visibility = Visibility.Collapsed;
            TopLeftNav2.Visibility = Visibility.Collapsed;
            TopLeftADF.Visibility = Visibility.Collapsed;
            TopLeftDME.Visibility = Visibility.Collapsed;
            TopLeftXPDR.Visibility = Visibility.Collapsed;
            LowerLeftCom1.Visibility = Visibility.Collapsed;
            LowerLeftCom2.Visibility = Visibility.Collapsed;
            LowerLeftNav1.Visibility = Visibility.Collapsed;
            LowerLeftNav2.Visibility = Visibility.Collapsed;
            LowerLeftADF.Visibility = Visibility.Collapsed;
            LowerLeftDME.Visibility = Visibility.Collapsed;
            LowerLeftXPDR.Visibility = Visibility.Collapsed;
            LowerLargerLCDKnobDec.Visibility = Visibility.Collapsed;
            UpperLargerLCDKnobInc.Visibility = Visibility.Collapsed;
            UpperRightSwitch.Visibility = Visibility.Collapsed;
            UpperSmallerLCDKnobDec.Visibility = Visibility.Collapsed;
            UpperSmallerLCDKnobInc.Visibility = Visibility.Collapsed;
            UpperLargerLCDKnobDec.Visibility = Visibility.Collapsed;
            LowerLargerLCDKnobInc.Visibility = Visibility.Collapsed;
            LowerRightSwitch.Visibility = Visibility.Collapsed;
            LowerSmallerLCDKnobDec.Visibility = Visibility.Collapsed;
            LowerSmallerLCDKnobInc.Visibility = Visibility.Collapsed;
        }

        private void HideButtonImages()
        {
            DotTopLeftLcd.Visibility = Visibility.Collapsed;
            DotBottomLeftLcd.Visibility = Visibility.Collapsed;
            DotTopRightLcd.Visibility = Visibility.Collapsed;
            DotBottomRightLcd.Visibility = Visibility.Collapsed;
        }

        private void HideAllImages()
        {
            HideButtonImages();
            HideSwitchImages();
        }

        private void ButtonGetId_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_radioPanelPZ69 != null)
                {
                    TextBoxLogPZ69.Text = string.Empty;
                    TextBoxLogPZ69.Text = _radioPanelPZ69.HIDInstance;
                    Clipboard.SetText(_radioPanelPZ69.HIDInstance);
                    MessageBox.Show("The Instance Id for the panel has been copied to the Clipboard.");
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void ComboBoxFreqKnobSensitivity_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (UserControlLoaded)
                {
                    Settings.Default.RadioFrequencyKnobSensitivityEmulator = int.Parse(ComboBoxFreqKnobSensitivity.SelectedValue.ToString());
                    _radioPanelPZ69.FrequencyKnobSensitivity = int.Parse(ComboBoxFreqKnobSensitivity.SelectedValue.ToString());
                    Settings.Default.Save();
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void ButtonLcd_OnClick(object sender, RoutedEventArgs e)
        {

            try
            {
                var button = (Button)sender;

                switch (button.Name)
                {
                    case "ButtonUpperLeftLcd":
                        {
                            ButtonLcdConfig(PZ69LCDPosition.UPPER_ACTIVE_LEFT, "Data to display on upper left LCD");
                            break;
                        }
                    case "ButtonLowerLeftLcd":
                        {
                            ButtonLcdConfig(PZ69LCDPosition.LOWER_ACTIVE_LEFT, "Data to display on bottom left LCD");
                            break;
                        }
                    case "ButtonUpperRightLcd":
                        {
                            ButtonLcdConfig(PZ69LCDPosition.UPPER_STBY_RIGHT, "Data to display on upper right LCD");
                            break;
                        }
                    case "ButtonLowerRightLcd":
                        {
                            ButtonLcdConfig(PZ69LCDPosition.LOWER_STBY_RIGHT, "Data to display on bottom right LCD");
                            break;
                        }
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }


        private void ButtonLcdConfig(PZ69LCDPosition pz69LCDPosition, string description)
        {
            try
            {
                DCSBiosOutputFormulaWindow dcsBiosOutputFormulaWindow = null;
                var dialPosition = _radioPanelPZ69.PZ69UpperDialPosition;
                if (pz69LCDPosition == PZ69LCDPosition.LOWER_ACTIVE_LEFT ||
                    pz69LCDPosition == PZ69LCDPosition.LOWER_STBY_RIGHT)
                {
                    dialPosition = _radioPanelPZ69.PZ69LowerDialPosition;
                }
                foreach (var dcsbiosBindingLCDPZ69 in _radioPanelPZ69.LCDBindings)
                {
                    if (dcsbiosBindingLCDPZ69.DialPosition == dialPosition && dcsbiosBindingLCDPZ69.PZ69LcdPosition == pz69LCDPosition)
                    {
                        if (dcsbiosBindingLCDPZ69.UseFormula)
                        {
                            dcsBiosOutputFormulaWindow = new DCSBiosOutputFormulaWindow(description, dcsbiosBindingLCDPZ69.DCSBIOSOutputFormulaObject, dcsbiosBindingLCDPZ69.LimitDecimalPlaces, dcsbiosBindingLCDPZ69.DecimalPlaces, false, true);
                            break;
                        }
                        dcsBiosOutputFormulaWindow = new DCSBiosOutputFormulaWindow(description, dcsbiosBindingLCDPZ69.DCSBIOSOutputObject, dcsbiosBindingLCDPZ69.LimitDecimalPlaces, dcsbiosBindingLCDPZ69.DecimalPlaces, false, true);
                        break;
                    }
                }
                if (dcsBiosOutputFormulaWindow == null)
                {
                    dcsBiosOutputFormulaWindow = new DCSBiosOutputFormulaWindow(description, false, true);
                }

                dcsBiosOutputFormulaWindow.ShowDialog();
                if (dcsBiosOutputFormulaWindow.DialogResult.HasValue && dcsBiosOutputFormulaWindow.DialogResult.Value)
                {
                    if (dcsBiosOutputFormulaWindow.UseFormula())
                    {
                        var dcsBiosOutputFormula = dcsBiosOutputFormulaWindow.DCSBIOSOutputFormula;
                        UpdateDCSBIOSBindingLCD(true, false, null, dcsBiosOutputFormula, pz69LCDPosition, dcsBiosOutputFormulaWindow.LimitDecimalPlaces, dcsBiosOutputFormulaWindow.DecimalPlaces);
                    }
                    else if (dcsBiosOutputFormulaWindow.UseSingleDCSBiosControl())
                    {
                        var dcsBiosOutput = dcsBiosOutputFormulaWindow.DCSBiosOutput;
                        UpdateDCSBIOSBindingLCD(false, false, dcsBiosOutput, null, pz69LCDPosition, dcsBiosOutputFormulaWindow.LimitDecimalPlaces, dcsBiosOutputFormulaWindow.DecimalPlaces);
                    }
                    else
                    {
                        //Delete config
                        UpdateDCSBIOSBindingLCD(false, true, null, null, pz69LCDPosition, dcsBiosOutputFormulaWindow.LimitDecimalPlaces, dcsBiosOutputFormulaWindow.DecimalPlaces);
                    }
                }
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
            PZ69LCDPosition pz69LCDPosition,
            bool limitDecimalPlaces,
            int decimalPlaces)
        {
            try
            {
                if (deleteConfig)
                {
                    if (pz69LCDPosition == PZ69LCDPosition.UPPER_ACTIVE_LEFT)
                    {
                        DotTopLeftLcd.Visibility = Visibility.Hidden;
                        _radioPanelPZ69.DeleteDCSBIOSLcdBinding(PZ69LCDPosition.UPPER_ACTIVE_LEFT);
                    }

                    if (pz69LCDPosition == PZ69LCDPosition.LOWER_ACTIVE_LEFT)
                    {
                        DotBottomLeftLcd.Visibility = Visibility.Hidden;
                        _radioPanelPZ69.DeleteDCSBIOSLcdBinding(PZ69LCDPosition.LOWER_ACTIVE_LEFT);
                    }

                    if (pz69LCDPosition == PZ69LCDPosition.UPPER_STBY_RIGHT)
                    {
                        DotTopRightLcd.Visibility = Visibility.Hidden;
                        _radioPanelPZ69.DeleteDCSBIOSLcdBinding(PZ69LCDPosition.UPPER_STBY_RIGHT);
                    }

                    if (pz69LCDPosition == PZ69LCDPosition.LOWER_STBY_RIGHT)
                    {
                        DotBottomRightLcd.Visibility = Visibility.Hidden;
                        _radioPanelPZ69.DeleteDCSBIOSLcdBinding(PZ69LCDPosition.LOWER_STBY_RIGHT);
                    }
                }

                if (!useFormula)
                {
                    if (pz69LCDPosition == PZ69LCDPosition.UPPER_ACTIVE_LEFT)
                    {
                        DotTopLeftLcd.Visibility = dcsbiosOutput == null ? Visibility.Collapsed : Visibility.Visible;
                        _radioPanelPZ69.AddOrUpdateLCDBinding(dcsbiosOutput, PZ69LCDPosition.UPPER_ACTIVE_LEFT, limitDecimalPlaces, decimalPlaces);
                    }
                    if (pz69LCDPosition == PZ69LCDPosition.LOWER_ACTIVE_LEFT)
                    {
                        DotBottomLeftLcd.Visibility = dcsbiosOutput == null ? Visibility.Collapsed : Visibility.Visible;
                        _radioPanelPZ69.AddOrUpdateLCDBinding(dcsbiosOutput, PZ69LCDPosition.LOWER_ACTIVE_LEFT, limitDecimalPlaces, decimalPlaces);
                    }
                    if (pz69LCDPosition == PZ69LCDPosition.UPPER_STBY_RIGHT)
                    {
                        DotTopRightLcd.Visibility = dcsbiosOutput == null ? Visibility.Collapsed : Visibility.Visible;
                        _radioPanelPZ69.AddOrUpdateLCDBinding(dcsbiosOutput, PZ69LCDPosition.UPPER_STBY_RIGHT, limitDecimalPlaces, decimalPlaces);
                    }
                    if (pz69LCDPosition == PZ69LCDPosition.LOWER_STBY_RIGHT)
                    {
                        DotBottomRightLcd.Visibility = dcsbiosOutput == null ? Visibility.Collapsed : Visibility.Visible;
                        _radioPanelPZ69.AddOrUpdateLCDBinding(dcsbiosOutput, PZ69LCDPosition.LOWER_STBY_RIGHT, limitDecimalPlaces, decimalPlaces);
                    }
                }

                if (useFormula)
                {
                    if (pz69LCDPosition == PZ69LCDPosition.UPPER_ACTIVE_LEFT)
                    {
                        DotTopLeftLcd.Visibility = dcsbiosOutputFormula == null ? Visibility.Collapsed : Visibility.Visible;
                        _radioPanelPZ69.AddOrUpdateLCDBinding(dcsbiosOutputFormula, PZ69LCDPosition.UPPER_ACTIVE_LEFT, limitDecimalPlaces, decimalPlaces);
                    }
                    if (pz69LCDPosition == PZ69LCDPosition.LOWER_ACTIVE_LEFT)
                    {
                        DotBottomLeftLcd.Visibility = dcsbiosOutputFormula == null ? Visibility.Collapsed : Visibility.Visible;
                        _radioPanelPZ69.AddOrUpdateLCDBinding(dcsbiosOutputFormula, PZ69LCDPosition.LOWER_ACTIVE_LEFT, limitDecimalPlaces, decimalPlaces);
                    }
                    if (pz69LCDPosition == PZ69LCDPosition.UPPER_STBY_RIGHT)
                    {
                        DotTopRightLcd.Visibility = dcsbiosOutputFormula == null ? Visibility.Collapsed : Visibility.Visible;
                        _radioPanelPZ69.AddOrUpdateLCDBinding(dcsbiosOutputFormula, PZ69LCDPosition.UPPER_STBY_RIGHT, limitDecimalPlaces, decimalPlaces);
                    }
                    if (pz69LCDPosition == PZ69LCDPosition.LOWER_STBY_RIGHT)
                    {
                        DotBottomRightLcd.Visibility = dcsbiosOutputFormula == null ? Visibility.Collapsed : Visibility.Visible;
                        _radioPanelPZ69.AddOrUpdateLCDBinding(dcsbiosOutputFormula, PZ69LCDPosition.LOWER_STBY_RIGHT, limitDecimalPlaces, decimalPlaces);
                    }
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
                TextBox t when t.Equals(TextBoxUpperCom1) => new PZ69SwitchOnOff(RadioPanelPZ69KnobsEmulator.UpperCOM1, true),
                TextBox t when t.Equals(TextBoxUpperCom2) => new PZ69SwitchOnOff(RadioPanelPZ69KnobsEmulator.UpperCOM2, true),
                TextBox t when t.Equals(TextBoxUpperNav1) => new PZ69SwitchOnOff(RadioPanelPZ69KnobsEmulator.UpperNAV1, true),
                TextBox t when t.Equals(TextBoxUpperNav2) => new PZ69SwitchOnOff(RadioPanelPZ69KnobsEmulator.UpperNAV2, true),
                TextBox t when t.Equals(TextBoxUpperADF) => new PZ69SwitchOnOff(RadioPanelPZ69KnobsEmulator.UpperADF, true),
                TextBox t when t.Equals(TextBoxUpperDME) => new PZ69SwitchOnOff(RadioPanelPZ69KnobsEmulator.UpperDME, true),
                TextBox t when t.Equals(TextBoxUpperXPDR) => new PZ69SwitchOnOff(RadioPanelPZ69KnobsEmulator.UpperXPDR, true),
                TextBox t when t.Equals(TextBoxUpperLargePlus) => new PZ69SwitchOnOff(RadioPanelPZ69KnobsEmulator.UpperLargeFreqWheelInc, true),
                TextBox t when t.Equals(TextBoxUpperLargeMinus) => new PZ69SwitchOnOff(RadioPanelPZ69KnobsEmulator.UpperLargeFreqWheelDec, true),
                TextBox t when t.Equals(TextBoxUpperSmallPlus) => new PZ69SwitchOnOff(RadioPanelPZ69KnobsEmulator.UpperSmallFreqWheelInc, true),
                TextBox t when t.Equals(TextBoxUpperSmallMinus) => new PZ69SwitchOnOff(RadioPanelPZ69KnobsEmulator.UpperSmallFreqWheelDec, true),
                TextBox t when t.Equals(TextBoxUpperActStbyOn) => new PZ69SwitchOnOff(RadioPanelPZ69KnobsEmulator.UpperFreqSwitch, true),
                TextBox t when t.Equals(TextBoxUpperActStbyOff) => new PZ69SwitchOnOff(RadioPanelPZ69KnobsEmulator.UpperFreqSwitch, false),
                TextBox t when t.Equals(TextBoxLowerCom1) => new PZ69SwitchOnOff(RadioPanelPZ69KnobsEmulator.LowerCOM1, true),
                TextBox t when t.Equals(TextBoxLowerCom2) => new PZ69SwitchOnOff(RadioPanelPZ69KnobsEmulator.LowerCOM2, true),
                TextBox t when t.Equals(TextBoxLowerNav1) => new PZ69SwitchOnOff(RadioPanelPZ69KnobsEmulator.LowerNAV1, true),
                TextBox t when t.Equals(TextBoxLowerNav2) => new PZ69SwitchOnOff(RadioPanelPZ69KnobsEmulator.LowerNAV2, true),
                TextBox t when t.Equals(TextBoxLowerADF) => new PZ69SwitchOnOff(RadioPanelPZ69KnobsEmulator.LowerADF, true),
                TextBox t when t.Equals(TextBoxLowerDME) => new PZ69SwitchOnOff(RadioPanelPZ69KnobsEmulator.LowerDME, true),
                TextBox t when t.Equals(TextBoxLowerXPDR) => new PZ69SwitchOnOff(RadioPanelPZ69KnobsEmulator.LowerXPDR, true),
                TextBox t when t.Equals(TextBoxLowerLargePlus) => new PZ69SwitchOnOff(RadioPanelPZ69KnobsEmulator.LowerLargeFreqWheelInc, true),
                TextBox t when t.Equals(TextBoxLowerLargeMinus) => new PZ69SwitchOnOff(RadioPanelPZ69KnobsEmulator.LowerLargeFreqWheelDec, true),
                TextBox t when t.Equals(TextBoxLowerSmallPlus) => new PZ69SwitchOnOff(RadioPanelPZ69KnobsEmulator.LowerSmallFreqWheelInc, true),
                TextBox t when t.Equals(TextBoxLowerSmallMinus) => new PZ69SwitchOnOff(RadioPanelPZ69KnobsEmulator.LowerSmallFreqWheelDec, true),
                TextBox t when t.Equals(TextBoxLowerActStbyOn) => new PZ69SwitchOnOff(RadioPanelPZ69KnobsEmulator.LowerFreqSwitch, true),
                TextBox t when t.Equals(TextBoxLowerActStbyOff) => new PZ69SwitchOnOff(RadioPanelPZ69KnobsEmulator.LowerFreqSwitch, false),
                _ => throw new Exception($"Failed to find RadioPanel knob for TextBox {textBox.Name}")
            };
        }

        public TextBox GetTextBox(object panelSwitch, bool isTurnedOn)
        {
            var key = (RadioPanelPZ69KnobsEmulator)panelSwitch;
            return (key, isTurnedOn) switch
            {
                (RadioPanelPZ69KnobsEmulator.UpperCOM1, true) => TextBoxUpperCom1,
                (RadioPanelPZ69KnobsEmulator.UpperCOM2, true) => TextBoxUpperCom2,
                (RadioPanelPZ69KnobsEmulator.UpperNAV1, true) => TextBoxUpperNav1,
                (RadioPanelPZ69KnobsEmulator.UpperNAV2, true) => TextBoxUpperNav2,
                (RadioPanelPZ69KnobsEmulator.UpperADF, true) => TextBoxUpperADF,
                (RadioPanelPZ69KnobsEmulator.UpperDME, true) => TextBoxUpperDME,
                (RadioPanelPZ69KnobsEmulator.UpperXPDR, true) => TextBoxUpperXPDR,
                (RadioPanelPZ69KnobsEmulator.UpperLargeFreqWheelInc, true) => TextBoxUpperLargePlus,
                (RadioPanelPZ69KnobsEmulator.UpperLargeFreqWheelDec, true) => TextBoxUpperLargeMinus,
                (RadioPanelPZ69KnobsEmulator.UpperSmallFreqWheelInc, true) => TextBoxUpperSmallPlus,
                (RadioPanelPZ69KnobsEmulator.UpperSmallFreqWheelDec, true) => TextBoxUpperSmallMinus,
                (RadioPanelPZ69KnobsEmulator.UpperFreqSwitch, true) => TextBoxUpperActStbyOn,
                (RadioPanelPZ69KnobsEmulator.UpperFreqSwitch, false) => TextBoxUpperActStbyOff,
                (RadioPanelPZ69KnobsEmulator.LowerCOM1, true) => TextBoxLowerCom1,
                (RadioPanelPZ69KnobsEmulator.LowerCOM2, true) => TextBoxLowerCom2,
                (RadioPanelPZ69KnobsEmulator.LowerNAV1, true) => TextBoxLowerNav1,
                (RadioPanelPZ69KnobsEmulator.LowerNAV2, true) => TextBoxLowerNav2,
                (RadioPanelPZ69KnobsEmulator.LowerADF, true) => TextBoxLowerADF,
                (RadioPanelPZ69KnobsEmulator.LowerDME, true) => TextBoxLowerDME,
                (RadioPanelPZ69KnobsEmulator.LowerXPDR, true) => TextBoxLowerXPDR,
                (RadioPanelPZ69KnobsEmulator.LowerLargeFreqWheelInc, true) => TextBoxLowerLargePlus,
                (RadioPanelPZ69KnobsEmulator.LowerLargeFreqWheelDec, true) => TextBoxLowerLargeMinus,
                (RadioPanelPZ69KnobsEmulator.LowerSmallFreqWheelInc, true) => TextBoxLowerSmallPlus,
                (RadioPanelPZ69KnobsEmulator.LowerSmallFreqWheelDec, true) => TextBoxLowerSmallMinus,
                (RadioPanelPZ69KnobsEmulator.LowerFreqSwitch, true) => TextBoxLowerActStbyOn,
                (RadioPanelPZ69KnobsEmulator.LowerFreqSwitch, false) => TextBoxLowerActStbyOff,
                _ => throw new Exception($"Failed to find text box based on key (RadioPanelUserControl) {key} and value {isTurnedOn}")
            };
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

        private void ButtonGetIdentify_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                _radioPanelPZ69.Identify();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }
    }
}
