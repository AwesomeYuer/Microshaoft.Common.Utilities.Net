//namespace Services.LogService.Contracts
//{
//    using System.Collections.Generic;
//    using System.Security;
//    using System.Security.Principal;
//    using System;
//    using System.Linq;
//    using System.Diagnostics.Eventing.Reader;    //
//    // Summary:
//    //     Contains the properties of an event instance for an event that is received from
//    //     an System.Diagnostics.Eventing.Reader.EventLogReader object. The event properties
//    //     provide information about the event such as the name of the computer where the
//    //     event was logged and the time that the event was created.
//    public class EventLogRecordEntry //: EventRecord
//    {

//        public EventLogRecordEntry()
//        {
//        }

//        public static Action<Exception> OnEventLogRecordCaughtException = null;


//        public EventLogRecordEntry(EventLogRecord eventLogRecord)
//        {
//            try { ActivityId = eventLogRecord.ActivityId; } catch (Exception exception) { OnEventLogRecordCaughtException?.Invoke(exception); }
//            try { Bookmark = eventLogRecord.Bookmark != null ? eventLogRecord.Bookmark.ToString() : string.Empty; } catch (Exception exception) { OnEventLogRecordCaughtException?.Invoke(exception); }
//            try { ContainerLog = eventLogRecord.ContainerLog; } catch (Exception exception) { OnEventLogRecordCaughtException?.Invoke(exception); }
//            try { Id = eventLogRecord.Id; } catch (Exception exception) { OnEventLogRecordCaughtException?.Invoke(exception); }
//            try { Keywords = eventLogRecord.Keywords; } catch (Exception exception) { OnEventLogRecordCaughtException?.Invoke(exception); }
//            try { KeywordsDisplayNames = eventLogRecord.KeywordsDisplayNames != null ? eventLogRecord.KeywordsDisplayNames.ToList() : null; } catch (Exception exception) { OnEventLogRecordCaughtException?.Invoke(exception); }
//            try { Level = eventLogRecord.Level; } catch (Exception exception) { OnEventLogRecordCaughtException?.Invoke(exception); }
//            try { LevelDisplayName = eventLogRecord.LevelDisplayName; } catch (Exception exception) { OnEventLogRecordCaughtException?.Invoke(exception); }
//            try { LogName = eventLogRecord.LogName; } catch (Exception exception) { OnEventLogRecordCaughtException?.Invoke(exception); }
//            try { MachineName = eventLogRecord.MachineName; } catch (Exception exception) { OnEventLogRecordCaughtException?.Invoke(exception); }
//            try { MatchedQueryIds = eventLogRecord.MatchedQueryIds != null ? eventLogRecord.MatchedQueryIds.ToList() : null; } catch (Exception exception) { OnEventLogRecordCaughtException?.Invoke(exception); }
//            try { Opcode = eventLogRecord.Opcode; } catch (Exception exception) { OnEventLogRecordCaughtException?.Invoke(exception); }
//            try { OpcodeDisplayName = eventLogRecord.OpcodeDisplayName; } catch (Exception exception) { OnEventLogRecordCaughtException?.Invoke(exception); }
//            try { ProcessId = eventLogRecord.ProcessId; } catch (Exception exception) { OnEventLogRecordCaughtException?.Invoke(exception); }
//            try { ProviderId = eventLogRecord.ProviderId; } catch (Exception exception) { OnEventLogRecordCaughtException?.Invoke(exception); }
//            try { ProviderName = eventLogRecord.ProviderName; } catch (Exception exception) { OnEventLogRecordCaughtException?.Invoke(exception); }
//            try { Qualifiers = eventLogRecord.Qualifiers; } catch (Exception exception) { OnEventLogRecordCaughtException?.Invoke(exception); }
//            try { RecordId = eventLogRecord.RecordId; } catch (Exception exception) { OnEventLogRecordCaughtException?.Invoke(exception); }
//            try { RelatedActivityId = eventLogRecord.RelatedActivityId; } catch (Exception exception) { OnEventLogRecordCaughtException?.Invoke(exception); }
//            try { Task = eventLogRecord.Task; } catch (Exception exception) { OnEventLogRecordCaughtException?.Invoke(exception); }
//            try { TaskDisplayName = eventLogRecord.TaskDisplayName; } catch (Exception exception) { OnEventLogRecordCaughtException?.Invoke(exception); }
//            try { ThreadId = eventLogRecord.ThreadId; } catch (Exception exception) { OnEventLogRecordCaughtException?.Invoke(exception); }
//            try { TimeCreated = eventLogRecord.TimeCreated; } catch (Exception exception) { OnEventLogRecordCaughtException?.Invoke(exception); }
//            try { UserId = (eventLogRecord.UserId != null ? eventLogRecord.UserId.ToString() : string.Empty); } catch (Exception exception) { OnEventLogRecordCaughtException?.Invoke(exception); }
//            try { Version = eventLogRecord.Version; } catch (Exception exception) { OnEventLogRecordCaughtException?.Invoke(exception); }
//            try { FormatDescription = eventLogRecord.FormatDescription(); } catch (Exception exception) { OnEventLogRecordCaughtException?.Invoke(exception); }
//            try { Xml = eventLogRecord.ToXml(); } catch (Exception exception) { OnEventLogRecordCaughtException?.Invoke(exception); }
//        }


