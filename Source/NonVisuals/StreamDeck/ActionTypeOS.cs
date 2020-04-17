using System.Threading;
using NonVisuals.Interfaces;

namespace NonVisuals.StreamDeck
{
    public class ActionTypeOS : OSCommandBinding, IStreamDeckButtonTypeBase, IStreamDeckButtonAction
    {
        public EnumStreamDeckActionType ActionType => EnumStreamDeckActionType.OSCommand;
        public bool IsRepeatable() => true;
        private EnumStreamDeckButtonNames _streamDeckButtonName;
        private string _streamDeckInstanceId;




        public string Description { get => "OS Command"; }

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
        
        public string StreamDeckInstanceId
        {
            get => _streamDeckInstanceId;
            set => _streamDeckInstanceId = value;
        }
    }
}
