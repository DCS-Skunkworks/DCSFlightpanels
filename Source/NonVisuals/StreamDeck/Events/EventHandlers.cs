using System.Diagnostics;
using NonVisuals.Interfaces;

namespace NonVisuals.StreamDeck.Events
{
    public static class EventHandlers
    {

        public static void AttachStreamDeckListener(IStreamDeckListener streamDeckListener)
        {
            OnDirtyConfigurationsEventHandler += streamDeckListener.IsDirtyQueryReport;
            OnDirtyNotificationEventHandler += streamDeckListener.SenderIsDirtyNotification;
            OnStreamDeckShowNewLayerEventHandler += streamDeckListener.LayerSwitched;
            OnStreamDeckSelectedButtonChangedEventHandler += streamDeckListener.SelectedButtonChanged;
            OnStreamDeckClearSettingsEventHandler += streamDeckListener.ClearSettings;
        }

        public static void DetachStreamDeckListener(IStreamDeckListener streamDeckListener)
        {
            OnDirtyConfigurationsEventHandler -= streamDeckListener.IsDirtyQueryReport;
            OnDirtyNotificationEventHandler -= streamDeckListener.SenderIsDirtyNotification;
            OnStreamDeckShowNewLayerEventHandler -= streamDeckListener.LayerSwitched;
            OnStreamDeckSelectedButtonChangedEventHandler -= streamDeckListener.SelectedButtonChanged;
            OnStreamDeckClearSettingsEventHandler -= streamDeckListener.ClearSettings;
        }

        /********************************************************************************************
        *                    Querying whether there are unsaved configurations
        ********************************************************************************************/
        public delegate void DirtyConfigurationsEventHandler(object sender, StreamDeckDirtyReportArgs e);
        public static event DirtyConfigurationsEventHandler OnDirtyConfigurationsEventHandler;

        public static bool AreThereDirtyListeners(object sender)
        {
            if (OnDirtyConfigurationsEventHandler == null)
            {
                return false;
            }
            var eventArguments = new StreamDeckDirtyReportArgs();

            OnDirtyConfigurationsEventHandler?.Invoke(sender, eventArguments);
            foreach (var @delegate in OnDirtyConfigurationsEventHandler.GetInvocationList())
            {
                @delegate.DynamicInvoke(sender, eventArguments);
                
                if (eventArguments.Cancel)
                {
                    return true; //There are dirty listeners out there
                }
            }

            return false;
        }

        /********************************************************************************************
        *                    UserControl Pro-actively reports it is dirty
        ********************************************************************************************/
        public delegate void DirtyNotificationEventHandler(object sender, StreamDeckDirtyNotificationArgs e);
        public static event DirtyNotificationEventHandler OnDirtyNotificationEventHandler;

        public static void SenderNotifiesIsDirty(object sender, EnumStreamDeckButtonNames buttonName, string layerName)
        {
            var eventArguments = new StreamDeckDirtyNotificationArgs();
            eventArguments.ButtonName = buttonName;
            eventArguments.LayerName = layerName;

            OnDirtyNotificationEventHandler?.Invoke(sender, eventArguments);
        }


        /********************************************************************************************
         *                                      Layer switched
         ********************************************************************************************/
        public delegate void StreamDeckShowNewLayerEventHandler(object sender, StreamDeckShowNewLayerArgs e);
        public static event StreamDeckShowNewLayerEventHandler OnStreamDeckShowNewLayerEventHandler;

        public static void LayerSwitched(object sender, StreamDeckShowNewLayerArgs e)
        {
            OnStreamDeckShowNewLayerEventHandler?.Invoke(sender, e);
        }

        /********************************************************************************************
         *                                      Button change
         ********************************************************************************************/
        public delegate void StreamDeckSelectedButtonChangedEventHandler(object sender, StreamDeckSelectedButtonChangedArgs e);
        public static event StreamDeckSelectedButtonChangedEventHandler OnStreamDeckSelectedButtonChangedEventHandler;

        public static void SelectedButtonChanged(object sender, StreamDeckButton streamDeckButton)
        {
            var eventArgs = new StreamDeckSelectedButtonChangedArgs() {SelectedButton = streamDeckButton};
            OnStreamDeckSelectedButtonChangedEventHandler?.Invoke(sender, eventArgs);
        }


        /********************************************************************************************
         *                                      Clear all settings
         ********************************************************************************************/
        public delegate void StreamDeckClearSettingsEventHandler(object sender, StreamDeckClearSettingsArgs e);
        public static event StreamDeckClearSettingsEventHandler OnStreamDeckClearSettingsEventHandler;

