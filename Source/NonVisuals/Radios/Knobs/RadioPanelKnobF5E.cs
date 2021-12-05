namespace NonVisuals.Radios.Knobs
{
    using System;
    using System.Collections.Generic;

    using MEF;

    using NonVisuals.Interfaces;

    public class RadioPanelKnobF5E : ISaitekPanelKnob
    {
        public RadioPanelKnobF5E(int group, int mask, bool isOn, RadioPanelPZ69KnobsF5E radioPanelPZ69Knob)
        {
            Group = group;
            Mask = mask;
            IsOn = isOn;
            RadioPanelPZ69Knob = radioPanelPZ69Knob;
        }

        public int Group { get; set; }

        public int Mask { get; set; }

        public bool IsOn { get; set; }

        public RadioPanelPZ69KnobsF5E RadioPanelPZ69Knob { get; set; }

        public static HashSet<ISaitekPanelKnob> GetRadioPanelKnobs()
        {
            // true means clockwise turn
            var result = new HashSet<ISaitekPanelKnob>
            {
                // Group 0
                new RadioPanelKnobF5E(2, Convert.ToInt32("1", 2), true, RadioPanelPZ69KnobsF5E.UPPER_SMALL_FREQ_WHEEL_INC),
                new RadioPanelKnobF5E(2, Convert.ToInt32("10", 2), false, RadioPanelPZ69KnobsF5E.UPPER_SMALL_FREQ_WHEEL_DEC),
                new RadioPanelKnobF5E(2, Convert.ToInt32("100", 2), true, RadioPanelPZ69KnobsF5E.UPPER_LARGE_FREQ_WHEEL_INC),
                new RadioPanelKnobF5E(2, Convert.ToInt32("1000", 2), false, RadioPanelPZ69KnobsF5E.UPPER_LARGE_FREQ_WHEEL_DEC),
                new RadioPanelKnobF5E(2, Convert.ToInt32("10000", 2), true, RadioPanelPZ69KnobsF5E.LOWER_SMALL_FREQ_WHEEL_INC),
                new RadioPanelKnobF5E(2, Convert.ToInt32("100000", 2), false, RadioPanelPZ69KnobsF5E.LOWER_SMALL_FREQ_WHEEL_DEC),
                new RadioPanelKnobF5E(2, Convert.ToInt32("1000000", 2), true, RadioPanelPZ69KnobsF5E.LOWER_LARGE_FREQ_WHEEL_INC),
                new RadioPanelKnobF5E(2, Convert.ToInt32("10000000", 2), false, RadioPanelPZ69KnobsF5E.LOWER_LARGE_FREQ_WHEEL_DEC),
                // Group 1
                new RadioPanelKnobF5E(1, Convert.ToInt32("1", 2), true, RadioPanelPZ69KnobsF5E.LOWER_NOUSE1), // LOWER COM 2
                new RadioPanelKnobF5E(1, Convert.ToInt32("10", 2), true, RadioPanelPZ69KnobsF5E.LOWER_NOUSE2), // LOWER NAV 1
                new RadioPanelKnobF5E(1, Convert.ToInt32("100", 2), true, RadioPanelPZ69KnobsF5E.LOWER_NOUSE3), // LOWER NAV 2
                new RadioPanelKnobF5E(1, Convert.ToInt32("1000", 2), true, RadioPanelPZ69KnobsF5E.LOWER_TACAN), // LOWER ADF
                new RadioPanelKnobF5E(1, Convert.ToInt32("10000", 2), true, RadioPanelPZ69KnobsF5E.LOWER_NOUSE4), // LOWER DME
                new RadioPanelKnobF5E(1, Convert.ToInt32("100000", 2), true, RadioPanelPZ69KnobsF5E.LOWER_NOUSE5), // LOWER XPDR
                new RadioPanelKnobF5E(1, Convert.ToInt32("1000000", 2), true, RadioPanelPZ69KnobsF5E.UPPER_FREQ_SWITCH),
                new RadioPanelKnobF5E(1, Convert.ToInt32("10000000", 2), true, RadioPanelPZ69KnobsF5E.LOWER_FREQ_SWITCH),
                // Group 2
                new RadioPanelKnobF5E(0, Convert.ToInt32("1", 2), true, RadioPanelPZ69KnobsF5E.UPPER_UHF), // UPPER COM 1
                new RadioPanelKnobF5E(0, Convert.ToInt32("10", 2), true, RadioPanelPZ69KnobsF5E.UPPER_NOUSE1), // UPPER COM 2
                new RadioPanelKnobF5E(0, Convert.ToInt32("100", 2), true, RadioPanelPZ69KnobsF5E.UPPER_NOUSE2), // UPPER NAV 1
                new RadioPanelKnobF5E(0, Convert.ToInt32("1000", 2), true, RadioPanelPZ69KnobsF5E.UPPER_NOUSE3), // UPPER NAV 2
                new RadioPanelKnobF5E(0, Convert.ToInt32("10000", 2), true, RadioPanelPZ69KnobsF5E.UPPER_TACAN), // UPPER ADF
                new RadioPanelKnobF5E(0, Convert.ToInt32("100000", 2), true, RadioPanelPZ69KnobsF5E.UPPER_NOUSE4), // UPPER DME
                new RadioPanelKnobF5E(0, Convert.ToInt32("1000000", 2), true, RadioPanelPZ69KnobsF5E.UPPER_NOUSE5), // UPPER XPDR
                new RadioPanelKnobF5E(0, Convert.ToInt32("10000000", 2), true, RadioPanelPZ69KnobsF5E.LOWER_UHF) // LOWER COM 1 
            };

            return result;
        }
    }
}
