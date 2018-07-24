﻿using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using ClassLibraryCommon;
using DCSFlightpanels.Properties;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using MessageBox = System.Windows.MessageBox;

namespace DCSFlightpanels
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

        private DCSAirframe _dcsAirframe;

        public SettingsWindow(DCSAirframe dcsAirframe)
        {
            InitializeComponent();
            _dcsAirframe = dcsAirframe;
        }

        private void SettingsWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                ButtonOk.IsEnabled = false;
                LoadSettings();
                if (_dcsAirframe == DCSAirframe.KEYEMULATOR)
                {
                    LabelDCSBIOS.Visibility = Visibility.Collapsed;
                    LabelSRS.Visibility = Visibility.Collapsed;
                }else if (_dcsAirframe == DCSAirframe.KEYEMULATOR_SRS)
                {
                    LabelDCSBIOS.Visibility = Visibility.Collapsed;
                }
                else
                {
                    //DCS-BIOS Airframe
                    LabelSRS.Visibility = Visibility.Collapsed;
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
            TextBoxDCSBIOSIPFrom.TextChanged += TextBoxDcsBios_OnTextChanged;
            TextBoxDCSBIOSIPTo.TextChanged += TextBoxDcsBios_OnTextChanged;
            TextBoxDCSBIOSPortFrom.TextChanged += TextBoxDcsBios_OnTextChanged;
            TextBoxDCSBIOSPortTo.TextChanged += TextBoxDcsBios_OnTextChanged;
            TextBoxSRSIPFrom.TextChanged += TextBoxSRS_OnTextChanged;
            TextBoxSRSIPTo.TextChanged += TextBoxSRS_OnTextChanged;
            TextBoxSRSPortFrom.TextChanged += TextBoxSRS_OnTextChanged;
            TextBoxSRSPortTo.TextChanged += TextBoxSRS_OnTextChanged;
            CheckBoxDoDebug.Checked += CheckBoxDebug_OnChecked;
            CheckBoxDebugToFile.Checked += CheckBoxDebug_OnChecked;
        }

        private void LoadSettings()
        {
            switch(Settings.Default.ProcessPriority)
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
            
            CheckBoxDoDebug.IsChecked = Settings.Default.DebugOn;
            CheckBoxDebugToFile.IsChecked = Settings.Default.DebugToFile;

            if (_dcsAirframe != DCSAirframe.KEYEMULATOR)
            {
                TextBoxDcsBiosJSONLocation.Text = Settings.Default.DCSBiosJSONLocation;
                TextBoxDCSBIOSIPFrom.Text = Settings.Default.DCSBiosIPFrom;
                TextBoxDCSBIOSIPTo.Text = Settings.Default.DCSBiosIPTo;
                TextBoxDCSBIOSPortFrom.Text = Settings.Default.DCSBiosPortFrom;
                TextBoxDCSBIOSPortTo.Text = Settings.Default.DCSBiosPortTo;
            }
            if (_dcsAirframe == DCSAirframe.KEYEMULATOR_SRS)
            {
                TextBoxSRSIPFrom.Text = Settings.Default.SRSIpFrom;
                TextBoxSRSIPTo.Text = Settings.Default.SRSIpTo;
                TextBoxSRSPortFrom.Text = Settings.Default.SRSPortFrom.ToString();
                TextBoxSRSPortTo.Text = Settings.Default.SRSPortTo.ToString();
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

                    Settings.Default.DebugOn = CheckBoxDoDebug.IsChecked == true;
                    Settings.Default.DebugToFile = CheckBoxDebugToFile.IsChecked == true;
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
                    folderBrowserDialog.SelectedPath = DCS_BIOS.DBCommon.GetDCSBIOSJSONDirectory(Settings.Default.DCSBiosJSONLocation);
                }
                if (folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    TextBoxDcsBiosJSONLocation.Text = folderBrowserDialog.SelectedPath;
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(1011, ex);
            }
        }

        private void CheckValuesDCSBIOS()
        {
            try
            {
                IPAddress ipAddress;
                if (string.IsNullOrEmpty(TextBoxDCSBIOSIPFrom.Text))
                {
                    throw new Exception("DCS-BIOS IP address from cannot be empty");
                }
                if (string.IsNullOrEmpty(TextBoxDCSBIOSIPTo.Text))
                {
                    throw new Exception("DCS-BIOS IP address to cannot be empty");
                }
                if (string.IsNullOrEmpty(TextBoxDCSBIOSPortFrom.Text))
                {
                    throw new Exception("DCS-BIOS Port from cannot be empty");
                }
                if (string.IsNullOrEmpty(TextBoxDCSBIOSPortTo.Text))
                {
                    throw new Exception("DCS-BIOS Port to cannot be empty");
                }
                if (string.IsNullOrEmpty(TextBoxDcsBiosJSONLocation.Text))
                {
                    throw new Exception("DCS-BIOS JSON directory cannot be empty");
                }
                try
                {
                    if (!IPAddress.TryParse(TextBoxDCSBIOSIPFrom.Text, out ipAddress))
                    {
                        throw new Exception();
                    }
                    _ipAddressFromDCSBIOS = TextBoxDCSBIOSIPFrom.Text;
                }
                catch (Exception e)
                {
                    throw new Exception("DCS-BIOS Error while checking IP from : " + e.Message);
                }
                try
                {
                    if (!IPAddress.TryParse(TextBoxDCSBIOSIPTo.Text, out ipAddress))
                    {
                        throw new Exception();
                    }
                    _ipAddressToDCSBIOS = TextBoxDCSBIOSIPTo.Text;
                }
                catch (Exception e)
                {
                    throw new Exception("DCS-BIOS Error while checking IP to : " + e.Message);
                }
                try
                {
                    var test = Convert.ToInt32(TextBoxDCSBIOSPortFrom.Text);
                    _portFromDCSBIOS = TextBoxDCSBIOSPortFrom.Text;
                }
                catch (Exception e)
                {
                    throw new Exception("DCS-BIOS Error while Port from : " + e.Message);
                }
                try
                {
                    var test = Convert.ToInt32(TextBoxDCSBIOSPortFrom.Text);
                    _portToDCSBIOS = TextBoxDCSBIOSPortTo.Text;
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
                if (string.IsNullOrEmpty(TextBoxSRSIPFrom.Text))
                {
                    throw new Exception("SRS IP address from cannot be empty");
                }
                if (string.IsNullOrEmpty(TextBoxSRSIPTo.Text))
                {
                    throw new Exception("SRS IP address to cannot be empty");
                }
                if (string.IsNullOrEmpty(TextBoxSRSPortFrom.Text))
                {
                    throw new Exception("SRS Port from cannot be empty");
                }
                if (string.IsNullOrEmpty(TextBoxSRSPortTo.Text))
                {
                    throw new Exception("SRS Port to cannot be empty");
                }
                try
                {
                    if (!IPAddress.TryParse(TextBoxSRSIPFrom.Text, out ipAddress))
                    {
                        throw new Exception();
                    }
                    _ipAddressFromSRS = TextBoxSRSIPFrom.Text;
                }
                catch (Exception e)
                {
                    throw new Exception("SRS Error while checking IP from : " + e.Message);
                }
                try
                {
                    if (!IPAddress.TryParse(TextBoxSRSIPTo.Text, out ipAddress))
                    {
                        throw new Exception();
                    }
                    _ipAddressToSRS = TextBoxSRSIPTo.Text;
                }
                catch (Exception e)
                {
                    throw new Exception("SRS Error while checking IP to : " + e.Message);
                }
                try
                {
                    var test = Convert.ToInt32(TextBoxSRSPortFrom.Text);
                    _portFromSRS = TextBoxSRSPortFrom.Text;
                }
                catch (Exception e)
                {
                    throw new Exception("SRS Error while Port from : " + e.Message);
                }
                try
                {
                    var test = Convert.ToInt32(TextBoxSRSPortFrom.Text);
                    _portToSRS = TextBoxSRSPortTo.Text;
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
    }
}
