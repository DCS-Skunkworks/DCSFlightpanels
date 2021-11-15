namespace DCSFlightpanels.Windows
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;

    using ClassLibraryCommon;

    using MEF;

    using NonVisuals;
    using NonVisuals.Interfaces;

    /// <summary>
    /// Interaction logic for SequenceWindow.xaml
    /// </summary>
    public partial class KeySequenceWindow : Window, IIsDirty
    {
        private readonly SortedList<int, IKeyPressInfo> _sortedList = new SortedList<int, IKeyPressInfo>();
        private bool _isDirty;
        private bool _formLoaded = false;
        private bool _supportIndefinite;

        public KeySequenceWindow(bool supportIndefinite = true)
        {
            InitializeComponent();
            SetFormState();
            _supportIndefinite = supportIndefinite;
        }

        public KeySequenceWindow(string description, SortedList<int, IKeyPressInfo> sortedList, bool supportIndefinite = true)
        {
            InitializeComponent();
            _sortedList = sortedList;
            TextBoxDescription.Text = description;
            DataGridSequences.DataContext = _sortedList;
            DataGridSequences.ItemsSource = _sortedList;
            DataGridSequences.Items.Refresh();
            _supportIndefinite = supportIndefinite;

            SetFormState();
        }

        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                _formLoaded = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public bool IsDirty
        {
            get { return _isDirty; }
        }

        public void SetIsDirty()
        {
            _isDirty = true;
        }

        public void StateSaved()
        {
            _isDirty = false;
        }

        public SortedList<int, IKeyPressInfo> KeySequence
        {
            get { return _sortedList; }
        }

        public string Description
        {
            get { return TextBoxDescription.Text; }
        }

        private void SetFormState()
        {
            ButtonUp.IsEnabled = DataGridSequences.SelectedItems.Count == 1 && DataGridSequences.SelectedIndex > 0;
            ButtonDown.IsEnabled = DataGridSequences.SelectedItems.Count == 1 && DataGridSequences.SelectedIndex < DataGridSequences.Items.Count - 1;
            ButtonAdd.IsEnabled = true;
            ButtonEdit.IsEnabled = DataGridSequences.SelectedItems.Count == 1;
            ButtonDelete.IsEnabled = DataGridSequences.SelectedItems.Count > 0;
            ButtonOk.IsEnabled = IsDirty;
        }

        private void ButtonAddClick(object sender, RoutedEventArgs e)
        {
            try
            {
                AddKeyPress();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox( ex);
            }
        }

        private void AddKeyPress()
        {
            var keyPressWindow = new KeyPressWindow(_supportIndefinite);
            keyPressWindow.ShowDialog();
            if (keyPressWindow.DialogResult.HasValue && keyPressWindow.DialogResult.Value)
            {
                // Clicked OK
                var keyPressInfo = new KeyPressInfo();
                keyPressInfo.LengthOfBreak = (KeyPressLength)keyPressWindow.ComboBoxBreak.SelectedItem;
                keyPressInfo.VirtualKeyCodes = KeyPress.SplitStringKeyCodes(keyPressWindow.TextBoxKeyPress.Text);
                keyPressInfo.LengthOfKeyPress = (KeyPressLength)keyPressWindow.ComboBoxKeyPressTime.SelectedItem;
                _sortedList.Add(GetNewKeyValue(), keyPressInfo);

                DataGridSequences.DataContext = _sortedList;
                DataGridSequences.ItemsSource = _sortedList;
                DataGridSequences.Items.Refresh();
                SetIsDirty();
                SetFormState();
            }
        }

        private int GetNewKeyValue()
        {
            if (_sortedList.Count == 0)
            {
                return 0;
            }

            return _sortedList.Keys.Max() + 1;
        }

        private void ButtonDeleteClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var value = (KeyValuePair<int, IKeyPressInfo>)DataGridSequences.SelectedItem;
                _sortedList.Remove(value.Key);
                DataGridSequences.Items.Refresh();
                SetIsDirty();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox( ex);
            }
        }

        private void ButtonEditClick(object sender, RoutedEventArgs e)
        {
            try
            {
                EditKeyPress();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox( ex);
            }
        }

        private void EditKeyPress()
        {
            var keyValuePair = (KeyValuePair<int, IKeyPressInfo>)DataGridSequences.SelectedItem;
            var keyPressWindow = new KeyPressWindow((KeyPressInfo)keyValuePair.Value);
            keyPressWindow.ShowDialog();
            if (keyPressWindow.DialogResult.HasValue && keyPressWindow.DialogResult.Value)
            {
                //Clicked OK
                if (!keyPressWindow.IsDirty)
                {
                    //User made no changes
                    return;
                }

                _sortedList[keyValuePair.Key].LengthOfBreak = (KeyPressLength)keyPressWindow.ComboBoxBreak.SelectedItem;
                _sortedList[keyValuePair.Key].VirtualKeyCodes = KeyPress.SplitStringKeyCodes(keyPressWindow.TextBoxKeyPress.Text);
                _sortedList[keyValuePair.Key].LengthOfKeyPress = (KeyPressLength)keyPressWindow.ComboBoxKeyPressTime.SelectedItem;
                DataGridSequences.DataContext = _sortedList;
                DataGridSequences.ItemsSource = _sortedList;
                DataGridSequences.Items.Refresh();
                SetIsDirty();
            }
        }

        private void ButtonUpClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var value = (KeyValuePair<int, IKeyPressInfo>)DataGridSequences.SelectedItem;
                MoveItemUp(value.Key);
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox( ex);
            }
        }

        private void ButtonDownClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var value = (KeyValuePair<int, IKeyPressInfo>)DataGridSequences.SelectedItem;
                MoveItemDown(value.Key);
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox( ex);
            }
        }

        private void ButtonOkClick(object sender, RoutedEventArgs e)
        {
            try
            {
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox( ex);
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
                Common.ShowErrorMessageBox( ex);
            }
        }

        private void DataGridSequencesSelectionChanged(object sender, SelectionChangedEventArgs e)
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

        private void MoveItemDown(int key)
        {
            //Moved down means index increases, i.e. moved down further in the list
            var itemToBeMovedDown = _sortedList[key];
            var itemToBeMovedUp = _sortedList[key + 1];
            _sortedList.Remove(key);
            _sortedList.Remove(key + 1);
            _sortedList.Add(key + 1, itemToBeMovedDown);
            _sortedList.Add(key, itemToBeMovedUp);
            DataGridSequences.Items.Refresh();
            SetIsDirty();
        }

        private void MoveItemUp(int key)
        {
            //Moved down means index decreases, i.e. moved down further in the list
            var itemToBeMovedUp = _sortedList[key];
            var itemToBeMovedDown = _sortedList[key - 1];
            _sortedList.Remove(key);
            _sortedList.Remove(key - 1);
            _sortedList.Add(key - 1, itemToBeMovedUp);
            _sortedList.Add(key, itemToBeMovedDown);
            DataGridSequences.Items.Refresh();
            SetIsDirty();
        }

        private void TextBoxDescriptionTextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (!_formLoaded)
                {
                    return;
                }

                SetIsDirty();
                SetFormState();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox( ex);
            }
        }

        private void SequenceWindow_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (!ButtonOk.IsEnabled && e.Key == Key.Escape)
            {
                DialogResult = false;
                e.Handled = true;
                Close();
            }
        }

        private void DataGridSequences_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (DataGridSequences.Items.Count == 0 || DataGridSequences.SelectedItems.Count == 0)
                {
                    AddKeyPress();
                }
                else if (DataGridSequences.SelectedItems.Count == 1)
                {
                    EditKeyPress();
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void DataGridSequences_OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.OriginalSource is ScrollViewer)
            {
                //Unselect when user presses any area not containing a row
                ((DataGrid)sender).UnselectAll();
            }
        }
    }
}
