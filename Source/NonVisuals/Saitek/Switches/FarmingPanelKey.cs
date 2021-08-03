using System;
using System.Collections.Generic;
using NonVisuals.Interfaces;

namespace NonVisuals.Saitek.Switches
{
    public enum FarmingPanelMKKeys
    {
        BUTTON_1 = 0,
        BUTTON_2 = 1,
        BUTTON_3 = 2,
        BUTTON_4 = 3,
        BUTTON_5 = 4,
        BUTTON_6 = 5,
        BUTTON_7 = 6,
        BUTTON_8 = 7,
        BUTTON_9 = 8,
        BUTTON_10 = 9,
        BUTTON_11 = 10,
        BUTTON_12 = 11,
        BUTTON_13 = 12,
        BUTTON_14 = 13,
        BUTTON_15 = 14,
        BUTTON_16 = 15,
        BUTTON_17 = 16,
        BUTTON_18 = 17,
        BUTTON_19 = 18,
        BUTTON_20 = 19,
        BUTTON_21 = 20,
        BUTTON_22 = 21,
        BUTTON_23 = 22,
        BUTTON_24 = 23,
        BUTTON_25 = 24,
        BUTTON_26 = 25,
        BUTTON_27 = 26,
        BUTTON_JOY_RIGHT = 27,
        BUTTON_JOY_LEFT = 28
    }

    public class FarmingPanelKey : ISaitekPanelKnob
    {

        public FarmingPanelKey(int group, int mask, bool isOn, FarmingPanelMKKeys farmingPanelKey)
        {
            Group = group;
            Mask = mask;
            IsOn = isOn;
            FarmingPanelMKKey = farmingPanelKey;
        }

        public int Group { get; set; }

        public int Mask { get; set; }

        public bool IsOn { get; set; }

        public FarmingPanelMKKeys FarmingPanelMKKey { get; set; }

        public static HashSet<ISaitekPanelKnob> GetPanelFarmingPanelKeys()
        {
            var result = new HashSet<ISaitekPanelKnob>();
            //Group 0
            result.Add(new FarmingPanelKey(0, Convert.ToInt32("1", 2), false, FarmingPanelMKKeys.BUTTON_1));
            result.Add(new FarmingPanelKey(0, Convert.ToInt32("10", 2), false, FarmingPanelMKKeys.BUTTON_2));
            result.Add(new FarmingPanelKey(0, Convert.ToInt32("100", 2), false, FarmingPanelMKKeys.BUTTON_3));
            result.Add(new FarmingPanelKey(0, Convert.ToInt32("1000", 2), false, FarmingPanelMKKeys.BUTTON_4));
            result.Add(new FarmingPanelKey(0, Convert.ToInt32("10000", 2), false, FarmingPanelMKKeys.BUTTON_5));
            result.Add(new FarmingPanelKey(0, Convert.ToInt32("100000", 2), false, FarmingPanelMKKeys.BUTTON_6));
            result.Add(new FarmingPanelKey(0, Convert.ToInt32("1000000", 2), false, FarmingPanelMKKeys.BUTTON_7));
            result.Add(new FarmingPanelKey(0, Convert.ToInt32("10000000", 2), false, FarmingPanelMKKeys.BUTTON_8));

            //Group 1
            result.Add(new FarmingPanelKey(1, Convert.ToInt32("1", 2), false, FarmingPanelMKKeys.BUTTON_9));
            result.Add(new FarmingPanelKey(1, Convert.ToInt32("10", 2), false, FarmingPanelMKKeys.BUTTON_10));
            result.Add(new FarmingPanelKey(1, Convert.ToInt32("100", 2), false, FarmingPanelMKKeys.BUTTON_11));
            result.Add(new FarmingPanelKey(1, Convert.ToInt32("1000", 2), false, FarmingPanelMKKeys.BUTTON_12));
            result.Add(new FarmingPanelKey(1, Convert.ToInt32("10000", 2), false, FarmingPanelMKKeys.BUTTON_13));
            result.Add(new FarmingPanelKey(1, Convert.ToInt32("100000", 2), false, FarmingPanelMKKeys.BUTTON_14));
            result.Add(new FarmingPanelKey(1, Convert.ToInt32("1000000", 2), false, FarmingPanelMKKeys.BUTTON_15));
            result.Add(new FarmingPanelKey(1, Convert.ToInt32("10000000", 2), false, FarmingPanelMKKeys.BUTTON_16));

            //Group 2
            result.Add(new FarmingPanelKey(1, Convert.ToInt32("1", 2), false, FarmingPanelMKKeys.BUTTON_17));
            result.Add(new FarmingPanelKey(1, Convert.ToInt32("10", 2), false, FarmingPanelMKKeys.BUTTON_18));
            result.Add(new FarmingPanelKey(1, Convert.ToInt32("100", 2), false, FarmingPanelMKKeys.BUTTON_19));
            result.Add(new FarmingPanelKey(1, Convert.ToInt32("1000", 2), false, FarmingPanelMKKeys.BUTTON_20));
            result.Add(new FarmingPanelKey(1, Convert.ToInt32("10000", 2), false, FarmingPanelMKKeys.BUTTON_21));
            result.Add(new FarmingPanelKey(1, Convert.ToInt32("100000", 2), false, FarmingPanelMKKeys.BUTTON_22));
            result.Add(new FarmingPanelKey(1, Convert.ToInt32("1000000", 2), false, FarmingPanelMKKeys.BUTTON_23));
            result.Add(new FarmingPanelKey(1, Convert.ToInt32("10000000", 2), false, FarmingPanelMKKeys.BUTTON_24));


            //Group 3
            result.Add(new FarmingPanelKey(1, Convert.ToInt32("1", 2), false, FarmingPanelMKKeys.BUTTON_25));
            result.Add(new FarmingPanelKey(1, Convert.ToInt32("10", 2), false, FarmingPanelMKKeys.BUTTON_26));
            result.Add(new FarmingPanelKey(1, Convert.ToInt32("100", 2), false, FarmingPanelMKKeys.BUTTON_27));
            result.Add(new FarmingPanelKey(1, Convert.ToInt32("1000", 2), false, FarmingPanelMKKeys.BUTTON_JOY_LEFT));
            result.Add(new FarmingPanelKey(1, Convert.ToInt32("10000000", 2), false, FarmingPanelMKKeys.BUTTON_JOY_RIGHT));

            return result;
        }
    }
}
