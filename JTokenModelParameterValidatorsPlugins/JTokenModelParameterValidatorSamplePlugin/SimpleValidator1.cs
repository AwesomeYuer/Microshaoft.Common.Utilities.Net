using System;
using System.Composition;
using Microshaoft.Web;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace Microshaoft.JTokenParameterValidators
{
    [Export(typeof(IJTokenModelParameterValidator))]
    public class SimpleValidator1 : IJTokenModelParameterValidator
    {
        //private string _name = ;
        public string Name => this.GetType().Name;

        public (bool IsValid, JsonResult result) Validate(JToken parameters)
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
                                           ,
                                           Message = "效验不通过ccc"
                                       }
                                   )
                {
                    StatusCode = 400
                                   ,
                    ContentType = "application/json"
                };
                isValid = false;
            }


           
            return (isValid, result);
        }
    }
}
