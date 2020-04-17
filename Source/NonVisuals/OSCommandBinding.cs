using NonVisuals.StreamDeck;

namespace NonVisuals
{
    public abstract class OSCommandBinding
    {
        /*
         This is the base class for all the OSCommand binding classes
         that binds a physical switch to a Windows OS command.
         */
        private bool _whenOnTurnedOn = true;
        
        private OSCommand _osCommand;

        internal abstract void ImportSettings(string settings);
        
        public abstract string ExportSettings();

        public bool WhenTurnedOn
        {
            get => _whenOnTurnedOn;
            set => _whenOnTurnedOn = value;
        }

        public OSCommand OSCommandObject
        {
            get => _osCommand;
            set => _osCommand = value;
        }
    }
}

