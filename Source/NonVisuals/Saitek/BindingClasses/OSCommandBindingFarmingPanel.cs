using NonVisuals.Saitek.Switches;

namespace NonVisuals.Saitek.BindingClasses
{
    using System;

    using MEF;

    [Serializable]
    public class OSCommandBindingFarmingPanel : OSCommandBindingBase
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
                var result = ParseSettingV1(settings);

                _farmingPanelKey = (FarmingPanelMKKeys)Enum.Parse(typeof(FarmingPanelMKKeys), result.Item2);
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

            return GetExportString("FarmingPanelOS", null, Enum.GetName(typeof(FarmingPanelMKKeys), _farmingPanelKey));
        }

        public FarmingPanelMKKeys FarmingPanelKey
        {
            get => _farmingPanelKey;
            set => _farmingPanelKey = value;
        }
    }
}
