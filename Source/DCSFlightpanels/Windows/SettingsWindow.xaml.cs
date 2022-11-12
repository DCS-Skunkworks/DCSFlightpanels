using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;
using ClassLibraryCommon;
using DCS_BIOS;
using DCSFlightpanels.Properties;
using NonVisuals.EventArgs;

namespace DCSFlightpanels.Windows
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {

        public string IpAddressFromDCSBIOS { get; private set; }
        public string PortFromDCSBIOS { get; private set; }
        public string IpAddressToDCSBIOS { get; private set; }
        public string PortToDCSBIOS { get; private set; }
        public string DcsBiosJSONLocation { get; private set; }
        public string IpAddressFromSRS { get; private set; }
        public string PortFromSRS { get; private set; }
        public string IpAddressToSRS { get; private set; }
        public string PortToSRS { get; private set; }
        public bool GeneralChanged { get; private set; } = false;
        public bool DCSBIOSChanged { get; private set; } = false;
        public bool StreamDeckChanged { get; private set; } = false;

        private bool _isLoaded = false;
        private readonly int _tabIndex;

        public SettingsWindow(int tabIndex)
        {
            InitializeComponent();
            _tabIndex = tabIndex;
        }

        private void SetFormState()
        {
            CheckDCSBIOSStatus();
        }

        private void SettingsWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_isLoaded)
                {
                    return;
                }

                TabControlSettings.SelectedIndex = _tabIndex;
                ButtonOk.IsEnabled = false;
                LoadSettings();
                SetEventsHandlers();
                SetFormState();
                _isLoaded = true;
            }
            catch (Exception exception)
            {
                System.Windows.MessageBox.Show(exception.Message + Environment.NewLine + exception.StackTrace);
            }
        }

        private void CheckDCSBIOSStatus()
        {
            var result = DCSBIOSCommon.CheckJSONDirectory(DCSBIOSCommon.GetDCSBIOSJSONDirectory(TextBoxDcsBiosJSONLocation.Text));

            if (result.Item1 == false && result.Item2 == false)
            {
                LabelDCSBIOSNotFound.Foreground = Brushes.Red;
                LabelDCSBIOSNotFound.Content = "<-- Warning, folder does not exist.";
                return;
            }

            if (result.Item1 == true && result.Item2 == false)
            {
                LabelDCSBIOSNotFound.Foreground = Brushes.Red;
                LabelDCSBIOSNotFound.Content = "<-- Warning, folder does not contain JSON files.";
                return;
            }

            LabelDCSBIOSNotFound.Foreground = Brushes.LimeGreen;
            LabelDCSBIOSNotFound.Content = " JSON files found in folder.";
        }

        private void SetEventsHandlers()
        {
            RadioButtonBelowNormal.Checked += GeneralDirty;
            RadioButtonNormal.Checked += GeneralDirty;
            RadioButtonAboveNormal.Checked += GeneralDirty;
            RadioButtonHigh.Checked += GeneralDirty;
            RadioButtonRealtime.Checked += GeneralDirty;
            RadioButtonKeyBd.Checked += GeneralDirty;
            RadioButtonSendInput.Checked += GeneralDirty;
            CheckBoxMinimizeToTray.Checked += GeneralDirty;
            CheckBoxMinimizeToTray.Unchecked += GeneralDirty;
            CheckBoxEnablePluginSupport.Checked += GeneralDirty;
            CheckBoxEnablePluginSupport.Unchecked += GeneralDirty;

            TextBoxDcsBiosJSONLocation.TextChanged += DcsBiosDirty;
            TextBoxDCSBIOSFromIP.TextChanged += DcsBiosDirty;
            TextBoxDCSBIOSToIP.TextChanged += DcsBiosDirty;
            TextBoxDCSBIOSFromPort.TextChanged += DcsBiosDirty;
            TextBoxDCSBIOSToPort.TextChanged += DcsBiosDirty;
        }

        private void LoadSettings()
        {
            switch (Settings.Default.ProcessPriority)
            {
                case ProcessPriorityClass.BelowNormal:
                    {
                        RadioButtonBelowNormal.IsChecked = true;
                        break;
                    }
                case ProcessPriorityClass.Normal:
                    {
                        RadioButtonNormal.IsChecked = true;
                        break;
                    }
                case ProcessPriorityClass.AboveNormal:
                    {
                        RadioButtonAboveNormal.IsChecked = true;
                        break;
                    }
                case ProcessPriorityClass.High:
                    {
                        RadioButtonHigh.IsChecked = true;
                        break;
                    }
                case ProcessPriorityClass.RealTime:
                    {
                        RadioButtonRealtime.IsChecked = true;
                        break;
                    }
            }
            if (Settings.Default.APIMode == 0)
            {
                RadioButtonKeyBd.IsChecked = true;
            }
            else
            {
                RadioButtonSendInput.IsChecked = true;
            }

            CheckBoxMinimizeToTray.IsChecked = Settings.Default.RunMinimized;
            CheckBoxEnablePluginSupport.IsChecked = Settings.Default.EnablePlugin;
            CheckBoxDisableKeyboardAPI.IsChecked = Settings.Default.DisableKeyboardAPI;

            TextBoxDcsBiosJSONLocation.Text = Settings.Default.DCSBiosJSONLocation;
            TextBoxDCSBIOSFromIP.Text = Settings.Default.DCSBiosIPFrom;
            TextBoxDCSBIOSToIP.Text = Settings.Default.DCSBiosIPTo;
            TextBoxDCSBIOSFromPort.Text = Settings.Default.DCSBiosPortFrom;
            TextBoxDCSBIOSToPort.Text = Settings.Default.DCSBiosPortTo;
        }

        private void ButtonCancel_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void ButtonOk_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                CheckValuesDCSBIOS();

                if (GeneralChanged)
                {
                    if (RadioButtonKeyBd.IsChecked == true)
                    {
                        Settings.Default.APIMode = 0;
                        Common.APIMode = APIModeEnum.keybd_event;
                        Settings.Default.Save();
                    }
                    if (RadioButtonSendInput.IsChecked == true)
                    {
                        Settings.Default.APIMode = 1;
                        Common.APIMode = APIModeEnum.SendInput;
                        Settings.Default.Save();
                    }
                    if (RadioButtonBelowNormal.IsChecked == true)
                    {
                        Settings.Default.ProcessPriority = ProcessPriorityClass.BelowNormal;
                        Settings.Default.Save();
                        Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.BelowNormal;
                    }
                    if (RadioButtonNormal.IsChecked == true)
                    {
                        Settings.Default.ProcessPriority = ProcessPriorityClass.Normal;
                        Settings.Default.Save();
                        Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.Normal;
                    }
                    if (RadioButtonAboveNormal.IsChecked == true)
                    {
                        Settings.Default.ProcessPriority = ProcessPriorityClass.AboveNormal;
                        Settings.Default.Save();
                        Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.AboveNormal;
                    }
                    if (RadioButtonHigh.IsChecked == true)
                    {
                        Settings.Default.ProcessPriority = ProcessPriorityClass.High;
                        Settings.Default.Save();
                        Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.High;
                    }
                    if (RadioButtonRealtime.IsChecked == true)
                    {
                        Settings.Default.ProcessPriority = ProcessPriorityClass.RealTime;
                        Settings.Default.Save();
                        Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.RealTime;
                    }

                    Settings.Default.Save();

                    Settings.Default.RunMinimized = CheckBoxMinimizeToTray.IsChecked == true;
                    Settings.Default.EnablePlugin = CheckBoxEnablePluginSupport.IsChecked == true;
                    Settings.Default.DisableKeyboardAPI = CheckBoxDisableKeyboardAPI.IsChecked == true;
                    Settings.Default.Save();
                }

                if (DCSBIOSChanged)
                {
                    Settings.Default.DCSBiosJSONLocation = TextBoxDcsBiosJSONLocation.Text;
                    Settings.Default.DCSBiosIPFrom = IpAddressFromDCSBIOS;
                    Settings.Default.DCSBiosPortFrom = PortFromDCSBIOS;
                    Settings.Default.DCSBiosIPTo = IpAddressToDCSBIOS;
                    Settings.Default.DCSBiosPortTo = PortToDCSBIOS;
                    Settings.Default.Save();
                }

                if (StreamDeckChanged)
                {
                    Settings.Default.Save();
                }
                DialogResult = true;
                Close();
            }
            catch (Exception exception)
            {
                System.Windows.MessageBox.Show($"{exception.Message}{Environment.NewLine}{exception.StackTrace}");
            }
        }

        private void ButtonBrowse_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var folderBrowserDialog = new FolderBrowserDialog()
                {
                    ShowNewFolderButton = false
                };

                if (!string.IsNullOrEmpty(DCSBIOSCommon.GetDCSBIOSJSONDirectory(Settings.Default.DCSBiosJSONLocation)))
                {
                    folderBrowserDialog.SelectedPath = DCSBIOSCommon.GetDCSBIOSJSONDirectory(Settings.Default.DCSBiosJSONLocation);
                }

                if (folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    var result = DCSBIOSCommon.CheckJSONDirectory(folderBrowserDialog.SelectedPath);
                    if (result.Item1 == true && result.Item2 == true)
                    {
                        TextBoxDcsBiosJSONLocation.Text = folderBrowserDialog.SelectedPath;
                    }
                    else if (result.Item1 == true && result.Item2 == false)
                    {
                        System.Windows.MessageBox.Show("Cannot use selected directory as it did not contain JSON files.", "Invalid directory", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }

                SetFormState();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void CheckValuesDCSBIOS()
        {
            try
            {
                IPAddress ipAddress;
                if (string.IsNullOrEmpty(TextBoxDCSBIOSFromIP.Text))
                {
                    throw new Exception("DCS-BIOS IP address from cannot be empty");
                }
                if (string.IsNullOrEmpty(TextBoxDCSBIOSToIP.Text))
                {
                    throw new Exception("DCS-BIOS IP address to cannot be empty");
                }
                if (string.IsNullOrEmpty(TextBoxDCSBIOSFromPort.Text))
                {
                    throw new Exception("DCS-BIOS Port from cannot be empty");
                }
                if (string.IsNullOrEmpty(TextBoxDCSBIOSToPort.Text))
                {
                    throw new Exception("DCS-BIOS Port to cannot be empty");
                }
                if (string.IsNullOrEmpty(TextBoxDcsBiosJSONLocation.Text))
                {
                    throw new Exception("DCS-BIOS JSON directory cannot be empty");
                }
                try
                {
                    if (!IPAddress.TryParse(TextBoxDCSBIOSFromIP.Text, out ipAddress))
                    {
                        throw new Exception();
                    }
                    IpAddressFromDCSBIOS = TextBoxDCSBIOSFromIP.Text;
                }
                catch (Exception ex)
                {
                    throw new Exception($"DCS-BIOS Error while checking IP from : {ex.Message}");
                }
                try
                {
                    if (!IPAddress.TryParse(TextBoxDCSBIOSToIP.Text, out ipAddress))
                    {
                        throw new Exception();
                    }
                    IpAddressToDCSBIOS = TextBoxDCSBIOSToIP.Text;
                }
                catch (Exception ex)
                {
                    throw new Exception($"DCS-BIOS Error while checking IP to : {ex.Message}");
                }
                try
                {
                    var test = Convert.ToInt32(TextBoxDCSBIOSFromPort.Text);
                    PortFromDCSBIOS = TextBoxDCSBIOSFromPort.Text;
                }
                catch (Exception ex)
                {
                    throw new Exception($"DCS-BIOS Error while Port from : {ex.Message}");
                }
                try
                {
                    var test = Convert.ToInt32(TextBoxDCSBIOSFromPort.Text);
                    PortToDCSBIOS = TextBoxDCSBIOSToPort.Text;
                }
                catch (Exception ex)
                {
                    throw new Exception($"DCS-BIOS Error while Port to : {ex.Message}");
                }
                try
                {
                    var directoryInfo = new DirectoryInfo(TextBoxDcsBiosJSONLocation.Text);
                    DcsBiosJSONLocation = TextBoxDcsBiosJSONLocation.Text;
                }
                catch (Exception ex)
                {
                    throw new Exception($"DCS-BIOS Error while checking DCS-BIOS location : {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"DCS-BIOS Error checking values : {Environment.NewLine}{ex.Message}");
            }
        }

        private void DcsBiosDirty(object sender, TextChangedEventArgs e)
        {
            DCSBIOSChanged = true;
            ButtonOk.IsEnabled = true;
        }

        private void SrsDirty(object sender, TextChangedEventArgs e)
        {
            ButtonOk.IsEnabled = true;
        }

        private void SettingsWindow_OnKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (!ButtonOk.IsEnabled && e.Key == Key.Escape)
            {
                DialogResult = false;
                e.Handled = true;
                Close();
            }
        }

        private void GeneralDirty(object sender, RoutedEventArgs e)
        {
            GeneralChanged = true;
            ButtonOk.IsEnabled = true;
        }

        private void HyperlinkRequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = e.Uri.AbsoluteUri,
                UseShellExecute = true
            });
            
            e.Handled = true;
        }

        private void CheckBoxDisableKeyboardAPI_OnChecked(object sender, RoutedEventArgs e)
        {
            try
            {
                AppEventHandler.ForwardKeyPressEvent(this, false);
                GeneralDirty(sender, e);
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void CheckBoxDisableKeyboardAPI_OnUnchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                AppEventHandler.ForwardKeyPressEvent(this, true);
                GeneralDirty(sender, e);
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }
    }
}
