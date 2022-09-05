﻿namespace NonVisuals.CockpitMaster.Panels
{
    using System;
    using System.Collections.Generic;
    using System.Threading;

    using ClassLibraryCommon;

    using DCS_BIOS;
    using DCS_BIOS.EventArgs;
    using DCS_BIOS.Interfaces;

    using NonVisuals.EventArgs;
    using NonVisuals.CockpitMaster.Switches;
    using HidLibrary;
    using NonVisuals.Interfaces;
    using System.Timers;
    using Timer = System.Timers.Timer;

    public enum CDU737Led
    {
        EXEC = 0b0000001,      // EXEC on 
        MSG = 0b0000010,     // MSG
        OFST = 0b0000100,     // OFST
        FAIL = 0b0001000,     // FAIL
        CALL = 0b0010000,     // CALL
    }

    public class CDU737PanelBase : CockpitMasterPanel, IDCSBIOSStringListener
    {
        public const int MAX_BRIGHT = 0xff;
        private const int BRIGHTNESS_STEP = 10;

        // refresh the CDU 2 times / sec. 
        // ok for most case , except when the master caution is blinking very fast in the A10

        private const int TICK_DISPLAY = 500;

        private int _screenBrightness = MAX_BRIGHT / 2;
        private int _keyboardBrightness = MAX_BRIGHT / 2;

        public const int LINES_ON_CDU = 14;

        private byte[][] ScreenBuffer;
        private byte[] LedBuffer;

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


        private byte LedStatus = 0;

        private Dictionary<char, CDUCharset> _convertTable = CDUTextLineHelpers.defaultConvertTable;

        private CDUTextLine[] _TextLines = new CDUTextLine[LINES_ON_CDU];

        public string[] CDULines
        {
            get
            {
                string[] result = new  string[LINES_ON_CDU];

                for (int i=0;i< LINES_ON_CDU;i++ )
                {
                    result[i] = _TextLines[i].Line;

                }
                return result;
            }
        }

        //private HashSet<DCSBIOSActionBindingCDU737> _dcsBiosBindings = new HashSet<DCSBIOSActionBindingCDU737>();

        protected HashSet<CDUMappedKey> CDUPanelKeys = new();

        private readonly Timer _displayCDUTimer = new Timer(TICK_DISPLAY);

        private readonly object _lockUpdateDisplay = new();

        public CDU737PanelBase(HIDSkeleton hidSkeleton) : base(GamingPanelEnum.CDU737, hidSkeleton)
        {
            if (hidSkeleton.GamingPanelType != GamingPanelEnum.CDU737)
            {
                throw new ArgumentException();
            }


            initCDU();

            VendorId = (int)GamingPanelVendorEnum.CockpitMaster;
            ProductId = (int)GamingPanelEnum.CDU737;

            Startup();
            BIOSEventHandler.AttachStringListener(this);
            BIOSEventHandler.AttachDataListener(this);

        }

        private void StartTimers()
        {
            _displayCDUTimer.Elapsed += TimedDisplayBufferOnCDU;
            _displayCDUTimer.Start();

        }

        private void initCDU()
        {
            // Init ScreenBuffer ( HidReport Representation of the CDU ) 
            ScreenBuffer = new byte[9][];

            for (byte i = 0; i < 9; i++)
            {
                ScreenBuffer[i] = new byte[63];
            }

            LedBuffer = new byte[64];
            LedBuffer[0] = 9;

            ScreenBrightness = MAX_BRIGHT;
            KeyboardBrightness = MAX_BRIGHT;

            // Init Text Lines 
            for (int line = 0; line < LINES_ON_CDU; line++)
            {
                _TextLines[line] = new CDUTextLine();
            }

            SetLine(5, "      DCS CDU 737       ");
            SetLine(7, "       by Cerppo        ");
            SetLine(9, "* waiting dcsBios data *");

            //SetLine(11, "        Rocker          ");
            //SetLine(12, "       Bindings         ");

            //_TextLines[11].applyColorToLine(CDUColors.WHITE);
            //_TextLines[12].applyColorToLine(CDUColors.WHITE);
            //_TextLines[13].applyColorToLine(CDUColors.WHITE);

            //_TextLines[11].setDisplayedCharAt(new DisplayedChar(CDUCharset.p, CDUColors.CYAN, true, true), 0);
            //_TextLines[12].setDisplayedCharAt(new DisplayedChar(CDUCharset.g, CDUColors.CYAN, true, true), 0);

            //_TextLines[11].setDisplayedCharAt(new DisplayedChar(CDUCharset.uparrow, CDUColors.CYAN, true, true), 23);
            //_TextLines[12].setDisplayedCharAt(new DisplayedChar(CDUCharset.downarrow, CDUColors.CYAN, true, true), 23);

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
                    BIOSEventHandler.DetachStringListener(this);
                    BIOSEventHandler.DetachDataListener(this);
                }

                _disposed = true;
            }

            // Call base class implementation.
            base.Dispose(disposing);
        }

        public Dictionary<char, CDUCharset> ConvertTable { 
            get
            {
                return _convertTable;
            } 
            set
            {
                _convertTable = value;
                foreach (CDUTextLine line in _TextLines) line.ConvertTable = value;

            }
        }

        public override void ImportSettings(GenericPanelBinding genericPanelBinding)
        {
            ClearSettings();

            BindingHash = genericPanelBinding.BindingHash;

            var settings = genericPanelBinding.Settings;
            SettingsLoading = true;
            
            foreach (var setting in settings)
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

            var result = new List<string>();

      
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



        protected override void GamingPanelKnobChanged(bool isFirstReport, IEnumerable<object> hashSet)
        {
            if (isFirstReport)
            {
                return;
            }

            try
            {
                foreach(CDUMappedKey key in hashSet)
                {
                    _ = DCSBIOS.Send(key.MappedCommand());
                }
                  
            }
            catch(Exception)
            {

            }
            
        }

        //public HashSet<DCSBIOSActionBindingCDU737> DCSBiosBindings
        //{
        //    get => _dcsBiosBindings;
        //    set => _dcsBiosBindings = value;
        //}

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
                LedBuffer[1] = (byte) _screenBrightness;
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
                LedBuffer[2] = (byte)_keyboardBrightness;
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
           LedStatus |= (byte)led;


        }

        public void Led_OFF(CDU737Led led)
        {
            LedStatus &= unchecked((byte)~led);
        }

        public void SetLine(int line, string text)
        {
            if (line < 0 || line > LINES_ON_CDU-1) throw new ArgumentOutOfRangeException("Line must be 0 to 13");
            _TextLines[line].Line = text;

        }

        public void SetColorForLine( int line, CDUColor color)
        {
            _TextLines[line].applyColorToLine(color);
        }

        public void SetMaskColorForLine( int line, CDUColor[] mask)
        {
            _TextLines[line].applyMaskColor(mask);
        }

        private void TimedDisplayBufferOnCDU(object sender, ElapsedEventArgs e)
        {
            // splitted in two methods because i was experimenting with another way to refresh, more event bases thant "tick" based
            // A10C quick caution pulse is not displayed correctly when the refresh is time based. 
            // if you send too much refresh to the CDU, it starts to "lag" ... 

            displayBufferOnCDU();
        }

        private void displayBufferOnCDU()
        {

            // Data structure is 
            // 3 bytes for 2 char
            // first byte is char 1 0x47  =>  -
            // second byte : 4 most it  , low part of the 2nd char ( if we want a - too, it's the 7 of the 0x47 )
            //             : 4 lowest = color of the 1st char
            // Third Byte  : most significant 4 bits , color of the 2nd char 
            //             / lsb : Hight part of the 2nd char ( it's the 4 of the 0x47)

            // Simply put  tow chars 0x47 0x47 
            //  =>                   0x47 0x7(col1) 0x(col2)4 

            lock (_lockUpdateDisplay)
            {


                HidLibrary.HidReport[] hidReport = {
                _hidWriteDevice.CreateReport(),
                _hidWriteDevice.CreateReport(),
                _hidWriteDevice.CreateReport(),
                _hidWriteDevice.CreateReport(),
                _hidWriteDevice.CreateReport(),
                _hidWriteDevice.CreateReport(),
                _hidWriteDevice.CreateReport(),
                _hidWriteDevice.CreateReport(),
                _hidWriteDevice.CreateReport(),
            };

                // Copy lines to ScreenBuffer
                int currentBuffer = 0;
                int copied = 0;
                int needbreak = 63;

                for (int line = 0; line < LINES_ON_CDU; line++)
                {
                    // Scan all the lines, 
                    // copy 63 char in a "buffer" , 
                    byte[] tempo = _TextLines[line].getEncodedBytes();

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

                // buffer is a 9 lines byte[]
                // 8 first byte[] maps the chars
                // 9th maps the LED status, brightness of screen / keyboard
                // as far as i understand, most of the 9th is filled with blank


                for (int i = 0; i < 8; i++)
                {
                    // Doing this here, ensure the Numbering of HidReport is not "broken" 
                    // by mistake, and simplifies "recopy of lines in buffers
                    byte[] buffer = new byte[64];
                    buffer[0] = (byte)(i + 1);
                    Array.Copy(ScreenBuffer[i], 0, buffer, 1, 63);
                    hidReport[i].Data = buffer;
                    _ = _hidWriteDevice.WriteReportAsync(hidReport[i]);


                }

                // Handles LED 

                LedBuffer[3] = LedStatus;
                hidReport[8].Data = LedBuffer;

                _ = _hidWriteDevice.WriteReport(hidReport[8]);
            }

        }

        public override void Startup()
        {
            
        }

        public override void DcsBiosDataReceived(object sender, DCSBIOSDataEventArgs e)
        {
            
        }

        public void DCSBIOSStringReceived(object sender, DCSBIOSStringDataEventArgs e)
        {
            
        }

        private void OnReport(HidReport report)
        {
            if (TypeOfPanel == GamingPanelEnum.CDU737 && report.Data.Length == 64)
            {
                Array.Copy(NewCDUPanelValues, OldCDUPanelValues, 64);
                Array.Copy(report.Data, NewCDUPanelValues, 64);
                var hashSet = GetHashSetOfChangedKnobs(OldCDUPanelValues, NewCDUPanelValues);
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
            var result = new HashSet<object>();

            for (var i = 0; i < 64; i++)
            {
                var oldByte = oldValue[i];
                var newByte = newValue[i];

                foreach (var key in CDUPanelKeys)
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

    }

}
