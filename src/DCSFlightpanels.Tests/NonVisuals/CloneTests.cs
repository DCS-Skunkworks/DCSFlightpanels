using System.Collections.Generic;
using System.Drawing;
using ClassLibraryCommon;
using DCS_BIOS.Serialized;
using MEF;
using NonVisuals;
using NonVisuals.BindingClasses.BIP;
using NonVisuals.HID;
using NonVisuals.KeyEmulation;
using NonVisuals.Panels.StreamDeck;
using NonVisuals.Panels.StreamDeck.Panels;
using Xunit;

namespace DCSFPTests.NonVisuals
{
    public class CloneTests {

        private const string _stringValue1 = "Some string value 1";
        private const string _stringValue2 = "Some string value 2";
        private const string _stringValue3 = "Some string value 3";
        private const int _intValue1 = 456;
        private readonly Color _colorValue1 = Color.Aquamarine;

        [Fact]
        public void KeypressInfo_MustBe_Clonable() {
            KeyPressInfo source = new()
            {
                LengthOfKeyPress = KeyPressLength.SecondAndHalf,
                LengthOfBreak = KeyPressLength.FortySecs
            };

            KeyPressInfo cloned = source.CloneJson();

            Assert.NotNull(cloned);
            Assert.Equal(KeyPressLength.SecondAndHalf, cloned.LengthOfKeyPress);
            Assert.Equal(KeyPressLength.FortySecs, cloned.LengthOfBreak);
        }

        [Fact]
        public void IKeyPressInfo_SortedList_MustBe_Clonable() {
            SortedList<int, IKeyPressInfo> source = new()
            {
                { 1, new KeyPressInfo() }
            };

            SortedList<int, IKeyPressInfo> cloned = source.CloneJson();

            Assert.NotNull(cloned);
            Assert.NotEmpty(cloned);
        }

        [Fact]
        public void DCSBIOSInput_List_MustBe_Clonable() {
            List<DCSBIOSInput> source = new()
            {
                new DCSBIOSInput()
            };

            List<DCSBIOSInput> cloned = source.CloneJson();

            Assert.NotNull(cloned);
            Assert.NotEmpty(cloned);
        }

        [Fact]
        public void BIPLinkPZ69_MustBe_Clonable() {
            //Note: BIPLink is Absract
            BIPLinkPZ69 source = new()
            {
                Description = _stringValue1,
                WhenTurnedOn = false
            };

            BIPLinkPZ69 cloned = source.CloneJson();

            Assert.NotNull(cloned);
            Assert.Equal(_stringValue1, cloned.Description);
            Assert.False(cloned.WhenTurnedOn);
        }

        [Fact]
        public void BIPLinkPZ55_MustBe_Clonable() {
            //Note: BIPLink is Absract
            BIPLinkPZ55 source = new()
            {
                Description = _stringValue1,
                WhenTurnedOn = false
            };

            BIPLinkPZ55 cloned = source.CloneJson();

            Assert.NotNull(cloned);
            Assert.Equal(_stringValue1, cloned.Description);
            Assert.False(cloned.WhenTurnedOn);
        }

        [Fact]
        public void BIPLinkPZ70_MustBe_Clonable() {
            //Note: BIPLink is Absract
            BIPLinkPZ70 source = new()
            {
                Description = _stringValue1,
                WhenTurnedOn = false
            };

            BIPLinkPZ70 cloned = source.CloneJson();

            Assert.NotNull(cloned);
            Assert.Equal(_stringValue1, cloned.Description);
            Assert.False(cloned.WhenTurnedOn);
        }

        [Fact]
        public void BIPLinkTPM_MustBe_Clonable() {
            //Note: BIPLink is Absract
            BIPLinkTPM source = new()
            {
                Description = _stringValue1,
                WhenTurnedOn = false
            };

            BIPLinkTPM cloned = source.CloneJson();

            Assert.NotNull(cloned);
            Assert.Equal(_stringValue1, cloned.Description);
            Assert.False(cloned.WhenTurnedOn);
        }