//        //
//        // Summary:
//        //     Gets the globally unique identifier (GUID) for the activity in process for which
//        //     the event is involved. This allows consumers to group related activities.
//        //
//        // Returns:
//        //     Returns a GUID value.
//        public Guid? ActivityId { get; }
//        //
//        // Summary:
//        //     Gets a placeholder (bookmark) that corresponds to this event. This can be used
//        //     as a placeholder in a stream of events.
//        //
//        // Returns:
//        //     Returns a System.Diagnostics.Eventing.Reader.EventBookmark object.
//        public string Bookmark { get; }
//        //
//        // Summary:
//        //     Gets the name of the event log or the event log file in which the event is stored.
//        //
//        // Returns:
//        //     Returns a string that contains the name of the event log or the event log file
//        //     in which the event is stored.
//        public string ContainerLog { get; }
//        //
//        // Summary:
//        //     Gets the identifier for this event. All events with this identifier value represent
//        //     the same type of event.
//        //
//        // Returns:
//        //     Returns an integer value. This value can be null.
//        public int Id { get; }
//        //
//        // Summary:
//        //     Gets the keyword mask of the event. Get the value of the System.Diagnostics.Eventing.Reader.EventLogRecord.KeywordsDisplayNames
//        //     property to get the name of the keywords used in this mask.
//        //
//        // Returns:
//        //     Returns a long value. This value can be null.
//        public long? Keywords { get; }
//        //
//        // Summary:
//        //     Gets the display names of the keywords used in the keyword mask for this event.
//        //
//        // Returns:
//        //     Returns an enumerable collection of strings that contain the display names of
//        //     the keywords used in the keyword mask for this event.
//        public List<string> KeywordsDisplayNames { get; }
//        //
//        // Summary:
//        //     Gets the level of the event. The level signifies the severity of the event. For
//        //     the name of the level, get the value of the System.Diagnostics.Eventing.Reader.EventLogRecord.LevelDisplayName
//        //     property.
//        //
//        // Returns:
//        //     Returns a byte value. This value can be null.
//        public int? Level { get; }
//        //
//        // Summary:
//        //     Gets the display name of the level for this event.
//        //
//        // Returns:
//        //     Returns a string that contains the display name of the level for this event.
//        public string LevelDisplayName { get; }
//        //
//        // Summary:
//        //     Gets the name of the event log where this event is logged.
//        //
//        // Returns:
//        //     Returns a string that contains a name of the event log that contains this event.
//        public string LogName { get; }
//        //
//        // Summary:
//        //     Gets the name of the computer on which this event was logged.
//        //
//        // Returns:
//        //     Returns a string that contains the name of the computer on which this event was
//        //     logged.
//        public string MachineName { get; }
//        //
//        // Summary:
//        //     Gets a list of query identifiers that this event matches. This event matches
//        //     a query if the query would return this event.
//        //
//        // Returns:
//        //     Returns an enumerable collection of integer values.
//        public List<int> MatchedQueryIds { get; }
//        //
//        // Summary:
//        //     Gets the opcode of the event. The opcode defines a numeric value that identifies
//        //     the activity or a point within an activity that the application was performing
//        //     when it raised the event. For the name of the opcode, get the value of the System.Diagnostics.Eventing.Reader.EventLogRecord.OpcodeDisplayName
//        //     property.
//        //
//        // Returns:
//        //     Returns a short value. This value can be null.
//        public short? Opcode { get; }
//        //
//        // Summary:
//        //     Gets the display name of the opcode for this event.
//        //
//        // Returns:
//        //     Returns a string that contains the display name of the opcode for this event.
//        public string OpcodeDisplayName { get; }
//        //
//        // Summary:
//        //     Gets the process identifier for the event provider that logged this event.
//        //
//        // Returns:
//        //     Returns an integer value. This value can be null.
//        public int? ProcessId { get; }
//        //
//        // Summary:
//        //     Gets the user-supplied properties of the event.
//        //
//        // Returns:
//        //     Returns a list of System.Diagnostics.Eventing.Reader.EventProperty objects.
//        public string Properties { get; }
//        //
//        // Summary:
//        //     Gets the globally unique identifier (GUID) of the event provider that published
//        //     this event.
//        //
//        // Returns:
//        //     Returns a GUID value. This value can be null.
//        public Guid? ProviderId { get; }
//        //
//        // Summary:
//        //     Gets the name of the event provider that published this event.
//        //
//        // Returns:
//        //     Returns a string that contains the name of the event provider that published
//        //     this event.
//        public string ProviderName { get; }
//        //
//        // Summary:
//        //     Gets qualifier numbers that are used for event identification.
//        //
//        // Returns:
//        //     Returns an integer value. This value can be null.
//        public int? Qualifiers { get; }
//        //
//        // Summary:
//        //     Gets the event record identifier of the event in the log.
//        //
//        // Returns:
//        //     Returns a long value. This value can be null.
//        public long? RecordId { get; }
//        //
//        // Summary:
//        //     Gets a globally unique identifier (GUID) for a related activity in a process
//        //     for which an event is involved.
//        //
//        // Returns:
//        //     Returns a GUID value. This value can be null.
//        public Guid? RelatedActivityId { get; }
//        //
//        // Summary:
//        //     Gets a task identifier for a portion of an application or a component that publishes
//        //     an event. A task is a 16-bit value with 16 top values reserved. This type allows
//        //     any value between 0x0000 and 0xffef to be used. For the name of the task, get
//        //     the value of the System.Diagnostics.Eventing.Reader.EventLogRecord.TaskDisplayName
//        //     property.
//        //
//        // Returns:
//        //     Returns an integer value. This value can be null.
//        public int? Task { get; }
//        //
//        // Summary:
//        //     Gets the display name of the task for the event.
//        //
//        // Returns:
//        //     Returns a string that contains the display name of the task for the event.
//        public string TaskDisplayName { get; }
//        //
//        // Summary:
//        //     Gets the thread identifier for the thread that the event provider is running
//        //     in.
//        //
//        // Returns:
//        //     Returns an integer value. This value can be null.
//        public int? ThreadId { get; }
//        //
//        // Summary:
//        //     Gets the time, in System.DateTime format, that the event was created.
//        //
//        // Returns:
//        //     Returns a System.DateTime value. The value can be null.
//        public DateTime? TimeCreated { get; }
//        //
//        // Summary:
//        //     Gets the security descriptor of the user whose context is used to publish the
//        //     event.
//        //
//        // Returns:
//        //     Returns a System.Security.Principal.SecurityIdentifier value.
//        public string UserId { get; }
//        //
//        // Summary:
//        //     Gets the version number for the event.
//        //
//        // Returns:
//        //     Returns a byte value. This value can be null.
//        public int? Version { get; }

