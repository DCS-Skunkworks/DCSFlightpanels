using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NonVisuals;
using NonVisuals.Interfaces;
using NonVisuals.StreamDeck;

namespace DCSFlightpanels
{
    public interface IStreamDeckUIParent
    {

        int GetButtonNumber();
        StreamDeckButtonNames GetButton();
        StreamDeckLayer GetSelectedStreamDeckLayer();
        List<string> GetStreamDeckLayerNames();
        EnumStreamDeckButtonActionType GetButtonActionType();
        void ChildChangesMade();
    }
}
