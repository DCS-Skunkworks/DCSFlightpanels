/*
 * This code is based on test code provided by Darren Pursey at Mad Catz * 
 */
namespace NonVisuals
{
    using System;
    using System.Text;
    using System.Runtime.InteropServices;

    public enum ReturnValues : uint
    {
        S_OK = 0,
        E_FAIL = 0x80004005,
        E_HANDLE = 0x80070006,
        E_INVALIDARG = 0x80070057,
        E_BUFFERTOOSMALL = 0xFF04006F,
        E_PAGENOTACTIVE = 0xFF040001
    }

    public enum DeviceTypes : uint
    {
        Fip = 0,
        X52Pro = 1
    }

    public sealed class DirectOutputClass
    {
        public delegate void DeviceCallback(IntPtr device,  bool bAdded, IntPtr context);
        public delegate void EnumerateCallback(IntPtr device, IntPtr context);
        public delegate void PageCallback(IntPtr device, IntPtr page, byte bActivated, IntPtr context);
        public delegate void SoftButtonCallback(IntPtr device, IntPtr buttons, IntPtr context);

        public const string DeviceTypeFip = "3E083CD8-6A37-4A58-80A8-3D6A2C07513E";
        public const string DeviceTypeX52Pro = "29DAD506-F93B-4F20-85FA-1E02C04FAC17";

        //private const string DirectOutputDll = @"C:\Program Files (x86)\Saitek\DirectOutput\DirectOutput.dll";
        private const string DirectOutputDll = @"C:\Program Files\Saitek\DirectOutput\DirectOutput.dll";
        private const uint FLAG_SET_AS_ACTIVE = 0x00000001; // Set this page as the Active Page

        //Imports
        //-------
        [DllImport(DirectOutputDll, SetLastError = true)]
        static extern IntPtr DirectOutput_Deinitialize();
        public static ReturnValues Deinitialize() { return (ReturnValues)DirectOutput_Deinitialize(); }

        [DllImport(DirectOutputDll, SetLastError = true)]
        static extern IntPtr DirectOutput_Initialize(StringBuilder wszAppName);
        public static ReturnValues Initialize(string appName)
        {
            return (ReturnValues)DirectOutput_Initialize(new StringBuilder(appName));
        }

        [DllImport(DirectOutputDll, SetLastError = true)]
        static extern IntPtr DirectOutput_RegisterDeviceCallback([MarshalAs(UnmanagedType.FunctionPtr)]DeviceCallback pfnCallback, IntPtr pvParam);
        public static ReturnValues RegisterDeviceCallback(DeviceCallback pfnCallback)
        {
            return (ReturnValues)DirectOutput_RegisterDeviceCallback(pfnCallback, IntPtr.Zero);
        }

        [DllImport(DirectOutputDll, SetLastError = true)]
        static extern IntPtr DirectOutput_Enumerate([MarshalAs(UnmanagedType.FunctionPtr)]EnumerateCallback pfnCallback, IntPtr pvParam);
        public static ReturnValues Enumerate(EnumerateCallback pfnCallback)
        {
            return (ReturnValues)DirectOutput_Enumerate(pfnCallback, IntPtr.Zero);
        }

        [DllImport(DirectOutputDll, SetLastError = true)]
        static extern IntPtr DirectOutput_GetDeviceType(IntPtr hDevice, ref Guid pGuidType);
        internal static ReturnValues GetDeviceType(IntPtr hDevice, ref Guid pGuidType)
        {
            return (ReturnValues)DirectOutput_GetDeviceType(hDevice, ref pGuidType);
        }

        [DllImport(DirectOutputDll, SetLastError = true)]
        static extern IntPtr DirectOutput_RegisterPageCallback(IntPtr hDevice, [MarshalAs(UnmanagedType.FunctionPtr)]PageCallback pfnCallback, IntPtr pvParam);
        internal static ReturnValues RegisterPageCallback(IntPtr hDevice, PageCallback pfnCallback)
        {
            return (ReturnValues)DirectOutput_RegisterPageCallback(hDevice, pfnCallback, IntPtr.Zero);
        }

