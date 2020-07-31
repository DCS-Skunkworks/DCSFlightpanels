using NonVisuals.StreamDeck.Events;

namespace NonVisuals.Interfaces
{
    public interface IStreamDeckListener
    {
        void LayerSwitched(object sender, StreamDeckShowNewLayerArgs e);
        void RemoteLayerSwitch(object sender, RemoteStreamDeckShowNewLayerArgs e);
        void SelectedButtonChanged(object sender, StreamDeckSelectedButtonChangedArgs e);
        void IsDirtyQueryReport(object sender, StreamDeckDirtyReportArgs e);
        void SenderIsDirtyNotification(object sender, StreamDeckDirtyNotificationArgs e);
        void ClearSettings(object sender, StreamDeckClearSettingsArgs e);
    }

    public interface IStreamDeckConfigListener
    {
        void SyncConfiguration(object sender, StreamDeckSyncConfigurationArgs e);
        void ConfigurationChanged(object sender, StreamDeckConfigurationChangedArgs e);
    }
    
}
