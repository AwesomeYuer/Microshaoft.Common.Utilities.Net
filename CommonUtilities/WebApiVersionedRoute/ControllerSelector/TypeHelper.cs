// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace Microshaoft
{
    using System;
    using System.Web.Http;
    /// <summary>
    /// A static class that provides various <see cref="Type"/> related helpers.
    /// </summary>
    public static partial class TypeHelper
    {
        //private static readonly Type TaskGenericType = typeof(Task<>);
        internal static readonly Type ApiControllerType = typeof(ApiController);
    }
}