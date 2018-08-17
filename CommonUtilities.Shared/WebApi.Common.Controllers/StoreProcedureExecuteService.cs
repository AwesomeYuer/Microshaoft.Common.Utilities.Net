#if NETCOREAPP2_X
namespace Microshaoft.Web
{
    using Microshaoft;
    using Microshaoft.Linq.Dynamic;
    using Microshaoft.WebApi.Controllers;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    public interface IStoreProceduresService
    {
        (int StatusCode, JToken Result)
                Process
                     (
                        string connectionID //= "mssql"
                        ,
                        string storeProcedureName
                        ,
                        JToken parameters = null
                        ,
                        string httpMethod = "Get"
                        ,
                        int commandTimeoutInSeconds = 101
                    );
    }
    public abstract class
                AbstractStoreProceduresService
                                : IStoreProceduresService
    {
        private object _locker = new object();
        public AbstractStoreProceduresService()
        {
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
                            _connections
                                = DataBasesConnectionsInfo
                                        .ToDictionary
                                            (
                                                (x) =>
                                                {
                                                    return
                                                        x.ConnectionID;
                                                }
                                                , StringComparer
                                                        .OrdinalIgnoreCase
                                            );
                        }
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
                            if (DynamicLoadExecutorsPaths != null)
                            {
                                var q =
                                    DynamicLoadExecutorsPaths
                                        .Where
                                            (
                                                (x) =>
                                                {
                                                    return
                                                        Directory
                                                            .Exists(x);
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
                                                                    (
                                                                        x
                                                                    );
                                                    return
                                                            r;
                                                }
                                            );
                                _executors =
                                    q
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
                                                IStoreProcedureParametersSetCacheAutoRefreshable
                                                    rr = x as IStoreProcedureParametersSetCacheAutoRefreshable;
                                                if (rr != null)
                                                {
                                                    rr.CachedExecutingParametersExpiredInSeconds
                                                        = CachedExecutingParametersExpiredInSeconds;
                                                    rr.NeedAutoRefreshExecutedTimeForSlideExpire
                                                        = NeedAutoRefreshExecutedTimeForSlideExpire;
                                                }
                                                return
                                                    x;
                                            }
                                            , StringComparer
                                                    .OrdinalIgnoreCase
                                        );
                            }
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
        protected abstract IEnumerable<DataBaseConnectionInfo> 
                    DataBasesConnectionsInfo
        {
            get;
        }
        private IDictionary<string, IStoreProcedureExecutable>
                    _executors;
        protected abstract string[]
                    DynamicLoadExecutorsPaths
        {
            get;
        }
        public 
            (int StatusCode, JToken Result)
                        Process
                            (
                                string connectionID //= "mssql"
                                ,
                                string storeProcedureName
                                ,
                                JToken parameters = null
                                ,
                                string httpMethod = "Get"
                                ,
                                int commandTimeoutInSeconds = 101
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