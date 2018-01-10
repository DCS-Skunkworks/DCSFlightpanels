using System;
using System.Collections.Generic;
using DCS_BIOS;

namespace NonVisuals
{

    public enum SaitekPanelsEnum
    {
        Unknown = 0,
        PZ55SwitchPanel = 2,
        PZ69RadioPanel = 4,
        PZ70MultiPanel = 8,
        BackLitPanel = 16,
        TPM = 32,
        HESP = 64
    }

    public abstract class SaitekPanel : IProfileHandlerListener, IDcsBiosDataListener
    {
        //These events can be raised by the descendants of this class.
        public delegate void SwitchesHasBeenChangedEventHandler(string uniqueId, SaitekPanelsEnum saitekPanelsEnum, HashSet<object> hashSet);
        public event SwitchesHasBeenChangedEventHandler OnSwitchesChangedA;

        public delegate void PanelDataToDcsAvailableEventHandler(string stringData);
        public event PanelDataToDcsAvailableEventHandler OnPanelDataAvailableA;

        public delegate void DeviceAttachedEventHandler(string uniqueId, SaitekPanelsEnum saitekPanelsEnum);
        public event DeviceAttachedEventHandler OnDeviceAttachedA;

        public delegate void DeviceDetachedEventHandler(string uniqueId, SaitekPanelsEnum saitekPanelsEnum);
        public event DeviceDetachedEventHandler OnDeviceDetachedA;

        public delegate void SettingsHasChangedEventHandler(string uniqueId, SaitekPanelsEnum saitekPanelsEnum);
        public event SettingsHasChangedEventHandler OnSettingsChangedA;

        public delegate void SettingsHasBeenAppliedEventHandler(string uniqueId, SaitekPanelsEnum saitekPanelsEnum);
        public event SettingsHasBeenAppliedEventHandler OnSettingsAppliedA;

        public delegate void SettingsClearedEventHandler(string uniqueId, SaitekPanelsEnum saitekPanelsEnum);
        public event SettingsClearedEventHandler OnSettingsClearedA;

        public delegate void LedLightChangedEventHandler(string uniqueId, SaitekPanelLEDPosition saitekPanelLEDPosition, PanelLEDColor panelLEDColor);
        public event LedLightChangedEventHandler OnLedLightChangedA;

        public delegate void UpdatesHasBeenMissedEventHandler(string uniqueId, SaitekPanelsEnum saitekPanelsEnum, int count);
        public event UpdatesHasBeenMissedEventHandler OnUpdatesHasBeenMissed;

        //For those that wants to listen to this panel
        public void Attach(ISaitekPanelListener iSaitekPanelListener)
        {
            OnDeviceAttachedA += new DeviceAttachedEventHandler(iSaitekPanelListener.DeviceAttached);
            OnSwitchesChangedA += new SwitchesHasBeenChangedEventHandler(iSaitekPanelListener.SwitchesChanged);
            OnPanelDataAvailableA += new PanelDataToDcsAvailableEventHandler(iSaitekPanelListener.PanelDataAvailable);
            OnSettingsAppliedA += new SettingsHasBeenAppliedEventHandler(iSaitekPanelListener.SettingsApplied);
            OnLedLightChangedA += new LedLightChangedEventHandler(iSaitekPanelListener.LedLightChanged);
            OnSettingsClearedA += new SettingsClearedEventHandler(iSaitekPanelListener.SettingsCleared);
            OnUpdatesHasBeenMissed += new UpdatesHasBeenMissedEventHandler(iSaitekPanelListener.UpdatesHasBeenMissed);
        }

        //For those that wants to listen to this panel
        public void Detach(ISaitekPanelListener iSaitekPanelListener)
        {
            OnDeviceAttachedA -= new DeviceAttachedEventHandler(iSaitekPanelListener.DeviceAttached);
            OnSwitchesChangedA -= new SwitchesHasBeenChangedEventHandler(iSaitekPanelListener.SwitchesChanged);
            OnPanelDataAvailableA -= new PanelDataToDcsAvailableEventHandler(iSaitekPanelListener.PanelDataAvailable);
            OnSettingsAppliedA -= new SettingsHasBeenAppliedEventHandler(iSaitekPanelListener.SettingsApplied);
            OnLedLightChangedA -= new LedLightChangedEventHandler(iSaitekPanelListener.LedLightChanged);
            OnSettingsClearedA -= new SettingsClearedEventHandler(iSaitekPanelListener.SettingsCleared);
            OnUpdatesHasBeenMissed -= new UpdatesHasBeenMissedEventHandler(iSaitekPanelListener.UpdatesHasBeenMissed);
        }

