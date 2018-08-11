
//namespace Microshaoft.WebApi.Controllers
//{
//    using Microshaoft;
//    using Microshaoft.Web;
//    using Newtonsoft.Json.Linq;
//    using System;
//    using System.Collections.Generic;
//    using System.Web.Http;

//    [RoutePrefix("api/StoreProcedureExecutor")]
    
//    public class StoreProcedureExecutorController 
//                : AbstractStoreProcedureExecutorControllerBase //ControllerBase //, IConnectionString
//    {
//        protected override string ConnectionString => @"Integrated Security=SSPI;Persist Security Info=False;Initial Catalog=D:\mssql\MSSQL13.LocalDB\LocalDB\TransportionSecrets\TransportionSecrets.mdf;Data Source=(localdb)\mssqllocaldb;";

//        protected override bool NeedCheckWhiteList => true;

//        protected override int CachedExecutingParametersExpiredInSeconds => 10;

//        protected override bool NeedAutoRefreshExecutedTimeForSlideExpire => false;

//        protected override string DynamicLoadExecutorsPath => throw new NotImplementedException();

//        public override IDictionary<string, HttpMethodsFlags> GetExecuteWhiteList()
//        {
//            return
//                new
//                    Dictionary<string, HttpMethodsFlags>
//                        (StringComparer.OrdinalIgnoreCase)
//                            {
//                                {
//                                    "zsp_GetDatesAfter"
//                                    , HttpMethodsFlags.Get //| HttpMethodsFlags.Post
//                                }
//                            };
//        }
//        /* 
//         * ASP.NET Framework should implement Get Action/Method
//         * but ASP.NET Core needn't  
//         */

//        [HttpGet]
//        [Route("{storeProcedureName}")]
//        public IHttpActionResult Get
//                        (
//                            string storeProcedureName
//                            ,
//                                [FromUri(Name = "gf")]
//                                int? groupFrom = null
//                            ,
//                                [FromUri(Name = "gb")]
//                                string groupBy = null
//                            ,
//                                [FromUri(Name = "p")]
//                                string parameters = null //string.Empty
//                        )
//        {
//            return
//                base.Get
//                        (
//                            storeProcedureName
//                            , groupFrom
//                            , groupBy
//                            , parameters
//                        );
//        }

//        [HttpPost]
//        [Route("{storeProcedureName}")]
//        public IHttpActionResult Post
//                            (
//                                string storeProcedureName
//                                ,
//                                    [FromBody]
//                                    //[FromForm]
//                                    JObject p //= null
//                            )
//        {

            
//            return
//                base
//                    .Post
//                        (
//                            storeProcedureName
//                            , p
//                        );
//        }
//        //[Route("test")]
//        //public IEnumerable<string> Get()
//        //{
//        //    return new string[] { "value1", "value2" };
//        //}
//    }
//}
