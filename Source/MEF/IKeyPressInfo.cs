namespace MEF
{
    using System.Collections.Generic;

    public interface IKeyPressInfo
    {
        int GetHash();

        KeyPressLength LengthOfBreak { get; set; }

        KeyPressLength LengthOfKeyPress { get; set; }

        HashSet<MEF.VirtualKeyCode> VirtualKeyCodes { get; set; }

        string VirtualKeyCodesAsString { get; }
    }
}
