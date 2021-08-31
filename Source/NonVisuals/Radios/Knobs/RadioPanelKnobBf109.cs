using System;
using System.Collections.Generic;
using NonVisuals.Interfaces;

namespace NonVisuals.Radios.Knobs
{

    public class RadioPanelKnobBf109 : ISaitekPanelKnob
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

        public static HashSet<ISaitekPanelKnob> GetRadioPanelKnobs()
        {
            //true means clockwise turn
            var result = new HashSet<ISaitekPanelKnob>();
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
            result.Add(new RadioPanelKnobBf109(1, Convert.ToInt32("100", 2), true, RadioPanelPZ69KnobsBf109.LOWER_NO_USE1)); //LOWER NAV2
            result.Add(new RadioPanelKnobBf109(1, Convert.ToInt32("1000", 2), true, RadioPanelPZ69KnobsBf109.LOWER_NO_USE2)); //LOWER ADF
            result.Add(new RadioPanelKnobBf109(1, Convert.ToInt32("10000", 2), true, RadioPanelPZ69KnobsBf109.LOWER_NO_USE3)); //LOWER DME
            result.Add(new RadioPanelKnobBf109(1, Convert.ToInt32("100000", 2), true, RadioPanelPZ69KnobsBf109.LOWER_NO_USE4)); //LOWER XPDR
            result.Add(new RadioPanelKnobBf109(1, Convert.ToInt32("1000000", 2), true, RadioPanelPZ69KnobsBf109.UPPER_FREQ_SWITCH));
            result.Add(new RadioPanelKnobBf109(1, Convert.ToInt32("10000000", 2), true, RadioPanelPZ69KnobsBf109.LOWER_FREQ_SWITCH));

            //Group 2
            result.Add(new RadioPanelKnobBf109(0, Convert.ToInt32("1", 2), true, RadioPanelPZ69KnobsBf109.UPPER_FUG16ZY)); //UPPER COM1
            result.Add(new RadioPanelKnobBf109(0, Convert.ToInt32("10", 2), true, RadioPanelPZ69KnobsBf109.UPPER_IFF)); //UPPER COM2
            result.Add(new RadioPanelKnobBf109(0, Convert.ToInt32("100", 2), true, RadioPanelPZ69KnobsBf109.UPPER_HOMING)); //UPPER NAV1
            result.Add(new RadioPanelKnobBf109(0, Convert.ToInt32("1000", 2), true, RadioPanelPZ69KnobsBf109.UPPER_NO_USE1)); //UPPER NAV2
            result.Add(new RadioPanelKnobBf109(0, Convert.ToInt32("10000", 2), true, RadioPanelPZ69KnobsBf109.UPPER_NO_USE2)); //UPPER ADF
            result.Add(new RadioPanelKnobBf109(0, Convert.ToInt32("100000", 2), true, RadioPanelPZ69KnobsBf109.UPPER_NO_USE3)); //UPPER DME
            result.Add(new RadioPanelKnobBf109(0, Convert.ToInt32("1000000", 2), true, RadioPanelPZ69KnobsBf109.UPPER_NO_USE4)); //UPPER XPDR
            result.Add(new RadioPanelKnobBf109(0, Convert.ToInt32("10000000", 2), true, RadioPanelPZ69KnobsBf109.LOWER_FUG16ZY)); //LOWER COM1
            return result;
        }
    }
}
