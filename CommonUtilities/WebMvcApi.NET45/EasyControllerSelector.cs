
// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.



//namespace System.Web.Http.Dispatcher
namespace Microshaoft.WebApi
{
    using System;
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
    using System.Web.Http;
    using System.Web.Http.Routing;
    using System.Web.Http.Dispatcher;
    using Microshaoft.Web;
    using Microshaoft;
    using Microshaoft.Versioning;
    /// <summary>
    /// Default <see cref="IHttpControllerSelector"/> instance for choosing a <see cref="HttpControllerDescriptor"/> given a <see cref="HttpRequestMessage"/>
    /// A different implementation can be registered via the <see cref="HttpConfiguration.Services"/>.
    /// </summary>
    public class EasyHttpControllerSelector : DefaultHttpControllerSelector
    {
       
        private readonly HttpConfiguration _configuration;
        private readonly Dictionary<string, Dictionary<string, HttpActionDescriptor>> _webMvcApiVersionedRoutesCache;
        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultHttpControllerSelector1"/> class.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        public EasyHttpControllerSelector(HttpConfiguration configuration) : 
                                                                                base(configuration)
        {
            //_webMvcApiVersionedRoutesCache = WebMvcApiVersionedRoutesHelper.LoadRoutes(configuration);
                //new Lazy<string, Dictionary<string, HttpControllerDescriptor>>(InitializeControllerInfoCache);
            _configuration = configuration;
            
        }

        //[SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Caller is responsible for disposing of response instance.")]
        public override HttpControllerDescriptor SelectController(HttpRequestMessage request)
        {
            //if (request == null)
            //{
            //    throw Error.ArgumentNull("request");
            //}
            //request.


            var version = GetVersionFromMediaType(request);

            IHttpRouteData routeData = request.GetRouteData();
            var a = routeData.GetDirectRouteCandidates();
            HttpControllerDescriptor controllerDescriptor;
            if (routeData != null)
            {
                controllerDescriptor = GetVersionedRouteController(routeData, version);
                if (controllerDescriptor != null)
                {
                    return controllerDescriptor;
                }
            }

           // var routeTemplate = routeData.Route.RouteTemplate;

            return null;

            
        }
        public override IDictionary<string, HttpControllerDescriptor> GetControllerMapping()
        {
            return base.GetControllerMapping();
        }
        private static HttpControllerDescriptor GetVersionedRouteController(IHttpRouteData routeData, string versionText)
        {
            HttpControllerDescriptor r = null;
            CandidateAction[] candidates = routeData.GetDirectRouteCandidates();
            var version = NuGetVersion.Parse(versionText);
            if (candidates != null)
            {
                SemanticVersionedAttribute semanticVersionedAttribute = null;
                r = candidates
                            .Where
                                (
                                    (x) =>
                                    {
                                        var rr = false;
                                        semanticVersionedAttribute = x
                                                        .ActionDescriptor
                                                        .GetCustomAttributes<SemanticVersionedAttribute>()
                                                        .FirstOrDefault() as SemanticVersionedAttribute;
                                        if (semanticVersionedAttribute != null)
                                        {
                                            rr = semanticVersionedAttribute
                                                        .AllowedVersionRange
                                                        .Satisfies(version);
                                        }
                                        return rr;
                                    }
                                )
                                .OrderByDescending
                                    (
                                        (x) =>
                                        {
                                            return semanticVersionedAttribute
                                                        .Version;
                                        }
                                        , new VersionComparer()
                                    )
                                .First()
                                .ActionDescriptor
                                .ControllerDescriptor;
            }

            return r;
        }
        private string GetVersionFromMediaType(HttpRequestMessage request)
        {
            var acceptHeader = request.Headers.Accept;
            var r = string.Empty;
            foreach (var accepts in acceptHeader)
            {
                foreach (var parameter in accepts.Parameters)
                {
                    if (parameter.Name.Equals("version", StringComparison.InvariantCultureIgnoreCase))
                    {
                        r = parameter.Value;
                        break;
                    }
                }
            }
            return r;
            //var regularExpression = new Regex(@"application\/vnd\.yournamespace\.([a-z]+)\.v([0-9]+)\+json",
            //    RegexOptions.IgnoreCase);

            //foreach (var mime in acceptHeader)
            //{
            //    Match match = regularExpression.Match(mime.MediaType);
            //    if (match.Success == true)
            //    {
            //        return match.Groups[2].Value;
            //    }
            //}
            //return "2"; //if not mime type return the API latest version
        }

        //public virtual IDictionary<string, HttpControllerDescriptor> GetControllerMapping()
        //{
        //    return null; 

        //        //_webMvcApiVersionedRoutesCache.Value.ToDictionary(c => c.Key, c => c.Value, StringComparer.OrdinalIgnoreCase);
        //}







    }

    
}


// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.






// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.



namespace Microshaoft.WebApi
{
    using System;
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
            return string.Equals(ActionDescriptor.ActionName, actionName, StringComparison.OrdinalIgnoreCase);
        }

        public bool MatchVerb(HttpMethod method)
        {
            return ActionDescriptor.SupportedHttpMethods.Contains(method);
        }

        internal string DebuggerToString()
        {
            return string.Format(CultureInfo.CurrentCulture, "{0}, Order={1}, Prec={2}", ActionDescriptor.ActionName, Order, Precedence);
        }
    }
}






// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.



// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.



namespace Microshaoft.WebApi
{
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Web.Http;
    using System.Web.Http.Routing;
    public static class HttpRouteDataExtensions
    {
        /// <summary>
        /// Remove all optional parameters that do not have a value from the route data. 
        /// </summary>
        /// <param name="routeData">route data, to be mutated in-place.</param>
        public static void RemoveOptionalRoutingParameters(this IHttpRouteData routeData)
        {
            RemoveOptionalRoutingParameters(routeData.Values);

            var subRouteData = routeData.GetSubRoutes();
            if (subRouteData != null)
            {
                foreach (IHttpRouteData sub in subRouteData)
                {
                    RemoveOptionalRoutingParameters(sub);
                }
            }
        }

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
        public static IEnumerable<IHttpRouteData> GetSubRoutes(this IHttpRouteData routeData)
        {
            IHttpRouteData[] subRoutes = null;
            if (routeData.Values.TryGetValue(RouteCollectionRoute.SubRouteDataKey, out subRoutes))
            {
                return subRoutes;
            }
            return null;
        }

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
                    return routeData.Route.GetDirectRouteCandidates();
                }
            }

            var list = new List<CandidateAction>();

            foreach (IHttpRouteData subData in subRoutes)
            {
                CandidateAction[] candidates = subData.Route.GetDirectRouteCandidates();

                if (candidates != null)
                {
                    list.AddRange(candidates);
                }
            }
            return list.ToArray();
        }
    }
}
// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microshaoft.WebApi
{
    /// <summary>
    /// Provides keys for looking up route values and data tokens.
    /// </summary>
    internal static class RouteDataTokenKeys
    {
        // Used to provide the action descriptors to consider for attribute routing
        public const string Actions = "actions";

        // Used to indicate that a route is a controller-level attribute route.
        public const string Controller = "controller";

        // Used to allow customer-provided disambiguation between multiple matching attribute routes
        public const string Order = "order";

        // Used to allow URI constraint-based disambiguation between multiple matching attribute routes
        public const string Precedence = "precedence";
    }
}

// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.


namespace Microshaoft.WebApi
{
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Net.Http;
    using System.Web.Http.Controllers;
    using System.Web.Http.Routing;
    using Microshaoft;
    internal static class HttpRouteExtensions
    {
        // If route is a direct route, get the action descriptors, order and precedence it may map to.
        public static CandidateAction[] GetDirectRouteCandidates(this IHttpRoute route)
        {
            Contract.Assert(route != null);

            IDictionary<string, object> dataTokens = route.DataTokens;
            if (dataTokens == null)
            {
                return null;
            }

            List<CandidateAction> candidates = new List<CandidateAction>();

            HttpActionDescriptor[] directRouteActions = null;
            HttpActionDescriptor[] possibleDirectRouteActions;
            if (dataTokens.TryGetValue<HttpActionDescriptor[]>(RouteDataTokenKeys.Actions, out possibleDirectRouteActions))
            {
                if (possibleDirectRouteActions != null && possibleDirectRouteActions.Length > 0)
                {
                    directRouteActions = possibleDirectRouteActions;
                }
            }

            if (directRouteActions == null)
            {
                return null;
            }

            int order = 0;
            int possibleOrder;
            if (dataTokens.TryGetValue<int>(RouteDataTokenKeys.Order, out possibleOrder))
            {
                order = possibleOrder;
            }

            decimal precedence = 0M;
            decimal possiblePrecedence;

            if (dataTokens.TryGetValue<decimal>(RouteDataTokenKeys.Precedence, out possiblePrecedence))
            {
                precedence = possiblePrecedence;
            }

            foreach (HttpActionDescriptor actionDescriptor in directRouteActions)
            {
                candidates.Add(new CandidateAction
                {
                    ActionDescriptor = actionDescriptor,
                    Order = order,
                    Precedence = precedence
                });
            }

            return candidates.ToArray();
        }

