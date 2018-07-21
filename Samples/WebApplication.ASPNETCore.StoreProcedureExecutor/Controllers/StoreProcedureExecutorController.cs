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
    public class StoreProcedureExecutorController 
                    : AbstractStoreProcedureExecutorControllerBase
                        //ControllerBase //, IConnectionString
    {
        public override HashSet<string> GetExecuteWhiteList()
        {
            return new HashSet<string>() { "zsp_GetDatesAfter" };
        }
        protected override string
              ConnectionString =>
                    @"Integrated Security=SSPI;Persist Security Info=False;Initial Catalog=D:\mssql\MSSQL13.LocalDB\LocalDB\TransportionSecrets\TransportionSecrets.mdf;Data Source=(localdb)\mssqllocaldb;";
        //protected override string ConnectionString => @"Persist Security Info=False;User ID=sa;Password=GreatWB001!~;Data Source=sops-db.eastus.cloudapp.azure.com;Initial Catalog=StandardFlowInfo";

        #region 未完成任务
        [HttpGet]
        [Route("groupsjoin/{storeProcedureName}")]
        //测试通过 但未完成
        public ActionResult<JObject> GetByGroupsjoin
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
                                , p

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
        public ActionResult<JObject> GetByGrouping
                               (
                                   string storeProcedureName
                                   ,
                                    [FromQuery]
                                    string p            = null //string.Empty
                               )
        {
            storeProcedureName = "zsp_Grouping";
            var result = base
                            .Get
                            (
                                storeProcedureName
                                , p

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



        #endregion

    }
}
#endif
