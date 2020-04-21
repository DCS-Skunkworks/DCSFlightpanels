using System;
using ClassLibraryCommon;

using NonVisuals.Radios;


namespace NonVisuals.Saitek
{
    public class OSCommandBindingPZ69Emulator : OSCommandBinding
    {
        /*
         This class binds a physical switch on the PZ69 key emulator with a Windows OS command.
         */
        private RadioPanelPZ69KnobsEmulator _panelPZ69Knob;


        internal override void ImportSettings(string settings)
        {
            if (string.IsNullOrEmpty(settings))
            {
                throw new ArgumentException("Import string empty. (KeyBinding)");
            }
            if (settings.StartsWith("RadioPanelOSPZ69{"))
            {
                //RadioPanelOSPZ69{1UpperCOM1}\o/OSCommand{FILE\o/ARGUMENTS\o/NAME}\o/\\?\hid#vid_06a3&pid_0d67#9&231fd360&0&0000#{4d1e55b2-f16f-11cf-88cb-001111000030}
                var parameters = settings.Split(new[] { SaitekConstants.SEPARATOR_SYMBOL }, StringSplitOptions.RemoveEmptyEntries);

                //RadioPanelOSPZ69{1UpperCOM1}
                var param0 = parameters[0].Replace("RadioPanelOSPZ69{", "").Replace("}", "");
                //1UpperCOM1
                WhenTurnedOn = (param0.Substring(0, 1) == "1");
                param0 = param0.Substring(1);
                _panelPZ69Knob = (RadioPanelPZ69KnobsEmulator)Enum.Parse(typeof(RadioPanelPZ69KnobsEmulator), param0);

                //OSCommand{FILE\o/ARGUMENTS\o/NAME}
                OSCommandObject = new OSCommand();
                OSCommandObject.ImportString(parameters[1]);
            }
        }

        public RadioPanelPZ69KnobsEmulator RadioPanelPZ69Key
        {
            get => _panelPZ69Knob;
            set => _panelPZ69Knob = value;
        }

        public override string ExportSettings()
        {
            if (OSCommandObject == null || OSCommandObject.IsEmpty)
            {
                return null;
            }
            Common.DebugP(Enum.GetName(typeof(RadioPanelPZ69KnobsEmulator), RadioPanelPZ69Key) + "      " + WhenTurnedOn);
            var onStr = WhenTurnedOn ? "1" : "0";
            return "RadioPanelOSPZ69{" + onStr + Enum.GetName(typeof(RadioPanelPZ69KnobsEmulator), RadioPanelPZ69Key) + "}" + SaitekConstants.SEPARATOR_SYMBOL + OSCommandObject.ExportString();
        }

    }
}
