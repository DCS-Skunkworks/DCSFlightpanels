namespace NonVisuals.Radios.Knobs
{
    using System.Collections.Generic;

    using Interfaces;


    
    public enum RadioPanelPZ69KnobsT45C
    {
        UPPER_VUHF1,
        UPPER_VUHF2,
        UPPER_TACAN,
        UPPER_VOR,
        UPPER_NO_USE1,
        UPPER_NO_USE2,
        UPPER_NO_USE3,
        UPPER_SMALL_FREQ_WHEEL_INC,
        UPPER_SMALL_FREQ_WHEEL_DEC,
        UPPER_LARGE_FREQ_WHEEL_INC,
        UPPER_LARGE_FREQ_WHEEL_DEC,
        UPPER_FREQ_SWITCH,
        LOWER_VUHF1,
        LOWER_VUHF2,
        LOWER_TACAN,
        LOWER_VOR,
        LOWER_NO_USE1,
        LOWER_NO_USE2,
        LOWER_NO_USE3,
        LOWER_SMALL_FREQ_WHEEL_INC,
        LOWER_SMALL_FREQ_WHEEL_DEC,
        LOWER_LARGE_FREQ_WHEEL_INC,
        LOWER_LARGE_FREQ_WHEEL_DEC,
        LOWER_FREQ_SWITCH
    }

    
    /// <summary>
    /// Represents a knob or button on the PZ69 Radio Panel. Used by the PZ69 instance to determine what knob & button the user is manipulating.
    /// </summary>
    public class RadioPanelKnobT45C : ISaitekPanelKnob
    {
        public RadioPanelKnobT45C(int group, int mask, bool isOn, RadioPanelPZ69KnobsT45C radioPanelPZ69Knob)
        {
            Group = group;
            Mask = mask;
            IsOn = isOn;
            RadioPanelPZ69Knob = radioPanelPZ69Knob;
        }

        public int Group { get; set; }

        public int Mask { get; set; }

        public bool IsOn { get; set; }

        public RadioPanelPZ69KnobsT45C RadioPanelPZ69Knob { get; set; }

        public static HashSet<ISaitekPanelKnob> GetRadioPanelKnobs()
        {
            // true means clockwise turn
            var result = new HashSet<ISaitekPanelKnob>
            {
                // Group 0
                new RadioPanelKnobT45C(2, 1 << 0, true, RadioPanelPZ69KnobsT45C.UPPER_SMALL_FREQ_WHEEL_INC),
                new RadioPanelKnobT45C(2, 1 << 1, false, RadioPanelPZ69KnobsT45C.UPPER_SMALL_FREQ_WHEEL_DEC),
                new RadioPanelKnobT45C(2, 1 << 2, true, RadioPanelPZ69KnobsT45C.UPPER_LARGE_FREQ_WHEEL_INC),
                new RadioPanelKnobT45C(2, 1 << 3, false, RadioPanelPZ69KnobsT45C.UPPER_LARGE_FREQ_WHEEL_DEC),
                new RadioPanelKnobT45C(2, 1 << 4, true, RadioPanelPZ69KnobsT45C.LOWER_SMALL_FREQ_WHEEL_INC),
                new RadioPanelKnobT45C(2, 1 << 5, false, RadioPanelPZ69KnobsT45C.LOWER_SMALL_FREQ_WHEEL_DEC),
                new RadioPanelKnobT45C(2, 1 << 6, true, RadioPanelPZ69KnobsT45C.LOWER_LARGE_FREQ_WHEEL_INC),
                new RadioPanelKnobT45C(2, 1 << 7, false, RadioPanelPZ69KnobsT45C.LOWER_LARGE_FREQ_WHEEL_DEC),
                // Group 1
                new RadioPanelKnobT45C(1, 1 << 0, true, RadioPanelPZ69KnobsT45C.LOWER_VUHF2),   // LOWER COMM 2
                new RadioPanelKnobT45C(1, 1 << 1, true, RadioPanelPZ69KnobsT45C.LOWER_TACAN),   // LOWER NAV 1 
                new RadioPanelKnobT45C(1, 1 << 2, true, RadioPanelPZ69KnobsT45C.LOWER_VOR),     // LOWER NAV 2
                new RadioPanelKnobT45C(1, 1 << 3, true, RadioPanelPZ69KnobsT45C.LOWER_NO_USE1),
                new RadioPanelKnobT45C(1, 1 << 4, true, RadioPanelPZ69KnobsT45C.LOWER_NO_USE2),
                new RadioPanelKnobT45C(1, 1 << 5, true, RadioPanelPZ69KnobsT45C.LOWER_NO_USE3),
                new RadioPanelKnobT45C(1, 1 << 6, true, RadioPanelPZ69KnobsT45C.UPPER_FREQ_SWITCH),
                new RadioPanelKnobT45C(1, 1 << 7, true, RadioPanelPZ69KnobsT45C.LOWER_FREQ_SWITCH),
                // Group 2
                new RadioPanelKnobT45C(0, 1 << 0, true, RadioPanelPZ69KnobsT45C.UPPER_VUHF1),   // UPPER COMM 1
                new RadioPanelKnobT45C(0, 1 << 1, true, RadioPanelPZ69KnobsT45C.UPPER_VUHF2),   // UPPER COMM 2
                new RadioPanelKnobT45C(0, 1 << 2, true, RadioPanelPZ69KnobsT45C.UPPER_TACAN),   // UPPER NAV 1
                new RadioPanelKnobT45C(0, 1 << 3, true, RadioPanelPZ69KnobsT45C.UPPER_VOR),     // UPPER NAV 2
                new RadioPanelKnobT45C(0, 1 << 4, true, RadioPanelPZ69KnobsT45C.UPPER_NO_USE1),
                new RadioPanelKnobT45C(0, 1 << 5, true, RadioPanelPZ69KnobsT45C.UPPER_NO_USE2),
                new RadioPanelKnobT45C(0, 1 << 6, true, RadioPanelPZ69KnobsT45C.UPPER_NO_USE3),
                new RadioPanelKnobT45C(0, 1 << 7, true, RadioPanelPZ69KnobsT45C.LOWER_VUHF1),   // LOWER COMM 1
            };

            return result;
        }
    }
}
