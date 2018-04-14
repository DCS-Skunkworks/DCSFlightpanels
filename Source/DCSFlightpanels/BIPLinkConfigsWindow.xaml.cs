using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using ClassLibraryCommon;
using DCS_BIOS;
using NonVisuals;

namespace DCSFlightpanels
{
    /// <summary>
    /// Interaction logic for BIPLinkConfigsWindow.xaml
    /// </summary>
    public partial class BIPLinkConfigsWindow : Window
    {
        private List<BIPLight> _bipLights = new List<BIPLight>();
        private string _header;
        private string _description;

        public BIPLinkConfigsWindow(string header, string description)
        {
            InitializeComponent();
            _header = header;
            _description = description;
            TextBoxDescription.Text = _description;
            ShowItems();
        }

        public BIPLinkConfigsWindow(string header, List<BIPLight> bipLights, string description)
        {
            InitializeComponent();
            if (bipLights != null)
            {
                _bipLights = bipLights;
            }
            _header = header;
            _description = description;
            TextBoxDescription.Text = _description;
            ShowItems();
        }

        private void BIPLinkConfigsWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            TextBoxHeader.Text = _header;
        }

        private void SetFormState()
        {
            EditButton.IsEnabled = DataGridValues.Items.Count > 0 && DataGridValues.SelectedItem != null;
            DeleteButton.IsEnabled = DataGridValues.Items.Count > 0 && DataGridValues.SelectedItem != null;
        }

        private void ShowItems()
        {
            try
            {
                DataGridValues.DataContext = _bipLights;
                DataGridValues.ItemsSource = _bipLights;
                DataGridValues.Items.Refresh();
                SetFormState();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(1013, ex);
            }
        }

        public List<BIPLight> BIPLights
        {
            get { return _bipLights; }
            set { _bipLights = value; }
        }

        private void OkButton_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                DialogResult = true;
                _description = TextBoxDescription.Text;
                Close();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(2010, ex);
            }
        }

        private void CancelButton_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                DialogResult = false;
                Close();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(2034, ex);
            }
        }

        private void DeleteButton_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (MessageBox.Show("Do you want to delete the selected hook?", "Delete hook?", MessageBoxButton.OKCancel, MessageBoxImage.Question) != MessageBoxResult.OK)
                {
                    return;
                }
                var bipLight = (BIPLight)DataGridValues.SelectedItem;
                _bipLights.Remove(bipLight);
                ShowItems();
                SetFormState();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(1090, ex);
            }
        }

        private void EditButton_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var bipLight = (BIPLight)DataGridValues.SelectedItem;
                var dcsBiosInputWindow = new DCSBiosInputWindow(_dcsAirframe, _header, bipLight);
                if (dcsBiosInputWindow.ShowDialog() == true)
                {
                    _bipLights.Remove(bipLight);
                    var tmpdcsBiosInput = dcsBiosInputWindow.DCSBiosInput;
                    _bipLights.Add(tmpdcsBiosInput);
                    ShowItems();
                }
                ShowItems();
                SetFormState();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(1072, ex);
            }
        }

        private void NewControlButton_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                DCSBiosInputWindow dcsBiosInputWindow;
                dcsBiosInputWindow = new DCSBiosInputWindow();
                dcsBiosInputWindow.ShowDialog();
                if (dcsBiosInputWindow.DialogResult.HasValue && dcsBiosInputWindow.DialogResult.Value)
                {
                    var dcsBiosInput = dcsBiosInputWindow.DCSBiosInput;
                    //1 appropriate text to textbox
                    //2 update bindings
                    _bipLights.Add(dcsBiosInput);
                }
                SetFormState();
                ShowItems();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(1016, ex);
            }
        }

        private void DataGridValues_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                SetFormState();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(2012, ex);
            }
        }

        public string Description
        {
            get { return _description; }
            set { _description = value; }
        }

        private void NewDelayButton_OnClick(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}
