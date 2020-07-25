using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using ClassLibraryCommon;
using DCSFlightpanels.Interfaces;
using DCSFlightpanels.PanelUserControls;
using NonVisuals;
using DCSFlightpanels.Properties;
using NonVisuals.Interfaces;
using NonVisuals.Radios;
using NonVisuals.Saitek;


namespace DCSFlightpanels.Radios
{
    /// <summary>
    /// Interaction logic for RadioPanelPZ69UserControlKa50.xaml
    /// </summary>
    public partial class RadioPanelPZ69UserControlKa50 : UserControlBase, IGamingPanelListener, IProfileHandlerListener, IGamingPanelUserControl
    {
        private readonly RadioPanelPZ69Ka50 _radioPanelPZ69;

        public RadioPanelPZ69UserControlKa50(HIDSkeleton hidSkeleton, TabItem parentTabItem, IGlobalHandler globalHandler)
        {
            InitializeComponent();
            ParentTabItem = parentTabItem;

            hidSkeleton.HIDReadDevice.Removed += DeviceRemovedHandler;

            HideAllImages();
            _radioPanelPZ69 = new RadioPanelPZ69Ka50(hidSkeleton);
            _radioPanelPZ69.FrequencyKnobSensitivity = Settings.Default.RadioFrequencyKnobSensitivity;
            _radioPanelPZ69.Attach((IGamingPanelListener)this);
            globalHandler.Attach(_radioPanelPZ69);
            GlobalHandler = globalHandler;

            //LoadConfiguration();
        }

        public void BipPanelRegisterEvent(object sender, BipPanelRegisteredEventArgs e)
        {}

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

        public void SelectedAirframe(object sender, AirframeEventArgs e) { }

        public void UISwitchesChanged(object sender, SwitchesChangedEventArgs e)
        {
            try
            {
                SetGraphicsState(e.Switches);
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox( ex);
            }
        }

        public void PanelBindingReadFromFile(object sender, PanelBindingReadFromFileEventArgs e){}

        public void SettingsCleared(object sender, PanelEventArgs e) { }

        public void LedLightChanged(object sender, LedLightChangeEventArgs e) { }

        public void PanelDataAvailable(object sender, PanelDataToDCSBIOSEventEventArgs e) { }

        public void DeviceAttached(object sender, PanelEventArgs e) { }

        public void SettingsApplied(object sender, PanelEventArgs e) { }

        public void PanelSettingsChanged(object sender, PanelEventArgs e) { }

        public void DeviceDetached(object sender, PanelEventArgs e) { }

