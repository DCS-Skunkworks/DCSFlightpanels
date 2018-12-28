using System;
using System.Collections.Generic;

namespace NonVisuals
{
    public enum RadioPanelPZ69KnobsP51D
    {
        UPPER_VHF = 0,   //COM1
        UPPER_NOUSE0 = 2,  //COM2
        UPPER_NOUSE1 = 4, //NAV1
        UPPER_NOUSE2 = 8, //NAV2
        UPPER_NOUSE3 = 16, //ADF
        UPPER_NOUSE4 = 32, //DME_
        UPPER_NOUSE5 = 64, //XPDR
        UPPER_SMALL_FREQ_WHEEL_INC = 128,
        UPPER_SMALL_FREQ_WHEEL_DEC = 256,
        UPPER_LARGE_FREQ_WHEEL_INC = 512,
        UPPER_LARGE_FREQ_WHEEL_DEC = 1024,
        UPPER_FREQ_SWITCH = 2056,
        LOWER_VHF = 4096,
        LOWER_NOUSE0 = 8192,
        LOWER_NOUSE1 = 16384,
        LOWER_NOUSE2 = 32768,
        LOWER_NOUSE3 = 65536,
        LOWER_NOUSE4 = 131072,
        LOWER_NOUSE5 = 262144,
        LOWER_SMALL_FREQ_WHEEL_INC = 8388608,
        LOWER_SMALL_FREQ_WHEEL_DEC = 524288,
        LOWER_LARGE_FREQ_WHEEL_INC = 1048576,
        LOWER_LARGE_FREQ_WHEEL_DEC = 2097152,
        LOWER_FREQ_SWITCH = 4194304
    }

    public enum CurrentP51DRadioMode
    {
        VHF = 0,
        NOUSE = 2
    }

    public class RadioPanelKnobP51D
    {
        public RadioPanelKnobP51D(int group, int mask, bool isOn, RadioPanelPZ69KnobsP51D radioPanelPZ69Knob)
        {
            Group = group;
            Mask = mask;
            IsOn = isOn;
            RadioPanelPZ69Knob = radioPanelPZ69Knob;
        }

        public int Group { get; set; }

        public int Mask { get; set; }

        public bool IsOn { get; set; }

        public RadioPanelPZ69KnobsP51D RadioPanelPZ69Knob { get; set; }

        public string ExportString()
        {
            return "RadioPanelKnob{" + Enum.GetName(typeof(RadioPanelPZ69KnobsP51D), RadioPanelPZ69Knob) + "}";
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
            RadioPanelPZ69Knob = (RadioPanelPZ69KnobsP51D)Enum.Parse(typeof(RadioPanelPZ69KnobsP51D), dataString.Trim());
        }

