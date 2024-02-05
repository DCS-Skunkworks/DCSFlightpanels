﻿using System.Collections.Generic;
using ClassLibraryCommon;
using NonVisuals.HID;
using NonVisuals.Interfaces;
using NonVisuals.Panels;
using NonVisuals.Panels.Saitek;

namespace NonVisuals.EventArgs
{
    public static class AppEventHandler
    {
        /*
         * Used by by HIDHandler and others to announce a certain event regarding a panel. 
         */
        public delegate void PanelEventHandler(object sender, PanelEventArgs e);
        public static event PanelEventHandler OnPanelEvent;

        public static bool OnPanelEventEventSubscribed()
        {
            return OnPanelEvent != null && OnPanelEvent.GetInvocationList().Length > 0;
        }

        public static void PanelEvent(object sender, string hidInstance, HIDSkeleton hidSkeleton, PanelEventType panelEventType)
        {
            OnPanelEvent?.Invoke(sender, new PanelEventArgs { HidInstance = hidInstance, HidSkeleton = hidSkeleton, EventType = panelEventType});
        }

        public static void AttachPanelEventListener(IPanelEventListener panelEventListener)
        {
            OnPanelEvent += panelEventListener.PanelEvent;
        }

        public static void DetachPanelEventListener(IPanelEventListener panelEventListener)
        {
            OnPanelEvent -= panelEventListener.PanelEvent;
        }

        /*
         * Used by ProfileHandler to detect changes in panel configurations.
         */
        public delegate void SettingsHasModifiedEventHandler(object sender, PanelInfoArgs e);
        public static event SettingsHasModifiedEventHandler OnSettingsModified;

        public static bool OnSettingsModifiedEventSubscribed()
        {
            return OnSettingsModified != null && OnSettingsModified.GetInvocationList().Length > 0;
        }

