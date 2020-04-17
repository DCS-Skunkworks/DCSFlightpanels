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
        string StreamDeckInstanceId { get; set; }
        EnumStreamDeckButtonNames StreamDeckButtonName { get; set; }
        void Show();
    }
}
