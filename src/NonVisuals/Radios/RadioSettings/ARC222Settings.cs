using NonVisuals.Helpers;
using NonVisuals.Interfaces;
using NonVisuals.Radios.RadioControls;


namespace NonVisuals.Radios.RadioSettings
{
    public class ARC222Settings : IFlightRadioSettings
    {
        public FlightRadioSettings RadioSettings { get; init; }

        public void VerifySettings()
        {
            RadioSettings.VerifySettings();
        }
        
        /*VHF  AN/ARC-222*/
        /*108.000 - 151.975 MHz*/

        public ARC222Settings(string dcsbiosIdentifier)
        {
            var supportedFrequencyBands = new[] { FlightRadioFrequencyBand.VHF1 };
            var lowIntegerFrequencyBounds = new uint[] { 0, 108, 0, 0 };
            var highIntegerFrequencyBounds = new uint[] { 0, 151, 0, 0 };
            var lowDecimalFrequencyBounds = new uint[] { 0, 0, 0, 0 };
            var highDecimalFrequencyBounds = new uint[] { 0, 975, 0, 0 };
            var integerChangeRates = new uint[] { 0, 1, 0, 0 };
            var integerHighChangeRates = new uint[] { 0, 10, 0, 0 };

            var decimalChangeRates = new uint[] { 0, 25, 0, 0 };
            var decimalHighChangeRates = new uint[] { 0, 25, 0, 0 };
            var skipCountForFrequencyBand = 2;
            var integerFrequencySkippers = new[] { new ClickSkipper(1), new ClickSkipper(1), new ClickSkipper(1), new ClickSkipper(1) };

            RadioSettings = new FlightRadioSettings(
                FlightRadioFrequencyBand.VHF1,
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
