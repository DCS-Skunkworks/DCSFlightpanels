namespace DCS_BIOS
{
    using DCS_BIOS.Interfaces;

    public static class DCSBIOSStringManager
    {
        private static DCSBIOSStringListener _dcsbiosStringListener;

        private static void AddAddress(uint address, int length)
        {
            CheckInstance();
            _dcsbiosStringListener.AddStringAddress(address, length);
        }

        public static void AddListener(uint address, int length, IDCSBIOSStringListener dcsbiosStringListener)
        {
            CheckInstance();
            AddAddress(address, length);
            _dcsbiosStringListener.Attach(dcsbiosStringListener);
        }

        public static void AddListener(DCSBIOSOutput dcsbiosOutput, IDCSBIOSStringListener dcsbiosStringListener)
        {
            CheckInstance();
            AddAddress(dcsbiosOutput.Address, dcsbiosOutput.MaxLength);
            _dcsbiosStringListener.Attach(dcsbiosStringListener);
        }

        private static void CheckInstance()
        {
            if (_dcsbiosStringListener == null)
            {
                _dcsbiosStringListener = new DCSBIOSStringListener();
            }
        }

        public static void DetachListener(IDCSBIOSStringListener dcsbiosStringListener)
        {
            CheckInstance();
            _dcsbiosStringListener.Detach(dcsbiosStringListener);
        }

        public static void Close()
        {
            _dcsbiosStringListener = null;
        }
    }
}
