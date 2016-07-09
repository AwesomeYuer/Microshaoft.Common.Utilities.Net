namespace Microshaoft
{
    //using ICSharpCode.SharpZipLib.Zip;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.IO.Compression;
    //using System.IO.Compression;
    public static class ZipHelper
    {

        public static void Create
                                (
                                    string zippedFileName
                                    , IEnumerable<string> originalFiles
                                    , Func<string, string> onCreateEntryProcessFunc//= null
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
                using (ZipArchive archive = new ZipArchive(fileStream, ZipArchiveMode.Create))
                {
                    foreach (var file in originalFiles)
                    {
                        var entryName = onCreateEntryProcessFunc(file);
                        archive.CreateEntryFromFile(file, entryName);
                    }
                    archive.Dispose();
                }
            }
        }
    }
}
