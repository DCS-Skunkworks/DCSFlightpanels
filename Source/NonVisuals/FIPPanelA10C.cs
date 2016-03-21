using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Threading;
using DCS_BIOS;
using NonVisuals.Properties;
using Size = System.Drawing.Size;

namespace NonVisuals
{
    public class FIPPanelA10C : FIPPanel
    {
        protected A10FIPPageType _activeFIPPageType = A10FIPPageType.None;
        private AutoResetEvent _autoResetEvent = new AutoResetEvent(false);
        private long _fipUpdateThrottle = DateTime.Now.Ticks;

        //private long _pageHasChanged;
        //private PerformanceTimer _performanceTimerDCSBIOSDataReceived = new PerformanceTimer("DCS-BIOS DATA RECEIVED BEGINNING and END time.");
        //private PerformanceTimer _performanceTimerHSIImageCreation = new PerformanceTimer("A-10C HSI Image creation times BEGINNING and END time.");
        private readonly ClickSpeedDetector _leftKnobClickSpeedDetectorInc = new ClickSpeedDetector(10);
        private readonly ClickSpeedDetector _leftKnobClickSpeedDetectorDec = new ClickSpeedDetector(10);
        private readonly ClickSpeedDetector _rightKnobClickSpeedDetectorInc = new ClickSpeedDetector(10);
        private readonly ClickSpeedDetector _rightKnobClickSpeedDetectorDec = new ClickSpeedDetector(10);

        /*
         *  A-10C HSI
         */
        private ImageAttributes _greyishTransparency;
        private ImageAttributes _blackTransparency;
        private readonly Bitmap _hsiBackgroundPowerOnImage = new Bitmap(Resources.A10C_HSI_background_power_on);
        private readonly Bitmap _hsiBackgroundPowerOffImage = new Bitmap(Resources.A10C_HSI_background_power_off);
        private readonly Bitmap _hsiCompassCardImage = new Bitmap(Resources.A10C_HSI_compass_card3);
        private readonly Bitmap _hsiPlaneSymbolImage = new Bitmap(Resources.A10C_HSI_plane_symbol);
        private readonly Bitmap _hsiHeadingMarkerImage = new Bitmap(Resources.A10C_HSI_heading_marker);
        private readonly Bitmap _hsiSetCourseNeedleToImage = new Bitmap(Resources.A10C_HSI_course_arrow_to_station_01);
        private readonly Bitmap _hsiSetCourseNeedleFromImage = new Bitmap(Resources.A10C_HSI_course_arrow_from_station_01);
        private readonly Bitmap _hsiSetCourseNeedleToOffCourseImage = new Bitmap(Resources.A10C_HSI_course_arrow_to_station_off_course);
        private readonly Bitmap _hsiSetCourseNeedleFromOffCourseImage = new Bitmap(Resources.A10C_HSI_course_arrow_from_station_off_course);
        private readonly Bitmap _hsiSetCourseNeedleBlankImage = new Bitmap(Resources.A10C_HSI_course_arrow_blank);
        private readonly Bitmap _hsiCourseDeviationLineImage = new Bitmap(Resources.A10C_HSI_deviation_line_3);
        private readonly Bitmap _hsiIsOffCourseFlagImage = new Bitmap(Resources.A10C_HSI_off_course_flag);
        private readonly Bitmap _hsiBearingOneArrowImage = new Bitmap(Resources.A10C_HSI_number_1_arrow);
        private readonly Bitmap _hsiBearingTwoArrowImage = new Bitmap(Resources.A10C_HSI_number_2_arrow);

        private Size _finalHSIImageSize;
        private DCSBIOSOutput _hsiHeadingOutput;
        private DCSBIOSOutput _hsiCourseOutput;
        private DCSBIOSOutput _hsiHeadingBugOutput;
        private DCSBIOSOutput _hsiPoweroffFlagOutput;
        private DCSBIOSOutput _hsiPoweroffRangeFlagOutput;
        private DCSBIOSOutput _hsiRangeDigitAOutput;
        private DCSBIOSOutput _hsiRangeDigitBOutput;
        private DCSBIOSOutput _hsiRangeDigitCOutput;
        private DCSBIOSOutput _hsiRangeDigitDOutput;
        private DCSBIOSOutput _hsiDeviationOutput;
        private DCSBIOSOutput _hsiBearinFlagOutput;
        private DCSBIOSOutput _hsiFlyingTowardsStationOutput;
        private DCSBIOSOutput _hsiFlyingFromStationOutput;
        private DCSBIOSOutput _hsiBearing1Output;
        private DCSBIOSOutput _hsiBearing2Output;
        private readonly A10C_HSI_DataHolderClass _a10CHsiDataHolderClass = new A10C_HSI_DataHolderClass(0, 0, 0, true, 0, true, 0, 0, 0, 0, true, false, false, 0, 0);
        //private readonly object _a10CHsiDataHolderClassLockObject = new object();



        /*
        *  A-10C VVI
        */
        private DCSBIOSOutput _vviValueOutput;
        private readonly A10C_VVI_DataHolderClass _a10CVviDataHolderClass = new A10C_VVI_DataHolderClass(0);
        //private readonly object _a10CVviDataHolderClassLockObject = new object();
        private readonly Bitmap _vviBackgroundImage = new Bitmap(Resources.A_10C_VVI_background);
        private readonly Bitmap _vviNeedleImage = new Bitmap(Resources.A_10C_VVI_needle02);


        public FIPPanelA10C(IntPtr devicePtr, FIPHandler fipHandler)
            : base(devicePtr, fipHandler)
        {
        }

