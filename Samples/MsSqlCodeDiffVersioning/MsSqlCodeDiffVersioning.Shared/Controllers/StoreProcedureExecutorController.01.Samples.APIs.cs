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
    using SysTxtJson = System.Text.Json;
    using System.Threading.Tasks;
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
        [Route("Echo/{* }")]
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
                                            SysTxtJson
                                                .JsonSerializer
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
                    [FromQuery(Name = "p1")]
                    string param1 = null
                )
        {
            return
                base
                    .EchoRequestInfoAsJsonResult
                            ();
        }
    }
}
#endif