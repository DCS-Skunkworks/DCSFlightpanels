using System;
using System.Collections.Generic;

namespace NonVisuals
{
    public enum RadioPanelPZ69KnobsBf109
    {
        UPPER_FUG16ZY = 0,      //COM1
        UPPER_IFF = 2,      //COM2
        UPPER_HOMING = 4,          //NAV1
        UPPER_NOUSE1 = 8,             //NAV2
        UPPER_NOUSE2 = 16,       //ADF
        UPPER_NOUSE3 = 32,          //DME_
        UPPER_NOUSE4 = 64,            //XPDR
        UPPER_SMALL_FREQ_WHEEL_INC = 128,
        UPPER_SMALL_FREQ_WHEEL_DEC = 256,
        UPPER_LARGE_FREQ_WHEEL_INC = 512,
        UPPER_LARGE_FREQ_WHEEL_DEC = 1024,
        UPPER_FREQ_SWITCH = 2056,
        LOWER_FUG16ZY = 4096,   //COM1
        LOWER_IFF = 8192,   //COM2
        LOWER_HOMING = 16384,      //NAV1
        LOWER_NOUSE1 = 32768,          //NAV2
        LOWER_NOUSE2 = 65536,    //ADF
        LOWER_NOUSE3 = 131072,      //DME_
        LOWER_NOUSE4 = 262144,        //XPDR
        LOWER_SMALL_FREQ_WHEEL_INC = 8388608,
        LOWER_SMALL_FREQ_WHEEL_DEC = 524288,
        LOWER_LARGE_FREQ_WHEEL_INC = 1048576,
        LOWER_LARGE_FREQ_WHEEL_DEC = 2097152,
        LOWER_FREQ_SWITCH = 4194304
    }

    public enum CurrentBf109RadioMode
    {
        /*
         *  COM1 Large Freq Sel
         *  COM1 Small Fine Tune
         *  Freq. Selector
         *  Fine Tuning
         *  
         *  COM2 Large IFF Control Switch
         *  COM2 Small Volume
         *  COM2 ACT/STBY IFF Test Button
         *  IFF Control Switch
         *  IFF Test Button
         *  Volume
         *  
         *  NAV1
         *  Homing Switch         
         */
        FUG16ZY = 0,
        IFF = 2,
        HOMING = 4,
        NOUSE = 64
    }

    public class RadioPanelKnobBf109
    {
        public RadioPanelKnobBf109(int group, int mask, bool isOn, RadioPanelPZ69KnobsBf109 radioPanelPZ69Knob)
        {
            Group = group;
            Mask = mask;
            IsOn = isOn;
            RadioPanelPZ69Knob = radioPanelPZ69Knob;
        }

        public int Group { get; set; }

        public int Mask { get; set; }

        public bool IsOn { get; set; }

        public RadioPanelPZ69KnobsBf109 RadioPanelPZ69Knob { get; set; }

        public string ExportString()
        {
            return "RadioPanelKnob{" + Enum.GetName(typeof(RadioPanelPZ69KnobsBf109), RadioPanelPZ69Knob) + "}";
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
            RadioPanelPZ69Knob = (RadioPanelPZ69KnobsBf109)Enum.Parse(typeof(RadioPanelPZ69KnobsBf109), dataString.Trim());
        }

