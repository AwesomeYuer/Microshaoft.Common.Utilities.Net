#if !NETFRAMEWORK4_X && !NETSTANDARD2_0
namespace Microshaoft.WebApi.Controllers
{
    using Microshaoft.Web;
    using Microsoft.AspNetCore.Cors;
    using Microsoft.AspNetCore.Mvc;
    [Route("api/[controller]")]
    [ApiController]
    [EnableCors("AllowAllOrigins")]
    public class StoreProcedureExecutorController
                    : AbstractStoreProceduresExecutorControllerBase
    {
        /*
         * http://localhost:5816/api/StoreProcedureExecutor/mssql1/zsp_test?{datetime:"2019-01-01",pBIT:true,pBOOL:1,pTINYINT:1,pSMALLINT:16,pMEDIUMINT:25,pINT:65536,pBIGINT:999999,pFLOAT:9999.99,pDOUBLE:9999.99,pDECIMAL:9999.99,pCHAR:"a",pVARCHAR:"aaaaaaaaaa",pDate:"2018-09-01",pDateTime:"2018-09-01 21:00:10",pTimeStamp:null,pTime:null,pYear:null,udt_vcidt:[{varchar:"aaaa",date:"2018-11-11",int:789},{varchar:"bbbb",date:"2018-11-12",int:123}]}
         */
        public StoreProcedureExecutorController(IStoreProceduresWebApiService service)
                : base(service)
        {
        }
    }
}
#endif
