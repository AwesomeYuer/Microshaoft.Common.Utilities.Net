#if NETFRAMEWORK4_X
namespace Microshaoft
{
    using System.Collections.Generic;
    using System.Web.Http.Routing;
    using System.Web.Http;
    using Microshaoft.Versioning;
    using System;
    using System.Linq;
    using System.Numerics;

    [
        AttributeUsage
            (
                AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Parameter
                , AllowMultiple = false
                , Inherited = false
            )
    ]
    public class SemanticVersionedAttribute : Attribute
    {
        //public int Version
        //{
        //    get;
        //    private set;
        //}

       
        public NuGetVersion Version
        {
            get;
            private set;
        }


        public VersionRange AllowedVersionRange
        {
            get;
            private set;
        }

       

        //public SemanticVersionedRouteAttribute(int version)
        //    : this(string.Empty, version)
        //{
        //}

        public SemanticVersionedAttribute
                        (
                            string version
                            , string allowedVersionRange
                        )
            
        {
            Version = NuGetVersion.Parse(version);
            AllowedVersionRange = VersionRange.Parse(allowedVersionRange);
        }
    }
}
#endif
