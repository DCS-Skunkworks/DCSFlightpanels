using NonVisuals.Helpers;
using NonVisuals.Interfaces;
using NonVisuals.Radios.RadioControls;

namespace NonVisuals.Radios.RadioSettings
{
    /// <summary>
    /// Single Band radio configuration for the FlightRadio class.
    /// </summary>
    internal class SingleBandRadioSettings : IFlightRadioSettings
    {
        public void VerifySettings()
        {
            RadioSettings.VerifySettings();
        }

        public FlightRadioSettings RadioSettings { get; init; }

        public SingleBandRadioSettings(
            string dcsbiosIdentifier, 
            FlightRadioFrequencyBand frequencyBand, 
            uint lowIntegerFrequency,
            uint highIntegerFrequency,
            uint lowDecimalFrequency,
            uint highDecimalFrequency,
            uint integerChangeRate = 1,
            uint integerHighChangeRate = 10,
            uint decimalChangeRate = 25,
            uint decimalHighChangeRate = 25,
            int integerSkipCount = 2)
        {
            var supportedFrequencyBands = new []{ frequencyBand };
            var lowIntegerFrequencyBounds = new uint[] { 0, 0, 0, 0 };
            var highIntegerFrequencyBounds = new uint[] { 0, 0, 0, 0 };
            var lowDecimalFrequencyBounds = new uint[] { 0, 0, 0, 0 };
            var highDecimalFrequencyBounds = new uint[] { 0, 0, 0, 0 };
            var integerChangeRates = new uint[] {0, 0, 0, 0 };
            var integerHighChangeRates = new uint[] { 0, 0, 0, 0 };
            var decimalChangeRates = new uint[] { 0, 0, 0, 0 };
            var decimalHighChangeRates = new uint[] { 0, 0, 0, 0 };
            var skipCountForFrequencyBand = 2;
            var integerFrequencySkippers = new[] { new ClickSkipper(integerSkipCount), new ClickSkipper(integerSkipCount), new ClickSkipper(integerSkipCount), new ClickSkipper(integerSkipCount) };

            lowIntegerFrequencyBounds[(int)frequencyBand] = lowIntegerFrequency;
            highIntegerFrequencyBounds[(int)frequencyBand] = highIntegerFrequency;
            lowDecimalFrequencyBounds[(int)frequencyBand] = lowDecimalFrequency;
            highDecimalFrequencyBounds[(int)frequencyBand] = highDecimalFrequency;

            integerChangeRates[(int)frequencyBand] = integerChangeRate;
            integerHighChangeRates[(int)frequencyBand] = integerHighChangeRate;
            decimalChangeRates[(int)frequencyBand] = decimalChangeRate;
            decimalHighChangeRates[(int)frequencyBand] = decimalHighChangeRate;

            RadioSettings = new FlightRadioSettings(
                frequencyBand,
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
