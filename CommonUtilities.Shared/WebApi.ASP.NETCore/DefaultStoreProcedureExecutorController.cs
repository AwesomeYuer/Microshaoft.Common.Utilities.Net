#if !NETFRAMEWORK4_X && !NETSTANDARD2_0

namespace Microshaoft.AspNETCore.WebApi.Controllers
{
    using Microshaoft;
    using Microsoft.AspNetCore.Mvc;
    using Newtonsoft.Json.Linq;
    using System.Data;
    using System.Data.SqlClient;
    using System.Linq;



    [Route("api/[controller]")]
    [ApiController]
    public class StoreProcedureExecutorController : ControllerBase //, IConnectionString
    {
        //protected abstract string ConnectionString
        //{
        //    get;
        //    //set;
        //}
        //protected abstract void SetConnectionString(string connectionString);

        private string _connectionString = @"Integrated Security=SSPI;Persist Security Info=False;Initial Catalog=D:\mssql\MSSQL13.LocalDB\LocalDB\TransportionSecrets\TransportionSecrets.mdf;Data Source=(localdb)\mssqllocaldb;";

        // GET api/values
        [HttpGet]
        [Route("{storeProcedureName}")]
        public ActionResult<JObject> Get
                                (
                                    string storeProcedureName
                                    ,
                                        [FromQuery]
                                        string p = null //string.Empty
                                )
        {



            var connection = new SqlConnection(_connectionString);
            return
                SqlHelper
                    .StoreProcedureWebExecute
                        (
                            connection
                            , storeProcedureName
                            , p
                        );
        }

       


    }
}
#endif