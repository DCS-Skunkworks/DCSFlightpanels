namespace DCSFlightpanels.PanelUserControls
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using ClassLibraryCommon;
    using CustomControls;
    using Interfaces;
    using MEF;
    using NonVisuals.EventArgs;
    using NonVisuals.Interfaces;
    using NonVisuals.Panels.Saitek.Panels;
    using NonVisuals.Panels.Saitek.Switches;
    using NonVisuals.Panels.Saitek;
    using NonVisuals.Panels;
    using NonVisuals.HID;

    /// <summary>
    /// Interaction logic for TPMPanelUserControl.xaml
    /// </summary>
    public partial class TPMPanelUserControl : IGamingPanelListener, IProfileHandlerListener, IGamingPanelUserControl, IPanelUI
    {
        private readonly TPMPanel _tpmPanel;

        public TPMPanelUserControl(HIDSkeleton hidSkeleton)
        {
            InitializeComponent();
            
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

        public override void Init()
        {
            try
            {
                _tpmPanel.InitPanel();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void TPMPanelUserControl_OnLoaded(object sender, RoutedEventArgs e)
        {
            if (!UserControlLoaded || !TextBoxEnvironmentSet)
            {
                DarkMode.SetFrameworkElementDarkMode(this);
                HidePositionIndicators();
                SetTextBoxEnvironment();
                UserControlLoaded = true;
            }
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
                if (e.PanelType == GamingPanelEnum.TPM && e.HidInstance.Equals(_tpmPanel.HIDInstance))
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
                if (e.PanelBinding.PanelType == GamingPanelEnum.TPM && _tpmPanel.HIDInstance.Equals(e.PanelBinding.HIDInstance))
                {
                    ShowGraphicConfiguration();
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }
        
        public void SettingsApplied(object sender, PanelInfoArgs e)
        {
            try
            {
                if (e.HidInstance.Equals(_tpmPanel.HIDInstance) && e.PanelType == GamingPanelEnum.TPM)
                {
                    Dispatcher?.BeginInvoke(ShowGraphicConfiguration);
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
                if (_tpmPanel.HIDInstance.Equals(e.HidInstance))
                {
                    Dispatcher?.BeginInvoke(ShowGraphicConfiguration);
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
                if (textBox == TextBoxLogTPM || textBox == null)
                {
                    continue;
                }

                textBox.ClearAll();
            }

            if (clearAlsoProfile)
            {
                _tpmPanel.ClearSettings(true);
            }

            ShowGraphicConfiguration();
        }

        private void SetTextBoxEnvironment()
        {
            if (TextBoxEnvironmentSet || !Common.FindVisualChildren<TPMTextBox>(this).Any())
            {
                return;
            }

            foreach (var textBox in Common.FindVisualChildren<TPMTextBox>(this))
            {
                if (textBox.Equals(TextBoxLogTPM))
                {
                    continue;
                }

                textBox.SetEnvironment(this, _tpmPanel);
            }
            TextBoxEnvironmentSet = true;
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
        
        private void ShowGraphicConfiguration()
        {
            try
            {
                if (!UserControlLoaded || !TextBoxEnvironmentSet)
                {
                    return;
                }

                foreach (var keyBinding in _tpmPanel.KeyBindingsHashSet)
                {
                    var textBox = (TPMTextBox) GetTextBox(keyBinding.TPMSwitch, keyBinding.WhenTurnedOn);
                    if (keyBinding.OSKeyPress != null)
                    {
                        textBox.KeyPress = keyBinding.OSKeyPress;
                    }
                    else
                    {
                        textBox.KeyPress = null;
                    }
                }

                foreach (var operatingSystemCommand in _tpmPanel.OSCommandHashSet)
                {
                    var textBox = (TPMTextBox)GetTextBox(operatingSystemCommand.TPMSwitch, operatingSystemCommand.WhenTurnedOn);
                    if (operatingSystemCommand.OSCommandObject != null)
                    {
                        textBox.OSCommandObject = operatingSystemCommand.OSCommandObject;
                    }
                    else
                    {
                        textBox.OSCommandObject = null;
                    }
                }

                foreach (var dcsBiosBinding in _tpmPanel.DCSBiosBindings)
                {
                    var textBox = (TPMTextBox)GetTextBox(dcsBiosBinding.TPMSwitch, dcsBiosBinding.WhenTurnedOn);
                    textBox.DCSBIOSBinding = dcsBiosBinding.DCSBIOSInputs.Count > 0 ? dcsBiosBinding : null;
                }

                foreach (var bipLink in _tpmPanel.BipLinkHashSet)
                {
                    var textBox = (TPMTextBox)GetTextBox(bipLink.TPMSwitch, bipLink.WhenTurnedOn);
                    textBox.BipLink = bipLink.BIPLights.Count > 0 ? bipLink : null;
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
                    TextBoxLogTPM.Text = _tpmPanel.HIDInstance;
                    Clipboard.SetText(_tpmPanel.HIDInstance);
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