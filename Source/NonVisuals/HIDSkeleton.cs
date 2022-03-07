using NonVisuals.EventArgs;

namespace NonVisuals
{
    using System;

    using ClassLibraryCommon;

    using HidLibrary;

    public class HIDSkeleton
    {
        public AppEventHandler AppEventHandler { get; private set; }
        private readonly GamingPanelSkeleton _gamingPanelSkeleton;
        public bool IsAttached { get; private set; }

        public HIDSkeleton(GamingPanelSkeleton gamingPanelSkeleton, string hidInstance, AppEventHandler appEventHandler)
        {
            AppEventHandler = appEventHandler;
            _gamingPanelSkeleton = gamingPanelSkeleton;
            HIDInstance = hidInstance;
            IsAttached = true;
        }

        public void Close()
        {
            try
            {
                IsAttached = false;

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

        public GamingPanelEnum GamingPanelType => _gamingPanelSkeleton.GamingPanelType;

        public bool PanelHasBeenInstantiated { get; set; }

        
        private bool _once = true;
        public void HIDDeviceOnInserted()
        {
            if (_once)
            {
                _once = false;
                return;
            }

            IsAttached = true;
            AppEventHandler.PanelEvent(this, HIDInstance, this, PanelEventType.Attached);
        }

        public void HIDDeviceOnRemoved()
        {
            IsAttached = false;
            AppEventHandler.PanelEvent(this, HIDInstance, this, PanelEventType.Detached);
        }
    }

}
