using DCSFlightpanels.CustomControls;
using NonVisuals.Saitek;

namespace DCSFlightpanels.Interfaces
{
    public interface IPanelUIPZ55
    {
        SwitchPanelPZ55KeyOnOff GetPZ55Key(PZ55TextBox textBox);
        PZ55TextBox GetTextBox(SwitchPanelPZ55Keys key, bool whenTurnedOn);
    }
}
