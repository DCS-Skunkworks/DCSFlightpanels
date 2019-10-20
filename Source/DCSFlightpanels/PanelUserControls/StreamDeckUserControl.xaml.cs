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
        private List<RadioButton> _radioButtonActionsList = new List<RadioButton>();

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
            UCStreamDeckButtonAction.GlobalHandler = _globalHandler;
            GenerateButtonImages(StreamDeckButtonNames.BUTTON0_NO_BUTTON);
            ShowGraphicConfiguration();
            SetFormState();
            SetRadioButtonLists();
            UCStreamDeckButtonAction.Update();
        }

        private void SetFormState()
        {
            try
            {
                var selectedButtonNumber = GetSelectedButtonNumber();
                
                RadioButtonSRS.Visibility = _globalHandler.GetAirframe() == DCSAirframe.KEYEMULATOR_SRS ? Visibility.Visible : Visibility.Collapsed;
                RadioButtonDCSBIOS.Visibility = _globalHandler.GetAirframe() != DCSAirframe.KEYEMULATOR ? Visibility.Visible : Visibility.Collapsed;

                UCStreamDeckButtonAction.Visibility = selectedButtonNumber != 0 ? Visibility.Visible : Visibility.Hidden;
                UCStreamDeckButtonImage.Visibility = selectedButtonNumber != 0 ? Visibility.Visible : Visibility.Hidden;
                StackPanelChooseButtonActionType.IsEnabled = selectedButtonNumber != 0;

                UCStreamDeckButtonAction.SetFormState();
                UCStreamDeckButtonImage.SetFormState();

                ButtonAcceptActionConfiguration.IsEnabled = UCStreamDeckButtonAction.IsDirty;
                ButtonCancelActionConfigurationChanges.IsEnabled = UCStreamDeckButtonAction.IsDirty && UCStreamDeckButtonAction.HasConfig;
                ButtonDeleteActionConfiguration.IsEnabled = UCStreamDeckButtonAction.HasConfig;

                ButtonAcceptImageConfiguration.IsEnabled = UCStreamDeckButtonImage.IsDirty;
                ButtonCancelImageConfigurationChanges.IsEnabled = UCStreamDeckButtonImage.IsDirty && UCStreamDeckButtonImage.HasConfig;
                ButtonDeleteImageConfiguration.IsEnabled = UCStreamDeckButtonImage.HasConfig;

                ComboBoxLayers.IsEnabled = !(UCStreamDeckButtonAction.IsDirty || UCStreamDeckButtonImage.IsDirty);
                ButtonNewLayer.IsEnabled = ComboBoxLayers.IsEnabled;
                ButtonDeleteLayer.IsEnabled = ComboBoxLayers.IsEnabled;
                CheckBoxMarkHomeLayer.IsEnabled = ComboBoxLayers.IsEnabled;
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(471473, ex);
            }
        }

        public void ChildChangesMade()
        {
            if (UCStreamDeckButtonAction.IsDirty)
            {
                foreach (var radioButton in _radioButtonActionsList)
                {
                    radioButton.IsEnabled = radioButton.IsChecked == true;
                }
            }
            else
            {
                foreach (var radioButton in _radioButtonActionsList)
                {
                    radioButton.IsEnabled = true;
                }
            }
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

        private void GenerateButtonImages(StreamDeckButtonNames selectedButtonName)
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
                    
                    if (selectedButtonName == StreamDeckButtonNames.BUTTON0_NO_BUTTON)
                    {
                        //No image selected, load all
                        var tagDataClass = new TagDataClassButtonImage();
                        tagDataClass.StreamDeckButtonName = (StreamDeckButtonNames)Enum.Parse(typeof(StreamDeckButtonNames), "BUTTON" + image.Name.Replace("ButtonImage", ""));
                        image.Tag = tagDataClass;
                        image.Source = BitMapCreator.CreateBitmapSourceFromGdiBitmap(BitMapCreator.CreateBitmapImage(tagDataClass.ButtonNumber().ToString(), fontSize, height, width, Color.Black, Color.White));
                        
                    }
                    else
                    {
                        var tagDataButtonImage = (TagDataClassButtonImage)image.Tag;

                        if (selectedButtonName == tagDataButtonImage.StreamDeckButtonName)
                        {
                            if (((TagDataClassButtonImage)image.Tag).IsSelected)
                            {
                                image.Source = BitMapCreator.CreateBitmapSourceFromGdiBitmap(BitMapCreator.CreateBitmapImage(tagDataButtonImage.ButtonNumber().ToString(), fontSize, height, width, Color.Black, Color.White));
                                ((TagDataClassButtonImage)image.Tag).IsSelected = false;
                            }
                            else
                            {
                                image.Source = BitMapCreator.CreateBitmapSourceFromGdiBitmap(BitMapCreator.CreateBitmapImage(tagDataButtonImage.ButtonNumber().ToString(), fontSize, height, width, Color.Black, Color.CadetBlue));
                                ((TagDataClassButtonImage)image.Tag).IsSelected = true;
                            }
                        }
                        else
                        {
                            image.Source = BitMapCreator.CreateBitmapSourceFromGdiBitmap(BitMapCreator.CreateBitmapImage(tagDataButtonImage.ButtonNumber().ToString(), fontSize, height, width, Color.Black, Color.White));
                            ((TagDataClassButtonImage)image.Tag).IsSelected = false;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Common.ShowErrorMessageBox(ex);
                }
            }
        }

        private TagDataClassButtonImage GetSelectedImageDataClass()
        {
            foreach (var image in Common.FindVisualChildren<Image>(GridButtons))
            {
                if (!image.Name.Contains("ButtonImage"))
                {
                    continue;
                }

                if (((TagDataClassButtonImage)image.Tag).IsSelected)
                {
                    return (TagDataClassButtonImage)image.Tag;
                }
            }

            return null;
        }

        public int GetSelectedButtonNumber()
        {
            var tagDataClass = GetSelectedImageDataClass();
            
            return tagDataClass?.ButtonNumber() ?? 0;
        }

        public StreamDeckButtonNames GetSelectedButtonName()
        {
            var tagDataClass = GetSelectedImageDataClass();
            return tagDataClass?.StreamDeckButtonName ?? StreamDeckButtonNames.BUTTON0_NO_BUTTON;
        }
        
        public StreamDeckButton GetSelectedStreamDeckButton()
        {
            var tagDataClass = GetSelectedImageDataClass();
            return _streamDeck.GetCurrentLayerStreamDeckButton(tagDataClass.StreamDeckButtonName);
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
            RadioButtonKeyPress.IsChecked = false;
            RadioButtonDCSBIOS.IsChecked = false;
            RadioButtonOSCommand.IsChecked = false;
            RadioButtonLayerNav.IsChecked = false;
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
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(993013, ex);
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
                UCStreamDeckButtonAction.Update();
                UCStreamDeckButtonImage.Update();
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

        public StreamDeckLayer GetSelectedStreamDeckLayer()
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
                
                SetCheckboxHomeLayer();

                _streamDeck.CurrentLayer = ComboBoxLayers.Text;
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

            _streamDeck.CurrentLayer = ComboBoxLayers.Text;
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
        
        private void ButtonImage_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                var image = (Image)sender;
                var imageTagClass = (TagDataClassButtonImage)image.Tag;

                if (GetSelectedButtonName() != imageTagClass.StreamDeckButtonName && (UCStreamDeckButtonAction.IsDirty || UCStreamDeckButtonImage.IsDirty))
                {
                    if (MessageBox.Show("Discard Changes to " + GetSelectedButtonName() + " ?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        UCStreamDeckButtonAction.Clear();
                        UCStreamDeckButtonImage.Clear();
                        SetFormState();
                    }
                    else
                    {
                        e.Handled = true;
                        return;
                    }
                }
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
                var imageTagClass = (TagDataClassButtonImage) image.Tag;
                

                GenerateButtonImages(imageTagClass.StreamDeckButtonName);

                var streamDeckButton = _streamDeck.GetCurrentLayerStreamDeckButton(imageTagClass.StreamDeckButtonName);
                SetButtonActionType();
                UCStreamDeckButtonAction.ShowActionConfiguration(streamDeckButton);
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
                    /*jsonSerializer.Serialize(jsonWriter, _streamDeck.KeyBindingsHashSet);
                    jsonSerializer.Serialize(jsonWriter, _streamDeck.DCSBiosBindings);
                    jsonSerializer.Serialize(jsonWriter, _streamDeck.OSCommandHashSet);*/
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

        public void SetButtonActionType()
        {
            var streamDeckButton = GetSelectedStreamDeckButton();
            foreach (var radioButton in _radioButtonActionsList)
            {
                radioButton.IsChecked = false;
            }

            if (streamDeckButton.StreamDeckButtonActionForPress == null)
            {
                return;
            }

            switch (streamDeckButton.StreamDeckButtonActionForPress.ActionType)
            {
                case EnumStreamDeckButtonActionType.KeyPress:
                {
                    RadioButtonKeyPress.IsChecked = true;
                    break;
                }
                case EnumStreamDeckButtonActionType.DCSBIOS:
                {
                    RadioButtonDCSBIOS.IsChecked = true;
                    break;
                }
                case EnumStreamDeckButtonActionType.OSCommand:
                {
                    RadioButtonOSCommand.IsChecked = true;
                    break;
                }
                case EnumStreamDeckButtonActionType.LayerNavigation:
                {
                    RadioButtonLayerNav.IsChecked = true;
                    break;
                }
                case EnumStreamDeckButtonActionType.SRS:
                {
                    RadioButtonSRS.IsChecked = true;
                    break;
                }
            }
        }

        public EnumStreamDeckButtonActionType GetSelectedActionType()
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
            if (RadioButtonLayerNav.IsChecked == true)
            {
                return EnumStreamDeckButtonActionType.LayerNavigation;
            }

            return EnumStreamDeckButtonActionType.Unknown;
        }


        private void SetGraphicsState(HashSet<object> buttons)
        {
            try
            {
                foreach (var streamDeckButton35 in buttons)
                {
                    var streamDeckButton = (StreamDeckButton)streamDeckButton35;
                    switch (streamDeckButton.StreamDeckButtonName)
                    {
                        case StreamDeckButtonNames.BUTTON1:
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
                        case StreamDeckButtonNames.BUTTON2:
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
                        case StreamDeckButtonNames.BUTTON3:
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
                        case StreamDeckButtonNames.BUTTON4:
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
                        case StreamDeckButtonNames.BUTTON5:
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
                        case StreamDeckButtonNames.BUTTON6:
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
                        case StreamDeckButtonNames.BUTTON7:
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
                        case StreamDeckButtonNames.BUTTON8:
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
                        case StreamDeckButtonNames.BUTTON9:
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
                        case StreamDeckButtonNames.BUTTON10:
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
                        case StreamDeckButtonNames.BUTTON11:
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
                        case StreamDeckButtonNames.BUTTON12:
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
                        case StreamDeckButtonNames.BUTTON13:
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
                        case StreamDeckButtonNames.BUTTON14:
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
                        case StreamDeckButtonNames.BUTTON15:
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

        private void RadioButtonButtonActionTypePress_OnClick(object sender, RoutedEventArgs e)
        {
            /*
            if (UCStreamDeckButtonAction.IsDirty || UCStreamDeckButtonImage.IsDirty)
            {
                if (MessageBox.Show("Discard Changes to " + GetSelectedButtonName() + " ?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    UCStreamDeckButtonAction.Clear();
                    UCStreamDeckButtonImage.Clear();
                }
                else
                {
                    e.Handled = true;
                    return;
                }
            }*/
            SetFormState();
        }

        private void ButtonAcceptActionConfiguration_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var streamDeckButton = GetSelectedStreamDeckLayer().GetStreamDeckButton(GetSelectedButtonName());
                streamDeckButton.StreamDeckButtonActionForPress = UCStreamDeckButtonAction.GetStreamDeckButtonAction(true);
                streamDeckButton.StreamDeckButtonActionForRelease = UCStreamDeckButtonAction.GetStreamDeckButtonAction(false);
                _streamDeck.AddStreamDeckButtonToCurrentLayer(streamDeckButton);
                UCStreamDeckButtonAction.StateSaved();
                SetFormState();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void ButtonDeleteActionConfiguration_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                UCStreamDeckButtonAction.Clear();
                SetFormState();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void ButtonCancelActionConfigurationChanges_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                UCStreamDeckButtonAction.Clear();
                UCStreamDeckButtonAction.ShowActionConfiguration(GetSelectedStreamDeckButton());
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void ButtonAcceptImageConfiguration_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                //var streamDeckButton = GetSelectedStreamDeckLayer().GetStreamDeckButton(GetSelectedButtonName());
                //streamDeckButton.StreamDeckButtonActionForPress = UCStreamDeckButtonAction.GetStreamDeckButtonActionForPress();
                //streamDeckButton.StreamDeckButtonActionForPress = UCStreamDeckButtonAction.GetStreamDeckButtonActionFor;
                //streamDeckButton.StreamDeckButtonAction = UCStreamDeckButtonAction.get
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void ButtonDeleteImageConfiguration_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                UCStreamDeckButtonImage.Clear();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void ButtonCancelImageConfigurationChanges_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        public List<string> GetStreamDeckLayerNames()
        {
            return _streamDeck.GetStreamDeckLayerNames();
        }

        private void SetRadioButtonLists()
        {
            _radioButtonActionsList.Add(RadioButtonKeyPress);
            _radioButtonActionsList.Add(RadioButtonDCSBIOS);
            _radioButtonActionsList.Add(RadioButtonOSCommand);
            _radioButtonActionsList.Add(RadioButtonLayerNav);
            _radioButtonActionsList.Add(RadioButtonSRS);
        }
    }




}
