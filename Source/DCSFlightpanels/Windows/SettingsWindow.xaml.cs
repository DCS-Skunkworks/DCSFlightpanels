using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using ClassLibraryCommon;
using DCS_BIOS;
using DCSFlightpanels.Properties;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using MessageBox = System.Windows.MessageBox;

namespace DCSFlightpanels.Windows
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {

        private bool _generalChanged = false;
        private bool _dcsbiosChanged = false;
        private bool _srsChanged = false;

        private string _ipAddressFromDCSBIOS;
        private string _portFromDCSBIOS;
        private string _ipAddressToDCSBIOS;
        private string _portToDCSBIOS;
        private string _dcsBiosJSONLocation;

        private string _ipAddressFromSRS;
        private string _portFromSRS;
        private string _ipAddressToSRS;
        private string _portToSRS;


        public SettingsWindow()
        {
            InitializeComponent();
        }

        private void SettingsWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                ButtonOk.IsEnabled = false;
                LoadSettings();
                if (!Common.IsEmulationModesFlagSet(EmulationMode.SRSEnabled))
                {
                    LabelSRS.Visibility = Visibility.Collapsed;
                }
                else if (Common.NoDCSBIOSEnabled())
                {
                    LabelDCSBIOS.Visibility = Visibility.Collapsed;
                }
                StackPanelGeneralSettings.Visibility = Visibility.Visible;
                StackPanelDCSBIOSSettings.Visibility = Visibility.Collapsed;
                StackPanelSRSSettings.Visibility = Visibility.Collapsed;
                ActivateTriggers();
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message + Environment.NewLine + exception.StackTrace);
            }
        }

        private void ActivateTriggers()
        {
            RadioButtonBelowNormal.Checked += RadioButtonProcessPriority_OnChecked;
            RadioButtonNormal.Checked += RadioButtonProcessPriority_OnChecked;
            RadioButtonAboveNormal.Checked += RadioButtonProcessPriority_OnChecked;
            RadioButtonHigh.Checked += RadioButtonProcessPriority_OnChecked;
            RadioButtonRealtime.Checked += RadioButtonProcessPriority_OnChecked;
            RadioButtonKeyBd.Checked += RadioButtonAPI_OnChecked;
            RadioButtonSendInput.Checked += RadioButtonAPI_OnChecked;
            TextBoxDcsBiosJSONLocation.TextChanged += TextBoxDcsBios_OnTextChanged;
            TextBoxDCSBIOSFromIP.TextChanged += TextBoxDcsBios_OnTextChanged;
            TextBoxDCSBIOSToIP.TextChanged += TextBoxDcsBios_OnTextChanged;
            TextBoxDCSBIOSFromPort.TextChanged += TextBoxDcsBios_OnTextChanged;
            TextBoxDCSBIOSToPort.TextChanged += TextBoxDcsBios_OnTextChanged;
            TextBoxSRSFromIP.TextChanged += TextBoxSRS_OnTextChanged;
            TextBoxSRSToIP.TextChanged += TextBoxSRS_OnTextChanged;
            TextBoxSRSFromPort.TextChanged += TextBoxSRS_OnTextChanged;
            TextBoxSRSToPort.TextChanged += TextBoxSRS_OnTextChanged;
            CheckBoxMinimizeToTray.Checked += CheckBoxMinimizeToTray_OnChecked;
            CheckBoxMinimizeToTray.Unchecked += CheckBoxMinimizeToTray_OnUnchecked;
            CheckBoxEnablePluginSupport.Checked += CheckBoxMinimizeToTray_OnChecked;
            CheckBoxEnablePluginSupport.Unchecked += this.CheckBoxMinimizeToTray_OnUnchecked;
            CheckBoxDisableKeyboardAPI.Checked += CheckBoxMinimizeToTray_OnChecked;
            CheckBoxDisableKeyboardAPI.Unchecked += this.CheckBoxMinimizeToTray_OnUnchecked;
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

            if (Common.PartialDCSBIOSEnabled())
            {
                TextBoxDcsBiosJSONLocation.Text = Settings.Default.DCSBiosJSONLocation;
                TextBoxDCSBIOSFromIP.Text = Settings.Default.DCSBiosIPFrom;
                TextBoxDCSBIOSToIP.Text = Settings.Default.DCSBiosIPTo;
                TextBoxDCSBIOSFromPort.Text = Settings.Default.DCSBiosPortFrom;
                TextBoxDCSBIOSToPort.Text = Settings.Default.DCSBiosPortTo;
            }
            if (Common.IsEmulationModesFlagSet(EmulationMode.SRSEnabled))
            {
                TextBoxSRSFromIP.Text = Settings.Default.SRSIpFrom;
                TextBoxSRSToIP.Text = Settings.Default.SRSIpTo;
                TextBoxSRSFromPort.Text = Settings.Default.SRSPortFrom.ToString();
                TextBoxSRSToPort.Text = Settings.Default.SRSPortTo.ToString();
            }
        }

        private void GeneralSettings_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            StackPanelGeneralSettings.Visibility = Visibility.Visible;
            StackPanelDCSBIOSSettings.Visibility = Visibility.Collapsed;
            StackPanelSRSSettings.Visibility = Visibility.Collapsed;
        }

        private void DCSBIOS_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            StackPanelGeneralSettings.Visibility = Visibility.Collapsed;
            StackPanelDCSBIOSSettings.Visibility = Visibility.Visible;
            StackPanelSRSSettings.Visibility = Visibility.Collapsed;
        }

        private void SRS_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            StackPanelGeneralSettings.Visibility = Visibility.Collapsed;
            StackPanelDCSBIOSSettings.Visibility = Visibility.Collapsed;
            StackPanelSRSSettings.Visibility = Visibility.Visible;
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
                CheckValuesSRS();

                if (_generalChanged)
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
                    Settings.Default.DisableKeyboardAPI = this.CheckBoxDisableKeyboardAPI.IsChecked == true;
                    Settings.Default.Save();
                }

                if (_dcsbiosChanged)
                {
                    Settings.Default.DCSBiosJSONLocation = TextBoxDcsBiosJSONLocation.Text;
                    Settings.Default.DCSBiosIPFrom = IpAddressFromDCSBIOS;
                    Settings.Default.DCSBiosPortFrom = PortFromDCSBIOS;
                    Settings.Default.DCSBiosIPTo = IpAddressToDCSBIOS;
                    Settings.Default.DCSBiosPortTo = PortToDCSBIOS;
                    Settings.Default.Save();
                }

                if (_srsChanged)
                {
                    Settings.Default.SRSIpFrom = IpAddressFromSRS;
                    Settings.Default.SRSPortFrom = int.Parse(PortFromSRS);
                    Settings.Default.SRSIpTo = IpAddressToSRS;
                    Settings.Default.SRSPortTo = int.Parse(PortToSRS);
                    Settings.Default.Save();
                }
                DialogResult = true;
                Close();
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message + Environment.NewLine + exception.StackTrace);
            }
        }

        private void ButtonBrowse_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var folderBrowserDialog = new FolderBrowserDialog();
                folderBrowserDialog.ShowNewFolderButton = false;
                if (!string.IsNullOrEmpty(DCS_BIOS.DBCommon.GetDCSBIOSJSONDirectory(Settings.Default.DCSBiosJSONLocation)))
                {
                    folderBrowserDialog.SelectedPath = DBCommon.GetDCSBIOSJSONDirectory(Settings.Default.DCSBiosJSONLocation);
                }

                if (folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    if (CheckJSONDirectory(folderBrowserDialog.SelectedPath))
                    {
                        TextBoxDcsBiosJSONLocation.Text = folderBrowserDialog.SelectedPath;
                    }
                    else
                    {
                        MessageBox.Show("Cannot use selected directory as it did not contain json files.", "Invalid directory", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private bool CheckJSONDirectory(string jsonDirectory)
        {
            jsonDirectory = DBCommon.GetDCSBIOSJSONDirectory(jsonDirectory);

            if (string.IsNullOrEmpty(jsonDirectory) || !Directory.Exists(jsonDirectory))
            {
                return false;
            }

            var files = Directory.EnumerateFiles(jsonDirectory);

            foreach (var filename in files)
            {
                if (filename.ToLower().EndsWith(".json"))
                {
                    return true;
                }
            }

            return false;
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
                    _ipAddressFromDCSBIOS = TextBoxDCSBIOSFromIP.Text;
                }
                catch (Exception e)
                {
                    throw new Exception("DCS-BIOS Error while checking IP from : " + e.Message);
                }
                try
                {
                    if (!IPAddress.TryParse(TextBoxDCSBIOSToIP.Text, out ipAddress))
                    {
                        throw new Exception();
                    }
                    _ipAddressToDCSBIOS = TextBoxDCSBIOSToIP.Text;
                }
                catch (Exception e)
                {
                    throw new Exception("DCS-BIOS Error while checking IP to : " + e.Message);
                }
                try
                {
                    var test = Convert.ToInt32(TextBoxDCSBIOSFromPort.Text);
                    _portFromDCSBIOS = TextBoxDCSBIOSFromPort.Text;
                }
                catch (Exception e)
                {
                    throw new Exception("DCS-BIOS Error while Port from : " + e.Message);
                }
                try
                {
                    var test = Convert.ToInt32(TextBoxDCSBIOSFromPort.Text);
                    _portToDCSBIOS = TextBoxDCSBIOSToPort.Text;
                }
                catch (Exception e)
                {
                    throw new Exception("DCS-BIOS Error while Port to : " + e.Message);
                }
                try
                {
                    var directoryInfo = new DirectoryInfo(TextBoxDcsBiosJSONLocation.Text);
                    _dcsBiosJSONLocation = TextBoxDcsBiosJSONLocation.Text;
                }
                catch (Exception e)
                {
                    throw new Exception("DCS-BIOS Error while checking DCS-BIOS location : " + e.Message);
                }
            }
            catch (Exception e)
            {
                throw new Exception("DCS-BIOS Error checking values : " + Environment.NewLine + e.Message);
            }
        }

        private void CheckValuesSRS()
        {
            try
            {
                IPAddress ipAddress;
                if (string.IsNullOrEmpty(TextBoxSRSFromIP.Text))
                {
                    throw new Exception("SRS IP address from cannot be empty");
                }
                if (string.IsNullOrEmpty(TextBoxSRSToIP.Text))
                {
                    throw new Exception("SRS IP address to cannot be empty");
                }
                if (string.IsNullOrEmpty(TextBoxSRSFromPort.Text))
                {
                    throw new Exception("SRS Port from cannot be empty");
                }
                if (string.IsNullOrEmpty(TextBoxSRSToPort.Text))
                {
                    throw new Exception("SRS Port to cannot be empty");
                }
                try
                {
                    if (!IPAddress.TryParse(TextBoxSRSFromIP.Text, out ipAddress))
                    {
                        throw new Exception();
                    }
                    _ipAddressFromSRS = TextBoxSRSFromIP.Text;
                }
                catch (Exception e)
                {
                    throw new Exception("SRS Error while checking IP from : " + e.Message);
                }
                try
                {
                    if (!IPAddress.TryParse(TextBoxSRSToIP.Text, out ipAddress))
                    {
                        throw new Exception();
                    }
                    _ipAddressToSRS = TextBoxSRSToIP.Text;
                }
                catch (Exception e)
                {
                    throw new Exception("SRS Error while checking IP to : " + e.Message);
                }
                try
                {
                    var test = Convert.ToInt32(TextBoxSRSFromPort.Text);
                    _portFromSRS = TextBoxSRSFromPort.Text;
                }
                catch (Exception e)
                {
                    throw new Exception("SRS Error while Port from : " + e.Message);
                }
                try
                {
                    var test = Convert.ToInt32(TextBoxSRSFromPort.Text);
                    _portToSRS = TextBoxSRSToPort.Text;
                }
                catch (Exception e)
                {
                    throw new Exception("SRS Error while Port to : " + e.Message);
                }
            }
            catch (Exception e)
            {
                throw new Exception("SRS Error checking values : " + Environment.NewLine + e.Message);
            }
        }


        public string IpAddressFromDCSBIOS => _ipAddressFromDCSBIOS;

        public string PortFromDCSBIOS => _portFromDCSBIOS;

        public string IpAddressToDCSBIOS => _ipAddressToDCSBIOS;

        public string PortToDCSBIOS => _portToDCSBIOS;

        public string DcsBiosJSONLocation => _dcsBiosJSONLocation;

        public string IpAddressFromSRS => _ipAddressFromSRS;

        public string PortFromSRS => _portFromSRS;

        public string IpAddressToSRS => _ipAddressToSRS;

        public string PortToSRS => _portToSRS;

        public bool GeneralChanged => _generalChanged;

        public bool DCSBIOSChanged => _dcsbiosChanged;

        public bool SRSChanged => _srsChanged;


        private void TextBoxDcsBios_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            _dcsbiosChanged = true;
            ButtonOk.IsEnabled = true;
        }

        private void TextBoxSRS_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            _srsChanged = true;
            ButtonOk.IsEnabled = true;
        }

        private void RadioButtonProcessPriority_OnChecked(object sender, RoutedEventArgs e)
        {
            _generalChanged = true;
            ButtonOk.IsEnabled = true;
        }

        private void CheckBoxDebug_OnChecked(object sender, RoutedEventArgs e)
        {
            _generalChanged = true;
            ButtonOk.IsEnabled = true;
        }

        private void RadioButtonAPI_OnChecked(object sender, RoutedEventArgs e)
        {
            _generalChanged = true;
            ButtonOk.IsEnabled = true;
        }

        private void CheckBoxDebug_OnUnchecked(object sender, RoutedEventArgs e)
        {
            _generalChanged = true;
            ButtonOk.IsEnabled = true;
        }

        private void SettingsWindow_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (!ButtonOk.IsEnabled && e.Key == Key.Escape)
            {
                DialogResult = false;
                e.Handled = true;
                Close();
            }
        }

        private void CheckBoxMinimizeToTray_OnChecked(object sender, RoutedEventArgs e)
        {
            _generalChanged = true;
            ButtonOk.IsEnabled = true;
        }

        private void CheckBoxMinimizeToTray_OnUnchecked(object sender, RoutedEventArgs e)
        {
            _generalChanged = true;
            ButtonOk.IsEnabled = true;
        }
    }
}
