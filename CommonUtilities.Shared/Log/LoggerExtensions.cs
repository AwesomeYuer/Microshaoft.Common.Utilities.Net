#if !NETFRAMEWORK4_X
namespace Microshaoft
{
    using Microsoft.Extensions.Logging;
    using System;
    public static class LoggerExtensions
    {
        public static void LogOnDemand
                        (
                            this ILogger target
                            , LogLevel logLevel
                            , Func
                                <
                                    (
                                        EventId LoggingEventId
                                        , Exception LoggingException
                                        , string LoggingMessage
                                        , object[] LoggingArguments
                                    )
                                > loggingPreprocess
                        )
        {
            if (target.IsEnabled(logLevel))
            {
                var r = loggingPreprocess();
                target
                    .Log
                        (
                            logLevel
                            , r.LoggingEventId
                            , r.LoggingException
                            , r.LoggingMessage
                            , r.LoggingArguments
                        );
            }
        }
        public static void LogOnDemand
                        (
                            this ILogger target
                            , LogLevel logLevel
                            , Func
                                <
                                    (
                                        EventId LoggingEventId
                                        , string LoggingMessage
                                        , object[] LoggingArguments
                                    )
                                > loggingPreprocess
                        )
        {
            if (target.IsEnabled(logLevel))
            {
                var r = loggingPreprocess();
                target
                    .Log
                        (
                            logLevel
                            , r.LoggingEventId
                            //, r.LoggingException
                            , r.LoggingMessage
                            , r.LoggingArguments
                        );
            }
        }
        public static void LogOnDemand
                        (
                            this ILogger target
                            , LogLevel logLevel
                            , Func
                                <
                                    (
                                        string LoggingMessage
                                        , object[] LoggingArguments
                                    )
                                > loggingPreprocess
                        )
        {
            if (target.IsEnabled(logLevel))
            {
                var r = loggingPreprocess();
                target
                    .Log
                        (
                            logLevel
                            , r.LoggingMessage
                            , r.LoggingArguments
                        );
            }
        }

        public static void LogOnDemand
                (
                    this ILogger target
                    , LogLevel logLevel
                    , Func
                        <
                            (
                                Exception LoggingException
                                , string LoggingMessage
                                , object[] LoggingArguments
                            )
                        > loggingPreprocess
                )
        {
            if (target.IsEnabled(logLevel))
            {
                var r = loggingPreprocess();
                target
                    .Log
                        (
                            logLevel
                            , r.LoggingException
                            , r.LoggingMessage
                            , r.LoggingArguments
                        );
            }
        }

        public static void LogOnDemand<TState>
                (
                    this ILogger target
                    , LogLevel logLevel
                    , Func
                        <
                            (
                                EventId LoggingEventId
                                , Exception LoggingException
                                , TState State
                            )
                        > loggingPreprocess
                    , Func<TState, Exception, string> formatter
                )
        {
            if (target.IsEnabled(logLevel))
            {
                var r = loggingPreprocess();
                target
                    .Log<TState>
                        (
                            logLevel
                            , r.LoggingEventId
                            , r.State
                            , r.LoggingException
                            , formatter
                        );
            }
        }
    }
}
#endif