#if NETCOREAPP2_X
namespace Microshaoft.Web
{
    using Microshaoft;
    using Microsoft.Extensions.Configuration;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.IO;
    using System.Linq;
    using System.Reflection;
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
    }
    public abstract class
                AbstractStoreProceduresService
                                : IStoreProceduresWebApiService
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
        protected virtual string[] GetDynamicLoadExecutorsPathsProcess()
        {
            var result =
                    _configuration
                        .GetSection("DynamicLoadExecutorsPaths")
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
                            string dynamicLoadExecutorsPathsJsonFile = "dynamicLoadExecutorsPaths.json"
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
                    GetDynamicLoadExecutorsPathsProcess
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
                                    var r =
                                        CompositionHelper
                                            .ImportManyExportsComposeParts
                                                <IStoreProcedureExecutable>
                                                    (x);
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
            JToken result = null;
            var statusCode = 200;
            var message = string.Empty;
            var r1 = TryGetStoreProcedureInfo
                        (
                            routeName
                            , httpMethod
                        );
            if 
                (
                    r1.Success
                    &&
                    r1.StatusCode == 200
                )
            {
                var r2 = Process
                            (
                                r1.ConnectionString
                                , r1.DataBaseType
                                , r1.StoreProcedureName
                                , out result
                                , parameters
                                , onReadRowColumnProcessFunc
                                , r1.EnableStatistics
                                , r1.CommandTimeoutInSeconds
                            );

                var jObject = result
                                    ["Outputs"]
                                    ["Parameters"] as JObject;
                if (jObject != null)
                {
                    if
                        (
                            jObject
                                .TryGetValue
                                    (
                                        "HttpResponseStatusCode"
                                        , StringComparison
                                                .OrdinalIgnoreCase
                                        , out var jv
                                    )
                        )
                    {
                        statusCode = jv.Value<int>();
                    }
                }
                if (r2)
                {
                    //support custom output nest json by JSONPath in JsonFile Config
                    var outputsConfiguration =
                                _configuration
                                        .GetSection($"Routes:{routeName}:{r1.HttpMethod}:Outputs");
                    if (outputsConfiguration.Exists())
                    {
                        var mappings = 
                                    outputsConfiguration
                                        .GetChildren()
                                        .Select
                                            (
                                                (x) =>
                                                {
                                                    (
                                                        string TargetJPath
                                                        , string SourceJPath
                                                    )
                                                        r =
                                                            (
                                                                x.Key
                                                               , x.Get<string>()
                                                            );
                                                    return r;
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
            else
            {
                statusCode = r1.StatusCode;
                message = r1.Message;
            }
            return
                (
                    statusCode
                    , message
                    , result
                );
        }
        protected virtual bool
                Process
                        (
                            string connectionString
                            , string dataBaseType
                            , string storeProcedureName
                            , out JToken result
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
            var r = false;
            result = null;
            var beginTime = DateTime.Now;
            IStoreProcedureExecutable executor = null;
            r = _indexedExecutors
                        .TryGetValue
                            (
                                dataBaseType
                                , out executor
                            );
            if (r)
            {
                r = executor
                        .Execute
                            (
                                connectionString
                                , storeProcedureName
                                , out result
                                , parameters
                                , onReadRowColumnProcessFunc
                                , enableStatistics
                                , commandTimeoutInSeconds
                            );
            }
            if (!r)
            {
                result = null;
                return r;
            }
            result["BeginTime"] = beginTime;
            var endTime = DateTime.Now;
            result["EndTime"] = endTime;
            result["DurationInMilliseconds"]
                    = DateTimeHelper
                            .MillisecondsDiff
                                    (
                                        beginTime
                                        , endTime
                                    );
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