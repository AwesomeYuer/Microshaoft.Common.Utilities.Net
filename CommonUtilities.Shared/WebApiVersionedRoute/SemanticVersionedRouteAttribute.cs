#if NETFRAMEWORK4_X
namespace Microshaoft.WebApi.Versioning
{
    using System.Collections.Generic;
    using System.Web.Http.Routing;
    using System.Web.Http;
    using Microshaoft.Versioning;
    using System;
    using System.Linq;
    using System.Numerics;
    

    public static class SemanticVersionedRouteCacheManager
    {
        public static readonly Dictionary<string, Dictionary<string, Tuple<NuGetVersion, VersionRange>>>
                        Cache = new Dictionary<string, Dictionary<string, Tuple<NuGetVersion, VersionRange>>>();
    }

    public class SemanticVersionedRouteAttribute : //RouteAttribute 
                                                    RouteFactoryAttribute
    {
        //public int Version
        //{
        //    get;
        //    private set;
        //}

        private int[] SemanticVersionDigits = { 10, 10, 10 , 10};
        


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

        public override IDictionary<string, object> Constraints
        {
            get
            {
                return
                    new HttpRouteValueDictionary
                    {
                        {
                            "version"
                            , new SemanticVersionConstraint(AllowedVersionRange)
                        }
                    };
            }
        }

        //public SemanticVersionedRouteAttribute()
        //    : this(string.Empty)
        //{
        //}

        //public SemanticVersionedRouteAttribute(string template)
        //    : this(template, 1)
        //{
        //}

        //public SemanticVersionedRouteAttribute(int version)
        //    : this(string.Empty, version)
        //{
        //}

        public SemanticVersionedRouteAttribute
                        (
                            string template
                            , string version
                            , string allowedVersionRange
                            , Type attributedType
                            , int order

                        )
            : base(template)
        {
            NuGetVersion semanticVersion = null;

            Console.WriteLine(this.DataTokens);


            //Order = -1 * order;
            //var declaringType = this.GetType().DeclaringType;

            var routePrefixAttribute = attributedType
                                    .GetCustomAttributes(typeof(RoutePrefixAttribute), true)
                                    .FirstOrDefault() as RoutePrefixAttribute;

            var routePrefix = string.Empty;
            var key = string.Empty;
            if (routePrefixAttribute != null)
            {
                routePrefix = routePrefixAttribute.Prefix;
                key = string.Format("{0}==={1}", routePrefix, template);

            }
            

            



            if
                (
                    NuGetVersion
                            .TryParse
                                (
                                    version
                                    , out semanticVersion
                                )
                )
            {
                Version = semanticVersion;
                //int i = 0;
                //long order = 0;
                int[] a =
                            {
                            semanticVersion.Major
                            , semanticVersion.Minor
                            , semanticVersion.Patch
                            , semanticVersion.Revision
                        };

                //var order = SemanticVersionDigits
                //        .Reverse()
                //        .Aggregate<int, long>
                //            (
                //                0
                //                , (x, y) =>
                //                {
                //                    x  += (a[i++] * (long)BigInteger.Pow(10, y));


                //                    return x;
                //                }

                //            );





            }

            VersionRange versionRange = null;
            if
                (
                    VersionRange.TryParse
                                    (
                                        allowedVersionRange
                                        , out versionRange
                                    )
                )
            {
                AllowedVersionRange = versionRange;
            }

            if
                (
                    AllowedVersionRange != null
                    &&
                    Version != null
                    &&
                    !key.IsNullOrEmptyOrWhiteSpace()
                )
            {
                var cacheDictionary = SemanticVersionedRouteCacheManager
                                    .Cache;
                Dictionary<string, Tuple<NuGetVersion, VersionRange>> dictionary = null;
                var isContains = false;
                if (cacheDictionary
                    .TryGetValue
                        (
                            key
                            , out dictionary
                        )
                        )
                {
                    isContains = true;
                }
                if (dictionary == null)
                {
                    dictionary = new Dictionary<string, Tuple<NuGetVersion, VersionRange>>(); 
                }
                if (!dictionary.ContainsKey(version))
                {
                    dictionary.Add(version, Tuple.Create(Version, AllowedVersionRange));
                }
                if (!isContains)
                {
                    cacheDictionary.Add(key, dictionary);
                }
            }

        }
    }
}
#endif
