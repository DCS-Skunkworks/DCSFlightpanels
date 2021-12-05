namespace NonVisuals.Radios.Knobs
{
    using System;
    using System.Collections.Generic;

    using MEF;

    using NonVisuals.Interfaces;

    public class RadioPanelKnobAJS37 : ISaitekPanelKnob
    {
        public RadioPanelKnobAJS37(int group, int mask, bool isOn, RadioPanelPZ69KnobsAJS37 radioPanelPZ69Knob)
        {
            Group = group;
            Mask = mask;
            IsOn = isOn;
            RadioPanelPZ69Knob = radioPanelPZ69Knob;
        }

        public int Group { get; set; }

        public int Mask { get; set; }

        public bool IsOn { get; set; }

        public RadioPanelPZ69KnobsAJS37 RadioPanelPZ69Knob { get; set; }
        
        public static HashSet<ISaitekPanelKnob> GetRadioPanelKnobs()
        {
            // true means clockwise turn
            var result = new HashSet<ISaitekPanelKnob>
            {
                // Group 0
                new RadioPanelKnobAJS37(2, Convert.ToInt32("1", 2), true, RadioPanelPZ69KnobsAJS37.UPPER_SMALL_FREQ_WHEEL_INC),
                new RadioPanelKnobAJS37(2, Convert.ToInt32("10", 2), false, RadioPanelPZ69KnobsAJS37.UPPER_SMALL_FREQ_WHEEL_DEC),
                new RadioPanelKnobAJS37(2, Convert.ToInt32("100", 2), true, RadioPanelPZ69KnobsAJS37.UPPER_LARGE_FREQ_WHEEL_INC),
                new RadioPanelKnobAJS37(2, Convert.ToInt32("1000", 2), false, RadioPanelPZ69KnobsAJS37.UPPER_LARGE_FREQ_WHEEL_DEC),
                new RadioPanelKnobAJS37(2, Convert.ToInt32("10000", 2), true, RadioPanelPZ69KnobsAJS37.LOWER_SMALL_FREQ_WHEEL_INC),
                new RadioPanelKnobAJS37(2, Convert.ToInt32("100000", 2), false, RadioPanelPZ69KnobsAJS37.LOWER_SMALL_FREQ_WHEEL_DEC),
                new RadioPanelKnobAJS37(2, Convert.ToInt32("1000000", 2), true, RadioPanelPZ69KnobsAJS37.LOWER_LARGE_FREQ_WHEEL_INC),
                new RadioPanelKnobAJS37(2, Convert.ToInt32("10000000", 2), false, RadioPanelPZ69KnobsAJS37.LOWER_LARGE_FREQ_WHEEL_DEC),
                // Group 1
                new RadioPanelKnobAJS37(1, Convert.ToInt32("1", 2), true, RadioPanelPZ69KnobsAJS37.LOWER_FR24), // LOWER COM2
                new RadioPanelKnobAJS37(1, Convert.ToInt32("10", 2), true, RadioPanelPZ69KnobsAJS37.LOWER_TILS), // LOWER NAV1
                new RadioPanelKnobAJS37(1, Convert.ToInt32("100", 2), true, RadioPanelPZ69KnobsAJS37.LOWER_NO_USE0), // LOWER NAV2
                new RadioPanelKnobAJS37(1, Convert.ToInt32("1000", 2), true, RadioPanelPZ69KnobsAJS37.LOWER_NO_USE1), // LOWER ADF
                new RadioPanelKnobAJS37(1, Convert.ToInt32("10000", 2), true, RadioPanelPZ69KnobsAJS37.LOWER_NO_USE2), // LOWER DME
                new RadioPanelKnobAJS37(1, Convert.ToInt32("100000", 2), true, RadioPanelPZ69KnobsAJS37.LOWER_NO_USE3), // LOWER XPDR
                new RadioPanelKnobAJS37(1, Convert.ToInt32("1000000", 2), true, RadioPanelPZ69KnobsAJS37.UPPER_FREQ_SWITCH),
                new RadioPanelKnobAJS37(1, Convert.ToInt32("10000000", 2), true, RadioPanelPZ69KnobsAJS37.LOWER_FREQ_SWITCH),
                // Group 2
                new RadioPanelKnobAJS37(0, Convert.ToInt32("1", 2), true, RadioPanelPZ69KnobsAJS37.UPPER_FR22), // UPPER COM1
                new RadioPanelKnobAJS37(0, Convert.ToInt32("10", 2), true, RadioPanelPZ69KnobsAJS37.UPPER_FR24), // UPPER COM2
                new RadioPanelKnobAJS37(0, Convert.ToInt32("100", 2), true, RadioPanelPZ69KnobsAJS37.UPPER_TILS), // UPPER NAV1
                new RadioPanelKnobAJS37(0, Convert.ToInt32("1000", 2), true, RadioPanelPZ69KnobsAJS37.UPPER_NO_USE0), // UPPER NAV2
                new RadioPanelKnobAJS37(0, Convert.ToInt32("10000", 2), true, RadioPanelPZ69KnobsAJS37.UPPER_NO_USE1), // UPPER ADF
                new RadioPanelKnobAJS37(0, Convert.ToInt32("100000", 2), true, RadioPanelPZ69KnobsAJS37.UPPER_NO_USE2), // UPPER DME
                new RadioPanelKnobAJS37(0, Convert.ToInt32("1000000", 2), true, RadioPanelPZ69KnobsAJS37.UPPER_NO_USE3), // UPPER XPDR
                new RadioPanelKnobAJS37(0, Convert.ToInt32("10000000", 2), true, RadioPanelPZ69KnobsAJS37.LOWER_FR22) // LOWER COM1
            };

            return result;
        }
    }


}
