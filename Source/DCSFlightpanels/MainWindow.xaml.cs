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
    using System.Text;
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

    using Microsoft.Win32;
    using NLog;
    using NLog.Targets;
    using NLog.Targets.Wrappers;
    using NonVisuals;
    using NonVisuals.EventArgs;
    using NonVisuals.Interfaces;
    using NonVisuals.Plugin;

    using Octokit;

    using Application = System.Windows.Application;
    using Cursors = System.Windows.Input.Cursors;
    using KeyEventArgs = System.Windows.Input.KeyEventArgs;
    using MenuItem = System.Windows.Controls.MenuItem;
    using MessageBox = System.Windows.MessageBox;
    using MouseEventArgs = System.Windows.Input.MouseEventArgs;
    using Timer = System.Timers.Timer;
    using UserControl = System.Windows.Controls.UserControl;

    public partial class MainWindow : IGamingPanelListener, IDcsBiosConnectionListener, ISettingsModifiedListener, IProfileHandlerListener, IDisposable, IHardwareConflictResolver, IPanelEventListener
    {
        internal static Logger logger = LogManager.GetCurrentClassLogger();

        private readonly List<KeyValuePair<string, GamingPanelEnum>> _profileFileInstanceIDs = new List<KeyValuePair<string, GamingPanelEnum>>();
        private readonly string _windowName = "DCSFlightpanels ";
        private readonly Timer _exceptionTimer = new Timer(1000);
        private readonly Timer _statusMessagesTimer = new Timer(1000);
        private readonly Timer _dcsStopGearTimer = new Timer(5000);
        private readonly Timer _dcsCheckDcsBiosStatusTimer = new Timer(5000);
        private readonly List<string> _statusMessages = new List<string>();
        private readonly object _lockObjectStatusMessages = new object();
        private readonly List<UserControl> _panelUserControls = new List<UserControl>();

        private ProfileHandler _profileHandler;
        private DCSBIOS _dcsBios;
        private bool _disablePanelEventsFromBeingRouted;
        private bool _isLoaded = false;

        public MainWindow()
        {
            InitializeComponent();

            DCSFPProfile.Init();

            // Stop annoying "Cannot find source for binding with reference .... " from being shown
            PresentationTraceSources.DataBindingSource.Switch.Level = SourceLevels.Critical;

            AppEventHandler.AttachSettingsMonitoringListener(this);
            AppEventHandler.AttachSettingsModified(this);
            AppEventHandler.AttachPanelEventListener(this);
            BIOSEventHandler.AttachConnectionListener(this);
        }

        #region IDisposable Support
        private bool _hasBeenCalledAlready = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!_hasBeenCalledAlready)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    _dcsCheckDcsBiosStatusTimer.Dispose();
                    _dcsStopGearTimer.Dispose();
                    _exceptionTimer.Dispose();
                    _statusMessagesTimer.Dispose();
                    _exceptionTimer.Dispose();
                    _dcsBios?.Dispose();
                    AppEventHandler.DetachPanelEventListener(this);
                    AppEventHandler.DetachSettingsMonitoringListener(this);
                    AppEventHandler.DetachSettingsModified(this);
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
            // GC.SuppressFinalize(this);
        }
        #endregion


        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
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


                DCSFPProfile.FillModulesListFromDcsBios(DBCommon.GetDCSBIOSJSONDirectory(Settings.Default.DCSBiosJSONLocation));

                CheckErrorLogAndDCSBIOSLocation();
                
                StartTimers();

                _profileHandler = new ProfileHandler(Settings.Default.DCSBiosJSONLocation, Settings.Default.LastProfileFileUsed, this);
                _profileHandler.Init();
                
                SetWindowTitle();
                SetWindowState();
                
                CheckForNewDCSFPRelease();

                ConfigurePlugins();

                _profileHandler.FindProfile();

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
            _dcsCheckDcsBiosStatusTimer.Elapsed += TimerCheckDcsBiosStatus;
            _statusMessagesTimer.Elapsed += TimerStatusMessagesTimer;
            _statusMessagesTimer.Start();
        }

        /// <summary>
        /// Try to find the path of the log with a file target given as parameter
        /// See NLog.config in the main folder of the application for configured log targets
        /// </summary>
        private string GetLogFilePathByTarget(string targetName)
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
                WrapperTargetBase wrapperTarget = target as WrapperTargetBase;

                // Unwrap the target if necessary.
                if (wrapperTarget == null)
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

        private void CheckErrorLogAndDCSBIOSLocation()
        {
            // FUGLY, I know but something quick to help the users
            try
            {
                string errorLogFilePath = GetLogFilePathByTarget("error_logfile");
                if (errorLogFilePath == null || !File.Exists(errorLogFilePath))
                {
                    return;
                }

                var loggerText = File.ReadAllText(errorLogFilePath);
                if (loggerText.Contains(DCSBIOSControlLocator.DCSBIOSNotFoundErrorMessage))
                {
                    var window = new DCSBIOSNotFoundWindow(Settings.Default.DCSBiosJSONLocation);
                    window.ShowDialog();
                    MessageBox.Show(
                        $"This warning will be shown as long as there are error messages in error log stating that DCS-BIOS can not be found. Delete or clear the error log{Environment.NewLine}{errorLogFilePath}{Environment.NewLine} once you have fixed the problem.",
                        "Delete Error Log",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void SetApplicationMode()
        {
            var dcsfpProfile = _profileHandler.Profile;

            if (!IsLoaded)
            {
                return;
            }

            LabelAirframe.Content = DCSFPProfile.IsNoFrameLoadedYet(dcsfpProfile) ? string.Empty : dcsfpProfile.Description;

            if (Common.IsEmulationModesFlagSet(EmulationMode.KeyboardEmulationOnly))
            {
                ShutdownDCSBIOS();
                CloseStreamDecks();
            }
            else if (!DCSFPProfile.IsNoFrameLoadedYet(dcsfpProfile))
            {
                CreateDCSBIOS();
                StartupDCSBIOS();
            }
        }

        private void CloseStreamDecks()
        {
            foreach (var hidSkeleton in HIDHandler.GetInstance().HIDSkeletons)
            {
                if (Common.IsStreamDeck(hidSkeleton.PanelInfo.GamingPanelType))
                {
                    CloseTabItem(hidSkeleton.InstanceId);
                }
            }
        }

        private void CloseTabItem(string instanceId)
        {
            try
            {
                var counter = 0;
                while (TabControlPanels.Items.Count > counter)
                {
                    var tabItem = (TabItem)TabControlPanels.Items[counter];
                    var userControl = (UserControlBase)tabItem.Content;
                    var gamingPanelUserControl = (IGamingPanelUserControl)tabItem.Content;
                    var gamingPanel = gamingPanelUserControl.GetGamingPanel();

                    if (gamingPanel != null && gamingPanel.HIDInstanceId == instanceId)
                    {
                        TabControlPanels.Items.Remove(tabItem);
                        userControl.Dispose();
                        _panelUserControls.Remove(userControl);

                        for (int i = 0; i < _profileFileInstanceIDs.Count; i++)
                        {
                            if (_profileFileInstanceIDs[i].Key == gamingPanel.HIDInstanceId)
                            {
                                _profileFileInstanceIDs.RemoveAt(i);
                                break;
                            }
                        }

                        break;
                    }

                    counter++;
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private int CloseTabItems()
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

                    _profileFileInstanceIDs.Clear();
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
                logger.Error(message);
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

            DCSBIOSControlLocator.Profile = _profileHandler.Profile;

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
            _dcsCheckDcsBiosStatusTimer.Start();
        }

        private void ShutdownDCSBIOS()
        {
            _dcsBios?.Shutdown();
            _dcsBios = null;

            DCSBIOSControlLocator.Profile = _profileHandler.Profile;
            _dcsStopGearTimer.Stop();
            _dcsCheckDcsBiosStatusTimer.Stop();
            ImageDcsBiosConnected.Visibility = Visibility.Collapsed;
        }

        private void DisposePanel(HIDSkeleton hidSkeleton)
        {
            for (var i = 0; i < TabControlPanels.Items.Count; i++)
            {
                var tabItem = (TabItem)TabControlPanels.Items.GetItemAt(i);
                var userControl = (IGamingPanelUserControl)tabItem.Content;

                if (userControl.GetGamingPanel().HIDInstanceId == hidSkeleton.InstanceId)
                {
                    userControl.Dispose();
                    TabControlPanels.Items.RemoveAt(i);
                    AppEventHandler.PanelEvent(this, hidSkeleton.InstanceId, hidSkeleton, PanelEventType.Disposed);
                    break;
                }
            }
        }

        private void CreatePanel(HIDSkeleton hidSkeleton)
        {
            try
            {
                switch (hidSkeleton.PanelInfo.GamingPanelType)
                {
                    case GamingPanelEnum.PZ55SwitchPanel:
                        {
                            var tabItem = new TabItem { Header = "PZ55" };
                            var switchPanelPZ55UserControl = new SwitchPanelPZ55UserControl(hidSkeleton, tabItem);
                            _panelUserControls.Add(switchPanelPZ55UserControl);
                            tabItem.Content = switchPanelPZ55UserControl;
                            TabControlPanels.Items.Add(tabItem);
                            _profileFileInstanceIDs.Add(new KeyValuePair<string, GamingPanelEnum>(hidSkeleton.HIDReadDevice.DevicePath, hidSkeleton.PanelInfo.GamingPanelType));
                            break;
                        }

                    case GamingPanelEnum.PZ70MultiPanel:
                        {
                            var tabItem = new TabItem { Header = "PZ70" };
                            var multiPanelUserControl = new MultiPanelUserControl(hidSkeleton, tabItem);
                            _panelUserControls.Add(multiPanelUserControl);
                            tabItem.Content = multiPanelUserControl;
                            TabControlPanels.Items.Add(tabItem);
                            _profileFileInstanceIDs.Add(new KeyValuePair<string, GamingPanelEnum>(hidSkeleton.HIDReadDevice.DevicePath, hidSkeleton.PanelInfo.GamingPanelType));
                            break;
                        }

                    case GamingPanelEnum.BackLitPanel:
                        {
                            var tabItem = new TabItem { Header = "B.I.P." };
                            var backLitPanelUserControl = new BackLitPanelUserControl(tabItem, hidSkeleton);
                            _panelUserControls.Add(backLitPanelUserControl);
                            tabItem.Content = backLitPanelUserControl;
                            TabControlPanels.Items.Add(tabItem);
                            _profileFileInstanceIDs.Add(new KeyValuePair<string, GamingPanelEnum>(hidSkeleton.HIDReadDevice.DevicePath, hidSkeleton.PanelInfo.GamingPanelType));
                            break;
                        }

                    case GamingPanelEnum.TPM:
                        {
                            var tabItem = new TabItem { Header = "TPM" };
                            var tpmPanelUserControl = new TPMPanelUserControl(hidSkeleton, tabItem);
                            _panelUserControls.Add(tpmPanelUserControl);
                            tabItem.Content = tpmPanelUserControl;
                            TabControlPanels.Items.Add(tabItem);
                            _profileFileInstanceIDs.Add(new KeyValuePair<string, GamingPanelEnum>(hidSkeleton.HIDReadDevice.DevicePath, hidSkeleton.PanelInfo.GamingPanelType));
                            break;
                        }

                    case GamingPanelEnum.StreamDeckMini:
                    case GamingPanelEnum.StreamDeckXL:
                    case GamingPanelEnum.StreamDeck:
                    case GamingPanelEnum.StreamDeckV2:
                    case GamingPanelEnum.StreamDeckMK2:
                        {
                            if (!DCSFPProfile.IsKeyEmulator(_profileHandler.Profile))
                            {
                                var tabItemStreamDeck = new TabItem { Header = hidSkeleton.PanelInfo.GamingPanelType.GetEnumDescriptionField() };
                                var streamDeckUserControl = new StreamDeckUserControl(hidSkeleton.PanelInfo.GamingPanelType, hidSkeleton, tabItemStreamDeck);
                                _panelUserControls.Add(streamDeckUserControl);
                                tabItemStreamDeck.Content = streamDeckUserControl;
                                TabControlPanels.Items.Add(tabItemStreamDeck);
                                _profileFileInstanceIDs.Add(new KeyValuePair<string, GamingPanelEnum>(hidSkeleton.InstanceId, hidSkeleton.PanelInfo.GamingPanelType));
                            }

                            break;
                        }

                    case GamingPanelEnum.FarmingPanel:
                        {
                            var tabItem = new TabItem { Header = "Side Panel" };
                            var farmingSidePanelUserControl = new FarmingPanelUserControl(hidSkeleton, tabItem);
                            _panelUserControls.Add(farmingSidePanelUserControl);
                            tabItem.Content = farmingSidePanelUserControl;
                            TabControlPanels.Items.Add(tabItem);
                            _profileFileInstanceIDs.Add(new KeyValuePair<string, GamingPanelEnum>(hidSkeleton.HIDReadDevice.DevicePath, hidSkeleton.PanelInfo.GamingPanelType));
                            break;
                        }

                    case GamingPanelEnum.PZ69RadioPanel:
                        {
                            var tabItem = new TabItem { Header = "PZ69" };
                            if (DCSFPProfile.IsKeyEmulator(_profileHandler.Profile))
                            {
                                var radioPanelPZ69UserControl = new RadioPanelPZ69UserControlEmulator(hidSkeleton, tabItem);
                                _panelUserControls.Add(radioPanelPZ69UserControl);
                                tabItem.Content = radioPanelPZ69UserControl;
                                TabControlPanels.Items.Add(tabItem);
                                _profileFileInstanceIDs.Add(new KeyValuePair<string, GamingPanelEnum>(hidSkeleton.HIDReadDevice.DevicePath, hidSkeleton.PanelInfo.GamingPanelType));
                            }
                            else if (DCSFPProfile.IsA10C(_profileHandler.Profile) && !_profileHandler.Profile.UseGenericRadio)
                            {
                                var radioPanelPZ69UserControl = new RadioPanelPZ69UserControlA10C(hidSkeleton, tabItem);
                                _panelUserControls.Add(radioPanelPZ69UserControl);
                                tabItem.Content = radioPanelPZ69UserControl;
                                TabControlPanels.Items.Add(tabItem);
                                _profileFileInstanceIDs.Add(new KeyValuePair<string, GamingPanelEnum>(hidSkeleton.HIDReadDevice.DevicePath, hidSkeleton.PanelInfo.GamingPanelType));
                            }
                            else if (DCSFPProfile.IsUH1H(_profileHandler.Profile) && !_profileHandler.Profile.UseGenericRadio)
                            {
                                var radioPanelPZ69UserControl = new RadioPanelPZ69UserControlUH1H(hidSkeleton, tabItem);
                                _panelUserControls.Add(radioPanelPZ69UserControl);
                                tabItem.Content = radioPanelPZ69UserControl;
                                TabControlPanels.Items.Add(tabItem);
                                _profileFileInstanceIDs.Add(new KeyValuePair<string, GamingPanelEnum>(hidSkeleton.HIDReadDevice.DevicePath, hidSkeleton.PanelInfo.GamingPanelType));
                            }
                            else if (DCSFPProfile.IsMiG21Bis(_profileHandler.Profile) && !_profileHandler.Profile.UseGenericRadio)
                            {
                                var radioPanelPZ69UserControl = new RadioPanelPZ69UserControlMiG21Bis(hidSkeleton, tabItem);
                                _panelUserControls.Add(radioPanelPZ69UserControl);
                                tabItem.Content = radioPanelPZ69UserControl;
                                TabControlPanels.Items.Add(tabItem);
                                _profileFileInstanceIDs.Add(new KeyValuePair<string, GamingPanelEnum>(hidSkeleton.HIDReadDevice.DevicePath, hidSkeleton.PanelInfo.GamingPanelType));
                            }
                            else if (DCSFPProfile.IsKa50(_profileHandler.Profile) && !_profileHandler.Profile.UseGenericRadio)
                            {
                                var radioPanelPZ69UserControl = new RadioPanelPZ69UserControlKa50(hidSkeleton, tabItem);
                                _panelUserControls.Add(radioPanelPZ69UserControl);
                                tabItem.Content = radioPanelPZ69UserControl;
                                TabControlPanels.Items.Add(tabItem);
                                _profileFileInstanceIDs.Add(new KeyValuePair<string, GamingPanelEnum>(hidSkeleton.HIDReadDevice.DevicePath, hidSkeleton.PanelInfo.GamingPanelType));
                            }
                            else if (DCSFPProfile.IsMi8MT(_profileHandler.Profile) && !_profileHandler.Profile.UseGenericRadio)
                            {
                                var radioPanelPZ69UserControl = new RadioPanelPZ69UserControlMi8(hidSkeleton, tabItem);
                                _panelUserControls.Add(radioPanelPZ69UserControl);
                                tabItem.Content = radioPanelPZ69UserControl;
                                TabControlPanels.Items.Add(tabItem);
                                _profileFileInstanceIDs.Add(new KeyValuePair<string, GamingPanelEnum>(hidSkeleton.HIDReadDevice.DevicePath, hidSkeleton.PanelInfo.GamingPanelType));
                            }
                            else if (DCSFPProfile.IsBf109K4(_profileHandler.Profile) && !_profileHandler.Profile.UseGenericRadio)
                            {
                                var radioPanelPZ69UserControl = new RadioPanelPZ69UserControlBf109(hidSkeleton, tabItem);
                                _panelUserControls.Add(radioPanelPZ69UserControl);
                                tabItem.Content = radioPanelPZ69UserControl;
                                TabControlPanels.Items.Add(tabItem);
                                _profileFileInstanceIDs.Add(new KeyValuePair<string, GamingPanelEnum>(hidSkeleton.HIDReadDevice.DevicePath, hidSkeleton.PanelInfo.GamingPanelType));
                            }
                            else if (DCSFPProfile.IsFW190D9(_profileHandler.Profile) && !_profileHandler.Profile.UseGenericRadio)
                            {
                                var radioPanelPZ69UserControl = new RadioPanelPZ69UserControlFw190(hidSkeleton, tabItem);
                                _panelUserControls.Add(radioPanelPZ69UserControl);
                                tabItem.Content = radioPanelPZ69UserControl;
                                TabControlPanels.Items.Add(tabItem);
                                _profileFileInstanceIDs.Add(new KeyValuePair<string, GamingPanelEnum>(hidSkeleton.HIDReadDevice.DevicePath, hidSkeleton.PanelInfo.GamingPanelType));
                            }
                            else if (DCSFPProfile.IsP51D(_profileHandler.Profile) && !_profileHandler.Profile.UseGenericRadio)
                            {
                                var radioPanelPZ69UserControl = new RadioPanelPZ69UserControlP51D(hidSkeleton, tabItem);
                                _panelUserControls.Add(radioPanelPZ69UserControl);
                                tabItem.Content = radioPanelPZ69UserControl;
                                TabControlPanels.Items.Add(tabItem);
                                _profileFileInstanceIDs.Add(new KeyValuePair<string, GamingPanelEnum>(hidSkeleton.HIDReadDevice.DevicePath, hidSkeleton.PanelInfo.GamingPanelType));
                            }
                            else if (DCSFPProfile.IsF86F(_profileHandler.Profile) && !_profileHandler.Profile.UseGenericRadio)
                            {
                                var radioPanelPZ69UserControl = new RadioPanelPZ69UserControlF86F(hidSkeleton, tabItem);
                                _panelUserControls.Add(radioPanelPZ69UserControl);
                                tabItem.Content = radioPanelPZ69UserControl;
                                TabControlPanels.Items.Add(tabItem);
                                _profileFileInstanceIDs.Add(new KeyValuePair<string, GamingPanelEnum>(hidSkeleton.HIDReadDevice.DevicePath, hidSkeleton.PanelInfo.GamingPanelType));
                            }
                            else if (DCSFPProfile.IsSpitfireLFMkIX(_profileHandler.Profile) && !_profileHandler.Profile.UseGenericRadio)
                            {
                                var radioPanelPZ69UserControl = new RadioPanelPZ69UserControlSpitfireLFMkIX(hidSkeleton, tabItem);
                                _panelUserControls.Add(radioPanelPZ69UserControl);
                                tabItem.Content = radioPanelPZ69UserControl;
                                TabControlPanels.Items.Add(tabItem);
                                _profileFileInstanceIDs.Add(new KeyValuePair<string, GamingPanelEnum>(hidSkeleton.HIDReadDevice.DevicePath, hidSkeleton.PanelInfo.GamingPanelType));
                            }
                            else if (DCSFPProfile.IsAJS37(_profileHandler.Profile) && !_profileHandler.Profile.UseGenericRadio)
                            {
                                var radioPanelPZ69UserControl = new RadioPanelPZ69UserControlAJS37(hidSkeleton, tabItem);
                                _panelUserControls.Add(radioPanelPZ69UserControl);
                                tabItem.Content = radioPanelPZ69UserControl;
                                TabControlPanels.Items.Add(tabItem);
                                _profileFileInstanceIDs.Add(new KeyValuePair<string, GamingPanelEnum>(hidSkeleton.HIDReadDevice.DevicePath, hidSkeleton.PanelInfo.GamingPanelType));
                            }
                            else if (DCSFPProfile.IsSA342(_profileHandler.Profile) && !_profileHandler.Profile.UseGenericRadio)
                            {
                                var radioPanelPZ69UserControl = new RadioPanelPZ69UserControlSA342(hidSkeleton, tabItem);
                                _panelUserControls.Add(radioPanelPZ69UserControl);
                                tabItem.Content = radioPanelPZ69UserControl;
                                TabControlPanels.Items.Add(tabItem);
                                _profileFileInstanceIDs.Add(new KeyValuePair<string, GamingPanelEnum>(hidSkeleton.HIDReadDevice.DevicePath, hidSkeleton.PanelInfo.GamingPanelType));
                            }
                            else if (DCSFPProfile.IsFA18C(_profileHandler.Profile) && !_profileHandler.Profile.UseGenericRadio)
                            {
                                var radioPanelPZ69UserControl = new RadioPanelPZ69UserControlFA18C(hidSkeleton, tabItem);
                                _panelUserControls.Add(radioPanelPZ69UserControl);
                                tabItem.Content = radioPanelPZ69UserControl;
                                TabControlPanels.Items.Add(tabItem);
                                _profileFileInstanceIDs.Add(new KeyValuePair<string, GamingPanelEnum>(hidSkeleton.HIDReadDevice.DevicePath, hidSkeleton.PanelInfo.GamingPanelType));
                            }
                            else if (DCSFPProfile.IsM2000C(_profileHandler.Profile) && !_profileHandler.Profile.UseGenericRadio)
                            {
                                var radioPanelPZ69UserControl = new RadioPanelPZ69UserControlM2000C(hidSkeleton, tabItem);
                                _panelUserControls.Add(radioPanelPZ69UserControl);
                                tabItem.Content = radioPanelPZ69UserControl;
                                TabControlPanels.Items.Add(tabItem);
                                _profileFileInstanceIDs.Add(new KeyValuePair<string, GamingPanelEnum>(hidSkeleton.HIDReadDevice.DevicePath, hidSkeleton.PanelInfo.GamingPanelType));
                            }
                            else if (DCSFPProfile.IsF5E(_profileHandler.Profile) && !_profileHandler.Profile.UseGenericRadio)
                            {
                                var radioPanelPZ69UserControl = new RadioPanelPZ69UserControlF5E(hidSkeleton, tabItem);
                                _panelUserControls.Add(radioPanelPZ69UserControl);
                                tabItem.Content = radioPanelPZ69UserControl;
                                TabControlPanels.Items.Add(tabItem);
                                _profileFileInstanceIDs.Add(new KeyValuePair<string, GamingPanelEnum>(hidSkeleton.HIDReadDevice.DevicePath, hidSkeleton.PanelInfo.GamingPanelType));
                            }
                            else if (DCSFPProfile.IsF14B(_profileHandler.Profile) && !_profileHandler.Profile.UseGenericRadio)
                            {
                                var radioPanelPZ69UserControl = new RadioPanelPZ69UserControlF14B(hidSkeleton, tabItem);
                                _panelUserControls.Add(radioPanelPZ69UserControl);
                                tabItem.Content = radioPanelPZ69UserControl;
                                TabControlPanels.Items.Add(tabItem);
                                _profileFileInstanceIDs.Add(new KeyValuePair<string, GamingPanelEnum>(hidSkeleton.HIDReadDevice.DevicePath, hidSkeleton.PanelInfo.GamingPanelType));
                            }
                            else if (DCSFPProfile.IsAV8B(_profileHandler.Profile) && !_profileHandler.Profile.UseGenericRadio)
                            {
                                var radioPanelPZ69UserControl = new RadioPanelPZ69UserControlAV8BNA(hidSkeleton, tabItem);
                                _panelUserControls.Add(radioPanelPZ69UserControl);
                                tabItem.Content = radioPanelPZ69UserControl;
                                TabControlPanels.Items.Add(tabItem);
                                _profileFileInstanceIDs.Add(new KeyValuePair<string, GamingPanelEnum>(hidSkeleton.HIDReadDevice.DevicePath, hidSkeleton.PanelInfo.GamingPanelType));
                            }
                            else if (DCSFPProfile.IsP47D(_profileHandler.Profile) && !_profileHandler.Profile.UseGenericRadio)
                            {
                                var radioPanelPZ69UserControl = new RadioPanelPZ69UserControlP47D(hidSkeleton, tabItem);
                                _panelUserControls.Add(radioPanelPZ69UserControl);
                                tabItem.Content = radioPanelPZ69UserControl;
                                TabControlPanels.Items.Add(tabItem);
                                _profileFileInstanceIDs.Add(new KeyValuePair<string, GamingPanelEnum>(hidSkeleton.HIDReadDevice.DevicePath, hidSkeleton.PanelInfo.GamingPanelType));
                            }
                            else if (DCSFPProfile.IsMi24P(_profileHandler.Profile) && !_profileHandler.Profile.UseGenericRadio)
                            {
                                var radioPanelPZ69UserControl = new RadioPanelPZ69UserControlMi24P(hidSkeleton, tabItem);
                                _panelUserControls.Add(radioPanelPZ69UserControl);
                                tabItem.Content = radioPanelPZ69UserControl;
                                TabControlPanels.Items.Add(tabItem);
                                _profileFileInstanceIDs.Add(new KeyValuePair<string, GamingPanelEnum>(hidSkeleton.HIDReadDevice.DevicePath, hidSkeleton.PanelInfo.GamingPanelType));
                            }
                            else
                            {
                                var radioPanelPZ69UserControl = new RadioPanelPZ69UserControlGeneric(hidSkeleton, tabItem);
                                _panelUserControls.Add(radioPanelPZ69UserControl);
                                tabItem.Content = radioPanelPZ69UserControl;
                                TabControlPanels.Items.Add(tabItem);
                                _profileFileInstanceIDs.Add(new KeyValuePair<string, GamingPanelEnum>(hidSkeleton.HIDReadDevice.DevicePath, hidSkeleton.PanelInfo.GamingPanelType));
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

            Common.APIMode = Settings.Default.APIMode == 0 ? APIModeEnum.keybd_event : APIModeEnum.SendInput;
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

        public void LedLightChanged(object sender, LedLightChangeEventArgs e) { }

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

        private void TimerCheckDcsBiosStatus(object sender, ElapsedEventArgs e)
        {
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
                logger.Error(ex, "Error checking for newer releases.");
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
            if (DCSFPProfile.IsNoFrameLoadedYet(_profileHandler.Profile))
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
                SaveNewOrExistingProfile();
                SetWindowState();
                SystemSounds.Asterisk.Play();
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
                // Wtf is hanging?
                Application.Current.Shutdown();
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
                _dcsCheckDcsBiosStatusTimer.Stop();
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
                Application.Current.Shutdown();
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
            _profileHandler.SaveProfile();
            SetWindowState();
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
                MenuItemUseNS430.IsEnabled = menuIsEnabled && DCSFPProfile.HasNS430();
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
                _profileHandler.OpenProfile();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }
        /*
        private bool CloseProfile()
        {
            if (_profileHandler.IsDirty && MessageBox.Show("Discard unsaved changes to current profile?", "Discard changes?", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
            {
                return false;
            }

            try
            {
                CloseTabItems();
                _profileHandler = new ProfileHandler(Settings.Default.DCSBiosJSONLocation, this);
                _profileHandler.Init();
                //_profileHandler.AttachUserMessageHandler(this);
                _dcsfpProfile = _profileHandler.Profile;
                SetApplicationMode(_dcsfpProfile);
                SetWindowTitle();
                SetWindowState();
                // Disabling can be used when user want to reset panel switches and does not want that resetting switches affects the game.
                AppEventHandler.ForwardKeyPressEvent(this, !_disablePanelEventsFromBeingRouted);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                if (ex.InnerException != null)
                    logger.Error(ex.InnerException);
                throw;
            }

            return true;
        }
        */
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
                Process.Start("https://github.com/DCSFlightpanels/DCSFlightpanels/wiki");
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
                _profileHandler.OpenProfile();
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
                RefreshProfile();
                SystemSounds.Beep.Play();
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

        private void ButtonImageDisableMouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                var forwardKeys = bool.Parse(ButtonImageDisable.Tag.ToString());
                forwardKeys = !forwardKeys;
                ButtonImageDisable.Tag = forwardKeys ? "True" : "False";
                // Disabling can be used when user want to reset panel switches and does not want that resetting switches affects the game.
                AppEventHandler.ForwardKeyPressEvent(this, !_disablePanelEventsFromBeingRouted);
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void OpenProfileInNotepad()
        {
            Process.Start(_profileHandler.Filename);
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

        public void DeviceAttached(GamingPanelEnum gamingPanelsEnum)
        {
            // ignore
        }

        public void DeviceDetached(GamingPanelEnum gamingPanelsEnum)
        {
            // ignore
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

        private void TryOpenLogFileWithTarget(string targetName)
        {
            try
            {
                string errorLogFilePath = GetLogFilePathByTarget(targetName);
                if (errorLogFilePath == null || !File.Exists(errorLogFilePath))
                {
                    MessageBox.Show($"No log file found {errorLogFilePath}", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
                Process.Start(errorLogFilePath);
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

        private void LoadProcessPriority()
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
                Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
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
            var settingsWindow = new SettingsWindow();
            if (settingsWindow.ShowDialog() == true)
            {
                if (settingsWindow.GeneralChanged)
                {
                    LoadProcessPriority();
                }

                if (settingsWindow.DCSBIOSChanged && Common.PartialDCSBIOSEnabled())
                {
                    // Refresh, make sure they are using the latest settings
                    DCSBIOSControlLocator.JSONDirectory = Settings.Default.DCSBiosJSONLocation;
                    _dcsBios.ReceiveFromIpUdp = Settings.Default.DCSBiosIPFrom;
                    _dcsBios.ReceivePortUdp = int.Parse(Settings.Default.DCSBiosPortFrom);
                    _dcsBios.SendToIpUdp = Settings.Default.DCSBiosIPTo;
                    _dcsBios.SendPortUdp = int.Parse(Settings.Default.DCSBiosPortTo);
                    _dcsBios.Shutdown();
                    _dcsBios.Startup();
                    _profileHandler.DCSBIOSJSONDirectory = Settings.Default.DCSBiosJSONLocation;
                }

                ConfigurePlugins();
            }
        }

        private void FixUSBEnhancedPowerManagerIssues()
        {
            /*
             * This is a slightly modified code version of the original code which was made by http://uraster.com
             */
            MessageBox.Show("You need to run DCSFP as Administrator for this function to work!");
            const string SaitekVid = "VID_06A3";
            var result = new StringBuilder();
            result.AppendLine("USB Enhanced Power Management Disabler");
            result.AppendLine("http://uraster.com/en-us/products/usbenhancedpowermanagerdisabler.aspx");
            result.AppendLine("Copywrite Uraster GmbH");
            result.AppendLine(new string('=', 60));
            result.AppendLine("This application disables the enhanced power management for the all USB devices of a specific vendor.");
            result.AppendLine("You need admin rights to do that.");
            result.AppendLine("Plug in all devices in the ports you intend to use before continuing.");
            result.AppendLine(new string('-', 60));
            result.AppendLine("Vendor ID (VID). For SAITEK use the default of " + SaitekVid);

            try
            {
                var devicesDisabled = 0;
                var devicesAlreadyDisabled = 0;
                using (var usbDevicesKey = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Enum\USB"))
                {
                    if (usbDevicesKey != null)
                    {
                        foreach (var usbDeviceKeyName in usbDevicesKey.GetSubKeyNames().Where(name => name.StartsWith(SaitekVid)))
                        {
                            result.Append(Environment.NewLine);
                            result.AppendLine("Processing product : " + GetProductId(SaitekVid, usbDeviceKeyName));
                            using (var usbDeviceKey = usbDevicesKey.OpenSubKey(usbDeviceKeyName))
                            {
                                if (usbDeviceKey != null)
                                {
                                    foreach (var instanceKeyName in usbDeviceKey.GetSubKeyNames())
                                    {
                                        result.AppendLine("Device instance : " + instanceKeyName);
                                        using (var instanceKey = usbDeviceKey.OpenSubKey(instanceKeyName))
                                        {
                                            if (instanceKey != null)
                                            {
                                                using (var deviceParametersKey = instanceKey.OpenSubKey("Device Parameters", true))
                                                {
                                                    if (deviceParametersKey == null)
                                                    {
                                                        result.AppendLine("no parameters, skipping");
                                                        continue;
                                                    }

                                                    var value = deviceParametersKey.GetValue("EnhancedPowerManagementEnabled");
                                                    if (0.Equals(value))
                                                    {
                                                        result.AppendLine("enhanced power management is already disabled");
                                                        devicesAlreadyDisabled++;
                                                    }
                                                    else
                                                    {
                                                        result.Append("enhanced power management is enabled, disabling... ");
                                                        deviceParametersKey.SetValue("EnhancedPowerManagementEnabled", 0);
                                                        result.AppendLine("now disabled");
                                                        devicesDisabled++;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    result.AppendLine(new string('-', 60));
                    result.AppendLine("Done. Unplug all devices and plug them again in the same ports.");
                    result.AppendLine("Device instances fixed " + devicesDisabled);
                    result.AppendLine("Device instances already fixed " + devicesAlreadyDisabled);
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex, "Error disabling Enhanced USB Power Management.");
                return;
            }

            var informationWindow = new InformationTextBlockWindow(result.ToString());
            informationWindow.ShowDialog();
        }

        private string GetProductId(string saitekVID, string usbDeviceKeyName)
        {
            var pos = usbDeviceKeyName.IndexOf("&", StringComparison.InvariantCulture);
            var vid = usbDeviceKeyName.Substring(0, pos);
            var pid = usbDeviceKeyName.Substring(pos + 1);
            var result = pid;
            if (vid == saitekVID)
            {
                if (pid.StartsWith("PID_0D06"))
                {
                    result += " (Multi panel, PZ70)";
                }
                else if (pid.StartsWith("PID_0D05"))
                {
                    result += " (Radio panel, PZ69)";
                }
                else if (pid.StartsWith("PID_0D67"))
                {
                    result += " (Switch panel, PZ55)";
                }
                else if (pid.StartsWith("PID_A2AE"))
                {
                    result += " (Instrument panel)";
                }
                else if (pid.StartsWith("PID_712C"))
                {
                    result += " (Yoke)";
                }
                else if (pid.StartsWith("PID_0C2D"))
                {
                    result += " (Throttle quadrant)";
                }
                else if (pid.StartsWith("PID_0763"))
                {
                    result += " (Pedals)";
                }
                else if (pid.StartsWith("PID_0B4E"))
                {
                    result += " (BIP)";
                }
            }

            return result;
        }

        private void MenuItemUSBPowerManagement_OnClick(object sender, RoutedEventArgs e)
        {
            FixUSBEnhancedPowerManagerIssues();
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

        private void ButtonDisableAllPanelInteractions_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                _disablePanelEventsFromBeingRouted = !_disablePanelEventsFromBeingRouted;
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

                // Disabling can be used when user want to reset panel switches and does not want that resetting switches affects the game.
                AppEventHandler.ForwardKeyPressEvent(this, !_disablePanelEventsFromBeingRouted);
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
            var bindingsMappingWindow = new BindingsMappingWindow(BindingMappingManager.PanelBindings, GamingPanel.GamingPanels);
            bindingsMappingWindow.ShowDialog();

            return bindingsMappingWindow.ModifiedGenericBindings;
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

        public void ProfileEvent(object sender, ProfileEventArgs e)
        {
            try
            {
                if (e.DCSProfile == DCSFPProfile.GetNoFrameLoadedYet())
                {
                    return;
                }

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
                            CloseTabItems();
                            SetWindowTitle();
                            SetWindowState();
                            break;
                        }
                }

                MenuItemUseNS430.IsChecked = _profileHandler.UseNS430;

                SetWindowState();
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
                        CreatePanel(e.HidSkeleton);
                        break;
                    }
                case PanelEventType.Attached:
                    {
                        CreatePanel(e.HidSkeleton);
                        break;
                    }
                case PanelEventType.Detached:
                    {
                        DisposePanel(e.HidSkeleton);
                        break;
                    }
                case PanelEventType.Disposed:
                    {
                        break;
                    }
                case PanelEventType.Created:
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

                        break;
                    }
                default: throw new Exception("Failed to understand PanelEventType in MainWindow");
            }

            SetWindowState();
        }
    }
}