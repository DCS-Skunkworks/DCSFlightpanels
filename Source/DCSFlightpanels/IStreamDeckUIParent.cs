using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NonVisuals;
using NonVisuals.StreamDeck;

namespace DCSFlightpanels
{
    public interface IStreamDeckUIParent
    {

        int GetButtonNumber();
        StreamDeckButtons GetButton();
        EnumStreamDeckButtonActionType GetButtonActionType();
    }
}
