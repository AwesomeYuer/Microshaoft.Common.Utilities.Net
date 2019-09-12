#if NETFRAMEWORK4_X
// WebApi.MVC.CountPerformanceActionFilter.cs

namespace Microshaoft.WebApi
{
    using Microshaoft.Web;
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Web;
    using System.Web.Http.Filters;

    //using Microshaoft.WebApplications;
    public class WebApiExceptionsHandlerFilterAttribute : ExceptionFilterAttribute
    {
        private string _processName = Process.GetCurrentProcess().ProcessName;
        public string ProcessName
        {
            get
            {
                return _processName;
            }
        }



        public string LogFileRootDirectoryPath
        {
            set;
            get;
        }
        public int LogFileNameAlignSeconds
        {
            set;
            get;    
        }

        public string LogFileNamePrefix
        {
            set;
            get;
        }

        private const string _requestKey = "RequestID";

        private const string _logFileNamePrefixKey = "LogFileNamePrefix";

        
        //private const string _userHostAddressKey = "UserHostAddressKey";
        //private const string _userHostNameKey = "UserHostNameKey";

        private const string _contextLogKey = "ContextLogKey";



        public override void OnException(HttpActionExecutedContext actionExecutedContext)
        {
#region Request Log
            var actionContext = actionExecutedContext.ActionContext;
            HttpContext httpContext = HttpContext.Current;
                var userHostAddress = httpContext.Request.UserHostAddress;
                var userHostName = httpContext.Request.UserHostName;
                var requestFullUrl = actionContext
                                            .Request
                                            .RequestUri
                                            .ToString();
                var guid = Guid.NewGuid().ToString("N");
                var controllerName = actionContext
                                            .ControllerContext
                                            .ControllerDescriptor
                                            .ControllerName;
                var actionName = actionContext
                                            .ActionDescriptor
                                            .ActionName;
                var logFileNamePrefix = string
                                            .Format
                                                (
                                                    "{1}{0}{2}{0}{3}"
                                                    , "."
                                                    , controllerName
                                                    , actionName
                                                    , "Exceptions"
                                                );

                var contextLog = string
                                    .Format
                                        (
                                            "RequestFullUrl: [{1}]{0}From Remote Addr: {2}[{3}]{0}RequestID: [{4}]{0}Controller: [{5}]{0}Action: [{6}]"
                                            , "\r\n"
                                            , requestFullUrl
                                            , userHostAddress
                                            , userHostName
                                            , guid
                                            , controllerName
                                            , actionName
                                        );
                var controllerAttribute = actionContext
                                                    .ActionDescriptor
                                                    .ControllerDescriptor
                                                    .GetCustomAttributes<WebFileLogsFilter>()
                                                    .FirstOrDefault();
                if (controllerAttribute != null)
                {
                    if (!string.IsNullOrEmpty(controllerAttribute.LogFileNamePrefix))
                    {
                        logFileNamePrefix = controllerAttribute.LogFileNamePrefix;
                    }
                }
                var actionAttribute = actionContext
                                                .ActionDescriptor
                                                .GetCustomAttributes<WebFileLogsFilter>()
                                                .FirstOrDefault();
                if (actionAttribute != null)
                {
                    if (!string.IsNullOrEmpty(actionAttribute.LogFileNamePrefix))
                    {
                        logFileNamePrefix = actionAttribute.LogFileNamePrefix;
                    }
                }

                LogRequestResponseMessage
                    (
                        actionContext
                            .ControllerContext
                            .ControllerDescriptor
                            .ControllerName
                        , actionContext
                            .ActionDescriptor
                            .ActionName
                        , contextLog
                        , logFileNamePrefix
                        , actionExecutedContext
                    );
            

            
                //LogResponseMessage
                //    (
                //        actionContext
                //                .ControllerContext
                //                .ControllerDescriptor
                //                .ControllerName
                //        , actionContext
                //                .ActionDescriptor
                //                .ActionName
                //        , string.Format("{1}{0}{2}", " ", contextLog, "Response")
                //        , logFileNamePrefix
                //        , actionExecutedContext
                //                .Response
                //    );
            
#endregion
        }

