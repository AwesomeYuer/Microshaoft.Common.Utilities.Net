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
    //using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Data;
    using System.Diagnostics;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Threading.Tasks;

    internal static class InternalApplicationManager
    {
        internal static readonly Process CurrentProcess = Process.GetCurrentProcess();
    }
    public enum DivisorOfBytesUnit : long
    {
          N = 1
        , K = 1024
        , M = K * K
        , G = K * K * K
    }

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
                        string actionRoutePath
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
                                    $"Routes:{actionRoutePath}:{httpMethod}:{accessingConfigurationKey}:Outputs"
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
                    "echo/{* }"
                )
        ]
        [
            Route
                (
                    "result/{* }"
                )
        ]
 
        [
            Route
                (
                    "export/{* }"
                )
        ]
        [
            Route
                (
                    "sync/{* }"
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
                            ProcessActionRequestResult
                                (
                                    [ModelBinder(typeof(JTokenModelBinder))]
                                        JToken parameters = null
                                )
        {
            bool allowEchoRequestInfo = _configuration
                                                .GetValue
                                                    (
                                                        $"{nameof(allowEchoRequestInfo)}"
                                                        , false
                                                    );
            if
                (
                    allowEchoRequestInfo
                    &&
                    Request.Path.Value.Contains("/echo/", StringComparison.OrdinalIgnoreCase)
                )
            {
                return
                    EchoRequestInfo(parameters);
            }
            //var actionRoutePath = Request.Path.Value;
            var actionRoutePath = Request.GetActionRoutePath();
            var beginTime = DateTime.Now;
            var beginTimestamp = Stopwatch.GetTimestamp();
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
                                actionRoutePath
                                , parameters
                                , OnReadRowColumnProcessFunc
                                , Request.Method
                                //, 102
                            );
            return
                ResultProcess
                    (
                        actionRoutePath
                        , beginTimestamp
                        , beginTime
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
                    "result/{* }"
                )
        ]
        [
            Route
                (
                    "export/{* }"
                )
        ]
//#endif
        [
            Route
                (
                    "async/{* }"
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
                            ProcessActionRequestResultAsync
                                (
                                    [ModelBinder(typeof(JTokenModelBinder))]
                                        JToken parameters = null
                                )
        {
            bool allowEchoRequestInfo = _configuration
                                                .GetValue
                                                    (
                                                        $"{nameof(allowEchoRequestInfo)}"
                                                        , false
                                                    );
            if
                (
                    allowEchoRequestInfo
                    &&
                    Request.Path.Value.Contains("/echo/", StringComparison.OrdinalIgnoreCase)
                )
            {
                return
                    EchoRequestInfo(parameters);
            }
            var actionRoutePath = Request.GetActionRoutePath();
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
                                        actionRoutePath
                                        , parameters
                                        , OnReadRowColumnProcessFunc
                                        , Request.Method
                                        //, 102
                                    );
            return
                ResultProcess
                    (
                        actionRoutePath
                        , beginTimestamp
                        , beginTime
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
                        string actionRoutePath
                        , long beginTimestamp
                        , DateTime beginTime
                        ,
                            (
                                int StatusCode
                                , string Message
                                , JToken JResult
                                , TimeSpan? DbExecutingDuration
                            )
                                result
                        , string resultJsonPathPart1 = null
                        , string resultJsonPathPart2 = null
                        , string resultJsonPathPart3 = null
                        , string resultJsonPathPart4 = null
                        , string resultJsonPathPart5 = null
                        , string resultJsonPathPart6 = null
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
                                        actionRoutePath
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
                                        , message = "result data not found by Json Path"
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
                if (!ReferenceEquals(@value, parameters))
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
        
        // 用于诊断回显请求信息
        public virtual JsonResult EchoRequestInfoAsJsonResult(JToken parameters = null)
        { 
            return
                new JsonResult
                (
                    new
                    {
                        jsonRequestParameters = parameters
                        , Request = new
                            {
                                  Request.ContentLength
                                , Request.ContentType
                                , Request.Cookies
                                , Request.HasFormContentType
                                , Request.Headers
                                , Request.Host
                                , Request.IsHttps
                                , Request.Method
                                , Request.Path
                                , Request.PathBase
                                , Request.Protocol
                                , Request.Query
                                , Request.QueryString
                                , Request.RouteValues
                                , Request.Scheme
                            }
                        , HttpContext = new
                            {
                                Connection = new
                                {
                                    RemoteIpAddress = HttpContext
                                                            .Connection
                                                            .RemoteIpAddress
                                                            .ToString()
                                }
                                //, HttpContext.Items
                                , User = 
                                    (
                                        HttpContext
                                                .User != null
                                        ?
                                        new
                                        {
                                            Identity = 
                                                (
                                                    HttpContext
                                                        .User
                                                        .Identity
                                                    !=
                                                    null
                                                    ?
                                                    new
                                                    {
                                                        HttpContext
                                                                .User
                                                                .Identity
                                                                .Name
                                                        , HttpContext
                                                                .User
                                                                .Identity
                                                                .IsAuthenticated
                                                        , HttpContext
                                                                .User
                                                                .Identity
                                                                .AuthenticationType
                                                    }
                                                    :
                                                    null
                                                )
                                            , Claims = 
                                                (
                                                    HttpContext
                                                            .User
                                                            .Claims?
                                                            .Select
                                                                (
                                                                    (x) =>
                                                                    {
                                                                        return
                                                                            new
                                                                            {
                                                                                x.Type
                                                                                , x.Value
                                                                                , x.ValueType
                                                                                , x.Issuer
                                                                                , x.OriginalIssuer
                                                                                , Subject = 
                                                                                        (
                                                                                            x.Subject != null
                                                                                            ?
                                                                                            new
                                                                                                { 
                                                                                                    x.Subject.IsAuthenticated
                                                                                                    , x.Subject.AuthenticationType
                                                                                                    , x.Subject.Name
                                                                                                    , x.Subject.NameClaimType
                                                                                                    , x.Subject.RoleClaimType
                                                                                                    , x.Subject.Label
                                                                                                    , Actor = 
                                                                                                        (
                                                                                                            x.Subject.Actor != null
                                                                                                            ?
                                                                                                            new
                                                                                                            { 
                                                                                                                  x.Subject.Actor.IsAuthenticated
                                                                                                                , x.Subject.Actor.AuthenticationType
                                                                                                                , x.Subject.Actor.Name
                                                                                                                , x.Subject.Actor.NameClaimType
                                                                                                                , x.Subject.Actor.RoleClaimType
                                                                                                                , x.Subject.Actor.Label
                                                                                                            }
                                                                                                            :
                                                                                                            null
                                                                                                        )
                                                                                                }
                                                                                            :
                                                                                            null
                                                                                        )
                                                                            };
                                                                    }
                                                                )
                                                )
                                        }
                                        :
                                        null
                                    )
                            }
                        , ProcessingControllerContext = new 
                            {
                                ActionDescriptor = new
                                { 
                                    ControllerContext.ActionDescriptor.ControllerName
                                    , ControllerContext.ActionDescriptor.ActionName
                                    , ControllerContext.ActionDescriptor.DisplayName
                                    , ControllerContext.ActionDescriptor.RouteValues
                                    , Parameters = ControllerContext.ActionDescriptor
                                                                    .Parameters
                                                                    .Select
                                                                        (
                                                                            (x) =>
                                                                            {
                                                                                return
                                                                                        new
                                                                                        {
                                                                                            ParameterName = x.Name
                                                                                            , ParameterTypeName = x.ParameterType.Name
                                                                                        };
                                                                            }
                                                                        )
                                }
                            }
                    }
                );
        }
        // 可在子类中调用开放成 WebAPI
        public virtual ActionResult EchoRequestInfo(JToken parameters = null)
        {
            return
                EchoRequestInfoAsJsonResult(parameters);
        }
        // 用于诊断回显请求信息
        public virtual JsonResult RuntimeAsJsonResult(string unit = null)
        {
            var type = typeof(DivisorOfBytesUnit);
            long unitDivisor = (long) DivisorOfBytesUnit.K;
            string unitName = $"{nameof(DivisorOfBytesUnit.K)}B";
            if (!unit.IsNullOrEmptyOrWhiteSpace())
            {
                if
                    (
                        string.Compare(unit, "N", StringComparison.OrdinalIgnoreCase) == 0
                        ||
                        string.Compare(unit, "B", StringComparison.OrdinalIgnoreCase) == 0
                    )
                {
                    unit = "N";
                }
                unit = unit.TrimEnd('B', 'b');
            }
            if (Enum.TryParse<DivisorOfBytesUnit>(unit, true, out var @value))
            {
                unitDivisor = (long) @value;
                var name = Enum.GetName(type, @value);
                if (name == "N")
                {
                    name = string.Empty;
                }
                unitName = $"{name}B";
            }
            return
                new JsonResult
                        (
                            new
                            {
                                Environment
                                        .OSVersion
                                , OSPlatform =
                                        EnumerableHelper
                                                    .Range
                                                        (
                                                            OSPlatform.Linux
                                                            , OSPlatform.OSX
                                                            , OSPlatform.Windows
                                                        )
                                                    .First
                                                        (
                                                            (x) =>
                                                            {
                                                                return
                                                                    RuntimeInformation
                                                                            .IsOSPlatform(x);
                                                            }
                                                        )
                                                    .ToString()
                                , RuntimeInformation
                                            .FrameworkDescription
                                , RuntimeInformation
                                            .OSArchitecture
                                , RuntimeInformation
                                            .OSDescription
                                , RuntimeInformation
                                            .ProcessArchitecture
                                , Process = new
                                    {
                                        InternalApplicationManager
                                                .CurrentProcess
                                                .StartTime
                                        , MemoryUtilization = new
                                            {
                                                WorkingSet64                = $"{InternalApplicationManager.CurrentProcess.PrivateMemorySize64       / unitDivisor:N9} {unitName}"
                                                , PeakWorkingSet64          = $"{InternalApplicationManager.CurrentProcess.PeakWorkingSet64          / unitDivisor:N9} {unitName}"
                                                , PrivateMemorySize64       = $"{InternalApplicationManager.CurrentProcess.PrivateMemorySize64       / unitDivisor:N9} {unitName}"
                                                , VirtualMemorySize64       = $"{InternalApplicationManager.CurrentProcess.VirtualMemorySize64       / unitDivisor:N9} {unitName}"
                                                , PeakVirtualMemorySize64   = $"{InternalApplicationManager.CurrentProcess.PeakVirtualMemorySize64   / unitDivisor:N9} {unitName}"
                                                , PagedMemorySize64         = $"{InternalApplicationManager.CurrentProcess.PagedMemorySize64         / unitDivisor:N9} {unitName}"
                                                , PeakPagedMemorySize64     = $"{InternalApplicationManager.CurrentProcess.PeakPagedMemorySize64     / unitDivisor:N9} {unitName}"
                                                , PagedSystemMemorySize64   = $"{InternalApplicationManager.CurrentProcess.PagedSystemMemorySize64   / unitDivisor:N9} {unitName}"
                                            }
                                        , ProcessorUtilization = new
                                            {
                                                InternalApplicationManager
                                                        .CurrentProcess
                                                        .TotalProcessorTime
                                                , InternalApplicationManager
                                                        .CurrentProcess
                                                        .UserProcessorTime
                                                , InternalApplicationManager
                                                        .CurrentProcess
                                                        .PrivilegedProcessorTime
                                        }
                                }
                                    
                            }
                        );
        }
        // 可在子类中调用开放成 WebAPI
        public virtual ActionResult Runtime
                                (
                                    [FromQuery(Name = "u")]
                                    string unit = "KB"
                                )
        {
            return
                RuntimeAsJsonResult(unit);
        }
    }
}
#endif