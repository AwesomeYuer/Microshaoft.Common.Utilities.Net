#if !NETFRAMEWORK4_X && !NETSTANDARD2_0
namespace Microshaoft.WebApi.Controllers
{
    using Microshaoft.Web;
    using Microshaoft.WebApi.ModelBinders;
    using Microsoft.AspNetCore.Cors;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Configuration;
    using Newtonsoft.Json.Linq;
    using System.Threading.Tasks;

    [Route("api/[controller]")]
    //[ApiController]
    [EnableCors("AllowAllAny")]
    //[Authorize]
    //[ValidateModelFilter]
    [ServiceFilter(typeof(JTokenParametersValidateFilterAttribute))]
    public class StoreProcedureExecutorController
                    : AbstractStoreProceduresExecutorControllerBase
    {
        public StoreProcedureExecutorController
                            (
                                AbstractStoreProceduresService service
                                , IConfiguration configuration
                            )
                : base(service, configuration)
        {
        }

        [BearerTokenBasedAuthorizeFilter(IsRequired = false)]
        public override ActionResult<JToken>
            ProcessActionRequest
                (
                    [FromRoute]
                        string routeName
                    , [ModelBinder(typeof(JTokenModelBinder))]
                        JToken parameters = null
                    , [FromRoute]
                        string resultJsonPathPart1 = null
                    , [FromRoute]
                        string resultJsonPathPart2 = null
                    , [FromRoute]
                        string resultJsonPathPart3 = null
                    , [FromRoute]
                        string resultJsonPathPart4 = null
                    , [FromRoute]
                        string resultJsonPathPart5 = null
                    , [FromRoute]
                        string resultJsonPathPart6 = null
                )
        {
            return
                base
                    .ProcessActionRequest
                        (
                            routeName
                            , parameters
                            , resultJsonPathPart1
                            , resultJsonPathPart2
                            , resultJsonPathPart3
                            , resultJsonPathPart4
                            , resultJsonPathPart5
                            , resultJsonPathPart6
                        );
        }

        [BearerTokenBasedAuthorizeFilter(IsRequired = false)]
        public override async Task<ActionResult<JToken>>
            ProcessActionRequestAsync
                (
                    [FromRoute]
                        string routeName
                    , [ModelBinder(typeof(JTokenModelBinder))]
                        JToken parameters = null
                    , [FromRoute]
                        string resultJsonPathPart1 = null
                    , [FromRoute]
                        string resultJsonPathPart2 = null
                    , [FromRoute]
                        string resultJsonPathPart3 = null
                    , [FromRoute]
                        string resultJsonPathPart4 = null
                    , [FromRoute]
                        string resultJsonPathPart5 = null
                    , [FromRoute]
                        string resultJsonPathPart6 = null
                )
        {
            return
                await
                    base
                        .ProcessActionRequestAsync
                            (
                                routeName
                                , parameters
                                , resultJsonPathPart1
                                , resultJsonPathPart2
                                , resultJsonPathPart3
                                , resultJsonPathPart4
                                , resultJsonPathPart5
                                , resultJsonPathPart6
                            );
        }
    }
}
#endif