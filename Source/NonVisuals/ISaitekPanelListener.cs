using System.Collections.Generic;

namespace NonVisuals
{
    public interface ISaitekPanelListener
    {
        void SwitchesChanged(string uniqueId, SaitekPanelsEnum saitekPanelsEnum, HashSet<object> hashSet);
        void SettingsApplied(string uniqueId, SaitekPanelsEnum saitekPanelsEnum);
        void SettingsCleared(string uniqueId, SaitekPanelsEnum saitekPanelsEnum);
        void PanelDataAvailable(string stringData);
        void LedLightChanged(string uniqueId, SaitekPanelLEDPosition saitekPanelLEDPosition, PanelLEDColor panelLEDColor);
        void DeviceAttached(string uniqueId, SaitekPanelsEnum saitekPanelsEnum);
        void DeviceDetached(string uniqueId, SaitekPanelsEnum saitekPanelsEnum);
        void UpdatesHasBeenMissed(string uniqueId, SaitekPanelsEnum saitekPanelsEnum, int count);
    }
}
