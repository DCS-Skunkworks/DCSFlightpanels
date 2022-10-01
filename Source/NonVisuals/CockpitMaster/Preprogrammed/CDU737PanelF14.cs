using DCS_BIOS.EventArgs;
using DCS_BIOS;
using NonVisuals.CockpitMaster.Panels;
using System;
using DCS_BIOS.Interfaces;
using NonVisuals.CockpitMaster.Switches;
using System.Collections.Generic;

namespace NonVisuals.CockpitMaster.Preprogrammed
{
    public class CDU737PanelF14 : CDU737PanelBase , IDCSBIOSStringListener
    {
        // List the DCSBios Mappings Here

        private DCSBIOSOutput _RIO_CAP_CATEGORY;

        private bool _disposed;
        private int capPage = 0;

        private string[] _categoryNames = { "BIT", "SPL", "NAV", "TAC DATA", "D/L", "TGT DATA" };

        private string[][] _categoryLabelPages = new string[12][]
        {
            //              "-0123456789001234567890-","-0123456789001234567890-","-0123456789001234567890-","-0123456789001234567890-","-0123456789001234567890-"
            new string[5] { "-   disp        rcvr   -","-   cmptr       xmtr   -","- amcs conf    ant ir  -","- mas moat      stt    -","-   fault     spl test -"},
            new string[5] { "      1          5      ","      2          6      ","      3          7      ","      4          8      ","    disp        nbr     "},
            
            //              "-0123456789001234567890-","-0123456789001234567890-","-0123456789001234567890-","-0123456789001234567890-","-0123456789001234567890-"
            new string[5] { "-   home                ","-    ift        bit    -","-   ip          obc    -","-   gss         maint  -","-   air to      obc    -"},
            new string[5] { "  on heli               ","    menu    moving tgt  ","  to tgt        bit     ","                disp    ","    ground     displ    "},

            //              "-0123456789001234567890-","-0123456789001234567890-","-0123456789001234567890-","-0123456789001234567890-","-0123456789001234567890-"
            new string[5] { "-   own        tacan   -","- store hdg     rdr    -","- tarps nav     vis    -","-   wind        fix    -","-   tarps     mag var  -"},
            new string[5] { "    a/c         fix     ","    align       fix     ","     fix        fix     ","  spd hdg      enable   ","               (hdg)    "},

            
            //              "-0123456789001234567890-","-0123456789001234567890-","-0123456789001234567890-","-0123456789001234567890-","-0123456789001234567890-"
            new string[5] { "-  waypt        home   -","-  waypt         def   -","-  waypt        host   -","-   fix         surf   -","-    ip        pt to   -"},
            new string[5] { "     1          base    ","     2           pt     ","     3          area    ","     pt         tgt     ","                 pt     "},

            //              "-0123456789001234567890-","-0123456789001234567890-","-0123456789001234567890-","-0123456789001234567890-","-0123456789001234567890-"
            new string[5] { "-  wilco       point   -","-  cantco      engage  -","-   nav        flrp    -","-   tid        chaff   -","- f/f nav    f/f auto  -"},
            new string[5] { "                        ","                        ","    grid                ","    avia       count    ","  update       rstt     "},

            //              "-0123456789001234567890-","-0123456789001234567890-","-0123456789001234567890-","-0123456789001234567890-","-0123456789001234567890-"
            new string[5] { "-   gnd        friend  -","-  do not        unk   -","-  ift aux      host   -","-   data        mult   -","-   test         sym   -"},
            new string[5] { "    map                 ","   attack               ","   launch               ","    trans        tgt    ","    tgt       delete    "},

        };

        public CDU737PanelF14(HIDSkeleton hidSkeleton) : base(hidSkeleton)
        {
            CDUPanelKeys = CDUMappedCommandKeyF14.GetMappedPanelKeys();
            
            BIOSEventHandler.AttachStringListener(this);
            BIOSEventHandler.AttachDataListener(this);
            Startup();
        }

        public sealed override void Startup()
        {
            try
            {
                _RIO_CAP_CATEGORY = DCSBIOSControlLocator.GetDCSBIOSOutput("RIO_CAP_CATRGORY");

                BaseColor = CDUColor.WHITE;
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

                // RIO CAP CATEGROY
                if (e.Address == _RIO_CAP_CATEGORY.Address)
                {
                    capPage = (int)_RIO_CAP_CATEGORY.GetUIntValue(e.Data);
                    displayCapLabels();
                }

            }
            catch (Exception)
            {

            }
        }

        private void displayCapLabels()
        {
            SetLine(0, string.Format("{0,24}",  _categoryNames[capPage]));

            // Display Columns for active CAP PAGE starting at line 2. 
            // Each Cap page is described with 2 string[5], 1st is the upper line and next is the lower 

            int currentLine = 2;
            
            for (int line = 0; line < 5; line++)
            {
                SetLine(currentLine++, 
                    string.Format("{0,24}", 
                        _categoryLabelPages[2*capPage][line]
                        ));

                SetLine(currentLine++,
                    string.Format("{0,24}",
                        _categoryLabelPages[2 * capPage + 1]
                        [line]));
            }

        }

        public  void DCSBIOSStringReceived(object sender, DCSBIOSStringDataEventArgs e)
        {
        }
    }

}
