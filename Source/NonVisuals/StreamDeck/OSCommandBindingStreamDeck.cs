using System;
using ClassLibraryCommon;
using NonVisuals.Interfaces;
using NonVisuals.StreamDeck;

namespace NonVisuals.StreamDeck
{
    public class OSCommandBindingStreamDeck : OSCommandBinding, IStreamDeckButtonAction
    {
        /*
         This class binds a physical button on the Stream Deck with a Windows OS command.
         */
        private StreamDeckButtonNames _streamDeckButtonName;


        public EnumStreamDeckButtonActionType GetActionType()
        {
            return EnumStreamDeckButtonActionType.OSCommand;
        }

        public void Execute()
        {
            OSCommandObject.Execute();
        }

        internal override void ImportSettings(string settings)
        {
            if (string.IsNullOrEmpty(settings))
            {
                throw new ArgumentException("Import string empty. (WindowsBinding)");
            }
            if (settings.StartsWith("StreamDeckOS{"))
            {
                //StreamDeckOS{1KNOB_ENGINE_LEFT}\o/OSCommand{FILE\o/ARGUMENTS\o/NAME}\o/\\?\hid#vid_06a3&pid_0d67#9&231fd360&0&0000#{4d1e55b2-f16f-11cf-88cb-001111000030}
                var parameters = settings.Split(new[] { SeparatorChars }, StringSplitOptions.RemoveEmptyEntries);

                //StreamDeckOS{1KNOB_ENGINE_LEFT}
                var param0 = parameters[0].Replace("StreamDeckOS{", "").Replace("}", "");
                //1KNOB_ENGINE_LEFT
                WhenTurnedOn = (param0.Substring(0, 1) == "1");
                param0 = param0.Substring(1);
                _streamDeckButtonName = (StreamDeckButtonNames)Enum.Parse(typeof(StreamDeckButtonNames), param0);

                //OSCommand{FILE\o/ARGUMENTS\o/NAME}
                OSCommandObject = new OSCommand();
                OSCommandObject.ImportString(parameters[1]);
            }
        }

        public StreamDeckButtonNames StreamDeckButtonName
        {
            get => _streamDeckButtonName;
            set => _streamDeckButtonName = value;
        }

        public override string ExportSettings()
        {
            if (OSCommandObject == null || OSCommandObject.IsEmpty)
            {
                return null;
            }
            Common.DebugP(Enum.GetName(typeof(StreamDeckButtonNames), StreamDeckButtonName) + "      " + WhenTurnedOn);
            var onStr = WhenTurnedOn ? "1" : "0";
            return "StreamDeckOS{" + onStr + Enum.GetName(typeof(StreamDeckButtonNames), StreamDeckButtonName) + "}" + SeparatorChars + OSCommandObject.ExportString();
        }
    }
}
