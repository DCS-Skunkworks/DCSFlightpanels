using NonVisuals.BindingClasses.BIP;

namespace NonVisuals.Radios
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Threading;
    using ClassLibraryCommon;
    using DCS_BIOS;
    using DCS_BIOS.EventArgs;
    using DCS_BIOS.Interfaces;

    using MEF;
    using Plugin;
    using Knobs;
    using Saitek;


    /*
     * Pre-programmed radio panel for the Mi24.
     */
    public class RadioPanelPZ69Mi24P : RadioPanelPZ69Base, IDCSBIOSStringListener
    {
        private enum CurrentMi24PRadioMode
        {
            R863_MANUAL,
            R863_PRESET,
            YADRO1A,
            R828_PRESETS,
            ADF_ARK15_HIGH,
            DME_ARK15_LOW,
            SPU8,
            NOUSE
        }

        private CurrentMi24PRadioMode _currentUpperRadioMode = CurrentMi24PRadioMode.R863_MANUAL;
        private CurrentMi24PRadioMode _currentLowerRadioMode = CurrentMi24PRadioMode.R863_MANUAL;
        const int CHANGE_VALUE = 10;

        /*Mi-24P VHF/UHF R-863 PRESETS COM2*/
        //Large dial 1-10 [step of 1]
        //Small dial volume control
        private readonly object _lockR863Preset1DialObject1 = new();
        private DCSBIOSOutput _r863Preset1DcsbiosOutputPresetDial;
        private volatile uint _r863PresetCockpitDialPos = 0;
        private const string R863_PRESET_COMMAND_INC = "PLT_R863_CHAN INC\n";
        private const string R863_PRESET_COMMAND_DEC = "PLT_R863_CHAN DEC\n";
        private int _r863PresetDialSkipper;
        private const string R863_PRESET_VOLUME_KNOB_COMMAND_INC = "PLT_R863_VOL +2500\n";
        private const string R863_PRESET_VOLUME_KNOB_COMMAND_DEC = "PLT_R863_VOL -2500\n";


        /*Mi-24P YaDRO 1A NAV1*/
        //Large dial 100-149  -> 20 - 179 [step of 1]
        //Small dial 0 - 99
        private readonly ClickSpeedDetector _yadro1ABigFreqIncreaseChangeMonitor = new(20);
        private readonly ClickSpeedDetector _yadro1ABigFreqDecreaseChangeMonitor = new(20);
        private uint _yadro1ABigFrequencyStandby = 100;
        private uint _yadro1ASmallFrequencyStandby;
        private volatile uint _yadro1ASavedCockpitBigFrequency;
        private volatile uint _yadro1ASavedCockpitSmallFrequency;
        private readonly object _lockYadro1ADialsObject1 = new();
        private readonly object _lockYadro1ADialsObject2 = new();
        private readonly object _lockYadro1ADialsObject3 = new();
        private readonly object _lockYadro1ADialsObject4 = new();
        private volatile uint _yadro1ACockpitFreq1DialPos = 1;
        private volatile uint _yadro1ACockpitFreq2DialPos = 1;
        private volatile uint _yadro1ACockpitFreq3DialPos = 1;
        private volatile uint _yadro1ACockpitFreq4DialPos = 1;
        private double _yadro1ACockpitFrequency = 100;
        private DCSBIOSOutput _yadro1ADcsbiosOutputCockpitFrequency;
        private const string YADRO1_A_FREQ_1DIAL_COMMAND = "PLT_JADRO_1M ";
        private const string YADRO1_A_FREQ_2DIAL_COMMAND = "PLT_JADRO_100K ";
        private const string YADRO1_A_FREQ_3DIAL_COMMAND = "PLT_JADRO_10K ";
        private const string YADRO1_A_FREQ_4DIAL_COMMAND = "PLT_JADRO_1K ";
        private Thread _yadro1ASyncThread;
        private long _yadro1AThreadNowSynching;
        private long _yadro1ADial1WaitingForFeedback;
        private long _yadro1ADial2WaitingForFeedback;
        private long _yadro1ADial3WaitingForFeedback;
        private long _yadro1ADial4WaitingForFeedback;

        /*Mi-24P R-828 FM Radio PRESETS NAV2*/
        //Large dial 1-10 [step of 1]
        //Small dial volume control
        //ACT/STBY AGC, automatic gain control
        private readonly object _lockR828Preset1DialObject1 = new();
        private DCSBIOSOutput _r828Preset1DcsbiosOutputDial;
        private volatile uint _r828PresetCockpitDialPos = 0;
        private const string R828_PRESET_COMMAND_INC = "PLT_R828_CHAN INC\n";
        private const string R828_PRESET_COMMAND_DEC = "PLT_R828_CHAN DEC\n";
        private int _r828PresetDialSkipper;
        private const string R828_PRESET_VOLUME_KNOB_COMMAND_INC = "PLT_R828_VOL +2500\n";
        private const string R828_PRESET_VOLUME_KNOB_COMMAND_DEC = "PLT_R828_VOL -2500\n";


        /*Mi-24P ARK-15 ADF*/
        //Large 100KHz 0 -> 17
        //Medium 10Khz 0 -> 9 (10 steps)
        private readonly object _lockADFMainDialObject1 = new();
        private readonly object _lockADFMainDialObject2 = new();
        private DCSBIOSOutput _adfMainDcsbiosOutputPresetDial1;
        private DCSBIOSOutput _adfMainDcsbiosOutputPresetDial2;
        private volatile uint _adfMainCockpitPresetDial1Pos = 0;
        private volatile uint _adfMainCockpitPresetDial2Pos = 0;
        private const string ADF_MAIN100_KHZ_PRESET_COMMAND_INC = "PLT_ARC_FREQ_L_100 INC\n";
        private const string ADF_MAIN100_KHZ_PRESET_COMMAND_DEC = "PLT_ARC_FREQ_L_100 DEC\n";
        private const string ADF_MAIN10_KHZ_PRESET_COMMAND_INC = "PLT_ARC_FREQ_L_10 INC\n";
        private const string ADF_MAIN10_KHZ_PRESET_COMMAND_DEC = "PLT_ARC_FREQ_L_10 DEC\n";
        private string _ark15_HighFrequency = string.Empty;
        private string _ark15_LowFrequency = string.Empty;
        /* ARK-15 ADF BACKUP */
        private readonly object _lockADFBackupDialObject1 = new();
        private readonly object _lockADFBackupDialObject2 = new();
        private DCSBIOSOutput _adfBackupDcsbiosOutputPresetDial1;
        private DCSBIOSOutput _adfBackupDcsbiosOutputPresetDial2;
        private volatile uint _adfBackupCockpitPresetDial1Pos = 0;
        private volatile uint _adfBackupCockpitPresetDial2Pos = 0;
        private const string ADF_BACKUP100_KHZ_PRESET_COMMAND_INC = "PLT_ARC_FREQ_R_100 INC\n";
        private const string ADF_BACKUP100_KHZ_PRESET_COMMAND_DEC = "PLT_ARC_FREQ_R_100 DEC\n";
        private const string ADF_BACKUP10_KHZ_PRESET_COMMAND_INC = "PLT_ARC_FREQ_R_10 INC\n";
        private const string ADF_BACKUP10_KHZ_PRESET_COMMAND_DEC = "PLT_ARC_FREQ_R_10 DEC\n";
        private int _adfPresetDial1Skipper;
        private int _adfPresetDial2Skipper;
        

        //0 = Backup ADF
        //1 = Main ADF
        private readonly object _lockADFBackupMainDialObject = new();
        private DCSBIOSOutput _adfBackupMainDcsbiosOutputPresetDial;
        private volatile uint _adfBackupMainCockpitDial1Pos = 0;
        private const string ADF_BACKUP_MAIN_SWITCH_TOGGLE_COMMAND = "PLT_ARC_CHAN TOGGLE\n";

        /*Mi-24P ARK-15 1KHz DME MAIN*/
        //Large 1Khz 0 -> 19 (20 steps)
        private readonly object _lockDMEMainDialObject1 = new();
        private DCSBIOSOutput _dmeMainDcsbiosOutputPresetDial1;
        private volatile uint _dmeMainCockpitPresetDial1Pos = 0;
        private const string DME_MAIN1_KHZ_PRESET_COMMAND_INC = "PLT_ARC_FREQ_L_1 INC\n";
        private const string DME_MAIN1_KHZ_PRESET_COMMAND_DEC = "PLT_ARC_FREQ_L_1 DEC\n";
        private int _dmePresetDial1Skipper;

        /*Mi-24P ARK-15 1KHz DME BACKUP*/
        private readonly object _lockDMEBackupDialObject1 = new();
        private DCSBIOSOutput _dmeBackupDcsbiosOutputPresetDial1;
        private volatile uint _dmeBackupCockpitPresetDial1Pos = 0;
        private const string DME_BACKUP1_KHZ_PRESET_COMMAND_INC = "PLT_ARC_FREQ_R_1 INC\n";
        private const string DME_BACKUP1_KHZ_PRESET_COMMAND_DEC = "PLT_ARC_FREQ_R_1 DEC\n";
        
       
        /*Mi-24P SPU-8 XPDR*/
        //Large dial 0-5 [step of 1]
        //Small dial volume control
        //ACT/STBY Toggle Radio/ICS Switch
        private readonly object _lockSpu8DialObject1 = new();
        private DCSBIOSOutput _spu8DcsbiosOutputPresetDial;
        private volatile uint _spu8CockpitDialPos = 0;
        private int _spu8DialSkipper;
        private const string SPU8_COMMAND_INC = "PLT_SPU8_MODE INC\n";
        private const string SPU8_COMMAND_DEC = "PLT_SPU8_MODE DEC\n";
        private const string SPU8_VOLUME_KNOB_COMMAND_INC = "PLT_SPU8_RADIO_VOL +2500\n";
        private const string SPU8_VOLUME_KNOB_COMMAND_DEC = "PLT_SPU8_RADIO_VOL -2500\n";
        private readonly object _lockSpu8ICSSwitchObject = new();
        private DCSBIOSOutput _spu8ICSSwitchDcsbiosOutput;
        private volatile uint _spu8ICSSwitchCockpitDialPos = 0;
        private const string SPU8_ICS_SWITCH_TOGGLE_COMMAND = "PLT_SPU8_ICS TOGGLE\n";

        private readonly object _lockShowFrequenciesOnPanelObject = new();
        private long _doUpdatePanelLCD;

        public RadioPanelPZ69Mi24P(HIDSkeleton hidSkeleton) : base(hidSkeleton)
        {
            CreateRadioKnobs();
            Startup();
            BIOSEventHandler.AttachStringListener(this);
            BIOSEventHandler.AttachDataListener(this);
        }

        private bool _disposed;
        // Protected implementation of Dispose pattern.
        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _shutdownYaDRO1AThread = true;
                    BIOSEventHandler.DetachStringListener(this);
                    BIOSEventHandler.DetachDataListener(this);
                }
                _disposed = true;
            }

            // Call base class implementation.
            base.Dispose(disposing);
        }

        public void DCSBIOSStringReceived(object sender, DCSBIOSStringDataEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(e.StringData))
                {
                    return;
                }

                if (e.Address.Equals(_yadro1ADcsbiosOutputCockpitFrequency.Address))
                {
                    // "02000.0" - "17999.9"
                    // Last digit not used in panel
                    double tmpFreq = double.Parse(e.StringData, NumberFormatInfoFullDisplay);
                    if (!tmpFreq.Equals(_yadro1ACockpitFrequency))
                    {
                        Interlocked.Increment(ref _doUpdatePanelLCD);
                    }
                    if (tmpFreq.Equals(_yadro1ACockpitFrequency))
                    {
                        //No need to process same data over and over
                        return;
                    }
                    _yadro1ACockpitFrequency = tmpFreq;
                    lock (_lockYadro1ADialsObject1)
                    {
                        // "02000.0" - "*17*999.9"
                        uint tmp = _yadro1ACockpitFreq1DialPos;
                        _yadro1ACockpitFreq1DialPos = uint.Parse(e.StringData.Substring(0, 2));
                        if (tmp != _yadro1ACockpitFreq1DialPos)
                        {
                            Interlocked.Increment(ref _doUpdatePanelLCD);
                            Interlocked.Exchange(ref _yadro1ADial1WaitingForFeedback, 0);
                        }
                    }
                    lock (_lockYadro1ADialsObject2)
                    {
                        // "02000.0" - "17*9*99.9"  
                        uint tmp = _yadro1ACockpitFreq2DialPos;
                        _yadro1ACockpitFreq2DialPos = uint.Parse(e.StringData.Substring(2, 1));
                        if (tmp != _yadro1ACockpitFreq2DialPos)
                        {
                            Interlocked.Increment(ref _doUpdatePanelLCD);
                            Interlocked.Exchange(ref _yadro1ADial2WaitingForFeedback, 0);
                        }
                    }
                    lock (_lockYadro1ADialsObject3)
                    {
                        // "02000.0" - "179*9*9.9"  
                        uint tmp = _yadro1ACockpitFreq3DialPos;
                        _yadro1ACockpitFreq3DialPos = uint.Parse(e.StringData.Substring(3, 1));
                        if (tmp != _yadro1ACockpitFreq3DialPos)
                        {
                            Interlocked.Increment(ref _doUpdatePanelLCD);
                            Interlocked.Exchange(ref _yadro1ADial3WaitingForFeedback, 0);
                        }
                    }
                    lock (_lockYadro1ADialsObject4)
                    {
                        // "02000.0" - "1799*9*.9"  
                        uint tmp = _yadro1ACockpitFreq4DialPos;
                        _yadro1ACockpitFreq4DialPos = uint.Parse(e.StringData.Substring(4, 1));
                        if (tmp != _yadro1ACockpitFreq4DialPos)
                        {
                            Interlocked.Increment(ref _doUpdatePanelLCD);
                            Interlocked.Exchange(ref _yadro1ADial4WaitingForFeedback, 0);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex, "DCSBIOSStringReceived()");
            }
            ShowFrequenciesOnPanel();
        }

        public override void DcsBiosDataReceived(object sender, DCSBIOSDataEventArgs e)
        {
            try
            {
                UpdateCounter(e.Address, e.Data);

                /*
                 * IMPORTANT INFORMATION REGARDING THE _*WaitingForFeedback variables
                 * Once a dial has been deemed to be "off" position and needs to be changed
                 * a change command is sent to DCS-BIOS.
                 * Only after a *change* has been acknowledged will the _*WaitingForFeedback be
                 * reset. Reading the dial's position with no change in value will not reset.
                 */

                //R-863 Preset Channel Dial
                if (e.Address == _r863Preset1DcsbiosOutputPresetDial.Address)
                {
                    lock (_lockR863Preset1DialObject1)
                    {
                        var tmp = _r863PresetCockpitDialPos;
                        _r863PresetCockpitDialPos = _r863Preset1DcsbiosOutputPresetDial.GetUIntValue(e.Data);
                        if (tmp != _r863PresetCockpitDialPos)
                        {
                            Interlocked.Increment(ref _doUpdatePanelLCD);
                        }
                    }
                }

                //YaDRO-1A
                //R-828 Preset Channel Dial
                if (e.Address == _r828Preset1DcsbiosOutputDial.Address)
                {
                    lock (_lockR828Preset1DialObject1)
                    {
                        var tmp = _r828PresetCockpitDialPos;
                        _r828PresetCockpitDialPos = _r828Preset1DcsbiosOutputDial.GetUIntValue(e.Data);
                        if (tmp != _r828PresetCockpitDialPos)
                        {
                            Interlocked.Increment(ref _doUpdatePanelLCD);
                        }
                    }
                }

                //ADF Main Preset Dial 1
                if (e.Address == _adfMainDcsbiosOutputPresetDial1.Address)
                {
                    lock (_lockADFMainDialObject1)
                    {
                        var tmp = _adfMainCockpitPresetDial1Pos;
                        _adfMainCockpitPresetDial1Pos = _adfMainDcsbiosOutputPresetDial1.GetUIntValue(e.Data);
                        if (tmp != _adfMainCockpitPresetDial1Pos)
                        {
                            Interlocked.Increment(ref _doUpdatePanelLCD);
                        }
                    }
                }

                //ADF Main Preset Dial 2
                if (e.Address == _adfMainDcsbiosOutputPresetDial2.Address)
                {
                    lock (_lockADFMainDialObject2)
                    {
                        var tmp = _adfMainCockpitPresetDial2Pos;
                        _adfMainCockpitPresetDial2Pos = _adfMainDcsbiosOutputPresetDial2.GetUIntValue(e.Data);
                        if (tmp != _adfMainCockpitPresetDial2Pos)
                        {
                            Interlocked.Increment(ref _doUpdatePanelLCD);
                        }
                    }
                }

                //ADF Backup Preset Dial 1
                if (e.Address == _adfBackupDcsbiosOutputPresetDial1.Address)
                {
                    lock (_lockADFBackupDialObject1)
                    {
                        var tmp = _adfBackupCockpitPresetDial1Pos;
                        _adfBackupCockpitPresetDial1Pos = _adfBackupDcsbiosOutputPresetDial1.GetUIntValue(e.Data);
                        if (tmp != _adfBackupCockpitPresetDial1Pos)
                        {
                            Interlocked.Increment(ref _doUpdatePanelLCD);
                        }
                    }
                }

                //ADF Backup Preset Dial 2
                if (e.Address == _adfBackupDcsbiosOutputPresetDial2.Address)
                {
                    lock (_lockADFBackupDialObject2)
                    {
                        var tmp = _adfBackupCockpitPresetDial2Pos;
                        _adfBackupCockpitPresetDial2Pos = _adfBackupDcsbiosOutputPresetDial2.GetUIntValue(e.Data);
                        if (tmp != _adfBackupCockpitPresetDial2Pos)
                        {
                            Interlocked.Increment(ref _doUpdatePanelLCD);
                        }
                    }
                }

                //ADF Backup or Main
                if (e.Address == _adfBackupMainDcsbiosOutputPresetDial.Address)
                {
                    lock (_lockADFBackupMainDialObject)
                    {
                        var tmp = _adfBackupMainCockpitDial1Pos;
                        _adfBackupMainCockpitDial1Pos = _adfBackupMainDcsbiosOutputPresetDial.GetUIntValue(e.Data);
                        if (tmp != _adfBackupMainCockpitDial1Pos)
                        {
                            Interlocked.Increment(ref _doUpdatePanelLCD);
                        }
                    }
                }

                //DME Main Preset Dial 1
                if (e.Address == _dmeMainDcsbiosOutputPresetDial1.Address)
                {
                    lock (_lockDMEMainDialObject1)
                    {
                        var tmp = _dmeMainCockpitPresetDial1Pos;
                        _dmeMainCockpitPresetDial1Pos = _dmeMainDcsbiosOutputPresetDial1.GetUIntValue(e.Data);
                        if (tmp != _dmeMainCockpitPresetDial1Pos)
                        {
                            Interlocked.Increment(ref _doUpdatePanelLCD);
                        }
                    }
                }

                //DME Backup Preset Dial 1
                if (e.Address == _dmeBackupDcsbiosOutputPresetDial1.Address)
                {
                    lock (_lockDMEBackupDialObject1)
                    {
                        var tmp = _dmeBackupCockpitPresetDial1Pos;
                        _dmeBackupCockpitPresetDial1Pos = _dmeBackupDcsbiosOutputPresetDial1.GetUIntValue(e.Data);
                        if (tmp != _dmeBackupCockpitPresetDial1Pos)
                        {
                            Interlocked.Increment(ref _doUpdatePanelLCD);
                        }
                    }
                }

                //SPU-8 Dial
                if (e.Address == _spu8DcsbiosOutputPresetDial.Address)
                {
                    lock (_lockSpu8DialObject1)
                    {
                        var tmp = _spu8CockpitDialPos;
                        _spu8CockpitDialPos = _spu8DcsbiosOutputPresetDial.GetUIntValue(e.Data);
                        if (tmp != _spu8CockpitDialPos)
                        {
                            Interlocked.Increment(ref _doUpdatePanelLCD);
                        }
                    }
                }

                //SPU-8 Radio/ICS
                if (e.Address == _spu8ICSSwitchDcsbiosOutput.Address)
                {
                    lock (_lockSpu8ICSSwitchObject)
                    {
                        var tmp = _spu8ICSSwitchCockpitDialPos;
                        _spu8ICSSwitchCockpitDialPos = _spu8ICSSwitchDcsbiosOutput.GetUIntValue(e.Data);
                        if (tmp != _spu8ICSSwitchCockpitDialPos)
                        {
                            Interlocked.Increment(ref _doUpdatePanelLCD);
                        }
                    }
                }

                //Set once
                DataHasBeenReceivedFromDCSBIOS = true;
                ShowFrequenciesOnPanel();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void SendFrequencyToDCSBIOS(bool knobIsOn, RadioPanelPZ69KnobsMi24P knob)
        {
            try
            {
                if (IgnoreSwitchButtonOnce() && (knob == RadioPanelPZ69KnobsMi24P.UPPER_FREQ_SWITCH || knob == RadioPanelPZ69KnobsMi24P.LOWER_FREQ_SWITCH))
                {
                    //Don't do anything on the very first button press as the panel sends ALL
                    //switches when it is manipulated the first time
                    //This would cause unintended sync.
                    return;
                }
                if (!DataHasBeenReceivedFromDCSBIOS)
                {
                    //Don't start communication with DCS-BIOS before we have had a first contact from "them"
                    return;
                }
                switch (knob)
                {
                    case RadioPanelPZ69KnobsMi24P.UPPER_FREQ_SWITCH:
                        {
                            switch (_currentUpperRadioMode)
                            {
                                case CurrentMi24PRadioMode.R863_PRESET:
                                    {
                                        break;
                                    }
                                case CurrentMi24PRadioMode.R863_MANUAL:
                                    {
                                        break;
                                    }
                                case CurrentMi24PRadioMode.YADRO1A:
                                    {
                                        SendYaDRO1AToDCSBIOS();
                                        break;
                                    }
                                case CurrentMi24PRadioMode.ADF_ARK15_HIGH:
                                    {
                                        break;
                                    }
                                case CurrentMi24PRadioMode.SPU8:
                                    {
                                        if (knobIsOn)
                                        {
                                            DCSBIOS.Send(SPU8_ICS_SWITCH_TOGGLE_COMMAND);
                                        }
                                        break;
                                    }
                            }
                            break;
                        }
                    case RadioPanelPZ69KnobsMi24P.LOWER_FREQ_SWITCH:
                        {
                            switch (_currentLowerRadioMode)
                            {
                                case CurrentMi24PRadioMode.R863_PRESET:
                                    {
                                        break;
                                    }
                                case CurrentMi24PRadioMode.R863_MANUAL:
                                    {
                                        break;
                                    }
                                case CurrentMi24PRadioMode.YADRO1A:
                                    {
                                        SendYaDRO1AToDCSBIOS();
                                        break;
                                    }
                                case CurrentMi24PRadioMode.ADF_ARK15_HIGH:
                                    {
                                        break;
                                    }
                                case CurrentMi24PRadioMode.SPU8:
                                    {
                                        if (knobIsOn)
                                        {
                                            DCSBIOS.Send(SPU8_ICS_SWITCH_TOGGLE_COMMAND);
                                        }
                                        break;
                                    }
                            }
                            break;
                        }
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void SendYaDRO1AToDCSBIOS()
        {
            try
            {
                if (YaDRO1ANowSyncing())
                {
                    return;
                }
                SaveCockpitFrequencyYaDRO1A();

                _shutdownYaDRO1AThread = true;
                Thread.Sleep(Constants.ThreadShutDownWaitTime);
                _shutdownYaDRO1AThread = false;
                _yadro1ASyncThread = new Thread(() => YaDRO1ASynchThreadMethod());
                _yadro1ASyncThread.Start();
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
        }

        private volatile bool _shutdownYaDRO1AThread;
        private void YaDRO1ASynchThreadMethod()
        {
            try
            {
                try
                {
                    try
                    {
                        // Mi-24P YaDRO-1A
                        Interlocked.Exchange(ref _yadro1AThreadNowSynching, 1);
                        long dial1Timeout = DateTime.Now.Ticks;
                        long dial2Timeout = DateTime.Now.Ticks;
                        long dial3Timeout = DateTime.Now.Ticks;
                        long dial4Timeout = DateTime.Now.Ticks;
                        long dial1OkTime = 0;
                        long dial2OkTime = 0;
                        long dial3OkTime = 0;
                        long dial4OkTime = 0;

                        var frequencyAsString = _yadro1ABigFrequencyStandby.ToString().PadLeft(3, '0') + _yadro1ASmallFrequencyStandby.ToString().PadLeft(2, '0');
                        frequencyAsString = frequencyAsString.PadRight(6, '0');
                        //Frequency selector 1      YADRO1A_FREQ1
                        //      "02" "03" "04" "05" "06" "07" "08" "09" "10" "11" "12" "13" "14" "15" "16" "17"
                        //Pos     0    1    2    3    4    5    6    7    8    9   10   11   12   13   14   15

                        //Frequency selector 2      YADRO1A_FREQ2
                        //0 1 2 3 4 5 6 7 8 9

                        //Frequency selector 3      YADRO1A_FREQ3
                        //0 1 2 3 4 5 6 7 8 9

                        //Frequency selector 4      YADRO1A_FREQ4
                        //0 1 2 3 4 5 6 7 8 9

                        //Reason for this is to separate the standby frequency from the sync loop
                        //If not the sync would pick up any changes made by the user during the
                        //sync process
                        var desiredPositionDial1X = 0;
                        var desiredPositionDial2X = 0;
                        var desiredPositionDial3X = 0;
                        var desiredPositionDial4X = 0;

                        //02000
                        //17999
                        //#1 = 17  (position = value)
                        //#2 = 9   (position = value)
                        //#3 = 9   (position = value)
                        //#4 = 9   (position = value)
                        desiredPositionDial1X = int.Parse(frequencyAsString.Substring(0, 2));
                        desiredPositionDial2X = int.Parse(frequencyAsString.Substring(2, 1));
                        desiredPositionDial3X = int.Parse(frequencyAsString.Substring(3, 1));
                        desiredPositionDial4X = int.Parse(frequencyAsString.Substring(4, 1));

                        do
                        {
                            if (IsTimedOut(ref dial1Timeout))
                            {
                                ResetWaitingForFeedBack(ref _yadro1ADial1WaitingForFeedback); // Lets do an ugly reset
                            }
                            if (IsTimedOut(ref dial2Timeout))
                            {
                                //Lets do an ugly reset
                                ResetWaitingForFeedBack(ref _yadro1ADial2WaitingForFeedback); // Lets do an ugly reset
                            }
                            if (IsTimedOut(ref dial3Timeout))
                            {
                                //Lets do an ugly reset
                                ResetWaitingForFeedBack(ref _yadro1ADial3WaitingForFeedback); // Lets do an ugly reset
                            }
                            if (IsTimedOut(ref dial4Timeout))
                            {
                                //Lets do an ugly reset
                                ResetWaitingForFeedBack(ref _yadro1ADial4WaitingForFeedback); // Lets do an ugly reset
                            }

                            string str;
                            if (Interlocked.Read(ref _yadro1ADial1WaitingForFeedback) == 0)
                            {
                                lock (_lockYadro1ADialsObject1)
                                {
                                    if (_yadro1ACockpitFreq1DialPos != desiredPositionDial1X)
                                    {
                                        dial1OkTime = DateTime.Now.Ticks;
                                        if (_yadro1ACockpitFreq1DialPos < desiredPositionDial1X)
                                        {
                                            str = YADRO1_A_FREQ_1DIAL_COMMAND + "INC\n";
                                        }
                                        else
                                        {
                                            str = YADRO1_A_FREQ_1DIAL_COMMAND + "DEC\n";
                                        }
                                        DCSBIOS.Send(str);
                                        Interlocked.Exchange(ref _yadro1ADial1WaitingForFeedback, 1);
                                    }
                                    Reset(ref dial1Timeout);
                                }
                            }
                            else
                            {
                                dial1OkTime = DateTime.Now.Ticks;
                            }
                            if (Interlocked.Read(ref _yadro1ADial2WaitingForFeedback) == 0)
                            {
                                lock (_lockYadro1ADialsObject2)
                                {
                                    if (_yadro1ACockpitFreq2DialPos != desiredPositionDial2X)
                                    {
                                        dial2OkTime = DateTime.Now.Ticks;
                                        str = YADRO1_A_FREQ_2DIAL_COMMAND + GetCommandDirectionFor0To9Dials(desiredPositionDial2X, _yadro1ACockpitFreq2DialPos);
                                        DCSBIOS.Send(str);
                                        Interlocked.Exchange(ref _yadro1ADial2WaitingForFeedback, 1);
                                    }
                                    Reset(ref dial2Timeout);
                                }
                            }
                            else
                            {
                                dial2OkTime = DateTime.Now.Ticks;
                            }
                            if (Interlocked.Read(ref _yadro1ADial3WaitingForFeedback) == 0)
                            {
                                lock (_lockYadro1ADialsObject3)
                                {
                                    if (_yadro1ACockpitFreq3DialPos != desiredPositionDial3X)
                                    {
                                        dial3OkTime = DateTime.Now.Ticks;
                                        str = YADRO1_A_FREQ_3DIAL_COMMAND + GetCommandDirectionFor0To9Dials(desiredPositionDial3X, _yadro1ACockpitFreq3DialPos);
                                        DCSBIOS.Send(str);
                                        Interlocked.Exchange(ref _yadro1ADial3WaitingForFeedback, 1);
                                    }
                                    Reset(ref dial3Timeout);
                                }
                            }
                            else
                            {
                                dial3OkTime = DateTime.Now.Ticks;
                            }
                            if (Interlocked.Read(ref _yadro1ADial4WaitingForFeedback) == 0)
                            {
                                lock (_lockYadro1ADialsObject4)
                                {
                                    if (_yadro1ACockpitFreq4DialPos != desiredPositionDial4X)
                                    {
                                        dial4OkTime = DateTime.Now.Ticks;
                                        str = YADRO1_A_FREQ_4DIAL_COMMAND + GetCommandDirectionFor0To9Dials(desiredPositionDial4X, _yadro1ACockpitFreq4DialPos);
                                        DCSBIOS.Send(str);
                                        Interlocked.Exchange(ref _yadro1ADial4WaitingForFeedback, 1);
                                    }
                                    Reset(ref dial4Timeout);
                                }
                            }
                            else
                            {
                                dial4OkTime = DateTime.Now.Ticks;
                            }
                            Thread.Sleep(SynchSleepTime); //Should be enough to get an update cycle from DCS-BIOS
                        }
                        while ((IsTooShort(dial1OkTime) || IsTooShort(dial2OkTime) || IsTooShort(dial3OkTime) || IsTooShort(dial4OkTime)) && !_shutdownYaDRO1AThread);
                        SwapCockpitStandbyFrequencyYaDRO1A();
                        ShowFrequenciesOnPanel();
                    }
                    catch (ThreadAbortException)
                    { }
                    catch (Exception ex)
                    {
                        Common.ShowErrorMessageBox(ex);
                    }
                }
                finally
                {
                    Interlocked.Exchange(ref _yadro1AThreadNowSynching, 0);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
            //Refresh panel once this debacle is finished
            Interlocked.Increment(ref _doUpdatePanelLCD);
        }

        private void SwapCockpitStandbyFrequencyYaDRO1A()
        {
            try
            {
                _yadro1ABigFrequencyStandby = _yadro1ASavedCockpitBigFrequency;
                _yadro1ASmallFrequencyStandby = _yadro1ASavedCockpitSmallFrequency;
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
        }

        public void PZ69KnobChanged(bool isFirstReport, IEnumerable<object> hashSet)
        {
            if (isFirstReport)
            {
                return;
            }

            try
            {
                Interlocked.Increment(ref _doUpdatePanelLCD);
                lock (LockLCDUpdateObject)
                {
                    foreach (var radioPanelKnobObject in hashSet)
                    {
                        var radioPanelKnob = (RadioPanelKnobMi24P)radioPanelKnobObject;

                        switch (radioPanelKnob.RadioPanelPZ69Knob)
                        {
                            case RadioPanelPZ69KnobsMi24P.UPPER_R863_MANUAL:
                                    SetUpperRadioMode(radioPanelKnob.IsOn, CurrentMi24PRadioMode.R863_MANUAL);
                                break;
               
                            case RadioPanelPZ69KnobsMi24P.UPPER_R863_PRESET:
                                    SetUpperRadioMode(radioPanelKnob.IsOn, CurrentMi24PRadioMode.R863_PRESET);
                                break;

                            case RadioPanelPZ69KnobsMi24P.UPPER_YADRO1A:
                                    SetUpperRadioMode(radioPanelKnob.IsOn, CurrentMi24PRadioMode.YADRO1A);
                                break;

                            case RadioPanelPZ69KnobsMi24P.UPPER_R828:
                                    SetUpperRadioMode(radioPanelKnob.IsOn, CurrentMi24PRadioMode.R828_PRESETS);
                                break;

                            case RadioPanelPZ69KnobsMi24P.UPPER_ADF_ARK15:
                                    SetUpperRadioMode(radioPanelKnob.IsOn, CurrentMi24PRadioMode.ADF_ARK15_HIGH);
                                break;

                            case RadioPanelPZ69KnobsMi24P.UPPER_SPU8:
                                    SetUpperRadioMode(radioPanelKnob.IsOn, CurrentMi24PRadioMode.SPU8);
                                break;

                            case RadioPanelPZ69KnobsMi24P.LOWER_R863_MANUAL:
                                    SetLowerRadioMode(radioPanelKnob.IsOn, CurrentMi24PRadioMode.R863_MANUAL);
                                break;

                            case RadioPanelPZ69KnobsMi24P.LOWER_R863_PRESET:
                                    SetLowerRadioMode(radioPanelKnob.IsOn, CurrentMi24PRadioMode.R863_PRESET);
                                break;
                            case RadioPanelPZ69KnobsMi24P.LOWER_YADRO1A:
                                    SetLowerRadioMode(radioPanelKnob.IsOn, CurrentMi24PRadioMode.YADRO1A);
                                break;

                            case RadioPanelPZ69KnobsMi24P.LOWER_R828:
                                    SetLowerRadioMode(radioPanelKnob.IsOn, CurrentMi24PRadioMode.R828_PRESETS);
                                break;

                            case RadioPanelPZ69KnobsMi24P.LOWER_ADF_ARK15:
                                    SetLowerRadioMode(radioPanelKnob.IsOn, CurrentMi24PRadioMode.ADF_ARK15_HIGH);
                                break;

                            case RadioPanelPZ69KnobsMi24P.LOWER_SPU8:
                                    SetLowerRadioMode(radioPanelKnob.IsOn, CurrentMi24PRadioMode.SPU8);
                                break;

                            case RadioPanelPZ69KnobsMi24P.UPPER_ARK_UD:
                                   SetUpperRadioMode(radioPanelKnob.IsOn, CurrentMi24PRadioMode.DME_ARK15_LOW);
                                break;

                            case RadioPanelPZ69KnobsMi24P.LOWER_ARK_UD:
                                   SetLowerRadioMode(radioPanelKnob.IsOn, CurrentMi24PRadioMode.DME_ARK15_LOW);
                                break;
 
                            case RadioPanelPZ69KnobsMi24P.UPPER_LARGE_FREQ_WHEEL_INC:
                            case RadioPanelPZ69KnobsMi24P.UPPER_LARGE_FREQ_WHEEL_DEC:
                            case RadioPanelPZ69KnobsMi24P.UPPER_SMALL_FREQ_WHEEL_INC:
                            case RadioPanelPZ69KnobsMi24P.UPPER_SMALL_FREQ_WHEEL_DEC:
                            case RadioPanelPZ69KnobsMi24P.LOWER_LARGE_FREQ_WHEEL_INC:
                            case RadioPanelPZ69KnobsMi24P.LOWER_LARGE_FREQ_WHEEL_DEC:
                            case RadioPanelPZ69KnobsMi24P.LOWER_SMALL_FREQ_WHEEL_INC:
                            case RadioPanelPZ69KnobsMi24P.LOWER_SMALL_FREQ_WHEEL_DEC:
                                {
                                    //Ignore
                                    break;
                                }
                            case RadioPanelPZ69KnobsMi24P.UPPER_FREQ_SWITCH:
                                {
                                    if (_currentUpperRadioMode == CurrentMi24PRadioMode.R863_PRESET)
                                    {
                                    }
                                    else if (_currentUpperRadioMode == CurrentMi24PRadioMode.ADF_ARK15_HIGH && radioPanelKnob.IsOn)
                                    {
                                        DCSBIOS.Send(ADF_BACKUP_MAIN_SWITCH_TOGGLE_COMMAND);
                                    }
                                    else if (_currentUpperRadioMode == CurrentMi24PRadioMode.DME_ARK15_LOW && radioPanelKnob.IsOn)
                                    {
                                    }
                                    else
                                    {
                                        SendFrequencyToDCSBIOS(radioPanelKnob.IsOn, RadioPanelPZ69KnobsMi24P.UPPER_FREQ_SWITCH);
                                    }
                                    break;
                                }
                            case RadioPanelPZ69KnobsMi24P.LOWER_FREQ_SWITCH:
                                {
                                    if (_currentLowerRadioMode == CurrentMi24PRadioMode.R863_PRESET)
                                    {
                                    }
                                    else if (_currentLowerRadioMode == CurrentMi24PRadioMode.ADF_ARK15_HIGH && radioPanelKnob.IsOn)
                                    {
                                        DCSBIOS.Send(ADF_BACKUP_MAIN_SWITCH_TOGGLE_COMMAND);
                                    }
                                    else if (_currentLowerRadioMode == CurrentMi24PRadioMode.DME_ARK15_LOW && radioPanelKnob.IsOn)
                                    {
                                    }
                                    else
                                    {
                                        SendFrequencyToDCSBIOS(radioPanelKnob.IsOn, RadioPanelPZ69KnobsMi24P.LOWER_FREQ_SWITCH);
                                    }
                                    break;
                                }
                        }

                        if (PluginManager.PlugSupportActivated && PluginManager.HasPlugin())
                        {
                            PluginManager.DoEvent(DCSFPProfile.SelectedProfile.Description, HIDInstance, PluginGamingPanelEnum.PZ69RadioPanel_PreProg_MI24P, (int)radioPanelKnob.RadioPanelPZ69Knob, radioPanelKnob.IsOn, null);
                        }
                    }
                    AdjustFrequency(hashSet);
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void YADRO1A_BigFrequencyIncrease()
        {
            bool changeFaster = false;
            _yadro1ABigFreqIncreaseChangeMonitor.Click();
            if (_yadro1ABigFreqIncreaseChangeMonitor.ClickThresholdReached())
            {
                //Change faster
                changeFaster = true;
            }
            if (changeFaster)
            {
                _yadro1ABigFrequencyStandby += CHANGE_VALUE;
            }
            if (_yadro1ABigFrequencyStandby >= 179)
            {
                //@ max value
                _yadro1ABigFrequencyStandby = 179;
                return;
            }
            _yadro1ABigFrequencyStandby++;
        }

        private void YADRO1A_SmallFrequencyIncrease()
        {
            if (_yadro1ASmallFrequencyStandby >= 99)
            {
                //At max value
                _yadro1ASmallFrequencyStandby = 0;
                return;
            }
            _yadro1ASmallFrequencyStandby++;
        }

        private void YADRO1A_SmallFrequencyDecrease()
        {
            if (_yadro1ASmallFrequencyStandby <= 0)
            {
                //At min value
                _yadro1ASmallFrequencyStandby = 99;
                return;
            }
            _yadro1ASmallFrequencyStandby--;
        }

        private void YADRO1A_BigFrequencyDecrease()
        {
            bool changeFaster = false;
            _yadro1ABigFreqDecreaseChangeMonitor.Click();
            if (_yadro1ABigFreqDecreaseChangeMonitor.ClickThresholdReached())
            {
                //Change faster
                changeFaster = true;
            }
            if (changeFaster)
            {
                _yadro1ABigFrequencyStandby -= CHANGE_VALUE;
            }
            if (_yadro1ABigFrequencyStandby <= 20)
            {
                //@ max value
                _yadro1ABigFrequencyStandby = 20;
                return;
            }
            _yadro1ABigFrequencyStandby--;
        }

        private void AdjustFrequency(IEnumerable<object> hashSet)
        {
            try
            {
                if (SkipCurrentFrequencyChange())
                {
                    return;
                }

                foreach (var o in hashSet)
                {
                    var radioPanelKnobMi24P = (RadioPanelKnobMi24P)o;
                    if (!radioPanelKnobMi24P.IsOn)
                    {
                        continue;
                    }
                    switch (radioPanelKnobMi24P.RadioPanelPZ69Knob)
                    {
                        case RadioPanelPZ69KnobsMi24P.UPPER_LARGE_FREQ_WHEEL_INC:
                            {
                                switch (_currentUpperRadioMode)
                                {
                                    case CurrentMi24PRadioMode.R863_MANUAL:
                                        {
                                            break;
                                        }
                                    case CurrentMi24PRadioMode.R863_PRESET:
                                        {
                                            if (!SkipR863PresetDialChange())
                                            {
                                                DCSBIOS.Send(R863_PRESET_COMMAND_INC);
                                            }
                                            break;
                                        }
                                    case CurrentMi24PRadioMode.YADRO1A:
                                        {
                                            YADRO1A_BigFrequencyIncrease();
                                            break;
                                        }
                                    case CurrentMi24PRadioMode.R828_PRESETS:
                                        {
                                            if (!SkipR828PresetDialChange())
                                            {
                                                DCSBIOS.Send(R828_PRESET_COMMAND_INC);
                                            }
                                            break;
                                        }
                                    case CurrentMi24PRadioMode.ADF_ARK15_HIGH:
                                        {
                                            if (!SkipADFPresetDial1Change())
                                            {
                                                DCSBIOS.Send(_adfBackupMainCockpitDial1Pos == 1 ? ADF_MAIN100_KHZ_PRESET_COMMAND_INC : ADF_BACKUP100_KHZ_PRESET_COMMAND_INC);
                                            }
                                            break;
                                        }
                                    case CurrentMi24PRadioMode.DME_ARK15_LOW:
                                        {
                                            if (!SkipDMEPresetDial1Change())
                                            {
                                                DCSBIOS.Send(_adfBackupMainCockpitDial1Pos == 1 ? DME_MAIN1_KHZ_PRESET_COMMAND_INC : DME_BACKUP1_KHZ_PRESET_COMMAND_INC);
                                            }
                                            break;
                                        }
                                    case CurrentMi24PRadioMode.SPU8:
                                        {
                                            if (!SkipSpu8PresetDialChange())
                                            {
                                                DCSBIOS.Send(SPU8_COMMAND_INC);
                                            }
                                            break;
                                        }
                                    case CurrentMi24PRadioMode.NOUSE:
                                        {
                                            break;
                                        }
                                }
                                break;
                            }
                        case RadioPanelPZ69KnobsMi24P.UPPER_LARGE_FREQ_WHEEL_DEC:
                            {
                                switch (_currentUpperRadioMode)
                                {
                                    case CurrentMi24PRadioMode.R863_MANUAL:
                                        {
                                            break;
                                        }
                                    case CurrentMi24PRadioMode.R863_PRESET:
                                        {
                                            if (!SkipR863PresetDialChange())
                                            {
                                                DCSBIOS.Send(R863_PRESET_COMMAND_DEC);
                                            }
                                            break;
                                        }
                                    case CurrentMi24PRadioMode.YADRO1A:
                                        {
                                            YADRO1A_BigFrequencyDecrease();
                                            break;
                                        }
                                    case CurrentMi24PRadioMode.R828_PRESETS:
                                        {
                                            if (!SkipR828PresetDialChange())
                                            {
                                                DCSBIOS.Send(R828_PRESET_COMMAND_DEC);
                                            }
                                            break;
                                        }
                                    case CurrentMi24PRadioMode.ADF_ARK15_HIGH:
                                        {
                                            if (!SkipADFPresetDial1Change())
                                            {
                                                DCSBIOS.Send(_adfBackupMainCockpitDial1Pos == 1 ? ADF_MAIN100_KHZ_PRESET_COMMAND_DEC : ADF_BACKUP100_KHZ_PRESET_COMMAND_DEC);
                                            }
                                            break;
                                        }
                                    case CurrentMi24PRadioMode.DME_ARK15_LOW:
                                        {
                                            if (!SkipDMEPresetDial1Change())
                                            {
                                                DCSBIOS.Send(_adfBackupMainCockpitDial1Pos == 1 ? DME_MAIN1_KHZ_PRESET_COMMAND_DEC : DME_BACKUP1_KHZ_PRESET_COMMAND_DEC);
                                            }
                                            break;
                                        }
                                    case CurrentMi24PRadioMode.SPU8:
                                        {
                                            if (!SkipSpu8PresetDialChange())
                                            {
                                                DCSBIOS.Send(SPU8_COMMAND_DEC);
                                            }
                                            break;
                                        }
                                    case CurrentMi24PRadioMode.NOUSE:
                                        {
                                            break;
                                        }
                                }
                                break;
                            }
                        case RadioPanelPZ69KnobsMi24P.UPPER_SMALL_FREQ_WHEEL_INC:
                            {
                                switch (_currentUpperRadioMode)
                                {
                                    case CurrentMi24PRadioMode.R863_MANUAL:
                                        {
                                            break;
                                        }
                                    case CurrentMi24PRadioMode.R863_PRESET:
                                        {
                                            DCSBIOS.Send(R863_PRESET_VOLUME_KNOB_COMMAND_INC);
                                            break;
                                        }
                                    case CurrentMi24PRadioMode.YADRO1A:
                                        {
                                            YADRO1A_SmallFrequencyIncrease();
                                            break;
                                        }
                                    case CurrentMi24PRadioMode.R828_PRESETS:
                                        {
                                            DCSBIOS.Send(R828_PRESET_VOLUME_KNOB_COMMAND_INC);
                                            break;
                                        }
                                    case CurrentMi24PRadioMode.ADF_ARK15_HIGH:
                                        {
                                            if (!SkipADFPresetDial2Change())
                                            {
                                                DCSBIOS.Send(_adfBackupMainCockpitDial1Pos == 1 ? ADF_MAIN10_KHZ_PRESET_COMMAND_INC : ADF_BACKUP10_KHZ_PRESET_COMMAND_INC);
                                            }
                                            break;
                                        }
                                    case CurrentMi24PRadioMode.DME_ARK15_LOW:
                                        {
                                            break;
                                        }
                                    case CurrentMi24PRadioMode.SPU8:
                                        {
                                            DCSBIOS.Send(SPU8_VOLUME_KNOB_COMMAND_INC);
                                            break;
                                        }
                                    case CurrentMi24PRadioMode.NOUSE:
                                        {
                                            break;
                                        }
                                }
                                break;
                            }
                        case RadioPanelPZ69KnobsMi24P.UPPER_SMALL_FREQ_WHEEL_DEC:
                            {
                                switch (_currentUpperRadioMode)
                                {
                                    case CurrentMi24PRadioMode.R863_MANUAL:
                                        {
                                            break;
                                        }
                                    case CurrentMi24PRadioMode.R863_PRESET:
                                        {
                                            DCSBIOS.Send(R863_PRESET_VOLUME_KNOB_COMMAND_DEC);
                                            break;
                                        }
                                    case CurrentMi24PRadioMode.YADRO1A:
                                        {
                                            YADRO1A_SmallFrequencyDecrease();
                                            break;
                                        }
                                    case CurrentMi24PRadioMode.R828_PRESETS:
                                        {
                                            DCSBIOS.Send(R828_PRESET_VOLUME_KNOB_COMMAND_DEC);
                                            break;
                                        }
                                    case CurrentMi24PRadioMode.ADF_ARK15_HIGH:
                                        {
                                            if (!SkipADFPresetDial2Change())
                                            {
                                                DCSBIOS.Send(_adfBackupMainCockpitDial1Pos == 1 ? ADF_MAIN10_KHZ_PRESET_COMMAND_DEC : ADF_BACKUP10_KHZ_PRESET_COMMAND_DEC);
                                            }
                                            break;
                                        }
                                    case CurrentMi24PRadioMode.DME_ARK15_LOW:
                                        {
                                            break;
                                        }
                                    case CurrentMi24PRadioMode.SPU8:
                                        {
                                            DCSBIOS.Send(SPU8_VOLUME_KNOB_COMMAND_DEC);
                                            break;
                                        }
                                    case CurrentMi24PRadioMode.NOUSE:
                                        {
                                            break;
                                        }
                                }
                                break;
                            }
                        case RadioPanelPZ69KnobsMi24P.LOWER_LARGE_FREQ_WHEEL_INC:
                            {
                                switch (_currentLowerRadioMode)
                                {
                                    case CurrentMi24PRadioMode.R863_MANUAL:
                                        {
                                            break;
                                        }
                                    case CurrentMi24PRadioMode.R863_PRESET:
                                        {
                                            if (!SkipR863PresetDialChange())
                                            {
                                                DCSBIOS.Send(R863_PRESET_COMMAND_INC);
                                            }
                                            break;
                                        }
                                    case CurrentMi24PRadioMode.YADRO1A:
                                        {
                                            YADRO1A_BigFrequencyIncrease();
                                            break;
                                        }
                                    case CurrentMi24PRadioMode.R828_PRESETS:
                                        {
                                            if (!SkipR828PresetDialChange())
                                            {
                                                DCSBIOS.Send(R828_PRESET_COMMAND_INC);
                                            }
                                            break;
                                        }
                                    case CurrentMi24PRadioMode.ADF_ARK15_HIGH:
                                        {
                                            if (!SkipADFPresetDial1Change())
                                            {
                                                DCSBIOS.Send(_adfBackupMainCockpitDial1Pos == 1 ? ADF_MAIN100_KHZ_PRESET_COMMAND_INC : ADF_BACKUP100_KHZ_PRESET_COMMAND_INC);
                                            }
                                            break;
                                        }
                                    case CurrentMi24PRadioMode.DME_ARK15_LOW:
                                        {
                                            if (!SkipDMEPresetDial1Change())
                                            {
                                                DCSBIOS.Send(_adfBackupMainCockpitDial1Pos == 1 ? DME_MAIN1_KHZ_PRESET_COMMAND_INC : DME_BACKUP1_KHZ_PRESET_COMMAND_INC);
                                            }
                                            break;
                                        }
                                    case CurrentMi24PRadioMode.SPU8:
                                        {
                                            if (!SkipSpu8PresetDialChange())
                                            {
                                                DCSBIOS.Send(SPU8_COMMAND_INC);
                                            }
                                            break;
                                        }
                                    case CurrentMi24PRadioMode.NOUSE:
                                        {
                                            break;
                                        }
                                }
                                break;
                            }
                        case RadioPanelPZ69KnobsMi24P.LOWER_LARGE_FREQ_WHEEL_DEC:
                            {
                                switch (_currentLowerRadioMode)
                                {
                                    case CurrentMi24PRadioMode.R863_MANUAL:
                                        {
                                            break;
                                        }
                                    case CurrentMi24PRadioMode.R863_PRESET:
                                        {
                                            if (!SkipR863PresetDialChange())
                                            {
                                                DCSBIOS.Send(R863_PRESET_COMMAND_DEC);
                                            }
                                            break;
                                        }
                                    case CurrentMi24PRadioMode.YADRO1A:
                                        {
                                            YADRO1A_BigFrequencyDecrease();
                                            break;
                                        }
                                    case CurrentMi24PRadioMode.R828_PRESETS:
                                        {
                                            if (!SkipR828PresetDialChange())
                                            {
                                                DCSBIOS.Send(R828_PRESET_COMMAND_DEC);
                                            }
                                            break;
                                        }
                                    case CurrentMi24PRadioMode.ADF_ARK15_HIGH:
                                        {
                                            if (!SkipADFPresetDial1Change())
                                            {
                                                DCSBIOS.Send(_adfBackupMainCockpitDial1Pos == 1 ? ADF_MAIN100_KHZ_PRESET_COMMAND_DEC : ADF_BACKUP100_KHZ_PRESET_COMMAND_DEC);
                                            }
                                            break;
                                        }
                                    case CurrentMi24PRadioMode.DME_ARK15_LOW:
                                        {
                                            if (!SkipDMEPresetDial1Change())
                                            {
                                                DCSBIOS.Send(_adfBackupMainCockpitDial1Pos == 1 ? DME_MAIN1_KHZ_PRESET_COMMAND_DEC : DME_BACKUP1_KHZ_PRESET_COMMAND_DEC);
                                            }
                                            break;
                                        }
                                    case CurrentMi24PRadioMode.SPU8:
                                        {
                                            if (!SkipSpu8PresetDialChange())
                                            {
                                                DCSBIOS.Send(SPU8_COMMAND_DEC);
                                            }
                                            break;
                                        }
                                    case CurrentMi24PRadioMode.NOUSE:
                                        {
                                            break;
                                        }
                                }
                                break;
                            }
                        case RadioPanelPZ69KnobsMi24P.LOWER_SMALL_FREQ_WHEEL_INC:
                            {
                                switch (_currentLowerRadioMode)
                                {
                                    case CurrentMi24PRadioMode.R863_MANUAL:
                                        {
                                            break;
                                        }
                                    case CurrentMi24PRadioMode.R863_PRESET:
                                        {
                                            DCSBIOS.Send(R863_PRESET_VOLUME_KNOB_COMMAND_INC);
                                            break;
                                        }
                                    case CurrentMi24PRadioMode.YADRO1A:
                                        {
                                            YADRO1A_SmallFrequencyIncrease();
                                            break;
                                        }
                                    case CurrentMi24PRadioMode.R828_PRESETS:
                                        {
                                            DCSBIOS.Send(R828_PRESET_VOLUME_KNOB_COMMAND_INC);
                                            break;
                                        }
                                    case CurrentMi24PRadioMode.ADF_ARK15_HIGH:
                                        {
                                            if (!SkipADFPresetDial2Change())
                                            {
                                                DCSBIOS.Send(_adfBackupMainCockpitDial1Pos == 1 ? ADF_MAIN10_KHZ_PRESET_COMMAND_INC : ADF_BACKUP10_KHZ_PRESET_COMMAND_INC);
                                            }
                                            break;
                                        }
                                    case CurrentMi24PRadioMode.DME_ARK15_LOW:
                                        {
                                            break;
                                        }
                                    case CurrentMi24PRadioMode.SPU8:
                                        {
                                            DCSBIOS.Send(SPU8_VOLUME_KNOB_COMMAND_INC);
                                            break;
                                        }
                                    case CurrentMi24PRadioMode.NOUSE:
                                        {
                                            break;
                                        }
                                }
                                break;
                            }
                        case RadioPanelPZ69KnobsMi24P.LOWER_SMALL_FREQ_WHEEL_DEC:
                            {
                                switch (_currentLowerRadioMode)
                                {
                                    case CurrentMi24PRadioMode.R863_MANUAL:
                                        {
                                            break;
                                        }
                                    case CurrentMi24PRadioMode.R863_PRESET:
                                        {
                                            DCSBIOS.Send(R863_PRESET_VOLUME_KNOB_COMMAND_DEC);
                                            break;
                                        }
                                    case CurrentMi24PRadioMode.YADRO1A:
                                        {
                                            YADRO1A_SmallFrequencyDecrease();
                                            break;
                                        }
                                    case CurrentMi24PRadioMode.R828_PRESETS:
                                        {
                                            DCSBIOS.Send(R828_PRESET_VOLUME_KNOB_COMMAND_DEC);
                                            break;
                                        }
                                    case CurrentMi24PRadioMode.ADF_ARK15_HIGH:
                                        {
                                            if (!SkipADFPresetDial2Change())
                                            {
                                                DCSBIOS.Send(_adfBackupMainCockpitDial1Pos == 1 ? ADF_MAIN10_KHZ_PRESET_COMMAND_DEC : ADF_BACKUP10_KHZ_PRESET_COMMAND_DEC);
                                            }
                                            break;
                                        }
                                    case CurrentMi24PRadioMode.DME_ARK15_LOW:
                                        {
                                            break;
                                        }
                                    case CurrentMi24PRadioMode.SPU8:
                                        {
                                            DCSBIOS.Send(SPU8_VOLUME_KNOB_COMMAND_DEC);
                                            break;
                                        }
                                    case CurrentMi24PRadioMode.NOUSE:
                                        {
                                            break;
                                        }
                                }
                                break;
                            }
                    }
                }
                ShowFrequenciesOnPanel();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void ShowFrequenciesOnPanel()
        {
            try
            {
                lock (_lockShowFrequenciesOnPanelObject)
                {
                    if (Interlocked.Read(ref _doUpdatePanelLCD) == 0)
                    {
                        return;
                    }

                    if (!FirstReportHasBeenRead)
                    {
                        return;
                    }

                    var bytes = new byte[21];
                    bytes[0] = 0x0;

                    switch (_currentUpperRadioMode)
                    {
                        case CurrentMi24PRadioMode.R863_MANUAL:
                            {
                                break;
                            }
                        case CurrentMi24PRadioMode.R863_PRESET:
                            {
                                //Preset Channel Selector
                                //      " 0" " 1" " 2" " 3" " 4" " 5" " 6" " 7" " 8" " 9"..."19"
                                //Pos     0    1    2    3    4    5    6    7    8    9 ... 19 
                                var channelAsString = string.Empty;
                                lock (_lockR863Preset1DialObject1)
                                {
                                    channelAsString = _r863PresetCockpitDialPos.ToString().PadLeft(2, ' ');
                                }
                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, Convert.ToUInt32(channelAsString), PZ69LCDPosition.UPPER_STBY_RIGHT);
                                SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.UPPER_ACTIVE_LEFT);
                                break;
                            }

                        case CurrentMi24PRadioMode.YADRO1A:
                            {
                                lock (_lockYadro1ADialsObject1)
                                {
                                    lock (_lockYadro1ADialsObject2)
                                    {
                                        lock (_lockYadro1ADialsObject3)
                                        {
                                            lock (_lockYadro1ADialsObject4)
                                            {
                                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, (uint)_yadro1ACockpitFrequency, PZ69LCDPosition.UPPER_ACTIVE_LEFT);
                                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, Convert.ToUInt32(_yadro1ABigFrequencyStandby.ToString().PadLeft(3, '0') + _yadro1ASmallFrequencyStandby.ToString().PadLeft(2, '0')), PZ69LCDPosition.UPPER_STBY_RIGHT);
                                            }
                                        }
                                    }
                                }
                                break;
                            }
                        case CurrentMi24PRadioMode.R828_PRESETS:
                            {
                                //Preset Channel Selector
                                //      " 0" " 1" " 2" " 3" " 4" " 5" " 6" " 7" " 8" " 9" 
                                //Pos     0    1    2    3    4    5    6    7    8    9  
                                var channelAsString = string.Empty;
                                lock (_lockR828Preset1DialObject1)
                                {
                                    channelAsString = _r828PresetCockpitDialPos.ToString().PadLeft(2, ' ');
                                }
                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, Convert.ToUInt32(channelAsString), PZ69LCDPosition.UPPER_STBY_RIGHT);
                                SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.UPPER_ACTIVE_LEFT);
                                break;
                            }
                        case CurrentMi24PRadioMode.ADF_ARK15_HIGH:
                            {
                                //Dial1 XX0..
                                //Dial2 00X..
                               
                                uint backupMain = 0;
                                lock (_lockADFBackupMainDialObject)
                                {
                                    backupMain = _adfBackupMainCockpitDial1Pos;
                                    if (_adfBackupMainCockpitDial1Pos == 1)
                                    {
                                        lock (_lockADFMainDialObject1)
                                        {
                                            _ark15_HighFrequency = _adfMainCockpitPresetDial1Pos.ToString();
                                        }
                                    }
                                    else
                                    {
                                        lock (_lockADFBackupDialObject1)
                                        {
                                            _ark15_HighFrequency = _adfBackupCockpitPresetDial1Pos.ToString();
                                        }
                                    }
                                    if (_adfBackupMainCockpitDial1Pos == 1)
                                    {
                                        lock (_lockADFMainDialObject2)
                                        {
                                            _ark15_HighFrequency += _adfMainCockpitPresetDial2Pos.ToString();
                                        }
                                    }
                                    else
                                    {
                                        lock (_lockADFBackupDialObject2)
                                        {
                                            _ark15_HighFrequency += _adfBackupCockpitPresetDial2Pos.ToString();
                                        }
                                    }
                                }
                                SetPZ69DisplayBytesDefault(ref bytes, Ark15MergedFrequencies, PZ69LCDPosition.UPPER_STBY_RIGHT);
                                //We'll set a +1 here so it matches the number on the switch. same for the other cases refering to ark-15
                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, backupMain + 1, PZ69LCDPosition.UPPER_ACTIVE_LEFT);
                                break;
                            }
                        case CurrentMi24PRadioMode.DME_ARK15_LOW:
                            {
                                //Large Dial1 X.X
                                //      " 0" "0.5" "1.0" "1.5" "2.0" "2.5"... "9.5"
                                //Pos     0     1     2     3     4     5 ...   19
                                uint backupMain = 0;
                                lock (_lockADFBackupMainDialObject)
                                {
                                    backupMain = _adfBackupMainCockpitDial1Pos;
                                    if (_adfBackupMainCockpitDial1Pos == 1)
                                    {
                                        lock (_lockDMEMainDialObject1)
                                        {
                                            _ark15_LowFrequency = DivideBy2AndFormatForDisplay(_dmeMainCockpitPresetDial1Pos);
                                        }
                                    }
                                    else
                                    {
                                        lock (_lockADFBackupDialObject1)
                                        {
                                            _ark15_LowFrequency = DivideBy2AndFormatForDisplay(_dmeBackupCockpitPresetDial1Pos);
                                        }
                                    }
                                }
                                SetPZ69DisplayBytesDefault(ref bytes, _ark15_LowFrequency.PadLeft(6,' '), PZ69LCDPosition.UPPER_STBY_RIGHT);
                                if (_currentLowerRadioMode == CurrentMi24PRadioMode.ADF_ARK15_HIGH)
                                {
                                    SetPZ69DisplayBytesDefault(ref bytes, Ark15MergedFrequencies, PZ69LCDPosition.LOWER_STBY_RIGHT); //update also the higher frequency display with the new value
                                }
                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, backupMain + 1, PZ69LCDPosition.UPPER_ACTIVE_LEFT);
                                break;
                            }
                        case CurrentMi24PRadioMode.SPU8:
                            {
                                //0 - 5
                                var channelAsString = string.Empty;
                                uint spuICSSwitch = 0;
                                lock (_lockSpu8DialObject1)
                                {
                                    channelAsString = _spu8CockpitDialPos.ToString().PadLeft(2, ' ');
                                }
                                lock (_lockSpu8ICSSwitchObject)
                                {
                                    spuICSSwitch = _spu8ICSSwitchCockpitDialPos;
                                }
                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, Convert.ToUInt32(channelAsString), PZ69LCDPosition.UPPER_STBY_RIGHT);
                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, spuICSSwitch, PZ69LCDPosition.UPPER_ACTIVE_LEFT);
                                break;
                            }
                        case CurrentMi24PRadioMode.NOUSE:
                            {
                                SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.UPPER_STBY_RIGHT);
                                SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.UPPER_ACTIVE_LEFT);
                                break;
                            }
                    }
                    switch (_currentLowerRadioMode)
                    {
                        case CurrentMi24PRadioMode.R863_MANUAL:
                            {
                                break;
                            }
                        case CurrentMi24PRadioMode.R863_PRESET:
                            {
                                //Preset Channel Selector
                                //      " 0" " 1" " 2" " 3" " 4" " 5" " 6" " 7" " 8" " 9"..."19"
                                //Pos     0    1    2    3    4    5    6    7    8    9 ... 19 
                                var channelAsString = string.Empty;
                                lock (_lockR863Preset1DialObject1)
                                {
                                    channelAsString = _r863PresetCockpitDialPos.ToString().PadLeft(2, ' ');
                                }
                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, Convert.ToUInt32(channelAsString), PZ69LCDPosition.LOWER_STBY_RIGHT);
                                SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.LOWER_ACTIVE_LEFT);
                                break;
                            }

                        case CurrentMi24PRadioMode.YADRO1A:
                            {
                                lock (_lockYadro1ADialsObject1)
                                {
                                    lock (_lockYadro1ADialsObject2)
                                    {
                                        lock (_lockYadro1ADialsObject3)
                                        {
                                            lock (_lockYadro1ADialsObject4)
                                            {
                                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, (uint)_yadro1ACockpitFrequency, PZ69LCDPosition.LOWER_ACTIVE_LEFT);
                                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, Convert.ToUInt32(_yadro1ABigFrequencyStandby.ToString().PadLeft(3, '0') + _yadro1ASmallFrequencyStandby.ToString().PadLeft(2, '0')), PZ69LCDPosition.LOWER_STBY_RIGHT);
                                            }
                                        }
                                    }
                                }
                                break;
                            }
                        case CurrentMi24PRadioMode.R828_PRESETS:
                            {
                                //Preset Channel Selector
                                //      " 0" " 1" " 2" " 3" " 4" " 5" " 6" " 7" " 8" " 9"
                                //Pos     0    1    2    3    4    5    6    7    8    9  
                                var channelAsString = string.Empty;
                                lock (_lockR828Preset1DialObject1)
                                {
                                    channelAsString = _r828PresetCockpitDialPos.ToString().PadLeft(2, ' ');
                                }
                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, Convert.ToUInt32(channelAsString), PZ69LCDPosition.LOWER_STBY_RIGHT);
                                SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.LOWER_ACTIVE_LEFT);
                                break;
                            }
                        case CurrentMi24PRadioMode.ADF_ARK15_HIGH:
                            {
                                //Dial1 XX0
                                //Dial2 00X
                                uint backupMain = 0;
                                lock (_lockADFBackupMainDialObject)
                                {
                                    backupMain = _adfBackupMainCockpitDial1Pos;
                                    if (_adfBackupMainCockpitDial1Pos == 1)
                                    {
                                        lock (_lockADFMainDialObject1)
                                        {
                                            _ark15_HighFrequency = _adfMainCockpitPresetDial1Pos.ToString();
                                        }
                                        lock (_lockADFMainDialObject2)
                                        {
                                            _ark15_HighFrequency += _adfMainCockpitPresetDial2Pos.ToString();
                                        }
                                    }
                                    else
                                    {
                                        lock (_lockADFBackupDialObject1)
                                        {
                                            _ark15_HighFrequency = _adfBackupCockpitPresetDial1Pos.ToString();
                                        }
                                        lock (_lockADFBackupDialObject2)
                                        {
                                            _ark15_HighFrequency += _adfBackupCockpitPresetDial2Pos.ToString();
                                        }
                                    }
                                }
                                SetPZ69DisplayBytesDefault(ref bytes, Ark15MergedFrequencies, PZ69LCDPosition.LOWER_STBY_RIGHT);
                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, backupMain + 1, PZ69LCDPosition.LOWER_ACTIVE_LEFT);
                                break;
                            }
                        case CurrentMi24PRadioMode.DME_ARK15_LOW:
                            {
                                //Large Dial1 X.X
                                uint backupMain = 0;
                                lock (_lockADFBackupMainDialObject)
                                {
                                    backupMain = _adfBackupMainCockpitDial1Pos;
                                    if (_adfBackupMainCockpitDial1Pos == 1)
                                    {
                                        lock (_lockDMEMainDialObject1)
                                        {
                                            _ark15_LowFrequency = DivideBy2AndFormatForDisplay(_dmeMainCockpitPresetDial1Pos);
                                        }
                                    }
                                    else
                                    {
                                        lock (_lockADFBackupDialObject1)
                                        {
                                            _ark15_LowFrequency = DivideBy2AndFormatForDisplay(_dmeBackupCockpitPresetDial1Pos);
                                        }
                                    }
                                }
                                SetPZ69DisplayBytesDefault(ref bytes, _ark15_LowFrequency.PadLeft(6, ' '), PZ69LCDPosition.LOWER_STBY_RIGHT);
                                if (_currentUpperRadioMode == CurrentMi24PRadioMode.ADF_ARK15_HIGH)
                                {
                                    SetPZ69DisplayBytesDefault(ref bytes, Ark15MergedFrequencies, PZ69LCDPosition.UPPER_STBY_RIGHT); //update also the higher frequency display with the new value
                                }
                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, backupMain + 1, PZ69LCDPosition.LOWER_ACTIVE_LEFT);
                                break;
                            }
                        case CurrentMi24PRadioMode.SPU8:
                            {
                                //0-5
                                var channelAsString = string.Empty;
                                uint spuICSSwitch = 0;
                                lock (_lockSpu8DialObject1)
                                {
                                    channelAsString = _spu8CockpitDialPos.ToString().PadLeft(2, ' ');
                                }
                                lock (_lockSpu8ICSSwitchObject)
                                {
                                    spuICSSwitch = _spu8ICSSwitchCockpitDialPos;
                                }
                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, Convert.ToUInt32(channelAsString), PZ69LCDPosition.LOWER_STBY_RIGHT);
                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, spuICSSwitch, PZ69LCDPosition.LOWER_ACTIVE_LEFT);
                                break;
                            }
                        case CurrentMi24PRadioMode.NOUSE:
                            {
                                SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.LOWER_ACTIVE_LEFT);
                                SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.LOWER_STBY_RIGHT);
                                break;
                            }
                    }
                    SendLCDData(bytes);
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
            Interlocked.Decrement(ref _doUpdatePanelLCD);
        }

        private static string DivideBy2AndFormatForDisplay(uint position)
        {
            double frequencyAsDouble = (double)position / 2;
            return frequencyAsDouble.ToString("0.0", CultureInfo.InvariantCulture);
        }

        protected override void GamingPanelKnobChanged(bool isFirstReport, IEnumerable<object> hashSet)
        {
            PZ69KnobChanged(isFirstReport, hashSet);
        }

        public sealed override void Startup()
        {
            try
            {
                //COM1

                //COM2
                _r863Preset1DcsbiosOutputPresetDial = DCSBIOSControlLocator.GetDCSBIOSOutput("PLT_R863_CHAN");

                //NAV1
                _yadro1ADcsbiosOutputCockpitFrequency = DCSBIOSControlLocator.GetDCSBIOSOutput("JADRO_FREQ");
                DCSBIOSStringManager.AddListeningAddress(_yadro1ADcsbiosOutputCockpitFrequency);

                //NAV2
                _r828Preset1DcsbiosOutputDial = DCSBIOSControlLocator.GetDCSBIOSOutput("PLT_R828_CHAN");

                //ADF
                _adfMainDcsbiosOutputPresetDial1 = DCSBIOSControlLocator.GetDCSBIOSOutput("PLT_ARC_FREQ_L_100");
                _adfMainDcsbiosOutputPresetDial2 = DCSBIOSControlLocator.GetDCSBIOSOutput("PLT_ARC_FREQ_L_10");
                _adfBackupDcsbiosOutputPresetDial1 = DCSBIOSControlLocator.GetDCSBIOSOutput("PLT_ARC_FREQ_R_100");
                _adfBackupDcsbiosOutputPresetDial2 = DCSBIOSControlLocator.GetDCSBIOSOutput("PLT_ARC_FREQ_R_10");
                _adfBackupMainDcsbiosOutputPresetDial = DCSBIOSControlLocator.GetDCSBIOSOutput("PLT_ARC_CHAN");

                //DME
                _dmeMainDcsbiosOutputPresetDial1 = DCSBIOSControlLocator.GetDCSBIOSOutput("PLT_ARC_FREQ_L_1");
                _dmeBackupDcsbiosOutputPresetDial1 = DCSBIOSControlLocator.GetDCSBIOSOutput("PLT_ARC_FREQ_R_1");

                //XPDR
                _spu8DcsbiosOutputPresetDial = DCSBIOSControlLocator.GetDCSBIOSOutput("PLT_SPU8_MODE");
                _spu8ICSSwitchDcsbiosOutput = DCSBIOSControlLocator.GetDCSBIOSOutput("PLT_SPU8_ICS");

                StartListeningForHidPanelChanges();
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
        }
        
        public override void ClearSettings(bool setIsDirty = false) { }

        public override DcsOutputAndColorBinding CreateDcsOutputAndColorBinding(SaitekPanelLEDPosition saitekPanelLEDPosition, PanelLEDColor panelLEDColor, DCSBIOSOutput dcsBiosOutput)
        {
            throw new Exception("Radio Panel does not support color bindings with DCS-BIOS.");
        }

        private void CreateRadioKnobs()
        {
            SaitekPanelKnobs = RadioPanelKnobMi24P.GetRadioPanelKnobs();
        }

        private void SetUpperRadioMode(bool radioKnobIsOn, CurrentMi24PRadioMode CurrentMi24PRadioMode)
        {
            if (!radioKnobIsOn)
            {
                return;
            }

            try
            {
                _currentUpperRadioMode = CurrentMi24PRadioMode;
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
        }

        private void SetLowerRadioMode(bool radioKnobIsOn, CurrentMi24PRadioMode CurrentMi24PRadioMode)
        {
            if (!radioKnobIsOn)
            {
                return;
            }
            try
            {
                _currentLowerRadioMode = CurrentMi24PRadioMode;
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
        }

        private bool YaDRO1ANowSyncing()
        {
            return Interlocked.Read(ref _yadro1AThreadNowSynching) > 0;
        }

        private void SaveCockpitFrequencyYaDRO1A()
        {
            try
            {
                /*
                 * 02000
                 * 17999
                 * 
                 * Dial 1
                 * 02 - 17
                 * 
                 * Dial 2
                 * 0 - 9
                 * 
                 * "."
                 * 
                 * Dial 3
                 * 0 - 9
                 * 
                 * Dial 4
                 * 0 - 9
                 */

                lock (_lockYadro1ADialsObject1)
                {
                    lock (_lockYadro1ADialsObject2)
                    {
                        lock (_lockYadro1ADialsObject3)
                        {
                            lock (_lockYadro1ADialsObject4)
                            {
                                _yadro1ASavedCockpitBigFrequency = uint.Parse(_yadro1ACockpitFreq1DialPos.ToString() + _yadro1ACockpitFreq2DialPos.ToString());
                                _yadro1ASavedCockpitSmallFrequency = uint.Parse(_yadro1ACockpitFreq3DialPos.ToString() + _yadro1ACockpitFreq4DialPos.ToString());
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
        }

        private static string GetCommandDirectionFor0To9Dials(int desiredDialPosition, uint actualDialPosition)
        {
            try
            {
                uint tmpActualDialPositionUp = actualDialPosition;
                uint upCount = actualDialPosition;
                do
                {
                    if (tmpActualDialPositionUp == 9)
                    {
                        tmpActualDialPositionUp = 0;
                    }
                    else
                    {
                        tmpActualDialPositionUp++;
                    }
                    upCount++;
                } while (tmpActualDialPositionUp != desiredDialPosition);

                tmpActualDialPositionUp = actualDialPosition;
                uint downCount = actualDialPosition;
                do
                {
                    if (tmpActualDialPositionUp == 0)
                    {
                        tmpActualDialPositionUp = 9;
                    }
                    else
                    {
                        tmpActualDialPositionUp--;
                    }
                    downCount++;
                } while (tmpActualDialPositionUp != desiredDialPosition);

                return upCount < downCount ? "INC\n" : "DEC\n";
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
            throw new Exception("Should not reach this code. private String GetCommandDirectionFor0To9Dials(uint desiredDialPosition, uint actualDialPosition) -> " + desiredDialPosition + "   " + actualDialPosition);
        }

        private bool SkipR863PresetDialChange()
        {
            try
            {
                if (_r863PresetDialSkipper > 2)
                {
                    _r863PresetDialSkipper = 0;
                    return false;
                }
                _r863PresetDialSkipper++;
                return true;
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
            return false;
        }

        private bool SkipR828PresetDialChange()
        {
            try
            {
                if (_r828PresetDialSkipper > 2)
                {
                    _r828PresetDialSkipper = 0;
                    return false;
                }
                _r828PresetDialSkipper++;
                return true;
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
            return false;
        }

        private bool SkipADFPresetDial1Change()
        {
            try
            {
                if (_adfPresetDial1Skipper > 2)
                {
                    _adfPresetDial1Skipper = 0;
                    return false;
                }
                _adfPresetDial1Skipper++;
                return true;
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
            return false;
        }

        private bool SkipDMEPresetDial1Change()
        {
            try
            {
                if (_dmePresetDial1Skipper > 2)
                {
                    _dmePresetDial1Skipper = 0;
                    return false;
                }
                _dmePresetDial1Skipper++;
                return true;
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
            return false;
        }

        private bool SkipADFPresetDial2Change()
        {
            try
            {
                if (_adfPresetDial2Skipper > 2)
                {
                    _adfPresetDial2Skipper = 0;
                    return false;
                }
                _adfPresetDial2Skipper++;
                return true;
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
            return false;
        }

        private bool SkipSpu8PresetDialChange()
        {
            try
            {
                if (_spu8DialSkipper > 2)
                {
                    _spu8DialSkipper = 0;
                    return false;
                }
                _spu8DialSkipper++;
                return true;
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
            return false;
        }

        public override void RemoveSwitchFromList(object controlList, PanelSwitchOnOff panelSwitchOnOff)
        {
        }

        public override void AddOrUpdateKeyStrokeBinding(PanelSwitchOnOff panelSwitchOnOff, string keyPress, KeyPressLength keyPressLength)
        {
        }

        public override void AddOrUpdateSequencedKeyBinding(PanelSwitchOnOff panelSwitchOnOff, string description, SortedList<int, IKeyPressInfo> keySequence)
        {
        }

        public override void AddOrUpdateDCSBIOSBinding(PanelSwitchOnOff panelSwitchOnOff, List<DCSBIOSInput> dcsbiosInputs, string description, bool isSequenced)
        {
        }

        public override void AddOrUpdateBIPLinkBinding(PanelSwitchOnOff panelSwitchOnOff, BIPLinkBase bipLink)
        {
        }

        public override void AddOrUpdateOSCommandBinding(PanelSwitchOnOff panelSwitchOnOff, OSCommand operatingSystemCommand)
        {
        }

        private string Ark15MergedFrequencies
        {
            get {
                return (_ark15_HighFrequency + _ark15_LowFrequency).PadLeft(6,' '); 
            }
        }
    }
}
