using ClassLibraryCommon;
using DCSFlightpanels.Bills;
using DCSFlightpanels.CustomControls;
using NonVisuals.Interfaces;
using NonVisuals.StreamDeck;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using DCSFlightpanels.Shared;
using DCSFlightpanels.Windows.StreamDeck;
using NonVisuals;
using NonVisuals.StreamDeck.Events;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using RadioButton = System.Windows.Controls.RadioButton;

namespace DCSFlightpanels.PanelUserControls.StreamDeck
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
        public string PanelHash;


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

                StackPanelButtonTextImage.Visibility = RadioButtonTextFace.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
                StackPanelButtonDCSBIOSImage.Visibility = RadioButtonDCSBIOSFace.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
                StackPanelButtonImage.Visibility = RadioButtonImageFace.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;

                //StackPanelRadioButtonsImageType.Visibility = UserControlStreamDeckButtonAction.GetSelectedActionType() != EnumStreamDeckActionType.Unknown ? Visibility.Visible : Visibility.Collapsed;

                ButtonTextFaceFont.IsEnabled = !string.IsNullOrEmpty(TextBoxButtonTextFace.Text);
                ButtonTextFaceFontColor.IsEnabled = !string.IsNullOrEmpty(TextBoxButtonTextFace.Text);
                ButtonTextFaceBackgroundColor.IsEnabled = !string.IsNullOrEmpty(TextBoxButtonTextFace.Text);
                ButtonTestTextFace.IsEnabled = !string.IsNullOrEmpty(TextBoxButtonTextFace.Text);
                
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

            SetFormState();
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
                    TextBoxFontInfo.TargetFont = TextBoxButtonTextFace.Bill.TextFont;
                    SetIsDirty();
                }
                TextBoxButtonTextFace.TestImage(StreamDeckPanel.GetInstance(PanelHash));
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
                    TextBoxFontInfo.TargetFontColor = TextBoxButtonTextFace.Bill.FontColor;
                    SetIsDirty();

                }
                TextBoxButtonTextFace.TestImage(StreamDeckPanel.GetInstance(PanelHash));
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
                    TextBoxFontInfo.TargetBackgroundColor = TextBoxButtonTextFace.Bill.BackgroundColor;
                    SetIsDirty();

                }
                TextBoxButtonTextFace.TestImage(StreamDeckPanel.GetInstance(PanelHash));
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
                TextBoxButtonTextFace.TestImage(StreamDeckPanel.GetInstance(PanelHash));
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

        private void SetInfoTextBoxes()
        {
            TextBoxFontInfo.TargetFont = TextBoxButtonTextFace.Bill.TextFont;
            TextBoxFontInfo.TargetFontColor = TextBoxButtonTextFace.Bill.FontColor;
            TextBoxFontInfo.TargetBackgroundColor = TextBoxButtonTextFace.Bill.BackgroundColor;

            TextBoxOffsetInfo.OffSetX = TextBoxOffsetInfo.OffSetX;
            TextBoxOffsetInfo.OffSetY = TextBoxOffsetInfo.OffSetY;
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
                        TextBoxButtonTextFace.Text = faceTypeText.ButtonTextTemplate;
                        TextBoxButtonTextFace.Bill.FontColor = faceTypeText.FontColor;
                        TextBoxButtonTextFace.Bill.BackgroundColor = faceTypeText.BackgroundColor;
                        TextBoxButtonTextFace.Bill.OffsetX = faceTypeText.OffsetX;
                        TextBoxButtonTextFace.Bill.OffsetY = faceTypeText.OffsetY;

                        SetInfoTextBoxes();

                        SetFormState();
                        return;
                    }
                case EnumStreamDeckFaceType.DCSBIOS:
                    {
                        TextBoxDCSBIOSDecoder.Bill.DCSBIOSDecoder = (DCSBIOSDecoder)streamDeckButtonFace;
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
                            result.PanelHash = PanelHash;
                            result.ButtonTextTemplate = TextBoxButtonTextFace.Text;
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
                        DCSBIOSDecoder result = null;
                        if (TextBoxDCSBIOSDecoder.Bill.ContainsDCSBIOS())
                        {
                            result = TextBoxDCSBIOSDecoder.Bill.DCSBIOSDecoder.DeepClone();
                            DCSBIOSDecoder.ShowOnly(result, PanelHash);
                            TextBoxDCSBIOSDecoder.Bill.DCSBIOSDecoder.IsVisible = false;
                            result.AfterClone();
                        }
                        return result;
                    }
                case EnumStreamDeckFaceType.Image:
                    {
                        if (TextBoxImageFace.Bill.ContainsImageFace())
                        {
                            var result = new FaceTypeImage();

                            result.StreamDeckButtonName = streamDeckButtonName;
                            result.PanelHash = PanelHash;
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
                textBox.TestImage(StreamDeckPanel.GetInstance(PanelHash));
                SetIsDirty();

                SetFormState();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }


        private void TextBoxButtonTextFace_OnTextChanged(object sender, TextChangedEventArgs e)
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

        private void RepeatButtonActionPressUp_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                TextBoxButtonTextFace.Bill.OffsetY -= StreamDeckConstants.ADJUST_OFFSET_CHANGE_VALUE;
                TextBoxOffsetInfo.OffSetY = TextBoxButtonTextFace.Bill.OffsetY;
                SettingsManager.OffsetY = TextBoxButtonTextFace.Bill.OffsetY;
                TextBoxButtonTextFace.TestImage(StreamDeckPanel.GetInstance(PanelHash));
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
                TextBoxOffsetInfo.OffSetY = TextBoxButtonTextFace.Bill.OffsetY;
                SettingsManager.OffsetY = TextBoxButtonTextFace.Bill.OffsetY;
                TextBoxButtonTextFace.TestImage(StreamDeckPanel.GetInstance(PanelHash));
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
                TextBoxOffsetInfo.OffSetX = TextBoxButtonTextFace.Bill.OffsetX;
                SettingsManager.OffsetX = TextBoxButtonTextFace.Bill.OffsetX;
                TextBoxButtonTextFace.TestImage(StreamDeckPanel.GetInstance(PanelHash));
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
                TextBoxOffsetInfo.OffSetX = TextBoxButtonTextFace.Bill.OffsetX;
                SettingsManager.OffsetX = TextBoxButtonTextFace.Bill.OffsetX;
                TextBoxButtonTextFace.TestImage(StreamDeckPanel.GetInstance(PanelHash));
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

        private void ButtonAddEditDCSBIOSFaceButton_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                StreamDeckDCSBIOSDecoderWindow streamDeckDCSBIOSDecoderWindow = null;

                if (TextBoxDCSBIOSDecoder.Bill.ContainsDCSBIOS())
                {
                    TextBoxDCSBIOSDecoder.Bill.DCSBIOSDecoder.IsVisible = false;
                    streamDeckDCSBIOSDecoderWindow = new StreamDeckDCSBIOSDecoderWindow(TextBoxDCSBIOSDecoder.Bill.DCSBIOSDecoder, PanelHash);
                }
                else
                {
                    streamDeckDCSBIOSDecoderWindow = new StreamDeckDCSBIOSDecoderWindow(PanelHash);
                }

                streamDeckDCSBIOSDecoderWindow.ShowDialog();

                if (streamDeckDCSBIOSDecoderWindow.DialogResult == true)
                {
                    TextBoxDCSBIOSDecoder.Bill.DCSBIOSDecoder?.Dispose();
                    TextBoxDCSBIOSDecoder.Bill.DCSBIOSDecoder = streamDeckDCSBIOSDecoderWindow.DCSBIOSDecoder.DeepClone();
                    TextBoxDCSBIOSDecoder.Bill.DCSBIOSDecoder.AfterClone();
                    streamDeckDCSBIOSDecoderWindow.Dispose();
                    SetIsDirty();

                }
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
                    StreamDeckPanel.GetInstance(PanelHash).SetImage(_streamDeckButton, bitmap);
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
                StreamDeckPanel.GetInstance(PanelHash).SetImage(_streamDeckButton, bitmap);
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
                Dispatcher?.BeginInvoke((Action)(() =>
                {
                    Clear();
                    SetFormState();
                }));
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
                ShowStreamDeckButton(StreamDeckPanel.GetInstance(PanelHash).SelectedButton);
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
                    if (TextBoxDCSBIOSDecoder.Bill.ContainsDCSBIOS())
                    {
                        TextBoxDCSBIOSDecoder.Bill.DCSBIOSDecoder.Dispose();
                        TextBoxDCSBIOSDecoder.Bill.DCSBIOSDecoder = null;
                    }
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
