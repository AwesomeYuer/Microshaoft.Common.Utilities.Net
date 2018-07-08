#if !NETFRAMEWORK4_X && !NETSTANDARD2_0

namespace Microshaoft.WebApi.Controllers
{
    using Microshaoft;
    using Microsoft.AspNetCore.Mvc;
    [Route("api/[controller]")]
    [ApiController]
    public class StoreProcedureExecutorController : AbstractStoreProcedureExecutorControllerBase //ControllerBase //, IConnectionString
    {
        protected override string ConnectionString => @"Integrated Security=SSPI;Persist Security Info=False;Initial Catalog=D:\mssql\MSSQL13.LocalDB\LocalDB\TransportionSecrets\TransportionSecrets.mdf;Data Source=(localdb)\mssqllocaldb;";
    }
}
#endif
