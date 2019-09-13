#if !XAMARIN
namespace Microshaoft.WebApi.Controllers
{
    using Microshaoft.Web;
    using Newtonsoft.Json.Linq;
    using System.Collections.Generic;
    using System.Linq;
    public enum DataBasesType
    {
        UnKnown ,
        MsSQL ,
        MySQL ,
        NpgSQL ,
        Oracle ,
        Sqlite
    }
    public abstract partial class 
            AbstractStoreProceduresExecutorControllerBase 
    {
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