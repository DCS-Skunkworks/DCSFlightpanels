namespace NonVisuals.Interfaces
{
    using Panels.StreamDeck.Events;

    public interface INvStreamDeckListener
    {
        void LayerSwitched(object sender, StreamDeckShowNewLayerArgs e);
        void RemoteLayerSwitch(object sender, RemoteStreamDeckShowNewLayerArgs e);
        void SelectedButtonChanged(object sender, StreamDeckSelectedButtonChangedArgs e);
        
        /// <summary>
        /// Used by the caller to query which components are dirty. 
        /// </summary>
        void IsDirtyQueryReport(object sender, StreamDeckDirtyReportArgs e);
        
        /// <summary>
        /// Used by component to inform others that it is dirty. 
        /// </summary>
        void SenderIsDirtyNotification(object sender, StreamDeckDirtyNotificationArgs e);
        void ClearSettings(object sender, StreamDeckClearSettingsArgs e);
    }

    public interface IStreamDeckConfigListener
    {
        /// <summary>
        /// Used for initiating the UI to update itself to show current user configuration. 
        /// </summary>
        void SyncConfiguration(object sender, StreamDeckSyncConfigurationArgs e);
        
        /// <summary>
        /// Used for messaging that the configuration has been changed and whoever listens will know when it happens.
        /// </summary>
        void ConfigurationChanged(object sender, StreamDeckConfigurationChangedArgs e);
    }
    
}
