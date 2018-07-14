//#if !NETFRAMEWORK4_X && !NETSTANDARD2_0



namespace Microshaoft.WebApi.Controllers
{
    using Newtonsoft.Json.Linq;
    using System.Linq;
    //[Route("api/[controller]")]
    //[ApiController]
    public abstract partial class AbstractStoreProcedureExecutorControllerBase 
    {
        protected abstract string ConnectionString
        {
            get;
            //set;
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