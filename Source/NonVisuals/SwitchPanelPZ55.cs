using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using ClassLibraryCommon;
using DCS_BIOS;
using HidLibrary;

namespace NonVisuals
{
    public enum SwitchPanelPZ55LEDPosition : byte
    {
        UP = 0x0,
        LEFT = 0x1,
        RIGHT = 0x2
    }

    public class SwitchPanelPZ55 : SaitekPanel
    {

        /*
         * For a specific toggle/switch/lever/knob the PZ55 can have :
         * - single key binding
         * - seqenced key binding
         * - DCS-BIOS control
         */
        private HashSet<DCSBIOSBindingPZ55> _dcsBiosBindings = new HashSet<DCSBIOSBindingPZ55>();
        private HashSet<KeyBindingPZ55> _keyBindings = new HashSet<KeyBindingPZ55>();
        private HashSet<BIPLinkPZ55> _bipLinks = new HashSet<BIPLinkPZ55>();
        //public static SwitchPanelPZ55 SwitchPanelPZ55SO;
        private HashSet<SwitchPanelKey> _switchPanelKeys = new HashSet<SwitchPanelKey>();
        private bool _isFirstNotification = true;
        private byte[] _oldSwitchPanelValue = { 0, 0, 0 };
        private byte[] _newSwitchPanelValue = { 0, 0, 0 };
        //private HidDevice _hidReadDevice;
        //private HidDevice _hidWriteDevice;
        private SwitchPanelPZ55LEDs _ledUpperColor = SwitchPanelPZ55LEDs.ALL_DARK;
        private SwitchPanelPZ55LEDs _ledLeftColor = SwitchPanelPZ55LEDs.ALL_DARK;
        private SwitchPanelPZ55LEDs _ledRightColor = SwitchPanelPZ55LEDs.ALL_DARK;
        private List<DcsOutputAndColorBindingPZ55> _listColorOutputBinding = new List<DcsOutputAndColorBindingPZ55>();
        private object _dcsBiosDataReceivedLock = new object();
        private bool _manualLandingGearLeds;
        private Thread _manualLandingGearThread;

        public SwitchPanelPZ55(HIDSkeleton hidSkeleton) : base(SaitekPanelsEnum.PZ55SwitchPanel, hidSkeleton)
        {
            //Fixed values
            VendorId = 0x6A3;
            ProductId = 0xD67;
            CreateSwitchKeys();
            //SwitchPanelPZ55SO = this;
            Startup();
        }

        public override sealed void Startup()
        {
            try
            {
                if (HIDSkeletonBase.HIDReadDevice != null && !Closed)
                {
                    HIDSkeletonBase.HIDReadDevice.ReadReport(OnReport);
                }
            }
            catch (Exception ex)
            {
                Common.DebugP("SwitchPanelPZ55.StartUp() : " + ex.Message);
                SetLastException(ex);
            }
        }

        public override void Shutdown()
        {
            try
            {
                Closed = true;
            }
            catch (Exception e)
            {
                SetLastException(e);
            }
        }

        public override void ImportSettings(List<string> settings)
        {
            //Clear current bindings
            ClearSettings();
            if (settings == null || settings.Count == 0)
            {
                return;
            }
            foreach (var setting in settings)
            {
                if (!setting.StartsWith("#") && setting.Length > 2 && setting.Contains(InstanceId))
                {
                    if (setting.StartsWith("SwitchPanelKey{"))
                    {
                        var keyBinding = new KeyBindingPZ55();
                        keyBinding.ImportSettings(setting);
                        _keyBindings.Add(keyBinding);
                    }
                    else if (setting.StartsWith("SwitchPanelLed"))
                    {
                        var colorOutput = new DcsOutputAndColorBindingPZ55();
                        colorOutput.ImportSettings(setting);
                        _listColorOutputBinding.Add(colorOutput);
                    }
                    else if (setting.StartsWith("SwitchPanelDCSBIOSControl{"))
                    {
                        var dcsBIOSBindingPZ55 = new DCSBIOSBindingPZ55();
                        dcsBIOSBindingPZ55.ImportSettings(setting);
                        _dcsBiosBindings.Add(dcsBIOSBindingPZ55);
                    }
                    else if (setting.StartsWith("SwitchPanelBIPLink{"))
                    {
                        var bipLinkPZ55 = new BIPLinkPZ55();
                        bipLinkPZ55.ImportSettings(setting);
                        _bipLinks.Add(bipLinkPZ55);
                    }
                    else if (setting.StartsWith("ManualLandingGearLEDs{"))
                    {
                        _manualLandingGearLeds = setting.Contains("True");
                    }
                }
            }
            OnSettingsApplied();
        }

