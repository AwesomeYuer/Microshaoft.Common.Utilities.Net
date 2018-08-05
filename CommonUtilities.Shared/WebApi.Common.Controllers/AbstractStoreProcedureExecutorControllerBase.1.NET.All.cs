namespace Microshaoft.WebApi.Controllers
{
    using Microshaoft.Web;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections.Generic;
    using System.Data.SqlClient;
    using System.Linq;
    //[Route("api/[controller]")]
    //[ApiController]
    public abstract partial class AbstractStoreProcedureExecutorControllerBase 
    {

        public AbstractStoreProcedureExecutorControllerBase()
        {
            SqlHelper
                .CachedExecutingParametersExpiredInSeconds = CachedExecutingParametersExpiredInSeconds;
            //_EnableCorsPolicyName = EnableCorsPolicyName;
        }

        //public static string _EnableCorsPolicyName;

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

        //protected abstract string EnableCorsPolicyName
        //{
        //    get;
        //    //set;
        //}

        protected abstract string ConnectionString
        {
            get;
            //set;
        }
        protected abstract bool NeedCheckWhiteList
        {
            get;
            //set;
        }

        private static
            IDictionary<string, HttpMethodsFlags>
                            _whiteList = null;
        private static object _locker = new object();
        private bool CheckList(string storeProcedureName, string httpMethod)
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
                
                if (_whiteList == null)
                {
                    lock (_locker)
                    {
                        _whiteList = GetExecuteWhiteList();
                    }
                }
                HttpMethodsFlags allowedHttpMethodsFlags;
                r = _whiteList
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
        

        public abstract IDictionary<string, HttpMethodsFlags> GetExecuteWhiteList();

        private bool Process(string storeProcedureName, JToken parameters, out JToken result)
        {
            result = null;
            SqlConnection connection = new SqlConnection(ConnectionString);
            result = SqlHelper
                            .StoreProcedureExecute
                                    (
                                        connection
                                        , storeProcedureName
                                        , parameters
                                        , 90
                                    );
            if (NeedAutoRefreshExecutedTimeForSlideExpire)
            {
                SqlHelper
                    .RefreshCachedStoreProcedureExecuted
                        (
                            connection
                            , storeProcedureName
                        );
            }
            return true;
        }
        private bool Process(string storeProcedureName, string parameters, out JToken result)
        {
            var j = JObject.Parse(parameters);
            var r = Process(storeProcedureName, j, out result);
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
