#if !NETFRAMEWORK4_X && !NETSTANDARD2_0

namespace Microshaoft.WebApi.Controllers
{
    using Microshaoft;
    using Microshaoft.Web;
    using Microshaoft.WebApi.ModelBinders;
    using Microsoft.AspNetCore.Cors;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Routing;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;

    [Route("api/[controller]")]
    [ApiController]
    [EnableCors("SPE")]
    public class StoreProcedureExecutorController 
                    : AbstractStoreProcedureExecutorControllerBase
                        //ControllerBase //, IConnectionString
    {
        public override IDictionary<string, HttpMethodsFlags> GetExecuteWhiteList()
        {
            return 
                new 
                    Dictionary<string, HttpMethodsFlags>
                        (StringComparer.OrdinalIgnoreCase)
                            {
                                {
                                    "zsp_GetDatesAfter"
                                    , HttpMethodsFlags.All
                                        //HttpMethodsFlags.Get 
                                        //| HttpMethodsFlags.Post
                                }
                            };
        }
        protected override string
              ConnectionString =>
                    @"Integrated Security=SSPI;Persist Security Info=False;Initial Catalog=D:\mssql\MSSQL13.LocalDB\LocalDB\TransportionSecrets\TransportionSecrets.mdf;Data Source=(localdb)\mssqllocaldb;";

        protected override bool NeedCheckWhiteList => true;

        protected override int CachedExecutingParametersExpiredInSeconds => 10;

        protected override bool NeedAutoRefreshExecutedTimeForSlideExpire => true;

        

        //[HttpDelete]
        //[HttpGet]
        //[HttpHead]
        //[HttpOptions]
        //[HttpPatch]
        //[HttpPost]
        //[HttpPut]
        //[Route("{dataBaseType}/{storeProcedureName}/0")]
        private ActionResult<JToken> GetResultSet1RowsOnly
                    (
                        [FromRoute]
                        string dataBaseType //= "mssql"
                        ,
                        string storeProcedureName
                        ,
                            [ModelBinder(typeof(JTokenModelBinder))]
                            JToken parameters = null
                    )
        {
            var result
                    = base
                        .ProcessActionRequest
                                (
                                    dataBaseType
                                    , storeProcedureName
                                    , parameters
                                );
            return
                new ActionResult<JToken>
                        (
                            result.Value["Outputs"]["ResultSets"][0]
                        );
            //result["Outputs"]["ResultSets"][0];


        }

        #region 未完成任务
        /*
        [HttpGet]
        [Route("groupsjoin/{storeProcedureName}")]
        //测试通过 但未完成
        public ActionResult<JToken> GetByGroupsjoin
                                (
                                    string storeProcedureName
                                    ,
                                    [FromQuery]
                                    string p            = null //string.Empty
                                )
        {
            
            //var storeProcedureName = "zsp_GroupsJoin";
            JObject j = JObject.Parse(p);
            var result = base
                            .Get
                            (
                                storeProcedureName
                                , j

                            ).Value;

            var resultSets = (JArray)result["Outputs"]["ResultSets"];
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

        [HttpGet]
        [Route("Grouping/{storeProcedureName}")]
        //未完成
        public ActionResult<JToken> GetByGrouping
                               (
                                   string storeProcedureName
                                   ,
                                    [FromQuery]
                                    string p            = null //string.Empty
                               )
        {
            storeProcedureName = "zsp_Grouping";
            JObject j = JObject.Parse(p);
            var result = base
                            .Get
                            (
                                storeProcedureName
                                , j

                            ).Value;

            var resultSets = (JArray)result["Outputs"]["ResultSets"];
            var grouping = JObject.Parse
                (
                    @"
{
    ""grouping"":
        {
            ""groupName"": ""C1"",
            ""groupBy"": [ """", ""F2"", ""F3"" ],
            ""selectMany"": 
                [
                    ""FF1"",
                    ""FF2"",
                    {
                        ""grouping"":
                            {
                                ""groupName"": ""gFF3"",
                                ""groupBy"": [ ""FFF1"", ""FFF2"", ""FFF3"" ],
                                ""selectMany"":
                                    [
                                        ""FFF4"",
                                        ""FFF5"",
                                        {
                                            ""grouping"":
                                                {
                                                    ""groupName"": ""gFFF6"",
                                                    ""groupBy"": [ ""F11"", ""F12"", ""F13"" ],
                                                    ""selectMany"": [ ""F14"", ""F15"" ]
                                                }
                                        }
                                    ]
                            }
                    }
                ]
        }
}


                    "
                );


            grouping = JObject.Parse
                (
                    @"
{
    ""grouping"":
            {
                ""groupName"": ""columns"",
                ""groupBy"": [ ""oid""],
                 ""groupings"" :
                    [
                        {
                            ""groupName"": ""columns2"",
                            ""groupBy"": [ ""oid"",""user_type_id""],
                        }
                    ]
            }
   
        
}


                    "
                );



            var master = resultSets[0].AsJEnumerable();
            var length = resultSets.Count();
            var groupName = grouping["grouping"]["groupName"].Value<string>();
            var groupBy = ((JArray)grouping["grouping"]["groupBy"])
                                                            .Where
                                                                (
                                                                    (xx) =>
                                                                    {
                                                                        return
                                                                            (xx.Type != JTokenType.Object);
                                                                    }
                                                                )
                                                            .Select
                                                                (
                                                                    (xx) =>
                                                                    {
                                                                        return
                                                                            xx.Value<string>();
                                                                    }
                                                                )
                                                            .ToArray();
            IEnumerable<IGrouping<JToken, JToken>> groups = NewMethod
                            (
                                master
                                , groupName
                                , groupBy
                                , (g) =>
                                {
                                    var key = g.Key;

                                    ((JObject)key)
                                        .Add
                                            (
                                                groupName
                                                , new JArray(g)
                                            );
                                }
                            );
            foreach (var x in grouping["grouping"]["groupings"])
            {
                groupName = x["groupName"].Value<string>();
                groupBy = x["groupBy"]
                                    .Where
                                        (
                                            (xx) =>
                                            {
                                                return
                                                    (xx.Type != JTokenType.Object);
                                            }
                                        )
                                    .Select
                                        (
                                            (xx) =>
                                            {
                                                return
                                                    xx.Value<string>();
                                            }
                                        )
                                    .ToArray(); ;
                var gs = NewMethod
                                (
                                    new JArray(groups)
                                    , groupName
                                    , groupBy
                                    , (g) =>
                                    {
                                        var key = g.Key;
                                        ((JObject)key)
                                            .Add
                                                (
                                                    groupName
                                                    , new JArray(g)
                                                );
                                    }
                                );
            }





            Console.WriteLine();

            resultSets[0] = new JArray(groups);

            return result;

            //================================================
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
            return grouping;
        }

        private static IEnumerable<IGrouping<JToken, JToken>> NewMethod
                        (
                            IJEnumerable<JToken> master
                            , string groupName, string[] groupBy
                            , Action<IGrouping<JToken, JToken>> action
                        )
        {
            var groups =
                        master
                            .GroupBy
                                (
                                    (x) =>
                                    {
                                        return
                                            x;
                                    }
                                    , new JObjectComparer()
                                    {
                                        CompareJTokensPaths = groupBy
                                    }
                                );


            foreach (var group in groups)
            {
                //var r = new JArray(group);
                //var key = group.Key;
                //((JObject)key)
                //    .Add
                //        (
                //            groupName
                //            , r
                //        );
                action(group);

            }

            return groups;
        }


*/
        #endregion

    }
}
#endif
