using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Threading;
using System.Windows.Media.Imaging;
using ClassLibraryCommon;
using DCS_BIOS;
using Newtonsoft.Json;
using NonVisuals.StreamDeck.Events;
using ThreadState = System.Threading.ThreadState;

namespace NonVisuals.StreamDeck
{
    [Serializable]
    public class DCSBIOSDecoder : FaceTypeDCSBIOS, IDcsBiosDataListener, IDCSBIOSStringListener
    {
        private DCSBIOSOutput _dcsbiosOutput = null;
        private string _formula = "";
        private bool _useFormula = false;
        private double _formulaResult = 0;
        private string _lastFormulaError = "";
        private List<DCSBIOSConverter> _dcsbiosConverters = new List<DCSBIOSConverter>();
        private volatile bool _valueUpdated;
        [NonSerialized] private int _jaceId = 0;
        private DCSBiosOutputType _decoderSourceType = DCSBiosOutputType.INTEGER_TYPE;
        private bool _treatStringAsNumber = false;
        private EnumDCSBIOSDecoderOutputType _decoderOutputType = EnumDCSBIOSDecoderOutputType.Raw;

        [NonSerialized] private AutoResetEvent _autoResetEvent = new AutoResetEvent(false);
        [NonSerialized] private Thread _imageUpdateTread = null;
        private bool _shutdown = false;




        
        public DCSBIOSDecoder()
        {
            DCSBIOS.GetInstance().AttachDataReceivedListener(this);
            _jaceId = RandomFactory.Get();
            _imageUpdateTread = new Thread(ImageRefreshingThread);
            _imageUpdateTread.Start();
            EventHandlers.AttachDCSBIOSDecoder(this);
        }

        ~DCSBIOSDecoder()
        {
            EventHandlers.DetachDCSBIOSDecoder(this);
            Destroy();
            DCSBIOSStringManager.DetachListener(this);
            DCSBIOS.GetInstance()?.DetachDataReceivedListener(this);
        }

        public override void Destroy()
        {
            IsVisible = false;
            _shutdown = true;
            if (_imageUpdateTread != null && _imageUpdateTread.ThreadState == (ThreadState.Suspended | ThreadState.WaitSleepJoin))
            {
                _autoResetEvent?.Set();
            }
        }

        public static void ShowOnly(DCSBIOSDecoder dcsbiosDecoder, string streamDeckInstanceId)
        {
            EventHandlers.HideDCSBIOSDecoders(dcsbiosDecoder, StreamDeckPanel.GetInstance(streamDeckInstanceId).SelectedLayerName);
            dcsbiosDecoder.IsVisible = true;
        }

        public void HideAllEvent(object sender, StreamDeckHideDecoderEventArgs e)
        {
            if (StreamDeckInstanceId == e.StreamDeckInstanceId && StreamDeckButtonName == e.StreamDeckButtonName)
            {
                IsVisible = false;
            }
        }

        public void AfterClone()
        {
            DCSBIOS.GetInstance().AttachDataReceivedListener(this);
            _autoResetEvent = new AutoResetEvent(false);
            if (_imageUpdateTread != null)
            {
                try
                {
                    _imageUpdateTread.Abort();
                }
                catch (Exception ) { }
            }
            _imageUpdateTread = new Thread(ImageRefreshingThread);
            _imageUpdateTread.Start();
        }

