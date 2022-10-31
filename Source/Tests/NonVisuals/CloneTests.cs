using ClassLibraryCommon;
using DCS_BIOS;
using MEF;
using NonVisuals;
using NonVisuals.Saitek.BindingClasses;
using NonVisuals.StreamDeck;
using NonVisuals.StreamDeck.Panels;
using System.Collections.Generic;
using System.Drawing;
using Xunit;

namespace Tests.NonVisuals
{
    public class CloneTests {
        
        private const string _sringValue1 ="Some string value 1";
        private const string _sringValue2 = "Some string value 2";
        private const string _sringValue3 = "Some string value 3";
        private const int _intValue1 = 456;
        private Color _colorValue1 = Color.Aquamarine;

        [Fact]
        public void KeypressInfo_MustBe_Clonable() {
            KeyPressInfo source = new();
            source.LengthOfKeyPress = KeyPressLength.SecondAndHalf;
            source.LengthOfBreak = KeyPressLength.FortySecs;
            
            KeyPressInfo cloned = source.CloneJson();
            
            Assert.NotNull(cloned);
            Assert.Equal(KeyPressLength.SecondAndHalf, cloned.LengthOfKeyPress);
            Assert.Equal(KeyPressLength.FortySecs, cloned.LengthOfBreak);
        }

        [Fact]
        public void IKeyPressInfo_SortedList_MustBe_Clonable() {
            SortedList<int, IKeyPressInfo> source = new();
            source.Add(1, new KeyPressInfo());

            SortedList<int, IKeyPressInfo> cloned = source.CloneJson();

            Assert.NotNull(cloned);
            Assert.NotEmpty(cloned);
        }

        [Fact]
        public void DCSBIOSInput_List_MustBe_Clonable() {
            List<DCSBIOSInput> source = new();
            source.Add(new DCSBIOSInput());

            List<DCSBIOSInput> cloned = source.CloneJson();

            Assert.NotNull(cloned);
            Assert.NotEmpty(cloned);
        }

        [Fact]        
        public void BIPLink_MustBe_Clonable_UseOf_BIPLinkPZ69() {
            //Note: BIPLink is Absract
            BIPLinkPZ69 source = new();
            source.Description = _sringValue1;
            source.WhenTurnedOn = false;
            
            BIPLinkPZ69 cloned = source.CloneJson();

            Assert.NotNull(cloned);
            Assert.Equal(_sringValue1, cloned.Description);
            Assert.False(cloned.WhenTurnedOn);
        }

        [Fact]
        public void BIPLink_MustBe_Clonable_UseOf_BIPLinkPZ55() {
            //Note: BIPLink is Absract
            BIPLinkPZ55 source = new();
            source.Description = _sringValue1;
            source.WhenTurnedOn = false;
            
            BIPLinkPZ55 cloned = source.CloneJson();

            Assert.NotNull(cloned);
            Assert.Equal(_sringValue1, cloned.Description);
            Assert.False(cloned.WhenTurnedOn);
        }

        [Fact]
        public void OSCommand_MustBe_Clonable() {
            OSCommand source = new();
            source.Name = _sringValue1;
            source.Arguments = _sringValue2;
            source.Command = _sringValue3;
            
            OSCommand cloned = source.CloneJson();

            Assert.NotNull(cloned);
            Assert.Equal(_sringValue1, cloned.Name);
            Assert.Equal(_sringValue2, cloned.Arguments);
            Assert.Equal(_sringValue3, cloned.Command);
        }

        [Fact]
        public void DCSBIOSDecoder_MustBe_Clonable() {
            DCSBIOSDecoder source = new();
            source.ButtonFinalText = _sringValue1;
            source.ButtonTextTemplate = _sringValue2;
            source.FontColor = _colorValue1;
           
            DCSBIOSDecoder cloned = source.CloneJson();

            Assert.NotNull(cloned);
            Assert.Equal(string.Empty, cloned.ButtonFinalText); // has [JsonIgnore] attribute, no value expected in clone
            Assert.Equal(_sringValue2, cloned.ButtonTextTemplate);
            Assert.Equal(_colorValue1, cloned.FontColor);
        }

        [Fact]
        public void StreamDeckButton_MustBe_Clonable() {
            StreamDeckButton source = new();
            source.StreamDeckButtonName = EnumStreamDeckButtonNames.BUTTON1;
            source.IsVisible = false;

            StreamDeckButton cloned = source.CloneJson();

            Assert.NotNull(cloned);
            Assert.Equal(EnumStreamDeckButtonNames.BUTTON1, cloned.StreamDeckButtonName);
            Assert.False(cloned.IsVisible);
        }

        [Fact]
        public void DCSBIOSInput_MustBe_Clonable() {
            DCSBIOSInput source = new();
            source.ControlId = _sringValue1;
            source.Delay = _intValue1;
            source.ControlDescription = _sringValue2;

            DCSBIOSInput cloned = source.CloneJson();

            Assert.NotNull(cloned);
            Assert.Equal(_sringValue1, cloned.ControlId);
            Assert.Equal(_intValue1, cloned.Delay);
            Assert.Equal(_sringValue2, cloned.ControlDescription);
        }

        [Fact]
        public void DCSBIOSConverter_MustBe_Clonable() {
            var gamingPanelSkeleton =
               new GamingPanelSkeleton(GamingPanelVendorEnum.Saitek, GamingPanelEnum.PZ70MultiPanel);
            StreamDeckPanel streamdeckPanel = new StreamDeckPanel(GamingPanelEnum.StreamDeck, new HIDSkeleton(gamingPanelSkeleton, "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA"), true);
            DCSBIOSConverter source = new(streamdeckPanel);
            source.ConverterOutputType = EnumConverterOutputType.Image;
            source.BackgroundColor = _colorValue1;
            source.OffsetX = _intValue1;

            DCSBIOSConverter cloned = source.CloneJson();

            Assert.NotNull(cloned);
            Assert.Equal(_colorValue1, cloned.BackgroundColor);
            Assert.Equal(EnumConverterOutputType.Image, cloned.ConverterOutputType);
            Assert.Equal(_intValue1, cloned.OffsetX);
        }
    }
}
