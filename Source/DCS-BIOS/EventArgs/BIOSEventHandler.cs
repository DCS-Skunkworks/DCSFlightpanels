using DCS_BIOS.Interfaces;

namespace DCS_BIOS.EventArgs
{
    public static class BIOSEventHandler
    {
        /*
         * Source of data from DCS-BIOS
         */
        public delegate void DcsDataAddressValueEventHandler(object sender, DCSBIOSDataEventArgs e);
        public static event DcsDataAddressValueEventHandler OnDcsDataAddressValue;

        public static void AttachDataListener(IDcsBiosDataListener dcsBiosDataListener)
        {
            OnDcsDataAddressValue += dcsBiosDataListener.DcsBiosDataReceived;
        }
        
        public static void DetachDataListener(IDcsBiosDataListener dcsBiosDataListener)
        {
            OnDcsDataAddressValue -= dcsBiosDataListener.DcsBiosDataReceived;
        }

        public static void DCSBIOSDataAvailable(object sender, uint address, uint data)
        {
            OnDcsDataAddressValue?.Invoke(sender, new DCSBIOSDataEventArgs() { Address = address, Data = data });
        }

        /*
         * Used for listening whether data comes from DCS-BIOS (UI spinning cog wheel for example)
         */
        public delegate void DcsConnectionActiveEventHandler(object sender, DCSBIOSConnectionEventArgs e);
        public static event DcsConnectionActiveEventHandler OnDcsConnectionActive;

        public static void AttachConnectionListener(IDcsBiosConnectionListener connectionListener)
        {
            OnDcsConnectionActive += connectionListener.DcsBiosConnectionActive;
        }

        public static void DetachConnectionListener(IDcsBiosConnectionListener connectionListener)
        {
            OnDcsConnectionActive -= connectionListener.DcsBiosConnectionActive;
        }

        public static void ConnectionActive(object sender)
        {
            OnDcsConnectionActive?.Invoke(sender, new DCSBIOSConnectionEventArgs()); // Informs main UI that data is coming
        }
        /*
         *
         */

        public delegate void DCSBIOSStringReceived(object sender, DCSBIOSStringDataEventArgs e);

        public static event DCSBIOSStringReceived OnDCSBIOSStringReceived;

        public static void AttachStringListener(IDCSBIOSStringListener stringListener)
        {
            OnDCSBIOSStringReceived += stringListener.DCSBIOSStringReceived;
        }

        public static void DetachStringListener(IDCSBIOSStringListener stringListener)
        {
            OnDCSBIOSStringReceived -= stringListener.DCSBIOSStringReceived;
        }

        public static void DCSBIOSStringAvailable(object sender, uint address, string data)
        {
            OnDCSBIOSStringReceived(sender, new DCSBIOSStringDataEventArgs() { Address = address, StringData = data });
        }
    }
}
