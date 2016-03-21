using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using DCS_BIOS;
using NonVisuals;

namespace ProUsbPanels
{
    /// <summary>
    /// Interaction logic for TPMPanelUserControl.xaml
    /// </summary>
public partial class TPMPanelUserControl : ISaitekPanelListener, IProfileHandlerListener, ISaitekUserControl
    {

        private readonly TPMPanel _tpmPanel;
        private TabItem _parentTabItem;
        private string _parentTabItemHeader;
        private IGlobalHandler _globalHandler;
        private bool _once;

        public TPMPanelUserControl(HIDSkeleton hidSkeleton, TabItem parentTabItem, IGlobalHandler globalHandler)
        {
            InitializeComponent();
            _parentTabItem = parentTabItem;
            _parentTabItemHeader = _parentTabItem.Header.ToString();
            _tpmPanel = new TPMPanel(hidSkeleton);

            _tpmPanel.Attach((ISaitekPanelListener)this);
            globalHandler.Attach(_tpmPanel);
            _globalHandler = globalHandler;
        }

        private void TPMPanelUserControl_OnLoaded(object sender, RoutedEventArgs e)
        {
            if (!_once)
            {
                HidePositionIndicators();
                _once = true;
            }
            SetContextMenuClickHandlers();
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
                Common.ShowErrorMessageBox(400003, ex);
            }
        }

        public SaitekPanel GetSaitekPanel()
        {
            return _tpmPanel;
        }

        public string GetName()
        {
            return GetType().Name;
        }

