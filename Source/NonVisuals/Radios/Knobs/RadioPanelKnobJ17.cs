namespace NonVisuals.Radios.Knobs
{
    using System.Collections.Generic;

    using Interfaces;

    public enum RadioPanelKnobsJ17
    {
        UPPER_COM1,
        UPPER_COM2,
        UPPER_NAV1,
        UPPER_NAV2,
        UPPER_ADF,
        UPPER_DME,
        UPPER_XPDR,
        UPPER_SMALL_FREQ_WHEEL_INC,
        UPPER_SMALL_FREQ_WHEEL_DEC,
        UPPER_LARGE_FREQ_WHEEL_INC,
        UPPER_LARGE_FREQ_WHEEL_DEC,
        UPPER_FREQ_SWITCH,
        LOWER_COM1,
        LOWER_COM2,
        LOWER_NAV1,
        LOWER_NAV2,
        LOWER_ADF,
        LOWER_DME,
        LOWER_XPDR,
        LOWER_SMALL_FREQ_WHEEL_INC,
        LOWER_SMALL_FREQ_WHEEL_DEC,
        LOWER_LARGE_FREQ_WHEEL_INC,
        LOWER_LARGE_FREQ_WHEEL_DEC,
        LOWER_FREQ_SWITCH
    }

    
    /// <summary>
    /// Represents a knob or button on the PZ69 Radio Panel. Used by the PZ69 instance to determine what knob & button the user is manipulating.
    /// </summary>
    public class RadioPanelKnobJ17 : ISaitekPanelKnob
    {
        public RadioPanelKnobJ17(int group, int mask, bool isOn, RadioPanelKnobsJ17 radioPanelPZ69Knob)
        {
            Group = group;
            Mask = mask;
            IsOn = isOn;
            RadioPanelPZ69Knob = radioPanelPZ69Knob;
        }

        public int Group { get; set; }

        public int Mask { get; set; }

        public bool IsOn { get; set; }

        public RadioPanelKnobsJ17 RadioPanelPZ69Knob { get; set; }

        public static HashSet<ISaitekPanelKnob> GetRadioPanelKnobs()
        {
            // true means clockwise turn
            var result = new HashSet<ISaitekPanelKnob>
            {
                // Group 0
                new RadioPanelKnobJ17(2, 1 << 0, true, RadioPanelKnobsJ17.UPPER_SMALL_FREQ_WHEEL_INC),
                new RadioPanelKnobJ17(2, 1 << 1, false, RadioPanelKnobsJ17.UPPER_SMALL_FREQ_WHEEL_DEC),
                new RadioPanelKnobJ17(2, 1 << 2, true, RadioPanelKnobsJ17.UPPER_LARGE_FREQ_WHEEL_INC),
                new RadioPanelKnobJ17(2, 1 << 3, false, RadioPanelKnobsJ17.UPPER_LARGE_FREQ_WHEEL_DEC),
                new RadioPanelKnobJ17(2, 1 << 4, true, RadioPanelKnobsJ17.LOWER_SMALL_FREQ_WHEEL_INC),
                new RadioPanelKnobJ17(2, 1 << 5, false, RadioPanelKnobsJ17.LOWER_SMALL_FREQ_WHEEL_DEC),
                new RadioPanelKnobJ17(2, 1 << 6, true, RadioPanelKnobsJ17.LOWER_LARGE_FREQ_WHEEL_INC),
                new RadioPanelKnobJ17(2, 1 << 7, false, RadioPanelKnobsJ17.LOWER_LARGE_FREQ_WHEEL_DEC),
                // Group 1
                new RadioPanelKnobJ17(1, 1 << 0, true, RadioPanelKnobsJ17.LOWER_COM2),// LOWER COM 2
                new RadioPanelKnobJ17(1, 1 << 1, true, RadioPanelKnobsJ17.LOWER_NAV1),// LOWER NAV 1
                new RadioPanelKnobJ17(1, 1 << 2, true, RadioPanelKnobsJ17.LOWER_NAV2),// LOWER NAV 2
                new RadioPanelKnobJ17(1, 1 << 3, true, RadioPanelKnobsJ17.LOWER_ADF),// LOWER ADF
                new RadioPanelKnobJ17(1, 1 << 4, true, RadioPanelKnobsJ17.LOWER_DME),// LOWER DME
                new RadioPanelKnobJ17(1, 1 << 5, true, RadioPanelKnobsJ17.LOWER_XPDR),// LOWER XPDR
                new RadioPanelKnobJ17(1, 1 << 6, true, RadioPanelKnobsJ17.UPPER_FREQ_SWITCH),
                new RadioPanelKnobJ17(1, 1 << 7, true, RadioPanelKnobsJ17.LOWER_FREQ_SWITCH),
                // Group 2
                new RadioPanelKnobJ17(0, 1 << 0, true, RadioPanelKnobsJ17.UPPER_COM1), // UPPER COM 1
                new RadioPanelKnobJ17(0, 1 << 1, true, RadioPanelKnobsJ17.UPPER_COM2), // UPPER COM 2
                new RadioPanelKnobJ17(0, 1 << 2, true, RadioPanelKnobsJ17.UPPER_NAV1), // UPPER NAV 1
                new RadioPanelKnobJ17(0, 1 << 3, true, RadioPanelKnobsJ17.UPPER_NAV2), // UPPER NAV 2
                new RadioPanelKnobJ17(0, 1 << 4, true, RadioPanelKnobsJ17.UPPER_ADF), // UPPER ADF
                new RadioPanelKnobJ17(0, 1 << 5, true, RadioPanelKnobsJ17.UPPER_DME), // UPPER DME
                new RadioPanelKnobJ17(0, 1 << 6, true, RadioPanelKnobsJ17.UPPER_XPDR), // UPPER XPDR
                new RadioPanelKnobJ17(0, 1 << 7, true, RadioPanelKnobsJ17.LOWER_COM1) // LOWER COM 1 
            };

            return result;
        }
    }
}
