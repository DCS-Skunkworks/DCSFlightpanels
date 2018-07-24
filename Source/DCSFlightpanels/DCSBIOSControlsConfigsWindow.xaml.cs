using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ClassLibraryCommon;
using DCS_BIOS;

namespace DCSFlightpanels
{
    /// <summary>
    /// Interaction logic for DCSBIOSControlsConfigsWindow.xaml
    /// </summary>
    public partial class DCSBIOSControlsConfigsWindow : Window
    {
        private List<DCSBIOSInput> _dcsbiosInputs = new List<DCSBIOSInput>();
        private string _header;
        private string _description;
        private DCSAirframe _dcsAirframe = DCSAirframe.A10C;

        public DCSBIOSControlsConfigsWindow(DCSAirframe dcsAirframe, string header, string description)
        {
            InitializeComponent();
            _dcsAirframe = dcsAirframe;
            _header = header;
            _description = description;
            TextBoxDescription.Text = _description;
            ShowItems();
        }

        public DCSBIOSControlsConfigsWindow(DCSAirframe dcsAirframe, string header, List<DCSBIOSInput> dcsbiosInputs, string description)
        {
            InitializeComponent();
            _dcsAirframe = dcsAirframe;
            if (dcsbiosInputs != null)
            {
                _dcsbiosInputs = dcsbiosInputs;
            }
            _header = header;
            _description = description;
            TextBoxDescription.Text = _description;
            ShowItems();
        }

        private void DCSBIOSControlsConfigsWindow_OnLoaded(object sender, RoutedEventArgs e)
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
                DataGridValues.DataContext = _dcsbiosInputs;
                DataGridValues.ItemsSource = _dcsbiosInputs;
                DataGridValues.Items.Refresh();
                SetFormState();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(1013, ex);
            }
        }

        public List<DCSBIOSInput> DCSBIOSInputs
        {
            get { return _dcsbiosInputs; }
            set { _dcsbiosInputs = value; }
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
                var dcsBIOSInput = (DCSBIOSInput)DataGridValues.SelectedItem;
                _dcsbiosInputs.Remove(dcsBIOSInput);
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
                var dcsBIOSInput = (DCSBIOSInput)DataGridValues.SelectedItem;
                var dcsBiosInputWindow = new DCSBiosInputWindow(_dcsAirframe, _header, dcsBIOSInput);
                if (dcsBiosInputWindow.ShowDialog() == true)
                {
                    _dcsbiosInputs.Remove(dcsBIOSInput);
                    var tmpdcsBiosInput = dcsBiosInputWindow.DCSBiosInput;
                    _dcsbiosInputs.Add(tmpdcsBiosInput);
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
                    _dcsbiosInputs.Add(dcsBiosInput);
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

        private void DCSBIOSControlsConfigsWindow_OnKeyDown(object sender, KeyEventArgs e)
        {

            if (!ButtonOk.IsEnabled)
            {
                DialogResult = false;
                Close();
            }
        }
    }
}
