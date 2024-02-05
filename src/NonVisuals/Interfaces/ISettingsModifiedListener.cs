using NonVisuals.EventArgs;

namespace NonVisuals.Interfaces
{
    public interface ISettingsModifiedListener
    {
        /// <summary>
        /// Used by ProfileHandler to detect changes in panel configurations. 
        /// </summary>
        void SettingsModified(object sender, PanelInfoArgs e); 
    }
}
