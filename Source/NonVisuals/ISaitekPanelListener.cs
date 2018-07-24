﻿namespace NonVisuals
{
    public interface ISaitekPanelListener
    {
        void SwitchesChanged(object sender, SwitchesChangedEventArgs e);
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
