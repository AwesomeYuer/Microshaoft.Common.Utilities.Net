
// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http.Controllers;
using System.Web.Http.Properties;
using System.Web.Http.Routing;

namespace System.Web.Http.Dispatcher
{
    /// <summary>
    /// Default <see cref="IHttpControllerSelector"/> instance for choosing a <see cref="HttpControllerDescriptor"/> given a <see cref="HttpRequestMessage"/>
    /// A different implementation can be registered via the <see cref="HttpConfiguration.Services"/>.
    /// </summary>
    public class EasyHttpControllerSelector : IHttpControllerSelector
    {
        public static readonly string ControllerSuffix = "Controller";

        private const string ControllerKey = "controller";

        private readonly HttpConfiguration _configuration;
        private readonly HttpControllerTypeCache _controllerTypeCache;
        private readonly Lazy<ConcurrentDictionary<string, HttpControllerDescriptor>> _controllerInfoCache;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultHttpControllerSelector1"/> class.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        public EasyHttpControllerSelector(HttpConfiguration configuration)
        {
            //if (configuration == null)
            //{
            //    throw Error.ArgumentNull("configuration");
            //}

            _controllerInfoCache = new Lazy<ConcurrentDictionary<string, HttpControllerDescriptor>>(InitializeControllerInfoCache);
            _configuration = configuration;
            _controllerTypeCache = new HttpControllerTypeCache(_configuration);
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Caller is responsible for disposing of response instance.")]
        public virtual HttpControllerDescriptor SelectController(HttpRequestMessage request)
        {
            //if (request == null)
            //{
            //    throw Error.ArgumentNull("request");
            //}

            IHttpRouteData routeData = request.GetRouteData();
            HttpControllerDescriptor controllerDescriptor;
            if (routeData != null)
            {
                controllerDescriptor = GetDirectRouteController(routeData);
                if (controllerDescriptor != null)
                {
                    return controllerDescriptor;
                }
            }

            string controllerName = GetControllerName(request);
            if (String.IsNullOrEmpty(controllerName))
            {
                throw new HttpResponseException
                    (
                        request.CreateErrorResponse(
                            HttpStatusCode.NotFound,


                            @"Error.Format(SRResources.ResourceNotFound, request.RequestUri),
                            Error.Format(SRResources.ControllerNameNotFound, request.RequestUri)"

                            )
                    );
            }

            if (_controllerInfoCache.Value.TryGetValue(controllerName, out controllerDescriptor))
            {
                return controllerDescriptor;
            }

            ICollection<Type> matchingTypes = _controllerTypeCache.GetControllerTypes(controllerName);

            // ControllerInfoCache is already initialized.
            Contract.Assert(matchingTypes.Count != 1);

            if (matchingTypes.Count == 0)
            {
                // no matching types
                throw new HttpResponseException(request.CreateErrorResponse(
                    HttpStatusCode.NotFound,
                    @"Error.Format(SRResources.ResourceNotFound, request.RequestUri),
                    Error.Format(SRResources.DefaultControllerFactory_ControllerNameNotFound, controllerName)"


                    )
                    );
            }
            else
            {
                // multiple matching types
                throw CreateAmbiguousControllerException(request.GetRouteData().Route, controllerName, matchingTypes);
            }
        }

        public virtual IDictionary<string, HttpControllerDescriptor> GetControllerMapping()
        {
            return _controllerInfoCache.Value.ToDictionary(c => c.Key, c => c.Value, StringComparer.OrdinalIgnoreCase);
        }

        public virtual string GetControllerName(HttpRequestMessage request)
        {
            if (request == null)
            {
                throw
                    new Exception
                    (
                        @"Error.ArgumentNull(""request"")"
                    );
            }

            IHttpRouteData routeData = request.GetRouteData();
            if (routeData == null)
            {
                return null;
            }

            // Look up controller in route data
            object controllerName = null;
            routeData.Values.TryGetValue(ControllerKey, out controllerName);
            return (string)controllerName;
        }

        // If routeData is from an attribute route, get the controller that can handle it. 
        // Else return null. Throws an exception if multiple controllers match
        private static HttpControllerDescriptor GetDirectRouteController(IHttpRouteData routeData)
        {
            CandidateAction[] candidates = routeData.GetDirectRouteCandidates();
            if (candidates != null)
            {
                // Set the controller descriptor for the first action descriptor
                Contract.Assert(candidates.Length > 0);
                Contract.Assert(candidates[0].ActionDescriptor != null);

                HttpControllerDescriptor controllerDescriptor = candidates[0].ActionDescriptor.ControllerDescriptor;

                // Check that all other candidate action descriptors share the same controller descriptor
                for (int i = 1; i < candidates.Length; i++)
                {
                    CandidateAction candidate = candidates[i];
                    if (candidate.ActionDescriptor.ControllerDescriptor != controllerDescriptor)
                    {
                        // We've found an ambiguity (multiple controllers matched)
                        throw CreateDirectRouteAmbiguousControllerException(candidates);
                    }
                }

                return controllerDescriptor;
            }

            return null;
        }

        private static Exception CreateDirectRouteAmbiguousControllerException(CandidateAction[] candidates)
        {
            Contract.Assert(candidates != null);
            Contract.Assert(candidates.Length > 1);

            HashSet<Type> matchingTypes = new HashSet<Type>();
            for (int i = 0; i < candidates.Length; i++)
            {
                matchingTypes.Add(candidates[i].ActionDescriptor.ControllerDescriptor.ControllerType);
            }

            // we need to generate an exception containing all the controller types
            StringBuilder typeList = new StringBuilder();
            foreach (Type matchedType in matchingTypes)
            {
                typeList.AppendLine();
                typeList.Append(matchedType.FullName);
            }

            return
                new Exception(

                "Error.InvalidOperation(SRResources.DirectRoute_AmbiguousController, typeList, Environment.NewLine)"
                );
        }

        private static Exception CreateAmbiguousControllerException(IHttpRoute route, string controllerName, ICollection<Type> matchingTypes)
        {
            Contract.Assert(route != null);
            Contract.Assert(controllerName != null);
            Contract.Assert(matchingTypes != null);

            // Generate an exception containing all the controller types
            StringBuilder typeList = new StringBuilder();
            foreach (Type matchedType in matchingTypes)
            {
                typeList.AppendLine();
                typeList.Append(matchedType.FullName);
            }

            string errorMessage = "Error.Format(SRResources.DefaultControllerFactory_ControllerNameAmbiguous_WithRouteTemplate, controllerName, route.RouteTemplate, typeList, Environment.NewLine)";
            return new InvalidOperationException(errorMessage);
        }

        private ConcurrentDictionary<string, HttpControllerDescriptor> InitializeControllerInfoCache()
        {
            var result = new ConcurrentDictionary<string, HttpControllerDescriptor>(StringComparer.OrdinalIgnoreCase);
            var duplicateControllers = new HashSet<string>();
            Dictionary<string, ILookup<string, Type>> controllerTypeGroups = _controllerTypeCache.Cache;

            foreach (KeyValuePair<string, ILookup<string, Type>> controllerTypeGroup in controllerTypeGroups)
            {
                string controllerName = controllerTypeGroup.Key;

                foreach (IGrouping<string, Type> controllerTypesGroupedByNs in controllerTypeGroup.Value)
                {
                    foreach (Type controllerType in controllerTypesGroupedByNs)
                    {
                        if (result.Keys.Contains(controllerName))
                        {
                            duplicateControllers.Add(controllerName);
                            break;
                        }
                        else
                        {
                            result.TryAdd(controllerName, new HttpControllerDescriptor(_configuration, controllerName, controllerType));
                        }
                    }
                }
            }

            foreach (string duplicateController in duplicateControllers)
            {
                HttpControllerDescriptor descriptor;
                result.TryRemove(duplicateController, out descriptor);
            }

            return result;
        }
    }
}


// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.



namespace System.Web.Http.Dispatcher
{
    using System.Collections.Generic;
    using System.Linq;
    using System;
    using System.Web.Http;
    using Microshaoft.WebApi.Versioning;
    using Microshaoft;
    using Microshaoft.Versioning;
    //using System.Web.Http.Dispatcher;
    /// <summary>
    /// Manages a cache of <see cref="System.Web.Http.Controllers.IHttpController"/> types detected in the system.
    /// </summary>
    internal sealed class HttpControllerTypeCache
    {
        private readonly HttpConfiguration _configuration;
        private readonly Lazy<Dictionary<string, ILookup<string, Type>>> _cache;

