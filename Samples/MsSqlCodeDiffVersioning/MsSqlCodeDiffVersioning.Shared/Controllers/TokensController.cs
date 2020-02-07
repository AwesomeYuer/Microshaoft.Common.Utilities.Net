namespace Microshaoft.WebApi.Controllers
{
    using Microshaoft;
    using Microshaoft.WebApi.ModelBinders;
    using Microsoft.AspNetCore.Mvc;
    using Newtonsoft.Json.Linq;
    using System;
    using WebApplication.ASPNetCore;

    [ApiController]
    [Route("[controller]")]
    public class TokensController : ControllerBase
    {
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
                        var claims = JObject
                                        .Parse(@"{""aa"": ""AAAA""}")
                                        .AsClaims();
                        if
                            (
                                JwtTokenHelper
                                    .TryIssueToken
                                        (
                                            "Issuer1"
                                            , "Audience1"
                                            , userName
                                            , claims
                                            , out _
                                            , out var token
                                            , GlobalManager.jwtSymmetricSecurityKey
                                            , GlobalManager.jwtSigningCredentials
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
                                            , GlobalManager.jwtSymmetricSecurityKey
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
