using System.Threading;
using NonVisuals.DCSBIOSBindings;
using NonVisuals.Interfaces;

namespace NonVisuals.StreamDeck
{
    public class ActionTypeDCSBIOS : DCSBIOSActionBindingBase, IStreamDeckButtonTypeBase, IStreamDeckButtonAction
    {
        public EnumStreamDeckActionType ActionType => EnumStreamDeckActionType.DCSBIOS;
        public bool IsRepeatable() => true;
        private EnumStreamDeckButtonNames _streamDeckButtonName;
        private string _streamDeckInstanceId;







        public void Execute(CancellationToken threadCancellationToken)
        {
            SendDCSBIOSCommands(threadCancellationToken);
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


        public string StreamDeckInstanceId
        {
            get => _streamDeckInstanceId;
            set => _streamDeckInstanceId = value;
        }
    }
}
