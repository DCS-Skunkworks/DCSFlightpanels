namespace NonVisuals.Radios.Knobs
{
    using System.Collections.Generic;

    using Interfaces;

    public enum RadioPanelPZ69KnobsM2000C
    {
        UPPER_VUHF,      //COM1
        UPPER_UHF,      //COM2
        UPPER_TACAN,          //NAV1
        UPPER_VOR,             //NAV2
        UPPER_NO_USE2,       //ADF
        UPPER_NO_USE3,          //DME_
        UPPER_NO_USE4,            //XPDR
        UPPER_SMALL_FREQ_WHEEL_INC,
        UPPER_SMALL_FREQ_WHEEL_DEC,
        UPPER_LARGE_FREQ_WHEEL_INC,
        UPPER_LARGE_FREQ_WHEEL_DEC,
        UPPER_FREQ_SWITCH,
        LOWER_VUHF,   //COM1
        LOWER_UHF,   //COM2
        LOWER_TACAN,      //NAV1
        LOWER_VOR,          //NAV2
        LOWER_NO_USE2,    //ADF
        LOWER_NO_USE3,      //DME_
        LOWER_NO_USE4,        //XPDR
        LOWER_SMALL_FREQ_WHEEL_INC,
        LOWER_SMALL_FREQ_WHEEL_DEC,
        LOWER_LARGE_FREQ_WHEEL_INC,
        LOWER_LARGE_FREQ_WHEEL_DEC,
        LOWER_FREQ_SWITCH
    }

    /// <summary>
    /// Represents a knob or button on the PZ69 Radio Panel. Used by the PZ69 instance to determine what knob & button the user is manipulating.
    /// </summary>
    public class RadioPanelKnobM2000C : ISaitekPanelKnob
    {
        public RadioPanelKnobM2000C(int group, int mask, bool isOn, RadioPanelPZ69KnobsM2000C radioPanelPZ69Knob)
        {
            Group = group;
            Mask = mask;
            IsOn = isOn;
            RadioPanelPZ69Knob = radioPanelPZ69Knob;
        }

        public int Group { get; set; }

        public int Mask { get; set; }

        public bool IsOn { get; set; }

        public RadioPanelPZ69KnobsM2000C RadioPanelPZ69Knob { get; set; }
        
        public static HashSet<ISaitekPanelKnob> GetRadioPanelKnobs()
        {
            // true means clockwise turn
            var result = new HashSet<ISaitekPanelKnob>
            {
                // Group 0
                new RadioPanelKnobM2000C(2, 1 << 0, true, RadioPanelPZ69KnobsM2000C.UPPER_SMALL_FREQ_WHEEL_INC),
                new RadioPanelKnobM2000C(2, 1 << 1, false, RadioPanelPZ69KnobsM2000C.UPPER_SMALL_FREQ_WHEEL_DEC),
                new RadioPanelKnobM2000C(2, 1 << 2, true, RadioPanelPZ69KnobsM2000C.UPPER_LARGE_FREQ_WHEEL_INC),
                new RadioPanelKnobM2000C(2, 1 << 3, false, RadioPanelPZ69KnobsM2000C.UPPER_LARGE_FREQ_WHEEL_DEC),
                new RadioPanelKnobM2000C(2, 1 << 4, true, RadioPanelPZ69KnobsM2000C.LOWER_SMALL_FREQ_WHEEL_INC),
                new RadioPanelKnobM2000C(2, 1 << 5, false, RadioPanelPZ69KnobsM2000C.LOWER_SMALL_FREQ_WHEEL_DEC),
                new RadioPanelKnobM2000C(2, 1 << 6, true, RadioPanelPZ69KnobsM2000C.LOWER_LARGE_FREQ_WHEEL_INC),
                new RadioPanelKnobM2000C(2, 1 << 7, false, RadioPanelPZ69KnobsM2000C.LOWER_LARGE_FREQ_WHEEL_DEC),
                // Group 1
                new RadioPanelKnobM2000C(1, 1 << 0, true, RadioPanelPZ69KnobsM2000C.LOWER_UHF), // LOWER COM2
                new RadioPanelKnobM2000C(1, 1 << 1, true, RadioPanelPZ69KnobsM2000C.LOWER_TACAN), // LOWER NAV1
                new RadioPanelKnobM2000C(1, 1 << 2, true, RadioPanelPZ69KnobsM2000C.LOWER_VOR), // LOWER NAV2
                new RadioPanelKnobM2000C(1, 1 << 3, true, RadioPanelPZ69KnobsM2000C.LOWER_NO_USE2), // LOWER ADF
                new RadioPanelKnobM2000C(1, 1 << 4, true, RadioPanelPZ69KnobsM2000C.LOWER_NO_USE3), // LOWER DME
                new RadioPanelKnobM2000C(1, 1 << 5, true, RadioPanelPZ69KnobsM2000C.LOWER_NO_USE4), // LOWER XPDR
                new RadioPanelKnobM2000C(1, 1 << 6, true, RadioPanelPZ69KnobsM2000C.UPPER_FREQ_SWITCH),
                new RadioPanelKnobM2000C(1, 1 << 7, true, RadioPanelPZ69KnobsM2000C.LOWER_FREQ_SWITCH),
                // Group 2
                new RadioPanelKnobM2000C(0, 1 << 0, true, RadioPanelPZ69KnobsM2000C.UPPER_VUHF), // UPPER COM1
                new RadioPanelKnobM2000C(0, 1 << 1, true, RadioPanelPZ69KnobsM2000C.UPPER_UHF), // UPPER COM2
                new RadioPanelKnobM2000C(0, 1 << 2, true, RadioPanelPZ69KnobsM2000C.UPPER_TACAN), // UPPER NAV1
                new RadioPanelKnobM2000C(0, 1 << 3, true, RadioPanelPZ69KnobsM2000C.UPPER_VOR), // UPPER NAV2
                new RadioPanelKnobM2000C(0, 1 << 4, true, RadioPanelPZ69KnobsM2000C.UPPER_NO_USE2), // UPPER ADF
                new RadioPanelKnobM2000C(0, 1 << 5, true, RadioPanelPZ69KnobsM2000C.UPPER_NO_USE3), // UPPER DME
                new RadioPanelKnobM2000C(0, 1 << 6, true, RadioPanelPZ69KnobsM2000C.UPPER_NO_USE4), // UPPER XPDR
                new RadioPanelKnobM2000C(0, 1 << 7, true, RadioPanelPZ69KnobsM2000C.LOWER_VUHF) // LOWER COM1
            };

            return result;
        }
    }


}
