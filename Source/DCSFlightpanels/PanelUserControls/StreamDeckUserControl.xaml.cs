using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ClassLibraryCommon;
using DCSFlightpanels.TagDataClasses;
using Newtonsoft.Json;
using NonVisuals;
using NonVisuals.Interfaces;
using NonVisuals.Saitek;
using NonVisuals.StreamDeck;
using Brushes = System.Windows.Media.Brushes;
using Image = System.Windows.Controls.Image;

namespace DCSFlightpanels.PanelUserControls
{
    /// <summary>
    /// Interaction logic for StreamDeckUserControl.xaml
    /// </summary>
    public partial class StreamDeckUserControl : UserControlBase, IGamingPanelListener, IProfileHandlerListener, ISaitekUserControl, IStreamDeckUIParent
    {
        private readonly StreamDeckPanel _streamDeck;
        private readonly TabItem _parentTabItem;
        private string _parentTabItemHeader;
        private readonly IGlobalHandler _globalHandler;
        private bool _userControlLoaded;

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
            if (_userControlLoaded)
            {
                return;
            }
            _userControlLoaded = true;
            UCStreamDeckButtonAction.SDUIParent = this;
            GenerateButtonImages(0);
            ShowGraphicConfiguration();
            SetFormState();
        }

        private void SetFormState()
        {
            try
            {
                var selectedButtonNumber = GetButtonNumber();
                
                UCStreamDeckButtonAction.Visibility = selectedButtonNumber != 0 ? Visibility.Visible : Visibility.Hidden;
                UCStreamDeckButtonImage.Visibility = selectedButtonNumber != 0 ? Visibility.Visible : Visibility.Hidden;
                StackPanelChooseButtonActionType.IsEnabled = selectedButtonNumber != 0;

                UCStreamDeckButtonAction.SetFormState();
                UCStreamDeckButtonImage.SetFormState();

                ButtonCancelButtonConfigurationChanges.IsEnabled = UCStreamDeckButtonAction.IsDirty || UCStreamDeckButtonImage.IsDirty;
                ButtonDeleteButtonConfiguration.IsEnabled = UCStreamDeckButtonAction.HasConfig || UCStreamDeckButtonImage.HasConfig;
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(471473, ex);
            }
        }

        public void ChildChangesMade()
        {
            SetFormState();
        }

        public void BipPanelRegisterEvent(object sender, BipPanelRegisteredEventArgs e)
        {
            var now = DateTime.Now.Ticks;
            //RemoveContextMenuClickHandlers();
            //SetContextMenuClickHandlers();
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

        private void GenerateButtonImages(int selectedButton)
        {
            var height = 50;
            var width = 50;
            var fontSize = 30;

            foreach (var image in Common.FindVisualChildren<Image>(GridButtons))
            {
                try
                {
                    if (!image.Name.Contains("ButtonImage"))
                    {
                        continue;
                    }

                    var number = image.Name.Replace("ButtonImage", "");
                    if (selectedButton == 0)
                    {
                        //No image selected, load all
                        image.Source = BitMapCreator.CreateBitmapSourceFromGdiBitmap(BitMapCreator.CreateBitmapImage(number, fontSize, height, width, Color.Black, Color.White));
                        image.Tag = new TagDataButtonImage();
                    }
                    else
                    {
                        if (selectedButton == int.Parse(number))
                        {
                            if (((TagDataButtonImage)image.Tag).IsSelected)
                            {
                                image.Source = BitMapCreator.CreateBitmapSourceFromGdiBitmap(BitMapCreator.CreateBitmapImage(number, fontSize, height, width, Color.Black, Color.White));
                                ((TagDataButtonImage)image.Tag).IsSelected = false;
                            }
                            else
                            {
                                image.Source = BitMapCreator.CreateBitmapSourceFromGdiBitmap(BitMapCreator.CreateBitmapImage(number, fontSize, height, width, Color.Black, Color.CadetBlue));
                                ((TagDataButtonImage)image.Tag).IsSelected = true;
                            }
                        }
                        else
                        {
                            image.Source = BitMapCreator.CreateBitmapSourceFromGdiBitmap(BitMapCreator.CreateBitmapImage(number, fontSize, height, width, Color.Black, Color.White));
                            ((TagDataButtonImage)image.Tag).IsSelected = false;
                        }
                    }
                }
                catch (Exception)
                {

                }
            }
        }

        public int GetButtonNumber()
        {
            foreach (var image in Common.FindVisualChildren<Image>(GridButtons))
            {
                if (!image.Name.Contains("ButtonImage"))
                {
                    continue;
                }

                var number = image.Name.Replace("ButtonImage", "");
                if (((TagDataButtonImage)image.Tag).IsSelected)
                {
                    return int.Parse(number);
                }
            }

            //none selected
            return 0;
        }

        public StreamDeckButtons GetButton()
        {
            return StreamDeckButtons.BUTTON1;
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
                /*if (e.UniqueId.Equals(_streamDeck.InstanceId) && e.GamingPanelEnum == GamingPanelEnum.StreamDeckMultiPanel)
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



        private void ShowGraphicConfiguration()
        {
            try
            {

                LoadComboBoxLayers("");

                if (GetSelectedStreamDeckLayer() == null)
                {
                    return;
                }

                if (!_userControlLoaded)
                {
                    return;
                }

                SetApplicationMode();
                /*
                foreach (var keyBinding in _streamDeck.KeyBindingsHashSet)
                {
                    var textBox = GetTextBox(keyBinding.StreamDeckButton, keyBinding.WhenTurnedOn);
                    if (keyBinding.OSKeyPress != null && keyBinding.Layer == GetSelectedStreamDeckLayer().Name)
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
                    if (dcsBiosBinding.Layer == GetSelectedStreamDeckLayer().Name)
                    {
                        ((TagDataClassStreamDeck)textBox.Tag).DCSBIOSBinding = dcsBiosBinding;
                    }
                }


                foreach (var bipLink in _streamDeck.BIPLinkHashSet)
                {
                    var textBox = GetTextBox(bipLink.StreamDeckButton, bipLink.WhenTurnedOn);
                    if (bipLink.BIPLights.Count > 0 && bipLink.Layer == GetSelectedStreamDeckLayer().Name)
                    {
                        ((TagDataClassStreamDeck)textBox.Tag).BIPLink = bipLink;
                    }
                }
                */
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(993013, ex);
            }
        }

