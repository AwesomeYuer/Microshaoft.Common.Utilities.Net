namespace Microshaoft
{
    using System;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    public static class WindowsConsoleHelper
    {
        public static void AllocateConsole()
        {
            AllocConsole();
            //Console.OpenStandardOutput();

            //IntPtr stdHandle = GetStdHandle(STD_OUTPUT_HANDLE);
            ////SafeFileHandle safeFileHandle = new SafeFileHandle(stdHandle, true);
            ////FileStream fileStream = new FileStream(safeFileHandle, FileAccess.Write);
            ////Encoding encoding = System.Text.Encoding.GetEncoding(MY_CODE_PAGE);
            //TextWriter writer = new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true };
            ////StreamWriter standardOutput = new StreamWriter(fileStream, encoding);
            ////standardOutput.AutoFlush = true;
            //Console.SetOut(writer);  



            //// stdout's handle seems to always be equal to 7
            //IntPtr defaultStdout = new IntPtr(7);
            //IntPtr currentStdout = GetStdHandle(StdOutputHandle);

            //if (currentStdout != defaultStdout)
            //{
            //    // reset stdout
            //    SetStdHandle(StdOutputHandle, defaultStdout);
            //}


            // reopen stdout
            //TextWriter writer = new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true };
            //Console.SetOut(writer);
        }

        public static bool RegisterOnCtrlProcessFunc(Func<CtrlTypes, string, bool> OnCtrlProcessFunc)
        {
            return
                SetConsoleCtrlHandler
                        (
                            new consoleCtrlHandler
                                    (
                                        (ctrlTypes) =>
                                        {
                                            //var xxx = ctrlTypes;
                                            //var @event = Enum.GetName(typeof(CtrlTypes), xxx);
                                            return
                                                OnCtrlProcessFunc
                                                        (
                                                            ctrlTypes
                                                            , "@event"
                                                        );
                                        }
                                    )
                                    , true
                        );
        }

        public static bool TryExit(Process process, int? timeoutInMilliseconds = null)
        {
            if (process is null)
            {
                throw new ArgumentNullException(nameof(process));
            }
            if (process.HasExited)
            {
                return true;
            }
            // 尝试将我们自己的进程附加到指定进程的控制台（如果有的话）。
            if (AttachConsole((uint)process.Id))
            {
                // 我们自己的进程需要忽略掉 Ctrl+C 信号，否则自己也会退出。
                SetConsoleCtrlHandler(null, true);

                // 将 Ctrl+C 信号发送到前面已关联（附加）的控制台进程中。
                GenerateConsoleCtrlEvent(CtrlTypes.CTRL_C_EVENT, 0);

                // 拾前面已经附加的控制台。
                FreeConsole();

                bool hasExited;
                // 由于 Ctrl+C 信号只是通知程序关闭，并不一定真的关闭。所以我们等待一定时间，如果仍未关闭，则超时不处理。
                // 业务可以通过判断返回值来角是否进行后续处理（例如强制杀掉）。
                if (timeoutInMilliseconds == null)
                {
                    // 如果没有超时处理，则一直等待，直到最终进程停止。
                    process.WaitForExit();
                    hasExited = true;
                }
                else
                {
                    // 如果有超时处理，则超时候返回。
                    hasExited = process.WaitForExit(timeoutInMilliseconds.Value);
                }

                // 重新恢复我们自己的进程对 Ctrl+C 信号的响应。
                SetConsoleCtrlHandler(null, false);

                return hasExited;
            }
            else
            {
                return false;
            }
        }


        // P/Invoke required:
        private const UInt32 StdOutputHandle = 0xFFFFFFF5;
        [DllImport("kernel32.dll")]
        private static extern IntPtr GetStdHandle(int nStdHandle);
        [DllImport("kernel32.dll")]
        private static extern void SetStdHandle(UInt32 nStdHandle, IntPtr handle);
        [DllImport("kernel32")]
        static extern bool AllocConsole();
        [DllImport("kernel32.dll")]
        public static extern bool FreeConsole();
        private const int STD_OUTPUT_HANDLE = -11;
        private const int MY_CODE_PAGE = 437;

        [DllImport("kernel32.dll")]
        private static extern bool AttachConsole(uint dwProcessId);

        [DllImport("kernel32.dll")]
        private static extern bool SetConsoleCtrlHandler(consoleCtrlHandler HandlerRoutine, bool Add);

        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GenerateConsoleCtrlEvent(CtrlTypes dwCtrlEvent, uint dwProcessGroupId);

        private delegate bool consoleCtrlHandler(CtrlTypes CtrlType);
    }
    public enum CtrlTypes : uint
    {
        CTRL_C_EVENT = 0,
        CTRL_BREAK_EVENT = 1,
        CTRL_CLOSE_EVENT = 2,
        CTRL_LOGOFF_EVENT = 5,
        CTRL_SHUTDOWN_EVENT = 6
    }
}
