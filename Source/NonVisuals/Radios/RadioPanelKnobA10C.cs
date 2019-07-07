using System;
using System.Collections.Generic;
using NonVisuals;

namespace NonVisuals.Radios
{
    public enum RadioPanelPZ69KnobsA10C
    {
        UPPER_VHFAM = 0,
        UPPER_UHF = 2,
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
        LOWER_VHFAM = 4096,
        LOWER_UHF = 8192,
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

    public enum CurrentA10RadioMode
    {
        UHF = 0,
        VHFFM = 2,
        VHFAM = 4,
        TACAN = 8,
        ILS = 16
    }

    public class RadioPanelKnobA10C : ISaitekPanelKnob
    {
        public RadioPanelKnobA10C(int group, int mask, bool isOn, RadioPanelPZ69KnobsA10C radioPanelPZ69Knob)
        {
            Group = group;
            Mask = mask;
            IsOn = isOn;
            RadioPanelPZ69Knob = radioPanelPZ69Knob;
        }

        public int Group { get; set; }

        public int Mask { get; set; }

        public bool IsOn { get; set; }

        public RadioPanelPZ69KnobsA10C RadioPanelPZ69Knob { get; set; }

        public string ExportString()
        {
            return "RadioPanelKnob{" + Enum.GetName(typeof(RadioPanelPZ69KnobsA10C), RadioPanelPZ69Knob) + "}";
        }

        public static HashSet<ISaitekPanelKnob> GetRadioPanelKnobs()
        {
            //true means clockwise turn
            var result = new HashSet<ISaitekPanelKnob>();
            //Group 0
            result.Add(new RadioPanelKnobA10C(2, Convert.ToInt32("1", 2), true, RadioPanelPZ69KnobsA10C.UPPER_SMALL_FREQ_WHEEL_INC));
            result.Add(new RadioPanelKnobA10C(2, Convert.ToInt32("10", 2), false, RadioPanelPZ69KnobsA10C.UPPER_SMALL_FREQ_WHEEL_DEC));
            result.Add(new RadioPanelKnobA10C(2, Convert.ToInt32("100", 2), true, RadioPanelPZ69KnobsA10C.UPPER_LARGE_FREQ_WHEEL_INC));
            result.Add(new RadioPanelKnobA10C(2, Convert.ToInt32("1000", 2), false, RadioPanelPZ69KnobsA10C.UPPER_LARGE_FREQ_WHEEL_DEC));
            result.Add(new RadioPanelKnobA10C(2, Convert.ToInt32("10000", 2), true, RadioPanelPZ69KnobsA10C.LOWER_SMALL_FREQ_WHEEL_INC));
            result.Add(new RadioPanelKnobA10C(2, Convert.ToInt32("100000", 2), false, RadioPanelPZ69KnobsA10C.LOWER_SMALL_FREQ_WHEEL_DEC));
            result.Add(new RadioPanelKnobA10C(2, Convert.ToInt32("1000000", 2), true, RadioPanelPZ69KnobsA10C.LOWER_LARGE_FREQ_WHEEL_INC));
            result.Add(new RadioPanelKnobA10C(2, Convert.ToInt32("10000000", 2), false, RadioPanelPZ69KnobsA10C.LOWER_LARGE_FREQ_WHEEL_DEC));

            //Group 1
            result.Add(new RadioPanelKnobA10C(1, Convert.ToInt32("1", 2), true, RadioPanelPZ69KnobsA10C.LOWER_UHF));
            result.Add(new RadioPanelKnobA10C(1, Convert.ToInt32("10", 2), true, RadioPanelPZ69KnobsA10C.LOWER_VHFFM));
            result.Add(new RadioPanelKnobA10C(1, Convert.ToInt32("100", 2), true, RadioPanelPZ69KnobsA10C.LOWER_ILS));
            result.Add(new RadioPanelKnobA10C(1, Convert.ToInt32("1000", 2), true, RadioPanelPZ69KnobsA10C.LOWER_TACAN));
            result.Add(new RadioPanelKnobA10C(1, Convert.ToInt32("10000", 2), true, RadioPanelPZ69KnobsA10C.LOWER_DME));
            result.Add(new RadioPanelKnobA10C(1, Convert.ToInt32("100000", 2), true, RadioPanelPZ69KnobsA10C.LOWER_XPDR));
            result.Add(new RadioPanelKnobA10C(1, Convert.ToInt32("1000000", 2), true, RadioPanelPZ69KnobsA10C.UPPER_FREQ_SWITCH));
            result.Add(new RadioPanelKnobA10C(1, Convert.ToInt32("10000000", 2), true, RadioPanelPZ69KnobsA10C.LOWER_FREQ_SWITCH));

            //Group 2
            result.Add(new RadioPanelKnobA10C(0, Convert.ToInt32("1", 2), true, RadioPanelPZ69KnobsA10C.UPPER_VHFAM)); //UPPER COM 1
            result.Add(new RadioPanelKnobA10C(0, Convert.ToInt32("10", 2), true, RadioPanelPZ69KnobsA10C.UPPER_UHF)); //UPPER COM 2
            result.Add(new RadioPanelKnobA10C(0, Convert.ToInt32("100", 2), true, RadioPanelPZ69KnobsA10C.UPPER_VHFFM)); //UPPER NAV 1
            result.Add(new RadioPanelKnobA10C(0, Convert.ToInt32("1000", 2), true, RadioPanelPZ69KnobsA10C.UPPER_ILS)); //UPPER NAV 2
            result.Add(new RadioPanelKnobA10C(0, Convert.ToInt32("10000", 2), true, RadioPanelPZ69KnobsA10C.UPPER_TACAN)); //UPPER ADF
            result.Add(new RadioPanelKnobA10C(0, Convert.ToInt32("100000", 2), true, RadioPanelPZ69KnobsA10C.UPPER_DME)); //UPPER DME
            result.Add(new RadioPanelKnobA10C(0, Convert.ToInt32("1000000", 2), true, RadioPanelPZ69KnobsA10C.UPPER_XPDR)); //UPPER XPDR
            result.Add(new RadioPanelKnobA10C(0, Convert.ToInt32("10000000", 2), true, RadioPanelPZ69KnobsA10C.LOWER_VHFAM)); //LOWER COM 1 
            return result;
        }




    }
}
