using System;
using System.Collections.Generic;
using System.Globalization;
using ClassLibraryCommon;
using DCS_BIOS;
using NonVisuals.Interfaces;

namespace NonVisuals
{

    public abstract class GamingPanel : IProfileHandlerListener, IDcsBiosDataListener, IIsDirty
    {
        private int _vendorId;
        private int _productId;
        private Exception _lastException;
        private readonly object _exceptionLockObject = new object();
        private GamingPanelEnum _typeOfGamingPanel;
        private bool _isDirty;
        //private bool _isAttached;
        private bool _forwardPanelEvent;
        private static readonly object LockObject = new object();
        private static readonly List<GamingPanel> GamingPanels = new List<GamingPanel>();
        private bool _settingsLoading = false;
        /*
         * IMPORTANT STUFF
         */
        private readonly DCSBIOSOutput _updateCounterDCSBIOSOutput;
        private static readonly object UpdateCounterLockObject = new object();
        private uint _count;
        private bool _synchedOnce;
        private readonly Guid _guid = Guid.NewGuid();
        //private readonly string _hash;
        public abstract string SettingsVersion();
        public abstract void Startup();
        public abstract void Dispose();
        public abstract void ClearSettings();
        public abstract void ImportSettings(List<string> settings);
        public abstract List<string> ExportSettings();
        public abstract void SavePanelSettings(object sender, ProfileHandlerEventArgs e);
        public abstract void SavePanelSettingsJSON(object sender, ProfileHandlerEventArgs e);
        public abstract void DcsBiosDataReceived(object sender, DCSBIOSDataEventArgs e);
        protected readonly HIDSkeleton HIDSkeletonBase;
        private bool _closed;
        public long ReportCounter = 0;
        
        protected bool FirstReportHasBeenRead = false;
        protected abstract void GamingPanelKnobChanged(bool isFirstReport, IEnumerable<object> hashSet);

        protected abstract void StartListeningForPanelChanges();

        private string _randomPanelHash = "";

        protected GamingPanel(GamingPanelEnum typeOfGamingPanel, HIDSkeleton hidSkeleton)
        {
            _typeOfGamingPanel = typeOfGamingPanel;
            HIDSkeletonBase = hidSkeleton;
            if (Common.IsOperationModeFlagSet(OperationFlag.DCSBIOSOutputEnabled))
            {
                _updateCounterDCSBIOSOutput = DCSBIOSOutput.GetUpdateCounter();
            }
            //_hash = Common.GetMd5Hash(hidSkeleton.InstanceId);
        }


