using System;
using MEF;

namespace NonVisuals.BindingClasses.OSCommand
{
    [Serializable]
    public class OSCommandBindingPZ69FullEmulator : OSCommandBindingBase
    {
        /*
         This class binds a physical switch on the PZ69 full emulator (includes DCS-BIOS) with a Windows OS command.
         */
        private PZ69DialPosition _pz69DialPosition;
        private RadioPanelPZ69KnobsEmulator _panelPZ69Knob;

        internal override void ImportSettings(string settings)
        {
            if (string.IsNullOrEmpty(settings))
            {
                throw new ArgumentException("Import string empty. (KeyBinding)");
            }

            if (settings.StartsWith("RadioPanelOSPZ69Full{"))
            {
                var result = ParseSettingV1(settings);
                _pz69DialPosition = (PZ69DialPosition)Enum.Parse(typeof(PZ69DialPosition), result.Item1);
                _panelPZ69Knob = (RadioPanelPZ69KnobsEmulator)Enum.Parse(typeof(RadioPanelPZ69KnobsEmulator), result.Item2);
                /*
                 * other settings already added
                 */
            }
        }

        public override string ExportSettings()
        {
            if (_pz69DialPosition == PZ69DialPosition.Unknown)
            {
                throw new Exception("Unknown dial position in OSCommandBindingPZ69FullEmulator for knob " + RadioPanelPZ69Key + ". Cannot export.");
            }

            if (OSCommandObject == null || OSCommandObject.IsEmpty)
            {
                return null;
            }

            return GetExportString("RadioPanelOSPZ69Full", Enum.GetName(typeof(PZ69DialPosition), _pz69DialPosition), Enum.GetName(typeof(RadioPanelPZ69KnobsEmulator), RadioPanelPZ69Key));
        }

        public PZ69DialPosition DialPosition
        {
            get => _pz69DialPosition;
            set => _pz69DialPosition = value;
        }

        public RadioPanelPZ69KnobsEmulator RadioPanelPZ69Key
        {
            get => _panelPZ69Knob;
            set => _panelPZ69Knob = value;
        }

    }
}
