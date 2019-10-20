using System.Windows.Forms;
using DCS_BIOS;
using NonVisuals.DCSBIOSBindings;
using NonVisuals.StreamDeck;

namespace NonVisuals.Interfaces
{

    public enum EnumStreamDeckButtonActionType
    {
        Unknown = 0,
        KeyPress = 1,
        DCSBIOS = 2,
        OSCommand = 4,
        SRS = 8,
        LayerNavigation = 16
    }

    public interface IStreamDeckButtonAction
    {
        EnumStreamDeckButtonActionType ActionType { get; }
        void Execute();
        //void SetAction(object action);
        //object GetAction();

        /*string ExportJSON();
        void ImportJSON(string json);*/




    }
}
