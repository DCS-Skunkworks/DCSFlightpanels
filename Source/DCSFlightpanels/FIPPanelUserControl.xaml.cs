using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using DCS_BIOS;
using NonVisuals;

namespace DCSFlightpanels
{
    /// <summary>
    /// Interaction logic for FIPPanelUserControl.xaml
    /// </summary>
    public partial class FIPPanelUserControl : ISaitekPanelListener, IProfileHandlerListener, ISaitekUserControl
    {
        private readonly FIPHandler _fipHandler;
        private TabItem _parentTabItem;
        private string _parentTabItemHeader;
        private IGlobalHandler _globalHandler;

        public FIPPanelUserControl(FIPHandler fipHandler, TabItem parentTabItem, IGlobalHandler globalHandler)
        {
            InitializeComponent();
            _parentTabItem = parentTabItem;
            _parentTabItemHeader = _parentTabItem.Header.ToString();
            _fipHandler = fipHandler;
            _fipHandler.OnFIPCountChanged += FIPCountChanged;
            _globalHandler = globalHandler;
            foreach (var fipPanel in _fipHandler.FIPPanels)
            {
                fipPanel.Attach((ISaitekPanelListener)this);
                _globalHandler.Attach(fipPanel);
            }
            /*
            InitializeComponent();
            
            InitializeComponent();
            _parentTabItem = parentTabItem;
            _parentTabItemHeader = _parentTabItem.Header.ToString();
            _fipPanel = fipPanel;

            _fipPanel.Attach((ISaitekPanelListener)this);
            globalHandler.Attach(_fipPanel);
            _globalHandler = globalHandler;*/

        }

        public void FIPCountChanged(int numberFIPsConnected)
        {
            try
            {
                ShowGraphicConfiguration();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(871373, ex);
            }
        }

        private void FIPPanelUserControl_OnLoaded(object sender, RoutedEventArgs e)
        {
            SetContextMenuClickHandlers();
        }

        public SaitekPanel GetSaitekPanel()
        {
            if (_fipHandler.FIPPanels.Count > 0)
            {
                return _fipHandler.FIPPanels[0];
            }
            return null;
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
                    if (image.Name.StartsWith("ImagePZ55LED") && dcsAirframe == DCSAirframe.NONE)
                    {
                        image.ContextMenu = null;
                    }
                    else
                        if (image.Name.StartsWith("ImagePZ55LED") && image.ContextMenu == null && dcsAirframe != DCSAirframe.NONE)
                    {
                        image.ContextMenu = (ContextMenu)Resources["PZ55LEDContextMenu"];
                        image.ContextMenu.Tag = image.Name;
                    }
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(471373, ex);
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
                /*if (saitekPanelsEnum == SaitekPanelsEnum.PZ55SwitchPanel && uniqueId.Equals(_fipPanel.InstanceId))
                {
                    NotifySwitchChanges(hashSet);
                }*/
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
                /*if (saitekPanelsEnum == SaitekPanelsEnum.PZ55SwitchPanel && uniqueId.Equals(_fipPanel.InstanceId))
                {
                    //Dispatcher.BeginInvoke((Action)(() => _parentTabItem.Header = _parentTabItemHeader + " (connected)"));
                }*/
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
                /*if (saitekPanelsEnum == SaitekPanelsEnum.PZ55SwitchPanel && uniqueId.Equals(_fipPanel.InstanceId))
                {
                    //Dispatcher.BeginInvoke((Action)(() => _parentTabItem.Header = _parentTabItemHeader + " (disconnected)"));
                }*/
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
                //uniqueId not used for FIPs
                if (saitekPanelsEnum == SaitekPanelsEnum.FIP)
                {
                    Dispatcher.BeginInvoke((Action)(ShowGraphicConfiguration));
                    Dispatcher.BeginInvoke((Action)(() => TextBoxLogFIP.Text = ""));
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
                if (saitekPanelsEnum == SaitekPanelsEnum.FIP)
                {
                    Dispatcher.BeginInvoke((Action)(ShowGraphicConfiguration));
                }
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
                TextBoxLogFIP.Focus();
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
                        UpdateKeyBindingProfileSequencedKeyStrokesFIP(textBox);
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
                //_fipPanel.ClearSettings();
            }
        }