        public override List<string> ExportSettings()
        {
            if (Closed)
            {
                return null;
            }
            var result = new List<string>();

            foreach (var keyBinding in _keyBindings)
            {
                if (keyBinding.OSKeyPress != null)
                {
                    result.Add(keyBinding.ExportSettings());
                }
            }
            foreach (var dcsBiosBinding in _dcsBiosBindings)
            {
                if (dcsBiosBinding.DCSBIOSInputs.Count > 0)
                {
                    result.Add(dcsBiosBinding.ExportSettings());
                }
            }
            foreach (var bipLink in _bipLinks)
            {
                if (bipLink.BIPLights.Count > 0)
                {
                    result.Add(bipLink.ExportSettings());
                }
            }
            Common.DebugP("Exporting " + _listColorOutputBinding.Count + " ColorOutBindings from SwitchPanelPZ55");
            foreach (var colorOutputBinding in _listColorOutputBinding)
            {
                result.Add(colorOutputBinding.ExportSettings());
            }
            result.Add("ManualLandingGearLEDs{" + _manualLandingGearLeds + "}");
            return result;
        }

        public override void SavePanelSettings(object sender, ProfileHandlerEventArgs e)
        {
            e.ProfileHandlerEA.RegisterProfileData(this, ExportSettings());
        }

        public override void DcsBiosDataReceived(object sender, DCSBIOSDataEventArgs e)
        {

            lock (_dcsBiosDataReceivedLock)
            {
                UpdateCounter(e.Address, e.Data);
                CheckDcsDataForColorChangeHook(e.Address, e.Data);
            }

        }

        public override void ClearSettings()
        {
            _keyBindings.Clear();
            _listColorOutputBinding.Clear();
            _dcsBiosBindings.Clear();
        }

        public HashSet<KeyBindingPZ55> KeyBindingsHashSet
        {
            get { return _keyBindings; }
            set { _keyBindings = value; }
        }

        public HashSet<BIPLinkPZ55> BIPLinkHashSet
        {
            get { return _bipLinks; }
            set { _bipLinks = value; }
        }

        private void PZ55SwitchChanged(SwitchPanelKey switchPanelKey)
        {
            if (!ForwardKeyPresses)
            {
                return;
            }
            foreach (var keyBinding in _keyBindings)
            {
                if (keyBinding.SwitchPanelPZ55Key == switchPanelKey.SwitchPanelPZ55Key && keyBinding.WhenTurnedOn == switchPanelKey.IsOn)
                {
                    keyBinding.OSKeyPress.Execute();
                }
            }
        }

        private void SetLandingGearLedsManually(PanelLEDColor panelLEDColor)
        {
            try
            {
                var random = new Random();
                var upSet = false;
                var rightSet = false;
                var leftSet = false;
                var delayUp = random.Next(1500, 10000);
                var delayRight = random.Next(1500, 10000);
                var delayLeft = random.Next(1500, 10000);
                var millisecsStart = DateTime.Now.Ticks / 10000;

                // Corrected the 'Manual LEDS' operation.
                // Now when the gear knob selection is changed, just like a real aircraft
                // the lights go to their 'Transit' state showing RED.
                // Then afterwards they change to their final colour (GREEN = DOWN, DARK = UP)
                SetLandingGearLED(SwitchPanelPZ55LEDPosition.UP, PanelLEDColor.RED);
                SetLandingGearLED(SwitchPanelPZ55LEDPosition.RIGHT, PanelLEDColor.RED);
                SetLandingGearLED(SwitchPanelPZ55LEDPosition.LEFT, PanelLEDColor.RED);

                while (true)
                {
                    var millisecsNow = DateTime.Now.Ticks / 10000;
                    Debug.Print("millisecsNow - millisecsStart > delayUp " + (millisecsNow - millisecsStart) + " " + delayUp);
                    Debug.Print("millisecsNow - millisecsStart > delayRight " + (millisecsNow - millisecsStart) + " " + delayRight);
                    Debug.Print("millisecsNow - millisecsStart > delayLeft " + (millisecsNow - millisecsStart) + " " + delayLeft);
                    if (millisecsNow - millisecsStart > delayUp && !upSet)
                    {
                        SetLandingGearLED(SwitchPanelPZ55LEDPosition.UP, panelLEDColor);
                        upSet = true;
                    }
                    if (millisecsNow - millisecsStart > delayRight && !rightSet)
                    {
                        SetLandingGearLED(SwitchPanelPZ55LEDPosition.RIGHT, panelLEDColor);
                        rightSet = true;
                    }
                    if (millisecsNow - millisecsStart > delayLeft && !leftSet)
                    {
                        SetLandingGearLED(SwitchPanelPZ55LEDPosition.LEFT, panelLEDColor);
                        leftSet = true;
                    }
                    if (leftSet && upSet && rightSet)
                    {
                        break;
                    }
                    Thread.Sleep(10);
                }
            }
            catch (ThreadAbortException)
            {
            }
            catch (Exception ex)
            {
                Common.LogError(90234, ex, "SetLandingGearLedsManually");
                throw;
            }
        }

