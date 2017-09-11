using System;
using System.Collections.Generic;

namespace NonVisuals
{
    public enum RadioPanelPZ69KnobsMi8
    {
        UPPER_R863_MANUAL = 0,      //COM1
        UPPER_R863_PRESET = 2,      //COM2
        UPPER_YADRO1A = 4,          //NAV1
        UPPER_R828 = 8,             //NAV2
        UPPER_ADF_ARK9 = 16,       //ADF
        UPPER_NOUSE3 = 32,          //DME_
        UPPER_SPU7 = 64,            //XPDR
        UPPER_SMALL_FREQ_WHEEL_INC = 128,
        UPPER_SMALL_FREQ_WHEEL_DEC = 256,
        UPPER_LARGE_FREQ_WHEEL_INC = 512,
        UPPER_LARGE_FREQ_WHEEL_DEC = 1024,
        UPPER_FREQ_SWITCH = 2056,
        LOWER_R863_MANUAL = 4096,   //COM1
        LOWER_R863_PRESET = 8192,   //COM2
        LOWER_YADRO1A = 16384,      //NAV1
        LOWER_R828 = 32768,          //NAV2
        LOWER_ADF_ARK9 = 65536,    //ADF
        LOWER_NOUSE3 = 131072,      //DME_
        LOWER_SPU7 = 262144,        //XPDR
        LOWER_SMALL_FREQ_WHEEL_INC = 8388608,
        LOWER_SMALL_FREQ_WHEEL_DEC = 524288,
        LOWER_LARGE_FREQ_WHEEL_INC = 1048576,
        LOWER_LARGE_FREQ_WHEEL_DEC = 2097152,
        LOWER_FREQ_SWITCH = 4194304
    }

    public enum CurrentMi8RadioMode
    {
        R863_MANUAL = 0,
        R863_PRESET = 2,
        YADRO1A = 4,
        R828_PRESETS = 8,
        ADF_ARK9 = 16,
        SPU7 = 32,
        NOUSE = 64
    }

    public class RadioPanelKnobMi8
    {
        public RadioPanelKnobMi8(int group, int mask, bool isOn, RadioPanelPZ69KnobsMi8 radioPanelPZ69Knob)
        {
            Group = group;
            Mask = mask;
            IsOn = isOn;
            RadioPanelPZ69Knob = radioPanelPZ69Knob;
        }

        public int Group { get; set; }

        public int Mask { get; set; }

        public bool IsOn { get; set; }

        public RadioPanelPZ69KnobsMi8 RadioPanelPZ69Knob { get; set; }

        public string ExportString()
        {
            return "RadioPanelKnob{" + Enum.GetName(typeof(RadioPanelPZ69KnobsMi8), RadioPanelPZ69Knob) + "}";
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
            RadioPanelPZ69Knob = (RadioPanelPZ69KnobsMi8)Enum.Parse(typeof(RadioPanelPZ69KnobsMi8), dataString.Trim());
        }

