namespace Microshaoft
{
    using System;
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
    }
}
