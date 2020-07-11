using System;
using System.ComponentModel;

namespace NonVisuals.StreamDeck.Events
{
    /*
     * Buttons change => update GUI to show new Button's configuration
     * Layer change => update GUI to show new Layer's configuration
     *
     * Before Button change => unsaved configs?
     * Before Layer change => unsaved configs?
     *
     * Pro-active, unsaved configs => show save button
     */

    public class StreamDeckSelectedButtonChangedArgs : EventArgs
    {
        /*
         * Show selected Button's configuration
         */
        public StreamDeckButton SelectedButton { get; set; }
    }

    public class StreamDeckShowNewLayerArgs :  EventArgs
    {
        /*
         * Show new Layer's configuration
         */
        public string SelectedLayerName { get; set; }
    }

    public class StreamDeckDirtyReportArgs : CancelEventArgs
    {
        /*
         * Before change, unsaved configurations?
         * Cancel = true means there are...
         */
    }

    public class StreamDeckDirtyNotificationArgs : CancelEventArgs
    {
        /*
         * Pro-active
         */
        public string LayerName { get; set; }
        public EnumStreamDeckButtonNames ButtonName { get; set; }
        public bool UndoIsPossible { get; set; } = true;
    }

    public class StreamDeckClearSettingsArgs : EventArgs
    {
        public bool ClearActionConfiguration = false;
        public bool ClearFaceConfiguration = false;
        public bool ClearUIConfiguration = false;
    }

    public class StreamDeckSyncConfigurationArgs : EventArgs { }

    public class StreamDeckConfigurationChangedArgs : EventArgs { }

    public class StreamDeckHideDecoderEventArgs : EventArgs
    {
        public StreamDeckPanel StreamDeckPanelInstance;
        public string LayerName;
        public EnumStreamDeckButtonNames StreamDeckButtonName;
    }

    /*public class StreamDeckOledImageChangeEventArgs : EventArgs
    {
        public string PanelHash;
        public EnumStreamDeckButtonNames StreamDeckButtonName;
        public Bitmap Bitmap;
    }*/
}