        private void SetGraphicsState(HashSet<object> knobs)
        {
            try
            {
                foreach (var radioKnobO in knobs)
                {
                    var radioKnob = (RadioPanelKnobKa50)radioKnobO;
                    Dispatcher?.BeginInvoke((Action)delegate
                    {
                        /*if (radioKnob.IsOn)
                        {
                            SetGroupboxVisibility(radioKnob.RadioPanelPZ69Knob);
                        }*/
                    });
                    switch (radioKnob.RadioPanelPZ69Knob)
                    {
                        case RadioPanelPZ69KnobsKa50.UPPER_VHF1_R828:
                            {
                                var key = radioKnob;
                                Dispatcher?.BeginInvoke(
                                    (Action)delegate
                                    {
                                        TopLeftCom1.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case RadioPanelPZ69KnobsKa50.UPPER_VHF2_R800L1:
                            {
                                var key = radioKnob;
                                Dispatcher?.BeginInvoke(
                                    (Action)delegate
                                    {
                                        TopLeftCom2.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case RadioPanelPZ69KnobsKa50.UPPER_ABRIS:
                            {
                                var key = radioKnob;
                                Dispatcher?.BeginInvoke(
                                    (Action)delegate
                                    {
                                        TopLeftNav1.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case RadioPanelPZ69KnobsKa50.UPPER_DATALINK:
                            {
                                var key = radioKnob;
                                Dispatcher?.BeginInvoke(
                                    (Action)delegate
                                    {
                                        TopLeftNav2.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case RadioPanelPZ69KnobsKa50.UPPER_ADF_ARK22:
                            {
                                var key = radioKnob;
                                Dispatcher?.BeginInvoke(
                                    (Action)delegate
                                    {
                                        TopLeftADF.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case RadioPanelPZ69KnobsKa50.UPPER_NO_USE3:
                            {
                                var key = radioKnob;
                                Dispatcher?.BeginInvoke(
                                    (Action)delegate
                                    {
                                        TopLeftDME.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case RadioPanelPZ69KnobsKa50.UPPER_NO_USE4:
                            {
                                var key = radioKnob;
                                Dispatcher?.BeginInvoke(
                                    (Action)delegate
                                    {
                                        TopLeftXPDR.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case RadioPanelPZ69KnobsKa50.LOWER_VHF1_R828:
                            {
                                var key = radioKnob;
                                Dispatcher?.BeginInvoke(
                                    (Action)delegate
                                    {
                                        LowerLeftCom1.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case RadioPanelPZ69KnobsKa50.LOWER_VHF2_R800L1:
                            {
                                var key = radioKnob;
                                Dispatcher?.BeginInvoke(
                                    (Action)delegate
                                    {
                                        LowerLeftCom2.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case RadioPanelPZ69KnobsKa50.LOWER_ABRIS:
                            {
                                var key = radioKnob;
                                Dispatcher?.BeginInvoke(
                                    (Action)delegate
                                    {
                                        LowerLeftNav1.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case RadioPanelPZ69KnobsKa50.LOWER_DATALINK:
                            {
                                var key = radioKnob;
                                Dispatcher?.BeginInvoke(
                                    (Action)delegate
                                    {
                                        LowerLeftNav2.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case RadioPanelPZ69KnobsKa50.LOWER_ADF_ARK22:
                            {
                                var key = radioKnob;
                                Dispatcher?.BeginInvoke(
                                    (Action)delegate
                                    {
                                        LowerLeftADF.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case RadioPanelPZ69KnobsKa50.LOWER_NO_USE3:
                            {
                                var key = radioKnob;
                                Dispatcher?.BeginInvoke(
                                    (Action)delegate
                                    {
                                        LowerLeftDME.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case RadioPanelPZ69KnobsKa50.LOWER_NO_USE4:
                            {
                                var key = radioKnob;
                                Dispatcher?.BeginInvoke(
                                    (Action)delegate
                                    {
                                        LowerLeftXPDR.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case RadioPanelPZ69KnobsKa50.UPPER_SMALL_FREQ_WHEEL_INC:
                            {
                                var key = radioKnob;
                                Dispatcher?.BeginInvoke(
                                    (Action)delegate
                                    {
                                        UpperSmallerLCDKnobInc.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case RadioPanelPZ69KnobsKa50.UPPER_SMALL_FREQ_WHEEL_DEC:
                            {
                                var key = radioKnob;
                                Dispatcher?.BeginInvoke(
                                    (Action)delegate
                                    {
                                        UpperSmallerLCDKnobDec.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case RadioPanelPZ69KnobsKa50.UPPER_LARGE_FREQ_WHEEL_INC:
                            {
                                var key = radioKnob;
                                Dispatcher?.BeginInvoke(
                                    (Action)delegate
                                    {
                                        UpperLargerLCDKnobInc.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case RadioPanelPZ69KnobsKa50.UPPER_LARGE_FREQ_WHEEL_DEC:
                            {
                                var key = radioKnob;
                                Dispatcher?.BeginInvoke(
                                    (Action)delegate
                                    {
                                        UpperLargerLCDKnobDec.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case RadioPanelPZ69KnobsKa50.LOWER_SMALL_FREQ_WHEEL_INC:
                            {
                                var key = radioKnob;
                                Dispatcher?.BeginInvoke(
                                    (Action)delegate
                                    {
                                        LowerSmallerLCDKnobInc.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case RadioPanelPZ69KnobsKa50.LOWER_SMALL_FREQ_WHEEL_DEC:
                            {
                                var key = radioKnob;
                                Dispatcher?.BeginInvoke(
                                    (Action)delegate
                                    {
                                        LowerSmallerLCDKnobDec.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case RadioPanelPZ69KnobsKa50.LOWER_LARGE_FREQ_WHEEL_INC:
                            {
                                var key = radioKnob;
                                Dispatcher?.BeginInvoke(
                                    (Action)delegate
                                    {
                                        LowerLargerLCDKnobInc.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case RadioPanelPZ69KnobsKa50.LOWER_LARGE_FREQ_WHEEL_DEC:
                            {
                                var key = radioKnob;
                                Dispatcher?.BeginInvoke(
                                    (Action)delegate
                                    {
                                        LowerLargerLCDKnobDec.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case RadioPanelPZ69KnobsKa50.UPPER_FREQ_SWITCH:
                            {
                                var key = radioKnob;
                                Dispatcher?.BeginInvoke(
                                    (Action)delegate
                                    {
                                        UpperRightSwitch.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case RadioPanelPZ69KnobsKa50.LOWER_FREQ_SWITCH:
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
                Common.ShowErrorMessageBox( ex);
            }
        }
        private void RadioPanelPZ69UserControlKa50_OnLoadedRadioPanelPZ69UserControlKa50_OnLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                ComboBoxFreqKnobSensitivity.SelectedValue = Settings.Default.RadioFrequencyKnobSensitivity;
                ComboBoxSyncOKDelayTimeout.SelectedValue = Settings.Default.SyncOKDelayTimeout;
                /*ComboBoxSynchSleepTime.SelectedValue = Settings.Default.BAKDialSynchSleepTime;
                ComboBoxSynchResetTimeout.SelectedValue = Settings.Default.BAKDialResetSyncTimeout;
                _radioPanelPZ69.ResetSyncTimeout = Settings.Default.BAKDialResetSyncTimeout;
                _radioPanelPZ69.SynchSleepTime = Settings.Default.BAKDialSynchSleepTime;*/
                UserControlLoaded = true;
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox( ex);
            }
        }

        private void ButtonGetId_OnClickButtonGetId_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_radioPanelPZ69 != null)
                {
                    TextBoxLogPZ69.Text = "";
                    TextBoxLogPZ69.Text = _radioPanelPZ69.HIDInstanceId;
                    Clipboard.SetText(_radioPanelPZ69.HIDInstanceId);
                    MessageBox.Show("The Instance Id for the panel has been copied to the Clipboard.");
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox( ex);
            }
        }

        private void ComboBoxSyncOKDelayTimeout_OnSelectionChangedComboBoxSyncOKDelayTimeout_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (UserControlLoaded)
                {
                    Settings.Default.SyncOKDelayTimeout = int.Parse(ComboBoxSyncOKDelayTimeout.SelectedValue.ToString());
                    _radioPanelPZ69.SyncOKDelayTimeout = int.Parse(ComboBoxSyncOKDelayTimeout.SelectedValue.ToString());
                    Settings.Default.Save();
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox( ex);
            }
        }

        private void ComboBoxFreqKnobSensitivity_OnSelectionChangedComboBoxFreqKnobSensitivity_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (UserControlLoaded)
                {
                    Settings.Default.RadioFrequencyKnobSensitivity = int.Parse(ComboBoxFreqKnobSensitivity.SelectedValue.ToString());
                    _radioPanelPZ69.FrequencyKnobSensitivity = int.Parse(ComboBoxFreqKnobSensitivity.SelectedValue.ToString());
                    Settings.Default.Save();
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox( ex);
            }
        }

        private void ComboBoxSynchSleepTime_OnSelectionChangedComboBoxSynchSleepTime_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (UserControlLoaded)
                {
                    /*Settings.Default.BAKDialSynchSleepTime = int.Parse(ComboBoxSynchSleepTime.SelectedValue.ToString());
                    _radioPanelPZ69.SynchSleepTime = int.Parse(ComboBoxSynchSleepTime.SelectedValue.ToString());
                    Settings.Default.Save();*/
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox( ex);
            }
        }

        private void ComboBoxSynchResetTimeout_OnSelectionChangedComboBoxSynchResetTimeout_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (UserControlLoaded)
                {
                    /*Settings.Default.BAKDialResetSyncTimeout = int.Parse(ComboBoxSynchResetTimeout.SelectedValue.ToString());
                    _radioPanelPZ69.ResetSyncTimeout = int.Parse(ComboBoxSynchResetTimeout.SelectedValue.ToString());
                    Settings.Default.Save();*/
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox( ex);
            }
        }


        private void HideAllImages()
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
