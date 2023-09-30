using ClassLibraryCommon;
using ControlReference.Properties;
using DCS_BIOS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ControlReference.UserControls;
using ControlReference.Windows;
using DCS_BIOS.EventArgs;
using DCS_BIOS.Json;
using DCS_BIOS.Interfaces;
using ControlReference.Events;
using ControlReference.Interfaces;
using NLog;
using System.Diagnostics;
using System.IO;
using System.Media;
using System.Text;
using System.Windows.Media.Imaging;
using NLog.Targets.Wrappers;
using NLog.Targets;

namespace ControlReference
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IDisposable, IDcsBiosConnectionListener, ICategoryChange, IDCSBIOSStringListener
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private IEnumerable<DCSBIOSControl> _loadedControls = null;
        private readonly List<DCSBIOSControlUserControl> _dcsbiosUIControlPanels = new();
        private readonly Timer _dcsStopGearTimer = new(5000);
        private DCSBIOS _dcsBios;
        private bool _formLoaded = false;
        private const int MAX_CONTROLS_ON_PAGE = 70;
        private DCSBIOSOutput _dcsbiosVersionOutput;
        private bool _checkDCSBIOSVersionOnce;
        private List<DCSBIOSControl> _metaControls;

        public MainWindow()
        {
            InitializeComponent();
            REFEventHandler.AttachDataListener(this);
            BIOSEventHandler.AttachStringListener(this);

            /*
             * Correct JSON folder path, move away from $USERDIRECTORY$.
             */
            Settings.Default.DCSBiosJSONLocation = Environment.ExpandEnvironmentVariables(Settings.Default.DCSBiosJSONLocation.Contains("$USERDIRECTORY$") ?
                Settings.Default.DCSBiosJSONLocation.Replace("$USERDIRECTORY$", "%userprofile%") : Settings.Default.DCSBiosJSONLocation);
            Settings.Default.Save();
        }

        private bool _hasBeenCalledAlready;

        #region Dispose
        protected virtual void Dispose(bool disposing)
        {
            if (!_hasBeenCalledAlready)
            {
                if (disposing)
                {
                    //  dispose managed state (managed objects).
                    _dcsStopGearTimer.Dispose();
                    _dcsBios?.Shutdown();
                    _dcsBios?.Dispose();
                    BIOSEventHandler.DetachConnectionListener(this);
                    REFEventHandler.DetachDataListener(this);
                    BIOSEventHandler.DetachStringListener(this);
                }

                //  free unmanaged resources (unmanaged objects) and override a finalizer below.

                //  set large fields to null.
                _hasBeenCalledAlready = true;
            }
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);

            //  uncomment the following line if the finalizer is overridden above.
            GC.SuppressFinalize(this);
        }

        #endregion

        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_formLoaded)
                {
                    return;
                }

                var result = Common.CheckJSONDirectory(Settings.Default.DCSBiosJSONLocation);
                if (result.Item1 == false && result.Item2 == false)
                {
                    MessageBox.Show(this, "Cannot continue, DCS-BIOS not found. Check DCS-BIOS path under Options.", "DCS-BIOS Not Found", MessageBoxButton.OK);
                    return;
                }
                SetTopMost();
                DCSBIOSControlLocator.JSONDirectory = Settings.Default.DCSBiosJSONLocation;
                DCSAircraft.FillModulesListFromDcsBios(Settings.Default.DCSBiosJSONLocation, true, false);
                UpdateComboBoxModules();
                CreateDCSBIOS();
                StartupDCSBIOS();
                BIOSEventHandler.AttachConnectionListener(this);
                _dcsStopGearTimer.Elapsed += TimerStopRotation;
                _dcsStopGearTimer.Start();
                FindDCSBIOSControls();
                _formLoaded = true;
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void SetFormState()
        {
            MenuItemExportValues.IsEnabled = _dcsbiosUIControlPanels.Count > 0;
        }

        private void FindDCSBIOSControls()
        {
            if (!DCSAircraft.HasDCSBIOSModules)
            {
                return;
            }
            _metaControls = DCSBIOSControlLocator.LoadMetaControls();
            _dcsbiosVersionOutput = new DCSBIOSOutput();
            _dcsbiosVersionOutput.Consume(_metaControls.Find(o => o.Identifier == "DCS_BIOS"));
            DCSBIOSStringManager.AddListeningAddress(_dcsbiosVersionOutput);
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

        private void CreateDCSBIOS()
        {
            if (_dcsBios != null)
            {
                return;
            }

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

        private void MenuSetDCSBIOSPath_OnClick(object sender, RoutedEventArgs e)
        {
            /*
             * Must remove topmost because settings window will be stuck behind main window.
             */
            var topMost = Topmost;
            Topmost = false;
            try
            {

                var settingsWindow = new SettingsWindow(0);
                if (settingsWindow.ShowDialog() == true)
                {
                    if (settingsWindow.DCSBIOSChanged)
                    {
                        DCSBIOSControlLocator.JSONDirectory = Settings.Default.DCSBiosJSONLocation;
                        DCSAircraft.FillModulesListFromDcsBios(Settings.Default.DCSBiosJSONLocation, true, false);
                        UpdateComboBoxModules();
                    }
                }
            }
            finally
            {
                Topmost = topMost;
            }
        }

        private bool CheckDCSBIOSStatus()
        {
            if (!DCSAircraft.Modules.Any())
            {
                MessageBox.Show(this,
                    "No DCS-BIOS modules found. Make sure DCS-BIOS is correctly installed and that the path is set in Settings.", "DCS-BIOS Not Found", MessageBoxButton.OK);
                return false;
            }

            return true;
        }

        private void UpdateComboBoxModules()
        {
            if (!CheckDCSBIOSStatus())
            {
                return;
            }
            ComboBoxModules.DataContext = DCSAircraft.Modules;
            ComboBoxModules.ItemsSource = DCSAircraft.Modules;
            ComboBoxModules.Items.Refresh();
            ComboBoxModules.SelectedIndex = 0;
            UpdateComboBoxCategories();
        }

        private void UpdateComboBoxCategories()
        {
            var categoriesList = _loadedControls.Select(o => o.Category).DistinctBy(o => o).ToList();

            if (_loadedControls.Count() <= MAX_CONTROLS_ON_PAGE)
            {
                /*
                 * If there aren't many controls to show then allow the user to show
                 * all categories at once.
                 */
                categoriesList.Insert(0, "All");
            }
            ComboBoxCategory.DataContext = categoriesList;
            ComboBoxCategory.ItemsSource = categoriesList;
            ComboBoxCategory.Items.Refresh();
            ComboBoxCategory.SelectedIndex = 0;
        }

        private void ComboBoxModules_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (ComboBoxModules.SelectedValue == null)
                {
                    return;
                }
                var selectedModule = (DCSAircraft)ComboBoxModules.SelectedItem;
                DCSBIOSControlLocator.DCSAircraft = selectedModule;
                _loadedControls = DCSBIOSControlLocator.ReadDataFromJsonFileSimple(selectedModule.JSONFilename);
                UpdateComboBoxCategories();
                ShowControls();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void ComboBoxCategory_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                TextBoxSearchControl.Text = "";
                ShowControls();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        public void ChangeCategory(object sender, CategoryEventArgs args)
        {
            try
            {
                ComboBoxCategory.SelectedValue = args.Category;
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void ButtonReloadJSON_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (!CheckDCSBIOSStatus())
                {
                    return;
                }
                var selectedModule = ComboBoxModules.SelectedValue;
                var selectedCategory = ComboBoxCategory.SelectedValue.ToString();
                DCSBIOSControlLocator.DCSAircraft = null;
                UpdateComboBoxModules();
                ComboBoxModules.SelectedValue = selectedModule;
                ComboBoxCategory.SelectedValue = selectedCategory;
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void ShowControls(bool searching = false)
        {
            try
            {
                try
                {
                    if (ComboBoxModules.SelectedValue == null || ComboBoxCategory.SelectedValue == null)
                    {
                        return;
                    }

                    Mouse.OverrideCursor = Cursors.Wait;
                    _dcsbiosUIControlPanels.Clear();
                    var searchText = string.IsNullOrEmpty(TextBoxSearchControl.Text) ? "" : TextBoxSearchControl.Text.Trim();
                    var filteredControls = _loadedControls;

                    /*
                     * Limit only on category if user is not searching
                     */
                    if (string.IsNullOrEmpty(searchText) && ComboBoxCategory.SelectedValue != null && ComboBoxCategory.SelectedValue.ToString() != "All")
                    {
                        filteredControls = _loadedControls.Where(o => o.Category == ComboBoxCategory.SelectedValue.ToString())
                            .ToList();
                    }

                    if (!string.IsNullOrEmpty(searchText))
                    {
                        var searchWord = searchText.ToLower();
                        filteredControls = _loadedControls.Where(o => o.Description.ToLower().Contains(searchWord) || o.Identifier.ToLower().Contains(searchWord))
                            .ToList();
                    }

                    if (filteredControls.Count() > MAX_CONTROLS_ON_PAGE)
                    {
                        Common.ShowMessageBox($"Query returned {filteredControls.Count()} DCS-BIOS Controls. Max controls that can be displayed at any time is {MAX_CONTROLS_ON_PAGE}.");
                        return;
                    }

                    foreach (var dcsbiosControl in filteredControls)
                    {
                        _dcsbiosUIControlPanels.Add(new DCSBIOSControlUserControl(dcsbiosControl));
                    }

                    ItemsControlControls.ItemsSource = null;
                    ItemsControlControls.Items.Clear();
                    ItemsControlControls.ItemsSource = _dcsbiosUIControlPanels;

                    LabelStatusBarRightInformation.Text = $"{filteredControls.Count()} DCS-BIOS Controls loaded.";

                    if (filteredControls.Any())
                    {
                        ItemsControlControls.Focus();
                    }

                    UpdateSearchButton();

                    if (searching)
                    {
                        TextBoxSearchControl.Focus();
                    }
                }
                finally
                {
                    Mouse.OverrideCursor = Cursors.Arrow;
                    SetFormState();
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
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

        private void MenuItemExit_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                Close();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void MenuItemDiscord_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void UpdateSearchButton()
        {

            if (!string.IsNullOrEmpty(TextBoxSearchControl.Text))
            {
                ButtonSearchControls.Source = new BitmapImage(new Uri(@"/ctrlref;component/Images/clear_search_result.png", UriKind.Relative));
                ButtonSearchControls.Tag = "Clear";
            }
            else
            {
                ButtonSearchControls.Source = new BitmapImage(new Uri(@"/ctrlref;component/Images/search_controls.png", UriKind.Relative));
                ButtonSearchControls.Tag = "Search";
            }
        }

        private void ButtonSearchControls_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                var mode = (string)ButtonSearchControls.Tag;
                if (mode == "Search")
                {
                    ButtonSearchControls.Source = new BitmapImage(new Uri(@"/ctrlref;component/Images/clear_search_result.png", UriKind.Relative));
                    ButtonSearchControls.Tag = "Clear";
                }
                else
                {
                    ButtonSearchControls.Source = new BitmapImage(new Uri(@"/ctrlref;component/Images/search_controls.png", UriKind.Relative));
                    ButtonSearchControls.Tag = "Search";
                    TextBoxSearchControl.Text = "";
                }

                ShowControls(true);
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
                Dispose(true);
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void MainWindow_OnLocationChanged(object sender, EventArgs e)
        {
            try
            {
                if (!_formLoaded)
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

        private void MainWindow_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            try
            {
                if (!_formLoaded)
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

        private void TextBoxSearchControl_OnKeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    ShowControls(true);
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void SetTopMost()
        {
            try
            {
                Settings.Default.AlwaysOnTop = !Settings.Default.AlwaysOnTop;
                Settings.Default.Save();
                MenuSetAlwaysOnTop.IsChecked = Settings.Default.AlwaysOnTop;
                Topmost = Settings.Default.AlwaysOnTop;
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void MenuSetAlwaysOnTop_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                SetTopMost();
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

        private void MenuItemExportValues_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var result = new StringBuilder();
                foreach (var dcsbiosControlUserControl in _dcsbiosUIControlPanels.Where(o => o.HasValue == true))
                {
                    result.AppendLine($"{dcsbiosControlUserControl.Identifier,-30}{dcsbiosControlUserControl.CurrentValue,-30}");
                }

                Clipboard.SetText(result.ToString());
                SystemSounds.Exclamation.Play();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

    }
}
