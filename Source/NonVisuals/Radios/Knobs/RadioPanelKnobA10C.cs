namespace NonVisuals.Radios.Knobs
{
    using System;
    using System.Collections.Generic;

    using Interfaces;

    public enum RadioPanelPZ69KnobsA10C
    {
        UPPER_VHFAM,
        UPPER_UHF,
        UPPER_VHFFM,
        UPPER_ILS,
        UPPER_TACAN,
        UPPER_DME,
        UPPER_XPDR,
        UPPER_SMALL_FREQ_WHEEL_INC,
        UPPER_SMALL_FREQ_WHEEL_DEC,
        UPPER_LARGE_FREQ_WHEEL_INC,
        UPPER_LARGE_FREQ_WHEEL_DEC,
        UPPER_FREQ_SWITCH,
        LOWER_VHFAM,
        LOWER_UHF,
        LOWER_VHFFM,
        LOWER_ILS,
        LOWER_TACAN,
        LOWER_DME,
        LOWER_XPDR,
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

    public class RadioPanelKnobA10C : ISaitekPanelKnob
    {
        public RadioPanelKnobA10C(int group, int mask, bool isOn, RadioPanelPZ69KnobsA10C radioPanelPZ69Knob)
        {
            Group = group;
            Mask = mask;
            IsOn = isOn;
            RadioPanelPZ69Knob = radioPanelPZ69Knob;
        }

        public int Group { get; set; }

        public int Mask { get; set; }

        public bool IsOn { get; set; }

        public RadioPanelPZ69KnobsA10C RadioPanelPZ69Knob { get; set; }

        public string ExportString()
        {
            return "RadioPanelKnob{" + Enum.GetName(typeof(RadioPanelPZ69KnobsA10C), RadioPanelPZ69Knob) + "}";
        }

        public static HashSet<ISaitekPanelKnob> GetRadioPanelKnobs()
        {
            // true means clockwise turn
            var result = new HashSet<ISaitekPanelKnob>
            {
                // Group 0
                new RadioPanelKnobA10C(2, 1 << 0, true, RadioPanelPZ69KnobsA10C.UPPER_SMALL_FREQ_WHEEL_INC),
                new RadioPanelKnobA10C(2, 1 << 1, false, RadioPanelPZ69KnobsA10C.UPPER_SMALL_FREQ_WHEEL_DEC),
                new RadioPanelKnobA10C(2, 1 << 2, true, RadioPanelPZ69KnobsA10C.UPPER_LARGE_FREQ_WHEEL_INC),
                new RadioPanelKnobA10C(2, 1 << 3, false, RadioPanelPZ69KnobsA10C.UPPER_LARGE_FREQ_WHEEL_DEC),
                new RadioPanelKnobA10C(2, 1 << 4, true, RadioPanelPZ69KnobsA10C.LOWER_SMALL_FREQ_WHEEL_INC),
                new RadioPanelKnobA10C(2, 1 << 5, false, RadioPanelPZ69KnobsA10C.LOWER_SMALL_FREQ_WHEEL_DEC),
                new RadioPanelKnobA10C(2, 1 << 6, true, RadioPanelPZ69KnobsA10C.LOWER_LARGE_FREQ_WHEEL_INC),
                new RadioPanelKnobA10C(2, 1 << 7, false, RadioPanelPZ69KnobsA10C.LOWER_LARGE_FREQ_WHEEL_DEC),
                // Group 1
                new RadioPanelKnobA10C(1, 1 << 0, true, RadioPanelPZ69KnobsA10C.LOWER_UHF),
                new RadioPanelKnobA10C(1, 1 << 1, true, RadioPanelPZ69KnobsA10C.LOWER_VHFFM),
                new RadioPanelKnobA10C(1, 1 << 2, true, RadioPanelPZ69KnobsA10C.LOWER_ILS),
                new RadioPanelKnobA10C(1, 1 << 3, true, RadioPanelPZ69KnobsA10C.LOWER_TACAN),
                new RadioPanelKnobA10C(1, 1 << 4, true, RadioPanelPZ69KnobsA10C.LOWER_DME),
                new RadioPanelKnobA10C(1, 1 << 5, true, RadioPanelPZ69KnobsA10C.LOWER_XPDR),
                new RadioPanelKnobA10C(1, 1 << 6, true, RadioPanelPZ69KnobsA10C.UPPER_FREQ_SWITCH),
                new RadioPanelKnobA10C(1, 1 << 7, true, RadioPanelPZ69KnobsA10C.LOWER_FREQ_SWITCH),
                // Group 2
                new RadioPanelKnobA10C(0, 1 << 0, true, RadioPanelPZ69KnobsA10C.UPPER_VHFAM), // UPPER COM 1
                new RadioPanelKnobA10C(0, 1 << 1, true, RadioPanelPZ69KnobsA10C.UPPER_UHF), // UPPER COM 2
                new RadioPanelKnobA10C(0, 1 << 2, true, RadioPanelPZ69KnobsA10C.UPPER_VHFFM), // UPPER NAV 1
                new RadioPanelKnobA10C(0, 1 << 3, true, RadioPanelPZ69KnobsA10C.UPPER_ILS), // UPPER NAV 2
                new RadioPanelKnobA10C(0, 1 << 4, true, RadioPanelPZ69KnobsA10C.UPPER_TACAN), // UPPER ADF
                new RadioPanelKnobA10C(0, 1 << 5, true, RadioPanelPZ69KnobsA10C.UPPER_DME), // UPPER DME
                new RadioPanelKnobA10C(0, 1 << 6, true, RadioPanelPZ69KnobsA10C.UPPER_XPDR), // UPPER XPDR
                new RadioPanelKnobA10C(0, 1 << 7, true, RadioPanelPZ69KnobsA10C.LOWER_VHFAM) // LOWER COM 1 
            };

            return result;
        }
    }
}
