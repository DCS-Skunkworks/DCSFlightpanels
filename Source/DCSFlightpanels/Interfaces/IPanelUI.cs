using System.Windows.Controls;
using NonVisuals.Panels.Saitek;

namespace DCSFlightpanels.Interfaces
{
    public interface IPanelUI
    {
        PanelSwitchOnOff GetSwitch(TextBox textBox);
        TextBox GetTextBox(object panelSwitch, bool whenTurnedOn);
    }
}
