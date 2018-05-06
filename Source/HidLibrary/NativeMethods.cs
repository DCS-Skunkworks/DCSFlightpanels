using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace HidLibrary
{
    internal static class NativeMethods
    {
        internal const int FileFlagOverlapped = 0x40000000;
        internal const short FileShareRead = 0x1;
        internal const short FileShareWrite = 0x2;
        internal const uint GenericRead = 0x80000000;
        internal const uint GenericWrite = 0x40000000;
        internal const int AccessNone = 0;
        internal const int InvalidHandleValue = -1;
        internal const short OpenExisting = 3;
        internal const int WaitTimeout = 0x102;
        internal const uint WaitObject0 = 0;
        internal const uint WaitFailed = 0xffffffff;
        internal const uint ErrorIOPending = 0x3E5;
        internal const int WaitInfinite = 0xffff;
        [StructLayout(LayoutKind.Sequential)]
        internal struct OVERLAPPED
        {
            public int Internal;
            public int InternalHigh;
            public int Offset;
            public int OffsetHigh;
            public int hEvent;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct SecurityAttributes
        {
            public int nLength;
            public IntPtr lpSecurityDescriptor;
            public bool bInheritHandle;
        }

        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true, CharSet = CharSet.Unicode)]
        static internal extern bool CancelIo(IntPtr hFile);

        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true, CharSet = CharSet.Unicode)]
        static internal extern bool CancelIoEx(IntPtr hFile, IntPtr lpOverlapped);

        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true, CharSet = CharSet.Unicode)]
        static internal extern bool CloseHandle(IntPtr hObject);

        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true, CharSet = CharSet.Unicode)]
        static internal extern bool CancelSynchronousIo(IntPtr hObject);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        static internal extern IntPtr CreateEvent(ref SecurityAttributes securityAttributes, int bManualReset, int bInitialState, string lpName);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        static internal extern IntPtr CreateFile(string lpFileName, uint dwDesiredAccess, int dwShareMode, ref SecurityAttributes lpSecurityAttributes, int dwCreationDisposition, int dwFlagsAndAttributes, int hTemplateFile);

        [DllImport("kernel32.dll", SetLastError = true)]
        static internal extern bool ReadFile(IntPtr hFile, [Out] byte[] lpBuffer, uint nNumberOfBytesToRead, out uint lpNumberOfBytesRead, [In] ref NativeOverlapped lpOverlapped);

        //[DllImport("kernel32.dll", SetLastError = true, EntryPoint = "ReadFile")]
        //static internal extern unsafe bool ReadFileUnsafe(IntPtr handle, IntPtr bytes, uint numBytesToRead, IntPtr numBytesRead, NativeOverlapped* overlapped);

        [DllImport("kernel32.dll")]
        static internal extern uint WaitForSingleObject(IntPtr hHandle, int dwMilliseconds);

        [DllImport("kernel32.dll", SetLastError = true)]
        static internal extern bool GetOverlappedResult(IntPtr hFile, [In] ref NativeOverlapped lpOverlapped, out uint lpNumberOfBytesTransferred, bool bWait);

        //[DllImport("kernel32.dll", SetLastError = true, EntryPoint = "GetOverlappedResult")]
        //internal static extern unsafe bool GetOverlappedResultUnsafe(IntPtr hFile, NativeOverlapped* lpOverlapped, out uint lpNumberOfBytesTransferred, bool bWait);
        //internal static extern unsafe bool GetOverlappedResultUnsafe(SafeFileHandle hFile, NativeOverlapped* lpOverlapped, out uint lpNumberOfBytesTransferred, bool bWait);
        /*[DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetOverlappedResult(IntPtr hFile, ref System.Threading.NativeOverlapped lpOverlapped,out uint nNumberOfBytesTransferred,bool bWait);
        */
        [DllImport("kernel32.dll")]
        static internal extern bool WriteFile(IntPtr hFile, byte[] lpBuffer, uint nNumberOfBytesToWrite, out uint lpNumberOfBytesWritten, [In] ref NativeOverlapped lpOverlapped);

        [DllImport("kernel32.dll")]
        static internal extern bool ResetEvent(IntPtr hEvent);

        internal const int DbtDevicearrival = 0x8000;
        internal const int DbtDeviceremovecomplete = 0x8004;
        internal const int DbtDevtypDeviceinterface = 5;
        internal const int DbtDevtypHandle = 6;
        internal const int DeviceNotifyAllInterfaceClasses = 4;
        internal const int DeviceNotifyServiceHandle = 1;
        internal const int DeviceNotifyWindowHandle = 0;
        internal const int WmDevicechange = 0x219;
        internal const short DigcfPresent = 0x2;
        internal const short DigcfDeviceinterface = 0x10;
        internal const int DigcfAllclasses = 0x4;

        internal const int MaxDevLen = 1000;
        internal const int SpdrpAddress = 0x1c;
        internal const int SpdrpBusnumber = 0x15;
        internal const int SpdrpBustypeguid = 0x13;
        internal const int SpdrpCapabilities = 0xf;
        internal const int SpdrpCharacteristics = 0x1b;
        internal const int SpdrpClass = 7;
        internal const int SpdrpClassguid = 8;
        internal const int SpdrpCompatibleids = 2;
        internal const int SpdrpConfigflags = 0xa;
        internal const int SpdrpDevicePowerData = 0x1e;
        internal const int SpdrpDevicedesc = 0;
        internal const int SpdrpDevtype = 0x19;
        internal const int SpdrpDriver = 9;
        internal const int SpdrpEnumeratorName = 0x16;
        internal const int SpdrpExclusive = 0x1a;
        internal const int SpdrpFriendlyname = 0xc;
        internal const int SpdrpHardwareid = 1;
        internal const int SpdrpLegacybustype = 0x14;
        internal const int SpdrpLocationInformation = 0xd;
        internal const int SpdrpLowerfilters = 0x12;
        internal const int SpdrpMfg = 0xb;
        internal const int SpdrpPhysicalDeviceObjectName = 0xe;
        internal const int SpdrpRemovalPolicy = 0x1f;
        internal const int SpdrpRemovalPolicyHwDefault = 0x20;
        internal const int SpdrpRemovalPolicyOverride = 0x21;
        internal const int SpdrpSecurity = 0x17;
        internal const int SpdrpSecuritySds = 0x18;
        internal const int SpdrpService = 4;
        internal const int SpdrpUINumber = 0x10;
        internal const int SpdrpUINumberDescFormat = 0x1d;

        internal const int SpdrpUpperfilters = 0x11;

        [StructLayout(LayoutKind.Sequential)]
        internal class DevBroadcastDeviceinterface
        {
            internal int dbcc_size;
            internal int dbcc_devicetype;
            internal int dbcc_reserved;
            internal Guid dbcc_classguid;
            internal short dbcc_name;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        internal class DevBroadcastDeviceinterface1
        {
            internal int dbcc_size;
            internal int dbcc_devicetype;
            internal int dbcc_reserved;
            [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 16)]
            internal byte[] dbcc_classguid;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 255)]
            internal char[] dbcc_name;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal class DevBroadcastHandle
        {
            internal int dbch_size;
            internal int dbch_devicetype;
            internal int dbch_reserved;
            internal int dbch_handle;
            internal int dbch_hdevnotify;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal class DevBroadcastHdr
        {
            internal int dbch_size;
            internal int dbch_devicetype;
            internal int dbch_reserved;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct SpDeviceInterfaceData
        {
            internal int cbSize;
            internal System.Guid InterfaceClassGuid;
            internal int Flags;
            internal IntPtr Reserved;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct SpDevinfoData
        {
            internal int cbSize;
            internal Guid ClassGuid;
            internal int DevInst;
            internal IntPtr Reserved;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        internal struct SpDeviceInterfaceDetailData
        {
            internal int Size;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
            internal string DevicePath;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct Devpropkey
        {
            public Guid fmtid;
            public ulong pid;
        }

        //To get the unique device instance ID
        [DllImport("setupapi.dll", CharSet = CharSet.Unicode)]
        public static extern int CM_Get_Device_ID(
           IntPtr dnDevInst,//UInt32 dnDevInst,
           IntPtr buffer,
           int bufferLen,
           int flags
        );

        internal static Devpropkey DevpkeyDeviceBusReportedDeviceDesc =
            new Devpropkey { fmtid = new Guid(0x540b947e, 0x8b40, 0x45bc, 0xa8, 0xa2, 0x6a, 0x0b, 0x89, 0x4c, 0xbd, 0xa2), pid = 4 };

        [DllImport("setupapi.dll", EntryPoint = "SetupDiGetDeviceRegistryProperty")]
        public static extern bool SetupDiGetDeviceRegistryProperty(IntPtr deviceInfoSet, ref SpDevinfoData deviceInfoData, int propertyVal, ref int propertyRegDataType, byte[] propertyBuffer, int propertyBufferSize, ref int requiredSize);

        [DllImport("setupapi.dll", EntryPoint = "SetupDiGetDevicePropertyW", SetLastError = true)]
        public static extern bool SetupDiGetDeviceProperty(IntPtr deviceInfo, ref SpDevinfoData deviceInfoData, ref Devpropkey propkey, ref ulong propertyDataType, byte[] propertyBuffer, int propertyBufferSize, ref int requiredSize, uint flags);

        [DllImport("setupapi.dll")]
        static internal extern bool SetupDiEnumDeviceInfo(IntPtr deviceInfoSet, int memberIndex, ref SpDevinfoData deviceInfoData);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        static internal extern IntPtr RegisterDeviceNotification(IntPtr hRecipient, IntPtr notificationFilter, Int32 flags);

        [DllImport("setupapi.dll")]
        internal static extern int SetupDiCreateDeviceInfoList(ref Guid classGuid, int hwndParent);

        [DllImport("setupapi.dll")]
        static internal extern int SetupDiDestroyDeviceInfoList(IntPtr deviceInfoSet);

        [DllImport("setupapi.dll")]
        static internal extern bool SetupDiEnumDeviceInterfaces(IntPtr deviceInfoSet, ref SpDevinfoData deviceInfoData, ref Guid interfaceClassGuid, int memberIndex, ref SpDeviceInterfaceData deviceInterfaceData);

        [DllImport("setupapi.dll", CharSet = CharSet.Unicode)]
        static internal extern IntPtr SetupDiGetClassDevs(ref System.Guid classGuid, string enumerator, int hwndParent, int flags);

        [DllImport("setupapi.dll", CharSet = CharSet.Unicode, EntryPoint = "SetupDiGetDeviceInterfaceDetail")]
        static internal extern bool SetupDiGetDeviceInterfaceDetailBuffer(IntPtr deviceInfoSet, ref SpDeviceInterfaceData deviceInterfaceData, IntPtr deviceInterfaceDetailData, int deviceInterfaceDetailDataSize, ref int requiredSize, IntPtr deviceInfoData);

        //WAS [DllImport("setupapi.dll", CharSet = CharSet.Auto)]
        [DllImport("setupapi.dll", CharSet = CharSet.Unicode)]
        static internal extern bool SetupDiGetDeviceInterfaceDetail(IntPtr deviceInfoSet, ref SpDeviceInterfaceData deviceInterfaceData, ref SpDeviceInterfaceDetailData deviceInterfaceDetailData, int deviceInterfaceDetailDataSize, ref int requiredSize, IntPtr deviceInfoData);

        [DllImport("user32.dll")]
        static internal extern bool UnregisterDeviceNotification(IntPtr handle);

        internal const short HidpInput = 0;
        internal const short HidpOutput = 1;

        internal const short HidpFeature = 2;
        [StructLayout(LayoutKind.Sequential)]
        internal struct HiddAttributes
        {
            internal int Size;
            internal ushort VendorID;
            internal ushort ProductID;
            internal short VersionNumber;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct HidpCaps
        {
            internal short Usage;
            internal short UsagePage;
            internal short InputReportByteLength;
            internal short OutputReportByteLength;
            internal short FeatureReportByteLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 17)]
            internal short[] Reserved;
            internal short NumberLinkCollectionNodes;
            internal short NumberInputButtonCaps;
            internal short NumberInputValueCaps;
            internal short NumberInputDataIndices;
            internal short NumberOutputButtonCaps;
            internal short NumberOutputValueCaps;
            internal short NumberOutputDataIndices;
            internal short NumberFeatureButtonCaps;
            internal short NumberFeatureValueCaps;
            internal short NumberFeatureDataIndices;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct HidpValueCaps
        {
            internal short UsagePage;
            internal byte ReportID;
            internal int IsAlias;
            internal short BitField;
            internal short LinkCollection;
            internal short LinkUsage;
            internal short LinkUsagePage;
            internal int IsRange;
            internal int IsStringRange;
            internal int IsDesignatorRange;
            internal int IsAbsolute;
            internal int HasNull;
            internal byte Reserved;
            internal short BitSize;
            internal short ReportCount;
            internal short Reserved2;
            internal short Reserved3;
            internal short Reserved4;
            internal short Reserved5;
            internal short Reserved6;
            internal int LogicalMin;
            internal int LogicalMax;
            internal int PhysicalMin;
            internal int PhysicalMax;
            internal short UsageMin;
            internal short UsageMax;
            internal short StringMin;
            internal short StringMax;
            internal short DesignatorMin;
            internal short DesignatorMax;
            internal short DataIndexMin;
            internal short DataIndexMax;
        }

        [DllImport("hid.dll")]
        static internal extern bool HidD_FlushQueue(IntPtr hidDeviceObject);

        [DllImport("hid.dll")]
        static internal extern bool HidD_GetAttributes(IntPtr hidDeviceObject, ref HiddAttributes attributes);

        [DllImport("hid.dll")]
        static internal extern bool HidD_GetFeature(IntPtr hidDeviceObject, byte[] lpReportBuffer, int reportBufferLength);

        [DllImport("hid.dll")]
        static internal extern bool HidD_GetInputReport(IntPtr hidDeviceObject, ref byte lpReportBuffer, int reportBufferLength);

        [DllImport("hid.dll")]
        static internal extern void HidD_GetHidGuid(ref Guid hidGuid);

        [DllImport("hid.dll")]
        static internal extern bool HidD_GetNumInputBuffers(IntPtr hidDeviceObject, ref int numberBuffers);

        [DllImport("hid.dll")]
        static internal extern bool HidD_GetPreparsedData(IntPtr hidDeviceObject, ref IntPtr preparsedData);

        [DllImport("hid.dll")]
        static internal extern bool HidD_FreePreparsedData(IntPtr preparsedData);

        [DllImport("hid.dll")]
        static internal extern bool HidD_SetFeature(IntPtr hidDeviceObject, byte[] lpReportBuffer, int reportBufferLength);

        [DllImport("hid.dll")]
        static internal extern bool HidD_SetNumInputBuffers(IntPtr hidDeviceObject, int numberBuffers);

        [DllImport("hid.dll")]
        static internal extern bool HidD_SetOutputReport(IntPtr hidDeviceObject, byte[] lpReportBuffer, int reportBufferLength);

        [DllImport("hid.dll")]
        static internal extern int HidP_GetCaps(IntPtr preparsedData, ref HidpCaps capabilities);

        [DllImport("hid.dll")]
        static internal extern int HidP_GetValueCaps(short reportType, ref byte valueCaps, ref short valueCapsLength, IntPtr preparsedData);

        [DllImport("hid.dll", CharSet = CharSet.Unicode)]
        internal static extern bool HidD_GetProductString(IntPtr hidDeviceObject, ref byte lpReportBuffer, int reportBufferLength);

        [DllImport("hid.dll", CharSet = CharSet.Unicode)]
        internal static extern bool HidD_GetManufacturerString(IntPtr hidDeviceObject, ref byte lpReportBuffer, int reportBufferLength);

        [DllImport("hid.dll", CharSet = CharSet.Unicode)]
        internal static extern bool HidD_GetSerialNumberString(IntPtr hidDeviceObject, ref byte lpReportBuffer, int reportBufferLength);
    }
}