        private void PZ55SwitchChanged(IEnumerable<object> hashSet)
        {
            if (!ForwardKeyPresses)
            {
                return;
            }
            foreach (var switchPanelKeyObject in hashSet)
            {
                //Looks which switches has been switched and sees whether any key emulation has been tied to them.
                var switchPanelKey = (SwitchPanelKey)switchPanelKeyObject;
                var found = false;

                //Look if leds are manually operated
                if (_manualLandingGearLeds)
                {
                    if (switchPanelKey.SwitchPanelPZ55Key == SwitchPanelPZ55Keys.LEVER_GEAR_UP && switchPanelKey.IsOn)
                    {
                        if (_manualLandingGearThread != null)
                        {
                            _manualLandingGearThread.Abort();
                        }
                        // Changed Lights to go DARK when gear level is selected to UP, instead of RED.
                        _manualLandingGearThread = new Thread(() => SetLandingGearLedsManually(PanelLEDColor.DARK));
                        _manualLandingGearThread.Start();
                    }
                    else if (switchPanelKey.SwitchPanelPZ55Key == SwitchPanelPZ55Keys.LEVER_GEAR_DOWN && switchPanelKey.IsOn)
                    {
                        if (_manualLandingGearThread != null)
                        {
                            _manualLandingGearThread.Abort();
                        }
                        _manualLandingGearThread = new Thread(() => SetLandingGearLedsManually(PanelLEDColor.GREEN));
                        _manualLandingGearThread.Start();
                    }
                }
                foreach (var keyBinding in _keyBindings)
                {
                    if (keyBinding.OSKeyPress != null && keyBinding.SwitchPanelPZ55Key == switchPanelKey.SwitchPanelPZ55Key && keyBinding.WhenTurnedOn == switchPanelKey.IsOn)
                    {
                        keyBinding.OSKeyPress.Execute();
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    foreach (var dcsBiosBinding in _dcsBiosBindings)
                    {
                        if (dcsBiosBinding.DCSBIOSInputs.Count > 0 && dcsBiosBinding.SwitchPanelPZ55Key == switchPanelKey.SwitchPanelPZ55Key && dcsBiosBinding.WhenTurnedOn == switchPanelKey.IsOn)
                        {
                            dcsBiosBinding.SendDCSBIOSCommands();
                            break;
                        }
                    }
                }
            }
        }

        internal void CheckDcsDataForColorChangeHook(uint address, uint data)
        {
            try
            {


                foreach (var cavb in _listColorOutputBinding)
                {
                    if (address == cavb.DCSBiosOutputLED.Address)
                    {
                        if (cavb.DCSBiosOutputLED.CheckForValueMatch(data))
                        {
                            /*
                             * If user tests cockpit lights (especially A-10C and handle light) and triggers (forces) a light change that light
                             * will stay on unless there is a overriding light for example landing gear down.
                             * Landing gear down light is sent regularly but it is processed only if value has changed. This is not the case here 
                             * as it is GREEN and should nevertheless override the RED light.
                             * Will this be efficient?
                             * https://sourceforge.net/p/flightpanels/tickets/13/
                             */
                            var color = cavb.LEDColor;
                            var position = (SwitchPanelPZ55LEDPosition)cavb.SaitekLEDPosition.Position;
                            var color2 = GetSwitchPanelPZ55LEDColor(position, color);
                            if (position == SwitchPanelPZ55LEDPosition.UP && color2 != _ledUpperColor)
                            {

                                SetLandingGearLED(cavb);
                            }
                            else if (position == SwitchPanelPZ55LEDPosition.LEFT && color2 != _ledLeftColor)
                            {

                                SetLandingGearLED(cavb);
                            }
                            else if (position == SwitchPanelPZ55LEDPosition.RIGHT && color2 != _ledRightColor)
                            {

                                SetLandingGearLED(cavb);
                            }
                        }
                        if (cavb.DCSBiosOutputLED.CheckForValueMatchAndChange(data))
                        {

                            SetLandingGearLED(cavb);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Common.LogError(981234, ex, "CheckDcsDataForColorChangeHook(uint address, uint data)");
                throw;
            }
        }

        public string GetKeyPressForLoggingPurposes(SwitchPanelKey switchPanelKey)
        {
            var result = "";
            foreach (var keyBinding in _keyBindings)
            {
                if (keyBinding.OSKeyPress != null && keyBinding.SwitchPanelPZ55Key == switchPanelKey.SwitchPanelPZ55Key && keyBinding.WhenTurnedOn == switchPanelKey.IsOn)
                {
                    result = keyBinding.OSKeyPress.GetNonFunctioningVirtualKeyCodesAsString();
                }
            }
            return result;
        }

        public void AddOrUpdateSingleKeyBinding(SwitchPanelPZ55Keys switchPanelPZ55Key, string keys, KeyPressLength keyPressLength, bool whenTurnedOn = true)
        {
            if (string.IsNullOrEmpty(keys))
            {
                var tmp = new SwitchPanelPZ55KeyOnOff(switchPanelPZ55Key, whenTurnedOn);
                ClearAllBindings(tmp);
                return;
            }
            var found = false;
            foreach (var keyBinding in _keyBindings)
            {
                if (keyBinding.SwitchPanelPZ55Key == switchPanelPZ55Key && keyBinding.WhenTurnedOn == whenTurnedOn)
                {
                    if (string.IsNullOrEmpty(keys))
                    {
                        keyBinding.OSKeyPress = null;
                    }
                    else
                    {
                        keyBinding.OSKeyPress = new OSKeyPress(keys, keyPressLength);
                        keyBinding.WhenTurnedOn = whenTurnedOn;
                    }
                    found = true;
                }
            }
            if (!found && !string.IsNullOrEmpty(keys))
            {
                var keyBinding = new KeyBindingPZ55();
                keyBinding.SwitchPanelPZ55Key = switchPanelPZ55Key;
                keyBinding.OSKeyPress = new OSKeyPress(keys, keyPressLength);
                keyBinding.WhenTurnedOn = whenTurnedOn;
                _keyBindings.Add(keyBinding);
            }
            Common.DebugP("SwitchPanelPZ55 _keyBindings : " + _keyBindings.Count);
            IsDirtyMethod();
        }

        public void ClearAllBindings(SwitchPanelPZ55KeyOnOff switchPanelPZ55KeyOnOff)
        {
            //This must accept lists
            foreach (var keyBinding in _keyBindings)
            {
                if (keyBinding.SwitchPanelPZ55Key == switchPanelPZ55KeyOnOff.SwitchPanelPZ55Key && keyBinding.WhenTurnedOn == switchPanelPZ55KeyOnOff.On)
                {
                    keyBinding.OSKeyPress = null;
                }
            }
            foreach (var dcsBiosBinding in _dcsBiosBindings)
            {
                if (dcsBiosBinding.SwitchPanelPZ55Key == switchPanelPZ55KeyOnOff.SwitchPanelPZ55Key && dcsBiosBinding.WhenTurnedOn == switchPanelPZ55KeyOnOff.On)
                {
                    dcsBiosBinding.DCSBIOSInputs.Clear();
                }
            }
            Common.DebugP("SwitchPanelPZ55 _keyBindings : " + _keyBindings.Count);
            Common.DebugP("SwitchPanelPZ55 _dcsBiosBindings : " + _dcsBiosBindings.Count);
            IsDirtyMethod();
        }

        public OSKeyPress AddOrUpdateSequencedKeyBinding(string information, SwitchPanelPZ55Keys switchPanelPZ55Key, SortedList<int, KeyPressInfo> sortedList, bool whenTurnedOn = true)
        {
            //This must accept lists
            var found = false;
            OSKeyPress osKeyPress = null;

            RemoveSwitchPanelKeyFromList(2, switchPanelPZ55Key, whenTurnedOn);
            foreach (var keyBinding in _keyBindings)
            {
                if (keyBinding.SwitchPanelPZ55Key == switchPanelPZ55Key && keyBinding.WhenTurnedOn == whenTurnedOn)
                {
                    if (sortedList.Count == 0)
                    {
                        keyBinding.OSKeyPress = null;
                    }
                    else
                    {
                        osKeyPress = new OSKeyPress(information, sortedList);
                        keyBinding.OSKeyPress = osKeyPress;
                        keyBinding.WhenTurnedOn = whenTurnedOn;
                    }
                    found = true;
                    break;
                }
            }
            if (!found && sortedList.Count > 0)
            {
                var keyBinding = new KeyBindingPZ55();
                keyBinding.SwitchPanelPZ55Key = switchPanelPZ55Key;
                osKeyPress = new OSKeyPress(information, sortedList);
                keyBinding.OSKeyPress = osKeyPress;
                keyBinding.WhenTurnedOn = whenTurnedOn;
                _keyBindings.Add(keyBinding);
            }
            IsDirtyMethod();
            return osKeyPress;
        }

        public void AddOrUpdateDCSBIOSBinding(SwitchPanelPZ55Keys switchPanelPZ55Key, List<DCSBIOSInput> dcsbiosInputs, string description, bool whenTurnedOn = true)
        {
            //!!!!!!!
            //If all DCS-BIOS commands has been deleted then provide a empty list, not null object!!!

            //This must accept lists
            var found = false;
            RemoveSwitchPanelKeyFromList(1, switchPanelPZ55Key, whenTurnedOn);
            foreach (var dcsBiosBinding in _dcsBiosBindings)
            {
                if (dcsBiosBinding.SwitchPanelPZ55Key == switchPanelPZ55Key && dcsBiosBinding.WhenTurnedOn == whenTurnedOn)
                {
                    dcsBiosBinding.DCSBIOSInputs = dcsbiosInputs;
                    dcsBiosBinding.WhenTurnedOn = whenTurnedOn;
                    dcsBiosBinding.Description = description;
                    found = true;
                    break;
                }
            }
            if (!found)
            {
                var dcsBiosBinding = new DCSBIOSBindingPZ55();
                dcsBiosBinding.SwitchPanelPZ55Key = switchPanelPZ55Key;
                dcsBiosBinding.DCSBIOSInputs = dcsbiosInputs;
                dcsBiosBinding.WhenTurnedOn = whenTurnedOn;
                dcsBiosBinding.Description = description;
                _dcsBiosBindings.Add(dcsBiosBinding);
            }
            IsDirtyMethod();
        }

        public BIPLinkPZ55 AddOrUpdateBIPLinkKeyBinding(SwitchPanelPZ55Keys switchPanelPZ55Key, BIPLinkPZ55 bipLinkPZ55, bool whenTurnedOn = true)
        {
            //This must accept lists
            var found = false;
            BIPLinkPZ55 tmpBIPLinkPZ55 = null;

            RemoveSwitchPanelKeyFromList(3, switchPanelPZ55Key, whenTurnedOn);
            foreach (var bipLink in _bipLinks)
            {
                if (bipLink.SwitchPanelPZ55Key == switchPanelPZ55Key && bipLink.WhenTurnedOn == whenTurnedOn)
                {
                    bipLink.BIPLights = bipLinkPZ55.BIPLights;
                    bipLink.Description = bipLinkPZ55.Description;
                    bipLink.SwitchPanelPZ55Key = switchPanelPZ55Key;
                    bipLink.WhenTurnedOn = whenTurnedOn;
                    tmpBIPLinkPZ55 = bipLink;
                    found = true;
                    break;
                }
            }
            if (!found && bipLinkPZ55.BIPLights.Count > 0)
            {
                bipLinkPZ55.SwitchPanelPZ55Key = switchPanelPZ55Key;
                bipLinkPZ55.WhenTurnedOn = whenTurnedOn;
                tmpBIPLinkPZ55 = bipLinkPZ55;
                _bipLinks.Add(bipLinkPZ55);
            }
            IsDirtyMethod();
            return tmpBIPLinkPZ55;
        }



        private void RemoveSwitchPanelKeyFromList(int list, SwitchPanelPZ55Keys switchPanelPZ55Key, bool whenTurnedOn = true)
        {
            switch (list)
            {
                case 1:
                    {
                        foreach (var keyBindingPZ55 in _keyBindings)
                        {
                            if (keyBindingPZ55.SwitchPanelPZ55Key == switchPanelPZ55Key && keyBindingPZ55.WhenTurnedOn == whenTurnedOn)
                            {
                                keyBindingPZ55.OSKeyPress = null;
                            }
                            break;
                        }
                        break;
                    }
                case 2:
                    {
                        foreach (var dcsBiosBinding in _dcsBiosBindings)
                        {
                            if (dcsBiosBinding.SwitchPanelPZ55Key == switchPanelPZ55Key && dcsBiosBinding.WhenTurnedOn == whenTurnedOn)
                            {
                                dcsBiosBinding.DCSBIOSInputs.Clear();
                            }
                            break;
                        }
                        break;
                    }
                case 3:
                    {
                        foreach (var bipLink in _bipLinks)
                        {
                            if (bipLink.SwitchPanelPZ55Key == switchPanelPZ55Key && bipLink.WhenTurnedOn == whenTurnedOn)
                            {
                                bipLink.BIPLights.Clear();
                            }
                            break;
                        }
                        break;
                    }
            }
        }

        private void IsDirtyMethod()
        {
            OnSettingsChanged();
            IsDirty = true;
        }

        public void Clear(SwitchPanelPZ55Keys switchPanelPZ55Key)
        {
            foreach (var keyBinding in _keyBindings)
            {
                if (keyBinding.SwitchPanelPZ55Key == switchPanelPZ55Key)
                {
                    keyBinding.OSKeyPress = null;
                }
            }
            IsDirtyMethod();
        }

        /*internal DCSBiosOutput GetOutput(PanelLEDColor switchPanelPZ55LEDColor, SwitchPanelPZ55LEDPosition switchPanelPZ55LEDPosition)
        {
            foreach (var colorOutputBinding in _listColorOutputBinding)
            {
                //todo
                if (colorOutputBinding.PanelPZ55LEDColor == switchPanelPZ55LEDColor && colorOutputBinding.PanelPZ55LEDPosition == switchPanelPZ55LEDPosition && colorOutputBinding.DCSBiosOutputLED.HasBeenSet)
                {
                    return colorOutputBinding.DCSBiosOutputLED;
                }
            }
            return null;
        }*/

        public bool LedIsConfigured(SwitchPanelPZ55LEDPosition switchPanelPZ55LEDPosition)
        {
            return _listColorOutputBinding.Any(colorOutputBinding => (SwitchPanelPZ55LEDPosition)colorOutputBinding.SaitekLEDPosition.GetPosition() == switchPanelPZ55LEDPosition);
        }

        public List<DcsOutputAndColorBinding> GetLedDcsBiosOutputs(SwitchPanelPZ55LEDPosition switchPanelPZ55LEDPosition)
        {
            var result = new List<DcsOutputAndColorBinding>();
            foreach (var colorOutputBinding in _listColorOutputBinding)
            {
                if ((SwitchPanelPZ55LEDPosition)colorOutputBinding.SaitekLEDPosition.GetPosition() == switchPanelPZ55LEDPosition)
                {
                    result.Add(colorOutputBinding);
                }
            }
            return result;
        }

        public void SetLedDcsBiosOutput(SwitchPanelPZ55LEDPosition switchPanelPZ55LEDPosition, List<DcsOutputAndColorBinding> dcsOutputAndColorBindingPZ55List)
        {
            /*
             * Replace all old entries found for this position with the new ones for this particular position
             * If list is empty then so be it
             */
            _listColorOutputBinding.RemoveAll(item => item.SaitekLEDPosition.Position.Equals(new SaitekPanelLEDPosition(switchPanelPZ55LEDPosition).Position));

            foreach (var dcsOutputAndColorBinding in dcsOutputAndColorBindingPZ55List)
            {
                _listColorOutputBinding.Add((DcsOutputAndColorBindingPZ55)dcsOutputAndColorBinding);
            }
            IsDirtyMethod();
        }

        private void OnReport(HidReport report)
        {
            //if (IsAttached == false) { return; }

            if (report.Data.Length == 3)
            {
                Array.Copy(_newSwitchPanelValue, _oldSwitchPanelValue, 3);
                Array.Copy(report.Data, _newSwitchPanelValue, 3);
                var hashSet = GetHashSetOfSwitchedKeys(_oldSwitchPanelValue, _newSwitchPanelValue);
                PZ55SwitchChanged(hashSet);
                OnSwitchesChanged(hashSet);
                _isFirstNotification = false;
                /*if (Common.Debug)
                {
                    var stringBuilder = new StringBuilder();
                    for (var i = 0; i < report.Data.Length; i++)
                    {
                        stringBuilder.Append(report.Data[i] + " ");
                    }
                    Common.DebugP(stringBuilder.ToString());
                    if (hashSet.Count > 0)
                    {
                        Common.DebugP("\nFollowing switches has been changed:\n");
                        foreach (var switchPanelKey in hashSet)
                        {
                            Common.DebugP(((SwitchPanelKey)switchPanelKey).SwitchPanelPZ55Key + ", value is " + FlagValue(_newSwitchPanelValue, ((SwitchPanelKey)switchPanelKey)));
                        }
                    }
                }
                Common.DebugP("\r\nDone!\r\n");*/
            }
            try
            {
                if (HIDSkeletonBase.HIDReadDevice != null && !Closed)
                {
                    Common.DebugP("Adding callback " + TypeOfSaitekPanel + " " + GuidString);
                    HIDSkeletonBase.HIDReadDevice.ReadReport(OnReport);
                }
            }
            catch (Exception ex)
            {
                Common.DebugP(ex.Message + "\n" + ex.StackTrace);
            }
        }

        private void DeviceAttachedHandler()
        {
            Startup();
            OnDeviceAttached();
        }

        private void DeviceRemovedHandler()
        {
            Shutdown();
            OnDeviceDetached();
        }

        public override DcsOutputAndColorBinding CreateDcsOutputAndColorBinding(SaitekPanelLEDPosition saitekPanelLEDPosition, PanelLEDColor panelLEDColor, DCSBIOSOutput dcsBiosOutput)
        {
            var dcsOutputAndColorBinding = new DcsOutputAndColorBindingPZ55();
            dcsOutputAndColorBinding.DCSBiosOutputLED = dcsBiosOutput;
            dcsOutputAndColorBinding.LEDColor = panelLEDColor;
            dcsOutputAndColorBinding.SaitekLEDPosition = saitekPanelLEDPosition;
            return dcsOutputAndColorBinding;
        }

        //SetLandingGearLED(cavb.PanelPZ55LEDPosition, cavb.PanelPZ55LEDColor);
        public void SetLandingGearLED(DcsOutputAndColorBindingPZ55 dcsOutputAndColorBindingPZ55)
        {
            SetLandingGearLED((SwitchPanelPZ55LEDPosition)dcsOutputAndColorBindingPZ55.SaitekLEDPosition.GetPosition(), dcsOutputAndColorBindingPZ55.LEDColor);
        }

        public void SetLandingGearLED(SwitchPanelPZ55LEDPosition switchPanelPZ55LEDPosition, PanelLEDColor switchPanelPZ55LEDColor)
        {
            try
            {
                switch (switchPanelPZ55LEDPosition)
                {
                    case SwitchPanelPZ55LEDPosition.UP:
                        {
                            _ledUpperColor = GetSwitchPanelPZ55LEDColor(switchPanelPZ55LEDPosition, switchPanelPZ55LEDColor);
                            break;
                        }
                    case SwitchPanelPZ55LEDPosition.LEFT:
                        {
                            _ledLeftColor = GetSwitchPanelPZ55LEDColor(switchPanelPZ55LEDPosition, switchPanelPZ55LEDColor);
                            break;
                        }
                    case SwitchPanelPZ55LEDPosition.RIGHT:
                        {
                            _ledRightColor = GetSwitchPanelPZ55LEDColor(switchPanelPZ55LEDPosition, switchPanelPZ55LEDColor);
                            break;
                        }
                }
                OnLedLightChanged(new SaitekPanelLEDPosition(switchPanelPZ55LEDPosition), switchPanelPZ55LEDColor);
                SetLandingGearLED(_ledUpperColor | _ledLeftColor | _ledRightColor);
            }
            catch (Exception e)
            {
                Common.DebugP("SetLandingGearLED() :\n" + e.Message + e.StackTrace);
                SetLastException(e);
            }
        }

        //Do not use directly !
        private void SetLandingGearLED(SwitchPanelPZ55LEDs switchPanelPZ55LEDs)
        {
            try
            {
                if (HIDSkeletonBase.HIDWriteDevice != null)
                {
                    var array = new[] { (byte)0x0, (byte)switchPanelPZ55LEDs };
                    Common.DebugP("HIDWriteDevice writing feature data " + TypeOfSaitekPanel + " " + GuidString);
                    HIDSkeletonBase.HIDWriteDevice.WriteFeatureData(new byte[] { 0, 0 });
                    HIDSkeletonBase.HIDWriteDevice.WriteFeatureData(array);
                }
                //if (IsAttached)
                //{
                Common.DebugP("Write ending");
                //}
            }
            catch (Exception e)
            {
                Common.DebugP("SetLandingGearLED(SwitchPanelPZ55LEDs switchPanelPZ55LEDs) :\n" + e.Message + e.StackTrace);
                SetLastException(e);
            }
        }

        private SwitchPanelPZ55LEDs GetSwitchPanelPZ55LEDColor(SwitchPanelPZ55LEDPosition switchPanelPZ55LEDPosition, PanelLEDColor panelLEDColor)
        {
            var result = SwitchPanelPZ55LEDs.ALL_DARK;

            switch (switchPanelPZ55LEDPosition)
            {
                case SwitchPanelPZ55LEDPosition.UP:
                    {
                        switch (panelLEDColor)
                        {
                            case PanelLEDColor.DARK:
                                {
                                    result = SwitchPanelPZ55LEDs.ALL_DARK;
                                    break;
                                }
                            case PanelLEDColor.GREEN:
                                {
                                    result = SwitchPanelPZ55LEDs.UP_GREEN;
                                    break;
                                }
                            case PanelLEDColor.RED:
                                {
                                    result = SwitchPanelPZ55LEDs.UP_RED;
                                    break;
                                }
                            case PanelLEDColor.YELLOW:
                                {
                                    result = SwitchPanelPZ55LEDs.UP_YELLOW;
                                    break;
                                }
                        }
                        break;
                    }
                case SwitchPanelPZ55LEDPosition.LEFT:
                    {
                        switch (panelLEDColor)
                        {
                            case PanelLEDColor.DARK:
                                {
                                    result = SwitchPanelPZ55LEDs.ALL_DARK;
                                    break;
                                }
                            case PanelLEDColor.GREEN:
                                {
                                    result = SwitchPanelPZ55LEDs.LEFT_GREEN;
                                    break;
                                }
                            case PanelLEDColor.RED:
                                {
                                    result = SwitchPanelPZ55LEDs.LEFT_RED;
                                    break;
                                }
                            case PanelLEDColor.YELLOW:
                                {
                                    result = SwitchPanelPZ55LEDs.LEFT_YELLOW;
                                    break;
                                }
                        }
                        break;
                    }
                case SwitchPanelPZ55LEDPosition.RIGHT:
                    {
                        switch (panelLEDColor)
                        {
                            case PanelLEDColor.DARK:
                                {
                                    result = SwitchPanelPZ55LEDs.ALL_DARK;
                                    break;
                                }
                            case PanelLEDColor.GREEN:
                                {
                                    result = SwitchPanelPZ55LEDs.RIGHT_GREEN;
                                    break;
                                }
                            case PanelLEDColor.RED:
                                {
                                    result = SwitchPanelPZ55LEDs.RIGHT_RED;
                                    break;
                                }
                            case PanelLEDColor.YELLOW:
                                {
                                    result = SwitchPanelPZ55LEDs.RIGHT_YELLOW;
                                    break;
                                }
                        }
                        break;
                    }
            }
            return result;
        }

        private HashSet<object> GetHashSetOfSwitchedKeys(byte[] oldValue, byte[] newValue)
        {
            var result = new HashSet<object>();




            for (var i = 0; i < 3; i++)
            {
                var oldByte = oldValue[i];
                var newByte = newValue[i];

                foreach (var switchPanelKey in _switchPanelKeys)
                {
                    if (switchPanelKey.Group == i && (FlagHasChanged(oldByte, newByte, switchPanelKey.Mask) || _isFirstNotification))
                    {
                        if (switchPanelKey.SwitchPanelPZ55Key == SwitchPanelPZ55Keys.SWITCHKEY_CLOSE_COWL)
                        {
                            //This button is special. The Panel reports the button ON when it us switched upwards towards [CLOSE]. This is confusing semantics.
                            //The button is considered OFF by the program when it is upwards which is opposite to the other buttons which all are considered ON when upwards.
                            switchPanelKey.IsOn = !FlagValue(newValue, switchPanelKey);
                        }
                        else
                        {
                            switchPanelKey.IsOn = FlagValue(newValue, switchPanelKey);
                        }
                        result.Add(switchPanelKey);
                    }
                }
            }
            return result;
        }

        private static bool FlagValue(byte[] currentValue, SwitchPanelKey switchPanelKey)
        {
            return (currentValue[switchPanelKey.Group] & switchPanelKey.Mask) > 0;
        }

        private void CreateSwitchKeys()
        {
            _switchPanelKeys = SwitchPanelKey.GetPanelSwitchKeys();
        }

        public HashSet<DCSBIOSBindingPZ55> DCSBiosBindings
        {
            get { return _dcsBiosBindings; }
            set { _dcsBiosBindings = value; }
        }

        public bool ManualLandingGearLeds
        {
            get { return _manualLandingGearLeds; }
            set
            {
                _manualLandingGearLeds = value;
                IsDirtyMethod();
            }
        }

        public override String SettingsVersion()
        {
            return "0X";
        }


    }


}
