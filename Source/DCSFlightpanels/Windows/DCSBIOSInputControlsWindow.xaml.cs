using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ClassLibraryCommon;
using DCS_BIOS;
using NonVisuals.Interfaces;

namespace DCSFlightpanels.Windows
{
    /// <summary>
    /// Interaction logic for DCSBIOSInputControlsWindow.xaml
    /// </summary>
    public partial class DCSBIOSInputControlsWindow : Window, IIsDirty
    {
        private List<DCSBIOSInput> _dcsbiosInputs = new List<DCSBIOSInput>();
        private readonly string _header;
        private string _description;
        private readonly DCSAirframe _dcsAirframe = DCSAirframe.A10C;
        private bool _isDirty = false;
        private bool _showSequenced = false;
        private bool _isSequenced = false;

        public DCSBIOSInputControlsWindow(DCSAirframe dcsAirframe, string header, string description, bool showSequenced = false)
        {
            InitializeComponent();
            _dcsAirframe = dcsAirframe;
            _header = header;
            _description = description;
            TextBoxDescription.Text = _description;
            _showSequenced = showSequenced;
            ShowItems();
        }

        public DCSBIOSInputControlsWindow(DCSAirframe dcsAirframe, string header, List<DCSBIOSInput> dcsbiosInputs, string description, bool showSequenced = false)
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
            _showSequenced = showSequenced;
            
            ShowItems();
        }

        private void DCSBIOSInputControlsWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            TextBoxHeader.Text = _header;
            SetFormState();
        }

        private void SetFormState()
        {
            EditButton.IsEnabled = DataGridValues.Items.Count > 0 && DataGridValues.SelectedItem != null;
            DeleteButton.IsEnabled = DataGridValues.Items.Count > 0 && DataGridValues.SelectedItem != null;
            CheckBoxIsSequenced.Visibility = _showSequenced ? Visibility.Visible : Visibility.Collapsed;
            LabelSequencedInfo.Visibility = _showSequenced ? Visibility.Visible : Visibility.Collapsed;
            ButtonOk.IsEnabled = _isDirty;
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
                Common.ShowErrorMessageBox( ex);
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
                Common.ShowErrorMessageBox( ex);
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
                Common.ShowErrorMessageBox( ex);
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
                SetIsDirty();
                ShowItems();
                SetFormState();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox( ex);
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
                    SetIsDirty();
                    ShowItems();
                }
                ShowItems();
                SetFormState();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox( ex);
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
                    SetIsDirty();
                }
                SetFormState();
                ShowItems();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox( ex);
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
                Common.ShowErrorMessageBox( ex);
            }
        }

        public string Description
        {
            get { return _description; }
            set { _description = value; }
        }


        private void DCSBIOSInputControlsWindow_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (!ButtonOk.IsEnabled && e.Key == Key.Escape)
            {
                DialogResult = false;
                e.Handled = true;
                Close();
            }
        }

        private void TextBoxDescription_OnKeyDown(object sender, KeyEventArgs e)
        {
            SetIsDirty();
            SetFormState();
        }

        public void SetIsDirty()
        {
            _isDirty = true;
        }

        public bool IsDirty
        {
            get => _isDirty;
            set => _isDirty = value;
        }

        public void StateSaved()
        {
            _isDirty = false;
        }

        private void CheckBoxIsSequenced_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                _isSequenced = CheckBoxIsSequenced.IsChecked == true;
                SetIsDirty();
                SetFormState();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox( ex);
            }
        }

        public bool IsSequenced
        {
            get => _isSequenced;
            set
            {
                if (value)
                {
                    CheckBoxIsSequenced.IsChecked = true;
                }
                _isSequenced = value;
            }
        }

        private void LabelSequencedInfo_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                var text = "Sequenced means that for each press of the button one of these DCS-BIOS commands are sent sequentially. \n\n" +
                           "First press first DCS-BIOS command, second press second DCS-BIOS command and so  on.\nUntil it starts over.";
                MessageBox.Show(text, "Sequenced execution", MessageBoxButton.OK, MessageBoxImage.Information);
                SetFormState();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox( ex);
            }
        }

        private void LabelSequencedInfo_OnMouseEnter(object sender, MouseEventArgs e)
        {
            try
            {
                Mouse.OverrideCursor = Cursors.Hand;
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox( ex);
            }
        }

        private void LabelSequencedInfo_OnMouseLeave(object sender, MouseEventArgs e)
        {
            try
            {
                Mouse.OverrideCursor = Cursors.Arrow;
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox( ex);
            }
        }
    }
}
