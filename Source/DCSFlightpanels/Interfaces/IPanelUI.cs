using System.Windows.Controls;
using NonVisuals.Saitek;

namespace DCSFlightpanels.Interfaces
{
    public interface IPanelUI
    {
        PanelSwitchOnOff GetSwitch(TextBox textBox);
        TextBox GetTextBox(object panelSwitch, bool whenTurnedOn);
    }
}
