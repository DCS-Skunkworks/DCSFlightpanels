using ClassLibraryCommon;
using DCSFlightpanels.Bills;
using DCSFlightpanels.CustomControls;
using DCSFlightpanels.Properties;
using DCSFlightpanels.Windows;
using NonVisuals;
using NonVisuals.Interfaces;
using NonVisuals.StreamDeck;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using RadioButton = System.Windows.Controls.RadioButton;

namespace DCSFlightpanels.PanelUserControls
{
    /// <summary>
    /// Interaction logic for UserControlStreamDeckButtonFace.xaml
    /// </summary>
    public partial class UserControlStreamDeckButtonFace : UserControlBase, UserControlStreamDeckButtonAction.IStreamDeckButtonActionListener, IIsDirty
    {
        private IGlobalHandler _globalHandler;
        private bool _isDirty = false;
        private bool _isLoaded = false;
        private List<StreamDeckFaceTextBox> _textBoxList = new List<StreamDeckFaceTextBox>();
        private List<RadioButton> _radioButtonList = new List<RadioButton>();
        private EnumStreamDeckButtonNames _streamDeckButton;
        private UserControlStreamDeckButtonAction _userControlStreamDeckButtonAction;



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
        
        public void SetDecoder(DCSBIOSDecoder dcsbiosDecoder)
        {
            TextBoxDCSBIOSDecoder.Bill.DCSBIOSDecoder = dcsbiosDecoder;
        }

        public void SetButton(EnumStreamDeckButtonNames streamDeckButton)
        {
            _streamDeckButton = streamDeckButton;
        }

