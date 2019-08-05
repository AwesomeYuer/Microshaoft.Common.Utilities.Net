#if NETCOREAPP2_X
namespace Microshaoft.Web
{
    using Microshaoft;
    using Microsoft.Extensions.Configuration;
    using Newtonsoft.Json.Linq;
    using System;
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
        )
               Process
                       (
                           string connectionString
                           , string dataBaseType
                           , string storeProcedureName
                           , JToken parameters = null
                           , Func
                               <
                                   IDataReader
                                   , Type        // fieldType
                                   , string    // fieldName
                                   , int       // row index
                                   , int       // column index
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
                                   IDataReader
                                   , Type        // fieldType
                                   , string    // fieldName
                                   , int       // row index
                                   , int       // column index
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
        )
            Process
                (
                    string routeName
                    , JToken parameters = null
                    , Func
                            <
                                IDataReader
                                , Type          // fieldType
                                , string        // fieldName
                                , int           // row index
                                , int           // column index
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
                )
            >
                ProcessAsync
                    (
                        string routeName
                        , JToken parameters = null
                        , Func
                                <
                                    IDataReader
                                    , Type          // fieldType
                                    , string        // fieldName
                                    , int           // row index
                                    , int           // column index
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

    public abstract class
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

        public AbstractStoreProceduresService(IConfiguration configuration)
        {
            _configuration = configuration;
            Initialize();
        }
        //for override from derived class
        public virtual void Initialize()
        {
            _cachedParametersDefinitionExpiredInSeconds =
                _configuration
                        .GetValue<int>
                            ("CachedParametersDefinitionExpiredInSeconds");
            _needAutoRefreshExecutedTimeForSlideExpire =
                _configuration
                        .GetValue<bool>
                            ("NeedAutoRefreshExecutedTimeForSlideExpire");
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
            return result;
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
                                                            , "*StoreProcedure*Plugin*.dll"
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
                                    return x;
                                }
                                , StringComparer
                                        .OrdinalIgnoreCase
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
        public IDictionary<string, IStoreProcedureExecutable> IndexedExecutors
        {
            get => _indexedExecutors;
            set => _indexedExecutors = value;
        }

        private IDictionary<string, IStoreProcedureExecutable>
                                _indexedExecutors;
        public
            (
                int StatusCode
                , string Message
                , JToken Result
            )
                    Process
                        (
                            string routeName
                            , JToken parameters = null
                            , Func
                                <
                                    IDataReader
                                    , Type          // fieldType
                                    , string        // fieldName
                                    , int           // row index
                                    , int           // column index
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
                (success, result) = Process
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
                );
        }

        public async
            Task
                <
                    (
                        int StatusCode
                        , string Message
                        , JToken Result
                    )
                >
            ProcessAsync
                (
                    string routeName
                    , JToken parameters = null
                    , Func
                        <
                            IDataReader
                            , Type          // fieldType
                            , string        // fieldName
                            , int           // row index
                            , int           // column index
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

                (success, result) = await
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
                )
                    Process
                            (
                                string connectionString
                                , string dataBaseType
                                , string storeProcedureName
                                , JToken parameters = null
                                , Func
                                    <
                                        IDataReader
                                        , Type          // fieldType
                                        , string        // fieldName
                                        , int           // row index
                                        , int           // column index
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
            if (success)
            {
                (success, result) = executor
                                            .Execute
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
                        );
                }
            }
            return
                    (
                        success
                        , result
                    );
        }

        private static void ResultTimingProcess
                (
                    JToken result
                    , DateTime beginTime
                    , long beginTimeStamp
                )
        {
            result["BeginTime"] = beginTime;
            result["EndTime"] = DateTime.Now;
            result["DurationInMilliseconds"]
                = beginTimeStamp
                        .GetNowElapsedTime()
                        .TotalMilliseconds;
        }

        public virtual async
                    Task
                        <
                            (
                                bool Success
                                , JToken Result
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
                            IDataReader
                            , Type          // fieldType
                            , string        // fieldName
                            , int           // row index
                            , int           // column index
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
            if (success)
            {
                (success, result) = await
                                        executor
                                            .ExecuteAsync
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
                        );
                }
            }
            return
                    (
                        success
                        , result
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
            var routeConfiguration = _configuration
                                            .GetSection($"Routes:{routeName}");
            if (!routeConfiguration.Exists())
            {
                success = false;
                statusCode = 404;
                message = $"{routeName} not found";
            }
            if (!success)
            {
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
            var actionConfiguration = routeConfiguration
                                            .GetSection($"{httpMethod}");
            if (!actionConfiguration.Exists())
            {
                success = false;
                statusCode = 403;
                message = $"{httpMethod} verb forbidden";
            }
            if (!success)
            {
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
            var connectionConfiguration =
                                _configuration
                                        .GetSection
                                            ($"Connections:{connectionID}");
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
                                        .GetValue<bool>
                                            ("EnableStatistics");
            var accessingConfiguration = actionConfiguration
                                            .GetSection
                                                ("DefaultAccessing");
            if (enableStatistics)
            {
                if 
                    (
                        actionConfiguration
                                    .GetSection("EnableStatistics")
                                    .Exists()
                    )
                {
                    enableStatistics = actionConfiguration
                                                .GetValue<bool>
                                                    ("EnableStatistics");
                }
                else
                {
                    if 
                        (
                            accessingConfiguration
                                        .GetSection("EnableStatistics")
                                        .Exists()
                        )
                    {
                        enableStatistics = accessingConfiguration
                                                        .GetValue<bool>
                                                            ("EnableStatistics");
                    }
                }
            }
            if 
                (
                    accessingConfiguration
                                    .GetSection("CommandTimeoutInSeconds")
                                    .Exists()
                )
            {
                commandTimeoutInSeconds = accessingConfiguration
                                                .GetValue<int>
                                                    ("CommandtimeoutInSeconds");
            }
            else
            {
                if
                    (
                        actionConfiguration
                                    .GetSection("CommandTimeoutInSeconds")
                                    .Exists()
                    )
                {
                    commandTimeoutInSeconds = actionConfiguration
                                                        .GetValue<int>
                                                            ("CommandtimeoutInSeconds");
                }
                else
                {
                    if 
                        (
                            connectionConfiguration
                                            .GetSection("CommandTimeoutInSeconds")
                                            .Exists()
                        )
                    {
                        commandTimeoutInSeconds = connectionConfiguration
                                                                .GetValue<int>
                                                                    ("CommandTimeoutInSeconds");
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
    }
}
#endif
