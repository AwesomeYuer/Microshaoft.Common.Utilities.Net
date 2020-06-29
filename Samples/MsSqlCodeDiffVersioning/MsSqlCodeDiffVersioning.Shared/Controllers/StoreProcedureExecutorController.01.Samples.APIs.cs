#if NETCOREAPP || NETSTANDARD2_X
namespace Microshaoft.WebApi.Controllers
{
    using Microshaoft.WebApi.ModelBinders;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.ModelBinding;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using System;
#if NETCOREAPP3_X
    using SystemJsonSerializer = System.Text.Json.JsonSerializer;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authorization;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
#endif
    //[ModelBinder(BinderType = typeof(RequestParams))]
    public class RequestParams : JTokenModelBinder
    {
        public string Param1 { get; set; }
        public int Param2 { get; set; }
        public DateTime Param3 { get; set; }

        public override Task BindModelAsync(ModelBindingContext bindingContext)
        {
            var jToken = base.GetJTokenModelBindingResult(bindingContext);
            var json = jToken.ToString();
            //var result = JsonSerializer
            //                    .Deserialize
            //                            <RequestParams>
            //                                (
            //                                    json
            //                                    , new JsonSerializerOptions()
            //                                        {
            //                                            PropertyNameCaseInsensitive = true
            //                                        }
            //                                );
            var result = JsonConvert.DeserializeObject<RequestParams>(json);
            bindingContext.Result = ModelBindingResult.Success(result);
            return
                Task.CompletedTask;
        }
    }


