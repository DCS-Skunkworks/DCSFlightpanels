namespace NonVisuals.Saitek.BindingClasses
{
    using System;

    using MEF;

    [Serializable]
    public class OSCommandBindingTPM : OSCommandBindingBase
    {
        /*
         This class binds a physical switch on the TPM with a Windows OS command.
         */
        private TPMPanelSwitches _tpmPanelSwitch;


        internal override void ImportSettings(string settings)
        {
            if (string.IsNullOrEmpty(settings))
            {
                throw new ArgumentException("Import string empty. (KeyBinding)");
            }

            if (settings.StartsWith("TPMPanelOSCommand{"))
            {
                var result = ParseSettingV1(settings);
                _tpmPanelSwitch = (TPMPanelSwitches)Enum.Parse(typeof(TPMPanelSwitches), result.Item2);
                /*
                 * other settings already added
                 */
            }
        }

        public override string ExportSettings()
        {
            if (OSCommandObject == null || OSCommandObject.IsEmpty)
            {
                return null;
            }
            return GetExportString("TPMPanelOSCommand", null, Enum.GetName(typeof(TPMPanelSwitches), TPMSwitch));
        }

        public TPMPanelSwitches TPMSwitch
        {
            get => _tpmPanelSwitch;
            set => _tpmPanelSwitch = value;
        }

    }
}
