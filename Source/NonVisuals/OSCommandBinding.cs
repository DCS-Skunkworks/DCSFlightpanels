using System;

namespace NonVisuals
{
    [Serializable]
    public abstract class OSCommandBinding
    {
        /*
         This is the base class for all the OSCommand binding classes
         that binds a physical switch to a Windows OS command.
         */
        private bool _whenOnTurnedOn = true;
        
        private OSCommand _operatingSystemCommand;

        internal abstract void ImportSettings(string settings);
        
        public abstract string ExportSettings();

        public int GetHash()
        {
            unchecked
            {
                var result = _whenOnTurnedOn.GetHashCode();
                result = (result * 397) ^ (_operatingSystemCommand?.GetHash() ?? 0);
                return result;
            }
        }

        public bool WhenTurnedOn
        {
            get => _whenOnTurnedOn;
            set => _whenOnTurnedOn = value;
        }

        public OSCommand OSCommandObject
        {
            get => _operatingSystemCommand;
            set => _operatingSystemCommand = value;
        }
    }
}

