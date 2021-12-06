namespace NonVisuals.StreamDeck.Events
{
    using System.Text;

    using MEF;

    using NonVisuals.Interfaces;

    public static class SDEventHandlers
    {







        /*
         *          _____ _                                _           _    
         *         / ____| |                              | |         | |   
         *        | (___ | |_ _ __ ___  __ _ _ __ ___   __| | ___  ___| | __
         *         \___ \| __| '__/ _ \/ _` | '_ ` _ \ / _` |/ _ \/ __| |/ /
         *         ____) | |_| | |  __/ (_| | | | | | | (_| |  __/ (__|   < 
         *        |_____/ \__|_|  \___|\__,_|_| |_| |_|\__,_|\___|\___|_|\_\
         */
        public static string GetInformation()
        {
            var stringBuilder = new StringBuilder(200);

            stringBuilder.Append("\nOnDirtyConfigurationsEventHandler :").Append(OnDirtyConfigurationsEventHandler != null ? OnDirtyConfigurationsEventHandler.GetInvocationList().Length.ToString() : "0").Append("\n");
            stringBuilder.Append("OnDirtyNotificationEventHandler :").Append(OnDirtyNotificationEventHandler != null ? OnDirtyNotificationEventHandler.GetInvocationList().Length.ToString() : "0").Append("\n");
            stringBuilder.Append("OnStreamDeckShowNewLayerEventHandler :").Append(OnStreamDeckShowNewLayerEventHandler != null ? OnStreamDeckShowNewLayerEventHandler.GetInvocationList().Length.ToString() : "0").Append("\n");
            stringBuilder.Append("OnStreamDeckSelectedButtonChangedEventHandler :").Append(OnStreamDeckSelectedButtonChangedEventHandler != null ? OnStreamDeckSelectedButtonChangedEventHandler.GetInvocationList().Length.ToString() : "0").Append("\n");
            stringBuilder.Append("OnStreamDeckClearSettingsEventHandler :").Append(OnStreamDeckClearSettingsEventHandler != null ? OnStreamDeckClearSettingsEventHandler.GetInvocationList().Length.ToString() : "0").Append("\n");
            stringBuilder.Append("OnStreamDeckSyncConfigurationEventHandler :").Append(OnStreamDeckSyncConfigurationEventHandler != null ? OnStreamDeckSyncConfigurationEventHandler.GetInvocationList().Length.ToString() : "0").Append("\n");
            stringBuilder.Append("OnStreamDeckConfigurationChangeEventHandler :").Append(OnStreamDeckConfigurationChangeEventHandler != null ? OnStreamDeckConfigurationChangeEventHandler.GetInvocationList().Length.ToString() : "0").Append("\n");
            stringBuilder.Append("OnStreamDeckHideDecodersEventHandler :").Append(OnStreamDeckHideDecodersEventHandler != null ? OnStreamDeckHideDecodersEventHandler.GetInvocationList().Length.ToString() : "0").Append("\n");

            return stringBuilder.ToString();
        }

        public static void AttachStreamDeckListener(INvStreamDeckListener streamDeckListener)
        {
            OnDirtyConfigurationsEventHandler += streamDeckListener.IsDirtyQueryReport;
            OnDirtyNotificationEventHandler += streamDeckListener.SenderIsDirtyNotification;
            OnStreamDeckShowNewLayerEventHandler += streamDeckListener.LayerSwitched;
            OnRemoteStreamDeckShowNewLayerEventHandler += streamDeckListener.RemoteLayerSwitch;
            OnStreamDeckSelectedButtonChangedEventHandler += streamDeckListener.SelectedButtonChanged;
            OnStreamDeckClearSettingsEventHandler += streamDeckListener.ClearSettings;
        }

        public static void DetachStreamDeckListener(INvStreamDeckListener streamDeckListener)
        {
            OnDirtyConfigurationsEventHandler -= streamDeckListener.IsDirtyQueryReport;
            OnDirtyNotificationEventHandler -= streamDeckListener.SenderIsDirtyNotification;
            OnStreamDeckShowNewLayerEventHandler -= streamDeckListener.LayerSwitched;
            OnStreamDeckSelectedButtonChangedEventHandler -= streamDeckListener.SelectedButtonChanged;
            OnStreamDeckClearSettingsEventHandler -= streamDeckListener.ClearSettings;
        }

        /********************************************************************************************
        *                    Streamdeck Querying whether there are unsaved configurations
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
                    return true; // There are dirty listeners out there
                }
            }

            return false;
        }

        /********************************************************************************************
        *                    Streamdeck UserControl Pro-actively reports it is dirty
        ********************************************************************************************/
        public delegate void DirtyNotificationEventHandler(object sender, StreamDeckDirtyNotificationArgs e);
        public static event DirtyNotificationEventHandler OnDirtyNotificationEventHandler;

        public static void SenderNotifiesIsDirty(object sender, EnumStreamDeckButtonNames buttonName, string layerName, string bindingHash)
        {
            var eventArguments = new StreamDeckDirtyNotificationArgs
            {
                BindingHash = bindingHash,
                ButtonName = buttonName,
                LayerName = layerName
            };

            OnDirtyNotificationEventHandler?.Invoke(sender, eventArguments);
        }


        /********************************************************************************************
         *                                      Streamdeck Layer switched
         ********************************************************************************************/
        public delegate void StreamDeckShowNewLayerEventHandler(object sender, StreamDeckShowNewLayerArgs e);
        public static event StreamDeckShowNewLayerEventHandler OnStreamDeckShowNewLayerEventHandler;

        public static void LayerSwitched(object sender, string bindingHash, string layerName)
        {
            var eventArgs = new StreamDeckShowNewLayerArgs { SelectedLayerName = layerName, BindingHash = bindingHash };
            OnStreamDeckShowNewLayerEventHandler?.Invoke(sender, eventArgs);
        }

        /********************************************************************************************
         *                                      Remote Streamdeck layer switch
         ********************************************************************************************/
        public delegate void RemoteStreamDeckShowNewLayerEventHandler(object sender, RemoteStreamDeckShowNewLayerArgs e);
        public static event RemoteStreamDeckShowNewLayerEventHandler OnRemoteStreamDeckShowNewLayerEventHandler;

        public static void RemoteLayerSwitch(object sender, string remoteBindingHash, string layerName)
        {
            var eventArgs = new RemoteStreamDeckShowNewLayerArgs { SelectedLayerName = layerName, RemoteBindingHash = remoteBindingHash };
            OnRemoteStreamDeckShowNewLayerEventHandler?.Invoke(sender, eventArgs);
        }

        /********************************************************************************************
         *                                      Streamdeck button change
         ********************************************************************************************/
        public delegate void StreamDeckSelectedButtonChangedEventHandler(object sender, StreamDeckSelectedButtonChangedArgs e);
        public static event StreamDeckSelectedButtonChangedEventHandler OnStreamDeckSelectedButtonChangedEventHandler;

        public static void SelectedButtonChanged(object sender, StreamDeckButton streamDeckButton, string bindingHash)
        {
            var eventArgs = new StreamDeckSelectedButtonChangedArgs { SelectedButton = streamDeckButton, BindingHash = bindingHash };
            OnStreamDeckSelectedButtonChangedEventHandler?.Invoke(sender, eventArgs);
        }


        /********************************************************************************************
         *                                      Streamdeck clear all settings
         ********************************************************************************************/
        public delegate void StreamDeckClearSettingsEventHandler(object sender, StreamDeckClearSettingsArgs e);
        public static event StreamDeckClearSettingsEventHandler OnStreamDeckClearSettingsEventHandler;

        public static void ClearSettings(object sender, bool clearAction, bool clearFace, bool clearUI, string bindingHash)
        {
            var newEvent = new StreamDeckClearSettingsArgs
            {
                BindingHash = bindingHash,
                ClearActionConfiguration = clearAction,
                ClearFaceConfiguration = clearFace,
                ClearUIConfiguration = clearUI
            };

            OnStreamDeckClearSettingsEventHandler?.Invoke(sender, newEvent);
        }


        /********************************************************************************************
        *                   Streamdeck Event to notify listener to sync configuration
        ********************************************************************************************/
        public delegate void StreamDeckSyncConfigurationEventHandler(object sender, StreamDeckSyncConfigurationArgs e);
        public static event StreamDeckSyncConfigurationEventHandler OnStreamDeckSyncConfigurationEventHandler;

        public static void NotifyToSyncConfiguration(object sender, string bindingHash)
        {
            OnStreamDeckSyncConfigurationEventHandler?.Invoke(sender, new StreamDeckSyncConfigurationArgs { BindingHash = bindingHash });
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
        *         Streamdeck Event to notify changes in Stream Deck configuration button/layer
        ********************************************************************************************/
        public delegate void StreamDeckConfigurationChangeEventHandler(object sender, StreamDeckConfigurationChangedArgs e);
        public static event StreamDeckConfigurationChangeEventHandler OnStreamDeckConfigurationChangeEventHandler;

        public static void NotifyStreamDeckConfigurationChange(object sender, string bindingHash)
        {
            OnStreamDeckConfigurationChangeEventHandler?.Invoke(sender, new StreamDeckConfigurationChangedArgs { BindingHash = bindingHash });
        }

        /********************************************************************************************
        * Event to notify DCSBIOSDecoders to go invisible (ugly workaround when I can't get handle of the various threads)
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

        public static void HideDCSBIOSDecoders(DCSBIOSDecoder dcsbiosDecoder, string layerName, string bindingHash)
        {
            var eventArgs = new StreamDeckHideDecoderEventArgs
            {
                BindingHash = bindingHash,
                StreamDeckButtonName = dcsbiosDecoder.StreamDeckButtonName,
                LayerName = layerName
            };
            OnStreamDeckHideDecodersEventHandler?.Invoke(dcsbiosDecoder, eventArgs);
        }


    }

}
