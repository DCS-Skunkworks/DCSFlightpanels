namespace NonVisuals.Radios.Knobs
{
    using System;
    using System.Collections.Generic;

    using MEF;

    using NonVisuals.Interfaces;

    public class RadioPanelKnobMi8 : ISaitekPanelKnob
    {
        public RadioPanelKnobMi8(int group, int mask, bool isOn, RadioPanelPZ69KnobsMi8 radioPanelPZ69Knob)
        {
            Group = group;
            Mask = mask;
            IsOn = isOn;
            RadioPanelPZ69Knob = radioPanelPZ69Knob;
        }

        public int Group { get; set; }

        public int Mask { get; set; }

        public bool IsOn { get; set; }

        public RadioPanelPZ69KnobsMi8 RadioPanelPZ69Knob { get; set; }
        
        public static HashSet<ISaitekPanelKnob> GetRadioPanelKnobs()
        {
            // true means clockwise turn
            var result = new HashSet<ISaitekPanelKnob>
            {
                // Group 0
                new RadioPanelKnobMi8(2, Convert.ToInt32("1", 2), true, RadioPanelPZ69KnobsMi8.UPPER_SMALL_FREQ_WHEEL_INC),
                new RadioPanelKnobMi8(2, Convert.ToInt32("10", 2), false, RadioPanelPZ69KnobsMi8.UPPER_SMALL_FREQ_WHEEL_DEC),
                new RadioPanelKnobMi8(2, Convert.ToInt32("100", 2), true, RadioPanelPZ69KnobsMi8.UPPER_LARGE_FREQ_WHEEL_INC),
                new RadioPanelKnobMi8(2, Convert.ToInt32("1000", 2), false, RadioPanelPZ69KnobsMi8.UPPER_LARGE_FREQ_WHEEL_DEC),
                new RadioPanelKnobMi8(2, Convert.ToInt32("10000", 2), true, RadioPanelPZ69KnobsMi8.LOWER_SMALL_FREQ_WHEEL_INC),
                new RadioPanelKnobMi8(2, Convert.ToInt32("100000", 2), false, RadioPanelPZ69KnobsMi8.LOWER_SMALL_FREQ_WHEEL_DEC),
                new RadioPanelKnobMi8(2, Convert.ToInt32("1000000", 2), true, RadioPanelPZ69KnobsMi8.LOWER_LARGE_FREQ_WHEEL_INC),
                new RadioPanelKnobMi8(2, Convert.ToInt32("10000000", 2), false, RadioPanelPZ69KnobsMi8.LOWER_LARGE_FREQ_WHEEL_DEC),
                // Group 1
                new RadioPanelKnobMi8(1, Convert.ToInt32("1", 2), true, RadioPanelPZ69KnobsMi8.LOWER_R863_PRESET), // LOWER COM2
                new RadioPanelKnobMi8(1, Convert.ToInt32("10", 2), true, RadioPanelPZ69KnobsMi8.LOWER_YADRO1A), // LOWER NAV1
                new RadioPanelKnobMi8(1, Convert.ToInt32("100", 2), true, RadioPanelPZ69KnobsMi8.LOWER_R828), // LOWER NAV2
                new RadioPanelKnobMi8(1, Convert.ToInt32("1000", 2), true, RadioPanelPZ69KnobsMi8.LOWER_ADF_ARK9), // LOWER ADF
                new RadioPanelKnobMi8(1, Convert.ToInt32("10000", 2), true, RadioPanelPZ69KnobsMi8.LOWER_ARK_UD), // LOWER DME
                new RadioPanelKnobMi8(1, Convert.ToInt32("100000", 2), true, RadioPanelPZ69KnobsMi8.LOWER_SPU7), // LOWER XPDR
                new RadioPanelKnobMi8(1, Convert.ToInt32("1000000", 2), true, RadioPanelPZ69KnobsMi8.UPPER_FREQ_SWITCH),
                new RadioPanelKnobMi8(1, Convert.ToInt32("10000000", 2), true, RadioPanelPZ69KnobsMi8.LOWER_FREQ_SWITCH),
                // Group 2
                new RadioPanelKnobMi8(0, Convert.ToInt32("1", 2), true, RadioPanelPZ69KnobsMi8.UPPER_R863_MANUAL), // UPPER COM1
                new RadioPanelKnobMi8(0, Convert.ToInt32("10", 2), true, RadioPanelPZ69KnobsMi8.UPPER_R863_PRESET), // UPPER COM2
                new RadioPanelKnobMi8(0, Convert.ToInt32("100", 2), true, RadioPanelPZ69KnobsMi8.UPPER_YADRO1A), // UPPER NAV1
                new RadioPanelKnobMi8(0, Convert.ToInt32("1000", 2), true, RadioPanelPZ69KnobsMi8.UPPER_R828), // UPPER NAV2
                new RadioPanelKnobMi8(0, Convert.ToInt32("10000", 2), true, RadioPanelPZ69KnobsMi8.UPPER_ADF_ARK9), // UPPER ADF
                new RadioPanelKnobMi8(0, Convert.ToInt32("100000", 2), true, RadioPanelPZ69KnobsMi8.UPPER_ARK_UD), // UPPER DME
                new RadioPanelKnobMi8(0, Convert.ToInt32("1000000", 2), true, RadioPanelPZ69KnobsMi8.UPPER_SPU7), // UPPER XPDR
                new RadioPanelKnobMi8(0, Convert.ToInt32("10000000", 2), true, RadioPanelPZ69KnobsMi8.LOWER_R863_MANUAL) // LOWER COM1
            };

            return result;
        }
    }


}
