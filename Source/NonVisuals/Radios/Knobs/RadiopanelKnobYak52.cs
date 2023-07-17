namespace NonVisuals.Radios.Knobs
{
    using System.Collections.Generic;

    using Interfaces;

    public enum RadioPanelPZ69KnobsYak52
    {
        UPPER_VHF,      //COM1
        UPPER_NO_USE1, //COM2
        UPPER_NO_USE2,          //NAV1
        UPPER_NO_USE3,             //NAV2
        UPPER_ADF_FRONT,       //ADF
        UPPER_ADF_REAR,          //DME_
        UPPER_NO_USE4,            //XPDR
        UPPER_SMALL_FREQ_WHEEL_INC,
        UPPER_SMALL_FREQ_WHEEL_DEC,
        UPPER_LARGE_FREQ_WHEEL_INC,
        UPPER_LARGE_FREQ_WHEEL_DEC,
        UPPER_FREQ_SWITCH,
        LOWER_VHF,   //COM1
        LOWER_NO_USE1,      //COM2
        LOWER_NO_USE2,      //NAV1
        LOWER_NO_USE3,          //NAV2
        LOWER_ADF_FRONT,    //ADF
        LOWER_ADF_REAR,      //DME_
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
    public class RadioPanelKnobYak52 : ISaitekPanelKnob
    {
        public RadioPanelKnobYak52(int group, int mask, bool isOn, RadioPanelPZ69KnobsYak52 radioPanelPZ69Knob)
        {
            Group = group;
            Mask = mask;
            IsOn = isOn;
            RadioPanelPZ69Knob = radioPanelPZ69Knob;
        }

        public int Group { get; set; }

        public int Mask { get; set; }

        public bool IsOn { get; set; }

        public RadioPanelPZ69KnobsYak52 RadioPanelPZ69Knob { get; set; }
        
        public static HashSet<ISaitekPanelKnob> GetRadioPanelKnobs()
        {
            // true means clockwise turn
            var result = new HashSet<ISaitekPanelKnob>
            {
                // Group 0
                new RadioPanelKnobYak52(2, 1 << 0, true, RadioPanelPZ69KnobsYak52.UPPER_SMALL_FREQ_WHEEL_INC),
                new RadioPanelKnobYak52(2, 1 << 1, false, RadioPanelPZ69KnobsYak52.UPPER_SMALL_FREQ_WHEEL_DEC),
                new RadioPanelKnobYak52(2, 1 << 2, true, RadioPanelPZ69KnobsYak52.UPPER_LARGE_FREQ_WHEEL_INC),
                new RadioPanelKnobYak52(2, 1 << 3, false, RadioPanelPZ69KnobsYak52.UPPER_LARGE_FREQ_WHEEL_DEC),
                new RadioPanelKnobYak52(2, 1 << 4, true, RadioPanelPZ69KnobsYak52.LOWER_SMALL_FREQ_WHEEL_INC),
                new RadioPanelKnobYak52(2, 1 << 5, false, RadioPanelPZ69KnobsYak52.LOWER_SMALL_FREQ_WHEEL_DEC),
                new RadioPanelKnobYak52(2, 1 << 6, true, RadioPanelPZ69KnobsYak52.LOWER_LARGE_FREQ_WHEEL_INC),
                new RadioPanelKnobYak52(2, 1 << 7, false, RadioPanelPZ69KnobsYak52.LOWER_LARGE_FREQ_WHEEL_DEC),
                // Group 1
                new RadioPanelKnobYak52(1, 1 << 0, true, RadioPanelPZ69KnobsYak52.LOWER_NO_USE1), // LOWER COM2
                new RadioPanelKnobYak52(1, 1 << 1, true, RadioPanelPZ69KnobsYak52.LOWER_NO_USE2), // LOWER NAV1
                new RadioPanelKnobYak52(1, 1 << 2, true, RadioPanelPZ69KnobsYak52.LOWER_NO_USE3), // LOWER NAV2
                new RadioPanelKnobYak52(1, 1 << 3, true, RadioPanelPZ69KnobsYak52.LOWER_ADF_FRONT), // LOWER ADF
                new RadioPanelKnobYak52(1, 1 << 4, true, RadioPanelPZ69KnobsYak52.LOWER_ADF_REAR), // LOWER DME
                new RadioPanelKnobYak52(1, 1 << 5, true, RadioPanelPZ69KnobsYak52.LOWER_NO_USE4), // LOWER XPDR
                new RadioPanelKnobYak52(1, 1 << 6, true, RadioPanelPZ69KnobsYak52.UPPER_FREQ_SWITCH),
                new RadioPanelKnobYak52(1, 1 << 7, true, RadioPanelPZ69KnobsYak52.LOWER_FREQ_SWITCH),
                // Group 2
                new RadioPanelKnobYak52(0, 1 << 0, true, RadioPanelPZ69KnobsYak52.UPPER_VHF), // UPPER COM1
                new RadioPanelKnobYak52(0, 1 << 1, true, RadioPanelPZ69KnobsYak52.UPPER_NO_USE1), // UPPER COM2
                new RadioPanelKnobYak52(0, 1 << 2, true, RadioPanelPZ69KnobsYak52.UPPER_NO_USE2), // UPPER NAV1
                new RadioPanelKnobYak52(0, 1 << 3, true, RadioPanelPZ69KnobsYak52.UPPER_NO_USE3), // UPPER NAV2
                new RadioPanelKnobYak52(0, 1 << 4, true, RadioPanelPZ69KnobsYak52.UPPER_ADF_FRONT), // UPPER ADF
                new RadioPanelKnobYak52(0, 1 << 5, true, RadioPanelPZ69KnobsYak52.UPPER_ADF_REAR), // UPPER DME
                new RadioPanelKnobYak52(0, 1 << 6, true, RadioPanelPZ69KnobsYak52.UPPER_NO_USE4), // UPPER XPDR
                new RadioPanelKnobYak52(0, 1 << 7, true, RadioPanelPZ69KnobsYak52.LOWER_VHF) // LOWER COM1
            };

            return result;
        }
    }
}
