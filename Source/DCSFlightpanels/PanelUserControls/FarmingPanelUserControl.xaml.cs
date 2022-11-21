namespace DCSFlightpanels.PanelUserControls
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;

    using ClassLibraryCommon;

    using Bills;
    using CustomControls;
    using Interfaces;
    using Windows;

    using MEF;
    using NonVisuals.EventArgs;
    using NonVisuals.Interfaces;

    using Brush = System.Windows.Media.Brush;
    using Brushes = System.Windows.Media.Brushes;
    using NonVisuals.Panels.Saitek.Panels;
    using NonVisuals.Panels.Saitek.Switches;
    using NonVisuals.Panels.Saitek;
    using NonVisuals.Panels;
    using NonVisuals.HID;

    /// <summary>
    /// Interaction logic for SwitchPanelPZ55UserControl.xaml
    /// </summary>
    public partial class FarmingPanelUserControl : IGamingPanelListener, IProfileHandlerListener, IGamingPanelUserControl, IPanelUI
    {

        private readonly FarmingSidePanel _farmingSidePanel;

        private bool _textBoxBillsSet;




        public FarmingPanelUserControl(HIDSkeleton hidSkeleton)
        {
            InitializeComponent();
            
            _farmingSidePanel = new FarmingSidePanel(hidSkeleton);

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
                    _farmingSidePanel.Dispose();
                    AppEventHandler.DetachGamingPanelListener(this);
                }

                _disposed = true;
            }

            // Call base class implementation.
            base.Dispose(disposing);
        }
        

        private void SwitchPanelPZ55UserControl_OnLoaded(object sender, RoutedEventArgs e)
        {
            SetTextBoxBills();
            UserControlLoaded = true;
            ShowGraphicConfiguration();
        }
        

        public override GamingPanel GetGamingPanel()
        {
            return _farmingSidePanel;
        }

        public override GamingPanelEnum GetPanelType()
        {
            return GamingPanelEnum.FarmingPanel;
        }

        public string GetName()
        {
            return GetType().Name;
        }
        
        public void UpdatesHasBeenMissed(object sender, DCSBIOSUpdatesMissedEventArgs e)
        {
            try
            {
                //
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        public void SwitchesChanged(object sender, SwitchesChangedEventArgs e)
        {
            try
            {
                if (e.PanelType == GamingPanelEnum.FarmingPanel && e.HidInstance.Equals(_farmingSidePanel.HIDInstance))
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
                if (e.PanelBinding.PanelType == GamingPanelEnum.FarmingPanel && _farmingSidePanel.HIDInstance.Equals(e.PanelBinding.HIDInstance))
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
                if (e.HidInstance.Equals(_farmingSidePanel.HIDInstance) && e.PanelType == GamingPanelEnum.FarmingPanel)
                {
                    Dispatcher?.BeginInvoke(ShowGraphicConfiguration);
                    Dispatcher?.BeginInvoke((Action)(() => TextBoxLogFarmingPanel.Text = string.Empty));
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
                if (_farmingSidePanel.HIDInstance.Equals(e.HidInstance))
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
                TextBoxLogFarmingPanel.Focus();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void ClearAll(bool clearAlsoProfile)
        {
            foreach (var textBox in Common.FindVisualChildren<FarmingPanelTextBox>(this))
            {
                if (textBox == TextBoxLogFarmingPanel || textBox.Bill == null)
                {
                    continue;
                }
                textBox.Bill.ClearAll();
            }
            if (clearAlsoProfile)
            {
                _farmingSidePanel.ClearSettings(true);
            }

            ShowGraphicConfiguration();
        }

        private void SetTextBoxBills()
        {
            if (_textBoxBillsSet || !Common.FindVisualChildren<FarmingPanelTextBox>(this).Any())
            {
                return;
            }
            foreach (var textBox in Common.FindVisualChildren<FarmingPanelTextBox>(this))
            {
                if (textBox.Bill != null || textBox == TextBoxLogFarmingPanel)
                {
                    continue;
                }

                textBox.Bill = new BillPFarmingPanel(this, _farmingSidePanel, textBox);
            }
            _textBoxBillsSet = true;
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
                Dispatcher?.BeginInvoke((Action)(() => TextBoxLogFarmingPanel.Focus()));
                foreach (var farmingPanelKey in switches)
                {
                    var key = (FarmingPanelKey)farmingPanelKey;

                    if (_farmingSidePanel.ForwardPanelEvent)
                    {
                        if (!string.IsNullOrEmpty(_farmingSidePanel.GetKeyPressForLoggingPurposes(key)))
                        {
                            Dispatcher?.BeginInvoke(
                                (Action)
                                (() =>
                                 TextBoxLogFarmingPanel.Text =
                                 TextBoxLogFarmingPanel.Text.Insert(0, _farmingSidePanel.GetKeyPressForLoggingPurposes(key) + "\n")));
                        }
                    }
                    else
                    {
                        Dispatcher?.BeginInvoke(
                            (Action)
                            (() =>
                             TextBoxLogFarmingPanel.Text =
                             TextBoxLogFarmingPanel.Text = TextBoxLogFarmingPanel.Text.Insert(0, "No action taken, panel events Disabled.\n")));
                    }
                }
                SetGraphicsState(switches);
            }
            catch (Exception ex)
            {
                Dispatcher?.BeginInvoke(
                    (Action)
                    (() =>
                     TextBoxLogFarmingPanel.Text = TextBoxLogFarmingPanel.Text.Insert(0, "0x16" + ex.Message + ".\n")));
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void SetGraphicsState(HashSet<object> switches)
        {
            try
            {
                foreach (var sidePanelKeyObject in switches)
                {
                    var farmingPanelKey = (FarmingPanelKey)sidePanelKeyObject;
                    switch (farmingPanelKey.FarmingPanelMKKey)
                    {
                        case FarmingPanelMKKeys.BUTTON_1:
                            {
                                var key = farmingPanelKey;
                                Dispatcher?.BeginInvoke(
                                    (Action)delegate
                                    {
                                        ImageButton1.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case FarmingPanelMKKeys.BUTTON_2:
                            {
                                var key = farmingPanelKey;
                                Dispatcher?.BeginInvoke(
                                    (Action)delegate
                                    {
                                        ImageButton2.Visibility = !key.IsOn ? Visibility.Collapsed : Visibility.Visible;
                                    });
                                break;
                            }
                        case FarmingPanelMKKeys.BUTTON_3:
                            {
                                var key = farmingPanelKey;
                                Dispatcher?.BeginInvoke(
                                    (Action)delegate
                                    {
                                        ImageButton3.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case FarmingPanelMKKeys.BUTTON_4:
                            {
                                var key = farmingPanelKey;
                                Dispatcher?.BeginInvoke(
                                    (Action)delegate
                                    {
                                        ImageButton4.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case FarmingPanelMKKeys.BUTTON_5:
                            {
                                var key = farmingPanelKey;
                                Dispatcher?.BeginInvoke(
                                    (Action)delegate
                                    {
                                        ImageButton5.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case FarmingPanelMKKeys.BUTTON_6:
                            {
                                var key = farmingPanelKey;
                                Dispatcher?.BeginInvoke(
                                    (Action)delegate
                                    {
                                        ImageButton6.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case FarmingPanelMKKeys.BUTTON_7:
                            {
                                var key = farmingPanelKey;
                                Dispatcher?.BeginInvoke(
                                    (Action)delegate
                                    {
                                        ImageButton7.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case FarmingPanelMKKeys.BUTTON_8:
                            {
                                var key = farmingPanelKey;
                                Dispatcher?.BeginInvoke(
                                    (Action)delegate
                                    {
                                        ImageButton8.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case FarmingPanelMKKeys.BUTTON_9:
                            {
                                var key = farmingPanelKey;
                                Dispatcher?.BeginInvoke(
                                    (Action)delegate
                                    {
                                        ImageButton9.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case FarmingPanelMKKeys.BUTTON_10:
                            {
                                var key = farmingPanelKey;
                                Dispatcher?.BeginInvoke(
                                    (Action)delegate
                                    {
                                        ImageButton10.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case FarmingPanelMKKeys.BUTTON_11:
                            {
                                var key = farmingPanelKey;
                                Dispatcher?.BeginInvoke(
                                    (Action)delegate
                                    {
                                        ImageButton11.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case FarmingPanelMKKeys.BUTTON_12:
                            {
                                var key = farmingPanelKey;
                                Dispatcher?.BeginInvoke(
                                    (Action)delegate
                                    {
                                        ImageButton12.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case FarmingPanelMKKeys.BUTTON_13:
                            {
                                var key = farmingPanelKey;
                                Dispatcher?.BeginInvoke(
                                    (Action)delegate
                                    {
                                        ImageButton13.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case FarmingPanelMKKeys.BUTTON_14:
                            {
                                var key = farmingPanelKey;
                                Dispatcher?.BeginInvoke(
                                    (Action)delegate
                                    {
                                        ImageButton14.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case FarmingPanelMKKeys.BUTTON_15:
                            {
                                var key = farmingPanelKey;
                                Dispatcher?.BeginInvoke(
                                    (Action)delegate
                                    {
                                        ImageButton15.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case FarmingPanelMKKeys.BUTTON_16:
                            {
                                var key = farmingPanelKey;
                                Dispatcher?.BeginInvoke(
                                    (Action)delegate
                                    {
                                        ImageButton16.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case FarmingPanelMKKeys.BUTTON_17:
                            {
                                var key = farmingPanelKey;
                                Dispatcher?.BeginInvoke(
                                    (Action)delegate
                                    {
                                        ImageButton17.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case FarmingPanelMKKeys.BUTTON_18:
                            {
                                var key = farmingPanelKey;
                                Dispatcher?.BeginInvoke(
                                    (Action)delegate
                                    {
                                        ImageButton18.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case FarmingPanelMKKeys.BUTTON_19:
                            {
                                var key = farmingPanelKey;
                                Dispatcher?.BeginInvoke(
                                    (Action)delegate
                                    {
                                        ImageButton19.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case FarmingPanelMKKeys.BUTTON_20:
                            {
                                var key = farmingPanelKey;
                                Dispatcher?.BeginInvoke(
                                    (Action)delegate
                                    {
                                        ImageButton20.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case FarmingPanelMKKeys.BUTTON_21:
                            {
                                var key = farmingPanelKey;
                                Dispatcher?.BeginInvoke(
                                    (Action)delegate
                                    {
                                        ImageButton21.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case FarmingPanelMKKeys.BUTTON_22:
                            {
                                var key = farmingPanelKey;
                                Dispatcher?.BeginInvoke(
                                    (Action)delegate
                                    {
                                        ImageButton22.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case FarmingPanelMKKeys.BUTTON_23:
                            {
                                var key = farmingPanelKey;
                                Dispatcher?.BeginInvoke(
                                    (Action)delegate
                                    {
                                        ImageButton23.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case FarmingPanelMKKeys.BUTTON_24:
                            {
                                var key = farmingPanelKey;
                                Dispatcher?.BeginInvoke(
                                    (Action)delegate
                                    {
                                        ImageButton24.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case FarmingPanelMKKeys.BUTTON_25:
                            {
                                var key = farmingPanelKey;
                                Dispatcher?.BeginInvoke(
                                    (Action)delegate
                                    {
                                        ImageButton25.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case FarmingPanelMKKeys.BUTTON_26:
                            {
                                var key = farmingPanelKey;
                                Dispatcher?.BeginInvoke(
                                    (Action)delegate
                                    {
                                        ImageButton26.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case FarmingPanelMKKeys.BUTTON_27:
                            {
                                var key = farmingPanelKey;
                                Dispatcher?.BeginInvoke(
                                    (Action)delegate
                                    {
                                        ImageButton27.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case FarmingPanelMKKeys.BUTTON_JOY_LEFT:
                            {
                                var key = farmingPanelKey;
                                Dispatcher?.BeginInvoke(
                                    (Action)delegate
                                    {
                                        ImageButtonJoyLeft.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case FarmingPanelMKKeys.BUTTON_JOY_RIGHT:
                            {
                                var key = farmingPanelKey;
                                Dispatcher?.BeginInvoke(
                                    (Action)delegate
                                    {
                                        ImageButtonJoyRight.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
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
                if (!UserControlLoaded || !_textBoxBillsSet)
                {
                    return;
                }
                foreach (var keyBinding in _farmingSidePanel.KeyBindingsHashSet)
                {
                    var textBox = (FarmingPanelTextBox)GetTextBox(keyBinding.FarmingPanelKey, keyBinding.WhenTurnedOn);
                    if (keyBinding.OSKeyPress != null)
                    {
                        textBox.Bill.KeyPress = keyBinding.OSKeyPress;
                    }
                    else
                    {
                        textBox.Bill.KeyPress = null;
                    }
                }

                foreach (var operatingSystemCommand in _farmingSidePanel.OSCommandList)
                {
                    var textBox = (FarmingPanelTextBox)GetTextBox(operatingSystemCommand.FarmingPanelKey, operatingSystemCommand.WhenTurnedOn);
                    if (operatingSystemCommand.OSCommandObject != null)
                    {
                        textBox.Bill.OSCommandObject = operatingSystemCommand.OSCommandObject;
                    }
                    else
                    {
                        textBox.Bill.OSCommandObject = null;
                    }
                }

                foreach (var dcsBiosBinding in _farmingSidePanel.DCSBiosBindings)
                {
                    var textBox = (FarmingPanelTextBox)GetTextBox(dcsBiosBinding.FarmingPanelKey, dcsBiosBinding.WhenTurnedOn);
                    if (dcsBiosBinding.DCSBIOSInputs.Count > 0)
                    {
                        textBox.Bill.DCSBIOSBinding = dcsBiosBinding;
                    }
                    else
                    {
                        textBox.Bill.DCSBIOSBinding = null;
                    }
                }

                SetTextBoxBackgroundColors(Brushes.White); //Maybe we can remove this function and only retain the _textBoxBillsSet = true; ?
                foreach (var bipLink in _farmingSidePanel.BIPLinkHashSet)
                {
                    var textBox = (FarmingPanelTextBox)GetTextBox(bipLink.FarmingPanelKey, bipLink.WhenTurnedOn);
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

        private void SetTextBoxBackgroundColors(Brush brush)
        {
            foreach (var textBox in Common.FindVisualChildren<TextBox>(this))
            {
                if (!textBox.IsFocused && textBox.Background != Brushes.Yellow)
                {
                    textBox.Background = brush;
                }
            }
            _textBoxBillsSet = true;
        }

        private void ButtonGetId_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_farmingSidePanel != null)
                {
                    TextBoxLogFarmingPanel.Text = string.Empty;
                    TextBoxLogFarmingPanel.Text = _farmingSidePanel.HIDInstance;
                    Clipboard.SetText(_farmingSidePanel.HIDInstance);
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
                TextBox t when t.Equals(TextBox1) => new FarmingPanelOnOff(FarmingPanelMKKeys.BUTTON_1, true),
                TextBox t when t.Equals(TextBox2) => new FarmingPanelOnOff(FarmingPanelMKKeys.BUTTON_2, true),
                TextBox t when t.Equals(TextBox3) => new FarmingPanelOnOff(FarmingPanelMKKeys.BUTTON_3, true),
                TextBox t when t.Equals(TextBox4) => new FarmingPanelOnOff(FarmingPanelMKKeys.BUTTON_4, true),
                TextBox t when t.Equals(TextBox5) => new FarmingPanelOnOff(FarmingPanelMKKeys.BUTTON_5, true),
                TextBox t when t.Equals(TextBox6) => new FarmingPanelOnOff(FarmingPanelMKKeys.BUTTON_6, true),
                TextBox t when t.Equals(TextBox7) => new FarmingPanelOnOff(FarmingPanelMKKeys.BUTTON_7, true),
                TextBox t when t.Equals(TextBox8) => new FarmingPanelOnOff(FarmingPanelMKKeys.BUTTON_8, true),
                TextBox t when t.Equals(TextBox9) => new FarmingPanelOnOff(FarmingPanelMKKeys.BUTTON_9, true),
                TextBox t when t.Equals(TextBox10) => new FarmingPanelOnOff(FarmingPanelMKKeys.BUTTON_10, true),
                TextBox t when t.Equals(TextBox11) => new FarmingPanelOnOff(FarmingPanelMKKeys.BUTTON_11, true),
                TextBox t when t.Equals(TextBox12) => new FarmingPanelOnOff(FarmingPanelMKKeys.BUTTON_12, true),
                TextBox t when t.Equals(TextBox13) => new FarmingPanelOnOff(FarmingPanelMKKeys.BUTTON_13, true),
                TextBox t when t.Equals(TextBox14) => new FarmingPanelOnOff(FarmingPanelMKKeys.BUTTON_14, true),
                TextBox t when t.Equals(TextBox15) => new FarmingPanelOnOff(FarmingPanelMKKeys.BUTTON_15, true),
                TextBox t when t.Equals(TextBox16) => new FarmingPanelOnOff(FarmingPanelMKKeys.BUTTON_16, true),
                TextBox t when t.Equals(TextBox17) => new FarmingPanelOnOff(FarmingPanelMKKeys.BUTTON_17, true),
                TextBox t when t.Equals(TextBox18) => new FarmingPanelOnOff(FarmingPanelMKKeys.BUTTON_18, true),
                TextBox t when t.Equals(TextBox19) => new FarmingPanelOnOff(FarmingPanelMKKeys.BUTTON_19, true),
                TextBox t when t.Equals(TextBox20) => new FarmingPanelOnOff(FarmingPanelMKKeys.BUTTON_20, true),
                TextBox t when t.Equals(TextBox21) => new FarmingPanelOnOff(FarmingPanelMKKeys.BUTTON_21, true),
                TextBox t when t.Equals(TextBox22) => new FarmingPanelOnOff(FarmingPanelMKKeys.BUTTON_22, true),
                TextBox t when t.Equals(TextBox23) => new FarmingPanelOnOff(FarmingPanelMKKeys.BUTTON_23, true),
                TextBox t when t.Equals(TextBox24) => new FarmingPanelOnOff(FarmingPanelMKKeys.BUTTON_24, true),
                TextBox t when t.Equals(TextBox25) => new FarmingPanelOnOff(FarmingPanelMKKeys.BUTTON_25, true),
                TextBox t when t.Equals(TextBox26) => new FarmingPanelOnOff(FarmingPanelMKKeys.BUTTON_26, true),
                TextBox t when t.Equals(TextBox27) => new FarmingPanelOnOff(FarmingPanelMKKeys.BUTTON_27, true),
                TextBox t when t.Equals(TextBoxJoyLeft) => new FarmingPanelOnOff(FarmingPanelMKKeys.BUTTON_JOY_LEFT, true),
                TextBox t when t.Equals(TextBoxJoyRight) => new FarmingPanelOnOff(FarmingPanelMKKeys.BUTTON_JOY_RIGHT, true),
                _ => throw new Exception($"Failed to find key based on text box (FarmingSidePanelUserControl)  {textBox.Name}")
            };
        }
        public TextBox GetTextBox(object panelSwitch, bool isTurnedOn)
        {
            var key = (FarmingPanelMKKeys)panelSwitch;
            return (key, isTurnedOn) switch
            {
                (FarmingPanelMKKeys.BUTTON_1, true) => TextBox1,
                (FarmingPanelMKKeys.BUTTON_2, true) => TextBox2,
                (FarmingPanelMKKeys.BUTTON_3, true) => TextBox3,
                (FarmingPanelMKKeys.BUTTON_4, true) => TextBox4,
                (FarmingPanelMKKeys.BUTTON_5, true) => TextBox5,
                (FarmingPanelMKKeys.BUTTON_6, true) => TextBox6,
                (FarmingPanelMKKeys.BUTTON_7, true) => TextBox7,
                (FarmingPanelMKKeys.BUTTON_8, true) => TextBox8,
                (FarmingPanelMKKeys.BUTTON_9, true) => TextBox9,
                (FarmingPanelMKKeys.BUTTON_10, true) => TextBox10,
                (FarmingPanelMKKeys.BUTTON_11, true) => TextBox11,
                (FarmingPanelMKKeys.BUTTON_12, true) => TextBox12,
                (FarmingPanelMKKeys.BUTTON_13, true) => TextBox13,
                (FarmingPanelMKKeys.BUTTON_14, true) => TextBox14,
                (FarmingPanelMKKeys.BUTTON_15, true) => TextBox15,
                (FarmingPanelMKKeys.BUTTON_16, true) => TextBox16,
                (FarmingPanelMKKeys.BUTTON_17, true) => TextBox17,
                (FarmingPanelMKKeys.BUTTON_18, true) => TextBox18,
                (FarmingPanelMKKeys.BUTTON_19, true) => TextBox19,
                (FarmingPanelMKKeys.BUTTON_20, true) => TextBox20,
                (FarmingPanelMKKeys.BUTTON_21, true) => TextBox21,
                (FarmingPanelMKKeys.BUTTON_22, true) => TextBox22,
                (FarmingPanelMKKeys.BUTTON_23, true) => TextBox23,
                (FarmingPanelMKKeys.BUTTON_24, true) => TextBox24,
                (FarmingPanelMKKeys.BUTTON_25, true) => TextBox25,
                (FarmingPanelMKKeys.BUTTON_26, true) => TextBox26,
                (FarmingPanelMKKeys.BUTTON_27, true) => TextBox27,
                (FarmingPanelMKKeys.BUTTON_JOY_LEFT, true) => TextBoxJoyLeft,
                (FarmingPanelMKKeys.BUTTON_JOY_RIGHT, true) => TextBoxJoyRight,
                _ => throw new Exception($"Failed to find text box based on key (FarmingPanelUserControl) {key} and value {isTurnedOn}")
            };
        }
        private void ButtonDEV_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var window = new KeyPressReadingSmallWindow(KeyPressLength.SecondAndHalf, "ESC");
                window.ShowDialog();
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
                _farmingSidePanel.Identify();
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


        private void HideAllImages()
        {
            ImageButton1.Visibility = Visibility.Collapsed;
            ImageButton2.Visibility = Visibility.Collapsed;
            ImageButton3.Visibility = Visibility.Collapsed;
            ImageButton4.Visibility = Visibility.Collapsed;
            ImageButton5.Visibility = Visibility.Collapsed;
            ImageButton6.Visibility = Visibility.Collapsed;
            ImageButton7.Visibility = Visibility.Collapsed;
            ImageButton8.Visibility = Visibility.Collapsed;
            ImageButton9.Visibility = Visibility.Collapsed;
            ImageButton10.Visibility = Visibility.Collapsed;
            ImageButton11.Visibility = Visibility.Collapsed;
            ImageButton12.Visibility = Visibility.Collapsed;
            ImageButton13.Visibility = Visibility.Collapsed;
            ImageButton14.Visibility = Visibility.Collapsed;
            ImageButton15.Visibility = Visibility.Collapsed;
            ImageButton16.Visibility = Visibility.Collapsed;
            ImageButton17.Visibility = Visibility.Collapsed;
            ImageButton18.Visibility = Visibility.Collapsed;
            ImageButton19.Visibility = Visibility.Collapsed;
            ImageButton20.Visibility = Visibility.Collapsed;
            ImageButton21.Visibility = Visibility.Collapsed;
            ImageButton22.Visibility = Visibility.Collapsed;
            ImageButton23.Visibility = Visibility.Collapsed;
            ImageButton24.Visibility = Visibility.Collapsed;
            ImageButton25.Visibility = Visibility.Collapsed;
            ImageButton26.Visibility = Visibility.Collapsed;
            ImageButton27.Visibility = Visibility.Collapsed;
            ImageButtonJoyLeft.Visibility = Visibility.Collapsed;
            ImageButtonJoyRight.Visibility = Visibility.Collapsed;
        }
    }
}
