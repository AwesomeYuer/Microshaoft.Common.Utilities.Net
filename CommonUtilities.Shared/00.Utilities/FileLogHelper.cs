//logHelper
namespace Microshaoft
{
    using System;
    using System.IO;
    using System.Text;

    /// <summary>
    /// Utility 的摘要说明。
    /// </summary>
    public class FileLogHelper
    {

        public static void LogToTimeAlignedFile
                                        (
                                            string log
                                            , string logType
                                            , string logFileRootDirectoryPath
                                            , int alignSeconds = 300
                                        )
        {
            var now = DateTime.Now;
            var alignedTime = DateTimeHelper.GetAlignSecondsDateTime
                                        (
                                            now
                                            , alignSeconds
                                        ).ToString("yyyy-MM-dd_HH-mm");


            log = string.Format
                            (
                                "{1}{0}{2}{0}{3}{0}{4}"
                                , "\r\n"
                                , "begin===================="
                                , DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fffff")
                                , log
                                , "end======================"
                            );
            var fileName = string.Format
                            (
                                "{1}{0}{2}{0}{3}{0}{4}"
                                , @"\"
                                , logFileRootDirectoryPath
                                , now.ToString("yyyy-MM-dd")
                                , ""
                                , string.Format
                                            (
                                                "{1}{0}{2}{0}{3}"
                                                , "."
                                                , alignedTime
                                                , logType
                                                , "log.txt"
                                            )
                            );
            FileLogHelper.WriteLog
                    (
                        fileName
                        , log
                        , logType
                    );
        }
        public static void WriteFile
            (
                string fileName
                , string content
                , Encoding encoding
            )
        {

            string path = Path.GetDirectoryName(fileName);

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            using (FileStream fs = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite))
            {
                StreamWriter sw = new StreamWriter(fs, encoding);
                sw.BaseStream.Seek(0, SeekOrigin.End);
                sw.WriteLine("\nBegin:==============================================");
                sw.WriteLine(content);
                sw.WriteLine("\nEnd;================================================");
                sw.Flush();

                sw.Close();
            }
        }
        public static void WriteLog
            (
                string fileName
                , string content
                , string logType
            )
        {

            WriteLog(fileName, content, logType, "\r\n", Encoding.GetEncoding("gb2312"));
        }
        public static void WriteLog
            (
                string fileName
                , string content
                , string logType
                , string delimiter
                , Encoding encoding
            )
        {

            string s = string.Format
                (
                    "{1}: {0}{2}{0}{3}"
                    , delimiter
                    , logType
                    , DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fffff")
                    , content
                );

            WriteFile(fileName, s, encoding);
        }
    }
}

