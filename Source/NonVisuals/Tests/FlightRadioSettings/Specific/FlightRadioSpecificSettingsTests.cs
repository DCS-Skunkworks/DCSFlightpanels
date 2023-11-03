using System;
using NonVisuals.Radios.RadioSettings;
using Xunit;

namespace NonVisuals.Tests.FlightRadioSettings.Specific
{
    public class FlightRadioSpecificSettingsTests
    {
        /* ARC-210 */
        /* FM      30.000 to 87.975 MHz */
        /* VHF AM 108.000 to 115.975 MHz */
        /* VHF AM 118.000 to 173.975 MHz */
        /* UHF AM 225.000 to 399.975 MHz */
        [Fact]
        internal void ARC210_Verify_Settings()
        {
            var arc210Settings = new ARC210Settings("ARC210");
            var exception = Record.Exception(() => arc210Settings.VerifySettings());
            Assert.Null(exception);
        }

        [Fact]
        internal void ARC210_No_DCSBIOS()
        {
            var arc210Settings = new ARC210Settings("");
            Assert.Throws<ArgumentOutOfRangeException>( () => arc210Settings.VerifySettings());
        }

        /* 108.000 to 173.975 MHz  225.000 to 399.975 MHz */
        [Fact]
        internal void JF17Com_Verify_Settings()
        {
            var jf17Settings = new JF17ComSettings("COMM1");
            var exception = Record.Exception(() => jf17Settings.VerifySettings());
            Assert.Null(exception);
        }

        [Fact]
        internal void JF17_No_DCSBIOS()
        {
            var jf17Settings = new ARC210Settings("");
            Assert.Throws<ArgumentOutOfRangeException>(() => jf17Settings.VerifySettings());
        }

        [Fact]
        internal void ARC164_Verify_Settings()
        {
            ARC164Settings settings = new("XXX");
            var exception = Record.Exception(() => settings.VerifySettings());
            Assert.Null(exception);
        }

        [Fact]
        internal void ARC222_Verify_Settings()
        {
            ARC222Settings settings = new("XXX");
            var exception = Record.Exception(() => settings.VerifySettings());
            Assert.Null(exception);
        }
    }
}