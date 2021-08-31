using System;
using System.Collections.Generic;
using NonVisuals.Interfaces;

namespace NonVisuals.Radios.Knobs
{


    public class RadioPanelKnobF14B : ISaitekPanelKnob
    {
        public RadioPanelKnobF14B(int group, int mask, bool isOn, RadioPanelPZ69KnobsF14B radioPanelPZ69Knob)
        {
            Group = group;
            Mask = mask;
            IsOn = isOn;
            RadioPanelPZ69Knob = radioPanelPZ69Knob;
        }

        public int Group { get; set; }

        public int Mask { get; set; }

        public bool IsOn { get; set; }

        public RadioPanelPZ69KnobsF14B RadioPanelPZ69Knob { get; set; }

        public static HashSet<ISaitekPanelKnob> GetRadioPanelKnobs()
        {
            //true means clockwise turn
            var result = new HashSet<ISaitekPanelKnob>();
            //Group 0
            result.Add(new RadioPanelKnobF14B(2, Convert.ToInt32("1", 2), true, RadioPanelPZ69KnobsF14B.UPPER_SMALL_FREQ_WHEEL_INC));
            result.Add(new RadioPanelKnobF14B(2, Convert.ToInt32("10", 2), false, RadioPanelPZ69KnobsF14B.UPPER_SMALL_FREQ_WHEEL_DEC));
            result.Add(new RadioPanelKnobF14B(2, Convert.ToInt32("100", 2), true, RadioPanelPZ69KnobsF14B.UPPER_LARGE_FREQ_WHEEL_INC));
            result.Add(new RadioPanelKnobF14B(2, Convert.ToInt32("1000", 2), false, RadioPanelPZ69KnobsF14B.UPPER_LARGE_FREQ_WHEEL_DEC));
            result.Add(new RadioPanelKnobF14B(2, Convert.ToInt32("10000", 2), true, RadioPanelPZ69KnobsF14B.LOWER_SMALL_FREQ_WHEEL_INC));
            result.Add(new RadioPanelKnobF14B(2, Convert.ToInt32("100000", 2), false, RadioPanelPZ69KnobsF14B.LOWER_SMALL_FREQ_WHEEL_DEC));
            result.Add(new RadioPanelKnobF14B(2, Convert.ToInt32("1000000", 2), true, RadioPanelPZ69KnobsF14B.LOWER_LARGE_FREQ_WHEEL_INC));
            result.Add(new RadioPanelKnobF14B(2, Convert.ToInt32("10000000", 2), false, RadioPanelPZ69KnobsF14B.LOWER_LARGE_FREQ_WHEEL_DEC));

            //Group 1
            result.Add(new RadioPanelKnobF14B(1, Convert.ToInt32("1", 2), true, RadioPanelPZ69KnobsF14B.LOWER_VUHF));
            result.Add(new RadioPanelKnobF14B(1, Convert.ToInt32("10", 2), true, RadioPanelPZ69KnobsF14B.LOWER_PLT_TACAN));
            result.Add(new RadioPanelKnobF14B(1, Convert.ToInt32("100", 2), true, RadioPanelPZ69KnobsF14B.LOWER_RIO_TACAN));
            result.Add(new RadioPanelKnobF14B(1, Convert.ToInt32("1000", 2), true, RadioPanelPZ69KnobsF14B.LOWER_ADF));
            result.Add(new RadioPanelKnobF14B(1, Convert.ToInt32("10000", 2), true, RadioPanelPZ69KnobsF14B.LOWER_LINK4));
            result.Add(new RadioPanelKnobF14B(1, Convert.ToInt32("100000", 2), true, RadioPanelPZ69KnobsF14B.LOWER_XPDR));
            result.Add(new RadioPanelKnobF14B(1, Convert.ToInt32("1000000", 2), true, RadioPanelPZ69KnobsF14B.UPPER_FREQ_SWITCH));
            result.Add(new RadioPanelKnobF14B(1, Convert.ToInt32("10000000", 2), true, RadioPanelPZ69KnobsF14B.LOWER_FREQ_SWITCH));

            //Group 2
            result.Add(new RadioPanelKnobF14B(0, Convert.ToInt32("1", 2), true, RadioPanelPZ69KnobsF14B.UPPER_UHF)); //UPPER COM 1
            result.Add(new RadioPanelKnobF14B(0, Convert.ToInt32("10", 2), true, RadioPanelPZ69KnobsF14B.UPPER_VUHF)); //UPPER COM 2
            result.Add(new RadioPanelKnobF14B(0, Convert.ToInt32("100", 2), true, RadioPanelPZ69KnobsF14B.UPPER_PLT_TACAN)); //UPPER NAV 1
            result.Add(new RadioPanelKnobF14B(0, Convert.ToInt32("1000", 2), true, RadioPanelPZ69KnobsF14B.UPPER_RIO_TACAN)); //UPPER NAV 2
            result.Add(new RadioPanelKnobF14B(0, Convert.ToInt32("10000", 2), true, RadioPanelPZ69KnobsF14B.UPPER_ADF)); //UPPER ADF
            result.Add(new RadioPanelKnobF14B(0, Convert.ToInt32("100000", 2), true, RadioPanelPZ69KnobsF14B.UPPER_LINK4)); //UPPER DME
            result.Add(new RadioPanelKnobF14B(0, Convert.ToInt32("1000000", 2), true, RadioPanelPZ69KnobsF14B.UPPER_XPDR)); //UPPER XPDR
            result.Add(new RadioPanelKnobF14B(0, Convert.ToInt32("10000000", 2), true, RadioPanelPZ69KnobsF14B.LOWER_UHF)); //LOWER COM 1 
            return result;
        }




    }
}
