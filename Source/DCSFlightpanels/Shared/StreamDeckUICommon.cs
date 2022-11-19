namespace DCSFlightpanels.Shared
{
    using System;
    using System.Drawing;
    using System.IO;
    using System.Windows.Forms;

    using ClassLibraryCommon;

    using CustomControls;
    using Properties;

    using NonVisuals;

    using Color = System.Drawing.Color;
    using NonVisuals.Panels.StreamDeck;

    public static class StreamDeckUICommon
    {

        public static DialogResult BrowseForImage(ref string initialDirectory, ref string imageRelativePath)
        {

            FileDialog fileDialog = new OpenFileDialog()
            {
                CheckPathExists = true,
                CheckFileExists = true,
                InitialDirectory = string.IsNullOrEmpty(initialDirectory) ? Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) : initialDirectory,
                Filter = @"Image files|*.jpg;*.jpeg;*.png"
            };
            
            var result = fileDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                imageRelativePath = Common.GetRelativePath(Common.GetApplicationPath(), fileDialog.FileName);
                initialDirectory = Path.GetDirectoryName(fileDialog.FileName);
            }

            return result;
        }

        public static DialogResult BrowseForSoundFile(ref string initialDirectory, ref string imageRelativePath)
        {

            FileDialog fileDialog = new OpenFileDialog()
            {
                CheckPathExists = true,
                CheckFileExists = true,
                InitialDirectory = string.IsNullOrEmpty(initialDirectory) ? Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) : initialDirectory,
                Filter = @"Sound files|*.mp3;*.wav"
            };

            var result = fileDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                imageRelativePath = Common.GetRelativePath(Common.GetApplicationPath(), fileDialog.FileName);
                initialDirectory = Path.GetDirectoryName(fileDialog.FileName);
            }

            return result;
        }

        public static DialogResult SetFontStyle(StreamDeckFaceTextBox textBox)
        {
            var font = SettingsManager.DefaultFont;
            var result = SetFontStyle(ref font);

            if (result == DialogResult.OK)
            {
                textBox.Bill.TextFont = font;
            }

            return result;
        }

        public static DialogResult SetFontStyle(ref Font font)
        {
            var fontDialog = new FontDialog()
            {
                FixedPitchOnly = true,
                FontMustExist = true,
                MinSize = 6,
                Font = SettingsManager.DefaultFont
            };

            var result = fontDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                font = fontDialog.Font;
                SettingsManager.DefaultFont = fontDialog.Font;
                Settings.Default.Save();
            }

            return result;
        }

        public static DialogResult SetFontColor(StreamDeckFaceTextBox textBox)
        {

            var color = Color.Transparent;
            var result = SetFontColor(ref color);

            if (result == DialogResult.OK)
            {
                textBox.Bill.FontColor = color;
            }

            return result;
        }

        public static DialogResult SetFontColor(ref Color color)
        {
            var colorDialog = new ColorDialog()
            {
                Color = SettingsManager.DefaultFontColor,
                CustomColors = StreamDeckConstants.GetOLEColors()
            };

            var result = colorDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                color = colorDialog.Color;
                SettingsManager.DefaultFontColor = colorDialog.Color;
                Settings.Default.Save();
            }

            return result;
        }

        public static DialogResult SetBackgroundColor(StreamDeckFaceTextBox textBox)
        {
            var color = Color.Transparent;
            var result = SetBackgroundColor(ref color);

            if (result == DialogResult.OK)
            {
                textBox.Bill.BackgroundColor = color;
            }

            return result;
        }

        public static DialogResult SetBackgroundColor(ref Color color)
        {
            var colorDialog = new ColorDialog()
            {
                Color = SettingsManager.DefaultBackgroundColor,
                CustomColors = StreamDeckConstants.GetOLEColors(),
            };

            var result = colorDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                color = colorDialog.Color;
                SettingsManager.DefaultBackgroundColor = colorDialog.Color;
                Settings.Default.Save();
            }

            return result;
        }
    }
}
