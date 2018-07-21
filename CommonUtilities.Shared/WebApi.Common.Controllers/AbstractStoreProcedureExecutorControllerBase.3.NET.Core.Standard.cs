#if !NETFRAMEWORK4_X && NETCOREAPP2_X

namespace Microshaoft.WebApi.Controllers
{
    using Microshaoft;
    using Microsoft.AspNetCore.Mvc;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Data.SqlClient;
    using System.Linq;

    [Route("api/[controller]")]
    [ApiController]
    public abstract partial class AbstractStoreProcedureExecutorControllerBase
            :
                ControllerBase //, IConnectionString
    {
        


        [HttpGet]
        [Route("{storeProcedureName}")]
        public ActionResult<JObject> Get
                            (
                                string storeProcedureName
                                ,
                                    [FromQuery(Name = "p")]
                                    string parameters = null
                                , 
                                    [FromQuery(Name = "gf")]
                                    int? groupFrom = null
                                , 
                                    [FromQuery(Name = "gb")]
                                    string groupBy = null

                            )
        {
            JObject result = null;
            var r = Process(storeProcedureName, parameters, out result);
            if (!r)
            {
                return StatusCode(403);
            }
            if
                (
                    groupFrom.HasValue
                    &&
                    groupBy != null
                )
            {
                GroupingJObjectResult(groupFrom.Value, groupBy, result);
            }
            return result;
        }
        [HttpPost]
        [Route("{storeProcedureName}")]
        public ActionResult<JObject> Post
                            (
                                string storeProcedureName
                                ,
                                    [FromBody]
                                    JObject parameters = null


                            )
        {
            JObject result = null;
            var r = Process(storeProcedureName, parameters, out result);
            if (!r)
            {
                return StatusCode(403);
            }
            return result;
        }
    }
}
#endif