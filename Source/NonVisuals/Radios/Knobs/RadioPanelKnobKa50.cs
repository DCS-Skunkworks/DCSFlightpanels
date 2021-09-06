using System;
using System.Collections.Generic;
using NonVisuals.Interfaces;

namespace NonVisuals.Radios.Knobs
{
    using MEF;

    public class RadioPanelKnobKa50 : ISaitekPanelKnob
    {
        public RadioPanelKnobKa50(int group, int mask, bool isOn, RadioPanelPZ69KnobsKa50 radioPanelPZ69Knob)
        {
            Group = group;
            Mask = mask;
            IsOn = isOn;
            RadioPanelPZ69Knob = radioPanelPZ69Knob;
        }

        public int Group { get; set; }

        public int Mask { get; set; }

        public bool IsOn { get; set; }

        public RadioPanelPZ69KnobsKa50 RadioPanelPZ69Knob { get; set; }
        
        public static HashSet<ISaitekPanelKnob> GetRadioPanelKnobs()
        {
            //true means clockwise turn
            var result = new HashSet<ISaitekPanelKnob>();
            //Group 0
            result.Add(new RadioPanelKnobKa50(2, Convert.ToInt32("1", 2), true, RadioPanelPZ69KnobsKa50.UPPER_SMALL_FREQ_WHEEL_INC));
            result.Add(new RadioPanelKnobKa50(2, Convert.ToInt32("10", 2), false, RadioPanelPZ69KnobsKa50.UPPER_SMALL_FREQ_WHEEL_DEC));
            result.Add(new RadioPanelKnobKa50(2, Convert.ToInt32("100", 2), true, RadioPanelPZ69KnobsKa50.UPPER_LARGE_FREQ_WHEEL_INC));
            result.Add(new RadioPanelKnobKa50(2, Convert.ToInt32("1000", 2), false, RadioPanelPZ69KnobsKa50.UPPER_LARGE_FREQ_WHEEL_DEC));
            result.Add(new RadioPanelKnobKa50(2, Convert.ToInt32("10000", 2), true, RadioPanelPZ69KnobsKa50.LOWER_SMALL_FREQ_WHEEL_INC));
            result.Add(new RadioPanelKnobKa50(2, Convert.ToInt32("100000", 2), false, RadioPanelPZ69KnobsKa50.LOWER_SMALL_FREQ_WHEEL_DEC));
            result.Add(new RadioPanelKnobKa50(2, Convert.ToInt32("1000000", 2), true, RadioPanelPZ69KnobsKa50.LOWER_LARGE_FREQ_WHEEL_INC));
            result.Add(new RadioPanelKnobKa50(2, Convert.ToInt32("10000000", 2), false, RadioPanelPZ69KnobsKa50.LOWER_LARGE_FREQ_WHEEL_DEC));

            //Group 1
            result.Add(new RadioPanelKnobKa50(1, Convert.ToInt32("1", 2), true, RadioPanelPZ69KnobsKa50.LOWER_VHF2_R800L1)); //LOWER COM2
            result.Add(new RadioPanelKnobKa50(1, Convert.ToInt32("10", 2), true, RadioPanelPZ69KnobsKa50.LOWER_ABRIS)); //LOWER NAV1
            result.Add(new RadioPanelKnobKa50(1, Convert.ToInt32("100", 2), true, RadioPanelPZ69KnobsKa50.LOWER_DATALINK)); //LOWER NAV2
            result.Add(new RadioPanelKnobKa50(1, Convert.ToInt32("1000", 2), true, RadioPanelPZ69KnobsKa50.LOWER_ADF_ARK22)); //LOWER ADF
            result.Add(new RadioPanelKnobKa50(1, Convert.ToInt32("10000", 2), true, RadioPanelPZ69KnobsKa50.LOWER_NO_USE3)); //LOWER DME
            result.Add(new RadioPanelKnobKa50(1, Convert.ToInt32("100000", 2), true, RadioPanelPZ69KnobsKa50.LOWER_NO_USE4)); //LOWER XPDR
            result.Add(new RadioPanelKnobKa50(1, Convert.ToInt32("1000000", 2), true, RadioPanelPZ69KnobsKa50.UPPER_FREQ_SWITCH));
            result.Add(new RadioPanelKnobKa50(1, Convert.ToInt32("10000000", 2), true, RadioPanelPZ69KnobsKa50.LOWER_FREQ_SWITCH));

            //Group 2
            result.Add(new RadioPanelKnobKa50(0, Convert.ToInt32("1", 2), true, RadioPanelPZ69KnobsKa50.UPPER_VHF1_R828)); //UPPER COM1
            result.Add(new RadioPanelKnobKa50(0, Convert.ToInt32("10", 2), true, RadioPanelPZ69KnobsKa50.UPPER_VHF2_R800L1)); //UPPER COM2
            result.Add(new RadioPanelKnobKa50(0, Convert.ToInt32("100", 2), true, RadioPanelPZ69KnobsKa50.UPPER_ABRIS)); //UPPER NAV1
            result.Add(new RadioPanelKnobKa50(0, Convert.ToInt32("1000", 2), true, RadioPanelPZ69KnobsKa50.UPPER_DATALINK)); //UPPER NAV2
            result.Add(new RadioPanelKnobKa50(0, Convert.ToInt32("10000", 2), true, RadioPanelPZ69KnobsKa50.UPPER_ADF_ARK22)); //UPPER ADF
            result.Add(new RadioPanelKnobKa50(0, Convert.ToInt32("100000", 2), true, RadioPanelPZ69KnobsKa50.UPPER_NO_USE3)); //UPPER DME
            result.Add(new RadioPanelKnobKa50(0, Convert.ToInt32("1000000", 2), true, RadioPanelPZ69KnobsKa50.UPPER_NO_USE4)); //UPPER XPDR
            result.Add(new RadioPanelKnobKa50(0, Convert.ToInt32("10000000", 2), true, RadioPanelPZ69KnobsKa50.LOWER_VHF1_R828)); //LOWER COM1
            return result;
        }




    }


}
