using System;
using System.Collections.Generic;

namespace NonVisuals
{

    public enum RadioPanelPZ69Area
    {
        UpperArea = 0,
        LowerArea = 2
    }

    public enum PZ69DialPosition
    {
        UpperCOM1 = 0,
        UpperCOM2 = 2,
        UpperNAV1 = 4,
        UpperNAV2 = 8,
        UpperADF = 16,
        UpperDME = 32,
        UpperXPDR = 64,
        LowerCOM1 = 4096,
        LowerCOM2 = 8192,
        LowerNAV1 = 16384,
        LowerNAV2 = 32768,
        LowerADF = 65536,
        LowerDME = 131072,
        LowerXPDR = 262144
    }

    public enum RadioPanelPZ69KnobsEmulator
    {
        UpperCOM1 = 0,
        UpperCOM2 = 2,
        UpperNAV1 = 4,
        UpperNAV2 = 8,
        UpperADF = 16,
        UpperDME = 32,
        UpperXPDR = 64,
        UpperSmallFreqWheelInc = 128,
        UpperSmallFreqWheelDec = 256,
        UpperLargeFreqWheelInc = 512,
        UpperLargeFreqWheelDec = 1024,
        UpperFreqSwitch = 2056,
        LowerCOM1 = 4096,
        LowerCOM2 = 8192,
        LowerNAV1 = 16384,
        LowerNAV2 = 32768,
        LowerADF = 65536,
        LowerDME = 131072,
        LowerXPDR = 262144,
        LowerSmallFreqWheelInc = 8388608,
        LowerSmallFreqWheelDec = 524288,
        LowerLargeFreqWheelInc = 1048576,
        LowerLargeFreqWheelDec = 2097152,
        LowerFreqSwitch = 4194304
    }

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

    public class RadioPanelPZ69KnobEmulator
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

        public string ExportString()
        {
            return "RadioPanelKnob{" + Enum.GetName(typeof(RadioPanelPZ69KnobsEmulator), RadioPanelPZ69Knob) + "}";
        }

        public void ImportString(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                throw new ArgumentException("Import string empty. (RadioPanelKnob)");
            }
            if (!str.StartsWith("RadioPanelKnob{") || !str.EndsWith("}"))
            {
                throw new ArgumentException("Import string format exception. (RadioPanelKnob) >" + str + "<");
            }
            //RadioPanelKnob{SWITCHKEY_MASTER_ALT}
            var dataString = str.Remove(0, 15);
            //SWITCHKEY_MASTER_ALT}
            dataString = dataString.Remove(dataString.Length - 1, 1);
            //SWITCHKEY_MASTER_ALT
            RadioPanelPZ69Knob = (RadioPanelPZ69KnobsEmulator)Enum.Parse(typeof(RadioPanelPZ69KnobsEmulator), dataString.Trim());
        }

        public static HashSet<RadioPanelPZ69KnobEmulator> GetRadioPanelKnobs()
        {
            //true means clockwise turn
            var result = new HashSet<RadioPanelPZ69KnobEmulator>();
            //Group 0
            result.Add(new RadioPanelPZ69KnobEmulator(2, Convert.ToInt32("1", 2), true, RadioPanelPZ69KnobsEmulator.UpperSmallFreqWheelInc));
            result.Add(new RadioPanelPZ69KnobEmulator(2, Convert.ToInt32("10", 2), false, RadioPanelPZ69KnobsEmulator.UpperSmallFreqWheelDec));
            result.Add(new RadioPanelPZ69KnobEmulator(2, Convert.ToInt32("100", 2), true, RadioPanelPZ69KnobsEmulator.UpperLargeFreqWheelInc));
            result.Add(new RadioPanelPZ69KnobEmulator(2, Convert.ToInt32("1000", 2), false, RadioPanelPZ69KnobsEmulator.UpperLargeFreqWheelDec));
            result.Add(new RadioPanelPZ69KnobEmulator(2, Convert.ToInt32("10000", 2), true, RadioPanelPZ69KnobsEmulator.LowerSmallFreqWheelInc));
            result.Add(new RadioPanelPZ69KnobEmulator(2, Convert.ToInt32("100000", 2), false, RadioPanelPZ69KnobsEmulator.LowerSmallFreqWheelDec));
            result.Add(new RadioPanelPZ69KnobEmulator(2, Convert.ToInt32("1000000", 2), true, RadioPanelPZ69KnobsEmulator.LowerLargeFreqWheelInc));
            result.Add(new RadioPanelPZ69KnobEmulator(2, Convert.ToInt32("10000000", 2), false, RadioPanelPZ69KnobsEmulator.LowerLargeFreqWheelDec));

            //Group 1
            result.Add(new RadioPanelPZ69KnobEmulator(1, Convert.ToInt32("1", 2), true, RadioPanelPZ69KnobsEmulator.LowerCOM2));
            result.Add(new RadioPanelPZ69KnobEmulator(1, Convert.ToInt32("10", 2), true, RadioPanelPZ69KnobsEmulator.LowerNAV1));
            result.Add(new RadioPanelPZ69KnobEmulator(1, Convert.ToInt32("100", 2), true, RadioPanelPZ69KnobsEmulator.LowerNAV2));
            result.Add(new RadioPanelPZ69KnobEmulator(1, Convert.ToInt32("1000", 2), true, RadioPanelPZ69KnobsEmulator.LowerADF));
            result.Add(new RadioPanelPZ69KnobEmulator(1, Convert.ToInt32("10000", 2), true, RadioPanelPZ69KnobsEmulator.LowerDME));
            result.Add(new RadioPanelPZ69KnobEmulator(1, Convert.ToInt32("100000", 2), true, RadioPanelPZ69KnobsEmulator.LowerXPDR));
            result.Add(new RadioPanelPZ69KnobEmulator(1, Convert.ToInt32("1000000", 2), true, RadioPanelPZ69KnobsEmulator.UpperFreqSwitch));
            result.Add(new RadioPanelPZ69KnobEmulator(1, Convert.ToInt32("10000000", 2), true, RadioPanelPZ69KnobsEmulator.LowerFreqSwitch));

            //Group 2
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


        public class RadioPanelPZ69KeyOnOff
        {
            private readonly RadioPanelPZ69KnobsEmulator _radioPanelPZ69Key;
            private readonly bool _on;

            public RadioPanelPZ69KeyOnOff(RadioPanelPZ69KnobsEmulator radioPanelPZ69Key, bool on)
            {
                _radioPanelPZ69Key = radioPanelPZ69Key;
                _on = @on;
            }

            public RadioPanelPZ69KnobsEmulator RadioPanelPZ69Key => _radioPanelPZ69Key;

            public bool On => _on;
        }

    }
}
