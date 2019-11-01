using System.Collections.Generic;
using System.Drawing;
using NonVisuals.Interfaces;
using NonVisuals.StreamDeck;

namespace DCSFlightpanels
{
    public interface IStreamDeckUIParent
    {
        int GetSelectedButtonNumber();
        EnumStreamDeckButtonNames GetSelectedButtonName();
        StreamDeckLayer GetUISelectedLayer();
        List<string> GetStreamDeckLayerNames();
        EnumStreamDeckActionType GetSelectedActionType();
        void TestImage(Bitmap bitmap);
        void ChildChangesMade();
    }
}
