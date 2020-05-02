using System;
using ClassLibraryCommon;
using HidLibrary;

namespace NonVisuals
{

    public class HIDSkeleton
    {

        private string _instanceId;
        private HidDevice _hidReadDevice;
        private HidDevice _hidWriteDevice;
        private readonly GamingPanelSkeleton _gamingPanelSkeleton;

        public HIDSkeleton(GamingPanelSkeleton gamingPanelSkeleton, string instanceId)
        {
            _gamingPanelSkeleton = gamingPanelSkeleton;
            _instanceId = instanceId;
        }

        public void Close()
        {
            try
            {
                if (_hidReadDevice.IsOpen)
                {
                    _hidReadDevice.CloseDevice();
                }
                if (_hidWriteDevice.IsOpen)
                {
                    _hidWriteDevice.CloseDevice();
                }
                _hidReadDevice.Dispose();
                _hidWriteDevice.Dispose();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox( ex);
            }
        }

        public GamingPanelSkeleton PanelInfo
        {
            get => _gamingPanelSkeleton;
        }

        public string InstanceId
        {
            get => _instanceId;
            set => _instanceId = value;
        }

        public HidDevice HIDReadDevice
        {
            get => _hidReadDevice;
            set => _hidReadDevice = value;
        }

        public HidDevice HIDWriteDevice
        {
            get => _hidWriteDevice;
            set => _hidWriteDevice = value;
        }
    }

}
