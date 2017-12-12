using System;

namespace NonVisuals
{
    public class KnobBindingPZ70
    {
        /*
         This class binds a physical switch on the PZ70 with a user made virtual keypress in Windows.
         */
        private PZ70DialPosition _pz70DialPosition;
        private MultiPanelPZ70Knobs _multiPanelPZ70Knob;
        private OSKeyPress _osKeyPress;
        private bool _whenOnTurnedOn = true;
        private const string SeparatorChars = "\\o/";

        internal void ImportSettings(string settings)
        {
            if (string.IsNullOrEmpty(settings))
            {
                throw new ArgumentException("Import string empty. (KeyBinding)");
            }
            if (settings.StartsWith("MultiPanelKnob{"))
            {
                //MultiPanelKey{ALT}\o/{1KNOB_ENGINE_LEFT}\o/OSKeyPress{[FiftyMilliSec,RCONTROL + RSHIFT + VK_R][FiftyMilliSec,RCONTROL + RSHIFT + VK_W]}\o/\\?\hid#vid_06a3&pid_0d67#9&231fd360&0&0000#{4d1e55b2-f16f-11cf-88cb-001111000030}
                var parameters = settings.Split(new[] { SeparatorChars }, StringSplitOptions.RemoveEmptyEntries);

                //MultiPanelKey{ALT}
                var param0 = parameters[0].Replace("MultiPanelKnob{", "").Replace("}", "");
                _pz70DialPosition = (PZ70DialPosition)Enum.Parse(typeof(PZ70DialPosition), param0);

                //{1KNOB_ENGINE_LEFT}
                var param1 = parameters[1].Replace("{", "").Replace("}", "");
                //1KNOB_ENGINE_LEFT
                _whenOnTurnedOn = param1.Substring(0, 1) == "1";
                param1 = param1.Substring(1);
                _multiPanelPZ70Knob = (MultiPanelPZ70Knobs)Enum.Parse(typeof(MultiPanelPZ70Knobs), param1);

                //OSKeyPress{[FiftyMilliSec,RCONTROL + RSHIFT + VK_R][FiftyMilliSec,RCONTROL + RSHIFT + VK_W]}
                _osKeyPress = new OSKeyPress();
                _osKeyPress.ImportString(parameters[2]);
            }
        }

        public PZ70DialPosition DialPosition
        {
            get { return _pz70DialPosition; }
            set { _pz70DialPosition = value; }
        }

        public MultiPanelPZ70Knobs MultiPanelPZ70Knob
        {
            get { return _multiPanelPZ70Knob; }
            set { _multiPanelPZ70Knob = value; }
        }

        public OSKeyPress OSKeyPress
        {
            get { return _osKeyPress; }
            set { _osKeyPress = value; }
        }


        public string ExportSettings()
        {
            if (OSKeyPress == null || OSKeyPress.IsEmpty())
            {
                return null;
            }
            Common.DebugP(Enum.GetName(typeof(MultiPanelPZ70Knobs), MultiPanelPZ70Knob) + "      " + _whenOnTurnedOn);
            var onStr = _whenOnTurnedOn ? "1" : "0";
            return "MultiPanelKnob{" + _pz70DialPosition + "}" + SeparatorChars + "{" + onStr + Enum.GetName(typeof(MultiPanelPZ70Knobs), MultiPanelPZ70Knob) + "}" + SeparatorChars + _osKeyPress.ExportString();
        }

        public bool WhenTurnedOn
        {
            get { return _whenOnTurnedOn; }
            set { _whenOnTurnedOn = value; }
        }

    }
}
