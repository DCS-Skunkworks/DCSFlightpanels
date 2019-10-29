using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using ClassLibraryCommon;
using DCSFlightpanels.Properties;
using DCSFlightpanels.Bills;
using DCSFlightpanels.CustomControls;
using NonVisuals.Interfaces;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using TextBox = System.Windows.Controls.TextBox;
using NonVisuals.StreamDeck;
using RadioButton = System.Windows.Controls.RadioButton;

namespace DCSFlightpanels.PanelUserControls
{
    /// <summary>
    /// Interaction logic for UserControlStreamDeckButtonFace.xaml
    /// </summary>
    public partial class UserControlStreamDeckButtonFace : UserControlBase
    {
        private IGlobalHandler _globalHandler;
        private bool _isDirty = false;
        private bool _isLoaded = false;
        private List<StreamDeckFaceTextBox> _textBoxList = new List<StreamDeckFaceTextBox>();
        private List<RadioButton> _radioButtonList = new List<RadioButton>();







        public UserControlStreamDeckButtonFace()
        {
            InitializeComponent();
        }

        private void UserControlStreamDeckButtonFace_OnLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_isLoaded)
                {
                    return;
                }
                FillControlLists();
                SetBills();
                LoadFontSettings();
                SetFormState();
                _isLoaded = true;
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        public void SetFormState()
        {
            try
            {
                RadioButtonDCSBIOSFace.Visibility = SDUIParent.GetSelectedActionType() != EnumStreamDeckActionType.LayerNavigation ? Visibility.Visible : Visibility.Collapsed;

                StackPanelButtonTextAndStyle.Visibility = RadioButtonTextFace.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
                StackPanelButtonImageToShow.Visibility = RadioButtonImageFace.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
                StackPanelButtonDCSBIOSImage.Visibility = RadioButtonDCSBIOSFace.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;

                StackPanelDCSBIOSBackgroundType.Visibility = RadioButtonDCSBIOSFace?.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;

                StackPanelButtonDCSBIOSBackgroundGeneratedImage.Visibility = RadioButtonDCSBIOSBackgroundGenerated.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
                StackPanelButtonDCSBIOSBackgroundExistingImage.Visibility = RadioButtonDCSBIOSBackgroundExisting.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;

                StackPanelButtonImageType.Visibility = SDUIParent.GetSelectedActionType() != EnumStreamDeckActionType.LayerNavigation ? Visibility.Visible : Visibility.Collapsed;

                StackPanelDCSBIOSBackgroundTypeSelection.Visibility = RadioButtonDCSBIOSFace.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
                StackPanelButtonDCSBIOSBackgroundGeneratedImage.Visibility = RadioButtonDCSBIOSBackgroundGenerated.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
                StackPanelButtonDCSBIOSBackgroundExistingImage.Visibility = RadioButtonDCSBIOSBackgroundExisting.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;

                ButtonOnTextFaceFont.IsEnabled = !string.IsNullOrEmpty(TextBoxButtonOnTextFace.Text);
                ButtonOnTextFaceFontColor.IsEnabled = !string.IsNullOrEmpty(TextBoxButtonOnTextFace.Text);
                ButtonOnTextFaceBackgroundColor.IsEnabled = !string.IsNullOrEmpty(TextBoxButtonOnTextFace.Text);
                ButtonOnTestTextFace.IsEnabled = !string.IsNullOrEmpty(TextBoxButtonOnTextFace.Text);

                ButtonOffTextFaceFont.IsEnabled = !string.IsNullOrEmpty(TextBoxButtonOffTextFace.Text);
                ButtonOffTextFaceFontColor.IsEnabled = !string.IsNullOrEmpty(TextBoxButtonOffTextFace.Text);
                ButtonOffTextFaceBackgroundColor.IsEnabled = !string.IsNullOrEmpty(TextBoxButtonOffTextFace.Text);
                ButtonOffTestTextFace.IsEnabled = !string.IsNullOrEmpty(TextBoxButtonOffTextFace.Text);

                /*
                 * Not yet implemented
                 */
                RadioButtonImageFace.IsEnabled = false;
                RadioButtonDCSBIOSFace.IsEnabled = false;
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(471473, ex);
            }
        }