        private void LogRequestResponseMessage
                          (
                              string controllerName
                              , string actionName
                              , string type
                              , string logFileNamePrefix
                              , HttpActionExecutedContext actionExecutedContext
                          )
        {
            var actionContext = actionExecutedContext.ActionContext;
            var httpRequestMessage = actionContext.Request;
            var log = string.Empty;
            if (httpRequestMessage != null)
            {
                var i = 0;
                var httpHeaders = httpRequestMessage
                                                .Headers;
                var result
                        = httpHeaders
                                    .SelectMany
                                        (
                                            (x) =>
                                            {
                                                var ii = 0;
                                                i++;
                                                return
                                                    x.Value
                                                        .Select
                                                            (
                                                                (xx) =>
                                                                {
                                                                    ii++;
                                                                    return
                                                                        string.Format
                                                                                (
                                                                                    "{0}{2}{1}{3}"
                                                                                    , (ii == 1 && i != 1 ? "\r\n" : "")
                                                                                    , (ii == 1 ? "" : ",")
                                                                                    , (ii == 1 ? x.Key + ": " : "")
                                                                                    , xx
                                                                                );
                                                                }
                                                            );
                                            }
                                        );
                var s = string
                            .Format
                                (
                                    "WebAPI Controler: {1}[Action: {2}]{0}[{0}{3}{0}]{0}{4}"
                                    , "\r\n\r\n"
                                    , controllerName
                                    , actionName
                                    , type
                                    , String.Join("", result)
                                );
                var httpContent = httpRequestMessage.Content;
                if (httpContent != null)
                {
                    s += "\r\n\r\n"
                            + httpContent
                                .ReadAsStringAsync()
                                .Result;
                }
                log += s;
            }
            var httpResponseMessage = actionExecutedContext.Response;
            if (httpResponseMessage != null)
            {
                var i = 0;
                var httpHeaders = httpResponseMessage.Headers;
                var result
                        = httpHeaders
                                .SelectMany
                                    (
                                        (x) =>
                                        {
                                            var ii = 0;
                                            i++;
                                            return
                                                x.Value
                                                    .Select
                                                        (
                                                            (xx) =>
                                                            {
                                                                ii++;
                                                                return
                                                                    string.Format
                                                                            (
                                                                                "{0}{2}{1}{3}"
                                                                                , (ii == 1 && i != 1 ? "\r\n" : "")
                                                                                , (ii == 1 ? "" : ",")
                                                                                , (ii == 1 ? x.Key + ": " : "")
                                                                                , xx
                                                                            );
                                                            }
                                                        );
                                        }
                                    );
                var s = string
                            .Format
                                (
                                    "WebAPI Controler: {1}[Action: {2}]{0}[{0}{3}{0}]{0}{4}"
                                    , "\r\n\r\n"
                                    , controllerName
                                    , actionName
                                    , type
                                    , string.Join("", result)
                                );
                var httpContent = httpResponseMessage.Content;
                if (httpContent != null)
                {
                    s += "\r\n\r\n"
                            + httpContent
                                .ReadAsStringAsync()
                                .Result;
                }
                var httpStatusCode = httpResponseMessage.StatusCode;
                s += "\r\n\r\n"
                        + string.Format
                                    (
                                        "HttpStatusCode = {0}({1})"
                                        , (int) httpStatusCode
                                        , httpStatusCode
                                    );
                log += s;

                
                
            }
            var caughtException = actionExecutedContext.Exception;
            if (caughtException != null)
            {
                type = "Exception";
                var s = "\r\n\r\n" +
                        string.Format
                            (
                                "WebAPI Controler: {1}[Action: {2}]{0}[{0}{3}{0}]{0}{4}"
                                , "\r\n\r\n"
                                , controllerName
                                , actionName
                                , type
                                , caughtException
                            );
                log += s;
                
            }
            FileLogHelper
                      .LogToTimeAlignedFile
                          (
                              log
                              , logFileNamePrefix
                              , LogFileRootDirectoryPath
                              , LogFileNameAlignSeconds
                           );


        }

    }

}

#endif
