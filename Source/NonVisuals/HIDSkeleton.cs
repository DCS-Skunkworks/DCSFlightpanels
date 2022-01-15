using NonVisuals.EventArgs;

namespace NonVisuals
{
    using System;

    using ClassLibraryCommon;

    using HidLibrary;

    public class HIDSkeleton
    {
        private readonly GamingPanelSkeleton _gamingPanelSkeleton;

        public HIDSkeleton(GamingPanelSkeleton gamingPanelSkeleton, string hidInstance)
        {
            _gamingPanelSkeleton = gamingPanelSkeleton;
            HIDInstance = hidInstance;
        }

        public void Close()
        {
            try
            {
                if (HIDReadDevice.IsOpen)
                {
                    HIDReadDevice.CloseDevice();
                }

                if (HIDWriteDevice.IsOpen)
                {
                    HIDWriteDevice.CloseDevice();
                }

                HIDReadDevice.Dispose();
                HIDWriteDevice.Dispose();
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

        public string HIDInstance { get; set; }

        public HidDevice HIDReadDevice { get; set; }

        public HidDevice HIDWriteDevice { get; set; }

        public GamingPanelSkeleton GamingPanelSkeleton => _gamingPanelSkeleton;

        public bool PanelHasBeenInstantiated { get; set; }

        
        private bool _once = true;
        public void HIDDeviceOnInserted()
        {
            if (_once)
            {
                _once = false;
                return;
            }

            AppEventHandler.PanelEvent(this, HIDInstance, this, PanelEventType.Attached);
        }

        public void HIDDeviceOnRemoved()
        {
            AppEventHandler.PanelEvent(this, HIDInstance, this, PanelEventType.Detached);
        }
    }

}
