using System;
using System.Threading;
using ClassLibraryCommon;
using NonVisuals.Interfaces;

namespace NonVisuals.StreamDeck
{
    public class OSCommandBindingStreamDeck : OSCommandBinding, IStreamDeckButtonAction
    {
        public EnumStreamDeckActionType ActionType => EnumStreamDeckActionType.OSCommand;









        public void Execute(CancellationToken cancellationToken)
        {
            OSCommandObject.Execute(cancellationToken);
        }

        internal override void ImportSettings(string settings) { }

        public override string ExportSettings()
        {
            return null;
        }
    }
}
