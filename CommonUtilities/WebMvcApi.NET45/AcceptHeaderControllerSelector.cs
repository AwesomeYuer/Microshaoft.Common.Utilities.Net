﻿//using System.Web.Http.SelfHost;

namespace Microshaoft.Web
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text.RegularExpressions;
    using System.Web;
    using System.Web.Http;
    using System.Web.Http.Controllers;
    using System.Web.Http.Dispatcher;
    using System.Web.Http.Routing;

    public class AcceptHeaderControllerSelector : IHttpControllerSelector
    {
        private const string ControllerKey = "controller";

        private readonly HttpConfiguration _configuration;
        private readonly Func<MediaTypeHeaderValue, string> _namespaceResolver;
        private readonly Lazy<Dictionary<string, HttpControllerDescriptor>> _controllers;
        private readonly HashSet<string> _duplicates;

        public AcceptHeaderControllerSelector
                        (
                            HttpConfiguration config
                            , Func<MediaTypeHeaderValue, string> namespaceResolver
                        )
        {
            _configuration = config;
            _namespaceResolver = namespaceResolver;
            _duplicates = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            _controllers = new Lazy<Dictionary<string, HttpControllerDescriptor>>(InitializeControllersDictionary);
        }

        private Dictionary<string, HttpControllerDescriptor> InitializeControllersDictionary()
        {
            var dictionary = new Dictionary<string, HttpControllerDescriptor>(StringComparer.OrdinalIgnoreCase);

            // Create a lookup table where key is "namespace.controller". The value of "namespace" is the last
            // segment of the full namespace. For example:
            // MyApplication.Controllers.V1.ProductsController => "V1.Products"
            IAssembliesResolver assembliesResolver = _configuration
                                                            .Services
                                                            .GetAssembliesResolver();
            IHttpControllerTypeResolver httpControllerTypeResolver = _configuration
                                                                    .Services
                                                                    .GetHttpControllerTypeResolver();

            ICollection<Type> controllersTypes = httpControllerTypeResolver
                                                            .GetControllerTypes(assembliesResolver);

            foreach (Type controllerType in controllersTypes)
            {
                var segments = controllerType
                                .Namespace
                                .Split
                                    (
                                        Type
                                            .Delimiter
                                    );

                // For the dictionary key, strip "Controller" from the end of the type name.
                // This matches the behavior of DefaultHttpControllerSelector.
                var controllerName = controllerType
                                        .Name
                                        .Remove
                                            (
                                                controllerType.Name.Length
                                                -
                                                DefaultHttpControllerSelector
                                                    .ControllerSuffix
                                                    .Length
                                            );

                var routePrefixAttribute = controllerType
                                                    .GetCustomAttributes(typeof(RoutePrefixAttribute), true)
                                                    .FirstOrDefault() as RoutePrefixAttribute;
                var routePrefix = string.Empty;
                if (routePrefixAttribute != null)
                {
                    routePrefix = routePrefixAttribute.Prefix.Trim('/');
                }

                var webApiVersionAttribute = controllerType
                                    .GetCustomAttributes(typeof(WebApiVersionAttribute), true)
                                    .FirstOrDefault() as WebApiVersionAttribute;
                var webApiVersionNumber = string.Empty;
                if (webApiVersionAttribute != null)
                {
                    webApiVersionNumber = webApiVersionAttribute.ApiVersionNumber;
                }

                var key = string
                                .Format
                                    (
                                        CultureInfo.InvariantCulture
                                        , "{0}.{1}"
                                        , segments[segments.Length - 1]
                                        , controllerName
                                    );
                if 
                    (
                        !webApiVersionNumber.IsNullOrEmptyOrWhiteSpace()
                        &&
                        !routePrefix.IsNullOrEmptyOrWhiteSpace()
                    )
                {
                    key = string
                            .Format
                                (
                                    CultureInfo.InvariantCulture
                                    , "RoutePrefix[{0}].Version[{1}]"
                                    , routePrefix
                                    , webApiVersionNumber
                                );
                    if (dictionary.Keys.Contains(key))
                    {
                        _duplicates.Add(key);
                    }
                    else
                    {
                        dictionary[key] = new HttpControllerDescriptor(_configuration, controllerType.Name, controllerType);
                    }
                }

                
                // Check for duplicate keys.
                
            }

            // Remove any duplicates from the dictionary, because these create ambiguous matches. 
            // For example, "Foo.V1.ProductsController" and "Bar.V1.ProductsController" both map to "v1.products".
            foreach (string s in _duplicates)
            {
                dictionary.Remove(s);
            }
            return dictionary;
        }

        // Get a value from the route data, if present.
        private static T GetRouteVariable<T>(IHttpRouteData routeData, string name)
        {
            object result = null;
            if (routeData.Values.TryGetValue(name, out result))
            {
                return (T)result;
            }
            return default(T);
        }

        public HttpControllerDescriptor SelectController(HttpRequestMessage request)
        {
            IHttpRouteData routeData = request.GetRouteData();
            if (routeData == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            // Get the namespace and controller variables from the route data.
            string webApiVersionNumber = string.Empty;
            foreach (var accepts in request.Headers.Accept)
            {
                webApiVersionNumber = _namespaceResolver(accepts);
                if (!webApiVersionNumber.IsNullOrEmptyOrWhiteSpace())
                {
                    break;
                }
            }
            //var routePrefix = "aaa";




            var routePrefix = string.Join("", request.RequestUri.Segments.Take(3)).Trim('/');

            var key = string
                    .Format
                        (
                            CultureInfo.InvariantCulture
                            , "RoutePrefix[{0}].Version[{1}]"
                            , routePrefix
                            , webApiVersionNumber
                        );

            IEnumerable<IHttpRouteData> attributeSubRoutes = routeData.GetSubRoutes();
            IEnumerable<IHttpRouteData> filteredSubRoutes = attributeSubRoutes
                    .Where(attrRouteData =>
                    {
                        HttpControllerDescriptor currentDescriptor = GetControllerDescriptor(attrRouteData);

                        //bool match = currentDescriptor.ControllerName.EndsWith(newControllerNameSuffix);

                        //if (match && (controllerDescriptor == null))
                        //{
                        //    controllerDescriptor = currentDescriptor;
                        //}

                        return true;
                    });

            routeData.Values["MS_SubRoutes"] = filteredSubRoutes.ToArray();

            HttpControllerDescriptor controllerDescriptor;
            if (_controllers.Value.TryGetValue(key, out controllerDescriptor))
            {
                return controllerDescriptor;
            }
            else if (_duplicates.Contains(key))
            {
                throw
                    new HttpResponseException
                            (
                                request
                                    .CreateErrorResponse
                                        (
                                            HttpStatusCode.InternalServerError
                                            , "Multiple controllers were found that match this request."
                                        )
                            );
            }
            else
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }
        }
        private HttpControllerDescriptor GetControllerDescriptor(IHttpRouteData routeData)
        {
            return ((HttpActionDescriptor[])routeData.Route.DataTokens["actions"]).First().ControllerDescriptor;
        }
        public IDictionary<string, HttpControllerDescriptor> GetControllerMapping()
        {
            return _controllers.Value;
        }
    }
}