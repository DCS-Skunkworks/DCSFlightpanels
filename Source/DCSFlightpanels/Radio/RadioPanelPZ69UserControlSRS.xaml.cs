using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using ClassLibraryCommon;
using NonVisuals;
using DCSFlightpanels.Properties;
using NonVisuals.Radio;

namespace DCSFlightpanels.Radio
{
    /// <summary>
    /// Interaction logic for RadioPanelPZ69UserControlSRS.xaml
    /// </summary>
    public partial class RadioPanelPZ69UserControlSRS : ISaitekPanelListener, IProfileHandlerListener, ISaitekUserControl
    {
        private readonly RadioPanelPZ69SRS _radioPanelPZ69;
        private readonly TabItem _parentTabItem;
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
            _radioPanelPZ69.SmallFreqStepping = Settings.Default.SRSSmallFreqStepping;
            _radioPanelPZ69.Attach((ISaitekPanelListener)this);
            globalHandler.Attach(_radioPanelPZ69);
            _globalHandler = globalHandler;

            //LoadConfiguration();
        }

        public void BipPanelRegisterEvent(object sender, BipPanelRegisteredEventArgs e)
        {
        }

        public SaitekPanel GetSaitekPanel()
        {
            return _radioPanelPZ69;
        }

        public string GetName()
        {
            return GetType().Name;
        }

        public void UpdatesHasBeenMissed(object sender, DCSBIOSUpdatesMissedEventArgs e)
        {
            try
            {
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(471074, ex);
            }
        }

        public void SelectedAirframe(object sender, AirframeEventArgs e)
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

        public void SwitchesChanged(object sender, SwitchesChangedEventArgs e)
        {
            try
            {
                SetGraphicsState(e.Switches);
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(1064, ex);
            }
        }

        public void PanelSettingsReadFromFile(object sender, SettingsReadFromFileEventArgs e)
        {
            try
            {
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(1081, ex);
            }
        }

        public void SettingsCleared(object sender, PanelEventArgs e)
        {
            try
            {
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(2001, ex);
            }
        }

        public void LedLightChanged(object sender, LedLightChangeEventArgs e)
        {
            try
            {
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(2012, ex);
            }
        }

        public void PanelDataAvailable(object sender, PanelDataToDCSBIOSEventEventArgs e)
        {
            try
            {
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(2013, ex);
            }
        }

        public void DeviceAttached(object sender, PanelEventArgs e)
        {
            try
            {
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(2014, ex);
            }
        }

        public void SettingsApplied(object sender, PanelEventArgs e)
        {
            try
            {
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(2015, ex);
            }
        }

        public void PanelSettingsChanged(object sender, PanelEventArgs e)
        {
            try
            {
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(2016, ex);
            }
        }

        public void DeviceDetached(object sender, PanelEventArgs e)
        {
            try
            {
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

        private void ComboBoxSmallFreqStepping_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (_userControlLoaded)
                {
                    var numberFormat = new NumberFormatInfo();
                    numberFormat.NumberDecimalSeparator = ".";
                    numberFormat.NumberDecimalDigits = 3;
                    numberFormat.NumberGroupSeparator = "";
                    Settings.Default.SRSSmallFreqStepping = double.Parse(ComboBoxSmallFreqStepping.SelectedValue.ToString(), numberFormat);
                    _radioPanelPZ69.SmallFreqStepping = double.Parse(ComboBoxSmallFreqStepping.SelectedValue.ToString(), numberFormat);
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
                ComboBoxSmallFreqStepping.SelectedValue = Settings.Default.SRSSmallFreqStepping;
                _userControlLoaded = true;
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(204331, ex);
            }
        }



    }
}
