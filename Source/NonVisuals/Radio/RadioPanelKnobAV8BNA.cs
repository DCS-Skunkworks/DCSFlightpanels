using System;
using System.Collections.Generic;

namespace NonVisuals.Radio
{
    public enum RadioPanelPZ69KnobsAV8BNA
    {
        UPPER_COMM1 = 0,
        UPPER_COMM2 = 2,
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
        LOWER_COMM1 = 4096,
        LOWER_COMM2 = 8192,
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

    public enum CurrentAV8BNARadioMode
    {
        COMM1 = 0,
        COMM2 = 2,
        NOUSE = 4
    }

    public class RadioPanelKnobAV8BNA : ISaitekPanelKnob
    {
        public RadioPanelKnobAV8BNA(int group, int mask, bool isOn, RadioPanelPZ69KnobsAV8BNA radioPanelPZ69Knob)
        {
            Group = group;
            Mask = mask;
            IsOn = isOn;
            RadioPanelPZ69Knob = radioPanelPZ69Knob;
        }

        public int Group { get; set; }

        public int Mask { get; set; }

        public bool IsOn { get; set; }

        public RadioPanelPZ69KnobsAV8BNA RadioPanelPZ69Knob { get; set; }

        public static HashSet<ISaitekPanelKnob> GetRadioPanelKnobs()
        {
            //true means clockwise turn
            var result = new HashSet<ISaitekPanelKnob>();
            //Group 0
            result.Add(new RadioPanelKnobAV8BNA(2, Convert.ToInt32("1", 2), true, RadioPanelPZ69KnobsAV8BNA.UPPER_SMALL_FREQ_WHEEL_INC));
            result.Add(new RadioPanelKnobAV8BNA(2, Convert.ToInt32("10", 2), false, RadioPanelPZ69KnobsAV8BNA.UPPER_SMALL_FREQ_WHEEL_DEC));
            result.Add(new RadioPanelKnobAV8BNA(2, Convert.ToInt32("100", 2), true, RadioPanelPZ69KnobsAV8BNA.UPPER_LARGE_FREQ_WHEEL_INC));
            result.Add(new RadioPanelKnobAV8BNA(2, Convert.ToInt32("1000", 2), false, RadioPanelPZ69KnobsAV8BNA.UPPER_LARGE_FREQ_WHEEL_DEC));
            result.Add(new RadioPanelKnobAV8BNA(2, Convert.ToInt32("10000", 2), true, RadioPanelPZ69KnobsAV8BNA.LOWER_SMALL_FREQ_WHEEL_INC));
            result.Add(new RadioPanelKnobAV8BNA(2, Convert.ToInt32("100000", 2), false, RadioPanelPZ69KnobsAV8BNA.LOWER_SMALL_FREQ_WHEEL_DEC));
            result.Add(new RadioPanelKnobAV8BNA(2, Convert.ToInt32("1000000", 2), true, RadioPanelPZ69KnobsAV8BNA.LOWER_LARGE_FREQ_WHEEL_INC));
            result.Add(new RadioPanelKnobAV8BNA(2, Convert.ToInt32("10000000", 2), false, RadioPanelPZ69KnobsAV8BNA.LOWER_LARGE_FREQ_WHEEL_DEC));

            //Group 1
            result.Add(new RadioPanelKnobAV8BNA(1, Convert.ToInt32("1", 2), true, RadioPanelPZ69KnobsAV8BNA.LOWER_COMM2));
            result.Add(new RadioPanelKnobAV8BNA(1, Convert.ToInt32("10", 2), true, RadioPanelPZ69KnobsAV8BNA.LOWER_NAV1));
            result.Add(new RadioPanelKnobAV8BNA(1, Convert.ToInt32("100", 2), true, RadioPanelPZ69KnobsAV8BNA.LOWER_NAV2));
            result.Add(new RadioPanelKnobAV8BNA(1, Convert.ToInt32("1000", 2), true, RadioPanelPZ69KnobsAV8BNA.LOWER_ADF));
            result.Add(new RadioPanelKnobAV8BNA(1, Convert.ToInt32("10000", 2), true, RadioPanelPZ69KnobsAV8BNA.LOWER_DME));
            result.Add(new RadioPanelKnobAV8BNA(1, Convert.ToInt32("100000", 2), true, RadioPanelPZ69KnobsAV8BNA.LOWER_XPDR));
            result.Add(new RadioPanelKnobAV8BNA(1, Convert.ToInt32("1000000", 2), true, RadioPanelPZ69KnobsAV8BNA.UPPER_FREQ_SWITCH));
            result.Add(new RadioPanelKnobAV8BNA(1, Convert.ToInt32("10000000", 2), true, RadioPanelPZ69KnobsAV8BNA.LOWER_FREQ_SWITCH));

            //Group 2
            result.Add(new RadioPanelKnobAV8BNA(0, Convert.ToInt32("1", 2), true, RadioPanelPZ69KnobsAV8BNA.UPPER_COMM1)); //UPPER COM 1
            result.Add(new RadioPanelKnobAV8BNA(0, Convert.ToInt32("10", 2), true, RadioPanelPZ69KnobsAV8BNA.UPPER_COMM2)); //UPPER COM 2
            result.Add(new RadioPanelKnobAV8BNA(0, Convert.ToInt32("100", 2), true, RadioPanelPZ69KnobsAV8BNA.UPPER_NAV1)); //UPPER NAV 1
            result.Add(new RadioPanelKnobAV8BNA(0, Convert.ToInt32("1000", 2), true, RadioPanelPZ69KnobsAV8BNA.UPPER_NAV2)); //UPPER NAV 2
            result.Add(new RadioPanelKnobAV8BNA(0, Convert.ToInt32("10000", 2), true, RadioPanelPZ69KnobsAV8BNA.UPPER_ADF)); //UPPER ADF
            result.Add(new RadioPanelKnobAV8BNA(0, Convert.ToInt32("100000", 2), true, RadioPanelPZ69KnobsAV8BNA.UPPER_DME)); //UPPER DME
            result.Add(new RadioPanelKnobAV8BNA(0, Convert.ToInt32("1000000", 2), true, RadioPanelPZ69KnobsAV8BNA.UPPER_XPDR)); //UPPER XPDR
            result.Add(new RadioPanelKnobAV8BNA(0, Convert.ToInt32("10000000", 2), true, RadioPanelPZ69KnobsAV8BNA.LOWER_COMM1)); //LOWER COM 1 
            return result;
        }




    }
}
