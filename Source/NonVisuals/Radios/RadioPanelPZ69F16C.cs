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
  
    /*
     * Pre-programmed radio panel for the F-16 C Block 50.
     */
    public class RadioPanelPZ69F16C : RadioPanelPZ69Base, IDCSBIOSStringListener
    {
        private enum CurrentF16CRadioMode
        {
            UHF,
            VHF,
            TACAN,
            ILS,
            NOUSE
        }
        private enum Pz69Mode
        {
            UPPER,
            LOWER
        }

        private CurrentF16CRadioMode _currentUpperRadioMode = CurrentF16CRadioMode.UHF;
        private CurrentF16CRadioMode _currentLowerRadioMode = CurrentF16CRadioMode.UHF;

        private DCSBIOSOutput _DEDLine1;  
        private DCSBIOSOutput _DEDLine2;  
        private DCSBIOSOutput _DEDLine3; 
        private DCSBIOSOutput _DEDLine4; 
        private DCSBIOSOutput _DEDLine5;

        private string _uhfFrequencyActive = string.Empty;
        private string _vhfFrequencyActive = string.Empty;
        private string _steerpointActive = string.Empty;
        private string _timeActive = string.Empty;
        private string _tacanFrequencyActive = string.Empty;
        private string _ilsFrequencyActive = string.Empty;

        private const string UHF_VOLUME_COMMAND_INC = "COMM1_PWR_KNB +3200\n"; 
        private const string UHF_VOLUME_COMMAND_DEC = "COMM1_PWR_KNB -3200\n"; 

        private const string VHF_VOLUME_COMMAND_INC = "COMM2_PWR_KNB +3200\n"; 
        private const string VHF_VOLUME_COMMAND_DEC = "COMM2_PWR_KNB -3200\n";

        private const string EHSI_CRS_COMMAND_INC = "EHSI_CRS_SET_KNB +6400\n";
        private const string EHSI_CRS_COMMAND_DEC = "EHSI_CRS_SET_KNB -6400\n";

        private const string EHSI_HDG_COMMAND_INC = "EHSI_HDG_SET_KNB +6400\n";
        private const string EHSI_HDG_COMMAND_DEC = "EHSI_HDG_SET_KNB -6400\n";

        private const string EHSI_MODE_COMMAND = "EHSI_MODE TOGGLE\n";



        private readonly object _lockShowFrequenciesOnPanelObject = new();
        private long _doUpdatePanelLCD;

        private bool _upperFreqSwitchPressedDown;
        private bool _lowerFreqSwitchPressedDown;

        private DEDPageIdentification _CurrentDedPage = DEDPageIdentification.Undefined;

        private enum FrequencyType
        {
            UHFActive,
            VHFActive,
            Steerpoint,
            Time,
            Tacan,
            Ils
        }

        private enum DEDPageIdentification
        {
            Undefined,
            Home,
            TacanIls
        }

        private readonly Dictionary<FrequencyType, string> _dedFrequencies = new();

        public RadioPanelPZ69F16C(HIDSkeleton hidSkeleton) : base(hidSkeleton)
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
                _DEDLine1 = DCSBIOSControlLocator.GetDCSBIOSOutput("DED_LINE_1");
                DCSBIOSStringManager.AddListeningAddress(_DEDLine1);

                _DEDLine2 = DCSBIOSControlLocator.GetDCSBIOSOutput("DED_LINE_2");
                DCSBIOSStringManager.AddListeningAddress(_DEDLine2);

                _DEDLine3 = DCSBIOSControlLocator.GetDCSBIOSOutput("DED_LINE_3");
                DCSBIOSStringManager.AddListeningAddress(_DEDLine3);

                _DEDLine4 = DCSBIOSControlLocator.GetDCSBIOSOutput("DED_LINE_4");
                DCSBIOSStringManager.AddListeningAddress(_DEDLine4);

                _DEDLine5 = DCSBIOSControlLocator.GetDCSBIOSOutput("DED_LINE_5");
                DCSBIOSStringManager.AddListeningAddress(_DEDLine5);

                StartListeningForHidPanelChanges();
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                throw new ArgumentOutOfRangeException($"Critical error during PZ69 panel Startup [{ex.Message}]");
            }
        }

        private bool DedFrequencyHasChanged(FrequencyType frequencyType, string rawFrequencyString)
        {
            if (_dedFrequencies.ContainsKey(frequencyType) && _dedFrequencies[frequencyType].Equals(rawFrequencyString))
            {
                return false;
            }

            if (!_dedFrequencies.ContainsKey(frequencyType)) 
            { 
                _dedFrequencies.Add(frequencyType, rawFrequencyString); 
                return true;
            }

            _dedFrequencies[frequencyType] = rawFrequencyString;

            return true;
        }

        private void TryIdentifyCurrentDedPage(string dedLineData)
        {
            _CurrentDedPage = DEDPageIdentification.Undefined;
            
            if (dedLineData.Length < 25)
                return;
            
            if (dedLineData.Substring(1, 3) == "UHF" && dedLineData.Substring(14, 4) == "STPT")
                _CurrentDedPage = DEDPageIdentification.Home;

            //" TCN REC     ILS  ON      "
            if (dedLineData.Substring(1, 3) == "TCN" && dedLineData.Substring(13, 3) == "ILS")
                _CurrentDedPage = DEDPageIdentification.TacanIls;
        }

        private bool DEDLineStringDataChanged(int dedLine, string dedLineData)
        {
            bool changes = false;
            switch (dedLine) {
                case 1:
                    TryIdentifyCurrentDedPage(dedLineData);
                    if (_CurrentDedPage == DEDPageIdentification.Home)
                    {
                        //" UHF  225.32  STPT a  1  " (25)
                        if (dedLineData.Substring(1, 3) == "UHF")
                            changes = DedFrequencyHasChanged(FrequencyType.UHFActive, dedLineData.Substring(6, 6).Trim()) == true;
                        if (dedLineData.Substring(14, 4) == "STPT")
                            changes = DedFrequencyHasChanged(FrequencyType.Steerpoint, dedLineData.Substring(20, 3).Trim()) == true;
                    }
                    break;
                case 2:
                    changes = false;
                    break;
                case 3:
                    if (_CurrentDedPage == DEDPageIdentification.Home)
                    {
                        //" VHF  145.65   14:20:13  " (25)
                        if (dedLineData.Substring(1, 3) == "VHF")
                            changes = DedFrequencyHasChanged(FrequencyType.VHFActive, dedLineData.Substring(6, 6).Trim()) == true;
                        if (dedLineData.Substring(17, 1) == ":" && dedLineData.Substring(20, 1) == ":")
                            changes = DedFrequencyHasChanged(FrequencyType.Time, dedLineData.Substring(15, 8).Trim()) == true;
                    }
                    break;
                case 4:
                    if (_CurrentDedPage == DEDPageIdentification.TacanIls)
                    {
                        //"CHAN* 22*   FRQ  108.10  "
                        if (dedLineData.Substring(0, 4) == "CHAN")
                            changes = DedFrequencyHasChanged(FrequencyType.Tacan, dedLineData.Substring(5, 3).Trim()) == true;
                        if (dedLineData.Substring(12, 3) == "FRQ" && dedLineData.Substring(16, 1) != "*") //ignore temp value
                            changes = DedFrequencyHasChanged(FrequencyType.Ils, dedLineData.Substring(17, 6).Trim()) == true;
                    }
                    break;
                case 5:
                    changes = false;
                    break;
                default: 
                    return false;
            }
            return changes;
        }

        public void DCSBIOSStringReceived(object sender, DCSBIOSStringDataEventArgs e)
        {
            try
            {
                //if (e.Address.Equals(_DEDLine1.Address) ||
                //    e.Address.Equals(_DEDLine2.Address) ||
                //    e.Address.Equals(_DEDLine3.Address) ||
                //    e.Address.Equals(_DEDLine4.Address) ||
                //    e.Address.Equals(_DEDLine5.Address))
                //{
                //    Debug.WriteLine("****** " + e.StringData.Length + "chars   ->" + e.StringData + "<-\n");
                //}
                bool updateLcd = false;
                if (e.Address.Equals(_DEDLine1.Address)){
                    updateLcd = DEDLineStringDataChanged(1, e.StringData);
                }
                if (e.Address.Equals(_DEDLine2.Address))
                {
                    updateLcd = DEDLineStringDataChanged(2, e.StringData);
                }
                if (e.Address.Equals(_DEDLine3.Address))
                {
                    updateLcd = DEDLineStringDataChanged(3, e.StringData);
                }
                if (e.Address.Equals(_DEDLine4.Address))
                {
                    updateLcd = DEDLineStringDataChanged(4, e.StringData);
                }
                if (e.Address.Equals(_DEDLine5.Address))
                {
                    updateLcd = DEDLineStringDataChanged(5, e.StringData);
                }

                if (updateLcd)
                {
                    Interlocked.Increment(ref _doUpdatePanelLCD);
                    ShowFrequenciesOnPanel();
                }
                DataHasBeenReceivedFromDCSBIOS = true;
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
                        var radioPanelKnob = (RadioPanelKnobF16C)radioPanelKnobObject;

                        switch (radioPanelKnob.RadioPanelPZ69Knob)
                        {
                            case RadioPanelPZ69KnobsF16C.UPPER_UHF:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetUpperRadioMode(CurrentF16CRadioMode.UHF);
                                    }
                                    break;
                                }

                            case RadioPanelPZ69KnobsF16C.UPPER_VHF:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetUpperRadioMode(CurrentF16CRadioMode.VHF);
                                    }
                                    break;
                                }

                            case RadioPanelPZ69KnobsF16C.UPPER_TACAN:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetUpperRadioMode(CurrentF16CRadioMode.TACAN);
                                    }
                                    break;
                                }
                            case RadioPanelPZ69KnobsF16C.UPPER_ILS:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetUpperRadioMode(CurrentF16CRadioMode.ILS);
                                    }
                                    break;
                                }
                            case RadioPanelPZ69KnobsF16C.UPPER_NO_USE2:
                            case RadioPanelPZ69KnobsF16C.UPPER_NO_USE3:
                            case RadioPanelPZ69KnobsF16C.UPPER_NO_USE4:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetUpperRadioMode(CurrentF16CRadioMode.NOUSE);
                                    }
                                    break;
                                }

                            case RadioPanelPZ69KnobsF16C.LOWER_UHF:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetLowerRadioMode(CurrentF16CRadioMode.UHF);
                                    }
                                    break;
                                }

                            case RadioPanelPZ69KnobsF16C.LOWER_VHF:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetLowerRadioMode(CurrentF16CRadioMode.VHF);
                                    }
                                    break;
                                }

                            case RadioPanelPZ69KnobsF16C.LOWER_TACAN:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetLowerRadioMode(CurrentF16CRadioMode.TACAN);
                                    }
                                    break;
                                }
                            case RadioPanelPZ69KnobsF16C.LOWER_ILS:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetLowerRadioMode(CurrentF16CRadioMode.ILS);
                                    }
                                    break;
                                }
                            case RadioPanelPZ69KnobsF16C.LOWER_NO_USE2:
                            case RadioPanelPZ69KnobsF16C.LOWER_NO_USE3:
                            case RadioPanelPZ69KnobsF16C.LOWER_NO_USE4:
                                {
                                    if (radioPanelKnob.IsOn)
                                    {
                                        SetLowerRadioMode(CurrentF16CRadioMode.NOUSE);
                                    }
                                    break;
                                }

                            case RadioPanelPZ69KnobsF16C.UPPER_LARGE_FREQ_WHEEL_INC:
                            case RadioPanelPZ69KnobsF16C.UPPER_LARGE_FREQ_WHEEL_DEC:
                            case RadioPanelPZ69KnobsF16C.UPPER_SMALL_FREQ_WHEEL_INC:
                            case RadioPanelPZ69KnobsF16C.UPPER_SMALL_FREQ_WHEEL_DEC:
                            case RadioPanelPZ69KnobsF16C.LOWER_LARGE_FREQ_WHEEL_INC:
                            case RadioPanelPZ69KnobsF16C.LOWER_LARGE_FREQ_WHEEL_DEC:
                            case RadioPanelPZ69KnobsF16C.LOWER_SMALL_FREQ_WHEEL_INC:
                            case RadioPanelPZ69KnobsF16C.LOWER_SMALL_FREQ_WHEEL_DEC:
                                {
                                    // Ignore for now, done in adjustfrequency function
                                    break;
                                }

                            case RadioPanelPZ69KnobsF16C.UPPER_FREQ_SWITCH:
                                {
                                    _upperFreqSwitchPressedDown = radioPanelKnob.IsOn;
                                    if (_currentUpperRadioMode == CurrentF16CRadioMode.TACAN || _currentUpperRadioMode == CurrentF16CRadioMode.ILS)
                                    {
                                        DCSBIOS.Send(EHSI_MODE_COMMAND);
                                    }
                                    break;
                                }

                            case RadioPanelPZ69KnobsF16C.LOWER_FREQ_SWITCH:
                                {
                                    _lowerFreqSwitchPressedDown = radioPanelKnob.IsOn;
                                    if (_currentLowerRadioMode == CurrentF16CRadioMode.TACAN || _currentLowerRadioMode == CurrentF16CRadioMode.ILS)
                                    {
                                        DCSBIOS.Send(EHSI_MODE_COMMAND);
                                    }
                                    break;
                                }
                        }

                        if (PluginManager.PlugSupportActivated && PluginManager.HasPlugin())
                        {
                            PluginManager.DoEvent(DCSAircraft.SelectedAircraft.Description, HIDInstance, PluginGamingPanelEnum.PZ69RadioPanel_PreProg_F16C, (int)radioPanelKnob.RadioPanelPZ69Knob, radioPanelKnob.IsOn, null);
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
                    var radioPanelKnobF16C = (RadioPanelKnobF16C)o;
                    if (radioPanelKnobF16C.IsOn)
                    {
                        switch (radioPanelKnobF16C.RadioPanelPZ69Knob)
                        {
                            case RadioPanelPZ69KnobsF16C.UPPER_LARGE_FREQ_WHEEL_INC:
                                {
                                    switch (_currentUpperRadioMode)
                                    {
                                        case CurrentF16CRadioMode.UHF:
                                            {
                                                if (!SkipUHFPresetDialChange())
                                                {
                                                }
                                                break;
                                            }
                                        case CurrentF16CRadioMode.VHF:
                                            {
                                                if (!SkipVHFPresetDialChange())
                                                {
                                                }
                                                break;
                                            }
                                        case CurrentF16CRadioMode.TACAN:
                                        case CurrentF16CRadioMode.ILS:
                                            {
                                                if (!SkipTacanIlsPresetDialChange())
                                                {
                                                    DCSBIOS.Send(EHSI_HDG_COMMAND_INC);
                                                }
                                                break;
                                            }
                                        case CurrentF16CRadioMode.NOUSE:
                                            {
                                                break;
                                            }
                                    }
                                    break;
                                }

                            case RadioPanelPZ69KnobsF16C.UPPER_LARGE_FREQ_WHEEL_DEC:
                                {
                                    switch (_currentUpperRadioMode)
                                    {
                                        case CurrentF16CRadioMode.UHF:
                                            {
                                                if (!SkipUHFPresetDialChange())
                                                {
                                                }
                                                break;
                                            }
                                        case CurrentF16CRadioMode.VHF:
                                            {
                                                if (!SkipVHFPresetDialChange())
                                                {
                                                }
                                                break;
                                            }
                                        case CurrentF16CRadioMode.TACAN:
                                        case CurrentF16CRadioMode.ILS:
                                            {
                                                if (!SkipTacanIlsPresetDialChange())
                                                {
                                                    DCSBIOS.Send(EHSI_HDG_COMMAND_DEC);
                                                }
                                                break;
                                            }
                                        case CurrentF16CRadioMode.NOUSE:
                                            {
                                                break;
                                            }

                                    }
                                    break;
                                }

                            case RadioPanelPZ69KnobsF16C.UPPER_SMALL_FREQ_WHEEL_INC:
                                {
                                    switch (_currentUpperRadioMode)
                                    {
                                        case CurrentF16CRadioMode.UHF:
                                            {
                                                if (!SkipUHFPresetDialChange())
                                                {
                                                    DCSBIOS.Send(UHF_VOLUME_COMMAND_INC);
                                                }
                                                break;
                                            }
                                        case CurrentF16CRadioMode.VHF:
                                            {
                                                if (!SkipVHFPresetDialChange())
                                                {
                                                    DCSBIOS.Send(VHF_VOLUME_COMMAND_INC);
                                                }
                                                break;
                                            }
                                        case CurrentF16CRadioMode.TACAN:
                                        case CurrentF16CRadioMode.ILS:
                                            {
                                                if (!SkipTacanIlsPresetDialChange())
                                                {
                                                    DCSBIOS.Send(EHSI_CRS_COMMAND_INC);
                                                }
                                                break;
                                            }
                                        case CurrentF16CRadioMode.NOUSE:
                                            {
                                                break;
                                            }
                                    }
                                    break;
                                }

                            case RadioPanelPZ69KnobsF16C.UPPER_SMALL_FREQ_WHEEL_DEC:
                                {
                                    switch (_currentUpperRadioMode)
                                    {
                                        case CurrentF16CRadioMode.UHF:
                                            {
                                                if (!SkipUHFPresetDialChange())
                                                {
                                                    DCSBIOS.Send(UHF_VOLUME_COMMAND_DEC);
                                                }
                                                break;
                                            }
                                        case CurrentF16CRadioMode.VHF:
                                            {
                                                if (!SkipVHFPresetDialChange())
                                                {
                                                    DCSBIOS.Send(VHF_VOLUME_COMMAND_DEC);
                                                }
                                                break;
                                            }
                                        case CurrentF16CRadioMode.TACAN:
                                        case CurrentF16CRadioMode.ILS:
                                            {
                                                if (!SkipTacanIlsPresetDialChange())
                                                {
                                                    DCSBIOS.Send(EHSI_CRS_COMMAND_DEC);
                                                }
                                                break;
                                            }
                                        case CurrentF16CRadioMode.NOUSE:
                                            {
                                                break;
                                            }
                                    }
                                    break;
                                }

                            case RadioPanelPZ69KnobsF16C.LOWER_LARGE_FREQ_WHEEL_INC:
                                {
                                    switch (_currentLowerRadioMode)
                                    {
                                        case CurrentF16CRadioMode.UHF:
                                            {
                                                if (!SkipUHFPresetDialChange())
                                                {
                                                }
                                                break;
                                            }
                                        case CurrentF16CRadioMode.VHF:
                                            {
                                                if (!SkipVHFPresetDialChange())
                                                {
                                                }
                                                break;
                                            }
                                        case CurrentF16CRadioMode.TACAN:
                                        case CurrentF16CRadioMode.ILS:
                                            {
                                                if (!SkipTacanIlsPresetDialChange())
                                                {
                                                    DCSBIOS.Send(EHSI_HDG_COMMAND_INC);
                                                }
                                                break;
                                            }
                                        case CurrentF16CRadioMode.NOUSE:
                                            {
                                                break;
                                            }
                                    }
                                    break;
                                }

                            case RadioPanelPZ69KnobsF16C.LOWER_LARGE_FREQ_WHEEL_DEC:
                                {
                                    switch (_currentLowerRadioMode)
                                    {
                                        case CurrentF16CRadioMode.UHF:
                                            {
                                                if (!SkipUHFPresetDialChange())
                                                {
                                                }
                                                break;
                                            }
                                        case CurrentF16CRadioMode.VHF:
                                            {
                                                if (!SkipVHFPresetDialChange())
                                                {
                                                }
                                                break;
                                            }
                                        case CurrentF16CRadioMode.TACAN:
                                        case CurrentF16CRadioMode.ILS:
                                            {
                                                if (!SkipTacanIlsPresetDialChange())
                                                {
                                                    DCSBIOS.Send(EHSI_HDG_COMMAND_DEC);
                                                }
                                                break;
                                            }
                                        case CurrentF16CRadioMode.NOUSE:
                                            {
                                                break;
                                            }
                                    }
                                    break;
                                }

                            case RadioPanelPZ69KnobsF16C.LOWER_SMALL_FREQ_WHEEL_INC:
                                {
                                    switch (_currentLowerRadioMode)
                                    {
                                        case CurrentF16CRadioMode.UHF:
                                            {
                                                if (!SkipUHFPresetDialChange())
                                                {
                                                    DCSBIOS.Send(UHF_VOLUME_COMMAND_INC);
                                                }
                                                break;
                                            }
                                        case CurrentF16CRadioMode.VHF:
                                            {
                                                if (!SkipVHFPresetDialChange())
                                                {
                                                    DCSBIOS.Send(VHF_VOLUME_COMMAND_INC);
                                                }
                                                break;
                                            }
                                        case CurrentF16CRadioMode.TACAN:
                                        case CurrentF16CRadioMode.ILS:
                                            {
                                                if (!SkipTacanIlsPresetDialChange())
                                                {
                                                    DCSBIOS.Send(EHSI_CRS_COMMAND_INC);
                                                }
                                                break;
                                            }
                                        case CurrentF16CRadioMode.NOUSE:
                                            {
                                                break;
                                            }
                                    }
                                    break;
                                }

                            case RadioPanelPZ69KnobsF16C.LOWER_SMALL_FREQ_WHEEL_DEC:
                                {
                                    switch (_currentLowerRadioMode)
                                    {
                                        case CurrentF16CRadioMode.UHF:
                                            {
                                                if (!SkipUHFPresetDialChange())
                                                {
                                                    DCSBIOS.Send(UHF_VOLUME_COMMAND_DEC);
                                                }
                                                break;
                                            }
                                        case CurrentF16CRadioMode.VHF:
                                            {
                                                if (!SkipVHFPresetDialChange())
                                                {
                                                    DCSBIOS.Send(VHF_VOLUME_COMMAND_DEC);
                                                }
                                                break;
                                            }
                                        case CurrentF16CRadioMode.TACAN:
                                        case CurrentF16CRadioMode.ILS:
                                            {
                                                if (!SkipTacanIlsPresetDialChange())
                                                {
                                                    DCSBIOS.Send(EHSI_CRS_COMMAND_DEC);
                                                }
                                                break;
                                            }
                                        case CurrentF16CRadioMode.NOUSE:
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
            if (!_dedFrequencies.ContainsKey(frequencyType) || string.IsNullOrEmpty(_dedFrequencies[frequencyType]))
                return string.Empty;

             return _dedFrequencies[frequencyType];
        }

        private bool IsPresetFrequency(string frequency)
        {
            return !frequency.Contains(".");
        }

        private void SetFrequencyBytes(FrequencyType frequencyType, Pz69Mode pz69mode, ref byte[] bytes)
        {
            switch (frequencyType) {
                case (FrequencyType.UHFActive):
                    lock (_uhfFrequencyActive)
                    {
                        _uhfFrequencyActive = GetSafeFrequency(FrequencyType.UHFActive);
                        if (!string.IsNullOrEmpty(_uhfFrequencyActive))
                        {
                            if (IsPresetFrequency(_uhfFrequencyActive))
                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, Convert.ToUInt32(_uhfFrequencyActive), pz69mode == Pz69Mode.UPPER ? PZ69LCDPosition.UPPER_ACTIVE_LEFT : PZ69LCDPosition.LOWER_ACTIVE_LEFT);
                            else
                                SetPZ69DisplayBytes(ref bytes, double.Parse(_uhfFrequencyActive, NumberFormatInfoFullDisplay), 2, pz69mode == Pz69Mode.UPPER ? PZ69LCDPosition.UPPER_ACTIVE_LEFT : PZ69LCDPosition.LOWER_ACTIVE_LEFT);
                        }
                        else
                        {
                            SetPZ69DisplayBlank(ref bytes, pz69mode == Pz69Mode.UPPER ? PZ69LCDPosition.UPPER_ACTIVE_LEFT : PZ69LCDPosition.LOWER_ACTIVE_LEFT);
                        }
                    }
                    break;

                case (FrequencyType.VHFActive):
                    lock (_vhfFrequencyActive)
                    {
                         _vhfFrequencyActive = GetSafeFrequency(FrequencyType.VHFActive);
                        if (!string.IsNullOrEmpty(_vhfFrequencyActive))
                        {
                            if (IsPresetFrequency(_vhfFrequencyActive))
                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, Convert.ToUInt32(_vhfFrequencyActive), pz69mode == Pz69Mode.UPPER ? PZ69LCDPosition.UPPER_ACTIVE_LEFT : PZ69LCDPosition.LOWER_ACTIVE_LEFT) ;
                            else
                                SetPZ69DisplayBytes(ref bytes, double.Parse(_vhfFrequencyActive, NumberFormatInfoFullDisplay), 2, pz69mode == Pz69Mode.UPPER ? PZ69LCDPosition.UPPER_ACTIVE_LEFT : PZ69LCDPosition.LOWER_ACTIVE_LEFT);
                        }
                        else
                        {
                            SetPZ69DisplayBlank(ref bytes, pz69mode == Pz69Mode.UPPER ? PZ69LCDPosition.UPPER_ACTIVE_LEFT : PZ69LCDPosition.LOWER_ACTIVE_LEFT);
                        }
                    }
                    break;
                case (FrequencyType.Steerpoint):
                    lock (_steerpointActive)
                    {
                        _steerpointActive = GetSafeFrequency(FrequencyType.Steerpoint);
                        if (!string.IsNullOrEmpty(_steerpointActive))
                        {
                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, Convert.ToUInt32(_steerpointActive), pz69mode == Pz69Mode.UPPER ? PZ69LCDPosition.UPPER_STBY_RIGHT : PZ69LCDPosition.LOWER_STBY_RIGHT);
                        }
                        else
                        {
                            SetPZ69DisplayBlank(ref bytes, pz69mode == Pz69Mode.UPPER ? PZ69LCDPosition.UPPER_STBY_RIGHT : PZ69LCDPosition.LOWER_STBY_RIGHT);
                        }
                    }
                    break;
                case (FrequencyType.Time):
                    lock (_timeActive)
                    {
                        _timeActive = GetSafeFrequency(FrequencyType.Time);
                        if (!string.IsNullOrEmpty(_timeActive))
                        {
                            SetPZ69DisplayBytesDefault(ref bytes, GeTimeAsFrequency(_timeActive, false), pz69mode == Pz69Mode.UPPER ? PZ69LCDPosition.UPPER_STBY_RIGHT : PZ69LCDPosition.LOWER_STBY_RIGHT);
                        }
                        else
                        {
                            SetPZ69DisplayBlank(ref bytes, pz69mode == Pz69Mode.UPPER ? PZ69LCDPosition.UPPER_STBY_RIGHT : PZ69LCDPosition.LOWER_STBY_RIGHT);
                        }
                    }
                    break;
                case (FrequencyType.Tacan):
                    lock (_tacanFrequencyActive)
                    {
                        _tacanFrequencyActive = GetSafeFrequency(FrequencyType.Tacan);
                        if (!string.IsNullOrEmpty(_tacanFrequencyActive))
                        {
                            if (IsPresetFrequency(_tacanFrequencyActive))
                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, Convert.ToUInt32(_tacanFrequencyActive), pz69mode == Pz69Mode.UPPER ? PZ69LCDPosition.UPPER_ACTIVE_LEFT : PZ69LCDPosition.LOWER_ACTIVE_LEFT);
                            else
                                SetPZ69DisplayBytes(ref bytes, double.Parse(_tacanFrequencyActive, NumberFormatInfoFullDisplay), 2, pz69mode == Pz69Mode.UPPER ? PZ69LCDPosition.UPPER_ACTIVE_LEFT : PZ69LCDPosition.LOWER_ACTIVE_LEFT);
                            
                            SetPZ69DisplayBlank(ref bytes, pz69mode == Pz69Mode.UPPER ? PZ69LCDPosition.UPPER_STBY_RIGHT : PZ69LCDPosition.LOWER_STBY_RIGHT);
                        }
                        else
                        {
                            SetPZ69DisplayBlank(ref bytes, pz69mode == Pz69Mode.UPPER ? PZ69LCDPosition.UPPER_ACTIVE_LEFT : PZ69LCDPosition.LOWER_ACTIVE_LEFT);
                        }
                    }
                    break;
                case (FrequencyType.Ils):
                    lock (_ilsFrequencyActive)
                    {
                        _ilsFrequencyActive = GetSafeFrequency(FrequencyType.Ils);
                        if (!string.IsNullOrEmpty(_ilsFrequencyActive))
                        {
                            if (IsPresetFrequency(_ilsFrequencyActive))
                                SetPZ69DisplayBytesUnsignedInteger(ref bytes, Convert.ToUInt32(_ilsFrequencyActive), pz69mode == Pz69Mode.UPPER ? PZ69LCDPosition.UPPER_ACTIVE_LEFT : PZ69LCDPosition.LOWER_ACTIVE_LEFT);
                            else
                                SetPZ69DisplayBytes(ref bytes, double.Parse(_ilsFrequencyActive, NumberFormatInfoFullDisplay), 2, pz69mode == Pz69Mode.UPPER ? PZ69LCDPosition.UPPER_ACTIVE_LEFT : PZ69LCDPosition.LOWER_ACTIVE_LEFT);

                            SetPZ69DisplayBlank(ref bytes, pz69mode == Pz69Mode.UPPER ? PZ69LCDPosition.UPPER_STBY_RIGHT : PZ69LCDPosition.LOWER_STBY_RIGHT);
                        }
                        else
                        {
                            SetPZ69DisplayBlank(ref bytes, pz69mode == Pz69Mode.UPPER ? PZ69LCDPosition.UPPER_ACTIVE_LEFT : PZ69LCDPosition.LOWER_ACTIVE_LEFT);
                        }
                    }
                    break;

                default:
                    throw new Exception("Unsupported frequency Type");
            }
        }

        private string GeTimeAsFrequency(string rawValue, bool cutSeconds)
        {
            //Too bad there are no 6 digits on the PZ69 :-\
            try
            {
                if (rawValue.Substring(2, 1) != ":" && rawValue.Substring(5, 1) != ":")
                    throw new Exception();
                else
                {
                    if (cutSeconds)
                    {
                        return $"{rawValue.Substring(0, 2)}.{rawValue.Substring(3, 2)}.{rawValue.Substring(6, 2)}";
                    }
                    else
                    { //cut the first digit of hour part
                        return $"{rawValue.Substring(1, 1)}.{rawValue.Substring(3, 2)}.{rawValue.Substring(6, 2)}";
                    }
                }
            }
            catch {
                return string.Empty;
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
                        case CurrentF16CRadioMode.UHF:
                            {
                                SetFrequencyBytes(FrequencyType.UHFActive, Pz69Mode.UPPER, ref bytes);
                                SetFrequencyBytes(FrequencyType.Steerpoint, Pz69Mode.UPPER, ref bytes);
                                break;
                            }
                        case CurrentF16CRadioMode.VHF:
                            {
                                SetFrequencyBytes(FrequencyType.VHFActive, Pz69Mode.UPPER, ref bytes);
                                SetFrequencyBytes(FrequencyType.Time, Pz69Mode.UPPER, ref bytes);
                                break;
                            }
                        case CurrentF16CRadioMode.TACAN:
                            {
                                SetFrequencyBytes(FrequencyType.Tacan, Pz69Mode.UPPER, ref bytes);
                                break;
                            }
                        case CurrentF16CRadioMode.ILS:
                            {
                                SetFrequencyBytes(FrequencyType.Ils, Pz69Mode.UPPER, ref bytes);
                                break;
                            }
                        case CurrentF16CRadioMode.NOUSE:
                            {
                                SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.UPPER_ACTIVE_LEFT);
                                SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.UPPER_STBY_RIGHT);
                                break;
                            }
                    }
                    switch (_currentLowerRadioMode)
                    {
                        case CurrentF16CRadioMode.UHF:
                            {
                                SetFrequencyBytes(FrequencyType.UHFActive, Pz69Mode.LOWER, ref bytes);
                                SetFrequencyBytes(FrequencyType.Steerpoint, Pz69Mode.LOWER, ref bytes);
                                break;
                            }
                        case CurrentF16CRadioMode.VHF:
                            {
                                SetFrequencyBytes(FrequencyType.VHFActive, Pz69Mode.LOWER, ref bytes);
                                SetFrequencyBytes(FrequencyType.Time, Pz69Mode.LOWER, ref bytes);
                                break;
                            }
                        case CurrentF16CRadioMode.TACAN:
                            {
                                SetFrequencyBytes(FrequencyType.Tacan, Pz69Mode.LOWER, ref bytes);
                                break;
                            }
                        case CurrentF16CRadioMode.ILS:
                            {
                                SetFrequencyBytes(FrequencyType.Ils, Pz69Mode.LOWER, ref bytes);
                                break;
                            }
                        case CurrentF16CRadioMode.NOUSE:
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
            SaitekPanelKnobs = RadioPanelKnobF16C.GetRadioPanelKnobs();
        }

        private void SetUpperRadioMode(CurrentF16CRadioMode currentF16CRadioMode)
        {
            try
            {
                _currentUpperRadioMode = currentF16CRadioMode;
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }

        private void SetLowerRadioMode(CurrentF16CRadioMode currentF16CRadioMode)
        {
            try
            {
                _currentLowerRadioMode = currentF16CRadioMode;

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
            return false;
        }

        private bool SkipUHFPresetDialChange()
        {
            return false;        
        }
        private bool SkipTacanIlsPresetDialChange()
        {
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
    }
}