        public void SetFormState()
        {
            try
            {
                if (!_isLoaded)
                {
                    return;
                }
                RadioButtonDCSBIOSFace.Visibility = UserControlStreamDeckButtonAction.GetSelectedActionType() != EnumStreamDeckActionType.LayerNavigation ? Visibility.Visible : Visibility.Collapsed;

                StackPanelButtonTextAndStyle.Visibility = RadioButtonTextFace.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
                StackPanelButtonDCSBIOSImage.Visibility = RadioButtonDCSBIOSFace.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
                StackPanelButtonImage.Visibility = RadioButtonImageFace.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;

                //StackPanelRadioButtonsImageType.Visibility = UserControlStreamDeckButtonAction.GetSelectedActionType() != EnumStreamDeckActionType.Unknown ? Visibility.Visible : Visibility.Collapsed;

                ButtonTextFaceFont.IsEnabled = !string.IsNullOrEmpty(TextBoxButtonTextFace.Text);
                ButtonTextFaceFontColor.IsEnabled = !string.IsNullOrEmpty(TextBoxButtonTextFace.Text);
                ButtonTextFaceBackgroundColor.IsEnabled = !string.IsNullOrEmpty(TextBoxButtonTextFace.Text);
                ButtonTestTextFace.IsEnabled = !string.IsNullOrEmpty(TextBoxButtonTextFace.Text);


                TextBoxFontInfo.Text = "Font : " + TextBoxButtonTextFace.Bill.TextFont.Name + " " +
                                                    TextBoxButtonTextFace.Bill.TextFont.Size + " " +
                                                    (TextBoxButtonTextFace.Bill.TextFont.Bold ? "Bold" : "Regular");
                TextBoxFontInfo.Text = TextBoxFontInfo.Text + "\n" + "Color : " + TextBoxButtonTextFace.Bill.BackgroundHex;

                ButtonDeleteDCSBIOSFaceButton.IsEnabled = TextBoxDCSBIOSDecoder.Bill.ContainsDCSBIOS();

                ButtonTestSelectImageGalleryButton.IsEnabled = TextBoxImageFace.Bill.ContainsImageFace();
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

        private void ClearRadioButtons()
        {
            RadioButtonTextFace.IsChecked = false;
            RadioButtonDCSBIOSFace.IsChecked = false;
            RadioButtonImageFace.IsChecked = false;
        }

        public EnumStreamDeckFaceType GetSelectedFaceType()
        {
            if (RadioButtonTextFace.IsChecked == true)
            {
                return EnumStreamDeckFaceType.Text;
            }
            if (RadioButtonDCSBIOSFace.IsChecked == true)
            {
                return EnumStreamDeckFaceType.DCSBIOS;
            }
            if (RadioButtonImageFace.IsChecked == true)
            {
                return EnumStreamDeckFaceType.Image;
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

        public void SetIsDirty()
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
            _textBoxList.Add(TextBoxButtonTextFace);
            _textBoxList.Add(TextBoxDCSBIOSDecoder);
            _textBoxList.Add(TextBoxImageFace);

            _radioButtonList.Add(RadioButtonTextFace);
            _radioButtonList.Add(RadioButtonDCSBIOSFace);
        }

        private void SetBills()
        {
            foreach (var textBox in _textBoxList)
            {
                textBox.Bill = new BillStreamDeckFace();
                textBox.Bill.TextBox = textBox;
            }
        }

        private void ButtonTextFaceFont_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                SetFontStyle(TextBoxButtonTextFace);
                TestImage(TextBoxButtonTextFace);
                SetFormState();
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
                SetFontColor(TextBoxButtonTextFace);
                TestImage(TextBoxButtonTextFace);
                SetFormState();
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
                SetBackgroundColor(TextBoxButtonTextFace);
                TestImage(TextBoxButtonTextFace);
                SetFormState();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void ButtonTestTextFace_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                TestImage(TextBoxButtonTextFace);
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
                TextBoxButtonTextFace.Bill.TextFont = Settings.Default.ButtonTextFaceFont;
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
                            return TextBoxButtonTextFace.Bill.ContainsTextFace();
                        }
                    case EnumStreamDeckFaceType.DCSBIOS:
                        {
                            return TextBoxDCSBIOSDecoder.Bill.ContainsDCSBIOS();
                        }
                    case EnumStreamDeckFaceType.Image:
                    {
                        return TextBoxImageFace.Bill.ContainsImageFace();
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

            _streamDeckButton = streamDeckButton.StreamDeckButtonName;

            ClearRadioButtons();

            switch (streamDeckButton.FaceType)
            {
                case EnumStreamDeckFaceType.Text:
                    {
                        RadioButtonTextFace.IsChecked = true;
                        break;
                    }
                case EnumStreamDeckFaceType.DCSBIOS:
                    {
                        RadioButtonDCSBIOSFace.IsChecked = true;
                        break;
                    }
                case EnumStreamDeckFaceType.Image:
                    {
                        RadioButtonImageFace.IsChecked = true;
                        break;
                    }
                default:
                    {
                        return;
                    }
            }
            ShowFaceConfiguration(streamDeckButton.Face);

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
                        TextBoxButtonTextFace.Bill.TextFont = faceTypeText.TextFont;
                        TextBoxButtonTextFace.Text = faceTypeText.Text;
                        TextBoxButtonTextFace.Bill.FontColor = faceTypeText.FontColor;
                        TextBoxButtonTextFace.Bill.BackgroundColor = faceTypeText.BackgroundColor;
                        SetFormState();
                        return;
                    }
                case EnumStreamDeckFaceType.DCSBIOS:
                    {
                        var dcsbiosDecoder = (DCSBIOSDecoder)streamDeckButtonFace;
                        TextBoxDCSBIOSDecoder.Bill.DCSBIOSDecoder = dcsbiosDecoder;
                        SetFormState();
                        return;
                    }
                case EnumStreamDeckFaceType.Image:
                    {
                        var faceTypeImage = (FaceTypeImage)streamDeckButtonFace;
                        TextBoxImageFace.Bill.ImageFilePath = faceTypeImage.ImageFile;
                        SetFormState();
                        return;
                    }
                case EnumStreamDeckFaceType.Unknown:
                    {
                        TextBoxButtonTextFace.Bill.Clear();
                        TextBoxDCSBIOSDecoder.Bill.Clear();
                        TextBoxImageFace.Bill.Clear();
                        return;
                    }
            }

            throw new ArgumentException("ShowFaceConfiguration, failed to determine Face Type");
        }


