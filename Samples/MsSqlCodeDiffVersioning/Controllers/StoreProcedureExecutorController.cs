#if !NETFRAMEWORK4_X && !NETSTANDARD2_0
namespace Microshaoft.WebApi.Controllers
{
    using Microshaoft.Web;
    using Microsoft.AspNetCore.Cors;
    using Microsoft.AspNetCore.Mvc;

    [Route("api/[controller]")]
    [ApiController]
    [EnableCors("AllowAllOrigins")]
    //[Authorize]
    public class StoreProcedureExecutorController
                    : AbstractStoreProceduresExecutorControllerBase
    {
        public StoreProcedureExecutorController(IStoreProceduresWebApiService service)
                : base(service)
        {
        }
    }
}
#endif
