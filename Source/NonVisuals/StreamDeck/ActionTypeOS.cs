using System;
using System.Text;
using System.Threading;
using Newtonsoft.Json;
using NonVisuals.Interfaces;

namespace NonVisuals.StreamDeck
{

    [Serializable]
    public class ActionTypeOS : OSCommandBinding, IStreamDeckButtonTypeBase, IStreamDeckButtonAction
    {
        public EnumStreamDeckActionType ActionType => EnumStreamDeckActionType.OSCommand;
        public bool IsRepeatable() => true;
        private EnumStreamDeckButtonNames _streamDeckButtonName;
        private string _panelHash;



        public new int GetHash()
        {
            unchecked
            {
                var result = OSCommandObject.GetHash();
                result = (result * 397) ^ _streamDeckButtonName.GetHashCode();
                return result;
            }
        }
        
        public string ActionDescription
        {
            get
            {
                var stringBuilder = new StringBuilder(100);
                stringBuilder.Append("OS Command");
                if (OSCommandObject != null)
                {
                    stringBuilder.Append(" ").Append(OSCommandObject.Name);
                }

                return stringBuilder.ToString();
            }
        }

        public bool IsRunning()
        {
            return OSCommandObject.IsRunning();
        }


        public void Execute(CancellationToken threadCancellationToken)
        {
            OSCommandObject.Execute(threadCancellationToken);
        }

        internal override void ImportSettings(string settings) { }

        public override string ExportSettings()
        {
            return null;
        }
        public EnumStreamDeckButtonNames StreamDeckButtonName
        {
            get => _streamDeckButtonName;
            set => _streamDeckButtonName = value;
        }

        [JsonIgnore]
        public string PanelHash
        {
            get => _panelHash;
            set => _panelHash = value;
        }
    }
}
