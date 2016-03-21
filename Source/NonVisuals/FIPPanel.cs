using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Media.Imaging;
using DCS_BIOS;

namespace NonVisuals
{
    public enum A10FIPPageType : uint 
    {
        None = 0,
        HSI = 1,
        VVI = 2,
        Altimeter = 3,
        Airspeed = 4,
        TurnIndicator = 5,
        Tachometer = 6,
        Custom = 7
    }
    /*
     * FIP programming code used in Flightpanels is based on test code provided by the very helpful programmer Darren Pursey @ Mad Catz * 
     */
    public abstract class FIPPanel : SaitekPanel
    {
        /*
         STANDARD SETUP (8 pages?)
         * 
         * 0 Splash
         * 1 Heading Indicator
         * 2 Altimeter
         * 3 Airspeed Indicator 
         * 4 Vertical speed indicator 
         * 5 Turn Indicator
         * 6 Tachometer
         * 7 Own setup
         */
        private IntPtr _devicePtr;
        private List<uint> _pageList = new List<uint>();
        private object _imageLockObject = new object();
        public const int FIP_RES_X = 320;
        public const int FIP_RES_Y = 240;
        private DeviceTypes _deviceType;
        private Bitmap _lastBitmapUsed;
        private uint _lastPageUsed;
        protected DirectOutputClass.PageCallback PageCallbackDelegate;
        protected DirectOutputClass.SoftButtonCallback SoftButtonCallbackDelegate;
        //private bool _useFileImageListForPages;
        private object _dcsBiosDataReceivedLock = new object();
        private HashSet<DCSBIOSBindingFIP> _dcsBiosBindings = new HashSet<DCSBIOSBindingFIP>();
        private HashSet<KeyBindingFIP> _keyBindings = new HashSet<KeyBindingFIP>();
        private FIPHandler _fipHandler;
        protected bool DoShutdown;

        /*
         * Bitmap related stuff
         */
        //protected System.Drawing.Size FIPBitmapTemplateSize;
        //A black 320*240 bitmap used for generating new bitmaps
        //protected Bitmap FIPBitmapTemplate;

        protected Thread GraphicsDrawingThread;


        protected virtual void PageCallback(IntPtr device, IntPtr page, byte bActivated, IntPtr context) { }
        protected virtual void SoftButtonCallback(IntPtr device, IntPtr buttons, IntPtr context) { }
        public abstract void ThreadedImageGenerator();

        public FIPPanel(IntPtr devicePtr, FIPHandler fipHandler) : base(SaitekPanelsEnum.FIP, null)
        {
            _devicePtr = devicePtr;
            _fipHandler = fipHandler;
            var guidType = Guid.Empty;
            var num = (int)DirectOutputClass.GetDeviceType(_devicePtr, ref guidType);
            _deviceType = string.Compare(guidType.ToString(), "3E083CD8-6A37-4A58-80A8-3D6A2C07513E", true, CultureInfo.InvariantCulture) == 0 ? DeviceTypes.Fip : DeviceTypes.X52Pro;
        }

        protected void InitalizeBase()
        {
            //Here should be determined what pages should be added. Supported airframe? Start page?
            /*_fipDisplay = new DirectOutputDevice(_devicePtr);
            _fipDisplay.Initalize();
            _fipDisplay.AddPage(true);
            _fipDisplay.AddPage(false);*/

            /*_fipDisplay.AddPage(false);
            _fipDisplay.AddPage(false);
            _fipDisplay.AddPage(false);
            _fipDisplay.AddPage(false);
            _fipDisplay.AddPage(false);
            _fipDisplay.AddPage(false);
            _fipDisplay.AddPage(false);*/
            //FIPBitmapTemplateSize = new System.Drawing.Size(320, 240);
            //FIPBitmapTemplate = new Bitmap(FIPBitmapTemplateSize.Width, FIPBitmapTemplateSize.Height);
            /*using (var graphics = Graphics.FromImage(FIPBitmapTemplate))
            {
                graphics.FillRectangle(Brushes.Black, 0, 0, FIPBitmapTemplate.Width, FIPBitmapTemplate.Height);
                var bitmapSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(FIPBitmapTemplate.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromWidthAndHeight(FIPBitmapTemplate.Width, FIPBitmapTemplate.Height));
                FIPBitmapTemplate = BitmapSource2Bitmap(bitmapSource);
            }*/
            GraphicsDrawingThread = new Thread(ThreadedImageGenerator);
            GraphicsDrawingThread.Start();
        }
        