        public void Initalize()
        {
            base.InitalizeBase();
            //Here should be determined what pages should be added. Supported airframe? Start page?
            //var piccy = new Bitmap(Properties.Resources.a10c_splash);
            //FIPDisplay.SetImage(FIPDisplay.Pages[0], piccy);

            PageCallbackDelegate = new DirectOutputClass.PageCallback(PageCallback);
            SoftButtonCallbackDelegate = new DirectOutputClass.SoftButtonCallback(SoftButtonCallback);

            /* HSI */
            _hsiHeadingOutput = DCSBIOSControlLocator.GetDCSBIOSOutput("HSI_HDG"); //0-65536  aircraft heading
            _hsiCourseOutput = DCSBIOSControlLocator.GetDCSBIOSOutput("HSI_CRS"); //0-65536  knob to right side
            _hsiHeadingBugOutput = DCSBIOSControlLocator.GetDCSBIOSOutput("HSI_HDG_BUG"); //0-65536 knob to left side
            _hsiPoweroffFlagOutput = DCSBIOSControlLocator.GetDCSBIOSOutput("HSI_PWROFF_FLAG"); //0 on, 65536 off
            _hsiPoweroffRangeFlagOutput = DCSBIOSControlLocator.GetDCSBIOSOutput("HSI_RANGE_FLAG"); //0 on, 65536 off
            _hsiRangeDigitAOutput = DCSBIOSControlLocator.GetDCSBIOSOutput("HSI_RC_A");
            _hsiRangeDigitBOutput = DCSBIOSControlLocator.GetDCSBIOSOutput("HSI_RC_B");
            _hsiRangeDigitCOutput = DCSBIOSControlLocator.GetDCSBIOSOutput("HSI_RC_C");
            _hsiRangeDigitDOutput = DCSBIOSControlLocator.GetDCSBIOSOutput("HSI_RC_D");
            _hsiDeviationOutput = DCSBIOSControlLocator.GetDCSBIOSOutput("HSI_DEVIATION");
            _hsiBearinFlagOutput = DCSBIOSControlLocator.GetDCSBIOSOutput("HSI_BEARING_FLAG");
            _hsiFlyingTowardsStationOutput = DCSBIOSControlLocator.GetDCSBIOSOutput("HSI_TOFROM1");
            _hsiFlyingFromStationOutput = DCSBIOSControlLocator.GetDCSBIOSOutput("HSI_TOFROM2");
            _hsiBearing1Output = DCSBIOSControlLocator.GetDCSBIOSOutput("HSI_BEARING1");
            _hsiBearing2Output = DCSBIOSControlLocator.GetDCSBIOSOutput("HSI_BEARING2");
            _finalHSIImageSize = _hsiBackgroundPowerOnImage.Size;

            /* VVI */
            _vviValueOutput = DCSBIOSControlLocator.GetDCSBIOSOutput("VVI");


            _greyishTransparency = new ImageAttributes();
            _greyishTransparency.SetColorKey(Color.FromArgb(55, 55, 55), Color.FromArgb(55, 55, 55));
            _blackTransparency = new ImageAttributes();
            _blackTransparency.SetColorKey(Color.Black, Color.Black);
            // = DCSBIOSControlLocator.GetDCSBIOSOutput("");
            // = DCSBIOSControlLocator.GetDCSBIOSOutput("");
            // = DCSBIOSControlLocator.GetDCSBIOSOutput("");


            var returnValues1 = DirectOutputClass.RegisterPageCallback(DevicePtr, PageCallbackDelegate);
            if (returnValues1 != ReturnValues.S_OK)
            {
                Common.LogError(3415412, returnValues1.ToString());
            }
            var returnValues2 = DirectOutputClass.RegisterSoftButtonCallback(DevicePtr, SoftButtonCallbackDelegate);
            if (returnValues2 != ReturnValues.S_OK)
            {
                Common.LogError(3415413, returnValues1.ToString());
            }

            AddPage(1, true);
            AddPage(2, false);
            //ReceivedDcsBiosDataThread = new Thread(HandleDcsBiosDataThreaded);
            //ReceivedDcsBiosDataThread.Start();
        }

        /*
        private void HandleDcsBiosDataThreaded()
        {
            try
            {
                while (!Closed)
                {
                    lock (DcsBiosDataReceivedLock)
                    {
                        if (ReceivedDcsBiosData.Count > 0)
                        {
                            var kvp = ReceivedDcsBiosData[0];
                            var address = kvp.Key;
                            var data = kvp.Value;
                            ReceivedDcsBiosData.RemoveAt(0);
                            DcsBiosDataReceivedThreaded(address, data);
                        }
                        else
                        {
                            Thread.Slep(Common.ThreadSleepValue);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Common.LogError(93232312, ex);
                throw;
            }
        }

        public override void DcsBiosDataReceived(uint address, uint data)
        {
            //lock (DcsBiosDataReceivedLock)
            //{
                ReceivedDcsBiosData.Add(new KeyValuePair<uint, uint>(address, data));
            
                Debug.Print(DevicePtr + " ReceivedDcsBiosData " + ReceivedDcsBiosData.Count);
            //}
        }
        */

