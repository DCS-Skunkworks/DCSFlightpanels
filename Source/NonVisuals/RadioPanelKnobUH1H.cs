using System;
using System.Collections.Generic;

namespace NonVisuals
{
    public enum RadioPanelPZ69KnobsUH1H
    {
        UPPER_VHFCOMM = 0,
        UPPER_UHF = 2,
        UPPER_VHFNAV = 4,
        UPPER_VHFFM = 8,
        UPPER_ADF = 16,
        UPPER_DME = 32,
        UPPER_INTERCOMM = 64,
        UPPER_SMALL_FREQ_WHEEL_INC = 128,
        UPPER_SMALL_FREQ_WHEEL_DEC = 256,
        UPPER_LARGE_FREQ_WHEEL_INC = 512,
        UPPER_LARGE_FREQ_WHEEL_DEC = 1024,
        UPPER_FREQ_SWITCH = 2056,
        LOWER_VHFCOMM = 4096,
        LOWER_UHF = 8192,
        LOWER_VHFNAV = 16384,
        LOWER_VHFFM = 32768,
        LOWER_ADF = 65536,
        LOWER_DME = 131072,
        LOWER_INTERCOMM = 262144,
        LOWER_SMALL_FREQ_WHEEL_INC = 8388608,
        LOWER_SMALL_FREQ_WHEEL_DEC = 524288,
        LOWER_LARGE_FREQ_WHEEL_INC = 1048576,
        LOWER_LARGE_FREQ_WHEEL_DEC = 2097152,
        LOWER_FREQ_SWITCH = 4194304
    }

    public enum CurrentUH1HRadioMode
    {
        INTERCOMM = 0,
        VHFCOMM = 2,
        UHF = 4,
        VHFFM = 8,
        VHFNAV = 16,
        ADF = 32
    }

    public class RadioPanelKnobUH1H
    {
        public RadioPanelKnobUH1H(int group, int mask, bool isOn, RadioPanelPZ69KnobsUH1H radioPanelPZ69Knob)
        {
            Group = group;
            Mask = mask;
            IsOn = isOn;
            RadioPanelPZ69Knob = radioPanelPZ69Knob;
        }

        public int Group { get; set; }

        public int Mask { get; set; }

        public bool IsOn { get; set; }

        public RadioPanelPZ69KnobsUH1H RadioPanelPZ69Knob { get; set; }

        public string ExportString()
        {
            return "RadioPanelKnob{" + Enum.GetName(typeof(RadioPanelPZ69KnobsUH1H), RadioPanelPZ69Knob) + "}";
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
            RadioPanelPZ69Knob = (RadioPanelPZ69KnobsUH1H)Enum.Parse(typeof(RadioPanelPZ69KnobsUH1H), dataString.Trim());
        }

        public static HashSet<RadioPanelKnobUH1H> GetRadioPanelKnobs()
        {
            //true means clockwise turn
            var result = new HashSet<RadioPanelKnobUH1H>();
            //Group 0
            result.Add(new RadioPanelKnobUH1H(2, Convert.ToInt32("1", 2), true, RadioPanelPZ69KnobsUH1H.UPPER_SMALL_FREQ_WHEEL_INC));
            result.Add(new RadioPanelKnobUH1H(2, Convert.ToInt32("10", 2), false, RadioPanelPZ69KnobsUH1H.UPPER_SMALL_FREQ_WHEEL_DEC));
            result.Add(new RadioPanelKnobUH1H(2, Convert.ToInt32("100", 2), true, RadioPanelPZ69KnobsUH1H.UPPER_LARGE_FREQ_WHEEL_INC));
            result.Add(new RadioPanelKnobUH1H(2, Convert.ToInt32("1000", 2), false, RadioPanelPZ69KnobsUH1H.UPPER_LARGE_FREQ_WHEEL_DEC));
            result.Add(new RadioPanelKnobUH1H(2, Convert.ToInt32("10000", 2), true, RadioPanelPZ69KnobsUH1H.LOWER_SMALL_FREQ_WHEEL_INC));
            result.Add(new RadioPanelKnobUH1H(2, Convert.ToInt32("100000", 2), false, RadioPanelPZ69KnobsUH1H.LOWER_SMALL_FREQ_WHEEL_DEC));
            result.Add(new RadioPanelKnobUH1H(2, Convert.ToInt32("1000000", 2), true, RadioPanelPZ69KnobsUH1H.LOWER_LARGE_FREQ_WHEEL_INC));
            result.Add(new RadioPanelKnobUH1H(2, Convert.ToInt32("10000000", 2), false, RadioPanelPZ69KnobsUH1H.LOWER_LARGE_FREQ_WHEEL_DEC));
            
            //Group 1
            result.Add(new RadioPanelKnobUH1H(1, Convert.ToInt32("1", 2), true, RadioPanelPZ69KnobsUH1H.LOWER_UHF));
            result.Add(new RadioPanelKnobUH1H(1, Convert.ToInt32("10", 2), true, RadioPanelPZ69KnobsUH1H.LOWER_VHFNAV));
            result.Add(new RadioPanelKnobUH1H(1, Convert.ToInt32("100", 2), true, RadioPanelPZ69KnobsUH1H.LOWER_VHFFM));
            result.Add(new RadioPanelKnobUH1H(1, Convert.ToInt32("1000", 2), true, RadioPanelPZ69KnobsUH1H.LOWER_ADF));
            result.Add(new RadioPanelKnobUH1H(1, Convert.ToInt32("10000", 2), true, RadioPanelPZ69KnobsUH1H.LOWER_DME));
            result.Add(new RadioPanelKnobUH1H(1, Convert.ToInt32("100000", 2), true, RadioPanelPZ69KnobsUH1H.LOWER_INTERCOMM));
            result.Add(new RadioPanelKnobUH1H(1, Convert.ToInt32("1000000", 2), true, RadioPanelPZ69KnobsUH1H.UPPER_FREQ_SWITCH));
            result.Add(new RadioPanelKnobUH1H(1, Convert.ToInt32("10000000", 2), true, RadioPanelPZ69KnobsUH1H.LOWER_FREQ_SWITCH));

            //Group 2
            result.Add(new RadioPanelKnobUH1H(0, Convert.ToInt32("1", 2), true, RadioPanelPZ69KnobsUH1H.UPPER_VHFCOMM));
            result.Add(new RadioPanelKnobUH1H(0, Convert.ToInt32("10", 2), true, RadioPanelPZ69KnobsUH1H.UPPER_UHF));
            result.Add(new RadioPanelKnobUH1H(0, Convert.ToInt32("100", 2), true, RadioPanelPZ69KnobsUH1H.UPPER_VHFNAV));
            result.Add(new RadioPanelKnobUH1H(0, Convert.ToInt32("1000", 2), true, RadioPanelPZ69KnobsUH1H.UPPER_VHFFM));
            result.Add(new RadioPanelKnobUH1H(0, Convert.ToInt32("10000", 2), true, RadioPanelPZ69KnobsUH1H.UPPER_ADF));
            result.Add(new RadioPanelKnobUH1H(0, Convert.ToInt32("100000", 2), true, RadioPanelPZ69KnobsUH1H.UPPER_DME));
            result.Add(new RadioPanelKnobUH1H(0, Convert.ToInt32("1000000", 2), true, RadioPanelPZ69KnobsUH1H.UPPER_INTERCOMM));
            result.Add(new RadioPanelKnobUH1H(0, Convert.ToInt32("10000000", 2), true, RadioPanelPZ69KnobsUH1H.LOWER_VHFCOMM));
            return result;
        }
    }
}
