using DCS_BIOS.EventArgs;
using DCS_BIOS;
using NonVisuals.CockpitMaster.Panels;
using System;
using DCS_BIOS.Interfaces;
using NonVisuals.CockpitMaster.Switches;
using System.Collections.Generic;
using NonVisuals.HID;
using DCS_BIOS.Serialized;
using DCS_BIOS.ControlLocator;

namespace NonVisuals.CockpitMaster.PreProgrammed
{
    public class CDU737PanelF14 : CDU737PanelBase, IDCSBIOSStringListener
    {
        // List the DCSBios Mappings Here

        private DCSBIOSOutput _RIO_CAP_CATEGORY;

        private DCSBIOSOutput _RIO_HCU_TID;
        private DCSBIOSOutput _RIO_HCU_TCS;
        private DCSBIOSOutput _RIO_HCU_RADAR;
        private DCSBIOSOutput _RIO_HCU_DDD;

        private DCSBIOSOutput _RIO_RADAR_TWSAUTO;
        private DCSBIOSOutput _RIO_RADAR_PULSESEARCH;
        private DCSBIOSOutput _RIO_RADAR_TWSMAN;
        private DCSBIOSOutput _RIO_RADAR_PDSRCH;

        private DCSBIOSOutput _RIO_RADAR_RWS;
        private DCSBIOSOutput _RIO_RADAR_PDSTT;
        private DCSBIOSOutput _RIO_RADAR_PSTT;

        private int _currentHCUMode;
        private readonly string[] _hcuModes = new string[4]
        {
            "TID", "TCS", "RADAR", "DDD"
        };

        private int _currentWCSmode;
        private readonly string[] _cwsModes = new string[7] {
            "TWS Auto","Puls Sch","TWS Man","PD Sch","PD STT","RWS","Puls Stt"
            };

        private bool _disposed;
        private int _capPage;

        private readonly string[] _categoryNames = { "BIT", "SPL", "NAV", "TAC DATA", "D/L", "TGT DATA" };

        private readonly string[][] _categoryLabelPages = {
            //              "-0123456789001234567890-","-0123456789001234567890-","-0123456789001234567890-","-0123456789001234567890-","-0123456789001234567890-"
            new[] { "-   disp        rcvr   -","-   cmptr       xmtr   -","- amcs conf    ant ir  -","- mas moat      stt    -","-   fault     spl test -"},
            new[] { "      1          5      ","      2          6      ","      3          7      ","      4          8      ","    disp        nbr     "},
            
            //              "-0123456789001234567890-","-0123456789001234567890-","-0123456789001234567890-","-0123456789001234567890-","-0123456789001234567890-"
            new[] { "-   home                ","-    ift        bit    -","-   ip          obc    -","-   gss         maint  -","-   air to      obc    -"},
            new[] { "  on heli               ","    menu    moving tgt  ","  to tgt        bit     ","                disp    ","    ground     displ    "},

            //              "-0123456789001234567890-","-0123456789001234567890-","-0123456789001234567890-","-0123456789001234567890-","-0123456789001234567890-"
            new[] { "-   own        tacan   -","- store hdg     rdr    -","- tarps nav     vis    -","-   wind        fix    -","-   tarps     mag var  -"},
            new[] { "    a/c         fix     ","    align       fix     ","     fix        fix     ","  spd hdg      enable   ","               (hdg)    "},

            
            //              "-0123456789001234567890-","-0123456789001234567890-","-0123456789001234567890-","-0123456789001234567890-","-0123456789001234567890-"
            new[] { "-  waypt        home   -","-  waypt         def   -","-  waypt        host   -","-   fix         surf   -","-    ip        pt to   -"},
            new[] { "     1          base    ","     2           pt     ","     3          area    ","     pt         tgt     ","                 pt     "},

            //              "-0123456789001234567890-","-0123456789001234567890-","-0123456789001234567890-","-0123456789001234567890-","-0123456789001234567890-"
            new[] { "-  wilco       point   -","-  cantco      engage  -","-   nav        flrp    -","-   tid        chaff   -","- f/f nav    f/f auto  -"},
            new[] { "                        ","                        ","    grid                ","    avia       count    ","  update       rstt     "},

            //              "-0123456789001234567890-","-0123456789001234567890-","-0123456789001234567890-","-0123456789001234567890-","-0123456789001234567890-"
            new[] { "-   gnd        friend  -","-  do not        unk   -","-  ift aux      host   -","-   data        mult   -","-   test         sym   -"},
            new[] { "    map                 ","   attack               ","   launch               ","    trans        tgt    ","    tgt       delete    "},

        };

