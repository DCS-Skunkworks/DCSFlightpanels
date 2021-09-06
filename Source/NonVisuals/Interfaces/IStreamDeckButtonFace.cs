using NonVisuals.StreamDeck;

namespace NonVisuals.Interfaces
{
    using MEF;

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
        StreamDeckPanel StreamDeckPanelInstance { get; set; }
        EnumStreamDeckButtonNames StreamDeckButtonName { get; set; }
        bool IsVisible { get; set; }
        void Dispose();
        bool ConfigurationOK { get; }
        string FaceDescription { get; }
        int GetHash();
        void AfterClone();
    }
}
