using System;
using System.Collections.Generic;

namespace NonVisuals
{
    public enum RadioPanelPZ69KnobsM2000C
    {
        UPPER_VUHF = 0,      //COM1
        UPPER_UHF = 2,      //COM2
        UPPER_TACAN = 4,          //NAV1
        UPPER_VOR = 8,             //NAV2
        UPPER_NO_USE2 = 16,       //ADF
        UPPER_NO_USE3 = 32,          //DME_
        UPPER_NO_USE4 = 64,            //XPDR
        UPPER_SMALL_FREQ_WHEEL_INC = 128,
        UPPER_SMALL_FREQ_WHEEL_DEC = 256,
        UPPER_LARGE_FREQ_WHEEL_INC = 512,
        UPPER_LARGE_FREQ_WHEEL_DEC = 1024,
        UPPER_FREQ_SWITCH = 2056,
        LOWER_VUHF = 4096,   //COM1
        LOWER_UHF = 8192,   //COM2
        LOWER_TACAN = 16384,      //NAV1
        LOWER_VOR = 32768,          //NAV2
        LOWER_NO_USE2 = 65536,    //ADF
        LOWER_NO_USE3 = 131072,      //DME_
        LOWER_NO_USE4 = 262144,        //XPDR
        LOWER_SMALL_FREQ_WHEEL_INC = 8388608,
        LOWER_SMALL_FREQ_WHEEL_DEC = 524288,
        LOWER_LARGE_FREQ_WHEEL_INC = 1048576,
        LOWER_LARGE_FREQ_WHEEL_DEC = 2097152,
        LOWER_FREQ_SWITCH = 4194304
    }

    public enum CurrentM2000CRadioMode
    {
        VUHF = 0,
        UHF = 2,
        TACAN = 4,
        VOR = 8,
        NOUSE = 32
    }

    public class RadioPanelKnobM2000C : ISaitekPanelKnob
    {
        public RadioPanelKnobM2000C(int group, int mask, bool isOn, RadioPanelPZ69KnobsM2000C radioPanelPZ69Knob)
        {
            Group = group;
            Mask = mask;
            IsOn = isOn;
            RadioPanelPZ69Knob = radioPanelPZ69Knob;
        }

        public int Group { get; set; }

        public int Mask { get; set; }

        public bool IsOn { get; set; }

        public RadioPanelPZ69KnobsM2000C RadioPanelPZ69Knob { get; set; }
        
        public static HashSet<ISaitekPanelKnob> GetRadioPanelKnobs()
        {
            //true means clockwise turn
            var result = new HashSet<ISaitekPanelKnob>();
            //Group 0
            result.Add(new RadioPanelKnobM2000C(2, Convert.ToInt32("1", 2), true, RadioPanelPZ69KnobsM2000C.UPPER_SMALL_FREQ_WHEEL_INC));
            result.Add(new RadioPanelKnobM2000C(2, Convert.ToInt32("10", 2), false, RadioPanelPZ69KnobsM2000C.UPPER_SMALL_FREQ_WHEEL_DEC));
            result.Add(new RadioPanelKnobM2000C(2, Convert.ToInt32("100", 2), true, RadioPanelPZ69KnobsM2000C.UPPER_LARGE_FREQ_WHEEL_INC));
            result.Add(new RadioPanelKnobM2000C(2, Convert.ToInt32("1000", 2), false, RadioPanelPZ69KnobsM2000C.UPPER_LARGE_FREQ_WHEEL_DEC));
            result.Add(new RadioPanelKnobM2000C(2, Convert.ToInt32("10000", 2), true, RadioPanelPZ69KnobsM2000C.LOWER_SMALL_FREQ_WHEEL_INC));
            result.Add(new RadioPanelKnobM2000C(2, Convert.ToInt32("100000", 2), false, RadioPanelPZ69KnobsM2000C.LOWER_SMALL_FREQ_WHEEL_DEC));
            result.Add(new RadioPanelKnobM2000C(2, Convert.ToInt32("1000000", 2), true, RadioPanelPZ69KnobsM2000C.LOWER_LARGE_FREQ_WHEEL_INC));
            result.Add(new RadioPanelKnobM2000C(2, Convert.ToInt32("10000000", 2), false, RadioPanelPZ69KnobsM2000C.LOWER_LARGE_FREQ_WHEEL_DEC));

            //Group 1
            result.Add(new RadioPanelKnobM2000C(1, Convert.ToInt32("1", 2), true, RadioPanelPZ69KnobsM2000C.LOWER_UHF)); //LOWER COM2
            result.Add(new RadioPanelKnobM2000C(1, Convert.ToInt32("10", 2), true, RadioPanelPZ69KnobsM2000C.LOWER_TACAN)); //LOWER NAV1
            result.Add(new RadioPanelKnobM2000C(1, Convert.ToInt32("100", 2), true, RadioPanelPZ69KnobsM2000C.LOWER_VOR)); //LOWER NAV2
            result.Add(new RadioPanelKnobM2000C(1, Convert.ToInt32("1000", 2), true, RadioPanelPZ69KnobsM2000C.LOWER_NO_USE2)); //LOWER ADF
            result.Add(new RadioPanelKnobM2000C(1, Convert.ToInt32("10000", 2), true, RadioPanelPZ69KnobsM2000C.LOWER_NO_USE3)); //LOWER DME
            result.Add(new RadioPanelKnobM2000C(1, Convert.ToInt32("100000", 2), true, RadioPanelPZ69KnobsM2000C.LOWER_NO_USE4)); //LOWER XPDR
            result.Add(new RadioPanelKnobM2000C(1, Convert.ToInt32("1000000", 2), true, RadioPanelPZ69KnobsM2000C.UPPER_FREQ_SWITCH));
            result.Add(new RadioPanelKnobM2000C(1, Convert.ToInt32("10000000", 2), true, RadioPanelPZ69KnobsM2000C.LOWER_FREQ_SWITCH));

            //Group 2
            result.Add(new RadioPanelKnobM2000C(0, Convert.ToInt32("1", 2), true, RadioPanelPZ69KnobsM2000C.UPPER_VUHF)); //UPPER COM1
            result.Add(new RadioPanelKnobM2000C(0, Convert.ToInt32("10", 2), true, RadioPanelPZ69KnobsM2000C.UPPER_UHF)); //UPPER COM2
            result.Add(new RadioPanelKnobM2000C(0, Convert.ToInt32("100", 2), true, RadioPanelPZ69KnobsM2000C.UPPER_TACAN)); //UPPER NAV1
            result.Add(new RadioPanelKnobM2000C(0, Convert.ToInt32("1000", 2), true, RadioPanelPZ69KnobsM2000C.UPPER_VOR)); //UPPER NAV2
            result.Add(new RadioPanelKnobM2000C(0, Convert.ToInt32("10000", 2), true, RadioPanelPZ69KnobsM2000C.UPPER_NO_USE2)); //UPPER ADF
            result.Add(new RadioPanelKnobM2000C(0, Convert.ToInt32("100000", 2), true, RadioPanelPZ69KnobsM2000C.UPPER_NO_USE3)); //UPPER DME
            result.Add(new RadioPanelKnobM2000C(0, Convert.ToInt32("1000000", 2), true, RadioPanelPZ69KnobsM2000C.UPPER_NO_USE4)); //UPPER XPDR
            result.Add(new RadioPanelKnobM2000C(0, Convert.ToInt32("10000000", 2), true, RadioPanelPZ69KnobsM2000C.LOWER_VUHF)); //LOWER COM1
            return result;
        }




    }


}
