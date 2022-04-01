using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ClassLibraryCommon;
using ClassLibraryCommon.Enums;
using DCS_BIOS;

namespace DCSFlightpanels.Windows
{
    /// <summary>
    /// Interaction logic for ChooseProfileModuleWindow.xaml
    /// </summary>
    public partial class ChooseProfileModuleWindow : Window
    {
        private DCSFPModule _dcsfpProfile;

        public ChooseProfileModuleWindow()
        {
            InitializeComponent();
        }

        private void ChooseProfileModuleWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            PopulateAirframeCombobox();
        }

        private void ButtonCancel_OnClick(object sender, RoutedEventArgs e)
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

        private void ButtonOk_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                DialogResult = true;
                SetAirframe();
                Close();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox( ex);
            }
        }


        private void PopulateAirframeCombobox()
        {
            if (!IsLoaded)
            {
                return;
            }

            var itemsSource = new List<DCSFPModule>();
            ComboBoxAirframe.SelectionChanged -= ComboBoxAirframe_OnSelectionChanged;
            ComboBoxAirframe.Items.Clear();

            foreach (var module in DCSFPProfile.Modules.Where(x => !x.IsModule(ManagedModule.NoFrameLoadedYet)))
            { 
                itemsSource.Add(module);
            }

            ComboBoxAirframe.DisplayMemberPath = "Description";
            ComboBoxAirframe.ItemsSource = itemsSource;
            ComboBoxAirframe.SelectedIndex = 0;
            ComboBoxAirframe.SelectionChanged += ComboBoxAirframe_OnSelectionChanged;
        }

        private void SetAirframe()
        {
            if (IsLoaded && ComboBoxAirframe.SelectedItem != null)
            {
                _dcsfpProfile = (DCSFPModule)ComboBoxAirframe.SelectedItem;
                _dcsfpProfile.UseGenericRadio = CheckBoxUseGenericRadio.IsChecked == true;
                DCSFPProfile.SelectedProfile = _dcsfpProfile;
                DCSBIOSControlLocator.Profile = _dcsfpProfile;
            }
        }

        private void ComboBoxAirframe_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                SetAirframe();
                if (!_dcsfpProfile.IsModule(ManagedModule.FlamingCliff) &&
                    !_dcsfpProfile.IsModule(ManagedModule.KeyEmulator))
                {
                    //User has chosen a DCS-BIOS compatible module
                    StackPanelUseGenericRadio.Visibility = Visibility.Visible;
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox( ex);
            }
        }

        public DCSFPModule Profile
        {
            get { return _dcsfpProfile; }
        }

        private void ChooseProfileModuleWindow_OnKeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Escape)
                {
                    e.Handled = true;
                    DialogResult = false;
                    Close();
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }
    }
}
