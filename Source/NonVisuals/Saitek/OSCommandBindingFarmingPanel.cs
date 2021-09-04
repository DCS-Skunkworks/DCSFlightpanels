namespace NonVisuals.Saitek
{
    using System;

    using MEF;

    [Serializable]
    public class OSCommandBindingFarmingPanel : OSCommandBinding
    {
        /*
         This class binds a physical switch on the Farming Simulator Side Panel with a Windows OS command.
         */
        private FarmingPanelMKKeys _farmingPanelKey;
        
        internal override void ImportSettings(string settings)
        {
            if (string.IsNullOrEmpty(settings))
            {
                throw new ArgumentException("Import string empty. (WindowsBinding)");
            }
            if (settings.StartsWith("FarmingPanelOS{"))
            {
                // FarmingPanelOS{1KNOB_ENGINE_LEFT}\o/OSCommand{FILE\o/ARGUMENTS\o/NAME}\o/\\?\hid#vid_06a3&pid_0d67#9&231fd360&0&0000#{4d1e55b2-f16f-11cf-88cb-001111000030}
                var parameters = settings.Split(new[] { SaitekConstants.SEPARATOR_SYMBOL }, StringSplitOptions.RemoveEmptyEntries);

                // FarmingPanelOS{1KNOB_ENGINE_LEFT}
                var param0 = parameters[0].Replace("FarmingPanelOS{", string.Empty).Replace("}", string.Empty);

                // 1KNOB_ENGINE_LEFT
                WhenTurnedOn = param0.Substring(0, 1) == "1";
                param0 = param0.Substring(1);
                _farmingPanelKey = (FarmingPanelMKKeys)Enum.Parse(typeof(FarmingPanelMKKeys), param0);

                // OSCommand{FILE\o/ARGUMENTS\o/NAME}
                OSCommandObject = new OSCommand();
                OSCommandObject.ImportString(parameters[1]);
            }
        }

        public FarmingPanelMKKeys FarmingPanelKey
        {
            get => _farmingPanelKey;
            set => _farmingPanelKey = value;
        }

        public override string ExportSettings()
        {
            if (OSCommandObject == null || OSCommandObject.IsEmpty)
            {
                return null;
            }

            var onStr = WhenTurnedOn ? "1" : "0";
            return "FarmingPanelOS{" + onStr + Enum.GetName(typeof(FarmingPanelMKKeys), FarmingPanelKey) + "}" + SaitekConstants.SEPARATOR_SYMBOL + OSCommandObject.ExportString();
        }
    }
}
