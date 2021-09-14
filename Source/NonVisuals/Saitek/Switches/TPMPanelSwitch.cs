namespace NonVisuals.Saitek.Switches
{
    using System;
    using System.Collections.Generic;

    using MEF;

    using NonVisuals.Interfaces;

    public class TPMPanelSwitch : ISaitekPanelKnob
    {
        public TPMPanelSwitch()
        {
        }

        public TPMPanelSwitch(int group, int mask, bool isOn, TPMPanelSwitches tpmPanelSwitch)
        {
            Group = group;
            Mask = mask;
            IsOn = isOn;
            TPMSwitch = tpmPanelSwitch;
        }

        public int Group { get; set; }

        public int Mask { get; set; }

        public bool IsOn { get; set; }

        public TPMPanelSwitches TPMSwitch { get; set; }

        public string ExportString()
        {
            return "TPMPanelSwitch{" + Enum.GetName(typeof(TPMPanelSwitches), TPMSwitch) + "}";
        }

        public void ImportString(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                throw new ArgumentException("Import string empty. (TPMPanelSwitch)");
            }

            if (!str.StartsWith("TPMPanelSwitch{") || !str.EndsWith("}"))
            {
                throw new ArgumentException("Import string format exception. (TPMPanelSwitch) >" + str + "<");
            }

            // TPMPanelSwitch{G1}
            var dataString = str.Remove(0, 15);

            // G1}
            dataString = dataString.Remove(dataString.Length - 1, 1);

            // G1
            TPMSwitch = (TPMPanelSwitches)Enum.Parse(typeof(TPMPanelSwitches), dataString.Trim());
        }

        public static HashSet<ISaitekPanelKnob> GetTPMPanelSwitches()
        {
            var result = new HashSet<ISaitekPanelKnob>();

            // Group 1
            result.Add(new TPMPanelSwitch(3, Convert.ToInt32("00001000", 2), false, TPMPanelSwitches.G1));
            result.Add(new TPMPanelSwitch(3, Convert.ToInt32("00010000", 2), false, TPMPanelSwitches.G2));
            result.Add(new TPMPanelSwitch(3, Convert.ToInt32("00100000", 2), false, TPMPanelSwitches.G3));
            result.Add(new TPMPanelSwitch(3, Convert.ToInt32("01000000", 2), false, TPMPanelSwitches.G4));
            result.Add(new TPMPanelSwitch(3, Convert.ToInt32("10000000", 2), false, TPMPanelSwitches.G5));

            // Group 0
            result.Add(new TPMPanelSwitch(4, Convert.ToInt32("1", 2), false, TPMPanelSwitches.G6));
            result.Add(new TPMPanelSwitch(4, Convert.ToInt32("10", 2), false, TPMPanelSwitches.G7));
            result.Add(new TPMPanelSwitch(4, Convert.ToInt32("100", 2), false, TPMPanelSwitches.G8));
            result.Add(new TPMPanelSwitch(4, Convert.ToInt32("1000", 2), false, TPMPanelSwitches.G9));

            return result;
        }
    }

}
