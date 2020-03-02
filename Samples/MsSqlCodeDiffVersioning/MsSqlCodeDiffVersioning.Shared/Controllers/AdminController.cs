namespace Microshaoft.WebApi.Controllers
{
    using Microshaoft;
    using Microshaoft.Web;
    using Microshaoft.WebApi.ModelBinders;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Data.SqlClient;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using WebApplication.ASPNetCore;

    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    /*
     * Jwt Bear
     * eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1bmlxdWVfbmFtZSI6ImFhYWFhIiwiYWEiOiJBQUFBIiwibmJmIjoxNTgxMDU3MjMwLCJleHAiOjE1ODEwNjA4MzAsImlhdCI6MTU4MTA1NzIzMCwiaXNzIjoiSXNzdWVyMSIsImF1ZCI6IkF1ZGllbmNlMSJ9.PBXgD2ZS1pwRgD3nyOumvcjMt0_u6-Ph0xyev_I3Wyo
    */
    //[AllowAnonymous]
    [ServiceFilter(typeof(ConfigurationSwitchAuthorizeFilter))]
    public class AdminController : ControllerBase
    {
        private readonly ConcurrentDictionary<string, ExecutingInfo>
                                        _executingCachingStore;
        protected readonly AbstractStoreProceduresService
                                        _storeProceduresService;
        public AdminController
                        (
                            ConcurrentDictionary<string, ExecutingInfo>
                                        executingCachingStore
                            , AbstractStoreProceduresService
                                        storeProceduresService

                        )
        {
            _executingCachingStore = executingCachingStore;
            _storeProceduresService = storeProceduresService;
        }
        [HttpPost]
        [HttpGet]
        [Route("RequestResponseLoggingLogLevel")]
        public LogLevel
            RequestResponseLoggingLogLevel
                    (
                        [ModelBinder(typeof(JTokenModelBinder))]
                            JToken parameters = null
                    )
        {
            //if (Request.Method == "POST")
            {
                
                if (parameters is JObject jObject)
                {
                    var r = jObject
                                .TryGetValue
                                        (
                                            "RequestResponseLoggingLogLevel"
                                            , StringComparison
                                                        .OrdinalIgnoreCase
                                            , out var @value
                                        );
                    if (r)
                    {
                        if (value is JValue jValue)
                        {
                            int i = jValue
                                        .Value<int>();
                            if (Enum.IsDefined(typeof(LogLevel), i))
                            {
                                GlobalManager
                                        .RequestResponseLoggingLogLevel = (LogLevel) i;
                            }
                        }
                    }
                }
            }
            return
                GlobalManager
                    .RequestResponseLoggingLogLevel;
        }
        [HttpPost]
        [HttpGet]
        [Route("GlobalLogger")]
        public ILogger
                    GlobalLogger
                            (
                                [ModelBinder(typeof(JTokenModelBinder))]
                                        JToken parameters = null
                            )
        {
            if (Request.Method == "POST")
            {
                if (parameters is JObject jObject)
                {
                    var r = jObject.TryGetValue("MinmumLogLevel", StringComparison.OrdinalIgnoreCase, out var @value);
                    if (r)
                    {
                        if (value is JValue jValue)
                        {
                            int i =  jValue.Value<int>();
                            if (Enum.IsDefined(typeof(LogLevel), i))
                            {

                            }
                        }
                    }
                }
            }

            var type = GlobalManager.GlobalLogger.GetType();
            var field = type.GetField("<MessageLoggers>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic);
            var array = ((Array)field.GetValue(GlobalManager.GlobalLogger));
            type = null;
            for (int i = 0; i < array.Length; i++)
            {
                var messageLogger = array.GetValue(i);
                if (type == null)
                {
                    type = messageLogger.GetType();
                }
                var f = type.GetField("<MinLevel>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic);
                int x = (int)f.GetValue(messageLogger);
                f.SetValue(messageLogger, LogLevel.Debug);
                x = (int)f.GetValue(messageLogger);
            }

            //GlobalManager.GlobalLogger.se

            //var xxxx = (GlobalManager.GlobalLogger);

            return
                GlobalManager
                        .GlobalLogger;
        }

        [HttpGet]
        [Route("IndexedExecutors")]
        public IDictionary<string, IStoreProcedureExecutable>
                    IndexedExecutors()
                        {
                            return
                                _storeProceduresService
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


        [HttpGet]
        [Route("ExecutingCachingStore")]
        public ActionResult ExecutingCachingStore()
        {
            return
                new JsonResult
                        (
                            _executingCachingStore
                        );
        }

        [HttpGet]
        [Route("RequestResponseLoggingProcessor")]
        public ActionResult RequestResponseLoggingProcessor()
        {
            return
                new JsonResult
                        (
                            GlobalManager
                                    .RequestResponseLoggingProcessor
                        );
        }


        [HttpGet]
        [Route("RequestResponseLoggingStats")]
        public ActionResult<JToken> RequestResponseLoggingStats
                                            (
                                                 [ModelBinder(typeof(JTokenModelBinder))]
                                                   JToken parameters = null
                                            )
        {
            return
                new JArray
                    (
                        new MsSqlStoreProceduresExecutor
                                        (_executingCachingStore)
                        {
                            CachedParametersDefinitionExpiredInSeconds =
                                ConfigurationHelper
                                            .Configuration
                                            .GetValue
                                                (
                                                    $"CachedParametersDefinitionExpiredInSeconds"
                                                    , 3600
                                                )
                        }
                            .ExecuteJsonResults
                                (
                                    new SqlConnection()
                                    {
                                        ConnectionString =
                                            ConfigurationHelper
                                                        .Configuration
                                                        .GetValue<string>
                                                            (
                                                                $"Connections:c1:connectionString"
                                                            )
                                    }
                                    , "zsp_Logging_Stats"
                                    ,
                                        (
                                            parameters??
                                                new JObject
                                                    {
                                                        { "p1", 0 }
                                                    }
                                        )
                                )
                                ["Outputs"]
                                ["ResultSets"]
                                .Select
                                    (
                                        (x) =>
                                        {
                                            return
                                                new
                                                    JArray(x["Rows"]);
                                        }
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
        [Route("Echo/{* }")]
        public ActionResult Echo
                (
                     [ModelBinder(typeof(JTokenModelBinder))]
                        JToken parameters = null
                )
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
                    }
                );
        }
    }
}