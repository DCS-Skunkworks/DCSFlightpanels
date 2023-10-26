using System;
using NonVisuals.Radios.RadioControls;
using Xunit;

namespace NonVisuals.Tests.ARC210_Radio
{
    public class ARC210Tests
    {
        [Theory]
        [InlineData("ARC_210_RADIO", ARC210FrequencyBand.VHF1, new[] { ARC210FrequencyBand.VHF2, ARC210FrequencyBand.UHF })]
        [InlineData("", ARC210FrequencyBand.VHF2, new[] { ARC210FrequencyBand.VHF2, ARC210FrequencyBand.UHF })]
        [InlineData("ARC_210_RADIO", ARC210FrequencyBand.VHF1, null)]
        [InlineData("ARC_210_RADIO", ARC210FrequencyBand.VHF1, new ARC210FrequencyBand[0])]
        internal void ARC210_Invalid_Instantiation(string dcsbiosIdentifier, ARC210FrequencyBand initialFrequencyBand, ARC210FrequencyBand[] supportedFrequencyBands)
        {
            var arc210 = new ARC210(dcsbiosIdentifier, initialFrequencyBand, supportedFrequencyBands);
            Assert.Throws<ArgumentOutOfRangeException>(() => arc210.InitRadio());
        }

        /* ARC-210 */
        /* FM      30.000 to 87.975 MHz */
        /* VHF AM 108.000 to 115.975 MHz */
        /* VHF AM 118.000 to 173.975 MHz */
        /* UHF AM 225.000 to 399.975 MHz */
        [Theory]
        [InlineData("30.000", "ARC_210_RADIO", ARC210FrequencyBand.FM, new[] { ARC210FrequencyBand.FM })]
        [InlineData("108.000", "ARC_210_RADIO", ARC210FrequencyBand.VHF1, new[] { ARC210FrequencyBand.VHF1 })]
        [InlineData("118.000", "ARC_210_RADIO", ARC210FrequencyBand.VHF2, new[] { ARC210FrequencyBand.VHF2 })]
        [InlineData("225.000", "ARC_210_RADIO", ARC210FrequencyBand.UHF, new[] { ARC210FrequencyBand.UHF })]
        internal void ARC210_Initial_Standby_Frequency_Equals(string frequency, string dcsbiosIdentifier, ARC210FrequencyBand initialFrequencyBand, ARC210FrequencyBand[] supportedFrequencyBands)
        {
            var arc210 = new ARC210(dcsbiosIdentifier, initialFrequencyBand, supportedFrequencyBands);
            arc210.InitRadio();
            Assert.Equal(frequency, arc210.GetStandbyFrequency());
        }

        /* ARC-210 */
        /* FM      30.000 to 87.975 MHz */
        /* VHF AM 108.000 to 115.975 MHz */
        /* VHF AM 118.000 to 173.975 MHz */
        /* UHF AM 225.000 to 399.975 MHz */
        [Theory]
        [InlineData("35.000", "ARC_210_RADIO", ARC210FrequencyBand.FM, new[] { ARC210FrequencyBand.FM })]
        [InlineData("113.000", "ARC_210_RADIO", ARC210FrequencyBand.VHF1, new[] { ARC210FrequencyBand.VHF1 })]
        [InlineData("123.000", "ARC_210_RADIO", ARC210FrequencyBand.VHF2, new[] { ARC210FrequencyBand.VHF2 })]
        [InlineData("230.000", "ARC_210_RADIO", ARC210FrequencyBand.UHF, new[] { ARC210FrequencyBand.UHF })]
        internal void ARC210_BigFrequencyUp_Equals(string newFrequency, string dcsbiosIdentifier, ARC210FrequencyBand initialFrequencyBand, ARC210FrequencyBand[] supportedFrequencyBands)
        {
            var arc210 = new ARC210(dcsbiosIdentifier, initialFrequencyBand, supportedFrequencyBands);
            arc210.InitRadio();

            arc210.BigFrequencyUp();
            arc210.BigFrequencyUp();
            arc210.BigFrequencyUp();
            arc210.BigFrequencyUp();
            arc210.BigFrequencyUp();
            Assert.Equal(newFrequency, arc210.GetStandbyFrequency());
        }

