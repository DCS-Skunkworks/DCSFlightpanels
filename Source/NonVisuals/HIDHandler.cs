using System;
using System.Collections.Generic;
using System.Text;
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
        private static HIDHandler _instance = null;

        public HIDHandler()
        {
            if (_instance != null)
            {
                throw new Exception("HIDHandler already exists.");
            }
            _instance = this;
        }

        public static string GetInformation()
        {
            if (_instance == null)
            {
                return "";
            }
            var stringBuilder = new StringBuilder(100);
            stringBuilder.Append("HIDHandler has the following skeletons (" + _instance.HIDSkeletons.Count + ") :\n");
            foreach (var skeleton in _instance.HIDSkeletons)
            {
                stringBuilder.Append("\t").Append(skeleton.PanelInfo ).Append("\n");
            }

            return stringBuilder.ToString();
        }

        public void Startup(bool loadStreamDeck)
        {
            try
            {
                foreach (var gamingPanelSkeleton in Common.GamingPanelSkeletons)
                {
                    foreach (var hidDevice in HidDevices.Enumerate(gamingPanelSkeleton.VendorId, gamingPanelSkeleton.ProductId))
                    {
                        if (hidDevice != null)
                        {
                            if (!loadStreamDeck && gamingPanelSkeleton.VendorId == (int)GamingPanelVendorEnum.Elgato)
                            {
                                continue;
                            }

                            var instanceId = hidDevice.DevicePath;
                            if (!HIDDeviceAlreadyExists(instanceId))
                            {
                                var hidSkeleton = new HIDSkeleton(gamingPanelSkeleton, instanceId);
                                HIDSkeletons.Add(hidSkeleton);
                                if (hidSkeleton.PanelInfo.VendorId == (int)GamingPanelVendorEnum.Saitek)
                                {
                                    hidSkeleton.HIDReadDevice = hidDevice;
                                    hidSkeleton.HIDReadDevice.OpenDevice(DeviceMode.NonOverlapped, DeviceMode.NonOverlapped, ShareMode.ShareRead | ShareMode.ShareWrite);
                                    hidSkeleton.HIDReadDevice.MonitorDeviceEvents = true;

                                    hidSkeleton.HIDWriteDevice = hidDevice;
                                    hidSkeleton.HIDWriteDevice.OpenDevice(DeviceMode.NonOverlapped, DeviceMode.NonOverlapped, ShareMode.ShareRead | ShareMode.ShareWrite);
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
            foreach (var hidSkeleton in HIDSkeletons)
            {
                if (hidSkeleton.InstanceId.Equals(instanceId))
                {
                    return true;
                }
            }
            return false;
        }
    }

}
