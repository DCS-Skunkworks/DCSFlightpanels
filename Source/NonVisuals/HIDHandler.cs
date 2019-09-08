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
        private readonly List<HIDSkeleton> _hidSkeletons = new List<HIDSkeleton>();

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
                                _hidSkeletons.Add(hidSkeleton);
                            }
                        }
                    }
                }

                foreach (var hidSkeleton in _hidSkeletons)
                {
                    if (hidSkeleton.PanelInfo.VendorId == (int) GamingPanelVendorEnum.Saitek)
                    {

                        //Creating read devices
                        foreach (var hidDevice in HidDevices.Enumerate(hidSkeleton.InstanceId))
                        {
                            if (hidDevice != null)
                            {
                                hidSkeleton.HIDReadDevice = hidDevice;
                                hidSkeleton.HIDReadDevice.OpenDevice(DeviceMode.NonOverlapped, DeviceMode.NonOverlapped, ShareMode.ShareRead | ShareMode.ShareWrite);
                                //hidSkeleton.HIDReadDevice.Inserted += DeviceAttachedHandler;
                                //hidSkeleton.HIDReadDevice.Removed += DeviceRemovedHandler;
                                hidSkeleton.HIDReadDevice.MonitorDeviceEvents = true;
                                Common.DebugP(hidSkeleton.PanelInfo.GamingPanelType + " [Saitek]HIDReadDevice has entered the building...");
                            }
                        }

                        //Creating write devices
                        foreach (var hidDevice in HidDevices.Enumerate(hidSkeleton.InstanceId))
                        {
                            if (hidDevice != null)
                            {
                                hidSkeleton.HIDWriteDevice = hidDevice;
                                hidSkeleton.HIDWriteDevice.OpenDevice(DeviceMode.NonOverlapped, DeviceMode.NonOverlapped, ShareMode.ShareRead | ShareMode.ShareWrite);
                                //hidSkeleton.HIDWriteDevice.Inserted += DeviceAttachedHandler;
                                //hidSkeleton.HIDWriteDevice.Removed += DeviceRemovedHandler;
                                hidSkeleton.HIDWriteDevice.MonitorDeviceEvents = true;
                                Common.DebugP(hidSkeleton.PanelInfo.GamingPanelType + " [Saitek]HIDWriteDevice has entered the building...");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(1073, ex);
            }
        }

        public void Shutdown()
        {
            try
            {
                foreach (var hidSkeleton in _hidSkeletons)
                {
                    try
                    {
                        hidSkeleton.Close();
                    }
                    catch (Exception ex)
                    {
                        Common.ShowErrorMessageBox(107123, ex);
                    }
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(107333, ex);
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
            foreach (var hidskeleton in _hidSkeletons)
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

        public List<HIDSkeleton> HIDSkeletons => _hidSkeletons;
    }

}
