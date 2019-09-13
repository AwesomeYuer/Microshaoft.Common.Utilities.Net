namespace WebApplication
{
    using Microshaoft.WebApi;
    using System.Web.Http;
    using System.Web.Http.Controllers;
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

            //config.Routes.MapHttpRoute(
            //    name: "DefaultApi",
            //    routeTemplate: "api/{controller}/{id}",
            //    defaults: new { id = RouteParameter.Optional }
            //);

            config
                .Services
                .Replace
                    (
                        typeof(IHttpControllerSelector)
                        , new WebApiVersionedHttpControllerSelector(config)
                    );
            //return;
            config
                .Services
                .Replace
                    (
                        typeof(IHttpActionSelector)
                        , new WebApiVersionedControllerActionSelector()
                    );
        }
    }
}
