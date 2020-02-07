

namespace Swagger.WebApplication.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Subjects;
    using System.Reflection.Metadata.Ecma335;
    using System.Runtime.InteropServices;
    using System.Security.Cryptography.Xml;
    using System.Threading.Tasks;
    using Microshaoft;
    using Microshaoft.WebApi.ModelBinders;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json.Linq;
    [ApiController]
    [Route("[controller]")]
    public class TokensController : ControllerBase
    {

        private readonly ILogger<EchoController> _logger;

        public TokensController(ILogger<EchoController> logger)
        {
            _logger = logger;
        }
        [HttpGet]
        public ActionResult<JToken> Issue
                (
                     [ModelBinder(typeof(JTokenModelBinder))]
                        JToken parameters //= null
                )
        {
            if (parameters is JObject jObject)
            {
                if (jObject.TryGetValue("UserName", StringComparison.OrdinalIgnoreCase, out var @value))
                {
                    if (@value is JValue jValue)
                    {
                        var userName = jValue.Value<string>();
                        if
                            (
                                JwtTokenHelper
                                    .TryIssueToken
                                        (
                                            "Issuer1"
                                            , "Audience1"
                                            , userName
                                            , JObject.Parse(@"{""aa"": ""AAAA""}")
                                            , Program.defaultSecretKey
                                            , out _
                                            , out var token
                                        )
                            )
                        {
                            parameters["Jwt"] = token;
                        }
                        if
                            (
                                JwtTokenHelper
                                    .TryValidateToken
                                        (
                                            token
                                            , Program.defaultSecretKey
                                            , out var x
                                            , out var y
                                        )
                            )

                        {
                            
                                                        


                        }

                    }
                }
            }
            return
                parameters;
        }
    }
}
