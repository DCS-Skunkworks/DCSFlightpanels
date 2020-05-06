using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ClassLibraryCommon;
using DCSFlightpanels.Bills;
using DCSFlightpanels.CustomControls;
using DCSFlightpanels.Windows;
using NonVisuals;
using NonVisuals.Interfaces;
using NonVisuals.Saitek;

namespace DCSFlightpanels.PanelUserControls
{
    /// <summary>
    /// Interaction logic for TPMPanelUserControl.xaml
    /// </summary>
    public partial class TPMPanelUserControl : UserControlBase, IGamingPanelListener, IProfileHandlerListener, IGamingPanelUserControl
    {

        private readonly TPMPanel _tpmPanel;
        private readonly TabItem _parentTabItem;
        private string _parentTabItemHeader;
        private readonly IGlobalHandler _globalHandler;
        private bool _once;
        private bool _textBoxBillsSet;
        private bool _controlLoaded;

        public TPMPanelUserControl(HIDSkeleton hidSkeleton, TabItem parentTabItem, IGlobalHandler globalHandler)
        {
            InitializeComponent();
            _parentTabItem = parentTabItem;
            _parentTabItemHeader = _parentTabItem.Header.ToString();
            _tpmPanel = new TPMPanel(hidSkeleton);

            _tpmPanel.Attach((IGamingPanelListener)this);
            globalHandler.Attach(_tpmPanel);
            _globalHandler = globalHandler;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _tpmPanel.Dispose();
                _tpmPanel.Dispose();
            }
        }
        
