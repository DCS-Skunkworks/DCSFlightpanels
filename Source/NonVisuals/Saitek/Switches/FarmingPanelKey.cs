namespace NonVisuals.Saitek.Switches
{
    using System.Collections.Generic;

    using MEF;

    using NonVisuals.Interfaces;

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
            var result = new HashSet<ISaitekPanelKnob>
            {
                // Group 0
                new FarmingPanelKey(0, 1 << 0, false, FarmingPanelMKKeys.BUTTON_1),
                new FarmingPanelKey(0, 1 << 1, false, FarmingPanelMKKeys.BUTTON_2),
                new FarmingPanelKey(0, 1 << 2, false, FarmingPanelMKKeys.BUTTON_3),
                new FarmingPanelKey(0, 1 << 3, false, FarmingPanelMKKeys.BUTTON_4),
                new FarmingPanelKey(0, 1 << 4, false, FarmingPanelMKKeys.BUTTON_5),
                new FarmingPanelKey(0, 1 << 5, false, FarmingPanelMKKeys.BUTTON_6),
                new FarmingPanelKey(0, 1 << 6, false, FarmingPanelMKKeys.BUTTON_7),
                new FarmingPanelKey(0, 1 << 7, false, FarmingPanelMKKeys.BUTTON_8),
                // Group 1
                new FarmingPanelKey(1, 1 << 0, false, FarmingPanelMKKeys.BUTTON_9),
                new FarmingPanelKey(1, 1 << 1, false, FarmingPanelMKKeys.BUTTON_10),
                new FarmingPanelKey(1, 1 << 2, false, FarmingPanelMKKeys.BUTTON_11),
                new FarmingPanelKey(1, 1 << 3, false, FarmingPanelMKKeys.BUTTON_12),
                new FarmingPanelKey(1, 1 << 4, false, FarmingPanelMKKeys.BUTTON_13),
                new FarmingPanelKey(1, 1 << 5, false, FarmingPanelMKKeys.BUTTON_14),
                new FarmingPanelKey(1, 1 << 6, false, FarmingPanelMKKeys.BUTTON_15),
                new FarmingPanelKey(1, 1 << 7, false, FarmingPanelMKKeys.BUTTON_16),
                // Group 2
                new FarmingPanelKey(2, 1 << 0, false, FarmingPanelMKKeys.BUTTON_17),
                new FarmingPanelKey(2, 1 << 1, false, FarmingPanelMKKeys.BUTTON_18),
                new FarmingPanelKey(2, 1 << 2, false, FarmingPanelMKKeys.BUTTON_19),
                new FarmingPanelKey(2, 1 << 3, false, FarmingPanelMKKeys.BUTTON_20),
                new FarmingPanelKey(2, 1 << 4, false, FarmingPanelMKKeys.BUTTON_21),
                new FarmingPanelKey(2, 1 << 5, false, FarmingPanelMKKeys.BUTTON_22),
                new FarmingPanelKey(2, 1 << 6, false, FarmingPanelMKKeys.BUTTON_23),
                new FarmingPanelKey(2, 1 << 7, false, FarmingPanelMKKeys.BUTTON_24),
                // Group 3
                new FarmingPanelKey(3, 1 << 0, false, FarmingPanelMKKeys.BUTTON_25),
                new FarmingPanelKey(3, 1 << 1, false, FarmingPanelMKKeys.BUTTON_26),
                new FarmingPanelKey(3, 1 << 2, false, FarmingPanelMKKeys.BUTTON_27),
                new FarmingPanelKey(3, 1 << 3, false, FarmingPanelMKKeys.BUTTON_JOY_LEFT),
                new FarmingPanelKey(3, 1 << 7, false, FarmingPanelMKKeys.BUTTON_JOY_RIGHT) //I wonder why it's 7 here ? Should be 4 IMO?
            };

            return result;
        }
    }
}
