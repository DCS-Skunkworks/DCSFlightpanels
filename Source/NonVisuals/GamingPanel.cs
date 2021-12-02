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

    public abstract class GamingPanel : IProfileHandlerListener, IDcsBiosDataListener, IIsDirty, IDisposable
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

            AppEventHandler.AttachForwardPanelEventListener(this);
            AppEventHandler.AttachSettingsConsumerListener(this);
            BIOSEventHandler.AttachDataListener(this);
        }

        private bool _disposed;
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                Closed = true; // Don't know if this is necessary atm. (2021)
                AppEventHandler.DetachForwardPanelEventListener(this);
                AppEventHandler.DetachSettingsConsumerListener(this);
                BIOSEventHandler.DetachDataListener(this);
            }

            _disposed = true;
        }

        public void Dispose()
        {
            // Dispose of unmanaged resources.
            Dispose(true);
            // Suppress finalization.
            GC.SuppressFinalize(this);
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
                        AppEventHandler.UpdatesMissed(this, HIDSkeletonBase.InstanceId, TypeOfPanel, (int)(newCount - _count));
                        _count = newCount;
                    }
                }
            }
        }


        public virtual void ProfileSelected(object sender, AirframeEventArgs e)
        {
        }

        // User can choose not to in case switches needs to be reset but not affect the airframe. E.g. after crashing.
        public void SetForwardPanelEvent(object sender, ForwardPanelEventArgs e)
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


        public void SetIsDirty()
        {
            IsDirty = true;
        }

        public bool IsDirty
        {
            get => _isDirty;
            set
            {
                _isDirty = value;
                if (_isDirty)
                {
                    AppEventHandler.SettingsChanged(this, HIDInstanceId, TypeOfPanel);
                }
            }
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
            AppEventHandler.SettingsCleared(this, HIDSkeletonBase.InstanceId, TypeOfPanel);
        }
    }
}
