using System;
using System.Windows;
using System.Windows.Input;
using ClassLibraryCommon;

namespace DCSFlightpanels.Windows
{
    /// <summary>
    /// Interaction logic for BindingsMappingWindow.xaml
    /// </summary>
    public partial class BindingsMappingWindow : Window
    {
        private bool _formLoaded;
        private bool _isDirty;

        public BindingsMappingWindow()
        {
            InitializeComponent();
        }

        private void BindingsMappingWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            try
            {

                //PopulateComboBoxes();
                //CopyValues();
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

        }

        private void BindingsMappingWindow_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (!IsDirty && e.Key == Key.Escape)
            {
                e.Handled = true;
                Close();
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
            Close();
        }

        private void ButtonSeeBindingText_OnClick(object sender, RoutedEventArgs e)
        {
            
        }

        private void ButtonSaveNewHardwareMapping_OnClick(object sender, RoutedEventArgs e)
        {
            
        }
    }
}
