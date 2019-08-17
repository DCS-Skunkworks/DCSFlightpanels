using System;
using System.Collections.Generic;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using DCS_BIOS;
using DCSFlightpanels.Properties;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Navigation;
using ClassLibraryCommon;
using DCSFlightpanels.Radios;
using Microsoft.Win32;
using NonVisuals;
using NonVisuals.Radios;
using Octokit;
using Application = System.Windows.Application;
using Cursors = System.Windows.Input.Cursors;
using MessageBox = System.Windows.MessageBox;
using Timer = System.Timers.Timer;
using UserControl = System.Windows.Controls.UserControl;

/*
 Custom Resharper Naming abbreviations
 ADF AJS ALL ALT APR BIOS BIP BIPS COM CRS DB DCS DCSBIOS DCSBIOSJSON DME DRO HDG HF IAS ICS IFF ILS IP IX JSON KEYS LCD LCDPZ LE LED NADIR NAV OS PZ REV SA SRS TACAN TPM UH UHF USB VHF VID VS XPDR XY ZY ARC ARN APX ABRIS OK ID FA ZA AV8BNA COMM NS DCSFP
*/
namespace DCSFlightpanels
{

    /*
     * REPORT_SIZE : size of a report in bits
     * REPORT_COUNT : of fields (of that size)
     */
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : IGamingPanelListener, IDcsBiosDataListener, IGlobalHandler, IProfileHandlerListener, IUserMessageHandler, IDisposable
    {
        public delegate void ForwardKeyPressesChangedEventHandler(object sender, ForwardPanelEventArgs e);

        public event ForwardKeyPressesChangedEventHandler OnForwardKeyPressesChanged;

        private readonly bool _doSearchForPanels = true;
        private HIDHandler _hidHandler;
        private ProfileHandler _panelProfileHandler;
        private readonly string _windowName = "DCSFlightpanels ";
        private readonly Timer _exceptionTimer = new Timer(1000);
        private readonly Timer _statusMessagesTimer = new Timer(1000);
        private readonly Timer _dcsStopGearTimer = new Timer(5000);
        private readonly Timer _dcsCheckDcsBiosStatusTimer = new Timer(5000);
        private DCSBIOS _dcsBios;
        private readonly List<string> _statusMessages = new List<string>();
        private readonly object _lockObjectStatusMessages = new object();
        private readonly List<UserControl> _saitekUserControls = new List<UserControl>();
        private DCSAirframe _dcsAirframe;
        private readonly string _debugLogFile = AppDomain.CurrentDomain.BaseDirectory + "\\DCSFlightpanels_debug_log.txt";
        private readonly string _errorLogFile = AppDomain.CurrentDomain.BaseDirectory + "\\DCSFlightpanels_error_log.txt";
        private bool _disablePanelEventsFromBeingRouted;

        public MainWindow()
        {
            InitializeComponent();

            // Stop annoying "Cannot find source for binding with reference .... " from being shown
            PresentationTraceSources.DataBindingSource.Switch.Level = SourceLevels.Critical;
        }

        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Settings.Default.RunMinimized)
                {
                    this.WindowState = WindowState.Minimized;
                }

                LoadSettings();

                if (!File.Exists(_debugLogFile))
                {
                    File.Create(_debugLogFile);
                }

                if (!File.Exists(_errorLogFile))
                {
                    File.Create(_errorLogFile);
                }

                Common.SetErrorLog(_errorLogFile);
                Common.SetDebugLog(_debugLogFile);

                CheckErrorLogAndDCSBIOSLocation();
                /*******************************************************************************************/
                /*DO NOT CHANGE INIT SEQUENCE BETWEEN HIDHANDLER DCSBIOS AND PROFILEHANDLER !!!!!  2.5.2018*/
                /*Changing these will cause difficult to trace problems with DCS-BIOS data being corrupted */
                /*******************************************************************************************/
                _hidHandler = new HIDHandler();
                if (_doSearchForPanels)
                {
                    _hidHandler.Startup();
                }

                _dcsBios = new DCSBIOS(this, Settings.Default.DCSBiosIPFrom, Settings.Default.DCSBiosIPTo, int.Parse(Settings.Default.DCSBiosPortFrom), int.Parse(Settings.Default.DCSBiosPortTo), DcsBiosNotificationMode.AddressValue);
                if (!_dcsBios.HasLastException())
                {
                    RotateGear(2000);
                }

                _exceptionTimer.Elapsed += TimerCheckExceptions;
                _exceptionTimer.Start();
                _dcsStopGearTimer.Elapsed += TimerStopRotation;
                _dcsCheckDcsBiosStatusTimer.Elapsed += TimerCheckDcsBiosStatus;
                _statusMessagesTimer.Elapsed += TimerStatusMessagesTimer;
                _statusMessagesTimer.Start();
                _dcsCheckDcsBiosStatusTimer.Start();

                /*******************************************************************************************/
                /*DO NOT CHANGE INIT SEQUENCE BETWEEN HIDHANDLER DCSBIOS AND PROFILEHANDLER !!!!!  2.5.2018*/
                /*Changing these will cause difficult to trace problems with DCS-BIOS data being corrupted */
                /*******************************************************************************************/
                _panelProfileHandler = new ProfileHandler(Settings.Default.DCSBiosJSONLocation, Settings.Default.LastProfileFileUsed);
                _panelProfileHandler.Attach(this);
                _panelProfileHandler.AttachUserMessageHandler(this);
                if (!_panelProfileHandler.LoadProfile(Settings.Default.LastProfileFileUsed))
                {
                    CreateNewProfile();
                }

                _dcsAirframe = _panelProfileHandler.Airframe;

                if (!Common.PartialDCSBIOSEnabled())
                {
                    _dcsBios?.Shutdown();
                    _dcsBios = null;
                }
                else
                {
                    _dcsBios?.Startup();
                }

                SetWindowTitle();
                SetWindowState();
                SendEventRegardingForwardingOfKeys();

                CheckForNewDCSFPRelease();

                if (Settings.Default.ShowBugReportInfo)
                {
                    MessageBox.Show("Remember to use the built in\nfunctionality to report bugs!\n[Options] -> [Report Bug]", "Bug related", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    Settings.Default.ShowBugReportInfo = false;
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(1017, ex);
            }
        }

