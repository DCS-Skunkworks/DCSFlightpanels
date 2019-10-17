using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ClassLibraryCommon;
using DCSFlightpanels.PanelUserControls;
using DCSFlightpanels.TagDataClasses;
using NonVisuals;
using NonVisuals.DCSBIOSBindings;
using NonVisuals.StreamDeck;

namespace DCSFlightpanels.PanelUserControls
{
    /// <summary>
    /// Interaction logic for UserControlStreamDeckButtonAction.xaml
    /// </summary>
    public partial class UserControlStreamDeckButtonAction : UserControlBase
    {
        private IStreamDeckButtonAction result = null;
        private bool _textBoxTagsSet;
        private KeyPress _keyPress;
        private DCSBIOSActionBindingStreamDeck _dcsbiosActionBinding;
        private OSCommand _osCommand;
        private StreamDeckLayer _streamDeckLayer;
        private IStreamDeckUIParent _streamDeckUIParent;
        private IGlobalHandler _globalHandler;
        private bool _isLoaded = false;
        private bool _isDirty = false;


        public UserControlStreamDeckButtonAction()
        {
            InitializeComponent();
        }

        private void UserControlStreamDeckButtonAction_OnLoaded(object sender, RoutedEventArgs e)
        {
            if (_isLoaded)
            {
                return;
            }
            SetContextMenuClickHandlers();
            SetTextBoxTagObjects();
            _isLoaded = true;
        }

        public void Clear()
        {
            TextBoxDCSBIOSActionButtonOff.Clear();
            TextBoxDCSBIOSActionButtonOn.Clear();
            TextBoxKeyPressButtonOff.Clear();
            TextBoxKeyPressButtonOn.Clear();
            TextBoxOSCommandButtonOff.Clear();
            TextBoxOSCommandButtonOn.Clear();
            ComboBoxLayerNavigationButtonOff.ItemsSource = null;
            ComboBoxLayerNavigationButtonOn.ItemsSource = null;
            ComboBoxLayerNavigationButtonOn.ItemsSource = null;
            ComboBoxLayerNavigationButtonOff.ItemsSource = null;
            result = null;
        }



        public void SetFormState()
        {
            try
            {
                if (!_isLoaded)
                {
                    return;
                }
                StackPanelButtonKeyPressSettings.Visibility = SDUIParent.GetButtonActionType() == EnumStreamDeckButtonActionType.KeyPress ? Visibility.Visible : Visibility.Collapsed;
                StackPanelButtonDCSBIOSSettings.Visibility = SDUIParent.GetButtonActionType() == EnumStreamDeckButtonActionType.DCSBIOS ? Visibility.Visible : Visibility.Collapsed;
                StackPanelButtonOSCommandSettings.Visibility = SDUIParent.GetButtonActionType() == EnumStreamDeckButtonActionType.OSCommand ? Visibility.Visible : Visibility.Collapsed;
                StackPanelButtonLayerNavigationSettings.Visibility = SDUIParent.GetButtonActionType() == EnumStreamDeckButtonActionType.LayerNavigation ? Visibility.Visible : Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(471473, ex);
            }
        }



        private void SetTextBoxTagObjects()
        {
            TextBoxKeyPressButtonOn.Tag = new TagDataClassStreamDeck(TextBoxKeyPressButtonOn, new StreamDeckButtonOnOff(_streamDeckUIParent.GetButton(), true));
            TextBoxKeyPressButtonOff.Tag = new TagDataClassStreamDeck(TextBoxKeyPressButtonOff, new StreamDeckButtonOnOff(_streamDeckUIParent.GetButton(), false));
            _textBoxTagsSet = true;
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
                SetIsDirty();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(3008, ex);
            }
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

        private void TextBoxDCSBIOSInputButtonOff_OnGotFocus(object sender, RoutedEventArgs e)
        {

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
                        ((TagDataClassStreamDeck)textBox.Tag).DCSBIOSBinding = null;
                        SetIsDirty();
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
                        SetIsDirty();
                    }
                    else if (((TagDataClassStreamDeck)textBox.Tag).ContainsSingleKey())
                    {
                        ((TagDataClassStreamDeck)textBox.Tag).KeyPress.KeySequence.Clear();
                        textBox.Text = "";
                        SetIsDirty();
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
                        SetIsDirty();
                    }
                }
                ButtonFocus.Focus();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(3001, ex);
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
                SetIsDirty();
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

        private void RemoveContextMenuClickHandlers()
        {
            foreach (var textBox in Common.FindVisualChildren<TextBox>(this))
            {
                textBox.ContextMenu = null;
                textBox.ContextMenuOpening -= TextBoxContextMenuOpening;
            }
        }

        private void SetContextMenuClickHandlers()
        {
            foreach (var textBox in Common.FindVisualChildren<TextBox>(this))
            {
                var contextMenu = (ContextMenu)Resources["TextBoxContextMenuStreamDeck"];

                textBox.ContextMenu = contextMenu;
                textBox.ContextMenuOpening += TextBoxContextMenuOpening;
            }
        }