        public static void SettingsChanged(object sender, string hidInstance, GamingPanelEnum gamingPanelEnum)
        {
            OnSettingsModified?.Invoke(sender, new PanelInfoArgs { HidInstance = hidInstance, PanelType = gamingPanelEnum });
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

        public static bool OnForwardPanelEventChangedSubscribed()
        {
            return OnForwardPanelEventChanged != null && OnForwardPanelEventChanged.GetInvocationList().Length > 0;
        }


        public static void ForwardKeyPressEvent(object sender, bool doForwardActions)
        {
            OnForwardPanelEventChanged?.Invoke(sender, new ForwardPanelEventArgs { Forward = doForwardActions });
        }

        public static void AttachForwardPanelEventListener(GamingPanel gamingPanel)
        {
            OnForwardPanelEventChanged += gamingPanel.SetForwardPanelEvent;
        }

        public static void DetachForwardPanelEventListener(GamingPanel gamingPanel)
        {
            OnForwardPanelEventChanged -= gamingPanel.SetForwardPanelEvent;
        }

        public static void AttachForwardPanelEventListener(IForwardPanelEventListener forwardPanelEventListener)
        {
            OnForwardPanelEventChanged += forwardPanelEventListener.SetForwardPanelEvent;
        }

        public static void DetachForwardPanelEventListener(IForwardPanelEventListener forwardPanelEventListener)
        {
            OnForwardPanelEventChanged -= forwardPanelEventListener.SetForwardPanelEvent;
        }

        /*
            ____             _____ __     __  __                ____             __       _                                __                        __      
           / __ \_________  / __(_) /__  / / / /___ _____  ____/ / /__  _____   / /______(_)___ _____ ____  ________  ____/ /  ___ _   _____  ____  / /______
          / /_/ / ___/ __ \/ /_/ / / _ \/ /_/ / __ `/ __ \/ __  / / _ \/ ___/  / __/ ___/ / __ `/ __ `/ _ \/ ___/ _ \/ __  /  / _ \ | / / _ \/ __ \/ __/ ___/
         / ____/ /  / /_/ / __/ / /  __/ __  / /_/ / / / / /_/ / /  __/ /     / /_/ /  / / /_/ / /_/ /  __/ /  /  __/ /_/ /  /  __/ |/ /  __/ / / / /_(__  ) 
        /_/   /_/   \____/_/ /_/_/\___/_/ /_/\__,_/_/ /_/\__,_/_/\___/_/      \__/_/  /_/\__, /\__, /\___/_/   \___/\__,_/   \___/|___/\___/_/ /_/\__/____/  
                                                                                /____//____/                                                         
         */

        public static void AttachSettingsConsumerListener(GamingPanel gamingPanel)
        {
            OnProfileEvent += gamingPanel.ProfileEvent;
            OnSavePanelSettings += gamingPanel.SavePanelSettings;
            OnSavePanelSettingsJSON += gamingPanel.SavePanelSettingsJSON;
        }

        public static void DetachSettingsConsumerListener(GamingPanel gamingPanel)
        {
            OnProfileEvent -= gamingPanel.ProfileEvent;
            OnSavePanelSettings -= gamingPanel.SavePanelSettings;
            OnSavePanelSettingsJSON -= gamingPanel.SavePanelSettingsJSON;
        }

        public delegate void ProfileEventHandler(object sender, ProfileEventArgs e);
        public static event ProfileEventHandler OnProfileEvent;

        public static bool OnProfileEventSubscribed()
        {
            return OnProfileEvent != null && OnProfileEvent.GetInvocationList().Length > 0;
        }

        public static void ProfileEvent(object sender, ProfileEventEnum profileEventType, GenericPanelBinding genericPanelBinding, DCSAircraft dcsAircraft)
        {
            OnProfileEvent?.Invoke(sender, new ProfileEventArgs { PanelBinding = genericPanelBinding, ProfileEventType = profileEventType, DCSAirCraft = dcsAircraft});
        }
        
        public delegate void SavePanelSettingsEventHandler(object sender, ProfileHandlerEventArgs e);
        public static event SavePanelSettingsEventHandler OnSavePanelSettings;

        public static bool OnSavePanelSettingsSubscribed()
        {
            return OnSavePanelSettings != null && OnSavePanelSettings.GetInvocationList().Length > 0;
        }

        public static void SavePanelSettings(IProfileHandler profileHandler)
        {
            OnSavePanelSettings?.Invoke(profileHandler, new ProfileHandlerEventArgs { ProfileHandlerCaller = profileHandler });
        }

        public delegate void SavePanelSettingsEventHandlerJSON(object sender, ProfileHandlerEventArgs e);
        public static event SavePanelSettingsEventHandlerJSON OnSavePanelSettingsJSON;

        public static bool OnSavePanelSettingsJSONSubscribed()
        {
            return OnSavePanelSettingsJSON != null && OnSavePanelSettingsJSON.GetInvocationList().Length > 0;
        }

        public static void SavePanelSettingsJSON(IProfileHandler profileHandler)
        {
            OnSavePanelSettingsJSON?.Invoke(profileHandler, new ProfileHandlerEventArgs { ProfileHandlerCaller = profileHandler });
        }

        public static void AttachSettingsMonitoringListener(IProfileHandlerListener profileHandlerListener)
        {
            OnProfileEvent += profileHandlerListener.ProfileEvent;
        }

        public static void DetachSettingsMonitoringListener(IProfileHandlerListener profileHandlerListener)
        {
            OnProfileEvent -= profileHandlerListener.ProfileEvent;
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

        public static void SwitchesChanged(object sender, string hidInstance, GamingPanelEnum gamingPanelEnum, HashSet<object> hashSet)
        {
            OnSwitchesChangedA?.Invoke(sender, new SwitchesChangedEventArgs { HidInstance = hidInstance, PanelType = gamingPanelEnum, Switches = hashSet });
        }
        
        /*
         * Used by some UserControls to know when panels have loaded their configurations.
         * Used by MainWindow to SetFormState().
         */
        public delegate void SettingsHasBeenAppliedEventHandler(object sender, PanelInfoArgs e);

        public static event SettingsHasBeenAppliedEventHandler OnSettingsAppliedA;
        public static void SettingsApplied(object sender, string hidInstance, GamingPanelEnum gamingPanelEnum)
        {
            OnSettingsAppliedA?.Invoke(sender, new PanelInfoArgs { HidInstance = hidInstance, PanelType = gamingPanelEnum });
        }

        /*
         * DCS-BIOS has a feature to detect if any updates has been missed.
         * It is not used as such since DCS-BIOS has been working so well.
         */
        public delegate void UpdatesHasBeenMissedEventHandler(object sender, DCSBIOSUpdatesMissedEventArgs e);

        public static event UpdatesHasBeenMissedEventHandler OnUpdatesHasBeenMissed;

        public static void UpdatesMissed(object sender, string hidInstance, GamingPanelEnum gamingPanelEnum, int missedUpdateCount)
        {
            OnUpdatesHasBeenMissed?.Invoke(
                sender,
                new DCSBIOSUpdatesMissedEventArgs { HidInstance = hidInstance, GamingPanelEnum = gamingPanelEnum, Count = missedUpdateCount });
        }

        // For those that wants to listen to this panel
        public static void AttachGamingPanelListener(IGamingPanelListener gamingPanelListener)
        {
            OnSwitchesChangedA += gamingPanelListener.SwitchesChanged;
            OnSettingsAppliedA += gamingPanelListener.SettingsApplied;
            OnSettingsModified += gamingPanelListener.SettingsModified;
            OnUpdatesHasBeenMissed += gamingPanelListener.UpdatesHasBeenMissed;
        }
        
        public static void DetachGamingPanelListener(IGamingPanelListener gamingPanelListener)
        {
            OnSwitchesChangedA -= gamingPanelListener.SwitchesChanged;
            OnSettingsAppliedA -= gamingPanelListener.SettingsApplied;
            OnSettingsModified -= gamingPanelListener.SettingsModified;
            OnUpdatesHasBeenMissed -= gamingPanelListener.UpdatesHasBeenMissed;
        }



        /*
         * Used by those UserControls who's panels can show LED lights.
         * Used to show the same color in the UserControl as the physical panels.
         */
        public delegate void LedLightChangedEventHandler(object sender, LedLightChangeEventArgs e);

        public static event LedLightChangedEventHandler OnLedLightChangedA;

        public static void LedLightChanged(object sender, string hidInstance, SaitekPanelLEDPosition saitekPanelLEDPosition, PanelLEDColor panelLEDColor)
        {
            OnLedLightChangedA?.Invoke(sender, new LedLightChangeEventArgs { HIDInstance = hidInstance, LEDPosition = saitekPanelLEDPosition, LEDColor = panelLEDColor });
        }
        
        public static void AttachLEDLightListener(ILedLightPanelListener ledLightPanelListener)
        {
            OnLedLightChangedA += ledLightPanelListener.LedLightChanged;
        }

        public static void DetachLEDLightListener(ILedLightPanelListener ledLightPanelListener)
        {
            OnLedLightChangedA -= ledLightPanelListener.LedLightChanged;
        }
    }
}
