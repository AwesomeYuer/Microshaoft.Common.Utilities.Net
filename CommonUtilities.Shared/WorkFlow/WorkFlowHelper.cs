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
        private class WorkFlowDefinition
        {
            public string ID;
            public Activity Activity;
            public Type Type;
            public DateTime CompiledTime;

        }


        #region Member
        /// <summary>
        /// Compiled Expressions Type Cache
        /// </summary>
        private static ConcurrentDictionary<string, WorkFlowDefinition>
                        _cache = new ConcurrentDictionary<string, WorkFlowDefinition>();
        /// Object for lock, make one Expressions Type only be compiled once
        /// </summary>
        private static object _locker = new object();
        #endregion

        public static WorkflowApplication CreateApplication
                                            (
                                                string definitionID
                                                , Func<string> getDefinitionXamlProcessFunc
                                                //, string localAssemblyFilePath = null
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
            var activity = definition.Activity;
            var workflowApplication = new WorkflowApplication(activity);
            if (onPersistProcessFunc != null)
            {
                workflowApplication.InstanceStore = onPersistProcessFunc();
            }
            return
                workflowApplication;
        }

        private static
                    WorkFlowDefinition
                        GetOrAddDefinition
                            (
                                string definitionID
                                , Func<string> getDefinitionXamlProcessFunc
                            )
        {
            WorkFlowDefinition r = null;
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
                    WorkFlowDefinition
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
            if
                (
                    TryGetCompiledResultType
                        (
                            (DynamicActivity)activity
                            , out var type
                        )
                )
            {
                CompileExpressions(type, activity);
            }
            return
                new WorkFlowDefinition()
                {
                    ID = definitionID
                     ,
                    Activity = activity
                     ,
                    Type = type
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
                r = (type != null);
            }
            return r;
        }
        private static TextExpressionCompilerSettings
                                        GetCompilerSettings
                                                (
                                                    DynamicActivity dynamicActivity
                                                )
        {
            int index = dynamicActivity.Name.LastIndexOf('.');
            //int length = dynamicActivity.Name.Length;
            string activityName =
                        (
                            (index > 0)
                            ?
                            dynamicActivity.Name.Substring(index + 1)
                            :
                            dynamicActivity.Name
                        );
            activityName += "_CompiledExpressionRoot";
            string activityNamespace =
                        (
                            (index > 0)
                            ?
                            dynamicActivity.Name.Substring(0, index)
                            :
                            null
                        );
            return
                new TextExpressionCompilerSettings
                {
                    Activity = dynamicActivity
                        ,
                    ActivityName = activityName
                        ,
                    ActivityNamespace = activityNamespace
                        ,
                    RootNamespace = null
                        ,
                    GenerateAsPartialClass = false
                        ,
                    AlwaysGenerateSource = true //if false,sometime return null type after compile
                        ,
                    Language = "C#"
                };
        }
        private static void CompileExpressions(Type type, Activity activity)
        {
            ICompiledExpressionRoot
                compiledExpressionRoot =
                    Activator
                        .CreateInstance
                            (
                                type
                                , new object[]
                                    {
                                        activity
                                    }
                            ) as ICompiledExpressionRoot;
            CompiledExpressionInvoker
                .SetCompiledExpressionRootForImplementation
                    (
                        activity
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
