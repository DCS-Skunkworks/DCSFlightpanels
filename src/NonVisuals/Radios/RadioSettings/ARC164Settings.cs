using NonVisuals.Helpers;
using NonVisuals.Interfaces;
using NonVisuals.Radios.RadioControls;
using System.Runtime.CompilerServices;


namespace NonVisuals.Radios.RadioSettings
{
    internal class ARC164Settings : IFlightRadioSettings
    {
        public FlightRadioSettings RadioSettings { get; init; }

        public void VerifySettings()
        {
            RadioSettings.VerifySettings();
        }

        /*UHF  AN/ARC-164 */
        /* 225.000 - 399.975 MHz */
        public ARC164Settings(string dcsbiosIdentifier)
        {
            var supportedFrequencyBands = new[] {  FlightRadioFrequencyBand.UHF };
            var lowIntegerFrequencyBounds = new uint[] { 0, 0, 0, 225 };
            var highIntegerFrequencyBounds = new uint[] { 0, 0, 0, 399 };
            var lowDecimalFrequencyBounds = new uint[] { 0, 0, 0, 0 };
            var highDecimalFrequencyBounds = new uint[] { 0, 0, 0, 975 };
            var integerChangeRates = new uint[] { 0, 0, 0, 1 };
            var integerHighChangeRates = new uint[] { 0, 0, 0, 10 };

            var decimalChangeRates = new uint[] { 0, 0, 0, 25 };
            var decimalHighChangeRates = new uint[] { 0, 0, 0, 25 };
            var skipCountForFrequencyBand = 2;
            var integerFrequencySkippers = new[] { new ClickSkipper(1), new ClickSkipper(1), new ClickSkipper(1), new ClickSkipper(1) };

            RadioSettings = new FlightRadioSettings(
                FlightRadioFrequencyBand.UHF,
                supportedFrequencyBands,
                dcsbiosIdentifier,
                lowIntegerFrequencyBounds,
                highIntegerFrequencyBounds,
                lowDecimalFrequencyBounds,
                highDecimalFrequencyBounds,
                integerChangeRates,
                integerHighChangeRates,
                decimalChangeRates,
                decimalHighChangeRates,
                skipCountForFrequencyBand,
                integerFrequencySkippers);
        }
    }
}
