using NonVisuals.Radios.RadioSettings;

namespace NonVisuals.Interfaces
{
    internal interface IFlightRadioSettings
    {
        internal void VerifySettings();
        internal FlightRadioSettings RadioSettings { get; init; }
    }
}