        private Bitmap BitmapSource2Bitmap(BitmapSource bitmapSource)
        {
            using (var outStream = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(bitmapSource));
                enc.Save(outStream);
                var bitmap = new Bitmap(outStream);
                return new Bitmap(bitmap);
            }
        }

        public ReturnValues AddPage(uint pageNumber, bool setActive)
        {
            if (_pageList.Contains(pageNumber))
            {
                return ReturnValues.S_OK;
            }

            var returnValues = DirectOutputClass.AddPage(_devicePtr, (IntPtr)((long)pageNumber), string.Concat(new object[4]
                                                  {
                                                    "0x",
                                                    _devicePtr.ToString(),
                                                    " PageNo: ",
                                                    pageNumber
                                                  }), setActive);
            if (returnValues == ReturnValues.S_OK)
            {
                Debug.Print("Page: " + (pageNumber) + " added");
                _pageList.Add(pageNumber);
            }
            return returnValues;
        }

        public void RemovePage(uint index)
        {
            if (!_pageList.Contains(index))
            {
                return;
            }
            var retVal = DirectOutputClass.RemovePage(_devicePtr, index);
            _pageList.Remove(index);
        }

        protected ReturnValues SetImage(uint page, Bitmap bitmap)
        {
            if (bitmap == null)
            {
                return ReturnValues.E_INVALIDARG;
            }
            var returnValues = ReturnValues.E_FAIL;
            try
            {
                if (DeviceType == DeviceTypes.X52Pro)
                {
                    return ReturnValues.E_FAIL;
                }

                bitmap.RotateFlip(RotateFlipType.Rotate180FlipX);
                var fipImage = bitmap;
                if (fipImage.Width != 320 || fipImage.Height != 240)
                {
                    Debug.Print("Resizing picture");
                    var bitmap1 = new Bitmap(320, 240);
                    var graphics = Graphics.FromImage(bitmap1);
                    graphics.Clear(Color.Black);
                    var num1 = 1.333333f;
                    var num2 = bitmap.Width / (float)bitmap.Height;
                    var num3 = 320f;
                    var num4 = 240f;
                    if (num1 < (double)num2)
                    {
                        num4 = num3 / num2;
                    }
                    else
                    {
                        num3 = num4 * num2;
                    }
                    var bitmap2 = new Bitmap(bitmap.GetThumbnailImage((int)num3, (int)num4, null, IntPtr.Zero));
                    var point = new System.Drawing.Point(320 - (int)num3 >> 1, 240 - (int)num4 >> 1);
                    graphics.DrawImage(bitmap2, point);
                    fipImage = bitmap1;
                }
                returnValues = SetImageNoResize(page, fipImage);
                _lastBitmapUsed = fipImage;
                _lastPageUsed = page;
            }
            catch (Exception ex)
            {
                Common.LogError(8998, ex, "FIPPanel.SetImage");
            }
            return returnValues;
        }

