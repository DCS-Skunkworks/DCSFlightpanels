﻿using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ClassLibraryCommon;
using DCS_BIOS;
using NonVisuals;
using DCSFlightpanels.Properties;

namespace DCSFlightpanels
{
    /// <summary>
    /// Interaction logic for RadioPanelPZ69UserControlSRS.xaml
    /// </summary>
    public partial class RadioPanelPZ69UserControlSRS : ISaitekPanelListener, IProfileHandlerListener, ISaitekUserControl
    {
        private readonly RadioPanelPZ69SRS _radioPanelPZ69;
        private TabItem _parentTabItem;
        private string _parentTabItemHeader;
        private IGlobalHandler _globalHandler;
        private bool _userControlLoaded;

        public RadioPanelPZ69UserControlSRS(HIDSkeleton hidSkeleton, TabItem parentTabItem, IGlobalHandler globalHandler)
        {
            InitializeComponent();
            _parentTabItem = parentTabItem;
            _parentTabItemHeader = _parentTabItem.Header.ToString();
            HideAllImages();

            _radioPanelPZ69 = new RadioPanelPZ69SRS(Settings.Default.SRSPortFrom, Settings.Default.SRSIpTo, Settings.Default.SRSPortTo, hidSkeleton);
            _radioPanelPZ69.FrequencyKnobSensitivity = Settings.Default.RadioFrequencyKnobSensitivity;
            _radioPanelPZ69.Attach((ISaitekPanelListener)this);
            globalHandler.Attach(_radioPanelPZ69);
            _globalHandler = globalHandler;

            //LoadConfiguration();
        }

        public SaitekPanel GetSaitekPanel()
        {
            return _radioPanelPZ69;
        }

        public string GetName()
        {
            return GetType().Name;
        }

        public void UpdatesHasBeenMissed(string uniqueId, SaitekPanelsEnum saitekPanelsEnum, int count)
        {
            try
            {
                //todo
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(471074, ex);
            }
        }

        public void SelectedAirframe(DCSAirframe dcsAirframe)
        {
            try
            {
                //nada
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(471173, ex);
            }
        }

        public void SwitchesChanged(string uniqueId, SaitekPanelsEnum saitekPanelsEnum, HashSet<object> hashSet)
        {
            try
            {
                SetGraphicsState(hashSet);
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(1064, ex);
            }
        }

        public void PanelSettingsReadFromFile(List<string> settings)
        {
            try
            {
                //todo
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(1081, ex);
            }
        }

        public void SettingsCleared(string uniqueId, SaitekPanelsEnum saitekPanelsEnum)
        {
            try
            {
                //todo
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(2001, ex);
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
                Common.ShowErrorMessageBox(2012, ex);
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
                Common.ShowErrorMessageBox(2013, ex);
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
                Common.ShowErrorMessageBox(2014, ex);
            }
        }

        public void SettingsApplied(string uniqueId, SaitekPanelsEnum saitekPanelsEnum)
        {
            try
            {
                //todo
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(2015, ex);
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
                Common.ShowErrorMessageBox(2016, ex);
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
                Common.ShowErrorMessageBox(2017, ex);
            }
        }

