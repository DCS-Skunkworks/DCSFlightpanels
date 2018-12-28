using System;
using System.Collections.Generic;

namespace NonVisuals
{
    public enum RadioPanelPZ69KnobsAJS37
    {
        UPPER_FR22 = 0,      //COM1
        UPPER_FR24 = 2,      //COM2
        UPPER_TILS = 4,          //NAV1
        UPPER_NOUSE0 = 8,             //NAV2
        UPPER_NOUSE1 = 16,       //ADF
        UPPER_NOUSE2 = 32,          //DME_
        UPPER_NOUSE3 = 64,            //XPDR
        UPPER_SMALL_FREQ_WHEEL_INC = 128,
        UPPER_SMALL_FREQ_WHEEL_DEC = 256,
        UPPER_LARGE_FREQ_WHEEL_INC = 512,
        UPPER_LARGE_FREQ_WHEEL_DEC = 1024,
        UPPER_FREQ_SWITCH = 2056,
        LOWER_FR22 = 4096,   //COM1
        LOWER_FR24 = 8192,   //COM2
        LOWER_TILS = 16384,      //NAV1
        LOWER_NOUSE0 = 32768,          //NAV2
        LOWER_NOUSE1 = 65536,    //ADF
        LOWER_NOUSE2 = 131072,      //DME_
        LOWER_NOUSE3 = 262144,        //XPDR
        LOWER_SMALL_FREQ_WHEEL_INC = 8388608,
        LOWER_SMALL_FREQ_WHEEL_DEC = 524288,
        LOWER_LARGE_FREQ_WHEEL_INC = 1048576,
        LOWER_LARGE_FREQ_WHEEL_DEC = 2097152,
        LOWER_FREQ_SWITCH = 4194304
    }

    public enum CurrentAJS37RadioMode
    {
        FR22 = 0,
        FR24 = 2,
        TILS = 4,
        NOUSE = 64
    }

    public class RadioPanelKnobAJS37
    {
        public RadioPanelKnobAJS37(int group, int mask, bool isOn, RadioPanelPZ69KnobsAJS37 radioPanelPZ69Knob)
        {
            Group = group;
            Mask = mask;
            IsOn = isOn;
            RadioPanelPZ69Knob = radioPanelPZ69Knob;
        }

        public int Group { get; set; }

        public int Mask { get; set; }

        public bool IsOn { get; set; }

        public RadioPanelPZ69KnobsAJS37 RadioPanelPZ69Knob { get; set; }

        public string ExportString()
        {
            return "RadioPanelKnob{" + Enum.GetName(typeof(RadioPanelPZ69KnobsAJS37), RadioPanelPZ69Knob) + "}";
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
            RadioPanelPZ69Knob = (RadioPanelPZ69KnobsAJS37)Enum.Parse(typeof(RadioPanelPZ69KnobsAJS37), dataString.Trim());
        }

