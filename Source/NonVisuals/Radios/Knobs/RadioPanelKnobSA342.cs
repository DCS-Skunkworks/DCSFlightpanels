namespace NonVisuals.Radios.Knobs
{
    using System.Collections.Generic;

    using NonVisuals.Interfaces;
    public enum RadioPanelPZ69KnobsSA342
    {
        UPPER_VHFAM,
        UPPER_VHFFM,
        UPPER_UHF,
        UPPER_NAV2,
        UPPER_ADF,
        UPPER_NADIR,
        UPPER_XPDR,
        UPPER_SMALL_FREQ_WHEEL_INC,
        UPPER_SMALL_FREQ_WHEEL_DEC,
        UPPER_LARGE_FREQ_WHEEL_INC,
        UPPER_LARGE_FREQ_WHEEL_DEC,
        UPPER_FREQ_SWITCH,
        LOWER_VHFAM,
        LOWER_VHFFM,
        LOWER_UHF,
        LOWER_NAV2,
        LOWER_ADF,
        LOWER_NADIR,
        LOWER_XPDR,
        LOWER_SMALL_FREQ_WHEEL_INC,
        LOWER_SMALL_FREQ_WHEEL_DEC,
        LOWER_LARGE_FREQ_WHEEL_INC,
        LOWER_LARGE_FREQ_WHEEL_DEC,
        LOWER_FREQ_SWITCH
    }

    public class RadioPanelKnobSA342 : ISaitekPanelKnob
    {
        public RadioPanelKnobSA342(int group, int mask, bool isOn, RadioPanelPZ69KnobsSA342 radioPanelPZ69Knob)
        {
            Group = group;
            Mask = mask;
            IsOn = isOn;
            RadioPanelPZ69Knob = radioPanelPZ69Knob;
        }

        public int Group { get; set; }

        public int Mask { get; set; }

        public bool IsOn { get; set; }

        public RadioPanelPZ69KnobsSA342 RadioPanelPZ69Knob { get; set; }
        
        public static HashSet<ISaitekPanelKnob> GetRadioPanelKnobs()
        {
            // true means clockwise turn
            var result = new HashSet<ISaitekPanelKnob>
            {
                // Group 0
                new RadioPanelKnobSA342(2, 1 << 0, true, RadioPanelPZ69KnobsSA342.UPPER_SMALL_FREQ_WHEEL_INC),
                new RadioPanelKnobSA342(2, 1 << 1, false, RadioPanelPZ69KnobsSA342.UPPER_SMALL_FREQ_WHEEL_DEC),
                new RadioPanelKnobSA342(2, 1 << 2, true, RadioPanelPZ69KnobsSA342.UPPER_LARGE_FREQ_WHEEL_INC),
                new RadioPanelKnobSA342(2, 1 << 3, false, RadioPanelPZ69KnobsSA342.UPPER_LARGE_FREQ_WHEEL_DEC),
                new RadioPanelKnobSA342(2, 1 << 4, true, RadioPanelPZ69KnobsSA342.LOWER_SMALL_FREQ_WHEEL_INC),
                new RadioPanelKnobSA342(2, 1 << 5, false, RadioPanelPZ69KnobsSA342.LOWER_SMALL_FREQ_WHEEL_DEC),
                new RadioPanelKnobSA342(2, 1 << 6, true, RadioPanelPZ69KnobsSA342.LOWER_LARGE_FREQ_WHEEL_INC),
                new RadioPanelKnobSA342(2, 1 << 7, false, RadioPanelPZ69KnobsSA342.LOWER_LARGE_FREQ_WHEEL_DEC),
                // Group 1
                new RadioPanelKnobSA342(1, 1 << 0, true, RadioPanelPZ69KnobsSA342.LOWER_VHFFM),
                new RadioPanelKnobSA342(1, 1 << 1, true, RadioPanelPZ69KnobsSA342.LOWER_UHF),
                new RadioPanelKnobSA342(1, 1 << 2, true, RadioPanelPZ69KnobsSA342.LOWER_NAV2),
                new RadioPanelKnobSA342(1, 1 << 3, true, RadioPanelPZ69KnobsSA342.LOWER_ADF),
                new RadioPanelKnobSA342(1, 1 << 4, true, RadioPanelPZ69KnobsSA342.LOWER_NADIR),
                new RadioPanelKnobSA342(1, 1 << 5, true, RadioPanelPZ69KnobsSA342.LOWER_XPDR),
                new RadioPanelKnobSA342(1, 1 << 6, true, RadioPanelPZ69KnobsSA342.UPPER_FREQ_SWITCH),
                new RadioPanelKnobSA342(1, 1 << 7, true, RadioPanelPZ69KnobsSA342.LOWER_FREQ_SWITCH),
                // Group 2
                new RadioPanelKnobSA342(0, 1 << 0, true, RadioPanelPZ69KnobsSA342.UPPER_VHFAM), // UPPER COM 1
                new RadioPanelKnobSA342(0, 1 << 1, true, RadioPanelPZ69KnobsSA342.UPPER_VHFFM), // UPPER COM 2
                new RadioPanelKnobSA342(0, 1 << 2, true, RadioPanelPZ69KnobsSA342.UPPER_UHF), // UPPER NAV 1
                new RadioPanelKnobSA342(0, 1 << 3, true, RadioPanelPZ69KnobsSA342.UPPER_NAV2), // UPPER NAV 2
                new RadioPanelKnobSA342(0, 1 << 4, true, RadioPanelPZ69KnobsSA342.UPPER_ADF), // UPPER ADF
                new RadioPanelKnobSA342(0, 1 << 5, true, RadioPanelPZ69KnobsSA342.UPPER_NADIR), // UPPER DME
                new RadioPanelKnobSA342(0, 1 << 6, true, RadioPanelPZ69KnobsSA342.UPPER_XPDR), // UPPER XPDR
                new RadioPanelKnobSA342(0, 1 << 7, true, RadioPanelPZ69KnobsSA342.LOWER_VHFAM) // LOWER COM 1 
            };

            return result;
        }
    }
}
