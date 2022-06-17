namespace NonVisuals.Radios.Knobs
{
    using System;
    using System.Collections.Generic;

    using MEF;

    using NonVisuals.Interfaces;

    public class RadioPanelKnobF86F : ISaitekPanelKnob
    {
        public RadioPanelKnobF86F(int group, int mask, bool isOn, RadioPanelPZ69KnobsF86F radioPanelPZ69Knob)
        {
            Group = group;
            Mask = mask;
            IsOn = isOn;
            RadioPanelPZ69Knob = radioPanelPZ69Knob;
        }

        public int Group { get; set; }

        public int Mask { get; set; }

        public bool IsOn { get; set; }

        public RadioPanelPZ69KnobsF86F RadioPanelPZ69Knob { get; set; }

        public static HashSet<ISaitekPanelKnob> GetRadioPanelKnobs()
        {
            // true means clockwise turn
            var result = new HashSet<ISaitekPanelKnob>
            {
                // Group 0
                new RadioPanelKnobF86F(2, 1 << 0, true, RadioPanelPZ69KnobsF86F.UPPER_SMALL_FREQ_WHEEL_INC),
                new RadioPanelKnobF86F(2, 1 << 1, false, RadioPanelPZ69KnobsF86F.UPPER_SMALL_FREQ_WHEEL_DEC),
                new RadioPanelKnobF86F(2, 1 << 2, true, RadioPanelPZ69KnobsF86F.UPPER_LARGE_FREQ_WHEEL_INC),
                new RadioPanelKnobF86F(2, 1 << 3, false, RadioPanelPZ69KnobsF86F.UPPER_LARGE_FREQ_WHEEL_DEC),
                new RadioPanelKnobF86F(2, 1 << 4, true, RadioPanelPZ69KnobsF86F.LOWER_SMALL_FREQ_WHEEL_INC),
                new RadioPanelKnobF86F(2, 1 << 5, false, RadioPanelPZ69KnobsF86F.LOWER_SMALL_FREQ_WHEEL_DEC),
                new RadioPanelKnobF86F(2, 1 << 6, true, RadioPanelPZ69KnobsF86F.LOWER_LARGE_FREQ_WHEEL_INC),
                new RadioPanelKnobF86F(2, 1 << 7, false, RadioPanelPZ69KnobsF86F.LOWER_LARGE_FREQ_WHEEL_DEC),
                // Group 1
                new RadioPanelKnobF86F(1, 1 << 0, true, RadioPanelPZ69KnobsF86F.LOWER_ARC27_VOL), // LOWER COM2
                new RadioPanelKnobF86F(1, 1 << 1, true, RadioPanelPZ69KnobsF86F.LOWER_ARN6), // LOWER NAV1
                new RadioPanelKnobF86F(1, 1 << 2, true, RadioPanelPZ69KnobsF86F.LOWER_ARN6_MODES), // LOWER NAV2
                new RadioPanelKnobF86F(1, 1 << 3, true, RadioPanelPZ69KnobsF86F.LOWER_ADF_APX6), // LOWER ADF
                new RadioPanelKnobF86F(1, 1 << 4, true, RadioPanelPZ69KnobsF86F.LOWER_NO_USE1), // LOWER DME
                new RadioPanelKnobF86F(1, 1 << 5, true, RadioPanelPZ69KnobsF86F.LOWER_NO_USE2), // LOWER XPDR
                new RadioPanelKnobF86F(1, 1 << 6, true, RadioPanelPZ69KnobsF86F.UPPER_FREQ_SWITCH),
                new RadioPanelKnobF86F(1, 1 << 7, true, RadioPanelPZ69KnobsF86F.LOWER_FREQ_SWITCH),
                // Group 2
                new RadioPanelKnobF86F(0, 1 << 0, true, RadioPanelPZ69KnobsF86F.UPPER_ARC27_PRESET), // UPPER COM1
                new RadioPanelKnobF86F(0, 1 << 1, true, RadioPanelPZ69KnobsF86F.UPPER_ARC27_VOL), // UPPER COM2
                new RadioPanelKnobF86F(0, 1 << 2, true, RadioPanelPZ69KnobsF86F.UPPER_ARN6), // UPPER NAV1
                new RadioPanelKnobF86F(0, 1 << 3, true, RadioPanelPZ69KnobsF86F.UPPER_ARN6_MODES), // UPPER NAV2
                new RadioPanelKnobF86F(0, 1 << 4, true, RadioPanelPZ69KnobsF86F.UPPER_ADF_APX6), // UPPER ADF
                new RadioPanelKnobF86F(0, 1 << 5, true, RadioPanelPZ69KnobsF86F.UPPER_NO_USE1), // UPPER DME
                new RadioPanelKnobF86F(0, 1 << 6, true, RadioPanelPZ69KnobsF86F.UPPER_NO_USE2), // UPPER XPDR
                new RadioPanelKnobF86F(0, 1 << 7, true, RadioPanelPZ69KnobsF86F.LOWER_ARC27_PRESET) // LOWER COM1
            };

            return result;
        }
    }


}
