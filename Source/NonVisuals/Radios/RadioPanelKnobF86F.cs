using System;
using System.Collections.Generic;

namespace NonVisuals.Radios
{
    public enum RadioPanelPZ69KnobsF86F
    {
        UPPER_ARC27_PRESET = 0,      //COM1
        UPPER_ARC27_VOL = 2,      //COM2
        UPPER_ARN6 = 4,          //NAV1
        UPPER_ARN6_MODES = 8,             //NAV2
        UPPER_ADF_APX6 = 16,       //ADF
        UPPER_NO_USE1 = 32,          //DME_
        UPPER_NO_USE2 = 64,            //XPDR
        UPPER_SMALL_FREQ_WHEEL_INC = 128,
        UPPER_SMALL_FREQ_WHEEL_DEC = 256,
        UPPER_LARGE_FREQ_WHEEL_INC = 512,
        UPPER_LARGE_FREQ_WHEEL_DEC = 1024,
        UPPER_FREQ_SWITCH = 2056,
        LOWER_ARC27_PRESET = 4096,   //COM1
        LOWER_ARC27_VOL = 8192,   //COM2
        LOWER_ARN6 = 16384,      //NAV1
        LOWER_ARN6_MODES = 32768,          //NAV2
        LOWER_ADF_APX6 = 65536,    //ADF
        LOWER_NO_USE1 = 131072,      //DME_
        LOWER_NO_USE2 = 262144,        //XPDR
        LOWER_SMALL_FREQ_WHEEL_INC = 8388608,
        LOWER_SMALL_FREQ_WHEEL_DEC = 524288,
        LOWER_LARGE_FREQ_WHEEL_INC = 1048576,
        LOWER_LARGE_FREQ_WHEEL_DEC = 2097152,
        LOWER_FREQ_SWITCH = 4194304
    }

    public enum CurrentF86FRadioMode
    {
        ARC27_PRESET = 0,
        ARC27_VOL = 2,
        ARN6 = 4,
        ARN6_MODES = 8,
        ADF_APX6 = 16,
        NOUSE = 32
    }

    public class RadioPanelKnobF86F : ISaitekPanelKnob
    {
        public RadioPanelKnobF86F(int group, int mask, bool isOn, RadioPanelPZ69KnobsF86F radioPanelPZ69Knob)
        {
            Group = group;
            Mask = mask;
            IsOn = isOn;
            RadioPanelPZ69Knob = radioPanelPZ69Knob;
        }

        public int Group { get; set; }

        public int Mask { get; set; }

        public bool IsOn { get; set; }

        public RadioPanelPZ69KnobsF86F RadioPanelPZ69Knob { get; set; }