        public void Update()
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

        public void Clear()
        {
            foreach (var textBox in _textBoxList)
            {
                textBox.Bill.Clear();
            }

            foreach (var radioButton in _radioButtonList)
            {
                radioButton.IsChecked = false;
            }
            _isDirty = false;
        }

        private void MouseDownFocusLogTextBox(object sender, MouseButtonEventArgs e)
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

        private void RadioButtonDCSBIOSImage_OnClick(object sender, RoutedEventArgs e)
        {
            SetFormState();
        }

        private void TextBoxLostFocus(object sender, RoutedEventArgs e)
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

        private void TextBoxShortcutKeyDown(object sender, KeyEventArgs e)
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

        private void TextBoxGotFocus(object sender, RoutedEventArgs e)
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

        private void TextBoxMouseDoubleClick(object sender, MouseButtonEventArgs e)
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

        private void TextBoxPreviewKeyDown(object sender, KeyEventArgs e)
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

        private void TextBoxTextChanged(object sender, TextChangedEventArgs e)
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

        private void TextBox_OnMouseDown(object sender, MouseButtonEventArgs e)
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

        public EnumStreamDeckFaceType GetSelectedFaceType()
        {
            if (RadioButtonTextFace.IsChecked == true)
            {
                return EnumStreamDeckFaceType.Text;
            }
            if (RadioButtonImageFace.IsChecked == true)
            {
                return EnumStreamDeckFaceType.ImageFile;
            }
            if (RadioButtonDCSBIOSFace.IsChecked == true)
            {
                return EnumStreamDeckFaceType.DCSBIOS;
            }

            return EnumStreamDeckFaceType.Unknown;
        }

        private void RadioButtonFaceType_OnClick(object sender, RoutedEventArgs e)
        {
            SetFormState();
        }

        public IGlobalHandler GlobalHandler
        {
            get => _globalHandler;
            set => _globalHandler = value;
        }

        public void StateSaved()
        {
            _isDirty = false;
        }

        private void SetIsDirty()
        {
            _isDirty = true;
        }

        public bool IsDirty
        {
            get => _isDirty;
            set => _isDirty = value;
        }

        private void FillControlLists()
        {
            _textBoxList.Add(TextBoxButtonOnTextFace);
            _textBoxList.Add(TextBoxButtonOffTextFace);
            _textBoxList.Add(TextBoxDCSBIOSFaceButtonOn);
            _textBoxList.Add(TextBoxDCSBIOSFaceButtonOff);
            _textBoxList.Add(TextBoxDCSBIOSBackgroundImageButtonOn);
            _textBoxList.Add(TextBoxDCSBIOSBackgroundImageButtonOff);
            _textBoxList.Add(TextBoxDCSBIOSFaceUnit);
            _textBoxList.Add(TextBoxSelectedImageFaceButtonOn);
            _textBoxList.Add(TextBoxSelectedImageFaceButtonOff);

            _radioButtonList.Add(RadioButtonTextFace);
            _radioButtonList.Add(RadioButtonImageFace);
            _radioButtonList.Add(RadioButtonDCSBIOSFace);
            _radioButtonList.Add(RadioButtonDCSBIOSBackgroundGenerated);
            _radioButtonList.Add(RadioButtonDCSBIOSBackgroundExisting);
        }

        private void SetBills()
        {
            foreach (var textBox in _textBoxList)
            {
                textBox.Bill = new BillStreamDeckFace();
                textBox.Bill.ParentTextBox = textBox;
            }
        }