        private ReturnValues SetImageNoResize(uint page, Bitmap fipImage)
        {
            if (fipImage == null)
            {
                return ReturnValues.E_INVALIDARG;
            }
            var returnValues = ReturnValues.E_FAIL;
            try
            {
                if (fipImage == null)
                    return ReturnValues.E_FAIL;
                lock (_imageLockObject)
                {
                    var local1 = fipImage.LockBits(new Rectangle(0, 0, fipImage.Width, fipImage.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
                    var local2 = local1.Scan0;
                    var local3 = local1.Stride * fipImage.Height;
                    returnValues = DirectOutputClass.SetImage(_devicePtr, page, 0U, local3, local2);
                    fipImage.UnlockBits(local1);
                }
            }
            catch (Exception ex)
            {
                Common.LogError(8998, ex, "FIPPanel.SetImageNoResize");
            }
            return returnValues;
        }

        public void Identify()
        {
            if (_pageList.Count == 0)
            {
                AddPage(1, true);
            }
            var str = "Flightpanels,\nconnecting\nSaitek Pro\nFlight Panels\nand DCS!";
            var fipImage = new Bitmap(320, 240);
            var graphics = Graphics.FromImage(fipImage);
            graphics.Clear(Color.Black);
            var font = new Font("Console", 20f);
            var size = TextRenderer.MeasureText(str, font);
            var solidBrush = new SolidBrush(Color.White);
            var point = new PointF(320 - size.Width >> 1, 240 - size.Height >> 1);
            graphics.DrawString(str, font, solidBrush, point);
            fipImage.RotateFlip(RotateFlipType.Rotate180FlipX);
            SetImageNoResize(_lastPageUsed, fipImage);
            //Thread
        }

        public ReturnValues SetString(uint page, string text)
        {
            var font = new Font(text, 10f);
            var size = TextRenderer.MeasureText(text, font);
            var image = new Bitmap(size.Width, size.Height);
            var graphics = Graphics.FromImage(image);
            graphics.Clear(Color.Black);
            var solidBrush = new SolidBrush(Color.White);
            var point = new PointF(0.0f, 0.0f);
            graphics.DrawString(text, font, solidBrush, point);
            return SetImage(_lastPageUsed, image);
        }


        public override sealed void Startup()
        {
            try
            {
                Common.DebugP("FIP PANEL has entered the building...");
                //IsAttached = true;
            }
            catch (Exception ex)
            {
                Common.DebugP("FIPPanel.StartUp() : " + ex.Message);
                SetLastException(ex);
            }
        }

        public override void Shutdown()
        {
            try
            {
                if (_pageList.Count > 0)
                {
                    do
                    {
                        DirectOutputClass.RemovePage(_devicePtr, _pageList[0]);
                        _pageList.Remove(_pageList[0]);
                    } while (_pageList.Count > 0);
                    Closed = true;
                }
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
                if (!setting.StartsWith("#") && setting.Length > 2)
                {
                    if (setting.StartsWith("FIPPanelButton{"))
                    {
                        var keyBinding = new KeyBindingFIP();
                        keyBinding.ImportSettings(setting);
                        _keyBindings.Add(keyBinding);
                    }
                    else if (setting.StartsWith("FIPPanelDCSBIOSControl{"))
                    {
                        var dcsBIOSBindingFIP = new DCSBIOSBindingFIP();
                        dcsBIOSBindingFIP.ImportSettings(setting);
                        _dcsBiosBindings.Add(dcsBIOSBindingFIP);
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
            return result;
        }

        public override void SavePanelSettings(ProfileHandler panelProfileHandler)
        {
            panelProfileHandler.RegisterProfileData(this, ExportSettings());
        }

        public override void DcsBiosDataReceived(uint address, uint data)
        {
            //Common.DebugP("FIP READ ENTERING");
            lock (_dcsBiosDataReceivedLock)
            {
                UpdateCounter(address, data);
            }
            //Common.DebugP("FIP READ EXITING");
        }


        public override DcsOutputAndColorBinding CreateDcsOutputAndColorBinding(SaitekPanelLEDPosition saitekPanelLEDPosition, PanelLEDColor panelLEDColor, DCSBIOSOutput dcsBiosOutput)
        {
            return null;
        }

        public override void ClearSettings()
        {
        }

        public void ClearAllBindings(FIPPanelButtons fipPanelButton)
        {
            //This must accept lists
            foreach (var keyBinding in _keyBindings)
            {
                if (keyBinding.FIPButton == fipPanelButton)
                {
                    keyBinding.OSKeyPress = null;
                }
            }
            foreach (var dcsBiosBinding in _dcsBiosBindings)
            {
                if (dcsBiosBinding.FIPButton == fipPanelButton)
                {
                    dcsBiosBinding.DCSBIOSInputs.Clear();
                }
            }
            Common.DebugP("FIPPanel _keyBindings : " + _keyBindings.Count);
            Common.DebugP("FIPPanel _dcsBiosBindings : " + _dcsBiosBindings.Count);
            IsDirtyMethod();
        }

        private void IsDirtyMethod()
        {
            OnSettingsChanged();
            IsDirty = true;
        }


        public void AddOrUpdateSequencedKeyBinding(string information, FIPPanelButtons fipPanelButton, SortedList<int, KeyPressInfo> sortedList, bool whenTurnedOn = true)
        {
            //This must accept lists
            var found = false;
            RemoveFIPPanelSwitchFromList(2, fipPanelButton, whenTurnedOn);
            foreach (var keyBinding in _keyBindings)
            {
                if (keyBinding.FIPButton == fipPanelButton && keyBinding.WhenTurnedOn == whenTurnedOn)
                {
                    if (sortedList.Count == 0)
                    {
                        keyBinding.OSKeyPress = null;
                    }
                    else
                    {
                        keyBinding.OSKeyPress = new OSKeyPress(information, sortedList);
                        keyBinding.WhenTurnedOn = whenTurnedOn;
                    }
                    found = true;
                    break;
                }
            }
            if (!found && sortedList.Count > 0)
            {
                var keyBinding = new KeyBindingFIP();
                keyBinding.FIPButton = fipPanelButton;
                keyBinding.OSKeyPress = new OSKeyPress(information, sortedList);
                keyBinding.WhenTurnedOn = whenTurnedOn;
                _keyBindings.Add(keyBinding);
            }
            IsDirtyMethod();
        }

        private void RemoveFIPPanelSwitchFromList(int list, FIPPanelButtons fipPanelButton, bool whenTurnedOn = true)
        {
            switch (list)
            {
                case 1:
                    {
                        foreach (var keyBinding in _keyBindings)
                        {
                            if (keyBinding.FIPButton == fipPanelButton && keyBinding.WhenTurnedOn == whenTurnedOn)
                            {
                                keyBinding.OSKeyPress = null;
                            }
                            break;
                        }
                        break;
                    }
                case 2:
                    {
                        foreach (var dcsBiosBinding in _dcsBiosBindings)
                        {
                            if (dcsBiosBinding.FIPButton == fipPanelButton && dcsBiosBinding.WhenTurnedOn == whenTurnedOn)
                            {
                                dcsBiosBinding.DCSBIOSInputs.Clear();
                            }
                            break;
                        }
                        break;
                    }
            }
        }

        public string GetKeyPressForLoggingPurposes(FIPPanelButtons fipPanelButton)
        {
            var result = "";
            foreach (var keyBinding in _keyBindings)
            {
                if (keyBinding.OSKeyPress != null && keyBinding.FIPButton == fipPanelButton)
                {
                    result = keyBinding.OSKeyPress.GetNonFunctioningVirtualKeyCodesAsString();
                }
            }
            return result;
        }

        //Big creds Gaax!   http://stackoverflow.com/questions/2352804/how-do-i-prevent-clipping-when-rotating-an-image-in-c
        // Rotates the input image by theta degrees around center.
        public static Bitmap RotateImage(Bitmap bmpSrc, float theta)
        {
            Matrix mRotate = new Matrix();
            mRotate.Translate(bmpSrc.Width / -2, bmpSrc.Height / -2, MatrixOrder.Append);
            mRotate.RotateAt(theta, new System.Drawing.Point(0, 0), MatrixOrder.Append);
            using (GraphicsPath gp = new GraphicsPath())
            {  // transform image points by rotation matrix
                gp.AddPolygon(new System.Drawing.Point[] { new System.Drawing.Point(0, 0), new System.Drawing.Point(bmpSrc.Width, 0), new System.Drawing.Point(0, bmpSrc.Height) });
                gp.Transform(mRotate);
                System.Drawing.PointF[] pts = gp.PathPoints;

                // create destination bitmap sized to contain rotated source image
                Rectangle bbox = BoundingBox(bmpSrc, mRotate);
                Bitmap bmpDest = new Bitmap(bbox.Width, bbox.Height);

                using (Graphics gDest = Graphics.FromImage(bmpDest))
                {  // draw source into dest
                    Matrix mDest = new Matrix();
                    mDest.Translate(bmpDest.Width / 2, bmpDest.Height / 2, MatrixOrder.Append);
                    gDest.Transform = mDest;
                    gDest.DrawImage(bmpSrc, pts);
                    return bmpDest;
                }
            }
        }

        private static Rectangle BoundingBox(Image img, Matrix matrix)
        {
            GraphicsUnit gu = new GraphicsUnit();
            Rectangle rImg = Rectangle.Round(img.GetBounds(ref gu));

            // Transform the four points of the image, to get the resized bounding box.
            System.Drawing.Point topLeft = new System.Drawing.Point(rImg.Left, rImg.Top);
            System.Drawing.Point topRight = new System.Drawing.Point(rImg.Right, rImg.Top);
            System.Drawing.Point bottomRight = new System.Drawing.Point(rImg.Right, rImg.Bottom);
            System.Drawing.Point bottomLeft = new System.Drawing.Point(rImg.Left, rImg.Bottom);
            System.Drawing.Point[] points = new System.Drawing.Point[] { topLeft, topRight, bottomRight, bottomLeft };
            GraphicsPath gp = new GraphicsPath(points,
                                                                new byte[] { (byte)PathPointType.Start, (byte)PathPointType.Line, (byte)PathPointType.Line, (byte)PathPointType.Line });
            gp.Transform(matrix);
            return Rectangle.Round(gp.GetBounds());
        }
        public void AddOrUpdateDCSBIOSBinding(FIPPanelButtons fipPanelButton, List<DCSBIOSInput> dcsbiosInputs, string description, bool whenTurnedOn = true)
        {
            //!!!!!!!
            //If all DCS-BIOS commands has been deleted then provide a empty list, not null object!!!

            //This must accept lists
            var found = false;
            RemoveFIPPanelSwitchFromList(1, fipPanelButton, whenTurnedOn);
            foreach (var dcsBiosBinding in _dcsBiosBindings)
            {
                if (dcsBiosBinding.FIPButton == fipPanelButton && dcsBiosBinding.WhenTurnedOn == whenTurnedOn)
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
                var dcsBiosBinding = new DCSBIOSBindingFIP();
                dcsBiosBinding.FIPButton = fipPanelButton;
                dcsBiosBinding.DCSBIOSInputs = dcsbiosInputs;
                dcsBiosBinding.WhenTurnedOn = whenTurnedOn;
                dcsBiosBinding.Description = description;
                _dcsBiosBindings.Add(dcsBiosBinding);
            }
            IsDirtyMethod();
        }


        /*
        public DirectOutputDevice FIPDisplay
        {
            get { return _fipDisplay; }
            set { _fipDisplay = value; }
        }*/

        public List<uint> PageList
        {
            get { return _pageList; }
            set { _pageList = value; }
        }

        public Bitmap LastBitmapUsed
        {
            get { return _lastBitmapUsed; }
            set { _lastBitmapUsed = value; }
        }

        public uint LastPageUsed
        {
            get { return _lastPageUsed; }
            set { _lastPageUsed = value; }
        }

        public IntPtr DevicePtr
        {
            get { return _devicePtr; }
            set { _devicePtr = value; }
        }


        public DeviceTypes DeviceType
        {
            get
            {
                return _deviceType;
            }
        }

        public bool IsMe(IntPtr device)
        {
            return device == _devicePtr;
        }

        public FIPHandler FIPHandler
        {
            get { return _fipHandler; }
            set { _fipHandler = value; }
        }

        public HashSet<KeyBindingFIP> KeyBindings
        {
            get { return _keyBindings; }
        }

        public HashSet<DCSBIOSBindingFIP> DCSBiosBindings
        {
            get { return _dcsBiosBindings; }
        }
    }

    public enum FIPPanelButtons
    {
        //111-120 are for gauges which are profile dependent (A-10C/MiG-21bis etc)
        //1-30 are for keyboard emulation/DCS-BIOS
        SOFTBUTTON_1 = 111,
        SOFTBUTTON_2 = 112,
        SOFTBUTTON_3 = 113,
        SOFTBUTTON_4 = 114,
        SOFTBUTTON_5 = 115,
        SOFTBUTTON_6 = 116,
        KNOB_LEFT_INC = 117,
        KNOB_LEFT_DEC = 118,
        KNOB_RIGHT_INC = 119,
        KNOB_RIGHT_DEC = 120,
        SOFTBUTTON_1_P1 = 1,
        SOFTBUTTON_2_P1 = 2,
        SOFTBUTTON_3_P1 = 3,
        SOFTBUTTON_4_P1 = 4,
        SOFTBUTTON_5_P1 = 5,
        SOFTBUTTON_6_P1 = 6,
        KNOB_LEFT_INC_P1 = 7,
        KNOB_LEFT_DEC_P1 = 8,
        KNOB_RIGHT_INC_P1 = 9,
        KNOB_RIGHT_DEC_P1 = 10,
        SOFTBUTTON_1_P2 = 11,
        SOFTBUTTON_2_P2 = 12,
        SOFTBUTTON_3_P2 = 13,
        SOFTBUTTON_4_P2 = 14,
        SOFTBUTTON_5_P2 = 15,
        SOFTBUTTON_6_P2 = 16,
        KNOB_LEFT_INC_P2 = 17,
        KNOB_LEFT_DEC_P2 = 18,
        KNOB_RIGHT_INC_P2 = 19,
        KNOB_RIGHT_DEC_P2 = 20,
        SOFTBUTTON_1_P3 = 21,
        SOFTBUTTON_2_P3 = 22,
        SOFTBUTTON_3_P3 = 23,
        SOFTBUTTON_4_P3 = 24,
        SOFTBUTTON_5_P3 = 25,
        SOFTBUTTON_6_P3 = 26,
        KNOB_LEFT_INC_P3 = 27,
        KNOB_LEFT_DEC_P3 = 28,
        KNOB_RIGHT_INC_P3 = 29,
        KNOB_RIGHT_DEC_P3 = 30,
        PAGE_UP = 1024,
        PAGE_DOWN = 2048
    }
}
