using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Color = System.Drawing.Color;
using PixelFormat = System.Drawing.Imaging.PixelFormat;

namespace NonVisuals.StreamDeck
{
    public static class BitMapCreator
    {
        public static Bitmap CreateBitmapImage(string text, int fontsize, int height, int width)
        {
            var createdBitmap = new Bitmap(2, 2);

            var intWidth = width;
            var intHeight = height;

            // Create the Font object for the image text drawing.
            System.Drawing.Font font = new System.Drawing.Font("Consolas", fontsize, FontStyle.Regular, GraphicsUnit.Pixel);

            // Create a graphics object to measure the text's width and height.
            var graphicsObject = Graphics.FromImage(createdBitmap);

            if (height > 0 && width > 0)
            {
                // This is where the bitmap size is determined.
                intWidth = (int)graphicsObject.MeasureString(text, font).Width;
                intHeight = (int)graphicsObject.MeasureString(text, font).Height;
            }

            // Create the bmpImage again with the correct size for the text and font.
            createdBitmap = new Bitmap(createdBitmap, new Size(intWidth, intHeight));


            // Add the colors to the new bitmap.
            graphicsObject = Graphics.FromImage(createdBitmap);

            // Set Background color
            graphicsObject.Clear(Color.White);
            graphicsObject.SmoothingMode = SmoothingMode.HighQuality;



            graphicsObject.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
            graphicsObject.DrawString(text, font, new SolidBrush(Color.Black), 0, 0, StringFormat.GenericDefault);

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
