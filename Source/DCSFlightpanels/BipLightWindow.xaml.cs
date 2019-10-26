using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ClassLibraryCommon;
using NonVisuals.Saitek;

namespace DCSFlightpanels
{
    /// <summary>
    /// Interaction logic for BipLightWindow.xaml
    /// </summary>
    public partial class BipLightWindow : Window
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
                Common.ShowErrorMessageBox(1001, ex);
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
                _bipLight = new BIPLight();
                _bipLight.BIPLedPosition = (BIPLedPositionEnum)ComboBoxPosition.SelectedValue;
                _bipLight.LEDColor = (PanelLEDColor)ComboBoxColor.SelectedValue;
                _bipLight.DelayBefore = (BIPLightDelays)ComboBoxDelay.SelectedValue;
                _bipLight.Hash = (string)ComboBoxBIPPanel.SelectedValue;
            }
            catch (Exception e)
            {
                throw new Exception("1003351 Error in CopyValues() : " + e.Message);
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
                Common.ShowErrorMessageBox(1002, ex);
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
                Common.ShowErrorMessageBox(1003, ex);
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
                    var comboBoxItem = new ComboBoxItem();
                    comboBoxItem.Content = position;
                    ComboBoxPosition.Items.Add(comboBoxItem);
                }

                ComboBoxDelay.SelectionChanged -= ComboBox_OnSelectionChanged;
                ComboBoxDelay.Items.Clear();
                foreach (BIPLightDelays delay in Enum.GetValues(typeof(BIPLightDelays)))
                {
                    var comboBoxItem = new ComboBoxItem();
                    comboBoxItem.Content = delay;
                    ComboBoxDelay.Items.Add(comboBoxItem);
                    ComboBoxDelay.SelectedValue = BIPLightDelays.Zeroms;
                }

                ComboBoxColor.SelectionChanged -= ComboBox_OnSelectionChanged;
                ComboBoxColor.Items.Clear();
                foreach (PanelLEDColor color in Enum.GetValues(typeof(PanelLEDColor)))
                {
                    var comboBoxItem = new ComboBoxItem();
                    comboBoxItem.Content = color;
                    ComboBoxColor.Items.Add(comboBoxItem);
                }
                ComboBoxColor.SelectedValue = PanelLEDColor.GREEN;

                ComboBoxBIPPanel.SelectionChanged -= ComboBox_OnSelectionChanged;
                ComboBoxBIPPanel.Items.Clear();
                foreach (BacklitPanelBIP bip in BipFactory.GetBips())
                {
                    var comboBoxItem = new ComboBoxItem();
                    comboBoxItem.Content = bip.Hash;
                    ComboBoxBIPPanel.Items.Add(comboBoxItem);
                }

                if (_bipLight != null)
                {
                    ComboBoxPosition.SelectedValue = _bipLight.BIPLedPosition;
                    ComboBoxDelay.SelectedValue = _bipLight.DelayBefore;
                    ComboBoxColor.SelectedValue = _bipLight.LEDColor;
                    ComboBoxBIPPanel.SelectedValue = _bipLight.Hash;
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
                Common.ShowErrorMessageBox(100906, ex);
            }
        }

        private void ShowLED()
        {
            try
            {
                _bipLight = new BIPLight();
                _bipLight.BIPLedPosition = (BIPLedPositionEnum)ComboBoxPosition.SelectedValue;
                _bipLight.LEDColor = (PanelLEDColor)ComboBoxColor.SelectedValue;
                _bipLight.DelayBefore = (BIPLightDelays)ComboBoxDelay.SelectedValue;
                _bipLight.Hash = (string)ComboBoxBIPPanel.SelectedValue;
                BipFactory.SetDark(_bipLight.Hash);
                BipFactory.ShowLight(_bipLight);
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(10906, ex);
            }
        }
        public bool IsDirty => _isDirty;

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