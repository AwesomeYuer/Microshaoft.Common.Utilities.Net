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
    using Microsoft.CodeAnalysis.Operations;
    using System;
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


        /// <summary>
        /// ProcessActionRequest
        /// </summary>
        /// <param name="routeName">routeName</param>
        /// <param name="parameters">Json Request Parameters</param>
        /// <param name="resultJsonPathPart1"></param>
        /// <param name="resultJsonPathPart2"></param>
        /// <param name="resultJsonPathPart3"></param>
        /// <param name="resultJsonPathPart4"></param>
        /// <param name="resultJsonPathPart5"></param>
        /// <param name="resultJsonPathPart6"></param>
        /// <returns>Json</returns>
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
            base
                .AddParametersToHttpContextItems
                        (
                            parameters
                        );
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




        /// <summary>
        /// ProcessActionRequestAsync 
        /// </summary>
        /// <param name="routeName">routeName</param>
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
            base
                .AddParametersToHttpContextItems
                        (
                            parameters
                        );
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


        /// <summary>
        /// ProcessActionRequestAsync  Export
        /// </summary>
        /// <param name="routeName">routeName</param>
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
            base
                .AddParametersToHttpContextItems
                        (
                            parameters
                        );
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