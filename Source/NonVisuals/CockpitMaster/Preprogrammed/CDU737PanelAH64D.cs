using DCS_BIOS.EventArgs;
using DCS_BIOS;
using NonVisuals.CockpitMaster.Panels;
using System;
using DCS_BIOS.Interfaces;
using NonVisuals.CockpitMaster.Switches;
using System.Collections.Generic;
using NonVisuals.HID;

namespace NonVisuals.CockpitMaster.Preprogrammed
{

    public class CDU737PanelAH64D : CDU737PanelBase , IDCSBIOSStringListener
    {
        // List the DCSBios Mappings Here

        private DCSBIOSOutput _PLT_KU_DISPLAY;

        private DCSBIOSOutput _PLT_EUFD_LINE1;
        private DCSBIOSOutput _PLT_EUFD_LINE2;
        private DCSBIOSOutput _PLT_EUFD_LINE3;
        private DCSBIOSOutput _PLT_EUFD_LINE4;
        private DCSBIOSOutput _PLT_EUFD_LINE5;

        private DCSBIOSOutput _PLT_EUFD_LINE8;
        private DCSBIOSOutput _PLT_EUFD_LINE9;
        private DCSBIOSOutput _PLT_EUFD_LINE10;
        private DCSBIOSOutput _PLT_EUFD_LINE11;
        private DCSBIOSOutput _PLT_EUFD_LINE12;
        private DCSBIOSOutput _PLT_EUFD_LINE14;

        private DCSBIOSOutput _PLT_MASTER_IGN_SW;
        private DCSBIOSOutput _PLT_EUFD_BRT;

        // Lights

        private DCSBIOSOutput _PLT_MASTER_WARNING_L;

        public CDU737PanelAH64D(HIDSkeleton hidSkeleton) : base(hidSkeleton)
        {
            ConvertTable = CDUTextLineHelpers.AH64ConvertTable;
            CDUPanelKeys = CDUMappedCommandKeyAH64D.GetMappedPanelKeys();
            BIOSEventHandler.AttachStringListener(this);
            BIOSEventHandler.AttachDataListener(this);
            Startup();
        }

