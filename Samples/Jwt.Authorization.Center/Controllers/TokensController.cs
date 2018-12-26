namespace Microshaoft.Web.Samples
{
    using Microshaoft;
    using Microshaoft.WebApi.ModelBinders;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Cors;
    using Microsoft.AspNetCore.Mvc;
    using Newtonsoft.Json.Linq;
    using System.Security.Claims;
    using System.Web;

    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    //启用跨域，指定允许的跨域策略
    [EnableCors("AllowAllOrigins")]

    public class TokensController : ControllerBase
    {
        [HttpGet]
        [Route("jsonp")]
        public ActionResult Issue2
                        (
                            //[FromRoute]
                            //    string callback
                            //,
                            [ModelBinder(typeof(JTokenModelBinder))]
                                JToken json
                        )
        {
            var jToken = Result(json);
            var callback = json["jsonp"].Value<string>();
            //Anti-XSS
            callback = HttpUtility.JavaScriptStringEncode(callback);
            var content = $"{callback}({jToken.ToString()})";
            return
                Content
                (
                      content
                      , "text/javascript"
                );
        }
        // GET api/values
        [HttpGet]
        public ActionResult<JToken> Issue
                        (
                            [ModelBinder(typeof(JTokenModelBinder))]
                                JToken json
                        )
        {
            return Result(json);
        }

        private JToken Result(JToken json)
        {
            var appID = json["AppID"].Value<string>();
            var userName = HttpContext
                                    .User
                                    .Identity
                                    .Name;

            var remoteIpAddress = Request
                                        .HttpContext
                                        .Connection
                                        .RemoteIpAddress;

            var b = JwtTokenHelper
                            .TryIssueToken
                                (
                                    "aaa"
                                    , appID
                                    , userName
                                    //, JObject.Parse(@"{a:""aaaaaa"",a1:{F1:""aaaaaa""}}")
                                    , new Claim[]
                                    {
                                        new Claim
                                        (
                                            "ip"
                                            , remoteIpAddress.ToString()
                                        )
                                        ,
                                        new Claim
                                            (
                                                "Extension"
                                                , @"
                                                [
                                                    { R:""manger1111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111"",D:""HR111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111"" }
                                                    ,{ R:""manger1"",D:""HR1"" }
                                                ]"
                                            )
                                    }
                                    , "0123456789ABCDEF"
                                    , out var plainToken
                                    , out var secretToken
                                );

            var result = new JObject();
            result.Add
                    (
                        "User"
                        , userName
                     );
            result.Add
                    (
                        "SecretToken"
                        , secretToken
                     );
            return result;
        }
    }
}
