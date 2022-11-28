namespace NonVisuals
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    using MEF;

    using Newtonsoft.Json;



    /// <summary>
    /// This class contains all information needed for a certain key emulation.
    /// This includes e.g.key(s) to press, breaks in between presses and length
    /// of the actual keypresses.
    /// </summary>
    [Serializable]
    public class KeyPressInfo : IKeyPressInfo
    {
        private KeyPressLength _lengthOfBreak = KeyPressLength.FiftyMilliSec;
        private KeyPressLength _lengthOfKeyPress = KeyPressLength.FiftyMilliSec;
        private HashSet<VirtualKeyCode> _virtualKeyCodes = new();


        public int GetHash()
        {
            unchecked
            {
                var result = 0;
                foreach (var virtualKeyCode in _virtualKeyCodes)
                {
                    result = (result * 397) ^ virtualKeyCode.GetHashCode();
                }

                result = (result * 397) ^ _lengthOfBreak.GetHashCode();
                result = (result * 397) ^ _lengthOfKeyPress.GetHashCode();
                return result;
            }
        }

        [JsonProperty("LengthOfBreak", Required = Required.Default)]
        public KeyPressLength LengthOfBreak
        {
            get => _lengthOfBreak;
            set => _lengthOfBreak = value;
        }

        [JsonProperty("LengthOfKeyPress", Required = Required.Default)]
        public KeyPressLength LengthOfKeyPress
        {
            get => _lengthOfKeyPress;
            set => _lengthOfKeyPress = value;
        }

        [JsonProperty("VirtualKeyCodes", Required = Required.Default)]
        public HashSet<VirtualKeyCode> VirtualKeyCodes
        {
            get => _virtualKeyCodes;
            set => _virtualKeyCodes = value;
        }

        [JsonIgnore]
        public string VirtualKeyCodesAsString
        {
            get
            {
                var result = new StringBuilder();
                if (_virtualKeyCodes.Count > 0)
                {
                    foreach (var virtualKeyCode in _virtualKeyCodes)
                    {
                        if (result.Length > 0)
                        {
                            result.Append(" + ");
                        }

                        result.Append(Enum.GetName(typeof(VirtualKeyCode), virtualKeyCode));
                    }
                }

                return result.ToString();
            }
        }
    }
}
