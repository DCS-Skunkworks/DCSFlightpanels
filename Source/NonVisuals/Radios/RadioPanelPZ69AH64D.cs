using NonVisuals.BindingClasses.BIP;

namespace NonVisuals.Radios
{
    using System;
    using System.Collections.Generic;
    using System.Threading;

    using ClassLibraryCommon;

    using DCS_BIOS;
    using DCS_BIOS.EventArgs;
    using DCS_BIOS.Interfaces;

    using MEF;
    using Plugin;
    using Knobs;
    using Panels.Saitek;
    using HID;
    using System.Globalization;


    /*
     * Pre-programmed radio panel for the AH-64D.
     */
    public class RadioPanelPZ69AH64D : RadioPanelPZ69Base, IDCSBIOSStringListener
    {
        private enum CurrentAH64DRadioMode
        {
            VHF,
            UHF,
            FM1,
            FM2,
            HF,
            NOUSE
        }
        private enum Pz69Mode
        {
            UPPER,
            LOWER
        }

        private CurrentAH64DRadioMode _currentUpperRadioMode = CurrentAH64DRadioMode.VHF;
        private CurrentAH64DRadioMode _currentLowerRadioMode = CurrentAH64DRadioMode.VHF;

        private DCSBIOSOutput _pltEUFDLine8;  //VFH Frequencies
        private DCSBIOSOutput _pltEUFDLine9;  //UHF Frequencies
        private DCSBIOSOutput _pltEUFDLine10; //FM1
        private DCSBIOSOutput _pltEUFDLine11; //FM2
        private DCSBIOSOutput _pltEUFDLine12; //HF
        private DCSBIOSOutput _pltEUFDLine14;


        private string _vhfFrequencyActive = string.Empty;
        private string _vhfFrequencyStby = string.Empty;
        private string _uhfFrequencyActive = string.Empty;
        private string _uhfFrequencyStby = string.Empty;
        private string _fm1FrequencyActive = string.Empty;
        private string _fm1FrequencyStby = string.Empty;
        private string _fm2FrequencyActive = string.Empty;
        private string _fm2FrequencyStby = string.Empty;
        private string _hfFrequencyActive = string.Empty;
        private string _hfFrequencyStby = string.Empty;

        private int _vhfPresetDialSkipper;
        private int _uhfPresetDialSkipper;
        private int _fm1PresetDialSkipper;
        private int _fm2PresetDialSkipper;
        private int _tempPLT_EUFD_RockerSkipper;

        //private const string VHF_VOLUME_COMMAND_INC = "PLT_COM_VHF_VOL +3200\n"; //*
        //private const string VHF_VOLUME_COMMAND_DEC = "PLT_COM_VHF_VOL -3200\n"; //*

        private const string PLT_EUFD_COMMAND_SWAP = "PLT_EUFD_SWAP TOGGLE\n";

        private const string PLT_EUFD_COMMAND_IDM = "PLT_EUFD_IDM";
        private const string PLT_EUFD_COMMAND_RTS = "PLT_EUFD_RTS";
        private const string PLT_EUFD_COMMAND_WCA = "PLT_EUFD_WCA";

        private readonly object _lockShowFrequenciesOnPanelObject = new();
        private long _doUpdatePanelLCD;

        private bool _upperFreqSwitchPressedDown;
        private bool _lowerFreqSwitchPressedDown;

        private enum FrequencyType
        {
            VHFActive,
            VHFStby,
            UHFActive,
            UHFStby,
            FM1Active,
            FM1Stby,
            FM2Active,
            FM2Stby,
            HFActive,
            HFStby,
        }


        private readonly Dictionary<FrequencyType, string> _ufdFrequencies = new();

        public RadioPanelPZ69AH64D(HIDSkeleton hidSkeleton) : base(hidSkeleton)
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
                    BIOSEventHandler.DetachStringListener(this);
                    BIOSEventHandler.DetachDataListener(this);
                }

