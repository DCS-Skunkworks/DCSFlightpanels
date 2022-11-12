namespace NonVisuals.Radios.Knobs
{
    using System.Collections.Generic;

    using Interfaces;

    public enum RadioPanelPZ69KnobsBf109
    {
        UPPER_FUG16ZY,      //COM1
        UPPER_IFF,      //COM2
        UPPER_HOMING,          //NAV1
        UPPER_NO_USE1,             //NAV2
        UPPER_NO_USE2,       //ADF
        UPPER_NO_USE3,          //DME_
        UPPER_NO_USE4,            //XPDR
        UPPER_SMALL_FREQ_WHEEL_INC,
        UPPER_SMALL_FREQ_WHEEL_DEC,
        UPPER_LARGE_FREQ_WHEEL_INC,
        UPPER_LARGE_FREQ_WHEEL_DEC,
        UPPER_FREQ_SWITCH,
        LOWER_FUG16ZY,   //COM1
        LOWER_IFF,   //COM2
        LOWER_HOMING,      //NAV1
        LOWER_NO_USE1,          //NAV2
        LOWER_NO_USE2,    //ADF
        LOWER_NO_USE3,      //DME_
        LOWER_NO_USE4,        //XPDR
        LOWER_SMALL_FREQ_WHEEL_INC,
        LOWER_SMALL_FREQ_WHEEL_DEC,
        LOWER_LARGE_FREQ_WHEEL_INC,
        LOWER_LARGE_FREQ_WHEEL_DEC,
        LOWER_FREQ_SWITCH
    }

    public class RadioPanelKnobBf109 : ISaitekPanelKnob
    {
        public RadioPanelKnobBf109(int group, int mask, bool isOn, RadioPanelPZ69KnobsBf109 radioPanelPZ69Knob)
        {
            Group = group;
            Mask = mask;
            IsOn = isOn;
            RadioPanelPZ69Knob = radioPanelPZ69Knob;
        }

        public int Group { get; set; }

        public int Mask { get; set; }

        public bool IsOn { get; set; }

        public RadioPanelPZ69KnobsBf109 RadioPanelPZ69Knob { get; set; }

        public static HashSet<ISaitekPanelKnob> GetRadioPanelKnobs()
        {
            // true means clockwise turn
            var result = new HashSet<ISaitekPanelKnob>
            {
                // Group 0
                new RadioPanelKnobBf109(2, 1 << 0, true, RadioPanelPZ69KnobsBf109.UPPER_SMALL_FREQ_WHEEL_INC),
                new RadioPanelKnobBf109(2, 1 << 1, false, RadioPanelPZ69KnobsBf109.UPPER_SMALL_FREQ_WHEEL_DEC),
                new RadioPanelKnobBf109(2, 1 << 2, true, RadioPanelPZ69KnobsBf109.UPPER_LARGE_FREQ_WHEEL_INC),
                new RadioPanelKnobBf109(2, 1 << 3, false, RadioPanelPZ69KnobsBf109.UPPER_LARGE_FREQ_WHEEL_DEC),
                new RadioPanelKnobBf109(2, 1 << 4, true, RadioPanelPZ69KnobsBf109.LOWER_SMALL_FREQ_WHEEL_INC),
                new RadioPanelKnobBf109(2, 1 << 5, false, RadioPanelPZ69KnobsBf109.LOWER_SMALL_FREQ_WHEEL_DEC),
                new RadioPanelKnobBf109(2, 1 << 6, true, RadioPanelPZ69KnobsBf109.LOWER_LARGE_FREQ_WHEEL_INC),
                new RadioPanelKnobBf109(2, 1 << 7, false, RadioPanelPZ69KnobsBf109.LOWER_LARGE_FREQ_WHEEL_DEC),
                // Group 1
                new RadioPanelKnobBf109(1, 1 << 0, true, RadioPanelPZ69KnobsBf109.LOWER_IFF), // LOWER COM2
                new RadioPanelKnobBf109(1, 1 << 1, true, RadioPanelPZ69KnobsBf109.LOWER_HOMING), // LOWER NAV1
                new RadioPanelKnobBf109(1, 1 << 2, true, RadioPanelPZ69KnobsBf109.LOWER_NO_USE1), // LOWER NAV2
                new RadioPanelKnobBf109(1, 1 << 3, true, RadioPanelPZ69KnobsBf109.LOWER_NO_USE2), // LOWER ADF
                new RadioPanelKnobBf109(1, 1 << 4, true, RadioPanelPZ69KnobsBf109.LOWER_NO_USE3), // LOWER DME
                new RadioPanelKnobBf109(1, 1 << 5, true, RadioPanelPZ69KnobsBf109.LOWER_NO_USE4), // LOWER XPDR
                new RadioPanelKnobBf109(1, 1 << 6, true, RadioPanelPZ69KnobsBf109.UPPER_FREQ_SWITCH),
                new RadioPanelKnobBf109(1, 1 << 7, true, RadioPanelPZ69KnobsBf109.LOWER_FREQ_SWITCH),
                // Group 2
                new RadioPanelKnobBf109(0, 1 << 0, true, RadioPanelPZ69KnobsBf109.UPPER_FUG16ZY), // UPPER COM1
                new RadioPanelKnobBf109(0, 1 << 1, true, RadioPanelPZ69KnobsBf109.UPPER_IFF), // UPPER COM2
                new RadioPanelKnobBf109(0, 1 << 2, true, RadioPanelPZ69KnobsBf109.UPPER_HOMING), // UPPER NAV1
                new RadioPanelKnobBf109(0, 1 << 3, true, RadioPanelPZ69KnobsBf109.UPPER_NO_USE1), // UPPER NAV2
                new RadioPanelKnobBf109(0, 1 << 4, true, RadioPanelPZ69KnobsBf109.UPPER_NO_USE2), // UPPER ADF
                new RadioPanelKnobBf109(0, 1 << 5, true, RadioPanelPZ69KnobsBf109.UPPER_NO_USE3), // UPPER DME
                new RadioPanelKnobBf109(0, 1 << 6, true, RadioPanelPZ69KnobsBf109.UPPER_NO_USE4), // UPPER XPDR
                new RadioPanelKnobBf109(0, 1 << 7, true, RadioPanelPZ69KnobsBf109.LOWER_FUG16ZY) // LOWER COM1
            };

            return result;
        }
    }
}
