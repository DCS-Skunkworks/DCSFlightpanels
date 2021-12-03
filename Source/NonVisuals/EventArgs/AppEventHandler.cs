using System.Collections.Generic;
using ClassLibraryCommon;
using NonVisuals.Interfaces;
using NonVisuals.Saitek;

namespace NonVisuals.EventArgs
{
    public static class AppEventHandler
    {

        /*
         * Used by ProfileHandler to detect changes in panel configurations.
         */
        public delegate void SettingsHasModifiedEventHandler(object sender, PanelEventArgs e);

        public static event SettingsHasModifiedEventHandler OnSettingsModified;
        public static void SettingsChanged(object sender, string hidInstanceId, GamingPanelEnum gamingPanelEnum)
        {
            OnSettingsModified?.Invoke(sender, new PanelEventArgs { HidInstance = hidInstanceId, PanelType = gamingPanelEnum });
        }

        public static void AttachSettingsModified(ISettingsModifiedListener settingsModifiedListener)
        {
            OnSettingsModified += settingsModifiedListener.SettingsModified;
        }

        public static void DetachSettingsModified(ISettingsModifiedListener settingsModifiedListener)
        {
            OnSettingsModified -= settingsModifiedListener.SettingsModified;
        }

        /*
         * Done from MainWindow
         * Used when user disables action (e.g. key press) forwarding so that panels knows not to do the actual action
         */

        public delegate void ForwardPanelActionChangedEventHandler(object sender, ForwardPanelEventArgs e);

        public static event ForwardPanelActionChangedEventHandler OnForwardPanelEventChanged;

        public static void ForwardKeyPressEvent(object sender, bool doForwardActions)
        {
            OnForwardPanelEventChanged?.Invoke(sender, new ForwardPanelEventArgs() { Forward = doForwardActions });
        }

        public static void AttachForwardPanelEventListener(GamingPanel gamingPanel)
        {
            OnForwardPanelEventChanged += gamingPanel.SetForwardPanelEvent;
        }

        public static void DetachForwardPanelEventListener(GamingPanel gamingPanel)
        {
            OnForwardPanelEventChanged -= gamingPanel.SetForwardPanelEvent;
        }



        /*
            ____             _____ __     __  __                ____             __       _                                __                        __      
           / __ \_________  / __(_) /__  / / / /___ _____  ____/ / /__  _____   / /______(_)___ _____ ____  ________  ____/ /  ___ _   _____  ____  / /______
          / /_/ / ___/ __ \/ /_/ / / _ \/ /_/ / __ `/ __ \/ __  / / _ \/ ___/  / __/ ___/ / __ `/ __ `/ _ \/ ___/ _ \/ __  /  / _ \ | / / _ \/ __ \/ __/ ___/
         / ____/ /  / /_/ / __/ / /  __/ __  / /_/ / / / / /_/ / /  __/ /     / /_/ /  / / /_/ / /_/ /  __/ /  /  __/ /_/ /  /  __/ |/ /  __/ / / / /_(__  ) 
        /_/   /_/   \____/_/ /_/_/\___/_/ /_/\__,_/_/ /_/\__,_/_/\___/_/      \__/_/  /_/\__, /\__, /\___/_/   \___/\__,_/   \___/|___/\___/_/ /_/\__/____/  
                                                                                /____//____/                                                         
         */
        public delegate void ProfileReadFromFileEventHandler(object sender, PanelBindingReadFromFileEventArgs e);

        public static event ProfileReadFromFileEventHandler OnSettingsReadFromFile;

        public static void SettingsReadFromFile(object sender, GenericPanelBinding genericPanelBinding)
        {
            OnSettingsReadFromFile?.Invoke(sender, new PanelBindingReadFromFileEventArgs { PanelBinding = genericPanelBinding });
        }

        public delegate void SavePanelSettingsEventHandler(object sender, ProfileHandlerEventArgs e);

        public static event SavePanelSettingsEventHandler OnSavePanelSettings;

        public static void SavePanelSettings(ProfileHandler profileHandler)
        {
            OnSavePanelSettings?.Invoke(profileHandler, new ProfileHandlerEventArgs { ProfileHandlerCaller = profileHandler });
        }

        public delegate void SavePanelSettingsEventHandlerJSON(object sender, ProfileHandlerEventArgs e);

        public static event SavePanelSettingsEventHandlerJSON OnSavePanelSettingsJSON;

        public static void SavePanelSettingsJSON(ProfileHandler profileHandler)
        {
            OnSavePanelSettingsJSON?.Invoke(profileHandler, new ProfileHandlerEventArgs { ProfileHandlerCaller = profileHandler });
        }

        public delegate void AirframeSelectedEventHandler(object sender, AirframeEventArgs e);

