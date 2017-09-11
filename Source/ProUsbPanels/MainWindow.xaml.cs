using System;
using System.Collections.Generic;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using DCS_BIOS;
using ProUsbPanels.Properties;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using NonVisuals;
using Timer = System.Timers.Timer;

namespace ProUsbPanels
{

    /*
     * REPORT_SIZE : size of a report in bits
     * REPORT_COUNT : of fields (of that size)
     */
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : ISaitekPanelListener, IDcsBiosDataListener, IGlobalHandler, IProfileHandlerListener, IUserMessageHandler
    {
        public delegate void ForwardKeyPressesChangedEventHandler(bool forwardKeyPress);
        public event ForwardKeyPressesChangedEventHandler OnForwardKeyPressesChanged;
        
        private bool _doSearchForPanels = true;
        private HIDHandler _hidHandler;
        private FIPHandler _fipHandler;
        private ProfileHandler _panelProfileHandler;
        private string _windowName = "Flightpanels ";
        private Timer _exceptionTimer = new Timer(1000);
        private Timer _statusMessagesTimer = new Timer(1000);
        private Timer _dcsStopGearTimer = new Timer(5000);
        private Timer _dcsCheckDcsBiosStatusTimer = new Timer(5000);
        private Timer _checkForDcsGameWindowTimer = new Timer(5000);
        private DCSBIOS _dcsBios;
        private List<string> _statusMessages = new List<string>();
        private object _lockObjectStatusMessages = new object();
        private List<UserControl> _saitekUserControls = new List<UserControl>();
        private DCSAirframe _dcsAirframe;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                LoadSettings();
                _dcsBios = new DCSBIOS(this, Settings.Default.DCSBiosIPFrom, Settings.Default.DCSBiosIPTo, int.Parse(Settings.Default.DCSBiosPortFrom), int.Parse(Settings.Default.DCSBiosPortTo), DcsBiosNotificationMode.AddressValue);
                _dcsBios.Startup();
                _hidHandler = new HIDHandler();
                if (_doSearchForPanels)
                {
                    _hidHandler.Startup();
                }
                _exceptionTimer.Elapsed += TimerCheckExceptions;
                _exceptionTimer.Start();
                _dcsStopGearTimer.Elapsed += TimerStopRotation;
                _dcsCheckDcsBiosStatusTimer.Elapsed += TimerCheckDcsBiosStatus;
                _statusMessagesTimer.Elapsed += TimerStatusMessagesTimer;
                _checkForDcsGameWindowTimer.Elapsed += TimerCheckForDCSGameWindow;
                _statusMessagesTimer.Start();
                _dcsCheckDcsBiosStatusTimer.Start();
                _checkForDcsGameWindowTimer.Start();

                if (!_dcsBios.HasLastException())
                {
                    RotateGear(2000);
                }
                _panelProfileHandler = new ProfileHandler(Settings.Default.DCSBiosJSONLocation, Settings.Default.LastProfileFileUsed);
                _panelProfileHandler.Attach(this);
                _panelProfileHandler.AttachUserMessageHandler(this);
                if (!_panelProfileHandler.LoadProfile(Settings.Default.LastProfileFileUsed))
                {
                    CreateNewProfile();
                }
                _dcsAirframe = _panelProfileHandler.Airframe;
                //SearchForPanels();
                SetWindowTitle();
                SetWindowState();
                SendEventRegardingForwardingOfKeys();

                var assembly = Assembly.GetExecutingAssembly();
                var fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
                LabelVersionInformation.Text = "v. " + fileVersionInfo.FileVersion;

                //For me so that debugging is not on for anyone else.
                if (!Environment.MachineName.Equals("TIMOFEI"))
                {
                    Common.DebugOn = false;
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(1017, ex);
            }
        }

