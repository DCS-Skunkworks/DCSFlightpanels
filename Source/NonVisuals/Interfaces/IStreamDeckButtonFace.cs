using NonVisuals.StreamDeck;

namespace NonVisuals.Interfaces
{

    public enum EnumStreamDeckFaceType
    {
        Unknown = 0,
        Text = 1,
        Image = 2,
        DCSBIOS = 4,
        DCSBIOSOverlay = 8
    }


    public interface IStreamDeckButtonFace
    {
        EnumStreamDeckFaceType FaceType { get; }
        string PanelHash { get; set; }
        EnumStreamDeckButtonNames StreamDeckButtonName { get; set; }
        bool IsVisible { get; set; }
        void Dispose();
        bool ConfigurationOK { get; }
        string FaceDescription { get; }
        int GetHash();
    }
}
