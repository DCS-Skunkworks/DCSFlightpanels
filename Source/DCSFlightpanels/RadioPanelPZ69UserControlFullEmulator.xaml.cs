using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ClassLibraryCommon;
using NonVisuals;
using DCSFlightpanels.Properties;
using DCS_BIOS;

namespace DCSFlightpanels
{
    /// <summary>
    /// Interaction logic for RadioPanelPZ69UserControlEmulator.xaml
    /// </summary>
    public partial class RadioPanelPZ69UserControlFullEmulator : ISaitekPanelListener, IProfileHandlerListener, ISaitekUserControl
    {
        private readonly RadioPanelPZ69EmulatorFull _radioPanelPZ69;
        private readonly TabItem _parentTabItem;
        private string _parentTabItemHeader;
        private IGlobalHandler _globalHandler;
        private bool _userControlLoaded;
        private bool _textBoxTagsSet;
        private bool _buttonTagsSet;
        private readonly List<Key> _allowedKeys = new List<Key>() { Key.D0, Key.D1, Key.D2, Key.D3, Key.D4, Key.D5, Key.D6, Key.D7, Key.D8, Key.D9, Key.OemPeriod, Key.Delete, Key.Back, Key.Left, Key.Right, Key.NumPad0, Key.NumPad1, Key.NumPad2, Key.NumPad3, Key.NumPad4, Key.NumPad5, Key.NumPad6, Key.NumPad7, Key.NumPad8, Key.NumPad9 };
        private const string UpperText = "Upper Dial Profile : ";
        private const string LowerText = "Lower Dial Profile : ";

        public RadioPanelPZ69UserControlFullEmulator(HIDSkeleton hidSkeleton, TabItem parentTabItem, IGlobalHandler globalHandler)
        {
            InitializeComponent();
            _parentTabItem = parentTabItem;
            _parentTabItemHeader = _parentTabItem.Header.ToString();
            HideAllImages();

            _radioPanelPZ69 = new RadioPanelPZ69EmulatorFull(hidSkeleton);
            _radioPanelPZ69.FrequencyKnobSensitivity = Settings.Default.RadioFrequencyKnobSensitivityEmulator;
            _radioPanelPZ69.Attach((ISaitekPanelListener)this);
            globalHandler.Attach(_radioPanelPZ69);
            _globalHandler = globalHandler;

            //LoadConfiguration();
        }

