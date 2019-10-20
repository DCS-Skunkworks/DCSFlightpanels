
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ClassLibraryCommon;
using DCSFlightpanels.TagDataClasses;
using NonVisuals;
using NonVisuals.Interfaces;
using NonVisuals.Saitek;

namespace DCSFlightpanels.PanelUserControls
{
    /// <summary>
    /// Interaction logic for SwitchPanelPZ55UserControl.xaml
    /// </summary>
    public partial class SwitchPanelPZ55UserControl : UserControlBase, IGamingPanelListener, IProfileHandlerListener, ISaitekUserControl
    {

        private readonly SwitchPanelPZ55 _switchPanelPZ55;
        private readonly TabItem _parentTabItem;
        private string _parentTabItemHeader;
        private readonly Image[] _imageArrayUpper = new Image[4];
        private readonly Image[] _imageArrayLeft = new Image[4];
        private readonly Image[] _imageArrayRight = new Image[4];
        private readonly IGlobalHandler _globalHandler;
        private bool _textBoxTagsSet;
        private bool _controlLoaded;

        public SwitchPanelPZ55UserControl(HIDSkeleton hidSkeleton, TabItem parentTabItem, IGlobalHandler globalHandler)
        {
            InitializeComponent();
            _parentTabItem = parentTabItem;
            _parentTabItemHeader = _parentTabItem.Header.ToString();
            _switchPanelPZ55 = new SwitchPanelPZ55(hidSkeleton);

            _switchPanelPZ55.Attach((IGamingPanelListener)this);
            globalHandler.Attach(_switchPanelPZ55);
            _globalHandler = globalHandler;
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
            SetTextBoxTagObjects();
            SetContextMenuClickHandlers();
            _controlLoaded = true;
            ShowGraphicConfiguration();
        }

        public void BipPanelRegisterEvent(object sender, BipPanelRegisteredEventArgs e)
        {
            var now = DateTime.Now.Ticks;
            RemoveContextMenuClickHandlers();
            SetContextMenuClickHandlers();
        }

        public SaitekPanel GetSaitekPanel()
        {
            return _switchPanelPZ55;
        }

        public string GetName()
        {
            return GetType().Name;
        }

