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
                    , Func<string> messageFactory
                )
        {
            var loggingFormatArguments = new object[] { };
            target
                .LogOnDemand
                    (
                        logLevel
                        , messageFactory
                        , loggingFormatArguments
                    );
        }
        public static void LogOnDemand
                                (
                                   this ILogger target
                                    , LogLevel logLevel
                                    , Func<string> messageFactory
                                    , params object[] loggingFormatArguments
                                )
        {
            if (target.IsEnabled(logLevel))
            {
                var message = messageFactory();
                target
                    .Log
                        (
                            logLevel
                            , message
                            , loggingFormatArguments
                        );
            }
        }
        public static void LogOnDemand
                                (
                                   this ILogger target
                                    , LogLevel logLevel
                                    , Exception exception
                                    , Func<string> messageFactory
                                    
                                )
        {
            var loggingFormatArguments = new object[] { };
            target
                .LogOnDemand
                    (
                        logLevel
                        , exception
                        , messageFactory
                        , loggingFormatArguments
                    );
        }
        public static void LogOnDemand
                                (
                                   this ILogger target
                                    , LogLevel logLevel
                                    , Exception exception
                                    , Func<string> messageFactory
                                    , params object[] loggingFormatArguments
                                )
        {
            if (target.IsEnabled(logLevel))
            {
                var message = messageFactory();
                target
                    .Log
                        (
                            logLevel
                            , exception
                            , message
                            , loggingFormatArguments
                        );
            }
        }

        public static void LogOnDemand
                         (
                            this ILogger target
                             , LogLevel logLevel
                             , EventId eventId
                             , Exception exception
                             , Func<string> messageFactory
                         )
        {
            var loggingFormatArguments = new object[] { };
            target
                .LogOnDemand
                    (
                        logLevel
                        , eventId
                        , exception
                        , messageFactory
                        , loggingFormatArguments
                    );
        }
        public static void LogOnDemand
                                (
                                   this ILogger target
                                    , LogLevel logLevel
                                    , EventId eventId
                                    , Exception exception
                                    , Func<string> messageFactory
                                    , params object[] loggingFormatArguments
                                )
        {
            if (target.IsEnabled(logLevel))
            {
                var message = messageFactory();
                target
                    .Log
                        (
                            logLevel
                            , eventId
                            , exception
                            , message
                            , loggingFormatArguments
                        );
            }
        }
        public static void LogOnDemand
                         (
                            this ILogger target
                             , LogLevel logLevel
                             , EventId eventId
                             , Func<string> messageFactory
                         )
        {
            var loggingFormatArguments = new object[] { };
            target
                .LogOnDemand
                    (
                        logLevel
                        , eventId
                        , messageFactory
                        , loggingFormatArguments
                    );
        }
        public static void LogOnDemand
                                (
                                   this ILogger target
                                    , LogLevel logLevel
                                    , EventId eventId
                                    , Func<string> messageFactory
                                    , params object[] loggingFormatArguments
                                )
        {
            if (target.IsEnabled(logLevel))
            {
                var message = messageFactory();
                target
                    .Log
                        (
                            logLevel
                            , eventId
                            , message
                            , loggingFormatArguments
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
                                        EventId loggingEventId
                                        , Exception loggingException
                                        , string loggingMessage
                                        , object[] loggingFormatArguments
                                    )
                                > loggingPreprocess
                        )
        {
            if (target.IsEnabled(logLevel))
            {
                var (loggingEventId, loggingException, loggingMessage, loggingFormatArguments) = loggingPreprocess();
                target
                    .Log
                        (
                            logLevel
                            , loggingEventId
                            , loggingException
                            , loggingMessage
                            , loggingFormatArguments
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
                                        EventId loggingEventId
                                        , string loggingMessage
                                        , object[] loggingFormatArguments
                                    )
                                > loggingPreprocess
                        )
        {
            if (target.IsEnabled(logLevel))
            {
                var (loggingEventId, loggingMessage, loggingFormatArguments) = loggingPreprocess();
                target
                    .Log
                        (
                            logLevel
                            , loggingEventId
                            , loggingMessage
                            , loggingFormatArguments
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
                                        string loggingMessage
                                        , object[] loggingFormatArguments
                                    )
                                > loggingPreprocess
                        )
        {
            if (target.IsEnabled(logLevel))
            {
                var (loggingMessage, loggingFormatArguments) = loggingPreprocess();
                target
                    .Log
                        (
                            logLevel
                            , loggingMessage
                            , loggingFormatArguments
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
                                Exception loggingException
                                , string loggingMessage
                                , object[] loggingFormatArguments
                            )
                        > loggingPreprocess
                )
        {
            if (target.IsEnabled(logLevel))
            {
                var 
                    (
                        loggingException
                        , loggingMessage
                        , loggingFormatArguments
                    ) = loggingPreprocess();
                target
                    .Log
                        (
                            logLevel
                            , loggingException
                            , loggingMessage
                            , loggingFormatArguments
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
                                EventId loggingEventId
                                , Exception loggingException
                                , TState State
                            )
                        > loggingPreprocess
                    , Func<TState, Exception, string> formatter
                )
        {
            if (target.IsEnabled(logLevel))
            {
                var (loggingEventId, loggingException, State) = loggingPreprocess();
                target
                    .Log
                        (
                            logLevel
                            , loggingEventId
                            , State
                            , loggingException
                            , formatter
                        );
            }
        }
    }
}
#endif