using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ClassLibraryCommon;
using NonVisuals;

namespace DCSFlightpanels
{
    /// <summary>
    /// Interaction logic for StreamDeckUserControl.xaml
    /// </summary>
    public partial class StreamDeckUserControl : UserControl, IGamingPanelListener, IProfileHandlerListener, ISaitekUserControl
    {
        private readonly StreamDeckPanel _streamDeck;
        private readonly TabItem _parentTabItem;
        private string _parentTabItemHeader;
        private readonly IGlobalHandler _globalHandler;
        private bool _userControlLoaded;
        private bool _textBoxTagsSet;

        public StreamDeckUserControl(HIDSkeleton hidSkeleton, TabItem parentTabItem, IGlobalHandler globalHandler)
        {
            InitializeComponent();
            _parentTabItem = parentTabItem;
            _parentTabItemHeader = _parentTabItem.Header.ToString();
            _streamDeck = new StreamDeckPanel(hidSkeleton);
            _streamDeck.Attach((IGamingPanelListener)this);
            globalHandler.Attach(_streamDeck);
            _globalHandler = globalHandler;

            HideAllImages();
        }

        private void StreamDeckUserControl_OnLoaded(object sender, RoutedEventArgs e)
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
            return null; //_streamDeck;
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

        private void SetFormState()
        {
            try
            {
                
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(471473, ex);
            }
        }

        private void SetApplicationMode()
        {
        }

        public void SwitchesChanged(object sender, SwitchesChangedEventArgs e)
        {
            try
            {
                if (e.GamingPanelEnum == GamingPanelEnum.StreamDeck && e.UniqueId.Equals(_streamDeck.InstanceId))
                {
                    NotifyButtonChanges(e.Switches);
                }
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
                if (textBox.Equals(TextBoxLogStreamDeck))
                {
                    continue;
                }
                var tagHolderClass = (TagDataClassStreamDeck)textBox.Tag;
                textBox.Text = "";
                tagHolderClass.ClearAll();
            }
            if (clearAlsoProfile)
            {
                _streamDeck.ClearSettings();
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
                if (!textBox.Equals(TextBoxLogStreamDeck))
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
                /*if (e.UniqueId.Equals(_streamDeck.InstanceId) && e.GamingPanelEnum == GamingPanelEnum.PZ70MultiPanel)
                {
                    Dispatcher?.BeginInvoke((Action)(ShowGraphicConfiguration));
                    Dispatcher?.BeginInvoke((Action)(() => TextBoxLogStreamDeck.Text = ""));
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
                /*if (_streamDeck != null)
                {
                    TextBoxLogStreamDeck.Text = "";
                    TextBoxLogStreamDeck.Text = _streamDeck.InstanceId;
                    Clipboard.SetText(_streamDeck.InstanceId);
                    MessageBox.Show("Instance id has been copied to the ClipBoard.");
                }*/
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(2000, ex);
            }
        }

