using System;
using System.Collections.Generic;

namespace NonVisuals
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


    public enum SwitchPanelPZ55Keys
    {
        SWITCHKEY_MASTER_BAT = 0,
        SWITCHKEY_MASTER_ALT = 2,
        SWITCHKEY_AVIONICS_MASTER = 4,
        SWITCHKEY_FUEL_PUMP = 8,
        SWITCHKEY_DE_ICE = 16,
        SWITCHKEY_PITOT_HEAT = 32,
        SWITCHKEY_CLOSE_COWL = 64,
        SWITCHKEY_LIGHTS_PANEL = 128,
        SWITCHKEY_LIGHTS_BEACON = 256,
        SWITCHKEY_LIGHTS_NAV = 512,
        SWITCHKEY_LIGHTS_STROBE = 1024,
        SWITCHKEY_LIGHTS_TAXI = 2048,
        SWITCHKEY_LIGHTS_LANDING = 4096,
        KNOB_ENGINE_OFF = 8192,
        KNOB_ENGINE_RIGHT = 16384,
        KNOB_ENGINE_LEFT = 32768,
        KNOB_ENGINE_BOTH = 65536,
        KNOB_ENGINE_START = 131072,
        LEVER_GEAR_UP = 262144,
        LEVER_GEAR_DOWN = 524288
    }

    public class SwitchPanelKey
    {
        public SwitchPanelKey()
        {
        }

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

        public string ExportString()
        {
            return "SwitchPanelKey{" + Enum.GetName(typeof(SwitchPanelPZ55Keys), SwitchPanelPZ55Key) + "}";
        }

        public void ImportString(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                throw new ArgumentException("Import string empty. (PanelSwitchKey)");
            }
            if (!str.StartsWith("SwitchPanelKey{") || !str.EndsWith("}"))
            {
                throw new ArgumentException("Import string format exception. (PanelSwitchKey) >" + str + "<");
            }
            //SwitchPanelKey{SWITCHKEY_MASTER_ALT}
            var dataString = str.Remove(0, 15);
            //SWITCHKEY_MASTER_ALT}
            dataString = dataString.Remove(dataString.Length - 1, 1);
            //SWITCHKEY_MASTER_ALT
            SwitchPanelPZ55Key = (SwitchPanelPZ55Keys)Enum.Parse(typeof(SwitchPanelPZ55Keys), dataString.Trim());
        }

        public static HashSet<SwitchPanelKey> GetPanelSwitchKeys()
        {
            var result = new HashSet<SwitchPanelKey>();
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











        /*
         * public String ExportString()
        {
            return "SwitchPanelKey{" + Group + "," + Mask + "," + Enum.GetName(typeof(SwitchPanelPZ55Keys), SwitchPanelPZ55Key) + "}";
        }

        public void ImportString(String str)
        {
            if (String.IsNullOrEmpty(str))
            {
                throw new ArgumentException("Import string empty. (PanelSwitchKey)");
            }
            if (!str.StartsWith("SwitchPanelKey{") || !str.EndsWith("}"))
            {
                throw new ArgumentException("Import string format exception. (PanelSwitchKey) >" + str + "<");
            }
            //SwitchPanelKey{1,255,SWITCHKEY_MASTER_ALT}
            var dataString = str.Remove(0, 15);
            //1,255,SWITCHKEY_MASTER_ALT}
            dataString = dataString.Remove(dataString.Length - 1, 1);
            //1,255,SWITCHKEY_MASTER_ALT
            Group = int.Parse(dataString.Substring(0, 1));
            dataString = dataString.Substring(2);
            //255,SWITCHKEY_MASTER_ALT
            Mask = int.Parse(dataString.Substring(0,dataString.IndexOf(",", StringComparison.Ordinal)));
            SwitchPanelPZ55Key = (SwitchPanelPZ55Keys)Enum.Parse(typeof(SwitchPanelPZ55Keys), dataString.Substring(dataString.IndexOf(",", StringComparison.Ordinal) +1 ));
        }
         */
    }

    public class SwitchPanelPZ55KeyOnOff
    {
        private readonly SwitchPanelPZ55Keys _switchPanelPZ55Key;
        private readonly bool _on;

        public SwitchPanelPZ55KeyOnOff(SwitchPanelPZ55Keys switchPanelPZ55Key, bool on)
        {
            _switchPanelPZ55Key = switchPanelPZ55Key;
            _on = @on;
        }

        public SwitchPanelPZ55Keys SwitchPanelPZ55Key
        {
            get { return _switchPanelPZ55Key; }
        }

        public bool On
        {
            get { return _on; }
        }
    }
}
