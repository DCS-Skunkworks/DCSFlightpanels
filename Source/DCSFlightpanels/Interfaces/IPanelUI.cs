using System.Windows.Controls;
using NonVisuals.Saitek;

namespace DCSFlightpanels.Interfaces
{
    public interface IPanelUI
    {
        PanelKeyOnOff GetKey(TextBox textBox);
        TextBox GetTextBox(object key, bool whenTurnedOn);
    }
}
