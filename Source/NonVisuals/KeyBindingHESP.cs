using System;
using ClassLibraryCommon;

namespace NonVisuals
{
    public class KeyBindingHESP
    {
        /*
         This class binds a physical switch on the Heavy Equipment Side Panel (HESP) with a user made virtual keypress in Windows.
         */
        private HESPKeys _hespKey;
        private OSKeyPress _osKeyPress;
        private bool _whenOnTurnedOn = true;
        private const string SeparatorChars = "\\o/";

        internal void ImportSettings(string settings)
        {
            if (string.IsNullOrEmpty(settings))
            {
                throw new ArgumentException("Import string empty. (KeyBinding)");
            }
            if (settings.StartsWith("HESPKey{"))
            {
                //HESPKey{1KNOB_ENGINE_LEFT}\o/OSKeyPress{[FiftyMilliSec,RCONTROL + RSHIFT + VK_R][FiftyMilliSec,RCONTROL + RSHIFT + VK_W]}\o/\\?\hid#vid_06a3&pid_0d67#9&231fd360&0&0000#{4d1e55b2-f16f-11cf-88cb-001111000030}
                var parameters = settings.Split(new[] { SeparatorChars }, StringSplitOptions.RemoveEmptyEntries);

                //HESPKey{1KNOB_ENGINE_LEFT}
                var param0 = parameters[0].Trim().Substring(15);
                //1KNOB_ENGINE_LEFT}
                param0 = param0.Remove(param0.Length - 1, 1);
                //1KNOB_ENGINE_LEFT
                _whenOnTurnedOn = (param0.Substring(0, 1) == "1");
                param0 = param0.Substring(1);
                _hespKey = (HESPKeys)Enum.Parse(typeof(HESPKeys), param0);

                //OSKeyPress{[FiftyMilliSec,RCONTROL + RSHIFT + VK_R][FiftyMilliSec,RCONTROL + RSHIFT + VK_W]}
                _osKeyPress = new OSKeyPress();
                _osKeyPress.ImportString(parameters[1]);
            }
        }

        public HESPKeys HESPKey
        {
            get { return _hespKey; }
            set { _hespKey = value; }
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
            Common.DebugP(Enum.GetName(typeof(HESPKeys), HESPKey) + "      " + _whenOnTurnedOn);
            var onStr = _whenOnTurnedOn ? "1" : "0";
            return "HESPKey{" + onStr + Enum.GetName(typeof(HESPKeys), HESPKey) + "}" + SeparatorChars + _osKeyPress.ExportString();
        }

        public bool WhenTurnedOn
        {
            get { return _whenOnTurnedOn; }
            set { _whenOnTurnedOn = value; }
        }




    }
}
