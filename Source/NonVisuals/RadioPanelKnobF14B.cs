using System;
using System.Collections.Generic;

namespace NonVisuals
{
    public enum RadioPanelPZ69KnobsF14B
    {
        UPPER_UHF = 0,
        UPPER_VUHF = 2,
        UPPER_NOUSE = 4,
        UPPER_NOUSE2 = 8,
        UPPER_TACAN = 16,
        UPPER_KY28 = 32,
        UPPER_NOUSE3 = 64,
        UPPER_SMALL_FREQ_WHEEL_INC = 128,
        UPPER_SMALL_FREQ_WHEEL_DEC = 256,
        UPPER_LARGE_FREQ_WHEEL_INC = 512,
        UPPER_LARGE_FREQ_WHEEL_DEC = 1024,
        UPPER_FREQ_SWITCH = 2056,
        LOWER_UHF = 4096,
        LOWER_VUHF = 8192,
        LOWER_NOUSE = 16384,
        LOWER_NOUSE2 = 32768,
        LOWER_TACAN = 65536,
        LOWER_KY28 = 131072,
        LOWER_NOUSE3 = 262144,
        LOWER_SMALL_FREQ_WHEEL_INC = 8388608,
        LOWER_SMALL_FREQ_WHEEL_DEC = 524288,
        LOWER_LARGE_FREQ_WHEEL_INC = 1048576,
        LOWER_LARGE_FREQ_WHEEL_DEC = 2097152,
        LOWER_FREQ_SWITCH = 4194304
    }

    public enum CurrentF14RadioMode
    {
        UHF = 0,
        VUHF = 2,
        TACAN = 4,
        KY28 = 16,
        NOUSE
    }

    public class RadioPanelKnobF14B
    {
        public RadioPanelKnobF14B(int group, int mask, bool isOn, RadioPanelPZ69KnobsF14B radioPanelPZ69Knob)
        {
            Group = group;
            Mask = mask;
            IsOn = isOn;
            RadioPanelPZ69Knob = radioPanelPZ69Knob;
        }

        public int Group { get; set; }

        public int Mask { get; set; }

        public bool IsOn { get; set; }

        public RadioPanelPZ69KnobsF14B RadioPanelPZ69Knob { get; set; }

        public string ExportString()
        {
            return "RadioPanelKnob{" + Enum.GetName(typeof(RadioPanelPZ69KnobsF14B), RadioPanelPZ69Knob) + "}";
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
            RadioPanelPZ69Knob = (RadioPanelPZ69KnobsF14B)Enum.Parse(typeof(RadioPanelPZ69KnobsF14B), dataString.Trim());
        }

        public static HashSet<RadioPanelKnobF14B> GetRadioPanelKnobs()
        {
            //true means clockwise turn
            var result = new HashSet<RadioPanelKnobF14B>();
            //Group 0
            result.Add(new RadioPanelKnobF14B(2, Convert.ToInt32("1", 2), true, RadioPanelPZ69KnobsF14B.UPPER_SMALL_FREQ_WHEEL_INC));
            result.Add(new RadioPanelKnobF14B(2, Convert.ToInt32("10", 2), false, RadioPanelPZ69KnobsF14B.UPPER_SMALL_FREQ_WHEEL_DEC));
            result.Add(new RadioPanelKnobF14B(2, Convert.ToInt32("100", 2), true, RadioPanelPZ69KnobsF14B.UPPER_LARGE_FREQ_WHEEL_INC));
            result.Add(new RadioPanelKnobF14B(2, Convert.ToInt32("1000", 2), false, RadioPanelPZ69KnobsF14B.UPPER_LARGE_FREQ_WHEEL_DEC));
            result.Add(new RadioPanelKnobF14B(2, Convert.ToInt32("10000", 2), true, RadioPanelPZ69KnobsF14B.LOWER_SMALL_FREQ_WHEEL_INC));
            result.Add(new RadioPanelKnobF14B(2, Convert.ToInt32("100000", 2), false, RadioPanelPZ69KnobsF14B.LOWER_SMALL_FREQ_WHEEL_DEC));
            result.Add(new RadioPanelKnobF14B(2, Convert.ToInt32("1000000", 2), true, RadioPanelPZ69KnobsF14B.LOWER_LARGE_FREQ_WHEEL_INC));
            result.Add(new RadioPanelKnobF14B(2, Convert.ToInt32("10000000", 2), false, RadioPanelPZ69KnobsF14B.LOWER_LARGE_FREQ_WHEEL_DEC));

            //Group 1
            result.Add(new RadioPanelKnobF14B(1, Convert.ToInt32("1", 2), true, RadioPanelPZ69KnobsF14B.LOWER_VUHF));
            result.Add(new RadioPanelKnobF14B(1, Convert.ToInt32("10", 2), true, RadioPanelPZ69KnobsF14B.LOWER_NOUSE));
            result.Add(new RadioPanelKnobF14B(1, Convert.ToInt32("100", 2), true, RadioPanelPZ69KnobsF14B.LOWER_NOUSE2));
            result.Add(new RadioPanelKnobF14B(1, Convert.ToInt32("1000", 2), true, RadioPanelPZ69KnobsF14B.LOWER_TACAN));
            result.Add(new RadioPanelKnobF14B(1, Convert.ToInt32("10000", 2), true, RadioPanelPZ69KnobsF14B.LOWER_KY28));
            result.Add(new RadioPanelKnobF14B(1, Convert.ToInt32("100000", 2), true, RadioPanelPZ69KnobsF14B.LOWER_NOUSE3));
            result.Add(new RadioPanelKnobF14B(1, Convert.ToInt32("1000000", 2), true, RadioPanelPZ69KnobsF14B.UPPER_FREQ_SWITCH));
            result.Add(new RadioPanelKnobF14B(1, Convert.ToInt32("10000000", 2), true, RadioPanelPZ69KnobsF14B.LOWER_FREQ_SWITCH));

            //Group 2
            result.Add(new RadioPanelKnobF14B(0, Convert.ToInt32("1", 2), true, RadioPanelPZ69KnobsF14B.UPPER_UHF)); //UPPER COM 1
            result.Add(new RadioPanelKnobF14B(0, Convert.ToInt32("10", 2), true, RadioPanelPZ69KnobsF14B.UPPER_VUHF)); //UPPER COM 2
            result.Add(new RadioPanelKnobF14B(0, Convert.ToInt32("100", 2), true, RadioPanelPZ69KnobsF14B.UPPER_NOUSE)); //UPPER NAV 1
            result.Add(new RadioPanelKnobF14B(0, Convert.ToInt32("1000", 2), true, RadioPanelPZ69KnobsF14B.UPPER_NOUSE2)); //UPPER NAV 2
            result.Add(new RadioPanelKnobF14B(0, Convert.ToInt32("10000", 2), true, RadioPanelPZ69KnobsF14B.UPPER_TACAN)); //UPPER ADF
            result.Add(new RadioPanelKnobF14B(0, Convert.ToInt32("100000", 2), true, RadioPanelPZ69KnobsF14B.UPPER_KY28)); //UPPER DME
            result.Add(new RadioPanelKnobF14B(0, Convert.ToInt32("1000000", 2), true, RadioPanelPZ69KnobsF14B.UPPER_NOUSE3)); //UPPER XPDR
            result.Add(new RadioPanelKnobF14B(0, Convert.ToInt32("10000000", 2), true, RadioPanelPZ69KnobsF14B.LOWER_UHF)); //LOWER COM 1 
            return result;
        }




    }
}
