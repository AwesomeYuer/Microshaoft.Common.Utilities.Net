#if !NETFRAMEWORK4_X && NETCOREAPP2_X
namespace Microshaoft.WebApi.Controllers
{
    using Microshaoft.Web;
    using Microshaoft.WebApi.ModelBinders;
    using Microsoft.AspNetCore.Mvc;
    using Newtonsoft.Json.Linq;
    using System.Collections.Generic;
    using System.Linq;
    [Route("api/[controller]")]
    [ApiController]
    public abstract partial class
            AbstractCommonActionProcessingControllerBase :
                                            ControllerBase
    {
        private readonly IActionCommonProcessable _service;
        public AbstractCommonActionProcessingControllerBase
                    (
                        IActionCommonProcessable service
                    )
        {
            _service = service;
        }

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
                    "{actionRoute}/"
                    //+ "{storeProcedureName}/"
                    //+ "{resultPathSegment1?}/"
                    //+ "{resultPathSegment2?}/"
                    //+ "{resultPathSegment3?}/"
                    //+ "{resultPathSegment4?}/"
                    //+ "{resultPathSegment5?}/"
                    //+ "{resultPathSegment6?}"
                )
        ]
        public ActionResult<JToken> ProcessActionRequest
                        (
                            [FromRoute]
                            string actionRoute
                            , [ModelBinder(typeof(JTokenModelBinder))]
                                JToken parameters = null
                        )
        {
            var r =
                    _service
                        .Process
                            (
                                parameters
                            );

            return r.Result;
        }
    }
}
#endif