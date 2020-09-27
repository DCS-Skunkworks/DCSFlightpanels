using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using ClassLibraryCommon;
using NonVisuals.Saitek;
using NonVisuals.Saitek.Panels;

namespace NonVisuals.Radios
{
    public enum PZ69LCDPosition
    {
        UPPER_ACTIVE_LEFT = 1,
        UPPER_STBY_RIGHT = 6,
        LOWER_ACTIVE_LEFT = 11,
        LOWER_STBY_RIGHT = 16
    }

    public abstract class RadioPanelPZ69Base : SaitekPanel
    {
        private byte _ignoreSwitchButtonCounter = 3;
        protected NumberFormatInfo NumberFormatInfoFullDisplay;
        protected NumberFormatInfo NumberFormatInfoEmpty;
        private int _frequencyKnobSensitivity;
        private volatile byte _frequencySensitivitySkipper;
        protected readonly object LockLCDUpdateObject = new object();
        protected bool DataHasBeenReceivedFromDCSBIOS;
        private Guid _guid = Guid.NewGuid();
        /*
         * IMPORTANT WHEN SYNCHING DIALS
         */
        //MSDN (DateTime.Now.Ticks : There are 10,000 ticks in a millisecond
        private int _synchSleepTime = 300;
        private long _resetSyncTimeout = 35000000;
        private long _syncOKDelayTimeout = 50000000; //5s


        protected RadioPanelPZ69Base(HIDSkeleton hidSkeleton) : base(GamingPanelEnum.PZ69RadioPanel, hidSkeleton)
        {
            if (hidSkeleton.PanelInfo.GamingPanelType != GamingPanelEnum.PZ69RadioPanel)
            {
                throw new ArgumentException();
            }
            VendorId = 0x6A3;
            ProductId = 0xD05;
            NumberFormatInfoFullDisplay = new NumberFormatInfo();
            NumberFormatInfoFullDisplay.NumberDecimalSeparator = ".";
            NumberFormatInfoFullDisplay.NumberDecimalDigits = 4;
            NumberFormatInfoFullDisplay.NumberGroupSeparator = "";

            NumberFormatInfoEmpty = new NumberFormatInfo();
            NumberFormatInfoEmpty.NumberDecimalSeparator = ".";
            NumberFormatInfoEmpty.NumberGroupSeparator = "";
        }


        /*         
            1 byte (header byte 0x0) [0]
            5 bytes upper left LCD   [1 - 5]
            5 bytes upper right LCD  [6 - 10]
            5 bytes lower left LCD   [11- 15]
            5 bytes lower right LCD  [16- 20]

            0x01 - 0x09 displays the figure 1-9
            0xD1 - 0xD9 displays the figure 1.-9. (figure followed by dot)
            0xFF -> blank, nothing is shown in that spot.
             
            var bytes = new byte[21];
            bytes[0] = 0x0;
            bytes = SetPZ69FrequencyBytes(bytes, _lcdFrequencyActiveUpper, 1);
            bytes = SetPZ69FrequencyBytes(bytes, _lcdFrequencyStandbyUpper, 6);
            bytes = SetPZ69FrequencyBytes(bytes, _lcdFrequencyActiveLower, 11);
            bytes = SetPZ69FrequencyBytes(bytes, _lcdFrequencyStandbyLower, 16);
        */
        protected void SetPZ69DisplayBytesInteger(ref byte[] bytes, int digits, PZ69LCDPosition pz69LCDPosition)
        {
            var arrayPosition = GetArrayPosition(pz69LCDPosition);
            var maxArrayPosition = GetArrayPosition(pz69LCDPosition) + 5;
            var i = 0;
            var digitsAsString = digits.ToString().PadLeft(5);

            //D = DARK
            //116 should become DD116!

            do
            {
                //5 digits can be displayed
                //12345 -> 12345
                //116   -> DD116 
                //1     -> DDDD1

                byte b;
                b = digitsAsString[i].ToString().Equals(" ") ? (byte)0xFF : byte.Parse(digitsAsString[i].ToString());
                bytes[arrayPosition] = b;

                arrayPosition++;
                i++;
            } while (i < digitsAsString.Length && arrayPosition < maxArrayPosition + 1);
        }

        protected void SetPZ69DisplayBlank(ref byte[] bytes, PZ69LCDPosition pz69LCDPosition)
        {

            var arrayPosition = GetArrayPosition(pz69LCDPosition);
            var i = 0;
            do
            {
                bytes[arrayPosition] = 0xFF;
                arrayPosition++;
                i++;
            } while (i < 5);
        }

        public override void SavePanelSettingsJSON(object sender, ProfileHandlerEventArgs e) { }

