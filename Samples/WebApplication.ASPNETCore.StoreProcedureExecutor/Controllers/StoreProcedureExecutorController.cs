#if !NETFRAMEWORK4_X && !NETSTANDARD2_0
namespace Microshaoft.WebApi.Controllers
{
    using Microshaoft;
    using Microshaoft.Web;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Cors;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Infrastructure;
    using Microsoft.Extensions.Configuration;
    using Newtonsoft.Json.Linq;
    using System;

    [Route("api/[controller]")]
    [ApiController]
    [EnableCors("AllowAllOrigins")]
    [Authorize]
    public class StoreProcedureExecutorController
                    : AbstractStoreProceduresExecutorControllerBase
    {
        public StoreProcedureExecutorController
                            (
                                AbstractStoreProceduresService service
                                , IConfiguration configuration
                            )
                : base(service, configuration)
        {
        }

        [BearerTokenBasedAuthorizeFilter]
        public override ActionResult<JToken> ProcessActionRequest
             (
                    //[FromRoute]
                    string routeName
                , //[ModelBinder(typeof(JTokenModelBinder))]
                    JToken parameters = null
                , //[FromRoute]
                    string resultPathSegment1 = null
                , //[FromRoute]
                    string resultPathSegment2 = null
                , //[FromRoute]
                    string resultPathSegment3 = null
                , //[FromRoute]
                    string resultPathSegment4 = null
                , //[FromRoute]
                    string resultPathSegment5 = null
                , //[FromRoute]
                    string resultPathSegment6 = null
            )
        {
            return
                ProcessActionRequest
                    (
                        routeName
                        , parameters
                    );
        }
        private ActionResult<JToken> ProcessActionRequest
                        (
                            //[FromRoute]
                            string routeName
                            ,
                                //[ModelBinder(typeof(JTokenModelBinder))]
                                JToken parameters = null
                        )
        {
            JObject jObject = null;

            if (parameters == null)
            {
                jObject = new JObject();
            }
            else
            {
                jObject = (JObject)parameters;
            }

            //var jsonObject = ((JObject)parameters);
            jObject
                    .Add
                        (
                            "UserName"
                            , HttpContext
                                .User
                                .Identity
                                .Name
                        );



            if
                (
                    HttpContext
                        .User
                        .TryGetClaimTypeJTokenValue
                            (
                                "Extension"
                                , out var claimValue
                            )
                )
            {

                var userID = claimValue["User"]["Ui"].Value<string>();
                var userDispalyName = claimValue["User"]["Un"].Value<string>();
                var deptID = claimValue["Dept"]["Di"].Value<string>();
                var deptName = claimValue["Dept"]["Dn"].Value<string>();
                var teamID = claimValue["Team"]["Ti"].Value<string>();
                var teamName = claimValue["Team"]["Tn"].Value<string>();
                var roles = claimValue["Roles"];//.Value<string>();

                jObject.Add("UserID", userID);
                jObject.Add("UserDisplayName", userDispalyName);
                jObject.Add("DeptID", deptID);
                jObject.Add("DeptName", deptName);
                jObject.Add("TeamID", teamID);
                jObject.Add("TeamName", teamName);
                jObject.Add("Roles", roles);


                //jObject
                //        .Add
                //            (
                //                "ExtensionClaims"
                //                , claimValue
                //            );
            }
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
                                , jObject
                                , (reader, fieldType, fieldName, rowIndex, columnIndex) =>
                                {
                                    JProperty field = null;
                                    if (fieldType == typeof(string))
                                    {
                                        if (fieldName.Contains("Json", StringComparison.OrdinalIgnoreCase))
                                        {
                                            //fieldName = fieldName.Replace("json", "", System.StringComparison.OrdinalIgnoreCase);
                                            var json = reader.GetString(columnIndex);
                                            field = new JProperty
                                                            (
                                                                fieldName
                                                                , JObject.Parse(json)
                                                            );
                                        }
                                    }
                                    return
                                        (
                                            field == null
                                            , field
                                        );
                                }
                                , Request.Method
                            );
            if (r.StatusCode == 200)
            {
                result =
                    r.Result
                        .GetDescendantByPathKeys
                            (
                                true
                                , "Outputs"
                                , "ResultSets"
                                , "0"
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
    }
}
#endif
