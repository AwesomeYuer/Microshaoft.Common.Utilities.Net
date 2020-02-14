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
    public partial class StoreProcedureExecutorController
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

        /// <summary>
        /// ProcessActionRequest
        /// </summary>
        /// <param name="actionRoutePath">actionRoutePath</param>
        /// <param name="parameters">Json Request Parameters</param>
        /// <param name="resultJsonPathPart1"></param>
        /// <param name="resultJsonPathPart2"></param>
        /// <param name="resultJsonPathPart3"></param>
        /// <param name="resultJsonPathPart4"></param>
        /// <param name="resultJsonPathPart5"></param>
        /// <param name="resultJsonPathPart6"></param>
        /// <returns>Json</returns>
#if NETCOREAPPX_X
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
            ProcessActionRequestResult
                (
                    [ModelBinder(typeof(JTokenModelBinder))]
                        JToken parameters = null
                )
        {
            base
                .AddParametersToHttpContextItems
                        (
                            parameters
                        );
            return
                    base
                        .ProcessActionRequestResult
                            (
                                parameters
                            );
        }

        /// <summary>
        /// ProcessActionRequestAsync 
        /// </summary>
        /// <param name="actionRoutePath">actionRoutePath</param>
        /// <param name="parameters">Json Request Parameters</param>
        /// <param name="resultJsonPathPart1"></param>
        /// <param name="resultJsonPathPart2"></param>
        /// <param name="resultJsonPathPart3"></param>
        /// <param name="resultJsonPathPart4"></param>
        /// <param name="resultJsonPathPart5"></param>
        /// <param name="resultJsonPathPart6"></param>
        /// <returns>Json</returns>
        [BearerTokenBasedAuthorizeFilter(IsRequired = false)]
        [JTokenParametersValidateFilter(AccessingConfigurationKey = "exporting")]
        override public async Task<ActionResult<JToken>>
            ProcessActionRequestResultAsync
                (
                    [ModelBinder(typeof(JTokenModelBinder))]
                        JToken parameters = null
                )
        {
            base
                .AddParametersToHttpContextItems
                        (
                            parameters
                        );
            return
                await
                    base
                        .ProcessActionRequestResultAsync
                            (
                                parameters
                            );
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
                    "foo/bar/{* }"
                )
        ]

        [
            Route
                (
                    "foo/bar/export/{* }"
                )
        ]
        [
            Route
            (
                "foo/bar/sync/{* }"
            )
        ]
        [OperationsAuthorizeFilter(false)]
        [
            RequestJTokenParametersProcessFilter
                (
                    AccessingConfigurationKey = "DefaultAccessing"
                )
        ]

        [OptionalProduces("text/csv", RequestPathKey = "/export/")]
        [JTokenParametersValidateFilter(AccessingConfigurationKey = "DefaultAccessing")]
        [BearerTokenBasedAuthorizeFilter(IsRequired = false)]
        public ActionResult<JToken>
                                ProcessActionRequestFooBar
                                (
                                    [ModelBinder(typeof(JTokenModelBinder))]
                                        JToken parameters = null
                                )
        {
            return
                    ProcessActionRequestResult
                            (
                                parameters
                            );
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
                    "foo/bar/{* }"
                )
        ]

        [
            Route
                (
                    "foo/bar/export/{* }"
                )
        ]
        [
            Route
            (
                "foo/bar/async/{* }/"
            )
        ]
        [OperationsAuthorizeFilter(false)]
        [
            RequestJTokenParametersProcessFilter
                (
                    AccessingConfigurationKey = "DefaultAccessing"
                )
        ]

        [OptionalProduces("text/csv", RequestPathKey = "/export/")]
        [JTokenParametersValidateFilter(AccessingConfigurationKey = "DefaultAccessing")]
        [BearerTokenBasedAuthorizeFilter(IsRequired = false)]
        public async Task<ActionResult<JToken>>
                                ProcessActionRequestFooBarAsync
                                (
                                    [ModelBinder(typeof(JTokenModelBinder))]
                                        JToken parameters = null
                                )
        {
            return
                await
                    ProcessActionRequestResultAsync
                            (
                                parameters
                            );
        }


        /// <summary>
        /// ProcessActionRequestAsync  Export
        /// </summary>
        /// <param name="actionRoutePath">actionRoutePath</param>
        /// <param name="parameters">Json Request Parameters</param>
        /// <param name="resultJsonPathPart1"></param>
        /// <param name="resultJsonPathPart2"></param>
        /// <param name="resultJsonPathPart3"></param>
        /// <param name="resultJsonPathPart4"></param>
        /// <param name="resultJsonPathPart5"></param>
        /// <param name="resultJsonPathPart6"></param>
        /// <returns>Json</returns>
        [
             Route
                 (
                     "bigdataexport/{* }"
                 )
        ]
        [BearerTokenBasedAuthorizeFilter(IsRequired = false)]
        [JTokenParametersValidateFilter(AccessingConfigurationKey = "exporting")]
        override public async Task
                        ProcessActionRequestAsync
                            (
                                [ModelBinder(typeof(JTokenModelBinder))]
                                    JToken parameters = null
                            )
        {
            base
                .AddParametersToHttpContextItems
                        (
                            parameters
                        );
            await
                base
                    .ProcessActionRequestAsync
                        (
                            parameters
                        );
        }
    }
}
#endif