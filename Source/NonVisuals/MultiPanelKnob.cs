using System;
using System.Collections.Generic;

namespace NonVisuals
{
    public enum MultiPanelPZ70Knobs
    {
        KNOB_ALT = 0,
        KNOB_VS = 2,
        KNOB_IAS = 4,
        KNOB_HDG = 8,
        KNOB_CRS = 16,
        LCD_WHEEL_INC = 32,
        LCD_WHEEL_DEC = 64,
        AUTO_THROTTLE = 512,
        FLAPS_LEVER_UP = 1024,
        FLAPS_LEVER_DOWN = 256,
        PITCH_TRIM_WHEEL_UP = 2056,
        PITCH_TRIM_WHEEL_DOWN = 128,
        AP_BUTTON = 4096,
        HDG_BUTTON = 8192,
        NAV_BUTTON = 16384,
        IAS_BUTTON = 32768,
        ALT_BUTTON = 65536,
        VS_BUTTON = 131072,
        APR_BUTTON = 262144,
        REV_BUTTON = 524288
    }

    public class MultiPanelKnob
    {
        public MultiPanelKnob()
        {
        }

        public MultiPanelKnob(int group, int mask, bool isOn, MultiPanelPZ70Knobs multiPanelPZ70Knob)
        {
            Group = group;
            Mask = mask;
            IsOn = isOn;
            MultiPanelPZ70Knob = multiPanelPZ70Knob;
        }

        public int Group { get; set; }

        public int Mask { get; set; }

        public bool IsOn { get; set; }

        public MultiPanelPZ70Knobs MultiPanelPZ70Knob { get; set; }

        public string ExportString()
        {
            return "MultiPanelKnob{" + Enum.GetName(typeof(MultiPanelPZ70Knobs), MultiPanelPZ70Knob) + "}";
        }

        public void ImportString(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                throw new ArgumentException("Import string empty. (MultiPanelKnob)");
            }
            if (!str.StartsWith("MultiPanelKnob{") || !str.EndsWith("}"))
            {
                throw new ArgumentException("Import string format exception. (MultiPanelKnob) >" + str + "<");
            }
            //MultiPanelKnob{SWITCHKEY_MASTER_ALT}
            var dataString = str.Remove(0, 15);
            //SWITCHKEY_MASTER_ALT}
            dataString = dataString.Remove(dataString.Length - 1, 1);
            //SWITCHKEY_MASTER_ALT
            MultiPanelPZ70Knob = (MultiPanelPZ70Knobs)Enum.Parse(typeof(MultiPanelPZ70Knobs), dataString.Trim());
        }

        public static HashSet<MultiPanelKnob> GetMultiPanelKnobs()
        {
            //true means clockwise turn
            var result = new HashSet<MultiPanelKnob>();
            //Group 0
            result.Add(new MultiPanelKnob(0, Convert.ToInt32("1", 2), true, MultiPanelPZ70Knobs.KNOB_ALT));
            result.Add(new MultiPanelKnob(0, Convert.ToInt32("10", 2), false, MultiPanelPZ70Knobs.KNOB_VS));
            result.Add(new MultiPanelKnob(0, Convert.ToInt32("100", 2), true, MultiPanelPZ70Knobs.KNOB_IAS));
            result.Add(new MultiPanelKnob(0, Convert.ToInt32("1000", 2), false, MultiPanelPZ70Knobs.KNOB_HDG));
            result.Add(new MultiPanelKnob(0, Convert.ToInt32("10000", 2), true, MultiPanelPZ70Knobs.KNOB_CRS));
            result.Add(new MultiPanelKnob(0, Convert.ToInt32("100000", 2), false, MultiPanelPZ70Knobs.LCD_WHEEL_INC));
            result.Add(new MultiPanelKnob(0, Convert.ToInt32("1000000", 2), true, MultiPanelPZ70Knobs.LCD_WHEEL_DEC));
            result.Add(new MultiPanelKnob(0, Convert.ToInt32("10000000", 2), false, MultiPanelPZ70Knobs.AP_BUTTON));
            
            //Group 1
            result.Add(new MultiPanelKnob(1, Convert.ToInt32("1", 2), true, MultiPanelPZ70Knobs.HDG_BUTTON));
            result.Add(new MultiPanelKnob(1, Convert.ToInt32("10", 2), true, MultiPanelPZ70Knobs.NAV_BUTTON));
            result.Add(new MultiPanelKnob(1, Convert.ToInt32("100", 2), true, MultiPanelPZ70Knobs.IAS_BUTTON));
            result.Add(new MultiPanelKnob(1, Convert.ToInt32("1000", 2), true, MultiPanelPZ70Knobs.ALT_BUTTON));
            result.Add(new MultiPanelKnob(1, Convert.ToInt32("10000", 2), true, MultiPanelPZ70Knobs.VS_BUTTON));
            result.Add(new MultiPanelKnob(1, Convert.ToInt32("100000", 2), true, MultiPanelPZ70Knobs.APR_BUTTON));
            result.Add(new MultiPanelKnob(1, Convert.ToInt32("1000000", 2), true, MultiPanelPZ70Knobs.REV_BUTTON));
            result.Add(new MultiPanelKnob(1, Convert.ToInt32("10000000", 2), true, MultiPanelPZ70Knobs.AUTO_THROTTLE));

            //Group 2
            result.Add(new MultiPanelKnob(2, Convert.ToInt32("1", 2), true, MultiPanelPZ70Knobs.FLAPS_LEVER_UP));
            result.Add(new MultiPanelKnob(2, Convert.ToInt32("10", 2), true, MultiPanelPZ70Knobs.FLAPS_LEVER_DOWN));
            result.Add(new MultiPanelKnob(2, Convert.ToInt32("100", 2), true, MultiPanelPZ70Knobs.PITCH_TRIM_WHEEL_UP));
            result.Add(new MultiPanelKnob(2, Convert.ToInt32("1000", 2), true, MultiPanelPZ70Knobs.PITCH_TRIM_WHEEL_DOWN));
            return result;
        }


    }

    public class MultiPanelPZ70KnobOnOff
    {
        private readonly MultiPanelPZ70Knobs _multiPanelPZ70Knobs;
        private readonly bool _on;

        public MultiPanelPZ70KnobOnOff(MultiPanelPZ70Knobs multiPanelPZ70Knobs, bool on)
        {
            _multiPanelPZ70Knobs = multiPanelPZ70Knobs;
            _on = @on;
        }

        public MultiPanelPZ70Knobs MultiPanelPZ70Knob
        {
            get { return _multiPanelPZ70Knobs; }
        }

        public bool On
        {
            get { return _on; }
        }
    }
}
