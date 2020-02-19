#if NETCOREAPP
namespace Microshaoft.HttpBatchHandler.Multipart
{
    using Microsoft.AspNetCore.Http.Features;
    public class HttpApplicationRequestSection
    {
        public IHttpRequestFeature RequestFeature { get; set; }
    }
}
#endif