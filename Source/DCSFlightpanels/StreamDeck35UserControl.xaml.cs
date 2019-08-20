using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ClassLibraryCommon;
using DCS_BIOS;
using DCSFlightpanels.Properties;
using NonVisuals;

namespace DCSFlightpanels
{
    /// <summary>
    /// Interaction logic for StreamDeck35UserControl.xaml
    /// </summary>
    public partial class StreamDeck35UserControl : UserControl, IGamingPanelListener, IProfileHandlerListener, ISaitekUserControl
    {
        private readonly StreamDeck35 _streamDeck35;
        private readonly TabItem _parentTabItem;
        private string _parentTabItemHeader;
        private readonly IGlobalHandler _globalHandler;
        private bool _userControlLoaded;
        private bool _textBoxTagsSet;

        public StreamDeck35UserControl(HIDSkeleton hidSkeleton, TabItem parentTabItem, IGlobalHandler globalHandler)
        {
            InitializeComponent();
            _parentTabItem = parentTabItem;
            _parentTabItemHeader = _parentTabItem.Header.ToString();
            _streamDeck35 = new StreamDeck35();
            _streamDeck35.Attach((IGamingPanelListener)this);
            globalHandler.Attach(_streamDeck35);
            _globalHandler = globalHandler;

            HideAllImages();
        }

        private void StreamDeck35UserControl_OnLoaded(object sender, RoutedEventArgs e)
        {
            SetTextBoxTagObjects();
            SetContextMenuClickHandlers();
            _userControlLoaded = true;
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
            return null; //_streamDeck35;
        }

        public void UpdatesHasBeenMissed(object sender, DCSBIOSUpdatesMissedEventArgs e)
        {
            try
            {
                //todo
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(471073, ex);
            }
        }

        public string GetName()
        {
            return GetType().Name;
        }

        public void SelectedAirframe(object sender, AirframeEventArgs e)
        {
            try
            {
                SetApplicationMode();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(471573, ex);
            }
        }

        private void SetApplicationMode()
        {
        }

        public void SwitchesChanged(object sender, SwitchesChangedEventArgs e)
        {
            try
            {
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(1018, ex);
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
                Common.ShowErrorMessageBox(1019, ex);
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
                Common.ShowErrorMessageBox(1020, ex);
            }
        }

        private void ClearAll(bool clearAlsoProfile = true)
        {
            foreach (var textBox in Common.FindVisualChildren<TextBox>(this))
            {
                if (textBox.Equals(TextBoxLogStreamDeck35))
                {
                    continue;
                }
                var tagHolderClass = (TagDataClassStreamDeck)textBox.Tag;
                textBox.Text = "";
                tagHolderClass.ClearAll();
            }
            if (clearAlsoProfile)
            {
                //_streamDeck35.ClearSettings();
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
                if (!textBox.Equals(TextBoxLogStreamDeck35))
                {
                    textBox.Tag = new TagDataClassStreamDeck(textBox, GetStreamDeckKey(textBox));
                }
            }
            _textBoxTagsSet = true;
        }

        public void LedLightChanged(object sender, LedLightChangeEventArgs e)
        {
            try
            {
                //todo
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(1021, ex);
            }
        }

        public void PanelSettingsChanged(object sender, PanelEventArgs e)
        {
            try
            {
                //todo nada?
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(1022, ex);
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
                Common.ShowErrorMessageBox(1023, ex);
            }
        }

        public void SettingsApplied(object sender, PanelEventArgs e)
        {
            try
            {
                /*if (e.UniqueId.Equals(_streamDeck35.InstanceId) && e.GamingPanelEnum == GamingPanelEnum.PZ70MultiPanel)
                {
                    Dispatcher?.BeginInvoke((Action)(ShowGraphicConfiguration));
                    Dispatcher?.BeginInvoke((Action)(() => TextBoxLogStreamDeck35.Text = ""));
                }*/
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(992032, ex);
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
                Common.ShowErrorMessageBox(1025, ex);
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
                Common.ShowErrorMessageBox(1026, ex);
            }
        }

