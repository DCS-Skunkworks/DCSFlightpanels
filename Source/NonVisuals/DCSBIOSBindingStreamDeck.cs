using System;
using System.Collections.Generic;
using System.Text;
using ClassLibraryCommon;
using DCS_BIOS;

namespace NonVisuals
{
    public class DCSBIOSBindingStreamDeck : DCSBIOSBindingBase
    {
        /*
         This class binds a physical key on the Stream Deck with a DCSBIOSInput
         */
        private StreamDeck35Buttons _streamDeck35Button;
        
        ~DCSBIOSBindingStreamDeck()
        {
            CancelSendDCSBIOSCommands = true;
            DCSBIOSCommandsThread?.Abort();
        }

        internal override void ImportSettings(string settings)
        {
            if (string.IsNullOrEmpty(settings))
            {
                throw new ArgumentException("Import string empty. (DCSBIOSBindingStreamDeck)");
            }
            if (settings.StartsWith("StreamDeckDCSBIOSControl{"))
            {
                //StreamDeckDCSBIOSControl{1BUTTON12|DCS-BIOS}\o/DCSBIOSInput{AAP_CDUPWR|SET_STATE|1|0}
                var parameters = settings.Split(new[] { SeparatorChars }, StringSplitOptions.RemoveEmptyEntries);

                //StreamDeckDCSBIOSControl{1BUTTON12|DCS-BIOS}
                var param0 = parameters[0].Replace("StreamDeckDCSBIOSControl{", "").Replace("}","");
                //1BUTTON12|DCS-BIOS
                WhenOnTurnedOn = (param0.Substring(0, 1) == "1");
                if (param0.Contains("|"))
                {
                    //1BUTTON12|DCS-BIOS
                    param0 = param0.Substring(1);
                    //BUTTON12|DCS-BIOS
                    var stringArray = param0.Split(new[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                    _streamDeck35Button = (StreamDeck35Buttons)Enum.Parse(typeof(StreamDeck35Buttons), stringArray[0]);
                    Description = stringArray[1];
                }
                else
                {
                    param0 = param0.Substring(1);
                    _streamDeck35Button = (StreamDeck35Buttons)Enum.Parse(typeof(StreamDeck35Buttons), param0);
                }
                //The rest of the array besides last entry are DCSBIOSInput
                //DCSBIOSInput{AAP_CDUPWR|SET_STATE|1|0}
                DCSBIOSInputs = new List<DCSBIOSInput>();
                for (int i = 1; i < parameters.Length - 1; i++)
                {
                    if (parameters[i].StartsWith("DCSBIOSInput"))
                    {
                        var dcsbiosInput = new DCSBIOSInput();
                        dcsbiosInput.ImportString(parameters[i]);
                        DCSBIOSInputs.Add(dcsbiosInput);
                    }
                }

            }
        }

        public override string ExportSettings()
        {
            if (DCSBIOSInputs.Count == 0)
            {
                return null;
            }
            Common.DebugP(Enum.GetName(typeof(StreamDeck35Buttons), StreamDeckButton) + "      " + WhenOnTurnedOn);
            var onStr = WhenOnTurnedOn ? "1" : "0";

            var stringBuilder = new StringBuilder();
            foreach (var dcsbiosInput in DCSBIOSInputs)
            {
                stringBuilder.Append(SeparatorChars + dcsbiosInput.ToString());
            }
            if (!string.IsNullOrWhiteSpace(Description))
            {
                return "StreamDeckDCSBIOSControl{" + onStr + Enum.GetName(typeof(StreamDeck35Buttons), StreamDeckButton) + "|" + Description + "}" + stringBuilder.ToString();
            }
            return "StreamDeckDCSBIOSControl{" + onStr + Enum.GetName(typeof(StreamDeck35Buttons), StreamDeckButton) + "}" + stringBuilder.ToString();
        }
        
        public StreamDeck35Buttons StreamDeckButton
        {
            get => _streamDeck35Button;
            set => _streamDeck35Button = value;
        }
    }
}
