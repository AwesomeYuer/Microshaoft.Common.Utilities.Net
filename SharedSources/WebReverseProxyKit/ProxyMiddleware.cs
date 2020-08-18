/*

https://github.com/ProxyKit/ProxyKit/ 

A toolkit to create code-first HTTP Reverse Proxies hosted in ASP.
用于创建代码优先的HTTP反向代理的工具包。

NET Core as middleware.
NET Core作为中间件。

This allows focused code-first proxies that can be embedded in existing ASP.
这允许集中的代码优先代理，可以嵌入到现有的ASP中。

NET Core applications or deployed as a standalone server.
NET Core应用程序或部署为独立服务器。

Deployable anywhere ASP.
可部署的ASP。

NET Core is deployable such as Windows, Linux, Containers and Serverless (with caveats).
NET Core是可部署的，比如Windows、Linux、容器和无服务器(注意事项)。

Having built proxies many times before, I felt it is time to make a package.
由于之前构建了很多次代理，我觉得是时候创建一个包了。

Forked from ASP.
分叉的ASP。

NET labs, it has been heavily modified with a different API, to facilitate a wider variety of proxying scenarios (i.e. routing based on a JWT claim) and interception of the proxy requests / responses for customization of headers and (optionally) request / response bodies.
NET labs，它已经用不同的API进行了大量修改，以促进更广泛的代理场景(例如，基于JWT声明的路由)和拦截代理请求/响应，以定制报头和(可选的)请求/响应体。

It also uses HttpClientFactory internally that will mitigate against DNS caching issues making it suitable for microservice / container environments.
它还在内部使用HttpClientFactory，这将减轻DNS缓存问题，使其适合于微服务/容器环境。

A toolkit to create code-first HTTP Reverse Proxies hosted in ASP.
用于创建代码优先的HTTP反向代理的工具包。

NET Core as middleware.
NET Core作为中间件。

This allows focused code-first proxies that can be embedded in existing ASP.
这允许集中的代码优先代理，可以嵌入到现有的ASP中。

NET Core applications or deployed as a standalone server.
NET Core应用程序或部署为独立服务器。

Deployable anywhere ASP.
可部署的ASP。
*/


// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
#if NETCOREAPP
namespace Microshaoft.Web.ReverseProxyKit
{
    //using System;
    using System.Linq;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    public class ProxyMiddleware<TProxyHandler> where TProxyHandler:IProxyHandler
    {
        private readonly TProxyHandler _handler;
        private const int StreamCopyBufferSize = 81920;

        public ProxyMiddleware(RequestDelegate _, TProxyHandler handler)
        {
            _handler = handler;
        }

        public async Task Invoke(HttpContext context)
        {
            using (var response = await _handler.HandleProxyRequest(context).ConfigureAwait(false))
            {
                await CopyProxyHttpResponse(context, response).ConfigureAwait(false);
            }
        }

        private static async Task CopyProxyHttpResponse(HttpContext context, HttpResponseMessage responseMessage)
        {
            var response = context.Response;

            response.StatusCode = (int)responseMessage.StatusCode;
            foreach (var header in responseMessage.Headers)
            {
                response.Headers[header.Key] = header.Value.ToArray();
            }

            if (responseMessage.Content != null)
            {
                foreach (var header in responseMessage.Content.Headers)
                {
                    response.Headers[header.Key] = header.Value.ToArray();
                }
            }

            // SendAsync removes chunking from the response. This removes the header so it doesn't expect a chunked response.
            response.Headers.Remove("transfer-encoding");

            if (responseMessage.Content != null)
            {
                using (var responseStream = await responseMessage.Content.ReadAsStreamAsync().ConfigureAwait(false))
                {
                    await responseStream.CopyToAsync(response.Body, StreamCopyBufferSize, context.RequestAborted).ConfigureAwait(false);
                    if (responseStream.CanWrite)
                    {
                        await responseStream.FlushAsync(context.RequestAborted).ConfigureAwait(false);    
                    }
                }
            }
        }
    }
}
#endif