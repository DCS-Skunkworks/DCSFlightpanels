using System;
using System.IO;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using ClassLibraryCommon;
using DCSFlightpanels.CustomControls;
using DCSFlightpanels.Properties;
using DCSFlightpanels.Shared;
using NonVisuals.Interfaces;
using NonVisuals.StreamDeck;
using ComboBox = System.Windows.Controls.ComboBox;
using Cursors = System.Windows.Input.Cursors;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;

namespace DCSFlightpanels.Windows
{
    public partial class DCSBIOSConverterWindow : Window, IIsDirty
    {
        private bool _isLoaded = false;
        private DCSBIOSValueToFaceConverter _dcsbiosConverter = new DCSBIOSValueToFaceConverter();
        private bool _isDirty;
        private bool _isPopulatingData = false;
        private StreamDeckPanel _streamDeckPanel;
        private EnumStreamDeckButtonNames _streamDeckButtonName;



        public DCSBIOSConverterWindow(EnumStreamDeckButtonNames streamDeckButtonName, StreamDeckPanel streamDeckPanel)
        {
            InitializeComponent();
            _streamDeckButtonName = streamDeckButtonName;
            _streamDeckPanel = streamDeckPanel;
        }

        public DCSBIOSConverterWindow(EnumStreamDeckButtonNames streamDeckButtonName, StreamDeckPanel streamDeckPanel, DCSBIOSValueToFaceConverter dcsbiosConverter)
        {
            InitializeComponent();
            _streamDeckButtonName = streamDeckButtonName;
            _streamDeckPanel = streamDeckPanel;
            _dcsbiosConverter = dcsbiosConverter;
        }

        private void DCSBIOSComparatorWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_isLoaded)
                {
                    return;
                }

                ShowConverter();
                SetFormState();
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



                /*TextBoxReferenceValue.IsEnabled = GetEnumValue() != EnumComparator.Always;
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
                */

            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void TextBoxOutputTextAsIs_OnKeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                if (!_isLoaded)
                {
                    return;
                }
                SetFormState();
                SetIsDirty();
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
                /*_dcsbiosConverter.OutputText = TextBoxOutputText.Text;
                if (GetEnumValue() != EnumComparator.Always)
                {
                    _dcsbiosConverter.ReferenceValue = UInt32.Parse(TextBoxReferenceValue.Text);
                }
                _dcsbiosConverter.Comparator = GetEnumValue();
                DialogResult = true;
                Close();*/
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void SecondCriteriaVisibility(bool show)
        {
            StackPanelAddSecondCriteria.Visibility = show ? Visibility.Collapsed : Visibility.Visible;
            StackPanelSecondCriteria.Visibility = show ? Visibility.Visible : Visibility.Collapsed;
        }

        private EnumComparator GetEnumValue(ComboBox comboBoxComparisonType)
        {
            if (comboBoxComparisonType.Text == "==")
            {
                return EnumComparator.Equals;
            }
            if (comboBoxComparisonType.Text == "!=")
            {
                return EnumComparator.NotEquals;
            }
            if (comboBoxComparisonType.Text == "<")
            {
                return EnumComparator.LessThan;
            }
            if (comboBoxComparisonType.Text == "<=")
            {
                return EnumComparator.LessThanEqual;
            }
            if (comboBoxComparisonType.Text == ">")
            {
                return EnumComparator.GreaterThan;
            }
            if (comboBoxComparisonType.Text == ">=")
            {
                return EnumComparator.GreaterThanEqual;
            }
            if (comboBoxComparisonType.Text == "Always")
            {
                return EnumComparator.Always;
            }
            throw new Exception("Failed to decode comparison type.");
        }

        private void SetComboBoxValue(ComboBox comboBoxComparisonType, EnumComparator comparator)
        {
            switch (comparator)
            {
                case EnumComparator.Equals:
                    {
                        comboBoxComparisonType.Text = "==";
                        break;
                    }
                case EnumComparator.NotEquals:
                    {
                        comboBoxComparisonType.Text = "!=";
                        break;
                    }
                case EnumComparator.LessThan:
                    {
                        comboBoxComparisonType.Text = "<";
                        break;
                    }
                case EnumComparator.LessThanEqual:
                    {
                        comboBoxComparisonType.Text = "<=";
                        break;
                    }
                case EnumComparator.GreaterThan:
                    {
                        comboBoxComparisonType.Text = ">";
                        break;
                    }
                case EnumComparator.GreaterThanEqual:
                    {
                        comboBoxComparisonType.Text = ">=";
                        break;
                    }
                case EnumComparator.Always:
                    {
                        comboBoxComparisonType.Text = "Always";
                        break;
                    }
                default:
                    {
                        throw new Exception("Failed to decode comparison type.");
                    }
            }
        }
        
