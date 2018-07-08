namespace Microshaoft.WebApi.Controllers
{
    using System.Web.Http;
    using Microshaoft.WebApi.Controllers;
    [RoutePrefix("api/StoreProcedureExecutor")]
    
    public class StoreProcedureExecutorController : AbstractStoreProcedureExecutorControllerBase //ControllerBase //, IConnectionString
    {
        protected override string ConnectionString => @"Integrated Security=SSPI;Persist Security Info=False;Initial Catalog=D:\mssql\MSSQL13.LocalDB\LocalDB\TransportionSecrets\TransportionSecrets.mdf;Data Source=(localdb)\mssqllocaldb;";
    }
}
