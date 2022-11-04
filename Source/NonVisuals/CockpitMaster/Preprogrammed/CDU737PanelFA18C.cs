using DCS_BIOS.EventArgs;
using DCS_BIOS;
using NonVisuals.CockpitMaster.Panels;
using System;
using DCS_BIOS.Interfaces;
using NonVisuals.CockpitMaster.Switches;
using System.Collections.Generic;

namespace NonVisuals.CockpitMaster.Preprogrammed
{
    
    public class CDU737PanelFA18C : CDU737PanelBase , IDCSBIOSStringListener
    {
        // List the DCSBios Mappings Here

        private DCSBIOSOutput UFC_OPTION_DISPLAY_1;
        private DCSBIOSOutput UFC_OPTION_CUEING_1;

        private DCSBIOSOutput UFC_OPTION_DISPLAY_2;
        private DCSBIOSOutput UFC_OPTION_CUEING_2;

        private DCSBIOSOutput UFC_OPTION_DISPLAY_3;
        private DCSBIOSOutput UFC_OPTION_CUEING_3;

        private DCSBIOSOutput UFC_OPTION_DISPLAY_4;
        private DCSBIOSOutput UFC_OPTION_CUEING_4;

        private DCSBIOSOutput UFC_OPTION_DISPLAY_5;
        private DCSBIOSOutput UFC_OPTION_CUEING_5;

        private DCSBIOSOutput UFC_SCRATCHPAD_NUMBER_DISPLAY;
        private DCSBIOSOutput UFC_SCRATCHPAD_STRING_1_DISPLAY;
        private DCSBIOSOutput UFC_SCRATCHPAD_STRING_2_DISPLAY;

        private DCSBIOSOutput MASTER_CAUTION_LT;

        string cue1 =" ", cue2=" ", cue3 = " ", cue4 = " ", cue5 = " ";
        string option1 = "    ", option2 = "    ", option3 = "    ", option4 = "    ", option5 = "    ";

        string scractchpad_number = "        "; //8
        string scratchpad_1 ="  ";
        string scratchpad_2 = "  ";

        uint master_caution = 0;

        public CDU737PanelFA18C(HIDSkeleton hidSkeleton) : base(hidSkeleton)
        {
            ConvertTable = CDUTextLineHelpers.AH64ConvertTable;
            CDUPanelKeys = CDUMappedCommandKeyFA18C.GetMappedPanelKeys();
            BIOSEventHandler.AttachStringListener(this);
            BIOSEventHandler.AttachDataListener(this);
            Startup();
        }

