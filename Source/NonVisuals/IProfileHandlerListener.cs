using System;
using System.Collections.Generic;

namespace NonVisuals
{
    public interface IProfileHandlerListener
    {
        void PanelSettingsChanged(object sender, PanelEventArgs e);
        void PanelSettingsReadFromFile(object sender, SettingsReadFromFileEventArgs e);
        void SelectedAirframe(object sender, AirframeEventArgs e);
    }

    public class SettingsReadFromFileEventArgs : EventArgs
    {
        public List<string> Settings { get; set; }
    }
}
