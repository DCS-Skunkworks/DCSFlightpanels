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

        string _cue1 =" ", _cue2=" ", _cue3 = " ", _cue4 = " ", _cue5 = " ";
        string _option1 = "    ", _option2 = "    ", _option3 = "    ", _option4 = "    ", _option5 = "    ";

        string _scratchPadNumber = "        "; //8
        string _scratchPad1 ="  ";
        string _scratchPad2 = "  ";

        uint _masterCaution = 0;

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

                UFC_OPTION_DISPLAY_1 = DCSBIOSControlLocator.GetStringDCSBIOSOutput("UFC_OPTION_DISPLAY_1");

                UFC_OPTION_CUEING_1 = DCSBIOSControlLocator.GetStringDCSBIOSOutput("UFC_OPTION_CUEING_1");

                UFC_OPTION_DISPLAY_2 = DCSBIOSControlLocator.GetStringDCSBIOSOutput("UFC_OPTION_DISPLAY_2");

                UFC_OPTION_CUEING_2 = DCSBIOSControlLocator.GetStringDCSBIOSOutput("UFC_OPTION_CUEING_2");

                UFC_OPTION_DISPLAY_3 = DCSBIOSControlLocator.GetStringDCSBIOSOutput("UFC_OPTION_DISPLAY_3");

                UFC_OPTION_CUEING_3 = DCSBIOSControlLocator.GetStringDCSBIOSOutput("UFC_OPTION_CUEING_3");

                UFC_OPTION_DISPLAY_4 = DCSBIOSControlLocator.GetStringDCSBIOSOutput("UFC_OPTION_DISPLAY_4");

                UFC_OPTION_CUEING_4 = DCSBIOSControlLocator.GetStringDCSBIOSOutput("UFC_OPTION_CUEING_4");

                UFC_OPTION_DISPLAY_5 = DCSBIOSControlLocator.GetStringDCSBIOSOutput("UFC_OPTION_DISPLAY_5");

                UFC_OPTION_CUEING_5 = DCSBIOSControlLocator.GetStringDCSBIOSOutput("UFC_OPTION_CUEING_5");

                UFC_SCRATCHPAD_NUMBER_DISPLAY = DCSBIOSControlLocator.GetStringDCSBIOSOutput("UFC_SCRATCHPAD_NUMBER_DISPLAY");

                UFC_SCRATCHPAD_STRING_1_DISPLAY = DCSBIOSControlLocator.GetStringDCSBIOSOutput("UFC_SCRATCHPAD_STRING_1_DISPLAY");

                UFC_SCRATCHPAD_STRING_2_DISPLAY = DCSBIOSControlLocator.GetStringDCSBIOSOutput("UFC_SCRATCHPAD_STRING_2_DISPLAY");

                MASTER_CAUTION_LT = DCSBIOSControlLocator.GetUIntDCSBIOSOutput("MASTER_CAUTION_LT");

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
                    if (_masterCaution != newMasterCaution)
                    {
                        _masterCaution = newMasterCaution;
                        if (_masterCaution == 0)
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
                string incomingData;
                const string filler = "                   ";

                SetLine(0, "");

                if (e.Address.Equals(UFC_SCRATCHPAD_NUMBER_DISPLAY.Address))
                {
                    incomingData = e.StringData;
                    if (string.Compare(incomingData, "   pww0w") == 0)
                    {
                        incomingData = "   ERROR";
                    };
                    
                    if (string.Compare(incomingData, _scratchPadNumber) !=0)
                    {
                        _scratchPadNumber = incomingData;
                    }
                }
                if (e.Address.Equals(UFC_SCRATCHPAD_STRING_1_DISPLAY.Address))
                {
                    incomingData = e.StringData;
                    if (string.Compare(incomingData, _scratchPad1) != 0)
                    {
                        _scratchPad1 = incomingData;
                    }

                }
                if (e.Address.Equals(UFC_SCRATCHPAD_STRING_2_DISPLAY.Address))
                {
                    incomingData = e.StringData;
                    if (string.Compare(incomingData, _scratchPad2) != 0)
                    {
                        _scratchPad2 = incomingData;
                    }

                }

                SetLine(1, string.Format("{0,2}{1,2}{2,8}" , _scratchPad1, _scratchPad2, _scratchPadNumber));

                if (e.Address.Equals(UFC_OPTION_DISPLAY_1.Address))
                {
                    incomingData = e.StringData;
                    if (string.Compare(incomingData, _option1) != 0)
                    {
                        _option1 = incomingData;
                    }
                }
                
                if (e.Address.Equals(UFC_OPTION_CUEING_1.Address))
                {
                    incomingData = e.StringData;
                    if (string.Compare(incomingData, _cue1) != 0)
                    {
                        _cue1 = incomingData;
                    }

                }
                SetLine(2, string.Format("{2,19}{0,1}{1,4}", _cue1, _option1, filler));
                SetLine(3, "");

                if (e.Address.Equals(UFC_OPTION_DISPLAY_2.Address))
                {
                    incomingData = e.StringData;
                    if (string.Compare(incomingData, _option2) != 0)
                    {
                        _option2 = incomingData;
                    }
                }

                if (e.Address.Equals(UFC_OPTION_CUEING_2.Address))
                {
                    incomingData = e.StringData;
                    if (string.Compare(incomingData, _cue2) != 0)
                    {
                        _cue2 = incomingData;
                    }
                }
                SetLine(4, string.Format("{2,19}{0,1}{1,4}", _cue2, _option2, filler));
                SetLine(5, "");

                if (e.Address.Equals(UFC_OPTION_DISPLAY_3.Address))
                {
                    incomingData = e.StringData;
                    if (string.Compare(incomingData, _option3) != 0)
                    {
                        _option3 = incomingData;
                    }
                }

                if (e.Address.Equals(UFC_OPTION_CUEING_3.Address))
                {
                    incomingData = e.StringData;
                    if (string.Compare(incomingData, _cue3) != 0)
                    {
                        _cue3 = incomingData;
                    }
                }
                SetLine(6, string.Format("{2,19}{0,1}{1,4}", _cue3, _option3, filler));
                SetLine(7, "");

                if (e.Address.Equals(UFC_OPTION_DISPLAY_4.Address))
                {
                    incomingData = e.StringData;
                    if (string.Compare(incomingData, _option4) != 0)
                    {
                        _option4 = incomingData;
                    }
                }

                if (e.Address.Equals(UFC_OPTION_CUEING_4.Address))
                {
                    incomingData = e.StringData;
                    if (string.Compare(incomingData, _cue4) != 0)
                    {
                        _cue4 = incomingData;
                    }
                }
                SetLine(8, string.Format("{2,19}{0,1}{1,4}", _cue4, _option4, filler));
                SetLine(9, "");

                if (e.Address.Equals(UFC_OPTION_DISPLAY_5.Address))
                {
                    incomingData = e.StringData;
                    if (string.Compare(incomingData, _option5) != 0)
                    {
                        _option5 = incomingData;
                    }
                }

                if (e.Address.Equals(UFC_OPTION_CUEING_5.Address))
                {
                    incomingData = e.StringData;
                    if (string.Compare(incomingData, _cue5) != 0)
                    {
                        _cue5 = incomingData;
                    }
                }
                SetLine(10, string.Format("{2,19}{0,1}{1,4}", _cue5, _option5,filler));
                SetLine(11, "");

            }

            catch (Exception)
            {

            }
        }
    }

}
