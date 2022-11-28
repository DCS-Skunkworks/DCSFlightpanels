namespace NonVisuals.Radios.Knobs
{
    using System.Collections.Generic;

    using Interfaces;

    public enum RadioPanelPZ69KnobsUH1H
    {
        UPPER_VHFCOMM,
        UPPER_UHF,
        UPPER_VHFNAV,
        UPPER_VHFFM,
        UPPER_ADF,
        UPPER_DME,
        UPPER_INTERCOMM,
        UPPER_SMALL_FREQ_WHEEL_INC,
        UPPER_SMALL_FREQ_WHEEL_DEC,
        UPPER_LARGE_FREQ_WHEEL_INC,
        UPPER_LARGE_FREQ_WHEEL_DEC,
        UPPER_FREQ_SWITCH,
        LOWER_VHFCOMM,
        LOWER_UHF,
        LOWER_VHFNAV,
        LOWER_VHFFM,
        LOWER_ADF,
        LOWER_DME,
        LOWER_INTERCOMM,
        LOWER_SMALL_FREQ_WHEEL_INC,
        LOWER_SMALL_FREQ_WHEEL_DEC,
        LOWER_LARGE_FREQ_WHEEL_INC,
        LOWER_LARGE_FREQ_WHEEL_DEC,
        LOWER_FREQ_SWITCH
    }

    
    /// <summary>
    /// Represents a knob or button on the PZ69 Radio Panel. Used by the PZ69 instance to determine what knob & button the user is manipulating.
    /// </summary>
    public class RadioPanelKnobUH1H : ISaitekPanelKnob
    {
        public RadioPanelKnobUH1H(int group, int mask, bool isOn, RadioPanelPZ69KnobsUH1H radioPanelPZ69Knob)
        {
            Group = group;
            Mask = mask;
            IsOn = isOn;
            RadioPanelPZ69Knob = radioPanelPZ69Knob;
        }

        public int Group { get; set; }
        public int Mask { get; set; }
        public bool IsOn { get; set; }
        public RadioPanelPZ69KnobsUH1H RadioPanelPZ69Knob { get; set; }
        
        public static HashSet<ISaitekPanelKnob> GetRadioPanelKnobs()
        {
            // true means clockwise turn
            return new HashSet<ISaitekPanelKnob>
            {
                // Group 0
                new RadioPanelKnobUH1H(2, 1 << 0, true, RadioPanelPZ69KnobsUH1H.UPPER_SMALL_FREQ_WHEEL_INC),
                new RadioPanelKnobUH1H(2, 1 << 1, false, RadioPanelPZ69KnobsUH1H.UPPER_SMALL_FREQ_WHEEL_DEC),
                new RadioPanelKnobUH1H(2, 1 << 2, true, RadioPanelPZ69KnobsUH1H.UPPER_LARGE_FREQ_WHEEL_INC),
                new RadioPanelKnobUH1H(2, 1 << 3, false, RadioPanelPZ69KnobsUH1H.UPPER_LARGE_FREQ_WHEEL_DEC),
                new RadioPanelKnobUH1H(2, 1 << 4, true, RadioPanelPZ69KnobsUH1H.LOWER_SMALL_FREQ_WHEEL_INC),
                new RadioPanelKnobUH1H(2, 1 << 5, false, RadioPanelPZ69KnobsUH1H.LOWER_SMALL_FREQ_WHEEL_DEC),
                new RadioPanelKnobUH1H(2, 1 << 6, true, RadioPanelPZ69KnobsUH1H.LOWER_LARGE_FREQ_WHEEL_INC),
                new RadioPanelKnobUH1H(2, 1 << 7, false, RadioPanelPZ69KnobsUH1H.LOWER_LARGE_FREQ_WHEEL_DEC),
                // Group 1
                new RadioPanelKnobUH1H(1, 1 << 0, true, RadioPanelPZ69KnobsUH1H.LOWER_UHF),
                new RadioPanelKnobUH1H(1, 1 << 1, true, RadioPanelPZ69KnobsUH1H.LOWER_VHFNAV),
                new RadioPanelKnobUH1H(1, 1 << 2, true, RadioPanelPZ69KnobsUH1H.LOWER_VHFFM),
                new RadioPanelKnobUH1H(1, 1 << 3, true, RadioPanelPZ69KnobsUH1H.LOWER_ADF),
                new RadioPanelKnobUH1H(1, 1 << 4, true, RadioPanelPZ69KnobsUH1H.LOWER_DME),
                new RadioPanelKnobUH1H(1, 1 << 5, true, RadioPanelPZ69KnobsUH1H.LOWER_INTERCOMM),
                new RadioPanelKnobUH1H(1, 1 << 6, true, RadioPanelPZ69KnobsUH1H.UPPER_FREQ_SWITCH),
                new RadioPanelKnobUH1H(1, 1 << 7, true, RadioPanelPZ69KnobsUH1H.LOWER_FREQ_SWITCH),
                // Group 2
                new RadioPanelKnobUH1H(0, 1 << 0, true, RadioPanelPZ69KnobsUH1H.UPPER_VHFCOMM),
                new RadioPanelKnobUH1H(0, 1 << 1, true, RadioPanelPZ69KnobsUH1H.UPPER_UHF),
                new RadioPanelKnobUH1H(0, 1 << 2, true, RadioPanelPZ69KnobsUH1H.UPPER_VHFNAV),
                new RadioPanelKnobUH1H(0, 1 << 3, true, RadioPanelPZ69KnobsUH1H.UPPER_VHFFM),
                new RadioPanelKnobUH1H(0, 1 << 4, true, RadioPanelPZ69KnobsUH1H.UPPER_ADF),
                new RadioPanelKnobUH1H(0, 1 << 5, true, RadioPanelPZ69KnobsUH1H.UPPER_DME),
                new RadioPanelKnobUH1H(0, 1 << 6, true, RadioPanelPZ69KnobsUH1H.UPPER_INTERCOMM),
                new RadioPanelKnobUH1H(0, 1 << 7, true, RadioPanelPZ69KnobsUH1H.LOWER_VHFCOMM)
            };
        }
    }
}
