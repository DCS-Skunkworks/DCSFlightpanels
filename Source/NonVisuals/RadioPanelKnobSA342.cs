using System;
using System.Collections.Generic;

namespace NonVisuals { 

    public enum RadioPanelPZ69KnobsSA342
    {
        UPPER_VHFAM = 0,
        UPPER_VHFFM = 2,
        UPPER_NAV1 = 4,
        UPPER_NAV2 = 8,
        UPPER_ADF = 16,
        UPPER_DME = 32,
        UPPER_XPDR = 64,
        UPPER_SMALL_FREQ_WHEEL_INC = 128,
        UPPER_SMALL_FREQ_WHEEL_DEC = 256,
        UPPER_LARGE_FREQ_WHEEL_INC = 512,
        UPPER_LARGE_FREQ_WHEEL_DEC = 1024,
        UPPER_FREQ_SWITCH = 2056,
        LOWER_VHFAM = 4096,
        LOWER_VHFFM = 8192,
        LOWER_NAV1 = 16384,
        LOWER_NAV2 = 32768,
        LOWER_ADF = 65536,
        LOWER_DME = 131072,
        LOWER_XPDR = 262144,
        LOWER_SMALL_FREQ_WHEEL_INC = 8388608,
        LOWER_SMALL_FREQ_WHEEL_DEC = 524288,
        LOWER_LARGE_FREQ_WHEEL_INC = 1048576,
        LOWER_LARGE_FREQ_WHEEL_DEC = 2097152,
        LOWER_FREQ_SWITCH = 4194304
    }

    public enum CurrentSA345RadioMode
    {
        VHFFM = 2,
        VHFAM = 4
    }

    public class RadioPanelKnobSA342
    {
        public RadioPanelKnobSA342(int group, int mask, bool isOn, RadioPanelPZ69KnobsSA342 radioPanelPZ69Knob)
        {
            Group = group;
            Mask = mask;
            IsOn = isOn;
            RadioPanelPZ69Knob = radioPanelPZ69Knob;
        }

        public int Group { get; set; }

        public int Mask { get; set; }

        public bool IsOn { get; set; }

        public RadioPanelPZ69KnobsSA342 RadioPanelPZ69Knob { get; set; }

        public string ExportString()
        {
            return "RadioPanelKnob{" + Enum.GetName(typeof(RadioPanelPZ69KnobsSA342), RadioPanelPZ69Knob) + "}";
        }

        public void ImportString(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                throw new ArgumentException("Import string empty. (RadioPanelKnob)");
            }
            if (!str.StartsWith("RadioPanelKnob{") || !str.EndsWith("}"))
            {
                throw new ArgumentException("Import string format exception. (RadioPanelKnob) >" + str + "<");
            }
            //RadioPanelKnob{SWITCHKEY_MASTER_ALT}
            var dataString = str.Remove(0, 15);
            //SWITCHKEY_MASTER_ALT}
            dataString = dataString.Remove(dataString.Length - 1, 1);
            //SWITCHKEY_MASTER_ALT
            RadioPanelPZ69Knob = (RadioPanelPZ69KnobsSA342)Enum.Parse(typeof(RadioPanelPZ69KnobsSA342), dataString.Trim());
        }