        protected void SetPZ69DisplayBytesUnsignedInteger(ref byte[] bytes, uint digits, PZ69LCDPosition pz69LCDPosition)
        {

            var arrayPosition = GetArrayPosition(pz69LCDPosition);
            var maxArrayPosition = GetArrayPosition(pz69LCDPosition) + 4;
            var i = 0;
            var digitsAsString = digits.ToString().PadLeft(5);


            //Debug.WriteLine("LCD position is " + pz69LCDPosition);
            //Debug.WriteLine("Array position = " + arrayPosition);
            //Debug.WriteLine("Max array position = " + (maxArrayPosition));
            //Debug.WriteLine("digitsAsString = >" + digitsAsString + "< length=" + digitsAsString.Length);
            //D = DARK
            //116 should become DD116!

            do
            {
                //5 digits can be displayed
                //12345 -> 12345
                //116   -> DD116 
                //1     -> DDDD1

                byte b;
                b = digitsAsString[i].ToString().Equals(" ") ? (byte)0xFF : byte.Parse(digitsAsString[i].ToString());
                bytes[arrayPosition] = b;

                arrayPosition++;
                i++;
            } while (i < digitsAsString.Length && arrayPosition < maxArrayPosition + 1);
        }

        protected void SetPZ69DisplayBytes(ref byte[] bytes, double digits, int decimals, PZ69LCDPosition pz69LCDPosition)
        {

            var arrayPosition = GetArrayPosition(pz69LCDPosition);
            var maxArrayPosition = GetArrayPosition(pz69LCDPosition) + 4;
            var i = 0;
            var formatString = "0.".PadRight(decimals + 2, '0');
            var digitsAsString = digits.ToString(formatString, NumberFormatInfoEmpty).PadLeft(6);

            do
            {
                if (digitsAsString[i] == '.')
                {
                    //skip to next position, this has already been dealt with
                    i++;
                }
                byte b;
                b = digitsAsString[i].ToString().Equals(" ") ? (byte)0xFF : byte.Parse(digitsAsString[i].ToString());
                bytes[arrayPosition] = b;
                if (digitsAsString.Length > i + 1 && digitsAsString[i + 1] == '.')
                {
                    bytes[arrayPosition] = (byte)(bytes[arrayPosition] + 0xd0);
                }
                arrayPosition++;
                i++;
            } while (i < digitsAsString.Length && arrayPosition < maxArrayPosition + 1);
        }

        protected void SetPZ69DisplayBytesCustom1(ref byte[] bytes, byte[] bytesToBeInjected, PZ69LCDPosition pz69LCDPosition)
        {
            var arrayPosition = GetArrayPosition(pz69LCDPosition);
            var i = 0;

            do
            {
                //5 digits can be displayed
                //12345 -> 12345
                //116   -> DD116 
                //1     -> DDDD1

                bytes[arrayPosition] = bytesToBeInjected[i];

                arrayPosition++;
                i++;
            } while (i < bytesToBeInjected.Length && i < 5);
        }

        public override void Identify()
        {
            try
            {
                var thread = new Thread(ShowIdentifyingValue);
                thread.Start();
            }
            catch (Exception e)
            {
            }
        }

        private void ShowIdentifyingValue()
        {
            try
            {
                var bytes = new byte[21];
                bytes[0] = 0x0;
                PZ69LCDPosition pz69LCDPosition = PZ69LCDPosition.UPPER_ACTIVE_LEFT;
                var random = new Random();
                var lcdPositionArray = Enum.GetValues(typeof(PZ69LCDPosition));
                var lcdValueArray = new string[] { "00000", "11111", "22222", "33333", "44444", "55555", "66666", "77777", "88888", "99999" };
                var spins = 8;

                while (spins > 0)
                {
                    var position = (PZ69LCDPosition)lcdPositionArray.GetValue(random.Next(lcdPositionArray.Length));
                    var value = (string)lcdValueArray.GetValue(random.Next(lcdValueArray.Length));

                    SetPZ69DisplayBytesString(ref bytes, value, position);
                    SendLCDData(bytes);

                    Thread.Sleep(500);
                    spins--;
                }

                TurnOffAllDisplays();
            }
            catch (Exception e)
            {
            }
        }

