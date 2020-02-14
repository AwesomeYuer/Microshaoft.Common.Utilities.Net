#if NETCOREAPP //|| NETSTANDARD2_X
namespace Microshaoft.Web
{
    using Microsoft.AspNetCore.Mvc.Filters;
    using Microsoft.Extensions.Configuration;
    using Newtonsoft.Json.Linq;
    using System;
    public class RequestJTokenParametersProcessFilterAttribute
                                :
                                    //AuthorizeAttribute
                                    Attribute
                                    , IActionFilter
    {
        private IConfiguration _configuration;
        private object _locker = new object();

        public string AccessingConfigurationKey { get; set; } = "DefaultAccessing";

        public RequestJTokenParametersProcessFilterAttribute()
        {
            Initialize();
        }
        public virtual void Initialize()
        {
        }
        public virtual void OnActionExecuting(ActionExecutingContext context)
        {
            var httpContext = context.HttpContext;
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
            var request = httpContext.Request;
            //var actionRoutePath = (string) context.ActionArguments["actionRoutePath"];
            if
                (
                    request
                        .TryGetActionRoutePath
                            (
                                out var actionRoutepath
                            )
                )
            {
                var httpMethod = $"http{request.Method}";
                JObject parameters = (JObject) context.ActionArguments["parameters"];

                if
                    (
                        _configuration
                            .TryGetSection
                                (
                                    $"Routes:{actionRoutepath}:{httpMethod}:{AccessingConfigurationKey}:InputsParameters"
                                    , out var inputsParametersConfiguration
                                )
                    )
                {
                    var inputsParameters = inputsParametersConfiguration.GetChildren();
                    foreach (var inputParameter in inputsParameters)
                    {
                        var parameterName = inputParameter
                                                    .GetValue<string>("Name");
                        var allowOverride = false;
                        if (parameters == null)
                        {
                            parameters = new JObject();
                        }

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