using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ClassLibraryCommon;
using NonVisuals.BindingClasses.BIP;
using NonVisuals.Saitek;

namespace DCSFlightpanels.Windows
{
    /// <summary>
    /// Interaction logic for SequenceWindow.xaml
    /// </summary>
    public partial class BIPLinkWindow
    {
        private readonly BIPLinkBase _bipLink;
        private bool _isDirty;
        
        public BIPLinkWindow(BIPLinkBase bipLink)
        {
            InitializeComponent();
            _bipLink = bipLink;
            TextBoxInformation.Text = bipLink.Description;
            DataGridSequences.DataContext = _bipLink.BIPLights;
            DataGridSequences.ItemsSource = _bipLink.BIPLights;
            DataGridSequences.Items.Refresh();

            SetFormState();
        }

        public bool IsDirty
        {
            get { return _isDirty; }
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
                AddBIPLight();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox( ex);
            }
        }

        private void AddBIPLight()
        {
            var bipLightWindow = new BipLightWindow();
            bipLightWindow.ShowDialog();
            if (bipLightWindow.DialogResult.HasValue && bipLightWindow.DialogResult.Value)
            {

                _bipLink.BIPLights.Add(GetNewKeyValue(), bipLightWindow.BIPLight);

                DataGridSequences.DataContext = _bipLink.BIPLights;
                DataGridSequences.ItemsSource = _bipLink.BIPLights;
                DataGridSequences.Items.Refresh();
                _isDirty = true;
            }
            SetFormState();
        }

        private int GetNewKeyValue()
        {
            if (_bipLink.BIPLights.Count == 0)
            {
                return 0;
            }
            return _bipLink.BIPLights.Keys.Max() + 1;
        }

        private void ButtonDeleteClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var value = (KeyValuePair<int, BIPLight>)DataGridSequences.SelectedItem;
                _bipLink.BIPLights.Remove(value.Key);
                DataGridSequences.Items.Refresh();
                _isDirty = true;
                SetFormState();
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
                EditBIPLight();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox( ex);
            }
        }

        private void EditBIPLight()
        {
            var keyValuePair = (KeyValuePair<int, BIPLight>)DataGridSequences.SelectedItem;
            var bipLightWindow = new BipLightWindow(keyValuePair.Value, _bipLink.Description);
            bipLightWindow.ShowDialog();
            if (bipLightWindow.DialogResult.HasValue && bipLightWindow.DialogResult.Value)
            {
                //Clicked OK
                if (!bipLightWindow.IsDirty)
                {
                    //User made no changes
                    return;
                }
                _bipLink.BIPLights[keyValuePair.Key] = bipLightWindow.BIPLight;
                DataGridSequences.DataContext = _bipLink.BIPLights;
                DataGridSequences.ItemsSource = _bipLink.BIPLights;
                DataGridSequences.Items.Refresh();
                _isDirty = true;
            }
            SetFormState();
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
                Common.ShowErrorMessageBox( ex);
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
                Common.ShowErrorMessageBox( ex);
            }
        }

        private void ButtonOkClick(object sender, RoutedEventArgs e)
        {
            try
            {
                _bipLink.Description = TextBoxInformation.Text;
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
            var itemToBeMovedDown = _bipLink.BIPLights[key];
            var itemToBeMovedUp = _bipLink.BIPLights[key + 1];
            _bipLink.BIPLights.Remove(key);
            _bipLink.BIPLights.Remove(key + 1);
            _bipLink.BIPLights.Add(key + 1, itemToBeMovedDown);
            _bipLink.BIPLights.Add(key, itemToBeMovedUp);
            DataGridSequences.Items.Refresh();
            _isDirty = true;
        }

        private void MoveItemUp(int key)
        {
            //Moved down means index decreases, i.e. moved down further in the list
            var itemToBeMovedUp = _bipLink.BIPLights[key];
            var itemToBeMovedDown = _bipLink.BIPLights[key - 1];
            _bipLink.BIPLights.Remove(key);
            _bipLink.BIPLights.Remove(key - 1);
            _bipLink.BIPLights.Add(key - 1, itemToBeMovedUp);
            _bipLink.BIPLights.Add(key, itemToBeMovedDown);
            DataGridSequences.Items.Refresh();
            _isDirty = true;
        }

        private void TextBoxInformationTextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                _isDirty = true;
                SetFormState();

            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox( ex);
            }
        }

        public BIPLinkBase BIPLink => _bipLink;

        private void BIPLinkWindow_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (!ButtonOk.IsEnabled && e.Key == Key.Escape)
            {
                e.Handled = true;
                Close();
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

        private void DataGridSequences_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (DataGridSequences.Items.Count == 0 || DataGridSequences.SelectedItems.Count == 0)
                {
                    AddBIPLight();
                }
                else if (DataGridSequences.SelectedItems.Count == 1)
                {
                    EditBIPLight();
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }
    }
}