        public override void DcsBiosDataReceived(uint address, uint data)
        {
            //_performanceTimerDCSBIOSDataReceived.ClickStart();

            /*
            HSI
            */
            if (_activeFIPPageType == A10FIPPageType.HSI)
            {
                if (address == _hsiHeadingOutput.Address)
                {
                    //This sets all other all other HSI values. Used as fix point
                    //lock (_a10CHsiDataHolderClassLockObject)
                    //{
                    var raw = 360 - (Convert.ToInt32(_hsiHeadingOutput.GetUIntValue(data)) * 360) / 65535;
                    if (raw == 360)
                    {
                        raw = 0;
                    }
                    _a10CHsiDataHolderClass.HsiHeading = raw;
                    //Debug.Print("Device " + DevicePtr + " received _hsiHeadingOutput " + raw);
                    //Common.DebugP("heading raw = " + raw + " heading modded = " + _a10CHsiDataHolderClass.HsiHeading);
                    //}
                }
                if (address == _hsiCourseOutput.Address)
                {
                    //lock (_a10CHsiDataHolderClassLockObject)
                    //{
                    var raw = ((Convert.ToInt32(_hsiCourseOutput.GetUIntValue(data)) * 360) / 65535);
                    if (raw == 360)
                    {
                        raw = 0;
                    }
                    _a10CHsiDataHolderClass.HsiCourse = raw;
                    //Common.DebugP("course raw = " + raw + " course modded = " + (_a10CHsiDataHolderClass.HsiCourse + _a10CHsiDataHolderClass.HsiHeading));
                    //}
                }
                if (address == _hsiHeadingBugOutput.Address)
                {
                    //lock (_a10CHsiDataHolderClassLockObject)
                    //{
                    var raw = (Convert.ToInt32(_hsiHeadingBugOutput.GetUIntValue(data)) * 360) / 65535;
                    if (raw == 360)
                    {
                        raw = 0;
                    }
                    _a10CHsiDataHolderClass.HsiHeadingBug = raw;
                    //}
                }
                if (address == _hsiBearing1Output.Address)
                {
                    //lock (_a10CHsiDataHolderClassLockObject)
                    //{
                    var raw = (Convert.ToInt32(_hsiBearing1Output.GetUIntValue(data)) * 360) / 65535;
                    if (raw == 360)
                    {
                        raw = 0;
                    }
                    _a10CHsiDataHolderClass.HsiBearing1 = raw;
                    //}
                }
                if (address == _hsiBearing2Output.Address)
                {
                    //lock (_a10CHsiDataHolderClassLockObject)
                    //{
                    var raw = (Convert.ToInt32(_hsiBearing2Output.GetUIntValue(data)) * 360) / 65535;
                    if (raw == 360)
                    {
                        raw = 0;
                    }
                    _a10CHsiDataHolderClass.HsiBearing2 = raw;
                    //}
                }
                if (address == _hsiPoweroffFlagOutput.Address)
                {
                    //lock (_a10CHsiDataHolderClassLockObject)
                    //{
                    var result = (int)_hsiPoweroffFlagOutput.GetUIntValue(data);
                    _a10CHsiDataHolderClass.HsiPowerOffFlag = result > 100;
                    //}
                }
                if (address == _hsiPoweroffRangeFlagOutput.Address)
                {
                    //lock (_a10CHsiDataHolderClassLockObject)
                    //{
                    var result = (int)_hsiPoweroffRangeFlagOutput.GetUIntValue(data);
                    _a10CHsiDataHolderClass.HsiRangePowerOffFlag = result > 100;
                    //}
                }
                if (address == _hsiRangeDigitAOutput.Address)
                {
                    //lock (_a10CHsiDataHolderClassLockObject)
                    //{
                    var result = (int)_hsiRangeDigitAOutput.GetUIntValue(data);
                    _a10CHsiDataHolderClass.HsiRangeDigitA = result;
                    //}
                }
                if (address == _hsiRangeDigitBOutput.Address)
                {
                    //lock (_a10CHsiDataHolderClassLockObject)
                    //{
                    var result = (int)_hsiRangeDigitBOutput.GetUIntValue(data);
                    _a10CHsiDataHolderClass.HsiRangeDigitB = result;
                    //}
                }
                if (address == _hsiRangeDigitCOutput.Address)
                {
                    //lock (_a10CHsiDataHolderClassLockObject)
                    //{
                    var result = (int)_hsiRangeDigitCOutput.GetUIntValue(data);
                    _a10CHsiDataHolderClass.HsiRangeDigitC = result;
                    //}
                }
                if (address == _hsiRangeDigitDOutput.Address)
                {
                    //lock (_a10CHsiDataHolderClassLockObject)
                    //{
                    var result = (int)_hsiRangeDigitDOutput.GetUIntValue(data);
                    _a10CHsiDataHolderClass.HsiRangeDigitD = result;
                    //}
                }
                if (address == _hsiDeviationOutput.Address)
                {
                    //lock (_a10CHsiDataHolderClassLockObject)
                    //{
                    var result = (int)_hsiDeviationOutput.GetUIntValue(data);
                    _a10CHsiDataHolderClass.HsiCourseDeviation = result;
                    //}
                }
                if (address == _hsiBearinFlagOutput.Address)
                {
                    //lock (_a10CHsiDataHolderClassLockObject)
                    //{
                    var result = (int)_hsiBearinFlagOutput.GetUIntValue(data);
                    _a10CHsiDataHolderClass.HsiIsOffCourse = result > 100;
                    //}
                }
                if (address == _hsiFlyingTowardsStationOutput.Address)
                {
                    //lock (_a10CHsiDataHolderClassLockObject)
                    //{
                    var result = (int)_hsiFlyingTowardsStationOutput.GetUIntValue(data);
                    _a10CHsiDataHolderClass.HsiFlyingTowardsStation = result > 100;
                    //}
                }
                if (address == _hsiFlyingFromStationOutput.Address)
                {
                    //lock (_a10CHsiDataHolderClassLockObject)
                    //{
                    var result = (int)_hsiFlyingFromStationOutput.GetUIntValue(data);
                    _a10CHsiDataHolderClass.HsiFlyingFromStation = result > 100;
                    //}
                }
            }

            if (_activeFIPPageType == A10FIPPageType.VVI)
            {
                if (address == _vviValueOutput.Address)
                {
                    //lock (_a10CVviDataHolderClassLockObject)
                    //{
                    var result = (int)_vviValueOutput.GetUIntValue(data);
                    _a10CVviDataHolderClass.VVIValue = Convert.ToInt32(result);
                    //Common.DebugP("VVIValue raw = " + result + " VVIValue modded = " + _a10CVviDataHolderClass.VVIValue);
                    //}
                }
            }

            if ((_a10CHsiDataHolderClass.DataHasChanged || _a10CVviDataHolderClass.DataHasChanged) && DateTime.Now.Ticks - _fipUpdateThrottle > 100000)//10ms
            {
                _fipUpdateThrottle = DateTime.Now.Ticks;
                _autoResetEvent.Set();
            }
            //_performanceTimerDCSBIOSDataReceived.ClickEnd();
        }


        protected override void PageCallback(IntPtr device, IntPtr page, byte bActivated, IntPtr context)
        {
            try
            {
                //This must be airframe specific
                Debug.Print(string.Concat(new object[6]
                {
                "PageCallback device: 0x",
                DevicePtr.ToString("x"),
                " Page: ",
                page,
                " IsActivated: ",
                bActivated
                }));
                if (bActivated == 0) // || _lastBitmapUsed == null)
                {
                    //This page isn't active or the bitmap isn't used anymore, exit..
                    //Set no page as active in case user chose Saitek ad loop.
                    SetActivePage(A10FIPPageType.None);
                    return;
                }
                LastPageUsed = (uint)page;
                switch (page.ToInt64())
                {
                    case 0:
                        {
                            //nada
                            SetActivePage(A10FIPPageType.None);
                            break;
                        }
                    case 1:
                        {
                            SetActivePage(A10FIPPageType.HSI);
                            SetImage(1, new Bitmap(Resources.A10C_HSI_background_power_off));
                            break;
                        }
                    case 2:
                        {
                            SetActivePage(A10FIPPageType.VVI);
                            SetImage(2, new Bitmap(Resources.A_10C_VVI_background_02_no_data_copy));
                            break;
                        }
                    case 3:
                        {
                            SetActivePage(A10FIPPageType.Airspeed);
                            break;
                        }
                    case 4:
                        {
                            SetActivePage(A10FIPPageType.TurnIndicator);
                            break;
                        }
                    case 5:
                        {
                            SetActivePage(A10FIPPageType.Tachometer);
                            break;
                        }
                    case 6:
                        {
                            SetActivePage(A10FIPPageType.Custom);
                            break;
                        }
                }
                /*if (UseFileImageListForPages)
                {
                    foreach (var keyValuePair in ImageFromFileList)
                    {
                        if (keyValuePair.Key == (uint)page)
                        {
                            var num = (int)SetImageFromFile((uint)page, keyValuePair.Value);
                            break;
                        }
                    }
                }*/
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(10217, ex);
            }
        }

        public void SetActivePage(A10FIPPageType activeFIPPageType)
        {
            _activeFIPPageType = activeFIPPageType;
            //Interlocked.Exchange(ref _pageHasChanged, 1);
        }