                _disposed = true;
            }

            // Call base class implementation.
            base.Dispose(disposing);
        }
        public sealed override void Startup()
        {
            try
            {
                _pltEUFDLine8 = DCSBIOSControlLocator.GetDCSBIOSOutput("PLT_EUFD_LINE8");
                DCSBIOSStringManager.AddListeningAddress(_pltEUFDLine8);

                _pltEUFDLine9 = DCSBIOSControlLocator.GetDCSBIOSOutput("PLT_EUFD_LINE9");
                DCSBIOSStringManager.AddListeningAddress(_pltEUFDLine9);

                _pltEUFDLine10 = DCSBIOSControlLocator.GetDCSBIOSOutput("PLT_EUFD_LINE10");
                DCSBIOSStringManager.AddListeningAddress(_pltEUFDLine10);

                _pltEUFDLine11 = DCSBIOSControlLocator.GetDCSBIOSOutput("PLT_EUFD_LINE11");
                DCSBIOSStringManager.AddListeningAddress(_pltEUFDLine11);

                _pltEUFDLine12 = DCSBIOSControlLocator.GetDCSBIOSOutput("PLT_EUFD_LINE12");
                DCSBIOSStringManager.AddListeningAddress(_pltEUFDLine12);

                _pltEUFDLine14 = DCSBIOSControlLocator.GetDCSBIOSOutput("PLT_EUFD_LINE14");
                DCSBIOSStringManager.AddListeningAddress(_pltEUFDLine14);

                StartListeningForHidPanelChanges();
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                throw new ArgumentOutOfRangeException($"Critical error during PZ69 panel Startup [{ex.Message}]");
            }
        }

        private bool UfdFrequencyHasChanged(FrequencyType frequencyType, string rawFrequencyString)
        {
            try
            {
                var frequencyCheck = decimal.Parse(rawFrequencyString, CultureInfo.InvariantCulture);
            }
            catch(Exception)
            {
                return false;
            }

            if (_ufdFrequencies.ContainsKey(frequencyType) && _ufdFrequencies[frequencyType].Equals(rawFrequencyString))
            {
                return false;
            }

            if (!_ufdFrequencies.ContainsKey(frequencyType)) 
            { 
                _ufdFrequencies.Add(frequencyType, rawFrequencyString); 
                return true;
            }

            _ufdFrequencies[frequencyType] = rawFrequencyString;

            return true;
        }

        private bool UFDStringDataChanged(int ufdLine, string udfLineData)
        {
            switch (ufdLine) {
                case 8:
                    //"~<>VHF*  134.000   -----              121.500   -----   "
                    return
                        UfdFrequencyHasChanged(FrequencyType.VHFActive, udfLineData.Substring(8, 10).Trim())
                        ||
                        UfdFrequencyHasChanged(FrequencyType.VHFStby, udfLineData.Substring(38, 10).Trim());
                case 9:
                    //" ==UHF*  263.000   -----              305.000   -----   "
                    return
                        UfdFrequencyHasChanged(FrequencyType.UHFActive, udfLineData.Substring(8, 10).Trim())
                        ||
                        UfdFrequencyHasChanged(FrequencyType.UHFStby, udfLineData.Substring(38, 10).Trim());
                case 10:
                    //" ==FM1*   30.005   -----    NORM       30.000   -----   "
                    //" <=FM1*   35.125   -----    NORM     | BATUMI   30.045  "
                    return
                        UfdFrequencyHasChanged(FrequencyType.FM1Active, udfLineData.Substring(8, 10).Trim())
                        ||
                        UfdFrequencyHasChanged(FrequencyType.FM1Stby, udfLineData.Substring(38, 10).Trim());
                case 11:
                    //" ==FM2*   30.005   -----               30.000   -----   "
                    return
                        UfdFrequencyHasChanged(FrequencyType.FM2Active, udfLineData.Substring(8, 10).Trim())
                        ||
                        UfdFrequencyHasChanged(FrequencyType.FM2Stby, udfLineData.Substring(38, 10).Trim());
                case 12:
                    //" <=HF *    2.0500A -----    LOW        25.5000A -----    "
                    return
                        UfdFrequencyHasChanged(FrequencyType.HFActive, udfLineData.Substring(8, 9).Trim())
                        ||
                        UfdFrequencyHasChanged(FrequencyType.HFStby, udfLineData.Substring(38, 8).Trim());
                default:
                    return false;
            }
        }

        public void DCSBIOSStringReceived(object sender, DCSBIOSStringDataEventArgs e)
        {
            try
            {
                int linesChanged = 0;
                if (e.Address.Equals(_pltEUFDLine8.Address)) //VHF
                {
                    if (UFDStringDataChanged(8, e.StringData))
                    {
                        linesChanged++;
                    }
                }
                if (e.Address.Equals(_pltEUFDLine9.Address)) //UHF
                {
                    if (UFDStringDataChanged(9, e.StringData))
                    {
                        linesChanged++;
                    }
                }
                if (e.Address.Equals(_pltEUFDLine10.Address)) //FM1
                {
                    if (UFDStringDataChanged(10, e.StringData))
                    {
                        linesChanged++;
                    }
                }
                if (e.Address.Equals(_pltEUFDLine11.Address)) //FM2
                {
                    if (UFDStringDataChanged(11, e.StringData))
                    {
                        linesChanged++;
                    }
                }
                if (e.Address.Equals(_pltEUFDLine12.Address)) //HF
                {
                    if (UFDStringDataChanged(12, e.StringData))
                    {
                        linesChanged++;
                    }
                }



                DataHasBeenReceivedFromDCSBIOS = true;
                if (linesChanged>0)
                {
                    Interlocked.Increment(ref _doUpdatePanelLCD);
                    ShowFrequenciesOnPanel();
                }
                
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex, "DCSBIOSStringReceived()");
            }
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


                // Set once
                DataHasBeenReceivedFromDCSBIOS = true;
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
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
                        var radioPanelKnob = (RadioPanelKnobAH64D)radioPanelKnobObject;

                        switch (radioPanelKnob.RadioPanelPZ69Knob)
                        {
                            case RadioPanelPZ69KnobsAH64D.UPPER_VHF:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetUpperRadioMode(CurrentAH64DRadioMode.VHF);
                                    }
                                    break;
                                }

                            case RadioPanelPZ69KnobsAH64D.UPPER_UHF:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetUpperRadioMode(CurrentAH64DRadioMode.UHF);
                                    }
                                    break;
                                }

                            case RadioPanelPZ69KnobsAH64D.UPPER_FM1:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetUpperRadioMode(CurrentAH64DRadioMode.FM1);
                                    }
                                    break;
                                }

                            case RadioPanelPZ69KnobsAH64D.UPPER_FM2:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetUpperRadioMode(CurrentAH64DRadioMode.FM2);
                                    }
                                    break;
                                }
                            case RadioPanelPZ69KnobsAH64D.UPPER_HF:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetUpperRadioMode(CurrentAH64DRadioMode.HF);
                                    }
                                    break;
                                }

                            case RadioPanelPZ69KnobsAH64D.UPPER_NO_USE3:
                            case RadioPanelPZ69KnobsAH64D.UPPER_NO_USE4:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetUpperRadioMode(CurrentAH64DRadioMode.NOUSE);
                                    }
                                    break;
                                }

                            case RadioPanelPZ69KnobsAH64D.LOWER_VHF:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetLowerRadioMode(CurrentAH64DRadioMode.VHF);
                                    }
                                    break;
                                }

                            case RadioPanelPZ69KnobsAH64D.LOWER_UHF:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetLowerRadioMode(CurrentAH64DRadioMode.UHF);
                                    }
                                    break;
                                }

                            case RadioPanelPZ69KnobsAH64D.LOWER_FM1:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetLowerRadioMode(CurrentAH64DRadioMode.FM1);
                                    }
                                    break;
                                }

                            case RadioPanelPZ69KnobsAH64D.LOWER_FM2:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetLowerRadioMode(CurrentAH64DRadioMode.FM2);
                                    }
                                    break;
                                }
                            case RadioPanelPZ69KnobsAH64D.LOWER_HF:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetLowerRadioMode(CurrentAH64DRadioMode.HF);
                                    }
                                    break;
                                }
                            case RadioPanelPZ69KnobsAH64D.LOWER_NO_USE3:
                            case RadioPanelPZ69KnobsAH64D.LOWER_NO_USE4:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetLowerRadioMode(CurrentAH64DRadioMode.NOUSE);
                                    }
                                    break;
                                }

                            case RadioPanelPZ69KnobsAH64D.UPPER_LARGE_FREQ_WHEEL_INC:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetTempRockerCommand(PLT_EUFD_COMMAND_IDM, 2);
                                    }
                                    break;
                                }
                            case RadioPanelPZ69KnobsAH64D.UPPER_LARGE_FREQ_WHEEL_DEC:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetTempRockerCommand(PLT_EUFD_COMMAND_IDM, 0);
                                    }
                                    break;
                                }
                            case RadioPanelPZ69KnobsAH64D.UPPER_SMALL_FREQ_WHEEL_INC:
                            case RadioPanelPZ69KnobsAH64D.UPPER_SMALL_FREQ_WHEEL_DEC:
                                break;

                            case RadioPanelPZ69KnobsAH64D.LOWER_LARGE_FREQ_WHEEL_INC:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetTempRockerCommand(PLT_EUFD_COMMAND_WCA, 2);
                                    }
                                    break;
                                }
                            case RadioPanelPZ69KnobsAH64D.LOWER_LARGE_FREQ_WHEEL_DEC:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetTempRockerCommand(PLT_EUFD_COMMAND_WCA, 0);
                                    }
                                    break;
                                }
                            case RadioPanelPZ69KnobsAH64D.LOWER_SMALL_FREQ_WHEEL_INC:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetTempRockerCommand(PLT_EUFD_COMMAND_RTS, 2);
                                    }
                                    break;
                                }

                            case RadioPanelPZ69KnobsAH64D.LOWER_SMALL_FREQ_WHEEL_DEC:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetTempRockerCommand(PLT_EUFD_COMMAND_RTS, 0);
                                    }
                                    break;
                                }

                            case RadioPanelPZ69KnobsAH64D.UPPER_FREQ_SWITCH:
                                {
                                    _upperFreqSwitchPressedDown = radioPanelKnob.IsOn;
                                    {
                                        DCSBIOS.Send(PLT_EUFD_COMMAND_SWAP);
                                    }
                                    break;
                                }

                            case RadioPanelPZ69KnobsAH64D.LOWER_FREQ_SWITCH:
                                {
                                    _lowerFreqSwitchPressedDown = radioPanelKnob.IsOn;
                                    {
                                        DCSBIOS.Send(PLT_EUFD_COMMAND_SWAP);
                                    }
                                    break;
                                }
                        }

                        if (PluginManager.PlugSupportActivated && PluginManager.HasPlugin())
                        {
                            PluginManager.DoEvent(DCSAircraft.SelectedAircraft.Description, HIDInstance, PluginGamingPanelEnum.PZ69RadioPanel_PreProg_AH64D, (int)radioPanelKnob.RadioPanelPZ69Knob, radioPanelKnob.IsOn, null);
                        }
                    }

                    AdjustFrequency(hashSet);
                    ShowFrequenciesOnPanel();
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void SetTempRockerCommand(string dcsBiosCommand, int rockerTempSwitchValue)
        {
            if (!SkipTempRockerChange())
            {
                DCSBIOS.Send($"{dcsBiosCommand} {rockerTempSwitchValue}\n");
                DCSBIOS.Send($"{dcsBiosCommand} 1\n");
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
                    var radioPanelKnobAH64D = (RadioPanelKnobAH64D)o;
                    if (radioPanelKnobAH64D.IsOn)
                    {
                        switch (radioPanelKnobAH64D.RadioPanelPZ69Knob)
                        {
                            case RadioPanelPZ69KnobsAH64D.UPPER_LARGE_FREQ_WHEEL_INC:
                                {
                                    switch (_currentUpperRadioMode)
                                    {
                                        case CurrentAH64DRadioMode.VHF:
                                            {
                                                if (!SkipVHFPresetDialChange())
                                                {
                                                }
                                                break;
                                            }

                                        case CurrentAH64DRadioMode.UHF:
                                            {
                                                if (!SkipUHFPresetDialChange())
                                                {
                                                }
                                                break;
                                            }

                                        case CurrentAH64DRadioMode.FM1:
                                            {
                                                if (!SkipFM1DialChange())
                                                {
                                                }
                                                break;
                                            }

                                        case CurrentAH64DRadioMode.FM2:
                                            {
                                                if (!SkipFM2DialChange())
                                                {
                                                }
                                                break;
                                            }

                                        case CurrentAH64DRadioMode.NOUSE:
                                            {
                                                break;
                                            }
                                    }
                                    break;
                                }

                            case RadioPanelPZ69KnobsAH64D.UPPER_LARGE_FREQ_WHEEL_DEC:
                                {
                                    switch (_currentUpperRadioMode)
                                    {
                                        case CurrentAH64DRadioMode.VHF:
                                            {
                                                if (!SkipVHFPresetDialChange())
                                                {
                                                }
                                                break;
                                            }

                                        case CurrentAH64DRadioMode.UHF:
                                            {
                                                if (!SkipUHFPresetDialChange())
                                                {
                                                }
                                                break;
                                            }

                                        case CurrentAH64DRadioMode.FM1:
                                            {
                                                if (!SkipFM1DialChange())
                                                {
                                                }
                                                break;
                                            }

                                        case CurrentAH64DRadioMode.FM2:
                                            {
                                                if (!SkipFM2DialChange())
                                                {
                                                }
                                                break;
                                            }
                                    }
                                    break;
                                }

                            case RadioPanelPZ69KnobsAH64D.UPPER_SMALL_FREQ_WHEEL_INC:
                                {
                                    switch (_currentUpperRadioMode)
                                    {
                                        case CurrentAH64DRadioMode.VHF:
                                            {
                                          //      DCSBIOS.Send(VHF_VOLUME_COMMAND_INC);
                                                break;
                                            }

                                        case CurrentAH64DRadioMode.UHF:
                                            {
                                                break;
                                            }

                                        case CurrentAH64DRadioMode.FM1:
                                            {
                                                if (!SkipFM1DialChange())
                                                {
                                                }
                                                break;
                                            }

                                        case CurrentAH64DRadioMode.FM2:
                                            {
                                                if (!SkipFM2DialChange())
                                                {
                                                }
                                                break;
                                            }

                                        case CurrentAH64DRadioMode.NOUSE:
                                            {
                                                break;
                                            }
                                    }
                                    break;
                                }

                            case RadioPanelPZ69KnobsAH64D.UPPER_SMALL_FREQ_WHEEL_DEC:
                                {
                                    switch (_currentUpperRadioMode)
                                    {
                                        case CurrentAH64DRadioMode.UHF:
                                            {
                                                break;
                                            }

                                        case CurrentAH64DRadioMode.VHF:
                                            {
                                            //    DCSBIOS.Send(VHF_VOLUME_COMMAND_DEC);
                                                break;
                                            }

                                        case CurrentAH64DRadioMode.FM1:
                                            {
                                                if (!SkipFM1DialChange())
                                                {
                                                }
                                                break;
                                            }

                                        case CurrentAH64DRadioMode.FM2:
                                            {
                                                if (!SkipFM2DialChange())
                                                {
                                                }
                                                break;
                                            }

                                        case CurrentAH64DRadioMode.NOUSE:
                                            {
                                                break;
                                            }
                                    }
                                    break;
                                }

                            case RadioPanelPZ69KnobsAH64D.LOWER_LARGE_FREQ_WHEEL_INC:
                                {
                                    switch (_currentLowerRadioMode)
                                    {
                                        case CurrentAH64DRadioMode.VHF:
                                            {
                                                if (!SkipVHFPresetDialChange())
                                                {
                                                }
                                                break;
                                            }

                                        case CurrentAH64DRadioMode.UHF:
                                            {
                                                if (!SkipUHFPresetDialChange())
                                                {
                                                }
                                                break;
                                            }

                                        case CurrentAH64DRadioMode.FM1:
                                            {
                                                if (!SkipFM1DialChange())
                                                {
                                                }
                                                break;
                                            }

                                        case CurrentAH64DRadioMode.FM2:
                                            {
                                                if (!SkipFM2DialChange())
                                                {
                                                }
                                                break;
                                            }

                                        case CurrentAH64DRadioMode.NOUSE:
                                            {
                                                break;
                                            }
                                    }
                                    break;
                                }

                            case RadioPanelPZ69KnobsAH64D.LOWER_LARGE_FREQ_WHEEL_DEC:
                                {
                                    switch (_currentLowerRadioMode)
                                    {
                                        case CurrentAH64DRadioMode.VHF:
                                            {
                                                if (!SkipVHFPresetDialChange())
                                                {
                                                }
                                                break;
                                            }

                                        case CurrentAH64DRadioMode.UHF:
                                            {
                                                if (!SkipUHFPresetDialChange())
                                                {
                                                }
                                                break;
                                            }

                                        case CurrentAH64DRadioMode.FM1:
                                            {
                                                if (!SkipFM1DialChange())
                                                {
                                                }
                                                break;
                                            }

                                        case CurrentAH64DRadioMode.FM2:
                                            {
                                                if (!SkipFM2DialChange())
                                                {
                                                }
                                                break;
                                            }

                                        case CurrentAH64DRadioMode.NOUSE:
                                            {
                                                break;
                                            }
                                    }
                                    break;
                                }

                            case RadioPanelPZ69KnobsAH64D.LOWER_SMALL_FREQ_WHEEL_INC:
                                {
                                    switch (_currentLowerRadioMode)
                                    {
                                        case CurrentAH64DRadioMode.UHF:
                                            {
                                                break;
                                            }

                                        case CurrentAH64DRadioMode.VHF:
                                            {
                                             //   DCSBIOS.Send(VHF_VOLUME_COMMAND_INC);
                                                break;
                                            }

                                        case CurrentAH64DRadioMode.FM1:
                                            {
                                                if (!SkipFM1DialChange())
                                                {
                                                }
                                                break;
                                            }

                                        case CurrentAH64DRadioMode.FM2:
                                            {
                                                if (!SkipFM2DialChange())
                                                {
                                                }
                                                break;
                                            }

                                        case CurrentAH64DRadioMode.NOUSE:
                                            {
                                                break;
                                            }
                                    }
                                    break;
                                }

                            case RadioPanelPZ69KnobsAH64D.LOWER_SMALL_FREQ_WHEEL_DEC:
                                {
                                    switch (_currentLowerRadioMode)
                                    {
                                        case CurrentAH64DRadioMode.UHF:
                                            {
                                                break;
                                            }

                                        case CurrentAH64DRadioMode.VHF:
                                            {
                                             //   DCSBIOS.Send(VHF_VOLUME_COMMAND_DEC);
                                                break;
                                            }

                                        case CurrentAH64DRadioMode.FM1:
                                            {
                                                if (!SkipFM1DialChange())
                                                {
                                                }
                                                break;
                                            }

                                        case CurrentAH64DRadioMode.FM2:
                                            {
                                                if (!SkipFM2DialChange())
                                                {
                                                }
                                                break;
                                            }

                                        case CurrentAH64DRadioMode.NOUSE:
                                            {
                                                break;
                                            }
                                    }
                                    break;
                                }
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

        private string GetSafeFrequency(FrequencyType frequencyType)
        {
            if (!_ufdFrequencies.ContainsKey(frequencyType) || string.IsNullOrEmpty(_ufdFrequencies[frequencyType]))
                return string.Empty;

             return _ufdFrequencies[frequencyType];
        }

        private void SetFrequencyBytes(FrequencyType frequencyType, Pz69Mode pz69mode, ref byte[] bytes)
        {
            switch (frequencyType) {
                case (FrequencyType.VHFActive):
                    lock (_vhfFrequencyActive)
                    {
                         _vhfFrequencyActive = GetSafeFrequency(FrequencyType.VHFActive);
                        if (!string.IsNullOrEmpty(_vhfFrequencyActive))
                        {
                            SetPZ69DisplayBytes(ref bytes, double.Parse(_vhfFrequencyActive, NumberFormatInfoFullDisplay), 2, pz69mode == Pz69Mode.UPPER ? PZ69LCDPosition.UPPER_ACTIVE_LEFT : PZ69LCDPosition.LOWER_ACTIVE_LEFT) ;
                        }
                        else
                        {
                            SetPZ69DisplayBlank(ref bytes, pz69mode == Pz69Mode.UPPER ? PZ69LCDPosition.UPPER_STBY_RIGHT : PZ69LCDPosition.LOWER_STBY_RIGHT);
                        }
                    }
                    lock (_vhfFrequencyStby)
                    {
                        _vhfFrequencyStby = GetSafeFrequency(FrequencyType.VHFStby);
                        if (!string.IsNullOrEmpty(_vhfFrequencyStby))
                        {
                            SetPZ69DisplayBytes(ref bytes, double.Parse(_vhfFrequencyStby, NumberFormatInfoFullDisplay), 2, pz69mode == Pz69Mode.UPPER ? PZ69LCDPosition.UPPER_STBY_RIGHT : PZ69LCDPosition.LOWER_STBY_RIGHT);
                        }
                        else
                        {
                            SetPZ69DisplayBlank(ref bytes, pz69mode == Pz69Mode.UPPER ? PZ69LCDPosition.UPPER_STBY_RIGHT: PZ69LCDPosition.LOWER_STBY_RIGHT);
                        }
                    }
                    break;

                case (FrequencyType.UHFActive):
                    lock (_uhfFrequencyActive)
                    {
                        _uhfFrequencyActive = GetSafeFrequency(FrequencyType.UHFActive);
                        if (!string.IsNullOrEmpty(_uhfFrequencyActive))
                        {
                            SetPZ69DisplayBytes(ref bytes, double.Parse(_uhfFrequencyActive, NumberFormatInfoFullDisplay), 2, pz69mode == Pz69Mode.UPPER ? PZ69LCDPosition.UPPER_ACTIVE_LEFT : PZ69LCDPosition.LOWER_ACTIVE_LEFT);
                        }
                        else
                        {
                            SetPZ69DisplayBlank(ref bytes, pz69mode == Pz69Mode.UPPER ? PZ69LCDPosition.UPPER_STBY_RIGHT : PZ69LCDPosition.LOWER_STBY_RIGHT);
                        }
                    }
                    lock (_uhfFrequencyStby)
                    {
                        _uhfFrequencyStby = GetSafeFrequency(FrequencyType.UHFStby);
                        if (!string.IsNullOrEmpty(_uhfFrequencyStby))
                        {
                            SetPZ69DisplayBytes(ref bytes, double.Parse(_uhfFrequencyStby, NumberFormatInfoFullDisplay), 2, pz69mode == Pz69Mode.UPPER ? PZ69LCDPosition.UPPER_STBY_RIGHT : PZ69LCDPosition.LOWER_STBY_RIGHT);
                        }
                        else
                        {
                            SetPZ69DisplayBlank(ref bytes, pz69mode == Pz69Mode.UPPER ? PZ69LCDPosition.UPPER_STBY_RIGHT : PZ69LCDPosition.LOWER_STBY_RIGHT);
                        }
                    }
                    break;

                case (FrequencyType.FM1Active):
                    lock (_fm1FrequencyActive)
                    {
                        _fm1FrequencyActive = GetSafeFrequency(FrequencyType.FM1Active);
                        if (!string.IsNullOrEmpty(_fm1FrequencyActive))
                        {
                            SetPZ69DisplayBytes(ref bytes, double.Parse(_fm1FrequencyActive, NumberFormatInfoFullDisplay), 3, pz69mode == Pz69Mode.UPPER ? PZ69LCDPosition.UPPER_ACTIVE_LEFT : PZ69LCDPosition.LOWER_ACTIVE_LEFT);
                        }
                        else
                        {
                            SetPZ69DisplayBlank(ref bytes, pz69mode == Pz69Mode.UPPER ? PZ69LCDPosition.UPPER_STBY_RIGHT : PZ69LCDPosition.LOWER_STBY_RIGHT);
                        }
                    }
                    lock (_fm1FrequencyStby)
                    {
                        _fm1FrequencyStby = GetSafeFrequency(FrequencyType.FM1Stby);
                        if (!string.IsNullOrEmpty(_fm1FrequencyStby))
                        {
                            SetPZ69DisplayBytes(ref bytes, double.Parse(_fm1FrequencyStby, NumberFormatInfoFullDisplay), 3, pz69mode == Pz69Mode.UPPER ? PZ69LCDPosition.UPPER_STBY_RIGHT : PZ69LCDPosition.LOWER_STBY_RIGHT);
                        }
                        else
                        {
                            SetPZ69DisplayBlank(ref bytes, pz69mode == Pz69Mode.UPPER ? PZ69LCDPosition.UPPER_STBY_RIGHT : PZ69LCDPosition.LOWER_STBY_RIGHT);
                        }
                    }
                    break;

                case (FrequencyType.FM2Active):
                    lock (_fm2FrequencyActive)
                    {
                        _fm2FrequencyActive = GetSafeFrequency(FrequencyType.FM2Active);
                        if (!string.IsNullOrEmpty(_fm2FrequencyActive))
                        {
                            SetPZ69DisplayBytes(ref bytes, double.Parse(_fm2FrequencyActive, NumberFormatInfoFullDisplay), 3, pz69mode == Pz69Mode.UPPER ? PZ69LCDPosition.UPPER_ACTIVE_LEFT : PZ69LCDPosition.LOWER_ACTIVE_LEFT);
                        }
                        else
                        {
                            SetPZ69DisplayBlank(ref bytes, pz69mode == Pz69Mode.UPPER ? PZ69LCDPosition.UPPER_STBY_RIGHT : PZ69LCDPosition.LOWER_STBY_RIGHT);
                        }
                    }
                    lock (_fm2FrequencyStby)
                    {
                        _fm2FrequencyStby = GetSafeFrequency(FrequencyType.FM2Stby);
                        if (!string.IsNullOrEmpty(_fm2FrequencyStby))
                        {
                            SetPZ69DisplayBytes(ref bytes, double.Parse(_fm2FrequencyStby, NumberFormatInfoFullDisplay), 3, pz69mode == Pz69Mode.UPPER ? PZ69LCDPosition.UPPER_STBY_RIGHT : PZ69LCDPosition.LOWER_STBY_RIGHT);
                        }
                        else
                        {
                            SetPZ69DisplayBlank(ref bytes, pz69mode == Pz69Mode.UPPER ? PZ69LCDPosition.UPPER_STBY_RIGHT : PZ69LCDPosition.LOWER_STBY_RIGHT);
                        }
                    }
                    break;

                case (FrequencyType.HFActive):
                    lock (_hfFrequencyActive)
                    {
                        _hfFrequencyActive = GetSafeFrequency(FrequencyType.HFActive);
                        if (!string.IsNullOrEmpty(_hfFrequencyActive))
                        {
                            SetPZ69DisplayBytes(ref bytes, double.Parse(_hfFrequencyActive, NumberFormatInfoFullDisplay), 3, pz69mode == Pz69Mode.UPPER ? PZ69LCDPosition.UPPER_ACTIVE_LEFT : PZ69LCDPosition.LOWER_ACTIVE_LEFT);
                        }
                        else
                        {
                            SetPZ69DisplayBlank(ref bytes, pz69mode == Pz69Mode.UPPER ? PZ69LCDPosition.UPPER_STBY_RIGHT : PZ69LCDPosition.LOWER_STBY_RIGHT);
                        }
                    }
                    lock (_hfFrequencyStby)
                    {
                        _hfFrequencyStby = GetSafeFrequency(FrequencyType.HFStby);
                        if (!string.IsNullOrEmpty(_hfFrequencyStby))
                        {
                            SetPZ69DisplayBytes(ref bytes, double.Parse(_hfFrequencyStby, NumberFormatInfoFullDisplay), 3, pz69mode == Pz69Mode.UPPER ? PZ69LCDPosition.UPPER_STBY_RIGHT : PZ69LCDPosition.LOWER_STBY_RIGHT);
                        }
                        else
                        {
                            SetPZ69DisplayBlank(ref bytes, pz69mode == Pz69Mode.UPPER ? PZ69LCDPosition.UPPER_STBY_RIGHT : PZ69LCDPosition.LOWER_STBY_RIGHT);
                        }
                    }
                    break;

                default:
                    throw new Exception("Unsupported frequency Type");
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
                        case CurrentAH64DRadioMode.VHF:
                            {
                                SetFrequencyBytes(FrequencyType.VHFActive, Pz69Mode.UPPER, ref bytes);
                                break;
                            }

                        case CurrentAH64DRadioMode.UHF:
                            {
                                SetFrequencyBytes(FrequencyType.UHFActive, Pz69Mode.UPPER, ref bytes);
                                break;
                            }

                        case CurrentAH64DRadioMode.FM1:
                            {
                                SetFrequencyBytes(FrequencyType.FM1Active, Pz69Mode.UPPER, ref bytes);
                                break;
                            }

                        case CurrentAH64DRadioMode.FM2:
                            {
                                SetFrequencyBytes(FrequencyType.FM2Active, Pz69Mode.UPPER, ref bytes);
                                break;
                            }
                        case CurrentAH64DRadioMode.HF:
                            {
                                SetFrequencyBytes(FrequencyType.HFActive, Pz69Mode.UPPER, ref bytes);
                                break;
                            }

                        case CurrentAH64DRadioMode.NOUSE:
                            {
                                SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.UPPER_ACTIVE_LEFT);
                                SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.UPPER_STBY_RIGHT);
                                break;
                            }
                    }
                    switch (_currentLowerRadioMode)
                    {
                        case CurrentAH64DRadioMode.VHF:
                            {
                                SetFrequencyBytes(FrequencyType.VHFActive, Pz69Mode.LOWER, ref bytes);
                                break;
                            }

                        case CurrentAH64DRadioMode.UHF:
                            {
                                SetFrequencyBytes(FrequencyType.UHFActive, Pz69Mode.LOWER, ref bytes);
                                break;
                            }

                        case CurrentAH64DRadioMode.FM1:
                            {
                                SetFrequencyBytes(FrequencyType.FM1Active, Pz69Mode.LOWER, ref bytes);
                                break;
                            }

                        case CurrentAH64DRadioMode.FM2:
                            {
                                SetFrequencyBytes(FrequencyType.FM2Active, Pz69Mode.LOWER, ref bytes);
                                break;
                            }
                        case CurrentAH64DRadioMode.HF:
                            {
                                SetFrequencyBytes(FrequencyType.HFActive, Pz69Mode.LOWER, ref bytes);
                                break;
                            }
                        case CurrentAH64DRadioMode.NOUSE:
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

        public override void ClearSettings(bool setIsDirty = false) { }

        public override DcsOutputAndColorBinding CreateDcsOutputAndColorBinding(SaitekPanelLEDPosition saitekPanelLEDPosition, PanelLEDColor panelLEDColor, DCSBIOSOutput dcsBiosOutput)
        {
            throw new Exception("Radio Panel does not support color bindings with DCS-BIOS.");
        }

        private void CreateRadioKnobs()
        {
            SaitekPanelKnobs = RadioPanelKnobAH64D.GetRadioPanelKnobs();
        }

        private void SetUpperRadioMode(CurrentAH64DRadioMode currentAH64DRadioMode)
        {
            try
            {
                _currentUpperRadioMode = currentAH64DRadioMode;
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }

        private void SetLowerRadioMode(CurrentAH64DRadioMode currentAH64DRadioMode)
        {
            try
            {
                _currentLowerRadioMode = currentAH64DRadioMode;

                // If NOUSE then send next round of data to the panel in order to clear the LCD.
                // _sendNextRoundToPanel = true;catch (Exception ex)
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }

        private bool SkipVHFPresetDialChange()
        {
            try
            {
                if (_currentUpperRadioMode == CurrentAH64DRadioMode.VHF || _currentLowerRadioMode == CurrentAH64DRadioMode.VHF)
                {
                    if (_vhfPresetDialSkipper > 2)
                    {
                        _vhfPresetDialSkipper = 0;
                        return false;
                    }
                    _vhfPresetDialSkipper++;
                    return true;
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
            return false;
        }

        private bool SkipUHFPresetDialChange()
        {
            try
            {
                if (_currentUpperRadioMode == CurrentAH64DRadioMode.UHF || _currentLowerRadioMode == CurrentAH64DRadioMode.UHF)
                {
                    if (_uhfPresetDialSkipper > 2)
                    {
                        _uhfPresetDialSkipper = 0;
                        return false;
                    }
                    _uhfPresetDialSkipper++;
                    return true;
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
            return false;
        }

        private bool SkipFM1DialChange()
        {
            try
            {
                if (_currentUpperRadioMode == CurrentAH64DRadioMode.FM1 || _currentLowerRadioMode == CurrentAH64DRadioMode.FM1)
                {
                    if (_fm1PresetDialSkipper > 2)
                    {
                        _fm1PresetDialSkipper = 0;
                        return false;
                    }
                    _fm1PresetDialSkipper++;
                    return true;
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
            return false;
        }

        private bool SkipFM2DialChange()
        {
            try
            {
                if (_currentUpperRadioMode == CurrentAH64DRadioMode.FM2 || _currentLowerRadioMode == CurrentAH64DRadioMode.FM2)
                {
                    if (_fm2PresetDialSkipper > 2)
                    {
                        _fm2PresetDialSkipper = 0;
                        return false;
                    }
                    _fm2PresetDialSkipper++;
                    return true;
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
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

        private bool SkipTempRockerChange()
        {
            try
            {
                if (_tempPLT_EUFD_RockerSkipper > 2)
                {
                    _tempPLT_EUFD_RockerSkipper = 0;
                    return false;
                }
                _tempPLT_EUFD_RockerSkipper++;
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
            return false;
        }
    }
}
