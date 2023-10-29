using System;
using NonVisuals.Radios.RadioControls;
using NonVisuals.Radios.RadioSettings;
using Xunit;

namespace NonVisuals.Tests.FlightRadioSettings.Generic
{
    /// <summary>
    /// ARC-210 is a multi-band radio so those settings are used.
    /// </summary>
    public class MultiBandRadioSettingsTests
    {
        [Theory]
        [InlineData(FlightRadioFrequencyBand.VHF1, "ARC210_RADIO")]
        internal void Multi_Band_Radio_Settings_Move_Frequency_Band_Up(FlightRadioFrequencyBand result, string dcsbiosIdentifier)
        {
            var multiBandRadioSettings = new ARC210Settings(dcsbiosIdentifier);
            var flightRadio = new Radios.RadioControls.FlightRadio(multiBandRadioSettings.RadioSettings);
            flightRadio.TemporaryFrequencyBandUp();
            flightRadio.SwitchFrequencyBand();
            Assert.Equal("119.001", flightRadio.StandbyFrequency);
            //Assert.Equal(result, flightRadio.ActiveFrequencyBand);
        }

    }
}