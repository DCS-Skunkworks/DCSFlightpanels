namespace NonVisuals.Saitek.Panels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;

    using ClassLibraryCommon;

    using DCS_BIOS;

    using MEF;

    using NonVisuals.EventArgs;

    public class BacklitPanelBIP : SaitekPanel
    {
        private const byte _1BIPMask = 0x01;

        private const byte _2BIPMask = 0x02;

        private const byte _3BIPMask = 0x04;

        private const byte _4BIPMask = 0x08;

        private const byte _5BIPMask = 0x10;

        private const byte _6BIPMask = 0x20;

        private const byte _7BIPMask = 0x40;

        private const byte _8BIPMask = 0x80;

        private readonly List<DcsOutputAndColorBindingBIP> _listColorOutputBinding = new List<DcsOutputAndColorBindingBIP>();

        private readonly byte[] _upperRowBytes = { 0x0, 0x0 }; // byte 1 & 4

        private readonly byte[] _middleRowBytes = { 0x0, 0x0 }; // byte 2 & 5

        private readonly byte[] _lowerRowBytes = { 0x0, 0x0 }; // byte 3 & 6

        private uint _ledBrightness = 50; // 0 - 100 in 5 step intervals

        private DCSBIOSBrightnessBinding _dcsBiosBrightnessBinding;

        private DCSBIOSOutput _dcsbiosBrightnessControl;

        /*
         * 01000000 2nd BIP from left GREEN
         * 00000000
         * 
         * 01000000 2nd BIP from left YELLOW
         * 01000000
         * 
         * 00000000 2nd BIP from left RED
         * 01000000
         * 
         * 00000000 2nd BIP from left DARK
         * 00000000
         *
         */
        public BacklitPanelBIP(uint ledBrightness, HIDSkeleton hidSkeleton)
            : base(GamingPanelEnum.BackLitPanel, hidSkeleton)
        {
            if (hidSkeleton.PanelInfo.GamingPanelType != GamingPanelEnum.BackLitPanel)
            {
                throw new ArgumentException();
            }

            VendorId = 0x6A3;
            ProductId = 0xB4E;
            _ledBrightness = ledBrightness;
            BipFactory.RegisterBip(this);
            Startup();
        }

        // sätta färg efter om Config finns
        public sealed override void Startup()
        {
            try
            {
                SetLedStrength();
            }
            catch (Exception ex)
            {
                SetLastException(ex);
            }
        }

        public override void Dispose()
        {
            try
            {
                BipFactory.DeRegisterBip(this);
                Closed = true;
            }
            catch (Exception e)
            {
                SetLastException(e);
            }
        }

        public override void ImportSettings(GenericPanelBinding genericPanelBinding)
        {
            ClearSettings();

            BindingHash = genericPanelBinding.BindingHash;

            var settings = genericPanelBinding.Settings;
            foreach (var setting in settings)
            {
                if (!setting.StartsWith("#") && setting.Length > 2)
                {
                    if (setting.StartsWith(DCSBIOSBrightnessBinding.Keyword))
                    {
                        _dcsBiosBrightnessBinding = new DCSBIOSBrightnessBinding();
                        _dcsBiosBrightnessBinding.ImportSettings(setting);
                        HandleBrightnessBinding();
                    }
                    else
                    {
                        var colorOutput = new DcsOutputAndColorBindingBIP();
                        colorOutput.ImportSettings(setting);
                        _listColorOutputBinding.Add(colorOutput);
                    }
                }
            }

            SettingsApplied();
        }

        public override List<string> ExportSettings()
        {
            if (Closed)
            {
                return null;
            }

            var result = new List<string>();
            if (_listColorOutputBinding.Any())
            {
                foreach (var colorOutputBinding in _listColorOutputBinding)
                {
                    result.Add(colorOutputBinding.ExportSettings());
                }
            }

            if (_dcsBiosBrightnessBinding != null)
            {
                result.Add(_dcsBiosBrightnessBinding.ExportSettings());
            }

            return result;
        }

        public List<DcsOutputAndColorBinding> GetLedDcsBiosOutputs(BIPLedPositionEnum bipLedPositionEnum)
        {
            var result = new List<DcsOutputAndColorBinding>();
            foreach (var colorOutputBinding in _listColorOutputBinding)
            {
                if ((BIPLedPositionEnum)colorOutputBinding.SaitekLEDPosition.GetPosition() == bipLedPositionEnum)
                {
                    result.Add(colorOutputBinding);
                }
            }

            return result;
        }

        public void SetLedDcsBiosOutput(BIPLedPositionEnum bipLedPositionEnum, List<DcsOutputAndColorBinding> dcsOutputAndColorBindingBIPList)
        {
            /*
             * Replace all old entries found for this position with the new ones for this particular position
             * If list is empty then so be it
             */
            _listColorOutputBinding.RemoveAll(item => item.SaitekLEDPosition.Position.Equals(new SaitekPanelLEDPosition(bipLedPositionEnum).Position));

            foreach (var dcsOutputAndColorBinding in dcsOutputAndColorBindingBIPList)
            {
                _listColorOutputBinding.Add((DcsOutputAndColorBindingBIP)dcsOutputAndColorBinding);
            }

            SetIsDirty();
        }

        internal void CheckDcsDataForColorChangeHook(uint address, uint data)
        {
            foreach (var cavb in _listColorOutputBinding)
            {
                if (address == cavb.DCSBiosOutputLED.Address)
                {
                    if (cavb.DCSBiosOutputLED.CheckForValueMatchAndChange(data))
                    {
                        SetLED((BIPLedPositionEnum)cavb.SaitekLEDPosition.GetPosition(), cavb.LEDColor);
                    }
                }
            }
        }

        protected override void GamingPanelKnobChanged(bool isFirstReport, IEnumerable<object> hashSet)
        {
            // Nothing needs to be done here
        }

        public PanelLEDColor GetColor(BIPLedPositionEnum bipLedPositionEnum)
        {
            try
            {
                var row = GetRow(bipLedPositionEnum);
                var index = GetIndex(bipLedPositionEnum);
                byte[] array = { 0x0 };
                byte mask = 0;

                switch (row)
                {
                    case 1:
                        {
                            array = _upperRowBytes;
                            break;
                        }

                    case 2:
                        {
                            array = _middleRowBytes;
                            break;
                        }

                    case 3:
                        {
                            array = _lowerRowBytes;
                            break;
                        }
                }

                switch (index)
                {
                    case 1:
                        {
                            mask = _1BIPMask;
                            break;
                        }

                    case 2:
                        {
                            mask = _2BIPMask;
                            break;
                        }

                    case 3:
                        {
                            mask = _3BIPMask;
                            break;
                        }

                    case 4:
                        {
                            mask = _4BIPMask;
                            break;
                        }

                    case 5:
                        {
                            mask = _5BIPMask;
                            break;
                        }

                    case 6:
                        {
                            mask = _6BIPMask;
                            break;
                        }

                    case 7:
                        {
                            mask = _7BIPMask;
                            break;
                        }

                    case 8:
                        {
                            mask = _8BIPMask;
                            break;
                        }
                }

                // [0] & [1] == 0  --> DARK
                // [0] == 1, [1] == 0 --> GREEN
                // [0] == 1, [1] == 1 --> YELLOW
                // [0] == 0, [1] == 1 --> RED
                if ((array[0] & mask) > 1)
                {
                    if ((array[1] & mask) < 1)
                    {
                        return PanelLEDColor.GREEN;
                    }

                    if ((array[1] & mask) > 1)
                    {
                        return PanelLEDColor.YELLOW;
                    }
                }

                if ((array[0] & mask) < 1)
                {
                    if ((array[1] & mask) < 1)
                    {
                        return PanelLEDColor.DARK;
                    }

                    if ((array[1] & mask) > 1)
                    {
                        return PanelLEDColor.RED;
                    }
                }
            }
            catch (Exception ex)
            {
                Common.LogError(ex);
            }

            return PanelLEDColor.RED;
        }

        public bool HasConfiguration(BIPLedPositionEnum position)
        {
            foreach (var dcsOutputAndColorBindingBIP in _listColorOutputBinding)
            {
                if (position.Equals((BIPLedPositionEnum)dcsOutputAndColorBindingBIP.SaitekLEDPosition.Position))
                {
                    return true;
                }
            }

            return false;
        }

        public override DcsOutputAndColorBinding CreateDcsOutputAndColorBinding(SaitekPanelLEDPosition saitekPanelLEDPosition, PanelLEDColor panelLEDColor, DCSBIOSOutput dcsBiosOutput)
        {
            var dcsOutputAndColorBinding = new DcsOutputAndColorBindingBIP { DCSBiosOutputLED = dcsBiosOutput, LEDColor = panelLEDColor, SaitekLEDPosition = saitekPanelLEDPosition };
            return dcsOutputAndColorBinding;
        }

        public override void SavePanelSettings(object sender, ProfileHandlerEventArgs e)
        {
            e.ProfileHandlerEA.RegisterPanelBinding(this, ExportSettings());
        }

        public override void SavePanelSettingsJSON(object sender, ProfileHandlerEventArgs e)
        {
        }

        public override void DcsBiosDataReceived(object sender, DCSBIOSDataEventArgs e)
        {
            UpdateCounter(e.Address, e.Data);
            CheckDcsDataForColorChangeHook(e.Address, e.Data);

            if (_dcsbiosBrightnessControl != null && e.Address == _dcsbiosBrightnessControl.Address)
            {
                _ledBrightness = (uint)(((double)100 / 0xFFFF) * e.Data);
                SetLedStrength();
            }
        }

        public override void ClearSettings(bool setIsDirty = false)
        {
            _listColorOutputBinding.Clear();

            if (setIsDirty)
            {
                SetIsDirty();
            }
        }

        public string GetPosString(BIPLedPositionEnum bipLedPositionEnum)
        {
            // Position_1_1
            return bipLedPositionEnum.ToString().Replace("Position_", string.Empty);

            // 1_1
        }

        public int GetIndex(BIPLedPositionEnum bipLedPositionEnum)
        {
            // Position_1_1
            return int.Parse(bipLedPositionEnum.ToString().Remove(0, 9).Substring(2, 1));
        }

        public int GetRow(BIPLedPositionEnum bipLedPositionEnum)
        {
            // Position_1_1
            return int.Parse(bipLedPositionEnum.ToString().Remove(0, 9).Substring(0, 1));
        }

        public override void Identify()
        {
            try
            {
                var thread = new Thread(ShowIdentifyingValue);
                thread.Start();
            }
            catch (Exception)
            {
                // ignored
            }
        }

        private void ShowIdentifyingValue()
        {
            try
            {
                var spins = 40;
                var random = new Random();
                var arrayBIPLedPositionEnum = Enum.GetValues(typeof(BIPLedPositionEnum));
                var arrayPanelLEDColor = Enum.GetValues(typeof(PanelLEDColor));

                while (spins > 0)
                {
                    var randomBIPLedPositionEnum = (BIPLedPositionEnum)arrayBIPLedPositionEnum.GetValue(random.Next(arrayBIPLedPositionEnum.Length));
                    var randomPanelLEDColor = (PanelLEDColor)arrayPanelLEDColor.GetValue(random.Next(arrayPanelLEDColor.Length));
                    SetLED(randomBIPLedPositionEnum, randomPanelLEDColor);

                    Thread.Sleep(50);
                    spins--;
                }

                TurnOffAllLEDs();
            }
            catch (Exception)
            {
                // ignored
            }
        }

        public void TurnOffAllLEDs()
        {
            var positions = Enum.GetValues(typeof(BIPLedPositionEnum)).Cast<BIPLedPositionEnum>();
            foreach (var position in positions)
            {
                SetLED(position, PanelLEDColor.DARK);
            }
        }

        public void SetLED(BIPLedPositionEnum bipLedPositionEnum, PanelLEDColor panelLEDColor)
        {
            try
            {
                var row = GetRow(bipLedPositionEnum);
                var index = GetIndex(bipLedPositionEnum);
                byte[] array = { 0x0 };
                byte mask = 0;

                switch (row)
                {
                    case 1:
                        {
                            array = _upperRowBytes;
                            break;
                        }

                    case 2:
                        {
                            array = _middleRowBytes;
                            break;
                        }

                    case 3:
                        {
                            array = _lowerRowBytes;
                            break;
                        }
                }

                switch (index)
                {
                    case 1:
                        {
                            mask = _1BIPMask;
                            break;
                        }

                    case 2:
                        {
                            mask = _2BIPMask;
                            break;
                        }

                    case 3:
                        {
                            mask = _3BIPMask;
                            break;
                        }

                    case 4:
                        {
                            mask = _4BIPMask;
                            break;
                        }

                    case 5:
                        {
                            mask = _5BIPMask;
                            break;
                        }

                    case 6:
                        {
                            mask = _6BIPMask;
                            break;
                        }

                    case 7:
                        {
                            mask = _7BIPMask;
                            break;
                        }

                    case 8:
                        {
                            mask = _8BIPMask;
                            break;
                        }
                }

                switch (panelLEDColor)
                {
                    case PanelLEDColor.DARK:
                        {
                            // Set all to 0
                            array[0] = (array[0] &= (byte)~mask);
                            array[1] = (array[1] &= (byte)~mask);
                            break;
                        }

                    case PanelLEDColor.GREEN:
                        {
                            // Set first byte's bit to 1
                            array[0] = array[0] |= mask;
                            array[1] = (array[1] &= (byte)~mask);
                            break;
                        }

                    case PanelLEDColor.YELLOW:
                        {
                            // Set both byte's bit to 1
                            array[0] = array[0] |= mask;
                            array[1] = array[1] |= mask;
                            break;
                        }

                    case PanelLEDColor.RED:
                        {
                            // Set second byte's bit to 1
                            array[0] = (array[0] &= (byte)~mask);
                            array[1] = array[1] |= mask;
                            break;
                        }
                }

                /*
                 * 01000000 2nd BIP from left GREEN
                 * 00000000
                 * 
                 * 01000000 2nd BIP from left YELLOW
                 * 01000000
                 * 
                 * 00000000 2nd BIP from left RED
                 * 01000000
                 * 
                 * 00000000 2nd BIP from left DARK
                 * 00000000
                 *
                 */

                // _upperRowBytes = { (byte)0x0, (byte)0x0 }; //byte 1 & 4
                // _middleRowBytes = { (byte)0x0, (byte)0x0 };//byte 2 & 5
                // _lowerRowBytes = { (byte)0x0, (byte)0x0 }; //byte 3 & 6
                var finalArray = new byte[7];
                finalArray[0] = 0xb8;
                finalArray[1] = _upperRowBytes[0];
                finalArray[2] = _middleRowBytes[0];
                finalArray[3] = _lowerRowBytes[0];
                finalArray[4] = _upperRowBytes[1];
                finalArray[5] = _middleRowBytes[1];
                finalArray[6] = _lowerRowBytes[1];

                SendLEDData(finalArray);
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        public void SendLEDData(byte[] array)
        {
            try
            {
                HIDSkeletonBase.HIDWriteDevice?.WriteFeatureData(array);
            }
            catch (Exception e)
            {
                SetLastException(e);
            }
        }

        private void DeviceAttachedHandler()
        {
            Startup();

            // IsAttached = true;
            DeviceAttached();
        }

        private void DeviceRemovedHandler()
        {
            Dispose();

            // IsAttached = false;
            DeviceDetached();
        }

        private void SetLedStrength()
        {
            try
            {
                var array = new byte[2];
                array[0] = 0xb2;
                array[1] = (byte)_ledBrightness;
                SendLEDData(array);
            }
            catch (Exception ex)
            {
                SetLastException(ex);
            }
        }

        public uint LEDBrightness
        {
            get => _ledBrightness;
            set
            {
                _ledBrightness = value;
                SetLedStrength();
            }
        }

        public void LEDBrightnessDecrease()
        {
            if (_ledBrightness == 20)
            {
                _ledBrightness = 100;
            }

            _ledBrightness -= 10;
            SetLedStrength();
        }

        public void LEDBrightnessIncrease()
        {
            if (_ledBrightness == 100)
            {
                _ledBrightness = 20;
            }

            _ledBrightness += 10;
            SetLedStrength();
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

        public void SetBrightnessBinding(DCSBIOSOutput dcsbiosOutput)
        {
            _dcsBiosBrightnessBinding = new DCSBIOSBrightnessBinding(dcsbiosOutput);
            HandleBrightnessBinding();
            SetIsDirty();
        }

        public DCSBIOSBrightnessBinding BrightnessBinding
        {
            get => _dcsBiosBrightnessBinding;
            set
            {
                _dcsBiosBrightnessBinding = value;
                HandleBrightnessBinding();
                SetIsDirty();
            }
        }

        private void HandleBrightnessBinding()
        {
            if (_dcsBiosBrightnessBinding == null)
            {
                _dcsbiosBrightnessControl = null;
                return;
            }

            _dcsbiosBrightnessControl = DCSBIOSControlLocator.GetDCSBIOSOutput(_dcsBiosBrightnessBinding.ControlId);
        }
    }

    [Serializable]
    public enum BIPLedPositionEnum
    {
        Position_1_1,
        Position_1_2,
        Position_1_3,
        Position_1_4,
        Position_1_5,
        Position_1_6,
        Position_1_7,
        Position_1_8,
        Position_2_1,
        Position_2_2,
        Position_2_3,
        Position_2_4,
        Position_2_5,
        Position_2_6,
        Position_2_7,
        Position_2_8,
        Position_3_1,
        Position_3_2,
        Position_3_3,
        Position_3_4,
        Position_3_5,
        Position_3_6,
        Position_3_7,
        Position_3_8
    }

    /*
        Byte #0
        Header = 0xb8
        Payload 6 bytes (48 bits)

        Byte #1 (Upper row)
        00000000
        ||||||||_ Leftmost GREEN, YELLOW when Byte #4 same bit is &
        |||||||_ 
        ||||||_ 
        |||||_ 
        ||||_ 
        |||_ 
        ||_ 
        |_ Rightmost GREEN, YELLOW when Byte #4 same bit is &

        Byte #2 (Middle row)
        00000000
        ||||||||_ Leftmost GREEN, YELLOW when Byte #5 same bit is &
        |||||||_ 
        ||||||_ 
        |||||_ 
        ||||_ 
        |||_ 
        ||_ 
        |_ Rightmost GREEN, YELLOW when Byte #5 same bit is &

        Byte #3 (Lower row)
        00000000
        ||||||||_ Leftmost GREEN, YELLOW when Byte #6 same bit is &
        |||||||_ 
        ||||||_ 
        |||||_ 
        ||||_ 
        |||_ 
        ||_ 
        |_ Rightmost GREEN, YELLOW when Byte #6 same bit is &

        Byte #4 (Upper row)
        00000000
        ||||||||_ Leftmost RED when not same bit in Byte #1 set
        |||||||_ 
        ||||||_ 
        |||||_ 
        ||||_ 
        |||_ 
        ||_ 
        |_ Rightmost RED when not same bit in Byte #1 set

        Byte #5 (Middle row)
        00000000
        ||||||||_ Leftmost RED when not same bit in Byte #2 set
        |||||||_ 
        ||||||_ 
        |||||_ 
        ||||_ 
        |||_ 
        ||_ 
        |_  Rightmost RED when not same bit in Byte #2 set

        Byte #6 (Lower row)
        00000000
        ||||||||_ Leftmost RED when not same bit in Byte #3 set
        |||||||_ 
        ||||||_ 
        |||||_ 
        ||||_ 
        |||_ 
        ||_ 
        |_  Rightmost RED when not same bit in Byte #3 set
 */
}