        private void SetContextMenuClickHandlers()
        {
            foreach (var textBox in Common.FindVisualChildren<TextBox>(this))
            {
                if (!textBox.Equals(TextBoxLogFIP))
                {
                    textBox.ContextMenu = (ContextMenu)Resources["TextBoxContextMenuFIP"];
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
                        /*
                        if (item.Name.Contains("EditSequence"))
                        {
                            item.Visibility = Visibility.Visible;
                        }
                        else if (!_fipPanel.KeyboardEmulationOnly && item.Name.Contains("EditDCSBIOS"))
                        {
                            item.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            item.Visibility = Visibility.Collapsed;
                        }*/
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
                if (textBox != TextBoxLogFIP && textBox.IsFocused && textBox.Background == Brushes.Yellow)
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
                        //_fipPanel.ClearAllBindings(GetFIPButton(textBox));
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
                UpdateKeyBindingProfileSequencedKeyStrokesFIP(textBox);
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(3008, ex);
            }
        }

        private void NotifyButtonChanges(HashSet<object> buttons)
        {
            try
            {
                //Set focus to this so that virtual keypresses won't affect settings
                Dispatcher.BeginInvoke((Action)(() => TextBoxLogFIP.Focus()));
                foreach (var fipPanelButton in buttons)
                {
                    var key = (FIPPanelButtons)fipPanelButton;

                    /*if (_fipPanel.ForwardKeyPresses)
                    {
                        if (!string.IsNullOrEmpty(_fipPanel.GetKeyPressForLoggingPurposes(key)))
                        {
                            Dispatcher.BeginInvoke(
                                (Action)
                                (() =>
                                 TextBoxLogFIP.Text =
                                 TextBoxLogFIP.Text.Insert(0, _fipPanel.GetKeyPressForLoggingPurposes(key) + "\n")));
                        }
                    }
                    else
                    {
                        Dispatcher.BeginInvoke(
                            (Action)
                            (() =>
                             TextBoxLogFIP.Text =
                             TextBoxLogFIP.Text = TextBoxLogFIP.Text.Insert(0, "No action taken, virtual key press disabled.\n")));
                    }*/
                }
                SetGraphicsState(buttons);
            }
            catch (Exception ex)
            {
                Dispatcher.BeginInvoke(
                    (Action)
                    (() =>
                     TextBoxLogFIP.Text = TextBoxLogFIP.Text.Insert(0, "0x16" + ex.Message + ".\n")));
                Common.ShowErrorMessageBox(3009, ex);
            }
        }

        private void SetGraphicsState(HashSet<object> switches)
        {
            try
            {
                foreach (var switchPanelKeyObject in switches)
                {
                    var switchPanelKey = (SwitchPanelKey)switchPanelKeyObject;
                    switch (switchPanelKey.SwitchPanelPZ55Key)
                    {
                        case SwitchPanelPZ55Keys.SWITCHKEY_AVIONICS_MASTER:
                            {
                                var key = switchPanelKey;
                                Dispatcher.BeginInvoke(
                                    (Action)delegate
                                    {
                                        //ImageAvMasterOn.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case SwitchPanelPZ55Keys.SWITCHKEY_CLOSE_COWL:
                            {
                                var key = switchPanelKey;
                                Dispatcher.BeginInvoke(
                                    (Action)delegate
                                    {
                                        //This button is special. The Panel reports the button ON when it us switched upwards towards [CLOSE]. This is confusing semantics.
                                        //The button is considered OFF by the program when it is upwards which is opposite to the other buttons which all are considered ON when upwards.
                                        //ImageCowlClosed.Visibility = !key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case SwitchPanelPZ55Keys.SWITCHKEY_DE_ICE:
                            {
                                var key = switchPanelKey;
                                Dispatcher.BeginInvoke(
                                    (Action)delegate
                                    {
                                        //ImageDeIceOn.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case SwitchPanelPZ55Keys.SWITCHKEY_FUEL_PUMP:
                            {
                                var key = switchPanelKey;
                                Dispatcher.BeginInvoke(
                                    (Action)delegate
                                    {
                                        //ImageFuelPumpOn.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case SwitchPanelPZ55Keys.LEVER_GEAR_DOWN:
                            {
                                var key = switchPanelKey;
                                Dispatcher.BeginInvoke(
                                    (Action)delegate
                                    {
                                        //ImageGearUp.Visibility = key.IsOn ? Visibility.Collapsed : Visibility.Visible;
                                    });
                                break;
                            }
                        case SwitchPanelPZ55Keys.LEVER_GEAR_UP:
                            {
                                var key = switchPanelKey;
                                Dispatcher.BeginInvoke(
                                    (Action)delegate
                                    {
                                        //ImageGearUp.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_BEACON:
                            {
                                var key = switchPanelKey;
                                Dispatcher.BeginInvoke(
                                    (Action)delegate
                                    {
                                        //ImageBeaconOn.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_LANDING:
                            {
                                var key = switchPanelKey;
                                Dispatcher.BeginInvoke(
                                    (Action)delegate
                                    {
                                        //ImageLandingOn.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_NAV:
                            {
                                var key = switchPanelKey;
                                Dispatcher.BeginInvoke(
                                    (Action)delegate
                                    {
                                        //ImageNavOn.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_PANEL:
                            {
                                var key = switchPanelKey;
                                Dispatcher.BeginInvoke(
                                    (Action)delegate
                                    {
                                        //ImagePanelOn.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_STROBE:
                            {
                                var key = switchPanelKey;
                                Dispatcher.BeginInvoke(
                                    (Action)delegate
                                    {
                                        //ImageStrobeOn.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_TAXI:
                            {
                                var key = switchPanelKey;
                                Dispatcher.BeginInvoke(
                                    (Action)delegate
                                    {
                                        //ImageTaxiOn.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case SwitchPanelPZ55Keys.SWITCHKEY_MASTER_ALT:
                            {
                                var key = switchPanelKey;
                                Dispatcher.BeginInvoke(
                                    (Action)delegate
                                    {
                                        //ImageMasterAltOn.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case SwitchPanelPZ55Keys.SWITCHKEY_MASTER_BAT:
                            {
                                var key = switchPanelKey;
                                Dispatcher.BeginInvoke(
                                    (Action)delegate
                                    {
                                        //ImageMasterBatOn.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case SwitchPanelPZ55Keys.SWITCHKEY_PITOT_HEAT:
                            {
                                var key = switchPanelKey;
                                Dispatcher.BeginInvoke(
                                    (Action)delegate
                                    {
                                        //ImagePitotHeatOn.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                    }
                    if (switchPanelKey.SwitchPanelPZ55Key == SwitchPanelPZ55Keys.KNOB_ENGINE_BOTH || switchPanelKey.SwitchPanelPZ55Key == SwitchPanelPZ55Keys.KNOB_ENGINE_LEFT ||
                        switchPanelKey.SwitchPanelPZ55Key == SwitchPanelPZ55Keys.KNOB_ENGINE_RIGHT || switchPanelKey.SwitchPanelPZ55Key == SwitchPanelPZ55Keys.KNOB_ENGINE_OFF ||
                        switchPanelKey.SwitchPanelPZ55Key == SwitchPanelPZ55Keys.KNOB_ENGINE_START)
                    {
                        var key = switchPanelKey;
                        Dispatcher.BeginInvoke(
                            (Action)delegate
                            {
                                if (key.IsOn)
                                {
                                }
                            });
                    }
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(3010, ex);
            }
        }

        private void NotifySwitchChanges(HashSet<object> buttons)
        {
            try
            {
                //Set focus to this so that virtual keypresses won't affect settings
                Dispatcher.BeginInvoke((Action)(() => TextBoxLogFIP.Focus()));
                foreach (var fipButton in buttons)
                {
                    var key = (FIPPanelButtons)fipButton;

                    /*if (_fipPanel.ForwardKeyPresses)
                    {
                        if (!string.IsNullOrEmpty(_fipPanel.GetKeyPressForLoggingPurposes(key)))
                        {
                            Dispatcher.BeginInvoke(
                                (Action)
                                (() =>
                                 TextBoxLogFIP.Text =
                                 TextBoxLogFIP.Text.Insert(0, _fipPanel.GetKeyPressForLoggingPurposes(key) + "\n")));
                        }
                    }
                    else
                    {
                        Dispatcher.BeginInvoke(
                            (Action)
                            (() =>
                             TextBoxLogFIP.Text =
                             TextBoxLogFIP.Text = TextBoxLogFIP.Text.Insert(0, "No action taken, virtual key press disabled.\n")));
                    }*/
                }
                SetGraphicsState(buttons);
            }
            catch (Exception ex)
            {
                Dispatcher.BeginInvoke(
                    (Action)
                    (() =>
                     TextBoxLogFIP.Text = TextBoxLogFIP.Text.Insert(0, "0x16" + ex.Message + ".\n")));
                Common.ShowErrorMessageBox(3009, ex);
            }
        }

        private void UpdateKeyBindingProfileSequencedKeyStrokesFIP(TextBox textBox)
        {
            try
            {
                if (textBox.Tag == null)
                {
                    textBox.Tag = new SortedList<int, KeyPressInfo>();
                }

                /*
                if (textBox.Equals(TextBoxPage1Button1))
                {
                    _fipPanel.AddOrUpdateSequencedKeyBinding(textBox.Text, FIPPanelButtons.SOFTBUTTON_1_P1, (SortedList<int, KeyPressInfo>)textBox.Tag);
                }
                if (textBox.Equals(TextBoxPage1Button2))
                {
                    _fipPanel.AddOrUpdateSequencedKeyBinding(textBox.Text, FIPPanelButtons.SOFTBUTTON_2_P1, (SortedList<int, KeyPressInfo>)textBox.Tag);
                }
                if (textBox.Equals(TextBoxPage1Button3))
                {
                    _fipPanel.AddOrUpdateSequencedKeyBinding(textBox.Text, FIPPanelButtons.SOFTBUTTON_3_P1, (SortedList<int, KeyPressInfo>)textBox.Tag);
                }
                if (textBox.Equals(TextBoxPage1Button4))
                {
                    _fipPanel.AddOrUpdateSequencedKeyBinding(textBox.Text, FIPPanelButtons.SOFTBUTTON_4_P1, (SortedList<int, KeyPressInfo>)textBox.Tag);
                }
                if (textBox.Equals(TextBoxPage1Button5))
                {
                    _fipPanel.AddOrUpdateSequencedKeyBinding(textBox.Text, FIPPanelButtons.SOFTBUTTON_5_P1, (SortedList<int, KeyPressInfo>)textBox.Tag);
                }
                if (textBox.Equals(TextBoxPage1Button6))
                {
                    _fipPanel.AddOrUpdateSequencedKeyBinding(textBox.Text, FIPPanelButtons.SOFTBUTTON_6_P1, (SortedList<int, KeyPressInfo>)textBox.Tag);
                }
                if (textBox.Equals(TextBoxPage2Button1))
                {
                    _fipPanel.AddOrUpdateSequencedKeyBinding(textBox.Text, FIPPanelButtons.SOFTBUTTON_1_P2, (SortedList<int, KeyPressInfo>)textBox.Tag);
                }
                if (textBox.Equals(TextBoxPage2Button2))
                {
                    _fipPanel.AddOrUpdateSequencedKeyBinding(textBox.Text, FIPPanelButtons.SOFTBUTTON_2_P2, (SortedList<int, KeyPressInfo>)textBox.Tag);
                }
                if (textBox.Equals(TextBoxPage2Button3))
                {
                    _fipPanel.AddOrUpdateSequencedKeyBinding(textBox.Text, FIPPanelButtons.SOFTBUTTON_3_P2, (SortedList<int, KeyPressInfo>)textBox.Tag);
                }
                if (textBox.Equals(TextBoxPage2Button4))
                {
                    _fipPanel.AddOrUpdateSequencedKeyBinding(textBox.Text, FIPPanelButtons.SOFTBUTTON_4_P2, (SortedList<int, KeyPressInfo>)textBox.Tag);
                }
                if (textBox.Equals(TextBoxPage2Button5))
                {
                    _fipPanel.AddOrUpdateSequencedKeyBinding(textBox.Text, FIPPanelButtons.SOFTBUTTON_5_P2, (SortedList<int, KeyPressInfo>)textBox.Tag);
                }
                if (textBox.Equals(TextBoxPage2Button6))
                {
                    _fipPanel.AddOrUpdateSequencedKeyBinding(textBox.Text, FIPPanelButtons.SOFTBUTTON_6_P2, (SortedList<int, KeyPressInfo>)textBox.Tag);
                }
                if (textBox.Equals(TextBoxPage3Button1))
                {
                    _fipPanel.AddOrUpdateSequencedKeyBinding(textBox.Text, FIPPanelButtons.SOFTBUTTON_1_P3, (SortedList<int, KeyPressInfo>)textBox.Tag);
                }
                if (textBox.Equals(TextBoxPage3Button2))
                {
                    _fipPanel.AddOrUpdateSequencedKeyBinding(textBox.Text, FIPPanelButtons.SOFTBUTTON_2_P3, (SortedList<int, KeyPressInfo>)textBox.Tag);
                }
                if (textBox.Equals(TextBoxPage3Button3))
                {
                    _fipPanel.AddOrUpdateSequencedKeyBinding(textBox.Text, FIPPanelButtons.SOFTBUTTON_3_P3, (SortedList<int, KeyPressInfo>)textBox.Tag);
                }
                if (textBox.Equals(TextBoxPage3Button4))
                {
                    _fipPanel.AddOrUpdateSequencedKeyBinding(textBox.Text, FIPPanelButtons.SOFTBUTTON_4_P3, (SortedList<int, KeyPressInfo>)textBox.Tag);
                }
                if (textBox.Equals(TextBoxPage3Button5))
                {
                    _fipPanel.AddOrUpdateSequencedKeyBinding(textBox.Text, FIPPanelButtons.SOFTBUTTON_5_P3, (SortedList<int, KeyPressInfo>)textBox.Tag);
                }
                if (textBox.Equals(TextBoxPage3Button6))
                {
                    _fipPanel.AddOrUpdateSequencedKeyBinding(textBox.Text, FIPPanelButtons.SOFTBUTTON_6_P3, (SortedList<int, KeyPressInfo>)textBox.Tag);
                }*/
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

                // if (textBox.Equals(TextBoxPage1Button1))
                //{
                //_fipPanel.AddOrUpdateSequencedKeyBinding(textBox.Text, FIPPanelButtons.SOFTBUTTON_1_P1, (SortedList<int, KeyPressInfo>)textBox.Tag);
                //}
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
                // if (textBox.Equals(TextBoxPage1Button1))
                //{
                //_fipPanel.AddOrUpdateDCSBIOSBinding(FIPPanelButtons.SOFTBUTTON_1_P1, dcsBiosInputs, textBox.Text);
                //}
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(345012, ex);
            }
        }

        private FIPPanelButtons GetFIPButton(TextBox textBox)
        {
            try
            {
                //if (textBox.Equals(TextBoxPage1Button1))
                //{
                return FIPPanelButtons.SOFTBUTTON_1_P1;
                //}
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
                Dispatcher.BeginInvoke(
                    (Action)
                    (() =>
                     LabelFIPsConnected.Content = "Number of connected FIPs : " + _fipHandler.FIPPanels.Count));

                /*
                foreach (var keyBinding in _fipPanel.KeyBindings)
                {
                    if (keyBinding.FIPButton == FIPPanelButtons.SOFTBUTTON_1_P1)
                    {
                        if (keyBinding.WhenTurnedOn)
                        {
                            if (keyBinding.OSKeyPress != null && !keyBinding.OSKeyPress.IsMultiSequenced())
                            {
                                TextBoxPage1Button1.Tag = keyBinding.OSKeyPress.LengthOfKeyPress();
                                TextBoxPage1Button1.Text = keyBinding.OSKeyPress.GetSimpleVirtualKeyCodesAsString();
                            }
                            else if (keyBinding.OSKeyPress != null && keyBinding.OSKeyPress.IsMultiSequenced())
                            {
                                TextBoxPage1Button1.Tag = keyBinding.OSKeyPress.GetSequence;
                                TextBoxPage1Button1.Text = keyBinding.OSKeyPress.Information;
                            }
                        }
                    }
                    if (keyBinding.FIPButton == FIPPanelButtons.SOFTBUTTON_2_P1)
                    {
                        if (keyBinding.WhenTurnedOn)
                        {
                            if (keyBinding.OSKeyPress != null && !keyBinding.OSKeyPress.IsMultiSequenced())
                            {
                                TextBoxPage1Button2.Tag = keyBinding.OSKeyPress.LengthOfKeyPress();
                                TextBoxPage1Button2.Text = keyBinding.OSKeyPress.GetSimpleVirtualKeyCodesAsString();
                            }
                            else if (keyBinding.OSKeyPress != null && keyBinding.OSKeyPress.IsMultiSequenced())
                            {
                                TextBoxPage1Button2.Tag = keyBinding.OSKeyPress.GetSequence;
                                TextBoxPage1Button2.Text = keyBinding.OSKeyPress.Information;
                            }
                        }
                    }
                    if (keyBinding.FIPButton == FIPPanelButtons.SOFTBUTTON_3_P1)
                    {
                        if (keyBinding.WhenTurnedOn)
                        {
                            if (keyBinding.OSKeyPress != null && !keyBinding.OSKeyPress.IsMultiSequenced())
                            {
                                TextBoxPage1Button3.Tag = keyBinding.OSKeyPress.LengthOfKeyPress();
                                TextBoxPage1Button3.Text = keyBinding.OSKeyPress.GetSimpleVirtualKeyCodesAsString();
                            }
                            else if (keyBinding.OSKeyPress != null && keyBinding.OSKeyPress.IsMultiSequenced())
                            {
                                TextBoxPage1Button3.Tag = keyBinding.OSKeyPress.GetSequence;
                                TextBoxPage1Button3.Text = keyBinding.OSKeyPress.Information;
                            }
                        }
                    }
                    if (keyBinding.FIPButton == FIPPanelButtons.SOFTBUTTON_4_P1)
                    {
                        if (keyBinding.WhenTurnedOn)
                        {
                            if (keyBinding.OSKeyPress != null && !keyBinding.OSKeyPress.IsMultiSequenced())
                            {
                                TextBoxPage1Button4.Tag = keyBinding.OSKeyPress.LengthOfKeyPress();
                                TextBoxPage1Button4.Text = keyBinding.OSKeyPress.GetSimpleVirtualKeyCodesAsString();
                            }
                            else if (keyBinding.OSKeyPress != null && keyBinding.OSKeyPress.IsMultiSequenced())
                            {
                                TextBoxPage1Button4.Tag = keyBinding.OSKeyPress.GetSequence;
                                TextBoxPage1Button4.Text = keyBinding.OSKeyPress.Information;
                            }
                        }
                    }
                    if (keyBinding.FIPButton == FIPPanelButtons.SOFTBUTTON_5_P1)
                    {
                        if (keyBinding.WhenTurnedOn)
                        {
                            if (keyBinding.OSKeyPress != null && !keyBinding.OSKeyPress.IsMultiSequenced())
                            {
                                TextBoxPage1Button5.Tag = keyBinding.OSKeyPress.LengthOfKeyPress();
                                TextBoxPage1Button5.Text = keyBinding.OSKeyPress.GetSimpleVirtualKeyCodesAsString();
                            }
                            else if (keyBinding.OSKeyPress != null && keyBinding.OSKeyPress.IsMultiSequenced())
                            {
                                TextBoxPage1Button5.Tag = keyBinding.OSKeyPress.GetSequence;
                                TextBoxPage1Button5.Text = keyBinding.OSKeyPress.Information;
                            }
                        }
                    }
                    if (keyBinding.FIPButton == FIPPanelButtons.SOFTBUTTON_6_P1)
                    {
                        if (keyBinding.WhenTurnedOn)
                        {
                            if (keyBinding.OSKeyPress != null && !keyBinding.OSKeyPress.IsMultiSequenced())
                            {
                                TextBoxPage1Button6.Tag = keyBinding.OSKeyPress.LengthOfKeyPress();
                                TextBoxPage1Button6.Text = keyBinding.OSKeyPress.GetSimpleVirtualKeyCodesAsString();
                            }
                            else if (keyBinding.OSKeyPress != null && keyBinding.OSKeyPress.IsMultiSequenced())
                            {
                                TextBoxPage1Button6.Tag = keyBinding.OSKeyPress.GetSequence;
                                TextBoxPage1Button6.Text = keyBinding.OSKeyPress.Information;
                            }
                        }
                    }

                    if (keyBinding.FIPButton == FIPPanelButtons.SOFTBUTTON_1_P2)
                    {
                        if (keyBinding.WhenTurnedOn)
                        {
                            if (keyBinding.OSKeyPress != null && !keyBinding.OSKeyPress.IsMultiSequenced())
                            {
                                TextBoxPage2Button1.Tag = keyBinding.OSKeyPress.LengthOfKeyPress();
                                TextBoxPage2Button1.Text = keyBinding.OSKeyPress.GetSimpleVirtualKeyCodesAsString();
                            }
                            else if (keyBinding.OSKeyPress != null && keyBinding.OSKeyPress.IsMultiSequenced())
                            {
                                TextBoxPage2Button1.Tag = keyBinding.OSKeyPress.GetSequence;
                                TextBoxPage2Button1.Text = keyBinding.OSKeyPress.Information;
                            }
                        }
                    }
                    if (keyBinding.FIPButton == FIPPanelButtons.SOFTBUTTON_2_P2)
                    {
                        if (keyBinding.WhenTurnedOn)
                        {
                            if (keyBinding.OSKeyPress != null && !keyBinding.OSKeyPress.IsMultiSequenced())
                            {
                                TextBoxPage2Button2.Tag = keyBinding.OSKeyPress.LengthOfKeyPress();
                                TextBoxPage2Button2.Text = keyBinding.OSKeyPress.GetSimpleVirtualKeyCodesAsString();
                            }
                            else if (keyBinding.OSKeyPress != null && keyBinding.OSKeyPress.IsMultiSequenced())
                            {
                                TextBoxPage2Button2.Tag = keyBinding.OSKeyPress.GetSequence;
                                TextBoxPage2Button2.Text = keyBinding.OSKeyPress.Information;
                            }
                        }
                    }
                    if (keyBinding.FIPButton == FIPPanelButtons.SOFTBUTTON_3_P2)
                    {
                        if (keyBinding.WhenTurnedOn)
                        {
                            if (keyBinding.OSKeyPress != null && !keyBinding.OSKeyPress.IsMultiSequenced())
                            {
                                TextBoxPage2Button3.Tag = keyBinding.OSKeyPress.LengthOfKeyPress();
                                TextBoxPage2Button3.Text = keyBinding.OSKeyPress.GetSimpleVirtualKeyCodesAsString();
                            }
                            else if (keyBinding.OSKeyPress != null && keyBinding.OSKeyPress.IsMultiSequenced())
                            {
                                TextBoxPage2Button3.Tag = keyBinding.OSKeyPress.GetSequence;
                                TextBoxPage2Button3.Text = keyBinding.OSKeyPress.Information;
                            }
                        }
                    }
                    if (keyBinding.FIPButton == FIPPanelButtons.SOFTBUTTON_4_P2)
                    {
                        if (keyBinding.WhenTurnedOn)
                        {
                            if (keyBinding.OSKeyPress != null && !keyBinding.OSKeyPress.IsMultiSequenced())
                            {
                                TextBoxPage2Button4.Tag = keyBinding.OSKeyPress.LengthOfKeyPress();
                                TextBoxPage2Button4.Text = keyBinding.OSKeyPress.GetSimpleVirtualKeyCodesAsString();
                            }
                            else if (keyBinding.OSKeyPress != null && keyBinding.OSKeyPress.IsMultiSequenced())
                            {
                                TextBoxPage2Button4.Tag = keyBinding.OSKeyPress.GetSequence;
                                TextBoxPage2Button4.Text = keyBinding.OSKeyPress.Information;
                            }
                        }
                    }
                    if (keyBinding.FIPButton == FIPPanelButtons.SOFTBUTTON_5_P2)
                    {
                        if (keyBinding.WhenTurnedOn)
                        {
                            if (keyBinding.OSKeyPress != null && !keyBinding.OSKeyPress.IsMultiSequenced())
                            {
                                TextBoxPage2Button5.Tag = keyBinding.OSKeyPress.LengthOfKeyPress();
                                TextBoxPage2Button5.Text = keyBinding.OSKeyPress.GetSimpleVirtualKeyCodesAsString();
                            }
                            else if (keyBinding.OSKeyPress != null && keyBinding.OSKeyPress.IsMultiSequenced())
                            {
                                TextBoxPage2Button5.Tag = keyBinding.OSKeyPress.GetSequence;
                                TextBoxPage2Button5.Text = keyBinding.OSKeyPress.Information;
                            }
                        }
                    }
                    if (keyBinding.FIPButton == FIPPanelButtons.SOFTBUTTON_6_P2)
                    {
                        if (keyBinding.WhenTurnedOn)
                        {
                            if (keyBinding.OSKeyPress != null && !keyBinding.OSKeyPress.IsMultiSequenced())
                            {
                                TextBoxPage2Button6.Tag = keyBinding.OSKeyPress.LengthOfKeyPress();
                                TextBoxPage2Button6.Text = keyBinding.OSKeyPress.GetSimpleVirtualKeyCodesAsString();
                            }
                            else if (keyBinding.OSKeyPress != null && keyBinding.OSKeyPress.IsMultiSequenced())
                            {
                                TextBoxPage2Button6.Tag = keyBinding.OSKeyPress.GetSequence;
                                TextBoxPage2Button6.Text = keyBinding.OSKeyPress.Information;
                            }
                        }
                    }

                    if (keyBinding.FIPButton == FIPPanelButtons.SOFTBUTTON_1_P3)
                    {
                        if (keyBinding.WhenTurnedOn)
                        {
                            if (keyBinding.OSKeyPress != null && !keyBinding.OSKeyPress.IsMultiSequenced())
                            {
                                TextBoxPage3Button1.Tag = keyBinding.OSKeyPress.LengthOfKeyPress();
                                TextBoxPage3Button1.Text = keyBinding.OSKeyPress.GetSimpleVirtualKeyCodesAsString();
                            }
                            else if (keyBinding.OSKeyPress != null && keyBinding.OSKeyPress.IsMultiSequenced())
                            {
                                TextBoxPage3Button1.Tag = keyBinding.OSKeyPress.GetSequence;
                                TextBoxPage3Button1.Text = keyBinding.OSKeyPress.Information;
                            }
                        }
                    }
                    if (keyBinding.FIPButton == FIPPanelButtons.SOFTBUTTON_2_P3)
                    {
                        if (keyBinding.WhenTurnedOn)
                        {
                            if (keyBinding.OSKeyPress != null && !keyBinding.OSKeyPress.IsMultiSequenced())
                            {
                                TextBoxPage3Button2.Tag = keyBinding.OSKeyPress.LengthOfKeyPress();
                                TextBoxPage3Button2.Text = keyBinding.OSKeyPress.GetSimpleVirtualKeyCodesAsString();
                            }
                            else if (keyBinding.OSKeyPress != null && keyBinding.OSKeyPress.IsMultiSequenced())
                            {
                                TextBoxPage3Button2.Tag = keyBinding.OSKeyPress.GetSequence;
                                TextBoxPage3Button2.Text = keyBinding.OSKeyPress.Information;
                            }
                        }
                    }
                    if (keyBinding.FIPButton == FIPPanelButtons.SOFTBUTTON_3_P3)
                    {
                        if (keyBinding.WhenTurnedOn)
                        {
                            if (keyBinding.OSKeyPress != null && !keyBinding.OSKeyPress.IsMultiSequenced())
                            {
                                TextBoxPage3Button3.Tag = keyBinding.OSKeyPress.LengthOfKeyPress();
                                TextBoxPage3Button3.Text = keyBinding.OSKeyPress.GetSimpleVirtualKeyCodesAsString();
                            }
                            else if (keyBinding.OSKeyPress != null && keyBinding.OSKeyPress.IsMultiSequenced())
                            {
                                TextBoxPage3Button3.Tag = keyBinding.OSKeyPress.GetSequence;
                                TextBoxPage3Button3.Text = keyBinding.OSKeyPress.Information;
                            }
                        }
                    }
                    if (keyBinding.FIPButton == FIPPanelButtons.SOFTBUTTON_4_P3)
                    {
                        if (keyBinding.WhenTurnedOn)
                        {
                            if (keyBinding.OSKeyPress != null && !keyBinding.OSKeyPress.IsMultiSequenced())
                            {
                                TextBoxPage3Button4.Tag = keyBinding.OSKeyPress.LengthOfKeyPress();
                                TextBoxPage3Button4.Text = keyBinding.OSKeyPress.GetSimpleVirtualKeyCodesAsString();
                            }
                            else if (keyBinding.OSKeyPress != null && keyBinding.OSKeyPress.IsMultiSequenced())
                            {
                                TextBoxPage3Button4.Tag = keyBinding.OSKeyPress.GetSequence;
                                TextBoxPage3Button4.Text = keyBinding.OSKeyPress.Information;
                            }
                        }
                    }
                    if (keyBinding.FIPButton == FIPPanelButtons.SOFTBUTTON_5_P3)
                    {
                        if (keyBinding.WhenTurnedOn)
                        {
                            if (keyBinding.OSKeyPress != null && !keyBinding.OSKeyPress.IsMultiSequenced())
                            {
                                TextBoxPage3Button5.Tag = keyBinding.OSKeyPress.LengthOfKeyPress();
                                TextBoxPage3Button5.Text = keyBinding.OSKeyPress.GetSimpleVirtualKeyCodesAsString();
                            }
                            else if (keyBinding.OSKeyPress != null && keyBinding.OSKeyPress.IsMultiSequenced())
                            {
                                TextBoxPage3Button5.Tag = keyBinding.OSKeyPress.GetSequence;
                                TextBoxPage3Button5.Text = keyBinding.OSKeyPress.Information;
                            }
                        }
                    }
                    if (keyBinding.FIPButton == FIPPanelButtons.SOFTBUTTON_6_P3)
                    {
                        if (keyBinding.WhenTurnedOn)
                        {
                            if (keyBinding.OSKeyPress != null && !keyBinding.OSKeyPress.IsMultiSequenced())
                            {
                                TextBoxPage3Button6.Tag = keyBinding.OSKeyPress.LengthOfKeyPress();
                                TextBoxPage3Button6.Text = keyBinding.OSKeyPress.GetSimpleVirtualKeyCodesAsString();
                            }
                            else if (keyBinding.OSKeyPress != null && keyBinding.OSKeyPress.IsMultiSequenced())
                            {
                                TextBoxPage3Button6.Tag = keyBinding.OSKeyPress.GetSequence;
                                TextBoxPage3Button6.Text = keyBinding.OSKeyPress.Information;
                            }
                        }
                    }
                }
                

                foreach (var dcsBiosBinding in _fipPanel.DCSBiosBindings)
                {
                    if (dcsBiosBinding.FIPButton == FIPPanelButtons.SOFTBUTTON_1_P1 && dcsBiosBinding.WhenTurnedOn && dcsBiosBinding.DCSBIOSInputs.Count > 0)
                    {
                        TextBoxPage1Button1.Tag = dcsBiosBinding.DCSBIOSInputs;
                        TextBoxPage1Button1.Text = dcsBiosBinding.Description;
                        TextBoxPage1Button1.ToolTip = "DCS-BIOS";
                    }
                    if (dcsBiosBinding.FIPButton == FIPPanelButtons.SOFTBUTTON_2_P1 && dcsBiosBinding.WhenTurnedOn && dcsBiosBinding.DCSBIOSInputs.Count > 0)
                    {
                        TextBoxPage1Button2.Tag = dcsBiosBinding.DCSBIOSInputs;
                        TextBoxPage1Button2.Text = dcsBiosBinding.Description;
                        TextBoxPage1Button2.ToolTip = "DCS-BIOS";
                    }
                    if (dcsBiosBinding.FIPButton == FIPPanelButtons.SOFTBUTTON_3_P1 && dcsBiosBinding.WhenTurnedOn && dcsBiosBinding.DCSBIOSInputs.Count > 0)
                    {
                        TextBoxPage1Button3.Tag = dcsBiosBinding.DCSBIOSInputs;
                        TextBoxPage1Button3.Text = dcsBiosBinding.Description;
                        TextBoxPage1Button3.ToolTip = "DCS-BIOS";
                    }
                    if (dcsBiosBinding.FIPButton == FIPPanelButtons.SOFTBUTTON_4_P1 && dcsBiosBinding.WhenTurnedOn && dcsBiosBinding.DCSBIOSInputs.Count > 0)
                    {
                        TextBoxPage1Button4.Tag = dcsBiosBinding.DCSBIOSInputs;
                        TextBoxPage1Button4.Text = dcsBiosBinding.Description;
                        TextBoxPage1Button4.ToolTip = "DCS-BIOS";
                    }
                    if (dcsBiosBinding.FIPButton == FIPPanelButtons.SOFTBUTTON_5_P1 && dcsBiosBinding.WhenTurnedOn && dcsBiosBinding.DCSBIOSInputs.Count > 0)
                    {
                        TextBoxPage1Button5.Tag = dcsBiosBinding.DCSBIOSInputs;
                        TextBoxPage1Button5.Text = dcsBiosBinding.Description;
                        TextBoxPage1Button5.ToolTip = "DCS-BIOS";
                    }
                    if (dcsBiosBinding.FIPButton == FIPPanelButtons.SOFTBUTTON_6_P1 && dcsBiosBinding.WhenTurnedOn && dcsBiosBinding.DCSBIOSInputs.Count > 0)
                    {
                        TextBoxPage1Button6.Tag = dcsBiosBinding.DCSBIOSInputs;
                        TextBoxPage1Button6.Text = dcsBiosBinding.Description;
                        TextBoxPage1Button6.ToolTip = "DCS-BIOS";
                    }


                    if (dcsBiosBinding.FIPButton == FIPPanelButtons.SOFTBUTTON_1_P2 && dcsBiosBinding.WhenTurnedOn && dcsBiosBinding.DCSBIOSInputs.Count > 0)
                    {
                        TextBoxPage2Button1.Tag = dcsBiosBinding.DCSBIOSInputs;
                        TextBoxPage2Button1.Text = dcsBiosBinding.Description;
                        TextBoxPage2Button1.ToolTip = "DCS-BIOS";
                    }
                    if (dcsBiosBinding.FIPButton == FIPPanelButtons.SOFTBUTTON_2_P2 && dcsBiosBinding.WhenTurnedOn && dcsBiosBinding.DCSBIOSInputs.Count > 0)
                    {
                        TextBoxPage2Button2.Tag = dcsBiosBinding.DCSBIOSInputs;
                        TextBoxPage2Button2.Text = dcsBiosBinding.Description;
                        TextBoxPage2Button2.ToolTip = "DCS-BIOS";
                    }
                    if (dcsBiosBinding.FIPButton == FIPPanelButtons.SOFTBUTTON_3_P2 && dcsBiosBinding.WhenTurnedOn && dcsBiosBinding.DCSBIOSInputs.Count > 0)
                    {
                        TextBoxPage2Button3.Tag = dcsBiosBinding.DCSBIOSInputs;
                        TextBoxPage2Button3.Text = dcsBiosBinding.Description;
                        TextBoxPage2Button3.ToolTip = "DCS-BIOS";
                    }
                    if (dcsBiosBinding.FIPButton == FIPPanelButtons.SOFTBUTTON_4_P2 && dcsBiosBinding.WhenTurnedOn && dcsBiosBinding.DCSBIOSInputs.Count > 0)
                    {
                        TextBoxPage2Button4.Tag = dcsBiosBinding.DCSBIOSInputs;
                        TextBoxPage2Button4.Text = dcsBiosBinding.Description;
                        TextBoxPage2Button4.ToolTip = "DCS-BIOS";
                    }
                    if (dcsBiosBinding.FIPButton == FIPPanelButtons.SOFTBUTTON_5_P2 && dcsBiosBinding.WhenTurnedOn && dcsBiosBinding.DCSBIOSInputs.Count > 0)
                    {
                        TextBoxPage2Button5.Tag = dcsBiosBinding.DCSBIOSInputs;
                        TextBoxPage2Button5.Text = dcsBiosBinding.Description;
                        TextBoxPage2Button5.ToolTip = "DCS-BIOS";
                    }
                    if (dcsBiosBinding.FIPButton == FIPPanelButtons.SOFTBUTTON_6_P2 && dcsBiosBinding.WhenTurnedOn && dcsBiosBinding.DCSBIOSInputs.Count > 0)
                    {
                        TextBoxPage2Button6.Tag = dcsBiosBinding.DCSBIOSInputs;
                        TextBoxPage2Button6.Text = dcsBiosBinding.Description;
                        TextBoxPage2Button6.ToolTip = "DCS-BIOS";
                    }


                    if (dcsBiosBinding.FIPButton == FIPPanelButtons.SOFTBUTTON_1_P3 && dcsBiosBinding.WhenTurnedOn && dcsBiosBinding.DCSBIOSInputs.Count > 0)
                    {
                        TextBoxPage3Button1.Tag = dcsBiosBinding.DCSBIOSInputs;
                        TextBoxPage3Button1.Text = dcsBiosBinding.Description;
                        TextBoxPage3Button1.ToolTip = "DCS-BIOS";
                    }
                    if (dcsBiosBinding.FIPButton == FIPPanelButtons.SOFTBUTTON_2_P3 && dcsBiosBinding.WhenTurnedOn && dcsBiosBinding.DCSBIOSInputs.Count > 0)
                    {
                        TextBoxPage3Button2.Tag = dcsBiosBinding.DCSBIOSInputs;
                        TextBoxPage3Button2.Text = dcsBiosBinding.Description;
                        TextBoxPage3Button2.ToolTip = "DCS-BIOS";
                    }
                    if (dcsBiosBinding.FIPButton == FIPPanelButtons.SOFTBUTTON_3_P3 && dcsBiosBinding.WhenTurnedOn && dcsBiosBinding.DCSBIOSInputs.Count > 0)
                    {
                        TextBoxPage3Button3.Tag = dcsBiosBinding.DCSBIOSInputs;
                        TextBoxPage3Button3.Text = dcsBiosBinding.Description;
                        TextBoxPage3Button3.ToolTip = "DCS-BIOS";
                    }
                    if (dcsBiosBinding.FIPButton == FIPPanelButtons.SOFTBUTTON_4_P3 && dcsBiosBinding.WhenTurnedOn && dcsBiosBinding.DCSBIOSInputs.Count > 0)
                    {
                        TextBoxPage3Button4.Tag = dcsBiosBinding.DCSBIOSInputs;
                        TextBoxPage3Button4.Text = dcsBiosBinding.Description;
                        TextBoxPage3Button4.ToolTip = "DCS-BIOS";
                    }
                    if (dcsBiosBinding.FIPButton == FIPPanelButtons.SOFTBUTTON_5_P3 && dcsBiosBinding.WhenTurnedOn && dcsBiosBinding.DCSBIOSInputs.Count > 0)
                    {
                        TextBoxPage3Button5.Tag = dcsBiosBinding.DCSBIOSInputs;
                        TextBoxPage3Button5.Text = dcsBiosBinding.Description;
                        TextBoxPage3Button5.ToolTip = "DCS-BIOS";
                    }
                    if (dcsBiosBinding.FIPButton == FIPPanelButtons.SOFTBUTTON_6_P3 && dcsBiosBinding.WhenTurnedOn && dcsBiosBinding.DCSBIOSInputs.Count > 0)
                    {
                        TextBoxPage3Button6.Tag = dcsBiosBinding.DCSBIOSInputs;
                        TextBoxPage3Button6.Text = dcsBiosBinding.Description;
                        TextBoxPage3Button6.ToolTip = "DCS-BIOS";
                    }

                }*/
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
                /*if (_fipPanel != null)
                {
                    Clipboard.SetText(_fipPanel.InstanceId);
                    MessageBox.Show("The Instance Id for the panel has been copied to the Clipboard.");
                }*/
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(3015, ex);
            }
        }

        private void ButtonSwitchPanelInfo_OnClick(object sender, RoutedEventArgs e)
        {
            var bytes = Encoding.UTF8.GetBytes(Properties.Resources.PZ55Notes);
            var informationWindow = new InformationWindow(bytes);
            informationWindow.ShowDialog();
        }

        private void ButtonBackgroundImageKeyEmPage1_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(3309015, ex);
            }
        }
    }
}
