#if NETCOREAPP

namespace Microshaoft.HttpBatchHandler
{
    using System;
    using Microshaoft.HttpBatchHandler.Events;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.DependencyInjection;
    public static class BatchMiddlewareExtensions
    {
        public static IApplicationBuilder UseBatchMiddleware(this IApplicationBuilder builder) => builder.UseBatchMiddleware(null);

        public static IApplicationBuilder UseBatchMiddleware(this IApplicationBuilder builder,
            Action<BatchMiddlewareOptions> configurationAction)
        {
            var factory = builder.ApplicationServices.GetRequiredService<IHttpContextFactory>();
            var options = new BatchMiddlewareOptions();
            configurationAction?.Invoke(options);
            return builder.UseMiddleware<BatchMiddleware>(factory, options);
        }
    }
}
#endif