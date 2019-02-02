#if !NETFRAMEWORK4_X && !NETSTANDARD2_0
namespace Microshaoft.WebApi.Controllers
{
    using Microshaoft.Web;
    using Microshaoft.WebApi.ModelBinders;
    using Microsoft.AspNetCore.Cors;
    using Microsoft.AspNetCore.Mvc;
    using Newtonsoft.Json.Linq;

    [Route("api/[controller]")]
    //[ApiController]
    [EnableCors("AllowAllOrigins")]
    //[Authorize]
    //[ValidateModelFilter]
    [ServiceFilter(typeof(JTokenParametersValidateFilterAttribute))]
    public class StoreProcedureExecutorController
                    : AbstractStoreProceduresExecutorControllerBase
    {
        public StoreProcedureExecutorController
                            (
                                IStoreProceduresWebApiService service
                            )
                : base(service)
        {
        }
        //[
        //    Route
        //        (
        //            "{routeName}/"
        //            + "{resultJsonPathPart1?}/"
        //            + "{resultJsonPathPart2?}/"
        //            + "{resultJsonPathPart3?}/"
        //            + "{resultJsonPathPart4?}/"
        //            + "{resultJsonPathPart5?}/"
        //            + "{resultJsonPathPart6?}"
        //        )
        //]        
        // this is a test for new route as above
        // you can set it use public and override for test for override base default implemention
        //public
        //private
        //public override ActionResult<JToken> ProcessActionRequest([FromRoute] string routeName, [ModelBinder(typeof(JTokenModelBinder))] JToken parameters = null, [FromRoute] string resultJsonPathPart1 = null, [FromRoute] string resultJsonPathPart2 = null, [FromRoute] string resultJsonPathPart3 = null, [FromRoute] string resultJsonPathPart4 = null, [FromRoute] string resultJsonPathPart5 = null, [FromRoute] string resultJsonPathPart6 = null)
        //{
        //    return base.ProcessActionRequest(routeName, parameters, resultJsonPathPart1, resultJsonPathPart2, resultJsonPathPart3, resultJsonPathPart4, resultJsonPathPart5, resultJsonPathPart6);
        //}

        [HttpDelete]
        [HttpGet]
        [HttpHead]
        [HttpOptions]
        [HttpPatch]
        [HttpPost]
        [HttpPut]
        [
            Route
                (
                    "{routeName}"
                )
        ]
        // this is a test for new route as above
        // you can set it use public for test
        public ActionResult<JToken>
                           ProcessActionRequest
                                (
                                    [FromRoute]
                                        string routeName
                                    , [ModelBinder(typeof(JTokenModelBinder))]
                                        JToken parameters = null
                                )
        {
            return
                base
                    .ProcessActionRequest
                        (
                            routeName
                            , parameters
                        );
        }
    }
}
#endif
