using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using ClassLibraryCommon;
using NonVisuals;

namespace DCSFlightpanels.Windows
{
    /// <summary>
    /// Interaction logic for BindingsMappingWindow.xaml
    /// </summary>
    public partial class BindingsMappingWindow : Window
    {
        private bool _formLoaded;
        private bool _isDirty;
        private List<GenericPanelBinding> _genericBindings;
        private List<ModifiedGenericBinding> _modifiedGenericBindings;
        private List<GamingPanel> _gamingPanels;
        private bool _problemsSolved = false;

        public BindingsMappingWindow(List<GenericPanelBinding> genericBindings, List<GamingPanel> gamingPanels)
        {
            _genericBindings = genericBindings;
            _gamingPanels = gamingPanels;
        }

        public BindingsMappingWindow()
        {
            InitializeComponent();
        }

        private void BindingsMappingWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_formLoaded)
                {
                    return;
                }

                PopulateComboBoxes();
                _formLoaded = true;
                SetFormState();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void SetFormState()
        {
            ButtonSaveNewHardwareMapping.IsEnabled = _isDirty && ComboBoxPanelTypesMatch(); 
        }

        private bool ComboBoxPanelTypesMatch()
        {
            var gamingPanel = (GamingPanel)ComboBoxReplacementHardware.SelectedItem;
            var genericBinding = (GenericPanelBinding)ComboBoxMissingHardware.SelectedItem;

            if (gamingPanel == null || genericBinding == null)
            {
                return false;
            }

            return gamingPanel.TypeOfPanel == genericBinding.PanelType;
        }
        
        private void PopulateComboBoxes()
        {
            PopulateMissingHardware();
        }

        private void PopulateMissingHardware()
        {
            ComboBoxMissingHardware.Items.Clear();
            var missingDevices = _genericBindings.FindAll(o => o.HardwareWasFound == false).ToList();
            ComboBoxMissingHardware.ItemsSource = missingDevices;
            ComboBoxMissingHardware.Items.Refresh();
        }

        private void PopulateReplacementHardware(GamingPanelEnum panelType)
        {
            ComboBoxReplacementHardware.Items.Clear();
            var sameTypeOfPanels = _genericBindings.FindAll(o => o.PanelType == panelType).ToList();
            ComboBoxReplacementHardware.ItemsSource = sameTypeOfPanels;
            ComboBoxReplacementHardware.Items.Refresh();
        }

        private void BindingsMappingWindow_OnKeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (!IsDirty && e.Key == Key.Escape)
                {
                    e.Handled = true;
                    Close();
                }
                SetFormState();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        public bool IsDirty => _isDirty;

        public void SetIsDirty()
        {
            _isDirty = true;
        }

        public void StateSaved()
        {
            _isDirty = false;
        }

        private void ButtonClose_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                Close();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void ButtonSeeBindingText_OnClick(object sender, RoutedEventArgs e)
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

        private void ButtonSaveNewHardwareMapping_OnClick(object sender, RoutedEventArgs e)
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

        private void ButtonIdentifyPanel_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var gamingPanel = (GamingPanel)ComboBoxReplacementHardware.SelectedItem;
                gamingPanel.Identify();
                SetFormState();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }
        
        private void ComboBoxMissingHardware_OnDropDownClosed(object sender, EventArgs e)
        {
            try
            {
                var genericBinding = (GenericPanelBinding) ComboBoxMissingHardware.SelectedItem;
                PopulateReplacementHardware(genericBinding.PanelType);
                SetFormState();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        public List<GenericPanelBinding> GenericBindings => _genericBindings;

        public List<ModifiedGenericBinding> ModifiedGenericBindings => _modifiedGenericBindings;
    }
}
