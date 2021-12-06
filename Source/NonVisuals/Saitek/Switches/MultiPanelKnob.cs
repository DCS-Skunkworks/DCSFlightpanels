namespace NonVisuals.Saitek.Switches
{
    using System;
    using System.Collections.Generic;

    using MEF;

    using NonVisuals.Interfaces;

    public class MultiPanelKnob : ISaitekPanelKnob
    {

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

            // MultiPanelKnob{SWITCHKEY_MASTER_ALT}
            var dataString = str.Remove(0, 15);

            // SWITCHKEY_MASTER_ALT}
            dataString = dataString.Remove(dataString.Length - 1, 1);

            // SWITCHKEY_MASTER_ALT
            MultiPanelPZ70Knob = (MultiPanelPZ70Knobs)Enum.Parse(typeof(MultiPanelPZ70Knobs), dataString.Trim());
        }

        public static HashSet<ISaitekPanelKnob> GetMultiPanelKnobs()
        {
            // true means clockwise turn
            var result = new HashSet<ISaitekPanelKnob>
            {
                // Group 0
                new MultiPanelKnob(0, Convert.ToInt32("1", 2), true, MultiPanelPZ70Knobs.KNOB_ALT),
                new MultiPanelKnob(0, Convert.ToInt32("10", 2), false, MultiPanelPZ70Knobs.KNOB_VS),
                new MultiPanelKnob(0, Convert.ToInt32("100", 2), true, MultiPanelPZ70Knobs.KNOB_IAS),
                new MultiPanelKnob(0, Convert.ToInt32("1000", 2), false, MultiPanelPZ70Knobs.KNOB_HDG),
                new MultiPanelKnob(0, Convert.ToInt32("10000", 2), true, MultiPanelPZ70Knobs.KNOB_CRS),
                new MultiPanelKnob(0, Convert.ToInt32("100000", 2), false, MultiPanelPZ70Knobs.LCD_WHEEL_INC),
                new MultiPanelKnob(0, Convert.ToInt32("1000000", 2), true, MultiPanelPZ70Knobs.LCD_WHEEL_DEC),
                new MultiPanelKnob(0, Convert.ToInt32("10000000", 2), false, MultiPanelPZ70Knobs.AP_BUTTON),
                // Group 1
                new MultiPanelKnob(1, Convert.ToInt32("1", 2), true, MultiPanelPZ70Knobs.HDG_BUTTON),
                new MultiPanelKnob(1, Convert.ToInt32("10", 2), true, MultiPanelPZ70Knobs.NAV_BUTTON),
                new MultiPanelKnob(1, Convert.ToInt32("100", 2), true, MultiPanelPZ70Knobs.IAS_BUTTON),
                new MultiPanelKnob(1, Convert.ToInt32("1000", 2), true, MultiPanelPZ70Knobs.ALT_BUTTON),
                new MultiPanelKnob(1, Convert.ToInt32("10000", 2), true, MultiPanelPZ70Knobs.VS_BUTTON),
                new MultiPanelKnob(1, Convert.ToInt32("100000", 2), true, MultiPanelPZ70Knobs.APR_BUTTON),
                new MultiPanelKnob(1, Convert.ToInt32("1000000", 2), true, MultiPanelPZ70Knobs.REV_BUTTON),
                new MultiPanelKnob(1, Convert.ToInt32("10000000", 2), true, MultiPanelPZ70Knobs.AUTO_THROTTLE),
                // Group 2
                new MultiPanelKnob(2, Convert.ToInt32("1", 2), true, MultiPanelPZ70Knobs.FLAPS_LEVER_UP),
                new MultiPanelKnob(2, Convert.ToInt32("10", 2), true, MultiPanelPZ70Knobs.FLAPS_LEVER_DOWN),
                new MultiPanelKnob(2, Convert.ToInt32("100", 2), true, MultiPanelPZ70Knobs.PITCH_TRIM_WHEEL_UP),
                new MultiPanelKnob(2, Convert.ToInt32("1000", 2), true, MultiPanelPZ70Knobs.PITCH_TRIM_WHEEL_DOWN)
            };

            return result;
        }
    }

}
