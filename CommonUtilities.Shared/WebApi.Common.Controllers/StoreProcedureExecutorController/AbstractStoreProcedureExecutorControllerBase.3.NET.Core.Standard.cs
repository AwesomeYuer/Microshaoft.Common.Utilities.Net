#if !NETFRAMEWORK4_X && NETCOREAPP2_X
namespace Microshaoft.WebApi.Controllers
{
    using Microshaoft;
    using Microshaoft.Web;
    using Microshaoft.WebApi.ModelBinders;
    using Microsoft.AspNetCore.Mvc;
    using Newtonsoft.Json.Linq;
    [Route("api/[controller]")]
    [ApiController]
    public abstract partial class 
                AbstractStoreProceduresExecutorControllerBase
                    :
                        ControllerBase
    {
        private readonly
                    IStoreProceduresWebApiService
                            _service;

        public AbstractStoreProceduresExecutorControllerBase
                    (
                        IStoreProceduresWebApiService service
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
                    "{connectionID}/"
                    + "{storeProcedureName}/"
                    + "{resultPathSegment1?}/"
                    + "{resultPathSegment2?}/"
                    + "{resultPathSegment3?}/"
                    + "{resultPathSegment4?}/"
                    + "{resultPathSegment5?}/"
                    + "{resultPathSegment6?}"
                )
        ]
        public virtual ActionResult<JToken> ProcessActionRequest
                            (
                                [FromRoute]
                                string connectionID //= "mssql"
                                , [FromRoute]
                                    string storeProcedureName
                                , [ModelBinder(typeof(JTokenModelBinder))]
                                    JToken parameters = null
                                , [FromRoute]
                                    string resultPathSegment1 = null
                                , [FromRoute]
                                    string resultPathSegment2 = null
                                , [FromRoute]
                                    string resultPathSegment3 = null
                                , [FromRoute]
                                    string resultPathSegment4 = null
                                , [FromRoute]
                                    string resultPathSegment5 = null
                                , [FromRoute]
                                    string resultPathSegment6 = null
                            )
        {
            JToken result = null;
            (int StatusCode, JToken Result) rr;
            rr = _service
                    .Process
                        (
                            connectionID
                            , storeProcedureName
                            , parameters
                            , Request.Method
                            , 102
                        );
            result = rr
                        .Result
                        .GetDescendantByPath
                            (
                                resultPathSegment1
                                , resultPathSegment2
                                , resultPathSegment3
                                , resultPathSegment4
                                , resultPathSegment5
                                , resultPathSegment6
                            );
            return result;
        }
    }
}
#endif
