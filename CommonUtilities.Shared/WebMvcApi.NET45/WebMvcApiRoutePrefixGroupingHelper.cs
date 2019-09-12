#if NETFRAMEWORK4_X

namespace Microshaoft.Web
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Http;
    using System.Web.Http.Controllers;

    public static class WebMvcApiVersionedRoutesHelper
    {
        public static Dictionary<string,Dictionary<string, HttpActionDescriptor>> LoadRoutes
                            (
                                HttpConfiguration httpConfiguration
                            )
        {
            var assembliesResolver = httpConfiguration
                                            .Services
                                            .GetAssembliesResolver();
            var httpControllerTypeResolver
                                    = httpConfiguration
                                            .Services
                                            .GetHttpControllerTypeResolver();
            var controllersTypes = httpControllerTypeResolver
                                            .GetControllerTypes(assembliesResolver);
            var httpControllerSelector = httpConfiguration
                                            .Services
                                            .GetHttpControllerSelector();
            var controllerMapping = httpControllerSelector
                                            .GetControllerMapping();
            Dictionary<string, Dictionary<string, HttpActionDescriptor>> result = null;
            if (controllerMapping != null)
            {
                var actions = controllerMapping
                                    .Values
                                    .SelectMany
                                        (
                                            (x) =>
                                            {
                                                var httpActionSelector = x
                                                                            .Configuration
                                                                            .Services
                                                                            .GetActionSelector();
                                                var actionsByName = httpActionSelector
                                                                        .GetActionMapping(x);
                                                return
                                                    actionsByName
                                                            .SelectMany
                                                                (
                                                                    (xx) =>
                                                                    {
                                                                        return xx;
                                                                    }
                                                                );
                                            }
                                        );

                RoutePrefixAttribute routePrefixAttribute = null;
                RouteAttribute routeAttribute = null;
                SemanticVersionedAttribute semanticVersionedAttribute = null;
                result = actions
                                    .Where
                                        (
                                            (x) =>
                                            {
#region 按 RoutePrefix + Route + Version 筛选
                                                var r = false;
                                                routePrefixAttribute = x
                                                                            .ControllerDescriptor
                                                                            .GetCustomAttributes<RoutePrefixAttribute>()
                                                                            .FirstOrDefault() as RoutePrefixAttribute;
                                                if (routePrefixAttribute != null)
                                                {
                                                    routeAttribute = x
                                                                    .GetCustomAttributes<RouteAttribute>()
                                                                    .FirstOrDefault() as RouteAttribute;
                                                    if (routeAttribute != null)
                                                    {
                                                        semanticVersionedAttribute = x
                                                                    .GetCustomAttributes<SemanticVersionedAttribute>()
                                                                    .FirstOrDefault() as SemanticVersionedAttribute;
                                                        if (semanticVersionedAttribute != null)
                                                        {
                                                            r = true;
                                                        }
                                                    }
                                                }
                                                return r; 
#endregion
                                            }
                                        )
                                    .ToLookup
                                        (
                                            (x) =>
                                            {
                                                var key = string
                                                                .Format
                                                                    (
                                                                        "{1}{0}{2}"
                                                                        , "/"
                                                                        , routePrefixAttribute.Prefix
                                                                        , routeAttribute.Template
                                                                    );
                                                return key;
                                            }
                                        )
                                    .ToDictionary
                                        (
                                            (x) =>
                                            {
                                                return x.Key;
                                            }
                                            , (x) =>
                                            {
                                                return
                                                    x
                                                        .ToDictionary
                                                            (
                                                                (xx) =>
                                                                {
                                                                    var attribute = xx
                                                                                        .GetCustomAttributes<SemanticVersionedAttribute>()
                                                                                        .FirstOrDefault() as SemanticVersionedAttribute;
                                                                    return
                                                                        attribute
                                                                                .Version
                                                                                .ToNormalizedString();
                                                                }
                                                            );
                                            }
                                        );


                }

            return result;

        }
    }
}
#endif
