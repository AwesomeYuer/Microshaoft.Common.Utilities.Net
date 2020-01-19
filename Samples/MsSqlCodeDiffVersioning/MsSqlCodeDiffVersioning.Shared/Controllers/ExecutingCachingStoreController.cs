namespace Microshaoft.WebApi.Controllers
{
    using Microshaoft;
    using Microshaoft.Web;
    using Microshaoft.WebApi.ModelBinders;
    using Microsoft.AspNetCore.Mvc;
    using Newtonsoft.Json.Linq;
    using System.Collections.Concurrent;
    using System.Collections.Generic;

    [Route("api/[controller]")]
    [ApiController]
    public class ExecutingCachingStoreController : ControllerBase
    {
        private ConcurrentDictionary<string, ExecutingInfo>
                                        _executingCachingStore;
                        
        public ExecutingCachingStoreController
                        (
                            ConcurrentDictionary<string, ExecutingInfo>
                                        executingCachingStore
                        )
        {
            _executingCachingStore = executingCachingStore;
        }

        [HttpGet]
        public ActionResult<ConcurrentDictionary<string, ExecutingInfo>>
                        ProcessActionRequest
                            (
                    
                            )
        {
            return
                _executingCachingStore;
        }

        
    }
}