namespace NonVisuals.Interfaces
{
    using EventArgs;

    public interface IGamingPanelListener
    {
        /// <summary>
        /// Used by UserControls to show switches that has been manipulated.
        /// Shows the actions in the Log textbox of the UserControl.
        /// </summary>
        void SwitchesChanged(object sender, SwitchesChangedEventArgs e);
        
        /// <summary>
        /// Used by some UserControls refresh UI to know when panels have loaded their configurations.
        /// Used by MainWindow to SetFormState().
        /// </summary>
        void SettingsApplied(object sender, PanelInfoArgs e);
        
        /// <summary>
        /// Used by some UserControls to show panel's updated configurations. 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void SettingsModified(object sender, PanelInfoArgs e);
        
        /// <summary>
        /// DCS-BIOS has a feature to detect if any updates has been missed.
        /// It is not used as such since DCS-BIOS has been working so well.
        /// </summary>
        void UpdatesHasBeenMissed(object sender, DCSBIOSUpdatesMissedEventArgs e);
    }
}
