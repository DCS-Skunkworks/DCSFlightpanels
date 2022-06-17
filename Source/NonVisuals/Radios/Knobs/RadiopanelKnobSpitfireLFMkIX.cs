namespace NonVisuals.Radios.Knobs
{
    using System;
    using System.Collections.Generic;

    using MEF;

    using NonVisuals.Interfaces;

    public class RadioPanelKnobSpitfireLFMkIX : ISaitekPanelKnob
    {
        public RadioPanelKnobSpitfireLFMkIX(int group, int mask, bool isOn, RadioPanelPZ69KnobsSpitfireLFMkIX radioPanelPZ69Knob)
        {
            Group = group;
            Mask = mask;
            IsOn = isOn;
            RadioPanelPZ69Knob = radioPanelPZ69Knob;
        }

        public int Group { get; set; }

        public int Mask { get; set; }

        public bool IsOn { get; set; }

        public RadioPanelPZ69KnobsSpitfireLFMkIX RadioPanelPZ69Knob { get; set; }
        
        public static HashSet<ISaitekPanelKnob> GetRadioPanelKnobs()
        {
            // true means clockwise turn
            var result = new HashSet<ISaitekPanelKnob>
            {
                // Group 0
                new RadioPanelKnobSpitfireLFMkIX(2, 1 << 0, true, RadioPanelPZ69KnobsSpitfireLFMkIX.UPPER_SMALL_FREQ_WHEEL_INC),
                new RadioPanelKnobSpitfireLFMkIX(2, 1 << 1, false, RadioPanelPZ69KnobsSpitfireLFMkIX.UPPER_SMALL_FREQ_WHEEL_DEC),
                new RadioPanelKnobSpitfireLFMkIX(2, 1 << 2, true, RadioPanelPZ69KnobsSpitfireLFMkIX.UPPER_LARGE_FREQ_WHEEL_INC),
                new RadioPanelKnobSpitfireLFMkIX(2, 1 << 3, false, RadioPanelPZ69KnobsSpitfireLFMkIX.UPPER_LARGE_FREQ_WHEEL_DEC),
                new RadioPanelKnobSpitfireLFMkIX(2, 1 << 4, true, RadioPanelPZ69KnobsSpitfireLFMkIX.LOWER_SMALL_FREQ_WHEEL_INC),
                new RadioPanelKnobSpitfireLFMkIX(2, 1 << 5, false, RadioPanelPZ69KnobsSpitfireLFMkIX.LOWER_SMALL_FREQ_WHEEL_DEC),
                new RadioPanelKnobSpitfireLFMkIX(2, 1 << 6, true, RadioPanelPZ69KnobsSpitfireLFMkIX.LOWER_LARGE_FREQ_WHEEL_INC),
                new RadioPanelKnobSpitfireLFMkIX(2, 1 << 7, false, RadioPanelPZ69KnobsSpitfireLFMkIX.LOWER_LARGE_FREQ_WHEEL_DEC),
                // Group 1
                new RadioPanelKnobSpitfireLFMkIX(1, 1 << 0, true, RadioPanelPZ69KnobsSpitfireLFMkIX.LOWER_IFF), // LOWER COM2
                new RadioPanelKnobSpitfireLFMkIX(1, 1 << 1, true, RadioPanelPZ69KnobsSpitfireLFMkIX.LOWER_NO_USE0), // LOWER NAV1
                new RadioPanelKnobSpitfireLFMkIX(1, 1 << 2, true, RadioPanelPZ69KnobsSpitfireLFMkIX.LOWER_NO_USE1), // LOWER NAV2
                new RadioPanelKnobSpitfireLFMkIX(1, 1 << 3, true, RadioPanelPZ69KnobsSpitfireLFMkIX.LOWER_NO_USE2), // LOWER ADF
                new RadioPanelKnobSpitfireLFMkIX(1, 1 << 4, true, RadioPanelPZ69KnobsSpitfireLFMkIX.LOWER_NO_USE3), // LOWER DME
                new RadioPanelKnobSpitfireLFMkIX(1, 1 << 5, true, RadioPanelPZ69KnobsSpitfireLFMkIX.LOWER_NO_USE4), // LOWER XPDR
                new RadioPanelKnobSpitfireLFMkIX(1, 1 << 6, true, RadioPanelPZ69KnobsSpitfireLFMkIX.UPPER_FREQ_SWITCH),
                new RadioPanelKnobSpitfireLFMkIX(1, 1 << 7, true, RadioPanelPZ69KnobsSpitfireLFMkIX.LOWER_FREQ_SWITCH),
                // Group 2
                new RadioPanelKnobSpitfireLFMkIX(0, 1 << 0, true, RadioPanelPZ69KnobsSpitfireLFMkIX.UPPER_HFRADIO), // UPPER COM1
                new RadioPanelKnobSpitfireLFMkIX(0, 1 << 1, true, RadioPanelPZ69KnobsSpitfireLFMkIX.UPPER_IFF), // UPPER COM2
                new RadioPanelKnobSpitfireLFMkIX(0, 1 << 2, true, RadioPanelPZ69KnobsSpitfireLFMkIX.UPPER_NO_USE0), // UPPER NAV1
                new RadioPanelKnobSpitfireLFMkIX(0, 1 << 3, true, RadioPanelPZ69KnobsSpitfireLFMkIX.UPPER_NO_USE1), // UPPER NAV2
                new RadioPanelKnobSpitfireLFMkIX(0, 1 << 4, true, RadioPanelPZ69KnobsSpitfireLFMkIX.UPPER_NO_USE2), // UPPER ADF
                new RadioPanelKnobSpitfireLFMkIX(0, 1 << 5, true, RadioPanelPZ69KnobsSpitfireLFMkIX.UPPER_NO_USE3), // UPPER DME
                new RadioPanelKnobSpitfireLFMkIX(0, 1 << 6, true, RadioPanelPZ69KnobsSpitfireLFMkIX.UPPER_NO_USE4), // UPPER XPDR
                new RadioPanelKnobSpitfireLFMkIX(0, 1 << 7, true, RadioPanelPZ69KnobsSpitfireLFMkIX.LOWER_HFRADIO) // LOWER COM1
            };

            return result;
        }
    }
}
