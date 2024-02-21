using System.Threading.Tasks;
using DCS_BIOS.Interfaces;

namespace DCS_BIOS.EventArgs
{
    public static class BIOSEventHandler
    {
        /*
         * Source of data from DCS-BIOS, parsed by ProtocolParser
         */
        public delegate void DcsDataAddressValueEventHandler(object sender, DCSBIOSDataEventArgs e);
        public static event DcsDataAddressValueEventHandler OnDcsDataAddressValue;
        
        public static bool OnDcsDataAddressValueEventSubscribed()
        {
            return OnDcsDataAddressValue != null && OnDcsDataAddressValue.GetInvocationList().Length > 0;
        }

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
            OnDcsDataAddressValue?.Invoke(sender, new DCSBIOSDataEventArgs { Address = address, Data = data });
        }        
        
        /*
         * Source of data from DCS-BIOS, this is not parsed by ProtocolParser, instead
         * passed on as is.
         */
        public delegate void DcsBulkDataEventHandler(object sender, DCSBIOSBulkDataEventArgs e);
        public static event DcsBulkDataEventHandler OnDcsBulkData;

        public static bool OnDcsBulkDataEventSubscribed()
        {
            return OnDcsBulkData != null && OnDcsBulkData.GetInvocationList().Length > 0;
        }

        public static void AttachBulkDataListener(IDcsBiosBulkDataListener biosBulkDataListener)
        {
            OnDcsBulkData += biosBulkDataListener.DcsBiosBulkDataReceived;
        }

        public static void DetachBulkDataListener(IDcsBiosBulkDataListener biosBulkDataListener)
        {
            OnDcsBulkData -= biosBulkDataListener.DcsBiosBulkDataReceived;
        }

        public static void DCSBIOSBulkDataAvailable(object sender, byte[] data)
        {
            OnDcsBulkData?.Invoke(sender, new DCSBIOSBulkDataEventArgs { Data = data });
        }

        public delegate Task AsyncDcsBulkDataEventHandler(object sender, DCSBIOSBulkDataEventArgs e);
        public static event AsyncDcsBulkDataEventHandler AsyncOnDcsBulkData;

        public static bool OnAsyncDcsBulkDataEventSubscribed()
        {
            return AsyncOnDcsBulkData != null && AsyncOnDcsBulkData.GetInvocationList().Length > 0;
        }

        public static void AttachAsyncBulkDataListener(IAsyncDcsBiosBulkDataListener asyncBulkDataListener)
        {
            AsyncOnDcsBulkData += asyncBulkDataListener.AsyncDcsBiosBulkDataReceived;
        }

        public static void DetachAsyncBulkDataListener(IAsyncDcsBiosBulkDataListener asyncBulkDataListener)
        {
            AsyncOnDcsBulkData -= asyncBulkDataListener.AsyncDcsBiosBulkDataReceived;
        }

        public static void AsyncDCSBIOSBulkDataAvailable(object sender, byte[] data)
        {
            AsyncOnDcsBulkData?.Invoke(sender, new DCSBIOSBulkDataEventArgs { Data = data });
        }

        /*
         * Used for listening whether data comes from DCS-BIOS (UI spinning cog wheel for example)
         */
        public delegate void DcsConnectionActiveEventHandler(object sender, DCSBIOSConnectionEventArgs e);
        public static event DcsConnectionActiveEventHandler OnDcsConnectionActive;

        public static bool OnDcsConnectionActiveEventSubscribed()
        {
            return OnDcsConnectionActive != null && OnDcsConnectionActive.GetInvocationList().Length > 0;
        }

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

        public static bool OnDCSBIOSStringReceivedEventSubscribed()
        {
            return OnDCSBIOSStringReceived != null && OnDCSBIOSStringReceived.GetInvocationList().Length > 0;
        }

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
            OnDCSBIOSStringReceived?.Invoke(sender, new DCSBIOSStringDataEventArgs { Address = address, StringData = data });
        }
    }
}
