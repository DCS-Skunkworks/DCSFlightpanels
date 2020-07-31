using ClassLibraryCommon;
using NonVisuals;

namespace DCSFlightpanels.Interfaces
{
    internal interface IGamingPanelUserControl
    {
        GamingPanel GetGamingPanel();
        GamingPanelEnum GetPanelType();
        string GetName();
    }
}
