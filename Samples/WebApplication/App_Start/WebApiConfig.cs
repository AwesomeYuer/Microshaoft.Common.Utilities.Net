namespace WebApplication
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Web.Http;
    using Microsoft.Owin.Security.OAuth;
    using Newtonsoft.Json.Serialization;
    using Microshaoft.Web;
    using Microshaoft.WebApi;
    using System.Web.Http.Dispatcher;
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services
            // Configure Web API to use only bearer token authentication.
            //config.SuppressDefaultHostAuthentication();
            //config.Filters.Add(new HostAuthenticationFilter(OAuthDefaults.AuthenticationType));

            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );


            //return;

            config
                .Services
                .Replace
                    (
                        typeof(IHttpControllerSelector)
                        , //new EasyHttpControllerSelector(config)


                        new AcceptHeaderControllerSelector
                                (
                                    config
                                    , (accept) =>
                                    {
                                        var r = string.Empty;
                                        foreach (var parameter in accept.Parameters)
                                        {
                                            if (parameter.Name.Equals("version", StringComparison.InvariantCultureIgnoreCase))
                                            {
                                                r = parameter.Value;
                                                break;
                                            }

                                        }
                                        return r;
                                        //return "v2"; // default namespace, return null to throw 404 when namespace not given
                                    }
                                )
                    );

        }
    }
}
