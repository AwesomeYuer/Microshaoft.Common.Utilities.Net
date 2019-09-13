namespace Microshaoft
{
    using System;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Threading;

    public static class WindowsBackgroundInteractiveConsole
    {
        /// <summary>
        /// 启动控制台
        /// </summary>
        /// <returns></returns>
        /// 
        [DllImport("kernel32.dll")]
        private static extern bool AllocConsole();
        /// <summary>
        /// 释放控制台
        /// </summary>
        /// <returns></returns>
        [DllImport("kernel32.dll")]
        private static extern bool FreeConsole();

        public static void Alloc()
        {
            var r = AllocConsole();
            new Thread
                    (
                        () =>
                        {
                            Console.Title = string.Format("Background Debug Console For : [{0}]", Console.Title);
                            var input = string.Empty;
                            TextWriter consoleOut = Console.Out;
                            StreamWriter writer = null;
                            Action<string, bool> consoleSetOut = (path, append) =>
                            {
                                if (writer != null)
                                {
                                    writer.Close();
                                    writer.Dispose();
                                    writer = null;
                                }
                                writer = new StreamWriter(path, append)
                                {
                                    AutoFlush = true
                                };
                                Console.SetOut(writer);
                            };

                            Action help = () =>
                            {
                                TextWriter temp = Console.Out;
                                temp.Flush();
                                Console.SetOut(consoleOut);
                                Console.WriteLine("Help:");

                                Console.WriteLine
                                            (
                                                "Command: {0}, Comment: {1}, Sample : \n\"{2}\""
                                                , "help"
                                                , "控制台输出帮助信息"
                                                , "help"
                                            );
                                Console.WriteLine
                                            (
                                                "Command: {0}, Comment: {1}, Sample : \n\"{2}\""
                                                , ">>"
                                                , "控制台输出至指定的追加文件"
                                                , ">>c:\a.txt"
                                            );
                                Console.WriteLine
                                            (
                                                "Command: {0}, Comment: {1}, Sample : \n\"{2}\""
                                                , ">"
                                                , "控制台输出至指定的文件"
                                                , ">c:\a.txt"
                                            );
                                Console.WriteLine
                                            (
                                                "Command: {0}, Comment: {1}, Sample : \n\"{2}\""
                                                , "<"
                                                , "将输出重置回控制台"
                                                , "<"
                                            );
                                Console.WriteLine
                                            (
                                                "Command: {0}, Comment: {1}, Sample : \n\"{2}\""
                                                , "flush"
                                                , "强制将当前缓冲区数据输出"
                                                , "flush"
                                            );
                                Console.WriteLine
                                            (
                                                "Command: {0}, Comment: {1}, Sample : \n\"{2}\""
                                                , "cls"
                                                , "控制台清屏"
                                                , "cls"
                                            );
                                Console.WriteLine
                                            (
                                                "Command: {0}, Comment: {1}, Sample : \n\"{2}\""
                                                , "now"
                                                , "输出当前时间"
                                                , "now"
                                            );
                                consoleOut.Flush();
                                Console.SetOut(temp);
                            };
                            Console.WriteLine("======================");
                            while ("q" != (input = Console.ReadLine()))
                            {
                                try
                                {
                                    if (string.Compare("cls", input, true) == 0)
                                    {
                                        Console.Clear();
                                    }
                                    else if (string.Compare("now", input, true) == 0)
                                    {
                                        Console.WriteLine("{0}{1}{0}", "\t", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fffff"));
                                    }
                                    else if (input.StartsWith(">>"))
                                    {
                                        var path = input.TrimStart('>');
                                        var directory = Path.GetDirectoryName(path);
                                        if (Directory.Exists(directory))
                                        {
                                            Directory.CreateDirectory(directory);
                                        }
                                        consoleSetOut(path, true);
                                    }
                                    else if (string.Compare("flush", input, true) == 0)
                                    {
                                        writer.Flush();
                                    }
                                    else if (input.StartsWith(">"))
                                    {
                                        var path = input.TrimStart('>');
                                        var directory = Path.GetDirectoryName(path);
                                        if (Directory.Exists(directory))
                                        {
                                            Directory.CreateDirectory(directory);
                                        }
                                        consoleSetOut(path, false);
                                    }
                                    else if (input.StartsWith("<"))
                                    {
                                        if (writer != null)
                                        {
                                            writer.Flush();
                                            writer.Close();
                                            writer.Dispose();
                                            writer = null;
                                        }
                                        Console.SetOut(consoleOut);
                                    }
                                    else if (string.Compare("help", input, true) == 0)
                                    {
                                        help();
                                    }
                                    else
                                    {
                                        Console.WriteLine("echo : [{0}]", input);
                                    }
                                }
                                catch (Exception exception)
                                {
                                    Console.WriteLine("BackgroundInteractiveConsole Caught Exception:{0}{1}", "\r\n", exception.ToString());

                                }
                            }
                            FreeConsole();
                        }
                    )
            {
                IsBackground = true
            }.Start();
        }
    }
}