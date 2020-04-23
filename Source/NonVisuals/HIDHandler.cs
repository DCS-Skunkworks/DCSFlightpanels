using System;
using System.Collections.Generic;
using ClassLibraryCommon;
using HidLibrary;

namespace NonVisuals
{

    public static class HIDSkeletonIgnore
    {
        public static readonly string HidSkeletonIgnore = "IGNORED_DEVICE_INSTANCE";
    }

    public class HIDHandler
    {
        public List<HIDSkeleton> HIDSkeletons { get; } = new List<HIDSkeleton>();

        public void Startup()
        {
            try
            {
                Common.DebugP("Entering HIDHandler.Startup()");
                foreach (var gamingPanelSkeleton in Common.GamingPanelSkeletons)
                {
                    foreach (var hidDevice in HidDevices.Enumerate(gamingPanelSkeleton.VendorId, gamingPanelSkeleton.ProductId))
                    {
                        if (hidDevice != null)
                        {
                            var instanceId = hidDevice.DevicePath;
                            if (!HIDDeviceAlreadyExists(instanceId))
                            {
                                var hidSkeleton = new HIDSkeleton(gamingPanelSkeleton, instanceId);
                                HIDSkeletons.Add(hidSkeleton);
                                if (hidSkeleton.PanelInfo.VendorId == (int)GamingPanelVendorEnum.Saitek)
                                {
                                    hidSkeleton.HIDReadDevice = hidDevice;
                                    hidSkeleton.HIDReadDevice.OpenDevice(DeviceMode.NonOverlapped, DeviceMode.NonOverlapped, ShareMode.ShareRead | ShareMode.ShareWrite);
                                    //hidSkeleton.HIDReadDevice.Inserted += DeviceAttachedHandler;
                                    //hidSkeleton.HIDReadDevice.Removed += DeviceRemovedHandler;
                                    hidSkeleton.HIDReadDevice.MonitorDeviceEvents = true;

                                    hidSkeleton.HIDWriteDevice = hidDevice;
                                    hidSkeleton.HIDWriteDevice.OpenDevice(DeviceMode.NonOverlapped, DeviceMode.NonOverlapped, ShareMode.ShareRead | ShareMode.ShareWrite);
                                    //hidSkeleton.HIDWriteDevice.Inserted += DeviceAttachedHandler;
                                    //hidSkeleton.HIDWriteDevice.Removed += DeviceRemovedHandler;
                                    hidSkeleton.HIDWriteDevice.MonitorDeviceEvents = true;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        public void Shutdown()
        {
            try
            {
                foreach (var hidSkeleton in HIDSkeletons)
                {
                    try
                    {
                        hidSkeleton.Close();
                    }
                    catch (Exception ex)
                    {
                        Common.ShowErrorMessageBox(ex);
                    }
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }


        private bool HIDDeviceAlreadyExists(string instanceId)
        {
            if (string.IsNullOrEmpty(instanceId))
            {
                throw new Exception("Looking for empty/null InstanceId HIDDeviceAlreadyExists().");
            }
            Common.DebugP("---------------");
            Common.DebugP("Looking for : " + instanceId + "   " + instanceId);
            foreach (var hidskeleton in HIDSkeletons)
            {
                if (hidskeleton.InstanceId.Equals(instanceId))
                {
                    Common.DebugP("HIDSkeleton already found : " + hidskeleton.InstanceId);
                    Common.DebugP("---------------");
                    return true;
                }
            }
            Common.DebugP("HIDSkeleton not found : " + instanceId);
            Common.DebugP("---------------");
            return false;
        }
    }

}