        public sealed override void Startup()
        {
            try
            {

                // PLT Keyboard display

                _PLT_KU_DISPLAY = DCSBIOSControlLocator.GetStringDCSBIOSOutput("PLT_KU_DISPLAY");

                _PLT_MASTER_IGN_SW = DCSBIOSControlLocator.GetStringDCSBIOSOutput("PLT_MASTER_IGN_SW");
                _PLT_EUFD_BRT = DCSBIOSControlLocator.GetStringDCSBIOSOutput("PLT_EUFD_BRT");

                // UFD Upper status 

                _PLT_EUFD_LINE1 = DCSBIOSControlLocator.GetStringDCSBIOSOutput("PLT_EUFD_LINE1");
                _PLT_EUFD_LINE2 = DCSBIOSControlLocator.GetStringDCSBIOSOutput("PLT_EUFD_LINE2");
                _PLT_EUFD_LINE3 = DCSBIOSControlLocator.GetStringDCSBIOSOutput("PLT_EUFD_LINE3");
                _PLT_EUFD_LINE4 = DCSBIOSControlLocator.GetStringDCSBIOSOutput("PLT_EUFD_LINE4");
                _PLT_EUFD_LINE5 = DCSBIOSControlLocator.GetStringDCSBIOSOutput("PLT_EUFD_LINE5");

                // UFD Frequency
                _PLT_EUFD_LINE8 = DCSBIOSControlLocator.GetStringDCSBIOSOutput("PLT_EUFD_LINE8");

                _PLT_EUFD_LINE9 = DCSBIOSControlLocator.GetStringDCSBIOSOutput("PLT_EUFD_LINE9");

                _PLT_EUFD_LINE10 = DCSBIOSControlLocator.GetStringDCSBIOSOutput("PLT_EUFD_LINE10");

                _PLT_EUFD_LINE11 = DCSBIOSControlLocator.GetStringDCSBIOSOutput("PLT_EUFD_LINE11");

                _PLT_EUFD_LINE12 = DCSBIOSControlLocator.GetStringDCSBIOSOutput("PLT_EUFD_LINE12");

                _PLT_EUFD_LINE14 = DCSBIOSControlLocator.GetStringDCSBIOSOutput("PLT_EUFD_LINE14");

                _PLT_MASTER_WARNING_L = DCSBIOSControlLocator.GetStringDCSBIOSOutput("PLT_MASTER_WARNING_L");

                SetLine(0, string.Format("{0,24}", "AH64D profile"));

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
                bool shouldUpdate;
                uint newValue;

                UpdateCounter(e.Address, e.Data);

                (shouldUpdate, newValue) = ShouldHandleDCSBiosData(e, _PLT_MASTER_IGN_SW);

                if (shouldUpdate)
                {
                    if (newValue == 0)
                    {
                        ScreenBrightness = 0;
                        KeyboardBrightness = 0;
                    }
                    displayBufferOnCDU();

                }

                ( shouldUpdate, newValue) = ShouldHandleDCSBiosData(e, _PLT_EUFD_BRT);

                if (shouldUpdate)
                {
                    int eufdBright = (int)newValue;
                    // MAX_BRIGHT is 256 , so 655356 / 256 is 256 , we need to divide by 2^8
                    ScreenBrightness = eufdBright >> 8;
                    KeyboardBrightness= eufdBright >>8;
                    refreshLedsAndBrightness();
                }

                // AH - 64D / PLT_MASTER_WARNING_L
                (shouldUpdate, newValue) = ShouldHandleDCSBiosData(e, _PLT_MASTER_WARNING_L);
                if(shouldUpdate)
                {
                    if (newValue == 1)
                    {
                        Led_ON(CDU737Led.FAIL);
                    }
                    else
                    {
                        Led_OFF(CDU737Led.FAIL);
                    }
                    refreshLedsAndBrightness();

                }
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
                string incomingData;

                if (e.Address.Equals(_PLT_EUFD_LINE14.Address))
                {
                    incomingData = e.StringData.Substring(46, 10);
                    if (HandleStringData(0, e, incomingData, ref linesChanged))
                    {
                        SetLine(0, string.Format("{0,24}", incomingData));
                    }
                    
                }

                if (e.Address.Equals(_PLT_EUFD_LINE1.Address))
                {
                    incomingData = e.StringData.Substring(38, 17);
                    if (HandleStringData(1, e, incomingData, ref linesChanged))
                    {
                        SetLine(1, string.Format("{0,24}", incomingData ));
                    }
                    
                }

                if (e.Address.Equals(_PLT_EUFD_LINE2.Address))
                {
                    incomingData = e.StringData.Substring(38, 17);
                    if (HandleStringData(2, e, incomingData, ref linesChanged))
                    {
                        SetLine(2, string.Format("{0,24}", incomingData));
                    }
                }

                if (e.Address.Equals(_PLT_EUFD_LINE3.Address))
                {
                    incomingData = e.StringData.Substring(38, 17);
                    if (HandleStringData(3, e, incomingData, ref linesChanged))
                    {
                        SetLine(3, string.Format("{0,24}", incomingData));
                    }
                }

                if (e.Address.Equals(_PLT_EUFD_LINE4.Address))
                {
                    incomingData = e.StringData.Substring(38, 17);
                    if (HandleStringData(4, e, incomingData, ref linesChanged))
                    {
                        SetLine(4, string.Format("{0,24}", incomingData));
                    }
                }

                if (e.Address.Equals(_PLT_EUFD_LINE5.Address))
                {
                    incomingData = e.StringData.Substring(38, 17);
                    if (HandleStringData(5, e, incomingData, ref linesChanged))
                    {
                        SetLine(5, string.Format("{0,24}", incomingData));
                    }
                }

                // UNused
                SetLine(6, "");

                // Radios Frequencies

                if (e.Address.Equals(_PLT_EUFD_LINE8.Address))
                {
                    incomingData = e.StringData.Substring(0, 18);
                    if (HandleStringData(7, e, incomingData, ref linesChanged))
                    {
                        SetLine(7, incomingData );
                    }
                        
                }
                if (e.Address.Equals(_PLT_EUFD_LINE9.Address))
                {
                    incomingData = e.StringData.Substring(0, 18);
                    if (HandleStringData(8, e, incomingData, ref linesChanged))
                    {
                        SetLine(8, incomingData);
                    }
                }
                if (e.Address.Equals(_PLT_EUFD_LINE10.Address))
                {
                    incomingData = e.StringData.Substring(0, 18);
                    if (HandleStringData(9, e, incomingData, ref linesChanged))
                    {
                        SetLine(9, incomingData);
                    }

                }
                if (e.Address.Equals(_PLT_EUFD_LINE11.Address))
                {
                    incomingData = e.StringData.Substring(0, 18);
                    if (HandleStringData(10, e, incomingData, ref linesChanged))
                    {
                        SetLine(10, incomingData);
                    }

                }
                if (e.Address.Equals(_PLT_EUFD_LINE12.Address))
                {
                    incomingData = e.StringData.Substring(0, 18);
                    if (HandleStringData(11, e, incomingData, ref linesChanged))
                    {
                        SetLine(11, incomingData);
                    }
                }

                SetLine(12, "- Keyboard -------------");

                if (e.Address.Equals(_PLT_KU_DISPLAY.Address))
                {
                    incomingData = e.StringData;
                    if (HandleStringData(13, e, incomingData, ref linesChanged))
                    {
                        SetLine(13, incomingData);
                    }

                }

            }

            catch (Exception)
            {

            }
        }
    }

}
