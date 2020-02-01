#if NETCOREAPP 
namespace Microshaoft.Web
{
    using Microshaoft;
    using Microsoft.Extensions.Configuration;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Data;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;

    public interface IStoreProceduresService
    {
        (
            bool Success
            , JToken Result
            , TimeSpan? DbExecutingDuration
        )
               Process
                       (
                           string connectionString
                           , string dataBaseType
                           , string storeProcedureName
                           , JToken parameters = null
                           , Func
                               <
                                    int             // resultSet index
                                    , IDataReader
                                    , int           // row index
                                    , int           // column index
                                    , Type          // fieldType
                                    , string        // fieldName
                                    ,
                                        (
                                            bool NeedDefaultProcess
                                            , JProperty Field   //  JObject Field 对象
                                        )
                               > onReadRowColumnProcessFunc = null
                           , bool enableStatistics = false
                           , int commandTimeoutInSeconds = 90
                       );
        Task
            <
                (
                    bool Success
                    , JToken Result
                    , TimeSpan? DbExecutingDuration
                )
            >
               ProcessAsync
                       (
                           string connectionString
                           , string dataBaseType
                           , string storeProcedureName
                           , JToken parameters = null
                           , Func
                               <
                                    int             // resultSet index
                                    , IDataReader
                                    , int           // row index         
                                    , int           // column index
                                    , Type          // fieldType
                                    , string        // fieldName
                                    ,
                                        (
                                            bool NeedDefaultProcess
                                            , JProperty Field   //  JObject Field 对象
                                        )
                               > onReadRowColumnProcessFunc = null
                           , bool enableStatistics = false
                           , int commandTimeoutInSeconds = 90
                       );
    }

    public interface IStoreProceduresWebApiService
    {
        (
            int StatusCode
            , string Message
            , JToken Result
            , TimeSpan? DbExecutingDuration
        )
            Process
                (
                    string routeName
                    , JToken parameters = null
                    , Func
                            <
                                int             // resultSet index
                                , IDataReader
                                , int           // row index        
                                , int           // column index
                                , Type          // fieldType
                                , string        // fieldName
                                , 
                                    (
                                        bool NeedDefaultProcess
                                        , JProperty Field   //  JObject Field 对象
                                    )
                            > onReadRowColumnProcessFunc = null
                    , string httpMethod = "Get"
                    //, bool enableStatistics = false
                    , int commandTimeoutInSeconds = 101
                );

        Task
            <
                (
                    int StatusCode
                    , string Message
                    , JToken Result
                    , TimeSpan? DbExecutingDuration
                )
            >
                ProcessAsync
                    (
                        string routeName
                        , JToken parameters = null
                        , Func
                                <
                                    int             // resultSet index
                                    , IDataReader
                                    , int           // row index
                                    , int           // column index
                                    , Type          // fieldType
                                    , string        // fieldName
                                    , 
                                        (
                                            bool NeedDefaultProcess
                                            , JProperty Field   //  JObject Field 对象
                                        )
                                > onReadRowColumnProcessFunc = null
                        , string httpMethod = "Get"
                        //, bool enableStatistics = false
                        , int commandTimeoutInSeconds = 101
                    );
    }

