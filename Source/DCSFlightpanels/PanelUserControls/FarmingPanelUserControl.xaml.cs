namespace DCSFlightpanels.PanelUserControls
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;

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

    using Brush = System.Windows.Media.Brush;
    using Brushes = System.Windows.Media.Brushes;

    /// <summary>
    /// Interaction logic for SwitchPanelPZ55UserControl.xaml
    /// </summary>
    public partial class FarmingPanelUserControl : UserControlBase, IGamingPanelListener, IProfileHandlerListener, IGamingPanelUserControl, IPanelUI
    {

        private readonly FarmingSidePanel _farmingSidePanel;

        private bool _textBoxBillsSet;




        public FarmingPanelUserControl(HIDSkeleton hidSkeleton, TabItem parentTabItem)
        {
            InitializeComponent();
            hidSkeleton.HIDReadDevice.Removed += DeviceRemovedHandler;

            ParentTabItem = parentTabItem;
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

        public void ProfileSelected(object sender, AirframeEventArgs e)
        {
            try
            {
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
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
                if (e.PanelType == GamingPanelEnum.FarmingPanel && e.HidInstance.Equals(_farmingSidePanel.HIDInstanceId))
                {
                    NotifySwitchChanges(e.Switches);
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        public void PanelBindingReadFromFile(object sender, PanelBindingReadFromFileEventArgs e)
        {
            try
            {
                if (e.PanelBinding.PanelType == GamingPanelEnum.FarmingPanel && _farmingSidePanel.HIDInstanceId == e.PanelBinding.HIDInstance)
                {
                    ShowGraphicConfiguration();
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        public void LedLightChanged(object sender, LedLightChangeEventArgs e)
        {
            try
            {

            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }
        
        public void DeviceAttached(object sender, PanelEventArgs e)
        {
            try
            {
                if (e.PanelType == GamingPanelEnum.FarmingPanel && e.HidInstance.Equals(_farmingSidePanel.HIDInstanceId))
                {
                    //Dispatcher?.BeginInvoke((Action)(() => _parentTabItem.Header = ParentTabItemHeader + " (connected)"));
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        public void DeviceDetached(object sender, PanelEventArgs e)
        {
            try
            {
                if (e.PanelType == GamingPanelEnum.FarmingPanel && e.HidInstance.Equals(_farmingSidePanel.HIDInstanceId))
                {
                    //Dispatcher?.BeginInvoke((Action)(() => _parentTabItem.Header = ParentTabItemHeader + " (disconnected)"));
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        public void SettingsApplied(object sender, PanelEventArgs e)
        {
            try
            {
                if (e.HidInstance.Equals(_farmingSidePanel.HIDInstanceId) && e.PanelType == GamingPanelEnum.FarmingPanel)
                {
                    Dispatcher?.BeginInvoke((Action)(ShowGraphicConfiguration));
                    Dispatcher?.BeginInvoke((Action)(() => TextBoxLogFarmingPanel.Text = string.Empty));
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        public void SettingsModified(object sender, PanelEventArgs e)
        {
            try
            {
                if (_farmingSidePanel.HIDInstanceId == e.HidInstance)
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
                    TextBoxLogFarmingPanel.Text = _farmingSidePanel.HIDInstanceId;
                    Clipboard.SetText(_farmingSidePanel.HIDInstanceId);
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
            try
            {
                if (textBox.Equals(TextBox1))
                {
                    return new FarmingPanelOnOff(FarmingPanelMKKeys.BUTTON_1, true);
                }
                if (textBox.Equals(TextBox2))
                {
                    return new FarmingPanelOnOff(FarmingPanelMKKeys.BUTTON_2, true);
                }
                if (textBox.Equals(TextBox3))
                {
                    return new FarmingPanelOnOff(FarmingPanelMKKeys.BUTTON_3, true);
                }
                if (textBox.Equals(TextBox4))
                {
                    return new FarmingPanelOnOff(FarmingPanelMKKeys.BUTTON_4, true);
                }
                if (textBox.Equals(TextBox5))
                {
                    return new FarmingPanelOnOff(FarmingPanelMKKeys.BUTTON_5, true);
                }
                if (textBox.Equals(TextBox6))
                {
                    return new FarmingPanelOnOff(FarmingPanelMKKeys.BUTTON_6, true);
                }
                if (textBox.Equals(TextBox7))
                {
                    return new FarmingPanelOnOff(FarmingPanelMKKeys.BUTTON_7, true);
                }
                if (textBox.Equals(TextBox8))
                {
                    return new FarmingPanelOnOff(FarmingPanelMKKeys.BUTTON_8, true);
                }
                if (textBox.Equals(TextBox9))
                {
                    return new FarmingPanelOnOff(FarmingPanelMKKeys.BUTTON_9, true);
                }
                if (textBox.Equals(TextBox10))
                {
                    return new FarmingPanelOnOff(FarmingPanelMKKeys.BUTTON_10, true);
                }
                if (textBox.Equals(TextBox11))
                {
                    return new FarmingPanelOnOff(FarmingPanelMKKeys.BUTTON_11, true);
                }
                if (textBox.Equals(TextBox12))
                {
                    return new FarmingPanelOnOff(FarmingPanelMKKeys.BUTTON_12, true);
                }
                if (textBox.Equals(TextBox13))
                {
                    return new FarmingPanelOnOff(FarmingPanelMKKeys.BUTTON_13, true);
                }
                if (textBox.Equals(TextBox14))
                {
                    return new FarmingPanelOnOff(FarmingPanelMKKeys.BUTTON_14, true);
                }
                if (textBox.Equals(TextBox15))
                {
                    return new FarmingPanelOnOff(FarmingPanelMKKeys.BUTTON_15, true);
                }
                if (textBox.Equals(TextBox16))
                {
                    return new FarmingPanelOnOff(FarmingPanelMKKeys.BUTTON_16, true);
                }
                if (textBox.Equals(TextBox17))
                {
                    return new FarmingPanelOnOff(FarmingPanelMKKeys.BUTTON_17, true);
                }
                if (textBox.Equals(TextBox18))
                {
                    return new FarmingPanelOnOff(FarmingPanelMKKeys.BUTTON_18, true);
                }
                if (textBox.Equals(TextBox19))
                {
                    return new FarmingPanelOnOff(FarmingPanelMKKeys.BUTTON_19, true);
                }
                if (textBox.Equals(TextBox20))
                {
                    return new FarmingPanelOnOff(FarmingPanelMKKeys.BUTTON_20, true);
                }
                if (textBox.Equals(TextBox21))
                {
                    return new FarmingPanelOnOff(FarmingPanelMKKeys.BUTTON_21, true);
                }
                if (textBox.Equals(TextBox22))
                {
                    return new FarmingPanelOnOff(FarmingPanelMKKeys.BUTTON_22, true);
                }
                if (textBox.Equals(TextBox23))
                {
                    return new FarmingPanelOnOff(FarmingPanelMKKeys.BUTTON_23, true);
                }
                if (textBox.Equals(TextBox24))
                {
                    return new FarmingPanelOnOff(FarmingPanelMKKeys.BUTTON_24, true);
                }
                if (textBox.Equals(TextBox25))
                {
                    return new FarmingPanelOnOff(FarmingPanelMKKeys.BUTTON_25, true);
                }
                if (textBox.Equals(TextBox26))
                {
                    return new FarmingPanelOnOff(FarmingPanelMKKeys.BUTTON_26, true);
                }
                if (textBox.Equals(TextBox27))
                {
                    return new FarmingPanelOnOff(FarmingPanelMKKeys.BUTTON_27, true);
                }
                if (textBox.Equals(TextBoxJoyLeft))
                {
                    return new FarmingPanelOnOff(FarmingPanelMKKeys.BUTTON_JOY_LEFT, true);
                }
                if (textBox.Equals(TextBoxJoyRight))
                {
                    return new FarmingPanelOnOff(FarmingPanelMKKeys.BUTTON_JOY_RIGHT, true);
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
            throw new Exception("Failed to find key based on text box (FarmingSidePanelUserControl) : " + textBox.Name);
        }



        public TextBox GetTextBox(object generalKey, bool whenTurnedOn)
        {
            var key = (FarmingPanelMKKeys)generalKey;
            try
            {
                if (key == FarmingPanelMKKeys.BUTTON_1 && whenTurnedOn)
                {
                    return TextBox1;
                }
                if (key == FarmingPanelMKKeys.BUTTON_2 && whenTurnedOn)
                {
                    return TextBox2;
                }
                if (key == FarmingPanelMKKeys.BUTTON_3 && whenTurnedOn)
                {
                    return TextBox3;
                }
                if (key == FarmingPanelMKKeys.BUTTON_4 && whenTurnedOn)
                {
                    return TextBox4;
                }
                if (key == FarmingPanelMKKeys.BUTTON_5 && whenTurnedOn)
                {
                    return TextBox5;
                }
                if (key == FarmingPanelMKKeys.BUTTON_6 && whenTurnedOn)
                {
                    return TextBox6;
                }
                if (key == FarmingPanelMKKeys.BUTTON_7 && whenTurnedOn)
                {
                    return TextBox7;
                }
                if (key == FarmingPanelMKKeys.BUTTON_8 && whenTurnedOn)
                {
                    return TextBox8;
                }
                if (key == FarmingPanelMKKeys.BUTTON_9 && whenTurnedOn)
                {
                    return TextBox9;
                }
                if (key == FarmingPanelMKKeys.BUTTON_10 && whenTurnedOn)
                {
                    return TextBox10;
                }
                if (key == FarmingPanelMKKeys.BUTTON_11 && whenTurnedOn)
                {
                    return TextBox11;
                }
                if (key == FarmingPanelMKKeys.BUTTON_12 && whenTurnedOn)
                {
                    return TextBox12;
                }
                if (key == FarmingPanelMKKeys.BUTTON_13 && whenTurnedOn)
                {
                    return TextBox13;
                }
                if (key == FarmingPanelMKKeys.BUTTON_14 && whenTurnedOn)
                {
                    return TextBox14;
                }
                if (key == FarmingPanelMKKeys.BUTTON_15 && whenTurnedOn)
                {
                    return TextBox15;
                }
                if (key == FarmingPanelMKKeys.BUTTON_16 && whenTurnedOn)
                {
                    return TextBox16;
                }
                if (key == FarmingPanelMKKeys.BUTTON_17 && whenTurnedOn)
                {
                    return TextBox17;
                }
                if (key == FarmingPanelMKKeys.BUTTON_18 && whenTurnedOn)
                {
                    return TextBox18;
                }
                if (key == FarmingPanelMKKeys.BUTTON_19 && whenTurnedOn)
                {
                    return TextBox19;
                }
                if (key == FarmingPanelMKKeys.BUTTON_20 && whenTurnedOn)
                {
                    return TextBox20;
                }
                if (key == FarmingPanelMKKeys.BUTTON_21 && whenTurnedOn)
                {
                    return TextBox21;
                }
                if (key == FarmingPanelMKKeys.BUTTON_22 && whenTurnedOn)
                {
                    return TextBox22;
                }
                if (key == FarmingPanelMKKeys.BUTTON_23 && whenTurnedOn)
                {
                    return TextBox23;
                }
                if (key == FarmingPanelMKKeys.BUTTON_24 && whenTurnedOn)
                {
                    return TextBox24;
                }
                if (key == FarmingPanelMKKeys.BUTTON_25 && whenTurnedOn)
                {
                    return TextBox25;
                }
                if (key == FarmingPanelMKKeys.BUTTON_26 && whenTurnedOn)
                {
                    return TextBox26;
                }
                if (key == FarmingPanelMKKeys.BUTTON_27 && whenTurnedOn)
                {
                    return TextBox27;
                }
                if (key == FarmingPanelMKKeys.BUTTON_JOY_LEFT && whenTurnedOn)
                {
                    return TextBoxJoyLeft;
                }
                if (key == FarmingPanelMKKeys.BUTTON_JOY_RIGHT && whenTurnedOn)
                {
                    return TextBoxJoyRight;
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
            throw new Exception("Failed to find text box based on key (SwitchPanelPZ55UserControl)" + key);
        }



        private void ButtonDEV_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var window = new KeyPressReadingWindow(KeyPressLength.SecondAndHalf, "ESC");
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
