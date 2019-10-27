using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Net.Mime;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using ClassLibraryCommon;
using DCSFlightpanels.Properties;
using DCSFlightpanels.TagDataClasses;
using NonVisuals.Interfaces;
using Color = System.Windows.Media.Color;
using FontFamily = System.Windows.Media.FontFamily;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using TextBox = System.Windows.Controls.TextBox;
using NonVisuals.StreamDeck;

namespace DCSFlightpanels.PanelUserControls
{
    /// <summary>
    /// Interaction logic for UserControlStreamDeckButtonImage.xaml
    /// </summary>
    public partial class UserControlStreamDeckButtonImage : UserControlBase
    {
        private IGlobalHandler _globalHandler;
        private bool _isDirty = false;
        private bool _isLoaded = false;
        private List<TextBox> _textBoxList = new List<TextBox>();








        public UserControlStreamDeckButtonImage()
        {
            InitializeComponent();
        }
        
        private void UserControlStreamDeckButtonImage_OnLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if(_isLoaded)
                {
                    return;
                }
                LoadFontSettings();
                SetFormState();
                FillControlLists();
                SetTagData();
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
                RadioButtonDCSBIOSOutput.Visibility = SDUIParent.GetSelectedActionType() != EnumStreamDeckActionType.LayerNavigation ? Visibility.Visible : Visibility.Collapsed;

                StackPanelButtonTextAndStyle.Visibility = RadioButtonText.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
                StackPanelButtonImageToShow.Visibility = RadioButtonImage.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
                StackPanelButtonDCSBIOSImage.Visibility = RadioButtonDCSBIOSOutput.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;

                StackPanelDCSBIOSBackgroundType.Visibility = RadioButtonDCSBIOSOutput?.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;

                StackPanelButtonDCSBIOSBackgroundGeneratedImage.Visibility = RadioButtonDCSBIOSBackgroundGenerated.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
                StackPanelButtonDCSBIOSBackgroundExistingImage.Visibility = RadioButtonDCSBIOSBackgroundExisting.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;

                StackPanelButtonImageType.Visibility = SDUIParent.GetSelectedActionType() != EnumStreamDeckActionType.LayerNavigation ? Visibility.Visible : Visibility.Collapsed;

                StackPanelDCSBIOSBackgroundTypeSelection.Visibility = RadioButtonDCSBIOSOutput.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
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

        private void RadioButtonImageType_OnClick(object sender, RoutedEventArgs e)
        {
            SetFormState();
        }

        public IGlobalHandler GlobalHandler
        {
            get => _globalHandler;
            set => _globalHandler = value;
        }


        public bool HasConfig
        {
            get
            {
                return false;
            }
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
            _textBoxList.Add(TextBoxDCSBIOSOutputButtonOn);
            _textBoxList.Add(TextBoxDCSBIOSOutputButtonOff);
        }

        private void SetTagData()
        {
            foreach (var textBox in _textBoxList)
            {
                textBox.Tag = new TagDataStreamDeckImage();
            }
        }

        private void ButtonOnTextFaceFont_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                SetFontStyle(TextBoxButtonOnTextFace);
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
            if (Settings.Default.ButtonTextImageFont != null)
            {
                TextBoxButtonOnTextFace.FontFamily = new FontFamily(Settings.Default.ButtonTextImageFont.Name);
                TextBoxButtonOnTextFace.FontWeight = Settings.Default.ButtonTextImageFont.Bold ? FontWeights.Bold : FontWeights.Regular;
                TextBoxButtonOnTextFace.FontSize = Settings.Default.ButtonTextImageFont.Size * 96.0 / 72.0;
                TextBoxButtonOnTextFace.FontStyle = Settings.Default.ButtonTextImageFont.Italic ? FontStyles.Italic : FontStyles.Normal;
                var textDecorationCollection = new TextDecorationCollection();
                if (Settings.Default.ButtonTextImageFont.Underline) textDecorationCollection.Add(TextDecorations.Underline);
                if (Settings.Default.ButtonTextImageFont.Strikeout) textDecorationCollection.Add(TextDecorations.Strikethrough);
                TextBoxButtonOnTextFace.TextDecorations = textDecorationCollection;
            }
        }

