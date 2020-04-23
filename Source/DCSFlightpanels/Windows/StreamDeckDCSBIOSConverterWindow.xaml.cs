using System;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
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
    public partial class StreamDeckDCSBIOSConverterWindow : Window, IIsDirty
    {
        private bool _isLoaded = false;
        private DCSBIOSConverter _dcsbiosConverter = new DCSBIOSConverter();
        private bool _isDirty;
        private bool _isPopulatingData = false;
        private StreamDeckPanel _streamDeckPanel;
        private string _streamDeckPanelInstanceId;
        private EnumStreamDeckButtonNames _streamDeckButtonName;



        public StreamDeckDCSBIOSConverterWindow(EnumStreamDeckButtonNames streamDeckButtonName, string streamDeckPanelInstanceId)
        {
            InitializeComponent();
            _streamDeckButtonName = streamDeckButtonName;
            _streamDeckPanelInstanceId = streamDeckPanelInstanceId;
            _streamDeckPanel = StreamDeckPanel.GetInstance(_streamDeckPanelInstanceId);
        }

        public StreamDeckDCSBIOSConverterWindow(EnumStreamDeckButtonNames streamDeckButtonName, string streamDeckPanelInstanceId, DCSBIOSConverter dcsbiosConverter)
        {
            InitializeComponent();
            _streamDeckButtonName = streamDeckButtonName;
            _streamDeckPanelInstanceId = streamDeckPanelInstanceId;
            _streamDeckPanel = StreamDeckPanel.GetInstance(_streamDeckPanelInstanceId);
            _dcsbiosConverter = dcsbiosConverter;
        }

        private void StreamDeckDCSBIOSConverterWindow_OnLoaded(object sender, RoutedEventArgs e)
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
                if (ComboBoxItemAlways1.IsEnabled)
                {
                    StackPanelSecondCriteria.Visibility = Visibility.Collapsed;
                    StackPanelAddSecondCriteria.Visibility = Visibility.Visible;
                }

                var criteria1DataOK = !string.IsNullOrEmpty(TextBoxReferenceValue1.Text) && SelectedComparator1 != EnumComparator.None;
                var criteria2DataOK = !string.IsNullOrEmpty(TextBoxReferenceValue2.Text) && SelectedComparator2 != EnumComparator.None;

                var criteriaOK = (Use2Criteria ? criteria2DataOK : true) && criteria1DataOK;


                //if(Use2Criteria)
                //ButtonOk.IsEnabled = ;
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

        public DCSBIOSConverter DCSBIOSConverter => _dcsbiosConverter;

        private void TextBoxReferenceValue_OnKeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                if (!_isLoaded)
                {
                    return;
                }

                if (sender.Equals(TextBoxReferenceValue1))
                {
                    if (!TextBoxReferenceValue1.ValidateDouble(true))
                    {
                        SetIsDirty();
                        return;
                    }
                    _dcsbiosConverter.ReferenceValue1 = double.Parse(TextBoxReferenceValue1.Text);
                    SetFormState();
                    SetIsDirty();
                }
                if (sender.Equals(TextBoxReferenceValue2))
                {
                    if (!TextBoxReferenceValue2.ValidateDouble(true ))
                    {
                        SetIsDirty();
                        return;
                    }
                    _dcsbiosConverter.ReferenceValue2 = double.Parse(TextBoxReferenceValue2.Text);
                    SetFormState();
                    SetIsDirty();
                }
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

        private void LabelInsert_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                TextBox textBox = null;
                if (sender.Equals(LabelInsertAsIs))
                {
                    textBox = TextBoxOutputTextAsIs;
                }
                if (sender.Equals(LabelInsertOverlayImage))
                {
                    textBox = TextBoxOutputTextOverlayImage;
                }
                if (textBox != null && !textBox.Text.Contains(StreamDeckConstants.DCSBIOSValuePlaceHolder))
                {
                    textBox.Text = string.IsNullOrEmpty(textBox.Text) ? StreamDeckConstants.DCSBIOSValuePlaceHolder : StreamDeckConstants.DCSBIOSValuePlaceHolder + " " + textBox.Text;
                    textBox.CaretIndex = textBox.Text.Length;
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
                case EnumConverterOutputType.Raw:
                    {
                        StackPanelAsIs.Visibility = Visibility.Visible;
                        StackPanelImage.Visibility = Visibility.Collapsed;
                        StackPanelOverlayImage.Visibility = Visibility.Collapsed;
                        break;
                    }
                case EnumConverterOutputType.Image:
                    {
                        StackPanelAsIs.Visibility = Visibility.Collapsed;
                        StackPanelImage.Visibility = Visibility.Visible;
                        StackPanelOverlayImage.Visibility = Visibility.Collapsed;
                        break;
                    }
                case EnumConverterOutputType.ImageOverlay:
                    {
                        StackPanelAsIs.Visibility = Visibility.Collapsed;
                        StackPanelImage.Visibility = Visibility.Collapsed;
                        StackPanelOverlayImage.Visibility = Visibility.Visible;
                        break;
                    }
            }


//            SetComboBoxValue();
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
                ComboBoxItemAlways1.IsEnabled = true;
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
                _dcsbiosConverter.OutputType = EnumConverterOutputType.Raw;
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
                _dcsbiosConverter.OutputType = EnumConverterOutputType.Image;
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
                _dcsbiosConverter.OutputType = EnumConverterOutputType.ImageOverlay;
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
                var imageRelativePath = "";
                var directory = Settings.Default.LastFileDialogLocation;

                var dialogResult = StreamDeckCommon.BrowseForImage(ref directory, ref imageRelativePath);

                if (dialogResult == System.Windows.Forms.DialogResult.OK)
                {
                    _dcsbiosConverter.ImageFileRelativePath = imageRelativePath;
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

        private void ButtonTestImage_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                StreamDeckPanel.GetInstance(_streamDeckPanelInstanceId).SetImage(_streamDeckButtonName, _dcsbiosConverter.Bitmap);
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
                _dcsbiosConverter.ButtonText = ((TextBox) sender).Text;
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void RepeatButtonActionPressUp_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                _dcsbiosConverter.OffsetY += StreamDeckConstants.ADJUST_OFFSET_CHANGE_VALUE;
                TestImage();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void RepeatButtonActionPressDown_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                _dcsbiosConverter.OffsetY -= StreamDeckConstants.ADJUST_OFFSET_CHANGE_VALUE;
                TestImage();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void RepeatButtonActionPressLeft_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                _dcsbiosConverter.OffsetX -= StreamDeckConstants.ADJUST_OFFSET_CHANGE_VALUE;
                TestImage();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void RepeatButtonActionPressRight_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                _dcsbiosConverter.OffsetX += StreamDeckConstants.ADJUST_OFFSET_CHANGE_VALUE;
                TestImage();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void TestImage()
        {
            _streamDeckPanel.SetImage(_streamDeckButtonName, _dcsbiosConverter.Bitmap);
        }

        private void ButtonTextFaceFont_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var font = Settings.Default.ButtonTextFaceFont;

                if (StreamDeckCommon.SetFontStyle(ref font) == System.Windows.Forms.DialogResult.OK)
                {
                    _dcsbiosConverter.TextFont = font;
                    TestImage();
                    UpdateFontInfo();
                    SetFormState();
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void ButtonTextFaceFontColor_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var color = Color.Transparent;

                if (StreamDeckCommon.SetFontColor(ref color) == System.Windows.Forms.DialogResult.OK)
                {
                    _dcsbiosConverter.FontColor = color;
                    SetIsDirty();
                    UpdateFontInfo();
                    SetFormState();
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void ButtonTextFaceBackgroundColor_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var color = Color.Transparent;

                if (StreamDeckCommon.SetFontColor(ref color) == System.Windows.Forms.DialogResult.OK)
                {
                    _dcsbiosConverter.BackgroundColor = color;
                    SetIsDirty();
                    UpdateFontInfo();
                    SetFormState();
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void UpdateFontInfo()
        {
            TextBoxFontInfo.Text = "Font : " + _dcsbiosConverter.TextFont.Name + " " +
                                   _dcsbiosConverter.TextFont.Size + " " +
                                   (_dcsbiosConverter.TextFont.Bold ? "Bold" : "Regular");
            TextBoxFontInfo.Text = TextBoxFontInfo.Text + "\n" + "Font Color : " + _dcsbiosConverter.FontColor.ToString();
            TextBoxFontInfo.Text = TextBoxFontInfo.Text + "\n" + "Background Color : " + _dcsbiosConverter.BackgroundColor.ToString();
        }

        private bool Use2Criteria
        {
            get => StackPanelSecondCriteria.Visibility == Visibility.Visible;
        }

        private EnumComparator SelectedComparator1 => (EnumComparator)ComboBoxComparisonType1.SelectedItem;

        private EnumComparator SelectedComparator2 => (EnumComparator)ComboBoxComparisonType2.SelectedItem;

        private EnumConverterOutputType OutputType
        {
            get
            {
                if (RadioButtonDCSBIOSValue.IsChecked == true)
                {
                    return EnumConverterOutputType.Raw;
                }
                if (RadioButtonImage.IsChecked == true)
                {
                    return EnumConverterOutputType.Image;
                }
                if (RadioButtonDCSBIOSValue.IsChecked == true)
                {
                    return EnumConverterOutputType.ImageOverlay;
                }
                return EnumConverterOutputType.NotSet;
            }
        }

        private bool FaceConfigurationIsOK()
        {
            var result = false;
            switch (OutputType)
            {
                case EnumConverterOutputType.Raw:
                {

                    break;
                }
                case EnumConverterOutputType.Image:
                {
                    break;
                }
                case EnumConverterOutputType.ImageOverlay:
                {
                    break;
                }
            }

            return result;
        }
    }
}
