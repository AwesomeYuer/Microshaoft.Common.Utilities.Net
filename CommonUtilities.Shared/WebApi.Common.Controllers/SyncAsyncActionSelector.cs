#if NETCOREAPP2_X && !NETCOREAPP3_X
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

    public class SyncOrAsyncActionSelector 
                                : IActionSelector
                                    , IServiceProvider
    {
        private readonly ActionSelector _actionSelector;
        private readonly IConfiguration _configuration;
        
        public SyncOrAsyncActionSelector
                    (
                        IActionDescriptorCollectionProvider
                                    actionDescriptorCollectionProvider
                        , ActionConstraintCache
                                    actionConstraintCache
                        , ILoggerFactory
                                    loggerFactory
                        , IConfiguration
                                    configuration
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
                    OnSelectSyncOrAsyncActionCandidate = null;

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
                    OnSelectSyncOrAsyncActionCandidate != null
                )
            {
                ActionDescriptor asyncCandidate = null;
                ActionDescriptor syncCandidate = null;
                var count = candidates
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
                                                }
                                                else
                                                {
                                                    syncCandidate = actionDescriptor;
                                                }
                                                return isAsync;
                                            }
                                        )
                                    .Distinct()
                                    .Count();
                if (count == 2)
                {
                    var candidatesPair = 
                                        (
                                            AsyncCandidate : asyncCandidate
                                            , SyncCandidate : syncCandidate
                                        );
                    var candidate = OnSelectSyncOrAsyncActionCandidate
                                        (
                                            context
                                            , candidatesPair
                                            , _configuration
                                        );
                    if (candidate != null)
                    {
                        candidates = new List<ActionDescriptor>()
                                            {
                                                candidate
                                            }
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
            throw new NotImplementedException();
            //return
            //    this;
        }
    }

    public static partial class ActionSelectorServiceExtensions
    {
        public static IApplicationBuilder UseCustomActionSelector<TActionSelector>
                    (
                        this IApplicationBuilder
                                            target
                        , Action<TActionSelector>
                                            OnActionSelectorInitializeProcessAction
                    )
                 where
                        TActionSelector : IActionSelector, IServiceProvider
        {
            var type = typeof(TActionSelector);
            TActionSelector actionSelector = 
                                (TActionSelector)
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
                OnActionSelectorInitializeProcessAction
                                            (actionSelector);
            }
            else
            {
                throw
                    new Exception
                            ($"can't found and use typeof({type.Name}) custom action selector!");
            }
            return
                target;
        }
    }
}
#endif