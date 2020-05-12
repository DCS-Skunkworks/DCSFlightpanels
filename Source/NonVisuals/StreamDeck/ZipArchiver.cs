using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;

namespace NonVisuals.StreamDeck
{
    public static class ZipArchiver
    {
        public static void CreateZipFile(string zipFileName, List<string> filesToInclude)
        {
            var tempFolder = Path.GetTempPath() + Guid.NewGuid();
            var directoryInfo = Directory.CreateDirectory(tempFolder);

            if (!directoryInfo.Exists)
            {
                throw new Exception("Failed to create temporary directory for Zip operations.");
            }

            using (var memoryStream = new MemoryStream())
            {
                /*
                 * Stream is left open, according to Stackoverflow comments final tasks are done when ZipArchive is finalized
                 * so we need that to happen before we stream the content to the final zip file.
                 */
                using (var zipArchive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
                {
                    foreach (var file in filesToInclude)
                    {
                        var entry = zipArchive.CreateEntry(Path.GetFileName(file), CompressionLevel.Fastest);

                        using (var entryStream = entry.Open())
                        {
                            using (var stream = File.OpenRead(file))
                            {
                                stream.CopyTo(entryStream);
                            }
                        }
                    }
                }

                using (var fileStream = new FileStream(zipFileName, FileMode.Create))
                {
                    memoryStream.Seek(0, SeekOrigin.Begin);
                    memoryStream.CopyTo(fileStream);
                }
            }


            Directory.Delete(tempFolder);
        }
    }
}