        private void ImageRefreshingThread()
        {
            while (!_shutdown)
            {
                if (!IsVisible)
                {
                    /*
                     * If decoder isn't visible we end up here until it is visible again
                     */
                    _autoResetEvent.WaitOne();
                }

                if (_shutdown)
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

        public void DcsBiosDataReceived(object sender, DCSBIOSDataEventArgs e)
        {
            try
            {
                if (_decoderSourceType == DCSBiosOutputType.STRING_TYPE)
                {
                    return;
                }

                if (_dcsbiosOutput?.Address == e.Address)
                {
                    if (!Equals(UintDcsBiosValue, e.Data))
                    {
                        UintDcsBiosValue = e.Data;
                        _valueUpdated = true;
                    }
                }
            }
            catch (Exception ex)
            {
                Common.LogError(ex, "DcsBiosDataReceived()");
            }
        }


        public void DCSBIOSStringReceived(object sender, DCSBIOSStringDataEventArgs e)
        {
            try
            {
                if (_decoderSourceType == DCSBiosOutputType.INTEGER_TYPE)
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
                            StringDcsBiosValue = "";
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
                Common.LogError(ex, "DCSBIOSStringReceived()");
            }
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

                var showImage = false;
                /*   1) Use decoder raw(formula / no formula)  */
                if (_dcsbiosConverters.Count == 0)
                {
                    if (UseFormula)
                    {
                        ButtonFinalText = ButtonTextTemplate.Replace(StreamDeckConstants.DCSBIOSValuePlaceHolder, _formulaResult.ToString(CultureInfo.InvariantCulture));
                        showImage = true;
                    }
                    else if (DecoderSourceType == DCSBiosOutputType.STRING_TYPE && !TreatStringAsNumber)
                    {
                        ButtonFinalText = ButtonTextTemplate.Replace(StreamDeckConstants.DCSBIOSValuePlaceHolder, (string.IsNullOrWhiteSpace(StringDcsBiosValue) ? "" : StringDcsBiosValue));
                        showImage = true;
                    }
                    else if (!string.IsNullOrEmpty(ButtonTextTemplate))
                    {
                        ButtonFinalText = ButtonTextTemplate.Replace(StreamDeckConstants.DCSBIOSValuePlaceHolder, UintDcsBiosValue.ToString(CultureInfo.InvariantCulture));
                        showImage = true;
                    }

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
                else if (_dcsbiosConverters.Count > 0 && (_decoderSourceType == DCSBiosOutputType.STRING_TYPE && _treatStringAsNumber) || _decoderSourceType == DCSBiosOutputType.INTEGER_TYPE)
                {
                    Bitmap converterBitmap = null;

                    foreach (var dcsbiosConverter in _dcsbiosConverters)
                    {
                        if (dcsbiosConverter.CriteriaFulfilled(UseFormula ? FormulaResult : UintDcsBiosValue))
                        {
                            converterBitmap = dcsbiosConverter.Get();
                            break;
                        }
                    }

                    if (IsVisible)
                    {
                        if (converterBitmap != null)
                        {
                            ShowBitmap(converterBitmap);
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
                _lastFormulaError = "";
            }
            catch (Exception exception)
            {
                //Common.LogError(exception);
                _lastFormulaError = exception.Message;
            }
        }

        public bool UseFormula
        {
            get => _useFormula;
            set => _useFormula = value;
        }

        private void BlackoutKey()
        {
            StreamDeckPanel.GetInstance(StreamDeckInstanceId).ClearFace(StreamDeckButtonName);
        }

        private void ShowBitmap(Bitmap bitmap)
        {
            StreamDeckPanel.GetInstance(StreamDeckInstanceId).SetImage(StreamDeckButtonName, bitmap);
        }

        private void ShowBitmapImage(BitmapImage bitmapImage)
        {
            StreamDeckPanel.GetInstance(StreamDeckInstanceId).SetImage(StreamDeckButtonName, bitmapImage);
        }

        public void RemoveDCSBIOSOutput()
        {
            _dcsbiosOutput = null;
        }

        public void Clear()
        {
            _formula = "";
            _dcsbiosOutput = null;
            _dcsbiosConverters.Clear();
            _valueUpdated = false;
            _lastFormulaError = "";
            _formulaResult = 0;
        }

        private double EvaluateFormula()
        {
            //360 - floor((HSI_HDG / 65535) * 360)
            var variables = new Dictionary<string, double>();
            variables.Add(_dcsbiosOutput.ControlId, 0);
            variables[_dcsbiosOutput.ControlId] = UintDcsBiosValue;
            return JaceExtendedFactory.Instance(ref _jaceId).CalculationEngine.Calculate(_formula, variables);
        }

        public string Formula
        {
            get => _formula;
            set => _formula = value;
        }

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
                StringDcsBiosValue = "";
                if (_dcsbiosOutput.DCSBiosOutputType == DCSBiosOutputType.STRING_TYPE)
                {
                    DCSBIOSStringManager.AddListener(_dcsbiosOutput, this);
                }
            }
        }

        public void Add(DCSBIOSConverter dcsbiosConverter)
        {
            _dcsbiosConverters.Add(dcsbiosConverter);
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

        public bool TreatStringAsNumber
        {
            get => _treatStringAsNumber;
            set => _treatStringAsNumber = value;
        }

        public Font RawTextFont
        {
            get => TextFont;
            set => TextFont = value;
        }

        public Color RawFontColor
        {
            get => FontColor;
            set => FontColor = value;
        }

        public Color RawBackgroundColor
        {
            get => BackgroundColor;
            set => BackgroundColor = value;
        }

        public DCSBiosOutputType DecoderSourceType
        {
            get => _decoderSourceType;
            set => _decoderSourceType = value;
        }

        public EnumDCSBIOSDecoderOutputType DecoderOutputType
        {
            get => _decoderOutputType;
            set => _decoderOutputType = value;
        }


        /*
         * It can have integer | string + treat as number | string input
         * It can have raw / converter output
         */
        public bool DecoderConfigurationOK()
        {
            var formulaIsOK = _useFormula ? !string.IsNullOrEmpty(_formula) : true;
            var sourceIsOK = _dcsbiosOutput != null;
            var convertersOK = _dcsbiosConverters.FindAll(o => o.FaceConfigurationIsOK == false).Count == 0;


            switch (DecoderOutputType)
            {
                case EnumDCSBIOSDecoderOutputType.Raw:
                    {
                        return formulaIsOK && sourceIsOK && ConfigurationOK;
                    }
                case EnumDCSBIOSDecoderOutputType.Converter:
                    {
                        return formulaIsOK && sourceIsOK && convertersOK;
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
                            _formula = "";
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
                if (base.IsVisible)
                {
                    _autoResetEvent?.Set();
                    Show();
                }
            }
        }
    }

    public enum EnumDCSBIOSDecoderOutputType
    {
        Raw,
        Converter
    }
}
