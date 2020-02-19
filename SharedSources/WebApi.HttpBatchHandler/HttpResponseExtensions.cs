#if NETCOREAPP
namespace Microshaoft.HttpBatchHandler
{
    using Microsoft.AspNetCore.Http;
    internal static class HttpResponseExtensions
    {
        public static bool IsSuccessStatusCode(this HttpResponse response) => response.StatusCode >= 200 && response.StatusCode <= 299;
    }
}
#endif