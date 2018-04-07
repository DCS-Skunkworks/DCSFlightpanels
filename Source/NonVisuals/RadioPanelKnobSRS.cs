using System;
using System.Collections.Generic;

namespace NonVisuals
{
    public enum RadioPanelPZ69KnobsSRS
    {
        UPPER_COM1 = 0,   //COM1
        UPPER_COM2 = 2,  //COM2
        UPPER_NAV1 = 4, //NAV1
        UPPER_NAV2 = 8, //NAV2
        UPPER_ADF = 16, //ADF
        UPPER_DME = 32, //DME_
        UPPER_XPDR = 64, //XPDR
        UPPER_SMALL_FREQ_WHEEL_INC = 128,
        UPPER_SMALL_FREQ_WHEEL_DEC = 256,
        UPPER_LARGE_FREQ_WHEEL_INC = 512,
        UPPER_LARGE_FREQ_WHEEL_DEC = 1024,
        UPPER_FREQ_SWITCH = 2056,
        LOWER_COM1 = 4096,
        LOWER_COM2 = 8192,
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

    public enum CurrentSRSRadioMode
    {
        COM1 = 0,
        COM2 = 2,
        NAV1 = 4,
        NAV2 = 8,
        ADF = 16,
        DME = 32,
        XPDR = 64
    }

    public class RadioPanelKnobSRS
    {
        public RadioPanelKnobSRS(int group, int mask, bool isOn, RadioPanelPZ69KnobsSRS radioPanelPZ69Knob)
        {
            Group = group;
            Mask = mask;
            IsOn = isOn;
            RadioPanelPZ69Knob = radioPanelPZ69Knob;
        }

        public int Group { get; set; }

        public int Mask { get; set; }

        public bool IsOn { get; set; }

        public RadioPanelPZ69KnobsSRS RadioPanelPZ69Knob { get; set; }

        public string ExportString()
        {
            return "RadioPanelKnob{" + Enum.GetName(typeof(RadioPanelPZ69KnobsSRS), RadioPanelPZ69Knob) + "}";
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
            RadioPanelPZ69Knob = (RadioPanelPZ69KnobsSRS)Enum.Parse(typeof(RadioPanelPZ69KnobsSRS), dataString.Trim());
        }

        public static HashSet<RadioPanelKnobSRS> GetRadioPanelKnobs()
        {
            //true means clockwise turn
            var result = new HashSet<RadioPanelKnobSRS>();
            //Group 0
            result.Add(new RadioPanelKnobSRS(2, Convert.ToInt32("1", 2), true, RadioPanelPZ69KnobsSRS.UPPER_SMALL_FREQ_WHEEL_INC));
            result.Add(new RadioPanelKnobSRS(2, Convert.ToInt32("10", 2), false, RadioPanelPZ69KnobsSRS.UPPER_SMALL_FREQ_WHEEL_DEC));
            result.Add(new RadioPanelKnobSRS(2, Convert.ToInt32("100", 2), true, RadioPanelPZ69KnobsSRS.UPPER_LARGE_FREQ_WHEEL_INC));
            result.Add(new RadioPanelKnobSRS(2, Convert.ToInt32("1000", 2), false, RadioPanelPZ69KnobsSRS.UPPER_LARGE_FREQ_WHEEL_DEC));
            result.Add(new RadioPanelKnobSRS(2, Convert.ToInt32("10000", 2), true, RadioPanelPZ69KnobsSRS.LOWER_SMALL_FREQ_WHEEL_INC));
            result.Add(new RadioPanelKnobSRS(2, Convert.ToInt32("100000", 2), false, RadioPanelPZ69KnobsSRS.LOWER_SMALL_FREQ_WHEEL_DEC));
            result.Add(new RadioPanelKnobSRS(2, Convert.ToInt32("1000000", 2), true, RadioPanelPZ69KnobsSRS.LOWER_LARGE_FREQ_WHEEL_INC));
            result.Add(new RadioPanelKnobSRS(2, Convert.ToInt32("10000000", 2), false, RadioPanelPZ69KnobsSRS.LOWER_LARGE_FREQ_WHEEL_DEC));

            //Group 1
            result.Add(new RadioPanelKnobSRS(1, Convert.ToInt32("1", 2), true, RadioPanelPZ69KnobsSRS.LOWER_COM2)); //LOWER COM2
            result.Add(new RadioPanelKnobSRS(1, Convert.ToInt32("10", 2), true, RadioPanelPZ69KnobsSRS.LOWER_NAV1)); //LOWER NAV1
            result.Add(new RadioPanelKnobSRS(1, Convert.ToInt32("100", 2), true, RadioPanelPZ69KnobsSRS.LOWER_NAV2)); //LOWER NAV2
            result.Add(new RadioPanelKnobSRS(1, Convert.ToInt32("1000", 2), true, RadioPanelPZ69KnobsSRS.LOWER_ADF)); //LOWER ADF
            result.Add(new RadioPanelKnobSRS(1, Convert.ToInt32("10000", 2), true, RadioPanelPZ69KnobsSRS.LOWER_DME)); //LOWER DME
            result.Add(new RadioPanelKnobSRS(1, Convert.ToInt32("100000", 2), true, RadioPanelPZ69KnobsSRS.LOWER_XPDR)); //LOWER XPDR
            result.Add(new RadioPanelKnobSRS(1, Convert.ToInt32("1000000", 2), true, RadioPanelPZ69KnobsSRS.UPPER_FREQ_SWITCH));
            result.Add(new RadioPanelKnobSRS(1, Convert.ToInt32("10000000", 2), true, RadioPanelPZ69KnobsSRS.LOWER_FREQ_SWITCH));

            //Group 2
            result.Add(new RadioPanelKnobSRS(0, Convert.ToInt32("1", 2), true, RadioPanelPZ69KnobsSRS.UPPER_COM1)); //UPPER COM1
            result.Add(new RadioPanelKnobSRS(0, Convert.ToInt32("10", 2), true, RadioPanelPZ69KnobsSRS.UPPER_COM2)); //UPPER COM2
            result.Add(new RadioPanelKnobSRS(0, Convert.ToInt32("100", 2), true, RadioPanelPZ69KnobsSRS.UPPER_NAV1)); //UPPER NAV1
            result.Add(new RadioPanelKnobSRS(0, Convert.ToInt32("1000", 2), true, RadioPanelPZ69KnobsSRS.UPPER_NAV2)); //UPPER NAV2
            result.Add(new RadioPanelKnobSRS(0, Convert.ToInt32("10000", 2), true, RadioPanelPZ69KnobsSRS.UPPER_ADF)); //UPPER ADF
            result.Add(new RadioPanelKnobSRS(0, Convert.ToInt32("100000", 2), true, RadioPanelPZ69KnobsSRS.UPPER_DME)); //UPPER DME
            result.Add(new RadioPanelKnobSRS(0, Convert.ToInt32("1000000", 2), true, RadioPanelPZ69KnobsSRS.UPPER_XPDR)); //UPPER XPDR
            result.Add(new RadioPanelKnobSRS(0, Convert.ToInt32("10000000", 2), true, RadioPanelPZ69KnobsSRS.LOWER_COM1)); //LOWER COM1
            return result;
        }




    }


}