        private void SetGraphicsState(HashSet<object> knobs)
        {
            try
            {
                foreach (var radioKnobO in knobs)
                {
                    var radioKnob = (RadioPanelKnobSRS)radioKnobO;
                    Dispatcher.BeginInvoke((Action)delegate
                   {
                        /*if (radioKnob.IsOn)
                        {
                            SetGroupboxVisibility(radioKnob.RadioPanelPZ69Knob);
                        }*/
                   });
                    switch (radioKnob.RadioPanelPZ69Knob)
                    {
                        case RadioPanelPZ69KnobsSRS.UPPER_COM1:
                            {
                                var key = radioKnob;
                                Dispatcher.BeginInvoke(
                                    (Action)delegate
                                    {
                                        TopLeftCom1.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case RadioPanelPZ69KnobsSRS.UPPER_COM2:
                            {
                                var key = radioKnob;
                                Dispatcher.BeginInvoke(
                                    (Action)delegate
                                    {
                                        TopLeftCom2.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case RadioPanelPZ69KnobsSRS.UPPER_NAV1:
                            {
                                var key = radioKnob;
                                Dispatcher.BeginInvoke(
                                    (Action)delegate
                                    {
                                        TopLeftNav1.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case RadioPanelPZ69KnobsSRS.UPPER_NAV2:
                            {
                                var key = radioKnob;
                                Dispatcher.BeginInvoke(
                                    (Action)delegate
                                    {
                                        TopLeftNav2.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case RadioPanelPZ69KnobsSRS.UPPER_ADF:
                            {
                                var key = radioKnob;
                                Dispatcher.BeginInvoke(
                                    (Action)delegate
                                    {
                                        TopLeftADF.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case RadioPanelPZ69KnobsSRS.UPPER_DME:
                            {
                                var key = radioKnob;
                                Dispatcher.BeginInvoke(
                                    (Action)delegate
                                    {
                                        TopLeftDME.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case RadioPanelPZ69KnobsSRS.UPPER_XPDR:
                            {
                                var key = radioKnob;
                                Dispatcher.BeginInvoke(
                                    (Action)delegate
                                    {
                                        TopLeftXPDR.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case RadioPanelPZ69KnobsSRS.LOWER_COM1:
                            {
                                var key = radioKnob;
                                Dispatcher.BeginInvoke(
                                    (Action)delegate
                                    {
                                        LowerLeftCom1.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case RadioPanelPZ69KnobsSRS.LOWER_COM2:
                            {
                                var key = radioKnob;
                                Dispatcher.BeginInvoke(
                                    (Action)delegate
                                    {
                                        LowerLeftCom2.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case RadioPanelPZ69KnobsSRS.LOWER_NAV1:
                            {
                                var key = radioKnob;
                                Dispatcher.BeginInvoke(
                                    (Action)delegate
                                    {
                                        LowerLeftNav1.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case RadioPanelPZ69KnobsSRS.LOWER_NAV2:
                            {
                                var key = radioKnob;
                                Dispatcher.BeginInvoke(
                                    (Action)delegate
                                    {
                                        LowerLeftNav2.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case RadioPanelPZ69KnobsSRS.LOWER_ADF:
                            {
                                var key = radioKnob;
                                Dispatcher.BeginInvoke(
                                    (Action)delegate
                                    {
                                        LowerLeftADF.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case RadioPanelPZ69KnobsSRS.LOWER_DME:
                            {
                                var key = radioKnob;
                                Dispatcher.BeginInvoke(
                                    (Action)delegate
                                    {
                                        LowerLeftDME.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case RadioPanelPZ69KnobsSRS.LOWER_XPDR:
                            {
                                var key = radioKnob;
                                Dispatcher.BeginInvoke(
                                    (Action)delegate
                                    {
                                        LowerLeftXPDR.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case RadioPanelPZ69KnobsSRS.UPPER_SMALL_FREQ_WHEEL_INC:
                            {
                                var key = radioKnob;
                                Dispatcher.BeginInvoke(
                                    (Action)delegate
                                    {
                                        UpperSmallerLCDKnobInc.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case RadioPanelPZ69KnobsSRS.UPPER_SMALL_FREQ_WHEEL_DEC:
                            {
                                var key = radioKnob;
                                Dispatcher.BeginInvoke(
                                    (Action)delegate
                                    {
                                        UpperSmallerLCDKnobDec.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case RadioPanelPZ69KnobsSRS.UPPER_LARGE_FREQ_WHEEL_INC:
                            {
                                var key = radioKnob;
                                Dispatcher.BeginInvoke(
                                    (Action)delegate
                                    {
                                        UpperLargerLCDKnobInc.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case RadioPanelPZ69KnobsSRS.UPPER_LARGE_FREQ_WHEEL_DEC:
                            {
                                var key = radioKnob;
                                Dispatcher.BeginInvoke(
                                    (Action)delegate
                                    {
                                        UpperLargerLCDKnobDec.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case RadioPanelPZ69KnobsSRS.LOWER_SMALL_FREQ_WHEEL_INC:
                            {
                                var key = radioKnob;
                                Dispatcher.BeginInvoke(
                                    (Action)delegate
                                    {
                                        LowerSmallerLCDKnobInc.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case RadioPanelPZ69KnobsSRS.LOWER_SMALL_FREQ_WHEEL_DEC:
                            {
                                var key = radioKnob;
                                Dispatcher.BeginInvoke(
                                    (Action)delegate
                                    {
                                        LowerSmallerLCDKnobDec.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case RadioPanelPZ69KnobsSRS.LOWER_LARGE_FREQ_WHEEL_INC:
                            {
                                var key = radioKnob;
                                Dispatcher.BeginInvoke(
                                    (Action)delegate
                                    {
                                        LowerLargerLCDKnobInc.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case RadioPanelPZ69KnobsSRS.LOWER_LARGE_FREQ_WHEEL_DEC:
                            {
                                var key = radioKnob;
                                Dispatcher.BeginInvoke(
                                    (Action)delegate
                                    {
                                        LowerLargerLCDKnobDec.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case RadioPanelPZ69KnobsSRS.UPPER_FREQ_SWITCH:
                            {
                                var key = radioKnob;
                                Dispatcher.BeginInvoke(
                                    (Action)delegate
                                    {
                                        UpperRightSwitch.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case RadioPanelPZ69KnobsSRS.LOWER_FREQ_SWITCH:
                            {
                                var key = radioKnob;
                                Dispatcher.BeginInvoke(
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
                Common.ShowErrorMessageBox(2019, ex);
            }
        }
        /*
        private void SetGroupboxVisibility(RadioPanelPZ69KnobsSRS panelPZ69KnobsA10C)
        {
            try
            {
                GroupLowerSelectorKnobUHF.Visibility = panelPZ69KnobsA10C == RadioPanelPZ69KnobsSRS.UPPER_UHF ? Visibility.Visible : Visibility.Hidden;
                GroupLowerSelectorKnobVHFFM.Visibility = panelPZ69KnobsA10C == RadioPanelPZ69KnobsSRS.UPPER_VHFFM ? Visibility.Visible : Visibility.Hidden;
                GroupLowerSelectorKnobVHFAM.Visibility = panelPZ69KnobsA10C == RadioPanelPZ69KnobsSRS.UPPER_VHFAM ? Visibility.Visible : Visibility.Hidden;
                GroupLowerSelectorKnobTACAN.Visibility = panelPZ69KnobsA10C == RadioPanelPZ69KnobsSRS.UPPER_TACAN ? Visibility.Visible : Visibility.Hidden;
                GroupLowerSelectorKnobILS.Visibility = panelPZ69KnobsA10C == RadioPanelPZ69KnobsSRS.UPPER_ILS ? Visibility.Visible : Visibility.Hidden;
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(129814, ex);
            }
        }
        */
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

        private void TextBoxShortcutKeyDown(object sender, KeyEventArgs e)
        {


        }

        private void TextBoxGotFocus(object sender, RoutedEventArgs e)
        {


        }

        private void TextBoxLostFocus(object sender, RoutedEventArgs e)
        {


        }

        private void TextBoxMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {


        }

        private void TextBoxPreviewKeyDown(object sender, KeyEventArgs e)
        {



        }


        private void TextBoxTextChanged(object sender, TextChangedEventArgs e)
        {

        }



        private void ButtonGetId_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_radioPanelPZ69 != null)
                {
                    Clipboard.SetText(_radioPanelPZ69.InstanceId);
                    MessageBox.Show("The Instance Id for the panel has been copied to the Clipboard.");
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(2030, ex);
            }
        }

        private void ComboBoxFreqKnobSensitivity_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (_userControlLoaded)
                {
                    Settings.Default.RadioFrequencyKnobSensitivity = int.Parse(ComboBoxFreqKnobSensitivity.SelectedValue.ToString());
                    _radioPanelPZ69.FrequencyKnobSensitivity = int.Parse(ComboBoxFreqKnobSensitivity.SelectedValue.ToString());
                    Settings.Default.Save();
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(204330, ex);
            }
        }

        private void RadioPanelPZ69UserControlSRS_OnLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                ComboBoxFreqKnobSensitivity.SelectedValue = Settings.Default.RadioFrequencyKnobSensitivity;
                ComboBoxSyncOKDelayTimeout.SelectedValue = Settings.Default.SyncOKDelayTimeout;
                /*ComboBoxSynchSleepTime.SelectedValue = Settings.Default.BAKDialSynchSleepTime;
                ComboBoxSynchResetTimeout.SelectedValue = Settings.Default.BAKDialResetSyncTimeout;
                _radioPanelPZ69.ResetSyncTimeout = Settings.Default.BAKDialResetSyncTimeout;
                _radioPanelPZ69.SynchSleepTime = Settings.Default.BAKDialSynchSleepTime;*/
                _userControlLoaded = true;
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(204331, ex);
            }
        }

        private void ComboBoxSynchSleepTime_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (_userControlLoaded)
                {
                    /*Settings.Default.BAKDialSynchSleepTime = int.Parse(ComboBoxSynchSleepTime.SelectedValue.ToString());
                    _radioPanelPZ69.SynchSleepTime = int.Parse(ComboBoxSynchSleepTime.SelectedValue.ToString());
                    Settings.Default.Save();*/
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(244331, ex);
            }
        }

        private void ComboBoxSynchResetTimeout_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (_userControlLoaded)
                {
                    /*Settings.Default.BAKDialResetSyncTimeout = int.Parse(ComboBoxSynchResetTimeout.SelectedValue.ToString());
                    _radioPanelPZ69.ResetSyncTimeout = int.Parse(ComboBoxSynchResetTimeout.SelectedValue.ToString());
                    Settings.Default.Save();*/
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(244331, ex);
            }
        }

        private void ComboBoxSyncOKDelayTimeout_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (_userControlLoaded)
                {
                    Settings.Default.SyncOKDelayTimeout = int.Parse(ComboBoxSyncOKDelayTimeout.SelectedValue.ToString());
                    _radioPanelPZ69.SyncOKDelayTimeout = int.Parse(ComboBoxSyncOKDelayTimeout.SelectedValue.ToString());
                    Settings.Default.Save();
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(244331, ex);
            }
        }
    }
}
