using DCS_BIOS.EventArgs;
using DCS_BIOS;
using NonVisuals.CockpitMaster.Panels;
using System;
using DCS_BIOS.Interfaces;
using NonVisuals.CockpitMaster.Switches;
using System.Collections.Generic;
using System.Xml;
using Theraot;

namespace NonVisuals.CockpitMaster.Preprogrammed
{
    
    public class CDU737PanelSA342 : CDU737PanelBase , IDCSBIOSStringListener
    {
        public CDU737PanelSA342(HIDSkeleton hidSkeleton) : base(hidSkeleton)
        {
            CDUPanelKeys = CDUMappedCommandKeySA342.GetMappedPanelKeys();
            
            BIOSEventHandler.AttachStringListener(this);
            BIOSEventHandler.AttachDataListener(this);
            for(int i=0;i<LINES_ON_CDU;i++)
            {
                SetColorForLine(i, CDUColor.RED);
            }
            Startup();
        }

        public sealed override void Startup()
        {
            try
            {

                // PLT Keyboard display


                SetLine(0, string.Format("{0,24}", "SA342 profile"));

                StartListeningForHidPanelChanges();
                
            }
            catch (Exception ex)
            {
                SetLastException(ex);
            }
        }

        private bool _disposed;
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
                    _ = DCSBIOS.Send(key.MappedCommand());
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
