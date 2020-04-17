using System.Collections.Generic;
using System.Drawing;
using NonVisuals.StreamDeck;

namespace DCSFlightpanels
{
    public interface IStreamDeckUIParent
    {
        int GetSelectedButtonNumber();
        EnumStreamDeckButtonNames GetSelectedButtonName();
        string GetStreamDeckInstanceId();
        StreamDeckLayer GetUISelectedLayer();
        List<string> GetStreamDeckLayerNames();
        void TestImage(Bitmap bitmap);
        void ChildChangesMade();
    }
}
