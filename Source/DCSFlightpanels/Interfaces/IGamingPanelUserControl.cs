using ClassLibraryCommon;
using NonVisuals.Panels;

namespace DCSFlightpanels.Interfaces
{
    internal interface IGamingPanelUserControl
    {
        void Dispose();
        GamingPanel GetGamingPanel();
        GamingPanelEnum GetPanelType();
        string GetName();
    }
}
