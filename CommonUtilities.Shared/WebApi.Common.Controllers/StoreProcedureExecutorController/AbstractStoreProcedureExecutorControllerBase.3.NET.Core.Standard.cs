#if !NETFRAMEWORK4_X && NETCOREAPP2_X
namespace Microshaoft.WebApi.Controllers
{
    using Microshaoft;
    using Microshaoft.Web;
    using Microshaoft.WebApi.ModelBinders;
    using Microsoft.AspNetCore.Mvc;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Data;

    [Route("api/[controller]")]
    [ApiController]
    public abstract partial class 
                AbstractStoreProceduresExecutorControllerBase
                    :
                        ControllerBase
    {
        protected readonly
                    IStoreProceduresWebApiService
                            _service;

        protected virtual 
                    (
                        bool NeedDefaultProcess
                        , JProperty Field
                    )
                        OnReadRowColumnProcessFunc
                                        (
                                            IDataReader dataReader
                                            , Type fieldType
                                            , string fieldName
                                            , int rowIndex
                                            , int columnIndex
                                        )
        {
            JProperty field = null;
            bool needDefaultProcess = true;
            if (fieldType == typeof(string))
            {
                //if (fieldName.Contains("Json", StringComparison.OrdinalIgnoreCase))
                {
                    //fieldName = fieldName.Replace("json", "", System.StringComparison.OrdinalIgnoreCase);
                    var s = dataReader.GetString(columnIndex);
                    //var ss = s.Trim();
                    if
                        (
                            //(ss.StartsWith("{") && ss.EndsWith("}"))
                            //||
                            //(ss.StartsWith("[") && ss.EndsWith("]"))
                            s.IsJson()
                        )
                    {
                        //try
                        //{
                            var jToken = JToken.Parse(s);
                            field = new JProperty
                                    (
                                        fieldName
                                        , jToken
                                    );
                            needDefaultProcess = false;
                        //}
                        //catch
                        //{
                        //}
                    }
                }
            }
            return 
                (
                    needDefaultProcess
                    , field
                );
        }
        public AbstractStoreProceduresExecutorControllerBase
                    (
                        IStoreProceduresWebApiService service
                    )
        {
            _service = service;
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
                    "{routeName}/"
                    + "{resultPathSegment1?}/"
                    + "{resultPathSegment2?}/"
                    + "{resultPathSegment3?}/"
                    + "{resultPathSegment4?}/"
                    + "{resultPathSegment5?}/"
                    + "{resultPathSegment6?}"
                )
        ]
        public virtual ActionResult<JToken> 
                            ProcessActionRequest
                                (
                                    [FromRoute]
                                        string routeName
                                    , [ModelBinder(typeof(JTokenModelBinder))]
                                        JToken parameters = null
                                    , [FromRoute]
                                        string resultJsonPathSegment1 = null
                                    , [FromRoute]
                                        string resultJsonPathSegment2 = null
                                    , [FromRoute]
                                        string resultJsonPathSegment3 = null
                                    , [FromRoute]
                                        string resultJsonPathSegment4 = null
                                    , [FromRoute]
                                        string resultJsonPathSegment5 = null
                                    , [FromRoute]
                                        string resultJsonPathSegment6 = null
                                )
        {
            JToken result = null;
            (
                int StatusCode
                , string Message
                , JToken Result
            )
            r =
                _service
                    .Process
                        (
                            routeName
                            , parameters
                            , OnReadRowColumnProcessFunc
                            , Request.Method
                            //, 102
                        );
            if (r.StatusCode != -1)
            {
                result = r.Result;
                result = result
                            .GetDescendantByPathKeys
                                (
                                    resultJsonPathSegment1
                                    , resultJsonPathSegment2
                                    , resultJsonPathSegment3
                                    , resultJsonPathSegment4
                                    , resultJsonPathSegment5
                                    , resultJsonPathSegment6
                                );
                Response.StatusCode = r.StatusCode;
            }
            else
            {
                return
                    new JsonResult
                        (
                            new
                            {
                                r.StatusCode
                                , r.Message
                            }
                        )
                    {
                        StatusCode = r.StatusCode
                    };
            }
            return result;
        }
    }
}
#endif
