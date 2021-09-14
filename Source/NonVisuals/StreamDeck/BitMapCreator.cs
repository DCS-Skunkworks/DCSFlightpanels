namespace NonVisuals.StreamDeck
{
    using System;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Drawing.Imaging;
    using System.Drawing.Text;
    using System.IO;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;

    using ClassLibraryCommon;

    using MEF;

    using Color = System.Drawing.Color;
    using PixelFormat = System.Drawing.Imaging.PixelFormat;

    public static class BitMapCreator
    {
        public static Bitmap BitmapImage2Bitmap(BitmapImage bitmapImage)
        {
            using (var outStream = new MemoryStream())
            {
                BitmapEncoder bmpBitmapEncoder = new BmpBitmapEncoder();
                bmpBitmapEncoder.Frames.Add(BitmapFrame.Create(bitmapImage));
                bmpBitmapEncoder.Save(outStream);
                var bitmap = new Bitmap(outStream);

                return new Bitmap(bitmap);
            }
        }

        public static bool IsSmallerThanStreamdeckDefault(Bitmap bitmap)
        {
            return bitmap.Width < Constants.StreamDeckImageSideSize || bitmap.Height < Constants.StreamDeckImageSideSize;
        }

        /*
                public static Bitmap AdjustImage(Bitmap bitmap, float contrast, float brightness, float gamma)
                {
                    /*
                     * 1.0f is no change
                     *
                     * https://stackoverflow.com/questions/15408607/adjust-brightness-contrast-and-gamma-of-an-image
                     *
                    float adjustedBrightness = brightness - 1.0f;
                    // create matrix that will brighten and contrast the image
                    float[][] ptsArray ={
                        new float[] {contrast, 0, 0, 0, 0}, // scale red
                        new float[] {0, contrast, 0, 0, 0}, // scale green
                        new float[] {0, 0, contrast, 0, 0}, // scale blue
                        new float[] {0, 0, 0, 1.0f, 0}, // don't scale alpha
                        new float[] {adjustedBrightness, adjustedBrightness, adjustedBrightness, 0, 1}};
        
                    var imageAttributes = new ImageAttributes();
                    imageAttributes.ClearColorMatrix();
                    imageAttributes.SetColorMatrix(new ColorMatrix(ptsArray), ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
                    imageAttributes.SetGamma(gamma, ColorAdjustType.Bitmap);
        
                    var graphics = Graphics.FromImage(bitmap);
                    graphics.DrawImage(bitmap, new Rectangle(0, 0, bitmap.Width, bitmap.Height)
                        , 0, 0, bitmap.Width, bitmap.Height,
                        GraphicsUnit.Pixel, imageAttributes);
        
                    return bitmap;
                }
            */
        public static void SetContrast(Bitmap bmp, int threshold)
        {
            // https://efundies.com/adjust-the-contrast-of-an-image-in-c/
            var lockedBitmap = new LockBitmap(bmp);
            lockedBitmap.LockBits();

            var contrast = Math.Pow((100.0 + threshold) / 100.0, 2);

            for (int y = 0; y < lockedBitmap.Height; y++)
            {
                for (int x = 0; x < lockedBitmap.Width; x++)
                {
                    var oldColor = lockedBitmap.GetPixel(x, y);
                    var red = ((((oldColor.R / 255.0) - 0.5) * contrast) + 0.5) * 255.0;
                    var green = ((((oldColor.G / 255.0) - 0.5) * contrast) + 0.5) * 255.0;
                    var blue = ((((oldColor.B / 255.0) - 0.5) * contrast) + 0.5) * 255.0;
                    if (red > 255) red = 255;
                    if (red < 0) red = 0;
                    if (green > 255) green = 255;
                    if (green < 0) green = 0;
                    if (blue > 255) blue = 255;
                    if (blue < 0) blue = 0;

                    var newColor = Color.FromArgb(oldColor.A, (int)red, (int)green, (int)blue);
                    lockedBitmap.SetPixel(x, y, newColor);
                }
            }

            lockedBitmap.UnlockBits();
        }

        public static Bitmap CreateBitmap(int width, int height, Color color)
        {
            var bitmap = new Bitmap(width, height);
            var graphics = Graphics.FromImage(bitmap);
            using (var solidBrush = new SolidBrush(Color.FromArgb(color.R, color.G, color.B)))
            {
                graphics.FillRectangle(solidBrush, 0, 0, width, height);
            }

            return bitmap;
        }

        public static Bitmap AdjustBitmap(Bitmap originalImage, float brightness, float contrast, float gamma)
        {
            var result = new Bitmap(originalImage.Width, originalImage.Height);

            /*float brightness = 1.0f; // no change in brightness
            float contrast = 2.0f; // twice the contrast
            float gamma = 1.0f; // no change in gamma*/
            float adjustedBrightness = brightness - 1.0f;
            // create matrix that will brighten and contrast the image
            float[][] ptsArray =
                {
                    new[] { contrast, 0, 0, 0, 0 }, // scale red
                    new[] { 0, contrast, 0, 0, 0 }, // scale green
                    new[] { 0, 0, contrast, 0, 0 }, // scale blue
                    new[] { 0, 0, 0, 1.0f, 0 }, // don't scale alpha
                    new[] { adjustedBrightness, adjustedBrightness, adjustedBrightness, 0, 1 }
                };

            ImageAttributes imageAttributes = new ImageAttributes();
            imageAttributes.ClearColorMatrix();
            imageAttributes.SetColorMatrix(new ColorMatrix(ptsArray), ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
            imageAttributes.SetGamma(gamma, ColorAdjustType.Bitmap);
            var graphics = Graphics.FromImage(result);
            graphics.DrawImage(originalImage, new Rectangle(0, 0, result.Width, result.Height), 0, 0, originalImage.Width, originalImage.Height, GraphicsUnit.Pixel, imageAttributes);

            return result;
        }

        public static Color GetBackgroundColor(Bitmap bitmap)
        {
            var colorCounter = new ColorCounter();

            for (var y = 0; y < bitmap.Height; y++)
            {
                for (var x = 0; x < bitmap.Width; x++)
                {
                    colorCounter.RegisterColor(bitmap.GetPixel(x, y));
                }
            }

            return colorCounter.GetMajority();
        }

        public static Bitmap EnlargeBitmapCanvas(Bitmap bitmap)
        {
            // Create blank canvas
            var streamdeckSizedBitmap = new Bitmap(Constants.StreamDeckImageSideSize, Constants.StreamDeckImageSideSize);

            var graphics = Graphics.FromImage(streamdeckSizedBitmap);

            graphics.SmoothingMode = SmoothingMode.None;
            graphics.CompositingMode = CompositingMode.SourceCopy;
            graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            graphics.PixelOffsetMode = PixelOffsetMode.Half;
            graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

            var y = (Constants.StreamDeckImageSideSize / 2) - (bitmap.Height / 2);
            var x = (Constants.StreamDeckImageSideSize / 2) - (bitmap.Width / 2);

            // Paste source image on blank canvas, then save it as .png
            graphics.DrawImage(bitmap, x, y, bitmap.Width, bitmap.Height);
            // graphics.DrawImageUnscaled(bitmap, 0, 0);
            return streamdeckSizedBitmap;
        }

        public static BitmapImage Bitmap2BitmapImage(Bitmap bitmap)
        {
            try
            {
                using (MemoryStream memory = new MemoryStream())
                {
                    bitmap.Save(memory, ImageFormat.Bmp);
                    memory.Position = 0;
                    BitmapImage bitmapimage = new BitmapImage();
                    bitmapimage.BeginInit();
                    bitmapimage.StreamSource = memory;
                    bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
                    bitmapimage.EndInit();

                    return bitmapimage;
                }
            }
            catch (Exception e)
            {
                Common.LogError(e, "Failed to convert bitmap to bitmapimage.");
            }

            return null;
        }

        public static BitmapImage GetButtonImageFromResources(EnumStreamDeckButtonNames streamDeckButtonName, Color color)
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
                graphicsObject.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
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

            var bitmapData = bitmap.LockBits(rect, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);

            try
            {
                var size = (rect.Width * rect.Height) * 4;

                return BitmapSource.Create(bitmap.Width, bitmap.Height, bitmap.HorizontalResolution, bitmap.VerticalResolution, PixelFormats.Bgra32, null, bitmapData.Scan0, size, bitmapData.Stride);
            }
            finally
            {
                bitmap.UnlockBits(bitmapData);
            }
        }
    }
}