        public void BipPanelRegisterEvent(object sender, BipPanelRegisteredEventArgs e)
        {
        }

        public void UserMessage(object sender, UserMessageEventArgs e)
        {
            try
            {
                Dispatcher.BeginInvoke((Action)(() => MessageBox.Show(e.UserMessage, "Information")));
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(19909017, ex);
            }
        }

        private void CheckErrorLogAndDCSBIOSLocation()
        {
            //FUGLY, I know but something quick to help the users
            try
            {
                var loggerText = File.ReadAllText(_errorLogFile);
                if (loggerText.Contains(DCSBIOSControlLocator.DCSBIOSNotFoundErrorMessage))
                {
                    var window = new DCSBIOSNotFoundWindow(Settings.Default.DCSBiosJSONLocation);
                    window.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(19909017, ex);
            }
        }

        private void CreateNewProfile()
        {
            var chooseProfileModuleWindow = new ChooseProfileModuleWindow();
            if (chooseProfileModuleWindow.ShowDialog() == true)
            {
                _panelProfileHandler.NewProfile();
                Common.UseGenericRadio = chooseProfileModuleWindow.UseGenericRadio;
                _panelProfileHandler.Airframe = chooseProfileModuleWindow.DCSAirframe;
                SendEventRegardingForwardingOfKeys();
            }

            SetWindowState();
        }

        private void SetApplicationMode(DCSAirframe dcsAirframe)
        {
            if (!IsLoaded)
            {
                return;
            }

            Common.DebugP("SetApplicationMode() Airframe has changed. Current airframe is " + dcsAirframe);

            if (dcsAirframe == DCSAirframe.NOFRAMELOADEDYET)
            {
                LabelAirframe.Content = "";
            }
            else
            {
                LabelAirframe.Content = dcsAirframe;
            }

            var itemCount = TabControlPanels.Items.Count;
            Common.DebugP("There are " + TabControlPanels.Items.Count + " TabControlPanels.Items");

            var closedItemCount = CloseTabItems();

            if (Common.IsOperationModeFlagSet(OperationFlag.KeyboardEmulationOnly))
            {
                Common.DebugP("Shutting down DCSBIOS");
                _dcsBios?.Shutdown();
                _dcsStopGearTimer.Stop();
                _dcsCheckDcsBiosStatusTimer.Stop();
                ImageDcsBiosConnected.Visibility = Visibility.Collapsed;
                SearchForPanels();
            }
            else if (dcsAirframe != DCSAirframe.NOFRAMELOADEDYET)
            {
                Common.DebugP("Starting up DCSBIOS");
                if (_dcsBios == null)
                {
                    _dcsBios = new DCSBIOS(this, Settings.Default.DCSBiosIPFrom, Settings.Default.DCSBiosIPTo, int.Parse(Settings.Default.DCSBiosPortFrom), int.Parse(Settings.Default.DCSBiosPortTo), DcsBiosNotificationMode.AddressValue);
                }

                _dcsBios.Startup();
                _dcsStopGearTimer.Start();
                _dcsCheckDcsBiosStatusTimer.Start();
                ImageDcsBiosConnected.Visibility = Visibility.Visible;
                SearchForPanels();
            }

            if (closedItemCount != itemCount)
            {
                //Something isn't right
                Common.LogError(8911, "SetApplicationMode(). Error closing tab items. Items to close was " + itemCount + ", items actually closed was " + closedItemCount);
            }
            else if (itemCount > 0)
            {
                Common.DebugP("Closed " + itemCount + " out of " + closedItemCount + " tab items");
            }
        }

        public int CloseTabItems()
        {
            var closedItemCount = 0;
            try
            {
                Common.DebugP("Entering CloseTabItems()");
                Common.DebugP("_saitekUserControls count is " + _saitekUserControls.Count);
                Common.DebugP("TabControlPanels.Items.Count is " + TabControlPanels.Items.Count);
                if (TabControlPanels.Items.Count > 0)
                {
                    do
                    {
                        var item = (TabItem)TabControlPanels.Items.GetItemAt(0);
                        TabControlPanels.Items.Remove(item);
                        var saitekPanelUserControl = ((ISaitekUserControl)item.Content);
                        var saitekPanel = saitekPanelUserControl.GetSaitekPanel();
                        if (saitekPanel != null)
                        {
                            _panelProfileHandler.Detach(saitekPanel);
                            saitekPanel.Detach(_panelProfileHandler);
                            saitekPanel.Detach((IProfileHandlerListener) this);
                            _dcsBios?.DetachDataReceivedListener(saitekPanel);

                            Common.DebugP("Shutting down " + saitekPanel.GetType().Name);
                            saitekPanel.Shutdown();
                            _saitekUserControls.Remove((UserControl) item.Content);
                            Common.DebugP("_saitekUserControls count is " + _saitekUserControls.Count);
                            Common.DebugP("TabControlPanels.Items.Count is " + TabControlPanels.Items.Count);
                            closedItemCount++;
                        }
                    } while (TabControlPanels.Items.Count > 0);
                }

                Common.DebugP("Leaving CloseTabItems()");
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(471072, ex);
            }

            return closedItemCount;
        }

        public void UpdatesHasBeenMissed(object sender, DCSBIOSUpdatesMissedEventArgs e)
        {
            try
            {
                var str = "DCS-BIOS UPDATES MISSED = " + e.GamingPanelEnum + "  " + e.Count;
                ShowStatusBarMessage(str);
                Common.LogError(471072, str);
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(471072, ex);
            }
        }

        public void Attach(SaitekPanel saitekPanel)
        {
            OnForwardKeyPressesChanged += saitekPanel.SetForwardKeyPresses;
            _panelProfileHandler.Attach(saitekPanel);
            saitekPanel.Attach(_panelProfileHandler);
            saitekPanel.Attach((IProfileHandlerListener)this);
            _dcsBios?.AttachDataReceivedListener(saitekPanel);
        }

        public void Detach(SaitekPanel saitekPanel)
        {
            OnForwardKeyPressesChanged -= saitekPanel.SetForwardKeyPresses;
            _panelProfileHandler.Detach(saitekPanel);
            saitekPanel.Detach(_panelProfileHandler);
            saitekPanel.Detach((IProfileHandlerListener)this);
        }

        public DCSAirframe GetAirframe()
        {
            return _panelProfileHandler.Airframe;
        }

        private void SearchForPanels()
        {
            try
            {
                if (_doSearchForPanels)
                {
                    foreach (var hidSkeleton in _hidHandler.HIDSkeletons)
                    {

                        switch (hidSkeleton.PanelType)
                        {
                            case GamingPanelEnum.Unknown:
                                {
                                    continue;
                                }
                            case GamingPanelEnum.PZ55SwitchPanel:
                                {
                                    var tabItem = new TabItem();
                                    tabItem.Header = "PZ55";
                                    var switchPanelPZ55UserControl = new SwitchPanelPZ55UserControl(hidSkeleton, tabItem, this);
                                    _saitekUserControls.Add(switchPanelPZ55UserControl);
                                    _panelProfileHandler.Attach(switchPanelPZ55UserControl);
                                    tabItem.Content = switchPanelPZ55UserControl;
                                    TabControlPanels.Items.Add(tabItem);
                                    break;
                                }
                            case GamingPanelEnum.PZ69RadioPanel:
                                {
                                    var tabItem = new TabItem();
                                    tabItem.Header = "PZ69";
                                    if (_panelProfileHandler.Airframe == DCSAirframe.KEYEMULATOR)
                                    {
                                        var radioPanelPZ69UserControl = new RadioPanelPZ69UserControlEmulator(hidSkeleton, tabItem, this);
                                        _saitekUserControls.Add(radioPanelPZ69UserControl);
                                        _panelProfileHandler.Attach(radioPanelPZ69UserControl);
                                        tabItem.Content = radioPanelPZ69UserControl;
                                        TabControlPanels.Items.Add(tabItem);
                                    }
                                    else if (Common.IsOperationModeFlagSet(OperationFlag.SRSEnabled) || _panelProfileHandler.Airframe == DCSAirframe.FC3_CD_SRS)
                                    {
                                        var radioPanelPZ69UserControl = new RadioPanelPZ69UserControlSRS(hidSkeleton, tabItem, this);
                                        _saitekUserControls.Add(radioPanelPZ69UserControl);
                                        _panelProfileHandler.Attach(radioPanelPZ69UserControl);
                                        tabItem.Content = radioPanelPZ69UserControl;
                                        TabControlPanels.Items.Add(tabItem);
                                    }
                                    else if (_panelProfileHandler.Airframe == DCSAirframe.A10C && !Common.UseGenericRadio)
                                    {
                                        var radioPanelPZ69UserControl = new RadioPanelPZ69UserControlA10C(hidSkeleton, tabItem, this);
                                        //var radioPanelPZ69UserControl = new RadioPanelPZ69UserControlFullEmulator(hidSkeleton, tabItem, this);
                                        _saitekUserControls.Add(radioPanelPZ69UserControl);
                                        _panelProfileHandler.Attach(radioPanelPZ69UserControl);
                                        tabItem.Content = radioPanelPZ69UserControl;
                                        TabControlPanels.Items.Add(tabItem);
                                    }
                                    else if (_panelProfileHandler.Airframe == DCSAirframe.UH1H && !Common.UseGenericRadio)
                                    {
                                        var radioPanelPZ69UserControl = new RadioPanelPZ69UserControlUH1H(hidSkeleton, tabItem, this);
                                        _saitekUserControls.Add(radioPanelPZ69UserControl);
                                        _panelProfileHandler.Attach(radioPanelPZ69UserControl);
                                        tabItem.Content = radioPanelPZ69UserControl;
                                        TabControlPanels.Items.Add(tabItem);
                                    }
                                    else if (_panelProfileHandler.Airframe == DCSAirframe.Mig21Bis && !Common.UseGenericRadio)
                                    {
                                        var radioPanelPZ69UserControl = new RadioPanelPZ69UserControlMiG21Bis(hidSkeleton, tabItem, this);
                                        _saitekUserControls.Add(radioPanelPZ69UserControl);
                                        _panelProfileHandler.Attach(radioPanelPZ69UserControl);
                                        tabItem.Content = radioPanelPZ69UserControl;
                                        TabControlPanels.Items.Add(tabItem);
                                    }
                                    else if (_panelProfileHandler.Airframe == DCSAirframe.Ka50 && !Common.UseGenericRadio)
                                    {
                                        var radioPanelPZ69UserControl = new RadioPanelPZ69UserControlKa50(hidSkeleton, tabItem, this);
                                        _saitekUserControls.Add(radioPanelPZ69UserControl);
                                        _panelProfileHandler.Attach(radioPanelPZ69UserControl);
                                        tabItem.Content = radioPanelPZ69UserControl;
                                        TabControlPanels.Items.Add(tabItem);
                                    }
                                    else if (_panelProfileHandler.Airframe == DCSAirframe.Mi8 && !Common.UseGenericRadio)
                                    {
                                        var radioPanelPZ69UserControl = new RadioPanelPZ69UserControlMi8(hidSkeleton, tabItem, this);
                                        _saitekUserControls.Add(radioPanelPZ69UserControl);
                                        _panelProfileHandler.Attach(radioPanelPZ69UserControl);
                                        tabItem.Content = radioPanelPZ69UserControl;
                                        TabControlPanels.Items.Add(tabItem);
                                    }
                                    else if (_panelProfileHandler.Airframe == DCSAirframe.Bf109 && !Common.UseGenericRadio)
                                    {
                                        var radioPanelPZ69UserControl = new RadioPanelPZ69UserControlBf109(hidSkeleton, tabItem, this);
                                        _saitekUserControls.Add(radioPanelPZ69UserControl);
                                        _panelProfileHandler.Attach(radioPanelPZ69UserControl);
                                        tabItem.Content = radioPanelPZ69UserControl;
                                        TabControlPanels.Items.Add(tabItem);
                                    }
                                    else if (_panelProfileHandler.Airframe == DCSAirframe.Fw190d9 && !Common.UseGenericRadio)
                                    {
                                        var radioPanelPZ69UserControl = new RadioPanelPZ69UserControlFw190(hidSkeleton, tabItem, this);
                                        _saitekUserControls.Add(radioPanelPZ69UserControl);
                                        _panelProfileHandler.Attach(radioPanelPZ69UserControl);
                                        tabItem.Content = radioPanelPZ69UserControl;
                                        TabControlPanels.Items.Add(tabItem);
                                    }
                                    else if (_panelProfileHandler.Airframe == DCSAirframe.P51D && !Common.UseGenericRadio)
                                    {
                                        var radioPanelPZ69UserControl = new RadioPanelPZ69UserControlP51D(hidSkeleton, tabItem, this);
                                        _saitekUserControls.Add(radioPanelPZ69UserControl);
                                        _panelProfileHandler.Attach(radioPanelPZ69UserControl);
                                        tabItem.Content = radioPanelPZ69UserControl;
                                        TabControlPanels.Items.Add(tabItem);
                                    }
                                    else if (_panelProfileHandler.Airframe == DCSAirframe.F86F && !Common.UseGenericRadio)
                                    {
                                        var radioPanelPZ69UserControl = new RadioPanelPZ69UserControlF86F(hidSkeleton, tabItem, this);
                                        _saitekUserControls.Add(radioPanelPZ69UserControl);
                                        _panelProfileHandler.Attach(radioPanelPZ69UserControl);
                                        tabItem.Content = radioPanelPZ69UserControl;
                                        TabControlPanels.Items.Add(tabItem);
                                    }
                                    else if (_panelProfileHandler.Airframe == DCSAirframe.SpitfireLFMkIX && !Common.UseGenericRadio)
                                    {
                                        var radioPanelPZ69UserControl = new RadioPanelPZ69UserControlSpitfireLFMkIX(hidSkeleton, tabItem, this);
                                        _saitekUserControls.Add(radioPanelPZ69UserControl);
                                        _panelProfileHandler.Attach(radioPanelPZ69UserControl);
                                        tabItem.Content = radioPanelPZ69UserControl;
                                        TabControlPanels.Items.Add(tabItem);
                                    }
                                    else if (_panelProfileHandler.Airframe == DCSAirframe.AJS37 && !Common.UseGenericRadio)
                                    {
                                        var radioPanelPZ69UserControl = new RadioPanelPZ69UserControlAJS37(hidSkeleton, tabItem, this);
                                        _saitekUserControls.Add(radioPanelPZ69UserControl);
                                        _panelProfileHandler.Attach(radioPanelPZ69UserControl);
                                        tabItem.Content = radioPanelPZ69UserControl;
                                        TabControlPanels.Items.Add(tabItem);
                                    }
                                    else if (_panelProfileHandler.Airframe == DCSAirframe.SA342M && !Common.UseGenericRadio)
                                    {
                                        var radioPanelPZ69UserControl = new RadioPanelPZ69UserControlSA342(hidSkeleton, tabItem, this);
                                        _saitekUserControls.Add(radioPanelPZ69UserControl);
                                        _panelProfileHandler.Attach(radioPanelPZ69UserControl);
                                        tabItem.Content = radioPanelPZ69UserControl;
                                        TabControlPanels.Items.Add(tabItem);
                                    }
                                    else if (_panelProfileHandler.Airframe == DCSAirframe.FA18C && !Common.UseGenericRadio)
                                    {
                                        var radioPanelPZ69UserControl = new RadioPanelPZ69UserControlFA18C(hidSkeleton, tabItem, this);
                                        _saitekUserControls.Add(radioPanelPZ69UserControl);
                                        _panelProfileHandler.Attach(radioPanelPZ69UserControl);
                                        tabItem.Content = radioPanelPZ69UserControl;
                                        TabControlPanels.Items.Add(tabItem);
                                    }
                                    else if (_panelProfileHandler.Airframe == DCSAirframe.M2000C && !Common.UseGenericRadio)
                                    {
                                        var radioPanelPZ69UserControl = new RadioPanelPZ69UserControlM2000C(hidSkeleton, tabItem, this);
                                        _saitekUserControls.Add(radioPanelPZ69UserControl);
                                        _panelProfileHandler.Attach(radioPanelPZ69UserControl);
                                        tabItem.Content = radioPanelPZ69UserControl;
                                        TabControlPanels.Items.Add(tabItem);
                                    }
                                    else if (_panelProfileHandler.Airframe == DCSAirframe.F5E && !Common.UseGenericRadio)
                                    {
                                        var radioPanelPZ69UserControl = new RadioPanelPZ69UserControlF5E(hidSkeleton, tabItem, this);
                                        _saitekUserControls.Add(radioPanelPZ69UserControl);
                                        _panelProfileHandler.Attach(radioPanelPZ69UserControl);
                                        tabItem.Content = radioPanelPZ69UserControl;
                                        TabControlPanels.Items.Add(tabItem);
                                    }
                                    else if (_panelProfileHandler.Airframe == DCSAirframe.F14B && !Common.UseGenericRadio)
                                    {
                                        var radioPanelPZ69UserControl = new RadioPanelPZ69UserControlF14B(hidSkeleton, tabItem, this);
                                        _saitekUserControls.Add(radioPanelPZ69UserControl);
                                        _panelProfileHandler.Attach(radioPanelPZ69UserControl);
                                        tabItem.Content = radioPanelPZ69UserControl;
                                        TabControlPanels.Items.Add(tabItem);
                                    }
                                    else if (_panelProfileHandler.Airframe == DCSAirframe.AV8BNA && !Common.UseGenericRadio)
                                    {
                                        var radioPanelPZ69UserControl = new RadioPanelPZ69UserControlAV8BNA(hidSkeleton, tabItem, this);
                                        _saitekUserControls.Add(radioPanelPZ69UserControl);
                                        _panelProfileHandler.Attach(radioPanelPZ69UserControl);
                                        tabItem.Content = radioPanelPZ69UserControl;
                                        TabControlPanels.Items.Add(tabItem);
                                    }
                                    else
                                    {
                                        var radioPanelPZ69UserControl = new RadioPanelPZ69UserControlEmulatorFull(hidSkeleton, tabItem, this);
                                        _saitekUserControls.Add(radioPanelPZ69UserControl);
                                        _panelProfileHandler.Attach(radioPanelPZ69UserControl);
                                        tabItem.Content = radioPanelPZ69UserControl;
                                        TabControlPanels.Items.Add(tabItem);
                                    }

                                    break;
                                }
                            case GamingPanelEnum.PZ70MultiPanel:
                                {
                                    var tabItem = new TabItem();
                                    tabItem.Header = "PZ70";
                                    var multiPanelUserControl = new MultiPanelUserControl(hidSkeleton, tabItem, this);
                                    _saitekUserControls.Add(multiPanelUserControl);
                                    _panelProfileHandler.Attach(multiPanelUserControl);
                                    tabItem.Content = multiPanelUserControl;
                                    TabControlPanels.Items.Add(tabItem);
                                    break;
                                }
                            case GamingPanelEnum.BackLitPanel:
                                {
                                    var tabItem = new TabItem();
                                    tabItem.Header = "B.I.P.";
                                    var backLitPanelUserControl = new BackLitPanelUserControl(tabItem, this, hidSkeleton);
                                    _saitekUserControls.Add(backLitPanelUserControl);
                                    _panelProfileHandler.Attach(backLitPanelUserControl);
                                    tabItem.Content = backLitPanelUserControl;
                                    TabControlPanels.Items.Add(tabItem);
                                    break;
                                }
                            case GamingPanelEnum.TPM:
                                {
                                    var tabItem = new TabItem();
                                    tabItem.Header = "TPM";
                                    var tpmPanelUserControl = new TPMPanelUserControl(hidSkeleton, tabItem, this);
                                    _saitekUserControls.Add(tpmPanelUserControl);
                                    _panelProfileHandler.Attach(tpmPanelUserControl);
                                    tabItem.Content = tpmPanelUserControl;
                                    TabControlPanels.Items.Add(tabItem);
                                    break;
                                }
                        }
                    } //for each
                }

                
                var tabItemSD = new TabItem();
                tabItemSD.Header = "StreamDeck35";
                var streamDeckUserControl = new StreamDeck35UserControl(null, tabItemSD, this);
                _saitekUserControls.Add(streamDeckUserControl);
                //_panelProfileHandler.Attach(tpmPanelUserControl);
                tabItemSD.Content = streamDeckUserControl;
                TabControlPanels.Items.Add(tabItemSD);

                SortTabs();
                if (TabControlPanels.Items.Count > 0)
                {
                    TabControlPanels.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(1073, ex);
            }
        }

        private void SortTabs()
        {
            //Crude sorting    
            var tabOrderChanged = false;
            do
            {
                tabOrderChanged = false;
                var firstPZ55 = -1;
                var firstPZ70 = -1;
                var firstPZ69 = -1;
                var firstTPM = -1;
                var firstBIP = -1;

                for (var i = 0; i < TabControlPanels.Items.Count; i++)
                {
                    var userControl = (ISaitekUserControl)((TabItem)TabControlPanels.Items.GetItemAt(i)).Content;
                    if (userControl is SwitchPanelPZ55UserControl && firstPZ55 < 0)
                    {
                        firstPZ55 = i;
                    }
                    else if (userControl.GetName().Contains("RadioPanel") && firstPZ69 < 0)
                    {
                        firstPZ69 = i;
                    }
                    else if (userControl is MultiPanelUserControl && firstPZ70 < 0)
                    {
                        firstPZ70 = i;
                    }
                    else if (userControl is TPMPanelUserControl && firstTPM < 0)
                    {
                        firstTPM = i;
                    }
                    else if (userControl is BackLitPanelUserControl && firstBIP < 0)
                    {
                        firstBIP = i;
                    }
                }

                if (firstPZ69 < firstPZ55)
                {
                    //Puff all early PZ69 forward
                    for (var i = 0; i < TabControlPanels.Items.Count; i++)
                    {
                        var tabItem = (TabItem)TabControlPanels.Items.GetItemAt(i);
                        var userControl = (ISaitekUserControl)tabItem.Content;
                        if (userControl.GetName().Contains("RadioPanel") && i < firstPZ55)
                        {
                            TabControlPanels.Items.RemoveAt(i);
                            TabControlPanels.Items.Insert(i + 1, tabItem);
                            tabOrderChanged = true;
                        }
                    }
                }

                if (firstPZ70 < firstPZ69)
                {
                    //Puff all early PZ70 forward
                    for (var i = 0; i < TabControlPanels.Items.Count; i++)
                    {
                        var tabItem = (TabItem)TabControlPanels.Items.GetItemAt(i);
                        var userControl = (ISaitekUserControl)tabItem.Content;
                        if (userControl.GetName().Contains("MultiPanel") && i < firstPZ69)
                        {
                            TabControlPanels.Items.RemoveAt(i);
                            TabControlPanels.Items.Insert(i + 1, tabItem);
                            tabOrderChanged = true;
                        }
                    }
                }

                if (firstTPM < firstPZ70)
                {
                    //Puff all early BIP forward
                    for (var i = 0; i < TabControlPanels.Items.Count; i++)
                    {
                        var tabItem = (TabItem)TabControlPanels.Items.GetItemAt(i);
                        var userControl = (ISaitekUserControl)tabItem.Content;
                        if (userControl.GetName().Contains("BackLit") && i < firstPZ70)
                        {
                            TabControlPanels.Items.RemoveAt(i);
                            TabControlPanels.Items.Insert(i + 1, tabItem);
                            tabOrderChanged = true;
                        }
                    }
                }

                if (firstBIP < firstTPM)
                {
                    //Puff all early BIP forward
                    for (var i = 0; i < TabControlPanels.Items.Count; i++)
                    {
                        var tabItem = (TabItem)TabControlPanels.Items.GetItemAt(i);
                        var userControl = (ISaitekUserControl)tabItem.Content;
                        if (userControl.GetName().Contains("BackLit") && i < firstTPM)
                        {
                            TabControlPanels.Items.RemoveAt(i);
                            TabControlPanels.Items.Insert(i + 1, tabItem);
                            tabOrderChanged = true;
                        }
                    }
                }
            } while (tabOrderChanged);
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

            Common.DebugOn = Settings.Default.DebugOn;
            Common.DebugToFile = Settings.Default.DebugToFile;


            if (Settings.Default.APIMode == 0)
            {
                Common.APIMode = APIModeEnum.keybd_event;
            }
            else
            {
                Common.APIMode = APIModeEnum.SendInput;
            }
        }

        private void MainWindowLocationChanged(object sender, EventArgs e)
        {
            try
            {
                if (Top > 0 && Left > 0)
                {
                    Settings.Default.MainWindowTop = Top;
                    Settings.Default.MainWindowLeft = Left;
                    Settings.Default.Save();
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(1093, ex);
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
                Common.ShowErrorMessageBox(2011, ex);
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
                Common.ShowErrorMessageBox(2035, ex);
            }
        }

        public void LedLightChanged(object sender, LedLightChangeEventArgs e)
        {
            try
            {
                //todo
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(2036, ex);
            }
        }

        public void PanelSettingsChanged(object sender, PanelEventArgs e)
        {
            try
            {
                SetWindowState();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(2037, ex);
            }
        }


        public void SelectedAirframe(object sender, AirframeEventArgs e)
        {
            try
            {
                if (_dcsAirframe != e.Airframe)
                {
                    _dcsAirframe = e.Airframe;
                    SetApplicationMode(_dcsAirframe);
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
                Common.ShowErrorMessageBox(471673, ex);
            }
        }

        public void PanelSettingsReadFromFile(object sender, SettingsReadFromFileEventArgs e)
        {
            try
            {
                if (_panelProfileHandler.Airframe != _dcsAirframe)
                {
                    _dcsAirframe = _panelProfileHandler.Airframe;
                    SetApplicationMode(_dcsAirframe);
                }

                MenuItemUseNS430.IsChecked = _panelProfileHandler.UseNS430;
                SetWindowState();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(2038, ex);
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
                Common.ShowErrorMessageBox(2039, ex);
            }
        }

        public void PanelDataAvailable(object sender, PanelDataToDCSBIOSEventEventArgs e)
        {
            try
            {
                //todo
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(2040, ex);
            }
        }

        public void DeviceAttached(object sender, PanelEventArgs e)
        {
            try
            {
                //todo
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(2041, ex);
            }
        }

        public void DeviceDetached(object sender, PanelEventArgs e)
        {
            try
            {
                //todo
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(2042, ex);
            }
        }

        private void MainWindowSizeChanged(object sender, SizeChangedEventArgs e)
        {
            try
            {
                if (WindowState != WindowState.Minimized && WindowState != WindowState.Maximized)
                {
                    Settings.Default.MainWindowHeight = Height;
                    Settings.Default.MainWindowWidth = Width;
                    Settings.Default.Save();
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(2043, ex);
            }
        }

        private void TimerCheckDcsBiosStatus(object sender, ElapsedEventArgs e)
        {
            try
            {
            }
            catch (Exception)
            {
            }
        }


        private async void CheckForNewDCSFPRelease()
        {
            //#if !DEBUG
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
                    var gitHubReleaseArray = lastRelease.TagName.Replace("v.", "").Replace("v", "").Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
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
                        Dispatcher.Invoke(() =>
                        {
                            LabelVersionInformation.Visibility = Visibility.Hidden;
                            LabelDownloadNewVersion.Visibility = Visibility.Visible;
                        });
                    }
                    else
                    {
                        Dispatcher.Invoke(() =>
                        {
                            LabelVersionInformation.Text = "v." + fileVersionInfo.FileVersion;
                            LabelVersionInformation.Visibility = Visibility.Visible;
                        });
                    }
                    var lastDCSBIOSRelease = await client.Repository.Release.GetLatest("DCSFlightpanels", "dcs-bios");
                    Dispatcher.Invoke(() =>
                    {
                        LabelDCSBIOSReleaseDate.Text = "DCS-BIOS Release Date : " + lastDCSBIOSRelease.CreatedAt.Date.ToLongDateString();
                        Settings.Default.LastDCSBIOSRelease = lastDCSBIOSRelease.CreatedAt.Date.ToLongDateString();
                        Settings.Default.Save();
                    });
                }
                else
                {
                    Dispatcher.Invoke(() =>
                    {
                        LabelVersionInformation.Text = "DCSFP version : " + fileVersionInfo.FileVersion;
                        LabelVersionInformation.Visibility = Visibility.Visible;
                        LabelDCSBIOSReleaseDate.Text = "DCS-BIOS Release Date : " +  Settings.Default.LastDCSBIOSRelease;
                    });
                }
            }
            catch (Exception ex)
            {
                Common.LogError(9011, "Error checking for newer releases. " + ex.Message + "\n" + ex.StackTrace);
                LabelVersionInformation.Text = "DCSFP version : " + fileVersionInfo.FileVersion;
                LabelDCSBIOSReleaseDate.Text = "DCS-BIOS Release Date : " + Settings.Default.LastDCSBIOSRelease;
            }
            //#endif
        }

        private void TimerCheckExceptions(object sender, ElapsedEventArgs e)
        {
            try
            {
                //todo
            }
            catch (Exception ex)
            {
                Common.DebugP("HUH? " + ex.Message + Environment.NewLine + Environment.NewLine + ex.StackTrace);
            }
        }

        private void TimerStopRotation(object sender, ElapsedEventArgs e)
        {
            try
            {
                Dispatcher.BeginInvoke((Action)(() => ImageDcsBiosConnected.IsEnabled = false));
                _dcsStopGearTimer.Stop();
            }
            catch (Exception)
            {
            }
        }

        private void SetWindowTitle()
        {
            if (_panelProfileHandler.Airframe == DCSAirframe.NOFRAMELOADEDYET)
            {
                Title = "";
            }
            else if (_panelProfileHandler.IsNewProfile)
            {
                Title = _windowName; // + "      " + Common.GetLocalIPAddress();
            }
            else
            {
                Title = _windowName + _panelProfileHandler.Filename; // + "      " + Common.GetLocalIPAddress();
            }

            if (_panelProfileHandler.IsDirty)
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
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(2044, ex);
            }
        }

        private void MainWindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                if (_panelProfileHandler.IsDirty && MessageBox.Show("Discard unsaved changes to current profile?", "Discard changes?", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
                {
                    e.Cancel = true;
                }

                Settings.Default.LastProfileFileUsed = _panelProfileHandler.LastProfileUsed;
                Settings.Default.Save();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(2100, ex);
            }
        }

        private void MainWindow_OnClosed(object sender, EventArgs e)
        {
            try
            {
                Shutdown();
                //Wtf is hanging?
                Application.Current.Shutdown();
                Environment.Exit(0);
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(22100, ex);
            }
        }

        private void Shutdown()
        {
            try
            {
                Common.DebugP("Entering Mainwindow Shutdown()");
                _exceptionTimer.Stop();
                _dcsStopGearTimer.Stop();
                _statusMessagesTimer.Stop();
                _dcsCheckDcsBiosStatusTimer.Stop();
                Common.DebugP("Mainwindow Shutdown() Timers stopped");
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(2013, ex);
            }

            try
            {
                foreach (var saitekUserControl in _saitekUserControls)
                {
                    ((ISaitekUserControl)saitekUserControl).GetSaitekPanel()?.Shutdown();
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(2016, ex);
            }

            Common.DebugP("Mainwindow Shutdown() saitekUserControls shutdown");
            try
            {
                //TODO THIS CAUSES HANGING WHEN CLOSING THE APPLICATION!?!?
                //_hidHandler.Shutdown();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(32018, ex);
            }

            Common.DebugP("Mainwindow Shutdown() _hidHandler shutdown");
            try
            {
                _dcsBios?.Shutdown();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(2018, ex);
            }

            Common.DebugP("Mainwindow Shutdown() _dcsBios shutdown");
            /*try
            {
                if (_fipHandler != null)
                {
                    _fipHandler.Close();
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(2018, ex);
            }
            Common.DebugP("Mainwindow Shutdown() _fipHandler shutdown");*/
            Common.DebugP("Leaving Mainwindow Shutdown()");
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
                Common.ShowErrorMessageBox(2019, ex);
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
                Common.ShowErrorMessageBox(2020, ex);
            }
        }

        private void SaveNewOrExistingProfile()
        {
            _panelProfileHandler.SaveProfile();
            SetWindowState();
        }

        private void MenuItemSaveAsClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_panelProfileHandler.SaveAsNewProfile())
                {
                    _panelProfileHandler.IsDirty = true;
                    SetWindowState();
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(2021, ex);
            }
        }

        private void MenuItemNewClick(object sender, RoutedEventArgs e)
        {
            try
            {
                _panelProfileHandler.NewProfile();
                var chooseProfileModuleWindow = new ChooseProfileModuleWindow();
                chooseProfileModuleWindow.ShowDialog();
                Common.UseGenericRadio = chooseProfileModuleWindow.UseGenericRadio;
                _panelProfileHandler.Airframe = chooseProfileModuleWindow.DCSAirframe;
                SetWindowState();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(237022, ex);
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
                Common.ShowErrorMessageBox(237022, ex);
            }
        }

        private void MenuItemOpenClick(object sender, RoutedEventArgs e)
        {
            try
            {
                try
                {
                    Mouse.OverrideCursor = Cursors.Wait;
                    _panelProfileHandler.OpenProfile();
                }
                finally
                {
                    Mouse.OverrideCursor = null;
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(2022, ex);
            }
        }

        private void CloseProfile()
        {
            CloseTabItems();
            _panelProfileHandler = new ProfileHandler(Settings.Default.DCSBiosJSONLocation);
            _panelProfileHandler.Attach(this);
            _panelProfileHandler.AttachUserMessageHandler(this);
            _dcsAirframe = _panelProfileHandler.Airframe;
            SetApplicationMode(_dcsAirframe);
            SetWindowTitle();
            SetWindowState();
            SendEventRegardingForwardingOfKeys();
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
                Common.ShowErrorMessageBox(2021, ex);
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
                Common.ShowErrorMessageBox(2027, ex);
            }
        }

        private void ButtonImageNewMouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                CreateNewProfile();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(2028, ex);
            }
        }

        private void ButtonImageOpenMouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                _panelProfileHandler.OpenProfile();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(2029, ex);
            }
        }

        private void ButtonImageRefreshMouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                RefreshProfile();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(2030167, ex);
            }
        }

        private void RefreshProfile()
        {
            _panelProfileHandler.ReloadProfile();
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
                Common.ShowErrorMessageBox(2031, ex);
            }
        }

        private void ButtonImageDisableMouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                var forwardKeys = bool.Parse(ButtonImageDisable.Tag.ToString());
                forwardKeys = !forwardKeys;
                if (forwardKeys)
                {
                    ButtonImageDisable.Tag = "True";
                }
                else
                {
                    ButtonImageDisable.Tag = "False";
                }

                SendEventRegardingForwardingOfKeys();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(2032, ex);
            }
        }

        private void SendEventRegardingForwardingOfKeys()
        {
            //Disabling can be used when user want to reset panel switches and does not want that resetting switches affects the game.
            OnForwardKeyPressesChanged?.Invoke(this, new ForwardPanelEventArgs() { Forward = !_disablePanelEventsFromBeingRouted });
        }

        private void OpenProfileInNotepad()
        {
            Process.Start(_panelProfileHandler.Filename);
        }

        private void SetWindowState()
        {
            MenuItemSaveAs.IsEnabled = _panelProfileHandler.ProfileLoaded;
            MenuItemCloseProfile.IsEnabled = _panelProfileHandler.ProfileLoaded;
            ButtonImageSave.IsEnabled = _panelProfileHandler.IsDirty;
            MenuItemSave.IsEnabled = _panelProfileHandler.IsDirty && !_panelProfileHandler.IsNewProfile;
            ButtonImageRefresh.IsEnabled = !_panelProfileHandler.IsNewProfile && !_panelProfileHandler.IsDirty;
            ButtonImageNotepad.IsEnabled = !_panelProfileHandler.IsNewProfile && !_panelProfileHandler.IsDirty;
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
            }
        }

        public void DcsBiosDataReceived(byte[] array)
        {
            return;
        }

        public void DcsBiosDataReceived(object sender, DCSBIOSDataEventArgs e)
        {
            try
            {
                Dispatcher.BeginInvoke((Action)(() => RotateGear()));
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(2047, ex);
            }
        }

        public void DeviceAttached(GamingPanelEnum saitekPanelsEnum)
        {
            //todo
        }

        public void DeviceDetached(GamingPanelEnum saitekPanelsEnum)
        {
            //todo
        }

        private void ButtonDev_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            Common.DebugOn = false;
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
                Common.ShowErrorMessageBox(2063, ex);
            }
        }

        private void TimerStatusMessagesTimer(object sender, ElapsedEventArgs e)
        {
            try
            {
                lock (_lockObjectStatusMessages)
                {
                    if (_statusMessages.Count > 0)
                    {
                        _statusMessagesTimer.Interval = 8000;
                    }
                    else
                    {
                        _statusMessagesTimer.Interval = 1000;
                    }

                    Dispatcher.BeginInvoke((Action)(() => LabelInformation.Text = ""));

                    if (_statusMessages.Count == 0)
                    {
                        return;
                    }

                    var message = _statusMessages[0];
                    Dispatcher.BeginInvoke((Action)(() => LabelInformation.Text = message));
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
                Common.ShowErrorMessageBox(2064, ex);
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
                Common.ShowErrorMessageBox(206411, ex);
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
                Common.ShowErrorMessageBox(2066, ex);
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
                Common.ShowErrorMessageBox(288067, ex);
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
                Common.ShowErrorMessageBox(2027, ex);
            }
        }

        private void MenuItemSettings_OnClick(object sender, RoutedEventArgs e)
        {
            var settingsWindow = new SettingsWindow();
            if (settingsWindow.ShowDialog() == true)
            {
                if (settingsWindow.GeneralChanged)
                {
                    LoadProcessPriority();
                    Common.DebugOn = Settings.Default.DebugOn;
                    Common.DebugToFile = Settings.Default.DebugToFile;
                }

                if (settingsWindow.DCSBIOSChanged && Common.PartialDCSBIOSEnabled())
                {
                    //Refresh, make sure they are using the latest settings
                    DCSBIOSControlLocator.JSONDirectory = Settings.Default.DCSBiosJSONLocation;
                    _dcsBios.ReceiveFromIp = Settings.Default.DCSBiosIPFrom;
                    _dcsBios.ReceivePort = int.Parse(Settings.Default.DCSBiosPortFrom);
                    _dcsBios.SendToIp = Settings.Default.DCSBiosIPTo;
                    _dcsBios.SendPort = int.Parse(Settings.Default.DCSBiosPortTo);
                    _dcsBios.Shutdown();
                    _dcsBios.Startup();
                    _panelProfileHandler.JSONDirectory = Settings.Default.DCSBiosJSONLocation;
                }

                if (settingsWindow.SRSChanged)
                {
                    SRSListenerFactory.SetParams(Settings.Default.SRSPortFrom, Settings.Default.SRSIpTo, Settings.Default.SRSPortTo);
                    SRSListenerFactory.ReStart();
                }
            }
        }

        private void FixUSBEnhancedPowerManagerIssues()
        {
            /*
             * This is a slightly modified code version of the original code which was made by http://uraster.com
             */
            MessageBox.Show("You need to run DCSFP as Administrator for this function to work!");
            const string saitekVID = "VID_06A3";
            var result = new StringBuilder();
            result.AppendLine("USB Enhanced Power Management Disabler");
            result.AppendLine("http://uraster.com/en-us/products/usbenhancedpowermanagerdisabler.aspx");
            result.AppendLine("Copywrite Uraster GmbH");
            result.AppendLine(new string('=', 60));
            result.AppendLine("This application disables the enhanced power management for the all USB devices of a specific vendor.");
            result.AppendLine("You need admin rights to do that.");
            result.AppendLine("Plug in all devices in the ports you intend to use before continuing.");
            result.AppendLine(new string('-', 60));
            result.AppendLine("Vendor ID (VID). For SAITEK use the default of " + saitekVID);

            try
            {
                var devicesDisabled = 0;
                var devicesAlreadyDisabled = 0;
                using (var usbDevicesKey = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Enum\USB"))
                {
                    if (usbDevicesKey != null)
                    {
                        foreach (var usbDeviceKeyName in usbDevicesKey.GetSubKeyNames().Where(name => name.StartsWith(saitekVID)))
                        {
                            result.Append(Environment.NewLine);
                            result.AppendLine("Processing product : " + GetProductId(saitekVID, usbDeviceKeyName));
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
                Common.ShowErrorMessageBox(0011, ex, "Error disabling Enhanced USB Power Management.");
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
                    result += " (Multi panel, PZ70)";
                else if (pid.StartsWith("PID_0D05"))
                    result += " (Radio panel, PZ69)";
                else if (pid.StartsWith("PID_0D67"))
                    result += " (Switch panel, PZ55)";
                else if (pid.StartsWith("PID_A2AE"))
                    result += " (Instrument panel)";
                else if (pid.StartsWith("PID_712C"))
                    result += " (Yoke)";
                else if (pid.StartsWith("PID_0C2D"))
                    result += " (Throttle quadrant)";
                else if (pid.StartsWith("PID_0763"))
                    result += " (Pedals)";
                else if (pid.StartsWith("PID_0B4E"))
                    result += " (BIP)";
            }

            return result;
        }

        private void MenuItemUSBPowerManagement_OnClick(object sender, RoutedEventArgs e)
        {
            FixUSBEnhancedPowerManagerIssues();
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
                _panelProfileHandler.UseNS430 = ((MenuItem)sender).IsChecked;
                SetWindowState();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(20297, ex);
            }
        }

        private void ButtonDisableAllPanelInteractions_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var button = (Button)sender;
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
                Common.ShowErrorMessageBox(20297, ex);
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
        private bool _disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                _disposedValue = true;
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

        private void MenuItemReportBug_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var bugWindow = new BugReportWindow();
                bugWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(20297, ex);
            }
        }
    }
}


