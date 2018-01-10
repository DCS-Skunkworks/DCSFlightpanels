using System;
using System.Collections.Generic;

namespace NonVisuals
{
    public enum HESPKeys
    {
        BUTTON1 = 0,
        BUTTON2 = 2,
        BUTTON3 = 4,
        BUTTON4 = 8,
        BUTTON5 = 16,
        BUTTON6 = 32,
        BUTTON7 = 64,
        BUTTON8 = 128,
        BUTTON9 = 256,
        BUTTON10 = 512,
        BUTTON11 = 1024,
        BUTTON12 = 2048,
        BUTTON13 = 4096,
        BUTTON14 = 8192,
        BUTTON15 = 16384,
        BUTTON17 = 32768,
        BUTTON18 = 65536,
        BUTTON19 = 131072,
        BUTTON20 = 262144,
        BUTTON21 = 524288,
        BUTTON22 = 524288,
        BUTTON23 = 524288,
        BUTTON24 = 524288,
        BUTTON25 = 524288,
    }

    public class HESPKey
    {
        public HESPKey()
        {
        }

        public HESPKey(int group, int mask, bool isOn, HESPKeys hespKey)
        {
            Group = group;
            Mask = mask;
            IsOn = isOn;
            Key = hespKey;
        }

        public int Group { get; set; }

        public int Mask { get; set; }

        public bool IsOn { get; set; }

        public HESPKeys Key { get; set; }

        public string ExportString()
        {
            return "HESPKey{" + Enum.GetName(typeof(HESPKeys), Key) + "}";
        }

        public void ImportString(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                throw new ArgumentException("Import string empty. (HESPKey)");
            }
            if (!str.StartsWith("HESPKey{") || !str.EndsWith("}"))
            {
                throw new ArgumentException("Import string format exception. (HESPKey) >" + str + "<");
            }
            //HESPKey{SWITCHKEY_MASTER_ALT}
            var dataString = str.Remove(0, 15);
            //SWITCHKEY_MASTER_ALT}
            dataString = dataString.Remove(dataString.Length - 1, 1);
            //SWITCHKEY_MASTER_ALT
            Key = (HESPKeys)Enum.Parse(typeof(HESPKeys), dataString.Trim());
        }

        public static HashSet<HESPKey> GetHESPKeys()
        {
            var result = new HashSet<HESPKey>();
            //Group 0
            result.Add(new HESPKey(0, Convert.ToInt32("1", 2), false, HESPKeys.BUTTON1));
            result.Add(new HESPKey(0, Convert.ToInt32("10", 2), false, HESPKeys.BUTTON1));
            result.Add(new HESPKey(0, Convert.ToInt32("100", 2), false, HESPKeys.BUTTON1));
            result.Add(new HESPKey(0, Convert.ToInt32("1000", 2), false, HESPKeys.BUTTON1));
            result.Add(new HESPKey(0, Convert.ToInt32("10000", 2), false, HESPKeys.BUTTON1));
            result.Add(new HESPKey(0, Convert.ToInt32("100000", 2), false, HESPKeys.BUTTON1));
            result.Add(new HESPKey(0, Convert.ToInt32("1000000", 2), false, HESPKeys.BUTTON1));
            result.Add(new HESPKey(0, Convert.ToInt32("10000000", 2), false, HESPKeys.BUTTON1));

            //Group 1
            result.Add(new HESPKey(1, Convert.ToInt32("1", 2), false, HESPKeys.BUTTON1));
            result.Add(new HESPKey(1, Convert.ToInt32("10", 2), false, HESPKeys.BUTTON1));
            result.Add(new HESPKey(1, Convert.ToInt32("100", 2), false, HESPKeys.BUTTON1));
            result.Add(new HESPKey(1, Convert.ToInt32("1000", 2), false, HESPKeys.BUTTON1));
            result.Add(new HESPKey(1, Convert.ToInt32("10000", 2), false, HESPKeys.BUTTON1));
            result.Add(new HESPKey(1, Convert.ToInt32("100000", 2), false, HESPKeys.BUTTON1));
            result.Add(new HESPKey(1, Convert.ToInt32("1000000", 2), false, HESPKeys.BUTTON1));
            result.Add(new HESPKey(1, Convert.ToInt32("10000000", 2), false, HESPKeys.BUTTON1));

            //Group 2
            result.Add(new HESPKey(2, Convert.ToInt32("1", 2), false, HESPKeys.BUTTON1));
            result.Add(new HESPKey(2, Convert.ToInt32("10", 2), false, HESPKeys.BUTTON1));
            result.Add(new HESPKey(2, Convert.ToInt32("100", 2), false, HESPKeys.BUTTON1));
            result.Add(new HESPKey(2, Convert.ToInt32("1000", 2), false, HESPKeys.BUTTON1));
            return result;
        }











        /*
         * public String ExportString()
        {
            return "HESPKey{" + Group + "," + Mask + "," + Enum.GetName(typeof(HESPKeys), HESPKey) + "}";
        }

        public void ImportString(String str)
        {
            if (String.IsNullOrEmpty(str))
            {
                throw new ArgumentException("Import string empty. (HESPKey)");
            }
            if (!str.StartsWith("HESPKey{") || !str.EndsWith("}"))
            {
                throw new ArgumentException("Import string format exception. (HESPKey) >" + str + "<");
            }
            //HESPKey{1,255,SWITCHKEY_MASTER_ALT}
            var dataString = str.Remove(0, 15);
            //1,255,SWITCHKEY_MASTER_ALT}
            dataString = dataString.Remove(dataString.Length - 1, 1);
            //1,255,SWITCHKEY_MASTER_ALT
            Group = int.Parse(dataString.Substring(0, 1));
            dataString = dataString.Substring(2);
            //255,SWITCHKEY_MASTER_ALT
            Mask = int.Parse(dataString.Substring(0,dataString.IndexOf(",", StringComparison.Ordinal)));
            HESPKey = (HESPKeys)Enum.Parse(typeof(HESPKeys), dataString.Substring(dataString.IndexOf(",", StringComparison.Ordinal) +1 ));
        }
         */
    }

    public class HESPKeyOnOff
    {
        private readonly HESPKeys _hespKey;
        private readonly bool _on;

        public HESPKeyOnOff(HESPKeys hespKey, bool on)
        {
            _hespKey = hespKey;
            _on = @on;
        }

        public HESPKeys HESPKey
        {
            get { return _hespKey; }
        }

        public bool On
        {
            get { return _on; }
        }
    }
}
