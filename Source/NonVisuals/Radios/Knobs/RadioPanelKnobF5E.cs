namespace NonVisuals.Radios.Knobs
{
     using System.Collections.Generic;

    using NonVisuals.Interfaces;

    public enum RadioPanelPZ69KnobsF5E
    {
        UPPER_UHF,
        UPPER_NOUSE1,
        UPPER_NOUSE2,
        UPPER_NOUSE3,
        UPPER_TACAN,
        UPPER_NOUSE4,
        UPPER_NOUSE5,
        UPPER_SMALL_FREQ_WHEEL_INC,
        UPPER_SMALL_FREQ_WHEEL_DEC,
        UPPER_LARGE_FREQ_WHEEL_INC,
        UPPER_LARGE_FREQ_WHEEL_DEC,
        UPPER_FREQ_SWITCH,
        LOWER_UHF,
        LOWER_NOUSE1,
        LOWER_NOUSE2,
        LOWER_NOUSE3,
        LOWER_TACAN,
        LOWER_NOUSE4,
        LOWER_NOUSE5,
        LOWER_SMALL_FREQ_WHEEL_INC,
        LOWER_SMALL_FREQ_WHEEL_DEC,
        LOWER_LARGE_FREQ_WHEEL_INC,
        LOWER_LARGE_FREQ_WHEEL_DEC,
        LOWER_FREQ_SWITCH
    }

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
                new RadioPanelKnobF5E(2, 1 << 0, true, RadioPanelPZ69KnobsF5E.UPPER_SMALL_FREQ_WHEEL_INC),
                new RadioPanelKnobF5E(2, 1 << 1, false, RadioPanelPZ69KnobsF5E.UPPER_SMALL_FREQ_WHEEL_DEC),
                new RadioPanelKnobF5E(2, 1 << 2, true, RadioPanelPZ69KnobsF5E.UPPER_LARGE_FREQ_WHEEL_INC),
                new RadioPanelKnobF5E(2, 1 << 3, false, RadioPanelPZ69KnobsF5E.UPPER_LARGE_FREQ_WHEEL_DEC),
                new RadioPanelKnobF5E(2, 1 << 4, true, RadioPanelPZ69KnobsF5E.LOWER_SMALL_FREQ_WHEEL_INC),
                new RadioPanelKnobF5E(2, 1 << 5, false, RadioPanelPZ69KnobsF5E.LOWER_SMALL_FREQ_WHEEL_DEC),
                new RadioPanelKnobF5E(2, 1 << 6, true, RadioPanelPZ69KnobsF5E.LOWER_LARGE_FREQ_WHEEL_INC),
                new RadioPanelKnobF5E(2, 1 << 7, false, RadioPanelPZ69KnobsF5E.LOWER_LARGE_FREQ_WHEEL_DEC),
                // Group 1
                new RadioPanelKnobF5E(1, 1 << 0, true, RadioPanelPZ69KnobsF5E.LOWER_NOUSE1), // LOWER COM 2
                new RadioPanelKnobF5E(1, 1 << 1, true, RadioPanelPZ69KnobsF5E.LOWER_NOUSE2), // LOWER NAV 1
                new RadioPanelKnobF5E(1, 1 << 2, true, RadioPanelPZ69KnobsF5E.LOWER_NOUSE3), // LOWER NAV 2
                new RadioPanelKnobF5E(1, 1 << 3, true, RadioPanelPZ69KnobsF5E.LOWER_TACAN), // LOWER ADF
                new RadioPanelKnobF5E(1, 1 << 4, true, RadioPanelPZ69KnobsF5E.LOWER_NOUSE4), // LOWER DME
                new RadioPanelKnobF5E(1, 1 << 5, true, RadioPanelPZ69KnobsF5E.LOWER_NOUSE5), // LOWER XPDR
                new RadioPanelKnobF5E(1, 1 << 6, true, RadioPanelPZ69KnobsF5E.UPPER_FREQ_SWITCH),
                new RadioPanelKnobF5E(1, 1 << 7, true, RadioPanelPZ69KnobsF5E.LOWER_FREQ_SWITCH),
                // Group 2
                new RadioPanelKnobF5E(0, 1 << 0, true, RadioPanelPZ69KnobsF5E.UPPER_UHF), // UPPER COM 1
                new RadioPanelKnobF5E(0, 1 << 1, true, RadioPanelPZ69KnobsF5E.UPPER_NOUSE1), // UPPER COM 2
                new RadioPanelKnobF5E(0, 1 << 2, true, RadioPanelPZ69KnobsF5E.UPPER_NOUSE2), // UPPER NAV 1
                new RadioPanelKnobF5E(0, 1 << 3, true, RadioPanelPZ69KnobsF5E.UPPER_NOUSE3), // UPPER NAV 2
                new RadioPanelKnobF5E(0, 1 << 4, true, RadioPanelPZ69KnobsF5E.UPPER_TACAN), // UPPER ADF
                new RadioPanelKnobF5E(0, 1 << 5, true, RadioPanelPZ69KnobsF5E.UPPER_NOUSE4), // UPPER DME
                new RadioPanelKnobF5E(0, 1 << 6, true, RadioPanelPZ69KnobsF5E.UPPER_NOUSE5), // UPPER XPDR
                new RadioPanelKnobF5E(0, 1 << 7, true, RadioPanelPZ69KnobsF5E.LOWER_UHF) // LOWER COM 1 
            };

            return result;
        }
    }
}
