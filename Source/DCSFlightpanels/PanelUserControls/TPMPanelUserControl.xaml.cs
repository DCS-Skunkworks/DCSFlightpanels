namespace DCSFlightpanels.PanelUserControls
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Media;

    using ClassLibraryCommon;

    using DCSFlightpanels.Bills;
    using DCSFlightpanels.CustomControls;
    using DCSFlightpanels.Interfaces;
    using DCSFlightpanels.Windows;

    using MEF;

    using NonVisuals;
    using NonVisuals.EventArgs;
    using NonVisuals.Interfaces;
    using NonVisuals.Saitek;
    using NonVisuals.Saitek.Panels;
    using NonVisuals.Saitek.Switches;

    /// <summary>
    /// Interaction logic for TPMPanelUserControl.xaml
    /// </summary>
    public partial class TPMPanelUserControl : UserControlBase, IGamingPanelListener, IProfileHandlerListener, IGamingPanelUserControl, IPanelUI
    {
        private readonly TPMPanel _tpmPanel;
        private bool _once;
        private bool _textBoxBillsSet;



        public TPMPanelUserControl(HIDSkeleton hidSkeleton, TabItem parentTabItem)
        {
            InitializeComponent();
            ParentTabItem = parentTabItem;

            hidSkeleton.HIDReadDevice.Removed += DeviceRemovedHandler;

            _tpmPanel = new TPMPanel(hidSkeleton);

            AppEventHandler.AttachGamingPanelListener(this);
        }


        private bool _disposed;
        // Protected implementation of Dispose pattern.
        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _tpmPanel.Dispose();
                    AppEventHandler.DetachGamingPanelListener(this);
                }

                _disposed = true;
            }

            // Call base class implementation.
            base.Dispose(disposing);
        }
        
        private void TPMPanelUserControl_OnLoaded(object sender, RoutedEventArgs e)
        {
            if (!_once)
            {
                HidePositionIndicators();
                _once = true;
            }
            
            SetTextBoxBills();
            UserControlLoaded = true;
            ShowGraphicConfiguration();
        }

        private void HidePositionIndicators()
        {
            try
            {
                var imageList = Common.FindVisualChildren<Image>(this);
                foreach (var image in imageList)
                {
                    if (image.Name.StartsWith("ImageG"))
                    {
                        image.Visibility = Visibility.Collapsed;
                    }
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }
        
        public override GamingPanel GetGamingPanel()
        {
            return _tpmPanel;
        }

        public override GamingPanelEnum GetPanelType()
        {
            return GamingPanelEnum.TPM;
        }

        public string GetName()
        {
            return GetType().Name;
        }
        
        public void UpdatesHasBeenMissed(object sender, DCSBIOSUpdatesMissedEventArgs e)
        {
        }

        public void SwitchesChanged(object sender, SwitchesChangedEventArgs e)
        {
            try
            {
                if (e.PanelType == GamingPanelEnum.TPM && e.HidInstance.Equals(_tpmPanel.HIDInstanceId))
                {
                    NotifySwitchChanges(e.Switches);
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
                if (e.PanelBinding.PanelType == GamingPanelEnum.TPM && _tpmPanel.HIDInstanceId == e.PanelBinding.HIDInstance)
                {
                    ShowGraphicConfiguration();
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }
        
        public void LedLightChanged(object sender, LedLightChangeEventArgs e) { }
        
        public void SettingsApplied(object sender, PanelInfoArgs e)
        {
            try
            {
                if (e.HidInstance.Equals(_tpmPanel.HIDInstanceId) && e.PanelType == GamingPanelEnum.TPM)
                {
                    Dispatcher?.BeginInvoke((Action)(ShowGraphicConfiguration));
                    Dispatcher?.BeginInvoke((Action)(() => TextBoxLogTPM.Text = string.Empty));
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
                if (_tpmPanel.HIDInstanceId == e.HidInstance)
                {
                    Dispatcher?.BeginInvoke((Action)(ShowGraphicConfiguration));
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
                TextBoxLogTPM.Focus();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void ClearAll(bool clearAlsoProfile)
        {
            foreach (var textBox in Common.FindVisualChildren<TPMTextBox>(this))
            {
                if (textBox.Equals(TextBoxLogTPM) || textBox.Bill == null)
                {
                    continue;
                }

                textBox.Bill.ClearAll();
            }

            if (clearAlsoProfile)
            {
                _tpmPanel.ClearSettings(true);
            }

            ShowGraphicConfiguration();
        }

        private void SetTextBoxBills()
        {
            if (_textBoxBillsSet || !Common.FindVisualChildren<TPMTextBox>(this).Any())
            {
                return;
            }

            foreach (var textBox in Common.FindVisualChildren<TPMTextBox>(this))
            {
                if (textBox.Bill != null || textBox.Equals(TextBoxLogTPM))
                {
                    continue;
                }

                textBox.Bill = new BillTPM(this, _tpmPanel, textBox);
            }
            _textBoxBillsSet = true;
        }

        private TPMTextBox GetTextBoxInFocus()
        {
            foreach (var textBox in Common.FindVisualChildren<TPMTextBox>(this))
            {
                if (!Equals(textBox, TextBoxLogTPM) && textBox.IsFocused && Equals(textBox.Background, Brushes.Yellow))
                {
                    return textBox;
                }
            }

            return null;
        }
        
        private void ButtonClearAllClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (MessageBox.Show("Clear all settings for the Switch Panel?", "Confirm", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                {
                    ClearAll(true);
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void NotifySwitchChanges(HashSet<object> switches)
        {
            try
            {
                //Set focus to this so that virtual keypresses won't affect settings
                Dispatcher?.BeginInvoke((Action)(() => TextBoxLogTPM.Focus()));
                foreach (var tpmPanelSwitch in switches)
                {
                    var key = (TPMPanelSwitch)tpmPanelSwitch;

                    if (_tpmPanel.ForwardPanelEvent)
                    {
                        if (!string.IsNullOrEmpty(_tpmPanel.GetKeyPressForLoggingPurposes(key)))
                        {
                            Dispatcher?.BeginInvoke(
                                (Action)
                                (() =>
                                    TextBoxLogTPM.Text =
                                        TextBoxLogTPM.Text.Insert(0, _tpmPanel.GetKeyPressForLoggingPurposes(key) + "\n")));
                        }
                    }
                    else
                    {
                        Dispatcher?.BeginInvoke(
                            (Action)
                            (() =>
                                TextBoxLogTPM.Text =
                                    TextBoxLogTPM.Text = TextBoxLogTPM.Text.Insert(0, "No action taken, panel events Disabled.\n")));
                    }
                }

                SetGraphicsState(switches);
            }
            catch (Exception ex)
            {
                Dispatcher?.BeginInvoke(
                    (Action)
                    (() =>
                        TextBoxLogTPM.Text = TextBoxLogTPM.Text.Insert(0, "0x16" + ex.Message + ".\n")));
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void SetGraphicsState(HashSet<object> switches)
        {
            try
            {
                foreach (var tpmPanelSwitchObject in switches)
                {
                    var tpmPanelSwitch = (TPMPanelSwitch)tpmPanelSwitchObject;
                    switch (tpmPanelSwitch.TPMSwitch)
                    {
                        case TPMPanelSwitches.G1:
                            {
                                var key = tpmPanelSwitch;
                                Dispatcher?.BeginInvoke(
                                    (Action)delegate
                                   {
                                        ImageG1On.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                        ImageG1Off.Visibility = key.IsOn ? Visibility.Collapsed : Visibility.Visible;
                                    });
                                break;
                            }
                        case TPMPanelSwitches.G2:
                            {
                                var key = tpmPanelSwitch;
                                Dispatcher?.BeginInvoke(
                                    (Action)delegate
                                   {
                                        ImageG2On.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                        ImageG2Off.Visibility = key.IsOn ? Visibility.Collapsed : Visibility.Visible;
                                    });
                                break;
                            }
                        case TPMPanelSwitches.G3:
                            {
                                var key = tpmPanelSwitch;
                                Dispatcher?.BeginInvoke(
                                    (Action)delegate
                                   {
                                        ImageG3On.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                        ImageG3Off.Visibility = key.IsOn ? Visibility.Collapsed : Visibility.Visible;
                                    });
                                break;
                            }
                        case TPMPanelSwitches.G4:
                            {
                                var key = tpmPanelSwitch;
                                Dispatcher?.BeginInvoke(
                                    (Action)delegate
                                   {
                                        ImageG4On.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                        ImageG4Off.Visibility = key.IsOn ? Visibility.Collapsed : Visibility.Visible;
                                    });
                                break;
                            }
                        case TPMPanelSwitches.G5:
                            {
                                var key = tpmPanelSwitch;
                                Dispatcher?.BeginInvoke(
                                    (Action)delegate
                                   {
                                        ImageG5On.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                        ImageG5Off.Visibility = key.IsOn ? Visibility.Collapsed : Visibility.Visible;
                                    });
                                break;
                            }
                        case TPMPanelSwitches.G6:
                            {
                                var key = tpmPanelSwitch;
                                Dispatcher?.BeginInvoke(
                                    (Action)delegate
                                   {
                                        ImageG6On.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                        ImageG6Off.Visibility = key.IsOn ? Visibility.Collapsed : Visibility.Visible;
                                    });
                                break;
                            }
                        case TPMPanelSwitches.G7:
                            {
                                var key = tpmPanelSwitch;
                                Dispatcher?.BeginInvoke(
                                    (Action)delegate
                                   {
                                        ImageG7On.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                        ImageG7Off.Visibility = key.IsOn ? Visibility.Collapsed : Visibility.Visible;
                                    });
                                break;
                            }
                        case TPMPanelSwitches.G8:
                            {
                                var key = tpmPanelSwitch;
                                Dispatcher?.BeginInvoke(
                                    (Action)delegate
                                   {
                                        ImageG8On.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                        ImageG8Off.Visibility = key.IsOn ? Visibility.Collapsed : Visibility.Visible;
                                    });
                                break;
                            }
                        case TPMPanelSwitches.G9:
                            {
                                var key = tpmPanelSwitch;
                                Dispatcher?.BeginInvoke(
                                    (Action)delegate
                                   {
                                        ImageG9On.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                        ImageG9Off.Visibility = key.IsOn ? Visibility.Collapsed : Visibility.Visible;
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
        
        private void UpdateKeyBindingProfileSimpleKeyStrokes(TPMTextBox textBox)
        {
            try
            {
                KeyPressLength keyPressLength;
                if (!textBox.Bill.ContainsKeyPress() || textBox.Bill.KeyPress.KeyPressSequence.Count == 0)
                {
                    keyPressLength = KeyPressLength.ThirtyTwoMilliSec;
                }
                else
                {
                    keyPressLength = textBox.Bill.KeyPress.GetLengthOfKeyPress();
                }

                _tpmPanel.AddOrUpdateKeyStrokeBinding(GetSwitch(textBox), textBox.Text, keyPressLength);
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void UpdateOSCommandBindingsTPM(TPMTextBox textBox)
        {
            try
            {
                _tpmPanel.AddOrUpdateOSCommandBinding(GetSwitch(textBox), textBox.Bill.OSCommandObject);
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

                foreach (var keyBinding in _tpmPanel.KeyBindingsHashSet)
                {
                    var textBox = (TPMTextBox) GetTextBox(keyBinding.TPMSwitch, keyBinding.WhenTurnedOn);
                    if (keyBinding.OSKeyPress != null)
                    {
                        textBox.Bill.KeyPress = keyBinding.OSKeyPress;
                    }
                    else
                    {
                        textBox.Bill.KeyPress = null;
                    }
                }

                foreach (var operatingSystemCommand in _tpmPanel.OSCommandHashSet)
                {
                    var textBox = (TPMTextBox)GetTextBox(operatingSystemCommand.TPMSwitch, operatingSystemCommand.WhenTurnedOn);
                    if (operatingSystemCommand.OSCommandObject != null)
                    {
                        textBox.Bill.OSCommandObject = operatingSystemCommand.OSCommandObject;
                    }
                    else
                    {
                        textBox.Bill.OSCommandObject = null;
                    }
                }

                foreach (var dcsBiosBinding in _tpmPanel.DCSBiosBindings)
                {
                    var textBox = (TPMTextBox)GetTextBox(dcsBiosBinding.TPMSwitch, dcsBiosBinding.WhenTurnedOn);
                    if (dcsBiosBinding.DCSBIOSInputs.Count > 0)
                    {
                        textBox.Bill.DCSBIOSBinding = dcsBiosBinding;
                    }
                    else
                    {
                        textBox.Bill.DCSBIOSBinding = null;
                    }
                }

                foreach (var bipLink in _tpmPanel.BipLinkHashSet)
                {
                    var textBox = (TPMTextBox)GetTextBox(bipLink.TPMSwitch, bipLink.WhenTurnedOn);
                    if (bipLink.BIPLights.Count > 0)
                    {
                        textBox.Bill.BipLink = bipLink;
                    }
                    else
                    {
                        textBox.Bill.BipLink = null;
                    }
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
                if (_tpmPanel != null)
                {
                    TextBoxLogTPM.Text = string.Empty;
                    TextBoxLogTPM.Text = _tpmPanel.HIDInstanceId;
                    Clipboard.SetText(_tpmPanel.HIDInstanceId);
                    MessageBox.Show("The Instance Id for the panel has been copied to the Clipboard.");
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
                TextBox t when t.Equals(TextBoxG1Off) => new TPMSwitchOnOff(TPMPanelSwitches.G1, false),
                TextBox t when t.Equals(TextBoxG1On) => new TPMSwitchOnOff(TPMPanelSwitches.G1, true),
                TextBox t when t.Equals(TextBoxG2Off) => new TPMSwitchOnOff(TPMPanelSwitches.G2, false),
                TextBox t when t.Equals(TextBoxG2On) => new TPMSwitchOnOff(TPMPanelSwitches.G2, true),
                TextBox t when t.Equals(TextBoxG3Off) => new TPMSwitchOnOff(TPMPanelSwitches.G3, false),
                TextBox t when t.Equals(TextBoxG3On) => new TPMSwitchOnOff(TPMPanelSwitches.G3, true),
                TextBox t when t.Equals(TextBoxG4Off) => new TPMSwitchOnOff(TPMPanelSwitches.G4, false),
                TextBox t when t.Equals(TextBoxG4On) => new TPMSwitchOnOff(TPMPanelSwitches.G4, true),
                TextBox t when t.Equals(TextBoxG5Off) => new TPMSwitchOnOff(TPMPanelSwitches.G5, false),
                TextBox t when t.Equals(TextBoxG5On) => new TPMSwitchOnOff(TPMPanelSwitches.G5, true),
                TextBox t when t.Equals(TextBoxG6Off) => new TPMSwitchOnOff(TPMPanelSwitches.G6, false),
                TextBox t when t.Equals(TextBoxG6On) => new TPMSwitchOnOff(TPMPanelSwitches.G6, true),
                TextBox t when t.Equals(TextBoxG7Off) => new TPMSwitchOnOff(TPMPanelSwitches.G7, false),
                TextBox t when t.Equals(TextBoxG7On) => new TPMSwitchOnOff(TPMPanelSwitches.G7, true),
                TextBox t when t.Equals(TextBoxG8Off) => new TPMSwitchOnOff(TPMPanelSwitches.G8, false),
                TextBox t when t.Equals(TextBoxG8On) => new TPMSwitchOnOff(TPMPanelSwitches.G8, true),
                TextBox t when t.Equals(TextBoxG9Off) => new TPMSwitchOnOff(TPMPanelSwitches.G9, false),
                TextBox t when t.Equals(TextBoxG9On) => new TPMSwitchOnOff(TPMPanelSwitches.G9, true),
                _ => throw new Exception($"Failed to find key based on text box (TPMUserControl) {textBox.Name}")
            };
        }
        public TextBox GetTextBox(object panelSwitch, bool isTurnedOn)
        {
            var key = (TPMPanelSwitches)panelSwitch;
            return (key, isTurnedOn) switch
            {
                (TPMPanelSwitches.G1, false) => TextBoxG1Off,
                (TPMPanelSwitches.G1, true) => TextBoxG1On,
                (TPMPanelSwitches.G2, false) => TextBoxG2Off,
                (TPMPanelSwitches.G2, true) => TextBoxG2On,
                (TPMPanelSwitches.G3, false) => TextBoxG3Off,
                (TPMPanelSwitches.G3, true) => TextBoxG3On,
                (TPMPanelSwitches.G4, false) => TextBoxG4Off,
                (TPMPanelSwitches.G4, true) => TextBoxG4On,
                (TPMPanelSwitches.G5, false) => TextBoxG5Off,
                (TPMPanelSwitches.G5, true) => TextBoxG5On,
                (TPMPanelSwitches.G6, false) => TextBoxG6Off,
                (TPMPanelSwitches.G6, true) => TextBoxG6On,
                (TPMPanelSwitches.G7, false) => TextBoxG7Off,
                (TPMPanelSwitches.G7, true) => TextBoxG7On,
                (TPMPanelSwitches.G8, false) => TextBoxG8Off,
                (TPMPanelSwitches.G8, true) => TextBoxG8On,
                (TPMPanelSwitches.G9, false) => TextBoxG9Off,
                (TPMPanelSwitches.G9, true) => TextBoxG9On,
                _ => throw new Exception($"Failed to find text box based on key (TPMPanelUserControl) {key} and value {isTurnedOn}")
            };
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

                textBox.Bill.ClearAll();
                var vkNull = Enum.GetName(typeof(MEF.VirtualKeyCode), MEF.VirtualKeyCode.VK_NULL);
                if (string.IsNullOrEmpty(vkNull))
                {
                    return;
                }

                var keyPress = new KeyPress(vkNull, KeyPressLength.ThirtyTwoMilliSec);
                textBox.Bill.KeyPress = keyPress;
                textBox.Bill.KeyPress.Description = "VK_NULL";
                textBox.Text = vkNull;
                UpdateKeyBindingProfileSimpleKeyStrokes(textBox);
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
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

                    var operatingSystemCommand = osCommandWindow.OSCommandObject;
                    textBox.Bill.OSCommandObject = operatingSystemCommand;
                    UpdateOSCommandBindingsTPM(textBox);
                    textBox.Text = operatingSystemCommand.Name;
                }

                TextBoxLogTPM.Focus();
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