        [Fact]
        public void BIPLinkStreamDeck_MustBe_Clonable() {
            //Note: BIPLink is Absract
            BIPLinkStreamDeck source = new()
            {
                Description = _stringValue1,
                WhenTurnedOn = false
            };

            BIPLinkStreamDeck cloned = source.CloneJson();

            Assert.NotNull(cloned);
            Assert.Equal(_stringValue1, cloned.Description);
            Assert.False(cloned.WhenTurnedOn);
        }

        [Fact]
        public void BIPLinkFarmingPanel_MustBe_Clonable() {
            //Note: BIPLink is Absract
            BIPLinkFarmingPanel source = new()
            {
                Description = _stringValue1,
                WhenTurnedOn = false
            };

            BIPLinkFarmingPanel cloned = source.CloneJson();

            Assert.NotNull(cloned);
            Assert.Equal(_stringValue1, cloned.Description);
            Assert.False(cloned.WhenTurnedOn);
        }

        [Fact]
        public void OSCommand_MustBe_Clonable() {
            OSCommand source = new()
            {
                Name = _stringValue1,
                Arguments = _stringValue2,
                Command = _stringValue3
            };

            OSCommand cloned = source.CloneJson();

            Assert.NotNull(cloned);
            Assert.Equal(_stringValue1, cloned.Name);
            Assert.Equal(_stringValue2, cloned.Arguments);
            Assert.Equal(_stringValue3, cloned.Command);
        }

        [Fact]
        public void DCSBIOSDecoder_MustBe_Clonable() {
            DCSBIOSDecoder source = new()
            {
                ButtonFinalText = _stringValue1,
                ButtonTextTemplate = _stringValue2,
                FontColor = _colorValue1
            };

            DCSBIOSDecoder cloned = source.CloneJson();

            Assert.NotNull(cloned);
            Assert.Equal(string.Empty, cloned.ButtonFinalText); // has [JsonIgnore] attribute, no value expected in clone
            Assert.Equal(_stringValue2, cloned.ButtonTextTemplate);
            Assert.Equal(_colorValue1, cloned.FontColor);
        }

        [Fact]
        public void StreamDeckButton_MustBe_Clonable() {
            StreamDeckButton source = new()
            {
                StreamDeckButtonName = EnumStreamDeckButtonNames.BUTTON1,
                IsVisible = false
            };

            StreamDeckButton cloned = source.CloneJson();

            Assert.NotNull(cloned);
            Assert.Equal(EnumStreamDeckButtonNames.BUTTON1, cloned.StreamDeckButtonName);
            Assert.False(cloned.IsVisible);
        }

        [Fact]
        public void DCSBIOSInput_MustBe_Clonable() {
            DCSBIOSInput source = new()
            {
                ControlId = _stringValue1,
                Delay = _intValue1,
                ControlDescription = _stringValue2
            };

            DCSBIOSInput cloned = source.CloneJson();

            Assert.NotNull(cloned);
            Assert.Equal(_stringValue1, cloned.ControlId);
            Assert.Equal(_intValue1, cloned.Delay);
            Assert.Equal(_stringValue2, cloned.ControlDescription);
        }

        [Fact]
        public void DCSBIOSConverter_MustBe_Clonable() {
            var gamingPanelSkeleton =
               new GamingPanelSkeleton(GamingPanelVendorEnum.Saitek, GamingPanelEnum.PZ70MultiPanel);
            StreamDeckPanel streamdeckPanel = new(GamingPanelEnum.StreamDeck, new HIDSkeleton(gamingPanelSkeleton, "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA"), true);
            DCSBIOSConverter source = new(streamdeckPanel)
            {
                ConverterOutputType = EnumConverterOutputType.Image,
                BackgroundColor = _colorValue1,
                OffsetX = _intValue1
            };

            DCSBIOSConverter cloned = source.CloneJson();

            Assert.NotNull(cloned);
            Assert.Equal(_colorValue1, cloned.BackgroundColor);
            Assert.Equal(EnumConverterOutputType.Image, cloned.ConverterOutputType);
            Assert.Equal(_intValue1, cloned.OffsetX);
        }
    }
}
