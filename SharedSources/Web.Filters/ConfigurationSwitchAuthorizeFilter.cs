#if NETCOREAPP
namespace Microshaoft.Web
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Filters;
    using Microsoft.Extensions.Configuration;
    using System;
    using System.Linq;

    public class ConfigurationSwitchAuthorizeFilter
                    :
                        //AuthorizeAttribute
                        Attribute
                        , IActionFilter
    {
        private IConfiguration _configuration;
        private object _locker = new object();
        private string[] _allows;
        public ConfigurationSwitchAuthorizeFilter
                        (
                        )
        {
            Initialize();
        }
        public virtual void Initialize()
        {
            //InstanceID = Interlocked.Increment(ref InstancesSeed);
        }

        public virtual void OnActionExecuting(ActionExecutingContext context)
        {
            var forbiddenMessage = string.Empty;
            void setForbidResult()
            {
                var statusCode = 403;
                var result = new JsonResult
                    (
                        new
                        {
                            statusCode
                            , resultCode = -1 * statusCode
                            , message = forbiddenMessage
                        }
                    )
                {
                    StatusCode = statusCode
                    , ContentType = "application/json"
                };
                context
                    .Result = result;
            }
            var httpContext = context.HttpContext;
            var request = httpContext.Request;
            _locker
                .LockIf
                    (
                        () =>
                        {
                            return
                                (
                                    _configuration == null
                                    ||
                                    _allows == null
                                );
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
                            _allows = _configuration
                                            .GetOrDefault($"AuthorizedAccessPaths", new string[] { })
                                            .Select
                                                (
                                                    (x) =>
                                                    {
                                                        x = x
                                                                .ToLower()
                                                                .Trim()
                                                                .Trim('/');
                                                        x = $"/{x}/";
                                                        return
                                                                x;
                                                    }
                                                )
                                            .OrderByDescending
                                                (
                                                    (x) =>
                                                    {
                                                        return
                                                            x;
                                                    }
                                                )
                                            .ToArray();
                        }
                    );
            var path = request
                            .Path
                            .Value
                            .Trim()
                            .Trim('/');
            path = $"/{path}/";
            var allow = _allows
                                .Any
                                    (
                                        (x) =>
                                        {
                                            return
                                                path
                                                    .StartsWith
                                                        (
                                                            x
                                                            , StringComparison
                                                                    .OrdinalIgnoreCase
                                                        );
                                        }
                                    );
            if (!allow)
            {
                forbiddenMessage = $"forbidden by configuration key : {path}";
                setForbidResult();
            }
        }
        public virtual void OnActionExecuted(ActionExecutedContext context)
        {

        }
    }
}
#endif