        public DCSBIOSValueToFaceConverter DCSBIOSConverter
        {
            get => _dcsbiosConverter;
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
                SetIsDirty();
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

        private void LabelInsertAsIs_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (!TextBoxOutputTextAsIs.Text.Contains(StreamDeckConstants.DCSBIOSValuePlaceHolder))
                {
                    TextBoxOutputTextAsIs.Text = string.IsNullOrEmpty(TextBoxOutputTextAsIs.Text) ? StreamDeckConstants.DCSBIOSValuePlaceHolder : StreamDeckConstants.DCSBIOSValuePlaceHolder + " " + TextBoxOutputTextAsIs.Text;
                    TextBoxOutputTextAsIs.CaretIndex = TextBoxOutputTextAsIs.Text.Length;
                    SetFormState();
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }


        private void ShowConverter()
        {
            _isPopulatingData = true;
            SecondCriteriaVisibility(_dcsbiosConverter.Comparator2 != EnumComparator.None);
            switch (_dcsbiosConverter.OutputType)
            {
                case EnumConverterOutputType.NotSet:
                    {
                        StackPanelAsIs.Visibility = Visibility.Collapsed;
                        StackPanelImage.Visibility = Visibility.Collapsed;
                        StackPanelOverlayImage.Visibility = Visibility.Collapsed;
                        break;
                    }
                case EnumConverterOutputType.OutputAsIs:
                    {
                        StackPanelAsIs.Visibility = Visibility.Visible;
                        StackPanelImage.Visibility = Visibility.Collapsed;
                        StackPanelOverlayImage.Visibility = Visibility.Collapsed;
                        break;
                    }
                case EnumConverterOutputType.OutputImage:
                    {
                        StackPanelAsIs.Visibility = Visibility.Collapsed;
                        StackPanelImage.Visibility = Visibility.Visible;
                        StackPanelOverlayImage.Visibility = Visibility.Collapsed;
                        break;
                    }
                case EnumConverterOutputType.OutputImageOverlay:
                    {
                        StackPanelAsIs.Visibility = Visibility.Collapsed;
                        StackPanelImage.Visibility = Visibility.Collapsed;
                        StackPanelOverlayImage.Visibility = Visibility.Visible;
                        break;
                    }
            }
            //SetComboBoxValue();
            _isPopulatingData = false;
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

        private void ButtonAddSecondCriteria_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                StackPanelAddSecondCriteria.Visibility = Visibility.Collapsed;
                StackPanelSecondCriteria.Visibility = Visibility.Visible;
                ComboBoxItemAlways1.IsEnabled = false;
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void ButtonRemoveSecondCriteria_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                StackPanelAddSecondCriteria.Visibility = Visibility.Visible;
                StackPanelSecondCriteria.Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void RadioButtonDCSBIOSValue_OnChecked(object sender, RoutedEventArgs e)
        {
            try
            {
                StackPanelAsIs.Visibility = Visibility.Visible;
                StackPanelImage.Visibility = Visibility.Collapsed;
                StackPanelOverlayImage.Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void RadioButtonImage_OnChecked(object sender, RoutedEventArgs e)
        {
            try
            {
                StackPanelAsIs.Visibility = Visibility.Collapsed;
                StackPanelImage.Visibility = Visibility.Visible;
                StackPanelOverlayImage.Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void RadioButtonOverlayImage_OnChecked(object sender, RoutedEventArgs e)
        {
            try
            {
                StackPanelAsIs.Visibility = Visibility.Collapsed;
                StackPanelImage.Visibility = Visibility.Collapsed;
                StackPanelOverlayImage.Visibility = Visibility.Visible;
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void ButtonBrowseImage_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var path = "";
                var directory = Settings.Default.LastFileDialogLocation;

                var dialogResult = StreamDeckCommon.BrowseForImage(ref directory, ref path);

                if (dialogResult == System.Windows.Forms.DialogResult.OK)
                {
                    TextBoxImage.Bill.ImageFilePath = path;
                    Settings.Default.LastFileDialogLocation = directory;
                    SetIsDirty();
                    SetFormState();
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void ButtonTestSelectImage_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {

            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void TextBoxOutputTextOverlayImage_OnKeyUp(object sender, KeyEventArgs e)
        {
            try
            {

            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void LabelInsertOverlayImage_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (!TextBoxOutputTextOverlayImage.Text.Contains(StreamDeckConstants.DCSBIOSValuePlaceHolder))
                {
                    TextBoxOutputTextOverlayImage.Text = string.IsNullOrEmpty(TextBoxOutputTextOverlayImage.Text) ? StreamDeckConstants.DCSBIOSValuePlaceHolder : StreamDeckConstants.DCSBIOSValuePlaceHolder + " " + TextBoxOutputTextOverlayImage.Text;
                    TextBoxOutputTextOverlayImage.CaretIndex = TextBoxOutputTextOverlayImage.Text.Length;
                    SetFormState();
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void ButtonBrowseOverlayImage_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var path = "";
                var directory = Settings.Default.LastFileDialogLocation;

                var dialogResult = StreamDeckCommon.BrowseForImage(ref directory, ref path);

                if (dialogResult == System.Windows.Forms.DialogResult.OK)
                {
                    TextBoxOverlayImage.Bill.ImageFilePath = path;
                    Settings.Default.LastFileDialogLocation = directory;
                    SetIsDirty();
                    SetFormState();
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void ButtonTestSelectOverlayImage_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var bitmap = BitMapCreator.CreateStreamDeckBitmap("", TextBoxOverlayImage.Bill.TextFont, TextBoxOverlayImage.Bill.FontColor, TextBoxOverlayImage.Bill.BackgroundColor, TextBoxOverlayImage.Bill.OffsetX, TextBoxOverlayImage.Bill.OffsetY);
                _streamDeckPanel.SetImage(_streamDeckButtonName, bitmap);
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void RepeatButtonActionPressUpAsIs_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                TextBoxOutputTextAsIs.Bill.OffsetY += StreamDeckConstants.ADJUST_OFFSET_CHANGE_VALUE;
                TestImage(TextBoxOutputTextAsIs);
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void RepeatButtonActionPressDownAsIs_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                TextBoxOutputTextAsIs.Bill.OffsetY -= StreamDeckConstants.ADJUST_OFFSET_CHANGE_VALUE;
                TestImage(TextBoxOutputTextAsIs);
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void RepeatButtonActionPressLeftAsIs_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                TextBoxOutputTextAsIs.Bill.OffsetX -= StreamDeckConstants.ADJUST_OFFSET_CHANGE_VALUE;
                TestImage(TextBoxOutputTextAsIs);
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void RepeatButtonActionPressRightAsIs_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                TextBoxOutputTextAsIs.Bill.OffsetX += StreamDeckConstants.ADJUST_OFFSET_CHANGE_VALUE;
                TestImage(TextBoxOutputTextAsIs);
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void TestImage(StreamDeckFaceTextBox textBox)
        {
            var bitmap = BitMapCreator.CreateStreamDeckBitmap(textBox.Text, textBox.Bill.TextFont, textBox.Bill.FontColor, textBox.Bill.BackgroundColor, textBox.Bill.OffsetX, textBox.Bill.OffsetY);
            _streamDeckPanel.SetImage(_streamDeckButtonName, bitmap);
        }

        private void ButtonTextFaceFontAsIs_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var font = Settings.Default.ButtonTextFaceFont;

                if (StreamDeckCommon.SetFontStyle(ref font) == System.Windows.Forms.DialogResult.OK)
                {
                    _dcsbiosConverter.TextFont = font;
                }
                TestImage(TextBoxOutputTextAsIs);
                SetFormState();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void ButtonTextFaceFontColorAsIs_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var font = Settings.Default.ButtonTextFaceFont;

                if (StreamDeckCommon.SetFontStyle(ref font) == System.Windows.Forms.DialogResult.OK)
                {
                    _dcsbiosConverter.TextFont = font;
                }
                TestImage(TextBoxOutputTextAsIs);
                SetFormState();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void ButtonTextFaceBackgroundColorAsIs_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {

            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void ButtonTestTextFaceAsIs_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {

            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void RepeatButtonActionPressUpOverlay_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {

            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void RepeatButtonActionPressDownOverlay_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {

            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void RepeatButtonActionPressLeftOverlay_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {

            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void RepeatButtonActionPressRightOverlay_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {

            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void ButtonTextFaceFontOverlay_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {

            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void ButtonTextFaceFontColorOverlay_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {

            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void ButtonTextFaceBackgroundColorOverlay_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {

            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void ButtonTestTextFaceOverlay_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {

            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }
    }
}
