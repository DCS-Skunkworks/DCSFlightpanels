using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ClassLibraryCommon;
using DCS_BIOS;

namespace DCSFlightpanels.Windows
{
    /// <summary>
    /// Interaction logic for ChooseProfileModuleWindow.xaml
    /// </summary>
    public partial class ChooseProfileModuleWindow : Window
    {
        private DCSFPProfile _dcsfpProfile;

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

            var itemsSource = new List<DCSFPProfile>();
            ComboBoxAirframe.SelectionChanged -= ComboBoxAirframe_OnSelectionChanged;
            ComboBoxAirframe.Items.Clear();
            foreach (var module in DCSFPProfile.Modules)
            {
                if (!DCSFPProfile.IsNoFrameLoadedYet(module)) //!DCSFPProfile.IsNS430(module) &&  
                {
                    itemsSource.Add(module);
                }
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
                _dcsfpProfile = (DCSFPProfile)ComboBoxAirframe.SelectedItem;
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
                if (!DCSFPProfile.IsFlamingCliff(_dcsfpProfile) &&
                    !DCSFPProfile.IsKeyEmulator(_dcsfpProfile))
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

        public DCSFPProfile Profile
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
