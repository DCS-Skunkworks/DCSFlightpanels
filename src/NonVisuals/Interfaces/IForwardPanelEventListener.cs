using NonVisuals.EventArgs;

namespace NonVisuals.Interfaces
{
    /// <summary>
    /// Used for announcing whether the panels should send key events to Windows API
    /// </summary>
    public interface IForwardPanelEventListener
    {
        public void SetForwardPanelEvent(object sender, ForwardPanelEventArgs e);
    }
}
