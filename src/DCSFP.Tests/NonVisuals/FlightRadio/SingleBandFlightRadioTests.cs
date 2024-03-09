using NonVisuals.Radios.RadioControls;
using NonVisuals.Radios.RadioSettings;
using Xunit;

namespace DCSFP.Tests.NonVisuals.FlightRadio
{
    public class SingleBandFlightRadioTests
    {
        [Theory]
        [InlineData("VHF_RADIO", FlightRadioFrequencyBand.VHF1, 118, 173, 0, 975, 1, 1, 25, 25, 0)]
        internal void Single_Band_FlightRadio_InitPanel_No_Exception(
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

            var flightRadio = new global::NonVisuals.Radios.RadioControls.FlightRadio(singleBandRadioSettings.RadioSettings);
            var exception = Record.Exception(() => flightRadio.InitRadio());
            Assert.Null(exception);
        }

        [Theory]
        [InlineData(FlightRadioFrequencyBand.VHF1, "VHF_RADIO", FlightRadioFrequencyBand.VHF1, 118, 173, 0, 975, 1, 1, 25, 25, 0)]
        internal void Single_Band_FlightRadio_Frequency_Band_Up(
            FlightRadioFrequencyBand result,
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

            var flightRadio = new global::NonVisuals.Radios.RadioControls.FlightRadio(singleBandRadioSettings.RadioSettings);
            flightRadio.InitRadio();
            flightRadio.TemporaryFrequencyBandUp();
            flightRadio.SwitchFrequencyBand();
            Assert.Equal(result, flightRadio.ActiveFrequencyBand);
        }

        [Theory]
        [InlineData(FlightRadioFrequencyBand.VHF1, "VHF_RADIO", FlightRadioFrequencyBand.VHF1, 118, 173, 0, 975, 1, 1, 25, 25, 0)]
        internal void Single_Band_FlightRadio_Frequency_Band_Down(
            FlightRadioFrequencyBand result,
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

            var flightRadio = new global::NonVisuals.Radios.RadioControls.FlightRadio(singleBandRadioSettings.RadioSettings);
            flightRadio.InitRadio();
            flightRadio.TemporaryFrequencyBandDown();
            flightRadio.SwitchFrequencyBand();
            Assert.Equal(result, flightRadio.ActiveFrequencyBand);
        }

        [Theory]
        [InlineData("118.000", "VHF_RADIO", FlightRadioFrequencyBand.VHF1, 118, 173, 0, 975, 1, 1, 25, 25, 0)]
        internal void Single_Band_FlightRadio_Check_Initial_Cockpit_Frequency(
            string result,
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

            var flightRadio = new global::NonVisuals.Radios.RadioControls.FlightRadio(singleBandRadioSettings.RadioSettings);
            flightRadio.InitRadio();
            flightRadio.TemporaryFrequencyBandDown();
            flightRadio.SwitchFrequencyBand();
            Assert.Equal(result, flightRadio.CockpitFrequency);
        }

        [Theory]
        [InlineData("118.000", "VHF_RADIO", FlightRadioFrequencyBand.VHF1, 118, 173, 0, 975, 1, 1, 25, 25, 0)]
        internal void Single_Band_FlightRadio_Check_Initial_StandBy_Frequency(
            string result,
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

            var flightRadio = new global::NonVisuals.Radios.RadioControls.FlightRadio(singleBandRadioSettings.RadioSettings);
            flightRadio.InitRadio();
            flightRadio.TemporaryFrequencyBandDown();
            flightRadio.SwitchFrequencyBand();
            Assert.Equal(result, flightRadio.StandbyFrequency);
        }

        [Theory]
        [InlineData("119.000", "VHF_RADIO", FlightRadioFrequencyBand.VHF1, 118, 173, 0, 975, 1, 1, 25, 25, 0)]
        [InlineData("226.000", "UHF_RADIO", FlightRadioFrequencyBand.UHF, 225, 399, 0, 975, 1, 1, 25, 25, 0)]
        internal void Single_Band_FlightRadio_Integer_Frequency_Up(
            string result,
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

            var flightRadio = new global::NonVisuals.Radios.RadioControls.FlightRadio(singleBandRadioSettings.RadioSettings);
            flightRadio.InitRadio();
            flightRadio.IntegerFrequencyUp();
            Assert.Equal(result, flightRadio.StandbyFrequency);
        }

