using DCS_BIOS.EventArgs;
using DCS_BIOS;
using NonVisuals.CockpitMaster.Panels;
using System;
using DCS_BIOS.Interfaces;
using NonVisuals.CockpitMaster.Switches;
using System.Collections.Generic;
using NonVisuals.HID;

namespace NonVisuals.CockpitMaster.PreProgrammed
{
    public class CDU737PanelM2000C : CDU737PanelBase, IDCSBIOSStringListener
    {
        // List the DCSBios Mappings Here
        private bool _disposed;

        public CDU737PanelM2000C(HIDSkeleton hidSkeleton) : base(hidSkeleton)
        {}

        public override void InitPanel()
        {
            try
            {
                base.InitPanel();

                CDUPanelKeys = CDUMappedCommandKeyM2000C.GetMappedPanelKeys();
                
                BIOSEventHandler.AttachStringListener(this);
                BIOSEventHandler.AttachDataListener(this);

                SetColorForLine(0, CDUColor.WHITE);
                SetLine(0, string.Format("{0,24}", "M2000C profile"));
                SetColorForLine(5, CDUColor.BLUE);
                SetColorForLine(7, CDUColor.WHITE);
                SetColorForLine(9, CDUColor.RED);
                SetLine(9,"  PCN Keyboard Mapping  ");

                StartListeningForHidPanelChanges();
                
            }
            catch (Exception ex)
            {
                SetLastException(ex);
            }
        }

        // Protected implementation of Dispose pattern.
        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    BIOSEventHandler.DetachStringListener(this);
                    BIOSEventHandler.DetachDataListener(this);
                }
                _disposed = true;
            }

            // Call base class implementation.
            base.Dispose(disposing);
        }
        protected override void GamingPanelKnobChanged(bool isFirstReport, IEnumerable<object> hashSet)
        {
            if (isFirstReport)
            {
                return;
            }

            try
            {
                foreach (CDUMappedCommandKey key in hashSet)
                {
                    _ = DCSBIOS.SendAsync(key.MappedCommand());
                }

            }
            catch (Exception)
            {
            }
        }

        public override void DcsBiosDataReceived(object sender, DCSBIOSDataEventArgs e)
        {

            if (SettingsLoading)
            {
                return;
            }

            try
            {
                UpdateCounter(e.Address, e.Data);

            }
            catch (Exception)
            {

            }
        }
        public void DCSBIOSStringReceived(object sender, DCSBIOSStringDataEventArgs e)
        {
        }

    }
}
