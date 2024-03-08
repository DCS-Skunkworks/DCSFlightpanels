using System;
using System.Drawing;
using System.IO;

namespace DCSFP.Tests.Serialization.Common
{
    internal class RepositorySerialized
    {
        private const string FOLDER_PATH = @"Serialization\Resources";
        private const string PROJECT_FOLDER = "DCSFP.Tests";

        private string GetMockFolderPath()
        {
            string path = Directory.GetCurrentDirectory();
            while (true)
            {
                if (Path.GetFileName(path) == PROJECT_FOLDER)
                {
                    break;
                }
                path = Directory.GetParent(path).FullName;
            }
            return path;
        }

        private string GetFile(Type objectType)
        {
            string filePath = Path.Combine(GetMockFolderPath(), FOLDER_PATH, objectType.Name + ".json");

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException(filePath);
            }
            return filePath;
        }

        internal string GetSerializedObjectString(Type objectType)
        {
            return File.ReadAllText(GetFile(objectType));
        }

        /// <summary>
        /// Even if this function has 0 references, do not delete this, used in tests
        /// </summary>
        internal void SaveSerializedObjectToFile(Type objectType, string serialized)
        {
            string mockFolderPath = GetMockFolderPath();
            string filePath = Path.Combine(mockFolderPath, FOLDER_PATH, objectType.Name + ".json");
            File.WriteAllText(filePath, serialized);
        }

        internal Bitmap GetTestImageBitmap() {
            string mockFolderPath = GetMockFolderPath();
            string filePath = Path.Combine(mockFolderPath, FOLDER_PATH, "TestImage.png");
            return new Bitmap(filePath);
        }

        internal byte[] GetTestImageBytes() {
            using var stream = new MemoryStream();
            GetTestImageBitmap().Save(stream, System.Drawing.Imaging.ImageFormat.Png);
            return stream.ToArray();
        }
    }
}
