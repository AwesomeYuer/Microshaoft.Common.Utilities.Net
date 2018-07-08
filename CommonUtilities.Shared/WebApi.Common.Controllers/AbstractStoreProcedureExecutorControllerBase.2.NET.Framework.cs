#if NETFRAMEWORK4_X
namespace Microshaoft.WebApi.Controllers
{
    using Microshaoft;
    using Newtonsoft.Json.Linq;
    using System.Web.Http;
    using System.Data.SqlClient;

    [RoutePrefix("api/[controller]")]
    public abstract partial class AbstractStoreProcedureExecutorControllerBase 
            :
                ApiController //, IConnectionString
    {
        // GET api/values
        [HttpGet]
        [Route("{storeProcedureName}")]
        public JObject Get
                                (
                                    string storeProcedureName
                                    ,
                                    [FromUri]
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