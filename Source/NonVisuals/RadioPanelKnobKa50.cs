using System;
using System.Collections.Generic;

namespace NonVisuals
{
    public enum RadioPanelPZ69KnobsKa50
    {
        UPPER_VHF1_R828 = 0,   //COM1
        UPPER_VHF2_R800L1 = 2,  //COM2
        UPPER_ABRIS = 4, //NAV1
        UPPER_DATALINK = 8, //NAV2
        UPPER_ADF_ARK22 = 16, //ADF
        UPPER_NO_USE3 = 32, //DME_
        UPPER_NO_USE4 = 64, //XPDR
        UPPER_SMALL_FREQ_WHEEL_INC = 128,
        UPPER_SMALL_FREQ_WHEEL_DEC = 256,
        UPPER_LARGE_FREQ_WHEEL_INC = 512,
        UPPER_LARGE_FREQ_WHEEL_DEC = 1024,
        UPPER_FREQ_SWITCH = 2056,
        LOWER_VHF1_R828 = 4096,
        LOWER_VHF2_R800L1 = 8192,
        LOWER_ABRIS = 16384,
        LOWER_DATALINK = 32768,
        LOWER_ADF_ARK22 = 65536,
        LOWER_NO_USE3 = 131072,
        LOWER_NO_USE4 = 262144,
        LOWER_SMALL_FREQ_WHEEL_INC = 8388608,
        LOWER_SMALL_FREQ_WHEEL_DEC = 524288,
        LOWER_LARGE_FREQ_WHEEL_INC = 1048576,
        LOWER_LARGE_FREQ_WHEEL_DEC = 2097152,
        LOWER_FREQ_SWITCH = 4194304
    }

    public enum CurrentKa50RadioMode
    {
        VHF1_R828 = 0,
        VHF2_R800L1 = 2,
        ADF_ARK22 = 4,
        ABRIS = 8,
        DATALINK = 16,
        NOUSE = 32
    }

    public class RadioPanelKnobKa50
    {
        public RadioPanelKnobKa50(int group, int mask, bool isOn, RadioPanelPZ69KnobsKa50 radioPanelPZ69Knob)
        {
            Group = group;
            Mask = mask;
            IsOn = isOn;
            RadioPanelPZ69Knob = radioPanelPZ69Knob;
        }

        public int Group { get; set; }

        public int Mask { get; set; }

        public bool IsOn { get; set; }

        public RadioPanelPZ69KnobsKa50 RadioPanelPZ69Knob { get; set; }

        public string ExportString()
        {
            return "RadioPanelKnob{" + Enum.GetName(typeof(RadioPanelPZ69KnobsKa50), RadioPanelPZ69Knob) + "}";
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
            RadioPanelPZ69Knob = (RadioPanelPZ69KnobsKa50)Enum.Parse(typeof(RadioPanelPZ69KnobsKa50), dataString.Trim());
        }

