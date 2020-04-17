using System;
using System.Collections.Generic;
using NonVisuals.Interfaces;

namespace NonVisuals.Radios
{
    public enum RadioPanelPZ69KnobsFw190
    {
        UPPER_FUG16ZY = 0,      //COM1
        UPPER_IFF = 2,      //COM2
        UPPER_HOMING = 4,          //NAV1
        UPPER_NO_USE1 = 8,             //NAV2
        UPPER_NO_USE2 = 16,       //ADF
        UPPER_NO_USE3 = 32,          //DME_
        UPPER_NO_USE4 = 64,            //XPDR
        UPPER_SMALL_FREQ_WHEEL_INC = 128,
        UPPER_SMALL_FREQ_WHEEL_DEC = 256,
        UPPER_LARGE_FREQ_WHEEL_INC = 512,
        UPPER_LARGE_FREQ_WHEEL_DEC = 1024,
        UPPER_FREQ_SWITCH = 2056,
        LOWER_FUG16ZY = 4096,   //COM1
        LOWER_IFF = 8192,   //COM2
        LOWER_HOMING = 16384,      //NAV1
        LOWER_NO_USE1 = 32768,          //NAV2
        LOWER_NO_USE2 = 65536,    //ADF
        LOWER_NO_USE3 = 131072,      //DME_
        LOWER_NO_USE4 = 262144,        //XPDR
        LOWER_SMALL_FREQ_WHEEL_INC = 8388608,
        LOWER_SMALL_FREQ_WHEEL_DEC = 524288,
        LOWER_LARGE_FREQ_WHEEL_INC = 1048576,
        LOWER_LARGE_FREQ_WHEEL_DEC = 2097152,
        LOWER_FREQ_SWITCH = 4194304
    }

    public enum CurrentFw190RadioMode
    {
        /*
         *  COM1 Large Freq Sel
         *  COM1 Small Fine Tune
         *  Freq. Selector
         *  Fine Tuning
         *  
         *  COM2 Large IFF Control Switch
         *  COM2 Small Volume
         *  COM2 ACT/STBY IFF Test Button
         *  IFF Control Switch
         *  IFF Test Button
         *  Volume
         *  
         *  NAV1
         *  Homing Switch         
         */
        FUG16ZY = 0,
        IFF = 2,
        HOMING = 4,
        NOUSE = 64
    }

    public class RadioPanelKnobFw190 : ISaitekPanelKnob
    {
        public RadioPanelKnobFw190(int group, int mask, bool isOn, RadioPanelPZ69KnobsFw190 radioPanelPZ69Knob)
        {
            Group = group;
            Mask = mask;
            IsOn = isOn;
            RadioPanelPZ69Knob = radioPanelPZ69Knob;
        }

        public int Group { get; set; }

        public int Mask { get; set; }

        public bool IsOn { get; set; }

        public RadioPanelPZ69KnobsFw190 RadioPanelPZ69Knob { get; set; }