        public HttpControllerTypeCache(HttpConfiguration configuration)
        {
            if (configuration == null)
            {
                throw //Error.ArgumentNull("configuration");
                        new ArgumentNullException("configuration");
            }

            _configuration = configuration;
            _cache = new Lazy<Dictionary<string, ILookup<string, Type>>>(InitializeCache);
        }

        internal Dictionary<string, ILookup<string, Type>> Cache
        {
            get { return _cache.Value; }
        }

        public ICollection<Type> GetControllerTypes(string controllerName)
        {
            if (String.IsNullOrEmpty(controllerName))
            {
                throw //Error.ArgumentNullOrEmpty("controllerName");
                        new ArgumentNullException("controllerName");
            }

            HashSet<Type> matchingTypes = new HashSet<Type>();

            ILookup<string, Type> namespaceLookup;
            if (_cache.Value.TryGetValue(controllerName, out namespaceLookup))
            {
                foreach (var namespaceGroup in namespaceLookup)
                {
                    matchingTypes.UnionWith(namespaceGroup);
                }
            }

            return matchingTypes;
        }

        private Dictionary<string, ILookup<string, Type>> InitializeCache()
        {
            IAssembliesResolver assembliesResolver = _configuration.Services.GetAssembliesResolver();
            IHttpControllerTypeResolver controllersResolver = _configuration.Services.GetHttpControllerTypeResolver();

            ICollection<Type> controllerTypes = controllersResolver.GetControllerTypes(assembliesResolver);





            SemanticVersionedRouteAttribute attribute = null;
            var groups = controllerTypes
                                        .Where
                                            (
                                                (x) =>
                                                {
                                                    var r = false;
                                                    attribute = x.GetCustomAttributes(typeof(SemanticVersionedRouteAttribute), true).FirstOrDefault() as SemanticVersionedRouteAttribute;
                                                    if (attribute != null)
                                                    {
                                                        r = true;
                                                    }
                                                    return r;
                                                }
                
                                            )
                                        .GroupBy
                                            (
                                                (t) =>
                                                {
                                                    return attribute.Template;
                                                }
                                            );
            return null;
            //return
            //    groups
            //        .ToDictionary
            //            (
            //                (x) =>
            //                {
            //                    return
            //                        x.Key;
            //                }
            //                ,
            //                (x) =>
            //                {
            //                    return
            //                        x.ToLookup<Type,NuGetVersion>
            //                                (
            //                                      (xx) =>
            //                                      {
            //                                          return attribute.Version;
            //                                      }
            //                                );
            //                }
            //                ,
            //                StringComparer.OrdinalIgnoreCase
            //    );
        }
    }
}


// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.



namespace System.Web.Http.Routing
{
    using System.Diagnostics;
    using System.Globalization;
    using System.Net.Http;
    using System.Web.Http.Controllers;
    // This is static description of an action and can be shared across requests. 
    // Direct routes may cache a list of these. 
    [DebuggerDisplay("{DebuggerToString()}")]
    internal class CandidateAction
    {
        public HttpActionDescriptor ActionDescriptor { get; set; }
        public int Order { get; set; }
        public decimal Precedence { get; set; }

