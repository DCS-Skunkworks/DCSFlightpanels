using NonVisuals.EventArgs;

namespace NonVisuals.Interfaces
{
    public interface ISettingsModifiedListener
    {
        /*
         * Used by ProfileHandler to detect changes in panel configurations.
         */
        void SettingsModified(object sender, PanelEventArgs e); 
    }
}
