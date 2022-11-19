﻿namespace NonVisuals.Interfaces
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

        void Execute(CancellationToken threadCancellationToken);

        bool IsRunning();

        bool IsRepeatable();

        StreamDeckPanel StreamDeckPanelInstance { get; set; }

        int GetHash();
        
        string SoundFile { get; set; }

        double Volume { get; set; }

        int Delay { get; set; }

        bool HasSound { get; }

        void PlaySound();
    }
}
