﻿using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ClassLibraryCommon;
using NonVisuals;

namespace DCSFlightpanels
{
    /// <summary>
    /// Interaction logic for LEDConfigsWindow.xaml
    /// </summary>
    public partial class LEDConfigsWindow : Window
    {
        private List<DcsOutputAndColorBinding> _colorOutputBindings;
        private readonly string _description;
        private readonly SaitekPanel _callingPanel;
        private readonly SaitekPanelLEDPosition _saitekPanelLEDPosition;
        private readonly DCSAirframe _dcsAirframe = DCSAirframe.A10C;

        public LEDConfigsWindow(DCSAirframe dcsAirframe, string description, SaitekPanelLEDPosition saitekPanelLEDPosition, List<DcsOutputAndColorBinding> colorOutputBindings, SaitekPanel callingPanel)
        {
            InitializeComponent();
            _dcsAirframe = dcsAirframe;
            _saitekPanelLEDPosition = saitekPanelLEDPosition;
            _callingPanel = callingPanel;
            if (colorOutputBindings != null)
            {
                _colorOutputBindings = colorOutputBindings;
            }
            _description = description;
            ShowItems();
        }

        private void LEDConfigsWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            TextBoxLabel.Text = _description;
        }

        private void SetFormState()
        {
            EditButton.IsEnabled = DataGridValues.Items.Count > 0 && DataGridValues.SelectedItem != null;
            DeleteButton.IsEnabled = DataGridValues.Items.Count > 0 && DataGridValues.SelectedItem != null;
            CloneButton.IsEnabled = DataGridValues.Items.Count > 0 && DataGridValues.SelectedItem != null;
        }

        private void ShowItems()
        {
            try
            {
                DataGridValues.DataContext = _colorOutputBindings;
                DataGridValues.ItemsSource = _colorOutputBindings;
                DataGridValues.Items.Refresh();
                SetFormState();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(1013, ex);
            }
        }

        private void UpdateState()
        {
            try
            {

            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(1015, ex);
            }
        }

        public List<DcsOutputAndColorBinding> ColorOutputBindings
        {
            get { return _colorOutputBindings; }
            set { _colorOutputBindings = value; }
        }

        private void NewButton_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var button = (Button)sender;
                var panelColor = PanelLEDColor.DARK;
                if (button.Name.Contains("Dark"))
                {
                    panelColor = PanelLEDColor.DARK;
                }
                if (button.Name.Contains("Green"))
                {
                    panelColor = PanelLEDColor.GREEN;
                }
                if (button.Name.Contains("Yellow"))
                {
                    panelColor = PanelLEDColor.YELLOW;
                }
                if (button.Name.Contains("Red"))
                {
                    panelColor = PanelLEDColor.RED;
                }

                var dcsBiosOutputTriggerWindow = new DCSBiosOutputTriggerWindow(_dcsAirframe, "Set hook for color " + panelColor);
                if (dcsBiosOutputTriggerWindow.ShowDialog() == true)
                {
                    _colorOutputBindings.Add(_callingPanel.CreateDcsOutputAndColorBinding(_saitekPanelLEDPosition, panelColor, dcsBiosOutputTriggerWindow.DCSBiosOutput));
                    ShowItems();
                }

                SetFormState();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(1016, ex);
            }
        }

        private void EditButton_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var dcsOutputAndColorBinding = (DcsOutputAndColorBinding)DataGridValues.SelectedItem;
                var dcsBiosOutputTriggerWindow = new DCSBiosOutputTriggerWindow(_dcsAirframe, "Set hook for color " + dcsOutputAndColorBinding.LEDColor, dcsOutputAndColorBinding.DCSBiosOutputLED);
                if (dcsBiosOutputTriggerWindow.ShowDialog() == true)
                {
                    _colorOutputBindings.Remove(dcsOutputAndColorBinding);
                    _colorOutputBindings.Add(_callingPanel.CreateDcsOutputAndColorBinding(_saitekPanelLEDPosition, dcsOutputAndColorBinding.LEDColor, dcsBiosOutputTriggerWindow.DCSBiosOutput));
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

        private void DeleteButton_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (MessageBox.Show("Do you want to delete the selected hook?", "Delete hook?", MessageBoxButton.OKCancel, MessageBoxImage.Question) != MessageBoxResult.OK)
                {
                    return;
                }
                var dcsOutputAndColorBinding = (DcsOutputAndColorBinding)DataGridValues.SelectedItem;
                _colorOutputBindings.Remove(dcsOutputAndColorBinding);
                ShowItems();
                SetFormState();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(1090, ex);
            }
        }
        
        private void OkButton_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                DialogResult = true;
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

        private void LEDConfigsWindow_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                DialogResult = false;
                e.Handled = true;
                Close();
            }
        }

        private void ContextMenuColors_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (DataGridValues.SelectedItems.Count == 0)
                {
                    return;
                }

                PanelLEDColor color = PanelLEDColor.DARK;
                var menu = (MenuItem)sender;

                if (menu.Name.Contains("Dark"))
                {
                    color = PanelLEDColor.DARK;
                }
                else if (menu.Name.Contains("Green"))
                {
                    color = PanelLEDColor.GREEN;
                }
                else if (menu.Name.Contains("Yellow"))
                {
                    color = PanelLEDColor.YELLOW;
                }
                else if (menu.Name.Contains("Red"))
                {
                    color = PanelLEDColor.RED;
                }

                for (var i = 0; i < DataGridValues.SelectedItems.Count; i++)
                {
                    var binding = (DcsOutputAndColorBinding)DataGridValues.SelectedItems[i];
                    _colorOutputBindings.Remove(binding);
                    _colorOutputBindings.Add(_callingPanel.CreateDcsOutputAndColorBinding(_saitekPanelLEDPosition, color, binding.DCSBiosOutputLED));
                    ShowItems();
                }
                SetFormState();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(2012, ex);
            }
        }

        private void DataGridValues_OnContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            if (DataGridValues.SelectedItems.Count == 0)
            {
                e.Handled = true;
            }
        }

        private void CloneButton_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (DataGridValues.SelectedItems.Count == 0)
                {
                    e.Handled = true;
                }
                for (var i = 0; i < DataGridValues.SelectedItems.Count; i++)
                {
                    var binding = (DcsOutputAndColorBinding)DataGridValues.SelectedItems[i];
                    _colorOutputBindings.Add(_callingPanel.CreateDcsOutputAndColorBinding(_saitekPanelLEDPosition, binding.LEDColor, binding.DCSBiosOutputLED));
                    ShowItems();
                }
                SetFormState();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(2012, ex);
            }
        }
    }
}
