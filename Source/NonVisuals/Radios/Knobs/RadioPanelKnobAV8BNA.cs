namespace NonVisuals.Radios.Knobs
{
    using System.Collections.Generic;

    using NonVisuals.Interfaces;

    public enum RadioPanelPZ69KnobsAV8BNA
    {
        UPPER_COMM1,
        UPPER_COMM2,
        UPPER_NAV1,
        UPPER_NAV2,
        UPPER_ADF,
        UPPER_DME,
        UPPER_XPDR,
        UPPER_SMALL_FREQ_WHEEL_INC ,
        UPPER_SMALL_FREQ_WHEEL_DEC,
        UPPER_LARGE_FREQ_WHEEL_INC,
        UPPER_LARGE_FREQ_WHEEL_DEC,
        UPPER_FREQ_SWITCH,
        LOWER_COMM1,
        LOWER_COMM2,
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

    public class RadioPanelKnobAV8BNA : ISaitekPanelKnob
    {
        public RadioPanelKnobAV8BNA(int group, int mask, bool isOn, RadioPanelPZ69KnobsAV8BNA radioPanelPZ69Knob)
        {
            Group = group;
            Mask = mask;
            IsOn = isOn;
            RadioPanelPZ69Knob = radioPanelPZ69Knob;
        }

        public int Group { get; set; }

        public int Mask { get; set; }

        public bool IsOn { get; set; }

        public RadioPanelPZ69KnobsAV8BNA RadioPanelPZ69Knob { get; set; }

        public static HashSet<ISaitekPanelKnob> GetRadioPanelKnobs()
        {
            // true means clockwise turn
            var result = new HashSet<ISaitekPanelKnob>
            {
                // Group 0
                new RadioPanelKnobAV8BNA(2, 1 << 0, true, RadioPanelPZ69KnobsAV8BNA.UPPER_SMALL_FREQ_WHEEL_INC),
                new RadioPanelKnobAV8BNA(2, 1 << 1, false, RadioPanelPZ69KnobsAV8BNA.UPPER_SMALL_FREQ_WHEEL_DEC),
                new RadioPanelKnobAV8BNA(2, 1 << 2, true, RadioPanelPZ69KnobsAV8BNA.UPPER_LARGE_FREQ_WHEEL_INC),
                new RadioPanelKnobAV8BNA(2, 1 << 3, false, RadioPanelPZ69KnobsAV8BNA.UPPER_LARGE_FREQ_WHEEL_DEC),
                new RadioPanelKnobAV8BNA(2, 1 << 4, true, RadioPanelPZ69KnobsAV8BNA.LOWER_SMALL_FREQ_WHEEL_INC),
                new RadioPanelKnobAV8BNA(2, 1 << 5, false, RadioPanelPZ69KnobsAV8BNA.LOWER_SMALL_FREQ_WHEEL_DEC),
                new RadioPanelKnobAV8BNA(2, 1 << 6, true, RadioPanelPZ69KnobsAV8BNA.LOWER_LARGE_FREQ_WHEEL_INC),
                new RadioPanelKnobAV8BNA(2, 1 << 7, false, RadioPanelPZ69KnobsAV8BNA.LOWER_LARGE_FREQ_WHEEL_DEC),
                // Group 1
                new RadioPanelKnobAV8BNA(1, 1 << 0, true, RadioPanelPZ69KnobsAV8BNA.LOWER_COMM2),
                new RadioPanelKnobAV8BNA(1, 1 << 1, true, RadioPanelPZ69KnobsAV8BNA.LOWER_NAV1),
                new RadioPanelKnobAV8BNA(1, 1 << 2, true, RadioPanelPZ69KnobsAV8BNA.LOWER_NAV2),
                new RadioPanelKnobAV8BNA(1, 1 << 3, true, RadioPanelPZ69KnobsAV8BNA.LOWER_ADF),
                new RadioPanelKnobAV8BNA(1, 1 << 4, true, RadioPanelPZ69KnobsAV8BNA.LOWER_DME),
                new RadioPanelKnobAV8BNA(1, 1 << 5, true, RadioPanelPZ69KnobsAV8BNA.LOWER_XPDR),
                new RadioPanelKnobAV8BNA(1, 1 << 6, true, RadioPanelPZ69KnobsAV8BNA.UPPER_FREQ_SWITCH),
                new RadioPanelKnobAV8BNA(1, 1 << 7, true, RadioPanelPZ69KnobsAV8BNA.LOWER_FREQ_SWITCH),
                // Group 2
                new RadioPanelKnobAV8BNA(0, 1 << 0, true, RadioPanelPZ69KnobsAV8BNA.UPPER_COMM1), // UPPER COM 1
                new RadioPanelKnobAV8BNA(0, 1 << 1, true, RadioPanelPZ69KnobsAV8BNA.UPPER_COMM2), // UPPER COM 2
                new RadioPanelKnobAV8BNA(0, 1 << 2, true, RadioPanelPZ69KnobsAV8BNA.UPPER_NAV1), // UPPER NAV 1
                new RadioPanelKnobAV8BNA(0, 1 << 3, true, RadioPanelPZ69KnobsAV8BNA.UPPER_NAV2), // UPPER NAV 2
                new RadioPanelKnobAV8BNA(0, 1 << 4, true, RadioPanelPZ69KnobsAV8BNA.UPPER_ADF), // UPPER ADF
                new RadioPanelKnobAV8BNA(0, 1 << 5, true, RadioPanelPZ69KnobsAV8BNA.UPPER_DME), // UPPER DME
                new RadioPanelKnobAV8BNA(0, 1 << 6, true, RadioPanelPZ69KnobsAV8BNA.UPPER_XPDR), // UPPER XPDR
                new RadioPanelKnobAV8BNA(0, 1 << 7, true, RadioPanelPZ69KnobsAV8BNA.LOWER_COMM1) // LOWER COM 1 
            };

            return result;
        }
    }
}
