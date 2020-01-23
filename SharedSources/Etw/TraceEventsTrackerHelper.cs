
// OopConsoleTraceEventListenerMonitor_TraceControllerEventsConsumer.cs
/*
	# Microshaoft
	/r:System.Xaml.dll
	/r:System.Activities.dll
	/r:System.Activities.DurableInstancing.dll
	/r:System.Runtime.DurableInstancing.dll
	/r:"D:\Microshaoft.Nuget.Packages\Newtonsoft.Json.7.0.1\lib\net45\Newtonsoft.Json.dll"
	/r:"D:\Microshaoft.Nuget.Packages\Microsoft.Diagnostics.Tracing.EventSource.Redist.1.1.28\lib\net46\Microsoft.Diagnostics.Tracing.EventSource.dll"
	/r:"D:\Microshaoft.Nuget.Packages\Microsoft.Diagnostics.Tracing.TraceEvent.1.0.40\lib\net40\Microsoft.Diagnostics.Tracing.TraceEvent.dll"
*/
#if NETFRAMEWORK4_X
namespace Test_OopConsoleTraceEventListenerMonitor_TraceControllerEventsConsumer
{
    using Microshaoft;
    using Microsoft.Diagnostics.Tracing;
    using Microsoft.Diagnostics.Tracing.Session;
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    class Program
    {
        static TextWriter Out = Console.Out;
        static void Main(string[] args)
        {
            Console.Title = Process.GetCurrentProcess().ProcessName;
            Action<long, TraceEventDispatcher, TraceEventSession, TraceEvent>
                    action =
                            (id, source, session, data) =>
                            {
                                var s = data
                                            .PayloadNames
                                            .Select
                                                (
                                                    (x) =>
                                                    {
                                                        return
                                                            string
                                                                .Format
                                                                    (
                                                                        "{1}{0}{2}"
                                                                        , " : "
                                                                        , x
                                                                        , data.PayloadByName(x)
                                                                    );
                                                    }
                                                )
                                            .Aggregate
                                                (
                                                    (x, y) =>
                                                    {
                                                        return
                                                            string
                                                                .Format
                                                                    (
                                                                        "{1}{0}{2}"
                                                                        , "\r\n"
                                                                        , x
                                                                        , y
                                                                    );
                                                    }
                                                );
                                s = string
                                        .Format
                                            (
                                                "{1}{0}{3}{0}{2}"
                                                , "\r\n"
                                                , ">>>>>>>>>>"
                                                , "<<<<<<<<<<"
                                                , string
                                                    .Format
                                                        (
                                                            "{2}{0}{3}{1}{4}"
                                                            , " : "
                                                            , "\r\n"
                                                            , nameof(data.EventName)
                                                            , string
                                                                .Format
                                                                    (
                                                                        "{0} hits ({1})"
                                                                        , data.EventName
                                                                        , id
                                                                    )
                                                            , s
                                                        )
                                            );
                                Console.WriteLine(s);
                                if (session != null)
                                {
                                    Console.WriteLine(session.FileName);
                                }
                            };
            var providerName = "Microshaoft-EventLogEventSource-001";
            var sessionName = "zTest";
            var tracedFileName = @"D:\EtwLogs\zTest.etl";
            var tracingFileName = string.Empty;
            //@"D:\EtwLogs\zTest3.etl";
            //+ string
            //	 .Format
            //		 (
            //			 "{1}{0}{2}{0}{3}{0}{4}"
            //			 , "."
            //			 , providerName
            //			 , sessionName
            //			 , DateTimeHelper.GetAlignSecondsDateTime(DateTime.Now, 60).ToString("yyyy-MM-dd.HH")
            //			 , "etl"
            //		 );
            var traceEvents = new string[]
                                    {
                                        "WriteEvent1Log"
										//, "WriteEvent2Log"
									};
            TraceEventsHelper
                .TraceETWTraceEventSourceAsync
                    (
                        providerName
                        , tracedFileName
                        , traceEvents
                        , action
                        , needCountHits: true
                    );
            TraceEventsHelper
                    .RealTimeTraceEventSessionAsync
                        (
                            providerName
                            , sessionName + "1"
                            , tracingFileName
                            , traceEvents
                            , action
                            , traceEventSourceType: TraceEventSourceType.Session
                            , needCountHits: true
                        );
            var tracker = new TraceEventsTracker();
            //var tracingFileName = "";
            tracker
                .RealTimeTraceEventSessionAsync
                    (
                        providerName
                        , sessionName + "2"
                        , tracingFileName
                        , traceEvents
                        ,
                            (
                                long id
                                , TraceEventDispatcher source
                                , TraceEventSession session
                                , TraceEvent data
                            ) =>
                            {
                                action(id, source, session, data);
                            }
                        , needCountHits: true
                    );
            //tracker = new TraceEventsTracker();
            //tracker
            //	.TraceETWTraceEventSourceAsync
            //		(
            //			providerName
            //			, etlFileName
            //			, traceEvents
            //			, true
            //			,
            //				(
            //					long id
            //					, TraceEventDispatcher source
            //					, TraceEventSession session
            //					, TraceEvent data
            //				) =>
            //				{
            //					action(id, source, session, data);
            //				}
            //		);
            Console.ReadLine();
        }
    }
}
namespace Microshaoft
{
    using Microsoft.Diagnostics.Tracing;
    using Microsoft.Diagnostics.Tracing.Session;
    //using Diagnostics.Tracing.Parsers;
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using System.Threading;
    public class TraceEventsTracker
    {
        public Task<bool> RealTimeTraceEventSessionAsync
                    (
                        string providerName
                        , string sessionName
                        , string tracingFileName = null
                        , string[] traceEvents = null
                        , Action
                                <
                                    long
                                    , TraceEventDispatcher
                                    , TraceEventSession
                                    , TraceEvent
                                > onOneEventTracedOnceProcessAction = null
                        , TraceEventProviderOptions traceEventProviderOptions = null
                        , TraceEventSessionOptions traceEventSessionOptions = TraceEventSessionOptions.Create
                        , TraceEventSourceType traceEventSourceType = TraceEventSourceType.MergeAll
                        , TraceEventLevel traceEventLevel = TraceEventLevel.Always
                        , ulong matchKeywords = ulong.MaxValue
                        , bool needCountHits = false
                    )
        {
            return
                Task
                    .Factory
                    .StartNew<bool>
                        (
                            () =>
                            {
                                return
                                    RealTimeTraceEventSession
                                        (
                                            providerName
                                            , sessionName
                                            , tracingFileName
                                            , traceEvents
                                            , onOneEventTracedOnceProcessAction
                                            , traceEventProviderOptions
                                            , traceEventSessionOptions
                                            , traceEventSourceType
                                            , traceEventLevel
                                            , matchKeywords
                                            , needCountHits
                                        );
                            }
                        );
        }
        public bool RealTimeTraceEventSession
            (
                string providerName
                , string sessionName
                , string tracingFileName = null
                , string[] traceEvents = null
                , Action
                        <
                            long
                            , TraceEventDispatcher
                            , TraceEventSession
                            , TraceEvent
                        > onOneEventTracedOnceProcessAction = null
                , TraceEventProviderOptions traceEventProviderOptions = null
                , TraceEventSessionOptions traceEventSessionOptions = TraceEventSessionOptions.Create
                , TraceEventSourceType traceEventSourceType = TraceEventSourceType.MergeAll
                , TraceEventLevel traceEventLevel = TraceEventLevel.Always
                , ulong matchKeywords = ulong.MaxValue
                , bool needCountHits = false)
        {

            var r = false;
            if
                (
                    traceEvents != null
                    &&
                    traceEvents.Length > 0
                    &&
                    onOneEventTracedOnceProcessAction != null
                )
            {
                r = TraceEventsHelper
                            .RealTimeTraceEventSession
                                (
                                    providerName
                                    , sessionName
                                    , tracingFileName
                                    , traceEvents
                                    , onOneEventTracedOnceProcessAction
                                    , traceEventProviderOptions
                                    , traceEventSessionOptions
                                    , traceEventSourceType
                                    , traceEventLevel
                                    , matchKeywords
                                    , needCountHits
                                );
            }
            return r;
        }
        public Task<bool> TraceETWTraceEventSourceAsync
                    (
                        string providerName
                        , string tracedFileName
                        , string[] traceEvents = null
                        , Action
                                <
                                    long
                                    , TraceEventDispatcher
                                    , TraceEventSession
                                    , TraceEvent
                                > onOneEventTracedOnceProcessAction = null
                        , TraceEventProviderOptions traceEventProviderOptions = null
                        , TraceEventSourceType traceEventSourceType = TraceEventSourceType.MergeAll
                        , TraceEventLevel traceEventLevel = TraceEventLevel.Always
                        , ulong matchKeywords = ulong.MaxValue
                        , bool needCountHits = false
                    )
        {
            return
                Task
                    .Factory
                    .StartNew<bool>
                        (
                            () =>
                            {
                                return
                                    TraceETWTraceEventSource
                                    (
                                        providerName
                                        , tracedFileName
                                        , traceEvents
                                        , onOneEventTracedOnceProcessAction
                                        , traceEventProviderOptions
                                        , traceEventSourceType
                                        , traceEventLevel
                                        , matchKeywords
                                        , needCountHits
                                    );
                            }
                        );
        }
        public bool TraceETWTraceEventSource
                    (
                        string providerName
                        , string tracedFileName
                        , string[] traceEvents = null
                        , Action
                                <
                                    long
                                    , TraceEventDispatcher
                                    , TraceEventSession
                                    , TraceEvent
                                > onOneEventTracedOnceProcessAction = null
                        , TraceEventProviderOptions traceEventProviderOptions = null
                        , TraceEventSourceType traceEventSourceType = TraceEventSourceType.MergeAll
                        , TraceEventLevel traceEventLevel = TraceEventLevel.Always
                        , ulong matchKeywords = ulong.MaxValue
                        , bool needCountHits = false
                    )
        {
            var r = false;
            if
                (
                    traceEvents != null
                    &&
                    traceEvents.Length > 0
                    &&
                    onOneEventTracedOnceProcessAction != null
                )
            {
                r = TraceEventsHelper
                            .TraceETWTraceEventSource
                                (
                                    providerName
                                    , tracedFileName
                                    , traceEvents
                                    , onOneEventTracedOnceProcessAction
                                    , traceEventProviderOptions
                                    , traceEventSourceType
                                    , traceEventLevel
                                    , matchKeywords
                                    , needCountHits
                                );
            }
            return r;
        }
    }
    public static class TraceEventsHelper
    {
        private static TextWriter Out = Console.Out;
        public static Task<bool> TraceETWTraceEventSourceAsync
                             (
                                string providerName
                                , string tracedFileName
                                , string[] traceEvents = null
                                , Action
                                        <
                                            long
                                            , TraceEventDispatcher
                                            , TraceEventSession
                                            , TraceEvent
                                        > onOneEventTracedOnceProcessAction = null
                                , TraceEventProviderOptions traceEventProviderOptions = null
                                , TraceEventSourceType traceEventSourceType = TraceEventSourceType.MergeAll
                                , TraceEventLevel traceEventLevel = TraceEventLevel.Always
                                , ulong matchKeywords = ulong.MaxValue
                                , bool needCountHits = false
                            )
        {
            return
                Task
                    .Factory
                    .StartNew<bool>
                        (
                            () =>
                            {
                                return
                                    TraceETWTraceEventSource
                                        (
                                            providerName
                                            , tracedFileName
                                            , traceEvents
                                            , onOneEventTracedOnceProcessAction
                                            , traceEventProviderOptions
                                            , traceEventSourceType
                                            , traceEventLevel
                                            , matchKeywords
                                            , needCountHits
                                        );
                            }
                            ,
                                TaskCreationOptions.LongRunning
                                |
                                TaskCreationOptions.DenyChildAttach
                        );
        }
        public static bool TraceETWTraceEventSource
                            (
                                string providerName
                                , string tracedFileName
                                , string[] traceEvents = null
                                , Action
                                        <
                                            long
                                            , TraceEventDispatcher
                                            , TraceEventSession
                                            , TraceEvent
                                        > onOneEventTracedOnceProcessAction = null
                                , TraceEventProviderOptions traceEventProviderOptions = null
                                , TraceEventSourceType traceEventSourceType = TraceEventSourceType.MergeAll
                                , TraceEventLevel traceEventLevel = TraceEventLevel.Always
                                , ulong matchKeywords = ulong.MaxValue
                                , bool needCountHits = false
                            )
        {
            var r = false;
            if (!(TraceEventSession.IsElevated() ?? false))
            {
                Out.WriteLine("To turn on ETW events you need to be Administrator, please run from an Admin process.");
                return r;
            }
            if
                (
                    traceEvents != null
                    &&
                    traceEvents.Length > 0
                    &&
                    onOneEventTracedOnceProcessAction != null
                )
            {
                using (var source = new ETWTraceEventSource(tracedFileName, traceEventSourceType))
                {
                    //闭包
                    long sequence = 0;
                    RegisterCallbacks
                        (
                            providerName
                            , traceEvents
                            , source
                            , null
                            , (x, y, z) =>
                            {
                                long id = 0;
                                if (needCountHits)
                                {
                                    id = Interlocked.Increment(ref sequence);
                                }
                                onOneEventTracedOnceProcessAction
                                                (
                                                    id
                                                    , x
                                                    , y
                                                    , z
                                                );
                            }
                        );
                    source.Process();   // call the callbacks for each event
                }
            }
            return true;
        }
        public static Task<bool> RealTimeTraceEventSessionAsync
                            (
                                string providerName
                                , string sessionName
                                , string tracingFileName = null
                                , string[] traceEvents = null
                                , Action
                                        <
                                            long
                                            , TraceEventDispatcher
                                            , TraceEventSession
                                            , TraceEvent
                                        > onOneEventTracedOnceProcessAction = null
                                , TraceEventProviderOptions traceEventProviderOptions = null
                                , TraceEventSessionOptions traceEventSessionOptions = TraceEventSessionOptions.Create
                                , TraceEventSourceType traceEventSourceType = TraceEventSourceType.MergeAll
                                , TraceEventLevel traceEventLevel = TraceEventLevel.Always
                                , ulong matchKeywords = ulong.MaxValue
                                , bool needCountHits = false
                            )
        {
            return
                Task
                    .Factory
                    .StartNew<bool>
                        (
                            () =>
                            {
                                return
                                    RealTimeTraceEventSession
                                        (
                                            providerName
                                            , sessionName
                                            , tracingFileName
                                            , traceEvents
                                            , onOneEventTracedOnceProcessAction
                                            , traceEventProviderOptions
                                            , traceEventSessionOptions
                                            , traceEventSourceType
                                            , traceEventLevel
                                            , matchKeywords
                                            , needCountHits
                                        );
                            }
                            ,
                                TaskCreationOptions.LongRunning
                                |
                                TaskCreationOptions.DenyChildAttach
                        );
        }
        public static bool RealTimeTraceEventSession
            (
                string providerName
                , string sessionName
                , string tracingFileName = null
                , string[] traceEvents = null
                , Action
                        <
                            long
                            , TraceEventDispatcher
                            , TraceEventSession
                            , TraceEvent
                        > onOneEventTracedOnceProcessAction = null
                , TraceEventProviderOptions traceEventProviderOptions = null
                , TraceEventSessionOptions traceEventSessionOptions = TraceEventSessionOptions.Create
                , TraceEventSourceType traceEventSourceType = TraceEventSourceType.MergeAll
                , TraceEventLevel traceEventLevel = TraceEventLevel.Always
                , ulong matchKeywords = ulong.MaxValue
                , bool needCountHits = false
            )
        {
            var r = false;
            if (!(TraceEventSession.IsElevated() ?? false))
            {
                Out.WriteLine("To turn on ETW events you need to be Administrator, please run from an Admin process.");
                return r;
            }
            var needTracingFile = !string.IsNullOrEmpty(tracingFileName);
            if
                (
                    traceEvents != null
                    &&
                    traceEvents.Length > 0
                    &&
                    onOneEventTracedOnceProcessAction != null
                )
            {
                using
                    (
                        var session =
                                (
                                    needTracingFile
                                    ?
                                    new TraceEventSession
                                                (
                                                    sessionName
                                                    , tracingFileName
                                                    , traceEventSessionOptions
                                                )
                                    {
                                        StopOnDispose = true
                                    }
                                    :
                                    new TraceEventSession
                                                (
                                                    sessionName
                                                    , traceEventSessionOptions
                                                )
                                    {
                                        StopOnDispose = true
                                    }
                                )
                    )
                {
                    using
                        (
                            var source =
                                        (
                                            needTracingFile
                                            ?
                                            new ETWTraceEventSource(tracingFileName)
                                            :
                                            session.Source
                                        )
                            )
                    {
                        long sequence = 0;
                        RegisterCallbacks
                            (
                                providerName
                                , traceEvents
                                , source
                                , session
                                , (x, y, z) =>
                                {
                                    long id = 0;
                                    if (needCountHits)
                                    {
                                        id = Interlocked.Increment(ref sequence);
                                    }
                                    onOneEventTracedOnceProcessAction
                                                    (
                                                        id
                                                        , x
                                                        , y
                                                        , z
                                                    );
                                }
                            );
                        var restarted = session
                                            .EnableProvider
                                                (
                                                    providerName
                                                    , traceEventLevel
                                                    , matchKeywords
                                                    , traceEventProviderOptions
                                                );
                        source
                            .Process();
                        r = true;
                    }
                }

            }
            return r;
        }
        private static void RegisterCallbacks
                                (
                                    string providerName
                                    , string[] traceEvents
                                    , TraceEventDispatcher source
                                    , TraceEventSession session
                                    , Action
                                            <
                                                TraceEventDispatcher
                                                , TraceEventSession
                                                , TraceEvent
                                            > onOneEventTracedOnceProcessAction
                                )
        {
            int l = traceEvents.Length;
            for (int i = 0; i < l; i++)
            {
                var eventName = traceEvents[i];
                var action = onOneEventTracedOnceProcessAction;
                if (action != null)
                {
                    if (string.Compare(eventName, "*") == 0)
                    {
                        source
                            .Dynamic
                            .All +=
                                    delegate (TraceEvent data)
                                    {
                                        action(source, session, data);
                                    };
                    }
                    else if (string.Compare(eventName, "UnhandledEvents") == 0)
                    {
                        source
                            .UnhandledEvents
                                += delegate (TraceEvent data)
                                {
                                    action(source, session, data);
                                };
                    }
                    else
                    {
                        source
                            .Dynamic
                            .AddCallbackForProviderEvent
                                (
                                    providerName
                                    , eventName
                                    , delegate (TraceEvent data)
                                    {
                                        action(source, session, data);
                                    }
                                );
                    }
                }
            }
        }
    }
}


#endif
