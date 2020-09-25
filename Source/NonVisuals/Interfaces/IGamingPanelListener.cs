using NonVisuals.Saitek;
using NonVisuals.Saitek.Panels;

namespace NonVisuals.Interfaces
{
    public interface IGamingPanelListener
    {
        void UISwitchesChanged(object sender, SwitchesChangedEventArgs e);
        void SettingsApplied(object sender, PanelEventArgs e);
        void SettingsCleared(object sender, PanelEventArgs e);
        void PanelSettingsChanged(object sender, PanelEventArgs e);
        void PanelDataAvailable(object sender, PanelDataToDCSBIOSEventEventArgs e);
        void LedLightChanged(object sender, LedLightChangeEventArgs e);
        void DeviceAttached(object sender, PanelEventArgs e);
        void DeviceDetached(object sender, PanelEventArgs e);
        void UpdatesHasBeenMissed(object sender, DCSBIOSUpdatesMissedEventArgs e);
        void BipPanelRegisterEvent(object sender, BipPanelRegisteredEventArgs e);
    }
}
