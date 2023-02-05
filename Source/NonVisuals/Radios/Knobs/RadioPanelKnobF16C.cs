namespace NonVisuals.Radios.Knobs
{
    using System.Collections.Generic;

    using Interfaces;

    public enum RadioPanelPZ69KnobsF16C
    {
        UPPER_UHF,      //COM1
        UPPER_VHF,      //COM2
        UPPER_TACAN,          //NAV1
        UPPER_ILS,             //NAV2
        UPPER_NO_USE2,       //ADF
        UPPER_NO_USE3,          //DME_
        UPPER_NO_USE4,            //XPDR
        UPPER_SMALL_FREQ_WHEEL_INC,
        UPPER_SMALL_FREQ_WHEEL_DEC,
        UPPER_LARGE_FREQ_WHEEL_INC,
        UPPER_LARGE_FREQ_WHEEL_DEC,
        UPPER_FREQ_SWITCH,
        
        LOWER_UHF,   //COM1
        LOWER_VHF,   //COM2
        LOWER_TACAN,      //NAV1
        LOWER_ILS,          //NAV2
        LOWER_NO_USE2,    //ADF
        LOWER_NO_USE3,      //DME_
        LOWER_NO_USE4,        //XPDR
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
    public class RadioPanelKnobF16C : ISaitekPanelKnob
    {
        public RadioPanelKnobF16C(int group, int mask, bool isOn, RadioPanelPZ69KnobsF16C radioPanelPZ69Knob)
        {
            Group = group;
            Mask = mask;
            IsOn = isOn;
            RadioPanelPZ69Knob = radioPanelPZ69Knob;
        }

        public int Group { get; set; }

        public int Mask { get; set; }

        public bool IsOn { get; set; }

        public RadioPanelPZ69KnobsF16C RadioPanelPZ69Knob { get; set; }
        
        public static HashSet<ISaitekPanelKnob> GetRadioPanelKnobs()
        {
            // true means clockwise turn
            var result = new HashSet<ISaitekPanelKnob>
            {
                // Group 0
                new RadioPanelKnobF16C(2, 1 << 0, true, RadioPanelPZ69KnobsF16C.UPPER_SMALL_FREQ_WHEEL_INC),
                new RadioPanelKnobF16C(2, 1 << 1, false, RadioPanelPZ69KnobsF16C.UPPER_SMALL_FREQ_WHEEL_DEC),
                new RadioPanelKnobF16C(2, 1 << 2, true, RadioPanelPZ69KnobsF16C.UPPER_LARGE_FREQ_WHEEL_INC),
                new RadioPanelKnobF16C(2, 1 << 3, false, RadioPanelPZ69KnobsF16C.UPPER_LARGE_FREQ_WHEEL_DEC),
                new RadioPanelKnobF16C(2, 1 << 4, true, RadioPanelPZ69KnobsF16C.LOWER_SMALL_FREQ_WHEEL_INC),
                new RadioPanelKnobF16C(2, 1 << 5, false, RadioPanelPZ69KnobsF16C.LOWER_SMALL_FREQ_WHEEL_DEC),
                new RadioPanelKnobF16C(2, 1 << 6, true, RadioPanelPZ69KnobsF16C.LOWER_LARGE_FREQ_WHEEL_INC),
                new RadioPanelKnobF16C(2, 1 << 7, false, RadioPanelPZ69KnobsF16C.LOWER_LARGE_FREQ_WHEEL_DEC),
                // Group 1
                new RadioPanelKnobF16C(1, 1 << 0, true, RadioPanelPZ69KnobsF16C.LOWER_VHF), 
                new RadioPanelKnobF16C(1, 1 << 1, true, RadioPanelPZ69KnobsF16C.LOWER_TACAN), 
                new RadioPanelKnobF16C(1, 1 << 2, true, RadioPanelPZ69KnobsF16C.LOWER_ILS), 
                new RadioPanelKnobF16C(1, 1 << 3, true, RadioPanelPZ69KnobsF16C.LOWER_NO_USE2), 
                new RadioPanelKnobF16C(1, 1 << 4, true, RadioPanelPZ69KnobsF16C.LOWER_NO_USE3), 
                new RadioPanelKnobF16C(1, 1 << 5, true, RadioPanelPZ69KnobsF16C.LOWER_NO_USE4), 
                new RadioPanelKnobF16C(1, 1 << 6, true, RadioPanelPZ69KnobsF16C.UPPER_FREQ_SWITCH),
                new RadioPanelKnobF16C(1, 1 << 7, true, RadioPanelPZ69KnobsF16C.LOWER_FREQ_SWITCH),
                // Group 2
                new RadioPanelKnobF16C(0, 1 << 0, true, RadioPanelPZ69KnobsF16C.UPPER_UHF), 
                new RadioPanelKnobF16C(0, 1 << 1, true, RadioPanelPZ69KnobsF16C.UPPER_VHF), 
                new RadioPanelKnobF16C(0, 1 << 2, true, RadioPanelPZ69KnobsF16C.UPPER_TACAN), 
                new RadioPanelKnobF16C(0, 1 << 3, true, RadioPanelPZ69KnobsF16C.UPPER_ILS), 
                new RadioPanelKnobF16C(0, 1 << 4, true, RadioPanelPZ69KnobsF16C.UPPER_NO_USE2), 
                new RadioPanelKnobF16C(0, 1 << 5, true, RadioPanelPZ69KnobsF16C.UPPER_NO_USE3), 
                new RadioPanelKnobF16C(0, 1 << 6, true, RadioPanelPZ69KnobsF16C.UPPER_NO_USE4), 
                new RadioPanelKnobF16C(0, 1 << 7, true, RadioPanelPZ69KnobsF16C.LOWER_UHF) 
            };

            return result;
        }
    }


}
