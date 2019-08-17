using System;
using System.Collections.Generic;
using ClassLibraryCommon;
using HidLibrary;

namespace NonVisuals
{
    public class HIDSkeleton
    {
        private GamingPanelEnum _panelType;
        private string _instanceId;
        private HidDevice _hidReadDevice;
        private HidDevice _hidWriteDevice;

        public HIDSkeleton(GamingPanelEnum panelType, string instanceId)
        {
            _panelType = panelType;
            _instanceId = instanceId;
        }

        public void Close()
        {
            try
            {
                if (_hidReadDevice.IsOpen)
                {
                    _hidReadDevice.CloseDevice();
                    Common.DebugP(_panelType + " : HidReadDevice has left the building...");
                }
                if (_hidWriteDevice.IsOpen)
                {
                    _hidWriteDevice.CloseDevice();
                    Common.DebugP(_panelType + " : HidWriteDevice has left the building...");
                }
                _hidReadDevice.Dispose();
                _hidWriteDevice.Dispose();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(15451073, ex);
            }
        }

        public GamingPanelEnum PanelType
        {
            get => _panelType;
            set => _panelType = value;
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

    public class HIDHandler
    {
        private readonly List<HIDSkeleton> _hidSkeletons = new List<HIDSkeleton>();

        public void Startup()
        {
            try
            {
                Common.DebugP("Entering HIDHandler.Startup()");
                foreach (var saitekPanelSkeleton in Common.SaitekPanelSkeletons)
                {
                    foreach (var hidDevice in HidDevices.Enumerate(saitekPanelSkeleton.VendorId, saitekPanelSkeleton.ProductId))
                    {
                        if (hidDevice != null)
                        {
                            var instanceId = hidDevice.DevicePath;
                            if (!HIDDeviceAlreadyExists(instanceId))
                            {
                                var hidSkeleton = new HIDSkeleton(saitekPanelSkeleton.SaitekPanelsType, instanceId);
                                _hidSkeletons.Add(hidSkeleton);
                            }
                        }

                    }
                }

                foreach (var hidSkeleton in _hidSkeletons)
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
                            Common.DebugP(hidSkeleton.PanelType + " HIDReadDevice has entered the building...");
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
                            Common.DebugP(hidSkeleton.PanelType + " HIDWriteDevice has entered the building...");
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
