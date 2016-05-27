namespace ConsoleApplication
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Collections.Generic;
    using Microshaoft;
    using System.Diagnostics.Eventing.Reader;

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

            EventLogHelper
                    .Query
                        (
                            "Appliation"
                            , null
                            , (x, y) =>
                            {
                                Console.WriteLine("{0},{1}", x, y.Count());
                                return true;
                            }
                            
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
    using System.Diagnostics.Eventing.Reader;
    using System.Collections.Generic;

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

        public static void Query
                            (
                                string path
                                , string queryString = null
                                , Func<int, IEnumerable<EventRecord>,bool>
                                        onPagedProcessFunc = null
                                , int pageSize = 100
                                , PathType pathType = PathType.LogName
                                , EventLogSession eventLogSession = null
                            )
        {
            EventLogQuery query = null;
            if (queryString.IsNullOrEmptyOrWhiteSpace())
            {
               query = new EventLogQuery(path, pathType);
            }
            else
            {
                query = new EventLogQuery(path, pathType, queryString);
            }
            if (eventLogSession != null)
            {
                query.Session = eventLogSession;
            }
            var reader = new EventLogReader(query);
            EventRecord eventRecord = null;
            int i = 0;
            int page = 1;
            List<EventRecord> list = null;
            if (pageSize >= 0)
            {
                list = new List<EventRecord>();
            }

            while (null != (eventRecord = reader.ReadEvent()))
            {
                if (pageSize >= 0)
                {
                    list.Add(eventRecord);
                    if (i % pageSize == 0)
                    {
                        if (onPagedProcessFunc != null)
                        {
                            var r = onPagedProcessFunc(page, list);
                            list.Clear();
                            if (r)
                            {
                                break;
                            }
                            page++;
                        }
                    }
                    i++;
                }
            }

        }



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

