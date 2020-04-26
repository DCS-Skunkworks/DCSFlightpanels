using NonVisuals.StreamDeck.Events;

namespace NonVisuals.Interfaces
{
    public interface IStreamDeckListener
    {
        void LayerSwitched(object sender, StreamDeckShowNewLayerArgs e);
        void SelectedButtonChanged(object sender, StreamDeckShowNewButtonArgs e);
        void IsDirtyQueryReport(object sender, StreamDeckDirtyReportArgs e);
        void SenderIsDirtyNotification(object sender, StreamDeckDirtyNotificationArgs e);
        void ClearSettings(object sender, StreamDeckClearSettingsArgs e);
    }
}
