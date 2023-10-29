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

        [Fact]
        internal void ARC210_No_DCSBIOS()
        {
            var arc210Settings = new ARC210Settings("");
            Assert.Throws<ArgumentOutOfRangeException>( () => arc210Settings.VerifySettings());
        }
    }
}