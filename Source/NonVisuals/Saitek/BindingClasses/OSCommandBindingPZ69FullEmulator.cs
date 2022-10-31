﻿namespace NonVisuals.Saitek.BindingClasses
{
    using System;

    using MEF;

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
                var parameters = settings.Split(new[] { SaitekConstants.SEPARATOR_SYMBOL }, StringSplitOptions.RemoveEmptyEntries);
                var param0 = parameters[0].Replace("RadioPanelOSPZ69Full{", string.Empty).Replace("}", string.Empty);
                _pz69DialPosition = (PZ69DialPosition)Enum.Parse(typeof(PZ69DialPosition), param0);
                var param1 = parameters[1].Replace("{", string.Empty).Replace("}", string.Empty);
                WhenTurnedOn = param1.Substring(0, 1) == "1";
                param1 = param1.Substring(1);
                _panelPZ69Knob = (RadioPanelPZ69KnobsEmulator)Enum.Parse(typeof(RadioPanelPZ69KnobsEmulator), param1);

                // OSCommand{FILE\o/ARGUMENTS\o/NAME}
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

            var onStr = WhenTurnedOn ? "1" : "0";
            return "RadioPanelKeyDialPos{" + _pz69DialPosition + "}" + SaitekConstants.SEPARATOR_SYMBOL + "{" + onStr + Enum.GetName(typeof(RadioPanelPZ69KnobsEmulator), RadioPanelPZ69Key) + "}" + SaitekConstants.SEPARATOR_SYMBOL + OSCommandObject.ExportString();
        }

    }
}
