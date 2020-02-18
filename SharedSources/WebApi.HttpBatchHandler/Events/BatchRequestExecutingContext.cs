#if NETCOREAPP
namespace Microshaoft.HttpBatchHandler.Events
{
    using Microsoft.AspNetCore.Http;
    public class BatchRequestExecutingContext
    {
        /// <summary>
        ///     The individual request
        /// </summary>
        public HttpRequest Request { get; set; }

        /// <summary>
        ///     State
        /// </summary>
        public object State { get; set; }
    }
}
#endif