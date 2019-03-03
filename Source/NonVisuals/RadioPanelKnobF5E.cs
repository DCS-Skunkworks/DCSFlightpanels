using System;
using System.Collections.Generic;

namespace NonVisuals
{
    public enum RadioPanelPZ69KnobsF5E
    {
        UPPER_UHF = 0,
        UPPER_NOUSE1 = 2,
        UPPER_NOUSE2 = 4,
        UPPER_NOUSE3 = 8,
        UPPER_TACAN = 16,
        UPPER_NOUSE4 = 32,
        UPPER_NOUSE5 = 64,
        UPPER_SMALL_FREQ_WHEEL_INC = 128,
        UPPER_SMALL_FREQ_WHEEL_DEC = 256,
        UPPER_LARGE_FREQ_WHEEL_INC = 512,
        UPPER_LARGE_FREQ_WHEEL_DEC = 1024,
        UPPER_FREQ_SWITCH = 2056,
        LOWER_UHF = 4096,
        LOWER_NOUSE1 = 8192,
        LOWER_NOUSE2 = 16384,
        LOWER_NOUSE3 = 32768,
        LOWER_TACAN = 65536,
        LOWER_NOUSE4 = 131072,
        LOWER_NOUSE5 = 262144,
        LOWER_SMALL_FREQ_WHEEL_INC = 8388608,
        LOWER_SMALL_FREQ_WHEEL_DEC = 524288,
        LOWER_LARGE_FREQ_WHEEL_INC = 1048576,
        LOWER_LARGE_FREQ_WHEEL_DEC = 2097152,
        LOWER_FREQ_SWITCH = 4194304
    }

    public enum CurrentF5ERadioMode
    {
        UHF = 0,
        TACAN = 8,
        NO_USE = 16
    }

    public class RadioPanelKnobF5E
    {
        public RadioPanelKnobF5E(int group, int mask, bool isOn, RadioPanelPZ69KnobsF5E radioPanelPZ69Knob)
        {
            Group = group;
            Mask = mask;
            IsOn = isOn;
            RadioPanelPZ69Knob = radioPanelPZ69Knob;
        }

        public int Group { get; set; }

        public int Mask { get; set; }

        public bool IsOn { get; set; }

        public RadioPanelPZ69KnobsF5E RadioPanelPZ69Knob { get; set; }

        public string ExportString()
        {
            return "RadioPanelKnob{" + Enum.GetName(typeof(RadioPanelPZ69KnobsF5E), RadioPanelPZ69Knob) + "}";
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
            RadioPanelPZ69Knob = (RadioPanelPZ69KnobsF5E)Enum.Parse(typeof(RadioPanelPZ69KnobsF5E), dataString.Trim());
        }

