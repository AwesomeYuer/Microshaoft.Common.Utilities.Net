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
        (bool Success, JToken Result)
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
        Task<(bool Success, JToken Result)>
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
                    , string httpMethod = "Get"
                    //, bool enableStatistics = false
                    , int commandTimeoutInSeconds = 101
                );

    }

    public abstract class
                AbstractStoreProceduresService
                                : IStoreProceduresWebApiService , IStoreProceduresService
    {
        private class StoreProcedureComparer
                        : IEqualityComparer<IStoreProcedureExecutable>
        {
            public bool Equals
                            (
                                IStoreProcedureExecutable x
                                , IStoreProcedureExecutable y
                            )
            {
                return
                    (x.DataBaseType == y.DataBaseType);
            }
            public int GetHashCode(IStoreProcedureExecutable obj)
            {
                return -1;
            }
        }
        private static object _locker = new object();
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
                        .GetValue<int>("CachedParametersDefinitionExpiredInSeconds");
            _needAutoRefreshExecutedTimeForSlideExpire =
                _configuration
                        .GetValue<bool>("NeedAutoRefreshExecutedTimeForSlideExpire");
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
                                    var r = CompositionHelper
                                                .ImportManyExportsComposeParts
                                                    <IStoreProcedureExecutable>
                                                        (x, "*StoreProcedure*plugin*.dll");
                                    return r;
                                }
                            );
            var indexedExecutors =
                    executors
                        .Distinct
                            (
                                 new StoreProcedureComparer()
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
                                    IParametersDefinitionCacheAutoRefreshable
                                        rr = x as IParametersDefinitionCacheAutoRefreshable;
                                    if (rr != null)
                                    {
                                        rr
                                            .CachedParametersDefinitionExpiredInSeconds
                                                = CachedParametersDefinitionExpiredInSeconds;
                                        rr
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
                            var r = (_indexedExecutors == null);
                            return r;
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
                            , string httpMethod = "Get"
                            //, bool enableStatistics = false
                            , int commandTimeoutInSeconds = 101
                        )
        {
            (
                int StatusCode
                , string Message
                , JToken Result
            ) r = (StatusCode: 200, Message: string.Empty, Result: null);
            JToken result = null;
            var statusCode = 200;
            var message = string.Empty;
            var has = TryGetStoreProcedureInfo
                        (
                            routeName
                            , httpMethod
                        );
            if 
                (
                    has.Success
                    &&
                    has.StatusCode == 200
                )
            {
                
                var rr = Process
                        (
                            has.ConnectionString
                            , has.DataBaseType
                            , has.StoreProcedureName
                            , parameters
                            , onReadRowColumnProcessFunc
                            , has.EnableStatistics
                            , has.CommandTimeoutInSeconds
                        );
                result = rr.Result;
                var success = rr.Success;

                AfterProcess(routeName, ref result, ref statusCode, ref message, has, success);
            }
            else
            {
                statusCode = has.StatusCode;
                message = has.Message;
            }
            return
                (
                    statusCode
                    , message
                    , result
                );
        }

        public async
            Task<(int StatusCode, string Message, JToken Result)>
            ProcessAsync
                (
                    string routeName
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
                    , string httpMethod = "Get"
                    //, bool enableStatistics = false
                    , int commandTimeoutInSeconds = 101
                )
        {
            (
                int StatusCode
                , string Message
                , JToken Result
            ) r = (StatusCode: 200, Message: string.Empty, Result: null);
            JToken result = null;
            var statusCode = 200;
            var message = string.Empty;
            var has = TryGetStoreProcedureInfo
                        (
                            routeName
                            , httpMethod
                        );
            if
                (
                    has.Success
                    &&
                    has.StatusCode == 200
                )
            {
                var rr = await ProcessAsync
                                    (
                                        has.ConnectionString
                                        , has.DataBaseType
                                        , has.StoreProcedureName
                                        , parameters
                                        , onReadRowColumnProcessFunc
                                        , has.EnableStatistics
                                        , has.CommandTimeoutInSeconds
                                    );
                result = rr.Result;
                var success = rr.Success;
                AfterProcess(routeName, ref result, ref statusCode, ref message, has, success);
            }
            else
            {
                statusCode = has.StatusCode;
                message = has.Message;
            }
            return
                (
                    statusCode
                    , message
                    , result
                );
        }

        private void AfterProcess
                        (
                            string routeName
                            , ref JToken result
                            , ref int statusCode
                            , ref string message
                            , 
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
                                ) has
                            , bool success
                        )
        {
            var jObject = result
                            ["Outputs"]
                            ["Parameters"] as JObject;
            if (jObject != null)
            {
                JToken jv = null;
                if
                    (
                        jObject
                            .TryGetValue
                                (
                                    "HttpResponseStatusCode"
                                    , StringComparison
                                            .OrdinalIgnoreCase
                                    , out jv
                                )
                    )
                {
                    statusCode = jv.Value<int>();
                }
                jv = null;
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
            if (success)
            {
                //support custom output nest json by JSONPath in JsonFile Config
                var outputsConfiguration = _configuration
                                                .GetSection
                                                    ($"Routes:{routeName}:{has.HttpMethod}:Outputs");
                if (outputsConfiguration.Exists())
                {
                    var mappings = outputsConfiguration
                                        .GetChildren()
                                        .Select
                                            (
                                                (x) =>
                                                {
                                                    (
                                                        string TargetJPath
                                                        , string SourceJPath
                                                    )
                                                        rrr =
                                                            (
                                                                x.Key
                                                               , x.Get<string>()
                                                            );
                                                    return rrr;
                                                }
                                            );
                    result = result
                                .MapToNew
                                    (
                                        mappings
                                    );
                }
            }
        }

        public virtual (bool Success, JToken Result)
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
                        )
        {
            (bool Success, JToken Result) r = (Success: false, Result: null);

            JToken result = null;
            var beginTimeStamp = Stopwatch.GetTimestamp();
            var beginTime = DateTime.Now;
            var success = _indexedExecutors
                                .TryGetValue
                                    (
                                        dataBaseType
                                        , out var executor
                                    );
            if (success)
            {
                r = executor
                            .Execute
                                (
                                    connectionString
                                    , storeProcedureName
                                    , parameters
                                    , onReadRowColumnProcessFunc
                                    , enableStatistics
                                    , commandTimeoutInSeconds
                                );
                result = r.Result;
            }
            if (!success)
            {
                //result = null;
                return r;
            }
            AfterExecute(result, beginTime, beginTimeStamp);
            return r;
        }

        private static void AfterExecute
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

        public virtual async Task<(bool Success, JToken Result)>
            ProcessAsync
                (
                    string connectionString
                    , string dataBaseType
                    , string storeProcedureName
                    //, out JToken result
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
                )
        {
            (bool Success, JToken Result) r = (Success : false, Result : null);
            
            JToken result = null;
            
            var beginTimeStamp = Stopwatch.GetTimestamp();
            var beginTime = DateTime.Now;
            var success = _indexedExecutors
                                .TryGetValue
                                    (
                                        dataBaseType
                                        , out var executor
                                    );
            if (success)
            {
                r = await executor
                            .ExecuteAsync
                                (
                                    connectionString
                                    , storeProcedureName
                                    , parameters
                                    , onReadRowColumnProcessFunc
                                    , enableStatistics
                                    , commandTimeoutInSeconds
                                );
                result = r.Result;
            }
            if (!success)
            {
                //result = null;
                return r;
            }
            AfterExecute(result, beginTime, beginTimeStamp);
            return r;
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
                return Result();
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
                return Result();
            }
            var connectionID = actionConfiguration
                                    .GetValue<string>("ConnectionID");
            success = !connectionID.IsNullOrEmptyOrWhiteSpace();
            if (!success)
            {
                statusCode = 500;
                message = $"Database connectionID error";
                return Result();
            }
            var connectionConfiguration =
                                _configuration
                                        .GetSection($"Connections:{connectionID}");
            connectionString = connectionConfiguration
                                        .GetValue<string>("ConnectionString");
            success = !connectionString.IsNullOrEmptyOrWhiteSpace();
            if (!success)
            {
                statusCode = 500;
                message = $"Database connection string error";
                return Result();
            }
            dataBaseType = connectionConfiguration
                                    .GetValue<string>("DataBaseType");
            if (connectionConfiguration.GetSection("CommandTimeoutInSeconds").Exists())
            {
                commandTimeoutInSeconds = connectionConfiguration.GetValue<int>("CommandTimeoutInSeconds");
            }
            success = !dataBaseType.IsNullOrEmptyOrWhiteSpace();
            if (!success)
            {
                statusCode = 500;
                message = $"Database Type error";
                return Result();
            }
            storeProcedureName = actionConfiguration
                                        .GetValue<string>("StoreProcedureName");
            enableStatistics = connectionConfiguration
                                        .GetValue<bool>("EnableStatistics");
            if (enableStatistics)
            {
                if (actionConfiguration.GetSection("EnableStatistics").Exists())
                {
                    enableStatistics = actionConfiguration
                                            .GetValue<bool>("EnableStatistics");
                }
            }
            if (actionConfiguration.GetSection("CommandTimeoutInSeconds").Exists())
            {
                commandTimeoutInSeconds = actionConfiguration
                                                .GetValue<int>("CommandtimeoutInSeconds");
            }
            success = !storeProcedureName.IsNullOrEmptyOrWhiteSpace();
            if (!success)
            {
                statusCode = 500;
                message = $"Database StoreProcedure Name error";
                return Result();
            }
            //success = true;
            statusCode = 200;
            return Result();
        }


    }
}
#endif
