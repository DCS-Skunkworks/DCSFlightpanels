namespace NonVisuals.Panels.Saitek.Switches
{
    using System;
    using System.Collections.Generic;

    using MEF;

    using Interfaces;


    /*
     * Logitech TPM Panel Switch, contains only the switch, no information about
     * what action(s) the switch should perform.
     * Used by the panel to get information on what switch(es) has been switched
     */
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
            var result = new HashSet<ISaitekPanelKnob>
            {
                // Group 1
                new TPMPanelSwitch(3, 1 << 3, false, TPMPanelSwitches.G1),
                new TPMPanelSwitch(3, 1 << 4, false, TPMPanelSwitches.G2),
                new TPMPanelSwitch(3, 1 << 5, false, TPMPanelSwitches.G3),
                new TPMPanelSwitch(3, 1 << 6, false, TPMPanelSwitches.G4),
                new TPMPanelSwitch(3, 1 << 7, false, TPMPanelSwitches.G5),
                // Group 0
                new TPMPanelSwitch(4, 1 << 0, false, TPMPanelSwitches.G6),
                new TPMPanelSwitch(4, 1 << 1, false, TPMPanelSwitches.G7),
                new TPMPanelSwitch(4, 1 << 2, false, TPMPanelSwitches.G8),
                new TPMPanelSwitch(4, 1 << 3, false, TPMPanelSwitches.G9)
            };

            return result;
        }
    }
    /*
        TPM (only toggle switches) first 3 bytes for TPM rods.


        Byte 4:
        00000000
        ||||||||_ 
        |||||||_ 
        ||||||_
        |||||_ G1
        ||||_ G2
        |||_ G3
        ||_ G4
        |_ G5


        Byte 5:
        00000000 
        ||||||||_ G6
        |||||||_ G7
        ||||||_ G8
        |||||_ G9
        ||||_
        |||_
        ||_
        |_

     */
}
