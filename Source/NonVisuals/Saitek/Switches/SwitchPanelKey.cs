using System;
using System.Collections.Generic;
using NonVisuals.Interfaces;

namespace NonVisuals.Saitek.Switches
{

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
            var result = new HashSet<ISaitekPanelKnob>();
            //Group 0
            result.Add(new SwitchPanelKey(0, Convert.ToInt32("1", 2), false, SwitchPanelPZ55Keys.SWITCHKEY_MASTER_BAT));
            result.Add(new SwitchPanelKey(0, Convert.ToInt32("10", 2), false, SwitchPanelPZ55Keys.SWITCHKEY_MASTER_ALT));
            result.Add(new SwitchPanelKey(0, Convert.ToInt32("100", 2), false, SwitchPanelPZ55Keys.SWITCHKEY_AVIONICS_MASTER));
            result.Add(new SwitchPanelKey(0, Convert.ToInt32("1000", 2), false, SwitchPanelPZ55Keys.SWITCHKEY_FUEL_PUMP));
            result.Add(new SwitchPanelKey(0, Convert.ToInt32("10000", 2), false, SwitchPanelPZ55Keys.SWITCHKEY_DE_ICE));
            result.Add(new SwitchPanelKey(0, Convert.ToInt32("100000", 2), false, SwitchPanelPZ55Keys.SWITCHKEY_PITOT_HEAT));
            result.Add(new SwitchPanelKey(0, Convert.ToInt32("1000000", 2), false, SwitchPanelPZ55Keys.SWITCHKEY_CLOSE_COWL));
            result.Add(new SwitchPanelKey(0, Convert.ToInt32("10000000", 2), false, SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_PANEL));

            //Group 1
            result.Add(new SwitchPanelKey(1, Convert.ToInt32("1", 2), false, SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_BEACON));
            result.Add(new SwitchPanelKey(1, Convert.ToInt32("10", 2), false, SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_NAV));
            result.Add(new SwitchPanelKey(1, Convert.ToInt32("100", 2), false, SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_STROBE));
            result.Add(new SwitchPanelKey(1, Convert.ToInt32("1000", 2), false, SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_TAXI));
            result.Add(new SwitchPanelKey(1, Convert.ToInt32("10000", 2), false, SwitchPanelPZ55Keys.SWITCHKEY_LIGHTS_LANDING));
            result.Add(new SwitchPanelKey(1, Convert.ToInt32("100000", 2), false, SwitchPanelPZ55Keys.KNOB_ENGINE_OFF));
            result.Add(new SwitchPanelKey(1, Convert.ToInt32("1000000", 2), false, SwitchPanelPZ55Keys.KNOB_ENGINE_RIGHT));
            result.Add(new SwitchPanelKey(1, Convert.ToInt32("10000000", 2), false, SwitchPanelPZ55Keys.KNOB_ENGINE_LEFT));

            //Group 2
            result.Add(new SwitchPanelKey(2, Convert.ToInt32("1", 2), false, SwitchPanelPZ55Keys.KNOB_ENGINE_BOTH));
            result.Add(new SwitchPanelKey(2, Convert.ToInt32("10", 2), false, SwitchPanelPZ55Keys.KNOB_ENGINE_START));
            result.Add(new SwitchPanelKey(2, Convert.ToInt32("100", 2), false, SwitchPanelPZ55Keys.LEVER_GEAR_UP));
            result.Add(new SwitchPanelKey(2, Convert.ToInt32("1000", 2), false, SwitchPanelPZ55Keys.LEVER_GEAR_DOWN));
            return result;
        }
    }
}
