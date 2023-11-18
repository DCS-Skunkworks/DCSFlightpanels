using SixLabors.ImageSharp;
using System;
using System.IO;
using System.Threading.Tasks;
using OpenMacroBoard.SDK.GraphicPrimitives;

namespace OpenMacroBoard.SDK
{
    /// <summary>
    /// A basic factory extension to create <see cref="KeyBitmap"/>s
    /// </summary>
    public static class KeyBitmapBasicFactoryExtensions
    {
        /// <summary>
        /// Creates a single color (single pixel) <see cref="KeyBitmap"/> with a given color.
        /// </summary>
        /// <param name="keyFactory">The builder that is used to create the <see cref="KeyBitmap"/></param>
        /// <param name="r">Red channel.</param>
        /// <param name="g">Green channel.</param>
        /// <param name="b">Blue channel.</param>
        public static KeyBitmap FromRgb(this IKeyBitmapFactory keyFactory, byte r, byte g, byte b)
        {
            // If everything is 0 (black) take a shortcut ;-)
            if (r == 0 && g == 0 && b == 0)
            {
                return KeyBitmap.Black;
            }

            var buffer = new byte[3] { b, g, r };
            return KeyBitmap.FromBgr24Array(1, 1, buffer);
        }

        /// <summary>
        /// Creates a single color (single pixel) <see cref="KeyBitmap"/> with a given color.
        /// </summary>
        /// <param name="keyFactory">The builder that is used to create the <see cref="KeyBitmap"/></param>
        /// <param name="color">The color.</param>
        public static KeyBitmap FromColor(this IKeyBitmapFactory keyFactory, OmbColor color)
        {
            return keyFactory.FromRgb(color.R, color.G, color.B);
        }

        /// <summary>
        /// Create a bitmap from an encoded given image stream.
        /// </summary>
        public static KeyBitmap FromStream(this IKeyBitmapFactory builder, Stream bitmapStream)
        {
            bitmapStream.Position = 0;
            return builder.FromImageSharpImage(Image.Load(bitmapStream));
        }

        /// <summary>
        /// Create a bitmap from an encoded given image stream.
        /// </summary>
        public static async Task<KeyBitmap> FromStreamAsync(this IKeyBitmapFactory builder, Stream bitmapStream)
        {
            bitmapStream.Position = 0;
            return builder.FromImageSharpImage(await Image.LoadAsync(bitmapStream));
        }

        /// <summary>
        /// Create a bitmap from an encoded given image file.
        /// </summary>
        public static KeyBitmap FromFile(this IKeyBitmapFactory builder, string bitmapFile)
        {
            return builder.FromImageSharpImage(Image.Load(bitmapFile));
        }

        /// <summary>
        /// Create a bitmap from an encoded given image file.
        /// </summary>
        public static async Task<KeyBitmap> FromFileAsync(this IKeyBitmapFactory builder, string bitmapFile)
        {
            return builder.FromImageSharpImage(await Image.LoadAsync(bitmapFile));
        }

        /// <summary>
        /// Creates a <see cref="KeyBitmap"/> from a given <see cref="Image{Bgr24}"/>.
        /// </summary>
        /// <exception cref="ArgumentNullException">The provided bitmap is null.</exception>
        /// <exception cref="NotSupportedException">The pixel format of the image is not supported.</exception>
        public static KeyBitmap FromImageSharpImage(this IKeyBitmapFactory keyFactory, Image image)
        {
            if (image is null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            using var ctx = image.WithBgr24();
            var pixelData = ctx.Item.ToBgr24PixelArray();
            return KeyBitmap.FromBgr24Array(image.Width, image.Height, pixelData);
        }
    }
}
