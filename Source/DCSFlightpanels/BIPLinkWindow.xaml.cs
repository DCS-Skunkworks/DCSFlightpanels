using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ClassLibraryCommon;
using NonVisuals;

namespace DCSFlightpanels
{
    /// <summary>
    /// Interaction logic for SequenceWindow.xaml
    /// </summary>
    public partial class BIPLinkWindow : Window
    {
        private BIPLinkPZ55 _bipLinkPZ55;
        private bool _isDirty;

        public BIPLinkWindow()
        {
            InitializeComponent();
            SetFormState();
            _bipLinkPZ55 = new BIPLinkPZ55();
        }

        public BIPLinkWindow(string information, BIPLinkPZ55 bipLinkPZ55)
        {
            InitializeComponent();
            _bipLinkPZ55 = bipLinkPZ55;
            textBoxInformation.Text = information;
            DataGridSequences.DataContext = _bipLinkPZ55.BIPLights;
            DataGridSequences.ItemsSource = _bipLinkPZ55.BIPLights;
            DataGridSequences.Items.Refresh();

            SetFormState();
        }

        public bool IsDirty
        {
            get { return _isDirty; }
        }

        public string GetInformation
        {
            get { return textBoxInformation.Text; }
        }

        private void SetFormState()
        {
            buttonUp.IsEnabled = DataGridSequences.SelectedItems.Count == 1 && DataGridSequences.SelectedIndex > 0;
            buttonDown.IsEnabled = DataGridSequences.SelectedItems.Count == 1 && DataGridSequences.SelectedIndex < DataGridSequences.Items.Count - 1;
            buttonAdd.IsEnabled = true;
            buttonEdit.IsEnabled = DataGridSequences.SelectedItems.Count == 1;
            buttonDelete.IsEnabled = DataGridSequences.SelectedItems.Count > 0;
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
                var bipLightWindow = new BipLightWindow();
                bipLightWindow.ShowDialog();
                if (bipLightWindow.DialogResult.HasValue && bipLightWindow.DialogResult.Value)
                {
                    
                    _bipLinkPZ55.BIPLights.Add(GetNewKeyValue(), bipLightWindow.BIPLight);

                    DataGridSequences.DataContext = _bipLinkPZ55.BIPLights;
                    DataGridSequences.ItemsSource = _bipLinkPZ55.BIPLights;
                    DataGridSequences.Items.Refresh();
                    _isDirty = true;
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(1065, ex);
            }
        }

        private int GetNewKeyValue()
        {
            if (_bipLinkPZ55.BIPLights.Count == 0)
            {
                return 0;
            }
            return _bipLinkPZ55.BIPLights.Keys.Max() + 1;
        }

        private void ButtonDeleteClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var value = (KeyValuePair<int, BIPLight>)DataGridSequences.SelectedItem;
                _bipLinkPZ55.BIPLights.Remove(value.Key);
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
                var keyValuePair = (KeyValuePair<int, BIPLight>)DataGridSequences.SelectedItem;
                var bipLightWindow = new BipLightWindow(keyValuePair.Value, _bipLinkPZ55.Description);
                bipLightWindow.ShowDialog();
                if (bipLightWindow.DialogResult.HasValue && bipLightWindow.DialogResult.Value)
                {
                    //Clicked OK
                    if (!bipLightWindow.IsDirty)
                    {
                        //User made no changes
                        return;
                    }
                    _bipLinkPZ55.BIPLights[keyValuePair.Key] = bipLightWindow.BIPLight;
                    DataGridSequences.DataContext = _bipLinkPZ55.BIPLights;
                    DataGridSequences.ItemsSource = _bipLinkPZ55.BIPLights;
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
                var value = (KeyValuePair<int, BIPLight>)DataGridSequences.SelectedItem;
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
                var value = (KeyValuePair<int, BIPLight>)DataGridSequences.SelectedItem;
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
                _bipLinkPZ55.Description = textBoxInformation.Text;
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
            var itemToBeMovedDown = _bipLinkPZ55.BIPLights[key];
            var itemToBeMovedUp = _bipLinkPZ55.BIPLights[key + 1];
            _bipLinkPZ55.BIPLights.Remove(key);
            _bipLinkPZ55.BIPLights.Remove(key + 1);
            _bipLinkPZ55.BIPLights.Add(key + 1, itemToBeMovedDown);
            _bipLinkPZ55.BIPLights.Add(key, itemToBeMovedUp);
            DataGridSequences.Items.Refresh();
            _isDirty = true;
        }

        private void MoveItemUp(int key)
        {
            //Moved down means index decreases, i.e. moved down further in the list
            var itemToBeMovedUp = _bipLinkPZ55.BIPLights[key];
            var itemToBeMovedDown = _bipLinkPZ55.BIPLights[key - 1];
            _bipLinkPZ55.BIPLights.Remove(key);
            _bipLinkPZ55.BIPLights.Remove(key - 1);
            _bipLinkPZ55.BIPLights.Add(key - 1, itemToBeMovedUp);
            _bipLinkPZ55.BIPLights.Add(key, itemToBeMovedDown);
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

        public BIPLinkPZ55 BIPLinkPZ55 => _bipLinkPZ55;
    }
}