        public void UserMessage(string userMessage)
        {
            try
            {
                Dispatcher.BeginInvoke((Action)(() => MessageBox.Show(userMessage, "Information")));
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
            //Do not remove, must be because of while()
            if (_fipHandler != null)
            {
                _fipHandler.Close();
            }

            var closedItemCount = CloseTabItems();

            if (dcsAirframe == DCSAirframe.NONE)
            {
                Common.DebugP("Shutting down DCSBIOS");
                _dcsBios.Shutdown();
                _dcsStopGearTimer.Stop();
                _dcsCheckDcsBiosStatusTimer.Stop();
                _checkForDcsGameWindowTimer.Stop();
                ImageDcsBiosConnected.Visibility = Visibility.Collapsed;
                MenuItemCheckForDCS.Visibility = Visibility.Collapsed;
                MenuItemDCSBIOSSettings.Visibility = Visibility.Collapsed;
                SearchForPanels();
            }
            else if(dcsAirframe != DCSAirframe.NOFRAMELOADEDYET)
            {
                Common.DebugP("Starting up DCSBIOS");
                _dcsBios.Startup();
                _dcsStopGearTimer.Start();
                _dcsCheckDcsBiosStatusTimer.Start();
                _checkForDcsGameWindowTimer.Start();
                ImageDcsBiosConnected.Visibility = Visibility.Visible;
                MenuItemCheckForDCS.Visibility = Visibility.Visible;
                MenuItemDCSBIOSSettings.Visibility = Visibility.Visible;
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

                        _panelProfileHandler.Detach(saitekPanel);
                        saitekPanel.Detach(_panelProfileHandler);
                        saitekPanel.Detach((IProfileHandlerListener)this);
                        _dcsBios.DetachDataReceivedListener(saitekPanel);

                        Common.DebugP("Shutting down " + saitekPanel.GetType().Name);
                        saitekPanel.Shutdown();
                        _saitekUserControls.Remove((UserControl)item.Content);
                        Common.DebugP("_saitekUserControls count is " + _saitekUserControls.Count);
                        Common.DebugP("TabControlPanels.Items.Count is " + TabControlPanels.Items.Count);
                        closedItemCount++;
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

        public void UpdatesHasBeenMissed(string uniqueId, SaitekPanelsEnum saitekPanelsEnum, int count)
        {
            try
            {
                var str = "DCS-BIOS UPDATES MISSED = " + saitekPanelsEnum + "  " + count;
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
            OnForwardKeyPressesChanged += new ForwardKeyPressesChangedEventHandler(saitekPanel.SetForwardKeyPresses);
            _panelProfileHandler.Attach(saitekPanel);
            saitekPanel.Attach(_panelProfileHandler);
            saitekPanel.Attach((IProfileHandlerListener)this);
            _dcsBios.AttachDataReceivedListener(saitekPanel);
        }

        public void Detach(SaitekPanel saitekPanel)
        {
            OnForwardKeyPressesChanged -= new ForwardKeyPressesChangedEventHandler(saitekPanel.SetForwardKeyPresses);
            _panelProfileHandler.Detach(saitekPanel);
            saitekPanel.Detach(_panelProfileHandler);
            saitekPanel.Detach((IProfileHandlerListener)this);
            _dcsBios.DetachDataReceivedListener(saitekPanel);
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
                            case SaitekPanelsEnum.Unknown:
                                {
                                    continue;
                                }
                            case SaitekPanelsEnum.PZ55SwitchPanel:
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
                            case SaitekPanelsEnum.PZ69RadioPanel:
                                {
                                    if (_panelProfileHandler.Airframe == DCSAirframe.NONE)
                                    {
                                        break;
                                    }
                                    var tabItem = new TabItem();
                                    tabItem.Header = "PZ69";
                                    if (_panelProfileHandler.Airframe == DCSAirframe.A10C)
                                    {
                                        var radioPanelPZ69UserControl = new RadioPanelPZ69UserControlA10C(hidSkeleton, tabItem, this);
                                        _saitekUserControls.Add(radioPanelPZ69UserControl);
                                        _panelProfileHandler.Attach(radioPanelPZ69UserControl);
                                        tabItem.Content = radioPanelPZ69UserControl;
                                        TabControlPanels.Items.Add(tabItem);
                                    }
                                    else if (_panelProfileHandler.Airframe == DCSAirframe.UH1H)
                                    {
                                        var radioPanelPZ69UserControl = new RadioPanelPZ69UserControlUH1H(hidSkeleton, tabItem, this);
                                        _saitekUserControls.Add(radioPanelPZ69UserControl);
                                        _panelProfileHandler.Attach(radioPanelPZ69UserControl);
                                        tabItem.Content = radioPanelPZ69UserControl;
                                        TabControlPanels.Items.Add(tabItem);
                                    }
                                    else if (_panelProfileHandler.Airframe == DCSAirframe.Mig21Bis)
                                    {
                                        var radioPanelPZ69UserControl = new RadioPanelPZ69UserControlMiG21Bis(hidSkeleton, tabItem, this);
                                        _saitekUserControls.Add(radioPanelPZ69UserControl);
                                        _panelProfileHandler.Attach(radioPanelPZ69UserControl);
                                        tabItem.Content = radioPanelPZ69UserControl;
                                        TabControlPanels.Items.Add(tabItem);
                                    }
                                    else if (_panelProfileHandler.Airframe == DCSAirframe.Ka50)
                                    {
                                        var radioPanelPZ69UserControl = new RadioPanelPZ69UserControlKa50(hidSkeleton, tabItem, this);
                                        _saitekUserControls.Add(radioPanelPZ69UserControl);
                                        _panelProfileHandler.Attach(radioPanelPZ69UserControl);
                                        tabItem.Content = radioPanelPZ69UserControl;
                                        TabControlPanels.Items.Add(tabItem);
                                    }
                                    else if (_panelProfileHandler.Airframe == DCSAirframe.Mi8)
                                    {
                                        var radioPanelPZ69UserControl = new RadioPanelPZ69UserControlMi8(hidSkeleton, tabItem, this);
                                        _saitekUserControls.Add(radioPanelPZ69UserControl);
                                        _panelProfileHandler.Attach(radioPanelPZ69UserControl);
                                        tabItem.Content = radioPanelPZ69UserControl;
                                        TabControlPanels.Items.Add(tabItem);
                                    }
                                    break;
                                }
                            case SaitekPanelsEnum.PZ70MultiPanel:
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
                            case SaitekPanelsEnum.BackLitPanel:
                                {
                                    if (_panelProfileHandler.Airframe == DCSAirframe.NONE)
                                    {
                                        break;
                                    }
                                    var tabItem = new TabItem();
                                    tabItem.Header = "B.I.P.";
                                    var backLitPanelUserControl = new BackLitPanelUserControl(tabItem, this, hidSkeleton);
                                    _saitekUserControls.Add(backLitPanelUserControl);
                                    _panelProfileHandler.Attach(backLitPanelUserControl);
                                    tabItem.Content = backLitPanelUserControl;
                                    TabControlPanels.Items.Add(tabItem);
                                    break;
                                }
                            case SaitekPanelsEnum.TPM:
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
                _fipHandler = new FIPHandler();
                if (_fipHandler.Initialize())
                {
                    if (_fipHandler.FIPPanels.Count > 0)
                    {
                        //Only one FIP tab regardless of one or many FIPs because they are all configured the same.
                        var tabItem = new TabItem();
                        tabItem.Header = "FIP";
                        var fipPanelUserControl = new FIPPanelUserControl(_fipHandler, tabItem, this);
                        _saitekUserControls.Add(fipPanelUserControl);
                        _panelProfileHandler.Attach(fipPanelUserControl);
                        tabItem.Content = fipPanelUserControl;
                        TabControlPanels.Items.Add(tabItem);
                    }
                    SortTabs();
                    if (TabControlPanels.Items.Count > 0)
                    {
                        TabControlPanels.SelectedIndex = 0;
                    }
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
            MenuItemCheckForDCS.IsChecked = Settings.Default.CheckForDCSBeforeSendingCommands;
            var toolTip = new ToolTip { Content = "Checks that DCS is started before sending key commands." };
            MenuItemCheckForDCS.ToolTip = toolTip;

            Common.DebugOn = Settings.Default.DebugOn;
            Common.DebugToFile = Settings.Default.DebugToFile;
            MenuItemDoDebugging.IsChecked = Common.DebugOn;
            MenuItemDebugToFile.IsChecked = Common.DebugToFile;


            if (Settings.Default.APIMode == 0)
            {
                MenuItemAPIKeyBdEvent.IsChecked = true;
                Common.APIMode = APIModeEnum.keybd_event;
            }
            else
            {
                MenuItemAPISendInput.IsChecked = true;
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

        public void SwitchesChanged(string uniqueId, SaitekPanelsEnum saitekPanelsEnum, HashSet<object> hashSet)
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


        public void SettingsCleared(string uniqueId, SaitekPanelsEnum saitekPanelsEnum)
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

        public void LedLightChanged(string uniqueId, SaitekPanelLEDPosition saitekPanelLEDPosition, PanelLEDColor panelLEDColor)
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

        public void PanelSettingsChanged(string uniqueId, SaitekPanelsEnum saitekPanelsEnum)
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


        public void SelectedAirframe(DCSAirframe dcsAirframe)
        {
            try
            {
                if (_dcsAirframe != dcsAirframe)
                {
                    _dcsAirframe = dcsAirframe;
                    SetApplicationMode(_dcsAirframe);
                    SendEventRegardingForwardingOfKeys();
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(471673, ex);
            }
        }

        public void PanelSettingsReadFromFile(List<string> settings)
        {
            try
            {
                if (_panelProfileHandler.Airframe != _dcsAirframe)
                {
                    _dcsAirframe = _panelProfileHandler.Airframe;
                    SetApplicationMode(_dcsAirframe);
                }
                SetWindowState();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(2038, ex);
            }
        }

        public void SettingsApplied(string uniqueId, SaitekPanelsEnum saitekPanelsEnum)
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

        public void PanelDataAvailable(string stringData)
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

        public void DeviceAttached(string uniqueId, SaitekPanelsEnum saitekPanelsEnum)
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

        public void DeviceDetached(string uniqueId, SaitekPanelsEnum saitekPanelsEnum)
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
                /*if (!_dcsBios.Running())
                {
                    ShowStatusBarMessage("Restarting DcsBios object.");
                    _dcsBios.Shutdown();
                    _dcsBios.Startup();
                }*/
            }
            catch (Exception)
            {
            }
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
                Title = _windowName;// + "      " + Common.GetLocalIPAddress();
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
                //Environment.Exit(0);
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
                _checkForDcsGameWindowTimer.Stop();
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
                    ((ISaitekUserControl)saitekUserControl).GetSaitekPanel().Shutdown();
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
                _dcsBios.Shutdown();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(2018, ex);
            }
            Common.DebugP("Mainwindow Shutdown() _dcsBios shutdown");
            try
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
            Common.DebugP("Mainwindow Shutdown() _fipHandler shutdown");
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
                _panelProfileHandler.Airframe = chooseProfileModuleWindow.DCSAirframe;
                SetWindowState();
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

        private void MenuItemCheckForDCSClick(object sender, RoutedEventArgs e)
        {
            try
            {
                MenuItemCheckForDCS.IsChecked = !MenuItemCheckForDCS.IsChecked;
                Settings.Default.CheckForDCSBeforeSendingCommands = MenuItemCheckForDCS.IsChecked;
                Settings.Default.Save();
                SendEventRegardingForwardingOfKeys();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(2023, ex);
            }
        }

        private void MenuItemAPIKeyBdEventClick(object sender, RoutedEventArgs e)
        {
            try
            {
                MenuItemAPIKeyBdEvent.IsChecked = !MenuItemAPIKeyBdEvent.IsChecked;
                MenuItemAPISendInput.IsChecked = !MenuItemAPIKeyBdEvent.IsChecked;
                if (MenuItemAPIKeyBdEvent.IsChecked)
                {
                    Settings.Default.APIMode = 0;
                    Common.APIMode = APIModeEnum.keybd_event;
                    Settings.Default.Save();
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(2024, ex);
            }
        }

        private void MenuItemAPISendInputClick(object sender, RoutedEventArgs e)
        {
            try
            {
                MenuItemAPISendInput.IsChecked = !MenuItemAPISendInput.IsChecked;
                MenuItemAPIKeyBdEvent.IsChecked = !MenuItemAPISendInput.IsChecked;
                if (MenuItemAPISendInput.IsChecked)
                {
                    Settings.Default.APIMode = 1;
                    Common.APIMode = APIModeEnum.SendInput;
                    Settings.Default.Save();
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(2025, ex);
            }
        }

        private void MenuItemDCSBIOSSettingsClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var dcsBiosWindow = new DcsBiosWindow(Settings.Default.DCSBiosIPFrom, Settings.Default.DCSBiosPortFrom, Settings.Default.DCSBiosIPTo, Settings.Default.DCSBiosPortTo, DBCommon.GetDCSBIOSJSONDirectory(Settings.Default.DCSBiosJSONLocation));

                if (dcsBiosWindow.ShowDialog() == true)
                {
                    Settings.Default.DCSBiosIPFrom = dcsBiosWindow.IPAddressFrom;
                    Settings.Default.DCSBiosPortFrom = dcsBiosWindow.PortFrom;
                    Settings.Default.DCSBiosIPTo = dcsBiosWindow.IPAddressTo;
                    Settings.Default.DCSBiosPortTo = dcsBiosWindow.PortTo;
                    Settings.Default.DCSBiosJSONLocation = dcsBiosWindow.DCSBiosJSONLocation;
                    Settings.Default.Save();

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
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(2026, ex);
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
            //Whether to send keypresses or not depends if user has pressed the disable button or has checked the "Check for DCS" menuItem (and DCS is found or not)
            var forwardKeys = !bool.Parse(ButtonImageDisable.Tag.ToString());
            var checkForDCS = Settings.Default.CheckForDCSBeforeSendingCommands;

            if (_panelProfileHandler.Airframe == DCSAirframe.NONE)
            {
                //DCS exists or not does not matter, keyboard emulation only
                checkForDCS = false;
            }
            //If forward keys and need to check for DCS then DCS has the final word.
            //If not forward keys then thats that
            if (checkForDCS && forwardKeys)
            {
                forwardKeys = DigitalCombatSimulatorWindowFound();
            }
            if (OnForwardKeyPressesChanged != null)
            {
                OnForwardKeyPressesChanged(forwardKeys);
            }
        }

        private static bool DigitalCombatSimulatorWindowFound()
        {
            var hwnd = WindowsAPI.FindWindow("DCS");
            if (hwnd == IntPtr.Zero)
            {
                hwnd = WindowsAPI.FindWindow("Digital Combat Simulator");
            }
            if (hwnd != IntPtr.Zero)
            {
                return true;
            }
            return false;
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

        public void DcsBiosDataReceived(uint address, uint data)
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

        public void DeviceAttached(SaitekPanelsEnum saitekPanelsEnum)
        {
            //todo
        }

        public void DeviceDetached(SaitekPanelsEnum saitekPanelsEnum)
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

        private void TimerCheckForDCSGameWindow(object sender, ElapsedEventArgs e)
        {
            try
            {
                Dispatcher.BeginInvoke((Action)(SendEventRegardingForwardingOfKeys));
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
                var file = Path.GetTempPath() + "\\Flightpanels_error_log.txt";
                if (!File.Exists(file))
                {
                    File.Create(file);
                }
                Process.Start(file);
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
                var file = Path.GetTempPath() + "\\Flightpanels_debug_log.txt";
                if (!File.Exists(file))
                {
                    File.Create(file);
                }
                Process.Start(file);
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(206411, ex);
            }
        }

        private void MenuItemProcessPriorityClick(object sender, RoutedEventArgs e)
        {

            try
            {
                var menuItem = (MenuItem)sender;
                if (menuItem.Name.Contains("BelowNormal"))
                {
                    //0
                    Settings.Default.ProcessPriority = ProcessPriorityClass.BelowNormal;
                    Settings.Default.Save();
                    Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.BelowNormal;
                    MenuItemProcessPriorityBelowNormal.IsChecked = true;
                    MenuItemProcessPriorityNormal.IsChecked = false;
                    MenuItemProcessPriorityAboveNormal.IsChecked = false;
                    MenuItemProcessPriorityHigh.IsChecked = false;
                    MenuItemProcessPriorityRealtime.IsChecked = false;
                }
                else if (menuItem.Name.Contains("ItemNormal"))
                {
                    //1
                    Settings.Default.ProcessPriority = ProcessPriorityClass.Normal;
                    Settings.Default.Save();
                    Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.Normal;
                    MenuItemProcessPriorityBelowNormal.IsChecked = false;
                    MenuItemProcessPriorityNormal.IsChecked = true;
                    MenuItemProcessPriorityAboveNormal.IsChecked = false;
                    MenuItemProcessPriorityHigh.IsChecked = false;
                    MenuItemProcessPriorityRealtime.IsChecked = false;
                }
                else if (menuItem.Name.Contains("AboveNormal"))
                {
                    //2
                    Settings.Default.ProcessPriority = ProcessPriorityClass.AboveNormal;
                    Settings.Default.Save();
                    Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.AboveNormal;
                    MenuItemProcessPriorityBelowNormal.IsChecked = false;
                    MenuItemProcessPriorityNormal.IsChecked = false;
                    MenuItemProcessPriorityAboveNormal.IsChecked = true;
                    MenuItemProcessPriorityHigh.IsChecked = false;
                    MenuItemProcessPriorityRealtime.IsChecked = false;
                }
                else if (menuItem.Name.Contains("High"))
                {
                    //3
                    Settings.Default.ProcessPriority = ProcessPriorityClass.High;
                    Settings.Default.Save();
                    Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.High;
                    MenuItemProcessPriorityBelowNormal.IsChecked = false;
                    MenuItemProcessPriorityNormal.IsChecked = false;
                    MenuItemProcessPriorityAboveNormal.IsChecked = false;
                    MenuItemProcessPriorityHigh.IsChecked = true;
                    MenuItemProcessPriorityRealtime.IsChecked = false;

                }
                else if (menuItem.Name.Contains("Realtime"))
                {
                    //4
                    Settings.Default.ProcessPriority = ProcessPriorityClass.RealTime;
                    Settings.Default.Save();
                    Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.RealTime;
                    MenuItemProcessPriorityBelowNormal.IsChecked = false;
                    MenuItemProcessPriorityNormal.IsChecked = false;
                    MenuItemProcessPriorityAboveNormal.IsChecked = false;
                    MenuItemProcessPriorityHigh.IsChecked = false;
                    MenuItemProcessPriorityRealtime.IsChecked = true;
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(2065, ex);
            }
        }

        private void LoadProcessPriority()
        {
            try
            {
                Process.GetCurrentProcess().PriorityClass = Settings.Default.ProcessPriority;
                switch (Settings.Default.ProcessPriority)
                {
                    case ProcessPriorityClass.BelowNormal:
                        {
                            MenuItemProcessPriorityBelowNormal.IsChecked = true;
                            break;
                        }
                    case ProcessPriorityClass.Normal:
                        {
                            MenuItemProcessPriorityNormal.IsChecked = true;
                            break;
                        }
                    case ProcessPriorityClass.AboveNormal:
                        {
                            MenuItemProcessPriorityAboveNormal.IsChecked = true;
                            break;
                        }
                    case ProcessPriorityClass.High:
                        {
                            MenuItemProcessPriorityHigh.IsChecked = true;
                            break;
                        }
                    case ProcessPriorityClass.RealTime:
                        {
                            MenuItemProcessPriorityRealtime.IsChecked = true;
                            break;
                        }
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(2066, ex);
            }
        }

        private void MenuItemDoDebugging_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                MenuItemDoDebugging.IsChecked = !MenuItemDoDebugging.IsChecked;
                Common.DebugOn = MenuItemDoDebugging.IsChecked;
                Settings.Default.DebugOn = Common.DebugOn;
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(28877066, ex);
            }
        }

        private void MenuItemDebugToFile_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                MenuItemDebugToFile.IsChecked = !MenuItemDebugToFile.IsChecked;
                Common.DebugToFile = MenuItemDebugToFile.IsChecked;
                Settings.Default.DebugToFile = Common.DebugToFile;
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(28877067, ex);
            }
        }

        private void ImageDcsBiosConnected_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            _panelProfileHandler.OpenProfileDEVELOPMENT();
        }

    }
}