    public abstract partial class
                AbstractStoreProceduresService
                                :
                                    IStoreProceduresWebApiService
                                    , IStoreProceduresService
    {
        private class DatabaseTypeComparer
                        : IEqualityComparer<IStoreProcedureExecutable>
        {
            public bool Equals
                            (
                                IStoreProcedureExecutable x
                                , IStoreProcedureExecutable y
                            )
            {
                return
                    (
                        x.DataBaseType
                        ==
                        y.DataBaseType
                    );
            }
            public int GetHashCode(IStoreProcedureExecutable obj)
            {
                return
                        -1;
            }
        }
        private static readonly object _locker = new object();
        protected readonly IConfiguration _configuration;

        public readonly ConcurrentDictionary<string, ExecutingInfo>
                            _dbParametersDefinitionCachingStore;
        public AbstractStoreProceduresService
                    (
                        IConfiguration configuration
                        , ConcurrentDictionary<string, ExecutingInfo>
                                    dbParametersDefinitionCachingStore
                    )
        {
            _configuration = configuration;
            _dbParametersDefinitionCachingStore
                = dbParametersDefinitionCachingStore;
            Initialize();
        }
        //for override from derived class
        public virtual void Initialize()
        {
            _cachedParametersDefinitionExpiredInSeconds =
                 _configuration
                            .GetValue
                                (
                                    "CachedParametersDefinitionExpiredInSeconds"
                                    , 3600
                                );
            _needAutoRefreshExecutedTimeForSlideExpire =
                _configuration
                            .GetValue
                                (
                                    "NeedAutoRefreshExecutedTimeForSlideExpire"
                                    , false
                                );
            LoadDynamicExecutors();
        }
        protected virtual string[] GetDynamicExecutorsPathsProcess()
        {
            var result =
                    _configuration
                        .GetSection("DynamicExecutorsPaths")
                        .AsEnumerable()
                        .Select
                            (
                                (x) =>
                                {
                                    return
                                        x.Value;
                                }
                            )
                        .ToArray();
            return
                result;
        }

        protected virtual void LoadDynamicExecutors
                        (
                            //string dynamicLoadExecutorsPathsJsonFile = "dynamicCompositionPluginsPaths.json"
                        )
        {
            var executingDirectory = Path
                                        .GetDirectoryName
                                                (
                                                    Assembly
                                                        .GetExecutingAssembly()
                                                        .Location
                                                );
            var executors =
                    GetDynamicExecutorsPathsProcess
                            (
                                //dynamicLoadExecutorsPathsJsonFile
                            )
                        .Select
                            (
                                (x) =>
                                {
                                    var path = x;
                                    if (!path.IsNullOrEmptyOrWhiteSpace())
                                    {
                                        if
                                            (
                                                x.StartsWith(".")
                                            )
                                        {
                                            path = path.TrimStart('.', '\\', '/');
                                        }
                                        path = Path.Combine
                                                        (
                                                            executingDirectory
                                                            , path
                                                        );
                                    }
                                    return path;
                                }
                            )
                        .Where
                            (
                                (x) =>
                                {
                                    return
                                        (
                                            !x
                                                .IsNullOrEmptyOrWhiteSpace()
                                            &&
                                            Directory
                                                .Exists(x)
                                        );
                                }
                            )
                        .SelectMany
                            (
                                (x) =>
                                {
                                    return
                                        CompositionHelper
                                                .ImportManyExportsComposeParts
                                                    <IStoreProcedureExecutable>
                                                        (
                                                            x
                                                            , "*SPE*Plugin*.dll"
                                                        );
                                }
                            );
            var indexedExecutors =
                    executors
                        .Distinct
                            (
                                 new DatabaseTypeComparer()
                            )
                        .ToDictionary
                            (
                                (x) =>
                                {
                                    return
                                        x.DataBaseType;
                                }
                                ,
                                (x) =>
                                {
                                    if (x is IParametersDefinitionCacheAutoRefreshable definitionCache)
                                    {
                                        definitionCache
                                            .CachedParametersDefinitionExpiredInSeconds
                                                = CachedParametersDefinitionExpiredInSeconds;
                                        definitionCache
                                            .NeedAutoRefreshExecutedTimeForSlideExpire
                                                = NeedAutoRefreshExecutedTimeForSlideExpire;
                                    }
                                    x.InitializeInvokingCachingStore(_dbParametersDefinitionCachingStore);
                                    return x;
                                }
                             
                            );
            _locker
                .LockIf
                    (
                        () =>
                        {
                            return
                                (
                                    _indexedExecutors
                                    ==
                                    null
                                );
                        }
                        , () =>
                        {
                            _indexedExecutors = indexedExecutors;
                        }
                    );
        }
        private int _cachedParametersDefinitionExpiredInSeconds = 3600;
        protected virtual int CachedParametersDefinitionExpiredInSeconds
        {
            get => _cachedParametersDefinitionExpiredInSeconds;
            private set => _cachedParametersDefinitionExpiredInSeconds = value;
        }
        private bool _needAutoRefreshExecutedTimeForSlideExpire = true;
        protected virtual bool NeedAutoRefreshExecutedTimeForSlideExpire
        {
            get => _needAutoRefreshExecutedTimeForSlideExpire;
            private set => _needAutoRefreshExecutedTimeForSlideExpire = value;
        }

        private
            IDictionary
                    <
                        string
                        , IStoreProcedureExecutable
                    >
                        _indexedExecutors;
        public
            IDictionary
                    <
                        string
                        , IStoreProcedureExecutable
                    >
                        IndexedExecutors
        {
            get => _indexedExecutors;
            set => _indexedExecutors = value;
        }

        public
            (
                int StatusCode
                , string Message
                , JToken Result
                , TimeSpan? DbExecutingDuration
            )
                Process
                    (
                        string routeName
                        , JToken parameters = null
                        , Func
                            <
                                int             // resultSet index
                                , IDataReader
                                , int           // row index
                                , int           // column index
                                , Type          // fieldType
                                , string        // fieldName
                                ,
                                    (
                                        bool NeedDefaultProcess
                                        , JProperty Field   //  JObject Field 对象
                                    )
                            > onReadRowColumnProcessFunc = null
                        , string httpMethod = "Get"
                        //, bool enableStatistics = false
                        , int commandTimeoutInSeconds = 101
                    )
        {
            JToken result = null;

            bool success;
            int statusCode;
            string message;
            string connectionString;
            string dataBaseType;
            string storeProcedureName;
            bool enableStatistics;
            TimeSpan? dbExecutingDuration = null;

            (
                success
                , statusCode
                , httpMethod
                , message
                , connectionString
                , dataBaseType
                , storeProcedureName
                , commandTimeoutInSeconds
                , enableStatistics
            )
                = TryGetStoreProcedureInfo
                            (
                                routeName
                                , httpMethod
                            );
            if 
                (
                    success
                    &&
                    statusCode == 200
                )
            {
                (
                    success
                    , result
                    , dbExecutingDuration
                ) = Process
                                        (
                                              connectionString
                                            , dataBaseType
                                            , storeProcedureName
                                            , parameters
                                            , onReadRowColumnProcessFunc
                                            , enableStatistics
                                            , commandTimeoutInSeconds
                                        );

                if
                    (
                        success
                        &&
                        result != null
                    )
                {
                    HttpResponseParametersProcess
                            (
                                ref result
                                , ref statusCode
                                , ref message
                            );
                }
            }
            return
                (
                    statusCode
                    , message
                    , result
                    , dbExecutingDuration
                );
        }

        public async
            Task
                <
                    (
                        int StatusCode
                        , string Message
                        , JToken Result
                        , TimeSpan? DbExecutingDuration
                    )
                >
            ProcessAsync
                (
                    string routeName
                    , JToken parameters = null
                    , Func
                        <
                            int             // resultSet index
                            , IDataReader
                            , int           // row index    
                            , int           // column index
                            , Type          // fieldType
                            , string        // fieldName
                            ,
                                (
                                    bool NeedDefaultProcess
                                    , JProperty Field   //  JObject Field 对象
                                )
                        > onReadRowColumnProcessFunc = null
                    , string httpMethod = "Get"
                    //, bool enableStatistics = false
                    , int commandTimeoutInSeconds = 101
                )
        {
            JToken result = null;

            bool success;
            int statusCode;
            string message;
            string connectionString;
            string dataBaseType;
            string storeProcedureName;
            TimeSpan? dbExecutingDuration = null;
            bool enableStatistics;

            (
                success
                , statusCode
                , httpMethod
                , message
                , connectionString
                , dataBaseType
                , storeProcedureName
                , commandTimeoutInSeconds
                , enableStatistics
            ) 
                = TryGetStoreProcedureInfo
                        (
                            routeName
                            , httpMethod
                        );
            if
                (
                    success
                    &&
                    statusCode == 200
                )
            {

                (
                    success
                    , result
                    , dbExecutingDuration
                ) = await
                                        ProcessAsync
                                            (
                                                  connectionString
                                                , dataBaseType
                                                , storeProcedureName
                                                , parameters
                                                , onReadRowColumnProcessFunc
                                                , enableStatistics
                                                , commandTimeoutInSeconds
                                            );

                if 
                    (
                        success
                        &&
                        result != null
                    )
                {
                    HttpResponseParametersProcess
                            (
                                ref result
                                , ref statusCode
                                , ref message
                            );
                }
            }
            return
                (
                    statusCode
                    , message
                    , result
                    , dbExecutingDuration
                );
        }

        private void HttpResponseParametersProcess
                        (
                            ref JToken result
                            , ref int statusCode
                            , ref string message
                        )
        {
            if 
                (
                    result
                        ["Outputs"]
                        ["Parameters"] is JObject jObject
                )
            {
                if
                    (
                        jObject
                            .TryGetValue
                                (
                                    "HttpResponseStatusCode"
                                    , StringComparison
                                            .OrdinalIgnoreCase
                                    , out JToken jv
                                )
                    )
                {
                    statusCode = jv.Value<int>();
                }
                if
                    (
                        jObject
                            .TryGetValue
                                (
                                    "HttpResponseMessage"
                                    , StringComparison
                                            .OrdinalIgnoreCase
                                    , out jv
                                )
                    )
                {
                    message = jv.Value<string>();
                }
            }
        }

        public virtual
                (
                    bool Success
                    , JToken Result
                    , TimeSpan? DbExecutingDuration

                )
                    Process
                            (
                                string connectionString
                                , string dataBaseType
                                , string storeProcedureName
                                , JToken parameters = null
                                , Func
                                    <
                                        int             // resultSet index
                                        , IDataReader
                                        , int           // row index
                                        , int           // column index
                                        , Type          // fieldType
                                        , string        // fieldName
                                        ,
                                            (
                                                bool NeedDefaultProcess
                                                , JProperty Field   //  JObject Field 对象
                                            )
                                    > onReadRowColumnProcessFunc = null
                                , bool enableStatistics = false
                                , int commandTimeoutInSeconds = 90
                            )
        {
            var beginTimeStamp = Stopwatch.GetTimestamp();
            var beginTime = DateTime.Now;
            
            var success = _indexedExecutors
                                        .TryGetValue
                                            (
                                                dataBaseType
                                                , out var executor
                                            );
            JToken result = null;
            TimeSpan? dbExecutingDuration = null;
            if (success)
            {

                (
                    success
                    , result
                    , dbExecutingDuration
                ) = executor
                            .ExecuteJsonResults
                                (
                                    connectionString
                                    , storeProcedureName
                                    , parameters
                                    , onReadRowColumnProcessFunc
                                    , enableStatistics
                                    , commandTimeoutInSeconds
                                );
                if (success)
                {
                    ResultTimingProcess
                        (
                            result
                            , beginTime
                            , beginTimeStamp
                            , dbExecutingDuration
                        );
                }
            }
            return
                    (
                        success
                        , result
                        , dbExecutingDuration
                    );
        }

        private static void ResultTimingProcess
                (
                    JToken result
                    , DateTime beginTime
                    , long beginTimeStamp
                    , TimeSpan? dbExecutingDuration
                )
        {
            result["BeginTime"] = beginTime;
            result["EndTime"] = DateTime.Now;
            var x = result["DbExecutingDurationInMilliseconds"];
            if
                (
                    dbExecutingDuration.HasValue
                )
            {
                x = dbExecutingDuration.Value.TotalMilliseconds;
            }
            result["DurationInMilliseconds"]
                = beginTimeStamp
                        .GetElapsedTimeToNow()
                        .TotalMilliseconds;
        }

        public virtual async
                    Task
                        <
                            (
                                bool Success
                                , JToken Result
                                , TimeSpan? DbExecutingDuration
                            )
                        >
            ProcessAsync
                (
                    string connectionString
                    , string dataBaseType
                    , string storeProcedureName
                    , JToken parameters = null
                    , Func
                        <
                            int             // resultSet index
                            , IDataReader
                            , int           // row index
                            , int           // column index
                            , Type          // fieldType
                            , string        // fieldName
                            ,
                                (
                                    bool NeedDefaultProcess
                                    , JProperty Field   //  JObject Field 对象
                                )
                        > onReadRowColumnProcessFunc = null
                    , bool enableStatistics = false
                    , int commandTimeoutInSeconds = 90
                )
        {
            var beginTimeStamp = Stopwatch.GetTimestamp();
            var beginTime = DateTime.Now;
            var success = _indexedExecutors
                                        .TryGetValue
                                            (
                                                dataBaseType
                                                , out var executor
                                            );
            JToken result = null;
            TimeSpan? dbExecutingDuration = null;
            if (success)
            {

                //executor
                //    .InitializeInvokingCachingStore
                //            (
                //                _dbParametersDefinitionCachingStore
                //            );
                (
                    success
                    , result
                    , dbExecutingDuration
                ) = await
                        executor
                            .ExecuteJsonResultsAsync
                                (
                                    connectionString
                                    , storeProcedureName
                                    , parameters
                                    , onReadRowColumnProcessFunc
                                    , enableStatistics
                                    , commandTimeoutInSeconds
                                );
                if (success)
                {
                    ResultTimingProcess
                        (
                            result
                            , beginTime
                            , beginTimeStamp
                            , dbExecutingDuration
                        );
                }
            }
            return
                    (
                        success
                        , result
                        , dbExecutingDuration
                    );
        }
        protected virtual
            (
                bool Success
                , int StatusCode
                , string HttpMethod
                , string Message
                , string ConnectionString
                , string DataBaseType
                , string StoreProcedureName
                , int CommandTimeoutInSeconds
                , bool EnableStatistics
            )
            TryGetStoreProcedureInfo
                        (
                            string routeName
                            , string httpMethod
                        )
        {
            var success = true;
            var statusCode = 500;
            string message = "ok";
            var connectionString = string.Empty;
            var storeProcedureName = string.Empty;
            var dataBaseType = string.Empty;
            var commandTimeoutInSeconds = 120;
            
            var enableStatistics = false;
            (
                bool Result
                , int StatusCode
                , string HttpMethod
                , string Message
                , string ConnectionString
                , string DataBaseType
                , string StoreProcedureName
                , int CommandTimeoutInSeconds
                , bool EnableStatistics
            )
                Result()
            {
                return
                    (
                        success
                        , statusCode
                        , httpMethod
                        , message
                        , connectionString
                        , dataBaseType
                        , storeProcedureName
                        , commandTimeoutInSeconds
                        , enableStatistics
                    );
            }
            success = _configuration
                            .TryGetSection
                                    (
                                        $"Routes:{routeName}"
                                        , out var routeConfiguration
                                    );
            if (!success)
            {
                statusCode = 404;
                message = $"{routeName} not found";
                return
                    Result();
            }
            if
                (
                    !httpMethod
                            .StartsWith
                                (
                                    "http"
                                    , StringComparison
                                        .OrdinalIgnoreCase
                                )
                )
            {
                httpMethod = "http" + httpMethod;
            }
            success = routeConfiguration
                                    .TryGetSection
                                            (
                                                $"{httpMethod}"
                                                , out var actionConfiguration
                                            );
            if (!success)
            {
                //success = false;
                statusCode = 403;
                message = $"{httpMethod} verb forbidden";
                return
                    Result();
            }
            var connectionID = actionConfiguration
                                            .GetValue<string>
                                                    ("ConnectionID");
            success = !connectionID.IsNullOrEmptyOrWhiteSpace();
            if (!success)
            {
                statusCode = 500;
                message = $"Database connectionID error";
                return
                    Result();
            }
            _configuration
                        .TryGetSection
                            (
                                $"Connections:{connectionID}"
                                , out var connectionConfiguration
                            );
            connectionString = connectionConfiguration
                                            .GetValue<string>
                                                    ("ConnectionString");
            success = !connectionString.IsNullOrEmptyOrWhiteSpace();
            if (!success)
            {
                statusCode = 500;
                message = $"Database connection string error";
                return
                    Result();
            }
            dataBaseType = connectionConfiguration
                                    .GetValue<string>
                                            ("DataBaseType");
            success = !dataBaseType
                            .IsNullOrEmptyOrWhiteSpace();
            if (!success)
            {
                statusCode = 500;
                message = $"Database Type error";
                return
                    Result();
            }

            storeProcedureName = actionConfiguration
                                        .GetValue<string>
                                            ("StoreProcedureName");
            enableStatistics = connectionConfiguration
                                        .GetValue
                                            ("EnableStatistics", false);
            actionConfiguration
                        .TryGetSection
                            ("DefaultAccessing", out var accessingConfiguration);
            if (enableStatistics)
            {
                bool b;
                if 
                    (
                        actionConfiguration
                                    .TryGet<bool>("EnableStatistics", out b)
                                    
                    )
                {
                    enableStatistics = b;
                }
                else
                {
                    if 
                        (
                            accessingConfiguration
                                        .TryGet<bool>("EnableStatistics", out b)
                        )
                    {
                        enableStatistics = b;
                    }
                }
            }
            int i = 0;
            if 
                (
                    accessingConfiguration
                                    .TryGet<int>("CommandTimeoutInSeconds", out i)
                )
            {
                commandTimeoutInSeconds = i;
            }
            else
            {
                if
                    (
                        actionConfiguration
                                   .TryGet<int>("CommandTimeoutInSeconds", out i)
                    )
                {
                    commandTimeoutInSeconds = i;
                }
                else
                {
                    if 
                        (
                            connectionConfiguration
                                            .TryGet<int>("CommandTimeoutInSeconds", out i)
                        )
                    {
                        commandTimeoutInSeconds = i;
                    }
                }
            }
            success = !storeProcedureName.IsNullOrEmptyOrWhiteSpace();
            if (!success)
            {
                statusCode = 500;
                message = $"Database StoreProcedure Name error";
                return
                    Result();
            }
            //success = true;
            statusCode = 200;
            return
                Result();
        }

        public virtual void
                    ProcessReaderReadRows
                        (
                            string routeName
                            , JToken parameters = null
                            , Action
                                <
                                    int             // resultSetIndex
                                    , JArray        // columns
                                    , IDataReader
                                    , int           // row index
                                > onReadRowProcessAction = null
                            , string httpMethod = "Get"
                            //, bool enableStatistics = false
                            , int commandTimeoutInSeconds = 101
                        )
        {
            //JToken result = null;

            bool success;
            int statusCode;
            string message;
            string connectionString;
            string dataBaseType;
            string storeProcedureName;
            bool enableStatistics;

            (
                success
                , statusCode
                , httpMethod
                , message
                , connectionString
                , dataBaseType
                , storeProcedureName
                , commandTimeoutInSeconds
                , enableStatistics
            )
                = TryGetStoreProcedureInfo
                            (
                                routeName
                                , httpMethod
                            );


            if
               (
                   success
                   &&
                   statusCode == 200
               )
            {
                ProcessReaderReadRows
                    (
                        connectionString
                        , dataBaseType
                        , storeProcedureName
                        , parameters
                        , onReadRowProcessAction
                        , enableStatistics
                        , commandTimeoutInSeconds
                    );
            }
        }

        public virtual void
                    ProcessReaderReadRows
                            (
                                string connectionString
                                , string dataBaseType
                                , string storeProcedureName
                                , JToken parameters = null
                                , Action
                                    <
                                        int           // resultSet Index
                                        , JArray
                                        , IDataReader
                                        , int           // row index
                                    > onReadRowProcessAction = null
                                , bool enableStatistics = false
                                , int commandTimeoutInSeconds = 90
                            )
        {
            //var beginTimeStamp = Stopwatch.GetTimestamp();
            //var beginTime = DateTime.Now;
            var success = _indexedExecutors
                                        .TryGetValue
                                            (
                                                dataBaseType
                                                , out var executor
                                            );
            if (success)
            {
                executor
                    .ExecuteReaderProcess
                        (
                            connectionString
                            , storeProcedureName
                            , parameters
                            , onReadRowProcessAction
                            , enableStatistics
                            , commandTimeoutInSeconds
                        );
            }
        }
        public virtual async Task
            ProcessReaderReadRowsAsync
                (
                    string routeName
                    , JToken parameters = null
                    , Func
                        <
                            int             // resultSetIndex
                            , JArray        // columns    
                            , IDataReader
                            , int           // row index
                            , Task
                        > onReadRowProcessActionAsync = null
                    , string httpMethod = "Get"
                    //, bool enableStatistics = false
                    , int commandTimeoutInSeconds = 101
                )
        {
            bool success;
            int statusCode;
            string message;
            string connectionString;
            string dataBaseType;
            string storeProcedureName;
            bool enableStatistics;

            (
                success
                , statusCode
                , httpMethod
                , message
                , connectionString
                , dataBaseType
                , storeProcedureName
                , commandTimeoutInSeconds
                , enableStatistics
            )
                = TryGetStoreProcedureInfo
                            (
                                routeName
                                , httpMethod
                            );


            if
               (
                   success
                   &&
                   statusCode == 200
               )
            {
                await
                    ProcessReaderReadRowsAsync
                        (
                            connectionString
                            , dataBaseType
                            , storeProcedureName
                            , parameters
                            , onReadRowProcessActionAsync
                            , enableStatistics
                            , commandTimeoutInSeconds
                        );
            }
        }
        public virtual async Task
                    ProcessReaderReadRowsAsync
                            (
                                string connectionString
                                , string dataBaseType
                                , string storeProcedureName
                                , JToken parameters = null
                                , Func
                                    <
                                        int           // resultSet Index
                                        , JArray
                                        , IDataReader
                                        , int           // row index
                                        , Task
                                    > onReadRowProcessActionAsync = null
                                , bool enableStatistics = false
                                , int commandTimeoutInSeconds = 90
                            )
        {
            //var beginTimeStamp = Stopwatch.GetTimestamp();
            //var beginTime = DateTime.Now;
            var success = _indexedExecutors
                                        .TryGetValue
                                            (
                                                dataBaseType
                                                , out var executor
                                            );
            if (success)
            {
                
                await
                    executor
                        .ExecuteReaderProcessAsync
                            (
                                connectionString
                                , storeProcedureName
                                , parameters
                                , onReadRowProcessActionAsync
                                , enableStatistics
                                , commandTimeoutInSeconds
                            );
            }
        }
    }
}
#endif
