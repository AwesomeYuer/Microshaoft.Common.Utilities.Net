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
                                    [ModelBinder(typeof(JTokenFormModelBinderProvider))]
                                    JObject parameters = null
                                //, 
                                //    [FromQuery(Name = "gf")]
                                //    int? groupFrom = null
                                //, 
                                //    [FromQuery(Name = "gb")]
                                //    string groupBy = null

                            )
        {
            JObject result = null;
            var r = Process(storeProcedureName, parameters, out result);
            if (!r)
            {
                return StatusCode(403);
            }

            ////var x = new int[]
            ////{
            ////    1
            ////}.AsQueryable().Where("x <= 1").ToArray();
                

            //if
            //    (
            //        groupFrom.HasValue
            //        &&
            //        groupBy != null
            //    )
            //{
            //    GroupingJObjectResult(groupFrom.Value, groupBy, result);
            //}
            return result;
        }
        [HttpPost]
        [Route("{storeProcedureName}")]
        public ActionResult<JObject> Post
                            (
                                string storeProcedureName
                                ,
                               [FromBody]
                               //    //[FromForm]
                               [ModelBinder(typeof(JTokenFormModelBinderProvider))]
                               JToken parameters = null
                            )
        {
            JObject result = null;
            var r = Process(storeProcedureName, (JObject) parameters, out result);
            if (!r)
            {
                return StatusCode(403);
            }
            return result;
        }
    }
}
#endif