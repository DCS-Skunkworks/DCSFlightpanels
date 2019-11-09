using NonVisuals.Interfaces;

namespace NonVisuals.StreamDeck.CustomLayers.SRS
{



    public class StreamDeckSRSButton : StreamDeckButton
    {
        private EnumSRSButtonType _srsFunction;
        private ButtonFunction _buttonFunction;
        private SRSLayer _parentLayer;

        public StreamDeckSRSButton(SRSLayer parentLayer, EnumStreamDeckButtonNames streamDeckButtonName, ButtonFunction buttonFunction) : base(streamDeckButtonName)
        {
            _parentLayer = parentLayer;
            _buttonFunction = buttonFunction;
            _srsFunction = Type();
            
            ActionForPress = new SRSAction(buttonFunction.Description, _srsFunction,  (ISRSHandler)_parentLayer);
        }
        
        public EnumSRSButtonType SRSFunction
        {
            get => _srsFunction;
            set => _srsFunction = value;
        }

        public ButtonFunction ButtonFunction
        {
            get => _buttonFunction;
            set => _buttonFunction = value;
        }

        public SRSLayer ParentLayer
        {
            get => _parentLayer;
            set => _parentLayer = value;
        }

        private EnumSRSButtonType Type()
        {
            return (EnumSRSButtonType) _buttonFunction.Id;
        }
    }
}
