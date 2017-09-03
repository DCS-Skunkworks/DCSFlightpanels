using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;

namespace NonVisuals
{
    public enum PZ69LCDPosition
    {
        UPPER_LEFT = 1,
        UPPER_RIGHT = 6,
        LOWER_LEFT = 11,
        LOWER_RIGHT = 16
    }

    public abstract class RadioPanelPZ69Base : SaitekPanel
    {
        protected bool FirstReportHasBeenRead = false;
        protected byte[] OldRadioPanelValue = { 0, 0, 0 };
        protected byte[] NewRadioPanelValue = { 0, 0, 0 };
        //protected HidDevice HidReadDevice;
        //protected HidDevice HidWriteDevice;
        protected NumberFormatInfo NumberFormatInfoFullDisplay;
        protected NumberFormatInfo NumberFormatInfoEmpty;
        private int _frequencyKnobSensitivity;
        protected volatile byte FrequencySensitivitySkipper;
        protected object _lockLCDUpdateObject = new object();
        protected bool DataHasBeenReceivedFromDCSBIOS;
        private Guid _guid = Guid.NewGuid();
        /*
         * IMPORTANT WHEN SYNCHING DIALS
         */
        //MSDN (DateTime.Now.Ticks : There are 10,000 ticks in a millisecond
        private int _synchSleepTime = 300;
        private long _resetSyncTimeout = 35000000;
        private long _syncOKDelayTimeout = 50000000; //5s


        public RadioPanelPZ69Base(HIDSkeleton hidSkeleton) : base(SaitekPanelsEnum.PZ69RadioPanel, hidSkeleton)
        {
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
            var i = 0;
            var digitsAsString = digits.ToString().PadLeft(5);

            //D = DARK
            //116 should become DD116!
            //Common.DebugP("Arrayposition : " + arrayPosition + " is " + frequencyAsString);
            do
            {
                //5 digits can be displayed
                //12345 -> 12345
                //116   -> DD116 
                //1     -> DDDD1

                byte b;
                if (digitsAsString[i].ToString().Equals(" "))
                {
                    b = 0xFF;
                }
                else
                {
                    b = Byte.Parse(digitsAsString[i].ToString());
                }
                bytes[arrayPosition] = b;
                //Common.DebugP("Byte [" + arrayPosition + "] has been set to " + b);
                arrayPosition++;
                i++;
            } while (i < digitsAsString.Length && i < 6);
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

        protected void SetPZ69DisplayBytesUnsignedInteger(ref byte[] bytes, uint digits, PZ69LCDPosition pz69LCDPosition)
        {

            var arrayPosition = GetArrayPosition(pz69LCDPosition);
            var i = 0;
            var digitsAsString = digits.ToString().PadLeft(5);

            //D = DARK
            //116 should become DD116!
            //Common.DebugP("Arrayposition : " + arrayPosition + " is " + frequencyAsString);
            do
            {
                //5 digits can be displayed
                //12345 -> 12345
                //116   -> DD116 
                //1     -> DDDD1

                byte b;
                if (digitsAsString[i].ToString().Equals(" "))
                {
                    b = 0xFF;
                }
                else
                {
                    b = Byte.Parse(digitsAsString[i].ToString());
                }
                bytes[arrayPosition] = b;
                //Common.DebugP("Byte [" + arrayPosition + "] has been set to " + b);
                arrayPosition++;
                i++;
            } while (i < digitsAsString.Length && i < 6);
        }

        protected void SetPZ69DisplayBytes(ref byte[] bytes, double digits, int decimals, PZ69LCDPosition pz69LCDPosition)
        {

            var arrayPosition = GetArrayPosition(pz69LCDPosition);
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
                if (digitsAsString[i].ToString().Equals(" "))
                {
                    b = 0xFF;
                }
                else
                {
                    b = Byte.Parse(digitsAsString[i].ToString());
                }
                bytes[arrayPosition] = b;
                if (digitsAsString.Length > i + 1 && digitsAsString[i + 1] == '.')
                {
                    bytes[arrayPosition] = (byte)(bytes[arrayPosition] + 0xd0);
                }
                arrayPosition++;
                i++;
            } while (i < digitsAsString.Length && i < 6);
        }

        protected void SetPZ69DisplayBytesCustom1(ref byte[] bytes, byte[] bytesToBeInjected, PZ69LCDPosition pz69LCDPosition)
        {
            var arrayPosition = GetArrayPosition(pz69LCDPosition);
            var i = 0;
            //Common.DebugP("Arrayposition : " + arrayPosition + " is " + frequencyAsString);
            do
            {
                //5 digits can be displayed
                //12345 -> 12345
                //116   -> DD116 
                //1     -> DDDD1

                bytes[arrayPosition] = bytesToBeInjected[i];
                //Common.DebugP("Byte [" + arrayPosition + "] has been set to " + b);
                arrayPosition++;
                i++;
            } while (i < bytesToBeInjected.Length && i < 5);
        }

