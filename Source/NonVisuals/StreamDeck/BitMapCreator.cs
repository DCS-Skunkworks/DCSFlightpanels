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
        public static Bitmap CreateBitmapImage(string text, int fontSize, int height, int width, Color fontColor, Color backgroundColor)
        {
            var createdBitmap = new Bitmap(2, 2);

            var intWidth = width;
            var intHeight = height;

            // Create the Font object for the image text drawing.
            var font = new Font("Consolas", fontSize, FontStyle.Regular, GraphicsUnit.Pixel);

            // Create a graphics object to measure the text's width and height.
            var graphicsObject = Graphics.FromImage(createdBitmap);

            if (height == 0)
            {
                // This is where the bitmap size is determined.
                intHeight = (int)graphicsObject.MeasureString(text, font).Height;
            }

            if (width == 0)
            {
                // This is where the bitmap size is determined.
                intWidth = (int)graphicsObject.MeasureString(text, font).Width;
            }
            // Create the bmpImage again with the correct size for the text and font.
            createdBitmap = new Bitmap(createdBitmap, new Size(intWidth, intHeight));


            // Add the colors to the new bitmap.
            graphicsObject = Graphics.FromImage(createdBitmap);

            // Set Background color
            graphicsObject.Clear(backgroundColor);
            graphicsObject.SmoothingMode = SmoothingMode.HighQuality;



            graphicsObject.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
            graphicsObject.DrawString(text, font, new SolidBrush(fontColor), 0, 0, StringFormat.GenericDefault);

            graphicsObject.Flush();

            return createdBitmap;
        }

        public static BitmapSource CreateBitmapSourceFromGdiBitmap(Bitmap bitmap)
        {
            if (bitmap == null)
            {
                throw new ArgumentNullException("Bitmap argument was null.");
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
