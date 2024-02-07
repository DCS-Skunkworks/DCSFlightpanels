using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ClassLibraryCommon;
using DCS_BIOS.ControlLocator;
using DCS_BIOS.misc;

namespace DCSFlightpanels.Windows
{
    /// <summary>
    /// Interaction logic for ChooseProfileModuleWindow.xaml
    /// </summary>
    public partial class ChooseProfileModuleWindow
    {
        private DCSAircraft _dcsAircraft;
        private bool _isLoaded;

        public ChooseProfileModuleWindow()
        {
            InitializeComponent();
        }

        private void ChooseProfileModuleWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            if (_isLoaded) return;

            PopulateAirframeCombobox();
            DarkMode.SetFrameworkElementDarkMode(this);
            _isLoaded = true;
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

            var itemsSource = new List<DCSAircraft>();
            ComboBoxAirframe.SelectionChanged -= ComboBoxAirframe_OnSelectionChanged;
            ComboBoxAirframe.Items.Clear();
            foreach (var module in DCSAircraft.Modules)
            {
                if (!DCSAircraft.IsNoFrameLoadedYet(module) && module.ID < DCSBIOSConstants.META_MODULE_ID_START_RANGE) //!DCSAircraft.IsNS430(module) &&  
                {
                    itemsSource.Add(module);
                }
            }
            itemsSource = itemsSource.OrderBy(o => o.Description).ToList();
            ComboBoxAirframe.DisplayMemberPath = "Description";
            ComboBoxAirframe.ItemsSource = itemsSource;
            ComboBoxAirframe.SelectedIndex = 0;
            ComboBoxAirframe.SelectionChanged += ComboBoxAirframe_OnSelectionChanged;

            if (ComboBoxAirframe.Items.Count <= 0)
            {
                return;
            }

            SetAirframe();
            StackPanelA10C.Visibility = DCSAircraft.IsA10C(_dcsAircraft) ? Visibility.Visible : Visibility.Collapsed;
        }

        private void SetAirframe()
        {
            if (IsLoaded && ComboBoxAirframe.SelectedItem != null)
            {
                _dcsAircraft = (DCSAircraft)ComboBoxAirframe.SelectedItem;
                _dcsAircraft.UseGenericRadio = CheckBoxUseGenericRadio.IsChecked == true;
                _dcsAircraft.Option1 = RadioButtonA10CII.IsChecked == true;
                DCSAircraft.SelectedAircraft = _dcsAircraft;
                DCSBIOSControlLocator.DCSAircraft = _dcsAircraft;
            }
        }

        private void ComboBoxAirframe_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                SetAirframe();
                if (!DCSAircraft.IsFlamingCliff(_dcsAircraft) &&
                    !DCSAircraft.IsKeyEmulator(_dcsAircraft) &&
                    !DCSAircraft.IsKeyEmulatorSRS(_dcsAircraft))
                {
                    //User has chosen a DCS-BIOS compatible module
                    StackPanelUseGenericRadio.Visibility = Visibility.Visible;
                }

                StackPanelA10C.Visibility = DCSAircraft.IsA10C(_dcsAircraft) ? Visibility.Visible : Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox( ex);
            }
        }

        public DCSAircraft Profile
        {
            get { return _dcsAircraft; }
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