        public static event AirframeSelectedEventHandler OnAirframeSelected;

        public static void AirframeSelected(object sender, DCSFPProfile dcsfpProfile)
        {
            OnAirframeSelected?.Invoke(sender, new AirframeEventArgs { Profile = dcsfpProfile });
        }

        public delegate void ClearPanelSettingsEventHandler(object sender);

        public static event ClearPanelSettingsEventHandler OnClearPanelSettings;

        public static void ClearPanelSettings(object sender)
        {
            OnClearPanelSettings?.Invoke(sender);
        }

        public static void AttachSettingsConsumerListener(GamingPanel gamingPanel)
        {
            OnSettingsReadFromFile += gamingPanel.PanelBindingReadFromFile;
            OnSavePanelSettings += gamingPanel.SavePanelSettings;
            OnSavePanelSettingsJSON += gamingPanel.SavePanelSettingsJSON;
            OnClearPanelSettings += gamingPanel.ClearPanelSettings;
            OnAirframeSelected += gamingPanel.ProfileSelected;
        }

        public static void DetachSettingsConsumerListener(GamingPanel gamingPanel)
        {
            OnSettingsReadFromFile -= gamingPanel.PanelBindingReadFromFile;
            OnSavePanelSettings -= gamingPanel.SavePanelSettings;
            OnSavePanelSettingsJSON -= gamingPanel.SavePanelSettingsJSON;
            OnClearPanelSettings -= gamingPanel.ClearPanelSettings;
            OnAirframeSelected -= gamingPanel.ProfileSelected;
        }

        public static void AttachSettingsMonitoringListener(IProfileHandlerListener gamingPanelSettingsListener)
        {
            OnSettingsReadFromFile += gamingPanelSettingsListener.PanelBindingReadFromFile;
            OnAirframeSelected += gamingPanelSettingsListener.ProfileSelected;
        }

        public static void DetachSettingsMonitoringListener(IProfileHandlerListener gamingPanelSettingsListener)
        {
            OnSettingsReadFromFile -= gamingPanelSettingsListener.PanelBindingReadFromFile;
            OnAirframeSelected -= gamingPanelSettingsListener.ProfileSelected;
        }



        /*
           ______                   _                ____                       __   __         _                                      __                            __       
          / ____/____ _ ____ ___   (_)____   ____ _ / __ \ ____ _ ____   ___   / /  / /_ _____ (_)____ _ ____ _ ___   _____ ___   ____/ /  ___  _   __ ___   ____   / /_ _____
         / / __ / __ `// __ `__ \ / // __ \ / __ `// /_/ // __ `// __ \ / _ \ / /  / __// ___// // __ `// __ `// _ \ / ___// _ \ / __  /  / _ \| | / // _ \ / __ \ / __// ___/
        / /_/ // /_/ // / / / / // // / / // /_/ // ____// /_/ // / / //  __// /  / /_ / /   / // /_/ // /_/ //  __// /   /  __// /_/ /  /  __/| |/ //  __// / / // /_ (__  ) 
        \____/ \__,_//_/ /_/ /_//_//_/ /_/ \__, //_/     \__,_//_/ /_/ \___//_/   \__//_/   /_/ \__, / \__, / \___//_/    \___/ \__,_/   \___/ |___/ \___//_/ /_/ \__//____/  
                                          /____/                                               /____/ /____/                                                                  
        */

        /*
         * Used by UserControls to show switches that has been manipulated.
         * Shows the actions in the Log textbox of the UserControl.
         */
        public delegate void SwitchesHasBeenChangedEventHandler(object sender, SwitchesChangedEventArgs e);

        public static event SwitchesHasBeenChangedEventHandler OnSwitchesChangedA;

        public static void SwitchesChanged(object sender, string hidInstanceId, GamingPanelEnum gamingPanelEnum, HashSet<object> hashSet)
        {
            OnSwitchesChangedA?.Invoke(sender, new SwitchesChangedEventArgs { HidInstance = hidInstanceId, PanelType = gamingPanelEnum, Switches = hashSet });
        }

        /*
         * Used for notifying when a device has been attached.
         * Not used atm.
         */
        public delegate void DeviceAttachedEventHandler(object sender, PanelEventArgs e);

        public static event DeviceAttachedEventHandler OnDeviceAttachedA;
        public static void DeviceAttached(object sender, string hidInstanceId, GamingPanelEnum gamingPanelEnum)
        {
            // IsAttached = true;
            OnDeviceAttachedA?.Invoke(sender, new PanelEventArgs { HidInstance = hidInstanceId, PanelType = gamingPanelEnum });
        }


