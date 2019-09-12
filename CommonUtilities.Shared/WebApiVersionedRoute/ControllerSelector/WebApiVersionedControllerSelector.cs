#if NETFRAMEWORK4_X

// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.



//namespace System.Web.Http.Dispatcher
namespace Microshaoft.WebApi
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Contracts;
    using System.Globalization;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Text;
    using System.Web.Http.Controllers;
    using System.Web.Http;
    using System.Web.Http.Routing;
    using System.Web.Http.Dispatcher;
    using Microshaoft.Web;
    using Microshaoft;
    using Microshaoft.Versioning;
    /// <summary>
    /// Default <see cref="IHttpControllerSelector"/> instance for choosing a <see cref="HttpControllerDescriptor"/> given a <see cref="HttpRequestMessage"/>
    /// A different implementation can be registered via the <see cref="HttpConfiguration.Services"/>.
    /// </summary>
    public class WebApiVersionedHttpControllerSelector : DefaultHttpControllerSelector
    {
       
        private readonly HttpConfiguration _configuration;
        //private readonly Dictionary<string, Dictionary<string, HttpActionDescriptor>> _webMvcApiVersionedRoutesCache;
        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultHttpControllerSelector1"/> class.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        public WebApiVersionedHttpControllerSelector(HttpConfiguration configuration) : 
                                                                                base(configuration)
        {
            //_webMvcApiVersionedRoutesCache = WebMvcApiVersionedRoutesHelper.LoadRoutes(configuration);
                //new Lazy<string, Dictionary<string, HttpControllerDescriptor>>(InitializeControllerInfoCache);
            _configuration = configuration;
            
        }

        //[SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Caller is responsible for disposing of response instance.")]
        public override HttpControllerDescriptor SelectController(HttpRequestMessage request)
        {
            //if (request == null)
            //{
            //    throw Error.ArgumentNull("request");
            //}
            //request.


            var version = GetVersionFromMediaType(request);

            IHttpRouteData routeData = request.GetRouteData();
            var a = routeData.GetDirectRouteCandidates();
            HttpControllerDescriptor controllerDescriptor;
            if (routeData != null)
            {
                controllerDescriptor = GetVersionedRouteController(routeData, version);
                if (controllerDescriptor != null)
                {
                    return controllerDescriptor;
                }
            }

           // var routeTemplate = routeData.Route.RouteTemplate;

            return null;

            
        }
        
        private static HttpControllerDescriptor GetVersionedRouteController(IHttpRouteData routeData, string versionText)
        {
            HttpControllerDescriptor httpControllerDescriptor = null;
            CandidateAction[] candidates = routeData.GetDirectRouteCandidates();
            var version = NuGetVersion.Parse(versionText);
            if (candidates != null)
            {
                //闭包
                SemanticVersionedAttribute semanticVersionedAttribute = null;
                var q = candidates
                            .Where
                                (
                                    (x) =>
                                    {
                                        var rr = false;
                                        //闭包
                                        semanticVersionedAttribute = x
                                                        .ActionDescriptor
                                                        .GetCustomAttributes<SemanticVersionedAttribute>()
                                                        .FirstOrDefault();
                                        if (semanticVersionedAttribute != null)
                                        {
                                            //rr = semanticVersionedAttribute
                                            //            .AllowedVersionRange
                                            //            .Satisfies(version);

                                            rr = true;
                                        }
                                        return rr;
                                    }
                                );
                q = q.OrderByDescending
                    (
                        (x) =>
                        {
                            return
                                     x
                                        .ActionDescriptor
                                        .GetCustomAttributes<SemanticVersionedAttribute>()
                                        .First()
                                        .Version;
                        }
                        , new VersionComparer()
                    );
                httpControllerDescriptor = q
                                            .First()
                                            .ActionDescriptor
                                            .ControllerDescriptor;
            }
            return httpControllerDescriptor;
        }
        private string GetVersionFromMediaType(HttpRequestMessage request)
        {
            var acceptHeader = request.Headers.Accept;
            var r = string.Empty;
            foreach (var accepts in acceptHeader)
            {
                foreach (var parameter in accepts.Parameters)
                {
                    if (parameter.Name.Equals("version", StringComparison.InvariantCultureIgnoreCase))
                    {
                        r = parameter.Value;
                        break;
                    }
                }
            }
            return r;
            //var regularExpression = new Regex(@"application\/vnd\.yournamespace\.([a-z]+)\.v([0-9]+)\+json",
            //    RegexOptions.IgnoreCase);

            //foreach (var mime in acceptHeader)
            //{
            //    Match match = regularExpression.Match(mime.MediaType);
            //    if (match.Success == true)
            //    {
            //        return match.Groups[2].Value;
            //    }
            //}
            //return "2"; //if not mime type return the API latest version
        }

        //public virtual IDictionary<string, HttpControllerDescriptor> GetControllerMapping()
        //{
        //    return null; 

        //        //_webMvcApiVersionedRoutesCache.Value.ToDictionary(c => c.Key, c => c.Value, StringComparer.OrdinalIgnoreCase);
        //}







    }

    
}


// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

























#endif