        public override void ThreadedImageGenerator()
        {
            while (!DoShutdown)
            {
                try
                {
                    /*if (Interlocked.Read(ref _pageHasChanged) == 1)
                    {
                        /*
                         * do what?
                         *
                         * unsubscribe not used dcs-bios address but how? others might need it, how do i know that i can shut it down?
                         * reset lists which are mine, does not affect others
                         * 
                         *
                        //ResetCockpitDataLists();
                        Interlocked.Exchange(ref _pageHasChanged, 0);
                    }*/
                    switch (_activeFIPPageType)
                    {
                        case A10FIPPageType.None:
                            {
                                //Do nada
                                break;
                            }
                        case A10FIPPageType.HSI:
                            {
                                if (_a10CHsiDataHolderClass.DataHasChanged)
                                {
                                    var bitmap = DrawSequenceHSI();
                                    SetImage((uint)A10FIPPageType.HSI, bitmap);
                                    _a10CHsiDataHolderClass.Reset();
                                }
                                break;
                            }
                        case A10FIPPageType.VVI:
                            {
                                if (_a10CVviDataHolderClass.DataHasChanged)
                                {
                                    var bitmap = DrawSequenceVVI();
                                    SetImage((uint)A10FIPPageType.VVI, bitmap);
                                    _a10CVviDataHolderClass.Reset();
                                }
                                break;
                            }
                    }
                    _autoResetEvent.WaitOne();
                }
                catch (Exception ex)
                {
                    Common.LogError(34344343, ex, "ThreadedImageGenerator, _activeFIPPageType = " + _activeFIPPageType);
                }
            }
        }

        public override void Shutdown()
        {
            try
            {
                //_performanceTimerDCSBIOSDataReceived.PrintToFile(@"e:\temp\dcs_bios_received.txt");
                //_performanceTimerHSIImageCreation.PrintToFile(@"e:\temp\hsi_image_creation.txt");
                base.Shutdown();
                _autoResetEvent.Set();
            }
            catch (Exception e)
            {
                SetLastException(e);
            }
        }

