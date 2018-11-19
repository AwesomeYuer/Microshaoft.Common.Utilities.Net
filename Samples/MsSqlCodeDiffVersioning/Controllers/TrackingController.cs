using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microshaoft;
using Microshaoft.WebApi.ModelBinders;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace MsSqlCodeDiffVersioning.WebApplication.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TrackerController : ControllerBase
    {
        private SingleThreadAsyncDequeueProcessor<JToken> _asyncDequeueProcessor;
        public TrackerController(SingleThreadAsyncDequeueProcessor<JToken> asyncDequeueProcessor)
        {
            _asyncDequeueProcessor = asyncDequeueProcessor;
        }

        [HttpGet]
        [Route("tracking")]
        public void
                           ProcessActionRequest
                                (
                                    [ModelBinder(typeof(JTokenModelBinder))]
                                        JToken parameters = null
                                )
        {

            _asyncDequeueProcessor.Enqueue(parameters);

            //return null;
        }
    }
}