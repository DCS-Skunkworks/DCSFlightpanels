using ClassLibraryCommon;
using DCSFlightpanels.Bills;
using DCSFlightpanels.CustomControls;
using DCSFlightpanels.Windows;
using NonVisuals.Interfaces;
using NonVisuals.StreamDeck;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using DCSFlightpanels.Shared;
using NonVisuals;
using NonVisuals.StreamDeck.Events;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using RadioButton = System.Windows.Controls.RadioButton;

namespace DCSFlightpanels.PanelUserControls
{
    /// <summary>
    /// Interaction logic for UserControlStreamDeckButtonFace.xaml
    /// </summary>
    public partial class UserControlStreamDeckButtonFace : UserControlBase, UserControlStreamDeckButtonAction.IStreamDeckButtonActionListener, IIsDirty, IStreamDeckListener
    {
        private IGlobalHandler _globalHandler;
        private bool _isDirty = false;
        private bool _isLoaded = false;
        private List<StreamDeckFaceTextBox> _textBoxList = new List<StreamDeckFaceTextBox>();
        private List<RadioButton> _radioButtonList = new List<RadioButton>();
        private EnumStreamDeckButtonNames _streamDeckButton;
        public string StreamDeckInstanceId;


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

        /*public void SetButton(EnumStreamDeckButtonNames streamDeckButton)
        {
            _streamDeckButton = streamDeckButton;a
        }*/

