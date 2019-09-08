using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NonVisuals.StreamDeck
{
    interface IStreamDeckButtonFace{
        void BindButton(StreamDeckButton streamDeckButton);
        void UpdateButton();

    }
}
