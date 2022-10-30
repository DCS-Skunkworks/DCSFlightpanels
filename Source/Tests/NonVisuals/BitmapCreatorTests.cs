using NonVisuals.StreamDeck;
using System;
using System.Drawing;
using System.IO;
using System.Windows.Media.Imaging;
using Xunit;

namespace Tests.NonVisuals
{
    public class BitmapCreatorTests
    {
        private string _TestsResourcesFolder = @"NonVisuals\BitmapCreatorTestsResources\";

        [Fact]
        public void CreateEmptyStreamDeckBitmapShouldReturnExpectedBitmap()
        {
            Bitmap createdBitmap = BitMapCreator.CreateEmptyStreamDeckBitmap(Color.Red);
            Bitmap expectedBitmap = new(Path.Combine(_TestsResourcesFolder, "EmptyRed_StreamdeckBitmap_72x72.bmp"));
            Assert.True(CompareBitmaps(expectedBitmap, createdBitmap));
        }

        [Theory]
        [InlineData(1.0f, 4.0f, 1.0f, "OriginalAdjusted_1.0_4.0_1.0.bmp")]
        [InlineData(1.2f, 2.0f, 0.8f, "OriginalAdjusted_1.2_2.0_0.8.bmp")]
        [InlineData(0.7f, 0.8f, 0.9f, "OriginalAdjusted_0.7_0.8_0.9.bmp")]
        public void AdjustBitmap_ShouldReturnExpectedBitmap(float brightness, float contrast, float gamma, string expecteImage)
        {
            Bitmap originalBitmap = new(Path.Combine(_TestsResourcesFolder, "Original_150x150.bmp"));
            var adjustedBitmap = BitMapCreator.AdjustBitmap(originalBitmap, brightness, contrast, gamma);

            Bitmap expectedBitmap = new(Path.Combine(_TestsResourcesFolder, expecteImage));
            Assert.True(CompareBitmaps(expectedBitmap, adjustedBitmap));
        }

        [Theory]
        [InlineData("Original_150x150.bmp", "EnlargedBitmapCanvas_FromGreaterThan_72x72.bmp")]
        [InlineData("Original_50x50.bmp", "EnlargedBitmapCanvas_FromSmallerThan_72x72.bmp")]
        [InlineData("EmptyRed_StreamdeckBitmap_72x72.bmp", "EmptyRed_StreamdeckBitmap_72x72.bmp")]
        public void EnlargeBitmapCanvas_ShouldReturnExpectedBitmap(string sourceImage, string expectedImage)
        {
            Bitmap originalBitmap = new(Path.Combine(_TestsResourcesFolder, sourceImage));
            var enlargedBitmap = BitMapCreator.EnlargeBitmapCanvas(originalBitmap);

            Bitmap expectedBitmap = new(Path.Combine(_TestsResourcesFolder, expectedImage));
            Assert.True(CompareBitmaps(expectedBitmap, enlargedBitmap));
        }
        
        [Fact]
        public void Bitmap2BitmapImage_ToAndFrom_ShouldBePossible()
        {
            Bitmap originalBitmap = new(Path.Combine(_TestsResourcesFolder, "Original_150x150.bmp"));
            var bitmapImage = BitMapCreator.Bitmap2BitmapImage(originalBitmap);
            Assert.True(CompareBitmaps(BitMapCreator.BitmapImage2Bitmap(bitmapImage), originalBitmap));         
        }

        [Theory]
        [InlineData("Ab* 1_9/", "Arial",12 ,2 ,15, "Created_With_BackgroundColor_72x72_1.bmp")]
        [InlineData("1_9/Ab*c", "Courier New",9, 10, 15, "Created_With_BackgroundColor_72x72_2.bmp")]
        [InlineData("1_9/Ab*c", "Courier New", 10, 10, 15, "Created_With_BackgroundColor_72x72_3.bmp")]
        [InlineData("1234567890", "Calibri", 15, 5, 5, "Created_With_BackgroundColor_72x72_4.bmp")]
        [InlineData("1234567890", "Calibri", 11, 0, 0, "Created_With_BackgroundColor_72x72_5.bmp")]
        [InlineData("AbC", "Calibri", 20, 5, 60, "Created_With_BackgroundColor_72x72_6.bmp")]
        [InlineData("AbC", "Calibri", 20, -10, -10, "Created_With_BackgroundColor_72x72_7.bmp")]
        public void CreateStreamDeckBitmapWithBackGroundColor_ShouldProduceExpectedBitmap(string text, string fontFamily,int fontSize, int x, int y, string expectedImage)
        {
            var createdBitmap = BitMapCreator.CreateStreamDeckBitmap(text, new Font(new FontFamily(fontFamily), fontSize, FontStyle.Bold), Color.Yellow, Color.Blue, x, y);
            
            Bitmap expectedBitmap = new(Path.Combine(_TestsResourcesFolder, expectedImage));
            Assert.True(CompareBitmaps(expectedBitmap, createdBitmap));
        }

