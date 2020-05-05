using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ClassLibraryCommon;
using DCS_BIOS;
using EnumEx = CommonClassLibraryJD.EnumEx;

namespace DCSFlightpanels.Windows
{
    /// <summary>
    /// Interaction logic for ChooseProfileModuleWindow.xaml
    /// </summary>
    public partial class ChooseProfileModuleWindow : Window
    {
        private DCSAirframe _dcsAirframe = DCSAirframe.A10C;
        private bool _useGenericRadio = false;

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
            ComboBoxAirframe.SelectionChanged -= ComboBoxAirframe_OnSelectionChanged;
            ComboBoxAirframe.Items.Clear();
            foreach (DCSAirframe airframe in Enum.GetValues(typeof(DCSAirframe)))
            {
                if (airframe != DCSAirframe.NOFRAMELOADEDYET && airframe != DCSAirframe.NS430)
                {
                    ComboBoxAirframe.Items.Add(EnumEx.GetDescription(airframe));
                }
            }
            ComboBoxAirframe.SelectedIndex = 0;
            ComboBoxAirframe.SelectionChanged += ComboBoxAirframe_OnSelectionChanged;
        }

        private void SetAirframe()
        {
            if (IsLoaded && ComboBoxAirframe.SelectedItem != null)
            {
                _dcsAirframe = EnumEx.GetValueFromDescription<DCSAirframe>(ComboBoxAirframe.SelectedItem.ToString());
                DCSBIOSControlLocator.Airframe = _dcsAirframe;
            }
        }

        private void ComboBoxAirframe_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                SetAirframe();
                if (_dcsAirframe != DCSAirframe.FC3_CD_SRS &&
                    _dcsAirframe != DCSAirframe.KEYEMULATOR &&
                    _dcsAirframe != DCSAirframe.KEYEMULATOR_SRS)
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

        public DCSAirframe DCSAirframe
        {
            get { return _dcsAirframe; }
        }

        public bool UseGenericRadio => _useGenericRadio;

        private void CheckBoxUseGenericRadio_OnChecked(object sender, RoutedEventArgs e)
        {
            try
            {
                _useGenericRadio = true;
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox( ex);
            }
        }

        private void CheckBoxUseGenericRadio_OnUnchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                _useGenericRadio = false;
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox( ex);
            }
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
