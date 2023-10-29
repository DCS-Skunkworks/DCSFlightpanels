using System;
using NonVisuals.Radios.RadioControls;
using NonVisuals.Radios.RadioSettings;
using Xunit;

namespace NonVisuals.Tests.FlightRadioSettings.Generic
{
    public class SingleBandRadioSettingsTests
    {
        [Theory]
        [InlineData("VHF_RADIO", FlightRadioFrequencyBand.VHF1, 118, 173, 0, 975, 1, 1, 25, 25, 0)]
        internal void Single_Band_Radio_Settings_Verify_Settings_No_Exception(
            string dcsbiosIdentifier,
            FlightRadioFrequencyBand frequencyBand,
            uint lowIntegerFrequency,
            uint highIntegerFrequency,
            uint lowDecimalFrequency,
            uint highDecimalFrequency,
            uint integerChangeRate,
            uint integerHighChangeRate,
            uint decimalChangeRate,
            uint decimalHighChangeRate,
            int integerSkipCount)
        {
            var singleBandRadioSettings = new SingleBandRadioSettings(
                dcsbiosIdentifier,
                frequencyBand,
                lowIntegerFrequency,
                highIntegerFrequency,
                lowDecimalFrequency,
                highDecimalFrequency,
                integerChangeRate,
                integerHighChangeRate,
                decimalChangeRate,
                decimalHighChangeRate,
                integerSkipCount);

            var exception = Record.Exception(() => singleBandRadioSettings.VerifySettings());
            Assert.Null(exception);
        }

        [Theory]
        [InlineData("VHF_RADIO", FlightRadioFrequencyBand.VHF1, 173, 118, 0, 975, 1, 1, 25, 25, 0)]
        [InlineData("VHF_RADIO", FlightRadioFrequencyBand.VHF1, 118, 173, 975, 5, 1, 1, 25, 25, 0)]
        internal void Single_Band_Radio_Settings_Invalid_Low_High_Integer_Frequencies(
            string dcsbiosIdentifier,
            FlightRadioFrequencyBand frequencyBand,
            uint lowIntegerFrequency,
            uint highIntegerFrequency,
            uint lowDecimalFrequency,
            uint highDecimalFrequency,
            uint integerChangeRate,
            uint integerHighChangeRate,
            uint decimalChangeRate,
            uint decimalHighChangeRate,
            int integerSkipCount)
        {
            var singleBandRadioSettings = new SingleBandRadioSettings(
                dcsbiosIdentifier,
                frequencyBand,
                lowIntegerFrequency,
                highIntegerFrequency,
                lowDecimalFrequency,
                highDecimalFrequency,
                integerChangeRate,
                integerHighChangeRate,
                decimalChangeRate,
                decimalHighChangeRate,
                integerSkipCount);

            Assert.Throws<ArgumentOutOfRangeException>(() => singleBandRadioSettings.VerifySettings());
        }

        [Theory]
        [InlineData("VHF_RADIO", FlightRadioFrequencyBand.VHF1, 118, 173, 0, 975, 0, 1, 25, 25, 0)]
        [InlineData("VHF_RADIO", FlightRadioFrequencyBand.VHF1, 118, 173, 0, 975, 1, 0, 25, 25, 0)]
        [InlineData("VHF_RADIO", FlightRadioFrequencyBand.VHF1, 118, 173, 0, 975, 1, 1, 0, 25, 0)]
        [InlineData("VHF_RADIO", FlightRadioFrequencyBand.VHF1, 118, 173, 0, 975, 1, 1, 25, 0, 0)]
        internal void Single_Band_Radio_Settings_Invalid_Change_Rates(
            string dcsbiosIdentifier,
            FlightRadioFrequencyBand frequencyBand,
            uint lowIntegerFrequency,
            uint highIntegerFrequency,
            uint lowDecimalFrequency,
            uint highDecimalFrequency,
            uint integerChangeRate,
            uint integerHighChangeRate,
            uint decimalChangeRate,
            uint decimalHighChangeRate,
            int integerSkipCount)
        {
            var singleBandRadioSettings = new SingleBandRadioSettings(
                dcsbiosIdentifier,
                frequencyBand,
                lowIntegerFrequency,
                highIntegerFrequency,
                lowDecimalFrequency,
                highDecimalFrequency,
                integerChangeRate,
                integerHighChangeRate,
                decimalChangeRate,
                decimalHighChangeRate,
                integerSkipCount);

            Assert.Throws<ArgumentOutOfRangeException>(() => singleBandRadioSettings.VerifySettings());
        }
    }
}