using NonVisuals.EventArgs;

namespace NonVisuals.Interfaces
{
    public interface ILedLightPanelListener
    {
        /// <summary>
        /// Used by those UserControls who's panels can show LED lights.
        /// Used to show the same color in the UserControl as the physical panels.
        /// </summary>
        void LedLightChanged(object sender, LedLightChangeEventArgs e);
    }
}
