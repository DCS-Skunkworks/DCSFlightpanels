namespace MEF
{
    using System.Collections.Generic;

    public interface IPanelEventHandler
    {
        /*
         * eventType : 0 Released / Inactive, 1 Pressed / Active
         * All id's for panels and switches can be found in Switched.cs
         */

        /// <summary>
        /// The panel event.
        /// </summary>
        /// <param name="profile">
        /// Airplane or helicopter currently in use.
        /// </param>
        /// <param name="panelHidId">
        /// The panel hid id. As there can be multiple panels of same type this will
        /// enable you to differentiate them.
        /// </param>
        /// <param name="panelId">
        /// From the enum PluginGamingPanelEnum (Switches.cs)
        /// </param>
        /// <param name="switchId">
        /// Each panel has its own enum describing the switches/buttons/knobs (Switches.cs)
        /// </param>
        /// <param name="pressed">
        /// Releases or Pressed
        /// </param>
        /// <param name="keySequence">
        /// List of keys including delays
        /// Is NULL if nothing is associated with the key/switch/knob
        /// </param>
        void PanelEvent(string profile, string panelHidId, int panelId, int switchId, bool pressed, SortedList<int, IKeyPressInfo> keySequence);

        /// <summary>
        /// Called by DCSFP when someone clicks the plugin's menu. So user can change settings, plugin may bring up a window, settings file.
        /// </summary>
        void Settings();
    }
}
