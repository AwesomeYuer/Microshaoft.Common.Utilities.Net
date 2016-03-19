namespace ConsoleApplication
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Collections.Generic;
    using Microshaoft;
    public class Program
    {
        static void Main1(string[] args)
        {
            //
            // TODO: 在此处添加代码以启动应用程序
            //
            var log = "TestLog001";
            var source = "TestSource001";
            var eventLogs = EventLog.GetEventLogs();
            Array.ForEach
                    (
                        eventLogs
                        , (x) =>
                        {
                            Console.WriteLine
                                        (
                                            "Source: {1}{0}Log: {2}"
                                            , "\t"
                                            , x.Source
                                            , x.Log
                                        );
                        }
                    );
            EventLogHelper.TryCreateEventLogSource(log, source);
            EventLogHelper.WriteEventLogEntry
                            (
                                source
                                , "TestMessage"
                                , EventLogEntryType.Information
                                , 1000
                                , 1
                            );
            Console.WriteLine("Hello World");
            Console.WriteLine(Environment.Version.ToString());
        }
    }
}
namespace Microshaoft
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Diagnostics;
    public static class EventLogHelper
    {

        private static string _processNameID = new Func<string>
                                                (
                                                    () =>
                                                    {
                                                        var process = Process.GetCurrentProcess();
                                                        return
                                                        string.Format
                                                                (
                                                                    "{1}{0}({2})"
                                                                    , ""
                                                                    , process.ProcessName
                                                                    , process.Id
                                                                );
                                                    }
                                                )();


        public static EventLog[] GetEventLogs()
        {
            var r = EventLog.GetEventLogs();
            return r;
        }

        public static void WriteEventLogEntry
                                        (
            //string logName,
                                            string sourceName,
                                            string logMessage,
                                            EventLogEntryType logEntryType
                                            , int eventID
                                            , short category
                                        )
        {
            EventLog eventLog = new EventLog();
            eventLog.Source = sourceName;
            //eventLog.Log = logName;

            logMessage = string.Format
                            (
                                "{1}{0}Process [{3}] @ {4}{0}{5}{0}{2}"
                                , "\r\n"
                                , "begin ==========================================="
                                , "end ============================================="
                                , _processNameID
                                , DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fffff")
                                , logMessage
                            );


            eventLog.WriteEntry
                (
                    logMessage
                    , logEntryType
                    , eventID
                    , category
                );
        }
        public static bool TryCreateEventLogSource
                                        (
                                            string logName,
                                            string sourceName,
                                            bool allowRecreate = false
                                        )
        {
            bool r = false;

            if (EventLog.SourceExists(sourceName))
            {
                if (allowRecreate)
                {
                    try
                    {
                        var s = EventLog.LogNameFromSourceName(sourceName, ".");
                        if (string.Compare(s, logName, true) != 0)
                        {
                            EventLog.DeleteEventSource(sourceName);
                            EventLog.Delete(logName);
                            EventLog.CreateEventSource(sourceName, logName);
                            r = true;
                        }
                    }
                    catch// (Exception e)
                    {
                        r = false;
                    }
                }
                else
                {
                    r = true;
                }

            }
            else
            {
                EventLog.CreateEventSource(sourceName, logName);
                r = true;
            }
            return r;
        }
    }
}