        public bool MatchName(string actionName)
        {
            return String.Equals(ActionDescriptor.ActionName, actionName, StringComparison.OrdinalIgnoreCase);
        }

        public bool MatchVerb(HttpMethod method)
        {
            return ActionDescriptor.SupportedHttpMethods.Contains(method);
        }

        internal string DebuggerToString()
        {
            return String.Format(CultureInfo.CurrentCulture, "{0}, Order={1}, Prec={2}", ActionDescriptor.ActionName, Order, Precedence);
        }
    }
}






// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.



// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.



namespace System.Web.Http.Routing
{
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    public static class HttpRouteDataExtensions
    {
        /// <summary>
        /// Remove all optional parameters that do not have a value from the route data. 
        /// </summary>
        /// <param name="routeData">route data, to be mutated in-place.</param>
        //public static void RemoveOptionalRoutingParameters(this IHttpRouteData routeData)
        //{
        //    RemoveOptionalRoutingParameters(routeData.Values);

        //    var subRouteData = routeData.GetSubRoutes();
        //    if (subRouteData != null)
        //    {
        //        foreach (IHttpRouteData sub in subRouteData)
        //        {
        //            RemoveOptionalRoutingParameters(sub);
        //        }
        //    }
        //}

        private static void RemoveOptionalRoutingParameters(IDictionary<string, object> routeValueDictionary)
        {
            Contract.Assert(routeValueDictionary != null);

            // Get all keys for which the corresponding value is 'Optional'.
            // Having a separate array is necessary so that we don't manipulate the dictionary while enumerating.
            // This is on a hot-path and linq expressions are showing up on the profile, so do array manipulation.
            int max = routeValueDictionary.Count;
            int i = 0;
            string[] matching = new string[max];
            foreach (KeyValuePair<string, object> kv in routeValueDictionary)
            {
                if (kv.Value == RouteParameter.Optional)
                {
                    matching[i] = kv.Key;
                    i++;
                }
            }
            for (int j = 0; j < i; j++)
            {
                string key = matching[j];
                routeValueDictionary.Remove(key);
            }
        }

        /// <summary>
        /// If a route is really a union of other routes, return the set of sub routes. 
        /// </summary>
        /// <param name="routeData">a union route data</param>
        /// <returns>set of sub soutes contained within this route</returns>
        //public static IEnumerable<IHttpRouteData> GetSubRoutes(this IHttpRouteData routeData)
        //{
        //    IHttpRouteData[] subRoutes = null;
        //    if (routeData.Values.TryGetValue(RouteCollectionRoute.SubRouteDataKey, out subRoutes))
        //    {
        //        return subRoutes;
        //    }
        //    return null;
        //}

        // If routeData is from an attribute route, get the action descriptors, order and precedence that it may match
        // to. Caller still needs to run action selection to pick the specific action.
        // Else return null.
        internal static CandidateAction[] GetDirectRouteCandidates(this IHttpRouteData routeData)
        {
            Contract.Assert(routeData != null);
            IEnumerable<IHttpRouteData> subRoutes = routeData.GetSubRoutes();

            // Possible this is being called on a subroute. This can happen after ElevateRouteData. Just chain. 
            if (subRoutes == null)
            {
                if (routeData.Route == null)
                {
                    // If the matched route is a System.Web.Routing.Route (in web host) then routeData.Route
                    // will be null. Normally a System.Web.Routing.Route match would go through an MVC handler
                    // but we can get here through HttpRoutingDispatcher in WebAPI batching. If that happens, 
                    // then obviously it's not a WebAPI attribute routing match.
                    return null;
                }
                else
                {
                    return routeData
                        //.Route
                        .GetDirectRouteCandidates();
                }
            }

            var list = new List<CandidateAction>();

            foreach (IHttpRouteData subData in subRoutes)
            {
                ////CandidateAction[] candidates = subData.Route.GetDirectRouteCandidates();
                //if (candidates != null)
                //{
                //    list.AddRange(candidates);
                //}
            }
            return list.ToArray();
        }
    }
}