        public IStreamDeckButtonFace GetStreamDeckButtonFace(EnumStreamDeckButtonNames streamDeckButtonName)
        {
            switch (GetSelectedFaceType())
            {
                case EnumStreamDeckFaceType.Text:
                    {
                        if (TextBoxButtonTextFace.Bill.ContainsTextFace())
                        {
                            var result = new FaceTypeText();

                            result.StreamDeckButtonName = streamDeckButtonName;
                            result.StreamDeckInstanceId = SDUIParent.GetStreamDeckInstanceId();
                            result.Text = TextBoxButtonTextFace.Text;
                            result.TextFont = TextBoxButtonTextFace.Bill.TextFont;
                            result.FontColor = TextBoxButtonTextFace.Bill.FontColor;
                            result.BackgroundColor = TextBoxButtonTextFace.Bill.BackgroundColor;
                            result.OffsetX = TextBoxButtonTextFace.Bill.OffsetX;
                            result.OffsetY = TextBoxButtonTextFace.Bill.OffsetY;

                            return result;
                        }

                        return null;
                    }
                case EnumStreamDeckFaceType.DCSBIOS:
                    {
                        return TextBoxDCSBIOSDecoder.Bill.DCSBIOSDecoder;
                    }
                case EnumStreamDeckFaceType.Image:
                    {
                        if (TextBoxImageFace.Bill.ContainsImageFace())
                        {
                            var result = new FaceTypeImage();

                            result.StreamDeckButtonName = streamDeckButtonName;
                            result.StreamDeckInstanceId = SDUIParent.GetStreamDeckInstanceId();
                            result.ImageFile = TextBoxImageFace.Bill.ImageFilePath;

                            return result;
                        }

                        return null;
                    }
                case EnumStreamDeckFaceType.Unknown:
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
            colorDialog.CustomColors = Constants.GetOLEColors();

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
            colorDialog.CustomColors = Constants.GetOLEColors();
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
                TestImage(textBox);
                SetIsDirty();
                SDUIParent.ChildChangesMade();
                SetFormState();
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
                TextBoxButtonTextFace.Bill.OffsetY -= Constants.ADJUST_OFFSET_CHANGE_VALUE;
                TestImage(TextBoxButtonTextFace);
                SetIsDirty();
                SDUIParent.ChildChangesMade();
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
                TextBoxButtonTextFace.Bill.OffsetY += Constants.ADJUST_OFFSET_CHANGE_VALUE;
                TestImage(TextBoxButtonTextFace);
                SetIsDirty();
                SDUIParent.ChildChangesMade();
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
                TextBoxButtonTextFace.Bill.OffsetX -= Constants.ADJUST_OFFSET_CHANGE_VALUE;
                TestImage(TextBoxButtonTextFace);
                SetIsDirty();
                SDUIParent.ChildChangesMade();
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
                TextBoxButtonTextFace.Bill.OffsetX += Constants.ADJUST_OFFSET_CHANGE_VALUE;
                TestImage(TextBoxButtonTextFace);
                SetIsDirty();
                SDUIParent.ChildChangesMade();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        public void ActionTypeChangedEvent(object sender, UserControlStreamDeckButtonAction.ActionTypeChangedEventArgs e)
        {
            /*
             * Add the incoming button face if there isn't any already specified.
             * Layer navigation always updates.
             */
            if (e.ActionType == EnumStreamDeckActionType.LayerNavigation && !string.IsNullOrEmpty(e.TargetLayerName))
            {
                if (e.TargetLayerName == Constants.HOME)
                {
                    Clear();
                    RadioButtonImageFace.IsChecked = true;
                    TextBoxImageFace.Bill.ImageFilePath = StreamDeckConstants.StreamDeckGalleryPathSymbols + StreamDeckConstants.StreamDeckGalleryHomeWhite;
                    SetIsDirty();
                }
                else if (e.TargetLayerName == Constants.BACK)
                {
                    Clear();
                    RadioButtonImageFace.IsChecked = true;
                    TextBoxImageFace.Bill.ImageFilePath = StreamDeckConstants.StreamDeckGalleryPathSymbols + StreamDeckConstants.StreamDeckGalleryBackWhite;
                    SetIsDirty();
                }
                else
                {
                    /*
                     * Create a basic face containing the name
                     */
                    Clear();
                    RadioButtonTextFace.IsChecked = true;
                    TextBoxButtonTextFace.Text = e.TargetLayerName;
                    TextBoxButtonTextFace.Bill.FontColor = ColorTranslator.FromHtml(Constants.COLOR_DEFAULT_WHITE);
                    TextBoxButtonTextFace.Bill.BackgroundColor = ColorTranslator.FromHtml(Constants.COLOR_GUNSHIP_GREEN);
                }
                SetIsDirty();
            }
        }

        private void ButtonSelectDCSBIOSFaceButton_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                StreamDeckDCSBIOSDecoderWindow streamDeckDCSBIOSDecoderWindow = null;

                if (TextBoxDCSBIOSDecoder.Bill.ContainsDCSBIOS())
                {
                    streamDeckDCSBIOSDecoderWindow = new StreamDeckDCSBIOSDecoderWindow(TextBoxDCSBIOSDecoder.Bill.DCSBIOSDecoder, SDUIParent.GetStreamDeckInstanceId());
                }
                else
                {
                    streamDeckDCSBIOSDecoderWindow = new StreamDeckDCSBIOSDecoderWindow(SDUIParent.GetStreamDeckInstanceId(), _streamDeckButton);
                }

                streamDeckDCSBIOSDecoderWindow.ShowDialog();

                if (streamDeckDCSBIOSDecoderWindow.DialogResult == true)
                {
                    TextBoxDCSBIOSDecoder.Bill.DCSBIOSDecoder = streamDeckDCSBIOSDecoderWindow.DCSBIOSDecoder;
                    SetIsDirty();
                    SDUIParent.ChildChangesMade();
                }
                ButtonFocus.Focus();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void ButtonDeleteDCSBIOSFaceButton_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                TextBoxDCSBIOSDecoder.Bill.DCSBIOSDecoder = null;
                SetIsDirty();
                SDUIParent.ChildChangesMade();
                ButtonFocus.Focus();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(442044, ex);
            }
        }

