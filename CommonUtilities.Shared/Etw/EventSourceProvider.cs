#if NETFRAMEWORK4_X
namespace Microshaoft.EventSources
{
    using System;
    using Microsoft.Diagnostics.Tracing;
    //
    // Shows how to use EventSouces to send messages to the Windows EventLog
    //
    // * EventLogEventSource
    //	 * Uses the new 'Channel' attribute to indicate that certain events
    //	   should go to the Windows Event Log.   Note that the EventSource
    //	   has to be registered with the Windows OS for this to work.  
    //	   In this example we send messages to the 'Admin' channel.   
    //
    // * EventLogEventSourceDemo 
    //	 * simulates a deployment step needed to register the event source's 
    //	   manifest on the machine. If running unelevated it will prompt the user
    //	   to allow running wevtutil.exe, the tool that performs the registration.
    //
    //	 * simulates processing multiple requests by calling the methods on 
    //	   EventLogEventSource to fire events.
    //	 * pauses to allow the user to examine the event logs created under 
    //	   'Application and Services Logs/Microsoft/EventSourceDemos/Channeled'
    //
    //	 * undoes the registration steps performed earlier.
    //
    [
        EventSource
            (
                Name = "Microsoft-EventLogEventSource-Uttm" //"Samples-EventSourceDemos-EventLog"
            )
    ]
    public sealed class EventLogEventSource : EventSource
    {
#region Singleton instance
        public static EventLogEventSource Log = new EventLogEventSource();
#endregion
        [
            Event
                (
                    1
                    , ActivityOptions = EventActivityOptions.None
                    , Channel = EventChannel.Admin
                    , Keywords = Keywords.EventKeyword1
                    //, Level = EventLevel.LogAlways
                    , Message =
@"
	sequenceID : ""{0}""
	, contextID : ""{1}""
	, operatorID : ""{2}""
	, data : ""{3}""
"
                    , Opcode = EventOpcode.Info
                    , Tags = EventTags.None
                    , Task = EventTasks.EventTask1
                    , Version = 1
                )
        ]
        public void WriteEvent1Log
                    (
                        int sequenceID
                        , Guid contextID
                        , string operatorID
                        , string data
                    )
        {
            WriteEvent
                (
                    1
                    , (object)sequenceID
                    , (object)contextID
                    , (object)operatorID
                    , (object)data
                );
        }
        [
            Event
                (
                    2
                    , ActivityOptions = EventActivityOptions.None
                    , Channel = EventChannel.Admin
                    , Keywords = Keywords.EventKeyword2
                    //, Level = EventLevel.LogAlways
                    , Message =
@"
	sequenceID : ""{0}""
	, contextID : ""{1}""
	, operatorID : ""{2}""
	, data : ""{3}""
"
                    , Opcode = EventOpcode.Info
                    , Tags = Tags.EventTag2
                    , Task = EventTasks.EventTask2
                    , Version = 1
                )
        ]
        public void WriteEvent2Log
            (
                int sequenceID
                , Guid contextID
                , string operatorID
                , string data
            )
        {
            WriteEvent
                (
                    2
                    , (object)sequenceID
                    , (object)contextID
                    , (object)operatorID
                    , (object)data
                );
        }
#region Keywords / Tasks / Opcodes
        public class Keywords   // This is a bitvector
        {
            public const EventKeywords EventKeyword1 = (EventKeywords)0x00000001;
            public const EventKeywords EventKeyword2 = (EventKeywords)0x00000002;
        }
        public class EventTasks
        {
            //[1,65534]
            public const EventTask EventTask1 = (EventTask)1;
            public const EventTask EventTask2 = (EventTask)2;
        }
        public class EventOpcodes
        {
            public const EventOpcode EventOpcode_0x0b = (EventOpcode)0x0b;
        }
        public class Tags
        {
            public const EventTags EventTag1 = (EventTags)1;
            public const EventTags EventTag2 = (EventTags)2;
        }
#endregion
    }
}
//namespace Microshaoft.EventListeners
//{
//    using System;
//    using System.IO;
//    using System.Linq;
//    using Microsoft.Diagnostics.Tracing;
//    /// <summary>
//    /// An EventListener is the most basic 'sink' for EventSource events.   All other sinks of 
//    /// EventSource data can be thought of as 'built in' EventListeners.	In any particular 
//    /// AppDomain all the EventSources send messages to any EventListener in the same
//    /// AppDomain that have subscribed to them (using the EnableEvents API.
//    /// <para>
//    /// You create a particular kind of EventListener by subclassing the EventListener class
//    /// Here we create an EventListener that 
//    ///   . Enables all events on any EventSource-derived class created in the appDomain
//    ///   . Sends all events raised by the event source classes created to the 'Out' textWriter 
//    ///	 (typically the Console).  
//    /// </para>
//    /// </summary>
//    public class ConsoleEventListener : EventListener
//    {
//        static TextWriter Out = Console.Out;
//        /// <summary>
//        /// Override this method to get a list of all the eventSources that exist.  
//        /// </summary>
//        protected override void OnEventSourceCreated(EventSource eventSource)
//        {
//            // Because we want to turn on every EventSource, we subscribe to a callback that triggers
//            // when new EventSources are created.  It is also fired when the EventListner is created
//            // for all pre-existing EventSources.  Thus this callback get called once for every 
//            // EventSource regardless of the order of EventSource and EventListener creation.  
//            // For any EventSource we learn about, turn it on.   
//            //Console.WriteLine(eventSource.GetType());
//            EnableEvents
//                (
//                    eventSource
//                    , EventLevel.LogAlways
//                    , EventKeywords.All
//                );
//        }
//        /// <summary>
//        /// We override this method to get a callback on every event we subscribed to with EnableEvents
//        /// </summary>
//        /// <param name="eventData"></param>
//        protected override void OnEventWritten(EventWrittenEventArgs eventData)
//        {
//            // report all event information
//            Out.Write
//                    (
//                        "  Event {0} "
//                        , eventData.EventName
//                    );
//            // Don't display activity information, as that's not used in the demos
//            // Out.Write(" (activity {0}{1}) ", ShortGuid(eventData.ActivityId), 
//            //								  eventData.RelatedActivityId != Guid.Empty ? "->" + ShortGuid(eventData.RelatedActivityId) : "");
//            // Events can have formatting strings 'the Message property on the 'Event' attribute.  
//            // If the event has a formatted message, print that, otherwise print out argument values.  
//            if (eventData.Message != null)
//            {
//                Out.WriteLine
//                    (
//                        eventData.Message
//                        , eventData.Payload != null
//                            ?
//                            eventData.Payload.ToArray()
//                            :
//                            null
//                    );
//            }
//            else
//            {
//                string[] sargs =
//                                    (
//                                        eventData.Payload != null
//                                        ?
//                                        eventData
//                                            .Payload
//                                            .Select(o => o.ToString())
//                                            .ToArray()
//                                        :
//                                        null
//                                    );
//                Out.WriteLine("({0}).", sargs != null ? string.Join(", ", sargs) : "");
//            }
//        }
//    }
//}
namespace Microshaoft
{
    /// <summary>
    /// For the Windows EventLog to listen for EventSources, they must be
    /// registered with the operating system.  This is a deployment step 
    /// (typically done by a installer).   For demo purposes, however we 
    /// have written code run by the demo itself that accomplishes this 
    /// </summary>
    using System;
    using System.IO;
    using System.Diagnostics;
    using System.Threading;
    public static class RegisterEventSourceWithOperatingSystemHelper
    {
        static TextWriter Out = Console.Out;
        /// <summary>
        /// Simulate an installation to 'destFolder' for the named eventSource.  If you don't
        /// specify eventSourceName all eventSources information next to the EXE is registered.
        /// </summary>
        public static void SimulateInstall
                                (
                                    string sourceFolder
                                    , string destFolder
                                    , string eventSourceName = ""
                                    , bool prompt = true
                                )
        {
            Out.WriteLine("Simulating the steps needed to register the EventSource with the OS");
            Out.WriteLine("These steps are only needed for Windows Event Log support.");
            Out.WriteLine("Admin privileges are needed to do this, so you will see elevation prompts");
            Out.WriteLine("If you are not already elevated.  Consider running from an admin window.");
            Out.WriteLine();
            if (prompt)
            {
                Out.WriteLine("Press <Enter> to proceed with installation");
                Console.ReadLine();
            }
            Out.WriteLine("Deploying EventSource to {0}", destFolder);
            // create deployment folder if needed
            if (Directory.Exists(destFolder))
            {
                Out.WriteLine("Error: detected a previous deployment.   Cleaning it up.");
                SimulateUninstall(destFolder, false);
                Out.WriteLine("Done Cleaning up orphaned installation.");
            }
            Out.WriteLine("Copying the EventSource manifest and compiled Manifest DLL to target directory.");
            Directory.CreateDirectory(destFolder);
            foreach (var filename in Directory.GetFiles(sourceFolder, "*" + eventSourceName + "*.etwManifest.???"))
            {
                var destPath = Path.Combine(destFolder, Path.GetFileName(filename));
                Out.WriteLine("xcopy \"{0}\" \"{1}\"", filename, destPath);
                File.Copy(filename, destPath, true);
            }
            Out.WriteLine("Registering the manifest with the OS (Need to be elevated)");
            foreach (var filename in Directory.GetFiles(destFolder, "*.etwManifest.man"))
            {
                var commandArgs = string
                                        .Format
                                            (
                                                "im {0} /rf:\"{1}\" /mf:\"{1}\""
                                                , filename
                                                , Path.Combine
                                                        (
                                                            destFolder
                                                            , Path.GetFileNameWithoutExtension(filename) + ".dll"
                                                        )
                                            );
                // as a precaution uninstall the manifest.   It is easy for the demos to not be cleaned up 
                // and the install will fail if the EventSource is already registered.   
                var process = Process
                                .Start
                                    (
                                        new ProcessStartInfo("wevtutil.exe", "um" + commandArgs.Substring(2))
                                        {
                                            Verb = "runAs"
                                            ,
                                            RedirectStandardOutput = true
                                            ,
                                            UseShellExecute = false
                                        }
                                    );
                Out.WriteLine("  wevtutil " + commandArgs);
                Console.WriteLine
                        (
                            "wevtutil OUTPUT: {0}"
                            , process.StandardOutput.ReadToEnd()
                        );
                process.WaitForExit();
                Thread.Sleep(200);      // just in case elevation makes the wait not work.  
                                        // The 'RunAs' indicates it needs to be elevated. 
                                        // Unfortunately this also makes it problematic to get the output or error code.  
                Out.WriteLine("  wevtutil " + commandArgs);
                process = Process
                            .Start
                                (
                                    new ProcessStartInfo("wevtutil.exe", commandArgs)
                                    {
                                        Verb = "runAs"
                                        ,
                                        RedirectStandardOutput = true
                                        ,
                                        UseShellExecute = false
                                    }
                                );
                Console.WriteLine
                        (
                            "wevtutil OUTPUT: {0}"
                            , process.StandardOutput.ReadToEnd()
                        );
                process.WaitForExit();
            }
            Thread.Sleep(1000);
            Out.WriteLine("Done deploying app.");
            Out.WriteLine();
        }
        /// <summary>
        /// Reverses the Install step 
        /// </summary>
        public static void SimulateUninstall(string destFolder, bool prompt = true)
        {
            Out.WriteLine("Uninstalling the EventSoure demos from {0}", destFolder);
            Out.WriteLine("This also requires elevation.");
            Out.WriteLine("Please close the event viewer if you have not already done so!");
            if (prompt)
            {
                Out.WriteLine("Press <Enter> to proceed with uninstall.");
                Console.ReadLine();
            }
            // run wevtutil elevated to unregister the ETW manifests
            Out.WriteLine("Unregistering manifests");
            foreach (var filename in Directory.GetFiles(destFolder, "*.etwManifest.man"))
            {
                var commandArgs = string.Format("um {0}", filename);
                Out.WriteLine("	wevtutil " + commandArgs);
                // The 'RunAs' indicates it needs to be elevated.  
                var process = Process.Start(new ProcessStartInfo("wevtutil.exe", commandArgs) { Verb = "runAs" });
                process.WaitForExit();
            }
            Out.WriteLine("Removing {0}", destFolder);
            // If this fails, it means that something is using the directory.  Typically this is an eventViewer or 
            // a command prompt in that directory or visual studio.	If all else fails, rebooting should fix this.  
            if (Directory.Exists(destFolder))
            {
                Directory.Delete(destFolder, true);
            }
            Out.WriteLine("Done uninstalling app.");
        }
    }
}

#endif