        public CDU737PanelF14(HIDSkeleton hidSkeleton) : base(hidSkeleton)
        {}

        public override void InitPanel()
        {
            try
            {
                base.InitPanel();

                CDUPanelKeys = CDUMappedCommandKeyF14.GetMappedPanelKeys();
                
                BaseColor = CDUColor.WHITE;

                _RIO_CAP_CATEGORY = DCSBIOSControlLocator.GetUIntDCSBIOSOutput("RIO_CAP_CATRGORY");

                _RIO_HCU_TID  = DCSBIOSControlLocator.GetUIntDCSBIOSOutput("RIO_HCU_TID");
                _RIO_HCU_TCS = DCSBIOSControlLocator.GetUIntDCSBIOSOutput("RIO_HCU_TCS");
                _RIO_HCU_RADAR = DCSBIOSControlLocator.GetUIntDCSBIOSOutput("RIO_HCU_RADAR");
                _RIO_HCU_DDD = DCSBIOSControlLocator.GetUIntDCSBIOSOutput("RIO_HCU_DDD");

                _RIO_RADAR_TWSAUTO = DCSBIOSControlLocator.GetUIntDCSBIOSOutput("RIO_RADAR_TWSAUTO");
                _RIO_RADAR_PULSESEARCH = DCSBIOSControlLocator.GetUIntDCSBIOSOutput("RIO_RADAR_PULSE");
                _RIO_RADAR_TWSMAN = DCSBIOSControlLocator.GetUIntDCSBIOSOutput("RIO_RADAR_TWSMAN");
                _RIO_RADAR_PDSRCH = DCSBIOSControlLocator.GetUIntDCSBIOSOutput("RIO_RADAR_PDSRCH");
                _RIO_RADAR_RWS = DCSBIOSControlLocator.GetUIntDCSBIOSOutput("RIO_RADAR_RWS");
                _RIO_RADAR_PDSTT = DCSBIOSControlLocator.GetUIntDCSBIOSOutput("RIO_RADAR_PDSTT");
                _RIO_RADAR_PSTT = DCSBIOSControlLocator.GetUIntDCSBIOSOutput("RIO_RADAR_PSTT");

                BIOSEventHandler.AttachStringListener(this);
                BIOSEventHandler.AttachDataListener(this);

                SetLine(0, string.Format("{0,24}","F14 RIO profile"));

                // Alternate CYAN every two lines to visually help separation
                // the aircraft have lines to do this. 

                int currentLine = 2;
                for (int line = 0; line < 6; line++)
                {
                    if(line%2 == 1)
                    {
                        SetColorForLine(currentLine++, CDUColor.CYAN);
                        SetColorForLine(currentLine++, CDUColor.CYAN);
                    }
                    else
                    {
                        currentLine += 2;
                    }
                }

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
                    DCSBIOS.Send(key.MappedCommand());
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

                // RIO CAP CATEGROY
                if (e.Address == _RIO_CAP_CATEGORY.Address)
                {
                    _capPage = (int)_RIO_CAP_CATEGORY.GetUIntValue(e.Data);
                    updateCDU();
                }

                // "TID", "TCS", "RADAR", "DDD"
                if (e.Address == _RIO_HCU_TID.Address)
                {
                    if (_RIO_HCU_TID.GetUIntValue(e.Data) == 1) 
                    { 
                        _currentHCUMode = 0;
                        updateCDU();
                    }

                }
                if (e.Address == _RIO_HCU_TCS.Address)
                {
                    if (_RIO_HCU_TCS.GetUIntValue(e.Data) == 1)
                    {
                        _currentHCUMode = 1;
                        updateCDU();
                    }

                }
                if (e.Address == _RIO_HCU_RADAR.Address)
                {
                    if (_RIO_HCU_RADAR.GetUIntValue(e.Data) == 1)
                    {
                        _currentHCUMode = 2;
                        updateCDU();
                    }

                }
                if (e.Address == _RIO_HCU_DDD.Address)
                {
                        if (_RIO_HCU_DDD.GetUIntValue(e.Data) == 1)
                        {
                            _currentHCUMode = 3;
                            updateCDU();
                        }
                }

                // CWS MODES 
                if (e.Address == _RIO_RADAR_TWSAUTO.Address)
                {
                    if (_RIO_RADAR_TWSAUTO.GetUIntValue(e.Data) == 1)
                    {
                        _currentWCSmode = 0;
                        updateCDU();
                    }
                }
                if (e.Address == _RIO_RADAR_PULSESEARCH.Address)
                {
                    if (_RIO_RADAR_PULSESEARCH.GetUIntValue(e.Data) == 1)
                    {
                        _currentWCSmode = 1;
                        updateCDU();
                    }
                }
                if (e.Address == _RIO_RADAR_TWSMAN.Address)
                {
                    if (_RIO_RADAR_TWSMAN.GetUIntValue(e.Data) == 1)
                    {
                        _currentWCSmode = 2;
                        updateCDU();
                    }
                }
                if (e.Address == _RIO_RADAR_PDSRCH.Address)
                {
                    if (_RIO_RADAR_PDSRCH.GetUIntValue(e.Data) == 1)
                    {
                        _currentWCSmode = 3;
                        updateCDU();
                    }
                }
                if (e.Address == _RIO_RADAR_PDSTT.Address)
                {
                    if (_RIO_RADAR_PDSTT.GetUIntValue(e.Data) == 1)
                    {
                        _currentWCSmode = 4;
                        updateCDU();
                    }
                }
                if (e.Address == _RIO_RADAR_RWS.Address)
                {
                    if (_RIO_RADAR_RWS.GetUIntValue(e.Data) == 1)
                    {
                        _currentWCSmode = 5;
                        updateCDU();
                    }
                }
                if (e.Address == _RIO_RADAR_PSTT.Address)
                {
                    if (_RIO_RADAR_PSTT.GetUIntValue(e.Data) == 1)
                    {
                        _currentWCSmode = 6;
                        updateCDU();
                    }
                }


            }
            catch (Exception)
            {

            }
        }

        private void updateCDU()
        {
            SetLine(0, string.Format("{0,-5} {1,-9}{2,9}",  
                _hcuModes[_currentHCUMode] , 
                _cwsModes[_currentWCSmode] ,
                _categoryNames[_capPage]));

            // Display Columns for active CAP PAGE starting at line 2. 
            // Each Cap page is described with 2 string[5], 1st is the upper line and next is the lower 

            int currentLine = 2;
            
            for (int line = 0; line < 5; line++)
            {
                SetLine(currentLine++, 
                    string.Format("{0,24}", 
                        _categoryLabelPages[2*_capPage][line]
                        ));

                SetLine(currentLine++,
                    string.Format("{0,24}",
                        _categoryLabelPages[2 * _capPage + 1]
                        [line]));
            }

        }

        public  void DCSBIOSStringReceived(object sender, DCSBIOSStringDataEventArgs e)
        {
        }
    }

}
