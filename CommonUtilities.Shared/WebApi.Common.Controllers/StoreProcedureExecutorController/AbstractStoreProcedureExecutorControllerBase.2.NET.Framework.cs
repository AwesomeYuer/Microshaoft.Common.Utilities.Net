#if NETFRAMEWORK4_X111111
namespace Microshaoft.WebApi.Controllers
{
    using Microshaoft;
    using Newtonsoft.Json.Linq;
    using System.Web.Http;
    using System.Data.SqlClient;
    using System.Net;

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
        public IHttpActionResult Get
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
            JToken result = null;
            var r = false;
            if (NeedCheckWhiteList)
            {
                r = CheckList(storeProcedureName, Request.Method.ToString());
                if (!r)
                {
                    return StatusCode(HttpStatusCode.Forbidden);
                }
            }
            r = Process(storeProcedureName, parameters, out result);
            if (!r)
            {
                return StatusCode(HttpStatusCode.Forbidden);
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
            return Ok(result);
        }
        [HttpPost]
        [Route("{storeProcedureName}")]
        public IHttpActionResult Post
                            (
                                string storeProcedureName
                                ,
                                [FromBody]
                                JObject parameters
                            )
        {
            JToken result = null;
            var r = Process(storeProcedureName, parameters, out result);
            if (!r)
            {
                return StatusCode(HttpStatusCode.Forbidden);
            }
            return Ok(result);
        }

    }
}
#endif