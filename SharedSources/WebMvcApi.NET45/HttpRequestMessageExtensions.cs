#if !NETSTANDARD2_X
namespace Microshaoft
{
    using Newtonsoft.Json.Linq;
    using System;
    using System.Configuration;
    using System.Net.Http;
    using System.Security.Principal;
    using System.Web;
#if !NETCOREAPP3_X
    using System.Web.Http;
    using System.Net.Http.Formatting;
#endif
#if NETFRAMEWORK4_X
    using System.Web.Http.Controllers;
#endif
    using System.Linq;
    using System.Net;
    using System.Net.Http.Headers;

    public static class HttpRequestResponseMessageExtensions
    {
        private const string HttpContext = "MS_HttpContext";
        private const string RemoteEndpointMessage = "System.ServiceModel.Channels.RemoteEndpointMessageProperty";

        public static string GetClientIPAddress(this HttpRequestMessage request)
        {
            if (request.Properties.ContainsKey(HttpContext))
            {
                dynamic ctx = request.Properties[HttpContext];
                if (ctx != null)
                {
                    return ctx.Request.UserHostAddress;
                }
            }

            if (request.Properties.ContainsKey(RemoteEndpointMessage))
            {
                dynamic remoteEndpoint = request.Properties[RemoteEndpointMessage];
                if (remoteEndpoint != null)
                {
                    return remoteEndpoint.Address;
                }
            }

            return null;
        }
#if !NETCOREAPP3_X
        public static void ClearCookie(this HttpResponseMessage target, string cookieName)
        {
            var cookie = new CookieHeaderValue(cookieName, "")
            {
                Expires = DateTimeOffset.Now.AddDays(-1),
            };
            target.Headers.AddCookies(new[] { cookie });
        }
#endif
    }

}
#endif