        public static HttpActionDescriptor[] GetTargetActionDescriptors(this IHttpRoute route)
        {
            Contract.Assert(route != null);
            IDictionary<string, object> dataTokens = route.DataTokens;

            if (dataTokens == null)
            {
                return null;
            }

            HttpActionDescriptor[] actions;

            if (!dataTokens.TryGetValue<HttpActionDescriptor[]>(RouteDataTokenKeys.Actions, out actions))
            {
                return null;
            }

            return actions;
        }

        public static HttpControllerDescriptor GetTargetControllerDescriptor(this IHttpRoute route)
        {
            Contract.Assert(route != null);
            IDictionary<string, object> dataTokens = route.DataTokens;

            if (dataTokens == null)
            {
                return null;
            }

            HttpControllerDescriptor controller;

            if (!dataTokens.TryGetValue<HttpControllerDescriptor>(RouteDataTokenKeys.Controller, out controller))
            {
                return null;
            }

            return controller;
        }
    }
}



// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.


namespace Microshaoft
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics.Contracts;
    using System.Collections.Generic;
    /// <summary>
    /// Extension methods for <see cref="IDictionary{TKey,TValue}"/>.
    /// </summary>
    //[EditorBrowsable(EditorBrowsableState.Never)]
    internal static class DictionaryExtensions
    {
        /// <summary>
        /// Remove entries from dictionary that match the removeCondition.
        /// </summary>
        public static void RemoveFromDictionary<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, Func<KeyValuePair<TKey, TValue>, bool> removeCondition)
        {
            // Pass the delegate as the state to avoid a delegate and closure
            dictionary.RemoveFromDictionary((entry, innerCondition) =>
            {
                return innerCondition(entry);
            },
                removeCondition);
        }

        /// <summary>
        /// Remove entries from dictionary that match the removeCondition.
        /// </summary>
        public static void RemoveFromDictionary<TKey, TValue, TState>(this IDictionary<TKey, TValue> dictionary, Func<KeyValuePair<TKey, TValue>, TState, bool> removeCondition, TState state)
        {
            Contract.Assert(dictionary != null);
            Contract.Assert(removeCondition != null);

            // Because it is not possible to delete while enumerating, a copy of the keys must be taken. Use the size of the dictionary as an upper bound
            // to avoid creating more than one copy of the keys.
            int removeCount = 0;
            TKey[] keys = new TKey[dictionary.Count];
            foreach (var entry in dictionary)
            {
                if (removeCondition(entry, state))
                {
                    keys[removeCount] = entry.Key;
                    removeCount++;
                }
            }
            for (int i = 0; i < removeCount; i++)
            {
                dictionary.Remove(keys[i]);
            }
        }

        /// <summary>
        /// Gets the value of <typeparamref name="T"/> associated with the specified key or <c>default</c> value if
        /// either the key is not present or the value is not of type <typeparamref name="T"/>. 
        /// </summary>
        /// <typeparam name="T">The type of the value associated with the specified key.</typeparam>
        /// <param name="collection">The <see cref="IDictionary{TKey,TValue}"/> instance where <c>TValue</c> is <c>object</c>.</param>
        /// <param name="key">The key whose value to get.</param>
        /// <param name="value">When this method returns, the value associated with the specified key, if the key is found; otherwise, the default value for the type of the value parameter.</param>
        /// <returns><c>true</c> if key was found, value is non-null, and value is of type <typeparamref name="T"/>; otherwise false.</returns>
        public static bool TryGetValue<T>(this IDictionary<string, object> collection, string key, out T value)
        {
            Contract.Assert(collection != null);

            object valueObj;
            if (collection.TryGetValue(key, out valueObj))
            {
                if (valueObj is T)
                {
                    value = (T)valueObj;
                    return true;
                }
            }

            value = default(T);
            return false;
        }

        internal static IEnumerable<KeyValuePair<string, TValue>> FindKeysWithPrefix<TValue>(this IDictionary<string, TValue> dictionary, string prefix)
        {
            Contract.Assert(dictionary != null);
            Contract.Assert(prefix != null);

            TValue exactMatchValue;
            if (dictionary.TryGetValue(prefix, out exactMatchValue))
            {
                yield return new KeyValuePair<string, TValue>(prefix, exactMatchValue);
            }

            foreach (var entry in dictionary)
            {
                string key = entry.Key;

                if (key.Length <= prefix.Length)
                {
                    continue;
                }

                if (!key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                // Everything is prefixed by the empty string
                if (prefix.Length == 0)
                {
                    yield return entry;
                }
                else
                {
                    char charAfterPrefix = key[prefix.Length];
                    switch (charAfterPrefix)
                    {
                        case '[':
                        case '.':
                            yield return entry;
                            break;
                    }
                }
            }
        }
    }
}



// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.


