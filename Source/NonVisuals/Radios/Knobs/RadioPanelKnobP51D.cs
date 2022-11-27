namespace NonVisuals.Radios.Knobs
{
    using System.Collections.Generic;

    using Interfaces;

    public enum RadioPanelPZ69KnobsP51D
    {
        UPPER_VHF,   //COM1
        UPPER_NO_USE0,  //COM2
        UPPER_NO_USE1, //NAV1
        UPPER_NO_USE2, //NAV2
        UPPER_NO_USE3, //ADF
        UPPER_NO_USE4, //DME_
        UPPER_NO_USE5, //XPDR
        UPPER_SMALL_FREQ_WHEEL_INC,
        UPPER_SMALL_FREQ_WHEEL_DEC,
        UPPER_LARGE_FREQ_WHEEL_INC,
        UPPER_LARGE_FREQ_WHEEL_DEC,
        UPPER_FREQ_SWITCH,
        LOWER_VHF,
        LOWER_NO_USE0,
        LOWER_NO_USE1,
        LOWER_NO_USE2,
        LOWER_NO_USE3,
        LOWER_NO_USE4,
        LOWER_NO_USE5,
        LOWER_SMALL_FREQ_WHEEL_INC,
        LOWER_SMALL_FREQ_WHEEL_DEC,
        LOWER_LARGE_FREQ_WHEEL_INC,
        LOWER_LARGE_FREQ_WHEEL_DEC,
        LOWER_FREQ_SWITCH
    }

    
    /// <summary>
    /// Represents a knob or button on the PZ69 Radio Panel. Used by the PZ69 instance to determine what knob & button the user is manipulating.
    /// </summary>
    public class RadioPanelKnobP51D : ISaitekPanelKnob
    {
        public RadioPanelKnobP51D(int group, int mask, bool isOn, RadioPanelPZ69KnobsP51D radioPanelPZ69Knob)
        {
            Group = group;
            Mask = mask;
            IsOn = isOn;
            RadioPanelPZ69Knob = radioPanelPZ69Knob;
        }

        public int Group { get; set; }

        public int Mask { get; set; }

        public bool IsOn { get; set; }

        public RadioPanelPZ69KnobsP51D RadioPanelPZ69Knob { get; set; }
        
        public static HashSet<ISaitekPanelKnob> GetRadioPanelKnobs()
        {
            // true means clockwise turn
            var result = new HashSet<ISaitekPanelKnob>
            {
                // Group 0
                new RadioPanelKnobP51D(2, 1 << 0, true, RadioPanelPZ69KnobsP51D.UPPER_SMALL_FREQ_WHEEL_INC),
                new RadioPanelKnobP51D(2, 1 << 1, false, RadioPanelPZ69KnobsP51D.UPPER_SMALL_FREQ_WHEEL_DEC),
                new RadioPanelKnobP51D(2, 1 << 2, true, RadioPanelPZ69KnobsP51D.UPPER_LARGE_FREQ_WHEEL_INC),
                new RadioPanelKnobP51D(2, 1 << 3, false, RadioPanelPZ69KnobsP51D.UPPER_LARGE_FREQ_WHEEL_DEC),
                new RadioPanelKnobP51D(2, 1 << 4, true, RadioPanelPZ69KnobsP51D.LOWER_SMALL_FREQ_WHEEL_INC),
                new RadioPanelKnobP51D(2, 1 << 5, false, RadioPanelPZ69KnobsP51D.LOWER_SMALL_FREQ_WHEEL_DEC),
                new RadioPanelKnobP51D(2, 1 << 6, true, RadioPanelPZ69KnobsP51D.LOWER_LARGE_FREQ_WHEEL_INC),
                new RadioPanelKnobP51D(2, 1 << 7, false, RadioPanelPZ69KnobsP51D.LOWER_LARGE_FREQ_WHEEL_DEC),
                // Group 1
                new RadioPanelKnobP51D(1, 1 << 0, true, RadioPanelPZ69KnobsP51D.LOWER_NO_USE0), // LOWER COM2
                new RadioPanelKnobP51D(1, 1 << 1, true, RadioPanelPZ69KnobsP51D.LOWER_NO_USE1), // LOWER NAV1
                new RadioPanelKnobP51D(1, 1 << 2, true, RadioPanelPZ69KnobsP51D.LOWER_NO_USE2), // LOWER NAV2
                new RadioPanelKnobP51D(1, 1 << 3, true, RadioPanelPZ69KnobsP51D.LOWER_NO_USE3), // LOWER ADF
                new RadioPanelKnobP51D(1, 1 << 4, true, RadioPanelPZ69KnobsP51D.LOWER_NO_USE4), // LOWER DME
                new RadioPanelKnobP51D(1, 1 << 5, true, RadioPanelPZ69KnobsP51D.LOWER_NO_USE5), // LOWER XPDR
                new RadioPanelKnobP51D(1, 1 << 6, true, RadioPanelPZ69KnobsP51D.UPPER_FREQ_SWITCH),
                new RadioPanelKnobP51D(1, 1 << 7, true, RadioPanelPZ69KnobsP51D.LOWER_FREQ_SWITCH),
                // Group 2
                new RadioPanelKnobP51D(0, 1 << 0, true, RadioPanelPZ69KnobsP51D.UPPER_VHF), // UPPER COM1
                new RadioPanelKnobP51D(0, 1 << 1, true, RadioPanelPZ69KnobsP51D.UPPER_NO_USE0), // UPPER COM2
                new RadioPanelKnobP51D(0, 1 << 2, true, RadioPanelPZ69KnobsP51D.UPPER_NO_USE1), // UPPER NAV1
                new RadioPanelKnobP51D(0, 1 << 3, true, RadioPanelPZ69KnobsP51D.UPPER_NO_USE2), // UPPER NAV2
                new RadioPanelKnobP51D(0, 1 << 4, true, RadioPanelPZ69KnobsP51D.UPPER_NO_USE3), // UPPER ADF
                new RadioPanelKnobP51D(0, 1 << 5, true, RadioPanelPZ69KnobsP51D.UPPER_NO_USE4), // UPPER DME
                new RadioPanelKnobP51D(0, 1 << 6, true, RadioPanelPZ69KnobsP51D.UPPER_NO_USE5), // UPPER XPDR
                new RadioPanelKnobP51D(0, 1 << 7, true, RadioPanelPZ69KnobsP51D.LOWER_VHF) // LOWER COM1
            };

            return result;
        }
    }


}
