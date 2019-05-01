#if NETCOREAPP2_X
namespace Microshaoft
{
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Mvc.Abstractions;
    using Microsoft.AspNetCore.Mvc.Controllers;
    using Microsoft.AspNetCore.Mvc.Infrastructure;
    using Microsoft.AspNetCore.Mvc.Internal;
    using Microsoft.AspNetCore.Routing;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public class SyncAsyncActionSelector : IActionSelector
    {
        private ActionSelector _actionSelector;
        private readonly IConfiguration _configuration;
        
        public SyncAsyncActionSelector
                    (
                        IActionDescriptorCollectionProvider actionDescriptorCollectionProvider
                        , ActionConstraintCache actionConstraintCache
                        , ILoggerFactory loggerFactory
                        , IConfiguration configuration
                    )
        {
            _actionSelector = new ActionSelector
                                        (
                                            actionDescriptorCollectionProvider
                                            , actionConstraintCache
                                            , loggerFactory
                                        );
            _configuration = configuration;
        }

        public
            Func
                <
                    RouteContext
                    , IReadOnlyList<ActionDescriptor>
                    , IConfiguration
                    , IReadOnlyList<ActionDescriptor>
                >
                    OnSelectSyncAsyncActionCandidates = null;

        public ActionDescriptor SelectBestCandidate
                                        (
                                            RouteContext context
                                            , IReadOnlyList<ActionDescriptor> candidates
                                        )
        {
            if 
                (
                    candidates.Count == 2
                    &&
                    OnSelectSyncAsyncActionCandidates != null
                )
            {
                //候选方法仅有 Async/Sync Task/Non-Task 返回类型的区别
                //这里不用 Action 对应的方法名字后缀 Async/Sync 作为区分
                var sum = candidates
                            .Select
                                (
                                    (actionDescriptor) =>
                                    {
                                        var r = 0;
                                        if
                                            (
                                                ((ControllerActionDescriptor) actionDescriptor)
                                                    .MethodInfo
                                                    .ReturnType
                                                    .BaseType
                                                ==
                                                typeof(Task)
                                            )
                                        {
                                            r = 1;
                                        }
                                        return r;
                                    }
                                )
                            .Sum();
                if (sum == 1)
                {
                    candidates = OnSelectSyncAsyncActionCandidates
                                    (
                                        context
                                        , candidates
                                        , _configuration
                                    );
                }
            }
            return
                _actionSelector
                    .SelectBestCandidate
                        (
                            context
                            , candidates
                        );
        }

        public IReadOnlyList<ActionDescriptor> SelectCandidates(RouteContext context)
        {
            throw new NotImplementedException();
            //return
            //    _actionSelector
            //        .SelectCandidates
            //            (
            //                context
            //            );
        }
    }

    public static class ActionSelectorApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseCustomActionSelector<TActionSelector>
            (
                this IApplicationBuilder target
                , Action<TActionSelector> OnActionSelectorInitializeProcessAction
            )
                 where
                        TActionSelector : IActionSelector
        {
            var type = typeof(TActionSelector);
            TActionSelector actionSelector = (TActionSelector)
                                                target
                                                    .ApplicationServices
                                                    .GetServices<IActionSelector>()
                                                    .FirstOrDefault
                                                        (
                                                            (_) =>
                                                            {
                                                                return
                                                                    (
                                                                        _.GetType()
                                                                        ==
                                                                        type
                                                                    );
                                                            }
                                                        );
            if (actionSelector != null)
            {
                OnActionSelectorInitializeProcessAction(actionSelector);
            }
            else
            {
                throw new Exception($"can't found and use typeof({type.Name}) custom action selector!");
            }
            return
                target;
        }
    }
}
#endif