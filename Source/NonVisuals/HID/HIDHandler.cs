using System.Linq;
using NonVisuals.EventArgs;

namespace NonVisuals.HID
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    using ClassLibraryCommon;

    using HidLibrary;

    public static class HIDSkeletonIgnore
    {
        public static readonly string HidSkeletonIgnore = "IGNORED_DEVICE_INSTANCE";
    }

    public class HIDHandler
    {
        public List<HIDSkeleton> HIDSkeletons { get; } = new List<HIDSkeleton>();
        private static HIDHandler _instance;

        public static HIDHandler GetInstance()
        {
            return _instance ??= new HIDHandler();
        }

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
                return string.Empty;
            }

            var stringBuilder = new StringBuilder(100);
            stringBuilder.AppendLine($"HIDHandler has the following skeletons ({_instance.HIDSkeletons.Count}) :");
            foreach (var skeleton in _instance.HIDSkeletons)
            {
                stringBuilder.Append('\t').Append(skeleton.PanelInfo).Append('\n');
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

                            var hidInstance = hidDevice.DevicePath;
                            if (!HIDDeviceAlreadyExists(hidInstance))
                            {
                                var hidSkeleton = new HIDSkeleton(gamingPanelSkeleton, hidInstance);
                                HIDSkeletons.Add(hidSkeleton);

                                hidDevice.MonitorDeviceEvents = true;
                                hidDevice.Inserted += hidSkeleton.HIDDeviceOnInserted;
                                hidDevice.Removed += hidSkeleton.HIDDeviceOnRemoved;

                                //Only Saitek needs this hid library, Stream Deck uses an other. But Stream Deck is added in order to have references.
                                if (hidSkeleton.PanelInfo.VendorId == (int)GamingPanelVendorEnum.Saitek
                                    || hidSkeleton.PanelInfo.VendorId == (int)GamingPanelVendorEnum.MadCatz
                                    || hidSkeleton.PanelInfo.VendorId == (int)GamingPanelVendorEnum.CockpitMaster && hidSkeleton.PanelInfo.GamingPanelType == GamingPanelEnum.CDU737
                                    )
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
                
                //Broadcast that this panel was found.
                HIDSkeletons.FindAll(o => o.IsAttached).ToList().ForEach(o => AppEventHandler.PanelEvent(this, o.HIDInstance, o, PanelEventType.Found));

                //Broadcast that panel search is over and all panels have been found that exists.
                AppEventHandler.PanelEvent(this, null, null, PanelEventType.AllPanelsFound);
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


        private bool HIDDeviceAlreadyExists(string hidInstance)
        {
            if (string.IsNullOrEmpty(hidInstance))
            {
                throw new Exception("Looking for empty/null HIDInstance HIDDeviceAlreadyExists().");
            }

            foreach (var hidSkeleton in HIDSkeletons)
            {
                if (hidSkeleton.HIDInstance.Equals(hidInstance))
                {
                    return true;
                }
            }

            return false;
        }
    }

}
