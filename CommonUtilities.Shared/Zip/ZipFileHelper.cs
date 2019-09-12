namespace Microshaoft
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Compression;
    public static class ZipFileHelper
    {
        public static void Create
                    (
                        string zippedFileName
                        , IEnumerable<string> originalFiles
                        , Func<string, string> onCreateEntryProcessFunc //= null
                    )
        {
            using
                    (
                        var fileStream = new FileStream
                                                (
                                                    zippedFileName
                                                    , FileMode.OpenOrCreate
                                                    , FileAccess.Write
                                                    , FileShare.None
                                                )
                    )
            {
                using
                    (
                        var archive = new ZipArchive
                                                (
                                                    fileStream
                                                    , ZipArchiveMode
                                                                .Create
                                                )
                    )
                {
                    foreach (var file in originalFiles)
                    {
                        var entryName = onCreateEntryProcessFunc(file);
                        archive.CreateEntryFromFile(file, entryName);
                    }
                    //archive.Dispose();
                }
            }
        }
        public static bool Decompress
                            (
                                string originalFileFullPath
                                , string targetDirectoryPath
                                , Func<string, string> onNamingDecompressedFileProcessFunc
                                , out string decompressedFileFullPath
                            )
        {
            var r = false;
            using
                (
                    var originalFileStream = File
                                                .OpenRead
                                                    (
                                                        originalFileFullPath
                                                    )
                )
            {
                var originalFileExtensionName = Path.GetExtension(originalFileFullPath);
                var originalDirectoryPath = Path.GetDirectoryName(originalFileFullPath);
                decompressedFileFullPath = PathFileHelper.GetNewPath(originalDirectoryPath, targetDirectoryPath, originalFileFullPath);
                string fileName = Path.GetFileName(decompressedFileFullPath);
                string directory = Path.GetDirectoryName(decompressedFileFullPath);
                if (onNamingDecompressedFileProcessFunc != null)
                {
                    fileName = onNamingDecompressedFileProcessFunc(fileName);
                }
                decompressedFileFullPath = Path.Combine(directory, fileName);
                using (var decompressedFileStream = File.Create(decompressedFileFullPath))
                {
                    using (var decompressionStream = new GZipStream(originalFileStream, CompressionMode.Decompress))
                    {
                        decompressionStream.CopyTo(decompressedFileStream);
                        r = true;
                    }
                }
            }
            return r;
        }
    }
}

