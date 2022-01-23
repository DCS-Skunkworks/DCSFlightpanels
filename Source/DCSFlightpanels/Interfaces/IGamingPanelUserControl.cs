using ClassLibraryCommon;
using NonVisuals;

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
