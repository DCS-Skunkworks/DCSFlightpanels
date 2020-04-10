using System.Drawing;
using NonVisuals.StreamDeck;

namespace NonVisuals.Interfaces
{

    public enum EnumStreamDeckFaceType
    {
        Unknown = 0,
        Text = 1,
        Image = 2,
        DCSBIOS = 4
    }


    public interface IStreamDeckButtonFace
    {
        EnumStreamDeckFaceType FaceType { get; }
        Font TextFont { get; set; }
        Color FontColor { get; set; }
        Color BackgroundColor { get; set; }
        int OffsetX { get; set; }
        int OffsetY { get; set; }
        void Show(StreamDeckRequisites streamDeckRequisites);
    }
}
