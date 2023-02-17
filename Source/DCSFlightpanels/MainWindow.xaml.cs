namespace DCSFlightpanels
{
    /*
    Custom Resharper Naming abbreviations
    ADF AJS ALL ALT APR BIOS BIP BIPS COM CRS DB DCS DCSBIOS DCSBIOSJSON DME DRO HDG HF IAS ICS IFF ILS IP IX JSON KEYS LCD LCDPZ LE LED NADIR NAV OS PZ REV SA SRS TACAN TPM UH UHF USB VHF VID VS XPDR XY ZY ARC ARN APX ABRIS OK ID FA ZA AV8BNA COMM NS DCSFP
    */
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Media;
    using System.Reflection;
    using System.Timers;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Navigation;

    using ClassLibraryCommon;

    using DCS_BIOS;
    using DCS_BIOS.EventArgs;
    using DCS_BIOS.Interfaces;

    using Interfaces;
    using PanelUserControls;
    using Properties;
    using Radios.Emulators;
    using Radios.PreProgrammed;
    using Shared;
    using Windows;
    using NLog;
    using NLog.Targets;
    using NLog.Targets.Wrappers;
    using NonVisuals;
    using NonVisuals.EventArgs;
    using NonVisuals.Interfaces;
    using NonVisuals.Plugin;
    using NonVisuals.Radios.SRS;

    using Octokit;
    using NonVisuals.Panels;
    using NonVisuals.HID;
    using System.Windows.Media.Imaging;

    public partial class MainWindow : IGamingPanelListener, IDcsBiosConnectionListener, ISettingsModifiedListener, IProfileHandlerListener, IDisposable, IHardwareConflictResolver, IPanelEventListener, IForwardPanelEventListener
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly List<KeyValuePair<string, GamingPanelEnum>> _profileFileHIDInstances = new();
        private readonly string _windowName = "DCSFlightpanels ";
        private readonly Timer _exceptionTimer = new(1000);
        private readonly Timer _statusMessagesTimer = new(1000);
        private readonly Timer _dcsStopGearTimer = new(5000);
        private readonly List<string> _statusMessages = new();
        private readonly object _lockObjectStatusMessages = new();
        private readonly List<UserControl> _panelUserControls = new();

        private ProfileHandler _profileHandler;
        private DCSBIOS _dcsBios;
        private bool _disablePanelEventsFromBeingRouted;
        private bool _isLoaded;

        public MainWindow()
        {
            if (Settings.Default.UpgradeRequired)
            {
                Settings.Default.Upgrade();
                Settings.Default.UpgradeRequired = false;
                Settings.Default.Save();
            }

            DarkModePrepare();
            InitializeComponent();

            if (DarkMode.DarkModeEnabled)
            {
                ImageDcsBiosConnected.Source = new BitmapImage(new Uri(@"/dcsfp;component/Images/gear-image-darkmode.png", UriKind.Relative));
            }

            DCSAircraft.Init();

            // Stop annoying "Cannot find source for binding with reference .... " from being shown
            PresentationTraceSources.DataBindingSource.Switch.Level = SourceLevels.Critical;

            AppEventHandler.AttachSettingsMonitoringListener(this);
            AppEventHandler.AttachSettingsModified(this);
            AppEventHandler.AttachPanelEventListener(this);
            AppEventHandler.AttachForwardPanelEventListener(this);
            BIOSEventHandler.AttachConnectionListener(this);
        }

        private void DarkModePrepare()
        {
            DarkMode.DarkModeEnabled = Settings.Default.DarkMode;
        }

        #region IDisposable Support
        private bool _hasBeenCalledAlready; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!_hasBeenCalledAlready)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    _dcsStopGearTimer.Dispose();
                    _exceptionTimer.Dispose();
                    _statusMessagesTimer.Dispose();
                    _exceptionTimer.Dispose();
                    _dcsBios?.Dispose();
                    AppEventHandler.DetachPanelEventListener(this);
                    AppEventHandler.DetachSettingsMonitoringListener(this);
                    AppEventHandler.DetachSettingsModified(this); 
                    AppEventHandler.DetachForwardPanelEventListener(this);
                    BIOSEventHandler.DetachConnectionListener(this);
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.

                // TODO: set large fields to null.
                _hasBeenCalledAlready = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~MainWindow() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);

            // TODO: uncomment the following line if the finalizer is overridden above.
            GC.SuppressFinalize(this);
        }
        #endregion


        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            DarkMode.SetFrameworkElemenDarkMode(this);
            try
            {
                if (_isLoaded)
                {
                    return;
                }

                if (Settings.Default.RunMinimized)
                {
                    WindowState = WindowState.Minimized;
                }

                LoadSettings();

                DCSAircraft.FillModulesListFromDcsBios(DCSBIOSCommon.GetDCSBIOSJSONDirectory(Settings.Default.DCSBiosJSONLocation));

                StartTimers();

                _profileHandler = new ProfileHandler(Settings.Default.DCSBiosJSONLocation, Settings.Default.LastProfileFileUsed, this);
                _profileHandler.Init();

                SetWindowTitle();
                SetWindowState();

                CheckForNewDCSFPRelease();

                ConfigurePlugins();

                _profileHandler.FindProfile();

                LabelDonate.IsEnabled = true;

                if (Settings.Default.ShowKeyAPIDialog)
                {
                    var apiWindow = new WindowsKeyAPIDialog();
                    apiWindow.ShowDialog();
                    Settings.Default.ShowKeyAPIDialog = apiWindow.ShowAtStartUp;
                }

                _isLoaded = true;

            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void StartTimers()
        {
            _exceptionTimer.Elapsed += TimerCheckExceptions;
            _exceptionTimer.Start();
            _dcsStopGearTimer.Elapsed += TimerStopRotation;
            _statusMessagesTimer.Elapsed += TimerStatusMessagesTimer;
            _statusMessagesTimer.Start();
        }

        /// <summary>
        /// Try to find the path of the log with a file target given as parameter
        /// See NLog.config in the main folder of the application for configured log targets
        /// </summary>
        private static string GetLogFilePathByTarget(string targetName)
        {
            string fileName;
            if (LogManager.Configuration != null && LogManager.Configuration.ConfiguredNamedTargets.Count != 0)
            {
                Target target = LogManager.Configuration.FindTargetByName(targetName);
                if (target == null)
                {
                    throw new Exception($"Could not find log with a target named: [{targetName}]. See NLog.config for configured targets");
                }

                FileTarget fileTarget;

                // Unwrap the target if necessary.
                if (target is not WrapperTargetBase wrapperTarget)
                {
                    fileTarget = target as FileTarget;
                }
                else
                {
                    fileTarget = wrapperTarget.WrappedTarget as FileTarget;
                }

                if (fileTarget == null)
                {
                    throw new Exception($"Could not get a FileTarget type log from {target.GetType()}");
                }

                var logEventInfo = new LogEventInfo { TimeStamp = DateTime.Now };
                fileName = fileTarget.FileName.Render(logEventInfo);
            }
            else
            {
                throw new Exception("LogManager contains no configuration or there are no named targets. See NLog.config file to configure the logs.");
            }
            return fileName;
        }

        private void SetApplicationMode()
        {
            var dcsAircraft = _profileHandler.DCSAircraft;

            if (!IsLoaded)
            {
                return;
            }

            LabelAirframe.Content = DCSAircraft.IsNoFrameLoadedYet(dcsAircraft) ? string.Empty : dcsAircraft.Description;

            if (DCSAircraft.IsNoFrameLoadedYet(dcsAircraft) || Common.IsEmulationModesFlagSet(EmulationMode.KeyboardEmulationOnly))
            {
                ShutdownDCSBIOS();
            }
            else if (!DCSAircraft.IsNoFrameLoadedYet(dcsAircraft))
            {
                CreateDCSBIOS();
                StartupDCSBIOS();
            }
        }

        private int DisposePanels()
        {
            var closedItemCount = 0;
            try
            {
                if (TabControlPanels.Items.Count > 0)
                {
                    do
                    {
                        var tabItem = (TabItem)TabControlPanels.Items.GetItemAt(0);
                        var userControl = (UserControlBase)tabItem.Content;
                        TabControlPanels.Items.Remove(tabItem);
                        var gamingPanelUserControl = (IGamingPanelUserControl)tabItem.Content;
                        var gamingPanel = gamingPanelUserControl.GetGamingPanel();

                        if (gamingPanel != null)
                        {
                            userControl.Dispose();
                            _panelUserControls.Remove(userControl);
                            closedItemCount++;
                        }
                    }
                    while (TabControlPanels.Items.Count > 0);

                    _profileFileHIDInstances.Clear();
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }

            return closedItemCount;
        }

        public void UpdatesHasBeenMissed(object sender, DCSBIOSUpdatesMissedEventArgs e)
        {
            try
            {
                var message = $"DCS-BIOS UPDATES MISSED = {e.GamingPanelEnum} {e.Count}";
                ShowStatusBarMessage(message);
                Logger.Error(message);
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void CreateDCSBIOS()
        {
            if (_dcsBios != null)
            {
                return;
            }

            DCSBIOSControlLocator.DCSAircraft = _profileHandler.DCSAircraft;

            _dcsBios = new DCSBIOS(Settings.Default.DCSBiosIPFrom, Settings.Default.DCSBiosIPTo, int.Parse(Settings.Default.DCSBiosPortFrom), int.Parse(Settings.Default.DCSBiosPortTo), DcsBiosNotificationMode.AddressValue);
            if (!_dcsBios.HasLastException())
            {
                RotateGear(2000);
            }

            ImageDcsBiosConnected.Visibility = Visibility.Visible;
        }

        private void StartupDCSBIOS()
        {
            if (_dcsBios.IsRunning)
            {
                return;
            }

            _dcsBios?.Startup();

            _dcsStopGearTimer.Start();
        }

        private void ShutdownDCSBIOS()
        {
            _dcsBios?.Shutdown();
            _dcsBios = null;

            DCSBIOSControlLocator.DCSAircraft = _profileHandler.DCSAircraft;
            _dcsStopGearTimer.Stop();
            ImageDcsBiosConnected.Visibility = Visibility.Collapsed;
        }

        private void DisposePanel(HIDSkeleton hidSkeleton)
        {
            void Action()
            {
                for (var i = 0; i < TabControlPanels.Items.Count; i++)
                {
                    var tabItem = (TabItem)TabControlPanels.Items.GetItemAt(i);
                    var userControl = (IGamingPanelUserControl)tabItem.Content;

                    if (userControl.GetGamingPanel().HIDInstance.Equals(hidSkeleton.HIDInstance))
                    {
                        userControl.Dispose();
                        TabControlPanels.Items.RemoveAt(i);
                        AppEventHandler.PanelEvent(this, hidSkeleton.HIDInstance, hidSkeleton, PanelEventType.Disposed);
                        break;
                    }
                }
            }

            Dispatcher?.BeginInvoke((Action)Action);
        }

        private void CreatePanel(HIDSkeleton hidSkeleton)
        {
            try
            {
                if (!hidSkeleton.IsAttached)
                {
                    return;
                }

                switch (hidSkeleton.GamingPanelType)
                {
                    case GamingPanelEnum.CDU737:
                        {
                            var tabItem = new TabItem { Header = "CDU 737" };

                            IGamingPanelUserControl panel = UserControlBaseFactoryHelpers.GetUSerControl(GamingPanelEnum.CDU737, 
                                                _profileHandler.DCSAircraft, 
                                                hidSkeleton, tabItem);
                            if (panel != null)
                            {
                                _panelUserControls.Add((UserControl )panel);
                                tabItem.Content = panel;
                                TabControlPanels.Items.Add(tabItem);

                                _profileFileHIDInstances
                                    .Add(new KeyValuePair<string, GamingPanelEnum>(hidSkeleton.HIDInstance, hidSkeleton.GamingPanelType));
                                AppEventHandler.PanelEvent(this, hidSkeleton.HIDInstance, hidSkeleton, PanelEventType.Created);
                            }
                           
                            break;


                        }
                    case GamingPanelEnum.PZ55SwitchPanel:
                        {
                            var tabItem = new TabItem { Header = "PZ55" };
                            var switchPanelPZ55UserControl = new SwitchPanelPZ55UserControl(hidSkeleton);
                            _panelUserControls.Add(switchPanelPZ55UserControl);
                            tabItem.Content = switchPanelPZ55UserControl;
                            TabControlPanels.Items.Add(tabItem);
                            _profileFileHIDInstances.Add(new KeyValuePair<string, GamingPanelEnum>(hidSkeleton.HIDInstance, hidSkeleton.GamingPanelType));

                            AppEventHandler.PanelEvent(this, hidSkeleton.HIDInstance, hidSkeleton, PanelEventType.Created);
                            break;
                        }

                    case GamingPanelEnum.PZ70MultiPanel:
                        {
                            var tabItem = new TabItem { Header = "PZ70" };
                            var multiPanelUserControl = new MultiPanelUserControl(hidSkeleton);
                            _panelUserControls.Add(multiPanelUserControl);
                            tabItem.Content = multiPanelUserControl;
                            TabControlPanels.Items.Add(tabItem);
                            _profileFileHIDInstances.Add(new KeyValuePair<string, GamingPanelEnum>(hidSkeleton.HIDInstance, hidSkeleton.GamingPanelType));

                            AppEventHandler.PanelEvent(this, hidSkeleton.HIDInstance, hidSkeleton, PanelEventType.Created);
                            break;
                        }

                    case GamingPanelEnum.BackLitPanel:
                        {
                            var tabItem = new TabItem { Header = "B.I.P." };
                            var backLitPanelUserControl = new BackLitPanelUserControl(hidSkeleton);
                            _panelUserControls.Add(backLitPanelUserControl);
                            tabItem.Content = backLitPanelUserControl;
                            TabControlPanels.Items.Add(tabItem);
                            _profileFileHIDInstances.Add(new KeyValuePair<string, GamingPanelEnum>(hidSkeleton.HIDInstance, hidSkeleton.GamingPanelType));

                            AppEventHandler.PanelEvent(this, hidSkeleton.HIDInstance, hidSkeleton, PanelEventType.Created);
                            break;
                        }

                    case GamingPanelEnum.TPM:
                        {
                            var tabItem = new TabItem { Header = "TPM" };
                            var tpmPanelUserControl = new TPMPanelUserControl(hidSkeleton);
                            _panelUserControls.Add(tpmPanelUserControl);
                            tabItem.Content = tpmPanelUserControl;
                            TabControlPanels.Items.Add(tabItem);
                            _profileFileHIDInstances.Add(new KeyValuePair<string, GamingPanelEnum>(hidSkeleton.HIDInstance, hidSkeleton.GamingPanelType));

                            AppEventHandler.PanelEvent(this, hidSkeleton.HIDInstance, hidSkeleton, PanelEventType.Created);
                            break;
                        }

                    case GamingPanelEnum.StreamDeckMini:
                    case GamingPanelEnum.StreamDeckXL:
                    case GamingPanelEnum.StreamDeck:
                    case GamingPanelEnum.StreamDeckV2:
                    case GamingPanelEnum.StreamDeckMK2:
                        {
                            var tabItemStreamDeck = new TabItem { Header = hidSkeleton.GamingPanelType.GetEnumDescriptionField() };
                            var streamDeckUserControl = new StreamDeckUserControl(hidSkeleton.GamingPanelType, hidSkeleton);
                            _panelUserControls.Add(streamDeckUserControl);
                            tabItemStreamDeck.Content = streamDeckUserControl;
                            TabControlPanels.Items.Add(tabItemStreamDeck);
                            _profileFileHIDInstances.Add(new KeyValuePair<string, GamingPanelEnum>(hidSkeleton.HIDInstance, hidSkeleton.GamingPanelType));

                            AppEventHandler.PanelEvent(this, hidSkeleton.HIDInstance, hidSkeleton, PanelEventType.Created);

                            break;
                        }

                    case GamingPanelEnum.FarmingPanel:
                        {
                            var tabItem = new TabItem { Header = "Side Panel" };
                            var farmingSidePanelUserControl = new FarmingPanelUserControl(hidSkeleton);
                            _panelUserControls.Add(farmingSidePanelUserControl);
                            tabItem.Content = farmingSidePanelUserControl;
                            TabControlPanels.Items.Add(tabItem);
                            _profileFileHIDInstances.Add(new KeyValuePair<string, GamingPanelEnum>(hidSkeleton.HIDInstance, hidSkeleton.GamingPanelType));

                            AppEventHandler.PanelEvent(this, hidSkeleton.HIDInstance, hidSkeleton, PanelEventType.Created);
                            break;
                        }

                    case GamingPanelEnum.PZ69RadioPanel:
                        {
                            var tabItem = new TabItem { Header = "PZ69" };
                            if (DCSAircraft.IsKeyEmulator(_profileHandler.DCSAircraft))
                            {
                                var radioPanelPZ69UserControl = new RadioPanelPZ69UserControlEmulator(hidSkeleton);
                                _panelUserControls.Add(radioPanelPZ69UserControl);
                                tabItem.Content = radioPanelPZ69UserControl;
                                TabControlPanels.Items.Add(tabItem);
                                _profileFileHIDInstances.Add(new KeyValuePair<string, GamingPanelEnum>(hidSkeleton.HIDInstance, hidSkeleton.GamingPanelType));

                                AppEventHandler.PanelEvent(this, hidSkeleton.HIDInstance, hidSkeleton, PanelEventType.Created);
                            }
                            else if (Common.IsEmulationModesFlagSet(EmulationMode.SRSEnabled) || DCSAircraft.IsFlamingCliff(_profileHandler.DCSAircraft))
                            {
                                var radioPanelPZ69UserControl = new RadioPanelPZ69UserControlSRS(hidSkeleton);
                                _panelUserControls.Add(radioPanelPZ69UserControl);
                                tabItem.Content = radioPanelPZ69UserControl;
                                TabControlPanels.Items.Add(tabItem);
                                _profileFileHIDInstances.Add(new KeyValuePair<string, GamingPanelEnum>(hidSkeleton.HIDReadDevice.DevicePath, hidSkeleton.PanelInfo.GamingPanelType));
                                AppEventHandler.PanelEvent(this, hidSkeleton.HIDInstance, hidSkeleton, PanelEventType.Created);
                            }
                            else if (DCSAircraft.IsA10C(_profileHandler.DCSAircraft) && !_profileHandler.DCSAircraft.UseGenericRadio)
                            {
                                var radioPanelPZ69UserControl = new RadioPanelPZ69UserControlA10C(hidSkeleton);
                                _panelUserControls.Add(radioPanelPZ69UserControl);
                                tabItem.Content = radioPanelPZ69UserControl;
                                TabControlPanels.Items.Add(tabItem);
                                _profileFileHIDInstances.Add(new KeyValuePair<string, GamingPanelEnum>(hidSkeleton.HIDInstance, hidSkeleton.GamingPanelType));

                                AppEventHandler.PanelEvent(this, hidSkeleton.HIDInstance, hidSkeleton, PanelEventType.Created);
                            }
                            else if (DCSAircraft.IsUH1H(_profileHandler.DCSAircraft) && !_profileHandler.DCSAircraft.UseGenericRadio)
                            {
                                var radioPanelPZ69UserControl = new RadioPanelPZ69UserControlUH1H(hidSkeleton);
                                _panelUserControls.Add(radioPanelPZ69UserControl);
                                tabItem.Content = radioPanelPZ69UserControl;
                                TabControlPanels.Items.Add(tabItem);
                                _profileFileHIDInstances.Add(new KeyValuePair<string, GamingPanelEnum>(hidSkeleton.HIDInstance, hidSkeleton.GamingPanelType));

                                AppEventHandler.PanelEvent(this, hidSkeleton.HIDInstance, hidSkeleton, PanelEventType.Created);
                            }
                            else if (DCSAircraft.IsMiG21Bis(_profileHandler.DCSAircraft) && !_profileHandler.DCSAircraft.UseGenericRadio)
                            {
                                var radioPanelPZ69UserControl = new RadioPanelPZ69UserControlMiG21Bis(hidSkeleton);
                                _panelUserControls.Add(radioPanelPZ69UserControl);
                                tabItem.Content = radioPanelPZ69UserControl;
                                TabControlPanels.Items.Add(tabItem);
                                _profileFileHIDInstances.Add(new KeyValuePair<string, GamingPanelEnum>(hidSkeleton.HIDInstance, hidSkeleton.GamingPanelType));

                                AppEventHandler.PanelEvent(this, hidSkeleton.HIDInstance, hidSkeleton, PanelEventType.Created);
                            }
                            else if (DCSAircraft.IsKa50(_profileHandler.DCSAircraft) && !_profileHandler.DCSAircraft.UseGenericRadio)
                            {
                                var radioPanelPZ69UserControl = new RadioPanelPZ69UserControlKa50(hidSkeleton);
                                _panelUserControls.Add(radioPanelPZ69UserControl);
                                tabItem.Content = radioPanelPZ69UserControl;
                                TabControlPanels.Items.Add(tabItem);
                                _profileFileHIDInstances.Add(new KeyValuePair<string, GamingPanelEnum>(hidSkeleton.HIDInstance, hidSkeleton.GamingPanelType));

                                AppEventHandler.PanelEvent(this, hidSkeleton.HIDInstance, hidSkeleton, PanelEventType.Created);
                            }
                            else if (DCSAircraft.IsMi8MT(_profileHandler.DCSAircraft) && !_profileHandler.DCSAircraft.UseGenericRadio)
                            {
                                var radioPanelPZ69UserControl = new RadioPanelPZ69UserControlMi8(hidSkeleton);
                                _panelUserControls.Add(radioPanelPZ69UserControl);
                                tabItem.Content = radioPanelPZ69UserControl;
                                TabControlPanels.Items.Add(tabItem);
                                _profileFileHIDInstances.Add(new KeyValuePair<string, GamingPanelEnum>(hidSkeleton.HIDInstance, hidSkeleton.GamingPanelType));

                                AppEventHandler.PanelEvent(this, hidSkeleton.HIDInstance, hidSkeleton, PanelEventType.Created);
                            }
                            else if (DCSAircraft.IsBf109K4(_profileHandler.DCSAircraft) && !_profileHandler.DCSAircraft.UseGenericRadio)
                            {
                                var radioPanelPZ69UserControl = new RadioPanelPZ69UserControlBf109(hidSkeleton);
                                _panelUserControls.Add(radioPanelPZ69UserControl);
                                tabItem.Content = radioPanelPZ69UserControl;
                                TabControlPanels.Items.Add(tabItem);
                                _profileFileHIDInstances.Add(new KeyValuePair<string, GamingPanelEnum>(hidSkeleton.HIDInstance, hidSkeleton.GamingPanelType));

                                AppEventHandler.PanelEvent(this, hidSkeleton.HIDInstance, hidSkeleton, PanelEventType.Created);
                            }
                            else if (DCSAircraft.IsFW190D9(_profileHandler.DCSAircraft) && !_profileHandler.DCSAircraft.UseGenericRadio)
                            {
                                var radioPanelPZ69UserControl = new RadioPanelPZ69UserControlFw190(hidSkeleton);
                                _panelUserControls.Add(radioPanelPZ69UserControl);
                                tabItem.Content = radioPanelPZ69UserControl;
                                TabControlPanels.Items.Add(tabItem);
                                _profileFileHIDInstances.Add(new KeyValuePair<string, GamingPanelEnum>(hidSkeleton.HIDInstance, hidSkeleton.GamingPanelType));

                                AppEventHandler.PanelEvent(this, hidSkeleton.HIDInstance, hidSkeleton, PanelEventType.Created);
                            }
                            else if (DCSAircraft.IsP51D(_profileHandler.DCSAircraft) && !_profileHandler.DCSAircraft.UseGenericRadio)
                            {
                                var radioPanelPZ69UserControl = new RadioPanelPZ69UserControlP51D(hidSkeleton);
                                _panelUserControls.Add(radioPanelPZ69UserControl);
                                tabItem.Content = radioPanelPZ69UserControl;
                                TabControlPanels.Items.Add(tabItem);
                                _profileFileHIDInstances.Add(new KeyValuePair<string, GamingPanelEnum>(hidSkeleton.HIDInstance, hidSkeleton.GamingPanelType));

                                AppEventHandler.PanelEvent(this, hidSkeleton.HIDInstance, hidSkeleton, PanelEventType.Created);
                            }
                            else if (DCSAircraft.IsF86F(_profileHandler.DCSAircraft) && !_profileHandler.DCSAircraft.UseGenericRadio)
                            {
                                var radioPanelPZ69UserControl = new RadioPanelPZ69UserControlF86F(hidSkeleton);
                                _panelUserControls.Add(radioPanelPZ69UserControl);
                                tabItem.Content = radioPanelPZ69UserControl;
                                TabControlPanels.Items.Add(tabItem);
                                _profileFileHIDInstances.Add(new KeyValuePair<string, GamingPanelEnum>(hidSkeleton.HIDInstance, hidSkeleton.GamingPanelType));

                                AppEventHandler.PanelEvent(this, hidSkeleton.HIDInstance, hidSkeleton, PanelEventType.Created);
                            }
                            else if (DCSAircraft.IsSpitfireLFMkIX(_profileHandler.DCSAircraft) && !_profileHandler.DCSAircraft.UseGenericRadio)
                            {
                                var radioPanelPZ69UserControl = new RadioPanelPZ69UserControlSpitfireLFMkIX(hidSkeleton);
                                _panelUserControls.Add(radioPanelPZ69UserControl);
                                tabItem.Content = radioPanelPZ69UserControl;
                                TabControlPanels.Items.Add(tabItem);
                                _profileFileHIDInstances.Add(new KeyValuePair<string, GamingPanelEnum>(hidSkeleton.HIDInstance, hidSkeleton.GamingPanelType));

                                AppEventHandler.PanelEvent(this, hidSkeleton.HIDInstance, hidSkeleton, PanelEventType.Created);
                            }
                            else if (DCSAircraft.IsAJS37(_profileHandler.DCSAircraft) && !_profileHandler.DCSAircraft.UseGenericRadio)
                            {
                                var radioPanelPZ69UserControl = new RadioPanelPZ69UserControlAJS37(hidSkeleton);
                                _panelUserControls.Add(radioPanelPZ69UserControl);
                                tabItem.Content = radioPanelPZ69UserControl;
                                TabControlPanels.Items.Add(tabItem);
                                _profileFileHIDInstances.Add(new KeyValuePair<string, GamingPanelEnum>(hidSkeleton.HIDInstance, hidSkeleton.GamingPanelType));

                                AppEventHandler.PanelEvent(this, hidSkeleton.HIDInstance, hidSkeleton, PanelEventType.Created);
                            }
                            else if (DCSAircraft.IsSA342(_profileHandler.DCSAircraft) && !_profileHandler.DCSAircraft.UseGenericRadio)
                            {
                                var radioPanelPZ69UserControl = new RadioPanelPZ69UserControlSA342(hidSkeleton);
                                _panelUserControls.Add(radioPanelPZ69UserControl);
                                tabItem.Content = radioPanelPZ69UserControl;
                                TabControlPanels.Items.Add(tabItem);
                                _profileFileHIDInstances.Add(new KeyValuePair<string, GamingPanelEnum>(hidSkeleton.HIDInstance, hidSkeleton.GamingPanelType));

                                AppEventHandler.PanelEvent(this, hidSkeleton.HIDInstance, hidSkeleton, PanelEventType.Created);
                            }
                            else if (DCSAircraft.IsFA18C(_profileHandler.DCSAircraft) && !_profileHandler.DCSAircraft.UseGenericRadio)
                            {
                                var radioPanelPZ69UserControl = new RadioPanelPZ69UserControlFA18C(hidSkeleton);
                                _panelUserControls.Add(radioPanelPZ69UserControl);
                                tabItem.Content = radioPanelPZ69UserControl;
                                TabControlPanels.Items.Add(tabItem);
                                _profileFileHIDInstances.Add(new KeyValuePair<string, GamingPanelEnum>(hidSkeleton.HIDInstance, hidSkeleton.GamingPanelType));

                                AppEventHandler.PanelEvent(this, hidSkeleton.HIDInstance, hidSkeleton, PanelEventType.Created);
                            }
                            else if (DCSAircraft.IsM2000C(_profileHandler.DCSAircraft) && !_profileHandler.DCSAircraft.UseGenericRadio)
                            {
                                var radioPanelPZ69UserControl = new RadioPanelPZ69UserControlM2000C(hidSkeleton);
                                _panelUserControls.Add(radioPanelPZ69UserControl);
                                tabItem.Content = radioPanelPZ69UserControl;
                                TabControlPanels.Items.Add(tabItem);
                                _profileFileHIDInstances.Add(new KeyValuePair<string, GamingPanelEnum>(hidSkeleton.HIDInstance, hidSkeleton.GamingPanelType));

                                AppEventHandler.PanelEvent(this, hidSkeleton.HIDInstance, hidSkeleton, PanelEventType.Created);
                            }
                            else if (DCSAircraft.IsF5E(_profileHandler.DCSAircraft) && !_profileHandler.DCSAircraft.UseGenericRadio)
                            {
                                var radioPanelPZ69UserControl = new RadioPanelPZ69UserControlF5E(hidSkeleton);
                                _panelUserControls.Add(radioPanelPZ69UserControl);
                                tabItem.Content = radioPanelPZ69UserControl;
                                TabControlPanels.Items.Add(tabItem);
                                _profileFileHIDInstances.Add(new KeyValuePair<string, GamingPanelEnum>(hidSkeleton.HIDInstance, hidSkeleton.GamingPanelType));

                                AppEventHandler.PanelEvent(this, hidSkeleton.HIDInstance, hidSkeleton, PanelEventType.Created);
                            }
                            else if (DCSAircraft.IsF14B(_profileHandler.DCSAircraft) && !_profileHandler.DCSAircraft.UseGenericRadio)
                            {
                                var radioPanelPZ69UserControl = new RadioPanelPZ69UserControlF14B(hidSkeleton);
                                _panelUserControls.Add(radioPanelPZ69UserControl);
                                tabItem.Content = radioPanelPZ69UserControl;
                                TabControlPanels.Items.Add(tabItem);
                                _profileFileHIDInstances.Add(new KeyValuePair<string, GamingPanelEnum>(hidSkeleton.HIDInstance, hidSkeleton.GamingPanelType));

                                AppEventHandler.PanelEvent(this, hidSkeleton.HIDInstance, hidSkeleton, PanelEventType.Created);
                            }
                            else if (DCSAircraft.IsAV8B(_profileHandler.DCSAircraft) && !_profileHandler.DCSAircraft.UseGenericRadio)
                            {
                                var radioPanelPZ69UserControl = new RadioPanelPZ69UserControlAV8BNA(hidSkeleton);
                                _panelUserControls.Add(radioPanelPZ69UserControl);
                                tabItem.Content = radioPanelPZ69UserControl;
                                TabControlPanels.Items.Add(tabItem);
                                _profileFileHIDInstances.Add(new KeyValuePair<string, GamingPanelEnum>(hidSkeleton.HIDInstance, hidSkeleton.GamingPanelType));

                                AppEventHandler.PanelEvent(this, hidSkeleton.HIDInstance, hidSkeleton, PanelEventType.Created);
                            }
                            else if (DCSAircraft.IsP47D(_profileHandler.DCSAircraft) && !_profileHandler.DCSAircraft.UseGenericRadio)
                            {
                                var radioPanelPZ69UserControl = new RadioPanelPZ69UserControlP47D(hidSkeleton);
                                _panelUserControls.Add(radioPanelPZ69UserControl);
                                tabItem.Content = radioPanelPZ69UserControl;
                                TabControlPanels.Items.Add(tabItem);
                                _profileFileHIDInstances.Add(new KeyValuePair<string, GamingPanelEnum>(hidSkeleton.HIDInstance, hidSkeleton.GamingPanelType));

                                AppEventHandler.PanelEvent(this, hidSkeleton.HIDInstance, hidSkeleton, PanelEventType.Created);
                            }                            
                            else if (DCSAircraft.IsT45C(_profileHandler.DCSAircraft) && !_profileHandler.DCSAircraft.UseGenericRadio)
                            {
                                var radioPanelPZ69UserControl = new RadioPanelPZ69UserControlT45C(hidSkeleton);
                                _panelUserControls.Add(radioPanelPZ69UserControl);
                                tabItem.Content = radioPanelPZ69UserControl;
                                TabControlPanels.Items.Add(tabItem);
                                _profileFileHIDInstances.Add(new KeyValuePair<string, GamingPanelEnum>(hidSkeleton.HIDInstance, hidSkeleton.GamingPanelType));

                                AppEventHandler.PanelEvent(this, hidSkeleton.HIDInstance, hidSkeleton, PanelEventType.Created);
                            }
                            else if (DCSAircraft.IsMi24P(_profileHandler.DCSAircraft) && !_profileHandler.DCSAircraft.UseGenericRadio)
                            {
                                var radioPanelPZ69UserControl = new RadioPanelPZ69UserControlMi24P(hidSkeleton);
                                _panelUserControls.Add(radioPanelPZ69UserControl);
                                tabItem.Content = radioPanelPZ69UserControl;
                                TabControlPanels.Items.Add(tabItem);
                                _profileFileHIDInstances.Add(new KeyValuePair<string, GamingPanelEnum>(hidSkeleton.HIDInstance, hidSkeleton.GamingPanelType));

                                AppEventHandler.PanelEvent(this, hidSkeleton.HIDInstance, hidSkeleton, PanelEventType.Created);
                            }
                            else if (DCSAircraft.IsAH64D(_profileHandler.DCSAircraft) && !_profileHandler.DCSAircraft.UseGenericRadio)
                            {
                                var radioPanelPZ69UserControl = new RadioPanelPZ69UserControlAH64D(hidSkeleton);
                                _panelUserControls.Add(radioPanelPZ69UserControl);
                                tabItem.Content = radioPanelPZ69UserControl;
                                TabControlPanels.Items.Add(tabItem);
                                _profileFileHIDInstances.Add(new KeyValuePair<string, GamingPanelEnum>(hidSkeleton.HIDInstance, hidSkeleton.GamingPanelType));

                                AppEventHandler.PanelEvent(this, hidSkeleton.HIDInstance, hidSkeleton, PanelEventType.Created);
                            }
                            else if (DCSAircraft.IsF16C(_profileHandler.DCSAircraft) && !_profileHandler.DCSAircraft.UseGenericRadio)
                            {
                                var radioPanelPZ69UserControl = new RadioPanelPZ69UserControlF16C(hidSkeleton);
                                _panelUserControls.Add(radioPanelPZ69UserControl);
                                tabItem.Content = radioPanelPZ69UserControl;
                                TabControlPanels.Items.Add(tabItem);
                                _profileFileHIDInstances.Add(new KeyValuePair<string, GamingPanelEnum>(hidSkeleton.HIDInstance, hidSkeleton.GamingPanelType));

                                AppEventHandler.PanelEvent(this, hidSkeleton.HIDInstance, hidSkeleton, PanelEventType.Created);
                            }
                            else if (DCSAircraft.IsMosquito(_profileHandler.DCSAircraft) && !_profileHandler.DCSAircraft.UseGenericRadio)
                            {
                                var radioPanelPZ69UserControl = new RadioPanelPZ69UserControlMosquito(hidSkeleton);
                                _panelUserControls.Add(radioPanelPZ69UserControl);
                                tabItem.Content = radioPanelPZ69UserControl;
                                TabControlPanels.Items.Add(tabItem);
                                _profileFileHIDInstances.Add(new KeyValuePair<string, GamingPanelEnum>(hidSkeleton.HIDInstance, hidSkeleton.GamingPanelType));

                                AppEventHandler.PanelEvent(this, hidSkeleton.HIDInstance, hidSkeleton, PanelEventType.Created);
                            }
                            else
                            {
                                var radioPanelPZ69UserControl = new RadioPanelPZ69UserControlGeneric(hidSkeleton);
                                _panelUserControls.Add(radioPanelPZ69UserControl);
                                tabItem.Content = radioPanelPZ69UserControl;
                                TabControlPanels.Items.Add(tabItem);
                                _profileFileHIDInstances.Add(new KeyValuePair<string, GamingPanelEnum>(hidSkeleton.HIDInstance, hidSkeleton.GamingPanelType));

                                /*
                                 * If the module doesn't have a pre-programmed radio it will end up here. If this is a new user profile
                                 * then set the value here so that if there is a pre-programmed radio available in the future it won't cause
                                 * problems. The problem would be that when the user loads the profile the pre-programmed radio is loaded
                                 * but the user has configs for the generic radio.
                                 * I.e. no pre-programmed radio exists => UseGenericRadio = true.
                                 */
                                _profileHandler.DCSAircraft.UseGenericRadio = true;
                                AppEventHandler.PanelEvent(this, hidSkeleton.HIDInstance, hidSkeleton, PanelEventType.Created);
                            }

                            break;
                        }
                }

                SortTabs();
                if (TabControlPanels.Items.Count > 0)
                {
                    TabControlPanels.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void SortTabs()
        {
            var panelOrderList = new List<GamingPanelEnum>
                                     {
                                        GamingPanelEnum.CDU737,
                                         GamingPanelEnum.StreamDeckXL,
                                         GamingPanelEnum.StreamDeck,
                                         GamingPanelEnum.StreamDeckV2,
                                         GamingPanelEnum.StreamDeckMK2,
                                         GamingPanelEnum.StreamDeckMini,
                                         GamingPanelEnum.BackLitPanel,
                                         GamingPanelEnum.PZ69RadioPanel,
                                         GamingPanelEnum.TPM,
                                         GamingPanelEnum.PZ70MultiPanel,
                                         GamingPanelEnum.PZ55SwitchPanel
                                     };

            foreach (var gamingPanelEnum in panelOrderList)
            {
                for (var i = 0; i < TabControlPanels.Items.Count; i++)
                {
                    var tabItem = (TabItem)TabControlPanels.Items.GetItemAt(i);
                    var userControl = (IGamingPanelUserControl)tabItem.Content;

                    var panelType = userControl.GetPanelType();
                    if (panelType == gamingPanelEnum)
                    {
                        TabControlPanels.Items.RemoveAt(i);
                        TabControlPanels.Items.Insert(0, tabItem);
                    }
                }
            }

            TabControlPanels.SelectedIndex = 0;
        }

        private void LoadSettings()
        {
            LoadProcessPriority();

            if (Settings.Default.MainWindowHeight > 0)
            {
                Height = Settings.Default.MainWindowHeight;
            }

            if (Settings.Default.MainWindowWidth > 0)
            {
                Width = Settings.Default.MainWindowWidth;
            }

            if (Settings.Default.MainWindowTop > 0)
            {
                Top = Settings.Default.MainWindowTop;
            }

            if (Settings.Default.MainWindowLeft > 0)
            {
                Left = Settings.Default.MainWindowLeft;
            }

            Common.APIModeUsed = Settings.Default.APIMode == 0 ? APIModeEnum.keybd_event : APIModeEnum.SendInput;
        }

        private void MainWindowLocationChanged(object sender, EventArgs e)
        {
            try
            {
                if (!_isLoaded)
                {
                    return;
                }

                if (Top > 0 && Left > 0)
                {
                    Settings.Default.MainWindowTop = Top;
                    Settings.Default.MainWindowLeft = Left;
                    Settings.Default.Save();
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        public void SwitchesChanged(object sender, SwitchesChangedEventArgs e)
        {
            try
            {
                SetWindowState();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        public void SettingsModified(object sender, PanelInfoArgs e)
        {
            try
            {
                Dispatcher?.BeginInvoke((Action)SetWindowState);
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        public void SettingsApplied(object sender, PanelInfoArgs e)
        {
            try
            {
                SetWindowState();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void MainWindowSizeChanged(object sender, SizeChangedEventArgs e)
        {
            try
            {
                if (!_isLoaded)
                {
                    return;
                }

                if (WindowState != WindowState.Minimized && WindowState != WindowState.Maximized)
                {
                    Settings.Default.MainWindowHeight = Height;
                    Settings.Default.MainWindowWidth = Width;
                    Settings.Default.Save();
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }
        
        private async void CheckForNewDCSFPRelease()
        {
            // #if !DEBUG
            var assembly = Assembly.GetExecutingAssembly();
            var fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
            try
            {
                var dateTime = Settings.Default.LastGitHubCheck;

                var client = new GitHubClient(new ProductHeaderValue("DCSFlightpanels"));
                var timeSpan = DateTime.Now - dateTime;
                if (timeSpan.Days > 1)
                {
                    Settings.Default.LastGitHubCheck = DateTime.Now;
                    Settings.Default.Save();
                    var lastRelease = await client.Repository.Release.GetLatest("DCSFlightpanels", "DCSFlightpanels");
                    var thisReleaseArray = fileVersionInfo.FileVersion.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
                    var gitHubReleaseArray = lastRelease.TagName.Replace("v.", string.Empty).Replace("v", string.Empty).Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
                    var newerAvailable = false;
                    if (int.Parse(gitHubReleaseArray[0]) > int.Parse(thisReleaseArray[0]))
                    {
                        newerAvailable = true;
                    }
                    else if (int.Parse(gitHubReleaseArray[0]) >= int.Parse(thisReleaseArray[0]))
                    {
                        if (int.Parse(gitHubReleaseArray[1]) > int.Parse(thisReleaseArray[1]))
                        {
                            newerAvailable = true;
                        }
                    }
                    else if (int.Parse(gitHubReleaseArray[0]) >= int.Parse(thisReleaseArray[0]))
                    {
                        if (int.Parse(gitHubReleaseArray[1]) >= int.Parse(thisReleaseArray[1]))
                        {
                            if (int.Parse(gitHubReleaseArray[1]) > int.Parse(thisReleaseArray[1]))
                            {
                                newerAvailable = true;
                            }
                        }
                    }

                    if (newerAvailable)
                    {
                        Dispatcher?.Invoke(() =>
                        {
                            LabelVersionInformation.Visibility = Visibility.Hidden;
                            LabelDownloadNewVersion.Visibility = Visibility.Visible;
                        });
                    }
                    else
                    {
                        Dispatcher?.Invoke(() =>
                        {
                            LabelVersionInformation.Text = "v." + fileVersionInfo.FileVersion;
                            LabelVersionInformation.Visibility = Visibility.Visible;
                        });
                    }

                    var lastDCSBIOSRelease = await client.Repository.Release.GetLatest("DCSFlightpanels", "dcs-bios");
                    Dispatcher?.Invoke(() =>
                    {
                        LabelDCSBIOSReleaseDate.Text = "DCS-BIOS Release Date : " + lastDCSBIOSRelease.CreatedAt.Date.ToLongDateString();
                        Settings.Default.LastDCSBIOSRelease = lastDCSBIOSRelease.CreatedAt.Date.ToLongDateString();
                        Settings.Default.Save();
                    });
                }
                else
                {
                    Dispatcher?.Invoke(() =>
                    {
                        LabelVersionInformation.Text = "DCSFP version : " + fileVersionInfo.FileVersion;
                        LabelVersionInformation.Visibility = Visibility.Visible;
                        LabelDCSBIOSReleaseDate.Text = "DCS-BIOS Release Date : " + Settings.Default.LastDCSBIOSRelease;
                    });
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error checking for newer releases.");
                LabelVersionInformation.Text = "DCSFP version : " + fileVersionInfo.FileVersion;
                LabelDCSBIOSReleaseDate.Text = "DCS-BIOS Release Date : " + Settings.Default.LastDCSBIOSRelease;
            }

            // #endif
        }

        private void TimerCheckExceptions(object sender, ElapsedEventArgs e)
        {
            // ignored
        }

        private void TimerStopRotation(object sender, ElapsedEventArgs e)
        {
            try
            {
                Dispatcher?.BeginInvoke((Action)(() => ImageDcsBiosConnected.IsEnabled = false));
                _dcsStopGearTimer.Stop();
            }
            catch (Exception)
            {
                // ignore
            }
        }

        private void SetWindowTitle()
        {
            if (DCSAircraft.IsNoFrameLoadedYet(_profileHandler.DCSAircraft))
            {
                Title = string.Empty;
            }
            else if (_profileHandler.IsNewProfile)
            {
                Title = _windowName;
            }
            else
            {
                Title = _windowName + _profileHandler.Filename;
            }

            if (_profileHandler.IsDirty)
            {
                Title += " *";
            }
        }

        private void ButtonImageSaveMouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                try
                {
                    Mouse.OverrideCursor = Cursors.Wait;
                    SaveNewOrExistingProfile();
                    SetWindowState();
                    SystemSounds.Asterisk.Play();
                }
                finally
                {
                    Mouse.OverrideCursor = Cursors.Arrow;
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void MainWindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                if (_profileHandler != null && CommonUI.DoDiscardAfterMessage(_profileHandler.IsDirty))
                {
                    Settings.Default.LastProfileFileUsed = _profileHandler.LastProfileUsed;
                    Settings.Default.Save();
                }
                else
                {
                    e.Cancel = true;
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void MainWindow_OnClosed(object sender, EventArgs e)
        {
            try
            {
                Shutdown();
                LogManager.Shutdown();
                System.Windows.Application.Current.Shutdown();
                Environment.Exit(0);
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void Shutdown()
        {
            try
            {
                _exceptionTimer.Stop();
                _dcsStopGearTimer.Stop();
                _statusMessagesTimer.Stop();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }

            try
            {
                foreach (var saitekUserControl in _panelUserControls)
                {
                    ((IGamingPanelUserControl)saitekUserControl).GetGamingPanel()?.Dispose();
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }

            try
            {
                // TODO THIS CAUSES HANGING WHEN CLOSING THE APPLICATION!?!?
                // _hidHandler.Shutdown();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }

            try
            {
                ShutdownDCSBIOS();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void MenuItemExitClick(object sender, RoutedEventArgs e)
        {
            try
            {
                Shutdown();
                System.Windows.Application.Current.Shutdown();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void MenuItemSaveClick(object sender, RoutedEventArgs e)
        {
            try
            {
                SaveNewOrExistingProfile();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void SaveNewOrExistingProfile()
        {
            try
            {
                Mouse.OverrideCursor = Cursors.Wait;
                var selectedIndex = TabControlPanels.SelectedIndex;
                _profileHandler.SaveProfile();
                SetWindowState();
                TabControlPanels.SelectedIndex = selectedIndex;
            }
            finally
            {
                Mouse.OverrideCursor = Cursors.Arrow;
            }
        }

        private void MenuItemSaveAsClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_profileHandler.SaveAsNewProfile())
                {
                    _profileHandler.IsDirty = true;
                    SetWindowState();
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void MenuItemNewClick(object sender, RoutedEventArgs e)
        {
            try
            {
                /*_profileHandler.NewProfile();
                var chooseProfileModuleWindow = new ChooseProfileModuleWindow();
                chooseProfileModuleWindow.ShowDialog();
                Common.UseGenericRadio = chooseProfileModuleWindow.UseGenericRadio;
                _profileHandler.Profile = chooseProfileModuleWindow.DCSAirframe;
                SetWindowState();*/
                //NewProfile();
                _profileHandler.CreateNewProfile();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void SetNS430Status(bool enable, bool menuIsEnabled = true)
        {
            try
            {
                MenuItemUseNS430.IsChecked = enable;
                MenuItemUseNS430.IsEnabled = menuIsEnabled && DCSAircraft.HasNS430();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void MenuItemOpenClick(object sender, RoutedEventArgs e)
        {
            try
            {
                try
                {
                    Mouse.OverrideCursor = Cursors.Wait;
                    _profileHandler.OpenProfile();
                }
                finally
                {
                    Mouse.OverrideCursor = Cursors.Arrow;
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void MenuItemCloseProfile_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                try
                {
                    Mouse.OverrideCursor = Cursors.Wait;
                    _profileHandler.CloseProfile();
                }
                finally
                {
                    Mouse.OverrideCursor = null;
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void MenuItemWikiClick(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "https://github.com/DCSFlightpanels/DCSFlightpanels/wiki",
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void MenuItemAboutClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var aboutFpWindow = new AboutFpWindow();
                aboutFpWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void ButtonImageNewMouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                _profileHandler.CreateNewProfile();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void ButtonImageOpenMouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                try
                {
                    Mouse.OverrideCursor = Cursors.Wait;
                    _profileHandler.OpenProfile();
                }
                finally
                {
                    Mouse.OverrideCursor = Cursors.Arrow;
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }
        /*
        private void NewProfile()
        {
            try
            {
                if (!CloseProfile())
                {
                    return;
                }

                RestartWithProfile("NEWPROFILE");
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void OpenAnOtherProfile()
        {
            if (!CloseProfile())
            {
                return;
            }

            var bindingsFile = _profileHandler.OpenProfile();
            RestartWithProfile(bindingsFile);
        }
        
        private void RestartWithProfile(string bindingsFile)
        {
            try
            {
                if (!string.IsNullOrEmpty(bindingsFile))
                {
                    if (bindingsFile != "NEWPROFILE" && !File.Exists(bindingsFile))
                    {
                        MessageBox.Show("File " + bindingsFile + " does not exist.", "Error finding file", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    var streamDeckArguments = Settings.Default.LoadStreamDeck ? string.Empty : Constants.CommandLineArgumentNoStreamDeck + " ";
                    Process.Start("dcsfp.exe", streamDeckArguments + Constants.CommandLineArgumentOpenProfile + "\"" + bindingsFile + "\"");
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }
        */
        private void ButtonImageRefreshMouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                try
                {
                    Mouse.OverrideCursor = Cursors.Wait;
                    RefreshProfile();
                    SystemSounds.Beep.Play();
                }
                finally
                {
                    Mouse.OverrideCursor = Cursors.Arrow;
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void RefreshProfile()
        {
            _profileHandler.ReloadProfile();
            SetWindowState();
        }

        private void ButtonImageNotepadMouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                OpenProfileInNotepad();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void OpenProfileInNotepad()
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = _profileHandler.Filename,
                UseShellExecute = true
            });
        }

        private void SetWindowState()
        {
            MenuItemSaveAs.IsEnabled = _profileHandler.ProfileLoaded;
            MenuItemCloseProfile.IsEnabled = _profileHandler.ProfileLoaded;
            ButtonImageSave.IsEnabled = _profileHandler.IsDirty;
            MenuItemSave.IsEnabled = _profileHandler.IsDirty && !_profileHandler.IsNewProfile;
            ButtonImageRefresh.IsEnabled = !_profileHandler.IsNewProfile && !_profileHandler.IsDirty;
            ButtonImageNotepad.IsEnabled = !_profileHandler.IsNewProfile && !_profileHandler.IsDirty;
            SetWindowTitle();
        }

        private void RotateGear(int howLong = 5000)
        {
            try
            {
                if (ImageDcsBiosConnected.IsEnabled)
                {
                    return;
                }

                ImageDcsBiosConnected.IsEnabled = true;
                if (_dcsStopGearTimer.Enabled)
                {
                    _dcsStopGearTimer.Stop();
                }

                _dcsStopGearTimer.Interval = howLong;
                _dcsStopGearTimer.Start();
            }
            catch (Exception)
            {
                // ignore
            }
        }

        public void DcsBiosConnectionActive(object sender, DCSBIOSConnectionEventArgs e)
        {
            try
            {
                Dispatcher?.BeginInvoke((Action)(() => RotateGear()));
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }
        
        private void ShowStatusBarMessage(string str)
        {
            try
            {
                lock (_lockObjectStatusMessages)
                {
                    _statusMessages.Add(str);
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void TimerStatusMessagesTimer(object sender, ElapsedEventArgs e)
        {
            try
            {
                lock (_lockObjectStatusMessages)
                {
                    _statusMessagesTimer.Interval = _statusMessages.Count > 0 ? 8000 : 1000;

                    Dispatcher?.BeginInvoke((Action)(() => LabelInformation.Text = string.Empty));

                    if (_statusMessages.Count == 0)
                    {
                        return;
                    }

                    var message = _statusMessages[0];
                    Dispatcher?.BeginInvoke((Action)(() => LabelInformation.Text = message));
                    _statusMessages.RemoveAt(0);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private static void TryOpenLogFileWithTarget(string targetName)
        {
            try
            {
                string errorLogFilePath = GetLogFilePathByTarget(targetName);
                if (errorLogFilePath == null || !File.Exists(errorLogFilePath))
                {
                    MessageBox.Show($"No log file found {errorLogFilePath}", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
                Process.Start(new ProcessStartInfo
                {
                    FileName = errorLogFilePath,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void MenuItemErrorLog_OnClick(object sender, RoutedEventArgs e)
        {
            TryOpenLogFileWithTarget("error_logfile");
        }

        private void MenuItemDebugLog_OnClick(object sender, RoutedEventArgs e)
        {
            TryOpenLogFileWithTarget("debug_logfile");
        }

        private void ProfilesAutoBackupFolderOpen_OnClick(object sender, RoutedEventArgs e)
        {
            TryOpenAutoBackupFolder();
        }

        private static void LoadProcessPriority()
        {
            try
            {
                Process.GetCurrentProcess().PriorityClass = Settings.Default.ProcessPriority;
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void Hyperlink_OnRequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = e.Uri.AbsoluteUri,
                    UseShellExecute = true
                });
                e.Handled = true;
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void MenuItemFormulaSandbox_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var formulaSandBoxWindow = new JaceSandboxWindow();
                formulaSandBoxWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void ConfigurePlugins()
        {
            PluginManager.PlugSupportActivated = Settings.Default.EnablePlugin;
            PluginManager.DisableKeyboardAPI = Settings.Default.DisableKeyboardAPI;

            MenuItemPlugins.Items.Clear();

            if (PluginManager.PlugSupportActivated && PluginManager.HasPlugin())
            {
                foreach (var plugin in PluginManager.Get().Plugins)
                {
                    var menuItem = new MenuItem { Header = plugin.Metadata.Name };
                    menuItem.Click += MenuItemPlugin_OnClick;
                    MenuItemPlugins.Items.Add(menuItem);
                }

                LabelPluginInfo.Text = "Plugin(s) Loaded";
            }
            else if (PluginManager.PlugSupportActivated && !PluginManager.HasPlugin())
            {
                LabelPluginInfo.Text = "No Plugins found.";
            }
            else
            {
                LabelPluginInfo.Text = "Plugin support disabled.";
            }

            MenuItemPlugins.IsEnabled = MenuItemPlugins.HasItems;
        }

        private void MenuItemSettings_OnClick(object sender, RoutedEventArgs e)
        {
            var settingsWindow = new SettingsWindow(0);
            if (settingsWindow.ShowDialog() == true)
            {
                if (settingsWindow.DCSBIOSChanged)
                {
                    _profileHandler.DCSBIOSJSONDirectory = Settings.Default.DCSBiosJSONLocation;
                    DCSAircraft.FillModulesListFromDcsBios(DCSBIOSCommon.GetDCSBIOSJSONDirectory(Settings.Default.DCSBiosJSONLocation));
                }
                if (settingsWindow.GeneralChanged)
                {
                    LoadProcessPriority();
                }
                if (settingsWindow.SRSChanged)
                {
                    SRSRadioFactory.SetParams(Settings.Default.SRSPortFrom, Settings.Default.SRSIpTo, Settings.Default.SRSPortTo);
                    SRSRadioFactory.ReStart();
                }
                ConfigurePlugins();
            }
        }
        
        private void MenuItemUSBPowerManagement_OnClick(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("You need to run DCSFP as Administrator for this function to work!");
            var result = USBPowerManagement.FixSaitekUSBEnhancedPowerManagerIssues();
            var informationTextBlockWindow = new InformationTextBlockWindow(result);
            informationTextBlockWindow.ShowDialog();
        }

        private void MenuItemBugReport_OnClick(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Please file an \"Issue\" on GitHub. This makes tracking and managing bugs easier and will speed up the fix.", "Use GitHub for Bug Reporting", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        protected override void OnStateChanged(EventArgs e)
        {
            if (WindowState == WindowState.Minimized && Settings.Default.RunMinimized)
            {
                Hide();
            }
        }

        private void MenuItemUseNS430_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                ((MenuItem)sender).IsChecked = !((MenuItem)sender).IsChecked;
                _profileHandler.UseNS430 = ((MenuItem)sender).IsChecked;
                SetWindowState();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        public void SetForwardPanelEvent(object sender, ForwardPanelEventArgs e)
        {
            _disablePanelEventsFromBeingRouted = !e.Forward;
            SetPanelInteractionMode(false);
        }

        private void MenuItemPlugin_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var pluginName = ((MenuItem)sender).Header;
                foreach (var plugin in PluginManager.Get().Plugins)
                {
                    if (plugin.Metadata.Name.Equals(pluginName))
                    {
                        plugin.Value.Settings();
                    }
                }

                SetWindowState();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void ButtonDisableAllPanelInteractions_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                _disablePanelEventsFromBeingRouted = !_disablePanelEventsFromBeingRouted;
                Settings.Default.DisableKeyboardAPI = _disablePanelEventsFromBeingRouted;
                Settings.Default.Save();
                SetPanelInteractionMode(true);
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void SetPanelInteractionMode(bool sendEvent)
        {
            try
            {
                if (_disablePanelEventsFromBeingRouted)
                {
                    ButtonDisablePanelEventsFromBeingRouted.Content = "Disabled!";
                    ButtonDisablePanelEventsFromBeingRouted.ToolTip = "Panel events are not routed";
                }
                else
                {
                    ButtonDisablePanelEventsFromBeingRouted.Content = "Enabled!";
                    ButtonDisablePanelEventsFromBeingRouted.ToolTip = "Panel events are routed";
                }

                /*
                 * This event can be sent from somewhere else than MainWindow, if so MainWindow should NOT send the event again.
                 */
                if (sendEvent)
                {
                    // Disabling can be used when user want to reset panel switches and does not want that resetting switches affects the game.
                    AppEventHandler.ForwardKeyPressEvent(this, !_disablePanelEventsFromBeingRouted);
                }
                SetWindowState();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void ButtonDisablePanelEventsFromBeingRouted_OnMouseEnter(object sender, MouseEventArgs e)
        {
            Cursor = Cursors.Hand;
        }

        private void ButtonDisablePanelEventsFromBeingRouted_OnMouseLeave(object sender, MouseEventArgs e)
        {
            Cursor = Cursors.Arrow;
        }


        private void MainWindow_OnKeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.S && Keyboard.Modifiers == ModifierKeys.Control && ButtonImageSave.IsEnabled)
                {
                    SaveNewOrExistingProfile();
                    SetWindowState();
                }
                else if (e.Key == Key.O && Keyboard.Modifiers == ModifierKeys.Control)
                {
                    _profileHandler.OpenProfile();
                }
                else if (e.Key == Key.N && Keyboard.Modifiers == ModifierKeys.Control)
                {
                    _profileHandler.CreateNewProfile();
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        public List<ModifiedGenericBinding> ResolveConflicts()
        {
            Mouse.OverrideCursor = Cursors.Arrow;
            var bindingsMappingWindow = new BindingsMappingWindow(BindingMappingManager.PanelBindings, GamingPanel.GamingPanels);
            bindingsMappingWindow.ShowDialog();

            return bindingsMappingWindow.ModifiedGenericBindings;
        }


        public void ProfileEvent(object sender, ProfileEventArgs e)
        {
            try
            {
                switch (e.ProfileEventType)
                {
                    case ProfileEventEnum.ProfileLoaded:
                        {
                            SetWindowTitle();
                            SetWindowState();
                            SetApplicationMode();
                            HIDHandler.GetInstance().Startup(Settings.Default.LoadStreamDeck);
                            break;
                        }
                    case ProfileEventEnum.ProfileClosed:
                        {
                            SetApplicationMode();
                            DisposePanels();
                            SetWindowTitle();
                            SetWindowState();

                            ButtonSearchPanels.IsEnabled = false;
                            break;
                        }
                }

                Dispatcher?.BeginInvoke((Action)(() => MenuItemUseNS430.IsChecked = _profileHandler.UseNS430));

                Dispatcher?.BeginInvoke((Action)SetWindowState);
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }


        public void PanelEvent(object sender, PanelEventArgs e)
        {
            switch (e.EventType)
            {
                case PanelEventType.Found:
                    {
                        if (!_profileFileHIDInstances.Any(o => o.Key.Equals(e.HidSkeleton.HIDInstance)))
                        {
                            Dispatcher?.Invoke((Action)(() => CreatePanel(e.HidSkeleton)));
                        }
                        break;
                    }
                case PanelEventType.Attached:
                    {
                        if (!_profileFileHIDInstances.Any(o => o.Key.Equals(e.HidSkeleton.HIDInstance)))
                        {
                            Dispatcher?.Invoke((Action)(() => CreatePanel(e.HidSkeleton)));
                        }
                        break;
                    }
                case PanelEventType.Detached:
                    {
                        _profileFileHIDInstances.RemoveAll(o => o.Key.Equals(e.HidSkeleton.HIDInstance));
                        Dispatcher?.Invoke((Action)(() => DisposePanel(e.HidSkeleton)));
                        break;
                    }
                case PanelEventType.Created:
                case PanelEventType.Disposed:
                    {
                        break;
                    }
                case PanelEventType.AllPanelsFound:
                    {
                        AppEventHandler.ForwardKeyPressEvent(this, !_disablePanelEventsFromBeingRouted);

                        if (Settings.Default.LoadStreamDeck == true && Process.GetProcessesByName("StreamDeck").Length >= 1 && HIDHandler.GetInstance().HIDSkeletons.Any(o => o.GamingPanelSkeleton.VendorId == (int)GamingPanelVendorEnum.Elgato))
                        {
                            MessageBox.Show("StreamDeck's official software is running in the background.", "Warning", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                        }

                        ButtonSearchPanels.IsEnabled = true;
                        break;
                    }
                default: throw new Exception("Failed to understand PanelEventType in MainWindow");
            }

            Dispatcher?.BeginInvoke((Action)SetWindowState);
        }

        private void ButtonSearchPanels_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                try
                {
                    Mouse.OverrideCursor = Cursors.Wait;
                    HIDHandler.GetInstance().Startup(Settings.Default.LoadStreamDeck);
                }
                finally
                {
                    Mouse.OverrideCursor = Cursors.Arrow;
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void LabelDonate_OnMouseEnter(object sender, MouseEventArgs e)
        {
            Mouse.OverrideCursor = Cursors.Hand;
        }

        private void LabelDonate_OnMouseLeave(object sender, MouseEventArgs e)
        {
            Mouse.OverrideCursor = Cursors.Arrow;
        }

        private void LabelDonate_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "https://www.paypal.com/paypalme/jerkerdahlblom",
                UseShellExecute = true
            });
        }

        private void MenuItemDiscordServer_OnClick(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "https://discord.gg/5svGwKX",
                UseShellExecute = true
            });
        }

        private void TryOpenAutoBackupFolder()
        {
            ProfileAutoBackup autoBackup = new();
            Process.Start(new ProcessStartInfo
            {
                FileName = autoBackup.AutoBackupFolderPath,
                UseShellExecute = true
            });
        }
    }
}
