namespace NonVisuals.Interfaces
{
    using System.Threading;
    using NonVisuals.Panels.StreamDeck.Panels;

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
        void Dispose();

        EnumStreamDeckActionType ActionType { get; }

        string ActionDescription { get; }

        void Execute(CancellationToken threadCancellationToken, bool executeOnce = false);

        bool IsRunning();

        /// <summary>
        /// If Action can be repeated by holding in the button. This does not apply
        /// when the Action has been configured with a sequence of keys or DCS-BIOS controls.
        /// </summary>
        /// <returns></returns>
        bool IsRepeatable();

        /// <summary>
        /// Whether it has a sequence of commands configured instead of just one.
        /// </summary>
        bool HasSequence { get; }

        /// <summary>
        /// Whether the commands should be executed in a sequenced fashion => each press
        /// moves to the next command until last command is reached and then first command
        /// is executed again.
        /// </summary>
        bool IsSequenced { get; }

        StreamDeckPanel StreamDeckPanelInstance { get; set; }

        int GetHash();
        
        string SoundFile { get; set; }

        double Volume { get; set; }

        int Delay { get; set; }

        bool HasSound { get; }

        void PlaySound();
    }
}
