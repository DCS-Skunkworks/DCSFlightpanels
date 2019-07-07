//
//  added by Capt Zeen
//

using System;
using System.Collections.Generic;

namespace NonVisuals.Radios
{
    public enum RadioPanelPZ69KnobsFA18C
    {
        UPPER_COMM1 = 0,
        UPPER_COMM2 = 2,
        UPPER_VHFFM = 4,
        UPPER_ILS = 8,
        UPPER_TACAN = 16,
        UPPER_DME = 32,
        UPPER_XPDR = 64,
        UPPER_SMALL_FREQ_WHEEL_INC = 128,
        UPPER_SMALL_FREQ_WHEEL_DEC = 256,
        UPPER_LARGE_FREQ_WHEEL_INC = 512,
        UPPER_LARGE_FREQ_WHEEL_DEC = 1024,
        UPPER_FREQ_SWITCH = 2056,
        LOWER_COMM1 = 4096,
        LOWER_COMM2 = 8192,
        LOWER_VHFFM = 16384,
        LOWER_ILS = 32768,
        LOWER_TACAN = 65536,
        LOWER_DME = 131072,
        LOWER_XPDR = 262144,
        LOWER_SMALL_FREQ_WHEEL_INC = 8388608,
        LOWER_SMALL_FREQ_WHEEL_DEC = 524288,
        LOWER_LARGE_FREQ_WHEEL_INC = 1048576,
        LOWER_LARGE_FREQ_WHEEL_DEC = 2097152,
        LOWER_FREQ_SWITCH = 4194304
    }

    public enum CurrentFA18CRadioMode
    {
        COMM2 = 0,
        VHFFM = 2,
        COMM1 = 4,
        TACAN = 8,
        ILS = 16
    }

    public class RadioPanelKnobFA18C : ISaitekPanelKnob
    {
        public RadioPanelKnobFA18C(int group, int mask, bool isOn, RadioPanelPZ69KnobsFA18C radioPanelPZ69Knob)
        {
            Group = group;
            Mask = mask;
            IsOn = isOn;
            RadioPanelPZ69Knob = radioPanelPZ69Knob;
        }

        public int Group { get; set; }

        public int Mask { get; set; }

        public bool IsOn { get; set; }

        public RadioPanelPZ69KnobsFA18C RadioPanelPZ69Knob { get; set; }

        public static HashSet<ISaitekPanelKnob> GetRadioPanelKnobs()
        {
            //true means clockwise turn
            var result = new HashSet<ISaitekPanelKnob>();
            //Group 0
            result.Add(new RadioPanelKnobFA18C(2, Convert.ToInt32("1", 2), true, RadioPanelPZ69KnobsFA18C.UPPER_SMALL_FREQ_WHEEL_INC));
            result.Add(new RadioPanelKnobFA18C(2, Convert.ToInt32("10", 2), false, RadioPanelPZ69KnobsFA18C.UPPER_SMALL_FREQ_WHEEL_DEC));
            result.Add(new RadioPanelKnobFA18C(2, Convert.ToInt32("100", 2), true, RadioPanelPZ69KnobsFA18C.UPPER_LARGE_FREQ_WHEEL_INC));
            result.Add(new RadioPanelKnobFA18C(2, Convert.ToInt32("1000", 2), false, RadioPanelPZ69KnobsFA18C.UPPER_LARGE_FREQ_WHEEL_DEC));
            result.Add(new RadioPanelKnobFA18C(2, Convert.ToInt32("10000", 2), true, RadioPanelPZ69KnobsFA18C.LOWER_SMALL_FREQ_WHEEL_INC));
            result.Add(new RadioPanelKnobFA18C(2, Convert.ToInt32("100000", 2), false, RadioPanelPZ69KnobsFA18C.LOWER_SMALL_FREQ_WHEEL_DEC));
            result.Add(new RadioPanelKnobFA18C(2, Convert.ToInt32("1000000", 2), true, RadioPanelPZ69KnobsFA18C.LOWER_LARGE_FREQ_WHEEL_INC));
            result.Add(new RadioPanelKnobFA18C(2, Convert.ToInt32("10000000", 2), false, RadioPanelPZ69KnobsFA18C.LOWER_LARGE_FREQ_WHEEL_DEC));

            //Group 1
            result.Add(new RadioPanelKnobFA18C(1, Convert.ToInt32("1", 2), true, RadioPanelPZ69KnobsFA18C.LOWER_COMM2));
            result.Add(new RadioPanelKnobFA18C(1, Convert.ToInt32("10", 2), true, RadioPanelPZ69KnobsFA18C.LOWER_VHFFM));
            result.Add(new RadioPanelKnobFA18C(1, Convert.ToInt32("100", 2), true, RadioPanelPZ69KnobsFA18C.LOWER_ILS));
            result.Add(new RadioPanelKnobFA18C(1, Convert.ToInt32("1000", 2), true, RadioPanelPZ69KnobsFA18C.LOWER_TACAN));
            result.Add(new RadioPanelKnobFA18C(1, Convert.ToInt32("10000", 2), true, RadioPanelPZ69KnobsFA18C.LOWER_DME));
            result.Add(new RadioPanelKnobFA18C(1, Convert.ToInt32("100000", 2), true, RadioPanelPZ69KnobsFA18C.LOWER_XPDR));
            result.Add(new RadioPanelKnobFA18C(1, Convert.ToInt32("1000000", 2), true, RadioPanelPZ69KnobsFA18C.UPPER_FREQ_SWITCH));
            result.Add(new RadioPanelKnobFA18C(1, Convert.ToInt32("10000000", 2), true, RadioPanelPZ69KnobsFA18C.LOWER_FREQ_SWITCH));

            //Group 2
            result.Add(new RadioPanelKnobFA18C(0, Convert.ToInt32("1", 2), true, RadioPanelPZ69KnobsFA18C.UPPER_COMM1)); //UPPER COM 1
            result.Add(new RadioPanelKnobFA18C(0, Convert.ToInt32("10", 2), true, RadioPanelPZ69KnobsFA18C.UPPER_COMM2)); //UPPER COM 2
            result.Add(new RadioPanelKnobFA18C(0, Convert.ToInt32("100", 2), true, RadioPanelPZ69KnobsFA18C.UPPER_VHFFM)); //UPPER NAV 1
            result.Add(new RadioPanelKnobFA18C(0, Convert.ToInt32("1000", 2), true, RadioPanelPZ69KnobsFA18C.UPPER_ILS)); //UPPER NAV 2
            result.Add(new RadioPanelKnobFA18C(0, Convert.ToInt32("10000", 2), true, RadioPanelPZ69KnobsFA18C.UPPER_TACAN)); //UPPER ADF
            result.Add(new RadioPanelKnobFA18C(0, Convert.ToInt32("100000", 2), true, RadioPanelPZ69KnobsFA18C.UPPER_DME)); //UPPER DME
            result.Add(new RadioPanelKnobFA18C(0, Convert.ToInt32("1000000", 2), true, RadioPanelPZ69KnobsFA18C.UPPER_XPDR)); //UPPER XPDR
            result.Add(new RadioPanelKnobFA18C(0, Convert.ToInt32("10000000", 2), true, RadioPanelPZ69KnobsFA18C.LOWER_COMM1)); //LOWER COM 1 
            return result;
        }




    }
}
