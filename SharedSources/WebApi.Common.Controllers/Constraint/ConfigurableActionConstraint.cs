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
        public virtual bool OnAcceptAsyncOrSyncCandidateActionSelectorProcessFunc
                                    (
                                        ConfigurableActionConstraint<TRouteAttribute> configurableActionConstraint
                                        , ActionConstraintContext actionConstraintContext
                                        , TRouteAttribute routeAttribute
                                    )
        {
            return
                OnAcceptAsyncOrSyncCandidateActionSelectorProcessFunc
                        <AbstractStoreProceduresExecutorControllerBase>
                            (
                                configurableActionConstraint
                                , actionConstraintContext
                                , routeAttribute
                            );
        }
        protected virtual bool OnAcceptAsyncOrSyncCandidateActionSelectorProcessFunc<TControllerType>
                                    (
                                        ConfigurableActionConstraint<TRouteAttribute> configurableActionConstraint
                                        , ActionConstraintContext actionConstraintContext
                                        , TRouteAttribute routeAttribute
                                    )
        {
            var r = (actionConstraintContext.Candidates.Count == 1);
            if (!r)
            {
                var currentCandidateAction = actionConstraintContext
                                                .CurrentCandidate
                                                .Action;
                var currentControllerActionDescriptor = ((ControllerActionDescriptor) currentCandidateAction);
                var currentControllerType = currentControllerActionDescriptor.ControllerTypeInfo.AsType();
                var routeContext = actionConstraintContext.RouteContext;
                var routeData = routeContext
                                        .RouteData;
                string routeName = routeData
                                        .Values[nameof(routeName)]
                                        .ToString();
                if
                    (
                        typeof(TControllerType)
                                .IsAssignableFrom(currentControllerType)
                        &&
                        routeData
                                .Values
                                .ContainsKey
                                    (nameof(routeName))
                        &&
                        !routeName
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
            return
                r;
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
            bool r;
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
                r = OnAcceptAsyncOrSyncCandidateActionSelectorProcessFunc
                            (
                                this
                                , context
                                , _routeAttribute
                            );
            }
            return
                    r;
        }
    }
}
#endif