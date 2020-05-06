using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using ClassLibraryCommon;
using DCS_BIOS;
using DCSFlightpanels.PanelUserControls.StreamDeck;
using DCSFlightpanels.Windows;
using DCSFlightpanels.Windows.StreamDeck;
using NonVisuals;
using NonVisuals.Interfaces;
using NonVisuals.Saitek;
using NonVisuals.StreamDeck;
using NonVisuals.StreamDeck.Events;


namespace DCSFlightpanels.PanelUserControls
{
    /// <summary>
    /// Interaction logic for StreamDeckUserControl.xaml
    /// </summary>
    public partial class StreamDeckUserControl : UserControlBase, IGamingPanelListener, IProfileHandlerListener, IGamingPanelUserControl, IStreamDeckListener
    {
        private readonly StreamDeckPanel _streamDeckPanel;
        private readonly DCSBIOS _dcsbios;
        private readonly TabItem _parentTabItem;
        private string _parentTabItemHeader;
        private readonly IGlobalHandler _globalHandler;
        private bool _userControlLoaded;

        private CancellationTokenSource _cancellationTokenSource;
        private readonly Random _random = new Random();
        private Thread _identificationThread;

        private readonly UserControlStreamDeckUIBase _uiButtonGrid;






        public StreamDeckUserControl(GamingPanelEnum panelType, HIDSkeleton hidSkeleton, TabItem parentTabItem, IGlobalHandler globalHandler, DCSBIOS dcsbios)
        {
            InitializeComponent();
            _parentTabItem = parentTabItem;
            _parentTabItemHeader = _parentTabItem.Header.ToString();
            _streamDeckPanel = new StreamDeckPanel(panelType, hidSkeleton);
            _streamDeckPanel.Attach((IGamingPanelListener)this);
            globalHandler.Attach(_streamDeckPanel);
            _globalHandler = globalHandler;
            _dcsbios = dcsbios;


            StackPanelButtonUI.Children.Clear();
            switch (panelType)
            {
                case GamingPanelEnum.StreamDeckMini:
                case GamingPanelEnum.StreamDeck:
                    {
                        var child = new UserControlStreamDeckUINormal();
                        child.PanelHash = _streamDeckPanel.PanelHash;
                        _uiButtonGrid = child;
                        StackPanelButtonUI.Children.Add(child);

                        break;
                    }
                case GamingPanelEnum.StreamDeckXL:
                    {
                        var child = new UserControlStreamDeckUIXL();
                        child.PanelHash = _streamDeckPanel.PanelHash;
                        _uiButtonGrid = child;
                        StackPanelButtonUI.Children.Add(child);
                        break;
                    }
            }


            EventHandlers.AttachStreamDeckListener(UCStreamDeckButtonAction);
            EventHandlers.AttachStreamDeckListener(UCStreamDeckButtonFace);
            EventHandlers.AttachStreamDeckListener(_uiButtonGrid);
            EventHandlers.AttachStreamDeckConfigListener(_uiButtonGrid);
            EventHandlers.AttachStreamDeckListener(this);

            UCStreamDeckButtonAction.GlobalHandler = _globalHandler;
            UCStreamDeckButtonFace.GlobalHandler = _globalHandler;

            UCStreamDeckButtonFace.PanelHash = _streamDeckPanel.PanelHash;
            UCStreamDeckButtonAction.PanelHash = _streamDeckPanel.PanelHash;
        }

        protected override void Dispose(bool dispose)
        {
            if (dispose)
            {
                _cancellationTokenSource?.Dispose();
                StackPanelButtonUI.Children.Clear();
                EventHandlers.DetachStreamDeckListener(UCStreamDeckButtonAction);
                EventHandlers.DetachStreamDeckListener(UCStreamDeckButtonFace);
                EventHandlers.DetachStreamDeckListener(_uiButtonGrid);
                EventHandlers.DetachStreamDeckConfigListener(_uiButtonGrid);
                EventHandlers.DetachStreamDeckListener(this);
                _streamDeckPanel.Dispose();
            }
        }
        
        private void UserControlStreamDeck_OnLoaded(object sender, RoutedEventArgs e)
        {
            if (!_userControlLoaded)
            {
                UCStreamDeckButtonAction.Update();
                UCStreamDeckButtonAction.AttachListener(UCStreamDeckButtonFace);
                _userControlLoaded = true;
            }
            SetFormState();
        }

