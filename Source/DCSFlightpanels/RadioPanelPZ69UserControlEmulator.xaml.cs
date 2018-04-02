using System;
using System.Collections.Generic;
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
    /// Interaction logic for RadioPanelPZ69UserControlEmulator.xaml
    /// </summary>
    public partial class RadioPanelPZ69UserControlEmulator : ISaitekPanelListener, IProfileHandlerListener, ISaitekUserControl
    {
        private readonly RadioPanelPZ69Emulator _radioPanelPZ69;
        private TabItem _parentTabItem;
        private string _parentTabItemHeader;
        private IGlobalHandler _globalHandler;
        private bool _userControlLoaded;
        private List<Key> _allowedKeys = new List<Key>() { Key.D0, Key.D1, Key.D2, Key.D3, Key.D4, Key.D5, Key.D6, Key.D7, Key.D8, Key.D9, Key.OemPeriod, Key.Delete, Key.Back, Key.Left, Key.Right, Key.NumPad0, Key.NumPad1, Key.NumPad2, Key.NumPad3, Key.NumPad4, Key.NumPad5, Key.NumPad6, Key.NumPad7, Key.NumPad8, Key.NumPad9 };

        public RadioPanelPZ69UserControlEmulator(HIDSkeleton hidSkeleton, TabItem parentTabItem, IGlobalHandler globalHandler)
        {
            InitializeComponent();
            _parentTabItem = parentTabItem;
            _parentTabItemHeader = _parentTabItem.Header.ToString();
            HideAllImages();

            _radioPanelPZ69 = new RadioPanelPZ69Emulator(hidSkeleton);
            _radioPanelPZ69.FrequencyKnobSensitivity = Settings.Default.RadioFrequencyKnobSensitivityEmulator;
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
                if (saitekPanelsEnum == SaitekPanelsEnum.PZ69RadioPanel && uniqueId.Equals(_radioPanelPZ69.InstanceId))
                {
                    NotifySwitchChanges(hashSet);
                }
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

        public void SettingsApplied(string uniqueId, SaitekPanelsEnum saitekPanelsEnum)
        {
            try
            {
                if (uniqueId.Equals(_radioPanelPZ69.InstanceId) && saitekPanelsEnum == SaitekPanelsEnum.PZ69RadioPanel)
                {
                    Dispatcher.BeginInvoke((Action)(ShowGraphicConfiguration));
                    Dispatcher.BeginInvoke((Action)(() => TextBoxLogPZ69.Text = ""));
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(2032, ex);
            }
        }

        public void PanelSettingsChanged(string uniqueId, SaitekPanelsEnum saitekPanelsEnum)
        {
            try
            {
                Dispatcher.BeginInvoke((Action)(ShowGraphicConfiguration));
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(2010, ex);
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
                Common.ShowErrorMessageBox(2014, ex);
            }
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
                if (textBox.Tag is SortedList<int, KeyPressInfo>)
                {
                    sequenceWindow = new SequenceWindow(textBox.Text, (SortedList<int, KeyPressInfo>)textBox.Tag);
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
                    textBox.ToolTip = null;
                    if (sequenceList.Count > 1)
                    {
                        textBox.Tag = sequenceList;
                        textBox.Text = string.IsNullOrEmpty(sequenceWindow.GetInformation) ? "Key press sequence" : sequenceWindow.GetInformation;
                        if (!string.IsNullOrEmpty(sequenceWindow.GetInformation))
                        {
                            var toolTip = new ToolTip { Content = sequenceWindow.GetInformation };
                            textBox.ToolTip = toolTip;
                        }
                        UpdateKeyBindingProfileSequencedKeyStrokesPZ69(textBox);
                    }
                    else
                    {
                        //If only one press was created treat it as a simple keypress
                        textBox.Tag = sequenceList.Values[0].LengthOfKeyPress;
                        textBox.Text = sequenceList.Values[0].VirtualKeyCodesAsString;
                        if (!string.IsNullOrEmpty(sequenceWindow.GetInformation))
                        {
                            var toolTip = new ToolTip { Content = sequenceWindow.GetInformation };
                            textBox.ToolTip = toolTip;
                        }
                        UpdateKeyBindingProfileSimpleKeyStrokes(textBox);
                    }
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(2044, ex);
            }
        }

        private void UpdateKeyBindingProfileSequencedKeyStrokesPZ69(TextBox textBox)
        {
            try
            {
                if (textBox.Tag == null)
                {
                    textBox.Tag = new SortedList<int, KeyPressInfo>();
                }


                if (textBox.Equals(TextBoxUpperCom1))
                {
                    _radioPanelPZ69.AddOrUpdateSequencedKeyBinding(textBox.Text, RadioPanelPZ69KnobsEmulator.UpperCOM1, (SortedList<int, KeyPressInfo>)textBox.Tag);
                }
                if (textBox.Equals(TextBoxUpperCom2))
                {
                    _radioPanelPZ69.AddOrUpdateSequencedKeyBinding(textBox.Text, RadioPanelPZ69KnobsEmulator.UpperCOM2, (SortedList<int, KeyPressInfo>)textBox.Tag);
                }
                if (textBox.Equals(TextBoxUpperNav1))
                {
                    _radioPanelPZ69.AddOrUpdateSequencedKeyBinding(textBox.Text, RadioPanelPZ69KnobsEmulator.UpperNAV1, (SortedList<int, KeyPressInfo>)textBox.Tag);
                }
                if (textBox.Equals(TextBoxUpperNav2))
                {
                    _radioPanelPZ69.AddOrUpdateSequencedKeyBinding(textBox.Text, RadioPanelPZ69KnobsEmulator.UpperNAV2, (SortedList<int, KeyPressInfo>)textBox.Tag);
                }
                if (textBox.Equals(TextBoxUpperADF))
                {
                    _radioPanelPZ69.AddOrUpdateSequencedKeyBinding(textBox.Text, RadioPanelPZ69KnobsEmulator.UpperADF, (SortedList<int, KeyPressInfo>)textBox.Tag);
                }
                if (textBox.Equals(TextBoxUpperDME))
                {
                    _radioPanelPZ69.AddOrUpdateSequencedKeyBinding(textBox.Text, RadioPanelPZ69KnobsEmulator.UpperDME, (SortedList<int, KeyPressInfo>)textBox.Tag);
                }
                if (textBox.Equals(TextBoxUpperXPDR))
                {
                    _radioPanelPZ69.AddOrUpdateSequencedKeyBinding(textBox.Text, RadioPanelPZ69KnobsEmulator.UpperXPDR, (SortedList<int, KeyPressInfo>)textBox.Tag);
                }
                if (textBox.Equals(TextBoxLowerCom1))
                {
                    _radioPanelPZ69.AddOrUpdateSequencedKeyBinding(textBox.Text, RadioPanelPZ69KnobsEmulator.LowerCOM1, (SortedList<int, KeyPressInfo>)textBox.Tag);
                }
                if (textBox.Equals(TextBoxLowerCom2))
                {
                    _radioPanelPZ69.AddOrUpdateSequencedKeyBinding(textBox.Text, RadioPanelPZ69KnobsEmulator.LowerCOM2, (SortedList<int, KeyPressInfo>)textBox.Tag);
                }
                if (textBox.Equals(TextBoxLowerNav1))
                {
                    _radioPanelPZ69.AddOrUpdateSequencedKeyBinding(textBox.Text, RadioPanelPZ69KnobsEmulator.UpperNAV1, (SortedList<int, KeyPressInfo>)textBox.Tag);
                }
                if (textBox.Equals(TextBoxLowerNav2))
                {
                    _radioPanelPZ69.AddOrUpdateSequencedKeyBinding(textBox.Text, RadioPanelPZ69KnobsEmulator.LowerNAV2, (SortedList<int, KeyPressInfo>)textBox.Tag);
                }
                if (textBox.Equals(TextBoxLowerADF))
                {
                    _radioPanelPZ69.AddOrUpdateSequencedKeyBinding(textBox.Text, RadioPanelPZ69KnobsEmulator.LowerADF, (SortedList<int, KeyPressInfo>)textBox.Tag);
                }
                if (textBox.Equals(TextBoxLowerDME))
                {
                    _radioPanelPZ69.AddOrUpdateSequencedKeyBinding(textBox.Text, RadioPanelPZ69KnobsEmulator.LowerDME, (SortedList<int, KeyPressInfo>)textBox.Tag);
                }
                if (textBox.Equals(TextBoxLowerXPDR))
                {
                    _radioPanelPZ69.AddOrUpdateSequencedKeyBinding(textBox.Text, RadioPanelPZ69KnobsEmulator.LowerXPDR, (SortedList<int, KeyPressInfo>)textBox.Tag);
                }



                if (textBox.Equals(TextBoxUpperLargePlus))
                {
                    _radioPanelPZ69.AddOrUpdateSequencedKeyBinding(textBox.Text, RadioPanelPZ69KnobsEmulator.UpperLargeFreqWheelInc, (SortedList<int, KeyPressInfo>)textBox.Tag);
                }
                if (textBox.Equals(TextBoxUpperLargeMinus))
                {
                    _radioPanelPZ69.AddOrUpdateSequencedKeyBinding(textBox.Text, RadioPanelPZ69KnobsEmulator.UpperLargeFreqWheelDec, (SortedList<int, KeyPressInfo>)textBox.Tag);
                }
                if (textBox.Equals(TextBoxUpperSmallPlus))
                {
                    _radioPanelPZ69.AddOrUpdateSequencedKeyBinding(textBox.Text, RadioPanelPZ69KnobsEmulator.UpperSmallFreqWheelInc, (SortedList<int, KeyPressInfo>)textBox.Tag);
                }
                if (textBox.Equals(TextBoxUpperSmallMinus))
                {
                    _radioPanelPZ69.AddOrUpdateSequencedKeyBinding(textBox.Text, RadioPanelPZ69KnobsEmulator.UpperSmallFreqWheelDec, (SortedList<int, KeyPressInfo>)textBox.Tag);
                }
                if (textBox.Equals(TextBoxUpperActStbyOn))
                {
                    _radioPanelPZ69.AddOrUpdateSequencedKeyBinding(textBox.Text, RadioPanelPZ69KnobsEmulator.UpperFreqSwitch, (SortedList<int, KeyPressInfo>)textBox.Tag);
                }
                if (textBox.Equals(TextBoxUpperActStbyOff))
                {
                    _radioPanelPZ69.AddOrUpdateSequencedKeyBinding(textBox.Text, RadioPanelPZ69KnobsEmulator.UpperFreqSwitch, (SortedList<int, KeyPressInfo>)textBox.Tag, false);
                }
                if (textBox.Equals(TextBoxLowerLargePlus))
                {
                    _radioPanelPZ69.AddOrUpdateSequencedKeyBinding(textBox.Text, RadioPanelPZ69KnobsEmulator.LowerLargeFreqWheelInc, (SortedList<int, KeyPressInfo>)textBox.Tag);
                }
                if (textBox.Equals(TextBoxLowerLargeMinus))
                {
                    _radioPanelPZ69.AddOrUpdateSequencedKeyBinding(textBox.Text, RadioPanelPZ69KnobsEmulator.LowerLargeFreqWheelDec, (SortedList<int, KeyPressInfo>)textBox.Tag);
                }
                if (textBox.Equals(TextBoxLowerSmallPlus))
                {
                    _radioPanelPZ69.AddOrUpdateSequencedKeyBinding(textBox.Text, RadioPanelPZ69KnobsEmulator.LowerSmallFreqWheelInc, (SortedList<int, KeyPressInfo>)textBox.Tag);
                }
                if (textBox.Equals(TextBoxLowerLargeMinus))
                {
                    _radioPanelPZ69.AddOrUpdateSequencedKeyBinding(textBox.Text, RadioPanelPZ69KnobsEmulator.LowerSmallFreqWheelDec, (SortedList<int, KeyPressInfo>)textBox.Tag);
                }
                if (textBox.Equals(TextBoxLowerActStbyOn))
                {
                    _radioPanelPZ69.AddOrUpdateSequencedKeyBinding(textBox.Text, RadioPanelPZ69KnobsEmulator.LowerFreqSwitch, (SortedList<int, KeyPressInfo>)textBox.Tag);
                }
                if (textBox.Equals(TextBoxLowerActStbyOff))
                {
                    _radioPanelPZ69.AddOrUpdateSequencedKeyBinding(textBox.Text, RadioPanelPZ69KnobsEmulator.LowerFreqSwitch, (SortedList<int, KeyPressInfo>)textBox.Tag, false);
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
                if (textBox.Tag == null)
                {
                    keyPressLength = KeyPressLength.FiftyMilliSec;
                }
                else
                {
                    keyPressLength = ((KeyPressLength)textBox.Tag);
                }
                if (textBox.Equals(TextBoxUpperCom1))
                {
                    _radioPanelPZ69.AddOrUpdateSingleKeyBinding(RadioPanelPZ69KnobsEmulator.UpperCOM1, TextBoxUpperCom1.Text, keyPressLength);
                }
                if (textBox.Equals(TextBoxUpperCom2))
                {
                    _radioPanelPZ69.AddOrUpdateSingleKeyBinding(RadioPanelPZ69KnobsEmulator.UpperCOM2, TextBoxUpperCom2.Text, keyPressLength);
                }
                if (textBox.Equals(TextBoxUpperNav1))
                {
                    _radioPanelPZ69.AddOrUpdateSingleKeyBinding(RadioPanelPZ69KnobsEmulator.UpperNAV1, TextBoxUpperNav1.Text, keyPressLength);
                }
                if (textBox.Equals(TextBoxUpperNav2))
                {
                    _radioPanelPZ69.AddOrUpdateSingleKeyBinding(RadioPanelPZ69KnobsEmulator.UpperNAV2, TextBoxUpperNav2.Text, keyPressLength);
                }
                if (textBox.Equals(TextBoxUpperADF))
                {
                    _radioPanelPZ69.AddOrUpdateSingleKeyBinding(RadioPanelPZ69KnobsEmulator.UpperADF, TextBoxUpperADF.Text, keyPressLength);
                }
                if (textBox.Equals(TextBoxUpperDME))
                {
                    _radioPanelPZ69.AddOrUpdateSingleKeyBinding(RadioPanelPZ69KnobsEmulator.UpperDME, TextBoxUpperDME.Text, keyPressLength);
                }
                if (textBox.Equals(TextBoxUpperXPDR))
                {
                    _radioPanelPZ69.AddOrUpdateSingleKeyBinding(RadioPanelPZ69KnobsEmulator.UpperXPDR, TextBoxUpperXPDR.Text, keyPressLength);
                }
                if (textBox.Equals(TextBoxUpperLargePlus))
                {
                    _radioPanelPZ69.AddOrUpdateSingleKeyBinding(RadioPanelPZ69KnobsEmulator.UpperLargeFreqWheelInc, TextBoxUpperLargePlus.Text, keyPressLength);
                }
                if (textBox.Equals(TextBoxUpperLargeMinus))
                {
                    _radioPanelPZ69.AddOrUpdateSingleKeyBinding(RadioPanelPZ69KnobsEmulator.UpperLargeFreqWheelDec, TextBoxUpperLargeMinus.Text, keyPressLength);
                }
                if (textBox.Equals(TextBoxUpperSmallPlus))
                {
                    _radioPanelPZ69.AddOrUpdateSingleKeyBinding(RadioPanelPZ69KnobsEmulator.UpperSmallFreqWheelInc, TextBoxUpperSmallPlus.Text, keyPressLength);
                }
                if (textBox.Equals(TextBoxUpperSmallMinus))
                {
                    _radioPanelPZ69.AddOrUpdateSingleKeyBinding(RadioPanelPZ69KnobsEmulator.UpperSmallFreqWheelDec, TextBoxUpperSmallMinus.Text, keyPressLength);
                }
                if (textBox.Equals(TextBoxUpperActStbyOn))
                {
                    _radioPanelPZ69.AddOrUpdateSingleKeyBinding(RadioPanelPZ69KnobsEmulator.UpperFreqSwitch, TextBoxUpperActStbyOn.Text, keyPressLength);
                }
                if (textBox.Equals(TextBoxUpperActStbyOff))
                {
                    _radioPanelPZ69.AddOrUpdateSingleKeyBinding(RadioPanelPZ69KnobsEmulator.UpperFreqSwitch, TextBoxUpperActStbyOff.Text, keyPressLength, false);
                }
                if (textBox.Equals(TextBoxLowerCom1))
                {
                    _radioPanelPZ69.AddOrUpdateSingleKeyBinding(RadioPanelPZ69KnobsEmulator.LowerCOM1, TextBoxLowerCom1.Text, keyPressLength);
                }
                if (textBox.Equals(TextBoxLowerCom2))
                {
                    _radioPanelPZ69.AddOrUpdateSingleKeyBinding(RadioPanelPZ69KnobsEmulator.LowerCOM2, TextBoxLowerCom2.Text, keyPressLength);
                }
                if (textBox.Equals(TextBoxLowerNav1))
                {
                    _radioPanelPZ69.AddOrUpdateSingleKeyBinding(RadioPanelPZ69KnobsEmulator.LowerNAV1, TextBoxLowerNav1.Text, keyPressLength);
                }
                if (textBox.Equals(TextBoxLowerNav2))
                {
                    _radioPanelPZ69.AddOrUpdateSingleKeyBinding(RadioPanelPZ69KnobsEmulator.LowerNAV2, TextBoxLowerNav2.Text, keyPressLength);
                }
                if (textBox.Equals(TextBoxLowerADF))
                {
                    _radioPanelPZ69.AddOrUpdateSingleKeyBinding(RadioPanelPZ69KnobsEmulator.LowerADF, TextBoxLowerADF.Text, keyPressLength);
                }
                if (textBox.Equals(TextBoxLowerDME))
                {
                    _radioPanelPZ69.AddOrUpdateSingleKeyBinding(RadioPanelPZ69KnobsEmulator.LowerDME, TextBoxLowerDME.Text, keyPressLength);
                }
                if (textBox.Equals(TextBoxLowerXPDR))
                {
                    _radioPanelPZ69.AddOrUpdateSingleKeyBinding(RadioPanelPZ69KnobsEmulator.LowerXPDR, TextBoxLowerXPDR.Text, keyPressLength);
                }
                if (textBox.Equals(TextBoxLowerLargePlus))
                {
                    _radioPanelPZ69.AddOrUpdateSingleKeyBinding(RadioPanelPZ69KnobsEmulator.LowerLargeFreqWheelInc, TextBoxLowerLargePlus.Text, keyPressLength);
                }
                if (textBox.Equals(TextBoxLowerLargeMinus))
                {
                    _radioPanelPZ69.AddOrUpdateSingleKeyBinding(RadioPanelPZ69KnobsEmulator.LowerLargeFreqWheelDec, TextBoxLowerLargeMinus.Text, keyPressLength);
                }
                if (textBox.Equals(TextBoxLowerSmallPlus))
                {
                    _radioPanelPZ69.AddOrUpdateSingleKeyBinding(RadioPanelPZ69KnobsEmulator.LowerSmallFreqWheelInc, TextBoxLowerSmallPlus.Text, keyPressLength);
                }
                if (textBox.Equals(TextBoxLowerSmallMinus))
                {
                    _radioPanelPZ69.AddOrUpdateSingleKeyBinding(RadioPanelPZ69KnobsEmulator.LowerSmallFreqWheelDec, TextBoxLowerSmallMinus.Text, keyPressLength);
                }
                if (textBox.Equals(TextBoxLowerActStbyOn))
                {
                    _radioPanelPZ69.AddOrUpdateSingleKeyBinding(RadioPanelPZ69KnobsEmulator.LowerFreqSwitch, TextBoxLowerActStbyOn.Text, keyPressLength);
                }
                if (textBox.Equals(TextBoxLowerActStbyOff))
                {
                    _radioPanelPZ69.AddOrUpdateSingleKeyBinding(RadioPanelPZ69KnobsEmulator.LowerFreqSwitch, TextBoxLowerActStbyOff.Text, keyPressLength, false);
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(3012, ex);
            }
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
                Common.ShowErrorMessageBox(3012, ex);
            }
        }

        private void TextBoxContextMenuIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            try
            {
                if (!(bool)e.NewValue)
                {
                    //Do not show if not visible
                    return;
                }

                var textBox = GetTextBoxInFocus();
                var contextMenu = (ContextMenu)sender;
                if (textBox == null)
                {
                    throw new Exception("Failed to locate which textbox is focused.");
                }

                if (textBox.Tag == null || textBox.Tag is SortedList<int, KeyPressInfo> || textBox.Tag is List<DCSBIOSInput>)
                {
                    return;
                }
                var keyPressLength = (KeyPressLength)textBox.Tag;

                foreach (MenuItem item in contextMenu.Items)
                {
                    item.IsChecked = false;
                }

                foreach (MenuItem item in contextMenu.Items)
                {
                    /*if (item.Name == "contextMenuItemZero" && keyPressLength == KeyPressLength.Zero)
                    {
                        item.IsChecked = true;
                    }*/
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
                Common.ShowErrorMessageBox(2061, ex);
            }
        }

        private void ClearAll(bool clearAlsoProfile)
        {
            foreach (var textBox in Common.FindVisualChildren<TextBox>(this))
            {
                textBox.Text = "";
                textBox.Tag = null;
            }
            if (clearAlsoProfile)
            {
                _radioPanelPZ69.ClearSettings();
            }
        }

        private void SetContextMenuClickHandlers()
        {
            foreach (var textBox in Common.FindVisualChildren<TextBox>(this))
            {
                if (textBox != TextBoxLogPZ69 && !textBox.Name.EndsWith("Numbers"))
                {
                    var contectMenu = (ContextMenu)Resources["TextBoxContextMenuPZ69"];
                    textBox.ContextMenu = contectMenu;
                    textBox.ContextMenuOpening += TextBoxContextMenuOpening;
                }
            }
        }

        private void TextBoxContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            try
            {
                //Timing values
                //Edit sequence
                //Edit DCS-BIOC Control
                var textBox = GetTextBoxInFocus();

                if (textBox == null)
                {
                    throw new Exception("Failed to locate which textbox is focused.");
                }
                var contextMenu = textBox.ContextMenu;

                // 1) If textbox.tag is List<DCSBIOSInput>, show Edit DCS-BIOS Control
                // 2) If textbox.tag is keyvaluepair, show Edit sequence
                // 3) If textbox.tag is null & text is empty && module!=NONE, show Edit sequence & DCS-BIOS Control

                // 4) If textbox has text and tag is not keyvaluepair/DCSBIOSInput, show press times
                // 5) If textbox is not empty, no tag show key press times
                // 6) If textbox is not empty, key press tag show key press times

                //1
                if (textBox.Tag != null && textBox.Tag is List<DCSBIOSInput>)
                {
                    // 1) If textbox.tag is List<DCSBIOSInput>, show Edit DCS-BIOS Control    
                    foreach (MenuItem item in contextMenu.Items)
                    {
                        if (!item.Name.Contains("EditDCSBIOS"))
                        {
                            item.Visibility = Visibility.Collapsed;
                        }
                        else
                        {
                            item.Visibility = Visibility.Visible;
                        }
                    }
                }
                else if (textBox.Tag != null && textBox.Tag is SortedList<int, KeyPressInfo>)
                {
                    // 2) If textbox.tag is keyvaluepair, show Edit sequence
                    foreach (MenuItem item in contextMenu.Items)
                    {
                        if (!item.Name.Contains("EditSequence"))
                        {
                            item.Visibility = Visibility.Collapsed;
                        }
                        else
                        {
                            item.Visibility = Visibility.Visible;
                        }
                    }
                }
                else if (textBox.Tag == null && string.IsNullOrWhiteSpace(textBox.Text))
                {
                    // 3) If textbox.tag is null & text is empty, show Edit sequence & DCS-BIOS Control
                    foreach (MenuItem item in contextMenu.Items)
                    {
                        if (item.Name.Contains("EditSequence"))
                        {
                            item.Visibility = Visibility.Visible;
                        }
                        else if (!_radioPanelPZ69.KeyboardEmulationOnly && item.Name.Contains("EditDCSBIOS"))
                        {
                            item.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            item.Visibility = Visibility.Collapsed;
                        }
                    }
                }
                else if (!string.IsNullOrWhiteSpace(textBox.Text) && (textBox.Tag == null || (!(textBox.Tag is List<DCSBIOSInput>) && !(textBox.Tag is SortedList<int, KeyPressInfo>))))
                {
                    // 4) If textbox has text and tag is not keyvaluepair/List<DCSBIOSInput>, show press times
                    foreach (MenuItem item in contextMenu.Items)
                    {
                        if (item.Name.Contains("EditSequence") || item.Name.Contains("EditDCSBIOS"))
                        {
                            item.Visibility = Visibility.Collapsed;
                        }
                        else
                        {
                            item.Visibility = Visibility.Visible;
                        }
                    }
                }
                else if (!string.IsNullOrWhiteSpace(textBox.Text) && (textBox.Tag == null))
                {
                    // 5) If textbox is not empty, no tag show key press times
                    foreach (MenuItem item in contextMenu.Items)
                    {
                        if (item.Name.Contains("EditDCSBIOS") || item.Name.Contains("EditSequence"))
                        {
                            item.Visibility = Visibility.Collapsed;
                        }
                        else
                        {
                            item.Visibility = Visibility.Visible;
                        }
                    }
                }

                // 6) If textbox is not empty, key press tag show key press times
                if ((string.IsNullOrEmpty(textBox.Text) && textBox.Tag != null) && textBox.Tag is KeyPressInfo)
                {
                    foreach (MenuItem item in contextMenu.Items)
                    {
                        if (item.Name.Contains("EditDCSBIOS") || item.Name.Contains("EditSequence"))
                        {
                            item.Visibility = Visibility.Collapsed;
                        }
                        else
                        {
                            item.Visibility = Visibility.Visible;
                        }
                    }
                }
                /*else
                {
                    foreach (MenuItem item in contextMenu.Items)
                    {
                        if (!item.Name.Contains("Sequence"))
                        {
                            item.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            item.Visibility = Visibility.Collapsed;
                        }
                    }
                }*/

            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(2081, ex);
            }
        }

        private TextBox GetTextBoxInFocus()
        {
            foreach (var textBox in Common.FindVisualChildren<TextBox>(this))
            {
                if (textBox != TextBoxLogPZ69 && textBox.IsFocused && textBox.Background == Brushes.Yellow)
                {
                    return textBox;
                }
            }
            return null;
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
                /*if(contextMenuItem.Name == "contextMenuItemZero")
                {
                    textBox.Tag = KeyPressLength.Zero;
                }*/
                /*if (contextMenuItem.Name == "contextMenuItemIndefinite")
                {
                    textBox.Tag = KeyPressLength.Indefinite;
                }*/
                if (contextMenuItem.Name == "contextMenuItemFiftyMilliSec")
                {
                    textBox.Tag = KeyPressLength.FiftyMilliSec;
                }
                else if (contextMenuItem.Name == "contextMenuItemHalfSecond")
                {
                    textBox.Tag = KeyPressLength.HalfSecond;
                }
                else if (contextMenuItem.Name == "contextMenuItemSecond")
                {
                    textBox.Tag = KeyPressLength.Second;
                }
                else if (contextMenuItem.Name == "contextMenuItemSecondAndHalf")
                {
                    textBox.Tag = KeyPressLength.SecondAndHalf;
                }
                else if (contextMenuItem.Name == "contextMenuItemTwoSeconds")
                {
                    textBox.Tag = KeyPressLength.TwoSeconds;
                }
                else if (contextMenuItem.Name == "contextMenuItemThreeSeconds")
                {
                    textBox.Tag = KeyPressLength.ThreeSeconds;
                }
                else if (contextMenuItem.Name == "contextMenuItemFourSeconds")
                {
                    textBox.Tag = KeyPressLength.FourSeconds;
                }
                else if (contextMenuItem.Name == "contextMenuItemFiveSecs")
                {
                    textBox.Tag = KeyPressLength.FiveSecs;
                }
                else if (contextMenuItem.Name == "contextMenuItemTenSecs")
                {
                    textBox.Tag = KeyPressLength.TenSecs;
                }
                else if (contextMenuItem.Name == "contextMenuItemFifteenSecs")
                {
                    textBox.Tag = KeyPressLength.FifteenSecs;
                }
                else if (contextMenuItem.Name == "contextMenuItemTwentySecs")
                {
                    textBox.Tag = KeyPressLength.TwentySecs;
                }
                else if (contextMenuItem.Name == "contextMenuItemThirtySecs")
                {
                    textBox.Tag = KeyPressLength.ThirtySecs;
                }
                else if (contextMenuItem.Name == "contextMenuItemFortySecs")
                {
                    textBox.Tag = KeyPressLength.FortySecs;
                }
                else if (contextMenuItem.Name == "contextMenuItemSixtySecs")
                {
                    textBox.Tag = KeyPressLength.SixtySecs;
                }

                UpdateKeyBindingProfileSimpleKeyStrokes(textBox);
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(2082, ex);
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
                    if (textBox.Tag != null && textBox.Tag is List<DCSBIOSInput>)
                    {
                        if (MessageBox.Show("Do you want to delete the DCS-BIOS configuration?", "Delete DCS-BIOS configuration?", MessageBoxButton.OKCancel, MessageBoxImage.Question) != MessageBoxResult.OK)
                        {
                            return;
                        }
                        textBox.ToolTip = null;
                        textBox.Text = "";
                        _radioPanelPZ69.ClearAllBindings(GetPZ69Key(textBox));
                        textBox.Tag = null;
                    }
                    else if (textBox.Tag != null && textBox.Tag is SortedList<int, KeyPressInfo>)
                    {
                        //Check if this textbox contains sequence information. If so then prompt the user for deletion
                        if (MessageBox.Show("Do you want to delete the key sequence?", "Delete key sequence?", MessageBoxButton.OKCancel, MessageBoxImage.Question) != MessageBoxResult.OK)
                        {
                            return;
                        }
                        textBox.Tag = null;
                        textBox.ToolTip = null;
                        textBox.Text = "";
                        UpdateKeyBindingProfileSimpleKeyStrokes(textBox);
                    }
                    else
                    {
                        textBox.Tag = null;
                        textBox.ToolTip = null;
                        textBox.Text = "";
                        UpdateKeyBindingProfileSimpleKeyStrokes(textBox);
                    }
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(3001, ex);
            }
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
                Common.ShowErrorMessageBox(3003, ex);
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
                Common.ShowErrorMessageBox(3004, ex);
            }
        }

        private void TextBoxLostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                ((TextBox)sender).Background = Brushes.White;
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(3005, ex);
            }
        }


        private void TextBoxPreviewKeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                var textBox = ((TextBox)sender);

                //Check if this textbox contains sequence or DCS-BIOS information. If so then exit
                if (textBox.Tag != null && (textBox.Tag is SortedList<int, KeyPressInfo> || textBox.Tag is List<DCSBIOSInput>))
                {
                    return;
                }
                var hashSetOfKeysPressed = new HashSet<string>();

                if (textBox.Tag == null)
                {
                    textBox.Tag = KeyPressLength.FiftyMilliSec;
                }

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
                UpdateKeyBindingProfileSimpleKeyStrokes(textBox);
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(3006, ex);
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
                var textBox = ((TextBox)sender);

                if (textBox.Text.Contains("."))
                {
                    textBox.MaxLength = 6;
                }
                else
                {
                    textBox.MaxLength = 5;
                }
                if (!_allowedKeys.Contains(e.Key))
                {
                    //Only figures and persion allowed
                    e.Handled = true;
                    return;
                }
                if (textBox.Text.Contains(".") && e.Key == Key.OemPeriod)
                {
                    e.Handled = true;
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(3006, ex);
            }
        }

        private void TextBoxTextChangedNumbers(object sender, TextChangedEventArgs e)
        {
            try
            {
                //MAKE SURE THE TAG IS SET BEFORE SETTING TEXT! OTHERWISE THIS DOESN'T FIRE
                var textBox = (TextBox)sender;
                if (textBox.Tag is SortedList<int, KeyPressInfo>)
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
                Common.ShowErrorMessageBox(3007, ex);
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
                Common.ShowErrorMessageBox(31007, ex);
            }
        }

        private void TextBoxMouseDoubleClickNumbers(object sender, MouseButtonEventArgs e)
        {
            try
            {
                var textBox = (TextBox)sender;
                var radioPanelKnob = RadioPanelPZ69KnobsEmulator.UpperCOM1;
                var radioDisplay = RadioPanelPZ69Display.UpperActive;

                if (e.ChangedButton == MouseButton.Left)
                {
                    if (textBox.Name.Contains("Upper"))
                    {
                        if (textBox.Name.Contains("Com1"))
                        {
                            radioPanelKnob = RadioPanelPZ69KnobsEmulator.UpperCOM1;
                        }
                        if (textBox.Name.Contains("Com2"))
                        {
                            radioPanelKnob = RadioPanelPZ69KnobsEmulator.UpperCOM2;
                        }
                        if (textBox.Name.Contains("Nav1"))
                        {
                            radioPanelKnob = RadioPanelPZ69KnobsEmulator.UpperNAV1;
                        }
                        if (textBox.Name.Contains("Nav2"))
                        {
                            radioPanelKnob = RadioPanelPZ69KnobsEmulator.UpperNAV2;
                        }
                        if (textBox.Name.Contains("ADF"))
                        {
                            radioPanelKnob = RadioPanelPZ69KnobsEmulator.UpperADF;
                        }
                        if (textBox.Name.Contains("DME"))
                        {
                            radioPanelKnob = RadioPanelPZ69KnobsEmulator.UpperDME;
                        }
                        if (textBox.Name.Contains("XPDR"))
                        {
                            radioPanelKnob = RadioPanelPZ69KnobsEmulator.UpperXPDR;
                        }

                        if (textBox.Name.Contains("Active"))
                        {
                            radioDisplay = RadioPanelPZ69Display.UpperActive;
                        }

                        if (textBox.Name.Contains("Standby"))
                        {
                            radioDisplay = RadioPanelPZ69Display.UpperStandby;
                        }
                    }
                    if (textBox.Name.Contains("Lower"))
                    {
                        if (textBox.Name.Contains("Com1"))
                        {
                            radioPanelKnob = RadioPanelPZ69KnobsEmulator.LowerCOM1;
                        }
                        if (textBox.Name.Contains("Com2"))
                        {
                            radioPanelKnob = RadioPanelPZ69KnobsEmulator.LowerCOM2;
                        }
                        if (textBox.Name.Contains("Nav1"))
                        {
                            radioPanelKnob = RadioPanelPZ69KnobsEmulator.LowerNAV1;
                        }
                        if (textBox.Name.Contains("Nav2"))
                        {
                            radioPanelKnob = RadioPanelPZ69KnobsEmulator.LowerNAV2;
                        }
                        if (textBox.Name.Contains("ADF"))
                        {
                            radioPanelKnob = RadioPanelPZ69KnobsEmulator.LowerADF;
                        }
                        if (textBox.Name.Contains("DME"))
                        {
                            radioPanelKnob = RadioPanelPZ69KnobsEmulator.LowerDME;
                        }
                        if (textBox.Name.Contains("XPDR"))
                        {
                            radioPanelKnob = RadioPanelPZ69KnobsEmulator.LowerXPDR;
                        }

                        if (textBox.Name.Contains("Active"))
                        {
                            radioDisplay = RadioPanelPZ69Display.LowerActive;
                        }

                        if (textBox.Name.Contains("Standby"))
                        {
                            radioDisplay = RadioPanelPZ69Display.LowerStandby;
                        }
                    }

                    _radioPanelPZ69.AddOrUpdateDisplayValue(radioPanelKnob, "-1", radioDisplay);
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(3001, ex);
            }
        }


        private void TextBoxShortcutKeyDownNumbers(object sender, KeyEventArgs e)
        {
            try
            {
                return;
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(3008, ex);
            }
        }
        /* ------------------------------------------------------------------------------------------------------------------------------------------------------------
         * ------------------------------------------------------------------------------------------------------------------------------------------------------------
         * ------------------------------------------------------------------------------------------------------------------------------------------------------------
         * ------------------------------------------------------------------------------------------------------------------------------------------------------------
         * ------------------------------------------------------------------------------------------------------------------------------------------------------------
         * ------------------------------------------------------------------------------------------------------------------------------------------------------------
         */

        private void TextBoxTextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                //MAKE SURE THE TAG IS SET BEFORE SETTING TEXT! OTHERWISE THIS DOESN'T FIRE
                var textBox = (TextBox)sender;
                if (textBox.Tag is SortedList<int, KeyPressInfo>)
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
                Common.ShowErrorMessageBox(3007, ex);
            }
        }


        private void TextBoxShortcutKeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                var textBox = ((TextBox)sender);
                //Check if this textbox contains sequence or DCS-BIOS information. If so then exit
                if (textBox.Tag != null && (textBox.Tag is SortedList<int, KeyPressInfo> || textBox.Tag is List<DCSBIOSInput>))
                {
                    return;
                }
                if (textBox.Tag == null)
                {
                    textBox.Tag = KeyPressLength.FiftyMilliSec;
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
                UpdateKeyBindingProfileSequencedKeyStrokesPZ69(textBox);
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(3008, ex);
            }
        }


        private void NotifySwitchChanges(HashSet<object> switches)
        {
            try
            {
                //Set focus to this so that virtual keypresses won't affect settings
                Dispatcher.BeginInvoke((Action)(() => TextBoxLogPZ69.Focus()));
                foreach (var radioPanelKey in switches)
                {
                    var key = (RadioPanelPZ69KnobEmulator)radioPanelKey;

                    if (_radioPanelPZ69.ForwardKeyPresses)
                    {
                        if (!string.IsNullOrEmpty(_radioPanelPZ69.GetKeyPressForLoggingPurposes(key)))
                        {
                            Dispatcher.BeginInvoke(
                                (Action)
                                (() =>
                                 TextBoxLogPZ69.Text =
                                 TextBoxLogPZ69.Text.Insert(0, _radioPanelPZ69.GetKeyPressForLoggingPurposes(key) + "\n")));
                        }
                    }
                    else
                    {
                        Dispatcher.BeginInvoke(
                            (Action)
                            (() =>
                             TextBoxLogPZ69.Text =
                             TextBoxLogPZ69.Text = TextBoxLogPZ69.Text.Insert(0, "No action taken, virtual key press disabled.\n")));
                    }
                }
                SetGraphicsState(switches);
            }
            catch (Exception ex)
            {
                Dispatcher.BeginInvoke(
                    (Action)
                    (() =>
                     TextBoxLogPZ69.Text = TextBoxLogPZ69.Text.Insert(0, "0x16" + ex.Message + ".\n")));
                Common.ShowErrorMessageBox(3009, ex);
            }
        }

        private void ShowGraphicConfiguration()
        {
            try
            {
                foreach (var displayValue in _radioPanelPZ69.DisplayValueHashSet)
                {
                    if (displayValue.RadioPanelPZ69Knob == RadioPanelPZ69KnobsEmulator.UpperCOM1)
                    {
                        if (displayValue.RadioPanelDisplay == RadioPanelPZ69Display.UpperActive)
                        {
                            TextBoxUpperCom1ActiveNumbers.Text = displayValue.Value.ToString(Common.GetPZ69FullDisplayNumberFormat());
                        }
                        if (displayValue.RadioPanelDisplay == RadioPanelPZ69Display.UpperStandby)
                        {
                            TextBoxUpperCom1StandbyNumbers.Text = displayValue.Value.ToString(Common.GetPZ69FullDisplayNumberFormat());
                        }
                    }
                    else if (displayValue.RadioPanelPZ69Knob == RadioPanelPZ69KnobsEmulator.UpperCOM2)
                    {
                        if (displayValue.RadioPanelDisplay == RadioPanelPZ69Display.UpperActive)
                        {
                            TextBoxUpperCom2ActiveNumbers.Text = displayValue.Value.ToString(Common.GetPZ69FullDisplayNumberFormat());
                        }
                        if (displayValue.RadioPanelDisplay == RadioPanelPZ69Display.UpperStandby)
                        {
                            TextBoxUpperCom2StandbyNumbers.Text = displayValue.Value.ToString(Common.GetPZ69FullDisplayNumberFormat());
                        }
                    }
                    else if (displayValue.RadioPanelPZ69Knob == RadioPanelPZ69KnobsEmulator.UpperNAV1)
                    {
                        if (displayValue.RadioPanelDisplay == RadioPanelPZ69Display.UpperActive)
                        {
                            TextBoxUpperNav1ActiveNumbers.Text = displayValue.Value.ToString(Common.GetPZ69FullDisplayNumberFormat());
                        }
                        if (displayValue.RadioPanelDisplay == RadioPanelPZ69Display.UpperStandby)
                        {
                            TextBoxUpperNav1StandbyNumbers.Text = displayValue.Value.ToString(Common.GetPZ69FullDisplayNumberFormat());
                        }
                    }
                    else if (displayValue.RadioPanelPZ69Knob == RadioPanelPZ69KnobsEmulator.UpperNAV2)
                    {
                        if (displayValue.RadioPanelDisplay == RadioPanelPZ69Display.UpperActive)
                        {
                            TextBoxUpperNav2ActiveNumbers.Text = displayValue.Value.ToString(Common.GetPZ69FullDisplayNumberFormat());
                        }
                        if (displayValue.RadioPanelDisplay == RadioPanelPZ69Display.UpperStandby)
                        {
                            TextBoxUpperNav2StandbyNumbers.Text = displayValue.Value.ToString(Common.GetPZ69FullDisplayNumberFormat());
                        }
                    }
                    else if (displayValue.RadioPanelPZ69Knob == RadioPanelPZ69KnobsEmulator.UpperADF)
                    {
                        if (displayValue.RadioPanelDisplay == RadioPanelPZ69Display.UpperActive)
                        {
                            TextBoxUpperADFActiveNumbers.Text = displayValue.Value.ToString(Common.GetPZ69FullDisplayNumberFormat());
                        }
                        if (displayValue.RadioPanelDisplay == RadioPanelPZ69Display.UpperStandby)
                        {
                            TextBoxUpperADFStandbyNumbers.Text = displayValue.Value.ToString(Common.GetPZ69FullDisplayNumberFormat());
                        }
                    }
                    else if (displayValue.RadioPanelPZ69Knob == RadioPanelPZ69KnobsEmulator.UpperDME)
                    {
                        if (displayValue.RadioPanelDisplay == RadioPanelPZ69Display.UpperActive)
                        {
                            TextBoxUpperDMEActiveNumbers.Text = displayValue.Value.ToString(Common.GetPZ69FullDisplayNumberFormat());
                        }
                        if (displayValue.RadioPanelDisplay == RadioPanelPZ69Display.UpperStandby)
                        {
                            TextBoxUpperDMEStandbyNumbers.Text = displayValue.Value.ToString(Common.GetPZ69FullDisplayNumberFormat());
                        }
                    }
                    else if (displayValue.RadioPanelPZ69Knob == RadioPanelPZ69KnobsEmulator.UpperXPDR)
                    {
                        if (displayValue.RadioPanelDisplay == RadioPanelPZ69Display.UpperActive)
                        {
                            TextBoxUpperXPDRActiveNumbers.Text = displayValue.Value.ToString(Common.GetPZ69FullDisplayNumberFormat());
                        }
                        if (displayValue.RadioPanelDisplay == RadioPanelPZ69Display.UpperStandby)
                        {
                            TextBoxUpperXPDRStandbyNumbers.Text = displayValue.Value.ToString(Common.GetPZ69FullDisplayNumberFormat());
                        }
                    }
                    else if (displayValue.RadioPanelPZ69Knob == RadioPanelPZ69KnobsEmulator.LowerCOM1)
                    {
                        if (displayValue.RadioPanelDisplay == RadioPanelPZ69Display.LowerActive)
                        {
                            TextBoxLowerCom1ActiveNumbers.Text = displayValue.Value.ToString(Common.GetPZ69FullDisplayNumberFormat());
                        }
                        if (displayValue.RadioPanelDisplay == RadioPanelPZ69Display.LowerStandby)
                        {
                            TextBoxLowerCom1StandbyNumbers.Text = displayValue.Value.ToString(Common.GetPZ69FullDisplayNumberFormat());
                        }
                    }
                    else if (displayValue.RadioPanelPZ69Knob == RadioPanelPZ69KnobsEmulator.LowerCOM2)
                    {
                        if (displayValue.RadioPanelDisplay == RadioPanelPZ69Display.LowerActive)
                        {
                            TextBoxLowerCom2ActiveNumbers.Text = displayValue.Value.ToString(Common.GetPZ69FullDisplayNumberFormat());
                        }
                        if (displayValue.RadioPanelDisplay == RadioPanelPZ69Display.LowerStandby)
                        {
                            TextBoxLowerCom2StandbyNumbers.Text = displayValue.Value.ToString(Common.GetPZ69FullDisplayNumberFormat());
                        }
                    }
                    else if (displayValue.RadioPanelPZ69Knob == RadioPanelPZ69KnobsEmulator.LowerNAV1)
                    {
                        if (displayValue.RadioPanelDisplay == RadioPanelPZ69Display.LowerActive)
                        {
                            TextBoxLowerNav1ActiveNumbers.Text = displayValue.Value.ToString(Common.GetPZ69FullDisplayNumberFormat());
                        }
                        if (displayValue.RadioPanelDisplay == RadioPanelPZ69Display.LowerStandby)
                        {
                            TextBoxLowerNav1StandbyNumbers.Text = displayValue.Value.ToString(Common.GetPZ69FullDisplayNumberFormat());
                        }
                    }
                    else if (displayValue.RadioPanelPZ69Knob == RadioPanelPZ69KnobsEmulator.LowerNAV2)
                    {
                        if (displayValue.RadioPanelDisplay == RadioPanelPZ69Display.LowerActive)
                        {
                            TextBoxLowerNav2ActiveNumbers.Text = displayValue.Value.ToString(Common.GetPZ69FullDisplayNumberFormat());
                        }
                        if (displayValue.RadioPanelDisplay == RadioPanelPZ69Display.LowerStandby)
                        {
                            TextBoxLowerNav2StandbyNumbers.Text = displayValue.Value.ToString(Common.GetPZ69FullDisplayNumberFormat());
                        }
                    }
                    else if (displayValue.RadioPanelPZ69Knob == RadioPanelPZ69KnobsEmulator.LowerADF)
                    {
                        if (displayValue.RadioPanelDisplay == RadioPanelPZ69Display.LowerActive)
                        {
                            TextBoxLowerADFActiveNumbers.Text = displayValue.Value.ToString(Common.GetPZ69FullDisplayNumberFormat());
                        }
                        if (displayValue.RadioPanelDisplay == RadioPanelPZ69Display.LowerStandby)
                        {
                            TextBoxLowerADFStandbyNumbers.Text = displayValue.Value.ToString(Common.GetPZ69FullDisplayNumberFormat());
                        }
                    }
                    else if (displayValue.RadioPanelPZ69Knob == RadioPanelPZ69KnobsEmulator.LowerDME)
                    {
                        if (displayValue.RadioPanelDisplay == RadioPanelPZ69Display.LowerActive)
                        {
                            TextBoxLowerDMEActiveNumbers.Text = displayValue.Value.ToString(Common.GetPZ69FullDisplayNumberFormat());
                        }
                        if (displayValue.RadioPanelDisplay == RadioPanelPZ69Display.LowerStandby)
                        {
                            TextBoxLowerDMEStandbyNumbers.Text = displayValue.Value.ToString(Common.GetPZ69FullDisplayNumberFormat());
                        }
                    }
                    else if (displayValue.RadioPanelPZ69Knob == RadioPanelPZ69KnobsEmulator.LowerXPDR)
                    {
                        if (displayValue.RadioPanelDisplay == RadioPanelPZ69Display.LowerActive)
                        {
                            TextBoxLowerXPDRActiveNumbers.Text = displayValue.Value.ToString(Common.GetPZ69FullDisplayNumberFormat());
                        }
                        if (displayValue.RadioPanelDisplay == RadioPanelPZ69Display.LowerStandby)
                        {
                            TextBoxLowerXPDRStandbyNumbers.Text = displayValue.Value.ToString(Common.GetPZ69FullDisplayNumberFormat());
                        }
                    }
                }

                foreach (var keyBinding in _radioPanelPZ69.KeyBindingsHashSet)
                {
                    if (keyBinding.RadioPanelPZ69Key == RadioPanelPZ69KnobsEmulator.UpperCOM1)
                    {
                        if (keyBinding.OSKeyPress != null && !keyBinding.OSKeyPress.IsMultiSequenced())
                        {
                            TextBoxUpperCom1.Tag = keyBinding.OSKeyPress.LengthOfKeyPress();
                            TextBoxUpperCom1.Text = keyBinding.OSKeyPress.GetSimpleVirtualKeyCodesAsString();
                        }
                        else if (keyBinding.OSKeyPress != null && keyBinding.OSKeyPress.IsMultiSequenced())
                        {
                            TextBoxUpperCom1.Tag = keyBinding.OSKeyPress.GetSequence;
                            TextBoxUpperCom1.Text = keyBinding.OSKeyPress.Information;
                        }
                    }
                    if (keyBinding.RadioPanelPZ69Key == RadioPanelPZ69KnobsEmulator.UpperCOM2)
                    {
                        if (keyBinding.OSKeyPress != null && !keyBinding.OSKeyPress.IsMultiSequenced())
                        {
                            TextBoxUpperCom2.Tag = keyBinding.OSKeyPress.LengthOfKeyPress();
                            TextBoxUpperCom2.Text = keyBinding.OSKeyPress.GetSimpleVirtualKeyCodesAsString();
                        }
                        else if (keyBinding.OSKeyPress != null && keyBinding.OSKeyPress.IsMultiSequenced())
                        {
                            TextBoxUpperCom2.Tag = keyBinding.OSKeyPress.GetSequence;
                            TextBoxUpperCom2.Text = keyBinding.OSKeyPress.Information;
                        }
                    }
                    if (keyBinding.RadioPanelPZ69Key == RadioPanelPZ69KnobsEmulator.UpperNAV1)
                    {
                        if (keyBinding.OSKeyPress != null && !keyBinding.OSKeyPress.IsMultiSequenced())
                        {
                            TextBoxUpperNav1.Tag = keyBinding.OSKeyPress.LengthOfKeyPress();
                            TextBoxUpperNav1.Text = keyBinding.OSKeyPress.GetSimpleVirtualKeyCodesAsString();
                        }
                        else if (keyBinding.OSKeyPress != null && keyBinding.OSKeyPress.IsMultiSequenced())
                        {
                            TextBoxUpperNav1.Tag = keyBinding.OSKeyPress.GetSequence;
                            TextBoxUpperNav1.Text = keyBinding.OSKeyPress.Information;
                        }
                    }
                    if (keyBinding.RadioPanelPZ69Key == RadioPanelPZ69KnobsEmulator.UpperNAV2)
                    {
                        if (keyBinding.OSKeyPress != null && !keyBinding.OSKeyPress.IsMultiSequenced())
                        {
                            TextBoxUpperNav2.Tag = keyBinding.OSKeyPress.LengthOfKeyPress();
                            TextBoxUpperNav2.Text = keyBinding.OSKeyPress.GetSimpleVirtualKeyCodesAsString();
                        }
                        else if (keyBinding.OSKeyPress != null && keyBinding.OSKeyPress.IsMultiSequenced())
                        {
                            TextBoxUpperNav2.Text = keyBinding.OSKeyPress.Information;
                            TextBoxUpperNav2.Tag = keyBinding.OSKeyPress.GetSequence;
                        }
                    }
                    if (keyBinding.RadioPanelPZ69Key == RadioPanelPZ69KnobsEmulator.UpperADF)
                    {
                        if (keyBinding.OSKeyPress != null && !keyBinding.OSKeyPress.IsMultiSequenced())
                        {
                            TextBoxUpperADF.Tag = keyBinding.OSKeyPress.LengthOfKeyPress();
                            TextBoxUpperADF.Text = keyBinding.OSKeyPress.GetSimpleVirtualKeyCodesAsString();
                        }
                        else if (keyBinding.OSKeyPress != null && keyBinding.OSKeyPress.IsMultiSequenced())
                        {
                            TextBoxUpperADF.Tag = keyBinding.OSKeyPress.GetSequence;
                            TextBoxUpperADF.Text = keyBinding.OSKeyPress.Information;
                        }
                    }

                    if (keyBinding.RadioPanelPZ69Key == RadioPanelPZ69KnobsEmulator.UpperDME)
                    {
                        if (keyBinding.OSKeyPress != null && !keyBinding.OSKeyPress.IsMultiSequenced())
                        {
                            TextBoxUpperDME.Tag = keyBinding.OSKeyPress.LengthOfKeyPress();
                            TextBoxUpperDME.Text = keyBinding.OSKeyPress.GetSimpleVirtualKeyCodesAsString();
                        }
                        else if (keyBinding.OSKeyPress != null && keyBinding.OSKeyPress.IsMultiSequenced())
                        {
                            TextBoxUpperDME.Tag = keyBinding.OSKeyPress.GetSequence;
                            TextBoxUpperDME.Text = keyBinding.OSKeyPress.Information;
                        }
                    }
                    if (keyBinding.RadioPanelPZ69Key == RadioPanelPZ69KnobsEmulator.UpperXPDR)
                    {
                        if (keyBinding.OSKeyPress != null && !keyBinding.OSKeyPress.IsMultiSequenced())
                        {
                            TextBoxUpperXPDR.Tag = keyBinding.OSKeyPress.LengthOfKeyPress();
                            TextBoxUpperXPDR.Text = keyBinding.OSKeyPress.GetSimpleVirtualKeyCodesAsString();
                        }
                        else if (keyBinding.OSKeyPress != null && keyBinding.OSKeyPress.IsMultiSequenced())
                        {
                            TextBoxUpperXPDR.Tag = keyBinding.OSKeyPress.GetSequence;
                            TextBoxUpperXPDR.Text = keyBinding.OSKeyPress.Information;
                        }

                    }
                    if (keyBinding.RadioPanelPZ69Key == RadioPanelPZ69KnobsEmulator.UpperLargeFreqWheelInc)
                    {
                        if (keyBinding.OSKeyPress != null && !keyBinding.OSKeyPress.IsMultiSequenced())
                        {
                            TextBoxUpperLargePlus.Tag = keyBinding.OSKeyPress.LengthOfKeyPress();
                            TextBoxUpperLargePlus.Text = keyBinding.OSKeyPress.GetSimpleVirtualKeyCodesAsString();
                        }
                        else if (keyBinding.OSKeyPress != null && keyBinding.OSKeyPress.IsMultiSequenced())
                        {
                            TextBoxUpperLargePlus.Tag = keyBinding.OSKeyPress.GetSequence;
                            TextBoxUpperLargePlus.Text = keyBinding.OSKeyPress.Information;
                        }
                    }
                    if (keyBinding.RadioPanelPZ69Key == RadioPanelPZ69KnobsEmulator.UpperLargeFreqWheelDec)
                    {
                        if (keyBinding.OSKeyPress != null && !keyBinding.OSKeyPress.IsMultiSequenced())
                        {
                            TextBoxUpperLargeMinus.Tag = keyBinding.OSKeyPress.LengthOfKeyPress();
                            TextBoxUpperLargeMinus.Text = keyBinding.OSKeyPress.GetSimpleVirtualKeyCodesAsString();
                        }
                        else if (keyBinding.OSKeyPress != null && keyBinding.OSKeyPress.IsMultiSequenced())
                        {
                            TextBoxUpperLargeMinus.Tag = keyBinding.OSKeyPress.GetSequence;
                            TextBoxUpperLargeMinus.Text = keyBinding.OSKeyPress.Information;
                        }
                    }
                    if (keyBinding.RadioPanelPZ69Key == RadioPanelPZ69KnobsEmulator.UpperSmallFreqWheelInc)
                    {
                        if (keyBinding.OSKeyPress != null && !keyBinding.OSKeyPress.IsMultiSequenced())
                        {
                            TextBoxUpperSmallPlus.Tag = keyBinding.OSKeyPress.LengthOfKeyPress();
                            TextBoxUpperSmallPlus.Text = keyBinding.OSKeyPress.GetSimpleVirtualKeyCodesAsString();
                        }
                        else if (keyBinding.OSKeyPress != null && keyBinding.OSKeyPress.IsMultiSequenced())
                        {
                            TextBoxUpperSmallPlus.Tag = keyBinding.OSKeyPress.GetSequence;
                            TextBoxUpperSmallPlus.Text = keyBinding.OSKeyPress.Information;
                        }
                    }
                    if (keyBinding.RadioPanelPZ69Key == RadioPanelPZ69KnobsEmulator.UpperSmallFreqWheelDec)
                    {
                        if (keyBinding.OSKeyPress != null && !keyBinding.OSKeyPress.IsMultiSequenced())
                        {
                            TextBoxUpperSmallMinus.Tag = keyBinding.OSKeyPress.LengthOfKeyPress();
                            TextBoxUpperSmallMinus.Text = keyBinding.OSKeyPress.GetSimpleVirtualKeyCodesAsString();
                        }
                        else if (keyBinding.OSKeyPress != null && keyBinding.OSKeyPress.IsMultiSequenced())
                        {
                            TextBoxUpperSmallMinus.Tag = keyBinding.OSKeyPress.GetSequence;
                            TextBoxUpperSmallMinus.Text = keyBinding.OSKeyPress.Information;
                        }
                    }
                    if (keyBinding.RadioPanelPZ69Key == RadioPanelPZ69KnobsEmulator.UpperFreqSwitch)
                    {
                        if (keyBinding.WhenTurnedOn)
                        {
                            if (keyBinding.OSKeyPress != null && !keyBinding.OSKeyPress.IsMultiSequenced())
                            {
                                TextBoxUpperActStbyOn.Tag = keyBinding.OSKeyPress.LengthOfKeyPress();
                                TextBoxUpperActStbyOn.Text = keyBinding.OSKeyPress.GetSimpleVirtualKeyCodesAsString();
                            }
                            else if (keyBinding.OSKeyPress != null && keyBinding.OSKeyPress.IsMultiSequenced())
                            {
                                TextBoxUpperActStbyOn.Tag = keyBinding.OSKeyPress.GetSequence;
                                TextBoxUpperActStbyOn.Text = keyBinding.OSKeyPress.Information;
                            }
                        }
                        else
                        {
                            if (keyBinding.OSKeyPress != null && !keyBinding.OSKeyPress.IsMultiSequenced())
                            {
                                TextBoxUpperActStbyOff.Tag = keyBinding.OSKeyPress.LengthOfKeyPress();
                                TextBoxUpperActStbyOff.Text = keyBinding.OSKeyPress.GetSimpleVirtualKeyCodesAsString();
                            }
                            else if (keyBinding.OSKeyPress != null && keyBinding.OSKeyPress.IsMultiSequenced())
                            {
                                TextBoxUpperActStbyOff.Tag = keyBinding.OSKeyPress.GetSequence;
                                TextBoxUpperActStbyOff.Text = keyBinding.OSKeyPress.Information;
                            }
                        }
                    }
                    if (keyBinding.RadioPanelPZ69Key == RadioPanelPZ69KnobsEmulator.LowerCOM1)
                    {
                        if (keyBinding.OSKeyPress != null && !keyBinding.OSKeyPress.IsMultiSequenced())
                        {
                            TextBoxLowerCom1.Tag = keyBinding.OSKeyPress.LengthOfKeyPress();
                            TextBoxLowerCom1.Text = keyBinding.OSKeyPress.GetSimpleVirtualKeyCodesAsString();
                        }
                        else if (keyBinding.OSKeyPress != null && keyBinding.OSKeyPress.IsMultiSequenced())
                        {
                            TextBoxLowerCom1.Tag = keyBinding.OSKeyPress.GetSequence;
                            TextBoxLowerCom1.Text = keyBinding.OSKeyPress.Information;
                        }
                    }
                    if (keyBinding.RadioPanelPZ69Key == RadioPanelPZ69KnobsEmulator.LowerCOM2)
                    {
                        if (keyBinding.OSKeyPress != null && !keyBinding.OSKeyPress.IsMultiSequenced())
                        {
                            TextBoxLowerCom2.Tag = keyBinding.OSKeyPress.LengthOfKeyPress();
                            TextBoxLowerCom2.Text = keyBinding.OSKeyPress.GetSimpleVirtualKeyCodesAsString();
                        }
                        else if (keyBinding.OSKeyPress != null && keyBinding.OSKeyPress.IsMultiSequenced())
                        {
                            TextBoxLowerCom2.Tag = keyBinding.OSKeyPress.GetSequence;
                            TextBoxLowerCom2.Text = keyBinding.OSKeyPress.Information;
                        }
                    }
                    if (keyBinding.RadioPanelPZ69Key == RadioPanelPZ69KnobsEmulator.LowerNAV1)
                    {
                        if (keyBinding.OSKeyPress != null && !keyBinding.OSKeyPress.IsMultiSequenced())
                        {
                            TextBoxLowerNav1.Tag = keyBinding.OSKeyPress.LengthOfKeyPress();
                            TextBoxLowerNav1.Text = keyBinding.OSKeyPress.GetSimpleVirtualKeyCodesAsString();
                        }
                        else if (keyBinding.OSKeyPress != null && keyBinding.OSKeyPress.IsMultiSequenced())
                        {
                            TextBoxLowerNav1.Tag = keyBinding.OSKeyPress.GetSequence;
                            TextBoxLowerNav1.Text = keyBinding.OSKeyPress.Information;
                        }
                    }
                    if (keyBinding.RadioPanelPZ69Key == RadioPanelPZ69KnobsEmulator.LowerNAV2)
                    {
                        if (keyBinding.OSKeyPress != null && !keyBinding.OSKeyPress.IsMultiSequenced())
                        {
                            TextBoxLowerNav2.Tag = keyBinding.OSKeyPress.LengthOfKeyPress();
                            TextBoxLowerNav2.Text = keyBinding.OSKeyPress.GetSimpleVirtualKeyCodesAsString();
                        }
                        else if (keyBinding.OSKeyPress != null && keyBinding.OSKeyPress.IsMultiSequenced())
                        {
                            TextBoxLowerNav2.Tag = keyBinding.OSKeyPress.GetSequence;
                            TextBoxLowerNav2.Text = keyBinding.OSKeyPress.Information;
                        }
                    }
                    if (keyBinding.RadioPanelPZ69Key == RadioPanelPZ69KnobsEmulator.LowerADF)
                    {
                        if (keyBinding.OSKeyPress != null && !keyBinding.OSKeyPress.IsMultiSequenced())
                        {
                            TextBoxLowerADF.Tag = keyBinding.OSKeyPress.LengthOfKeyPress();
                            TextBoxLowerADF.Text = keyBinding.OSKeyPress.GetSimpleVirtualKeyCodesAsString();
                        }
                        else if (keyBinding.OSKeyPress != null && keyBinding.OSKeyPress.IsMultiSequenced())
                        {
                            TextBoxLowerADF.Tag = keyBinding.OSKeyPress.GetSequence;
                            TextBoxLowerADF.Text = keyBinding.OSKeyPress.Information;
                        }
                    }
                    if (keyBinding.RadioPanelPZ69Key == RadioPanelPZ69KnobsEmulator.LowerDME)
                    {
                        if (keyBinding.OSKeyPress != null && !keyBinding.OSKeyPress.IsMultiSequenced())
                        {
                            TextBoxLowerDME.Tag = keyBinding.OSKeyPress.LengthOfKeyPress();
                            TextBoxLowerDME.Text = keyBinding.OSKeyPress.GetSimpleVirtualKeyCodesAsString();
                        }
                        else if (keyBinding.OSKeyPress != null && keyBinding.OSKeyPress.IsMultiSequenced())
                        {
                            TextBoxLowerDME.Tag = keyBinding.OSKeyPress.GetSequence;
                            TextBoxLowerDME.Text = keyBinding.OSKeyPress.Information;
                        }
                    }
                    if (keyBinding.RadioPanelPZ69Key == RadioPanelPZ69KnobsEmulator.LowerXPDR)
                    {
                        if (keyBinding.WhenTurnedOn)
                        {
                            if (keyBinding.OSKeyPress != null && !keyBinding.OSKeyPress.IsMultiSequenced())
                            {
                                TextBoxLowerXPDR.Tag = keyBinding.OSKeyPress.LengthOfKeyPress();
                                TextBoxLowerXPDR.Text = keyBinding.OSKeyPress.GetSimpleVirtualKeyCodesAsString();
                            }
                            else if (keyBinding.OSKeyPress != null && keyBinding.OSKeyPress.IsMultiSequenced())
                            {
                                TextBoxLowerXPDR.Tag = keyBinding.OSKeyPress.GetSequence;
                                TextBoxLowerXPDR.Text = keyBinding.OSKeyPress.Information;
                            }
                        }
                    }
                    if (keyBinding.RadioPanelPZ69Key == RadioPanelPZ69KnobsEmulator.LowerLargeFreqWheelInc)
                    {
                        if (keyBinding.OSKeyPress != null && !keyBinding.OSKeyPress.IsMultiSequenced())
                        {
                            TextBoxLowerLargePlus.Tag = keyBinding.OSKeyPress.LengthOfKeyPress();
                            TextBoxLowerLargePlus.Text = keyBinding.OSKeyPress.GetSimpleVirtualKeyCodesAsString();
                        }
                        else if (keyBinding.OSKeyPress != null && keyBinding.OSKeyPress.IsMultiSequenced())
                        {
                            TextBoxLowerLargePlus.Tag = keyBinding.OSKeyPress.GetSequence;
                            TextBoxLowerLargePlus.Text = keyBinding.OSKeyPress.Information;
                        }
                    }
                    if (keyBinding.RadioPanelPZ69Key == RadioPanelPZ69KnobsEmulator.LowerLargeFreqWheelDec)
                    {
                        if (keyBinding.OSKeyPress != null && !keyBinding.OSKeyPress.IsMultiSequenced())
                        {
                            TextBoxLowerLargeMinus.Tag = keyBinding.OSKeyPress.LengthOfKeyPress();
                            TextBoxLowerLargeMinus.Text = keyBinding.OSKeyPress.GetSimpleVirtualKeyCodesAsString();
                        }
                        else if (keyBinding.OSKeyPress != null && keyBinding.OSKeyPress.IsMultiSequenced())
                        {
                            TextBoxLowerLargeMinus.Tag = keyBinding.OSKeyPress.GetSequence;
                            TextBoxLowerLargeMinus.Text = keyBinding.OSKeyPress.Information;
                        }
                    }
                    if (keyBinding.RadioPanelPZ69Key == RadioPanelPZ69KnobsEmulator.LowerSmallFreqWheelInc)
                    {
                        if (keyBinding.OSKeyPress != null && !keyBinding.OSKeyPress.IsMultiSequenced())
                        {
                            TextBoxLowerSmallPlus.Tag = keyBinding.OSKeyPress.LengthOfKeyPress();
                            TextBoxLowerSmallPlus.Text = keyBinding.OSKeyPress.GetSimpleVirtualKeyCodesAsString();
                        }
                        else if (keyBinding.OSKeyPress != null && keyBinding.OSKeyPress.IsMultiSequenced())
                        {
                            TextBoxLowerSmallPlus.Tag = keyBinding.OSKeyPress.GetSequence;
                            TextBoxLowerSmallPlus.Text = keyBinding.OSKeyPress.Information;
                        }
                    }
                    if (keyBinding.RadioPanelPZ69Key == RadioPanelPZ69KnobsEmulator.LowerSmallFreqWheelDec)
                    {
                        if (keyBinding.OSKeyPress != null && !keyBinding.OSKeyPress.IsMultiSequenced())
                        {
                            TextBoxLowerSmallMinus.Tag = keyBinding.OSKeyPress.LengthOfKeyPress();
                            TextBoxLowerSmallMinus.Text = keyBinding.OSKeyPress.GetSimpleVirtualKeyCodesAsString();
                        }
                        else if (keyBinding.OSKeyPress != null && keyBinding.OSKeyPress.IsMultiSequenced())
                        {
                            TextBoxLowerSmallMinus.Tag = keyBinding.OSKeyPress.GetSequence;
                            TextBoxLowerSmallMinus.Text = keyBinding.OSKeyPress.Information;
                        }
                    }
                    if (keyBinding.RadioPanelPZ69Key == RadioPanelPZ69KnobsEmulator.LowerFreqSwitch)
                    {
                        if (keyBinding.WhenTurnedOn)
                        {
                            if (keyBinding.OSKeyPress != null && !keyBinding.OSKeyPress.IsMultiSequenced())
                            {
                                TextBoxLowerActStbyOn.Tag = keyBinding.OSKeyPress.LengthOfKeyPress();
                                TextBoxLowerActStbyOn.Text = keyBinding.OSKeyPress.GetSimpleVirtualKeyCodesAsString();
                            }
                            else if (keyBinding.OSKeyPress != null && keyBinding.OSKeyPress.IsMultiSequenced())
                            {
                                TextBoxLowerActStbyOn.Tag = keyBinding.OSKeyPress.GetSequence;
                                TextBoxLowerActStbyOn.Text = keyBinding.OSKeyPress.Information;
                            }
                        }
                        else
                        {
                            if (keyBinding.OSKeyPress != null && !keyBinding.OSKeyPress.IsMultiSequenced())
                            {
                                TextBoxLowerActStbyOff.Tag = keyBinding.OSKeyPress.LengthOfKeyPress();
                                TextBoxLowerActStbyOff.Text = keyBinding.OSKeyPress.GetSimpleVirtualKeyCodesAsString();
                            }
                            else if (keyBinding.OSKeyPress != null && keyBinding.OSKeyPress.IsMultiSequenced())
                            {
                                TextBoxLowerActStbyOff.Tag = keyBinding.OSKeyPress.GetSequence;
                                TextBoxLowerActStbyOff.Text = keyBinding.OSKeyPress.Information;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(3013, ex);
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
                                Dispatcher.BeginInvoke(
                                    (Action)delegate
                                    {
                                        TopLeftCom1.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case RadioPanelPZ69KnobsEmulator.UpperCOM2:
                            {
                                var key = radioKnob;
                                Dispatcher.BeginInvoke(
                                    (Action)delegate
                                    {
                                        TopLeftCom2.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case RadioPanelPZ69KnobsEmulator.UpperNAV1:
                            {
                                var key = radioKnob;
                                Dispatcher.BeginInvoke(
                                    (Action)delegate
                                    {
                                        TopLeftNav1.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case RadioPanelPZ69KnobsEmulator.UpperNAV2:
                            {
                                var key = radioKnob;
                                Dispatcher.BeginInvoke(
                                    (Action)delegate
                                    {
                                        TopLeftNav2.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case RadioPanelPZ69KnobsEmulator.UpperADF:
                            {
                                var key = radioKnob;
                                Dispatcher.BeginInvoke(
                                    (Action)delegate
                                    {
                                        TopLeftADF.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case RadioPanelPZ69KnobsEmulator.UpperDME:
                            {
                                var key = radioKnob;
                                Dispatcher.BeginInvoke(
                                    (Action)delegate
                                    {
                                        TopLeftDME.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case RadioPanelPZ69KnobsEmulator.UpperXPDR:
                            {
                                var key = radioKnob;
                                Dispatcher.BeginInvoke(
                                    (Action)delegate
                                    {
                                        TopLeftXPDR.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case RadioPanelPZ69KnobsEmulator.LowerCOM1:
                            {
                                var key = radioKnob;
                                Dispatcher.BeginInvoke(
                                    (Action)delegate
                                    {
                                        LowerLeftCom1.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case RadioPanelPZ69KnobsEmulator.LowerCOM2:
                            {
                                var key = radioKnob;
                                Dispatcher.BeginInvoke(
                                    (Action)delegate
                                    {
                                        LowerLeftCom2.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case RadioPanelPZ69KnobsEmulator.LowerNAV1:
                            {
                                var key = radioKnob;
                                Dispatcher.BeginInvoke(
                                    (Action)delegate
                                    {
                                        LowerLeftNav1.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case RadioPanelPZ69KnobsEmulator.LowerNAV2:
                            {
                                var key = radioKnob;
                                Dispatcher.BeginInvoke(
                                    (Action)delegate
                                    {
                                        LowerLeftNav2.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case RadioPanelPZ69KnobsEmulator.LowerADF:
                            {
                                var key = radioKnob;
                                Dispatcher.BeginInvoke(
                                    (Action)delegate
                                    {
                                        LowerLeftADF.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case RadioPanelPZ69KnobsEmulator.LowerDME:
                            {
                                var key = radioKnob;
                                Dispatcher.BeginInvoke(
                                    (Action)delegate
                                    {
                                        LowerLeftDME.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case RadioPanelPZ69KnobsEmulator.LowerXPDR:
                            {
                                var key = radioKnob;
                                Dispatcher.BeginInvoke(
                                    (Action)delegate
                                    {
                                        LowerLeftXPDR.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case RadioPanelPZ69KnobsEmulator.UpperSmallFreqWheelInc:
                            {
                                var key = radioKnob;
                                Dispatcher.BeginInvoke(
                                    (Action)delegate
                                    {
                                        UpperSmallerLCDKnobInc.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case RadioPanelPZ69KnobsEmulator.UpperSmallFreqWheelDec:
                            {
                                var key = radioKnob;
                                Dispatcher.BeginInvoke(
                                    (Action)delegate
                                    {
                                        UpperSmallerLCDKnobDec.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case RadioPanelPZ69KnobsEmulator.UpperLargeFreqWheelInc:
                            {
                                var key = radioKnob;
                                Dispatcher.BeginInvoke(
                                    (Action)delegate
                                    {
                                        UpperLargerLCDKnobInc.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case RadioPanelPZ69KnobsEmulator.UpperLargeFreqWheelDec:
                            {
                                var key = radioKnob;
                                Dispatcher.BeginInvoke(
                                    (Action)delegate
                                    {
                                        UpperLargerLCDKnobDec.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case RadioPanelPZ69KnobsEmulator.LowerSmallFreqWheelInc:
                            {
                                var key = radioKnob;
                                Dispatcher.BeginInvoke(
                                    (Action)delegate
                                    {
                                        LowerSmallerLCDKnobInc.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case RadioPanelPZ69KnobsEmulator.LowerSmallFreqWheelDec:
                            {
                                var key = radioKnob;
                                Dispatcher.BeginInvoke(
                                    (Action)delegate
                                    {
                                        LowerSmallerLCDKnobDec.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case RadioPanelPZ69KnobsEmulator.LowerLargeFreqWheelInc:
                            {
                                var key = radioKnob;
                                Dispatcher.BeginInvoke(
                                    (Action)delegate
                                    {
                                        LowerLargerLCDKnobInc.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case RadioPanelPZ69KnobsEmulator.LowerLargeFreqWheelDec:
                            {
                                var key = radioKnob;
                                Dispatcher.BeginInvoke(
                                    (Action)delegate
                                    {
                                        LowerLargerLCDKnobDec.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case RadioPanelPZ69KnobsEmulator.UpperFreqSwitch:
                            {
                                var key = radioKnob;
                                Dispatcher.BeginInvoke(
                                    (Action)delegate
                                    {
                                        UpperRightSwitch.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case RadioPanelPZ69KnobsEmulator.LowerFreqSwitch:
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
                    Settings.Default.RadioFrequencyKnobSensitivityEmulator = int.Parse(ComboBoxFreqKnobSensitivity.SelectedValue.ToString());
                    _radioPanelPZ69.FrequencyKnobSensitivity = int.Parse(ComboBoxFreqKnobSensitivity.SelectedValue.ToString());
                    Settings.Default.Save();
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(204330, ex);
            }
        }

        private void RadioPanelPZ69UserControlEmulator_OnLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                ComboBoxFreqKnobSensitivity.SelectedValue = Settings.Default.RadioFrequencyKnobSensitivityEmulator;
                SetContextMenuClickHandlers();
                _userControlLoaded = true;
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(204331, ex);
            }
        }

        private RadioPanelPZ69KnobEmulator.RadioPanelPZ69KeyOnOff GetPZ69Key(TextBox textBox)
        {
            try
            {
                if (textBox.Equals(TextBoxUpperCom1))
                {
                    return new RadioPanelPZ69KnobEmulator.RadioPanelPZ69KeyOnOff(RadioPanelPZ69KnobsEmulator.UpperCOM1, true);
                }
                if (textBox.Equals(TextBoxUpperCom2))
                {
                    return new RadioPanelPZ69KnobEmulator.RadioPanelPZ69KeyOnOff(RadioPanelPZ69KnobsEmulator.UpperCOM2, true);
                }
                if (textBox.Equals(TextBoxUpperNav1))
                {
                    return new RadioPanelPZ69KnobEmulator.RadioPanelPZ69KeyOnOff(RadioPanelPZ69KnobsEmulator.UpperNAV1, true);
                }
                if (textBox.Equals(TextBoxUpperNav2))
                {
                    return new RadioPanelPZ69KnobEmulator.RadioPanelPZ69KeyOnOff(RadioPanelPZ69KnobsEmulator.UpperNAV2, true);
                }
                if (textBox.Equals(TextBoxUpperADF))
                {
                    return new RadioPanelPZ69KnobEmulator.RadioPanelPZ69KeyOnOff(RadioPanelPZ69KnobsEmulator.UpperADF, true);
                }
                if (textBox.Equals(TextBoxUpperDME))
                {
                    return new RadioPanelPZ69KnobEmulator.RadioPanelPZ69KeyOnOff(RadioPanelPZ69KnobsEmulator.UpperDME, true);
                }
                if (textBox.Equals(TextBoxUpperXPDR))
                {
                    return new RadioPanelPZ69KnobEmulator.RadioPanelPZ69KeyOnOff(RadioPanelPZ69KnobsEmulator.UpperXPDR, true);
                }
                if (textBox.Equals(TextBoxUpperLargePlus))
                {
                    return new RadioPanelPZ69KnobEmulator.RadioPanelPZ69KeyOnOff(RadioPanelPZ69KnobsEmulator.UpperLargeFreqWheelInc, true);
                }
                if (textBox.Equals(TextBoxUpperLargeMinus))
                {
                    return new RadioPanelPZ69KnobEmulator.RadioPanelPZ69KeyOnOff(RadioPanelPZ69KnobsEmulator.UpperLargeFreqWheelDec, true);
                }
                if (textBox.Equals(TextBoxUpperSmallPlus))
                {
                    return new RadioPanelPZ69KnobEmulator.RadioPanelPZ69KeyOnOff(RadioPanelPZ69KnobsEmulator.UpperSmallFreqWheelInc, true);
                }
                if (textBox.Equals(TextBoxUpperSmallMinus))
                {
                    return new RadioPanelPZ69KnobEmulator.RadioPanelPZ69KeyOnOff(RadioPanelPZ69KnobsEmulator.UpperSmallFreqWheelDec, true);
                }
                if (textBox.Equals(TextBoxUpperActStbyOn))
                {
                    return new RadioPanelPZ69KnobEmulator.RadioPanelPZ69KeyOnOff(RadioPanelPZ69KnobsEmulator.UpperFreqSwitch, true);
                }
                if (textBox.Equals(TextBoxUpperActStbyOff))
                {
                    return new RadioPanelPZ69KnobEmulator.RadioPanelPZ69KeyOnOff(RadioPanelPZ69KnobsEmulator.UpperFreqSwitch, false);
                }
                if (textBox.Equals(TextBoxLowerCom1))
                {
                    return new RadioPanelPZ69KnobEmulator.RadioPanelPZ69KeyOnOff(RadioPanelPZ69KnobsEmulator.LowerCOM1, true);
                }
                if (textBox.Equals(TextBoxLowerCom2))
                {
                    return new RadioPanelPZ69KnobEmulator.RadioPanelPZ69KeyOnOff(RadioPanelPZ69KnobsEmulator.LowerCOM2, true);
                }
                if (textBox.Equals(TextBoxLowerNav1))
                {
                    return new RadioPanelPZ69KnobEmulator.RadioPanelPZ69KeyOnOff(RadioPanelPZ69KnobsEmulator.LowerNAV1, true);
                }
                if (textBox.Equals(TextBoxLowerNav2))
                {
                    return new RadioPanelPZ69KnobEmulator.RadioPanelPZ69KeyOnOff(RadioPanelPZ69KnobsEmulator.LowerNAV2, true);
                }
                if (textBox.Equals(TextBoxLowerADF))
                {
                    return new RadioPanelPZ69KnobEmulator.RadioPanelPZ69KeyOnOff(RadioPanelPZ69KnobsEmulator.LowerADF, true);
                }
                if (textBox.Equals(TextBoxLowerDME))
                {
                    return new RadioPanelPZ69KnobEmulator.RadioPanelPZ69KeyOnOff(RadioPanelPZ69KnobsEmulator.LowerDME, true);
                }
                if (textBox.Equals(TextBoxLowerXPDR))
                {
                    return new RadioPanelPZ69KnobEmulator.RadioPanelPZ69KeyOnOff(RadioPanelPZ69KnobsEmulator.LowerXPDR, true);
                }
                if (textBox.Equals(TextBoxLowerLargePlus))
                {
                    return new RadioPanelPZ69KnobEmulator.RadioPanelPZ69KeyOnOff(RadioPanelPZ69KnobsEmulator.LowerLargeFreqWheelInc, true);
                }
                if (textBox.Equals(TextBoxLowerLargeMinus))
                {
                    return new RadioPanelPZ69KnobEmulator.RadioPanelPZ69KeyOnOff(RadioPanelPZ69KnobsEmulator.LowerLargeFreqWheelDec, true);
                }
                if (textBox.Equals(TextBoxLowerSmallPlus))
                {
                    return new RadioPanelPZ69KnobEmulator.RadioPanelPZ69KeyOnOff(RadioPanelPZ69KnobsEmulator.LowerSmallFreqWheelInc, true);
                }
                if (textBox.Equals(TextBoxLowerSmallMinus))
                {
                    return new RadioPanelPZ69KnobEmulator.RadioPanelPZ69KeyOnOff(RadioPanelPZ69KnobsEmulator.LowerSmallFreqWheelDec, true);
                }
                if (textBox.Equals(TextBoxLowerActStbyOn))
                {
                    return new RadioPanelPZ69KnobEmulator.RadioPanelPZ69KeyOnOff(RadioPanelPZ69KnobsEmulator.LowerFreqSwitch, true);
                }
                if (textBox.Equals(TextBoxLowerActStbyOff))
                {
                    return new RadioPanelPZ69KnobEmulator.RadioPanelPZ69KeyOnOff(RadioPanelPZ69KnobsEmulator.LowerFreqSwitch, false);
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(345012, ex);
            }
            throw new Exception("Should not reach this point");
        }

    }
}