        private void ButtonBrowse_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                FileDialog fileDialog = new OpenFileDialog();
                fileDialog.CheckPathExists = true;
                fileDialog.CheckFileExists = true;
                fileDialog.InitialDirectory = string.IsNullOrEmpty(Settings.Default.LastFileDialogLocation) ? Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) : Settings.Default.LastFileDialogLocation;
                fileDialog.Filter = @"Image files|*.jpg;*.jpeg;*.png";

                if (fileDialog.ShowDialog() == DialogResult.OK)
                {
                    TextBoxImageFace.Bill.ImageFilePath = fileDialog.FileName;
                    Settings.Default.LastFileDialogLocation = Path.GetDirectoryName(fileDialog.FileName);
                }
                SetIsDirty();
                SDUIParent.ChildChangesMade();
                SetFormState();
                ButtonFocus.Focus();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(442044, ex);
            }
        }

        private void ButtonTestSelectImageGalleryButton_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!TextBoxImageFace.Bill.ContainsImageFace())
                {
                    return;
                }
                var bitmap = new Bitmap(TextBoxImageFace.Bill.ImageFilePath);
                SDUIParent.TestImage(bitmap);
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(442044, ex);
            }
        }

        public UserControlStreamDeckButtonAction UserControlStreamDeckButtonAction
        {
            get => _userControlStreamDeckButtonAction;
            set => _userControlStreamDeckButtonAction = value;
        }
    }
}