        /* ARC-210 */
        /* FM      30.000 to 87.975 MHz */
        /* VHF AM 108.000 to 115.975 MHz */
        /* VHF AM 118.000 to 173.975 MHz */
        /* UHF AM 225.000 to 399.975 MHz */
        [Theory]
        [InlineData("83.000", "ARC_210_RADIO", ARC210FrequencyBand.FM, new[] { ARC210FrequencyBand.FM })]
        [InlineData("111.000", "ARC_210_RADIO", ARC210FrequencyBand.VHF1, new[] { ARC210FrequencyBand.VHF1 })]
        [InlineData("169.000", "ARC_210_RADIO", ARC210FrequencyBand.VHF2, new[] { ARC210FrequencyBand.VHF2 })]
        [InlineData("395.000", "ARC_210_RADIO", ARC210FrequencyBand.UHF, new[] { ARC210FrequencyBand.UHF })]
        internal void ARC210_BigFrequencyDown_Equals(string newFrequency, string dcsbiosIdentifier, ARC210FrequencyBand initialFrequencyBand, ARC210FrequencyBand[] supportedFrequencyBands)
        {
            var arc210 = new ARC210(dcsbiosIdentifier, initialFrequencyBand, supportedFrequencyBands);
            arc210.InitRadio();

            arc210.BigFrequencyDown();
            arc210.BigFrequencyDown();
            arc210.BigFrequencyDown();
            arc210.BigFrequencyDown();
            arc210.BigFrequencyDown();
            Assert.Equal(newFrequency, arc210.GetStandbyFrequency());
        }

        /* ARC-210 */
        /* FM      30.000 to 87.975 MHz */
        /* VHF AM 108.000 to 115.975 MHz */
        /* VHF AM 118.000 to 173.975 MHz */
        /* UHF AM 225.000 to 399.975 MHz */
        [Theory]
        [InlineData("30.075", "ARC_210_RADIO", ARC210FrequencyBand.FM, new[] { ARC210FrequencyBand.FM })]
        [InlineData("108.075", "ARC_210_RADIO", ARC210FrequencyBand.VHF1, new[] { ARC210FrequencyBand.VHF1 })]
        [InlineData("118.075", "ARC_210_RADIO", ARC210FrequencyBand.VHF2, new[] { ARC210FrequencyBand.VHF2 })]
        [InlineData("225.075", "ARC_210_RADIO", ARC210FrequencyBand.UHF, new[] { ARC210FrequencyBand.UHF })]
        internal void ARC210_SmallFrequencyUp_Equals(string newFrequency, string dcsbiosIdentifier, ARC210FrequencyBand initialFrequencyBand, ARC210FrequencyBand[] supportedFrequencyBands)
        {
            var arc210 = new ARC210(dcsbiosIdentifier, initialFrequencyBand, supportedFrequencyBands);
            arc210.InitRadio();

            arc210.SmallFrequencyUp();
            arc210.SmallFrequencyUp();
            arc210.SmallFrequencyUp();
            Assert.Equal(newFrequency, arc210.GetStandbyFrequency());
        }

        /* ARC-210 */
        /* FM      30.000 to 87.975 MHz */
        /* VHF AM 108.000 to 115.975 MHz */
        /* VHF AM 118.000 to 173.975 MHz */
        /* UHF AM 225.000 to 399.975 MHz */
        [Theory]
        [InlineData("30.925", "ARC_210_RADIO", ARC210FrequencyBand.FM, new[] { ARC210FrequencyBand.FM })]
        [InlineData("108.925", "ARC_210_RADIO", ARC210FrequencyBand.VHF1, new[] { ARC210FrequencyBand.VHF1 })]
        [InlineData("118.925", "ARC_210_RADIO", ARC210FrequencyBand.VHF2, new[] { ARC210FrequencyBand.VHF2 })]
        [InlineData("225.925", "ARC_210_RADIO", ARC210FrequencyBand.UHF, new[] { ARC210FrequencyBand.UHF })]
        internal void ARC210_SmallFrequencyDown_Equals(string newFrequency, string dcsbiosIdentifier, ARC210FrequencyBand initialFrequencyBand, ARC210FrequencyBand[] supportedFrequencyBands)
        {
            var arc210 = new ARC210(dcsbiosIdentifier, initialFrequencyBand, supportedFrequencyBands);
            arc210.InitRadio();

            arc210.SmallFrequencyDown();
            arc210.SmallFrequencyDown();
            arc210.SmallFrequencyDown();
            Assert.Equal(newFrequency, arc210.GetStandbyFrequency());
        }

