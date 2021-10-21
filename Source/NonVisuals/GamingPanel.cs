namespace NonVisuals
{
    using System;
    using System.Collections.Generic;
    using System.Windows;

    using ClassLibraryCommon;

    using DCS_BIOS;
    using DCS_BIOS.EventArgs;
    using DCS_BIOS.Interfaces;
    using NLog;
    using NonVisuals.EventArgs;
    using NonVisuals.Interfaces;

    public abstract class GamingPanel : IProfileHandlerListener, IDcsBiosDataListener, IIsDirty
    {
        internal static Logger logger = LogManager.GetCurrentClassLogger();
        private readonly DCSBIOSOutput _updateCounterDCSBIOSOutput;
        private readonly Guid _guid = Guid.NewGuid();
        private static readonly object UpdateCounterLockObject = new object();
        private static readonly object LockObject = new object();
        private readonly object _exceptionLockObject = new object();
        public static readonly List<GamingPanel> GamingPanels = new List<GamingPanel>(); // TODO REMOVE PUBLIC

        private Exception _lastException;

        private bool _isDirty;

        private uint _count;

        private bool _synchedOnce;

        public abstract void Startup();

        public abstract void Identify();

        public abstract void Dispose();

        public abstract void ClearSettings(bool setIsDirty = false);

        public abstract void ImportSettings(GenericPanelBinding genericPanelBinding);

        public abstract List<string> ExportSettings();

        public abstract void SavePanelSettings(object sender, ProfileHandlerEventArgs e);

        public abstract void SavePanelSettingsJSON(object sender, ProfileHandlerEventArgs e);

        public abstract void DcsBiosDataReceived(object sender, DCSBIOSDataEventArgs e);

        protected readonly HIDSkeleton HIDSkeletonBase;

        public long ReportCounter = 0;

        protected bool FirstReportHasBeenRead = false;

        protected abstract void GamingPanelKnobChanged(bool isFirstReport, IEnumerable<object> hashSet);

        protected abstract void StartListeningForPanelChanges();

        private string _randomBindingHash = string.Empty;

        protected GamingPanel(GamingPanelEnum typeOfGamingPanel, HIDSkeleton hidSkeleton)
        {
            this.TypeOfPanel = typeOfGamingPanel;
            HIDSkeletonBase = hidSkeleton;
            if (Common.IsEmulationModesFlagSet(EmulationMode.DCSBIOSOutputEnabled))
            {
                try
                {
                    _updateCounterDCSBIOSOutput = DCSBIOSOutput.GetUpdateCounter();
                }
                catch (Exception)
                {
                    // ignore
                }
            }

            GamingPanels.Add(this);

            if (hidSkeleton.HIDReadDevice != null)
            {
                hidSkeleton.HIDReadDevice.Inserted += DeviceInsertedHandler;
            }
        }

        public void DeviceInsertedHandler()
        {
            /*
             * Not working, hidSkeleton deleted when panel is removed => no instance where this can be executed on. Regardless, restarting isn't a big of a deal.
             */
            MessageBox.Show("New device has been detected. Restart DCSFP to take it into use", "New hardware detected", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        protected void UpdateCounter(uint address, uint data)
        {
            lock (UpdateCounterLockObject)
            {
                if (_updateCounterDCSBIOSOutput != null && _updateCounterDCSBIOSOutput.Address == address)
                {
                    var newCount = _updateCounterDCSBIOSOutput.GetUIntValue(data);
                    if (!_synchedOnce)
                    {
                        _count = newCount;
                        _synchedOnce = true;
                        return;
                    }

                    // Max is 255
                    if ((newCount == 0 && _count == 255) || newCount - _count == 1)
                    {
                        // All is well
                        _count = newCount;
                    }
                    else if (newCount - _count != 1)
                    {
                        // Not good
                        if (OnUpdatesHasBeenMissed != null)
                        {
                            OnUpdatesHasBeenMissed(
                                this,
                                new DCSBIOSUpdatesMissedEventArgs { HidInstance = HIDSkeletonBase.InstanceId, GamingPanelEnum = this.TypeOfPanel, Count = (int)(newCount - _count) });
                            _count = newCount;
                        }
                    }
                }
            }
        }
        
        public void SetIsDirty()
        {
            SettingsChanged();
            IsDirty = true;
        }

        public virtual void SelectedProfile(object sender, AirframeEventArgs e)
        {
        }

        // User can choose not to in case switches needs to be reset but not affect the airframe. E.g. after crashing.
        public void SetForwardKeyPresses(object sender, ForwardPanelEventArgs e)
        {
            ForwardPanelEvent = e.Forward;
        }

        public bool ForwardPanelEvent { get; set; }

        public int VendorId { get; set; }

        public int ProductId { get; set; }

        public string HIDInstanceId
        {
            get => HIDSkeletonBase.InstanceId;
            set => HIDSkeletonBase.InstanceId = value;
        }

        public string BindingHash
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_randomBindingHash))
                {
                    _randomBindingHash = Common.GetRandomMd5Hash();
                }

                return _randomBindingHash;
            }

            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    _randomBindingHash = Common.GetRandomMd5Hash();
                    SetIsDirty();
                }
                else
                {
                    _randomBindingHash = value;
                }
            }
        }

        // public string Hash => _hash;
        public string GuidString => _guid.ToString();

        protected void SetLastException(Exception ex)
        {
            try
            {
                if (ex == null)
                {
                    return;
                }

                logger.Error(ex, "Via GamingPanel.SetLastException()");
                lock (_exceptionLockObject)
                {
                    _lastException = new Exception(ex.GetType() + Environment.NewLine + ex.Message + Environment.NewLine + ex.StackTrace);
                }
            }
            catch (Exception)
            {
                // ignored
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

        public bool SettingsLoading { get; set; }

        public GamingPanelEnum TypeOfPanel { get; set; }

        // TODO fixa att man kan koppla in/ur panelerna?
        /*
         * 
        
        public bool IsAttached
        {
            get { return _isAttached; }
            set { _isAttached = value; }
        }
        */
        protected bool Closed { get; set; }

        // These events can be raised by the descendants of this class.
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

        // For those that wants to listen to this panel
        public virtual void Attach(IGamingPanelListener gamingPanelListener)
        {
            OnDeviceAttachedA += gamingPanelListener.DeviceAttached;
            OnSwitchesChangedA += gamingPanelListener.UISwitchesChanged;
            OnPanelDataAvailableA += gamingPanelListener.PanelDataAvailable;
            OnSettingsAppliedA += gamingPanelListener.SettingsApplied;
            OnSettingsClearedA += gamingPanelListener.SettingsCleared;
            OnUpdatesHasBeenMissed += gamingPanelListener.UpdatesHasBeenMissed;
            OnSettingsChangedA += gamingPanelListener.PanelSettingsChanged;
        }

        // For those that wants to listen to this panel
        public virtual void Detach(IGamingPanelListener gamingPanelListener)
        {
            OnDeviceAttachedA -= gamingPanelListener.DeviceAttached;
            OnSwitchesChangedA -= gamingPanelListener.UISwitchesChanged;
            OnPanelDataAvailableA -= gamingPanelListener.PanelDataAvailable;
            OnSettingsAppliedA -= gamingPanelListener.SettingsApplied;
            OnSettingsClearedA -= gamingPanelListener.SettingsCleared;
            OnUpdatesHasBeenMissed -= gamingPanelListener.UpdatesHasBeenMissed;
            OnSettingsChangedA -= gamingPanelListener.PanelSettingsChanged;
        }

        // For those that wants to listen to this panel when it's settings change
        public void Attach(IProfileHandlerListener profileHandlerListener)
        {
            OnSettingsChangedA += profileHandlerListener.PanelSettingsChanged;
        }

        // For those that wants to listen to this panel
        public void Detach(IProfileHandlerListener profileHandlerListener)
        {
            OnSettingsChangedA -= profileHandlerListener.PanelSettingsChanged;
        }

        // Used by any but descendants that wants to see buttons that have changed, UI for example
        protected virtual void UISwitchesChanged(HashSet<object> hashSet)
        {
            OnSwitchesChangedA?.Invoke(this, new SwitchesChangedEventArgs { HidInstance = HIDInstanceId, GamingPanelEnum = this.TypeOfPanel, Switches = hashSet });
        }

        // Used by any but descendants that wants to see buttons that have changed, UI for example
        protected virtual void PanelDataAvailable(string stringData)
        {
            OnPanelDataAvailableA?.Invoke(this, new PanelDataToDCSBIOSEventEventArgs { StringData = stringData });
        }

        protected virtual void DeviceAttached()
        {
            // IsAttached = true;
            OnDeviceAttachedA?.Invoke(this, new PanelEventArgs { HidInstance = HIDInstanceId, PanelType = this.TypeOfPanel });
        }

        protected virtual void DeviceDetached()
        {
            // IsAttached = false;
            OnDeviceDetachedA?.Invoke(this, new PanelEventArgs { HidInstance = HIDInstanceId, PanelType = this.TypeOfPanel });
        }

        protected virtual void SettingsChanged()
        {
            OnSettingsChangedA?.Invoke(this, new PanelEventArgs { HidInstance = HIDInstanceId, PanelType = this.TypeOfPanel });
        }

        protected virtual void SettingsApplied()
        {
            OnSettingsAppliedA?.Invoke(this, new PanelEventArgs { HidInstance = HIDInstanceId, PanelType = this.TypeOfPanel });
        }

        protected virtual void SettingsCleared()
        {
            OnSettingsClearedA?.Invoke(this, new PanelEventArgs { HidInstance = HIDInstanceId, PanelType = this.TypeOfPanel });
        }

        public void PanelSettingsChanged(object sender, PanelEventArgs e)
        {
        }

        public void PanelBindingReadFromFile(object sender, PanelBindingReadFromFileEventArgs e)
        {
            if (e.PanelBinding.HIDInstance == HIDInstanceId)
            {
                ImportSettings(e.PanelBinding);
            }
        }

        public void ClearPanelSettings(object sender)
        {
            ClearSettings();
            SettingsCleared();
        }
    }
}
