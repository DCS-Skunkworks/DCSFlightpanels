using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ClassLibraryCommon;
using NonVisuals;
using NonVisuals.StreamDeck;

namespace DCSFlightpanels.Windows
{

    public partial class DCSBIOSComparatorWindow : Window
    {
        private bool _isLoaded = false;
        private DCSBIOSNumberToText _dcsbiosComparator = new DCSBIOSNumberToText();
        private bool _isDirty;

        public DCSBIOSComparatorWindow()
        {
            InitializeComponent();
        }

        public DCSBIOSComparatorWindow(DCSBIOSNumberToText dcsbiosNumberToText)
        {
            InitializeComponent();
            _dcsbiosComparator = dcsbiosNumberToText;
            TextBoxOutputText.Text = dcsbiosNumberToText.OutputText;
            TextBoxValue.Text = dcsbiosNumberToText.ReferenceValue.ToString();
        }

        private void DCSBIOSComparatorWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_isLoaded)
                {
                    return;
                }

                SetFormState();
                _isLoaded = true;
                TextBoxValue.Focus();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void SetFormState()
        {
            try
            {
                ButtonOk.IsEnabled = !string.IsNullOrEmpty(TextBoxValue.Text) && !string.IsNullOrEmpty(TextBoxOutputText.Text) && !string.IsNullOrEmpty(ComboBoxComparisonType.Text);
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void TextBoxOutputText_OnKeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                if (!_isLoaded)
                {
                    return;
                }
                SetFormState();
                _isDirty = true;
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
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
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void ButtonOk_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                _dcsbiosComparator.OutputText = TextBoxOutputText.Text;
                _dcsbiosComparator.ReferenceValue = UInt32.Parse(TextBoxValue.Text);
                _dcsbiosComparator.Comparator = GetEnumValue();
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private EnumComparator GetEnumValue()
        {
            if (ComboBoxComparisonType.Text == "==")
            {
                return EnumComparator.Equals;
            }
            if (ComboBoxComparisonType.Text == "!=")
            {
                return EnumComparator.NotEquals;
            }
            if (ComboBoxComparisonType.Text == "<")
            {
                return EnumComparator.LessThan;
            }
            if (ComboBoxComparisonType.Text == "<=")
            {
                return EnumComparator.LessThanEqual;
            }
            if (ComboBoxComparisonType.Text == ">")
            {
                return EnumComparator.BiggerThan;
            }
            if (ComboBoxComparisonType.Text == ">=")
            {
                return EnumComparator.BiggerThanEqual;
            }
            throw new Exception("Failed to decode comparison type.");
        }

        public DCSBIOSNumberToText DCSBIOSComparator
        {
            get => _dcsbiosComparator;
        }

        private void TextBoxValue_OnKeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                if (!_isLoaded)
                {
                    return;
                }
                SetFormState();
                _isDirty = true;
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        public bool IsDirty => _isDirty;
    }
}
