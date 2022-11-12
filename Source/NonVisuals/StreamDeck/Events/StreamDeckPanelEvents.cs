using NonVisuals.Interfaces;

namespace NonVisuals.StreamDeck.Events
{
    using System;
    using System.ComponentModel;

    using MEF;


    public class ActionTypeChangedEventArgs : EventArgs
    {
        public string BindingHash { get; set; }
        public EnumStreamDeckActionType ActionType { get; set; }
        public string TargetLayerName { get; set; }
    }

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
        public string BindingHash { get; set; }
        public StreamDeckButton SelectedButton { get; set; }
    }

    public class StreamDeckShowNewLayerArgs :  EventArgs
    {
        /*
         * Show new Layer's configuration
         */
        public string BindingHash { get; set; }
        public string SelectedLayerName { get; set; }
    }

    public class RemoteStreamDeckShowNewLayerArgs : EventArgs
    {
        /*
         * Show new Layer's configuration
         */
        public string RemoteBindingHash { get; set; }
        public string SelectedLayerName { get; set; }
    }

    public class StreamDeckDirtyReportArgs : CancelEventArgs
    {
        /*
         * Before change, unsaved configurations?
         * Cancel = true means there are...
         */
        public string BindingHash { get; set; }
    }

    public class StreamDeckDirtyNotificationArgs : CancelEventArgs
    {
        /*
         * Pro-active
         */
        public string LayerName { get; set; }
        public EnumStreamDeckButtonNames ButtonName { get; set; }
        public bool UndoIsPossible { get; set; } = true;
        public string BindingHash { get; set; }
    }

    public class StreamDeckClearSettingsArgs : EventArgs
    {
        public string BindingHash;
        public bool ClearActionConfiguration = false;
        public bool ClearFaceConfiguration = false;
        public bool ClearUIConfiguration = false;
    }

    public class StreamDeckSyncConfigurationArgs : EventArgs
    {
        public string BindingHash { get; set; }
    }

    public class StreamDeckConfigurationChangedArgs : EventArgs
    {
        public string BindingHash { get; set; }
    }

    public class StreamDeckHideDecoderEventArgs : EventArgs
    {
        public string BindingHash { get; set; }
        public string LayerName;
        public EnumStreamDeckButtonNames StreamDeckButtonName;
    }

}
