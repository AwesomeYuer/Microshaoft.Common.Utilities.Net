namespace Microshaoft.WebApi.Controllers
{
    using Microshaoft;
    using Microsoft.AspNetCore.Mvc;
    using System;
    using System.Collections.Concurrent;
    using System.Diagnostics;
    using System.Linq;
    using System.Runtime.InteropServices;
    using WebApplication.ASPNetCore;

    [Route("api/[controller]")]
    [ApiController]
    public class PerfMonController : ControllerBase
    {
        private readonly ConcurrentDictionary<string, ExecutingInfo>
                                        _executingCachingStore;
        
        public PerfMonController
                        (
                            ConcurrentDictionary<string, ExecutingInfo>
                                        executingCachingStore
                        )
        {
            _executingCachingStore = executingCachingStore;
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
        [Route("Runtime")]
        public ActionResult Runtime()
        {
            var process = Process.GetCurrentProcess();
            return
                new JsonResult
                        (
                            new
                            {
                                Environment.OSVersion
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
                                        WorkingSet64                = $"{Math.Round(process.PrivateMemorySize64        / 1e+6, 2)} MB"
                                        , PeakWorkingSet64          = $"{Math.Round(process.PeakWorkingSet64           / 1e+6, 2)} MB"
                                        , PrivateMemorySize64       = $"{Math.Round(process.PrivateMemorySize64        / 1e+6, 2)} MB"
                                        , VirtualMemorySize64       = $"{Math.Round(process.VirtualMemorySize64        / 1e+6, 2)} MB"
                                        , PeakVirtualMemorySize64   = $"{Math.Round(process.PeakVirtualMemorySize64    / 1e+6, 2)} MB"
                                    }
                            }
                        );
        }
    }
}