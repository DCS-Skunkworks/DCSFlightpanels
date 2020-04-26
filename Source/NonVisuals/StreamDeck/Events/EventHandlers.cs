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
            OnStreamDeckShowNewButtonEventHandler += streamDeckListener.SelectedButtonChanged;
            OnStreamDeckClearSettingsEventHandler += streamDeckListener.ClearSettings;
        }

        public static void DetachStreamDeckListener(IStreamDeckListener streamDeckListener)
        {
            OnDirtyConfigurationsEventHandler -= streamDeckListener.IsDirtyQueryReport;
            OnDirtyNotificationEventHandler -= streamDeckListener.SenderIsDirtyNotification;
            OnStreamDeckShowNewLayerEventHandler -= streamDeckListener.LayerSwitched;
            OnStreamDeckShowNewButtonEventHandler -= streamDeckListener.SelectedButtonChanged;
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
        public delegate void StreamDeckShowNewButtonEventHandler(object sender, StreamDeckShowNewButtonArgs e);
        public static event StreamDeckShowNewButtonEventHandler OnStreamDeckShowNewButtonEventHandler;

        public static void SelectedButtonChanged(object sender, EnumStreamDeckButtonNames streamDeckButtonName)
        {
            var eventArgs = new StreamDeckShowNewButtonArgs() {SelectedButtonName = streamDeckButtonName};
            OnStreamDeckShowNewButtonEventHandler?.Invoke(sender, eventArgs);
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
    }

}
