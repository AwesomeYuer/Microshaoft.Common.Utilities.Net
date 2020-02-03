using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microshaoft.WebApi.ModelBinders;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace Swagger.WebApplication.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class EchoController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<EchoController> _logger;

        public EchoController(ILogger<EchoController> logger)
        {
            _logger = logger;
        }

        [HttpDelete]
        [HttpGet]
        [HttpHead]
        [HttpOptions]
        [HttpPatch]
        [HttpPost]
        [HttpPut]
        [Route("{* }")]
        public ActionResult Get
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
                        Request = new
                        {
                              parameters
                            , Request.ContentLength
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
                    }
                );

                
        }
    }
}
