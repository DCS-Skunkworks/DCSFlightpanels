using NonVisuals.Interfaces;

namespace NonVisuals.StreamDeck
{
    public class OSCommandBindingStreamDeck : OSCommandBinding, IStreamDeckButtonAction
    {
        public EnumStreamDeckActionType ActionType => EnumStreamDeckActionType.OSCommand;
        public bool IsRepeatable() => true;





        public string Description { get => "OS Command"; }

        public bool IsRunning()
        {
            return OSCommandObject.IsRunning();
        }


        public void Execute(StreamDeckRequisites streamDeckRequisite)
        {
            OSCommandObject.Execute(streamDeckRequisite.ThreadCancellationToken);
        }

        internal override void ImportSettings(string settings) { }

        public override string ExportSettings()
        {
            return null;
        }
    }
}