        public void SetFormState()
        {
            try
            {
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
            return _streamDeckPanel;
        }

        public void UpdatesHasBeenMissed(object sender, DCSBIOSUpdatesMissedEventArgs e) { }

        public string GetName()
        {
            return GetType().Name;
        }

        public void SelectedAirframe(object sender, AirframeEventArgs e)
        {
            try
            {
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        public void SwitchesChanged(object sender, SwitchesChangedEventArgs e)
        {
            try
            {
                if (e.GamingPanelEnum == GamingPanelEnum.StreamDeck && e.UniqueId.Equals(_streamDeckPanel.PanelHash))
                {
                    NotifyButtonChanges(e.Switches);
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        public void PanelSettingsReadFromFile(object sender, SettingsReadFromFileEventArgs e)
        {
            try
            {
                LoadComboBoxLayers();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
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
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
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
                Common.ShowErrorMessageBox(ex);
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
                Common.ShowErrorMessageBox(ex);
            }
        }

        public void SettingsApplied(object sender, PanelEventArgs e)
        {
            try
            {
                /*if (e.UniqueId.Equals(_streamDeck.PanelHash) && e.GamingPanelEnum == GamingPanelEnum.StreamDeckMultiPanel)
                {
                    Dispatcher?.BeginInvoke((Action)(ShowGraphicConfiguration));
                    Dispatcher?.BeginInvoke((Action)(() => TextBoxLogStreamDeck.Text = ""));
                }*/
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        public void DeviceAttached(object sender, PanelEventArgs e) { }

        public void DeviceDetached(object sender, PanelEventArgs e) { }

        private void ButtonGetId_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_streamDeckPanel != null)
                {
                    TextBoxLogStreamDeck.Text = "";
                    TextBoxLogStreamDeck.Text = _streamDeckPanel.PanelHash;
                    Clipboard.SetText(_streamDeckPanel.InstanceId);
                    MessageBox.Show("Instance id has been copied to the ClipBoard.");
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private string _comboBoxLayerTextComparison;
        private void ComboBoxLayers_OnContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            _comboBoxLayerTextComparison = ComboBoxLayers.Text;
        }

        private void LoadComboBoxLayers()
        {
            var layerList = _streamDeckPanel.LayerNameList;

            if (layerList == null || layerList.Count == 0)
            {
                return;
            }

            ComboBoxLayers.ItemsSource = layerList;
            ComboBoxLayers.Items.Refresh();

            ComboBoxLayers.Text = _streamDeckPanel.SelectedLayerName;
        }

        private void ComboBoxLayers_OnDropDownClosed(object sender, EventArgs e)
        {
            try
            {
                if (_comboBoxLayerTextComparison == ComboBoxLayers.Text)
                {
                    return;
                }
                _streamDeckPanel.SelectedLayerName = ComboBoxLayers.Text;
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
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

                    if (_streamDeckPanel.ForwardPanelEvent)
                    {
                        if (!string.IsNullOrEmpty(_streamDeckPanel.GetKeyPressForLoggingPurposes(streamDeckButton)))
                        {
                            Dispatcher?.BeginInvoke(
                                (Action)
                                (() =>
                                 TextBoxLogStreamDeck.Text = TextBoxLogStreamDeck.Text.Insert(0, _streamDeckPanel.GetKeyPressForLoggingPurposes(streamDeckButton) + "\n")));
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
            }
            catch (Exception ex)
            {
                Dispatcher?.BeginInvoke((Action)(() => Common.ShowErrorMessageBox(ex)));
            }
        }

        private void ButtonNewLayer_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var layerWindow = new StreamDeckLayerWindow(_streamDeckPanel.LayerList, _streamDeckPanel.PanelHash);
                layerWindow.ShowDialog();
                if (layerWindow.DialogResult == true)
                {
                    _streamDeckPanel.AddLayer(layerWindow.NewLayer);
                    SetFormState();
                }
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
                if (MessageBox.Show("Delete layer " + ComboBoxLayers.Text + "?", "Can not be undone!", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    _streamDeckPanel.DeleteLayer(ComboBoxLayers.Text);

                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }



        private void ButtonAcceptButtonChanges_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                try
                {
                    var streamDeckButton = _streamDeckPanel.SelectedButton;
                    streamDeckButton.ActionForPress = UCStreamDeckButtonAction.GetStreamDeckButtonAction(true);
                    streamDeckButton.ActionForRelease = UCStreamDeckButtonAction.GetStreamDeckButtonAction(false);

                    streamDeckButton.Face = UCStreamDeckButtonFace.GetStreamDeckButtonFace(streamDeckButton.StreamDeckButtonName);

                    if (streamDeckButton.HasConfig)
                    {
                        _streamDeckPanel.SelectedLayer.AddButton(streamDeckButton);
                    }
                    else
                    {
                        _streamDeckPanel.SelectedLayer.RemoveButton(streamDeckButton);
                    }
                    UCStreamDeckButtonAction.StateSaved();
                    UCStreamDeckButtonFace.StateSaved();
                    EventHandlers.NotifyToSyncConfiguration(this);
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
                var streamDeckButton = _streamDeckPanel.SelectedButton;
                if (streamDeckButton.ActionForPress != null && streamDeckButton.ActionForPress.ActionType == EnumStreamDeckActionType.LayerNavigation && streamDeckButton.Face != null)
                {
                    streamDeckButton.Face.Dispose();
                    streamDeckButton.Face = null;
                    UCStreamDeckButtonFace.Clear();
                }
                streamDeckButton.ActionForPress = null;
                streamDeckButton.ActionForRelease = null;

                if (streamDeckButton.HasConfig)
                {
                    _streamDeckPanel.SelectedLayer.AddButton(streamDeckButton);
                }
                else
                {
                    _streamDeckPanel.SelectedLayer.RemoveButton(streamDeckButton);
                }

                UCStreamDeckButtonAction.Clear();

                EventHandlers.NotifyToSyncConfiguration(this);
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
                EventHandlers.ClearSettings(this, true, false, false);
                EventHandlers.SelectedButtonChanged(this, _streamDeckPanel.SelectedButton);
                EventHandlers.NotifyToSyncConfiguration(this);
                SetFormState();
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
                var streamDeckButton = _streamDeckPanel.SelectedButton;
                var streamDeckButtonName = streamDeckButton.StreamDeckButtonName;

                streamDeckButton.Face.Dispose(); //todo this must be properly made 
                streamDeckButton.Face = null; //todo this must be properly made 

                if (streamDeckButton.HasConfig)
                {
                    _streamDeckPanel.SelectedLayer.AddButton(streamDeckButton);
                }
                else
                {
                    _streamDeckPanel.SelectedLayer.RemoveButton(streamDeckButton);
                }
                UCStreamDeckButtonFace.Clear();
                _streamDeckPanel.ClearFace(streamDeckButtonName);
                EventHandlers.NotifyToSyncConfiguration(this);
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
                EventHandlers.ClearSettings(this, false, true, false);
                EventHandlers.SelectedButtonChanged(this, _streamDeckPanel.SelectedButton);
                EventHandlers.NotifyToSyncConfiguration(this);
                _streamDeckPanel.SelectedButton.ClearFace();
                SetFormState();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void ButtonClearImages_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                _streamDeckPanel.ClearAllFaces();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
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
                Common.ShowErrorMessageBox(ex);
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
                    var bitmap = BitMapCreator.CreateEmptyStreamDeckBitmap(_colors[_random.Next(0, 20)]);
                    _streamDeckPanel.SetImage(_random.Next(0, _streamDeckPanel.ButtonCount - 1), bitmap);
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

        public void LayerSwitched(object sender, StreamDeckShowNewLayerArgs e)
        {
            try
            {
                Dispatcher?.BeginInvoke((Action)(() =>
               {
                   if (ComboBoxLayers.Text != e.SelectedLayerName)
                   {
                       Dispatcher?.BeginInvoke((Action)LoadComboBoxLayers);
                       Dispatcher?.BeginInvoke((Action)SetFormState);
                   }
               }));
            }
            catch (Exception ex)
            {
                Common.LogError(ex);
            }
        }

        public void SelectedButtonChanged(object sender, StreamDeckSelectedButtonChangedArgs e)
        {
            try
            {
                SetFormState();
            }
            catch (Exception ex)
            {
                Common.LogError(ex);
            }
        }

        public void IsDirtyQueryReport(object sender, StreamDeckDirtyReportArgs e)
        {
            try
            {
                if (sender.Equals(this))
                {
                    return;
                }
            }
            catch (Exception ex)
            {
                Common.LogError(ex);
            }
        }

        public void SenderIsDirtyNotification(object sender, StreamDeckDirtyNotificationArgs e)
        {
            try
            {
                SetFormState();
            }
            catch (Exception ex)
            {
                Common.LogError(ex);
            }
        }

        public void ClearSettings(object sender, StreamDeckClearSettingsArgs e)
        {
            try
            {
                SetFormState();
            }
            catch (Exception ex)
            {
                Common.LogError(ex);
            }
        }

        private void ButtonInfoLayer_OnClick(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine(_streamDeckPanel.GetConfigurationInformation());
            Debug.WriteLine(HIDHandler.GetInformation());
            Debug.WriteLine(_streamDeckPanel.GetLayerHandlerInformation());
            Debug.WriteLine(EventHandlers.GetInformation());
        }

        private void ButtonExport_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var exportWindow = new ExportWindow(_streamDeckPanel.PanelHash);
                exportWindow.Show();
                SetFormState();
            }
            catch (Exception ex)
            {
                Common.LogError(ex);
            }
        }
    }
}
