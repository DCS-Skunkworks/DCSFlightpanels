using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        private Random _random = new Random();
        private Thread _identificationThread;

        private IStreamDeckUI _streamDeckUI;






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
                        child.StreamDeckInstanceId = _streamDeckPanel.InstanceId;
                        _streamDeckUI = child;
                        StackPanelButtonUI.Children.Add(child);

                        break;
                    }
                case GamingPanelEnum.StreamDeckXL:
                    {
                        var child = new UserControlStreamDeckUIXL();
                        child.StreamDeckInstanceId = _streamDeckPanel.InstanceId;
                        _streamDeckUI = child;
                        StackPanelButtonUI.Children.Add(child);
                        break;
                    }
            }

            EventHandlers.AttachStreamDeckListener(UCStreamDeckButtonAction);
            EventHandlers.AttachStreamDeckListener(UCStreamDeckButtonFace);
            EventHandlers.AttachStreamDeckListener(_streamDeckUI);
            EventHandlers.AttachStreamDeckListener(this);

            UCStreamDeckButtonAction.GlobalHandler = _globalHandler;
            UCStreamDeckButtonFace.GlobalHandler = _globalHandler;

            UCStreamDeckButtonFace.StreamDeckInstanceId = _streamDeckPanel.InstanceId;
            UCStreamDeckButtonAction.StreamDeckInstanceId = _streamDeckPanel.InstanceId;

        }

        private void StreamDeckUserControl_OnLoaded(object sender, RoutedEventArgs e)
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
                var selectedButtonNumber = _streamDeckPanel.SelectedButtonNumber;
                
                if (_streamDeckUI.PastedStreamDeckButton != null)
                {
                    _streamDeckPanel.AddStreamDeckButtonToActiveLayer(_streamDeckUI.PastedStreamDeckButton);
                    _streamDeckUI.PastedStreamDeckButton = null;
                }

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
                if (e.GamingPanelEnum == GamingPanelEnum.StreamDeck && e.UniqueId.Equals(_streamDeckPanel.InstanceId))
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
                /*if (e.UniqueId.Equals(_streamDeck.InstanceId) && e.GamingPanelEnum == GamingPanelEnum.StreamDeckMultiPanel)
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
                    TextBoxLogStreamDeck.Text = _streamDeckPanel.InstanceId;
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
        
        private void ComboBoxLayers_OnDropDownOpened(object sender, EventArgs e)
        {
            
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
                Dispatcher?.BeginInvoke((Action) (() => Common.ShowErrorMessageBox(ex)));
            }
        }

        private void ButtonNewLayer_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var layerWindow = new StreamDeckLayerWindow(_streamDeckPanel.LayerList);
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

        private void ButtonAcceptButtonChanges_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                try
                {
                    var streamDeckButton = _streamDeckPanel.SelectedButton;
                    streamDeckButton.Face = UCStreamDeckButtonFace.GetStreamDeckButtonFace(streamDeckButton.StreamDeckButtonName);
                    streamDeckButton.ActionForPress = UCStreamDeckButtonAction.GetStreamDeckButtonAction(true);
                    streamDeckButton.ActionForRelease = UCStreamDeckButtonAction.GetStreamDeckButtonAction(false);

                    if (streamDeckButton.HasConfig)
                    {
                        _streamDeckPanel.AddStreamDeckButtonToActiveLayer(streamDeckButton);
                    }
                    else
                    {
                        _streamDeckPanel.RemoveButton(streamDeckButton);
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
                var streamDeckButton = _streamDeckPanel.SelectedButton;
                if (streamDeckButton.ActionForPress != null && streamDeckButton.ActionForPress.ActionType == EnumStreamDeckActionType.LayerNavigation)
                {
                    streamDeckButton.Face = null;
                    UCStreamDeckButtonFace.Clear();
                }
                streamDeckButton.ActionForPress = null;
                streamDeckButton.ActionForRelease = null;

                UCStreamDeckButtonAction.Clear();
                _streamDeckPanel.SignalPanelChange(); //todo fix event propagation
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
                EventHandlers.ClearSettings(this,true,false,false);
                EventHandlers.SelectedButtonChanged(this, _streamDeckPanel.SelectedButtonName);
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
                streamDeckButton.Face = null;
                UCStreamDeckButtonFace.Clear();
                _streamDeckPanel.SignalPanelChange(); //todo fix event propagation
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
                EventHandlers.SelectedButtonChanged(this, _streamDeckPanel.SelectedButtonName);
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
                    var bitmap = BitMapCreator.CreateEmtpyStreamDeckBitmap(_colors[_random.Next(0, 20)]);
                    _streamDeckPanel.SetImage(_random.Next(1, _streamDeckPanel.ButtonCount), bitmap);
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
                if (ComboBoxLayers.Text != e.SelectedLayerName)
                {
                    Dispatcher?.BeginInvoke((Action)LoadComboBoxLayers);
                }
            }
            catch (Exception ex)
            {
                Common.LogError(ex);
            }
        }

        public void SelectedButtonChanged(object sender, StreamDeckShowNewButtonArgs e)
        {
            try
            {
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
            }
            catch (Exception ex)
            {
                Common.LogError(ex);
            }
        }
    }
}
