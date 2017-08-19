
namespace Microshaoft
{
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    public static class JsonFileHelper
    {
        public static string WriteOneJArray<T>
                    (
                            IEnumerable<T> data
                            , string baseFileName
                            , int alignSeconds = 15
                            , string beginSplitter = "/*begin =====*/"
                            , string endSplitter = "/*end count:[{0}]=====*/"
                            , string fileExtensionName = ".JsonX"
                    )
        {
            var timeStamp = DateTimeHelper.GetAlignSecondsDateTime(DateTime.Now, alignSeconds).ToString("yyyy-MM-dd_HH-mm-ss");
            var directory = Path.GetDirectoryName(baseFileName);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            var count = data.Count();
            beginSplitter = string.Format(beginSplitter, count);
            endSplitter = string.Format(endSplitter, count);
            var fileName = string.Format("{1}{0}{2}{3}", ".", baseFileName, timeStamp, fileExtensionName);
            fileName = Path.Combine(directory, fileName);
            using (var fileStream = File.Open(fileName, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None))
            {
                using (var streamWriter = new StreamWriter(fileStream))
                {
                    var json = JsonConvert.SerializeObject(data);
                    //var r = JsonConvert.DeserializeObject<T[]>(json);
                    streamWriter.BaseStream.Seek(0, SeekOrigin.End);
                    streamWriter.WriteLine(beginSplitter);
                    streamWriter.WriteLine(json);
                    streamWriter.WriteLine(endSplitter);
                    streamWriter.Close();
                }
                fileStream.Close();
            }
            return fileName;
        }

        public static void ReadAllAsArrays<T>
                    (
                            string fileName
                            , Action<int, T[]> OnReadedOneJArrayProcessFunc = null
                            , string beginSplitterPrefix = "/*begin"
                            , string endSplitterPrefix = "/*end"
                    )
        {
            int i = 0;
            using (var fileStream = File.Open(fileName, FileMode.OpenOrCreate, FileAccess.Read, FileShare.None))
            {
                using (var streamReader = new StreamReader(fileStream))
                {
                    var line = string.Empty;
                    StringBuilder sb = new StringBuilder();
                    while (!streamReader.EndOfStream)
                    {
                        line = streamReader.ReadLine();
                        if (line.StartsWith(beginSplitterPrefix))
                        {

                        }
                        else if (line.StartsWith(endSplitterPrefix))
                        {
                            if (OnReadedOneJArrayProcessFunc != null)
                            {
                                i++;
                                var json = sb.ToString().Trim();
                                sb.Clear();
                                var r = JsonConvert.DeserializeObject<T[]>(json);
                                //Console.WriteLine("JArray: {0}", r.Count());
                                OnReadedOneJArrayProcessFunc(i, r);
                            }
                        }
                        else
                        {
                            sb.AppendLine(line);
                        }
                    }
                    sb = null;
                    streamReader.Close();
                }
                fileStream.Close();
            }
            //Console.WriteLine("记录:{0},文件:{1}", i ,fileName);
        }

    }
}
