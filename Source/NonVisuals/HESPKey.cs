using System;
using System.Collections.Generic;

namespace NonVisuals
{
    public enum HESPKeys
    {
        BUTTON1,
        BUTTON2,
        BUTTON3,
        BUTTON4,
        BUTTON5,
        BUTTON6,
        BUTTON7,
        BUTTON8,
        BUTTON9,
        BUTTON10,
        BUTTON11,
        BUTTON12,
        BUTTON13,
        BUTTON14,
        BUTTON15,
        BUTTON16,
        BUTTON17,
        BUTTON18,
        BUTTON19,
        BUTTON20,
        BUTTON21,
        BUTTON22,
        BUTTON23,
        BUTTON24,
        BUTTON25,
        BUTTON26
    }

    public class HESPKey
    {
        public HESPKey()
        {}

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
            //HESPKey{BUTTON1}
            var dataString = str.Replace("HESPKey{", "").Replace("}","");
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
