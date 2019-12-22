#if NETCOREAPP
namespace Microshaoft
{
    using Microsoft.AspNetCore.Mvc.ActionConstraints;
    using Microsoft.Extensions.Configuration;
    using System;

    public class ConfigurableActionConstraint
                                : IActionConstraint
                                    , IActionConstraintMetadata
                                    , IServiceProvider
    {
        private readonly ConstraintedRouteAttribute _constraintedRouteAttribute;
        public ConfigurableActionConstraint
                    (
                        ConstraintedRouteAttribute
                               constraintedRouteAttribute
                        , Func
                                <
                                    ActionConstraintContext
                                    , ConstraintedRouteAttribute
                                    , bool
                                > onAcceptCandidateActionProcessFunc
                    )
        {
            _constraintedRouteAttribute = constraintedRouteAttribute;
            _onAcceptCandidateActionProcessFunc = onAcceptCandidateActionProcessFunc;
        }

        private readonly
                            Func
                                <
                                    ActionConstraintContext
                                    , ConstraintedRouteAttribute
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
            return
                _onAcceptCandidateActionProcessFunc
                            (
                                context
                                , _constraintedRouteAttribute
                            );
        }
    }
}
#endif