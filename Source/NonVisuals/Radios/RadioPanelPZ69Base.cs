namespace NonVisuals.Radios
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Threading;

    using ClassLibraryCommon;
    using NonVisuals.EventArgs;
    using NonVisuals.Radios.Misc;
    using NonVisuals.Saitek.Panels;

    public enum PZ69LCDPosition
    {
        UPPER_ACTIVE_LEFT = 1,
        UPPER_STBY_RIGHT = 6,
        LOWER_ACTIVE_LEFT = 11,
        LOWER_STBY_RIGHT = 16
    }

    public abstract class RadioPanelPZ69Base : SaitekPanel
    {
        private byte _ignoreSwitchButtonCounter = 1;
        private readonly NumberFormatInfo _numberFormatInfoFullDisplay;
        private int _frequencyKnobSensitivity;
        private volatile int _frequencySensitivitySkipper;
        protected readonly object LockLCDUpdateObject = new object();
        protected bool DataHasBeenReceivedFromDCSBIOS;

        /// <summary>
        /// IMPORTANT WHEN SYNCHING DIALS
        /// MSDN (DateTime.Now.Ticks : There are 10,000 ticks in a millisecond
        /// </summary>
        public int SynchSleepTime { get; set; } = 300;
        public long ResetSyncTimeout { get; set; } = 35000000;

        private long _syncOKDelayTimeout = 50000000; // 5s
        private readonly PZ69DisplayBytes _pZ69DisplayBytes = new PZ69DisplayBytes();

        protected RadioPanelPZ69Base(HIDSkeleton hidSkeleton, AppEventHandler appEventHandler) : base(GamingPanelEnum.PZ69RadioPanel, hidSkeleton, appEventHandler)
        {
            if (hidSkeleton.GamingPanelType != GamingPanelEnum.PZ69RadioPanel)
            {
                throw new ArgumentException();
            }

            _numberFormatInfoFullDisplay = new NumberFormatInfo
            {
                NumberDecimalSeparator = ".",
                NumberDecimalDigits = 4,
                NumberGroupSeparator = string.Empty
            };
        }

        private bool _disposed;
        // Protected implementation of Dispose pattern.
        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                }

                _disposed = true;
            }

            // Call base class implementation.
            base.Dispose(disposing);
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

        /// <summary>
        /// Sets the given position to blank without modifying the other positions in the array
        /// </summary>
        protected void SetPZ69DisplayBlank(ref byte[] bytes, PZ69LCDPosition pz69LCDPosition)
        {
            _pZ69DisplayBytes.SetPositionBlank(ref bytes, pz69LCDPosition);
        }

        public override void SavePanelSettingsJSON(object sender, ProfileHandlerEventArgs e)
        {
        }

        /// <summary>
        /// Right justify, pad left with blanks.
        /// </summary>
        protected void SetPZ69DisplayBytesUnsignedInteger(ref byte[] bytes, uint digits, PZ69LCDPosition pz69LCDPosition)
        {
            _pZ69DisplayBytes.UnsignedInteger(ref bytes, digits, pz69LCDPosition);
        }

        protected void SetPZ69DisplayBytes(ref byte[] bytes, double digits, int decimals, PZ69LCDPosition pz69LCDPosition)
        {
            _pZ69DisplayBytes.DoubleWithSpecifiedDecimalsPlaces(ref bytes, digits, decimals, pz69LCDPosition);
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
                var bytes = new byte[21];
                bytes[0] = 0x0;
                var random = new Random();
                var lcdPositionArray = Enum.GetValues(typeof(PZ69LCDPosition));
                var lcdValueArray = new[] { "00000", "11111", "22222", "33333", "44444", "55555", "66666", "77777", "88888", "99999", "12345", "1.2.3.4.5." };
                var spins = 12;

                while (spins > 0)
                {
                    var position = (PZ69LCDPosition)lcdPositionArray.GetValue(random.Next(lcdPositionArray.Length));
                    var value = (string)lcdValueArray.GetValue(random.Next(lcdValueArray.Length));

                    SetPZ69DisplayBytesDefault(ref bytes, value, position);
                    SendLCDData(bytes);

                    Thread.Sleep(500);
                    spins--;
                }

                TurnOffAllDisplays();
            }
            catch (Exception)
            {
                // ignored
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
            catch (Exception)
            {
                // ignored
            }
        }

        /// <summary>
        /// Expect a string of 5 chars that are going to be displayed as it.
        /// Can deal with multiple '.' chars.
        /// If size does not match 5, it will NOT replace previous characters in the array (no padding left or right).
        /// </summary>
        protected void SetPZ69DisplayBytesDefault(ref byte[] bytes, string digits, PZ69LCDPosition pz69LCDPosition)
        {
            try
            {
                _pZ69DisplayBytes.DefaultStringAsIs(ref bytes, digits, pz69LCDPosition);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "SetPZ69DisplayBytesDefault()");
            }
        }

        protected void SetPZ69DisplayBytesDefault(ref byte[] bytes, double digits, PZ69LCDPosition pz69LCDPosition)
        {
            _pZ69DisplayBytes.DoubleJustifyLeft(ref bytes, digits, pz69LCDPosition);
        }

        public void SendLCDData(byte[] array)
        {
            try
            {
                HIDSkeletonBase.HIDWriteDevice?.WriteFeatureData(array);
                // if (IsAttached)
                // {

                // }
            }
            catch (Exception ex)
            {
                SetLastException(ex);
            }
        }

        public override void ImportSettings(GenericPanelBinding genericPanelBinding)
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
                        // Do nothing all manipulation is let through
                        break;
                    }

                case -1:
                    {
                        // Skip every 2 manipulations
                        Interlocked.Increment(ref _frequencySensitivitySkipper);
                        if (_frequencySensitivitySkipper <= 2)
                        {
                            return true;
                        }

                        _frequencySensitivitySkipper = 0;
                        break;
                    }

                case -2:
                    {
                        // Skip every 4 manipulations
                        Interlocked.Increment(ref _frequencySensitivitySkipper);
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
        
        public override void SavePanelSettings(object sender, ProfileHandlerEventArgs e)
        {
            e.ProfileHandlerCaller.RegisterPanelBinding(this, ExportSettings());
        }
        

        protected void Reset(ref long syncVariable)
        {
            syncVariable = DateTime.Now.Ticks;
        }

        protected bool IsTimedOut(ref long syncVariable)
        {
            if (DateTime.Now.Ticks - syncVariable > ResetSyncTimeout)
            {
                syncVariable = DateTime.Now.Ticks;
                return true;
            }
            return false;
        }

        protected void ResetWaitingForFeedBack(ref long syncVariable)
        {
            Interlocked.Exchange(ref syncVariable, 0);
        }

        protected bool IsTooShort(long dialOkTime)
        {
            if (DateTime.Now.Ticks - dialOkTime > _syncOKDelayTimeout)
            {
                return false; // good!
            }

            return true; // wait more
        }

        public long SyncOKDelayTimeout
        {
            get => _syncOKDelayTimeout / 10000;
            set => _syncOKDelayTimeout = value * 10000;
        }

        public NumberFormatInfo NumberFormatInfoFullDisplay
        {
            get => _numberFormatInfoFullDisplay;
        }

        public int FrequencyKnobSensitivity
        {
            get => _frequencyKnobSensitivity;
            set => _frequencyKnobSensitivity = value;
        }

        protected bool IgnoreSwitchButtonOnce()
        {
            // Counter for both upper ACT/STBY and lower
            if (_ignoreSwitchButtonCounter > 0)
            {
                _ignoreSwitchButtonCounter--;
            }

            return _ignoreSwitchButtonCounter > 0;
        }
    }
}
/*
    Information for writing on the LCD displays. (20 bytes with a header byte (0x0).)

    1 byte (header byte 0x0)
    5 bytes upper left LCD
    5 bytes upper right LCD
    5 bytes lower left LCD
    5 bytes lower right LCD

    0x01 - 0x09 displays the figure 1-9
    0xD1 - 0xD9 displays the figure 1.-9. (figure followed by dot)
    0xFF -> blank, nothing is shown in that spot.

    Byte #1
    00000000
    ||||||||_ COM1 (upper) 
    |||||||_ COM2 (upper)
    ||||||_ NAV1 (upper)
    |||||_ NAV2 (upper)
    ||||_ ADF (upper)
    |||_ DME (upper)
    ||_ XPDR (upper)
    |_ COM1 (lower) **!!!**!!!***!!!

    Byte #2
    00000000
    ||||||||_ COM2 (lower) 
    |||||||_ NAV1 (lower)
    ||||||_ NAV2 (lower)
    |||||_ ADF (lower)
    ||||_ DME (lower)
    |||_ XPDR (lower)
    ||_ Switch (upper)
    |_ Switch (lower)

    Byte #3
    00000000
    ||||||||_ Little (+freq) (upper)
    |||||||_ Little (-freq) (upper)
    ||||||_ Big (+freq) (upper)
    |||||_ Big (-freq) (upper)
    ||||_ Little (+freq) (lower)
    |||_ Little (-freq) (lower)
    ||_ Big (+freq) (lower)
    |_ Big (-freq) (lower)
*/