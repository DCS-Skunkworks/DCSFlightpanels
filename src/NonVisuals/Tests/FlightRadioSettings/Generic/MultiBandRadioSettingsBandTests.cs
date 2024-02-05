using NonVisuals.Radios.RadioControls;
using NonVisuals.Radios.RadioSettings;
using Xunit;

namespace NonVisuals.Tests.FlightRadioSettings.Generic
{
    /// <summary>
    /// ARC-210 is a multi-band radio so those settings are used.
    /// Initial frequency band is VHF1.
    /// </summary>
    public class MultiBandRadioSettingsBandTests
    {
        [Theory]
        [InlineData(FlightRadioFrequencyBand.VHF2, "ARC210_RADIO")]
        internal void Multi_Band_Radio_Settings_Move_Frequency_Band_Up(FlightRadioFrequencyBand result, string dcsbiosIdentifier)
        {
            var multiBandRadioSettings = new ARC210Settings(dcsbiosIdentifier);
            multiBandRadioSettings.RadioSettings.FrequencyBandSkipper.ClicksToSkip = 0;

            var flightRadio = new Radios.RadioControls.FlightRadio(multiBandRadioSettings.RadioSettings);
            flightRadio.InitRadio(); 
            
            flightRadio.TemporaryFrequencyBandUp();
            flightRadio.SwitchFrequencyBand();

            Assert.Equal(result, flightRadio.ActiveFrequencyBand);
        }

        [Theory]
        [InlineData(FlightRadioFrequencyBand.HF, "ARC210_RADIO")]
        internal void Multi_Band_Radio_Settings_Move_Frequency_Band_Down(FlightRadioFrequencyBand result, string dcsbiosIdentifier)
        {
            var multiBandRadioSettings = new ARC210Settings(dcsbiosIdentifier);
            multiBandRadioSettings.RadioSettings.FrequencyBandSkipper.ClicksToSkip = 0;

            var flightRadio = new Radios.RadioControls.FlightRadio(multiBandRadioSettings.RadioSettings);
            flightRadio.InitRadio();

            flightRadio.TemporaryFrequencyBandDown();
            flightRadio.SwitchFrequencyBand();

            Assert.Equal(result, flightRadio.ActiveFrequencyBand);
        }

        [Theory]
        [InlineData(FlightRadioFrequencyBand.VHF1, "ARC210_RADIO")]
        internal void Multi_Band_Radio_Settings_Move_Frequency_Up_Until_Is_Same(FlightRadioFrequencyBand result, string dcsbiosIdentifier)
        {
            var multiBandRadioSettings = new ARC210Settings(dcsbiosIdentifier);
            multiBandRadioSettings.RadioSettings.FrequencyBandSkipper.ClicksToSkip = 0;

            var flightRadio = new Radios.RadioControls.FlightRadio(multiBandRadioSettings.RadioSettings);
            flightRadio.InitRadio();

            flightRadio.TemporaryFrequencyBandUp();
            flightRadio.TemporaryFrequencyBandUp();
            flightRadio.TemporaryFrequencyBandUp();
            flightRadio.TemporaryFrequencyBandUp();
            flightRadio.SwitchFrequencyBand();

            Assert.Equal(result, flightRadio.ActiveFrequencyBand);
        }

        [Theory]
        [InlineData(FlightRadioFrequencyBand.VHF1, "ARC210_RADIO")]
        internal void Multi_Band_Radio_Settings_Move_Frequency_Band_Down_Until_Is_Same(FlightRadioFrequencyBand result, string dcsbiosIdentifier)
        {
            var multiBandRadioSettings = new ARC210Settings(dcsbiosIdentifier);
            multiBandRadioSettings.RadioSettings.FrequencyBandSkipper.ClicksToSkip = 0;

            var flightRadio = new Radios.RadioControls.FlightRadio(multiBandRadioSettings.RadioSettings);
            flightRadio.InitRadio();

            flightRadio.TemporaryFrequencyBandDown();
            flightRadio.TemporaryFrequencyBandDown();
            flightRadio.TemporaryFrequencyBandDown();
            flightRadio.TemporaryFrequencyBandDown();
            flightRadio.SwitchFrequencyBand();

            Assert.Equal(result, flightRadio.ActiveFrequencyBand);
        }
    }
}