#if NETFRAMEWORK4_X
namespace Microshaoft
{
    using Newtonsoft.Json;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.Eventing.Reader;
    using System.Globalization;
    using System.Linq;
    using System.Net.Http;
    //using System.Runtime.ExceptionServices;
    using System.Security;
    public static class EventLogHelper
    {
        [Flags]
        public enum AppDomainExceptionsHandlerType
        {
            None = 0
            , UnhandledException = 2
            , FirstChanceException = 4
            
        }

        private static AppDomainExceptionsHandlerType _registeredExceptionsType = AppDomainExceptionsHandlerType.None;
        


        public static void RegisterAutoEventLogExceptions
                                        (
                                            string sourceName
                                            , int eventID //设计时指定
                                            , short category
                                            , AppDomainExceptionsHandlerType exceptionsType = AppDomainExceptionsHandlerType.None
                                        )
        {
            if (exceptionsType.HasFlag(AppDomainExceptionsHandlerType.UnhandledException))
            {
                if (_registeredExceptionsType.HasFlag(AppDomainExceptionsHandlerType.UnhandledException))
                {
                    _registeredExceptionsType |= AppDomainExceptionsHandlerType.UnhandledException;
                    AppDomain
                           .CurrentDomain
                           .UnhandledException +=
                                   (
                                       (sender, e) =>
                                       {
                                           var logMessage = string
                                                                .Format
                                                                    (
                                                                        "{1} On Type Of Sender: [{2}], Value Of Sender: [{3}]"
                                                                        , "\r\n"
                                                                        , Enum
                                                                            .GetName
                                                                                (
                                                                                    typeof(AppDomainExceptionsHandlerType)
                                                                                    , AppDomainExceptionsHandlerType
                                                                                            .UnhandledException
                                                                                )
                                                                        , sender.GetType()
                                                                        , sender
                                                                    );
                                           logMessage = string
                                                            .Format
                                                                (
                                                                    "{1}{0}{2}"
                                                                    , ":\r\n"
                                                                    , logMessage
                                                                    , e
                                                                        .ExceptionObject
                                                                        .ToString()
                                                                );
                                           WriteEventLogEntry
                                               (
                                                   //string logName,
                                                   sourceName,
                                                   eventID,
                                                   logMessage,
                                                   category,
                                                   EventLogEntryType.Error //设计时指定
                                               );
                                       }
                                   );
                }
            }
            if (exceptionsType.HasFlag(AppDomainExceptionsHandlerType.FirstChanceException))
            {
                if (_registeredExceptionsType.HasFlag(AppDomainExceptionsHandlerType.FirstChanceException))
                {
                    _registeredExceptionsType |= AppDomainExceptionsHandlerType.FirstChanceException;
                    AppDomain
                           .CurrentDomain
                           .FirstChanceException +=
                                   (
                                       (sender, e) =>
                                       {
                                           var logMessage = string
                                                                .Format
                                                                    (
                                                                        "{1} On Type Of Sender: [{2}], Value Of Sender: [{3}]"
                                                                        , "\r\n"
                                                                        , Enum
                                                                            .GetName
                                                                                (
                                                                                    typeof(AppDomainExceptionsHandlerType)
                                                                                    , AppDomainExceptionsHandlerType
                                                                                            .FirstChanceException
                                                                                )
                                                                        , sender.GetType()
                                                                        , sender
                                                                    );
                                           logMessage = string
                                                            .Format
                                                                (
                                                                    "{1}{0}{2}"
                                                                    , ":\r\n"
                                                                    , logMessage
                                                                    , e
                                                                        .Exception
                                                                        .ToString()
                                                                );
                                           WriteEventLogEntry
                                               (
                                                   //string logName,
                                                   sourceName,
                                                   eventID,
                                                   logMessage,
                                                   category,
                                                   EventLogEntryType.Error //设计时指定
                                               );
                                       }
                                   );
                }
            }
        }
        private static string _processNameID
                                = new Func<string>
                                        (
                                            () =>
                                            {
                                                var process = Process.GetCurrentProcess();
                                                return
                                                    string
                                                        .Format
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
        /// <summary>
        /// 数据库访问失败时捕获异常时调用此方法, 如:连接失败、执行失败
        /// </summary>
        /// <param name="sourceName"></param>
        /// <param name="logMessage">通常为 exception.ToString() </param>
        public static void DataAccessFailed(string sourceName, string logMessage)
        {
            logMessage = string.Format("{1}{0}{2}", ":\r\n", "DataAccessFailed", logMessage);
            WriteEventLogEntry
                (
                    //string logName,
                    sourceName,
                    101,
                    logMessage,
                    1,
                    EventLogEntryType.Error //设计时指定
                );
        }
        /// <summary>
        /// 网络访问失败时捕获异常时调用此方法 ,如: 作为客户端访问 WebAPI 接口时, 访问网络共享目录时 网络访问失败
        /// </summary>
        /// <param name="sourceName"></param>
        /// <param name="logMessage">通常为 exception.ToString() </param>
        public static void NetworkAccessFailed(string sourceName, string logMessage)
        {
            logMessage = string.Format("{1}{0}{2}", ":\r\n", "NetworkAccessFailed", logMessage);
            WriteEventLogEntry
                (
                    //string logName,
                    sourceName,
                    102,
                    logMessage,
                    2,
                    EventLogEntryType.Error //设计时指定
                );
        }
        /// <summary>
        /// 本机资源访问失败时捕获异常时调用此方法 ,如: 磁盘目录、文件访问时失败
        /// </summary>
        /// <param name="sourceName"></param>
        /// <param name="logMessage">通常为 exception.ToString() </param>
        public static void LocalResourceAccessFailed(string sourceName, string logMessage)
        {
            WriteEventLogEntry
                (
                    //string logName,
                    sourceName,
                    103,
                    logMessage,
                    3,
                    EventLogEntryType.Error //设计时指定
                );
        }
        /// <summary>
        /// 外设资源访问失败时捕获异常时调用此方法 ,如: 卡箱
        /// </summary>
        /// <param name="sourceName"></param>
        /// <param name="logMessage">通常为 exception.ToString() </param>
        public static void DeviceAccessFailed(string sourceName, string logMessage)
        {
            logMessage = string.Format("{1}{0}{2}", ":\r\n", "DeviceAccessFailed", logMessage);
            WriteEventLogEntry
                (
                    //string logName,
                    sourceName,
                    104,
                    logMessage,
                    4,
                    EventLogEntryType.Error //设计时指定
                );
        }

        /// <summary>
        /// 远程调用失败时捕获异常时调用此方法 ,如: 作为客户端访问 WebAPI 接口时, 访问返回失败结果
        /// </summary>
        /// <param name="sourceName"></param>
        /// <param name="logMessage">通常为 exception.ToString() </param>
        public static void RemoteProcedureCallFailed(string sourceName, string logMessage)
        {
            logMessage = string.Format("{1}{0}{2}", ":\r\n", "RemoteProcedureCallFailed", logMessage);
            WriteEventLogEntry
                (
                    //string logName,
                    sourceName,
                    105,
                    logMessage,
                    5,
                    EventLogEntryType.Error //设计时指定
                );
        }
        /// <summary>
        /// 通用 EventLog API
        /// </summary>
        /// <param name="sourceName">事件源 设计时按规范指定粒度：组件</param>
        /// <param name="eventID">事件ID 设计时指定 , EventID > 1000 在本事件源中不重复</param>
        /// <param name="logMessage">日志内容 设计时指定或运行时(如：捕获到的异常)调用方动态赋值</param>
        /// <param name="category">任务分类Category 设计时指定</param>
        /// <param name="logEntryType">条目类型用于报警级别 设计时指定默认值, 运行时可能被替换为动态分级</param>
        public static void WriteEventLogEntry
                (
                    //string logName,
                    string sourceName, //设计时指定
                    int eventID, //设计时指定
                    string logMessage,
                    short category,
                    EventLogEntryType logEntryType = EventLogEntryType.Information //设计时指定
                )
        {
            WriteEventLogEntry
                (
                    sourceName
                    , eventID
                    , logMessage
                    , category
                    , logEntryType
                    , null
                );
        }
        /// <summary>
        /// 通用 EventLog API
        /// </summary>
        /// <param name="sourceName">事件源 设计时按规范指定粒度：组件</param>
        /// <param name="eventID">事件ID 设计时指定 , EventID > 1000 在本事件源中不重复</param>
        /// <param name="logMessage">日志内容 设计时指定或运行时(如：捕获到的异常)调用方动态赋值</param>
        /// <param name="category">任务分类Category 设计时指定</param>
        /// <param name="logEntryType">条目类型用于报警级别 设计时指定默认值, 运行时可能被替换为动态分级</param>
        public static void WriteEventLogEntry
                (
                    //string logName,
                    string sourceName, //设计时指定
                    int eventID, //设计时指定
                    string logMessage,
                    short category,
                    EventLogEntryType logEntryType = EventLogEntryType.Information, //设计时指定
                    byte[] rawData = null
                )
        {
            if (logEntryType <= _enabledMaxEventLogEntryTypeLevel)
            {
                EventLog eventLog = new EventLog();
                eventLog.Source = sourceName;
                logMessage = string
                                .Format
                                    (
                                        "{1}{0}Process [{3}] @ {4}{0}{5}{0}{2}"
                                        , "\r\n"
                                        , "begin ========="
                                        , "end ==========="
                                        , _processNameID
                                        , DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fffff")
                                        , logMessage
                                    );
                EventLogEntryType? eventLogEntryType = null;
                if
                    (
                        TryGetLevelBySourceEventID
                            (
                                sourceName
                                , eventID
                                , out eventLogEntryType
                            )
                    )
                {
                    logEntryType = eventLogEntryType.Value;
                }
                if (rawData == null)
                {
                    eventLog
                        .WriteEntry
                            (
                                logMessage
                                , logEntryType
                                , eventID
                                , category
                            );
                }
                else
                {
                    eventLog
                        .WriteEntry
                            (
                                logMessage
                                , logEntryType
                                , eventID
                                , category
                                , rawData
                            );
                }
            }
        }
        private class KeyValueEntry
        {
            public string Key;
            public EventLogEntryType Value;
        }

        private static EventLogEntryType _enabledMaxEventLogEntryTypeLevel = EventLogEntryType.Error;

        public static EventLogEntryType EnabledMaxEventLogEntryTypeLevel
        {
            get
            {
                return _enabledMaxEventLogEntryTypeLevel;
            }
            set
            {
                _enabledMaxEventLogEntryTypeLevel = value;
            }

        }



        private static ConcurrentDictionary<string, EventLogEntryType> _sourceEventIDLevels
                        = null;

        private static ConcurrentDictionary<string, EventLogEntryType> _sourceEventIDKeyWordsLevels
                        = null;

        public static void RegisterGetLevelsBySourceEventID
                                (
                                    string addressUrl
                                    , int timerIntervalInSeconds = -1
                                )
        {
            if (timerIntervalInSeconds > 0)
            {
                var easyTimer = new EasyTimer
                                    (
                                        timerIntervalInSeconds
                                        , 3
                                        , (x) =>
                                        {
                                            GetSourceEventIDLevels(addressUrl);
                                        }
                                        , true
                                        , false
                                    );
            }
        }

        public static void RegisterGetLevelsBySourceEventIDKeyWords
                        (
                            string addressUrl
                            , int timerIntervalInSeconds = -1
                        )
        {
            if (timerIntervalInSeconds > 0)
            {
                var easyTimer = new EasyTimer
                                    (
                                        timerIntervalInSeconds
                                        , 3
                                        , (x) =>
                                        {
                                            GetSourceEventIDKeyWordsLevels(addressUrl);
                                        }
                                        , true
                                        , false
                                    );
            }
        }


        private static void GetSourceEventIDLevels
                                (
                                    string addressUrl
                                )
        {
            ConcurrentDictionary<string, EventLogEntryType> dictionary = null;
            if (!IsNullOrEmptyOrWhiteSpace(addressUrl))
            {
                try
                {
                    HttpClient httpClient = new HttpClient();
                    var json = httpClient.GetStringAsync(addressUrl).Result;
                    var a = JsonConvert.DeserializeObject<KeyValueEntry[]>(json);
                    dictionary = new ConcurrentDictionary<string, EventLogEntryType>();
                    foreach (var x in a)
                    {
                        dictionary
                            .AddOrUpdate
                                (
                                    x.Key.Trim().ToLower()
                                    , x.Value
                                    , (xx, yy) =>
                                    {
                                        return x.Value;
                                    }
                                );
                    }
                    _sourceEventIDLevels = dictionary;
                }
                catch
                {

                }
            }
        }


        private static void GetSourceEventIDKeyWordsLevels
                        (
                            string addressUrl
                        )
        {
            ConcurrentDictionary<string, EventLogEntryType> dictionary = null;
            if (!IsNullOrEmptyOrWhiteSpace(addressUrl))
            {
                try
                {
                    HttpClient httpClient = new HttpClient();
                    var json = httpClient.GetStringAsync(addressUrl).Result;
                    var a = JsonConvert.DeserializeObject<KeyValueEntry[]>(json);
                    dictionary = new ConcurrentDictionary<string, EventLogEntryType>();
                    foreach (var x in a)
                    {
                        dictionary
                            .AddOrUpdate
                                (
                                    x.Key.Trim().ToLower()
                                    , x.Value
                                    , (xx, yy) =>
                                    {
                                        return x.Value;
                                    }
                                );
                    }
                    _sourceEventIDKeyWordsLevels = dictionary;
                }
                catch
                {

                }
            }
        }

        private static bool TryGetLevelBySourceEventID
                                    (
                                        string source
                                        , int eventID
                                        , out EventLogEntryType? eventLogEntryType
                                    )
        {
            var r = false;
            eventLogEntryType = null;
            EventLogEntryType eventLogEntryTypeValue;
            var dictionary = _sourceEventIDLevels;
            if (dictionary != null)
            {

                if
                    (
                        dictionary
                            .TryGetValue
                                (
                                    string.Format("[{1}]-[{2}]", source, eventID).Trim().ToLower()
                                    , out eventLogEntryTypeValue
                                )
                    )
                {
                    eventLogEntryType = eventLogEntryTypeValue;
                    r = true;
                }
            }
            return r;
        }
        private static bool TryGetLevelBySourceEventIDKeyWords
                                    (
                                        string source
                                        , int eventID
                                        , IEnumerable<string> keyWords
                                        , out EventLogEntryType? eventLogEntryType
                                    )
        {
            var r = false;
            eventLogEntryType = null;
            var dictionary = _sourceEventIDLevels;
            if (dictionary != null)
            {
                var s = string.Empty;
                var keyWordsSequence
                            = keyWords
                                    .OrderBy(x => x)
                                    .Select
                                        (
                                            (x) =>
                                            {
                                                s += "-[" + x + "]";
                                                return
                                                    string
                                                        .Format
                                                            (
                                                                "[{0}]-[{1}]{2}"
                                                                , source
                                                                , eventID
                                                                , s
                                                            );
                                            }
                                        )
                                    .OrderByDescending
                                        (
                                            (x) =>
                                            {
                                                return x.Length;
                                            }
                                        );
                EventLogEntryType eventLogEntryTypeValue;
                foreach (var x in keyWordsSequence)
                {
                    if
                        (
                            _sourceEventIDKeyWordsLevels
                                .TryGetValue
                                    (
                                        x.ToLower()
                                        , out eventLogEntryTypeValue
                                    )
                        )
                    {
                        eventLogEntryType = eventLogEntryTypeValue;
                        r = true;
                        break;
                    }
                }
            }
            return r;
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
        public static string QueryExportLogAndMessages
                                     (
                                            string path
                                            , string query
                                            , string exportFile
                                            , string machine = "."
                                            , PathType pathType = PathType.LogName
                                            , string domain = "."
                                            , string user = null
                                            , string password = null
                                            , SessionAuthentication logOnType
                                                        = SessionAuthentication.Kerberos
                                    )
        {
            var r = string.Empty;
            EventLogSession eventLogSession = null;
            if
                (
                    TryGetEventLogSession
                        (
                            machine
                            , domain
                            , user
                            , password
                            , out eventLogSession
                            , logOnType
                        )
                )
            {
                //Console.WriteLine(exportFile);


                eventLogSession
                        .ExportLogAndMessages
                            (
                                path
                                , pathType
                                , query
                                , exportFile
                                , false
                                , CultureInfo.CurrentCulture
                            );



            }
            r = exportFile;
            return r;
        }

        private static bool TryGetEventLogSession
                                (
                                    string machine
                                    , string domain
                                    , string user
                                    , string password
                                    , out EventLogSession eventLogSession
                                    , SessionAuthentication logOnType
                                )
        {
            var r = false;
            if (machine != ".")
            {
                if
                    (
                        string.IsNullOrEmpty(password)
                        ||
                        string.IsNullOrWhiteSpace(password)
                        ||
                        string.IsNullOrEmpty(user)
                        ||
                        string.IsNullOrWhiteSpace(user)
                    )
                {
                    eventLogSession = new EventLogSession
                                            (
                                                machine

                                            );
                }
                else
                {
                    var securePassword
                            = password
                                    .ToCharArray()
                                    .Aggregate//<char, SecureString>
                                        (
                                            new SecureString()
                                            , (x, y) =>
                                            {
                                                x.AppendChar(y);
                                                return x;
                                            }
                                        );
                    eventLogSession = new EventLogSession
                                            (
                                                machine
                                                , domain
                                                , user
                                                , securePassword
                                                , logOnType
                                            );
                }
            }
            else
            {
                eventLogSession = new EventLogSession();
            }
            r = true;
            return r;
        }

        public static void Query<TEventLogRecordWrapper>
                    (
                        string path
                        , Func<EventLogRecord, TEventLogRecordWrapper> onFactoryProcessFunc
                        , string queryString = null
                        , Func<int, List<TEventLogRecordWrapper>, bool>
                                onPagedProcessFunc = null
                        , int pageSize = 100
                        , string machine = "."
                        , PathType pathType = PathType.LogName
                        , string domain = "."
                        , string user = null
                        , string password = null
                        , SessionAuthentication logOnType
                                            = SessionAuthentication.Default
                    )
        {
            EventLogQuery query = null;
            if (!IsNullOrEmptyOrWhiteSpace(queryString))
            {
                query = new EventLogQuery(path, pathType, queryString);
            }
            else
            {
                query = new EventLogQuery(path, pathType);
            }
            EventLogSession eventLogSession = null;
            try
            {
                if
                    (
                        TryGetEventLogSession
                            (
                                machine
                                , domain
                                , user
                                , password
                                , out eventLogSession
                                , logOnType
                            )
                    )
                {
                    query.Session = eventLogSession;
                    using (var reader = new EventLogReader(query))
                    {
                        EventRecord eventRecord = null;
                        int i = 1;
                        int page = 1;
                        List<TEventLogRecordWrapper> entries = null;
                        if (pageSize > 0)
                        {
                            entries = new List<TEventLogRecordWrapper>();
                        }
                        while (null != (eventRecord = reader.ReadEvent()))
                        {
                            if (pageSize >= 0)
                            {
                                var eventLogRecord = (EventLogRecord)eventRecord;
                                var entry = onFactoryProcessFunc(eventLogRecord);
                                entries.Add(entry);
                                if (i % pageSize == 0)
                                {
                                    if (onPagedProcessFunc != null)
                                    {
                                        var r = onPagedProcessFunc
                                                    (
                                                        page
                                                        , entries
                                                    );
                                        entries.Clear();
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
                        if (entries.Count > 0)
                        {
                            if (onPagedProcessFunc != null)
                            {
                                var r = onPagedProcessFunc(page, entries);
                                entries.Clear();
                                entries = null;
                            }
                        }
                    }
                }
            }
            finally
            {
                if (eventLogSession != null)
                {
                    eventLogSession.Dispose();
                    eventLogSession = null;
                }
            }
        }

        public static void Query
                            (
                                string path
                                , string queryString = null
                                , Func<int, List<EventLogRecord>, bool>
                                        onPagedProcessFunc = null
                                , int pageSize = 100
                                , string machine = "."
                                , PathType pathType = PathType.LogName
                                , string domain = "."
                                , string user = null
                                , string password = null
                                , SessionAuthentication logOnType = SessionAuthentication.Default
                            )
        {
            Query<EventLogRecord>
                (
                    path
                    , (eventLogRecord) =>
                    {
                        return eventLogRecord;
                    }
                    , queryString
                    , onPagedProcessFunc
                    , pageSize
                    , machine
                    , pathType
                    , domain
                    , user
                    , password
                    , logOnType
                );
        }

        public static bool IsNullOrEmptyOrWhiteSpace(string target)
        {
            return
                (
                    string.IsNullOrEmpty(target)
                    ||
                    string.IsNullOrWhiteSpace(target)
                );
        }
    }
}

#endif