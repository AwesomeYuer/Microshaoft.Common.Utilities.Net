#if NETCOREAPP
namespace Microshaoft.WebApi.Controllers
{
    using Microshaoft;
    using Microshaoft.Web;
    using Microshaoft.WebApi.ModelBinders;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Options;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;

    //[Route("api/[controller]")]
    [ConstrainedRoute("api/[controller]")]
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
                        AbstractStoreProceduresService
                                        service
                        , IConfiguration
                                        configuration
                        , IOptions<CsvFormatterOptions>
                                        csvFormatterOptions
                    )
        {
            _csvFormatterOptions = csvFormatterOptions.Value;
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
            if 
                (
                    _configuration
                            .TryGetSection
                                (
                                    $"Routes:{routeName}:{httpMethod}:{accessingConfigurationKey}:Outputs"
                                    , out var outputsConfiguration
                                )
                )
            {
                var mappings =
                        outputsConfiguration
                                    .GetChildren()
                                    .Select
                                        (
                                            (x) =>
                                            {
                                                return
                                                    (
                                                        TargetJPath : x.Key
                                                        , SourceJPath : x.Get<string>()
                                                    );
                                            }
                                        );
                result = result
                            .MapToNew
                                (
                                    mappings
                                );
            }
            return
                result;
        }

        protected virtual 
                    (
                        bool NeedDefaultProcess
                        , JProperty Field
                    )
                        OnReadRowColumnProcessFunc
                                        (
                                            int resultSetIndex
                                            , IDataReader dataReader
                                            , int rowIndex
                                            , int columnIndex
                                            , Type fieldType
                                            , string fieldName
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
                    "export/{routeName}/"
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
        [OperationsAuthorizeFilter(false)]
        [
            RequestJTokenParametersProcessFilter
                (
                    AccessingConfigurationKey = "DefaultAccessing"
                )
        ]
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
            var beginTimestamp = Stopwatch.GetTimestamp();
            var beginTime = DateTime.Now;
             
            (
                int statusCode
                , string message
                , JToken jResult 
                , TimeSpan? dbExecutingDuration
            )
                =
                    _service
                        .Process
                            (
                                routeName
                                , parameters
                                , OnReadRowColumnProcessFunc
                                , Request.Method
                                //, 102
                            );
            return
                ResultProcess
                    (
                        routeName
                        , beginTimestamp
                        , beginTime
                        , resultJsonPathPart1
                        , resultJsonPathPart2
                        , resultJsonPathPart3
                        , resultJsonPathPart4
                        , resultJsonPathPart5
                        , resultJsonPathPart6
                        , 
                            (
                                statusCode
                                , message
                                , jResult
                                , dbExecutingDuration
                            )
                    );
        }

        [HttpDelete]
        [HttpGet]
        [HttpHead]
        [HttpOptions]
        [HttpPatch]
        [HttpPost]
        [HttpPut]
//#if !NETCOREAPP3_X
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
                    "export/{routeName}/"
                    + "{resultJsonPathPart1?}/"
                    + "{resultJsonPathPart2?}/"
                    + "{resultJsonPathPart3?}/"
                    + "{resultJsonPathPart4?}/"
                    + "{resultJsonPathPart5?}/"
                    + "{resultJsonPathPart6?}"
                )
        ]
//#endif
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
       
        [OperationsAuthorizeFilter(false)]
        [
            RequestJTokenParametersProcessFilter
                    (
                        AccessingConfigurationKey = "DefaultAccessing"
                    )
        ]
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

            var beginTimestamp = Stopwatch.GetTimestamp();
            var beginTime = DateTime.Now;
            (
                int statusCode
                , string message
                , JToken jResult
                , TimeSpan? dbExecutingDuration
            )
                = await
                        _service
                                .ProcessAsync
                                    (
                                        routeName
                                        , parameters
                                        , OnReadRowColumnProcessFunc
                                        , Request.Method
                                        //, 102
                                    );
            return
                ResultProcess
                    (
                        routeName
                        , beginTimestamp
                        , beginTime
                        , resultJsonPathPart1
                        , resultJsonPathPart2
                        , resultJsonPathPart3
                        , resultJsonPathPart4
                        , resultJsonPathPart5
                        , resultJsonPathPart6
                        , 
                            (
                                statusCode
                                , message
                                , jResult
                                , dbExecutingDuration
                            )
                    );
        }
        private ActionResult<JToken> ResultProcess
                    (
                        string routeName
                        , long beginTimestamp
                        , DateTime beginTime
                        , string resultJsonPathPart1
                        , string resultJsonPathPart2
                        , string resultJsonPathPart3
                        , string resultJsonPathPart4
                        , string resultJsonPathPart5
                        , string resultJsonPathPart6
                        ,
                            (
                                int StatusCode
                                , string Message
                                , JToken JResult
                                , TimeSpan? DbExecutingDuration
                            )
                                result
                    )
        {
            Response
                    .StatusCode = result
                                        .StatusCode;
            if (result.StatusCode == 200)
            {
                var httpContext = Response.HttpContext;
                var dbExecutingDuration = result
                                                .DbExecutingDuration;
                if
                    (
                        dbExecutingDuration
                            .HasValue
                    )
                {
                    httpContext
                            .Items
                            .TryAdd
                                (
                                    "dbExecutingDuration"
                                    , dbExecutingDuration
                                );
                }
                var jResult = result.JResult;
                jResult["BeginTime"] = beginTime;
                jResult["EndTime"] = DateTime.Now;
                jResult["DurationInMilliseconds"] =
                                beginTimestamp
                                        .GetElapsedTimeToNow()
                                        .TotalMilliseconds;

                //support custom output nest json by JSONPath in JsonFile Config
                result
                    .JResult = MapByConfiguration
                                    (
                                        routeName
                                        , jResult
                                    );
                result
                    .JResult = jResult
                                .GetDescendantByPathKeys
                                        (
                                            resultJsonPathPart1
                                            , resultJsonPathPart2
                                            , resultJsonPathPart3
                                            , resultJsonPathPart4
                                            , resultJsonPathPart5
                                            , resultJsonPathPart6
                                        );
                if (result.JResult == null)
                {
                    return
                        new
                            JsonResult
                                (
                                    new
                                    {
                                        statusCode = 404
                                        , resultCode = -404
                                        , message = "data path not found"
                                    }
                                )
                        {
                            StatusCode = 404
                            , ContentType = "application/json"
                        };
                }
            }
            else
            {
                return
                    new
                        JsonResult
                            (
                                new
                                {
                                    statusCode = result
                                                    .StatusCode
                                    , resultCode = -1 * result
                                                            .StatusCode
                                    , message = result
                                                    .Message
                                }
                            )
                {
                    StatusCode = result.StatusCode
                    , ContentType = "application/json"
                };
            }
            return
                result
                    .JResult;
        }
        public virtual void AddParametersToHttpContextItems
            (
                JToken parameters
                , string key = requestJTokenParametersItemKey
            )
        {
            var httpContext = ControllerContext
                                            .HttpContext;
            if
                (
                    httpContext
                        .Items
                        .TryGetValue
                            (
                                key
                                , out var @value
                            )
                )
            {
                if (!object.ReferenceEquals(@value, parameters))
                {
                    httpContext
                        .Items[key]
                            = parameters;
                }
            }
            else
            {
                httpContext
                    .Items
                    .Add(key, parameters);
            }
        }
    }
}
#endif