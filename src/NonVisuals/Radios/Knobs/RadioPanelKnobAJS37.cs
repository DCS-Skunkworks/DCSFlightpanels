namespace NonVisuals.Radios.Knobs
{
    using System.Collections.Generic;

    using Interfaces;



    public enum RadioPanelPZ69KnobsAJS37
    {
        UPPER_FR22,      //COM1
        UPPER_FR24,      //COM2
        UPPER_TILS,          //NAV1
        UPPER_NO_USE0,             //NAV2
        UPPER_NO_USE1,       //ADF
        UPPER_NO_USE2,          //DME_
        UPPER_NO_USE3,            //XPDR
        UPPER_SMALL_FREQ_WHEEL_INC,
        UPPER_SMALL_FREQ_WHEEL_DEC,
        UPPER_LARGE_FREQ_WHEEL_INC,
        UPPER_LARGE_FREQ_WHEEL_DEC,
        UPPER_FREQ_SWITCH,
        LOWER_FR22,   //COM1
        LOWER_FR24,   //COM2
        LOWER_TILS,      //NAV1
        LOWER_NO_USE0,          //NAV2
        LOWER_NO_USE1,    //ADF
        LOWER_NO_USE2,      //DME_
        LOWER_NO_USE3,        //XPDR
        LOWER_SMALL_FREQ_WHEEL_INC,
        LOWER_SMALL_FREQ_WHEEL_DEC,
        LOWER_LARGE_FREQ_WHEEL_INC,
        LOWER_LARGE_FREQ_WHEEL_DEC,
        LOWER_FREQ_SWITCH
    }


    
    /// <summary>
    /// Represents a knob or button on the PZ69 Radio Panel.
    /// Used by the PZ69 instance to determine what knob & button the user is manipulating.
    /// </summary>
    public class RadioPanelKnobAJS37 : ISaitekPanelKnob
    {
        public RadioPanelKnobAJS37(int group, int mask, bool isOn, RadioPanelPZ69KnobsAJS37 radioPanelPZ69Knob)
        {
            Group = group;
            Mask = mask;
            IsOn = isOn;
            RadioPanelPZ69Knob = radioPanelPZ69Knob;
        }

        public int Group { get; set; }

        public int Mask { get; set; }

        public bool IsOn { get; set; }

        public RadioPanelPZ69KnobsAJS37 RadioPanelPZ69Knob { get; set; }
        
        public static HashSet<ISaitekPanelKnob> GetRadioPanelKnobs()
        {
            // true means clockwise turn
            var result = new HashSet<ISaitekPanelKnob>
            {
                // Group 0
                new RadioPanelKnobAJS37(2, 1 << 0, true, RadioPanelPZ69KnobsAJS37.UPPER_SMALL_FREQ_WHEEL_INC),
                new RadioPanelKnobAJS37(2, 1 << 1, false, RadioPanelPZ69KnobsAJS37.UPPER_SMALL_FREQ_WHEEL_DEC),
                new RadioPanelKnobAJS37(2, 1 << 2, true, RadioPanelPZ69KnobsAJS37.UPPER_LARGE_FREQ_WHEEL_INC),
                new RadioPanelKnobAJS37(2, 1 << 3, false, RadioPanelPZ69KnobsAJS37.UPPER_LARGE_FREQ_WHEEL_DEC),
                new RadioPanelKnobAJS37(2, 1 << 4, true, RadioPanelPZ69KnobsAJS37.LOWER_SMALL_FREQ_WHEEL_INC),
                new RadioPanelKnobAJS37(2, 1 << 5, false, RadioPanelPZ69KnobsAJS37.LOWER_SMALL_FREQ_WHEEL_DEC),
                new RadioPanelKnobAJS37(2, 1 << 6, true, RadioPanelPZ69KnobsAJS37.LOWER_LARGE_FREQ_WHEEL_INC),
                new RadioPanelKnobAJS37(2, 1 << 7, false, RadioPanelPZ69KnobsAJS37.LOWER_LARGE_FREQ_WHEEL_DEC),
                // Group 1
                new RadioPanelKnobAJS37(1, 1 << 0, true, RadioPanelPZ69KnobsAJS37.LOWER_FR24), // LOWER COM2
                new RadioPanelKnobAJS37(1, 1 << 1, true, RadioPanelPZ69KnobsAJS37.LOWER_TILS), // LOWER NAV1
                new RadioPanelKnobAJS37(1, 1 << 2, true, RadioPanelPZ69KnobsAJS37.LOWER_NO_USE0), // LOWER NAV2
                new RadioPanelKnobAJS37(1, 1 << 3, true, RadioPanelPZ69KnobsAJS37.LOWER_NO_USE1), // LOWER ADF
                new RadioPanelKnobAJS37(1, 1 << 4, true, RadioPanelPZ69KnobsAJS37.LOWER_NO_USE2), // LOWER DME
                new RadioPanelKnobAJS37(1, 1 << 5, true, RadioPanelPZ69KnobsAJS37.LOWER_NO_USE3), // LOWER XPDR
                new RadioPanelKnobAJS37(1, 1 << 6, true, RadioPanelPZ69KnobsAJS37.UPPER_FREQ_SWITCH),
                new RadioPanelKnobAJS37(1, 1 << 7, true, RadioPanelPZ69KnobsAJS37.LOWER_FREQ_SWITCH),
                // Group 2
                new RadioPanelKnobAJS37(0, 1 << 0, true, RadioPanelPZ69KnobsAJS37.UPPER_FR22), // UPPER COM1
                new RadioPanelKnobAJS37(0, 1 << 1, true, RadioPanelPZ69KnobsAJS37.UPPER_FR24), // UPPER COM2
                new RadioPanelKnobAJS37(0, 1 << 2, true, RadioPanelPZ69KnobsAJS37.UPPER_TILS), // UPPER NAV1
                new RadioPanelKnobAJS37(0, 1 << 3, true, RadioPanelPZ69KnobsAJS37.UPPER_NO_USE0), // UPPER NAV2
                new RadioPanelKnobAJS37(0, 1 << 4, true, RadioPanelPZ69KnobsAJS37.UPPER_NO_USE1), // UPPER ADF
                new RadioPanelKnobAJS37(0, 1 << 5, true, RadioPanelPZ69KnobsAJS37.UPPER_NO_USE2), // UPPER DME
                new RadioPanelKnobAJS37(0, 1 << 6, true, RadioPanelPZ69KnobsAJS37.UPPER_NO_USE3), // UPPER XPDR
                new RadioPanelKnobAJS37(0, 1 << 7, true, RadioPanelPZ69KnobsAJS37.LOWER_FR22) // LOWER COM1
            };

            return result;
        }
    }


}