        private void RadioPanelPZ69UserControlFullEmulator_OnLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                ComboBoxFreqKnobSensitivity.SelectedValue = Settings.Default.RadioFrequencyKnobSensitivityEmulator;
                SetTextBoxTagObjects();
                SetButtonTagObjects();
                SetContextMenuClickHandlers();
                _userControlLoaded = true;
                ShowGraphicConfiguration();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(204331, ex);
            }
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
                //todo
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
                if (e.SaitekPanelEnum == SaitekPanelsEnum.PZ69RadioPanel && e.UniqueId.Equals(_radioPanelPZ69.InstanceId))
                {
                    NotifySwitchChanges(e.Switches);
                }
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
                //todo
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
                //todo
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
                //todo
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
                //todo
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
                //todo
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(2014, ex);
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
                Common.ShowErrorMessageBox(2017, ex);
            }
        }

        public void SettingsApplied(object sender, PanelEventArgs e)
        {
            try
            {
                if (e.UniqueId.Equals(_radioPanelPZ69.InstanceId) && e.SaitekPanelEnum == SaitekPanelsEnum.PZ69RadioPanel)
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

        public void PanelSettingsChanged(object sender, PanelEventArgs e)
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



        private void SetTextBoxTagObjects()
        {
            if (_textBoxTagsSet || !Common.FindVisualChildren<TextBox>(this).Any())
            {
                return;
            }
            foreach (var textBox in Common.FindVisualChildren<TextBox>(this))
            {
                //Debug.WriteLine("Adding TextBoxTagHolderClass for TextBox " + textBox.Name);
                if (textBox.Equals(TextBoxLogPZ69))
                {
                    continue;
                }
                textBox.Tag = new TagDataClassPZ69Full(textBox);
            }
            _textBoxTagsSet = true;
        }

        private void SetButtonTagObjects()
        {
            if (_buttonTagsSet)
            {
                return;
            }
            ButtonUpperLeftLcd.Tag = new TagDataClassPZ69Button(ButtonUpperLeftLcd);
            ButtonLowerLeftLcd.Tag = new TagDataClassPZ69Button(ButtonLowerLeftLcd);
            ButtonUpperRightLcd.Tag = new TagDataClassPZ69Button(ButtonUpperRightLcd);
            ButtonLowerRightLcd.Tag = new TagDataClassPZ69Button(ButtonLowerRightLcd);
            _buttonTagsSet = true;
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
                if (((TagDataClassPZ69Full)textBox.Tag).ContainsKeySequence())
                {
                    sequenceWindow = new SequenceWindow(textBox.Text, ((TagDataClassPZ69Full)textBox.Tag).GetKeySequence());
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
                        var osKeyPress = new OSKeyPress("Key press sequence", sequenceList);
                        ((TagDataClassPZ69Full)textBox.Tag).KeyPress = osKeyPress;
                        ((TagDataClassPZ69Full)textBox.Tag).KeyPress.Information = sequenceWindow.GetInformation;
                        if (!string.IsNullOrEmpty(sequenceWindow.GetInformation))
                        {
                            textBox.Text = sequenceWindow.GetInformation;
                        }
                        UpdateKeyBindingProfileSequencedKeyStrokesPZ69(textBox);
                    }
                    else
                    {
                        //If only one press was created treat it as a simple keypress
                        ((TagDataClassPZ69Full)textBox.Tag).ClearAll();
                        var osKeyPress = new OSKeyPress(sequenceList[0].VirtualKeyCodesAsString, sequenceList[0].LengthOfKeyPress);
                        ((TagDataClassPZ69Full)textBox.Tag).KeyPress = osKeyPress;
                        ((TagDataClassPZ69Full)textBox.Tag).KeyPress.Information = sequenceWindow.GetInformation;
                        textBox.Text = sequenceList[0].VirtualKeyCodesAsString;
                        UpdateKeyBindingProfileSimpleKeyStrokes(textBox);
                    }
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(2044, ex);
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
                if (((TagDataClassPZ69Full)textBox.Tag).ContainsBIPLink())
                {
                    var bipLink = ((TagDataClassPZ69Full)textBox.Tag).BIPLink;
                    bipLinkWindow = new BIPLinkWindow(bipLink);
                }
                else
                {
                    var bipLink = new BIPLinkPZ69();
                    bipLinkWindow = new BIPLinkWindow(bipLink);
                }
                bipLinkWindow.ShowDialog();
                if (bipLinkWindow.DialogResult.HasValue && bipLinkWindow.DialogResult == true && bipLinkWindow.IsDirty && bipLinkWindow.BIPLink != null && bipLinkWindow.BIPLink.BIPLights.Count > 0)
                {
                    ((TagDataClassPZ69Full)textBox.Tag).BIPLink = (BIPLinkPZ69)bipLinkWindow.BIPLink;
                    UpdateBipLinkBindings(textBox);
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(442044, ex);
            }
        }


        private void UpdateKeyBindingProfileSequencedKeyStrokesPZ69(TextBox textBox)
        {
            try
            {
                var key = GetPZ69Key(textBox);
                _radioPanelPZ69.AddOrUpdateSequencedKeyBinding(textBox.Text, key.RadioPanelPZ69Key, ((TagDataClassPZ69Full)textBox.Tag).GetKeySequence(), key.ButtonState);
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(3011, ex);
            }
        }


        private void UpdateBipLinkBindings(TextBox textBox)
        {
            try
            {
                var key = GetPZ69Key(textBox);
                _radioPanelPZ69.AddOrUpdateBIPLinkKeyBinding(key.RadioPanelPZ69Key, ((TagDataClassPZ69Full)textBox.Tag).BIPLink, key.ButtonState);
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
                if (!((TagDataClassPZ69Full)textBox.Tag).ContainsOSKeyPress() || ((TagDataClassPZ69Full)textBox.Tag).KeyPress.KeySequence.Count == 0)
                {
                    keyPressLength = KeyPressLength.FiftyMilliSec;
                }
                else
                {
                    keyPressLength = ((TagDataClassPZ69Full)textBox.Tag).KeyPress.GetLengthOfKeyPress();
                }
                var key = GetPZ69Key(textBox);
                _radioPanelPZ69.AddOrUpdateSingleKeyBinding(key.RadioPanelPZ69Key, TextBoxUpperCom1.Text, keyPressLength, key.ButtonState);
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



                if (!((TagDataClassPZ69Full)textBox.Tag).ContainsSingleKey())
                {
                    return;
                }
                var keyPressLength = ((TagDataClassPZ69Full)textBox.Tag).KeyPress.GetLengthOfKeyPress();

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
                if (textBox.Equals(TextBoxLogPZ69))
                {
                    continue;
                }
                var tagHolderClass = (TagDataClassPZ69Full)textBox.Tag;
                textBox.Text = "";
                tagHolderClass.ClearAll();
            }
            if (clearAlsoProfile)
            {
                _radioPanelPZ69.ClearSettings();
            }
        }

        private void ClearAllDisplayValues()
        {
            foreach (var textBox in Common.FindVisualChildren<TextBox>(this))
            {
                if (textBox.Name.EndsWith("Numbers"))
                {
                    textBox.Text = "";
                }
            }
        }

        private void ClearCommands()
        {
            foreach (var textBox in Common.FindVisualChildren<TextBox>(this))
            {
                if (!textBox.Name.EndsWith("Numbers"))
                {
                    var tagHolderClass = (TagDataClassPZ69Full) textBox.Tag;
                    textBox.Text = "";
                    tagHolderClass.ClearAll();
                }
            }
        }


        private void RemoveContextMenuClickHandlers()
        {
            foreach (var textBox in Common.FindVisualChildren<TextBox>(this))
            {
                if (!Equals(textBox, TextBoxLogPZ69))
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
                if (!Equals(textBox, TextBoxLogPZ69) && !textBox.Name.EndsWith("Numbers"))
                {
                    var contectMenu = (ContextMenu)Resources["TextBoxContextMenuPZ69"];
                    if (!BipFactory.HasBips())
                    {
                        MenuItem bipMenuItem = null;
                        foreach (var item in contectMenu.Items)
                        {
                            if (((MenuItem)item).Name == "contextMenuItemEditBIP")
                            {
                                bipMenuItem = (MenuItem)item;
                                break;
                            }
                        }
                        if (bipMenuItem != null)
                        {
                            contectMenu.Items.Remove(bipMenuItem);
                        }
                    }
                    textBox.ContextMenu = contectMenu;
                    textBox.ContextMenuOpening += TextBoxContextMenuOpening;
                }
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

                if (((TagDataClassPZ69Full)textBox.Tag).ContainsDCSBIOS())
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
                else if (((TagDataClassPZ69Full)textBox.Tag).ContainsKeySequence())
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
                else if (((TagDataClassPZ69Full)textBox.Tag).IsEmpty())
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
                else if (((TagDataClassPZ69Full)textBox.Tag).ContainsSingleKey())
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
                else if (((TagDataClassPZ69Full)textBox.Tag).ContainsBIPLink())
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


        private TextBox GetTextBoxInFocus()
        {
            foreach (var textBox in Common.FindVisualChildren<TextBox>(this))
            {
                if (!Equals(textBox, TextBoxLogPZ69) && textBox.IsFocused && Equals(textBox.Background, Brushes.Yellow))
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
                if (contextMenuItem.Name == "contextMenuItemFiftyMilliSec")
                {
                    ((TagDataClassPZ69Full)textBox.Tag).KeyPress.SetLengthOfKeyPress(KeyPressLength.FiftyMilliSec);
                }
                else if (contextMenuItem.Name == "contextMenuItemHalfSecond")
                {
                    ((TagDataClassPZ69Full)textBox.Tag).KeyPress.SetLengthOfKeyPress(KeyPressLength.HalfSecond);
                }
                else if (contextMenuItem.Name == "contextMenuItemSecond")
                {
                    ((TagDataClassPZ69Full)textBox.Tag).KeyPress.SetLengthOfKeyPress(KeyPressLength.Second);
                }
                else if (contextMenuItem.Name == "contextMenuItemSecondAndHalf")
                {
                    ((TagDataClassPZ69Full)textBox.Tag).KeyPress.SetLengthOfKeyPress(KeyPressLength.SecondAndHalf);
                }
                else if (contextMenuItem.Name == "contextMenuItemTwoSeconds")
                {
                    ((TagDataClassPZ69Full)textBox.Tag).KeyPress.SetLengthOfKeyPress(KeyPressLength.TwoSeconds);
                }
                else if (contextMenuItem.Name == "contextMenuItemThreeSeconds")
                {
                    ((TagDataClassPZ69Full)textBox.Tag).KeyPress.SetLengthOfKeyPress(KeyPressLength.ThreeSeconds);
                }
                else if (contextMenuItem.Name == "contextMenuItemFourSeconds")
                {
                    ((TagDataClassPZ69Full)textBox.Tag).KeyPress.SetLengthOfKeyPress(KeyPressLength.FourSeconds);
                }
                else if (contextMenuItem.Name == "contextMenuItemFiveSecs")
                {
                    ((TagDataClassPZ69Full)textBox.Tag).KeyPress.SetLengthOfKeyPress(KeyPressLength.FiveSecs);
                }
                else if (contextMenuItem.Name == "contextMenuItemTenSecs")
                {
                    ((TagDataClassPZ69Full)textBox.Tag).KeyPress.SetLengthOfKeyPress(KeyPressLength.TenSecs);
                }
                else if (contextMenuItem.Name == "contextMenuItemFifteenSecs")
                {
                    ((TagDataClassPZ69Full)textBox.Tag).KeyPress.SetLengthOfKeyPress(KeyPressLength.FifteenSecs);
                }
                else if (contextMenuItem.Name == "contextMenuItemTwentySecs")
                {
                    ((TagDataClassPZ69Full)textBox.Tag).KeyPress.SetLengthOfKeyPress(KeyPressLength.TwentySecs);
                }
                else if (contextMenuItem.Name == "contextMenuItemThirtySecs")
                {
                    ((TagDataClassPZ69Full)textBox.Tag).KeyPress.SetLengthOfKeyPress(KeyPressLength.ThirtySecs);
                }
                else if (contextMenuItem.Name == "contextMenuItemFortySecs")
                {
                    ((TagDataClassPZ69Full)textBox.Tag).KeyPress.SetLengthOfKeyPress(KeyPressLength.FortySecs);
                }
                else if (contextMenuItem.Name == "contextMenuItemSixtySecs")
                {
                    ((TagDataClassPZ69Full)textBox.Tag).KeyPress.SetLengthOfKeyPress(KeyPressLength.SixtySecs);
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
                    if (((TagDataClassPZ69Full)textBox.Tag).ContainsKeySequence())
                    {
                        //Check if this textbox contains sequence information. If so then prompt the user for deletion
                        if (MessageBox.Show("Do you want to delete the key sequence?", "Delete key sequence?", MessageBoxButton.OKCancel, MessageBoxImage.Question) != MessageBoxResult.OK)
                        {
                            return;
                        }
                        ((TagDataClassPZ69Full)textBox.Tag).KeyPress.KeySequence.Clear();
                        textBox.Text = "";
                        UpdateKeyBindingProfileSimpleKeyStrokes(textBox);
                    }
                    else if (((TagDataClassPZ69Full)textBox.Tag).ContainsSingleKey())
                    {
                        ((TagDataClassPZ69Full)textBox.Tag).KeyPress.KeySequence.Clear();
                        textBox.Text = "";
                        UpdateKeyBindingProfileSimpleKeyStrokes(textBox);
                    }
                    if (((TagDataClassPZ69Full)textBox.Tag).ContainsBIPLink())
                    {
                        //Check if this textbox contains sequence information. If so then prompt the user for deletion
                        if (MessageBox.Show("Do you want to delete BIP Links?", "Delete BIP Link?", MessageBoxButton.OKCancel, MessageBoxImage.Question) != MessageBoxResult.OK)
                        {
                            return;
                        }
                        ((TagDataClassPZ69Full)textBox.Tag).BIPLink.BIPLights.Clear();
                        textBox.Background = Brushes.White;
                        UpdateBipLinkBindings(textBox);
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
                if (MessageBox.Show("Clear all settings for the Radio Panel?", "Confirm", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
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
            var textBox = (TextBox)sender;
            if (((TagDataClassPZ69Full)textBox.Tag).ContainsBIPLink())
            {
                ((TextBox)sender).Background = Brushes.Bisque;
            }
            else
            {
                ((TextBox)sender).Background = Brushes.White;
            }
        }


        private void TextBoxPreviewKeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                var textBox = ((TextBox)sender);

                //Check if this textbox contains sequence or DCS-BIOS information. If so then exit
                if (((TagDataClassPZ69Full)textBox.Tag).ContainsKeySequence())
                {
                    return;
                }
                var hashSetOfKeysPressed = new HashSet<string>();

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
                if (((TagDataClassPZ69Full)textBox.Tag).ContainsKeySequence())
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
                        else if (textBox.Name.Contains("Com2"))
                        {
                            radioPanelKnob = RadioPanelPZ69KnobsEmulator.UpperCOM2;
                        }
                        else if (textBox.Name.Contains("Nav1"))
                        {
                            radioPanelKnob = RadioPanelPZ69KnobsEmulator.UpperNAV1;
                        }
                        else if (textBox.Name.Contains("Nav2"))
                        {
                            radioPanelKnob = RadioPanelPZ69KnobsEmulator.UpperNAV2;
                        }
                        else if (textBox.Name.Contains("ADF"))
                        {
                            radioPanelKnob = RadioPanelPZ69KnobsEmulator.UpperADF;
                        }
                        else if (textBox.Name.Contains("DME"))
                        {
                            radioPanelKnob = RadioPanelPZ69KnobsEmulator.UpperDME;
                        }
                        else if (textBox.Name.Contains("XPDR"))
                        {
                            radioPanelKnob = RadioPanelPZ69KnobsEmulator.UpperXPDR;
                        }

                        else if (textBox.Name.Contains("Active"))
                        {
                            radioDisplay = RadioPanelPZ69Display.UpperActive;
                        }

                        else if (textBox.Name.Contains("Standby"))
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
                        else if (textBox.Name.Contains("Com2"))
                        {
                            radioPanelKnob = RadioPanelPZ69KnobsEmulator.LowerCOM2;
                        }
                        else if (textBox.Name.Contains("Nav1"))
                        {
                            radioPanelKnob = RadioPanelPZ69KnobsEmulator.LowerNAV1;
                        }
                        else if (textBox.Name.Contains("Nav2"))
                        {
                            radioPanelKnob = RadioPanelPZ69KnobsEmulator.LowerNAV2;
                        }
                        else if (textBox.Name.Contains("ADF"))
                        {
                            radioPanelKnob = RadioPanelPZ69KnobsEmulator.LowerADF;
                        }
                        else if (textBox.Name.Contains("DME"))
                        {
                            radioPanelKnob = RadioPanelPZ69KnobsEmulator.LowerDME;
                        }
                        else if (textBox.Name.Contains("XPDR"))
                        {
                            radioPanelKnob = RadioPanelPZ69KnobsEmulator.LowerXPDR;
                        }

                        else if (textBox.Name.Contains("Active"))
                        {
                            radioDisplay = RadioPanelPZ69Display.LowerActive;
                        }

                        if (textBox.Name.Contains("Standby"))
                        {
                            radioDisplay = RadioPanelPZ69Display.LowerStandby;
                        }
                    }

                    _radioPanelPZ69.AddOrUpdateDisplayValue(radioPanelKnob, "-1", radioDisplay);
                    ClearAllDisplayValues();
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
                if (((TagDataClassPZ69Full)textBox.Tag).ContainsKeySequence())
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
                if (((TagDataClassPZ69Full)textBox.Tag).ContainsKeySequence())
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

                    if (_radioPanelPZ69.ForwardPanelEvent)
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
                             TextBoxLogPZ69.Text = TextBoxLogPZ69.Text.Insert(0, "No action taken, panel events Disabled.\n")));
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
                if (!_userControlLoaded || !_textBoxTagsSet)
                {
                    return;
                }
                HideButtonImages();

                foreach (var displayValue in _radioPanelPZ69.DisplayValueHashSet)
                {
                    
                    if (displayValue.RadioPanelPZ69Knob == RadioPanelPZ69KnobsEmulator.UpperCOM1)
                    {
                        if (displayValue.RadioPanelDisplay == RadioPanelPZ69Display.UpperActive)
                        {
                            TextBoxUpperCom1ActiveNumbers.Text =
                                displayValue.Value.ToString(Common.GetPZ69FullDisplayNumberFormat());
                        }

                        if (displayValue.RadioPanelDisplay == RadioPanelPZ69Display.UpperStandby)
                        {
                            TextBoxUpperCom1StandbyNumbers.Text =
                                displayValue.Value.ToString(Common.GetPZ69FullDisplayNumberFormat());
                        }
                    }
                    else if (displayValue.RadioPanelPZ69Knob == RadioPanelPZ69KnobsEmulator.UpperCOM2)
                    {
                        if (displayValue.RadioPanelDisplay == RadioPanelPZ69Display.UpperActive)
                        {
                            TextBoxUpperCom2ActiveNumbers.Text =
                                displayValue.Value.ToString(Common.GetPZ69FullDisplayNumberFormat());
                        }

                        if (displayValue.RadioPanelDisplay == RadioPanelPZ69Display.UpperStandby)
                        {
                            TextBoxUpperCom2StandbyNumbers.Text =
                                displayValue.Value.ToString(Common.GetPZ69FullDisplayNumberFormat());
                        }
                    }
                    else if (displayValue.RadioPanelPZ69Knob == RadioPanelPZ69KnobsEmulator.UpperNAV1)
                    {
                        if (displayValue.RadioPanelDisplay == RadioPanelPZ69Display.UpperActive)
                        {
                            TextBoxUpperNav1ActiveNumbers.Text =
                                displayValue.Value.ToString(Common.GetPZ69FullDisplayNumberFormat());
                        }

                        if (displayValue.RadioPanelDisplay == RadioPanelPZ69Display.UpperStandby)
                        {
                            TextBoxUpperNav1StandbyNumbers.Text =
                                displayValue.Value.ToString(Common.GetPZ69FullDisplayNumberFormat());
                        }
                    }
                    else if (displayValue.RadioPanelPZ69Knob == RadioPanelPZ69KnobsEmulator.UpperNAV2)
                    {
                        if (displayValue.RadioPanelDisplay == RadioPanelPZ69Display.UpperActive)
                        {
                            TextBoxUpperNav2ActiveNumbers.Text =
                                displayValue.Value.ToString(Common.GetPZ69FullDisplayNumberFormat());
                        }

                        if (displayValue.RadioPanelDisplay == RadioPanelPZ69Display.UpperStandby)
                        {
                            TextBoxUpperNav2StandbyNumbers.Text =
                                displayValue.Value.ToString(Common.GetPZ69FullDisplayNumberFormat());
                        }
                    }
                    else if (displayValue.RadioPanelPZ69Knob == RadioPanelPZ69KnobsEmulator.UpperADF)
                    {
                        if (displayValue.RadioPanelDisplay == RadioPanelPZ69Display.UpperActive)
                        {
                            TextBoxUpperADFActiveNumbers.Text =
                                displayValue.Value.ToString(Common.GetPZ69FullDisplayNumberFormat());
                        }

                        if (displayValue.RadioPanelDisplay == RadioPanelPZ69Display.UpperStandby)
                        {
                            TextBoxUpperADFStandbyNumbers.Text =
                                displayValue.Value.ToString(Common.GetPZ69FullDisplayNumberFormat());
                        }
                    }
                    else if (displayValue.RadioPanelPZ69Knob == RadioPanelPZ69KnobsEmulator.UpperDME)
                    {
                        if (displayValue.RadioPanelDisplay == RadioPanelPZ69Display.UpperActive)
                        {
                            TextBoxUpperDMEActiveNumbers.Text =
                                displayValue.Value.ToString(Common.GetPZ69FullDisplayNumberFormat());
                        }

                        if (displayValue.RadioPanelDisplay == RadioPanelPZ69Display.UpperStandby)
                        {
                            TextBoxUpperDMEStandbyNumbers.Text =
                                displayValue.Value.ToString(Common.GetPZ69FullDisplayNumberFormat());
                        }
                    }
                    else if (displayValue.RadioPanelPZ69Knob == RadioPanelPZ69KnobsEmulator.UpperXPDR)
                    {
                        if (displayValue.RadioPanelDisplay == RadioPanelPZ69Display.UpperActive)
                        {
                            TextBoxUpperXPDRActiveNumbers.Text =
                                displayValue.Value.ToString(Common.GetPZ69FullDisplayNumberFormat());
                        }

                        if (displayValue.RadioPanelDisplay == RadioPanelPZ69Display.UpperStandby)
                        {
                            TextBoxUpperXPDRStandbyNumbers.Text =
                                displayValue.Value.ToString(Common.GetPZ69FullDisplayNumberFormat());
                        }
                    }
                    else if (displayValue.RadioPanelPZ69Knob == RadioPanelPZ69KnobsEmulator.LowerCOM1)
                    {
                        if (displayValue.RadioPanelDisplay == RadioPanelPZ69Display.LowerActive)
                        {
                            TextBoxLowerCom1ActiveNumbers.Text =
                                displayValue.Value.ToString(Common.GetPZ69FullDisplayNumberFormat());
                        }

                        if (displayValue.RadioPanelDisplay == RadioPanelPZ69Display.LowerStandby)
                        {
                            TextBoxLowerCom1StandbyNumbers.Text =
                                displayValue.Value.ToString(Common.GetPZ69FullDisplayNumberFormat());
                        }
                    }
                    else if (displayValue.RadioPanelPZ69Knob == RadioPanelPZ69KnobsEmulator.LowerCOM2)
                    {
                        if (displayValue.RadioPanelDisplay == RadioPanelPZ69Display.LowerActive)
                        {
                            TextBoxLowerCom2ActiveNumbers.Text =
                                displayValue.Value.ToString(Common.GetPZ69FullDisplayNumberFormat());
                        }

                        if (displayValue.RadioPanelDisplay == RadioPanelPZ69Display.LowerStandby)
                        {
                            TextBoxLowerCom2StandbyNumbers.Text =
                                displayValue.Value.ToString(Common.GetPZ69FullDisplayNumberFormat());
                        }
                    }
                    else if (displayValue.RadioPanelPZ69Knob == RadioPanelPZ69KnobsEmulator.LowerNAV1)
                    {
                        if (displayValue.RadioPanelDisplay == RadioPanelPZ69Display.LowerActive)
                        {
                            TextBoxLowerNav1ActiveNumbers.Text =
                                displayValue.Value.ToString(Common.GetPZ69FullDisplayNumberFormat());
                        }

                        if (displayValue.RadioPanelDisplay == RadioPanelPZ69Display.LowerStandby)
                        {
                            TextBoxLowerNav1StandbyNumbers.Text =
                                displayValue.Value.ToString(Common.GetPZ69FullDisplayNumberFormat());
                        }
                    }
                    else if (displayValue.RadioPanelPZ69Knob == RadioPanelPZ69KnobsEmulator.LowerNAV2)
                    {
                        if (displayValue.RadioPanelDisplay == RadioPanelPZ69Display.LowerActive)
                        {
                            TextBoxLowerNav2ActiveNumbers.Text =
                                displayValue.Value.ToString(Common.GetPZ69FullDisplayNumberFormat());
                        }

                        if (displayValue.RadioPanelDisplay == RadioPanelPZ69Display.LowerStandby)
                        {
                            TextBoxLowerNav2StandbyNumbers.Text =
                                displayValue.Value.ToString(Common.GetPZ69FullDisplayNumberFormat());
                        }
                    }
                    else if (displayValue.RadioPanelPZ69Knob == RadioPanelPZ69KnobsEmulator.LowerADF)
                    {
                        if (displayValue.RadioPanelDisplay == RadioPanelPZ69Display.LowerActive)
                        {
                            TextBoxLowerADFActiveNumbers.Text =
                                displayValue.Value.ToString(Common.GetPZ69FullDisplayNumberFormat());
                        }

                        if (displayValue.RadioPanelDisplay == RadioPanelPZ69Display.LowerStandby)
                        {
                            TextBoxLowerADFStandbyNumbers.Text =
                                displayValue.Value.ToString(Common.GetPZ69FullDisplayNumberFormat());
                        }
                    }
                    else if (displayValue.RadioPanelPZ69Knob == RadioPanelPZ69KnobsEmulator.LowerDME)
                    {
                        if (displayValue.RadioPanelDisplay == RadioPanelPZ69Display.LowerActive)
                        {
                            TextBoxLowerDMEActiveNumbers.Text =
                                displayValue.Value.ToString(Common.GetPZ69FullDisplayNumberFormat());
                        }

                        if (displayValue.RadioPanelDisplay == RadioPanelPZ69Display.LowerStandby)
                        {
                            TextBoxLowerDMEStandbyNumbers.Text =
                                displayValue.Value.ToString(Common.GetPZ69FullDisplayNumberFormat());
                        }
                    }
                    else if (displayValue.RadioPanelPZ69Knob == RadioPanelPZ69KnobsEmulator.LowerXPDR)
                    {
                        if (displayValue.RadioPanelDisplay == RadioPanelPZ69Display.LowerActive)
                        {
                            TextBoxLowerXPDRActiveNumbers.Text =
                                displayValue.Value.ToString(Common.GetPZ69FullDisplayNumberFormat());
                        }

                        if (displayValue.RadioPanelDisplay == RadioPanelPZ69Display.LowerStandby)
                        {
                            TextBoxLowerXPDRStandbyNumbers.Text =
                                displayValue.Value.ToString(Common.GetPZ69FullDisplayNumberFormat());
                        }
                    }
                }

                foreach (var keyBinding in _radioPanelPZ69.KeyBindingsHashSet)
                {
                    if (_radioPanelPZ69.IsBindingActive(keyBinding) && keyBinding.RadioPanelPZ69Key == RadioPanelPZ69KnobsEmulator.UpperCOM1)
                    {
                        if (keyBinding.OSKeyPress != null)
                        {
                            ((TagDataClassPZ69Full)TextBoxUpperCom1.Tag).KeyPress = keyBinding.OSKeyPress;
                        }
                    }

                    if (_radioPanelPZ69.IsBindingActive(keyBinding) && keyBinding.RadioPanelPZ69Key == RadioPanelPZ69KnobsEmulator.UpperCOM2)
                    {
                        if (keyBinding.OSKeyPress != null)
                        {
                            ((TagDataClassPZ69Full)TextBoxUpperCom2.Tag).KeyPress = keyBinding.OSKeyPress;
                        }
                    }

                    if (_radioPanelPZ69.IsBindingActive(keyBinding) && keyBinding.RadioPanelPZ69Key == RadioPanelPZ69KnobsEmulator.UpperNAV1)
                    {
                        if (keyBinding.OSKeyPress != null)
                        {
                            ((TagDataClassPZ69Full)TextBoxUpperNav1.Tag).KeyPress = keyBinding.OSKeyPress;
                        }
                    }

                    if (_radioPanelPZ69.IsBindingActive(keyBinding) && keyBinding.RadioPanelPZ69Key == RadioPanelPZ69KnobsEmulator.UpperNAV2)
                    {
                        if (keyBinding.OSKeyPress != null)
                        {
                            ((TagDataClassPZ69Full)TextBoxUpperNav2.Tag).KeyPress = keyBinding.OSKeyPress;
                        }
                    }

                    if (_radioPanelPZ69.IsBindingActive(keyBinding) && keyBinding.RadioPanelPZ69Key == RadioPanelPZ69KnobsEmulator.UpperADF)
                    {
                        if (keyBinding.OSKeyPress != null)
                        {
                            ((TagDataClassPZ69Full)TextBoxUpperADF.Tag).KeyPress = keyBinding.OSKeyPress;
                        }
                    }

                    if (_radioPanelPZ69.IsBindingActive(keyBinding) && keyBinding.RadioPanelPZ69Key == RadioPanelPZ69KnobsEmulator.UpperDME)
                    {
                        if (keyBinding.OSKeyPress != null)
                        {
                            ((TagDataClassPZ69Full)TextBoxUpperDME.Tag).KeyPress = keyBinding.OSKeyPress;
                        }
                    }

                    if (_radioPanelPZ69.IsBindingActive(keyBinding) && keyBinding.RadioPanelPZ69Key == RadioPanelPZ69KnobsEmulator.UpperXPDR)
                    {
                        if (keyBinding.OSKeyPress != null)
                        {
                            ((TagDataClassPZ69Full)TextBoxUpperXPDR.Tag).KeyPress = keyBinding.OSKeyPress;
                        }

                    }

                    if (_radioPanelPZ69.IsBindingActive(keyBinding) && keyBinding.RadioPanelPZ69Key == RadioPanelPZ69KnobsEmulator.UpperLargeFreqWheelInc)
                    {
                        if (keyBinding.OSKeyPress != null)
                        {
                            ((TagDataClassPZ69Full)TextBoxUpperLargePlus.Tag).KeyPress = keyBinding.OSKeyPress;
                        }
                    }

                    if (_radioPanelPZ69.IsBindingActive(keyBinding) && keyBinding.RadioPanelPZ69Key == RadioPanelPZ69KnobsEmulator.UpperLargeFreqWheelDec)
                    {
                        if (keyBinding.OSKeyPress != null)
                        {
                            ((TagDataClassPZ69Full)TextBoxUpperLargeMinus.Tag).KeyPress = keyBinding.OSKeyPress;
                        }
                    }

                    if (_radioPanelPZ69.IsBindingActive(keyBinding) && keyBinding.RadioPanelPZ69Key == RadioPanelPZ69KnobsEmulator.UpperSmallFreqWheelInc)
                    {
                        if (keyBinding.OSKeyPress != null)
                        {
                            ((TagDataClassPZ69Full)TextBoxUpperSmallPlus.Tag).KeyPress = keyBinding.OSKeyPress;
                        }
                    }

                    if (_radioPanelPZ69.IsBindingActive(keyBinding) && keyBinding.RadioPanelPZ69Key == RadioPanelPZ69KnobsEmulator.UpperSmallFreqWheelDec)
                    {
                        if (keyBinding.OSKeyPress != null)
                        {
                            ((TagDataClassPZ69Full)TextBoxUpperSmallMinus.Tag).KeyPress = keyBinding.OSKeyPress;
                        }
                    }

                    if (_radioPanelPZ69.IsBindingActive(keyBinding) && keyBinding.RadioPanelPZ69Key == RadioPanelPZ69KnobsEmulator.UpperFreqSwitch)
                    {
                        if (keyBinding.WhenTurnedOn)
                        {
                            if (keyBinding.OSKeyPress != null)
                            {
                                ((TagDataClassPZ69Full)TextBoxUpperActStbyOn.Tag).KeyPress = keyBinding.OSKeyPress;
                            }
                        }
                        else
                        {
                            if (keyBinding.OSKeyPress != null)
                            {
                                ((TagDataClassPZ69Full)TextBoxUpperActStbyOff.Tag).KeyPress = keyBinding.OSKeyPress;
                            }
                        }
                    }

                    if (_radioPanelPZ69.IsBindingActive(keyBinding) && keyBinding.RadioPanelPZ69Key == RadioPanelPZ69KnobsEmulator.LowerCOM1)
                    {
                        if (keyBinding.WhenTurnedOn)
                        {
                            if (keyBinding.OSKeyPress != null)
                            {
                                ((TagDataClassPZ69Full)TextBoxLowerCom1.Tag).KeyPress = keyBinding.OSKeyPress;
                            }
                        }
                    }

                    if (_radioPanelPZ69.IsBindingActive(keyBinding) && keyBinding.RadioPanelPZ69Key == RadioPanelPZ69KnobsEmulator.LowerCOM2)
                    {
                        if (keyBinding.WhenTurnedOn)
                        {
                            if (keyBinding.OSKeyPress != null)
                            {
                                ((TagDataClassPZ69Full)TextBoxLowerCom2.Tag).KeyPress = keyBinding.OSKeyPress;
                            }
                        }
                    }

                    if (_radioPanelPZ69.IsBindingActive(keyBinding) && keyBinding.RadioPanelPZ69Key == RadioPanelPZ69KnobsEmulator.LowerNAV1)
                    {
                        if (keyBinding.WhenTurnedOn)
                        {
                            if (keyBinding.OSKeyPress != null)
                            {
                                ((TagDataClassPZ69Full)TextBoxLowerNav1.Tag).KeyPress = keyBinding.OSKeyPress;
                            }
                        }
                    }

                    if (_radioPanelPZ69.IsBindingActive(keyBinding) && keyBinding.RadioPanelPZ69Key == RadioPanelPZ69KnobsEmulator.LowerNAV2)
                    {
                        if (keyBinding.WhenTurnedOn)
                        {
                            if (keyBinding.OSKeyPress != null)
                            {
                                ((TagDataClassPZ69Full)TextBoxLowerNav2.Tag).KeyPress = keyBinding.OSKeyPress;
                            }
                        }
                    }

                    if (_radioPanelPZ69.IsBindingActive(keyBinding) && keyBinding.RadioPanelPZ69Key == RadioPanelPZ69KnobsEmulator.LowerADF)
                    {
                        if (keyBinding.WhenTurnedOn)
                        {
                            if (keyBinding.OSKeyPress != null)
                            {
                                ((TagDataClassPZ69Full)TextBoxLowerADF.Tag).KeyPress = keyBinding.OSKeyPress;
                            }
                        }
                    }

                    if (_radioPanelPZ69.IsBindingActive(keyBinding) && keyBinding.RadioPanelPZ69Key == RadioPanelPZ69KnobsEmulator.LowerDME)
                    {
                        if (keyBinding.WhenTurnedOn)
                        {
                            if (keyBinding.OSKeyPress != null)
                            {
                                ((TagDataClassPZ69Full)TextBoxLowerDME.Tag).KeyPress = keyBinding.OSKeyPress;
                            }
                        }
                    }

                    if (_radioPanelPZ69.IsBindingActive(keyBinding) && keyBinding.RadioPanelPZ69Key == RadioPanelPZ69KnobsEmulator.LowerXPDR)
                    {
                        if (keyBinding.WhenTurnedOn)
                        {
                            if (keyBinding.OSKeyPress != null)
                            {
                                ((TagDataClassPZ69Full)TextBoxLowerXPDR.Tag).KeyPress = keyBinding.OSKeyPress;
                            }
                        }
                    }

                    if (_radioPanelPZ69.IsBindingActive(keyBinding) && keyBinding.RadioPanelPZ69Key == RadioPanelPZ69KnobsEmulator.LowerLargeFreqWheelInc)
                    {
                        if (keyBinding.OSKeyPress != null)
                        {
                            ((TagDataClassPZ69Full)TextBoxLowerLargePlus.Tag).KeyPress = keyBinding.OSKeyPress;
                        }
                    }

                    if (_radioPanelPZ69.IsBindingActive(keyBinding) && keyBinding.RadioPanelPZ69Key == RadioPanelPZ69KnobsEmulator.LowerLargeFreqWheelDec)
                    {
                        if (keyBinding.OSKeyPress != null)
                        {
                            ((TagDataClassPZ69Full)TextBoxLowerLargeMinus.Tag).KeyPress = keyBinding.OSKeyPress;
                        }
                    }

                    if (_radioPanelPZ69.IsBindingActive(keyBinding) && keyBinding.RadioPanelPZ69Key == RadioPanelPZ69KnobsEmulator.LowerSmallFreqWheelInc)
                    {
                        if (keyBinding.OSKeyPress != null)
                        {
                            ((TagDataClassPZ69Full)TextBoxLowerSmallPlus.Tag).KeyPress = keyBinding.OSKeyPress;
                        }
                    }

                    if (_radioPanelPZ69.IsBindingActive(keyBinding) && keyBinding.RadioPanelPZ69Key == RadioPanelPZ69KnobsEmulator.LowerSmallFreqWheelDec)
                    {
                        if (keyBinding.OSKeyPress != null)
                        {
                            ((TagDataClassPZ69Full)TextBoxLowerSmallMinus.Tag).KeyPress = keyBinding.OSKeyPress;
                        }
                    }

                    if (_radioPanelPZ69.IsBindingActive(keyBinding) && keyBinding.RadioPanelPZ69Key == RadioPanelPZ69KnobsEmulator.LowerFreqSwitch)
                    {
                        if (keyBinding.WhenTurnedOn)
                        {
                            if (keyBinding.OSKeyPress != null)
                            {
                                ((TagDataClassPZ69Full)TextBoxLowerActStbyOn.Tag).KeyPress = keyBinding.OSKeyPress;
                            }
                        }
                        else
                        {
                            if (keyBinding.OSKeyPress != null)
                            {
                                ((TagDataClassPZ69Full)TextBoxLowerActStbyOff.Tag).KeyPress = keyBinding.OSKeyPress;
                            }
                        }
                    }
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
                            ((TagDataClassPZ69Button)ButtonUpperLeftLcd.Tag).DCSBIOSBindingLCD = lcdBinding;
                            DotTopLeftLcd.Visibility = Visibility.Visible;
                        }

                        if (lcdBinding.PZ69LcdPosition == PZ69LCDPosition.UPPER_STBY_RIGHT)
                        {
                            ((TagDataClassPZ69Button)ButtonUpperRightLcd.Tag).DCSBIOSBindingLCD = lcdBinding;
                            DotTopRightLcd.Visibility = Visibility.Visible;
                        }

                    }
                    else if (lcdBinding.DialPosition == _radioPanelPZ69.PZ69LowerDialPosition)
                    {
                        if (lcdBinding.PZ69LcdPosition == PZ69LCDPosition.LOWER_ACTIVE_LEFT)
                        {
                            ((TagDataClassPZ69Button)ButtonLowerLeftLcd.Tag).DCSBIOSBindingLCD = lcdBinding;
                            DotBottomLeftLcd.Visibility = Visibility.Visible;
                        }

                        if (lcdBinding.PZ69LcdPosition == PZ69LCDPosition.LOWER_STBY_RIGHT)
                        {
                            ((TagDataClassPZ69Button)ButtonLowerRightLcd.Tag).DCSBIOSBindingLCD = lcdBinding;
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

                    if (dcsbiosBinding.DialPosition == _radioPanelPZ69.PZ69UpperDialPosition)
                    {
                        if (dcsbiosBinding.RadioPanelPZ69Knob == RadioPanelPZ69KnobsEmulator.UpperCOM1)
                        {
                            ((TagDataClassPZ69Full)TextBoxUpperCom1.Tag).DCSBIOSBinding = dcsbiosBinding;
                            TextBoxUpperCom1.Text = dcsbiosBinding.Description;
                        }

                        if (dcsbiosBinding.RadioPanelPZ69Knob == RadioPanelPZ69KnobsEmulator.UpperCOM2)
                        {
                            ((TagDataClassPZ69Full)TextBoxUpperCom2.Tag).DCSBIOSBinding = dcsbiosBinding;
                            TextBoxUpperCom2.Text = dcsbiosBinding.Description;
                        }

                        if (dcsbiosBinding.RadioPanelPZ69Knob == RadioPanelPZ69KnobsEmulator.UpperNAV1)
                        {
                            ((TagDataClassPZ69Full)TextBoxUpperNav1.Tag).DCSBIOSBinding = dcsbiosBinding;
                            TextBoxUpperNav1.Text = dcsbiosBinding.Description;
                        }

                        if (dcsbiosBinding.RadioPanelPZ69Knob == RadioPanelPZ69KnobsEmulator.UpperNAV2)
                        {
                            ((TagDataClassPZ69Full)TextBoxUpperNav2.Tag).DCSBIOSBinding = dcsbiosBinding;
                            TextBoxUpperNav2.Text = dcsbiosBinding.Description;
                        }

                        if (dcsbiosBinding.RadioPanelPZ69Knob == RadioPanelPZ69KnobsEmulator.UpperADF)
                        {
                            ((TagDataClassPZ69Full)TextBoxUpperADF.Tag).DCSBIOSBinding = dcsbiosBinding;
                            TextBoxUpperADF.Text = dcsbiosBinding.Description;
                        }

                        if (dcsbiosBinding.RadioPanelPZ69Knob == RadioPanelPZ69KnobsEmulator.UpperDME)
                        {
                            ((TagDataClassPZ69Full)TextBoxUpperDME.Tag).DCSBIOSBinding = dcsbiosBinding;
                            TextBoxUpperDME.Text = dcsbiosBinding.Description;
                        }

                        if (dcsbiosBinding.RadioPanelPZ69Knob == RadioPanelPZ69KnobsEmulator.UpperXPDR)
                        {
                            ((TagDataClassPZ69Full)TextBoxUpperXPDR.Tag).DCSBIOSBinding = dcsbiosBinding;
                            TextBoxUpperXPDR.Text = dcsbiosBinding.Description;

                        }

                        if (dcsbiosBinding.RadioPanelPZ69Knob == RadioPanelPZ69KnobsEmulator.UpperLargeFreqWheelInc)
                        {
                            ((TagDataClassPZ69Full)TextBoxUpperLargePlus.Tag).DCSBIOSBinding = dcsbiosBinding;
                            TextBoxUpperLargePlus.Text = dcsbiosBinding.Description;
                        }

                        if (dcsbiosBinding.RadioPanelPZ69Knob == RadioPanelPZ69KnobsEmulator.UpperLargeFreqWheelDec)
                        {
                            ((TagDataClassPZ69Full)TextBoxUpperLargeMinus.Tag).DCSBIOSBinding = dcsbiosBinding;
                            TextBoxUpperLargeMinus.Text = dcsbiosBinding.Description;
                        }

                        if (dcsbiosBinding.RadioPanelPZ69Knob == RadioPanelPZ69KnobsEmulator.UpperSmallFreqWheelInc)
                        {
                            ((TagDataClassPZ69Full)TextBoxUpperSmallPlus.Tag).DCSBIOSBinding = dcsbiosBinding;
                            TextBoxUpperSmallPlus.Text = dcsbiosBinding.Description;
                        }

                        if (dcsbiosBinding.RadioPanelPZ69Knob == RadioPanelPZ69KnobsEmulator.UpperSmallFreqWheelDec)
                        {
                            ((TagDataClassPZ69Full)TextBoxUpperSmallMinus.Tag).DCSBIOSBinding = dcsbiosBinding;
                            TextBoxUpperSmallMinus.Text = dcsbiosBinding.Description;
                        }

                        if (dcsbiosBinding.RadioPanelPZ69Knob == RadioPanelPZ69KnobsEmulator.UpperFreqSwitch)
                        {
                            if (dcsbiosBinding.WhenTurnedOn)
                            {
                                ((TagDataClassPZ69Full)TextBoxUpperActStbyOn.Tag).DCSBIOSBinding = dcsbiosBinding;
                                TextBoxUpperActStbyOn.Text = dcsbiosBinding.Description;
                            }
                            else
                            {
                                ((TagDataClassPZ69Full)TextBoxUpperActStbyOff.Tag).DCSBIOSBinding = dcsbiosBinding;
                                TextBoxUpperActStbyOff.Text = dcsbiosBinding.Description;
                            }
                        }
                    }
                    else if(dcsbiosBinding.DialPosition == _radioPanelPZ69.PZ69LowerDialPosition)
                    {
                        if (dcsbiosBinding.RadioPanelPZ69Knob == RadioPanelPZ69KnobsEmulator.LowerCOM1)
                        {
                            ((TagDataClassPZ69Full)TextBoxLowerCom1.Tag).DCSBIOSBinding = dcsbiosBinding;
                            TextBoxLowerCom1.Text = dcsbiosBinding.Description;
                        }

                        if (dcsbiosBinding.RadioPanelPZ69Knob == RadioPanelPZ69KnobsEmulator.LowerCOM2)
                        {
                            ((TagDataClassPZ69Full)TextBoxLowerCom2.Tag).DCSBIOSBinding = dcsbiosBinding;
                            TextBoxLowerCom2.Text = dcsbiosBinding.Description;
                        }

                        if (dcsbiosBinding.RadioPanelPZ69Knob == RadioPanelPZ69KnobsEmulator.LowerNAV1)
                        {
                            ((TagDataClassPZ69Full)TextBoxLowerNav1.Tag).DCSBIOSBinding = dcsbiosBinding;
                            TextBoxLowerNav1.Text = dcsbiosBinding.Description;
                        }

                        if (dcsbiosBinding.RadioPanelPZ69Knob == RadioPanelPZ69KnobsEmulator.LowerNAV2)
                        {
                            ((TagDataClassPZ69Full)TextBoxLowerNav2.Tag).DCSBIOSBinding = dcsbiosBinding;
                            TextBoxLowerNav2.Text = dcsbiosBinding.Description;
                        }

                        if (dcsbiosBinding.RadioPanelPZ69Knob == RadioPanelPZ69KnobsEmulator.LowerADF)
                        {
                            ((TagDataClassPZ69Full)TextBoxLowerADF.Tag).DCSBIOSBinding = dcsbiosBinding;
                            TextBoxLowerADF.Text = dcsbiosBinding.Description;
                        }

                        if (dcsbiosBinding.RadioPanelPZ69Knob == RadioPanelPZ69KnobsEmulator.LowerDME)
                        {
                            ((TagDataClassPZ69Full)TextBoxLowerDME.Tag).DCSBIOSBinding = dcsbiosBinding;
                            TextBoxLowerDME.Text = dcsbiosBinding.Description;
                        }

                        if (dcsbiosBinding.RadioPanelPZ69Knob == RadioPanelPZ69KnobsEmulator.LowerXPDR)
                        {
                            ((TagDataClassPZ69Full)TextBoxLowerXPDR.Tag).DCSBIOSBinding = dcsbiosBinding;
                            TextBoxLowerXPDR.Text = dcsbiosBinding.Description;

                        }

                        if (dcsbiosBinding.RadioPanelPZ69Knob == RadioPanelPZ69KnobsEmulator.LowerLargeFreqWheelInc)
                        {
                            ((TagDataClassPZ69Full)TextBoxLowerLargePlus.Tag).DCSBIOSBinding = dcsbiosBinding;
                            TextBoxLowerLargePlus.Text = dcsbiosBinding.Description;
                        }

                        if (dcsbiosBinding.RadioPanelPZ69Knob == RadioPanelPZ69KnobsEmulator.LowerLargeFreqWheelDec)
                        {
                            ((TagDataClassPZ69Full)TextBoxLowerLargeMinus.Tag).DCSBIOSBinding = dcsbiosBinding;
                            TextBoxLowerLargeMinus.Text = dcsbiosBinding.Description;
                        }

                        if (dcsbiosBinding.RadioPanelPZ69Knob == RadioPanelPZ69KnobsEmulator.LowerSmallFreqWheelInc)
                        {
                            ((TagDataClassPZ69Full)TextBoxLowerSmallPlus.Tag).DCSBIOSBinding = dcsbiosBinding;
                            TextBoxLowerSmallPlus.Text = dcsbiosBinding.Description;
                        }

                        if (dcsbiosBinding.RadioPanelPZ69Knob == RadioPanelPZ69KnobsEmulator.LowerSmallFreqWheelDec)
                        {
                            ((TagDataClassPZ69Full)TextBoxLowerSmallMinus.Tag).DCSBIOSBinding = dcsbiosBinding;
                            TextBoxLowerSmallMinus.Text = dcsbiosBinding.Description;
                        }

                        if (dcsbiosBinding.RadioPanelPZ69Knob == RadioPanelPZ69KnobsEmulator.LowerFreqSwitch)
                        {
                            if (dcsbiosBinding.WhenTurnedOn)
                            {
                                ((TagDataClassPZ69Full)TextBoxLowerActStbyOn.Tag).DCSBIOSBinding = dcsbiosBinding;
                                TextBoxLowerActStbyOn.Text = dcsbiosBinding.Description;
                            }
                            else
                            {
                                ((TagDataClassPZ69Full)TextBoxLowerActStbyOff.Tag).DCSBIOSBinding = dcsbiosBinding;
                                TextBoxLowerActStbyOff.Text = dcsbiosBinding.Description;
                            }
                        }
                    }


                    foreach (var bipLinkPZ69 in _radioPanelPZ69.BipLinkHashSet)
                    {
                        if (bipLinkPZ69.RadioPanelPZ69Knob == RadioPanelPZ69KnobsEmulator.UpperCOM1)
                        {
                            ((TagDataClassPZ69Full)TextBoxUpperCom1.Tag).BIPLink = bipLinkPZ69; 
                        }

                        if (bipLinkPZ69.RadioPanelPZ69Knob == RadioPanelPZ69KnobsEmulator.UpperCOM2)
                        {
                            ((TagDataClassPZ69Full)TextBoxUpperCom2.Tag).BIPLink = bipLinkPZ69; 
                        }

                        if (bipLinkPZ69.RadioPanelPZ69Knob == RadioPanelPZ69KnobsEmulator.UpperNAV1)
                        {
                            ((TagDataClassPZ69Full)TextBoxUpperNav1.Tag).BIPLink = bipLinkPZ69; 
                        }

                        if (bipLinkPZ69.RadioPanelPZ69Knob == RadioPanelPZ69KnobsEmulator.UpperNAV2)
                        {
                            ((TagDataClassPZ69Full)TextBoxUpperNav2.Tag).BIPLink = bipLinkPZ69; 
                        }

                        if (bipLinkPZ69.RadioPanelPZ69Knob == RadioPanelPZ69KnobsEmulator.UpperADF)
                        {
                            ((TagDataClassPZ69Full)TextBoxUpperADF.Tag).BIPLink = bipLinkPZ69; 
                        }

                        if (bipLinkPZ69.RadioPanelPZ69Knob == RadioPanelPZ69KnobsEmulator.UpperDME)
                        {
                            ((TagDataClassPZ69Full)TextBoxUpperDME.Tag).BIPLink = bipLinkPZ69;
                        }

                        if (bipLinkPZ69.RadioPanelPZ69Knob == RadioPanelPZ69KnobsEmulator.UpperXPDR)
                        {
                            ((TagDataClassPZ69Full)TextBoxUpperXPDR.Tag).BIPLink = bipLinkPZ69; 

                        }

                        if (bipLinkPZ69.RadioPanelPZ69Knob == RadioPanelPZ69KnobsEmulator.UpperLargeFreqWheelInc)
                        {
                            ((TagDataClassPZ69Full)TextBoxUpperLargePlus.Tag).BIPLink = bipLinkPZ69; 
                        }

                        if (bipLinkPZ69.RadioPanelPZ69Knob == RadioPanelPZ69KnobsEmulator.UpperLargeFreqWheelDec)
                        {
                            ((TagDataClassPZ69Full)TextBoxUpperLargeMinus.Tag).BIPLink = bipLinkPZ69; 
                        }

                        if (bipLinkPZ69.RadioPanelPZ69Knob == RadioPanelPZ69KnobsEmulator.UpperSmallFreqWheelInc)
                        {
                            ((TagDataClassPZ69Full)TextBoxUpperSmallPlus.Tag).BIPLink = bipLinkPZ69; 
                        }

                        if (bipLinkPZ69.RadioPanelPZ69Knob == RadioPanelPZ69KnobsEmulator.UpperSmallFreqWheelDec)
                        {
                            ((TagDataClassPZ69Full)TextBoxUpperSmallMinus.Tag).BIPLink = bipLinkPZ69; 
                        }

                        if (bipLinkPZ69.RadioPanelPZ69Knob == RadioPanelPZ69KnobsEmulator.UpperFreqSwitch)
                        {
                            if (bipLinkPZ69.WhenTurnedOn)
                            {
                                ((TagDataClassPZ69Full)TextBoxUpperActStbyOn.Tag).BIPLink = bipLinkPZ69; 
                            }
                            else
                            {
                                ((TagDataClassPZ69Full)TextBoxUpperActStbyOff.Tag).BIPLink = bipLinkPZ69; 
                            }
                        }

                        if (bipLinkPZ69.RadioPanelPZ69Knob == RadioPanelPZ69KnobsEmulator.LowerCOM1)
                        {
                            if (bipLinkPZ69.WhenTurnedOn)
                            {
                                ((TagDataClassPZ69Full)TextBoxLowerCom1.Tag).BIPLink = bipLinkPZ69; 
                            }
                        }

                        if (bipLinkPZ69.RadioPanelPZ69Knob == RadioPanelPZ69KnobsEmulator.LowerCOM2)
                        {
                            if (bipLinkPZ69.WhenTurnedOn)
                            {
                                ((TagDataClassPZ69Full)TextBoxLowerCom2.Tag).BIPLink = bipLinkPZ69; 
                            }
                        }

                        if (bipLinkPZ69.RadioPanelPZ69Knob == RadioPanelPZ69KnobsEmulator.LowerNAV1)
                        {
                            if (bipLinkPZ69.WhenTurnedOn)
                            {
                                ((TagDataClassPZ69Full)TextBoxLowerNav1.Tag).BIPLink = bipLinkPZ69; 
                            }
                        }

                        if (bipLinkPZ69.RadioPanelPZ69Knob == RadioPanelPZ69KnobsEmulator.LowerNAV2)
                        {
                            if (bipLinkPZ69.WhenTurnedOn)
                            {
                                ((TagDataClassPZ69Full)TextBoxLowerNav2.Tag).BIPLink = bipLinkPZ69; 
                            }
                        }

                        if (bipLinkPZ69.RadioPanelPZ69Knob == RadioPanelPZ69KnobsEmulator.LowerADF)
                        {
                            if (bipLinkPZ69.WhenTurnedOn)
                            {
                                ((TagDataClassPZ69Full)TextBoxLowerADF.Tag).BIPLink = bipLinkPZ69; 
                            }
                        }

                        if (bipLinkPZ69.RadioPanelPZ69Knob == RadioPanelPZ69KnobsEmulator.LowerDME)
                        {
                            if (bipLinkPZ69.WhenTurnedOn)
                            {
                                ((TagDataClassPZ69Full)TextBoxLowerDME.Tag).BIPLink = bipLinkPZ69; 
                            }
                        }

                        if (bipLinkPZ69.RadioPanelPZ69Knob == RadioPanelPZ69KnobsEmulator.LowerXPDR)
                        {
                            if (bipLinkPZ69.WhenTurnedOn)
                            {
                                ((TagDataClassPZ69Full)TextBoxLowerXPDR.Tag).BIPLink = bipLinkPZ69; 
                            }
                        }

                        if (bipLinkPZ69.RadioPanelPZ69Knob == RadioPanelPZ69KnobsEmulator.LowerLargeFreqWheelInc)
                        {
                            ((TagDataClassPZ69Full)TextBoxLowerLargePlus.Tag).BIPLink = bipLinkPZ69; 
                        }

                        if (bipLinkPZ69.RadioPanelPZ69Knob == RadioPanelPZ69KnobsEmulator.LowerLargeFreqWheelDec)
                        {
                            ((TagDataClassPZ69Full)TextBoxLowerLargeMinus.Tag).BIPLink = bipLinkPZ69; 
                        }

                        if (bipLinkPZ69.RadioPanelPZ69Knob == RadioPanelPZ69KnobsEmulator.LowerSmallFreqWheelInc)
                        {
                            ((TagDataClassPZ69Full)TextBoxLowerSmallPlus.Tag).BIPLink = bipLinkPZ69; 
                        }

                        if (bipLinkPZ69.RadioPanelPZ69Knob == RadioPanelPZ69KnobsEmulator.LowerSmallFreqWheelDec)
                        {
                            ((TagDataClassPZ69Full)TextBoxLowerSmallMinus.Tag).BIPLink = bipLinkPZ69; 
                        }

                        if (bipLinkPZ69.RadioPanelPZ69Knob == RadioPanelPZ69KnobsEmulator.LowerFreqSwitch)
                        {
                            if (bipLinkPZ69.WhenTurnedOn)
                            {
                                ((TagDataClassPZ69Full)TextBoxLowerActStbyOn.Tag).BIPLink = bipLinkPZ69; 
                            }
                            else
                            {
                                ((TagDataClassPZ69Full)TextBoxLowerActStbyOff.Tag).BIPLink = bipLinkPZ69; 
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
                                        if (key.IsOn)
                                        {
                                            ClearAll(false);
                                            ShowGraphicConfiguration();
                                            LabelDialPosUpper.Content = UpperText + "COM1";
                                        }
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
                                        if (key.IsOn)
                                        {
                                            ClearAll(false);
                                            ShowGraphicConfiguration();
                                            LabelDialPosUpper.Content = UpperText + "COM2";
                                        }
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
                                        if (key.IsOn)
                                        {
                                            ClearAll(false);
                                            ShowGraphicConfiguration();
                                            LabelDialPosUpper.Content = UpperText + "NAV1";
                                        }
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
                                        if (key.IsOn)
                                        {
                                            ClearAll(false);
                                            ShowGraphicConfiguration();
                                            LabelDialPosUpper.Content = UpperText + "NAV2";
                                        }
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
                                        if (key.IsOn)
                                        {
                                            ClearAll(false);
                                            ShowGraphicConfiguration();
                                            LabelDialPosUpper.Content = UpperText + "ADF";
                                        }
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
                                        if (key.IsOn)
                                        {
                                            ClearAll(false);
                                            ShowGraphicConfiguration();
                                            LabelDialPosUpper.Content = UpperText + "DME";
                                        }
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
                                        if (key.IsOn)
                                        {
                                            ClearAll(false);
                                            ShowGraphicConfiguration();
                                            LabelDialPosUpper.Content = UpperText + "XPDR";
                                        }
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
                                        if (key.IsOn)
                                        {
                                            ClearAll(false);
                                            ShowGraphicConfiguration();
                                            LabelDialPosLower.Content = LowerText + "COM1";
                                        }
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
                                        if (key.IsOn)
                                        {
                                            ClearAll(false);
                                            ShowGraphicConfiguration();
                                            LabelDialPosLower.Content = LowerText + "COM2";
                                        }
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
                                        if (key.IsOn)
                                        {
                                            ClearAll(false);
                                            ShowGraphicConfiguration();
                                            LabelDialPosLower.Content = LowerText + "NAV1";
                                        }
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
                                        if (key.IsOn)
                                        {
                                            ClearAll(false);
                                            ShowGraphicConfiguration();
                                            LabelDialPosLower.Content = LowerText + "NAV2";
                                        }
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
                                        if (key.IsOn)
                                        {
                                            ClearAll(false);
                                            ShowGraphicConfiguration();
                                            LabelDialPosLower.Content = LowerText + "ADF";
                                        }
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
                                        if (key.IsOn)
                                        {
                                            ClearAll(false);
                                            ShowGraphicConfiguration();
                                            LabelDialPosLower.Content = LowerText + "DME";
                                        }
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
                                        if (key.IsOn)
                                        {
                                            ClearAll(false);
                                            ShowGraphicConfiguration();
                                            LabelDialPosLower.Content = LowerText + "XPDR";
                                        }
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
                Common.ShowErrorMessageBox(9965005, ex);
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
                            dcsBiosOutputFormulaWindow = new DCSBiosOutputFormulaWindow(_globalHandler.GetAirframe(), description, dcsbiosBindingLCDPZ69.DCSBIOSOutputFormulaObject);
                            break;
                        }
                        dcsBiosOutputFormulaWindow = new DCSBiosOutputFormulaWindow(_globalHandler.GetAirframe(), description, dcsbiosBindingLCDPZ69.DCSBIOSOutputObject);
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
                        UpdateDCSBIOSBindingLCD(true, false, null, dcsBiosOutputFormula, pz69LCDPosition);
                    }
                    else if (dcsBiosOutputFormulaWindow.UseSingleDCSBiosControl())
                    {
                        var dcsBiosOutput = dcsBiosOutputFormulaWindow.DCSBiosOutput;
                        UpdateDCSBIOSBindingLCD(false, false, dcsBiosOutput, null, pz69LCDPosition);
                    }
                    else
                    {
                        //Delete config
                        UpdateDCSBIOSBindingLCD(false, true, null, null, pz69LCDPosition);
                    }
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(49942044, ex);
            }
        }


        private void UpdateDCSBIOSBindingLCD(bool useFormula, bool deleteConfig, DCSBIOSOutput dcsbiosOutput, DCSBIOSOutputFormula dcsbiosOutputFormula, PZ69LCDPosition pz69LCDPosition)
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
                        _radioPanelPZ69.AddOrUpdateLCDBinding(dcsbiosOutput, PZ69LCDPosition.UPPER_ACTIVE_LEFT);
                    }
                    if (pz69LCDPosition == PZ69LCDPosition.LOWER_ACTIVE_LEFT)
                    {
                        DotBottomLeftLcd.Visibility = dcsbiosOutput == null ? Visibility.Collapsed : Visibility.Visible;
                        _radioPanelPZ69.AddOrUpdateLCDBinding(dcsbiosOutput, PZ69LCDPosition.LOWER_ACTIVE_LEFT);
                    }
                    if (pz69LCDPosition == PZ69LCDPosition.UPPER_STBY_RIGHT)
                    {
                        DotTopRightLcd.Visibility = dcsbiosOutput == null ? Visibility.Collapsed : Visibility.Visible;
                        _radioPanelPZ69.AddOrUpdateLCDBinding(dcsbiosOutput, PZ69LCDPosition.UPPER_STBY_RIGHT);
                    }
                    if (pz69LCDPosition == PZ69LCDPosition.LOWER_STBY_RIGHT)
                    {
                        DotBottomRightLcd.Visibility = dcsbiosOutput == null ? Visibility.Collapsed : Visibility.Visible;
                        _radioPanelPZ69.AddOrUpdateLCDBinding(dcsbiosOutput, PZ69LCDPosition.LOWER_STBY_RIGHT);
                    }
                }

                if (useFormula)
                {
                    if (pz69LCDPosition == PZ69LCDPosition.UPPER_ACTIVE_LEFT)
                    {
                        DotTopLeftLcd.Visibility = dcsbiosOutputFormula == null ? Visibility.Collapsed : Visibility.Visible;
                        _radioPanelPZ69.AddOrUpdateLCDBinding(dcsbiosOutputFormula, PZ69LCDPosition.UPPER_ACTIVE_LEFT);
                    }
                    if (pz69LCDPosition == PZ69LCDPosition.LOWER_ACTIVE_LEFT)
                    {
                        DotBottomLeftLcd.Visibility = dcsbiosOutputFormula == null ? Visibility.Collapsed : Visibility.Visible;
                        _radioPanelPZ69.AddOrUpdateLCDBinding(dcsbiosOutputFormula, PZ69LCDPosition.LOWER_ACTIVE_LEFT);
                    }
                    if (pz69LCDPosition == PZ69LCDPosition.UPPER_STBY_RIGHT)
                    {
                        DotTopRightLcd.Visibility = dcsbiosOutputFormula == null ? Visibility.Collapsed : Visibility.Visible;
                        _radioPanelPZ69.AddOrUpdateLCDBinding(dcsbiosOutputFormula, PZ69LCDPosition.UPPER_STBY_RIGHT);
                    }
                    if (pz69LCDPosition == PZ69LCDPosition.LOWER_STBY_RIGHT)
                    {
                        DotBottomRightLcd.Visibility = dcsbiosOutputFormula == null ? Visibility.Collapsed : Visibility.Visible;
                        _radioPanelPZ69.AddOrUpdateLCDBinding(dcsbiosOutputFormula, PZ69LCDPosition.LOWER_STBY_RIGHT);
                    }
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(34501287, ex);
            }
        }

        private void UpdateDCSBIOSBinding(TextBox textBox)
        {
            try
            {
                var key = GetPZ69Key(textBox);
                _radioPanelPZ69.AddOrUpdateDCSBIOSBinding(key.RadioPanelPZ69Key, ((TagDataClassPZ69Full)textBox.Tag).DCSBIOSBinding.DCSBIOSInputs, textBox.Text, key.ButtonState);
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(345012, ex);
            }
        }

        private RadioPanelPZ69KeyOnOff GetPZ69Key(TextBox textBox)
        {
            try
            {
                if (textBox.Equals(TextBoxUpperCom1))
                {
                    return new RadioPanelPZ69KeyOnOff(RadioPanelPZ69KnobsEmulator.UpperCOM1, true);
                }
                if (textBox.Equals(TextBoxUpperCom2))
                {
                    return new RadioPanelPZ69KeyOnOff(RadioPanelPZ69KnobsEmulator.UpperCOM2, true);
                }
                if (textBox.Equals(TextBoxUpperNav1))
                {
                    return new RadioPanelPZ69KeyOnOff(RadioPanelPZ69KnobsEmulator.UpperNAV1, true);
                }
                if (textBox.Equals(TextBoxUpperNav2))
                {
                    return new RadioPanelPZ69KeyOnOff(RadioPanelPZ69KnobsEmulator.UpperNAV2, true);
                }
                if (textBox.Equals(TextBoxUpperADF))
                {
                    return new RadioPanelPZ69KeyOnOff(RadioPanelPZ69KnobsEmulator.UpperADF, true);
                }
                if (textBox.Equals(TextBoxUpperDME))
                {
                    return new RadioPanelPZ69KeyOnOff(RadioPanelPZ69KnobsEmulator.UpperDME, true);
                }
                if (textBox.Equals(TextBoxUpperXPDR))
                {
                    return new RadioPanelPZ69KeyOnOff(RadioPanelPZ69KnobsEmulator.UpperXPDR, true);
                }
                if (textBox.Equals(TextBoxUpperLargePlus))
                {
                    return new RadioPanelPZ69KeyOnOff(RadioPanelPZ69KnobsEmulator.UpperLargeFreqWheelInc, true);
                }
                if (textBox.Equals(TextBoxUpperLargeMinus))
                {
                    return new RadioPanelPZ69KeyOnOff(RadioPanelPZ69KnobsEmulator.UpperLargeFreqWheelDec, true);
                }
                if (textBox.Equals(TextBoxUpperSmallPlus))
                {
                    return new RadioPanelPZ69KeyOnOff(RadioPanelPZ69KnobsEmulator.UpperSmallFreqWheelInc, true);
                }
                if (textBox.Equals(TextBoxUpperSmallMinus))
                {
                    return new RadioPanelPZ69KeyOnOff(RadioPanelPZ69KnobsEmulator.UpperSmallFreqWheelDec, true);
                }
                if (textBox.Equals(TextBoxUpperActStbyOn))
                {
                    return new RadioPanelPZ69KeyOnOff(RadioPanelPZ69KnobsEmulator.UpperFreqSwitch, true);
                }
                if (textBox.Equals(TextBoxUpperActStbyOff))
                {
                    return new RadioPanelPZ69KeyOnOff(RadioPanelPZ69KnobsEmulator.UpperFreqSwitch, false);
                }
                if (textBox.Equals(TextBoxLowerCom1))
                {
                    return new RadioPanelPZ69KeyOnOff(RadioPanelPZ69KnobsEmulator.LowerCOM1, true);
                }
                if (textBox.Equals(TextBoxLowerCom2))
                {
                    return new RadioPanelPZ69KeyOnOff(RadioPanelPZ69KnobsEmulator.LowerCOM2, true);
                }
                if (textBox.Equals(TextBoxLowerNav1))
                {
                    return new RadioPanelPZ69KeyOnOff(RadioPanelPZ69KnobsEmulator.LowerNAV1, true);
                }
                if (textBox.Equals(TextBoxLowerNav2))
                {
                    return new RadioPanelPZ69KeyOnOff(RadioPanelPZ69KnobsEmulator.LowerNAV2, true);
                }
                if (textBox.Equals(TextBoxLowerADF))
                {
                    return new RadioPanelPZ69KeyOnOff(RadioPanelPZ69KnobsEmulator.LowerADF, true);
                }
                if (textBox.Equals(TextBoxLowerDME))
                {
                    return new RadioPanelPZ69KeyOnOff(RadioPanelPZ69KnobsEmulator.LowerDME, true);
                }
                if (textBox.Equals(TextBoxLowerXPDR))
                {
                    return new RadioPanelPZ69KeyOnOff(RadioPanelPZ69KnobsEmulator.LowerXPDR, true);
                }
                if (textBox.Equals(TextBoxLowerLargePlus))
                {
                    return new RadioPanelPZ69KeyOnOff(RadioPanelPZ69KnobsEmulator.LowerLargeFreqWheelInc, true);
                }
                if (textBox.Equals(TextBoxLowerLargeMinus))
                {
                    return new RadioPanelPZ69KeyOnOff(RadioPanelPZ69KnobsEmulator.LowerLargeFreqWheelDec, true);
                }
                if (textBox.Equals(TextBoxLowerSmallPlus))
                {
                    return new RadioPanelPZ69KeyOnOff(RadioPanelPZ69KnobsEmulator.LowerSmallFreqWheelInc, true);
                }
                if (textBox.Equals(TextBoxLowerSmallMinus))
                {
                    return new RadioPanelPZ69KeyOnOff(RadioPanelPZ69KnobsEmulator.LowerSmallFreqWheelDec, true);
                }
                if (textBox.Equals(TextBoxLowerActStbyOn))
                {
                    return new RadioPanelPZ69KeyOnOff(RadioPanelPZ69KnobsEmulator.LowerFreqSwitch, true);
                }
                if (textBox.Equals(TextBoxLowerActStbyOff))
                {
                    return new RadioPanelPZ69KeyOnOff(RadioPanelPZ69KnobsEmulator.LowerFreqSwitch, false);
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(345012, ex);
            }
            throw new Exception("Failed to find Radiopanel knob for TextBox " + textBox.Name);
        }

        private TextBox GetTextBox(RadioPanelPZ69KnobsEmulator knob, bool whenTurnedOn)
        {
            try
            {
                if (knob == RadioPanelPZ69KnobsEmulator.UpperCOM1 && whenTurnedOn)
                {
                    return TextBoxUpperCom1;
                }
                if (knob == RadioPanelPZ69KnobsEmulator.UpperCOM2 && whenTurnedOn)
                {
                    return TextBoxUpperCom2;
                }
                if (knob == RadioPanelPZ69KnobsEmulator.UpperNAV1 && whenTurnedOn)
                {
                    return TextBoxUpperNav1;
                }
                if (knob == RadioPanelPZ69KnobsEmulator.UpperNAV2 && whenTurnedOn)
                {
                    return TextBoxUpperNav2;
                }
                if (knob == RadioPanelPZ69KnobsEmulator.UpperADF && whenTurnedOn)
                {
                    return TextBoxUpperADF;
                }
                if (knob == RadioPanelPZ69KnobsEmulator.UpperDME && whenTurnedOn)
                {
                    return TextBoxUpperDME;
                }
                if (knob == RadioPanelPZ69KnobsEmulator.UpperXPDR && whenTurnedOn)
                {
                    return TextBoxUpperXPDR;
                }
                if (knob == RadioPanelPZ69KnobsEmulator.UpperLargeFreqWheelInc && whenTurnedOn)
                {
                    return TextBoxUpperLargePlus;
                }
                if (knob == RadioPanelPZ69KnobsEmulator.UpperLargeFreqWheelDec && whenTurnedOn)
                {
                    return TextBoxUpperLargeMinus;
                }
                if (knob == RadioPanelPZ69KnobsEmulator.UpperSmallFreqWheelInc && whenTurnedOn)
                {
                    return TextBoxUpperSmallPlus;
                }
                if (knob == RadioPanelPZ69KnobsEmulator.UpperSmallFreqWheelDec && whenTurnedOn)
                {
                    return TextBoxUpperSmallMinus;
                }
                if (knob == RadioPanelPZ69KnobsEmulator.UpperFreqSwitch && whenTurnedOn)
                {
                    return TextBoxUpperActStbyOn;
                }
                if (knob == RadioPanelPZ69KnobsEmulator.UpperFreqSwitch && !whenTurnedOn)
                {
                    return TextBoxUpperActStbyOff;
                }
                if (knob == RadioPanelPZ69KnobsEmulator.LowerCOM1 && whenTurnedOn)
                {
                    return TextBoxLowerCom1;
                }
                if (knob == RadioPanelPZ69KnobsEmulator.LowerCOM2 && whenTurnedOn)
                {
                    return TextBoxLowerCom2;
                }
                if (knob == RadioPanelPZ69KnobsEmulator.LowerNAV1 && whenTurnedOn)
                {
                    return TextBoxLowerNav1;
                }
                if (knob == RadioPanelPZ69KnobsEmulator.LowerNAV2 && whenTurnedOn)
                {
                    return TextBoxLowerNav2;
                }
                if (knob == RadioPanelPZ69KnobsEmulator.LowerADF && whenTurnedOn)
                {
                    return TextBoxLowerADF;
                }
                if (knob == RadioPanelPZ69KnobsEmulator.LowerDME && whenTurnedOn)
                {
                    return TextBoxLowerDME;
                }
                if (knob == RadioPanelPZ69KnobsEmulator.LowerXPDR && whenTurnedOn)
                {
                    return TextBoxLowerXPDR;
                }
                if (knob == RadioPanelPZ69KnobsEmulator.LowerLargeFreqWheelInc && whenTurnedOn)
                {
                    return TextBoxLowerLargePlus;
                }
                if (knob == RadioPanelPZ69KnobsEmulator.LowerLargeFreqWheelDec && whenTurnedOn)
                {
                    return TextBoxLowerLargeMinus;
                }
                if (knob == RadioPanelPZ69KnobsEmulator.LowerSmallFreqWheelInc && whenTurnedOn)
                {
                    return TextBoxLowerSmallPlus;
                }
                if (knob == RadioPanelPZ69KnobsEmulator.LowerSmallFreqWheelDec && whenTurnedOn)
                {
                    return TextBoxLowerSmallMinus;
                }
                if (knob == RadioPanelPZ69KnobsEmulator.LowerFreqSwitch && whenTurnedOn)
                {
                    return TextBoxLowerActStbyOn;
                }
                if (knob == RadioPanelPZ69KnobsEmulator.LowerFreqSwitch && !whenTurnedOn)
                {
                    return TextBoxLowerActStbyOff;
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(345012, ex);
            }
            throw new Exception("Failed to find TextBox for Radiopanel knob " + knob);
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
                if (((TagDataClassPZ69Full)textBox.Tag).ContainsDCSBIOS())
                {
                    dcsBIOSControlsConfigsWindow = new DCSBIOSControlsConfigsWindow(_globalHandler.GetAirframe(), textBox.Name.Replace("TextBox", ""), ((TagDataClassPZ69Full)textBox.Tag).DCSBIOSBinding.DCSBIOSInputs, textBox.Text);
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
                    ((TagDataClassPZ69Full)textBox.Tag).Consume(dcsBiosInputs);
                    UpdateDCSBIOSBinding(textBox);
                }
                TextBoxLogPZ69.Focus();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(442044, ex);
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
    }
}
