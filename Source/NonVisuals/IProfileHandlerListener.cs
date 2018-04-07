using System.Collections.Generic;
using ClassLibraryCommon;
using DCS_BIOS;

namespace NonVisuals
{
    public interface IProfileHandlerListener
    {
        void PanelSettingsChanged(string uniqueId, SaitekPanelsEnum saitekPanelsEnum);
        void PanelSettingsReadFromFile(List<string> settings);
        void SelectedAirframe(DCSAirframe dcsAirframe);
    }
}
