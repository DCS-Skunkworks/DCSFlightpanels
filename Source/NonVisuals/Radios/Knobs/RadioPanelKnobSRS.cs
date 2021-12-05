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
            var result = new HashSet<ISaitekPanelKnob>
            {
                // Group 0
                new RadioPanelKnobSRS(2, Convert.ToInt32("1", 2), true, RadioPanelPZ69KnobsSRS.UPPER_SMALL_FREQ_WHEEL_INC),
                new RadioPanelKnobSRS(2, Convert.ToInt32("10", 2), false, RadioPanelPZ69KnobsSRS.UPPER_SMALL_FREQ_WHEEL_DEC),
                new RadioPanelKnobSRS(2, Convert.ToInt32("100", 2), true, RadioPanelPZ69KnobsSRS.UPPER_LARGE_FREQ_WHEEL_INC),
                new RadioPanelKnobSRS(2, Convert.ToInt32("1000", 2), false, RadioPanelPZ69KnobsSRS.UPPER_LARGE_FREQ_WHEEL_DEC),
                new RadioPanelKnobSRS(2, Convert.ToInt32("10000", 2), true, RadioPanelPZ69KnobsSRS.LOWER_SMALL_FREQ_WHEEL_INC),
                new RadioPanelKnobSRS(2, Convert.ToInt32("100000", 2), false, RadioPanelPZ69KnobsSRS.LOWER_SMALL_FREQ_WHEEL_DEC),
                new RadioPanelKnobSRS(2, Convert.ToInt32("1000000", 2), true, RadioPanelPZ69KnobsSRS.LOWER_LARGE_FREQ_WHEEL_INC),
                new RadioPanelKnobSRS(2, Convert.ToInt32("10000000", 2), false, RadioPanelPZ69KnobsSRS.LOWER_LARGE_FREQ_WHEEL_DEC),
                // Group 1
                new RadioPanelKnobSRS(1, Convert.ToInt32("1", 2), true, RadioPanelPZ69KnobsSRS.LOWER_COM2), // LOWER COM2
                new RadioPanelKnobSRS(1, Convert.ToInt32("10", 2), true, RadioPanelPZ69KnobsSRS.LOWER_NAV1), // LOWER NAV1
                new RadioPanelKnobSRS(1, Convert.ToInt32("100", 2), true, RadioPanelPZ69KnobsSRS.LOWER_NAV2), // LOWER NAV2
                new RadioPanelKnobSRS(1, Convert.ToInt32("1000", 2), true, RadioPanelPZ69KnobsSRS.LOWER_ADF), // LOWER ADF
                new RadioPanelKnobSRS(1, Convert.ToInt32("10000", 2), true, RadioPanelPZ69KnobsSRS.LOWER_DME), // LOWER DME
                new RadioPanelKnobSRS(1, Convert.ToInt32("100000", 2), true, RadioPanelPZ69KnobsSRS.LOWER_XPDR), // LOWER XPDR
                new RadioPanelKnobSRS(1, Convert.ToInt32("1000000", 2), true, RadioPanelPZ69KnobsSRS.UPPER_FREQ_SWITCH),
                new RadioPanelKnobSRS(1, Convert.ToInt32("10000000", 2), true, RadioPanelPZ69KnobsSRS.LOWER_FREQ_SWITCH),
                // Group 2
                new RadioPanelKnobSRS(0, Convert.ToInt32("1", 2), true, RadioPanelPZ69KnobsSRS.UPPER_COM1), // UPPER COM1
                new RadioPanelKnobSRS(0, Convert.ToInt32("10", 2), true, RadioPanelPZ69KnobsSRS.UPPER_COM2), // UPPER COM2
                new RadioPanelKnobSRS(0, Convert.ToInt32("100", 2), true, RadioPanelPZ69KnobsSRS.UPPER_NAV1), // UPPER NAV1
                new RadioPanelKnobSRS(0, Convert.ToInt32("1000", 2), true, RadioPanelPZ69KnobsSRS.UPPER_NAV2), // UPPER NAV2
                new RadioPanelKnobSRS(0, Convert.ToInt32("10000", 2), true, RadioPanelPZ69KnobsSRS.UPPER_ADF), // UPPER ADF
                new RadioPanelKnobSRS(0, Convert.ToInt32("100000", 2), true, RadioPanelPZ69KnobsSRS.UPPER_DME), // UPPER DME
                new RadioPanelKnobSRS(0, Convert.ToInt32("1000000", 2), true, RadioPanelPZ69KnobsSRS.UPPER_XPDR), // UPPER XPDR
                new RadioPanelKnobSRS(0, Convert.ToInt32("10000000", 2), true, RadioPanelPZ69KnobsSRS.LOWER_COM1) // LOWER COM1
            };

            return result;
        }
    }


}