        private void TextBoxDCSBIOSInputButtonOff_OnMouseDown(object sender, MouseButtonEventArgs e)
        {

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
                CheckContextMenuItems(keyPressLength, contextMenu);
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
                SetKeyPressLength(textBox, (MenuItem)sender);
                SetIsDirty();
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
                if (textBox.IsFocused && Equals(textBox.Background, Brushes.Yellow))
                {
                    return textBox;
                }
            }
            return null;
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
                    SetIsDirty();
                }
                ButtonFocus.Focus();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(2044, ex);
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
                        SetIsDirty();
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
                        SetIsDirty();
                    }
                }
                ButtonFocus.Focus();
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
                    SetIsDirty();
                }
                ButtonFocus.Focus();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(442044, ex);
            }
        }


        private void MenuContextEditDCSBIOSOpening(object sender, ContextMenuEventArgs e)
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
                    SetIsDirty();
                }
                ButtonFocus.Focus();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(442044, ex);
            }
        }

        private void SetIsDirty()
        {
            _isDirty = true;
        }

        public bool IsDirty
        {
            get => _isDirty;
            set => _isDirty = value;
        }

        public IGlobalHandler GlobalHandler
        {
            get => _globalHandler;
            set => _globalHandler = value;
        }

        public IStreamDeckUIParent SDUIParent
        {
            get => _streamDeckUIParent;
            set => _streamDeckUIParent = value;
        }


        public StreamDeckButtonOnOff GetStreamDeckButtonOnOff(TextBox textBox)
        {
            try
            {
                switch (_streamDeckUIParent.GetButtonActionType())
                {
                    case EnumStreamDeckButtonActionType.KeyPress:
                        {
                            if (textBox.Equals(TextBoxKeyPressButtonOn))
                            {
                                return new StreamDeckButtonOnOff(_streamDeckUIParent.GetButton(), true);
                            }
                            if (textBox.Equals(TextBoxKeyPressButtonOff))
                            {
                                return new StreamDeckButtonOnOff(_streamDeckUIParent.GetButton(), false);
                            }

                            break;
                        }
                    case EnumStreamDeckButtonActionType.DCSBIOS:
                        {
                            if (textBox.Equals(TextBoxDCSBIOSActionButtonOn))
                            {
                                return new StreamDeckButtonOnOff(_streamDeckUIParent.GetButton(), true);
                            }
                            if (textBox.Equals(TextBoxDCSBIOSActionButtonOff))
                            {
                                return new StreamDeckButtonOnOff(_streamDeckUIParent.GetButton(), false);
                            }

                            break;
                        }
                    case EnumStreamDeckButtonActionType.OSCommand:
                        {
                            if (textBox.Equals(TextBoxOSCommandButtonOn))
                            {
                                return new StreamDeckButtonOnOff(_streamDeckUIParent.GetButton(), true);
                            }
                            if (textBox.Equals(TextBoxOSCommandButtonOff))
                            {
                                return new StreamDeckButtonOnOff(_streamDeckUIParent.GetButton(), false);
                            }

                            break;
                        }
                    case EnumStreamDeckButtonActionType.LayerNavigation:
                        {
                            /*if (textBox.Equals(ComboBoxLayerNavigationButtonOn))
                            {
                                return new StreamDeckButtonOnOff(_streamDeckUIParent.GetButton(), true);
                            }
                            if (textBox.Equals(ComboBoxLayerNavigationButtonOff))
                            {
                                return new StreamDeckButtonOnOff(_streamDeckUIParent.GetButton(), false);
                            }
                            */
                            break;
                        }
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(3012, ex);
            }
            throw new Exception("Failed to determine focused component (GetStreamDeckButtonOnOff) ");
        }

        private void ButtonAddEditKeySequenceButtonOn_OnClick(object sender, RoutedEventArgs e)
        {
            AddEditKeyPress(TextBoxKeyPressButtonOn);
        }


        private void ButtonAddEditKeySequenceButtonOff_OnClick(object sender, RoutedEventArgs e)
        {
            AddEditKeyPress(TextBoxKeyPressButtonOff);
        }

        private void AddEditKeyPress(TextBox textBox)
        {
            try
            {
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
                    }
                    else
                    {
                        //If only one press was created treat it as a simple keypress
                        ((TagDataClassStreamDeck)textBox.Tag).ClearAll();
                        var osKeyPress = new KeyPress(sequenceList[0].VirtualKeyCodesAsString, sequenceList[0].LengthOfKeyPress);
                        ((TagDataClassStreamDeck)textBox.Tag).KeyPress = osKeyPress;
                        ((TagDataClassStreamDeck)textBox.Tag).KeyPress.Information = keySequenceWindow.GetInformation;
                        textBox.Text = sequenceList[0].VirtualKeyCodesAsString;
                    }
                }

                ButtonFocus.Focus();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(2044, ex);
            }
        }

    }
}
