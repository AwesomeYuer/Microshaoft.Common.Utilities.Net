#if NETCOREAPP2_X
namespace Microshaoft.Web
{
    using Microshaoft;
    using Microshaoft.Linq.Dynamic;
    using Microshaoft.WebApi.Controllers;
    using Microsoft.Extensions.Configuration;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    public interface IStoreProceduresWebApiService
    {
        (int StatusCode, JToken Result)
                Process
                     (
                        string connectionID
                        , string storeProcedureName
                        , JToken parameters = null
                        , string httpMethod = "Get"
                        , int commandTimeoutInSeconds = 101
                    );
    }
    public abstract class
                AbstractStoreProceduresService
                                : IStoreProceduresWebApiService
    {
        private static object _locker = new object();

        protected IDictionary<string, DataBaseConnectionInfo>
                        GetDataBasesConfigurationProcess
                                    (
                                        string dbConnectionsJsonFile = "dbConnections.json"
                                    )
        {
            var configurationBuilder =
                        new ConfigurationBuilder()
                                .AddJsonFile(dbConnectionsJsonFile);
            var configuration = configurationBuilder.Build();
            var result =
                    configuration
                        .GetSection("Connections")
                        .AsEnumerable()
                        .Where
                            (
                                (x) =>
                                {
                                    return
                                        !x
                                            .Value
                                            .IsNullOrEmptyOrWhiteSpace();
                                }
                            )
                        .GroupBy
                            (
                                (x) =>
                                {
                                    var key = x.Key;
                                    var i = key.FindIndex(":", 2);
                                    var rr = key.Substring(0, i);
                                    return rr;
                                }
                            )
                        .ToDictionary
                            (
                                (x) =>
                                {
                                    var r = configuration[$"{x.Key}:ConnectionID"];
                                    return r;
                                }
                                , (x) =>
                                {
                                    var whiteList =
                                            configuration
                                                .GetSection($"{x.Key}:WhiteList")
                                                .AsEnumerable()
                                                .Where
                                                    (
                                                        (xx) =>
                                                        {
                                                            var v = xx.Value;
                                                            var rr = !v.IsNullOrEmptyOrWhiteSpace();
                                                            return
                                                                    rr;
                                                        }
                                                    )
                                                .GroupBy
                                                    (
                                                        (xx) =>
                                                        {
                                                            var i = xx.Key.FindIndex(":", 4);
                                                            var rr = xx.Key.Substring(0, i);
                                                            return rr;
                                                        }
                                                    )
                                                .ToDictionary
                                                    (
                                                        (xx) =>
                                                        {
                                                            var rr = configuration[$"{xx.Key}:StoreProcedureName"];
                                                            return rr;
                                                        }
                                                        ,
                                                        (xx) =>
                                                        {
                                                            var s = configuration[$"{xx.Key}:AllowedHttpMethods"];
                                                            var allowedHttpMethods =
                                                                        Enum
                                                                            .Parse<HttpMethodsFlags>
                                                                                (
                                                                                    s
                                                                                    , true
                                                                                );
                                                            return
                                                                allowedHttpMethods;
                                                        }
                                                        ,
                                                        StringComparer.OrdinalIgnoreCase
                                                    );
                                    var r = new DataBaseConnectionInfo()
                                    {
                                        ConnectionID = configuration[$"{x.Key}:ConnectionID"]
                                        , ConnectionString = configuration[$"{x.Key}:ConnectionString"]
                                        , DataBaseType = Enum.Parse<DataBasesType>(configuration[$"{x.Key}:DataBaseType"], true)
                                        , WhiteList = whiteList
                                    };
                                    return r;
                                }
                                ,
                                StringComparer
                                    .OrdinalIgnoreCase
                            );
            return result;
        }
        protected virtual void LoadDataBasesConfiguration
                                    (
                                        string dbConnectionsJsonFile
                                                    = "dbConnections.json"
                                    )
        {
            var connections = GetDataBasesConfigurationProcess(dbConnectionsJsonFile);
            _locker
                .LockIf
                    (
                        () =>
                        {
                            var r = (_connections == null);
                            return r;
                        }
                        , () =>
                        {
                            _connections = connections;
                        }
                    );
        }
        protected virtual string[] GetDynamicLoadExecutorsPathsProcess
                    (
                        string dynamicLoadExecutorsPathsJsonFile
                                    = "dynamicLoadExecutorsPaths.json"
                    )
        {
            var configurationBuilder =
                        new ConfigurationBuilder()
                                .AddJsonFile(dynamicLoadExecutorsPathsJsonFile);
            var configuration = configurationBuilder.Build();
            var result =
                    configuration
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
            var executors =
                    GetDynamicLoadExecutorsPathsProcess
                            (
                                dynamicLoadExecutorsPathsJsonFile
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
                                    ICacheAutoRefreshable
                                        rr = x as ICacheAutoRefreshable;
                                    if (rr != null)
                                    {
                                        rr
                                            .CachedExpiredInSeconds
                                                = CachedExecutingParametersExpiredInSeconds;
                                        rr
                                            .NeedAutoRefreshForSlideExpire
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
                            var r = (_executors == null);
                            return r;
                        }
                        , () =>
                        {
                            _executors = executors;
                        }
                    );
        }
        protected abstract int 
                    CachedExecutingParametersExpiredInSeconds
        {
            get;
            //set;
        }
        protected abstract bool 
                    NeedAutoRefreshExecutedTimeForSlideExpire
        {
            get;
            //set;
        }

        private IDictionary<string, DataBaseConnectionInfo> 
                    _connections;

        private IDictionary<string, IStoreProcedureExecutable>
                    _executors;

        public 
            (int StatusCode, JToken Result)
                        Process
                            (
                                string connectionID //= "mssql"
                                , string storeProcedureName
                                , JToken parameters = null
                                , string httpMethod = "Get"
                                , int commandTimeoutInSeconds = 101
                            )
        {
            var beginTime = DateTime.Now;
            JToken result = null;
            var r = false;
            int statusCode = 200;
            DataBaseConnectionInfo connectionInfo = null;
            r = _connections
                        .TryGetValue
                            (
                                connectionID
                                , out connectionInfo
                            );
            if (r)
            {
                r = Process
                        (
                            connectionInfo
                            , storeProcedureName
                            , httpMethod
                            , parameters
                            , out result
                            , commandTimeoutInSeconds
                        );
            }
            if (!r)
            {
                statusCode = 403;
                result = null;
                return (statusCode, result);
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
            return (statusCode, result); 
        }
        private bool Process
            (
                DataBaseConnectionInfo connectionInfo
                , string storeProcedureName
                , string httpMethod
                , JToken parameters
                , out JToken result
                , int commandTimeoutInSeconds = 90
            )
        {
            var r = false;
            result = null;
            var whiteList = connectionInfo.WhiteList;
            if (whiteList != null)
            {
                if (whiteList.Count > 0)
                {
                    r = CheckList
                            (
                                whiteList
                                , storeProcedureName
                                , httpMethod
                            );
                }
            }
            else
            {
                r = true;
            }
            if (r)
            {
                r = Process
                        (
                            connectionInfo.ConnectionString
                            , connectionInfo.DataBaseType.ToString()
                            , storeProcedureName
                            , parameters
                            , out result
                            , commandTimeoutInSeconds
                        );
            }
            return r;
        }
        private bool Process
                        (
                            string connectionString
                            , string dataBaseType
                            , string storeProcedureName
                            , JToken parameters
                            , out JToken result
                            , int commandTimeoutInSeconds = 90
                        )
        {
            var r = false;
            result = null;
            IStoreProcedureExecutable executor = null;
            r = _executors
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
                                , parameters
                                , out result
                                , commandTimeoutInSeconds
                            );
            }
            return r;
        }
        private bool Process
                        (
                            string connectionString
                            , string dataBaseType
                            , string storeProcedureName
                            , string parameters
                            , out JToken result
                            , int commandTimeoutInSeconds = 90
                        )
        {
            var j = JObject.Parse(parameters);
            var r = Process
                        (
                            connectionString
                            , dataBaseType
                            , storeProcedureName
                            , j
                            , out result
                            , commandTimeoutInSeconds
                        );
            return r;
        }
        private bool CheckList
                (
                    IDictionary
                        <string, HttpMethodsFlags>
                            whiteList
                    , string storeProcedureName
                    , string httpMethod
                )
        {
            var r = false;
            HttpMethodsFlags httpMethodsFlag;
            r = Enum
                    .TryParse<HttpMethodsFlags>
                        (
                            httpMethod
                            , true
                            , out httpMethodsFlag
                        );
            if (r)
            {
                HttpMethodsFlags allowedHttpMethodsFlags;
                r = whiteList
                        .TryGetValue
                            (
                                storeProcedureName
                                , out allowedHttpMethodsFlags
                            );
                if (r)
                {
                    r = allowedHttpMethodsFlags
                            .HasFlag(httpMethodsFlag);
                }
            }
            return r;
        }
    }
}
#endif