using System;
using System.Collections.Generic;
using ClassLibraryCommon;
using DCS_BIOS;
using HidLibrary;

namespace NonVisuals
{

    public abstract class SaitekPanel : GamingPanel, IProfileHandlerListener, IDcsBiosDataListener
    {
        
        protected virtual void OnLedLightChanged(SaitekPanelLEDPosition saitekPanelLEDPosition, PanelLEDColor panelLEDColor)
        {
            OnLedLightChangedA?.Invoke(this, new LedLightChangeEventArgs() { UniqueId = InstanceId, LEDPosition = saitekPanelLEDPosition, LEDColor = panelLEDColor });
        }

        private int _vendorId;
        private int _productId;
        private Exception _lastException;
        private readonly object _exceptionLockObject = new object();
        private GamingPanelEnum _typeOfGamingPanel;
        private bool _isDirty;
        //private bool _isAttached;
        private bool _forwardPanelEvent;
        private static readonly object _lockObject = new object();
        private static readonly List<SaitekPanel> _saitekPanels = new List<SaitekPanel>();
        private bool _settingsLoading = false;
        /*
         * IMPORTANT STUFF
         */
        private readonly DCSBIOSOutput _updateCounterDCSBIOSOutput;
        private static readonly object _updateCounterLockObject = new object();
        private uint _count;
        private bool _synchedOnce;
        private readonly Guid _guid = Guid.NewGuid();
        private readonly string _hash;

        protected SaitekPanel(GamingPanelEnum typeOfGamingPanel, HIDSkeleton hidSkeleton)
        {
            _typeOfGamingPanel = typeOfGamingPanel;
            HIDSkeletonBase = hidSkeleton;
            if (Common.IsOperationModeFlagSet(OperationFlag.DCSBIOSOutputEnabled))
            {
                _updateCounterDCSBIOSOutput = DCSBIOSOutput.GetUpdateCounter();
            }
            _hash = Common.GetMd5Hash(hidSkeleton.InstanceId);
        }

        public abstract string SettingsVersion();
        public abstract void Startup();
        public abstract void Shutdown();
        public abstract void ClearSettings();
        public abstract void ImportSettings(List<string> settings);
        public abstract List<string> ExportSettings();
        public abstract void SavePanelSettings(object sender, ProfileHandlerEventArgs e);
        public abstract DcsOutputAndColorBinding CreateDcsOutputAndColorBinding(SaitekPanelLEDPosition saitekPanelLEDPosition, PanelLEDColor panelLEDColor, DCSBIOSOutput dcsBiosOutput);
        public abstract void DcsBiosDataReceived(object sender, DCSBIOSDataEventArgs e);
        //public abstract void DcsBiosDataReceived(byte[] array);
        //public abstract void GetDcsBiosData(byte[] bytes);
        protected HIDSkeleton HIDSkeletonBase;
        private bool _closed;
        public long ReportCounter = 0;


        protected bool FirstReportHasBeenRead = false;
        protected HashSet<ISaitekPanelKnob> SaitekPanelKnobs = new HashSet<ISaitekPanelKnob>();
        protected byte[] OldSaitekPanelValue = { 0, 0, 0 };
        protected byte[] NewSaitekPanelValue = { 0, 0, 0 };
        protected byte[] OldSaitekPanelValueTPM = { 0, 0, 0, 0, 0 };
        protected byte[] NewSaitekPanelValueTPM = { 0, 0, 0, 0, 0 };
        protected abstract void SaitekPanelKnobChanged(IEnumerable<object> hashSet);

        protected void StartListeningForPanelChanges()
        {
            try
            {
                if (HIDSkeletonBase.HIDReadDevice != null && !Closed)
                {
                    //Common.DebugP("Adding callback " + TypeOfSaitekPanel + " " + GuidString);
                    HIDSkeletonBase.HIDReadDevice.ReadReport(OnReport);
                }
            }
            catch (Exception ex)
            {
                Common.DebugP(ex.Message + "\n" + ex.StackTrace);
            }
        }