        public static HashSet<RadioPanelKnobMi8> GetRadioPanelKnobs()
        {
            //true means clockwise turn
            var result = new HashSet<RadioPanelKnobMi8>();
            //Group 0
            result.Add(new RadioPanelKnobMi8(2, Convert.ToInt32("1", 2), true, RadioPanelPZ69KnobsMi8.UPPER_SMALL_FREQ_WHEEL_INC));
            result.Add(new RadioPanelKnobMi8(2, Convert.ToInt32("10", 2), false, RadioPanelPZ69KnobsMi8.UPPER_SMALL_FREQ_WHEEL_DEC));
            result.Add(new RadioPanelKnobMi8(2, Convert.ToInt32("100", 2), true, RadioPanelPZ69KnobsMi8.UPPER_LARGE_FREQ_WHEEL_INC));
            result.Add(new RadioPanelKnobMi8(2, Convert.ToInt32("1000", 2), false, RadioPanelPZ69KnobsMi8.UPPER_LARGE_FREQ_WHEEL_DEC));
            result.Add(new RadioPanelKnobMi8(2, Convert.ToInt32("10000", 2), true, RadioPanelPZ69KnobsMi8.LOWER_SMALL_FREQ_WHEEL_INC));
            result.Add(new RadioPanelKnobMi8(2, Convert.ToInt32("100000", 2), false, RadioPanelPZ69KnobsMi8.LOWER_SMALL_FREQ_WHEEL_DEC));
            result.Add(new RadioPanelKnobMi8(2, Convert.ToInt32("1000000", 2), true, RadioPanelPZ69KnobsMi8.LOWER_LARGE_FREQ_WHEEL_INC));
            result.Add(new RadioPanelKnobMi8(2, Convert.ToInt32("10000000", 2), false, RadioPanelPZ69KnobsMi8.LOWER_LARGE_FREQ_WHEEL_DEC));

            //Group 1
            result.Add(new RadioPanelKnobMi8(1, Convert.ToInt32("1", 2), true, RadioPanelPZ69KnobsMi8.LOWER_R863_PRESET)); //LOWER COM2
            result.Add(new RadioPanelKnobMi8(1, Convert.ToInt32("10", 2), true, RadioPanelPZ69KnobsMi8.LOWER_YADRO1A)); //LOWER NAV1
            result.Add(new RadioPanelKnobMi8(1, Convert.ToInt32("100", 2), true, RadioPanelPZ69KnobsMi8.LOWER_R828)); //LOWER NAV2
            result.Add(new RadioPanelKnobMi8(1, Convert.ToInt32("1000", 2), true, RadioPanelPZ69KnobsMi8.LOWER_ADF_ARK9)); //LOWER ADF
            result.Add(new RadioPanelKnobMi8(1, Convert.ToInt32("10000", 2), true, RadioPanelPZ69KnobsMi8.LOWER_NOUSE3)); //LOWER DME
            result.Add(new RadioPanelKnobMi8(1, Convert.ToInt32("100000", 2), true, RadioPanelPZ69KnobsMi8.LOWER_SPU7)); //LOWER XPDR
            result.Add(new RadioPanelKnobMi8(1, Convert.ToInt32("1000000", 2), true, RadioPanelPZ69KnobsMi8.UPPER_FREQ_SWITCH));
            result.Add(new RadioPanelKnobMi8(1, Convert.ToInt32("10000000", 2), true, RadioPanelPZ69KnobsMi8.LOWER_FREQ_SWITCH));

            //Group 2
            result.Add(new RadioPanelKnobMi8(0, Convert.ToInt32("1", 2), true, RadioPanelPZ69KnobsMi8.UPPER_R863_MANUAL)); //UPPER COM1
            result.Add(new RadioPanelKnobMi8(0, Convert.ToInt32("10", 2), true, RadioPanelPZ69KnobsMi8.UPPER_R863_PRESET)); //UPPER COM2
            result.Add(new RadioPanelKnobMi8(0, Convert.ToInt32("100", 2), true, RadioPanelPZ69KnobsMi8.UPPER_YADRO1A)); //UPPER NAV1
            result.Add(new RadioPanelKnobMi8(0, Convert.ToInt32("1000", 2), true, RadioPanelPZ69KnobsMi8.UPPER_R828)); //UPPER NAV2
            result.Add(new RadioPanelKnobMi8(0, Convert.ToInt32("10000", 2), true, RadioPanelPZ69KnobsMi8.UPPER_ADF_ARK9)); //UPPER ADF
            result.Add(new RadioPanelKnobMi8(0, Convert.ToInt32("100000", 2), true, RadioPanelPZ69KnobsMi8.UPPER_NOUSE3)); //UPPER DME
            result.Add(new RadioPanelKnobMi8(0, Convert.ToInt32("1000000", 2), true, RadioPanelPZ69KnobsMi8.UPPER_SPU7)); //UPPER XPDR
            result.Add(new RadioPanelKnobMi8(0, Convert.ToInt32("10000000", 2), true, RadioPanelPZ69KnobsMi8.LOWER_R863_MANUAL)); //LOWER COM1
            return result;
        }




    }


}
