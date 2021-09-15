namespace DCSFlightpanels.Shared
{
    using System;
    using System.Drawing;
    using System.IO;
    using System.Windows.Forms;

    using ClassLibraryCommon;

    using DCSFlightpanels.CustomControls;
    using DCSFlightpanels.Properties;

    using NonVisuals;
    using NonVisuals.StreamDeck;

    using Color = System.Drawing.Color;

    public static class StreamDeckUICommon
    {

        public static DialogResult BrowseForImage(ref string initialDirectory, ref string imageRelativePath)
        {
            
            FileDialog fileDialog = new OpenFileDialog();
            fileDialog.CheckPathExists = true;
            fileDialog.CheckFileExists = true;
            
            fileDialog.InitialDirectory = string.IsNullOrEmpty(initialDirectory) ? Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) : initialDirectory;
            fileDialog.Filter = @"Image files|*.jpg;*.jpeg;*.png";

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

            FileDialog fileDialog = new OpenFileDialog();
            fileDialog.CheckPathExists = true;
            fileDialog.CheckFileExists = true;

            fileDialog.InitialDirectory = string.IsNullOrEmpty(initialDirectory) ? Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) : initialDirectory;
            fileDialog.Filter = @"Sound files|*.mp3;*.wav";

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
            var fontDialog = new FontDialog();

            fontDialog.FixedPitchOnly = true;
            fontDialog.FontMustExist = true;
            fontDialog.MinSize = 6;

            fontDialog.Font = SettingsManager.DefaultFont;


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
            var colorDialog = new ColorDialog();
            colorDialog.Color = SettingsManager.DefaultFontColor;
            colorDialog.CustomColors = StreamDeckConstants.GetOLEColors();

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
            var colorDialog = new ColorDialog();
            colorDialog.Color = SettingsManager.DefaultBackgroundColor;
            colorDialog.CustomColors = StreamDeckConstants.GetOLEColors();
            var result = colorDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                color = colorDialog.Color;
                SettingsManager.DefaultBackgroundColor = colorDialog.Color;
                Settings.Default.Save();
            }

            return result;
        }
        

        public static Bitmap FileNotFoundBitmap
        {
            get
            {
                var uri = new Uri("pack://application:,,,/dcsfp;component/Images/filenotfound.png");
                return new Bitmap(uri.AbsolutePath);
            }
        }


    }
}
