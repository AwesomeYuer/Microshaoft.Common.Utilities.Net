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
                    , IActionResult Result
                )
                    Validate
                        (
                            JToken
                                    parameters
                            , ActionExecutingContext
                                    actionExecutingContext
                        )
        {
            //var httpContext = actionExecutingContext.HttpContext;
            IActionResult result = null;
            var isValid = true;
            var jObject = parameters as JObject;
            if
                (
                    parameters != null
                    &&
                    jObject != null
                )
            {
                if
                    (
                        jObject
                            .TryGetValue
                                (
                                    "ccc"
                                    , StringComparison
                                            .OrdinalIgnoreCase
                                    , out _
                                )
                    )
                {
                    result = new JsonResult
                                    (
                                        new
                                        {
                                            statusCode = 400
                                            , resultCode = -400
                                            , message = "Bad Request, invalidate, must remove ccc"
                                        }
                                    )
                    {
                        StatusCode = 400
                        , ContentType = "application/json"
                    };
                    isValid = false;
                }
            }
            //else
            //{ 
            //    result = new JsonResult
            //    (
            //        new
            //        {
            //            statusCode = 400
            //            , resultCode = -400
            //            , message = "Bad Request, parameters should not be Null"
            //        }
            //    )
            //    {
            //        StatusCode = 400
            //        , ContentType = "application/json"
            //    };
            //}
            return
                (
                    IsValid : isValid
                    , Result : result
                );
        }
    }
}