        private void ButtonOnTextFaceFont_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                SetFontStyle(TextBoxButtonOnTextFace);
                TestImage(TextBoxButtonOnTextFace);
                SetFormState();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void ButtonOnTextFaceFontColor_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                SetFontColor(TextBoxButtonOnTextFace);
                TestImage(TextBoxButtonOnTextFace);
                SetFormState();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void ButtonOnTextFaceBackgroundColor_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                SetBackgroundColor(TextBoxButtonOnTextFace);
                TestImage(TextBoxButtonOnTextFace);
                SetFormState();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void ButtonOnTestTextFace_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                TestImage(TextBoxButtonOnTextFace);
                SetFormState();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void ButtonOffTextFaceFont_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                SetFontStyle(TextBoxButtonOffTextFace);
                TestImage(TextBoxButtonOffTextFace);
                SetFormState();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void ButtonOffTextFaceFontColor_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                SetFontColor(TextBoxButtonOffTextFace);
                TestImage(TextBoxButtonOffTextFace);
                SetFormState();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void ButtonOffTextFaceBackgroundColor_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                SetBackgroundColor(TextBoxButtonOffTextFace);
                TestImage(TextBoxButtonOffTextFace);
                SetFormState();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void ButtonOffTestTextFace_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                TestImage(TextBoxButtonOffTextFace);
                SetFormState();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void LoadFontSettings()
        {
            if (Settings.Default.ButtonTextFaceFont != null)
            {
                TextBoxButtonOnTextFace.Bill.TextFont = Settings.Default.ButtonTextFaceFont;
                TextBoxButtonOffTextFace.Bill.TextFont = Settings.Default.ButtonTextFaceFont;
            }
        }


        public bool HasConfig
        {
            get
            {
                switch (GetSelectedFaceType())
                {
                    case EnumStreamDeckFaceType.Text:
                        {
                            return TextBoxButtonOnTextFace.Bill.ContainsTextFace();
                        }
                    case EnumStreamDeckFaceType.ImageFile:
                        {
                            return false;
                        }
                    case EnumStreamDeckFaceType.DCSBIOS:
                        {
                            return false;
                        }
                }
                return false;
            }
        }

        public void ShowFaceConfiguration(StreamDeckButton streamDeckButton)
        {
            Clear();
            if (streamDeckButton == null)
            {
                return;
            }


            switch (streamDeckButton.FaceType)
            {
                case EnumStreamDeckFaceType.Text:
                    {
                        RadioButtonTextFace.IsChecked = true;
                        break;
                    }
                case EnumStreamDeckFaceType.ImageFile:
                    {
                        RadioButtonImageFace.IsChecked = true;
                        break;
                    }
                case EnumStreamDeckFaceType.DCSBIOS:
                    {
                        RadioButtonDCSBIOSFace.IsChecked = true;
                        break;
                    }
                case EnumStreamDeckFaceType.Unknown:
                    {
                        return;
                    }
                default:
                    {
                        return;
                    }
            }
            ShowFaceConfiguration(streamDeckButton.FaceForPress);
            ShowFaceConfiguration(streamDeckButton.FaceForRelease);

        }

        public void ShowFaceConfiguration(IStreamDeckButtonFace streamDeckButtonFace)
        {
            if (streamDeckButtonFace == null)
            {
                return;
            }

            switch (streamDeckButtonFace.FaceType)
            {
                case EnumStreamDeckFaceType.Text:
                    {
                        var faceTypeText = (FaceTypeText)streamDeckButtonFace;
                        var textBox = faceTypeText.WhenTurnedOn ? TextBoxButtonOnTextFace : TextBoxButtonOffTextFace;
                        textBox.Bill.TextFont = faceTypeText.TextFont;
                        textBox.Text = faceTypeText.Text;
                        textBox.Bill.FontColor = faceTypeText.FontColor;
                        textBox.Bill.BackgroundColor = faceTypeText.BackgroundColor;
                        SetFormState();
                        return;
                    }
                case EnumStreamDeckFaceType.ImageFile:
                    {
                        SetFormState();
                        return;
                    }
                case EnumStreamDeckFaceType.DCSBIOS:
                    {

                        SetFormState();
                        return;
                    }
            }

            throw new ArgumentException("ShowFaceConfiguration, failed to determine Face Type");
        }


