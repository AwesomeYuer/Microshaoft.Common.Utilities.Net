//#if !NETFRAMEWORK4_X && !NETSTANDARD2_0

namespace Microshaoft.WebApi.Controllers
{
    //[Route("api/[controller]")]
    //[ApiController]
    public abstract partial class AbstractStoreProcedureExecutorControllerBase 
    {
        protected abstract string ConnectionString
        {
            get;
            //set;
        }
    }
}
//#endif