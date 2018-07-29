//#if !NETFRAMEWORK4_X && !NETSTANDARD2_0
namespace Microshaoft.WebApi.Controllers
{
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections.Generic;
    using System.Data.SqlClient;
    using System.Linq;
    //[Route("api/[controller]")]
    //[ApiController]
    public abstract partial class AbstractStoreProcedureExecutorControllerBase 
    {
        private static object _locker = new object();
        private bool CheckList(string x)
        {
            if (SqlHelper.StoreProceduresExecuteWhiteList == null)
            {
                lock (_locker)
                {
                    SqlHelper
                        .StoreProceduresExecuteWhiteList
                            = GetExecuteWhiteList();
                }
            }
            return
                SqlHelper
                        .StoreProceduresExecuteWhiteList
                        .Contains(x);
        }
        protected abstract string ConnectionString
        {
            get;
            //set;
        }

        public abstract HashSet<string> GetExecuteWhiteList();

        private bool Process(string storeProcedureName, JObject parameters, out JObject result)
        {

            result = null;
            if (!CheckList(storeProcedureName))
            {
                return false;
            }
           

            SqlConnection connection = new SqlConnection(ConnectionString);
            result = SqlHelper
                                .StoreProcedureExecute
                                        (
                                            connection
                                            , storeProcedureName
                                            , parameters
                                            , 90
                                        );
            return true;
        }
        private bool Process(string storeProcedureName, string parameters, out JObject result)
        {
            var j = JObject.Parse(parameters);
            var r = Process(storeProcedureName, j, out result);
            return r;
        }
        private void GroupingJObjectResult(int groupFrom, string groupBy, JObject result)
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
//#endif