        protected void UpdateCounter(uint address, uint data)
        {
            lock (UpdateCounterLockObject)
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
        /*
        public void SignalPanelChange()
        {
            SetIsDirty();
        }
        */
        public void SetIsDirty()
        {
            SettingsChanged();
            IsDirty = true;
        }

        public virtual void SelectedAirframe(object sender, AirframeEventArgs e) {}

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

        //todo fix that a hash is created for the configuration ONCE, that is only if there are none.

        public string PanelHash
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_randomPanelHash))
                {
                    _randomPanelHash = Common.GetRandomMd5Hash();
                    SetIsDirty();
                }
                return _randomPanelHash;
            }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    _randomPanelHash = Common.GetRandomMd5Hash();
                    SetIsDirty();
                }
                else
                {
                    _randomPanelHash = value;
                }
            }
        }

        //public string Hash => _hash;

        public string GuidString => _guid.ToString();

        public void SetLastException(Exception ex)
        {
            try
            {
                if (ex == null)
                {
                    return;
                }
                Common.LogError( ex, "Via GamingPanel.SetLastException()");
                lock (_exceptionLockObject)
                {
                    _lastException = new Exception(ex.GetType() + Environment.NewLine + ex.Message + Environment.NewLine + ex.StackTrace);
                }
            }
            catch (Exception)
            {
            }
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

        public void StateSaved()
        {
            _isDirty = false;
        }

        public bool SettingsLoading
        {
            get => _settingsLoading;
            set => _settingsLoading = value;
        }

        public GamingPanelEnum TypeOfPanel
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

        //These events can be raised by the descendants of this class.
        public delegate void SwitchesHasBeenChangedEventHandler(object sender, SwitchesChangedEventArgs e);
        public event SwitchesHasBeenChangedEventHandler OnSwitchesChangedA;

        public delegate void PanelDataToDcsAvailableEventHandler(object sender, PanelDataToDCSBIOSEventEventArgs e);
        public event PanelDataToDcsAvailableEventHandler OnPanelDataAvailableA;

        public delegate void DeviceAttachedEventHandler(object sender, PanelEventArgs e);
        public event DeviceAttachedEventHandler OnDeviceAttachedA;

        public delegate void DeviceDetachedEventHandler(object sender, PanelEventArgs e);
        public event DeviceDetachedEventHandler OnDeviceDetachedA;

        public delegate void SettingsHasChangedEventHandler(object sender, PanelEventArgs e);
        public event SettingsHasChangedEventHandler OnSettingsChangedA;

        public delegate void SettingsHasBeenAppliedEventHandler(object sender, PanelEventArgs e);
        public event SettingsHasBeenAppliedEventHandler OnSettingsAppliedA;

        public delegate void SettingsClearedEventHandler(object sender, PanelEventArgs e);
        public event SettingsClearedEventHandler OnSettingsClearedA;

        public delegate void UpdatesHasBeenMissedEventHandler(object sender, DCSBIOSUpdatesMissedEventArgs e);
        public event UpdatesHasBeenMissedEventHandler OnUpdatesHasBeenMissed;

        //For those that wants to listen to this panel
        public virtual void Attach(IGamingPanelListener iGamingPanelListener)
        {
            OnDeviceAttachedA += iGamingPanelListener.DeviceAttached;
            OnSwitchesChangedA += iGamingPanelListener.UISwitchesChanged;
            OnPanelDataAvailableA += iGamingPanelListener.PanelDataAvailable;
            OnSettingsAppliedA += iGamingPanelListener.SettingsApplied;
            OnSettingsClearedA += iGamingPanelListener.SettingsCleared;
            OnUpdatesHasBeenMissed += iGamingPanelListener.UpdatesHasBeenMissed;
            OnSettingsChangedA += iGamingPanelListener.PanelSettingsChanged;
        }

        //For those that wants to listen to this panel
        public virtual void Detach(IGamingPanelListener iGamingPanelListener)
        {
            OnDeviceAttachedA -= iGamingPanelListener.DeviceAttached;
            OnSwitchesChangedA -= iGamingPanelListener.UISwitchesChanged;
            OnPanelDataAvailableA -= iGamingPanelListener.PanelDataAvailable;
            OnSettingsAppliedA -= iGamingPanelListener.SettingsApplied;
            OnSettingsClearedA -= iGamingPanelListener.SettingsCleared;
            OnUpdatesHasBeenMissed -= iGamingPanelListener.UpdatesHasBeenMissed;
            OnSettingsChangedA -= iGamingPanelListener.PanelSettingsChanged;
        }

        //For those that wants to listen to this panel when it's settings change
        public void Attach(IProfileHandlerListener iProfileHandlerListener)
        {
            OnSettingsChangedA += iProfileHandlerListener.PanelSettingsChanged;
        }

        //For those that wants to listen to this panel
        public void Detach(IProfileHandlerListener iProfileHandlerListener)
        {
            OnSettingsChangedA -= iProfileHandlerListener.PanelSettingsChanged;
        }

        //Used by any but descendants that wants to see buttons that have changed, UI for example
        protected virtual void UISwitchesChanged(HashSet<object> hashSet)
        {
            OnSwitchesChangedA?.Invoke(this, new SwitchesChangedEventArgs() { UniqueId = InstanceId, GamingPanelEnum = _typeOfGamingPanel, Switches = hashSet });
        }

        //Used by any but descendants that wants to see buttons that have changed, UI for example
        protected virtual void PanelDataAvailable(string stringData)
        {
            OnPanelDataAvailableA?.Invoke(this, new PanelDataToDCSBIOSEventEventArgs() { StringData = stringData });
        }


        protected virtual void DeviceAttached()
        {
            //IsAttached = true;
            OnDeviceAttachedA?.Invoke(this, new PanelEventArgs() { UniqueId = InstanceId, GamingPanelEnum = _typeOfGamingPanel });
        }


        protected virtual void DeviceDetached()
        {
            //IsAttached = false;
            OnDeviceDetachedA?.Invoke(this, new PanelEventArgs() { UniqueId = InstanceId, GamingPanelEnum = _typeOfGamingPanel });
        }


        protected virtual void SettingsChanged()
        {
            OnSettingsChangedA?.Invoke(this, new PanelEventArgs() { UniqueId = InstanceId, GamingPanelEnum = _typeOfGamingPanel });
        }


        protected virtual void SettingsApplied()
        {
            OnSettingsAppliedA?.Invoke(this, new PanelEventArgs() { UniqueId = InstanceId, GamingPanelEnum = _typeOfGamingPanel });
        }


        protected virtual void SettingsCleared()
        {
            OnSettingsClearedA?.Invoke(this, new PanelEventArgs() { UniqueId = InstanceId, GamingPanelEnum = _typeOfGamingPanel });
        }

        public void PanelSettingsChanged(object sender, PanelEventArgs e) { }

        public void PanelSettingsReadFromFile(object sender, SettingsReadFromFileEventArgs e)
        {
            ClearPanelSettings(this);
            ImportSettings(e.Settings);
        }

        public void ClearPanelSettings(object sender)
        {
            ClearSettings();
            SettingsCleared();
        }

    }

    public class DCSBIOSUpdatesMissedEventArgs : EventArgs
    {
        public string UniqueId { get; set; }
        public GamingPanelEnum GamingPanelEnum { get; set; }
        public int Count { get; set; }
    }

    public class PanelEventArgs : EventArgs
    {
        public string UniqueId { get; set; }
        public GamingPanelEnum GamingPanelEnum { get; set; }
    }

    public class PanelDataToDCSBIOSEventEventArgs : EventArgs
    {
        public string StringData { get; set; }
    }

    public class SwitchesChangedEventArgs : EventArgs
    {
        public string UniqueId { get; set; }
        public GamingPanelEnum GamingPanelEnum { get; set; }
        public HashSet<object> Switches { get; set; }
    }

    public class ForwardPanelEventArgs : EventArgs
    {
        public bool Forward { get; set; }
    }
}
