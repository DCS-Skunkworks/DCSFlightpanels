using DCS_BIOS.Serialized;

namespace DCS_BIOS.StringClasses
{
    /// <summary>
    /// Easy wrapper for listening to DCS-BIOS strings.
    /// </summary>
    public static class DCSBIOSStringManager
    {
        private static DCSBIOSStringListener _dcsbiosStringListener;

        private static void AddAddress(uint address, int length)
        {
            CheckInstance();
            _dcsbiosStringListener.AddStringAddress(address, length);
        }

        public static void AddListeningAddress(DCSBIOSOutput dcsbiosOutput)
        {
            CheckInstance();
            AddAddress(dcsbiosOutput.Address, dcsbiosOutput.MaxLength);
        }

        private static void CheckInstance()
        {
            _dcsbiosStringListener ??= new DCSBIOSStringListener();
        }

        public static void Close()
        {
            _dcsbiosStringListener?.Dispose();
        }
    }
}
