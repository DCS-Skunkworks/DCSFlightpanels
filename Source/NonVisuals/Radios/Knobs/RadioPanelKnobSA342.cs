namespace NonVisuals.Radios.Knobs
{
    using System;
    using System.Collections.Generic;

    using MEF;

    using NonVisuals.Interfaces;

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
                new RadioPanelKnobSA342(2, Convert.ToInt32("1", 2), true, RadioPanelPZ69KnobsSA342.UPPER_SMALL_FREQ_WHEEL_INC),
                new RadioPanelKnobSA342(2, Convert.ToInt32("10", 2), false, RadioPanelPZ69KnobsSA342.UPPER_SMALL_FREQ_WHEEL_DEC),
                new RadioPanelKnobSA342(2, Convert.ToInt32("100", 2), true, RadioPanelPZ69KnobsSA342.UPPER_LARGE_FREQ_WHEEL_INC),
                new RadioPanelKnobSA342(2, Convert.ToInt32("1000", 2), false, RadioPanelPZ69KnobsSA342.UPPER_LARGE_FREQ_WHEEL_DEC),
                new RadioPanelKnobSA342(2, Convert.ToInt32("10000", 2), true, RadioPanelPZ69KnobsSA342.LOWER_SMALL_FREQ_WHEEL_INC),
                new RadioPanelKnobSA342(2, Convert.ToInt32("100000", 2), false, RadioPanelPZ69KnobsSA342.LOWER_SMALL_FREQ_WHEEL_DEC),
                new RadioPanelKnobSA342(2, Convert.ToInt32("1000000", 2), true, RadioPanelPZ69KnobsSA342.LOWER_LARGE_FREQ_WHEEL_INC),
                new RadioPanelKnobSA342(2, Convert.ToInt32("10000000", 2), false, RadioPanelPZ69KnobsSA342.LOWER_LARGE_FREQ_WHEEL_DEC),
                // Group 1
                new RadioPanelKnobSA342(1, Convert.ToInt32("1", 2), true, RadioPanelPZ69KnobsSA342.LOWER_VHFFM),
                new RadioPanelKnobSA342(1, Convert.ToInt32("10", 2), true, RadioPanelPZ69KnobsSA342.LOWER_UHF),
                new RadioPanelKnobSA342(1, Convert.ToInt32("100", 2), true, RadioPanelPZ69KnobsSA342.LOWER_NAV2),
                new RadioPanelKnobSA342(1, Convert.ToInt32("1000", 2), true, RadioPanelPZ69KnobsSA342.LOWER_ADF),
                new RadioPanelKnobSA342(1, Convert.ToInt32("10000", 2), true, RadioPanelPZ69KnobsSA342.LOWER_NADIR),
                new RadioPanelKnobSA342(1, Convert.ToInt32("100000", 2), true, RadioPanelPZ69KnobsSA342.LOWER_XPDR),
                new RadioPanelKnobSA342(1, Convert.ToInt32("1000000", 2), true, RadioPanelPZ69KnobsSA342.UPPER_FREQ_SWITCH),
                new RadioPanelKnobSA342(1, Convert.ToInt32("10000000", 2), true, RadioPanelPZ69KnobsSA342.LOWER_FREQ_SWITCH),
                // Group 2
                new RadioPanelKnobSA342(0, Convert.ToInt32("1", 2), true, RadioPanelPZ69KnobsSA342.UPPER_VHFAM), // UPPER COM 1
                new RadioPanelKnobSA342(0, Convert.ToInt32("10", 2), true, RadioPanelPZ69KnobsSA342.UPPER_VHFFM), // UPPER COM 2
                new RadioPanelKnobSA342(0, Convert.ToInt32("100", 2), true, RadioPanelPZ69KnobsSA342.UPPER_UHF), // UPPER NAV 1
                new RadioPanelKnobSA342(0, Convert.ToInt32("1000", 2), true, RadioPanelPZ69KnobsSA342.UPPER_NAV2), // UPPER NAV 2
                new RadioPanelKnobSA342(0, Convert.ToInt32("10000", 2), true, RadioPanelPZ69KnobsSA342.UPPER_ADF), // UPPER ADF
                new RadioPanelKnobSA342(0, Convert.ToInt32("100000", 2), true, RadioPanelPZ69KnobsSA342.UPPER_NADIR), // UPPER DME
                new RadioPanelKnobSA342(0, Convert.ToInt32("1000000", 2), true, RadioPanelPZ69KnobsSA342.UPPER_XPDR), // UPPER XPDR
                new RadioPanelKnobSA342(0, Convert.ToInt32("10000000", 2), true, RadioPanelPZ69KnobsSA342.LOWER_VHFAM) // LOWER COM 1 
            };

            return result;
        }
    }
}
