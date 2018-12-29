namespace Test
{
    using Microshaoft;
    using System;
    using System.Diagnostics;
    using System.IO;
    public class Program
    {

        public static void Main111()
        {

            var files = Directory.EnumerateFiles(@"d:\", "*.gz*", SearchOption.TopDirectoryOnly);
            foreach (var file in files)
            {
                var r = false;
                string path = string.Empty;
                try
                {
                    if
                        (
                            ZipFileHelper
                                .Decompress
                                    (
                                        file
                                        , @"d:\temp"
                                        , (x) =>
                                        {
                                            var s = ".Gz";
                                            int p = x.ToLower().LastIndexOf(s.ToLower());
                                            if (p > 0)
                                            {
                                                x = x.Remove(p + 1, s.Length);
                                            }
                                            return x;
                                        }
                                        , out path
                                    )
                        )
                    {
                        r = PathFileHelper
                                .MoveFileTo
                                    (
                                        path
                                        , @"d:\Temp"
                                        , @"d:\Temp2"

                                        , true
                                    );
                    }
                }
                catch (Exception e)
                {
                    string log = string
                                    .Format
                                        (
                                            "Process file: [{1}] caught exception:{0}{2}"
                                            , "\r\n"
                                            , file
                                            , e.ToString()
                                        );
                    Console.Error.WriteLine(log);
                    //EventLogHelper
                    //    .WriteEventLogEntry
                    //        (
                    //            ""
                    //            , 1001
                    //            , log
                    //            , 101
                    //            , EventLogEntryType.Error
                                
                    //        );
                    r = false;
                }
                if (r)
                {
                    PathFileHelper
                            .MoveFileTo
                                (
                                    file
                                    , @"d:\"
                                    , @"d:\Temp3"
                                    , true
                                );
                    Console.WriteLine("ok: {0}", file);
                }
                else
                {
                    PathFileHelper
                        .MoveFileTo
                            (
                                file
                                , @"d:\Temp"
                                , @"d:\Temp4"
                                , true
                            );
                }
            }
            Console.ReadLine();

        }
    }
}
namespace Microshaoft
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Runtime.InteropServices;
    public static class PathFileHelper
    {
        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.I1)]
        private static extern bool CreateSymbolicLink(string lpSymlinkFileName, string lpTargetFileName, int dwFlags);

        public static bool TryCreateFileSymbolicLink
                            (
                                string symlinkFileDirectoryName
                                , string targetFileFullName
                            )
        {
            var r = false;
            if (File.Exists(targetFileFullName))
            {
                var targetDirectoryName = Path.GetDirectoryName(targetFileFullName);
                var fileName = Path.GetFileName(targetFileFullName);
                var symlinkFileName = Path.Combine(symlinkFileDirectoryName, fileName);
                if (!Directory.Exists(symlinkFileDirectoryName))
                {
                    Directory.CreateDirectory(symlinkFileDirectoryName);
                }
                r = CreateSymbolicLink
                            (
                                symlinkFileName
                                , targetFileFullName
                                , 0
                            );
            }
            return r;
        }
        public static bool MoveFileTo
                            (
                                string sourceFullPathFileName
                                , string sourceDirectoryPath
                                , string destDirectoryPath
                                , bool deleteExistsDest = false
                                , string destFileName = null
                            )
        {
            var r = false;
            var destFullPathFileName = GetNewPath(sourceDirectoryPath, destDirectoryPath, sourceFullPathFileName);
            if (!string.IsNullOrEmpty(destFileName))
            {
                destFullPathFileName = Path.Combine
                                            (
                                                Path.GetDirectoryName(destFullPathFileName)
                                                , destFileName
                                            );
            }
            var destDirectory = Path.GetDirectoryName(destFullPathFileName);
            if (deleteExistsDest)
            {
                if (File.Exists(destDirectory))
                {
                    File.Delete(destDirectory);
                }
            }
            if (!Directory.Exists(destDirectory))
            {
                Directory.CreateDirectory(destDirectory);
            }
            if (deleteExistsDest)
            {
                if (File.Exists(destFullPathFileName))
                {
                    File.Delete(destFullPathFileName);
                }
            }
            File.Move(sourceFullPathFileName, destFullPathFileName);
            r = true;
            return r;
        }
        public static string GetValidPathOrFileName(string path, string replacement)
        {
            string s = string.Empty;
            var chars = Path.GetInvalidPathChars();
            chars = chars.Union(Path.GetInvalidFileNameChars()).ToArray();
            //Array
            //    .ForEach
            //        (
            //            chars
            //            , (x) =>
            //            {
            //                s = s.Replace(x.ToString(), replacement);
            //            }
            //        );

            foreach (var x in chars)
            {
                s = s.Replace(x.ToString(), replacement);
            }

            return s;
        }
        public static string GetNewPath(string oldDirectoryPath, string newDirectoryPath, string originalFileFullPath)
        {
            string newPath = newDirectoryPath;
            originalFileFullPath = Path.GetFullPath(originalFileFullPath);
            var directorySeparator = Path.DirectorySeparatorChar.ToString();
            oldDirectoryPath = Path.GetFullPath(oldDirectoryPath);
            newDirectoryPath = Path.GetFullPath(newDirectoryPath);
            if (!oldDirectoryPath.EndsWith(directorySeparator))
            {
                oldDirectoryPath += directorySeparator;
            }
            if (!newDirectoryPath.EndsWith(directorySeparator))
            {
                newDirectoryPath += directorySeparator;
            }
            string relativeDirectoryPath = string.Empty;
            int p = originalFileFullPath
                        .ToLower()
                        .IndexOf(oldDirectoryPath.ToLower());
            if (p >= 0)
            {
                p += oldDirectoryPath.Length;
                relativeDirectoryPath = originalFileFullPath.Substring(p);
                newPath = Path.Combine(newPath, relativeDirectoryPath);
            }
            newPath = Path.GetFullPath(newPath);
            return newPath;
        }
    }
}

