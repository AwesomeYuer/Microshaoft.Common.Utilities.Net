#if NETCOREAPP
namespace Microshaoft
{
    using Microsoft.AspNetCore.Mvc.ActionConstraints;
    using Microsoft.AspNetCore.Mvc.ApplicationModels;
    using Microsoft.Extensions.Configuration;
    using System;
    using System.Linq;

    public class ConfigurableActionConstraintRouteApplicationModelProvider
                        : IApplicationModelProvider
    {
        private readonly IConfiguration _configuration;
        private readonly
                     Func
                             <
                                ConstraintedRouteAttribute
                                , IActionConstraint
                            >
                             _onActionConstraintFactoryProcessFunc = null;

        public ConfigurableActionConstraintRouteApplicationModelProvider
                    (
                        IConfiguration configuration
                        , Func
                            <
                                ConstraintedRouteAttribute
                                , IActionConstraint
                            >
                             onActionConstraintFactoryProcessFunc = null
                        , int order = -1000
                    )
        {
            _configuration = configuration;
            _onActionConstraintFactoryProcessFunc = onActionConstraintFactoryProcessFunc;
            _order = order;
        }

        private int _order = -1000;
        public int Order
        { 
            get
            {
                return _order;
            }
        }

        public void OnProvidersExecuted(ApplicationModelProviderContext context)
        {
            var items = context
                            .Result
                            .Controllers
                            .SelectMany
                                (
                                    (controllerModel) =>
                                    {
                                        return
                                            controllerModel
                                                    .Attributes
                                                    .OfType<ConstraintedRouteAttribute>();
                                    }
                                );
            foreach (var item in items)
            {
                item.Configuration = _configuration;
                item.OnActionConstraintFactoryProcessFunc = _onActionConstraintFactoryProcessFunc;
            }
        }

        public void OnProvidersExecuting(ApplicationModelProviderContext context)
        {
            
        }
    }
}
#endif