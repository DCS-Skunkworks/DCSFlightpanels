using System;
using System.Drawing;
using System.IO;

namespace Tests.Common {
    public class RepositorySerializedBase
    {
        private readonly string _ResourcesFolderPath = @"Serialization\Resources";
        private readonly string _ProjectFolder = "DCS-BIOS.Tests";

        public RepositorySerializedBase(string resourcesFolderPath, string projectFolder) {
            _ResourcesFolderPath = resourcesFolderPath;
            _ProjectFolder = projectFolder;
        }

        private string GetMockFolderPath()
        {
            string path = Directory.GetCurrentDirectory();
            while (true)
            {
                if (Path.GetFileName(path) == _ProjectFolder)
                {
                    break;
                }
                path = Directory.GetParent(path).FullName;
            }
            return path;
        }

        private string GetFile(Type objectType)
        {
            string filePath = Path.Combine(GetMockFolderPath(), _ResourcesFolderPath, objectType.Name + ".json");

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException(filePath);
            }
            return filePath;
        }

        public string GetSerializedObjectString(Type objectType)
        {
            return File.ReadAllText(GetFile(objectType));
        }

        /// <summary>
        /// Even if this function has 0 references, do not delete this, used in tests
        /// </summary>
        public void SaveSerializedObjectToFile(Type objectType, string serialized)
        {
            string mockFolderPath = GetMockFolderPath();
            string filePath = Path.Combine(mockFolderPath, _ResourcesFolderPath, objectType.Name + ".json");
            File.WriteAllText(filePath, serialized);
        }

        public Bitmap GetTestImageBitmap() {
            string mockFolderPath = GetMockFolderPath();
            string filePath = Path.Combine(mockFolderPath, _ResourcesFolderPath, "TestImage.png");
            return new Bitmap(filePath);
        }

        public byte[] GetTestImageBytes() {
            using var stream = new MemoryStream();
            GetTestImageBitmap().Save(stream, System.Drawing.Imaging.ImageFormat.Png);
            return stream.ToArray();
        }
    }
}