        public static HashSet<RadioPanelKnobKa50> GetRadioPanelKnobs()
        {
            //true means clockwise turn
            var result = new HashSet<RadioPanelKnobKa50>();
            //Group 0
            result.Add(new RadioPanelKnobKa50(2, Convert.ToInt32("1", 2), true, RadioPanelPZ69KnobsKa50.UPPER_SMALL_FREQ_WHEEL_INC));
            result.Add(new RadioPanelKnobKa50(2, Convert.ToInt32("10", 2), false, RadioPanelPZ69KnobsKa50.UPPER_SMALL_FREQ_WHEEL_DEC));
            result.Add(new RadioPanelKnobKa50(2, Convert.ToInt32("100", 2), true, RadioPanelPZ69KnobsKa50.UPPER_LARGE_FREQ_WHEEL_INC));
            result.Add(new RadioPanelKnobKa50(2, Convert.ToInt32("1000", 2), false, RadioPanelPZ69KnobsKa50.UPPER_LARGE_FREQ_WHEEL_DEC));
            result.Add(new RadioPanelKnobKa50(2, Convert.ToInt32("10000", 2), true, RadioPanelPZ69KnobsKa50.LOWER_SMALL_FREQ_WHEEL_INC));
            result.Add(new RadioPanelKnobKa50(2, Convert.ToInt32("100000", 2), false, RadioPanelPZ69KnobsKa50.LOWER_SMALL_FREQ_WHEEL_DEC));
            result.Add(new RadioPanelKnobKa50(2, Convert.ToInt32("1000000", 2), true, RadioPanelPZ69KnobsKa50.LOWER_LARGE_FREQ_WHEEL_INC));
            result.Add(new RadioPanelKnobKa50(2, Convert.ToInt32("10000000", 2), false, RadioPanelPZ69KnobsKa50.LOWER_LARGE_FREQ_WHEEL_DEC));

            //Group 1
            result.Add(new RadioPanelKnobKa50(1, Convert.ToInt32("1", 2), true, RadioPanelPZ69KnobsKa50.LOWER_VHF2_R800L1)); //LOWER COM2
            result.Add(new RadioPanelKnobKa50(1, Convert.ToInt32("10", 2), true, RadioPanelPZ69KnobsKa50.LOWER_ABRIS)); //LOWER NAV1
            result.Add(new RadioPanelKnobKa50(1, Convert.ToInt32("100", 2), true, RadioPanelPZ69KnobsKa50.LOWER_DATALINK)); //LOWER NAV2
            result.Add(new RadioPanelKnobKa50(1, Convert.ToInt32("1000", 2), true, RadioPanelPZ69KnobsKa50.LOWER_ADF_ARK22)); //LOWER ADF
            result.Add(new RadioPanelKnobKa50(1, Convert.ToInt32("10000", 2), true, RadioPanelPZ69KnobsKa50.LOWER_NO_USE3)); //LOWER DME
            result.Add(new RadioPanelKnobKa50(1, Convert.ToInt32("100000", 2), true, RadioPanelPZ69KnobsKa50.LOWER_NO_USE4)); //LOWER XPDR
            result.Add(new RadioPanelKnobKa50(1, Convert.ToInt32("1000000", 2), true, RadioPanelPZ69KnobsKa50.UPPER_FREQ_SWITCH));
            result.Add(new RadioPanelKnobKa50(1, Convert.ToInt32("10000000", 2), true, RadioPanelPZ69KnobsKa50.LOWER_FREQ_SWITCH));

            //Group 2
            result.Add(new RadioPanelKnobKa50(0, Convert.ToInt32("1", 2), true, RadioPanelPZ69KnobsKa50.UPPER_VHF1_R828)); //UPPER COM1
            result.Add(new RadioPanelKnobKa50(0, Convert.ToInt32("10", 2), true, RadioPanelPZ69KnobsKa50.UPPER_VHF2_R800L1)); //UPPER COM2
            result.Add(new RadioPanelKnobKa50(0, Convert.ToInt32("100", 2), true, RadioPanelPZ69KnobsKa50.UPPER_ABRIS)); //UPPER NAV1
            result.Add(new RadioPanelKnobKa50(0, Convert.ToInt32("1000", 2), true, RadioPanelPZ69KnobsKa50.UPPER_DATALINK)); //UPPER NAV2
            result.Add(new RadioPanelKnobKa50(0, Convert.ToInt32("10000", 2), true, RadioPanelPZ69KnobsKa50.UPPER_ADF_ARK22)); //UPPER ADF
            result.Add(new RadioPanelKnobKa50(0, Convert.ToInt32("100000", 2), true, RadioPanelPZ69KnobsKa50.UPPER_NO_USE3)); //UPPER DME
            result.Add(new RadioPanelKnobKa50(0, Convert.ToInt32("1000000", 2), true, RadioPanelPZ69KnobsKa50.UPPER_NO_USE4)); //UPPER XPDR
            result.Add(new RadioPanelKnobKa50(0, Convert.ToInt32("10000000", 2), true, RadioPanelPZ69KnobsKa50.LOWER_VHF1_R828)); //LOWER COM1
            return result;
        }




    }


}
