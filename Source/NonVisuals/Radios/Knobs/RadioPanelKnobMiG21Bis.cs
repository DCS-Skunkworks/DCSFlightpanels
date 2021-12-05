namespace NonVisuals.Radios.Knobs
{
    using System;
    using System.Collections.Generic;

    using MEF;

    using NonVisuals.Interfaces;

    public class RadioPanelKnobMiG21Bis : ISaitekPanelKnob
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

        public static HashSet<ISaitekPanelKnob> GetRadioPanelKnobs()
        {
            // true means clockwise turn
            var result = new HashSet<ISaitekPanelKnob>
            {
                // Group 0
                new RadioPanelKnobMiG21Bis(2, Convert.ToInt32("1", 2), true, RadioPanelPZ69KnobsMiG21Bis.UpperSmallFreqWheelInc),
                new RadioPanelKnobMiG21Bis(2, Convert.ToInt32("10", 2), false, RadioPanelPZ69KnobsMiG21Bis.UpperSmallFreqWheelDec),
                new RadioPanelKnobMiG21Bis(2, Convert.ToInt32("100", 2), true, RadioPanelPZ69KnobsMiG21Bis.UpperLargeFreqWheelInc),
                new RadioPanelKnobMiG21Bis(2, Convert.ToInt32("1000", 2), false, RadioPanelPZ69KnobsMiG21Bis.UpperLargeFreqWheelDec),
                new RadioPanelKnobMiG21Bis(2, Convert.ToInt32("10000", 2), true, RadioPanelPZ69KnobsMiG21Bis.LowerSmallFreqWheelInc),
                new RadioPanelKnobMiG21Bis(2, Convert.ToInt32("100000", 2), false, RadioPanelPZ69KnobsMiG21Bis.LowerSmallFreqWheelDec),
                new RadioPanelKnobMiG21Bis(2, Convert.ToInt32("1000000", 2), true, RadioPanelPZ69KnobsMiG21Bis.LowerLargeFreqWheelInc),
                new RadioPanelKnobMiG21Bis(2, Convert.ToInt32("10000000", 2), false, RadioPanelPZ69KnobsMiG21Bis.LowerLargeFreqWheelDec),
                // Group 1
                new RadioPanelKnobMiG21Bis(1, Convert.ToInt32("1", 2), true, RadioPanelPZ69KnobsMiG21Bis.LowerCom2),
                new RadioPanelKnobMiG21Bis(1, Convert.ToInt32("10", 2), true, RadioPanelPZ69KnobsMiG21Bis.LowerRsbn),
                new RadioPanelKnobMiG21Bis(1, Convert.ToInt32("100", 2), true, RadioPanelPZ69KnobsMiG21Bis.LowerNav2),
                new RadioPanelKnobMiG21Bis(1, Convert.ToInt32("1000", 2), true, RadioPanelPZ69KnobsMiG21Bis.LowerArc),
                new RadioPanelKnobMiG21Bis(1, Convert.ToInt32("10000", 2), true, RadioPanelPZ69KnobsMiG21Bis.LowerDme),
                new RadioPanelKnobMiG21Bis(1, Convert.ToInt32("100000", 2), true, RadioPanelPZ69KnobsMiG21Bis.LowerXpdr),
                new RadioPanelKnobMiG21Bis(1, Convert.ToInt32("1000000", 2), true, RadioPanelPZ69KnobsMiG21Bis.UpperFreqSwitch),
                new RadioPanelKnobMiG21Bis(1, Convert.ToInt32("10000000", 2), true, RadioPanelPZ69KnobsMiG21Bis.LowerFreqSwitch),
                // Group 2
                new RadioPanelKnobMiG21Bis(0, Convert.ToInt32("1", 2), true, RadioPanelPZ69KnobsMiG21Bis.UpperRadio),
                new RadioPanelKnobMiG21Bis(0, Convert.ToInt32("10", 2), true, RadioPanelPZ69KnobsMiG21Bis.UpperCom2),
                new RadioPanelKnobMiG21Bis(0, Convert.ToInt32("100", 2), true, RadioPanelPZ69KnobsMiG21Bis.UpperRsbn),
                new RadioPanelKnobMiG21Bis(0, Convert.ToInt32("1000", 2), true, RadioPanelPZ69KnobsMiG21Bis.UpperNav2),
                new RadioPanelKnobMiG21Bis(0, Convert.ToInt32("10000", 2), true, RadioPanelPZ69KnobsMiG21Bis.UpperArc),
                new RadioPanelKnobMiG21Bis(0, Convert.ToInt32("100000", 2), true, RadioPanelPZ69KnobsMiG21Bis.UpperDme),
                new RadioPanelKnobMiG21Bis(0, Convert.ToInt32("1000000", 2), true, RadioPanelPZ69KnobsMiG21Bis.UpperXpdr),
                new RadioPanelKnobMiG21Bis(0, Convert.ToInt32("10000000", 2), true, RadioPanelPZ69KnobsMiG21Bis.LowerRadio)
            };

            return result;
        }
    }
}
