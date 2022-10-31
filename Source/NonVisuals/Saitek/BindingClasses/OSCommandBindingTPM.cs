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
                // TPMPanelOSCommand{1KNOB_ENGINE_LEFT}\o/OSCommand{FILE\o/ARGUMENTS\o/NAME}\o/\\?\hid#vid_06a3&pid_0d67#9&231fd360&0&0000#{4d1e55b2-f16f-11cf-88cb-001111000030}
                var parameters = settings.Split(new[] { SaitekConstants.SEPARATOR_SYMBOL }, StringSplitOptions.RemoveEmptyEntries);

                // TPMPanelOSCommand{1KNOB_ENGINE_LEFT}
                var param0 = parameters[0].Replace("TPMPanelOSCommand{", string.Empty).Replace("}", string.Empty);

                // 1KNOB_ENGINE_LEFT
                WhenTurnedOn = param0.Substring(0, 1) == "1";
                param0 = param0.Substring(1);
                _tpmPanelSwitch = (TPMPanelSwitches)Enum.Parse(typeof(TPMPanelSwitches), param0);

                // OSCommand{FILE\o/ARGUMENTS\o/NAME}
                OSCommandObject = new OSCommand();
                OSCommandObject.ImportString(parameters[1]);
            }
        }

        public TPMPanelSwitches TPMSwitch
        {
            get => _tpmPanelSwitch;
            set => _tpmPanelSwitch = value;
        }

        public override string ExportSettings()
        {
            if (OSCommandObject == null || OSCommandObject.IsEmpty)
            {
                return null;
            }

            var onStr = WhenTurnedOn ? "1" : "0";
            return "TPMPanelOSCommand{" + onStr + Enum.GetName(typeof(TPMPanelSwitches), TPMSwitch) + "}" + SaitekConstants.SEPARATOR_SYMBOL + OSCommandObject.ExportString();
        }

    }
}
