using NonVisuals.StreamDeck.Panels;

namespace DCSFlightpanels.Windows.StreamDeck
{
    using System;
    using System.Drawing;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Documents;
    using System.Windows.Input;
    using ClassLibraryCommon;
    using DCSFlightpanels.Shared;
    using MEF;
    using NonVisuals;
    using NonVisuals.Interfaces;
    using NonVisuals.StreamDeck;

    public partial class StreamDeckDCSBIOSConverterWindow : Window, IIsDirty
    {
        private readonly StreamDeckPanel _streamDeckPanel;
        private readonly EnumStreamDeckButtonNames _streamDeckButtonName;
        private bool _isLoaded = false;
        private DCSBIOSConverter _dcsbiosConverter = null;
        private bool _isDirty;

        public StreamDeckDCSBIOSConverterWindow(EnumStreamDeckButtonNames streamDeckButtonName, StreamDeckPanel streamDeckPanel)
        {
            InitializeComponent();
            _streamDeckButtonName = streamDeckButtonName;
            _streamDeckPanel = streamDeckPanel;
            _dcsbiosConverter = new DCSBIOSConverter(_streamDeckPanel);
        }

        public StreamDeckDCSBIOSConverterWindow(EnumStreamDeckButtonNames streamDeckButtonName, DCSBIOSConverter dcsbiosConverter, StreamDeckPanel streamDeckPanel)
        {
            InitializeComponent();
            _streamDeckButtonName = streamDeckButtonName;
            _streamDeckPanel = streamDeckPanel;
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
                _isLoaded = true;
                SetFormState();
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
                if (!_isLoaded)
                {
                    return;
                }

                if (_dcsbiosConverter.ConverterOutputType == EnumConverterOutputType.NotSet)
                {
                    StackPanelRaw.Visibility = Visibility.Collapsed;
                    StackPanelImage.Visibility = Visibility.Collapsed;
                    StackPanelOverlayImage.Visibility = Visibility.Collapsed;
                }

                if (StreamDeckCommon.GetComparatorValue(ComboBoxComparisonType1.Text) == EnumComparator.NotSet || StreamDeckCommon.GetComparatorValue(ComboBoxComparisonType1.Text) == EnumComparator.Always)
                {
                    StackPanelSecondCriteria.Visibility = Visibility.Collapsed;
                    StackPanelAddSecondCriteria.Visibility = Visibility.Visible;
                }
                ButtonAddSecondCriteria.IsEnabled = ComboBoxComparisonType1.SelectedItem.Equals(ComboBoxItemAlways1) == false;

                var criteria1DataOK = !string.IsNullOrEmpty(TextBoxReferenceValue1.Text) && SelectedComparator1 != EnumComparator.NotSet;
                var criteria2DataOK = !string.IsNullOrEmpty(TextBoxReferenceValue2.Text) && SelectedComparator2 != EnumComparator.NotSet;

                var referenceValuesOK = Use2Criteria ? double.TryParse(TextBoxReferenceValue1.Text, out var result1) && double.TryParse(TextBoxReferenceValue2.Text, out var result2) : double.TryParse(TextBoxReferenceValue1.Text, out var result3);



                var criteriaOK = (!Use2Criteria || criteria2DataOK) && criteria1DataOK && referenceValuesOK;

                ButtonOk.IsEnabled = criteriaOK && _dcsbiosConverter.FaceConfigurationIsOK && IsDirty;
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
                CancelWindow();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }
        
        private void CancelWindow()
        {
            if (CommonUI.DoDiscardAfterMessage(_isDirty))
            {
                DialogResult = false;
                Close();
            }
        }

        private void ButtonOk_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                _dcsbiosConverter.Clean();
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }
        
        public DCSBIOSConverter DCSBIOSConverter
        {
            get => _dcsbiosConverter;
            set => _dcsbiosConverter = value;
        }

        private string _lastChecked1 = string.Empty;
        private string _lastChecked2 = string.Empty;
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
                    if (_lastChecked1 != TextBoxReferenceValue1.Text && !TextBoxReferenceValue1.ValidateDouble(true))
                    {
                        _lastChecked1 = TextBoxReferenceValue1.Text;
                        return;
                    }

