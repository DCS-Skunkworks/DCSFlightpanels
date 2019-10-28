using System;
using System.Threading;
using ClassLibraryCommon;
using NonVisuals.DCSBIOSBindings;
using NonVisuals.Interfaces;

namespace NonVisuals.StreamDeck
{
    public class DCSBIOSActionBindingStreamDeck : DCSBIOSActionBindingBase, IStreamDeckButtonAction
    {
        public EnumStreamDeckActionType ActionType => EnumStreamDeckActionType.DCSBIOS;









        public void Execute(CancellationToken cancellationToken)
        {
            SendDCSBIOSCommands();
        }


        internal override void ImportSettings(string settings) { }

        public override string ExportSettings()
        {
            return null;
        }
    }
}
