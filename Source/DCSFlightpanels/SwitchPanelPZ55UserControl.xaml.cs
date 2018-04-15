using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
    /// Interaction logic for SwitchPanelPZ55UserControl.xaml
    /// </summary>
    public partial class SwitchPanelPZ55UserControl : ISaitekPanelListener, IProfileHandlerListener, ISaitekUserControl
    {

        private readonly SwitchPanelPZ55 _switchPanelPZ55;
        private TabItem _parentTabItem;
        private string _parentTabItemHeader;
        private Image[] _imageArrayUpper = new Image[4];
        private Image[] _imageArrayLeft = new Image[4];
        private Image[] _imageArrayRight = new Image[4];
        private IGlobalHandler _globalHandler;
        private bool _enableDCSBIOS;
        private bool _textBoxTagsSet;
        private bool _controlLoaded;

        public SwitchPanelPZ55UserControl(HIDSkeleton hidSkeleton, TabItem parentTabItem, IGlobalHandler globalHandler, bool enableDCSBIOS)
        {
            InitializeComponent();
            _parentTabItem = parentTabItem;
            _parentTabItemHeader = _parentTabItem.Header.ToString();
            _switchPanelPZ55 = new SwitchPanelPZ55(hidSkeleton);

            _switchPanelPZ55.Attach((ISaitekPanelListener)this);
            globalHandler.Attach(_switchPanelPZ55);
            _globalHandler = globalHandler;
            _enableDCSBIOS = enableDCSBIOS;
            _imageArrayUpper[0] = ImagePZ55LEDDarkUpper;
            _imageArrayUpper[1] = ImagePZ55LEDGreenUpper;
            _imageArrayUpper[2] = ImagePZ55LEDYellowUpper;
            _imageArrayUpper[3] = ImagePZ55LEDRedUpper;

            _imageArrayLeft[0] = ImagePZ55LEDDarkLeft;
            _imageArrayLeft[1] = ImagePZ55LEDGreenLeft;
            _imageArrayLeft[2] = ImagePZ55LEDYellowLeft;
            _imageArrayLeft[3] = ImagePZ55LEDRedLeft;

            _imageArrayRight[0] = ImagePZ55LEDDarkRight;
            _imageArrayRight[1] = ImagePZ55LEDGreenRight;
            _imageArrayRight[2] = ImagePZ55LEDYellowRight;
            _imageArrayRight[3] = ImagePZ55LEDRedRight;
        }

        private void SwitchPanelPZ55UserControl_OnLoaded(object sender, RoutedEventArgs e)
        {
            var now = DateTime.Now.Ticks;
            Debug.WriteLine("Start SwitchPanelPZ55UserControl_OnLoaded");
            SetTextBoxTagObjects();
            SetContextMenuClickHandlers();
            Debug.WriteLine("End SwitchPanelPZ55UserControl_OnLoaded" + new TimeSpan(DateTime.Now.Ticks - now).Milliseconds);
            _controlLoaded = true;
            ShowGraphicConfiguration();
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
            return _switchPanelPZ55;
        }

        public string GetName()
        {
            return GetType().Name;
        }

        public void SelectedAirframe(object sender, AirframEventArgs e)
        {
            try
            {
                foreach (var image in Common.FindVisualChildren<Image>(this))
                {
                    if (image.Name.StartsWith("ImagePZ55LED") && Common.IsKeyEmulationProfile(e.Airframe))
                    {
                        image.ContextMenu = null;
                    }
                    else
                        if (image.Name.StartsWith("ImagePZ55LED") && image.ContextMenu == null && Common.IsDCSBIOSProfile(e.Airframe))
                    {
                        image.ContextMenu = (ContextMenu)Resources["PZ55LEDContextMenu"];
                        if (image.ContextMenu != null)
                        {
                            image.ContextMenu.Tag = image.Name;
                        }
                    }
                }
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
                if (e.SaitekPanelEnum == SaitekPanelsEnum.PZ55SwitchPanel && e.UniqueId.Equals(_switchPanelPZ55.InstanceId))
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

        public void LedLightChanged(object sender, LedLightChangeEventArgs e)
        {
            try
            {
                if (_switchPanelPZ55.InstanceId.Equals(e.UniqueId))
                {
                    var position = (SwitchPanelPZ55LEDPosition)e.LEDPosition.Position;
                    var imageArray = _imageArrayUpper;

                    switch (position)
                    {
                        case SwitchPanelPZ55LEDPosition.UP:
                            {
                                HideLedImages(SwitchPanelPZ55LEDPosition.UP);
                                break;
                            }
                        case SwitchPanelPZ55LEDPosition.LEFT:
                            {
                                HideLedImages(SwitchPanelPZ55LEDPosition.LEFT);
                                imageArray = _imageArrayLeft;
                                break;
                            }
                        case SwitchPanelPZ55LEDPosition.RIGHT:
                            {
                                HideLedImages(SwitchPanelPZ55LEDPosition.RIGHT);
                                imageArray = _imageArrayRight;
                                break;
                            }
                    }

                    switch (e.LEDColor)
                    {
                        case PanelLEDColor.DARK:
                            {
                                Dispatcher.BeginInvoke((Action)(() => imageArray[0].Visibility = Visibility.Visible));
                                break;
                            }
                        case PanelLEDColor.GREEN:
                            {
                                Dispatcher.BeginInvoke((Action)(() => imageArray[1].Visibility = Visibility.Visible));
                                break;
                            }
                        case PanelLEDColor.YELLOW:
                            {
                                Dispatcher.BeginInvoke((Action)(() => imageArray[2].Visibility = Visibility.Visible));
                                break;
                            }
                        case PanelLEDColor.RED:
                            {
                                Dispatcher.BeginInvoke((Action)(() => imageArray[3].Visibility = Visibility.Visible));
                                break;
                            }
                    }

                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(1069, ex);
            }
        }

        private void HideLedImages(SwitchPanelPZ55LEDPosition switchPanelPZ55LEDPosition)
        {
            var imageArray = _imageArrayUpper;
            switch (switchPanelPZ55LEDPosition)
            {
                case SwitchPanelPZ55LEDPosition.UP:
                    {

                        break;
                    }
                case SwitchPanelPZ55LEDPosition.LEFT:
                    {
                        imageArray = _imageArrayLeft;
                        break;
                    }
                case SwitchPanelPZ55LEDPosition.RIGHT:
                    {
                        imageArray = _imageArrayRight;
                        break;
                    }
            }
            for (int i = 0; i < 4; i++)
            {
                var image1 = imageArray[i];
                Dispatcher.BeginInvoke((Action)(() => image1.Visibility = Visibility.Collapsed));
            }
        }

        public void PanelDataAvailable(object sender, PanelDataToDCSBIOSEventEventArgs e)
        {
            try
            {
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
                if (e.SaitekPanelEnum == SaitekPanelsEnum.PZ55SwitchPanel && e.UniqueId.Equals(_switchPanelPZ55.InstanceId))
                {
                    //Dispatcher.BeginInvoke((Action)(() => _parentTabItem.Header = _parentTabItemHeader + " (connected)"));
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(2008, ex);
            }
        }

        public void DeviceDetached(object sender, PanelEventArgs e)
        {
            try
            {
                if (e.SaitekPanelEnum == SaitekPanelsEnum.PZ55SwitchPanel && e.UniqueId.Equals(_switchPanelPZ55.InstanceId))
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
                if (e.UniqueId.Equals(_switchPanelPZ55.InstanceId) && e.SaitekPanelEnum == SaitekPanelsEnum.PZ55SwitchPanel)
                {
                    Dispatcher.BeginInvoke((Action)(ShowGraphicConfiguration));
                    Dispatcher.BeginInvoke((Action)(() => TextBoxLogPZ55.Text = ""));
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
                TextBoxLogPZ55.Focus();
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
                if (((TagDataClassPZ55)textBox.Tag).ContainsKeySequence())
                {
                    sequenceWindow = new SequenceWindow(textBox.Text, ((TagDataClassPZ55)textBox.Tag).GetKeySequence());
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
                        ((TagDataClassPZ55)textBox.Tag).KeyPress = osKeyPress;
                        //textBox.Text = string.IsNullOrEmpty(sequenceWindow.GetInformation) ? "Key press sequence" : sequenceWindow.GetInformation;
                        /*if (!string.IsNullOrEmpty(sequenceWindow.GetInformation))
                        {
                            var toolTip = new ToolTip { Content = sequenceWindow.GetInformation };
                            textBox.ToolTipa = toolTip;
                        }*/
                        UpdateKeyBindingProfileSequencedKeyStrokesPZ55(textBox);
                    }
                    else
                    {
                        //If only one press was created treat it as a simple keypress
                        ((TagDataClassPZ55)textBox.Tag).ClearAll();
                        var osKeyPress = new OSKeyPress(sequenceList[0].VirtualKeyCodesAsString, sequenceList[0].LengthOfKeyPress);
                        ((TagDataClassPZ55)textBox.Tag).KeyPress = osKeyPress;
                        /*textBox.Text = sequenceList.Values[0].VirtualKeyCodesAsString;
                        if (!string.IsNullOrEmpty(sequenceWindow.GetInformation))
                        {
                            var toolTip = new ToolTip { Content = sequenceWindow.GetInformation };
                            textBox.ToolTipa = toolTip;
                        }*/
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
                if (((TagDataClassPZ55)textBox.Tag).ContainsDCSBIOS())
                {
                    dcsBIOSControlsConfigsWindow = new DCSBIOSControlsConfigsWindow(_globalHandler.GetAirframe(), textBox.Name.Replace("TextBox", ""), ((TagDataClassPZ55)textBox.Tag).DCSBIOSInputs, textBox.Text);
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
                    ((TagDataClassPZ55)textBox.Tag).DCSBIOSInputs = dcsBiosInputs;
                    UpdateDCSBIOSBinding(textBox);
                }
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
                if (((TagDataClassPZ55)textBox.Tag).ContainsBIPLink())
                {
                    var bipLink = ((TagDataClassPZ55)textBox.Tag).BIPLink;
                    bipLinkWindow = new BIPLinkWindow(bipLink);
                }
                else
                {
                    var bipLink = new BIPLinkPZ55();
                    bipLinkWindow = new BIPLinkWindow(bipLink);
                }
                bipLinkWindow.ShowDialog();
                if (bipLinkWindow.DialogResult.HasValue && bipLinkWindow.DialogResult == true && bipLinkWindow.IsDirty && bipLinkWindow.BIPLink != null && bipLinkWindow.BIPLink.BIPLights.Count > 0)
                {
                    ((TagDataClassPZ55)textBox.Tag).BIPLink = (BIPLinkPZ55)bipLinkWindow.BIPLink;
                    UpdateBIPLinkBindings(textBox);
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



                if (!((TagDataClassPZ55)textBox.Tag).ContainsSingleKey())
                {
                    return;
                }
                var keyPressLength = ((TagDataClassPZ55)textBox.Tag).KeyPress.GetLengthOfKeyPress();

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
                var tagHolderClass = (TagDataClassPZ55)textBox.Tag;
                textBox.Text = "";
                tagHolderClass.ClearAll();
            }
            if (clearAlsoProfile)
            {
                _switchPanelPZ55.ClearSettings();
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
                textBox.Tag = new TagDataClassPZ55();
            }
            _textBoxTagsSet = true;
        }

        private void RemoveContextMenuClickHandlers()
        {
            foreach (var textBox in Common.FindVisualChildren<TextBox>(this))
            {
                if (!Equals(textBox, TextBoxLogPZ55))
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
                if (!Equals(textBox, TextBoxLogPZ55))
                {
                    var contectMenu = (ContextMenu)Resources["TextBoxContextMenuPZ55"];
                    if (!BipFactory.HasBips())
                    {
                        MenuItem bipMenuItem = null;
                        foreach (var item in contectMenu.Items)
                        {
                            if (((MenuItem)item).Name.Contains("EditBIP"))
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
                    if (!_enableDCSBIOS)
                    {
                        MenuItem dcsBIOSMenuItem = null;
                        foreach (var item in contectMenu.Items)
                        {
                            if (((MenuItem)item).Name.Contains("EditDCSBIOS"))
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
                    if (image.Name.StartsWith("ImagePZ55LED"))
                    {
                        image.ContextMenu = (ContextMenu)Resources["PZ55LEDContextMenu"];
                        if (image.ContextMenu != null) image.ContextMenu.Tag = image.Name;
                    }
                }
            }
        }


        private void ContextConfigureLandingGearLED_OnContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            try
            {
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(204165, ex);
            }
        }

        private void ContextConfigureLandingGearLEDClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var menuItem = (MenuItem)sender;
                var contextMenu = (ContextMenu)menuItem.Parent;
                var imageName = contextMenu.Tag.ToString();
                var position = SwitchPanelPZ55LEDPosition.LEFT;

                if (imageName.Contains("Upper"))
                {
                    position = SwitchPanelPZ55LEDPosition.UP;
                }
                else if (imageName.Contains("Left"))
                {
                    position = SwitchPanelPZ55LEDPosition.LEFT;
                }
                else if (imageName.Contains("Right"))
                {
                    position = SwitchPanelPZ55LEDPosition.RIGHT;
                }
                var ledConfigsWindow = new LEDConfigsWindow(_globalHandler.GetAirframe(), "Set configuration for LED : " + position, new SaitekPanelLEDPosition(position), _switchPanelPZ55.GetLedDcsBiosOutputs(position), _switchPanelPZ55);
                if (ledConfigsWindow.ShowDialog() == true)
                {
                    //must include position because if user has deleted all entries then there is nothing to go after regarding position
                    _switchPanelPZ55.SetLedDcsBiosOutput(position, ledConfigsWindow.ColorOutputBindings);
                }
                SetConfigExistsImageVisibility();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(2065, ex);
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

                if (((TagDataClassPZ55)textBox.Tag).ContainsDCSBIOS())
                {
                    // 1) If Contains DCSBIOS, show Edit DCS-BIOS Control & BIP
                    foreach (MenuItem item in contextMenu.Items)
                    {
                        if (item.Name.Contains("EditDCSBIOS"))
                        {
                            item.Visibility = Visibility.Visible;
                        }
                        if (BipFactory.HasBips() && item.Name.Contains("EditBIP"))
                        {
                            item.Visibility = Visibility.Visible;
                        }
                    }
                }
                else if (((TagDataClassPZ55)textBox.Tag).ContainsKeySequence())
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
                else if (((TagDataClassPZ55)textBox.Tag).IsEmpty())
                {
                    // 4) 
                    foreach (MenuItem item in contextMenu.Items)
                    {
                        if (item.Name.Contains("EditSequence"))
                        {
                            item.Visibility = Visibility.Visible;
                        }
                        else if (!_switchPanelPZ55.KeyboardEmulationOnly && item.Name.Contains("EditDCSBIOS"))
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
                else if (((TagDataClassPZ55)textBox.Tag).ContainsSingleKey())
                {
                    // 5) 
                    foreach (MenuItem item in contextMenu.Items)
                    {
                        if (!(item.Name.Contains("EditSequence") || item.Name.Contains("EditDCSBIOS")))
                        {
                            item.Visibility = Visibility.Visible;
                        }
                    }
                }
                else if (((TagDataClassPZ55)textBox.Tag).ContainsBIPLink())
                {
                    // 3) 
                    foreach (MenuItem item in contextMenu.Items)
                    {
                        if (!_switchPanelPZ55.KeyboardEmulationOnly && item.Name.Contains("EditDCSBIOS"))
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
                if (!Equals(textBox, TextBoxLogPZ55) && textBox.IsFocused && Equals(textBox.Background, Brushes.Yellow))
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
                    ((TagDataClassPZ55)textBox.Tag).KeyPress.SetLengthOfKeyPress(KeyPressLength.FiftyMilliSec);
                }
                else if (contextMenuItem.Name == "contextMenuItemHalfSecond")
                {
                    ((TagDataClassPZ55)textBox.Tag).KeyPress.SetLengthOfKeyPress(KeyPressLength.HalfSecond);
                }
                else if (contextMenuItem.Name == "contextMenuItemSecond")
                {
                    ((TagDataClassPZ55)textBox.Tag).KeyPress.SetLengthOfKeyPress(KeyPressLength.Second);
                }
                else if (contextMenuItem.Name == "contextMenuItemSecondAndHalf")
                {
                    ((TagDataClassPZ55)textBox.Tag).KeyPress.SetLengthOfKeyPress(KeyPressLength.SecondAndHalf);
                }
                else if (contextMenuItem.Name == "contextMenuItemTwoSeconds")
                {
                    ((TagDataClassPZ55)textBox.Tag).KeyPress.SetLengthOfKeyPress(KeyPressLength.TwoSeconds);
                }
                else if (contextMenuItem.Name == "contextMenuItemThreeSeconds")
                {
                    ((TagDataClassPZ55)textBox.Tag).KeyPress.SetLengthOfKeyPress(KeyPressLength.ThreeSeconds);
                }
                else if (contextMenuItem.Name == "contextMenuItemFourSeconds")
                {
                    ((TagDataClassPZ55)textBox.Tag).KeyPress.SetLengthOfKeyPress(KeyPressLength.FourSeconds);
                }
                else if (contextMenuItem.Name == "contextMenuItemFiveSecs")
                {
                    ((TagDataClassPZ55)textBox.Tag).KeyPress.SetLengthOfKeyPress(KeyPressLength.FiveSecs);
                }
                else if (contextMenuItem.Name == "contextMenuItemTenSecs")
                {
                    ((TagDataClassPZ55)textBox.Tag).KeyPress.SetLengthOfKeyPress(KeyPressLength.TenSecs);
                }
                else if (contextMenuItem.Name == "contextMenuItemFifteenSecs")
                {
                    ((TagDataClassPZ55)textBox.Tag).KeyPress.SetLengthOfKeyPress(KeyPressLength.FifteenSecs);
                }
                else if (contextMenuItem.Name == "contextMenuItemTwentySecs")
                {
                    ((TagDataClassPZ55)textBox.Tag).KeyPress.SetLengthOfKeyPress(KeyPressLength.TwentySecs);
                }
                else if (contextMenuItem.Name == "contextMenuItemThirtySecs")
                {
                    ((TagDataClassPZ55)textBox.Tag).KeyPress.SetLengthOfKeyPress(KeyPressLength.ThirtySecs);
                }
                else if (contextMenuItem.Name == "contextMenuItemFortySecs")
                {
                    ((TagDataClassPZ55)textBox.Tag).KeyPress.SetLengthOfKeyPress(KeyPressLength.FortySecs);
                }
                else if (contextMenuItem.Name == "contextMenuItemSixtySecs")
                {
                    ((TagDataClassPZ55)textBox.Tag).KeyPress.SetLengthOfKeyPress(KeyPressLength.SixtySecs);
                }

                UpdateKeyBindingProfileSimpleKeyStrokes(textBox);
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(2082, ex);
            }
        }
        private void ImageLEDClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                //Only left mouse clicks here!
                if (e.RightButton == MouseButtonState.Pressed)
                {
                    return;
                }
                var imageArray = new Image[4];
                var clickedImage = (Image)sender;
                var position = SwitchPanelPZ55LEDPosition.UP;
                //Name of graphics file
                var imageSource = (string)clickedImage.Tag;

                if (clickedImage.Name.Contains("Upper"))
                {
                    position = SwitchPanelPZ55LEDPosition.UP;
                    imageArray = _imageArrayUpper;
                }
                else if (clickedImage.Name.Contains("Left"))
                {
                    position = SwitchPanelPZ55LEDPosition.LEFT;
                    imageArray = _imageArrayLeft;
                }
                else if (clickedImage.Name.Contains("Right"))
                {
                    position = SwitchPanelPZ55LEDPosition.RIGHT;
                    imageArray = _imageArrayRight;
                }
                var nextImageIndex = 0;
                HideLedImages(position);
                for (int i = 0; i < 4; i++)
                {
                    var image = imageArray[i];
                    if (clickedImage.Equals(image))
                    {
                        nextImageIndex = i + 1;
                        if (nextImageIndex > 3)
                        {
                            nextImageIndex = 0;
                        }
                    }
                }

                imageArray[nextImageIndex].Visibility = Visibility.Visible;


                if (imageArray[nextImageIndex].Name.Contains("Dark"))
                {
                    _switchPanelPZ55.SetLandingGearLED(position, PanelLEDColor.DARK);
                }
                else if (imageArray[nextImageIndex].Name.Contains("Green"))
                {
                    _switchPanelPZ55.SetLandingGearLED(position, PanelLEDColor.GREEN);
                }
                else if (imageArray[nextImageIndex].Name.Contains("Yellow"))
                {
                    _switchPanelPZ55.SetLandingGearLED(position, PanelLEDColor.YELLOW);
                }
                else if (imageArray[nextImageIndex].Name.Contains("Red"))
                {
                    _switchPanelPZ55.SetLandingGearLED(position, PanelLEDColor.RED);
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(2083, ex);
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
                    if (((TagDataClassPZ55)textBox.Tag).ContainsDCSBIOS())
                    {
                        if (MessageBox.Show("Do you want to delete the DCS-BIOS configuration?", "Delete DCS-BIOS configuration?", MessageBoxButton.OKCancel, MessageBoxImage.Question) != MessageBoxResult.OK)
                        {
                            return;
                        }
                        textBox.Text = "";
                        _switchPanelPZ55.RemoveSwitchPanelSwitchFromList(ControlListPZ55.DCSBIOS, GetPZ55Key(textBox).SwitchPanelPZ55Key, GetPZ55Key(textBox).On);
                        ((TagDataClassPZ55)textBox.Tag).DCSBIOSInputs.Clear();
                    }
                    else if (((TagDataClassPZ55)textBox.Tag).ContainsKeySequence())
                    {
                        //Check if this textbox contains sequence information. If so then prompt the user for deletion
                        if (MessageBox.Show("Do you want to delete the key sequence?", "Delete key sequence?", MessageBoxButton.OKCancel, MessageBoxImage.Question) != MessageBoxResult.OK)
                        {
                            return;
                        }
                        ((TagDataClassPZ55)textBox.Tag).KeyPress.KeySequence.Clear();
                        textBox.Text = "";
                        UpdateKeyBindingProfileSimpleKeyStrokes(textBox);
                    }
                    else if (((TagDataClassPZ55)textBox.Tag).ContainsSingleKey())
                    {
                        ((TagDataClassPZ55)textBox.Tag).KeyPress.KeySequence.Clear();
                        textBox.Text = "";
                        UpdateKeyBindingProfileSimpleKeyStrokes(textBox);
                    }
                    if (((TagDataClassPZ55)textBox.Tag).ContainsBIPLink())
                    {
                        //Check if this textbox contains sequence information. If so then prompt the user for deletion
                        if (MessageBox.Show("Do you want to delete BIP Links?", "Delete BIP Link?", MessageBoxButton.OKCancel, MessageBoxImage.Question) != MessageBoxResult.OK)
                        {
                            return;
                        }
                        ((TagDataClassPZ55)textBox.Tag).BIPLink.BIPLights.Clear();
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


        private void TextBox_OnMouseDown(object sender, MouseButtonEventArgs e)
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
                if (((TagDataClassPZ55)textBox.Tag).ContainsBIPLink())
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
                if (((TagDataClassPZ55)textBox.Tag).ContainsKeySequence() || ((TagDataClassPZ55)textBox.Tag).ContainsDCSBIOS())
                {
                    return;
                }
                var hashSetOfKeysPressed = new HashSet<string>();

                /*if (((TextBoxTagHolderClass)textBox.Tag) == null)
                {
                    ((TextBoxTagHolderClass)textBox.Tag) = xxKeyPressLength.FiftyMilliSec;
                }*/

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
                if (((TagDataClassPZ55)textBox.Tag).ContainsKeySequence())
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
                if (((TagDataClassPZ55)textBox.Tag).ContainsKeySequence() || ((TagDataClassPZ55)textBox.Tag).ContainsDCSBIOS())
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

                UpdateKeyBindingProfileSequencedKeyStrokesPZ55(textBox);
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
                Dispatcher.BeginInvoke((Action)(() => TextBoxLogPZ55.Focus()));
                foreach (var switchPanelKey in switches)
                {
                    var key = (SwitchPanelKey)switchPanelKey;

                    if (_switchPanelPZ55.ForwardKeyPresses)
                    {
                        if (!string.IsNullOrEmpty(_switchPanelPZ55.GetKeyPressForLoggingPurposes(key)))
                        {
                            Dispatcher.BeginInvoke(
                                (Action)
                                (() =>
                                 TextBoxLogPZ55.Text =
                                 TextBoxLogPZ55.Text.Insert(0, _switchPanelPZ55.GetKeyPressForLoggingPurposes(key) + "\n")));
                        }
                    }
                    else
                    {
                        Dispatcher.BeginInvoke(
                            (Action)
                            (() =>
                             TextBoxLogPZ55.Text =
                             TextBoxLogPZ55.Text = TextBoxLogPZ55.Text.Insert(0, "No action taken, virtual key press disabled.\n")));
                    }
                }
                SetGraphicsState(switches);
            }
            catch (Exception ex)
            {
                Dispatcher.BeginInvoke(
                    (Action)
                    (() =>
                     TextBoxLogPZ55.Text = TextBoxLogPZ55.Text.Insert(0, "0x16" + ex.Message + ".\n")));
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
                                        ImageAvMasterOn.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
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
                                        ImageCowlClosed.Visibility = !key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case SwitchPanelPZ55Keys.SWITCHKEY_DE_ICE:
                            {
                                var key = switchPanelKey;
                                Dispatcher.BeginInvoke(
                                    (Action)delegate
                                    {
                                        ImageDeIceOn.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case SwitchPanelPZ55Keys.SWITCHKEY_FUEL_PUMP:
                            {
                                var key = switchPanelKey;
                                Dispatcher.BeginInvoke(
                                    (Action)delegate
                                    {
                                        ImageFuelPumpOn.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case SwitchPanelPZ55Keys.LEVER_GEAR_DOWN:
                            {
                                var key = switchPanelKey;
                                Dispatcher.BeginInvoke(
                                    (Action)delegate
                                    {
                                        ImageGearUp.Visibility = key.IsOn ? Visibility.Collapsed : Visibility.Visible;
                                    });
                                break;
                            }
                        case SwitchPanelPZ55Keys.LEVER_GEAR_UP:
                            {
                                var key = switchPanelKey;
                                Dispatcher.BeginInvoke(
                                    (Action)delegate
                                    {
                                        ImageGearUp.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_BEACON:
                            {
                                var key = switchPanelKey;
                                Dispatcher.BeginInvoke(
                                    (Action)delegate
                                    {
                                        ImageBeaconOn.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_LANDING:
                            {
                                var key = switchPanelKey;
                                Dispatcher.BeginInvoke(
                                    (Action)delegate
                                    {
                                        ImageLandingOn.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_NAV:
                            {
                                var key = switchPanelKey;
                                Dispatcher.BeginInvoke(
                                    (Action)delegate
                                    {
                                        ImageNavOn.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_PANEL:
                            {
                                var key = switchPanelKey;
                                Dispatcher.BeginInvoke(
                                    (Action)delegate
                                    {
                                        ImagePanelOn.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_STROBE:
                            {
                                var key = switchPanelKey;
                                Dispatcher.BeginInvoke(
                                    (Action)delegate
                                    {
                                        ImageStrobeOn.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_TAXI:
                            {
                                var key = switchPanelKey;
                                Dispatcher.BeginInvoke(
                                    (Action)delegate
                                    {
                                        ImageTaxiOn.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case SwitchPanelPZ55Keys.SWITCHKEY_MASTER_ALT:
                            {
                                var key = switchPanelKey;
                                Dispatcher.BeginInvoke(
                                    (Action)delegate
                                    {
                                        ImageMasterAltOn.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case SwitchPanelPZ55Keys.SWITCHKEY_MASTER_BAT:
                            {
                                var key = switchPanelKey;
                                Dispatcher.BeginInvoke(
                                    (Action)delegate
                                    {
                                        ImageMasterBatOn.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case SwitchPanelPZ55Keys.SWITCHKEY_PITOT_HEAT:
                            {
                                var key = switchPanelKey;
                                Dispatcher.BeginInvoke(
                                    (Action)delegate
                                    {
                                        ImagePitotHeatOn.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
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
                                    ImageKnobAll.Visibility = key.IsOn && key.SwitchPanelPZ55Key == SwitchPanelPZ55Keys.KNOB_ENGINE_BOTH ? Visibility.Visible : Visibility.Collapsed;
                                    ImageKnobL.Visibility = key.IsOn && key.SwitchPanelPZ55Key == SwitchPanelPZ55Keys.KNOB_ENGINE_LEFT ? Visibility.Visible : Visibility.Collapsed;
                                    ImageKnobR.Visibility = key.IsOn && key.SwitchPanelPZ55Key == SwitchPanelPZ55Keys.KNOB_ENGINE_RIGHT ? Visibility.Visible : Visibility.Collapsed;
                                    ImageKnobStart.Visibility = key.IsOn && key.SwitchPanelPZ55Key == SwitchPanelPZ55Keys.KNOB_ENGINE_START ? Visibility.Visible : Visibility.Collapsed;
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


        private void UpdateBIPLinkBindings(TextBox textBox)
        {
            try
            {
                /*
                if (((TextBoxTagHolderClass)textBox.Tag) == null)
                {
                    ((TextBoxTagHolderClass)textBox.Tag) = xxnew SortedList<int, KeyPressInfo>();
                }
                */

                if (textBox.Equals(TextBoxKnobOff))
                {
                    _switchPanelPZ55.AddOrUpdateBIPLinkKeyBinding(SwitchPanelPZ55Keys.KNOB_ENGINE_OFF, ((TagDataClassPZ55)textBox.Tag).BIPLink);
                }
                if (textBox.Equals(TextBoxKnobR))
                {
                    _switchPanelPZ55.AddOrUpdateBIPLinkKeyBinding(SwitchPanelPZ55Keys.KNOB_ENGINE_RIGHT, ((TagDataClassPZ55)textBox.Tag).BIPLink);
                }
                if (textBox.Equals(TextBoxKnobL))
                {
                    _switchPanelPZ55.AddOrUpdateBIPLinkKeyBinding(SwitchPanelPZ55Keys.KNOB_ENGINE_LEFT, ((TagDataClassPZ55)textBox.Tag).BIPLink);
                }
                if (textBox.Equals(TextBoxKnobAll))
                {
                    _switchPanelPZ55.AddOrUpdateBIPLinkKeyBinding(SwitchPanelPZ55Keys.KNOB_ENGINE_BOTH, ((TagDataClassPZ55)textBox.Tag).BIPLink);
                }
                if (textBox.Equals(TextBoxKnobStart))
                {
                    _switchPanelPZ55.AddOrUpdateBIPLinkKeyBinding(SwitchPanelPZ55Keys.KNOB_ENGINE_START, ((TagDataClassPZ55)textBox.Tag).BIPLink);
                }
                if (textBox.Equals(TextBoxCowlClose))
                {
                    _switchPanelPZ55.AddOrUpdateBIPLinkKeyBinding(SwitchPanelPZ55Keys.SWITCHKEY_CLOSE_COWL, ((TagDataClassPZ55)textBox.Tag).BIPLink, false);
                }
                if (textBox.Equals(TextBoxCowlOpen))
                {
                    _switchPanelPZ55.AddOrUpdateBIPLinkKeyBinding(SwitchPanelPZ55Keys.SWITCHKEY_CLOSE_COWL, ((TagDataClassPZ55)textBox.Tag).BIPLink);
                }
                if (textBox.Equals(TextBoxPanelOff))
                {
                    _switchPanelPZ55.AddOrUpdateBIPLinkKeyBinding(SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_PANEL, ((TagDataClassPZ55)textBox.Tag).BIPLink, false);
                }
                if (textBox.Equals(TextBoxPanelOn))
                {
                    _switchPanelPZ55.AddOrUpdateBIPLinkKeyBinding(SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_PANEL, ((TagDataClassPZ55)textBox.Tag).BIPLink);
                }
                if (textBox.Equals(TextBoxBeaconOff))
                {
                    _switchPanelPZ55.AddOrUpdateBIPLinkKeyBinding(SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_BEACON, ((TagDataClassPZ55)textBox.Tag).BIPLink, false);
                }
                if (textBox.Equals(TextBoxBeaconOn))
                {
                    _switchPanelPZ55.AddOrUpdateBIPLinkKeyBinding(SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_BEACON, ((TagDataClassPZ55)textBox.Tag).BIPLink);
                }
                if (textBox.Equals(TextBoxNavOff))
                {
                    _switchPanelPZ55.AddOrUpdateBIPLinkKeyBinding(SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_NAV, ((TagDataClassPZ55)textBox.Tag).BIPLink, false);
                }
                if (textBox.Equals(TextBoxNavOn))
                {
                    _switchPanelPZ55.AddOrUpdateBIPLinkKeyBinding(SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_NAV, ((TagDataClassPZ55)textBox.Tag).BIPLink);
                }
                if (textBox.Equals(TextBoxStrobeOff))
                {
                    _switchPanelPZ55.AddOrUpdateBIPLinkKeyBinding(SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_STROBE, ((TagDataClassPZ55)textBox.Tag).BIPLink, false);
                }
                if (textBox.Equals(TextBoxStrobeOn))
                {
                    _switchPanelPZ55.AddOrUpdateBIPLinkKeyBinding(SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_STROBE, ((TagDataClassPZ55)textBox.Tag).BIPLink);
                }
                if (textBox.Equals(TextBoxTaxiOff))
                {
                    _switchPanelPZ55.AddOrUpdateBIPLinkKeyBinding(SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_TAXI, ((TagDataClassPZ55)textBox.Tag).BIPLink, false);
                }
                if (textBox.Equals(TextBoxTaxiOn))
                {
                    _switchPanelPZ55.AddOrUpdateBIPLinkKeyBinding(SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_TAXI, ((TagDataClassPZ55)textBox.Tag).BIPLink);
                }
                if (textBox.Equals(TextBoxLandingOff))
                {
                    _switchPanelPZ55.AddOrUpdateBIPLinkKeyBinding(SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_LANDING, ((TagDataClassPZ55)textBox.Tag).BIPLink, false);
                }
                if (textBox.Equals(TextBoxLandingOn))
                {
                    _switchPanelPZ55.AddOrUpdateBIPLinkKeyBinding(SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_LANDING, ((TagDataClassPZ55)textBox.Tag).BIPLink);
                }
                if (textBox.Equals(TextBoxMasterBatOff))
                {
                    _switchPanelPZ55.AddOrUpdateBIPLinkKeyBinding(SwitchPanelPZ55Keys.SWITCHKEY_MASTER_BAT, ((TagDataClassPZ55)textBox.Tag).BIPLink, false);
                }
                if (textBox.Equals(TextBoxMasterBatOn))
                {
                    _switchPanelPZ55.AddOrUpdateBIPLinkKeyBinding(SwitchPanelPZ55Keys.SWITCHKEY_MASTER_BAT, ((TagDataClassPZ55)textBox.Tag).BIPLink);
                }
                if (textBox.Equals(TextBoxMasterAltOff))
                {
                    _switchPanelPZ55.AddOrUpdateBIPLinkKeyBinding(SwitchPanelPZ55Keys.SWITCHKEY_MASTER_ALT, ((TagDataClassPZ55)textBox.Tag).BIPLink, false);
                }
                if (textBox.Equals(TextBoxMasterAltOn))
                {
                    _switchPanelPZ55.AddOrUpdateBIPLinkKeyBinding(SwitchPanelPZ55Keys.SWITCHKEY_MASTER_ALT, ((TagDataClassPZ55)textBox.Tag).BIPLink);
                }
                if (textBox.Equals(TextBoxAvionicsMasterOff))
                {
                    _switchPanelPZ55.AddOrUpdateBIPLinkKeyBinding(SwitchPanelPZ55Keys.SWITCHKEY_AVIONICS_MASTER, ((TagDataClassPZ55)textBox.Tag).BIPLink, false);
                }
                if (textBox.Equals(TextBoxAvionicsMasterOn))
                {
                    _switchPanelPZ55.AddOrUpdateBIPLinkKeyBinding(SwitchPanelPZ55Keys.SWITCHKEY_AVIONICS_MASTER, ((TagDataClassPZ55)textBox.Tag).BIPLink);
                }
                if (textBox.Equals(TextBoxFuelPumpOff))
                {
                    _switchPanelPZ55.AddOrUpdateBIPLinkKeyBinding(SwitchPanelPZ55Keys.SWITCHKEY_FUEL_PUMP, ((TagDataClassPZ55)textBox.Tag).BIPLink, false);
                }
                if (textBox.Equals(TextBoxFuelPumpOn))
                {
                    _switchPanelPZ55.AddOrUpdateBIPLinkKeyBinding(SwitchPanelPZ55Keys.SWITCHKEY_FUEL_PUMP, ((TagDataClassPZ55)textBox.Tag).BIPLink);
                }
                if (textBox.Equals(TextBoxDeIceOff))
                {
                    _switchPanelPZ55.AddOrUpdateBIPLinkKeyBinding(SwitchPanelPZ55Keys.SWITCHKEY_DE_ICE, ((TagDataClassPZ55)textBox.Tag).BIPLink, false);
                }
                if (textBox.Equals(TextBoxDeIceOn))
                {
                    _switchPanelPZ55.AddOrUpdateBIPLinkKeyBinding(SwitchPanelPZ55Keys.SWITCHKEY_DE_ICE, ((TagDataClassPZ55)textBox.Tag).BIPLink);
                }
                if (textBox.Equals(TextBoxPitotHeatOff))
                {
                    _switchPanelPZ55.AddOrUpdateBIPLinkKeyBinding(SwitchPanelPZ55Keys.SWITCHKEY_PITOT_HEAT, ((TagDataClassPZ55)textBox.Tag).BIPLink, false);
                }
                if (textBox.Equals(TextBoxPitotHeatOn))
                {
                    _switchPanelPZ55.AddOrUpdateBIPLinkKeyBinding(SwitchPanelPZ55Keys.SWITCHKEY_PITOT_HEAT, ((TagDataClassPZ55)textBox.Tag).BIPLink);
                }
                if (textBox.Equals(TextBoxGearUp))
                {
                    _switchPanelPZ55.AddOrUpdateBIPLinkKeyBinding(SwitchPanelPZ55Keys.LEVER_GEAR_UP, ((TagDataClassPZ55)textBox.Tag).BIPLink);
                }
                if (textBox.Equals(TextBoxGearDown))
                {
                    _switchPanelPZ55.AddOrUpdateBIPLinkKeyBinding(SwitchPanelPZ55Keys.LEVER_GEAR_DOWN, ((TagDataClassPZ55)textBox.Tag).BIPLink);
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(3011, ex);
            }
        }

        private void UpdateKeyBindingProfileSequencedKeyStrokesPZ55(TextBox textBox)
        {
            try
            {
                if (textBox.Equals(TextBoxKnobOff))
                {
                    _switchPanelPZ55.AddOrUpdateSequencedKeyBinding(textBox.Text, SwitchPanelPZ55Keys.KNOB_ENGINE_OFF, ((TagDataClassPZ55)textBox.Tag).GetKeySequence());
                }
                if (textBox.Equals(TextBoxKnobR))
                {
                    _switchPanelPZ55.AddOrUpdateSequencedKeyBinding(textBox.Text, SwitchPanelPZ55Keys.KNOB_ENGINE_RIGHT, ((TagDataClassPZ55)textBox.Tag).GetKeySequence());
                }
                if (textBox.Equals(TextBoxKnobL))
                {
                    _switchPanelPZ55.AddOrUpdateSequencedKeyBinding(textBox.Text, SwitchPanelPZ55Keys.KNOB_ENGINE_LEFT, ((TagDataClassPZ55)textBox.Tag).GetKeySequence());
                }
                if (textBox.Equals(TextBoxKnobAll))
                {
                    _switchPanelPZ55.AddOrUpdateSequencedKeyBinding(textBox.Text, SwitchPanelPZ55Keys.KNOB_ENGINE_BOTH, ((TagDataClassPZ55)textBox.Tag).GetKeySequence());
                }
                if (textBox.Equals(TextBoxKnobStart))
                {
                    _switchPanelPZ55.AddOrUpdateSequencedKeyBinding(textBox.Text, SwitchPanelPZ55Keys.KNOB_ENGINE_START, ((TagDataClassPZ55)textBox.Tag).GetKeySequence());
                }
                if (textBox.Equals(TextBoxCowlClose))
                {
                    _switchPanelPZ55.AddOrUpdateSequencedKeyBinding(textBox.Text, SwitchPanelPZ55Keys.SWITCHKEY_CLOSE_COWL, ((TagDataClassPZ55)textBox.Tag).GetKeySequence(), false);
                }
                if (textBox.Equals(TextBoxCowlOpen))
                {
                    _switchPanelPZ55.AddOrUpdateSequencedKeyBinding(textBox.Text, SwitchPanelPZ55Keys.SWITCHKEY_CLOSE_COWL, ((TagDataClassPZ55)textBox.Tag).GetKeySequence());
                }
                if (textBox.Equals(TextBoxPanelOff))
                {
                    _switchPanelPZ55.AddOrUpdateSequencedKeyBinding(textBox.Text, SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_PANEL, ((TagDataClassPZ55)textBox.Tag).GetKeySequence(), false);
                }
                if (textBox.Equals(TextBoxPanelOn))
                {
                    _switchPanelPZ55.AddOrUpdateSequencedKeyBinding(textBox.Text, SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_PANEL, ((TagDataClassPZ55)textBox.Tag).GetKeySequence());
                }
                if (textBox.Equals(TextBoxBeaconOff))
                {
                    _switchPanelPZ55.AddOrUpdateSequencedKeyBinding(textBox.Text, SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_BEACON, ((TagDataClassPZ55)textBox.Tag).GetKeySequence(), false);
                }
                if (textBox.Equals(TextBoxBeaconOn))
                {
                    _switchPanelPZ55.AddOrUpdateSequencedKeyBinding(textBox.Text, SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_BEACON, ((TagDataClassPZ55)textBox.Tag).GetKeySequence());
                }
                if (textBox.Equals(TextBoxNavOff))
                {
                    _switchPanelPZ55.AddOrUpdateSequencedKeyBinding(textBox.Text, SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_NAV, ((TagDataClassPZ55)textBox.Tag).GetKeySequence(), false);
                }
                if (textBox.Equals(TextBoxNavOn))
                {
                    _switchPanelPZ55.AddOrUpdateSequencedKeyBinding(textBox.Text, SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_NAV, ((TagDataClassPZ55)textBox.Tag).GetKeySequence());
                }
                if (textBox.Equals(TextBoxStrobeOff))
                {
                    _switchPanelPZ55.AddOrUpdateSequencedKeyBinding(textBox.Text, SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_STROBE, ((TagDataClassPZ55)textBox.Tag).GetKeySequence(), false);
                }
                if (textBox.Equals(TextBoxStrobeOn))
                {
                    _switchPanelPZ55.AddOrUpdateSequencedKeyBinding(textBox.Text, SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_STROBE, ((TagDataClassPZ55)textBox.Tag).GetKeySequence());
                }
                if (textBox.Equals(TextBoxTaxiOff))
                {
                    _switchPanelPZ55.AddOrUpdateSequencedKeyBinding(textBox.Text, SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_TAXI, ((TagDataClassPZ55)textBox.Tag).GetKeySequence(), false);
                }
                if (textBox.Equals(TextBoxTaxiOn))
                {
                    _switchPanelPZ55.AddOrUpdateSequencedKeyBinding(textBox.Text, SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_TAXI, ((TagDataClassPZ55)textBox.Tag).GetKeySequence());
                }
                if (textBox.Equals(TextBoxLandingOff))
                {
                    _switchPanelPZ55.AddOrUpdateSequencedKeyBinding(textBox.Text, SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_LANDING, ((TagDataClassPZ55)textBox.Tag).GetKeySequence(), false);
                }
                if (textBox.Equals(TextBoxLandingOn))
                {
                    _switchPanelPZ55.AddOrUpdateSequencedKeyBinding(textBox.Text, SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_LANDING, ((TagDataClassPZ55)textBox.Tag).GetKeySequence());
                }
                if (textBox.Equals(TextBoxMasterBatOff))
                {
                    _switchPanelPZ55.AddOrUpdateSequencedKeyBinding(textBox.Text, SwitchPanelPZ55Keys.SWITCHKEY_MASTER_BAT, ((TagDataClassPZ55)textBox.Tag).GetKeySequence(), false);
                }
                if (textBox.Equals(TextBoxMasterBatOn))
                {
                    _switchPanelPZ55.AddOrUpdateSequencedKeyBinding(textBox.Text, SwitchPanelPZ55Keys.SWITCHKEY_MASTER_BAT, ((TagDataClassPZ55)textBox.Tag).GetKeySequence());
                }
                if (textBox.Equals(TextBoxMasterAltOff))
                {
                    _switchPanelPZ55.AddOrUpdateSequencedKeyBinding(textBox.Text, SwitchPanelPZ55Keys.SWITCHKEY_MASTER_ALT, ((TagDataClassPZ55)textBox.Tag).GetKeySequence(), false);
                }
                if (textBox.Equals(TextBoxMasterAltOn))
                {
                    _switchPanelPZ55.AddOrUpdateSequencedKeyBinding(textBox.Text, SwitchPanelPZ55Keys.SWITCHKEY_MASTER_ALT, ((TagDataClassPZ55)textBox.Tag).GetKeySequence());
                }
                if (textBox.Equals(TextBoxAvionicsMasterOff))
                {
                    _switchPanelPZ55.AddOrUpdateSequencedKeyBinding(textBox.Text, SwitchPanelPZ55Keys.SWITCHKEY_AVIONICS_MASTER, ((TagDataClassPZ55)textBox.Tag).GetKeySequence(), false);
                }
                if (textBox.Equals(TextBoxAvionicsMasterOn))
                {
                    _switchPanelPZ55.AddOrUpdateSequencedKeyBinding(textBox.Text, SwitchPanelPZ55Keys.SWITCHKEY_AVIONICS_MASTER, ((TagDataClassPZ55)textBox.Tag).GetKeySequence());
                }
                if (textBox.Equals(TextBoxFuelPumpOff))
                {
                    _switchPanelPZ55.AddOrUpdateSequencedKeyBinding(textBox.Text, SwitchPanelPZ55Keys.SWITCHKEY_FUEL_PUMP, ((TagDataClassPZ55)textBox.Tag).GetKeySequence(), false);
                }
                if (textBox.Equals(TextBoxFuelPumpOn))
                {
                    _switchPanelPZ55.AddOrUpdateSequencedKeyBinding(textBox.Text, SwitchPanelPZ55Keys.SWITCHKEY_FUEL_PUMP, ((TagDataClassPZ55)textBox.Tag).GetKeySequence());
                }
                if (textBox.Equals(TextBoxDeIceOff))
                {
                    _switchPanelPZ55.AddOrUpdateSequencedKeyBinding(textBox.Text, SwitchPanelPZ55Keys.SWITCHKEY_DE_ICE, ((TagDataClassPZ55)textBox.Tag).GetKeySequence(), false);
                }
                if (textBox.Equals(TextBoxDeIceOn))
                {
                    _switchPanelPZ55.AddOrUpdateSequencedKeyBinding(textBox.Text, SwitchPanelPZ55Keys.SWITCHKEY_DE_ICE, ((TagDataClassPZ55)textBox.Tag).GetKeySequence());
                }
                if (textBox.Equals(TextBoxPitotHeatOff))
                {
                    _switchPanelPZ55.AddOrUpdateSequencedKeyBinding(textBox.Text, SwitchPanelPZ55Keys.SWITCHKEY_PITOT_HEAT, ((TagDataClassPZ55)textBox.Tag).GetKeySequence(), false);
                }
                if (textBox.Equals(TextBoxPitotHeatOn))
                {
                    _switchPanelPZ55.AddOrUpdateSequencedKeyBinding(textBox.Text, SwitchPanelPZ55Keys.SWITCHKEY_PITOT_HEAT, ((TagDataClassPZ55)textBox.Tag).GetKeySequence());
                }
                if (textBox.Equals(TextBoxGearUp))
                {
                    _switchPanelPZ55.AddOrUpdateSequencedKeyBinding(textBox.Text, SwitchPanelPZ55Keys.LEVER_GEAR_UP, ((TagDataClassPZ55)textBox.Tag).GetKeySequence());
                }
                if (textBox.Equals(TextBoxGearDown))
                {
                    _switchPanelPZ55.AddOrUpdateSequencedKeyBinding(textBox.Text, SwitchPanelPZ55Keys.LEVER_GEAR_DOWN, ((TagDataClassPZ55)textBox.Tag).GetKeySequence());
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
                if (!((TagDataClassPZ55)textBox.Tag).ContainsOSKeyPress() || ((TagDataClassPZ55)textBox.Tag).KeyPress.KeySequence.Count == 0)
                {
                    keyPressLength = KeyPressLength.FiftyMilliSec;
                }
                else
                {
                    keyPressLength = ((TagDataClassPZ55)textBox.Tag).KeyPress.GetLengthOfKeyPress();
                }
                if (textBox.Equals(TextBoxKnobOff))
                {
                    _switchPanelPZ55.AddOrUpdateSingleKeyBinding(SwitchPanelPZ55Keys.KNOB_ENGINE_OFF, TextBoxKnobOff.Text, keyPressLength);
                }
                if (textBox.Equals(TextBoxKnobR))
                {
                    _switchPanelPZ55.AddOrUpdateSingleKeyBinding(SwitchPanelPZ55Keys.KNOB_ENGINE_RIGHT, TextBoxKnobR.Text, keyPressLength);
                }
                if (textBox.Equals(TextBoxKnobL))
                {
                    _switchPanelPZ55.AddOrUpdateSingleKeyBinding(SwitchPanelPZ55Keys.KNOB_ENGINE_LEFT, TextBoxKnobL.Text, keyPressLength);
                }
                if (textBox.Equals(TextBoxKnobAll))
                {
                    _switchPanelPZ55.AddOrUpdateSingleKeyBinding(SwitchPanelPZ55Keys.KNOB_ENGINE_BOTH, TextBoxKnobAll.Text, keyPressLength);
                }
                if (textBox.Equals(TextBoxKnobStart))
                {
                    _switchPanelPZ55.AddOrUpdateSingleKeyBinding(SwitchPanelPZ55Keys.KNOB_ENGINE_START, TextBoxKnobStart.Text, keyPressLength);
                }
                if (textBox.Equals(TextBoxCowlClose))
                {
                    _switchPanelPZ55.AddOrUpdateSingleKeyBinding(SwitchPanelPZ55Keys.SWITCHKEY_CLOSE_COWL, TextBoxCowlClose.Text, keyPressLength, false);
                }
                if (textBox.Equals(TextBoxCowlOpen))
                {
                    _switchPanelPZ55.AddOrUpdateSingleKeyBinding(SwitchPanelPZ55Keys.SWITCHKEY_CLOSE_COWL, TextBoxCowlOpen.Text, keyPressLength);
                }
                if (textBox.Equals(TextBoxPanelOff))
                {
                    _switchPanelPZ55.AddOrUpdateSingleKeyBinding(SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_PANEL, TextBoxPanelOff.Text, keyPressLength, false);
                }
                if (textBox.Equals(TextBoxPanelOn))
                {
                    _switchPanelPZ55.AddOrUpdateSingleKeyBinding(SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_PANEL, TextBoxPanelOn.Text, keyPressLength);
                }
                if (textBox.Equals(TextBoxBeaconOff))
                {
                    _switchPanelPZ55.AddOrUpdateSingleKeyBinding(SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_BEACON, TextBoxBeaconOff.Text, keyPressLength, false);
                }
                if (textBox.Equals(TextBoxBeaconOn))
                {
                    _switchPanelPZ55.AddOrUpdateSingleKeyBinding(SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_BEACON, TextBoxBeaconOn.Text, keyPressLength);
                }
                if (textBox.Equals(TextBoxNavOff))
                {
                    _switchPanelPZ55.AddOrUpdateSingleKeyBinding(SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_NAV, TextBoxNavOff.Text, keyPressLength, false);
                }
                if (textBox.Equals(TextBoxNavOn))
                {
                    _switchPanelPZ55.AddOrUpdateSingleKeyBinding(SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_NAV, TextBoxNavOn.Text, keyPressLength);
                }
                if (textBox.Equals(TextBoxStrobeOff))
                {
                    _switchPanelPZ55.AddOrUpdateSingleKeyBinding(SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_STROBE, TextBoxStrobeOff.Text, keyPressLength, false);
                }
                if (textBox.Equals(TextBoxStrobeOn))
                {
                    _switchPanelPZ55.AddOrUpdateSingleKeyBinding(SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_STROBE, TextBoxStrobeOn.Text, keyPressLength);
                }
                if (textBox.Equals(TextBoxTaxiOff))
                {
                    _switchPanelPZ55.AddOrUpdateSingleKeyBinding(SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_TAXI, TextBoxTaxiOff.Text, keyPressLength, false);
                }
                if (textBox.Equals(TextBoxTaxiOn))
                {
                    _switchPanelPZ55.AddOrUpdateSingleKeyBinding(SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_TAXI, TextBoxTaxiOn.Text, keyPressLength);
                }
                if (textBox.Equals(TextBoxLandingOff))
                {
                    _switchPanelPZ55.AddOrUpdateSingleKeyBinding(SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_LANDING, TextBoxLandingOff.Text, keyPressLength, false);
                }
                if (textBox.Equals(TextBoxLandingOn))
                {
                    _switchPanelPZ55.AddOrUpdateSingleKeyBinding(SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_LANDING, TextBoxLandingOn.Text, keyPressLength);
                }
                if (textBox.Equals(TextBoxMasterBatOff))
                {
                    _switchPanelPZ55.AddOrUpdateSingleKeyBinding(SwitchPanelPZ55Keys.SWITCHKEY_MASTER_BAT, TextBoxMasterBatOff.Text, keyPressLength, false);
                }
                if (textBox.Equals(TextBoxMasterBatOn))
                {
                    _switchPanelPZ55.AddOrUpdateSingleKeyBinding(SwitchPanelPZ55Keys.SWITCHKEY_MASTER_BAT, TextBoxMasterBatOn.Text, keyPressLength);
                }
                if (textBox.Equals(TextBoxMasterAltOff))
                {
                    _switchPanelPZ55.AddOrUpdateSingleKeyBinding(SwitchPanelPZ55Keys.SWITCHKEY_MASTER_ALT, TextBoxMasterAltOff.Text, keyPressLength, false);
                }
                if (textBox.Equals(TextBoxMasterAltOn))
                {
                    _switchPanelPZ55.AddOrUpdateSingleKeyBinding(SwitchPanelPZ55Keys.SWITCHKEY_MASTER_ALT, TextBoxMasterAltOn.Text, keyPressLength);
                }
                if (textBox.Equals(TextBoxAvionicsMasterOff))
                {
                    _switchPanelPZ55.AddOrUpdateSingleKeyBinding(SwitchPanelPZ55Keys.SWITCHKEY_AVIONICS_MASTER, TextBoxAvionicsMasterOff.Text, keyPressLength, false);
                }
                if (textBox.Equals(TextBoxAvionicsMasterOn))
                {
                    _switchPanelPZ55.AddOrUpdateSingleKeyBinding(SwitchPanelPZ55Keys.SWITCHKEY_AVIONICS_MASTER, TextBoxAvionicsMasterOn.Text, keyPressLength);
                }
                if (textBox.Equals(TextBoxFuelPumpOff))
                {
                    _switchPanelPZ55.AddOrUpdateSingleKeyBinding(SwitchPanelPZ55Keys.SWITCHKEY_FUEL_PUMP, TextBoxFuelPumpOff.Text, keyPressLength, false);
                }
                if (textBox.Equals(TextBoxFuelPumpOn))
                {
                    _switchPanelPZ55.AddOrUpdateSingleKeyBinding(SwitchPanelPZ55Keys.SWITCHKEY_FUEL_PUMP, TextBoxFuelPumpOn.Text, keyPressLength);
                }
                if (textBox.Equals(TextBoxDeIceOff))
                {
                    _switchPanelPZ55.AddOrUpdateSingleKeyBinding(SwitchPanelPZ55Keys.SWITCHKEY_DE_ICE, TextBoxDeIceOff.Text, keyPressLength, false);
                }
                if (textBox.Equals(TextBoxDeIceOn))
                {
                    _switchPanelPZ55.AddOrUpdateSingleKeyBinding(SwitchPanelPZ55Keys.SWITCHKEY_DE_ICE, TextBoxDeIceOn.Text, keyPressLength);
                }
                if (textBox.Equals(TextBoxPitotHeatOff))
                {
                    _switchPanelPZ55.AddOrUpdateSingleKeyBinding(SwitchPanelPZ55Keys.SWITCHKEY_PITOT_HEAT, TextBoxPitotHeatOff.Text, keyPressLength, false);
                }
                if (textBox.Equals(TextBoxPitotHeatOn))
                {
                    _switchPanelPZ55.AddOrUpdateSingleKeyBinding(SwitchPanelPZ55Keys.SWITCHKEY_PITOT_HEAT, TextBoxPitotHeatOn.Text, keyPressLength);
                }
                if (textBox.Equals(TextBoxGearUp))
                {
                    _switchPanelPZ55.AddOrUpdateSingleKeyBinding(SwitchPanelPZ55Keys.LEVER_GEAR_UP, TextBoxGearUp.Text, keyPressLength);
                }
                if (textBox.Equals(TextBoxGearDown))
                {
                    _switchPanelPZ55.AddOrUpdateSingleKeyBinding(SwitchPanelPZ55Keys.LEVER_GEAR_DOWN, TextBoxGearDown.Text, keyPressLength);
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
                /*if (((TextBoxTagHolderClass)textBox.Tag) == null)
                {
                    return;
                }*/
                if (((TagDataClassPZ55)textBox.Tag).ContainsDCSBIOS())
                {
                    dcsBiosInputs = ((TagDataClassPZ55)textBox.Tag).DCSBIOSInputs;
                }
                if (textBox.Equals(TextBoxKnobOff))
                {
                    _switchPanelPZ55.AddOrUpdateDCSBIOSBinding(SwitchPanelPZ55Keys.KNOB_ENGINE_OFF, dcsBiosInputs, textBox.Text);
                }
                if (textBox.Equals(TextBoxKnobR))
                {
                    _switchPanelPZ55.AddOrUpdateDCSBIOSBinding(SwitchPanelPZ55Keys.KNOB_ENGINE_RIGHT, dcsBiosInputs, textBox.Text);
                }
                if (textBox.Equals(TextBoxKnobL))
                {
                    _switchPanelPZ55.AddOrUpdateDCSBIOSBinding(SwitchPanelPZ55Keys.KNOB_ENGINE_LEFT, dcsBiosInputs, textBox.Text);
                }
                if (textBox.Equals(TextBoxKnobAll))
                {
                    _switchPanelPZ55.AddOrUpdateDCSBIOSBinding(SwitchPanelPZ55Keys.KNOB_ENGINE_BOTH, dcsBiosInputs, textBox.Text);
                }
                if (textBox.Equals(TextBoxKnobStart))
                {
                    _switchPanelPZ55.AddOrUpdateDCSBIOSBinding(SwitchPanelPZ55Keys.KNOB_ENGINE_START, dcsBiosInputs, textBox.Text);
                }
                if (textBox.Equals(TextBoxCowlClose))
                {
                    _switchPanelPZ55.AddOrUpdateDCSBIOSBinding(SwitchPanelPZ55Keys.SWITCHKEY_CLOSE_COWL, dcsBiosInputs, textBox.Text, false);
                }
                if (textBox.Equals(TextBoxCowlOpen))
                {
                    _switchPanelPZ55.AddOrUpdateDCSBIOSBinding(SwitchPanelPZ55Keys.SWITCHKEY_CLOSE_COWL, dcsBiosInputs, textBox.Text);
                }
                if (textBox.Equals(TextBoxPanelOff))
                {
                    _switchPanelPZ55.AddOrUpdateDCSBIOSBinding(SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_PANEL, dcsBiosInputs, textBox.Text, false);
                }
                if (textBox.Equals(TextBoxPanelOn))
                {
                    _switchPanelPZ55.AddOrUpdateDCSBIOSBinding(SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_PANEL, dcsBiosInputs, textBox.Text);
                }
                if (textBox.Equals(TextBoxBeaconOff))
                {
                    _switchPanelPZ55.AddOrUpdateDCSBIOSBinding(SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_BEACON, dcsBiosInputs, textBox.Text, false);
                }
                if (textBox.Equals(TextBoxBeaconOn))
                {
                    _switchPanelPZ55.AddOrUpdateDCSBIOSBinding(SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_BEACON, dcsBiosInputs, textBox.Text);
                }
                if (textBox.Equals(TextBoxNavOff))
                {
                    _switchPanelPZ55.AddOrUpdateDCSBIOSBinding(SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_NAV, dcsBiosInputs, textBox.Text, false);
                }
                if (textBox.Equals(TextBoxNavOn))
                {
                    _switchPanelPZ55.AddOrUpdateDCSBIOSBinding(SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_NAV, dcsBiosInputs, textBox.Text);
                }
                if (textBox.Equals(TextBoxStrobeOff))
                {
                    _switchPanelPZ55.AddOrUpdateDCSBIOSBinding(SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_STROBE, dcsBiosInputs, textBox.Text, false);
                }
                if (textBox.Equals(TextBoxStrobeOn))
                {
                    _switchPanelPZ55.AddOrUpdateDCSBIOSBinding(SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_STROBE, dcsBiosInputs, textBox.Text);
                }
                if (textBox.Equals(TextBoxTaxiOff))
                {
                    _switchPanelPZ55.AddOrUpdateDCSBIOSBinding(SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_TAXI, dcsBiosInputs, textBox.Text, false);
                }
                if (textBox.Equals(TextBoxTaxiOn))
                {
                    _switchPanelPZ55.AddOrUpdateDCSBIOSBinding(SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_TAXI, dcsBiosInputs, textBox.Text);
                }
                if (textBox.Equals(TextBoxLandingOff))
                {
                    _switchPanelPZ55.AddOrUpdateDCSBIOSBinding(SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_LANDING, dcsBiosInputs, textBox.Text, false);
                }
                if (textBox.Equals(TextBoxLandingOn))
                {
                    _switchPanelPZ55.AddOrUpdateDCSBIOSBinding(SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_LANDING, dcsBiosInputs, textBox.Text);
                }
                if (textBox.Equals(TextBoxMasterBatOff))
                {
                    _switchPanelPZ55.AddOrUpdateDCSBIOSBinding(SwitchPanelPZ55Keys.SWITCHKEY_MASTER_BAT, dcsBiosInputs, textBox.Text, false);
                }
                if (textBox.Equals(TextBoxMasterBatOn))
                {
                    _switchPanelPZ55.AddOrUpdateDCSBIOSBinding(SwitchPanelPZ55Keys.SWITCHKEY_MASTER_BAT, dcsBiosInputs, textBox.Text);
                }
                if (textBox.Equals(TextBoxMasterAltOff))
                {
                    _switchPanelPZ55.AddOrUpdateDCSBIOSBinding(SwitchPanelPZ55Keys.SWITCHKEY_MASTER_ALT, dcsBiosInputs, textBox.Text, false);
                }
                if (textBox.Equals(TextBoxMasterAltOn))
                {
                    _switchPanelPZ55.AddOrUpdateDCSBIOSBinding(SwitchPanelPZ55Keys.SWITCHKEY_MASTER_ALT, dcsBiosInputs, textBox.Text);
                }
                if (textBox.Equals(TextBoxAvionicsMasterOff))
                {
                    _switchPanelPZ55.AddOrUpdateDCSBIOSBinding(SwitchPanelPZ55Keys.SWITCHKEY_AVIONICS_MASTER, dcsBiosInputs, textBox.Text, false);
                }
                if (textBox.Equals(TextBoxAvionicsMasterOn))
                {
                    _switchPanelPZ55.AddOrUpdateDCSBIOSBinding(SwitchPanelPZ55Keys.SWITCHKEY_AVIONICS_MASTER, dcsBiosInputs, textBox.Text);
                }
                if (textBox.Equals(TextBoxFuelPumpOff))
                {
                    _switchPanelPZ55.AddOrUpdateDCSBIOSBinding(SwitchPanelPZ55Keys.SWITCHKEY_FUEL_PUMP, dcsBiosInputs, textBox.Text, false);
                }
                if (textBox.Equals(TextBoxFuelPumpOn))
                {
                    _switchPanelPZ55.AddOrUpdateDCSBIOSBinding(SwitchPanelPZ55Keys.SWITCHKEY_FUEL_PUMP, dcsBiosInputs, textBox.Text);
                }
                if (textBox.Equals(TextBoxDeIceOff))
                {
                    _switchPanelPZ55.AddOrUpdateDCSBIOSBinding(SwitchPanelPZ55Keys.SWITCHKEY_DE_ICE, dcsBiosInputs, textBox.Text, false);
                }
                if (textBox.Equals(TextBoxDeIceOn))
                {
                    _switchPanelPZ55.AddOrUpdateDCSBIOSBinding(SwitchPanelPZ55Keys.SWITCHKEY_DE_ICE, dcsBiosInputs, textBox.Text);
                }
                if (textBox.Equals(TextBoxPitotHeatOff))
                {
                    _switchPanelPZ55.AddOrUpdateDCSBIOSBinding(SwitchPanelPZ55Keys.SWITCHKEY_PITOT_HEAT, dcsBiosInputs, textBox.Text, false);
                }
                if (textBox.Equals(TextBoxPitotHeatOn))
                {
                    _switchPanelPZ55.AddOrUpdateDCSBIOSBinding(SwitchPanelPZ55Keys.SWITCHKEY_PITOT_HEAT, dcsBiosInputs, textBox.Text);
                }
                if (textBox.Equals(TextBoxGearUp))
                {
                    _switchPanelPZ55.AddOrUpdateDCSBIOSBinding(SwitchPanelPZ55Keys.LEVER_GEAR_UP, dcsBiosInputs, textBox.Text);
                }
                if (textBox.Equals(TextBoxGearDown))
                {
                    _switchPanelPZ55.AddOrUpdateDCSBIOSBinding(SwitchPanelPZ55Keys.LEVER_GEAR_DOWN, dcsBiosInputs, textBox.Text);
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(345012, ex);
            }
        }


        private SwitchPanelPZ55KeyOnOff GetPZ55Key(TextBox textBox)
        {
            try
            {
                if (textBox.Equals(TextBoxKnobOff))
                {
                    return new SwitchPanelPZ55KeyOnOff(SwitchPanelPZ55Keys.KNOB_ENGINE_OFF, true);
                }
                if (textBox.Equals(TextBoxKnobR))
                {
                    return new SwitchPanelPZ55KeyOnOff(SwitchPanelPZ55Keys.KNOB_ENGINE_RIGHT, true);
                }
                if (textBox.Equals(TextBoxKnobL))
                {
                    return new SwitchPanelPZ55KeyOnOff(SwitchPanelPZ55Keys.KNOB_ENGINE_LEFT, true);
                }
                if (textBox.Equals(TextBoxKnobAll))
                {
                    return new SwitchPanelPZ55KeyOnOff(SwitchPanelPZ55Keys.KNOB_ENGINE_BOTH, true);
                }
                if (textBox.Equals(TextBoxKnobStart))
                {
                    return new SwitchPanelPZ55KeyOnOff(SwitchPanelPZ55Keys.KNOB_ENGINE_START, true);
                }
                if (textBox.Equals(TextBoxCowlClose))
                {
                    return new SwitchPanelPZ55KeyOnOff(SwitchPanelPZ55Keys.SWITCHKEY_CLOSE_COWL, false);
                }
                if (textBox.Equals(TextBoxCowlOpen))
                {
                    return new SwitchPanelPZ55KeyOnOff(SwitchPanelPZ55Keys.SWITCHKEY_CLOSE_COWL, true);
                }
                if (textBox.Equals(TextBoxPanelOff))
                {
                    return new SwitchPanelPZ55KeyOnOff(SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_PANEL, false);
                }
                if (textBox.Equals(TextBoxPanelOn))
                {
                    return new SwitchPanelPZ55KeyOnOff(SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_PANEL, true);
                }
                if (textBox.Equals(TextBoxBeaconOff))
                {
                    return new SwitchPanelPZ55KeyOnOff(SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_BEACON, false);
                }
                if (textBox.Equals(TextBoxBeaconOn))
                {
                    return new SwitchPanelPZ55KeyOnOff(SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_BEACON, true);
                }
                if (textBox.Equals(TextBoxNavOff))
                {
                    return new SwitchPanelPZ55KeyOnOff(SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_NAV, false);
                }
                if (textBox.Equals(TextBoxNavOn))
                {
                    return new SwitchPanelPZ55KeyOnOff(SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_NAV, true);
                }
                if (textBox.Equals(TextBoxStrobeOff))
                {
                    return new SwitchPanelPZ55KeyOnOff(SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_STROBE, false);
                }
                if (textBox.Equals(TextBoxStrobeOn))
                {
                    return new SwitchPanelPZ55KeyOnOff(SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_STROBE, true);
                }
                if (textBox.Equals(TextBoxTaxiOff))
                {
                    return new SwitchPanelPZ55KeyOnOff(SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_TAXI, false);
                }
                if (textBox.Equals(TextBoxTaxiOn))
                {
                    return new SwitchPanelPZ55KeyOnOff(SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_TAXI, true);
                }
                if (textBox.Equals(TextBoxLandingOff))
                {
                    return new SwitchPanelPZ55KeyOnOff(SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_LANDING, false);
                }
                if (textBox.Equals(TextBoxLandingOn))
                {
                    return new SwitchPanelPZ55KeyOnOff(SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_LANDING, true);
                }
                if (textBox.Equals(TextBoxMasterBatOff))
                {
                    return new SwitchPanelPZ55KeyOnOff(SwitchPanelPZ55Keys.SWITCHKEY_MASTER_BAT, false);
                }
                if (textBox.Equals(TextBoxMasterBatOn))
                {
                    return new SwitchPanelPZ55KeyOnOff(SwitchPanelPZ55Keys.SWITCHKEY_MASTER_BAT, true);
                }
                if (textBox.Equals(TextBoxMasterAltOff))
                {
                    return new SwitchPanelPZ55KeyOnOff(SwitchPanelPZ55Keys.SWITCHKEY_MASTER_ALT, false);
                }
                if (textBox.Equals(TextBoxMasterAltOn))
                {
                    return new SwitchPanelPZ55KeyOnOff(SwitchPanelPZ55Keys.SWITCHKEY_MASTER_ALT, true);
                }
                if (textBox.Equals(TextBoxAvionicsMasterOff))
                {
                    return new SwitchPanelPZ55KeyOnOff(SwitchPanelPZ55Keys.SWITCHKEY_AVIONICS_MASTER, false);
                }
                if (textBox.Equals(TextBoxAvionicsMasterOn))
                {
                    return new SwitchPanelPZ55KeyOnOff(SwitchPanelPZ55Keys.SWITCHKEY_AVIONICS_MASTER, true);
                }
                if (textBox.Equals(TextBoxFuelPumpOff))
                {
                    return new SwitchPanelPZ55KeyOnOff(SwitchPanelPZ55Keys.SWITCHKEY_FUEL_PUMP, false);
                }
                if (textBox.Equals(TextBoxFuelPumpOn))
                {
                    return new SwitchPanelPZ55KeyOnOff(SwitchPanelPZ55Keys.SWITCHKEY_FUEL_PUMP, true);
                }
                if (textBox.Equals(TextBoxDeIceOff))
                {
                    return new SwitchPanelPZ55KeyOnOff(SwitchPanelPZ55Keys.SWITCHKEY_DE_ICE, false);
                }
                if (textBox.Equals(TextBoxDeIceOn))
                {
                    return new SwitchPanelPZ55KeyOnOff(SwitchPanelPZ55Keys.SWITCHKEY_DE_ICE, true);
                }
                if (textBox.Equals(TextBoxPitotHeatOff))
                {
                    return new SwitchPanelPZ55KeyOnOff(SwitchPanelPZ55Keys.SWITCHKEY_PITOT_HEAT, false);
                }
                if (textBox.Equals(TextBoxPitotHeatOn))
                {
                    return new SwitchPanelPZ55KeyOnOff(SwitchPanelPZ55Keys.SWITCHKEY_PITOT_HEAT, true);
                }
                if (textBox.Equals(TextBoxGearUp))
                {
                    return new SwitchPanelPZ55KeyOnOff(SwitchPanelPZ55Keys.LEVER_GEAR_UP, true);
                }
                if (textBox.Equals(TextBoxGearDown))
                {
                    return new SwitchPanelPZ55KeyOnOff(SwitchPanelPZ55Keys.LEVER_GEAR_DOWN, true);
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
                foreach (var keyBinding in _switchPanelPZ55.KeyBindingsHashSet)
                {
                    if (keyBinding.SwitchPanelPZ55Key == SwitchPanelPZ55Keys.KNOB_ENGINE_OFF)
                    {
                        if (keyBinding.WhenTurnedOn)
                        {
                            if (keyBinding.OSKeyPress != null)
                            {
                                ((TagDataClassPZ55)TextBoxKnobOff.Tag).KeyPress = keyBinding.OSKeyPress;
                                TextBoxKnobOff.Text = ((TagDataClassPZ55)TextBoxKnobOff.Tag).GetTextBoxKeyPressInfo();
                            }
                        }
                    }
                    if (keyBinding.SwitchPanelPZ55Key == SwitchPanelPZ55Keys.KNOB_ENGINE_RIGHT)
                    {
                        if (keyBinding.WhenTurnedOn)
                        {
                            if (keyBinding.OSKeyPress != null)
                            {
                                ((TagDataClassPZ55)TextBoxKnobR.Tag).KeyPress = keyBinding.OSKeyPress;
                                TextBoxKnobR.Text = ((TagDataClassPZ55)TextBoxKnobR.Tag).GetTextBoxKeyPressInfo();
                            }
                        }
                    }
                    if (keyBinding.SwitchPanelPZ55Key == SwitchPanelPZ55Keys.KNOB_ENGINE_LEFT)
                    {
                        if (keyBinding.WhenTurnedOn)
                        {
                            if (keyBinding.OSKeyPress != null)
                            {
                                ((TagDataClassPZ55)TextBoxKnobL.Tag).KeyPress = keyBinding.OSKeyPress;
                                TextBoxKnobL.Text = ((TagDataClassPZ55)TextBoxKnobL.Tag).GetTextBoxKeyPressInfo();
                            }
                        }
                    }
                    if (keyBinding.SwitchPanelPZ55Key == SwitchPanelPZ55Keys.KNOB_ENGINE_BOTH)
                    {
                        if (keyBinding.WhenTurnedOn)
                        {
                            if (keyBinding.OSKeyPress != null)
                            {
                                ((TagDataClassPZ55)TextBoxKnobAll.Tag).KeyPress = keyBinding.OSKeyPress;
                                TextBoxKnobAll.Text = ((TagDataClassPZ55)TextBoxKnobAll.Tag).GetTextBoxKeyPressInfo();
                            }
                        }
                    }
                    if (keyBinding.SwitchPanelPZ55Key == SwitchPanelPZ55Keys.KNOB_ENGINE_START)
                    {
                        if (keyBinding.WhenTurnedOn)
                        {
                            if (keyBinding.OSKeyPress != null)
                            {
                                ((TagDataClassPZ55)TextBoxKnobStart.Tag).KeyPress = keyBinding.OSKeyPress;
                                TextBoxKnobStart.Text = ((TagDataClassPZ55)TextBoxKnobStart.Tag).GetTextBoxKeyPressInfo();
                            }
                        }
                    }

                    if (keyBinding.SwitchPanelPZ55Key == SwitchPanelPZ55Keys.SWITCHKEY_CLOSE_COWL)
                    {
                        if (keyBinding.WhenTurnedOn)
                        {
                            if (keyBinding.OSKeyPress != null)
                            {
                                ((TagDataClassPZ55)TextBoxCowlOpen.Tag).KeyPress = keyBinding.OSKeyPress;
                                TextBoxCowlOpen.Text = ((TagDataClassPZ55)TextBoxCowlOpen.Tag).GetTextBoxKeyPressInfo();
                            }
                        }
                        else
                        {
                            if (keyBinding.OSKeyPress != null)
                            {
                                ((TagDataClassPZ55)TextBoxCowlClose.Tag).KeyPress = keyBinding.OSKeyPress;
                                TextBoxCowlClose.Text = ((TagDataClassPZ55)TextBoxCowlClose.Tag).GetTextBoxKeyPressInfo();
                            }
                        }
                    }
                    if (keyBinding.SwitchPanelPZ55Key == SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_PANEL)
                    {
                        if (keyBinding.WhenTurnedOn)
                        {
                            if (keyBinding.OSKeyPress != null)
                            {
                                ((TagDataClassPZ55)TextBoxPanelOn.Tag).KeyPress = keyBinding.OSKeyPress;
                                TextBoxPanelOn.Text = ((TagDataClassPZ55)TextBoxPanelOn.Tag).GetTextBoxKeyPressInfo();
                            }
                        }
                        else
                        {
                            if (keyBinding.OSKeyPress != null)
                            {
                                ((TagDataClassPZ55)TextBoxPanelOff.Tag).KeyPress = keyBinding.OSKeyPress;
                                TextBoxPanelOff.Text = ((TagDataClassPZ55)TextBoxPanelOff.Tag).GetTextBoxKeyPressInfo();
                            }
                        }
                    }
                    if (keyBinding.SwitchPanelPZ55Key == SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_BEACON)
                    {
                        if (keyBinding.WhenTurnedOn)
                        {
                            if (keyBinding.OSKeyPress != null)
                            {
                                ((TagDataClassPZ55)TextBoxBeaconOn.Tag).KeyPress = keyBinding.OSKeyPress;
                                TextBoxBeaconOn.Text = ((TagDataClassPZ55)TextBoxBeaconOn.Tag).GetTextBoxKeyPressInfo();
                            }
                        }
                        else
                        {
                            if (keyBinding.OSKeyPress != null)
                            {
                                ((TagDataClassPZ55)TextBoxBeaconOff.Tag).KeyPress = keyBinding.OSKeyPress;
                                TextBoxBeaconOff.Text = ((TagDataClassPZ55)TextBoxBeaconOff.Tag).GetTextBoxKeyPressInfo();
                            }
                        }
                    }
                    if (keyBinding.SwitchPanelPZ55Key == SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_NAV)
                    {
                        if (keyBinding.WhenTurnedOn)
                        {
                            if (keyBinding.OSKeyPress != null)
                            {
                                ((TagDataClassPZ55)TextBoxNavOn.Tag).KeyPress = keyBinding.OSKeyPress;
                                TextBoxNavOn.Text = ((TagDataClassPZ55)TextBoxNavOn.Tag).GetTextBoxKeyPressInfo();
                            }
                        }
                        else
                        {
                            if (keyBinding.OSKeyPress != null)
                            {
                                ((TagDataClassPZ55)TextBoxNavOff.Tag).KeyPress = keyBinding.OSKeyPress;
                                TextBoxNavOff.Text = ((TagDataClassPZ55)TextBoxNavOff.Tag).GetTextBoxKeyPressInfo();
                            }
                        }
                    }
                    if (keyBinding.SwitchPanelPZ55Key == SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_STROBE)
                    {
                        if (keyBinding.WhenTurnedOn)
                        {
                            if (keyBinding.OSKeyPress != null)
                            {
                                ((TagDataClassPZ55)TextBoxStrobeOn.Tag).KeyPress = keyBinding.OSKeyPress;
                                TextBoxStrobeOn.Text = ((TagDataClassPZ55)TextBoxStrobeOn.Tag).GetTextBoxKeyPressInfo();
                            }
                        }
                        else
                        {
                            if (keyBinding.OSKeyPress != null)
                            {
                                ((TagDataClassPZ55)TextBoxStrobeOff.Tag).KeyPress = keyBinding.OSKeyPress;
                                TextBoxStrobeOff.Text = ((TagDataClassPZ55)TextBoxStrobeOff.Tag).GetTextBoxKeyPressInfo();
                            }
                        }
                    }
                    if (keyBinding.SwitchPanelPZ55Key == SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_TAXI)
                    {
                        if (keyBinding.WhenTurnedOn)
                        {
                            if (keyBinding.OSKeyPress != null)
                            {
                                ((TagDataClassPZ55)TextBoxTaxiOn.Tag).KeyPress = keyBinding.OSKeyPress;
                                TextBoxTaxiOn.Text = ((TagDataClassPZ55)TextBoxTaxiOn.Tag).GetTextBoxKeyPressInfo();
                            }
                        }
                        else
                        {
                            if (keyBinding.OSKeyPress != null)
                            {
                                ((TagDataClassPZ55)TextBoxTaxiOff.Tag).KeyPress = keyBinding.OSKeyPress;
                                TextBoxTaxiOff.Text = ((TagDataClassPZ55)TextBoxTaxiOff.Tag).GetTextBoxKeyPressInfo();
                            }
                        }
                    }
                    if (keyBinding.SwitchPanelPZ55Key == SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_LANDING)
                    {
                        if (keyBinding.WhenTurnedOn)
                        {
                            if (keyBinding.OSKeyPress != null)
                            {
                                ((TagDataClassPZ55)TextBoxLandingOn.Tag).KeyPress = keyBinding.OSKeyPress;
                                TextBoxLandingOn.Text = ((TagDataClassPZ55)TextBoxLandingOn.Tag).GetTextBoxKeyPressInfo();
                            }
                        }
                        else
                        {
                            if (keyBinding.OSKeyPress != null)
                            {
                                ((TagDataClassPZ55)TextBoxLandingOff.Tag).KeyPress = keyBinding.OSKeyPress;
                                TextBoxLandingOff.Text = ((TagDataClassPZ55)TextBoxLandingOff.Tag).GetTextBoxKeyPressInfo();
                            }
                        }
                    }
                    if (keyBinding.SwitchPanelPZ55Key == SwitchPanelPZ55Keys.SWITCHKEY_MASTER_BAT)
                    {
                        if (keyBinding.WhenTurnedOn)
                        {
                            if (keyBinding.OSKeyPress != null)
                            {
                                ((TagDataClassPZ55)TextBoxMasterBatOn.Tag).KeyPress = keyBinding.OSKeyPress;
                                TextBoxMasterBatOn.Text = ((TagDataClassPZ55)TextBoxMasterBatOn.Tag).GetTextBoxKeyPressInfo();
                            }
                        }
                        else
                        {
                            if (keyBinding.OSKeyPress != null)
                            {
                                ((TagDataClassPZ55)TextBoxMasterBatOff.Tag).KeyPress = keyBinding.OSKeyPress;
                                TextBoxMasterBatOff.Text = ((TagDataClassPZ55)TextBoxMasterBatOff.Tag).GetTextBoxKeyPressInfo();
                            }
                        }
                    }
                    if (keyBinding.SwitchPanelPZ55Key == SwitchPanelPZ55Keys.SWITCHKEY_MASTER_ALT)
                    {
                        if (keyBinding.WhenTurnedOn)
                        {
                            if (keyBinding.OSKeyPress != null)
                            {
                                ((TagDataClassPZ55)TextBoxMasterAltOn.Tag).KeyPress = keyBinding.OSKeyPress;
                                TextBoxMasterAltOn.Text = ((TagDataClassPZ55)TextBoxMasterAltOn.Tag).GetTextBoxKeyPressInfo();
                            }
                        }
                        else
                        {
                            if (keyBinding.OSKeyPress != null)
                            {
                                ((TagDataClassPZ55)TextBoxMasterAltOff.Tag).KeyPress = keyBinding.OSKeyPress;
                                TextBoxMasterAltOff.Text = ((TagDataClassPZ55)TextBoxMasterAltOff.Tag).GetTextBoxKeyPressInfo();
                            }
                        }
                    }
                    if (keyBinding.SwitchPanelPZ55Key == SwitchPanelPZ55Keys.SWITCHKEY_AVIONICS_MASTER)
                    {
                        if (keyBinding.WhenTurnedOn)
                        {
                            if (keyBinding.OSKeyPress != null)
                            {
                                ((TagDataClassPZ55)TextBoxAvionicsMasterOn.Tag).KeyPress = keyBinding.OSKeyPress;
                                TextBoxAvionicsMasterOn.Text = ((TagDataClassPZ55)TextBoxAvionicsMasterOn.Tag).GetTextBoxKeyPressInfo();
                            }
                        }
                        else
                        {
                            if (keyBinding.OSKeyPress != null)
                            {
                                ((TagDataClassPZ55)TextBoxAvionicsMasterOff.Tag).KeyPress = keyBinding.OSKeyPress;
                                TextBoxAvionicsMasterOff.Text = ((TagDataClassPZ55)TextBoxAvionicsMasterOff.Tag).GetTextBoxKeyPressInfo();
                            }
                        }
                    }
                    if (keyBinding.SwitchPanelPZ55Key == SwitchPanelPZ55Keys.SWITCHKEY_FUEL_PUMP)
                    {
                        if (keyBinding.WhenTurnedOn)
                        {
                            if (keyBinding.OSKeyPress != null)
                            {
                                ((TagDataClassPZ55)TextBoxFuelPumpOn.Tag).KeyPress = keyBinding.OSKeyPress;
                                TextBoxFuelPumpOn.Text = ((TagDataClassPZ55)TextBoxFuelPumpOn.Tag).GetTextBoxKeyPressInfo();
                            }
                        }
                        else
                        {
                            if (keyBinding.OSKeyPress != null)
                            {
                                ((TagDataClassPZ55)TextBoxFuelPumpOff.Tag).KeyPress = keyBinding.OSKeyPress;
                                TextBoxFuelPumpOff.Text = ((TagDataClassPZ55)TextBoxFuelPumpOff.Tag).GetTextBoxKeyPressInfo();
                            }
                        }
                    }
                    if (keyBinding.SwitchPanelPZ55Key == SwitchPanelPZ55Keys.SWITCHKEY_DE_ICE)
                    {
                        if (keyBinding.WhenTurnedOn)
                        {
                            if (keyBinding.OSKeyPress != null)
                            {
                                ((TagDataClassPZ55)TextBoxDeIceOn.Tag).KeyPress = keyBinding.OSKeyPress;
                                TextBoxDeIceOn.Text = ((TagDataClassPZ55)TextBoxDeIceOn.Tag).GetTextBoxKeyPressInfo();
                            }
                        }
                        else
                        {
                            if (keyBinding.OSKeyPress != null)
                            {
                                ((TagDataClassPZ55)TextBoxDeIceOff.Tag).KeyPress = keyBinding.OSKeyPress;
                                TextBoxDeIceOff.Text = ((TagDataClassPZ55)TextBoxDeIceOff.Tag).GetTextBoxKeyPressInfo();
                            }
                        }
                    }
                    if (keyBinding.SwitchPanelPZ55Key == SwitchPanelPZ55Keys.SWITCHKEY_PITOT_HEAT)
                    {
                        if (keyBinding.WhenTurnedOn)
                        {
                            if (keyBinding.OSKeyPress != null)
                            {
                                ((TagDataClassPZ55)TextBoxPitotHeatOn.Tag).KeyPress = keyBinding.OSKeyPress;
                                TextBoxPitotHeatOn.Text = ((TagDataClassPZ55)TextBoxPitotHeatOn.Tag).GetTextBoxKeyPressInfo();
                            }
                        }
                        else
                        {
                            if (keyBinding.OSKeyPress != null)
                            {
                                ((TagDataClassPZ55)TextBoxPitotHeatOff.Tag).KeyPress = keyBinding.OSKeyPress;
                                TextBoxPitotHeatOff.Text = ((TagDataClassPZ55)TextBoxPitotHeatOff.Tag).GetTextBoxKeyPressInfo();
                            }
                        }
                    }
                    if (keyBinding.SwitchPanelPZ55Key == SwitchPanelPZ55Keys.LEVER_GEAR_UP)
                    {
                        //When gear is down is it OFF -> NOT BEING USED
                        if (keyBinding.WhenTurnedOn)
                        {
                            if (keyBinding.OSKeyPress != null)
                            {
                                ((TagDataClassPZ55)TextBoxGearUp.Tag).KeyPress = keyBinding.OSKeyPress;
                                TextBoxGearUp.Text = ((TagDataClassPZ55)TextBoxGearUp.Tag).GetTextBoxKeyPressInfo();
                            }
                        }
                    }
                    if (keyBinding.SwitchPanelPZ55Key == SwitchPanelPZ55Keys.LEVER_GEAR_DOWN)
                    {
                        //When gear is down is it ON -> BEING USED
                        if (keyBinding.WhenTurnedOn)
                        {
                            if (keyBinding.OSKeyPress != null)
                            {
                                ((TagDataClassPZ55)TextBoxGearDown.Tag).KeyPress = keyBinding.OSKeyPress;
                                TextBoxGearDown.Text = ((TagDataClassPZ55)TextBoxGearDown.Tag).GetTextBoxKeyPressInfo();
                            }
                        }
                    }
                }





                foreach (var dcsBiosBinding in _switchPanelPZ55.DCSBiosBindings)
                {
                    if (dcsBiosBinding.SwitchPanelPZ55Key == SwitchPanelPZ55Keys.KNOB_ENGINE_OFF && dcsBiosBinding.WhenTurnedOn && dcsBiosBinding.DCSBIOSInputs.Count > 0)
                    {
                        ((TagDataClassPZ55)TextBoxKnobOff.Tag).DCSBIOSInputs = dcsBiosBinding.DCSBIOSInputs;
                        TextBoxKnobOff.Text = dcsBiosBinding.Description;
                        TextBoxKnobOff.ToolTip = "DCS-BIOS";
                    }
                    if (dcsBiosBinding.SwitchPanelPZ55Key == SwitchPanelPZ55Keys.KNOB_ENGINE_RIGHT && dcsBiosBinding.WhenTurnedOn && dcsBiosBinding.DCSBIOSInputs.Count > 0)
                    {
                        ((TagDataClassPZ55)TextBoxKnobR.Tag).DCSBIOSInputs = dcsBiosBinding.DCSBIOSInputs;
                        TextBoxKnobR.Text = dcsBiosBinding.Description;
                        TextBoxKnobR.ToolTip = "DCS-BIOS";
                    }
                    if (dcsBiosBinding.SwitchPanelPZ55Key == SwitchPanelPZ55Keys.KNOB_ENGINE_LEFT && dcsBiosBinding.WhenTurnedOn && dcsBiosBinding.DCSBIOSInputs.Count > 0)
                    {
                        ((TagDataClassPZ55)TextBoxKnobL.Tag).DCSBIOSInputs = dcsBiosBinding.DCSBIOSInputs;
                        TextBoxKnobL.Text = dcsBiosBinding.Description;
                        TextBoxKnobL.ToolTip = "DCS-BIOS";
                    }
                    if (dcsBiosBinding.SwitchPanelPZ55Key == SwitchPanelPZ55Keys.KNOB_ENGINE_BOTH && dcsBiosBinding.WhenTurnedOn && dcsBiosBinding.DCSBIOSInputs.Count > 0)
                    {
                        ((TagDataClassPZ55)TextBoxKnobAll.Tag).DCSBIOSInputs = dcsBiosBinding.DCSBIOSInputs;
                        TextBoxKnobAll.Text = dcsBiosBinding.Description;
                        TextBoxKnobAll.ToolTip = "DCS-BIOS";
                    }
                    if (dcsBiosBinding.SwitchPanelPZ55Key == SwitchPanelPZ55Keys.KNOB_ENGINE_START && dcsBiosBinding.WhenTurnedOn && dcsBiosBinding.DCSBIOSInputs.Count > 0)
                    {
                        ((TagDataClassPZ55)TextBoxKnobStart.Tag).DCSBIOSInputs = dcsBiosBinding.DCSBIOSInputs;
                        TextBoxKnobStart.Text = dcsBiosBinding.Description;
                        TextBoxKnobStart.ToolTip = "DCS-BIOS";
                    }

                    if (dcsBiosBinding.SwitchPanelPZ55Key == SwitchPanelPZ55Keys.SWITCHKEY_CLOSE_COWL)
                    {
                        if (dcsBiosBinding.WhenTurnedOn)
                        {
                            if (dcsBiosBinding.DCSBIOSInputs.Count > 0)
                            {
                                ((TagDataClassPZ55)TextBoxCowlOpen.Tag).DCSBIOSInputs = dcsBiosBinding.DCSBIOSInputs;
                                TextBoxCowlOpen.Text = dcsBiosBinding.Description;
                                TextBoxCowlOpen.ToolTip = "DCS-BIOS";
                            }
                        }
                        else
                        {
                            if (dcsBiosBinding.DCSBIOSInputs.Count > 0)
                            {
                                ((TagDataClassPZ55)TextBoxCowlClose.Tag).DCSBIOSInputs = dcsBiosBinding.DCSBIOSInputs;
                                TextBoxCowlClose.Text = dcsBiosBinding.Description;
                                TextBoxCowlClose.ToolTip = "DCS-BIOS";
                            }
                        }
                    }
                    if (dcsBiosBinding.SwitchPanelPZ55Key == SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_PANEL)
                    {
                        if (dcsBiosBinding.WhenTurnedOn)
                        {
                            if (dcsBiosBinding.DCSBIOSInputs.Count > 0)
                            {
                                ((TagDataClassPZ55)TextBoxPanelOn.Tag).DCSBIOSInputs = dcsBiosBinding.DCSBIOSInputs;
                                TextBoxPanelOn.Text = dcsBiosBinding.Description;
                                TextBoxPanelOn.ToolTip = "DCS-BIOS";
                            }
                        }
                        else
                        {
                            if (dcsBiosBinding.DCSBIOSInputs.Count > 0)
                            {
                                ((TagDataClassPZ55)TextBoxPanelOff.Tag).DCSBIOSInputs = dcsBiosBinding.DCSBIOSInputs;
                                TextBoxPanelOff.Text = dcsBiosBinding.Description;
                                TextBoxPanelOff.ToolTip = "DCS-BIOS";
                            }
                        }
                    }
                    if (dcsBiosBinding.SwitchPanelPZ55Key == SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_BEACON)
                    {
                        if (dcsBiosBinding.WhenTurnedOn)
                        {
                            if (dcsBiosBinding.DCSBIOSInputs.Count > 0)
                            {
                                ((TagDataClassPZ55)TextBoxBeaconOn.Tag).DCSBIOSInputs = dcsBiosBinding.DCSBIOSInputs;
                                TextBoxBeaconOn.Text = dcsBiosBinding.Description;
                                TextBoxBeaconOn.ToolTip = "DCS-BIOS";
                            }
                        }
                        else
                        {
                            if (dcsBiosBinding.DCSBIOSInputs.Count > 0)
                            {
                                ((TagDataClassPZ55)TextBoxBeaconOff.Tag).DCSBIOSInputs = dcsBiosBinding.DCSBIOSInputs;
                                TextBoxBeaconOff.Text = dcsBiosBinding.Description;
                                TextBoxBeaconOff.ToolTip = "DCS-BIOS";
                            }
                        }
                    }
                    if (dcsBiosBinding.SwitchPanelPZ55Key == SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_NAV)
                    {
                        if (dcsBiosBinding.WhenTurnedOn)
                        {
                            if (dcsBiosBinding.DCSBIOSInputs.Count > 0)
                            {
                                ((TagDataClassPZ55)TextBoxNavOn.Tag).DCSBIOSInputs = dcsBiosBinding.DCSBIOSInputs;
                                TextBoxNavOn.Text = dcsBiosBinding.Description;
                                TextBoxNavOn.ToolTip = "DCS-BIOS";
                            }
                        }
                        else
                        {
                            if (dcsBiosBinding.DCSBIOSInputs.Count > 0)
                            {
                                ((TagDataClassPZ55)TextBoxNavOff.Tag).DCSBIOSInputs = dcsBiosBinding.DCSBIOSInputs;
                                TextBoxNavOff.Text = dcsBiosBinding.Description;
                                TextBoxNavOff.ToolTip = "DCS-BIOS";
                            }
                        }
                    }
                    if (dcsBiosBinding.SwitchPanelPZ55Key == SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_STROBE)
                    {
                        if (dcsBiosBinding.WhenTurnedOn)
                        {
                            if (dcsBiosBinding.DCSBIOSInputs.Count > 0)
                            {
                                ((TagDataClassPZ55)TextBoxStrobeOn.Tag).DCSBIOSInputs = dcsBiosBinding.DCSBIOSInputs;
                                TextBoxStrobeOn.Text = dcsBiosBinding.Description;
                                TextBoxStrobeOn.ToolTip = "DCS-BIOS";
                            }
                        }
                        else
                        {
                            if (dcsBiosBinding.DCSBIOSInputs.Count > 0)
                            {
                                ((TagDataClassPZ55)TextBoxStrobeOff.Tag).DCSBIOSInputs = dcsBiosBinding.DCSBIOSInputs;
                                TextBoxStrobeOff.Text = dcsBiosBinding.Description;
                                TextBoxStrobeOff.ToolTip = "DCS-BIOS";
                            }
                        }
                    }
                    if (dcsBiosBinding.SwitchPanelPZ55Key == SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_TAXI)
                    {
                        if (dcsBiosBinding.WhenTurnedOn)
                        {
                            if (dcsBiosBinding.DCSBIOSInputs.Count > 0)
                            {
                                ((TagDataClassPZ55)TextBoxTaxiOn.Tag).DCSBIOSInputs = dcsBiosBinding.DCSBIOSInputs;
                                TextBoxTaxiOn.Text = dcsBiosBinding.Description;
                                TextBoxTaxiOn.ToolTip = "DCS-BIOS";
                            }
                        }
                        else
                        {
                            if (dcsBiosBinding.DCSBIOSInputs.Count > 0)
                            {
                                ((TagDataClassPZ55)TextBoxTaxiOff.Tag).DCSBIOSInputs = dcsBiosBinding.DCSBIOSInputs;
                                TextBoxTaxiOff.Text = dcsBiosBinding.Description;
                                TextBoxTaxiOff.ToolTip = "DCS-BIOS";
                            }
                        }
                    }
                    if (dcsBiosBinding.SwitchPanelPZ55Key == SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_LANDING)
                    {
                        if (dcsBiosBinding.WhenTurnedOn)
                        {
                            if (dcsBiosBinding.DCSBIOSInputs.Count > 0)
                            {
                                ((TagDataClassPZ55)TextBoxLandingOn.Tag).DCSBIOSInputs = dcsBiosBinding.DCSBIOSInputs;
                                TextBoxLandingOn.Text = dcsBiosBinding.Description;
                                TextBoxLandingOn.ToolTip = "DCS-BIOS";
                            }
                        }
                        else
                        {
                            if (dcsBiosBinding.DCSBIOSInputs.Count > 0)
                            {
                                ((TagDataClassPZ55)TextBoxLandingOff.Tag).DCSBIOSInputs = dcsBiosBinding.DCSBIOSInputs;
                                TextBoxLandingOff.Text = dcsBiosBinding.Description;
                                TextBoxLandingOff.ToolTip = "DCS-BIOS";
                            }
                        }
                    }
                    if (dcsBiosBinding.SwitchPanelPZ55Key == SwitchPanelPZ55Keys.SWITCHKEY_MASTER_BAT)
                    {
                        if (dcsBiosBinding.WhenTurnedOn)
                        {
                            if (dcsBiosBinding.DCSBIOSInputs.Count > 0)
                            {
                                ((TagDataClassPZ55)TextBoxMasterBatOn.Tag).DCSBIOSInputs = dcsBiosBinding.DCSBIOSInputs;
                                TextBoxMasterBatOn.Text = dcsBiosBinding.Description;
                                TextBoxMasterBatOn.ToolTip = "DCS-BIOS";
                            }
                        }
                        else
                        {
                            if (dcsBiosBinding.DCSBIOSInputs.Count > 0)
                            {
                                ((TagDataClassPZ55)TextBoxMasterBatOff.Tag).DCSBIOSInputs = dcsBiosBinding.DCSBIOSInputs;
                                TextBoxMasterBatOff.Text = dcsBiosBinding.Description;
                                TextBoxMasterBatOff.ToolTip = "DCS-BIOS";
                            }
                        }
                    }
                    if (dcsBiosBinding.SwitchPanelPZ55Key == SwitchPanelPZ55Keys.SWITCHKEY_MASTER_ALT)
                    {
                        if (dcsBiosBinding.WhenTurnedOn)
                        {
                            if (dcsBiosBinding.DCSBIOSInputs.Count > 0)
                            {
                                ((TagDataClassPZ55)TextBoxMasterAltOn.Tag).DCSBIOSInputs = dcsBiosBinding.DCSBIOSInputs;
                                TextBoxMasterAltOn.Text = dcsBiosBinding.Description;
                                TextBoxMasterAltOn.ToolTip = "DCS-BIOS";
                            }
                        }
                        else
                        {
                            if (dcsBiosBinding.DCSBIOSInputs.Count > 0)
                            {
                                ((TagDataClassPZ55)TextBoxMasterAltOff.Tag).DCSBIOSInputs = dcsBiosBinding.DCSBIOSInputs;
                                TextBoxMasterAltOff.Text = dcsBiosBinding.Description;
                                TextBoxMasterAltOff.ToolTip = "DCS-BIOS";
                            }
                        }
                    }
                    if (dcsBiosBinding.SwitchPanelPZ55Key == SwitchPanelPZ55Keys.SWITCHKEY_AVIONICS_MASTER)
                    {
                        if (dcsBiosBinding.WhenTurnedOn)
                        {
                            if (dcsBiosBinding.DCSBIOSInputs.Count > 0)
                            {
                                ((TagDataClassPZ55)TextBoxAvionicsMasterOn.Tag).DCSBIOSInputs = dcsBiosBinding.DCSBIOSInputs;
                                TextBoxAvionicsMasterOn.Text = dcsBiosBinding.Description;
                                TextBoxAvionicsMasterOn.ToolTip = "DCS-BIOS";
                            }
                        }
                        else
                        {
                            if (dcsBiosBinding.DCSBIOSInputs.Count > 0)
                            {
                                ((TagDataClassPZ55)TextBoxAvionicsMasterOff.Tag).DCSBIOSInputs = dcsBiosBinding.DCSBIOSInputs;
                                TextBoxAvionicsMasterOff.Text = dcsBiosBinding.Description;
                                TextBoxAvionicsMasterOff.ToolTip = "DCS-BIOS";
                            }
                        }
                    }
                    if (dcsBiosBinding.SwitchPanelPZ55Key == SwitchPanelPZ55Keys.SWITCHKEY_FUEL_PUMP)
                    {
                        if (dcsBiosBinding.WhenTurnedOn)
                        {
                            if (dcsBiosBinding.DCSBIOSInputs.Count > 0)
                            {
                                ((TagDataClassPZ55)TextBoxFuelPumpOn.Tag).DCSBIOSInputs = dcsBiosBinding.DCSBIOSInputs;
                                TextBoxFuelPumpOn.Text = dcsBiosBinding.Description;
                                TextBoxFuelPumpOn.ToolTip = "DCS-BIOS";
                            }
                        }
                        else
                        {
                            if (dcsBiosBinding.DCSBIOSInputs.Count > 0)
                            {
                                ((TagDataClassPZ55)TextBoxFuelPumpOff.Tag).DCSBIOSInputs = dcsBiosBinding.DCSBIOSInputs;
                                TextBoxFuelPumpOff.Text = dcsBiosBinding.Description;
                                TextBoxFuelPumpOff.ToolTip = "DCS-BIOS";
                            }
                        }
                    }
                    if (dcsBiosBinding.SwitchPanelPZ55Key == SwitchPanelPZ55Keys.SWITCHKEY_DE_ICE)
                    {
                        if (dcsBiosBinding.WhenTurnedOn)
                        {
                            if (dcsBiosBinding.DCSBIOSInputs.Count > 0)
                            {
                                ((TagDataClassPZ55)TextBoxDeIceOn.Tag).DCSBIOSInputs = dcsBiosBinding.DCSBIOSInputs;
                                TextBoxDeIceOn.Text = dcsBiosBinding.Description;
                                TextBoxDeIceOn.ToolTip = "DCS-BIOS";
                            }
                        }
                        else
                        {
                            if (dcsBiosBinding.DCSBIOSInputs.Count > 0)
                            {
                                ((TagDataClassPZ55)TextBoxDeIceOff.Tag).DCSBIOSInputs = dcsBiosBinding.DCSBIOSInputs;
                                TextBoxDeIceOff.Text = dcsBiosBinding.Description;
                                TextBoxDeIceOff.ToolTip = "DCS-BIOS";
                            }
                        }
                    }
                    if (dcsBiosBinding.SwitchPanelPZ55Key == SwitchPanelPZ55Keys.SWITCHKEY_PITOT_HEAT)
                    {
                        if (dcsBiosBinding.WhenTurnedOn)
                        {
                            if (dcsBiosBinding.DCSBIOSInputs.Count > 0)
                            {
                                ((TagDataClassPZ55)TextBoxPitotHeatOn.Tag).DCSBIOSInputs = dcsBiosBinding.DCSBIOSInputs;
                                TextBoxPitotHeatOn.Text = dcsBiosBinding.Description;
                                TextBoxPitotHeatOn.ToolTip = "DCS-BIOS";
                            }
                        }
                        else
                        {
                            if (dcsBiosBinding.DCSBIOSInputs.Count > 0)
                            {
                                ((TagDataClassPZ55)TextBoxPitotHeatOff.Tag).DCSBIOSInputs = dcsBiosBinding.DCSBIOSInputs;
                                TextBoxPitotHeatOff.Text = dcsBiosBinding.Description;
                                TextBoxPitotHeatOff.ToolTip = "DCS-BIOS";
                            }
                        }
                    }
                    if (dcsBiosBinding.SwitchPanelPZ55Key == SwitchPanelPZ55Keys.LEVER_GEAR_UP && dcsBiosBinding.WhenTurnedOn && dcsBiosBinding.DCSBIOSInputs.Count > 0)
                    {
                        //When gear is down is it OFF -> NOT BEING USED
                        ((TagDataClassPZ55)TextBoxGearUp.Tag).DCSBIOSInputs = dcsBiosBinding.DCSBIOSInputs;
                        TextBoxGearUp.Text = dcsBiosBinding.Description;
                        TextBoxGearUp.ToolTip = "DCS-BIOS";
                    }
                    if (dcsBiosBinding.SwitchPanelPZ55Key == SwitchPanelPZ55Keys.LEVER_GEAR_DOWN && dcsBiosBinding.WhenTurnedOn && dcsBiosBinding.DCSBIOSInputs.Count > 0)
                    {
                        //When gear is down is it ON -> BEING USED
                        ((TagDataClassPZ55)TextBoxGearDown.Tag).DCSBIOSInputs = dcsBiosBinding.DCSBIOSInputs;
                        TextBoxGearDown.Text = dcsBiosBinding.Description;
                        TextBoxGearDown.ToolTip = "DCS-BIOS";
                    }
                }

                foreach (var bipLinkPZ55 in _switchPanelPZ55.BIPLinkHashSet)
                {
                    if (bipLinkPZ55.SwitchPanelPZ55Key == SwitchPanelPZ55Keys.KNOB_ENGINE_OFF && bipLinkPZ55.WhenTurnedOn && bipLinkPZ55.BIPLights.Count > 0)
                    {
                        ((TagDataClassPZ55)TextBoxKnobOff.Tag).BIPLink = bipLinkPZ55;
                        TextBoxKnobOff.Background = Brushes.Bisque;
                    }
                    if (bipLinkPZ55.SwitchPanelPZ55Key == SwitchPanelPZ55Keys.KNOB_ENGINE_RIGHT && bipLinkPZ55.WhenTurnedOn && bipLinkPZ55.BIPLights.Count > 0)
                    {
                        ((TagDataClassPZ55)TextBoxKnobOff.Tag).BIPLink = bipLinkPZ55;
                        TextBoxKnobR.Background = Brushes.Bisque;
                    }
                    if (bipLinkPZ55.SwitchPanelPZ55Key == SwitchPanelPZ55Keys.KNOB_ENGINE_LEFT && bipLinkPZ55.WhenTurnedOn && bipLinkPZ55.BIPLights.Count > 0)
                    {
                        ((TagDataClassPZ55)TextBoxKnobL.Tag).BIPLink = bipLinkPZ55;
                        TextBoxKnobL.Background = Brushes.Bisque;
                    }
                    if (bipLinkPZ55.SwitchPanelPZ55Key == SwitchPanelPZ55Keys.KNOB_ENGINE_BOTH && bipLinkPZ55.WhenTurnedOn && bipLinkPZ55.BIPLights.Count > 0)
                    {
                        ((TagDataClassPZ55)TextBoxKnobAll.Tag).BIPLink = bipLinkPZ55;
                        TextBoxKnobAll.Background = Brushes.Bisque;
                    }
                    if (bipLinkPZ55.SwitchPanelPZ55Key == SwitchPanelPZ55Keys.KNOB_ENGINE_START && bipLinkPZ55.WhenTurnedOn && bipLinkPZ55.BIPLights.Count > 0)
                    {
                        ((TagDataClassPZ55)TextBoxKnobStart.Tag).BIPLink = bipLinkPZ55;
                        TextBoxKnobStart.Background = Brushes.Bisque;
                    }

                    if (bipLinkPZ55.SwitchPanelPZ55Key == SwitchPanelPZ55Keys.SWITCHKEY_CLOSE_COWL)
                    {
                        if (bipLinkPZ55.WhenTurnedOn)
                        {
                            if (bipLinkPZ55.BIPLights.Count > 0)
                            {
                                ((TagDataClassPZ55)TextBoxCowlOpen.Tag).BIPLink = bipLinkPZ55;
                                TextBoxCowlOpen.Background = Brushes.Bisque;
                            }
                        }
                        else
                        {
                            if (bipLinkPZ55.BIPLights.Count > 0)
                            {
                                ((TagDataClassPZ55)TextBoxCowlClose.Tag).BIPLink = bipLinkPZ55;
                                TextBoxCowlClose.Background = Brushes.Bisque;
                            }
                        }
                    }
                    if (bipLinkPZ55.SwitchPanelPZ55Key == SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_PANEL)
                    {
                        if (bipLinkPZ55.WhenTurnedOn)
                        {
                            if (bipLinkPZ55.BIPLights.Count > 0)
                            {
                                ((TagDataClassPZ55)TextBoxPanelOn.Tag).BIPLink = bipLinkPZ55;
                                TextBoxPanelOn.Background = Brushes.Bisque;
                            }
                        }
                        else
                        {
                            if (bipLinkPZ55.BIPLights.Count > 0)
                            {
                                ((TagDataClassPZ55)TextBoxPanelOff.Tag).BIPLink = bipLinkPZ55;
                                TextBoxPanelOff.Background = Brushes.Bisque;
                            }
                        }
                    }
                    if (bipLinkPZ55.SwitchPanelPZ55Key == SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_BEACON)
                    {
                        if (bipLinkPZ55.WhenTurnedOn)
                        {
                            if (bipLinkPZ55.BIPLights.Count > 0)
                            {
                                ((TagDataClassPZ55)TextBoxBeaconOn.Tag).BIPLink = bipLinkPZ55;
                                TextBoxBeaconOn.Background = Brushes.Bisque;
                            }
                        }
                        else
                        {
                            if (bipLinkPZ55.BIPLights.Count > 0)
                            {
                                ((TagDataClassPZ55)TextBoxBeaconOff.Tag).BIPLink = bipLinkPZ55;
                                TextBoxBeaconOff.Background = Brushes.Bisque;
                            }
                        }
                    }
                    if (bipLinkPZ55.SwitchPanelPZ55Key == SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_NAV)
                    {
                        if (bipLinkPZ55.WhenTurnedOn)
                        {
                            if (bipLinkPZ55.BIPLights.Count > 0)
                            {
                                ((TagDataClassPZ55)TextBoxNavOn.Tag).BIPLink = bipLinkPZ55;
                                TextBoxNavOn.Background = Brushes.Bisque;
                            }
                        }
                        else
                        {
                            if (bipLinkPZ55.BIPLights.Count > 0)
                            {
                                ((TagDataClassPZ55)TextBoxNavOff.Tag).BIPLink = bipLinkPZ55;
                                TextBoxNavOff.Background = Brushes.Bisque;
                            }
                        }
                    }
                    if (bipLinkPZ55.SwitchPanelPZ55Key == SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_STROBE)
                    {
                        if (bipLinkPZ55.WhenTurnedOn)
                        {
                            if (bipLinkPZ55.BIPLights.Count > 0)
                            {
                                ((TagDataClassPZ55)TextBoxStrobeOn.Tag).BIPLink = bipLinkPZ55;
                                TextBoxStrobeOn.Background = Brushes.Bisque;
                            }
                        }
                        else
                        {
                            if (bipLinkPZ55.BIPLights.Count > 0)
                            {
                                ((TagDataClassPZ55)TextBoxStrobeOff.Tag).BIPLink = bipLinkPZ55;
                                TextBoxStrobeOff.Background = Brushes.Bisque;
                            }
                        }
                    }
                    if (bipLinkPZ55.SwitchPanelPZ55Key == SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_TAXI)
                    {
                        if (bipLinkPZ55.WhenTurnedOn)
                        {
                            if (bipLinkPZ55.BIPLights.Count > 0)
                            {
                                ((TagDataClassPZ55)TextBoxTaxiOn.Tag).BIPLink = bipLinkPZ55;
                                TextBoxTaxiOn.Background = Brushes.Bisque;
                            }
                        }
                        else
                        {
                            if (bipLinkPZ55.BIPLights.Count > 0)
                            {
                                ((TagDataClassPZ55)TextBoxTaxiOff.Tag).BIPLink = bipLinkPZ55;
                                TextBoxTaxiOff.Background = Brushes.Bisque;
                            }
                        }
                    }
                    if (bipLinkPZ55.SwitchPanelPZ55Key == SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_LANDING)
                    {
                        if (bipLinkPZ55.WhenTurnedOn)
                        {
                            if (bipLinkPZ55.BIPLights.Count > 0)
                            {
                                ((TagDataClassPZ55)TextBoxLandingOn.Tag).BIPLink = bipLinkPZ55;
                                TextBoxLandingOn.Background = Brushes.Bisque;
                            }
                        }
                        else
                        {
                            if (bipLinkPZ55.BIPLights.Count > 0)
                            {
                                ((TagDataClassPZ55)TextBoxLandingOff.Tag).BIPLink = bipLinkPZ55;
                                TextBoxLandingOff.Background = Brushes.Bisque;
                            }
                        }
                    }
                    if (bipLinkPZ55.SwitchPanelPZ55Key == SwitchPanelPZ55Keys.SWITCHKEY_MASTER_BAT)
                    {
                        if (bipLinkPZ55.WhenTurnedOn)
                        {
                            if (bipLinkPZ55.BIPLights.Count > 0)
                            {
                                ((TagDataClassPZ55)TextBoxMasterBatOn.Tag).BIPLink = bipLinkPZ55;
                                TextBoxMasterBatOn.Background = Brushes.Bisque;
                            }
                        }
                        else
                        {
                            if (bipLinkPZ55.BIPLights.Count > 0)
                            {
                                ((TagDataClassPZ55)TextBoxMasterBatOff.Tag).BIPLink = bipLinkPZ55;
                                TextBoxMasterBatOff.Background = Brushes.Bisque;
                            }
                        }
                    }
                    if (bipLinkPZ55.SwitchPanelPZ55Key == SwitchPanelPZ55Keys.SWITCHKEY_MASTER_ALT)
                    {
                        if (bipLinkPZ55.WhenTurnedOn)
                        {
                            if (bipLinkPZ55.BIPLights.Count > 0)
                            {
                                ((TagDataClassPZ55)TextBoxMasterAltOn.Tag).BIPLink = bipLinkPZ55;
                                TextBoxMasterAltOn.Background = Brushes.Bisque;
                            }
                        }
                        else
                        {
                            if (bipLinkPZ55.BIPLights.Count > 0)
                            {
                                ((TagDataClassPZ55)TextBoxMasterAltOff.Tag).BIPLink = bipLinkPZ55;
                                TextBoxMasterAltOff.Background = Brushes.Bisque;
                            }
                        }
                    }
                    if (bipLinkPZ55.SwitchPanelPZ55Key == SwitchPanelPZ55Keys.SWITCHKEY_AVIONICS_MASTER)
                    {
                        if (bipLinkPZ55.WhenTurnedOn)
                        {
                            if (bipLinkPZ55.BIPLights.Count > 0)
                            {
                                ((TagDataClassPZ55)TextBoxAvionicsMasterOn.Tag).BIPLink = bipLinkPZ55;
                                TextBoxAvionicsMasterOn.Background = Brushes.Bisque;
                            }
                        }
                        else
                        {
                            if (bipLinkPZ55.BIPLights.Count > 0)
                            {
                                ((TagDataClassPZ55)TextBoxAvionicsMasterOff.Tag).BIPLink = bipLinkPZ55;
                                TextBoxAvionicsMasterOff.Background = Brushes.Bisque;
                            }
                        }
                    }
                    if (bipLinkPZ55.SwitchPanelPZ55Key == SwitchPanelPZ55Keys.SWITCHKEY_FUEL_PUMP)
                    {
                        if (bipLinkPZ55.WhenTurnedOn)
                        {
                            if (bipLinkPZ55.BIPLights.Count > 0)
                            {
                                ((TagDataClassPZ55)TextBoxFuelPumpOn.Tag).BIPLink = bipLinkPZ55;
                                TextBoxFuelPumpOn.Background = Brushes.Bisque;
                            }
                        }
                        else
                        {
                            if (bipLinkPZ55.BIPLights.Count > 0)
                            {
                                ((TagDataClassPZ55)TextBoxFuelPumpOff.Tag).BIPLink = bipLinkPZ55;
                                TextBoxFuelPumpOff.Background = Brushes.Bisque;
                            }
                        }
                    }
                    if (bipLinkPZ55.SwitchPanelPZ55Key == SwitchPanelPZ55Keys.SWITCHKEY_DE_ICE)
                    {
                        if (bipLinkPZ55.WhenTurnedOn)
                        {
                            if (bipLinkPZ55.BIPLights.Count > 0)
                            {
                                ((TagDataClassPZ55)TextBoxDeIceOn.Tag).BIPLink = bipLinkPZ55;
                                TextBoxDeIceOn.Background = Brushes.Bisque;
                            }
                        }
                        else
                        {
                            if (bipLinkPZ55.BIPLights.Count > 0)
                            {
                                ((TagDataClassPZ55)TextBoxDeIceOff.Tag).BIPLink = bipLinkPZ55;
                                TextBoxDeIceOff.Background = Brushes.Bisque;
                            }
                        }
                    }
                    if (bipLinkPZ55.SwitchPanelPZ55Key == SwitchPanelPZ55Keys.SWITCHKEY_PITOT_HEAT)
                    {
                        if (bipLinkPZ55.WhenTurnedOn)
                        {
                            if (bipLinkPZ55.BIPLights.Count > 0)
                            {
                                ((TagDataClassPZ55)TextBoxPitotHeatOn.Tag).BIPLink = bipLinkPZ55;
                                TextBoxPitotHeatOn.Background = Brushes.Bisque;
                            }
                        }
                        else
                        {
                            if (bipLinkPZ55.BIPLights.Count > 0)
                            {
                                ((TagDataClassPZ55)TextBoxPitotHeatOff.Tag).BIPLink = bipLinkPZ55;
                                TextBoxPitotHeatOff.Background = Brushes.Bisque;
                            }
                        }
                    }
                    if (bipLinkPZ55.SwitchPanelPZ55Key == SwitchPanelPZ55Keys.LEVER_GEAR_UP && bipLinkPZ55.WhenTurnedOn && bipLinkPZ55.BIPLights.Count > 0)
                    {
                        //When gear is down is it OFF -> NOT BEING USED
                        ((TagDataClassPZ55)TextBoxGearUp.Tag).BIPLink = bipLinkPZ55;
                        TextBoxGearUp.Background = Brushes.Bisque;
                    }
                    if (bipLinkPZ55.SwitchPanelPZ55Key == SwitchPanelPZ55Keys.LEVER_GEAR_DOWN && bipLinkPZ55.WhenTurnedOn && bipLinkPZ55.BIPLights.Count > 0)
                    {
                        //When gear is down is it ON -> BEING USED
                        ((TagDataClassPZ55)TextBoxGearDown.Tag).BIPLink = bipLinkPZ55;
                        TextBoxGearDown.Background = Brushes.Bisque;
                    }
                }

                checkBoxManualLEDs.IsChecked = _switchPanelPZ55.ManualLandingGearLeds;
                SetConfigExistsImageVisibility();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(3013, ex);
            }
        }

        private void SetConfigExistsImageVisibility()
        {
            foreach (SwitchPanelPZ55LEDPosition value in Enum.GetValues(typeof(SwitchPanelPZ55LEDPosition)))
            {
                var hasConfiguration = _switchPanelPZ55.LedIsConfigured(value);
                switch (value)
                {
                    case SwitchPanelPZ55LEDPosition.UP:
                        {
                            ImageConfigFoundUpper.Visibility = hasConfiguration ? Visibility.Visible : Visibility.Hidden;
                            break;
                        }
                    case SwitchPanelPZ55LEDPosition.LEFT:
                        {
                            ImageConfigFoundLeft.Visibility = hasConfiguration ? Visibility.Visible : Visibility.Hidden;
                            break;
                        }
                    case SwitchPanelPZ55LEDPosition.RIGHT:
                        {
                            ImageConfigFoundRight.Visibility = hasConfiguration ? Visibility.Visible : Visibility.Hidden;
                            break;
                        }
                }
            }
        }

        private void ButtonGetId_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_switchPanelPZ55 != null)
                {
                    Clipboard.SetText(_switchPanelPZ55.InstanceId);
                    MessageBox.Show("The Instance Id for the panel has been copied to the Clipboard.");
                }
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

        private void CheckBoxManualLEDs_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_switchPanelPZ55 != null)
                {
                    _switchPanelPZ55.ManualLandingGearLeds = checkBoxManualLEDs.IsChecked.HasValue && checkBoxManualLEDs.IsChecked.Value;
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(30545, ex);
            }
        }


    }
}
