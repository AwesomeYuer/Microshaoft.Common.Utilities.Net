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

    public class SyncAsyncActionSelector 
                                : IActionSelector
                                    , IServiceProvider
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
                    , 
                        (
                            ActionDescriptor AsyncCandidate
                            , ActionDescriptor SyncCandidate
                        )
                    , IConfiguration
                    , ActionDescriptor
                >
                    OnSelectSyncAsyncActionCandidate = null;

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
                    OnSelectSyncAsyncActionCandidate != null
                )
            {
                ActionDescriptor asyncCandidate = null;
                ActionDescriptor syncCandidate = null;
                var sum = candidates
                            .Select
                                (
                                    (actionDescriptor) =>
                                    {
                                        var isAsync =  ((ControllerActionDescriptor) actionDescriptor)
                                                            .MethodInfo
                                                            .IsAsync();
                                        if (isAsync)
                                        {
                                            asyncCandidate = actionDescriptor;
                                            return 1;
                                        }
                                        else
                                        {
                                            syncCandidate = actionDescriptor;
                                            return 0;
                                        }
                                    }
                                )
                            .Sum();
                if (sum == 1)
                {
                    var candidatesPair = 
                                        (
                                            AsyncCandidate : asyncCandidate
                                            , SyncCandidate : syncCandidate
                                        );
                    var candidate = OnSelectSyncAsyncActionCandidate
                                        (
                                            context
                                            , candidatesPair
                                            , _configuration
                                        );
                    if (candidate != null)
                    {
                        candidates = EnumerableHelper
                                            .Range//<ActionDescriptor>
                                                (
                                                    candidate
                                                )
                                            .ToList()
                                            .AsReadOnly();
                    }
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

        public object GetService(Type serviceType)
        {
            return
                this;
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
                        TActionSelector : IActionSelector, IServiceProvider
        {
            var type = typeof(TActionSelector);
            TActionSelector actionSelector = (TActionSelector)
                                                target
                                                    .ApplicationServices
                                                    //.GetRequiredService<TActionSelector>();
                                                    //.GetService(typeof(IActionSelector));
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