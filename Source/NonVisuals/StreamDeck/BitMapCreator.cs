using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Color = System.Drawing.Color;
using PixelFormat = System.Drawing.Imaging.PixelFormat;

namespace NonVisuals.StreamDeck
{
    public static class BitMapCreator
    {
        public static BitmapImage GetButtonNumberImage(EnumStreamDeckButtonNames streamDeckButtonName, Color color)
        {
            return new BitmapImage(new Uri(StreamDeckConstants.NUMBER_BUTTON_LOCATION + StreamDeckCommon.ButtonNumber(streamDeckButtonName) + "_" + color.Name.ToLower() + ".png", UriKind.Absolute));
        }

        public static Bitmap CreateStreamDeckBitmap(string text, Font font, Color fontColor, int offsetX, int offsetY, Bitmap backgroundBitmap)
        {
            return CreateBitmapImage(text, font, offsetX, offsetY, fontColor, Color.Transparent, backgroundBitmap);
        }

        public static Bitmap CreateStreamDeckBitmap(string text, Font font, Color fontColor, Color backgroundColor, int offsetX, int offsetY)
        {
            return CreateBitmapImage(text, font, offsetX, offsetY, fontColor, backgroundColor);
        }

        private static Bitmap CreateBitmapImage(string text, Font font, int offsetX, int offsetY, Color fontColor, Color backgroundColor, Bitmap backgroundBitmap = null)
        {
            Bitmap createdBitmap;

            if (backgroundBitmap == null)
            {
                createdBitmap = new Bitmap(StreamDeckConstants.STREAMDECK_ICON_WIDTH, StreamDeckConstants.STREAMDECK_ICON_HEIGHT);
            }
            else
            {
                createdBitmap = new Bitmap(backgroundBitmap, StreamDeckConstants.STREAMDECK_ICON_WIDTH, StreamDeckConstants.STREAMDECK_ICON_HEIGHT);
            }

            // Create a graphics object to measure the text's width and height.
            var graphicsObject = Graphics.FromImage(createdBitmap);
            
            // Set Background color
            if (backgroundBitmap == null)
            {
                graphicsObject.Clear(backgroundColor);
            }

            graphicsObject.SmoothingMode = SmoothingMode.HighQuality;

            if (!string.IsNullOrEmpty(text))
            {
                graphicsObject.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
                graphicsObject.DrawString(text, font, new SolidBrush(fontColor), offsetX, offsetY, StringFormat.GenericDefault);
            }

            graphicsObject.Flush();

            return createdBitmap;
        }

        public static Bitmap CreateEmptyStreamDeckBitmap(Color color)
        {
            return CreateStreamDeckBitmap(null, null, color, color, 0, 0);
        }

        public static BitmapSource CreateBitmapSourceFromGdiBitmap(Bitmap bitmap)
        {
            if (bitmap == null)
            {
                throw new Exception("Bitmap argument was null.");
            }

            var rect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);

            var bitmapData = bitmap.LockBits(
                rect,
                ImageLockMode.ReadWrite,
                PixelFormat.Format32bppArgb);

            try
            {
                var size = (rect.Width * rect.Height) * 4;

                return BitmapSource.Create(
                    bitmap.Width,
                    bitmap.Height,
                    bitmap.HorizontalResolution,
                    bitmap.VerticalResolution,
                    PixelFormats.Bgra32,
                    null,
                    bitmapData.Scan0,
                    size,
                    bitmapData.Stride);
            }
            finally
            {
                bitmap.UnlockBits(bitmapData);
            }
        }



    }
}
