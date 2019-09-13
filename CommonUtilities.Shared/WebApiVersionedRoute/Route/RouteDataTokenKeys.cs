﻿
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