        public void SelectedAirframe(object sender, AirframeEventArgs e)
        {
            try
            {
                foreach (var image in Common.FindVisualChildren<Image>(this))
                {
                    if (image.Name.StartsWith("ImagePZ55LED") && Common.IsOperationModeFlagSet(OperationFlag.KeyboardEmulationOnly))
                    {
                        image.ContextMenu = null;
                    }
                    else
                        if (image.Name.StartsWith("ImagePZ55LED") && image.ContextMenu == null && Common.IsOperationModeFlagSet(OperationFlag.DCSBIOSOutputEnabled))
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
                if (e.GamingPanelEnum == GamingPanelEnum.PZ55SwitchPanel && e.UniqueId.Equals(_switchPanelPZ55.InstanceId))
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
                                Dispatcher?.BeginInvoke((Action)(() => imageArray[0].Visibility = Visibility.Visible));
                                break;
                            }
                        case PanelLEDColor.GREEN:
                            {
                                Dispatcher?.BeginInvoke((Action)(() => imageArray[1].Visibility = Visibility.Visible));
                                break;
                            }
                        case PanelLEDColor.YELLOW:
                            {
                                Dispatcher?.BeginInvoke((Action)(() => imageArray[2].Visibility = Visibility.Visible));
                                break;
                            }
                        case PanelLEDColor.RED:
                            {
                                Dispatcher?.BeginInvoke((Action)(() => imageArray[3].Visibility = Visibility.Visible));
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
                Dispatcher?.BeginInvoke((Action)(() => image1.Visibility = Visibility.Collapsed));
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
                if (e.GamingPanelEnum == GamingPanelEnum.PZ55SwitchPanel && e.UniqueId.Equals(_switchPanelPZ55.InstanceId))
                {
                    //Dispatcher?.BeginInvoke((Action)(() => _parentTabItem.Header = _parentTabItemHeader + " (connected)"));
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
                if (e.GamingPanelEnum == GamingPanelEnum.PZ55SwitchPanel && e.UniqueId.Equals(_switchPanelPZ55.InstanceId))
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
                if (e.UniqueId.Equals(_switchPanelPZ55.InstanceId) && e.GamingPanelEnum == GamingPanelEnum.PZ55SwitchPanel)
                {
                    Dispatcher?.BeginInvoke((Action)(ShowGraphicConfiguration));
                    Dispatcher?.BeginInvoke((Action) (() => TextBoxLogPZ55.Text = ""));
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
                KeySequenceWindow keySequenceWindow;
                if (((TagDataClassPZ55)textBox.Tag).ContainsKeySequence())
                {
                    keySequenceWindow = new KeySequenceWindow(textBox.Text, ((TagDataClassPZ55)textBox.Tag).GetKeySequence());
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
                        ((TagDataClassPZ55)textBox.Tag).KeyPress = keyPress;
                        ((TagDataClassPZ55)textBox.Tag).KeyPress.Information = keySequenceWindow.GetInformation;
                        if (!string.IsNullOrEmpty(keySequenceWindow.GetInformation))
                        {
                            textBox.Text = keySequenceWindow.GetInformation;
                        }
                        UpdateKeyBindingProfileSequencedKeyStrokesPZ55(textBox);
                    }
                    else
                    {
                        //If only one press was created treat it as a simple keypress
                        ((TagDataClassPZ55)textBox.Tag).ClearAll();
                        var keyPress = new KeyPress(sequenceList[0].VirtualKeyCodesAsString, sequenceList[0].LengthOfKeyPress);
                        ((TagDataClassPZ55)textBox.Tag).KeyPress = keyPress;
                        ((TagDataClassPZ55)textBox.Tag).KeyPress.Information = keySequenceWindow.GetInformation;
                        textBox.Text = sequenceList[0].VirtualKeyCodesAsString;
                        UpdateKeyBindingProfileSimpleKeyStrokes(textBox);
                    }
                }
                TextBoxLogPZ55.Focus();
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
                    dcsBIOSControlsConfigsWindow = new DCSBIOSControlsConfigsWindow(_globalHandler.GetAirframe(), textBox.Name.Replace("TextBox", ""), ((TagDataClassPZ55)textBox.Tag).DCSBIOSBinding.DCSBIOSInputs, textBox.Text);
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
                    ((TagDataClassPZ55)textBox.Tag).Consume(dcsBiosInputs);
                    UpdateDCSBIOSBinding(textBox);
                }
                TextBoxLogPZ55.Focus();
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
                if (bipLinkWindow.DialogResult.HasValue && bipLinkWindow.DialogResult == true && bipLinkWindow.IsDirty && bipLinkWindow.BIPLink != null)
                {
                    ((TagDataClassPZ55)textBox.Tag).BIPLink = (BIPLinkPZ55)bipLinkWindow.BIPLink;
                    UpdateBIPLinkBindings(textBox);

                }
                TextBoxLogPZ55.Focus();
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

                if (!((TagDataClassPZ55)textBox.Tag).ContainsSingleKey())
                {
                    return;
                }
                var keyPressLength = ((TagDataClassPZ55)textBox.Tag).KeyPress.GetLengthOfKeyPress();

                foreach (MenuItem item in contextMenu.Items)
                {
                    item.IsChecked = false;
                }
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
                if (textBox == TextBoxLogPZ55)
                {
                    continue;
                }
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
                if (textBox == TextBoxLogPZ55)
                {
                    continue;
                }
                textBox.Tag = new TagDataClassPZ55(textBox, GetPZ55Key(textBox));
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
                    textBox.ContextMenu = contectMenu;
                    textBox.ContextMenuOpening += TextBoxContextMenuOpening;
                }
            }
            if (Common.IsOperationModeFlagSet(OperationFlag.DCSBIOSOutputEnabled))
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
                        if (!Common.IsOperationModeFlagSet(OperationFlag.KeyboardEmulationOnly) && item.Name.Contains("EditDCSBIOS"))
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
                else if (((TagDataClassPZ55)textBox.Tag).ContainsSingleKey())
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
                else if (((TagDataClassPZ55)textBox.Tag).ContainsBIPLink())
                {
                    // 3) 
                    foreach (MenuItem item in contextMenu.Items)
                    {
                        if (!Common.IsOperationModeFlagSet(OperationFlag.KeyboardEmulationOnly) && item.Name.Contains("EditDCSBIOS"))
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
                else if (((TagDataClassPZ55)textBox.Tag).ContainsOSCommand())
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
                SetKeyPressLength(textBox, (MenuItem)sender);

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
                        _switchPanelPZ55.RemoveSwitchPanelSwitchFromList(ControlListPZ55.DCSBIOS, GetPZ55Key(textBox).SwitchPanelPZ55Key, GetPZ55Key(textBox).ButtonState);
                        ((TagDataClassPZ55)textBox.Tag).DCSBIOSBinding = null;
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
                        textBox.Background = Brushes.White;
                        UpdateBIPLinkBindings(textBox);
                    }
                    TextBoxLogPZ55.Focus();
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
                Dispatcher?.BeginInvoke((Action)(() => TextBoxLogPZ55.Focus()));
                foreach (var switchPanelKey in switches)
                {
                    var key = (SwitchPanelKey)switchPanelKey;

                    if (_switchPanelPZ55.ForwardPanelEvent)
                    {
                        if (!string.IsNullOrEmpty(_switchPanelPZ55.GetKeyPressForLoggingPurposes(key)))
                        {
                            Dispatcher?.BeginInvoke(
                                (Action)
                                (() =>
                                 TextBoxLogPZ55.Text =
                                 TextBoxLogPZ55.Text.Insert(0, _switchPanelPZ55.GetKeyPressForLoggingPurposes(key) + "\n")));
                        }
                    }
                    else
                    {
                        Dispatcher?.BeginInvoke(
                            (Action)
                            (() =>
                             TextBoxLogPZ55.Text =
                             TextBoxLogPZ55.Text = TextBoxLogPZ55.Text.Insert(0, "No action taken, panel events Disabled.\n")));
                    }
                }
                SetGraphicsState(switches);
            }
            catch (Exception ex)
            {
                Dispatcher?.BeginInvoke(
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
                                Dispatcher?.BeginInvoke(
                                    (Action)delegate
                                    {
                                        ImageAvMasterOn.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case SwitchPanelPZ55Keys.SWITCHKEY_CLOSE_COWL:
                            {
                                var key = switchPanelKey;
                                Dispatcher?.BeginInvoke(
                                    (Action)delegate
                                    {
                                        //This button is special. The Panel reports the button ON when it us switched upwards towards [CLOSE]. This is confusing semantics.
                                        //The button is considered OFF by the program when it is upwards which is opposite to the other buttons which all are considered ON when upwards.
                                        ImageCowlClosed.Visibility = !key.IsOn ? Visibility.Collapsed : Visibility.Visible;
                                    });
                                break;
                            }
                        case SwitchPanelPZ55Keys.SWITCHKEY_DE_ICE:
                            {
                                var key = switchPanelKey;
                                Dispatcher?.BeginInvoke(
                                    (Action)delegate
                                    {
                                        ImageDeIceOn.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case SwitchPanelPZ55Keys.SWITCHKEY_FUEL_PUMP:
                            {
                                var key = switchPanelKey;
                                Dispatcher?.BeginInvoke(
                                    (Action)delegate
                                    {
                                        ImageFuelPumpOn.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case SwitchPanelPZ55Keys.LEVER_GEAR_DOWN:
                            {
                                var key = switchPanelKey;
                                Dispatcher?.BeginInvoke(
                                    (Action)delegate
                                    {
                                        ImageGearUp.Visibility = key.IsOn ? Visibility.Collapsed : Visibility.Visible;
                                    });
                                break;
                            }
                        case SwitchPanelPZ55Keys.LEVER_GEAR_UP:
                            {
                                var key = switchPanelKey;
                                Dispatcher?.BeginInvoke(
                                    (Action)delegate
                                    {
                                        ImageGearUp.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_BEACON:
                            {
                                var key = switchPanelKey;
                                Dispatcher?.BeginInvoke(
                                    (Action)delegate
                                    {
                                        ImageBeaconOn.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_LANDING:
                            {
                                var key = switchPanelKey;
                                Dispatcher?.BeginInvoke(
                                    (Action)delegate
                                    {
                                        ImageLandingOn.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_NAV:
                            {
                                var key = switchPanelKey;
                                Dispatcher?.BeginInvoke(
                                    (Action)delegate
                                    {
                                        ImageNavOn.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_PANEL:
                            {
                                var key = switchPanelKey;
                                Dispatcher?.BeginInvoke(
                                    (Action)delegate
                                    {
                                        ImagePanelOn.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_STROBE:
                            {
                                var key = switchPanelKey;
                                Dispatcher?.BeginInvoke(
                                    (Action)delegate
                                    {
                                        ImageStrobeOn.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_TAXI:
                            {
                                var key = switchPanelKey;
                                Dispatcher?.BeginInvoke(
                                    (Action)delegate
                                    {
                                        ImageTaxiOn.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case SwitchPanelPZ55Keys.SWITCHKEY_MASTER_ALT:
                            {
                                var key = switchPanelKey;
                                Dispatcher?.BeginInvoke(
                                    (Action)delegate
                                    {
                                        ImageMasterAltOn.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case SwitchPanelPZ55Keys.SWITCHKEY_MASTER_BAT:
                            {
                                var key = switchPanelKey;
                                Dispatcher?.BeginInvoke(
                                    (Action)delegate
                                    {
                                        ImageMasterBatOn.Visibility = key.IsOn ? Visibility.Visible : Visibility.Collapsed;
                                    });
                                break;
                            }
                        case SwitchPanelPZ55Keys.SWITCHKEY_PITOT_HEAT:
                            {
                                var key = switchPanelKey;
                                Dispatcher?.BeginInvoke(
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
                        Dispatcher?.BeginInvoke(
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
                var tag = (TagDataClassPZ55)textBox.Tag;
                _switchPanelPZ55.AddOrUpdateBIPLinkKeyBinding(tag.Key.SwitchPanelPZ55Key, tag.BIPLink, tag.Key.ButtonState);
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
                var tag = (TagDataClassPZ55)textBox.Tag;
                _switchPanelPZ55.AddOrUpdateSequencedKeyBinding(textBox.Text, tag.Key.SwitchPanelPZ55Key, tag.GetKeySequence(), tag.Key.ButtonState);
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(3011, ex);
            }
        }

        private void UpdateOSCommandBindingsPZ55(TextBox textBox)
        {
            try
            {
                var tag = (TagDataClassPZ55)textBox.Tag;
                _switchPanelPZ55.AddOrUpdateOSCommandBinding(tag.Key.SwitchPanelPZ55Key, tag.OSCommandObject, tag.Key.ButtonState);
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
                if (!((TagDataClassPZ55)textBox.Tag).ContainsKeyPress() || ((TagDataClassPZ55)textBox.Tag).KeyPress.KeySequence.Count == 0)
                {
                    keyPressLength = KeyPressLength.FiftyMilliSec;
                }
                else
                {
                    keyPressLength = ((TagDataClassPZ55)textBox.Tag).KeyPress.GetLengthOfKeyPress();
                }

                var tag = (TagDataClassPZ55)textBox.Tag;
                _switchPanelPZ55.AddOrUpdateSingleKeyBinding(tag.Key.SwitchPanelPZ55Key, textBox.Text, keyPressLength, tag.Key.ButtonState);
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
                var tag = (TagDataClassPZ55)textBox.Tag;
                _switchPanelPZ55.AddOrUpdateDCSBIOSBinding(tag.Key.SwitchPanelPZ55Key, tag.DCSBIOSBinding.DCSBIOSInputs, textBox.Text, tag.Key.ButtonState);
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
                foreach (var keyBinding in _switchPanelPZ55.KeyBindingsHashSet)
                {
                    var textBox = GetTextBox(keyBinding.SwitchPanelPZ55Key, keyBinding.WhenTurnedOn);
                    if (keyBinding.OSKeyPress != null)
                    {
                        ((TagDataClassPZ55)textBox.Tag).KeyPress = keyBinding.OSKeyPress;
                    }
                }

                foreach (var osCommand in _switchPanelPZ55.OSCommandHashSet)
                {
                    var textBox = GetTextBox(osCommand.SwitchPanelPZ55Key, osCommand.WhenTurnedOn);
                    if (osCommand.OSCommandObject != null)
                    {
                        ((TagDataClassPZ55)textBox.Tag).OSCommandObject = osCommand.OSCommandObject;
                    }
                }

                foreach (var dcsBiosBinding in _switchPanelPZ55.DCSBiosBindings)
                {
                    var textBox = GetTextBox(dcsBiosBinding.SwitchPanelPZ55Key, dcsBiosBinding.WhenTurnedOn);
                    if (dcsBiosBinding.DCSBIOSInputs.Count > 0)
                    {
                        ((TagDataClassPZ55)textBox.Tag).DCSBIOSBinding = dcsBiosBinding;
                    }
                }

                SetTextBoxBackgroundColors(Brushes.White);
                foreach (var bipLinkPZ55 in _switchPanelPZ55.BIPLinkHashSet)
                {
                    var textBox = GetTextBox(bipLinkPZ55.SwitchPanelPZ55Key, bipLinkPZ55.WhenTurnedOn);
                    if (bipLinkPZ55.BIPLights.Count > 0)
                    {
                        ((TagDataClassPZ55)textBox.Tag).BIPLink = bipLinkPZ55;
                    }
                }

                CheckBoxManualLeDs.IsChecked = _switchPanelPZ55.ManualLandingGearLeds;
                SetConfigExistsImageVisibility();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(3013, ex);
            }
        }

        private void SetTextBoxBackgroundColors(Brush brush)
        {
            foreach (var textBox in Common.FindVisualChildren<TextBox>(this))
            {
                textBox.Background = brush;
            }
            _textBoxTagsSet = true;
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
                    TextBoxLogPZ55.Text = "";
                    TextBoxLogPZ55.Text = _switchPanelPZ55.InstanceId;
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
            var informationWindow = new InformationRichTextWindow(bytes);
            informationWindow.ShowDialog();
        }

        private void CheckBoxManualLEDs_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_switchPanelPZ55 != null)
                {
                    _switchPanelPZ55.ManualLandingGearLeds = CheckBoxManualLeDs.IsChecked.HasValue && CheckBoxManualLeDs.IsChecked.Value;
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(30545, ex);
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
            throw new Exception("Failed to find key based on text box (SwitchPanelPZ55UserControl) : " + textBox.Name);
        }

        private TextBox GetTextBox(SwitchPanelPZ55Keys key, bool whenTurnedOn)
        {
            try
            {
                if (key == SwitchPanelPZ55Keys.KNOB_ENGINE_OFF && whenTurnedOn)
                {
                    return TextBoxKnobOff;
                }
                if (key == SwitchPanelPZ55Keys.KNOB_ENGINE_RIGHT && whenTurnedOn)
                {
                    return TextBoxKnobR;
                }
                if (key == SwitchPanelPZ55Keys.KNOB_ENGINE_LEFT && whenTurnedOn)
                {
                    return TextBoxKnobL;
                }
                if (key == SwitchPanelPZ55Keys.KNOB_ENGINE_BOTH && whenTurnedOn)
                {
                    return TextBoxKnobAll;
                }
                if (key == SwitchPanelPZ55Keys.KNOB_ENGINE_START && whenTurnedOn)
                {
                    return TextBoxKnobStart;
                }
                if (key == SwitchPanelPZ55Keys.SWITCHKEY_CLOSE_COWL && !whenTurnedOn)
                {
                    return TextBoxCowlClose;
                }
                if (key == SwitchPanelPZ55Keys.SWITCHKEY_CLOSE_COWL && whenTurnedOn)
                {
                    return TextBoxCowlOpen;
                }
                if (key == SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_PANEL && !whenTurnedOn)
                {
                    return TextBoxPanelOff;
                }
                if (key == SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_PANEL && whenTurnedOn)
                {
                    return TextBoxPanelOn;
                }
                if (key == SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_BEACON && !whenTurnedOn)
                {
                    return TextBoxBeaconOff;
                }
                if (key == SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_BEACON && whenTurnedOn)
                {
                    return TextBoxBeaconOn;
                }
                if (key == SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_NAV && !whenTurnedOn)
                {
                    return TextBoxNavOff;
                }
                if (key == SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_NAV && whenTurnedOn)
                {
                    return TextBoxNavOn;
                }
                if (key == SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_STROBE && !whenTurnedOn)
                {
                    return TextBoxStrobeOff;
                }
                if (key == SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_STROBE && whenTurnedOn)
                {
                    return TextBoxStrobeOn;
                }
                if (key == SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_TAXI && !whenTurnedOn)
                {
                    return TextBoxTaxiOff;
                }
                if (key == SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_TAXI && whenTurnedOn)
                {
                    return TextBoxTaxiOn;
                }
                if (key == SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_LANDING && !whenTurnedOn)
                {
                    return TextBoxLandingOff;
                }
                if (key == SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_LANDING && whenTurnedOn)
                {
                    return TextBoxLandingOn;
                }
                if (key == SwitchPanelPZ55Keys.SWITCHKEY_MASTER_BAT && !whenTurnedOn)
                {
                    return TextBoxMasterBatOff;
                }
                if (key == SwitchPanelPZ55Keys.SWITCHKEY_MASTER_BAT && whenTurnedOn)
                {
                    return TextBoxMasterBatOn;
                }
                if (key == SwitchPanelPZ55Keys.SWITCHKEY_MASTER_ALT && !whenTurnedOn)
                {
                    return TextBoxMasterAltOff;
                }
                if (key == SwitchPanelPZ55Keys.SWITCHKEY_MASTER_ALT && whenTurnedOn)
                {
                    return TextBoxMasterAltOn;
                }
                if (key == SwitchPanelPZ55Keys.SWITCHKEY_AVIONICS_MASTER && !whenTurnedOn)
                {
                    return TextBoxAvionicsMasterOff;
                }
                if (key == SwitchPanelPZ55Keys.SWITCHKEY_AVIONICS_MASTER && whenTurnedOn)
                {
                    return TextBoxAvionicsMasterOn;
                }
                if (key == SwitchPanelPZ55Keys.SWITCHKEY_FUEL_PUMP && !whenTurnedOn)
                {
                    return TextBoxFuelPumpOff;
                }
                if (key == SwitchPanelPZ55Keys.SWITCHKEY_FUEL_PUMP && whenTurnedOn)
                {
                    return TextBoxFuelPumpOn;
                }
                if (key == SwitchPanelPZ55Keys.SWITCHKEY_DE_ICE && !whenTurnedOn)
                {
                    return TextBoxDeIceOff;
                }
                if (key == SwitchPanelPZ55Keys.SWITCHKEY_DE_ICE && whenTurnedOn)
                {
                    return TextBoxDeIceOn;
                }
                if (key == SwitchPanelPZ55Keys.SWITCHKEY_PITOT_HEAT && !whenTurnedOn)
                {
                    return TextBoxPitotHeatOff;
                }
                if (key == SwitchPanelPZ55Keys.SWITCHKEY_PITOT_HEAT && whenTurnedOn)
                {
                    return TextBoxPitotHeatOn;
                }
                if (key == SwitchPanelPZ55Keys.LEVER_GEAR_UP && whenTurnedOn)
                {
                    return TextBoxGearUp;
                }
                if (key == SwitchPanelPZ55Keys.LEVER_GEAR_DOWN && whenTurnedOn)
                {
                    return TextBoxGearDown;
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(345012, ex);
            }
            throw new Exception("Failed to find text box based on key (SwitchPanelPZ55UserControl)" + key);
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

                ((TagDataClassPZ55)textBox.Tag).ClearAll();
                var vkNull = Enum.GetName(typeof(VirtualKeyCode), VirtualKeyCode.VK_NULL);
                if (string.IsNullOrEmpty(vkNull))
                {
                    return;
                }
                var keyPress = new KeyPress(vkNull, KeyPressLength.FiftyMilliSec);
                ((TagDataClassPZ55)textBox.Tag).KeyPress = keyPress;
                ((TagDataClassPZ55)textBox.Tag).KeyPress.Information = "VK_NULL";
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
                if (((TagDataClassPZ55)textBox.Tag).ContainsOSCommand())
                {
                    osCommandWindow = new OSCommandWindow(((TagDataClassPZ55)textBox.Tag).OSCommandObject);
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
                    ((TagDataClassPZ55)textBox.Tag).OSCommandObject = osCommand;
                    textBox.Text = osCommand.Name;
                    UpdateOSCommandBindingsPZ55(textBox);
                }
                TextBoxLogPZ55.Focus();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(2044, ex);
            }
        }

        
    }
}