        public static HashSet<ISaitekPanelKnob> GetRadioPanelKnobs()
        {
            //true means clockwise turn
            var result = new HashSet<ISaitekPanelKnob>();
            //Group 0
            result.Add(new RadioPanelKnobFw190(2, Convert.ToInt32("1", 2), true, RadioPanelPZ69KnobsFw190.UPPER_SMALL_FREQ_WHEEL_INC));
            result.Add(new RadioPanelKnobFw190(2, Convert.ToInt32("10", 2), false, RadioPanelPZ69KnobsFw190.UPPER_SMALL_FREQ_WHEEL_DEC));
            result.Add(new RadioPanelKnobFw190(2, Convert.ToInt32("100", 2), true, RadioPanelPZ69KnobsFw190.UPPER_LARGE_FREQ_WHEEL_INC));
            result.Add(new RadioPanelKnobFw190(2, Convert.ToInt32("1000", 2), false, RadioPanelPZ69KnobsFw190.UPPER_LARGE_FREQ_WHEEL_DEC));
            result.Add(new RadioPanelKnobFw190(2, Convert.ToInt32("10000", 2), true, RadioPanelPZ69KnobsFw190.LOWER_SMALL_FREQ_WHEEL_INC));
            result.Add(new RadioPanelKnobFw190(2, Convert.ToInt32("100000", 2), false, RadioPanelPZ69KnobsFw190.LOWER_SMALL_FREQ_WHEEL_DEC));
            result.Add(new RadioPanelKnobFw190(2, Convert.ToInt32("1000000", 2), true, RadioPanelPZ69KnobsFw190.LOWER_LARGE_FREQ_WHEEL_INC));
            result.Add(new RadioPanelKnobFw190(2, Convert.ToInt32("10000000", 2), false, RadioPanelPZ69KnobsFw190.LOWER_LARGE_FREQ_WHEEL_DEC));

            //Group 1
            result.Add(new RadioPanelKnobFw190(1, Convert.ToInt32("1", 2), true, RadioPanelPZ69KnobsFw190.LOWER_IFF)); //LOWER COM2
            result.Add(new RadioPanelKnobFw190(1, Convert.ToInt32("10", 2), true, RadioPanelPZ69KnobsFw190.LOWER_HOMING)); //LOWER NAV1
            result.Add(new RadioPanelKnobFw190(1, Convert.ToInt32("100", 2), true, RadioPanelPZ69KnobsFw190.LOWER_NO_USE1)); //LOWER NAV2
            result.Add(new RadioPanelKnobFw190(1, Convert.ToInt32("1000", 2), true, RadioPanelPZ69KnobsFw190.LOWER_NO_USE2)); //LOWER ADF
            result.Add(new RadioPanelKnobFw190(1, Convert.ToInt32("10000", 2), true, RadioPanelPZ69KnobsFw190.LOWER_NO_USE3)); //LOWER DME
            result.Add(new RadioPanelKnobFw190(1, Convert.ToInt32("100000", 2), true, RadioPanelPZ69KnobsFw190.LOWER_NO_USE4)); //LOWER XPDR
            result.Add(new RadioPanelKnobFw190(1, Convert.ToInt32("1000000", 2), true, RadioPanelPZ69KnobsFw190.UPPER_FREQ_SWITCH));
            result.Add(new RadioPanelKnobFw190(1, Convert.ToInt32("10000000", 2), true, RadioPanelPZ69KnobsFw190.LOWER_FREQ_SWITCH));

            //Group 2
            result.Add(new RadioPanelKnobFw190(0, Convert.ToInt32("1", 2), true, RadioPanelPZ69KnobsFw190.UPPER_FUG16ZY)); //UPPER COM1
            result.Add(new RadioPanelKnobFw190(0, Convert.ToInt32("10", 2), true, RadioPanelPZ69KnobsFw190.UPPER_IFF)); //UPPER COM2
            result.Add(new RadioPanelKnobFw190(0, Convert.ToInt32("100", 2), true, RadioPanelPZ69KnobsFw190.UPPER_HOMING)); //UPPER NAV1
            result.Add(new RadioPanelKnobFw190(0, Convert.ToInt32("1000", 2), true, RadioPanelPZ69KnobsFw190.UPPER_NO_USE1)); //UPPER NAV2
            result.Add(new RadioPanelKnobFw190(0, Convert.ToInt32("10000", 2), true, RadioPanelPZ69KnobsFw190.UPPER_NO_USE2)); //UPPER ADF
            result.Add(new RadioPanelKnobFw190(0, Convert.ToInt32("100000", 2), true, RadioPanelPZ69KnobsFw190.UPPER_NO_USE3)); //UPPER DME
            result.Add(new RadioPanelKnobFw190(0, Convert.ToInt32("1000000", 2), true, RadioPanelPZ69KnobsFw190.UPPER_NO_USE4)); //UPPER XPDR
            result.Add(new RadioPanelKnobFw190(0, Convert.ToInt32("10000000", 2), true, RadioPanelPZ69KnobsFw190.LOWER_FUG16ZY)); //LOWER COM1
            return result;
        }
    }
}
