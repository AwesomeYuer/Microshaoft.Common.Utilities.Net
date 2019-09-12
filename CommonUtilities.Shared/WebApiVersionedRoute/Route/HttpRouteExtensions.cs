#if NETFRAMEWORK4_X
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
#endif
