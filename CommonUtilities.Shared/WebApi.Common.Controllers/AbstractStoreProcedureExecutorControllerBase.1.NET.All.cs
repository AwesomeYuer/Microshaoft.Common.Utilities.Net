#if !XAMARIN
namespace Microshaoft.WebApi.Controllers
{
    using Microshaoft.Web;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    public enum DataBasesType
    {
        MsSQL ,
        MySQL
    }
    public class DataBaseConnectionInfo
    {
        public string ConnectionID;
        public DataBasesType DataBaseType;
        public string ConnectionString;
        public IDictionary<string, HttpMethodsFlags>
                            WhiteList;

    }
    public abstract partial class 
        AbstractStoreProcedureExecutorControllerBase 
            //: IStoreProcedureParametersSetCacheAutoRefreshable
    {
        


        private static IDictionary<string, IStoreProcedureExecutable> _executors;
        protected abstract string[] DynamicLoadExecutorsPaths
        {
            get;
            //set;
        }
        protected abstract int CachedExecutingParametersExpiredInSeconds
        {
            get;
            //set;
        }
        protected abstract bool NeedAutoRefreshExecutedTimeForSlideExpire
        {
            get;
            //set;
        }
        private static IDictionary<string, DataBaseConnectionInfo> _connections = null;
        protected abstract IEnumerable<DataBaseConnectionInfo> GetDataBasesConnectionsInfo();
       
        private static object _locker = new object();
        private bool CheckList
                (
                    IDictionary<string, HttpMethodsFlags> whiteList
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
                    r = allowedHttpMethodsFlags.HasFlag(httpMethodsFlag);
                }
            }
            return r;
        }
        private bool Process
                    (
                        DataBaseConnectionInfo connectionInfo
                        , string storeProcedureName
                        , string httpMethod
                        , JToken parameters
                        , out JToken result
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
                            );
            }
            return r;
        }
        private bool Process
                        (
                            string   connectionString
                            , string dataBaseType
                            , string storeProcedureName
                            , string parameters
                            , out JToken result
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
                        );
            return r;
        }
        private void GroupingJObjectResult(int groupFrom, string groupBy, JToken result)
        {
            var jTokenPath = $"Outputs.ResultSets[{groupFrom}]";
            var originalResultSet = result.SelectToken(jTokenPath);
            var compareJTokensPaths
                                = groupBy.Split(',');
            var groups = originalResultSet
                             .GroupBy
                                 (
                                     (x) =>
                                     {
                                         var r = new JObject();
                                         foreach (var s in compareJTokensPaths)
                                         {
                                             r.Add(s, x[s]);
                                         }
                                         return
                                                 r;
                                     }
                                     , new JObjectComparer()
                                     {
                                         CompareJTokensPaths = compareJTokensPaths
                                     }
                                 );
            var resultSet = groups
                             .Select
                                 (
                                     (x) =>
                                     {
                                         var r = new JObject();
                                         r.Merge(x.Key);
                                         r.Add("Details", new JArray(x));
                                         return r;
                                     }
                                 );
            result
                .SelectToken(jTokenPath)
                .Parent[groupFrom] = new JArray(resultSet);
        }
    }
}
#endif