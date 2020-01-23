#if NETFRAMEWORK4_X
namespace Microshaoft.WebApi.Versioning
{
    using System.Collections.Generic;
    using System.Web.Http.Routing;
    using System.Web.Http;

    public class VersionedRouteAttribute : //RouteAttribute 
                                    RouteFactoryAttribute
    {
        public int Version
        {
            get;
            private set;
        }

        public override IDictionary<string, object> Constraints
        {
            get
            {
                return
                    new HttpRouteValueDictionary
                    {
                        {
                            "version"
                            , new VersionConstraint(Version)
                        }
                    };
            }
        }

        public VersionedRouteAttribute()
            : this(string.Empty)
        {
        }

        public VersionedRouteAttribute(string template)
            : this(template, 1)
        {
        }

        public VersionedRouteAttribute(int version)
            : this(string.Empty, version)
        {
        }

        public VersionedRouteAttribute(string template, int version)
            : base(template)
        {
            Version = version;
        }
    }
}
#endif
