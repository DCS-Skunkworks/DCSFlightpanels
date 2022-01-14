namespace NonVisuals.Interfaces
{
    using EventArgs;

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
        void SettingsApplied(object sender, PanelInfoArgs e);
        
        /*
         * Used by some UserControls to show panel's updated configurations.
         */
        void SettingsModified(object sender, PanelInfoArgs e);
        
        /*
         * DCS-BIOS has a feature to detect if any updates has been missed.
         * It is not used as such since DCS-BIOS has been working so well.
         */
        void UpdatesHasBeenMissed(object sender, DCSBIOSUpdatesMissedEventArgs e);
    }
}
