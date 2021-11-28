namespace NonVisuals.Radios
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Threading;

    using ClassLibraryCommon;

    using DCS_BIOS;
    using DCS_BIOS.EventArgs;
    using DCS_BIOS.Interfaces;

    using MEF;
    using NonVisuals.Interfaces;
    using NonVisuals.Plugin;
    using NonVisuals.Radios.Knobs;
    using NonVisuals.Saitek;

    public class RadioPanelPZ69Mi8 : RadioPanelPZ69Base, IRadioPanel, IDCSBIOSStringListener
    {
        private CurrentMi8RadioMode _currentUpperRadioMode = CurrentMi8RadioMode.R863_MANUAL;
        private CurrentMi8RadioMode _currentLowerRadioMode = CurrentMi8RadioMode.R863_MANUAL;

        /*
         * Mi-8 VHF/UHF R-863 MANUAL COM1
         * Large dial 100-149  -> 220 - 399 [step of 1]
         * Small dial 0 - 95
         */
        private readonly ClickSpeedDetector _bigFreqIncreaseChangeMonitor = new ClickSpeedDetector(20);

        private readonly ClickSpeedDetector _bigFreqDecreaseChangeMonitor = new ClickSpeedDetector(20);

        private const int CHANGE_VALUE = 10;

        // private int[] _r863ManualFreq1DialValues = { 10, 11, 12, 13, 14, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39 };
        private volatile uint _r863ManualBigFrequencyStandby = 108;

        private volatile uint _r863ManualSmallFrequencyStandby;

        private volatile uint _r863ManualSavedCockpitBigFrequency;

        private volatile uint _r863ManualSavedCockpitSmallFrequency;

        private readonly object _lockR863ManualDialsObject1 = new object();

        private readonly object _lockR863ManualDialsObject2 = new object();

        private readonly object _lockR863ManualDialsObject3 = new object();

        private readonly object _lockR863ManualDialsObject4 = new object();

        private volatile uint _r863ManualCockpitFreq1DialPos = 1;

        private volatile uint _r863ManualCockpitFreq2DialPos = 1;

        private volatile uint _r863ManualCockpitFreq3DialPos = 1;

        private volatile uint _r863ManualCockpitFreq4DialPos;

        private double _r863ManualCockpitFrequency = 100.000;

        private DCSBIOSOutput _r863ManualDcsbiosOutputCockpitFrequency;

        private const string R863_MANUAL_FREQ_1DIAL_COMMAND = "R863_FREQ1 ";

        private const string R863_MANUAL_FREQ_2DIAL_COMMAND = "R863_FREQ2 ";

        private const string R863_MANUAL_FREQ_3DIAL_COMMAND = "R863_FREQ3 ";

        private const string R863_MANUAL_FREQ_4DIAL_COMMAND = "R863_FREQ4 ";

        private Thread _r863ManualSyncThread;

        private long _r863ManualThreadNowSynching;

        private long _r863ManualDial1WaitingForFeedback;

        private long _r863ManualDial2WaitingForFeedback;

        private long _r863ManualDial3WaitingForFeedback;

        private long _r863ManualDial4WaitingForFeedback;

        /*
         * Mi-8 VHF/UHF R-863 PRESETS COM2
         * Large dial 1-10 [step of 1]
         * Small dial volume control
         * ACT/STBY Unit Switch, DIAL/MEMORY
         */
        private readonly object _lockR863Preset1DialObject1 = new object();

        private DCSBIOSOutput _r863Preset1DcsbiosOutputPresetDial;

        private volatile uint _r863PresetCockpitDialPos = 1;

        private const string R863_PRESET_COMMAND_INC = "R863_CNL_SEL INC\n";

        private const string R863_PRESET_COMMAND_DEC = "R863_CNL_SEL DEC\n";

        // private int _r863PresetDialSkipper;

        private const string R863_PRESET_VOLUME_KNOB_COMMAND_INC = "R863_VOL +2500\n";

        private const string R863_PRESET_VOLUME_KNOB_COMMAND_DEC = "R863_VOL -2500\n";

        private readonly object _lockR863UnitSwitchObject = new object();

        private DCSBIOSOutput _r863UnitSwitchDcsbiosOutput;

        private volatile uint _r863UnitSwitchCockpitPos = 1;

        private const string R863_UNIT_SWITCH_COMMAND_TOGGLE = "R863_UNIT_SWITCH TOGGLE\n";

        /*
         * Mi-8 YaDRO 1A NAV1
         * Large dial 100-149  -> 20 - 179 [step of 1]
         * Small dial 0 - 99
         */
        private readonly ClickSpeedDetector _yadro1ABigFreqIncreaseChangeMonitor = new ClickSpeedDetector(20);

        private readonly ClickSpeedDetector _yadro1ABigFreqDecreaseChangeMonitor = new ClickSpeedDetector(20);

        private volatile uint _yadro1ABigFrequencyStandby = 100;

        private volatile uint _yadro1ASmallFrequencyStandby;

        private volatile uint _yadro1ASavedCockpitBigFrequency;

        private volatile uint _yadro1ASavedCockpitSmallFrequency;

        private readonly object _lockYadro1ADialsObject1 = new object();

        private readonly object _lockYadro1ADialsObject2 = new object();

        private readonly object _lockYadro1ADialsObject3 = new object();

        private readonly object _lockYadro1ADialsObject4 = new object();

        private volatile uint _yadro1ACockpitFreq1DialPos = 1;

        private volatile uint _yadro1ACockpitFreq2DialPos = 1;

        private volatile uint _yadro1ACockpitFreq3DialPos = 1;

        private volatile uint _yadro1ACockpitFreq4DialPos = 1;

        private double _yadro1ACockpitFrequency = 100;

        private DCSBIOSOutput _yadro1ADcsbiosOutputCockpitFrequency;

        private const string YADRO1_A_FREQ_1DIAL_COMMAND = "YADRO1A_FREQ1 ";

        private const string YADRO1_A_FREQ_2DIAL_COMMAND = "YADRO1A_FREQ2 ";

        private const string YADRO1_A_FREQ_3DIAL_COMMAND = "YADRO1A_FREQ3 ";

        private const string YADRO1_A_FREQ_4DIAL_COMMAND = "YADRO1A_FREQ4 ";

        private Thread _yadro1ASyncThread;

        private long _yadro1AThreadNowSynching;

        private long _yadro1ADial1WaitingForFeedback;

        private long _yadro1ADial2WaitingForFeedback;

        private long _yadro1ADial3WaitingForFeedback;

        private long _yadro1ADial4WaitingForFeedback;

        /*
         * Mi-8 R-828 FM Radio PRESETS NAV2
         * Large dial 1-10 [step of 1]
         * Small dial volume control
         * ACT/STBY AGC, automatic gain control
         */
        private readonly object _lockR828Preset1DialObject1 = new object();

        private DCSBIOSOutput _r828Preset1DcsbiosOutputDial;

        private volatile uint _r828PresetCockpitDialPos = 1;

        private const string R828_PRESET_COMMAND_INC = "R828_PRST_CHAN_SEL INC\n";

        private const string R828_PRESET_COMMAND_DEC = "R828_PRST_CHAN_SEL DEC\n";

        // private int _r828PresetDialSkipper;

        private const string R828_PRESET_VOLUME_KNOB_COMMAND_INC = "R828_VOL +2500\n";

        private const string R828_PRESET_VOLUME_KNOB_COMMAND_DEC = "R828_VOL -2500\n";

        private const string R828_GAIN_CONTROL_COMMAND_ON = "R828_TUNER INC\n";

        private const string R828_GAIN_CONTROL_COMMAND_OFF = "R828_TUNER DEC\n";

        /*
         * Mi-8 ARK-9 ADF MAIN
         * Large 100KHz 01 -> 12
         * Small 10Khz 00 -> 90 (10 steps)
         */
        private readonly object _lockADFMainDialObject1 = new object();

        private readonly object _lockADFMainDialObject2 = new object();

        private DCSBIOSOutput _adfMainDcsbiosOutputPresetDial1;

        private DCSBIOSOutput _adfMainDcsbiosOutputPresetDial2;

        private volatile uint _adfMainCockpitPresetDial1Pos = 1;

        private volatile uint _adfMainCockpitPresetDial2Pos = 1;

        private const string ADF_MAIN100_KHZ_PRESET_COMMAND_INC = "ARC_MAIN_100KHZ INC\n";

        private const string ADF_MAIN100_KHZ_PRESET_COMMAND_DEC = "ARC_MAIN_100KHZ DEC\n";

        private const string ADF_MAIN10_KHZ_PRESET_COMMAND_INC = "ARC_MAIN_10KHZ INC\n";

        private const string ADF_MAIN10_KHZ_PRESET_COMMAND_DEC = "ARC_MAIN_10KHZ DEC\n";

        /*
         *  ADF BACKUP
         */
        private readonly object _lockADFBackupDialObject1 = new object();

        private readonly object _lockADFBackupDialObject2 = new object();

        private DCSBIOSOutput _adfBackupDcsbiosOutputPresetDial1;

        private DCSBIOSOutput _adfBackupDcsbiosOutputPresetDial2;

        private volatile uint _adfBackupCockpitPresetDial1Pos = 1;

        private volatile uint _adfBackupCockpitPresetDial2Pos = 1;

        private const string ADF_BACKUP100_KHZ_PRESET_COMMAND_INC = "ARC_BCK_100KHZ INC\n";

        private const string ADF_BACKUP100_KHZ_PRESET_COMMAND_DEC = "ARC_BCK_100KHZ DEC\n";

        private const string ADF_BACKUP10_KHZ_PRESET_COMMAND_INC = "ARC_BCK_10KHZ INC\n";

        private const string ADF_BACKUP10_KHZ_PRESET_COMMAND_DEC = "ARC_BCK_10KHZ DEC\n";

        /*
         * 0 = Backup ADF
         * 1 = Main ADF
         */
        private readonly object _lockADFBackupMainDialObject = new object();

        private DCSBIOSOutput _adfBackupMainDcsbiosOutputPresetDial;

        private volatile uint _adfBackupMainCockpitDial1Pos;

        private const string ADF_BACKUP_MAIN_SWITCH_TOGGLE_COMMAND = "ARC9_MAIN_BACKUP TOGGLE\n";

        /*Mi-8 ARK-UD VHF Homing (DME)*/
        // Large Frequency 1-6
        // Small Mode
        // ACT/STBY   VHF/UHF
        private readonly object _lockArkudPresetDialObject = new object();

        private DCSBIOSOutput _arkUdPresetDcsbiosOutputPresetDial;

        private volatile uint _arkUdPresetCockpitDial1Pos;

        private const string ARKUD_PRESET_COMMAND_INC = "ARCUD_CHL INC\n";

        private const string ARKUD_PRESET_COMMAND_DEC = "ARCUD_CHL DEC\n";

        // private int _arkUdPresetDialSkipper;

        private readonly object _lockArkudModeDialObject = new object();

        private DCSBIOSOutput _arkUdModeDcsbiosOutputDial;

        private volatile uint _arkUdModeCockpitDial1Pos;

        private const string ARKUD_MODE_COMMAND_INC = "ARCUD_MODE INC\n";

        private const string ARKUD_MODE_COMMAND_DEC = "ARCUD_MODE DEC\n";

        // private int _arkUdModeDialSkipper;

        private readonly object _lockArkudVhfUhfModeDialObject = new object();

        private DCSBIOSOutput _arkUdVhfUhfModeDcsbiosOutputDial;

        private volatile uint _arkUdVhfUhfModeCockpitDial1Pos;

        private const string ARKUD_VHF_UHF_MODE_COMMAND_TOGGLE = "ARCUD_WAVE TOGGLE\n";

        /* XPDR
           Mi-8 SPU-7 XPDR
           Large dial 0-5 [step of 1]
           Small dial volume control
           ACT/STBY Toggle Radio/ICS Switch
        */
        private readonly object _lockSpu7DialObject1 = new object();

        private DCSBIOSOutput _spu7DcsbiosOutputPresetDial;

        private volatile uint _spu7CockpitDialPos;

        // private int _spu7DialSkipper;

        private const string SPU7_COMMAND_INC = "RADIO_SEL_R INC\n";

        private const string SPU7_COMMAND_DEC = "RADIO_SEL_R DEC\n";

        private const string SPU7_VOLUME_KNOB_COMMAND_INC = "LST_VOL_KNOB_L +2500\n";

        private const string SPU7_VOLUME_KNOB_COMMAND_DEC = "LST_VOL_KNOB_L -2500\n";

        private readonly object _lockSpu7ICSSwitchObject = new object();

        private DCSBIOSOutput _spu7ICSSwitchDcsbiosOutput;

        private volatile uint _spu7ICSSwitchCockpitDialPos;

        private const string SPU7_ICS_SWITCH_TOGGLE_COMMAND = "SPU7_L_ICS TOGGLE\n";

        private readonly object _lockShowFrequenciesOnPanelObject = new object();

        private long _doUpdatePanelLCD;

        // private const int SKIP_CONSTANT = 0;

        public RadioPanelPZ69Mi8(HIDSkeleton hidSkeleton)
            : base(hidSkeleton)
        {
            CreateRadioKnobs();
            Startup();
        }

        private bool _disposed;
        // Protected implementation of Dispose pattern.
        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _r863ManualSyncThread?.Abort();
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
                    var tmpFreq = double.Parse(e.StringData, NumberFormatInfoFullDisplay);
                    if (!tmpFreq.Equals(_yadro1ACockpitFrequency))
                    {
                        Interlocked.Increment(ref _doUpdatePanelLCD);
                    }

                    if (tmpFreq.Equals(_yadro1ACockpitFrequency))
                    {
                        // No need to process same data over and over
                        return;
                    }

                    _yadro1ACockpitFrequency = tmpFreq;
                    lock (_lockYadro1ADialsObject1)
                    {
                        // "02000.0" - "*17*999.9"
                        var tmp = _yadro1ACockpitFreq1DialPos;
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
                        var tmp = _yadro1ACockpitFreq2DialPos;
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
                        var tmp = _yadro1ACockpitFreq3DialPos;
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
                        var tmp = _yadro1ACockpitFreq4DialPos;
                        _yadro1ACockpitFreq4DialPos = uint.Parse(e.StringData.Substring(4, 1));
                        if (tmp != _yadro1ACockpitFreq4DialPos)
                        {
                            Interlocked.Increment(ref _doUpdatePanelLCD);
                            Interlocked.Exchange(ref _yadro1ADial4WaitingForFeedback, 0);
                        }
                    }
                }

                if (e.Address.Equals(_r863ManualDcsbiosOutputCockpitFrequency.Address))
                {
                    // "100.000" - "399.975"
                    // Last digit not used in panel
                    double tmpFreq = 0;
                    try
                    {
                        tmpFreq = double.Parse(e.StringData, NumberFormatInfoFullDisplay);
                    }
                    catch (Exception)
                    {
                        return;
                    }

                    if (!tmpFreq.Equals(_r863ManualCockpitFrequency))
                    {
                        Interlocked.Increment(ref _doUpdatePanelLCD);
                    }

                    if (tmpFreq.Equals(_r863ManualCockpitFrequency))
                    {
                        // No need to process same data over and over
                        return;
                    }
                    
                    _r863ManualCockpitFrequency = tmpFreq;
                    lock (_lockR863ManualDialsObject1)
                    {
                        // "100.000" - "*39*9.975"
                        var tmp = _r863ManualCockpitFreq1DialPos;
                        _r863ManualCockpitFreq1DialPos = uint.Parse(e.StringData.Substring(0, 2));
                        if (tmp != _r863ManualCockpitFreq1DialPos)
                        {
                            Interlocked.Increment(ref _doUpdatePanelLCD);
                            Interlocked.Exchange(ref _r863ManualDial1WaitingForFeedback, 0);
                        }
                    }

                    lock (_lockR863ManualDialsObject2)
                    {
                        // "100.000" - "39*9*.975"
                        var tmp = _r863ManualCockpitFreq2DialPos;
                        _r863ManualCockpitFreq2DialPos = uint.Parse(e.StringData.Substring(2, 1));
                        if (tmp != _r863ManualCockpitFreq2DialPos)
                        {
                            Interlocked.Increment(ref _doUpdatePanelLCD);
                            Interlocked.Exchange(ref _r863ManualDial2WaitingForFeedback, 0);
                        }
                    }

                    lock (_lockR863ManualDialsObject3)
                    {
                        // "100.000" - "399.*9*75"
                        var tmp = _r863ManualCockpitFreq3DialPos;
                        _r863ManualCockpitFreq3DialPos = uint.Parse(e.StringData.Substring(4, 1));
                        if (tmp != _r863ManualCockpitFreq3DialPos)
                        {
                            Interlocked.Increment(ref _doUpdatePanelLCD);
                            Interlocked.Exchange(ref _r863ManualDial3WaitingForFeedback, 0);
                        }
                    }

                    lock (_lockR863ManualDialsObject4)
                    {
                        // "100.000" - "399.9*75*"
                        // Read only the first char
                        var tmp = _r863ManualCockpitFreq4DialPos;
                        _r863ManualCockpitFreq4DialPos = uint.Parse(e.StringData.Substring(5, 2));
                        if (tmp != _r863ManualCockpitFreq4DialPos)
                        {
                            Interlocked.Increment(ref _doUpdatePanelLCD);
                            Interlocked.Exchange(ref _r863ManualDial4WaitingForFeedback, 0);
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

                // R-863 Preset Channel Dial
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

                // R-863 Unit Switch
                if (e.Address == _r863UnitSwitchDcsbiosOutput.Address)
                {
                    lock (_lockR863UnitSwitchObject)
                    {
                        var tmp = _r863UnitSwitchCockpitPos;
                        _r863UnitSwitchCockpitPos = _r863UnitSwitchDcsbiosOutput.GetUIntValue(e.Data);
                        if (tmp != _r863UnitSwitchCockpitPos)
                        {
                            Interlocked.Increment(ref _doUpdatePanelLCD);
                        }
                    }
                }

                // YaDRO-1A

                // R-828 Preset Channel Dial
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

                // ADF Main Preset Dial 1
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

                // ADF Main Preset Dial 2
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

                // ADF Backup Preset Dial 1
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

                // ADF Backup Preset Dial 2
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

                // ADF Backup or Main
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

                // ARK-UD  VHF Homing Preset Channels
                if (e.Address == _arkUdPresetDcsbiosOutputPresetDial.Address)
                {
                    lock (_lockArkudPresetDialObject)
                    {
                        var tmp = _arkUdPresetCockpitDial1Pos;
                        _arkUdPresetCockpitDial1Pos = _arkUdPresetDcsbiosOutputPresetDial.GetUIntValue(e.Data);
                        if (tmp != _arkUdPresetCockpitDial1Pos)
                        {
                            Interlocked.Increment(ref _doUpdatePanelLCD);
                        }
                    }
                }

                // ARK-UD  Mode 
                if (e.Address == _arkUdModeDcsbiosOutputDial.Address)
                {
                    lock (_lockArkudModeDialObject)
                    {
                        var tmp = _arkUdModeCockpitDial1Pos;
                        _arkUdModeCockpitDial1Pos = _arkUdModeDcsbiosOutputDial.GetUIntValue(e.Data);
                        if (tmp != _arkUdModeCockpitDial1Pos)
                        {
                            Interlocked.Increment(ref _doUpdatePanelLCD);
                        }
                    }
                }

                // ARK-UD  VHF/UHF
                if (e.Address == _arkUdVhfUhfModeDcsbiosOutputDial.Address)
                {
                    lock (_lockArkudVhfUhfModeDialObject)
                    {
                        var tmp = _arkUdVhfUhfModeCockpitDial1Pos;
                        _arkUdVhfUhfModeCockpitDial1Pos = _arkUdVhfUhfModeDcsbiosOutputDial.GetUIntValue(e.Data);
                        if (tmp != _arkUdVhfUhfModeCockpitDial1Pos)
                        {
                            Interlocked.Increment(ref _doUpdatePanelLCD);
                        }
                    }
                }

                // SPU-7 Dial
                if (e.Address == _spu7DcsbiosOutputPresetDial.Address)
                {
                    lock (_lockSpu7DialObject1)
                    {
                        var tmp = _spu7CockpitDialPos;
                        _spu7CockpitDialPos = _spu7DcsbiosOutputPresetDial.GetUIntValue(e.Data);
                        if (tmp != _spu7CockpitDialPos)
                        {
                            Interlocked.Increment(ref _doUpdatePanelLCD);
                        }
                    }
                }

                // SPU-7 Radio/ICS
                if (e.Address == _spu7ICSSwitchDcsbiosOutput.Address)
                {
                    lock (_lockSpu7ICSSwitchObject)
                    {
                        var tmp = _spu7ICSSwitchCockpitDialPos;
                        _spu7ICSSwitchCockpitDialPos = _spu7ICSSwitchDcsbiosOutput.GetUIntValue(e.Data);
                        if (tmp != _spu7ICSSwitchCockpitDialPos)
                        {
                            Interlocked.Increment(ref _doUpdatePanelLCD);
                        }
                    }
                }

                // Set once
                DataHasBeenReceivedFromDCSBIOS = true;
                ShowFrequenciesOnPanel();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void SendFrequencyToDCSBIOS(bool knobIsOn, RadioPanelPZ69KnobsMi8 knob)
        {
            try
            {
                if (IgnoreSwitchButtonOnce() && (knob == RadioPanelPZ69KnobsMi8.UPPER_FREQ_SWITCH || knob == RadioPanelPZ69KnobsMi8.LOWER_FREQ_SWITCH))
                {
                    // Don't do anything on the very first button press as the panel sends ALL
                    // switches when it is manipulated the first time
                    // This would cause unintended sync.
                    return;
                }

                if (!DataHasBeenReceivedFromDCSBIOS)
                {
                    // Don't start communication with DCS-BIOS before we have had a first contact from "them"
                    return;
                }

                switch (knob)
                {
                    case RadioPanelPZ69KnobsMi8.UPPER_FREQ_SWITCH:
                        {
                            switch (_currentUpperRadioMode)
                            {
                                case CurrentMi8RadioMode.R863_PRESET:
                                    {
                                        break;
                                    }

                                case CurrentMi8RadioMode.R863_MANUAL:
                                    {
                                        SendR863ManualToDCSBIOS();
                                        break;
                                    }

                                case CurrentMi8RadioMode.YADRO1A:
                                    {
                                        SendYaDRO1AToDCSBIOS();
                                        break;
                                    }

                                case CurrentMi8RadioMode.R828_PRESETS:
                                    {
                                        DCSBIOS.Send(knobIsOn ? R828_GAIN_CONTROL_COMMAND_ON : R828_GAIN_CONTROL_COMMAND_OFF);
                                        break;
                                    }

                                case CurrentMi8RadioMode.ADF_ARK9:
                                    {
                                        break;
                                    }

                                case CurrentMi8RadioMode.SPU7:
                                    {
                                        if (knobIsOn)
                                        {
                                            DCSBIOS.Send(SPU7_ICS_SWITCH_TOGGLE_COMMAND);
                                        }

                                        break;
                                    }
                            }

                            break;
                        }

                    case RadioPanelPZ69KnobsMi8.LOWER_FREQ_SWITCH:
                        {
                            switch (_currentLowerRadioMode)
                            {
                                case CurrentMi8RadioMode.R863_PRESET:
                                    {
                                        break;
                                    }

                                case CurrentMi8RadioMode.R863_MANUAL:
                                    {
                                        SendR863ManualToDCSBIOS();
                                        break;
                                    }

                                case CurrentMi8RadioMode.YADRO1A:
                                    {
                                        SendYaDRO1AToDCSBIOS();
                                        break;
                                    }

                                case CurrentMi8RadioMode.R828_PRESETS:
                                    {
                                        DCSBIOS.Send(knobIsOn ? R828_GAIN_CONTROL_COMMAND_ON : R828_GAIN_CONTROL_COMMAND_OFF);
                                        break;
                                    }

                                case CurrentMi8RadioMode.ADF_ARK9:
                                    {
                                        break;
                                    }

                                case CurrentMi8RadioMode.SPU7:
                                    {
                                        if (knobIsOn)
                                        {
                                            DCSBIOS.Send(SPU7_ICS_SWITCH_TOGGLE_COMMAND);
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

        private void SendR863ManualToDCSBIOS()
        {
            try
            {
                if (R863ManualNowSyncing())
                {
                    return;
                }

                SaveCockpitFrequencyR863Manual();

                _r863ManualSyncThread?.Abort();
                _r863ManualSyncThread = new Thread(() => R863ManualSynchThreadMethod());
                _r863ManualSyncThread.Start();
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
        }

        private void R863ManualSynchThreadMethod()
        {
            try
            {
                try
                {
                    try
                    {
                        /*
                                          * Mi-8 R-863 COM1
                                          */
                        Interlocked.Exchange(ref _r863ManualThreadNowSynching, 1);
                        long dial1Timeout = DateTime.Now.Ticks;
                        long dial2Timeout = DateTime.Now.Ticks;
                        long dial3Timeout = DateTime.Now.Ticks;
                        long dial4Timeout = DateTime.Now.Ticks;
                        long dial1OkTime = 0;
                        long dial2OkTime = 0;
                        long dial3OkTime = 0;
                        long dial4OkTime = 0;
                        var dial1SendCount = 0;
                        var dial2SendCount = 0;
                        var dial3SendCount = 0;
                        var dial4SendCount = 0;

                        var frequencyAsString = this._r863ManualBigFrequencyStandby + "." + _r863ManualSmallFrequencyStandby.ToString().PadLeft(3, '0');
                        frequencyAsString = frequencyAsString.PadRight(7, '0');

                        // Frequency selector 1      R863_FREQ1
                        // "10" "11" "12" "13" "14" "22" "23" "24" "25" "26" "27" "28" "29" "30" "31" "32" "33" "34" "35" "36" "37" "38" "39"
                        // Pos     0    1    2    3    4    5    6    7    8    9   10   11   12   13   14   15   16   17   18   19   20   21   22

                        // Frequency selector 2      R863_FREQ2
                        // 0 1 2 3 4 5 6 7 8 9

                        // Frequency selector 3      R863_FREQ3
                        // 0 1 2 3 4 5 6 7 8 9

                        // Frequency selector 4      R863_FREQ4
                        // "00" "25" "50" "75", only "00" and "50" used.
                        // Pos     0    1    2    3

                        // Reason for this is to separate the standby frequency from the sync loop
                        // If not the sync would pick up any changes made by the user during the
                        // sync process
                        var desiredPositionDial1X = 0;
                        var desiredPositionDial2X = 0;
                        var desiredPositionDial3X = 0;
                        var desiredPositionDial4X = 0;

                        // 151.95
                        // #1 = 15  (position = value - 3)
                        // #2 = 1   (position = value)
                        // #3 = 9   (position = value)
                        // #4 = 5
                        desiredPositionDial1X = int.Parse(frequencyAsString.Substring(0, 2)); // Array.IndexOf(_r863ManualFreq1DialValues, int.Parse(xfrequencyAsString.Substring(0, 2)));
                        desiredPositionDial2X = int.Parse(frequencyAsString.Substring(2, 1));
                        desiredPositionDial3X = int.Parse(frequencyAsString.Substring(4, 1));
                        desiredPositionDial4X = int.Parse(frequencyAsString.Substring(5, 2));
                        Debug.WriteLine("Frequency " + frequencyAsString);
                        Debug.WriteLine("Desired1 " + desiredPositionDial1X);
                        Debug.WriteLine("Desired2 " + desiredPositionDial2X);
                        Debug.WriteLine("Desired3 " + desiredPositionDial3X);
                        Debug.WriteLine("Desired4 " + desiredPositionDial4X);

                        do
                        {
                            if (IsTimedOut(ref dial1Timeout))
                            {
                                ResetWaitingForFeedBack(ref _r863ManualDial1WaitingForFeedback); // Lets do an ugly reset
                            }

                            if (IsTimedOut(ref dial2Timeout))
                            {
                                ResetWaitingForFeedBack(ref _r863ManualDial2WaitingForFeedback); // Lets do an ugly reset
                            }

                            if (IsTimedOut(ref dial3Timeout))
                            {
                                ResetWaitingForFeedBack(ref _r863ManualDial3WaitingForFeedback); // Lets do an ugly reset
                            }

                            if (IsTimedOut(ref dial4Timeout))
                            {
                                ResetWaitingForFeedBack(ref _r863ManualDial4WaitingForFeedback); // Lets do an ugly reset
                            }

                            string str;
                            if (Interlocked.Read(ref _r863ManualDial1WaitingForFeedback) == 0)
                            {
                                lock (_lockR863ManualDialsObject1)
                                {
                                    if (_r863ManualCockpitFreq1DialPos != desiredPositionDial1X)
                                    {
                                        dial1OkTime = DateTime.Now.Ticks;
                                        str = R863_MANUAL_FREQ_1DIAL_COMMAND
                                              + GetCommandDirectionForR863ManualDial1(desiredPositionDial1X, _r863ManualCockpitFreq1DialPos);// + "DEC\n"; // TODO is this still a problem? 30.7.2018 Went into loop GetCommandDirectionForR863ManualDial1(desiredPositionDial1X, _r863ManualCockpitFreq1DialPos);

                                        /*
                                                                                    25.7.2018
                                                                                    10	0.22999967634678
                                                                                    39	0.21999971568584
                                        
                                                                                    10Mhz  Rotary Knob (Mi-8MT/R863_FREQ1)
                                                                                    Changing the dial (in cockpit) 39 => 10 does not show in DCS-BIOS in the CTRL-Ref page. But going down from 10 => 39 is OK.
                                                                                    I have gotten the values above from DCS using the Lua console so I can see that they do actually change but there is something not working in DCS-BIOS
                                                                                    So only go down with it
                                                                                 */
                                        DCSBIOS.Send(str);
                                        dial1SendCount++;
                                        Interlocked.Exchange(ref _r863ManualDial1WaitingForFeedback, 1);
                                    }

                                    Reset(ref dial1Timeout);
                                }
                            }
                            else
                            {
                                dial1OkTime = DateTime.Now.Ticks;
                            }

                            if (Interlocked.Read(ref _r863ManualDial2WaitingForFeedback) == 0)
                            {
                                lock (_lockR863ManualDialsObject2)
                                {
                                    if (_r863ManualCockpitFreq2DialPos != desiredPositionDial2X)
                                    {
                                        dial2OkTime = DateTime.Now.Ticks;
                                        str = R863_MANUAL_FREQ_2DIAL_COMMAND + GetCommandDirectionFor0To9Dials(desiredPositionDial2X, _r863ManualCockpitFreq2DialPos);
                                        DCSBIOS.Send(str);
                                        dial2SendCount++;
                                        Interlocked.Exchange(ref _r863ManualDial2WaitingForFeedback, 1);
                                    }

                                    Reset(ref dial2Timeout);
                                }
                            }
                            else
                            {
                                dial2OkTime = DateTime.Now.Ticks;
                            }

                            if (Interlocked.Read(ref _r863ManualDial3WaitingForFeedback) == 0)
                            {
                                lock (_lockR863ManualDialsObject3)
                                {
                                    if (_r863ManualCockpitFreq3DialPos != desiredPositionDial3X)
                                    {
                                        dial3OkTime = DateTime.Now.Ticks;
                                        str = R863_MANUAL_FREQ_3DIAL_COMMAND + GetCommandDirectionFor0To9Dials(desiredPositionDial3X, _r863ManualCockpitFreq3DialPos);
                                        DCSBIOS.Send(str);
                                        dial3SendCount++;
                                        Interlocked.Exchange(ref _r863ManualDial3WaitingForFeedback, 1);
                                    }

                                    Reset(ref dial3Timeout);
                                }
                            }
                            else
                            {
                                dial3OkTime = DateTime.Now.Ticks;
                            }

                            if (Interlocked.Read(ref _r863ManualDial4WaitingForFeedback) == 0)
                            {
                                lock (_lockR863ManualDialsObject4)
                                {
                                    if (_r863ManualCockpitFreq4DialPos < desiredPositionDial4X)
                                    {
                                        dial4OkTime = DateTime.Now.Ticks;
                                        str = R863_MANUAL_FREQ_4DIAL_COMMAND + "INC\n";
                                        DCSBIOS.Send(str);
                                        dial4SendCount++;
                                        Interlocked.Exchange(ref _r863ManualDial4WaitingForFeedback, 1);
                                    }
                                    else if (_r863ManualCockpitFreq4DialPos > desiredPositionDial4X)
                                    {
                                        dial4OkTime = DateTime.Now.Ticks;
                                        str = R863_MANUAL_FREQ_4DIAL_COMMAND + "DEC\n";
                                        DCSBIOS.Send(str);
                                        dial4SendCount++;
                                        Interlocked.Exchange(ref _r863ManualDial4WaitingForFeedback, 1);
                                    }

                                    Reset(ref dial4Timeout);
                                }
                            }
                            else
                            {
                                dial4OkTime = DateTime.Now.Ticks;
                            }

                            // if (dial1SendCount > 12 || dial2SendCount > 10 || dial3SendCount > 10 || dial4SendCount > 5)
                            /* ATTN ! 12.05.2019
                             * This radio is problematic, sometimes it goes bonkers, DCS-BIOS doesn't see the update from DCS
                             * and everything goes wrong after that. Been like this from start.
                             * Added big send counts here just to make sure it has time to find the correct position.
                             */
                            if (dial1SendCount > 40 || dial2SendCount > 20 || dial3SendCount > 20 || dial4SendCount > 10)
                            {
                                // "Race" condition detected?
                                dial1SendCount = 0;
                                dial2SendCount = 0;
                                dial3SendCount = 0;
                                dial4SendCount = 0;
                                Thread.Sleep(5000);
                            }

                            Thread.Sleep(SynchSleepTime); // Should be enough to get an update cycle from DCS-BIOS
                        }
                        while (IsTooShort(dial1OkTime) || IsTooShort(dial2OkTime) || IsTooShort(dial3OkTime) || IsTooShort(dial4OkTime));
                    }
                    catch (ThreadAbortException ex)
                    {
                        logger.Error(ex);
                    }
                    catch (Exception ex)
                    {
                        Common.ShowErrorMessageBox(ex);
                    }
                }
                finally
                {
                    Interlocked.Exchange(ref _r863ManualThreadNowSynching, 0);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }

            // Refresh panel once this debacle is finished
            Interlocked.Increment(ref _doUpdatePanelLCD);
            SwapCockpitStandbyFrequencyR863Manual();
            ShowFrequenciesOnPanel();
        }

        private void SwapCockpitStandbyFrequencyR863Manual()
        {
            try
            {
                _r863ManualBigFrequencyStandby = _r863ManualSavedCockpitBigFrequency;
                _r863ManualSmallFrequencyStandby = _r863ManualSavedCockpitSmallFrequency;
            }
            catch (Exception ex)
            {
                logger.Error(ex);
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

                _yadro1ASyncThread?.Abort();
                _yadro1ASyncThread = new Thread(() => YaDRO1ASynchThreadMethod());
                _yadro1ASyncThread.Start();
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
        }

        private void YaDRO1ASynchThreadMethod()
        {
            try
            {
                try
                {
                    try
                    {
                        /*
                                          * Mi-8 YaDRO-1A
                                          */
                        Interlocked.Exchange(ref _yadro1AThreadNowSynching, 1);
                        long dial1Timeout = DateTime.Now.Ticks;
                        long dial2Timeout = DateTime.Now.Ticks;
                        long dial3Timeout = DateTime.Now.Ticks;
                        long dial4Timeout = DateTime.Now.Ticks;
                        long dial1OkTime = 0;
                        long dial2OkTime = 0;
                        long dial3OkTime = 0;
                        long dial4OkTime = 0;
                        var dial1SendCount = 0;
                        var dial2SendCount = 0;
                        var dial3SendCount = 0;
                        var dial4SendCount = 0;

                        var frequencyAsString = _yadro1ABigFrequencyStandby.ToString().PadLeft(3, '0') + _yadro1ASmallFrequencyStandby.ToString().PadLeft(2, '0');
                        frequencyAsString = frequencyAsString.PadRight(6, '0');

                        // Frequency selector 1      YADRO1A_FREQ1
                        // "02" "03" "04" "05" "06" "07" "08" "09" "10" "11" "12" "13" "14" "15" "16" "17"
                        // Pos     0    1    2    3    4    5    6    7    8    9   10   11   12   13   14   15

                        // Frequency selector 2      YADRO1A_FREQ2
                        // 0 1 2 3 4 5 6 7 8 9

                        // Frequency selector 3      YADRO1A_FREQ3
                        // 0 1 2 3 4 5 6 7 8 9

                        // Frequency selector 4      YADRO1A_FREQ4
                        // 0 1 2 3 4 5 6 7 8 9

                        // Reason for this is to separate the standby frequency from the sync loop
                        // If not the sync would pick up any changes made by the user during the
                        // sync process
                        var desiredPositionDial1X = 0;
                        var desiredPositionDial2X = 0;
                        var desiredPositionDial3X = 0;
                        var desiredPositionDial4X = 0;

                        // 02000
                        // 17999
                        // #1 = 17  (position = value)
                        // #2 = 9   (position = value)
                        // #3 = 9   (position = value)
                        // #4 = 9   (position = value)
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
                                ResetWaitingForFeedBack(ref _yadro1ADial2WaitingForFeedback); // Lets do an ugly reset
                            }

                            if (IsTimedOut(ref dial3Timeout))
                            {
                                ResetWaitingForFeedBack(ref _yadro1ADial3WaitingForFeedback); // Lets do an ugly reset
                            }

                            if (IsTimedOut(ref dial4Timeout))
                            {
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
                                        dial1SendCount++;
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
                                        dial2SendCount++;
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
                                        dial3SendCount++;
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
                                        dial4SendCount++;
                                        Interlocked.Exchange(ref _yadro1ADial4WaitingForFeedback, 1);
                                    }

                                    Reset(ref dial4Timeout);
                                }
                            }
                            else
                            {
                                dial4OkTime = DateTime.Now.Ticks;
                            }

                            Thread.Sleep(SynchSleepTime); // Should be enough to get an update cycle from DCS-BIOS
                        }
                        while (IsTooShort(dial1OkTime) || IsTooShort(dial2OkTime) || IsTooShort(dial3OkTime) || IsTooShort(dial4OkTime));

                        SwapCockpitStandbyFrequencyYaDRO1A();
                        ShowFrequenciesOnPanel();
                    }
                    catch (ThreadAbortException)
                    {
                    }
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

            // Refresh panel once this debacle is finished
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
            try
            {
                Interlocked.Increment(ref _doUpdatePanelLCD);
                lock (LockLCDUpdateObject)
                {
                    foreach (var radioPanelKnobObject in hashSet)
                    {
                        var radioPanelKnob = (RadioPanelKnobMi8)radioPanelKnobObject;

                        switch (radioPanelKnob.RadioPanelPZ69Knob)
                        {
                            case RadioPanelPZ69KnobsMi8.UPPER_R863_MANUAL:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetUpperRadioMode(CurrentMi8RadioMode.R863_MANUAL);
                                    }

                                    break;
                                }

                            case RadioPanelPZ69KnobsMi8.UPPER_R863_PRESET:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetUpperRadioMode(CurrentMi8RadioMode.R863_PRESET);
                                    }

                                    break;
                                }

                            case RadioPanelPZ69KnobsMi8.UPPER_YADRO1A:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetUpperRadioMode(CurrentMi8RadioMode.YADRO1A);
                                    }

                                    break;
                                }

                            case RadioPanelPZ69KnobsMi8.UPPER_R828:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetUpperRadioMode(CurrentMi8RadioMode.R828_PRESETS);
                                    }

                                    break;
                                }

                            case RadioPanelPZ69KnobsMi8.UPPER_ADF_ARK9:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetUpperRadioMode(CurrentMi8RadioMode.ADF_ARK9);
                                    }

                                    break;
                                }

                            case RadioPanelPZ69KnobsMi8.UPPER_SPU7:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetUpperRadioMode(CurrentMi8RadioMode.SPU7);
                                    }

                                    break;
                                }

                            case RadioPanelPZ69KnobsMi8.LOWER_R863_MANUAL:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetLowerRadioMode(CurrentMi8RadioMode.R863_MANUAL);
                                    }

                                    break;
                                }

                            case RadioPanelPZ69KnobsMi8.LOWER_R863_PRESET:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetLowerRadioMode(CurrentMi8RadioMode.R863_PRESET);
                                    }

                                    break;
                                }

                            case RadioPanelPZ69KnobsMi8.LOWER_YADRO1A:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetLowerRadioMode(CurrentMi8RadioMode.YADRO1A);
                                    }

                                    break;
                                }

                            case RadioPanelPZ69KnobsMi8.LOWER_R828:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetLowerRadioMode(CurrentMi8RadioMode.R828_PRESETS);
                                    }

                                    break;
                                }

                            case RadioPanelPZ69KnobsMi8.LOWER_ADF_ARK9:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetLowerRadioMode(CurrentMi8RadioMode.ADF_ARK9);
                                    }

                                    break;
                                }

                            case RadioPanelPZ69KnobsMi8.LOWER_SPU7:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetLowerRadioMode(CurrentMi8RadioMode.SPU7);
                                    }

                                    break;
                                }

                            case RadioPanelPZ69KnobsMi8.UPPER_ARK_UD:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetUpperRadioMode(CurrentMi8RadioMode.ARK_UD);
                                    }

                                    break;
                                }

                            case RadioPanelPZ69KnobsMi8.LOWER_ARK_UD:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetLowerRadioMode(CurrentMi8RadioMode.ARK_UD);
                                    }

                                    break;
                                }

                            case RadioPanelPZ69KnobsMi8.UPPER_LARGE_FREQ_WHEEL_INC:
                            case RadioPanelPZ69KnobsMi8.UPPER_LARGE_FREQ_WHEEL_DEC:
                            case RadioPanelPZ69KnobsMi8.UPPER_SMALL_FREQ_WHEEL_INC:
                            case RadioPanelPZ69KnobsMi8.UPPER_SMALL_FREQ_WHEEL_DEC:
                            case RadioPanelPZ69KnobsMi8.LOWER_LARGE_FREQ_WHEEL_INC:
                            case RadioPanelPZ69KnobsMi8.LOWER_LARGE_FREQ_WHEEL_DEC:
                            case RadioPanelPZ69KnobsMi8.LOWER_SMALL_FREQ_WHEEL_INC:
                            case RadioPanelPZ69KnobsMi8.LOWER_SMALL_FREQ_WHEEL_DEC:
                                {
                                    // Ignore
                                    break;
                                }

                            case RadioPanelPZ69KnobsMi8.UPPER_FREQ_SWITCH:
                                {
                                    if (_currentUpperRadioMode == CurrentMi8RadioMode.R863_PRESET)
                                    {
                                        if (radioPanelKnob.IsOn)
                                        {
                                            DCSBIOS.Send(R863_UNIT_SWITCH_COMMAND_TOGGLE);
                                        }
                                    }
                                    else if (_currentUpperRadioMode == CurrentMi8RadioMode.ADF_ARK9 && radioPanelKnob.IsOn)
                                    {
                                        DCSBIOS.Send(ADF_BACKUP_MAIN_SWITCH_TOGGLE_COMMAND);
                                    }
                                    else if (_currentUpperRadioMode == CurrentMi8RadioMode.ARK_UD && radioPanelKnob.IsOn)
                                    {
                                        DCSBIOS.Send(ARKUD_VHF_UHF_MODE_COMMAND_TOGGLE);
                                    }
                                    else
                                    {
                                        SendFrequencyToDCSBIOS(radioPanelKnob.IsOn, RadioPanelPZ69KnobsMi8.UPPER_FREQ_SWITCH);
                                    }

                                    break;
                                }

                            case RadioPanelPZ69KnobsMi8.LOWER_FREQ_SWITCH:
                                {
                                    if (_currentLowerRadioMode == CurrentMi8RadioMode.R863_PRESET)
                                    {
                                        if (radioPanelKnob.IsOn)
                                        {
                                            DCSBIOS.Send(R863_UNIT_SWITCH_COMMAND_TOGGLE);
                                        }
                                    }
                                    else if (_currentLowerRadioMode == CurrentMi8RadioMode.ADF_ARK9 && radioPanelKnob.IsOn)
                                    {
                                        DCSBIOS.Send(ADF_BACKUP_MAIN_SWITCH_TOGGLE_COMMAND);
                                    }
                                    else if (_currentLowerRadioMode == CurrentMi8RadioMode.ARK_UD && radioPanelKnob.IsOn)
                                    {
                                        DCSBIOS.Send(ARKUD_VHF_UHF_MODE_COMMAND_TOGGLE);
                                    }
                                    else
                                    {
                                        SendFrequencyToDCSBIOS(radioPanelKnob.IsOn, RadioPanelPZ69KnobsMi8.LOWER_FREQ_SWITCH);
                                    }

                                    break;
                                }
                        }

                        if (PluginManager.PlugSupportActivated && PluginManager.HasPlugin())
                        {
                            PluginManager.DoEvent(
                                ProfileHandler.SelectedProfile().Description,
                                HIDInstanceId,
                                (int)PluginGamingPanelEnum.PZ69RadioPanel,
                                (int)radioPanelKnob.RadioPanelPZ69Knob,
                                radioPanelKnob.IsOn,
                                null);
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
                    var radioPanelKnobMi8 = (RadioPanelKnobMi8)o;
                    if (radioPanelKnobMi8.IsOn)
                    {
                        switch (radioPanelKnobMi8.RadioPanelPZ69Knob)
                        {
                            case RadioPanelPZ69KnobsMi8.UPPER_LARGE_FREQ_WHEEL_INC:
                                {
                                    switch (_currentUpperRadioMode)
                                    {
                                        case CurrentMi8RadioMode.R863_MANUAL:
                                            {
                                                var changeFaster = false;
                                                _bigFreqIncreaseChangeMonitor.Click();
                                                if (_bigFreqIncreaseChangeMonitor.ClickThresholdReached())
                                                {
                                                    // Change faster
                                                    changeFaster = true;
                                                }
                                                
                                                if (changeFaster)
                                                {
                                                    _r863ManualBigFrequencyStandby = _r863ManualBigFrequencyStandby + CHANGE_VALUE;
                                                }
                                                else
                                                {
                                                    _r863ManualBigFrequencyStandby++;
                                                }

                                                if (_r863ManualBigFrequencyStandby > 149 && _r863ManualBigFrequencyStandby < 220)
                                                {
                                                    _r863ManualBigFrequencyStandby = 220;
                                                }

                                                // 100-399-100
                                                if (_r863ManualBigFrequencyStandby > 399)
                                                {
                                                    _r863ManualBigFrequencyStandby = 100;
                                                }

                                                break;
                                            }

                                        case CurrentMi8RadioMode.R863_PRESET:
                                            {
                                                if (!SkipR863PresetDialChange())
                                                {
                                                    DCSBIOS.Send(R863_PRESET_COMMAND_INC);
                                                }

                                                break;
                                            }

                                        case CurrentMi8RadioMode.YADRO1A:
                                            {
                                                var changeFaster = false;
                                                _yadro1ABigFreqIncreaseChangeMonitor.Click();
                                                if (_yadro1ABigFreqIncreaseChangeMonitor.ClickThresholdReached())
                                                {
                                                    // Change faster
                                                    changeFaster = true;
                                                }

                                                if (changeFaster)
                                                {
                                                    _yadro1ABigFrequencyStandby = _yadro1ABigFrequencyStandby + CHANGE_VALUE;
                                                }
                                                else
                                                {
                                                    _yadro1ABigFrequencyStandby++;
                                                }

                                                // 20-179-20
                                                if (_yadro1ABigFrequencyStandby > 179)
                                                {
                                                    _yadro1ABigFrequencyStandby = 20;
                                                }
                                                
                                                break;
                                            }

                                        case CurrentMi8RadioMode.R828_PRESETS:
                                            {
                                                if (!SkipR828PresetDialChange())
                                                {
                                                    DCSBIOS.Send(R828_PRESET_COMMAND_INC);
                                                }

                                                break;
                                            }

                                        case CurrentMi8RadioMode.ADF_ARK9:
                                            {
                                                if (!SkipADFPresetDial1Change())
                                                {
                                                    DCSBIOS.Send(_adfBackupMainCockpitDial1Pos == 1 ? ADF_MAIN100_KHZ_PRESET_COMMAND_INC : ADF_BACKUP100_KHZ_PRESET_COMMAND_INC);
                                                }

                                                break;
                                            }

                                        case CurrentMi8RadioMode.ARK_UD:
                                            {
                                                if (!SkipArkudPresetDialChange())
                                                {
                                                    DCSBIOS.Send(ARKUD_PRESET_COMMAND_INC);
                                                }

                                                break;
                                            }

                                        case CurrentMi8RadioMode.SPU7:
                                            {
                                                if (!SkipSpu7PresetDialChange())
                                                {
                                                    DCSBIOS.Send(SPU7_COMMAND_INC);
                                                }

                                                break;
                                            }

                                        case CurrentMi8RadioMode.NOUSE:
                                            {
                                                break;
                                            }
                                    }

                                    break;
                                }

                            case RadioPanelPZ69KnobsMi8.UPPER_LARGE_FREQ_WHEEL_DEC:
                                {
                                    switch (_currentUpperRadioMode)
                                    {
                                        case CurrentMi8RadioMode.R863_MANUAL:
                                            {
                                                var changeFaster = false;
                                                _bigFreqDecreaseChangeMonitor.Click();
                                                if (_bigFreqDecreaseChangeMonitor.ClickThresholdReached())
                                                {
                                                    // Change faster
                                                    changeFaster = true;
                                                }

                                                if (changeFaster)
                                                {
                                                    _r863ManualBigFrequencyStandby = _r863ManualBigFrequencyStandby - CHANGE_VALUE;
                                                }
                                                else
                                                {
                                                    _r863ManualBigFrequencyStandby--;
                                                }

                                                if (_r863ManualBigFrequencyStandby < 220 && _r863ManualBigFrequencyStandby > 149)
                                                {
                                                    _r863ManualBigFrequencyStandby = 149;
                                                }

                                                // 100-399-100
                                                if (_r863ManualBigFrequencyStandby < 100)
                                                {
                                                    _r863ManualBigFrequencyStandby = 399;
                                                }
                                                
                                                break;
                                            }

                                        case CurrentMi8RadioMode.R863_PRESET:
                                            {
                                                if (!SkipR863PresetDialChange())
                                                {
                                                    DCSBIOS.Send(R863_PRESET_COMMAND_DEC);
                                                }

                                                break;
                                            }

                                        case CurrentMi8RadioMode.YADRO1A:
                                            {
                                                var changeFaster = false;
                                                _yadro1ABigFreqDecreaseChangeMonitor.Click();
                                                if (_yadro1ABigFreqDecreaseChangeMonitor.ClickThresholdReached())
                                                {
                                                    // Change faster
                                                    changeFaster = true;
                                                }

                                                if (changeFaster)
                                                {
                                                    _yadro1ABigFrequencyStandby = _yadro1ABigFrequencyStandby - CHANGE_VALUE;
                                                }
                                                else
                                                {
                                                    _yadro1ABigFrequencyStandby--;
                                                }
                                                
                                                // 20-179-20
                                                if (_yadro1ABigFrequencyStandby < 20)
                                                {
                                                    _yadro1ABigFrequencyStandby = 179;
                                                }

                                                break;
                                            }

                                        case CurrentMi8RadioMode.R828_PRESETS:
                                            {
                                                if (!SkipR828PresetDialChange())
                                                {
                                                    DCSBIOS.Send(R828_PRESET_COMMAND_DEC);
                                                }

                                                break;
                                            }

                                        case CurrentMi8RadioMode.ADF_ARK9:
                                            {
                                                if (!SkipADFPresetDial1Change())
                                                {
                                                    DCSBIOS.Send(_adfBackupMainCockpitDial1Pos == 1 ? ADF_MAIN100_KHZ_PRESET_COMMAND_DEC : ADF_BACKUP100_KHZ_PRESET_COMMAND_DEC);
                                                }

                                                break;
                                            }

                                        case CurrentMi8RadioMode.ARK_UD:
                                            {
                                                if (!SkipArkudPresetDialChange())
                                                {
                                                    DCSBIOS.Send(ARKUD_PRESET_COMMAND_DEC);
                                                }

                                                break;
                                            }

                                        case CurrentMi8RadioMode.SPU7:
                                            {
                                                if (!SkipSpu7PresetDialChange())
                                                {
                                                    DCSBIOS.Send(SPU7_COMMAND_DEC);
                                                }

                                                break;
                                            }

                                        case CurrentMi8RadioMode.NOUSE:
                                            {
                                                break;
                                            }
                                    }

                                    break;
                                }

                            case RadioPanelPZ69KnobsMi8.UPPER_SMALL_FREQ_WHEEL_INC:
                                {
                                    switch (_currentUpperRadioMode)
                                    {
                                        case CurrentMi8RadioMode.R863_MANUAL:
                                            {
                                                if (_r863ManualSmallFrequencyStandby >= 975)
                                                {
                                                    // At max value
                                                    _r863ManualSmallFrequencyStandby = 0;
                                                    break;
                                                }

                                                _r863ManualSmallFrequencyStandby = _r863ManualSmallFrequencyStandby + 25;
                                                break;
                                            }

                                        case CurrentMi8RadioMode.R863_PRESET:
                                            {
                                                DCSBIOS.Send(R863_PRESET_VOLUME_KNOB_COMMAND_INC);
                                                break;
                                            }

                                        case CurrentMi8RadioMode.YADRO1A:
                                            {
                                                if (_yadro1ASmallFrequencyStandby >= 99)
                                                {
                                                    // At max value
                                                    _yadro1ASmallFrequencyStandby = 0;
                                                    break;
                                                }

                                                _yadro1ASmallFrequencyStandby = _yadro1ASmallFrequencyStandby + 1;
                                                break;
                                            }

                                        case CurrentMi8RadioMode.R828_PRESETS:
                                            {
                                                DCSBIOS.Send(R828_PRESET_VOLUME_KNOB_COMMAND_INC);
                                                break;
                                            }

                                        case CurrentMi8RadioMode.ADF_ARK9:
                                            {
                                                if (!SkipADFPresetDial2Change())
                                                {
                                                    DCSBIOS.Send(_adfBackupMainCockpitDial1Pos == 1 ? ADF_MAIN10_KHZ_PRESET_COMMAND_INC : ADF_BACKUP10_KHZ_PRESET_COMMAND_INC);
                                                }

                                                break;
                                            }

                                        case CurrentMi8RadioMode.ARK_UD:
                                            {
                                                if (!SkipArkudModeDialChange())
                                                {
                                                    DCSBIOS.Send(ARKUD_MODE_COMMAND_INC);
                                                }

                                                break;
                                            }

                                        case CurrentMi8RadioMode.SPU7:
                                            {
                                                DCSBIOS.Send(SPU7_VOLUME_KNOB_COMMAND_INC);
                                                break;
                                            }

                                        case CurrentMi8RadioMode.NOUSE:
                                            {
                                                break;
                                            }
                                    }

                                    break;
                                }

                            case RadioPanelPZ69KnobsMi8.UPPER_SMALL_FREQ_WHEEL_DEC:
                                {
                                    switch (_currentUpperRadioMode)
                                    {
                                        case CurrentMi8RadioMode.R863_MANUAL:
                                            {
                                                if (_r863ManualSmallFrequencyStandby <= 0)
                                                {
                                                    // At min value
                                                    _r863ManualSmallFrequencyStandby = 975;
                                                    break;
                                                }

                                                _r863ManualSmallFrequencyStandby = _r863ManualSmallFrequencyStandby - 25;
                                                break;
                                            }

                                        case CurrentMi8RadioMode.R863_PRESET:
                                            {
                                                DCSBIOS.Send(R863_PRESET_VOLUME_KNOB_COMMAND_DEC);
                                                break;
                                            }

                                        case CurrentMi8RadioMode.YADRO1A:
                                            {
                                                if (_yadro1ASmallFrequencyStandby <= 0)
                                                {
                                                    // At min value
                                                    _yadro1ASmallFrequencyStandby = 99;
                                                    break;
                                                }

                                                _yadro1ASmallFrequencyStandby = _yadro1ASmallFrequencyStandby - 1;
                                                break;
                                            }

                                        case CurrentMi8RadioMode.R828_PRESETS:
                                            {
                                                DCSBIOS.Send(R828_PRESET_VOLUME_KNOB_COMMAND_DEC);
                                                break;
                                            }

                                        case CurrentMi8RadioMode.ADF_ARK9:
                                            {
                                                if (!SkipADFPresetDial2Change())
                                                {
                                                    DCSBIOS.Send(_adfBackupMainCockpitDial1Pos == 1 ? ADF_MAIN10_KHZ_PRESET_COMMAND_DEC : ADF_BACKUP10_KHZ_PRESET_COMMAND_DEC);
                                                }

                                                break;
                                            }

                                        case CurrentMi8RadioMode.ARK_UD:
                                            {
                                                if (!SkipArkudModeDialChange())
                                                {
                                                    DCSBIOS.Send(ARKUD_MODE_COMMAND_DEC);
                                                }

                                                break;
                                            }

                                        case CurrentMi8RadioMode.SPU7:
                                            {
                                                DCSBIOS.Send(SPU7_VOLUME_KNOB_COMMAND_DEC);
                                                break;
                                            }

                                        case CurrentMi8RadioMode.NOUSE:
                                            {
                                                break;
                                            }
                                    }

                                    break;
                                }

                            case RadioPanelPZ69KnobsMi8.LOWER_LARGE_FREQ_WHEEL_INC:
                                {
                                    switch (_currentLowerRadioMode)
                                    {
                                        case CurrentMi8RadioMode.R863_MANUAL:
                                            {
                                                var changeFaster = false;
                                                _bigFreqIncreaseChangeMonitor.Click();
                                                if (_bigFreqIncreaseChangeMonitor.ClickThresholdReached())
                                                {
                                                    // Change faster
                                                    changeFaster = true;
                                                }

                                                if (changeFaster)
                                                {
                                                    _r863ManualBigFrequencyStandby = _r863ManualBigFrequencyStandby + CHANGE_VALUE;
                                                }
                                                else
                                                {
                                                    _r863ManualBigFrequencyStandby++;
                                                }

                                                if (_r863ManualBigFrequencyStandby > 149 && _r863ManualBigFrequencyStandby < 220)
                                                {
                                                    _r863ManualBigFrequencyStandby = 220;
                                                }
                                                
                                                // 100-399-100
                                                if (_r863ManualBigFrequencyStandby > 399)
                                                {
                                                    _r863ManualBigFrequencyStandby = 100;
                                                }

                                                break;
                                            }

                                        case CurrentMi8RadioMode.R863_PRESET:
                                            {
                                                if (!SkipR863PresetDialChange())
                                                {
                                                    DCSBIOS.Send(R863_PRESET_COMMAND_INC);
                                                }

                                                break;
                                            }

                                        case CurrentMi8RadioMode.YADRO1A:
                                            {
                                                var changeFaster = false;
                                                _yadro1ABigFreqIncreaseChangeMonitor.Click();
                                                if (_yadro1ABigFreqIncreaseChangeMonitor.ClickThresholdReached())
                                                {
                                                    // Change faster
                                                    changeFaster = true;
                                                }

                                                if (changeFaster)
                                                {
                                                    _yadro1ABigFrequencyStandby = _yadro1ABigFrequencyStandby + CHANGE_VALUE;
                                                }
                                                else
                                                {
                                                    _yadro1ABigFrequencyStandby++;
                                                }

                                                // 20-179-20
                                                if (_yadro1ABigFrequencyStandby > 179)
                                                {
                                                    _yadro1ABigFrequencyStandby = 20;
                                                }

                                                break;
                                            }

                                        case CurrentMi8RadioMode.R828_PRESETS:
                                            {
                                                if (!SkipR828PresetDialChange())
                                                {
                                                    DCSBIOS.Send(R828_PRESET_COMMAND_INC);
                                                }

                                                break;
                                            }

                                        case CurrentMi8RadioMode.ADF_ARK9:
                                            {
                                                if (!SkipADFPresetDial1Change())
                                                {
                                                    DCSBIOS.Send(_adfBackupMainCockpitDial1Pos == 1 ? ADF_MAIN100_KHZ_PRESET_COMMAND_INC : ADF_BACKUP100_KHZ_PRESET_COMMAND_INC);
                                                }

                                                break;
                                            }

                                        case CurrentMi8RadioMode.ARK_UD:
                                            {
                                                if (!SkipArkudPresetDialChange())
                                                {
                                                    DCSBIOS.Send(ARKUD_PRESET_COMMAND_INC);
                                                }

                                                break;
                                            }

                                        case CurrentMi8RadioMode.SPU7:
                                            {
                                                if (!SkipSpu7PresetDialChange())
                                                {
                                                    DCSBIOS.Send(SPU7_COMMAND_INC);
                                                }

                                                break;
                                            }

                                        case CurrentMi8RadioMode.NOUSE:
                                            {
                                                break;
                                            }
                                    }

                                    break;
                                }

                            case RadioPanelPZ69KnobsMi8.LOWER_LARGE_FREQ_WHEEL_DEC:
                                {
                                    switch (_currentLowerRadioMode)
                                    {
                                        case CurrentMi8RadioMode.R863_MANUAL:
                                            {
                                                var changeFaster = false;
                                                _bigFreqDecreaseChangeMonitor.Click();
                                                if (_bigFreqDecreaseChangeMonitor.ClickThresholdReached())
                                                {
                                                    // Change faster
                                                    changeFaster = true;
                                                }

                                                if (changeFaster)
                                                {
                                                    _r863ManualBigFrequencyStandby = _r863ManualBigFrequencyStandby - CHANGE_VALUE;
                                                }
                                                else
                                                {
                                                    _r863ManualBigFrequencyStandby--;
                                                }

                                                if (_r863ManualBigFrequencyStandby < 220 && _r863ManualBigFrequencyStandby > 149)
                                                {
                                                    _r863ManualBigFrequencyStandby = 149;
                                                }
                                                
                                                // 100-399-100
                                                if (_r863ManualBigFrequencyStandby < 100)
                                                {
                                                    _r863ManualBigFrequencyStandby = 399;
                                                }

                                                break;
                                            }

                                        case CurrentMi8RadioMode.R863_PRESET:
                                            {
                                                if (!SkipR863PresetDialChange())
                                                {
                                                    DCSBIOS.Send(R863_PRESET_COMMAND_DEC);
                                                }

                                                break;
                                            }

                                        case CurrentMi8RadioMode.YADRO1A:
                                            {
                                                var changeFaster = false;
                                                _yadro1ABigFreqDecreaseChangeMonitor.Click();
                                                if (_yadro1ABigFreqDecreaseChangeMonitor.ClickThresholdReached())
                                                {
                                                    // Change faster
                                                    changeFaster = true;
                                                }

                                                if (changeFaster)
                                                {
                                                    _yadro1ABigFrequencyStandby = _yadro1ABigFrequencyStandby - CHANGE_VALUE;
                                                }
                                                else
                                                {
                                                    _yadro1ABigFrequencyStandby--;
                                                }

                                                // 20-179-20
                                                if (_yadro1ABigFrequencyStandby < 20)
                                                {
                                                    _yadro1ABigFrequencyStandby = 179;
                                                }

                                                break;
                                            }

                                        case CurrentMi8RadioMode.R828_PRESETS:
                                            {
                                                if (!SkipR828PresetDialChange())
                                                {
                                                    DCSBIOS.Send(R828_PRESET_COMMAND_DEC);
                                                }

                                                break;
                                            }

                                        case CurrentMi8RadioMode.ADF_ARK9:
                                            {
                                                if (!SkipADFPresetDial1Change())
                                                {
                                                    DCSBIOS.Send(_adfBackupMainCockpitDial1Pos == 1 ? ADF_MAIN100_KHZ_PRESET_COMMAND_DEC : ADF_BACKUP100_KHZ_PRESET_COMMAND_DEC);
                                                }

                                                break;
                                            }

                                        case CurrentMi8RadioMode.ARK_UD:
                                            {
                                                if (!SkipArkudPresetDialChange())
                                                {
                                                    DCSBIOS.Send(ARKUD_PRESET_COMMAND_DEC);
                                                }

                                                break;
                                            }

                                        case CurrentMi8RadioMode.SPU7:
                                            {
                                                if (!SkipSpu7PresetDialChange())
                                                {
                                                    DCSBIOS.Send(SPU7_COMMAND_DEC);
                                                }

                                                break;
                                            }

                                        case CurrentMi8RadioMode.NOUSE:
                                            {
                                                break;
                                            }
                                    }

                                    break;
                                }

                            case RadioPanelPZ69KnobsMi8.LOWER_SMALL_FREQ_WHEEL_INC:
                                {
                                    switch (_currentLowerRadioMode)
                                    {
                                        case CurrentMi8RadioMode.R863_MANUAL:
                                            {
                                                if (_r863ManualSmallFrequencyStandby >= 975)
                                                {
                                                    // At max value
                                                    _r863ManualSmallFrequencyStandby = 0;
                                                    break;
                                                }

                                                _r863ManualSmallFrequencyStandby = _r863ManualSmallFrequencyStandby + 25;
                                                break;
                                            }

                                        case CurrentMi8RadioMode.R863_PRESET:
                                            {
                                                DCSBIOS.Send(R863_PRESET_VOLUME_KNOB_COMMAND_INC);
                                                break;
                                            }

                                        case CurrentMi8RadioMode.YADRO1A:
                                            {
                                                if (_yadro1ASmallFrequencyStandby >= 99)
                                                {
                                                    // At max value
                                                    _yadro1ASmallFrequencyStandby = 0;
                                                    break;
                                                }

                                                _yadro1ASmallFrequencyStandby = _yadro1ASmallFrequencyStandby + 1;
                                                break;
                                            }

                                        case CurrentMi8RadioMode.R828_PRESETS:
                                            {
                                                DCSBIOS.Send(R828_PRESET_VOLUME_KNOB_COMMAND_INC);
                                                break;
                                            }

                                        case CurrentMi8RadioMode.ADF_ARK9:
                                            {
                                                if (!SkipADFPresetDial2Change())
                                                {
                                                    DCSBIOS.Send(_adfBackupMainCockpitDial1Pos == 1 ? ADF_MAIN10_KHZ_PRESET_COMMAND_INC : ADF_BACKUP10_KHZ_PRESET_COMMAND_INC);
                                                }

                                                break;
                                            }

                                        case CurrentMi8RadioMode.ARK_UD:
                                            {
                                                if (!SkipArkudModeDialChange())
                                                {
                                                    DCSBIOS.Send(ARKUD_MODE_COMMAND_INC);
                                                }

                                                break;
                                            }

                                        case CurrentMi8RadioMode.SPU7:
                                            {
                                                DCSBIOS.Send(SPU7_VOLUME_KNOB_COMMAND_INC);
                                                break;
                                            }

                                        case CurrentMi8RadioMode.NOUSE:
                                            {
                                                break;
                                            }
                                    }

                                    break;
                                }

                            case RadioPanelPZ69KnobsMi8.LOWER_SMALL_FREQ_WHEEL_DEC:
                                {
                                    switch (_currentLowerRadioMode)
                                    {
                                        case CurrentMi8RadioMode.R863_MANUAL:
                                            {
                                                if (_r863ManualSmallFrequencyStandby <= 0)
                                                {
                                                    // At min value
                                                    _r863ManualSmallFrequencyStandby = 975;
                                                    break;
                                                }
                                                
                                                _r863ManualSmallFrequencyStandby = _r863ManualSmallFrequencyStandby - 25;
                                                break;
                                            }

                                        case CurrentMi8RadioMode.R863_PRESET:
                                            {
                                                DCSBIOS.Send(R863_PRESET_VOLUME_KNOB_COMMAND_DEC);
                                                break;
                                            }

                                        case CurrentMi8RadioMode.YADRO1A:
                                            {
                                                if (_yadro1ASmallFrequencyStandby <= 0)
                                                {
                                                    // At min value
                                                    _yadro1ASmallFrequencyStandby = 99;
                                                    break;
                                                }

                                                _yadro1ASmallFrequencyStandby = _yadro1ASmallFrequencyStandby - 1;
                                                break;
                                            }

                                        case CurrentMi8RadioMode.R828_PRESETS:
                                            {
                                                DCSBIOS.Send(R828_PRESET_VOLUME_KNOB_COMMAND_DEC);
                                                break;
                                            }

                                        case CurrentMi8RadioMode.ADF_ARK9:
                                            {
                                                if (!SkipADFPresetDial2Change())
                                                {
                                                    DCSBIOS.Send(_adfBackupMainCockpitDial1Pos == 1 ? ADF_MAIN10_KHZ_PRESET_COMMAND_DEC : ADF_BACKUP10_KHZ_PRESET_COMMAND_DEC);
                                                }

                                                break;
                                            }

                                        case CurrentMi8RadioMode.ARK_UD:
                                            {
                                                if (!SkipArkudModeDialChange())
                                                {
                                                    DCSBIOS.Send(ARKUD_MODE_COMMAND_DEC);
                                                }

                                                break;
                                            }

                                        case CurrentMi8RadioMode.SPU7:
                                            {
                                                DCSBIOS.Send(SPU7_VOLUME_KNOB_COMMAND_DEC);
                                                break;
                                            }

                                        case CurrentMi8RadioMode.NOUSE:
                                            {
                                                break;
                                            }
                                    }

                                    break;
                                }
                        }
                    }
                }

                ShowFrequenciesOnPanel(true);
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void CheckFrequenciesForValidity()
        {
            try
            {
                // Crude fix if any freqs are outside the valid boundaries

                // R-800L VHF 2
                // 100.00 - 149.00
                // 220.00 - 399.00
                if (_r863ManualBigFrequencyStandby < 100)
                {
                    _r863ManualBigFrequencyStandby = 100;
                }

                if (_r863ManualBigFrequencyStandby > 399)
                {
                    _r863ManualBigFrequencyStandby = 399;
                }

                /*if (_r863ManualBigFrequencyStandby == 399 && _r863ManualSmallFrequencyStandby > 0)
                {
                    _r863ManualSmallFrequencyStandby = 0;
                }

                if (_r863ManualBigFrequencyStandby == 149 && _r863ManualSmallFrequencyStandby > 0)
                {
                    _r863ManualSmallFrequencyStandby = 0;
                }*/
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
        }

        private void ShowFrequenciesOnPanel(bool force = false)
        {
            try
            {
                lock (_lockShowFrequenciesOnPanelObject)
                {
                    if (!force && Interlocked.Read(ref _doUpdatePanelLCD) == 0)
                    {
                        return;
                    }

                    if (!FirstReportHasBeenRead)
                    {
                        return;
                    }

                    CheckFrequenciesForValidity();
                    var bytes = new byte[21];
                    bytes[0] = 0x0;

                    switch (_currentUpperRadioMode)
                    {
                        case CurrentMi8RadioMode.R863_MANUAL:
                            {
                                var frequencyAsString = string.Empty;
                                lock (_lockR863ManualDialsObject1)
                                {
                                    frequencyAsString = _r863ManualCockpitFreq1DialPos.ToString(); // _r863ManualFreq1DialValues[_r863ManualCockpitFreq1DialPos].ToString();
                                }

                                lock (_lockR863ManualDialsObject2)
                                {
                                    frequencyAsString = frequencyAsString + _r863ManualCockpitFreq2DialPos;
                                }

                                frequencyAsString = frequencyAsString + ".";
                                lock (_lockR863ManualDialsObject3)
                                {
                                    frequencyAsString = frequencyAsString + _r863ManualCockpitFreq3DialPos;
                                }

                                lock (_lockR863ManualDialsObject4)
                                {
                                    frequencyAsString = frequencyAsString + _r863ManualCockpitFreq4DialPos.ToString().PadLeft(2, '0');

                                    // GetR863ManualDialFrequencyForPosition(_r863ManualCockpitFreq4DialPos);
                                }

                                SetPZ69DisplayBytesDefault(ref bytes, double.Parse(frequencyAsString, NumberFormatInfoFullDisplay), PZ69LCDPosition.UPPER_ACTIVE_LEFT);
                                SetPZ69DisplayBytesDefault(
                                    ref bytes,
                                    double.Parse(
                                        _r863ManualBigFrequencyStandby + "." + _r863ManualSmallFrequencyStandby.ToString(CultureInfo.InvariantCulture).PadLeft(3, '0'),
                                        NumberFormatInfoFullDisplay),
                                    PZ69LCDPosition.UPPER_STBY_RIGHT);
                                break;
                            }

                        case CurrentMi8RadioMode.R863_PRESET:
                            {
                                // Preset Channel Selector
                                // " 1" " 2" " 3" " 4" " 5" " 6" " 7" "8" "9" "10"
                                // Pos     0    1    2    3    4    5    6    7    8    9   10   11   12
                                var channelAsString = string.Empty;
                                uint unitSwitch = 0;
                                lock (_lockR863Preset1DialObject1)
                                {
                                    channelAsString = (_r863PresetCockpitDialPos + 1).ToString().PadLeft(2, ' ');
                                }

                                lock (_lockR863UnitSwitchObject)
                                {
                                    unitSwitch = _r863UnitSwitchCockpitPos;
                                }

                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, Convert.ToUInt32(channelAsString), PZ69LCDPosition.UPPER_STBY_RIGHT);
                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, unitSwitch, PZ69LCDPosition.UPPER_ACTIVE_LEFT);
                                break;
                            }

                        case CurrentMi8RadioMode.YADRO1A:
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
                                                SetPZ69DisplayBytesUnsignedInteger(
                                                    ref bytes,
                                                    Convert.ToUInt32(_yadro1ABigFrequencyStandby.ToString().PadLeft(3, '0') + _yadro1ASmallFrequencyStandby.ToString().PadLeft(2, '0')),
                                                    PZ69LCDPosition.UPPER_STBY_RIGHT);

                                                // SetPZ69DisplayBytesDefault(ref bytes, double.Parse(_yadro1aBigFrequencyStandby.ToString().PadLeft(3, '0') + _yadro1aSmallFrequencyStandby.ToString().PadLeft(2, '0'), NumberFormatInfoFullDisplay), PZ69LCDPosition.UPPER_RIGHT);
                                            }
                                        }
                                    }
                                }

                                break;
                            }

                        case CurrentMi8RadioMode.R828_PRESETS:
                            {
                                // Preset Channel Selector
                                // " 1" " 2" " 3" " 4" " 5" " 6" " 7" "8" "9" "10"
                                // Pos     0    1    2    3    4    5    6    7    8    9   10 
                                var channelAsString = string.Empty;
                                lock (_lockR828Preset1DialObject1)
                                {
                                    channelAsString = (_r828PresetCockpitDialPos + 1).ToString().PadLeft(2, ' ');
                                }

                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, Convert.ToUInt32(channelAsString), PZ69LCDPosition.UPPER_STBY_RIGHT);
                                SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.UPPER_ACTIVE_LEFT);
                                break;
                            }

                        case CurrentMi8RadioMode.ADF_ARK9:
                            {
                                // Dial1 XX00
                                // Dial2 00XX
                                var channelAsString = string.Empty;
                                uint backupMain = 0;
                                lock (_lockADFBackupMainDialObject)
                                {
                                    backupMain = _adfBackupMainCockpitDial1Pos;
                                    if (_adfBackupMainCockpitDial1Pos == 1)
                                    {
                                        lock (_lockADFMainDialObject1)
                                        {
                                            channelAsString = (_adfMainCockpitPresetDial1Pos + 1).ToString();
                                        }
                                    }
                                    else
                                    {
                                        lock (_lockADFBackupDialObject1)
                                        {
                                            channelAsString = (_adfBackupCockpitPresetDial1Pos + 1).ToString();
                                        }
                                    }

                                    if (_adfBackupMainCockpitDial1Pos == 1)
                                    {
                                        lock (_lockADFMainDialObject2)
                                        {
                                            channelAsString = channelAsString + _adfMainCockpitPresetDial2Pos.ToString().PadRight(2, '0');
                                        }
                                    }
                                    else
                                    {
                                        lock (_lockADFBackupDialObject2)
                                        {
                                            channelAsString = channelAsString + _adfBackupCockpitPresetDial2Pos.ToString().PadRight(2, '0');
                                        }
                                    }
                                }

                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, Convert.ToUInt32(channelAsString), PZ69LCDPosition.UPPER_STBY_RIGHT);
                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, backupMain, PZ69LCDPosition.UPPER_ACTIVE_LEFT);
                                break;
                            }

                        case CurrentMi8RadioMode.ARK_UD:
                            {
                                var stringToBeShownLeft = string.Empty;
                                uint arkPreset = 0;
                                uint arkMode = 0;
                                uint arkBand = 0;
                                lock (_lockArkudPresetDialObject)
                                {
                                    arkPreset = _arkUdPresetCockpitDial1Pos + 1;
                                }

                                lock (_lockArkudModeDialObject)
                                {
                                    arkMode = _arkUdModeCockpitDial1Pos;
                                }

                                lock (_lockArkudVhfUhfModeDialObject)
                                {
                                    arkBand = _arkUdVhfUhfModeCockpitDial1Pos;
                                }

                                // 1 4 5
                                // 12345
                                stringToBeShownLeft = arkBand + "   " + arkMode;
                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, arkPreset, PZ69LCDPosition.UPPER_STBY_RIGHT);
                                SetPZ69DisplayBytesDefault(ref bytes, stringToBeShownLeft, PZ69LCDPosition.UPPER_ACTIVE_LEFT);
                                break;
                            }

                        case CurrentMi8RadioMode.SPU7:
                            {
                                // 0-5
                                var channelAsString = string.Empty;
                                uint spuICSSwitch = 0;
                                lock (_lockSpu7DialObject1)
                                {
                                    channelAsString = (_spu7CockpitDialPos).ToString().PadLeft(2, ' ');
                                }

                                lock (_lockSpu7ICSSwitchObject)
                                {
                                    spuICSSwitch = _spu7ICSSwitchCockpitDialPos;
                                }

                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, Convert.ToUInt32(channelAsString), PZ69LCDPosition.UPPER_STBY_RIGHT);
                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, spuICSSwitch, PZ69LCDPosition.UPPER_ACTIVE_LEFT);
                                break;
                            }

                        case CurrentMi8RadioMode.NOUSE:
                            {
                                SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.UPPER_STBY_RIGHT);
                                SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.UPPER_ACTIVE_LEFT);
                                break;
                            }
                    }

                    switch (_currentLowerRadioMode)
                    {
                        case CurrentMi8RadioMode.R863_MANUAL:
                            {
                                var frequencyAsString = string.Empty;
                                lock (_lockR863ManualDialsObject1)
                                {
                                    frequencyAsString = _r863ManualCockpitFreq1DialPos.ToString(); // _r863ManualFreq1DialValues[_r863ManualCockpitFreq1DialPos].ToString();
                                }

                                lock (_lockR863ManualDialsObject2)
                                {
                                    frequencyAsString = frequencyAsString + _r863ManualCockpitFreq2DialPos;
                                }

                                frequencyAsString = frequencyAsString + ".";
                                lock (_lockR863ManualDialsObject3)
                                {
                                    frequencyAsString = frequencyAsString + _r863ManualCockpitFreq3DialPos;
                                }

                                lock (_lockR863ManualDialsObject4)
                                {
                                    frequencyAsString = frequencyAsString + _r863ManualCockpitFreq4DialPos.ToString().PadLeft(
                                                            2,
                                                            '0'); // GetR863ManualDialFrequencyForPosition(_r863ManualCockpitFreq4DialPos);
                                }

                                SetPZ69DisplayBytesDefault(ref bytes, double.Parse(frequencyAsString, NumberFormatInfoFullDisplay), PZ69LCDPosition.LOWER_ACTIVE_LEFT);
                                SetPZ69DisplayBytesDefault(
                                    ref bytes,
                                    double.Parse(
                                        _r863ManualBigFrequencyStandby + "." + _r863ManualSmallFrequencyStandby.ToString(CultureInfo.InvariantCulture).PadLeft(3, '0'),
                                        NumberFormatInfoFullDisplay),
                                    PZ69LCDPosition.LOWER_STBY_RIGHT);
                                break;
                            }

                        case CurrentMi8RadioMode.R863_PRESET:
                            {
                                // Preset Channel Selector
                                // " 1" " 2" " 3" " 4" " 5" " 6" " 7" "8" "9" "10"
                                // Pos     0    1    2    3    4    5    6    7    8    9   10   11   12
                                var channelAsString = string.Empty;
                                uint unitSwitch = 0;
                                lock (_lockR863Preset1DialObject1)
                                {
                                    channelAsString = (_r863PresetCockpitDialPos + 1).ToString().PadLeft(2, ' ');
                                }

                                lock (_lockR863UnitSwitchObject)
                                {
                                    unitSwitch = _r863UnitSwitchCockpitPos;
                                }

                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, Convert.ToUInt32(channelAsString), PZ69LCDPosition.LOWER_STBY_RIGHT);
                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, unitSwitch, PZ69LCDPosition.LOWER_ACTIVE_LEFT);
                                break;
                            }

                        case CurrentMi8RadioMode.YADRO1A:
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
                                                SetPZ69DisplayBytesUnsignedInteger(
                                                    ref bytes,
                                                    Convert.ToUInt32(_yadro1ABigFrequencyStandby.ToString().PadLeft(3, '0') + _yadro1ASmallFrequencyStandby.ToString().PadLeft(2, '0')),
                                                    PZ69LCDPosition.LOWER_STBY_RIGHT);

                                                // SetPZ69DisplayBytesDefault(ref bytes, double.Parse(_yadro1aBigFrequencyStandby.ToString().PadLeft(3, '0') + _yadro1aSmallFrequencyStandby.ToString().PadLeft(2, '0'), NumberFormatInfoFullDisplay), PZ69LCDPosition.UPPER_RIGHT);
                                            }
                                        }
                                    }
                                }

                                break;
                            }

                        case CurrentMi8RadioMode.R828_PRESETS:
                            {
                                // Preset Channel Selector
                                // " 1" " 2" " 3" " 4" " 5" " 6" " 7" "8" "9" "10"
                                // Pos     0    1    2    3    4    5    6    7    8    9   10 
                                var channelAsString = string.Empty;
                                lock (_lockR828Preset1DialObject1)
                                {
                                    channelAsString = (_r828PresetCockpitDialPos + 1).ToString().PadLeft(2, ' ');
                                }

                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, Convert.ToUInt32(channelAsString), PZ69LCDPosition.LOWER_STBY_RIGHT);
                                SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.LOWER_ACTIVE_LEFT);
                                break;
                            }

                        case CurrentMi8RadioMode.ADF_ARK9:
                            {
                                // Dial1 XX00
                                // Dial2 00XX
                                var channelAsString = string.Empty;
                                uint backupMain = 0;
                                lock (_lockADFBackupMainDialObject)
                                {
                                    backupMain = _adfBackupMainCockpitDial1Pos;
                                    if (_adfBackupMainCockpitDial1Pos == 1)
                                    {
                                        lock (_lockADFMainDialObject1)
                                        {
                                            channelAsString = (_adfMainCockpitPresetDial1Pos + 1).ToString();
                                        }
                                    }
                                    else
                                    {
                                        lock (_lockADFBackupDialObject1)
                                        {
                                            channelAsString = (_adfBackupCockpitPresetDial1Pos + 1).ToString();
                                        }
                                    }

                                    if (_adfBackupMainCockpitDial1Pos == 1)
                                    {
                                        lock (_lockADFMainDialObject2)
                                        {
                                            channelAsString = channelAsString + _adfMainCockpitPresetDial2Pos.ToString().PadRight(2, '0');
                                        }
                                    }
                                    else
                                    {
                                        lock (_lockADFBackupDialObject2)
                                        {
                                            channelAsString = channelAsString + _adfBackupCockpitPresetDial2Pos.ToString().PadRight(2, '0');
                                        }
                                    }
                                }

                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, Convert.ToUInt32(channelAsString), PZ69LCDPosition.LOWER_STBY_RIGHT);
                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, backupMain, PZ69LCDPosition.LOWER_ACTIVE_LEFT);
                                break;
                            }

                        case CurrentMi8RadioMode.ARK_UD:
                            {
                                var stringToBeShownLeft = string.Empty;
                                uint arkPreset = 0;
                                uint arkMode = 0;
                                uint arkBand = 0;
                                lock (_lockArkudPresetDialObject)
                                {
                                    arkPreset = _arkUdPresetCockpitDial1Pos + 1;
                                }

                                lock (_lockArkudModeDialObject)
                                {
                                    arkMode = _arkUdModeCockpitDial1Pos;
                                }

                                lock (_lockArkudVhfUhfModeDialObject)
                                {
                                    arkBand = _arkUdVhfUhfModeCockpitDial1Pos;
                                }

                                // 1 4 5
                                // 12345
                                stringToBeShownLeft = arkBand + "   " + arkMode;
                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, arkPreset, PZ69LCDPosition.LOWER_STBY_RIGHT);
                                SetPZ69DisplayBytesDefault(ref bytes, stringToBeShownLeft, PZ69LCDPosition.LOWER_ACTIVE_LEFT);
                                break;
                            }

                        case CurrentMi8RadioMode.SPU7:
                            {
                                // 0-5
                                var channelAsString = string.Empty;
                                uint spuICSSwitch = 0;
                                lock (_lockSpu7DialObject1)
                                {
                                    channelAsString = (_spu7CockpitDialPos).ToString().PadLeft(2, ' ');
                                }

                                lock (_lockSpu7ICSSwitchObject)
                                {
                                    spuICSSwitch = _spu7ICSSwitchCockpitDialPos;
                                }

                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, Convert.ToUInt32(channelAsString), PZ69LCDPosition.LOWER_STBY_RIGHT);
                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, spuICSSwitch, PZ69LCDPosition.LOWER_ACTIVE_LEFT);
                                break;
                            }

                        case CurrentMi8RadioMode.NOUSE:
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

        protected override void GamingPanelKnobChanged(bool isFirstReport, IEnumerable<object> hashSet)
        {
            PZ69KnobChanged(isFirstReport, hashSet);
        }

        public sealed override void Startup()
        {
            try
            {
                StartupBase("Mi-8");

                // COM1
                _r863ManualDcsbiosOutputCockpitFrequency = DCSBIOSControlLocator.GetDCSBIOSOutput("R863_FREQ");
                DCSBIOSStringManager.AddListener(_r863ManualDcsbiosOutputCockpitFrequency, this);

                // COM2
                _r863Preset1DcsbiosOutputPresetDial = DCSBIOSControlLocator.GetDCSBIOSOutput("R863_CNL_SEL");
                _r863UnitSwitchDcsbiosOutput = DCSBIOSControlLocator.GetDCSBIOSOutput("R863_UNIT_SWITCH");

                // NAV1
                _yadro1ADcsbiosOutputCockpitFrequency = DCSBIOSControlLocator.GetDCSBIOSOutput("YADRO1A_FREQ");
                DCSBIOSStringManager.AddListener(_yadro1ADcsbiosOutputCockpitFrequency, this);

                // NAV2
                _r828Preset1DcsbiosOutputDial = DCSBIOSControlLocator.GetDCSBIOSOutput("R828_PRST_CHAN_SEL");

                // ADF
                _adfMainDcsbiosOutputPresetDial1 = DCSBIOSControlLocator.GetDCSBIOSOutput("ARC_MAIN_100KHZ");
                _adfMainDcsbiosOutputPresetDial2 = DCSBIOSControlLocator.GetDCSBIOSOutput("ARC_MAIN_10KHZ");
                _adfBackupDcsbiosOutputPresetDial1 = DCSBIOSControlLocator.GetDCSBIOSOutput("ARC_BCK_100KHZ");
                _adfBackupDcsbiosOutputPresetDial2 = DCSBIOSControlLocator.GetDCSBIOSOutput("ARC_BCK_10KHZ");
                _adfBackupMainDcsbiosOutputPresetDial = DCSBIOSControlLocator.GetDCSBIOSOutput("ARC9_MAIN_BACKUP");

                // DME
                _arkUdPresetDcsbiosOutputPresetDial = DCSBIOSControlLocator.GetDCSBIOSOutput("ARCUD_CHL");
                _arkUdModeDcsbiosOutputDial = DCSBIOSControlLocator.GetDCSBIOSOutput("ARCUD_MODE");
                _arkUdVhfUhfModeDcsbiosOutputDial = DCSBIOSControlLocator.GetDCSBIOSOutput("ARCUD_WAVE");

                // XPDR
                _spu7DcsbiosOutputPresetDial = DCSBIOSControlLocator.GetDCSBIOSOutput("RADIO_SEL_L");
                _spu7ICSSwitchDcsbiosOutput = DCSBIOSControlLocator.GetDCSBIOSOutput("SPU7_L_ICS");

                StartListeningForPanelChanges();

                // IsAttached = true;
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
        }
        
        public override void ClearSettings(bool setIsDirty = false)
        {
        }

        public override DcsOutputAndColorBinding CreateDcsOutputAndColorBinding(SaitekPanelLEDPosition saitekPanelLEDPosition, PanelLEDColor panelLEDColor, DCSBIOSOutput dcsBiosOutput)
        {
            var dcsOutputAndColorBinding = new DcsOutputAndColorBindingPZ55();
            dcsOutputAndColorBinding.DCSBiosOutputLED = dcsBiosOutput;
            dcsOutputAndColorBinding.LEDColor = panelLEDColor;
            dcsOutputAndColorBinding.SaitekLEDPosition = saitekPanelLEDPosition;
            return dcsOutputAndColorBinding;
        }

        private void CreateRadioKnobs()
        {
            SaitekPanelKnobs = RadioPanelKnobMi8.GetRadioPanelKnobs();
        }

        private void SetUpperRadioMode(CurrentMi8RadioMode currentMi8RadioMode)
        {
            try
            {
                _currentUpperRadioMode = currentMi8RadioMode;
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
        }

        private void SetLowerRadioMode(CurrentMi8RadioMode currentMi8RadioMode)
        {
            try
            {
                _currentLowerRadioMode = currentMi8RadioMode;

                // If NOUSE then send next round of data to the panel in order to clear the LCD.
                // _sendNextRoundToPanel = true;catch (Exception ex)
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
        }

        private bool R863ManualNowSyncing()
        {
            return Interlocked.Read(ref _r863ManualThreadNowSynching) > 0;
        }

        private bool YaDRO1ANowSyncing()
        {
            return Interlocked.Read(ref _yadro1AThreadNowSynching) > 0;
        }

        private void SaveCockpitFrequencyR863Manual()
        {
            try
            {
                /*
                 * Dial 1
                 *      10 11 12 13 14 22 23 24 25 26 27 28 29 30 31 32 33 34 35 36 37 38 39
                 * Pos  0   1  2  3  4  5  6  7  8  9 10 11 12 13 14 15 16 17 18 19 20 21 22
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
                 * 00/50
                 */
                lock (_lockR863ManualDialsObject1)
                {
                    lock (_lockR863ManualDialsObject2)
                    {
                        lock (_lockR863ManualDialsObject3)
                        {
                            lock (_lockR863ManualDialsObject4)
                            {
                                _r863ManualSavedCockpitBigFrequency =
                                    uint.Parse(
                                        this._r863ManualCockpitFreq1DialPos
                                        + _r863ManualCockpitFreq2DialPos
                                            .ToString()); // uint.Parse(_r863ManualFreq1DialValues[_r863ManualCockpitFreq1DialPos].ToString() + _r863ManualCockpitFreq2DialPos.ToString());
                                _r863ManualSavedCockpitSmallFrequency = uint.Parse(this._r863ManualCockpitFreq3DialPos + _r863ManualCockpitFreq4DialPos.ToString().PadLeft(2 , '0'));
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
                                _yadro1ASavedCockpitBigFrequency = uint.Parse(this._yadro1ACockpitFreq1DialPos + _yadro1ACockpitFreq2DialPos.ToString());
                                _yadro1ASavedCockpitSmallFrequency = uint.Parse(this._yadro1ACockpitFreq3DialPos + _yadro1ACockpitFreq4DialPos.ToString());
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

        private string GetCommandDirectionForR863ManualDial1(int desiredDialPosition, uint actualDialPosition)
        {
            const string inc = "INC\n";
            const string dec = "DEC\n";
            try
            {
                /*
                 * Min is 10
                 * Max is 39
                 */

                // count up
                var tmpActualDialPositionUp = actualDialPosition;
                var upCount = actualDialPosition;
                do
                {
                    if (tmpActualDialPositionUp == 39)
                    {
                        tmpActualDialPositionUp = 10;
                    }
                    else if (tmpActualDialPositionUp == 14)
                    {
                        tmpActualDialPositionUp = 22;
                    }
                    else
                    {
                        tmpActualDialPositionUp++;
                    }

                    upCount++;
                }
                while (tmpActualDialPositionUp != desiredDialPosition);

                // count down
                var tmpActualDialPositionDown = actualDialPosition;
                var downCount = actualDialPosition;
                do
                {
                    if (tmpActualDialPositionDown == 10)
                    {
                        tmpActualDialPositionDown = 39;
                    }
                    else if (tmpActualDialPositionDown == 22)
                    {
                        tmpActualDialPositionDown = 14;
                    }
                    else
                    {
                        tmpActualDialPositionDown--;
                    }

                    downCount++;
                }
                while (tmpActualDialPositionDown != desiredDialPosition);

                if (upCount < downCount)
                {
                    return inc;
                }

                return dec;
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }

            return inc;
        }

        private string GetCommandDirectionFor0To9Dials(int desiredDialPosition, uint actualDialPosition)
        {
            try
            {
                const string inc = "INC\n";
                const string dec = "DEC\n";

                var tmpActualDialPositionUp = actualDialPosition;
                var upCount = actualDialPosition;
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
                }
                while (tmpActualDialPositionUp != desiredDialPosition);

                tmpActualDialPositionUp = actualDialPosition;
                var downCount = actualDialPosition;
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
                }
                while (tmpActualDialPositionUp != desiredDialPosition);

                if (upCount < downCount)
                {
                    return inc;
                }

                return dec;
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }

            throw new Exception(
                "Should not reach this code. private String GetCommandDirectionFor0To9Dials(uint desiredDialPosition, uint actualDialPosition) -> " + desiredDialPosition + "   " + actualDialPosition);
        }

        /*
        private string GetR863ManualDialFrequencyForPosition(uint position)
        {
            try
            {
                Common.DebugP("Entering Mi-8 Radio GetR863ManualDialFrequencyForPosition()");
                //        "00"  "25" "50" "75"
                //          0    2    5    7  
                switch (position)
                {
                    case 0:
                        {
                            Common.DebugP("Leaving Mi-8 Radio GetR863ManualDialFrequencyForPosition()");
                            return "0";
                        }
                    case 2:
                        {
                            Common.DebugP("Leaving Mi-8 Radio GetR863ManualDialFrequencyForPosition()");
                            return "5";
                        }
                    case 5:
                        {
                            Common.DebugP("Leaving Mi-8 Radio GetR863ManualDialFrequencyForPosition()");
                            return "5";
                        }
                    case 7:
                        {
                            Common.DebugP("Leaving Mi-8 Radio GetR863ManualDialFrequencyForPosition()");
                            return "0";
                        }
                }
                Common.LogError(1111, "ERROR!!! Leaving Mi-8 Radio GetR863ManualDialFrequencyForPosition() Position : [" + position + "]");
            }
            catch (Exception ex)
            {
                Common.LogError(78019, ex);
            }
            return "0";
        }
        */
        private bool SkipR863PresetDialChange()
        {
            try
            {
                if (_currentUpperRadioMode == CurrentMi8RadioMode.R863_PRESET || _currentLowerRadioMode == CurrentMi8RadioMode.R863_PRESET)
                {
                    /*if (_r863PresetDialSkipper > SKIP_CONSTANT)
                    {
                        _r863PresetDialSkipper = 0;
                        return false;
                    }

                    _r863PresetDialSkipper++;
                    return true;*/
                }
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
                if (_currentUpperRadioMode == CurrentMi8RadioMode.R828_PRESETS || _currentLowerRadioMode == CurrentMi8RadioMode.R828_PRESETS)
                {
                    /*if (_r828PresetDialSkipper > SKIP_CONSTANT)
                    {
                        _r828PresetDialSkipper = 0;
                        return false;
                    }

                    _r828PresetDialSkipper++;
                    return true;*/
                }
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
                if (_currentUpperRadioMode == CurrentMi8RadioMode.ADF_ARK9 || _currentLowerRadioMode == CurrentMi8RadioMode.ADF_ARK9)
                {
                    /*if (_adfPresetDial1Skipper > SKIP_CONSTANT)
                    {
                        _adfPresetDial1Skipper = 0;
                        return false;
                    }

                    _adfPresetDial1Skipper++;
                    return true;*/
                }
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
                if (_currentUpperRadioMode == CurrentMi8RadioMode.ADF_ARK9 || _currentLowerRadioMode == CurrentMi8RadioMode.ADF_ARK9)
                {
                    /*if (_adfPresetDial2Skipper > SKIP_CONSTANT)
                    {
                        _adfPresetDial2Skipper = 0;
                        return false;
                    }

                    _adfPresetDial2Skipper++;
                    return true;*/
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }

            return false;
        }

        private bool SkipSpu7PresetDialChange()
        {
            try
            {
                if (_currentUpperRadioMode == CurrentMi8RadioMode.SPU7 || _currentLowerRadioMode == CurrentMi8RadioMode.SPU7)
                {
                    /*if (_spu7DialSkipper > SKIP_CONSTANT)
                    {
                        _spu7DialSkipper = 0;
                        return false;
                    }

                    _spu7DialSkipper++;
                    return true;*/
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }

            return false;
        }

        private bool SkipArkudPresetDialChange()
        {
            try
            {
                if (_currentUpperRadioMode == CurrentMi8RadioMode.ARK_UD || _currentLowerRadioMode == CurrentMi8RadioMode.ARK_UD)
                {
                    /*if (_arkUdPresetDialSkipper > SKIP_CONSTANT)
                    {
                        _arkUdPresetDialSkipper = 0;
                        return false;
                    }

                    _arkUdPresetDialSkipper++;
                    return true;*/
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }

            return false;
        }

        private bool SkipArkudModeDialChange()
        {
            try
            {
                if (_currentUpperRadioMode == CurrentMi8RadioMode.ARK_UD || _currentLowerRadioMode == CurrentMi8RadioMode.ARK_UD)
                {
                    /*if (_arkUdModeDialSkipper > SKIP_CONSTANT)
                    {
                        _arkUdModeDialSkipper = 0;
                        return false;
                    }

                    _arkUdModeDialSkipper++;
                    return true;*/
                }
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

        public override void AddOrUpdateDCSBIOSBinding(PanelSwitchOnOff panelSwitchOnOff, List<DCSBIOSInput> dcsbiosInputs, string description)
        {
        }

        public override void AddOrUpdateBIPLinkBinding(PanelSwitchOnOff panelSwitchOnOff, BIPLink bipLink)
        {
        }

        public override void AddOrUpdateOSCommandBinding(PanelSwitchOnOff panelSwitchOnOff, OSCommand operatingSystemCommand)
        {
        }
    }
}
