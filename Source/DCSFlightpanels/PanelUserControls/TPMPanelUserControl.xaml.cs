using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ClassLibraryCommon;
using DCSFlightpanels.TagDataClasses;
using NonVisuals;

namespace DCSFlightpanels.PanelUserControls
{
    /// <summary>
    /// Interaction logic for TPMPanelUserControl.xaml
    /// </summary>
    public partial class TPMPanelUserControl : UserControlBase, IGamingPanelListener, IProfileHandlerListener, ISaitekUserControl
    {

        private readonly TPMPanel _tpmPanel;
        private readonly TabItem _parentTabItem;
        private string _parentTabItemHeader;
        private readonly IGlobalHandler _globalHandler;
        private bool _once;
        private bool _textBoxTagsSet;
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

        private void TPMPanelUserControl_OnLoaded(object sender, RoutedEventArgs e)
        {
            if (!_once)
            {
                HidePositionIndicators();
                _once = true;
            }
            var now = DateTime.Now.Ticks;
            SetTextBoxTagObjects();
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
                Common.ShowErrorMessageBox(400003, ex);
            }
        }

        public void BipPanelRegisterEvent(object sender, BipPanelRegisteredEventArgs e)
        {
            RemoveContextMenuClickHandlers();
            SetContextMenuClickHandlers();
        }

        public SaitekPanel GetSaitekPanel()
        {
            return _tpmPanel;
        }

        public string GetName()
        {
            return GetType().Name;
        }

        public void SelectedAirframe(object sender, AirframeEventArgs e)
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

