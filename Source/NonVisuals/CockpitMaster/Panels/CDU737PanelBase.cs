namespace NonVisuals.CockpitMaster.Panels
{
    using System;
    using System.Collections.Generic;
    using ClassLibraryCommon;

    using DCS_BIOS;
    using DCS_BIOS.EventArgs;
    using EventArgs;
    using Switches;
    using HidLibrary;
    using Interfaces;
    using System.Timers;
    using Timer = System.Timers.Timer;
    using HID;

    public enum CDU737Led
    {
        EXEC = 0b0000001,      // EXEC on 
        MSG = 0b0000010,     // MSG
        OFST = 0b0000100,     // OFST
        FAIL = 0b0001000,     // FAIL
        CALL = 0b0010000,     // CALL
    }

    public class CDU737PanelBase : CockpitMasterPanel
    {
        public const int MAX_BRIGHT = 0xff;
        private const int BRIGHTNESS_STEP = 10;

        // refresh the CDU 4 times / sec. 
        // ok for most case , except when the master caution is blinking very fast in the A10

        private const int TICK_DISPLAY = 250;

        private int _screenBrightness = MAX_BRIGHT / 2;
        private int _keyboardBrightness = MAX_BRIGHT / 2;

        public const int LINES_ON_CDU = 14;

        private byte[][] ScreenBuffer;
        private byte[] BrightAndLedBuffer;

        protected byte[] OldCDUPanelValues = {
            0,0,0,0,0,0,0,0,
            0,0,0,0,0,0,0,0,
            0,0,0,0,0,0,0,0,
            0,0,0,0,0,0,0,0,
            0,0,0,0,0,0,0,0,
            0,0,0,0,0,0,0,0,
            0,0,0,0,0,0,0,0,
            0,0,0,0,0,0,0,0
        };

        protected byte[] NewCDUPanelValues = {
            0,0,0,0,0,0,0,0,
            0,0,0,0,0,0,0,0,
            0,0,0,0,0,0,0,0,
            0,0,0,0,0,0,0,0,
            0,0,0,0,0,0,0,0,
            0,0,0,0,0,0,0,0,
            0,0,0,0,0,0,0,0,
            0,0,0,0,0,0,0,0
        };

        // 14 lines to track for DCSBios Change
        protected string[] oldCDULines =
        {
            "","","","","","","",
            "","","","","","","",
        };

        private HidReport[] _hidReport;

        // This byte stores the physical panel Led Status 
        // it's a combination of bits coming from CDU737Led enum
        private byte _ledStatus = 0;

        // Default convertTable to transform chars from DCSBios to 
        // existing chars in the CDU charset
        private Dictionary<char, CDUCharset> _convertTable = CDUTextLineHelpers.defaultConvertTable;

        // storage of lines status in a human comprehensible way
        // is used later to encode the hidreport buffers 
        private readonly CDUTextLine[] _textLines = new CDUTextLine[LINES_ON_CDU];

        private CDUColor _baseColor;

        public string[] CDULines
        {
            get
            {
                string[] result = new string[LINES_ON_CDU];

                for (int i=0;i< LINES_ON_CDU;i++ )
                {
                    result[i] = _textLines[i].Line;
                }
                return result;
            }
        }

        protected HashSet<CDUMappedCommandKey> CDUPanelKeys = new();

        private readonly Timer _displayCDUTimer = new(TICK_DISPLAY);

        private readonly object _lockUpdateDisplay = new();

        public CDU737PanelBase(HIDSkeleton hidSkeleton) : base(GamingPanelEnum.CDU737, hidSkeleton)
        {
            VendorId = (int)GamingPanelVendorEnum.CockpitMaster;
            ProductId = (int)GamingPanelEnum.CDU737;
        }

        private void StartTimers()
        {
            _displayCDUTimer.Elapsed += TimedDisplayBufferOnCDU;
            _displayCDUTimer.Start();
        }

        public CDUColor BaseColor
        {
            get
            {
                return _baseColor;
            }
            set
            {
                _baseColor = value;
                for (int i = 0; i < LINES_ON_CDU; i++)
                {
                    SetColorForLine(i, _baseColor);
                }
            }
        }
        
        public override void Init()
        {
            if (HIDSkeletonBase.GamingPanelType != GamingPanelEnum.CDU737)
            {
                throw new ArgumentException($"GamingPanelType {GamingPanelEnum.CDU737} expected");
            }

            // Init ScreenBuffer ( HidReport Representation of the CDU ) 
            // The screen on the physical device is refreshed by sending 
            // 9 HidReport of 64 bytes. 
            // the 1st byte of each report is the "packet" number, from 1 to 9

            ScreenBuffer = new byte[9][];

            for (byte i = 0; i < 9; i++)
            {
                // i keep the 1st byte off this buffer on purpose
                // helps copying text from cdu lines to buffers. 
                ScreenBuffer[i] = new byte[63];
            }

            // this is the 9th hidreport
            // it contains 3 significant bytes 
            // Screen brightness 
            // Keyboard brightness
            // led Buffer
            BrightAndLedBuffer = new byte[64];
            BrightAndLedBuffer[0] = 9;

            ScreenBrightness = MAX_BRIGHT;
            KeyboardBrightness = MAX_BRIGHT;

            // Init Text Lines 
            for (int line = 0; line < LINES_ON_CDU; line++)
            {
                _textLines[line] = new CDUTextLine();
            }

            SetLine(5, "      DCS CDU 737       ");
            SetLine(7, "       by Cerppo        ");
            SetLine(9, "* waiting dcsBios data *");

            _hidReport = new HidReport[] {
                HIDWriteDevice.CreateReport(),
                HIDWriteDevice.CreateReport(),
                HIDWriteDevice.CreateReport(),
                HIDWriteDevice.CreateReport(),
                HIDWriteDevice.CreateReport(),
                HIDWriteDevice.CreateReport(),
                HIDWriteDevice.CreateReport(),
                HIDWriteDevice.CreateReport(),
                HIDWriteDevice.CreateReport(),
            };

            StartTimers();
        }

        private bool _disposed;
        // Protected implementation of Dispose pattern.
        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _displayCDUTimer.Stop();
                    _displayCDUTimer.Dispose();
                }

                _disposed = true;
            }

            // Call base class implementation.
            base.Dispose(disposing);
        }

        protected override void GamingPanelKnobChanged(bool isFirstReport, IEnumerable<object> hashSet)
        {

        }

        public Dictionary<char, CDUCharset> ConvertTable { 
            get
            {
                return _convertTable;
            } 
            set
            {
                _convertTable = value;
                foreach (CDUTextLine line in _textLines) line.ConvertTable = value;
            }
        }

        public override void ImportSettings(GenericPanelBinding genericPanelBinding)
        {
            ClearSettings();

            BindingHash = genericPanelBinding.BindingHash;

            List<string> settings = genericPanelBinding.Settings;
            SettingsLoading = true;
            
            foreach (string setting in settings)
            {
                if (!setting.StartsWith("#") && setting.Length > 2)
                {
                }
            }
          
            AppEventHandler.SettingsApplied(this, HIDSkeletonBase.HIDInstance, TypeOfPanel);
        }

        public override List<string> ExportSettings()
        {
            if (Closed)
            {
                return null;
            }

            List<string> result = new();
      
            return result;
        }

        public override void SavePanelSettings(object sender, ProfileHandlerEventArgs e)
        {
            e.ProfileHandlerCaller.RegisterPanelBinding(this, ExportSettings());
        }

        public override void SavePanelSettingsJSON(object sender, ProfileHandlerEventArgs e) { }

        public override void ClearSettings(bool setIsDirty = false)
        {

            if (setIsDirty)
            {
                SetIsDirty();
            }
        }

        public override void Identify()
        {
        }


        public int ScreenBrightness
        {
            get
            {
                return _screenBrightness;
            }

            set
            {
                _screenBrightness = value;
                if (_screenBrightness < 0) _screenBrightness = 0;
                if (_screenBrightness > MAX_BRIGHT) _screenBrightness = MAX_BRIGHT;
                BrightAndLedBuffer[1] = (byte) _screenBrightness;
            }
        }

        public int KeyboardBrightness
        {
            get
            {
                return _keyboardBrightness;
            }
            set
            {
                _keyboardBrightness = value;
                if (_keyboardBrightness < 0) _keyboardBrightness = 0;
                if (_keyboardBrightness > MAX_BRIGHT) _keyboardBrightness = MAX_BRIGHT;
                BrightAndLedBuffer[2] = (byte)_keyboardBrightness;
            }
        }

        public void IncreaseBrightness()
        {
            ScreenBrightness += BRIGHTNESS_STEP;
            KeyboardBrightness += BRIGHTNESS_STEP;
        }

        public void DecreaseBrighness()
        {
            ScreenBrightness -= BRIGHTNESS_STEP;
            KeyboardBrightness -= BRIGHTNESS_STEP;
        }

        public void Led_ON(CDU737Led led)
        {
           _ledStatus |= (byte)led;
        }

        public void Led_OFF(CDU737Led led)
        {
            _ledStatus &= unchecked((byte)~led);
        }

        public void SetLine(int line, string text)
        {
            if (line < 0 || line > LINES_ON_CDU-1) throw new ArgumentOutOfRangeException(nameof(line), "CDU Line must be 0 to 13");
            _textLines[line].Line = text;
        }

        public void SetDisplayCharAt( int line, DisplayedChar ch, int index)
        {
            if (line < 0 || line > LINES_ON_CDU - 1) throw new ArgumentOutOfRangeException(nameof(line), "CDU Line must be 0 to 13");
            _textLines[line].SetDisplayedCharAt(ch, index);

        }

        public void SetColorForLine( int line, CDUColor color)
        {
            _textLines[line].ApplyColorToLine(color);
        }

        public void SetMaskColorForLine( int line, CDUColor[] mask)
        {
            _textLines[line].ApplyMaskColor(mask);
        }

        private void TimedDisplayBufferOnCDU(object sender, ElapsedEventArgs e)
        {
            // splitted in two methods because i was experimenting with another way to refresh, more event bases thant "tick" based
            // A10C quick caution pulse is not displayed correctly when the refresh is time based. 
            // if you send too much refresh to the CDU, it starts to "lag" ... 

            displayBufferOnCDU();
        }

        protected void displayBufferOnCDU()
        {

            // Data structure in the hidReport is 
            // 3 bytes for 2 char
            // Simply put two chars 0x47 0x47 
            // =>                   0x47 0x7(col1) 0x(col2)4 
            // => if the color is 0x3 and 0x5
            // => 0x47 0x73 0x54

            lock (_lockUpdateDisplay)
            {
                // Copy lines to ScreenBuffer
                int currentBuffer = 0;
                int copied = 0;
                int needbreak = 63;

                for (int line = 0; line < LINES_ON_CDU; line++)
                {
                    // Scan all the lines, 
                    // copy 63 char in a "buffer" , 
                    byte[] tempo = _textLines[line].GetEncodedBytes();

                    for (int i = 0; i < tempo.Length; i++)
                    {
                        ScreenBuffer[currentBuffer][copied++] = tempo[i];
                        if (copied == needbreak)
                        {
                            copied = 0;
                            currentBuffer++;
                        }

                    }
                }

                // buffer is a 9 byte[]
                // 8 first byte[] maps the chars on the screen (14 lines!)
                // then 9th byte[] maps the LED status, brightness of screen / keyboard
                // as far as i understand, most of the 9th is filled with blank

                for (int i = 0; i < 8; i++)
                {
                    // Screenbuffers are declared 63 bytes
                    // Doing this here, ensure the Numbering of HidReport is not "broken" 
                    // by mistake, and simplifies "recopy of lines in buffers

                    _hidReport[i].Data[0] = (byte)(i + 1);
                    Array.Copy(ScreenBuffer[i], 0, _hidReport[i].Data, 1, 63);
                    _ = HIDWriteDevice.WriteReportAsync(_hidReport[i]);
                }
            }

        }

        public void refreshLedsAndBrightness()
        {
            // Handles LED 
            // BrightAndLefbuffer[0] = 9 and should not be modified 

            BrightAndLedBuffer[3] = _ledStatus;
            _hidReport[8].Data = BrightAndLedBuffer;

            _ = HIDWriteDevice.WriteReportAsync(_hidReport[8]);

        }
        
        public override void DcsBiosDataReceived(object sender, DCSBIOSDataEventArgs e)
        {

        }

        private void OnReport(HidReport report)
        {
            if (TypeOfPanel == GamingPanelEnum.CDU737 && report.Data.Length == 64)
            {
                Array.Copy(NewCDUPanelValues, OldCDUPanelValues, 64);
                Array.Copy(report.Data, NewCDUPanelValues, 64);
                HashSet<object> hashSet = GetHashSetOfChangedKnobs(OldCDUPanelValues, NewCDUPanelValues);
                if (hashSet.Count > 0)
                {
                    GamingPanelKnobChanged(!FirstReportHasBeenRead, hashSet);
                    AppEventHandler.SwitchesChanged(this, HIDSkeletonBase.HIDInstance, TypeOfPanel, hashSet);
                }

                FirstReportHasBeenRead = true;
            }

            StartListeningForHidPanelChanges();
        }

        private HashSet<object> GetHashSetOfChangedKnobs(byte[] oldValue, byte[] newValue)
        {
            HashSet<object> result = new();

            for (int i = 0; i < 64; i++)
            {
                byte oldByte = oldValue[i];
                byte newByte = newValue[i];

                foreach (CDUMappedCommandKey key in CDUPanelKeys)
                {
                    if (key.Group == i && (FlagHasChanged(oldByte, newByte, key.Mask) || !FirstReportHasBeenRead))
                    {
                        key.IsOn = FlagValue(newValue, key);
                        result.Add(key);
                    }
                }

            }

            return result;
        }

        protected override void StartListeningForHidPanelChanges()
        {
            try
            {
                if (HIDSkeletonBase.HIDReadDevice != null && !Closed)
                {
                    HIDSkeletonBase.HIDReadDevice.ReadReportAsync().ContinueWith(x => OnReport(x.Result));
                }
            }
            catch (Exception)
            {
                // ignored
            }
        }

        private static bool FlagValue(byte[] currentValue, ICockpitMasterCDUKey panelKnob)
        {
            return (currentValue[panelKnob.Group] & panelKnob.Mask) > 0;
        }


        protected bool HandleStringData( int line, DCSBIOSStringDataEventArgs e,
            string data, 
            ref int changed)
        {

            if (string.Compare(data,oldCDULines[line])==0)
            {
                return false;
            }
            
            changed++;
            oldCDULines[line] = data;
            return true;

        }

        protected static (bool, uint) ShouldHandleDCSBiosData(DCSBIOSDataEventArgs e, DCSBIOSOutput output)
        {
            if (e.Address != output.Address) return (false, 0);
            var oldValue = output.LastUIntValue;
            var newValue = output.GetUIntValue(e.Data);
            if (oldValue == newValue) return (false, 0);

            return (true, output.GetUIntValue(e.Data));
        }
    }
}
