#if NETFRAMEWORK4_X
/*
/r:System.Xaml.dll
/r:System.Activities.dll
/r:System.Activities.DurableInstancing.dll
/r:System.Runtime.DurableInstancing.dll
/r:"D:\Microshaoft.Nuget.Packages\Newtonsoft.Json.7.0.1\lib\net45\Newtonsoft.Json.dll"
*/
namespace Microshaoft
{
    using Newtonsoft.Json.Linq;
    using System;
    using System.Activities;
    using System.Activities.Expressions;
    using System.Activities.Tracking;
    using System.Activities.XamlIntegration;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Runtime.DurableInstancing;
    using System.Xaml;
    using System.Xml;

    public static class WorkFlowHelper
    {
        private class DynamicActivityExtensionInfo
        {
            public string ID;
            public DynamicActivity DynamicActivity;
            public Type Type;
            public DateTime CompiledTime;

        }


        #region Member
        /// <summary>
        /// Compiled Expressions Type Cache
        /// </summary>
        private static ConcurrentDictionary<string, DynamicActivityExtensionInfo>
                        _cache = new ConcurrentDictionary<string, DynamicActivityExtensionInfo>();
        /// Object for lock, make one Expressions Type only be compiled once
        /// </summary>
        private static object _locker = new object();
        #endregion

        public static WorkflowApplication CreateApplication
                                            (
                                                string definitionID
                                                , Func<string> getDefinitionXamlProcessFunc
                                                , IDictionary<string, object> inputs = null
                                                , Func<InstanceStore> onPersistProcessFunc = null
                                            )
        {
            var definition = GetOrAddDefinition
                                (
                                    definitionID
                                    , () =>
                                    {
                                        var r = getDefinitionXamlProcessFunc();
                                        return r;
                                    }
                                );
            var dynamicActivity = definition.DynamicActivity;
            //var newDynamicActivity = new DynamicActivity();
            //AttachNewInstance(definition.Type, newDynamicActivity);

            WorkflowApplication workflowApplication = null;
            if (inputs == null)
            {
                workflowApplication = new WorkflowApplication(dynamicActivity);
            }
            else
            {
                workflowApplication = new WorkflowApplication(dynamicActivity, inputs);
            }
            

            if (onPersistProcessFunc != null)
            {
                workflowApplication.InstanceStore = onPersistProcessFunc();
            }
            return
                workflowApplication;
        }

        private static
                    DynamicActivityExtensionInfo
                        GetOrAddDefinition
                            (
                                string definitionID
                                , Func<string> getDefinitionXamlProcessFunc
                            )
        {
            DynamicActivityExtensionInfo r = null;
            var cached = _cache
                            .TryGetValue
                                (
                                    definitionID
                                    , out r
                                );
            if (!cached)
            {
                _locker
                    .LockIf
                        (
                            () =>
                            {
                                return
                                    (
                                        !_cache
                                            .TryGetValue
                                                (
                                                    definitionID
                                                    , out r
                                                )
                                    );
                            }
                            ,
                            () =>
                            {
                                Console.WriteLine($"Compile {definitionID}");
                                var xaml = getDefinitionXamlProcessFunc();
                                r = Compile(definitionID, xaml);
                                cached = _cache
                                            .TryAdd
                                                (
                                                    definitionID
                                                    , r
                                                );
                            }
                        );
            }
            return
                r;
        }

