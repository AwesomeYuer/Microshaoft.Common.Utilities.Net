#if !NETFRAMEWORK4_X && NETCOREAPP2_X
namespace Microshaoft.WebApi.Controllers
{
    using Microshaoft;
    using Microshaoft.Web;
    using Microshaoft.WebApi.ModelBinders;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Infrastructure;
    using Microsoft.Extensions.Configuration;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Data;
    using System.Threading.Tasks;

    [Route("api/[controller]")]
    [ApiController]
    public abstract partial class 
                AbstractStoreProceduresExecutorControllerBase
                    :
                        ControllerBase
    {
        protected readonly
                    AbstractStoreProceduresService
                            _service;

        protected virtual 
                    (
                        bool NeedDefaultProcess
                        , JProperty Field
                    )
                        OnReadRowColumnProcessFunc
                                        (
                                            IDataReader dataReader
                                            , Type fieldType
                                            , string fieldName
                                            , int rowIndex
                                            , int columnIndex
                                        )
        {
            JProperty field = null;
            bool needDefaultProcess = true;
            if (!dataReader.IsDBNull(columnIndex))
            {
                if (fieldType == typeof(string))
                {
                    //if (fieldName.Contains("Json", StringComparison.OrdinalIgnoreCase))
                    {
                        //fieldName = fieldName.Replace("json", "", System.StringComparison.OrdinalIgnoreCase);
                        {
                            var s = dataReader.GetString(columnIndex);
                            //var ss = s.Trim();
                            if
                                (
                                    //(ss.StartsWith("{") && ss.EndsWith("}"))
                                    //||
                                    //(ss.StartsWith("[") && ss.EndsWith("]"))
                                    s.IsJson(out var jToken, true)
                                )
                            {
                                //try
                                //{
                                field = new JProperty
                                        (
                                            fieldName
                                            , jToken
                                        );
                                needDefaultProcess = false;
                                //}
                                //catch
                                //{
                                //}
                            }
                        }
                    }
                }
            }
            
            return 
                (
                    needDefaultProcess
                    , field
                );
        }

        protected IActionSelector _actionSelector;

        public AbstractStoreProceduresExecutorControllerBase
                    (
                        AbstractStoreProceduresService service
                        , IActionSelector actionSelector
                    )
        {
            _service = service;
            _actionSelector = actionSelector;
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
                    "{routeName}/"
                    + "{resultJsonPathPart1?}/"
                    + "{resultJsonPathPart2?}/"
                    + "{resultJsonPathPart3?}/"
                    + "{resultJsonPathPart4?}/"
                    + "{resultJsonPathPart5?}/"
                    + "{resultJsonPathPart6?}"
                )
        ]
        [
            Route
                (
                    "sync/{routeName}/"
                    + "{resultJsonPathPart1?}/"
                    + "{resultJsonPathPart2?}/"
                    + "{resultJsonPathPart3?}/"
                    + "{resultJsonPathPart4?}/"
                    + "{resultJsonPathPart5?}/"
                    + "{resultJsonPathPart6?}"
                )
        ]
        [OperationsAuthorizeFilter]
        [RequestJTokenParametersDefaultProcessFilter]
        public virtual ActionResult<JToken>
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
            JToken result = null;
            (
                int StatusCode
                , string Message
                , JToken Result
            )
            r =
                _service
                    .Process
                        (
                            routeName
                            , parameters
                            , OnReadRowColumnProcessFunc
                            , Request.Method
                            //, 102
                        );
            if (r.StatusCode != -1)
            {
                result = r.Result;
                result = result
                            .GetDescendantByPathKeys
                                (
                                    resultJsonPathPart1
                                    , resultJsonPathPart2
                                    , resultJsonPathPart3
                                    , resultJsonPathPart4
                                    , resultJsonPathPart5
                                    , resultJsonPathPart6
                                );
                Response.StatusCode = r.StatusCode;
            }
            else
            {
                return
                    new JsonResult
                        (
                            new
                            {
                                r.StatusCode
                                , r.Message
                            }
                        )
                    {
                        StatusCode = r.StatusCode
                    };
            }
            return result;
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
                    "{routeName}/"
                    + "{resultJsonPathPart1?}/"
                    + "{resultJsonPathPart2?}/"
                    + "{resultJsonPathPart3?}/"
                    + "{resultJsonPathPart4?}/"
                    + "{resultJsonPathPart5?}/"
                    + "{resultJsonPathPart6?}"
                )
        ]
        [
            Route
                (
                    "async/{routeName}/"
                    + "{resultJsonPathPart1?}/"
                    + "{resultJsonPathPart2?}/"
                    + "{resultJsonPathPart3?}/"
                    + "{resultJsonPathPart4?}/"
                    + "{resultJsonPathPart5?}/"
                    + "{resultJsonPathPart6?}"
                )
        ]
        [OperationsAuthorizeFilter]
        [RequestJTokenParametersDefaultProcessFilter]
        public virtual async Task<ActionResult<JToken>>
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

            JToken result = null;
            (
                int StatusCode
                , string Message
                , JToken Result
            )
            r = await
                _service
                    .ProcessAsync
                        (
                            routeName
                            , parameters
                            , OnReadRowColumnProcessFunc
                            , Request.Method
                        //, 102
                        );
            if (r.StatusCode != -1)
            {
                result = r.Result;
                result = result
                            .GetDescendantByPathKeys
                                (
                                    resultJsonPathPart1
                                    , resultJsonPathPart2
                                    , resultJsonPathPart3
                                    , resultJsonPathPart4
                                    , resultJsonPathPart5
                                    , resultJsonPathPart6
                                );
                Response.StatusCode = r.StatusCode;
            }
            else
            {
                return
                    new JsonResult
                        (
                            new
                            {
                                r.StatusCode
                                ,
                                r.Message
                            }
                        )
                    {
                        StatusCode = r.StatusCode
                    };
            }
            return result;
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
                    "export/{routeName}/"
                    + "{resultJsonPathPart1?}/"
                    + "{resultJsonPathPart2?}/"
                    + "{resultJsonPathPart3?}/"
                    + "{resultJsonPathPart4?}/"
                    + "{resultJsonPathPart5?}/"
                    + "{resultJsonPathPart6?}"
                )
        ]
        [Produces("text/csv")]
        [OperationsAuthorizeFilter]
        [RequestJTokenParametersDefaultProcessFilter]
        public virtual ActionResult<JToken>
                            ProcessActionRequestForExport
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
                                    , [FromQuery]
                                        string e = "utf-8"
                                )
        {
            if (!CheckOperation("exporting", HttpContext, parameters, routeName, out var failResult))
            {
                return
                    failResult;
            }
            return
                ProcessActionRequest
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
                    "export/{routeName}/"
                    + "{resultJsonPathPart1?}/"
                    + "{resultJsonPathPart2?}/"
                    + "{resultJsonPathPart3?}/"
                    + "{resultJsonPathPart4?}/"
                    + "{resultJsonPathPart5?}/"
                    + "{resultJsonPathPart6?}"
                )
        ]
        [Produces("text/csv")]
        [OperationsAuthorizeFilter]
        [RequestJTokenParametersDefaultProcessFilter]
        public virtual async Task<ActionResult<JToken>>
                    ProcessActionRequestForExportAsync
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
                            , [FromQuery]
                                string e = "utf-8"
                        )
        {
            if (!CheckOperation("exporting", HttpContext, parameters,routeName, out var failResult))
            {
                return
                    failResult;
            }
            return
                await
                    ProcessActionRequestAsync
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

        private bool CheckOperation
                        (
                            string operationConfigurationKey
                            , HttpContext httpContext
                            , JToken parameters
                            , string routeName
                            , out JsonResult failResult
                        )
        {
            var statusCode = 200;
            var message = string.Empty;
            failResult = null;
            JsonResult result = null;
            void FailResult()
            {
                result = new JsonResult
                    (
                        new
                        {
                            StatusCode = statusCode
                            , Message = message
                        }
                    )
                {
                    StatusCode = statusCode
                };
            }
            var configuration = httpContext
                                    .RequestServices
                                    .GetService
                                        (
                                            typeof(IConfiguration)
                                        ) as IConfiguration;
            var request = HttpContext
                                .Request;
            var httpMethod = $"Http{request.Method}";
            var allowExport = false;
            var allowExportConfiguration =
                        configuration
                            .GetSection($"Routes:{routeName}:{httpMethod}:{operationConfigurationKey}:allow");
            if (allowExportConfiguration.Exists())
            {
                allowExport = allowExportConfiguration.Get<bool>();
            }
            if (!allowExport)
            {
                statusCode = 403;
                message = "forbidden export";
                FailResult();
            }
            else //(allowExport)
            {
                var allowExportOperationsConfiguration =
                       configuration
                           .GetSection($"Routes:{routeName}:{httpMethod}:{operationConfigurationKey}:operations");
                if (allowExportOperationsConfiguration.Exists())
                {
                    var operations = allowExportOperationsConfiguration.Get<string[]>();
                    //var userName = "anonymous";
                    var user = httpContext.User;
                    //to do: User-Role-Action/Operation 鉴权
                }
                if (!allowExport)
                {
                    statusCode = 403;
                    message = "forbidden export operation";
                    FailResult();
                }
            }
            failResult = result;
            return
                allowExport;
        }
    }
}
#endif
