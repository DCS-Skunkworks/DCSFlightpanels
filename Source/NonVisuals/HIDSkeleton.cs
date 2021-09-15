namespace NonVisuals
{
    using System;

    using ClassLibraryCommon;

    using HidLibrary;

    public class HIDSkeleton
    {
        private readonly GamingPanelSkeleton _gamingPanelSkeleton;

        public HIDSkeleton(GamingPanelSkeleton gamingPanelSkeleton, string instanceId)
        {
            _gamingPanelSkeleton = gamingPanelSkeleton;
            this.InstanceId = instanceId;
        }

        public void Close()
        {
            try
            {
                if (this.HIDReadDevice.IsOpen)
                {
                    this.HIDReadDevice.CloseDevice();
                }

                if (this.HIDWriteDevice.IsOpen)
                {
                    this.HIDWriteDevice.CloseDevice();
                }

                this.HIDReadDevice.Dispose();
                this.HIDWriteDevice.Dispose();
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

        public string InstanceId { get; set; }

        public HidDevice HIDReadDevice { get; set; }

        public HidDevice HIDWriteDevice { get; set; }

        public GamingPanelSkeleton GamingPanelSkeleton => _gamingPanelSkeleton;

        public bool PanelHasBeenInstantiated { get; set; }
    }

}
