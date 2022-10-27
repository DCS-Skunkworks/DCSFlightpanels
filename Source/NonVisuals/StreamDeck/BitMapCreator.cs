namespace NonVisuals.StreamDeck
{
    using System;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Drawing.Imaging;
    using System.Drawing.Text;
    using System.IO;
    using System.Reflection;
    using System.Windows.Media.Imaging;

    using NLog;

    public static class BitMapCreator
    {
        internal static Logger logger = LogManager.GetCurrentClassLogger();

        public static Bitmap BitmapImage2Bitmap(BitmapImage bitmapImage)
        {
            using MemoryStream outStream = new();
            BmpBitmapEncoder bmpBitmapEncoder = new();
            bmpBitmapEncoder.Frames.Add(BitmapFrame.Create(bitmapImage));
            bmpBitmapEncoder.Save(outStream);
            Bitmap bitmap = new(outStream);

            return new Bitmap(bitmap);
        }

        public static BitmapImage Bitmap2BitmapImage(Bitmap bitmap)
        {
            try
            {
                using MemoryStream memory = new();
                bitmap.Save(memory, ImageFormat.Bmp);
                memory.Position = 0;
                BitmapImage bitmapImage = new();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();

                return bitmapImage;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Failed to convert Bitmap to BitmapImage.");
            }
            return null;
        }

        public static bool IsSmallerThanStreamdeckDefault(Bitmap bitmap)
        {
            return bitmap.Width < Constants.StreamDeckImageSideSize || bitmap.Height < Constants.StreamDeckImageSideSize;
        }

        public static Bitmap AdjustBitmap(Bitmap originalImage, float brightness, float contrast, float gamma)
        {
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

            ImageAttributes imageAttributes = new();
            imageAttributes.ClearColorMatrix();
            imageAttributes.SetColorMatrix(new ColorMatrix(ptsArray), ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
            imageAttributes.SetGamma(gamma, ColorAdjustType.Bitmap);

            Bitmap resultBitmap = new(originalImage.Width, originalImage.Height);
            Graphics graphics = Graphics.FromImage(resultBitmap);
            graphics.DrawImage(originalImage, new Rectangle(0, 0, resultBitmap.Width, resultBitmap.Height), 0, 0, originalImage.Width, originalImage.Height, GraphicsUnit.Pixel, imageAttributes);

            return resultBitmap;
        }

        public static Bitmap EnlargeBitmapCanvas(Bitmap bitmap)
        {
            // Create blank canvas
            Bitmap streamdeckSizedBitmap = new(Constants.StreamDeckImageSideSize, Constants.StreamDeckImageSideSize);

            Graphics graphics = Graphics.FromImage(streamdeckSizedBitmap);
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

        public static Bitmap CreateStreamDeckBitmap(string text, Font font, Color fontColor, int offsetX, int offsetY, Bitmap backgroundBitmap)
        {
            return CreateBitmapImage(text, font, offsetX, offsetY, fontColor, Color.Transparent, backgroundBitmap);
        }

        public static Bitmap CreateStreamDeckBitmap(string text, Font font, Color fontColor, int offsetX, int offsetY, Color backgroundColor)
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
            Graphics graphicsObject = Graphics.FromImage(createdBitmap);

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
            return CreateStreamDeckBitmap(null, null, color, 0, 0, color);
        }

        public static Bitmap BitmapOrFileNotFound(string imagePath)
        {
            return File.Exists(imagePath) ? new Bitmap(imagePath) : FileNotFoundBitmap();
        }

        public static Bitmap FileNotFoundBitmap()
        {
            var assembly = Assembly.GetExecutingAssembly();

            BitmapImage tmpBitMapImage = new();
            using (var stream = assembly.GetManifestResourceStream(@"NonVisuals.Images.filenotfound.png"))
            {
                tmpBitMapImage.BeginInit();
                tmpBitMapImage.StreamSource = stream;
                tmpBitMapImage.CacheOption = BitmapCacheOption.OnLoad;
                tmpBitMapImage.EndInit();
            }

            return BitmapImage2Bitmap(tmpBitMapImage);
        }
    }
}
