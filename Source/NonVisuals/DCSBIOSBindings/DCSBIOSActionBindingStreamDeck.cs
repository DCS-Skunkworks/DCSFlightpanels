using System;
using System.Collections.Generic;
using System.Text;
using ClassLibraryCommon;
using DCS_BIOS;
using NonVisuals.DCSBIOSBindings;
using NonVisuals.Interfaces;
using NonVisuals.StreamDeck;

namespace NonVisuals.DCSBIOSBindings
{
    public class DCSBIOSActionBindingStreamDeck : DCSBIOSActionBindingBase, IStreamDeckButtonAction
    {
        /*
         This class binds a physical key on the Stream Deck with a DCSBIOSInput
         Pressing the button will send a DCSBIOS command.
         */
        private StreamDeckButtonNames _streamDeckButtonName;
        private string _layerName = "";











        public EnumStreamDeckButtonActionType GetActionType()
        {
            return EnumStreamDeckButtonActionType.DCSBIOS;
        }

        public void Execute()
        {
            SendDCSBIOSCommands();
        }


        ~DCSBIOSActionBindingStreamDeck()
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
            if (settings.StartsWith("StreamDeckDCSBIOSInput{"))
            {
                //StreamDeckDCSBIOSInput{Home Layer|1BUTTON12|DCS-BIOS}\o/DCSBIOSInput{AAP_CDUPWR|SET_STATE|1|0}
                var parameters = settings.Split(new[] { SeparatorChars }, StringSplitOptions.RemoveEmptyEntries);

                //StreamDeckDCSBIOSInput{Home Layer|1BUTTON12|DCS-BIOS}
                var param0 = parameters[0].Replace("StreamDeckDCSBIOSInput{", "").Replace("}","");
                //Home Layer|1BUTTON12|DCS-BIOS
                var param0Split = param0.Split(new[] { "|" }, StringSplitOptions.RemoveEmptyEntries);

                LayerName = param0Split[0];
                WhenOnTurnedOn = (param0Split[1].Substring(0, 1) == "1");
                param0Split[1] = param0Split[1].Substring(1);
                _streamDeckButtonName = (StreamDeckButtonNames)Enum.Parse(typeof(StreamDeckButtonNames), param0Split[1]);

                if (param0Split.Length > 2)
                {
                    Description = param0Split[2];
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
            Common.DebugP(Enum.GetName(typeof(StreamDeckButtonNames), StreamDeckButtonName) + "      " + WhenOnTurnedOn);
            var onStr = WhenOnTurnedOn ? "1" : "0";

            var stringBuilder = new StringBuilder();
            foreach (var dcsbiosInput in DCSBIOSInputs)
            {
                stringBuilder.Append(SeparatorChars + dcsbiosInput.ToString());
            }
            if (!string.IsNullOrWhiteSpace(Description))
            {
                return "StreamDeckDCSBIOSInput{" + LayerName + "|" + onStr + Enum.GetName(typeof(StreamDeckButtonNames), StreamDeckButtonName) + "|" + Description + "}" + stringBuilder.ToString();
            }
            return "StreamDeckDCSBIOSInput{" + LayerName + "|" + onStr + Enum.GetName(typeof(StreamDeckButtonNames), StreamDeckButtonName) + "}" + stringBuilder.ToString();
        }
        
        public StreamDeckButtonNames StreamDeckButtonName
        {
            get => _streamDeckButtonName;
            set => _streamDeckButtonName = value;
        }

        public string LayerName
        {
            get => _layerName;
            set => _layerName = value;
        }
    }
}
