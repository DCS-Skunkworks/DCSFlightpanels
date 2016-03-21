using System;
using System.IO;
using System.Net;
using System.Windows;
using System.Windows.Forms;
using NonVisuals;
using ProUsbPanels.Properties;

namespace ProUsbPanels
{
    /// <summary>
    /// Interaction logic for DcsBiosWindow.xaml
    /// </summary>
    public partial class DcsBiosWindow : Window
    {
        private string _ipAddressFrom;
        private string _portFrom;
        private string _ipAddressTo;
        private string _portTo;
        private string _dcsBiosJSONLocation;


        public DcsBiosWindow()
        {
            InitializeComponent();
        }

        public DcsBiosWindow(string addressFrom, string portFrom, string addressTo, string portTo, string dcsBiosJSONLocation)
        {
            InitializeComponent();
            TextBoxIPFrom.Text = addressFrom;
            TextBoxPortFrom.Text = portFrom;
            TextBoxIPTo.Text = addressTo;
            TextBoxPortTo.Text = portTo;
            TextBoxDcsBiosJSONLocation.Text = dcsBiosJSONLocation;
        }

        private void ButtonOkClick(object sender, RoutedEventArgs e)
        {
            try
            {
                CheckValues();
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(1009, ex);
            }
        }

        private void ButtonCancelClick(object sender, RoutedEventArgs e)
        {
            try
            {
                DialogResult = false;
                Close();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(1010, ex);
            }
        }

        private void CheckValues()
        {
            try
            {
                IPAddress ipAddress;
                if (string.IsNullOrEmpty(TextBoxIPFrom.Text))
                {
                    throw new Exception("IP address from cannot be empty");
                }
                if (string.IsNullOrEmpty(TextBoxIPTo.Text))
                {
                    throw new Exception("IP address to cannot be empty");
                }
                if (string.IsNullOrEmpty(TextBoxPortFrom.Text))
                {
                    throw new Exception("Port from cannot be empty");
                }
                if (string.IsNullOrEmpty(TextBoxPortTo.Text))
                {
                    throw new Exception("Port to cannot be empty");
                }
                if (string.IsNullOrEmpty(TextBoxDcsBiosJSONLocation.Text))
                {
                    throw new Exception("DCS-BIOS JSON directory cannot be empty");
                }
                try
                {
                    if (!IPAddress.TryParse(TextBoxIPFrom.Text, out ipAddress))
                    {
                        throw new Exception();
                    }
                    _ipAddressFrom = TextBoxIPFrom.Text;
                }
                catch (Exception e)
                {
                    throw new Exception("Error while checking IP from : " + e.Message);
                }
                try
                {
                    if (!IPAddress.TryParse(TextBoxIPTo.Text, out ipAddress))
                    {
                        throw new Exception();
                    }
                    _ipAddressTo = TextBoxIPTo.Text;
                }
                catch (Exception e)
                {
                    throw new Exception("Error while checking IP to : " + e.Message);
                }
                try
                {
                    var test = Convert.ToInt32(TextBoxPortFrom.Text);
                    _portFrom = TextBoxPortFrom.Text;
                }
                catch (Exception e)
                {
                    throw new Exception("Error while Port from : " + e.Message);
                }
                try
                {
                    var test = Convert.ToInt32(TextBoxPortFrom.Text);
                    _portTo = TextBoxPortTo.Text;
                }
                catch (Exception e)
                {
                    throw new Exception("Error while Port to : " + e.Message);
                }
                try
                {
                    var directoryInfo = new DirectoryInfo(TextBoxDcsBiosJSONLocation.Text);
                    _dcsBiosJSONLocation = TextBoxDcsBiosJSONLocation.Text;
                }
                catch (Exception e)
                {
                    throw new Exception("Error while checking DCS-BIOS location : " + e.Message);
                }
            }
            catch (Exception e)
            {
                throw new Exception("Error checking values : " + Environment.NewLine + e.Message);
            }
        }

        public string IPAddressFrom
        {
            get { return _ipAddressFrom; }
        }

        public string PortFrom
        {
            get { return _portFrom; }
        }

        public string IPAddressTo
        {
            get { return _ipAddressTo; }
        }

        public string PortTo
        {
            get { return _portTo; }
        }

        public string DCSBiosJSONLocation
        {
            get { return _dcsBiosJSONLocation; }
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
                    Settings.Default.DCSBiosJSONLocation = folderBrowserDialog.SelectedPath;
                    Settings.Default.Save();
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(1011, ex);
            }
        }

        private void DcsBiosWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(1012, ex);
            }
        }
    }
}