        private void SetFontStyle(TextBox textBox)
        {
            var fontDialog = new FontDialog();

            fontDialog.FixedPitchOnly = true;
            fontDialog.FontMustExist = true;
            fontDialog.MinSize = 6;

            if (Settings.Default.ButtonTextImageFont != null)
            {
                fontDialog.Font = Settings.Default.ButtonTextImageFont;
            }

            var result = fontDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                textBox.FontFamily = new FontFamily(fontDialog.Font.Name);
                textBox.FontSize = fontDialog.Font.Size * 96.0 / 72.0;
                textBox.FontWeight = fontDialog.Font.Bold ? FontWeights.Bold : FontWeights.Regular;
                textBox.FontStyle = fontDialog.Font.Italic ? FontStyles.Italic : FontStyles.Normal;
                
                var textDecorationCollection = new TextDecorationCollection();
                if (fontDialog.Font.Underline) textDecorationCollection.Add(TextDecorations.Underline);
                if (fontDialog.Font.Strikeout) textDecorationCollection.Add(TextDecorations.Strikethrough);
                textBox.TextDecorations = textDecorationCollection;

                Settings.Default.ButtonTextImageFont = fontDialog.Font;
                Settings.Default.Save();

                SetIsDirty();
            }
        }

        private void SetFontColor(TextBox textBox)
        {
            var colorDialog = new ColorDialog();
            colorDialog.Color = Settings.Default.ButtonTextImageFontColor;

            var result = colorDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                textBox.Foreground = new SolidColorBrush(Color.FromArgb(colorDialog.Color.A, colorDialog.Color.R, colorDialog.Color.G, colorDialog.Color.B));
                Settings.Default.ButtonTextImageFontColor = colorDialog.Color;
                Settings.Default.Save();
                SetIsDirty();
            }
        }

        private void SetBackgroundColor(TextBox textBox)
        {
            var colorDialog = new ColorDialog();
            colorDialog.Color = Settings.Default.ButtonTextImageBackgroundColor;

            var result = colorDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                textBox.Background = new SolidColorBrush(Color.FromArgb(colorDialog.Color.A, colorDialog.Color.R, colorDialog.Color.G, colorDialog.Color.B));
                Settings.Default.ButtonTextImageBackgroundColor = colorDialog.Color;
                Settings.Default.Save();
                SetIsDirty();
            }
        }
        
        private void TestImage(TextBox textBox)
        {
            var bitmap = BitMapCreator.CreateStreamDeckBitmap(textBox.Text, Tagg(textBox).TextFont, Tagg(textBox).FontColor, Tagg(textBox).BackgroundColor , Tagg(textBox).OffsetX, Tagg(textBox).OffsetY);
            SDUIParent.TestImage(bitmap);
        }

        public IStreamDeckUIParent SDUIParent { get; set; }

        private TagDataStreamDeckImage Tagg(TextBox textBox)
        {
            return (TagDataStreamDeckImage) textBox.Tag;
        }

        private void TextBoxButtonTextFace_OnKeyUp(object sender, KeyEventArgs e)
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

        private const int OFFSET_CHANGE_VALUE = 2;

        private void RepeatButtonActionPressUp_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                Tagg(TextBoxButtonOnTextFace).OffsetY -= OFFSET_CHANGE_VALUE;
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
                Tagg(TextBoxButtonOnTextFace).OffsetY += OFFSET_CHANGE_VALUE;
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
                Tagg(TextBoxButtonOnTextFace).OffsetX -= OFFSET_CHANGE_VALUE;
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
                Tagg(TextBoxButtonOnTextFace).OffsetX += OFFSET_CHANGE_VALUE;
                TestImage(TextBoxButtonOnTextFace);
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }
    }
}