        public static HashSet<RadioPanelKnobF5E> GetRadioPanelKnobs()
        {
            //true means clockwise turn
            var result = new HashSet<RadioPanelKnobF5E>();
            //Group 0
            result.Add(new RadioPanelKnobF5E(2, Convert.ToInt32("1", 2), true, RadioPanelPZ69KnobsF5E.UPPER_SMALL_FREQ_WHEEL_INC));
            result.Add(new RadioPanelKnobF5E(2, Convert.ToInt32("10", 2), false, RadioPanelPZ69KnobsF5E.UPPER_SMALL_FREQ_WHEEL_DEC));
            result.Add(new RadioPanelKnobF5E(2, Convert.ToInt32("100", 2), true, RadioPanelPZ69KnobsF5E.UPPER_LARGE_FREQ_WHEEL_INC));
            result.Add(new RadioPanelKnobF5E(2, Convert.ToInt32("1000", 2), false, RadioPanelPZ69KnobsF5E.UPPER_LARGE_FREQ_WHEEL_DEC));
            result.Add(new RadioPanelKnobF5E(2, Convert.ToInt32("10000", 2), true, RadioPanelPZ69KnobsF5E.LOWER_SMALL_FREQ_WHEEL_INC));
            result.Add(new RadioPanelKnobF5E(2, Convert.ToInt32("100000", 2), false, RadioPanelPZ69KnobsF5E.LOWER_SMALL_FREQ_WHEEL_DEC));
            result.Add(new RadioPanelKnobF5E(2, Convert.ToInt32("1000000", 2), true, RadioPanelPZ69KnobsF5E.LOWER_LARGE_FREQ_WHEEL_INC));
            result.Add(new RadioPanelKnobF5E(2, Convert.ToInt32("10000000", 2), false, RadioPanelPZ69KnobsF5E.LOWER_LARGE_FREQ_WHEEL_DEC));

            //Group 1
            result.Add(new RadioPanelKnobF5E(1, Convert.ToInt32("1", 2), true, RadioPanelPZ69KnobsF5E.LOWER_NOUSE1)); //LOWER COM 2
            result.Add(new RadioPanelKnobF5E(1, Convert.ToInt32("10", 2), true, RadioPanelPZ69KnobsF5E.LOWER_NOUSE2)); //LOWER NAV 1
            result.Add(new RadioPanelKnobF5E(1, Convert.ToInt32("100", 2), true, RadioPanelPZ69KnobsF5E.LOWER_NOUSE3)); //LOWER NAV 2
            result.Add(new RadioPanelKnobF5E(1, Convert.ToInt32("1000", 2), true, RadioPanelPZ69KnobsF5E.LOWER_TACAN)); //LOWER ADF
            result.Add(new RadioPanelKnobF5E(1, Convert.ToInt32("10000", 2), true, RadioPanelPZ69KnobsF5E.LOWER_NOUSE4)); //LOWER DME
            result.Add(new RadioPanelKnobF5E(1, Convert.ToInt32("100000", 2), true, RadioPanelPZ69KnobsF5E.LOWER_NOUSE5)); //LOWER XPDR
            result.Add(new RadioPanelKnobF5E(1, Convert.ToInt32("1000000", 2), true, RadioPanelPZ69KnobsF5E.UPPER_FREQ_SWITCH));
            result.Add(new RadioPanelKnobF5E(1, Convert.ToInt32("10000000", 2), true, RadioPanelPZ69KnobsF5E.LOWER_FREQ_SWITCH));

            //Group 2
            result.Add(new RadioPanelKnobF5E(0, Convert.ToInt32("1", 2), true, RadioPanelPZ69KnobsF5E.UPPER_UHF)); //UPPER COM 1
            result.Add(new RadioPanelKnobF5E(0, Convert.ToInt32("10", 2), true, RadioPanelPZ69KnobsF5E.UPPER_NOUSE1)); //UPPER COM 2
            result.Add(new RadioPanelKnobF5E(0, Convert.ToInt32("100", 2), true, RadioPanelPZ69KnobsF5E.UPPER_NOUSE2)); //UPPER NAV 1
            result.Add(new RadioPanelKnobF5E(0, Convert.ToInt32("1000", 2), true, RadioPanelPZ69KnobsF5E.UPPER_NOUSE3)); //UPPER NAV 2
            result.Add(new RadioPanelKnobF5E(0, Convert.ToInt32("10000", 2), true, RadioPanelPZ69KnobsF5E.UPPER_TACAN)); //UPPER ADF
            result.Add(new RadioPanelKnobF5E(0, Convert.ToInt32("100000", 2), true, RadioPanelPZ69KnobsF5E.UPPER_NOUSE4)); //UPPER DME
            result.Add(new RadioPanelKnobF5E(0, Convert.ToInt32("1000000", 2), true, RadioPanelPZ69KnobsF5E.UPPER_NOUSE5)); //UPPER XPDR
            result.Add(new RadioPanelKnobF5E(0, Convert.ToInt32("10000000", 2), true, RadioPanelPZ69KnobsF5E.LOWER_UHF)); //LOWER COM 1 
            return result;
        }




    }
}