        private void ButtonGetId_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                /*if (_streamDeck35 != null)
                {
                    TextBoxLogStreamDeck35.Text = "";
                    TextBoxLogStreamDeck35.Text = _streamDeck35.InstanceId;
                    Clipboard.SetText(_streamDeck35.InstanceId);
                    MessageBox.Show("Instance id has been copied to the ClipBoard.");
                }*/
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(2000, ex);
            }
        }

        private void SetGraphicsState(HashSet<object> knobs)
        {
            try
            {
                foreach (var streamDeckButton35 in knobs)
                {
                    var streamDeckButton = (StreamDeck35Button)streamDeckButton35;
                    switch (streamDeckButton.Button)
                    {
                        case StreamDeck35Buttons.BUTTON11:
                            {
                                var key = streamDeckButton;
                                Dispatcher?.BeginInvoke(
                                    (Action)delegate
                                   {
                                       Image11.Visibility = key.IsPressed ? Visibility.Visible : Visibility.Collapsed;
                                       if (key.IsPressed)
                                       {
                                           ClearAll(false);
                                           ShowGraphicConfiguration();
                                       }
                                   });
                                break;
                            }
                        case StreamDeck35Buttons.BUTTON12:
                            {
                                var key = streamDeckButton;
                                Dispatcher?.BeginInvoke(
                                    (Action)delegate
                                    {
                                        DotImage12.Visibility = key.IsPressed ? Visibility.Visible : Visibility.Collapsed;
                                        if (key.IsPressed)
                                        {
                                            ClearAll(false);
                                            ShowGraphicConfiguration();
                                        }
                                    });
                                break;
                            }
                        case StreamDeck35Buttons.BUTTON13:
                            {
                                var key = streamDeckButton;
                                Dispatcher?.BeginInvoke(
                                    (Action)delegate
                                    {
                                        DotImage13.Visibility = key.IsPressed ? Visibility.Visible : Visibility.Collapsed;
                                        if (key.IsPressed)
                                        {
                                            ClearAll(false);
                                            ShowGraphicConfiguration();
                                        }
                                    });
                                break;
                            }
                        case StreamDeck35Buttons.BUTTON14:
                            {
                                var key = streamDeckButton;
                                Dispatcher?.BeginInvoke(
                                    (Action)delegate
                                    {
                                        DotImage14.Visibility = key.IsPressed ? Visibility.Visible : Visibility.Collapsed;
                                        if (key.IsPressed)
                                        {
                                            ClearAll(false);
                                            ShowGraphicConfiguration();
                                        }
                                    });
                                break;
                            }
                        case StreamDeck35Buttons.BUTTON15:
                            {
                                var key = streamDeckButton;
                                Dispatcher?.BeginInvoke(
                                    (Action)delegate
                                    {
                                        DotImage15.Visibility = key.IsPressed ? Visibility.Visible : Visibility.Collapsed;
                                        if (key.IsPressed)
                                        {
                                            ClearAll(false);
                                            ShowGraphicConfiguration();
                                        }
                                    });
                                break;
                            }
                        case StreamDeck35Buttons.BUTTON21:
                            {
                                var key = streamDeckButton;
                                Dispatcher?.BeginInvoke(
                                    (Action)delegate
                                    {
                                        DotImage21.Visibility = key.IsPressed ? Visibility.Visible : Visibility.Collapsed;
                                        if (key.IsPressed)
                                        {
                                            ClearAll(false);
                                            ShowGraphicConfiguration();
                                        }
                                    });
                                break;
                            }
                        case StreamDeck35Buttons.BUTTON22:
                            {
                                var key = streamDeckButton;
                                Dispatcher?.BeginInvoke(
                                    (Action)delegate
                                    {
                                        DotImage22.Visibility = key.IsPressed ? Visibility.Visible : Visibility.Collapsed;
                                        if (key.IsPressed)
                                        {
                                            ClearAll(false);
                                            ShowGraphicConfiguration();
                                        }
                                    });
                                break;
                            }
                        case StreamDeck35Buttons.BUTTON23:
                            {
                                var key = streamDeckButton;
                                Dispatcher?.BeginInvoke(
                                    (Action)delegate
                                    {
                                        DotImage23.Visibility = key.IsPressed ? Visibility.Visible : Visibility.Collapsed;
                                        if (key.IsPressed)
                                        {
                                            ClearAll(false);
                                            ShowGraphicConfiguration();
                                        }
                                    });
                                break;
                            }
                        case StreamDeck35Buttons.BUTTON24:
                            {
                                var key = streamDeckButton;
                                Dispatcher?.BeginInvoke(
                                    (Action)delegate
                                    {
                                        DotImage24.Visibility = key.IsPressed ? Visibility.Visible : Visibility.Collapsed;
                                        if (key.IsPressed)
                                        {
                                            ClearAll(false);
                                            ShowGraphicConfiguration();
                                        }
                                    });
                                break;
                            }
                        case StreamDeck35Buttons.BUTTON25:
                            {
                                var key = streamDeckButton;
                                Dispatcher?.BeginInvoke(
                                    (Action)delegate
                                    {
                                        DotImage25.Visibility = key.IsPressed ? Visibility.Visible : Visibility.Collapsed;
                                        if (key.IsPressed)
                                        {
                                            ClearAll(false);
                                            ShowGraphicConfiguration();
                                        }
                                    });
                                break;
                            }
                        case StreamDeck35Buttons.BUTTON31:
                            {
                                var key = streamDeckButton;
                                Dispatcher?.BeginInvoke(
                                    (Action)delegate
                                    {
                                        DotImage31.Visibility = key.IsPressed ? Visibility.Visible : Visibility.Collapsed;
                                        if (key.IsPressed)
                                        {
                                            ClearAll(false);
                                            ShowGraphicConfiguration();
                                        }
                                    });
                                break;
                            }
                        case StreamDeck35Buttons.BUTTON32:
                            {
                                var key = streamDeckButton;
                                Dispatcher?.BeginInvoke(
                                    (Action)delegate
                                    {
                                        DotImage32.Visibility = key.IsPressed ? Visibility.Visible : Visibility.Collapsed;
                                        if (key.IsPressed)
                                        {
                                            ClearAll(false);
                                            ShowGraphicConfiguration();
                                        }
                                    });
                                break;
                            }
                        case StreamDeck35Buttons.BUTTON33:
                            {
                                var key = streamDeckButton;
                                Dispatcher?.BeginInvoke(
                                    (Action)delegate
                                    {
                                        DotImage33.Visibility = key.IsPressed ? Visibility.Visible : Visibility.Collapsed;
                                        if (key.IsPressed)
                                        {
                                            ClearAll(false);
                                            ShowGraphicConfiguration();
                                        }
                                    });
                                break;
                            }
                        case StreamDeck35Buttons.BUTTON34:
                            {
                                var key = streamDeckButton;
                                Dispatcher?.BeginInvoke(
                                    (Action)delegate
                                    {
                                        DotImage34.Visibility = key.IsPressed ? Visibility.Visible : Visibility.Collapsed;
                                        if (key.IsPressed)
                                        {
                                            ClearAll(false);
                                            ShowGraphicConfiguration();
                                        }
                                    });
                                break;
                            }
                        case StreamDeck35Buttons.BUTTON35:
                            {
                                var key = streamDeckButton;
                                Dispatcher?.BeginInvoke(
                                    (Action)delegate
                                    {
                                        DotImage35.Visibility = key.IsPressed ? Visibility.Visible : Visibility.Collapsed;
                                        if (key.IsPressed)
                                        {
                                            ClearAll(false);
                                            ShowGraphicConfiguration();
                                        }
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
            DotImage11.Visibility = Visibility.Collapsed;
            DotImage12.Visibility = Visibility.Collapsed;
            DotImage13.Visibility = Visibility.Collapsed;
            DotImage14.Visibility = Visibility.Collapsed;
            DotImage15.Visibility = Visibility.Collapsed;
            DotImage21.Visibility = Visibility.Collapsed;
            DotImage22.Visibility = Visibility.Collapsed;
            DotImage23.Visibility = Visibility.Collapsed;
            DotImage24.Visibility = Visibility.Collapsed;
            DotImage25.Visibility = Visibility.Collapsed;
            DotImage31.Visibility = Visibility.Collapsed;
            DotImage32.Visibility = Visibility.Collapsed;
            DotImage33.Visibility = Visibility.Collapsed;
            DotImage34.Visibility = Visibility.Collapsed;
            DotImage35.Visibility = Visibility.Collapsed;
        }


        private void TextBoxLostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                var textBox = (TextBox)sender;
                if (((TagDataClassStreamDeck)textBox.Tag).ContainsBIPLink())
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

        private void ShowGraphicConfiguration()
        {
            try
            {
                if (!_userControlLoaded || !_textBoxTagsSet)
                {
                    return;
                }

                SetApplicationMode();

                foreach (var keyBinding in _streamDeck35.KeyBindingsHashSet)
                {
                    var textBox = GetTextBox(keyBinding.StreamDeckButton, keyBinding.WhenTurnedOn);
                    if (keyBinding.OSKeyPress != null)
                    {
                        ((TagDataClassStreamDeck)textBox.Tag).KeyPress = keyBinding.OSKeyPress;
                    }
                }

                foreach (var osCommand in _streamDeck35.OSCommandHashSet)
                {
                    var textBox = GetTextBox(osCommand.StreamDeckButton, osCommand.WhenTurnedOn);
                    if (osCommand.OSCommandObject != null)
                    {
                        ((TagDataClassStreamDeck)textBox.Tag).OSCommandObject = osCommand.OSCommandObject;
                    }
                }

                foreach (var dcsBiosBinding in _streamDeck35.DCSBiosBindings)
                {
                    var textBox = GetTextBox(dcsBiosBinding.StreamDeckButton, dcsBiosBinding.WhenTurnedOn);

                    ((TagDataClassStreamDeck)textBox.Tag).DCSBIOSBinding = dcsBiosBinding;

                }


                foreach (var bipLink in _streamDeck35.BIPLinkHashSet)
                {
                    var textBox = GetTextBox(bipLink.StreamDeckButton, bipLink.WhenTurnedOn);
                    if (bipLink.BIPLights.Count > 0)
                    {
                        ((TagDataClassStreamDeck)textBox.Tag).BIPLink = bipLink;
                    }
                }

            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(993013, ex);
            }
        }


        private void TextBoxShortcutKeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                var textBox = ((TextBox)sender);
                //Check if this textbox contains sequence or DCS-BIOS information. If so then exit
                if (((TagDataClassStreamDeck)textBox.Tag).ContainsKeySequence() || ((TagDataClassStreamDeck)textBox.Tag).ContainsDCSBIOS())
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
                ((TagDataClassStreamDeck)textBox.Tag).KeyPress = new OSKeyPress(result);
                UpdateKeyBindingProfileSequencedKeyStrokesPZ70(textBox);
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(3008, ex);
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
                    if (((TagDataClassStreamDeck)textBox.Tag).ContainsDCSBIOS())
                    {
                        if (MessageBox.Show("Do you want to delete the DCS-BIOS configuration?", "Delete DCS-BIOS control?", MessageBoxButton.OKCancel, MessageBoxImage.Question) != MessageBoxResult.OK)
                        {
                            return;
                        }
                        textBox.Text = "";
                        _streamDeck35.RemoveMultiPanelKnobFromList(ControlListStreamDeck.DCSBIOS, GetStreamDeckKey(textBox).StreamDeckButton, GetStreamDeckKey(textBox).ButtonState);
                        ((TagDataClassStreamDeck)textBox.Tag).DCSBIOSBinding = null;
                    }
                    else if (((TagDataClassStreamDeck)textBox.Tag).ContainsKeySequence())
                    {
                        //Check if this textbox contains sequence information. If so then prompt the user for deletion
                        if (MessageBox.Show("Do you want to delete the key sequence?", "Delete key sequence?", MessageBoxButton.OKCancel, MessageBoxImage.Question) != MessageBoxResult.OK)
                        {
                            return;
                        }
                        ((TagDataClassStreamDeck)textBox.Tag).KeyPress.KeySequence.Clear();
                        textBox.Text = "";
                        UpdateKeyBindingProfileSimpleKeyStrokes(textBox);
                    }
                    else if (((TagDataClassStreamDeck)textBox.Tag).ContainsSingleKey())
                    {
                        ((TagDataClassStreamDeck)textBox.Tag).KeyPress.KeySequence.Clear();
                        textBox.Text = "";
                        UpdateKeyBindingProfileSimpleKeyStrokes(textBox);
                    }
                    if (((TagDataClassStreamDeck)textBox.Tag).ContainsBIPLink())
                    {
                        //Check if this textbox contains sequence information. If so then prompt the user for deletion
                        if (MessageBox.Show("Do you want to delete BIP Links?", "Delete BIP Link?", MessageBoxButton.OKCancel, MessageBoxImage.Question) != MessageBoxResult.OK)
                        {
                            return;
                        }
                        ((TagDataClassStreamDeck)textBox.Tag).BIPLink.BIPLights.Clear();
                        textBox.Background = Brushes.White;
                        UpdateBIPLinkBindings(textBox);
                    }
                }
                TextBoxLogStreamDeck35.Focus();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(3001, ex);
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
                Common.ShowErrorMessageBox(993004, ex);
            }
        }


        private void TextBoxPreviewKeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                var textBox = ((TextBox)sender);

                //Check if this textbox contains sequence or DCS-BIOS information. If so then exit
                if (((TagDataClassStreamDeck)textBox.Tag).ContainsKeySequence() || ((TagDataClassStreamDeck)textBox.Tag).ContainsDCSBIOS())
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
                ((TagDataClassStreamDeck)textBox.Tag).KeyPress = new OSKeyPress(result);
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
                if (((TagDataClassStreamDeck)textBox.Tag).ContainsKeySequence())
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
                Common.ShowErrorMessageBox(993007, ex);
            }
        }

        private void MouseDownFocusLogTextBox(object sender, MouseButtonEventArgs e)
        {
            try
            {
                TextBoxLogStreamDeck35.Focus();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(992014, ex);
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

                if (!((TagDataClassStreamDeck)textBox.Tag).ContainsSingleKey())
                {
                    return;
                }
                var keyPressLength = ((TagDataClassStreamDeck)textBox.Tag).KeyPress.GetLengthOfKeyPress();
                //CheckContextMenuItems(keyPressLength, contextMenu);
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(992061, ex);
            }
        }


        private void TextBoxContextMenuClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var textBox = GetTextBoxInFocus();
                //SetKeyPressLength(textBox, (MenuItem)sender);

                UpdateKeyBindingProfileSimpleKeyStrokes(textBox);
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(992082, ex);
            }
        }

        private TextBox GetTextBoxInFocus()
        {
            foreach (var textBox in Common.FindVisualChildren<TextBox>(this))
            {
                if (!textBox.Equals(TextBoxLogStreamDeck35) && textBox.IsFocused && Equals(textBox.Background, Brushes.Yellow))
                {
                    return textBox;
                }
            }
            return null;
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
                if (((TagDataClassStreamDeck)textBox.Tag).ContainsKeySequence())
                {
                    keySequenceWindow = new KeySequenceWindow(textBox.Text, ((TagDataClassStreamDeck)textBox.Tag).GetKeySequence());
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
                        var osKeyPress = new OSKeyPress("Key press sequence", sequenceList);
                        ((TagDataClassStreamDeck)textBox.Tag).KeyPress = osKeyPress;
                        ((TagDataClassStreamDeck)textBox.Tag).KeyPress.Information = keySequenceWindow.GetInformation;
                        if (!string.IsNullOrEmpty(keySequenceWindow.GetInformation))
                        {
                            textBox.Text = keySequenceWindow.GetInformation;
                        }
                        //textBox.Text = string.IsNullOrEmpty(keySequenceWindow.GetInformation) ? "Key press sequence" : keySequenceWindow.GetInformation;
                        /*if (!string.IsNullOrEmpty(keySequenceWindow.GetInformation))
                        {
                            var toolTip = new ToolTip { Content = keySequenceWindow.GetInformation };
                            textBox.ToolTipa = toolTip;
                        }*/
                        UpdateKeyBindingProfileSequencedKeyStrokesPZ70(textBox);
                    }
                    else
                    {
                        //If only one press was created treat it as a simple keypress
                        ((TagDataClassStreamDeck)textBox.Tag).ClearAll();
                        var osKeyPress = new OSKeyPress(sequenceList[0].VirtualKeyCodesAsString, sequenceList[0].LengthOfKeyPress);
                        ((TagDataClassStreamDeck)textBox.Tag).KeyPress = osKeyPress;
                        ((TagDataClassStreamDeck)textBox.Tag).KeyPress.Information = keySequenceWindow.GetInformation;
                        textBox.Text = sequenceList[0].VirtualKeyCodesAsString;
                        /*textBox.Text = sequenceList.Values[0].VirtualKeyCodesAsString;
                        if (!string.IsNullOrEmpty(keySequenceWindow.GetInformation))
                        {
                            var toolTip = new ToolTip { Content = keySequenceWindow.GetInformation };
                            textBox.ToolTipa = toolTip;
                        }*/
                        UpdateKeyBindingProfileSimpleKeyStrokes(textBox);
                    }
                }
                TextBoxLogStreamDeck35.Focus();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(2044, ex);
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
                if (((TagDataClassStreamDeck)textBox.Tag).ContainsDCSBIOS())
                {
                    dcsBIOSControlsConfigsWindow = new DCSBIOSControlsConfigsWindow(_globalHandler.GetAirframe(), textBox.Name.Replace("TextBox", ""), ((TagDataClassStreamDeck)textBox.Tag).DCSBIOSBinding.DCSBIOSInputs, textBox.Text);
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
                    ((TagDataClassStreamDeck)textBox.Tag).Consume(dcsBiosInputs);
                    UpdateDCSBIOSBinding(textBox);
                }
                TextBoxLogStreamDeck35.Focus();
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
                if (((TagDataClassStreamDeck)textBox.Tag).ContainsBIPLink())
                {
                    var bipLink = ((TagDataClassStreamDeck)textBox.Tag).BIPLink;
                    bipLinkWindow = new BIPLinkWindow(bipLink);
                }
                else
                {
                    var bipLink = new BIPLinkStreamDeck();
                    bipLinkWindow = new BIPLinkWindow(bipLink);
                }
                bipLinkWindow.ShowDialog();
                if (bipLinkWindow.DialogResult.HasValue && bipLinkWindow.DialogResult == true && bipLinkWindow.IsDirty && bipLinkWindow.BIPLink != null)
                {
                    ((TagDataClassStreamDeck)textBox.Tag).BIPLink = (BIPLinkStreamDeck)bipLinkWindow.BIPLink;
                    UpdateBIPLinkBindings(textBox);
                }
                TextBoxLogStreamDeck35.Focus();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(442044, ex);
            }
        }


        private void UpdateBIPLinkBindings(TextBox textBox)
        {
            try
            {
                var key = GetStreamDeckKey(textBox);
                _streamDeck35.AddOrUpdateBIPLinkKnobBinding(key.StreamDeckButton, ((TagDataClassStreamDeck)textBox.Tag).BIPLink, key.ButtonState);
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(3011, ex);
            }
        }

        private void UpdateKeyBindingProfileSequencedKeyStrokesPZ70(TextBox textBox)
        {
            try
            {
                var key = GetStreamDeckKey(textBox);
                _streamDeck35.AddOrUpdateSequencedKeyBinding(textBox.Text, key.StreamDeckButton, ((TagDataClassStreamDeck)textBox.Tag).GetKeySequence(), key.ButtonState);
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
                if (!((TagDataClassStreamDeck)textBox.Tag).ContainsOSKeyPress() || ((TagDataClassStreamDeck)textBox.Tag).KeyPress.KeySequence.Count == 0)
                {
                    keyPressLength = KeyPressLength.FiftyMilliSec;
                }
                else
                {
                    keyPressLength = ((TagDataClassStreamDeck)textBox.Tag).KeyPress.GetLengthOfKeyPress();
                }
                var key = GetStreamDeckKey(textBox);
                _streamDeck35.AddOrUpdateSingleKeyBinding(key.StreamDeckButton, textBox.Text, keyPressLength, key.ButtonState);
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
                var tag = (TagDataClassStreamDeck)textBox.Tag;
                _streamDeck35.AddOrUpdateOSCommandBinding(tag.Key.StreamDeckButton, tag.OSCommandObject, tag.Key.ButtonState);
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
                var key = GetStreamDeckKey(textBox);
                _streamDeck35.AddOrUpdateDCSBIOSBinding(key.StreamDeckButton, ((TagDataClassStreamDeck)textBox.Tag).DCSBIOSBinding.DCSBIOSInputs, textBox.Text, key.ButtonState);
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(345012, ex);
            }
        }


        private void RemoveContextMenuClickHandlers()
        {
            foreach (var textBox in Common.FindVisualChildren<TextBox>(this))
            {
                if (!Equals(textBox, TextBoxLogStreamDeck35))
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
                if (!Equals(textBox, TextBoxLogStreamDeck35))
                {
                    var contextMenu = (ContextMenu)Resources["TextBoxContextMenuStreamDeck35"];

                    textBox.ContextMenu = contextMenu;
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

                if (contextMenu != null)
                {
                    foreach (MenuItem item in contextMenu.Items)
                    {
                        item.Visibility = Visibility.Collapsed;
                    }

                    if (((TagDataClassStreamDeck) textBox.Tag).ContainsDCSBIOS())
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
                    else if (((TagDataClassStreamDeck) textBox.Tag).ContainsKeySequence())
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
                    else if (((TagDataClassStreamDeck) textBox.Tag).IsEmpty())
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
                            else if (item.Name.Contains("EditOSCommand"))
                            {
                                item.Visibility = Visibility.Visible;
                            }
                            else
                            {
                                item.Visibility = Visibility.Collapsed;
                            }
                        }
                    }
                    else if (((TagDataClassStreamDeck) textBox.Tag).ContainsSingleKey())
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
                    else if (((TagDataClassStreamDeck) textBox.Tag).ContainsBIPLink())
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
                    else if (((TagDataClassStreamDeck) textBox.Tag).ContainsOSCommand())
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
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(2081, ex);
            }
        }

        private void NotifyKnobChanges(HashSet<object> knobs)
        {
            try
            {
                //Set focus to this so that virtual keypresses won't affect settings
                Dispatcher?.BeginInvoke((Action)(() => TextBoxLogStreamDeck35.Focus()));
                foreach (var knob in knobs)
                {
                    var multiPanelKnob = (MultiPanelKnob)knob;

                    /*if (_streamDeck35.ForwardPanelEvent)
                    {
                        if (!string.IsNullOrEmpty(_streamDeck35.GetKeyPressForLoggingPurposes(multiPanelKnob)))
                        {
                            Dispatcher?.BeginInvoke(
                                (Action)
                                (() =>
                                 TextBoxLogStreamDeck35.Text = TextBoxLogStreamDeck35.Text.Insert(0, _streamDeck35.GetKeyPressForLoggingPurposes(multiPanelKnob) + "\n")));
                        }
                    }
                    else
                    {
                        Dispatcher?.BeginInvoke(
                            (Action)
                            (() =>
                             TextBoxLogStreamDeck35.Text = TextBoxLogStreamDeck35.Text.Insert(0, "No action taken, panel events Disabled.\n")));
                    }*/
                }
                SetGraphicsState(knobs);
            }
            catch (Exception ex)
            {
                Dispatcher?.BeginInvoke(
                    (Action)
                    (() =>
                     TextBoxLogStreamDeck35.Text = TextBoxLogStreamDeck35.Text.Insert(0, "0x16" + ex.Message + ".\n")));
                Common.ShowErrorMessageBox(3009, ex);
            }
        }

        private void ButtonLcdMenuItemDeleteBinding_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var menuItem = (MenuItem)sender;
                var button = (Button)((ContextMenu)(menuItem.Parent)).Tag;
                if (button.Name.Contains("Upper"))
                {
                    //DeleteLCDBinding(PZ70LCDPosition.UpperLCD, button);
                }
                else
                {
                    //DeleteLCDBinding(PZ70LCDPosition.LowerLCD, button);
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(7365005, ex);
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


        private StreamDeckKeyOnOff GetStreamDeckKey(TextBox textBox)
        {
            try
            {
                if (textBox.Equals(TextBoxButton11On))
                {
                    return new StreamDeckKeyOnOff(StreamDeck35Buttons.BUTTON11, true);
                }
                if (textBox.Equals(TextBoxButton11Off))
                {
                    return new StreamDeckKeyOnOff(StreamDeck35Buttons.BUTTON11, false);
                }
                if (textBox.Equals(TextBoxButton12On))
                {
                    return new StreamDeckKeyOnOff(StreamDeck35Buttons.BUTTON12, true);
                }
                if (textBox.Equals(TextBoxButton12Off))
                {
                    return new StreamDeckKeyOnOff(StreamDeck35Buttons.BUTTON12, false);
                }
                if (textBox.Equals(TextBoxButton13On))
                {
                    return new StreamDeckKeyOnOff(StreamDeck35Buttons.BUTTON13, true);
                }
                if (textBox.Equals(TextBoxButton13Off))
                {
                    return new StreamDeckKeyOnOff(StreamDeck35Buttons.BUTTON13, false);
                }
                if (textBox.Equals(TextBoxButton14On))
                {
                    return new StreamDeckKeyOnOff(StreamDeck35Buttons.BUTTON14, true);
                }
                if (textBox.Equals(TextBoxButton14Off))
                {
                    return new StreamDeckKeyOnOff(StreamDeck35Buttons.BUTTON14, false);
                }
                if (textBox.Equals(TextBoxButton15On))
                {
                    return new StreamDeckKeyOnOff(StreamDeck35Buttons.BUTTON15, true);
                }
                if (textBox.Equals(TextBoxButton15Off))
                {
                    return new StreamDeckKeyOnOff(StreamDeck35Buttons.BUTTON15, false);
                }
                if (textBox.Equals(TextBoxButton21On))
                {
                    return new StreamDeckKeyOnOff(StreamDeck35Buttons.BUTTON21, true);
                }
                if (textBox.Equals(TextBoxButton21Off))
                {
                    return new StreamDeckKeyOnOff(StreamDeck35Buttons.BUTTON21, false);
                }
                if (textBox.Equals(TextBoxButton22On))
                {
                    return new StreamDeckKeyOnOff(StreamDeck35Buttons.BUTTON22, true);
                }
                if (textBox.Equals(TextBoxButton22Off))
                {
                    return new StreamDeckKeyOnOff(StreamDeck35Buttons.BUTTON22, false);
                }
                if (textBox.Equals(TextBoxButton23On))
                {
                    return new StreamDeckKeyOnOff(StreamDeck35Buttons.BUTTON23, true);
                }
                if (textBox.Equals(TextBoxButton23Off))
                {
                    return new StreamDeckKeyOnOff(StreamDeck35Buttons.BUTTON23, false);
                }
                if (textBox.Equals(TextBoxButton24On))
                {
                    return new StreamDeckKeyOnOff(StreamDeck35Buttons.BUTTON24, true);
                }
                if (textBox.Equals(TextBoxButton24Off))
                {
                    return new StreamDeckKeyOnOff(StreamDeck35Buttons.BUTTON24, false);
                }
                if (textBox.Equals(TextBoxButton25On))
                {
                    return new StreamDeckKeyOnOff(StreamDeck35Buttons.BUTTON25, true);
                }
                if (textBox.Equals(TextBoxButton25Off))
                {
                    return new StreamDeckKeyOnOff(StreamDeck35Buttons.BUTTON25, false);
                }
                if (textBox.Equals(TextBoxButton31On))
                {
                    return new StreamDeckKeyOnOff(StreamDeck35Buttons.BUTTON31, true);
                }
                if (textBox.Equals(TextBoxButton31Off))
                {
                    return new StreamDeckKeyOnOff(StreamDeck35Buttons.BUTTON31, false);
                }
                if (textBox.Equals(TextBoxButton32On))
                {
                    return new StreamDeckKeyOnOff(StreamDeck35Buttons.BUTTON32, true);
                }
                if (textBox.Equals(TextBoxButton32Off))
                {
                    return new StreamDeckKeyOnOff(StreamDeck35Buttons.BUTTON32, false);
                }
                if (textBox.Equals(TextBoxButton33On))
                {
                    return new StreamDeckKeyOnOff(StreamDeck35Buttons.BUTTON33, true);
                }
                if (textBox.Equals(TextBoxButton33Off))
                {
                    return new StreamDeckKeyOnOff(StreamDeck35Buttons.BUTTON33, false);
                }
                if (textBox.Equals(TextBoxButton34On))
                {
                    return new StreamDeckKeyOnOff(StreamDeck35Buttons.BUTTON34, true);
                }
                if (textBox.Equals(TextBoxButton34Off))
                {
                    return new StreamDeckKeyOnOff(StreamDeck35Buttons.BUTTON34, false);
                }
                if (textBox.Equals(TextBoxButton35On))
                {
                    return new StreamDeckKeyOnOff(StreamDeck35Buttons.BUTTON35, true);
                }
                if (textBox.Equals(TextBoxButton35Off))
                {
                    return new StreamDeckKeyOnOff(StreamDeck35Buttons.BUTTON35, false);
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(3012, ex);
            }
            throw new Exception("Failed to find Stream Deck key for TextBox " + textBox.Name);
            
        }


        private TextBox GetTextBox(StreamDeck35Buttons knob, bool whenTurnedOn)
        {
            try
            {
                if (knob == StreamDeck35Buttons.BUTTON11 && whenTurnedOn)
                {
                    return TextBoxButton11On;
                }
                if (knob == StreamDeck35Buttons.BUTTON11 && !whenTurnedOn)
                {
                    return TextBoxButton11Off;
                }
                if (knob == StreamDeck35Buttons.BUTTON12 && whenTurnedOn)
                {
                    return TextBoxButton12On;
                }
                if (knob == StreamDeck35Buttons.BUTTON12 && !whenTurnedOn)
                {
                    return TextBoxButton12Off;
                }
                if (knob == StreamDeck35Buttons.BUTTON13 && whenTurnedOn)
                {
                    return TextBoxButton13On;
                }
                if (knob == StreamDeck35Buttons.BUTTON13 && !whenTurnedOn)
                {
                    return TextBoxButton13Off;
                }
                if (knob == StreamDeck35Buttons.BUTTON14 && whenTurnedOn)
                {
                    return TextBoxButton14On;
                }
                if (knob == StreamDeck35Buttons.BUTTON14 && !whenTurnedOn)
                {
                    return TextBoxButton14Off;
                }
                if (knob == StreamDeck35Buttons.BUTTON15 && whenTurnedOn)
                {
                    return TextBoxButton15On;
                }
                if (knob == StreamDeck35Buttons.BUTTON15 && !whenTurnedOn)
                {
                    return TextBoxButton15Off;
                }
                if (knob == StreamDeck35Buttons.BUTTON21 && whenTurnedOn)
                {
                    return TextBoxButton21On;
                }
                if (knob == StreamDeck35Buttons.BUTTON21 && !whenTurnedOn)
                {
                    return TextBoxButton21Off;
                }
                if (knob == StreamDeck35Buttons.BUTTON22 && whenTurnedOn)
                {
                    return TextBoxButton22On;
                }
                if (knob == StreamDeck35Buttons.BUTTON22 && !whenTurnedOn)
                {
                    return TextBoxButton22Off;
                }
                if (knob == StreamDeck35Buttons.BUTTON23 && whenTurnedOn)
                {
                    return TextBoxButton23On;
                }
                if (knob == StreamDeck35Buttons.BUTTON23 && !whenTurnedOn)
                {
                    return TextBoxButton23Off;
                }
                if (knob == StreamDeck35Buttons.BUTTON24 && whenTurnedOn)
                {
                    return TextBoxButton24On;
                }
                if (knob == StreamDeck35Buttons.BUTTON24 && !whenTurnedOn)
                {
                    return TextBoxButton24Off;
                }
                if (knob == StreamDeck35Buttons.BUTTON25 && whenTurnedOn)
                {
                    return TextBoxButton25On;
                }
                if (knob == StreamDeck35Buttons.BUTTON25 && !whenTurnedOn)
                {
                    return TextBoxButton25Off;
                }
                if (knob == StreamDeck35Buttons.BUTTON31 && whenTurnedOn)
                {
                    return TextBoxButton31On;
                }
                if (knob == StreamDeck35Buttons.BUTTON31 && !whenTurnedOn)
                {
                    return TextBoxButton31Off;
                }
                if (knob == StreamDeck35Buttons.BUTTON32 && whenTurnedOn)
                {
                    return TextBoxButton32On;
                }
                if (knob == StreamDeck35Buttons.BUTTON32 && !whenTurnedOn)
                {
                    return TextBoxButton32Off;
                }
                if (knob == StreamDeck35Buttons.BUTTON33 && whenTurnedOn)
                {
                    return TextBoxButton33On;
                }
                if (knob == StreamDeck35Buttons.BUTTON33 && !whenTurnedOn)
                {
                    return TextBoxButton33Off;
                }
                if (knob == StreamDeck35Buttons.BUTTON34 && whenTurnedOn)
                {
                    return TextBoxButton34On;
                }
                if (knob == StreamDeck35Buttons.BUTTON34 && !whenTurnedOn)
                {
                    return TextBoxButton34Off;
                }
                if (knob == StreamDeck35Buttons.BUTTON35 && whenTurnedOn)
                {
                    return TextBoxButton35On;
                }
                if (knob == StreamDeck35Buttons.BUTTON35 && !whenTurnedOn)
                {
                    return TextBoxButton35Off;
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(3012, ex);
            }
            throw new Exception("Failed to find TextBox from Stream Deck key : " + knob);
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
                if (((TagDataClassStreamDeck)textBox.Tag).ContainsOSCommand())
                {
                    osCommandWindow = new OSCommandWindow(((TagDataClassStreamDeck)textBox.Tag).OSCommandObject);
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
                    ((TagDataClassStreamDeck)textBox.Tag).OSCommandObject = osCommand;
                    textBox.Text = osCommand.Name;
                    UpdateOSCommandBindingsPZ55(textBox);
                }
                TextBoxLogStreamDeck35.Focus();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(2044, ex);
            }
        }

    }
}
