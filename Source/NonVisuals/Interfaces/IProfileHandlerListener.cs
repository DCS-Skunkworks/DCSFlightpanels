using System;

namespace NonVisuals.Interfaces
{
    public interface IProfileHandlerListener
    {
        void PanelSettingsChanged(object sender, PanelEventArgs e);

        void PanelBindingReadFromFile(object sender, PanelBindingReadFromFileEventArgs e);

        void SelectedProfile(object sender, AirframeEventArgs e);
    }

    public class PanelBindingReadFromFileEventArgs : EventArgs
    {
        public GenericPanelBinding PanelBinding { get; set; }
    }
}
