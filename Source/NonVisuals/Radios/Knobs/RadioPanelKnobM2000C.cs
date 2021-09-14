namespace NonVisuals.Radios.Knobs
{
    using System;
    using System.Collections.Generic;

    using MEF;

    using NonVisuals.Interfaces;

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
            // true means clockwise turn
            var result = new HashSet<ISaitekPanelKnob>();

            // Group 0
            result.Add(new RadioPanelKnobM2000C(2, Convert.ToInt32("1", 2), true, RadioPanelPZ69KnobsM2000C.UPPER_SMALL_FREQ_WHEEL_INC));
            result.Add(new RadioPanelKnobM2000C(2, Convert.ToInt32("10", 2), false, RadioPanelPZ69KnobsM2000C.UPPER_SMALL_FREQ_WHEEL_DEC));
            result.Add(new RadioPanelKnobM2000C(2, Convert.ToInt32("100", 2), true, RadioPanelPZ69KnobsM2000C.UPPER_LARGE_FREQ_WHEEL_INC));
            result.Add(new RadioPanelKnobM2000C(2, Convert.ToInt32("1000", 2), false, RadioPanelPZ69KnobsM2000C.UPPER_LARGE_FREQ_WHEEL_DEC));
            result.Add(new RadioPanelKnobM2000C(2, Convert.ToInt32("10000", 2), true, RadioPanelPZ69KnobsM2000C.LOWER_SMALL_FREQ_WHEEL_INC));
            result.Add(new RadioPanelKnobM2000C(2, Convert.ToInt32("100000", 2), false, RadioPanelPZ69KnobsM2000C.LOWER_SMALL_FREQ_WHEEL_DEC));
            result.Add(new RadioPanelKnobM2000C(2, Convert.ToInt32("1000000", 2), true, RadioPanelPZ69KnobsM2000C.LOWER_LARGE_FREQ_WHEEL_INC));
            result.Add(new RadioPanelKnobM2000C(2, Convert.ToInt32("10000000", 2), false, RadioPanelPZ69KnobsM2000C.LOWER_LARGE_FREQ_WHEEL_DEC));

            // Group 1
            result.Add(new RadioPanelKnobM2000C(1, Convert.ToInt32("1", 2), true, RadioPanelPZ69KnobsM2000C.LOWER_UHF)); // LOWER COM2
            result.Add(new RadioPanelKnobM2000C(1, Convert.ToInt32("10", 2), true, RadioPanelPZ69KnobsM2000C.LOWER_TACAN)); // LOWER NAV1
            result.Add(new RadioPanelKnobM2000C(1, Convert.ToInt32("100", 2), true, RadioPanelPZ69KnobsM2000C.LOWER_VOR)); // LOWER NAV2
            result.Add(new RadioPanelKnobM2000C(1, Convert.ToInt32("1000", 2), true, RadioPanelPZ69KnobsM2000C.LOWER_NO_USE2)); // LOWER ADF
            result.Add(new RadioPanelKnobM2000C(1, Convert.ToInt32("10000", 2), true, RadioPanelPZ69KnobsM2000C.LOWER_NO_USE3)); // LOWER DME
            result.Add(new RadioPanelKnobM2000C(1, Convert.ToInt32("100000", 2), true, RadioPanelPZ69KnobsM2000C.LOWER_NO_USE4)); // LOWER XPDR
            result.Add(new RadioPanelKnobM2000C(1, Convert.ToInt32("1000000", 2), true, RadioPanelPZ69KnobsM2000C.UPPER_FREQ_SWITCH));
            result.Add(new RadioPanelKnobM2000C(1, Convert.ToInt32("10000000", 2), true, RadioPanelPZ69KnobsM2000C.LOWER_FREQ_SWITCH));

            // Group 2
            result.Add(new RadioPanelKnobM2000C(0, Convert.ToInt32("1", 2), true, RadioPanelPZ69KnobsM2000C.UPPER_VUHF)); // UPPER COM1
            result.Add(new RadioPanelKnobM2000C(0, Convert.ToInt32("10", 2), true, RadioPanelPZ69KnobsM2000C.UPPER_UHF)); // UPPER COM2
            result.Add(new RadioPanelKnobM2000C(0, Convert.ToInt32("100", 2), true, RadioPanelPZ69KnobsM2000C.UPPER_TACAN)); // UPPER NAV1
            result.Add(new RadioPanelKnobM2000C(0, Convert.ToInt32("1000", 2), true, RadioPanelPZ69KnobsM2000C.UPPER_VOR)); // UPPER NAV2
            result.Add(new RadioPanelKnobM2000C(0, Convert.ToInt32("10000", 2), true, RadioPanelPZ69KnobsM2000C.UPPER_NO_USE2)); // UPPER ADF
            result.Add(new RadioPanelKnobM2000C(0, Convert.ToInt32("100000", 2), true, RadioPanelPZ69KnobsM2000C.UPPER_NO_USE3)); // UPPER DME
            result.Add(new RadioPanelKnobM2000C(0, Convert.ToInt32("1000000", 2), true, RadioPanelPZ69KnobsM2000C.UPPER_NO_USE4)); // UPPER XPDR
            result.Add(new RadioPanelKnobM2000C(0, Convert.ToInt32("10000000", 2), true, RadioPanelPZ69KnobsM2000C.LOWER_VUHF)); // LOWER COM1
            return result;
        }
    }


}
