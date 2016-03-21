using System;
using System.Collections.Generic;

namespace NonVisuals
{
    public enum TPMPanelSwitches
    {
        G1 = 0,
        G2 = 2,
        G3 = 4,
        G4 = 8,
        G5 = 16,
        G6 = 32,
        G7 = 64,
        G8 = 128,
        G9 = 256
    }

    public class TPMPanelSwitch
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
            return "TPMPanelSwitch{" + Enum.GetName(typeof (TPMPanelSwitches), TPMSwitch) + "}";
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
            //TPMPanelSwitch{G1}
            var dataString = str.Remove(0, 15);
            //G1}
            dataString = dataString.Remove(dataString.Length - 1, 1);
            //G1
            TPMSwitch = (TPMPanelSwitches) Enum.Parse(typeof (TPMPanelSwitches), dataString.Trim());
        }

        public static HashSet<TPMPanelSwitch> GetTPMPanelSwitches()
        {
            var result = new HashSet<TPMPanelSwitch>();
            //Group 1
            result.Add(new TPMPanelSwitch(3, Convert.ToInt32("00001000", 2), false, TPMPanelSwitches.G1));
            result.Add(new TPMPanelSwitch(3, Convert.ToInt32("00010000", 2), false, TPMPanelSwitches.G2));
            result.Add(new TPMPanelSwitch(3, Convert.ToInt32("00100000", 2), false, TPMPanelSwitches.G3));
            result.Add(new TPMPanelSwitch(3, Convert.ToInt32("01000000", 2), false, TPMPanelSwitches.G4));
            result.Add(new TPMPanelSwitch(3, Convert.ToInt32("10000000", 2), false, TPMPanelSwitches.G5));

            //Group 0
            result.Add(new TPMPanelSwitch(4, Convert.ToInt32("1", 2), false, TPMPanelSwitches.G6));
            result.Add(new TPMPanelSwitch(4, Convert.ToInt32("10", 2), false, TPMPanelSwitches.G7));
            result.Add(new TPMPanelSwitch(4, Convert.ToInt32("100", 2), false, TPMPanelSwitches.G8));
            result.Add(new TPMPanelSwitch(4, Convert.ToInt32("1000", 2), false, TPMPanelSwitches.G9));

            return result;
        }

        public class TPMPanelSwitchOnOff
        {
            private readonly TPMPanelSwitches _tpmPanelSwitch;
            private readonly bool _on;

            public TPMPanelSwitchOnOff(TPMPanelSwitches tpmPanelSwitch, bool on)
            {
                _tpmPanelSwitch = tpmPanelSwitch;
                _on = @on;
            }

            public TPMPanelSwitches TPMSwitch
            {
                get { return _tpmPanelSwitch; }
            }

            public bool On
            {
                get { return _on; }
            }
        }
    }
}
