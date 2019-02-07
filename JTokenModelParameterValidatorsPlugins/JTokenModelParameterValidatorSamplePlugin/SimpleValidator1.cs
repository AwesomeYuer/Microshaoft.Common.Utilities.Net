namespace Microshaoft.JTokenParameterValidators
{
    using Microshaoft.Web;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Filters;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Composition;

    [Export(typeof(IHttpRequestValidateable<JToken>))]
    public class SimpleValidator1 : IHttpRequestValidateable<JToken>
    {
        public string Name
        {
            private set;
            get;
        }
        public SimpleValidator1()
        {
            Name = GetType().Name;
        }
        public 
                (
                    bool IsValid
                    ,
                    IActionResult Result
                )
                    Validate
                        (
                            JToken parameters
                            ,
                            ActionExecutingContext actionExecutingContext
                        )
        {
            var httpContext = actionExecutingContext.HttpContext;
            var request = httpContext.Request;

            IActionResult result = null;
            var isValid = true;
            var jObject = parameters as JObject;
            if 
                (
                    jObject
                        .TryGetValue
                            (
                                "ccc"
                                , StringComparison
                                        .OrdinalIgnoreCase
                                ,out _
                            )
                )
            {
                result = new JsonResult
                                (
                                    new
                                    {
                                        StatusCode = 400
                                        , Message = "invalidate, must remove ccc"
                                    }
                                )
                {
                    StatusCode = 400
                    , ContentType = "application/json"
                };
                isValid = false;
            }
            httpContext = null;
            request = null;
            return
                (
                    IsValid: isValid
                    ,
                    Result: result
                );
        }
    }
}
