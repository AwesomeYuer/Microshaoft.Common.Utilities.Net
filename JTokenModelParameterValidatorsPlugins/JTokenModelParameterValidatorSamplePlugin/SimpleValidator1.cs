namespace Microshaoft.JTokenParameterValidators
{
    using System;
    using System.Composition;
    using Microshaoft.Web;
    using Microsoft.AspNetCore.Mvc;
    using Newtonsoft.Json.Linq;

    [Export(typeof(IJTokenParameterValidator))]
    public class SimpleValidator1 : IJTokenParameterValidator
    {
        public string Name => this.GetType().Name;

        public (bool IsValid, IActionResult Result) Validate(JToken parameters)
        {
            JsonResult result = null;
            var isValid = true;
            var jObject = parameters as JObject;
            if (!jObject.TryGetValue("ccc", StringComparison.OrdinalIgnoreCase,out _))
            {
                result = new JsonResult
                                   (
                                       new
                                       {
                                           StatusCode = 400
                                           , Message = "invalidate ccc"
                                       }
                                   )
                {
                    StatusCode = 400
                    , ContentType = "application/json"
                };
                isValid = false;
            }
            return (isValid, result);
        }
    }
}
