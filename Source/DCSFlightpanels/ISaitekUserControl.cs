using NonVisuals;

namespace DCSFlightpanels
{
    interface ISaitekUserControl
    {
        SaitekPanel GetSaitekPanel();
        string GetName();
    }
}
