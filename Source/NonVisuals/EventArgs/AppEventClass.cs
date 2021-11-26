using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClassLibraryCommon;
using NonVisuals.Interfaces;

namespace NonVisuals.EventArgs
{
    public static class AppEventClass
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
            OnSettingsModified += settingsModifiedListener.PanelSettingsModified;
        }

        public static void DetachSettingsModified(ISettingsModifiedListener settingsModifiedListener)
        {
            OnSettingsModified -= settingsModifiedListener.PanelSettingsModified;
        }

        /*
         * Used when user disables action (e.g. key press) forwarding so that panels knows not to do the actual action
         */

        public delegate void ForwardPanelActionChangedEventHandler(object sender, ForwardPanelEventArgs e);

        public static event ForwardPanelActionChangedEventHandler ForwardPanelEventChanged;

        public static void ForwardKeyPressEvent(object sender, bool doForwardActions)
        {
            ForwardPanelEventChanged?.Invoke(sender, new ForwardPanelEventArgs() { Forward = !doForwardActions });
        }

        public static void AttachForwardPanelEventListener(GamingPanel gamingPanel)
        {
            ForwardPanelEventChanged += gamingPanel.SetForwardPanelEvent;
        }

        public static void DetachForwardPanelEventListener(GamingPanel gamingPanel)
        {
            ForwardPanelEventChanged -= gamingPanel.SetForwardPanelEvent;
        }


        /*
         * Events triggered by ProfileHandler
         */
        public delegate void ProfileReadFromFileEventHandler(object sender, PanelBindingReadFromFileEventArgs e);

        public static event ProfileReadFromFileEventHandler OnSettingsReadFromFile;

        public static void SettingsReadFromFile(object sender, GenericPanelBinding genericPanelBinding)
        {
            OnSettingsReadFromFile(sender, new PanelBindingReadFromFileEventArgs { PanelBinding = genericPanelBinding });
        }

        public delegate void SavePanelSettingsEventHandler(object sender, ProfileHandlerEventArgs e);

        public static event SavePanelSettingsEventHandler OnSavePanelSettings;

        public static void SavePanelSettings(ProfileHandler profileHandler)
        {
            OnSavePanelSettings?.Invoke(profileHandler, new ProfileHandlerEventArgs { ProfileHandlerEA = profileHandler });
        }

        public delegate void SavePanelSettingsEventHandlerJSON(object sender, ProfileHandlerEventArgs e);

        public static event SavePanelSettingsEventHandlerJSON OnSavePanelSettingsJSON;

        public static void SavePanelSettingsJSON(ProfileHandler profileHandler)
        {
            OnSavePanelSettingsJSON?.Invoke(profileHandler, new ProfileHandlerEventArgs { ProfileHandlerEA = profileHandler });
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

        /*
         * Events triggered by the ProfileHandler regarding read configurations and saving them
         */
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
    }
}
