namespace Microshaoft
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    public static class FileShardingHelper
    {
        public static IEnumerable<string> ShardingFileParts
                                        (
                                            string originalFile
                                            , int singlePartSize = 1024 * 1024
                                            , string fileExtensionNameSuffix = "x"
                                        )

        {
            var directory = Path.GetDirectoryName(originalFile);
            var fileName = Path.GetFileNameWithoutExtension(originalFile);
            var fileExtensionName = Path.GetExtension(originalFile);
            fileName = Path.Combine
                                (
                                    directory
                                    , fileName
                                );
            var destFileNamePattern = fileName + ".{0}.{1}" + fileExtensionName + fileExtensionNameSuffix;
            var fileInfo = new FileInfo(originalFile);
            //var list = new List<string>();
            var length = fileInfo.Length;
            if
                (
                    length
                    >
                    singlePartSize
                )
            {
                using
                    (
                        var sourceStream = File.OpenRead(originalFile)
                    )
                {
                    var parts = Math.Ceiling((1.0d * length) / singlePartSize);
                    var part = 0;
                    var buffers = StreamDataHelper
                                    .ReadDataToBuffers
                                        (
                                            sourceStream
                                            , singlePartSize
                                        );
                    foreach (var buffer in buffers)
                    {
                        part++;
                        var destPartFileName = string
                                                    .Format
                                                        (
                                                            destFileNamePattern
                                                            , parts
                                                            , part
                                                        );
                        //zippedPartFileName += "x";
                        using
                            (
                                var fileStream = new FileStream
                                                        (
                                                            destPartFileName
                                                            , FileMode.Create
                                                            , FileAccess.Write
                                                            , FileShare.None
                                                        )
                            )
                        {
                            fileStream.Write(buffer, 0, buffer.Length);
                            fileStream.Close();
                            //list.Add(zippedPartFileName);
                            yield return destPartFileName;
                        }
                    }
                }
                File.Delete(originalFile);
                //File.Delete(fileName);
            }
            else
            {
                //list.Add(zippedFileName);
                var destPartFileName
                            = string.Format
                                    (
                                        destFileNamePattern
                                        , 1
                                        , 1
                                    );
                File.Move
                        (
                            originalFile
                            , destPartFileName
                        );

                yield return destPartFileName;
            }
        }
        public static IEnumerable<string> MergeShardedFilesParts
                                (
                                    string directory
                                    , string searchPattern = "*.*x"
                                    , string mergedFileExtensionName = ".zip"
                                )
        {
            var files = Directory
                                .EnumerateFiles
                                    (
                                        directory
                                        , searchPattern
                                    ).Distinct();
            var filesGroups = files
                                    .GroupBy
                                        (
                                            (x) =>
                                            {
                                                var fileName = Path.GetFileName(x);
                                                var fileNameSegments = fileName.Split('.');
                                                var take = fileNameSegments.Length - 2;
                                                return
                                                    fileNameSegments
                                                        .Take(take)
                                                        .Aggregate
                                                            (
                                                                (xx, yy) =>
                                                                {
                                                                    return
                                                                        string
                                                                            .Format
                                                                                (
                                                                                    "{1}{0}{2}"
                                                                                    , "."
                                                                                    , xx
                                                                                    , yy
                                                                                );
                                                                }
                                                            );
                                            }
                                        );
            foreach (var filesGroup in filesGroups)
            {
                var filePartsName = Path.GetFileNameWithoutExtension(filesGroup.Key);
                filePartsName += mergedFileExtensionName;
                filePartsName = Path.Combine(directory, filePartsName);
                var orderedFilesGroup
                            = filesGroup
                                .OrderBy
                                    (
                                        (x) =>
                                        {
                                            var fileName = Path.GetFileName(x);
                                            var fileNameSegments = fileName.Split('.');
                                            var i = fileNameSegments.Length - 2;
                                            var r = int.Parse(fileNameSegments[i]);
                                            return r;
                                        }
                                    );
                var segments = filesGroup.Key.Split('.');
                var ii = segments.Length - 1;
                var parts = int.Parse(segments[ii]);
                if (File.Exists(filePartsName))
                {
                    File.Delete(filePartsName);
                }
                var mergedZipFiles = MergePartialFilesGroupProcess
                                            (
                                                filePartsName
                                                , orderedFilesGroup
                                                , parts
                                            );
                foreach (var mergedZipFile in mergedZipFiles)
                {
                    yield return mergedZipFile;
                }
            }
        }
        private static IEnumerable<string> MergePartialFilesGroupProcess
                                    (
                                        string mergedFileName
                                        , IOrderedEnumerable<string> orderedPartialFilesGroup
                                        , int parts
                                    )
        {
            var list = new List<string>();
            int i = 0;
            foreach (var file in orderedPartialFilesGroup)
            {
                i++;
                var fileName = Path.GetFileName(file);
                var fileNameSegments = fileName.Split('.');
                var ii = fileNameSegments.Length - 2;
                int part = int.Parse(fileNameSegments[ii]);
                if (part == i)
                {
                    using
                            (
                                var sourceStream = File.OpenRead(file)
                            )
                    {
                        using
                            (
                                var fileStream
                                        =
                                            (
                                                parts > 1
                                                ?
                                                new FileStream
                                                        (
                                                            mergedFileName
                                                            , FileMode.OpenOrCreate
                                                            , FileAccess.Write
                                                            , FileShare.None
                                                        )
                                                :
                                                new FileStream
                                                        (
                                                            mergedFileName
                                                            , FileMode.Create
                                                            , FileAccess.Write
                                                            , FileShare.None
                                                        )
                                            )
                            )
                        {
                            if (parts > 1 && i > 1)
                            {
                                fileStream.Seek(0, SeekOrigin.End);
                            }
                            var buffers = StreamDataHelper
                                                .ReadDataToBuffers
                                                    (
                                                        sourceStream
                                                        , 64 * 1024 //* 1024
                                                    );
                            foreach (var buffer in buffers)
                            {
                                fileStream.Write(buffer, 0, buffer.Length);
                            }
                            fileStream.Close();
                        }
                        sourceStream.Close();
                        list.Add(file);
                    }
                    if (i == parts)
                    {
                        //ExtractFileProcess(zippedFileName);
                        yield return mergedFileName;
                        foreach (var x in list)
                        {
                            File.Delete(x);
                        }
                    }
                }
                else
                {
                    if (File.Exists(mergedFileName))
                    {
                        File.Delete(mergedFileName);
                    }
                    break;
                }
            }
            list.Clear();
        }
    }
}
