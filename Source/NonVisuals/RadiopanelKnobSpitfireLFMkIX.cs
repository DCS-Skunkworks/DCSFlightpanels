using System;
using System.Collections.Generic;

namespace NonVisuals
{
    public enum RadioPanelPZ69KnobsSpitfireLFMkIX
    {
        UPPER_HFRADIO = 0,      //COM1
        UPPER_IFF = 2,      //COM2
        UPPER_NO_USE0 = 4,          //NAV1
        UPPER_NO_USE1 = 8,             //NAV2
        UPPER_NO_USE2 = 16,       //ADF
        UPPER_NO_USE3 = 32,          //DME_
        UPPER_NO_USE4 = 64,            //XPDR
        UPPER_SMALL_FREQ_WHEEL_INC = 128,
        UPPER_SMALL_FREQ_WHEEL_DEC = 256,
        UPPER_LARGE_FREQ_WHEEL_INC = 512,
        UPPER_LARGE_FREQ_WHEEL_DEC = 1024,
        UPPER_FREQ_SWITCH = 2056,
        LOWER_HFRADIO = 4096,   //COM1
        LOWER_IFF = 8192,   //COM2
        LOWER_NO_USE0 = 16384,      //NAV1
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

    public enum CurrentSpitfireLFMkIXRadioMode
    {
        /*
         *  COM1 Large Freq Mode
         *  COM1 Small Fine Channel/OFF
         *  Freq. Selector Mode
         *  
         *  COM2 Large IFF Circuit B
         *  COM2 Small IFF Circuit D
         *  COM2 ACT/STBY IFF Destruction
         *     
         */
        HFRADIO = 0,
        HFRADIO2 = 2,
        IFF = 4,
        NOUSE = 64
    }

    public class RadioPanelKnobSpitfireLFMkIX : ISaitekPanelKnob
    {
        public RadioPanelKnobSpitfireLFMkIX(int group, int mask, bool isOn, RadioPanelPZ69KnobsSpitfireLFMkIX radioPanelPZ69Knob)
        {
            Group = group;
            Mask = mask;
            IsOn = isOn;
            RadioPanelPZ69Knob = radioPanelPZ69Knob;
        }

        public int Group { get; set; }

        public int Mask { get; set; }

        public bool IsOn { get; set; }

        public RadioPanelPZ69KnobsSpitfireLFMkIX RadioPanelPZ69Knob { get; set; }
        