        public IStreamDeckButtonFace GetStreamDeckButtonFace(bool forButtonPressed)
        {
            var textBoxTextFace = forButtonPressed ? TextBoxButtonOnTextFace : TextBoxButtonOffTextFace;

            switch (GetSelectedFaceType())
            {
                case EnumStreamDeckFaceType.Text:
                    {
                        if (textBoxTextFace.Bill.ContainsTextFace())
                        {
                            var result = new FaceTypeText();

                            result.WhenTurnedOn = forButtonPressed;
                            result.Text = textBoxTextFace.Text;
                            result.TextFont = textBoxTextFace.Bill.TextFont;
                            result.FontColor = textBoxTextFace.Bill.FontColor;
                            result.BackgroundColor = textBoxTextFace.Bill.BackgroundColor;
                            result.OffsetX = textBoxTextFace.Bill.OffsetX;
                            result.OffsetY = textBoxTextFace.Bill.OffsetY;

                            return result;
                        }

                        return null;
                    }
                case EnumStreamDeckFaceType.ImageFile:
                    {
                        return null;
                    }
                case EnumStreamDeckFaceType.DCSBIOS:
                    {
                        return null;
                    }
            }

            throw new ArgumentException("GetStreamDeckButtonFace, failed to determine Face Type");
        }

        private void SetFontStyle(StreamDeckFaceTextBox textBox)
        {
            var fontDialog = new FontDialog();

            fontDialog.FixedPitchOnly = true;
            fontDialog.FontMustExist = true;
            fontDialog.MinSize = 6;

            if (Settings.Default.ButtonTextFaceFont != null)
            {
                fontDialog.Font = Settings.Default.ButtonTextFaceFont;
            }

            var result = fontDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                textBox.Bill.TextFont = fontDialog.Font;

                Settings.Default.ButtonTextFaceFont = fontDialog.Font;
                Settings.Default.Save();

                SetIsDirty();
                SDUIParent.ChildChangesMade();
            }
        }

        private void SetFontColor(StreamDeckFaceTextBox textBox)
        {
            var colorDialog = new ColorDialog();
            colorDialog.Color = Settings.Default.ButtonTextFaceFontColor;

            var result = colorDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                textBox.Bill.FontColor = colorDialog.Color;

                Settings.Default.ButtonTextFaceFontColor = colorDialog.Color;
                Settings.Default.Save();

                SetIsDirty();
                SDUIParent.ChildChangesMade();
            }
        }

        private void SetBackgroundColor(StreamDeckFaceTextBox textBox)
        {
            var colorDialog = new ColorDialog();
            colorDialog.Color = Settings.Default.ButtonTextFaceBackgroundColor;

            var result = colorDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                textBox.Bill.BackgroundColor = colorDialog.Color;

                Settings.Default.ButtonTextFaceBackgroundColor = colorDialog.Color;
                Settings.Default.Save();

                SetIsDirty();
                SDUIParent.ChildChangesMade();
            }
        }

        private void TestImage(StreamDeckFaceTextBox textBox)
        {
            var bitmap = BitMapCreator.CreateStreamDeckBitmap(textBox.Text, textBox.Bill.TextFont, textBox.Bill.FontColor, textBox.Bill.BackgroundColor, textBox.Bill.OffsetX, textBox.Bill.OffsetY);
            SDUIParent.TestImage(bitmap);
        }

        public IStreamDeckUIParent SDUIParent { get; set; }

        private void TextBoxButtonTextFace_OnKeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                var textBox = (StreamDeckFaceTextBox)sender;
                if (!string.IsNullOrEmpty(textBox.Text))
                {
                    TestImage(textBox);
                    SetIsDirty();
                    SDUIParent.ChildChangesMade();
                }
                SetFormState();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private const int OFFSET_CHANGE_VALUE = 2;

        private void RepeatButtonActionPressUp_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                TextBoxButtonOnTextFace.Bill.OffsetY -= OFFSET_CHANGE_VALUE;
                TestImage(TextBoxButtonOnTextFace);
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
                TextBoxButtonOnTextFace.Bill.OffsetY += OFFSET_CHANGE_VALUE;
                TestImage(TextBoxButtonOnTextFace);
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
                TextBoxButtonOnTextFace.Bill.OffsetX -= OFFSET_CHANGE_VALUE;
                TestImage(TextBoxButtonOnTextFace);
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
                TextBoxButtonOnTextFace.Bill.OffsetX += OFFSET_CHANGE_VALUE;
                TestImage(TextBoxButtonOnTextFace);
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

    }
}