        /* ARC-210 */
        /* FM      30.000 to 87.975 MHz */
        /* VHF AM 108.000 to 115.975 MHz */
        /* VHF AM 118.000 to 173.975 MHz */
        /* UHF AM 225.000 to 399.975 MHz */
        [Theory]
        [InlineData(ARC210FrequencyBand.FM, ARC210FrequencyBand.FM, new[] { ARC210FrequencyBand.FM })]
        [InlineData(ARC210FrequencyBand.FM, ARC210FrequencyBand.VHF1, new[] { ARC210FrequencyBand.UHF, ARC210FrequencyBand.VHF1, ARC210FrequencyBand.FM })]
        [InlineData(ARC210FrequencyBand.VHF1, ARC210FrequencyBand.UHF, new[] { ARC210FrequencyBand.VHF2, ARC210FrequencyBand.UHF, ARC210FrequencyBand.VHF1 })]
        [InlineData(ARC210FrequencyBand.FM, ARC210FrequencyBand.VHF1, new[] { ARC210FrequencyBand.VHF2, ARC210FrequencyBand.UHF, ARC210FrequencyBand.VHF1, ARC210FrequencyBand.FM, ARC210FrequencyBand.VHF1, ARC210FrequencyBand.FM })]
        [InlineData(ARC210FrequencyBand.VHF2, ARC210FrequencyBand.UHF, new[] { ARC210FrequencyBand.UHF, ARC210FrequencyBand.VHF2 })]
        internal void ARC210_Check_That_Frequency_Band_Is_Sorted_1st_Entry_Equals(ARC210FrequencyBand firstARC210FrequencyBand, ARC210FrequencyBand initialFrequencyBand, ARC210FrequencyBand[] supportedFrequencyBands)
        {
            var arc210 = new ARC210("ARC_210_RADIO", initialFrequencyBand, supportedFrequencyBands);
            arc210.InitRadio();
            
            Assert.Equal(firstARC210FrequencyBand, arc210.SupportedFrequencyBands()[0]);
        }

        /* ARC-210 */
        /* FM      30.000 to 87.975 MHz */
        /* VHF AM 108.000 to 115.975 MHz */
        /* VHF AM 118.000 to 173.975 MHz */
        /* UHF AM 225.000 to 399.975 MHz */
        [Theory]
        [InlineData(ARC210FrequencyBand.VHF1, ARC210FrequencyBand.VHF1, new[] { ARC210FrequencyBand.UHF, ARC210FrequencyBand.VHF1, ARC210FrequencyBand.FM })]
        [InlineData(ARC210FrequencyBand.VHF2, ARC210FrequencyBand.UHF, new[] { ARC210FrequencyBand.VHF2, ARC210FrequencyBand.UHF, ARC210FrequencyBand.VHF1 })]
        [InlineData(ARC210FrequencyBand.VHF2, ARC210FrequencyBand.UHF, new[] { ARC210FrequencyBand.VHF2, ARC210FrequencyBand.UHF, ARC210FrequencyBand.VHF1, ARC210FrequencyBand.VHF2, ARC210FrequencyBand.UHF, ARC210FrequencyBand.VHF1 })]
        [InlineData(ARC210FrequencyBand.VHF1, ARC210FrequencyBand.VHF1, new[] { ARC210FrequencyBand.VHF2, ARC210FrequencyBand.UHF, ARC210FrequencyBand.VHF1, ARC210FrequencyBand.FM, ARC210FrequencyBand.VHF1, ARC210FrequencyBand.FM })]
        [InlineData(ARC210FrequencyBand.UHF, ARC210FrequencyBand.UHF, new[] { ARC210FrequencyBand.UHF, ARC210FrequencyBand.VHF2 })]
        internal void ARC210_Check_That_Frequency_Band_Is_Sorted_2nd_Entry_Equals(ARC210FrequencyBand firstARC210FrequencyBand, ARC210FrequencyBand initialFrequencyBand, ARC210FrequencyBand[] supportedFrequencyBands)
        {
            var arc210 = new ARC210("ARC_210_RADIO", initialFrequencyBand, supportedFrequencyBands);
            arc210.InitRadio();

            Assert.Equal(firstARC210FrequencyBand, arc210.SupportedFrequencyBands()[1]);
        }

        /* ARC-210 */
        /* FM      30.000 to 87.975 MHz */
        /* VHF AM 108.000 to 115.975 MHz */
        /* VHF AM 118.000 to 173.975 MHz */
        /* UHF AM 225.000 to 399.975 MHz */
        [Theory]
        [InlineData(ARC210FrequencyBand.UHF, ARC210FrequencyBand.VHF1, new[] { ARC210FrequencyBand.UHF, ARC210FrequencyBand.VHF1, ARC210FrequencyBand.FM })]
        [InlineData(ARC210FrequencyBand.UHF, ARC210FrequencyBand.UHF, new[] { ARC210FrequencyBand.VHF2, ARC210FrequencyBand.UHF, ARC210FrequencyBand.VHF1 })]
        [InlineData(ARC210FrequencyBand.VHF2, ARC210FrequencyBand.UHF, new[] { ARC210FrequencyBand.VHF2, ARC210FrequencyBand.UHF, ARC210FrequencyBand.VHF1, ARC210FrequencyBand.VHF2, ARC210FrequencyBand.UHF, ARC210FrequencyBand.VHF1 })]
        [InlineData(ARC210FrequencyBand.VHF2, ARC210FrequencyBand.VHF1, new[] { ARC210FrequencyBand.VHF2, ARC210FrequencyBand.UHF, ARC210FrequencyBand.VHF1, ARC210FrequencyBand.FM, ARC210FrequencyBand.VHF1, ARC210FrequencyBand.FM })]
        internal void ARC210_Check_That_Frequency_Band_Is_Sorted_3rd_Entry_Equals(ARC210FrequencyBand firstARC210FrequencyBand, ARC210FrequencyBand initialFrequencyBand, ARC210FrequencyBand[] supportedFrequencyBands)
        {
            var arc210 = new ARC210("ARC_210_RADIO", initialFrequencyBand, supportedFrequencyBands);
            arc210.InitRadio();

            Assert.Equal(firstARC210FrequencyBand, arc210.SupportedFrequencyBands()[2]);
        }