        private Bitmap DrawSequenceVVI()
        {
            Bitmap finalVVIImage = null;
            try
            {
                var angle = Convert.ToInt32((_a10CVviDataHolderClass.VVIValue - 32590.27206) / 191.6014706);
                //Common.DebugOn = true;
                //Common.DebugP("Angle = " + angle.ToString() + " DCS-BIOS = " + _a10CVviDataHolderClass.VVIValue);
                finalVVIImage = new Bitmap(_vviBackgroundImage.Width, _vviBackgroundImage.Height);

                using (var graphics = Graphics.FromImage(finalVVIImage))
                {
                    graphics.FillRectangle(Brushes.Black, 0, 0, finalVVIImage.Width, finalVVIImage.Height);
                    graphics.DrawImage(_vviBackgroundImage, 0, 0, _vviBackgroundImage.Width, _vviBackgroundImage.Height);
                    graphics.TranslateTransform(_vviBackgroundImage.Width / 2f, _vviBackgroundImage.Height / 2f);
                    graphics.RotateTransform(angle);
                    graphics.TranslateTransform(_vviNeedleImage.Width / -2f, _vviNeedleImage.Height / -2f);
                    var vviNeedleImageRectangle = new Rectangle(0, 0, _vviNeedleImage.Width, _vviNeedleImage.Height);
                    graphics.DrawImage(_vviNeedleImage, vviNeedleImageRectangle, 0, 0, _vviNeedleImage.Width, _vviNeedleImage.Height, GraphicsUnit.Pixel, _blackTransparency);
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(101337, ex);
            }
            return finalVVIImage;
        }


        private Bitmap DrawSequenceHSI()
        {
            Bitmap finalHSIImage = null;
            try
            {
                //Debug.Print(DevicePtr + " DrawSequenceHSI()");
                //_performanceTimerHSIImageCreation.ClickStart();
                var interpolation = InterpolationMode.Default;
                var correctCurrentHeading = _a10CHsiDataHolderClass.HsiHeading * -1; // no correction needed

                finalHSIImage = new Bitmap(_finalHSIImageSize.Width, _finalHSIImageSize.Height);

                Bitmap hsiBackPlateImage;
                if (_a10CHsiDataHolderClass.HsiPowerOffFlag)
                {
                    hsiBackPlateImage = _hsiBackgroundPowerOffImage;
                }
                else
                {
                    hsiBackPlateImage = _hsiBackgroundPowerOnImage;
                }
                using (var graphics = Graphics.FromImage(finalHSIImage))
                {
                    graphics.InterpolationMode = interpolation;
                    graphics.FillRectangle(Brushes.Black, 0, 0, finalHSIImage.Width, finalHSIImage.Height);
                    graphics.DrawImage(hsiBackPlateImage, 0, 0, hsiBackPlateImage.Width, hsiBackPlateImage.Height);
                }
                //Backplate added
                /************************************************************************************
                 ************************************************************************************ 
                 ************************************************************************************/


                var hsiHeadingMarkerImageTmp = RotateImage(_hsiHeadingMarkerImage, _a10CHsiDataHolderClass.HsiHeadingBug);
                using (var graphics = Graphics.FromImage(finalHSIImage))
                {
                    graphics.InterpolationMode = interpolation;
                    graphics.TranslateTransform(finalHSIImage.Width / 2f, finalHSIImage.Height / 2f + 2);
                    var x = 0f;
                    var y = 0f;
                    if (_a10CHsiDataHolderClass.HsiHeadingBug >= 0 || _a10CHsiDataHolderClass.HsiHeadingBug < 90)
                    {
                        x = Convert.ToSingle(Math.Sin(Math.PI / 180 * _a10CHsiDataHolderClass.HsiHeadingBug) * 95);
                        y = Convert.ToSingle(Math.Cos(Math.PI / 180 * _a10CHsiDataHolderClass.HsiHeadingBug) * 95) * -1;
                    }
                    graphics.TranslateTransform(x - hsiHeadingMarkerImageTmp.Width / 2f, y - hsiHeadingMarkerImageTmp.Height / 2f);
                    var hsiHeadingMarkerImageRectangle = new Rectangle(0, 0, hsiHeadingMarkerImageTmp.Width, hsiHeadingMarkerImageTmp.Height);
                    graphics.DrawImage(hsiHeadingMarkerImageTmp, hsiHeadingMarkerImageRectangle, 0, 0, hsiHeadingMarkerImageTmp.Width, hsiHeadingMarkerImageTmp.Height, GraphicsUnit.Pixel, _blackTransparency);
                }
                //Heading marker etched at correct rotation and location
                /************************************************************************************
                 ************************************************************************************ 
                 ************************************************************************************/

                using (var graphics = Graphics.FromImage(finalHSIImage))
                {
                    graphics.TranslateTransform(hsiBackPlateImage.Width / 2f, hsiBackPlateImage.Height / 2f + 3);
                    graphics.RotateTransform(_a10CHsiDataHolderClass.HsiBearing1);
                    graphics.TranslateTransform(_hsiBearingOneArrowImage.Width / -2f, _hsiBearingOneArrowImage.Height / -2f);
                    var hsiBearingOneArrowImageRectangle = new Rectangle(0, 0, _hsiBearingOneArrowImage.Width, _hsiBearingOneArrowImage.Height);
                    graphics.DrawImage(_hsiBearingOneArrowImage, hsiBearingOneArrowImageRectangle, 0, 0, _hsiBearingOneArrowImage.Width, _hsiBearingOneArrowImage.Height, GraphicsUnit.Pixel, _blackTransparency);
                }
                //Bearing 1 marker etched

                using (var graphics = Graphics.FromImage(finalHSIImage))
                {
                    graphics.TranslateTransform(hsiBackPlateImage.Width / 2f, hsiBackPlateImage.Height / 2f + 3);
                    graphics.RotateTransform(_a10CHsiDataHolderClass.HsiBearing2);
                    graphics.TranslateTransform(_hsiBearingTwoArrowImage.Width / -2f, _hsiBearingTwoArrowImage.Height / -2f);
                    var _hsiBearingTwoArrowImagewImageRectangle = new Rectangle(0, 0, _hsiBearingTwoArrowImage.Width, _hsiBearingTwoArrowImage.Height);
                    graphics.DrawImage(_hsiBearingTwoArrowImage, _hsiBearingTwoArrowImagewImageRectangle, 0, 0, _hsiBearingTwoArrowImage.Width, _hsiBearingTwoArrowImage.Height, GraphicsUnit.Pixel, _blackTransparency);
                }
                //Bearing 1 marker etched

                using (var graphics = Graphics.FromImage(finalHSIImage))
                {
                    graphics.InterpolationMode = interpolation;
                    graphics.TranslateTransform(hsiBackPlateImage.Width / 2f, hsiBackPlateImage.Height / 2f + 3);
                    graphics.RotateTransform(correctCurrentHeading);
                    graphics.TranslateTransform(_hsiCompassCardImage.Width / -2f, _hsiCompassCardImage.Height / -2f);
                    var hsiCompassCardImageRectangle = new Rectangle(0, 0, _hsiCompassCardImage.Width, _hsiCompassCardImage.Height);
                    graphics.DrawImage(_hsiCompassCardImage, hsiCompassCardImageRectangle, 0, 0, _hsiCompassCardImage.Width, _hsiCompassCardImage.Height, GraphicsUnit.Pixel, _greyishTransparency);
                }
                //Compass card etched at correct rotation
                /************************************************************************************
                 ************************************************************************************ 
                 ************************************************************************************/


                /*
               if (_a10CHsiDataHolderClass.HsiIsOffCourse)
               {
                   using (var graphics = Graphics.FromImage(finalHSIImage))
                   {
                       graphics.InterpolationMode = interpolation;
                       var offset = 40;
                       var x = 0f;
                       var y = 0f;
                       x = Convert.ToSingle(Math.Sin((Math.PI / 180) * (180 - _a10CHsiDataHolderClass.HsiCourse)) * offset);
                       y = Convert.ToSingle(Math.Cos((Math.PI / 180) * (180 - _a10CHsiDataHolderClass.HsiCourse)) * offset);
                       graphics.TranslateTransform(hsiBackPlateImage.Width / 2f + x, hsiBackPlateImage.Height / 2f + y);
                       graphics.RotateTransform(_a10CHsiDataHolderClass.HsiCourse);
                       var hsiOffCourseFlagImageRectangle = new Rectangle(0, 0, _hsiIsOffCourseFlagImage.Width, _hsiIsOffCourseFlagImage.Height);
                       graphics.DrawImage(_hsiIsOffCourseFlagImage, hsiOffCourseFlagImageRectangle, 0, 0, _hsiIsOffCourseFlagImage.Width, _hsiIsOffCourseFlagImage.Height, GraphicsUnit.Pixel, _blackTransparency);
                       graphics.TranslateTransform(hsiBackPlateImage.Width / -2f - x, hsiBackPlateImage.Height / -2f - y);
                   }
               }*/
                //Off course warning flag
                /************************************************************************************
                 ************************************************************************************ 
                 ************************************************************************************/

                using (var graphics = Graphics.FromImage(finalHSIImage))
                {
                    graphics.InterpolationMode = interpolation;
                    var hsiPlaneSymbolImageRectangle = new Rectangle(0, 0, _hsiPlaneSymbolImage.Width, _hsiPlaneSymbolImage.Height);
                    graphics.TranslateTransform(finalHSIImage.Width / 2f - _hsiPlaneSymbolImage.Width / 2f, finalHSIImage.Height / 2f - _hsiPlaneSymbolImage.Height / 2f);
                    graphics.DrawImage(_hsiPlaneSymbolImage, hsiPlaneSymbolImageRectangle, 0, 0, _hsiPlaneSymbolImage.Width, _hsiPlaneSymbolImage.Height, GraphicsUnit.Pixel, _blackTransparency);
                }
                //Airplane lines etched on compass card
                /************************************************************************************
                 ************************************************************************************ 
                 ************************************************************************************/

                using (var graphics = Graphics.FromImage(finalHSIImage))
                {
                    graphics.InterpolationMode = interpolation;
                    var solidBrush = new SolidBrush(Color.White);
                    var point5 = new PointF(255.0f, 50);
                    var font5 = new Font(_a10CHsiDataHolderClass.GetHsiCourseAbsoluteString(), 12f, System.Drawing.FontStyle.Bold, GraphicsUnit.Pixel);
                    graphics.TextRenderingHint = TextRenderingHint.AntiAlias;
                    graphics.DrawString(_a10CHsiDataHolderClass.GetHsiCourseAbsoluteString(), font5, solidBrush, point5);
                }
                //Set course number etched
                /************************************************************************************
                 ************************************************************************************ 
                 ************************************************************************************/

                if (!_a10CHsiDataHolderClass.HsiRangePowerOffFlag)
                {
                    using (var graphics = Graphics.FromImage(finalHSIImage))
                    {
                        graphics.InterpolationMode = interpolation;
                        var tmpRange = _a10CHsiDataHolderClass.HsiRangeDigits.ToString().PadLeft(4, ' ');
                        var solidBrush = new SolidBrush(Color.White);
                        var point5 = new PointF(42.0f, 50);
                        var font5 = new Font(tmpRange, 12f, System.Drawing.FontStyle.Bold, GraphicsUnit.Pixel);

                        graphics.TextRenderingHint = TextRenderingHint.AntiAlias;
                        graphics.DrawString(tmpRange, font5, solidBrush, point5);
                    }
                }
                //Range number etched
                /************************************************************************************
                 ************************************************************************************ 
                 ************************************************************************************/

                var hsiCourseNeedleImage = _hsiSetCourseNeedleBlankImage;
                if (_a10CHsiDataHolderClass.HsiIsOffCourse)
                {
                    //We are off course
                    if (_a10CHsiDataHolderClass.HsiFlyingTowardsStation)
                    {
                        hsiCourseNeedleImage = _hsiSetCourseNeedleToOffCourseImage;
                    }
                    else if (_a10CHsiDataHolderClass.HsiFlyingFromStation)
                    {
                        hsiCourseNeedleImage = _hsiSetCourseNeedleFromOffCourseImage;
                    }
                }
                else if (_a10CHsiDataHolderClass.HsiFlyingTowardsStation)
                {
                    hsiCourseNeedleImage = _hsiSetCourseNeedleToImage;
                }
                else if (_a10CHsiDataHolderClass.HsiFlyingFromStation)
                {
                    hsiCourseNeedleImage = _hsiSetCourseNeedleFromImage;
                }
                else
                {
                    hsiCourseNeedleImage = _hsiSetCourseNeedleBlankImage;
                }

                using (var graphics = Graphics.FromImage(finalHSIImage))
                {
                    graphics.InterpolationMode = interpolation;
                    graphics.TranslateTransform(finalHSIImage.Width / 2f, finalHSIImage.Height / 2f);
                    graphics.RotateTransform(_a10CHsiDataHolderClass.HsiCourse);
                    graphics.TranslateTransform(hsiCourseNeedleImage.Width / -2f, hsiCourseNeedleImage.Height / -2f);
                    var hsiPlaneSymbolImageRectangle = new Rectangle(0, 0, hsiCourseNeedleImage.Width, hsiCourseNeedleImage.Height);
                    graphics.DrawImage(hsiCourseNeedleImage, hsiPlaneSymbolImageRectangle, 0, 0, hsiCourseNeedleImage.Width, hsiCourseNeedleImage.Height, GraphicsUnit.Pixel, _blackTransparency);
                }
                //Set course needle etched
                /************************************************************************************
                 ************************************************************************************ 
                 ************************************************************************************/

                var hsiCourseDeviationLineImage = RotateImage(_hsiCourseDeviationLineImage, _a10CHsiDataHolderClass.HsiCourse);
                using (var graphics = Graphics.FromImage(finalHSIImage))
                {
                    graphics.InterpolationMode = interpolation;
                    var x = 0f;
                    var y = 0f;
                    //positive offset right
                    //negative offset left
                    // +/- 21 first dot
                    // +/- 42 first dot
                    var offset = -41 + (82 * _a10CHsiDataHolderClass.HsiCourseDeviation / 65532);
                    x = Convert.ToSingle(Math.Sin((Math.PI / 180) * (90 - _a10CHsiDataHolderClass.HsiCourse))) * offset;
                    y = Convert.ToSingle(Math.Cos((Math.PI / 180) * (90 - _a10CHsiDataHolderClass.HsiCourse))) * offset;
                    graphics.TranslateTransform(finalHSIImage.Width / 2f + x, finalHSIImage.Height / 2f + y);
                    graphics.TranslateTransform(hsiCourseDeviationLineImage.Width / -2f, hsiCourseDeviationLineImage.Height / -2f);
                    var hsiDeviationLineImageRectangle = new Rectangle(0, 0, hsiCourseDeviationLineImage.Width, hsiCourseDeviationLineImage.Height);
                    graphics.DrawImage(hsiCourseDeviationLineImage, hsiDeviationLineImageRectangle, 0, 0, hsiCourseDeviationLineImage.Width, hsiCourseDeviationLineImage.Height, GraphicsUnit.Pixel, _blackTransparency);
                }
                //Deviation line etched
                /************************************************************************************
                 ************************************************************************************ 
                 ************************************************************************************/

                //ImageTestHSI1.Source = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(stage1Image.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromWidthAndHeight(stage1Size.Width, stage1Size.Height));
                //_fipDisplay.SetImage(_fipDisplay.Pages[0], stage1Image);
                //ImageTestHSI2.Source = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(stage2Image.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromWidthAndHeight(stage1Size.Width, stage1Size.Height));
                //_fipDisplay.SetImage(_fipDisplay.Pages[1], stage2Image);
                /*
                //var headingCardImageWithNeedle = BitmapSource2Bitmap(headingCardImageWithNeedleImageSource);
                using (var graphics = Graphics.FromImage(stage2Image))
                {
                    graphics.Clear(Color.Black);
                    //graphics.FillRectangle(Brushes.Black, 0, 0, stage2Size.Width, stage2Size.Height);
                    //graphics.TranslateTransform(finalImage.Width / 2f, finalImage.Height / 2f);
                    graphics.TranslateTransform(200, 120);
                    graphics.RotateTransform(correctHeading);
                    graphics.TranslateTransform(-200, -120);
                    graphics.DrawImage(stage1Image, 80, 0, 240, 240);
                    //graphics.DrawImage(leftHandImage, 0, 0, leftHandImage.Width, leftHandImage.Height);
                    //graphics.TranslateTransform(-120, 0);
                }
                ImageTestADF2.Source = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(stage2Image.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromWidthAndHeight(stage2Size.Width, stage2Size.Height));


                using (var graphics = Graphics.FromImage(stage2Image))
                {
                    var distance = 50f;
                    var correction = 5f;
                    var text1 = "Rad 1";
                    var text2 = "Rad 2";
                    var text3 = "Rad 3";
                    var text4 = "Rad 4";
                    var text5 = "Rad 5";
                    var solidBrush = new SolidBrush(Color.White);

                    var point1 = new PointF(0.0f, 5.0f);
                    var font1 = new Font(text1, 15f);
                    graphics.DrawString(text1, font1, solidBrush, point1);

                    var point2 = new PointF(0.0f, distance + correction);
                    var font2 = new Font(text1, 15f);
                    graphics.DrawString(text2, font2, solidBrush, point2);

                    var point3 = new PointF(0.0f, 2 * distance + correction);
                    var font3 = new Font(text3, 15f);
                    graphics.DrawString(text3, font3, solidBrush, point3);

                    var point4 = new PointF(0.0f, 3 * distance + correction);
                    var font4 = new Font(text3, 15f);
                    graphics.DrawString(text4, font4, solidBrush, point4);

                    var point5 = new PointF(0.0f, 4 * distance + correction);
                    var font5 = new Font(text3, 15f);
                    graphics.DrawString(text5, font5, solidBrush, point5);


                    var headingBugImageRectangle = new Rectangle(0, 0, headingBugImage.Width, headingBugImage.Height);
                    graphics.TranslateTransform(191, 0);
                    graphics.DrawImage(headingBugImage, headingBugImageRectangle, 0, 0, headingBugImage.Width, headingBugImage.Height, GraphicsUnit.Pixel, attr);
                }
                ImageTestADF3.Source = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(stage2Image.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromWidthAndHeight(stage2Size.Width, stage2Size.Height));
                */
                //_performanceTimerHSIImageCreation.ClickEnd();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(10198337, ex);
            }
            return finalHSIImage;
        }

        protected override void SoftButtonCallback(IntPtr device, IntPtr buttons, IntPtr context)
        {
            try
            {

                //This must be airframe specific
                /*
                    Side buttons from top to bottom:
                    0x20
                    0x40
                    0x80
                    0x100
                    0x200
                    0x400

                    Left knob:
                    counterclockwise 0x10
                    clockwise 0x8

                    Right knob:
                    counterclockwise 0x4
                    clockwise 0x2
             */

                var minChange = 700;
                var maxChange = 6000;
                if (device == DevicePtr)
                {
                    /*
                     * LEFT                 
                     */
                    var value = buttons.ToInt32();

                    if ((value & 0x10) > 0)
                    {
                        //Heading Set (-)
                        _leftKnobClickSpeedDetectorDec.Click();
                        if (_leftKnobClickSpeedDetectorDec.ClickThresholdReached())
                        {
                            DCSBIOS.Send("HSI_HDG_KNOB -" + maxChange + "\n");
                        }
                        DCSBIOS.Send("HSI_HDG_KNOB -" + minChange + "\n");
                    }
                    if ((value & 0x8) > 0)
                    {
                        //Heading Set (+)
                        _leftKnobClickSpeedDetectorInc.Click();
                        if (_leftKnobClickSpeedDetectorInc.ClickThresholdReached())
                        {
                            DCSBIOS.Send("HSI_HDG_KNOB +" + maxChange + "\n");
                        }
                        DCSBIOS.Send("HSI_HDG_KNOB +" + minChange + "\n");
                    }


                    /*
                     * RIGHT
                     */
                    if ((value & 0x4) > 0)
                    {
                        //Course Set (-)
                        _rightKnobClickSpeedDetectorDec.Click();
                        if (_rightKnobClickSpeedDetectorDec.ClickThresholdReached())
                        {
                            DCSBIOS.Send("HSI_CRS_KNOB -" + maxChange + "\n");
                        }
                        DCSBIOS.Send("HSI_CRS_KNOB -" + minChange + "\n");
                    }
                    if ((value & 0x2) > 0)
                    {
                        //Course Set (+)
                        _rightKnobClickSpeedDetectorInc.Click();
                        if (_rightKnobClickSpeedDetectorInc.ClickThresholdReached())
                        {
                            DCSBIOS.Send("HSI_CRS_KNOB +" + maxChange + "\n");
                        }
                        DCSBIOS.Send("HSI_CRS_KNOB +" + minChange + "\n");
                    }


                    Debug.Print("SoftButtonCallback device: 0x" + DevicePtr.ToString("x") + " Buttons: 0x" + buttons.ToString("x"));
                }
            }
            catch (Exception ex)
            {
                Common.LogError(3433343, ex);
            }
        }
    }


