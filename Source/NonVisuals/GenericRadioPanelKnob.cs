using System;
using System.Collections.Generic;
using NonVisuals.Radio;

namespace NonVisuals
{
    public enum GenericRadioPanelKnobs
    {
        UPPER_COM1 = 0,
        UPPER_COM2 = 2,
        UPPER_NAV1 = 4,
        UPPER_NAV2 = 8,
        UPPER_ADF = 16,
        UPPER_DME = 32,
        UPPER_XPDR = 64,
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

    public enum GenericRadioKnobType
    {
        COM1 = 0,
        COM2 = 2,
        NAV1 = 4,
        NAV2 = 8,
        ADF = 16,
        DME = 32,
        XPDR = 64,
        NOUSE = 128,
        SMALL_FREQ_WHEEL_INC = 256,
        SMALL_FREQ_WHEEL_DEC = 512,
        LARGE_FREQ_WHEEL_INC = 1024,
        LARGE_FREQ_WHEEL_DEC = 2056,
        FREQ_SWITCH = 4096
    }



    public class GenericRadioPanelKnob
    {
        public GenericRadioPanelKnob(int group, int mask, bool isOn, GenericRadioPanelKnobs genericRadioPanelKnob)
        {
            Group = group;
            Mask = mask;
            IsOn = isOn;
            RadioPanelPZ69Knob = genericRadioPanelKnob;
        }

        public int Group { get; set; }

        public int Mask { get; set; }

        public bool IsOn { get; set; }

        public GenericRadioPanelKnobs RadioPanelPZ69Knob { get; set; }
        
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
            result.Add(new RadioPanelKnobMi8(1, Convert.ToInt32("10000", 2), true, RadioPanelPZ69KnobsMi8.LOWER_ARK_UD)); //LOWER DME
            result.Add(new RadioPanelKnobMi8(1, Convert.ToInt32("100000", 2), true, RadioPanelPZ69KnobsMi8.LOWER_SPU7)); //LOWER XPDR
            result.Add(new RadioPanelKnobMi8(1, Convert.ToInt32("1000000", 2), true, RadioPanelPZ69KnobsMi8.UPPER_FREQ_SWITCH));
            result.Add(new RadioPanelKnobMi8(1, Convert.ToInt32("10000000", 2), true, RadioPanelPZ69KnobsMi8.LOWER_FREQ_SWITCH));

            //Group 2
            result.Add(new RadioPanelKnobMi8(0, Convert.ToInt32("1", 2), true, RadioPanelPZ69KnobsMi8.UPPER_R863_MANUAL)); //UPPER COM1
            result.Add(new RadioPanelKnobMi8(0, Convert.ToInt32("10", 2), true, RadioPanelPZ69KnobsMi8.UPPER_R863_PRESET)); //UPPER COM2
            result.Add(new RadioPanelKnobMi8(0, Convert.ToInt32("100", 2), true, RadioPanelPZ69KnobsMi8.UPPER_YADRO1A)); //UPPER NAV1
            result.Add(new RadioPanelKnobMi8(0, Convert.ToInt32("1000", 2), true, RadioPanelPZ69KnobsMi8.UPPER_R828)); //UPPER NAV2
            result.Add(new RadioPanelKnobMi8(0, Convert.ToInt32("10000", 2), true, RadioPanelPZ69KnobsMi8.UPPER_ADF_ARK9)); //UPPER ADF
            result.Add(new RadioPanelKnobMi8(0, Convert.ToInt32("100000", 2), true, RadioPanelPZ69KnobsMi8.UPPER_ARK_UD)); //UPPER DME
            result.Add(new RadioPanelKnobMi8(0, Convert.ToInt32("1000000", 2), true, RadioPanelPZ69KnobsMi8.UPPER_SPU7)); //UPPER XPDR
            result.Add(new RadioPanelKnobMi8(0, Convert.ToInt32("10000000", 2), true, RadioPanelPZ69KnobsMi8.LOWER_R863_MANUAL)); //LOWER COM1
            return result;
        }
    }
}
