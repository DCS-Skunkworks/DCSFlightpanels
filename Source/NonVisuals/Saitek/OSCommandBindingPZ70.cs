using System;
using ClassLibraryCommon;

namespace NonVisuals
{
    public class OSCommandBindingPZ70 : OSCommandBinding
    {
        /*
         This class binds a physical switch on the PZ70 with a Windows OS command.
         */
        private PZ70DialPosition _pz70DialPosition;
        private MultiPanelPZ70Knobs _multiPanelPZ70Knob;


        internal override void ImportSettings(string settings)
        {
            if (string.IsNullOrEmpty(settings))
            {
                throw new ArgumentException("Import string empty. (WindowsBinding)");
            }
            if (settings.StartsWith("MultiPanelOSPZ70{"))
            {
                //MultiPanelOSPZ70{ALT}\o/{1KNOB_ENGINE_LEFT}\o/OSCommand{FILE\o/ARGUMENTS\o/NAME}\o/\\?\hid#vid_06a3&pid_0d67#9&231fd360&0&0000#{4d1e55b2-f16f-11cf-88cb-001111000030}
                var parameters = settings.Split(new[] { SeparatorChars }, StringSplitOptions.RemoveEmptyEntries);

                //MultiPanelOSPZ70{ALT}
                var param0 = parameters[0].Replace("MultiPanelOSPZ70{", "").Replace("}", "");
                _pz70DialPosition = (PZ70DialPosition)Enum.Parse(typeof(PZ70DialPosition), param0);

                //{1KNOB_ENGINE_LEFT}
                var param1 = parameters[1].Replace("{", "").Replace("}", "");
                //1KNOB_ENGINE_LEFT
                WhenTurnedOn = param1.Substring(0, 1) == "1";
                param1 = param1.Substring(1);
                _multiPanelPZ70Knob = (MultiPanelPZ70Knobs)Enum.Parse(typeof(MultiPanelPZ70Knobs), param1);

                //OSCommand{FILE\o/ARGUMENTS\o/NAME}
                OSCommandObject = new OSCommand();
                OSCommandObject.ImportString(parameters[2]);
            }
        }

        public PZ70DialPosition DialPosition
        {
            get => _pz70DialPosition;
            set => _pz70DialPosition = value;
        }

        public MultiPanelPZ70Knobs MultiPanelPZ70Knob
        {
            get => _multiPanelPZ70Knob;
            set => _multiPanelPZ70Knob = value;
        }

        public override string ExportSettings()
        {
            if (OSCommandObject == null || OSCommandObject.IsEmpty)
            {
                return null;
            }
            Common.DebugP(Enum.GetName(typeof(MultiPanelPZ70Knobs), MultiPanelPZ70Knob) + "      " + WhenTurnedOn);
            var onStr = WhenTurnedOn ? "1" : "0";
            return "MultiPanelOSPZ70{" + _pz70DialPosition + "}" + SeparatorChars + "{" + onStr + Enum.GetName(typeof(MultiPanelPZ70Knobs), MultiPanelPZ70Knob) + "}" + SeparatorChars + OSCommandObject.ExportString();
        }
    }
}
