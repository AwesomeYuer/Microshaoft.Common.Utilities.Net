#if NETCOREAPP2_X
namespace Microshaoft.Web
{
    using Microsoft.AspNetCore.Mvc.Filters;
    using Microsoft.Extensions.Configuration;
    using Newtonsoft.Json.Linq;
    using System;
    public class RequestJTokenParametersDefaultProcessFilterAttribute
                                :
                                    //AuthorizeAttribute
                                    Attribute
                                    , IActionFilter
    {
        private IConfiguration _configuration;
        private object _locker = new object();
        public RequestJTokenParametersDefaultProcessFilterAttribute()
        {
            Initialize();
        }
        public virtual void Initialize()
        {
        }
        public virtual void OnActionExecuting(ActionExecutingContext context)
        {
            var httpContext = context.HttpContext;
            var request = httpContext.Request;
            var httpMethod = $"http{request.Method}";
            var routeName = (string) context.ActionArguments["routeName"];
            JObject parameters = (JObject) context.ActionArguments["parameters"];
            _locker
                .LockIf
                    (
                        () =>
                        {
                            return
                                (_configuration == null);
                        }
                        , () =>
                        {
                            _configuration = (IConfiguration)
                                                httpContext
                                                    .RequestServices
                                                    .GetService
                                                        (
                                                            typeof(IConfiguration)
                                                        );
                        }
                    );
            var inputsParametersConfiguration =
                    _configuration
                            .GetSection
                                ($"Routes:{routeName}:{httpMethod}:InputsParameters");
            if (inputsParametersConfiguration.Exists())
            {
                var inputsParameters = inputsParametersConfiguration.GetChildren();
                foreach (var inputParameter in inputsParameters)
                {
                    var parameterName = inputParameter
                                                .GetValue<string>("Name");
                    var allowOverride = false;
                    var r = parameters
                                .TryGetValue
                                    (
                                        parameterName
                                        , StringComparison
                                                .OrdinalIgnoreCase
                                        , out _
                                    );
                    if (r)
                    {
                        allowOverride = inputParameter
                                            .GetValue<bool>("allowOverride");
                    }
                    if 
                        (
                            (!r)
                            ||
                            (r && !allowOverride)
                        )
                    {
                        object parameterValue = inputParameter.GetValue<object>("Value");
                        parameters[parameterName] = new JValue(parameterValue);
                    }
                }
            }
            httpContext = null;
            request = null;
        }
        public virtual void OnActionExecuted(ActionExecutedContext context)
        {

        }
    }
}
#endif