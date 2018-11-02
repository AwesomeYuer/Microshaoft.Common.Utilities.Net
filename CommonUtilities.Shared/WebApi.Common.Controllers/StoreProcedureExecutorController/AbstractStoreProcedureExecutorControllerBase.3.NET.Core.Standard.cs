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


        private Func
                                <
                                    IDataReader
                                    , Type        // fieldType
                                    , string    // fieldName
                                    , int       // row index
                                    , int       // column index
                                    , JProperty   //  JObject Field 对象
                                > _onReadRowColumnProcessFunc = null;
        protected virtual Func<IDataReader, Type, string, int, int, JProperty> OnReadRowColumnProcessFunc
        {
            get => _onReadRowColumnProcessFunc;
            set => _onReadRowColumnProcessFunc = value;
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
        public virtual ActionResult<JToken> ProcessActionRequest
                            (
                                [FromRoute]
                                    string routeName
                                , [ModelBinder(typeof(JTokenModelBinder))]
                                    JToken parameters = null
                                , [FromRoute]
                                    string resultPathSegment1 = null
                                , [FromRoute]
                                    string resultPathSegment2 = null
                                , [FromRoute]
                                    string resultPathSegment3 = null
                                , [FromRoute]
                                    string resultPathSegment4 = null
                                , [FromRoute]
                                    string resultPathSegment5 = null
                                , [FromRoute]
                                    string resultPathSegment6 = null
                            )
        {
            JToken result = null;
            (int StatusCode, JToken Result) r =
                _service
                    .Process
                        (
                            routeName
                            , parameters
                            , _onReadRowColumnProcessFunc
                            , Request.Method
                            //, 102
                        );
            if (r.StatusCode == 200)
            {
                result = r
                            .Result
                            .GetDescendantByPath
                                (
                                    resultPathSegment1
                                    , resultPathSegment2
                                    , resultPathSegment3
                                    , resultPathSegment4
                                    , resultPathSegment5
                                    , resultPathSegment6
                                );
                
                if 
                    (
                        (
                            HttpContext
                                .Request
                                .Method
                            ==
                            "HttpPost"
                        )
                        ||
                        (
                            HttpContext
                                .Request
                                .Method
                            ==
                            "HttpPut"
                        )
                    )
                {
                    (
                        (JObject)
                        result
                    )
                    .Property("Inputs")
                    .Remove();
                }
            }
            else
            {
                Response.StatusCode = r.StatusCode;
            }
            return result;
        }
    }
}
#endif