        private void UpdateBIPLinkBindings(TextBox textBox)
        {
            try
            {
                //var key = GetStreamDeckKey(textBox);
                //_streamDeck.AddOrUpdateBIPLinkKeyBinding(GetSelectedStreamDeckLayer().Name, key.StreamDeckButton, ((TagDataClassStreamDeck)textBox.Tag).BIPLink, key.ButtonState);
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(3011, ex);
            }
        }

        private void UpdateKeyBindingProfileSequencedKeyStrokesStreamDeck(TextBox textBox)
        {
            try
            {
                //var key = GetSelectedButton();
                //_streamDeck.AddOrUpdateSequencedKeyBinding(GetSelectedStreamDeckLayer().Name, textBox.Text, key.StreamDeckButton, ((TagDataClassStreamDeck)textBox.Tag).GetKeySequence(), key.ButtonState);
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
                //var key = GetStreamDeckKey(textBox);
                //_streamDeck.AddOrUpdateSingleKeyBinding(GetSelectedStreamDeckLayer().Name, key.StreamDeckButton, textBox.Text, keyPressLength, key.ButtonState);
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(3012, ex);
            }
        }

        private void UpdateOSCommandBindingsStreamDeck(TextBox textBox)
        {
            try
            {
                var tag = (TagDataClassStreamDeck)textBox.Tag;
                _streamDeck.AddOrUpdateOSCommandBinding(GetSelectedStreamDeckLayer().Name, tag.Key.StreamDeckButton, tag.OSCommandObject, tag.Key.ButtonState);
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(3011, ex);
            }
        }





