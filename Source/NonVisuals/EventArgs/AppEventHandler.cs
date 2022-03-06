using System.Collections.Generic;
using ClassLibraryCommon;
using NonVisuals.Interfaces;
using NonVisuals.Saitek;

namespace NonVisuals.EventArgs
{
    public class AppEventHandler
    {
        /*
         * Used by by HIDHandler and others to announce a certain event regarding a panel. 
         */
        public delegate void PanelEventHandler(object sender, PanelEventArgs e);
        public event PanelEventHandler OnPanelEvent;

        public bool OnPanelEventEventSubscribed()
        {
            return OnPanelEvent != null && OnPanelEvent.GetInvocationList().Length > 0;
        }

        public void PanelEvent(object sender, string hidInstance, HIDSkeleton hidSkeleton, PanelEventType panelEventType)
        {
            OnPanelEvent?.Invoke(sender, new PanelEventArgs { HidInstance = hidInstance, HidSkeleton = hidSkeleton, EventType = panelEventType});
        }

        public void AttachPanelEventListener(IPanelEventListener panelEventListener)
        {
            OnPanelEvent += panelEventListener.PanelEvent;
        }

        public void DetachPanelEventListener(IPanelEventListener panelEventListener)
        {
            OnPanelEvent -= panelEventListener.PanelEvent;
        }

        /*
         * Used by ProfileHandler to detect changes in panel configurations.
         */
        public delegate void SettingsHasModifiedEventHandler(object sender, PanelInfoArgs e);
        public event SettingsHasModifiedEventHandler OnSettingsModified;

        public bool OnSettingsModifiedEventSubscribed()
        {
            return OnSettingsModified != null && OnSettingsModified.GetInvocationList().Length > 0;
        }

        public void SettingsChanged(object sender, string hidInstance, GamingPanelEnum gamingPanelEnum)
        {
            OnSettingsModified?.Invoke(sender, new PanelInfoArgs { HidInstance = hidInstance, PanelType = gamingPanelEnum });
        }

        public void AttachSettingsModified(ISettingsModifiedListener settingsModifiedListener)
        {
            OnSettingsModified += settingsModifiedListener.SettingsModified;
        }

        public void DetachSettingsModified(ISettingsModifiedListener settingsModifiedListener)
        {
            OnSettingsModified -= settingsModifiedListener.SettingsModified;
        }

        /*
         * Done from MainWindow
         * Used when user disables action (e.g. key press) forwarding so that panels knows not to do the actual action
         */

        public delegate void ForwardPanelActionChangedEventHandler(object sender, ForwardPanelEventArgs e);
        public event ForwardPanelActionChangedEventHandler OnForwardPanelEventChanged;

        public bool OnForwardPanelEventChangedSubscribed()
        {
            return OnForwardPanelEventChanged != null && OnForwardPanelEventChanged.GetInvocationList().Length > 0;
        }


        public void ForwardKeyPressEvent(object sender, bool doForwardActions)
        {
            OnForwardPanelEventChanged?.Invoke(sender, new ForwardPanelEventArgs() { Forward = doForwardActions });
        }

        public void AttachForwardPanelEventListener(GamingPanel gamingPanel)
        {
            OnForwardPanelEventChanged += gamingPanel.SetForwardPanelEvent;
        }

        public void DetachForwardPanelEventListener(GamingPanel gamingPanel)
        {
            OnForwardPanelEventChanged -= gamingPanel.SetForwardPanelEvent;
        }

        public void AttachForwardPanelEventListener(IForwardPanelEventListener forwardPanelEventListener)
        {
            OnForwardPanelEventChanged += forwardPanelEventListener.SetForwardPanelEvent;
        }

        public void DetachForwardPanelEventListener(IForwardPanelEventListener forwardPanelEventListener)
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

        public void AttachSettingsConsumerListener(GamingPanel gamingPanel)
        {
            OnProfileEvent += gamingPanel.ProfileEvent;
            OnSavePanelSettings += gamingPanel.SavePanelSettings;
            OnSavePanelSettingsJSON += gamingPanel.SavePanelSettingsJSON;
        }

        public void DetachSettingsConsumerListener(GamingPanel gamingPanel)
        {
            OnProfileEvent -= gamingPanel.ProfileEvent;
            OnSavePanelSettings -= gamingPanel.SavePanelSettings;
            OnSavePanelSettingsJSON -= gamingPanel.SavePanelSettingsJSON;
        }

        public delegate void ProfileEventHandler(object sender, ProfileEventArgs e);
        public event ProfileEventHandler OnProfileEvent;

        public bool OnProfileEventSubscribed()
        {
            return OnProfileEvent != null && OnProfileEvent.GetInvocationList().Length > 0;
        }