                    //this is necessary?  var validateResult2 = double.TryParse(TextBoxReferenceValue1.Text, NumberStyles.Number, StreamDeckConstants.DoubleCultureInfo, out _);
                    if (double.TryParse(TextBoxReferenceValue1.Text,NumberStyles.Number, StreamDeckConstants.DoubleCultureInfo, out var result))
                    {
                        _dcsbiosConverter.ReferenceValue1 = result;
                        SetIsDirty();
                    }
                }
                if (sender.Equals(TextBoxReferenceValue2))
                {
                    if (_lastChecked2 != TextBoxReferenceValue2.Text && !TextBoxReferenceValue2.ValidateDouble(true))
                    {
                        _lastChecked2 = TextBoxReferenceValue2.Text;
                        return;
                    }

                    if (double.TryParse(TextBoxReferenceValue2.Text, out var result))
                    {
                        _dcsbiosConverter.ReferenceValue2 = result;
                        SetIsDirty();
                    }
                }
                SetFormState();
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
                if (sender.Equals(LabelInsertRaw))
                {
                    textBox = TextBoxOutputButtonTextRaw;
                }
                if (sender.Equals(LabelInsertOverlayImage))
                {
                    textBox = TextBoxOutputButtonTextOverlayImage;
                }
                if (textBox != null && !textBox.Text.Contains(StreamDeckConstants.DCSBIOSValuePlaceHolder))
                {
                    textBox.Text = string.IsNullOrEmpty(textBox.Text) ? StreamDeckConstants.DCSBIOSValuePlaceHolder : StreamDeckConstants.DCSBIOSValuePlaceHolder + " " + textBox.Text;
                    textBox.CaretIndex = textBox.Text.Length;
                    
                    _dcsbiosConverter.ButtonTextTemplate = textBox.Text;
                    SetIsDirty();
                    SetFormState();
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void SetInfoTextBoxes()
        {
            TextBoxFontInfo.TargetFont = _dcsbiosConverter.TextFont;
            TextBoxFontInfo.TargetFontColor = _dcsbiosConverter.FontColor;
            TextBoxFontInfo.TargetBackgroundColor = _dcsbiosConverter.BackgroundColor;

            TextBoxOffsetInfo.OffSetX = _dcsbiosConverter.OffsetX;
            TextBoxOffsetInfo.OffSetY = _dcsbiosConverter.OffsetY;
        }

        private void ShowConverter()
        {
            if (_dcsbiosConverter.Comparator2 == EnumComparator.NotSet)
            {
                HideSecondCriteria();
            }

            TextBoxReferenceValue1.Text = _dcsbiosConverter.ReferenceValue1.ToString(CultureInfo.InvariantCulture);
            TextBoxReferenceValue2.Text = _dcsbiosConverter.ReferenceValue2.ToString(CultureInfo.InvariantCulture);

            SetInfoTextBoxes();

            switch (_dcsbiosConverter.ConverterOutputType)
            {
                case EnumConverterOutputType.NotSet:
                    {
                        StackPanelRaw.Visibility = Visibility.Collapsed;
                        StackPanelImage.Visibility = Visibility.Collapsed;
                        StackPanelOverlayImage.Visibility = Visibility.Collapsed;
                        break;
                    }
                case EnumConverterOutputType.Raw:
                    {
                        StackPanelRaw.Visibility = Visibility.Visible;
                        StackPanelImage.Visibility = Visibility.Collapsed;
                        StackPanelOverlayImage.Visibility = Visibility.Collapsed;
                        RadioButtonDCSBIOSValue.IsChecked = true;
                        TextBoxOutputButtonTextRaw.Text = _dcsbiosConverter.ButtonTextTemplate.ToString(CultureInfo.InvariantCulture);
                        break;
                    }
                case EnumConverterOutputType.Image:
                    {
                        StackPanelRaw.Visibility = Visibility.Collapsed;
                        StackPanelImage.Visibility = Visibility.Visible;
                        StackPanelOverlayImage.Visibility = Visibility.Collapsed;
                        RadioButtonImage.IsChecked = true;
                        TextBoxImagePath.Text = _dcsbiosConverter.ImageFileRelativePath;
                        break;
                    }
                case EnumConverterOutputType.ImageOverlay:
                    {
                        StackPanelRaw.Visibility = Visibility.Collapsed;
                        StackPanelImage.Visibility = Visibility.Collapsed;
                        StackPanelOverlayImage.Visibility = Visibility.Visible;
                        RadioButtonOverlayImage.IsChecked = true;
                        TextBoxOutputButtonTextOverlayImage.Text = _dcsbiosConverter.ButtonTextTemplate.ToString(CultureInfo.InvariantCulture);
                        TextBoxOverlayImagePath.Text = _dcsbiosConverter.ImageFileRelativePath;
                        break;
                    }
            }

            StreamDeckCommon.SetComparatorValue(ComboBoxComparisonType1, _dcsbiosConverter.Comparator1);
            StreamDeckCommon.SetComparatorValue(ComboBoxComparisonType2, _dcsbiosConverter.Comparator2);
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
                infoWindow.AddInline(new Run("0°-359°.\n") { FontWeight = FontWeights.Bold });
                infoWindow.AddInline("This is the formula for the A-10C HSI heading:\n");
                infoWindow.AddInline(new Run("360 - truncate(HSI_HDG*360/65535)\n") { FontWeight = FontWeights.Bold });
                infoWindow.AddInline("\n");
                infoWindow.AddInline("Some DCS-BIOS values are useless in the number format, one example is a radio's dial position.\n");
                infoWindow.AddInline("\n");
                infoWindow.AddInline("Here you can convert DCS-BIOS values to have unique images / texts based on DCS-BIOS value.\n");
                infoWindow.AddInline("A radio can based on a dial's position show texts : OFF MANUAL PRESET and so on.\n");
                infoWindow.AddInline("If you want to show units after the value you can do this here too. Choose comparison mode \"Always\" for \n");
                infoWindow.AddInline(new Run("7400 RPM\n") { FontWeight = FontWeights.Bold });
                infoWindow.AddInline(new Run("242°\n") { FontWeight = FontWeights.Bold });
                infoWindow.AddInline(new Run("180°C\n") { FontWeight = FontWeights.Bold });
                infoWindow.AddInline("\n");
                infoWindow.AddInline("You can also have different colors for different temperature ranges.");
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
                ComboBox comboBox;
                if (sender.Equals(ComboBoxComparisonType1))
                {
                    comboBox = ComboBoxComparisonType1;
                    if (_dcsbiosConverter.Comparator1 != StreamDeckCommon.GetComparatorValue(comboBox.Text))
                    {
                        _dcsbiosConverter.Comparator1 = StreamDeckCommon.GetComparatorValue(comboBox.Text);
                        SetIsDirty();
                    }
                }
                else
                {
                    comboBox = ComboBoxComparisonType2;
                    if (_dcsbiosConverter.Comparator2 != StreamDeckCommon.GetComparatorValue(comboBox.Text))
                    {
                        _dcsbiosConverter.Comparator2 = StreamDeckCommon.GetComparatorValue(comboBox.Text);
                        SetIsDirty();
                    }
                }

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
                HideSecondCriteria();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void HideSecondCriteria()
        {
            StackPanelAddSecondCriteria.Visibility = Visibility.Visible;
            StackPanelSecondCriteria.Visibility = Visibility.Collapsed;
            ComboBoxItemAlways1.IsEnabled = true;
            _dcsbiosConverter.Comparator2 = EnumComparator.NotSet;
            _dcsbiosConverter.ReferenceValue2 = 0;
        }

        private void RadioButtonDCSBIOSValue_OnChecked(object sender, RoutedEventArgs e)
        {
            try
            {
                StackPanelRaw.Visibility = Visibility.Visible;
                StackPanelImage.Visibility = Visibility.Collapsed;
                StackPanelOverlayImage.Visibility = Visibility.Collapsed;
                _dcsbiosConverter.ConverterOutputType = EnumConverterOutputType.Raw;
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
                StackPanelRaw.Visibility = Visibility.Collapsed;
                StackPanelImage.Visibility = Visibility.Visible;
                StackPanelOverlayImage.Visibility = Visibility.Collapsed;
                _dcsbiosConverter.ConverterOutputType = EnumConverterOutputType.Image;
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
                StackPanelRaw.Visibility = Visibility.Collapsed;
                StackPanelImage.Visibility = Visibility.Collapsed;
                StackPanelOverlayImage.Visibility = Visibility.Visible;
                _dcsbiosConverter.ConverterOutputType = EnumConverterOutputType.ImageOverlay;
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
                TextBox textBox;
                if (sender.Equals(ButtonBrowseImage))
                {
                    textBox = TextBoxImagePath;
                }
                else
                {
                    textBox = TextBoxOverlayImagePath;
                }
                var imageRelativePath = string.Empty;
                var directory = SettingsManager.LastImageFileDirectory;

                var dialogResult = StreamDeckUICommon.BrowseForImage(ref directory, ref imageRelativePath);

                if (dialogResult == System.Windows.Forms.DialogResult.OK)
                {
                    _dcsbiosConverter.ImageFileRelativePath = imageRelativePath;
                    textBox.Text = imageRelativePath;
                    SettingsManager.LastImageFileDirectory = directory;
                    SetIsDirty();
                    SetFormState();
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void ButtonTestDCSBIOSDecoder_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                _streamDeckPanel.SetImage(_streamDeckButtonName, _dcsbiosConverter.Bitmap);
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
                _dcsbiosConverter.ButtonTextTemplate = ((TextBox) sender).Text;
                SetIsDirty();
                SetFormState();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void TextBoxOutputButtonText_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                SetInfoTextBoxes();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void RepeatButtonPressUp_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                _dcsbiosConverter.OffsetY -= StreamDeckConstants.ADJUST_OFFSET_CHANGE_VALUE;
                SettingsManager.OffsetY = _dcsbiosConverter.OffsetY;
                TestImage();
                SetIsDirty();
                SetFormState();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void RepeatButtonPressDown_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                _dcsbiosConverter.OffsetY += StreamDeckConstants.ADJUST_OFFSET_CHANGE_VALUE;
                SettingsManager.OffsetY = _dcsbiosConverter.OffsetY;
                TestImage();
                SetIsDirty();
                SetFormState();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void RepeatButtonPressLeft_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                _dcsbiosConverter.OffsetX -= StreamDeckConstants.ADJUST_OFFSET_CHANGE_VALUE;
                SettingsManager.OffsetX = _dcsbiosConverter.OffsetX;
                TestImage();
                SetIsDirty();
                SetFormState();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void RepeatButtonPressRight_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                _dcsbiosConverter.OffsetX += StreamDeckConstants.ADJUST_OFFSET_CHANGE_VALUE;
                SettingsManager.OffsetX = _dcsbiosConverter.OffsetX;
                TestImage();
                SetIsDirty();
                SetFormState();
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
                var font = SettingsManager.DefaultFont;

                if (StreamDeckUICommon.SetFontStyle(ref font) == System.Windows.Forms.DialogResult.OK)
                {
                    _dcsbiosConverter.TextFont = font;
                    TextBoxFontInfo.TargetFont = font;
                    SettingsManager.DefaultFont = font;
                    TestImage();
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

                if (StreamDeckUICommon.SetFontColor(ref color) == System.Windows.Forms.DialogResult.OK)
                {
                    _dcsbiosConverter.FontColor = color;
                    TextBoxFontInfo.TargetFontColor = color;
                    SettingsManager.DefaultFontColor = color;
                    SetIsDirty();
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

                if (StreamDeckUICommon.SetFontColor(ref color) == System.Windows.Forms.DialogResult.OK)
                {
                    _dcsbiosConverter.BackgroundColor = color;
                    TextBoxFontInfo.TargetBackgroundColor = color;
                    SettingsManager.DefaultBackgroundColor = color;
                    SetIsDirty();
                    SetFormState();
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private bool Use2Criteria
        {
            get => StackPanelSecondCriteria.Visibility == Visibility.Visible;
        }

        private EnumComparator SelectedComparator1 => StreamDeckCommon.GetComparatorValue(ComboBoxComparisonType1.Text);

        private EnumComparator SelectedComparator2 => StreamDeckCommon.GetComparatorValue(ComboBoxComparisonType2.Text);


        private void StreamDeckDCSBIOSConverterWindow_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                e.Handled = true;
                CancelWindow();
            }
        }

    }
}