        [DllImport(DirectOutputDll, SetLastError = true)]
        static extern IntPtr DirectOutput_RegisterSoftButtonCallback(IntPtr hDevice, [MarshalAs(UnmanagedType.FunctionPtr)]SoftButtonCallback pfnCallback, IntPtr pvParam);
        internal static ReturnValues RegisterSoftButtonCallback(IntPtr hDevice, SoftButtonCallback pfnCallback)
        {
            return (ReturnValues)DirectOutput_RegisterSoftButtonCallback(hDevice, pfnCallback, IntPtr.Zero);
        }

        [DllImport(DirectOutputDll, SetLastError = true)]
        static extern IntPtr DirectOutput_AddPage(IntPtr hDevice, IntPtr dwPage, ref string wszName, IntPtr dwFlags);
        internal static ReturnValues AddPage(IntPtr hDevice, IntPtr dwPage, string wszName, bool setActive)
        {
            return (ReturnValues)DirectOutput_AddPage(hDevice, dwPage, ref wszName, setActive ? (IntPtr)FLAG_SET_AS_ACTIVE : (IntPtr)0);
        }

        [DllImport(DirectOutputDll, SetLastError = true)]
        static extern IntPtr DirectOutput_RemovePage(IntPtr hDevice, IntPtr dwPage);
        internal static ReturnValues RemovePage(IntPtr hDevice, uint dwPage)
        {
            return (ReturnValues)DirectOutput_RemovePage(hDevice, (IntPtr)dwPage);
        }

        [DllImport(DirectOutputDll, SetLastError = true)]
        static extern IntPtr DirectOutput_SetLed(IntPtr hDevice, IntPtr dwPage, IntPtr dwIndex, IntPtr dwValue);
        internal static ReturnValues SetLed(IntPtr hDevice, uint dwPage, uint dwIndex, bool on)
        {
            return (ReturnValues)DirectOutput_SetLed(hDevice, (IntPtr)dwPage, (IntPtr)dwIndex, on ? (IntPtr)1 : (IntPtr)0);
        }

        [DllImport(DirectOutputDll, SetLastError = true)]
        static extern IntPtr DirectOutput_SetString(IntPtr hDevice, IntPtr dwPage, IntPtr dwIndex, IntPtr cchValue, ref string wszValue);
        internal static ReturnValues SetString(IntPtr hDevice, uint dwPage, uint dwIndex, uint cchValue, string wszValue)
        {
            return (ReturnValues)DirectOutput_SetString(hDevice, (IntPtr)dwPage, (IntPtr)dwIndex, (IntPtr)cchValue, ref wszValue);
        }

        [DllImport(DirectOutputDll, SetLastError = true)]
        static extern IntPtr DirectOutput_SetImage(IntPtr hDevice, IntPtr dwPage, IntPtr dwIndex, IntPtr pbValue, IntPtr cbValue);
        internal static ReturnValues SetImage(IntPtr hDevice, uint dwPage, uint dwIndex, int cbValue, IntPtr pbValue)
        {
            try
            {
                return (ReturnValues)DirectOutput_SetImage(hDevice, (IntPtr)dwPage, (IntPtr)dwIndex, (IntPtr)cbValue, pbValue);
            }
            catch
            {
                return ReturnValues.E_FAIL;
            }
        }

        [DllImport(DirectOutputDll, SetLastError = true)]
        static extern IntPtr DirectOutput_SetImageFromFile(IntPtr hDevice, IntPtr dwPage, IntPtr dwIndex, IntPtr wszFilename, ref string cchFilename);
        internal static ReturnValues SetImageFromFile(IntPtr hDevice, uint dwPage, uint dwIndex, string filename)
        {
            try
            {
                return (ReturnValues)DirectOutput_SetImageFromFile(hDevice, (IntPtr)dwPage, (IntPtr)dwIndex, (IntPtr)filename.Length, ref filename);
            }
            catch
            {
                return ReturnValues.E_FAIL;
            }
        }
    }
}
