#if NETCOREAPP || NETSTANDARD2_X
namespace Microshaoft.WebApi.Controllers
{
    using Microshaoft.Web;
    using Microshaoft.WebApi.ModelBinders;
    using Microsoft.AspNetCore.Cors;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Options;
    using Newtonsoft.Json.Linq;
    using System.Threading.Tasks;
#if NETCOREAPP3_X
    using Microshaoft.AspNetCore.ConcurrencyLimiters;
#endif

    //[Route("api/[controller]")]
    //[ApiController]
    [EnableCors("AllowAllAny")]
    //[Authorize]
    public class StoreProcedureExecutorController
                    : AbstractStoreProceduresExecutorControllerBase
    {
        public StoreProcedureExecutorController
                            (
                                AbstractStoreProceduresService service
                                , IConfiguration configuration
                                , IOptions<CsvFormatterOptions> csvFormatterOptions
                            )
                : base
                    (
                        service
                        , configuration
                        , csvFormatterOptions
                    )
        {
        }
#if NETCOREAPP3_X
        [
            ConcurrencyLimiterFilter
                (
                    QueueStoreType = QueueStoreTypeEnum.QueueFIFO
                    , MaxConcurrentRequests = 1
                    , RequestQueueLimit = 1
                )
        ]
#endif
        [JTokenParametersValidateFilter(AccessingConfigurationKey = "DefaultAccessing")]
        [BearerTokenBasedAuthorizeFilter(IsRequired = false)]
        override public ActionResult<JToken>
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
        [JTokenParametersValidateFilter(AccessingConfigurationKey = "exporting")]
        override public async Task<ActionResult<JToken>>
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

        [
             Route
                 (
                     "bigdataexport/{routeName}/"
                 )
        ]
        [BearerTokenBasedAuthorizeFilter(IsRequired = false)]
        [JTokenParametersValidateFilter(AccessingConfigurationKey = "exporting")]
        override public async Task
                        ProcessActionRequestAsync
                            (
                                [FromRoute]
                                string routeName
                                , [ModelBinder(typeof(JTokenModelBinder))]
                                JToken parameters = null
                            )
        {
            await
                base
                    .ProcessActionRequestAsync
                        (
                            routeName
                            , parameters
                        );
        }
    }
}
#endif