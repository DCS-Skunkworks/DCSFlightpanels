using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ClassLibraryCommon;
using DCS_BIOS;
using NonVisuals;

namespace DCSFlightpanels
{
    /// <summary>
    /// Interaction logic for HESPUserControl.xaml
    /// </summary>
    public partial class HESPUserControl : ISaitekPanelListener, IProfileHandlerListener, ISaitekUserControl
    {

        private readonly HESP _hesp;
        private TabItem _parentTabItem;
        private string _parentTabItemHeader;
        private IGlobalHandler _globalHandler;
        private bool _enableDCSBIOS;

        public HESPUserControl(HIDSkeleton hidSkeleton, TabItem parentTabItem, IGlobalHandler globalHandler, bool enableDCSBIOS)
        {
            InitializeComponent();
            _parentTabItem = parentTabItem;
            _parentTabItemHeader = _parentTabItem.Header.ToString();
            _hesp = new HESP(hidSkeleton);

            _hesp.Attach((ISaitekPanelListener)this);
            globalHandler.Attach(_hesp);
            _globalHandler = globalHandler;
            _enableDCSBIOS = enableDCSBIOS;
        }

        private void HESPUserControl_OnLoaded(object sender, RoutedEventArgs e)
        {
            SetContextMenuClickHandlers();
        }

        public SaitekPanel GetSaitekPanel()
        {
            return _hesp;
        }

        public string GetName()
        {
            return GetType().Name;
        }

