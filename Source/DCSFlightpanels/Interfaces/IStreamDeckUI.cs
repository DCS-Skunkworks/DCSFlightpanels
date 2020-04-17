using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NonVisuals.StreamDeck;

namespace DCSFlightpanels.Interfaces
{
    public interface IStreamDeckUI
    {
        StreamDeckButton StreamDeckButton { get; }
        EnumStreamDeckButtonNames SelectedButtonName { get; }
        int SelectedButtonNumber { get; }
        void UIShowLayer(string layerName);
        void Clear();
        void UpdateAllButtonsSelectedStatus(EnumStreamDeckButtonNames selectedButtonName);
        void HideAllDotImages();
        void SetButtonGridStatus(bool enabled);
    }
}