        private void OnReport(HidReport report)
        {
            if (_typeOfGamingPanel == GamingPanelEnum.TPM && report.Data.Length == 5)
            {
                Array.Copy(NewSaitekPanelValueTPM, OldSaitekPanelValueTPM, 5);
                Array.Copy(report.Data, NewSaitekPanelValueTPM, 5);
                var hashSet = GetHashSetOfChangedKnobs(OldSaitekPanelValueTPM, NewSaitekPanelValueTPM);
                SaitekPanelKnobChanged(hashSet);
                OnSwitchesChanged(hashSet);
                FirstReportHasBeenRead = true;
            }
            else if (report.Data.Length == 3)
            {
                Array.Copy(NewSaitekPanelValue, OldSaitekPanelValue, 3);
                Array.Copy(report.Data, NewSaitekPanelValue, 3);
                var hashSet = GetHashSetOfChangedKnobs(OldSaitekPanelValue, NewSaitekPanelValue);
                SaitekPanelKnobChanged(hashSet);
                OnSwitchesChanged(hashSet);
                FirstReportHasBeenRead = true;
            }
            
            StartListeningForPanelChanges();
        }

        private HashSet<object> GetHashSetOfChangedKnobs(byte[] oldValue, byte[] newValue)
        {
            var result = new HashSet<object>();

            var endValue = 3;
            if(_typeOfGamingPanel == GamingPanelEnum.TPM)
            {
                endValue = 5;
            }

            for (var i = 0; i < endValue; i++)
            {
                var oldByte = oldValue[i];
                var newByte = newValue[i];

                foreach (var saitekPanelKnob in SaitekPanelKnobs)
                {
                    if (saitekPanelKnob.Group == i && (FlagHasChanged(oldByte, newByte, saitekPanelKnob.Mask) || !FirstReportHasBeenRead))
                    {
                        var addKnob = true;

                        saitekPanelKnob.IsOn = FlagValue(newValue, saitekPanelKnob);
                        if (saitekPanelKnob.GetType() == typeof(MultiPanelKnob) && !saitekPanelKnob.IsOn)
                        {
                            var multiPanelKnob = (MultiPanelKnob)saitekPanelKnob;
                            switch (multiPanelKnob.MultiPanelPZ70Knob)
                            {
                                case MultiPanelPZ70Knobs.AP_BUTTON:
                                case MultiPanelPZ70Knobs.HDG_BUTTON:
                                case MultiPanelPZ70Knobs.NAV_BUTTON:
                                case MultiPanelPZ70Knobs.IAS_BUTTON:
                                case MultiPanelPZ70Knobs.ALT_BUTTON:
                                case MultiPanelPZ70Knobs.VS_BUTTON:
                                case MultiPanelPZ70Knobs.APR_BUTTON:
                                case MultiPanelPZ70Knobs.REV_BUTTON:
                                    {
                                        /*
                                         * IMPORTANT
                                         * ---------
                                         * The LCD buttons toggle between on and off. It is the toggle value that defines if the button is OFF, not the fact that the user releases the button.
                                         * Therefore the fore-mentioned buttons cannot be used as usual in a loop with knobBinding.WhenTurnedOn
                                         * Instead the buttons global bool value must be used!
                                         * 
                                         */
                                        //Do not add OFF values for these buttons! Read comment above.
                                        addKnob = false;
                                        break;
                                    }
                            }
                        }

                        if (addKnob)
                        {
                            result.Add(saitekPanelKnob);
                        }

                    }
                }
            }
            return result;
        }

        private static bool FlagValue(byte[] currentValue, ISaitekPanelKnob saitekPanelKnob)
        {
            return (currentValue[saitekPanelKnob.Group] & saitekPanelKnob.Mask) > 0;
        }

