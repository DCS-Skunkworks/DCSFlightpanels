namespace NonVisuals
{
    using System;
    using System.Collections.Generic;
    using ClassLibraryCommon;

    using DCS_BIOS;
    using DCS_BIOS.EventArgs;
    using DCS_BIOS.Interfaces;
    using NLog;
    using NonVisuals.EventArgs;
    using NonVisuals.Interfaces;

    public abstract class GamingPanel : IProfileHandlerListener, IDcsBiosDataListener, IIsDirty, IDisposable
    {
        internal AppEventHandler _appEventHandler;
        internal static Logger logger = LogManager.GetCurrentClassLogger();

        public abstract void Startup();
        public abstract void Identify();
        public abstract void ClearSettings(bool setIsDirty = false);
        public abstract void ImportSettings(GenericPanelBinding genericPanelBinding);
        public abstract List<string> ExportSettings();
        public abstract void SavePanelSettings(object sender, ProfileHandlerEventArgs e);
        public abstract void SavePanelSettingsJSON(object sender, ProfileHandlerEventArgs e);
        public abstract void DcsBiosDataReceived(object sender, DCSBIOSDataEventArgs e);
        protected abstract void GamingPanelKnobChanged(bool isFirstReport, IEnumerable<object> hashSet);
        protected abstract void StartListeningForHidPanelChanges();

        private readonly DCSBIOSOutput _updateCounterDCSBIOSOutput;
        private static readonly object UpdateCounterLockObject = new object();
        private readonly object _exceptionLockObject = new object();
        private Exception _lastException;
        private string _randomBindingHash = string.Empty;
        private uint _count;
        private bool _synchedOnce;
        protected bool Closed { get; set; }
        protected bool FirstReportHasBeenRead = false;
        protected readonly HIDSkeleton HIDSkeletonBase;
        public bool IsDirty { get; set; }
        public bool SettingsLoading { get; set; }
        public GamingPanelEnum TypeOfPanel { get; set; }
        public bool ForwardPanelEvent { get; set; }
        public int VendorId { get; set; }
        public int ProductId { get; set; }
        public static readonly List<GamingPanel> GamingPanels = new List<GamingPanel>(); 

        public string HIDInstance
        {
            get => HIDSkeletonBase.HIDInstance;
            set => HIDSkeletonBase.HIDInstance = value;
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

        protected GamingPanel(GamingPanelEnum typeOfGamingPanel, HIDSkeleton hidSkeleton, AppEventHandler appEventHandler)
        {
            _appEventHandler = appEventHandler;
            TypeOfPanel = typeOfGamingPanel;
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

            _appEventHandler.AttachForwardPanelEventListener(this);
            _appEventHandler.AttachSettingsConsumerListener(this);
        }

        public void Dispose()
        {
            // Dispose of unmanaged resources.
            Dispose(true);
            // Suppress finalization.
            GC.SuppressFinalize(this);
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
                _appEventHandler.DetachForwardPanelEventListener(this);
                _appEventHandler.DetachSettingsConsumerListener(this);
                _appEventHandler.PanelEvent(this, HIDSkeletonBase.HIDInstance, HIDSkeletonBase, PanelEventType.Disposed);
            }

            _disposed = true;
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
                        _appEventHandler.UpdatesMissed(this, HIDSkeletonBase.HIDInstance, TypeOfPanel, (int)(newCount - _count));
                        _count = newCount;
                    }
                }
            }
        }

        public void SetIsDirty()
        {
            _appEventHandler.SettingsChanged(this, HIDInstance, TypeOfPanel);
            IsDirty = true;
        }
        

        // User can choose not to in case switches needs to be reset but not affect the airframe. E.g. after crashing.
        public void SetForwardPanelEvent(object sender, ForwardPanelEventArgs e)
        {
            ForwardPanelEvent = e.Forward;
        }

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

        public void StateSaved()
        {
            IsDirty = false;
        }

        public void ProfileEvent(object sender, ProfileEventArgs e)
        {
            if (e.ProfileEventType == ProfileEventEnum.ProfileSettings && e.PanelBinding.Match(HIDSkeletonBase))
            {
                ImportSettings(e.PanelBinding);
            }
        }
    }
}
