namespace Microsoft.Boc.WebMvc
{
    using System.Web.Mvc;

    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }
    }
}

// WebApi.MVC.CountPerformanceActionFilter.cs
namespace Microsoft.Boc.Web
{
    using Microsoft.Boc.Communication.Configurations;
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Web.Http.Controllers;
    using System.Web.Http.Filters;
    //using Microshaoft.WebApplications;
    public class CountPerformanceActionFilter : ActionFilterAttribute
    {
        private const string _stopwatchKey = "StopWatch";
        private const string _requestKey = "RequestID";
        private string _performanceCountersCategoryName
                                        = WebMvcApiPerformanceCountersConfiguration
                                            .PerformanceCountersCategoryName;
        private string _performanceCountersCategoryInstanceName
                                        = WebMvcApiPerformanceCountersConfiguration
                                            .PerformanceCountersCategoryInstanceName;
        private MultiPerformanceCountersTypeFlags _enableCounters
                                        = WebMvcApiPerformanceCountersConfiguration
                                            .EnableCounters;

        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            ///			if (SkipLogging(actionContext))
            ///			{
            ///				return;
            ///			}
            ///			
            if (ConfigurationAppSettingsManager.RunTimeAppSettings.EnableCountPerformance)
            {
                Stopwatch stopwatch =
                                EasyPerformanceCountersHelper<CommonPerformanceCountersContainer>
                                        .CountPerformanceBegin
                                            (
                                                _enableCounters
                                                , _performanceCountersCategoryName
                                                , _performanceCountersCategoryInstanceName
                                            );

                if (stopwatch != null)
                {
                    actionContext.Request.Properties[_stopwatchKey] = stopwatch;
                }
            }
            
            #region Request Log
            if (ConfigurationAppSettingsManager.RunTimeAppSettings.EnableDebugLog)
            {

                #region Response Log
                if 
                    (
                        ConfigurationAppSettingsManager
                            .RunTimeAppSettings
                            .EnableDebugLog
                    )
                {
                    var guid = Guid.NewGuid().ToString("N");
                    actionContext
                        .Request
                        .Properties[_requestKey] = guid;
                    LogMessage
                        (
                            actionContext.Request.Headers
                            , actionContext
                                .ControllerContext
                                .ControllerDescriptor
                                .ControllerName
                            , actionContext
                                .ActionDescriptor
                                .ActionName
                            , string.Format("RequestID:[{0}]", guid)
                            , actionContext
                                .Request
                                .Content
                        );
                }
                #endregion

                
            } 
            #endregion
   
            
        }
        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        {
            if (!actionExecutedContext.Request.Properties.ContainsKey(_stopwatchKey))
            {
                return;
            }
            var stopwatch = actionExecutedContext
                                    .Request
                                    .Properties[_stopwatchKey] as Stopwatch;
            if (stopwatch != null)
            {
                //var actionName = actionExecutedContext
                //                        .ActionContext
                //                        .ActionDescriptor
                //                        .ActionName;
                //var controllerName = actionExecutedContext
                //                        .ActionContext
                //                        .ActionDescriptor
                //                        .ControllerDescriptor
                //                        .ControllerName;

                //Debug.Print
                //        (
                //            string.Format
                //                    (
                //                        "[Execution of Controller: {1}, {0}Action: {2}, {0}took(ms) {3}.]"
                //                        , ""
                //                        , controllerName
                //                        , actionName
                //                        , stopwatch.ElapsedMilliseconds
                //                    )
                //        );
                if (ConfigurationAppSettingsManager.RunTimeAppSettings.EnableCountPerformance)
                {
                    EasyPerformanceCountersHelper<CommonPerformanceCountersContainer>
                        .CountPerformanceEnd
                            (
                                _enableCounters
                                , _performanceCountersCategoryName
                                , _performanceCountersCategoryInstanceName
                                , stopwatch
                            );
                }
                if (stopwatch != null)
                {
                    stopwatch = null;
                }
                //var s = actionExecutedContext
                //            .Response
                //            .Content
                //            .ReadAsStringAsync()
                //            .Result;

                #region Response Log
                if 
                    (
                        ConfigurationAppSettingsManager
                            .RunTimeAppSettings
                            .EnableDebugLog
                    )
                {
                    var httpActionContext = actionExecutedContext
                                                .ActionContext;
                    string guid = httpActionContext
                                    .Request
                                    .Properties[_requestKey] as string;
                    LogMessage
                        (
                            actionExecutedContext.Response.Headers
                            , httpActionContext
                                .ControllerContext
                                .ControllerDescriptor
                                .ControllerName
                            , httpActionContext
                                .ActionDescriptor
                                .ActionName
                            , string.Format("RequestID:[{0}] Response", guid)
                            , actionExecutedContext
                                .Response
                                .Content
                            , actionExecutedContext
                                .Response
                                .StatusCode
                        );
                }
                #endregion
            }
        }

        private void LogMessage
            (
                HttpHeaders httpHeaders
                , string controllerName
                , string actionName
                , string type
                , HttpContent httpContent
                , HttpStatusCode? httpStatusCode = null
            )
        {
            var i = 0;
            var result = httpHeaders
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
                                "WebAPI Controler:{1}[Action:{2}]-[{3}]{0}{4}"
                                , "\r\n\r\n"
                                , controllerName
                                , actionName
                                , type
                                , String.Join("", result)
                            );
            if (httpContent != null)
            {
                s += "\r\n\r\n"
                        + httpContent
                            .ReadAsStringAsync()
                            .Result;
            }
            if (httpStatusCode != null)
            {
                if (httpStatusCode.HasValue)
                {
                    var status = httpStatusCode.Value;
                    s += "\r\n\r\n"
                            + string.Format
                                        (
                                            "HttpStatusCode = {0}({1})"
                                            , (int) status
                                            , status
                                        );
                }
            }
            FileLogHelper
                .LogToTimeAlignedFile
                    (
                        s
                        , "WebAPI"
                        , ConfigurationAppSettingsManager
                            .RunTimeAppSettings
                            .LogFileRootDirectoryPath
                        , ConfigurationAppSettingsManager
                            .RunTimeAppSettings
                            .LogFileNameAlignSeconds
                     );
        }
        private static bool Skip(HttpActionContext actionContext)
        {
            return
                    actionContext
                        .ActionDescriptor
                        .GetCustomAttributes<CountPerformanceAttribute>()
                        .Any()
                    ||
                    actionContext
                        .ControllerContext
                        .ControllerDescriptor
                        .GetCustomAttributes<CountPerformanceAttribute>()
                        .Any();
        }
    }
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = true)]
    public class CountPerformanceAttribute : Attribute
    {
    }
}
