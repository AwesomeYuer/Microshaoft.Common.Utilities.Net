namespace Microshaoft.Web.Samples
{
    using Microshaoft.WebApi.ModelBinders;
    using Microsoft.AspNetCore.Cors;
    using Microsoft.AspNetCore.Mvc;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Text.Encodings.Web;
    using System.Web;

    [Route("api/[controller]")]
    [ApiController]
    //启用跨域，指定允许的跨域策略
    [EnableCors("AllowAllOrigins")]

    public class AntiXssJsonpController : ControllerBase
    {
        HtmlEncoder _htmlEncoder;
        JavaScriptEncoder _javaScriptEncoder;
        UrlEncoder _urlEncoder;

        public AntiXssJsonpController
                        (
                            HtmlEncoder htmlEncoder
                            , JavaScriptEncoder javascriptEncoder
                            , UrlEncoder urlEncoder
                        )
        {
            _htmlEncoder = htmlEncoder;
            _javaScriptEncoder = javascriptEncoder;
            _urlEncoder = urlEncoder;
        }

        [HttpGet]
        [Route("jsonp-AntiXss")]
        public ActionResult Process
                        (
                            [ModelBinder(typeof(JTokenModelBinder))]
                                JToken parameters
                        )
        {
            var script = parameters["callback"].Value<string>();
            //Anti-XSS

            var callback = HttpUtility.JavaScriptStringEncode(script);
            Console.WriteLine(callback);
            callback = _javaScriptEncoder.Encode(script);
            Console.WriteLine(callback);

            var content = $"{callback}({parameters.ToString()})";
            
            return
                    Content
                        (
                              content
                              , "text/javascript"
                        );
        }
    }
}
