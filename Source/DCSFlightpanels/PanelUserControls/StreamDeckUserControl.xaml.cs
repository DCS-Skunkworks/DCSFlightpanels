using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using ClassLibraryCommon;
using DCS_BIOS;
using DCSFlightpanels.Interfaces;
using DCSFlightpanels.Windows;
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
        private readonly DCSBIOS _dcsbios;
        private readonly TabItem _parentTabItem;
        private string _parentTabItemHeader;
        private readonly IGlobalHandler _globalHandler;
        private bool _userControlLoaded;

        private CancellationTokenSource _cancellationTokenSource;
        private Random _random = new Random();
        private Thread _identificationThread;

        private const int IMAGE_HEIGHT = 50;
        private const int IMAGE_WIDTH = 50;
        private const int FONT_SIZE = 30;

        private IStreamDeckUI _streamDeckUI;

        public StreamDeckUserControl(GamingPanelEnum panelType, HIDSkeleton hidSkeleton, TabItem parentTabItem, IGlobalHandler globalHandler, DCSBIOS dcsbios)
        {
            InitializeComponent();
            _parentTabItem = parentTabItem;
            _parentTabItemHeader = _parentTabItem.Header.ToString();
            _streamDeck = new StreamDeckPanel(hidSkeleton);
            _streamDeck.Attach((IGamingPanelListener)this);
            globalHandler.Attach(_streamDeck);
            _globalHandler = globalHandler;

            _dcsbios = dcsbios;

            StackPanelButtonUI.Children.Clear();
            switch (panelType)
            {
                case GamingPanelEnum.StreamDeckMini:
                case GamingPanelEnum.StreamDeck:
                    {
                        var child = new UserControlStreamDeckUINormal();
                        _streamDeckUI = child;
                        child.SetSDUIParent(this);
                        StackPanelButtonUI.Children.Add(child);

                        break;
                    }
                case GamingPanelEnum.StreamDeckXL:
                    {
                        var child = new UserControlStreamDeckUIXL();
                        _streamDeckUI = child;
                        child.SetSDUIParent(this);
                        StackPanelButtonUI.Children.Add(child);
                        break;
                    }
            }
            
            UCStreamDeckButtonAction.SDUIParent = this;
            UCStreamDeckButtonAction.GlobalHandler = _globalHandler;
            UCStreamDeckButtonFace.SDUIParent = this;
            UCStreamDeckButtonFace.GlobalHandler = _globalHandler;
            UCStreamDeckButtonFace.UserControlStreamDeckButtonAction = UCStreamDeckButtonAction;
            UCStreamDeckButtonAction.UserControlStreamDeckButtonFace = UCStreamDeckButtonFace;
            _streamDeckUI.HideAllDotImages();
        }

        private void StreamDeckUserControl_OnLoaded(object sender, RoutedEventArgs e)
        {
            if (!_userControlLoaded)
            {
                ShowGraphicConfiguration();
                UCStreamDeckButtonAction.Update();
                UCStreamDeckButtonAction.AttachListener(UCStreamDeckButtonFace);
                _userControlLoaded = true;
            }
            SetFormState();
        }

        public int SelectedButtonNumber => _streamDeckUI.SelectedButtonNumber;
        public EnumStreamDeckButtonNames SelectedButtonName => _streamDeckUI.SelectedButtonName;

        public void SetFormState()
        {
            try
            {
                var selectedButtonNumber = _streamDeckUI.SelectedButtonNumber;

                _streamDeckUI.SetButtonGridStatus(_streamDeck.HasLayers);

                UCStreamDeckButtonAction.Visibility = selectedButtonNumber != 0 ? Visibility.Visible : Visibility.Hidden;
                UCStreamDeckButtonFace.Visibility = selectedButtonNumber != 0 ? Visibility.Visible : Visibility.Hidden;

                UCStreamDeckButtonAction.SetFormState();
                UCStreamDeckButtonFace.SetFormState();

                ButtonAcceptButtonChanges.IsEnabled = UCStreamDeckButtonAction.IsDirty || UCStreamDeckButtonFace.IsDirty;
                ButtonCancelAction.IsEnabled = UCStreamDeckButtonAction.IsDirty && UCStreamDeckButtonAction.HasConfig;
                ButtonDeleteAction.IsEnabled = UCStreamDeckButtonAction.HasConfig;
                ButtonCancelFace.IsEnabled = UCStreamDeckButtonFace.IsDirty && UCStreamDeckButtonFace.HasConfig;
                ButtonDeleteFace.IsEnabled = UCStreamDeckButtonFace.HasConfig;

                ComboBoxLayers.IsEnabled = !(UCStreamDeckButtonAction.IsDirty || UCStreamDeckButtonFace.IsDirty);
                ButtonNewLayer.IsEnabled = ComboBoxLayers.IsEnabled;
                ButtonDeleteLayer.IsEnabled = ComboBoxLayers.IsEnabled && ComboBoxLayers.Text != null;
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

        public UserControlStreamDeckButtonAction ActionPanel
        {
            get => UCStreamDeckButtonAction;
        }

        public UserControlStreamDeckButtonFace FacePanel
        {
            get => UCStreamDeckButtonFace;
        }

        public IStreamDeckUI UIPanel
        {
            get => null;//UCStreamDeckButtonFace;
        }

        public void UpdatesHasBeenMissed(object sender, DCSBIOSUpdatesMissedEventArgs e) { }

        public void TestImage(Bitmap bitmap)
        {
            try
            {
                if (_streamDeckUI.SelectedButtonName != EnumStreamDeckButtonNames.BUTTON0_NO_BUTTON)
                {
                    _streamDeck.SetImage(_streamDeckUI.SelectedButtonName, bitmap);
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void UIShowLayer(string layerName)
        {
            try
            {
                if (string.IsNullOrEmpty(layerName) || ComboBoxLayers.Text == null)
                {
                    ClearAll();
                    return;
                }

                /*
                 * Two choices.
                 * Settings has been read from the config file => Set current layer to whatever the combobox shows.
                 * StreamDeck has a current layer => 
                 */
                LoadComboBoxLayers("");
                
                _streamDeckUI.HideAllDotImages();
                _streamDeck.ActiveLayer = ComboBoxLayers.Text;

                _streamDeckUI.UIShowLayer(layerName);
                
                SetFormState();

                SetApplicationMode();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        public string GetStreamDeckInstanceId()
        {
            return _streamDeck.InstanceId;
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
                _streamDeckUI.UnSelect();
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
                ClearAll();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(1020, ex);
            }
        }

        private void ClearAll()
        {

            _streamDeckUI.Clear();

            UCStreamDeckButtonAction.Clear();
            UCStreamDeckButtonFace.Clear();

            _streamDeck.ClearAllFaces();
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
                if (1 == 1)
                {

                }
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
                UIShowLayer(_streamDeck.HomeLayer.Name);
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
                _streamDeckUI.UnSelect();
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
                }
                UIShowLayer(layerWindow.NewLayer.Name);
                SetFormState();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(20235442, ex);
            }
        }

        private void Clear()
        {

        }

        private void ButtonDeleteLayer_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                ClearAll();

                if (MessageBox.Show("Delete layer " + ComboBoxLayers.Text + "?", "Can not be undone!", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    _streamDeck.DeleteLayer(ComboBoxLayers.Text);

                }

                LoadComboBoxLayers("");

                if (!string.IsNullOrEmpty(ComboBoxLayers.Text))
                {
                    UIShowLayer(ComboBoxLayers.Text);
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(20235443, ex);
            }
        }

        public StreamDeckLayer GetUISelectedLayer()
        {
            return _streamDeck.GetLayer(ComboBoxLayers.Text);
        }


        private string _comboBoxLayerTextComparison;
        private void ComboBoxLayers_OnContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            _comboBoxLayerTextComparison = ComboBoxLayers.Text;
        }

        private void ComboBoxLayers_OnDropDownClosed(object sender, EventArgs e)
        {
            try
            {
                if (_comboBoxLayerTextComparison == ComboBoxLayers.Text)
                {
                    return;
                }

                ClearAll();

                UIShowLayer(ComboBoxLayers.Text);

                //De-select if whatever button is selected
                _streamDeckUI. UpdateAllButtonsSelectedStatus(EnumStreamDeckButtonNames.BUTTON0_NO_BUTTON);

                SetFormState();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(20135444, ex);
            }
        }

        private void LoadComboBoxLayers(string selectedLayerName)
        {
            var selectedIndex = ComboBoxLayers.SelectedIndex;

            var layerList = _streamDeck.LayerNameList;

            if (layerList == null || layerList.Count == 0)
            {
                return;
            }

            ComboBoxLayers.ItemsSource = layerList;
            ComboBoxLayers.Items.Refresh();

            if (!string.IsNullOrEmpty(selectedLayerName))
            {
                foreach (string layer in ComboBoxLayers.Items)
                {
                    if (layer == selectedLayerName)
                    {
                        ComboBoxLayers.Text = layer;
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

            if (!_userControlLoaded)
            {
                _streamDeck.ActiveLayer = ComboBoxLayers.Text;
            }
        }

        private void ButtonAcceptButtonChanges_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                try
                {
                    var streamDeckButton = _streamDeck.GetActiveLayer().GetStreamDeckButton(_streamDeckUI.SelectedButtonName);
                    var buttonFace = UCStreamDeckButtonFace.GetStreamDeckButtonFace(streamDeckButton.StreamDeckButtonName);
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

                    if (buttonFace != null)
                    {
                        streamDeckButton.Face = buttonFace;
                        added = true;
                    }

                    if (added)
                    {
                        _streamDeck.AddStreamDeckButtonToActiveLayer(streamDeckButton);
                    }
                    UCStreamDeckButtonAction.StateSaved();
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

        private void ButtonDeleteAction_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var streamDeckButton = _streamDeck.GetActiveLayerStreamDeckButton(_streamDeckUI.SelectedButtonName);
                if (streamDeckButton.ActionForPress != null && streamDeckButton.ActionForPress.ActionType == EnumStreamDeckActionType.LayerNavigation)
                {
                    streamDeckButton.Face = null;
                    UCStreamDeckButtonFace.Clear();
                }
                streamDeckButton.ActionForPress = null;
                streamDeckButton.ActionForRelease = null;

                UCStreamDeckButtonAction.Clear();
                _streamDeck.SignalPanelChange(); //todo fix event propagation
                SetFormState();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void ButtonCancelAction_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                UCStreamDeckButtonAction.Clear();
                UCStreamDeckButtonAction.ShowActionConfiguration(_streamDeckUI.StreamDeckButton);
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void ButtonDeleteFace_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var streamDeckButton = _streamDeck.GetActiveLayerStreamDeckButton(_streamDeckUI.SelectedButtonName);
                streamDeckButton.Face = null;
                UCStreamDeckButtonFace.Clear();
                _streamDeck.SignalPanelChange(); //todo fix event propagation
                SetFormState();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void ButtonCancelFace_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                UCStreamDeckButtonFace.Clear();
                UCStreamDeckButtonFace.ShowFaceConfiguration(_streamDeckUI.StreamDeckButton);
                SetFormState();
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

        private void ButtonClearImages_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                _streamDeck.ClearAllFaces();
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

        private readonly Color[] _colors = new Color[]
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

        

    }
}
