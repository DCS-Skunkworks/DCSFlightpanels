namespace NonVisuals.Saitek.Switches
{
    using System;
    using System.Collections.Generic;

    using MEF;

    using Interfaces;

    [Flags]
    public enum SwitchPanelPZ55LEDs : byte
    {
        ALL_DARK = 0x0,
        UP_GREEN = 0x1,
        LEFT_GREEN = 0x2,
        RIGHT_GREEN = 0x4,
        UP_RED = 0x8,
        UP_YELLOW = 0x9,
        LEFT_RED = 0x10,
        LEFT_YELLOW = 0x12,
        RIGHT_RED = 0x20,
        RIGHT_YELLOW = 0x24,
        UP_MASK = 0x9,
        LEFT_MASK = 0x12,
        RIGHT_MASK = 0x24

        /*
         * 00000000 0x0 ALL DARK
         * 
         * 00000001 0x1 UP GREEN
         * 00001000 0x8 UP RED
         * 00001001 0x9 UP YELLOW
         * 
         * 00000010 0x2 LEFT GREEN
         * 00010000 0x10 LEFT RED
         * 00010010 0x12 LEFT YELLOW
         * 
         * 00100000 0x20 RIGHT RED
         * 00000100 0x4 RIGHT GREEN
         * 00100100 0x24 RIGHT YELLOW
         * 
         * UP MASK
         * 00001001
         * 
         * LEFT MASK
         * 00010010
         * 
         * RIGHT MASK
         * 00100100
         */
    }


    public class SwitchPanelKey : ISaitekPanelKnob
    {

        public SwitchPanelKey(int group, int mask, bool isOn, SwitchPanelPZ55Keys switchPanelPZ55Key)
        {
            Group = group;
            Mask = mask;
            IsOn = isOn;
            SwitchPanelPZ55Key = switchPanelPZ55Key;
        }

        public int Group { get; set; }

        public int Mask { get; set; }

        public bool IsOn { get; set; }

        public SwitchPanelPZ55Keys SwitchPanelPZ55Key { get; set; }

        public static HashSet<ISaitekPanelKnob> GetPanelSwitchKeys()
        {
            var result = new HashSet<ISaitekPanelKnob>
            {
                // Group 0
                new SwitchPanelKey(0, 1 << 0, false, SwitchPanelPZ55Keys.SWITCHKEY_MASTER_BAT),
                new SwitchPanelKey(0, 1 << 1, false, SwitchPanelPZ55Keys.SWITCHKEY_MASTER_ALT),
                new SwitchPanelKey(0, 1 << 2, false, SwitchPanelPZ55Keys.SWITCHKEY_AVIONICS_MASTER),
                new SwitchPanelKey(0, 1 << 3, false, SwitchPanelPZ55Keys.SWITCHKEY_FUEL_PUMP),
                new SwitchPanelKey(0, 1 << 4, false, SwitchPanelPZ55Keys.SWITCHKEY_DE_ICE),
                new SwitchPanelKey(0, 1 << 5, false, SwitchPanelPZ55Keys.SWITCHKEY_PITOT_HEAT),
                new SwitchPanelKey(0, 1 << 6, false, SwitchPanelPZ55Keys.SWITCHKEY_CLOSE_COWL),
                new SwitchPanelKey(0, 1 << 7, false, SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_PANEL),
                // Group 1
                new SwitchPanelKey(1, 1 << 0, false, SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_BEACON),
                new SwitchPanelKey(1, 1 << 1, false, SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_NAV),
                new SwitchPanelKey(1, 1 << 2, false, SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_STROBE),
                new SwitchPanelKey(1, 1 << 3, false, SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_TAXI),
                new SwitchPanelKey(1, 1 << 4, false, SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_LANDING),
                new SwitchPanelKey(1, 1 << 5, false, SwitchPanelPZ55Keys.KNOB_ENGINE_OFF),
                new SwitchPanelKey(1, 1 << 6, false, SwitchPanelPZ55Keys.KNOB_ENGINE_RIGHT),
                new SwitchPanelKey(1, 1 << 7, false, SwitchPanelPZ55Keys.KNOB_ENGINE_LEFT),
                // Group 2
                new SwitchPanelKey(2, 1 << 0, false, SwitchPanelPZ55Keys.KNOB_ENGINE_BOTH),
                new SwitchPanelKey(2, 1 << 1, false, SwitchPanelPZ55Keys.KNOB_ENGINE_START),
                new SwitchPanelKey(2, 1 << 2, false, SwitchPanelPZ55Keys.LEVER_GEAR_UP),
                new SwitchPanelKey(2, 1 << 3, false, SwitchPanelPZ55Keys.LEVER_GEAR_DOWN)
            };

            return result;
        }
    }
}
/*
    Byte #1
    00000000
    ||||||||_ SWITCHKEY_MASTER_BAT
    |||||||_ SWITCHKEY_MASTER_ALT
    ||||||_ SWITCHKEY_AVIONICS_MASTER
    |||||_ SWITCHKEY_FUEL_PUMP
    ||||_ SWITCHKEY_DE_ICE
    |||_ SWITCHKEY_PITOT_HEAT
    ||_ SWITCHKEY_CLOSE_COWL ** ~
    |_ SWITCHKEY_LIGHTS_PANEL

    Byte #2 
    00000000
    ||||||||_ SWITCHKEY_LIGHTS_BEACON
    |||||||_ SWITCHKEY_LIGHTS_NAV
    ||||||_ SWITCHKEY_LIGHTS_STROBE
    |||||_ SWITCHKEY_LIGHTS_TAXI
    ||||_ SWITCHKEY_LIGHTS_LANDING
    |||_ KNOB_ENGINE_OFF
    ||_ KNOB_ENGINE_RIGHT
    |_ KNOB_ENGINE_LEFT

    Byte #3
    00000000
    ||||||||_ KNOB_ENGINE_BOTH
    |||||||_ KNOB_ENGINE_START
    ||||||_ LEVER_GEAR_UP
    |||||_ LEVER_GEAR_DOWN
    ||||_ 
    |||_ 
    ||_ 
    |_



    LED Byte:
    * 00000000 0x0 ALL DARK
    * 
    * 00000001 0x1 UP GREEN
    * 00001000 0x8 UP RED
    * 00001001 0x9 UP YELLOW
    * 
    * 00000010 0x2 LEFT GREEN
    * 00010000 0x10 LEFT RED
    * 00010010 0x12 LEFT YELLOW
    * 
    * 00100000 0x20 RIGHT RED
    * 00000100 0x4 RIGHT GREEN
    * 00100100 0x24 RIGHT YELLOW
 */