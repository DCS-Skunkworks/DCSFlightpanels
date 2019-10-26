using NonVisuals;
using NonVisuals.Saitek;

namespace DCSFlightpanels
{
    interface IGamingPanelUserControl
    {
        GamingPanel GetGamingPanel();
        string GetName();
    }
}
