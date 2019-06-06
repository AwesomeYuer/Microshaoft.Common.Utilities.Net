#if !NETFRAMEWORK4_X && NETCOREAPP2_X
namespace Microshaoft.WebApi.Controllers
{
    using Microshaoft;
    //using Microshaoft.CompositionPlugins;
    using Microshaoft.Web;
    using Microshaoft.WebApi.ModelBinders;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Configuration;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
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
        protected readonly
                    IConfiguration
                            _configuration;
        public AbstractStoreProceduresExecutorControllerBase
                    (
                        AbstractStoreProceduresService service
                        , IConfiguration configuration
                    )
        {
            _service = service;
            _configuration = configuration;
        }
        private JToken MapByConfiguration
                    (
                        string routeName
                        , JToken result
                    )
        {
            var httpMethod = $"Http{Request.Method}";
            var accessingConfigurationKey = "DefaultAccessing";
            if
                (
                    Request
                        .Path
                        .ToString()
                        .Contains
                            (
                                "/export/"
                                , StringComparison
                                        .OrdinalIgnoreCase
                            )
                )
            {
                accessingConfigurationKey = "exporting";
            }
            var outputsConfiguration = _configuration
                                            .GetSection
                                                ($"Routes:{routeName}:{httpMethod}:{accessingConfigurationKey}:Outputs");

            if (outputsConfiguration.Exists())
            {
                var mappings = outputsConfiguration
                                    .GetChildren()
                                    .Select
                                        (
                                            (x) =>
                                            {
                                                (
                                                    string TargetJPath
                                                    , string SourceJPath
                                                )
                                                    rrr =
                                                        (
                                                            x.Key
                                                            , x.Get<string>()
                                                        );
                                                return rrr;
                                            }
                                        );
                result = result
                            .MapToNew
                                (
                                    mappings
                                );
            }
            return result;
        }

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
                else if (fieldType == typeof(DateTime))
                {
                    var path = Request.Path.ToString();
                    needDefaultProcess = true;
                    if
                        (
                            !path
                                .Contains
                                    (
                                        "/export/"
                                        , StringComparison
                                                .OrdinalIgnoreCase
                                    )
                        )
                    {
                        var s = dataReader
                                        .GetDateTime(columnIndex)
                                        .ToString
                                            (
                                                "yyyy-MM-ddTHH:mm:ss.fffzzz"
                                            );
                        JValue jValue = new JValue(s);
                        field = new JProperty
                                        (
                                            fieldName
                                            , jValue
                                        );
                        needDefaultProcess = false;
                    }
                }
            }
            return 
                (
                    needDefaultProcess
                    , field
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
        [OperationsAuthorizeFilter(false)]
        [RequestJTokenParametersDefaultProcessFilter]
        [OptionalProduces("text/csv", RequestPathKey = "/export/")]
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
                //support custom output nest json by JSONPath in JsonFile Config
                result = MapByConfiguration(routeName, r.Result);
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
                [HttpGet]
        [Route("admin/{routeName}")]
        public IDictionary<string, IStoreProcedureExecutable>
                    Admin
                        (
                            string routeName
                        )
        {
            return
                _service
                    .IndexedExecutors
                    //.Select
                    //    (
                    //        (x) =>
                    //        {
                    //            x.Value

                    //        }
                    
                    //    )
                    ;
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
        [OperationsAuthorizeFilter(false)]
        [RequestJTokenParametersDefaultProcessFilter]
        [OptionalProduces("text/csv", RequestPathKey = "/export/")]
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
                //support custom output nest json by JSONPath in JsonFile Config
                result = MapByConfiguration
                                (
                                    routeName
                                    , r.Result
                                );
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
            return
                result;
        }
    }
}
#endif