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
    using System.Reflection;
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
        public AbstractStoreProceduresService()
        {
            Initialize();
        }
        //for override from derived class
        public virtual void Initialize()
        {
            LoadDataBasesConnectionsInfo("dbConnections.json");
            LoadDynamicExecutors("dynamicLoadExecutorsPaths.json");
        }
        protected IDictionary<string, DataBaseConnectionInfo>
                        GetDataBasesConnectionsInfoProcess
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
                                    var r = configuration[$"{x.Key}ConnectionID"];
                                    return r;
                                }
                                , (x) =>
                                {
                                    var allowExecuteWhiteList
                                        = configuration
                                            .GetSection($"{x.Key}WhiteList")
                                            .AsEnumerable()
                                            .Where
                                                (
                                                    (xx) =>
                                                    {
                                                        var v = xx.Value;
                                                        var rr = !v.IsNullOrEmptyOrWhiteSpace();
                                                        return rr;
                                                    }
                                                )
                                            .GroupBy
                                                (
                                                    (xx) =>
                                                    {
                                                        var key = xx.Key;
                                                        var i = key.FindIndex(":", 4);
                                                        var rr = key.Substring(0, i);
                                                        return rr;
                                                    }
                                                )
                                            .ToDictionary
                                                (
                                                    (xx) =>
                                                    {
                                                        var key = configuration[$"{xx.Key}StoreProcedureAlias"];
                                                        var storeProcedureName = configuration[$"{xx.Key}StoreProcedureName"];
                                                        if (key.IsNullOrEmptyOrWhiteSpace())
                                                        {
                                                            key = storeProcedureName;
                                                        }
                                                        return key;
                                                    }
                                                    ,
                                                    (xx) =>
                                                    {
                                                        var storeProcedureName = configuration[$"{xx.Key}StoreProcedureName"];
                                                        var s = configuration[$"{xx.Key}AllowedHttpMethods"];
                                                        var allowedHttpMethods =
                                                                    Enum
                                                                        .Parse<HttpMethodsFlags>
                                                                            (
                                                                                s
                                                                                , true
                                                                            );
                                                        var rr = new StoreProcedureInfo()
                                                        {
                                                            Alias = xx.Key
                                                            , Name = storeProcedureName
                                                            , AllowedHttpMethods = allowedHttpMethods
                                                        };
                                                        return
                                                            rr;
                                                    }
                                                    ,
                                                    StringComparer
                                                            .OrdinalIgnoreCase
                                                );
                                    var r = new DataBaseConnectionInfo()
                                    {
                                        ConnectionID = configuration[$"{x.Key}ConnectionID"]
                                        , ConnectionString = configuration[$"{x.Key}ConnectionString"]
                                        , DataBaseType = Enum.Parse<DataBasesType>(configuration[$"{x.Key}DataBaseType"], true)
                                        , AllowExecuteWhiteList = allowExecuteWhiteList
                                    };
                                    return r;
                                }
                                , StringComparer
                                        .OrdinalIgnoreCase
                            );
            return result;
        }
        protected virtual void LoadDataBasesConnectionsInfo
                                    (
                                        string dbConnectionsJsonFile
                                                    = "dbConnections.json"
                                    )
        {
            var connections = GetDataBasesConnectionsInfoProcess
                                    (dbConnectionsJsonFile);
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
        private class StoreProcedureComparer : IEqualityComparer<IStoreProcedureExecutable>
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
                                dynamicLoadExecutorsPathsJsonFile
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
                            )
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
                                            .NeedAutoRefreshParametersDefinitionCacheForSlideExpire
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
                    CachedParametersDefinitionExpiredInSeconds
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
                , string storeProcedureAliasOrName
                , string httpMethod
                , JToken parameters
                , out JToken result
                , int commandTimeoutInSeconds = 90
            )
        {
            var r = false;
            result = null;
            var allowExecuteWhiteList = connectionInfo.AllowExecuteWhiteList;
            var storeProcedureName = string.Empty;
            if (allowExecuteWhiteList != null)
            {
                if (allowExecuteWhiteList.Count > 0)
                {
                    r = CheckList
                            (
                                allowExecuteWhiteList
                                , storeProcedureAliasOrName
                                , httpMethod
                                , out StoreProcedureInfo storeProcedureInfo
                            );
                    if (r)
                    {
                        storeProcedureName = storeProcedureInfo.Name;
                    }
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
                        <string, StoreProcedureInfo>
                            allowExecuteWhiteList
                    , string storeProcedureAliasOrName
                    , string httpMethod
                    , out StoreProcedureInfo storeProcedureInfo
                )
        {
            var r = false;
            HttpMethodsFlags httpMethodsFlag;
            storeProcedureInfo = null;
            r = Enum
                    .TryParse<HttpMethodsFlags>
                        (
                            httpMethod
                            , true
                            , out httpMethodsFlag
                        );
            if (r)
            {
                r = allowExecuteWhiteList
                        .TryGetValue
                            (
                                storeProcedureAliasOrName
                                , out storeProcedureInfo
                            );
                if (r)
                {
                    r = storeProcedureInfo
                            .AllowedHttpMethods
                            .HasFlag(httpMethodsFlag);
                }
            }
            return r;
        }
    }
}
#endif