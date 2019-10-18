using NonVisuals;
using NonVisuals.Saitek;

namespace DCSFlightpanels
{
    interface ISaitekUserControl
    {
        SaitekPanel GetSaitekPanel();
        string GetName();
    }
}
