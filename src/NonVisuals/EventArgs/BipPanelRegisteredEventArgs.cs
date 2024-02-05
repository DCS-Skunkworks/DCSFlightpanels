namespace NonVisuals.EventArgs
{
    using NonVisuals.Panels.Saitek.Panels;
    using EventArgs = System.EventArgs;

    public class BipPanelRegisteredEventArgs : EventArgs
    {
        public string HIDInstance { get; set; }

        public BacklitPanelBIP BacklitPanelBip { get; set; }
    }
}