//        //
//        // Summary:
//        //     Gets the event message in the current locale.
//        //
//        // Returns:
//        //     Returns a string that contains the event message in the current locale.
//        public string FormatDescription;
//        //
//        // Summary:
//        //     Gets the event message, replacing variables in the message with the specified
//        //     values.
//        //
//        // Parameters:
//        //   values:
//        //     The values used to replace variables in the event message. Variables are represented
//        //     by %n, where n is a number.
//        //
//        // Returns:
//        //     Returns a string that contains the event message in the current locale.
//        //public override string FormatDescription(IEnumerable<object> values);
//        //
//        // Summary:
//        //     Gets the enumeration of the values of the user-supplied event properties, or
//        //     the results of XPath-based data if the event has XML representation.
//        //
//        // Parameters:
//        //   propertySelector:
//        //     Selects the property values to return.
//        //
//        // Returns:
//        //     Returns a list of objects.
//        //public IList<object> GetPropertyValues(EventLogPropertySelector propertySelector);
//        //
//        // Summary:
//        //     Gets the XML representation of the event. All of the event properties are represented
//        //     in the event's XML. The XML conforms to the event schema.
//        //
//        // Returns:
//        //     Returns a string that contains the XML representation of the event.
//        //[SecuritySafeCritical]
//        public string Xml;
//        //
//        // Summary:
//        //     Releases the unmanaged resources used by this object, and optionally releases
//        //     the managed resources.
//        //
//        // Parameters:
//        //   disposing:
//        //     true to release both managed and unmanaged resources; false to release only unmanaged
//        //     resources.
//        //[SecuritySafeCritical]
//        //protected override void Dispose(bool disposing);
//    }
//}