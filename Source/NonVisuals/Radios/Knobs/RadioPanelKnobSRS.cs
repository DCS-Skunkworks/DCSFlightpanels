namespace NonVisuals.Radios.Knobs
{
    using System.Collections.Generic;

    using NonVisuals.Interfaces;
   
    public enum RadioPanelPZ69KnobsSRS
    {
        UPPER_COM1,   //COM1
        UPPER_COM2,  //COM2
        UPPER_NAV1, //NAV1
        UPPER_NAV2, //NAV2
        UPPER_ADF, //ADF
        UPPER_DME, //DME_
        UPPER_XPDR, //XPDR
        UPPER_SMALL_FREQ_WHEEL_INC,
        UPPER_SMALL_FREQ_WHEEL_DEC,
        UPPER_LARGE_FREQ_WHEEL_INC,
        UPPER_LARGE_FREQ_WHEEL_DEC,
        UPPER_FREQ_SWITCH ,
        LOWER_COM1,
        LOWER_COM2,
        LOWER_NAV1,
        LOWER_NAV2,
        LOWER_ADF,
        LOWER_DME,
        LOWER_XPDR,
        LOWER_SMALL_FREQ_WHEEL_INC,
        LOWER_SMALL_FREQ_WHEEL_DEC,
        LOWER_LARGE_FREQ_WHEEL_INC,
        LOWER_LARGE_FREQ_WHEEL_DEC,
        LOWER_FREQ_SWITCH
    }
    public class RadioPanelKnobSRS : ISaitekPanelKnob
    {
        public RadioPanelKnobSRS(int group, int mask, bool isOn, RadioPanelPZ69KnobsSRS radioPanelPZ69Knob)
        {
            Group = group;
            Mask = mask;
            IsOn = isOn;
            RadioPanelPZ69Knob = radioPanelPZ69Knob;
        }

        public int Group { get; set; }

        public int Mask { get; set; }

        public bool IsOn { get; set; }

        public RadioPanelPZ69KnobsSRS RadioPanelPZ69Knob { get; set; }

        public static HashSet<ISaitekPanelKnob> GetRadioPanelKnobs()
        {
            // true means clockwise turn
            var result = new HashSet<ISaitekPanelKnob>
            {
                // Group 0
                new RadioPanelKnobSRS(2, 1 << 0, true, RadioPanelPZ69KnobsSRS.UPPER_SMALL_FREQ_WHEEL_INC),
                new RadioPanelKnobSRS(2, 1 << 1, false, RadioPanelPZ69KnobsSRS.UPPER_SMALL_FREQ_WHEEL_DEC),
                new RadioPanelKnobSRS(2, 1 << 2, true, RadioPanelPZ69KnobsSRS.UPPER_LARGE_FREQ_WHEEL_INC),
                new RadioPanelKnobSRS(2, 1 << 3, false, RadioPanelPZ69KnobsSRS.UPPER_LARGE_FREQ_WHEEL_DEC),
                new RadioPanelKnobSRS(2, 1 << 4, true, RadioPanelPZ69KnobsSRS.LOWER_SMALL_FREQ_WHEEL_INC),
                new RadioPanelKnobSRS(2, 1 << 5, false, RadioPanelPZ69KnobsSRS.LOWER_SMALL_FREQ_WHEEL_DEC),
                new RadioPanelKnobSRS(2, 1 << 6, true, RadioPanelPZ69KnobsSRS.LOWER_LARGE_FREQ_WHEEL_INC),
                new RadioPanelKnobSRS(2, 1 << 7, false, RadioPanelPZ69KnobsSRS.LOWER_LARGE_FREQ_WHEEL_DEC),
                // Group 1
                new RadioPanelKnobSRS(1, 1 << 0, true, RadioPanelPZ69KnobsSRS.LOWER_COM2), // LOWER COM2
                new RadioPanelKnobSRS(1, 1 << 1, true, RadioPanelPZ69KnobsSRS.LOWER_NAV1), // LOWER NAV1
                new RadioPanelKnobSRS(1, 1 << 2, true, RadioPanelPZ69KnobsSRS.LOWER_NAV2), // LOWER NAV2
                new RadioPanelKnobSRS(1, 1 << 3, true, RadioPanelPZ69KnobsSRS.LOWER_ADF), // LOWER ADF
                new RadioPanelKnobSRS(1, 1 << 4, true, RadioPanelPZ69KnobsSRS.LOWER_DME), // LOWER DME
                new RadioPanelKnobSRS(1, 1 << 5, true, RadioPanelPZ69KnobsSRS.LOWER_XPDR), // LOWER XPDR
                new RadioPanelKnobSRS(1, 1 << 6, true, RadioPanelPZ69KnobsSRS.UPPER_FREQ_SWITCH),
                new RadioPanelKnobSRS(1, 1 << 7, true, RadioPanelPZ69KnobsSRS.LOWER_FREQ_SWITCH),
                // Group 2
                new RadioPanelKnobSRS(0, 1 << 0, true, RadioPanelPZ69KnobsSRS.UPPER_COM1), // UPPER COM1
                new RadioPanelKnobSRS(0, 1 << 1, true, RadioPanelPZ69KnobsSRS.UPPER_COM2), // UPPER COM2
                new RadioPanelKnobSRS(0, 1 << 2, true, RadioPanelPZ69KnobsSRS.UPPER_NAV1), // UPPER NAV1
                new RadioPanelKnobSRS(0, 1 << 3, true, RadioPanelPZ69KnobsSRS.UPPER_NAV2), // UPPER NAV2
                new RadioPanelKnobSRS(0, 1 << 4, true, RadioPanelPZ69KnobsSRS.UPPER_ADF), // UPPER ADF
                new RadioPanelKnobSRS(0, 1 << 5, true, RadioPanelPZ69KnobsSRS.UPPER_DME), // UPPER DME
                new RadioPanelKnobSRS(0, 1 << 6, true, RadioPanelPZ69KnobsSRS.UPPER_XPDR), // UPPER XPDR
                new RadioPanelKnobSRS(0, 1 << 7, true, RadioPanelPZ69KnobsSRS.LOWER_COM1) // LOWER COM1
            };

            return result;
        }
    }


}