        [Theory]
        [InlineData("118.025", "VHF_RADIO", FlightRadioFrequencyBand.VHF1, 118, 173, 0, 975, 1, 1, 25, 25, 0)]
        [InlineData("225.025", "UHF_RADIO", FlightRadioFrequencyBand.UHF, 225, 399, 0, 975, 1, 1, 25, 25, 0)]
        internal void Single_Band_FlightRadio_Decimal_Frequency_Up(
            string result,
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

            var flightRadio = new global::NonVisuals.Radios.RadioControls.FlightRadio(singleBandRadioSettings.RadioSettings);
            flightRadio.InitRadio();
            flightRadio.DecimalFrequencyUp();
            Assert.Equal(result, flightRadio.StandbyFrequency);
        }

        [Theory]
        [InlineData("173.000", "VHF_RADIO", FlightRadioFrequencyBand.VHF1, 118, 173, 0, 975, 1, 1, 25, 25, 0)]
        [InlineData("399.000", "UHF_RADIO", FlightRadioFrequencyBand.UHF, 225, 399, 0, 975, 1, 1, 25, 25, 0)]
        internal void Single_Band_FlightRadio_Integer_Frequency_Down_Cross_Lower_Bound(
            string result,
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

            var flightRadio = new global::NonVisuals.Radios.RadioControls.FlightRadio(singleBandRadioSettings.RadioSettings);
            flightRadio.InitRadio();
            flightRadio.IntegerFrequencyDown();
            Assert.Equal(result, flightRadio.StandbyFrequency);
        }

        [Theory]
        [InlineData("118.975", "VHF_RADIO", FlightRadioFrequencyBand.VHF1, 118, 173, 0, 975, 1, 1, 25, 25, 0)]
        [InlineData("225.975", "UHF_RADIO", FlightRadioFrequencyBand.UHF, 225, 399, 0, 975, 1, 1, 25, 25, 0)]
        internal void Single_Band_FlightRadio_Decimal_Frequency_Down_Cross_Lower_Bound(
            string result,
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

            var flightRadio = new global::NonVisuals.Radios.RadioControls.FlightRadio(singleBandRadioSettings.RadioSettings);
            flightRadio.InitRadio();
            flightRadio.DecimalFrequencyDown();
            Assert.Equal(result, flightRadio.StandbyFrequency);
        }

        [Theory]
        [InlineData("118.000", "173.000", "VHF_RADIO", FlightRadioFrequencyBand.VHF1, 118, 173, 0, 975, 1, 1, 25, 25, 0)]
        [InlineData("225.000", "399.000", "UHF_RADIO", FlightRadioFrequencyBand.UHF, 225, 399, 0, 975, 1, 1, 25, 25, 0)]
        internal void Single_Band_FlightRadio_Integer_Frequency_Up_Cross_Higher_Bound(
            string result,
            string newFrequency,
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

            var flightRadio = new global::NonVisuals.Radios.RadioControls.FlightRadio(singleBandRadioSettings.RadioSettings);
            flightRadio.InitRadio();
            flightRadio.SetCockpitFrequency(newFrequency);
            flightRadio.SetCockpitFrequency(result);//now standby frequency = newFrequency
            flightRadio.IntegerFrequencyUp();
            Assert.Equal(result, flightRadio.StandbyFrequency);
        }

        [Theory]
        [InlineData("173.000", "173.975", "VHF_RADIO", FlightRadioFrequencyBand.VHF1, 118, 173, 0, 975, 1, 1, 25, 25, 0)]
        [InlineData("399.000", "399.975", "UHF_RADIO", FlightRadioFrequencyBand.UHF, 225, 399, 0, 975, 1, 1, 25, 25, 0)]
        internal void Single_Band_FlightRadio_Decimal_Frequency_Up_Cross_Higher_Bound(
            string result,
            string newFrequency,
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

            var flightRadio = new global::NonVisuals.Radios.RadioControls.FlightRadio(singleBandRadioSettings.RadioSettings);
            flightRadio.InitRadio(); 
            flightRadio.SetCockpitFrequency(newFrequency);
            flightRadio.SetCockpitFrequency(result);//now standby frequency = newFrequency
            flightRadio.DecimalFrequencyUp();
            Assert.Equal(result, flightRadio.StandbyFrequency);
        }



    }
}