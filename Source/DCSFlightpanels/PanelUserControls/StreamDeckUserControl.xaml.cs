using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ClassLibraryCommon;
using DCSFlightpanels.CustomControls;
using DCSFlightpanels.Bills;
using NonVisuals;
using NonVisuals.Interfaces;
using NonVisuals.Saitek;
using NonVisuals.StreamDeck;

namespace DCSFlightpanels.PanelUserControls
{
    /// <summary>
    /// Interaction logic for StreamDeckUserControl.xaml
    /// </summary>
    public partial class StreamDeckUserControl : UserControlBase, IGamingPanelListener, IProfileHandlerListener, IGamingPanelUserControl, IStreamDeckUIParent
    {
        private readonly StreamDeckPanel _streamDeck;
        private readonly TabItem _parentTabItem;
        private string _parentTabItemHeader;
        private readonly IGlobalHandler _globalHandler;
        private bool _userControlLoaded;
        private List<RadioButton> _radioButtonActionsList = new List<RadioButton>();
        private List<StreamDeckImage> _buttonImages = new List<StreamDeckImage>();
        private List<System.Windows.Controls.Image> _dotImages = new List<System.Windows.Controls.Image>();

        private CancellationTokenSource _cancellationTokenSource;
        Random _random = new Random();
        private Thread _identificationThread;

        private const int IMAGE_HEIGHT = 50;
        private const int IMAGE_WIDTH = 50;
        private const int FONT_SIZE = 30;


        public StreamDeckUserControl(HIDSkeleton hidSkeleton, TabItem parentTabItem, IGlobalHandler globalHandler)
        {
            InitializeComponent();
            _parentTabItem = parentTabItem;
            _parentTabItemHeader = _parentTabItem.Header.ToString();
            _streamDeck = new StreamDeckPanel(hidSkeleton);
            _streamDeck.Attach((IGamingPanelListener)this);
            globalHandler.Attach(_streamDeck);
            _globalHandler = globalHandler;

            FillControlLists();
            SetImageBills();

            UCStreamDeckButtonAction.SDUIParent = this;
            UCStreamDeckButtonAction.GlobalHandler = _globalHandler;
            UCStreamDeckButtonFace.SDUIParent = this;
            UCStreamDeckButtonFace.GlobalHandler = _globalHandler;

            HideAllDotImages();
        }

        private void StreamDeckUserControl_OnLoaded(object sender, RoutedEventArgs e)
        {
            if (!_userControlLoaded)
            {
                _userControlLoaded = true;
                ShowGraphicConfiguration();
                UCStreamDeckButtonAction.Update();
            }
            SetFormState();
        }

        private void SetFormState()
        {
            try
            {
                var selectedButtonNumber = GetSelectedButtonNumber();

                SetButtonGridStatus(_streamDeck.HasLayers);

                LabelCreateLayer.Visibility = _streamDeck.HasLayers ? Visibility.Collapsed : Visibility.Visible;

                RadioButtonDCSBIOS.Visibility = _globalHandler.GetAirframe() != DCSAirframe.KEYEMULATOR ? Visibility.Visible : Visibility.Collapsed;

                UCStreamDeckButtonAction.Visibility = selectedButtonNumber != 0 ? Visibility.Visible : Visibility.Hidden;
                UCStreamDeckButtonFace.Visibility = selectedButtonNumber != 0 ? Visibility.Visible : Visibility.Hidden;
                StackPanelChooseButtonActionType.IsEnabled = selectedButtonNumber != 0;

                UCStreamDeckButtonAction.SetFormState();
                UCStreamDeckButtonFace.SetFormState();

                ButtonAcceptActionConfiguration.IsEnabled = UCStreamDeckButtonAction.IsDirty;
                ButtonCancelActionConfigurationChanges.IsEnabled = UCStreamDeckButtonAction.IsDirty && UCStreamDeckButtonAction.HasConfig;
                ButtonDeleteActionConfiguration.IsEnabled = UCStreamDeckButtonAction.HasConfig;

                ButtonAcceptImageConfiguration.IsEnabled = UCStreamDeckButtonFace.IsDirty;
                ButtonCancelImageConfigurationChanges.IsEnabled = UCStreamDeckButtonFace.IsDirty && UCStreamDeckButtonFace.HasConfig;
                ButtonDeleteImageConfiguration.IsEnabled = UCStreamDeckButtonFace.HasConfig;

                ComboBoxLayers.IsEnabled = !(UCStreamDeckButtonAction.IsDirty || UCStreamDeckButtonFace.IsDirty);
                CheckBoxMarkHomeLayer.IsEnabled = !(UCStreamDeckButtonAction.IsDirty || UCStreamDeckButtonFace.IsDirty);
                ButtonNewLayer.IsEnabled = ComboBoxLayers.IsEnabled;
                ButtonDeleteLayer.IsEnabled = ComboBoxLayers.IsEnabled && ComboBoxLayers.SelectedValue != null;
                //CheckBoxMarkHomeLayer.IsEnabled = ComboBoxLayers.IsEnabled;
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(471473, ex);
            }
        }