    public class A10C_HSI_DataHolderClass
    {
        /*
         * Incorporated in same update cycle.
         * If heading changes, no updated until the two others have changed too
         */
        private int _updateCycle = 0;
        private int _hsiHeading; // 0x4
        private int _hsiCourse; // 0x2
        private int _hsiHeadingBug; // 0x1
                                    /*
                                     * 
                                     */
        private bool _hsiPowerOffFlag;
        private bool _hsiRangePowerOffFlag;
        private int _hsiCourseDeviation;
        private bool _dataHasChanged;
        private int _hsiCourseAbsolute;
        private int _hsiRangeDigitA;
        private int _hsiRangeDigitB;
        private int _hsiRangeDigitC;
        private int _hsiRangeDigitD;
        private bool _hsiIsOffCourse;
        private bool _hsiFlyingTowardsStation;
        private bool _hsiFlyingFromStation;
        private int _hsiBearing1;
        private int _hsiBearing2;

        public A10C_HSI_DataHolderClass(int hsiHeading,
            int hsiCourse,
            int hsiHeadingBug,
            bool hsiPowerOffFlag,
            int hsiCourseDeviation,
            bool hsiRangePowerOffFlag,
            int hsiRangeDigitA,
            int hsiRangeDigitB,
            int hsiRangeDigitC,
            int hsiRangeDigitD,
            bool hsiIsOffCourse,
            bool hsiFlyingTowardsStation,
            bool hsiFlyingFromStation,
            int hsiBearing1,
            int hsiBearing2)
        {
            _hsiHeading = hsiHeading;
            _hsiCourse = hsiCourse;
            _hsiHeadingBug = hsiHeadingBug;
            _hsiPowerOffFlag = hsiPowerOffFlag;
            _hsiCourseDeviation = hsiCourseDeviation;
            _hsiRangePowerOffFlag = hsiRangePowerOffFlag;
            _hsiRangeDigitA = hsiRangeDigitA;
            _hsiRangeDigitB = hsiRangeDigitB;
            _hsiRangeDigitC = hsiRangeDigitC;
            _hsiRangeDigitD = hsiRangeDigitD;
            _hsiIsOffCourse = hsiIsOffCourse;
            _hsiFlyingTowardsStation = hsiFlyingTowardsStation;
            _hsiFlyingFromStation = hsiFlyingFromStation;
            _hsiBearing1 = hsiBearing1;
            _hsiBearing2 = hsiBearing2;
            SetHsiCourseAbsolute();
        }

