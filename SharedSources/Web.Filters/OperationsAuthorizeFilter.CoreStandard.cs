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
        //private bool _allowDefault = false;
        public string AccessingConfigurationKey { get; set; } = "DefaultAccessing";
        public OperationsAuthorizeFilter
                        (
                            //bool allowDefault = false
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
                                out var actionRoutePath
                            )
                )
            {
                var httpMethod = $"http{request.Method}";
                var allow = false; // _allowDefault;
                var success = _configuration
                                        .TryGetSection
                                            (
                                                $"Routes:{actionRoutePath}:{httpMethod}:{AccessingConfigurationKey}"
                                                , out var masterConfiguration
                                            );
                if (success)
                {
                    bool needRequestResponseLogging = masterConfiguration
                                                                    .GetValue
                                                                        (
                                                                            nameof(needRequestResponseLogging)
                                                                            , true
                                                                        );
                    httpContext
                            .Items
                            .Add
                                (
                                    nameof(needRequestResponseLogging)
                                    , needRequestResponseLogging
                                );

                    allow = masterConfiguration
                                            .GetValue
                                                    (
                                                        $"allow"
                                                        , false
                                                    );




                    if (allow)
                    {
                        bool needCheckOperations = masterConfiguration
                                                    .GetValue
                                                        (
                                                            nameof(needCheckOperations)
                                                            , false
                                                        );
                        if (needCheckOperations)
                        {
                            string[] operations = masterConfiguration
                                                        .GetOrDefault<string[]>
                                                            (
                                                                nameof(operations)
                                                            );
                            allow = CheckUserOperations
                                            (
                                                httpContext
                                                , operations
                                            );
                            if (!allow)
                            {
                                forbiddenMessage = $"forbidden by {masterConfiguration.Key} check Operations";
                                setForbidResult();
                            }
                        }
                    }
                    else //(!allow)
                    {
                        forbiddenMessage = $"forbidden by {masterConfiguration.Key}";
                        setForbidResult();
                    }
                }
            }
            else
            {
                context
                    .SetNotFoundJsonResult();
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