        protected void UpdateCounter(uint address, uint data)
        {
            lock (_updateCounterLockObject)
            {
                if (_updateCounterDCSBIOSOutput.Address == address)
                {
                    var newCount = _updateCounterDCSBIOSOutput.GetUIntValue(data);
                    if (!_synchedOnce)
                    {
                        _count = newCount;
                        _synchedOnce = true;
                        return;
                    }
                    //Max is 255
                    if ((newCount == 0 && _count == 255) || newCount - _count == 1)
                    {
                        //All is well
                        _count = newCount;
                    }
                    else if (newCount - _count != 1)
                    {
                        //Not good
                        if (OnUpdatesHasBeenMissed != null)
                        {
                            OnUpdatesHasBeenMissed(this, new DCSBIOSUpdatesMissedEventArgs() { UniqueId = HIDSkeletonBase.InstanceId, GamingPanelEnum = _typeOfGamingPanel, Count = (int)(newCount - _count) });
                            _count = newCount;
                        }
                    }
                }
            }
        }

        public void SelectedAirframe(object sender, AirframeEventArgs e)
        {

        }

        //User can choose not to in case switches needs to be reset but not affect the airframe. E.g. after crashing.
        public void SetForwardKeyPresses(object sender, ForwardPanelEventArgs e)
        {
            _forwardPanelEvent = e.Forward;
        }

        public bool ForwardPanelEvent
        {
            get => _forwardPanelEvent;
            set => _forwardPanelEvent = value;
        }

        public int VendorId
        {
            get => _vendorId;
            set => _vendorId = value;
        }

        public int ProductId
        {
            get => _productId;
            set => _productId = value;
        }

        public string InstanceId
        {
            get => HIDSkeletonBase.InstanceId;
            set => HIDSkeletonBase.InstanceId = value;
        }

        public string Hash => _hash;

        public string GuidString => _guid.ToString();

        public void SetLastException(Exception ex)
        {
            try
            {
                if (ex == null)
                {
                    return;
                }
                Common.LogError(666, ex, "Via SaitekPanel.SetLastException()");
                lock (_exceptionLockObject)
                {
                    _lastException = new Exception(ex.GetType() + Environment.NewLine + ex.Message + Environment.NewLine + ex.StackTrace);
                }
            }
            catch (Exception)
            {
            }
        }


        /*         
         * These are used for static handling, the OnReport method must be static and to access the object that the OnReport belongs to must go via these methods.
         * If a static object would be kept inside the class it would go nuts considering there can be many panels of same type
         */
        public static void AddSaitekPanelObject(SaitekPanel saitekPanel)
        {
            lock (_lockObject)
            {
                _saitekPanels.Add(saitekPanel);
            }
        }

        public static void RemoveSaitekPanelObject(SaitekPanel saitekPanel)
        {
            lock (_lockObject)
            {
                _saitekPanels.Remove(saitekPanel);
            }
        }

        public static SaitekPanel GetSaitekPanelObject(string instanceId)
        {
            lock (_lockObject)
            {
                foreach (var saitekPanel in _saitekPanels)
                {
                    if (saitekPanel.InstanceId.Equals(instanceId))
                    {
                        return saitekPanel;
                    }
                }
            }
            return null;
        }

        public Exception GetLastException(bool resetException = false)
        {
            Exception result;
            lock (_exceptionLockObject)
            {
                result = _lastException;
                if (resetException)
                {
                    _lastException = null;
                }
            }
            return result;
        }

        public bool IsDirty
        {
            get => _isDirty;
            set => _isDirty = value;
        }

        public bool SettingsLoading
        {
            get => _settingsLoading;
            set => _settingsLoading = value;
        }

        public GamingPanelEnum TypeOfSaitekPanel
        {
            get => _typeOfGamingPanel;
            set => _typeOfGamingPanel = value;
        }
        //TODO fixa att man kan koppla in/ur panelerna?
        /*
         * 
        
        public bool IsAttached
        {
            get { return _isAttached; }
            set { _isAttached = value; }
        }
        */

        public bool Closed
        {
            get => _closed;
            set => _closed = value;
        }
        
    }

    public class LedLightChangeEventArgs : EventArgs
    {
        public string UniqueId { get; set; }
        public SaitekPanelLEDPosition LEDPosition { get; set; }
        public PanelLEDColor LEDColor { get; set; }
    }

}