        private void SetHsiCourseAbsolute()
        {
            _hsiCourseAbsolute = _hsiHeading + _hsiCourse;
            if (_hsiCourseAbsolute >= 360)
            {
                _hsiCourseAbsolute = _hsiCourseAbsolute - 360;
            }
        }

        public String GetHsiCourseAbsoluteString()
        {
            return _hsiCourseAbsolute.ToString().PadLeft(3, '0');
        }

        public bool DataHasChanged
        {
            get
            {
                return _dataHasChanged || _updateCycle == 0x7;
            }
        }

        public void Reset()
        {
            //Debug.Print("Reset()");
            _updateCycle = 0;
            _dataHasChanged = false;
        }

        public int HsiHeading
        {
            get { return _hsiHeading; }
            set
            {
                if (value != _hsiHeading)
                {
                    _hsiHeading = value;
                    _updateCycle = _updateCycle | 0x4;
                }
            }
        }

        public int HsiCourse
        {
            get { return _hsiCourse; }
            set
            {
                if (value != _hsiCourse)
                {
                    _hsiCourse = value;
                    SetHsiCourseAbsolute();
                    _updateCycle = _updateCycle | 0x2;
                }
            }
        }

        public int HsiCourseDeviation
        {
            get { return _hsiCourseDeviation; }
            set
            {
                if (value != _hsiCourseDeviation)
                {
                    _hsiCourseDeviation = value;
                    _dataHasChanged = true;
                }
            }
        }