        public void SelectedAirframe(DCSAirframe dcsAirframe)
        {
            try
            {
               //zip zilch nada
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
                if (saitekPanelsEnum == SaitekPanelsEnum.TPM && uniqueId.Equals(_tpmPanel.InstanceId))
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
                if (saitekPanelsEnum == SaitekPanelsEnum.TPM && uniqueId.Equals(_tpmPanel.InstanceId))
                {
                    //Dispatcher.BeginInvoke((Action)(() => _parentTabItem.Header = _parentTabItemHeader + " (connected)"));
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(2008, ex);
            }
        }

        public void LedLightChanged(string uniqueId, SaitekPanelLEDPosition saitekPanelLEDPosition, PanelLEDColor panelLEDColor)
        {
            try
            {
                //nada zip zilch
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(1021, ex);
            }
        }

        public void DeviceDetached(string uniqueId, SaitekPanelsEnum saitekPanelsEnum)
        {
            try
            {
                if (saitekPanelsEnum == SaitekPanelsEnum.TPM && uniqueId.Equals(_tpmPanel.InstanceId))
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
                if (uniqueId.Equals(_tpmPanel.InstanceId) && saitekPanelsEnum == SaitekPanelsEnum.TPM)
                {
                    Dispatcher.BeginInvoke((Action)(ShowGraphicConfiguration));
                    Dispatcher.BeginInvoke((Action)(() => TextBoxLogTPM.Text = ""));
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
                TextBoxLogTPM.Focus();
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
                        UpdateKeyBindingProfileSequencedKeyStrokesTPM(textBox);
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
                _tpmPanel.ClearSettings();
            }
        }

        private void SetContextMenuClickHandlers()
        {
            foreach (var textBox in Common.FindVisualChildren<TextBox>(this))
            {
                if (textBox != TextBoxLogTPM)
                {
                    textBox.ContextMenu = (ContextMenu)Resources["TextBoxContextMenuTPM"];
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
                        else if (!_tpmPanel.KeyboardEmulationOnly && item.Name.Contains("EditDCSBIOS"))
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
                if (textBox != TextBoxLogTPM && textBox.IsFocused && textBox.Background == Brushes.Yellow)
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
                        _tpmPanel.ClearAllBindings(GetTPMSwitch(textBox));
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
                    //Common.DebugP("Pressed key is " + keyCode);
                    hashSetOfKeysPressed.Add(Enum.GetName(typeof(VirtualKeyCode), keyCode));
                }
                var modifiers = Common.GetPressedVirtualKeyCodesThatAreModifiers();
                foreach (var virtualKeyCode in modifiers)
                {
                    //Common.DebugP("Pressed modifiers -->  " + virtualKeyCode);
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

                var modifiers = Common.GetPressedVirtualKeyCodesThatAreModifiers();
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
                UpdateKeyBindingProfileSequencedKeyStrokesTPM(textBox);
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
                Dispatcher.BeginInvoke((Action)(() => TextBoxLogTPM.Focus()));
                foreach (var tpmPanelSwitch in switches)
                {
                    var key = (TPMPanelSwitch)tpmPanelSwitch;

                    if (_tpmPanel.ForwardKeyPresses)
                    {
                        if (!string.IsNullOrEmpty(_tpmPanel.GetKeyPressForLoggingPurposes(key)))
                        {
                            Dispatcher.BeginInvoke(
                                (Action)
                                (() =>
                                 TextBoxLogTPM.Text =
                                 TextBoxLogTPM.Text.Insert(0, _tpmPanel.GetKeyPressForLoggingPurposes(key) + "\n")));
                        }
                    }
                    else
                    {
                        Dispatcher.BeginInvoke(
                            (Action)
                            (() =>
                             TextBoxLogTPM.Text =
                             TextBoxLogTPM.Text = TextBoxLogTPM.Text.Insert(0, "No action taken, virtual key press disabled.\n")));
                    }
                }
                SetGraphicsState(switches);
            }
            catch (Exception ex)
            {
                Dispatcher.BeginInvoke(
                    (Action)
                    (() =>
                     TextBoxLogTPM.Text = TextBoxLogTPM.Text.Insert(0, "0x16" + ex.Message + ".\n")));
                Common.ShowErrorMessageBox(3009, ex);
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
                                Dispatcher.BeginInvoke(
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
                                Dispatcher.BeginInvoke(
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
                                Dispatcher.BeginInvoke(
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
                                Dispatcher.BeginInvoke(
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
                                Dispatcher.BeginInvoke(
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
                                Dispatcher.BeginInvoke(
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
                                Dispatcher.BeginInvoke(
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
                                Dispatcher.BeginInvoke(
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
                                Dispatcher.BeginInvoke(
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
                Common.ShowErrorMessageBox(3010, ex);
            }
        }

        private void UpdateKeyBindingProfileSequencedKeyStrokesTPM(TextBox textBox)
        {
            try
            {
                if (textBox.Tag == null)
                {
                    textBox.Tag = new SortedList<int, KeyPressInfo>();
                }
                
                if (textBox.Equals(TextBoxG1Off))
                {
                    _tpmPanel.AddOrUpdateSequencedKeyBinding(textBox.Text, TPMPanelSwitches.G1, (SortedList<int, KeyPressInfo>)textBox.Tag, false);
                }
                if (textBox.Equals(TextBoxG1On))
                {
                    _tpmPanel.AddOrUpdateSequencedKeyBinding(textBox.Text, TPMPanelSwitches.G1, (SortedList<int, KeyPressInfo>)textBox.Tag);
                }
                if (textBox.Equals(TextBoxG2Off))
                {
                    _tpmPanel.AddOrUpdateSequencedKeyBinding(textBox.Text, TPMPanelSwitches.G2, (SortedList<int, KeyPressInfo>)textBox.Tag, false);
                }
                if (textBox.Equals(TextBoxG2On))
                {
                    _tpmPanel.AddOrUpdateSequencedKeyBinding(textBox.Text, TPMPanelSwitches.G2, (SortedList<int, KeyPressInfo>)textBox.Tag);
                }
                if (textBox.Equals(TextBoxG3Off))
                {
                    _tpmPanel.AddOrUpdateSequencedKeyBinding(textBox.Text, TPMPanelSwitches.G3, (SortedList<int, KeyPressInfo>)textBox.Tag, false);
                }
                if (textBox.Equals(TextBoxG3On))
                {
                    _tpmPanel.AddOrUpdateSequencedKeyBinding(textBox.Text, TPMPanelSwitches.G3, (SortedList<int, KeyPressInfo>)textBox.Tag);
                }
                if (textBox.Equals(TextBoxG4Off))
                {
                    _tpmPanel.AddOrUpdateSequencedKeyBinding(textBox.Text, TPMPanelSwitches.G4, (SortedList<int, KeyPressInfo>)textBox.Tag, false);
                }
                if (textBox.Equals(TextBoxG4On))
                {
                    _tpmPanel.AddOrUpdateSequencedKeyBinding(textBox.Text, TPMPanelSwitches.G4, (SortedList<int, KeyPressInfo>)textBox.Tag);
                }
                if (textBox.Equals(TextBoxG5Off))
                {
                    _tpmPanel.AddOrUpdateSequencedKeyBinding(textBox.Text, TPMPanelSwitches.G5, (SortedList<int, KeyPressInfo>)textBox.Tag, false);
                }
                if (textBox.Equals(TextBoxG5On))
                {
                    _tpmPanel.AddOrUpdateSequencedKeyBinding(textBox.Text, TPMPanelSwitches.G5, (SortedList<int, KeyPressInfo>)textBox.Tag);
                }
                if (textBox.Equals(TextBoxG6Off))
                {
                    _tpmPanel.AddOrUpdateSequencedKeyBinding(textBox.Text, TPMPanelSwitches.G6, (SortedList<int, KeyPressInfo>)textBox.Tag, false);
                }
                if (textBox.Equals(TextBoxG6On))
                {
                    _tpmPanel.AddOrUpdateSequencedKeyBinding(textBox.Text, TPMPanelSwitches.G6, (SortedList<int, KeyPressInfo>)textBox.Tag);
                }
                if (textBox.Equals(TextBoxG7Off))
                {
                    _tpmPanel.AddOrUpdateSequencedKeyBinding(textBox.Text, TPMPanelSwitches.G7, (SortedList<int, KeyPressInfo>)textBox.Tag, false);
                }
                if (textBox.Equals(TextBoxG7On))
                {
                    _tpmPanel.AddOrUpdateSequencedKeyBinding(textBox.Text, TPMPanelSwitches.G7, (SortedList<int, KeyPressInfo>)textBox.Tag);
                }
                if (textBox.Equals(TextBoxG8Off))
                {
                    _tpmPanel.AddOrUpdateSequencedKeyBinding(textBox.Text, TPMPanelSwitches.G8, (SortedList<int, KeyPressInfo>)textBox.Tag, false);
                }
                if (textBox.Equals(TextBoxG8On))
                {
                    _tpmPanel.AddOrUpdateSequencedKeyBinding(textBox.Text, TPMPanelSwitches.G8, (SortedList<int, KeyPressInfo>)textBox.Tag);
                }
                if (textBox.Equals(TextBoxG9Off))
                {
                    _tpmPanel.AddOrUpdateSequencedKeyBinding(textBox.Text, TPMPanelSwitches.G9, (SortedList<int, KeyPressInfo>)textBox.Tag, false);
                }
                if (textBox.Equals(TextBoxG9On))
                {
                    _tpmPanel.AddOrUpdateSequencedKeyBinding(textBox.Text, TPMPanelSwitches.G9, (SortedList<int, KeyPressInfo>)textBox.Tag);
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
                if (textBox.Equals(TextBoxG1Off))
                {
                    _tpmPanel.AddOrUpdateSingleKeyBinding(TPMPanelSwitches.G1, TextBoxG1Off.Text, keyPressLength, false);
                }
                if (textBox.Equals(TextBoxG1On))
                {
                    _tpmPanel.AddOrUpdateSingleKeyBinding(TPMPanelSwitches.G1, TextBoxG1On.Text, keyPressLength);
                }
                if (textBox.Equals(TextBoxG2Off))
                {
                    _tpmPanel.AddOrUpdateSingleKeyBinding(TPMPanelSwitches.G2, TextBoxG2Off.Text, keyPressLength, false);
                }
                if (textBox.Equals(TextBoxG2On))
                {
                    _tpmPanel.AddOrUpdateSingleKeyBinding(TPMPanelSwitches.G2, TextBoxG2On.Text, keyPressLength);
                }
                if (textBox.Equals(TextBoxG3Off))
                {
                    _tpmPanel.AddOrUpdateSingleKeyBinding(TPMPanelSwitches.G3, TextBoxG3Off.Text, keyPressLength, false);
                }
                if (textBox.Equals(TextBoxG3On))
                {
                    _tpmPanel.AddOrUpdateSingleKeyBinding(TPMPanelSwitches.G3, TextBoxG3On.Text, keyPressLength);
                }
                if (textBox.Equals(TextBoxG4Off))
                {
                    _tpmPanel.AddOrUpdateSingleKeyBinding(TPMPanelSwitches.G4, TextBoxG4Off.Text, keyPressLength, false);
                }
                if (textBox.Equals(TextBoxG4On))
                {
                    _tpmPanel.AddOrUpdateSingleKeyBinding(TPMPanelSwitches.G4, TextBoxG4On.Text, keyPressLength);
                }
                if (textBox.Equals(TextBoxG5Off))
                {
                    _tpmPanel.AddOrUpdateSingleKeyBinding(TPMPanelSwitches.G5, TextBoxG5Off.Text, keyPressLength, false);
                }
                if (textBox.Equals(TextBoxG5On))
                {
                    _tpmPanel.AddOrUpdateSingleKeyBinding(TPMPanelSwitches.G5, TextBoxG5On.Text, keyPressLength);
                }
                if (textBox.Equals(TextBoxG6Off))
                {
                    _tpmPanel.AddOrUpdateSingleKeyBinding(TPMPanelSwitches.G6, TextBoxG6Off.Text, keyPressLength, false);
                }
                if (textBox.Equals(TextBoxG6On))
                {
                    _tpmPanel.AddOrUpdateSingleKeyBinding(TPMPanelSwitches.G6, TextBoxG6On.Text, keyPressLength);
                }
                if (textBox.Equals(TextBoxG7Off))
                {
                    _tpmPanel.AddOrUpdateSingleKeyBinding(TPMPanelSwitches.G7, TextBoxG7Off.Text, keyPressLength, false);
                }
                if (textBox.Equals(TextBoxG7On))
                {
                    _tpmPanel.AddOrUpdateSingleKeyBinding(TPMPanelSwitches.G7, TextBoxG7On.Text, keyPressLength);
                }
                if (textBox.Equals(TextBoxG8Off))
                {
                    _tpmPanel.AddOrUpdateSingleKeyBinding(TPMPanelSwitches.G8, TextBoxG8Off.Text, keyPressLength, false);
                }
                if (textBox.Equals(TextBoxG8On))
                {
                    _tpmPanel.AddOrUpdateSingleKeyBinding(TPMPanelSwitches.G8, TextBoxG8On.Text, keyPressLength);
                }
                if (textBox.Equals(TextBoxG9Off))
                {
                    _tpmPanel.AddOrUpdateSingleKeyBinding(TPMPanelSwitches.G9, TextBoxG9Off.Text, keyPressLength, false);
                }
                if (textBox.Equals(TextBoxG9On))
                {
                    _tpmPanel.AddOrUpdateSingleKeyBinding(TPMPanelSwitches.G9, TextBoxG9On.Text, keyPressLength);
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
                if (textBox.Equals(TextBoxG1Off))
                {
                    _tpmPanel.AddOrUpdateDCSBIOSBinding(TPMPanelSwitches.G1, dcsBiosInputs, textBox.Text, false);
                }
                if (textBox.Equals(TextBoxG1On))
                {
                    _tpmPanel.AddOrUpdateDCSBIOSBinding(TPMPanelSwitches.G1, dcsBiosInputs, textBox.Text);
                }
                if (textBox.Equals(TextBoxG2Off))
                {
                    _tpmPanel.AddOrUpdateDCSBIOSBinding(TPMPanelSwitches.G2, dcsBiosInputs, textBox.Text, false);
                }
                if (textBox.Equals(TextBoxG2On))
                {
                    _tpmPanel.AddOrUpdateDCSBIOSBinding(TPMPanelSwitches.G2, dcsBiosInputs, textBox.Text);
                }
                if (textBox.Equals(TextBoxG3Off))
                {
                    _tpmPanel.AddOrUpdateDCSBIOSBinding(TPMPanelSwitches.G3, dcsBiosInputs, textBox.Text, false);
                }
                if (textBox.Equals(TextBoxG3On))
                {
                    _tpmPanel.AddOrUpdateDCSBIOSBinding(TPMPanelSwitches.G3, dcsBiosInputs, textBox.Text);
                }
                if (textBox.Equals(TextBoxG4Off))
                {
                    _tpmPanel.AddOrUpdateDCSBIOSBinding(TPMPanelSwitches.G4, dcsBiosInputs, textBox.Text, false);
                }
                if (textBox.Equals(TextBoxG4On))
                {
                    _tpmPanel.AddOrUpdateDCSBIOSBinding(TPMPanelSwitches.G4, dcsBiosInputs, textBox.Text);
                }
                if (textBox.Equals(TextBoxG5Off))
                {
                    _tpmPanel.AddOrUpdateDCSBIOSBinding(TPMPanelSwitches.G5, dcsBiosInputs, textBox.Text, false);
                }
                if (textBox.Equals(TextBoxG5On))
                {
                    _tpmPanel.AddOrUpdateDCSBIOSBinding(TPMPanelSwitches.G5, dcsBiosInputs, textBox.Text);
                }
                if (textBox.Equals(TextBoxG6Off))
                {
                    _tpmPanel.AddOrUpdateDCSBIOSBinding(TPMPanelSwitches.G6, dcsBiosInputs, textBox.Text, false);
                }
                if (textBox.Equals(TextBoxG6On))
                {
                    _tpmPanel.AddOrUpdateDCSBIOSBinding(TPMPanelSwitches.G6, dcsBiosInputs, textBox.Text);
                }
                if (textBox.Equals(TextBoxG7Off))
                {
                    _tpmPanel.AddOrUpdateDCSBIOSBinding(TPMPanelSwitches.G7, dcsBiosInputs, textBox.Text, false);
                }
                if (textBox.Equals(TextBoxG7On))
                {
                    _tpmPanel.AddOrUpdateDCSBIOSBinding(TPMPanelSwitches.G7, dcsBiosInputs, textBox.Text);
                }
                if (textBox.Equals(TextBoxG8Off))
                {
                    _tpmPanel.AddOrUpdateDCSBIOSBinding(TPMPanelSwitches.G8, dcsBiosInputs, textBox.Text, false);
                }
                if (textBox.Equals(TextBoxG8On))
                {
                    _tpmPanel.AddOrUpdateDCSBIOSBinding(TPMPanelSwitches.G8, dcsBiosInputs, textBox.Text);
                }
                if (textBox.Equals(TextBoxG9Off))
                {
                    _tpmPanel.AddOrUpdateDCSBIOSBinding(TPMPanelSwitches.G9, dcsBiosInputs, textBox.Text, false);
                }
                if (textBox.Equals(TextBoxG9On))
                {
                    _tpmPanel.AddOrUpdateDCSBIOSBinding(TPMPanelSwitches.G9, dcsBiosInputs, textBox.Text);
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(345012, ex);
            }
        }

        private TPMPanelSwitch.TPMPanelSwitchOnOff GetTPMSwitch(TextBox textBox)
        {
            try
            {
                if (textBox.Equals(TextBoxG1Off))
                {
                    return new TPMPanelSwitch.TPMPanelSwitchOnOff(TPMPanelSwitches.G1, false);
                }
                if (textBox.Equals(TextBoxG1On))
                {
                    return new TPMPanelSwitch.TPMPanelSwitchOnOff(TPMPanelSwitches.G1, true);
                }
                if (textBox.Equals(TextBoxG2Off))
                {
                    return new TPMPanelSwitch.TPMPanelSwitchOnOff(TPMPanelSwitches.G2, false);
                }
                if (textBox.Equals(TextBoxG2On))
                {
                    return new TPMPanelSwitch.TPMPanelSwitchOnOff(TPMPanelSwitches.G2, true);
                }
                if (textBox.Equals(TextBoxG3Off))
                {
                    return new TPMPanelSwitch.TPMPanelSwitchOnOff(TPMPanelSwitches.G3, false);
                }
                if (textBox.Equals(TextBoxG3On))
                {
                    return new TPMPanelSwitch.TPMPanelSwitchOnOff(TPMPanelSwitches.G3, true);
                }
                if (textBox.Equals(TextBoxG4Off))
                {
                    return new TPMPanelSwitch.TPMPanelSwitchOnOff(TPMPanelSwitches.G4, false);
                }
                if (textBox.Equals(TextBoxG4On))
                {
                    return new TPMPanelSwitch.TPMPanelSwitchOnOff(TPMPanelSwitches.G4, true);
                }
                if (textBox.Equals(TextBoxG5Off))
                {
                    return new TPMPanelSwitch.TPMPanelSwitchOnOff(TPMPanelSwitches.G5, false);
                }
                if (textBox.Equals(TextBoxG5On))
                {
                    return new TPMPanelSwitch.TPMPanelSwitchOnOff(TPMPanelSwitches.G5, true);
                }
                if (textBox.Equals(TextBoxG6Off))
                {
                    return new TPMPanelSwitch.TPMPanelSwitchOnOff(TPMPanelSwitches.G6, false);
                }
                if (textBox.Equals(TextBoxG6On))
                {
                    return new TPMPanelSwitch.TPMPanelSwitchOnOff(TPMPanelSwitches.G6, true);
                }
                if (textBox.Equals(TextBoxG7Off))
                {
                    return new TPMPanelSwitch.TPMPanelSwitchOnOff(TPMPanelSwitches.G7, false);
                }
                if (textBox.Equals(TextBoxG7On))
                {
                    return new TPMPanelSwitch.TPMPanelSwitchOnOff(TPMPanelSwitches.G7, true);
                }
                if (textBox.Equals(TextBoxG8Off))
                {
                    return new TPMPanelSwitch.TPMPanelSwitchOnOff(TPMPanelSwitches.G8, false);
                }
                if (textBox.Equals(TextBoxG8On))
                {
                    return new TPMPanelSwitch.TPMPanelSwitchOnOff(TPMPanelSwitches.G8, true);
                }
                if (textBox.Equals(TextBoxG9Off))
                {
                    return new TPMPanelSwitch.TPMPanelSwitchOnOff(TPMPanelSwitches.G9, false);
                }
                if (textBox.Equals(TextBoxG9On))
                {
                    return new TPMPanelSwitch.TPMPanelSwitchOnOff(TPMPanelSwitches.G9, true);
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
                foreach (var keyBinding in _tpmPanel.KeyBindingsHashSet)
                {
                    if (keyBinding.TPMSwitch == TPMPanelSwitches.G1)
                    {
                        if (keyBinding.WhenTurnedOn)
                        {
                            if (keyBinding.OSKeyPress != null && !keyBinding.OSKeyPress.IsMultiSequenced())
                            {
                                TextBoxG1On.Tag = keyBinding.OSKeyPress.LengthOfKeyPress();
                                TextBoxG1On.Text = keyBinding.OSKeyPress.GetSimpleVirtualKeyCodesAsString();
                            }
                            else if (keyBinding.OSKeyPress != null && keyBinding.OSKeyPress.IsMultiSequenced())
                            {
                                TextBoxG1On.Tag = keyBinding.OSKeyPress.GetSequence;
                                TextBoxG1On.Text = keyBinding.OSKeyPress.Information;
                            }
                        }
                        else
                        {
                            if (keyBinding.OSKeyPress != null && !keyBinding.OSKeyPress.IsMultiSequenced())
                            {
                                TextBoxG1Off.Tag = keyBinding.OSKeyPress.LengthOfKeyPress();
                                TextBoxG1Off.Text = keyBinding.OSKeyPress.GetSimpleVirtualKeyCodesAsString();
                            }
                            else if (keyBinding.OSKeyPress != null && keyBinding.OSKeyPress.IsMultiSequenced())
                            {
                                TextBoxG1Off.Tag = keyBinding.OSKeyPress.GetSequence;
                                TextBoxG1Off.Text = keyBinding.OSKeyPress.Information;
                            }
                        }
                    }
                    if (keyBinding.TPMSwitch == TPMPanelSwitches.G2)
                    {
                        if (keyBinding.WhenTurnedOn)
                        {
                            if (keyBinding.OSKeyPress != null && !keyBinding.OSKeyPress.IsMultiSequenced())
                            {
                                TextBoxG2On.Tag = keyBinding.OSKeyPress.LengthOfKeyPress();
                                TextBoxG2On.Text = keyBinding.OSKeyPress.GetSimpleVirtualKeyCodesAsString();
                            }
                            else if (keyBinding.OSKeyPress != null && keyBinding.OSKeyPress.IsMultiSequenced())
                            {
                                TextBoxG2On.Tag = keyBinding.OSKeyPress.GetSequence;
                                TextBoxG2On.Text = keyBinding.OSKeyPress.Information;
                            }
                        }
                        else
                        {
                            if (keyBinding.OSKeyPress != null && !keyBinding.OSKeyPress.IsMultiSequenced())
                            {
                                TextBoxG2Off.Tag = keyBinding.OSKeyPress.LengthOfKeyPress();
                                TextBoxG2Off.Text = keyBinding.OSKeyPress.GetSimpleVirtualKeyCodesAsString();
                            }
                            else if (keyBinding.OSKeyPress != null && keyBinding.OSKeyPress.IsMultiSequenced())
                            {
                                TextBoxG2Off.Tag = keyBinding.OSKeyPress.GetSequence;
                                TextBoxG2Off.Text = keyBinding.OSKeyPress.Information;
                            }
                        }
                    }
                    if (keyBinding.TPMSwitch == TPMPanelSwitches.G3)
                    {
                        if (keyBinding.WhenTurnedOn)
                        {
                            if (keyBinding.OSKeyPress != null && !keyBinding.OSKeyPress.IsMultiSequenced())
                            {
                                TextBoxG3On.Tag = keyBinding.OSKeyPress.LengthOfKeyPress();
                                TextBoxG3On.Text = keyBinding.OSKeyPress.GetSimpleVirtualKeyCodesAsString();
                            }
                            else if (keyBinding.OSKeyPress != null && keyBinding.OSKeyPress.IsMultiSequenced())
                            {
                                TextBoxG3On.Tag = keyBinding.OSKeyPress.GetSequence;
                                TextBoxG3On.Text = keyBinding.OSKeyPress.Information;
                            }
                        }
                        else
                        {
                            if (keyBinding.OSKeyPress != null && !keyBinding.OSKeyPress.IsMultiSequenced())
                            {
                                TextBoxG3Off.Tag = keyBinding.OSKeyPress.LengthOfKeyPress();
                                TextBoxG3Off.Text = keyBinding.OSKeyPress.GetSimpleVirtualKeyCodesAsString();
                            }
                            else if (keyBinding.OSKeyPress != null && keyBinding.OSKeyPress.IsMultiSequenced())
                            {
                                TextBoxG3Off.Tag = keyBinding.OSKeyPress.GetSequence;
                                TextBoxG3Off.Text = keyBinding.OSKeyPress.Information;
                            }
                        }
                    }
                    if (keyBinding.TPMSwitch == TPMPanelSwitches.G4)
                    {
                        if (keyBinding.WhenTurnedOn)
                        {
                            if (keyBinding.OSKeyPress != null && !keyBinding.OSKeyPress.IsMultiSequenced())
                            {
                                TextBoxG4On.Tag = keyBinding.OSKeyPress.LengthOfKeyPress();
                                TextBoxG4On.Text = keyBinding.OSKeyPress.GetSimpleVirtualKeyCodesAsString();
                            }
                            else if (keyBinding.OSKeyPress != null && keyBinding.OSKeyPress.IsMultiSequenced())
                            {
                                TextBoxG4On.Tag = keyBinding.OSKeyPress.GetSequence;
                                TextBoxG4On.Text = keyBinding.OSKeyPress.Information;
                            }
                        }
                        else
                        {
                            if (keyBinding.OSKeyPress != null && !keyBinding.OSKeyPress.IsMultiSequenced())
                            {
                                TextBoxG4Off.Tag = keyBinding.OSKeyPress.LengthOfKeyPress();
                                TextBoxG4Off.Text = keyBinding.OSKeyPress.GetSimpleVirtualKeyCodesAsString();
                            }
                            else if (keyBinding.OSKeyPress != null && keyBinding.OSKeyPress.IsMultiSequenced())
                            {
                                TextBoxG4Off.Tag = keyBinding.OSKeyPress.GetSequence;
                                TextBoxG4Off.Text = keyBinding.OSKeyPress.Information;
                            }
                        }
                    }
                    if (keyBinding.TPMSwitch == TPMPanelSwitches.G5)
                    {
                        if (keyBinding.WhenTurnedOn)
                        {
                            if (keyBinding.OSKeyPress != null && !keyBinding.OSKeyPress.IsMultiSequenced())
                            {
                                TextBoxG5On.Tag = keyBinding.OSKeyPress.LengthOfKeyPress();
                                TextBoxG5On.Text = keyBinding.OSKeyPress.GetSimpleVirtualKeyCodesAsString();
                            }
                            else if (keyBinding.OSKeyPress != null && keyBinding.OSKeyPress.IsMultiSequenced())
                            {
                                TextBoxG5On.Tag = keyBinding.OSKeyPress.GetSequence;
                                TextBoxG5On.Text = keyBinding.OSKeyPress.Information;
                            }
                        }
                        else
                        {
                            if (keyBinding.OSKeyPress != null && !keyBinding.OSKeyPress.IsMultiSequenced())
                            {
                                TextBoxG5Off.Tag = keyBinding.OSKeyPress.LengthOfKeyPress();
                                TextBoxG5Off.Text = keyBinding.OSKeyPress.GetSimpleVirtualKeyCodesAsString();
                            }
                            else if (keyBinding.OSKeyPress != null && keyBinding.OSKeyPress.IsMultiSequenced())
                            {
                                TextBoxG5Off.Tag = keyBinding.OSKeyPress.GetSequence;
                                TextBoxG5Off.Text = keyBinding.OSKeyPress.Information;
                            }
                        }
                    }
                    if (keyBinding.TPMSwitch == TPMPanelSwitches.G6)
                    {
                        if (keyBinding.WhenTurnedOn)
                        {
                            if (keyBinding.OSKeyPress != null && !keyBinding.OSKeyPress.IsMultiSequenced())
                            {
                                TextBoxG6On.Tag = keyBinding.OSKeyPress.LengthOfKeyPress();
                                TextBoxG6On.Text = keyBinding.OSKeyPress.GetSimpleVirtualKeyCodesAsString();
                            }
                            else if (keyBinding.OSKeyPress != null && keyBinding.OSKeyPress.IsMultiSequenced())
                            {
                                TextBoxG6On.Tag = keyBinding.OSKeyPress.GetSequence;
                                TextBoxG6On.Text = keyBinding.OSKeyPress.Information;
                            }
                        }
                        else
                        {
                            if (keyBinding.OSKeyPress != null && !keyBinding.OSKeyPress.IsMultiSequenced())
                            {
                                TextBoxG6Off.Tag = keyBinding.OSKeyPress.LengthOfKeyPress();
                                TextBoxG6Off.Text = keyBinding.OSKeyPress.GetSimpleVirtualKeyCodesAsString();
                            }
                            else if (keyBinding.OSKeyPress != null && keyBinding.OSKeyPress.IsMultiSequenced())
                            {
                                TextBoxG6Off.Tag = keyBinding.OSKeyPress.GetSequence;
                                TextBoxG6Off.Text = keyBinding.OSKeyPress.Information;
                            }
                        }
                    }
                    if (keyBinding.TPMSwitch == TPMPanelSwitches.G7)
                    {
                        if (keyBinding.WhenTurnedOn)
                        {
                            if (keyBinding.OSKeyPress != null && !keyBinding.OSKeyPress.IsMultiSequenced())
                            {
                                TextBoxG7On.Tag = keyBinding.OSKeyPress.LengthOfKeyPress();
                                TextBoxG7On.Text = keyBinding.OSKeyPress.GetSimpleVirtualKeyCodesAsString();
                            }
                            else if (keyBinding.OSKeyPress != null && keyBinding.OSKeyPress.IsMultiSequenced())
                            {
                                TextBoxG7On.Tag = keyBinding.OSKeyPress.GetSequence;
                                TextBoxG7On.Text = keyBinding.OSKeyPress.Information;
                            }
                        }
                        else
                        {
                            if (keyBinding.OSKeyPress != null && !keyBinding.OSKeyPress.IsMultiSequenced())
                            {
                                TextBoxG7Off.Tag = keyBinding.OSKeyPress.LengthOfKeyPress();
                                TextBoxG7Off.Text = keyBinding.OSKeyPress.GetSimpleVirtualKeyCodesAsString();
                            }
                            else if (keyBinding.OSKeyPress != null && keyBinding.OSKeyPress.IsMultiSequenced())
                            {
                                TextBoxG7Off.Tag = keyBinding.OSKeyPress.GetSequence;
                                TextBoxG7Off.Text = keyBinding.OSKeyPress.Information;
                            }
                        }
                    }
                    if (keyBinding.TPMSwitch == TPMPanelSwitches.G8)
                    {
                        if (keyBinding.WhenTurnedOn)
                        {
                            if (keyBinding.OSKeyPress != null && !keyBinding.OSKeyPress.IsMultiSequenced())
                            {
                                TextBoxG8On.Tag = keyBinding.OSKeyPress.LengthOfKeyPress();
                                TextBoxG8On.Text = keyBinding.OSKeyPress.GetSimpleVirtualKeyCodesAsString();
                            }
                            else if (keyBinding.OSKeyPress != null && keyBinding.OSKeyPress.IsMultiSequenced())
                            {
                                TextBoxG8On.Tag = keyBinding.OSKeyPress.GetSequence;
                                TextBoxG8On.Text = keyBinding.OSKeyPress.Information;
                            }
                        }
                        else
                        {
                            if (keyBinding.OSKeyPress != null && !keyBinding.OSKeyPress.IsMultiSequenced())
                            {
                                TextBoxG8Off.Tag = keyBinding.OSKeyPress.LengthOfKeyPress();
                                TextBoxG8Off.Text = keyBinding.OSKeyPress.GetSimpleVirtualKeyCodesAsString();
                            }
                            else if (keyBinding.OSKeyPress != null && keyBinding.OSKeyPress.IsMultiSequenced())
                            {
                                TextBoxG8Off.Tag = keyBinding.OSKeyPress.GetSequence;
                                TextBoxG8Off.Text = keyBinding.OSKeyPress.Information;
                            }
                        }
                    }
                    if (keyBinding.TPMSwitch == TPMPanelSwitches.G9)
                    {
                        if (keyBinding.WhenTurnedOn)
                        {
                            if (keyBinding.OSKeyPress != null && !keyBinding.OSKeyPress.IsMultiSequenced())
                            {
                                TextBoxG9On.Tag = keyBinding.OSKeyPress.LengthOfKeyPress();
                                TextBoxG9On.Text = keyBinding.OSKeyPress.GetSimpleVirtualKeyCodesAsString();
                            }
                            else if (keyBinding.OSKeyPress != null && keyBinding.OSKeyPress.IsMultiSequenced())
                            {
                                TextBoxG9On.Tag = keyBinding.OSKeyPress.GetSequence;
                                TextBoxG9On.Text = keyBinding.OSKeyPress.Information;
                            }
                        }
                        else
                        {
                            if (keyBinding.OSKeyPress != null && !keyBinding.OSKeyPress.IsMultiSequenced())
                            {
                                TextBoxG9Off.Tag = keyBinding.OSKeyPress.LengthOfKeyPress();
                                TextBoxG9Off.Text = keyBinding.OSKeyPress.GetSimpleVirtualKeyCodesAsString();
                            }
                            else if (keyBinding.OSKeyPress != null && keyBinding.OSKeyPress.IsMultiSequenced())
                            {
                                TextBoxG9Off.Tag = keyBinding.OSKeyPress.GetSequence;
                                TextBoxG9Off.Text = keyBinding.OSKeyPress.Information;
                            }
                        }
                    }
                }





                foreach (var dcsBiosBinding in _tpmPanel.DCSBiosBindings)
                {
                    if (dcsBiosBinding.TPMSwitch == TPMPanelSwitches.G1)
                    {
                        if (dcsBiosBinding.WhenTurnedOn)
                        {
                            if (dcsBiosBinding.DCSBIOSInputs.Count > 0)
                            {
                                TextBoxG1On.Tag = dcsBiosBinding.DCSBIOSInputs;
                                TextBoxG1On.Text = dcsBiosBinding.Description;
                                TextBoxG1On.ToolTip = "DCS-BIOS";
                            }
                        }
                        else
                        {
                            if (dcsBiosBinding.DCSBIOSInputs.Count > 0)
                            {
                                TextBoxG1Off.Tag = dcsBiosBinding.DCSBIOSInputs;
                                TextBoxG1Off.Text = dcsBiosBinding.Description;
                                TextBoxG1Off.ToolTip = "DCS-BIOS";
                            }
                        }
                    }
                    if (dcsBiosBinding.TPMSwitch == TPMPanelSwitches.G2)
                    {
                        if (dcsBiosBinding.WhenTurnedOn)
                        {
                            if (dcsBiosBinding.DCSBIOSInputs.Count > 0)
                            {
                                TextBoxG2On.Tag = dcsBiosBinding.DCSBIOSInputs;
                                TextBoxG2On.Text = dcsBiosBinding.Description;
                            }
                        }
                        else
                        {
                            if (dcsBiosBinding.DCSBIOSInputs.Count > 0)
                            {
                                TextBoxG2Off.Tag = dcsBiosBinding.DCSBIOSInputs;
                                TextBoxG2Off.Text = dcsBiosBinding.Description;
                                TextBoxG2Off.ToolTip = "DCS-BIOS";
                            }
                        }
                    }
                    if (dcsBiosBinding.TPMSwitch == TPMPanelSwitches.G3)
                    {
                        if (dcsBiosBinding.WhenTurnedOn)
                        {
                            if (dcsBiosBinding.DCSBIOSInputs.Count > 0)
                            {
                                TextBoxG3On.Tag = dcsBiosBinding.DCSBIOSInputs;
                                TextBoxG3On.Text = dcsBiosBinding.Description;
                                TextBoxG3On.ToolTip = "DCS-BIOS";
                            }
                        }
                        else
                        {
                            if (dcsBiosBinding.DCSBIOSInputs.Count > 0)
                            {
                                TextBoxG3Off.Tag = dcsBiosBinding.DCSBIOSInputs;
                                TextBoxG3Off.Text = dcsBiosBinding.Description;
                                TextBoxG3Off.ToolTip = "DCS-BIOS";
                            }
                        }
                    }
                    if (dcsBiosBinding.TPMSwitch == TPMPanelSwitches.G4)
                    {
                        if (dcsBiosBinding.WhenTurnedOn)
                        {
                            if (dcsBiosBinding.DCSBIOSInputs.Count > 0)
                            {
                                TextBoxG4On.Tag = dcsBiosBinding.DCSBIOSInputs;
                                TextBoxG4On.Text = dcsBiosBinding.Description;
                                TextBoxG4On.ToolTip = "DCS-BIOS";
                            }
                        }
                        else
                        {
                            if (dcsBiosBinding.DCSBIOSInputs.Count > 0)
                            {
                                TextBoxG4Off.Tag = dcsBiosBinding.DCSBIOSInputs;
                                TextBoxG4Off.Text = dcsBiosBinding.Description;
                                TextBoxG4Off.ToolTip = "DCS-BIOS";
                            }
                        }
                    }
                    if (dcsBiosBinding.TPMSwitch == TPMPanelSwitches.G5)
                    {
                        if (dcsBiosBinding.WhenTurnedOn)
                        {
                            if (dcsBiosBinding.DCSBIOSInputs.Count > 0)
                            {
                                TextBoxG5On.Tag = dcsBiosBinding.DCSBIOSInputs;
                                TextBoxG5On.Text = dcsBiosBinding.Description;
                                TextBoxG5On.ToolTip = "DCS-BIOS";
                            }
                        }
                        else
                        {
                            if (dcsBiosBinding.DCSBIOSInputs.Count > 0)
                            {
                                TextBoxG5Off.Tag = dcsBiosBinding.DCSBIOSInputs;
                                TextBoxG5Off.Text = dcsBiosBinding.Description;
                                TextBoxG5Off.ToolTip = "DCS-BIOS";
                            }
                        }
                    }
                    if (dcsBiosBinding.TPMSwitch == TPMPanelSwitches.G6)
                    {
                        if (dcsBiosBinding.WhenTurnedOn)
                        {
                            if (dcsBiosBinding.DCSBIOSInputs.Count > 0)
                            {
                                TextBoxG6On.Tag = dcsBiosBinding.DCSBIOSInputs;
                                TextBoxG6On.Text = dcsBiosBinding.Description;
                                TextBoxG6On.ToolTip = "DCS-BIOS";
                            }
                        }
                        else
                        {
                            if (dcsBiosBinding.DCSBIOSInputs.Count > 0)
                            {
                                TextBoxG6Off.Tag = dcsBiosBinding.DCSBIOSInputs;
                                TextBoxG6Off.Text = dcsBiosBinding.Description;
                                TextBoxG6Off.ToolTip = "DCS-BIOS";
                            }
                        }
                    }
                    if (dcsBiosBinding.TPMSwitch == TPMPanelSwitches.G7)
                    {
                        if (dcsBiosBinding.WhenTurnedOn)
                        {
                            if (dcsBiosBinding.DCSBIOSInputs.Count > 0)
                            {
                                TextBoxG7On.Tag = dcsBiosBinding.DCSBIOSInputs;
                                TextBoxG7On.Text = dcsBiosBinding.Description;
                                TextBoxG7On.ToolTip = "DCS-BIOS";
                            }
                        }
                        else
                        {
                            if (dcsBiosBinding.DCSBIOSInputs.Count > 0)
                            {
                                TextBoxG7Off.Tag = dcsBiosBinding.DCSBIOSInputs;
                                TextBoxG7Off.Text = dcsBiosBinding.Description;
                                TextBoxG7Off.ToolTip = "DCS-BIOS";
                            }
                        }
                    }
                    if (dcsBiosBinding.TPMSwitch == TPMPanelSwitches.G8)
                    {
                        if (dcsBiosBinding.WhenTurnedOn)
                        {
                            if (dcsBiosBinding.DCSBIOSInputs.Count > 0)
                            {
                                TextBoxG8On.Tag = dcsBiosBinding.DCSBIOSInputs;
                                TextBoxG8On.Text = dcsBiosBinding.Description;
                                TextBoxG8On.ToolTip = "DCS-BIOS";
                            }
                        }
                        else
                        {
                            if (dcsBiosBinding.DCSBIOSInputs.Count > 0)
                            {
                                TextBoxG8Off.Tag = dcsBiosBinding.DCSBIOSInputs;
                                TextBoxG8Off.Text = dcsBiosBinding.Description;
                                TextBoxG8Off.ToolTip = "DCS-BIOS";
                            }
                        }
                    }
                    if (dcsBiosBinding.TPMSwitch == TPMPanelSwitches.G9)
                    {
                        if (dcsBiosBinding.WhenTurnedOn)
                        {
                            if (dcsBiosBinding.DCSBIOSInputs.Count > 0)
                            {
                                TextBoxG9On.Tag = dcsBiosBinding.DCSBIOSInputs;
                                TextBoxG9On.Text = dcsBiosBinding.Description;
                                TextBoxG9On.ToolTip = "DCS-BIOS";
                            }
                        }
                        else
                        {
                            if (dcsBiosBinding.DCSBIOSInputs.Count > 0)
                            {
                                TextBoxG9Off.Tag = dcsBiosBinding.DCSBIOSInputs;
                                TextBoxG9Off.Text = dcsBiosBinding.Description;
                                TextBoxG9Off.ToolTip = "DCS-BIOS";
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
    
        private void ButtonGetId_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_tpmPanel != null)
                {
                    Clipboard.SetText(_tpmPanel.InstanceId);
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