    public partial class StoreProcedureExecutorController
                    : AbstractStoreProceduresExecutorControllerBase
    {

        /// <summary>
        /// 重写某 WebAPI 
        /// </summary>
        /// <returns></returns>
        [HttpDelete]
        [HttpGet]
        [HttpHead]
        [HttpOptions]
        [HttpPatch]
        [HttpPost]
        [HttpPut]
        // 该 Route URL Path 优先级高于 Routes.json Config
        [Route("result/mssql/aaa/bbb/objects1")]
        public ActionResult<JToken> Test
                        (
            
                        )
        {
            return
                new JsonResult(new { a = 1 });
        }


        /// <summary>
        /// JToken parameters Sample Web API
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="resultJsonPathPart1"></param>
        /// <param name="resultJsonPathPart2"></param>
        /// <returns></returns>
        [HttpDelete]
        [HttpGet]
        [HttpHead]
        [HttpOptions]
        [HttpPatch]
        [HttpPost]
        [HttpPut]
        [Route("Echo1")]
        public ActionResult<JToken> Echo
                        (
                            [ModelBinder(typeof(JTokenModelBinder))]
                                JToken parameters = null
                            , [FromRoute]
                                string resultJsonPathPart1 = null
                            , [FromRoute]
                                string resultJsonPathPart2 = null
                            //, [FromRoute]
                            //    string resultJsonPathPart3 = null
                            //, [FromRoute]
                            //    string resultJsonPathPart4 = null
                            //, [FromRoute]
                            //    string resultJsonPathPart5 = null
                            //, [FromRoute]
                            //    string resultJsonPathPart6 = null
                        )
        {
            return
                base
                    .EchoRequestInfoAsJsonResult
                            (
                                parameters
                            );
        }


        /// <summary>
        /// ModelBinder parameters Sample Web API
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="resultJsonPathPart1"></param>
        /// <param name="resultJsonPathPart2"></param>
        /// <returns></returns>      
        [HttpDelete]
        [HttpGet]
        [HttpHead]
        [HttpOptions]
        [HttpPatch]
        [HttpPost]
        [HttpPut]
        [Route("Echo2")]
        public ActionResult Echo
                        (
                            [ModelBinder(typeof(RequestParams))]
                            RequestParams
                                    parameters = null
                        )
        {
            return
                base
                    .EchoRequestInfoAsJsonResult
                            (
                                JToken
                                    .Parse
                                        (
                                            SystemJsonSerializer
                                                    .Serialize
                                                        (
                                                            parameters
                                                        )
                                        )
                            );
        }

        /// <summary>
        /// Query String parameter Sample Web API
        /// </summary>
        /// <param name="param1"></param>
        /// <returns></returns>
        [HttpDelete]
        [HttpGet]
        [HttpHead]
        [HttpOptions]
        [HttpPatch]
        [HttpPost]
        [HttpPut]
        [Route("Echo3")]
        public ActionResult Echo
                (
                    [FromQuery(Name = "q1")]
                    string param1 = null
                )
        {
            return
                base
                    .EchoRequestInfoAsJsonResult
                            ();
        }

        [HttpGet]
        [Route("admin/echo/{* }")]
        [Authorize]
        /*
        * Test Jwt Bear Token
        * eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1bmlxdWVfbmFtZSI6ImFhYWFhIiwiYWEiOiJBQUFBIiwibmJmIjoxNTgxMDU3MjMwLCJleHAiOjE1ODEwNjA4MzAsImlhdCI6MTU4MTA1NzIzMCwiaXNzIjoiSXNzdWVyMSIsImF1ZCI6IkF1ZGllbmNlMSJ9.PBXgD2ZS1pwRgD3nyOumvcjMt0_u6-Ph0xyev_I3Wyo
        */
        public override ActionResult EchoRequestInfo
                                (
                                    [ModelBinder(typeof(JTokenModelBinder))]
                                    JToken parameters = null
                                )
        {
            return
                base
                    .EchoRequestInfo(parameters);
        }

        [HttpGet]
        [Route("admin/runtime")]
        [Authorize]
        /*
        * Test Jwt Bear Token
        * eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1bmlxdWVfbmFtZSI6ImFhYWFhIiwiYWEiOiJBQUFBIiwibmJmIjoxNTgxMDU3MjMwLCJleHAiOjE1ODEwNjA4MzAsImlhdCI6MTU4MTA1NzIzMCwiaXNzIjoiSXNzdWVyMSIsImF1ZCI6IkF1ZGllbmNlMSJ9.PBXgD2ZS1pwRgD3nyOumvcjMt0_u6-Ph0xyev_I3Wyo
        */
        public override ActionResult Runtime
                                        (
                                            [FromQuery(Name = "u")]
                                            string unit = "KB"
                                        )
        {
            return
                base
                    .Runtime(unit);
        }


        [HttpGet]
        [Route("admin/ExecutingDefinitionsCachingStore")]
        [Authorize]
        /*
        * Test Jwt Bear Token
        * eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1bmlxdWVfbmFtZSI6ImFhYWFhIiwiYWEiOiJBQUFBIiwibmJmIjoxNTgxMDU3MjMwLCJleHAiOjE1ODEwNjA4MzAsImlhdCI6MTU4MTA1NzIzMCwiaXNzIjoiSXNzdWVyMSIsImF1ZCI6IkF1ZGllbmNlMSJ9.PBXgD2ZS1pwRgD3nyOumvcjMt0_u6-Ph0xyev_I3Wyo
        */
        public ConcurrentDictionary<string, ExecutingInfo> GetDbParametersDefinitionCachingStore()
        {
            return
                _service
                    .DbParametersDefinitionCachingStore;
        }
        [HttpGet]
        [Route("admin/IndexedStoreProceduresExecutors")]
        [Authorize]
        /*
        * Test Jwt Bear Token
        * eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1bmlxdWVfbmFtZSI6ImFhYWFhIiwiYWEiOiJBQUFBIiwibmJmIjoxNTgxMDU3MjMwLCJleHAiOjE1ODEwNjA4MzAsImlhdCI6MTU4MTA1NzIzMCwiaXNzIjoiSXNzdWVyMSIsImF1ZCI6IkF1ZGllbmNlMSJ9.PBXgD2ZS1pwRgD3nyOumvcjMt0_u6-Ph0xyev_I3Wyo
        */
        public IDictionary
                    <
                        string
                        , IStoreProcedureExecutable
                    > GetIndexedExecutors()
        {
            return
                _service
                    .IndexedExecutors;
        }
    }
}
#endif