        public int HsiHeadingBug
        {
            get { return _hsiHeadingBug; }
            set
            {
                if (value != _hsiHeadingBug)
                {
                    _hsiHeadingBug = value;
                    _updateCycle = _updateCycle | 0x1;
                }
            }
        }

        public int HsiBearing1
        {
            get { return _hsiBearing1; }
            set
            {
                if (value != _hsiBearing1)
                {
                    _hsiBearing1 = value;
                    _dataHasChanged = true;
                }
            }
        }

        public int HsiBearing2
        {
            get { return _hsiBearing2; }
            set
            {
                if (value != _hsiBearing2)
                {
                    _hsiBearing2 = value;
                    _dataHasChanged = true;
                }
            }
        }

        public int HsiRangeDigitA
        {
            get { return _hsiRangeDigitA; }
            set
            {
                var tmpValue = HsiRangeDCSBIOSValueToDigit(value);
                if (tmpValue != _hsiRangeDigitA)
                {
                    _hsiRangeDigitA = tmpValue;
                    _dataHasChanged = true;
                }
            }
        }

        public int HsiRangeDigitB
        {
            get { return _hsiRangeDigitB; }
            set
            {
                var tmpValue = HsiRangeDCSBIOSValueToDigit(value);
                if (tmpValue != _hsiRangeDigitB)
                {
                    _hsiRangeDigitB = tmpValue;
                    _dataHasChanged = true;
                }
            }
        }

        public int HsiRangeDigitC
        {
            get { return _hsiRangeDigitC; }
            set
            {
                var tmpValue = HsiRangeDCSBIOSValueToDigit(value);
                if (tmpValue != _hsiRangeDigitC)
                {
                    _hsiRangeDigitC = tmpValue;
                    _dataHasChanged = true;
                }
            }
        }

        public int HsiRangeDigitD
        {
            get { return _hsiRangeDigitD; }
            set
            {
                var tmpValue = HsiRangeDCSBIOSValueToDigit(value);
                if (tmpValue != _hsiRangeDigitD)
                {
                    _hsiRangeDigitD = tmpValue;
                    _dataHasChanged = true;
                }
            }
        }

        private int HsiRangeDCSBIOSValueToDigit(int value)
        {
            return Convert.ToInt32(10f * (Convert.ToSingle(value) / 65535f));
        }

        public string HsiRangeDigits
        {
            get { return _hsiRangeDigitA.ToString() + _hsiRangeDigitB.ToString() + _hsiRangeDigitC.ToString() + _hsiRangeDigitD.ToString(); }
        }

        public bool HsiPowerOffFlag
        {
            get { return _hsiPowerOffFlag; }
            set
            {
                if (value != _hsiPowerOffFlag)
                {
                    _hsiPowerOffFlag = value;
                    _dataHasChanged = true;
                }
            }
        }

        public bool HsiIsOffCourse
        {
            get { return _hsiIsOffCourse; }
            set
            {
                if (value != _hsiIsOffCourse)
                {
                    _hsiIsOffCourse = value;
                    _dataHasChanged = true;
                }
            }
        }

        public bool HsiFlyingTowardsStation
        {
            get { return _hsiFlyingTowardsStation; }
            set
            {
                if (value != _hsiFlyingTowardsStation)
                {
                    _hsiFlyingTowardsStation = value;
                    _dataHasChanged = true;
                }
            }
        }

        public bool HsiFlyingFromStation
        {
            get { return _hsiFlyingFromStation; }
            set
            {
                if (value != _hsiFlyingFromStation)
                {
                    _hsiFlyingFromStation = value;
                    _dataHasChanged = true;
                }
            }
        }

        public bool HsiRangePowerOffFlag
        {
            get { return _hsiRangePowerOffFlag; }
            set
            {
                if (value != _hsiRangePowerOffFlag)
                {
                    _hsiRangePowerOffFlag = value;
                    _dataHasChanged = true;
                }
            }
        }
    }

    public class A10C_VVI_DataHolderClass
    {
        /*
         * Incorporated in same update cycle.
         * If heading changes, no updated until the two others have changed too
         */
        private int _vviValue;
        /*
         * 
         */
        private bool _dataHasChanged;

        public A10C_VVI_DataHolderClass(int vviValue)
        {
            _vviValue = vviValue;
        }

        public bool DataHasChanged
        {
            get { return _dataHasChanged; }
        }

        public void Reset()
        {
            _dataHasChanged = false;
        }

        public int VVIValue
        {
            get { return _vviValue; }
            set
            {
                if (value != _vviValue)
                {
                    _vviValue = value;
                    _dataHasChanged = true;
                }
            }
        }

    }
}


/*
            if (_a10CHsiDataHolderClass.HsiPowerOffFlag)
            {
                using (var graphics = Graphics.FromImage(finalHSIImage))
                {
                    graphics.TranslateTransform(255, 85);
                    var hsiPowerOffSignRectangle = new Rectangle(0, 0, _hsiPowerOffSign.Width, _hsiPowerOffSign.Height);
                    graphics.DrawImage(_hsiPowerOffSign, hsiPowerOffSignRectangle, 0, 0, _hsiPowerOffSign.Width, _hsiPowerOffSign.Height, GraphicsUnit.Pixel, _blackTransparency);
                    //graphics.TranslateTransform(0, 0);
                }
            }
            //Power off added
            /************************************************************************************
             ************************************************************************************ 
             ************************************************************************************

            if (_a10CHsiDataHolderClass.HsiRangePowerOffFlag)
            {
                using (var graphics = Graphics.FromImage(finalHSIImage))
                {
                    graphics.TranslateTransform(17, 50);
                    var hsiRangePowerOffFlagImageRectangle = new Rectangle(0, 0, _hsiRangePowerOffFlagImage.Width, _hsiRangePowerOffFlagImage.Height);
graphics.DrawImage(_hsiRangePowerOffFlagImage, hsiRangePowerOffFlagImageRectangle, 0, 0, _hsiRangePowerOffFlagImage.Width, _hsiRangePowerOffFlagImage.Height, GraphicsUnit.Pixel, _blackTransparency);
                    //graphics.TranslateTransform(0, 0);
                }
            }
            //Power off sign added covering the range digits
            /************************************************************************************
             ************************************************************************************ 
             *************************************************************************************/
