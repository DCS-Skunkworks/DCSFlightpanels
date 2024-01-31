namespace DCSFlightpanels.PanelUserControls.StreamDeck
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Forms;
    using System.Windows.Input;
    using ClassLibraryCommon;
    using CustomControls;
    using Shared;
    using DCSFlightpanels.Windows.StreamDeck;

    using MEF;
    using NLog;
    using NonVisuals;
    using NonVisuals.Interfaces;

    using KeyEventArgs = System.Windows.Input.KeyEventArgs;
    using RadioButton = System.Windows.Controls.RadioButton;
    using NonVisuals.Panels.StreamDeck.Events;
    using NonVisuals.Panels.StreamDeck.Panels;
    using NonVisuals.Panels.StreamDeck;
    using System.IO;

    /// <summary>
    /// Interaction logic for UserControlStreamDeckButtonFace.xaml
    /// </summary>
    public partial class UserControlStreamDeckButtonFace : IStreamDeckButtonActionListener, IIsDirty, INvStreamDeckListener
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly List<StreamDeckFaceTextBox> _textBoxList = new();
        private readonly List<RadioButton> _radioButtonList = new();
        private EnumStreamDeckButtonNames _streamDeckButton;
        private StreamDeckPanel _streamDeckPanel;
        public bool IsDirty { get; set; }

        public UserControlStreamDeckButtonFace()
        {
            InitializeComponent();
        }

        private bool _disposed;
        // Protected implementation of Dispose pattern.
        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                }

                _disposed = true;
            }

            // Call base class implementation.
            base.Dispose(disposing);
        }

        public override void Init()
        {
            try
            {
                if (Common.KeyEmulationOnly())
                {
                    RadioButtonDCSBIOSFace.Visibility = Visibility.Collapsed;
                }
                FillControlLists();
                LoadFontSettings();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        internal void SetStreamDeckPanel(StreamDeckPanel streamDeckPanel)
        {
            _streamDeckPanel = streamDeckPanel;
        }

        private void UserControlStreamDeckButtonFace_OnLoaded(object sender, RoutedEventArgs e)
        {
            DarkMode.SetFrameworkElementDarkMode(this);
            SetFormState();
            UserControlLoaded = true;
        }

        private void SetFormState()
        {
            try
            {
                if (!UserControlLoaded)
                {
                    return;
                }

                StackPanelButtonTextImage.Visibility = RadioButtonTextFace.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
                StackPanelButtonDCSBIOSImage.Visibility = RadioButtonDCSBIOSFace.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
                StackPanelButtonImage.Visibility = RadioButtonImageFace.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
                ButtonTextFaceFont.IsEnabled = !string.IsNullOrEmpty(TextBoxButtonTextFace.Text);
                ButtonTextFaceFontColor.IsEnabled = !string.IsNullOrEmpty(TextBoxButtonTextFace.Text);
                ButtonTextFaceBackgroundColor.IsEnabled = !string.IsNullOrEmpty(TextBoxButtonTextFace.Text);

                DisplayImagePreview();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void DisplayImagePreview()
        {
            if (TextBoxImageFace.ContainsImagePath())
            {
                var bitmap = BitMapCreator.BitmapOrFileNotFound(TextBoxImageFace.ImageFileRelativePath);
                ButtonImagePreview.Source = BitMapCreator.Bitmap2BitmapImage(bitmap);
            }
            if (TextBoxButtonTextFace.ContainsTextFace())
            {
                var bitmap = BitMapCreator.CreateStreamDeckBitmap(TextBoxButtonTextFace.Text, TextBoxButtonTextFace.TextFont, TextBoxButtonTextFace.FontColor, TextBoxButtonTextFace.OffsetX, TextBoxButtonTextFace.OffsetY, TextBoxButtonTextFace.BackgroundColor);
                TextBoxImagePreview.Source = BitMapCreator.Bitmap2BitmapImage(bitmap);
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
                textBox.Clear();
            }

            foreach (var radioButton in _radioButtonList)
            {
                radioButton.IsChecked = false;
            }
            
            ButtonImagePreview.Source = null;

            SetFormState();
            IsDirty = false;
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

        private EnumStreamDeckFaceType GetSelectedFaceType()
        {
            return true switch
            {
                _ when RadioButtonTextFace.IsChecked == true => EnumStreamDeckFaceType.Text,
                _ when RadioButtonDCSBIOSFace.IsChecked == true => EnumStreamDeckFaceType.DCSBIOS,
                _ when RadioButtonImageFace.IsChecked == true => EnumStreamDeckFaceType.Image,
                _ => EnumStreamDeckFaceType.Unknown
            };
        }

        private void RadioButtonFaceType_OnClick(object sender, RoutedEventArgs e)
        {
            SetFormState();
        }

        public void StateSaved()
        {
            IsDirty = false;
        }

        public void SetIsDirty()
        {
            IsDirty = true;
            SDEventHandler.SenderNotifiesIsDirty(this, _streamDeckPanel.BindingHash);
        }

        private void FillControlLists()
        {
            _textBoxList.Add(TextBoxButtonTextFace);
            _textBoxList.Add(TextBoxDCSBIOSDecoder);
            _textBoxList.Add(TextBoxImageFace);

            _radioButtonList.Add(RadioButtonTextFace);
            _radioButtonList.Add(RadioButtonDCSBIOSFace);
        }

        private void ButtonTextFaceFont_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (StreamDeckUICommon.SetFontStyle(TextBoxButtonTextFace) == DialogResult.OK)
                {
                    TextBoxFontInfo.TargetFont = TextBoxButtonTextFace.TextFont;
                    SetIsDirty();
                }
                TextBoxButtonTextFace.TestImage(_streamDeckPanel);
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
                    TextBoxFontInfo.TargetFontColor = TextBoxButtonTextFace.FontColor;
                    SetIsDirty();

                }
                TextBoxButtonTextFace.TestImage(_streamDeckPanel);
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
                    TextBoxFontInfo.TargetBackgroundColor = TextBoxButtonTextFace.BackgroundColor;
                    SetIsDirty();

                }
                TextBoxButtonTextFace.TestImage(_streamDeckPanel);
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
                TextBoxButtonTextFace.TextFont = SettingsManager.DefaultFont;
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
                            return TextBoxButtonTextFace.ContainsTextFace();
                        }
                    case EnumStreamDeckFaceType.DCSBIOS:
                        {
                            return TextBoxDCSBIOSDecoder.ContainsDCSBIOS();
                        }
                    case EnumStreamDeckFaceType.Image:
                        {
                            return TextBoxImageFace.ContainsImagePath();
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

            RadioButtonTextFace.IsChecked = streamDeckButton.FaceType == EnumStreamDeckFaceType.Text;
            RadioButtonDCSBIOSFace.IsChecked = streamDeckButton.FaceType == EnumStreamDeckFaceType.DCSBIOS;
            RadioButtonImageFace.IsChecked = streamDeckButton.FaceType == EnumStreamDeckFaceType.Image;

            ShowFaceConfiguration(streamDeckButton.Face);
        }

        private void SetInfoTextBoxes()
        {
            TextBoxFontInfo.TargetFont = TextBoxButtonTextFace.TextFont;
            TextBoxFontInfo.TargetFontColor = TextBoxButtonTextFace.FontColor;
            TextBoxFontInfo.TargetBackgroundColor = TextBoxButtonTextFace.BackgroundColor;

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
                        TextBoxButtonTextFace.TextFont = faceTypeText.TextFont;
                        TextBoxButtonTextFace.Text = faceTypeText.ButtonTextTemplate;
                        TextBoxButtonTextFace.FontColor = faceTypeText.FontColor;
                        TextBoxButtonTextFace.BackgroundColor = faceTypeText.BackgroundColor;
                        TextBoxButtonTextFace.OffsetX = faceTypeText.OffsetX;
                        TextBoxButtonTextFace.OffsetY = faceTypeText.OffsetY;

                        SetInfoTextBoxes();

                        SetFormState();
                        return;
                    }
                case EnumStreamDeckFaceType.DCSBIOS:
                    {
                        TextBoxDCSBIOSDecoder.DCSBIOSDecoder = (DCSBIOSDecoder)streamDeckButtonFace;
                        SetFormState();
                        return;
                    }
                case EnumStreamDeckFaceType.Image:
                    {
                        var faceTypeImage = (FaceTypeImage)streamDeckButtonFace;
                        TextBoxImageFace.ImageFileRelativePath = faceTypeImage.ImageFile;
                        SetFormState();
                        return;
                    }
                case EnumStreamDeckFaceType.Unknown:
                    {
                        TextBoxButtonTextFace.Clear();
                        TextBoxDCSBIOSDecoder.Clear();
                        TextBoxImageFace.Clear();
                        return;
                    }
            }

            throw new ArgumentException("ShowFaceConfiguration, failed to determine Face Type");
        }

        public IStreamDeckButtonFace GetStreamDeckButtonFace(StreamDeckButton streamDeckButton)
        {
            switch (GetSelectedFaceType())
            {
                case EnumStreamDeckFaceType.Text:
                    {
                        if (TextBoxButtonTextFace.ContainsTextFace())
                        {
                            return new FaceTypeText()
                            {
                                StreamDeckButtonName = streamDeckButton.StreamDeckButtonName,
                                StreamDeckPanelInstance = _streamDeckPanel,
                                ButtonTextTemplate = TextBoxButtonTextFace.Text,
                                TextFont = TextBoxButtonTextFace.TextFont,
                                FontColor = TextBoxButtonTextFace.FontColor,
                                BackgroundColor = TextBoxButtonTextFace.BackgroundColor,
                                OffsetX = TextBoxButtonTextFace.OffsetX,
                                OffsetY = TextBoxButtonTextFace.OffsetY
                            };
                        }
                        return null;
                    }
                case EnumStreamDeckFaceType.DCSBIOS:
                    {
                        DCSBIOSDecoder result = null;
                        if (TextBoxDCSBIOSDecoder.ContainsDCSBIOS())
                        {
                            result = TextBoxDCSBIOSDecoder.DCSBIOSDecoder.CloneJson();
                            result.StreamDeckPanelInstance = _streamDeckPanel;
                            DCSBIOSDecoder.ShowOnly(result, _streamDeckPanel);
                            TextBoxDCSBIOSDecoder.DCSBIOSDecoder.IsVisible = false;
                            result.AfterClone();
                        }
                        return result;
                    }
                case EnumStreamDeckFaceType.Image:
                    {
                        if (TextBoxImageFace.ContainsImagePath())
                        {
                            FaceTypeImage faceTypeImage = new()
                            {
                                StreamDeckButtonName = streamDeckButton.StreamDeckButtonName,
                                StreamDeckPanelInstance = _streamDeckPanel,
                                ImageFile = TextBoxImageFace.ImageFileRelativePath
                            };
                            
                            //Fixes issue https://github.com/DCS-Skunkworks/DCSFlightpanels/issues/394
                            //Since the path is busted but we have a serialized bitmap, copy the old bitmap on the new face
                            if (!File.Exists(faceTypeImage.ImageFile) && ((FaceTypeImage)streamDeckButton.Face).Bitmap != null)
                            {
                                faceTypeImage.Bitmap = ((FaceTypeImage)streamDeckButton.Face).Bitmap;
                                faceTypeImage.RawBitmap = ((FaceTypeImage)streamDeckButton.Face).RawBitmap;
                            }
                            return faceTypeImage;
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
                textBox.TestImage(_streamDeckPanel);
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
                TextBoxButtonTextFace.OffsetY -= StreamDeckConstants.ADJUST_OFFSET_CHANGE_VALUE;
                TextBoxOffsetInfo.OffSetY = TextBoxButtonTextFace.OffsetY;
                SettingsManager.OffsetY = TextBoxButtonTextFace.OffsetY;
                TextBoxButtonTextFace.TestImage(_streamDeckPanel);                
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
                TextBoxButtonTextFace.OffsetY += StreamDeckConstants.ADJUST_OFFSET_CHANGE_VALUE;
                TextBoxOffsetInfo.OffSetY = TextBoxButtonTextFace.OffsetY;
                SettingsManager.OffsetY = TextBoxButtonTextFace.OffsetY;
                TextBoxButtonTextFace.TestImage(_streamDeckPanel);
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
                TextBoxButtonTextFace.OffsetX -= StreamDeckConstants.ADJUST_OFFSET_CHANGE_VALUE;
                TextBoxOffsetInfo.OffSetX = TextBoxButtonTextFace.OffsetX;
                SettingsManager.OffsetX = TextBoxButtonTextFace.OffsetX;
                TextBoxButtonTextFace.TestImage(_streamDeckPanel);
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
                TextBoxButtonTextFace.OffsetX += StreamDeckConstants.ADJUST_OFFSET_CHANGE_VALUE;
                TextBoxOffsetInfo.OffSetX = TextBoxButtonTextFace.OffsetX;
                SettingsManager.OffsetX = TextBoxButtonTextFace.OffsetX;
                TextBoxButtonTextFace.TestImage(_streamDeckPanel);
                SetIsDirty();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        public void ActionTypeChangedEvent(object sender, ActionTypeChangedEventArgs e)
        {
            /*
             * Add the incoming button face if there isn't any already specified.
             * Layer navigation always updates.
             */
            if (e.BindingHash == _streamDeckPanel.BindingHash && e.ActionType == EnumStreamDeckActionType.LayerNavigation && !string.IsNullOrEmpty(e.TargetLayerName))
            {
                if (e.TargetLayerName == StreamDeckConstants.NO_ACTION)
                {
                    Clear();
                }
                else if (e.TargetLayerName == StreamDeckConstants.HOME)
                {
                    Clear();
                    RadioButtonImageFace.IsChecked = true;
                    TextBoxImageFace.ImageFileRelativePath = @"StreamDeckGallery\\Symbols\\home_white.png";
                }
                else if (e.TargetLayerName == StreamDeckConstants.BACK)
                {
                    Clear();
                    RadioButtonImageFace.IsChecked = true;
                    TextBoxImageFace.ImageFileRelativePath = @"StreamDeckGallery\\Symbols\\back_white.png";
                }
                else
                {
                    /*
                     * Create a basic face containing the name
                     */
                    Clear();
                    RadioButtonTextFace.IsChecked = true;
                    TextBoxButtonTextFace.Text = e.TargetLayerName;
                    TextBoxButtonTextFace.FontColor = ColorTranslator.FromHtml(StreamDeckConstants.COLOR_DEFAULT_WHITE);
                    TextBoxButtonTextFace.BackgroundColor = ColorTranslator.FromHtml(StreamDeckConstants.COLOR_GUNSHIP_GREEN);
                }
                SetIsDirty();
            }
        }

        private void ButtonAddEditDCSBIOSFaceButton_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                StreamDeckDCSBIOSDecoderWindow streamDeckDCSBIOSDecoderWindow = null;

                if (TextBoxDCSBIOSDecoder.ContainsDCSBIOS())
                {
                    TextBoxDCSBIOSDecoder.DCSBIOSDecoder.IsVisible = false;
                    streamDeckDCSBIOSDecoderWindow = new StreamDeckDCSBIOSDecoderWindow(TextBoxDCSBIOSDecoder.DCSBIOSDecoder, _streamDeckPanel);
                }
                else
                {
                    streamDeckDCSBIOSDecoderWindow = new StreamDeckDCSBIOSDecoderWindow(_streamDeckPanel);
                }

                streamDeckDCSBIOSDecoderWindow.ShowDialog();

                if (streamDeckDCSBIOSDecoderWindow.DialogResult == true)
                {
                    TextBoxDCSBIOSDecoder.DCSBIOSDecoder?.Dispose();
                    TextBoxDCSBIOSDecoder.DCSBIOSDecoder = streamDeckDCSBIOSDecoderWindow.DCSBIOSDecoder.CloneJson();
                    TextBoxDCSBIOSDecoder.DCSBIOSDecoder.StreamDeckPanelInstance = _streamDeckPanel;
                    TextBoxDCSBIOSDecoder.DCSBIOSDecoder.AfterClone();
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

        private void ButtonBrowseForImage_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var imageRelativePath = string.Empty;
                var directory = SettingsManager.LastImageFileDirectory;

                var dialogResult = StreamDeckUICommon.BrowseForImage(ref directory, ref imageRelativePath);

                if (dialogResult == DialogResult.OK)
                {
                    TextBoxImageFace.ImageFileRelativePath = imageRelativePath;
                    SettingsManager.LastImageFileDirectory = directory;
                    _streamDeckPanel.SetImage(_streamDeckButton, new Bitmap(TextBoxImageFace.ImageFileRelativePath));
                    SetIsDirty();

                    SetFormState();
                }
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
                if (_streamDeckPanel.BindingHash == e.BindingHash)
                {
                    Dispatcher?.BeginInvoke((Action) (() =>
                    {
                        Clear();
                        SetFormState();
                    }));
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }

        public void RemoteLayerSwitch(object sender, RemoteStreamDeckShowNewLayerArgs e)
        {
            try
            {
                if (_streamDeckPanel.BindingHash == e.RemoteBindingHash)
                {
                    Dispatcher?.BeginInvoke(SetFormState);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }

        public void SelectedButtonChanged(object sender, StreamDeckSelectedButtonChangedArgs e)
        {
            try
            {
                if (_streamDeckPanel.BindingHash == e.BindingHash)
                {
                    ShowStreamDeckButton(_streamDeckPanel.SelectedButton);
                    SetFormState();
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
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
                Logger.Error(ex);
            }
        }

        public void SenderIsDirtyNotification(object sender, StreamDeckDirtyNotificationArgs e)
        {
            try
            {
                if (_streamDeckPanel.BindingHash == e.BindingHash)
                {
                    SetFormState();
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }

        public void ClearSettings(object sender, StreamDeckClearSettingsArgs e)
        {
            try
            {
                if (_streamDeckPanel.BindingHash == e.BindingHash && e.ClearFaceConfiguration)
                {
                    if (TextBoxDCSBIOSDecoder.ContainsDCSBIOS())
                    {
                        TextBoxDCSBIOSDecoder.DCSBIOSDecoder.Dispose();
                        TextBoxDCSBIOSDecoder.DCSBIOSDecoder = null;
                    }
                    Clear();
                }
                SetFormState();
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }
    }
}