        private void SetTextBoxesVisibleStatus(bool show)
        {
            /*StackPanelButtonColumn0.Visibility = (show ? Visibility.Visible : Visibility.Hidden);
            StackPanelButtonColumn1.Visibility = (show ? Visibility.Visible : Visibility.Hidden);
            StackPanelButtonColumn2.Visibility = (show ? Visibility.Visible : Visibility.Hidden);
            StackPanelButtonColumn3.Visibility = (show ? Visibility.Visible : Visibility.Hidden);
            StackPanelButtonColumn4.Visibility = (show ? Visibility.Visible : Visibility.Hidden);*/
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
                    //DeleteLCDBinding(StreamDeckLCDPosition.UpperLCD, button);
                }
                else
                {
                    //DeleteLCDBinding(StreamDeckLCDPosition.LowerLCD, button);
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(7365005, ex);
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
                    _streamDeck.AddLayer(layerWindow.NewLayer);
                }
                LoadComboBoxLayers(layerWindow.NewLayer);
                SetFormState();
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
                if (MessageBox.Show("Delete layer " + GetSelectedStreamDeckLayer() + "?", "Can not be undone!", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    _streamDeck.DeleteLayer(GetSelectedStreamDeckLayer());
                }
                LoadComboBoxLayers("");
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(20235443, ex);
            }
        }

        private StreamDeckLayer GetSelectedStreamDeckLayer()
        {
            try
            {
                return (StreamDeckLayer)ComboBoxLayers.SelectedItem;
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

                SetTextBoxesVisibleStatus(ComboBoxLayers.SelectionBoxItem != null);

                SetCheckboxHomeLayer();

                SetFormState();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(20135444, ex);
            }
        }

        private void LoadComboBoxLayers(StreamDeckLayer selectedLayer)
        {
            LoadComboBoxLayers(selectedLayer.Name);
        }

        private void LoadComboBoxLayers(string selectedLayerNane)
        {
            var selectedIndex = ComboBoxLayers.SelectedIndex;

            ComboBoxLayers.SelectionChanged -= ComboBoxLayers_OnSelectionChanged;
            ComboBoxLayers.ItemsSource = _streamDeck.LayerList;
            ComboBoxLayers.Items.Refresh();

            if (!string.IsNullOrEmpty(selectedLayerNane))
            {
                foreach (StreamDeckLayer layer in ComboBoxLayers.Items)
                {
                    if (layer.Name == selectedLayerNane)
                    {
                        ComboBoxLayers.SelectedItem = layer;
                        break;
                    }
                }
                ComboBoxLayers.SelectedItem = selectedLayerNane;
            }
            else if (selectedIndex >= 0 && selectedIndex < _streamDeck.LayerList.Count)
            {
                ComboBoxLayers.SelectedIndex = selectedIndex;
            }
            else if (_streamDeck.LayerList.Count > 0)
            {
                ComboBoxLayers.SelectedIndex = 0;
            }
            ComboBoxLayers.SelectionChanged += ComboBoxLayers_OnSelectionChanged;
            SetCheckboxHomeLayer();
        }

        private void SetCheckboxHomeLayer()
        {
            CheckBoxMarkHomeLayer.Checked -= CheckBoxMarkHomeLayer_OnChecked;
            CheckBoxMarkHomeLayer.IsChecked = GetSelectedStreamDeckLayer() == _streamDeck.HomeLayer;
            CheckBoxMarkHomeLayer.Checked += CheckBoxMarkHomeLayer_OnChecked;
        }

        private void CheckBoxMarkHomeLayer_OnChecked(object sender, RoutedEventArgs e)
        {
            try
            {
                _streamDeck.SetHomeLayer(GetSelectedStreamDeckLayer());
                SetFormState();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(20135444, ex);
            }
        }

        private void ButtonImage_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                var image = (Image)sender;
                GenerateButtonImages(int.Parse(image.Name.Replace("ButtonImage", "")));
                SetFormState();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(20135444, ex);
            }
        }


        private void ButtonJSONTest_OnClick(object sender, RoutedEventArgs e)
        {
            var jsonSerializer = new JsonSerializer();
            jsonSerializer.ReferenceLoopHandling = ReferenceLoopHandling.Serialize;
            jsonSerializer.PreserveReferencesHandling = PreserveReferencesHandling.Objects;

            using (var streamWriter = new StreamWriter(@"e:temp\test_json.txt"))
            {
                using (JsonWriter jsonWriter = new JsonTextWriter(streamWriter))
                {
                    jsonWriter.Formatting = Formatting.Indented;
                    jsonSerializer.Serialize(jsonWriter, _streamDeck.KeyBindingsHashSet);
                    jsonSerializer.Serialize(jsonWriter, _streamDeck.DCSBiosBindings);
                    jsonSerializer.Serialize(jsonWriter, _streamDeck.OSCommandHashSet);
                }
            }

            /*
            using (var streamWriter = new StreamWriter(@"e:temp\test_json.txt"))
            {
                foreach (var keyBindingStreamDeck in _switchPanelStreamDeck.KeyBindingsHashSet)
                {
                    using (JsonWriter writer = new JsonTextWriter(streamWriter))
                    {
                        jsonSerializer.Serialize(writer, keyBindingStreamDeck);
                    }
                }
            }
            */
        }


        public EnumStreamDeckButtonActionType GetButtonActionType()
        {
            if (RadioButtonKeyPress.IsChecked == true)
            {
                return EnumStreamDeckButtonActionType.KeyPress;
            }
            if (RadioButtonDCSBIOS.IsChecked == true)
            {
                return EnumStreamDeckButtonActionType.DCSBIOS;
            }
            if (RadioButtonOSCommand.IsChecked == true)
            {
                return EnumStreamDeckButtonActionType.OSCommand;
            }
            if (RadioButtonNavigational.IsChecked == true)
            {
                return EnumStreamDeckButtonActionType.LayerNavigation;
            }

            return EnumStreamDeckButtonActionType.Unknown;
        }


        private void SetGraphicsState(HashSet<object> buttons)
        {
            try
            {
                SetTextBoxesVisibleStatus(GetSelectedStreamDeckLayer() != null);

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
                                        DotImage1.Visibility = key.IsPressed ? Visibility.Visible : Visibility.Collapsed;
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

        private void RadioButtonButtonTypePress_OnClick(object sender, RoutedEventArgs e)
        {
            SetFormState();
        }
    }

    internal class TagDataButtonImage
    {
        private bool _isSelected = false;

        public bool IsSelected
        {
            get => _isSelected;
            set => _isSelected = value;
        }
    }


}
