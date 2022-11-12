using System.Collections.Generic;
using NonVisuals.Interfaces;

namespace NonVisuals.Radios.Knobs
{
    public enum RadioPanelPZ69KnobsMi24P
    {
        UPPER_R863_MANUAL,      //COM1
        UPPER_R863_PRESET,      //COM2
        UPPER_YADRO1A,          //NAV1
        UPPER_R828,             //NAV2
        UPPER_ADF_ARK15,       //ADF
        UPPER_ARK_UD,          //DME_
        UPPER_SPU8,            //XPDR
        UPPER_SMALL_FREQ_WHEEL_INC,
        UPPER_SMALL_FREQ_WHEEL_DEC,
        UPPER_LARGE_FREQ_WHEEL_INC,
        UPPER_LARGE_FREQ_WHEEL_DEC,
        UPPER_FREQ_SWITCH,
        LOWER_R863_MANUAL,   //COM1
        LOWER_R863_PRESET,   //COM2
        LOWER_YADRO1A,      //NAV1
        LOWER_R828,          //NAV2
        LOWER_ADF_ARK15,    //ADF
        LOWER_ARK_UD,      //DME_
        LOWER_SPU8,        //XPDR
        LOWER_SMALL_FREQ_WHEEL_INC,
        LOWER_SMALL_FREQ_WHEEL_DEC,
        LOWER_LARGE_FREQ_WHEEL_INC,
        LOWER_LARGE_FREQ_WHEEL_DEC,
        LOWER_FREQ_SWITCH
    }


    /*
     * Represents a knob or button on the PZ69 Radio Panel.
     * Used by the PZ69 instance to determine what knob & button
     * the user is manipulating.
     */
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
                new RadioPanelKnobMi24P(2, 1 << 0, true, RadioPanelPZ69KnobsMi24P.UPPER_SMALL_FREQ_WHEEL_INC),
                new RadioPanelKnobMi24P(2, 1 << 1, false, RadioPanelPZ69KnobsMi24P.UPPER_SMALL_FREQ_WHEEL_DEC),
                new RadioPanelKnobMi24P(2, 1 << 2, true, RadioPanelPZ69KnobsMi24P.UPPER_LARGE_FREQ_WHEEL_INC),
                new RadioPanelKnobMi24P(2, 1 << 3, false, RadioPanelPZ69KnobsMi24P.UPPER_LARGE_FREQ_WHEEL_DEC),
                new RadioPanelKnobMi24P(2, 1 << 4, true, RadioPanelPZ69KnobsMi24P.LOWER_SMALL_FREQ_WHEEL_INC),
                new RadioPanelKnobMi24P(2, 1 << 5, false, RadioPanelPZ69KnobsMi24P.LOWER_SMALL_FREQ_WHEEL_DEC),
                new RadioPanelKnobMi24P(2, 1 << 6, true, RadioPanelPZ69KnobsMi24P.LOWER_LARGE_FREQ_WHEEL_INC),
                new RadioPanelKnobMi24P(2, 1 << 7, false, RadioPanelPZ69KnobsMi24P.LOWER_LARGE_FREQ_WHEEL_DEC),
                //Group 1
                new RadioPanelKnobMi24P(1, 1 << 0, true, RadioPanelPZ69KnobsMi24P.LOWER_R863_PRESET), //LOWER COM2
                new RadioPanelKnobMi24P(1, 1 << 1, true, RadioPanelPZ69KnobsMi24P.LOWER_YADRO1A), //LOWER NAV1
                new RadioPanelKnobMi24P(1, 1 << 2, true, RadioPanelPZ69KnobsMi24P.LOWER_R828), //LOWER NAV2
                new RadioPanelKnobMi24P(1, 1 << 3, true, RadioPanelPZ69KnobsMi24P.LOWER_ADF_ARK15), //LOWER ADF
                new RadioPanelKnobMi24P(1, 1 << 4, true, RadioPanelPZ69KnobsMi24P.LOWER_ARK_UD), //LOWER DME
                new RadioPanelKnobMi24P(1, 1 << 5, true, RadioPanelPZ69KnobsMi24P.LOWER_SPU8), //LOWER XPDR
                new RadioPanelKnobMi24P(1, 1 << 6, true, RadioPanelPZ69KnobsMi24P.UPPER_FREQ_SWITCH),
                new RadioPanelKnobMi24P(1, 1 << 7, true, RadioPanelPZ69KnobsMi24P.LOWER_FREQ_SWITCH),
                //Group 2
                new RadioPanelKnobMi24P(0, 1 << 0, true, RadioPanelPZ69KnobsMi24P.UPPER_R863_MANUAL), //UPPER COM1
                new RadioPanelKnobMi24P(0, 1 << 1, true, RadioPanelPZ69KnobsMi24P.UPPER_R863_PRESET), //UPPER COM2
                new RadioPanelKnobMi24P(0, 1 << 2, true, RadioPanelPZ69KnobsMi24P.UPPER_YADRO1A), //UPPER NAV1
                new RadioPanelKnobMi24P(0, 1 << 3, true, RadioPanelPZ69KnobsMi24P.UPPER_R828), //UPPER NAV2
                new RadioPanelKnobMi24P(0, 1 << 4, true, RadioPanelPZ69KnobsMi24P.UPPER_ADF_ARK15), //UPPER ADF
                new RadioPanelKnobMi24P(0, 1 << 5, true, RadioPanelPZ69KnobsMi24P.UPPER_ARK_UD), //UPPER DME
                new RadioPanelKnobMi24P(0, 1 << 6, true, RadioPanelPZ69KnobsMi24P.UPPER_SPU8), //UPPER XPDR
                new RadioPanelKnobMi24P(0, 1 << 7, true, RadioPanelPZ69KnobsMi24P.LOWER_R863_MANUAL) //LOWER COM1
            };
            return result;
        }
    }
}
