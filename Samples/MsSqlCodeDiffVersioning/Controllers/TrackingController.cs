namespace Microshaoft.WebApi.Controllers
{
    using Microshaoft;
    using Microshaoft.Web;
    using Microshaoft.WebApi.ModelBinders;
    using Microsoft.AspNetCore.Mvc;
    using Newtonsoft.Json.Linq;
    [Route("api/[controller]")]
    [ApiController]
    public class TrackerController : ControllerBase
    {
        private SingleThreadAsyncDequeueProcessorSlim<JToken> _asyncDequeueProcessor;
        public TrackerController(SingleThreadAsyncDequeueProcessorSlim<JToken> asyncDequeueProcessor)
        {
            _asyncDequeueProcessor = asyncDequeueProcessor;
        }

        [HttpGet]
        [Route("tracking")]
        [BearerTokenBasedAuthorizeFilter(IsRequired = false)]
        public void
                    ProcessActionRequest
                        (
                            [ModelBinder(typeof(JTokenModelBinder))]
                                JToken parameters = null
                        )
        {
            var request = HttpContext.Request;
            parameters["User"] = HttpContext.User.Identity.Name;
            parameters["RemoteIP"] = HttpContext.Connection.RemoteIpAddress.ToString();
            parameters["RequestUserAgent"] = request.Headers["User-agent"].ToString();
            parameters["RequestReferer"] = request.Headers["Referer"].ToString();
            _asyncDequeueProcessor.Enqueue(parameters);
        }
    }
}