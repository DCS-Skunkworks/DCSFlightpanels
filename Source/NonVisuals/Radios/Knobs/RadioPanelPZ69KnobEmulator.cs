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
            var result = new HashSet<ISaitekPanelKnob>();

            // Group 0
            result.Add(new RadioPanelPZ69KnobEmulator(2, Convert.ToInt32("1", 2), true, RadioPanelPZ69KnobsEmulator.UpperSmallFreqWheelInc));
            result.Add(new RadioPanelPZ69KnobEmulator(2, Convert.ToInt32("10", 2), false, RadioPanelPZ69KnobsEmulator.UpperSmallFreqWheelDec));
            result.Add(new RadioPanelPZ69KnobEmulator(2, Convert.ToInt32("100", 2), true, RadioPanelPZ69KnobsEmulator.UpperLargeFreqWheelInc));
            result.Add(new RadioPanelPZ69KnobEmulator(2, Convert.ToInt32("1000", 2), false, RadioPanelPZ69KnobsEmulator.UpperLargeFreqWheelDec));
            result.Add(new RadioPanelPZ69KnobEmulator(2, Convert.ToInt32("10000", 2), true, RadioPanelPZ69KnobsEmulator.LowerSmallFreqWheelInc));
            result.Add(new RadioPanelPZ69KnobEmulator(2, Convert.ToInt32("100000", 2), false, RadioPanelPZ69KnobsEmulator.LowerSmallFreqWheelDec));
            result.Add(new RadioPanelPZ69KnobEmulator(2, Convert.ToInt32("1000000", 2), true, RadioPanelPZ69KnobsEmulator.LowerLargeFreqWheelInc));
            result.Add(new RadioPanelPZ69KnobEmulator(2, Convert.ToInt32("10000000", 2), false, RadioPanelPZ69KnobsEmulator.LowerLargeFreqWheelDec));

            // Group 1
            result.Add(new RadioPanelPZ69KnobEmulator(1, Convert.ToInt32("1", 2), true, RadioPanelPZ69KnobsEmulator.LowerCOM2));
            result.Add(new RadioPanelPZ69KnobEmulator(1, Convert.ToInt32("10", 2), true, RadioPanelPZ69KnobsEmulator.LowerNAV1));
            result.Add(new RadioPanelPZ69KnobEmulator(1, Convert.ToInt32("100", 2), true, RadioPanelPZ69KnobsEmulator.LowerNAV2));
            result.Add(new RadioPanelPZ69KnobEmulator(1, Convert.ToInt32("1000", 2), true, RadioPanelPZ69KnobsEmulator.LowerADF));
            result.Add(new RadioPanelPZ69KnobEmulator(1, Convert.ToInt32("10000", 2), true, RadioPanelPZ69KnobsEmulator.LowerDME));
            result.Add(new RadioPanelPZ69KnobEmulator(1, Convert.ToInt32("100000", 2), true, RadioPanelPZ69KnobsEmulator.LowerXPDR));
            result.Add(new RadioPanelPZ69KnobEmulator(1, Convert.ToInt32("1000000", 2), true, RadioPanelPZ69KnobsEmulator.UpperFreqSwitch));
            result.Add(new RadioPanelPZ69KnobEmulator(1, Convert.ToInt32("10000000", 2), true, RadioPanelPZ69KnobsEmulator.LowerFreqSwitch));

            // Group 2
            result.Add(new RadioPanelPZ69KnobEmulator(0, Convert.ToInt32("1", 2), true, RadioPanelPZ69KnobsEmulator.UpperCOM1));
            result.Add(new RadioPanelPZ69KnobEmulator(0, Convert.ToInt32("10", 2), true, RadioPanelPZ69KnobsEmulator.UpperCOM2));
            result.Add(new RadioPanelPZ69KnobEmulator(0, Convert.ToInt32("100", 2), true, RadioPanelPZ69KnobsEmulator.UpperNAV1));
            result.Add(new RadioPanelPZ69KnobEmulator(0, Convert.ToInt32("1000", 2), true, RadioPanelPZ69KnobsEmulator.UpperNAV2));
            result.Add(new RadioPanelPZ69KnobEmulator(0, Convert.ToInt32("10000", 2), true, RadioPanelPZ69KnobsEmulator.UpperADF));
            result.Add(new RadioPanelPZ69KnobEmulator(0, Convert.ToInt32("100000", 2), true, RadioPanelPZ69KnobsEmulator.UpperDME));
            result.Add(new RadioPanelPZ69KnobEmulator(0, Convert.ToInt32("1000000", 2), true, RadioPanelPZ69KnobsEmulator.UpperXPDR));
            result.Add(new RadioPanelPZ69KnobEmulator(0, Convert.ToInt32("10000000", 2), true, RadioPanelPZ69KnobsEmulator.LowerCOM1));
            return result;
        }
    }
}
