namespace Swagger.WebApplication.Controllers
{
    using Microshaoft;
    using Microshaoft.WebApi.ModelBinders;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json.Linq;
    using System;
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
