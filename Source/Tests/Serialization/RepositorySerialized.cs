using System;
using System.IO;


namespace DCSFPTests.Serialization {
    internal class RepositorySerialized {
        private const string _folderPath = @"Serialization\Resources";
        private const string _projectFolder = "Tests";

        private string GetMockFolderPath() {
            string path = Directory.GetCurrentDirectory();
            while (true) {
                if (Path.GetFileName(path) == _projectFolder) {
                    break;
                }
                path = Directory.GetParent(path).FullName;
            }

            return path;
        }

        private string GetFile(Type objectType) {
            string filePath = Path.Combine(GetMockFolderPath(), _folderPath, objectType.Name + ".json");

            if (!File.Exists(filePath)) {
                throw new FileNotFoundException(filePath);
            }
            return filePath;
        }

        internal string GetSerializedObjectString(Type objectType) {
            return File.ReadAllText(GetFile(objectType));
        }

        internal void SaveSerializedObjectToFile(Type objectType, string serialized) {
            string mockFolderPath = GetMockFolderPath();
            string filePath = Path.Combine(mockFolderPath, _folderPath, objectType.Name + ".json");
            File.WriteAllText(filePath, serialized);
        }

    }
}