        public void SetFormState()
        {
            try
            {
                if (!_isLoaded)
                {
                    return;
                }

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
                Common.ShowErrorMessageBox(ex);
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
            EventHandlers.SenderNotifiesIsDirty(this, _streamDeckButton, "");
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
                if (StreamDeckUICommon.SetFontStyle(TextBoxButtonTextFace) == DialogResult.OK)
                {
                    SetIsDirty();

                }
                TextBoxButtonTextFace.TestImage(StreamDeckPanel.GetInstance(StreamDeckInstanceId));
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
                if (StreamDeckUICommon.SetFontColor(TextBoxButtonTextFace) == DialogResult.OK)
                {
                    SetIsDirty();

                }
                TextBoxButtonTextFace.TestImage(StreamDeckPanel.GetInstance(StreamDeckInstanceId));
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
                if (StreamDeckUICommon.SetBackgroundColor(TextBoxButtonTextFace) == DialogResult.OK)
                {
                    SetIsDirty();

                }
                TextBoxButtonTextFace.TestImage(StreamDeckPanel.GetInstance(StreamDeckInstanceId));
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
                TextBoxButtonTextFace.TestImage(StreamDeckPanel.GetInstance(StreamDeckInstanceId));
                SetFormState();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void LoadFontSettings()
        {
            if (SettingsManager.DefaultFont != null)
            {
                TextBoxButtonTextFace.Bill.TextFont = SettingsManager.DefaultFont;
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

        public void ShowStreamDeckButton(StreamDeckButton streamDeckButton)
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

        private void ShowFaceConfiguration(IStreamDeckButtonFace streamDeckButtonFace)
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
                        TextBoxButtonTextFace.Text = faceTypeText.ButtonText;
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
                        TextBoxImageFace.Bill.ImageFileRelativePath = faceTypeImage.ImageFile;
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
                            result.StreamDeckInstanceId = StreamDeckInstanceId;
                            result.ButtonText = TextBoxButtonTextFace.Text;
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
                            result.StreamDeckInstanceId = StreamDeckInstanceId;
                            result.ImageFile = TextBoxImageFace.Bill.ImageFileRelativePath;

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


        private void TextBoxButtonTextFace_OnKeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                var textBox = (StreamDeckFaceTextBox)sender;
                textBox.TestImage(StreamDeckPanel.GetInstance(StreamDeckInstanceId));
                SetIsDirty();

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
                TextBoxButtonTextFace.Bill.OffsetY -= StreamDeckConstants.ADJUST_OFFSET_CHANGE_VALUE;
                SettingsManager.OffsetY = TextBoxButtonTextFace.Bill.OffsetY;
                TextBoxButtonTextFace.TestImage(StreamDeckPanel.GetInstance(StreamDeckInstanceId));
                SetIsDirty();

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
                TextBoxButtonTextFace.Bill.OffsetY += StreamDeckConstants.ADJUST_OFFSET_CHANGE_VALUE;
                SettingsManager.OffsetY = TextBoxButtonTextFace.Bill.OffsetY;
                TextBoxButtonTextFace.TestImage(StreamDeckPanel.GetInstance(StreamDeckInstanceId));
                SetIsDirty();

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
                TextBoxButtonTextFace.Bill.OffsetX -= StreamDeckConstants.ADJUST_OFFSET_CHANGE_VALUE;
                SettingsManager.OffsetX = TextBoxButtonTextFace.Bill.OffsetX;
                TextBoxButtonTextFace.TestImage(StreamDeckPanel.GetInstance(StreamDeckInstanceId));
                SetIsDirty();

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
                TextBoxButtonTextFace.Bill.OffsetX += StreamDeckConstants.ADJUST_OFFSET_CHANGE_VALUE;
                SettingsManager.OffsetX = TextBoxButtonTextFace.Bill.OffsetX;
                TextBoxButtonTextFace.TestImage(StreamDeckPanel.GetInstance(StreamDeckInstanceId));
                SetIsDirty();

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
                if (e.TargetLayerName == StreamDeckConstants.HOME)
                {
                    Clear();
                    RadioButtonImageFace.IsChecked = true;
                    TextBoxImageFace.Bill.ImageFileRelativePath = StreamDeckConstants.StreamDeckGalleryPathSymbols + StreamDeckConstants.StreamDeckGalleryHomeWhite;
                }
                else if (e.TargetLayerName == StreamDeckConstants.BACK)
                {
                    Clear();
                    RadioButtonImageFace.IsChecked = true;
                    TextBoxImageFace.Bill.ImageFileRelativePath = StreamDeckConstants.StreamDeckGalleryPathSymbols + StreamDeckConstants.StreamDeckGalleryBackWhite;
                }
                else
                {
                    /*
                     * Create a basic face containing the name
                     */
                    Clear();
                    RadioButtonTextFace.IsChecked = true;
                    TextBoxButtonTextFace.Text = e.TargetLayerName;
                    TextBoxButtonTextFace.Bill.FontColor = ColorTranslator.FromHtml(StreamDeckConstants.COLOR_DEFAULT_WHITE);
                    TextBoxButtonTextFace.Bill.BackgroundColor = ColorTranslator.FromHtml(StreamDeckConstants.COLOR_GUNSHIP_GREEN);
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
                    streamDeckDCSBIOSDecoderWindow = new StreamDeckDCSBIOSDecoderWindow(TextBoxDCSBIOSDecoder.Bill.DCSBIOSDecoder, StreamDeckInstanceId);
                }
                else
                {
                    streamDeckDCSBIOSDecoderWindow = new StreamDeckDCSBIOSDecoderWindow(StreamDeckInstanceId);
                }

                streamDeckDCSBIOSDecoderWindow.ShowDialog();

                if (streamDeckDCSBIOSDecoderWindow.DialogResult == true)
                {
                    TextBoxDCSBIOSDecoder.Bill.DCSBIOSDecoder = streamDeckDCSBIOSDecoderWindow.DCSBIOSDecoder;
                    SetIsDirty();

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

                ButtonFocus.Focus();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void ButtonBrowse_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var imageRelativePath = "";
                var directory = SettingsManager.LastImageFileDirectory;

                var dialogResult = StreamDeckUICommon.BrowseForImage(ref directory, ref imageRelativePath);

                if (dialogResult == DialogResult.OK)
                {
                    TextBoxImageFace.Bill.ImageFileRelativePath = imageRelativePath;
                    SettingsManager.LastImageFileDirectory = directory;
                    var bitmap = new Bitmap(TextBoxImageFace.Bill.ImageFileRelativePath);
                    StreamDeckPanel.GetInstance(StreamDeckInstanceId).SetImage(_streamDeckButton, bitmap);
                    SetIsDirty();

                    SetFormState();
                    ButtonFocus.Focus();
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
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
                var bitmap = new Bitmap(TextBoxImageFace.Bill.ImageFileRelativePath);
                StreamDeckPanel.GetInstance(StreamDeckInstanceId).SetImage(_streamDeckButton, bitmap);
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }
        
        public void LayerSwitched(object sender, StreamDeckShowNewLayerArgs e)
        {
            try
            {
                Clear();
                SetFormState();
            }
            catch (Exception ex)
            {
                Common.LogError(ex);
            }
        }

        public void SelectedButtonChanged(object sender, StreamDeckSelectedButtonChangedArgs e)
        {
            try
            {
                ShowStreamDeckButton(StreamDeckPanel.GetInstance(StreamDeckInstanceId).SelectedButton);
                SetFormState();
            }
            catch (Exception ex)
            {
                Common.LogError(ex);
            }
        }

        public void IsDirtyQueryReport(object sender, StreamDeckDirtyReportArgs e)
        {
            try
            {
                if (sender.Equals(this))
                {
                    return;
                }
                e.Cancel = IsDirty;
                SetFormState();
            }
            catch (Exception ex)
            {
                Common.LogError(ex);
            }
        }

        public void SenderIsDirtyNotification(object sender, StreamDeckDirtyNotificationArgs e)
        {
            try
            {
                SetFormState();
            }
            catch (Exception ex)
            {
                Common.LogError(ex);
            }
        }

        public void ClearSettings(object sender, StreamDeckClearSettingsArgs e)
        {
            try
            {
                if (e.ClearFaceConfiguration)
                {
                    Clear();
                }
                SetFormState();
            }
            catch (Exception ex)
            {
                Common.LogError(ex);
            }
        }
    }
}
