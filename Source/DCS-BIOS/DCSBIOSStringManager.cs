namespace DCS_BIOS
{
    public static class DCSBIOSStringManager
    {
        private static DCSBIOSStringListener _dcsbiosStringListener;

        public static void AddAddress(uint address, int length)
        {
            CheckInstance();
            _dcsbiosStringListener.AddStringAddress(address, length);
        }

        public static void AddAddress(uint address, int length, IDCSBIOSStringListener dcsbiosStringListener)
        {
            CheckInstance();
            AddAddress(address, length);
            _dcsbiosStringListener.Attach(dcsbiosStringListener);
        }

        public static void AddAddress(DCSBIOSOutput dcsbiosOutput, IDCSBIOSStringListener dcsbiosStringListener)
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

        public static void Detach(IDCSBIOSStringListener dcsbiosStringListener)
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
