using System;
using System.Collections.Generic;

namespace NonVisuals.Interfaces
{
    public interface IProfileHandlerListener
    {
        void PanelSettingsChanged(object sender, PanelEventArgs e);
        void PanelBindingReadFromFile(object sender, PanelBindingReadFromFileEventArgs e);
        void SelectedAirframe(object sender, AirframeEventArgs e);
    }

    public class PanelBindingReadFromFileEventArgs : EventArgs
    {
        public GenericPanelBinding PanelBinding { get; set; }
    }
}