        public static HashSet<ISaitekPanelKnob> GetRadioPanelKnobs()
        {
            //true means clockwise turn
            var result = new HashSet<ISaitekPanelKnob>();
            //Group 0
            result.Add(new RadioPanelKnobF86F(2, Convert.ToInt32("1", 2), true, RadioPanelPZ69KnobsF86F.UPPER_SMALL_FREQ_WHEEL_INC));
            result.Add(new RadioPanelKnobF86F(2, Convert.ToInt32("10", 2), false, RadioPanelPZ69KnobsF86F.UPPER_SMALL_FREQ_WHEEL_DEC));
            result.Add(new RadioPanelKnobF86F(2, Convert.ToInt32("100", 2), true, RadioPanelPZ69KnobsF86F.UPPER_LARGE_FREQ_WHEEL_INC));
            result.Add(new RadioPanelKnobF86F(2, Convert.ToInt32("1000", 2), false, RadioPanelPZ69KnobsF86F.UPPER_LARGE_FREQ_WHEEL_DEC));
            result.Add(new RadioPanelKnobF86F(2, Convert.ToInt32("10000", 2), true, RadioPanelPZ69KnobsF86F.LOWER_SMALL_FREQ_WHEEL_INC));
            result.Add(new RadioPanelKnobF86F(2, Convert.ToInt32("100000", 2), false, RadioPanelPZ69KnobsF86F.LOWER_SMALL_FREQ_WHEEL_DEC));
            result.Add(new RadioPanelKnobF86F(2, Convert.ToInt32("1000000", 2), true, RadioPanelPZ69KnobsF86F.LOWER_LARGE_FREQ_WHEEL_INC));
            result.Add(new RadioPanelKnobF86F(2, Convert.ToInt32("10000000", 2), false, RadioPanelPZ69KnobsF86F.LOWER_LARGE_FREQ_WHEEL_DEC));

            //Group 1
            result.Add(new RadioPanelKnobF86F(1, Convert.ToInt32("1", 2), true, RadioPanelPZ69KnobsF86F.LOWER_ARC27_VOL)); //LOWER COM2
            result.Add(new RadioPanelKnobF86F(1, Convert.ToInt32("10", 2), true, RadioPanelPZ69KnobsF86F.LOWER_ARN6)); //LOWER NAV1
            result.Add(new RadioPanelKnobF86F(1, Convert.ToInt32("100", 2), true, RadioPanelPZ69KnobsF86F.LOWER_ARN6_MODES)); //LOWER NAV2
            result.Add(new RadioPanelKnobF86F(1, Convert.ToInt32("1000", 2), true, RadioPanelPZ69KnobsF86F.LOWER_ADF_APX6)); //LOWER ADF
            result.Add(new RadioPanelKnobF86F(1, Convert.ToInt32("10000", 2), true, RadioPanelPZ69KnobsF86F.LOWER_NO_USE1)); //LOWER DME
            result.Add(new RadioPanelKnobF86F(1, Convert.ToInt32("100000", 2), true, RadioPanelPZ69KnobsF86F.LOWER_NO_USE2)); //LOWER XPDR
            result.Add(new RadioPanelKnobF86F(1, Convert.ToInt32("1000000", 2), true, RadioPanelPZ69KnobsF86F.UPPER_FREQ_SWITCH));
            result.Add(new RadioPanelKnobF86F(1, Convert.ToInt32("10000000", 2), true, RadioPanelPZ69KnobsF86F.LOWER_FREQ_SWITCH));

            //Group 2
            result.Add(new RadioPanelKnobF86F(0, Convert.ToInt32("1", 2), true, RadioPanelPZ69KnobsF86F.UPPER_ARC27_PRESET)); //UPPER COM1
            result.Add(new RadioPanelKnobF86F(0, Convert.ToInt32("10", 2), true, RadioPanelPZ69KnobsF86F.UPPER_ARC27_VOL)); //UPPER COM2
            result.Add(new RadioPanelKnobF86F(0, Convert.ToInt32("100", 2), true, RadioPanelPZ69KnobsF86F.UPPER_ARN6)); //UPPER NAV1
            result.Add(new RadioPanelKnobF86F(0, Convert.ToInt32("1000", 2), true, RadioPanelPZ69KnobsF86F.UPPER_ARN6_MODES)); //UPPER NAV2
            result.Add(new RadioPanelKnobF86F(0, Convert.ToInt32("10000", 2), true, RadioPanelPZ69KnobsF86F.UPPER_ADF_APX6)); //UPPER ADF
            result.Add(new RadioPanelKnobF86F(0, Convert.ToInt32("100000", 2), true, RadioPanelPZ69KnobsF86F.UPPER_NO_USE1)); //UPPER DME
            result.Add(new RadioPanelKnobF86F(0, Convert.ToInt32("1000000", 2), true, RadioPanelPZ69KnobsF86F.UPPER_NO_USE2)); //UPPER XPDR
            result.Add(new RadioPanelKnobF86F(0, Convert.ToInt32("10000000", 2), true, RadioPanelPZ69KnobsF86F.LOWER_ARC27_PRESET)); //LOWER COM1
            return result;
        }




    }


}
