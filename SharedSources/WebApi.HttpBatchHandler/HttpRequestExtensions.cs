#if NETCOREAPP
namespace Microshaoft.HttpBatchHandler
{
    using System;
    using Microsoft.AspNetCore.Http;
    internal static class HttpRequestExtensions
    {
        public static bool IsMultiPartBatchRequest(this HttpRequest request) => request.ContentType?.StartsWith("multipart/", StringComparison.OrdinalIgnoreCase) ?? false;
    }
}
#endif