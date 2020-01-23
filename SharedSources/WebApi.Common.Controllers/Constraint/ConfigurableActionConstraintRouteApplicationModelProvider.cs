#if NETCOREAPP
namespace Microshaoft
{
    using Microsoft.AspNetCore.Mvc.ActionConstraints;
    using Microsoft.AspNetCore.Mvc.ApplicationModels;
    using Microsoft.Extensions.Configuration;
    using System;
    using System.Linq;

    public class ConfigurableActionConstrainedRouteApplicationModelProvider<TRouteAttribute>
                                : IApplicationModelProvider
                                    where
                                        TRouteAttribute
                                            :
                                                IConfigurable
                                                , IConstrained<TRouteAttribute>
    {
        private readonly IConfiguration _configuration;
        private readonly
                     Func
                             <
                                TRouteAttribute
                                , IActionConstraint
                            >
                             _onActionConstraintFactoryProcessFunc = null;

        public ConfigurableActionConstrainedRouteApplicationModelProvider
                    (
                        IConfiguration configuration
                        , Func
                            <
                                TRouteAttribute
                                , IActionConstraint
                            >
                             onActionConstraintFactoryProcessFunc = null
                        , int order = -1000
                    )
        {
            _configuration = configuration;
            _onActionConstraintFactoryProcessFunc
                        = onActionConstraintFactoryProcessFunc;
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
            var attributes = context
                                .Result
                                .Controllers
                                .SelectMany
                                    (
                                        (controllerModel) =>
                                        {
                                            return
                                                controllerModel
                                                        .Attributes
                                                        .OfType
                                                            <TRouteAttribute>();
                                        }
                                    );
            foreach (var attribute in attributes)
            {
                //attribute
                //    .Configuration
                //        = _configuration;
                attribute
                    .OnActionConstraintFactoryProcessFunc
                        = _onActionConstraintFactoryProcessFunc;
            }
        }

        public void OnProvidersExecuting(ApplicationModelProviderContext context)
        {
            
        }
    }
}
#endif