        public static HashSet<RadioPanelKnobP51D> GetRadioPanelKnobs()
        {
            //true means clockwise turn
            var result = new HashSet<RadioPanelKnobP51D>();
            //Group 0
            result.Add(new RadioPanelKnobP51D(2, Convert.ToInt32("1", 2), true, RadioPanelPZ69KnobsP51D.UPPER_SMALL_FREQ_WHEEL_INC));
            result.Add(new RadioPanelKnobP51D(2, Convert.ToInt32("10", 2), false, RadioPanelPZ69KnobsP51D.UPPER_SMALL_FREQ_WHEEL_DEC));
            result.Add(new RadioPanelKnobP51D(2, Convert.ToInt32("100", 2), true, RadioPanelPZ69KnobsP51D.UPPER_LARGE_FREQ_WHEEL_INC));
            result.Add(new RadioPanelKnobP51D(2, Convert.ToInt32("1000", 2), false, RadioPanelPZ69KnobsP51D.UPPER_LARGE_FREQ_WHEEL_DEC));
            result.Add(new RadioPanelKnobP51D(2, Convert.ToInt32("10000", 2), true, RadioPanelPZ69KnobsP51D.LOWER_SMALL_FREQ_WHEEL_INC));
            result.Add(new RadioPanelKnobP51D(2, Convert.ToInt32("100000", 2), false, RadioPanelPZ69KnobsP51D.LOWER_SMALL_FREQ_WHEEL_DEC));
            result.Add(new RadioPanelKnobP51D(2, Convert.ToInt32("1000000", 2), true, RadioPanelPZ69KnobsP51D.LOWER_LARGE_FREQ_WHEEL_INC));
            result.Add(new RadioPanelKnobP51D(2, Convert.ToInt32("10000000", 2), false, RadioPanelPZ69KnobsP51D.LOWER_LARGE_FREQ_WHEEL_DEC));

            //Group 1
            result.Add(new RadioPanelKnobP51D(1, Convert.ToInt32("1", 2), true, RadioPanelPZ69KnobsP51D.LOWER_NOUSE0)); //LOWER COM2
            result.Add(new RadioPanelKnobP51D(1, Convert.ToInt32("10", 2), true, RadioPanelPZ69KnobsP51D.LOWER_NOUSE1)); //LOWER NAV1
            result.Add(new RadioPanelKnobP51D(1, Convert.ToInt32("100", 2), true, RadioPanelPZ69KnobsP51D.LOWER_NOUSE2)); //LOWER NAV2
            result.Add(new RadioPanelKnobP51D(1, Convert.ToInt32("1000", 2), true, RadioPanelPZ69KnobsP51D.LOWER_NOUSE3)); //LOWER ADF
            result.Add(new RadioPanelKnobP51D(1, Convert.ToInt32("10000", 2), true, RadioPanelPZ69KnobsP51D.LOWER_NOUSE4)); //LOWER DME
            result.Add(new RadioPanelKnobP51D(1, Convert.ToInt32("100000", 2), true, RadioPanelPZ69KnobsP51D.LOWER_NOUSE5)); //LOWER XPDR
            result.Add(new RadioPanelKnobP51D(1, Convert.ToInt32("1000000", 2), true, RadioPanelPZ69KnobsP51D.UPPER_FREQ_SWITCH));
            result.Add(new RadioPanelKnobP51D(1, Convert.ToInt32("10000000", 2), true, RadioPanelPZ69KnobsP51D.LOWER_FREQ_SWITCH));

            //Group 2
            result.Add(new RadioPanelKnobP51D(0, Convert.ToInt32("1", 2), true, RadioPanelPZ69KnobsP51D.UPPER_VHF)); //UPPER COM1
            result.Add(new RadioPanelKnobP51D(0, Convert.ToInt32("10", 2), true, RadioPanelPZ69KnobsP51D.UPPER_NOUSE0)); //UPPER COM2
            result.Add(new RadioPanelKnobP51D(0, Convert.ToInt32("100", 2), true, RadioPanelPZ69KnobsP51D.UPPER_NOUSE1)); //UPPER NAV1
            result.Add(new RadioPanelKnobP51D(0, Convert.ToInt32("1000", 2), true, RadioPanelPZ69KnobsP51D.UPPER_NOUSE2)); //UPPER NAV2
            result.Add(new RadioPanelKnobP51D(0, Convert.ToInt32("10000", 2), true, RadioPanelPZ69KnobsP51D.UPPER_NOUSE3)); //UPPER ADF
            result.Add(new RadioPanelKnobP51D(0, Convert.ToInt32("100000", 2), true, RadioPanelPZ69KnobsP51D.UPPER_NOUSE4)); //UPPER DME
            result.Add(new RadioPanelKnobP51D(0, Convert.ToInt32("1000000", 2), true, RadioPanelPZ69KnobsP51D.UPPER_NOUSE5)); //UPPER XPDR
            result.Add(new RadioPanelKnobP51D(0, Convert.ToInt32("10000000", 2), true, RadioPanelPZ69KnobsP51D.LOWER_VHF)); //LOWER COM1
            return result;
        }




    }


}
