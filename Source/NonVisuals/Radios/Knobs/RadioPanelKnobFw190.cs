namespace NonVisuals.Radios.Knobs
{
    using System.Collections.Generic;

     using NonVisuals.Interfaces;

    public enum RadioPanelPZ69KnobsFw190
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
        LOWER_HOMING ,      //NAV1
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

    public class RadioPanelKnobFw190 : ISaitekPanelKnob
    {
        public RadioPanelKnobFw190(int group, int mask, bool isOn, RadioPanelPZ69KnobsFw190 radioPanelPZ69Knob)
        {
            Group = group;
            Mask = mask;
            IsOn = isOn;
            RadioPanelPZ69Knob = radioPanelPZ69Knob;
        }

        public int Group { get; set; }

        public int Mask { get; set; }

        public bool IsOn { get; set; }

        public RadioPanelPZ69KnobsFw190 RadioPanelPZ69Knob { get; set; }

        public static HashSet<ISaitekPanelKnob> GetRadioPanelKnobs()
        {
            // true means clockwise turn
            var result = new HashSet<ISaitekPanelKnob>
            {
                // Group 0
                new RadioPanelKnobFw190(2, 1 << 0, true, RadioPanelPZ69KnobsFw190.UPPER_SMALL_FREQ_WHEEL_INC),
                new RadioPanelKnobFw190(2, 1 << 1, false, RadioPanelPZ69KnobsFw190.UPPER_SMALL_FREQ_WHEEL_DEC),
                new RadioPanelKnobFw190(2, 1 << 2, true, RadioPanelPZ69KnobsFw190.UPPER_LARGE_FREQ_WHEEL_INC),
                new RadioPanelKnobFw190(2, 1 << 3, false, RadioPanelPZ69KnobsFw190.UPPER_LARGE_FREQ_WHEEL_DEC),
                new RadioPanelKnobFw190(2, 1 << 4, true, RadioPanelPZ69KnobsFw190.LOWER_SMALL_FREQ_WHEEL_INC),
                new RadioPanelKnobFw190(2, 1 << 5, false, RadioPanelPZ69KnobsFw190.LOWER_SMALL_FREQ_WHEEL_DEC),
                new RadioPanelKnobFw190(2, 1 << 6, true, RadioPanelPZ69KnobsFw190.LOWER_LARGE_FREQ_WHEEL_INC),
                new RadioPanelKnobFw190(2, 1 << 7, false, RadioPanelPZ69KnobsFw190.LOWER_LARGE_FREQ_WHEEL_DEC),
                // Group 1
                new RadioPanelKnobFw190(1, 1 << 0, true, RadioPanelPZ69KnobsFw190.LOWER_IFF), // LOWER COM2
                new RadioPanelKnobFw190(1, 1 << 1, true, RadioPanelPZ69KnobsFw190.LOWER_HOMING), // LOWER NAV1
                new RadioPanelKnobFw190(1, 1 << 2, true, RadioPanelPZ69KnobsFw190.LOWER_NO_USE1), // LOWER NAV2
                new RadioPanelKnobFw190(1, 1 << 3, true, RadioPanelPZ69KnobsFw190.LOWER_NO_USE2), // LOWER ADF
                new RadioPanelKnobFw190(1, 1 << 4, true, RadioPanelPZ69KnobsFw190.LOWER_NO_USE3), // LOWER DME
                new RadioPanelKnobFw190(1, 1 << 5, true, RadioPanelPZ69KnobsFw190.LOWER_NO_USE4), // LOWER XPDR
                new RadioPanelKnobFw190(1, 1 << 6, true, RadioPanelPZ69KnobsFw190.UPPER_FREQ_SWITCH),
                new RadioPanelKnobFw190(1, 1 << 7, true, RadioPanelPZ69KnobsFw190.LOWER_FREQ_SWITCH),
                // Group 2
                new RadioPanelKnobFw190(0, 1 << 0, true, RadioPanelPZ69KnobsFw190.UPPER_FUG16ZY), // UPPER COM1
                new RadioPanelKnobFw190(0, 1 << 1, true, RadioPanelPZ69KnobsFw190.UPPER_IFF), // UPPER COM2
                new RadioPanelKnobFw190(0, 1 << 2, true, RadioPanelPZ69KnobsFw190.UPPER_HOMING), // UPPER NAV1
                new RadioPanelKnobFw190(0, 1 << 3, true, RadioPanelPZ69KnobsFw190.UPPER_NO_USE1), // UPPER NAV2
                new RadioPanelKnobFw190(0, 1 << 4, true, RadioPanelPZ69KnobsFw190.UPPER_NO_USE2), // UPPER ADF
                new RadioPanelKnobFw190(0, 1 << 5, true, RadioPanelPZ69KnobsFw190.UPPER_NO_USE3), // UPPER DME
                new RadioPanelKnobFw190(0, 1 << 6, true, RadioPanelPZ69KnobsFw190.UPPER_NO_USE4), // UPPER XPDR
                new RadioPanelKnobFw190(0, 1 << 7, true, RadioPanelPZ69KnobsFw190.LOWER_FUG16ZY) // LOWER COM1
            };

            return result;
        }
    }
}
