#if NETFRAMEWORK4_X
namespace Microshaoft.Web
{
    using System.Diagnostics;
    using System.Linq;
    using System.Web.Http;
    using System.Web.Http.Description;
    public static class WebMvcApiPerformanceCountersHelper
    {
        public static void AttachPerformanceCounters
                                        (
                                            HttpConfiguration httpConfiguration
                                            , string controllerCategoryNamePrefix = null
                                            , string actionInstanceNamePrefix = null
                                        )
        {
            IApiExplorer apiExplorer = httpConfiguration
                                                    .Services
                                                    .GetApiExplorer();
            var apiDescriptions = apiExplorer
                                            .ApiDescriptions;
            var groups = apiDescriptions
                            .ToLookup
                                (
                                    (x) =>
                                    {
                                        return
                                            x
                                                .ActionDescriptor
                                                .ControllerDescriptor
                                                .ControllerName;
                                    }
                                );
            var processName = Process
                                    .GetCurrentProcess()
                                    .ProcessName;
            groups
                .AsParallel()
                .WithDegreeOfParallelism(groups.Count)
                .ForAll
                    (
                        (x) =>
                        {
                            bool controllerPerformanceCounterProcessed = false;
                            foreach (var xx in x)
                            {
                                var performanceCounterCategoryName = string
                                                                            .Format
                                                                                (
                                                                                    "{1}{0}{2}"
                                                                                    , "-"
                                                                                    , controllerCategoryNamePrefix
                                                                                    , x.Key
                                                                                );
                                var performanceCounterInstanceName = string.Empty;
                                CommonPerformanceCountersContainer commonPerformanceCountersContainer = null;
                                if (!controllerPerformanceCounterProcessed)
                                {
                                    performanceCounterInstanceName = "*";
                                    var controllerAttribute = xx
                                                                .ActionDescriptor
                                                                .ControllerDescriptor
                                                                .GetCustomAttributes<CommonPerformanceCounterAttribute>()
                                                                .FirstOrDefault();
                                    if (controllerAttribute != null)
                                    {
                                        if (!string.IsNullOrEmpty(controllerAttribute.PerformanceCounterCategoryName))
                                        {
                                            performanceCounterCategoryName = controllerAttribute.PerformanceCounterCategoryName;
                                        }
                                        if (!string.IsNullOrEmpty(controllerAttribute.PerformanceCounterInstanceName))
                                        {
                                            performanceCounterInstanceName = controllerAttribute.PerformanceCounterInstanceName;
                                        }
                                    }
                                    EasyPerformanceCountersHelper<CommonPerformanceCountersContainer>
                                            .AttachPerformanceCountersCategoryInstance
                                                    (
                                                        performanceCounterCategoryName
                                                        , performanceCounterInstanceName
                                                        , out commonPerformanceCountersContainer
                                                    );
                                    controllerPerformanceCounterProcessed = true;
                                }
                                performanceCounterCategoryName
                                                = string
                                                        .Format
                                                            (
                                                                "{1}{0}{2}"
                                                                , "-"
                                                                , controllerCategoryNamePrefix
                                                                , x.Key
                                                            );
                                performanceCounterInstanceName
                                                = string
                                                        .Format
                                                            (
                                                                "{1}{0}{2}{0}{3}{0}{4}{0}{5}"
                                                                , "-"
                                                                , actionInstanceNamePrefix
                                                                , xx
                                                                    .ActionDescriptor
                                                                    .ControllerDescriptor
                                                                    .ControllerName
                                                                , xx
                                                                    .ActionDescriptor
                                                                    .ActionName
                                                                , xx
                                                                    .HttpMethod
                                                                    .Method
                                                                , processName
                                                            );
                                var actionAttribute = xx
                                                        .ActionDescriptor
                                                        .GetCustomAttributes<CommonPerformanceCounterAttribute>()
                                                        .FirstOrDefault();
                                if (actionAttribute != null)
                                {
                                    if (!string.IsNullOrEmpty(actionAttribute.PerformanceCounterCategoryName))
                                    {
                                        performanceCounterCategoryName = actionAttribute.PerformanceCounterCategoryName;
                                    }
                                    if (!string.IsNullOrEmpty(actionAttribute.PerformanceCounterInstanceName))
                                    {
                                        performanceCounterInstanceName = actionAttribute.PerformanceCounterInstanceName;
                                    }
                                }
                                EasyPerformanceCountersHelper<CommonPerformanceCountersContainer>
                                                .AttachPerformanceCountersCategoryInstance
                                                        (
                                                            performanceCounterCategoryName
                                                            , performanceCounterInstanceName
                                                            , out commonPerformanceCountersContainer
                                                        );
                            }
                        }
                    );
        }
    }
}
#endif
