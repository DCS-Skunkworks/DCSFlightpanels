namespace NonVisuals.Radios.Knobs
{
    using System;
    using System.Collections.Generic;

    using MEF;

    using NonVisuals.Interfaces;

    public class RadioPanelKnobSRS : ISaitekPanelKnob
    {
        public RadioPanelKnobSRS(int group, int mask, bool isOn, RadioPanelPZ69KnobsSRS radioPanelPZ69Knob)
        {
            Group = group;
            Mask = mask;
            IsOn = isOn;
            RadioPanelPZ69Knob = radioPanelPZ69Knob;
        }

        public int Group { get; set; }

        public int Mask { get; set; }

        public bool IsOn { get; set; }

        public RadioPanelPZ69KnobsSRS RadioPanelPZ69Knob { get; set; }

        public static HashSet<ISaitekPanelKnob> GetRadioPanelKnobs()
        {
            // true means clockwise turn
            var result = new HashSet<ISaitekPanelKnob>();

            // Group 0
            result.Add(new RadioPanelKnobSRS(2, Convert.ToInt32("1", 2), true, RadioPanelPZ69KnobsSRS.UPPER_SMALL_FREQ_WHEEL_INC));
            result.Add(new RadioPanelKnobSRS(2, Convert.ToInt32("10", 2), false, RadioPanelPZ69KnobsSRS.UPPER_SMALL_FREQ_WHEEL_DEC));
            result.Add(new RadioPanelKnobSRS(2, Convert.ToInt32("100", 2), true, RadioPanelPZ69KnobsSRS.UPPER_LARGE_FREQ_WHEEL_INC));
            result.Add(new RadioPanelKnobSRS(2, Convert.ToInt32("1000", 2), false, RadioPanelPZ69KnobsSRS.UPPER_LARGE_FREQ_WHEEL_DEC));
            result.Add(new RadioPanelKnobSRS(2, Convert.ToInt32("10000", 2), true, RadioPanelPZ69KnobsSRS.LOWER_SMALL_FREQ_WHEEL_INC));
            result.Add(new RadioPanelKnobSRS(2, Convert.ToInt32("100000", 2), false, RadioPanelPZ69KnobsSRS.LOWER_SMALL_FREQ_WHEEL_DEC));
            result.Add(new RadioPanelKnobSRS(2, Convert.ToInt32("1000000", 2), true, RadioPanelPZ69KnobsSRS.LOWER_LARGE_FREQ_WHEEL_INC));
            result.Add(new RadioPanelKnobSRS(2, Convert.ToInt32("10000000", 2), false, RadioPanelPZ69KnobsSRS.LOWER_LARGE_FREQ_WHEEL_DEC));

            // Group 1
            result.Add(new RadioPanelKnobSRS(1, Convert.ToInt32("1", 2), true, RadioPanelPZ69KnobsSRS.LOWER_COM2)); // LOWER COM2
            result.Add(new RadioPanelKnobSRS(1, Convert.ToInt32("10", 2), true, RadioPanelPZ69KnobsSRS.LOWER_NAV1)); // LOWER NAV1
            result.Add(new RadioPanelKnobSRS(1, Convert.ToInt32("100", 2), true, RadioPanelPZ69KnobsSRS.LOWER_NAV2)); // LOWER NAV2
            result.Add(new RadioPanelKnobSRS(1, Convert.ToInt32("1000", 2), true, RadioPanelPZ69KnobsSRS.LOWER_ADF)); // LOWER ADF
            result.Add(new RadioPanelKnobSRS(1, Convert.ToInt32("10000", 2), true, RadioPanelPZ69KnobsSRS.LOWER_DME)); // LOWER DME
            result.Add(new RadioPanelKnobSRS(1, Convert.ToInt32("100000", 2), true, RadioPanelPZ69KnobsSRS.LOWER_XPDR)); // LOWER XPDR
            result.Add(new RadioPanelKnobSRS(1, Convert.ToInt32("1000000", 2), true, RadioPanelPZ69KnobsSRS.UPPER_FREQ_SWITCH));
            result.Add(new RadioPanelKnobSRS(1, Convert.ToInt32("10000000", 2), true, RadioPanelPZ69KnobsSRS.LOWER_FREQ_SWITCH));

            // Group 2
            result.Add(new RadioPanelKnobSRS(0, Convert.ToInt32("1", 2), true, RadioPanelPZ69KnobsSRS.UPPER_COM1)); // UPPER COM1
            result.Add(new RadioPanelKnobSRS(0, Convert.ToInt32("10", 2), true, RadioPanelPZ69KnobsSRS.UPPER_COM2)); // UPPER COM2
            result.Add(new RadioPanelKnobSRS(0, Convert.ToInt32("100", 2), true, RadioPanelPZ69KnobsSRS.UPPER_NAV1)); // UPPER NAV1
            result.Add(new RadioPanelKnobSRS(0, Convert.ToInt32("1000", 2), true, RadioPanelPZ69KnobsSRS.UPPER_NAV2)); // UPPER NAV2
            result.Add(new RadioPanelKnobSRS(0, Convert.ToInt32("10000", 2), true, RadioPanelPZ69KnobsSRS.UPPER_ADF)); // UPPER ADF
            result.Add(new RadioPanelKnobSRS(0, Convert.ToInt32("100000", 2), true, RadioPanelPZ69KnobsSRS.UPPER_DME)); // UPPER DME
            result.Add(new RadioPanelKnobSRS(0, Convert.ToInt32("1000000", 2), true, RadioPanelPZ69KnobsSRS.UPPER_XPDR)); // UPPER XPDR
            result.Add(new RadioPanelKnobSRS(0, Convert.ToInt32("10000000", 2), true, RadioPanelPZ69KnobsSRS.LOWER_COM1)); // LOWER COM1
            return result;
        }
    }


}
