using DCS_BIOS;
using NonVisuals.DCSBIOSBindings;

namespace NonVisuals.StreamDeck
{

    public enum EnumStreamDeckButtonActionType
    {
        Unknown = 0,
        KeyPress = 1,
        DCSBIOS = 2,
        OSCommand = 4,
        LayerNavigation = 8
    }

    public interface IStreamDeckButtonAction
    {
        EnumStreamDeckButtonActionType GetActionType();
        void DoButtonPress();
        /*string ExportJSON();
        void ImportJSON(string json);*/

        void SetKeyPress(KeyPress keyPress);
        KeyPress GetKeyPress();

        void SetDCSBIOSAction(DCSBIOSActionBindingStreamDeck dcsbiosActionBindingStreamDeck);
        DCSBIOSActionBindingStreamDeck GetDCSBIOS();

        void SetOSCommand(OSCommandBindingStreamDeck osCommandBindingStreamDeck);
        OSCommandBindingStreamDeck GetOSCommand();

        void SetStreamDeckLayer(StreamDeckLayer streamDeckLayer);
        StreamDeckLayer GetStreamDeckLayer();
    }
}
