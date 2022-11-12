namespace NonVisuals.Interfaces
{
    using StreamDeck.Events;

    public interface INvStreamDeckListener
    {
        void LayerSwitched(object sender, StreamDeckShowNewLayerArgs e);
        void RemoteLayerSwitch(object sender, RemoteStreamDeckShowNewLayerArgs e);
        void SelectedButtonChanged(object sender, StreamDeckSelectedButtonChangedArgs e);

        /*
         * Used by the caller to query which components are dirty.
         */
        void IsDirtyQueryReport(object sender, StreamDeckDirtyReportArgs e);

        /*
         * Used by component to inform others that it is dirty.
         */
        void SenderIsDirtyNotification(object sender, StreamDeckDirtyNotificationArgs e);
        void ClearSettings(object sender, StreamDeckClearSettingsArgs e);
    }

    public interface IStreamDeckConfigListener
    {
        /*
         * Used for initiating the UI to update itself to show current user configuration.
         */
        void SyncConfiguration(object sender, StreamDeckSyncConfigurationArgs e);

        /*
         * Used for messaging that the configuration has been changed and
         * whoever listens will know when it happens.
         */
        void ConfigurationChanged(object sender, StreamDeckConfigurationChangedArgs e);
    }
    
}
