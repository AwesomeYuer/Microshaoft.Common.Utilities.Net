//#if !NETFRAMEWORK4_X && !NETSTANDARD2_0

//namespace Microshaoft.AspNETCore.WebApi.Controllers
//{
//    using Microshaoft;
//    using Microsoft.AspNetCore.Mvc;
//    using Newtonsoft.Json.Linq;
//    using System.Data;
//    using System.Data.SqlClient;
//    using System.Linq;



//    [Route("api/[controller]")]
//    [ApiController]
//    public class StoreProcedureExecutor111Controller : ControllerBase //, IConnectionString
//    {
//        //protected abstract string ConnectionString
//        //{
//        //    get;
//        //    //set;
//        //}
//        //protected abstract void SetConnectionString(string connectionString);

//        private string _connectionString = @"Integrated Security=SSPI;Persist Security Info=False;Initial Catalog=D:\mssql\MSSQL13.LocalDB\LocalDB\TransportionSecrets\TransportionSecrets.mdf;Data Source=(localdb)\mssqllocaldb;";

//        // GET api/values
//        [HttpGet]
//        [Route("{storeProcedureName}")]
//        public ActionResult<string> Get
//                                (
//                                    string storeProcedureName
//                                    ,
//                                        [FromQuery]
//                                        string p = null //string.Empty
//                                )
//        {



//            var connection = new SqlConnection(_connectionString);
//            return "aaaa";
//            //return
//            //    SqlHelper
//            //        .StoreProcedureWebExecute
//            //            (
//            //                connection
//            //                , storeProcedureName
//            //                , p
//            //            );
//        }

//        // GET api/values/5
//        [HttpGet("{id}")]
//        public ActionResult<string> Get(int id)
//        {
//            return "value";
//        }

//        // POST api/values
//        [HttpPost]
//        public void Post([FromBody] string value)
//        {
//        }

//        // PUT api/values/5
//        [HttpPut("{id}")]
//        public void Put(int id, [FromBody] string value)
//        {
//        }

//        // DELETE api/values/5
//        [HttpDelete("{id}")]
//        public void Delete(int id)
//        {
//        }


//    }
//}
//#endif
