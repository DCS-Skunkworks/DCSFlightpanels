using System;
using ClassLibraryCommon;

using NonVisuals.Radios;


namespace NonVisuals
{
    public class OSCommandBindingPZ69FullEmulator : OSCommandBinding
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
                var parameters = settings.Split(new[] { SeparatorChars }, StringSplitOptions.RemoveEmptyEntries);
                var param0 = parameters[0].Replace("RadioPanelOSPZ69Full{", "").Replace("}", "");
                _pz69DialPosition = (PZ69DialPosition)Enum.Parse(typeof(PZ69DialPosition), param0);
                var param1 = parameters[1].Replace("{", "").Replace("}", "");
                WhenTurnedOn = param1.Substring(0, 1) == "1";
                param1 = param1.Substring(1);
                _panelPZ69Knob = (RadioPanelPZ69KnobsEmulator)Enum.Parse(typeof(RadioPanelPZ69KnobsEmulator), param1);

                //OSCommand{FILE\o/ARGUMENTS\o/NAME}
                OSCommandObject = new OSCommand();
                OSCommandObject.ImportString(parameters[2]);
            }
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
            Common.DebugP(Enum.GetName(typeof(RadioPanelPZ69KnobsEmulator), RadioPanelPZ69Key) + "      " + WhenTurnedOn);
            var onStr = WhenTurnedOn ? "1" : "0";
            return "RadioPanelKeyDialPos{" + _pz69DialPosition + "}" + SeparatorChars + "{" + onStr + Enum.GetName(typeof(RadioPanelPZ69KnobsEmulator), RadioPanelPZ69Key) + "}" + SeparatorChars + OSCommandObject.ExportString();
        }

    }
}