        //For those that wants to listen to this panel when it's settings change
        public void Attach(IProfileHandlerListener iProfileHandlerListener)
        {
            OnSettingsChangedA += new SettingsHasChangedEventHandler(iProfileHandlerListener.PanelSettingsChanged);
        }

        //For those that wants to listen to this panel
        public void Detach(IProfileHandlerListener iProfileHandlerListener)
        {
            OnSettingsChangedA -= new SettingsHasChangedEventHandler(iProfileHandlerListener.PanelSettingsChanged);
        }

        //Used by descendants that wants to raise the event
        protected virtual void OnSwitchesChanged(HashSet<object> hashSet)
        {
            if (OnSwitchesChangedA != null)
            {
                OnSwitchesChangedA(InstanceId, _typeOfSaitekPanel, hashSet);
            }
        }

        //Used by descendants that wants to raise the event
        protected virtual void OnPanelDataAvailable(string stringData)
        {
            if (OnPanelDataAvailableA != null)
            {
                OnPanelDataAvailableA(stringData);
            }
        }

        //Used by descendants that wants to raise the event
        protected virtual void OnDeviceAttached()
        {
            if (OnDeviceAttachedA != null)
            {
                //IsAttached = true;
                OnDeviceAttachedA(InstanceId, _typeOfSaitekPanel);
            }
        }

        //Used by descendants that wants to raise the event
        protected virtual void OnDeviceDetached()
        {
            if (OnDeviceDetachedA != null)
            {
                //IsAttached = false;
                OnDeviceDetachedA(InstanceId, _typeOfSaitekPanel);
            }
        }

        //Used by descendants that wants to raise the event
        protected virtual void OnSettingsChanged()
        {
            if (OnSettingsChangedA != null)
            {
                OnSettingsChangedA(InstanceId, _typeOfSaitekPanel);
            }
        }

        //Used by descendants that wants to raise the event
        protected virtual void OnSettingsApplied()
        {
            if (OnSettingsAppliedA != null)
            {
                OnSettingsAppliedA(InstanceId, _typeOfSaitekPanel);
            }
        }

        //Used by descendants that wants to raise the event
        protected virtual void OnSettingsCleared()
        {
            if (OnSettingsClearedA != null)
            {
                OnSettingsClearedA(InstanceId, _typeOfSaitekPanel);
            }
        }

        //Used by descendants that wants to raise the event
        protected virtual void OnLedLightChanged(SaitekPanelLEDPosition saitekPanelLEDPosition, PanelLEDColor panelLEDColor)
        {
            if (OnLedLightChangedA != null)
            {
                OnLedLightChangedA(InstanceId, saitekPanelLEDPosition, panelLEDColor);
            }
        }

        public void PanelSettingsChanged(string uniqueId, SaitekPanelsEnum saitekPanelsEnum)
        {
            //do nada
        }

        public void PanelSettingsReadFromFile(List<string> settings)
        {
            ClearPanelSettings();
            ImportSettings(settings);
        }

        public void ClearPanelSettings()
        {
            ClearSettings();
            OnSettingsCleared();
        }

        private int _vendorId;
        private int _productId;
        private Exception _lastException;
        private readonly object _exceptionLockObject = new object();
        private SaitekPanelsEnum _typeOfSaitekPanel;
        private bool _isDirty;
        //private bool _isAttached;
        private bool _forwardKeyPresses;
        private static object _lockObject = new object();
        private static List<SaitekPanel> _saitekPanels = new List<SaitekPanel>();
        private bool _keyboardEmulation;
        /*
         * IMPORTANT STUFF
         */
        private DCSBIOSOutput _updateCounterDCSBIOSOutput;
        private static object _updateCounterLockObject = new object();
        private uint _count;
        private bool _synchedOnce;
        private Guid _guid = Guid.NewGuid();

