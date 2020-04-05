using System;
using System.Globalization;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
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
            TextBoxReferenceValue.Text = dcsbiosNumberToText.ReferenceValue.ToString(CultureInfo.InvariantCulture);
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
                SetComboBoxValue();
                TextBoxReferenceValue.Focus();
                _isLoaded = true;
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
                ButtonOk.IsEnabled = false;

                TextBoxReferenceValue.IsEnabled = GetEnumValue() != EnumComparator.Always;
                TextBoxReferenceValue.Background = GetEnumValue() != EnumComparator.Always ? new SolidColorBrush(Colors.White) : new SolidColorBrush(Colors.LightGray);

                switch (GetEnumValue())
                {
                    case EnumComparator.Always:
                        {
                            ButtonOk.IsEnabled = !string.IsNullOrEmpty(TextBoxOutputText.Text) &&
                                                 !string.IsNullOrEmpty(ComboBoxComparisonType.Text);
                            break;
                        }
                    default:
                        {
                            ButtonOk.IsEnabled = !string.IsNullOrEmpty(TextBoxReferenceValue.Text) &&
                                                 double.TryParse(TextBoxReferenceValue.Text, out var value) &&
                                                 !string.IsNullOrEmpty(TextBoxOutputText.Text) &&
                                                 !string.IsNullOrEmpty(ComboBoxComparisonType.Text);
                            break;
                        }
                }


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
                if (GetEnumValue() != EnumComparator.Always)
                {
                    _dcsbiosComparator.ReferenceValue = UInt32.Parse(TextBoxReferenceValue.Text);
                }
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
                return EnumComparator.GreaterThan;
            }
            if (ComboBoxComparisonType.Text == ">=")
            {
                return EnumComparator.GreaterThanEqual;
            }
            if (ComboBoxComparisonType.Text == "Always")
            {
                return EnumComparator.Always;
            }
            throw new Exception("Failed to decode comparison type.");
        }

        private void SetComboBoxValue()
        {
            switch (_dcsbiosComparator.Comparator)
            {
                case EnumComparator.Equals:
                    {
                        ComboBoxComparisonType.Text = "==";
                        break;
                    }
                case EnumComparator.NotEquals:
                    {
                        ComboBoxComparisonType.Text = "!=";
                        break;
                    }
                case EnumComparator.LessThan:
                    {
                        ComboBoxComparisonType.Text = "<";
                        break;
                    }
                case EnumComparator.LessThanEqual:
                    {
                        ComboBoxComparisonType.Text = "<=";
                        break;
                    }
                case EnumComparator.GreaterThan:
                    {
                        ComboBoxComparisonType.Text = ">";
                        break;
                    }
                case EnumComparator.GreaterThanEqual:
                    {
                        ComboBoxComparisonType.Text = ">=";
                        break;
                    }
                case EnumComparator.Always:
                    {
                        ComboBoxComparisonType.Text = "Always";
                        break;
                    }
                default:
                    {
                        throw new Exception("Failed to decode comparison type.");
                    }
            }
        }

        public DCSBIOSNumberToText DCSBIOSComparator
        {
            get => _dcsbiosComparator;
        }

        private void TextBoxReferenceValue_OnKeyUp(object sender, KeyEventArgs e)
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


        private void LabelInsert_OnMouseEnter(object sender, MouseEventArgs e)
        {
            try
            {
                Mouse.OverrideCursor = Cursors.Hand;
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void LabelInsert_OnMouseLeave(object sender, MouseEventArgs e)
        {
            try
            {
                Mouse.OverrideCursor = Cursors.Arrow;
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void LabelInsert_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (!TextBoxOutputText.Text.Contains(CommonStreamDeck.DCSBIOS_PLACE_HOLDER))
                {
                    TextBoxOutputText.Text = string.IsNullOrEmpty(TextBoxOutputText.Text) ? CommonStreamDeck.DCSBIOS_PLACE_HOLDER : CommonStreamDeck.DCSBIOS_PLACE_HOLDER + " " + TextBoxOutputText.Text;
                    TextBoxOutputText.CaretIndex = TextBoxOutputText.Text.Length;
                    SetFormState();
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void ButtonHelp_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var infoWindow = new InformationTextBlockWindow();
                infoWindow.TextBlockInformation.FontSize = 14;
                infoWindow.AddInline("Most DCS-BIOS values must be formatted to human readable values. These values usually range between ");
                infoWindow.AddInline(new Run("0-65535.\n") { FontWeight = FontWeights.Bold });
                infoWindow.AddInline("Heading for example must be calculated from this to a proper value ");
                infoWindow.AddInline(new Run("0-359°.\n") { FontWeight = FontWeights.Bold });
                infoWindow.AddInline("This is the formula for the A-10C HSI heading:\n");
                infoWindow.AddInline(new Run("360 - truncate(HSI_HDG*360/65535)\n") { FontWeight = FontWeights.Bold });
                infoWindow.AddInline("\n");
                infoWindow.AddInline("Some DCS-BIOS values are useless in the number format, one example is a radio's dial position.\n");
                infoWindow.AddInline("\n");
                infoWindow.AddInline("Here you can decode values so that the various dial positions will be translated into text like OFF MANUAL PRESET and so on.\n");
                infoWindow.AddInline("");
                infoWindow.AddInline("If you want to show units after the value you can do this here too. Choose comparion mode \"Always\" for this.\n");
                infoWindow.AddInline(new Run("7400 RPM\n") { FontWeight = FontWeights.Bold });
                infoWindow.AddInline(new Run("242°\n") { FontWeight = FontWeights.Bold });
                infoWindow.AddInline(new Run("180°C\n") { FontWeight = FontWeights.Bold });
                infoWindow.AddInline("\n");
                infoWindow.AddInline("You can have different colors for different temperature ranges.");
                infoWindow.Show();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void ComboBoxComparisonType_OnDropDownClosed(object sender, EventArgs e)
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
    }
}
