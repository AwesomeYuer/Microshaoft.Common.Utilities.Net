#if NETCOREAPP2_X
namespace Microshaoft.Web
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Filters;
    using System;
    using System.Collections.Generic;
    using System.Text;
    public class CustomExceptionFilter : Attribute, IExceptionFilter
    {
        public void OnException(ExceptionContext context)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("发生了异常：{0}", context.Exception.Message);
            Console.ForegroundColor = ConsoleColor.Gray;
            context.Result = new JsonResult
                                    (
                                        new
                                        {
                                            statusCode = 500
                                            , message = context.Exception.Message
                                        }
                                    )
                                {
                                    StatusCode = 500
                                    , ContentType = "application/json"
                                };
            context.ExceptionHandled = true;
        }
        public class CustomResultFilter : Attribute, IResultFilter
        {
            public void OnResultExecuted(ResultExecutedContext context)
            {
                Console.WriteLine("OnResultExecuted");
            }
            public void OnResultExecuting(ResultExecutingContext context)
            {
                Console.WriteLine("OnResultExecuting");
            }
        }
    }
}
#endif