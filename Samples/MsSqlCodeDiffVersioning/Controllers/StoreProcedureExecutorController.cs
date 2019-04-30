#if !NETFRAMEWORK4_X && !NETSTANDARD2_0
namespace Microshaoft.WebApi.Controllers
{
    using Microshaoft.Web;
    using Microshaoft.WebApi.ModelBinders;
    using Microsoft.AspNetCore.Cors;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Infrastructure;
    using Newtonsoft.Json.Linq;

    [Route("api/[controller]")]
    //[ApiController]
    [EnableCors("AllowAllAny")]
    //[Authorize]
    //[ValidateModelFilter]
    [ServiceFilter(typeof(JTokenParametersValidateFilterAttribute))]
    public class StoreProcedureExecutorController
                    : AbstractStoreProceduresExecutorControllerBase
    {
        public StoreProcedureExecutorController
                            (
                                AbstractStoreProceduresService service
                                , IActionSelector actionSelector
                            )
                : base(service, actionSelector)
        {
        }

    }
}
#endif