        private void TPMPanelUserControl_OnLoaded(object sender, RoutedEventArgs e)
        {
            if (!_once)
            {
                HidePositionIndicators();
                _once = true;
            }
            SetTextBoxBills();
            SetContextMenuClickHandlers();
            _controlLoaded = true;
            ShowGraphicConfiguration();
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
                Common.ShowErrorMessageBox( ex);
            }
        }

        public void BipPanelRegisterEvent(object sender, BipPanelRegisteredEventArgs e)
        {
            RemoveContextMenuClickHandlers();
            SetContextMenuClickHandlers();
        }

        public GamingPanel GetGamingPanel()
        {
            return _tpmPanel;
        }

        public string GetName()
        {
            return GetType().Name;
        }

        public void SelectedAirframe(object sender, AirframeEventArgs e) { }

        public void UpdatesHasBeenMissed(object sender, DCSBIOSUpdatesMissedEventArgs e) { }

        public void SwitchesChanged(object sender, SwitchesChangedEventArgs e)
        {
            try
            {
                if (e.GamingPanelEnum == GamingPanelEnum.TPM && e.UniqueId.Equals(_tpmPanel.InstanceId))
                {
                    NotifySwitchChanges(e.Switches);
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox( ex);
            }
        }

        public void PanelSettingsReadFromFile(object sender, SettingsReadFromFileEventArgs e)
        {
            try
            {
                ShowGraphicConfiguration();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox( ex);
            }
        }

        public void SettingsCleared(object sender, PanelEventArgs e)
        {
            try
            {
                ClearAll(false);
                ShowGraphicConfiguration();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox( ex);
            }
        }

        public void PanelDataAvailable(object sender, PanelDataToDCSBIOSEventEventArgs e) { }

        public void DeviceAttached(object sender, PanelEventArgs e) { }

        public void LedLightChanged(object sender, LedLightChangeEventArgs e) { }

        public void DeviceDetached(object sender, PanelEventArgs e) { }

        public void SettingsApplied(object sender, PanelEventArgs e)
        {
            try
            {
                if (e.UniqueId.Equals(_tpmPanel.InstanceId) && e.GamingPanelEnum == GamingPanelEnum.TPM)
                {
                    Dispatcher?.BeginInvoke((Action)(ShowGraphicConfiguration));
                    Dispatcher?.BeginInvoke((Action)(() => TextBoxLogTPM.Text = ""));
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox( ex);
            }
        }

        public void PanelSettingsChanged(object sender, PanelEventArgs e)
        {
            try
            {
                Dispatcher?.BeginInvoke((Action)(ShowGraphicConfiguration));
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox( ex);
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
                Common.ShowErrorMessageBox( ex);
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
                KeySequenceWindow keySequenceWindow;
                if (textBox.Bill.ContainsKeySequence())
                {
                    keySequenceWindow = new KeySequenceWindow(textBox.Text, textBox.Bill.GetKeySequence());
                }
                else
                {
                    keySequenceWindow = new KeySequenceWindow();
                }
                keySequenceWindow.ShowDialog();
                if (keySequenceWindow.DialogResult.HasValue && keySequenceWindow.DialogResult.Value)
                {
                    //Clicked OK
                    //If the user added only a single key stroke combo then let's not treat this as a sequence
                    if (!keySequenceWindow.IsDirty)
                    {
                        //User made no changes
                        return;
                    }
                    var sequenceList = keySequenceWindow.GetSequence;
                    if (sequenceList.Count > 1)
                    {
                        var keyPress = new KeyPress("Key press sequence", sequenceList);
                        textBox.Bill.KeyPress = keyPress;
                        textBox.Bill.KeyPress.Information = keySequenceWindow.GetInformation;
                        if (!string.IsNullOrEmpty(keySequenceWindow.GetInformation))
                        {
                            textBox.Text = keySequenceWindow.GetInformation;
                        }
                        UpdateKeyBindingProfileSequencedKeyStrokesTPM(textBox);
                    }
                    else
                    {
                        //If only one press was created treat it as a simple keypress
                        textBox.Bill.Clear();
                        var keyPress = new KeyPress(sequenceList[0].VirtualKeyCodesAsString, sequenceList[0].LengthOfKeyPress);
                        textBox.Bill.KeyPress = keyPress;
                        textBox.Bill.KeyPress.Information = keySequenceWindow.GetInformation;
                        textBox.Text = sequenceList[0].VirtualKeyCodesAsString;
                        UpdateKeyBindingProfileSimpleKeyStrokes(textBox);
                    }
                }
                TextBoxLogTPM.Focus();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox( ex);
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
                DCSBIOSInputControlsWindow dcsBIOSInputControlsWindow;
                if (textBox.Bill.ContainsDCSBIOS())
                {
                    dcsBIOSInputControlsWindow = new DCSBIOSInputControlsWindow(_globalHandler.GetAirframe(), textBox.Name.Replace("TextBox", ""), textBox.Bill.DCSBIOSBinding.DCSBIOSInputs, textBox.Text);
                }
                else
                {
                    dcsBIOSInputControlsWindow = new DCSBIOSInputControlsWindow(_globalHandler.GetAirframe(), textBox.Name.Replace("TextBox", ""), null);
                }
                dcsBIOSInputControlsWindow.ShowDialog();
                if (dcsBIOSInputControlsWindow.DialogResult.HasValue && dcsBIOSInputControlsWindow.DialogResult == true)
                {
                    var dcsBiosInputs = dcsBIOSInputControlsWindow.DCSBIOSInputs;
                    var text = string.IsNullOrWhiteSpace(dcsBIOSInputControlsWindow.Description) ? "DCS-BIOS" : dcsBIOSInputControlsWindow.Description;
                    //1 appropriate text to textbox
                    //2 update bindings
                    textBox.Text = text;
                    textBox.Bill.Consume(dcsBiosInputs);
                    UpdateDCSBIOSBinding(textBox);
                }
                TextBoxLogTPM.Focus();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox( ex);
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
                if (textBox.Bill.ContainsBIPLink())
                {
                    var bipLink = textBox.Bill.BIPLink;
                    bipLinkWindow = new BIPLinkWindow(bipLink);
                }
                else
                {
                    var bipLink = new BIPLinkTPM();
                    bipLinkWindow = new BIPLinkWindow(bipLink);
                }
                bipLinkWindow.ShowDialog();
                if (bipLinkWindow.DialogResult.HasValue && bipLinkWindow.DialogResult == true && bipLinkWindow.IsDirty && bipLinkWindow.BIPLink != null)
                {
                    textBox.Bill.BIPLink = (BIPLinkTPM)bipLinkWindow.BIPLink;
                    UpdateBIPLinkBindings(textBox);
                }
                TextBoxLogTPM.Focus();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
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

                //Check new value, is menu visible?
                if (!(bool)e.NewValue)
                {
                    //Do not show if not visible
                    return;
                }
                if (!textBox.Bill.ContainsSingleKey())
                {
                    return;
                }
                var keyPressLength = textBox.Bill.KeyPress.GetLengthOfKeyPress();
                CheckContextMenuItems(keyPressLength, contextMenu);

            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void ClearAll(bool clearAlsoProfile)
        {
            foreach (var textBox in Common.FindVisualChildren<TPMTextBox>(this))
            {
                if (textBox.Equals(TextBoxLogTPM))
                {
                    continue;
                }
                textBox.Bill.Clear();
            }
            if (clearAlsoProfile)
            {
                _tpmPanel.ClearSettings();
            }
        }

        private void SetTextBoxBills()
        {
            if (_textBoxBillsSet || !Common.FindVisualChildren<TPMTextBox>(this).Any())
            {
                return;
            }
            foreach (var textBox in Common.FindVisualChildren<TPMTextBox>(this))
            {
                if (textBox.Equals(TextBoxLogTPM))
                {
                    continue;
                }
                textBox.Bill = new BillTPM(textBox, GetTPMSwitch(textBox));
            }
            _textBoxBillsSet = true;
        }

        private void RemoveContextMenuClickHandlers()
        {
            foreach (var textBox in Common.FindVisualChildren<TPMTextBox>(this))
            {
                if (!Equals(textBox, TextBoxLogTPM))
                {
                    textBox.ContextMenu = null;
                    textBox.ContextMenuOpening -= TextBoxContextMenuOpening;
                }
            }
        }

        private void SetContextMenuClickHandlers()
        {
            foreach (var textBox in Common.FindVisualChildren<TPMTextBox>(this))
            {
                if (!Equals(textBox, TextBoxLogTPM))
                {
                    var contectMenu = (ContextMenu)Resources["TextBoxContextMenuTPM"];

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
                    if (Common.NoDCSBIOSEnabled())
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
        }

        private void TextBoxContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            try
            {
                var textBox = (TPMTextBox)sender;
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

                if (textBox.Bill.ContainsDCSBIOS())
                {
                    // 1) If Contains DCSBIOS, show Edit DCS-BIOS Control & BIP
                    foreach (MenuItem item in contextMenu.Items)
                    {
                        if (Common.FullDCSBIOSEnabled() && item.Name.Contains("EditDCSBIOS"))
                        {
                            item.Visibility = Visibility.Visible;
                        }
                        if (BipFactory.HasBips() && item.Name.Contains("EditBIP"))
                        {
                            item.Visibility = Visibility.Visible;
                        }
                    }
                }
                else if (textBox.Bill.ContainsKeySequence())
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
                else if (textBox.Bill.IsEmpty())
                {
                    // 4) 
                    foreach (MenuItem item in contextMenu.Items)
                    {
                        if (item.Name.Contains("EditSequence"))
                        {
                            item.Visibility = Visibility.Visible;
                        }
                        else if (Common.FullDCSBIOSEnabled() && item.Name.Contains("EditDCSBIOS"))
                        {
                            item.Visibility = Visibility.Visible;
                        }
                        else if (BipFactory.HasBips() && item.Name.Contains("EditBIP"))
                        {
                            item.Visibility = Visibility.Visible;
                        }
                        else if (item.Name.Contains("EditOSCommand"))
                        {
                            item.Visibility = Visibility.Visible;
                        }
                        else if (item.Name.Contains("AddNullKey"))
                        {
                            item.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            item.Visibility = Visibility.Collapsed;
                        }
                    }
                }
                else if (textBox.Bill.ContainsSingleKey())
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
                else if (textBox.Bill.ContainsBIPLink())
                {
                    // 3) 
                    foreach (MenuItem item in contextMenu.Items)
                    {
                        if (Common.FullDCSBIOSEnabled() && item.Name.Contains("EditDCSBIOS"))
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
                else if (textBox.Bill.ContainsOSCommand())
                {
                    foreach (MenuItem item in contextMenu.Items)
                    {
                        if (item.Name.Contains("EditOSCommand"))
                        {
                            item.Visibility = Visibility.Visible;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox( ex);
            }
        }

        private TPMTextBox GetTextBoxInFocus()
        {
            foreach (var textBox in Common.FindVisualChildren<TPMTextBox>(this))
            {
                if (!Equals(textBox, TextBoxLogTPM) && textBox.IsFocused && Equals(textBox.Background, Brushes.Yellow))
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
                var textBox = GetTextBoxInFocus();//OK
                SetKeyPressLength(textBox, (MenuItem) sender);
                
                UpdateKeyBindingProfileSimpleKeyStrokes(textBox);
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox( ex);
            }
        }

        private void TextBoxMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                var textBox = (TPMTextBox)sender;

                if (e.ChangedButton == MouseButton.Left)
                {

                    //Check if this textbox contains DCS-BIOS information. If so then prompt the user for deletion
                    if (textBox.Bill.ContainsDCSBIOS())
                    {
                        if (MessageBox.Show("Do you want to delete the DCS-BIOS configuration?", "Delete DCS-BIOS configuration?", MessageBoxButton.OKCancel, MessageBoxImage.Question) != MessageBoxResult.OK)
                        {
                            return;
                        }
                        textBox.Text = "";
                        _tpmPanel.RemoveTPMPanelSwitchFromList(ControlListTPM.DCSBIOS, GetTPMSwitch(textBox).TPMSwitch, GetTPMSwitch(textBox).ButtonState);
                        textBox.Bill.DCSBIOSBinding = null;
                    }
                    else if (textBox.Bill.ContainsKeySequence())
                    {
                        //Check if this textbox contains sequence information. If so then prompt the user for deletion
                        if (MessageBox.Show("Do you want to delete the key sequence?", "Delete key sequence?", MessageBoxButton.OKCancel, MessageBoxImage.Question) != MessageBoxResult.OK)
                        {
                            return;
                        }
                        textBox.Bill.KeyPress.KeySequence.Clear();
                        textBox.Text = "";
                        UpdateKeyBindingProfileSimpleKeyStrokes(textBox);
                    }
                    else if (textBox.Bill.ContainsSingleKey())
                    {
                        textBox.Bill.KeyPress.KeySequence.Clear();
                        textBox.Text = "";
                        UpdateKeyBindingProfileSimpleKeyStrokes(textBox);
                    }
                    if (textBox.Bill.ContainsBIPLink())
                    {
                        //Check if this textbox contains sequence information. If so then prompt the user for deletion
                        if (MessageBox.Show("Do you want to delete BIP Links?", "Delete BIP Link?", MessageBoxButton.OKCancel, MessageBoxImage.Question) != MessageBoxResult.OK)
                        {
                            return;
                        }
                        textBox.Bill.BIPLink.BIPLights.Clear();
                        textBox.Background = Brushes.White;
                        UpdateBIPLinkBindings(textBox);
                    }
                    TextBoxLogTPM.Focus();
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox( ex);
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
                Common.ShowErrorMessageBox( ex);
            }
        }


        private void TextBoxGotFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                ((TPMTextBox)sender).Background = Brushes.Yellow;
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox( ex);
            }
        }

        private void TextBoxLostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                var textBox = (TPMTextBox)sender;
                if (textBox.Bill.ContainsBIPLink())
                {
                    ((TPMTextBox)sender).Background = Brushes.Bisque;
                }
                else
                {
                    ((TPMTextBox)sender).Background = Brushes.White;
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox( ex);
            }
        }


        private void TextBoxPreviewKeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                var textBox = ((TPMTextBox)sender);

                //Check if this textbox contains sequence or DCS-BIOS information. If so then exit
                if (textBox.Bill.ContainsKeySequence() || textBox.Bill.ContainsDCSBIOS())
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
                Common.ShowErrorMessageBox( ex);
            }
        }


        private void TextBoxTextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                //MAKE SURE THE Tag iss SET BEFORE SETTING TEXT! OTHERWISE THIS DOESN'T FIRE
                var textBox = (TPMTextBox)sender;
                if (textBox.Bill.ContainsKeySequence())
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
                Common.ShowErrorMessageBox( ex);
            }
        }


        private void TextBoxShortcutKeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                var textBox = ((TPMTextBox)sender);
                //Check if this textbox contains sequence or DCS-BIOS information. If so then exit
                if (textBox.Bill.ContainsKeySequence() || textBox.Bill.ContainsDCSBIOS())
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
                UpdateKeyBindingProfileSequencedKeyStrokesTPM(textBox);
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox( ex);
            }
        }

        private void NotifySwitchChanges(HashSet<object> switches)
        {
            try
            {
                //Set focus to this so that virtual keypresses won't affect settings
                Dispatcher?.BeginInvoke((Action)(() => TextBoxLogTPM.Focus()));
                foreach (var tpmPanelSwitch in switches)
                {
                    var key = (TPMPanelSwitch)tpmPanelSwitch;

                    if (_tpmPanel.ForwardPanelEvent)
                    {
                        if (!string.IsNullOrEmpty(_tpmPanel.GetKeyPressForLoggingPurposes(key)))
                        {
                            Dispatcher?.BeginInvoke(
                                (Action)
                                (() =>
                                 TextBoxLogTPM.Text =
                                 TextBoxLogTPM.Text.Insert(0, _tpmPanel.GetKeyPressForLoggingPurposes(key) + "\n")));
                        }
                    }
                    else
                    {
                        Dispatcher?.BeginInvoke(
                            (Action)
                            (() =>
                             TextBoxLogTPM.Text =
                             TextBoxLogTPM.Text = TextBoxLogTPM.Text.Insert(0, "No action taken, panel events Disabled.\n")));
                    }
                }
                SetGraphicsState(switches);
            }
            catch (Exception ex)
            {
                Dispatcher?.BeginInvoke(
                    (Action)
                    (() =>
                     TextBoxLogTPM.Text = TextBoxLogTPM.Text.Insert(0, "0x16" + ex.Message + ".\n")));
                Common.ShowErrorMessageBox( ex);
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
                                Dispatcher?.BeginInvoke(
                                    (Action) delegate
                                    {
                                        ImageG1On.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                        ImageG1Off.Visibility = key.IsOn ? Visibility.Collapsed : Visibility.Visible;
                                    });
                                break;
                            }
                        case TPMPanelSwitches.G2:
                            {
                                var key = tpmPanelSwitch;
                                Dispatcher?.BeginInvoke(
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
                                Dispatcher?.BeginInvoke(
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
                                Dispatcher?.BeginInvoke(
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
                                Dispatcher?.BeginInvoke(
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
                                Dispatcher?.BeginInvoke(
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
                                Dispatcher?.BeginInvoke(
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
                                Dispatcher?.BeginInvoke(
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
                                Dispatcher?.BeginInvoke(
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
                Common.ShowErrorMessageBox( ex);
            }
        }

        private void UpdateKeyBindingProfileSequencedKeyStrokesTPM(TPMTextBox textBox)
        {
            try
            {
                _tpmPanel.AddOrUpdateSequencedKeyBinding(textBox.Text, textBox.Bill.Key.TPMSwitch, textBox.Bill.GetKeySequence(), textBox.Bill.Key.ButtonState);
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox( ex);
            }
        }

        private void UpdateBIPLinkBindings(TPMTextBox textBox)
        {
            try
            {
                _tpmPanel.AddOrUpdateBIPLinkKeyBinding(textBox.Bill.Key.TPMSwitch, textBox.Bill.BIPLink, textBox.Bill.Key.ButtonState);
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox( ex);
            }
        }

        private void UpdateKeyBindingProfileSimpleKeyStrokes(TPMTextBox textBox)
        {
            try
            {
                KeyPressLength keyPressLength;
                if (!textBox.Bill.ContainsKeyPress() || textBox.Bill.KeyPress.KeySequence.Count == 0)
                {
                    keyPressLength = KeyPressLength.ThirtyTwoMilliSec;
                }
                else
                {
                    keyPressLength = textBox.Bill.KeyPress.GetLengthOfKeyPress();
                }
                _tpmPanel.AddOrUpdateSingleKeyBinding(textBox.Bill.Key.TPMSwitch, textBox.Text, keyPressLength, textBox.Bill.Key.ButtonState);
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox( ex);
            }
        }

        private void UpdateOSCommandBindingsTPM(TPMTextBox textBox)
        {
            try
            {
                _tpmPanel.AddOrUpdateOSCommandBinding(textBox.Bill.Key.TPMSwitch, textBox.Bill.OSCommandObject, textBox.Bill.Key.ButtonState);
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox( ex);
            }
        }

        private void UpdateDCSBIOSBinding(TPMTextBox textBox)
        {
            try
            {
                _tpmPanel.AddOrUpdateDCSBIOSBinding(textBox.Bill.Key.TPMSwitch, textBox.Bill.DCSBIOSBinding.DCSBIOSInputs, textBox.Text, textBox.Bill.Key.ButtonState);
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox( ex);
            }
        }


        private void ShowGraphicConfiguration()
        {
            try
            {
                if (!_controlLoaded || !_textBoxBillsSet)
                {
                    return;
                }
                foreach (var keyBinding in _tpmPanel.KeyBindingsHashSet)
                {
                    var textBox = GetTextBox(keyBinding.TPMSwitch, keyBinding.WhenTurnedOn);
                    if (keyBinding.OSKeyPress != null)
                    {
                        textBox.Bill.KeyPress = keyBinding.OSKeyPress;
                    }
                }

                foreach (var osCommand in _tpmPanel.OSCommandHashSet)
                {
                    var textBox = GetTextBox(osCommand.TPMSwitch, osCommand.WhenTurnedOn);
                    if (osCommand.OSCommandObject != null)
                    {
                        textBox.Bill.OSCommandObject = osCommand.OSCommandObject;
                    }
                }

                foreach (var dcsBiosBinding in _tpmPanel.DCSBiosBindings)
                {
                    var textBox = GetTextBox(dcsBiosBinding.TPMSwitch, dcsBiosBinding.WhenTurnedOn);
                    if (dcsBiosBinding.DCSBIOSInputs.Count > 0)
                    {
                        textBox.Bill.DCSBIOSBinding = dcsBiosBinding;
                    }
                }

                foreach (var bipLink in _tpmPanel.BipLinkHashSet)
                {
                    var textBox = GetTextBox(bipLink.TPMSwitch, bipLink.WhenTurnedOn);
                    if (bipLink.BIPLights.Count > 0)
                    {
                        textBox.Bill.BIPLink = bipLink;
                    }
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox( ex);
            }
        }

        private void ButtonGetId_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_tpmPanel != null)
                {
                    TextBoxLogTPM.Text = "";
                    TextBoxLogTPM.Text = _tpmPanel.InstanceId;
                    Clipboard.SetText(_tpmPanel.InstanceId);
                    MessageBox.Show("The Instance Id for the panel has been copied to the Clipboard.");
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox( ex);
            }
        }


        private TPMPanelSwitchOnOff GetTPMSwitch(TPMTextBox textBox)
        {
            try
            {
                if (textBox.Equals(TextBoxG1Off))
                {
                    return new TPMPanelSwitchOnOff(TPMPanelSwitches.G1, false);
                }
                if (textBox.Equals(TextBoxG1On))
                {
                    return new TPMPanelSwitchOnOff(TPMPanelSwitches.G1, true);
                }
                if (textBox.Equals(TextBoxG2Off))
                {
                    return new TPMPanelSwitchOnOff(TPMPanelSwitches.G2, false);
                }
                if (textBox.Equals(TextBoxG2On))
                {
                    return new TPMPanelSwitchOnOff(TPMPanelSwitches.G2, true);
                }
                if (textBox.Equals(TextBoxG3Off))
                {
                    return new TPMPanelSwitchOnOff(TPMPanelSwitches.G3, false);
                }
                if (textBox.Equals(TextBoxG3On))
                {
                    return new TPMPanelSwitchOnOff(TPMPanelSwitches.G3, true);
                }
                if (textBox.Equals(TextBoxG4Off))
                {
                    return new TPMPanelSwitchOnOff(TPMPanelSwitches.G4, false);
                }
                if (textBox.Equals(TextBoxG4On))
                {
                    return new TPMPanelSwitchOnOff(TPMPanelSwitches.G4, true);
                }
                if (textBox.Equals(TextBoxG5Off))
                {
                    return new TPMPanelSwitchOnOff(TPMPanelSwitches.G5, false);
                }
                if (textBox.Equals(TextBoxG5On))
                {
                    return new TPMPanelSwitchOnOff(TPMPanelSwitches.G5, true);
                }
                if (textBox.Equals(TextBoxG6Off))
                {
                    return new TPMPanelSwitchOnOff(TPMPanelSwitches.G6, false);
                }
                if (textBox.Equals(TextBoxG6On))
                {
                    return new TPMPanelSwitchOnOff(TPMPanelSwitches.G6, true);
                }
                if (textBox.Equals(TextBoxG7Off))
                {
                    return new TPMPanelSwitchOnOff(TPMPanelSwitches.G7, false);
                }
                if (textBox.Equals(TextBoxG7On))
                {
                    return new TPMPanelSwitchOnOff(TPMPanelSwitches.G7, true);
                }
                if (textBox.Equals(TextBoxG8Off))
                {
                    return new TPMPanelSwitchOnOff(TPMPanelSwitches.G8, false);
                }
                if (textBox.Equals(TextBoxG8On))
                {
                    return new TPMPanelSwitchOnOff(TPMPanelSwitches.G8, true);
                }
                if (textBox.Equals(TextBoxG9Off))
                {
                    return new TPMPanelSwitchOnOff(TPMPanelSwitches.G9, false);
                }
                if (textBox.Equals(TextBoxG9On))
                {
                    return new TPMPanelSwitchOnOff(TPMPanelSwitches.G9, true);
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox( ex);
            }
            throw new Exception("Failed to find TPM switch for TextBox : " + textBox.Name);
        }

        private TPMTextBox GetTextBox(TPMPanelSwitches key, bool whenTurnedOn)
        {
            try
            {
                if (key == TPMPanelSwitches.G1 && !whenTurnedOn)
                {
                    return TextBoxG1Off;
                }
                if (key == TPMPanelSwitches.G1 && whenTurnedOn)
                {
                    return TextBoxG1On;
                }
                if (key == TPMPanelSwitches.G2 && !whenTurnedOn)
                {
                    return TextBoxG2Off;
                }
                if (key == TPMPanelSwitches.G2 && whenTurnedOn)
                {
                    return TextBoxG2On;
                }
                if (key == TPMPanelSwitches.G3 && !whenTurnedOn)
                {
                    return TextBoxG3Off;
                }
                if (key == TPMPanelSwitches.G3 && whenTurnedOn)
                {
                    return TextBoxG3On;
                }
                if (key == TPMPanelSwitches.G4 && !whenTurnedOn)
                {
                    return TextBoxG4Off;
                }
                if (key == TPMPanelSwitches.G4 && whenTurnedOn)
                {
                    return TextBoxG4On;
                }
                if (key == TPMPanelSwitches.G5 && !whenTurnedOn)
                {
                    return TextBoxG5Off;
                }
                if (key == TPMPanelSwitches.G5 && whenTurnedOn)
                {
                    return TextBoxG5On;
                }
                if (key == TPMPanelSwitches.G6 && !whenTurnedOn)
                {
                    return TextBoxG6Off;
                }
                if (key == TPMPanelSwitches.G6 && whenTurnedOn)
                {
                    return TextBoxG6On;
                }
                if (key == TPMPanelSwitches.G7 && !whenTurnedOn)
                {
                    return TextBoxG7Off;
                }
                if (key == TPMPanelSwitches.G7 && whenTurnedOn)
                {
                    return TextBoxG7On;
                }
                if (key == TPMPanelSwitches.G8 && !whenTurnedOn)
                {
                    return TextBoxG8Off;
                }
                if (key == TPMPanelSwitches.G8 && whenTurnedOn)
                {
                    return TextBoxG8On;
                }
                if (key == TPMPanelSwitches.G9 && !whenTurnedOn)
                {
                    return TextBoxG9Off;
                }
                if (key == TPMPanelSwitches.G9 && whenTurnedOn)
                {
                    return TextBoxG9On;
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
            throw new Exception("Failed to find TextBox for TPM switch : " + key);
        }


        private void MenuItemAddNullKey_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var textBox = GetTextBoxInFocus();
                if (textBox == null)
                {
                    throw new Exception("Failed to locate which textbox is focused.");
                }

                textBox.Bill.Clear();
                var vkNull = Enum.GetName(typeof(VirtualKeyCode), VirtualKeyCode.VK_NULL);
                if (string.IsNullOrEmpty(vkNull))
                {
                    return;
                }
                var keyPress = new KeyPress(vkNull, KeyPressLength.ThirtyTwoMilliSec);
                textBox.Bill.KeyPress = keyPress;
                textBox.Bill.KeyPress.Information = "VK_NULL";
                textBox.Text = vkNull;
                UpdateKeyBindingProfileSimpleKeyStrokes(textBox);
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void MenuContextEditOSCommandTextBoxClick_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var textBox = GetTextBoxInFocus();
                if (textBox == null)
                {
                    throw new Exception("Failed to locate which textbox is focused.");
                }
                OSCommandWindow osCommandWindow;
                if (textBox.Bill.ContainsOSCommand())
                {
                    osCommandWindow = new OSCommandWindow(textBox.Bill.OSCommandObject);
                }
                else
                {
                    osCommandWindow = new OSCommandWindow();
                }
                osCommandWindow.ShowDialog();
                if (osCommandWindow.DialogResult.HasValue && osCommandWindow.DialogResult.Value)
                {
                    //Clicked OK
                    if (!osCommandWindow.IsDirty)
                    {
                        //User made no changes
                        return;
                    }
                    var osCommand = osCommandWindow.OSCommandObject;
                    textBox.Bill.OSCommandObject = osCommand;
                    UpdateOSCommandBindingsTPM(textBox);
                    textBox.Text = osCommand.Name;
                }
                TextBoxLogTPM.Focus();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox( ex);
            }
        }

    }
}