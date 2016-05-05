﻿// The MIT License (MIT)
// 
// Copyright (c) 2015 Microsoft
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

namespace Microsoft.Diagnostics.Tracing.Logging
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Tracing;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Runtime.Serialization;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading;

    using Microsoft.Diagnostics.Tracing.Parsers;
    using Microsoft.Diagnostics.Tracing.Session;

    /// <summary>
    /// Different types of loggers
    /// </summary>
    internal enum LoggerType
    {
        None,
        Console,
        MemoryBuffer,
        TextLogFile,
        ETLFile,
        Network
    }

    public sealed class EventProviderSubscription
    {
        /// <summary>
        /// Keywords to match.
        /// </summary>
        public EventKeywords Keywords = EventKeywords.None;

        /// <summary>
        /// Minimum event level to record.
        /// </summary>
        public EventLevel MinimumLevel = EventLevel.Informational;

        public EventProviderSubscription(EventSource source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            this.Source = source;
        }

        public EventProviderSubscription(Guid providerID)
        {
            if (providerID == Guid.Empty)
            {
                throw new ArgumentException("Must specify valid provider ID", "providerID");
            }

            this.ProviderID = providerID;
        }

        /// <summary>
        /// EventSource to subscribe to. May be null if ProviderID is provided.
        /// </summary>
        public EventSource Source { get; }

        /// <summary>
        /// Guid to subscribe to. May be empty if Source is provided.
        /// </summary>
        public Guid ProviderID { get; }
    }

    /// <summary>
    /// The common interfaces for an event logger which allow callers to subscribe to and unsubscribe from specific
    /// events, add filters, and get the backing filename.
    /// </summary>
    /// <remarks>
    /// Not all operations are supported by all event loggers, any unsupported operations should result in a
    /// NotSupportedException being thrown.
    /// </remarks>
    public interface IEventLogger
    {
        /// <summary>
        /// Full name of the log file for disk-backed loggers
        /// </summary>
        string Filename { get; set; }

        /// <summary>
        /// Subscribe to events from a particular provider.
        /// </summary>
        /// <param name="subscription">Subscription data.</param>
        void SubscribeToEvents(EventProviderSubscription subscription);

        /// <summary>
        /// Subscribe to events from a collection of event providers.
        /// </summary>
        /// <param name="subscriptions">Collection of subscription data.</param>
        void SubscribeToEvents(ICollection<EventProviderSubscription> subscriptions);

        /// <summary>
        /// Subscribe to events from a particular event provider
        /// </summary>
        /// <param name="source">The event provider to subscribe to</param>
        /// <param name="minimumLevel">The minimum level of event severity to receive events for</param>
        void SubscribeToEvents(EventSource source, EventLevel minimumLevel);

        /// <summary>
        /// Subscribe to events from a particular event provider
        /// </summary>
        /// <param name="source">The event provider to subscribe to</param>
        /// <param name="minimumLevel">The minimum level of event severity to receive events for</param>
        /// <param name="keywords">Keywords (if any) to match against</param>
        void SubscribeToEvents(EventSource source, EventLevel minimumLevel, EventKeywords keywords);

        /// <summary>
        /// Subscribe to events from a particular event provider by Guid
        /// </summary>
        /// <param name="providerId">The Guid of the event provider</param>
        /// <param name="minimumLevel">The minimum level of event severity to receive events for</param>
        void SubscribeToEvents(Guid providerId, EventLevel minimumLevel);

        /// <summary>
        /// Subscribe to events from a particular event provider by Guid
        /// </summary>
        /// <param name="providerId">The Guid of the event provider</param>
        /// <param name="minimumLevel">The minimum level of event severity to receive events for</param>
        /// <param name="keywords">Keywords (if any) to match against</param>
        void SubscribeToEvents(Guid providerId, EventLevel minimumLevel, EventKeywords keywords);

        /// <summary>
        /// Unsubscribe from a specified event provider
        /// </summary>
        /// <param name="source">The source to unsubscribe from</param>
        void UnsubscribeFromEvents(EventSource source);

        /// <summary>
        /// Unsubscribe from a specified event provider by Guid
        /// </summary>
        /// <param name="providerId"></param>
        void UnsubscribeFromEvents(Guid providerId);

        /// <summary>
        /// Add a regular expression filter for formatted output messages
        /// </summary>
        /// <param name="pattern">The pattern to be added</param>
        void AddRegexFilter(string pattern);
    }

    #region Formatters
    [Flags]
    public enum TextLogFormatOptions
    {
        None = 0,

        /// <summary>
        /// Controls whether the thread's Activity ID is shown.
        /// </summary>
        ShowActivityID = 0x1,

        /// <summary>
        /// Emit a full timestamp.
        /// </summary>
        Timestamp = 0x2,

        /// <summary>
        /// Produces an offset from the start of logging instead of a normal timestamp.
        /// </summary>
        TimeOffset = 0x4,

        /// <summary>
        /// Produces process and thread ID data.
        /// </summary>
        ProcessAndThreadData = 0x8,

        /// <summary>
        /// Writes timestamps using the local time instead of UTC.
        /// </summary>
        TimestampInLocalTime = 0x10,

        /// <summary>
        /// The default settings used by <see cref="EventStringFormatter">EventStringFormatter</see>
        /// </summary>
        Default = ShowActivityID | Timestamp | ProcessAndThreadData | TimestampInLocalTime
    }

    /// <summary>
    /// Interface for formatters of events
    /// </summary>
    public interface IEventFormatter
    {
        TextLogFormatOptions Options { get; set; }
        string Format(ETWEvent ev);
    }

    public sealed class EventStringFormatter : IEventFormatter
    {
        /// <summary>
        /// The format to use for timestamps in formatted messages.
        /// </summary>
        /// <remarks>
        /// The current value yeilds ISO 8601-compatible timestamps which sort well.
        /// This could become a configuration knob in the future.
        /// </remarks>
        public const string TimeFormat = "yyyy-MM-ddTHH:mm:ss.ffffff";

        private readonly StringBuilder builder = new StringBuilder(2048);
        private readonly long startTicks;

        public EventStringFormatter()
        {
            this.startTicks = Stopwatch.GetTimestamp();
            this.Options = TextLogFormatOptions.Default;
        }

        /// <summary>
        /// Options to apply when formatting the text
        /// </summary>
        public TextLogFormatOptions Options { get; set; }

        public string Format(ETWEvent ev)
        {
            this.builder.Clear();

            if ((int)(this.Options & TextLogFormatOptions.TimeOffset) != 0)
            {
                var timeDiff = (double)(Stopwatch.GetTimestamp() - this.startTicks);
                timeDiff /= Stopwatch.Frequency;
                this.builder.Append(timeDiff.ToString("F6"));
                this.builder.Append(' ');
            }
            else if ((int)(this.Options & TextLogFormatOptions.Timestamp) != 0)
            {
                if ((int)(this.Options & TextLogFormatOptions.TimestampInLocalTime) != 0)
                {
                    this.builder.Append(ev.Timestamp.ToString(TimeFormat));
                }
                else
                {
                    this.builder.Append(ev.Timestamp.ToUniversalTime().ToString(TimeFormat));
                }
                this.builder.Append(' ');
            }

            if ((int)(this.Options & TextLogFormatOptions.ShowActivityID) != 0 && ev.ActivityID != Guid.Empty)
            {
                this.builder.Append('(');
                this.builder.Append(ev.ActivityID.ToString("N"));
                this.builder.Append(") ");
            }

            this.builder.Append('[');
            if ((int)(this.Options & TextLogFormatOptions.ProcessAndThreadData) != 0)
            {
                this.builder.Append(ev.ProcessID);
                this.builder.Append('/');
                this.builder.Append(ev.ThreadID);
                this.builder.Append('/');
            }
            this.builder.Append(ETWEvent.EventLevelToChar(ev.Level));
            this.builder.Append(':');
            this.builder.Append(ev.ProviderName);
            this.builder.Append(' ');
            this.builder.Append(ev.EventName);
            this.builder.Append(']');

            if (ev.Parameters != null)
            {
                foreach (DictionaryEntry pair in ev.Parameters)
                {
                    var name = pair.Key as string;
                    object o = pair.Value;

                    var s = o as string;
                    var a = o as Array;
                    if (s != null)
                    {
                        // strings can't be trusted, welcome to costlytown.
                        this.builder.Append(' ');
                        this.builder.Append(name);
                        this.builder.Append(@"=""");
                        foreach (var ch in s)
                        {
                            switch (ch)
                            {
                            case '\0':
                                this.builder.Append(@"\0");
                                break;
                            case '\\':
                                this.builder.Append(@"\\");
                                break;
                            case '"':
                                this.builder.Append(@"\""");
                                break;
                            case '\n':
                                this.builder.Append(@"\n");
                                break;
                            case '\r':
                                this.builder.Append(@"\r");
                                break;
                            default:
                                this.builder.Append(ch);
                                break;
                            }
                        }
                        this.builder.Append('\"');
                    }
                    else if (a != null)
                    {
                        // This behavior may be too basic and could be changed in the future. For example
                        // it may be interesting to emit the raw contents of small arrays instead of the type.
                        // This was not done at present because nobody needed it, but anybody should feel free
                        // to make such a change if it is desirable.
                        this.builder.Append(' ');
                        this.builder.Append(name);
                        this.builder.Append('=');
                        this.builder.Append(a.GetType());
                        this.builder.Append('[');
                        this.builder.Append(a.Length);
                        this.builder.Append(']');
                    }
                    else
                    {
                        this.builder.Append(' ');
                        this.builder.Append(name);
                        this.builder.Append('=');
                        this.builder.Append(o);
                    }
                }
            }

            return this.builder.ToString();
        }
    }
    #endregion Formatters

    #region Loggers
    /// <summary>
    /// Base class which handles common work to safely dispatch individual events for all EventListener-based classes.
    /// </summary>
    public abstract class EventListenerDispatcher : EventListener, IEventLogger
    {
        #region Private
        private volatile bool disabled;
        #endregion

        #region Public
        /// <summary>
        /// Whether this logger has been disabled (and should stop posting/writing new data)
        /// </summary>
        public bool Disabled
        {
            get { return this.disabled; }
            set
            {
                lock (this.WriterLock)
                {
                    this.disabled = value;
                }
            }
        }

        /// <summary>
        /// Specific activity ID to filter for. Any lines without this ID will be dropped. Set to
        /// <see cref="Guid.Empty">Guid.Empty</see> to disable activity ID filtering.
        /// </summary>
        public Guid FilterActivityID { get; set; }
        #endregion

        #region Protected
        protected EventListenerDispatcher()
        {
            this.Filters = new List<Regex>();
        }

        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields",
            Justification = "Making this property adds no value and does not improve the code quality")]
        protected readonly object WriterLock = new object();

        /// <summary>
        /// Regular expression filters for output.
        /// </summary>
        protected List<Regex> Filters { get; }
        #endregion

        #region EventListener
        /// <summary>
        /// Constructs an <see cref="ETWEvent"/> object from eventData and calls the overloadable Write method
        /// with the data.
        /// </summary>
        /// <param name="eventData">Event data passed up from the Event Source</param>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0",
            Justification =
                "eventData being null would be a catastrophic contract break by EventSource. This is not anticipated.")]
        protected override sealed void OnEventWritten(EventWrittenEventArgs eventData)
        {
            if (eventData.EventId == (int)DynamicTraceEventParser.ManifestEventID)
            {
                return; // we do not want to write this as it's not really helpful for users
            }

            Guid currentActivityId;
            LogManager.GetActivityId(out currentActivityId);
            if (this.FilterActivityID != Guid.Empty && currentActivityId != this.FilterActivityID)
            {
                return;
            }

            LogManager.EventSourceInfo source = LogManager.GetEventSourceInfo(eventData.EventSource);
            LogManager.EventInfo eventInfo = source[eventData.EventId];
            OrderedDictionary payload = null;
            if (eventInfo.Arguments != null)
            {
                payload = new OrderedDictionary(eventInfo.Arguments.Length);
                int argCount = 0;
                foreach (var o in eventData.Payload)
                {
                    payload.Add(eventInfo.Arguments[argCount], o);
                    ++argCount;
                }
            }

            var ev = new ETWEvent(DateTime.Now, eventData.EventSource.Guid, eventData.EventSource.Name,
                                  (ushort)eventData.EventId, eventInfo.Name, eventData.Version, eventData.Keywords,
                                  eventData.Level, eventData.Opcode, currentActivityId, LogManager.ProcessID,
                                  NativeMethods.GetCurrentWin32ThreadId(),
                                  payload);

            lock (this.WriterLock)
            {
                if (!this.disabled)
                {
                    this.Write(ev);
                }
            }
        }

        /// <summary>
        /// Called when an ETWEvent has been constructed (via <see cref="OnEventWritten"/>).
        /// </summary>
        /// <param name="ev"></param>
        public abstract void Write(ETWEvent ev);
        #endregion

        #region IEventLogger
        public void SubscribeToEvents(EventProviderSubscription subscription)
        {
            if (subscription == null)
            {
                throw new ArgumentNullException("subscription");
            }

            if (subscription.Source != null)
            {
                this.SubscribeToEvents(subscription.Source, subscription.MinimumLevel, subscription.Keywords);
            }
            else
            {
                throw new NotSupportedException("Subscription to GUIDs is not supported");
            }
        }

        public void SubscribeToEvents(ICollection<EventProviderSubscription> subscriptions)
        {
            if (subscriptions == null)
            {
                throw new ArgumentNullException("subscriptions");
            }

            foreach (var sub in subscriptions)
            {
                this.SubscribeToEvents(sub);
            }
        }

        public void SubscribeToEvents(EventSource source, EventLevel minimumLevel)
        {
            SubscribeToEvents(source, minimumLevel, EventKeywords.None);
        }

        public void SubscribeToEvents(EventSource source, EventLevel minimumLevel, EventKeywords keywords)
        {
            if (source != null)
            {
                this.EnableEvents(source, minimumLevel, keywords);
            }
            else
            {
                throw new ArgumentNullException("source");
            }
        }

        public void SubscribeToEvents(Guid providerId, EventLevel minimumLevel)
        {
            SubscribeToEvents(providerId, minimumLevel, EventKeywords.None);
        }

        public void SubscribeToEvents(Guid providerId, EventLevel minimumLevel, EventKeywords keywords)
        {
            throw new NotSupportedException("Subscription to GUIDs is not supported");
        }

        public void UnsubscribeFromEvents(EventSource source)
        {
            this.DisableEvents(source);
        }

        public void UnsubscribeFromEvents(Guid providerId)
        {
            throw new NotSupportedException("Unsubscription from GUIDs is not supported");
        }

        public void AddRegexFilter(string pattern)
        {
            lock (this.WriterLock)
            {
                this.Filters.Add(new Regex(pattern, RegexOptions.IgnoreCase));
            }
        }

        public virtual string Filename
        {
            get { throw new NotSupportedException("This is not a file-backed logger"); }
            set { throw new NotSupportedException("This is not a file-backed logger"); }
        }
        #endregion

        #region IDisposable
        public override void Dispose()
        {
            lock (this.WriterLock)
            {
                base.Dispose();
                this.Dispose(true);
            }
            GC.SuppressFinalize(this);
        }

        protected abstract void Dispose(bool disposing);
        #endregion
    }

    /// <summary>
    /// Base class which handles common work for all EventListener-based classes.
    /// </summary>
    /// <remarks>
    /// This class provides text serialization for EventSource events and is also responsible for processing new
    /// EventSources as they are seen and extracting some useful data from their manifests. This data assists in
    /// mapping EventId values to more meaningful names.
    /// </remarks>
    public abstract class BaseTextLogger : EventListenerDispatcher
    {
        private readonly EventStringFormatter formatter = new EventStringFormatter();

        #region Protected
        /// <summary>
        /// The TextWriter object used to emit logged data
        /// </summary>
        protected TextWriter Writer { get; set; }
        #endregion

        /// <summary>
        /// Expose formatting options to the consumer.
        /// </summary>
        public TextLogFormatOptions FormatOptions
        {
            get { return this.formatter.Options; }
            set { this.formatter.Options = value; }
        }

        public override sealed void Write(ETWEvent ev)
        {
            if (this.Writer == null)
            {
                return;
            }

            string output = ev.ToString(this.formatter);
            if (this.Filters.Count > 0)
            {
                bool matched = false;
                foreach (var filter in this.Filters)
                {
                    if (filter.IsMatch(output))
                    {
                        matched = true;
                        break;
                    }
                }

                if (!matched)
                {
                    return;
                }
            }

            if (this.Writer != null)
            {
                try
                {
                    this.Writer.WriteLine(output);
                    this.Writer.Flush(); // flush after every line -- performance? meh
                }
                catch (ObjectDisposedException) { } // finalizer may come nuke the TextWriter in some edge cases.
            }
        }

        #region IDisposable
        [SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly",
            Justification = "This code ends up cleaner than copying the Dispose() method to inheritors"),
         SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times",
             Justification =
                 "EventListener does not provide a protected Dispose(bool) method to correctly implement the pattern")]
        public override sealed void Dispose()
        {
            lock (this.WriterLock)
            {
                base.Dispose();
                this.Dispose(true);
                this.Writer = null;
            }
            GC.SuppressFinalize(this);
        }
        #endregion
    }

    internal sealed class ConsoleLogger : BaseTextLogger
    {
        #region Public
        public ConsoleLogger()
        {
            this.Writer = Console.Out;
            InternalLogger.Write.LoggerDestinationOpened(this.GetType().ToString(), LogManager.ConsoleLoggerName);
        }
        #endregion

        #region IDisposable
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.Writer.Dispose();
                InternalLogger.Write.LoggerDestinationClosed(this.GetType().ToString(), LogManager.ConsoleLoggerName);
            }
        }
        #endregion
    }

    /// <summary>
    /// The MemoryLogger stores written events in an in-memory buffer which callers may retrieve and inspect.
    /// </summary>
    /// <remarks>
    /// The data in the memory stream is encoded in UTF8 with no BOM.
    /// </remarks>
    public sealed class MemoryLogger : BaseTextLogger
    {
        #region IDisposable
        protected override void Dispose(bool disposing)
        {
            if (disposing && this.Stream != null)
            {
                this.Writer.Dispose(); // calls Dispose on the owned stream for us.
                this.Stream = null;
                InternalLogger.Write.LoggerDestinationClosed(this.GetType().ToString(), ":memory:");
            }
        }
        #endregion

        #region Public
        /// <summary>
        /// Construct a new in-memory logger.
        /// </summary>
        /// <param name="stream">The memory stream to write into.</param>
        internal MemoryLogger(MemoryStream stream)
        {
            this.Stream = stream;
            this.Writer = new StreamWriter(this.Stream, new UTF8Encoding(false, false));
            InternalLogger.Write.LoggerDestinationOpened(this.GetType().ToString(), ":memory:");
        }

        /// <summary>
        /// Retrieve the attached MemoryStream object being used by the logger.
        /// </summary>
        /// <remarks>
        /// The stream will continue updating unless you also set the <see cref="BaseTextLogger.Disabled">Disabled</see>
        /// property to true.
        /// </remarks>
        public MemoryStream Stream { get; private set; }
        #endregion
    }

    internal sealed class TextFileLogger : BaseTextLogger
    {
        #region Public
        public TextFileLogger(string filename, int bufferSizeMB)
        {
            this.outputBufferSize = bufferSizeMB * 1024 * 1024;
            this.Open(filename);
        }
        #endregion

        #region IEventLogger
        public override string Filename
        {
            get
            {
                lock (this.WriterLock)
                {
                    return this.outputFile.Name;
                }
            }
            set
            {
                lock (this.WriterLock)
                {
                    this.Open(value);
                }
            }
        }
        #endregion

        #region IDisposable
        protected override void Dispose(bool disposing)
        {
            if (disposing && this.outputFile != null)
            {
                this.Close();
            }
        }
        #endregion

        #region Private
        private readonly int outputBufferSize;
        private FileStream outputFile;

        private void Open(string filename)
        {
            if (this.outputFile != null
                && string.Compare(this.outputFile.Name, filename, StringComparison.OrdinalIgnoreCase) == 0)
            {
                return;
            }

            this.Close();
            this.outputFile = new FileStream(filename, FileMode.Append, FileAccess.Write, FileShare.Read,
                                             this.outputBufferSize);
            this.Writer = new StreamWriter(this.outputFile, new UTF8Encoding(false, false));
            InternalLogger.Write.LoggerDestinationOpened(this.GetType().ToString(), filename);
        }

        private void Close()
        {
            if (this.outputFile != null)
            {
                this.Writer.Flush(); // We must flush to get the actual length
                long size = this.outputFile.Length;
                string filename = this.outputFile.Name;

                this.Writer.Dispose();
                this.Writer = null;

                this.outputFile.Close();
                this.outputFile = null;

                if (size == 0)
                {
                    File.Delete(filename);
                    InternalLogger.Write.RemovedEmptyFile(filename);
                }

                InternalLogger.Write.LoggerDestinationClosed(this.GetType().ToString(), filename);
            }
        }
        #endregion
    }

    internal sealed class ETLFileLogger : IEventLogger, IDisposable
    {
        #region IDisposable
        public void Dispose()
        {
            string filename = this.session.FileName;
            this.session.Dispose();
            InternalLogger.Write.LoggerDestinationClosed(this.GetType().ToString(), filename);
        }
        #endregion

        #region Public
        public ETLFileLogger(string sessionName, string filename, int bufferSizeMB)
        {
            string fullSessionName = SessionPrefix + sessionName;
            this.session = null;

            // In the event of catastrophe (abnormal process termination) we may have a "dangling" session. In order
            // to establish a new session we must first close the previous session. These circumstances are expected
            // to be extremely rare and extremely unlikely to occur at any time other than process startup.
            if (TraceEventSession.GetActiveSession(fullSessionName) != null)
            {
                CloseDuplicateTraceSession(fullSessionName);
            }

            this.session = new TraceEventSession(fullSessionName, filename)
                           {
                               BufferSizeMB = bufferSizeMB,
                               BufferQuantumKB = GetIndividualBufferSizeKB(bufferSizeMB),
                               StopOnDispose = true,
                               CaptureStateOnSetFileName = true
                           };

            InternalLogger.Write.LoggerDestinationOpened(this.GetType().ToString(), filename);
        }

        public static void CloseDuplicateTraceSession(string sessionName)
        {
            InternalLogger.Write.ConflictingTraceSessionFound(sessionName);

            TraceEventSession s = null;
            try
            {
                // we can't control this session so we need to stop it
                s = new TraceEventSession(sessionName); // might throw if it's in the midst of being shut down
                s.Stop();
            }
            catch (FileNotFoundException)
            {
                // well, okay, then it's probably gone now.
            }
            finally
            {
                if (s != null)
                {
                    s.Dispose();
                }
            }

            // Now we enter a brief waiting period to make sure it dies. We must do this because ControlTrace()
            // (the underlying win32 API) is asynchronous and our request to terminate the session may take
            // a small amount of time to complete.
            if (!WaitForSessionChange(sessionName, false))
            {
                InternalLogger.Write.ConflictingTraceSessionStuck(sessionName);
                throw new OperationCanceledException("could not tear down existing trace session");
            }
        }
        #endregion

        #region Private
        internal const string SessionPrefix = "Microsoft.Diagnostics.Tracing.Logging.";
        private const int MaxRenameTries = 3;
        private const int RenameRetryWaitMS = 500;
        private const int MaxWaitForSessionChange = 5000; // in ms
        private readonly TraceEventSession session;
        private bool hasSubscription;

        // Maps the total requested buffer size to the sizes we'll use for individual ETW buffers. The mentality here
        // is that requests for particularly large overall buffers indicate a need for overall higher throughput, in
        // those cases Windows performs best for both read and write operations if the buffers are larger.
        // Individual buffer sizes are not exposed to the end user because the other types of logs have no analogue,
        // and we can derive the user intent from the overall buffer size they request.
        // Mapping:
        // Below 4MB - 64KB buffers
        // Below 8MB - 128KB buffers
        // Below 16MB - 256KB buffers
        // Below 32MB - 512KB buffers
        // 32MB and up - 1024KB buffers
        private static int GetIndividualBufferSizeKB(int totalBufferSizeMB)
        {
            const int minBufferSizeMB = 2;
            const int maxIndividualBufferSizeKB = 1024;

            totalBufferSizeMB = Math.Max(minBufferSizeMB, totalBufferSizeMB);

            return Math.Min(maxIndividualBufferSizeKB, (totalBufferSizeMB >> 1) * 64);
        }

        private static TraceEventLevel EventLevelToTraceEventLevel(EventLevel level)
        {
            switch (level)
            {
            case EventLevel.Critical:
                return TraceEventLevel.Critical;
            case EventLevel.Error:
                return TraceEventLevel.Error;
            case EventLevel.Informational:
                return TraceEventLevel.Informational;
            case EventLevel.LogAlways:
                return TraceEventLevel.Always;
            case EventLevel.Verbose:
                return TraceEventLevel.Verbose;
            case EventLevel.Warning:
                return TraceEventLevel.Warning;

            default:
                throw new ArgumentException("level had unexpected value", "level");
            }
        }

        /// <summary>
        /// Wait for a pre-determined amount of time for the state of a session to change.
        /// </summary>
        /// <param name="sessionName">Name of the session.</param>
        /// <param name="open">Whether the session should *end* in the open or closed state.</param>
        /// <returns>True if the state changed successfully within the alotted time.</returns>
        private static bool WaitForSessionChange(string sessionName, bool open)
        {
            int slept = 0;
            TraceEventSession session = TraceEventSession.GetActiveSession(sessionName);

            while ((open ? session == null : session != null) && slept < MaxWaitForSessionChange)
            {
                const int sleepFor = MaxWaitForSessionChange / 10;
                Thread.Sleep(sleepFor);
                slept += sleepFor;
                session = TraceEventSession.GetActiveSession(sessionName);
            }

            return (open ? session != null : session == null);
        }
        #endregion

        #region IEventLogger
        public void SubscribeToEvents(EventProviderSubscription subscription)
        {
            if (subscription == null)
            {
                throw new ArgumentNullException("subscription");
            }

            if (subscription.Source != null)
            {
                this.SubscribeToEvents(subscription.Source.Guid, subscription.MinimumLevel, subscription.Keywords);
            }
            else
            {
                this.SubscribeToEvents(subscription.ProviderID, subscription.MinimumLevel, subscription.Keywords);
            }
        }

        public void SubscribeToEvents(ICollection<EventProviderSubscription> subscriptions)
        {
            if (subscriptions == null)
            {
                throw new ArgumentNullException("subscriptions");
            }

            // There is a fun Windows feature where, if you want to use mixed-mode kernel and userland ETW sessions,
            // you MUST subscribe to the kernel first.
            var kernelSub = subscriptions.FirstOrDefault(sub => sub.ProviderID == KernelTraceEventParser.ProviderGuid);
            if (kernelSub != null)
            {
                this.SubscribeToEvents(kernelSub.ProviderID, kernelSub.MinimumLevel, kernelSub.Keywords);
            }
            foreach (var sub in subscriptions)
            {
                if (kernelSub != null && sub == kernelSub)
                {
                    continue;
                }

                this.SubscribeToEvents(sub);
            }
        }

        public void SubscribeToEvents(EventSource source, EventLevel minimumLevel)
        {
            this.SubscribeToEvents(source, minimumLevel, EventKeywords.None);
        }

        public void SubscribeToEvents(EventSource source, EventLevel minimumLevel, EventKeywords keywords)
        {
            this.SubscribeToEvents(source.Guid, minimumLevel, keywords);
        }

        public void SubscribeToEvents(Guid providerId, EventLevel minimumLevel)
        {
            this.SubscribeToEvents(providerId, minimumLevel, EventKeywords.None);
        }

        public void SubscribeToEvents(Guid providerId, EventLevel minimumLevel, EventKeywords keywords)
        {
            try
            {
                if (providerId == KernelTraceEventParser.ProviderGuid)
                {
                    // No support for stack capture flags. Consider adding in future.
                    this.session.EnableKernelProvider((KernelTraceEventParser.Keywords)keywords);
                }
                else
                {
                    this.session.EnableProvider(providerId, EventLevelToTraceEventLevel(minimumLevel), (ulong)keywords);
                }
                if (!this.hasSubscription && !WaitForSessionChange(this.session.SessionName, true))
                {
                    throw new OperationCanceledException("Could not open session in time");
                }
                this.hasSubscription = true;
            }
            catch (Exception e)
            {
                if (!this.hasSubscription)
                {
                    InternalLogger.Write.UnableToOpenTraceSession(this.session.SessionName, e.GetType().ToString(),
                                                                  e.Message);
                }
                throw;
            }
        }

        public void UnsubscribeFromEvents(EventSource source)
        {
            // user needs to restart the trace (aka reconstruct this object)
            throw new NotSupportedException("Unsubscribing is not supported");
        }

        public void UnsubscribeFromEvents(Guid providerId)
        {
            throw new NotSupportedException("Unsubscribing is not supported");
        }

        public void AddRegexFilter(string pattern)
        {
            throw new NotSupportedException("Cannot use regex filters for binary traces");
        }

        public string Filename
        {
            get { return this.session.FileName; }
            set
            {
                if (string.Compare(this.session.FileName, value, StringComparison.OrdinalIgnoreCase) != 0)
                {
                    string oldFilename = this.session.FileName;
                    for (var i = 0; i < MaxRenameTries; ++i)
                    {
                        try
                        {
                            this.session.SetFileName(value);
                            // Write 'close' event after setting the session's filename since that is what triggers actually
                            // closing.
                            InternalLogger.Write.LoggerDestinationClosed(this.GetType().ToString(), oldFilename);
                            InternalLogger.Write.LoggerDestinationOpened(this.GetType().ToString(), value);
                            return;
                        }
                        catch (FileLoadException e)
                        {
                            // This is thrown when the current file is in use and we cannot rename.
                            InternalLogger.Write.UnableToChangeTraceSessionFilename(this.session.SessionName, value,
                                                                                    e.GetType().ToString(), e.Message);
                            Thread.Sleep(RenameRetryWaitMS);
                        }
                        catch (Exception e)
                        {
                            InternalLogger.Write.UnableToChangeTraceSessionFilename(this.session.SessionName, value,
                                                                                    e.GetType().ToString(), e.Message);
                            throw;
                        }
                    }

                    throw new OperationCanceledException(
                        string.Format("Unable to rename file from {0} to {1}", oldFilename, value));
                }
            }
        }
        #endregion
    }

    /// <summary>
    /// Holder for file based loggers and associated metadata
    /// </summary>
    internal sealed class FileBackedLogger : IDisposable
    {
        #region Public
        /// <summary>The default filename template for rotated log files</summary>
        /// <remarks>
        /// This yields a string like "foo_20110623T154000Z--T155000Z"
        /// This bit of goop is intended to be ISO 8601 compliant and sorts very nicely.
        /// </remarks>
        public const string DefaultFilenameTemplate = "{0}_{1:yyyyMMdd}T{1:HHmmss}Z--T{2:HHmmss}Z";

        /// <summary>The default filename template for rotated log files when using local timestamps</summary>
        /// <remarks>
        /// This yields a string like "foo_20110623T154000-08--T155000-08". Note the zone offsets which help deal
        /// with timezone changes. HOWEVER, this template assumes a timezone with ONLY hour-based offsets and this
        /// code would not be suitable for use in areas where timezone offsets cross over into minutes (e.g. Tibet,
        /// India, etc).
        /// </remarks>
        public const string DefaultLocalTimeFilenameTemplate = "{0}_{1:yyyyMMdd}T{1:HHmmsszz}--T{2:HHmmsszz}";

        /// <summary>
        /// The filename extension used for text logs.
        /// </summary>
        public const string TextLogExtension = ".log";

        /// <summary>
        /// The filename extension used for ETW logs.
        /// </summary>
        public const string ETLExtension = ".etl";

        /// <summary>
        /// Constructs a manager for a file-backed logger
        /// </summary>
        /// <param name="baseFilename">Base portion of filename</param>
        /// <param name="directoryName">Directory to store file(s) in</param>
        /// <param name="logType">Type of log output</param>
        /// <param name="bufferSizeMB">Size in kilobytes of underlying buffers used by the logger</param>
        /// <param name="rotationInterval">Time period a single file should be open for</param>
        /// <param name="filenameTemplate">String formatting template for the filename (if it uses rotation)</param>
        /// <param name="timestampLocal">Whether to use local time for the timestamps</param>
        /// <remarks>
        /// Callers are expected to call CheckedRotate() periodically to cause file rotation to occur as desired.
        /// If rotationInterval is set to 0 the file will not be rotated and the filename will not contain timestamps.
        /// </remarks>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope",
            Justification = "We hold the logger internally and so do not want to dispose it")]
        public FileBackedLogger(string baseFilename, string directoryName, LoggerType logType, int bufferSizeMB,
                                int rotationInterval, string filenameTemplate, bool timestampLocal)
        {
            this.directoryName = Path.GetFullPath(directoryName);
            if (!Directory.Exists(this.directoryName))
            {
                Directory.CreateDirectory(this.directoryName); // allowed to throw, caller should handle it
            }

            this.baseFilename = baseFilename;
            this.directoryName = directoryName;
            this.loggerType = logType;
            this.RotationInterval = rotationInterval;
            this.TimestampLocal = timestampLocal;

            DateTime now = this.AdjustUtcTime(DateTime.UtcNow);

            // The rest of the library callers are built to pass in our constant for filename template, ignoring
            // whether they want local timestamps or not. Keep the logic here. We do ReferenceEquals because we
            // expect external callers who are passing in their own templates to put in the right format if they
            // want local time.
            if (timestampLocal && filenameTemplate == DefaultFilenameTemplate)
            {
                filenameTemplate = DefaultLocalTimeFilenameTemplate;
            }

            if (!IsValidFilenameTemplate(filenameTemplate))
            {
                throw new ArgumentException("invalid template format", "filenameTemplate");
            }

            switch (this.loggerType)
            {
            case LoggerType.TextLogFile:
                this.fileExtension = TextLogExtension;
                this.FilenameTemplate = filenameTemplate + this.fileExtension;
                this.UpdateCurrentFilename(now);
                var textFileLogger = new TextFileLogger(this.currentFilename, bufferSizeMB);
                if (!timestampLocal)
                {
                    textFileLogger.FormatOptions &= ~TextLogFormatOptions.TimestampInLocalTime;
                }
                this.Logger = textFileLogger;
                break;
            case LoggerType.ETLFile:
                this.fileExtension = ETLExtension;
                this.FilenameTemplate = filenameTemplate + this.fileExtension;
                this.UpdateCurrentFilename(now);
                this.Logger = new ETLFileLogger(this.baseFilename, this.currentFilename, bufferSizeMB);
                break;
            default:
                throw new ArgumentException("log type " + logType + " not implemented", "logType");
            }

            InternalLogger.Write.CreateFileDestination(this.baseFilename, this.directoryName, this.RotationInterval,
                                                       filenameTemplate);
        }

        // we don't want users to be able to tweak file/dir names, only filtering.
        public IEventLogger Logger { get; private set; }

        public int RotationInterval { get; }

        /// <summary>
        /// Whether we will defer to local time when setting a filename during rotation
        /// </summary>
        public bool TimestampLocal { get; }

        /// <summary>
        /// The template in use for filename rotation.
        /// </summary>
        public string FilenameTemplate { get; }

        /// <summary>
        /// Check to see whether a file rotation is due and rotate the file if necessary.
        /// </summary>
        /// <param name="now">The current UTC time (<see cref="DateTime.UtcNow"/>)</param>
        public void CheckedRotate(DateTime now)
        {
            now = this.AdjustUtcTime(now);

            if (this.RotationInterval > 0 && this.intervalEnd.Ticks <= now.Ticks)
            {
                this.Rotate(now);
            }
        }

        /// <summary>
        /// Immediately rotate/rename the file with the provided timestamp.
        /// </summary>
        /// <param name="now">The current UTC time (<see cref="DateTime.UtcNow"/>)</param>
        public void Rotate(DateTime now)
        {
            now = this.AdjustUtcTime(now);

            this.UpdateCurrentFilename(now);
            this.Logger.Filename = this.currentFilename;
        }

        /// <summary>
        /// Ensure a filename template formats correctly and generates a valid filename
        /// </summary>
        /// <param name="template">The template to validate</param>
        /// <returns>true if the template is valid, false otherwise</returns>
        public static bool IsValidFilenameTemplate(string template)
        {
            const string baseName = "history";
            var fourScoreEtc = new DateTime(1776, 7, 4);
            var gAddress = new DateTime(1863, 11, 19);
            const string someMachine = "ORTHANC";
            const long jenny = 8675309;

            try
            {
                if (!template.Contains("{0}"))
                {
                    return false; // base filename MUST be represented without any goop in the formatting.
                }

                string generatedName = string.Format(template, baseName, fourScoreEtc, gAddress, someMachine, jenny);
                if (generatedName.IndexOfAny(Path.GetInvalidFileNameChars()) != -1)
                {
                    return false;
                }
            }
            catch (FormatException)
            {
                return false;
            }
            return true;
        }
        #endregion

        #region Private
        private readonly string baseFilename;
        private readonly string directoryName;
        private readonly string fileExtension;
        private readonly LoggerType loggerType;

        private string currentFilename;
        private DateTime intervalEnd;
        private DateTime intervalStart;

        /// <summary>
        /// Adjust the given 'now' value from UTC to local time if we need that.
        /// </summary>
        /// <param name="utcNow">The current time (from <see cref="DateTime.UtcNow"/>)</param>
        /// <returns>The adjusted time value.</returns>
        private DateTime AdjustUtcTime(DateTime utcNow)
        {
            return this.TimestampLocal ? utcNow.ToLocalTime() : utcNow;
        }

        /// <summary>
        /// Conditionally update the current filename
        /// </summary>
        /// <param name="now">The current time</param>
        /// <returns>true if the filename required updating, false otherwise</returns>
        private void UpdateCurrentFilename(DateTime now)
        {
            string newFilename;
            if (this.RotationInterval <= 0)
            {
                this.intervalStart = this.intervalEnd = new DateTime(0);
                newFilename = this.baseFilename + this.fileExtension;
            }
            else
            {
                // calculate start / end times which we will use for the filename
                long startTicks = now.Ticks - (now.Ticks % (this.RotationInterval * TimeSpan.TicksPerSecond));
                long endTicks = startTicks + (this.RotationInterval * TimeSpan.TicksPerSecond);

                this.intervalStart = new DateTime(now.Ticks);
                this.intervalEnd = new DateTime(endTicks);
                newFilename = string.Format(this.FilenameTemplate, this.baseFilename, this.intervalStart,
                                            this.intervalEnd, Environment.MachineName, MillisecondsSinceMidnight(now));
            }

            string newFileName = Path.Combine(this.directoryName, newFilename);
            this.currentFilename = newFileName;
            InternalLogger.Write.UpdateFileRotationTimes(this.baseFilename, this.intervalStart.Ticks,
                                                         this.intervalEnd.Ticks);
        }

        /// <summary>
        /// Turn a timestamp into a "sequence number"-esque value for granular indication of log starts
        /// </summary>
        /// <param name="now">The current time</param>
        /// <returns>The number of milliseconds since midnight</returns>
        /// <remarks>
        /// This partner-specific hack should NOT exist. All effort should be made to get this out of the code.
        /// </remarks>
        private static long MillisecondsSinceMidnight(DateTime now)
        {
            long sequence = ((now.Hour * 60) + now.Minute) * 60000;
            sequence += (now.Second * 1000) + now.Millisecond;
            return sequence;
        }
        #endregion

        #region IDisposable
        public void Dispose()
        {
            this.Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            if (disposing && this.Logger != null)
            {
                (this.Logger as IDisposable).Dispose();
                this.Logger = null;
            }
        }
        #endregion
    }

    /// <summary>
    /// The NetworkLogger sends the events to a network endpoint which watchers may retrieve and inspect.
    /// </summary>
    public sealed class NetworkLogger : EventListenerDispatcher
    {
        #region Public
        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="hostname">Hostname without scheme</param>
        /// <param name="port">Port</param>
        public NetworkLogger(string hostname, int port)
        {
            if (Uri.CheckHostName(hostname) == UriHostNameType.Unknown)
            {
                throw new ArgumentException("hostname");
            }

            if (port <= 0 || port > 65535)
            {
                throw new ArgumentException("port");
            }

            this.serverUri = new Uri(string.Format("http://{0}:{1}", hostname, port));
            InternalLogger.Write.LoggerDestinationOpened(this.GetType().ToString(), this.serverUri.ToString());
        }
        #endregion

        #region IDisposable
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                InternalLogger.Write.LoggerDestinationClosed(this.GetType().ToString(), this.serverUri.ToString());
            }
        }
        #endregion

        #region Private
        private readonly Uri serverUri;
        private readonly DataContractSerializer serializer = new DataContractSerializer(typeof(ETWEvent));
        private readonly MemoryStream serializationBuffer = new MemoryStream();
        #endregion

        #region EventListenerDispatcher
        public override void Write(ETWEvent ev)
        {
            try
            {
                WebRequest request = WebRequest.Create(this.serverUri);
                request.Method = WebRequestMethods.Http.Post;
                request.ContentType = "text/xml";

                // We are guaranteed safe access to our serializer and its buffer, but we have to dupe the bytes off
                // before using async web request. It's possible we could be smarter about this with a distinct
                // serializer per request and use of its begin/end write code. Not trying for now.
                this.serializationBuffer.Position = 0;
                this.serializationBuffer.SetLength(0);
                this.serializer.WriteObject(this.serializationBuffer, ev);

                var postData = new byte[this.serializationBuffer.Length];
                Array.Copy(this.serializationBuffer.GetBuffer(), postData, postData.Length);
                request.ContentLength = postData.Length;

                request.BeginGetRequestStream(GetPostStreamCallback,
                                              new RequestState {WebRequest = request, PostData = postData});
            }
            catch (WebException) { } // Skip all of these since we don't currently care if the remote endpoint is down.
            catch (InvalidOperationException) { }
            catch (IOException) { }
        }

        /// <summary>
        /// Callback of BeginGetRequestStream (for post data). This will start off an async write to it.
        /// </summary>
        private static void GetPostStreamCallback(IAsyncResult ar)
        {
            try
            {
                var requestState = (RequestState)ar.AsyncState;
                WebRequest request = requestState.WebRequest;
                byte[] postData = requestState.PostData;

                using (Stream requestStream = request.EndGetRequestStream(ar))
                {
                    requestStream.Write(postData, 0, postData.Length);
                    requestStream.Close();
                }

                request.BeginGetResponse(GetResponseCallback, request);
            }
            catch (WebException) { }
            catch (InvalidOperationException) { }
            catch (IOException) { }
        }

        /// <summary>
        /// Callback of BeginGetResponse. Does not do anything with response.
        /// </summary>
        private static void GetResponseCallback(IAsyncResult asynchronousResult)
        {
            try
            {
                var request = (WebRequest)asynchronousResult.AsyncState;
                using (var response = (HttpWebResponse)request.EndGetResponse(asynchronousResult))
                {
                    response.Close();
                }
            }
            catch (WebException) { } // Skip all of these since we don't currently care if the remote endpoint is down.
            catch (InvalidOperationException) { }
            catch (IOException) { }
        }

        private class RequestState
        {
            public byte[] PostData;
            public WebRequest WebRequest;
        }
        #endregion
    }
    #endregion Loggers
}