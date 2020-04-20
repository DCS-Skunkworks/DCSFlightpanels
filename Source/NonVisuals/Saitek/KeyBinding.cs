using System;

namespace NonVisuals.Saitek
{
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
        
        internal abstract void ImportSettings(string settings);

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
