using System.Collections.Generic;
using System.Drawing;
using DCSFlightpanels.Interfaces;
using DCSFlightpanels.PanelUserControls;
using NonVisuals.StreamDeck;

namespace DCSFlightpanels
{
    public interface IStreamDeckUIParent
    {
        int SelectedButtonNumber { get; }
        EnumStreamDeckButtonNames SelectedButtonName { get; }
        string GetStreamDeckInstanceId();
        StreamDeckLayer GetUISelectedLayer();
        List<string> GetStreamDeckLayerNames();
        void TestImage(Bitmap bitmap);
        void ChildChangesMade();
        UserControlStreamDeckButtonAction ActionPanel { get;}
        UserControlStreamDeckButtonFace FacePanel { get;}
        IStreamDeckUI UIPanel { get;}
        void SetFormState();
    }
}
