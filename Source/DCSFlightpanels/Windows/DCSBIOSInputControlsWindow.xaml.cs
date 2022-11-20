namespace DCSFlightpanels.Windows
{
    using System;
    using System.Collections.Generic;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using ClassLibraryCommon;
    using DCS_BIOS;
    using NonVisuals;
    using NonVisuals.Interfaces;

    /// <summary>
    /// Interaction logic for DCSBIOSInputControlsWindow.xaml
    /// </summary>
    public partial class DCSBIOSInputControlsWindow : IIsDirty
    {
        private readonly string _header;
        private List<DCSBIOSInput> _dcsbiosInputs = new();
        private string _description;
        private bool _isDirty;
        private readonly bool _showSequenced;
        private bool _isSequenced;

        public DCSBIOSInputControlsWindow(string header, string description, bool isSequenced, bool showSequenced)
        {
            InitializeComponent();
            _header = header;
            _description = description;
            TextBoxDescription.Text = _description;
            IsSequenced = isSequenced;
            _showSequenced = showSequenced;
            ShowItems();
        }

        public DCSBIOSInputControlsWindow(string header, List<DCSBIOSInput> dcsbiosInputs, string description, bool isSequenced, bool showSequenced)
        {
            InitializeComponent();
            if (dcsbiosInputs != null)
            {
                _dcsbiosInputs = dcsbiosInputs;
            }
            _header = header;
            _description = description;
            TextBoxDescription.Text = _description;
            IsSequenced = isSequenced;
            _showSequenced = showSequenced;

            ShowItems();
        }

        private void DCSBIOSInputControlsWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            TextBoxHeader.Text = _header + Environment.NewLine + ProfileHandler.ActiveDCSFPProfile.Description;
            SetFormState();
        }

        private void SetFormState()
        {
            EditButton.IsEnabled = DataGridValues.Items.Count > 0 && DataGridValues.SelectedItem != null;
            DeleteButton.IsEnabled = DataGridValues.Items.Count > 0 && DataGridValues.SelectedItem != null;
            CheckBoxIsSequenced.Visibility = _showSequenced ? Visibility.Visible : Visibility.Collapsed;
            LabelSequencedInfo.Visibility = _showSequenced ? Visibility.Visible : Visibility.Collapsed;
            DuplicateButton.IsEnabled = DataGridValues.Items.Count > 0 && DataGridValues.SelectedItem != null;
            UpButton.IsEnabled = DataGridValues.Items.Count > 0 && DataGridValues.SelectedItem != null && DataGridValues.SelectedIndex > 0;
            DownButton.IsEnabled = DataGridValues.Items.Count > 0 && DataGridValues.SelectedItem != null && DataGridValues.SelectedIndex < DataGridValues.Items.Count - 1;
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
                Common.ShowErrorMessageBox(ex);
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
                Common.ShowErrorMessageBox(ex);
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
                Common.ShowErrorMessageBox(ex);
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
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void EditButton_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                EditControl();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void EditControl()
        {
            var dcsBIOSInput = (DCSBIOSInput)DataGridValues.SelectedItem;
            var dcsBiosInputWindow = new DCSBiosInputWindow(_header, dcsBIOSInput);
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

        private void NewControlButton_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                AddNewControl();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void AddNewControl()
        {
            var dcsBiosInputWindow = new DCSBiosInputWindow(string.Empty);
            dcsBiosInputWindow.ShowDialog();
            if (dcsBiosInputWindow.DialogResult.HasValue && dcsBiosInputWindow.DialogResult.Value)
            {
                var dcsBiosInput = dcsBiosInputWindow.DCSBiosInput;
                // 1 appropriate text to textbox
                // 2 update bindings
                _dcsbiosInputs.Add(dcsBiosInput);
                SetIsDirty();
            }

            SetFormState();
            ShowItems();
        }

        private void DataGridValues_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                SetFormState();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
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
                Common.ShowErrorMessageBox(ex);
            }
        }

        public bool IsSequenced
        {
            get => _isSequenced;
            init
            {
                CheckBoxIsSequenced.IsChecked = value;
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
                Common.ShowErrorMessageBox(ex);
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
                Common.ShowErrorMessageBox(ex);
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
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void DataGridValues_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (DataGridValues.Items.Count == 0 || DataGridValues.SelectedItems.Count == 0)
                {
                    AddNewControl();
                }
                else if (DataGridValues.SelectedItems.Count == 1)
                {
                    EditControl();
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void DataGridValues_OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.OriginalSource is ScrollViewer)
            {
                // Unselect when user presses any area not containing a row
                ((DataGrid)sender).UnselectAll();
            }
        }

        private void DuplicateButton_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var dcsBIOSInput = DCSBIOSInputs[DataGridValues.SelectedIndex];
                var newDcsBIOSInput = dcsBIOSInput.CloneJson();
                DCSBIOSInputs.Add(newDcsBIOSInput);
                ShowItems();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void UpButton_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var oldIndex = DataGridValues.SelectedIndex;
                var newIndex = DataGridValues.SelectedIndex - 1;
                var dcsInputToMove = DCSBIOSInputs[DataGridValues.SelectedIndex];
                var dcsInputToReplace = DCSBIOSInputs[DataGridValues.SelectedIndex - 1];

                DCSBIOSInputs[oldIndex] = dcsInputToReplace;
                DCSBIOSInputs[newIndex] = dcsInputToMove;

                ShowItems();
                SetIsDirty();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void DownButton_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var oldIndex = DataGridValues.SelectedIndex;
                var newIndex = DataGridValues.SelectedIndex + 1;
                var dcsInputToMove = DCSBIOSInputs[DataGridValues.SelectedIndex];
                var dcsInputToReplace = DCSBIOSInputs[DataGridValues.SelectedIndex + 1];

                DCSBIOSInputs[oldIndex] = dcsInputToReplace;
                DCSBIOSInputs[newIndex] = dcsInputToMove;

                ShowItems();
                SetIsDirty();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }
    }
}