        public static HashSet<RadioPanelKnobAJS37> GetRadioPanelKnobs()
        {
            //true means clockwise turn
            var result = new HashSet<RadioPanelKnobAJS37>();
            //Group 0
            result.Add(new RadioPanelKnobAJS37(2, Convert.ToInt32("1", 2), true, RadioPanelPZ69KnobsAJS37.UPPER_SMALL_FREQ_WHEEL_INC));
            result.Add(new RadioPanelKnobAJS37(2, Convert.ToInt32("10", 2), false, RadioPanelPZ69KnobsAJS37.UPPER_SMALL_FREQ_WHEEL_DEC));
            result.Add(new RadioPanelKnobAJS37(2, Convert.ToInt32("100", 2), true, RadioPanelPZ69KnobsAJS37.UPPER_LARGE_FREQ_WHEEL_INC));
            result.Add(new RadioPanelKnobAJS37(2, Convert.ToInt32("1000", 2), false, RadioPanelPZ69KnobsAJS37.UPPER_LARGE_FREQ_WHEEL_DEC));
            result.Add(new RadioPanelKnobAJS37(2, Convert.ToInt32("10000", 2), true, RadioPanelPZ69KnobsAJS37.LOWER_SMALL_FREQ_WHEEL_INC));
            result.Add(new RadioPanelKnobAJS37(2, Convert.ToInt32("100000", 2), false, RadioPanelPZ69KnobsAJS37.LOWER_SMALL_FREQ_WHEEL_DEC));
            result.Add(new RadioPanelKnobAJS37(2, Convert.ToInt32("1000000", 2), true, RadioPanelPZ69KnobsAJS37.LOWER_LARGE_FREQ_WHEEL_INC));
            result.Add(new RadioPanelKnobAJS37(2, Convert.ToInt32("10000000", 2), false, RadioPanelPZ69KnobsAJS37.LOWER_LARGE_FREQ_WHEEL_DEC));

            //Group 1
            result.Add(new RadioPanelKnobAJS37(1, Convert.ToInt32("1", 2), true, RadioPanelPZ69KnobsAJS37.LOWER_FR24)); //LOWER COM2
            result.Add(new RadioPanelKnobAJS37(1, Convert.ToInt32("10", 2), true, RadioPanelPZ69KnobsAJS37.LOWER_TILS)); //LOWER NAV1
            result.Add(new RadioPanelKnobAJS37(1, Convert.ToInt32("100", 2), true, RadioPanelPZ69KnobsAJS37.LOWER_NOUSE0)); //LOWER NAV2
            result.Add(new RadioPanelKnobAJS37(1, Convert.ToInt32("1000", 2), true, RadioPanelPZ69KnobsAJS37.LOWER_NOUSE1)); //LOWER ADF
            result.Add(new RadioPanelKnobAJS37(1, Convert.ToInt32("10000", 2), true, RadioPanelPZ69KnobsAJS37.LOWER_NOUSE2)); //LOWER DME
            result.Add(new RadioPanelKnobAJS37(1, Convert.ToInt32("100000", 2), true, RadioPanelPZ69KnobsAJS37.LOWER_NOUSE3)); //LOWER XPDR
            result.Add(new RadioPanelKnobAJS37(1, Convert.ToInt32("1000000", 2), true, RadioPanelPZ69KnobsAJS37.UPPER_FREQ_SWITCH));
            result.Add(new RadioPanelKnobAJS37(1, Convert.ToInt32("10000000", 2), true, RadioPanelPZ69KnobsAJS37.LOWER_FREQ_SWITCH));

            //Group 2
            result.Add(new RadioPanelKnobAJS37(0, Convert.ToInt32("1", 2), true, RadioPanelPZ69KnobsAJS37.UPPER_FR22)); //UPPER COM1
            result.Add(new RadioPanelKnobAJS37(0, Convert.ToInt32("10", 2), true, RadioPanelPZ69KnobsAJS37.UPPER_FR24)); //UPPER COM2
            result.Add(new RadioPanelKnobAJS37(0, Convert.ToInt32("100", 2), true, RadioPanelPZ69KnobsAJS37.UPPER_TILS)); //UPPER NAV1
            result.Add(new RadioPanelKnobAJS37(0, Convert.ToInt32("1000", 2), true, RadioPanelPZ69KnobsAJS37.UPPER_NOUSE0)); //UPPER NAV2
            result.Add(new RadioPanelKnobAJS37(0, Convert.ToInt32("10000", 2), true, RadioPanelPZ69KnobsAJS37.UPPER_NOUSE1)); //UPPER ADF
            result.Add(new RadioPanelKnobAJS37(0, Convert.ToInt32("100000", 2), true, RadioPanelPZ69KnobsAJS37.UPPER_NOUSE2)); //UPPER DME
            result.Add(new RadioPanelKnobAJS37(0, Convert.ToInt32("1000000", 2), true, RadioPanelPZ69KnobsAJS37.UPPER_NOUSE3)); //UPPER XPDR
            result.Add(new RadioPanelKnobAJS37(0, Convert.ToInt32("10000000", 2), true, RadioPanelPZ69KnobsAJS37.LOWER_FR22)); //LOWER COM1
            return result;
        }




    }


}
