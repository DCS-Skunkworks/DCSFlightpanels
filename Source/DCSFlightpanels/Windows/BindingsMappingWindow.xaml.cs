namespace DCSFlightpanels.Windows
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Media;
    using System.Windows;
    using System.Windows.Input;
    using ClassLibraryCommon;
    using NonVisuals;
    using NonVisuals.Panels;

    /// <summary>
    /// Interaction logic for BindingsMappingWindow.xaml
    /// </summary>
    public partial class BindingsMappingWindow : Window
    {
        private bool _formLoaded;
        private bool _isDirty;
        private readonly List<GenericPanelBinding> _genericBindings;
        private readonly List<ModifiedGenericBinding> _modifiedGenericBindings = new();
        private readonly List<GamingPanel> _gamingPanels;

        public BindingsMappingWindow(List<GenericPanelBinding> genericBindings, List<GamingPanel> gamingPanels)
        {
            InitializeComponent();
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
                
                PopulateMissingHardware();
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
            ButtonSaveNewHardwareMapping.IsEnabled = ComboBoxPanelTypesMatch(); 
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
        
        private void PopulateMissingHardware()
        {
            ClearReplacementHardware();

            ComboBoxMissingHardware.ItemsSource = null;
            var missingDevices = _genericBindings.FindAll(o => o.InUse == false && o.HasBeenDeleted == false).ToList();
            ComboBoxMissingHardware.ItemsSource = missingDevices;
            ComboBoxMissingHardware.Items.Refresh();
            ShowMissingHardwareInformation();

            var genericBinding = (GenericPanelBinding)ComboBoxMissingHardware.SelectedItem;
            if (genericBinding == null)
            {
                return;
            }
            PopulateReplacementHardware(genericBinding.PanelType);
        }

        private void ShowMissingHardwareInformation()
        {
            LabelMissingPanelInformation.Content = string.Empty;
            var genericBinding = (GenericPanelBinding)ComboBoxMissingHardware.SelectedItem;
            if (genericBinding == null)
            {
                return;
            }

            LabelMissingPanelInformation.Content = genericBinding.HIDInstance;
        }

        private void ShowReplacementHardwareInformation()
        {
            LabelReplacementInformation.Content = string.Empty;
            var gamingPanel = (GamingPanel)ComboBoxReplacementHardware.SelectedItem;
            if (gamingPanel == null)
            {
                return;
            }
            LabelReplacementInformation.Content = gamingPanel.HIDInstance;
        }

        private void ClearReplacementHardware()
        {
            ComboBoxReplacementHardware.ItemsSource = null;
            LabelReplacementInformation.Content = string.Empty;
        }

        private void PopulateReplacementHardware(GamingPanelEnum panelType)
        {
            ComboBoxReplacementHardware.ItemsSource = null;
            var sameTypeOfPanels = _gamingPanels.FindAll(o => o.TypeOfPanel == panelType).ToList();
            foreach (var genericPanelBinding in _genericBindings)
            {
                if (genericPanelBinding.InUse)
                {
                    //Remove those that has been mapped already so they can't be re-mapped
                    sameTypeOfPanels.RemoveAll(o => o.HIDInstance.Equals(genericPanelBinding.HIDInstance) && o.TypeOfPanel == genericPanelBinding.PanelType);
                }
            }
            ComboBoxReplacementHardware.ItemsSource = sameTypeOfPanels;
            ComboBoxReplacementHardware.Items.Refresh();
            ShowReplacementHardwareInformation();
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
                var genericBinding = (GenericPanelBinding) ComboBoxMissingHardware.SelectedItem;
                if (genericBinding == null)
                {
                    return;
                }

                var informationTextBlockWindow = new InformationTextBlockWindow(string.IsNullOrEmpty(genericBinding.SettingsString) ? string.Empty : genericBinding.SettingsString);

                informationTextBlockWindow.ShowDialog();

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
                var genericBinding = (GenericPanelBinding)ComboBoxMissingHardware.SelectedItem;
                var gamingPanel = (GamingPanel)ComboBoxReplacementHardware.SelectedItem;
                if (genericBinding == null || gamingPanel == null)
                {
                    return;
                }

                genericBinding.HIDInstance = gamingPanel.HIDInstance;
                genericBinding.InUse = true;
                var modifiedGenericBinding = new ModifiedGenericBinding(GenericBindingStateEnum.Modified, genericBinding);
                _modifiedGenericBindings.Add(modifiedGenericBinding);
                _isDirty = true;

                SystemSounds.Asterisk.Play();

                if (!BindingMappingManager.UnusedBindingsExists())
                {
                    Close();
                }

                PopulateMissingHardware();
                
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
                if (ComboBoxMissingHardware.SelectedItem == null)
                {
                    return;
                }
                var genericBinding = (GenericPanelBinding) ComboBoxMissingHardware.SelectedItem;
                PopulateReplacementHardware(genericBinding.PanelType);
                LabelMissingPanelInformation.Content = genericBinding.HIDInstance;
                
                SetFormState();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        public List<GenericPanelBinding> GenericBindings => _genericBindings;

        public List<ModifiedGenericBinding> ModifiedGenericBindings => _modifiedGenericBindings;

        private void ComboBoxReplacementHardware_OnDropDownClosed(object sender, EventArgs e)
        {
            try
            {
                if (ComboBoxReplacementHardware.SelectedItem == null)
                {
                    return;
                }
                var gamingPanel = (GamingPanel)ComboBoxReplacementHardware.SelectedItem;
                
                LabelReplacementInformation.Content = gamingPanel.HIDInstance;
                SetFormState();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void ButtonDeleteBinding_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var genericBinding = (GenericPanelBinding)ComboBoxMissingHardware.SelectedItem;
                genericBinding.HasBeenDeleted = true;
                var modifiedGenericBinding = new ModifiedGenericBinding(GenericBindingStateEnum.Deleted, genericBinding);
                _modifiedGenericBindings.Add(modifiedGenericBinding);
                PopulateMissingHardware();
                SetFormState();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }
    }
}
