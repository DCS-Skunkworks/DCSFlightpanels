using System;
using System.Collections.Generic;
using NonVisuals.Interfaces;

namespace NonVisuals.Radios
{
    public enum RadioPanelPZ69KnobsP51D
    {
        UPPER_VHF = 0,   //COM1
        UPPER_NO_USE0 = 2,  //COM2
        UPPER_NO_USE1 = 4, //NAV1
        UPPER_NO_USE2 = 8, //NAV2
        UPPER_NO_USE3 = 16, //ADF
        UPPER_NO_USE4 = 32, //DME_
        UPPER_NO_USE5 = 64, //XPDR
        UPPER_SMALL_FREQ_WHEEL_INC = 128,
        UPPER_SMALL_FREQ_WHEEL_DEC = 256,
        UPPER_LARGE_FREQ_WHEEL_INC = 512,
        UPPER_LARGE_FREQ_WHEEL_DEC = 1024,
        UPPER_FREQ_SWITCH = 2056,
        LOWER_VHF = 4096,
        LOWER_NO_USE0 = 8192,
        LOWER_NO_USE1 = 16384,
        LOWER_NO_USE2 = 32768,
        LOWER_NO_USE3 = 65536,
        LOWER_NO_USE4 = 131072,
        LOWER_NO_USE5 = 262144,
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

    public class RadioPanelKnobP51D : ISaitekPanelKnob
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
        
        public static HashSet<ISaitekPanelKnob> GetRadioPanelKnobs()
        {
            //true means clockwise turn
            var result = new HashSet<ISaitekPanelKnob>();
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
            result.Add(new RadioPanelKnobP51D(1, Convert.ToInt32("1", 2), true, RadioPanelPZ69KnobsP51D.LOWER_NO_USE0)); //LOWER COM2
            result.Add(new RadioPanelKnobP51D(1, Convert.ToInt32("10", 2), true, RadioPanelPZ69KnobsP51D.LOWER_NO_USE1)); //LOWER NAV1
            result.Add(new RadioPanelKnobP51D(1, Convert.ToInt32("100", 2), true, RadioPanelPZ69KnobsP51D.LOWER_NO_USE2)); //LOWER NAV2
            result.Add(new RadioPanelKnobP51D(1, Convert.ToInt32("1000", 2), true, RadioPanelPZ69KnobsP51D.LOWER_NO_USE3)); //LOWER ADF
            result.Add(new RadioPanelKnobP51D(1, Convert.ToInt32("10000", 2), true, RadioPanelPZ69KnobsP51D.LOWER_NO_USE4)); //LOWER DME
            result.Add(new RadioPanelKnobP51D(1, Convert.ToInt32("100000", 2), true, RadioPanelPZ69KnobsP51D.LOWER_NO_USE5)); //LOWER XPDR
            result.Add(new RadioPanelKnobP51D(1, Convert.ToInt32("1000000", 2), true, RadioPanelPZ69KnobsP51D.UPPER_FREQ_SWITCH));
            result.Add(new RadioPanelKnobP51D(1, Convert.ToInt32("10000000", 2), true, RadioPanelPZ69KnobsP51D.LOWER_FREQ_SWITCH));

            //Group 2
            result.Add(new RadioPanelKnobP51D(0, Convert.ToInt32("1", 2), true, RadioPanelPZ69KnobsP51D.UPPER_VHF)); //UPPER COM1
            result.Add(new RadioPanelKnobP51D(0, Convert.ToInt32("10", 2), true, RadioPanelPZ69KnobsP51D.UPPER_NO_USE0)); //UPPER COM2
            result.Add(new RadioPanelKnobP51D(0, Convert.ToInt32("100", 2), true, RadioPanelPZ69KnobsP51D.UPPER_NO_USE1)); //UPPER NAV1
            result.Add(new RadioPanelKnobP51D(0, Convert.ToInt32("1000", 2), true, RadioPanelPZ69KnobsP51D.UPPER_NO_USE2)); //UPPER NAV2
            result.Add(new RadioPanelKnobP51D(0, Convert.ToInt32("10000", 2), true, RadioPanelPZ69KnobsP51D.UPPER_NO_USE3)); //UPPER ADF
            result.Add(new RadioPanelKnobP51D(0, Convert.ToInt32("100000", 2), true, RadioPanelPZ69KnobsP51D.UPPER_NO_USE4)); //UPPER DME
            result.Add(new RadioPanelKnobP51D(0, Convert.ToInt32("1000000", 2), true, RadioPanelPZ69KnobsP51D.UPPER_NO_USE5)); //UPPER XPDR
            result.Add(new RadioPanelKnobP51D(0, Convert.ToInt32("10000000", 2), true, RadioPanelPZ69KnobsP51D.LOWER_VHF)); //LOWER COM1
            return result;
        }




    }


}
