using System;
using ClassLibraryCommon;

namespace NonVisuals
{
    public class KeyBindingPZ69DialPosition
    {
        /*
         This class binds a physical switch on the PZ69 with a user made virtual keypress in Windows.
         This class differs that the binding is bound to what the dial position is in. So all
         dials positions COM1 -> XPDR have different bindings.
         */
        private PZ69DialPosition _pz69DialPosition;
        private RadioPanelPZ69KnobsEmulator _panelPZ69Knob;
        private OSKeyPress _osKeyPress;
        private bool _whenOnTurnedOn = true;
        private const string SeparatorChars = "\\o/";

        internal void ImportSettings(string settings)
        {
            if (string.IsNullOrEmpty(settings))
            {
                throw new ArgumentException("Import string empty. (KeyBinding)");
            }
            if (settings.StartsWith("RadioPanelKeyDialPos{"))
            {
                var parameters = settings.Split(new[] { SeparatorChars }, StringSplitOptions.RemoveEmptyEntries);
                var param0 = parameters[0].Replace("RadioPanelKeyDialPos{", "").Replace("}", "");
                _pz69DialPosition = (PZ69DialPosition)Enum.Parse(typeof(PZ69DialPosition), param0);
                var param1 = parameters[1].Replace("{", "").Replace("}", "");
                _whenOnTurnedOn = param1.Substring(0, 1) == "1";
                param1 = param1.Substring(1);
                _panelPZ69Knob = (RadioPanelPZ69KnobsEmulator)Enum.Parse(typeof(RadioPanelPZ69KnobsEmulator), param1);
                _osKeyPress = new OSKeyPress();
                _osKeyPress.ImportString(parameters[2]);
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

        public OSKeyPress OSKeyPress
        {
            get => _osKeyPress;
            set => _osKeyPress = value;
        }


        public string ExportSettings()
        {
            if (_pz69DialPosition == PZ69DialPosition.Unknown)
            {
                throw new Exception("Unknown dial position in KeyBindingPZ69DialPosition for knob " + RadioPanelPZ69Key + ". Cannot export.");
            }
            if (OSKeyPress == null || OSKeyPress.IsEmpty())
            {
                return null;
            }
            Common.DebugP(Enum.GetName(typeof(RadioPanelPZ69KnobsEmulator), RadioPanelPZ69Key) + "      " + _whenOnTurnedOn);
            var onStr = _whenOnTurnedOn ? "1" : "0";
            return "RadioPanelKeyDialPos{" + _pz69DialPosition + "}" + SeparatorChars + "{" + onStr + Enum.GetName(typeof(RadioPanelPZ69KnobsEmulator), RadioPanelPZ69Key) + "}" + SeparatorChars + _osKeyPress.ExportString();
        }

        public bool WhenTurnedOn
        {
            get => _whenOnTurnedOn;
            set => _whenOnTurnedOn = value;
        }




    }
}