        public static HashSet<ISaitekPanelKnob> GetRadioPanelKnobs()
        {
            //true means clockwise turn
            var result = new HashSet<ISaitekPanelKnob>();
            //Group 0
            result.Add(new RadioPanelKnobSpitfireLFMkIX(2, Convert.ToInt32("1", 2), true, RadioPanelPZ69KnobsSpitfireLFMkIX.UPPER_SMALL_FREQ_WHEEL_INC));
            result.Add(new RadioPanelKnobSpitfireLFMkIX(2, Convert.ToInt32("10", 2), false, RadioPanelPZ69KnobsSpitfireLFMkIX.UPPER_SMALL_FREQ_WHEEL_DEC));
            result.Add(new RadioPanelKnobSpitfireLFMkIX(2, Convert.ToInt32("100", 2), true, RadioPanelPZ69KnobsSpitfireLFMkIX.UPPER_LARGE_FREQ_WHEEL_INC));
            result.Add(new RadioPanelKnobSpitfireLFMkIX(2, Convert.ToInt32("1000", 2), false, RadioPanelPZ69KnobsSpitfireLFMkIX.UPPER_LARGE_FREQ_WHEEL_DEC));
            result.Add(new RadioPanelKnobSpitfireLFMkIX(2, Convert.ToInt32("10000", 2), true, RadioPanelPZ69KnobsSpitfireLFMkIX.LOWER_SMALL_FREQ_WHEEL_INC));
            result.Add(new RadioPanelKnobSpitfireLFMkIX(2, Convert.ToInt32("100000", 2), false, RadioPanelPZ69KnobsSpitfireLFMkIX.LOWER_SMALL_FREQ_WHEEL_DEC));
            result.Add(new RadioPanelKnobSpitfireLFMkIX(2, Convert.ToInt32("1000000", 2), true, RadioPanelPZ69KnobsSpitfireLFMkIX.LOWER_LARGE_FREQ_WHEEL_INC));
            result.Add(new RadioPanelKnobSpitfireLFMkIX(2, Convert.ToInt32("10000000", 2), false, RadioPanelPZ69KnobsSpitfireLFMkIX.LOWER_LARGE_FREQ_WHEEL_DEC));

            //Group 1
            result.Add(new RadioPanelKnobSpitfireLFMkIX(1, Convert.ToInt32("1", 2), true, RadioPanelPZ69KnobsSpitfireLFMkIX.LOWER_IFF)); //LOWER COM2
            result.Add(new RadioPanelKnobSpitfireLFMkIX(1, Convert.ToInt32("10", 2), true, RadioPanelPZ69KnobsSpitfireLFMkIX.LOWER_NO_USE0)); //LOWER NAV1
            result.Add(new RadioPanelKnobSpitfireLFMkIX(1, Convert.ToInt32("100", 2), true, RadioPanelPZ69KnobsSpitfireLFMkIX.LOWER_NO_USE1)); //LOWER NAV2
            result.Add(new RadioPanelKnobSpitfireLFMkIX(1, Convert.ToInt32("1000", 2), true, RadioPanelPZ69KnobsSpitfireLFMkIX.LOWER_NO_USE2)); //LOWER ADF
            result.Add(new RadioPanelKnobSpitfireLFMkIX(1, Convert.ToInt32("10000", 2), true, RadioPanelPZ69KnobsSpitfireLFMkIX.LOWER_NO_USE3)); //LOWER DME
            result.Add(new RadioPanelKnobSpitfireLFMkIX(1, Convert.ToInt32("100000", 2), true, RadioPanelPZ69KnobsSpitfireLFMkIX.LOWER_NO_USE4)); //LOWER XPDR
            result.Add(new RadioPanelKnobSpitfireLFMkIX(1, Convert.ToInt32("1000000", 2), true, RadioPanelPZ69KnobsSpitfireLFMkIX.UPPER_FREQ_SWITCH));
            result.Add(new RadioPanelKnobSpitfireLFMkIX(1, Convert.ToInt32("10000000", 2), true, RadioPanelPZ69KnobsSpitfireLFMkIX.LOWER_FREQ_SWITCH));

            //Group 2
            result.Add(new RadioPanelKnobSpitfireLFMkIX(0, Convert.ToInt32("1", 2), true, RadioPanelPZ69KnobsSpitfireLFMkIX.UPPER_HFRADIO)); //UPPER COM1
            result.Add(new RadioPanelKnobSpitfireLFMkIX(0, Convert.ToInt32("10", 2), true, RadioPanelPZ69KnobsSpitfireLFMkIX.UPPER_IFF)); //UPPER COM2
            result.Add(new RadioPanelKnobSpitfireLFMkIX(0, Convert.ToInt32("100", 2), true, RadioPanelPZ69KnobsSpitfireLFMkIX.UPPER_NO_USE0)); //UPPER NAV1
            result.Add(new RadioPanelKnobSpitfireLFMkIX(0, Convert.ToInt32("1000", 2), true, RadioPanelPZ69KnobsSpitfireLFMkIX.UPPER_NO_USE1)); //UPPER NAV2
            result.Add(new RadioPanelKnobSpitfireLFMkIX(0, Convert.ToInt32("10000", 2), true, RadioPanelPZ69KnobsSpitfireLFMkIX.UPPER_NO_USE2)); //UPPER ADF
            result.Add(new RadioPanelKnobSpitfireLFMkIX(0, Convert.ToInt32("100000", 2), true, RadioPanelPZ69KnobsSpitfireLFMkIX.UPPER_NO_USE3)); //UPPER DME
            result.Add(new RadioPanelKnobSpitfireLFMkIX(0, Convert.ToInt32("1000000", 2), true, RadioPanelPZ69KnobsSpitfireLFMkIX.UPPER_NO_USE4)); //UPPER XPDR
            result.Add(new RadioPanelKnobSpitfireLFMkIX(0, Convert.ToInt32("10000000", 2), true, RadioPanelPZ69KnobsSpitfireLFMkIX.LOWER_HFRADIO)); //LOWER COM1
            return result;
        }
    }
}
