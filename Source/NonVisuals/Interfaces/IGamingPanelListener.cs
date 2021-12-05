namespace NonVisuals.Interfaces
{
    using NonVisuals.EventArgs;

    public interface IGamingPanelListener
    {
        /*
         * Used by UserControls to show switches that has been manipulated.
         * Shows the actions in the Log textbox of the UserControl.
         */
        void SwitchesChanged(object sender, SwitchesChangedEventArgs e);

        /*
         * Used by some UserControls refresh UI to know when panels have loaded their configurations.
         * Used by MainWindow to SetFormState().
         */
        void SettingsApplied(object sender, PanelEventArgs e);
        
        /*
         * Used by some UserControls to show panel's updated configurations.
         */
        void SettingsModified(object sender, PanelEventArgs e);

        /*
         * Used by those UserControls who's panels can show LED lights.
         * Used to show the same color in the UserControl as the physical panels.
         */
        void LedLightChanged(object sender, LedLightChangeEventArgs e);

        /*
         * Used for notifying when a device has been attached.
         * Not used atm.
         */
        void DeviceAttached(object sender, PanelEventArgs e);

        /*
         * Used for notifying when a device has been detached.
         * Not used atm.
         */
        void DeviceDetached(object sender, PanelEventArgs e);

        /*
         * DCS-BIOS has a feature to detect if any updates has been missed.
         * It is not used as such since DCS-BIOS has been working so well.
         */
        void UpdatesHasBeenMissed(object sender, DCSBIOSUpdatesMissedEventArgs e);
    }
}