        public static HashSet<RadioPanelKnobSA342> GetRadioPanelKnobs()
        {
            //true means clockwise turn
            var result = new HashSet<RadioPanelKnobSA342>();
            //Group 0
            result.Add(new RadioPanelKnobSA342(2, Convert.ToInt32("1", 2), true, RadioPanelPZ69KnobsSA342.UPPER_SMALL_FREQ_WHEEL_INC));
            result.Add(new RadioPanelKnobSA342(2, Convert.ToInt32("10", 2), false, RadioPanelPZ69KnobsSA342.UPPER_SMALL_FREQ_WHEEL_DEC));
            result.Add(new RadioPanelKnobSA342(2, Convert.ToInt32("100", 2), true, RadioPanelPZ69KnobsSA342.UPPER_LARGE_FREQ_WHEEL_INC));
            result.Add(new RadioPanelKnobSA342(2, Convert.ToInt32("1000", 2), false, RadioPanelPZ69KnobsSA342.UPPER_LARGE_FREQ_WHEEL_DEC));
            result.Add(new RadioPanelKnobSA342(2, Convert.ToInt32("10000", 2), true, RadioPanelPZ69KnobsSA342.LOWER_SMALL_FREQ_WHEEL_INC));
            result.Add(new RadioPanelKnobSA342(2, Convert.ToInt32("100000", 2), false, RadioPanelPZ69KnobsSA342.LOWER_SMALL_FREQ_WHEEL_DEC));
            result.Add(new RadioPanelKnobSA342(2, Convert.ToInt32("1000000", 2), true, RadioPanelPZ69KnobsSA342.LOWER_LARGE_FREQ_WHEEL_INC));
            result.Add(new RadioPanelKnobSA342(2, Convert.ToInt32("10000000", 2), false, RadioPanelPZ69KnobsSA342.LOWER_LARGE_FREQ_WHEEL_DEC));

            //Group 1
            result.Add(new RadioPanelKnobSA342(1, Convert.ToInt32("1", 2), true, RadioPanelPZ69KnobsSA342.LOWER_VHFFM));
            result.Add(new RadioPanelKnobSA342(1, Convert.ToInt32("10", 2), true, RadioPanelPZ69KnobsSA342.LOWER_NAV1));
            result.Add(new RadioPanelKnobSA342(1, Convert.ToInt32("100", 2), true, RadioPanelPZ69KnobsSA342.LOWER_NAV2));
            result.Add(new RadioPanelKnobSA342(1, Convert.ToInt32("1000", 2), true, RadioPanelPZ69KnobsSA342.LOWER_ADF));
            result.Add(new RadioPanelKnobSA342(1, Convert.ToInt32("10000", 2), true, RadioPanelPZ69KnobsSA342.LOWER_DME));
            result.Add(new RadioPanelKnobSA342(1, Convert.ToInt32("100000", 2), true, RadioPanelPZ69KnobsSA342.LOWER_XPDR));
            result.Add(new RadioPanelKnobSA342(1, Convert.ToInt32("1000000", 2), true, RadioPanelPZ69KnobsSA342.UPPER_FREQ_SWITCH));
            result.Add(new RadioPanelKnobSA342(1, Convert.ToInt32("10000000", 2), true, RadioPanelPZ69KnobsSA342.LOWER_FREQ_SWITCH));

            //Group 2
            result.Add(new RadioPanelKnobSA342(0, Convert.ToInt32("1", 2), true, RadioPanelPZ69KnobsSA342.UPPER_VHFAM)); //UPPER COM 1
            result.Add(new RadioPanelKnobSA342(0, Convert.ToInt32("10", 2), true, RadioPanelPZ69KnobsSA342.UPPER_VHFFM)); //UPPER COM 2
            result.Add(new RadioPanelKnobSA342(0, Convert.ToInt32("100", 2), true, RadioPanelPZ69KnobsSA342.UPPER_VHFFM)); //UPPER NAV 1
            result.Add(new RadioPanelKnobSA342(0, Convert.ToInt32("1000", 2), true, RadioPanelPZ69KnobsSA342.UPPER_NAV2)); //UPPER NAV 2
            result.Add(new RadioPanelKnobSA342(0, Convert.ToInt32("10000", 2), true, RadioPanelPZ69KnobsSA342.UPPER_ADF)); //UPPER ADF
            result.Add(new RadioPanelKnobSA342(0, Convert.ToInt32("100000", 2), true, RadioPanelPZ69KnobsSA342.UPPER_DME)); //UPPER DME
            result.Add(new RadioPanelKnobSA342(0, Convert.ToInt32("1000000", 2), true, RadioPanelPZ69KnobsSA342.UPPER_XPDR)); //UPPER XPDR
            result.Add(new RadioPanelKnobSA342(0, Convert.ToInt32("10000000", 2), true, RadioPanelPZ69KnobsSA342.LOWER_VHFAM)); //LOWER COM 1 
            return result;
        }




    }
}
