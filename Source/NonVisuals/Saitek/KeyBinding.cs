namespace NonVisuals.Saitek
{
    using System;

    using Newtonsoft.Json;

    [Serializable]
    public abstract class KeyBinding
    {
        /*
         This is the base class for all the key binding classes
         that binds a physical switch to a user made virtual 
         keypress in Windows or other functionality.
         */
        private KeyPress _keyPress;
        private bool _whenOnTurnedOn = true;

        public int GetHash()
        {
            unchecked
            {
                var result = _keyPress?.GetHash() ?? 0;
                return result;
            }
        }

        internal abstract void ImportSettings(string settings);

        [JsonProperty("OSKeyPress", Required = Required.Always)]
        public KeyPress OSKeyPress
        {
            get => _keyPress;
            set => _keyPress = value;
        }
        
        public abstract string ExportSettings();

        public bool WhenTurnedOn
        {
            get => _whenOnTurnedOn;
            set => _whenOnTurnedOn = value;
        }

    }
}
