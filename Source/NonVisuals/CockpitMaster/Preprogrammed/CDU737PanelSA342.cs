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
            try
            {
                int linesChanged = 0;
                string incomingData = "";

                //if (e.Address.Equals(_PLT_EUFD_LINE14.Address))
                //{
                //    incomingData = e.StringData.Substring(46, 10);
                //    if (HandleStringData(0, e, incomingData, ref linesChanged))
                //    {
                //        SetLine(0, string.Format("{0,24}", incomingData));
                //    }
                    
                //}

                //if (e.Address.Equals(_PLT_EUFD_LINE1.Address))
                //{
                //    incomingData = e.StringData.Substring(38, 17);
                //    if (HandleStringData(1, e, incomingData, ref linesChanged))
                //    {
                //        SetLine(1, string.Format("{0,24}", incomingData ));
                //    }
                    
                //}

                //if (e.Address.Equals(_PLT_EUFD_LINE2.Address))
                //{
                //    incomingData = e.StringData.Substring(38, 17);
                //    if (HandleStringData(2, e, incomingData, ref linesChanged))
                //    {
                //        SetLine(2, string.Format("{0,24}", incomingData));
                //    }
                //}

                //if (e.Address.Equals(_PLT_EUFD_LINE3.Address))
                //{
                //    incomingData = e.StringData.Substring(38, 17);
                //    if (HandleStringData(3, e, incomingData, ref linesChanged))
                //    {
                //        SetLine(3, string.Format("{0,24}", incomingData));
                //    }
                //}

                //if (e.Address.Equals(_PLT_EUFD_LINE4.Address))
                //{
                //    incomingData = e.StringData.Substring(38, 17);
                //    if (HandleStringData(4, e, incomingData, ref linesChanged))
                //    {
                //        SetLine(4, string.Format("{0,24}", incomingData));
                //    }
                //}

                //if (e.Address.Equals(_PLT_EUFD_LINE5.Address))
                //{
                //    incomingData = e.StringData.Substring(38, 17);
                //    if (HandleStringData(5, e, incomingData, ref linesChanged))
                //    {
                //        SetLine(5, string.Format("{0,24}", incomingData));
                //    }
                //}

                //// UNused
                //SetLine(6, "");

                //// Radios Frequencies

                //if (e.Address.Equals(_PLT_EUFD_LINE8.Address))
                //{
                //    incomingData = e.StringData.Substring(0, 18);
                //    if (HandleStringData(7, e, incomingData, ref linesChanged))
                //    {
                //        SetLine(7, incomingData );
                //    }
                        
                //}
                //if (e.Address.Equals(_PLT_EUFD_LINE9.Address))
                //{
                //    incomingData = e.StringData.Substring(0, 18);
                //    if (HandleStringData(8, e, incomingData, ref linesChanged))
                //    {
                //        SetLine(8, incomingData);
                //    }
                //}
                //if (e.Address.Equals(_PLT_EUFD_LINE10.Address))
                //{
                //    incomingData = e.StringData.Substring(0, 18);
                //    if (HandleStringData(9, e, incomingData, ref linesChanged))
                //    {
                //        SetLine(9, incomingData);
                //    }

                //}
                //if (e.Address.Equals(_PLT_EUFD_LINE11.Address))
                //{
                //    incomingData = e.StringData.Substring(0, 18);
                //    if (HandleStringData(10, e, incomingData, ref linesChanged))
                //    {
                //        SetLine(10, incomingData);
                //    }

                //}
                //if (e.Address.Equals(_PLT_EUFD_LINE12.Address))
                //{
                //    incomingData = e.StringData.Substring(0, 18);
                //    if (HandleStringData(11, e, incomingData, ref linesChanged))
                //    {
                //        SetLine(11, incomingData);
                //    }
                //}

                //SetLine(12, "- Keyboard -------------");

                //if (e.Address.Equals(_PLT_KU_DISPLAY.Address))
                //{
                //    incomingData = e.StringData;
                //    if (HandleStringData(13, e, incomingData, ref linesChanged))
                //    {
                //        SetLine(13, incomingData);
                //    }

                //}

            }

            catch (Exception)
            {

            }
        }
    }

}