namespace Microshaoft.WebApi
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Net.Http;
    using System.Web.Http.Properties;
    using System.Web.Http.Routing;
    using Microshaoft;
    /// <summary>
    /// A single route that is the composite of multiple "sub routes".  
    /// </summary>
    /// <remarks>
    /// Corresponds to the MVC implementation of attribute routing in System.Web.Mvc.Routing.RouteCollectionRoute.
    /// </remarks>
    internal class RouteCollectionRoute : IHttpRoute, IReadOnlyCollection<IHttpRoute>
    {
        // Key for accessing SubRoutes on a RouteData.
        // We expose this through the RouteData.Values instead of a derived class because 
        // RouteData can get wrapped in another type, but Values still gets persisted through the wrappers. 
        // Prefix with a \0 to protect against conflicts with user keys. 
        public const string SubRouteDataKey = "MS_SubRoutes";

        private IReadOnlyCollection<IHttpRoute> _subRoutes;

        private static readonly IDictionary<string, object> _empty = EmptyReadOnlyDictionary<string, object>.Value;

        public RouteCollectionRoute()
        {
        }

        // This will enumerate all controllers and action descriptors, which will run those 
        // Initialization hooks, which may try to initialize controller-specific config, which
        // may call back to the initialize hook. So guard against that reentrancy.
        private bool _beingInitialized;

        // deferred hook for initializing the sub routes. The composite route can be added during the middle of 
        // intializing, but then the actual sub routes can get populated after initialization has finished. 
        public void EnsureInitialized(Func<IReadOnlyCollection<IHttpRoute>> initializer)
        {
            if (_beingInitialized && _subRoutes == null)
            {
                // Avoid reentrant initialization
                return;
            }

            try
            {
                _beingInitialized = true;

                _subRoutes = initializer();
                Contract.Assert(_subRoutes != null);
            }
            finally
            {
                _beingInitialized = false;
            }
        }

        private IReadOnlyCollection<IHttpRoute> SubRoutes
        {
            get
            {
                // Caller should have already explicitly called EnsureInitialize. 
                // Avoid lazy initilization from within the route table because the route table
                // is shared resource and init can happen 
                if (_subRoutes == null)
                {
                    string msg = "Error.Format(SRResources.Object_NotYetInitialized)";
                    throw new InvalidOperationException(msg);
                }

                return _subRoutes;
            }
        }

        public string RouteTemplate
        {
            get { return String.Empty; }
        }

        public IDictionary<string, object> Defaults
        {
            get { return _empty; }
        }

        public IDictionary<string, object> Constraints
        {
            get { return _empty; }
        }

        public IDictionary<string, object> DataTokens
        {
            get { return null; }
        }

        public HttpMessageHandler Handler
        {
            get
            {
                return null;
            }
        }

        // Returns null if no match. 
        // Else, returns a composite route data that encapsulates the possible routes this may match against. 
        public IHttpRouteData GetRouteData(string virtualPathRoot, HttpRequestMessage request)
        {
            List<IHttpRouteData> matches = new List<IHttpRouteData>();
            foreach (IHttpRoute route in SubRoutes)
            {
                IHttpRouteData match = route.GetRouteData(virtualPathRoot, request);
                if (match != null)
                {
                    matches.Add(match);
                }
            }
            if (matches.Count == 0)
            {
                return null;  // no matches
            }

            return new RouteCollectionRouteData(this, matches.ToArray());
        }

        public IHttpVirtualPathData GetVirtualPath(HttpRequestMessage request, IDictionary<string, object> values)
        {
            // Use LinkGenerationRoute stubs to get placeholders for all the sub routes. 
            return null;
        }

        public int Count
        {
            get { return SubRoutes.Count; }
        }

        public IEnumerator<IHttpRoute> GetEnumerator()
        {
            return SubRoutes.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return SubRoutes.GetEnumerator();
        }

        // Represents a union of multiple IHttpRouteDatas. 
        private class RouteCollectionRouteData : IHttpRouteData
        {
            public RouteCollectionRouteData(IHttpRoute parent, IHttpRouteData[] subRouteDatas)
            {
                Route = parent;

                // Each sub route may have different values. Callers need to enumerate the subroutes 
                // and individually query each. 
                // Find sub-routes via the SubRouteDataKey; don't expose as a property since the RouteData 
                // can be wrapped in an outer type that doesn't propagate properties. 
                Values = new HttpRouteValueDictionary() { { SubRouteDataKey, subRouteDatas } };
            }

            public IHttpRoute Route { get; private set; }

            public IDictionary<string, object> Values { get; private set; }
        }
    }
}

// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.


namespace Microshaoft
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    internal class EmptyReadOnlyDictionary<TKey, TValue>
    {
        private static readonly ReadOnlyDictionary<TKey, TValue> _value = new ReadOnlyDictionary<TKey, TValue>(new Dictionary<TKey, TValue>());

        public static IDictionary<TKey, TValue> Value
        {
            get { return _value; }
        }
    }
}