        protected SaitekPanel(SaitekPanelsEnum typeOfSaitekPanel, HIDSkeleton hidSkeleton)
        {
            _typeOfSaitekPanel = typeOfSaitekPanel;
            HIDSkeletonBase = hidSkeleton;
            _updateCounterDCSBIOSOutput = DCSBIOSOutput.GetUpdateCounter();
        }

        public abstract string SettingsVersion();
        public abstract void Startup();
        public abstract void Shutdown();
        public abstract void ClearSettings();
        public abstract void ImportSettings(List<string> settings);
        public abstract List<string> ExportSettings();
        public abstract void SavePanelSettings(ProfileHandler panelProfileHandler);
        public abstract DcsOutputAndColorBinding CreateDcsOutputAndColorBinding(SaitekPanelLEDPosition saitekPanelLEDPosition, PanelLEDColor panelLEDColor, DCSBIOSOutput dcsBiosOutput);
        public abstract void DcsBiosDataReceived(uint address, uint data);
        //public abstract void DcsBiosDataReceived(byte[] array);
        //public abstract void GetDcsBiosData(byte[] bytes);
        protected HIDSkeleton HIDSkeletonBase;
        private bool _closed;

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
                            OnUpdatesHasBeenMissed(HIDSkeletonBase.InstanceId, _typeOfSaitekPanel, (int)(newCount - _count));
                            _count = newCount;
                        }
                    }
                }
            }
        }

        public void SelectedAirframe(DCSAirframe dcsAirframe)
        {
            _keyboardEmulation = dcsAirframe == DCSAirframe.KEYEMULATOR;
        }

        //User can choose not to in case switches needs to be reset but not affect the airframe. E.g. after crashing.
        public void SetForwardKeyPresses(bool forward)
        {
            _forwardKeyPresses = forward;
        }

        public bool ForwardKeyPresses
        {
            get { return _forwardKeyPresses; }
            set { _forwardKeyPresses = value; }
        }

        public int VendorId
        {
            get { return _vendorId; }
            set { _vendorId = value; }
        }

        public int ProductId
        {
            get { return _productId; }
            set { _productId = value; }
        }

        public string InstanceId
        {
            get
            {
                return HIDSkeletonBase.InstanceId;
            }
            set { HIDSkeletonBase.InstanceId = value; }
        }

        public string GuidString
        {
            get { return _guid.ToString(); }
        }

        public void SetLastException(Exception ex)
        {
            try
            {
                if (ex == null)
                {
                    return;
                }
                DBCommon.LogError(666, ex, "Via SaitekPanel.SetLastException()");
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
            get { return _isDirty; }
            set
            {
                _isDirty = value;
            }
        }

        public SaitekPanelsEnum TypeOfSaitekPanel
        {
            get { return _typeOfSaitekPanel; }
            set { _typeOfSaitekPanel = value; }
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
        public bool KeyboardEmulationOnly
        {
            get { return _keyboardEmulation; }
            set { _keyboardEmulation = value; }
        }

        public bool Closed
        {
            get { return _closed; }
            set { _closed = value; }
        }

        protected static bool FlagHasChanged(byte oldValue, byte newValue, int bitMask)
        {
            /*  --------------------------------------- 
             *  Example #1
             *  Old value 10110101
             *  New value 10110001
             *  Bit mask  00000100  <- This is the one we are interested in to see whether it has changed
             *  ---------------------------------------
             *  
             *  XOR       10110101
             *  ^         10110001
             *            --------
             *            00000100   <- Here are the bit(s) that has changed between old & new value
             *            
             *  AND       00000100
             *  &         00000100
             *            --------
             *            00000100   <- This shows that the value for this mask has changed since last time. Now get what is it (ON/OFF) using FlagValue function
             */

            /*  --------------------------------------- 
             *  Example #2
             *  Old value 10110101
             *  New value 10100101
             *  Bit mask  00000100  <- This is the one we are interested in to see whether it has changed
             *  ---------------------------------------
             *  
             *  XOR       10110101
             *  ^         10100101
             *            --------
             *            00010000   <- Here are the bit(s) that has changed between old & new value
             *            
             *  AND       00010000
             *  &         00000100
             *            --------
             *            00000000   <- This shows that the value for this mask has NOT changed since last time.
             */
            return ((oldValue ^ newValue) & bitMask) > 0;
        }

    }
}
