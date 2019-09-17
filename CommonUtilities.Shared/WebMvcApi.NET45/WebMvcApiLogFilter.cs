#if NETFRAMEWORK4_X
// WebApi.MVC.CountPerformanceActionFilter.cs
namespace Microshaoft.Web
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Net.Http;
    using System.Web;
    using System.Web.Http.Controllers;
    using System.Web.Http.Filters;

    //using Microshaoft.WebApplications;
    public class WebFileLogsFilter : ActionFilterAttribute
    {
        private string _processName = Process.GetCurrentProcess().ProcessName;
        public string ProcessName
        {
            get
            {
                return _processName;
            }
        }

        public Func<bool> GetEnableDebugLogProcessFunc
        {
            get;
            set;
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

        public override void OnActionExecuting(HttpActionContext actionContext)
        {
#region Request Log
            var enableDebugLog = false;
            if (GetEnableDebugLogProcessFunc != null)
            {
                enableDebugLog = GetEnableDebugLogProcessFunc();
            }
            if (enableDebugLog)
            {
                HttpContext httpContext = HttpContext.Current;
                var userHostAddress = httpContext.Request.UserHostAddress;
                var userHostName = httpContext.Request.UserHostName;
                var requestFullUrl = actionContext
                                            .Request
                                            .RequestUri
                                            .ToString();
                //actionContext
                //           .Request
                //           .Properties[_userHostAddressKey] = userHostAddress;

                //actionContext
                //           .Request
                //           .Properties[_userHostNameKey] = userHostName;

                var guid = Guid.NewGuid().ToString("N");
                //actionContext
                //            .Request
                //            .Properties[_requestKey] = guid;


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
                                                    "{1}{0}{2}"
                                                    , "."
                                                    , controllerName
                                                    , actionName
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
                actionContext
                        .Request
                        .Properties[_contextLogKey] = contextLog;

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

                actionContext
                       .Request
                       .Properties[_logFileNamePrefixKey] = logFileNamePrefix;

                LogRequestMessage
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
                        , actionContext
                            .Request
                    );
            }
#endregion
        }


        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        {
#region Response Log
            var enableDebugLog = false;
            if (GetEnableDebugLogProcessFunc != null)
            {
                enableDebugLog = GetEnableDebugLogProcessFunc();
            }
            if (enableDebugLog)
            {
                var actionContext = actionExecutedContext
                                                    .ActionContext;
                var logFileNamePrefix = actionContext
                                                    .Request
                                                    .Properties[_logFileNamePrefixKey] as string;
                //string guid = actionContext
                //                        .Request
                //                        .Properties[_requestKey] as string;



                //var userHostAddress = actionContext
                //                                   .Request
                //                                   .Properties[_userHostAddressKey] as string;
                //var userHostName = actionContext
                //                               .Request
                //                               .Properties[_userHostNameKey] as string;

                var contextLog = actionContext
                                        .Request
                                        .Properties[_contextLogKey] as string;
                LogResponseMessage
                    (
                        actionContext
                                .ControllerContext
                                .ControllerDescriptor
                                .ControllerName
                        , actionContext
                                .ActionDescriptor
                                .ActionName
                        , string.Format("{1}{0}{2}", " ", contextLog, "Response")
                        , logFileNamePrefix
                        , actionExecutedContext
                                .Response
                    );
            }
#endregion
        }

        private void LogRequestMessage
                          (
                              string controllerName
                              , string actionName
                              , string type
                              , string logFileNamePrefix
                              , HttpRequestMessage httpRequestMessage
                          )
        {
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
                FileLogHelper
                        .LogToTimeAlignedFile
                            (
                                s
                                , logFileNamePrefix
                                , LogFileRootDirectoryPath
                                , LogFileNameAlignSeconds
                             );
            }
        }

        private void LogResponseMessage
            (
                string controllerName
                , string actionName
                , string type
                , string logFileNamePrefix
                , HttpResponseMessage httpResponseMessage
            )
        {
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
                                    , String.Join("", result)
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
                FileLogHelper
                      .LogToTimeAlignedFile
                          (
                              s
                              , logFileNamePrefix
                              , LogFileRootDirectoryPath
                              , LogFileNameAlignSeconds
                           );
            }
            
  
        }
        //private static bool Skip(HttpActionContext actionContext)
        //{
        //    return
        //            actionContext
        //                .ActionDescriptor
        //                .GetCustomAttributes<PerformanceCounterAttribute>()
        //                .Any()
        //            ||
        //            actionContext
        //                .ControllerContext
        //                .ControllerDescriptor
        //                .GetCustomAttributes<PerformanceCounterAttribute>()
        //                .Any();
        //}
    }

}

#endif
