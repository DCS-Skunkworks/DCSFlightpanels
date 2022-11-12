namespace NonVisuals.Saitek.Switches
{
    using System;
    using System.Collections.Generic;

    using MEF;

    using Interfaces;


    /*
     * Logitech Multi Panel Knob, contains only the key, no information about
     * what action(s) the knob should perform.
     * Used by the panel to get information on what knob(s) has been switched
     */
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
                new MultiPanelKnob(0, 1 << 0, true, MultiPanelPZ70Knobs.KNOB_ALT),
                new MultiPanelKnob(0, 1 << 1, false, MultiPanelPZ70Knobs.KNOB_VS),
                new MultiPanelKnob(0, 1 << 2, true, MultiPanelPZ70Knobs.KNOB_IAS),
                new MultiPanelKnob(0, 1 << 3, false, MultiPanelPZ70Knobs.KNOB_HDG),
                new MultiPanelKnob(0, 1 << 4, true, MultiPanelPZ70Knobs.KNOB_CRS),
                new MultiPanelKnob(0, 1 << 5, false, MultiPanelPZ70Knobs.LCD_WHEEL_INC),
                new MultiPanelKnob(0, 1 << 6, true, MultiPanelPZ70Knobs.LCD_WHEEL_DEC),
                new MultiPanelKnob(0, 1 << 7, false, MultiPanelPZ70Knobs.AP_BUTTON),
                // Group 1
                new MultiPanelKnob(1, 1 << 0, true, MultiPanelPZ70Knobs.HDG_BUTTON),
                new MultiPanelKnob(1, 1 << 1, true, MultiPanelPZ70Knobs.NAV_BUTTON),
                new MultiPanelKnob(1, 1 << 2, true, MultiPanelPZ70Knobs.IAS_BUTTON),
                new MultiPanelKnob(1, 1 << 3, true, MultiPanelPZ70Knobs.ALT_BUTTON),
                new MultiPanelKnob(1, 1 << 4, true, MultiPanelPZ70Knobs.VS_BUTTON),
                new MultiPanelKnob(1, 1 << 5, true, MultiPanelPZ70Knobs.APR_BUTTON),
                new MultiPanelKnob(1, 1 << 6, true, MultiPanelPZ70Knobs.REV_BUTTON),
                new MultiPanelKnob(1, 1 << 7, true, MultiPanelPZ70Knobs.AUTO_THROTTLE),
                // Group 2
                new MultiPanelKnob(2, 1 << 0, true, MultiPanelPZ70Knobs.FLAPS_LEVER_UP),
                new MultiPanelKnob(2, 1 << 1, true, MultiPanelPZ70Knobs.FLAPS_LEVER_DOWN),
                new MultiPanelKnob(2, 1 << 2, true, MultiPanelPZ70Knobs.PITCH_TRIM_WHEEL_UP),
                new MultiPanelKnob(2, 1 << 3, true, MultiPanelPZ70Knobs.PITCH_TRIM_WHEEL_DOWN)
            };

            return result;
        }
    }

}
/*
     Byte #1
    00000000
    ||||||||_ KNOB_ALT
    |||||||_ KNOB_VS
    ||||||_ KNOB_IAS
    |||||_ KNOB_HDG
    ||||_ KNOB_CRS
    |||_ LCD WHEEL INC 
    ||_ LCD WHEEL DEC
    |_ AP_BUTTON

    Byte #2
    00000000
    ||||||||_ HDG_BUTTON
    |||||||_ NAV_BUTTON
    ||||||_ IAS_BUTTON
    |||||_ ALT_BUTTON
    ||||_ VS_BUTTON
    |||_ APR_BUTTON
    ||_ REV_BUTTON
    |_ AUTO THROTTLE

    Byte #3
    00000000
    ||||||||_ FLAPS UP 
    |||||||_ FLAPS DOWN
    ||||||_ PITCH TRIM DOWN
    |||||_ PITCH TRIM UP
    ||||_ 
    |||_ 
    ||_ 
    |_ 



    Bytes sent to the PZ70:

    1 Report ID byte(0x0) + 11 payload bytes

    h p1 p2 p3 p4 p5 p6 p7 p8 p9 p10 p11
    |  |  |  |  |  |  |  |  |  |  |  |_ Lights LCD buttons on/off (LCD Button Byte)
    |  |  |  |  |  |  |  |  |  |  |_ Rightmost number on lower LCD row (0x0-0x9, 0xA above will darken the digit position, except 0xEE which will show a dash)
    |  |  |  |  |  |  |  |  |  |_ 
    |  |  |  |  |  |  |  |  |_ 
    |  |  |  |  |  |  |  |_ 
    |  |  |  |  |  |  |_ Leftmost number on lower LCD row (0x0-0x9, 0xA above will darken the digit position, except 0xEE which will show a dash)
    |  |  |  |  |  |_ Rightmost number on upper LCD row (0x0-0x9, 0xA above will darken the digit position)
    |  |  |  |  |_ 
    |  |  |  |_ 
    |  |  |_ 
    |  |_ Leftmost number on upper LCD row (0x0-0x9, 0xA above will darken the digit position)
    |_ Report ID byte, always 0x0

    The leftmost text in the display is set by the panel itself when it receives feature data.
    "ALT / VS"
    "IAS"
    "HDG"
    "CRS"

    The panel limits which digit can be displayed depending on which mode is selected.

    ALT / VS : p1 - p10
    IAS : p4 - p6
    HDG : p4 - p6
    CRS : p4 - p6

    LCD Button Byte
    00000000
    ||||||||_ AP_BUTTON
    |||||||_ HDG_BUTTON
    ||||||_ NAV_BUTTON
    |||||_ IAS_BUTTON
    ||||_ ALT_BUTTON
    |||_ VS_BUTTON
    ||_ APR_BUTTON
    |_ REV_BUTTON

 */