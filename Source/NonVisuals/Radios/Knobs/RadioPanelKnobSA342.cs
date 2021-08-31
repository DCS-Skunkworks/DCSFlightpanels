using System;
using System.Collections.Generic;
using NonVisuals.Interfaces;

namespace NonVisuals.Radios.Knobs
{ 

    public class RadioPanelKnobSA342 : ISaitekPanelKnob
    {
        public RadioPanelKnobSA342(int group, int mask, bool isOn, RadioPanelPZ69KnobsSA342 radioPanelPZ69Knob)
        {
            Group = group;
            Mask = mask;
            IsOn = isOn;
            RadioPanelPZ69Knob = radioPanelPZ69Knob;
        }

        public int Group { get; set; }

        public int Mask { get; set; }

        public bool IsOn { get; set; }

        public RadioPanelPZ69KnobsSA342 RadioPanelPZ69Knob { get; set; }
        
        public static HashSet<ISaitekPanelKnob> GetRadioPanelKnobs()
        {
            //true means clockwise turn
            var result = new HashSet<ISaitekPanelKnob>();
            //Group 0
            result.Add(new RadioPanelKnobSA342(2, Convert.ToInt32("1", 2), true, RadioPanelPZ69KnobsSA342.UPPER_SMALL_FREQ_WHEEL_INC));
            result.Add(new RadioPanelKnobSA342(2, Convert.ToInt32("10", 2), false, RadioPanelPZ69KnobsSA342.UPPER_SMALL_FREQ_WHEEL_DEC));
            result.Add(new RadioPanelKnobSA342(2, Convert.ToInt32("100", 2), true, RadioPanelPZ69KnobsSA342.UPPER_LARGE_FREQ_WHEEL_INC));
            result.Add(new RadioPanelKnobSA342(2, Convert.ToInt32("1000", 2), false, RadioPanelPZ69KnobsSA342.UPPER_LARGE_FREQ_WHEEL_DEC));
            result.Add(new RadioPanelKnobSA342(2, Convert.ToInt32("10000", 2), true, RadioPanelPZ69KnobsSA342.LOWER_SMALL_FREQ_WHEEL_INC));
            result.Add(new RadioPanelKnobSA342(2, Convert.ToInt32("100000", 2), false, RadioPanelPZ69KnobsSA342.LOWER_SMALL_FREQ_WHEEL_DEC));
            result.Add(new RadioPanelKnobSA342(2, Convert.ToInt32("1000000", 2), true, RadioPanelPZ69KnobsSA342.LOWER_LARGE_FREQ_WHEEL_INC));
            result.Add(new RadioPanelKnobSA342(2, Convert.ToInt32("10000000", 2), false, RadioPanelPZ69KnobsSA342.LOWER_LARGE_FREQ_WHEEL_DEC));

            //Group 1
            result.Add(new RadioPanelKnobSA342(1, Convert.ToInt32("1", 2), true, RadioPanelPZ69KnobsSA342.LOWER_VHFFM));
            result.Add(new RadioPanelKnobSA342(1, Convert.ToInt32("10", 2), true, RadioPanelPZ69KnobsSA342.LOWER_UHF));
            result.Add(new RadioPanelKnobSA342(1, Convert.ToInt32("100", 2), true, RadioPanelPZ69KnobsSA342.LOWER_NAV2));
            result.Add(new RadioPanelKnobSA342(1, Convert.ToInt32("1000", 2), true, RadioPanelPZ69KnobsSA342.LOWER_ADF));
            result.Add(new RadioPanelKnobSA342(1, Convert.ToInt32("10000", 2), true, RadioPanelPZ69KnobsSA342.LOWER_NADIR));
            result.Add(new RadioPanelKnobSA342(1, Convert.ToInt32("100000", 2), true, RadioPanelPZ69KnobsSA342.LOWER_XPDR));
            result.Add(new RadioPanelKnobSA342(1, Convert.ToInt32("1000000", 2), true, RadioPanelPZ69KnobsSA342.UPPER_FREQ_SWITCH));
            result.Add(new RadioPanelKnobSA342(1, Convert.ToInt32("10000000", 2), true, RadioPanelPZ69KnobsSA342.LOWER_FREQ_SWITCH));

            //Group 2
            result.Add(new RadioPanelKnobSA342(0, Convert.ToInt32("1", 2), true, RadioPanelPZ69KnobsSA342.UPPER_VHFAM)); //UPPER COM 1
            result.Add(new RadioPanelKnobSA342(0, Convert.ToInt32("10", 2), true, RadioPanelPZ69KnobsSA342.UPPER_VHFFM)); //UPPER COM 2
            result.Add(new RadioPanelKnobSA342(0, Convert.ToInt32("100", 2), true, RadioPanelPZ69KnobsSA342.UPPER_UHF)); //UPPER NAV 1
            result.Add(new RadioPanelKnobSA342(0, Convert.ToInt32("1000", 2), true, RadioPanelPZ69KnobsSA342.UPPER_NAV2)); //UPPER NAV 2
            result.Add(new RadioPanelKnobSA342(0, Convert.ToInt32("10000", 2), true, RadioPanelPZ69KnobsSA342.UPPER_ADF)); //UPPER ADF
            result.Add(new RadioPanelKnobSA342(0, Convert.ToInt32("100000", 2), true, RadioPanelPZ69KnobsSA342.UPPER_NADIR)); //UPPER DME
            result.Add(new RadioPanelKnobSA342(0, Convert.ToInt32("1000000", 2), true, RadioPanelPZ69KnobsSA342.UPPER_XPDR)); //UPPER XPDR
            result.Add(new RadioPanelKnobSA342(0, Convert.ToInt32("10000000", 2), true, RadioPanelPZ69KnobsSA342.LOWER_VHFAM)); //LOWER COM 1 
            return result;
        }




    }
}
