#if !NETFRAMEWORK4_X && !NETSTANDARD2_0

namespace Microshaoft.WebApi.Controllers
{
    using Microshaoft;
    using Microsoft.AspNetCore.Mvc;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    [Route("api/[controller]")]
    [ApiController]
    public class StoreProcedureExecutorController : AbstractStoreProcedureExecutorControllerBase //ControllerBase //, IConnectionString
    {
        protected override string 
                ConnectionString =>
                        @"Integrated Security=SSPI;Persist Security Info=False;Initial Catalog=D:\mssql\MSSQL13.LocalDB\LocalDB\TransportionSecrets\TransportionSecrets.mdf;Data Source=(localdb)\mssqllocaldb;";

        //[Route("{storeProcedureName}/{groupFrom?}/{groupBy?}")]
        //public ActionResult<JObject> Get
        //                       (
        //                           string storeProcedureName
        //                           , 
        //                           int groupFrom         = 0
        //                           ,
        //                           string groupBy            = null
        //                           ,
        //                            [FromQuery]
        //                            string p            = null //string.Empty
        //                       )
        //{
        //    /*
        //     https://localhost:44306/api/StoreProcedureExecutor/zsp_GetDatesAfter/groupby?g=%E5%91%A8%E5%87%A0,%E5%BD%93%E6%97%A5%E6%97%B6%E6%AE%B5&p=%7Bbegindate:%222018-01-01%22,AmountMax:10%7D
        //     */
        //    var originalResult = base.Get(storeProcedureName, p).Value;
        //    var jTokenPath = $"Outputs.ResultSets[{groupFrom}]";
        //    var originalResultSet = originalResult.SelectToken(jTokenPath);
        //    var compareJTokensPaths
        //                        = groupBy.Split(',');
        //    var groups = originalResultSet
        //                     .GroupBy
        //                         (
        //                             (x) =>
        //                             {
        //                                 var r = new JObject();
        //                                 foreach (var s in compareJTokensPaths)
        //                                 {
        //                                     r.Add(s, x[s]);
        //                                 }
        //                                 return
        //                                         r;
        //                             }
        //                             , new JObjectComparer()
        //                             {
        //                                  CompareJTokensPaths = compareJTokensPaths
        //                             }
        //                         );
        //    var resultSet = groups
        //                     .Select
        //                         (
        //                             (x) =>
        //                             {
        //                                 var r = new JObject();
        //                                 r.Merge(x.Key);
        //                                 r.Add("Details", new JArray(x));
        //                                 return r;
        //                             }
        //                         );
        //    originalResult
        //        .SelectToken(jTokenPath)
        //        .Parent[groupFrom] = new JArray(resultSet);
        //    return originalResult;
        //}
    }
}
#endif
