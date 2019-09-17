#if NETFRAMEWORK4_X
namespace Microshaoft.WebMvc
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
namespace Microshaoft
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Web.Http.Controllers;
    using System.Web.Http.Filters;
    public class WebPerformanceCountersFilter : ActionFilterAttribute
    {
        private string _processName = Process
                                            .GetCurrentProcess()
                                            .ProcessName;
        public string ProcessName
        {
            get
            {
                return _processName;
            }
        }
        public string ControllerCategoryNamePrefix
        {
            get;
            set;
        }
        private string ActionInstanceNamePrefix
        {
            get;
            set;
        }
        public Func<bool> OnCheckEnabledCountPerformanceProcessFunc
        {
            get;
            set;
        }
        private const string _controllerStopwatchKey = "ControllerStopwatch";
        private const string _actionStopwatchKey = "ActionStopwatch";

        private const string _controllerPerformanceCounterCategoryNameKey = "ControllerPerformanceCounterCategoryName";
        private const string _controllerPerformanceCounterInstanceNameKey = "ControllerPerformanceCounterInstanceName";

        private const string _actionPerformanceCounterAttributeCategoryNameKey = "actionPerformanceCounterAttributeCategoryName";
        private const string _actionPerformanceCounterAttributeInstanceNameKey = "actionPerformanceCounterAttributeInstanceName";

        private MultiPerformanceCountersTypeFlags
                            _enabledCounters
                                        = MultiPerformanceCountersTypeFlags.ProcessCounter
                                            | MultiPerformanceCountersTypeFlags.ProcessedAverageTimerCounter
                                            | MultiPerformanceCountersTypeFlags.ProcessedCounter
                                            | MultiPerformanceCountersTypeFlags.ProcessedRateOfCountsPerSecondCounter
                                            | MultiPerformanceCountersTypeFlags.ProcessingCounter;

        public MultiPerformanceCountersTypeFlags EnabledCounters
        {
            get
            {
                return _enabledCounters;
            }

            set
            {
                _enabledCounters = value;
            }
        }

        public override void OnActionExecuting(HttpActionContext actionContext)
        {
#region 计数器
            var controllerPerformanceCounterCategoryName
                                = string
                                    .Format
                                        (
                                            "{1}{0}{2}"
                                            , "-"
                                            , ControllerCategoryNamePrefix
                                            , actionContext
                                                .ActionDescriptor
                                                .ControllerDescriptor
                                                .ControllerName
                                        );

            var controllerPerformanceCounterInstanceName = "*";

            var controllerAttribute = actionContext
                                            .ActionDescriptor
                                            .ControllerDescriptor
                                            .GetCustomAttributes<CommonPerformanceCounterAttribute>()
                                            .FirstOrDefault();
            if (controllerAttribute != null)
            {
                if (!string.IsNullOrEmpty(controllerAttribute.PerformanceCounterCategoryName))
                {
                    controllerPerformanceCounterCategoryName = controllerAttribute.PerformanceCounterCategoryName;
                }
                if (!string.IsNullOrEmpty(controllerAttribute.PerformanceCounterInstanceName))
                {
                    controllerPerformanceCounterInstanceName = controllerAttribute.PerformanceCounterInstanceName;
                }
            }
            actionContext
                   .Request
                   .Properties[_controllerPerformanceCounterCategoryNameKey] = controllerPerformanceCounterCategoryName;
            actionContext
                   .Request
                   .Properties[_controllerPerformanceCounterInstanceNameKey] = controllerPerformanceCounterInstanceName;
            
            CountPerformanceBegin
                        (
                            actionContext
                            , controllerPerformanceCounterCategoryName
                            , controllerPerformanceCounterInstanceName
                            , _controllerStopwatchKey
                        );


            var actionPerformanceCounterCategoryName
                                = string
                                        .Format
                                            (
                                                "{1}{0}{2}"
                                                , "-"
                                                , ControllerCategoryNamePrefix
                                                , actionContext
                                                    .ActionDescriptor
                                                    .ControllerDescriptor
                                                    .ControllerName
                                            );

            var actionPerformanceCounterInstanceName
                                = string.Format
                                            (
                                                "{1}{0}{2}{0}{3}{0}{4}{0}{5}"
                                                , "-"
                                                , ActionInstanceNamePrefix
                                                , actionContext
                                                    .ActionDescriptor
                                                    .ControllerDescriptor
                                                    .ControllerName
                                                , actionContext
                                                    .ActionDescriptor
                                                    .ActionName
                                                , actionContext.Request.Method.Method
                                                , ProcessName
                                            );
            

            var actionAttribute = actionContext
                                            .ActionDescriptor
                                            .GetCustomAttributes<CommonPerformanceCounterAttribute>()
                                            .FirstOrDefault();
            if (actionAttribute != null)
            {
                if (!string.IsNullOrEmpty(actionAttribute.PerformanceCounterCategoryName))
                {
                    actionPerformanceCounterCategoryName = actionAttribute.PerformanceCounterCategoryName;
                }
                if (!string.IsNullOrEmpty(actionAttribute.PerformanceCounterInstanceName))
                {
                    actionPerformanceCounterInstanceName = actionAttribute.PerformanceCounterInstanceName;
                }
            }
            actionContext
                   .Request
                   .Properties[_actionPerformanceCounterAttributeCategoryNameKey] = actionPerformanceCounterCategoryName;
            actionContext
                   .Request
                   .Properties[_actionPerformanceCounterAttributeInstanceNameKey] = actionPerformanceCounterInstanceName;

            CountPerformanceBegin
                        (
                            actionContext
                            , actionPerformanceCounterCategoryName
                            , actionPerformanceCounterInstanceName
                            , _actionStopwatchKey
                        );

#endregion
        }
        private void CountPerformanceBegin
                                (
                                    HttpActionContext actionContext
                                    , string performanceCountersCategoryName
                                    , string performanceCountersInstanceName
                                    , string stopwatchKey
                                )
        {
            var enableCountPerformance = false;
            if (OnCheckEnabledCountPerformanceProcessFunc != null)
            {
                enableCountPerformance = OnCheckEnabledCountPerformanceProcessFunc();
            }
            if (enableCountPerformance)
            {
                Stopwatch stopwatch =
                                EasyPerformanceCountersHelper<CommonPerformanceCountersContainer>
                                        .CountPerformanceBegin
                                            (
                                                EnabledCounters
                                                , performanceCountersCategoryName
                                                , performanceCountersInstanceName
                                                , () =>
                                                {
                                                    return enableCountPerformance;
                                                }

                                            );

                if (stopwatch != null)
                {
                    actionContext
                        .Request
                        .Properties[stopwatchKey] = stopwatch;
                }
            }
        }
        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        {
            CountPerformanceEnd
                        (
                            actionExecutedContext
                            , _actionStopwatchKey
                            , _actionPerformanceCounterAttributeCategoryNameKey
                            , _actionPerformanceCounterAttributeInstanceNameKey
                        );

            CountPerformanceEnd
                        (
                            actionExecutedContext
                            , _controllerStopwatchKey
                            , _controllerPerformanceCounterCategoryNameKey
                            , _controllerPerformanceCounterInstanceNameKey
                        );
        }
        private void CountPerformanceEnd
                                (
                                    HttpActionExecutedContext actionExecutedContext
                                    , string stopwatchKey
                                    , string performanceCountersCategoryNameKey
                                    , string performanceCountersInstanceNameKey
                                )
        {
            if (actionExecutedContext.Request.Properties.ContainsKey(stopwatchKey))
            {
                var stopwatch = actionExecutedContext
                                        .Request
                                        .Properties[stopwatchKey] as Stopwatch;
                if (stopwatch != null)
                {
                    var enableCountPerformance = false;
                    if (OnCheckEnabledCountPerformanceProcessFunc != null)
                    {
                        enableCountPerformance = OnCheckEnabledCountPerformanceProcessFunc();
                    }
                    if (enableCountPerformance)
                    {
                        var performanceCountersCategoryName
                                                = actionExecutedContext
                                                            .Request
                                                            .Properties[performanceCountersCategoryNameKey] as string;
                        var performanceCountersInstanceName = actionExecutedContext
                                                                    .Request
                                                                    .Properties[performanceCountersInstanceNameKey] as string;
                        if
                            (
                                !string.IsNullOrEmpty(performanceCountersCategoryName)
                                &&
                                !string.IsNullOrEmpty(performanceCountersInstanceName)
                            )
                        {
                            EasyPerformanceCountersHelper<CommonPerformanceCountersContainer>
                                    .CountPerformanceEnd
                                        (
                                            EnabledCounters
                                            , performanceCountersCategoryName
                                            , performanceCountersInstanceName
                                            , stopwatch
                                        );
                        }
                    }
                    if (stopwatch != null)
                    {
                        stopwatch = null;
                    }
                }
            }
        }
    }
    //[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = true)]
    //public class WebPerformanceCounterAttribute : Attribute
    //{
    //    public string PerformanceCounterCategoryName;
    //    public string PerformanceCounterInstanceName;
    //}


    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = true)]
    public class CommonPerformanceCounterAttribute : Attribute
    {
        public string PerformanceCounterCategoryName;
        public string PerformanceCounterInstanceName;
    }
}

#endif
