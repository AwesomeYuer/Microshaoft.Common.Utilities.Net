#if NETCOREAPP
namespace Microshaoft
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.ActionConstraints;
    using Microsoft.Extensions.Configuration;
    using System;

    public class ConstraintedRouteAttribute
                        : RouteAttribute
                            , IActionConstraintFactory
    {

        public IConfiguration Configuration { get; set; }
        public Func
            <
                ConstraintedRouteAttribute
                , IActionConstraint
            > OnActionConstraintFactoryProcessFunc;

        public bool IsReusable => true;

        public ConstraintedRouteAttribute
                        (
                            string template
                        ) 
                            : base(template)
        {

        }

        private IActionConstraint _constraint = null;
        private object _locker = new object();
        public IActionConstraint CreateInstance(IServiceProvider services)
        {
            _locker
                .LockIf
                    (
                        () =>
                        {
                            return
                                (_constraint == null);
                        }
                        ,() =>
                        {
                            _constraint = OnActionConstraintFactoryProcessFunc(this);
                        }
                    );
            return _constraint;
        }
    }
}
#endif

