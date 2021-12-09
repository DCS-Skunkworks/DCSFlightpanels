using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NonVisuals.StreamDeck.Events;

namespace NonVisuals.Interfaces
{
    public interface IStreamDeckButtonActionListener
    {
        void ActionTypeChangedEvent(object sender, ActionTypeChangedEventArgs e);
    }
}
