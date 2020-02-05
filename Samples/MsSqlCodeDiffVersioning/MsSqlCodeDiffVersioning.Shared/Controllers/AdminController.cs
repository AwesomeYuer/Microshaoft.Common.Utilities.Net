namespace Microshaoft.WebApi.Controllers
{
    using Microshaoft;
    using Microshaoft.Web;
    using Microshaoft.WebApi.ModelBinders;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Configuration;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Data.SqlClient;
    using System.Linq;
    using System.Runtime.InteropServices;
    using WebApplication.ASPNetCore;

    [Route("api/[controller]")]
    [ApiController]
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
                                    .AsyncRequestResponseLoggingProcessor
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

        [HttpGet]
        [Route("Runtime")]
        public ActionResult Runtime()
        {
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
                                          GlobalManager
                                                .CurrentProcess
                                                .StartTime
                                        , MemoryUtilization = new
                                            {
                                                WorkingSet64                = $"{GlobalManager.CurrentProcess.PrivateMemorySize64       / 1e+6:N} MB"
                                                , PeakWorkingSet64          = $"{GlobalManager.CurrentProcess.PeakWorkingSet64          / 1e+6:N} MB"
                                                , PrivateMemorySize64       = $"{GlobalManager.CurrentProcess.PrivateMemorySize64       / 1e+6:N} MB"
                                                , VirtualMemorySize64       = $"{GlobalManager.CurrentProcess.VirtualMemorySize64       / 1e+6:N} MB"
                                                , PeakVirtualMemorySize64   = $"{GlobalManager.CurrentProcess.PeakVirtualMemorySize64   / 1e+6:N} MB"
                                                , PagedMemorySize64         = $"{GlobalManager.CurrentProcess.PagedMemorySize64         / 1e+6:N} MB"
                                                , PeakPagedMemorySize64     = $"{GlobalManager.CurrentProcess.PeakPagedMemorySize64     / 1e+6:N} MB"
                                                , PagedSystemMemorySize64   = $"{GlobalManager.CurrentProcess.PagedSystemMemorySize64   / 1e+6:N} MB"
                                            }
                                        , ProcessorUtilization = new
                                            { 
                                                  GlobalManager
                                                        .CurrentProcess
                                                        .TotalProcessorTime
                                                , GlobalManager
                                                        .CurrentProcess
                                                        .UserProcessorTime
                                                , GlobalManager
                                                        .CurrentProcess
                                                        .PrivilegedProcessorTime
                                        }
                                }
                                    
                            }
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
                                , User = new
                                {
                                    HttpContext.User.Claims
                                    , Identity = new
                                    {
                                        HttpContext
                                                .User
                                                .Identity
                                                .Name
                                    }
                                }
                            }
                    }
                );
        }
    }
}