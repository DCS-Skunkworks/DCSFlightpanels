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

        int GetSelectedButtonNumber();
        StreamDeckButtonNames GetSelectedButtonName();
        StreamDeckLayer GetSelectedStreamDeckLayer();
        List<string> GetStreamDeckLayerNames();
        EnumStreamDeckButtonActionType GetSelectedActionType();
        void ChildChangesMade();
    }
}
