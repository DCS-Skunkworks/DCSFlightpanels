using NonVisuals.EventArgs;

namespace NonVisuals.Interfaces
{
    public interface IPanelEventListener
    {
        /// <summary>
        /// Used for announcing panel events, attached, removed, found. 
        /// </summary>
        public void PanelEvent(object sender, PanelEventArgs e);
    }
}