        /* ARC-210 */
        /* FM      30.000 to 87.975 MHz */
        /* VHF AM 108.000 to 115.975 MHz */
        /* VHF AM 118.000 to 173.975 MHz */
        /* UHF AM 225.000 to 399.975 MHz */
        [Theory]
        [InlineData(ARC210FrequencyBand.UHF, ARC210FrequencyBand.VHF1, new[] { ARC210FrequencyBand.VHF2, ARC210FrequencyBand.UHF, ARC210FrequencyBand.VHF1, ARC210FrequencyBand.FM, ARC210FrequencyBand.VHF1, ARC210FrequencyBand.FM })]
        [InlineData(ARC210FrequencyBand.UHF, ARC210FrequencyBand.UHF, new[] { ARC210FrequencyBand.VHF2, ARC210FrequencyBand.UHF, ARC210FrequencyBand.VHF1, ARC210FrequencyBand.VHF2, ARC210FrequencyBand.UHF, ARC210FrequencyBand.VHF1 , ARC210FrequencyBand.FM})]
        internal void ARC210_Check_That_Frequency_Band_Is_Sorted_4th_Entry_Equals(ARC210FrequencyBand firstARC210FrequencyBand, ARC210FrequencyBand initialFrequencyBand, ARC210FrequencyBand[] supportedFrequencyBands)
        {
            var arc210 = new ARC210("ARC_210_RADIO", initialFrequencyBand, supportedFrequencyBands);
            arc210.InitRadio();

            Assert.Equal(firstARC210FrequencyBand, arc210.SupportedFrequencyBands()[3]);
        }

        /* ARC-210 */
        /* FM      30.000 to 87.975 MHz */
        /* VHF AM 108.000 to 115.975 MHz */
        /* VHF AM 118.000 to 173.975 MHz */
        /* UHF AM 225.000 to 399.975 MHz */
        [Theory]
        [InlineData("108.000", "ARC_210_RADIO", ARC210FrequencyBand.FM, new[] { ARC210FrequencyBand.FM, ARC210FrequencyBand.VHF1 })]
        [InlineData("118.000", "ARC_210_RADIO", ARC210FrequencyBand.VHF1, new[] { ARC210FrequencyBand.VHF1, ARC210FrequencyBand.VHF2 })]
        [InlineData("225.000", "ARC_210_RADIO", ARC210FrequencyBand.VHF2, new[] { ARC210FrequencyBand.VHF2, ARC210FrequencyBand.UHF })]
        [InlineData("30.000", "ARC_210_RADIO", ARC210FrequencyBand.UHF, new[] { ARC210FrequencyBand.FM, ARC210FrequencyBand.UHF })]

        [InlineData("225.000", "ARC_210_RADIO", ARC210FrequencyBand.FM, new[] { ARC210FrequencyBand.FM, ARC210FrequencyBand.UHF })]
        [InlineData("30.000", "ARC_210_RADIO", ARC210FrequencyBand.VHF1, new[] { ARC210FrequencyBand.VHF1, ARC210FrequencyBand.FM })]
        [InlineData("108.000", "ARC_210_RADIO", ARC210FrequencyBand.VHF2, new[] { ARC210FrequencyBand.VHF1, ARC210FrequencyBand.VHF2 })]
        [InlineData("118.000", "ARC_210_RADIO", ARC210FrequencyBand.UHF, new[] { ARC210FrequencyBand.VHF2, ARC210FrequencyBand.UHF })]
        internal void ARC210_SwitchFrequencyBandUp_Equals(string newFrequency, string dcsbiosIdentifier, ARC210FrequencyBand initialFrequencyBand, ARC210FrequencyBand[] supportedFrequencyBands)
        {
            var arc210 = new ARC210(dcsbiosIdentifier, initialFrequencyBand, supportedFrequencyBands, 25, 0);
            arc210.InitRadio();

            arc210.TemporaryFrequencyBandUp();
            arc210.SwitchFrequencyBand();

            Assert.Equal(newFrequency, arc210.GetStandbyFrequency());
        }
    }
}
