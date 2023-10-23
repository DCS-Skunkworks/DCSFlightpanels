namespace NonVisuals.Radios.Knobs
{
    using System.Collections.Generic;

    using Interfaces;

    public enum RadioPanelKnobsC101
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
    public class RadioPanelKnobC101 : ISaitekPanelKnob
    {
        public RadioPanelKnobC101(int group, int mask, bool isOn, RadioPanelKnobsC101 radioPanelPZ69Knob)
        {
            Group = group;
            Mask = mask;
            IsOn = isOn;
            RadioPanelPZ69Knob = radioPanelPZ69Knob;
        }

        public int Group { get; set; }

        public int Mask { get; set; }

        public bool IsOn { get; set; }

        public RadioPanelKnobsC101 RadioPanelPZ69Knob { get; set; }

        public static HashSet<ISaitekPanelKnob> GetRadioPanelKnobs()
        {
            // true means clockwise turn
            var result = new HashSet<ISaitekPanelKnob>
            {
                // Group 0
                new RadioPanelKnobC101(2, 1 << 0, true, RadioPanelKnobsC101.UPPER_SMALL_FREQ_WHEEL_INC),
                new RadioPanelKnobC101(2, 1 << 1, false, RadioPanelKnobsC101.UPPER_SMALL_FREQ_WHEEL_DEC),
                new RadioPanelKnobC101(2, 1 << 2, true, RadioPanelKnobsC101.UPPER_LARGE_FREQ_WHEEL_INC),
                new RadioPanelKnobC101(2, 1 << 3, false, RadioPanelKnobsC101.UPPER_LARGE_FREQ_WHEEL_DEC),
                new RadioPanelKnobC101(2, 1 << 4, true, RadioPanelKnobsC101.LOWER_SMALL_FREQ_WHEEL_INC),
                new RadioPanelKnobC101(2, 1 << 5, false, RadioPanelKnobsC101.LOWER_SMALL_FREQ_WHEEL_DEC),
                new RadioPanelKnobC101(2, 1 << 6, true, RadioPanelKnobsC101.LOWER_LARGE_FREQ_WHEEL_INC),
                new RadioPanelKnobC101(2, 1 << 7, false, RadioPanelKnobsC101.LOWER_LARGE_FREQ_WHEEL_DEC),
                // Group 1
                new RadioPanelKnobC101(1, 1 << 0, true, RadioPanelKnobsC101.LOWER_UHF),// LOWER COM 2
                new RadioPanelKnobC101(1, 1 << 1, true, RadioPanelKnobsC101.LOWER_NAV1),// LOWER NAV 1
                new RadioPanelKnobC101(1, 1 << 2, true, RadioPanelKnobsC101.LOWER_NAV2),// LOWER NAV 2
                new RadioPanelKnobC101(1, 1 << 3, true, RadioPanelKnobsC101.LOWER_ADF),// LOWER ADF
                new RadioPanelKnobC101(1, 1 << 4, true, RadioPanelKnobsC101.LOWER_DME),// LOWER DME
                new RadioPanelKnobC101(1, 1 << 5, true, RadioPanelKnobsC101.LOWER_XPDR),// LOWER XPDR
                new RadioPanelKnobC101(1, 1 << 6, true, RadioPanelKnobsC101.UPPER_FREQ_SWITCH),
                new RadioPanelKnobC101(1, 1 << 7, true, RadioPanelKnobsC101.LOWER_FREQ_SWITCH),
                // Group 2
                new RadioPanelKnobC101(0, 1 << 0, true, RadioPanelKnobsC101.UPPER_VHF), // UPPER COM 1
                new RadioPanelKnobC101(0, 1 << 1, true, RadioPanelKnobsC101.UPPER_UHF), // UPPER COM 2
                new RadioPanelKnobC101(0, 1 << 2, true, RadioPanelKnobsC101.UPPER_NAV1), // UPPER NAV 1
                new RadioPanelKnobC101(0, 1 << 3, true, RadioPanelKnobsC101.UPPER_NAV2), // UPPER NAV 2
                new RadioPanelKnobC101(0, 1 << 4, true, RadioPanelKnobsC101.UPPER_ADF), // UPPER ADF
                new RadioPanelKnobC101(0, 1 << 5, true, RadioPanelKnobsC101.UPPER_DME), // UPPER DME
                new RadioPanelKnobC101(0, 1 << 6, true, RadioPanelKnobsC101.UPPER_XPDR), // UPPER XPDR
                new RadioPanelKnobC101(0, 1 << 7, true, RadioPanelKnobsC101.LOWER_VHF) // LOWER COM 1 
            };

            return result;
        }
    }
}
