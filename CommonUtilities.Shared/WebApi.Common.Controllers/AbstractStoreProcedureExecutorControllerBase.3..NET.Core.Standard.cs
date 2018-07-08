#if !NETFRAMEWORK4_X && NETCOREAPP2_X

namespace Microshaoft.WebApi.Controllers
{
    using Microshaoft;
    using Microsoft.AspNetCore.Mvc;
    using Newtonsoft.Json.Linq;
    using System.Data.SqlClient;
    
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
            return
                SqlHelper
                    .StoreProcedureWebExecute
                        (
                            connection
                            , storeProcedureName
                            , p
                        );
        }
    }
}
#endif