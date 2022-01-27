using NonVisuals.EventArgs;

namespace NonVisuals.Interfaces
{
    public interface IForwardPanelEventListener
    {
        /*
         * Used for announcing whether the panels should send key events to Windows API
         */
        public void SetForwardPanelEvent(object sender, ForwardPanelEventArgs e);
    }
}
