using NonVisuals.Helpers;
using NonVisuals.Interfaces;
using NonVisuals.Radios.RadioControls;

namespace NonVisuals.Radios.RadioSettings
{
    internal class ARC210Settings : IFlightRadioSettings
    {
        public void VerifySettings()
        {
            RadioSettings.VerifySettings();
        }

        public FlightRadioSettings RadioSettings { get; init; }

        /* (FM) 30.000 to 87.975 MHz */
        /* VHF AM 108.000 to 115.975 MHz */
        /* VHF AM 118.000 to 173.975 MHz */
        /* UHF AM 225.000 to 399.975 MHz */
        public ARC210Settings(string dcsbiosIdentifier)
        {
            var supportedFrequencyBands = new [] { FlightRadioFrequencyBand.HF, FlightRadioFrequencyBand.VHF1, FlightRadioFrequencyBand.VHF2, FlightRadioFrequencyBand.UHF };
            var lowIntegerFrequencyBounds = new uint[] { 30, 108, 118, 225 };
            var highIntegerFrequencyBounds = new uint[] { 87, 115, 173, 399 };
            var lowDecimalFrequencyBounds = new uint[] { 0, 0, 0, 0 };
            var highDecimalFrequencyBounds = new uint[] { 975, 975, 975, 975 };
            var integerChangeRates = new uint[] { 1, 1, 1, 1 };
            var integerHighChangeRates = new uint[] { 5, 5, 10, 10 };
            var decimalChangeRates = new uint[] { 25, 25, 25, 25 };
            var decimalHighChangeRates = new uint[] { 25, 25, 25, 25 };
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