        public void ProfileEvent(object sender, ProfileEventEnum profileEventType, GenericPanelBinding genericPanelBinding, DCSFPProfile dcsfpProfile)
        {
            OnProfileEvent?.Invoke(sender, new ProfileEventArgs { PanelBinding = genericPanelBinding, ProfileEventType = profileEventType, DCSProfile = dcsfpProfile});
        }
        
        public delegate void SavePanelSettingsEventHandler(object sender, ProfileHandlerEventArgs e);
        public event SavePanelSettingsEventHandler OnSavePanelSettings;

        public bool OnSavePanelSettingsSubscribed()
        {
            return OnSavePanelSettings != null && OnSavePanelSettings.GetInvocationList().Length > 0;
        }

        public void SavePanelSettings(IProfileHandler profileHandler)
        {
            OnSavePanelSettings?.Invoke(profileHandler, new ProfileHandlerEventArgs { ProfileHandlerCaller = profileHandler });
        }

        public delegate void SavePanelSettingsEventHandlerJSON(object sender, ProfileHandlerEventArgs e);
        public event SavePanelSettingsEventHandlerJSON OnSavePanelSettingsJSON;

        public bool OnSavePanelSettingsJSONSubscribed()
        {
            return OnSavePanelSettingsJSON != null && OnSavePanelSettingsJSON.GetInvocationList().Length > 0;
        }

        public void SavePanelSettingsJSON(IProfileHandler profileHandler)
        {
            OnSavePanelSettingsJSON?.Invoke(profileHandler, new ProfileHandlerEventArgs { ProfileHandlerCaller = profileHandler });
        }

        public void AttachSettingsMonitoringListener(IProfileHandlerListener profileHandlerListener)
        {
            OnProfileEvent += profileHandlerListener.ProfileEvent;
        }

        public void DetachSettingsMonitoringListener(IProfileHandlerListener profileHandlerListener)
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

        public event SwitchesHasBeenChangedEventHandler OnSwitchesChangedA;

        public void SwitchesChanged(object sender, string hidInstance, GamingPanelEnum gamingPanelEnum, HashSet<object> hashSet)
        {
            OnSwitchesChangedA?.Invoke(sender, new SwitchesChangedEventArgs { HidInstance = hidInstance, PanelType = gamingPanelEnum, Switches = hashSet });
        }
        
        /*
         * Used by some UserControls to know when panels have loaded their configurations.
         * Used by MainWindow to SetFormState().
         */
        public delegate void SettingsHasBeenAppliedEventHandler(object sender, PanelInfoArgs e);

        public event SettingsHasBeenAppliedEventHandler OnSettingsAppliedA;
        public void SettingsApplied(object sender, string hidInstance, GamingPanelEnum gamingPanelEnum)
        {
            OnSettingsAppliedA?.Invoke(sender, new PanelInfoArgs { HidInstance = hidInstance, PanelType = gamingPanelEnum });
        }

        /*
         * DCS-BIOS has a feature to detect if any updates has been missed.
         * It is not used as such since DCS-BIOS has been working so well.
         */
        public delegate void UpdatesHasBeenMissedEventHandler(object sender, DCSBIOSUpdatesMissedEventArgs e);

        public event UpdatesHasBeenMissedEventHandler OnUpdatesHasBeenMissed;

        public void UpdatesMissed(object sender, string hidInstance, GamingPanelEnum gamingPanelEnum, int missedUpdateCount)
        {
            OnUpdatesHasBeenMissed?.Invoke(
                sender,
                new DCSBIOSUpdatesMissedEventArgs { HidInstance = hidInstance, GamingPanelEnum = gamingPanelEnum, Count = missedUpdateCount });
        }

        // For those that wants to listen to this panel
        public void AttachGamingPanelListener(IGamingPanelListener gamingPanelListener)
        {
            OnSwitchesChangedA += gamingPanelListener.SwitchesChanged;
            OnSettingsAppliedA += gamingPanelListener.SettingsApplied;
            OnSettingsModified += gamingPanelListener.SettingsModified;
            OnUpdatesHasBeenMissed += gamingPanelListener.UpdatesHasBeenMissed;
        }
        
        public void DetachGamingPanelListener(IGamingPanelListener gamingPanelListener)
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

        public event LedLightChangedEventHandler OnLedLightChangedA;

        public void LedLightChanged(object sender, string hidInstance, SaitekPanelLEDPosition saitekPanelLEDPosition, PanelLEDColor panelLEDColor)
        {
            OnLedLightChangedA?.Invoke(sender, new LedLightChangeEventArgs { HIDInstance = hidInstance, LEDPosition = saitekPanelLEDPosition, LEDColor = panelLEDColor });
        }
        
        public void AttachLEDLightListener(ILedLightPanelListener ledLightPanelListener)
        {
            OnLedLightChangedA += ledLightPanelListener.LedLightChanged;
        }

        public void DetachLEDLightListener(ILedLightPanelListener ledLightPanelListener)
        {
            OnLedLightChangedA -= ledLightPanelListener.LedLightChanged;
        }
    }
}
