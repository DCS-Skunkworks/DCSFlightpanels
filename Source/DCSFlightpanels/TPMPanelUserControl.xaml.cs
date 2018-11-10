using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
    /// Interaction logic for TPMPanelUserControl.xaml
    /// </summary>
    public partial class TPMPanelUserControl : ISaitekPanelListener, IProfileHandlerListener, ISaitekUserControl
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
            var now = DateTime.Now.Ticks;
            Debug.WriteLine("Start TPMPanelUserControl_OnLoaded");
            SetTextBoxTagObjects();
            SetContextMenuClickHandlers();
            Debug.WriteLine("End TPMPanelUserControl_OnLoaded" + new TimeSpan(DateTime.Now.Ticks - now).Milliseconds);
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
                if (e.SaitekPanelEnum == SaitekPanelsEnum.TPM && e.UniqueId.Equals(_tpmPanel.InstanceId))
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
                if (e.SaitekPanelEnum == SaitekPanelsEnum.TPM && e.UniqueId.Equals(_tpmPanel.InstanceId))
                {
                    //Dispatcher.BeginInvoke((Action)(() => _parentTabItem.Header = _parentTabItemHeader + " (connected)"));
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
                if (e.SaitekPanelEnum == SaitekPanelsEnum.TPM && e.UniqueId.Equals(_tpmPanel.InstanceId))
                {
                    //Dispatcher.BeginInvoke((Action)(() => _parentTabItem.Header = _parentTabItemHeader + " (disconnected)"));
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
                if (e.UniqueId.Equals(_tpmPanel.InstanceId) && e.SaitekPanelEnum == SaitekPanelsEnum.TPM)
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
                if (((TagDataClassTPM)textBox.Tag).ContainsKeySequence())
                {
                    sequenceWindow = new SequenceWindow(textBox.Text, ((TagDataClassTPM)textBox.Tag).GetKeySequence());
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
                    if (sequenceList.Count > 1)
                    {
                        var osKeyPress = new OSKeyPress("Key press sequence", sequenceList);
                        ((TagDataClassTPM)textBox.Tag).KeyPress = osKeyPress;
                        ((TagDataClassTPM)textBox.Tag).KeyPress.Information = sequenceWindow.GetInformation;
                        if (!string.IsNullOrEmpty(sequenceWindow.GetInformation))
                        {
                            textBox.Text = sequenceWindow.GetInformation;
                        }
                        UpdateKeyBindingProfileSequencedKeyStrokesTPM(textBox);
                    }
                    else
                    {
                        //If only one press was created treat it as a simple keypress
                        ((TagDataClassTPM)textBox.Tag).ClearAll();
                        var osKeyPress = new OSKeyPress(sequenceList[0].VirtualKeyCodesAsString, sequenceList[0].LengthOfKeyPress);
                        ((TagDataClassTPM)textBox.Tag).KeyPress = osKeyPress;
                        ((TagDataClassTPM)textBox.Tag).KeyPress.Information = sequenceWindow.GetInformation;
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
                    dcsBIOSControlsConfigsWindow = new DCSBIOSControlsConfigsWindow(_globalHandler.GetAirframe(), textBox.Name.Replace("TextBox", ""), ((TagDataClassTPM)textBox.Tag).DCSBIOSInputs, textBox.Text);
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
                    ((TagDataClassTPM)textBox.Tag).DCSBIOSInputs = dcsBiosInputs;
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
                textBox.Tag = new TagDataClassTPM();
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
                if (textBox == null)
                {
                    throw new Exception("Failed to locate which textbox is focused.");
                }

                var contextMenuItem = (MenuItem)sender;
                if (contextMenuItem.Name == "contextMenuItemFiftyMilliSec")
                {
                    ((TagDataClassTPM)textBox.Tag).KeyPress.SetLengthOfKeyPress(KeyPressLength.FiftyMilliSec);
                }
                else if (contextMenuItem.Name == "contextMenuItemHalfSecond")
                {
                    ((TagDataClassTPM)textBox.Tag).KeyPress.SetLengthOfKeyPress(KeyPressLength.HalfSecond);
                }
                else if (contextMenuItem.Name == "contextMenuItemSecond")
                {
                    ((TagDataClassTPM)textBox.Tag).KeyPress.SetLengthOfKeyPress(KeyPressLength.Second);
                }
                else if (contextMenuItem.Name == "contextMenuItemSecondAndHalf")
                {
                    ((TagDataClassTPM)textBox.Tag).KeyPress.SetLengthOfKeyPress(KeyPressLength.SecondAndHalf);
                }
                else if (contextMenuItem.Name == "contextMenuItemTwoSeconds")
                {
                    ((TagDataClassTPM)textBox.Tag).KeyPress.SetLengthOfKeyPress(KeyPressLength.TwoSeconds);
                }
                else if (contextMenuItem.Name == "contextMenuItemThreeSeconds")
                {
                    ((TagDataClassTPM)textBox.Tag).KeyPress.SetLengthOfKeyPress(KeyPressLength.ThreeSeconds);
                }
                else if (contextMenuItem.Name == "contextMenuItemFourSeconds")
                {
                    ((TagDataClassTPM)textBox.Tag).KeyPress.SetLengthOfKeyPress(KeyPressLength.FourSeconds);
                }
                else if (contextMenuItem.Name == "contextMenuItemFiveSecs")
                {
                    ((TagDataClassTPM)textBox.Tag).KeyPress.SetLengthOfKeyPress(KeyPressLength.FiveSecs);
                }
                else if (contextMenuItem.Name == "contextMenuItemTenSecs")
                {
                    ((TagDataClassTPM)textBox.Tag).KeyPress.SetLengthOfKeyPress(KeyPressLength.TenSecs);
                }
                else if (contextMenuItem.Name == "contextMenuItemFifteenSecs")
                {
                    ((TagDataClassTPM)textBox.Tag).KeyPress.SetLengthOfKeyPress(KeyPressLength.FifteenSecs);
                }
                else if (contextMenuItem.Name == "contextMenuItemTwentySecs")
                {
                    ((TagDataClassTPM)textBox.Tag).KeyPress.SetLengthOfKeyPress(KeyPressLength.TwentySecs);
                }
                else if (contextMenuItem.Name == "contextMenuItemThirtySecs")
                {
                    ((TagDataClassTPM)textBox.Tag).KeyPress.SetLengthOfKeyPress(KeyPressLength.ThirtySecs);
                }
                else if (contextMenuItem.Name == "contextMenuItemFortySecs")
                {
                    ((TagDataClassTPM)textBox.Tag).KeyPress.SetLengthOfKeyPress(KeyPressLength.FortySecs);
                }
                else if (contextMenuItem.Name == "contextMenuItemSixtySecs")
                {
                    ((TagDataClassTPM)textBox.Tag).KeyPress.SetLengthOfKeyPress(KeyPressLength.SixtySecs);
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
                    if (((TagDataClassTPM)textBox.Tag).ContainsDCSBIOS())
                    {
                        if (MessageBox.Show("Do you want to delete the DCS-BIOS configuration?", "Delete DCS-BIOS configuration?", MessageBoxButton.OKCancel, MessageBoxImage.Question) != MessageBoxResult.OK)
                        {
                            return;
                        }
                        textBox.Text = "";
                        _tpmPanel.RemoveTPMPanelSwitchFromList(ControlListTPM.DCSBIOS, GetTPMSwitch(textBox).TPMSwitch, GetTPMSwitch(textBox).On);
                        ((TagDataClassTPM)textBox.Tag).DCSBIOSInputs.Clear();
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
                Dispatcher.BeginInvoke((Action)(() => TextBoxLogTPM.Focus()));
                foreach (var tpmPanelSwitch in switches)
                {
                    var key = (TPMPanelSwitch)tpmPanelSwitch;

                    if (_tpmPanel.ForwardPanelEvent)
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
                             TextBoxLogTPM.Text = TextBoxLogTPM.Text.Insert(0, "No action taken, panel events Disabled.\n")));
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
                if (textBox.Equals(TextBoxG1Off))
                {
                    _tpmPanel.AddOrUpdateSequencedKeyBinding(textBox.Text, TPMPanelSwitches.G1, ((TagDataClassTPM)textBox.Tag).GetKeySequence(), false);
                }
                if (textBox.Equals(TextBoxG1On))
                {
                    _tpmPanel.AddOrUpdateSequencedKeyBinding(textBox.Text, TPMPanelSwitches.G1, ((TagDataClassTPM)textBox.Tag).GetKeySequence());
                }
                if (textBox.Equals(TextBoxG2Off))
                {
                    _tpmPanel.AddOrUpdateSequencedKeyBinding(textBox.Text, TPMPanelSwitches.G2, ((TagDataClassTPM)textBox.Tag).GetKeySequence(), false);
                }
                if (textBox.Equals(TextBoxG2On))
                {
                    _tpmPanel.AddOrUpdateSequencedKeyBinding(textBox.Text, TPMPanelSwitches.G2, ((TagDataClassTPM)textBox.Tag).GetKeySequence());
                }
                if (textBox.Equals(TextBoxG3Off))
                {
                    _tpmPanel.AddOrUpdateSequencedKeyBinding(textBox.Text, TPMPanelSwitches.G3, ((TagDataClassTPM)textBox.Tag).GetKeySequence(), false);
                }
                if (textBox.Equals(TextBoxG3On))
                {
                    _tpmPanel.AddOrUpdateSequencedKeyBinding(textBox.Text, TPMPanelSwitches.G3, ((TagDataClassTPM)textBox.Tag).GetKeySequence());
                }
                if (textBox.Equals(TextBoxG4Off))
                {
                    _tpmPanel.AddOrUpdateSequencedKeyBinding(textBox.Text, TPMPanelSwitches.G4, ((TagDataClassTPM)textBox.Tag).GetKeySequence(), false);
                }
                if (textBox.Equals(TextBoxG4On))
                {
                    _tpmPanel.AddOrUpdateSequencedKeyBinding(textBox.Text, TPMPanelSwitches.G4, ((TagDataClassTPM)textBox.Tag).GetKeySequence());
                }
                if (textBox.Equals(TextBoxG5Off))
                {
                    _tpmPanel.AddOrUpdateSequencedKeyBinding(textBox.Text, TPMPanelSwitches.G5, ((TagDataClassTPM)textBox.Tag).GetKeySequence(), false);
                }
                if (textBox.Equals(TextBoxG5On))
                {
                    _tpmPanel.AddOrUpdateSequencedKeyBinding(textBox.Text, TPMPanelSwitches.G5, ((TagDataClassTPM)textBox.Tag).GetKeySequence());
                }
                if (textBox.Equals(TextBoxG6Off))
                {
                    _tpmPanel.AddOrUpdateSequencedKeyBinding(textBox.Text, TPMPanelSwitches.G6, ((TagDataClassTPM)textBox.Tag).GetKeySequence(), false);
                }
                if (textBox.Equals(TextBoxG6On))
                {
                    _tpmPanel.AddOrUpdateSequencedKeyBinding(textBox.Text, TPMPanelSwitches.G6, ((TagDataClassTPM)textBox.Tag).GetKeySequence());
                }
                if (textBox.Equals(TextBoxG7Off))
                {
                    _tpmPanel.AddOrUpdateSequencedKeyBinding(textBox.Text, TPMPanelSwitches.G7, ((TagDataClassTPM)textBox.Tag).GetKeySequence(), false);
                }
                if (textBox.Equals(TextBoxG7On))
                {
                    _tpmPanel.AddOrUpdateSequencedKeyBinding(textBox.Text, TPMPanelSwitches.G7, ((TagDataClassTPM)textBox.Tag).GetKeySequence());
                }
                if (textBox.Equals(TextBoxG8Off))
                {
                    _tpmPanel.AddOrUpdateSequencedKeyBinding(textBox.Text, TPMPanelSwitches.G8, ((TagDataClassTPM)textBox.Tag).GetKeySequence(), false);
                }
                if (textBox.Equals(TextBoxG8On))
                {
                    _tpmPanel.AddOrUpdateSequencedKeyBinding(textBox.Text, TPMPanelSwitches.G8, ((TagDataClassTPM)textBox.Tag).GetKeySequence());
                }
                if (textBox.Equals(TextBoxG9Off))
                {
                    _tpmPanel.AddOrUpdateSequencedKeyBinding(textBox.Text, TPMPanelSwitches.G9, ((TagDataClassTPM)textBox.Tag).GetKeySequence(), false);
                }
                if (textBox.Equals(TextBoxG9On))
                {
                    _tpmPanel.AddOrUpdateSequencedKeyBinding(textBox.Text, TPMPanelSwitches.G9, ((TagDataClassTPM)textBox.Tag).GetKeySequence());
                }
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
                if (textBox.Equals(TextBoxG1Off))
                {
                    _tpmPanel.AddOrUpdateBIPLinkKeyBinding(TPMPanelSwitches.G1, ((TagDataClassTPM)textBox.Tag).BIPLink, false);
                }
                if (textBox.Equals(TextBoxG1On))
                {
                    _tpmPanel.AddOrUpdateBIPLinkKeyBinding(TPMPanelSwitches.G1, ((TagDataClassTPM)textBox.Tag).BIPLink);
                }
                if (textBox.Equals(TextBoxG2Off))
                {
                    _tpmPanel.AddOrUpdateBIPLinkKeyBinding(TPMPanelSwitches.G2, ((TagDataClassTPM)textBox.Tag).BIPLink, false);
                }
                if (textBox.Equals(TextBoxG2On))
                {
                    _tpmPanel.AddOrUpdateBIPLinkKeyBinding(TPMPanelSwitches.G2, ((TagDataClassTPM)textBox.Tag).BIPLink);
                }
                if (textBox.Equals(TextBoxG3Off))
                {
                    _tpmPanel.AddOrUpdateBIPLinkKeyBinding(TPMPanelSwitches.G3, ((TagDataClassTPM)textBox.Tag).BIPLink, false);
                }
                if (textBox.Equals(TextBoxG3On))
                {
                    _tpmPanel.AddOrUpdateBIPLinkKeyBinding(TPMPanelSwitches.G3, ((TagDataClassTPM)textBox.Tag).BIPLink);
                }
                if (textBox.Equals(TextBoxG4Off))
                {
                    _tpmPanel.AddOrUpdateBIPLinkKeyBinding(TPMPanelSwitches.G4, ((TagDataClassTPM)textBox.Tag).BIPLink, false);
                }
                if (textBox.Equals(TextBoxG4On))
                {
                    _tpmPanel.AddOrUpdateBIPLinkKeyBinding(TPMPanelSwitches.G4, ((TagDataClassTPM)textBox.Tag).BIPLink);
                }
                if (textBox.Equals(TextBoxG5Off))
                {
                    _tpmPanel.AddOrUpdateBIPLinkKeyBinding(TPMPanelSwitches.G5, ((TagDataClassTPM)textBox.Tag).BIPLink, false);
                }
                if (textBox.Equals(TextBoxG5On))
                {
                    _tpmPanel.AddOrUpdateBIPLinkKeyBinding(TPMPanelSwitches.G5, ((TagDataClassTPM)textBox.Tag).BIPLink);
                }
                if (textBox.Equals(TextBoxG6Off))
                {
                    _tpmPanel.AddOrUpdateBIPLinkKeyBinding(TPMPanelSwitches.G6, ((TagDataClassTPM)textBox.Tag).BIPLink, false);
                }
                if (textBox.Equals(TextBoxG6On))
                {
                    _tpmPanel.AddOrUpdateBIPLinkKeyBinding(TPMPanelSwitches.G6, ((TagDataClassTPM)textBox.Tag).BIPLink);
                }
                if (textBox.Equals(TextBoxG7Off))
                {
                    _tpmPanel.AddOrUpdateBIPLinkKeyBinding(TPMPanelSwitches.G7, ((TagDataClassTPM)textBox.Tag).BIPLink, false);
                }
                if (textBox.Equals(TextBoxG7On))
                {
                    _tpmPanel.AddOrUpdateBIPLinkKeyBinding(TPMPanelSwitches.G7, ((TagDataClassTPM)textBox.Tag).BIPLink);
                }
                if (textBox.Equals(TextBoxG8Off))
                {
                    _tpmPanel.AddOrUpdateBIPLinkKeyBinding(TPMPanelSwitches.G8, ((TagDataClassTPM)textBox.Tag).BIPLink, false);
                }
                if (textBox.Equals(TextBoxG8On))
                {
                    _tpmPanel.AddOrUpdateBIPLinkKeyBinding(TPMPanelSwitches.G8, ((TagDataClassTPM)textBox.Tag).BIPLink);
                }
                if (textBox.Equals(TextBoxG9Off))
                {
                    _tpmPanel.AddOrUpdateBIPLinkKeyBinding(TPMPanelSwitches.G9, ((TagDataClassTPM)textBox.Tag).BIPLink, false);
                }
                if (textBox.Equals(TextBoxG9On))
                {
                    _tpmPanel.AddOrUpdateBIPLinkKeyBinding(TPMPanelSwitches.G9, ((TagDataClassTPM)textBox.Tag).BIPLink);
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
                if (!((TagDataClassTPM)textBox.Tag).ContainsOSKeyPress() || ((TagDataClassTPM)textBox.Tag).KeyPress.KeySequence.Count == 0)
                {
                    keyPressLength = KeyPressLength.FiftyMilliSec;
                }
                else
                {
                    keyPressLength = ((TagDataClassTPM)textBox.Tag).KeyPress.GetLengthOfKeyPress();
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
                if (((TagDataClassTPM)textBox.Tag).ContainsDCSBIOS())
                {
                    dcsBiosInputs = ((TagDataClassTPM)textBox.Tag).DCSBIOSInputs;
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
                if (!_controlLoaded || !_textBoxTagsSet)
                {
                    return;
                }
                foreach (var keyBinding in _tpmPanel.KeyBindingsHashSet)
                {
                    if (keyBinding.TPMSwitch == TPMPanelSwitches.G1)
                    {
                        if (keyBinding.WhenTurnedOn)
                        {
                            if (keyBinding.OSKeyPress != null)
                            {
                                ((TagDataClassTPM)TextBoxG1On.Tag).KeyPress = keyBinding.OSKeyPress;
                                TextBoxG1On.Text = ((TagDataClassTPM)TextBoxG1On.Tag).GetTextBoxKeyPressInfo();
                            }
                        }
                        else
                        {
                            if (keyBinding.OSKeyPress != null)
                            {
                                ((TagDataClassTPM)TextBoxG1Off.Tag).KeyPress = keyBinding.OSKeyPress;
                                TextBoxG1Off.Text = ((TagDataClassTPM)TextBoxG1Off.Tag).GetTextBoxKeyPressInfo();
                            }
                        }
                    }
                    if (keyBinding.TPMSwitch == TPMPanelSwitches.G2)
                    {
                        if (keyBinding.WhenTurnedOn)
                        {
                            if (keyBinding.OSKeyPress != null)
                            {
                                ((TagDataClassTPM)TextBoxG2On.Tag).KeyPress = keyBinding.OSKeyPress;
                                TextBoxG2On.Text = ((TagDataClassTPM)TextBoxG2On.Tag).GetTextBoxKeyPressInfo();
                            }
                        }
                        else
                        {
                            if (keyBinding.OSKeyPress != null && !keyBinding.OSKeyPress.IsMultiSequenced())
                            {
                                ((TagDataClassTPM)TextBoxG2Off.Tag).KeyPress = keyBinding.OSKeyPress;
                                TextBoxG2Off.Text = ((TagDataClassTPM)TextBoxG2Off.Tag).GetTextBoxKeyPressInfo();
                            }
                        }
                    }
                    if (keyBinding.TPMSwitch == TPMPanelSwitches.G3)
                    {
                        if (keyBinding.WhenTurnedOn)
                        {
                            if (keyBinding.OSKeyPress != null)
                            {
                                ((TagDataClassTPM)TextBoxG3On.Tag).KeyPress = keyBinding.OSKeyPress;
                                TextBoxG3On.Text = ((TagDataClassTPM)TextBoxG3On.Tag).GetTextBoxKeyPressInfo();
                            }
                        }
                        else
                        {
                            if (keyBinding.OSKeyPress != null)
                            {
                                ((TagDataClassTPM)TextBoxG3Off.Tag).KeyPress = keyBinding.OSKeyPress;
                                TextBoxG3Off.Text = ((TagDataClassTPM)TextBoxG3Off.Tag).GetTextBoxKeyPressInfo();
                            }
                        }
                    }
                    if (keyBinding.TPMSwitch == TPMPanelSwitches.G4)
                    {
                        if (keyBinding.WhenTurnedOn)
                        {
                            if (keyBinding.OSKeyPress != null)
                            {
                                ((TagDataClassTPM)TextBoxG4On.Tag).KeyPress = keyBinding.OSKeyPress;
                                TextBoxG4On.Text = ((TagDataClassTPM)TextBoxG4On.Tag).GetTextBoxKeyPressInfo();
                            }
                        }
                        else
                        {
                            if (keyBinding.OSKeyPress != null)
                            {
                                ((TagDataClassTPM)TextBoxG4Off.Tag).KeyPress = keyBinding.OSKeyPress;
                                TextBoxG4Off.Text = ((TagDataClassTPM)TextBoxG4Off.Tag).GetTextBoxKeyPressInfo();
                            }
                        }
                    }
                    if (keyBinding.TPMSwitch == TPMPanelSwitches.G5)
                    {
                        if (keyBinding.WhenTurnedOn)
                        {
                            if (keyBinding.OSKeyPress != null)
                            {
                                ((TagDataClassTPM)TextBoxG5On.Tag).KeyPress = keyBinding.OSKeyPress;
                                TextBoxG5On.Text = ((TagDataClassTPM)TextBoxG5On.Tag).GetTextBoxKeyPressInfo();
                            }
                        }
                        else
                        {
                            if (keyBinding.OSKeyPress != null)
                            {
                                ((TagDataClassTPM)TextBoxG5Off.Tag).KeyPress = keyBinding.OSKeyPress;
                                TextBoxG5Off.Text = ((TagDataClassTPM)TextBoxG5Off.Tag).GetTextBoxKeyPressInfo();
                            }
                        }
                    }
                    if (keyBinding.TPMSwitch == TPMPanelSwitches.G6)
                    {
                        if (keyBinding.WhenTurnedOn)
                        {
                            if (keyBinding.OSKeyPress != null)
                            {
                                ((TagDataClassTPM)TextBoxG6On.Tag).KeyPress = keyBinding.OSKeyPress;
                                TextBoxG6On.Text = ((TagDataClassTPM)TextBoxG6On.Tag).GetTextBoxKeyPressInfo();
                            }
                        }
                        else
                        {
                            if (keyBinding.OSKeyPress != null)
                            {
                                ((TagDataClassTPM)TextBoxG6Off.Tag).KeyPress = keyBinding.OSKeyPress;
                                TextBoxG6Off.Text = ((TagDataClassTPM)TextBoxG6Off.Tag).GetTextBoxKeyPressInfo();
                            }
                        }
                    }
                    if (keyBinding.TPMSwitch == TPMPanelSwitches.G7)
                    {
                        if (keyBinding.WhenTurnedOn)
                        {
                            if (keyBinding.OSKeyPress != null)
                            {
                                ((TagDataClassTPM)TextBoxG7On.Tag).KeyPress = keyBinding.OSKeyPress;
                                TextBoxG7On.Text = ((TagDataClassTPM)TextBoxG7On.Tag).GetTextBoxKeyPressInfo();
                            }
                        }
                        else
                        {
                            if (keyBinding.OSKeyPress != null)
                            {
                                ((TagDataClassTPM)TextBoxG7Off.Tag).KeyPress = keyBinding.OSKeyPress;
                                TextBoxG7Off.Text = ((TagDataClassTPM)TextBoxG7Off.Tag).GetTextBoxKeyPressInfo();
                            }
                        }
                    }
                    if (keyBinding.TPMSwitch == TPMPanelSwitches.G8)
                    {
                        if (keyBinding.WhenTurnedOn)
                        {
                            if (keyBinding.OSKeyPress != null)
                            {
                                ((TagDataClassTPM)TextBoxG8On.Tag).KeyPress = keyBinding.OSKeyPress;
                                TextBoxG8On.Text = ((TagDataClassTPM)TextBoxG8On.Tag).GetTextBoxKeyPressInfo();
                            }
                        }
                        else
                        {
                            if (keyBinding.OSKeyPress != null)
                            {
                                ((TagDataClassTPM)TextBoxG8Off.Tag).KeyPress = keyBinding.OSKeyPress;
                                TextBoxG8Off.Text = ((TagDataClassTPM)TextBoxG8Off.Tag).GetTextBoxKeyPressInfo();
                            }
                        }
                    }
                    if (keyBinding.TPMSwitch == TPMPanelSwitches.G9)
                    {
                        if (keyBinding.WhenTurnedOn)
                        {
                            if (keyBinding.OSKeyPress != null)
                            {
                                ((TagDataClassTPM)TextBoxG9On.Tag).KeyPress = keyBinding.OSKeyPress;
                                TextBoxG9On.Text = ((TagDataClassTPM)TextBoxG9On.Tag).GetTextBoxKeyPressInfo();
                            }
                        }
                        else
                        {
                            if (keyBinding.OSKeyPress != null)
                            {
                                ((TagDataClassTPM)TextBoxG9Off.Tag).KeyPress = keyBinding.OSKeyPress;
                                TextBoxG9Off.Text = ((TagDataClassTPM)TextBoxG9Off.Tag).GetTextBoxKeyPressInfo();
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
                                ((TagDataClassTPM)TextBoxG1On.Tag).DCSBIOSInputs = dcsBiosBinding.DCSBIOSInputs;
                                TextBoxG1On.Text = dcsBiosBinding.Description;
                                TextBoxG1On.ToolTip = "DCS-BIOS";
                            }
                        }
                        else
                        {
                            if (dcsBiosBinding.DCSBIOSInputs.Count > 0)
                            {
                                ((TagDataClassTPM)TextBoxG1Off.Tag).DCSBIOSInputs = dcsBiosBinding.DCSBIOSInputs;
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
                                ((TagDataClassTPM)TextBoxG2On.Tag).DCSBIOSInputs = dcsBiosBinding.DCSBIOSInputs;
                                TextBoxG2On.Text = dcsBiosBinding.Description;
                            }
                        }
                        else
                        {
                            if (dcsBiosBinding.DCSBIOSInputs.Count > 0)
                            {
                                ((TagDataClassTPM)TextBoxG2Off.Tag).DCSBIOSInputs = dcsBiosBinding.DCSBIOSInputs;
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
                                ((TagDataClassTPM)TextBoxG3On.Tag).DCSBIOSInputs = dcsBiosBinding.DCSBIOSInputs;
                                TextBoxG3On.Text = dcsBiosBinding.Description;
                                TextBoxG3On.ToolTip = "DCS-BIOS";
                            }
                        }
                        else
                        {
                            if (dcsBiosBinding.DCSBIOSInputs.Count > 0)
                            {
                                ((TagDataClassTPM)TextBoxG3Off.Tag).DCSBIOSInputs = dcsBiosBinding.DCSBIOSInputs;
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
                                ((TagDataClassTPM)TextBoxG4On.Tag).DCSBIOSInputs = dcsBiosBinding.DCSBIOSInputs;
                                TextBoxG4On.Text = dcsBiosBinding.Description;
                                TextBoxG4On.ToolTip = "DCS-BIOS";
                            }
                        }
                        else
                        {
                            if (dcsBiosBinding.DCSBIOSInputs.Count > 0)
                            {
                                ((TagDataClassTPM)TextBoxG4Off.Tag).DCSBIOSInputs = dcsBiosBinding.DCSBIOSInputs;
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
                                ((TagDataClassTPM)TextBoxG5On.Tag).DCSBIOSInputs = dcsBiosBinding.DCSBIOSInputs;
                                TextBoxG5On.Text = dcsBiosBinding.Description;
                                TextBoxG5On.ToolTip = "DCS-BIOS";
                            }
                        }
                        else
                        {
                            if (dcsBiosBinding.DCSBIOSInputs.Count > 0)
                            {
                                ((TagDataClassTPM)TextBoxG5Off.Tag).DCSBIOSInputs = dcsBiosBinding.DCSBIOSInputs;
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
                                ((TagDataClassTPM)TextBoxG6On.Tag).DCSBIOSInputs = dcsBiosBinding.DCSBIOSInputs;
                                TextBoxG6On.Text = dcsBiosBinding.Description;
                                TextBoxG6On.ToolTip = "DCS-BIOS";
                            }
                        }
                        else
                        {
                            if (dcsBiosBinding.DCSBIOSInputs.Count > 0)
                            {
                                ((TagDataClassTPM)TextBoxG6Off.Tag).DCSBIOSInputs = dcsBiosBinding.DCSBIOSInputs;
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
                                ((TagDataClassTPM)TextBoxG7On.Tag).DCSBIOSInputs = dcsBiosBinding.DCSBIOSInputs;
                                TextBoxG7On.Text = dcsBiosBinding.Description;
                                TextBoxG7On.ToolTip = "DCS-BIOS";
                            }
                        }
                        else
                        {
                            if (dcsBiosBinding.DCSBIOSInputs.Count > 0)
                            {
                                ((TagDataClassTPM)TextBoxG7Off.Tag).DCSBIOSInputs = dcsBiosBinding.DCSBIOSInputs;
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
                                ((TagDataClassTPM)TextBoxG8On.Tag).DCSBIOSInputs = dcsBiosBinding.DCSBIOSInputs;
                                TextBoxG8On.Text = dcsBiosBinding.Description;
                                TextBoxG8On.ToolTip = "DCS-BIOS";
                            }
                        }
                        else
                        {
                            if (dcsBiosBinding.DCSBIOSInputs.Count > 0)
                            {
                                ((TagDataClassTPM)TextBoxG8Off.Tag).DCSBIOSInputs = dcsBiosBinding.DCSBIOSInputs;
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
                                ((TagDataClassTPM)TextBoxG9On.Tag).DCSBIOSInputs = dcsBiosBinding.DCSBIOSInputs;
                                TextBoxG9On.Text = dcsBiosBinding.Description;
                                TextBoxG9On.ToolTip = "DCS-BIOS";
                            }
                        }
                        else
                        {
                            if (dcsBiosBinding.DCSBIOSInputs.Count > 0)
                            {
                                ((TagDataClassTPM)TextBoxG9Off.Tag).DCSBIOSInputs = dcsBiosBinding.DCSBIOSInputs;
                                TextBoxG9Off.Text = dcsBiosBinding.Description;
                                TextBoxG9Off.ToolTip = "DCS-BIOS";
                            }
                        }
                    }
                }

                foreach (var bipLink in _tpmPanel.BipLinkHashSet)
                {
                    if (bipLink.TPMSwitch == TPMPanelSwitches.G1)
                    {
                        if (bipLink.WhenTurnedOn)
                        {
                            if (bipLink.BIPLights.Count > 0)
                            {
                                ((TagDataClassTPM)TextBoxG1On.Tag).BIPLink = bipLink;
                                TextBoxG1On.Background = Brushes.Bisque;
                            }
                        }
                        else
                        {
                            if (bipLink.BIPLights.Count > 0)
                            {
                                ((TagDataClassTPM)TextBoxG1Off.Tag).BIPLink = bipLink;
                                TextBoxG1Off.Background = Brushes.Bisque;
                            }
                        }
                    }
                    if (bipLink.TPMSwitch == TPMPanelSwitches.G2)
                    {
                        if (bipLink.WhenTurnedOn)
                        {
                            if (bipLink.BIPLights.Count > 0)
                            {
                                ((TagDataClassTPM)TextBoxG2On.Tag).BIPLink = bipLink;
                                TextBoxG2On.Background = Brushes.Bisque;
                            }
                        }
                        else
                        {
                            if (bipLink.BIPLights.Count > 0)
                            {
                                ((TagDataClassTPM)TextBoxG2Off.Tag).BIPLink = bipLink;
                                TextBoxG2Off.Background = Brushes.Bisque;
                            }
                        }
                    }
                    if (bipLink.TPMSwitch == TPMPanelSwitches.G3)
                    {
                        if (bipLink.WhenTurnedOn)
                        {
                            if (bipLink.BIPLights.Count > 0)
                            {
                                ((TagDataClassTPM)TextBoxG3On.Tag).BIPLink = bipLink;
                                TextBoxG3On.Background = Brushes.Bisque;
                            }
                        }
                        else
                        {
                            if (bipLink.BIPLights.Count > 0)
                            {
                                ((TagDataClassTPM)TextBoxG3Off.Tag).BIPLink = bipLink;
                                TextBoxG3Off.Background = Brushes.Bisque;
                            }
                        }
                    }
                    if (bipLink.TPMSwitch == TPMPanelSwitches.G4)
                    {
                        if (bipLink.WhenTurnedOn)
                        {
                            if (bipLink.BIPLights.Count > 0)
                            {
                                ((TagDataClassTPM)TextBoxG4On.Tag).BIPLink = bipLink;
                                TextBoxG4On.Background = Brushes.Bisque;
                            }
                        }
                        else
                        {
                            if (bipLink.BIPLights.Count > 0)
                            {
                                ((TagDataClassTPM)TextBoxG4Off.Tag).BIPLink = bipLink;
                                TextBoxG4Off.Background = Brushes.Bisque;
                            }
                        }
                    }
                    if (bipLink.TPMSwitch == TPMPanelSwitches.G5)
                    {
                        if (bipLink.WhenTurnedOn)
                        {
                            if (bipLink.BIPLights.Count > 0)
                            {
                                ((TagDataClassTPM)TextBoxG5On.Tag).BIPLink = bipLink;
                                TextBoxG5On.Background = Brushes.Bisque;
                            }
                        }
                        else
                        {
                            if (bipLink.BIPLights.Count > 0)
                            {
                                ((TagDataClassTPM)TextBoxG5Off.Tag).BIPLink = bipLink;
                                TextBoxG5Off.Background = Brushes.Bisque;
                            }
                        }
                    }
                    if (bipLink.TPMSwitch == TPMPanelSwitches.G6)
                    {
                        if (bipLink.WhenTurnedOn)
                        {
                            if (bipLink.BIPLights.Count > 0)
                            {
                                ((TagDataClassTPM)TextBoxG6On.Tag).BIPLink = bipLink;
                                TextBoxG6On.Background = Brushes.Bisque;
                            }
                        }
                        else
                        {
                            if (bipLink.BIPLights.Count > 0)
                            {
                                ((TagDataClassTPM)TextBoxG6Off.Tag).BIPLink = bipLink;
                                TextBoxG6Off.Background = Brushes.Bisque;
                            }
                        }
                    }
                    if (bipLink.TPMSwitch == TPMPanelSwitches.G7)
                    {
                        if (bipLink.WhenTurnedOn)
                        {
                            if (bipLink.BIPLights.Count > 0)
                            {
                                ((TagDataClassTPM)TextBoxG7On.Tag).BIPLink = bipLink;
                                TextBoxG7On.Background = Brushes.Bisque;
                            }
                        }
                        else
                        {
                            if (bipLink.BIPLights.Count > 0)
                            {
                                ((TagDataClassTPM)TextBoxG7Off.Tag).BIPLink = bipLink;
                                TextBoxG7Off.Background = Brushes.Bisque;
                            }
                        }
                    }
                    if (bipLink.TPMSwitch == TPMPanelSwitches.G8)
                    {
                        if (bipLink.WhenTurnedOn)
                        {
                            if (bipLink.BIPLights.Count > 0)
                            {
                                ((TagDataClassTPM)TextBoxG8On.Tag).BIPLink = bipLink;
                                TextBoxG8On.Background = Brushes.Bisque;
                            }
                        }
                        else
                        {
                            if (bipLink.BIPLights.Count > 0)
                            {
                                ((TagDataClassTPM)TextBoxG8Off.Tag).BIPLink = bipLink;
                                TextBoxG8Off.Background = Brushes.Bisque;
                            }
                        }
                    }
                    if (bipLink.TPMSwitch == TPMPanelSwitches.G9)
                    {
                        if (bipLink.WhenTurnedOn)
                        {
                            if (bipLink.BIPLights.Count > 0)
                            {
                                ((TagDataClassTPM)TextBoxG9On.Tag).BIPLink = bipLink;
                                TextBoxG9On.Background = Brushes.Bisque;
                            }
                        }
                        else
                        {
                            if (bipLink.BIPLights.Count > 0)
                            {
                                ((TagDataClassTPM)TextBoxG9Off.Tag).BIPLink = bipLink;
                                TextBoxG9Off.Background = Brushes.Bisque;
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


