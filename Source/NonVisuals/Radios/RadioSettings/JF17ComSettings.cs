using NonVisuals.Helpers;
using NonVisuals.Radios.RadioControls;

namespace NonVisuals.Radios.RadioSettings
{
    internal class JF17ComSettings
    {
        internal FlightRadioSettings RadioSettings { get; init; }

        /* 108.000 to 173.975 MHz  225.000 to 399.975 MHz */
        public JF17ComSettings(string dcsbiosIdentifier)
        {
            var supportedFrequencyBands = new []{ FlightRadioFrequencyBand.VHF2, FlightRadioFrequencyBand.UHF };
            var lowIntegerFrequencyBounds = new uint[] { 0, 0, 108, 225 };
            var highIntegerFrequencyBounds = new uint[] { 0, 0, 173, 399 };
            var lowDecimalFrequencyBounds = new uint[] { 0, 0, 0, 0 };
            var highDecimalFrequencyBounds = new uint[] { 0, 0, 975, 975 };
            var integerChangeRates = new uint[] {0, 0, 1, 1 };
            var integerHigherChangeRates = new uint[] { 0, 0, 10, 10 };
            var decimalChangeRates = new uint[] { 0, 0, 25, 25 };
            var decimalHigherChangeRates = new uint[] { 0, 0, 25, 25 };
            var skipCountForFrequencyBand = 2;
            var integerFrequencySkippers = new[] { new ClickSkipper(1), new ClickSkipper(1), new ClickSkipper(1), new ClickSkipper(1) };

            RadioSettings = new FlightRadioSettings(
                supportedFrequencyBands,
                dcsbiosIdentifier,
                lowIntegerFrequencyBounds,
                highIntegerFrequencyBounds,
                lowDecimalFrequencyBounds,
                highDecimalFrequencyBounds,
                integerChangeRates,
                integerHigherChangeRates,
                decimalChangeRates,
                decimalHigherChangeRates,
                skipCountForFrequencyBand,
                integerFrequencySkippers);
        }
    }
}
