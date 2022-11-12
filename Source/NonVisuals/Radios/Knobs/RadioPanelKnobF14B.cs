namespace NonVisuals.Radios.Knobs
{
    using System.Collections.Generic;

    using Interfaces;


    
    public enum RadioPanelPZ69KnobsF14B
    {
        UPPER_UHF,
        UPPER_VUHF,
        UPPER_PLT_TACAN,
        UPPER_RIO_TACAN,
        UPPER_ADF,
        UPPER_LINK4,
        UPPER_XPDR,
        UPPER_SMALL_FREQ_WHEEL_INC,
        UPPER_SMALL_FREQ_WHEEL_DEC,
        UPPER_LARGE_FREQ_WHEEL_INC,
        UPPER_LARGE_FREQ_WHEEL_DEC,
        UPPER_FREQ_SWITCH,
        LOWER_UHF,
        LOWER_VUHF,
        LOWER_PLT_TACAN,
        LOWER_RIO_TACAN,
        LOWER_ADF,
        LOWER_LINK4,
        LOWER_XPDR,
        LOWER_SMALL_FREQ_WHEEL_INC,
        LOWER_SMALL_FREQ_WHEEL_DEC,
        LOWER_LARGE_FREQ_WHEEL_INC,
        LOWER_LARGE_FREQ_WHEEL_DEC,
        LOWER_FREQ_SWITCH
    }


    /*
     * Represents a knob or button on the PZ69 Radio Panel.
     * Used by the PZ69 instance to determine what knob & button
     * the user is manipulating.
     */

    public class RadioPanelKnobF14B : ISaitekPanelKnob
    {
        public RadioPanelKnobF14B(int group, int mask, bool isOn, RadioPanelPZ69KnobsF14B radioPanelPZ69Knob)
        {
            Group = group;
            Mask = mask;
            IsOn = isOn;
            RadioPanelPZ69Knob = radioPanelPZ69Knob;
        }

        public int Group { get; set; }

        public int Mask { get; set; }

        public bool IsOn { get; set; }

        public RadioPanelPZ69KnobsF14B RadioPanelPZ69Knob { get; set; }

        public static HashSet<ISaitekPanelKnob> GetRadioPanelKnobs()
        {
            // true means clockwise turn
            var result = new HashSet<ISaitekPanelKnob>
            {
                // Group 0
                new RadioPanelKnobF14B(2, 1 << 0, true, RadioPanelPZ69KnobsF14B.UPPER_SMALL_FREQ_WHEEL_INC),
                new RadioPanelKnobF14B(2, 1 << 1, false, RadioPanelPZ69KnobsF14B.UPPER_SMALL_FREQ_WHEEL_DEC),
                new RadioPanelKnobF14B(2, 1 << 2, true, RadioPanelPZ69KnobsF14B.UPPER_LARGE_FREQ_WHEEL_INC),
                new RadioPanelKnobF14B(2, 1 << 3, false, RadioPanelPZ69KnobsF14B.UPPER_LARGE_FREQ_WHEEL_DEC),
                new RadioPanelKnobF14B(2, 1 << 4, true, RadioPanelPZ69KnobsF14B.LOWER_SMALL_FREQ_WHEEL_INC),
                new RadioPanelKnobF14B(2, 1 << 5, false, RadioPanelPZ69KnobsF14B.LOWER_SMALL_FREQ_WHEEL_DEC),
                new RadioPanelKnobF14B(2, 1 << 6, true, RadioPanelPZ69KnobsF14B.LOWER_LARGE_FREQ_WHEEL_INC),
                new RadioPanelKnobF14B(2, 1 << 7, false, RadioPanelPZ69KnobsF14B.LOWER_LARGE_FREQ_WHEEL_DEC),
                // Group 1
                new RadioPanelKnobF14B(1, 1 << 0, true, RadioPanelPZ69KnobsF14B.LOWER_VUHF),
                new RadioPanelKnobF14B(1, 1 << 1, true, RadioPanelPZ69KnobsF14B.LOWER_PLT_TACAN),
                new RadioPanelKnobF14B(1, 1 << 2, true, RadioPanelPZ69KnobsF14B.LOWER_RIO_TACAN),
                new RadioPanelKnobF14B(1, 1 << 3, true, RadioPanelPZ69KnobsF14B.LOWER_ADF),
                new RadioPanelKnobF14B(1, 1 << 4, true, RadioPanelPZ69KnobsF14B.LOWER_LINK4),
                new RadioPanelKnobF14B(1, 1 << 5, true, RadioPanelPZ69KnobsF14B.LOWER_XPDR),
                new RadioPanelKnobF14B(1, 1 << 6, true, RadioPanelPZ69KnobsF14B.UPPER_FREQ_SWITCH),
                new RadioPanelKnobF14B(1, 1 << 7, true, RadioPanelPZ69KnobsF14B.LOWER_FREQ_SWITCH),
                // Group 2
                new RadioPanelKnobF14B(0, 1 << 0, true, RadioPanelPZ69KnobsF14B.UPPER_UHF), // UPPER COM 1
                new RadioPanelKnobF14B(0, 1 << 1, true, RadioPanelPZ69KnobsF14B.UPPER_VUHF), // UPPER COM 2
                new RadioPanelKnobF14B(0, 1 << 2, true, RadioPanelPZ69KnobsF14B.UPPER_PLT_TACAN), // UPPER NAV 1
                new RadioPanelKnobF14B(0, 1 << 3, true, RadioPanelPZ69KnobsF14B.UPPER_RIO_TACAN), // UPPER NAV 2
                new RadioPanelKnobF14B(0, 1 << 4, true, RadioPanelPZ69KnobsF14B.UPPER_ADF), // UPPER ADF
                new RadioPanelKnobF14B(0, 1 << 5, true, RadioPanelPZ69KnobsF14B.UPPER_LINK4), // UPPER DME
                new RadioPanelKnobF14B(0, 1 << 6, true, RadioPanelPZ69KnobsF14B.UPPER_XPDR), // UPPER XPDR
                new RadioPanelKnobF14B(0, 1 << 7, true, RadioPanelPZ69KnobsF14B.LOWER_UHF) // LOWER COM 1 
            };

            return result;
        }
    }
}
