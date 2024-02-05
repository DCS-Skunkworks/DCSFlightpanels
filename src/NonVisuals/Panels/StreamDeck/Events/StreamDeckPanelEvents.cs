using NonVisuals.Interfaces;

namespace NonVisuals.Panels.StreamDeck.Events
{
    using System;
    using System.ComponentModel;

    using MEF;
    using StreamDeck;

    public class ActionTypeChangedEventArgs : EventArgs
    {
        public string BindingHash { get; init; }
        public EnumStreamDeckActionType ActionType { get; init; }
        public string TargetLayerName { get; init; }
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
        public string BindingHash { get; init; }
        public StreamDeckButton SelectedButton { get; init; }
        public StreamDeckPushRotary SelectedPushRotary { get; init; }
    }

    public class StreamDeckShowNewLayerArgs : EventArgs
    {
        /*
         * Show new Layer's configuration
         */
        public string BindingHash { get; init; }
        public string SelectedLayerName { get; init; }
        public bool SwitchedByUser { get; init; }
        public bool RemotelySwitched { get; init; }
    }

    public class RemoteStreamDeckShowNewLayerArgs : EventArgs
    {
        /*
         * Show new Layer's configuration
         */
        public string RemoteBindingHash { get; init; }
        public string SelectedLayerName { get; init; }
    }

    public class StreamDeckDirtyReportArgs : CancelEventArgs
    {
        /*
         * Before change, unsaved configurations?
         * Cancel = true means there are...
         */
        public string BindingHash { get; init; }
    }

    public class StreamDeckDirtyNotificationArgs : CancelEventArgs
    {
        /*
         * Pro-active
         */

        public string BindingHash { get; init; }
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
        public string BindingHash { get; init; }
    }

    public class StreamDeckConfigurationChangedArgs : EventArgs
    {
        public string BindingHash { get; init; }
    }

    public class StreamDeckHideDecoderEventArgs : EventArgs
    {
        public string BindingHash { get; init; }
        public string LayerName;
        public EnumStreamDeckButtonNames StreamDeckButtonName;
    }

}
