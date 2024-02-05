namespace NonVisuals.Radios.Knobs
{
    using System.Collections.Generic;

    using Interfaces;

    public enum RadioPanelPZ69KnobsAH64D
    {
        UPPER_VHF,      //COM1
        UPPER_UHF,      //COM2
        UPPER_FM1,          //NAV1
        UPPER_FM2,             //NAV2
        UPPER_HF,       //ADF
        UPPER_NO_USE3,          //DME_
        UPPER_NO_USE4,            //XPDR
        UPPER_SMALL_FREQ_WHEEL_INC,
        UPPER_SMALL_FREQ_WHEEL_DEC,
        UPPER_LARGE_FREQ_WHEEL_INC,
        UPPER_LARGE_FREQ_WHEEL_DEC,
        UPPER_FREQ_SWITCH,
        
        LOWER_VHF,   //COM1
        LOWER_UHF,   //COM2
        LOWER_FM1,      //NAV1
        LOWER_FM2,          //NAV2
        LOWER_HF,    //ADF
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
    public class RadioPanelKnobAH64D : ISaitekPanelKnob
    {
        public RadioPanelKnobAH64D(int group, int mask, bool isOn, RadioPanelPZ69KnobsAH64D radioPanelPZ69Knob)
        {
            Group = group;
            Mask = mask;
            IsOn = isOn;
            RadioPanelPZ69Knob = radioPanelPZ69Knob;
        }

        public int Group { get; set; }

        public int Mask { get; set; }

        public bool IsOn { get; set; }

        public RadioPanelPZ69KnobsAH64D RadioPanelPZ69Knob { get; set; }
        
        public static HashSet<ISaitekPanelKnob> GetRadioPanelKnobs()
        {
            // true means clockwise turn
            var result = new HashSet<ISaitekPanelKnob>
            {
                // Group 0
                new RadioPanelKnobAH64D(2, 1 << 0, true, RadioPanelPZ69KnobsAH64D.UPPER_SMALL_FREQ_WHEEL_INC),
                new RadioPanelKnobAH64D(2, 1 << 1, false, RadioPanelPZ69KnobsAH64D.UPPER_SMALL_FREQ_WHEEL_DEC),
                new RadioPanelKnobAH64D(2, 1 << 2, true, RadioPanelPZ69KnobsAH64D.UPPER_LARGE_FREQ_WHEEL_INC),
                new RadioPanelKnobAH64D(2, 1 << 3, false, RadioPanelPZ69KnobsAH64D.UPPER_LARGE_FREQ_WHEEL_DEC),
                new RadioPanelKnobAH64D(2, 1 << 4, true, RadioPanelPZ69KnobsAH64D.LOWER_SMALL_FREQ_WHEEL_INC),
                new RadioPanelKnobAH64D(2, 1 << 5, false, RadioPanelPZ69KnobsAH64D.LOWER_SMALL_FREQ_WHEEL_DEC),
                new RadioPanelKnobAH64D(2, 1 << 6, true, RadioPanelPZ69KnobsAH64D.LOWER_LARGE_FREQ_WHEEL_INC),
                new RadioPanelKnobAH64D(2, 1 << 7, false, RadioPanelPZ69KnobsAH64D.LOWER_LARGE_FREQ_WHEEL_DEC),
                // Group 1
                new RadioPanelKnobAH64D(1, 1 << 0, true, RadioPanelPZ69KnobsAH64D.LOWER_UHF), 
                new RadioPanelKnobAH64D(1, 1 << 1, true, RadioPanelPZ69KnobsAH64D.LOWER_FM1), 
                new RadioPanelKnobAH64D(1, 1 << 2, true, RadioPanelPZ69KnobsAH64D.LOWER_FM2), 
                new RadioPanelKnobAH64D(1, 1 << 3, true, RadioPanelPZ69KnobsAH64D.LOWER_HF), 
                new RadioPanelKnobAH64D(1, 1 << 4, true, RadioPanelPZ69KnobsAH64D.LOWER_NO_USE3), 
                new RadioPanelKnobAH64D(1, 1 << 5, true, RadioPanelPZ69KnobsAH64D.LOWER_NO_USE4), 
                new RadioPanelKnobAH64D(1, 1 << 6, true, RadioPanelPZ69KnobsAH64D.UPPER_FREQ_SWITCH),
                new RadioPanelKnobAH64D(1, 1 << 7, true, RadioPanelPZ69KnobsAH64D.LOWER_FREQ_SWITCH),
                // Group 2
                new RadioPanelKnobAH64D(0, 1 << 0, true, RadioPanelPZ69KnobsAH64D.UPPER_VHF), 
                new RadioPanelKnobAH64D(0, 1 << 1, true, RadioPanelPZ69KnobsAH64D.UPPER_UHF), 
                new RadioPanelKnobAH64D(0, 1 << 2, true, RadioPanelPZ69KnobsAH64D.UPPER_FM1), 
                new RadioPanelKnobAH64D(0, 1 << 3, true, RadioPanelPZ69KnobsAH64D.UPPER_FM2), 
                new RadioPanelKnobAH64D(0, 1 << 4, true, RadioPanelPZ69KnobsAH64D.UPPER_HF), 
                new RadioPanelKnobAH64D(0, 1 << 5, true, RadioPanelPZ69KnobsAH64D.UPPER_NO_USE3), 
                new RadioPanelKnobAH64D(0, 1 << 6, true, RadioPanelPZ69KnobsAH64D.UPPER_NO_USE4), 
                new RadioPanelKnobAH64D(0, 1 << 7, true, RadioPanelPZ69KnobsAH64D.LOWER_VHF) 
            };

            return result;
        }
    }


}
