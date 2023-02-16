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

namespace ControlReference
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IDisposable, IDcsBiosConnectionListener, ICategoryChange
    {
        private IEnumerable<DCSBIOSControl> _loadedControls = null;
        private readonly List<DCSBIOSControlUserControl> _dcsbiosUIControlPanels = new();
        private readonly Timer _dcsStopGearTimer = new(5000);
        private DCSBIOS _dcsBios;
        private bool _formLoaded = false;
        private const int MAX_CONTROLS_ON_PAGE = 70;

        public MainWindow()
        {
            InitializeComponent();
            REFEventHandler.AttachDataListener(this);
        }

        private bool _hasBeenCalledAlready;
        protected virtual void Dispose(bool disposing)
        {
            if (!_hasBeenCalledAlready)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    _dcsStopGearTimer.Dispose();
                    _dcsBios?.Shutdown();
                    _dcsBios?.Dispose();
                    BIOSEventHandler.DetachConnectionListener(this);
                    REFEventHandler.AttachDataListener(this);
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.

                // TODO: set large fields to null.
                _hasBeenCalledAlready = true;
            }
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);

            // TODO: uncomment the following line if the finalizer is overridden above.
            GC.SuppressFinalize(this);
        }

        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_formLoaded)
                {
                    return;
                }

                SetTopMost();
                DCSBIOSControlLocator.JSONDirectory = Settings.Default.DCSBiosJSONLocation;
                DCSAircraft.FillModulesListFromDcsBios(DCSBIOSCommon.GetDCSBIOSJSONDirectory(Settings.Default.DCSBiosJSONLocation), true, false);
                UpdateComboBoxModules();
                CreateDCSBIOS();
                StartupDCSBIOS();
                BIOSEventHandler.AttachConnectionListener(this);
                _dcsStopGearTimer.Elapsed += TimerStopRotation;
                _dcsStopGearTimer.Start();
                _formLoaded = true;
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void SetFormState()
        {

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
            var settingsWindow = new SettingsWindow(0);
            if (settingsWindow.ShowDialog() == true)
            {
                if (settingsWindow.DCSBIOSChanged)
                {
                    DCSBIOSControlLocator.JSONDirectory = Settings.Default.DCSBiosJSONLocation;
                    DCSAircraft.FillModulesListFromDcsBios(DCSBIOSCommon.GetDCSBIOSJSONDirectory(Settings.Default.DCSBiosJSONLocation), true);
                    UpdateComboBoxModules();
                }
            }
        }

        private void UpdateComboBoxModules()
        {
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
                categoriesList.Insert(0,"All");
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
                var selectedModule = (DCSAircraft)ComboBoxModules.SelectedItem;
                DCSBIOSControlLocator.DCSAircraft = selectedModule;
                _loadedControls = DCSBIOSControlLocator.GetControls(true);
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

        private void ShowControls()
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

                    var filteredControls = _loadedControls;

                    /*
                     * Limit only on category if user is not searching
                     */
                    if (string.IsNullOrEmpty(TextBoxSearchControl.Text) && ComboBoxCategory.SelectedValue != null && ComboBoxCategory.SelectedValue.ToString() != "All")
                    {
                        filteredControls = _loadedControls.Where(o => o.Category == ComboBoxCategory.SelectedValue.ToString())
                            .ToList();
                    }

                    if (!string.IsNullOrEmpty(TextBoxSearchControl.Text))
                    {
                        var searchWord = TextBoxSearchControl.Text.ToLower();
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

        private void ButtonSearchControls_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                ShowControls();
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
                    ShowControls();
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
    }
}
