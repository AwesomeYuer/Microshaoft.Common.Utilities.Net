#if !NETFRAMEWORK4_X && NETCOREAPP2_X
namespace Microshaoft.WebApi.Controllers
{
    using Microshaoft;
    using Microshaoft.Linq.Dynamic;
    using Microshaoft.WebApi.ModelBinders;
    using Microsoft.AspNetCore.Mvc;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Data.SqlClient;
    using System.Linq;
    using System.Net.Http.Formatting;
    using Microshaoft.Web;

    [Route("api/[controller]")]
    [ApiController]
    public abstract partial class AbstractStoreProcedureExecutorControllerBase
            :
                ControllerBase //, IConnectionString
    {
        //[ResponseCache(Duration = 10)]
        //[
        //    TypeFilter
        //        (
        //            typeof(RouteAuthorizeActionFilter)
        //            //, IsReusable = false
        //            , Arguments = new object[] {  
        //                new string[]
        //                {
        //                    "storeProcedureName"
        //                }
        //            }
        //        )
        //]
        //[
        //    RouteAuthorizeActionFilter
        //    (
        //        new string[]
        //            {
        //                "storeProcedureName"
        //            }
        //    )
        //]
        [HttpDelete]
        [HttpGet]
        [HttpHead]
        [HttpOptions]
        [HttpPatch]
        [HttpPost]
        [HttpPut]
        [Route("{storeProcedureName}")]
        public ActionResult<JToken> ProcessActionRequest
                            (
                                string storeProcedureName
                                ,
                                [ModelBinder(typeof(JTokenModelBinder))]
                                JToken parameters = null
                            )
        {
            JToken result = null;
            var r = false;
            if (NeedCheckWhiteList)
            {
                r = CheckList(storeProcedureName, Request.Method);
                if (!r)
                {
                    return StatusCode(403);
                }
            }
            r = Process(storeProcedureName, parameters, out result);
            if (!r)
            {
                return StatusCode(403);
            }
            result["TimeStamp"] = DateTime.Now;
            return result;
        }
    }
}
#endif