        private void SetGraphicsState(HashSet<object> buttons)
        {
            try
            {
                SetTextBoxesVisibleStatus(string.IsNullOrEmpty(GetStreamDeckLayer()));

                foreach (var streamDeckButton35 in buttons)
                {
                    var streamDeckButton = (StreamDeckButton)streamDeckButton35;
                    switch (streamDeckButton.Button)
                    {
                        case StreamDeckButtons.BUTTON1:
                            {
                                var key = streamDeckButton;
                                Dispatcher?.BeginInvoke(
                                    (Action)delegate
                                   {
                                       Image1.Visibility = key.IsPressed ? Visibility.Visible : Visibility.Collapsed;
                                       if (key.IsPressed)
                                       {
                                           ClearAll(false);
                                           ShowGraphicConfiguration();
                                       }
                                   });
                                break;
                            }
                        case StreamDeckButtons.BUTTON2:
                            {
                                var key = streamDeckButton;
                                Dispatcher?.BeginInvoke(
                                    (Action)delegate
                                    {
                                        DotImage2.Visibility = key.IsPressed ? Visibility.Visible : Visibility.Collapsed;
                                        if (key.IsPressed)
                                        {
                                            ClearAll(false);
                                            ShowGraphicConfiguration();
                                        }
                                    });
                                break;
                            }
                        case StreamDeckButtons.BUTTON3:
                            {
                                var key = streamDeckButton;
                                Dispatcher?.BeginInvoke(
                                    (Action)delegate
                                    {
                                        DotImage3.Visibility = key.IsPressed ? Visibility.Visible : Visibility.Collapsed;
                                        if (key.IsPressed)
                                        {
                                            ClearAll(false);
                                            ShowGraphicConfiguration();
                                        }
                                    });
                                break;
                            }
                        case StreamDeckButtons.BUTTON4:
                            {
                                var key = streamDeckButton;
                                Dispatcher?.BeginInvoke(
                                    (Action)delegate
                                    {
                                        DotImage4.Visibility = key.IsPressed ? Visibility.Visible : Visibility.Collapsed;
                                        if (key.IsPressed)
                                        {
                                            ClearAll(false);
                                            ShowGraphicConfiguration();
                                        }
                                    });
                                break;
                            }
                        case StreamDeckButtons.BUTTON5:
                            {
                                var key = streamDeckButton;
                                Dispatcher?.BeginInvoke(
                                    (Action)delegate
                                    {
                                        DotImage5.Visibility = key.IsPressed ? Visibility.Visible : Visibility.Collapsed;
                                        if (key.IsPressed)
                                        {
                                            ClearAll(false);
                                            ShowGraphicConfiguration();
                                        }
                                    });
                                break;
                            }
                        case StreamDeckButtons.BUTTON6:
                            {
                                var key = streamDeckButton;
                                Dispatcher?.BeginInvoke(
                                    (Action)delegate
                                    {
                                        DotImage6.Visibility = key.IsPressed ? Visibility.Visible : Visibility.Collapsed;
                                        if (key.IsPressed)
                                        {
                                            ClearAll(false);
                                            ShowGraphicConfiguration();
                                        }
                                    });
                                break;
                            }
                        case StreamDeckButtons.BUTTON7:
                            {
                                var key = streamDeckButton;
                                Dispatcher?.BeginInvoke(
                                    (Action)delegate
                                    {
                                        DotImage7.Visibility = key.IsPressed ? Visibility.Visible : Visibility.Collapsed;
                                        if (key.IsPressed)
                                        {
                                            ClearAll(false);
                                            ShowGraphicConfiguration();
                                        }
                                    });
                                break;
                            }
                        case StreamDeckButtons.BUTTON8:
                            {
                                var key = streamDeckButton;
                                Dispatcher?.BeginInvoke(
                                    (Action)delegate
                                    {
                                        DotImage8.Visibility = key.IsPressed ? Visibility.Visible : Visibility.Collapsed;
                                        if (key.IsPressed)
                                        {
                                            ClearAll(false);
                                            ShowGraphicConfiguration();
                                        }
                                    });
                                break;
                            }
                        case StreamDeckButtons.BUTTON9:
                            {
                                var key = streamDeckButton;
                                Dispatcher?.BeginInvoke(
                                    (Action)delegate
                                    {
                                        DotImage9.Visibility = key.IsPressed ? Visibility.Visible : Visibility.Collapsed;
                                        if (key.IsPressed)
                                        {
                                            ClearAll(false);
                                            ShowGraphicConfiguration();
                                        }
                                    });
                                break;
                            }
                        case StreamDeckButtons.BUTTON10:
                            {
                                var key = streamDeckButton;
                                Dispatcher?.BeginInvoke(
                                    (Action)delegate
                                    {
                                        DotImage10.Visibility = key.IsPressed ? Visibility.Visible : Visibility.Collapsed;
                                        if (key.IsPressed)
                                        {
                                            ClearAll(false);
                                            ShowGraphicConfiguration();
                                        }
                                    });
                                break;
                            }
                        case StreamDeckButtons.BUTTON11:
                            {
                                var key = streamDeckButton;
                                Dispatcher?.BeginInvoke(
                                    (Action)delegate
                                    {
                                        DotImage11.Visibility = key.IsPressed ? Visibility.Visible : Visibility.Collapsed;
                                        if (key.IsPressed)
                                        {
                                            ClearAll(false);
                                            ShowGraphicConfiguration();
                                        }
                                    });
                                break;
                            }
                        case StreamDeckButtons.BUTTON12:
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
                        case StreamDeckButtons.BUTTON13:
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
                        case StreamDeckButtons.BUTTON14:
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
                        case StreamDeckButtons.BUTTON15:
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
            DotImage1.Visibility = Visibility.Collapsed;
            DotImage2.Visibility = Visibility.Collapsed;
            DotImage3.Visibility = Visibility.Collapsed;
            DotImage4.Visibility = Visibility.Collapsed;
            DotImage5.Visibility = Visibility.Collapsed;
            DotImage6.Visibility = Visibility.Collapsed;
            DotImage7.Visibility = Visibility.Collapsed;
            DotImage8.Visibility = Visibility.Collapsed;
            DotImage9.Visibility = Visibility.Collapsed;
            DotImage10.Visibility = Visibility.Collapsed;
            DotImage11.Visibility = Visibility.Collapsed;
            DotImage12.Visibility = Visibility.Collapsed;
            DotImage13.Visibility = Visibility.Collapsed;
            DotImage14.Visibility = Visibility.Collapsed;
            DotImage15.Visibility = Visibility.Collapsed;
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

                foreach (var keyBinding in _streamDeck.KeyBindingsHashSet)
                {
                    var textBox = GetTextBox(keyBinding.StreamDeckButton, keyBinding.WhenTurnedOn);
                    if (keyBinding.OSKeyPress != null && keyBinding.Layer == GetStreamDeckLayer())
                    {
                        ((TagDataClassStreamDeck)textBox.Tag).KeyPress = keyBinding.OSKeyPress;
                    }
                }

                foreach (var osCommand in _streamDeck.OSCommandHashSet)
                {
                    var textBox = GetTextBox(osCommand.StreamDeckButton, osCommand.WhenTurnedOn);
                    if (osCommand.OSCommandObject != null)//&& osCommand.Layer == GetStreamDeckLayer()
                    {
                        ((TagDataClassStreamDeck)textBox.Tag).OSCommandObject = osCommand.OSCommandObject;
                    }
                }

                foreach (var dcsBiosBinding in _streamDeck.DCSBiosBindings)
                {
                    var textBox = GetTextBox(dcsBiosBinding.StreamDeckButton, dcsBiosBinding.WhenTurnedOn);
                    if (dcsBiosBinding.Layer == GetStreamDeckLayer())
                    {
                        ((TagDataClassStreamDeck)textBox.Tag).DCSBIOSBinding = dcsBiosBinding;
                    }
                }


                foreach (var bipLink in _streamDeck.BIPLinkHashSet)
                {
                    var textBox = GetTextBox(bipLink.StreamDeckButton, bipLink.WhenTurnedOn);
                    if (bipLink.BIPLights.Count > 0 && bipLink.Layer == GetStreamDeckLayer())
                    {
                        ((TagDataClassStreamDeck)textBox.Tag).BIPLink = bipLink;
                    }
                }

                LoadComboBoxLayers(null);

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
                ((TagDataClassStreamDeck)textBox.Tag).KeyPress = new KeyPress(result);
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
                        _streamDeck.RemoveButtonFromList(GetStreamDeckLayer(), ControlListStreamDeck.DCSBIOS, GetStreamDeckKey(textBox).StreamDeckButton, GetStreamDeckKey(textBox).ButtonState);
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
                TextBoxLogStreamDeck.Focus();
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
                ((TagDataClassStreamDeck)textBox.Tag).KeyPress = new KeyPress(result);
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
                TextBoxLogStreamDeck.Focus();
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
                if (!textBox.Equals(TextBoxLogStreamDeck) && textBox.IsFocused && Equals(textBox.Background, Brushes.Yellow))
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
                        var osKeyPress = new KeyPress("Key press sequence", sequenceList);
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
                        var osKeyPress = new KeyPress(sequenceList[0].VirtualKeyCodesAsString, sequenceList[0].LengthOfKeyPress);
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
                TextBoxLogStreamDeck.Focus();
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
                TextBoxLogStreamDeck.Focus();
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
                TextBoxLogStreamDeck.Focus();
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
                _streamDeck.AddOrUpdateBIPLinkKeyBinding(GetStreamDeckLayer(), key.StreamDeckButton, ((TagDataClassStreamDeck)textBox.Tag).BIPLink, key.ButtonState);
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
                _streamDeck.AddOrUpdateSequencedKeyBinding(GetStreamDeckLayer(), textBox.Text, key.StreamDeckButton, ((TagDataClassStreamDeck)textBox.Tag).GetKeySequence(), key.ButtonState);
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
                _streamDeck.AddOrUpdateSingleKeyBinding(GetStreamDeckLayer(), key.StreamDeckButton, textBox.Text, keyPressLength, key.ButtonState);
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
                _streamDeck.AddOrUpdateOSCommandBinding(GetStreamDeckLayer(), tag.Key.StreamDeckButton, tag.OSCommandObject, tag.Key.ButtonState);
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
                _streamDeck.AddOrUpdateDCSBIOSBinding(GetStreamDeckLayer(), key.StreamDeckButton, ((TagDataClassStreamDeck)textBox.Tag).DCSBIOSBinding.DCSBIOSInputs, textBox.Text, key.ButtonState);
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
                if (!Equals(textBox, TextBoxLogStreamDeck))
                {
                    textBox.ContextMenu = null;
                    textBox.ContextMenuOpening -= TextBoxContextMenuOpening;
                }
            }
        }

        private void SetTextBoxesVisibleStatus(bool show)
        {
            StackPanelButtonColumn0.Visibility = (show ? Visibility.Visible : Visibility.Hidden);
            StackPanelButtonColumn1.Visibility = (show ? Visibility.Visible : Visibility.Hidden);
            StackPanelButtonColumn2.Visibility = (show ? Visibility.Visible : Visibility.Hidden);
            StackPanelButtonColumn3.Visibility = (show ? Visibility.Visible : Visibility.Hidden);
            StackPanelButtonColumn4.Visibility = (show ? Visibility.Visible : Visibility.Hidden);
        }

        private void SetContextMenuClickHandlers()
        {
            foreach (var textBox in Common.FindVisualChildren<TextBox>(this))
            {
                if (!Equals(textBox, TextBoxLogStreamDeck))
                {
                    var contextMenu = (ContextMenu)Resources["TextBoxContextMenuStreamDeck"];

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

                    if (((TagDataClassStreamDeck)textBox.Tag).ContainsDCSBIOS())
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
                    else if (((TagDataClassStreamDeck)textBox.Tag).ContainsKeySequence())
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
                    else if (((TagDataClassStreamDeck)textBox.Tag).IsEmpty())
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
                    else if (((TagDataClassStreamDeck)textBox.Tag).ContainsSingleKey())
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
                    else if (((TagDataClassStreamDeck)textBox.Tag).ContainsBIPLink())
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
                    else if (((TagDataClassStreamDeck)textBox.Tag).ContainsOSCommand())
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

        private void NotifyButtonChanges(HashSet<object> buttons)
        {
            try
            {
                //Set focus to this so that virtual keypresses won't affect settings
                Dispatcher?.BeginInvoke((Action)(() => TextBoxLogStreamDeck.Focus()));
                foreach (var button in buttons)
                {
                    var streamDeckButton = (StreamDeckButton)button;

                    if (_streamDeck.ForwardPanelEvent)
                    {
                        if (!string.IsNullOrEmpty(_streamDeck.GetKeyPressForLoggingPurposes(streamDeckButton)))
                        {
                            Dispatcher?.BeginInvoke(
                                (Action)
                                (() =>
                                 TextBoxLogStreamDeck.Text = TextBoxLogStreamDeck.Text.Insert(0, _streamDeck.GetKeyPressForLoggingPurposes(streamDeckButton) + "\n")));
                        }
                    }
                    else
                    {
                        Dispatcher?.BeginInvoke(
                            (Action)
                            (() =>
                             TextBoxLogStreamDeck.Text = TextBoxLogStreamDeck.Text.Insert(0, "No action taken, panel events Disabled.\n")));
                    }
                }
                SetGraphicsState(buttons);
            }
            catch (Exception ex)
            {
                Dispatcher?.BeginInvoke(
                    (Action)
                    (() =>
                     TextBoxLogStreamDeck.Text = TextBoxLogStreamDeck.Text.Insert(0, "0x16" + ex.Message + ".\n")));
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
                if (textBox.Equals(TextBoxButton1On))
                {
                    return new StreamDeckKeyOnOff(StreamDeckButtons.BUTTON1, true);
                }
                if (textBox.Equals(TextBoxButton1Off))
                {
                    return new StreamDeckKeyOnOff(StreamDeckButtons.BUTTON1, false);
                }
                if (textBox.Equals(TextBoxButton2On))
                {
                    return new StreamDeckKeyOnOff(StreamDeckButtons.BUTTON2, true);
                }
                if (textBox.Equals(TextBoxButton2Off))
                {
                    return new StreamDeckKeyOnOff(StreamDeckButtons.BUTTON2, false);
                }
                if (textBox.Equals(TextBoxButton3On))
                {
                    return new StreamDeckKeyOnOff(StreamDeckButtons.BUTTON3, true);
                }
                if (textBox.Equals(TextBoxButton3Off))
                {
                    return new StreamDeckKeyOnOff(StreamDeckButtons.BUTTON3, false);
                }
                if (textBox.Equals(TextBoxButton4On))
                {
                    return new StreamDeckKeyOnOff(StreamDeckButtons.BUTTON4, true);
                }
                if (textBox.Equals(TextBoxButton4Off))
                {
                    return new StreamDeckKeyOnOff(StreamDeckButtons.BUTTON4, false);
                }
                if (textBox.Equals(TextBoxButton5On))
                {
                    return new StreamDeckKeyOnOff(StreamDeckButtons.BUTTON5, true);
                }
                if (textBox.Equals(TextBoxButton5Off))
                {
                    return new StreamDeckKeyOnOff(StreamDeckButtons.BUTTON5, false);
                }
                if (textBox.Equals(TextBoxButton6On))
                {
                    return new StreamDeckKeyOnOff(StreamDeckButtons.BUTTON6, true);
                }
                if (textBox.Equals(TextBoxButton6Off))
                {
                    return new StreamDeckKeyOnOff(StreamDeckButtons.BUTTON6, false);
                }
                if (textBox.Equals(TextBoxButton7On))
                {
                    return new StreamDeckKeyOnOff(StreamDeckButtons.BUTTON7, true);
                }
                if (textBox.Equals(TextBoxButton7Off))
                {
                    return new StreamDeckKeyOnOff(StreamDeckButtons.BUTTON7, false);
                }
                if (textBox.Equals(TextBoxButton8On))
                {
                    return new StreamDeckKeyOnOff(StreamDeckButtons.BUTTON8, true);
                }
                if (textBox.Equals(TextBoxButton8Off))
                {
                    return new StreamDeckKeyOnOff(StreamDeckButtons.BUTTON8, false);
                }
                if (textBox.Equals(TextBoxButton9On))
                {
                    return new StreamDeckKeyOnOff(StreamDeckButtons.BUTTON9, true);
                }
                if (textBox.Equals(TextBoxButton9Off))
                {
                    return new StreamDeckKeyOnOff(StreamDeckButtons.BUTTON9, false);
                }
                if (textBox.Equals(TextBoxButton10On))
                {
                    return new StreamDeckKeyOnOff(StreamDeckButtons.BUTTON10, true);
                }
                if (textBox.Equals(TextBoxButton10Off))
                {
                    return new StreamDeckKeyOnOff(StreamDeckButtons.BUTTON10, false);
                }
                if (textBox.Equals(TextBoxButton11On))
                {
                    return new StreamDeckKeyOnOff(StreamDeckButtons.BUTTON11, true);
                }
                if (textBox.Equals(TextBoxButton11Off))
                {
                    return new StreamDeckKeyOnOff(StreamDeckButtons.BUTTON11, false);
                }
                if (textBox.Equals(TextBoxButton12On))
                {
                    return new StreamDeckKeyOnOff(StreamDeckButtons.BUTTON12, true);
                }
                if (textBox.Equals(TextBoxButton12Off))
                {
                    return new StreamDeckKeyOnOff(StreamDeckButtons.BUTTON12, false);
                }
                if (textBox.Equals(TextBoxButton13On))
                {
                    return new StreamDeckKeyOnOff(StreamDeckButtons.BUTTON13, true);
                }
                if (textBox.Equals(TextBoxButton13Off))
                {
                    return new StreamDeckKeyOnOff(StreamDeckButtons.BUTTON13, false);
                }
                if (textBox.Equals(TextBoxButton14On))
                {
                    return new StreamDeckKeyOnOff(StreamDeckButtons.BUTTON14, true);
                }
                if (textBox.Equals(TextBoxButton14Off))
                {
                    return new StreamDeckKeyOnOff(StreamDeckButtons.BUTTON14, false);
                }
                if (textBox.Equals(TextBoxButton15On))
                {
                    return new StreamDeckKeyOnOff(StreamDeckButtons.BUTTON15, true);
                }
                if (textBox.Equals(TextBoxButton15Off))
                {
                    return new StreamDeckKeyOnOff(StreamDeckButtons.BUTTON15, false);
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(3012, ex);
            }
            throw new Exception("Failed to find Stream Deck key for TextBox " + textBox.Name);

        }


        private TextBox GetTextBox(StreamDeckButtons knob, bool whenTurnedOn)
        {
            try
            {
                if (knob == StreamDeckButtons.BUTTON1 && whenTurnedOn)
                {
                    return TextBoxButton1On;
                }
                if (knob == StreamDeckButtons.BUTTON1 && !whenTurnedOn)
                {
                    return TextBoxButton1Off;
                }
                if (knob == StreamDeckButtons.BUTTON2 && whenTurnedOn)
                {
                    return TextBoxButton2On;
                }
                if (knob == StreamDeckButtons.BUTTON2 && !whenTurnedOn)
                {
                    return TextBoxButton2Off;
                }
                if (knob == StreamDeckButtons.BUTTON3 && whenTurnedOn)
                {
                    return TextBoxButton3On;
                }
                if (knob == StreamDeckButtons.BUTTON3 && !whenTurnedOn)
                {
                    return TextBoxButton3Off;
                }
                if (knob == StreamDeckButtons.BUTTON4 && whenTurnedOn)
                {
                    return TextBoxButton4On;
                }
                if (knob == StreamDeckButtons.BUTTON4 && !whenTurnedOn)
                {
                    return TextBoxButton4Off;
                }
                if (knob == StreamDeckButtons.BUTTON5 && whenTurnedOn)
                {
                    return TextBoxButton5On;
                }
                if (knob == StreamDeckButtons.BUTTON5 && !whenTurnedOn)
                {
                    return TextBoxButton5Off;
                }
                if (knob == StreamDeckButtons.BUTTON6 && whenTurnedOn)
                {
                    return TextBoxButton6On;
                }
                if (knob == StreamDeckButtons.BUTTON6 && !whenTurnedOn)
                {
                    return TextBoxButton6Off;
                }
                if (knob == StreamDeckButtons.BUTTON7 && whenTurnedOn)
                {
                    return TextBoxButton7On;
                }
                if (knob == StreamDeckButtons.BUTTON7 && !whenTurnedOn)
                {
                    return TextBoxButton7Off;
                }
                if (knob == StreamDeckButtons.BUTTON8 && whenTurnedOn)
                {
                    return TextBoxButton8On;
                }
                if (knob == StreamDeckButtons.BUTTON8 && !whenTurnedOn)
                {
                    return TextBoxButton8Off;
                }
                if (knob == StreamDeckButtons.BUTTON9 && whenTurnedOn)
                {
                    return TextBoxButton9On;
                }
                if (knob == StreamDeckButtons.BUTTON9 && !whenTurnedOn)
                {
                    return TextBoxButton9Off;
                }
                if (knob == StreamDeckButtons.BUTTON10 && whenTurnedOn)
                {
                    return TextBoxButton10On;
                }
                if (knob == StreamDeckButtons.BUTTON10 && !whenTurnedOn)
                {
                    return TextBoxButton10Off;
                }
                if (knob == StreamDeckButtons.BUTTON11 && whenTurnedOn)
                {
                    return TextBoxButton11On;
                }
                if (knob == StreamDeckButtons.BUTTON11 && !whenTurnedOn)
                {
                    return TextBoxButton11Off;
                }
                if (knob == StreamDeckButtons.BUTTON12 && whenTurnedOn)
                {
                    return TextBoxButton12On;
                }
                if (knob == StreamDeckButtons.BUTTON12 && !whenTurnedOn)
                {
                    return TextBoxButton12Off;
                }
                if (knob == StreamDeckButtons.BUTTON13 && whenTurnedOn)
                {
                    return TextBoxButton13On;
                }
                if (knob == StreamDeckButtons.BUTTON13 && !whenTurnedOn)
                {
                    return TextBoxButton13Off;
                }
                if (knob == StreamDeckButtons.BUTTON14 && whenTurnedOn)
                {
                    return TextBoxButton14On;
                }
                if (knob == StreamDeckButtons.BUTTON14 && !whenTurnedOn)
                {
                    return TextBoxButton14Off;
                }
                if (knob == StreamDeckButtons.BUTTON15 && whenTurnedOn)
                {
                    return TextBoxButton15On;
                }
                if (knob == StreamDeckButtons.BUTTON15 && !whenTurnedOn)
                {
                    return TextBoxButton15Off;
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
                TextBoxLogStreamDeck.Focus();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(2044, ex);
            }
        }

        private void ButtonNewLayer_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var layerWindow = new StreamDeckLayerWindow(_streamDeck.LayerList);
                layerWindow.ShowDialog();
                if (layerWindow.DialogResult == true)
                {
                    _streamDeck.AddLayer(layerWindow.LayerName);
                }
                LoadComboBoxLayers(layerWindow.LayerName);
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(20235442, ex);
            }
        }

        private void ButtonDeleteLayer_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (MessageBox.Show("Delete layer " + GetStreamDeckLayer() + "?", "Can not be undone!", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    _streamDeck.DeleteLayer(GetStreamDeckLayer());
                }
                LoadComboBoxLayers(null);
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(20235443, ex);
            }
        }

        private string GetStreamDeckLayer()
        {
            try
            {
                return ComboBoxLayers.SelectionBoxItem.ToString();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(20235444, ex);
            }

            return null;
        }

        private void ComboBoxLayers_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                ClearAll(false);
                SetTextBoxesVisibleStatus(!string.IsNullOrEmpty(GetStreamDeckLayer()));

                CheckBoxMarkHomeLayer.Checked -= CheckBoxMarkHomeLayer_OnChecked;
                CheckBoxMarkHomeLayer.IsChecked = GetStreamDeckLayer() == _streamDeck.HomeLayer;
                CheckBoxMarkHomeLayer.Checked += CheckBoxMarkHomeLayer_OnChecked;

                SetFormState();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(20135444, ex);
            }
        }

        private void LoadComboBoxLayers(string selectedLayer)
        {
            var selectedValue = (string)ComboBoxLayers.SelectedValue;
            ComboBoxLayers.SelectionChanged -= ComboBoxLayers_OnSelectionChanged;
            ComboBoxLayers.ItemsSource = _streamDeck.LayerList;
            if (!string.IsNullOrEmpty(selectedLayer))
            {
                ComboBoxLayers.SelectedValue = selectedLayer;
            }
            else if (!string.IsNullOrEmpty(selectedValue))
            {
                ComboBoxLayers.SelectedValue = selectedLayer;
            }
            ComboBoxLayers.SelectionChanged += ComboBoxLayers_OnSelectionChanged;
        }

        private void CheckBoxMarkHomeLayer_OnChecked(object sender, RoutedEventArgs e)
        {
            try
            {
                _streamDeck.HomeLayer = GetStreamDeckLayer();
                SetFormState();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(20135444, ex);
            }
        }
    }
}
