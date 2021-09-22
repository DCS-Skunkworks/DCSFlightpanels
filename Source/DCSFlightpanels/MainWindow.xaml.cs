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

    using DCSFlightpanels.Interfaces;
    using DCSFlightpanels.PanelUserControls;
    using DCSFlightpanels.Properties;
    using DCSFlightpanels.Radios.Emulators;
    using DCSFlightpanels.Radios.PreProgrammed;
    using DCSFlightpanels.Shared;
    using DCSFlightpanels.Windows;

    using Microsoft.Win32;

    using NonVisuals;
    using NonVisuals.EventArgs;
    using NonVisuals.Interfaces;
    using NonVisuals.Plugin;
    using NonVisuals.Radios.SRS;

    using Octokit;

    using Application = System.Windows.Application;
    using Button = System.Windows.Controls.Button;
    using Cursors = System.Windows.Input.Cursors;
    using KeyEventArgs = System.Windows.Input.KeyEventArgs;
    using MenuItem = System.Windows.Controls.MenuItem;
    using MessageBox = System.Windows.MessageBox;
    using MouseEventArgs = System.Windows.Input.MouseEventArgs;
    using Timer = System.Timers.Timer;
    using UserControl = System.Windows.Controls.UserControl;

    public partial class MainWindow : IGamingPanelListener, IDcsBiosConnectionListener, IGlobalHandler, IProfileHandlerListener, IUserMessageHandler, IDisposable, IHardwareConflictResolver
    {
        private readonly List<KeyValuePair<string, GamingPanelEnum>> _profileFileInstanceIDs = new List<KeyValuePair<string, GamingPanelEnum>>();
        private readonly List<GamingPanel> _gamingPanels = new List<GamingPanel>();
        private readonly string _windowName = "DCSFlightpanels ";
        private readonly Timer _exceptionTimer = new Timer(1000);
        private readonly Timer _statusMessagesTimer = new Timer(1000);
        private readonly Timer _dcsStopGearTimer = new Timer(5000);
        private readonly Timer _dcsCheckDcsBiosStatusTimer = new Timer(5000);
        private readonly List<string> _statusMessages = new List<string>();
        private readonly object _lockObjectStatusMessages = new object();
        private readonly List<UserControl> _panelUserControls = new List<UserControl>();
        private readonly string _debugLogFile = AppDomain.CurrentDomain.BaseDirectory + "DCSFlightpanels_debug_log.txt";
        private readonly string _errorLogFile = AppDomain.CurrentDomain.BaseDirectory + "DCSFlightpanels_error_log.txt";

        private readonly bool _doSearchForPanels = true;
        private HIDHandler _hidHandler;
        private ProfileHandler _profileHandler;
        private DCSBIOS _dcsBios;
        private DCSFPProfile _dcsfpProfile;
        private bool _disablePanelEventsFromBeingRouted;
        private bool _isLoaded = false;

        public MainWindow()
        {
            InitializeComponent();

            DCSFPProfile.Init();

            // Stop annoying "Cannot find source for binding with reference .... " from being shown
            PresentationTraceSources.DataBindingSource.Switch.Level = SourceLevels.Critical;
        }

        public delegate void ForwardKeyPressesChangedEventHandler(object sender, ForwardPanelEventArgs e);

        public event ForwardKeyPressesChangedEventHandler OnForwardKeyPressesChanged;

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

                if (!File.Exists(_debugLogFile))
                {
                    var stream = File.Create(_debugLogFile);
                    stream.Close();
                }

                if (!File.Exists(_errorLogFile))
                {
                    var stream = File.Create(_errorLogFile);
                    stream.Close();
                }

                Common.SetErrorLog(_errorLogFile);

                DCSFPProfile.ParseSettings(DBCommon.GetDCSBIOSJSONDirectory(Settings.Default.DCSBiosJSONLocation));

                CheckErrorLogAndDCSBIOSLocation();
                /*******************************************************************************************/
                /*DO NOT CHANGE INIT SEQUENCE BETWEEN HIDHANDLER DCSBIOS AND PROFILEHANDLER !!!!!  2.5.2018*/
                /*Changing these will cause difficult to trace problems with DCS-BIOS data being corrupted */
                /*******************************************************************************************/
                _hidHandler = new HIDHandler();
                if (_doSearchForPanels)
                {
                    _hidHandler.Startup(Settings.Default.LoadStreamDeck);
                }

                CreateDCSBIOS();
                StartTimers();

                /*******************************************************************************************/
                /*DO NOT CHANGE INIT SEQUENCE BETWEEN HIDHANDLER DCSBIOS AND PROFILEHANDLER !!!!!  2.5.2018*/
                /*Changing these will cause difficult to trace problems with DCS-BIOS data being corrupted */
                /*******************************************************************************************/
                _profileHandler = new ProfileHandler(Settings.Default.DCSBiosJSONLocation, Settings.Default.LastProfileFileUsed);
                _profileHandler.Init();
                SearchForPanels();
                _profileHandler.Attach(this);
                _profileHandler.AttachUserMessageHandler(this);
                if (!_profileHandler.LoadProfile(Settings.Default.LastProfileFileUsed, this))
                {
                    CreateNewProfile();
                }

                _dcsfpProfile = _profileHandler.Profile;

                SetWindowTitle();
                SetWindowState();

                if (!Common.PartialDCSBIOSEnabled())
                {
                    ShutdownDCSBIOS();
                }
                else
                {
                    StartupDCSBIOS();
                }

                SendEventRegardingForwardingOfKeys();

                CheckForNewDCSFPRelease();

                if (Settings.Default.LoadStreamDeck == false && Process.GetProcessesByName("StreamDeck").Length >= 1)
                {
                    MessageBox.Show("StreamDeck's official software is running in the background.", "Warning", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }

                ConfigurePlugins();

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

        public void BipPanelRegisterEvent(object sender, BipPanelRegisteredEventArgs e)
        {
        }

        public void UserMessage(object sender, UserMessageEventArgs e)
        {
            try
            {
                Dispatcher?.BeginInvoke((Action)(() => MessageBox.Show(e.UserMessage, "Information")));
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void CheckErrorLogAndDCSBIOSLocation()
        {
            // FUGLY, I know but something quick to help the users
            try
            {
                var loggerText = File.ReadAllText(_errorLogFile);
                if (loggerText.Contains(DCSBIOSControlLocator.DCSBIOSNotFoundErrorMessage))
                {
                    var window = new DCSBIOSNotFoundWindow(Settings.Default.DCSBiosJSONLocation);
                    window.ShowDialog();
                    MessageBox.Show(
                        "This warning will be shown as long as there are error messages in error log stating that DCS-BIOS can not be found. Delete or clear the error log once you have fixed the problem.",
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

        private void CreateNewProfile()
        {
            var chooseProfileModuleWindow = new ChooseProfileModuleWindow();
            if (chooseProfileModuleWindow.ShowDialog() == true)
            {
                _profileHandler.NewProfile();
                _profileHandler.Profile = chooseProfileModuleWindow.Profile;
                SendEventRegardingForwardingOfKeys();
            }

            SetWindowState();
        }

        private void SetApplicationMode(DCSFPProfile dcsfpProfile)
        {
            if (!IsLoaded)
            {
                return;
            }

            LabelAirframe.Content = DCSFPProfile.IsNoFrameLoadedYet(dcsfpProfile) ? string.Empty : dcsfpProfile.Description;

            /*
             * Special case as loaded type of radio panel depends on profile settings, all other panels are the same regardless of profile.
             */

            CloseTemporaryRadioPanels();

            SearchForRadioPanels();

            _profileHandler.SendRadioSettings();

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

            SortTabs();
        }

        private void CloseTemporaryRadioPanels()
        {
            try
            {
                var counter = 0;
                while (TabControlPanels.Items.Count > counter)
                {
                    var tabItem = (TabItem)TabControlPanels.Items[counter];
                    var userControl = (UserControlBase)tabItem.Content;

                    if (tabItem.Header.Equals(Constants.TemporaryRadioTabHeader))
                    {
                        TabControlPanels.Items.Remove(tabItem);
                        userControl.Dispose();
                        counter = 0;
                        continue;
                    }

                    counter++;
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void CloseStreamDecks()
        {
            foreach (var hidSkeleton in _hidHandler.HIDSkeletons)
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
                        Detach(gamingPanel);
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
                            Detach(gamingPanel);

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
                var str = "DCS-BIOS UPDATES MISSED = " + e.GamingPanelEnum + "  " + e.Count;
                ShowStatusBarMessage(str);
                Common.LogError(str);
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        /*
         * DCS-BIOS may be created AFTER the panels are created so then
         * they need to be added as listeners to DCS-BIOS.
         */
        public void Attach(GamingPanel gamingPanel)
        {
            _gamingPanels.Add(gamingPanel);

            OnForwardKeyPressesChanged += gamingPanel.SetForwardKeyPresses;
            _profileHandler.Attach(gamingPanel);
            gamingPanel.Attach(_profileHandler);
            gamingPanel.Attach((IProfileHandlerListener)this);
            _dcsBios?.AttachDataReceivedListener(gamingPanel);
        }

        public void Detach(GamingPanel gamingPanel)
        {
            _gamingPanels.Remove(gamingPanel);

            OnForwardKeyPressesChanged -= gamingPanel.SetForwardKeyPresses;
            _profileHandler.Detach(gamingPanel);
            gamingPanel.Detach(_profileHandler);
            gamingPanel.Detach((IProfileHandlerListener)this);
            _dcsBios?.DetachDataReceivedListener(gamingPanel);

            Dispatcher?.BeginInvoke((Action)(() => CloseTabItem(gamingPanel.HIDInstanceId)));
        }

        private void CreateDCSBIOS()
        {
            if (_dcsBios != null)
            {
                return;
            }

            _dcsBios = new DCSBIOS(this, Settings.Default.DCSBiosIPFrom, Settings.Default.DCSBiosIPTo, int.Parse(Settings.Default.DCSBiosPortFrom), int.Parse(Settings.Default.DCSBiosPortTo), DcsBiosNotificationMode.AddressValue);
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

            _dcsBios?.DetachConnectionListener(this);
            _dcsBios?.AttachConnectionListener(this);
            AttachGamingPanelsToDCSBIOS();
            _dcsStopGearTimer.Start();
            _dcsCheckDcsBiosStatusTimer.Start();
        }

        private void ShutdownDCSBIOS()
        {
            DetachGamingPanelsFromDCSBIOS();
            _dcsBios?.Shutdown();
            _dcsBios = null;

            _dcsStopGearTimer.Stop();
            _dcsCheckDcsBiosStatusTimer.Stop();
            ImageDcsBiosConnected.Visibility = Visibility.Collapsed;
        }

        private void AttachGamingPanelsToDCSBIOS()
        {
            foreach (var gamingPanel in _gamingPanels)
            {
                _dcsBios?.AttachDataReceivedListener(gamingPanel);
            }
        }

        private void DetachGamingPanelsFromDCSBIOS()
        {
            foreach (var gamingPanel in _gamingPanels)
            {
                _dcsBios?.DetachDataReceivedListener(gamingPanel);
            }
        }

        public DCSFPProfile GetProfile()
        {
            return _dcsfpProfile;
        }

        private void SearchForPanels()
        {
            try
            {
                if (_doSearchForPanels)
                {
                    foreach (var hidSkeleton in _hidHandler.HIDSkeletons)
                    {
                        switch (hidSkeleton.PanelInfo.GamingPanelType)
                        {
                            case GamingPanelEnum.Unknown:
                                {
                                    continue;
                                }

                            case GamingPanelEnum.PZ55SwitchPanel:
                                {
                                    var tabItem = new TabItem { Header = "PZ55" };
                                    var switchPanelPZ55UserControl = new SwitchPanelPZ55UserControl(hidSkeleton, tabItem, this);
                                    _panelUserControls.Add(switchPanelPZ55UserControl);
                                    _profileHandler.Attach(switchPanelPZ55UserControl);
                                    tabItem.Content = switchPanelPZ55UserControl;
                                    TabControlPanels.Items.Add(tabItem);
                                    _profileFileInstanceIDs.Add(new KeyValuePair<string, GamingPanelEnum>(hidSkeleton.HIDReadDevice.DevicePath, hidSkeleton.PanelInfo.GamingPanelType));
                                    break;
                                }

                            case GamingPanelEnum.PZ70MultiPanel:
                                {
                                    var tabItem = new TabItem { Header = "PZ70" };
                                    var multiPanelUserControl = new MultiPanelUserControl(hidSkeleton, tabItem, this);
                                    _panelUserControls.Add(multiPanelUserControl);
                                    _profileHandler.Attach(multiPanelUserControl);
                                    tabItem.Content = multiPanelUserControl;
                                    TabControlPanels.Items.Add(tabItem);
                                    _profileFileInstanceIDs.Add(new KeyValuePair<string, GamingPanelEnum>(hidSkeleton.HIDReadDevice.DevicePath, hidSkeleton.PanelInfo.GamingPanelType));
                                    break;
                                }

                            case GamingPanelEnum.BackLitPanel:
                                {
                                    var tabItem = new TabItem { Header = "B.I.P." };
                                    var backLitPanelUserControl = new BackLitPanelUserControl(tabItem, this, hidSkeleton);
                                    _panelUserControls.Add(backLitPanelUserControl);
                                    _profileHandler.Attach(backLitPanelUserControl);
                                    tabItem.Content = backLitPanelUserControl;
                                    TabControlPanels.Items.Add(tabItem);
                                    _profileFileInstanceIDs.Add(new KeyValuePair<string, GamingPanelEnum>(hidSkeleton.HIDReadDevice.DevicePath, hidSkeleton.PanelInfo.GamingPanelType));
                                    break;
                                }

                            case GamingPanelEnum.TPM:
                                {
                                    var tabItem = new TabItem { Header = "TPM" };
                                    var tpmPanelUserControl = new TPMPanelUserControl(hidSkeleton, tabItem, this);
                                    _panelUserControls.Add(tpmPanelUserControl);
                                    _profileHandler.Attach(tpmPanelUserControl);
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
                                        var tabItemStreamDeck = new TabItem { Header = hidSkeleton.PanelInfo.GamingPanelType.GetDescription() };
                                        var streamDeckUserControl = new StreamDeckUserControl(hidSkeleton.PanelInfo.GamingPanelType, hidSkeleton, tabItemStreamDeck, this);
                                        _panelUserControls.Add(streamDeckUserControl);
                                        _profileHandler.Attach(streamDeckUserControl);
                                        tabItemStreamDeck.Content = streamDeckUserControl;
                                        TabControlPanels.Items.Add(tabItemStreamDeck);
                                        _profileFileInstanceIDs.Add(new KeyValuePair<string, GamingPanelEnum>(hidSkeleton.InstanceId, hidSkeleton.PanelInfo.GamingPanelType));
                                    }

                                    break;
                                }

                            case GamingPanelEnum.PZ69RadioPanel:
                                {
                                    var tabItem = new TabItem { Header = Constants.TemporaryRadioTabHeader };
                                    var radioPanelPZ69UserControl = new RadioPanelPZ69UserControlNotImplemented(hidSkeleton, tabItem, this);
                                    tabItem.Content = radioPanelPZ69UserControl;
                                    TabControlPanels.Items.Add(tabItem);
                                    break;
                                }

                            case GamingPanelEnum.FarmingPanel:
                                {
                                    var tabItem = new TabItem { Header = "Side Panel" };
                                    var farmingSidePanelUserControl = new FarmingPanelUserControl(hidSkeleton, tabItem, this);
                                    _panelUserControls.Add(farmingSidePanelUserControl);
                                    _profileHandler.Attach(farmingSidePanelUserControl);
                                    tabItem.Content = farmingSidePanelUserControl;
                                    TabControlPanels.Items.Add(tabItem);
                                    _profileFileInstanceIDs.Add(new KeyValuePair<string, GamingPanelEnum>(hidSkeleton.HIDReadDevice.DevicePath, hidSkeleton.PanelInfo.GamingPanelType));
                                    break;
                                }
                        }
                    } // for each
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

        private void SearchForRadioPanels()
        {
            try
            {
                if (_doSearchForPanels)
                {
                    if (DCSFPProfile.IsNoFrameLoadedYet(_profileHandler.Profile))
                    {
                        return;
                    }

                    foreach (var hidSkeleton in _hidHandler.HIDSkeletons)
                    {
                        switch (hidSkeleton.PanelInfo.GamingPanelType)
                        {
                            case GamingPanelEnum.Unknown:
                                {
                                    continue;
                                }

                            case GamingPanelEnum.PZ69RadioPanel:
                                {
                                    var tabItem = new TabItem { Header = "PZ69" };
                                    if (DCSFPProfile.IsKeyEmulator(_profileHandler.Profile))
                                    {
                                        var radioPanelPZ69UserControl = new RadioPanelPZ69UserControlEmulator(hidSkeleton, tabItem, this);
                                        _panelUserControls.Add(radioPanelPZ69UserControl);
                                        _profileHandler.Attach(radioPanelPZ69UserControl);
                                        tabItem.Content = radioPanelPZ69UserControl;
                                        TabControlPanels.Items.Add(tabItem);
                                        _profileFileInstanceIDs.Add(new KeyValuePair<string, GamingPanelEnum>(hidSkeleton.HIDReadDevice.DevicePath, hidSkeleton.PanelInfo.GamingPanelType));
                                    }
                                    else if (Common.IsEmulationModesFlagSet(EmulationMode.SRSEnabled) || DCSFPProfile.IsFlamingCliff(_profileHandler.Profile))
                                    {
                                        var radioPanelPZ69UserControl = new RadioPanelPZ69UserControlSRS(hidSkeleton, tabItem, this);
                                        _panelUserControls.Add(radioPanelPZ69UserControl);
                                        _profileHandler.Attach(radioPanelPZ69UserControl);
                                        tabItem.Content = radioPanelPZ69UserControl;
                                        TabControlPanels.Items.Add(tabItem);
                                        _profileFileInstanceIDs.Add(new KeyValuePair<string, GamingPanelEnum>(hidSkeleton.HIDReadDevice.DevicePath, hidSkeleton.PanelInfo.GamingPanelType));
                                    }
                                    else if (DCSFPProfile.IsA10C(_profileHandler.Profile) && !_profileHandler.Profile.UseGenericRadio)
                                    {
                                        var radioPanelPZ69UserControl = new RadioPanelPZ69UserControlA10C(hidSkeleton, tabItem, this);
                                        _panelUserControls.Add(radioPanelPZ69UserControl);
                                        _profileHandler.Attach(radioPanelPZ69UserControl);
                                        tabItem.Content = radioPanelPZ69UserControl;
                                        TabControlPanels.Items.Add(tabItem);
                                        _profileFileInstanceIDs.Add(new KeyValuePair<string, GamingPanelEnum>(hidSkeleton.HIDReadDevice.DevicePath, hidSkeleton.PanelInfo.GamingPanelType));
                                    }
                                    else if (DCSFPProfile.IsUH1H(_profileHandler.Profile) && !_profileHandler.Profile.UseGenericRadio)
                                    {
                                        var radioPanelPZ69UserControl = new RadioPanelPZ69UserControlUH1H(hidSkeleton, tabItem, this);
                                        _panelUserControls.Add(radioPanelPZ69UserControl);
                                        _profileHandler.Attach(radioPanelPZ69UserControl);
                                        tabItem.Content = radioPanelPZ69UserControl;
                                        TabControlPanels.Items.Add(tabItem);
                                        _profileFileInstanceIDs.Add(new KeyValuePair<string, GamingPanelEnum>(hidSkeleton.HIDReadDevice.DevicePath, hidSkeleton.PanelInfo.GamingPanelType));
                                    }
                                    else if (DCSFPProfile.IsMiG21Bis(_profileHandler.Profile) && !_profileHandler.Profile.UseGenericRadio)
                                    {
                                        var radioPanelPZ69UserControl = new RadioPanelPZ69UserControlMiG21Bis(hidSkeleton, tabItem, this);
                                        _panelUserControls.Add(radioPanelPZ69UserControl);
                                        _profileHandler.Attach(radioPanelPZ69UserControl);
                                        tabItem.Content = radioPanelPZ69UserControl;
                                        TabControlPanels.Items.Add(tabItem);
                                        _profileFileInstanceIDs.Add(new KeyValuePair<string, GamingPanelEnum>(hidSkeleton.HIDReadDevice.DevicePath, hidSkeleton.PanelInfo.GamingPanelType));
                                    }
                                    else if (DCSFPProfile.IsKa50(_profileHandler.Profile) && !_profileHandler.Profile.UseGenericRadio)
                                    {
                                        var radioPanelPZ69UserControl = new RadioPanelPZ69UserControlKa50(hidSkeleton, tabItem, this);
                                        _panelUserControls.Add(radioPanelPZ69UserControl);
                                        _profileHandler.Attach(radioPanelPZ69UserControl);
                                        tabItem.Content = radioPanelPZ69UserControl;
                                        TabControlPanels.Items.Add(tabItem);
                                        _profileFileInstanceIDs.Add(new KeyValuePair<string, GamingPanelEnum>(hidSkeleton.HIDReadDevice.DevicePath, hidSkeleton.PanelInfo.GamingPanelType));
                                    }
                                    else if (DCSFPProfile.IsMi8MT(_profileHandler.Profile) && !_profileHandler.Profile.UseGenericRadio)
                                    {
                                        var radioPanelPZ69UserControl = new RadioPanelPZ69UserControlMi8(hidSkeleton, tabItem, this);
                                        _panelUserControls.Add(radioPanelPZ69UserControl);
                                        _profileHandler.Attach(radioPanelPZ69UserControl);
                                        tabItem.Content = radioPanelPZ69UserControl;
                                        TabControlPanels.Items.Add(tabItem);
                                        _profileFileInstanceIDs.Add(new KeyValuePair<string, GamingPanelEnum>(hidSkeleton.HIDReadDevice.DevicePath, hidSkeleton.PanelInfo.GamingPanelType));
                                    }
                                    else if (DCSFPProfile.IsBf109K4(_profileHandler.Profile) && !_profileHandler.Profile.UseGenericRadio)
                                    {
                                        var radioPanelPZ69UserControl = new RadioPanelPZ69UserControlBf109(hidSkeleton, tabItem, this);
                                        _panelUserControls.Add(radioPanelPZ69UserControl);
                                        _profileHandler.Attach(radioPanelPZ69UserControl);
                                        tabItem.Content = radioPanelPZ69UserControl;
                                        TabControlPanels.Items.Add(tabItem);
                                        _profileFileInstanceIDs.Add(new KeyValuePair<string, GamingPanelEnum>(hidSkeleton.HIDReadDevice.DevicePath, hidSkeleton.PanelInfo.GamingPanelType));
                                    }
                                    else if (DCSFPProfile.IsFW190D9(_profileHandler.Profile) && !_profileHandler.Profile.UseGenericRadio)
                                    {
                                        var radioPanelPZ69UserControl = new RadioPanelPZ69UserControlFw190(hidSkeleton, tabItem, this);
                                        _panelUserControls.Add(radioPanelPZ69UserControl);
                                        _profileHandler.Attach(radioPanelPZ69UserControl);
                                        tabItem.Content = radioPanelPZ69UserControl;
                                        TabControlPanels.Items.Add(tabItem);
                                        _profileFileInstanceIDs.Add(new KeyValuePair<string, GamingPanelEnum>(hidSkeleton.HIDReadDevice.DevicePath, hidSkeleton.PanelInfo.GamingPanelType));
                                    }
                                    else if (DCSFPProfile.IsP51D(_profileHandler.Profile) && !_profileHandler.Profile.UseGenericRadio)
                                    {
                                        var radioPanelPZ69UserControl = new RadioPanelPZ69UserControlP51D(hidSkeleton, tabItem, this);
                                        _panelUserControls.Add(radioPanelPZ69UserControl);
                                        _profileHandler.Attach(radioPanelPZ69UserControl);
                                        tabItem.Content = radioPanelPZ69UserControl;
                                        TabControlPanels.Items.Add(tabItem);
                                        _profileFileInstanceIDs.Add(new KeyValuePair<string, GamingPanelEnum>(hidSkeleton.HIDReadDevice.DevicePath, hidSkeleton.PanelInfo.GamingPanelType));
                                    }
                                    else if (DCSFPProfile.IsF86F(_profileHandler.Profile) && !_profileHandler.Profile.UseGenericRadio)
                                    {
                                        var radioPanelPZ69UserControl = new RadioPanelPZ69UserControlF86F(hidSkeleton, tabItem, this);
                                        _panelUserControls.Add(radioPanelPZ69UserControl);
                                        _profileHandler.Attach(radioPanelPZ69UserControl);
                                        tabItem.Content = radioPanelPZ69UserControl;
                                        TabControlPanels.Items.Add(tabItem);
                                        _profileFileInstanceIDs.Add(new KeyValuePair<string, GamingPanelEnum>(hidSkeleton.HIDReadDevice.DevicePath, hidSkeleton.PanelInfo.GamingPanelType));
                                    }
                                    else if (DCSFPProfile.IsSpitfireLFMkIX(_profileHandler.Profile) && !_profileHandler.Profile.UseGenericRadio)
                                    {
                                        var radioPanelPZ69UserControl = new RadioPanelPZ69UserControlSpitfireLFMkIX(hidSkeleton, tabItem, this);
                                        _panelUserControls.Add(radioPanelPZ69UserControl);
                                        _profileHandler.Attach(radioPanelPZ69UserControl);
                                        tabItem.Content = radioPanelPZ69UserControl;
                                        TabControlPanels.Items.Add(tabItem);
                                        _profileFileInstanceIDs.Add(new KeyValuePair<string, GamingPanelEnum>(hidSkeleton.HIDReadDevice.DevicePath, hidSkeleton.PanelInfo.GamingPanelType));
                                    }
                                    else if (DCSFPProfile.IsAJS37(_profileHandler.Profile) && !_profileHandler.Profile.UseGenericRadio)
                                    {
                                        var radioPanelPZ69UserControl = new RadioPanelPZ69UserControlAJS37(hidSkeleton, tabItem, this);
                                        _panelUserControls.Add(radioPanelPZ69UserControl);
                                        _profileHandler.Attach(radioPanelPZ69UserControl);
                                        tabItem.Content = radioPanelPZ69UserControl;
                                        TabControlPanels.Items.Add(tabItem);
                                        _profileFileInstanceIDs.Add(new KeyValuePair<string, GamingPanelEnum>(hidSkeleton.HIDReadDevice.DevicePath, hidSkeleton.PanelInfo.GamingPanelType));
                                    }
                                    else if (DCSFPProfile.IsSA342(_profileHandler.Profile) && !_profileHandler.Profile.UseGenericRadio)
                                    {
                                        var radioPanelPZ69UserControl = new RadioPanelPZ69UserControlSA342(hidSkeleton, tabItem, this);
                                        _panelUserControls.Add(radioPanelPZ69UserControl);
                                        _profileHandler.Attach(radioPanelPZ69UserControl);
                                        tabItem.Content = radioPanelPZ69UserControl;
                                        TabControlPanels.Items.Add(tabItem);
                                        _profileFileInstanceIDs.Add(new KeyValuePair<string, GamingPanelEnum>(hidSkeleton.HIDReadDevice.DevicePath, hidSkeleton.PanelInfo.GamingPanelType));
                                    }
                                    else if (DCSFPProfile.IsFA18C(_profileHandler.Profile) && !_profileHandler.Profile.UseGenericRadio)
                                    {
                                        var radioPanelPZ69UserControl = new RadioPanelPZ69UserControlFA18C(hidSkeleton, tabItem, this);
                                        _panelUserControls.Add(radioPanelPZ69UserControl);
                                        _profileHandler.Attach(radioPanelPZ69UserControl);
                                        tabItem.Content = radioPanelPZ69UserControl;
                                        TabControlPanels.Items.Add(tabItem);
                                        _profileFileInstanceIDs.Add(new KeyValuePair<string, GamingPanelEnum>(hidSkeleton.HIDReadDevice.DevicePath, hidSkeleton.PanelInfo.GamingPanelType));
                                    }
                                    else if (DCSFPProfile.IsM2000C(_profileHandler.Profile) && !_profileHandler.Profile.UseGenericRadio)
                                    {
                                        var radioPanelPZ69UserControl = new RadioPanelPZ69UserControlM2000C(hidSkeleton, tabItem, this);
                                        _panelUserControls.Add(radioPanelPZ69UserControl);
                                        _profileHandler.Attach(radioPanelPZ69UserControl);
                                        tabItem.Content = radioPanelPZ69UserControl;
                                        TabControlPanels.Items.Add(tabItem);
                                        _profileFileInstanceIDs.Add(new KeyValuePair<string, GamingPanelEnum>(hidSkeleton.HIDReadDevice.DevicePath, hidSkeleton.PanelInfo.GamingPanelType));
                                    }
                                    else if (DCSFPProfile.IsF5E(_profileHandler.Profile) && !_profileHandler.Profile.UseGenericRadio)
                                    {
                                        var radioPanelPZ69UserControl = new RadioPanelPZ69UserControlF5E(hidSkeleton, tabItem, this);
                                        _panelUserControls.Add(radioPanelPZ69UserControl);
                                        _profileHandler.Attach(radioPanelPZ69UserControl);
                                        tabItem.Content = radioPanelPZ69UserControl;
                                        TabControlPanels.Items.Add(tabItem);
                                        _profileFileInstanceIDs.Add(new KeyValuePair<string, GamingPanelEnum>(hidSkeleton.HIDReadDevice.DevicePath, hidSkeleton.PanelInfo.GamingPanelType));
                                    }
                                    else if (DCSFPProfile.IsF14B(_profileHandler.Profile) && !_profileHandler.Profile.UseGenericRadio)
                                    {
                                        var radioPanelPZ69UserControl = new RadioPanelPZ69UserControlF14B(hidSkeleton, tabItem, this);
                                        _panelUserControls.Add(radioPanelPZ69UserControl);
                                        _profileHandler.Attach(radioPanelPZ69UserControl);
                                        tabItem.Content = radioPanelPZ69UserControl;
                                        TabControlPanels.Items.Add(tabItem);
                                        _profileFileInstanceIDs.Add(new KeyValuePair<string, GamingPanelEnum>(hidSkeleton.HIDReadDevice.DevicePath, hidSkeleton.PanelInfo.GamingPanelType));
                                    }
                                    else if (DCSFPProfile.IsAV8B(_profileHandler.Profile) && !_profileHandler.Profile.UseGenericRadio)
                                    {
                                        var radioPanelPZ69UserControl = new RadioPanelPZ69UserControlAV8BNA(hidSkeleton, tabItem, this);
                                        _panelUserControls.Add(radioPanelPZ69UserControl);
                                        _profileHandler.Attach(radioPanelPZ69UserControl);
                                        tabItem.Content = radioPanelPZ69UserControl;
                                        TabControlPanels.Items.Add(tabItem);
                                        _profileFileInstanceIDs.Add(new KeyValuePair<string, GamingPanelEnum>(hidSkeleton.HIDReadDevice.DevicePath, hidSkeleton.PanelInfo.GamingPanelType));
                                    }
                                    else if (DCSFPProfile.IsP47D(_profileHandler.Profile) && !_profileHandler.Profile.UseGenericRadio)
                                    {
                                        var radioPanelPZ69UserControl = new RadioPanelPZ69UserControlP47D(hidSkeleton, tabItem, this);
                                        _panelUserControls.Add(radioPanelPZ69UserControl);
                                        _profileHandler.Attach(radioPanelPZ69UserControl);
                                        tabItem.Content = radioPanelPZ69UserControl;
                                        TabControlPanels.Items.Add(tabItem);
                                        _profileFileInstanceIDs.Add(new KeyValuePair<string, GamingPanelEnum>(hidSkeleton.HIDReadDevice.DevicePath, hidSkeleton.PanelInfo.GamingPanelType));
                                    }
                                    else
                                    {
                                        var radioPanelPZ69UserControl = new RadioPanelPZ69UserControlGeneric(hidSkeleton, tabItem, this);
                                        _panelUserControls.Add(radioPanelPZ69UserControl);
                                        _profileHandler.Attach(radioPanelPZ69UserControl);
                                        tabItem.Content = radioPanelPZ69UserControl;
                                        TabControlPanels.Items.Add(tabItem);
                                        _profileFileInstanceIDs.Add(new KeyValuePair<string, GamingPanelEnum>(hidSkeleton.HIDReadDevice.DevicePath, hidSkeleton.PanelInfo.GamingPanelType));
                                    }

                                    break;
                                }
                        }
                    } // for each
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

        public void UISwitchesChanged(object sender, SwitchesChangedEventArgs e)
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

        public void SettingsCleared(object sender, PanelEventArgs e)
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

        public void LedLightChanged(object sender, LedLightChangeEventArgs e)
        {
        }

        public void PanelSettingsChanged(object sender, PanelEventArgs e)
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

        public void SelectedProfile(object sender, AirframeEventArgs e)
        {
            try
            {
                if (_dcsfpProfile != e.Profile)
                {
                    _dcsfpProfile = e.Profile;
                    SetApplicationMode(_dcsfpProfile);
                    SendEventRegardingForwardingOfKeys();
                }

                if (Common.KeyEmulationOnly())
                {
                    SetNS430Status(false, false);
                }
                else
                {
                    SetNS430Status(false);
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        public void PanelBindingReadFromFile(object sender, PanelBindingReadFromFileEventArgs e)
        {
            try
            {
                if (_profileHandler.Profile != _dcsfpProfile)
                {
                    _dcsfpProfile = _profileHandler.Profile;
                    SetApplicationMode(_dcsfpProfile);
                }

                MenuItemUseNS430.IsChecked = _profileHandler.UseNS430;
                SetWindowState();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        public void SettingsApplied(object sender, PanelEventArgs e)
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

        public void PanelDataAvailable(object sender, PanelDataToDCSBIOSEventEventArgs e)
        {
        }

        public void DeviceAttached(object sender, PanelEventArgs e)
        {
        }

        public void DeviceDetached(object sender, PanelEventArgs e)
        {
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
                Common.LogError("Error checking for newer releases. " + ex.Message + "\n" + ex.StackTrace);
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
                Title = _windowName; // + "      " + Common.GetLocalIPAddress();
            }
            else
            {
                Title = _windowName + _profileHandler.Filename; // + "      " + Common.GetLocalIPAddress();
            }

            if (_profileHandler.IsDirty)
            {
                Title = Title + " *";
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
                NewProfile();
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
                MenuItemUseNS430.IsEnabled = menuIsEnabled;
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
                OpenAnOtherProfile();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private bool CloseProfile()
        {
            if (_profileHandler.IsDirty && MessageBox.Show("Discard unsaved changes to current profile?", "Discard changes?", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
            {
                return false;
            }

            try
            {
                CloseTabItems();
                _profileHandler = new ProfileHandler(Settings.Default.DCSBiosJSONLocation);
                _profileHandler.Init();
                _profileHandler.Attach(this);
                _profileHandler.AttachUserMessageHandler(this);
                _dcsfpProfile = _profileHandler.Profile;
                SetApplicationMode(_dcsfpProfile);
                SetWindowTitle();
                SetWindowState();
                SendEventRegardingForwardingOfKeys();
            }
            catch (Exception e)
            {
                Common.LogError(e);
                Common.LogError(e.InnerException);
                throw;
            }

            return true;
        }

        private void MenuItemCloseProfile_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                try
                {
                    Mouse.OverrideCursor = Cursors.Wait;
                    CloseProfile();
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
                NewProfile();
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
                OpenAnOtherProfile();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

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
                this.ButtonImageDisable.Tag = forwardKeys ? "True" : "False";

                SendEventRegardingForwardingOfKeys();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void SendEventRegardingForwardingOfKeys()
        {
            // Disabling can be used when user want to reset panel switches and does not want that resetting switches affects the game.
            OnForwardKeyPressesChanged?.Invoke(this, new ForwardPanelEventArgs() { Forward = !_disablePanelEventsFromBeingRouted });
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
                    _statusMessagesTimer.Interval = this._statusMessages.Count > 0 ? 8000 : 1000;

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

        private void MenuItemErrorLog_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!File.Exists(_errorLogFile))
                {
                    File.Create(_errorLogFile);
                }

                Process.Start(_errorLogFile);
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void MenuItemDebugLog_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!File.Exists(_debugLogFile))
                {
                    File.Create(_debugLogFile);
                }

                Process.Start(_debugLogFile);
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
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
                var formulaSanboxWindow = new JaceSandboxWindow(_dcsBios);
                formulaSanboxWindow.ShowDialog();
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
                    _dcsBios.ReceiveFromIp = Settings.Default.DCSBiosIPFrom;
                    _dcsBios.ReceivePort = int.Parse(Settings.Default.DCSBiosPortFrom);
                    _dcsBios.SendToIp = Settings.Default.DCSBiosIPTo;
                    _dcsBios.SendPort = int.Parse(Settings.Default.DCSBiosPortTo);
                    _dcsBios.Shutdown();
                    _dcsBios.Startup();
                    _profileHandler.DCSBIOSJSONDirectory = Settings.Default.DCSBiosJSONLocation;
                }

                if (settingsWindow.SRSChanged)
                {
                    SRSListenerFactory.SetParams(Settings.Default.SRSPortFrom, Settings.Default.SRSIpTo, Settings.Default.SRSPortTo);
                    SRSListenerFactory.ReStart();
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

        private string GetProductId(string saitedVID, string usbDeviceKeyName)
        {
            var pos = usbDeviceKeyName.IndexOf("&", StringComparison.InvariantCulture);
            var vid = usbDeviceKeyName.Substring(0, pos);
            var pid = usbDeviceKeyName.Substring(pos + 1);
            var result = pid;
            if (vid == saitedVID)
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

                SendEventRegardingForwardingOfKeys();
                SetWindowState();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void ButtonDisablePanelEventsFromBeingRouted_OnMouseEnter(object sender, MouseEventArgs e)
        {
            this.Cursor = Cursors.Hand;
        }

        private void ButtonDisablePanelEventsFromBeingRouted_OnMouseLeave(object sender, MouseEventArgs e)
        {
            this.Cursor = Cursors.Arrow;
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
                    CreateNewProfile();
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
    }
}