        public void SelectedAirframe(DCSAirframe dcsAirframe)
        {
            try
            {
                foreach (var image in Common.FindVisualChildren<Image>(this))
                {
                    if (image.Name.StartsWith("ImageHESPLED") && dcsAirframe == DCSAirframe.KEYEMULATOR)
                    {
                        image.ContextMenu = null;
                    }
                    else
                        if (image.Name.StartsWith("ImageHESPLED") && image.ContextMenu == null && dcsAirframe != DCSAirframe.KEYEMULATOR)
                    {
                        image.ContextMenu = (ContextMenu)Resources["HESPLEDContextMenu"];
                        image.ContextMenu.Tag = image.Name;
                    }
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(471373, ex);
            }
        }

        public void UpdatesHasBeenMissed(string uniqueId, SaitekPanelsEnum saitekPanelsEnum, int count)
        {
            try
            {
                //
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(471076, ex);
            }
        }

        public void SwitchesChanged(string uniqueId, SaitekPanelsEnum saitekPanelsEnum, HashSet<object> hashSet)
        {
            try
            {
                if (saitekPanelsEnum == SaitekPanelsEnum.HESP && uniqueId.Equals(_hesp.InstanceId))
                {
                    NotifySwitchChanges(hashSet);
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(1066, ex);
            }
        }

        public void PanelSettingsReadFromFile(List<string> settings)
        {
            try
            {
                ShowGraphicConfiguration();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(1067, ex);
            }
        }

        public void SettingsCleared(string uniqueId, SaitekPanelsEnum saitekPanelsEnum)
        {
            try
            {
                ClearAll(false);
                ShowGraphicConfiguration();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(1068, ex);
            }
        }

        public void LedLightChanged(string uniqueId, SaitekPanelLEDPosition saitekPanelLEDPosition, PanelLEDColor panelLEDColor)
        {
        }


        public void PanelDataAvailable(string stringData)
        {
            try
            {
                //todo
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(1085, ex);
            }
        }

        public void DeviceAttached(string uniqueId, SaitekPanelsEnum saitekPanelsEnum)
        {
            try
            {
                if (saitekPanelsEnum == SaitekPanelsEnum.HESP && uniqueId.Equals(_hesp.InstanceId))
                {
                    //Dispatcher.BeginInvoke((Action)(() => _parentTabItem.Header = _parentTabItemHeader + " (connected)"));
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(2008, ex);
            }
        }

        public void DeviceDetached(string uniqueId, SaitekPanelsEnum saitekPanelsEnum)
        {
            try
            {
                if (saitekPanelsEnum == SaitekPanelsEnum.HESP && uniqueId.Equals(_hesp.InstanceId))
                {
                    //Dispatcher.BeginInvoke((Action)(() => _parentTabItem.Header = _parentTabItemHeader + " (disconnected)"));
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(2031, ex);
            }
        }

        public void SettingsApplied(string uniqueId, SaitekPanelsEnum saitekPanelsEnum)
        {
            try
            {
                if (uniqueId.Equals(_hesp.InstanceId) && saitekPanelsEnum == SaitekPanelsEnum.HESP)
                {
                    Dispatcher.BeginInvoke((Action)(ShowGraphicConfiguration));
                    Dispatcher.BeginInvoke((Action)(() => TextBoxLogHESP.Text = ""));
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
                TextBoxLogHESP.Focus();
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
                        UpdateKeyBindingProfileSequencedKeyStrokesHESP(textBox);
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
                if (textBox.Tag is List<DCSBIOSInput>)
                {
                    dcsBIOSControlsConfigsWindow = new DCSBIOSControlsConfigsWindow(_globalHandler.GetAirframe(), textBox.Name.Replace("TextBox", ""), (List<DCSBIOSInput>)textBox.Tag, textBox.Text);
                }
                else
                {
                    dcsBIOSControlsConfigsWindow = new DCSBIOSControlsConfigsWindow(_globalHandler.GetAirframe(), textBox.Name.Replace("TextBox", ""), null);
                }
                dcsBIOSControlsConfigsWindow.ShowDialog();
                if (dcsBIOSControlsConfigsWindow.DialogResult.HasValue && dcsBIOSControlsConfigsWindow.DialogResult == true && dcsBIOSControlsConfigsWindow.DCSBIOSInputs.Count > 0)
                {
                    var dcsBiosInputs = dcsBIOSControlsConfigsWindow.DCSBIOSInputs;
                    var text = string.IsNullOrWhiteSpace(dcsBIOSControlsConfigsWindow.Description) ? "DCS-BIOS" : dcsBIOSControlsConfigsWindow.Description;
                    //1 appropriate text to textbox
                    //2 update bindings
                    textBox.Text = text;
                    textBox.Tag = dcsBiosInputs;
                    textBox.ToolTip = textBox.Text;
                    UpdateDCSBIOSBinding(textBox);
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(442044, ex);
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
                _hesp.ClearSettings();
            }
        }

        private void SetContextMenuClickHandlers()
        {
            foreach (var textBox in Common.FindVisualChildren<TextBox>(this))
            {
                if (textBox != TextBoxLogHESP)
                {
                    var contectMenu = (ContextMenu)Resources["TextBoxContextMenuHESP"];
                    if (!_enableDCSBIOS)
                    {
                        MenuItem dcsBIOSMenuItem = null;
                        foreach (var item in contectMenu.Items)
                        {
                            if (((MenuItem)item).Name == "contextMenuItemEditDCSBIOS")
                            {
                                dcsBIOSMenuItem = (MenuItem)item;
                                break;
                            }
                        }
                        if (dcsBIOSMenuItem != null)
                        {
                            contectMenu.Items.Remove(dcsBIOSMenuItem);
                        }
                    }
                    textBox.ContextMenu = contectMenu;
                    textBox.ContextMenuOpening += TextBoxContextMenuOpening;
                }
            }
            if (_enableDCSBIOS)
            {
                foreach (var image in Common.FindVisualChildren<Image>(this))
                {
                    if (image.Name.StartsWith("ImageHESPLED"))
                    {
                        image.ContextMenu = (ContextMenu)Resources["HESPLEDContextMenu"];
                        if (image.ContextMenu != null) image.ContextMenu.Tag = image.Name;
                    }
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
                        else if (!_hesp.KeyboardEmulationOnly && item.Name.Contains("EditDCSBIOS"))
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
                if (textBox != TextBoxLogHESP && textBox.IsFocused && textBox.Background == Brushes.Yellow)
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
                        _hesp.ClearAllBindings(GetHESPKey(textBox));
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
                if (MessageBox.Show("Clear all settings for the HESP?", "Confirm", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
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
                UpdateKeyBindingProfileSequencedKeyStrokesHESP(textBox);
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
                Dispatcher.BeginInvoke((Action)(() => TextBoxLogHESP.Focus()));
                foreach (var hespKey in switches)
                {
                    var key = (HESPKey)hespKey;

                    if (_hesp.ForwardKeyPresses)
                    {
                        if (!string.IsNullOrEmpty(_hesp.GetKeyPressForLoggingPurposes(key)))
                        {
                            Dispatcher.BeginInvoke(
                                (Action)
                                (() =>
                                 TextBoxLogHESP.Text =
                                 TextBoxLogHESP.Text.Insert(0, _hesp.GetKeyPressForLoggingPurposes(key) + "\n")));
                        }
                    }
                    else
                    {
                        Dispatcher.BeginInvoke(
                            (Action)
                            (() =>
                             TextBoxLogHESP.Text =
                             TextBoxLogHESP.Text = TextBoxLogHESP.Text.Insert(0, "No action taken, virtual key press disabled.\n")));
                    }
                }
                SetGraphicsState(switches);
            }
            catch (Exception ex)
            {
                Dispatcher.BeginInvoke(
                    (Action)
                    (() =>
                     TextBoxLogHESP.Text = TextBoxLogHESP.Text.Insert(0, "0x16" + ex.Message + ".\n")));
                Common.ShowErrorMessageBox(3009, ex);
            }
        }

        private void SetGraphicsState(HashSet<object> switches)
        {
            try
            {
                foreach (var hespKeyObject in switches)
                {
                    var hespKey = (HESPKey)hespKeyObject;
                    switch (hespKey.Key)
                    {
                        case HESPKeys.BUTTON1:
                            {
                                var key = hespKey;
                                Dispatcher.BeginInvoke(
                                    (Action)delegate
                                    {
                                        ImageButton1.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case HESPKeys.BUTTON2:
                        {
                            var key = hespKey;
                            Dispatcher.BeginInvoke(
                                (Action)delegate
                                {
                                    ImageButton2.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                });
                            break;
                        }
                        case HESPKeys.BUTTON3:
                        {
                            var key = hespKey;
                            Dispatcher.BeginInvoke(
                                (Action)delegate
                                {
                                    ImageButton3.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                });
                            break;
                        }
                        case HESPKeys.BUTTON4:
                        {
                            var key = hespKey;
                            Dispatcher.BeginInvoke(
                                (Action)delegate
                                {
                                    ImageButton4.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                });
                            break;
                        }
                        case HESPKeys.BUTTON5:
                        {
                            var key = hespKey;
                            Dispatcher.BeginInvoke(
                                (Action)delegate
                                {
                                    ImageButton5.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                });
                            break;
                        }
                        case HESPKeys.BUTTON6:
                        {
                            var key = hespKey;
                            Dispatcher.BeginInvoke(
                                (Action)delegate
                                {
                                    ImageButton6.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                });
                            break;
                        }
                        case HESPKeys.BUTTON7:
                        {
                            var key = hespKey;
                            Dispatcher.BeginInvoke(
                                (Action)delegate
                                {
                                    ImageButton7.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                });
                            break;
                        }
                        case HESPKeys.BUTTON8:
                        {
                            var key = hespKey;
                            Dispatcher.BeginInvoke(
                                (Action)delegate
                                {
                                    ImageButton8.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                });
                            break;
                        }
                        case HESPKeys.BUTTON9:
                        {
                            var key = hespKey;
                            Dispatcher.BeginInvoke(
                                (Action)delegate
                                {
                                    ImageButton9.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                });
                            break;
                        }
                        case HESPKeys.BUTTON10:
                        {
                            var key = hespKey;
                            Dispatcher.BeginInvoke(
                                (Action)delegate
                                {
                                    ImageButton10.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                });
                            break;
                        }
                        case HESPKeys.BUTTON11:
                        {
                            var key = hespKey;
                            Dispatcher.BeginInvoke(
                                (Action)delegate
                                {
                                    ImageButton11.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                });
                            break;
                        }
                        case HESPKeys.BUTTON12:
                        {
                            var key = hespKey;
                            Dispatcher.BeginInvoke(
                                (Action)delegate
                                {
                                    ImageButton12.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                });
                            break;
                        }
                        case HESPKeys.BUTTON13:
                        {
                            var key = hespKey;
                            Dispatcher.BeginInvoke(
                                (Action)delegate
                                {
                                    ImageButton13.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                });
                            break;
                        }
                        case HESPKeys.BUTTON14:
                        {
                            var key = hespKey;
                            Dispatcher.BeginInvoke(
                                (Action)delegate
                                {
                                    ImageButton14.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                });
                            break;
                        }
                        case HESPKeys.BUTTON15:
                        {
                            var key = hespKey;
                            Dispatcher.BeginInvoke(
                                (Action)delegate
                                {
                                    ImageButton15.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                });
                            break;
                        }
                        case HESPKeys.BUTTON16:
                        {
                            var key = hespKey;
                            Dispatcher.BeginInvoke(
                                (Action)delegate
                                {
                                    ImageButton16.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                });
                            break;
                        }
                        case HESPKeys.BUTTON17:
                        {
                            var key = hespKey;
                            Dispatcher.BeginInvoke(
                                (Action)delegate
                                {
                                    ImageButton17.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                });
                            break;
                        }
                        case HESPKeys.BUTTON18:
                        {
                            var key = hespKey;
                            Dispatcher.BeginInvoke(
                                (Action)delegate
                                {
                                    ImageButton18.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                });
                            break;
                        }
                        case HESPKeys.BUTTON19:
                        {
                            var key = hespKey;
                            Dispatcher.BeginInvoke(
                                (Action)delegate
                                {
                                    ImageButton19.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                });
                            break;
                        }
                        case HESPKeys.BUTTON20:
                        {
                            var key = hespKey;
                            Dispatcher.BeginInvoke(
                                (Action)delegate
                                {
                                    ImageButton20.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                });
                            break;
                        }
                        case HESPKeys.BUTTON21:
                        {
                            var key = hespKey;
                            Dispatcher.BeginInvoke(
                                (Action)delegate
                                {
                                    ImageButton21.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                });
                            break;
                        }
                        case HESPKeys.BUTTON22:
                        {
                            var key = hespKey;
                            Dispatcher.BeginInvoke(
                                (Action)delegate
                                {
                                    ImageButton22.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                });
                            break;
                        }
                        case HESPKeys.BUTTON23:
                        {
                            var key = hespKey;
                            Dispatcher.BeginInvoke(
                                (Action)delegate
                                {
                                    ImageButton23.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                });
                            break;
                        }
                        case HESPKeys.BUTTON24:
                        {
                            var key = hespKey;
                            Dispatcher.BeginInvoke(
                                (Action)delegate
                                {
                                    ImageButton24.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                });
                            break;
                        }
                        case HESPKeys.BUTTON25:
                        {
                            var key = hespKey;
                            Dispatcher.BeginInvoke(
                                (Action)delegate
                                {
                                    ImageButton25.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                });
                            break;
                        }
                        case HESPKeys.BUTTON26:
                        {
                            var key = hespKey;
                            Dispatcher.BeginInvoke(
                                (Action)delegate
                                {
                                    ImageButton26.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                });
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(3010, ex);
            }
        }

        private void UpdateKeyBindingProfileSequencedKeyStrokesHESP(TextBox textBox)
        {
            try
            {
                if (textBox.Tag == null)
                {
                    textBox.Tag = new SortedList<int, KeyPressInfo>();
                }
                
                if (textBox.Equals(TextBox1On))
                {
                    _hesp.AddOrUpdateSequencedKeyBinding(textBox.Text, HESPKeys.BUTTON1, (SortedList<int, KeyPressInfo>)textBox.Tag, false);
                }
                if (textBox.Equals(TextBox1Off))
                {
                    _hesp.AddOrUpdateSequencedKeyBinding(textBox.Text, HESPKeys.BUTTON1, (SortedList<int, KeyPressInfo>)textBox.Tag);
                }

                if (textBox.Equals(TextBox2On))
                {
                    _hesp.AddOrUpdateSequencedKeyBinding(textBox.Text, HESPKeys.BUTTON2, (SortedList<int, KeyPressInfo>)textBox.Tag, false);
                }
                if (textBox.Equals(TextBox2Off))
                {
                    _hesp.AddOrUpdateSequencedKeyBinding(textBox.Text, HESPKeys.BUTTON2, (SortedList<int, KeyPressInfo>)textBox.Tag);
                }
                if (textBox.Equals(TextBox3On))
                {
                    _hesp.AddOrUpdateSequencedKeyBinding(textBox.Text, HESPKeys.BUTTON3, (SortedList<int, KeyPressInfo>)textBox.Tag, false);
                }
                if (textBox.Equals(TextBox3Off))
                {
                    _hesp.AddOrUpdateSequencedKeyBinding(textBox.Text, HESPKeys.BUTTON3, (SortedList<int, KeyPressInfo>)textBox.Tag);
                }
                if (textBox.Equals(TextBox4On))
                {
                    _hesp.AddOrUpdateSequencedKeyBinding(textBox.Text, HESPKeys.BUTTON4, (SortedList<int, KeyPressInfo>)textBox.Tag, false);
                }
                if (textBox.Equals(TextBox4Off))
                {
                    _hesp.AddOrUpdateSequencedKeyBinding(textBox.Text, HESPKeys.BUTTON4, (SortedList<int, KeyPressInfo>)textBox.Tag);
                }
                if (textBox.Equals(TextBox5On))
                {
                    _hesp.AddOrUpdateSequencedKeyBinding(textBox.Text, HESPKeys.BUTTON5, (SortedList<int, KeyPressInfo>)textBox.Tag, false);
                }
                if (textBox.Equals(TextBox5Off))
                {
                    _hesp.AddOrUpdateSequencedKeyBinding(textBox.Text, HESPKeys.BUTTON5, (SortedList<int, KeyPressInfo>)textBox.Tag);
                }
                if (textBox.Equals(TextBox6On))
                {
                    _hesp.AddOrUpdateSequencedKeyBinding(textBox.Text, HESPKeys.BUTTON6, (SortedList<int, KeyPressInfo>)textBox.Tag, false);
                }
                if (textBox.Equals(TextBox6Off))
                {
                    _hesp.AddOrUpdateSequencedKeyBinding(textBox.Text, HESPKeys.BUTTON6, (SortedList<int, KeyPressInfo>)textBox.Tag);
                }
                if (textBox.Equals(TextBox7On))
                {
                    _hesp.AddOrUpdateSequencedKeyBinding(textBox.Text, HESPKeys.BUTTON7, (SortedList<int, KeyPressInfo>)textBox.Tag, false);
                }
                if (textBox.Equals(TextBox7Off))
                {
                    _hesp.AddOrUpdateSequencedKeyBinding(textBox.Text, HESPKeys.BUTTON7, (SortedList<int, KeyPressInfo>)textBox.Tag);
                }
                if (textBox.Equals(TextBox8On))
                {
                    _hesp.AddOrUpdateSequencedKeyBinding(textBox.Text, HESPKeys.BUTTON8, (SortedList<int, KeyPressInfo>)textBox.Tag, false);
                }
                if (textBox.Equals(TextBox8Off))
                {
                    _hesp.AddOrUpdateSequencedKeyBinding(textBox.Text, HESPKeys.BUTTON8, (SortedList<int, KeyPressInfo>)textBox.Tag);
                }
                if (textBox.Equals(TextBox9On))
                {
                    _hesp.AddOrUpdateSequencedKeyBinding(textBox.Text, HESPKeys.BUTTON9, (SortedList<int, KeyPressInfo>)textBox.Tag, false);
                }
                if (textBox.Equals(TextBox9Off))
                {
                    _hesp.AddOrUpdateSequencedKeyBinding(textBox.Text, HESPKeys.BUTTON9, (SortedList<int, KeyPressInfo>)textBox.Tag);
                }
                if (textBox.Equals(TextBox10On))
                {
                    _hesp.AddOrUpdateSequencedKeyBinding(textBox.Text, HESPKeys.BUTTON10, (SortedList<int, KeyPressInfo>)textBox.Tag, false);
                }
                if (textBox.Equals(TextBox10Off))
                {
                    _hesp.AddOrUpdateSequencedKeyBinding(textBox.Text, HESPKeys.BUTTON10, (SortedList<int, KeyPressInfo>)textBox.Tag);
                }
                if (textBox.Equals(TextBox11On))
                {
                    _hesp.AddOrUpdateSequencedKeyBinding(textBox.Text, HESPKeys.BUTTON11, (SortedList<int, KeyPressInfo>)textBox.Tag, false);
                }
                if (textBox.Equals(TextBox11Off))
                {
                    _hesp.AddOrUpdateSequencedKeyBinding(textBox.Text, HESPKeys.BUTTON11, (SortedList<int, KeyPressInfo>)textBox.Tag);
                }
                if (textBox.Equals(TextBox12On))
                {
                    _hesp.AddOrUpdateSequencedKeyBinding(textBox.Text, HESPKeys.BUTTON12, (SortedList<int, KeyPressInfo>)textBox.Tag, false);
                }
                if (textBox.Equals(TextBox12Off))
                {
                    _hesp.AddOrUpdateSequencedKeyBinding(textBox.Text, HESPKeys.BUTTON12, (SortedList<int, KeyPressInfo>)textBox.Tag);
                }
                if (textBox.Equals(TextBox13On))
                {
                    _hesp.AddOrUpdateSequencedKeyBinding(textBox.Text, HESPKeys.BUTTON13, (SortedList<int, KeyPressInfo>)textBox.Tag, false);
                }
                if (textBox.Equals(TextBox13Off))
                {
                    _hesp.AddOrUpdateSequencedKeyBinding(textBox.Text, HESPKeys.BUTTON13, (SortedList<int, KeyPressInfo>)textBox.Tag);
                }
                if (textBox.Equals(TextBox14On))
                {
                    _hesp.AddOrUpdateSequencedKeyBinding(textBox.Text, HESPKeys.BUTTON14, (SortedList<int, KeyPressInfo>)textBox.Tag, false);
                }
                if (textBox.Equals(TextBox14Off))
                {
                    _hesp.AddOrUpdateSequencedKeyBinding(textBox.Text, HESPKeys.BUTTON14, (SortedList<int, KeyPressInfo>)textBox.Tag);
                }
                if (textBox.Equals(TextBox15On))
                {
                    _hesp.AddOrUpdateSequencedKeyBinding(textBox.Text, HESPKeys.BUTTON15, (SortedList<int, KeyPressInfo>)textBox.Tag, false);
                }
                if (textBox.Equals(TextBox15Off))
                {
                    _hesp.AddOrUpdateSequencedKeyBinding(textBox.Text, HESPKeys.BUTTON15, (SortedList<int, KeyPressInfo>)textBox.Tag);
                }
                if (textBox.Equals(TextBox16On))
                {
                    _hesp.AddOrUpdateSequencedKeyBinding(textBox.Text, HESPKeys.BUTTON16, (SortedList<int, KeyPressInfo>)textBox.Tag, false);
                }
                if (textBox.Equals(TextBox16Off))
                {
                    _hesp.AddOrUpdateSequencedKeyBinding(textBox.Text, HESPKeys.BUTTON16, (SortedList<int, KeyPressInfo>)textBox.Tag);
                }
                if (textBox.Equals(TextBox17On))
                {
                    _hesp.AddOrUpdateSequencedKeyBinding(textBox.Text, HESPKeys.BUTTON17, (SortedList<int, KeyPressInfo>)textBox.Tag, false);
                }
                if (textBox.Equals(TextBox17Off))
                {
                    _hesp.AddOrUpdateSequencedKeyBinding(textBox.Text, HESPKeys.BUTTON17, (SortedList<int, KeyPressInfo>)textBox.Tag);
                }
                if (textBox.Equals(TextBox18On))
                {
                    _hesp.AddOrUpdateSequencedKeyBinding(textBox.Text, HESPKeys.BUTTON18, (SortedList<int, KeyPressInfo>)textBox.Tag, false);
                }
                if (textBox.Equals(TextBox18Off))
                {
                    _hesp.AddOrUpdateSequencedKeyBinding(textBox.Text, HESPKeys.BUTTON18, (SortedList<int, KeyPressInfo>)textBox.Tag);
                }
                if (textBox.Equals(TextBox19On))
                {
                    _hesp.AddOrUpdateSequencedKeyBinding(textBox.Text, HESPKeys.BUTTON19, (SortedList<int, KeyPressInfo>)textBox.Tag, false);
                }
                if (textBox.Equals(TextBox19Off))
                {
                    _hesp.AddOrUpdateSequencedKeyBinding(textBox.Text, HESPKeys.BUTTON19, (SortedList<int, KeyPressInfo>)textBox.Tag);
                }
                if (textBox.Equals(TextBox20On))
                {
                    _hesp.AddOrUpdateSequencedKeyBinding(textBox.Text, HESPKeys.BUTTON20, (SortedList<int, KeyPressInfo>)textBox.Tag, false);
                }
                if (textBox.Equals(TextBox20Off))
                {
                    _hesp.AddOrUpdateSequencedKeyBinding(textBox.Text, HESPKeys.BUTTON20, (SortedList<int, KeyPressInfo>)textBox.Tag);
                }
                if (textBox.Equals(TextBox21On))
                {
                    _hesp.AddOrUpdateSequencedKeyBinding(textBox.Text, HESPKeys.BUTTON21, (SortedList<int, KeyPressInfo>)textBox.Tag, false);
                }
                if (textBox.Equals(TextBox21Off))
                {
                    _hesp.AddOrUpdateSequencedKeyBinding(textBox.Text, HESPKeys.BUTTON21, (SortedList<int, KeyPressInfo>)textBox.Tag);
                }
                if (textBox.Equals(TextBox22On))
                {
                    _hesp.AddOrUpdateSequencedKeyBinding(textBox.Text, HESPKeys.BUTTON22, (SortedList<int, KeyPressInfo>)textBox.Tag, false);
                }
                if (textBox.Equals(TextBox22Off))
                {
                    _hesp.AddOrUpdateSequencedKeyBinding(textBox.Text, HESPKeys.BUTTON22, (SortedList<int, KeyPressInfo>)textBox.Tag);
                }
                if (textBox.Equals(TextBox23On))
                {
                    _hesp.AddOrUpdateSequencedKeyBinding(textBox.Text, HESPKeys.BUTTON23, (SortedList<int, KeyPressInfo>)textBox.Tag, false);
                }
                if (textBox.Equals(TextBox23Off))
                {
                    _hesp.AddOrUpdateSequencedKeyBinding(textBox.Text, HESPKeys.BUTTON23, (SortedList<int, KeyPressInfo>)textBox.Tag);
                }
                if (textBox.Equals(TextBox24On))
                {
                    _hesp.AddOrUpdateSequencedKeyBinding(textBox.Text, HESPKeys.BUTTON24, (SortedList<int, KeyPressInfo>)textBox.Tag, false);
                }
                if (textBox.Equals(TextBox24Off))
                {
                    _hesp.AddOrUpdateSequencedKeyBinding(textBox.Text, HESPKeys.BUTTON24, (SortedList<int, KeyPressInfo>)textBox.Tag);
                }
                if (textBox.Equals(TextBox25On))
                {
                    _hesp.AddOrUpdateSequencedKeyBinding(textBox.Text, HESPKeys.BUTTON25, (SortedList<int, KeyPressInfo>)textBox.Tag, false);
                }
                if (textBox.Equals(TextBox25Off))
                {
                    _hesp.AddOrUpdateSequencedKeyBinding(textBox.Text, HESPKeys.BUTTON25, (SortedList<int, KeyPressInfo>)textBox.Tag);
                }
                if (textBox.Equals(TextBox26On))
                {
                    _hesp.AddOrUpdateSequencedKeyBinding(textBox.Text, HESPKeys.BUTTON26, (SortedList<int, KeyPressInfo>)textBox.Tag, false);
                }
                if (textBox.Equals(TextBox26Off))
                {
                    _hesp.AddOrUpdateSequencedKeyBinding(textBox.Text, HESPKeys.BUTTON26, (SortedList<int, KeyPressInfo>)textBox.Tag);
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
                
                if (textBox.Equals(TextBox1On))
                {
                    _hesp.AddOrUpdateSingleKeyBinding(HESPKeys.BUTTON1, TextBox1On.Text, keyPressLength, false);
                }
                if (textBox.Equals(TextBox1Off))
                {
                    _hesp.AddOrUpdateSingleKeyBinding(HESPKeys.BUTTON1, TextBox1Off.Text, keyPressLength);
                }
                if (textBox.Equals(TextBox2On))
                {
                    _hesp.AddOrUpdateSingleKeyBinding(HESPKeys.BUTTON2, TextBox2On.Text, keyPressLength, false);
                }
                if (textBox.Equals(TextBox2Off))
                {
                    _hesp.AddOrUpdateSingleKeyBinding(HESPKeys.BUTTON2, TextBox2Off.Text, keyPressLength);
                }
                if (textBox.Equals(TextBox3On))
                {
                    _hesp.AddOrUpdateSingleKeyBinding(HESPKeys.BUTTON3, TextBox3On.Text, keyPressLength, false);
                }
                if (textBox.Equals(TextBox3Off))
                {
                    _hesp.AddOrUpdateSingleKeyBinding(HESPKeys.BUTTON3, TextBox3Off.Text, keyPressLength);
                }
                if (textBox.Equals(TextBox4On))
                {
                    _hesp.AddOrUpdateSingleKeyBinding(HESPKeys.BUTTON4, TextBox4On.Text, keyPressLength, false);
                }
                if (textBox.Equals(TextBox4Off))
                {
                    _hesp.AddOrUpdateSingleKeyBinding(HESPKeys.BUTTON4, TextBox4Off.Text, keyPressLength);
                }
                if (textBox.Equals(TextBox5On))
                {
                    _hesp.AddOrUpdateSingleKeyBinding(HESPKeys.BUTTON5, TextBox5On.Text, keyPressLength, false);
                }
                if (textBox.Equals(TextBox5Off))
                {
                    _hesp.AddOrUpdateSingleKeyBinding(HESPKeys.BUTTON5, TextBox5Off.Text, keyPressLength);
                }
                if (textBox.Equals(TextBox6On))
                {
                    _hesp.AddOrUpdateSingleKeyBinding(HESPKeys.BUTTON6, TextBox6On.Text, keyPressLength, false);
                }
                if (textBox.Equals(TextBox6Off))
                {
                    _hesp.AddOrUpdateSingleKeyBinding(HESPKeys.BUTTON6, TextBox6Off.Text, keyPressLength);
                }
                if (textBox.Equals(TextBox7On))
                {
                    _hesp.AddOrUpdateSingleKeyBinding(HESPKeys.BUTTON7, TextBox7On.Text, keyPressLength, false);
                }
                if (textBox.Equals(TextBox7Off))
                {
                    _hesp.AddOrUpdateSingleKeyBinding(HESPKeys.BUTTON7, TextBox7Off.Text, keyPressLength);
                }
                if (textBox.Equals(TextBox8On))
                {
                    _hesp.AddOrUpdateSingleKeyBinding(HESPKeys.BUTTON8, TextBox8On.Text, keyPressLength, false);
                }
                if (textBox.Equals(TextBox8Off))
                {
                    _hesp.AddOrUpdateSingleKeyBinding(HESPKeys.BUTTON8, TextBox8Off.Text, keyPressLength, false);
                }
                if (textBox.Equals(TextBox9On))
                {
                    _hesp.AddOrUpdateSingleKeyBinding(HESPKeys.BUTTON9, TextBox9On.Text, keyPressLength);
                }
                if (textBox.Equals(TextBox9Off))
                {
                    _hesp.AddOrUpdateSingleKeyBinding(HESPKeys.BUTTON9, TextBox9Off.Text, keyPressLength);
                }
                if (textBox.Equals(TextBox10On))
                {
                    _hesp.AddOrUpdateSingleKeyBinding(HESPKeys.BUTTON10, TextBox10On.Text, keyPressLength, false);
                }
                if (textBox.Equals(TextBox10Off))
                {
                    _hesp.AddOrUpdateSingleKeyBinding(HESPKeys.BUTTON10, TextBox10Off.Text, keyPressLength);
                }
                if (textBox.Equals(TextBox11On))
                {
                    _hesp.AddOrUpdateSingleKeyBinding(HESPKeys.BUTTON11, TextBox11On.Text, keyPressLength, false);
                }
                if (textBox.Equals(TextBox11Off))
                {
                    _hesp.AddOrUpdateSingleKeyBinding(HESPKeys.BUTTON11, TextBox11Off.Text, keyPressLength);
                }
                if (textBox.Equals(TextBox12On))
                {
                    _hesp.AddOrUpdateSingleKeyBinding(HESPKeys.BUTTON12, TextBox12On.Text, keyPressLength, false);
                }
                if (textBox.Equals(TextBox12Off))
                {
                    _hesp.AddOrUpdateSingleKeyBinding(HESPKeys.BUTTON12, TextBox12Off.Text, keyPressLength);
                }
                if (textBox.Equals(TextBox13On))
                {
                    _hesp.AddOrUpdateSingleKeyBinding(HESPKeys.BUTTON13, TextBox13On.Text, keyPressLength, false);
                }
                if (textBox.Equals(TextBox13Off))
                {
                    _hesp.AddOrUpdateSingleKeyBinding(HESPKeys.BUTTON13, TextBox13Off.Text, keyPressLength);
                }
                if (textBox.Equals(TextBox14On))
                {
                    _hesp.AddOrUpdateSingleKeyBinding(HESPKeys.BUTTON14, TextBox14On.Text, keyPressLength, false);
                }
                if (textBox.Equals(TextBox14Off))
                {
                    _hesp.AddOrUpdateSingleKeyBinding(HESPKeys.BUTTON14, TextBox14Off.Text, keyPressLength);
                }
                if (textBox.Equals(TextBox15On))
                {
                    _hesp.AddOrUpdateSingleKeyBinding(HESPKeys.BUTTON15, TextBox15On.Text, keyPressLength, false);
                }
                if (textBox.Equals(TextBox15Off))
                {
                    _hesp.AddOrUpdateSingleKeyBinding(HESPKeys.BUTTON15, TextBox15Off.Text, keyPressLength);
                }
                if (textBox.Equals(TextBox16On))
                {
                    _hesp.AddOrUpdateSingleKeyBinding(HESPKeys.BUTTON16, TextBox16On.Text, keyPressLength, false);
                }
                if (textBox.Equals(TextBox16Off))
                {
                    _hesp.AddOrUpdateSingleKeyBinding(HESPKeys.BUTTON16, TextBox16Off.Text, keyPressLength);
                }
                if (textBox.Equals(TextBox17On))
                {
                    _hesp.AddOrUpdateSingleKeyBinding(HESPKeys.BUTTON17, TextBox17On.Text, keyPressLength, false);
                }
                if (textBox.Equals(TextBox17Off))
                {
                    _hesp.AddOrUpdateSingleKeyBinding(HESPKeys.BUTTON17, TextBox17Off.Text, keyPressLength);
                }
                if (textBox.Equals(TextBox18On))
                {
                    _hesp.AddOrUpdateSingleKeyBinding(HESPKeys.BUTTON18, TextBox18On.Text, keyPressLength, false);
                }
                if (textBox.Equals(TextBox18Off))
                {
                    _hesp.AddOrUpdateSingleKeyBinding(HESPKeys.BUTTON18, TextBox18Off.Text, keyPressLength);
                }
                if (textBox.Equals(TextBox19On))
                {
                    _hesp.AddOrUpdateSingleKeyBinding(HESPKeys.BUTTON19, TextBox19On.Text, keyPressLength, false);
                }
                if (textBox.Equals(TextBox19Off))
                {
                    _hesp.AddOrUpdateSingleKeyBinding(HESPKeys.BUTTON19, TextBox19Off.Text, keyPressLength);
                }
                if (textBox.Equals(TextBox20On))
                {
                    _hesp.AddOrUpdateSingleKeyBinding(HESPKeys.BUTTON20, TextBox20On.Text, keyPressLength, false);
                }
                if (textBox.Equals(TextBox20Off))
                {
                    _hesp.AddOrUpdateSingleKeyBinding(HESPKeys.BUTTON20, TextBox20Off.Text, keyPressLength);
                }
                if (textBox.Equals(TextBox21On))
                {
                    _hesp.AddOrUpdateSingleKeyBinding(HESPKeys.BUTTON21, TextBox21On.Text, keyPressLength, false);
                }
                if (textBox.Equals(TextBox21Off))
                {
                    _hesp.AddOrUpdateSingleKeyBinding(HESPKeys.BUTTON21, TextBox21Off.Text, keyPressLength);
                }
                if (textBox.Equals(TextBox22On))
                {
                    _hesp.AddOrUpdateSingleKeyBinding(HESPKeys.BUTTON22, TextBox22On.Text, keyPressLength, false);
                }
                if (textBox.Equals(TextBox22Off))
                {
                    _hesp.AddOrUpdateSingleKeyBinding(HESPKeys.BUTTON22, TextBox22Off.Text, keyPressLength);
                }
                if (textBox.Equals(TextBox23On))
                {
                    _hesp.AddOrUpdateSingleKeyBinding(HESPKeys.BUTTON23, TextBox23On.Text, keyPressLength, false);
                }
                if (textBox.Equals(TextBox23Off))
                {
                    _hesp.AddOrUpdateSingleKeyBinding(HESPKeys.BUTTON23, TextBox23Off.Text, keyPressLength);
                }
                if (textBox.Equals(TextBox24On))
                {
                    _hesp.AddOrUpdateSingleKeyBinding(HESPKeys.BUTTON24, TextBox24On.Text, keyPressLength, false);
                }
                if (textBox.Equals(TextBox24Off))
                {
                    _hesp.AddOrUpdateSingleKeyBinding(HESPKeys.BUTTON24, TextBox24Off.Text, keyPressLength);
                }
                if (textBox.Equals(TextBox25On))
                {
                    _hesp.AddOrUpdateSingleKeyBinding(HESPKeys.BUTTON25, TextBox25On.Text, keyPressLength, false);
                }
                if (textBox.Equals(TextBox25Off))
                {
                    _hesp.AddOrUpdateSingleKeyBinding(HESPKeys.BUTTON25, TextBox25Off.Text, keyPressLength);
                }
                if (textBox.Equals(TextBox26On))
                {
                    _hesp.AddOrUpdateSingleKeyBinding(HESPKeys.BUTTON26, TextBox26On.Text, keyPressLength, false);
                }
                if (textBox.Equals(TextBox26Off))
                {
                    _hesp.AddOrUpdateSingleKeyBinding(HESPKeys.BUTTON26, TextBox26Off.Text, keyPressLength);
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(3012, ex);
            }
        }

        private void UpdateDCSBIOSBinding(TextBox textBox)
        {
            try
            {
                List<DCSBIOSInput> dcsBiosInputs = null;
                if (textBox.Tag == null)
                {
                    return;
                }
                if (textBox.Tag is List<DCSBIOSInput>)
                {
                    dcsBiosInputs = ((List<DCSBIOSInput>)textBox.Tag);
                }
                if (textBox.Equals(TextBox1Off))
                {
                    _hesp.AddOrUpdateDCSBIOSBinding(HESPKeys.BUTTON1, dcsBiosInputs, textBox.Text, false);
                }
                if (textBox.Equals(TextBox1On))
                {
                    _hesp.AddOrUpdateDCSBIOSBinding(HESPKeys.BUTTON1, dcsBiosInputs, textBox.Text);
                }
                if (textBox.Equals(TextBox2Off))
                {
                    _hesp.AddOrUpdateDCSBIOSBinding(HESPKeys.BUTTON3, dcsBiosInputs, textBox.Text, false);
                }
                if (textBox.Equals(TextBox2On))
                {
                    _hesp.AddOrUpdateDCSBIOSBinding(HESPKeys.BUTTON2, dcsBiosInputs, textBox.Text);
                }
                if (textBox.Equals(TextBox3Off))
                {
                    _hesp.AddOrUpdateDCSBIOSBinding(HESPKeys.BUTTON3, dcsBiosInputs, textBox.Text, false);
                }
                if (textBox.Equals(TextBox3On))
                {
                    _hesp.AddOrUpdateDCSBIOSBinding(HESPKeys.BUTTON3, dcsBiosInputs, textBox.Text);
                }
                if (textBox.Equals(TextBox4Off))
                {
                    _hesp.AddOrUpdateDCSBIOSBinding(HESPKeys.BUTTON4, dcsBiosInputs, textBox.Text, false);
                }
                if (textBox.Equals(TextBox4On))
                {
                    _hesp.AddOrUpdateDCSBIOSBinding(HESPKeys.BUTTON4, dcsBiosInputs, textBox.Text);
                }
                if (textBox.Equals(TextBox5Off))
                {
                    _hesp.AddOrUpdateDCSBIOSBinding(HESPKeys.BUTTON5, dcsBiosInputs, textBox.Text, false);
                }
                if (textBox.Equals(TextBox5On))
                {
                    _hesp.AddOrUpdateDCSBIOSBinding(HESPKeys.BUTTON5, dcsBiosInputs, textBox.Text);
                }
                if (textBox.Equals(TextBox6Off))
                {
                    _hesp.AddOrUpdateDCSBIOSBinding(HESPKeys.BUTTON6, dcsBiosInputs, textBox.Text, false);
                }
                if (textBox.Equals(TextBox6On))
                {
                    _hesp.AddOrUpdateDCSBIOSBinding(HESPKeys.BUTTON6, dcsBiosInputs, textBox.Text);
                }
                if (textBox.Equals(TextBox7Off))
                {
                    _hesp.AddOrUpdateDCSBIOSBinding(HESPKeys.BUTTON7, dcsBiosInputs, textBox.Text, false);
                }
                if (textBox.Equals(TextBox7On))
                {
                    _hesp.AddOrUpdateDCSBIOSBinding(HESPKeys.BUTTON7, dcsBiosInputs, textBox.Text);
                }
                if (textBox.Equals(TextBox8Off))
                {
                    _hesp.AddOrUpdateDCSBIOSBinding(HESPKeys.BUTTON8, dcsBiosInputs, textBox.Text, false);
                }
                if (textBox.Equals(TextBox8On))
                {
                    _hesp.AddOrUpdateDCSBIOSBinding(HESPKeys.BUTTON8, dcsBiosInputs, textBox.Text);
                }
                if (textBox.Equals(TextBox9Off))
                {
                    _hesp.AddOrUpdateDCSBIOSBinding(HESPKeys.BUTTON9, dcsBiosInputs, textBox.Text, false);
                }
                if (textBox.Equals(TextBox9On))
                {
                    _hesp.AddOrUpdateDCSBIOSBinding(HESPKeys.BUTTON9, dcsBiosInputs, textBox.Text);
                }
                if (textBox.Equals(TextBox10Off))
                {
                    _hesp.AddOrUpdateDCSBIOSBinding(HESPKeys.BUTTON10, dcsBiosInputs, textBox.Text, false);
                }
                if (textBox.Equals(TextBox10On))
                {
                    _hesp.AddOrUpdateDCSBIOSBinding(HESPKeys.BUTTON10, dcsBiosInputs, textBox.Text);
                }
                if (textBox.Equals(TextBox11Off))
                {
                    _hesp.AddOrUpdateDCSBIOSBinding(HESPKeys.BUTTON11, dcsBiosInputs, textBox.Text, false);
                }
                if (textBox.Equals(TextBox11On))
                {
                    _hesp.AddOrUpdateDCSBIOSBinding(HESPKeys.BUTTON11, dcsBiosInputs, textBox.Text);
                }
                if (textBox.Equals(TextBox12Off))
                {
                    _hesp.AddOrUpdateDCSBIOSBinding(HESPKeys.BUTTON12, dcsBiosInputs, textBox.Text, false);
                }
                if (textBox.Equals(TextBox12On))
                {
                    _hesp.AddOrUpdateDCSBIOSBinding(HESPKeys.BUTTON12, dcsBiosInputs, textBox.Text);
                }
                if (textBox.Equals(TextBox13Off))
                {
                    _hesp.AddOrUpdateDCSBIOSBinding(HESPKeys.BUTTON13, dcsBiosInputs, textBox.Text, false);
                }
                if (textBox.Equals(TextBox13On))
                {
                    _hesp.AddOrUpdateDCSBIOSBinding(HESPKeys.BUTTON13, dcsBiosInputs, textBox.Text);
                }
                if (textBox.Equals(TextBox14Off))
                {
                    _hesp.AddOrUpdateDCSBIOSBinding(HESPKeys.BUTTON14, dcsBiosInputs, textBox.Text, false);
                }
                if (textBox.Equals(TextBox14On))
                {
                    _hesp.AddOrUpdateDCSBIOSBinding(HESPKeys.BUTTON14, dcsBiosInputs, textBox.Text);
                }
                if (textBox.Equals(TextBox15Off))
                {
                    _hesp.AddOrUpdateDCSBIOSBinding(HESPKeys.BUTTON15, dcsBiosInputs, textBox.Text, false);
                }
                if (textBox.Equals(TextBox15On))
                {
                    _hesp.AddOrUpdateDCSBIOSBinding(HESPKeys.BUTTON15, dcsBiosInputs, textBox.Text);
                }
                if (textBox.Equals(TextBox16Off))
                {
                    _hesp.AddOrUpdateDCSBIOSBinding(HESPKeys.BUTTON16, dcsBiosInputs, textBox.Text, false);
                }
                if (textBox.Equals(TextBox16On))
                {
                    _hesp.AddOrUpdateDCSBIOSBinding(HESPKeys.BUTTON16, dcsBiosInputs, textBox.Text);
                }
                if (textBox.Equals(TextBox17Off))
                {
                    _hesp.AddOrUpdateDCSBIOSBinding(HESPKeys.BUTTON17, dcsBiosInputs, textBox.Text, false);
                }
                if (textBox.Equals(TextBox17On))
                {
                    _hesp.AddOrUpdateDCSBIOSBinding(HESPKeys.BUTTON17, dcsBiosInputs, textBox.Text);
                }
                if (textBox.Equals(TextBox18Off))
                {
                    _hesp.AddOrUpdateDCSBIOSBinding(HESPKeys.BUTTON18, dcsBiosInputs, textBox.Text, false);
                }
                if (textBox.Equals(TextBox18On))
                {
                    _hesp.AddOrUpdateDCSBIOSBinding(HESPKeys.BUTTON18, dcsBiosInputs, textBox.Text);
                }
                if (textBox.Equals(TextBox19Off))
                {
                    _hesp.AddOrUpdateDCSBIOSBinding(HESPKeys.BUTTON19, dcsBiosInputs, textBox.Text, false);
                }
                if (textBox.Equals(TextBox19On))
                {
                    _hesp.AddOrUpdateDCSBIOSBinding(HESPKeys.BUTTON19, dcsBiosInputs, textBox.Text);
                }
                if (textBox.Equals(TextBox20Off))
                {
                    _hesp.AddOrUpdateDCSBIOSBinding(HESPKeys.BUTTON20, dcsBiosInputs, textBox.Text, false);
                }
                if (textBox.Equals(TextBox20On))
                {
                    _hesp.AddOrUpdateDCSBIOSBinding(HESPKeys.BUTTON20, dcsBiosInputs, textBox.Text);
                }
                if (textBox.Equals(TextBox21Off))
                {
                    _hesp.AddOrUpdateDCSBIOSBinding(HESPKeys.BUTTON21, dcsBiosInputs, textBox.Text, false);
                }
                if (textBox.Equals(TextBox21On))
                {
                    _hesp.AddOrUpdateDCSBIOSBinding(HESPKeys.BUTTON21, dcsBiosInputs, textBox.Text);
                }
                if (textBox.Equals(TextBox22Off))
                {
                    _hesp.AddOrUpdateDCSBIOSBinding(HESPKeys.BUTTON22, dcsBiosInputs, textBox.Text, false);
                }
                if (textBox.Equals(TextBox22On))
                {
                    _hesp.AddOrUpdateDCSBIOSBinding(HESPKeys.BUTTON22, dcsBiosInputs, textBox.Text);
                }
                if (textBox.Equals(TextBox23Off))
                {
                    _hesp.AddOrUpdateDCSBIOSBinding(HESPKeys.BUTTON23, dcsBiosInputs, textBox.Text, false);
                }
                if (textBox.Equals(TextBox23On))
                {
                    _hesp.AddOrUpdateDCSBIOSBinding(HESPKeys.BUTTON23, dcsBiosInputs, textBox.Text);
                }
                if (textBox.Equals(TextBox24Off))
                {
                    _hesp.AddOrUpdateDCSBIOSBinding(HESPKeys.BUTTON24, dcsBiosInputs, textBox.Text, false);
                }
                if (textBox.Equals(TextBox24On))
                {
                    _hesp.AddOrUpdateDCSBIOSBinding(HESPKeys.BUTTON24, dcsBiosInputs, textBox.Text);
                }
                if (textBox.Equals(TextBox25Off))
                {
                    _hesp.AddOrUpdateDCSBIOSBinding(HESPKeys.BUTTON25, dcsBiosInputs, textBox.Text, false);
                }
                if (textBox.Equals(TextBox25On))
                {
                    _hesp.AddOrUpdateDCSBIOSBinding(HESPKeys.BUTTON25, dcsBiosInputs, textBox.Text);
                }
                if (textBox.Equals(TextBox26Off))
                {
                    _hesp.AddOrUpdateDCSBIOSBinding(HESPKeys.BUTTON26, dcsBiosInputs, textBox.Text, false);
                }
                if (textBox.Equals(TextBox26On))
                {
                    _hesp.AddOrUpdateDCSBIOSBinding(HESPKeys.BUTTON26, dcsBiosInputs, textBox.Text);
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(345012, ex);
            }
        }

        private HESPKeyOnOff GetHESPKey(TextBox textBox)
        {
            try
            {
                if (textBox.Equals(TextBox1Off))
                {
                    return new HESPKeyOnOff(HESPKeys.BUTTON1, false);
                }
                if (textBox.Equals(TextBox1On))
                {
                    return new HESPKeyOnOff(HESPKeys.BUTTON1, true);
                }
                if (textBox.Equals(TextBox2Off))
                {
                    return new HESPKeyOnOff(HESPKeys.BUTTON2, false);
                }
                if (textBox.Equals(TextBox2On))
                {
                    return new HESPKeyOnOff(HESPKeys.BUTTON2, true);
                }
                if (textBox.Equals(TextBox3Off))
                {
                    return new HESPKeyOnOff(HESPKeys.BUTTON3, false);
                }
                if (textBox.Equals(TextBox3On))
                {
                    return new HESPKeyOnOff(HESPKeys.BUTTON3, true);
                }
                if (textBox.Equals(TextBox4Off))
                {
                    return new HESPKeyOnOff(HESPKeys.BUTTON4, false);
                }
                if (textBox.Equals(TextBox4On))
                {
                    return new HESPKeyOnOff(HESPKeys.BUTTON4, true);
                }
                if (textBox.Equals(TextBox5Off))
                {
                    return new HESPKeyOnOff(HESPKeys.BUTTON5, false);
                }
                if (textBox.Equals(TextBox5On))
                {
                    return new HESPKeyOnOff(HESPKeys.BUTTON5, true);
                }
                if (textBox.Equals(TextBox6Off))
                {
                    return new HESPKeyOnOff(HESPKeys.BUTTON6, false);
                }
                if (textBox.Equals(TextBox6On))
                {
                    return new HESPKeyOnOff(HESPKeys.BUTTON6, true);
                }
                if (textBox.Equals(TextBox7Off))
                {
                    return new HESPKeyOnOff(HESPKeys.BUTTON7, false);
                }
                if (textBox.Equals(TextBox7On))
                {
                    return new HESPKeyOnOff(HESPKeys.BUTTON7, true);
                }
                if (textBox.Equals(TextBox8Off))
                {
                    return new HESPKeyOnOff(HESPKeys.BUTTON8, false);
                }
                if (textBox.Equals(TextBox8On))
                {
                    return new HESPKeyOnOff(HESPKeys.BUTTON8, true);
                }
                if (textBox.Equals(TextBox9Off))
                {
                    return new HESPKeyOnOff(HESPKeys.BUTTON9, false);
                }
                if (textBox.Equals(TextBox9On))
                {
                    return new HESPKeyOnOff(HESPKeys.BUTTON9, true);
                }
                if (textBox.Equals(TextBox10Off))
                {
                    return new HESPKeyOnOff(HESPKeys.BUTTON10, false);
                }
                if (textBox.Equals(TextBox10On))
                {
                    return new HESPKeyOnOff(HESPKeys.BUTTON10, true);
                }
                if (textBox.Equals(TextBox11Off))
                {
                    return new HESPKeyOnOff(HESPKeys.BUTTON11, false);
                }
                if (textBox.Equals(TextBox11On))
                {
                    return new HESPKeyOnOff(HESPKeys.BUTTON11, true);
                }
                if (textBox.Equals(TextBox12Off))
                {
                    return new HESPKeyOnOff(HESPKeys.BUTTON12, false);
                }
                if (textBox.Equals(TextBox12On))
                {
                    return new HESPKeyOnOff(HESPKeys.BUTTON12, true);
                }
                if (textBox.Equals(TextBox13Off))
                {
                    return new HESPKeyOnOff(HESPKeys.BUTTON13, false);
                }
                if (textBox.Equals(TextBox13On))
                {
                    return new HESPKeyOnOff(HESPKeys.BUTTON13, true);
                }
                if (textBox.Equals(TextBox14Off))
                {
                    return new HESPKeyOnOff(HESPKeys.BUTTON14, false);
                }
                if (textBox.Equals(TextBox14On))
                {
                    return new HESPKeyOnOff(HESPKeys.BUTTON14, true);
                }
                if (textBox.Equals(TextBox15Off))
                {
                    return new HESPKeyOnOff(HESPKeys.BUTTON15, false);
                }
                if (textBox.Equals(TextBox15On))
                {
                    return new HESPKeyOnOff(HESPKeys.BUTTON15, true);
                }
                if (textBox.Equals(TextBox16Off))
                {
                    return new HESPKeyOnOff(HESPKeys.BUTTON16, false);
                }
                if (textBox.Equals(TextBox16On))
                {
                    return new HESPKeyOnOff(HESPKeys.BUTTON16, true);
                }
                if (textBox.Equals(TextBox17Off))
                {
                    return new HESPKeyOnOff(HESPKeys.BUTTON17, false);
                }
                if (textBox.Equals(TextBox17On))
                {
                    return new HESPKeyOnOff(HESPKeys.BUTTON17, true);
                }
                if (textBox.Equals(TextBox18Off))
                {
                    return new HESPKeyOnOff(HESPKeys.BUTTON18, false);
                }
                if (textBox.Equals(TextBox18On))
                {
                    return new HESPKeyOnOff(HESPKeys.BUTTON18, true);
                }
                if (textBox.Equals(TextBox19Off))
                {
                    return new HESPKeyOnOff(HESPKeys.BUTTON19, false);
                }
                if (textBox.Equals(TextBox19On))
                {
                    return new HESPKeyOnOff(HESPKeys.BUTTON19, true);
                }

                if (textBox.Equals(TextBox20Off))
                {
                    return new HESPKeyOnOff(HESPKeys.BUTTON20, false);
                }
                if (textBox.Equals(TextBox20On))
                {
                    return new HESPKeyOnOff(HESPKeys.BUTTON20, true);
                }

                if (textBox.Equals(TextBox21Off))
                {
                    return new HESPKeyOnOff(HESPKeys.BUTTON21, false);
                }
                if (textBox.Equals(TextBox21On))
                {
                    return new HESPKeyOnOff(HESPKeys.BUTTON21, true);
                }

                if (textBox.Equals(TextBox22Off))
                {
                    return new HESPKeyOnOff(HESPKeys.BUTTON22, false);
                }
                if (textBox.Equals(TextBox22On))
                {
                    return new HESPKeyOnOff(HESPKeys.BUTTON22, true);
                }
                if (textBox.Equals(TextBox23Off))
                {
                    return new HESPKeyOnOff(HESPKeys.BUTTON23, false);
                }
                if (textBox.Equals(TextBox23On))
                {
                    return new HESPKeyOnOff(HESPKeys.BUTTON23, true);
                }
                if (textBox.Equals(TextBox24Off))
                {
                    return new HESPKeyOnOff(HESPKeys.BUTTON24, false);
                }
                if (textBox.Equals(TextBox24On))
                {
                    return new HESPKeyOnOff(HESPKeys.BUTTON24, true);
                }
                if (textBox.Equals(TextBox25Off))
                {
                    return new HESPKeyOnOff(HESPKeys.BUTTON25, false);
                }
                if (textBox.Equals(TextBox25On))
                {
                    return new HESPKeyOnOff(HESPKeys.BUTTON25, true);
                }
                if (textBox.Equals(TextBox26Off))
                {
                    return new HESPKeyOnOff(HESPKeys.BUTTON26, false);
                }
                if (textBox.Equals(TextBox26On))
                {
                    return new HESPKeyOnOff(HESPKeys.BUTTON26, true);
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(345012, ex);
            }
            throw new Exception("Should not reach this point");
        }

        private void ShowGraphicConfiguration()
        {
            try
            {
                TextBox textBoxOn = null;
                TextBox textBoxOff = null;
                foreach (var keyBinding in _hesp.KeyBindingsHashSet)
                {

                    if (keyBinding.HESPKey == HESPKeys.BUTTON1 && keyBinding.WhenTurnedOn)
                    {
                        textBoxOn = TextBox1On;
                    }
                    else if (keyBinding.HESPKey == HESPKeys.BUTTON1 && !keyBinding.WhenTurnedOn)
                    {
                        textBoxOff = TextBox1Off;
                    }
                    if (keyBinding.HESPKey == HESPKeys.BUTTON2 && keyBinding.WhenTurnedOn)
                    {
                        textBoxOn = TextBox2On;
                    }
                    else if (keyBinding.HESPKey == HESPKeys.BUTTON2 && !keyBinding.WhenTurnedOn)
                    {
                        textBoxOff = TextBox2Off;
                    }
                    if (keyBinding.HESPKey == HESPKeys.BUTTON3 && keyBinding.WhenTurnedOn)
                    {
                        textBoxOn = TextBox3On;
                    }
                    else if (keyBinding.HESPKey == HESPKeys.BUTTON3 && !keyBinding.WhenTurnedOn)
                    {
                        textBoxOff = TextBox3Off;
                    }
                    if (keyBinding.HESPKey == HESPKeys.BUTTON4 && keyBinding.WhenTurnedOn)
                    {
                        textBoxOn = TextBox4On;
                    }
                    else if (keyBinding.HESPKey == HESPKeys.BUTTON4 && !keyBinding.WhenTurnedOn)
                    {
                        textBoxOff = TextBox4Off;
                    }
                    if (keyBinding.HESPKey == HESPKeys.BUTTON5 && keyBinding.WhenTurnedOn)
                    {
                        textBoxOn = TextBox5On;
                    }
                    else if (keyBinding.HESPKey == HESPKeys.BUTTON5 && !keyBinding.WhenTurnedOn)
                    {
                        textBoxOff = TextBox5Off;
                    }
                    if (keyBinding.HESPKey == HESPKeys.BUTTON6 && keyBinding.WhenTurnedOn)
                    {
                        textBoxOn = TextBox6On;
                    }
                    else if (keyBinding.HESPKey == HESPKeys.BUTTON6 && !keyBinding.WhenTurnedOn)
                    {
                        textBoxOff = TextBox6Off;
                    }
                    if (keyBinding.HESPKey == HESPKeys.BUTTON7 && keyBinding.WhenTurnedOn)
                    {
                        textBoxOn = TextBox7On;
                    }
                    else if (keyBinding.HESPKey == HESPKeys.BUTTON7 && !keyBinding.WhenTurnedOn)
                    {
                        textBoxOff = TextBox7Off;
                    }
                    if (keyBinding.HESPKey == HESPKeys.BUTTON8 && keyBinding.WhenTurnedOn)
                    {
                        textBoxOn = TextBox8On;
                    }
                    else if (keyBinding.HESPKey == HESPKeys.BUTTON8 && !keyBinding.WhenTurnedOn)
                    {
                        textBoxOff = TextBox8Off;
                    }
                    if (keyBinding.HESPKey == HESPKeys.BUTTON9 && keyBinding.WhenTurnedOn)
                    {
                        textBoxOn = TextBox9On;
                    }
                    else if (keyBinding.HESPKey == HESPKeys.BUTTON9 && !keyBinding.WhenTurnedOn)
                    {
                        textBoxOff = TextBox9Off;
                    }
                    if (keyBinding.HESPKey == HESPKeys.BUTTON10 && keyBinding.WhenTurnedOn)
                    {
                        textBoxOn = TextBox10On;
                    }
                    else if (keyBinding.HESPKey == HESPKeys.BUTTON10 && !keyBinding.WhenTurnedOn)
                    {
                        textBoxOff = TextBox10Off;
                    }
                    if (keyBinding.HESPKey == HESPKeys.BUTTON11 && keyBinding.WhenTurnedOn)
                    {
                        textBoxOn = TextBox11On;
                    }
                    else if (keyBinding.HESPKey == HESPKeys.BUTTON11 && !keyBinding.WhenTurnedOn)
                    {
                        textBoxOff = TextBox11Off;
                    }
                    if (keyBinding.HESPKey == HESPKeys.BUTTON12 && keyBinding.WhenTurnedOn)
                    {
                        textBoxOn = TextBox12On;
                    }
                    else if (keyBinding.HESPKey == HESPKeys.BUTTON12 && !keyBinding.WhenTurnedOn)
                    {
                        textBoxOff = TextBox12Off;
                    }
                    if (keyBinding.HESPKey == HESPKeys.BUTTON13 && keyBinding.WhenTurnedOn)
                    {
                        textBoxOn = TextBox13On;
                    }
                    else if (keyBinding.HESPKey == HESPKeys.BUTTON13 && !keyBinding.WhenTurnedOn)
                    {
                        textBoxOff = TextBox13Off;
                    }
                    if (keyBinding.HESPKey == HESPKeys.BUTTON14 && keyBinding.WhenTurnedOn)
                    {
                        textBoxOn = TextBox14On;
                    }
                    else if (keyBinding.HESPKey == HESPKeys.BUTTON14 && !keyBinding.WhenTurnedOn)
                    {
                        textBoxOff = TextBox14Off;
                    }
                    if (keyBinding.HESPKey == HESPKeys.BUTTON15 && keyBinding.WhenTurnedOn)
                    {
                        textBoxOn = TextBox15On;
                    }
                    else if (keyBinding.HESPKey == HESPKeys.BUTTON15 && !keyBinding.WhenTurnedOn)
                    {
                        textBoxOff = TextBox15Off;
                    }
                    if (keyBinding.HESPKey == HESPKeys.BUTTON16 && keyBinding.WhenTurnedOn)
                    {
                        textBoxOn = TextBox16On;
                    }
                    else if (keyBinding.HESPKey == HESPKeys.BUTTON16 && !keyBinding.WhenTurnedOn)
                    {
                        textBoxOff = TextBox16Off;
                    }
                    if (keyBinding.HESPKey == HESPKeys.BUTTON17 && keyBinding.WhenTurnedOn)
                    {
                        textBoxOn = TextBox17On;
                    }
                    else if (keyBinding.HESPKey == HESPKeys.BUTTON17 && !keyBinding.WhenTurnedOn)
                    {
                        textBoxOff = TextBox17Off;
                    }
                    if (keyBinding.HESPKey == HESPKeys.BUTTON18 && keyBinding.WhenTurnedOn)
                    {
                        textBoxOn = TextBox18On;
                    }
                    else if (keyBinding.HESPKey == HESPKeys.BUTTON18 && !keyBinding.WhenTurnedOn)
                    {
                        textBoxOff = TextBox18Off;
                    }
                    if (keyBinding.HESPKey == HESPKeys.BUTTON19 && keyBinding.WhenTurnedOn)
                    {
                        textBoxOn = TextBox19On;
                    }
                    else if (keyBinding.HESPKey == HESPKeys.BUTTON19 && !keyBinding.WhenTurnedOn)
                    {
                        textBoxOff = TextBox19Off;
                    }
                    if (keyBinding.HESPKey == HESPKeys.BUTTON20 && keyBinding.WhenTurnedOn)
                    {
                        textBoxOn = TextBox20On;
                    }
                    else if (keyBinding.HESPKey == HESPKeys.BUTTON20 && !keyBinding.WhenTurnedOn)
                    {
                        textBoxOff = TextBox2Off;
                    }
                    if (keyBinding.HESPKey == HESPKeys.BUTTON21 && keyBinding.WhenTurnedOn)
                    {
                        textBoxOn = TextBox21On;
                    }
                    else if (keyBinding.HESPKey == HESPKeys.BUTTON21 && !keyBinding.WhenTurnedOn)
                    {
                        textBoxOff = TextBox21Off;
                    }
                    if (keyBinding.HESPKey == HESPKeys.BUTTON22 && keyBinding.WhenTurnedOn)
                    {
                        textBoxOn = TextBox22On;
                    }
                    else if (keyBinding.HESPKey == HESPKeys.BUTTON22 && !keyBinding.WhenTurnedOn)
                    {
                        textBoxOff = TextBox22Off;
                    }
                    if (keyBinding.HESPKey == HESPKeys.BUTTON23 && keyBinding.WhenTurnedOn)
                    {
                        textBoxOn = TextBox23On;
                    }
                    else if (keyBinding.HESPKey == HESPKeys.BUTTON23 && !keyBinding.WhenTurnedOn)
                    {
                        textBoxOff = TextBox23Off;
                    }
                    if (keyBinding.HESPKey == HESPKeys.BUTTON24 && keyBinding.WhenTurnedOn)
                    {
                        textBoxOn = TextBox24On;
                    }
                    else if (keyBinding.HESPKey == HESPKeys.BUTTON24 && !keyBinding.WhenTurnedOn)
                    {
                        textBoxOff = TextBox24Off;
                    }
                    if (keyBinding.HESPKey == HESPKeys.BUTTON25 && keyBinding.WhenTurnedOn)
                    {
                        textBoxOn = TextBox25On;
                    }
                    else if (keyBinding.HESPKey == HESPKeys.BUTTON25 && !keyBinding.WhenTurnedOn)
                    {
                        textBoxOff = TextBox25Off;
                    }
                    if (keyBinding.HESPKey == HESPKeys.BUTTON26 && keyBinding.WhenTurnedOn)
                    {
                        textBoxOn = TextBox26On;
                    }
                    else if (keyBinding.HESPKey == HESPKeys.BUTTON26 && !keyBinding.WhenTurnedOn)
                    {
                        textBoxOff = TextBox26Off;
                    }

                    if (textBoxOn != null && keyBinding.WhenTurnedOn)
                    {
                        if (keyBinding.OSKeyPress != null && !keyBinding.OSKeyPress.IsMultiSequenced())
                        {
                            textBoxOn.Tag = keyBinding.OSKeyPress.LengthOfKeyPress();
                            textBoxOn.Text = keyBinding.OSKeyPress.GetSimpleVirtualKeyCodesAsString();
                        }
                        else if (keyBinding.OSKeyPress != null && keyBinding.OSKeyPress.IsMultiSequenced())
                        {
                            textBoxOn.Tag = keyBinding.OSKeyPress.GetSequence;
                            textBoxOn.Text = keyBinding.OSKeyPress.Information;
                        }
                    }
                    else if (textBoxOff != null && !keyBinding.WhenTurnedOn)
                    {
                        if (keyBinding.OSKeyPress != null && !keyBinding.OSKeyPress.IsMultiSequenced())
                        {
                            textBoxOff.Tag = keyBinding.OSKeyPress.LengthOfKeyPress();
                            textBoxOff.Text = keyBinding.OSKeyPress.GetSimpleVirtualKeyCodesAsString();
                        }
                        else if (keyBinding.OSKeyPress != null && keyBinding.OSKeyPress.IsMultiSequenced())
                        {
                            textBoxOff.Tag = keyBinding.OSKeyPress.GetSequence;
                            textBoxOff.Text = keyBinding.OSKeyPress.Information;
                        }
                    }
                }






                foreach (var dcsBiosBinding in _hesp.DCSBiosBindings)
                {
                    if (dcsBiosBinding.HESPKey == HESPKeys.BUTTON1 && dcsBiosBinding.WhenTurnedOn)
                    {
                        textBoxOn = TextBox1On;
                    }
                    else if (dcsBiosBinding.HESPKey == HESPKeys.BUTTON1 && !dcsBiosBinding.WhenTurnedOn)
                    {
                        textBoxOff = TextBox1Off;
                    }
                    if (dcsBiosBinding.HESPKey == HESPKeys.BUTTON2 && dcsBiosBinding.WhenTurnedOn)
                    {
                        textBoxOn = TextBox2On;
                    }
                    else if (dcsBiosBinding.HESPKey == HESPKeys.BUTTON2 && !dcsBiosBinding.WhenTurnedOn)
                    {
                        textBoxOff = TextBox2Off;
                    }
                    if (dcsBiosBinding.HESPKey == HESPKeys.BUTTON3 && dcsBiosBinding.WhenTurnedOn)
                    {
                        textBoxOn = TextBox3On;
                    }
                    else if (dcsBiosBinding.HESPKey == HESPKeys.BUTTON3 && !dcsBiosBinding.WhenTurnedOn)
                    {
                        textBoxOff = TextBox3Off;
                    }
                    if (dcsBiosBinding.HESPKey == HESPKeys.BUTTON4 && dcsBiosBinding.WhenTurnedOn)
                    {
                        textBoxOn = TextBox4On;
                    }
                    else if (dcsBiosBinding.HESPKey == HESPKeys.BUTTON4 && !dcsBiosBinding.WhenTurnedOn)
                    {
                        textBoxOff = TextBox4Off;
                    }
                    if (dcsBiosBinding.HESPKey == HESPKeys.BUTTON5 && dcsBiosBinding.WhenTurnedOn)
                    {
                        textBoxOn = TextBox5On;
                    }
                    else if (dcsBiosBinding.HESPKey == HESPKeys.BUTTON5 && !dcsBiosBinding.WhenTurnedOn)
                    {
                        textBoxOff = TextBox5Off;
                    }
                    if (dcsBiosBinding.HESPKey == HESPKeys.BUTTON6 && dcsBiosBinding.WhenTurnedOn)
                    {
                        textBoxOn = TextBox6On;
                    }
                    else if (dcsBiosBinding.HESPKey == HESPKeys.BUTTON6 && !dcsBiosBinding.WhenTurnedOn)
                    {
                        textBoxOff = TextBox6Off;
                    }
                    if (dcsBiosBinding.HESPKey == HESPKeys.BUTTON7 && dcsBiosBinding.WhenTurnedOn)
                    {
                        textBoxOn = TextBox7On;
                    }
                    else if (dcsBiosBinding.HESPKey == HESPKeys.BUTTON7 && !dcsBiosBinding.WhenTurnedOn)
                    {
                        textBoxOff = TextBox7Off;
                    }
                    if (dcsBiosBinding.HESPKey == HESPKeys.BUTTON8 && dcsBiosBinding.WhenTurnedOn)
                    {
                        textBoxOn = TextBox8On;
                    }
                    else if (dcsBiosBinding.HESPKey == HESPKeys.BUTTON8 && !dcsBiosBinding.WhenTurnedOn)
                    {
                        textBoxOff = TextBox8Off;
                    }
                    if (dcsBiosBinding.HESPKey == HESPKeys.BUTTON9 && dcsBiosBinding.WhenTurnedOn)
                    {
                        textBoxOn = TextBox9On;
                    }
                    else if (dcsBiosBinding.HESPKey == HESPKeys.BUTTON9 && !dcsBiosBinding.WhenTurnedOn)
                    {
                        textBoxOff = TextBox9Off;
                    }
                    if (dcsBiosBinding.HESPKey == HESPKeys.BUTTON10 && dcsBiosBinding.WhenTurnedOn)
                    {
                        textBoxOn = TextBox10On;
                    }
                    else if (dcsBiosBinding.HESPKey == HESPKeys.BUTTON10 && !dcsBiosBinding.WhenTurnedOn)
                    {
                        textBoxOff = TextBox10Off;
                    }
                    if (dcsBiosBinding.HESPKey == HESPKeys.BUTTON11 && dcsBiosBinding.WhenTurnedOn)
                    {
                        textBoxOn = TextBox11On;
                    }
                    else if (dcsBiosBinding.HESPKey == HESPKeys.BUTTON11 && !dcsBiosBinding.WhenTurnedOn)
                    {
                        textBoxOff = TextBox11Off;
                    }
                    if (dcsBiosBinding.HESPKey == HESPKeys.BUTTON12 && dcsBiosBinding.WhenTurnedOn)
                    {
                        textBoxOn = TextBox12On;
                    }
                    else if (dcsBiosBinding.HESPKey == HESPKeys.BUTTON12 && !dcsBiosBinding.WhenTurnedOn)
                    {
                        textBoxOff = TextBox12Off;
                    }
                    if (dcsBiosBinding.HESPKey == HESPKeys.BUTTON13 && dcsBiosBinding.WhenTurnedOn)
                    {
                        textBoxOn = TextBox13On;
                    }
                    else if (dcsBiosBinding.HESPKey == HESPKeys.BUTTON13 && !dcsBiosBinding.WhenTurnedOn)
                    {
                        textBoxOff = TextBox13Off;
                    }
                    if (dcsBiosBinding.HESPKey == HESPKeys.BUTTON14 && dcsBiosBinding.WhenTurnedOn)
                    {
                        textBoxOn = TextBox14On;
                    }
                    else if (dcsBiosBinding.HESPKey == HESPKeys.BUTTON14 && !dcsBiosBinding.WhenTurnedOn)
                    {
                        textBoxOff = TextBox14Off;
                    }
                    if (dcsBiosBinding.HESPKey == HESPKeys.BUTTON15 && dcsBiosBinding.WhenTurnedOn)
                    {
                        textBoxOn = TextBox15On;
                    }
                    else if (dcsBiosBinding.HESPKey == HESPKeys.BUTTON15 && !dcsBiosBinding.WhenTurnedOn)
                    {
                        textBoxOff = TextBox15Off;
                    }
                    if (dcsBiosBinding.HESPKey == HESPKeys.BUTTON16 && dcsBiosBinding.WhenTurnedOn)
                    {
                        textBoxOn = TextBox16On;
                    }
                    else if (dcsBiosBinding.HESPKey == HESPKeys.BUTTON16 && !dcsBiosBinding.WhenTurnedOn)
                    {
                        textBoxOff = TextBox16Off;
                    }
                    if (dcsBiosBinding.HESPKey == HESPKeys.BUTTON17 && dcsBiosBinding.WhenTurnedOn)
                    {
                        textBoxOn = TextBox17On;
                    }
                    else if (dcsBiosBinding.HESPKey == HESPKeys.BUTTON17 && !dcsBiosBinding.WhenTurnedOn)
                    {
                        textBoxOff = TextBox17Off;
                    }
                    if (dcsBiosBinding.HESPKey == HESPKeys.BUTTON18 && dcsBiosBinding.WhenTurnedOn)
                    {
                        textBoxOn = TextBox18On;
                    }
                    else if (dcsBiosBinding.HESPKey == HESPKeys.BUTTON18 && !dcsBiosBinding.WhenTurnedOn)
                    {
                        textBoxOff = TextBox18Off;
                    }
                    if (dcsBiosBinding.HESPKey == HESPKeys.BUTTON19 && dcsBiosBinding.WhenTurnedOn)
                    {
                        textBoxOn = TextBox19On;
                    }
                    else if (dcsBiosBinding.HESPKey == HESPKeys.BUTTON19 && !dcsBiosBinding.WhenTurnedOn)
                    {
                        textBoxOff = TextBox19Off;
                    }
                    if (dcsBiosBinding.HESPKey == HESPKeys.BUTTON20 && dcsBiosBinding.WhenTurnedOn)
                    {
                        textBoxOn = TextBox20On;
                    }
                    else if (dcsBiosBinding.HESPKey == HESPKeys.BUTTON20 && !dcsBiosBinding.WhenTurnedOn)
                    {
                        textBoxOff = TextBox20Off;
                    }
                    if (dcsBiosBinding.HESPKey == HESPKeys.BUTTON21 && dcsBiosBinding.WhenTurnedOn)
                    {
                        textBoxOn = TextBox21On;
                    }
                    else if (dcsBiosBinding.HESPKey == HESPKeys.BUTTON21 && !dcsBiosBinding.WhenTurnedOn)
                    {
                        textBoxOff = TextBox21Off;
                    }
                    if (dcsBiosBinding.HESPKey == HESPKeys.BUTTON22 && dcsBiosBinding.WhenTurnedOn)
                    {
                        textBoxOn = TextBox22On;
                    }
                    else if (dcsBiosBinding.HESPKey == HESPKeys.BUTTON22 && !dcsBiosBinding.WhenTurnedOn)
                    {
                        textBoxOff = TextBox22Off;
                    }
                    if (dcsBiosBinding.HESPKey == HESPKeys.BUTTON23 && dcsBiosBinding.WhenTurnedOn)
                    {
                        textBoxOn = TextBox23On;
                    }
                    else if (dcsBiosBinding.HESPKey == HESPKeys.BUTTON23 && !dcsBiosBinding.WhenTurnedOn)
                    {
                        textBoxOff = TextBox23Off;
                    }
                    if (dcsBiosBinding.HESPKey == HESPKeys.BUTTON24 && dcsBiosBinding.WhenTurnedOn)
                    {
                        textBoxOn = TextBox24On;
                    }
                    else if (dcsBiosBinding.HESPKey == HESPKeys.BUTTON24 && !dcsBiosBinding.WhenTurnedOn)
                    {
                        textBoxOff = TextBox24Off;
                    }
                    if (dcsBiosBinding.HESPKey == HESPKeys.BUTTON25 && dcsBiosBinding.WhenTurnedOn)
                    {
                        textBoxOn = TextBox25On;
                    }
                    else if (dcsBiosBinding.HESPKey == HESPKeys.BUTTON25 && !dcsBiosBinding.WhenTurnedOn)
                    {
                        textBoxOff = TextBox25Off;
                    }
                    if (dcsBiosBinding.HESPKey == HESPKeys.BUTTON26 && dcsBiosBinding.WhenTurnedOn)
                    {
                        textBoxOn = TextBox26On;
                    }
                    else if (dcsBiosBinding.HESPKey == HESPKeys.BUTTON26 && !dcsBiosBinding.WhenTurnedOn)
                    {
                        textBoxOff = TextBox26Off;
                    }


                    if (textBoxOn != null && dcsBiosBinding.WhenTurnedOn)
                    {
                        if (dcsBiosBinding.DCSBIOSInputs.Count > 0)
                        {
                            textBoxOn.Tag = dcsBiosBinding.DCSBIOSInputs;
                            textBoxOn.Text = dcsBiosBinding.Description;
                            textBoxOn.ToolTip = "DCS-BIOS";
                        }
                    }
                    else if(textBoxOff != null)
                    {
                        if (dcsBiosBinding.DCSBIOSInputs.Count > 0)
                        {
                            textBoxOff.Tag = dcsBiosBinding.DCSBIOSInputs;
                            textBoxOff.Text = dcsBiosBinding.Description;
                            textBoxOff.ToolTip = "DCS-BIOS";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(3013, ex);
            }
        }

        private void ButtonGetId_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_hesp != null)
                {
                    Clipboard.SetText(_hesp.InstanceId);
                    MessageBox.Show("The Instance Id for the panel has been copied to the Clipboard.");
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(3015, ex);
            }
        }
    }
}
