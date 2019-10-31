using NonVisuals.StreamDeck;
using StreamDeckSharp;

namespace NonVisuals.Interfaces
{

    public enum EnumStreamDeckFaceType
    {
        Unknown = 0,
        Text = 1,
        ImageFile = 2,
        DCSBIOS = 4
    }


    public interface IStreamDeckButtonFace
    {
        EnumStreamDeckFaceType FaceType { get; }
        void Show(StreamDeckRequisites streamDeckRequisites);
    }
}
