#if NETCOREAPP
namespace Microshaoft
{
    using Microshaoft.WebApi.Controllers;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.ActionConstraints;
    using Microsoft.AspNetCore.Mvc.Controllers;
    using System;

    public class ConfigurableActionConstraint<TRouteAttribute>
                                : 
                                    IActionConstraint
                                    , IActionConstraintMetadata
                                    , IServiceProvider
                    where
                        TRouteAttribute
                                : RouteAttribute , IConfigurable
    {
        private readonly TRouteAttribute _routeAttribute;
        public ConfigurableActionConstraint
                    (
                        TRouteAttribute
                            routeAttribute
                        , Func
                                <
                                    ConfigurableActionConstraint<TRouteAttribute>
                                    , ActionConstraintContext
                                    , TRouteAttribute
                                    , bool
                                >
                            onAcceptCandidateActionProcessFunc = null
                    )
        {
            _routeAttribute = routeAttribute;
            _onAcceptCandidateActionProcessFunc
                        = onAcceptCandidateActionProcessFunc;
        }

        private readonly
                        Func
                            <
                                ConfigurableActionConstraint<TRouteAttribute>
                                , ActionConstraintContext
                                , TRouteAttribute
                                , bool
                            >
                                _onAcceptCandidateActionProcessFunc = null;
        public int Order
        {
            get
            {
                return 1;
            }
        }

        public object GetService(Type serviceType)
        {
            throw new NotImplementedException();
            //return
            //    this;
        }

        public bool Accept(ActionConstraintContext context)
        {
            var r = false;
            if (_onAcceptCandidateActionProcessFunc != null)
            {
                r = _onAcceptCandidateActionProcessFunc
                            (
                                this
                                , context
                                , _routeAttribute
                            );
            }
            else
            {
                r = (context.Candidates.Count == 1);
                if (!r)
                {
                    var currentCandidateAction = context
                                                .CurrentCandidate
                                                .Action;
                    var currentControllerActionDescriptor = ((ControllerActionDescriptor) currentCandidateAction);
                    var controllerType = currentControllerActionDescriptor.ControllerTypeInfo.AsType();
                    var routeContext = context.RouteContext;
                    var routeData = routeContext
                                            .RouteData;
                    var routeName = string.Empty;

                    if
                        (
                            typeof(AbstractStoreProceduresExecutorControllerBase)
                                    .IsAssignableFrom(controllerType)
                            &&
                            routeData
                                    .Values
                                    .ContainsKey
                                        (nameof(routeName))
                            &&
                            !routeData
                                    .Values[nameof(routeName)]
                                    .ToString()
                                    .IsNullOrEmptyOrWhiteSpace()
                        )
                    {
                        var httpContext = routeContext
                                                    .HttpContext;
                        var request = httpContext
                                                .Request;
                        var isAsyncExecuting = currentControllerActionDescriptor
                                                            .MethodInfo
                                                            .IsAsync();
                        var httpMethod = $"Http{request.Method}";
                        var accessingConfigurationKey = "DefaultAccessing";
                        if (request.Path.ToString().Contains("/export/", StringComparison.OrdinalIgnoreCase))
                        {
                            accessingConfigurationKey = "exporting";
                        }
                        var isAsyncExecutingInConfiguration =
                                        _routeAttribute
                                                        .Configuration
                                                        .GetOrDefault
                                                            (
                                                                $"Routes:{routeName}:{httpMethod}:{accessingConfigurationKey}:isAsyncExecuting"
                                                                , false
                                                            );
                        r =
                            (
                                isAsyncExecutingInConfiguration
                                ==
                                isAsyncExecuting
                            );
                    }
                }
            }
            return
                    r;
        }
    }
}
#endif