        public void ChildChangesMade()
        {
            try
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
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        public void BipPanelRegisterEvent(object sender, BipPanelRegisteredEventArgs e)
        {
            var now = DateTime.Now.Ticks;
            //RemoveContextMenuClickHandlers();
            //SetContextMenuClickHandlers();
        }

        public GamingPanel GetGamingPanel()
        {
            return _streamDeck;
        }

        public void UpdatesHasBeenMissed(object sender, DCSBIOSUpdatesMissedEventArgs e) { }

        private void SetImageBills()
        {
            foreach (var buttonImage in _buttonImages)
            {
                if (buttonImage.Bill != null)
                {
                    continue;
                }
                buttonImage.Bill = new BillStreamDeckFace();
                buttonImage.Bill.StreamDeckButtonName = (StreamDeckButtonNames)Enum.Parse(typeof(StreamDeckButtonNames), "BUTTON" + buttonImage.Name.Replace("ButtonImage", ""));
                buttonImage.Bill.SelectedImage = BitMapCreator.GetButtonNumberImage(buttonImage.Bill.StreamDeckButtonName, Color.Green);
                buttonImage.Bill.DeselectedImage = BitMapCreator.GetButtonNumberImage(buttonImage.Bill.StreamDeckButtonName, Color.Blue);
                buttonImage.Source = buttonImage.Bill.DeselectedImage;
            }
        }

        private void UpdateAllButtonsSelectedStatus(StreamDeckButtonNames selectedButtonName)
        {

            //System.Diagnostics.Debugger.Break();

            foreach (var buttonImage in _buttonImages)
            {
                try
                {
                    if (selectedButtonName == buttonImage.Bill.StreamDeckButtonName)
                    {
                        if (buttonImage.Bill.IsSelected)
                        {
                            buttonImage.Source = buttonImage.Bill.DeselectedImage;
                            buttonImage.Bill.IsSelected = false;
                        }
                        else
                        {
                            buttonImage.Source = buttonImage.Bill.SelectedImage;
                            buttonImage.Bill.IsSelected = true;
                        }
                    }
                    else
                    {
                        buttonImage.Source = buttonImage.Bill.DeselectedImage;
                        buttonImage.Bill.IsSelected = false;
                    }
                }
                catch (Exception ex)
                {
                    Common.ShowErrorMessageBox(ex);
                }
            }
        }


        public void TestImage(Bitmap bitmap)
        {
            try
            {
                if (GetSelectedButtonName() != StreamDeckButtonNames.BUTTON0_NO_BUTTON)
                {
                    _streamDeck.SetImage(GetSelectedButtonName(), bitmap);
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }
        private void ShowLayer(string layerName)
        {
            if (string.IsNullOrEmpty(layerName))
            {
                return;
            }

            var selectedLayer = _streamDeck.GetLayer(layerName);

            HideAllDotImages();

            foreach (var buttonImage in _buttonImages)
            {
                try
                {
                    buttonImage.Bill.Clear();

                    var streamDeckButton = selectedLayer.GetStreamDeckButton(buttonImage.Bill.StreamDeckButtonName);

                    buttonImage.Bill.Button = streamDeckButton;

                    if (streamDeckButton.HasConfig)
                    {
                        SetDotImageStatus(true, StreamDeckFunction.ButtonNumber(streamDeckButton.StreamDeckButtonName));
                    }
                    SetFormState();
                }
                catch (Exception ex)
                {
                    Common.ShowErrorMessageBox(ex);
                }
            }
        }

        private BillStreamDeckFace GetSelectedImageBill()
        {
            foreach (var image in _buttonImages)
            {
                if (image.Bill.IsSelected)
                {
                    return image.Bill;
                }
            }
            return null;
        }

        public int GetSelectedButtonNumber()
        {
            return GetSelectedImageBill()?.ButtonNumber() ?? 0;
        }

        public StreamDeckButtonNames GetSelectedButtonName()
        {
            var bill = GetSelectedImageBill();
            if (bill == null)
            {
                return StreamDeckButtonNames.BUTTON0_NO_BUTTON;
            }
            return bill.StreamDeckButtonName;
        }

        public StreamDeckButton GetSelectedStreamDeckButton()
        {
            var bill = GetSelectedImageBill();
            if (bill == null)
            {
                return null;
            }
            return _streamDeck.GetCurrentLayerStreamDeckButton(bill.StreamDeckButtonName);
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
                if (!_userControlLoaded)
                {
                    return;
                }
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

            foreach (var buttonImage in _buttonImages)
            {
                buttonImage.Bill.Clear();
            }

            UCStreamDeckButtonAction.Clear();
            UCStreamDeckButtonFace.Clear();

            if (clearAlsoProfile)
            {
                _streamDeck.ClearSettings();
            }
        }

        public void LedLightChanged(object sender, LedLightChangeEventArgs e) { }

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

        public void DeviceAttached(object sender, PanelEventArgs e) { }

        public void DeviceDetached(object sender, PanelEventArgs e) { }

        private void ButtonGetId_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_streamDeck != null)
                {
                    TextBoxLogStreamDeck.Text = "";
                    TextBoxLogStreamDeck.Text = _streamDeck.InstanceId;
                    Clipboard.SetText(_streamDeck.InstanceId);
                    MessageBox.Show("Instance id has been copied to the ClipBoard.");
                }
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
                /*
                 * Two choices.
                 * Settings has been read from the config file => Set current layer to whatever the combobox shows.
                 * StreamDeck has a current layer => 
                 */
                LoadComboBoxLayers("");

                if (GetSelectedStreamDeckLayer() == null)
                {
                    return;
                }

                if (string.IsNullOrEmpty(_streamDeck.CurrentLayerName))
                {
                    _streamDeck.CurrentLayerName = GetSelectedStreamDeckLayer().Name;
                }

                if (!_userControlLoaded)
                {
                    return;
                }
                ShowLayer(_streamDeck.CurrentLayerName);
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
                //SetGraphicsState(buttons);
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
                    var result = _streamDeck.AddLayer(layerWindow.NewLayer);
                    SetCheckboxHomeStatus(result);
                }
                LoadComboBoxLayers(layerWindow.NewLayer);
                UCStreamDeckButtonAction.Update();
                UCStreamDeckButtonFace.Update();
                SetFormState();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(20235442, ex);
            }
        }