        [Theory]
        [InlineData("Ab* 1_9/", "Arial", 12, 2, 15, "BackGroundImage_CduU_72x72.bmp", "Created_With_BackgroundImage72_72x72_1.bmp")]
        [InlineData("1_9/Ab*c", "Courier New", 9, 10, 15, "BackGroundImage_CduU_72x72.bmp", "Created_With_BackgroundImage72_72x72_2.bmp")]
        [InlineData("1_9/Ab*c", "Courier New", 10, 10, 15, "BackGroundImage_CduU_72x72.bmp", "Created_With_BackgroundImage72_72x72_3.bmp")]
        [InlineData("1234567890", "Calibri", 15, 5, 5, "BackGroundImage_CduU_72x72.bmp", "Created_With_BackgroundImage72_72x72_4.bmp")]
        [InlineData("1234567890", "Calibri", 11, 0, 0, "BackGroundImage_CduU_72x72.bmp", "Created_With_BackgroundImage72_72x72_5.bmp")]
        [InlineData("AbC", "Calibri", 20, 5, 60, "BackGroundImage_CduU_72x72.bmp", "Created_With_BackgroundImage72_72x72_6.bmp")]
        [InlineData("AbC", "Calibri", 20, -10, -10, "BackGroundImage_CduU_72x72.bmp", "Created_With_BackgroundImage72_72x72_7.bmp")]

        [InlineData("Ab* 1_9/", "Arial", 12, 2, 15, "BackGroundImage_CduU_150x150.bmp", "Created_With_BackgroundImage150_72x72_1.bmp")]
        [InlineData("1_9/Ab*c", "Courier New", 9, 10, 15, "BackGroundImage_CduU_150x150.bmp", "Created_With_BackgroundImage150_72x72_2.bmp")]
        [InlineData("1_9/Ab*c", "Courier New", 10, 10, 15, "BackGroundImage_CduU_150x150.bmp", "Created_With_BackgroundImage150_72x72_3.bmp")]
        [InlineData("1234567890", "Calibri", 15, 5, 5, "BackGroundImage_CduU_150x150.bmp", "Created_With_BackgroundImage150_72x72_4.bmp")]
        [InlineData("1234567890", "Calibri", 11, 0, 0, "BackGroundImage_CduU_150x150.bmp", "Created_With_BackgroundImage150_72x72_5.bmp")]
        [InlineData("AbC", "Calibri", 20, 5, 60, "BackGroundImage_CduU_150x150.bmp", "Created_With_BackgroundImage150_72x72_6.bmp")]
        [InlineData("AbC", "Calibri", 20, -10, -10, "BackGroundImage_CduU_150x150.bmp", "Created_With_BackgroundImage150_72x72_7.bmp")]

        [InlineData("Ab* 1_9/", "Arial", 12, 2, 15, "BackGroundImage_CduU_30x30.bmp", "Created_With_BackgroundImage30_72x72_1.bmp")]
        [InlineData("1_9/Ab*c", "Courier New", 9, 10, 15, "BackGroundImage_CduU_30x30.bmp", "Created_With_BackgroundImage30_72x72_2.bmp")]
        [InlineData("1_9/Ab*c", "Courier New", 10, 10, 15, "BackGroundImage_CduU_30x30.bmp", "Created_With_BackgroundImage30_72x72_3.bmp")]
        [InlineData("1234567890", "Calibri", 15, 5, 5, "BackGroundImage_CduU_30x30.bmp", "Created_With_BackgroundImage30_72x72_4.bmp")]
        [InlineData("1234567890", "Calibri", 11, 0, 0, "BackGroundImage_CduU_30x30.bmp", "Created_With_BackgroundImage30_72x72_5.bmp")]
        [InlineData("AbC", "Calibri", 20, 5, 60, "BackGroundImage_CduU_30x30.bmp", "Created_With_BackgroundImage30_72x72_6.bmp")]
        [InlineData("AbC", "Calibri", 20, -10, -10, "BackGroundImage_CduU_30x30.bmp", "Created_With_BackgroundImage30_72x72_7.bmp")]
        public void CreateStreamDeckBitmapWithBackGroundImage_ShouldProduceExpectedBitmap(string text, string fontFamily, int fontSize, int x, int y, string backGroundImage, string expectedImage)
        {
            Bitmap backgroundImage = new(Path.Combine(_TestsResourcesFolder, backGroundImage));
            var createdBitmap = BitMapCreator.CreateStreamDeckBitmap(text, new Font(new FontFamily(fontFamily), fontSize, FontStyle.Bold), Color.Yellow, x, y, backgroundImage);
            Bitmap expectedBitmap = new(Path.Combine(_TestsResourcesFolder, expectedImage));
            Assert.True(CompareBitmaps(expectedBitmap, createdBitmap));
        }

        /// <summary>
        /// This is quite a dumb comparison function. 
        /// We only care about the size & pixel values, no encoding check or other fancy stuff.
        /// </summary>
        public bool CompareBitmaps(Bitmap bmp1, Bitmap bmp2)
        {
            if (bmp1 == null || bmp2 == null)
                return false;
            
            if (object.Equals(bmp1, bmp2))
                return true;

            if (!bmp1.Size.Equals(bmp2.Size))
                return false;
            
            if (bmp1.Width != bmp2.Width || bmp1.Height != bmp2.Height)
                return false;

            for (int x = 0; x < bmp1.Width; x++)
                for (int y = 0; y < bmp1.Height; y++)
                    if (bmp1.GetPixel(x, y) != bmp2.GetPixel(x, y))
                        return false;

            return true;
        }
    }
}
