using System.Threading;
using NonVisuals.StreamDeck;

namespace NonVisuals.Interfaces
{

    public enum EnumStreamDeckActionType
    {
        Unknown = 0,
        KeyPress = 1,
        DCSBIOS = 2,
        OSCommand = 4,
        LayerNavigation = 16
    }

    public interface IStreamDeckButtonAction
    {
        EnumStreamDeckActionType ActionType { get; }
        string ActionDescription { get; }
        void Execute(CancellationToken threadCancellationToken);
        bool IsRunning();
        bool IsRepeatable();
        StreamDeckPanel StreamDeckPanelInstance { get; set; }

        int GetHash();
    }
}
