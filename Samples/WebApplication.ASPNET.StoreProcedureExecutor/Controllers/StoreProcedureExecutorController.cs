
namespace Microshaoft.WebApi.Controllers
{
    using Microshaoft;
    using Newtonsoft.Json.Linq;
    using System.Collections.Generic;
    using System.Web.Http;

    [RoutePrefix("api/StoreProcedureExecutor")]
    
    public class StoreProcedureExecutorController : AbstractStoreProcedureExecutorControllerBase //ControllerBase //, IConnectionString
    {
        protected override string ConnectionString => @"Integrated Security=SSPI;Persist Security Info=False;Initial Catalog=D:\mssql\MSSQL13.LocalDB\LocalDB\TransportionSecrets\TransportionSecrets.mdf;Data Source=(localdb)\mssqllocaldb;";


        /* 
         * ASP.NET Framework should implement Get Action/Method
         * but ASP.NET Core needn't  
         */

        [HttpGet]
        [Route("{storeProcedureName}")]
        public JObject Get
                        (
                            string storeProcedureName
                            ,
                            [FromUri]
                            string p = null //string.Empty
                        )
        {
            return
                base.Get
                        (
                            storeProcedureName
                            , p
                        );
        }


        [Route("test")]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }
    }
}
