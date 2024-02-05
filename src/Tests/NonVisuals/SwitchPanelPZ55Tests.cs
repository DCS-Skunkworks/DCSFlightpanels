using NonVisuals.Panels.Saitek;
using NonVisuals.Panels.Saitek.Panels;
using NonVisuals.Panels.Saitek.Switches;
using Xunit;

namespace DCSFPTests.NonVisuals
{
    public class SwitchPanelPZ55Tests
    {
        private const SwitchPanelPZ55LEDPosition _Invalid_SwitchPanelPZ55LEDPosition = (SwitchPanelPZ55LEDPosition)byte.MaxValue;
        private const PanelLEDColor _Invalid_PanelLEDColor = (PanelLEDColor)byte.MaxValue;


        [Theory]
        [InlineData(SwitchPanelPZ55LEDPosition.UP, PanelLEDColor.DARK, SwitchPanelPZ55LEDs.ALL_DARK)]
        [InlineData(SwitchPanelPZ55LEDPosition.UP, PanelLEDColor.GREEN, SwitchPanelPZ55LEDs.UP_GREEN)]
        [InlineData(SwitchPanelPZ55LEDPosition.UP, PanelLEDColor.RED, SwitchPanelPZ55LEDs.UP_RED)]
        [InlineData(SwitchPanelPZ55LEDPosition.UP, PanelLEDColor.YELLOW, SwitchPanelPZ55LEDs.UP_YELLOW)]

        [InlineData(SwitchPanelPZ55LEDPosition.LEFT, PanelLEDColor.DARK, SwitchPanelPZ55LEDs.ALL_DARK)]
        [InlineData(SwitchPanelPZ55LEDPosition.LEFT, PanelLEDColor.GREEN, SwitchPanelPZ55LEDs.LEFT_GREEN)]
        [InlineData(SwitchPanelPZ55LEDPosition.LEFT, PanelLEDColor.RED, SwitchPanelPZ55LEDs.LEFT_RED)]
        [InlineData(SwitchPanelPZ55LEDPosition.LEFT, PanelLEDColor.YELLOW, SwitchPanelPZ55LEDs.LEFT_YELLOW)]

        [InlineData(SwitchPanelPZ55LEDPosition.RIGHT, PanelLEDColor.DARK, SwitchPanelPZ55LEDs.ALL_DARK)]
        [InlineData(SwitchPanelPZ55LEDPosition.RIGHT, PanelLEDColor.GREEN, SwitchPanelPZ55LEDs.RIGHT_GREEN)]
        [InlineData(SwitchPanelPZ55LEDPosition.RIGHT, PanelLEDColor.RED, SwitchPanelPZ55LEDs.RIGHT_RED)]
        [InlineData(SwitchPanelPZ55LEDPosition.RIGHT, PanelLEDColor.YELLOW, SwitchPanelPZ55LEDs.RIGHT_YELLOW)]
        public void GetSwitchPanelPZ55LEDColor_ShouldReturn_ExpectedEnumValue(SwitchPanelPZ55LEDPosition switchPanelPZ55LEDPosition, PanelLEDColor panelLEDColor, SwitchPanelPZ55LEDs expectedEnumValue)
        {
            Assert.Equal(expectedEnumValue, SwitchPanelPZ55.GetSwitchPanelPZ55LEDColor(switchPanelPZ55LEDPosition, panelLEDColor));
        }

        [Theory]
        [InlineData(_Invalid_SwitchPanelPZ55LEDPosition, PanelLEDColor.DARK, SwitchPanelPZ55LEDs.ALL_DARK)]
        [InlineData(_Invalid_SwitchPanelPZ55LEDPosition, PanelLEDColor.GREEN, SwitchPanelPZ55LEDs.ALL_DARK)]
        [InlineData(_Invalid_SwitchPanelPZ55LEDPosition, PanelLEDColor.RED, SwitchPanelPZ55LEDs.ALL_DARK)]
        [InlineData(_Invalid_SwitchPanelPZ55LEDPosition, PanelLEDColor.YELLOW, SwitchPanelPZ55LEDs.ALL_DARK)]

        [InlineData(SwitchPanelPZ55LEDPosition.UP, _Invalid_PanelLEDColor, SwitchPanelPZ55LEDs.ALL_DARK)]
        [InlineData(SwitchPanelPZ55LEDPosition.LEFT, _Invalid_PanelLEDColor, SwitchPanelPZ55LEDs.ALL_DARK)]
        [InlineData(SwitchPanelPZ55LEDPosition.RIGHT, _Invalid_PanelLEDColor, SwitchPanelPZ55LEDs.ALL_DARK)]

        public void GetSwitchPanelPZ55LEDColor_ShouldReturn_ALL_DARK_For_UnExpectedEnumValues(SwitchPanelPZ55LEDPosition switchPanelPZ55LEDPosition, PanelLEDColor panelLEDColor, SwitchPanelPZ55LEDs expectedEnumValue)
        {
            Assert.Equal(expectedEnumValue, SwitchPanelPZ55.GetSwitchPanelPZ55LEDColor(switchPanelPZ55LEDPosition, panelLEDColor));
        }
    }
}
