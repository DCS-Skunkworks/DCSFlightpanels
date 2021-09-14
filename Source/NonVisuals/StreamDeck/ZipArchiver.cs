namespace NonVisuals.StreamDeck
{
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Compression;

    public static class ZipArchiver
    {

        public static bool ZipFileContainsFile(string zipFileName, string filenameInsideArchive)
        {
            var result = false;

            using (var zipArchive = ZipFile.OpenRead(zipFileName))
            {
                foreach (var zipArchiveEntry in zipArchive.Entries)
                {
                    if (zipArchiveEntry.Name == filenameInsideArchive)
                    {
                        result = true;
                        break;
                    }
                }
            }

            return result;
        }

        public static void ExtractZipFile(string zipFileName, string extractFolder)
        {
            // ZipFile.ExtractToDirectory(zipFileName, extractFolder);
            var zipArchive = ZipFile.OpenRead(zipFileName);

            foreach (var zipArchiveEntry in zipArchive.Entries)
            {
                using (var zipEntryStream = zipArchiveEntry.Open())
                {
                    using (var fileStream = File.Create(extractFolder + "\\" + zipArchiveEntry.Name ))
                    {
                        zipEntryStream.CopyTo(fileStream);
                    }
                }
            }
        }

        public static void CreateZipFile(string zipFileName, List<string> filesToInclude)
        {
            
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

        }
    }
}
