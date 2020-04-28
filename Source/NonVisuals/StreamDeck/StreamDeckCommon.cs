using System;
using System.Drawing;
using System.IO;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using ClassLibraryCommon;

namespace NonVisuals.StreamDeck
{
    public static class StreamDeckCommon
    {


        public static int ButtonNumber(EnumStreamDeckButtonNames streamDeckButtonName)
        {
            if (streamDeckButtonName == EnumStreamDeckButtonNames.BUTTON0_NO_BUTTON)
            {
                return -1;
            }

            return int.Parse(streamDeckButtonName.ToString().Replace("BUTTON", ""));
        }

        public static EnumStreamDeckButtonNames ButtonName(int streamDeckButtonNumber)
        {
            if (streamDeckButtonNumber == 0)
            {
                return EnumStreamDeckButtonNames.BUTTON0_NO_BUTTON;
            }

            return (EnumStreamDeckButtonNames)Enum.Parse(typeof(EnumStreamDeckButtonNames), "BUTTON" + streamDeckButtonNumber);
        }

        public static EnumStreamDeckButtonNames ButtonName(string streamDeckButtonNumber)
        {
            if (string.IsNullOrEmpty(streamDeckButtonNumber) || streamDeckButtonNumber == "0")
            {
                return EnumStreamDeckButtonNames.BUTTON0_NO_BUTTON;
            }

            return (EnumStreamDeckButtonNames)Enum.Parse(typeof(EnumStreamDeckButtonNames), "BUTTON" + streamDeckButtonNumber);
        }



        public static Bitmap BitmapImage2Bitmap(BitmapImage bitmapImage)
        {
            using (var outStream = new MemoryStream())
            {
                BitmapEncoder bmpBitmapEncoder = new BmpBitmapEncoder();
                bmpBitmapEncoder.Frames.Add(BitmapFrame.Create(bitmapImage));
                bmpBitmapEncoder.Save(outStream);
                var bitmap = new System.Drawing.Bitmap(outStream);

                return new Bitmap(bitmap);
            }
        }

        public static BitmapImage ConvertBitMap(Bitmap bitmap)
        {
            try
            {
                using (MemoryStream memory = new MemoryStream())
                {
                    bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
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


        public static EnumComparator ComparatorValue(string text)
        {
            if (text == "NotSet")
            {
                return EnumComparator.NotSet;
            }
            if (text == "==")
            {
                return EnumComparator.Equals;
            }
            if (text == "!=")
            {
                return EnumComparator.NotEquals;
            }
            if (text == "<")
            {
                return EnumComparator.LessThan;
            }
            if (text == "<=")
            {
                return EnumComparator.LessThanEqual;
            }
            if (text == ">")
            {
                return EnumComparator.GreaterThan;
            }
            if (text == ">=")
            {
                return EnumComparator.GreaterThanEqual;
            }
            if (text == "Always")
            {
                return EnumComparator.Always;
            }
            throw new Exception("Failed to decode comparison type.");
        }

        public static void SetComparatorValue(ComboBox comboBox, EnumComparator comparator)
        {
            switch (comparator)
            {
                case EnumComparator.NotSet:
                    {
                        comboBox.Text = "NotSet";
                        break;
                    }
                case EnumComparator.Equals:
                    {
                        comboBox.Text = "==";
                        break;
                    }
                case EnumComparator.NotEquals:
                    {
                        comboBox.Text = "!=";
                        break;
                    }
                case EnumComparator.LessThan:
                    {
                        comboBox.Text = "<";
                        break;
                    }
                case EnumComparator.LessThanEqual:
                    {
                        comboBox.Text = "<=";
                        break;
                    }
                case EnumComparator.GreaterThan:
                    {
                        comboBox.Text = ">";
                        break;
                    }
                case EnumComparator.GreaterThanEqual:
                    {
                        comboBox.Text = ">=";
                        break;
                    }
                case EnumComparator.Always:
                    {
                        comboBox.Text = "Always";
                        break;
                    }
                default:
                    {
                        throw new Exception("Failed to decode comparison type.");
                    }
            }
        }

    }
}
