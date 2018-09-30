#if !NETFRAMEWORK4_X && !NETSTANDARD2_0
namespace Microshaoft.WebApi.Controllers
{
    using Microshaoft.Web;
    using Microshaoft.WebApi.ModelBinders;
    using Microsoft.AspNetCore.Cors;
    using Microsoft.AspNetCore.Mvc;
    using Newtonsoft.Json.Linq;
    using Microshaoft;
    using Microshaoft.Web;
    using System.Linq;

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


        [HttpDelete]
        [HttpGet]
        [HttpHead]
        [HttpOptions]
        [HttpPatch]
        [HttpPost]
        [HttpPut]
        [
            Route
                (
                    "test2/"
                        + "{storeProcedureName}/"
                )
        ]
        public ActionResult<JToken> ProcessActionRequest11
                        (
                            [FromRoute]
                                string storeProcedureName
                            , [ModelBinder(typeof(JTokenModelBinder))]
                                JToken parameters = null
                        )
        {
            JToken result = null;
            (int StatusCode, JToken Result) r =
                    _service
                        .Process
                            (
                                "mssql2"
                                , "objects"
                                , parameters
                                , (reader, fieldType, fieldName, rowIndex, columnIndex) =>
                                {
                                    JProperty field = null;
                                    if (fieldType == typeof(string))
                                    {
                                        if (fieldName.Contains("Json", System.StringComparison.OrdinalIgnoreCase))
                                        {
                                            fieldName = fieldName.Replace("json", "", System.StringComparison.OrdinalIgnoreCase);
                                            field = new JProperty
                                                            (
                                                                fieldName
                                                                , JObject.Parse(reader.GetString(columnIndex))
                                                            );
                                        }
                                    }
                                    return field;
                                }
                                , Request.Method
                            );
            if (r.StatusCode == 200)
            {
                result =
                    r.Result
                        .GetDescendantByPath
                            (
                                "Outputs"
                                , "ResultSets"
                                , "1"
                                , "Rows"
                            );
            }
            else
            {
                Response
                    .StatusCode = r.StatusCode;
            }
            return
                result;
        }
        [HttpGet]
        [
            Route
                (
                    "test/{connectionID}/"
                    + "{storeProcedureName}"
                //+ "{resultPathSegment1?}/"
                //+ "{resultPathSegment2?}/"
                //+ "{resultPathSegment3?}/"
                //+ "{resultPathSegment4?}/"
                //+ "{resultPathSegment5?}/"
                //+ "{resultPathSegment6?}"
                )
        ]
        public ActionResult<JToken> ProcessActionRequest2
                            (
                                [FromRoute]
                                string connectionID //= "mssql"
                                , [FromRoute]
                                    string storeProcedureName
                                , [ModelBinder(typeof(JTokenModelBinder))]
                                    JToken parameters = null

                            )
        {
            var result = base.ProcessActionRequest(connectionID, storeProcedureName, parameters);
            var jToken = result.Value;
            jToken = jToken.GetDescendantByPath("Outputs", "ResultSets", "1", "Rows");

            var r =
                    from c in jToken
                        //.SelectMany
                        //    (
                        //        i => i["categories"]
                        //    ).Values<string>()
                    group c by c
                    into g
                    orderby g.Count() descending
                    select g;

            //new { Category = g.Key, Count = g.Count() };

            return jToken;


        }

    }
}
#endif