        protected void SetPZ69DisplayBytesDefault(ref byte[] bytes, double digits, PZ69LCDPosition pz69LCDPosition)
        {

            var arrayPosition = GetArrayPosition(pz69LCDPosition);
            var i = 0;
            var digitsAsString = digits.ToString("0.0000", NumberFormatInfoFullDisplay);
            //116 should become 116.00!
            //Common.DebugP("Arrayposition : " + arrayPosition + " is " + frequencyAsString);
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

                byte b = 0;
                try
                {
                    b = Byte.Parse(digitsAsString[i].ToString());
                    bytes[arrayPosition] = b;
                }
                catch (Exception e)
                {
                    Common.LogError(38410, e, "SetPZ69DisplayBytesDefault()");
                }
                //Common.DebugP("Byte [" + arrayPosition + "] has been set to " + b);
                if (digitsAsString.Length > i + 1 && digitsAsString[i + 1] == '.')
                {
                    //Common.DebugP("We are at Byte[" + arrayPosition + "] (" + frequencyAsString[i] + ") and next comes the decimal marker");
                    //Add decimal marker
                    //Common.DebugP("Byte is before addition " + bytes[arrayPosition]);
                    bytes[arrayPosition] = (byte)(bytes[arrayPosition] + 0xd0);
                    //Common.DebugP("Byte is after addition " + bytes[arrayPosition]);
                }

                arrayPosition++;
                i++;
            } while (i < digitsAsString.Length && i < 6);
        }

        private int GetArrayPosition(PZ69LCDPosition pz69LCDPosition)
        {
            switch (pz69LCDPosition)
            {
                case PZ69LCDPosition.UPPER_LEFT:
                    {
                        return 1;
                    }
                case PZ69LCDPosition.UPPER_RIGHT:
                    {
                        return 6;
                    }
                case PZ69LCDPosition.LOWER_LEFT:
                    {
                        return 11;
                    }
                case PZ69LCDPosition.LOWER_RIGHT:
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
                if (HIDSkeletonBase.HIDWriteDevice != null)
                {
                    //Common.DebugP("HIDWriteDevice writing feature data " + TypeOfSaitekPanel + " " + GuidString);
                    HIDSkeletonBase.HIDWriteDevice.WriteFeatureData(array);
                }
                //if (IsAttached)
                //{
                //Common.DebugP("Write ending to Radio Panel " + _guid);
                //}
            }
            catch (Exception e)
            {
                Common.DebugP("SendLCDData() :\n" + e.Message + e.StackTrace);
                SetLastException(e);
            }
        }

        public override void ImportSettings(List<string> settings)
        {
        }

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
                        FrequencySensitivitySkipper++;
                        if (FrequencySensitivitySkipper <= 2)
                        {
                            return true;
                        }
                        FrequencySensitivitySkipper = 0;
                        break;
                    }
                case -2:
                    {
                        //Skip every 4 manipulations
                        FrequencySensitivitySkipper++;
                        if (FrequencySensitivitySkipper <= 4)
                        {
                            return true;
                        }
                        FrequencySensitivitySkipper = 0;
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
                Common.DebugP("RadioPanelPZ69Base.StartUp() : " + ex.Message);
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

        public override void SavePanelSettings(ProfileHandler panelProfileHandler)
        {
            panelProfileHandler.RegisterProfileData(this, ExportSettings());
        }

        protected void DeviceAttachedHandler()
        {
            Startup();
            //IsAttached = true;
        }

        protected void DeviceRemovedHandler()
        {
            Shutdown();
            //IsAttached = false;
        }

        protected void Reset(ref long syncVariable)
        {
            syncVariable = DateTime.Now.Ticks;
        }

        protected bool IsTimedOut(ref long syncVariable, long timeoutValue, string name)
        {
            //Common.DebugP(name + " : timeoutValue =" + timeoutValue + "  syncVariable =" + syncVariable + "  DateTime.Now.Ticks = " + DateTime.Now.Ticks + " || " + (DateTime.Now.Ticks - syncVariable));
            if (DateTime.Now.Ticks - syncVariable > timeoutValue)
            {
                syncVariable = DateTime.Now.Ticks;
                return true;
            }
            return false;
        }

        protected bool IsTooShort(long dialOkTime)
        {
            //Common.DebugP("  DateTime.Now.Ticks = " + DateTime.Now.Ticks + " || " + "  dialOkTime = " + dialOkTime + " || " + (DateTime.Now.Ticks - dialOkTime) + "   >   " + _syncOKDelayTimeout);
            if (DateTime.Now.Ticks - dialOkTime > _syncOKDelayTimeout)
            {
                return false; //good!
            }
            return true; //wait more
        }

        public int SynchSleepTime
        {
            get { return _synchSleepTime; }
            set { _synchSleepTime = value; }
        }

        public long ResetSyncTimeout
        {
            get { return _resetSyncTimeout; }
            set { _resetSyncTimeout = value; }
        }

        public long SyncOKDelayTimeout
        {
            get
            {
                //10,000 ticks in a millisecond
                return _syncOKDelayTimeout / 10000;
            }
            set { _syncOKDelayTimeout = value * 10000; }

        }
        public NumberFormatInfo NumberFormatInfo
        {
            get { return NumberFormatInfoFullDisplay; }
            set { NumberFormatInfoFullDisplay = value; }
        }

        public int FrequencyKnobSensitivity
        {
            get { return _frequencyKnobSensitivity; }
            set { _frequencyKnobSensitivity = value; }
        }


    }
}
