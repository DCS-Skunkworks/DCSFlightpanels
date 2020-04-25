using System;
using System.ComponentModel;
using System.Windows.Controls;

namespace NonVisuals.StreamDeck.Events
{
    
    public class StreamDeckLayerSwitchArgs :  EventArgs
    {
        public string SelectedLayerName { get; set; }
    }

    public class StreamDeckSelectedButtonChangeArgs : EventArgs
    {
        public StreamDeckButton Button { get; set; }
    }

    public class StreamDeckSelectedButtonChangePreviewArgs : CancelEventArgs
    {
        public bool ConfigIsUnsaved { get; set; }
        public EnumStreamDeckButtonNames ButtonName { get; set; }
    }

    public class StreamDeckUIControlDirtyChangeArgs : EventArgs
    {
        public UserControl UserControl { get; set; }
    }
}
