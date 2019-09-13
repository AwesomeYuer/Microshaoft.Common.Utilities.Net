namespace Microshaoft.TraceEventSources
{
    using System;
    using System.Diagnostics.Tracing;
    using System.IO;
    using System.Threading;

    using Microsoft.Diagnostics.Tracing.Logging;

    [EventSource(Name = "Demo", Guid = "{5636f2a4-0394-410a-abaf-89080f8542ce}")]
    public sealed class DemoEvents : EventSource
    {
        public static DemoEvents Write = new DemoEvents();

        [Event(1, Level = EventLevel.Informational)]
        public void Log(string message)
        {
            if (this.IsEnabled())
            {
                this.WriteEvent(1, message);
            }
        }

        protected override void OnEventCommand(EventCommandEventArgs command)
        {

            Console.WriteLine("Got Event Command: {0}", command.Command);
            foreach (var kvp in command.Arguments)
            {
                Console.WriteLine("{0}:{1}", kvp.Key, kvp.Value);
            }
        }
    }

    public sealed class Program
    {
        private static void Main(string[] args)
        {
            var dir = Path.GetFullPath(args[0]);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            Console.WriteLine("Writing logs in {0}", dir);

            var config = string
                            .Format
                            (@"
<loggers>
  <etwlogging enabled='true' />
  <log name='demo' rotationInterval='60' type='etl' directory='{0}' timestampLocal='true'>
    <source name='Demo' minimumSeverity='informational' />
  </log>
</loggers>
", dir);

            LogManager
                .Start();
            //LogAssert
            //    .Assert
            //        (
            LogManager
                .SetConfiguration(config);
            //);
            LogManager
                .ConsoleLogger
                .SubscribeToEvents
                    (
                        DemoEvents.Write
                        , EventLevel.Verbose
                    );
            LogManager
                    .ConsoleLogger
                    .SubscribeToEvents
                        (
                            InternalLogger.Write
                            , EventLevel.Verbose
                        );

            var t = new Timer
                            (
                                _ => DemoEvents.Write.Log(DateTime.Now.ToString())
                                , null
                                , TimeSpan.Zero
                                , new TimeSpan(0, 0, 1)
                            );

            Console.CancelKeyPress +=
                (sender, eventArgs) =>
                {
                    Console.WriteLine("Shutting down...");
                    LogManager.Shutdown();
                    Environment.Exit(0);
                };

            while (true)
            {
                Thread.Sleep(100);
            }
        }
    }
}