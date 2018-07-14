#if !NETFRAMEWORK4_X && !NETSTANDARD2_0

namespace Microshaoft.WebApi.Controllers
{
    using Microshaoft;
    using Microsoft.AspNetCore.Mvc;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    [Route("api/[controller]")]
    [ApiController]
    public class StoreProcedureExecutorController : AbstractStoreProcedureExecutorControllerBase //ControllerBase //, IConnectionString
    {
        protected override string 
                ConnectionString =>
                        @"Integrated Security=SSPI;Persist Security Info=False;Initial Catalog=D:\mssql\MSSQL13.LocalDB\LocalDB\TransportionSecrets\TransportionSecrets.mdf;Data Source=(localdb)\mssqllocaldb;";
        [HttpGet]
        [Route("groupsjoin/{storeProcedureName}")]
        public ActionResult<JObject> Get1
                               (
                                   string storeProcedureName
                                   ,
                                    [FromQuery]
                                    string p            = null //string.Empty
                               )
        {
            //var storeProcedureName = "zsp_GroupsJoin";
            var result = base
                            .Get
                            (
                                storeProcedureName
                                , p

                            ).Value;

            var resultSets = (JArray) result["Outputs"]["ResultSets"];
            var groupJoinConditions = JObject.Parse
                (
                    @"
                    {}

                    "
                );
            var master = resultSets[0].AsJEnumerable();
            var length = resultSets.Count();
            for (var i = 1; i < length; i++)
            {
                var detail = resultSets[i];


                master
                    .GroupJoin//<JToken,JToken,JToken,JArray>
                        (
                            detail
                            , (x) =>
                            {
                                //master
                                return x;
                            }
                            , (x) =>
                            {
                                //Detail
                                return x;
                            }
                            , (x, y) =>
                            {
                                var r = new JArray(y);
                                ((JObject)x).Add($"Details{i}", r);
                                return
                                     r;
                            }
                            , new JObjectComparer()
                            {
                                CompareJTokensPaths = new string[]
                                 {
                                     "object_id"
                                 }
                            }
                        ).ToArray();
            }
            return result;
        }
    }
}
#endif
