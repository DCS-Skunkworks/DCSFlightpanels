using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NonVisuals.StreamDeck
{
    public class ZipArchiver
    {
        private static string _exportFolderName = @"DCSFP_Export";

        public static void CreateZipFile(string exportName, string fileNameFullPath, List<string> filesToInclude)
        {
            var exportFolder = Path.GetTempPath() + _exportFolderName + "_" + Guid.NewGuid().ToString();

            Directory.CreateDirectory(exportFolder);

            var zipArchive = ZipFile.Open(exportFolder + "\\dcsfp_button_export.zip", ZipArchiveMode.Create);


        }
    }
}
