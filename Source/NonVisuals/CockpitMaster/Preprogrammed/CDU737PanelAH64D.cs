using DCS_BIOS.EventArgs;
using DCS_BIOS;
using NonVisuals.CockpitMaster.Panels;
using System;
using DCS_BIOS.Interfaces;
using NonVisuals.CockpitMaster.Switches;
using System.Collections.Generic;

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

        }

        public sealed override void Startup()
        {
            try
            {

                // PLT Keyboard display

                _PLT_KU_DISPLAY = DCSBIOSControlLocator.GetDCSBIOSOutput("PLT_KU_DISPLAY");
                DCSBIOSStringManager.AddListeningAddress(_PLT_KU_DISPLAY);

                _PLT_MASTER_IGN_SW = DCSBIOSControlLocator.GetDCSBIOSOutput("PLT_MASTER_IGN_SW");
                _PLT_EUFD_BRT = DCSBIOSControlLocator.GetDCSBIOSOutput("PLT_EUFD_BRT");

                // UFD Upper status 

                _PLT_EUFD_LINE1 = DCSBIOSControlLocator.GetDCSBIOSOutput("PLT_EUFD_LINE1");
                DCSBIOSStringManager.AddListeningAddress(_PLT_EUFD_LINE1);
                _PLT_EUFD_LINE2 = DCSBIOSControlLocator.GetDCSBIOSOutput("PLT_EUFD_LINE2");
                DCSBIOSStringManager.AddListeningAddress(_PLT_EUFD_LINE2);
                _PLT_EUFD_LINE3 = DCSBIOSControlLocator.GetDCSBIOSOutput("PLT_EUFD_LINE3");
                DCSBIOSStringManager.AddListeningAddress(_PLT_EUFD_LINE3);
                _PLT_EUFD_LINE4 = DCSBIOSControlLocator.GetDCSBIOSOutput("PLT_EUFD_LINE4");
                DCSBIOSStringManager.AddListeningAddress(_PLT_EUFD_LINE4);
                _PLT_EUFD_LINE5 = DCSBIOSControlLocator.GetDCSBIOSOutput("PLT_EUFD_LINE5");
                DCSBIOSStringManager.AddListeningAddress(_PLT_EUFD_LINE5);

                // UFD Frequency
                _PLT_EUFD_LINE8 = DCSBIOSControlLocator.GetDCSBIOSOutput("PLT_EUFD_LINE8");
                DCSBIOSStringManager.AddListeningAddress(_PLT_EUFD_LINE8);

                _PLT_EUFD_LINE9 = DCSBIOSControlLocator.GetDCSBIOSOutput("PLT_EUFD_LINE9");
                DCSBIOSStringManager.AddListeningAddress(_PLT_EUFD_LINE9);

                _PLT_EUFD_LINE10 = DCSBIOSControlLocator.GetDCSBIOSOutput("PLT_EUFD_LINE10");
                DCSBIOSStringManager.AddListeningAddress(_PLT_EUFD_LINE10);

                _PLT_EUFD_LINE11 = DCSBIOSControlLocator.GetDCSBIOSOutput("PLT_EUFD_LINE11");
                DCSBIOSStringManager.AddListeningAddress(_PLT_EUFD_LINE11);

                _PLT_EUFD_LINE12 = DCSBIOSControlLocator.GetDCSBIOSOutput("PLT_EUFD_LINE12");
                DCSBIOSStringManager.AddListeningAddress(_PLT_EUFD_LINE12);

                _PLT_EUFD_LINE14 = DCSBIOSControlLocator.GetDCSBIOSOutput("PLT_EUFD_LINE14");
                DCSBIOSStringManager.AddListeningAddress(_PLT_EUFD_LINE14);

                _PLT_MASTER_WARNING_L = DCSBIOSControlLocator.GetDCSBIOSOutput("PLT_MASTER_WARNING_L");

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
                UpdateCounter(e.Address, e.Data);

                if (e.Address == _PLT_MASTER_IGN_SW.Address)
                {
                    if (_PLT_MASTER_IGN_SW.GetUIntValue(e.Data) == 0)
                    {
                        ScreenBrightness = 0;
                        KeyboardBrightness = 0;
                    }
                }

                if (e.Address == _PLT_EUFD_BRT.Address)
                {
                    long eufdBright = (int)_PLT_EUFD_BRT.GetUIntValue(e.Data);
                    // MAX_BRIGHT is 256 , so 655356 / 256 is 256 , we need to divide by 2^8
                    ScreenBrightness = (int) eufdBright >> 8;
                    KeyboardBrightness= (int)  eufdBright >>8;
                }

                // AH - 64D / PLT_MASTER_WARNING_L

                if ( e.Address == _PLT_MASTER_WARNING_L.Address)
                {
                    if (_PLT_MASTER_WARNING_L.GetUIntValue(e.Data) ==1 )
                    {
                        Led_ON(CDU737Led.FAIL);
                    } else
                    {
                        Led_OFF(CDU737Led.FAIL);
                    }
                }
            }
            catch (Exception)
            {
            }

        }
        public new void DCSBIOSStringReceived(object sender, DCSBIOSStringDataEventArgs e)
        {
            try
            {
                if (e.Address.Equals(_PLT_EUFD_LINE14.Address))
                {
                    SetLine(0, string.Format("{0,24}",e.StringData.Substring(46, 10)));
                }

                if (e.Address.Equals(_PLT_EUFD_LINE1.Address))
                {
                    SetLine(1, string.Format("{0,24}", e.StringData.Substring(38, 17)));
                }
                if (e.Address.Equals(_PLT_EUFD_LINE2.Address))
                {
                    SetLine(2, string.Format("{0,24}", e.StringData.Substring(38, 17)));
                }
                if (e.Address.Equals(_PLT_EUFD_LINE3.Address))
                {
                    SetLine(3, string.Format("{0,24}", e.StringData.Substring(38, 17)));
                }
                if (e.Address.Equals(_PLT_EUFD_LINE4.Address))
                {
                    SetLine(4, string.Format("{0,24}", e.StringData.Substring(38, 17)));
                }
                if (e.Address.Equals(_PLT_EUFD_LINE5.Address))
                {
                    SetLine(5, string.Format("{0,24}", e.StringData.Substring(38, 17)));
                }

                // UNused
                SetLine(6, "");

                // Radios Frequencies

                if (e.Address.Equals(_PLT_EUFD_LINE8.Address))
                {
                    SetLine(7, e.StringData.Substring(0,18));
                }
                if (e.Address.Equals(_PLT_EUFD_LINE9.Address))
                {
                    SetLine(8, e.StringData.Substring(0, 18));
                }
                if (e.Address.Equals(_PLT_EUFD_LINE10.Address))
                {
                    SetLine(9, e.StringData.Substring(0, 18));
                }
                if (e.Address.Equals(_PLT_EUFD_LINE11.Address))
                {
                    SetLine(10, e.StringData.Substring(0, 18));
                }
                if (e.Address.Equals(_PLT_EUFD_LINE12.Address))
                {
                    SetLine(11, e.StringData.Substring(0, 18));
                }

                SetLine(12, "- Keyboard -------------");

                if (e.Address.Equals(_PLT_KU_DISPLAY.Address))
                {
                    SetLine(13, e.StringData);
                }

            }

            catch (Exception)
            {

            }
        }
    }

}
