namespace NonVisuals.Radios.Knobs
{
    using System.Collections.Generic;

    using Interfaces;

    public enum RadioPanelKnobsF15E
    {
        UPPER_VHF,
        UPPER_UHF,
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
        LOWER_VHF,
        LOWER_UHF,
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
    public class RadioPanelKnobF15E : ISaitekPanelKnob
    {
        public RadioPanelKnobF15E(int group, int mask, bool isOn, RadioPanelKnobsF15E radioPanelPZ69Knob)
        {
            Group = group;
            Mask = mask;
            IsOn = isOn;
            RadioPanelPZ69Knob = radioPanelPZ69Knob;
        }

        public int Group { get; set; }

        public int Mask { get; set; }

        public bool IsOn { get; set; }

        public RadioPanelKnobsF15E RadioPanelPZ69Knob { get; set; }

        public static HashSet<ISaitekPanelKnob> GetRadioPanelKnobs()
        {
            // true means clockwise turn
            var result = new HashSet<ISaitekPanelKnob>
            {
                // Group 0
                new RadioPanelKnobF15E(2, 1 << 0, true, RadioPanelKnobsF15E.UPPER_SMALL_FREQ_WHEEL_INC),
                new RadioPanelKnobF15E(2, 1 << 1, false, RadioPanelKnobsF15E.UPPER_SMALL_FREQ_WHEEL_DEC),
                new RadioPanelKnobF15E(2, 1 << 2, true, RadioPanelKnobsF15E.UPPER_LARGE_FREQ_WHEEL_INC),
                new RadioPanelKnobF15E(2, 1 << 3, false, RadioPanelKnobsF15E.UPPER_LARGE_FREQ_WHEEL_DEC),
                new RadioPanelKnobF15E(2, 1 << 4, true, RadioPanelKnobsF15E.LOWER_SMALL_FREQ_WHEEL_INC),
                new RadioPanelKnobF15E(2, 1 << 5, false, RadioPanelKnobsF15E.LOWER_SMALL_FREQ_WHEEL_DEC),
                new RadioPanelKnobF15E(2, 1 << 6, true, RadioPanelKnobsF15E.LOWER_LARGE_FREQ_WHEEL_INC),
                new RadioPanelKnobF15E(2, 1 << 7, false, RadioPanelKnobsF15E.LOWER_LARGE_FREQ_WHEEL_DEC),
                // Group 1
                new RadioPanelKnobF15E(1, 1 << 0, true, RadioPanelKnobsF15E.LOWER_UHF),// LOWER COM 2
                new RadioPanelKnobF15E(1, 1 << 1, true, RadioPanelKnobsF15E.LOWER_NAV1),// LOWER NAV 1
                new RadioPanelKnobF15E(1, 1 << 2, true, RadioPanelKnobsF15E.LOWER_NAV2),// LOWER NAV 2
                new RadioPanelKnobF15E(1, 1 << 3, true, RadioPanelKnobsF15E.LOWER_ADF),// LOWER ADF
                new RadioPanelKnobF15E(1, 1 << 4, true, RadioPanelKnobsF15E.LOWER_DME),// LOWER DME
                new RadioPanelKnobF15E(1, 1 << 5, true, RadioPanelKnobsF15E.LOWER_XPDR),// LOWER XPDR
                new RadioPanelKnobF15E(1, 1 << 6, true, RadioPanelKnobsF15E.UPPER_FREQ_SWITCH),
                new RadioPanelKnobF15E(1, 1 << 7, true, RadioPanelKnobsF15E.LOWER_FREQ_SWITCH),
                // Group 2
                new RadioPanelKnobF15E(0, 1 << 0, true, RadioPanelKnobsF15E.UPPER_VHF), // UPPER COM 1
                new RadioPanelKnobF15E(0, 1 << 1, true, RadioPanelKnobsF15E.UPPER_UHF), // UPPER COM 2
                new RadioPanelKnobF15E(0, 1 << 2, true, RadioPanelKnobsF15E.UPPER_NAV1), // UPPER NAV 1
                new RadioPanelKnobF15E(0, 1 << 3, true, RadioPanelKnobsF15E.UPPER_NAV2), // UPPER NAV 2
                new RadioPanelKnobF15E(0, 1 << 4, true, RadioPanelKnobsF15E.UPPER_ADF), // UPPER ADF
                new RadioPanelKnobF15E(0, 1 << 5, true, RadioPanelKnobsF15E.UPPER_DME), // UPPER DME
                new RadioPanelKnobF15E(0, 1 << 6, true, RadioPanelKnobsF15E.UPPER_XPDR), // UPPER XPDR
                new RadioPanelKnobF15E(0, 1 << 7, true, RadioPanelKnobsF15E.LOWER_VHF) // LOWER COM 1 
            };

            return result;
        }
    }
}
