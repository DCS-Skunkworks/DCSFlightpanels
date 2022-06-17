namespace NonVisuals.Radios.Knobs
{
    using System;
    using System.Collections.Generic;

    using MEF;

    using NonVisuals.Interfaces;

    public class RadioPanelKnobKa50 : ISaitekPanelKnob
    {
        public RadioPanelKnobKa50(int group, int mask, bool isOn, RadioPanelPZ69KnobsKa50 radioPanelPZ69Knob)
        {
            Group = group;
            Mask = mask;
            IsOn = isOn;
            RadioPanelPZ69Knob = radioPanelPZ69Knob;
        }

        public int Group { get; set; }

        public int Mask { get; set; }

        public bool IsOn { get; set; }

        public RadioPanelPZ69KnobsKa50 RadioPanelPZ69Knob { get; set; }
        
        public static HashSet<ISaitekPanelKnob> GetRadioPanelKnobs()
        {
            // true means clockwise turn
            var result = new HashSet<ISaitekPanelKnob>
            {
                // Group 0
                new RadioPanelKnobKa50(2, 1 << 0, true, RadioPanelPZ69KnobsKa50.UPPER_SMALL_FREQ_WHEEL_INC),
                new RadioPanelKnobKa50(2, 1 << 1, false, RadioPanelPZ69KnobsKa50.UPPER_SMALL_FREQ_WHEEL_DEC),
                new RadioPanelKnobKa50(2, 1 << 2, true, RadioPanelPZ69KnobsKa50.UPPER_LARGE_FREQ_WHEEL_INC),
                new RadioPanelKnobKa50(2, 1 << 3, false, RadioPanelPZ69KnobsKa50.UPPER_LARGE_FREQ_WHEEL_DEC),
                new RadioPanelKnobKa50(2, 1 << 4, true, RadioPanelPZ69KnobsKa50.LOWER_SMALL_FREQ_WHEEL_INC),
                new RadioPanelKnobKa50(2, 1 << 5, false, RadioPanelPZ69KnobsKa50.LOWER_SMALL_FREQ_WHEEL_DEC),
                new RadioPanelKnobKa50(2, 1 << 6, true, RadioPanelPZ69KnobsKa50.LOWER_LARGE_FREQ_WHEEL_INC),
                new RadioPanelKnobKa50(2, 1 << 7, false, RadioPanelPZ69KnobsKa50.LOWER_LARGE_FREQ_WHEEL_DEC),
                // Group 1
                new RadioPanelKnobKa50(1, 1 << 0, true, RadioPanelPZ69KnobsKa50.LOWER_VHF2_R800L1), // LOWER COM2
                new RadioPanelKnobKa50(1, 1 << 1, true, RadioPanelPZ69KnobsKa50.LOWER_ABRIS), // LOWER NAV1
                new RadioPanelKnobKa50(1, 1 << 2, true, RadioPanelPZ69KnobsKa50.LOWER_DATALINK), // LOWER NAV2
                new RadioPanelKnobKa50(1, 1 << 3, true, RadioPanelPZ69KnobsKa50.LOWER_ADF_ARK22), // LOWER ADF
                new RadioPanelKnobKa50(1, 1 << 4, true, RadioPanelPZ69KnobsKa50.LOWER_NO_USE3), // LOWER DME
                new RadioPanelKnobKa50(1, 1 << 5, true, RadioPanelPZ69KnobsKa50.LOWER_NO_USE4), // LOWER XPDR
                new RadioPanelKnobKa50(1, 1 << 6, true, RadioPanelPZ69KnobsKa50.UPPER_FREQ_SWITCH),
                new RadioPanelKnobKa50(1, 1 << 7, true, RadioPanelPZ69KnobsKa50.LOWER_FREQ_SWITCH),
                // Group 2
                new RadioPanelKnobKa50(0, 1 << 0, true, RadioPanelPZ69KnobsKa50.UPPER_VHF1_R828), // UPPER COM1
                new RadioPanelKnobKa50(0, 1 << 1, true, RadioPanelPZ69KnobsKa50.UPPER_VHF2_R800L1), // UPPER COM2
                new RadioPanelKnobKa50(0, 1 << 2, true, RadioPanelPZ69KnobsKa50.UPPER_ABRIS), // UPPER NAV1
                new RadioPanelKnobKa50(0, 1 << 3, true, RadioPanelPZ69KnobsKa50.UPPER_DATALINK), // UPPER NAV2
                new RadioPanelKnobKa50(0, 1 << 4, true, RadioPanelPZ69KnobsKa50.UPPER_ADF_ARK22), // UPPER ADF
                new RadioPanelKnobKa50(0, 1 << 5, true, RadioPanelPZ69KnobsKa50.UPPER_NO_USE3), // UPPER DME
                new RadioPanelKnobKa50(0, 1 << 6, true, RadioPanelPZ69KnobsKa50.UPPER_NO_USE4), // UPPER XPDR
                new RadioPanelKnobKa50(0, 1 << 7, true, RadioPanelPZ69KnobsKa50.LOWER_VHF1_R828) // LOWER COM1
            };

            return result;
        }
    }


}
