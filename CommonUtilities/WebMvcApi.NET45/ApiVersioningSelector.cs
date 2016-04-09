namespace Microshaoft.WebApi
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Text.RegularExpressions;
    using System.Web.Http;
    using System.Web.Http.Controllers;
    using System.Web.Http.Dispatcher;

    //needed for .GetSubRoutes() extension method
    using System.Web.Http.Routing;


    public class ApiVersioningSelector : DefaultHttpControllerSelector
    {
        private HttpConfiguration _HttpConfiguration;
        public ApiVersioningSelector(HttpConfiguration httpConfiguration)
            : base(httpConfiguration)
        {
            _HttpConfiguration = httpConfiguration;
        }

        public override HttpControllerDescriptor SelectController(HttpRequestMessage request)
        {
            HttpControllerDescriptor controllerDescriptor = null;

            // get list of all controllers provided by the default selector
            IDictionary<string, HttpControllerDescriptor> controllers = GetControllerMapping();

            IHttpRouteData routeData = request.GetRouteData();

            if (routeData == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            //check if this route is actually an attribute route
            IEnumerable<IHttpRouteData> attributeSubRoutes = routeData.GetSubRoutes();

            var apiVersion = GetVersionFromMediaType(request);

            if (attributeSubRoutes == null)
            {
                string controllerName = GetRouteVariable<string>(routeData, "controller");
                if (controllerName == null)
                {
                    throw new HttpResponseException(HttpStatusCode.NotFound);
                }

                string newControllerName = String.Concat(controllerName, "V", apiVersion);

                if (controllers.TryGetValue(newControllerName, out controllerDescriptor))
                {
                    return controllerDescriptor;
                }
                else
                {
                    throw new HttpResponseException(HttpStatusCode.NotFound);
                }
            }
            else
            {
                // we want to find all controller descriptors whose controller type names end with
                // the following suffix (example: PeopleV1)
                string newControllerNameSuffix = String.Concat("V", apiVersion);

                IEnumerable<IHttpRouteData> filteredSubRoutes = attributeSubRoutes
                    .Where(attrRouteData =>
                    {
                        HttpControllerDescriptor currentDescriptor = GetControllerDescriptor(attrRouteData);

                        bool match = currentDescriptor.ControllerName.EndsWith(newControllerNameSuffix);

                        if (match && (controllerDescriptor == null))
                        {
                            controllerDescriptor = currentDescriptor;
                        }

                        return match;
                    });

                routeData.Values["MS_SubRoutes"] = filteredSubRoutes.ToArray();
            }

            return controllerDescriptor;
        }

        private HttpControllerDescriptor GetControllerDescriptor(IHttpRouteData routeData)
        {
            return ((HttpActionDescriptor[])routeData.Route.DataTokens["actions"]).First().ControllerDescriptor;
        }

        // Get a value from the route data, if present.
        private static T GetRouteVariable<T>(IHttpRouteData routeData, string name)
        {
            object result = null;
            if (routeData.Values.TryGetValue(name, out result))
            {
                return (T)result;
            }
            return default(T);
        }


        //Accept: application/vnd.yournamespace.{yourresource}.v{version}+json
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

    }
}