        public void UpdatesHasBeenMissed(object sender, DCSBIOSUpdatesMissedEventArgs e)
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
                Common.ShowErrorMessageBox(1066, ex);
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
                Common.ShowErrorMessageBox(1067, ex);
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
                Common.ShowErrorMessageBox(1068, ex);
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
                Common.ShowErrorMessageBox(1085, ex);
            }
        }

        public void DeviceAttached(object sender, PanelEventArgs e)
        {
            try
            {
                if (e.GamingPanelEnum == GamingPanelEnum.TPM && e.UniqueId.Equals(_tpmPanel.InstanceId))
                {
                    //Dispatcher?.BeginInvoke((Action)(() => _parentTabItem.Header = _parentTabItemHeader + " (connected)"));
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(2008, ex);
            }
        }

        public void LedLightChanged(object sender, LedLightChangeEventArgs e)
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

        public void DeviceDetached(object sender, PanelEventArgs e)
        {
            try
            {
                if (e.GamingPanelEnum == GamingPanelEnum.TPM && e.UniqueId.Equals(_tpmPanel.InstanceId))
                {
                    //Dispatcher?.BeginInvoke((Action)(() => _parentTabItem.Header = _parentTabItemHeader + " (disconnected)"));
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(2031, ex);
            }
        }

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
                Common.ShowErrorMessageBox(2032, ex);
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
                KeySequenceWindow keySequenceWindow;
                if (((TagDataClassTPM)textBox.Tag).ContainsKeySequence())
                {
                    keySequenceWindow = new KeySequenceWindow(textBox.Text, ((TagDataClassTPM)textBox.Tag).GetKeySequence());
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
                        var osKeyPress = new KeyPress("Key press sequence", sequenceList);
                        ((TagDataClassTPM)textBox.Tag).KeyPress = osKeyPress;
                        ((TagDataClassTPM)textBox.Tag).KeyPress.Information = keySequenceWindow.GetInformation;
                        if (!string.IsNullOrEmpty(keySequenceWindow.GetInformation))
                        {
                            textBox.Text = keySequenceWindow.GetInformation;
                        }
                        UpdateKeyBindingProfileSequencedKeyStrokesTPM(textBox);
                    }
                    else
                    {
                        //If only one press was created treat it as a simple keypress
                        ((TagDataClassTPM)textBox.Tag).ClearAll();
                        var osKeyPress = new KeyPress(sequenceList[0].VirtualKeyCodesAsString, sequenceList[0].LengthOfKeyPress);
                        ((TagDataClassTPM)textBox.Tag).KeyPress = osKeyPress;
                        ((TagDataClassTPM)textBox.Tag).KeyPress.Information = keySequenceWindow.GetInformation;
                        textBox.Text = sequenceList[0].VirtualKeyCodesAsString;
                        UpdateKeyBindingProfileSimpleKeyStrokes(textBox);
                    }
                }
                TextBoxLogTPM.Focus();
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
                if (((TagDataClassTPM)textBox.Tag).ContainsDCSBIOS())
                {
                    dcsBIOSControlsConfigsWindow = new DCSBIOSControlsConfigsWindow(_globalHandler.GetAirframe(), textBox.Name.Replace("TextBox", ""), ((TagDataClassTPM)textBox.Tag).DCSBIOSBinding.DCSBIOSInputs, textBox.Text);
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
                    ((TagDataClassTPM)textBox.Tag).Consume(dcsBiosInputs);
                    UpdateDCSBIOSBinding(textBox);
                }
                TextBoxLogTPM.Focus();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(442044, ex);
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
                if (((TagDataClassTPM)textBox.Tag).ContainsBIPLink())
                {
                    var bipLink = ((TagDataClassTPM)textBox.Tag).BIPLink;
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
                    ((TagDataClassTPM)textBox.Tag).BIPLink = (BIPLinkTPM)bipLinkWindow.BIPLink;
                    UpdateBIPLinkBindings(textBox);
                }
                TextBoxLogTPM.Focus();
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
                if (!((TagDataClassTPM)textBox.Tag).ContainsSingleKey())
                {
                    return;
                }
                var keyPressLength = ((TagDataClassTPM)textBox.Tag).KeyPress.GetLengthOfKeyPress();
                CheckContextMenuItems(keyPressLength, contextMenu);

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
                if (textBox.Equals(TextBoxLogTPM))
                {
                    continue;
                }
                var tagHolderClass = (TagDataClassTPM)textBox.Tag;
                textBox.Text = "";
                tagHolderClass.ClearAll();
            }
            if (clearAlsoProfile)
            {
                _tpmPanel.ClearSettings();
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
                if (textBox.Equals(TextBoxLogTPM))
                {
                    continue;
                }
                textBox.Tag = new TagDataClassTPM(textBox, GetTPMSwitch(textBox));
            }
            _textBoxTagsSet = true;
        }

        private void RemoveContextMenuClickHandlers()
        {
            foreach (var textBox in Common.FindVisualChildren<TextBox>(this))
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
            foreach (var textBox in Common.FindVisualChildren<TextBox>(this))
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

                if (((TagDataClassTPM)textBox.Tag).ContainsDCSBIOS())
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
                else if (((TagDataClassTPM)textBox.Tag).ContainsKeySequence())
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
                else if (((TagDataClassTPM)textBox.Tag).IsEmpty())
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
                else if (((TagDataClassTPM)textBox.Tag).ContainsSingleKey())
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
                else if (((TagDataClassTPM)textBox.Tag).ContainsBIPLink())
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
                else if (((TagDataClassTPM)textBox.Tag).ContainsOSCommand())
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
                Common.ShowErrorMessageBox(2081, ex);
            }
        }

        private TextBox GetTextBoxInFocus()
        {
            foreach (var textBox in Common.FindVisualChildren<TextBox>(this))
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
                    if (((TagDataClassTPM)textBox.Tag).ContainsDCSBIOS())
                    {
                        if (MessageBox.Show("Do you want to delete the DCS-BIOS configuration?", "Delete DCS-BIOS configuration?", MessageBoxButton.OKCancel, MessageBoxImage.Question) != MessageBoxResult.OK)
                        {
                            return;
                        }
                        textBox.Text = "";
                        _tpmPanel.RemoveTPMPanelSwitchFromList(ControlListTPM.DCSBIOS, GetTPMSwitch(textBox).TPMSwitch, GetTPMSwitch(textBox).ButtonState);
                        ((TagDataClassTPM)textBox.Tag).DCSBIOSBinding = null;
                    }
                    else if (((TagDataClassTPM)textBox.Tag).ContainsKeySequence())
                    {
                        //Check if this textbox contains sequence information. If so then prompt the user for deletion
                        if (MessageBox.Show("Do you want to delete the key sequence?", "Delete key sequence?", MessageBoxButton.OKCancel, MessageBoxImage.Question) != MessageBoxResult.OK)
                        {
                            return;
                        }
                        ((TagDataClassTPM)textBox.Tag).KeyPress.KeySequence.Clear();
                        textBox.Text = "";
                        UpdateKeyBindingProfileSimpleKeyStrokes(textBox);
                    }
                    else if (((TagDataClassTPM)textBox.Tag).ContainsSingleKey())
                    {
                        ((TagDataClassTPM)textBox.Tag).KeyPress.KeySequence.Clear();
                        textBox.Text = "";
                        UpdateKeyBindingProfileSimpleKeyStrokes(textBox);
                    }
                    if (((TagDataClassTPM)textBox.Tag).ContainsBIPLink())
                    {
                        //Check if this textbox contains sequence information. If so then prompt the user for deletion
                        if (MessageBox.Show("Do you want to delete BIP Links?", "Delete BIP Link?", MessageBoxButton.OKCancel, MessageBoxImage.Question) != MessageBoxResult.OK)
                        {
                            return;
                        }
                        ((TagDataClassTPM)textBox.Tag).BIPLink.BIPLights.Clear();
                        textBox.Background = Brushes.White;
                        UpdateBIPLinkBindings(textBox);
                    }
                    TextBoxLogTPM.Focus();
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
                var textBox = (TextBox)sender;
                if (((TagDataClassTPM)textBox.Tag).ContainsBIPLink())
                {
                    ((TextBox)sender).Background = Brushes.Bisque;
                }
                else
                {
                    ((TextBox)sender).Background = Brushes.White;
                }
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
                if (((TagDataClassTPM)textBox.Tag).ContainsKeySequence() || ((TagDataClassTPM)textBox.Tag).ContainsDCSBIOS())
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


        private void TextBoxTextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                //MAKE SURE THE Tag iss SET BEFORE SETTING TEXT! OTHERWISE THIS DOESN'T FIRE
                var textBox = (TextBox)sender;
                if (((TagDataClassTPM)textBox.Tag).ContainsKeySequence())
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
                if (((TagDataClassTPM)textBox.Tag).ContainsKeySequence() || ((TagDataClassTPM)textBox.Tag).ContainsDCSBIOS())
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
                Common.ShowErrorMessageBox(3008, ex);
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
                Common.ShowErrorMessageBox(3010, ex);
            }
        }

        private void UpdateKeyBindingProfileSequencedKeyStrokesTPM(TextBox textBox)
        {
            try
            {
                var tag = (TagDataClassTPM)textBox.Tag;
                _tpmPanel.AddOrUpdateSequencedKeyBinding(textBox.Text, tag.Key.TPMSwitch, tag.GetKeySequence(), tag.Key.ButtonState);
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(3011, ex);
            }
        }

        private void UpdateBIPLinkBindings(TextBox textBox)
        {
            try
            {
                var tag = (TagDataClassTPM)textBox.Tag;
                _tpmPanel.AddOrUpdateBIPLinkKeyBinding(tag.Key.TPMSwitch, tag.BIPLink, tag.Key.ButtonState);
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
                if (!((TagDataClassTPM)textBox.Tag).ContainsOSKeyPress() || ((TagDataClassTPM)textBox.Tag).KeyPress.KeySequence.Count == 0)
                {
                    keyPressLength = KeyPressLength.FiftyMilliSec;
                }
                else
                {
                    keyPressLength = ((TagDataClassTPM)textBox.Tag).KeyPress.GetLengthOfKeyPress();
                }
                var tag = (TagDataClassTPM)textBox.Tag;
                _tpmPanel.AddOrUpdateSingleKeyBinding(tag.Key.TPMSwitch, textBox.Text, keyPressLength, tag.Key.ButtonState);
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(3012, ex);
            }
        }

        private void UpdateOSCommandBindingsPZ55(TextBox textBox)
        {
            try
            {
                var tag = (TagDataClassTPM)textBox.Tag;
                _tpmPanel.AddOrUpdateOSCommandBinding(tag.Key.TPMSwitch, tag.OSCommandObject, tag.Key.ButtonState);
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(3011, ex);
            }
        }

        private void UpdateDCSBIOSBinding(TextBox textBox)
        {
            try
            {
                var tag = (TagDataClassTPM)textBox.Tag;
                _tpmPanel.AddOrUpdateDCSBIOSBinding(tag.Key.TPMSwitch, tag.DCSBIOSBinding.DCSBIOSInputs, textBox.Text, tag.Key.ButtonState);
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(345012, ex);
            }
        }


        private void ShowGraphicConfiguration()
        {
            try
            {
                if (!_controlLoaded || !_textBoxTagsSet)
                {
                    return;
                }
                foreach (var keyBinding in _tpmPanel.KeyBindingsHashSet)
                {
                    var textBox = GetTextBox(keyBinding.TPMSwitch, keyBinding.WhenTurnedOn);
                    if (keyBinding.OSKeyPress != null)
                    {
                        ((TagDataClassTPM)textBox.Tag).KeyPress = keyBinding.OSKeyPress;
                    }
                }

                foreach (var osCommand in _tpmPanel.OSCommandHashSet)
                {
                    var textBox = GetTextBox(osCommand.TPMSwitch, osCommand.WhenTurnedOn);
                    if (osCommand.OSCommandObject != null)
                    {
                        ((TagDataClassTPM)textBox.Tag).OSCommandObject = osCommand.OSCommandObject;
                    }
                }

                foreach (var dcsBiosBinding in _tpmPanel.DCSBiosBindings)
                {
                    var textBox = GetTextBox(dcsBiosBinding.TPMSwitch, dcsBiosBinding.WhenTurnedOn);
                    if (dcsBiosBinding.DCSBIOSInputs.Count > 0)
                    {
                        ((TagDataClassTPM)textBox.Tag).DCSBIOSBinding = dcsBiosBinding;
                    }
                }

                foreach (var bipLink in _tpmPanel.BipLinkHashSet)
                {
                    var textBox = GetTextBox(bipLink.TPMSwitch, bipLink.WhenTurnedOn);
                    if (bipLink.BIPLights.Count > 0)
                    {
                        ((TagDataClassTPM)textBox.Tag).BIPLink = bipLink;
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
                    TextBoxLogTPM.Text = "";
                    TextBoxLogTPM.Text = _tpmPanel.InstanceId;
                    Clipboard.SetText(_tpmPanel.InstanceId);
                    MessageBox.Show("The Instance Id for the panel has been copied to the Clipboard.");
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(3015, ex);
            }
        }


        private TPMPanelSwitchOnOff GetTPMSwitch(TextBox textBox)
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
                Common.ShowErrorMessageBox(345012, ex);
            }
            throw new Exception("Failed to find TPM switch for TextBox : " + textBox.Name);
        }

        private TextBox GetTextBox(TPMPanelSwitches key, bool whenTurnedOn)
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
                Common.ShowErrorMessageBox(345012, ex);
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

                ((TagDataClassTPM)textBox.Tag).ClearAll();
                var vkNull = Enum.GetName(typeof(VirtualKeyCode), VirtualKeyCode.VK_NULL);
                if (string.IsNullOrEmpty(vkNull))
                {
                    return;
                }
                var osKeyPress = new KeyPress(vkNull, KeyPressLength.FiftyMilliSec);
                ((TagDataClassTPM)textBox.Tag).KeyPress = osKeyPress;
                ((TagDataClassTPM)textBox.Tag).KeyPress.Information = "VK_NULL";
                textBox.Text = vkNull;
                UpdateKeyBindingProfileSimpleKeyStrokes(textBox);
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(2039, ex);
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
                if (((TagDataClassTPM)textBox.Tag).ContainsOSCommand())
                {
                    osCommandWindow = new OSCommandWindow(((TagDataClassTPM)textBox.Tag).OSCommandObject);
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
                    ((TagDataClassTPM)textBox.Tag).OSCommandObject = osCommand;
                    UpdateOSCommandBindingsPZ55(textBox);
                    textBox.Text = osCommand.Name;
                }
                TextBoxLogTPM.Focus();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(2044, ex);
            }
        }

    }
}