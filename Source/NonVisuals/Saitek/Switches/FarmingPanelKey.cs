using System;
using System.Collections.Generic;
using NonVisuals.Interfaces;

namespace NonVisuals.Saitek.Switches
{
    using MEF;

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
            result.Add(new FarmingPanelKey(2, Convert.ToInt32("1", 2), false, FarmingPanelMKKeys.BUTTON_17));
            result.Add(new FarmingPanelKey(2, Convert.ToInt32("10", 2), false, FarmingPanelMKKeys.BUTTON_18));
            result.Add(new FarmingPanelKey(2, Convert.ToInt32("100", 2), false, FarmingPanelMKKeys.BUTTON_19));
            result.Add(new FarmingPanelKey(2, Convert.ToInt32("1000", 2), false, FarmingPanelMKKeys.BUTTON_20));
            result.Add(new FarmingPanelKey(2, Convert.ToInt32("10000", 2), false, FarmingPanelMKKeys.BUTTON_21));
            result.Add(new FarmingPanelKey(2, Convert.ToInt32("100000", 2), false, FarmingPanelMKKeys.BUTTON_22));
            result.Add(new FarmingPanelKey(2, Convert.ToInt32("1000000", 2), false, FarmingPanelMKKeys.BUTTON_23));
            result.Add(new FarmingPanelKey(2, Convert.ToInt32("10000000", 2), false, FarmingPanelMKKeys.BUTTON_24));


            //Group 3
            result.Add(new FarmingPanelKey(3, Convert.ToInt32("1", 2), false, FarmingPanelMKKeys.BUTTON_25));
            result.Add(new FarmingPanelKey(3, Convert.ToInt32("10", 2), false, FarmingPanelMKKeys.BUTTON_26));
            result.Add(new FarmingPanelKey(3, Convert.ToInt32("100", 2), false, FarmingPanelMKKeys.BUTTON_27));
            result.Add(new FarmingPanelKey(3, Convert.ToInt32("1000", 2), false, FarmingPanelMKKeys.BUTTON_JOY_LEFT));
            result.Add(new FarmingPanelKey(3, Convert.ToInt32("10000000", 2), false, FarmingPanelMKKeys.BUTTON_JOY_RIGHT));

            return result;
        }
    }
}
