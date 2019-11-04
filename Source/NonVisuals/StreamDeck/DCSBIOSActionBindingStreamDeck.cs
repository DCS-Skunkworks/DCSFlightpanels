using System.Threading;
using NonVisuals.DCSBIOSBindings;
using NonVisuals.Interfaces;

namespace NonVisuals.StreamDeck
{
    public class DCSBIOSActionBindingStreamDeck : DCSBIOSActionBindingBase, IStreamDeckButtonAction
    {
        public EnumStreamDeckActionType ActionType => EnumStreamDeckActionType.DCSBIOS;
        public bool IsRepeatable() => true;








        public void Execute(StreamDeckRequisites streamDeckRequisite)
        {
            SendDCSBIOSCommands(streamDeckRequisite.ThreadCancellationToken);
        }

        internal override void ImportSettings(string settings) { }

        public override string ExportSettings()
        {
            return null;
        }
    }
}
