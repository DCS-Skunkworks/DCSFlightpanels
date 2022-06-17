//
//  added by Capt Zeen
//

namespace NonVisuals.Radios.Knobs
{
    using System;
    using System.Collections.Generic;

    using MEF;

    using NonVisuals.Interfaces;

    public class RadioPanelKnobFA18C : ISaitekPanelKnob
    {
        public RadioPanelKnobFA18C(int group, int mask, bool isOn, RadioPanelPZ69KnobsFA18C radioPanelPZ69Knob)
        {
            Group = group;
            Mask = mask;
            IsOn = isOn;
            RadioPanelPZ69Knob = radioPanelPZ69Knob;
        }

        public int Group { get; set; }

        public int Mask { get; set; }

        public bool IsOn { get; set; }

        public RadioPanelPZ69KnobsFA18C RadioPanelPZ69Knob { get; set; }

        public static HashSet<ISaitekPanelKnob> GetRadioPanelKnobs()
        {
            // true means clockwise turn
            var result = new HashSet<ISaitekPanelKnob>
            {
                // Group 0
                new RadioPanelKnobFA18C(2, 1 << 0, true, RadioPanelPZ69KnobsFA18C.UPPER_SMALL_FREQ_WHEEL_INC),
                new RadioPanelKnobFA18C(2, 1 << 1, false, RadioPanelPZ69KnobsFA18C.UPPER_SMALL_FREQ_WHEEL_DEC),
                new RadioPanelKnobFA18C(2, 1 << 2, true, RadioPanelPZ69KnobsFA18C.UPPER_LARGE_FREQ_WHEEL_INC),
                new RadioPanelKnobFA18C(2, 1 << 3, false, RadioPanelPZ69KnobsFA18C.UPPER_LARGE_FREQ_WHEEL_DEC),
                new RadioPanelKnobFA18C(2, 1 << 4, true, RadioPanelPZ69KnobsFA18C.LOWER_SMALL_FREQ_WHEEL_INC),
                new RadioPanelKnobFA18C(2, 1 << 5, false, RadioPanelPZ69KnobsFA18C.LOWER_SMALL_FREQ_WHEEL_DEC),
                new RadioPanelKnobFA18C(2, 1 << 6, true, RadioPanelPZ69KnobsFA18C.LOWER_LARGE_FREQ_WHEEL_INC),
                new RadioPanelKnobFA18C(2, 1 << 7, false, RadioPanelPZ69KnobsFA18C.LOWER_LARGE_FREQ_WHEEL_DEC),
                // Group 1
                new RadioPanelKnobFA18C(1, 1 << 0, true, RadioPanelPZ69KnobsFA18C.LOWER_COMM2),
                new RadioPanelKnobFA18C(1, 1 << 1, true, RadioPanelPZ69KnobsFA18C.LOWER_VHFFM),
                new RadioPanelKnobFA18C(1, 1 << 2, true, RadioPanelPZ69KnobsFA18C.LOWER_ILS),
                new RadioPanelKnobFA18C(1, 1 << 3, true, RadioPanelPZ69KnobsFA18C.LOWER_TACAN),
                new RadioPanelKnobFA18C(1, 1 << 4, true, RadioPanelPZ69KnobsFA18C.LOWER_DME),
                new RadioPanelKnobFA18C(1, 1 << 5, true, RadioPanelPZ69KnobsFA18C.LOWER_XPDR),
                new RadioPanelKnobFA18C(1, 1 << 6, true, RadioPanelPZ69KnobsFA18C.UPPER_FREQ_SWITCH),
                new RadioPanelKnobFA18C(1, 1 << 7, true, RadioPanelPZ69KnobsFA18C.LOWER_FREQ_SWITCH),
                // Group 2
                new RadioPanelKnobFA18C(0, 1 << 0, true, RadioPanelPZ69KnobsFA18C.UPPER_COMM1), // UPPER COM 1
                new RadioPanelKnobFA18C(0, 1 << 1, true, RadioPanelPZ69KnobsFA18C.UPPER_COMM2), // UPPER COM 2
                new RadioPanelKnobFA18C(0, 1 << 2, true, RadioPanelPZ69KnobsFA18C.UPPER_VHFFM), // UPPER NAV 1
                new RadioPanelKnobFA18C(0, 1 << 3, true, RadioPanelPZ69KnobsFA18C.UPPER_ILS), // UPPER NAV 2
                new RadioPanelKnobFA18C(0, 1 << 4, true, RadioPanelPZ69KnobsFA18C.UPPER_TACAN), // UPPER ADF
                new RadioPanelKnobFA18C(0, 1 << 5, true, RadioPanelPZ69KnobsFA18C.UPPER_DME), // UPPER DME
                new RadioPanelKnobFA18C(0, 1 << 6, true, RadioPanelPZ69KnobsFA18C.UPPER_XPDR), // UPPER XPDR
                new RadioPanelKnobFA18C(0, 1 << 7, true, RadioPanelPZ69KnobsFA18C.LOWER_COMM1) // LOWER COM 1 
            };

            return result;
        }
    }
}
