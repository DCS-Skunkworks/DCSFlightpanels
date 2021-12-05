namespace NonVisuals.Radios.Knobs
{
    using System;
    using System.Collections.Generic;

    using MEF;

    using NonVisuals.Interfaces;

    public enum CurrentEmulatorRadioMode
    {
        COM1 = 0,
        COM2 = 2,
        NAV1 = 4,
        NAV2 = 8,
        ADF = 16,
        DME = 32,
        XPDR = 64
    }

    public class RadioPanelPZ69KnobEmulator : ISaitekPanelKnob
    {
        public RadioPanelPZ69KnobEmulator(int group, int mask, bool isOn, RadioPanelPZ69KnobsEmulator radioPanelPZ69Knob)
        {
            Group = group;
            Mask = mask;
            IsOn = isOn;
            RadioPanelPZ69Knob = radioPanelPZ69Knob;
        }

        public int Group { get; set; }

        public int Mask { get; set; }

        public bool IsOn { get; set; }

        public RadioPanelPZ69KnobsEmulator RadioPanelPZ69Knob { get; set; }
        
        public static HashSet<ISaitekPanelKnob> GetRadioPanelKnobs()
        {
            // true means clockwise turn
            var result = new HashSet<ISaitekPanelKnob>
            {
                // Group 0
                new RadioPanelPZ69KnobEmulator(2, Convert.ToInt32("1", 2), true, RadioPanelPZ69KnobsEmulator.UpperSmallFreqWheelInc),
                new RadioPanelPZ69KnobEmulator(2, Convert.ToInt32("10", 2), false, RadioPanelPZ69KnobsEmulator.UpperSmallFreqWheelDec),
                new RadioPanelPZ69KnobEmulator(2, Convert.ToInt32("100", 2), true, RadioPanelPZ69KnobsEmulator.UpperLargeFreqWheelInc),
                new RadioPanelPZ69KnobEmulator(2, Convert.ToInt32("1000", 2), false, RadioPanelPZ69KnobsEmulator.UpperLargeFreqWheelDec),
                new RadioPanelPZ69KnobEmulator(2, Convert.ToInt32("10000", 2), true, RadioPanelPZ69KnobsEmulator.LowerSmallFreqWheelInc),
                new RadioPanelPZ69KnobEmulator(2, Convert.ToInt32("100000", 2), false, RadioPanelPZ69KnobsEmulator.LowerSmallFreqWheelDec),
                new RadioPanelPZ69KnobEmulator(2, Convert.ToInt32("1000000", 2), true, RadioPanelPZ69KnobsEmulator.LowerLargeFreqWheelInc),
                new RadioPanelPZ69KnobEmulator(2, Convert.ToInt32("10000000", 2), false, RadioPanelPZ69KnobsEmulator.LowerLargeFreqWheelDec),
                // Group 1
                new RadioPanelPZ69KnobEmulator(1, Convert.ToInt32("1", 2), true, RadioPanelPZ69KnobsEmulator.LowerCOM2),
                new RadioPanelPZ69KnobEmulator(1, Convert.ToInt32("10", 2), true, RadioPanelPZ69KnobsEmulator.LowerNAV1),
                new RadioPanelPZ69KnobEmulator(1, Convert.ToInt32("100", 2), true, RadioPanelPZ69KnobsEmulator.LowerNAV2),
                new RadioPanelPZ69KnobEmulator(1, Convert.ToInt32("1000", 2), true, RadioPanelPZ69KnobsEmulator.LowerADF),
                new RadioPanelPZ69KnobEmulator(1, Convert.ToInt32("10000", 2), true, RadioPanelPZ69KnobsEmulator.LowerDME),
                new RadioPanelPZ69KnobEmulator(1, Convert.ToInt32("100000", 2), true, RadioPanelPZ69KnobsEmulator.LowerXPDR),
                new RadioPanelPZ69KnobEmulator(1, Convert.ToInt32("1000000", 2), true, RadioPanelPZ69KnobsEmulator.UpperFreqSwitch),
                new RadioPanelPZ69KnobEmulator(1, Convert.ToInt32("10000000", 2), true, RadioPanelPZ69KnobsEmulator.LowerFreqSwitch),
                // Group 2
                new RadioPanelPZ69KnobEmulator(0, Convert.ToInt32("1", 2), true, RadioPanelPZ69KnobsEmulator.UpperCOM1),
                new RadioPanelPZ69KnobEmulator(0, Convert.ToInt32("10", 2), true, RadioPanelPZ69KnobsEmulator.UpperCOM2),
                new RadioPanelPZ69KnobEmulator(0, Convert.ToInt32("100", 2), true, RadioPanelPZ69KnobsEmulator.UpperNAV1),
                new RadioPanelPZ69KnobEmulator(0, Convert.ToInt32("1000", 2), true, RadioPanelPZ69KnobsEmulator.UpperNAV2),
                new RadioPanelPZ69KnobEmulator(0, Convert.ToInt32("10000", 2), true, RadioPanelPZ69KnobsEmulator.UpperADF),
                new RadioPanelPZ69KnobEmulator(0, Convert.ToInt32("100000", 2), true, RadioPanelPZ69KnobsEmulator.UpperDME),
                new RadioPanelPZ69KnobEmulator(0, Convert.ToInt32("1000000", 2), true, RadioPanelPZ69KnobsEmulator.UpperXPDR),
                new RadioPanelPZ69KnobEmulator(0, Convert.ToInt32("10000000", 2), true, RadioPanelPZ69KnobsEmulator.LowerCOM1)
            };

            return result;
        }
    }
}
