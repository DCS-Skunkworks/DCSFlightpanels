using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NonVisuals.StreamDeck.CustomLayers
{



    public class StreamDeckSRSButton : StreamDeckButton
    {
        private EnumSRSButtonType _srsFunction;
        private ButtonFunction _buttonFunction;

        public StreamDeckSRSButton(EnumStreamDeckButtonNames enumStreamDeckButton) : base(enumStreamDeckButton)
        {
        }

        public new void DoPress(StreamDeckRequisites streamDeckRequisites)
        {

        }

        public new void DoRelease(StreamDeckRequisites streamDeckRequisites)
        {

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

        private EnumSRSButtonType Type()
        {
            return (EnumSRSButtonType) _buttonFunction.Id;
        }
    }
}
