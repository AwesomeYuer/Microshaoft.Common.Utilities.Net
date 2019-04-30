#if NETCOREAPP2_X
namespace Microshaoft.Web
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Filters;
    using Microsoft.Extensions.Configuration;
    using System;
    public class OperationsAuthorizeFilter
                    :
                        //AuthorizeAttribute
                        Attribute
                        , IActionFilter
    {



        private IConfiguration _configuration;
        private object _locker = new object();
        public OperationsAuthorizeFilter()
        {
            Initialize();
        }
        public virtual void Initialize()
        {
            //InstanceID = Interlocked.Increment(ref InstancesSeed);
        }

        public virtual void OnActionExecuting(ActionExecutingContext context)
        {
            var ok = false;
            var errorMessage = string.Empty;
            var errorStatusCode = -1;
            
            void ErrorResult()
            {
                context
                    .Result = new JsonResult
                                    (
                                        new
                                        {
                                            StatusCode = errorStatusCode
                                            , Message = errorMessage
                                        }
                                    )
                                {
                                    StatusCode = errorStatusCode
                                    , ContentType = "application/json"
                                };
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
            var routeName = (string)context.ActionArguments["routeName"];
            var httpMethod = $"http{request.Method}";
            var operationsConfiguration = _configuration
                                                    .GetSection
                                                        ($"Routes:{routeName}:{httpMethod}:Operations");
            if (operationsConfiguration.Exists())
            {
                var operations = operationsConfiguration.Get<string[]>();
                var userName = "anonymous";
                var user = httpContext.User;
            }
        }
        public virtual void OnActionExecuted(ActionExecutedContext context)
        {

        }
    }
}
#endif