        private void Clear()
        {
            ComboBoxButtonReleaseDelay.SelectedIndex = 0;
        }



        private void SetComboBoxButtonReleaseDelayValue(int value)
        {
            ComboBoxButtonReleaseDelay.SelectionChanged -= ComboBoxReleaseDelay_OnSelectionChanged;
            foreach (ComboBoxItem item in ComboBoxButtonReleaseDelay.Items)
            {
                if (int.Parse(item.Content.ToString()) == value)
                {
                    ComboBoxButtonReleaseDelay.SelectedItem = item;
                    break;
                }
            }
            ComboBoxButtonReleaseDelay.SelectionChanged += ComboBoxReleaseDelay_OnSelectionChanged;
        }

        private void ComboBoxReleaseDelay_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (GetSelectedButtonName() == StreamDeckButtonNames.BUTTON0_NO_BUTTON)
                {
                    return;
                }
                _streamDeck.GetStreamDeckButton(GetSelectedButtonName(), ComboBoxLayers.Text).ExecutionDelay = int.Parse(ComboBoxButtonReleaseDelay.Text);
                _streamDeck.SignalPanelChange();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void LabelDelayedReleaseInfo_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                MessageBox.Show(
                    "Stream Deck doesn't send information when a button is released, only when the button is pressed.\nSet therefore a time delay when the button can be considered released after have being pressed.",
                    "Information", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void LabelDelayedReleaseInfo_OnMouseEnter(object sender, MouseEventArgs e)
        {
            try
            {
                Mouse.OverrideCursor = Cursors.Hand;
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void LabelDelayedReleaseInfo_OnMouseLeave(object sender, MouseEventArgs e)
        {
            try
            {
                Mouse.OverrideCursor = null;
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
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
                
                _streamDeck.CurrentLayerName = ((StreamDeckLayer)ComboBoxLayers.SelectedItem).Name;

                SetCheckboxHomeLayer();

                ShowLayer(_streamDeck.CurrentLayerName);
                
                //De-select if whatever button is selected
                UpdateAllButtonsSelectedStatus(StreamDeckButtonNames.BUTTON0_NO_BUTTON);

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

        private void LoadComboBoxLayers(string selectedLayerName)
        {
            var selectedIndex = ComboBoxLayers.SelectedIndex;

            var layerList = _streamDeck.EmptyLayerList;

            if (layerList == null || layerList.Count == 0)
            {
                return;
            }

            ComboBoxLayers.SelectionChanged -= ComboBoxLayers_OnSelectionChanged;
            ComboBoxLayers.ItemsSource = layerList;
            ComboBoxLayers.Items.Refresh();

            if (!string.IsNullOrEmpty(selectedLayerName))
            {
                foreach (StreamDeckLayer layer in ComboBoxLayers.Items)
                {
                    if (layer.Name == selectedLayerName)
                    {
                        ComboBoxLayers.SelectedItem = layer;
                        break;
                    }
                }
                ComboBoxLayers.SelectedItem = selectedLayerName;
            }
            else if (selectedIndex >= 0 && selectedIndex < _streamDeck.LayerList.Count)
            {
                ComboBoxLayers.SelectedIndex = selectedIndex;
            }
            else if (_streamDeck.LayerList.Count > 0)
            {
                ComboBoxLayers.SelectedIndex = 0;
            }

            _streamDeck.CurrentLayerName = ((StreamDeckLayer)ComboBoxLayers.SelectedItem).Name;
            SetCheckboxHomeLayer();
            ComboBoxLayers.SelectionChanged += ComboBoxLayers_OnSelectionChanged;
        }

        private void SetCheckboxHomeLayer()
        {
            if (GetSelectedStreamDeckLayer() != null && _streamDeck.HomeLayer != null)
            {
                SetCheckboxHomeStatus(GetSelectedStreamDeckLayer().Name == _streamDeck.HomeLayer.Name);
            }
        }

        private void SetCheckboxHomeStatus(bool isChecked)
        {
            CheckBoxMarkHomeLayer.Checked -= CheckBoxMarkHomeLayer_CheckedChanged;
            CheckBoxMarkHomeLayer.Unchecked -= CheckBoxMarkHomeLayer_CheckedChanged;
            CheckBoxMarkHomeLayer.IsChecked = isChecked;
            CheckBoxMarkHomeLayer.Checked += CheckBoxMarkHomeLayer_CheckedChanged;
            CheckBoxMarkHomeLayer.Unchecked += CheckBoxMarkHomeLayer_CheckedChanged;
        }

        private void CheckBoxMarkHomeLayer_CheckedChanged(object sender, RoutedEventArgs e)
        {
            try
            {
                var isChecked = CheckBoxMarkHomeLayer.IsChecked == true;
                _streamDeck.SetHomeLayerStatus(isChecked, GetSelectedStreamDeckLayer());
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
                if (GetSelectedButtonName() == StreamDeckButtonNames.BUTTON0_NO_BUTTON)
                {
                    return;
                }

                var image = (StreamDeckImage)sender;

                if (GetSelectedButtonName() != image.Bill.StreamDeckButtonName && (UCStreamDeckButtonAction.IsDirty || UCStreamDeckButtonFace.IsDirty))
                {
                    if (MessageBox.Show("Discard Changes to " + GetSelectedButtonName() + " ?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        UCStreamDeckButtonAction.Clear();
                        UCStreamDeckButtonFace.Clear();
                        SetFormState();
                    }
                    else
                    {
                        e.Handled = true;
                    }
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(20135444, ex);
            }
        }

        private void ButtonImage_OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                /*
                 * bselect 
                 */
                var image = (StreamDeckImage)sender;


                UpdateAllButtonsSelectedStatus(image.Bill.StreamDeckButtonName);

                var streamDeckButton = image.Bill.Button;
                if (streamDeckButton != null)
                {
                    SetComboBoxButtonReleaseDelayValue(streamDeckButton.ExecutionDelay);
                    SetButtonActionType();
                    ComboBoxButtonReleaseDelay.SelectedValue = streamDeckButton.ExecutionDelay;
                    UCStreamDeckButtonAction.ShowActionConfiguration(streamDeckButton);
                    UCStreamDeckButtonFace.ShowFaceConfiguration(streamDeckButton);
                }
                SetFormState();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(20135444, ex);
            }
        }

        private void SetDotImageStatus(bool show, int number, bool allOthersNegated = false)
        {
            foreach (var dotImage in _dotImages)
            {
                if (allOthersNegated)
                {
                    dotImage.Visibility = show ? Visibility.Collapsed : Visibility.Visible;
                }

                if (dotImage.Name == "DotImage" + number)
                {
                    dotImage.Visibility = show ? Visibility.Visible : Visibility.Collapsed;
                }
            }
        }

        public void SetButtonActionType()
        {
            var streamDeckButton = GetSelectedStreamDeckButton();

            if (streamDeckButton == null)
            {
                return;
            }

            foreach (var radioButton in _radioButtonActionsList)
            {
                radioButton.IsChecked = false;
            }
            
            switch (streamDeckButton.ActionType)
            {
                case EnumStreamDeckActionType.KeyPress:
                    {
                        RadioButtonKeyPress.IsChecked = true;
                        break;
                    }
                case EnumStreamDeckActionType.DCSBIOS:
                    {
                        RadioButtonDCSBIOS.IsChecked = true;
                        break;
                    }
                case EnumStreamDeckActionType.OSCommand:
                    {
                        RadioButtonOSCommand.IsChecked = true;
                        break;
                    }
                case EnumStreamDeckActionType.LayerNavigation:
                    {
                        RadioButtonLayerNav.IsChecked = true;
                        break;
                    }
            }
        }

        public EnumStreamDeckActionType GetSelectedActionType()
        {
            if (RadioButtonKeyPress.IsChecked == true)
            {
                return EnumStreamDeckActionType.KeyPress;
            }
            if (RadioButtonDCSBIOS.IsChecked == true)
            {
                return EnumStreamDeckActionType.DCSBIOS;
            }
            if (RadioButtonOSCommand.IsChecked == true)
            {
                return EnumStreamDeckActionType.OSCommand;
            }
            if (RadioButtonLayerNav.IsChecked == true)
            {
                return EnumStreamDeckActionType.LayerNavigation;
            }

            return EnumStreamDeckActionType.Unknown;
        }


        private void RadioButtonButtonActionTypePress_OnClick(object sender, RoutedEventArgs e)
        {
            SetFormState();
        }

        private void ButtonAcceptActionConfiguration_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var streamDeckButton = GetSelectedStreamDeckLayer().GetStreamDeckButton(GetSelectedButtonName());
                var actionPress = UCStreamDeckButtonAction.GetStreamDeckButtonAction(true);
                var actionRelease = UCStreamDeckButtonAction.GetStreamDeckButtonAction(false);
                var added = false;

                if (actionPress != null)
                {
                    streamDeckButton.ActionForPress = actionPress;
                    added = true;
                }

                if (actionRelease != null)
                {
                    streamDeckButton.ActionForRelease = actionRelease;
                    added = true;
                }

                if (added)
                {
                    _streamDeck.AddStreamDeckButtonToCurrentLayer(streamDeckButton);
                }
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
                _streamDeck.SignalPanelChange(); //todo fix event propagation
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
                try
                {
                    var streamDeckButton = GetSelectedStreamDeckLayer().GetStreamDeckButton(GetSelectedButtonName());
                    var facePress = UCStreamDeckButtonFace.GetStreamDeckButtonFace(true);
                    var faceRelease = UCStreamDeckButtonFace.GetStreamDeckButtonFace(false);
                    var added = false;

                    if (facePress != null)
                    {
                        streamDeckButton.FaceForPress = facePress;
                        added = true;
                    }

                    if (faceRelease != null)
                    {
                        streamDeckButton.FaceForRelease = faceRelease;
                        streamDeckButton.ExecutionDelay = int.Parse(ComboBoxButtonReleaseDelay.Text);
                        added = true;
                    }

                    if (added)
                    {
                        _streamDeck.AddStreamDeckButtonToCurrentLayer(streamDeckButton);
                    }
                    UCStreamDeckButtonFace.StateSaved();

                    SetFormState();
                }
                catch (Exception ex)
                {
                    Common.ShowErrorMessageBox(ex);
                }
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
                UCStreamDeckButtonFace.Clear();
                _streamDeck.SignalPanelChange(); //todo fix event propagation
                SetFormState();
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
                UCStreamDeckButtonFace.Clear();
                UCStreamDeckButtonFace.ShowFaceConfiguration(GetSelectedStreamDeckButton());
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

        private void FillControlLists()
        {
            _radioButtonActionsList.Add(RadioButtonKeyPress);
            _radioButtonActionsList.Add(RadioButtonDCSBIOS);
            _radioButtonActionsList.Add(RadioButtonOSCommand);
            _radioButtonActionsList.Add(RadioButtonLayerNav);

            _buttonImages.Add(ButtonImage1);
            _buttonImages.Add(ButtonImage2);
            _buttonImages.Add(ButtonImage3);
            _buttonImages.Add(ButtonImage4);
            _buttonImages.Add(ButtonImage5);
            _buttonImages.Add(ButtonImage6);
            _buttonImages.Add(ButtonImage7);
            _buttonImages.Add(ButtonImage8);
            _buttonImages.Add(ButtonImage9);
            _buttonImages.Add(ButtonImage10);
            _buttonImages.Add(ButtonImage11);
            _buttonImages.Add(ButtonImage12);
            _buttonImages.Add(ButtonImage13);
            _buttonImages.Add(ButtonImage14);
            _buttonImages.Add(ButtonImage15);

            _dotImages.Add(DotImage1);
            _dotImages.Add(DotImage2);
            _dotImages.Add(DotImage3);
            _dotImages.Add(DotImage4);
            _dotImages.Add(DotImage5);
            _dotImages.Add(DotImage6);
            _dotImages.Add(DotImage7);
            _dotImages.Add(DotImage8);
            _dotImages.Add(DotImage9);
            _dotImages.Add(DotImage10);
            _dotImages.Add(DotImage11);
            _dotImages.Add(DotImage12);
            _dotImages.Add(DotImage13);
            _dotImages.Add(DotImage14);
            _dotImages.Add(DotImage15);
        }
        
        private void HideAllDotImages()
        {
            foreach (var dotImage in _dotImages)
            {
                dotImage.Visibility = Visibility.Collapsed;
            }
        }

        private void ButtonClearImages_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                _streamDeck.ClearAllImageFaces();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(2019, ex);
            }
        }

        private void ButtonIdentifyPanel_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_identificationThread == null)
                {
                    _cancellationTokenSource = new CancellationTokenSource();
                    _identificationThread = new Thread(() => ThreadedPanelIdentification(_cancellationTokenSource.Token));
                    _identificationThread.Start();
                }
                else
                {
                    _cancellationTokenSource.Cancel();
                    _identificationThread = null;
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(2019, ex);
            }
        }

        private Color[] _colors = new Color[]
        {
            Color.White, Color.Aqua, Color.Black, Color.Blue, Color.BurlyWood, Color.Chartreuse, Color.DarkOrange, Color.Lavender, Color.Silver, Color.Red,
            Color.Yellow, Color.Violet, Color.Thistle, Color.Teal, Color.Salmon, Color.SeaShell, Color.PowderBlue, Color.PaleGreen, Color.Olive, Color.LawnGreen
        };

        private void ThreadedPanelIdentification(CancellationToken cancellationToken)
        {
            try
            {
                while (true)
                {
                    var bitmap = BitMapCreator.CreateEmtpyStreamDeckBitmap(_colors[_random.Next(0, 20)]);
                    _streamDeck.SetImage(_random.Next(1, 15), bitmap);
                    Thread.Sleep(50);
                    if (cancellationToken.IsCancellationRequested)
                    {
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Common.LogError(ex);
            }
        }

        private void SetButtonGridStatus(bool enabled)
        {
            foreach (var streamDeckImage in _buttonImages)
            {
                streamDeckImage.IsEnabled = enabled;
            }
        }
    }
}
