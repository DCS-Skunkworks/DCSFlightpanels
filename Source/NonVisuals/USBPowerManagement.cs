using ClassLibraryCommon;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NonVisuals
{
    public static class USBPowerManagement
    {

        const string SAITEK_VID = "VID_06A3";


        public static string FixSaitekUSBEnhancedPowerManagerIssues()
        {
            return FixUSBEnhancedPowerManagerIssues(SAITEK_VID);
        }

        private static string FixUSBEnhancedPowerManagerIssues(string vendorId)
        {
            /*
             * This is a slightly modified code version of the original code which was made by http://uraster.com
             */
            var result = new StringBuilder();
            result.AppendLine("USB Enhanced Power Management Disabler");
            result.AppendLine("http://uraster.com/en-us/products/usbenhancedpowermanagerdisabler.aspx");
            result.AppendLine("Copywrite Uraster GmbH");
            result.AppendLine(new string('=', 60));
            result.AppendLine("This application disables the enhanced power management for the all USB devices of a specific vendor.");
            result.AppendLine("You need admin rights to do that.");
            result.AppendLine("Plug in all devices in the ports you intend to use before continuing.");
            result.AppendLine(new string('-', 60));
            result.AppendLine("Vendor ID (VID). For SAITEK use the default of " + SAITEK_VID);

            try
            {
                var devicesDisabled = 0;
                var devicesAlreadyDisabled = 0;
                using var usbDevicesKey = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Enum\USB");
                if (usbDevicesKey != null)
                {
                    foreach (var usbDeviceKeyName in usbDevicesKey.GetSubKeyNames().Where(name => name.StartsWith(SAITEK_VID)))
                    {
                        result.Append(Environment.NewLine);
                        result.AppendLine("Processing product : " + GetProductId(SAITEK_VID, usbDeviceKeyName));
                        using var usbDeviceKey = usbDevicesKey.OpenSubKey(usbDeviceKeyName);
                        if (usbDeviceKey != null)
                        {
                            foreach (var instanceKeyName in usbDeviceKey.GetSubKeyNames())
                            {
                                result.AppendLine("Device instance : " + instanceKeyName);
                                using var instanceKey = usbDeviceKey.OpenSubKey(instanceKeyName);
                                if (instanceKey != null)
                                {
                                    using var deviceParametersKey = instanceKey.OpenSubKey("Device Parameters", true);
                                    if (deviceParametersKey == null)
                                    {
                                        result.AppendLine("no parameters, skipping");
                                        continue;
                                    }

                                    var value = deviceParametersKey.GetValue("EnhancedPowerManagementEnabled");
                                    if (0.Equals(value))
                                    {
                                        result.AppendLine("enhanced power management is already disabled");
                                        devicesAlreadyDisabled++;
                                    }
                                    else
                                    {
                                        result.Append("enhanced power management is enabled, disabling... ");
                                        deviceParametersKey.SetValue("EnhancedPowerManagementEnabled", 0);
                                        result.AppendLine("now disabled");
                                        devicesDisabled++;
                                    }
                                }
                            }
                        }
                    }
                }

                result.AppendLine(new string('-', 60));
                result.AppendLine("Done. Unplug all devices and plug them again in the same ports.");
                result.AppendLine("Device instances fixed " + devicesDisabled);
                result.AppendLine("Device instances already fixed " + devicesAlreadyDisabled);
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex, "Error disabling Enhanced USB Power Management.");
                return null;
            }

            return result.ToString();
        }

        private static string GetProductId(string vendorVID, string usbDeviceKeyName)
        {
            var pos = usbDeviceKeyName.IndexOf("&", StringComparison.InvariantCulture);
            var vid = usbDeviceKeyName.Substring(0, pos);
            var pid = usbDeviceKeyName.Substring(pos + 1);
            var result = pid;
            if (vid == vendorVID)
            {
                if (pid.StartsWith("PID_0D06"))
                {
                    result += " (Multi panel, PZ70)";
                }
                else if (pid.StartsWith("PID_0D05"))
                {
                    result += " (Radio panel, PZ69)";
                }
                else if (pid.StartsWith("PID_0D67"))
                {
                    result += " (Switch panel, PZ55)";
                }
                else if (pid.StartsWith("PID_A2AE"))
                {
                    result += " (Instrument panel)";
                }
                else if (pid.StartsWith("PID_712C"))
                {
                    result += " (Yoke)";
                }
                else if (pid.StartsWith("PID_0C2D"))
                {
                    result += " (Throttle quadrant)";
                }
                else if (pid.StartsWith("PID_0763"))
                {
                    result += " (Pedals)";
                }
                else if (pid.StartsWith("PID_0B4E"))
                {
                    result += " (BIP)";
                }
            }

            return result;
        }
    }
}
