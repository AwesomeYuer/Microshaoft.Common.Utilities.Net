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
                                    , JProperty   //  JObject Field 对象
                                > onReadRowColumnProcessFunc = null
                        , string httpMethod = "Get"
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
            (int StatusCode, JToken Result)
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
                                        , JProperty   //  JObject Field 对象
                                    > onReadRowColumnProcessFunc = null
                                , string httpMethod = "Get"
                                , int commandTimeoutInSeconds = 101
                            )
        {
            var r = false;
            JToken result = null;
            var statusCode = 500;
            var r1 = TryGetStoreProcedureInfo
                        (
                            routeName
                            , httpMethod

                        );

            if (r1.Success && r1.StatusCode == 200)
            {
                var r2 = Process
                            (
                                r1.ConnectionString
                                , r1.DataBaseType
                                , r1.StoreProcedureName
                                , out result
                                , parameters
                                , onReadRowColumnProcessFunc
                                , commandTimeoutInSeconds
                            );
                if (r2)
                {
                    statusCode = 200;
                }
            }
            else
            {
                statusCode = r1.StatusCode;
            }
            return
                (
                    statusCode
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
                                    , JProperty   //  JObject Field 对象
                                > onReadRowColumnProcessFunc = null
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
                , string ConnectionString
                , string DataBaseType
                , string StoreProcedureName
            )
            TryGetStoreProcedureInfo
                        (
                            string routeName
                            , string httpMethod
                        )
        {
            var success = false;
            var statusCode = 500;
            var connectionString = string.Empty;
            var storeProcedureName = string.Empty;
            var dataBaseType = string.Empty;
            (
                bool Result
                , int StatusCode
                , string ConnectionString
                , string DataBaseType
                , string StoreProcedureName
            )
            Result()
            {
                return
                    (
                        success
                        , statusCode
                        , connectionString
                        , dataBaseType
                        , storeProcedureName
                    );
            }
            IConfigurationSection configurationSection = null;
            try
            {
                configurationSection =
                    _configuration
                                .GetSection("Routes")
                                //Ignore Case
                                .GetChildren()
                                .First
                                    (
                                        (x) =>
                                        {
                                            return
                                                (
                                                    string
                                                        .Compare
                                                            (
                                                                x.Key
                                                                , routeName
                                                                , true
                                                            )
                                                    ==
                                                    0
                                                );
                                        }
                                    );
                success = true;
            }
            catch// (Exception)
            {
                success = false;
                statusCode = 404;
            }
            if (!success)
            {
                return Result();
            }

            try
            {
                configurationSection =
                        configurationSection
                            .GetChildren()
                            .First
                                (
                                    (x) =>
                                    {
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
                                        return
                                            (
                                                string
                                                    .Compare
                                                        (
                                                            x.Key
                                                            , httpMethod
                                                            , true
                                                        )
                                                ==
                                                0
                                            );
                                    }
                                );
                success = true;
            }
            catch// (Exception)
            {
                success = false;
                statusCode = 403;
            }
            if (!success)
            {
                return Result();
            }
            var connectionID = configurationSection
                                    .GetValue<string>("ConnectionID");
            success = !connectionID.IsNullOrEmptyOrWhiteSpace();
            if (!success)
            {
                statusCode = 500;
                return Result();
            }
            connectionString = _configuration
                                    .GetSection("Connections")
                                    .GetSection(connectionID)
                                    .GetValue<string>("ConnectionString");
            success = !connectionString.IsNullOrEmptyOrWhiteSpace();
            if (!success)
            {
                statusCode = 500;
                return Result();
            }
            dataBaseType = _configuration
                                    .GetSection("Connections")
                                    .GetSection(connectionID)
                                    .GetValue<string>("DataBaseType");
            success = !dataBaseType.IsNullOrEmptyOrWhiteSpace();
            if (!success)
            {
                statusCode = 500;
                return Result();
            }
            success = !connectionString.IsNullOrEmptyOrWhiteSpace();
            if (!success)
            {
                statusCode = 500;
                return Result();
            }
            storeProcedureName = configurationSection
                                        .GetValue<string>("StoreProcedureName");
            success = !storeProcedureName.IsNullOrEmptyOrWhiteSpace();
            if (!success)
            {
                statusCode = 500;
                return Result();
            }
            success = true;
            statusCode = 200;
            return Result();
        }
    }
}
#endif