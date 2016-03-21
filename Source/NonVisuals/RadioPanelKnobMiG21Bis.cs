﻿using System;
using System.Collections.Generic;

namespace NonVisuals
{
    public enum RadioPanelPZ69KnobsMiG21Bis
    {
        UpperRadio = 0,
        UpperCom2 = 2,
        UpperRsbn = 4,
        UpperNav2 = 8,
        UpperArc = 16,
        UpperDme = 32,
        UpperXpdr = 64,
        UpperSmallFreqWheelInc = 128,
        UpperSmallFreqWheelDec = 256,
        UpperLargeFreqWheelInc = 512,
        UpperLargeFreqWheelDec = 1024,
        UpperFreqSwitch = 2056,
        LowerRadio = 4096,
        LowerCom2 = 8192,
        LowerRsbn = 16384,
        LowerNav2 = 32768,
        LowerArc = 65536,
        LowerDme = 131072,
        LowerXpdr = 262144,
        LowerSmallFreqWheelInc = 8388608,
        LowerSmallFreqWheelDec = 524288,
        LowerLargeFreqWheelInc = 1048576,
        LowerLargeFreqWheelDec = 2097152,
        LowerFreqSwitch = 4194304
    }

    public enum CurrentMiG21BisRadioMode
    {
        Radio = 0,
        RSBN = 2,
        ARC = 4
    }

    public class RadioPanelKnobMiG21Bis
    {
        public RadioPanelKnobMiG21Bis(int group, int mask, bool isOn, RadioPanelPZ69KnobsMiG21Bis radioPanelPZ69Knob)
        {
            Group = group;
            Mask = mask;
            IsOn = isOn;
            RadioPanelPZ69Knob = radioPanelPZ69Knob;
        }

        public int Group { get; set; }

        public int Mask { get; set; }

        public bool IsOn { get; set; }

        public RadioPanelPZ69KnobsMiG21Bis RadioPanelPZ69Knob { get; set; }

        public string ExportString()
        {
            return "RadioPanelKnob{" + Enum.GetName(typeof(RadioPanelPZ69KnobsMiG21Bis), RadioPanelPZ69Knob) + "}";
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
            RadioPanelPZ69Knob = (RadioPanelPZ69KnobsMiG21Bis)Enum.Parse(typeof(RadioPanelPZ69KnobsMiG21Bis), dataString.Trim());
        }

        public static HashSet<RadioPanelKnobMiG21Bis> GetRadioPanelKnobs()
        {
            //true means clockwise turn
            var result = new HashSet<RadioPanelKnobMiG21Bis>();
            //Group 0
            result.Add(new RadioPanelKnobMiG21Bis(2, Convert.ToInt32("1", 2), true, RadioPanelPZ69KnobsMiG21Bis.UpperSmallFreqWheelInc));
            result.Add(new RadioPanelKnobMiG21Bis(2, Convert.ToInt32("10", 2), false, RadioPanelPZ69KnobsMiG21Bis.UpperSmallFreqWheelDec));
            result.Add(new RadioPanelKnobMiG21Bis(2, Convert.ToInt32("100", 2), true, RadioPanelPZ69KnobsMiG21Bis.UpperLargeFreqWheelInc));
            result.Add(new RadioPanelKnobMiG21Bis(2, Convert.ToInt32("1000", 2), false, RadioPanelPZ69KnobsMiG21Bis.UpperLargeFreqWheelDec));
            result.Add(new RadioPanelKnobMiG21Bis(2, Convert.ToInt32("10000", 2), true, RadioPanelPZ69KnobsMiG21Bis.LowerSmallFreqWheelInc));
            result.Add(new RadioPanelKnobMiG21Bis(2, Convert.ToInt32("100000", 2), false, RadioPanelPZ69KnobsMiG21Bis.LowerSmallFreqWheelDec));
            result.Add(new RadioPanelKnobMiG21Bis(2, Convert.ToInt32("1000000", 2), true, RadioPanelPZ69KnobsMiG21Bis.LowerLargeFreqWheelInc));
            result.Add(new RadioPanelKnobMiG21Bis(2, Convert.ToInt32("10000000", 2), false, RadioPanelPZ69KnobsMiG21Bis.LowerLargeFreqWheelDec));

            //Group 1
            result.Add(new RadioPanelKnobMiG21Bis(1, Convert.ToInt32("1", 2), true, RadioPanelPZ69KnobsMiG21Bis.LowerCom2));
            result.Add(new RadioPanelKnobMiG21Bis(1, Convert.ToInt32("10", 2), true, RadioPanelPZ69KnobsMiG21Bis.LowerRsbn));
            result.Add(new RadioPanelKnobMiG21Bis(1, Convert.ToInt32("100", 2), true, RadioPanelPZ69KnobsMiG21Bis.LowerNav2));
            result.Add(new RadioPanelKnobMiG21Bis(1, Convert.ToInt32("1000", 2), true, RadioPanelPZ69KnobsMiG21Bis.LowerArc));
            result.Add(new RadioPanelKnobMiG21Bis(1, Convert.ToInt32("10000", 2), true, RadioPanelPZ69KnobsMiG21Bis.LowerDme));
            result.Add(new RadioPanelKnobMiG21Bis(1, Convert.ToInt32("100000", 2), true, RadioPanelPZ69KnobsMiG21Bis.LowerXpdr));
            result.Add(new RadioPanelKnobMiG21Bis(1, Convert.ToInt32("1000000", 2), true, RadioPanelPZ69KnobsMiG21Bis.UpperFreqSwitch));
            result.Add(new RadioPanelKnobMiG21Bis(1, Convert.ToInt32("10000000", 2), true, RadioPanelPZ69KnobsMiG21Bis.LowerFreqSwitch));

            //Group 2
            result.Add(new RadioPanelKnobMiG21Bis(0, Convert.ToInt32("1", 2), true, RadioPanelPZ69KnobsMiG21Bis.UpperRadio));
            result.Add(new RadioPanelKnobMiG21Bis(0, Convert.ToInt32("10", 2), true, RadioPanelPZ69KnobsMiG21Bis.UpperCom2));
            result.Add(new RadioPanelKnobMiG21Bis(0, Convert.ToInt32("100", 2), true, RadioPanelPZ69KnobsMiG21Bis.UpperRsbn));
            result.Add(new RadioPanelKnobMiG21Bis(0, Convert.ToInt32("1000", 2), true, RadioPanelPZ69KnobsMiG21Bis.UpperNav2));
            result.Add(new RadioPanelKnobMiG21Bis(0, Convert.ToInt32("10000", 2), true, RadioPanelPZ69KnobsMiG21Bis.UpperArc));
            result.Add(new RadioPanelKnobMiG21Bis(0, Convert.ToInt32("100000", 2), true, RadioPanelPZ69KnobsMiG21Bis.UpperDme));
            result.Add(new RadioPanelKnobMiG21Bis(0, Convert.ToInt32("1000000", 2), true, RadioPanelPZ69KnobsMiG21Bis.UpperXpdr));
            result.Add(new RadioPanelKnobMiG21Bis(0, Convert.ToInt32("10000000", 2), true, RadioPanelPZ69KnobsMiG21Bis.LowerRadio));
            return result;
        }




    }
}
