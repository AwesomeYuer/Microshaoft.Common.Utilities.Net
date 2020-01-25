#if NETCOREAPP
namespace Microshaoft.Web
{
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Filters;
    using Microsoft.Extensions.Configuration;
    using System;
    public interface ICheckUserOperations
    {
        bool CheckUserOperations(HttpContext httpContext, string[] operations);
    }

    public class OperationsAuthorizeFilter
                    :
                        //AuthorizeAttribute
                        Attribute
                        , IActionFilter
                        , ICheckUserOperations
    {
        private IConfiguration _configuration;
        private object _locker = new object();
        private bool _allowDefault = false;
        public string AccessingConfigurationKey { get; set; } = "DefaultAccessing";
        public OperationsAuthorizeFilter
                        (
                            bool allowDefault = false
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
            var routeName = (string) context.ActionArguments["routeName"];
            var httpMethod = $"http{request.Method}";
            var allow = _allowDefault;
            var success = _configuration
                                    .TryGetSection
                                        (
                                            $"Routes:{routeName}:{httpMethod}:{AccessingConfigurationKey}"
                                            , out var masterConfiguration
                                        );
            if (success)
            {
                success = masterConfiguration
                                        .TryGetSection
                                                (
                                                    $"allow"
                                                    , out var configuration
                                                );
                if (success)
                {
                    allow = configuration.Get<bool>();
                }
                if (allow)
                {
                    var needCheckOperations = false;
                    success = masterConfiguration
                                            .TryGetSection
                                                    (
                                                        $"needcheckoperations"
                                                        , out configuration
                                                    );
                    if (success)
                    {
                        needCheckOperations = configuration.Get<bool>();
                    }
                    if (needCheckOperations)
                    {
                        success = masterConfiguration
                                                .TryGetSection
                                                    (
                                                        $"operations"
                                                        , out configuration
                                                    );
                        if (success)
                        {
                            var operations = configuration
                                                    .Get<string[]>();
                            allow = CheckUserOperations
                                            (
                                                httpContext
                                                , operations
                                            );
                            if (!allow)
                            {
                                forbiddenMessage = $"forbidden by {configuration.Key}";
                                setForbidResult();
                            }
                        }
                    }
                }
                else //(!allow)
                {
                    forbiddenMessage = $"forbidden by {configuration.Key}";
                    setForbidResult();
                }
            }
        }
        public virtual void OnActionExecuted(ActionExecutedContext context)
        {

        }

        public bool CheckUserOperations(HttpContext httpContext, string[] operations)
        {
            //to do

            //var userName = "anonymous";
            var user = httpContext.User;
            return true;
        }
    }
}
#endif