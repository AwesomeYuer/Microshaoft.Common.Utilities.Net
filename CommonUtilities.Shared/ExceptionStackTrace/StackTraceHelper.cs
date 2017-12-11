
namespace Microshaoft
{
    using System;
    using System.Diagnostics;
    using System.Text;
    using System.Reflection;
    using System.IO;
    public static class StackTraceHelper
    {
        public static string EnhancedStackTrace(Exception exception)
        {
            return EnhancedStackTrace(new StackTrace(exception, true));
        }
        public static string EnhancedStackTrace(StackTrace stackTrace)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(Environment.NewLine);
            sb.Append("---- Stack Trace ----");
            sb.Append(Environment.NewLine);

            for (int i = 0; i < stackTrace.FrameCount; i++)
            {
                StackFrame sf = stackTrace.GetFrame(i);
                MemberInfo mi = sf.GetMethod();
                sb.Append(StackFrameToString(sf));
            }
            sb.Append(Environment.NewLine);

            return sb.ToString();
        }
        public static string StackFrameToString(StackFrame stackFrame)
        {
            StringBuilder sb = new StringBuilder();
            int intParam; MemberInfo mi = stackFrame.GetMethod();
            sb.Append("   ");
            sb.Append(mi.DeclaringType.Namespace);
            sb.Append(".");
            sb.Append(mi.DeclaringType.Name);
            sb.Append(".");
            sb.Append(mi.Name);
            // -- build method params           
            sb.Append("(");
            intParam = 0;
            foreach (ParameterInfo param in stackFrame.GetMethod().GetParameters())
            {
                intParam += 1;
                sb.Append(param.Name);
                sb.Append(" As ");
                sb.Append(param.ParameterType.Name);
            }
            sb.Append(")");
            sb.Append(Environment.NewLine);
            // -- if source code is available, append location info           
            sb.Append("       ");
            if (string.IsNullOrEmpty(stackFrame.GetFileName()))
            {
                sb.Append("(unknown file)");
                //-- native code offset is always available               
                sb.Append(": N ");
                sb.Append(string.Format("{0:#00000}", stackFrame.GetNativeOffset()));
            }
            else
            {
                sb.Append(Path.GetFileName(stackFrame.GetFileName()));
                sb.Append(": line ");
                sb.Append(string.Format("{0:#0000}", stackFrame.GetFileLineNumber()));
                sb.Append(", col ");
                sb.Append(string.Format("{0:#00}", stackFrame.GetFileColumnNumber()));
                if (stackFrame.GetILOffset() != StackFrame.OFFSET_UNKNOWN)
                {
                    sb.Append(", IL ");
                    sb.Append(string.Format("{0:#0000}", stackFrame.GetILOffset()));
                }
            }
            sb.Append(Environment.NewLine);
            return sb.ToString();
        }
    }

}
