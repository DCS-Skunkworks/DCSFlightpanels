using DCS_BIOS.EventArgs;
using DCS_BIOS;
using NonVisuals.CockpitMaster.Panels;
using System;
using DCS_BIOS.Interfaces;
using NonVisuals.CockpitMaster.Switches;
using System.Collections.Generic;

namespace NonVisuals.CockpitMaster.Preprogrammed
{
    public class CDU737PanelA10C : CDU737PanelBase , IDCSBIOSStringListener
    {
        // List the DCSBios Mappings Here

        private DCSBIOSOutput _CDU_LINE_0;
        private DCSBIOSOutput _CDU_LINE_1;
        private DCSBIOSOutput _CDU_LINE_2;
        private DCSBIOSOutput _CDU_LINE_3;
        private DCSBIOSOutput _CDU_LINE_4;
        private DCSBIOSOutput _CDU_LINE_5;
        private DCSBIOSOutput _CDU_LINE_6;
        private DCSBIOSOutput _CDU_LINE_7;
        private DCSBIOSOutput _CDU_LINE_8;
        private DCSBIOSOutput _CDU_LINE_9;

        private DCSBIOSOutput _CDU_BRT;

        private DCSBIOSOutput _MASTER_CAUTION;

        public CDU737PanelA10C(HIDSkeleton hidSkeleton) : base(hidSkeleton)
        {
            CDUPanelKeys = CDUMappedCommandKeyA10C.GetMappedPanelKeys();
            BIOSEventHandler.AttachStringListener(this);
            BIOSEventHandler.AttachDataListener(this);
        }

        public sealed override void Startup()
        {
            try
            {
                // CDU Lines & BRT

                _CDU_LINE_0 = DCSBIOSControlLocator.GetDCSBIOSOutput("CDU_LINE0");
                DCSBIOSStringManager.AddListeningAddress(_CDU_LINE_0);
                _CDU_LINE_1 = DCSBIOSControlLocator.GetDCSBIOSOutput("CDU_LINE1");
                DCSBIOSStringManager.AddListeningAddress(_CDU_LINE_1);
                _CDU_LINE_2 = DCSBIOSControlLocator.GetDCSBIOSOutput("CDU_LINE2");
                DCSBIOSStringManager.AddListeningAddress(_CDU_LINE_2);
                _CDU_LINE_3 = DCSBIOSControlLocator.GetDCSBIOSOutput("CDU_LINE3");
                DCSBIOSStringManager.AddListeningAddress(_CDU_LINE_3);
                _CDU_LINE_4 = DCSBIOSControlLocator.GetDCSBIOSOutput("CDU_LINE4");
                DCSBIOSStringManager.AddListeningAddress(_CDU_LINE_4);
                _CDU_LINE_5 = DCSBIOSControlLocator.GetDCSBIOSOutput("CDU_LINE5");
                DCSBIOSStringManager.AddListeningAddress(_CDU_LINE_5);
                _CDU_LINE_6 = DCSBIOSControlLocator.GetDCSBIOSOutput("CDU_LINE6");
                DCSBIOSStringManager.AddListeningAddress(_CDU_LINE_6);
                _CDU_LINE_7 = DCSBIOSControlLocator.GetDCSBIOSOutput("CDU_LINE7");
                DCSBIOSStringManager.AddListeningAddress(_CDU_LINE_7);
                _CDU_LINE_8 = DCSBIOSControlLocator.GetDCSBIOSOutput("CDU_LINE8");
                DCSBIOSStringManager.AddListeningAddress(_CDU_LINE_8);
                _CDU_LINE_9 = DCSBIOSControlLocator.GetDCSBIOSOutput("CDU_LINE9");
                DCSBIOSStringManager.AddListeningAddress(_CDU_LINE_9);

                _CDU_BRT = DCSBIOSControlLocator.GetDCSBIOSOutput("CDU_BRT");
                _MASTER_CAUTION = DCSBIOSControlLocator.GetDCSBIOSOutput("MASTER_CAUTION");

                SetLine(0, string.Format("{0,24}","A10-C profile"));

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

                // CDU Brightness
                if (e.Address == _CDU_BRT.Address)
                {
                    int rocketSwitch = (int)_CDU_BRT.GetUIntValue(e.Data);
                    switch (rocketSwitch)
                    {
                        case 0:
                            DecreaseBrighness();
                            break;
                        case 2:
                            IncreaseBrightness();
                            break;
                    }

                }

                if (e.Address == _MASTER_CAUTION.Address)
                {
                    int masterCaution = (int) _MASTER_CAUTION.GetUIntValue(e.Data);
                    if ( masterCaution == 1)
                    {
                        Led_ON(CDU737Led.FAIL);
                        
                    }
                    else
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
                string data = e.StringData
                    .Replace("***", "xxx")
                    .Replace("**", "xx");

                if (e.Address.Equals(_CDU_LINE_0.Address))
                {
                    SetLine(0, data);
                }
                if (e.Address.Equals(_CDU_LINE_1.Address))
                {
                    SetLine(1, data);
                }

                if (e.Address.Equals(_CDU_LINE_2.Address))
                {
                    SetLine(2, data);
                }

                if (e.Address.Equals(_CDU_LINE_3.Address))
                {
                    SetLine(3, data);
                }

                if (e.Address.Equals(_CDU_LINE_4.Address))
                {
                    SetLine(4, data);
                }

                if (e.Address.Equals(_CDU_LINE_5.Address))
                {
                    SetLine(5, data);
                }

                if (e.Address.Equals(_CDU_LINE_6.Address))
                {
                    SetLine(6, data);
                }

                if (e.Address.Equals(_CDU_LINE_7.Address))
                {
                    SetLine(7, data);
                }

                if (e.Address.Equals(_CDU_LINE_8.Address))
                {
                    SetLine(8, data);
                }

                if (e.Address.Equals(_CDU_LINE_9.Address))
                {
                    SetLine(9, data);
                }

            }

            catch (Exception)
            {

            }
        }
    }

}
