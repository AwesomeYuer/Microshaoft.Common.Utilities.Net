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
        // GET api/values
        [HttpGet]
        [Route("{storeProcedureName}")]
        public ActionResult<JObject> Get
                                (
                                    string storeProcedureName
                                    ,
                                        [FromQuery]
                                        string p = null //string.Empty
                                )
        {
            var connection = new SqlConnection(ConnectionString);

            var result = SqlHelper
                            .StoreProcedureWebExecute
                                (
                                    connection
                                    , storeProcedureName
                                    , p
                                );


            


            




            return
                result;
        }
    }
}
#endif