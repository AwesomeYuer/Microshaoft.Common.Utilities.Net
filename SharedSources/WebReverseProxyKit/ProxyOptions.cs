#if NETCOREAPP
namespace Microshaoft.Web.ReverseProxyKit
{
    using System;
    public class ProxyOptions
    {
        public TimeSpan? WebSocketKeepAliveInterval { get; set; }

        public int? WebSocketBufferSize { get; set; }
    }
}
#endif