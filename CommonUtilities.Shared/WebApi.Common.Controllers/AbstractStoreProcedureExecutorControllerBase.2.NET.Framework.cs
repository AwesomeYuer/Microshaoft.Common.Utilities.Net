#if NETFRAMEWORK4_X
namespace Microshaoft.WebApi.Controllers
{
    using Microshaoft;
    using Newtonsoft.Json.Linq;
    using System.Web.Http;
    using System.Data.SqlClient;

    [RoutePrefix("api/StoreProcedureExecutor")]
    public abstract partial class AbstractStoreProcedureExecutorControllerBase 
            :
                ApiController //, IConnectionString
    {

        /* 
        * ASP.NET Framework should implement Get Action/Method
        * but ASP.NET Core needn't  
        */
        // GET api/values
        [HttpGet]
        [Route("{storeProcedureName}")]
        public JObject Get
                            (
                                string storeProcedureName
                                ,
                                    [FromUri(Name = "gf")]
                                    int? groupFrom = null
                                ,
                                    [FromUri(Name = "gb")]
                                    string groupBy = null
                                ,
                                    [FromUri(Name = "p")]
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