        public sealed override void Startup()
        {
            try
            {
                // UFC_BRT = DCSBIOSControlLocator.GetDCSBIOSOutput("UFC_BRT");

                UFC_OPTION_DISPLAY_1 = DCSBIOSControlLocator.GetDCSBIOSOutput("UFC_OPTION_DISPLAY_1");
                DCSBIOSStringManager.AddListeningAddress(UFC_OPTION_DISPLAY_1);

                UFC_OPTION_CUEING_1 = DCSBIOSControlLocator.GetDCSBIOSOutput("UFC_OPTION_CUEING_1");
                DCSBIOSStringManager.AddListeningAddress(UFC_OPTION_CUEING_1);

                UFC_OPTION_DISPLAY_2 = DCSBIOSControlLocator.GetDCSBIOSOutput("UFC_OPTION_DISPLAY_2");
                DCSBIOSStringManager.AddListeningAddress(UFC_OPTION_DISPLAY_2);

                UFC_OPTION_CUEING_2 = DCSBIOSControlLocator.GetDCSBIOSOutput("UFC_OPTION_CUEING_2");
                DCSBIOSStringManager.AddListeningAddress(UFC_OPTION_CUEING_2);

                UFC_OPTION_DISPLAY_3 = DCSBIOSControlLocator.GetDCSBIOSOutput("UFC_OPTION_DISPLAY_3");
                DCSBIOSStringManager.AddListeningAddress(UFC_OPTION_DISPLAY_3);

                UFC_OPTION_CUEING_3 = DCSBIOSControlLocator.GetDCSBIOSOutput("UFC_OPTION_CUEING_3");
                DCSBIOSStringManager.AddListeningAddress(UFC_OPTION_CUEING_3);

                UFC_OPTION_DISPLAY_4 = DCSBIOSControlLocator.GetDCSBIOSOutput("UFC_OPTION_DISPLAY_4");
                DCSBIOSStringManager.AddListeningAddress(UFC_OPTION_DISPLAY_4);

                UFC_OPTION_CUEING_4 = DCSBIOSControlLocator.GetDCSBIOSOutput("UFC_OPTION_CUEING_4");
                DCSBIOSStringManager.AddListeningAddress(UFC_OPTION_CUEING_4);

                UFC_OPTION_DISPLAY_5 = DCSBIOSControlLocator.GetDCSBIOSOutput("UFC_OPTION_DISPLAY_5");
                DCSBIOSStringManager.AddListeningAddress(UFC_OPTION_DISPLAY_5);

                UFC_OPTION_CUEING_5 = DCSBIOSControlLocator.GetDCSBIOSOutput("UFC_OPTION_CUEING_5");
                DCSBIOSStringManager.AddListeningAddress(UFC_OPTION_CUEING_5);

                UFC_SCRATCHPAD_NUMBER_DISPLAY = DCSBIOSControlLocator.GetDCSBIOSOutput("UFC_SCRATCHPAD_NUMBER_DISPLAY");
                DCSBIOSStringManager.AddListeningAddress(UFC_SCRATCHPAD_NUMBER_DISPLAY);

                UFC_SCRATCHPAD_STRING_1_DISPLAY = DCSBIOSControlLocator.GetDCSBIOSOutput("UFC_SCRATCHPAD_STRING_1_DISPLAY");
                DCSBIOSStringManager.AddListeningAddress(UFC_SCRATCHPAD_STRING_1_DISPLAY);

                UFC_SCRATCHPAD_STRING_2_DISPLAY = DCSBIOSControlLocator.GetDCSBIOSOutput("UFC_SCRATCHPAD_STRING_2_DISPLAY");
                DCSBIOSStringManager.AddListeningAddress(UFC_SCRATCHPAD_STRING_2_DISPLAY);

                MASTER_CAUTION_LT = DCSBIOSControlLocator.GetDCSBIOSOutput("MASTER_CAUTION_LT");

                SetLine(0, string.Format("{0,24}", "F/A-18C profile"));

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

                if ( e.Address.Equals(MASTER_CAUTION_LT.Address))
                {
                    uint newMasterCaution = MASTER_CAUTION_LT.GetUIntValue(e.Data);
                    if (master_caution != newMasterCaution)
                    {
                        master_caution = newMasterCaution;
                        if (master_caution == 0)
                        {
                            Led_OFF(CDU737Led.FAIL);
                        }
                        else
                        {
                            Led_ON(CDU737Led.FAIL);
                        }
                        displayBufferOnCDU();
                    }

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
                string incomingData = "";
                const string filler = "                   ";

                SetLine(0, "");

                if (e.Address.Equals(UFC_SCRATCHPAD_NUMBER_DISPLAY.Address))
                {
                    incomingData = e.StringData;
                    if (string.Compare(incomingData, "   pww0w") == 0)
                    {
                        incomingData = "   ERROR";
                    };
                    
                    if (string.Compare(incomingData, scractchpad_number) !=0)
                    {
                        linesChanged++;
                        scractchpad_number = incomingData;
                    }
                }
                if (e.Address.Equals(UFC_SCRATCHPAD_STRING_1_DISPLAY.Address))
                {
                    incomingData = e.StringData;
                    if (string.Compare(incomingData, scratchpad_1) != 0)
                    {
                        linesChanged++;
                        scratchpad_1 = incomingData;
                    }

                }
                if (e.Address.Equals(UFC_SCRATCHPAD_STRING_2_DISPLAY.Address))
                {
                    incomingData = e.StringData;
                    if (string.Compare(incomingData, scratchpad_2) != 0)
                    {
                        linesChanged++;
                        scratchpad_2 = incomingData;
                    }

                }

                SetLine(1, string.Format("{0,2}{1,2}{2,8}" , scratchpad_1, scratchpad_2, scractchpad_number));

                if (e.Address.Equals(UFC_OPTION_DISPLAY_1.Address))
                {
                    incomingData = e.StringData;
                    if (string.Compare(incomingData, option1) != 0)
                    {
                        linesChanged++;
                        option1 = incomingData;
                    }
                }
                
                if (e.Address.Equals(UFC_OPTION_CUEING_1.Address))
                {
                    incomingData = e.StringData;
                    if (string.Compare(incomingData, cue1) != 0)
                    {
                        linesChanged++;
                        cue1 = incomingData;
                    }

                }
                SetLine(2, string.Format("{2,19}{0,1}{1,4}", cue1, option1, filler));
                SetLine(3, "");

                if (e.Address.Equals(UFC_OPTION_DISPLAY_2.Address))
                {
                    incomingData = e.StringData;
                    if (string.Compare(incomingData, option2) != 0)
                    {
                        linesChanged++;
                        option2 = incomingData;
                    }
                }

                if (e.Address.Equals(UFC_OPTION_CUEING_2.Address))
                {
                    incomingData = e.StringData;
                    if (string.Compare(incomingData, cue2) != 0)
                    {
                        linesChanged++;
                        cue2 = incomingData;
                    }
                }
                SetLine(4, string.Format("{2,19}{0,1}{1,4}", cue2, option2, filler));
                SetLine(5, "");

                if (e.Address.Equals(UFC_OPTION_DISPLAY_3.Address))
                {
                    incomingData = e.StringData;
                    if (string.Compare(incomingData, option3) != 0)
                    {
                        linesChanged++;
                        option3 = incomingData;
                    }
                }

                if (e.Address.Equals(UFC_OPTION_CUEING_3.Address))
                {
                    incomingData = e.StringData;
                    if (string.Compare(incomingData, cue3) != 0)
                    {
                        linesChanged++;
                        cue3 = incomingData;
                    }
                }
                SetLine(6, string.Format("{2,19}{0,1}{1,4}", cue3, option3, filler));
                SetLine(7, "");

                if (e.Address.Equals(UFC_OPTION_DISPLAY_4.Address))
                {
                    incomingData = e.StringData;
                    if (string.Compare(incomingData, option4) != 0)
                    {
                        linesChanged++;
                        option4 = incomingData;
                    }
                }

                if (e.Address.Equals(UFC_OPTION_CUEING_4.Address))
                {
                    incomingData = e.StringData;
                    if (string.Compare(incomingData, cue4) != 0)
                    {
                        linesChanged++;
                        cue4 = incomingData;
                    }
                }
                SetLine(8, string.Format("{2,19}{0,1}{1,4}", cue4, option4, filler));
                SetLine(9, "");

                if (e.Address.Equals(UFC_OPTION_DISPLAY_5.Address))
                {
                    incomingData = e.StringData;
                    if (string.Compare(incomingData, option5) != 0)
                    {
                        linesChanged++;
                        option5 = incomingData;
                    }
                }

                if (e.Address.Equals(UFC_OPTION_CUEING_5.Address))
                {
                    incomingData = e.StringData;
                    if (string.Compare(incomingData, cue5) != 0)
                    {
                        linesChanged++;
                        cue5 = incomingData;
                    }
                }
                SetLine(10, string.Format("{2,19}{0,1}{1,4}", cue5, option5,filler));
                SetLine(11, "");

            }

            catch (Exception)
            {

            }
        }
    }

}
