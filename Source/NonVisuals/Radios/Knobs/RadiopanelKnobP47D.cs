namespace NonVisuals.Radios.Knobs
{
    using System;
    using System.Collections.Generic;

    using MEF;

    using NonVisuals.Interfaces;

    public class RadioPanelKnobP47D : ISaitekPanelKnob
    {
        public RadioPanelKnobP47D(int group, int mask, bool isOn, RadioPanelPZ69KnobsP47D radioPanelPZ69Knob)
        {
            Group = group;
            Mask = mask;
            IsOn = isOn;
            RadioPanelPZ69Knob = radioPanelPZ69Knob;
        }

        public int Group { get; set; }

        public int Mask { get; set; }

        public bool IsOn { get; set; }

        public RadioPanelPZ69KnobsP47D RadioPanelPZ69Knob { get; set; }
        
        public static HashSet<ISaitekPanelKnob> GetRadioPanelKnobs()
        {
            // true means clockwise turn
            var result = new HashSet<ISaitekPanelKnob>
            {
                // Group 0
                new RadioPanelKnobP47D(2, 1 << 0, true, RadioPanelPZ69KnobsP47D.UPPER_SMALL_FREQ_WHEEL_INC),
                new RadioPanelKnobP47D(2, 1 << 1, false, RadioPanelPZ69KnobsP47D.UPPER_SMALL_FREQ_WHEEL_DEC),
                new RadioPanelKnobP47D(2, 1 << 2, true, RadioPanelPZ69KnobsP47D.UPPER_LARGE_FREQ_WHEEL_INC),
                new RadioPanelKnobP47D(2, 1 << 3, false, RadioPanelPZ69KnobsP47D.UPPER_LARGE_FREQ_WHEEL_DEC),
                new RadioPanelKnobP47D(2, 1 << 4, true, RadioPanelPZ69KnobsP47D.LOWER_SMALL_FREQ_WHEEL_INC),
                new RadioPanelKnobP47D(2, 1 << 5, false, RadioPanelPZ69KnobsP47D.LOWER_SMALL_FREQ_WHEEL_DEC),
                new RadioPanelKnobP47D(2, 1 << 6, true, RadioPanelPZ69KnobsP47D.LOWER_LARGE_FREQ_WHEEL_INC),
                new RadioPanelKnobP47D(2, 1 << 7, false, RadioPanelPZ69KnobsP47D.LOWER_LARGE_FREQ_WHEEL_DEC),
                // Group 1
                new RadioPanelKnobP47D(1, 1 << 0, true, RadioPanelPZ69KnobsP47D.LOWER_NO_USE5), // LOWER_IFF)); //LOWER COM2
                new RadioPanelKnobP47D(1, 1 << 1, true, RadioPanelPZ69KnobsP47D.LOWER_NO_USE0), // LOWER NAV1
                new RadioPanelKnobP47D(1, 1 << 2, true, RadioPanelPZ69KnobsP47D.LOWER_NO_USE1), // LOWER NAV2
                new RadioPanelKnobP47D(1, 1 << 3, true, RadioPanelPZ69KnobsP47D.LOWER_NO_USE2), // LOWER ADF
                new RadioPanelKnobP47D(1, 1 << 4, true, RadioPanelPZ69KnobsP47D.LOWER_NO_USE3), // LOWER DME
                new RadioPanelKnobP47D(1, 1 << 5, true, RadioPanelPZ69KnobsP47D.LOWER_NO_USE4), // LOWER XPDR
                new RadioPanelKnobP47D(1, 1 << 6, true, RadioPanelPZ69KnobsP47D.UPPER_FREQ_SWITCH),
                new RadioPanelKnobP47D(1, 1 << 7, true, RadioPanelPZ69KnobsP47D.LOWER_FREQ_SWITCH),
                // Group 2
                new RadioPanelKnobP47D(0, 1 << 0, true, RadioPanelPZ69KnobsP47D.UPPER_HFRADIO), // UPPER COM1
                new RadioPanelKnobP47D(0, 1 << 1, true, RadioPanelPZ69KnobsP47D.UPPER_NO_USE5), // UPPER_IFF)); //UPPER COM2
                new RadioPanelKnobP47D(0, 1 << 2, true, RadioPanelPZ69KnobsP47D.UPPER_NO_USE0), // UPPER NAV1
                new RadioPanelKnobP47D(0, 1 << 3, true, RadioPanelPZ69KnobsP47D.UPPER_NO_USE1), // UPPER NAV2
                new RadioPanelKnobP47D(0, 1 << 4, true, RadioPanelPZ69KnobsP47D.UPPER_NO_USE2), // UPPER ADF
                new RadioPanelKnobP47D(0, 1 << 5, true, RadioPanelPZ69KnobsP47D.UPPER_NO_USE3), // UPPER DME
                new RadioPanelKnobP47D(0, 1 << 6, true, RadioPanelPZ69KnobsP47D.UPPER_NO_USE4), // UPPER XPDR
                new RadioPanelKnobP47D(0, 1 << 7, true, RadioPanelPZ69KnobsP47D.LOWER_HFRADIO) // LOWER COM1
            };

            return result;
        }
    }
}
