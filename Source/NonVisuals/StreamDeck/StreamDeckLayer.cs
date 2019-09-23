using System.Collections.Generic;

namespace NonVisuals.StreamDeck
{
    public class StreamDeckLayer
    {
        private bool _isActive = false;
        private bool _isHomeLayer = false;
        private string _name = "";
        //private List<StreamDeckButton> _buttons = new List<StreamDeckButton>();

        public bool IsActive
        {
            get => _isActive;
            set => _isActive = value;
        }


        public bool IsHomeLayer
        {
            get => _isHomeLayer;
            set => _isHomeLayer = value;
        }

        public string Name
        {
            get => _name;
            set => _name = value;
        }

        /*
        public List<StreamDeckButton> Buttons
        {
            get => _buttons;
            set => _buttons = value;
        }
        */
    }
}
