#if NETCOREAPP
namespace Microshaoft.HttpBatchHandler.Events
{
    using Microsoft.AspNetCore.Http;
    public class BatchMiddlewareOptions
    {
        /// <summary>
        ///  Events
        /// </summary>
        public BatchMiddlewareEvents Events { get; set; } = new BatchMiddlewareEvents();
        /// <summary>
        /// Endpoint
        /// </summary>
        public PathString Match { get; set; } = "/api/batch";
    }
}
#endif