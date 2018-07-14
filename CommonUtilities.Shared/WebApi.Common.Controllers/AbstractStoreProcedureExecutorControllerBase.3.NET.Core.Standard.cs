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
                                    [FromQuery(Name = "gf")]
                                    int? groupFrom = null
                                , 
                                    [FromQuery(Name = "gb")]
                                    string groupBy = null
                                ,   
                                    [FromQuery(Name = "p")]
                                    string parameters = null
                            )
        {
            SqlConnection connection = new SqlConnection(ConnectionString);
            JObject result = SqlHelper.StoreProcedureWebExecute(connection, storeProcedureName, parameters, 90);
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
    }
}
#endif