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
            //_streamDeck35.Attach((IGamingPanelListener)this);
            //globalHandler.Attach(_streamDeck35);
            _globalHandler = globalHandler;

            HideAllImages();
        }

        private void StreamDeck35UserControl_OnLoaded(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void MultiPanelUserControl_OnLoaded(object sender, RoutedEventArgs e)
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
                var tagHolderClass = (TagDataClassPZ70)textBox.Tag;
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
                    textBox.Tag = new TagDataClassPZ70(textBox, GetPZ70Knob(textBox));
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
                if (((TagDataClassPZ70)textBox.Tag).ContainsBIPLink())
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
                    var textBox = GetTextBox(keyBinding.MultiPanelPZ70Knob, keyBinding.WhenTurnedOn);
                    if (keyBinding.DialPosition == _streamDeck35.PZ70_DialPosition)
                    {
                        if (keyBinding.OSKeyPress != null)
                        {
                            ((TagDataClassPZ70)textBox.Tag).KeyPress = keyBinding.OSKeyPress;
                        }
                    }
                }

                foreach (var osCommand in _streamDeck35.OSCommandHashSet)
                {
                    var textBox = GetTextBox(osCommand.MultiPanelPZ70Knob, osCommand.WhenTurnedOn);
                    if (osCommand.DialPosition == _streamDeck35.PZ70_DialPosition)
                        if (osCommand.OSCommandObject != null)
                        {
                            ((TagDataClassPZ70)textBox.Tag).OSCommandObject = osCommand.OSCommandObject;
                        }
                }

                foreach (var dcsBiosBinding in _streamDeck35.DCSBiosBindings)
                {
                    var textBox = GetTextBox(dcsBiosBinding.MultiPanelPZ70Knob, dcsBiosBinding.WhenTurnedOn);
                    if (dcsBiosBinding.DialPosition == _streamDeck35.PZ70_DialPosition && dcsBiosBinding.DCSBIOSInputs.Count > 0)
                    {
                        ((TagDataClassPZ70)textBox.Tag).DCSBIOSBinding = dcsBiosBinding;
                    }
                }


                foreach (var bipLink in _streamDeck35.BIPLinkHashSet)
                {
                    var textBox = GetTextBox(bipLink.MultiPanelPZ70Knob, bipLink.WhenTurnedOn);
                    if (bipLink.DialPosition == _streamDeck35.PZ70_DialPosition && bipLink.BIPLights.Count > 0)
                    {
                        ((TagDataClassPZ70)textBox.Tag).BIPLink = bipLink;
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
                if (((TagDataClassPZ70)textBox.Tag).ContainsKeySequence() || ((TagDataClassPZ70)textBox.Tag).ContainsDCSBIOS())
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
                ((TagDataClassPZ70)textBox.Tag).KeyPress = new OSKeyPress(result);
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
                    if (((TagDataClassPZ70)textBox.Tag).ContainsDCSBIOS())
                    {
                        if (MessageBox.Show("Do you want to delete the DCS-BIOS configuration?", "Delete DCS-BIOS control?", MessageBoxButton.OKCancel, MessageBoxImage.Question) != MessageBoxResult.OK)
                        {
                            return;
                        }
                        textBox.Text = "";
                        _streamDeck35.RemoveMultiPanelKnobFromList(ControlListPZ70.DCSBIOS, GetPZ70Knob(textBox).MultiPanelPZ70Knob, GetPZ70Knob(textBox).ButtonState);
                        ((TagDataClassPZ70)textBox.Tag).DCSBIOSBinding = null;
                    }
                    else if (((TagDataClassPZ70)textBox.Tag).ContainsKeySequence())
                    {
                        //Check if this textbox contains sequence information. If so then prompt the user for deletion
                        if (MessageBox.Show("Do you want to delete the key sequence?", "Delete key sequence?", MessageBoxButton.OKCancel, MessageBoxImage.Question) != MessageBoxResult.OK)
                        {
                            return;
                        }
                        ((TagDataClassPZ70)textBox.Tag).KeyPress.KeySequence.Clear();
                        textBox.Text = "";
                        UpdateKeyBindingProfileSimpleKeyStrokes(textBox);
                    }
                    else if (((TagDataClassPZ70)textBox.Tag).ContainsSingleKey())
                    {
                        ((TagDataClassPZ70)textBox.Tag).KeyPress.KeySequence.Clear();
                        textBox.Text = "";
                        UpdateKeyBindingProfileSimpleKeyStrokes(textBox);
                    }
                    if (((TagDataClassPZ70)textBox.Tag).ContainsBIPLink())
                    {
                        //Check if this textbox contains sequence information. If so then prompt the user for deletion
                        if (MessageBox.Show("Do you want to delete BIP Links?", "Delete BIP Link?", MessageBoxButton.OKCancel, MessageBoxImage.Question) != MessageBoxResult.OK)
                        {
                            return;
                        }
                        ((TagDataClassPZ70)textBox.Tag).BIPLink.BIPLights.Clear();
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
                if (((TagDataClassPZ70)textBox.Tag).ContainsKeySequence() || ((TagDataClassPZ70)textBox.Tag).ContainsDCSBIOS())
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
                ((TagDataClassPZ70)textBox.Tag).KeyPress = new OSKeyPress(result);
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
                if (((TagDataClassPZ70)textBox.Tag).ContainsKeySequence())
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

                if (!((TagDataClassPZ70)textBox.Tag).ContainsSingleKey())
                {
                    return;
                }
                var keyPressLength = ((TagDataClassPZ70)textBox.Tag).KeyPress.GetLengthOfKeyPress();
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
                if (((TagDataClassPZ70)textBox.Tag).ContainsKeySequence())
                {
                    keySequenceWindow = new KeySequenceWindow(textBox.Text, ((TagDataClassPZ70)textBox.Tag).GetKeySequence());
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
                        ((TagDataClassPZ70)textBox.Tag).KeyPress = osKeyPress;
                        ((TagDataClassPZ70)textBox.Tag).KeyPress.Information = keySequenceWindow.GetInformation;
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
                        ((TagDataClassPZ70)textBox.Tag).ClearAll();
                        var osKeyPress = new OSKeyPress(sequenceList[0].VirtualKeyCodesAsString, sequenceList[0].LengthOfKeyPress);
                        ((TagDataClassPZ70)textBox.Tag).KeyPress = osKeyPress;
                        ((TagDataClassPZ70)textBox.Tag).KeyPress.Information = keySequenceWindow.GetInformation;
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
                if (((TagDataClassPZ70)textBox.Tag).ContainsDCSBIOS())
                {
                    dcsBIOSControlsConfigsWindow = new DCSBIOSControlsConfigsWindow(_globalHandler.GetAirframe(), textBox.Name.Replace("TextBox", ""), ((TagDataClassPZ70)textBox.Tag).DCSBIOSBinding.DCSBIOSInputs, textBox.Text);
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
                    ((TagDataClassPZ70)textBox.Tag).Consume(dcsBiosInputs);
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
                if (((TagDataClassPZ70)textBox.Tag).ContainsBIPLink())
                {
                    var bipLink = ((TagDataClassPZ70)textBox.Tag).BIPLink;
                    bipLinkWindow = new BIPLinkWindow(bipLink);
                }
                else
                {
                    var bipLink = new BIPLinkPZ70();
                    bipLinkWindow = new BIPLinkWindow(bipLink);
                }
                bipLinkWindow.ShowDialog();
                if (bipLinkWindow.DialogResult.HasValue && bipLinkWindow.DialogResult == true && bipLinkWindow.IsDirty && bipLinkWindow.BIPLink != null)
                {
                    ((TagDataClassPZ70)textBox.Tag).BIPLink = (BIPLinkPZ70)bipLinkWindow.BIPLink;
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
                var key = GetPZ70Knob(textBox);
                _streamDeck35.AddOrUpdateBIPLinkKnobBinding(key.MultiPanelPZ70Knob, ((TagDataClassPZ70)textBox.Tag).BIPLink, key.ButtonState);
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
                var key = GetPZ70Knob(textBox);
                _streamDeck35.AddOrUpdateSequencedKeyBinding(textBox.Text, key.MultiPanelPZ70Knob, ((TagDataClassPZ70)textBox.Tag).GetKeySequence(), key.ButtonState);
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
                if (!((TagDataClassPZ70)textBox.Tag).ContainsOSKeyPress() || ((TagDataClassPZ70)textBox.Tag).KeyPress.KeySequence.Count == 0)
                {
                    keyPressLength = KeyPressLength.FiftyMilliSec;
                }
                else
                {
                    keyPressLength = ((TagDataClassPZ70)textBox.Tag).KeyPress.GetLengthOfKeyPress();
                }
                var key = GetPZ70Knob(textBox);
                _streamDeck35.AddOrUpdateSingleKeyBinding(key.MultiPanelPZ70Knob, textBox.Text, keyPressLength, key.ButtonState);
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
                var tag = (TagDataClassPZ70)textBox.Tag;
                _streamDeck35.AddOrUpdateOSCommandBinding(tag.Key.MultiPanelPZ70Knob, tag.OSCommandObject, tag.Key.ButtonState);
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
                var key = GetPZ70Knob(textBox);
                _streamDeck35.AddOrUpdateDCSBIOSBinding(key.MultiPanelPZ70Knob, ((TagDataClassPZ70)textBox.Tag).DCSBIOSBinding.DCSBIOSInputs, textBox.Text, key.ButtonState);
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

                foreach (MenuItem item in contextMenu.Items)
                {
                    item.Visibility = Visibility.Collapsed;
                }

                if (((TagDataClassPZ70)textBox.Tag).ContainsDCSBIOS())
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
                else if (((TagDataClassPZ70)textBox.Tag).ContainsKeySequence())
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
                else if (((TagDataClassPZ70)textBox.Tag).IsEmpty())
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
                else if (((TagDataClassPZ70)textBox.Tag).ContainsSingleKey())
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
                else if (((TagDataClassPZ70)textBox.Tag).ContainsBIPLink())
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


        private MultiPanelPZ70KnobOnOff GetPZ70Knob(TextBox textBox)
        {
            return null;
            /*
            try
            {
                if (textBox.Equals(TextBoxLcdKnobDecrease))
                {
                    return new MultiPanelPZ70KnobOnOff(MultiPanelPZ70Knobs.LCD_WHEEL_DEC, true);
                }
                if (textBox.Equals(TextBoxLcdKnobIncrease))
                {
                    return new MultiPanelPZ70KnobOnOff(MultiPanelPZ70Knobs.LCD_WHEEL_INC, true);
                }
                if (textBox.Equals(TextBoxAutoThrottleOff))
                {
                    return new MultiPanelPZ70KnobOnOff(MultiPanelPZ70Knobs.AUTO_THROTTLE, false);
                }
                if (textBox.Equals(TextBoxAutoThrottleOn))
                {
                    return new MultiPanelPZ70KnobOnOff(MultiPanelPZ70Knobs.AUTO_THROTTLE, true);
                }
                if (textBox.Equals(TextBoxFlapsUp))
                {
                    return new MultiPanelPZ70KnobOnOff(MultiPanelPZ70Knobs.FLAPS_LEVER_UP, true);
                }
                if (textBox.Equals(TextBoxFlapsDown))
                {
                    return new MultiPanelPZ70KnobOnOff(MultiPanelPZ70Knobs.FLAPS_LEVER_DOWN, true);
                }
                if (textBox.Equals(TextBoxPitchTrimUp))
                {
                    return new MultiPanelPZ70KnobOnOff(MultiPanelPZ70Knobs.PITCH_TRIM_WHEEL_UP, true);
                }
                if (textBox.Equals(TextBoxPitchTrimDown))
                {
                    return new MultiPanelPZ70KnobOnOff(MultiPanelPZ70Knobs.PITCH_TRIM_WHEEL_DOWN, true);
                }
                if (textBox.Equals(TextBoxApButtonOn))
                {
                    return new MultiPanelPZ70KnobOnOff(MultiPanelPZ70Knobs.AP_BUTTON, true);
                }
                if (textBox.Equals(TextBoxApButtonOff))
                {
                    return new MultiPanelPZ70KnobOnOff(MultiPanelPZ70Knobs.AP_BUTTON, false);
                }
                if (textBox.Equals(TextBoxHdgButtonOn))
                {
                    return new MultiPanelPZ70KnobOnOff(MultiPanelPZ70Knobs.HDG_BUTTON, true);
                }
                if (textBox.Equals(TextBoxHdgButtonOff))
                {
                    return new MultiPanelPZ70KnobOnOff(MultiPanelPZ70Knobs.HDG_BUTTON, false);
                }
                if (textBox.Equals(TextBoxNavButtonOn))
                {
                    return new MultiPanelPZ70KnobOnOff(MultiPanelPZ70Knobs.NAV_BUTTON, true);
                }
                if (textBox.Equals(TextBoxNavButtonOff))
                {
                    return new MultiPanelPZ70KnobOnOff(MultiPanelPZ70Knobs.NAV_BUTTON, false);
                }
                if (textBox.Equals(TextBoxIasButtonOn))
                {
                    return new MultiPanelPZ70KnobOnOff(MultiPanelPZ70Knobs.IAS_BUTTON, true);
                }
                if (textBox.Equals(TextBoxIasButtonOff))
                {
                    return new MultiPanelPZ70KnobOnOff(MultiPanelPZ70Knobs.IAS_BUTTON, false);
                }
                if (textBox.Equals(TextBoxAltButtonOn))
                {
                    return new MultiPanelPZ70KnobOnOff(MultiPanelPZ70Knobs.ALT_BUTTON, true);
                }
                if (textBox.Equals(TextBoxAltButtonOff))
                {
                    return new MultiPanelPZ70KnobOnOff(MultiPanelPZ70Knobs.ALT_BUTTON, false);
                }
                if (textBox.Equals(TextBoxVsButtonOn))
                {
                    return new MultiPanelPZ70KnobOnOff(MultiPanelPZ70Knobs.VS_BUTTON, true);
                }
                if (textBox.Equals(TextBoxVsButtonOff))
                {
                    return new MultiPanelPZ70KnobOnOff(MultiPanelPZ70Knobs.VS_BUTTON, false);
                }
                if (textBox.Equals(TextBoxAprButtonOn))
                {
                    return new MultiPanelPZ70KnobOnOff(MultiPanelPZ70Knobs.APR_BUTTON, true);
                }
                if (textBox.Equals(TextBoxAprButtonOff))
                {
                    return new MultiPanelPZ70KnobOnOff(MultiPanelPZ70Knobs.APR_BUTTON, false);
                }
                if (textBox.Equals(TextBoxRevButtonOn))
                {
                    return new MultiPanelPZ70KnobOnOff(MultiPanelPZ70Knobs.REV_BUTTON, true);
                }
                if (textBox.Equals(TextBoxRevButtonOff))
                {
                    return new MultiPanelPZ70KnobOnOff(MultiPanelPZ70Knobs.REV_BUTTON, false);
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(3012, ex);
            }
            throw new Exception("Failed to find MultiPanel knob for TextBox " + textBox.Name);
            */
        }


        private TextBox GetTextBox(MultiPanelPZ70Knobs knob, bool whenTurnedOn)
        {
            return null;
            /*
            try
            {
                if (knob == MultiPanelPZ70Knobs.LCD_WHEEL_DEC && whenTurnedOn)
                {
                    return TextBoxLcdKnobDecrease;
                }
                if (knob == MultiPanelPZ70Knobs.LCD_WHEEL_INC && whenTurnedOn)
                {
                    return TextBoxLcdKnobIncrease;
                }
                if (knob == MultiPanelPZ70Knobs.AUTO_THROTTLE && !whenTurnedOn)
                {
                    return TextBoxAutoThrottleOff;
                }
                if (knob == MultiPanelPZ70Knobs.AUTO_THROTTLE && whenTurnedOn)
                {
                    return TextBoxAutoThrottleOn;
                }
                if (knob == MultiPanelPZ70Knobs.FLAPS_LEVER_UP && whenTurnedOn)
                {
                    return TextBoxFlapsUp;
                }
                if (knob == MultiPanelPZ70Knobs.FLAPS_LEVER_DOWN && whenTurnedOn)
                {
                    return TextBoxFlapsDown;
                }
                if (knob == MultiPanelPZ70Knobs.PITCH_TRIM_WHEEL_UP && whenTurnedOn)
                {
                    return TextBoxPitchTrimUp;
                }
                if (knob == MultiPanelPZ70Knobs.PITCH_TRIM_WHEEL_DOWN && whenTurnedOn)
                {
                    return TextBoxPitchTrimDown;
                }
                if (knob == MultiPanelPZ70Knobs.AP_BUTTON && whenTurnedOn)
                {
                    return TextBoxApButtonOn;
                }
                if (knob == MultiPanelPZ70Knobs.AP_BUTTON && !whenTurnedOn)
                {
                    return TextBoxApButtonOff;
                }
                if (knob == MultiPanelPZ70Knobs.HDG_BUTTON && whenTurnedOn)
                {
                    return TextBoxHdgButtonOn;
                }
                if (knob == MultiPanelPZ70Knobs.HDG_BUTTON && !whenTurnedOn)
                {
                    return TextBoxHdgButtonOff;
                }
                if (knob == MultiPanelPZ70Knobs.NAV_BUTTON && whenTurnedOn)
                {
                    return TextBoxNavButtonOn;
                }
                if (knob == MultiPanelPZ70Knobs.NAV_BUTTON && !whenTurnedOn)
                {
                    return TextBoxNavButtonOff;
                }
                if (knob == MultiPanelPZ70Knobs.IAS_BUTTON && whenTurnedOn)
                {
                    return TextBoxIasButtonOn;
                }
                if (knob == MultiPanelPZ70Knobs.IAS_BUTTON && !whenTurnedOn)
                {
                    return TextBoxIasButtonOff;
                }
                if (knob == MultiPanelPZ70Knobs.ALT_BUTTON && whenTurnedOn)
                {
                    return TextBoxAltButtonOn;
                }
                if (knob == MultiPanelPZ70Knobs.ALT_BUTTON && !whenTurnedOn)
                {
                    return TextBoxAltButtonOff;
                }
                if (knob == MultiPanelPZ70Knobs.VS_BUTTON && whenTurnedOn)
                {
                    return TextBoxVsButtonOn;
                }
                if (knob == MultiPanelPZ70Knobs.VS_BUTTON && !whenTurnedOn)
                {
                    return TextBoxVsButtonOff;
                }
                if (knob == MultiPanelPZ70Knobs.APR_BUTTON && whenTurnedOn)
                {
                    return TextBoxAprButtonOn;
                }
                if (knob == MultiPanelPZ70Knobs.APR_BUTTON && !whenTurnedOn)
                {
                    return TextBoxAprButtonOff;
                }
                if (knob == MultiPanelPZ70Knobs.REV_BUTTON && whenTurnedOn)
                {
                    return TextBoxRevButtonOn;
                }
                if (knob == MultiPanelPZ70Knobs.REV_BUTTON && !whenTurnedOn)
                {
                    return TextBoxRevButtonOff;
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(3012, ex);
            }
            throw new Exception("Failed to find TextBox from MultiPanel Knob : " + knob);
            */
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
                if (((TagDataClassPZ70)textBox.Tag).ContainsOSCommand())
                {
                    osCommandWindow = new OSCommandWindow(((TagDataClassPZ70)textBox.Tag).OSCommandObject);
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
                    ((TagDataClassPZ70)textBox.Tag).OSCommandObject = osCommand;
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
