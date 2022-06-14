using System.Diagnostics;
using NonVisuals.StreamDeck.Panels;

namespace NonVisuals.StreamDeck
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Globalization;
    using System.IO;
    using System.Text;
    using System.Threading;
    using DCS_BIOS;
    using DCS_BIOS.EventArgs;
    using DCS_BIOS.Interfaces;

    using Newtonsoft.Json;
    using NLog;
    using NonVisuals.StreamDeck.Events;

    [Serializable]
    public class DCSBIOSDecoder : FaceTypeDCSBIOS, IDcsBiosDataListener, IDCSBIOSStringListener, IDisposable
    {
        internal static Logger logger = LogManager.GetCurrentClassLogger();
        private DCSBIOSOutput _dcsbiosOutput;
        private DCSBIOSOutputFormula _dcsbiosOutputFormula;
        private string _formulaObsolete = string.Empty;
        private bool _useFormula;
        private double _formulaResult = double.MaxValue;
        private string _lastFormulaError = string.Empty;
        private List<DCSBIOSConverter> _dcsbiosConverters = new List<DCSBIOSConverter>();
        private volatile bool _valueUpdated = true;
        private DCSBiosOutputType _decoderSourceType = DCSBiosOutputType.IntegerType;
        private bool _treatStringAsNumber;
        private EnumDCSBIOSDecoderOutputType _decoderOutputType = EnumDCSBIOSDecoderOutputType.Raw;

        [NonSerialized] private Thread _imageUpdateTread;
        private volatile bool _shutdownThread;

        private volatile Bitmap _converterBitmap;

        private bool _limitDecimalPlaces = false;
        private NumberFormatInfo _numberFormatInfoFormula;

        /*[NonSerialized]
        private List<string> _listDecimalFormatters = new List<string>()
        {
            "0", "0.0", "0.00", "0.000", "0.0000", "0.00000"
        };*/

        public DCSBIOSDecoder()
        {
            Debug.WriteLine("Creating Image Update Thread for Streamdeck #2");
            _imageUpdateTread = new Thread(ImageRefreshingThread);
            _imageUpdateTread.Start();
            BIOSEventHandler.AttachDataListener(this);
            BIOSEventHandler.AttachStringListener(this);
            SDEventHandler.AttachDCSBIOSDecoder(this);
        }

        private bool _disposed;
        // Protected implementation of Dispose pattern.
        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _shutdownThread = true;
                    while (_imageUpdateTread is { IsAlive: true })
                    {
                        Thread.Sleep(10);
                    }
                    SDEventHandler.DetachDCSBIOSDecoder(this);
                    BIOSEventHandler.DetachStringListener(this);
                    BIOSEventHandler.DetachDataListener(this);
                }

                _disposed = true;
            }

            // Call base class implementation.
            base.Dispose(disposing);
        }

        public new void Dispose()
        {
            // Dispose of unmanaged resources.
            Dispose(true);
        }

        [JsonIgnore]
        public override string FaceDescription
        {
            get
            {
                var stringBuilder = new StringBuilder(100);
                stringBuilder.Append("Face DCS-BIOS Decoder");
                if (FormulaSelectedAndOk())
                {
                    stringBuilder.Append(" ").Append(_dcsbiosOutputFormula.DCSBIOSOutputs()[0].ControlId);
                }
                else if (_dcsbiosOutput != null)
                {
                    stringBuilder.Append(" ").Append(_dcsbiosOutput.ControlId);
                }

                return stringBuilder.ToString();
            }
        }

        public static void ShowOnly(DCSBIOSDecoder dcsbiosDecoder, StreamDeckPanel streamDeckPanel)
        {
            SDEventHandler.HideDCSBIOSDecoders(dcsbiosDecoder, streamDeckPanel.SelectedLayerName, streamDeckPanel.BindingHash);
            dcsbiosDecoder.IsVisible = true;
        }

        public void HideAllEvent(object sender, StreamDeckHideDecoderEventArgs e)
        {
            if (StreamDeckPanelInstance.BindingHash == e.BindingHash && StreamDeckButtonName == e.StreamDeckButtonName)
            {
                IsVisible = false;
            }
        }


        public override void AfterClone()
        {
            if (_imageUpdateTread != null)
            {
                try
                {
                    _shutdownThread = true;
                    Thread.Sleep(Constants.ThreadShutDownWaitTime);
                    _shutdownThread = false;
                }
                catch (Exception)
                {
                    // ignored
                }
            }

            Debug.WriteLine("Creating Image Update Thread for Streamdeck #1");
            _imageUpdateTread = new Thread(ImageRefreshingThread);
            _imageUpdateTread.Start();
        }

        private void ImageRefreshingThread()
        {
            while (!_shutdownThread)
            {
                if (!IsVisible)
                {
                    /*
                     * If decoder isn't visible we end up here until it is visible again
                     */
                    // _autoResetEvent.WaitOne();
                }

                if (_shutdownThread)
                {
                    break;
                }

                if (ValueUpdated)
                {
                    HandleNewDCSBIOSValue();
                }

                Thread.Sleep(StreamDeckConstants.IMAGE_UPDATING_THREAD_SLEEP_TIME);
            }
        }



        /*
         * 18 Nov 2021
         * Issue with DCSBIOSDecoder not updating a button's text. Adding this helped.
         */
        [NonSerialized] private readonly int _refreshIntervalLimit = 20;
        [NonSerialized] private int _refreshInterval = 0;
        public void DcsBiosDataReceived(object sender, DCSBIOSDataEventArgs e)
        {
            try
            {
                if (_decoderSourceType == DCSBiosOutputType.StringType)
                {
                    return;
                }

                if (FormulaSelectedAndOk() && _dcsbiosOutputFormula.CheckForMatchAndNewValue(e.Address, e.Data, 20))
                {
                    _valueUpdated = true;
                }
                else if (!_useFormula && _dcsbiosOutput?.Address == e.Address)
                {
                    _refreshInterval++;
                    if (!Equals(UintDcsBiosValue, e.Data) || _refreshInterval > _refreshIntervalLimit)
                    {
                        _refreshInterval = 0;
                        UintDcsBiosValue = _dcsbiosOutput.GetUIntValue(e.Data);
                        _valueUpdated = true;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "DcsBiosDataReceived()");
            }
        }


        public void DCSBIOSStringReceived(object sender, DCSBIOSStringDataEventArgs e)
        {
            try
            {
                if (_decoderSourceType == DCSBiosOutputType.IntegerType)
                {
                    return;
                }

                if (_dcsbiosOutput?.Address == e.Address)
                {
                    if (TreatStringAsNumber && string.IsNullOrWhiteSpace(e.StringData))
                    {
                        StringDcsBiosValue = "0";
                    }
                    else
                    {
                        if (string.IsNullOrWhiteSpace(e.StringData))
                        {
                            StringDcsBiosValue = string.Empty;
                        }
                        else if (e.StringData.Length < _dcsbiosOutput.MaxLength)
                        {
                            StringDcsBiosValue = e.StringData.Substring(0, e.StringData.Length);
                        }
                        else
                        {
                            StringDcsBiosValue = e.StringData.Substring(0, _dcsbiosOutput.MaxLength);
                        }
                    }

                    /*
                     * If DCS-BIOS sends null string data and the decoder should 
                     * treat it as number then it will be represented by zero.
                     */
                    if (_treatStringAsNumber && uint.TryParse(string.IsNullOrWhiteSpace(e.StringData) ? "0" : e.StringData.Substring(0, _dcsbiosOutput.MaxLength), out var tmpUint))
                    {
                        UintDcsBiosValue = tmpUint;
                    }

                    _valueUpdated = true;
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "DCSBIOSStringReceived()");
            }
        }

        public bool FormulaSelectedAndOk()
        {
            return _useFormula && _dcsbiosOutputFormula != null;
        }

        /*
         * 1) integer
         * 2) string but treat as integer
         * 3) string and treat it as string (no formulas, no converters)
         */
        public void HandleNewDCSBIOSValue()
        {
            try
            {
                if (UseFormula)
                {
                    _formulaResult = EvaluateFormula();
                }

                /*
                 * 1) Use decoder raw  (formula / no formula) STRING_TYPE or INTEGER_TYPE
                 * 2) Use converter    (formula / no formula)
                 * 3) show blank image
                 */

                /*   1) Use decoder raw(formula / no formula)  */
                if (_dcsbiosConverters.Count == 0)
                {
                    ButtonFinalText = ButtonTextTemplate.Replace(StreamDeckConstants.DCSBIOSValuePlaceHolder, GetResultString());

                    var showImage = !string.IsNullOrEmpty(ButtonTextTemplate);

                    if (IsVisible)
                    {
                        if (showImage)
                        {
                            Show();
                        }
                        else
                        {
                            BlackoutKey();
                        }
                    }
                }

                /* 2) Use converter    (formula / no formula) */
                else if (_dcsbiosConverters.Count > 0 && (_decoderSourceType == DCSBiosOutputType.StringType && _treatStringAsNumber) || _decoderSourceType == DCSBiosOutputType.IntegerType)
                {
                    foreach (var dcsbiosConverter in _dcsbiosConverters)
                    {
                        if (dcsbiosConverter.CriteriaFulfilled(UseFormula ? FormulaResult : UintDcsBiosValue))
                        {
                            _converterBitmap = dcsbiosConverter.Get(_useFormula && _limitDecimalPlaces ? _numberFormatInfoFormula : null);
                            break;
                        }
                    }

                    if (IsVisible)
                    {
                        if (_converterBitmap != null)
                        {
                            ShowBitmap(_converterBitmap);
                        }
                        else
                        {
                            BlackoutKey();
                        }
                    }
                }

                /* 3) show blank image */
                else
                {
                    if (IsVisible)
                    {
                        BlackoutKey();
                    }
                }

                _lastFormulaError = string.Empty;
            }
            catch (Exception exception)
            {
                // Common.LogError(exception);
                _lastFormulaError = exception.Message;
            }
        }

        private void ShowBitmap(Bitmap bitmap)
        {
            if (StreamDeckPanelInstance == null)
            {
                throw new Exception("StreamDeckPanelInstance is not set, cannot show image [DCSBIOSDecoder]");
            }

            StreamDeckPanelInstance.SetImage(StreamDeckButtonName, bitmap);
        }

        public string GetResultString()
        {
            if (_useFormula && _limitDecimalPlaces)
            {
                return string.Format(_numberFormatInfoFormula, "{0:N}", _formulaResult);
            }

            if (_useFormula)
            {
                return _formulaResult.ToString(CultureInfo.InvariantCulture);
            }

            if (DecoderSourceType == DCSBiosOutputType.StringType && !TreatStringAsNumber)
            {
                return string.IsNullOrWhiteSpace(StringDcsBiosValue) ? string.Empty : StringDcsBiosValue;
            }

            return UintDcsBiosValue.ToString(CultureInfo.InvariantCulture);
        }

        [JsonProperty("UseFormula", Required = Required.Default)]
        public bool UseFormula
        {
            get => _useFormula;
            set => _useFormula = value;
        }

        private void BlackoutKey()
        {
            StreamDeckPanelInstance.ClearFace(StreamDeckButtonName);
        }

        /*
        private void ShowBitmapImage(BitmapImage bitmapImage)
        {
            if (StreamDeckPanelInstance == null)
            {
                throw new Exception("StreamDeckPanelInstance is not set, cannot show image [DCSBIOSDecoder]");
            }

            StreamDeckPanelInstance.SetImage(StreamDeckButtonName, bitmapImage);
        }
        */
        [JsonIgnore]
        public new StreamDeckPanel StreamDeckPanelInstance
        {
            get => base.StreamDeckPanelInstance;
            set
            {
                base.StreamDeckPanelInstance = value;
                foreach (var dcsbiosConverter in _dcsbiosConverters)
                {
                    dcsbiosConverter.StreamDeckPanelInstance = base.StreamDeckPanelInstance;
                }
            }
        }

        public void RemoveDCSBIOSOutput()
        {
            _dcsbiosOutput = null;
        }

        public void Clear()
        {
            _dcsbiosOutputFormula = null;
            _dcsbiosOutput = null;
            _dcsbiosConverters.Clear();
            _valueUpdated = false;
            _lastFormulaError = string.Empty;
            _formulaResult = 0;
        }

        private double EvaluateFormula()
        {
            double result = 0;

            if (_dcsbiosOutputFormula != null)
            {
                return _dcsbiosOutputFormula.Evaluate(true);
            }

            return result;
        }

        [JsonProperty("FormulaInstance", Required = Required.Default)]
        public DCSBIOSOutputFormula FormulaInstance
        {
            get => _dcsbiosOutputFormula;
            set => _dcsbiosOutputFormula = value;
        }

        public void SetFormula(string formula)
        {
            if (string.IsNullOrEmpty(formula))
            {
                return;
            }
            _dcsbiosOutputFormula = new DCSBIOSOutputFormula(formula);
        }

        [Obsolete]
        [JsonProperty("Formula", Required = Required.Default)]
        public string FormulaObsolete
        {
            get => _formulaObsolete;
            set => _formulaObsolete = value;
        }

        [JsonProperty("DCSBIOSOutput", Required = Required.AllowNull)]
        public DCSBIOSOutput DCSBIOSOutput
        {
            get => _dcsbiosOutput;
            set
            {
                /*
                 * Can be of two types, integer or string output
                 */
                _valueUpdated = true;
                _dcsbiosOutput = value;
                UintDcsBiosValue = uint.MaxValue;
                StringDcsBiosValue = string.Empty;
                if (_dcsbiosOutput != null && _dcsbiosOutput.DCSBiosOutputType == DCSBiosOutputType.StringType)
                {
                    DCSBIOSStringManager.AddListeningAddress(_dcsbiosOutput);
                }
            }
        }

        public void Add(DCSBIOSConverter dcsbiosConverter)
        {
            _dcsbiosConverters.Add(dcsbiosConverter);
        }

        [JsonProperty("ImageFiles", Required = Required.Default)]
        public List<string> ImageFiles
        {
            get
            {
                var result = new List<string>();

                foreach (var dcsbiosConverter in _dcsbiosConverters)
                {
                    if (!string.IsNullOrEmpty(dcsbiosConverter.ImageFileRelativePath))
                    {
                        result.Add(dcsbiosConverter.ImageFileRelativePath);
                    }
                }

                return result;
            }
        }

        /*
         * When exporting buttons the image file's path will be removed.
         * When user import the buttons and specifies to where the images should
         * be stored then the path is updated again.
         *
         * So there will only be the filename in ImageFileRelativePath.
         */
        public void ResetImageFilePaths()
        {
            foreach (var dcsbiosConverter in _dcsbiosConverters)
            {
                if (!string.IsNullOrEmpty(dcsbiosConverter.ImageFileRelativePath))
                {
                    dcsbiosConverter.ImageFileRelativePath = Path.GetFileName(dcsbiosConverter.ImageFileRelativePath);
                }
            }
        }

        public void SetImageFilePaths(string path)
        {
            foreach (var dcsbiosConverter in _dcsbiosConverters)
            {
                if (dcsbiosConverter.ConverterOutputType == EnumConverterOutputType.Image || dcsbiosConverter.ConverterOutputType == EnumConverterOutputType.ImageOverlay)
                {
                    dcsbiosConverter.ImageFileRelativePath = path + "\\" + Path.GetFileName(dcsbiosConverter.ImageFileRelativePath);
                }
            }
        }

        public void Replace(DCSBIOSConverter oldDcsBiosValueToFaceConverter, DCSBIOSConverter newDcsBiosValueToFaceConverter)
        {
            Remove(oldDcsBiosValueToFaceConverter);
            Add(newDcsBiosValueToFaceConverter);
        }

        public void Remove(DCSBIOSConverter dcsbiosConverter)
        {
            _dcsbiosConverters.Remove(dcsbiosConverter);
        }

        [JsonProperty("DCSBIOSConverters", Required = Required.Default)]
        public List<DCSBIOSConverter> DCSBIOSConverters
        {
            get => _dcsbiosConverters;
            set => _dcsbiosConverters = value;
        }

        [JsonIgnore]
        public bool ValueUpdated
        {
            get
            {
                var result = false;
                if (_valueUpdated)
                {
                    result = true;
                    _valueUpdated = false; // Reset so next read without update will give false
                }

                return result;
            }
        }

        [JsonIgnore]
        public bool HasErrors => !string.IsNullOrEmpty(_lastFormulaError);

        [JsonIgnore]
        public string LastFormulaError => _lastFormulaError;

        [JsonIgnore]
        public double FormulaResult => _formulaResult;

        public string GetFriendlyInfo()
        {
            return _dcsbiosOutput.ControlId;
        }

        [JsonProperty("TreatStringAsNumber", Required = Required.Default)]
        public bool TreatStringAsNumber
        {
            get => _treatStringAsNumber;
            set => _treatStringAsNumber = value;
        }

        [JsonProperty("RawTextFont", Required = Required.Default)]
        public Font RawTextFont
        {
            get => TextFont;
            set => TextFont = value;
        }

        [JsonProperty("RawFontColor", Required = Required.Default)]
        public Color RawFontColor
        {
            get => FontColor;
            set => FontColor = value;
        }

        [JsonProperty("RawBackgroundColor", Required = Required.Default)]
        public Color RawBackgroundColor
        {
            get => BackgroundColor;
            set => BackgroundColor = value;
        }

        [JsonProperty("DecoderSourceType", Required = Required.Default)]
        public DCSBiosOutputType DecoderSourceType
        {
            get => _decoderSourceType;
            set => _decoderSourceType = value;
        }

        [JsonProperty("DecoderOutputType", Required = Required.Default)]
        public EnumDCSBIOSDecoderOutputType DecoderOutputType
        {
            get => _decoderOutputType;
            set
            {
                _decoderOutputType = value;
            }
        }


        /*
         * It can have integer | string + treat as number | string input
         * It can have raw / converter output
         */
        public bool DecoderConfigurationOK()
        {
            var formulaIsOK = !_useFormula || _dcsbiosOutputFormula != null;
            var sourceIsOK = _dcsbiosOutput != null;
            var convertersOK = _dcsbiosConverters.FindAll(o => o.FaceConfigurationIsOK == false).Count == 0;


            switch (DecoderOutputType)
            {
                case EnumDCSBIOSDecoderOutputType.Raw:
                    {
                        if (_useFormula)
                        {
                            return formulaIsOK;
                        }

                        return sourceIsOK;
                    }

                case EnumDCSBIOSDecoderOutputType.Converter:
                    {
                        if (_useFormula)
                        {
                            return formulaIsOK && convertersOK;
                        }

                        return sourceIsOK && convertersOK;
                    }

                default:
                    {
                        return false;
                    }
            }
        }

        /*
         * Remove settings not relevant based on output type
         */
        public void Clean()
        {
            switch (DecoderOutputType)
            {
                case EnumDCSBIOSDecoderOutputType.Raw:
                    {
                        _dcsbiosConverters.Clear();
                        if (!_useFormula)
                        {
                            _dcsbiosOutputFormula = null;
                        }

                        break;
                    }

                case EnumDCSBIOSDecoderOutputType.Converter:
                    {
                        break;
                    }
            }
        }

        [JsonIgnore]
        public override bool IsVisible
        {
            get => base.IsVisible;
            set
            {
                base.IsVisible = value;
                if (IsVisible)
                {
                    if (_dcsbiosConverters.Count > 0)
                    {
                        if (_converterBitmap != null)
                        {
                            ShowBitmap(_converterBitmap);
                        }
                        else
                        {
                            BlackoutKey();
                        }
                    }
                    else
                    {
                        Show();
                    }
                }
            }
        }

        [JsonProperty("DefaultImageFilePath", Required = Required.Default)]
        public string DefaultImageFilePath
        {
            // No getter, this is to be phased out, setter here so that any existing setting in user's file still can be parsed by JSON.
            set
            {
                var notUsedAnymoreDefaultImageFilePath = value;
            }
        }

        public void SetNumberOfDecimals(bool limitDecimals, int decimalPlaces = 0)
        {
            if (!UseFormula || FormulaInstance == null)
            {
                return;
            }

            LimitDecimalPlaces = limitDecimals;
            _numberFormatInfoFormula = new NumberFormatInfo
            {
                NumberDecimalSeparator = ".",
                NumberDecimalDigits = decimalPlaces
            };
        }

        [JsonProperty("LimitDecimalPlaces", Required = Required.Default)]
        public bool LimitDecimalPlaces
        {
            get => _limitDecimalPlaces;
            set => _limitDecimalPlaces = value;
        }

        [JsonProperty("NumberFormatInfoFormula", Required = Required.Default)]
        public NumberFormatInfo NumberFormatInfoFormula
        {
            get => _numberFormatInfoFormula;
            set => _numberFormatInfoFormula = value;
        }
    }

    public enum EnumDCSBIOSDecoderOutputType
    {
        Raw,
        Converter
    }

}