        private static
                    DynamicActivityExtensionInfo
                        Compile
                            (
                                string definitionID
                                , string xaml
                            //, string localAssemblyFilePath = null
                            )
        {
            var stringReader = new StringReader(xaml);
            var xmlReader = XmlReader
                                    .Create(stringReader);
            var xamlXmlReader = new XamlXmlReader
                                            (
                                                xmlReader
                                            );
            var xamlReader = ActivityXamlServices
                                        .CreateReader
                                            (
                                                xamlXmlReader
                                            );
            var activity =
                    ActivityXamlServices
                                    .Load
                                        (
                                            xamlReader
                                            , new ActivityXamlServicesSettings()
                                            {
                                                CompileExpressions = true
                                            }
                                        );
            DynamicActivity dynamicActivity = (DynamicActivity) activity;
            var got = TryGetCompiledResultType
                (
                    dynamicActivity
                    , out var compiledResultType
                );
            if (got)
            {
                AttachNewInstance(compiledResultType, dynamicActivity);
            }
            return
                new DynamicActivityExtensionInfo()
                {
                    ID = definitionID
                     ,
                    DynamicActivity = dynamicActivity
                     ,
                    Type = compiledResultType
                     ,
                    CompiledTime = DateTime.Now
                };
        }
        private static bool TryGetCompiledResultType
                                    (
                                        DynamicActivity activity
                                        , out Type type
                                    )
        {
            type = null;
            var settings = GetCompilerSettings(activity);
            var results = new TextExpressionCompiler
                                            (settings)
                                                .Compile();
            var r = results.HasErrors;
            if (!r)
            {
                type = results
                            .ResultType;
                r = (type == null);
            }
            if (r)
            {
                throw new Exception("Compilation failed.");
            }
            return r;
        }
        private static TextExpressionCompilerSettings
                                        GetCompilerSettings
                                                (
                                                    DynamicActivity dynamicActivity
                                                )
        {
            // activityName is the Namespace.Type of the activity that contains the
            // C# expressions. For Dynamic Activities this can be retrieved using the
            // name property , which must be in the form Namespace.Type.
            string activityName = dynamicActivity.Name;

            // Split activityName into Namespace and Type.Append _CompiledExpressionRoot to the type name
            // to represent the new type that represents the compiled expressions.
            // Take everything after the last . for the type name.
            string activityType = activityName.Split('.').Last() + "_CompiledExpressionRoot";
            // Take everything before the last . for the namespace.
            string activityNamespace = string.Join(".", activityName.Split('.').Reverse().Skip(1).Reverse());
            return
                new TextExpressionCompilerSettings
                {
                    Activity = dynamicActivity,
                    Language = "C#",
                    ActivityName = activityType,
                    ActivityNamespace = activityNamespace,
                    RootNamespace = null,
                    GenerateAsPartialClass = false,
                    AlwaysGenerateSource = true,
                    ForImplementation = true
                };
        }
        private static void AttachNewInstance(Type compiledResultType, DynamicActivity dynamicActivity)
        {
            /*
             * https://docs.microsoft.com/en-us/dotnet/framework/windows-workflow-foundation/csharp-expressions
             */
            // Create an instance of the new compiled expression type.
            ICompiledExpressionRoot
                compiledExpressionRoot =
                    Activator
                        .CreateInstance
                            (
                                compiledResultType
                                , new object[]
                                    {
                                        dynamicActivity
                                    }
                            ) as ICompiledExpressionRoot;
            // Attach it to the activity.
            CompiledExpressionInvoker
                .SetCompiledExpressionRootForImplementation
                    (
                        dynamicActivity
                        , compiledExpressionRoot
                    );
        }
        public static TrackingProfile GetTrackingProfileFromJson
            (
                string json
                , bool isArray = false
            )
        {
            TrackingProfile trackingProfile = null;
            var trackingQueries = GetTrackingQueriesFromJson(json, isArray);
            if (trackingQueries != null)
            {
                foreach (var trackingQuery in trackingQueries)
                {
                    if (trackingProfile == null)
                    {
                        trackingProfile = new TrackingProfile();
                    }
                    trackingProfile
                            .Queries
                            .Add(trackingQuery);
                }
            }
            return trackingProfile;
        }
        public static TrackingParticipant
                            GetTrackingParticipantFromJson<TTrackingParticipant>
                                (
                                    string json
                                    , bool isArray = false
                                )
            where TTrackingParticipant : TrackingParticipant, new()
        {
            TrackingParticipant trackingParticipant = null;
            TrackingProfile trackingProfile
                                = GetTrackingProfileFromJson(json, isArray);
            if (trackingProfile != null)
            {
                trackingParticipant = new TTrackingParticipant();
                trackingParticipant.TrackingProfile = trackingProfile;
            }
            return trackingParticipant;
        }
        public static IEnumerable<TrackingQuery> GetTrackingQueriesFromJson
                                                        (
                                                            string json
                                                            , bool isArray = false
                                                        )
        {
            IEnumerable<TrackingQuery> r = null;
            if (isArray)
            {
                //闭包
                var key = string.Empty;
                r = JsonHelper
                        .DeserializeToFromDictionary<string, JObject[], JObject[]>
                                (
                                    json
                                    , (x, y) =>
                                    {
                                        //闭包
                                        key = x;
                                        return y;
                                    }
                                )
                                .SelectMany
                                    (
                                        (x) =>
                                        {
                                            return x;
                                        }
                                    )
                                .Select
                                    (
                                        (x) =>
                                        {
                                            //闭包
                                            return
                                                GetTrackingQuery(key, x);
                                        }
                                    );
            }
            else
            {
                r = JsonHelper
                        .DeserializeToFromDictionary<string, JObject, TrackingQuery>
                            (
                                json
                                , (x, y) =>
                                {
                                    return GetTrackingQuery(x, y);
                                }
                            );
            }
            return r;
        }
        public static TrackingQuery GetTrackingQuery(string queryName, JObject jObject)
        {
            var json = jObject.ToString();
            return
                GetTrackingQuery
                        (
                            queryName
                            , json
                        );
        }
        public static TrackingQuery GetTrackingQuery(string queryName, string json)
        {
            TrackingQuery r = null;
            if (string.Compare(queryName, "WorkflowInstanceQuery", true) == 0)
            {
                r = JsonHelper
                            .DeserializeByJTokenPath<WorkflowInstanceQuery>
                                (
                                    json
                                );
            }
            else if (string.Compare(queryName, "ActivityStateQuery", true) == 0)
            {
                r = JsonHelper
                            .DeserializeByJTokenPath<ActivityStateQuery>
                                (
                                    json
                                );
            }
            else if (string.Compare(queryName, "CustomTrackingQuery", true) == 0)
            {
                r = JsonHelper
                            .DeserializeByJTokenPath<CustomTrackingQuery>
                                (
                                    json
                                );
            }
            else if (string.Compare(queryName, "FaultPropagationQuery", true) == 0)
            {
                r = JsonHelper
                            .DeserializeByJTokenPath<FaultPropagationQuery>
                                (
                                    json
                                );
            }
            else if (string.Compare(queryName, "BookmarkResumptionQuery", true) == 0)
            {
                r = JsonHelper
                            .DeserializeByJTokenPath<BookmarkResumptionQuery>
                                (
                                    json
                                );
            }
            else if (string.Compare(queryName, "ActivityScheduledQuery", true) == 0)
            {
                r = JsonHelper
                            .DeserializeByJTokenPath<ActivityScheduledQuery>
                                (
                                    json
                                );
            }
            else if (string.Compare(queryName, "CancelRequestedQuery", true) == 0)
            {
                r = JsonHelper
                            .DeserializeByJTokenPath<CancelRequestedQuery>
                                (
                                    json
                                );
            }
            return r;
        }
    }
}

namespace Microshaoft
{
    using System;
    using System.Activities.Tracking;
    public class CommonTrackingParticipant : TrackingParticipant
    {
        public Func<TrackingRecord, TimeSpan, bool> OnTrackingRecordReceived;
        protected override void Track(TrackingRecord record, TimeSpan timeout)
        {
            var r = false;
            if (OnTrackingRecordReceived != null)
            {
                r = OnTrackingRecordReceived(record, timeout);
            }
        }
    }
}

#endif
