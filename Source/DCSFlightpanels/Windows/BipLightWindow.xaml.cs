using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ClassLibraryCommon;
using NonVisuals.Interfaces;
using NonVisuals.Saitek;
using NonVisuals.Saitek.Panels;

namespace DCSFlightpanels.Windows
{
    /// <summary>
    /// Interaction logic for BipLightWindow.xaml
    /// </summary>
    public partial class BipLightWindow : Window, IIsDirty
    {
        private readonly string _description;
        private bool _formLoaded;
        private BIPLight _bipLight;
        private bool _isDirty;

        public BipLightWindow()
        {
            InitializeComponent();
        }

        public BipLightWindow(BIPLight bipLight, string description)
        {
            InitializeComponent();
            _description = description;
            _bipLight = bipLight;
        }

        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                LabelDescription.Content = _description;
                _formLoaded = true;
                PopulateComboBoxes();
                CopyValues();
                SetFormState();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox( ex);
            }
        }

        private void SetFormState()
        {
            if (!_formLoaded)
            {
                return;
            }
            ButtonOk.IsEnabled = _bipLight != null;
            if (_bipLight == null)
            {
                LabelDescription.Visibility = Visibility.Collapsed;
            }
        }

        private void CopyValues()
        {
            try
            {
                _bipLight = new BIPLight
                {
                    BIPLedPosition = (BIPLedPositionEnum)ComboBoxPosition.SelectedValue,
                    LEDColor = (PanelLEDColor)ComboBoxColor.SelectedValue,
                    DelayBefore = (BIPLightDelays)ComboBoxDelay.SelectedValue,
                    BindingHash = (string)ComboBoxBIPPanel.SelectedValue
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"1003351 Error in CopyValues() : {ex.Message}");
            }
        }

        private void ClearAll()
        {
            _bipLight = null;
        }

        private void ButtonOkClick(object sender, RoutedEventArgs e)
        {
            try
            {
                CopyValues();
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
                ClearAll();
                DialogResult = false;
                Close();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox( ex);
            }
        }

        public BIPLight BIPLight
        {
            get { return _bipLight; }
            set { _bipLight = value; }
        }

        private void PopulateComboBoxes()
        {
            try
            {
                BipFactory.SetAllDark();
                ComboBoxPosition.SelectionChanged -= ComboBox_OnSelectionChanged;
                ComboBoxPosition.Items.Clear();
                foreach (BIPLedPositionEnum position in Enum.GetValues(typeof(BIPLedPositionEnum)))
                {
                    var comboBoxItem = new ComboBoxItem
                    {
                        Content = position
                    };
                    ComboBoxPosition.Items.Add(comboBoxItem);
                }

                ComboBoxDelay.SelectionChanged -= ComboBox_OnSelectionChanged;
                ComboBoxDelay.Items.Clear();
                foreach (BIPLightDelays delay in Enum.GetValues(typeof(BIPLightDelays)))
                {
                    var comboBoxItem = new ComboBoxItem
                    {
                        Content = delay
                    };
                    ComboBoxDelay.Items.Add(comboBoxItem);
                    ComboBoxDelay.SelectedValue = BIPLightDelays.Zeroms;
                }

                ComboBoxColor.SelectionChanged -= ComboBox_OnSelectionChanged;
                ComboBoxColor.Items.Clear();
                foreach (PanelLEDColor color in Enum.GetValues(typeof(PanelLEDColor)))
                {
                    var comboBoxItem = new ComboBoxItem
                    {
                        Content = color
                    };
                    ComboBoxColor.Items.Add(comboBoxItem);
                }
                ComboBoxColor.SelectedValue = PanelLEDColor.GREEN;

                ComboBoxBIPPanel.SelectionChanged -= ComboBox_OnSelectionChanged;
                ComboBoxBIPPanel.Items.Clear();
                foreach (BacklitPanelBIP bip in BipFactory.GetBips())
                {
                    var comboBoxItem = new ComboBoxItem
                    {
                        Content = bip.BindingHash
                    };
                    ComboBoxBIPPanel.Items.Add(comboBoxItem);
                }

                if (_bipLight != null)
                {
                    ComboBoxPosition.SelectedValue = _bipLight.BIPLedPosition;
                    ComboBoxDelay.SelectedValue = _bipLight.DelayBefore;
                    ComboBoxColor.SelectedValue = _bipLight.LEDColor;
                    ComboBoxBIPPanel.SelectedValue = _bipLight.BindingHash;
                }
                ShowLED();
            }
            finally
            {
                ComboBoxPosition.SelectionChanged += ComboBox_OnSelectionChanged;
                ComboBoxDelay.SelectionChanged += ComboBox_OnSelectionChanged;
                ComboBoxColor.SelectionChanged += ComboBox_OnSelectionChanged;
                ComboBoxBIPPanel.SelectionChanged += ComboBox_OnSelectionChanged;
            }
        }

        private void ComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                BipFactory.SetAllDark();
                if (string.IsNullOrEmpty(ComboBoxPosition.Text) || ComboBoxPosition.SelectedIndex == -1)
                {
                    return;
                }
                if (string.IsNullOrEmpty(ComboBoxDelay.Text) || ComboBoxDelay.SelectedIndex == -1)
                {
                    return;
                }
                if (string.IsNullOrEmpty(ComboBoxColor.Text) || ComboBoxColor.SelectedIndex == -1)
                {
                    return;
                }
                if (string.IsNullOrEmpty(ComboBoxBIPPanel.Text) || ComboBoxBIPPanel.SelectedIndex == -1)
                {
                    return;
                }
                SetFormState();
                _isDirty = true;
                ShowLED();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox( ex);
            }
        }

        private void ShowLED()
        {
            try
            {
                _bipLight = new BIPLight
                {
                    BIPLedPosition = (BIPLedPositionEnum)ComboBoxPosition.SelectedValue,
                    LEDColor = (PanelLEDColor)ComboBoxColor.SelectedValue,
                    DelayBefore = (BIPLightDelays)ComboBoxDelay.SelectedValue,
                    BindingHash = (string)ComboBoxBIPPanel.SelectedValue
                };
                BipFactory.SetDark(_bipLight.BindingHash);
                BipFactory.ShowLight(_bipLight);
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox( ex);
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

        private void BipLightWindow_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (!IsDirty && e.Key == Key.Escape)
            {
                e.Handled = true;
                Close();
            }
        }
    }
}