        public static void ClearSettings(object sender, bool clearAction, bool clearFace, bool clearUI)
        {
            var newEvent = new StreamDeckClearSettingsArgs();
            newEvent.ClearActionConfiguration = clearAction;
            newEvent.ClearFaceConfiguration = clearFace;
            newEvent.ClearUIConfiguration = clearUI;

            OnStreamDeckClearSettingsEventHandler?.Invoke(sender, newEvent);
        }


        /********************************************************************************************
        *                            Event to notify listener to sync configuration
        ********************************************************************************************/
        public delegate void StreamDeckSyncConfigurationEventHandler(object sender, StreamDeckSyncConfigurationArgs e);
        public static event StreamDeckSyncConfigurationEventHandler OnStreamDeckSyncConfigurationEventHandler;

        public static void NotifyToSyncConfiguration(object sender)
        {
            OnStreamDeckSyncConfigurationEventHandler?.Invoke(sender, new StreamDeckSyncConfigurationArgs());
        }

        public static void AttachStreamDeckConfigListener(IStreamDeckConfigListener streamDeckConfigListener)
        {
            OnStreamDeckSyncConfigurationEventHandler += streamDeckConfigListener.SyncConfiguration;
            OnStreamDeckConfigurationChangeEventHandler += streamDeckConfigListener.ConfigurationChanged;
        }

        public static void DetachStreamDeckConfigListener(IStreamDeckConfigListener streamDeckConfigListener)
        {
            OnStreamDeckSyncConfigurationEventHandler -= streamDeckConfigListener.SyncConfiguration;
            OnStreamDeckConfigurationChangeEventHandler -= streamDeckConfigListener.ConfigurationChanged;
        }

        /********************************************************************************************
        *                   Event to notify changes in Stream Deck configuration button/layer
        ********************************************************************************************/
        public delegate void StreamDeckConfigurationChangeEventHandler(object sender, StreamDeckConfigurationChangedArgs e);
        public static event StreamDeckConfigurationChangeEventHandler OnStreamDeckConfigurationChangeEventHandler;

        public static void NotifyStreamDeckConfigurationChange(object sender)
        {
            OnStreamDeckConfigurationChangeEventHandler?.Invoke(sender, new StreamDeckConfigurationChangedArgs());
        }

        /********************************************************************************************
        *                   Event to notify DCSBIOSDecoders to go invisible (ugly workaround when I can't get handle of the various threads)
        ********************************************************************************************/
        public delegate void StreamDeckHideDecodersEventHandler(object sender, StreamDeckHideDecoderEventArgs e);
        public static event StreamDeckHideDecodersEventHandler OnStreamDeckHideDecodersEventHandler;

        public static void AttachDCSBIOSDecoder(DCSBIOSDecoder dcsbiosDecoder)
        {
            OnStreamDeckHideDecodersEventHandler += dcsbiosDecoder.HideAllEvent;
        }

        public static void DetachDCSBIOSDecoder(DCSBIOSDecoder dcsbiosDecoder)
        {
            OnStreamDeckHideDecodersEventHandler -= dcsbiosDecoder.HideAllEvent;
        }

        public static void HideDCSBIOSDecoders(DCSBIOSDecoder dcsbiosDecoder, string layerName)
        {
            var eventArgs = new StreamDeckHideDecoderEventArgs();
            eventArgs.StreamDeckButtonName = dcsbiosDecoder.StreamDeckButtonName;
            eventArgs.LayerName = layerName;
            eventArgs.StreamDeckInstanceId = dcsbiosDecoder.StreamDeckInstanceId;
            OnStreamDeckHideDecodersEventHandler?.Invoke(dcsbiosDecoder, eventArgs);
        }

        /********************************************************************************************
        *                Event to when physical panel image changes so UI can replicate
        ********************************************************************************************/
        /*public delegate void StreamDeckOledImageChangeEventHandler(object sender, StreamDeckOledImageChangeEventArgs e);
        public static event StreamDeckOledImageChangeEventHandler OnStreamDeckOledImageChangeEventHandler;

        public static void AttachOledImageListener(IOledImageListener oledImageListener)
        {
            OnStreamDeckOledImageChangeEventHandler += oledImageListener.OledImageChanged;
        }

        public static void DetachOledImageListener(IOledImageListener oledImageListener)
        {
            OnStreamDeckOledImageChangeEventHandler -= oledImageListener.OledImageChanged;
        }

        public static void NotifyOledImageChange(object sender, string streamDeckInstanceId, EnumStreamDeckButtonNames streamDeckButtonName, System.Drawing.Bitmap bitMap)
        {
            var eventArgs = new StreamDeckOledImageChangeEventArgs();
            eventArgs.StreamDeckButtonName = streamDeckButtonName;
            eventArgs.StreamDeckInstanceId = streamDeckInstanceId;
            eventArgs.Bitmap = bitMap;
            OnStreamDeckOledImageChangeEventHandler?.Invoke(sender, eventArgs);
        }*/
    }

}
