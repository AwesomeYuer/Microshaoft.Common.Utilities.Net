#if NETFRAMEWORK4_X
namespace Microshaoft
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using System.IO;
    using System.Web.ModelBinding;
    using System.Web.Mvc;
    public class JTokenModelBinder : System.Web.Mvc.DefaultModelBinder
    {
        public override object BindModel
                                    (
                                        ControllerContext controllerContext
                                        , System.Web.Mvc.ModelBindingContext bindingContext
                                    )
        {
            if (!IsJSONRequest(controllerContext))
            {
                return base.BindModel(controllerContext, bindingContext);
            }
            var request = controllerContext.HttpContext.Request;
            request.InputStream.Seek(0, SeekOrigin.Begin);
            var json = new StreamReader(request.InputStream).ReadToEnd();
            var result = JToken.Parse(json);
            return result;
        }
        private static bool IsJSONRequest(ControllerContext controllerContext)
        {
            var contentType = controllerContext.HttpContext.Request.ContentType;
            return contentType.Contains("application/json");
        }
    }
}
#endif