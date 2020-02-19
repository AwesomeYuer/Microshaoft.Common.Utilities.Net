#if NETCOREAPP
namespace Microshaoft.HttpBatchHandler.Multipart
{
    using Microsoft.AspNetCore.Http.Features;
    public class HttpApplicationResponseSection
    {
        public IHttpResponseFeature ResponseFeature { get; set; }
    }
}
#endif