        protected void TurnOffAllDisplays()
        {
            try
            {
                var bytes = new byte[21];
                SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.LOWER_ACTIVE_LEFT);
                SendLCDData(bytes);
                SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.UPPER_ACTIVE_LEFT);
                SendLCDData(bytes);
                SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.LOWER_STBY_RIGHT);
                SendLCDData(bytes);
                SetPZ69DisplayBlank(ref bytes, PZ69LCDPosition.UPPER_STBY_RIGHT);
                SendLCDData(bytes);
            }
            catch (Exception e)
            {
            }
        }

        protected void SetPZ69DisplayBytesString(ref byte[] bytes, string digitString, PZ69LCDPosition pz69LCDPosition)
        {

            var arrayPosition = GetArrayPosition(pz69LCDPosition);
            var maxArrayPosition = GetArrayPosition(pz69LCDPosition) + 4;
            var i = 0;
            var digits = "";
            if (digitString.Length > 5)
            {
                if (digitString.Contains("."))
                {
                    digits = digitString.Substring(0, 6);
                }
                else
                {
                    digits = digitString.Substring(0, 5);
                }
            }
            else if (digitString.Length < 5)
            {
                if (digitString.Contains("."))
                {
                    digits = digitString.PadLeft(6, ' ');
                }
                else
                {
                    digits = digitString.PadLeft(5, ' ');
                }
            }
            else if (digitString.Length == 5)
            {
                if (digitString.Contains("."))
                {
                    digits = digitString.PadLeft(1, ' ');
                }
                else
                {
                    digits = digitString;
                }
            }

            do
            {
                if (digits[i] == '.')
                {
                    //skip to next position, this has already been dealt with
                    i++;
                }

                try
                {
                    if (digits[i] == ' ')
                    {
                        bytes[arrayPosition] = 0xff;
                    }
                    else
                    {
                        var b = byte.Parse(digits[i].ToString());
                        bytes[arrayPosition] = b;
                    }
                }
                catch (Exception e)
                {
                    Common.LogError(e, "SetPZ69DisplayBytesDefault()");
                }
                if (digits.Length > i + 1 && digits[i + 1] == '.')
                {
                    //Add decimal marker
                    bytes[arrayPosition] = (byte)(bytes[arrayPosition] + 0xd0);
                }

                arrayPosition++;
                i++;
            } while (i < digits.Length && arrayPosition < maxArrayPosition + 1);
        }

        protected void SetPZ69DisplayBytesDefault(ref byte[] bytes, string digits, PZ69LCDPosition pz69LCDPosition)
        {

            var arrayPosition = GetArrayPosition(pz69LCDPosition);
            var maxArrayPosition = GetArrayPosition(pz69LCDPosition) + 4;
            var i = 0;
            do
            {
                if (digits[i] == '.')
                {
                    //skip to next position, this has already been dealt with
                    i++;
                }

                try
                {
                    if (digits[i] == ' ')
                    {
                        bytes[arrayPosition] = 0xff;
                    }
                    else
                    {
                        var b = byte.Parse(digits[i].ToString());
                        bytes[arrayPosition] = b;
                    }
                }
                catch (Exception e)
                {
                    Common.LogError(e, "SetPZ69DisplayBytesDefault()");
                }
                if (digits.Length > i + 1 && digits[i + 1] == '.')
                {
                    //Add decimal marker
                    bytes[arrayPosition] = (byte)(bytes[arrayPosition] + 0xd0);
                }

                arrayPosition++;
                i++;
            } while (i < digits.Length && arrayPosition < maxArrayPosition + 1);
        }

        protected void SetPZ69DisplayBytesDefault(ref byte[] bytes, double digits, PZ69LCDPosition pz69LCDPosition)
        {


            var arrayPosition = GetArrayPosition(pz69LCDPosition);
            var maxArrayPosition = GetArrayPosition(pz69LCDPosition) + 4;

            //Debug.WriteLine("LCD position is " + pz69LCDPosition);
            //Debug.WriteLine("Array position = " + arrayPosition);
            //Debug.WriteLine("Max array position = " + (maxArrayPosition));
            var i = 0;
            var digitsAsString = digits.ToString("0.0000", NumberFormatInfoFullDisplay);
            //116 should become 116.00!

            do
            {
                //5 digits can be displayed
                //1.00000011241 -> 1.0000
                //116.0434      -> 116.04 
                //1199330.12449 -> 11993
                if (digitsAsString[i] == '.')
                {
                    //skip to next position, this has already been dealt with
                    i++;
                }

                try
                {
                    var tmp = digitsAsString[i].ToString();
                    var b = byte.Parse(tmp);
                    bytes[arrayPosition] = b;
                    //Debug.WriteLine("Current string char is " + tmp + " from i = " + i + ", writing byte " + b + " to array position " + arrayPosition);
                }
                catch (Exception e)
                {
                    Common.LogError(e, "SetPZ69DisplayBytesDefault() digitsAsString.Length = " + digitsAsString.Length);
                }

                if (digitsAsString.Length > i + 1 && digitsAsString[i + 1] == '.')
                {
                    //Add decimal marker
                    bytes[arrayPosition] = (byte)(bytes[arrayPosition] + 0xd0);
                    //Debug.WriteLine("Writing decimal marker to array position " + arrayPosition);
                }

                arrayPosition++;
                i++;
            } while (i < digitsAsString.Length && arrayPosition < maxArrayPosition + 1);
        }

        private int GetArrayPosition(PZ69LCDPosition pz69LCDPosition)
        {
            switch (pz69LCDPosition)
            {
                case PZ69LCDPosition.UPPER_ACTIVE_LEFT:
                    {
                        return 1;
                    }
                case PZ69LCDPosition.UPPER_STBY_RIGHT:
                    {
                        return 6;
                    }
                case PZ69LCDPosition.LOWER_ACTIVE_LEFT:
                    {
                        return 11;
                    }
                case PZ69LCDPosition.LOWER_STBY_RIGHT:
                    {
                        return 16;
                    }
            }
            return 1;
        }

        public void SendLCDData(byte[] array)
        {
            try
            {
                HIDSkeletonBase.HIDWriteDevice?.WriteFeatureData(array);
                //if (IsAttached)
                //{

                //}
            }
            catch (Exception e)
            {
                SetLastException(e);
            }
        }

        public override void ImportSettings(GenericPanelBinding genericPanelBinding) {}

        public override List<string> ExportSettings()
        {
            if (Closed)
            {
                return null;
            }
            return new List<string>();
        }

        protected bool SkipCurrentFrequencyChange()
        {
            switch (FrequencyKnobSensitivity)
            {
                case 0:
                    {
                        //Do nothing all manipulation is let through
                        break;
                    }
                case -1:
                    {
                        //Skip every 2 manipulations
                        _frequencySensitivitySkipper++;
                        if (_frequencySensitivitySkipper <= 2)
                        {
                            return true;
                        }
                        _frequencySensitivitySkipper = 0;
                        break;
                    }
                case -2:
                    {
                        //Skip every 4 manipulations
                        _frequencySensitivitySkipper++;
                        if (_frequencySensitivitySkipper <= 4)
                        {
                            return true;
                        }
                        _frequencySensitivitySkipper = 0;
                        break;
                    }
            }
            return false;
        }

        protected void StartupBase(string id)
        {
            try
            {
            }
            catch (Exception ex)
            {
                SetLastException(ex);
            }
        }

        protected void ShutdownBase()
        {
            try
            {
                Closed = true;
                //Damn hanging problems. Trying threading this shit now.
                var thread = new Thread(ShutdownBaseThreaded);
                thread.Start();
                Thread.Sleep(200);
            }
            catch (Exception e)
            {
                SetLastException(e);
            }
        }

        private void ShutdownBaseThreaded()
        {
            try
            {
                //HIDSkeletonBase = null;
            }
            catch (Exception e)
            {
                SetLastException(e);
            }
        }

        public override void SavePanelSettings(object sender, ProfileHandlerEventArgs e)
        {
            e.ProfileHandlerEA.RegisterPanelBinding(this, ExportSettings());
        }

        protected void DeviceAttachedHandler()
        {
            Startup();
            //IsAttached = true;
        }


        protected void DeviceRemovedHandler()
        {
            Dispose();
            //IsAttached = false;
        }

        protected void Reset(ref long syncVariable)
        {
            syncVariable = DateTime.Now.Ticks;
        }

        protected bool IsTimedOut(ref long syncVariable, long timeoutValue, string name)
        {

            if (DateTime.Now.Ticks - syncVariable > timeoutValue)
            {
                syncVariable = DateTime.Now.Ticks;
                return true;
            }
            return false;
        }

        protected bool IsTooShort(long dialOkTime)
        {
            if (DateTime.Now.Ticks - dialOkTime > _syncOKDelayTimeout)
            {
                return false; //good!
            }
            return true; //wait more
        }

        public int SynchSleepTime
        {
            get => _synchSleepTime;
            set => _synchSleepTime = value;
        }

        public long ResetSyncTimeout
        {
            get => _resetSyncTimeout;
            set => _resetSyncTimeout = value;
        }

        public long SyncOKDelayTimeout
        {
            get => _syncOKDelayTimeout / 10000;
            set => _syncOKDelayTimeout = value * 10000;
        }
        public NumberFormatInfo NumberFormatInfo
        {
            get => NumberFormatInfoFullDisplay;
            set => NumberFormatInfoFullDisplay = value;
        }

        public int FrequencyKnobSensitivity
        {
            get => _frequencyKnobSensitivity;
            set => _frequencyKnobSensitivity = value;
        }

        protected bool IgnoreSwitchButtonOnce()
        {
            //Counter for both upper ACT/STBY and lower
            if (_ignoreSwitchButtonCounter > 0)
            {
                _ignoreSwitchButtonCounter--;
            }

            return _ignoreSwitchButtonCounter > 0;
        }
    }
}