        public static HashSet<RadioPanelKnobBf109> GetRadioPanelKnobs()
        {
            //true means clockwise turn
            var result = new HashSet<RadioPanelKnobBf109>();
            //Group 0
            result.Add(new RadioPanelKnobBf109(2, Convert.ToInt32("1", 2), true, RadioPanelPZ69KnobsBf109.UPPER_SMALL_FREQ_WHEEL_INC));
            result.Add(new RadioPanelKnobBf109(2, Convert.ToInt32("10", 2), false, RadioPanelPZ69KnobsBf109.UPPER_SMALL_FREQ_WHEEL_DEC));
            result.Add(new RadioPanelKnobBf109(2, Convert.ToInt32("100", 2), true, RadioPanelPZ69KnobsBf109.UPPER_LARGE_FREQ_WHEEL_INC));
            result.Add(new RadioPanelKnobBf109(2, Convert.ToInt32("1000", 2), false, RadioPanelPZ69KnobsBf109.UPPER_LARGE_FREQ_WHEEL_DEC));
            result.Add(new RadioPanelKnobBf109(2, Convert.ToInt32("10000", 2), true, RadioPanelPZ69KnobsBf109.LOWER_SMALL_FREQ_WHEEL_INC));
            result.Add(new RadioPanelKnobBf109(2, Convert.ToInt32("100000", 2), false, RadioPanelPZ69KnobsBf109.LOWER_SMALL_FREQ_WHEEL_DEC));
            result.Add(new RadioPanelKnobBf109(2, Convert.ToInt32("1000000", 2), true, RadioPanelPZ69KnobsBf109.LOWER_LARGE_FREQ_WHEEL_INC));
            result.Add(new RadioPanelKnobBf109(2, Convert.ToInt32("10000000", 2), false, RadioPanelPZ69KnobsBf109.LOWER_LARGE_FREQ_WHEEL_DEC));

            //Group 1
            result.Add(new RadioPanelKnobBf109(1, Convert.ToInt32("1", 2), true, RadioPanelPZ69KnobsBf109.LOWER_IFF)); //LOWER COM2
            result.Add(new RadioPanelKnobBf109(1, Convert.ToInt32("10", 2), true, RadioPanelPZ69KnobsBf109.LOWER_HOMING)); //LOWER NAV1
            result.Add(new RadioPanelKnobBf109(1, Convert.ToInt32("100", 2), true, RadioPanelPZ69KnobsBf109.LOWER_NOUSE1)); //LOWER NAV2
            result.Add(new RadioPanelKnobBf109(1, Convert.ToInt32("1000", 2), true, RadioPanelPZ69KnobsBf109.LOWER_NOUSE2)); //LOWER ADF
            result.Add(new RadioPanelKnobBf109(1, Convert.ToInt32("10000", 2), true, RadioPanelPZ69KnobsBf109.LOWER_NOUSE3)); //LOWER DME
            result.Add(new RadioPanelKnobBf109(1, Convert.ToInt32("100000", 2), true, RadioPanelPZ69KnobsBf109.LOWER_NOUSE4)); //LOWER XPDR
            result.Add(new RadioPanelKnobBf109(1, Convert.ToInt32("1000000", 2), true, RadioPanelPZ69KnobsBf109.UPPER_FREQ_SWITCH));
            result.Add(new RadioPanelKnobBf109(1, Convert.ToInt32("10000000", 2), true, RadioPanelPZ69KnobsBf109.LOWER_FREQ_SWITCH));

            //Group 2
            result.Add(new RadioPanelKnobBf109(0, Convert.ToInt32("1", 2), true, RadioPanelPZ69KnobsBf109.UPPER_FUG16ZY)); //UPPER COM1
            result.Add(new RadioPanelKnobBf109(0, Convert.ToInt32("10", 2), true, RadioPanelPZ69KnobsBf109.UPPER_IFF)); //UPPER COM2
            result.Add(new RadioPanelKnobBf109(0, Convert.ToInt32("100", 2), true, RadioPanelPZ69KnobsBf109.UPPER_HOMING)); //UPPER NAV1
            result.Add(new RadioPanelKnobBf109(0, Convert.ToInt32("1000", 2), true, RadioPanelPZ69KnobsBf109.UPPER_NOUSE1)); //UPPER NAV2
            result.Add(new RadioPanelKnobBf109(0, Convert.ToInt32("10000", 2), true, RadioPanelPZ69KnobsBf109.UPPER_NOUSE2)); //UPPER ADF
            result.Add(new RadioPanelKnobBf109(0, Convert.ToInt32("100000", 2), true, RadioPanelPZ69KnobsBf109.UPPER_NOUSE3)); //UPPER DME
            result.Add(new RadioPanelKnobBf109(0, Convert.ToInt32("1000000", 2), true, RadioPanelPZ69KnobsBf109.UPPER_NOUSE4)); //UPPER XPDR
            result.Add(new RadioPanelKnobBf109(0, Convert.ToInt32("10000000", 2), true, RadioPanelPZ69KnobsBf109.LOWER_FUG16ZY)); //LOWER COM1
            return result;
        }




    }


}
