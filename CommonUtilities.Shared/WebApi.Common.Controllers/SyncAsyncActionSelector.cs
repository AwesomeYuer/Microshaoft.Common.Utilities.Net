#if NETCOREAPP2_X
namespace Microshaoft
{
    using Microshaoft.WebApi.Controllers;
    using Microsoft.AspNetCore.Mvc.Abstractions;
    using Microsoft.AspNetCore.Mvc.Controllers;
    using Microsoft.AspNetCore.Mvc.Infrastructure;
    using Microsoft.AspNetCore.Mvc.Internal;
    using Microsoft.AspNetCore.Routing;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Collections.Generic;
    using System.Linq;

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
        public ActionDescriptor SelectBestCandidate
                                        (
                                            RouteContext context
                                            , IReadOnlyList<ActionDescriptor> candidates
                                        )
        {
            if (candidates.Count > 1)
            {
                var r = candidates
                                .All
                                    (
                                        (x) =>
                                        {
                                            var controllerActionDescriptor = (ControllerActionDescriptor)x;
                                            var rr =
                                                    (
                                                        controllerActionDescriptor
                                                                .ControllerTypeInfo
                                                                .UnderlyingSystemType
                                                                .BaseType 
                                                        ==
                                                        typeof(AbstractStoreProceduresExecutorControllerBase)
                                                    );
                                            return rr;
                                        }
                                    );
                if (r)
                {
                    var routeName = context.RouteData.Values["routeName"].ToString();
                    var httpMethod = $"Http{context.HttpContext.Request.Method}";
                    var isExecuteAsync = false;
                    var isExecuteAsyncConfiguration =
                                _configuration
                                    .GetSection($"Routes:{routeName}:{httpMethod}:IsExecuteAsync");
                    if (isExecuteAsyncConfiguration.Exists())
                    {
                        isExecuteAsync = isExecuteAsyncConfiguration.Get<bool>();
                    }
                    if (isExecuteAsync)
                    {
                        candidates = candidates
                                            .Where
                                                (
                                                    (x) =>
                                                    {
                                                        return
                                                        x.RouteValues["action"].EndsWith("async", StringComparison.OrdinalIgnoreCase);
                                                        
                                                    }
                                                )
                                            .ToList()
                                            .AsReadOnly();
                    }
                    else
                    {
                        candidates = candidates
                                            .Where
                                                (
                                                    (x) =>
                                                    {
                                                        return
                                                        !x
                                                            .RouteValues["action"]
                                                            .EndsWith
                                                                (
                                                                    "async"
                                                                    , StringComparison
                                                                        .OrdinalIgnoreCase
                                                                );
                                                    }
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
            return
                _actionSelector
                    .SelectCandidates
                        (
                            context
                        );
        }
    }
}
#endif