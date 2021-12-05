using System;
using System.Collections.Generic;
using NonVisuals.Interfaces;

namespace NonVisuals.Radios.Knobs
{
    using MEF;

    public class RadioPanelKnobMi24P : ISaitekPanelKnob
    {
        public RadioPanelKnobMi24P(int group, int mask, bool isOn, RadioPanelPZ69KnobsMi24P radioPanelPZ69Knob)
        {
            Group = group;
            Mask = mask;
            IsOn = isOn;
            RadioPanelPZ69Knob = radioPanelPZ69Knob;
        }

        public int Group { get; set; }

        public int Mask { get; set; }

        public bool IsOn { get; set; }

        public RadioPanelPZ69KnobsMi24P RadioPanelPZ69Knob { get; set; }

        public static HashSet<ISaitekPanelKnob> GetRadioPanelKnobs()
        {
            //true means clockwise turn
            var result = new HashSet<ISaitekPanelKnob>
            {
                //Group 0
                new RadioPanelKnobMi24P(2, Convert.ToInt32("1", 2), true, RadioPanelPZ69KnobsMi24P.UPPER_SMALL_FREQ_WHEEL_INC),
                new RadioPanelKnobMi24P(2, Convert.ToInt32("10", 2), false, RadioPanelPZ69KnobsMi24P.UPPER_SMALL_FREQ_WHEEL_DEC),
                new RadioPanelKnobMi24P(2, Convert.ToInt32("100", 2), true, RadioPanelPZ69KnobsMi24P.UPPER_LARGE_FREQ_WHEEL_INC),
                new RadioPanelKnobMi24P(2, Convert.ToInt32("1000", 2), false, RadioPanelPZ69KnobsMi24P.UPPER_LARGE_FREQ_WHEEL_DEC),
                new RadioPanelKnobMi24P(2, Convert.ToInt32("10000", 2), true, RadioPanelPZ69KnobsMi24P.LOWER_SMALL_FREQ_WHEEL_INC),
                new RadioPanelKnobMi24P(2, Convert.ToInt32("100000", 2), false, RadioPanelPZ69KnobsMi24P.LOWER_SMALL_FREQ_WHEEL_DEC),
                new RadioPanelKnobMi24P(2, Convert.ToInt32("1000000", 2), true, RadioPanelPZ69KnobsMi24P.LOWER_LARGE_FREQ_WHEEL_INC),
                new RadioPanelKnobMi24P(2, Convert.ToInt32("10000000", 2), false, RadioPanelPZ69KnobsMi24P.LOWER_LARGE_FREQ_WHEEL_DEC),
                //Group 1
                new RadioPanelKnobMi24P(1, Convert.ToInt32("1", 2), true, RadioPanelPZ69KnobsMi24P.LOWER_R863_PRESET), //LOWER COM2
                new RadioPanelKnobMi24P(1, Convert.ToInt32("10", 2), true, RadioPanelPZ69KnobsMi24P.LOWER_YADRO1A), //LOWER NAV1
                new RadioPanelKnobMi24P(1, Convert.ToInt32("100", 2), true, RadioPanelPZ69KnobsMi24P.LOWER_R828), //LOWER NAV2
                new RadioPanelKnobMi24P(1, Convert.ToInt32("1000", 2), true, RadioPanelPZ69KnobsMi24P.LOWER_ADF_ARK15), //LOWER ADF
                new RadioPanelKnobMi24P(1, Convert.ToInt32("10000", 2), true, RadioPanelPZ69KnobsMi24P.LOWER_ARK_UD), //LOWER DME
                new RadioPanelKnobMi24P(1, Convert.ToInt32("100000", 2), true, RadioPanelPZ69KnobsMi24P.LOWER_SPU8), //LOWER XPDR
                new RadioPanelKnobMi24P(1, Convert.ToInt32("1000000", 2), true, RadioPanelPZ69KnobsMi24P.UPPER_FREQ_SWITCH),
                new RadioPanelKnobMi24P(1, Convert.ToInt32("10000000", 2), true, RadioPanelPZ69KnobsMi24P.LOWER_FREQ_SWITCH),
                //Group 2
                new RadioPanelKnobMi24P(0, Convert.ToInt32("1", 2), true, RadioPanelPZ69KnobsMi24P.UPPER_R863_MANUAL), //UPPER COM1
                new RadioPanelKnobMi24P(0, Convert.ToInt32("10", 2), true, RadioPanelPZ69KnobsMi24P.UPPER_R863_PRESET), //UPPER COM2
                new RadioPanelKnobMi24P(0, Convert.ToInt32("100", 2), true, RadioPanelPZ69KnobsMi24P.UPPER_YADRO1A), //UPPER NAV1
                new RadioPanelKnobMi24P(0, Convert.ToInt32("1000", 2), true, RadioPanelPZ69KnobsMi24P.UPPER_R828), //UPPER NAV2
                new RadioPanelKnobMi24P(0, Convert.ToInt32("10000", 2), true, RadioPanelPZ69KnobsMi24P.UPPER_ADF_ARK15), //UPPER ADF
                new RadioPanelKnobMi24P(0, Convert.ToInt32("100000", 2), true, RadioPanelPZ69KnobsMi24P.UPPER_ARK_UD), //UPPER DME
                new RadioPanelKnobMi24P(0, Convert.ToInt32("1000000", 2), true, RadioPanelPZ69KnobsMi24P.UPPER_SPU8), //UPPER XPDR
                new RadioPanelKnobMi24P(0, Convert.ToInt32("10000000", 2), true, RadioPanelPZ69KnobsMi24P.LOWER_R863_MANUAL) //LOWER COM1
            };

            return result;
        }




    }


}
