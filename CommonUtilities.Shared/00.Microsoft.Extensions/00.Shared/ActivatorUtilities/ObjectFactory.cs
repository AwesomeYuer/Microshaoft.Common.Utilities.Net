// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.


#if ActivatorUtilities_In_DependencyInjection
namespace Microshaoft.Extensions.DependencyInjection
#else
namespace Microshaoft.Extensions.Internal
#endif
{
    using System;
    /// <summary>
    /// The result of <see cref="ActivatorUtilities.CreateFactory(Type, Type[])"/>.
    /// </summary>
    /// <param name="serviceProvider">The <see cref="IServiceProvider"/> to get service arguments from.</param>
    /// <param name="arguments">Additional constructor arguments.</param>
    /// <returns>The instantiated type.</returns>
#if ActivatorUtilities_In_DependencyInjection
    public
#else
    public
#endif
    delegate object ObjectFactory(IServiceProvider serviceProvider, object[] arguments);
}