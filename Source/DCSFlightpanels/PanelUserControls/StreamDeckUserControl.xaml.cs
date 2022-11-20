namespace DCSFlightpanels.PanelUserControls
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Windows;
    using System.Windows.Controls;

    using ClassLibraryCommon;

    using Interfaces;
    using StreamDeck;
    using DCSFlightpanels.Windows.StreamDeck;
    using NLog;
    using NonVisuals.EventArgs;
    using NonVisuals.Interfaces;
    using NonVisuals.Panels.StreamDeck.Events;
    using NonVisuals.Panels.StreamDeck.Panels;
    using NonVisuals.Panels.StreamDeck;
    using NonVisuals.Panels;
    using NonVisuals.HID;

    /// <summary>
    /// Interaction logic for StreamDeckUserControl.xaml
    /// </summary>
    public partial class StreamDeckUserControl : UserControlBase, IGamingPanelListener, IProfileHandlerListener, IGamingPanelUserControl, INvStreamDeckListener
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly StreamDeckPanel _streamDeckPanel;
        private readonly UserControlStreamDeckUIBase _uiButtonGrid;
        private string _comboBoxLayerTextComparison;

        public StreamDeckUserControl(GamingPanelEnum panelType, HIDSkeleton hidSkeleton)
        {
            InitializeComponent();
            
            _streamDeckPanel = new StreamDeckPanel(panelType, hidSkeleton);
            
            UCStreamDeckButtonAction.SetStreamDeckPanel(_streamDeckPanel);
            UCStreamDeckButtonFace.SetStreamDeckPanel(_streamDeckPanel);

            StackPanelButtonUI.Children.Clear();
            switch (panelType)
            {
                case GamingPanelEnum.StreamDeckMini:
                    {
                        var child = new UserControlStreamDeckUIMini(_streamDeckPanel);
                        _uiButtonGrid = child;
                        StackPanelButtonUI.Children.Add(child);

                        break;
                    }
                case GamingPanelEnum.StreamDeck:
                case GamingPanelEnum.StreamDeckV2:
                case GamingPanelEnum.StreamDeckMK2:
                    {
                        var child = new UserControlStreamDeckUINormal(_streamDeckPanel);
                        _uiButtonGrid = child;
                        StackPanelButtonUI.Children.Add(child);

                        break;
                    }
                case GamingPanelEnum.StreamDeckXL:
                    {
                        var child = new UserControlStreamDeckUIXL(_streamDeckPanel);
                        _uiButtonGrid = child;
                        StackPanelButtonUI.Children.Add(child);
                        break;
                    }
            }


            SDEventHandler.AttachStreamDeckListener(UCStreamDeckButtonAction);
            SDEventHandler.AttachStreamDeckListener(UCStreamDeckButtonFace);
            SDEventHandler.AttachStreamDeckListener(_uiButtonGrid);
            SDEventHandler.AttachStreamDeckConfigListener(_uiButtonGrid);
            SDEventHandler.AttachStreamDeckListener(this);
            SDEventHandler.AttachActionTypeChangedListener(UCStreamDeckButtonFace);
            AppEventHandler.AttachGamingPanelListener(this);
            UCStreamDeckButtonFace.SetStreamDeckPanel(_streamDeckPanel);
            UCStreamDeckButtonAction.SetStreamDeckPanel(_streamDeckPanel);

        }

        private bool _disposed;
        // Protected implementation of Dispose pattern.
        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    StackPanelButtonUI.Children.Clear();
                    SDEventHandler.DetachStreamDeckListener(UCStreamDeckButtonAction);
                    SDEventHandler.DetachStreamDeckListener(UCStreamDeckButtonFace);
                    SDEventHandler.DetachStreamDeckListener(_uiButtonGrid);
                    SDEventHandler.DetachStreamDeckConfigListener(_uiButtonGrid);
                    SDEventHandler.DetachStreamDeckListener(this);
                    SDEventHandler.DetachActionTypeChangedListener(UCStreamDeckButtonFace);
                    AppEventHandler.DetachGamingPanelListener(this);
                    _streamDeckPanel.Dispose();
                }

                _disposed = true;
            }

            // Call base class implementation.
            base.Dispose(disposing);
        }

        private void UserControlStreamDeck_OnLoaded(object sender, RoutedEventArgs e)
        {
            if (!UserControlLoaded)
            {
                UCStreamDeckButtonAction.Update();
                UserControlLoaded = true;
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
                ButtonEraseLayerButtons.IsEnabled = ButtonDeleteLayer.IsEnabled && _streamDeckPanel.SelectedLayer.HasButtons;
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        public override GamingPanel GetGamingPanel()
        {
            return _streamDeckPanel;
        }

        public override GamingPanelEnum GetPanelType()
        {
            return _streamDeckPanel.TypeOfPanel;
        }

        public void UpdatesHasBeenMissed(object sender, DCSBIOSUpdatesMissedEventArgs e) { }

        public string GetName()
        {
            return GetType().Name;
        }
        
        public void SwitchesChanged(object sender, SwitchesChangedEventArgs e)
        {
            try
            {
                if (e.PanelType == _streamDeckPanel.TypeOfPanel && e.HidInstance.Equals(_streamDeckPanel.HIDInstance))
                {
                    NotifyButtonChanges(e.Switches);
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        public void ProfileEvent(object sender, ProfileEventArgs e)
        {
            try
            {
                if (e.PanelBinding.PanelType == _streamDeckPanel.TypeOfPanel && _streamDeckPanel.HIDInstance == e.PanelBinding.HIDInstance)
                {
                    LoadComboBoxLayers();
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }
        
        public void SettingsModified(object sender, PanelInfoArgs e)
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
        
        public void SettingsApplied(object sender, PanelInfoArgs e)
        {
            try
            {
                if (e.HidInstance.Equals(_streamDeckPanel.HIDInstance) && e.PanelType == _streamDeckPanel.TypeOfPanel)
                {
                    SDEventHandler.NotifyToSyncConfiguration(this, _streamDeckPanel.BindingHash);
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }
        
        private void ButtonGetId_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_streamDeckPanel != null)
                {
                    TextBoxLogStreamDeck.Text = string.Empty;
                    TextBoxLogStreamDeck.Text = _streamDeckPanel.BindingHash;
                    Clipboard.SetText(_streamDeckPanel.HIDInstance);
                    MessageBox.Show("Instance id has been copied to the ClipBoard.");
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

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
                var layerWindow = new StreamDeckLayerWindow(_streamDeckPanel.LayerList, _streamDeckPanel);
                layerWindow.ShowDialog();
                if (layerWindow.DialogResult == true)
                {
                    _streamDeckPanel.AddLayer(layerWindow.NewLayer);
                    UCStreamDeckButtonAction.Update();
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
                    UCStreamDeckButtonAction.Update();
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }
        
        private void ButtonEraseLayerButtons_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (MessageBox.Show("Erase all buttons on layer " + ComboBoxLayers.Text + "?", "Can not be undone!", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    _streamDeckPanel.EraseLayerButtons(ComboBoxLayers.Text);
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
                    SDEventHandler.NotifyToSyncConfiguration(this, _streamDeckPanel.BindingHash);
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

                SDEventHandler.NotifyToSyncConfiguration(this, _streamDeckPanel.BindingHash);
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
                SDEventHandler.ClearSettings(this, true, false, false, _streamDeckPanel.BindingHash);
                SDEventHandler.SelectedButtonChanged(this, _streamDeckPanel.SelectedButton, _streamDeckPanel.BindingHash);
                SDEventHandler.NotifyToSyncConfiguration(this, _streamDeckPanel.BindingHash);
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
                SDEventHandler.NotifyToSyncConfiguration(this, _streamDeckPanel.BindingHash);
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
                SDEventHandler.ClearSettings(this, false, true, false, _streamDeckPanel.BindingHash);
                SDEventHandler.SelectedButtonChanged(this, _streamDeckPanel.SelectedButton, _streamDeckPanel.BindingHash);
                SDEventHandler.NotifyToSyncConfiguration(this, _streamDeckPanel.BindingHash);
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
                _streamDeckPanel.Identify();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        public void LayerSwitched(object sender, StreamDeckShowNewLayerArgs e)
        {
            try
            {
                if (_streamDeckPanel.BindingHash == e.BindingHash)
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
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }

        public void SelectedButtonChanged(object sender, StreamDeckSelectedButtonChangedArgs e)
        {
            try
            {
                if (_streamDeckPanel.BindingHash == e.BindingHash)
                {
                    SetFormState();
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
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
                Logger.Error(ex);
            }
        }

        public void SenderIsDirtyNotification(object sender, StreamDeckDirtyNotificationArgs e)
        {
            try
            {
                if (_streamDeckPanel.BindingHash == e.BindingHash)
                {
                    SetFormState();
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }

        public void ClearSettings(object sender, StreamDeckClearSettingsArgs e)
        {
            try
            {
                if (_streamDeckPanel.BindingHash == e.BindingHash)
                {
                    SetFormState();
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }

        private void ButtonInfoLayer_OnClick(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine(_streamDeckPanel.GetConfigurationInformation());
            Debug.WriteLine(HIDHandler.GetInformation());
            Debug.WriteLine(_streamDeckPanel.GetLayerHandlerInformation());
            Debug.WriteLine(SDEventHandler.GetInformation());
        }

        private void ButtonImport_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var importWindow = new ImportWindow(_streamDeckPanel.BindingHash);
                importWindow.Show();
                SetFormState();
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }

        private void ButtonExport_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var exportWindow = new ExportWindow(_streamDeckPanel);
                exportWindow.Show();
                SetFormState();
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }
        
        public void RemoteLayerSwitch(object sender, RemoteStreamDeckShowNewLayerArgs e)
        {
            try
            {
                if (e.RemoteBindingHash == _streamDeckPanel.BindingHash)
                {
                    Dispatcher?.BeginInvoke(SetFormState);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }

    }
}
