using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ClassLibraryCommon;
using NonVisuals;

namespace DCSFlightpanels
{
    /// <summary>
    /// Interaction logic for SequenceWindow.xaml
    /// </summary>
    public partial class KeySequenceWindow : Window
    {

        private readonly SortedList<int, KeyPressInfo> _sortedList = new SortedList<int, KeyPressInfo>();
        private bool _isDirty;

        public KeySequenceWindow()
        {
            InitializeComponent();
            SetFormState();
        }

        public KeySequenceWindow(string information, SortedList<int, KeyPressInfo> sortedList)
        {
            InitializeComponent();
            _sortedList = sortedList;
            TextBoxInformation.Text = information;
            DataGridSequences.DataContext = _sortedList;
            DataGridSequences.ItemsSource = _sortedList;
            DataGridSequences.Items.Refresh();

            SetFormState();
        }

        public bool IsDirty
        {
            get { return _isDirty; }
        }

        public SortedList<int, KeyPressInfo> GetSequence
        {
            get { return _sortedList; }
        }

        public string GetInformation
        {
            get { return TextBoxInformation.Text; }
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

        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void ButtonAddClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var keyPressWindow = new KeyPressWindow();
                keyPressWindow.ShowDialog();
                if (keyPressWindow.DialogResult.HasValue && keyPressWindow.DialogResult.Value)
                {
                    //Clicked OK
                    var keyPressInfo = new KeyPressInfo();
                    keyPressInfo.LengthOfBreak = (KeyPressLength)keyPressWindow.ComboBoxBreak.SelectedItem;
                    keyPressInfo.VirtualKeyCodes = KeyPress.SplitStringKeyCodes(keyPressWindow.TextBoxKeyPress.Text);
                    keyPressInfo.LengthOfKeyPress = (KeyPressLength)keyPressWindow.ComboBoxKeyPressTime.SelectedItem;
                    _sortedList.Add(GetNewKeyValue(), keyPressInfo);

                    DataGridSequences.DataContext = _sortedList;
                    DataGridSequences.ItemsSource = _sortedList;
                    DataGridSequences.Items.Refresh();
                    _isDirty = true;
                    SetFormState();
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(1065, ex);
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
                var value = (KeyValuePair<int, KeyPressInfo>)DataGridSequences.SelectedItem;
                _sortedList.Remove(value.Key);
                DataGridSequences.Items.Refresh();
                _isDirty = true;
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(1082, ex);
            }
        }

        private void ButtonEditClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var keyValuePair = (KeyValuePair<int, KeyPressInfo>)DataGridSequences.SelectedItem;
                var keyPressWindow = new KeyPressWindow(keyValuePair.Value);
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
                    _isDirty = true;
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(1083, ex);
            }
        }

        private void ButtonUpClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var value = (KeyValuePair<int, KeyPressInfo>)DataGridSequences.SelectedItem;
                MoveItemUp(value.Key);
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(1084, ex);
            }
        }

        private void ButtonDownClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var value = (KeyValuePair<int, KeyPressInfo>)DataGridSequences.SelectedItem;
                MoveItemDown(value.Key);
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(2003, ex);
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
                Common.ShowErrorMessageBox(2004, ex);
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
                Common.ShowErrorMessageBox(2005, ex);
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
                Common.ShowErrorMessageBox(2006, ex);
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
            _isDirty = true;
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
            _isDirty = true;
        }

        private void TextBoxInformationTextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                _isDirty = true;
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(2007, ex);
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
    }
}