        /*
         * Used for notifying when a device has been detached.
         * Not used atm.
         */
        public delegate void DeviceDetachedEventHandler(object sender, PanelEventArgs e);

        public static event DeviceDetachedEventHandler OnDeviceDetachedA;
        public static void DeviceDetached(object sender, string hidInstanceId, GamingPanelEnum gamingPanelEnum)
        {
            // IsAttached = false;
            OnDeviceDetachedA?.Invoke(sender, new PanelEventArgs { HidInstance = hidInstanceId, PanelType = gamingPanelEnum });
        }

        /*
         * Used by some UserControls to know when panels have loaded their configurations.
         * Used by MainWindow to SetFormstate().
         */
        public delegate void SettingsHasBeenAppliedEventHandler(object sender, PanelEventArgs e);

        public static event SettingsHasBeenAppliedEventHandler OnSettingsAppliedA;
        public static void SettingsApplied(object sender, string hidInstanceId, GamingPanelEnum gamingPanelEnum)
        {
            OnSettingsAppliedA?.Invoke(sender, new PanelEventArgs { HidInstance = hidInstanceId, PanelType = gamingPanelEnum });
        }


        /*
         * Used by some UserControls refresh UI when panel has cleared all its settings.
         */
        /*public delegate void SettingsClearedEventHandler(object sender, PanelEventArgs e);

        public static event SettingsClearedEventHandler OnSettingsClearedA;
        public static void SettingsCleared(object sender, string hidInstanceId, GamingPanelEnum gamingPanelEnum)
        {
            OnSettingsClearedA?.Invoke(sender, new PanelEventArgs { HidInstance = hidInstanceId, PanelType = gamingPanelEnum });
        }*/


        /*
         * DCS-BIOS has a feature to detect if any updates has been missed.
         * It is not used as such since DCS-BIOS has been working so well.
         */
        public delegate void UpdatesHasBeenMissedEventHandler(object sender, DCSBIOSUpdatesMissedEventArgs e);

        public static event UpdatesHasBeenMissedEventHandler OnUpdatesHasBeenMissed;

        public static void UpdatesMissed(object sender, string hidInstanceId, GamingPanelEnum gamingPanelEnum, int missedUpdateCount)
        {
            OnUpdatesHasBeenMissed?.Invoke(
                sender,
                new DCSBIOSUpdatesMissedEventArgs { HidInstance = hidInstanceId, GamingPanelEnum = gamingPanelEnum, Count = missedUpdateCount });
        }

        // For those that wants to listen to this panel
        public static void AttachGamingPanelListener(IGamingPanelListener gamingPanelListener)
        {
            OnDeviceAttachedA += gamingPanelListener.DeviceAttached;
            OnSwitchesChangedA += gamingPanelListener.SwitchesChanged;
            OnSettingsAppliedA += gamingPanelListener.SettingsApplied;
            //OnSettingsClearedA += gamingPanelListener.SettingsCleared;
            OnSettingsModified += gamingPanelListener.SettingsModified;
            OnUpdatesHasBeenMissed += gamingPanelListener.UpdatesHasBeenMissed;
        }
        
        public static void DetachGamingPanelListener(IGamingPanelListener gamingPanelListener)
        {
            OnDeviceAttachedA -= gamingPanelListener.DeviceAttached;
            OnSwitchesChangedA -= gamingPanelListener.SwitchesChanged;
            OnSettingsAppliedA -= gamingPanelListener.SettingsApplied;
            //OnSettingsClearedA -= gamingPanelListener.SettingsCleared;
            OnSettingsModified -= gamingPanelListener.SettingsModified;
            OnUpdatesHasBeenMissed -= gamingPanelListener.UpdatesHasBeenMissed;
        }



        /*
         * Used by those UserControls who's panels can show LED lights.
         * Used to show the same color in the UserControl as the physical panels.
         */
        public delegate void LedLightChangedEventHandler(object sender, LedLightChangeEventArgs e);

        public static event LedLightChangedEventHandler OnLedLightChangedA;

        public static void LedLightChanged(object sender, string hidInstanceId, SaitekPanelLEDPosition saitekPanelLEDPosition, PanelLEDColor panelLEDColor)
        {
            OnLedLightChangedA?.Invoke(sender, new LedLightChangeEventArgs { UniqueId = hidInstanceId, LEDPosition = saitekPanelLEDPosition, LEDColor = panelLEDColor });
        }

        public static void AttachLEDLightListener(IGamingPanelListener gamingPanelListener)
        {
            OnLedLightChangedA += gamingPanelListener.LedLightChanged;
        }

        public static void DetachLEDLightListener(IGamingPanelListener gamingPanelListener)
        {
            OnLedLightChangedA -= gamingPanelListener.LedLightChanged;
        }
    }
}
