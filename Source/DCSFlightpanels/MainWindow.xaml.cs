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
    using Properties;
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

    public partial class MainWindow : IGamingPanelListener, IDcsBiosConnectionListener, ISettingsModifiedListener, IProfileHandlerListener, IDisposable, IHardwareConflictResolver, IPanelEventListener, IForwardPanelEventListener, IDCSBIOSStringListener
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private List<KeyValuePair<string, GamingPanelEnum>> _profileFileHIDInstances = new();
        private readonly string _windowName = "DCSFlightpanels ";
        private readonly Timer _statusMessagesTimer = new(1000);
        private readonly List<string> _statusMessages = new();
        private readonly object _lockObjectStatusMessages = new();

        private ProfileHandler _profileHandler;
        private DCSBIOS _dcsBios;
        private bool _disablePanelEventsFromBeingRouted;
        private bool _isLoaded;
        private DCSBIOSOutput _dcsbiosVersionOutput;
        private bool _checkDCSBIOSVersionOnce;

        public MainWindow()
        {
            DarkModePrepare();
            InitializeComponent();

            DCSAircraft.Init();

            // Stop annoying "Cannot find source for binding with reference .... " from being shown
            PresentationTraceSources.DataBindingSource.Switch.Level = SourceLevels.Critical;

            AppEventHandler.AttachSettingsMonitoringListener(this);
            AppEventHandler.AttachSettingsModified(this);
            AppEventHandler.AttachPanelEventListener(this);
            AppEventHandler.AttachForwardPanelEventListener(this);
            BIOSEventHandler.AttachConnectionListener(this); 
            BIOSEventHandler.AttachStringListener(this);
            /*
             * Correct JSON folder path, move away from $USERDIRECTORY$.
             */
            Settings.Default.DCSBiosJSONLocation = Environment.ExpandEnvironmentVariables(Settings.Default.DCSBiosJSONLocation.Contains("$USERDIRECTORY$") ?
                Settings.Default.DCSBiosJSONLocation.Replace("$USERDIRECTORY$", "%userprofile%") : Settings.Default.DCSBiosJSONLocation);
            Settings.Default.Save();
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
                    //  dispose managed state (managed objects).
                    _statusMessagesTimer.Dispose();
                    _dcsBios?.Dispose();
                    AppEventHandler.DetachPanelEventListener(this);
                    AppEventHandler.DetachSettingsMonitoringListener(this);
                    AppEventHandler.DetachSettingsModified(this);
                    AppEventHandler.DetachForwardPanelEventListener(this);
                    BIOSEventHandler.DetachConnectionListener(this);
                    BIOSEventHandler.DetachStringListener(this);
                }

                //  free unmanaged resources (unmanaged objects) and override a finalizer below.

                //  set large fields to null.
                _hasBeenCalledAlready = true;
            }
        }

        //  override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~MainWindow() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);

            //  uncomment the following line if the finalizer is overridden above.
            GC.SuppressFinalize(this);
        }
        #endregion


        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            DarkMode.SetFrameworkElementDarkMode(this);
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

                DCSAircraft.FillModulesListFromDcsBios(Settings.Default.DCSBiosJSONLocation);

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
                    Settings.Default.Save();
                }

                FindDCSBIOSControls();

                _isLoaded = true;

            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }
        
        private void FindDCSBIOSControls()
        {
            if (!DCSAircraft.HasDCSBIOSModules || DCSAircraft.IsKeyEmulator(ProfileHandler.ActiveDCSAircraft) || DCSAircraft.IsKeyEmulatorSRS(ProfileHandler.ActiveDCSAircraft))
            {
                return;
            }

            _dcsbiosVersionOutput = DCSBIOSControlLocator.GetStringDCSBIOSOutput("DCS_BIOS");
        }

        public void DCSBIOSStringReceived(object sender, DCSBIOSStringDataEventArgs e)
        {
            try
            {
                if (_checkDCSBIOSVersionOnce || string.IsNullOrWhiteSpace(e.StringData) || _dcsbiosVersionOutput == null)
                {
                    return;
                }

                if (e.Address != _dcsbiosVersionOutput.Address)
                {
                    return;
                }

                Dispatcher?.Invoke(() =>
                {
                    LabelDCSBIOSVersion.Visibility = Visibility.Visible;
                    LabelDCSBIOSVersion.Text = "DCS-BIOS Version : " + e.StringData;
                });

                _checkDCSBIOSVersionOnce = true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "DCSBIOSStringReceived()");
            }
        }

        private void StartTimers()
        {
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
                LabelDCSBIOSVersion.Visibility = Visibility.Hidden;
            }
            else if (!DCSAircraft.IsNoFrameLoadedYet(dcsAircraft))
            {
                CreateDCSBIOS();
                StartupDCSBIOS();
                LabelDCSBIOSVersion.Visibility = Visibility.Visible;
            }
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
                ControlSpinningWheel.RotateGear(2000);
            }
        }

        private void StartupDCSBIOS()
        {
            if (_dcsBios.IsRunning)
            {
                return;
            }

            _dcsBios?.Startup();
        }

        private void ShutdownDCSBIOS()
        {
            _dcsBios?.Shutdown();
            _dcsBios = null;

            DCSBIOSControlLocator.DCSAircraft = _profileHandler.DCSAircraft;
            ControlSpinningWheel.Stop();
            ControlSpinningWheel.Visibility = Visibility.Collapsed;
        }

        private void CreatePanel(HIDSkeleton hidSkeleton)
        {
            try
            {
                TabControlPanels.AddPanel(hidSkeleton, _profileHandler.DCSAircraft, ref _profileFileHIDInstances);
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
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
            if (string.IsNullOrEmpty(fileVersionInfo.FileVersion)) return;

            var thisVersion = new Version(fileVersionInfo.FileVersion);

            try
            {
                var dateTime = Settings.Default.LastGitHubCheck;

                var client = new GitHubClient(new ProductHeaderValue("DCSFlightpanels"));
                var timeSpan = DateTime.Now - dateTime;
                if (timeSpan.Days > 1)
                {
                    Settings.Default.LastGitHubCheck = DateTime.Now;
                    Settings.Default.Save();
                    var lastRelease = await client.Repository.Release.GetLatest("DCS-Skunkworks", "DCSFlightpanels");
                    var githubVersion = new Version(lastRelease.TagName.Replace("v", ""));
                    if (githubVersion.CompareTo(thisVersion) > 0)
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
                }
                else
                {
                    Dispatcher?.Invoke(() =>
                    {
                        LabelVersionInformation.Text = "DCSFP version : " + fileVersionInfo.FileVersion;
                        LabelVersionInformation.Visibility = Visibility.Visible;
                    });
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error checking for newer releases.");
                LabelVersionInformation.Text = "DCSFP version : " + fileVersionInfo.FileVersion;
            }

            // #endif
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
                ControlSpinningWheel.Stop();
                _statusMessagesTimer.Stop();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }

            try
            {
                TabControlPanels.DisposePanels();
                _profileFileHIDInstances.Clear();
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
                    FileName = "https://github.com/DCS-Skunkworks/DCSFlightpanels/wiki",
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

        public void DcsBiosConnectionActive(object sender, DCSBIOSConnectionEventArgs e)
        {
            try
            {
                Dispatcher?.BeginInvoke((Action)(() => ControlSpinningWheel.RotateGear()));
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
                    DCSAircraft.FillModulesListFromDcsBios(Settings.Default.DCSBiosJSONLocation);
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
                            TabControlPanels.DisposePanels();
                            _profileFileHIDInstances.Clear();
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
                        Dispatcher?.Invoke((Action)(() => TabControlPanels.DisposePanel(e.HidSkeleton)));
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
            string folder = string.Empty;
            if (Settings.Default.AutoBackupDefaultFolderActive == false && !string.IsNullOrEmpty(Settings.Default.AutoBackupCustomFolderPath))
            {
                folder = Settings.Default.AutoBackupCustomFolderPath;
            }
            ProfileAutoBackup autoBackup = new(folder);
            Process.Start(new ProcessStartInfo
            {
                FileName = autoBackup.AutoBackupFolderPath,
                UseShellExecute = true
            });
        }

        private void MenuItemCTRLRef_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = AppDomain.CurrentDomain.BaseDirectory + "ctrlref.exe",
                    UseShellExecute = true
                });
                
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void LabelDownloadNewVersion_OnMouseEnter(object sender, MouseEventArgs e)
        {
            Mouse.OverrideCursor = Cursors.Hand;
        }

        private void LabelDownloadNewVersion_OnMouseLeave(object sender, MouseEventArgs e)
        {
            Mouse.OverrideCursor = Cursors.Arrow;
        }
    }
}
