/*
# Microshaoft
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
    using System.Activities.Tracking;
    using System.Activities.XamlIntegration;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Xaml;
    using System.Xml;
    using System.Runtime.DurableInstancing;
    public static class WorkFlowHelper
    {
        public static WorkflowApplication CreateWorkflowApplication
                                            (
                                                string xaml
                                                , string localAssemblyFilePath = null
                                                , Func<InstanceStore> onPersistProcessFunc = null
                                            )
        {
            var activity = XamlToActivity
                                (
                                    xaml
                                    , localAssemblyFilePath
                                );
            WorkflowApplication workflowApplication = new WorkflowApplication(activity);
            if (onPersistProcessFunc != null)
            {
                workflowApplication.InstanceStore = onPersistProcessFunc();
            }
            return workflowApplication;
        }
        public static Activity XamlToActivity
                                    (
                                        string xaml
                                        , string localAssemblyFilePath = null
                                    )
        {
            Assembly localAssembly = null;
            if (string.IsNullOrEmpty(localAssemblyFilePath))
            {
                localAssembly = Assembly
                                    .GetExecutingAssembly();
            }
            else
            {
                localAssembly = Assembly
                                    .LoadFrom(localAssemblyFilePath);
            }
            var stringReader = new StringReader(xaml);
            var xmlReader = XmlReader.Create(stringReader);
            var xamlXmlReader = new XamlXmlReader
                                                (
                                                    xmlReader
                                                    , new XamlXmlReaderSettings()
                                                    {
                                                        LocalAssembly = localAssembly
                                                    }
                                                );
            var xamlReader = ActivityXamlServices
                                        .CreateReader
                                            (
                                                xamlXmlReader
                                            );
            var activity = ActivityXamlServices
                                .Load
                                    (
                                        xamlReader
                                        , new ActivityXamlServicesSettings()
                                        {
                                            CompileExpressions = true
                                        }
                                    );
            return activity;
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
        public static TrackingParticipant GetTrackingParticipantFromJson<TTrackingParticipant>
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
