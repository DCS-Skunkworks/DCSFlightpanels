namespace NonVisuals.Radios.Knobs
{
    using System.Collections.Generic;

    using Interfaces;

    public enum RadioPanelPZ69KnobsMiG21Bis
    {
        UpperRadio,
        UpperCom2,
        UpperRsbn,
        UpperNav2,
        UpperArc,
        UpperDme,
        UpperXpdr,
        UpperSmallFreqWheelInc,
        UpperSmallFreqWheelDec,
        UpperLargeFreqWheelInc,
        UpperLargeFreqWheelDec,
        UpperFreqSwitch,
        LowerRadio,
        LowerCom2,
        LowerRsbn,
        LowerNav2,
        LowerArc,
        LowerDme,
        LowerXpdr,
        LowerSmallFreqWheelInc,
        LowerSmallFreqWheelDec,
        LowerLargeFreqWheelInc,
        LowerLargeFreqWheelDec,
        LowerFreqSwitch
    }


    /*
     * Represents a knob or button on the PZ69 Radio Panel.
     * Used by the PZ69 instance to determine what knob & button
     * the user is manipulating.
     */
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
                new RadioPanelKnobMiG21Bis(2, 1 << 0, true, RadioPanelPZ69KnobsMiG21Bis.UpperSmallFreqWheelInc),
                new RadioPanelKnobMiG21Bis(2, 1 << 1, false, RadioPanelPZ69KnobsMiG21Bis.UpperSmallFreqWheelDec),
                new RadioPanelKnobMiG21Bis(2, 1 << 2, true, RadioPanelPZ69KnobsMiG21Bis.UpperLargeFreqWheelInc),
                new RadioPanelKnobMiG21Bis(2, 1 << 3, false, RadioPanelPZ69KnobsMiG21Bis.UpperLargeFreqWheelDec),
                new RadioPanelKnobMiG21Bis(2, 1 << 4, true, RadioPanelPZ69KnobsMiG21Bis.LowerSmallFreqWheelInc),
                new RadioPanelKnobMiG21Bis(2, 1 << 5, false, RadioPanelPZ69KnobsMiG21Bis.LowerSmallFreqWheelDec),
                new RadioPanelKnobMiG21Bis(2, 1 << 6, true, RadioPanelPZ69KnobsMiG21Bis.LowerLargeFreqWheelInc),
                new RadioPanelKnobMiG21Bis(2, 1 << 7, false, RadioPanelPZ69KnobsMiG21Bis.LowerLargeFreqWheelDec),
                // Group 1
                new RadioPanelKnobMiG21Bis(1, 1 << 0, true, RadioPanelPZ69KnobsMiG21Bis.LowerCom2),
                new RadioPanelKnobMiG21Bis(1, 1 << 1, true, RadioPanelPZ69KnobsMiG21Bis.LowerRsbn),
                new RadioPanelKnobMiG21Bis(1, 1 << 2, true, RadioPanelPZ69KnobsMiG21Bis.LowerNav2),
                new RadioPanelKnobMiG21Bis(1, 1 << 3, true, RadioPanelPZ69KnobsMiG21Bis.LowerArc),
                new RadioPanelKnobMiG21Bis(1, 1 << 4, true, RadioPanelPZ69KnobsMiG21Bis.LowerDme),
                new RadioPanelKnobMiG21Bis(1, 1 << 5, true, RadioPanelPZ69KnobsMiG21Bis.LowerXpdr),
                new RadioPanelKnobMiG21Bis(1, 1 << 6, true, RadioPanelPZ69KnobsMiG21Bis.UpperFreqSwitch),
                new RadioPanelKnobMiG21Bis(1, 1 << 7, true, RadioPanelPZ69KnobsMiG21Bis.LowerFreqSwitch),
                // Group 2
                new RadioPanelKnobMiG21Bis(0, 1 << 0, true, RadioPanelPZ69KnobsMiG21Bis.UpperRadio),
                new RadioPanelKnobMiG21Bis(0, 1 << 1, true, RadioPanelPZ69KnobsMiG21Bis.UpperCom2),
                new RadioPanelKnobMiG21Bis(0, 1 << 2, true, RadioPanelPZ69KnobsMiG21Bis.UpperRsbn),
                new RadioPanelKnobMiG21Bis(0, 1 << 3, true, RadioPanelPZ69KnobsMiG21Bis.UpperNav2),
                new RadioPanelKnobMiG21Bis(0, 1 << 4, true, RadioPanelPZ69KnobsMiG21Bis.UpperArc),
                new RadioPanelKnobMiG21Bis(0, 1 << 5, true, RadioPanelPZ69KnobsMiG21Bis.UpperDme),
                new RadioPanelKnobMiG21Bis(0, 1 << 6, true, RadioPanelPZ69KnobsMiG21Bis.UpperXpdr),
                new RadioPanelKnobMiG21Bis(0, 1 << 7, true, RadioPanelPZ69KnobsMiG21Bis.LowerRadio)
            };

            return result;
        }
    }
}
