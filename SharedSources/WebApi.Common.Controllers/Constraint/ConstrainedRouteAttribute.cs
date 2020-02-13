#if NETCOREAPP
namespace Microshaoft
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.ActionConstraints;
    using Microsoft.Extensions.Configuration;
    using System;

    public interface IConfigurable
    {
        IConfiguration Configuration
        {
            get;
            set;
        }
    }

    public interface IConstrained<TRouteAttribute>
    {
        Func
            <
                TRouteAttribute
                , IActionConstraint
            >
            OnActionConstraintFactoryProcessFunc
        {
            get;
            set;
        }
    }

    public class ConstrainedRouteAttribute
                        : 
                            RouteAttribute
                            , IActionConstraintFactory
                            , IConfigurable
                            , IConstrained<ConstrainedRouteAttribute>
    {

        public IConfiguration Configuration
        {
            get;
            //{
            //    return
            //        ConfigurationHelper
            //                    .Configuration;
            //}
            set;
        }

        public
            Func
                <
                    ConstrainedRouteAttribute
                    , IActionConstraint
                > 
                    OnActionConstraintFactoryProcessFunc 
        { 
            get;
            set;
        }

        public bool IsReusable => true;
        public ConstrainedRouteAttribute
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
                            _constraint
                                = OnActionConstraintFactoryProcessFunc(this);
                        }
                    );
            return
                _constraint;
        }
    }
}
#endif